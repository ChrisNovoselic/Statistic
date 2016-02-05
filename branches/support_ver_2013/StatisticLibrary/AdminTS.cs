using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Globalization;
//using System.Data.SqlClient;
//using MySql.Data.MySqlClient;
using System.Threading;
using System.Windows.Forms;
using HClassLibrary;

namespace StatisticCommon
{    
    public class AdminTS : HAdmin
    {
        protected Semaphore semaDBAccess;
        protected volatile Errors saveResult;
        protected volatile bool saving;

        /* Passwords
        private Semaphore semaGetPass;
        private Semaphore semaSetPass;
        private volatile Errors passResult;
        private volatile string passReceive;
        private volatile uint m_idPass;
        */

        //private Semaphore semaLoadLayout;
        //private volatile Errors loadLayoutResult;
        //private LayoutData layoutForLoading;

        public static int m_sOwner_PBR = 0;

        public double m_curRDGValues_PBR_0;

        protected DelegateFunc delegateImportForeignValuesRequuest,
                                delegateExportForeignValuesRequuest;
        protected delegate int DelegateFuncInt();
        protected DelegateFuncInt delegateImportForeignValuesResponse,
                                    delegateExportForeignValuesResponse;

        protected DataTable m_tableValuesResponse,
                    m_tableRDGExcelValuesResponse;

        protected enum StatesMachine
        {
            CurrentTime,
            AdminValues, //��������� ���������������� ������
            PPBRValues,
            AdminDates, //��������� ������ ����������� ������� ��������
            PPBRDates,
            ImpRDGExcelValues,
            ExpRDGExcelValues,
            SaveAdminValues, //���������� ���������������� ������
            SavePPBRValues, //���������� PPBR
            SaveRDGExcelValues,
            CSVValues,
            //UpdateValuesPPBR, //���������� PPBR ����� 'SaveValuesPPBR'
            //GetPass,
            //SetPassInsert,
            //SetPassUpdate,
            //LayoutGet,
            //LayoutSet,
            ClearPPBRValues,
            ClearAdminValues,
        }

        private enum StateActions
        {
            Request,
            Data,
        }

        public enum INDEX_MARK_PPBRVALUES { PBR_ENABLED, PBR_AVALIABLE, ADMIN_ENABLED, ADMIN_AVALIABLE };
        protected HMark m_markSavedValues;

        public AdminTS(bool[] arMarkSavePPBRValues)
            : base()
        {
            m_markSavedValues = new HMark(0);

            if (!(arMarkSavePPBRValues == null))
            {
                if (arMarkSavePPBRValues.Length > (int)INDEX_MARK_PPBRVALUES.PBR_ENABLED)
                    m_markSavedValues.Set((int)INDEX_MARK_PPBRVALUES.PBR_ENABLED, arMarkSavePPBRValues[(int)INDEX_MARK_PPBRVALUES.PBR_ENABLED]);
                else ;

                if (arMarkSavePPBRValues.Length > (int)INDEX_MARK_PPBRVALUES.PBR_AVALIABLE)
                    m_markSavedValues.Set((int)INDEX_MARK_PPBRVALUES.PBR_AVALIABLE, arMarkSavePPBRValues[(int)INDEX_MARK_PPBRVALUES.PBR_AVALIABLE] == true);
                else ;
            }
            else
                ;

            m_markSavedValues.Marked((int)INDEX_MARK_PPBRVALUES.ADMIN_ENABLED);
            m_markSavedValues.Marked((int)INDEX_MARK_PPBRVALUES.ADMIN_AVALIABLE);
        }

        protected override void Initialize () {
            base.Initialize ();
        }   

        public virtual Errors SaveChanges()
        {
            //Logging.Logg().Debug("AdminTS::SaveChanges () - ����...", Logging.INDEX_MESSAGE.NOT_SET);

            bool bResSemaDbAccess = false;
            
            delegateStartWait();

            //Logging.Logg().Debug("AdminTS::SaveChanges () - delegateStartWait() - �������� �������� ��� semaDBAccess=" + DbInterface.MAX_WATING, Logging.INDEX_MESSAGE.NOT_SET);

            bResSemaDbAccess = semaDBAccess.WaitOne(DbInterface.MAX_WATING);
            //if (semaDBAccess.WaitOne(6666) == true) {
            if (bResSemaDbAccess == true)
            //if (WaitHandle.WaitAll (new WaitHandle [] {semaState, semaDBAccess}, DbInterface.MAX_WATING) == true)
            //if ((semaState.WaitOne(DbInterface.MAX_WATING) == true) && (semaDBAccess.WaitOne(DbInterface.MAX_WATING) == true))
            {
                lock (m_lockState)
                {
                    ClearStates ();

                    saveResult = Errors.NoAccess;
                    saving = true;
                    using_date = false;
                    m_curDate = m_prevDate;

                    //Logging.Logg().Debug("AdminTS::SaveChanges () - states.Clear() - ...", Logging.INDEX_MESSAGE.NOT_SET);

                    AddState((int)StatesMachine.CurrentTime);

                    if (m_markSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.ADMIN_AVALIABLE) == true)
                    {
                        AddState((int)StatesMachine.AdminDates);
                        AddState((int)StatesMachine.SaveAdminValues);
                    }
                    else ;

                    if (m_markSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.PBR_AVALIABLE) == true)
                    {
                        AddState((int)StatesMachine.PPBRDates);
                        AddState((int)StatesMachine.SavePPBRValues);
                    }
                    else ;

                    Run(@"AdminTS::SaveChanges ()");
                }

                bResSemaDbAccess = semaDBAccess.WaitOne(DbInterface.MAX_WATING);
                //Logging.Logg().Debug("AdminTS::SaveChanges () - semaDBAccess.WaitOne()=" + bResSemaDbAccess.ToString(), Logging.INDEX_MESSAGE.NOT_SET);

                try
                {
                    semaDBAccess.Release(1);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"AdminTS::SaveChanges () - semaDBAccess.Release(1)", Logging.INDEX_MESSAGE.NOT_SET);
                }

                saving = false;

                if (! (saveComplete == null)) saveComplete(); else ;
            }
            else {
                Logging.Logg().Debug("AdminTS::SaveChanges () - semaDBAccess.WaitOne(" + DbInterface.MAX_WATING + @")=false", Logging.INDEX_MESSAGE.NOT_SET);

                saveResult = Errors.NoAccess;
                saving = true;
            }

            delegateStopWait();

            return saveResult;
        }

        protected virtual Errors ClearRDG()
        {
            Errors errClearResult;

            delegateStartWait();

            //Logging.Logg().Debug("AdminTS::ClearRDG () - delegateStartWait() - �������� �������� ��� semaDBAccess=" + DbInterface.MAX_WATING, Logging.INDEX_MESSAGE.NOT_SET);

            //if (semaDBAccess.WaitOne(6666) == true) {
            if (semaDBAccess.WaitOne(DbInterface.MAX_WATING) == true)
            {
                lock (m_lockState)
                {
                    ClearStates ();

                    errClearResult = Errors.NoError;
                    using_date = false;
                    m_curDate = m_prevDate;

                    //Logging.Logg().Debug("AdminTS::ClearRDG () - states.Clear() - ...", Logging.INDEX_MESSAGE.NOT_SET);

                    AddState((int)StatesMachine.CurrentTime);
                    if (m_markSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.ADMIN_AVALIABLE) == true)
                    {
                        AddState((int)StatesMachine.AdminDates);
                        AddState((int)StatesMachine.ClearAdminValues);
                    }
                    else
                        ;

                    if (m_markSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.PBR_AVALIABLE) == true)
                    {
                        AddState((int)StatesMachine.PPBRDates);
                        AddState((int)StatesMachine.ClearPPBRValues);
                    }
                    else
                        ;

                    Run(@"AdminTS::ClearRDG ()");
                }

                //�������� ��������� ������
                bool bSemaDBAccessWaitRes = semaDBAccess.WaitOne(DbInterface.MAX_WATING);
                //Logging.Logg().Debug("AdminTS::ClearRDG () - semaDBAccess.WaitOne()=" + bSemaDBAccessWaitRes.ToString(), Logging.INDEX_MESSAGE.NOT_SET);

                try
                {
                    semaDBAccess.Release(1);
                }
                catch
                {
                }
            }
            else {
                Logging.Logg().Debug("AdminTS::ClearRDG () - semaDBAccess.WaitOne()=false", Logging.INDEX_MESSAGE.NOT_SET);

                errClearResult = Errors.NoAccess;
            }

            delegateStopWait();

            return errClearResult;
        }

        public List <int>GetListIndexTECComponent (FormChangeMode.MODE_TECCOMPONENT mode) {
            List <int>listIndex = new List <int> ();

            int indx = -1;

            switch (mode) {
                case FormChangeMode.MODE_TECCOMPONENT.TEC:
                    foreach (TECComponent comp in allTECComponents)
                    {
                        indx = comp.tec.m_id;
                        if (listIndex.IndexOf(indx) < 0)
                            listIndex.Add(indx);
                        else
                            ;
                    }
                    break;
                case FormChangeMode.MODE_TECCOMPONENT.GTP:
                case FormChangeMode.MODE_TECCOMPONENT.PC:
                case FormChangeMode.MODE_TECCOMPONENT.TG:
                    foreach (TECComponent comp in allTECComponents) {
                        indx ++;
                        if (mode == modeTECComponent (indx))
                        {
                            listIndex.Add (indx);
                        }
                        else
                            ;
                    }
                    break;
                default:
                    break;
            }

            return listIndex;
        }

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate (active);

            if ((active == true)
                && (bRes == true)
                && (IsFirstActivated == true)) //������ ��� 1-�� ���������
                GetRDGValues (indxTECComponents);
            else
                ;

            return bRes;
        }

        public virtual bool IsRDGExcel (int indx) {
            bool bRes = false;
            if (allTECComponents[indx].tec.m_path_rdg_excel.Length > 0)
                bRes = true;
            else
                ;

            return bRes;
        }

        public void Reinit()
        {
            if (! (Actived == true))
                return;
            else
                ;

            /*InitDbInterfaces ();*/

            lock (m_lockState)
            {
                ClearStates();

                //m_curDate = mcldrDate.SelectionStart;
                m_curDate = m_prevDate;
                saving = false;

                using_date = true; //???

                AddState((int)StatesMachine.CurrentTime);

                Run(@"AdminTS::Reinit ()");
            }
        }

        public void GetCurrentTime(int indx)
        {
            lock (m_lockState)
            {
                ClearStates();

                indxTECComponents = indx;

                //Logging.Logg().Debug("AdminTS::GetCurrentTime () - states.Clear() - ...", Logging.INDEX_MESSAGE.NOT_SET);

                AddState((int)StatesMachine.CurrentTime);

                Run(@"AdminTS::GetCurrentTime ()");
            }
        }

        /// <summary>
        /// ��������� ������ ���-��������
        /// </summary>
        private void protectSavedPPBRValues()
        {
            if (m_markSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.PBR_ENABLED) == true) m_markSavedValues.UnMarked((int)INDEX_MARK_PPBRVALUES.PBR_AVALIABLE); else ;
        }
        
        public virtual void GetRDGValues (int indx) {
            //��������� ������ ���-��������
            protectSavedPPBRValues();

            lock (m_lockState)
            {
                ClearStates();

                indxTECComponents = indx;
                
                ClearValues();

                using_date = true;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                AddState((int)StatesMachine.CurrentTime);
                if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.PBR) == true)
                    AddState((int)StatesMachine.PPBRValues);
                else ;
                if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true)
                    AddState((int)StatesMachine.AdminValues);
                else ;

                Run(@"AdminTS::GetRDGValues ()");
            }
        }

        public override void GetRDGValues(int indx, DateTime date)
        {
            //��������� ������ ���-��������
            protectSavedPPBRValues();

            lock (m_lockState)
            {
                ClearStates();

                indxTECComponents = indx;
                
                ClearValues();

                using_date = false;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_prevDate = date.Date;
                m_curDate = m_prevDate;

                //???����� ����������...
                if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.PBR) == true)
                    AddState((int)StatesMachine.PPBRValues);
                else ;
                if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true)
                    AddState((int)StatesMachine.AdminValues);
                else ;

                Run(@"AdminTS::GetRDGValues ()");
            }
        }

        protected override void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date)
        {
            Request(m_dictIdListeners [t.m_id][(int)CONN_SETT_TYPE.PBR], t.GetPBRValueQuery(comp, date));
        }

        private void GetAdminValuesRequest(TEC t, TECComponent comp, DateTime date) {
            Request(m_dictIdListeners[t.m_id][(int)CONN_SETT_TYPE.ADMIN], t.GetAdminValueQuery(comp, date));
        }

        public virtual void ImpRDGExcelValues(int indx, DateTime date)
        {
            lock (m_lockState)
            {
                ClearStates();

                indxTECComponents = indx;
                
                ClearValues();

                using_date = false;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_prevDate = date.Date;
                m_curDate = m_prevDate;

                AddState((int)StatesMachine.ImpRDGExcelValues);

                Run(@"AdminTS::ImpRDGExcelValues ()");
            }
        }

        public virtual Errors ExpRDGExcelValues(int indx, DateTime date)
        {
            delegateStartWait();
            //Logging.Logg().Debug("AdminTS::ExpRDGExcelValues () - delegateStartWait() - �������� �������� ��� semaDBAccess=" + DbInterface.MAX_WATING, Logging.INDEX_MESSAGE.NOT_SET);

            bool bSemaDBAccessWaitRes = semaDBAccess.WaitOne(DbInterface.MAX_WATING);
            //if (semaDBAccess.WaitOne(6666) == true) {
            if (bSemaDBAccessWaitRes == true)
            {
                lock (m_lockState)
                {
                    ClearStates();

                    indxTECComponents = indx;

                    saveResult = Errors.NoAccess;
                    saving = true;
                    using_date = false;
                    m_curDate = m_prevDate;

                    AddState((int)StatesMachine.ExpRDGExcelValues);

                    Run(@"AdminTS::ExpRDGExcelValues ()");
                }

                bSemaDBAccessWaitRes = semaDBAccess.WaitOne(DbInterface.MAX_WATING);
                //Logging.Logg().Debug("AdminTS::ExpRDGExcelValues () - semaDBAccess.WaitOne()=" + bSemaDBAccessWaitRes.ToString(), Logging.INDEX_MESSAGE.NOT_SET);
                try
                {
                    semaDBAccess.Release(1);
                }
                catch
                {
                }

                saving = false;
            }
            else {
                lock (m_lockState)
                {
                    Logging.Logg().Debug("AdminTS::ExpRDGExcelValues () - semaDBAccess.WaitOne(" + DbInterface.MAX_WATING + @")=" + bSemaDBAccessWaitRes.ToString (), Logging.INDEX_MESSAGE.NOT_SET);
                    
                    saveResult = Errors.NoAccess;
                    saving = true;
                }
            }

            delegateStopWait();

            return saveResult;
        }

        protected override int GetPPBRValuesResponse(DataTable table, DateTime date)
        {
            int iRes = 0;

            m_tableValuesResponse = table.Copy ();

            return iRes;
        }

        protected virtual int GetAdminValuesResponse(DataTable tableAdminValuesResponse, DateTime date)
        {
            DataTable table = null;
            int i = -1, j = -1, k = -1,
                hour = -1, day = -1;
            //������ �������� ������, 1-�� ��-� ������� - ������ ������� � ������� ���-� �����
            //1-�� ��-� ������� - ������ ������� � ������� ���-� �����
            //���� ���-�� ����� �����, �� 1-�� ��-� ������ ����, 2-�� �����_���
            int[] arIndexTables = { 0, 1 },
                arFieldsCount = { -1, -1 };
            bool bSeason = false;

            if (tableAdminValuesResponse == null)
                tableAdminValuesResponse = new DataTable();
            else ;

            if (m_tableValuesResponse == null)
                m_tableValuesResponse = new DataTable();
            else ;

            DataTable[] arTable = { m_tableValuesResponse, tableAdminValuesResponse };

            //int offsetPBR_NUMBER = m_tableValuesResponse.Columns.IndexOf ("PBR_NUMBER");
            //if (offsetPBR_NUMBER > 0) offsetPBR_NUMBER = 0; else ;

            int offsetPBR = -1
                , offsetPBRNumber = -1
                , offsetDATE_ADMIN = -1;
            if (!(m_tableValuesResponse == null))
                offsetPBR = m_tableValuesResponse.Columns.IndexOf("PBR");
            else
                ;

            if (offsetPBR > 0) offsetPBR = 0; else ;

            //���������� ������� ���� �������� ������� (�������, �� ��� �������� � �����) - ����� 'TecView'
            if (HAdmin.SeasonDateTime.Date.CompareTo(date.Date) == 0)
                bSeason = true;
            else
                ;

            //�������� �������� 'ID_COMPONENT'
            for (i = 0; i < arTable.Length; i++)
                if (!(arTable[i].Columns.IndexOf("ID_COMPONENT") < 0))
                    try { arTable[i].Columns.Remove("ID_COMPONENT"); }
                    catch (Exception e)
                    { //ArgumentException
                        Logging.Logg().Exception(e, "Remove(\"ID_COMPONENT\")", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                else
                    ;

            if (arTable[0].Rows.Count < arTable[1].Rows.Count)
            {
                arIndexTables[0] = 1;
                arIndexTables[1] = 0;
            }
            else
            {
            }

            for (i = 0; i < arTable.Length; i++)
            {
                arFieldsCount[i] = arTable[i].Columns.Count;
            }

            table = arTable[arIndexTables[0]].Copy();
            table.Merge(arTable[arIndexTables[1]].Clone(), false);

            Type typeCol;
            try
            {
                for (i = 0; i < arTable[arIndexTables[0]].Rows.Count; i++)
                {
                    for (j = 0; j < arTable[arIndexTables[1]].Rows.Count; j++)
                    {
                        //���������� ����/����� 0 = [DATE_PBR], [DATE_ADMIN]
                        if (arTable[arIndexTables[0]].Rows[i][0].Equals(arTable[arIndexTables[1]].Rows[j][0]))
                        {
                            for (k = 0; k < arTable[arIndexTables[1]].Columns.Count; k++)
                            {
                                table.Rows[i][arTable[arIndexTables[1]].Columns[k].ColumnName] = arTable[arIndexTables[1]].Rows[j][k];
                            }

                            break;
                        }
                        else
                            ;
                    }

                    if (!(j < arTable[arIndexTables[1]].Rows.Count))
                    {//�� ���� ������� ������������ �� ���� ���� � �����./������
                        for (k = 0; k < arTable[arIndexTables[1]].Columns.Count; k++)
                        {
                            if (k == 0) //Columns[k].ColumnName == DATE
                                table.Rows[i][arTable[arIndexTables[1]].Columns[k].ColumnName] = table.Rows[i][k];
                            else
                            {
                                typeCol = arTable[arIndexTables[1]].Columns[k].DataType;
                                if (typeCol.IsPrimitive == true)
                                    table.Rows[i][arTable[arIndexTables[1]].Columns[k].ColumnName] = 0;
                                else
                                    if (typeCol.Equals(typeof(DateTime)) == true)
                                        table.Rows[i][arTable[arIndexTables[1]].Columns[k].ColumnName] = DateTime.MinValue;
                                    else
                                        if (typeCol.Equals(typeof(string)) == true)
                                            table.Rows[i][arTable[arIndexTables[1]].Columns[k].ColumnName] = string.Empty;
                                        else
                                            throw new Exception(@"AdminTS::GetAdminValuesResponse () - ����������� ��� ���� ...");
                            }
                        }
                    }
                    else
                        ;
                }
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"AdminTS::GetAdminValuesResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            offsetPBRNumber = m_tableValuesResponse.Columns.IndexOf("PBR_NUMBER");
            offsetDATE_ADMIN = table.Columns.IndexOf("DATE_ADMIN");

            //��� ������ ���������� �����
            //int prev_hour = -1;
            int offset = 0;
            //0 - DATE_ADMIN, 1 - REC, 2 - IS_PER, 3 - DIVIAT, 4 - DATE_PBR, 5 - PBR, 6 - PBR_NUMBER
            for (i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i][0] is System.DBNull) //"DATE_PBR" ???
                {
                    try
                    {
                        hour = ((DateTime)table.Rows[i]["DATE_PBR"]).Hour;
                        day = ((DateTime)table.Rows[i]["DATE_PBR"]).Day;

                        if ((hour == 0) && (day != date.Day))
                            hour = 24;
                        else
                            if (hour == 0)
                            {
                                m_curRDGValues_PBR_0 = (double)table.Rows[i][arIndexTables[1] * arFieldsCount[1] + (0 + 1)];

                                continue;
                            }
                            else
                                ;

                        //GetSeasonHours (ref prev_hour, ref hour);
                        offset = GetSeasonHourOffset(hour);
                        hour += offset;

                        //for (j = 0; j < 3 /*4 ��� SN???*/; j ++)
                        //{
                        j = 0;
                        if (!(table.Rows[i][arIndexTables[1] * arFieldsCount[1] + (j + 1) /*+ offsetPBR_NUMBER*/ /*+ offsetPBR*/ /*"PBR"*/] is DBNull))
                            m_curRDGValues[hour - 1].pbr = (double)table.Rows[i][arIndexTables[1] * arFieldsCount[1] + (j + 1) /*+ offsetPBR_NUMBER*/ /*+ offsetPBR*/ /*"PBR"*/];
                        else
                            m_curRDGValues[hour - 1].pbr = 0;
                        //}

                        j = 1;
                        if (!(table.Rows[i][arIndexTables[1] * arFieldsCount[1] + (j + 1) /*+ offsetPBR_NUMBER*/ /*+ offsetPBR*/ /*"PBR"*/] is DBNull))
                            m_curRDGValues[hour - 1].pmin = (double)table.Rows[i][arIndexTables[1] * arFieldsCount[1] + (j + 1) /*+ offsetPBR_NUMBER*/ /*+ offsetPBR*/ /*"PBR"*/];
                        else
                            m_curRDGValues[hour - 1].pmin = 0;

                        j = 2;
                        if (!(table.Rows[i][arIndexTables[1] * arFieldsCount[1] + (j + 1) /*+ offsetPBR_NUMBER*/ /*+ offsetPBR*/ /*"PBR"*/] is DBNull))
                            m_curRDGValues[hour - 1].pmax = (double)table.Rows[i][arIndexTables[1] * arFieldsCount[1] + (j + 1) /*+ offsetPBR_NUMBER*/ /*+ offsetPBR*/ /*"PBR"*/];
                        else
                            m_curRDGValues[hour - 1].pmax = 0;

                        m_curRDGValues[hour - 1].fc = false;

                        m_curRDGValues[hour - 1].recomendation = 0;
                        m_curRDGValues[hour - 1].deviationPercent = false;
                        m_curRDGValues[hour - 1].deviation = 0;
                    }
                    catch { }
                }
                else
                {
                    DateTime iDate;
                    try
                    {
                        if (!(offsetDATE_ADMIN < 0))
                        {
                            iDate = ((DateTime)table.Rows[i]["DATE_ADMIN"]);
                        }
                        else
                        {
                            iDate = ((DateTime)table.Rows[i]["DATE_PBR"]);
                        }

                        hour = iDate.Hour;
                        day = iDate.Day;

                        if ((hour == 0) && (!(day == date.Day)))
                            hour = 24;
                        else
                            if (hour == 0)
                            {
                                if ((arIndexTables[0] * arFieldsCount[1] + 1) < table.Columns.Count)
                                    m_curRDGValues_PBR_0 = (double)table.Rows[i][arIndexTables[0] * arFieldsCount[1] + 1];
                                else
                                    m_curRDGValues_PBR_0 = 0F;

                                continue;
                            }
                            else
                                ;

                        if (bSeason == true)
                        {
                            if (hour == HAdmin.SeasonDateTime.Hour)
                            {
                                DataRow[] arSeasonRows;
                                //����� � 'TecView'
                                //arSeasonRows = tableAdminValuesResponse.Select(@"DATE_ADMIN='" + iDate.ToString(@"yyyyMMdd HH:00") + @"' AND SEASON=" + (SEASON_BASE + (int)HAdmin.seasonJumpE.SummerToWinter));
                                arSeasonRows = tableAdminValuesResponse.Select(@"DATE_ADMIN='" + iDate.ToString(@"yyyy-MM-dd HH:00") + @"'");
                                if (arSeasonRows.Length > 0)
                                {
                                    int h = -1;
                                    foreach (DataRow r in arSeasonRows)
                                    {
                                        h = iDate.Hour;
                                        GetSeasonHourIndex(Int32.Parse(r[@"SEASON"].ToString()), ref h);

                                        m_curRDGValues[h - 1].recomendation = (byte)r[@"FC"];

                                        m_curRDGValues[h - 1].recomendation = (double)r[@"REC"];
                                        m_curRDGValues[h - 1].deviationPercent = (int)r[@"IS_PER"] == 1;
                                        m_curRDGValues[h - 1].deviation = (double)r[@"DIVIAT"];
                                    }
                                }
                                else
                                {//������ ... ???
                                    Logging.Logg().Error(@"AdminTS::GetAdminValueResponse () - ... ��� �� ����� ������ ��� [HAdmin.SeasonDateTime.Hour] = " + hour, Logging.INDEX_MESSAGE.NOT_SET);
                                }

                                //continue; ???
                                //���������� �������� ���� ???
                            }
                            else
                            {
                                //GetSeasonHours (ref prev_hour, ref hour);
                                offset = GetSeasonHourOffset(hour);
                                hour += offset;
                            }
                        }
                        else
                        {
                        }

                        if ((bSeason == false) ||
                            ((!(hour == HAdmin.SeasonDateTime.Hour)) && (bSeason == true)))
                            if (!(offsetDATE_ADMIN < 0))
                            {
                                m_curRDGValues[hour - 1].fc = (byte)table.Rows[i][arIndexTables[1] * arFieldsCount[0] + 5] == 1;

                                m_curRDGValues[hour - 1].recomendation = (double)table.Rows[i][arIndexTables[1] * arFieldsCount[0] + 1 /*+ offsetPBR_NUMBER*/ /*+ offsetPBR*/ /*"REC"*/];
                                m_curRDGValues[hour - 1].deviationPercent = (int)table.Rows[i][arIndexTables[1] * arFieldsCount[0] + 2 /*+ offsetPBR_NUMBER*/ /*+ offsetPBR*/ /*"IS_PER"*/] == 1;
                                m_curRDGValues[hour - 1].deviation = (double)table.Rows[i][arIndexTables[1] * arFieldsCount[0] + 3 /*+ offsetPBR_NUMBER*/ /*+ offsetPBR*/ /*"DIVIAT"*/];
                            }
                            else
                            {
                                m_curRDGValues[hour - 1].fc = false;

                                m_curRDGValues[hour - 1].recomendation = 0.0;
                                m_curRDGValues[hour - 1].deviationPercent = false;
                                m_curRDGValues[hour - 1].deviation = 0F;
                            }
                        else
                            ;

                        if ((offsetPBR == 0) && (!(table.Rows[i]["DATE_PBR"] is System.DBNull)))
                        {
                            //for (j = 0; j < 3 /*4 ��� SN???*/; j ++)
                            //{
                            j = 0;
                            if (!(table.Rows[i][arIndexTables[0] * arFieldsCount[1] + (j + 1)/* + offsetPBR_NUMBER*//*"PBR"*/] is DBNull))
                                m_curRDGValues[hour - 1].pbr = (double)table.Rows[i][arIndexTables[0] * arFieldsCount[1] + (j + 1)/* + offsetPBR_NUMBER*/ /*"PBR"*/];
                            else
                                m_curRDGValues[hour - 1].pbr = 0.0;
                            //}

                            j = 1;
                            if (!(table.Rows[i][arIndexTables[0] * arFieldsCount[1] + (j + 1)/* + offsetPBR_NUMBER*//*"PBR"*/] is DBNull))
                                m_curRDGValues[hour - 1].pmin = (double)table.Rows[i][arIndexTables[0] * arFieldsCount[1] + (j + 1)/* + offsetPBR_NUMBER*/ /*"PBR"*/];
                            else
                                m_curRDGValues[hour - 1].pmin = 0.0;

                            j = 2;
                            if (!(table.Rows[i][arIndexTables[0] * arFieldsCount[1] + (j + 1)/* + offsetPBR_NUMBER*//*"PBR"*/] is DBNull))
                                m_curRDGValues[hour - 1].pmax = (double)table.Rows[i][arIndexTables[0] * arFieldsCount[1] + (j + 1)/* + offsetPBR_NUMBER*/ /*"PBR"*/];
                            else
                                m_curRDGValues[hour - 1].pmax = 0.0;
                        }
                        else
                        {
                            m_curRDGValues[hour - 1].pbr =
                            m_curRDGValues[hour - 1].pmin = m_curRDGValues[hour - 1].pmax =
                            0.0;
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, @"AdminRS::GetAdminValuesResponse () - ... hour = " + hour, Logging.INDEX_MESSAGE.NOT_SET);
                    }
                }

                if (hour > 0)
                    if (!(offsetPBRNumber < 0))
                        m_curRDGValues[hour - 1].pbr_number = table.Rows[i]["PBR_NUMBER"].ToString();
                    else
                        m_curRDGValues[hour - 1].pbr_number = getNamePBRNumber(hour - 1);
                else
                    ;

                m_curRDGValues[hour - 1].dtRecUpdate = (DateTime)table.Rows[i]["WR_DATE_TIME"];

                //����� ������ �� ������� ... + ����� � 'TecView'
                if (bSeason == true)
                {
                    if ((hour + 0) == (HAdmin.SeasonDateTime.Hour - 0))
                    {
                        m_curRDGValues[hour + 0].pbr = m_curRDGValues[hour - 1].pbr;
                        m_curRDGValues[hour + 0].pmin = m_curRDGValues[hour - 1].pmin;
                        m_curRDGValues[hour + 0].pmax = m_curRDGValues[hour - 1].pmax;

                        m_curRDGValues[hour + 0].pbr_number = m_curRDGValues[hour - 1].pbr_number;
                        m_curRDGValues[hour + 0].dtRecUpdate = m_curRDGValues[hour - 1].dtRecUpdate;
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }

            return 0;
        }

        protected void setRDGExcelValuesItem(out RDGStruct item, int iRows)
        {
            int j = -1;
            item = new RDGStruct();
            double val = -1F;

            for (j = 0; j < allTECComponents[indxTECComponents].m_listTG.Count; j++)
                if (allTECComponents[indxTECComponents].m_listTG[j].m_indx_col_rdg_excel > 1)
                    if (! (m_tableRDGExcelValuesResponse.Rows[iRows][allTECComponents[indxTECComponents].m_listTG[j].m_indx_col_rdg_excel - 1] is DBNull) &&
                        (double.TryParse (m_tableRDGExcelValuesResponse.Rows[iRows][allTECComponents[indxTECComponents].m_listTG[j].m_indx_col_rdg_excel - 1].ToString (), out val) == true))
                        item.pbr += val;
                    else
                        ;
                else
                    return ;

            item.recomendation = 0;

            if (item.pbr > 0)
            {
                item.deviationPercent = true;
                item.deviation = 0.2;
            }
            else
            {
                item.deviationPercent = false;
                item.deviation = 0.0;
            }
        }

        protected void GetCurrentTimeRequest()
        {
            if (IsCanUseTECComponents())
            {
                TEC tec = allTECComponents[indxTECComponents].tec;
                int indx = -1;
                if (m_markQueries.IsMarked ((int)CONN_SETT_TYPE.ADMIN) == true)
                    indx = (int)CONN_SETT_TYPE.ADMIN;
                else if (m_markQueries.IsMarked ((int)CONN_SETT_TYPE.PBR) == true)
                        indx = (int)CONN_SETT_TYPE.PBR;
                    else
                        ;

                if (! (indx < 0))
                    GetCurrentTimeRequest(DbTSQLInterface.getTypeDB(tec.connSetts[indx].port), m_dictIdListeners[tec.m_id][indx]);
                else
                    ;
            }
            else
                ;
        }

        protected virtual void GetAdminDatesRequest(DateTime date)
        {
            if (m_curDate.Date > date.Date)
            {
                date = m_curDate.Date;
            }
            else
                ;

            if (IsCanUseTECComponents ())
                //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, allTECComponents[indxTECComponents].tec.GetAdminDatesQuery(date));
                Request(m_dictIdListeners[allTECComponents[indxTECComponents].tec.m_id][(int)CONN_SETT_TYPE.ADMIN], allTECComponents[indxTECComponents].tec.GetAdminDatesQuery(date, allTECComponents[indxTECComponents]));
            else
                ;
        }

        protected override void GetPPBRDatesRequest(DateTime date)
        {
            if (m_curDate.Date > date.Date)
            {
                date = m_curDate.Date;
            }
            else
                ;

            m_iHavePBR_Number = -1;

            if (IsCanUseTECComponents () == true)
                //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, allTECComponents[indxTECComponents].tec.GetPBRDatesQuery(date));
                Request(m_dictIdListeners[allTECComponents[indxTECComponents].tec.m_id][(int)CONN_SETT_TYPE.PBR],
                        allTECComponents[indxTECComponents].tec.GetPBRDatesQuery(date, allTECComponents[indxTECComponents]));
            else
                ;
        }

        private void ClearAdminDates()
        {
            ClearDates(CONN_SETT_TYPE.ADMIN);
        }

        protected virtual int GetDatesResponse(CONN_SETT_TYPE type, DataTable table, DateTime date)
        {
            int addingVal = -1;
            string pbr_number = string.Empty;

            if (type == CONN_SETT_TYPE.ADMIN)
                addingVal = (int)HAdmin.seasonJumpE.None;
            else
                if (type == CONN_SETT_TYPE.PBR)
                    addingVal = -1;
                else
                    ;

            //0 - [DATE_TIME]([DATE]), 1 - [ID]
            for (int i = 0, hour; i < table.Rows.Count; i++)
            {
                try
                {
                    hour = ((DateTime)table.Rows[i][0]).Hour;
                    if ((hour == 0) && (!(((DateTime)table.Rows[i][0]).Day == date.Day)))
                        hour = 24;
                    else
                        ;

                    //if (!(table.Columns.IndexOf(@"SEASON") < 0))
                    if (type == CONN_SETT_TYPE.ADMIN) {
                        addingVal = Int32.Parse(table.Rows[i][@"SEASON"].ToString ());
                        GetSeasonHourIndex (addingVal, ref hour);
                    }
                    else
                        ;

                    //if (!(table.Columns.IndexOf(@"PBR_NUMBER") < 0))
                    if (type == CONN_SETT_TYPE.PBR)
                    {
                        pbr_number = table.Rows[i][@"PBR_NUMBER"].ToString();
                        if (pbr_number.Length > @"���".Length)
                        {
                            pbr_number = pbr_number.Substring(@"���".Length);

                            if (Int32.TryParse(pbr_number, out addingVal) == true)
                                if (m_iHavePBR_Number < addingVal)
                                    m_iHavePBR_Number = addingVal;
                                else
                                    ;
                            else
                                m_iHavePBR_Number = 0;
                        }
                        else
                            ;
                    }
                    else
                        ;

                    m_arHaveDates[(int)type, hour - 1] = Convert.ToInt32 (table.Rows[i][1]); //true;
                }
                catch { }
            }

            return 0;
        }

        private int GetAdminDatesResponse(DataTable table, DateTime date)
        {
            return GetDatesResponse(CONN_SETT_TYPE.ADMIN, table, date);
        }

        protected override int GetPPBRDatesResponse(DataTable table, DateTime date)
        {
            return GetDatesResponse(CONN_SETT_TYPE.PBR, table, date);
        }
        /// <summary>
        /// ���������� ����� ����
        /// </summary>
        /// <param name="dt">��������� ����/�����</param>
        /// <param name="type">��� ������ (���, �����./��������)</param>
        /// <returns>����� ����</returns>
        private int getCurrentHour (DateTime dt, CONN_SETT_TYPE type) {
            int iRes = -1;

            ////������� �1
            //if (dt < serverTime.Date)
            //    //��� ��������� ���� ������, ��� ���� �� ������� (������ ������)
            //    if (m_ignore_date == true)
            //        //� ���������� ������� - ������������ ����/����� �������
            //        iRes = 0;
            //    else {
            //        //� ���������� ������� - ��������� ����/����� �������
            //        int offset_days = (dt - serverTime.Date).Days;
            //        if ((offset_days == -1) && (serverTime.Hour < 1)) //�������������� ��������
            //            iRes = m_curRDGValues.Length;
            //        else
            //            iRes = -1; //������������ ��������
            //    }
            //else
            //    if (dt == serverTime.Date)
            //        //��� ������� ����
            //        if (m_ignore_date == true)
            //            //� ���������� ������� - ������������ ����/����� �������
            //            iRes = 0;
            //        else
            //        {
            //            //� ���������� ������� - ��������� ����/����� �������                                        
            //            if (type == CONN_SETT_TYPE.ADMIN)
            //                //����������� �������� ������������ �� ���./��� ������ ��� �����./����.
            //                iRes = serverTime.Hour == 0 ? serverTime.Hour : serverTime.Hour - 1;
            //            else
            //                if (type == CONN_SETT_TYPE.PBR)
            //                    iRes = serverTime.Hour;
            //                else
            //                    ;
            //        }
            //    else
            //        if (dt > serverTime.Date)
            //            //��� ���� "� �������"
            //            iRes = 0;
            //        else
            //            ;

            //������� �2
            if (m_ignore_date == false)
                //� ���������� ������� - ��������� ����/����� ������� (�� ������������)
                if (dt < serverTime.Date)                    
                {
                    //��� ��������� ���� ������, ��� ���� �� ������� (������ ������)
                    int offset_days = (dt - serverTime.Date).Days;
                    if ((offset_days == -1) && (serverTime.Hour < 1)) //�������������� ��������
                        iRes = m_curRDGValues.Length;
                    else
                        iRes = -1; //������������ ��������
                }
                else
                    if (dt == serverTime.Date)
                        //��� ������� ����
                        if (type == CONN_SETT_TYPE.ADMIN)
                            //����������� �������� ������������ �� ���./��� ������ ��� �����./����.
                            iRes = serverTime.Hour == 0 ? serverTime.Hour : serverTime.Hour - 1;
                        else
                            if (type == CONN_SETT_TYPE.PBR)
                                iRes = serverTime.Hour;
                            else
                                ; //??? ������������ ���
                    else
                        if (dt > serverTime.Date)
                            //��� ���� "� �������"
                            iRes = 0;
                        else
                            ; //??? ������������ ���
            else
                //� ���������� ������� - ������������ ����/����� �������
                iRes = 0;

            //����� ��� "������������"
            if (type == CONN_SETT_TYPE.ADMIN)
                //����������� �������� ������������ �� ����./���
                if (iRes > 0)
                    iRes--;
                else
                    ;
            else
                ;

            return iRes;
        }

        protected virtual string [] setAdminValuesQuery(StatisticCommon.TEC t, TECComponent comp, DateTime date)
        {
            string[] resQuery = new string[(int)DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE] { string.Empty, string.Empty, string.Empty };

            date = date.Date;
            int offset = -1
                , currentHour = -1;

            currentHour = getCurrentHour(date, CONN_SETT_TYPE.ADMIN);
            if (currentHour < 0)
                //������������ ��������
                return resQuery;
            else
                ;

            for (int i = currentHour; i < m_curRDGValues.Length; i++)
            {
                offset = GetSeasonHourOffset(i + 1);

                // ������ ��� ����� ���� �������, ������������ �
                if (IsHaveDates(CONN_SETT_TYPE.ADMIN, i) == true)
                {
                    resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + TEC.s_NameTableAdminValues +
                        @" SET " +
                        @"REC='" + m_curRDGValues[i].recomendation.ToString("F3", CultureInfo.InvariantCulture) +
                        @"', " + @"IS_PER=" + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                        @", " + "DIVIAT='" + m_curRDGValues[i].deviation.ToString("F3", CultureInfo.InvariantCulture) +
                        @"', " + "SEASON=" + (offset > 0 ? (SEASON_BASE + (int)HAdmin.seasonJumpE.WinterToSummer) : (SEASON_BASE + (int)HAdmin.seasonJumpE.SummerToWinter)) +
                        @", " + "FC=" + (m_curRDGValues[i].fc ? 1 : 0) +
                        @" WHERE " +
                        //@"DATE = '" + date.AddHours(i + 1 - offset).ToString("yyyyMMdd HH:mm:ss") +
                        //@"'" +
                        //@" AND ID_COMPONENT = " + comp.m_id +
                        //@" AND " +
                        @"ID = " + m_arHaveDates[(int)CONN_SETT_TYPE.ADMIN, i] +
                        @"; ";
                }
                else
                {
                    // ������ �����������, ���������� ��������
                    resQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours(i + 1 - offset).ToString("yyyyMMdd HH:mm:ss") +
                        @"', '" + m_curRDGValues[i].recomendation.ToString("F3", CultureInfo.InvariantCulture) +
                        @"', " + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                        @", '" + m_curRDGValues[i].deviation.ToString("F3", CultureInfo.InvariantCulture) +
                        @"', " + (comp.m_id) +
                        @", " + (offset > 0 ? (SEASON_BASE + (int)HAdmin.seasonJumpE.WinterToSummer) : (SEASON_BASE + (int)HAdmin.seasonJumpE.SummerToWinter)) +
                        @", " + (m_curRDGValues[i].fc ? 1 : 0) +
                        @"),";
                }
            }

            resQuery[(int)DbTSQLInterface.QUERY_TYPE.DELETE] = string.Empty;

            return resQuery;
        }
        
        protected virtual void SetAdminValuesRequest(StatisticCommon.TEC t, TECComponent comp, DateTime date)
        {
            //Logging.Logg().Debug("AdminTS::SetAdminValuesRequest () - ...", Logging.INDEX_MESSAGE.NOT_SET);

            string[] query = setAdminValuesQuery(t, comp, date);

            // ��������� ��� ������, �� ��������� � ����
            if (! (query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] == ""))
            {
                query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = @"INSERT INTO " + TEC.s_NameTableAdminValues + " (DATE, " + @"REC" +
                    @", " + "IS_PER" +
                    @", " + "DIVIAT" +
                    @", " + "ID_COMPONENT" +
                    @", " + "SEASON" +
                    @", " + "FC" +
                    @") VALUES" + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Substring(0, query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Length - 1) + ";";                    
            }
            else
                ;

            Request(m_dictIdListeners[t.m_id][(int)CONN_SETT_TYPE.ADMIN], query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]);
        }

        protected virtual void ClearAdminValuesRequest(TEC t, TECComponent comp, DateTime date)
        {
            string[] query = new string[(int)DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE] { string.Empty, string.Empty, string.Empty };
            
            int currentHour = -1;

            date = date.Date;

            if ((serverTime.Date < date) || (m_ignore_date == true))
                currentHour = 0;
            else
                currentHour = serverTime.Hour;

            for (int i = currentHour; i < 24; i++)
                // ������ ��� ����� ���� �������, ������������ �
                if (IsHaveDates(CONN_SETT_TYPE.ADMIN, i) == true)
                    query[(int)DbTSQLInterface.QUERY_TYPE.DELETE] += @"DELETE FROM " + TEC.s_NameTableAdminValues +
                        @" WHERE " +
                        @"DATE = '" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                        @"'" +
                        @" AND ID_COMPONENT = " + comp.m_id + "; ";
                else
                    ;

            //Logging.Logg().Debug("AdminTS::ClearAdminValuesRequest () - ...", Logging.INDEX_MESSAGE.NOT_SET);

            //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, requestUpdate + requestInsert + requestDelete);
            Request(m_dictIdListeners[t.m_id][(int)CONN_SETT_TYPE.ADMIN], query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]);
        }

        protected virtual string[] setPPBRQuery(TEC t, TECComponent comp, DateTime date)
        {
            string[] resQuery = new string[(int)DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE] { string.Empty, string.Empty, string.Empty };

            date = date.Date;
            int currentHour = getCurrentHour (date, CONN_SETT_TYPE.PBR);

            for (int i = currentHour; i < 24; i++)
            {
                // ������ ��� ����� ���� �������, ������������ �
                if (IsHaveDates(CONN_SETT_TYPE.PBR, i) == true)
                {
                    bool bUpdate = m_ignore_date;
                    int pbr_number = GetPBRNumber (i);

                    if (bUpdate == false)
                        if (m_iHavePBR_Number < pbr_number)
                            //����������� ������
                            bUpdate = true;
                        else
                            if (m_iHavePBR_Number == pbr_number)
                                if (m_sOwner_PBR == 1)
                                    //��� �������� - ���������� ������������
                                    bUpdate = true;
                                else
                                    //��� �������� - �������� ���������
                                    ;
                            else
                                //����������� �����
                                ;
                    else
                        ;

                    Logging.Logg().Debug(@"AdminTS::setPPBRQuery () - [ID_COMPONENT=" + comp.m_id + @"] ���=" + i + @"; ��=" + m_iHavePBR_Number + @"; �����=" + pbr_number, Logging.INDEX_MESSAGE.D_002);

                    if (bUpdate == true) {
                        resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE [" + TEC.s_NameTableUsedPPBRvsPBR + @"]" +
                                    " SET " +
                                    @"PBR_NUMBER='";
                        resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += m_curRDGValues[i].pbr_number;
                        resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += "'" +
                                    @", WR_DATE_TIME='" + serverTime.ToString("yyyyMMdd HH:mm:ss") + @"'" +
                                    @", OWNER=" + m_sOwner_PBR +
                                    @", PBR='" + m_curRDGValues[i].pbr.ToString("F3", CultureInfo.InvariantCulture) + "'" +
                                    @", Pmin='" + m_curRDGValues[i].pmin.ToString("F3", CultureInfo.InvariantCulture) + "'" +
                                    @", Pmax='" + m_curRDGValues[i].pmax.ToString("F3", CultureInfo.InvariantCulture) + "'" +
                                    @" WHERE " +
                                    //@"DATE_TIME" + @" = '" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") + @"'" +
                                    //@" AND ID_COMPONENT = " + comp.m_id +
                                    //@" AND " +
                                    @"ID = " + m_arHaveDates[(int)CONN_SETT_TYPE.PBR, i] +
                                    @"; ";
                    }
                    else
                        ;
                }
                else
                {
                    string strPBRNumber = string.Empty;
                    if ((!(m_curRDGValues[i].pbr_number == null)) && (m_curRDGValues[i].pbr_number.Length > @"���".Length))
                        strPBRNumber = m_curRDGValues[i].pbr_number;
                    else
                        strPBRNumber = getNamePBRNumber();

                    // ������ �����������, ���������� ��������
                    Logging.Logg().Debug(@"AdminTS::setPPBRQuery () - [ID_COMPONENT=" + comp.m_id + @"] ���=" + i + @"; ��=" + m_iHavePBR_Number + @"; �����=" + strPBRNumber, Logging.INDEX_MESSAGE.D_002);

                    if (!(m_curRDGValues[i].pbr < 0))
                        resQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                            @"', '" + serverTime.ToString("yyyyMMdd HH:mm:ss") + @"'" +
                            //@", 'GETDATE()" +
                            @", '" + strPBRNumber +
                            @"', " + comp.m_id +
                            @", '" + m_sOwner_PBR + "'" +
                            @", '" + m_curRDGValues[i].pbr.ToString("F3", CultureInfo.InvariantCulture) + "'" +
                            @", '" + m_curRDGValues[i].pmin.ToString("F3", CultureInfo.InvariantCulture) + "'" +
                            @", '" + m_curRDGValues[i].pmax.ToString("F3", CultureInfo.InvariantCulture) + "'" +
                            @"),";
                    else
                        ; //������ ���������� �������� "-1"
                }
            }

            return resQuery;
        }

        protected /*virtual*/ void SetPPBRRequest(TEC t, TECComponent comp, DateTime date)
        {
            string[] query = setPPBRQuery(t, comp, date);

            // ��������� ��� ������, �� ��������� � ����
            if (query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Equals (string.Empty) == false)
            {
                query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = @"INSERT INTO [" + TEC.s_NameTableUsedPPBRvsPBR + "] (DATE_TIME, WR_DATE_TIME, PBR_NUMBER, ID_COMPONENT, OWNER, PBR, Pmin, Pmax) VALUES" + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Substring(0, query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Length - 1) + ";";
            }
            else
                ;

            query[(int)DbTSQLInterface.QUERY_TYPE.DELETE] = @"";

            if ((query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE].Equals(string.Empty) == false) ||
                (query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Equals(string.Empty) == false))
            {
                //Logging.Logg().Debug(@"AdminTS::setPPBRQuery () - INSERT: " + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT], Logging.INDEX_MESSAGE.D_005);
                //Logging.Logg().Debug(@"AdminTS::setPPBRQuery () - UPDATE: " + query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE], Logging.INDEX_MESSAGE.D_005);
                //Logging.Logg().Debug(@"AdminTS::setPPBRQuery () - DELETE: " + resQuery[(int)DbTSQLInterface.QUERY_TYPE.DELETE], Logging.INDEX_MESSAGE.D_005);

                Request(m_dictIdListeners[t.m_id][(int)CONN_SETT_TYPE.PBR], query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE]
                                                                            + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT]
                                                                            + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]);
            }
            else
                //Logging.Logg().Debug("AdminTS::SetPPBRRequest () - Empty", Logging.INDEX_MESSAGE.NOT_SET) //������ ����
                ;
        }

        protected virtual void ClearPPBRRequest(TEC t, TECComponent comp, DateTime date)
        {
            string[] query = new string[(int)DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE] { string.Empty, string.Empty, string.Empty };
            
            int currentHour = -1;

            date = date.Date;

            if ((serverTime.Date < date) || (m_ignore_date == true))
                currentHour = 0;
            else
                currentHour = serverTime.Hour;

            for (int i = currentHour; i < 24; i++)
                // ������ ��� ����� ���� �������, ������������ �
                if (IsHaveDates(CONN_SETT_TYPE.PBR, i) == true)
                    query[(int)DbTSQLInterface.QUERY_TYPE.DELETE] += @"DELETE FROM [" + TEC.s_NameTableUsedPPBRvsPBR + @"]" +
                        @" WHERE " +
                        @"DATE_TIME" + @" = '" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                        @"'" +
                        @" AND ID_COMPONENT = " + comp.m_id + "; ";
                else
                    ;

            //Logging.Logg().Debug("ClearPPBRRequest () - ...", Logging.INDEX_MESSAGE.NOT_SET);

            //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, requestUpdate + requestInsert + requestDelete);
            Request(m_dictIdListeners[t.m_id][(int)CONN_SETT_TYPE.PBR], query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]);
        }

        public int GetIndexTECComponent (int idTEC, int idComp) {
            int iRes = -1;

            foreach (TECComponent comp in allTECComponents)
            {
                if ((comp.tec.m_id == idTEC) && (comp.m_id == idComp)) {
                    iRes = allTECComponents.IndexOf (comp);
                    break;
                }
                else
                    ;
            }

            return iRes;
        }

        /*protected override bool InitDbInterfaces()
        {
            bool bRes = true;

            m_listDbInterfaces.Clear();

            m_listListenerIdCurrent.Clear();
            m_indxDbInterfaceCurrent = -1;

            Int16 connSettType = -1;
            DbTSQLInterface.DB_TSQL_INTERFACE_TYPE dbType = DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.UNKNOWN;
            foreach (TEC t in m_list_tec)
            {
                for (connSettType = (int)CONN_SETT_TYPE.ADMIN; connSettType < (int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; connSettType++)
                {
                    if ((m_ignore_connsett_data == true) && ((connSettType == (int)CONN_SETT_TYPE.DATA_ASKUE) && (connSettType == (int)CONN_SETT_TYPE.DATA_SOTIASSO)))
                        continue;
                    else
                        ;

                    dbType = DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.UNKNOWN;

                    if (object.ReferenceEquals(t.connSetts[connSettType], null) == false)
                    {
                        bool isAlready = false;

                        foreach (DbTSQLInterface dbi in m_listDbInterfaces)
                        {
                            //if (! (t.connSetts [0] == cs))
                            //if (dbi.connectionSettings.Equals(t.connSetts[(int)CONN_SETT_TYPE.ADMIN]) == true)
                            if (((ConnectionSettings)dbi.m_connectionSettings) == t.connSetts[connSettType])
                            //if (! (dbi.connectionSettings != t.connSetts[(int)CONN_SETT_TYPE.ADMIN]))
                            {
                                isAlready = true;

                                t.m_arIndxDbInterfaces[connSettType] = m_listDbInterfaces.IndexOf(dbi);
                                t.m_arListenerIds[connSettType] = m_listDbInterfaces[t.m_arIndxDbInterfaces[connSettType]].ListenerRegister();

                                break;
                            }
                            else
                                ;
                        }

                        if (isAlready == false)
                        {
                            string dbNameType = string.Empty;

                            dbType = DbTSQLInterface.getTypeDB (t.connSetts[connSettType].port);
                            switch (dbType)
                            {
                                case DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.MySQL:
                                    dbNameType = "MySql";
                                    break;
                                case DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.MSSQL:
                                    dbNameType = "MSSQL";
                                    break;
                                default:
                                    dbNameType = string.Empty;
                                    break;
                            }

                            if (!(dbType < 0))
                            {
                                m_listDbInterfaces.Add(new DbTSQLInterface(dbType, "���������: " + dbNameType + "-��, " + "���: " + t.name_shr));
                                m_listListenerIdCurrent.Add(-1);

                                t.m_arIndxDbInterfaces[connSettType] = m_listDbInterfaces.Count - 1;
                                t.m_arListenerIds[connSettType] = m_listDbInterfaces[m_listDbInterfaces.Count - 1].ListenerRegister();

                                m_listDbInterfaces[m_listDbInterfaces.Count - 1].Start();

                                m_listDbInterfaces[m_listDbInterfaces.Count - 1].SetConnectionSettings(t.connSetts[connSettType], true);
                            }
                            else
                                ;
                        }
                        else
                            ; //isAlready == true
                    }
                    else
                        ; //t.connSetts[connSettType] == null
                }
            }

            return bRes;
        }*/

        public override void Start()
        {
            //if (threadIsWorking == true)
            //    return;
            //else
            //    ;

            if (!(m_list_tec == null))
                //foreach (TEC t in m_list_tec) {
                    StartDbInterfaces();
                //}
            else
                Logging.Logg().Error(@"AdminTS::Start () - m_list_tec == null", Logging.INDEX_MESSAGE.NOT_SET);

            semaDBAccess = new Semaphore(1, 1);

            base.Start();
        }

        public override void Stop()
        {
            base.Stop();

            if (! (m_list_tec == null))
                foreach (TEC t in m_list_tec)
                {
                    StopDbInterfaces();
                }
            else
                Logging.Logg().Error(@"AdminTS::Stop () - m_list_tec == null", Logging.INDEX_MESSAGE.NOT_SET);
        }

        protected override int StateRequest(int /*StatesMachine*/ state)
        {
            int result = 0;
            string strRep = string.Empty;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    strRep = @"��������� �������� ������� �������.";
                    GetCurrentTimeRequest();
                    break;
                case (int)StatesMachine.PPBRValues:
                    strRep = @"��������� ������ �����.";
                    if (indxTECComponents < allTECComponents.Count)
                        GetPPBRValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate.Date);
                    else
                        ; //result = false;
                    break;
                case (int)StatesMachine.AdminValues:
                    strRep = @"��������� ���������������� ������.";
                    if ((indxTECComponents < allTECComponents.Count) && (m_markQueries.IsMarked ((int)CONN_SETT_TYPE.ADMIN) == true))
                        GetAdminValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate.Date);
                    else
                        ; //result = false;

                    //this.BeginInvoke(delegateCalendarSetDate, m_prevDatetime);
                    break;

                case (int)StatesMachine.ImpRDGExcelValues:
                    strRep = @"������ ��� �� Excel.";
                    delegateImportForeignValuesRequuest();
                    break;
                case (int)StatesMachine.ExpRDGExcelValues:
                    strRep = @"������� ��� � ����� Excel.";
                    delegateExportForeignValuesRequuest();
                    break;
                 case (int)StatesMachine.CSVValues:
                    strRep = @"������ �� ������� CSV.";
                    delegateImportForeignValuesRequuest();
                    break;
                case (int)StatesMachine.PPBRDates:
                    if ((serverTime.Date > m_curDate.Date) && (m_ignore_date == false))
                    {
                        //������������� ����������
                        saveResult = Errors.InvalidValue;
                        try
                        {
                            semaDBAccess.Release(1);
                        }
                        catch
                        {
                        }
                        result = -1;
                        break;
                    }
                    else
                        ;                        
                    strRep = @"��������� ������ ����������� ������� ��������.";
                    GetPPBRDatesRequest(m_curDate);
                    break;
                case (int)StatesMachine.AdminDates:
                    //int offset_days = (m_curDate.Date - serverTime.Date).Days;
                    //if (((offset_days > 0) && (m_ignore_date == false))
                    //    || (((offset_days > 1) && (serverTime.Hour > 0)) && (m_ignore_date == false)))
                    if (getCurrentHour (m_curDate, CONN_SETT_TYPE.ADMIN) < 0)
                    {
                        //������������� ����������
                        saveResult = Errors.InvalidValue;
                        try
                        {
                            semaDBAccess.Release(1);
                        }
                        catch
                        {
                        }
                        result = -1;
                        break;
                    }
                    else
                        ;
                    strRep = @"��������� ������ ����������� ������� ��������.";
                    if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true)
                        GetAdminDatesRequest(m_curDate);
                    else
                        ;
                    break;
                case (int)StatesMachine.SaveAdminValues:
                    strRep = @"���������� ���������������� ������.";
                    if ((indxTECComponents < allTECComponents.Count) && (m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true))
                        SetAdminValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    else
                        ; //result = false;
                    break;
                case (int)StatesMachine.SavePPBRValues:
                    strRep = @"���������� �����.";
                    if (indxTECComponents < allTECComponents.Count)
                        SetPPBRRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    else
                        ; //result = false;
                    break;
                //case StatesMachine.LayoutGet:
                //    ActionReport("��������� ���������������� ������ ������.");
                //    GetLayoutRequest(m_curDate);
                //    break;
                //case StatesMachine.LayoutSet:
                //    ActionReport("���������� ���������������� ������ ������.");
                //    SetLayoutRequest(m_curDate);
                //    break;
                case (int)StatesMachine.ClearAdminValues:
                    strRep = @"���������� ���������������� ������.";
                    if (indxTECComponents < allTECComponents.Count)
                        ClearAdminValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    else
                        ; //result = false;
                    break;
                case (int)StatesMachine.ClearPPBRValues:
                    strRep = @"���������� �����.";
                    if (indxTECComponents < allTECComponents.Count)
                        ClearPPBRRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    else
                        ; //result = false;
                    break;
                default:
                    break;
            }

            ActionReport(strRep);

            //Logging.Logg().Debug(@"AdminTS::StateRequest () - state=" + state.ToString() + @" - �����...");

            return result;
        }

        protected override int StateCheckResponse(int /*StatesMachine*/ state, out bool error, out object table)
        {
            int iRes = -1;

            error = true;
            table = null;

            if (((state == (int)StatesMachine.ImpRDGExcelValues) || (state == (int)StatesMachine.ExpRDGExcelValues)) ||
                (state == (int)StatesMachine.CSVValues) ||
                /*((!(m_indxDbInterfaceCurrent < 0)) && (m_listListenerIdCurrent.Count > 0))*/
                (!(m_IdListenerCurrent < 0)))
            {
                switch (state)
                {
                    case (int)StatesMachine.ImpRDGExcelValues:
                        if ((!(m_tableRDGExcelValuesResponse == null)) && (m_tableRDGExcelValuesResponse.Rows.Count > 24))
                        {
                            error = false;

                            //??? ����� �� ������...
                            iRes = 0;
                        }
                        else
                            ;
                        break;
                    case (int)StatesMachine.ExpRDGExcelValues:
                            //??? ������ ����� ???
                            error = false;
                            iRes = 0;
                        break;
                     case (int)StatesMachine.CSVValues:
                        if ((!(m_tableValuesResponse == null)) && (m_tableValuesResponse.Rows.Count > 0))
                        {
                            error = false;

                            iRes = 0;
                        }
                        else
                            ;
                        break;
                    case (int)StatesMachine.AdminDates:
                        if (m_markQueries.IsMarked ((int)CONN_SETT_TYPE.ADMIN) == true)
                            iRes = response(m_IdListenerCurrent, out error, out table/*, false*/);
                        else {
                            error = false;
                            table = null;

                            iRes = 0;
                        }
                        break;
                    case (int)StatesMachine.CurrentTime:
                    case (int)StatesMachine.PPBRValues:
                    case (int)StatesMachine.AdminValues:
                    case (int)StatesMachine.PPBRDates:
                    case (int)StatesMachine.SaveAdminValues:
                    case (int)StatesMachine.SavePPBRValues:
                    //case (int)StatesMachine.UpdateValuesPPBR:
                    case (int)StatesMachine.ClearAdminValues:
                    case (int)StatesMachine.ClearPPBRValues:
                    //case (int)StatesMachine.GetPass:
                        iRes = response(m_IdListenerCurrent, out error, out table/*, false*/);
                        break;
                    //case (int)StatesMachine.LayoutGet:
                    //case (int)StatesMachine.LayoutSet:
                        //bRes = GetResponse(m_indxDbInterfaceCurrent, m_listListenerIdCurrent[m_indxDbInterfaceCurrent], out error, out table/*, true*/);
                        //break;
                    default:
                        break;
                }
            }
            else {
                //������???

                error = true;
                table = null;

                iRes = -1;
            }

            return iRes;
        }

        protected override int StateResponse(int /*StatesMachine*/ state, object table)
        {
            int result = -1;
            string strRep = string.Empty;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    result = GetCurrentTimeResponse(table as DataTable);
                    if (result == 0)
                    {
                        if (using_date == true) {
                            m_prevDate = serverTime.Date;
                            m_curDate = m_prevDate;

                            if (!(setDatetime == null)) setDatetime(m_curDate); else;
                        }
                        else
                            ;
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.PPBRValues:
                    result = GetPPBRValuesResponse(table as DataTable, m_curDate);
                    if (result == 0)
                    {
                        if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == false)
                        {
                            result = GetAdminValuesResponse(null, m_curDate);

                            if (result == 0)
                            {
                                readyData(m_prevDate);
                            }
                            else
                                ;
                        }
                        else
                            ;                        
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.AdminValues:
                    if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true)
                        result = GetAdminValuesResponse(table as System.Data.DataTable, m_curDate);
                    else {
                        table = null;

                        if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.PBR) == true)
                            result = GetAdminValuesResponse(null, m_curDate);
                        else
                            result = -1;
                    }

                    if (result == 0)
                    {
                        readyData(m_prevDate);
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.ImpRDGExcelValues:
                    ActionReport("������ ��� �� Excel.");
                    //result = GetRDGExcelValuesResponse(table, m_curDate);
                    result = delegateImportForeignValuesResponse();
                    if (result == 0)
                    {
                        readyData(m_prevDate);
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.ExpRDGExcelValues:
                    ActionReport("������� ��� � ����� Excel.");
                    //??? ������ ����� ???
                    saveResult = Errors.NoError;
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch
                    {
                    }
                    result = 0;
                    break;
                case (int)StatesMachine.CSVValues:
                    ActionReport("������ �������� �� ������� CSV.");
                    //result = GetRDGExcelValuesResponse(table, m_curDate);
                    result = delegateImportForeignValuesResponse();
                    if (result == 0)
                    {
                        readyData(m_prevDate);
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.PPBRDates:
                    ClearPPBRDates();
                    result = GetPPBRDatesResponse(table as System.Data.DataTable, m_curDate);
                    if (result == 0)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.AdminDates:
                    ClearAdminDates();
                    if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true)
                        result = GetAdminDatesResponse(table as System.Data.DataTable, m_curDate);
                    else
                        result = 0;

                    if (result == 0)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.SaveAdminValues:
                    saveResult = Errors.NoError;
                    //���� ��������� �������, �� ���������� ������ � ��
                    if (isLastState (state) == true)
                        try { semaDBAccess.Release(1); }
                        catch { }
                    else
                        ;
                    result = 0;
                    if (result == 0) { }
                    else ;
                    break;
                case (int)StatesMachine.SavePPBRValues:
                    saveResult = Errors.NoError;
                    //���� ��������� �������, �� ���������� ������ � ��
                    if (isLastState(state) == true)
                        try { semaDBAccess.Release(1); }
                        catch (Exception e) {
                            Logging.Logg().Exception(e, @"AdminTS::StateResponse () - semaDBAccess.Release(1) - StatesMachine.SavePPBRValues...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    else
                        ;
                    result = 0;
                    if (result == 0)
                    {
                        Logging.Logg().Debug(@"AdminTS::StateResponse () - saveComplete is set=" + (saveComplete == null ? false.ToString() : true.ToString()) + @" - �����...", Logging.INDEX_MESSAGE.NOT_SET);

                        //����� ���������� �������� ���������� ��������� - ������ �������� �� ���������
                        //if (!(saveComplete == null)) saveComplete(); else ;
                    }
                    else
                        ;
                    break;
                //case (int)StatesMachine.LayoutGet:
                //    result = GetLayoutResponse(table, m_curDate);
                //    if (result == true)
                //    {
                //        loadLayoutResult = Errors.NoError;
                //        try
                //        {
                //            semaLoadLayout.Release(1);
                //        }
                //        catch
                //        {
                //        }
                //    }
                //    break;
                //case (int)StatesMachine.LayoutSet:
                //    loadLayoutResult = Errors.NoError;
                //    try
                //    {
                //        semaLoadLayout.Release(1);
                //    }
                //    catch
                //    {
                //    }
                //    result = true;
                //    if (result == true)
                //    {
                //    }
                //    else
                //        ;
                //    break;
                case (int)StatesMachine.ClearAdminValues:
                    result = 0;
                    if (result == 0) { }
                    else ;
                    break;
                case (int)StatesMachine.ClearPPBRValues:
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch
                    {
                    }
                    result = 0;
                    if (result == 0)
                    {
                    }
                    break;
                default:
                    break;
            }

            if (result == 0)
                clearReportStates (false);
            else
                ;

            //Logging.Logg().Debug(@"AdminTS::StateResponse () - state=" + state.ToString() + @", result=" + result.ToString() + @" - �����...");

            return result;
        }

        protected override INDEX_WAITHANDLE_REASON StateErrors(int /*StatesMachine*/ state, int request, int result)
        {
            INDEX_WAITHANDLE_REASON reasonRes = INDEX_WAITHANDLE_REASON.SUCCESS;
            
            bool bClear = false;

            string error = string.Empty,
                reason = string.Empty,
                waiting = string.Empty;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    if (request == 0)
                    {
                        reason = @"�������";

                        if (saving == true)
                            saveResult = Errors.ParseError;
                        else
                            ;
                    }
                    else
                    {
                        reason = @"���������";

                        if (saving == true)
                            saveResult = Errors.NoAccess;
                        else
                            ;
                    }

                    reason += @" �������� ������� �������";
                    waiting = @"������� � ��������";

                    if (saving == true)
                    {
                        try
                        {
                            semaDBAccess.Release(1);
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, "AdminTS::StateErrors () - semaDBAccess.Release(1)", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.PPBRValues:
                    if (request == 0)
                        reason = @"�������";
                    else {
                        reason = @"���������";
                        bClear = true;
                    }

                    reason += @" ������ �����";
                    waiting = @"������� � ��������";

                    break;
                case (int)StatesMachine.AdminValues:
                    if (request == 0)
                        reason = @"�������";
                    else {
                        reason = @"���������";
                        bClear = true;
                    }

                    reason += @" ���������������� ������";
                    waiting = @"������� � ��������";

                    break;
                case (int)StatesMachine.ImpRDGExcelValues:
                    reason = @"������� ��� �� ����� Excel";
                    waiting = @"������� � ��������";

                    // ???
                    break;
                case (int)StatesMachine.ExpRDGExcelValues:
                    reason = @"�������� ��� �� ����� Excel";
                    waiting = @"������� � ��������";
                    // ???
                    saveResult = Errors.NoAccess;
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                case (int)StatesMachine.CSVValues:
                    reason = @"������� �� ������� CSV";
                    waiting = @"������� � ��������";

                    // ???
                    break;
                case (int)StatesMachine.PPBRDates:
                    if (request == 0)
                    {
                        reason = @"�������";
                        saveResult = Errors.ParseError;
                    }
                    else
                    {
                        reason = @"���������";
                        saveResult = Errors.NoAccess;
                    }
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "AdminTS::StateErrors () - semaDBAccess.Release(1)", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    reason += @" ����������� ������� �������� (PPBR)";
                    waiting = @"������� � ��������";

                    break;
                case (int)StatesMachine.AdminDates:
                    if (request == 0)
                    {
                        reason = @"�������";
                        saveResult = Errors.ParseError;
                    }
                    else
                    {
                        reason = @"���������";
                        saveResult = Errors.NoAccess;
                    }
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "AdminTS::StateErrors () - semaDBAccess.Release(1)", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    reason += @" ����������� ������� �������� (AdminValues)";
                    waiting = @"������� � ��������";

                    break;
                case (int)StatesMachine.SaveAdminValues:
                    saveResult = Errors.NoAccess;
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "AdminTS::StateErrors () - semaDBAccess.Release(1)", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    reason = @"���������� ���������������� ������";
                    waiting = @"������� � ��������";
                    break;
                case (int)StatesMachine.SavePPBRValues:
                    saveResult = Errors.NoAccess;
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch
                    {
                    }

                    reason = @"���������� ������ �����";
                    waiting = @"������� � ��������";

                    break;
                //case (int)StatesMachine.LayoutGet:
                //    if (response)
                //    {
                //        ErrorReport("������ ������� ���������������� ������ ������. ������� � ��������.");
                //        loadLayoutResult = Errors.ParseError;
                //    }
                //    else
                //    {
                //        ErrorReport("������ ��������� ���������������� ������ ������. ������� � ��������.");
                //        loadLayoutResult = Errors.ParseError;
                //    }
                //    try
                //    {
                //        semaLoadLayout.Release(1);
                //    }
                //    catch
                //    {
                //    }
                //    break;
                //case (int)StatesMachine.LayoutSet:
                //    ErrorReport("������ ���������� ���������������� ������ ������. ������� � ��������.");
                //    loadLayoutResult = Errors.NoAccess;
                //    try
                //    {
                //        semaLoadLayout.Release(1);
                //    }
                //    catch
                //    {
                //    }
                //    break;
                case (int)StatesMachine.ClearAdminValues:
                    reason = @"�������� ���������������� ������";
                    waiting = @"������� � ��������";
                    break;
                case (int)StatesMachine.ClearPPBRValues:
                    reason = @"�������� ������ �����";
                    waiting = @"������� � ��������";
                    break;
                default:
                    break;
            }

            if (bClear) {
                ClearValues();
                //ClearTables();
            }
            else
                ;

            error = "������ " + reason + ".";

            if (waiting.Equals(string.Empty) == false)
                error += " " + waiting + ".";
            else
                ;

            ErrorReport(error);

            if (! (errorData == null)) errorData (); else ;

            Logging.Logg().Error(@"AdminTS::StateErrors () - error=" + error + @" - �����...", Logging.INDEX_MESSAGE.NOT_SET);

            return reasonRes;
        }

        protected override void StateWarnings(int /*StatesMachine*/ state, int req, int res)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indx"></param>
        /// <param name="date"></param>
        /// <param name="bCallback"></param>
        public virtual void SaveRDGValues(/*TYPE_FIELDS mode, */int indx, DateTime date, bool bCallback)
        {
            lock (m_lockState) //???
            {
                indxTECComponents = indx;
                m_prevDate = date.Date;
            }

            Errors resultSaving = SaveChanges();
            if (resultSaving == Errors.NoError)
            {
                if (bCallback == true)
                    lock (m_lockState)
                    {
                        ClearStates();
                        ClearValues();

                        //m_prevDate = date.Date;
                        m_curDate = m_prevDate;
                        using_date = false;

                        //Logging.Logg().Debug("AdminTS::SaveRDGValues () - states.Clear() - ...", Logging.INDEX_MESSAGE.NOT_SET);

                        //AddState((int)StatesMachine.CurrentTime);
                        if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.PBR) == true)
                            AddState((int)StatesMachine.PPBRValues);
                        else
                            ;

                        if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true)
                            AddState((int)StatesMachine.AdminValues);
                        else
                            ;

                        Run(@"AdminTS::SaveRDGValues ()");
                    }
                else
                    ;
            }
            else
            {
                if (resultSaving == Errors.InvalidValue)
                    //MessageBox.Show(this, "��������� ������������� �����������!", "��������", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    base.MessageBox("��������� ������������� �����������!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                else
                    //MessageBox.Show(this, "�� ������� ��������� ���������, �������� ����������� ����� � ����� ������.", "������ ����������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    base.MessageBox("�� ������� ��������� ���������, �������� ����������� ����� � ����� ������.");
            }
        }

        //public void SaveRDGValues(/*TYPE_FIELDS mode, */int indx)
        //{
        //    lock (m_lockObj)
        //    {
        //        indxTECComponents = indx;
        //    }
            
        //    Errors resultSaving = SaveChanges();
        //    if (resultSaving == Errors.NoError)
        //    {
        //        lock (m_lockObj)
        //        {                    
        //            ClearValues();

        //            //m_curDate = date.Date;
        //            using_date = true;

        //            newState = true;
        //            states.Clear();

        //            Logging.Logg().LogLock();
        //            Logging.Logg().Send("SaveRDGValues () - states.Clear()", true, true, false);
        //            Logging.Logg().LogUnlock();

        //            AddState((int)StatesMachine.CurrentTime);
        //            AddState((int)StatesMachine.PPBRValues);
        //            AddState((int)StatesMachine.AdminValues);

        //            try
        //            {
        //                semaState.Release(1);
        //            }
        //            catch
        //            {
                        //Logging.Logg().LogLock();
                        //Logging.Logg().Send("catch - SaveRDGValues () - semaState.Release(1)", true, true, false);
                        //Logging.Logg().LogUnlock();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (resultSaving == Errors.InvalidValue)
        //            //MessageBox.Show(this, "��������� ������������� �����������!", "��������", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        //            MessageBox("��������� ������������� �����������!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        //        else
        //            //MessageBox.Show(this, "�� ������� ��������� ���������, �������� ����������� ����� � ����� ������.", "������ ����������", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            MessageBox("�� ������� ��������� ���������, �������� ����������� ����� � ����� ������.");
        //    }
        //}

        public virtual void ClearRDGValues(DateTime date)
        {
            if (ClearRDG() == Errors.NoError)
            {
                lock (m_lockState)
                {
                    ClearStates();
                    ClearValues();

                    m_prevDate = date.Date;
                    m_curDate = m_prevDate;
                    using_date = false;

                    //AddState((int)StatesMachine.CurrentTime);
                    if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.PBR) == true)
                        AddState((int)StatesMachine.PPBRValues);
                    else
                        ;
                    if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true)
                        AddState((int)StatesMachine.AdminValues);
                    else
                        ;

                    Run(@"AdminTS::ClearRDGValues ()");
                }
            }
            else
            {
                MessageBox("�� ������� ������� �������� ���, �������� ����������� ����� � ����� ������.");
            }
        }

        public void ReConnSettingsRDGSource(int idListaener, int [] arIdSource)
        {
            int err = -1;

            for (int i = 0; i < m_list_tec.Count; i ++) {
                if (m_list_tec[i].Type == TEC.TEC_TYPE.COMMON) {
                    //TYPE_DATABASE_CFG.CFG_200 = ???
                    m_list_tec[i].connSettings(StatisticCommon.InitTECBase.getConnSettingsOfIdSource(idListaener, arIdSource[(int)TEC.TEC_TYPE.COMMON], -1, out err), (int)CONN_SETT_TYPE.ADMIN);
                    m_list_tec[i].connSettings(StatisticCommon.InitTECBase.getConnSettingsOfIdSource(idListaener, arIdSource[(int)TEC.TEC_TYPE.COMMON], -1, out err), (int)CONN_SETT_TYPE.PBR);
                }
                else {
                    if (m_list_tec[i].Type == TEC.TEC_TYPE.BIYSK)
                    {
                        //TYPE_DATABASE_CFG.CFG_200 = ???
                        m_list_tec[i].connSettings(StatisticCommon.InitTECBase.getConnSettingsOfIdSource(idListaener, arIdSource[(int)TEC.TEC_TYPE.BIYSK], -1, out err), (int)CONN_SETT_TYPE.ADMIN);
                        m_list_tec[i].connSettings(StatisticCommon.InitTECBase.getConnSettingsOfIdSource(idListaener, arIdSource[(int)TEC.TEC_TYPE.BIYSK], -1, out err), (int)CONN_SETT_TYPE.PBR);
                    }
                    else
                    {
                    }
                }
            }
        }

        public int GetIdOwnerTECComponent(FormChangeMode.MODE_TECCOMPONENT selfMode, FormChangeMode.MODE_TECCOMPONENT ownerMode, int indx = -1)
        {
            if (indx < 0) indx = indxTECComponents; else ;

            if ((indx < allTECComponents.Count) && (modeTECComponent(indx) == selfMode))
            {
                foreach (TECComponent comp in allTECComponents)
                {
                    if ((comp.tec.m_id == allTECComponents[indx].tec.m_id) && (modeTECComponent(allTECComponents.IndexOf(comp)) == ownerMode))
                    {
                        foreach (TG tg in comp.m_listTG)
                        {
                            if (tg.m_id == allTECComponents[indx].m_id)
                            {
                                return comp.m_id;
                            }
                            else
                                ;
                        }
                    }
                    else
                        ;
                }
            }
            else ;

            return -1;
        }

        public int GetIdPCOwnerTECComponent(int indx = -1)
        {
            return GetIdOwnerTECComponent(FormChangeMode.MODE_TECCOMPONENT.TG, FormChangeMode.MODE_TECCOMPONENT.PC, indx);
        }

        public int GetIdGTPOwnerTECComponent(int indx = -1)
        {
            return GetIdOwnerTECComponent(FormChangeMode.MODE_TECCOMPONENT.TG, FormChangeMode.MODE_TECCOMPONENT.GTP, indx);
        }

        /// <summary>
        /// ������������� ���������� ���
        /// </summary>
        /// <param name="indx">������ � ������� '��� ����������'</param>
        /// <returns>�������������</returns>
        public int GetIdTECComponent(int indx = -1)
        {
            if (indx < 0) indx = indxTECComponents; else ;

            int iRes = -1;

            if (indx < allTECComponents.Count) iRes = allTECComponents[indx].m_id; else ;

            return iRes;
        }
        
        /// <summary>
        /// ������������ (�������) ���������� ���
        /// </summary>
        /// <param name="indx">������ � ������� '��� ����������'</param>
        /// <returns>������������</returns>
        public string GetNameTECComponent(int indx = -1)
        {
            if (indx < 0) indx = indxTECComponents; else ;

            string strRes = string.Empty;

            if (indx < allTECComponents.Count) strRes = allTECComponents[indx].name_shr; else ;

            return strRes;
        }

        /// <summary>
        /// ���������� ������� �������� (��� + ������������ = ���) ��� ������������ ���������
        /// </summary>
        public override void CopyCurToPrevRDGValues()
        {
            base.CopyCurToPrevRDGValues ();

            for (int i = 0; i < m_curRDGValues.Length; i++)
            {
                m_prevRDGValues[i].pbr = Math.Round(m_curRDGValues[i].pbr, 2);
                m_prevRDGValues[i].pmin = Math.Round(m_curRDGValues[i].pmin, 2);
                m_prevRDGValues[i].pmax = Math.Round(m_curRDGValues[i].pmax, 2);
                m_prevRDGValues[i].pbr_number = m_curRDGValues[i].pbr_number;
                m_prevRDGValues[i].dtRecUpdate = m_curRDGValues[i].dtRecUpdate;
                m_prevRDGValues[i].recomendation = m_curRDGValues[i].recomendation;
                m_prevRDGValues[i].deviationPercent = m_curRDGValues[i].deviationPercent;
                m_prevRDGValues[i].deviation = m_curRDGValues[i].deviation;
            }
        }

        /// <summary>
        /// ����������� �������� (��� + ������������ = ���) �� ���������
        /// </summary>
        /// <param name="source">��������</param>
        public override void getCurRDGValues(HAdmin source)
        {
            base.getCurRDGValues (source);

            for (int i = 0; i < source.m_curRDGValues.Length; i++)
            {
                m_curRDGValues[i].pbr = ((HAdmin)source).m_curRDGValues[i].pbr;
                m_curRDGValues[i].pmin = ((HAdmin)source).m_curRDGValues[i].pmin;
                m_curRDGValues[i].pmax = ((HAdmin)source).m_curRDGValues[i].pmax;
                m_curRDGValues[i].pbr_number = ((HAdmin)source).m_curRDGValues[i].pbr_number;
                m_curRDGValues[i].dtRecUpdate = ((HAdmin)source).m_curRDGValues[i].dtRecUpdate;
                m_curRDGValues[i].fc = ((HAdmin)source).m_curRDGValues[i].fc;
                m_curRDGValues[i].recomendation = ((HAdmin)source).m_curRDGValues[i].recomendation;
                m_curRDGValues[i].deviationPercent = ((HAdmin)source).m_curRDGValues[i].deviationPercent;
                m_curRDGValues[i].deviation = ((HAdmin)source).m_curRDGValues[i].deviation;
            }
        }

        //public override void ClearValues(int cnt = -1)
        public override void ClearValues()
        {
            //base.ClearValues (cnt);
            base.ClearValues();

            m_curRDGValues_PBR_0 = 0F;

            for (int i = 0; i < m_curRDGValues.Length; i++)
            {
                m_curRDGValues[i].pbr =
                m_curRDGValues[i].pmin = m_curRDGValues[i].pmax = 
                m_curRDGValues[i].recomendation = m_curRDGValues[i].deviation = 0;
                m_curRDGValues[i].deviationPercent = false;

                m_curRDGValues[i].pbr_number = string.Empty;
                m_curRDGValues[i].dtRecUpdate = DateTime.MinValue;
            }

            //CopyCurToPrevRDGValues();
        }

        public override bool WasChanged()
        {
            for (int i = 0; i < m_curRDGValues.Length; i++)
            {
                if (m_prevRDGValues[i].pbr != m_curRDGValues[i].pbr /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.PLAN].Value.ToString())*/)
                    return true;
                else
                    ;
                if (m_prevRDGValues[i].recomendation != m_curRDGValues[i].recomendation /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION].Value.ToString())*/)
                    return true;
                else
                    ;
                if (m_prevRDGValues[i].deviationPercent != m_curRDGValues[i].deviationPercent /*bool.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION_TYPE].Value.ToString())*/)
                    return true;
                else
                    ;
                if (m_prevRDGValues[i].deviation != m_curRDGValues[i].deviation /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION].Value.ToString())*/)
                    return true;
                else
                    ;
            }

            return false;
        }
    }
}
