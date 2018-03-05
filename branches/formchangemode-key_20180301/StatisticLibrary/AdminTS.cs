using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
//using MySql.Data.MySqlClient;
using System.Threading;
using System.Windows.Forms;

using ASUTP.Helper;
using ASUTP.Database;
using ASUTP.Core;
using ASUTP;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatisticCommon
{    
    public class AdminTS : HAdmin
    {
        protected Semaphore semaDBAccess;
        protected volatile Errors saveResult;
        protected volatile bool saving;

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
            AdminDates, //��������� ������ ���������� ������� ��������
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
        /// <summary>
        /// ������ �������� � ������� ��������� ����������/���������� ����������/������� �������� ���, ���������������� ��������
        ///  , �������� - ����������� �������� ��������� ������� 'SAVED'
        ///  , 'SAVED' - ������ ���������/���������
        /// </summary>
        public enum INDEX_MARK_PPBRVALUES { PBR_ENABLED, PBR_SAVED, ADMIN_ENABLED, ADMIN_SAVED };
        protected HMark m_markSavedValues;

        /// <summary>
        /// ������������ - �������� ��� ������� ������ ������ (�����. + ���) ��
        ///  , ����� ���������� ��� ������������� �������� ��������� � ��
        /// </summary>
        [Flags]
        public enum MODE_GET_RDG_VALUES {
            NOT_SET, DISPLAY = 0x1, EXPORT = 0x2, UNIT_TEST = 0x4
        }

        private MODE_GET_RDG_VALUES _modeGetRDGValues;

        /// <summary>
        /// ����� ������ ������ (�����. + ���) ��
        ///  , ��� ����������� �������� ������ �� ���
        ///  , ���� ��� ��� �������� �������� � ���� ��� ���./���� � ����� ��������� �������� ��� � ������������ ���������� �� ������ ����������
        /// </summary>
        public MODE_GET_RDG_VALUES ModeGetRDGValues
        {
            get
            {
                return _modeGetRDGValues;
            }

            set
            {
                if (((value & MODE_GET_RDG_VALUES.DISPLAY) == MODE_GET_RDG_VALUES.DISPLAY)
                    && (((value & MODE_GET_RDG_VALUES.EXPORT) == MODE_GET_RDG_VALUES.EXPORT)))
                    throw new InvalidOperationException ("AdminTS.ModeGetRDGValues::set - ����������������� ��������...");
                else if (((value & MODE_GET_RDG_VALUES.DISPLAY) == MODE_GET_RDG_VALUES.DISPLAY)
                    && ((_modeGetRDGValues & MODE_GET_RDG_VALUES.EXPORT) == MODE_GET_RDG_VALUES.EXPORT)) {
                    // ����������������� ��������
                    _modeGetRDGValues &= ~MODE_GET_RDG_VALUES.EXPORT;
                } else if (((value & MODE_GET_RDG_VALUES.EXPORT) == MODE_GET_RDG_VALUES.EXPORT)
                    && ((_modeGetRDGValues & MODE_GET_RDG_VALUES.DISPLAY) == MODE_GET_RDG_VALUES.DISPLAY)) {
                    // ����������������� ��������
                    _modeGetRDGValues &= ~MODE_GET_RDG_VALUES.DISPLAY;
                } else
                    ;

                if ((_modeGetRDGValues & MODE_GET_RDG_VALUES.UNIT_TEST) == MODE_GET_RDG_VALUES.UNIT_TEST)
                    _modeGetRDGValues |= value;
                else
                    _modeGetRDGValues = value;

                Logging.Logg ().Action ($@"AdminTS.ModeGetRDGValues::set - <{_modeGetRDGValues.ToString()}>..."
                    , Logging.INDEX_MESSAGE.D_006);
            }
        }

        public AdminTS(bool[] arMarkSavePPBRValues, TECComponentBase.TYPE type)
            : base(type)
        {
            _modeGetRDGValues = MODE_GET_RDG_VALUES.DISPLAY;
            m_markSavedValues = new HMark(0);

            if (!(arMarkSavePPBRValues == null))
            {
                if (arMarkSavePPBRValues.Length > (int)INDEX_MARK_PPBRVALUES.PBR_ENABLED)
                    m_markSavedValues.Set((int)INDEX_MARK_PPBRVALUES.PBR_ENABLED, arMarkSavePPBRValues[(int)INDEX_MARK_PPBRVALUES.PBR_ENABLED]);
                else ;

                if (arMarkSavePPBRValues.Length > (int)INDEX_MARK_PPBRVALUES.PBR_SAVED)
                    m_markSavedValues.Set((int)INDEX_MARK_PPBRVALUES.PBR_SAVED, arMarkSavePPBRValues[(int)INDEX_MARK_PPBRVALUES.PBR_SAVED] == true);
                else ;
            }
            else
                ;

            m_markSavedValues.Marked((int)INDEX_MARK_PPBRVALUES.ADMIN_ENABLED);
            m_markSavedValues.Marked((int)INDEX_MARK_PPBRVALUES.ADMIN_SAVED);
        }

        protected override void Initialize () {
            base.Initialize ();
        }   

        /// <summary>
        /// ���������� ���������
        /// </summary>
        /// <returns>���������� ������</returns>
        public virtual Errors SaveChanges()
        {
            Logging.Logg().Debug($"AdminTS::SaveChanges () - ���� ...{Environment.NewLine}StackTrace: {new System.Diagnostics.StackTrace()}"
                , Logging.INDEX_MESSAGE.D_006);

            bool bResSemaDbAccess = false;
            
            delegateStartWait?.Invoke();

            //Logging.Logg().Debug("AdminTS::SaveChanges () - delegateStartWait() - �������� �������� ��� semaDBAccess=" + DbInterface.MAX_WATING, Logging.INDEX_MESSAGE.NOT_SET);

            bResSemaDbAccess = semaDBAccess.WaitOne(Constants.MAX_WATING);
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

                    if (m_markSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.ADMIN_SAVED) == true)
                    {
                        AddState((int)StatesMachine.AdminDates);
                        AddState((int)StatesMachine.SaveAdminValues);
                    }
                    else ;

                    if (m_markSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.PBR_SAVED) == true)
                    {
                        AddState((int)StatesMachine.PPBRDates);
                        AddState((int)StatesMachine.SavePPBRValues);
                    }
                    else ;

                    Run(@"AdminTS::SaveChanges ()");
                }

                bResSemaDbAccess = semaDBAccess.WaitOne(Constants.MAX_WATING);
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
                Logging.Logg().Debug("AdminTS::SaveChanges () - semaDBAccess.WaitOne(" + Constants.MAX_WATING + @")=false", Logging.INDEX_MESSAGE.NOT_SET);

                saveResult = Errors.NoAccess;
                saving = true;
            }

            delegateStopWait?.Invoke();

            return saveResult;
        }

        /// <summary>
        /// ������� ���������������� �������� � �������� ���
        /// </summary>
        /// <returns>���������� ������</returns>
        protected virtual Errors ClearRDG()
        {
            Errors errClearResult;

            delegateStartWait();

            //Logging.Logg().Debug("AdminTS::ClearRDG () - delegateStartWait() - �������� �������� ��� semaDBAccess=" + DbInterface.MAX_WATING, Logging.INDEX_MESSAGE.NOT_SET);

            //if (semaDBAccess.WaitOne(6666) == true) {
            if (semaDBAccess.WaitOne(Constants.MAX_WATING) == true)
            {
                lock (m_lockState)
                {
                    ClearStates ();

                    errClearResult = Errors.NoError;
                    using_date = false;
                    m_curDate = m_prevDate;

                    //Logging.Logg().Debug("AdminTS::ClearRDG () - states.Clear() - ...", Logging.INDEX_MESSAGE.NOT_SET);

                    AddState((int)StatesMachine.CurrentTime);
                    if (m_markSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.ADMIN_SAVED) == true)
                    {
                        AddState((int)StatesMachine.AdminDates);
                        AddState((int)StatesMachine.ClearAdminValues);
                    }
                    else
                        ;

                    if (m_markSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.PBR_SAVED) == true)
                    {
                        AddState((int)StatesMachine.PPBRDates);
                        AddState((int)StatesMachine.ClearPPBRValues);
                    }
                    else
                        ;

                    Run(@"AdminTS::ClearRDG ()");
                }

                //�������� ��������� ������
                bool bSemaDBAccessWaitRes = semaDBAccess.WaitOne(Constants.MAX_WATING);
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

        /// <summary>
        /// ��������� ������ ����������� ��� � ����������� �� ���� ����������
        /// </summary>
        /// <param name="mode">����������� ���� �����������</param>
        /// <param name="bLimitLK">������� ����� ������ �� ��� ������������ ������</param>
        /// <returns>���������� ������ ������, �� ������� �������� ����� ����������</returns>
        public virtual List <FormChangeMode.KeyDevice>GetListKeyTECComponent (FormChangeMode.MODE_TECCOMPONENT mode, bool bLimitLK)
        {
            List <FormChangeMode.KeyDevice> listRes = new List <FormChangeMode.KeyDevice> ();

            int iLimitIdTec = bLimitLK == true ? (int)TECComponent.ID.LK : (int)TECComponent.ID.GTP;

            switch (mode) {
                case FormChangeMode.MODE_TECCOMPONENT.TEC:
                    foreach (TEC tec in m_list_tec) {
                        if (! (tec.m_id > iLimitIdTec))
                            listRes.Add(new FormChangeMode.KeyDevice () { Id = tec.m_id, Mode = mode });
                        else
                            ;
                    }
                    break;
                case FormChangeMode.MODE_TECCOMPONENT.GTP:
                case FormChangeMode.MODE_TECCOMPONENT.PC:
                case FormChangeMode.MODE_TECCOMPONENT.TG:
                    foreach (TECComponent comp in allTECComponents) {
                        if ((!(comp.tec.m_id > iLimitIdTec))
                            && (mode == comp.Mode))
                            listRes.Add(new FormChangeMode.KeyDevice () { Id = comp.m_id, Mode = mode });
                        else
                            ;
                    }
                    break;
                default:
                    break;
            }

            return listRes;
        }

        /// <summary>
        /// ��������� �������� ��� � ���������������� ��� ������ �������
        /// </summary>
        /// <param name="active">���� ����������</param>
        /// <returns>���������� ��������� ����� ���������� ������</returns>
        public override bool Activate(bool active)
        {
            bool bRes = base.Activate (active);

            if ((active == true)
                && (bRes == true)
                && (IsFirstActivated == true)) //������ ��� 1-�� ���������
                GetRDGValues (CurrentKey);
            else
                ;

            return bRes;
        }

        /// <summary>
        /// ���� �� ���� ��� ������ � Excel
        /// </summary>
        /// <param name="indx">������������� ���(��)</param>
        /// <returns>���������� ���� �����������</returns>
        public virtual bool IsRDGExcel (FormChangeMode.KeyDevice key) {
            bool bRes = false;

            bRes =  FindTECComponent(key).tec.GetAddingParameter(TEC.ADDING_PARAM_KEY.PATH_RDG_EXCEL).ToString().Length > 0;

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

        /// <summary>
        /// ��������� ������ ���-��������
        /// </summary>
        private void protectSavedPPBRValues()
        {
            if (m_markSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.PBR_ENABLED) == true) m_markSavedValues.UnMarked((int)INDEX_MARK_PPBRVALUES.PBR_SAVED); else ;
        }
        
        /// <summary>
        /// ���������� � ������� ��������� ���������������� � ��� ��������
        /// </summary>
        /// <param name="indx">������ ����������</param>
        public virtual void GetRDGValues (FormChangeMode.KeyDevice key) {
            //��������� ������ ���-��������
            protectSavedPPBRValues();

            lock (m_lockState)
            {
                ClearStates();
                ClearValues ();

                CurrentKey = key;

                using_date = true;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                //m_typeFields = mode;

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

        /// <summary>
        /// ���������� � ������� ��������� ���������������� � ��� ��������
        /// </summary>
        /// <param name="indx">������ ����������</param>
        /// <param name="date">���� ������������� ��������</param>
        public override void GetRDGValues(FormChangeMode.KeyDevice key, DateTime date)
        {
            //��������� ������ ���-��������
            protectSavedPPBRValues();

            lock (m_lockState)
            {
                ClearStates();

                CurrentKey = key;

                ClearValues();

                using_date = false;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_prevDate = date.Date;
                m_curDate = m_prevDate;

                //m_typeFields = (TYPE_FIELDS)mode;

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

        /// <summary>
        /// ��������� � ���������� ������� ��� ��������� �������� ����
        /// </summary>
        /// <param name="t">���</param>
        /// <param name="comp">��������� ���</param>
        /// <param name="date">���� �� ������� ���������� �������� ��������</param>
        protected override void getPPBRValuesRequest(TEC t, IDevice comp, DateTime date/*, AdminTS.TYPE_FIELDS mode*/)
        {
            Request(m_dictIdListeners [t.m_id][(int)CONN_SETT_TYPE.PBR], t.GetPBRValueQuery(comp, date/*, mode*/));
        }

        /// <summary>
        /// ��������� � ���������� ������� ��� ��������� ���������������� ��������
        /// </summary>
        /// <param name="t">���</param>
        /// <param name="comp">��������� ���</param>
        /// <param name="date">���� �� ������� ���������� �������� ��������</param>
        protected void getAdminValuesRequest(TEC t, IDevice comp, DateTime date/*, AdminTS.TYPE_FIELDS mode*/)
        {
            Request(m_dictIdListeners[t.m_id][(int)CONN_SETT_TYPE.ADMIN], t.GetAdminValueQuery(comp, date/*, mode*/));
        }

        /// <summary>
        /// ���������� � ������� ��������� �������� �� Excel
        /// </summary>
        /// <param name="indx">������ ����������</param>
        /// <param name="date">���� �� ������� ���������� �������� ��������</param>
        public virtual void ImpRDGExcelValues(FormChangeMode.KeyDevice key, DateTime date)
        {
            lock (m_lockState)
            {
                ClearStates();

                CurrentKey = key;
                
                ClearValues();

                using_date = false;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_prevDate = date.Date;
                m_curDate = m_prevDate;

                AddState((int)StatesMachine.ImpRDGExcelValues);

                Run(@"AdminTS::ImpRDGExcelValues ()");
            }
        }

        /// <summary>
        /// ���������� � ������� �������� �������� � Excel
        /// </summary>
        /// <param name="indx">������ ����������</param>
        /// <param name="date">���� �� ������� ���������� ��������� ��������</param>
        /// <returns>���������� ������</returns>
        public virtual Errors ExpRDGExcelValues(FormChangeMode.KeyDevice key, DateTime date)
        {
            delegateStartWait();
            //Logging.Logg().Debug("AdminTS::ExpRDGExcelValues () - delegateStartWait() - �������� �������� ��� semaDBAccess=" + DbInterface.MAX_WATING, Logging.INDEX_MESSAGE.NOT_SET);

            bool bSemaDBAccessWaitRes = semaDBAccess.WaitOne(Constants.MAX_WATING);
            //if (semaDBAccess.WaitOne(6666) == true) {
            if (bSemaDBAccessWaitRes == true)
            {
                lock (m_lockState)
                {
                    ClearStates();

                    CurrentKey = key;

                    saveResult = Errors.NoAccess;
                    saving = true;
                    using_date = false;
                    m_curDate = m_prevDate;

                    AddState((int)StatesMachine.ExpRDGExcelValues);

                    Run(@"AdminTS::ExpRDGExcelValues ()");
                }

                bSemaDBAccessWaitRes = semaDBAccess.WaitOne(Constants.MAX_WATING);
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
                    Logging.Logg().Debug("AdminTS::ExpRDGExcelValues () - semaDBAccess.WaitOne(" + Constants.MAX_WATING + @")=" + bSemaDBAccessWaitRes.ToString (), Logging.INDEX_MESSAGE.NOT_SET);
                    
                    saveResult = Errors.NoAccess;
                    saving = true;
                }
            }

            delegateStopWait();

            return saveResult;
        }

        /// <summary>
        /// ��������� �������� ����
        /// </summary>
        /// <param name="table">������� � ����������� �������</param>
        /// <param name="date">����</param>
        /// <returns>������</returns>
        protected override int getPPBRValuesResponse(DataTable table, DateTime date)
        {
            int iRes = 0;
            
            m_tableValuesResponse = table.Copy ();
            
            return iRes;
        }

        protected virtual double getRDGValue_PBR_0(DataRow r, int indxTables, int cntFields)
        {
            return (double)r[indxTables * cntFields + (0 + 1)];
        }

        /// <summary>
        /// ��������� ���������������� �������� � ����������� ������ ���������������� � ���� �������� 
        /// </summary>
        /// <param name="tableAdminValuesResponse">������� � ����������������� ����������</param>
        /// <param name="date">���� �� ������� ���������� �������</param>
        /// <returns>������</returns>
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
            else
                ;


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
                        hour = ((DateTime)table.Rows[i]["DATE_PBR"]).Add(m_tsOffsetToMoscow).Hour;
                        day = ((DateTime)table.Rows[i]["DATE_PBR"]).Add(m_tsOffsetToMoscow).Day;

                        if ((hour == 0) && (day != date.Day))
                            hour = 24;
                        else
                            if (hour == 0)
                            {
                                m_curRDGValues_PBR_0 = getRDGValue_PBR_0(table.Rows[i], arIndexTables[1], arFieldsCount[1]); //(double)table.Rows[i][arIndexTables[1] * arFieldsCount[1] + (0 + 1)];

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

                        hour = iDate.Add(m_tsOffsetToMoscow).Hour;
                        //hour = hour + m_tsOffsetToMoscow;
                        day = iDate.Add(m_tsOffsetToMoscow).Day;

                        if (hour == 24)
                        {
                            hour = 0;
                            day ++;
                        }

                        if ((hour == 0) && (!(day == date.Day)))
                            hour = 24;
                        else
                            if (hour == 0)
                            {
                                if ((arIndexTables[0] * arFieldsCount[1] + 1) < table.Columns.Count)
                                    m_curRDGValues_PBR_0 = getRDGValue_PBR_0(table.Rows[i], arIndexTables[0], arFieldsCount[1]); //(double)table.Rows[i][arIndexTables[0] * arFieldsCount[1] + 1];
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

        /// <summary>
        /// ��������� �������� ��� ������ � Excel
        /// </summary>
        /// <param name="item">���������� ��������� ���������� � ������� �� ���</param>
        /// <param name="iRows">������������� ������</param>
        protected void setRDGExcelValuesItem(out RDGStruct item, int iRows)
        {
            int indx_col = 1
                , j = -1;
            item = new RDGStruct();
            double val = -1F;

            IDevice curDev = CurrentDevice;

            for (j = 0; j < curDev.ListLowPointDev.Count; j++) {
                indx_col = curDev.ListLowPointDev[j].m_indx_col_rdg_excel;
                if (indx_col > 1)
                    if (!(m_tableRDGExcelValuesResponse.Rows[iRows][indx_col - 1] is DBNull) &&
                        (double.TryParse(m_tableRDGExcelValuesResponse.Rows[iRows][indx_col - 1].ToString(), out val) == true))
                        item.pbr += val;
                    else
                        ;
                else
                    return;
            }

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

        /// <summary>
        /// ���������� �����
        /// </summary>
        protected void GetCurrentTimeRequest()
        {
            if (IsCanUseTECComponents == true)
            {
                TEC tec = CurrentDevice.tec;
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
                throw new InvalidOperationException ("AdminTS::GetCurrentTimeRequest () - ��� ����������� ���...");
        }

        /// <summary>
        /// ��������� � ���������� ������� ��� ��������� ���������������� ������
        /// </summary>
        /// <param name="date">���� �� ������� ���������� �������</param>
        protected virtual void GetAdminDatesRequest(DateTime date)
        {
            IDevice curDev;

            if (m_curDate.Date > date.Date)
            {
                date = m_curDate.Date;
            }
            else
                ;

            if (IsCanUseTECComponents == true) {
                curDev = CurrentDevice;

                //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, allTECComponents[indxTECComponents].tec.GetAdminDatesQuery(date));
                Request (m_dictIdListeners [curDev.tec.m_id] [(int)CONN_SETT_TYPE.ADMIN]
                    , curDev.tec.GetAdminDatesQuery (curDev, date.Add (-m_tsOffsetToMoscow)));
            } else
                throw new InvalidOperationException ("AdminTS::GetAdminDatesRequest () - ��� ����������� ���...");
        }

        /// <summary>
        /// ��������� � ���������� ������� ��� ��������� ���������������� ������
        /// </summary>
        /// <param name="date">���� �� ������� ���������� �������</param>
        protected override void getPPBRDatesRequest(DateTime date)
        {
            IDevice curDev;

            if (m_curDate.Date > date.Date)
            {
                date = m_curDate.Date;
            }
            else
                ;

            m_iHavePBR_Number = -1;

            if (IsCanUseTECComponents == true) {
                curDev = CurrentDevice;

                //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, allTECComponents[indxTECComponents].tec.GetPBRDatesQuery(date));
                Request (m_dictIdListeners [curDev.tec.m_id] [(int)CONN_SETT_TYPE.PBR],
                    curDev.tec.GetPBRDatesQuery (curDev, date.Add (-m_tsOffsetToMoscow)));
            } else
                throw new InvalidOperationException ("AdminTS::getPPBRDatesRequest () - ��� ����������� ���...");
        }

        /// <summary>
        /// ������� ���������������� ��������
        /// </summary>
        private void clearAdminDates()
        {
            clearDates(CONN_SETT_TYPE.ADMIN);
        }

        /// <summary>
        /// ��������� ������ �����(�������������� �����)
        /// </summary>
        /// <param name="type">��� ������(���������������� ��� ����)</param>
        /// <param name="table">������� � �������</param>
        /// <param name="date">���� �� ������� ������������ �������</param>
        /// <returns>������</returns>
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

            ////���� ��������� ��� ����� �������� ������� �� �������� �����
            //actualizeTimezone(ref table, 0);

            //0 - [DATE_TIME]([DATE]), 1 - [ID]
            for (int i = 0, hour; i < table.Rows.Count; i++)
            {
                try
                {
                    table.Rows[i][0] = Convert.ToDateTime(table.Rows[i][0].ToString()).Add(m_tsOffsetToMoscow);

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
                        if (pbr_number.Length > HAdmin.PBR_PREFIX.Length)
                        {
                            pbr_number = pbr_number.Substring(HAdmin.PBR_PREFIX.Length);

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
                catch (Exception e) {
                    Logging.Logg().Exception(e, @"AdminTS::GetDatesResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            return 0;
        }

        /// <summary>
        /// ��������� ���������������� ������(������ �����)
        /// </summary>
        /// <param name="table">������� � �������</param>
        /// <param name="date">���� �� ������� ������������ �������</param>
        /// <returns>������</returns>
        private int getAdminDatesResponse(DataTable table, DateTime date)
        {
            return GetDatesResponse(CONN_SETT_TYPE.ADMIN, table, date);
        }

        /// <summary>
        /// ��������� ������ ����(������ �����)
        /// </summary>
        /// <param name="table">������� � �������</param>
        /// <param name="date">���� �� ������� ������������ �������</param>
        /// <returns>������</returns>
        protected override int getPPBRDatesResponse(DataTable table, DateTime date)
        {
            return GetDatesResponse(CONN_SETT_TYPE.PBR, table, date);
        }

        /// <summary>
        /// ���������� ����� ����
        /// </summary>
        /// <param name="dt">��������� ����/�����</param>
        /// <param name="type">��� ������ (���, �����./��������)</param>
        /// <returns>����� ����</returns>
        protected int getCurrentHour (DateTime dt, CONN_SETT_TYPE type)
        {
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

        /// <summary>
        /// ��������� �������� ��� ����������, ���������� ���������������� ��������
        /// </summary>
        /// <param name="comp">��������� ���</param>
        /// <param name="date">����</param>
        /// <returns>������ ��������</returns>
        protected virtual string [] setAdminValuesQuery(TECComponent comp, DateTime date)
        {
            string[] resQuery = new string[(int)DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE] { string.Empty, string.Empty, string.Empty };

            date = date.Date;
            int offset = -1
                , currentHour = -1;

            currentHour = getCurrentHour(date, CONN_SETT_TYPE.ADMIN);
            //currentHour = currentHour - m_tsOffsetToMoscow;
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
                    //switch (m_typeFields)
                    //{
                    //    case AdminTS.TYPE_FIELDS.STATIC:
                    //        ;
                    //        break;
                    //    case AdminTS.TYPE_FIELDS.DYNAMIC:
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + comp.tec.m_strNameTableAdminValues/*[(int)m_typeFields]*/ +
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
                    //        break;
                    //    default:
                    //        break;
                    //}
                }
                else
                {
                    // ������ �����������, ���������� ��������
                    //switch (m_typeFields)
                    //{
                    //    case AdminTS.TYPE_FIELDS.STATIC:
                    //        resQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                    //                    @"', '" + m_curRDGValues[i].recomendation.ToString("F3", CultureInfo.InvariantCulture) +
                    //                    @"', " + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                    //                    @", '" + m_curRDGValues[i].deviation.ToString("F3", CultureInfo.InvariantCulture) +
                    //                    @"'),";
                    //        break;
                    //    case AdminTS.TYPE_FIELDS.DYNAMIC:
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours(i + 1 - offset - m_tsOffsetToMoscow.Hours).ToString("yyyyMMdd HH:mm:ss") +
                                @"', '" + m_curRDGValues[i].recomendation.ToString("F3", CultureInfo.InvariantCulture) +
                                @"', " + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                @", '" + m_curRDGValues[i].deviation.ToString("F3", CultureInfo.InvariantCulture) +
                                @"', " + (comp.m_id) +
                                @", " + (offset > 0 ? (SEASON_BASE + (int)HAdmin.seasonJumpE.WinterToSummer) : (SEASON_BASE + (int)HAdmin.seasonJumpE.SummerToWinter)) +
                                @", " + (m_curRDGValues[i].fc ? 1 : 0) +
                                @"),";
                    //        break;
                    //    default:
                    //        break;
                    //}
                }
            }

            resQuery[(int)DbTSQLInterface.QUERY_TYPE.DELETE] = string.Empty;
            //@"DELETE FROM " + t.m_strUsedAdminValues + " WHERE " +
            //@"BTEC_TG1_REC = 0 AND BTEC_TG1_IS_PER = 0 AND BTEC_TG1_DIVIAT = 0 AND " +
            //@"BTEC_TG2_REC = 0 AND BTEC_TG2_IS_PER = 0 AND BTEC_TG2_DIVIAT = 0 AND " +
            //@"BTEC_TG35_REC = 0 AND BTEC_TG35_IS_PER = 0 AND BTEC_TG35_DIVIAT = 0 AND " +
            //@"BTEC_TG4_REC = 0 AND BTEC_TG4_IS_PER = 0 AND BTEC_TG4_DIVIAT = 0 AND " +
            //@"TEC2_REC = 0 AND TEC2_IS_PER = 0 AND TEC2_DIVIAT = 0 AND " +
            //@"TEC3_TG1_REC = 0 AND TEC3_TG1_IS_PER = 0 AND TEC3_TG1_DIVIAT = 0 AND " +
            //@"TEC3_TG5_REC = 0 AND TEC3_TG5_IS_PER = 0 AND TEC3_TG5_DIVIAT = 0 AND " +
            //@"TEC3_TG712_REC = 0 AND TEC3_TG712_IS_PER = 0 AND TEC3_TG712_DIVIAT = 0 AND " +
            //@"TEC3_TG1314_REC = 0 AND TEC3_TG1314_IS_PER = 0 AND TEC3_TG1314_DIVIAT = 0 AND " +
            //@"TEC4_TG3_REC = 0 AND TEC4_TG3_IS_PER = 0 AND TEC4_TG3_DIVIAT = 0 AND " +
            //@"TEC4_TG48_REC = 0 AND TEC4_TG48_IS_PER = 0 AND TEC4_TG48_DIVIAT = 0 AND " +
            //@"TEC5_TG12_REC = 0 AND TEC5_TG12_IS_PER = 0 AND TEC5_TG12_DIVIAT = 0 AND " +
            //@"TEC5_TG36_REC = 0 AND TEC5_TG36_IS_PER = 0 AND TEC5_TG36_DIVIAT = 0 AND " +
            //@"DATE > '" + date.ToString("yyyyMMdd HH:mm:ss") +
            //@"' AND DATE <= '" + date.AddHours(1).ToString("yyyyMMdd HH:mm:ss") +
            //@"';";

            return resQuery;
        }

        #region ��������� ���� ���������� ���������������� ��������
        /// <summary>
        /// ��� �������� ������ ��� ������������� � ��������� ������
        /// </summary>
        /// <param name="nextIndex">��������� ������</param>
        /// <param name="t">������ ��� - �������� ���������� ����������-������� � ������ (���, ����������, ��� ������, ����� ��������� = null)</param>
        /// <param name="comp">���������-������ ��������� � ������</param>
        /// <param name="date">����, �� ������� ��������� ��������/��������� ��������</param>
        /// <param name="listIdRec">������ ��������������� ������� � ������� �� ��� ����������</param>
        public delegate void DelegateUnitTestSetValuesRequest(TECComponent comp, DateTime date, CONN_SETT_TYPE type, string[]queries, IEnumerable<int>listIdRec);

        private DelegateUnitTestSetValuesRequest _eventUnitTestSetValuesRequest;

        public event DelegateUnitTestSetValuesRequest EventUnitTestSetValuesRequest
        {
            add
            {
                if (Equals(_eventUnitTestSetValuesRequest, null) == true)
                    _eventUnitTestSetValuesRequest += value;
                else
                    ;
            }

            remove
            {
                if (Equals(_eventUnitTestSetValuesRequest, null) == false) {
                    _eventUnitTestSetValuesRequest -= value;
                    _eventUnitTestSetValuesRequest = null;
                } else
                    ;
            }
        }
        #endregion

        private IEnumerable<int> getHaveDates(CONN_SETT_TYPE type)
        {
            List<int> listRes;

            listRes = new List<int>();

            for (int i = 0; i < m_arHaveDates.GetLength(m_arHaveDates.Rank - 1); i++)
                listRes.Add(m_arHaveDates[(int)type, i]);

            return listRes;
        }

        /// <summary>
        /// ��������� � ���������� �������� ��� ����������, ���������� ���������������� ��������
        /// </summary>
        /// <param name="comp">��������� ���</param>
        /// <param name="date">����</param>
        protected virtual void SetAdminValuesRequest(TECComponent comp, DateTime date)
        {
            //Logging.Logg().Debug("AdminTS::SetAdminValuesRequest () - ...", Logging.INDEX_MESSAGE.NOT_SET);

            string[] query = setAdminValuesQuery(comp, date);

            // ��������� ��� ������, �� ��������� � ����
            if (! (query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] == ""))
            {
                //switch (m_typeFields)
                //{
                //    case AdminTS.TYPE_FIELDS.STATIC:
                //        ;
                //        break;
                //    case AdminTS.TYPE_FIELDS.DYNAMIC:
                        query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = $@"INSERT INTO {comp.tec.m_strNameTableAdminValues/*[(int)m_typeFields]*/} (" +
                            @"DATE " +
                            @", " + @"REC" +
                            @", " + "IS_PER" +
                            @", " + "DIVIAT" +
                            @", " + "ID_COMPONENT" +
                            @", " + "SEASON" +
                            @", " + "FC" +
                            $@") VALUES {query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Substring(0, query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Length - 1)};";
                //        break;
                //    default:
                //        break;
                //}
            }
            else
                ;

            if (!((ModeGetRDGValues & MODE_GET_RDG_VALUES.UNIT_TEST) == MODE_GET_RDG_VALUES.UNIT_TEST)) {
                Request (m_dictIdListeners [comp.tec.m_id] [(int)CONN_SETT_TYPE.ADMIN], query [(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + query [(int)DbTSQLInterface.QUERY_TYPE.INSERT] + query [(int)DbTSQLInterface.QUERY_TYPE.DELETE]);

                Logging.Logg ().Action ($@"AdminTS::SetAdminValuesRequest () - UPDATE=[{query [(int)DbTSQLInterface.QUERY_TYPE.UPDATE]}];{Environment.NewLine}"
                        + $@"INSERT=[{query [(int)DbTSQLInterface.QUERY_TYPE.INSERT]}];{Environment.NewLine}"
                        + $@"DELETE=[{query [(int)DbTSQLInterface.QUERY_TYPE.DELETE]}]"
                    , Logging.INDEX_MESSAGE.D_006);
            } else {
                Request (m_dictIdListeners [comp.tec.m_id] [(int)CONN_SETT_TYPE.ADMIN], GetCurrentTimeQuery(DbInterface.DB_TSQL_INTERFACE_TYPE.MSSQL));
                // ��������� �� ������, ��� ������������ ���������� �����
                _eventUnitTestSetValuesRequest?.Invoke (comp, date, CONN_SETT_TYPE.ADMIN, query, getHaveDates(CONN_SETT_TYPE.ADMIN));
            }
        }

        /// <summary>
        /// ��������� � ���������� ������� ��� �������� ���������������� ��������
        /// </summary>
        /// <param name="comp">��������� ���</param>
        /// <param name="date">����</param>
        protected virtual void ClearAdminValuesRequest(TECComponent comp, DateTime date)
        {
            string[] query = new string[(int)DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE] { string.Empty, string.Empty, string.Empty };
            
            int currentHour = -1;

            date = date.Date;

            if ((serverTime.Date < date) || (m_ignore_date == true))
                currentHour = 0;
            else
                currentHour = serverTime.Hour;

            for (int i = currentHour; i < 24; i++)
            {
                // ������ ��� ����� ���� �������, ������������ �
                if (IsHaveDates(CONN_SETT_TYPE.ADMIN, i) == true)
                {
                    //switch (m_typeFields)
                    //{
                    //    case AdminTS.TYPE_FIELDS.STATIC:
                    //        ;
                    //        break;
                    //    case AdminTS.TYPE_FIELDS.DYNAMIC:
                            query[(int)DbTSQLInterface.QUERY_TYPE.DELETE] += @"DELETE FROM " + comp.tec.m_strNameTableAdminValues/*[(int)m_typeFields]*/ +
                                @" WHERE " +
                                @"DATE = '" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                                @"'" +
                                @" AND ID_COMPONENT = " + comp.m_id + "; ";
                    //        break;
                    //    default:
                    //        break;
                    //}
                }
                else
                {
                }
            }

            //Logging.Logg().Debug("AdminTS::ClearAdminValuesRequest () - ...", Logging.INDEX_MESSAGE.NOT_SET);

            //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, requestUpdate + requestInsert + requestDelete);
            Request(m_dictIdListeners[comp.tec.m_id][(int)CONN_SETT_TYPE.ADMIN], query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]);
        }

        /// <summary>
        /// ��������� �������� �� ���������� � ���������� ��������� ����
        /// </summary>
        /// <param name="t">���</param>
        /// <param name="comp">��������� ���</param>
        /// <param name="date">����</param>
        /// <returns>������ ��������</returns>
        protected virtual string[] setPPBRQuery(TECComponent comp, DateTime date)
        {
            int err = -1; // ������� ������ ��� ����������� ������ ���
            string[] resQuery = new string[(int)DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE] { string.Empty, string.Empty, string.Empty };

            date = date.Date;
            int currentHour = getCurrentHour (date, CONN_SETT_TYPE.PBR);
            //currentHour = currentHour - m_tsOffsetToMoscow;

            for (int i = currentHour; i < 24; i++)
            {
                // ������ ��� ����� ���� �������, ������������ �
                if (IsHaveDates(CONN_SETT_TYPE.PBR, i) == true)
                {
                    //switch (m_typeFields)
                    //{
                    //    case AdminTS.TYPE_FIELDS.STATIC:
                    //        ;
                    //        break;
                    //    case AdminTS.TYPE_FIELDS.DYNAMIC:
                            bool bUpdate = m_ignore_date;
                            int pbr_number_calc = GetPBRNumber (i, out err)
                                , pbr_number_having = -1;

                            if (bUpdate == false)
                                if (m_iHavePBR_Number < pbr_number_calc)
                                    //����������� ������
                                    bUpdate = true;
                                else
                                    if (m_iHavePBR_Number == pbr_number_calc)
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

                            pbr_number_having = GetPBRNumber (m_curRDGValues [i].pbr_number, out err);

                            Logging.Logg().Debug(@"AdminTS::setPPBRQuery () - [ID_COMPONENT=" + comp.m_id + @"] ���=" + i + @"; ��=" + m_iHavePBR_Number + @"; �����=" + pbr_number_calc, Logging.INDEX_MESSAGE.D_001);

                            if (bUpdate == true) {
                                resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE [" + comp.tec.m_strNameTableUsedPPBRvsPBR/*[(int)m_typeFields]*/ + @"]" +
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
                    ////        break;
                    ////    default:
                    ////        break;
                    ////}
                }
                else
                {
                    string strPBRNumber = string.Empty;
                    if ((!(m_curRDGValues[i].pbr_number == null)) && (m_curRDGValues[i].pbr_number.Length > HAdmin.PBR_PREFIX.Length))
                        strPBRNumber = m_curRDGValues[i].pbr_number;
                    else
                        strPBRNumber = getNamePBRNumber();

                    // ������ �����������, ���������� ��������
                    //switch (m_typeFields)
                    //{
                    //    case AdminTS.TYPE_FIELDS.STATIC:
                    //        ;
                    //        break;
                    //    case AdminTS.TYPE_FIELDS.DYNAMIC:
                            Logging.Logg().Debug(@"AdminTS::setPPBRQuery () - [ID_COMPONENT=" + comp.m_id + @"] ���=" + i + @"; ��=" + m_iHavePBR_Number + @"; �����=" + strPBRNumber, Logging.INDEX_MESSAGE.D_001);

                            if (!(m_curRDGValues[i].pbr < 0))
                                resQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours(i + 1 - m_tsOffsetToMoscow.Hours).ToString("yyyyMMdd HH:mm:ss") +
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
                    //        break;
                    //    default:
                    //        break;
                    //}
                }
            }

            return resQuery;
        }

        public enum INDEX_SET_LOGGING { PBR_QUERY, PBR_NUMBER }

        private HMark m_markSetLogging;

        /// <summary>
        /// ��������� � ���������� �������� �� ���������� � ���������� ��������� ����
        /// </summary>
        /// <param name="t">���</param>
        /// <param name="comp">��������� ���</param>
        /// <param name="date">����</param>
        protected /*virtual*/ void SetPPBRRequest(TECComponent comp, DateTime date)
        {
            string[] query = setPPBRQuery(comp, date);

            // ��������� ��� ������, �� ��������� � ����
            if (query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Equals (string.Empty) == false)
            {
                //switch (m_typeFields)
                //{
                //    case AdminTS.TYPE_FIELDS.STATIC:
                //        ;
                //        break;
                //    case AdminTS.TYPE_FIELDS.DYNAMIC:
                        query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = @"INSERT INTO [" + comp.tec.m_strNameTableUsedPPBRvsPBR/*[(int)m_typeFields]*/ + "] (DATE_TIME, WR_DATE_TIME, PBR_NUMBER, ID_COMPONENT, OWNER, PBR, Pmin, Pmax) VALUES" + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Substring(0, query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Length - 1) + ";";
                //        break;
                //    default:
                //        break;
                //}
            }
            else
                ;

            query[(int)DbTSQLInterface.QUERY_TYPE.DELETE] = @"";
                //@"DELETE FROM " + m_strUsedPPBRvsPBR + " WHERE " +
                //@"BTEC_PBR = 0 AND BTEC_Pmax = 0 AND BTEC_Pmin = 0 AND " +
                //@"BTEC_TG1_PBR = 0 AND BTEC_TG1_Pmax = 0 AND BTEC_TG1_Pmin = 0 AND " +
                //@"BTEC_TG2_PBR = 0 AND BTEC_TG2_Pmax = 0 AND BTEC_TG2_Pmin = 0 AND " +
                //@"BTEC_TG35_PBR = 0 AND BTEC_TG35_Pmax = 0 AND BTEC_TG35_Pmin = 0 AND " +
                //@"BTEC_TG4_PBR = 0 AND BTEC_TG4_Pmax = 0 AND BTEC_TG4_Pmin = 0 AND " +
                //@"TEC2_PBR = 0 AND TEC2_Pmax = 0 AND TEC2_Pmin = 0 AND " +
                //@"TEC3_PBR = 0 AND TEC3_TG1_Pmax = 0 AND TEC3_TG1_Pmin = 0 AND " +
                //@"TEC3_TG1_PBR = 0 AND TEC3_TG1_Pmax = 0 AND TEC3_TG1_Pmin = 0 AND " +
                //@"TEC3_TG5_PBR = 0 AND TEC3_TG5_Pmax = 0 AND TEC3_TG5_Pmin = 0 AND " +
                //@"TEC3_TG712_PBR = 0 AND TEC3_TG712_Pmax = 0 AND TEC3_TG712_Pmin = 0 AND " +
                //@"TEC3_TG1314_PBR = 0 AND TEC3_TG1314_Pmax = 0 AND TEC3_TG1314_Pmin = 0 AND " +
                //@"TEC4_PBR = 0 AND TEC4_TG3_Pmax = 0 AND TEC4_TG3_Pmin = 0 AND " +
                //@"TEC4_TG3_PBR = 0 AND TEC4_TG3_Pmax = 0 AND TEC4_TG3_Pmin = 0 AND " +
                //@"TEC4_TG48_PBR = 0 AND TEC4_TG48_Pmax = 0 AND TEC4_TG48_Pmin = 0 AND " +
                //@"TEC5_PBR = 0 AND TEC5_TG12_Pmax = 0 AND TEC5_TG12_Pmin = 0 AND " +
                //@"TEC5_TG12_PBR = 0 AND TEC5_TG12_Pmax = 0 AND TEC5_TG12_Pmin = 0 AND " +
                //@"TEC5_TG36_PBR = 0 AND TEC5_TG36_Pmax = 0 AND TEC5_TG36_Pmin = 0 AND " +
                //@"DATE_TIME > '" + date.ToString("yyyyMMdd HH:mm:ss") +
                //@"' AND DATE_TIME <= '" + date.AddHours(1).ToString("yyyyMMdd HH:mm:ss") +
                //@"';";

            if ((query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE].Equals(string.Empty) == false) ||
                (query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Equals(string.Empty) == false)) {
                Logging.Logg().Debug("AdminTS::SetPPBRRequest ()", Logging.INDEX_MESSAGE.D_002);

                //Logging.Logg().Debug(@"AdminTS::setPPBRQuery () - INSERT: " + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT], Logging.INDEX_MESSAGE.D_005);
                //Logging.Logg().Debug(@"AdminTS::setPPBRQuery () - UPDATE: " + query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE], Logging.INDEX_MESSAGE.D_005);
                //Logging.Logg().Debug(@"AdminTS::setPPBRQuery () - DELETE: " + resQuery[(int)DbTSQLInterface.QUERY_TYPE.DELETE], Logging.INDEX_MESSAGE.D_005);

                Request(m_dictIdListeners[comp.tec.m_id][(int)CONN_SETT_TYPE.PBR]
                    , query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE]
                        + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT]
                        + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]
                );
            }
            else
                //Logging.Logg().Debug("AdminTS::SetPPBRRequest () - Empty", Logging.INDEX_MESSAGE.NOT_SET) //������ ����
                ;
        }

        /// <summary>
        /// ��������� � ���������� ������� �� �������� ��������� ����
        /// </summary>
        /// <param name="t">���</param>
        /// <param name="comp">��������� ���</param>
        /// <param name="date">����</param>
        protected virtual void ClearPPBRRequest(TECComponent comp, DateTime date)
        {
            string[] query = new string[(int)DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE] { string.Empty, string.Empty, string.Empty };
            
            int currentHour = -1;

            date = date.Date;

            if ((serverTime.Date < date) || (m_ignore_date == true))
                currentHour = 0;
            else
                currentHour = serverTime.Hour;

            for (int i = currentHour; i < 24; i++)
            {
                // ������ ��� ����� ���� �������, ������������ �
                if (IsHaveDates(CONN_SETT_TYPE.PBR, i) == true)
                {
                    //switch (m_typeFields)
                    //{
                    //    case AdminTS.TYPE_FIELDS.STATIC:
                    //        ;
                    //        break;
                    //    case AdminTS.TYPE_FIELDS.DYNAMIC:
                            query[(int)DbTSQLInterface.QUERY_TYPE.DELETE] += @"DELETE FROM [" + comp.tec.m_strNameTableUsedPPBRvsPBR/*[(int)m_typeFields]*/ + @"]" +
                                @" WHERE " +
                                @"DATE_TIME" + @" = '" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                                @"'" +
                                @" AND ID_COMPONENT = " + comp.m_id + "; ";
                    //        break;
                    //    default:
                    //        break;
                    //}
                }
                else
                {
                }
            }

            //Logging.Logg().Debug("ClearPPBRRequest () - ...", Logging.INDEX_MESSAGE.NOT_SET);

            //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, requestUpdate + requestInsert + requestDelete);
            Request(m_dictIdListeners[comp.tec.m_id][(int)CONN_SETT_TYPE.PBR], query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]);
        }

        ///// <summary>
        ///// ��������� ������� ���������� ���
        ///// </summary>
        ///// <param name="idTEC">�� ���</param>
        ///// <param name="idComp">�� ����������</param>
        ///// <returns>���������� ������</returns>
        //public int GetIndexTECComponent (int idTEC, int idComp) {
        //    int iRes = -1;

        //    foreach (TECComponent comp in allTECComponents)
        //    {
        //        if ((comp.tec.m_id == idTEC) && (comp.m_id == idComp)) {
        //            iRes = allTECComponents.IndexOf (comp);
        //            break;
        //        }
        //        else
        //            ;
        //    }

        //    return iRes;
        //}

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

        /// <summary>
        /// ����� ���������� ��
        /// </summary>
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

        /// <summary>
        /// ��������� ���������� ��
        /// </summary>
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

        /// <summary>
        /// ����������� ������� �� ��������� � ���������� ��������
        /// </summary>
        /// <param name="state">������������� ��������</param>
        /// <returns>������</returns>
        protected override int StateRequest(int /*StatesMachine*/ state)
        {
            int result = 0;

            string strRep = string.Empty;
            StatesMachine stateMachine = (StatesMachine)state;
            IDevice comp;

            comp = CurrentDevice;

            switch (stateMachine)
            {
                case StatesMachine.CurrentTime:
                    strRep = @"��������� �������� ������� �������.";
                    GetCurrentTimeRequest();
                    break;
                case StatesMachine.PPBRValues:
                    strRep = @"��������� ������ �����.";
                    if (CurrentKey.Id > 0)
                        getPPBRValuesRequest(comp.tec, comp as TECComponent
                            , m_curDate.Date.Add(-m_tsOffsetToMoscow)/*, m_typeFields*/);
                    else
                        ; //result = false;
                    break;
                case StatesMachine.AdminValues:
                    strRep = @"��������� ���������������� ������.";
                    if (m_markQueries.IsMarked ((int)CONN_SETT_TYPE.ADMIN) == true)
                        getAdminValuesRequest(comp.tec, comp as TECComponent
                            , m_curDate.Date.Add(-m_tsOffsetToMoscow)/*, m_typeFields*/);
                    else
                        ;
                    break;
                #region ������/������� ��������
                case StatesMachine.ImpRDGExcelValues:
                    strRep = @"������ ��� �� Excel.";
                    delegateImportForeignValuesRequuest();
                    break;
                case StatesMachine.ExpRDGExcelValues:
                    strRep = @"������� ��� � ����� Excel.";
                    delegateExportForeignValuesRequuest();
                    break;
                 case StatesMachine.CSVValues:
                    strRep = @"������ �� ������� CSV.";
                    delegateImportForeignValuesRequuest();
                    break;
                #endregion
                case StatesMachine.PPBRDates:
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
                    strRep = @"��������� ������ ���������� ������� ��������.";
                    getPPBRDatesRequest(m_curDate);
                    break;
                case StatesMachine.AdminDates:
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
                    strRep = @"��������� ������ ���������� ������� ��������.";
                    if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true)
                        GetAdminDatesRequest(m_curDate);
                    else
                        ;
                    break;
                case StatesMachine.SaveAdminValues:
                    strRep = @"���������� ���������������� ������.";
                    if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true)
                        SetAdminValuesRequest(comp as TECComponent, m_curDate);
                    else
                        ; //result = false;
                    break;
                case StatesMachine.SavePPBRValues:
                    strRep = @"���������� �����.";
                    SetPPBRRequest(comp as TECComponent, m_curDate);
                    break;
                //case StatesMachine.LayoutGet:
                //    ActionReport("��������� ���������������� ������ ������.");
                //    GetLayoutRequest(m_curDate);
                //    break;
                //case StatesMachine.LayoutSet:
                //    ActionReport("���������� ���������������� ������ ������.");
                //    SetLayoutRequest(m_curDate);
                //    break;
                case StatesMachine.ClearAdminValues:
                    strRep = @"���������� ���������������� ������.";
                    ClearAdminValuesRequest (comp as TECComponent, m_curDate);
                    break;
                case StatesMachine.ClearPPBRValues:
                    strRep = @"���������� �����.";
                    ClearPPBRRequest(comp as TECComponent, m_curDate);
                    break;
                default:
                    break;
            }

            //Logging.Logg().Debug(@"AdminTS::StateRequest () - state=" + state.ToString() + @" - �����...");

            return result;
        }

        /// <summary>
        /// ����������� ������� �� ��������� ������ �� ����������� ��������
        /// </summary>
        /// <param name="state">������������� ��������</param>
        /// <param name="error">������� ������</param>
        /// <param name="table">������� � ����������� �������</param>
        /// <returns>������</returns>
        protected override int StateCheckResponse(int /*StatesMachine*/ state, out bool error, out object table)
        {
            int iRes = -1;

            error = true;
            table = null;

            StatesMachine stateMachine = (StatesMachine)state;

            if (((state == (int)StatesMachine.ImpRDGExcelValues) || (state == (int)StatesMachine.ExpRDGExcelValues)) ||
                (state == (int)StatesMachine.CSVValues) ||
                /*((!(m_indxDbInterfaceCurrent < 0)) && (m_listListenerIdCurrent.Count > 0))*/
                (!(m_IdListenerCurrent < 0)))
            {
                switch (stateMachine)
                {
                    case StatesMachine.ImpRDGExcelValues:
                        if ((!(m_tableRDGExcelValuesResponse == null)) && (m_tableRDGExcelValuesResponse.Rows.Count > 24))
                        {
                            error = false;

                            //??? ����� �� ������...
                            iRes = 0;
                        }
                        else
                            ;
                        break;
                    case StatesMachine.ExpRDGExcelValues:
                            //??? ������ ����� ???
                            error = false;
                            iRes = 0;
                        break;
                     case StatesMachine.CSVValues:
                        if ((!(m_tableValuesResponse == null)) && (m_tableValuesResponse.Rows.Count > 0))
                        {
                            error = false;

                            iRes = 0;
                        }
                        else
                            ;
                        break;
                    case StatesMachine.AdminDates:
                        if (m_markQueries.IsMarked ((int)CONN_SETT_TYPE.ADMIN) == true)
                            iRes = response(m_IdListenerCurrent, out error, out table/*, false*/);
                        else {
                            error = false;
                            table = null;

                            iRes = 0;
                        }
                        break;
                    case StatesMachine.CurrentTime:
                    case StatesMachine.PPBRValues:
                    case StatesMachine.AdminValues:
                    case StatesMachine.PPBRDates:
                    case StatesMachine.SaveAdminValues:
                    case StatesMachine.SavePPBRValues:
                    //case StatesMachine.UpdateValuesPPBR:
                    case StatesMachine.ClearAdminValues:
                    case StatesMachine.ClearPPBRValues:
                    //case StatesMachine.GetPass:
                        iRes = response(m_IdListenerCurrent, out error, out table/*, false*/);
                        break;
                    //case StatesMachine.LayoutGet:
                    //case StatesMachine.LayoutSet:
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

        /// <summary>
        /// ����������� ������� �� ��������� ������
        /// </summary>
        /// <param name="state">������������� ��������</param>
        /// <param name="table">������� � ����������� �������</param>
        /// <returns>������</returns>
        protected override int StateResponse(int /*StatesMachine*/ state, object table)
        {
            int result = -1;

            string strRep = string.Empty;
            StatesMachine stateMachine = (StatesMachine)state;

            switch (stateMachine)
            {
                case StatesMachine.CurrentTime:
                    result = GetCurrentTimeResponse(table as DataTable);
                    if (result == 0)
                    {
                        if (using_date == true) {
                            m_prevDate = serverTime.Date;
                            m_curDate = m_prevDate;

                            setDatetime?.Invoke(m_curDate);
                        }
                        else
                            ;
                    }
                    else
                        ;
                    break;
                case StatesMachine.PPBRValues:
                    result = getPPBRValuesResponse(table as DataTable, m_curDate);
                    if (result == 0)
                    {
                        if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == false)
                        {
                            result = GetAdminValuesResponse(null, m_curDate);

                            readyData (m_prevDate, result == 0);
                        }
                        else
                            ;
                    }
                    else
                        ;
                    break;
                case StatesMachine.AdminValues:
                    if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true)
                    {
                        result = GetAdminValuesResponse(table as System.Data.DataTable, m_curDate);
                    }
                    else
                    {
                        table = null;

                        if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.PBR) == true)
                            result = GetAdminValuesResponse(null, m_curDate);
                        else
                            result = -1;
                    }

                    readyData(m_prevDate, result == 0);
                    break;
                #region ������/������� ��������
                case StatesMachine.ImpRDGExcelValues:
                    ActionReport("������ ��� �� Excel.");
                    //result = GetRDGExcelValuesResponse(table, m_curDate);
                    result = delegateImportForeignValuesResponse();

                    readyData (m_prevDate, result == 0);
                    break;
                case StatesMachine.ExpRDGExcelValues:
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
                case StatesMachine.CSVValues:
                    ActionReport("������ �������� �� ������� CSV.");
                    //result = GetRDGExcelValuesResponse(table, m_curDate);
                    result = delegateImportForeignValuesResponse();

                    readyData(m_prevDate, result == 0);
                    break;
                #endregion
                case StatesMachine.PPBRDates:
                    clearPPBRDates();
                    result = getPPBRDatesResponse(table as System.Data.DataTable, m_curDate);
                    if (result == 0)
                    {
                    }
                    else
                        ;
                    break;
                case StatesMachine.AdminDates:
                    clearAdminDates();
                    if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true)
                        result = getAdminDatesResponse(table as System.Data.DataTable, m_curDate);
                    else
                        result = 0;

                    if (result == 0)
                    {
                    }
                    else
                        ;
                    break;
                case StatesMachine.SaveAdminValues:
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
                case StatesMachine.SavePPBRValues:
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
                //case StatesMachine.LayoutGet:
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
                //case StatesMachine.LayoutSet:
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
                case StatesMachine.ClearAdminValues:
                    result = 0;
                    if (result == 0) { }
                    else ;
                    break;
                case StatesMachine.ClearPPBRValues:
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

            StatesMachine stateMachine = (StatesMachine)state;

            switch (stateMachine)
            {
                case StatesMachine.CurrentTime:
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
                case StatesMachine.PPBRValues:
                    if (request == 0)
                        reason = @"�������";
                    else {
                        reason = @"���������";
                        bClear = true;
                    }

                    reason += @" ������ �����";
                    waiting = @"������� � ��������";

                    break;
                case StatesMachine.AdminValues:
                    if (request == 0)
                        reason = @"�������";
                    else {
                        reason = @"���������";
                        bClear = true;
                    }

                    reason += @" ���������������� ������";
                    waiting = @"������� � ��������";

                    break;
                case StatesMachine.ImpRDGExcelValues:
                    reason = @"������� ��� �� ����� Excel";
                    waiting = @"������� � ��������";

                    // ???
                    break;
                case StatesMachine.ExpRDGExcelValues:
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
                case StatesMachine.CSVValues:
                    reason = @"������� �� ������� CSV";
                    waiting = @"������� � ��������";

                    // ???
                    break;
                case StatesMachine.PPBRDates:
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

                    reason += @" ���������� ������� �������� (PPBR)";
                    waiting = @"������� � ��������";

                    break;
                case StatesMachine.AdminDates:
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

                    reason += @" ���������� ������� �������� (AdminValues)";
                    waiting = @"������� � ��������";

                    break;
                case StatesMachine.SaveAdminValues:
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
                case StatesMachine.SavePPBRValues:
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
                //case StatesMachine.LayoutGet:
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
                //case StatesMachine.LayoutSet:
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
                case StatesMachine.ClearAdminValues:
                    reason = @"�������� ���������������� ������";
                    waiting = @"������� � ��������";
                    break;
                case StatesMachine.ClearPPBRValues:
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

            errorData?.Invoke ();

            Logging.Logg().Error(@"AdminTS::StateErrors () - error=" + error + @" - �����...", Logging.INDEX_MESSAGE.NOT_SET);

            return reasonRes;
        }

        protected override void StateWarnings(int /*StatesMachine*/ state, int req, int res)
        {
        }

        /// <summary>
        /// ��������� ���-��������
        /// </summary>
        /// <param name="indx">������ �������� � ���������� ������, �������� ��� �������� ��������� ���������</param>
        /// <param name="date">����, �������� �� ������� ��������� ���������</param>
        /// <param name="bCallback">������� ������������� ��������� � ���������� ���������� ������</param>
        public virtual void SaveRDGValues(FormChangeMode.KeyDevice key, DateTime date, bool bCallback)
        {
            lock (m_lockState) //???
            {
                CurrentKey = key;
                m_prevDate = date.Date;
            }

            Errors resultSaving = SaveChanges();
            if (resultSaving == Errors.NoError)
            {
                if (bCallback == true)
                //??? �� ���� 'GetRDGValues' ������ 'SaveRDGValues'
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
                        else ;
                        if (m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true)
                            AddState((int)StatesMachine.AdminValues);
                        else ;

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

        /// <summary>
        /// �������� ���������������� � ���� ��������
        /// </summary>
        /// <param name="date">����</param>
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

        /// <summary>
        /// ��������� �������������� ����������
        /// </summary>
        /// <param name="ownerMode">����� �������-���������</param>
        /// <param name="indx">������ ����������</param>
        /// <returns>������������� ����������</returns>
        public int GetIdOwnerTECComponent(FormChangeMode.MODE_TECCOMPONENT ownerMode, FormChangeMode.KeyDevice key)
        {
            int id_tec = -1;

            id_tec = allTECComponents.Find(comp => comp.m_id == key.Id).tec.m_id;

            foreach (TECComponent comp in allTECComponents)
            {
                if ((comp.tec.m_id == id_tec)
                    && (comp.Mode == ownerMode))
                {
                    foreach (TECComponentBase tc in comp.ListLowPointDev)
                    {
                        if (tc.m_id == key.Id)
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

            return -1;
        }

        /// <summary>
        /// ��������� �� ������������� ��
        /// </summary>
        /// <param name="indx">������ ����������</param>
        /// <returns>�� ����������</returns>
        public int GetIdPCOwnerTECComponent(FormChangeMode.KeyDevice key)
        {
            return GetIdOwnerTECComponent(FormChangeMode.MODE_TECCOMPONENT.PC, key);
        }

        /// <summary>
        /// ��������� �� ������������� ���
        /// </summary>
        /// <param name="indx">������ ����������</param>
        /// <returns>�� ����������</returns>
        public int GetIdGTPOwnerTECComponent(FormChangeMode.KeyDevice key)
        {
            return GetIdOwnerTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP, key);
        }

        private int getIndexTECComponent(FormChangeMode.KeyDevice key)
        {
            int iRes = -1;

            try {
                iRes = allTECComponents.IndexOf (allTECComponents.Find (comp => comp.m_id == key.Id));
            } catch (Exception e) {
                Logging.Logg ().Exception (e, $@"AdminTS::getIndexTECComponent (key={key.Id}) - ��������� �� ������...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            return iRes;
        }

        private int currentIndexTECComponent
        {
            get
            {
                return getIndexTECComponent(CurrentKey);
            }
        }

        /// <summary>
        /// ������������� ���������� ���
        /// </summary>
        /// <param name="indx">������ � ������� '��� ����������'</param>
        /// <returns>�������������</returns>
        public int GetIdTECComponent(int indx = -1)
        {
            int iRes = -1;

            if (indx < 0)
                indx = currentIndexTECComponent;
            else ;

            if ((!(indx < 0))
                && (indx < allTECComponents.Count))
                iRes = allTECComponents[indx].m_id;
            else ;

            return iRes;
        }

        ///// <summary>
        ///// ������������ (�������) ���������� ���
        ///// </summary>
        ///// <param name="indx">������ � ������� '��� ����������'</param>
        ///// <returns>������������</returns>
        //public string GetNameTECComponent(int indx = -1)
        //{
        //    string strRes = string.Empty;

        //    if (indx < 0) {
        //        indx = currentIndexTECComponent;
        //    } else ;

        //    if ((!(indx < 0))
        //        && (indx < allTECComponents.Count))
        //        strRes = allTECComponents[indx].name_shr;
        //    else ;

        //    return strRes;
        //}

        /// <summary>
        /// ������������ (�������) ���������� ���
        /// </summary>
        /// <param name="key">���� ����������</param>
        /// <param name="bWithNameTECOwner">������� ��������� � ������������ ������������ ������������ ���</param>
        /// <returns>������������</returns>
        public string GetNameTECComponent (FormChangeMode.KeyDevice key, bool bWithNameTECOwner)
        {
            string strRes = string.Empty;

            IDevice dev;

            if (allTECComponents.Count > 0) {
                dev = FindTECComponent (key);

                if (key.Mode == FormChangeMode.MODE_TECCOMPONENT.TEC)
                    strRes = dev.name_shr;
                else
                    strRes = bWithNameTECOwner == true
                        ? string.Format ("{0} - {1}", dev.tec.name_shr, dev.name_shr)
                            : dev.name_shr;
            } else
                ;

            return strRes;
        }

        /// <summary>
        /// ���������� ������� �������� (��� + ������������ = ���) ��� ������������ ���������
        /// </summary>
        public override void CopyCurToPrevRDGValues()
        {
            base.CopyCurToPrevRDGValues ();

            for (int i = 0; i < m_curRDGValues.Length; i++) {
                m_prevRDGValues[i].From(m_curRDGValues[i]);
                m_prevRDGValues[i].pbr = Math.Round(float.Parse(m_prevRDGValues[i].pbr.ToString("F2")), 2);
                m_prevRDGValues[i].pmin = Math.Round (float.Parse (m_prevRDGValues [i].pmin.ToString ("F2")), 2);
                m_prevRDGValues [i].pmax = Math.Round (float.Parse (m_prevRDGValues [i].pmax.ToString ("F2")), 2);
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
        /// <summary>
        /// ������� ���������������� � ���� �������� �������� ����������
        /// </summary>
        public override void ClearValues()
        {
            //base.ClearValues (cnt);
            base.ClearValues();

            m_curRDGValues_PBR_0 = 0F;

            for (int i = 0; i < m_curRDGValues.Length; i++)
            {
                m_curRDGValues[i].pbr =
                m_curRDGValues[i].pmin =
                m_curRDGValues[i].pmax = 
                m_curRDGValues[i].recomendation =
                m_curRDGValues[i].deviation =
                    0;
                m_curRDGValues[i].deviationPercent = false;

                m_curRDGValues[i].pbr_number = string.Empty;
                m_curRDGValues[i].dtRecUpdate = DateTime.MinValue;
            }

            //CopyCurToPrevRDGValues();
        }

        /// <summary>
        /// �������� �� ������� ���������
        /// </summary>
        /// <returns>������� ���������</returns>
        public override bool WasChanged()
        {
            bool bRes = false;

            for (int i = 0; (i < m_curRDGValues.Length) && (bRes == false); i++)
                bRes = !(m_prevRDGValues [i] == m_curRDGValues [i]);

            return bRes;
        }

        /// <summary>
        /// ���������� ��������� ��� � ��������� � ��������� ���
        ///  , ������ ��� 'trans_mc_cmd'
        /// </summary>
        /// <param name="id_mc">������������� ��</param>
        /// <returns>��������� ���</returns>
        public TECComponent GetTECComponentOfIdMC (int id_mc)
        {
            return allTECComponents.Find (comp => !(comp.m_listMCentreId.IndexOf (id_mc) < 0));
        }
    }
}
