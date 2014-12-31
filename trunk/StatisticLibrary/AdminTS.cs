using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
//using System.Data.SqlClient;
using System.Data.OleDb;
using System.IO;
//using MySql.Data.MySqlClient;
using System.Threading;
using System.Globalization;

using HClassLibrary;

namespace StatisticCommon
{    
    public class AdminTS : HAdmin
    {
        public enum TYPE_FIELDS : uint {STATIC, DYNAMIC, COUNT_TYPE_FIELDS};

        public AdminTS.TYPE_FIELDS m_typeFields;

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
        protected delegate bool DelegateFuncBool();
        protected DelegateFuncBool delegateImportForeignValuesResponse,
                                    delegateExportForeignValuesResponse;

        protected DataTable m_tablePPBRValuesResponse,
                    m_tableRDGExcelValuesResponse;

        protected enum StatesMachine
        {
            CurrentTime,
            AdminValues, //Получение административных данных
            PPBRValues,
            AdminDates, //Получение списка сохранённых часовых значений
            PPBRDates,
            ImpRDGExcelValues,
            ExpRDGExcelValues,
            SaveAdminValues, //Сохранение административных данных
            SavePPBRValues, //Сохранение PPBR
            SaveRDGExcelValues,
            PPBRCSVValues,
            //UpdateValuesPPBR, //Обновление PPBR после 'SaveValuesPPBR'
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
        protected HMark m_MarkSavedValues;

        public AdminTS(bool[] arMarkSavePPBRValues)
            : base()
        {
            m_MarkSavedValues = new HMark();

            if (!(arMarkSavePPBRValues == null))
            {
                if ((arMarkSavePPBRValues.Length > (int)INDEX_MARK_PPBRVALUES.PBR_ENABLED) && (arMarkSavePPBRValues[(int)INDEX_MARK_PPBRVALUES.PBR_ENABLED] == true))
                    m_MarkSavedValues.Marked((int)INDEX_MARK_PPBRVALUES.PBR_ENABLED);
                else ;

                if ((arMarkSavePPBRValues.Length > (int)INDEX_MARK_PPBRVALUES.PBR_AVALIABLE) && (arMarkSavePPBRValues[(int)INDEX_MARK_PPBRVALUES.PBR_AVALIABLE] == true))
                    m_MarkSavedValues.Marked((int)INDEX_MARK_PPBRVALUES.PBR_AVALIABLE);
                else ;
            }
            else
                ;

            m_MarkSavedValues.Marked((int)INDEX_MARK_PPBRVALUES.ADMIN_ENABLED);
            m_MarkSavedValues.Marked((int)INDEX_MARK_PPBRVALUES.ADMIN_AVALIABLE);
        }

        protected override void Initialize () {
            base.Initialize ();
        }

        public virtual Errors SaveChanges()
        {
            Logging.Logg().Debug("AdminTS::SaveChanges () - вХод...");

            delegateStartWait();

            Logging.Logg().Debug("AdminTS::SaveChanges () - delegateStartWait() - Интервал ожидания для semaDBAccess=" + DbInterface.MAX_WATING);

            //if (semaDBAccess.WaitOne(6666) == true) {
            if (semaDBAccess.WaitOne(DbInterface.MAX_WATING) == true)
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

                    Logging.Logg().Debug("AdminTS::SaveChanges () - states.Clear()");

                    states.Add((int)StatesMachine.CurrentTime);

                    if (m_MarkSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.ADMIN_AVALIABLE) == true)
                    {
                        states.Add((int)StatesMachine.AdminDates);
                        states.Add((int)StatesMachine.SaveAdminValues);
                    }
                    else ;

                    if (m_MarkSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.PBR_AVALIABLE) == true)
                    {
                        states.Add((int)StatesMachine.PPBRDates);
                        states.Add((int)StatesMachine.SavePPBRValues);
                    }
                    else ;

                    try
                    {
                        semaState.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "AdminTS::SaveChanges () - semaState.Release(1)");
                    }
                }

                Logging.Logg().Debug("AdminTS::SaveChanges () - semaDBAccess.WaitOne()=" + semaDBAccess.WaitOne(DbInterface.MAX_WATING).ToString());

                try
                {
                    semaDBAccess.Release(1);
                }
                catch
                {
                }

                saving = false;

                if (! (saveComplete == null)) saveComplete(); else ;
            }
            else {
                Logging.Logg().Debug("AdminTS::SaveChanges () - semaDBAccess.WaitOne()=false");

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

            Logging.Logg().Debug("AdminTS::ClearRDG () - delegateStartWait() - Интервал ожидания для semaDBAccess=" + DbInterface.MAX_WATING);

            //if (semaDBAccess.WaitOne(6666) == true) {
            if (semaDBAccess.WaitOne(DbInterface.MAX_WATING) == true)
            {
                lock (m_lockState)
                {
                    ClearStates ();

                    errClearResult = Errors.NoError;
                    using_date = false;
                    m_curDate = m_prevDate;

                    Logging.Logg().Debug("AdminTS::ClearRDG () - states.Clear()");

                    states.Add((int)StatesMachine.CurrentTime);
                    states.Add((int)StatesMachine.AdminDates);
                    //??? Состояния позволяют НАЧать процесс разработки возможности редактирования ПЛАНа на вкладке 'Редактирование ПБР'
                    states.Add((int)StatesMachine.PPBRDates);
                    states.Add((int)StatesMachine.ClearAdminValues);
                    states.Add((int)StatesMachine.ClearPPBRValues);
                    //states.Add((int)StatesMachine.UpdateValuesPPBR);

                    try
                    {
                        semaState.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "AdminTS::ClearRDG () - semaState.Release(1)");
                    }
                }

                //Ожидание окончания записи
                Logging.Logg().Debug("AdminTS::ClearRDG () - semaDBAccess.WaitOne()=" + semaDBAccess.WaitOne(DbInterface.MAX_WATING).ToString ());

                try
                {
                    semaDBAccess.Release(1);
                }
                catch
                {
                }
            }
            else {
                Logging.Logg().Debug("AdminTS::ClearRDG () - semaDBAccess.WaitOne()=false");

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

        public override void Activate(bool active)
        {
            base.Activate (active);

            if ((active == true) && (threadIsWorking == 1))
                GetRDGValues (m_typeFields, indxTECComponents);
            else
                ;
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
            if (! (m_bIsActive == true))
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

                states.Add((int)StatesMachine.CurrentTime);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, "AdminTS::Reinit () - semaState.Release(1)");
                }
            }
        }

        public void GetCurrentTime(int indx)
        {
            lock (m_lockState)
            {
                ClearStates();

                indxTECComponents = indx;

                Logging.Logg().Debug("AdminTS::GetCurrentTime () - states.Clear()");

                states.Add((int)StatesMachine.CurrentTime);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, "AdminTS::GetCurrentTime () - semaState.Release(1)");
                }
            }
        }

        /// <summary>
        /// Запретить запись ПБР-значений
        /// </summary>
        private void protectSavedPPBRValues()
        {
            if (m_MarkSavedValues.IsMarked((int)INDEX_MARK_PPBRVALUES.PBR_ENABLED) == true) m_MarkSavedValues.UnMarked((int)INDEX_MARK_PPBRVALUES.PBR_AVALIABLE); else ;
        }
        
        public virtual void GetRDGValues (TYPE_FIELDS mode, int indx) {
            //Запретить запись ПБР-значений
            protectSavedPPBRValues();

            lock (m_lockState)
            {
                ClearStates();

                indxTECComponents = indx;
                
                ClearValues();

                using_date = true;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_typeFields = mode;

                states.Add((int)StatesMachine.CurrentTime);
                states.Add((int)StatesMachine.PPBRValues);
                states.Add((int)StatesMachine.AdminValues);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, "AdminTS::GetRDGValues () - semaState.Release(1)");
                }
            }
        }

        public override void GetRDGValues(int /*TYPE_FIELDS*/ mode, int indx, DateTime date)
        {
            //Запретить запись ПБР-значений
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

                m_typeFields = (TYPE_FIELDS)mode;

                states.Add((int)StatesMachine.PPBRValues);
                states.Add((int)StatesMachine.AdminValues);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, "AdminTS::GetRDGValues () - semaState.Release(1)");
                }
            }
        }

        protected override void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode)
        {
            Request(m_dictIdListeners [t.m_id][(int)CONN_SETT_TYPE.PBR], t.GetPBRValueQuery(comp, date, mode));
        }

        private void GetAdminValuesRequest(TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode) {
            Request(m_dictIdListeners[t.m_id][(int)CONN_SETT_TYPE.ADMIN], t.GetAdminValueQuery(comp, date, mode));
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

                states.Add((int)StatesMachine.ImpRDGExcelValues);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, "AdminTS::ImpRDGExcelValues () - semaState.Release(1)");
                }
            }
        }

        public virtual Errors ExpRDGExcelValues(int indx, DateTime date)
        {
            delegateStartWait();
            Logging.Logg().Debug("AdminTS::ExpRDGExcelValues () - delegateStartWait() - Интервал ожидания для semaDBAccess=" + DbInterface.MAX_WATING);

            //if (semaDBAccess.WaitOne(6666) == true) {
            if (semaDBAccess.WaitOne(DbInterface.MAX_WATING) == true)
            {
                lock (m_lockState)
                {
                    ClearStates();

                    indxTECComponents = indx;

                    saveResult = Errors.NoAccess;
                    saving = true;
                    using_date = false;
                    m_curDate = m_prevDate;

                    states.Add((int)StatesMachine.ExpRDGExcelValues);

                    try
                    {
                        semaState.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "AdminTS::ExpRDGExcelValues () - semaState.Release(1)");
                    }
                }

                Logging.Logg().Debug("AdminTS::ExpRDGExcelValues () - semaDBAccess.WaitOne()=" + semaDBAccess.WaitOne(DbInterface.MAX_WATING).ToString());
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
                    Logging.Logg().Debug("AdminTS::ExpRDGExcelValues () - semaDBAccess.WaitOne()=false");
                    
                    saveResult = Errors.NoAccess;
                    saving = true;
                }
            }

            delegateStopWait();

            return saveResult;
        }

        protected override bool GetPPBRValuesResponse(DataTable table, DateTime date)
        {
            bool bRes = true;

            m_tablePPBRValuesResponse = table.Copy ();

            return bRes;
        }

        protected bool GetAdminValuesResponseWithoutAdminValues(DateTime date)
        {
            bool bRes = false;

            return bRes;
        }

        protected virtual bool GetAdminValuesResponse(DataTable tableAdminValuesResponse, DateTime date)
        {
            DataTable table = null;
            int i = -1, j = -1, k = -1,
                hour = -1, day = -1;
            //Массив индексов таблиц, 1-ый эл-т таблицы - индекс таблицы с БОЛЬШИМ кол-м строк
            //1-ый эл-т таблицы - индекс таблицы с МЕНЬШИМ кол-м строк
            //если кол-во строк РАВНЫ, то 1-ый эл-т индекс ППБР, 2-ой АДМИН_ВАЛ
            int [] arIndexTables = {0, 1},
                arFieldsCount = {-1, -1};
            bool bSeason = false;

            if (tableAdminValuesResponse == null) {
                tableAdminValuesResponse = new DataTable ();

                //for (i = 0; i < m_tablePPBRValuesResponse.Rows.Count; i ++)
                //    tableAdminValuesResponse.Rows.Add (new object [] {});
            } else { }

            DataTable[] arTable = { m_tablePPBRValuesResponse, tableAdminValuesResponse };

            //int offsetPBR_NUMBER = m_tablePPBRValuesResponse.Columns.IndexOf ("PBR_NUMBER");
            //if (offsetPBR_NUMBER > 0) offsetPBR_NUMBER = 0; else ;

            int offsetPBR = m_tablePPBRValuesResponse.Columns.IndexOf("PBR")
                , offsetPBRNumber = -1
                , offsetDATE_ADMIN = -1;
            if (offsetPBR > 0) offsetPBR = 0; else ;

            //Определить признак даты переходы сезонов (заранее, не при итерации в цикле) - копия 'TecView'
            if (HAdmin.SeasonDateTime.Date.CompareTo (date.Date) == 0)
                bSeason = true;
            else
                ;

            //Удаление столбцов 'ID_COMPONENT'
            for (i = 0; i < arTable.Length; i++) {
                /*
                for (j = 0; j < arTable[i].Columns.Count; j++)
                {
                    if (arTable[i].Columns [j].ColumnName == "ID_COMPONENT") {
                        arTable[i].Columns.RemoveAt (j);
                        break;
                    }
                    else
                        ;
                }
                */
                if (!(arTable[i].Columns.IndexOf("ID_COMPONENT") < 0))
                    try { arTable[i].Columns.Remove("ID_COMPONENT"); }
                    catch (Exception e)
                    { //ArgumentException
                        Logging.Logg().Exception(e, "Remove(\"ID_COMPONENT\")");
                    }
                else
                    ;
            }

            if (arTable[0].Rows.Count < arTable[1].Rows.Count) {
                arIndexTables[0] = 1;
                arIndexTables[1] = 0;
            }
            else {
            }

            for (i = 0; i < arTable.Length; i++) {
                arFieldsCount [i] = arTable [i].Columns.Count;
            }

            table = arTable[arIndexTables [0]].Copy();
            table.Merge(arTable[arIndexTables[1]].Clone (), false);

            for (i = 0; i < arTable[arIndexTables[0]].Rows.Count; i++)
            {
                for (j = 0; j < arTable[arIndexTables[1]].Rows.Count; j++)
                {
                    //Сравниваем дату/время 0 = [DATE_PBR], [DATE_ADMIN]
                    if (arTable[arIndexTables[0]].Rows[i][0].Equals (arTable[arIndexTables[1]].Rows[j][0])) {
                        for (k = 0; k < arTable[arIndexTables[1]].Columns.Count; k++)
                        {
                            table.Rows [i] [arTable[arIndexTables[1]].Columns [k].ColumnName] = arTable[arIndexTables[1]].Rows[j][k];
                        }

                        break;
                    }
                    else
                        ;
                }

                if (!(j < arTable[arIndexTables[1]].Rows.Count))
                {//Не было найдено соответствия по дате ППБР и админ./данных
                    for (k = 0; k < arTable[arIndexTables[1]].Columns.Count; k++)
                    {
                        if (k == 0) //Columns[k].ColumnName == DATE
                            table.Rows[i][arTable[arIndexTables[1]].Columns[k].ColumnName] = table.Rows[i][k];
                        else
                        {
                            //Type type = arTable[arIndexTables[1]].Columns[k].DataType;
                            table.Rows[i][arTable[arIndexTables[1]].Columns[k].ColumnName] = 0;
                        }
                    }
                }
                else
                    ;
            }

            offsetPBRNumber = m_tablePPBRValuesResponse.Columns.IndexOf("PBR_NUMBER");
            offsetDATE_ADMIN = table.Columns.IndexOf("DATE_ADMIN");

            //Для поиска одинаковых часов
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

                        //for (j = 0; j < 3 /*4 для SN???*/; j ++)
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
                                m_curRDGValues_PBR_0 = (double)table.Rows[i][arIndexTables[0] * arFieldsCount[1] + 1];

                                continue;
                            }
                            else
                                ;

                        if (bSeason == true)
                        {
                            if (hour == HAdmin.SeasonDateTime.Hour)
                            {
                                DataRow[] arSeasonRows;
                                //Копия в 'TecView'
                                //arSeasonRows = tableAdminValuesResponse.Select(@"DATE_ADMIN='" + iDate.ToString(@"yyyyMMdd HH:00") + @"' AND SEASON=" + (SEASON_BASE + (int)HAdmin.seasonJumpE.SummerToWinter));
                                arSeasonRows = tableAdminValuesResponse.Select(@"DATE_ADMIN='" + iDate.ToString(@"yyyy-MM-dd HH:00") + @"'");
                                if (arSeasonRows.Length > 0)
                                {
                                    int h = -1;
                                    foreach (DataRow r in arSeasonRows) {
                                        h = iDate.Hour;
                                        GetSeasonHourIndex(Int32.Parse(r[@"SEASON"].ToString ()), ref h);

                                        m_curRDGValues[h - 1].recomendation = (byte)r[@"FC"];

                                        m_curRDGValues[h - 1].recomendation = (double)r[@"REC"];
                                        m_curRDGValues[h - 1].deviationPercent = (int)r[@"IS_PER"] == 1;
                                        m_curRDGValues[h - 1].deviation = (double)r[@"DIVIAT"];
                                    }
                                }
                                else
                                {//Ошибка ... ???
                                    Logging.Logg().Error(@"AdminTS::GetAdminValueResponse () - ... нет ни одной записи для [HAdmin.SeasonDateTime.Hour] = " + hour);
                                }

                                //continue; ???
                                //Необходимо получить ППБР ???
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
                            ((! (hour == HAdmin.SeasonDateTime.Hour)) && (bSeason == true)))
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

                        if ((!(table.Rows[i]["DATE_PBR"] is System.DBNull)) && (offsetPBR == 0))
                        {
                            //for (j = 0; j < 3 /*4 для SN???*/; j ++)
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
                        Logging.Logg().Exception(e, @"AdminRS::GetAdminValuesResponse () - ... hour = " + hour);
                    }
                }

                if (hour > 0)
                    if (!(offsetPBRNumber < 0))
                        m_curRDGValues[hour - 1].pbr_number = table.Rows[i]["PBR_NUMBER"].ToString ();
                    else
                        m_curRDGValues[hour - 1].pbr_number = GetPBRNumber (hour - 1);
                else
                    ;

                //Копия сверху по разбору ... + копии в 'TecView'
                if (bSeason == true)
                {
                    if ((hour + 0) == (HAdmin.SeasonDateTime.Hour - 0))
                    {
                        m_curRDGValues[hour + 0].pbr = m_curRDGValues[hour - 1].pbr;
                        m_curRDGValues[hour + 0].pmin = m_curRDGValues[hour - 1].pmin;
                        m_curRDGValues[hour + 0].pmax = m_curRDGValues[hour - 1].pmax;

                        m_curRDGValues[hour + 0].pbr_number = m_curRDGValues[hour - 1].pbr_number;
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }

            return true;
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
                if (tec.m_markQueries.IsMarked ((int)CONN_SETT_TYPE.ADMIN) == true)
                    indx = (int)CONN_SETT_TYPE.ADMIN;
                else if (tec.m_markQueries.IsMarked ((int)CONN_SETT_TYPE.PBR) == true)
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
                Request(m_dictIdListeners[allTECComponents[indxTECComponents].tec.m_id][(int)CONN_SETT_TYPE.ADMIN], allTECComponents[indxTECComponents].tec.GetAdminDatesQuery(date, m_typeFields, allTECComponents[indxTECComponents]));
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
                        allTECComponents[indxTECComponents].tec.GetPBRDatesQuery(date, m_typeFields, allTECComponents[indxTECComponents]));
            else
                ;
        }

        private void ClearAdminDates()
        {
            ClearDates(CONN_SETT_TYPE.ADMIN);
        }

        protected virtual bool GetDatesResponse(CONN_SETT_TYPE type, DataTable table, DateTime date)
        {
            int addingVal = -1;

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
                    if (type == CONN_SETT_TYPE.PBR) {
                        addingVal = Int32.Parse(table.Rows[i][@"PBR_NUMBER"].ToString ().Substring (@"ПБР".Length));
                        if (m_iHavePBR_Number < addingVal)
                            m_iHavePBR_Number = addingVal;
                        else
                            ;
                    }
                    else
                        ;

                    m_arHaveDates[(int)type, hour - 1] = Convert.ToInt32 (table.Rows[i][1]); //true;
                }
                catch { }
            }

            return true;
        }

        private bool GetAdminDatesResponse(DataTable table, DateTime date)
        {
            return GetDatesResponse(CONN_SETT_TYPE.ADMIN, table, date);
        }

        protected override bool GetPPBRDatesResponse(DataTable table, DateTime date)
        {
            return GetDatesResponse(CONN_SETT_TYPE.PBR, table, date);
        }

        private int getCurrentHour (DateTime dt) {
            int iRes = -1;

            if ((serverTime.Date < dt) || (m_ignore_date == true))
                iRes = 0;
            else
                iRes = serverTime.Hour == 0 ? serverTime.Hour : serverTime.Hour - 1; //Возможность изменять рекомендации за тек./час

            //Возможность изменять рекомендации за пред./час
            if (iRes > 0)
                iRes--;
            else
                ;

            return iRes;
        }

        protected virtual string [] setAdminValuesQuery(TEC t, TECComponent comp, DateTime date)
        {
            string[] resQuery = new string[(int)DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE] { string.Empty, string.Empty, string.Empty };

            date = date.Date;
            int offset = -1
                , currentHour = getCurrentHour (date);

            for (int i = currentHour; i < m_curRDGValues.Length; i++)
            {
                offset = GetSeasonHourOffset(i + 1);

                // запись для этого часа имеется, модифицируем её
                if (IsHaveDates(CONN_SETT_TYPE.ADMIN, i) == true)
                {
                    switch (m_typeFields)
                    {
                        case AdminTS.TYPE_FIELDS.STATIC:
                            //name = t.NameFieldOfAdminRequest(comp);
                            string name = t.NameFieldOfAdminRequest(comp);
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + t.m_arNameTableAdminValues[(int)m_typeFields] +
                                        @" SET " + name +
                                        @"_REC='" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + name + @"_IS_PER=" + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                        @", " + name + "_DIVIAT='" + m_curRDGValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE = '" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                                        @"'" +
                                        @" AND " +
                                        @"ID = " + m_arHaveDates[(int)CONN_SETT_TYPE.ADMIN, i] +
                                        @"; ";
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + t.m_arNameTableAdminValues[(int)m_typeFields] +
                                        @" SET " +
                                        @"REC='" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + @"IS_PER=" + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                        @", " + "DIVIAT='" + m_curRDGValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + "SEASON=" + (offset > 0 ? (SEASON_BASE + (int)HAdmin.seasonJumpE.WinterToSummer) : (SEASON_BASE + (int)HAdmin.seasonJumpE.SummerToWinter)) +
                                        @", " + "FC=" + (m_curRDGValues[i].fc ? 1 : 0) +
                                        @" WHERE " +
                                        //@"DATE = '" + date.AddHours(i + 1 - offset).ToString("yyyyMMdd HH:mm:ss") +
                                        //@"'" +
                                        //@" AND ID_COMPONENT = " + comp.m_id +
                                        //@" AND " +
                                        @"ID = " + m_arHaveDates[(int)CONN_SETT_TYPE.ADMIN, i] +
                                        @"; ";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    // запись отсутствует, запоминаем значения
                    switch (m_typeFields)
                    {
                        case AdminTS.TYPE_FIELDS.STATIC:
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                                        @"', '" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                        @", '" + m_curRDGValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"'),";
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours(i + 1 - offset).ToString("yyyyMMdd HH:mm:ss") +
                                        @"', '" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                        @", '" + m_curRDGValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + (comp.m_id) +
                                        @", " + (offset > 0 ? (SEASON_BASE + (int)HAdmin.seasonJumpE.WinterToSummer) : (SEASON_BASE + (int)HAdmin.seasonJumpE.SummerToWinter)) +
                                        @", " + (m_curRDGValues[i].fc ? 1 : 0) +
                                        @"),";
                            break;
                        default:
                            break;
                    }
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
        
        protected virtual void SetAdminValuesRequest(TEC t, TECComponent comp, DateTime date)
        {
            Logging.Logg().Debug("AdminTS::SetAdminValuesRequest ()");

            string[] query = setAdminValuesQuery(t, comp, date);

            // добавляем все записи, не найденные в базе
            if (! (query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] == ""))
            {
                switch (m_typeFields)
                {
                    case AdminTS.TYPE_FIELDS.STATIC:
                        string name = t.NameFieldOfAdminRequest(comp);
                        query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = @"INSERT INTO " + t.m_arNameTableAdminValues[(int)m_typeFields] + " (DATE, " + name + @"_REC" +
                                @", " + name + "_IS_PER" +
                                @", " + name + "_DIVIAT) VALUES" + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Substring(0, query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Length - 1) + ";";
                        break;
                    case AdminTS.TYPE_FIELDS.DYNAMIC:
                        query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = @"INSERT INTO " + t.m_arNameTableAdminValues[(int)m_typeFields] + " (DATE, " + @"REC" +
                                @", " + "IS_PER" +
                                @", " + "DIVIAT" +
                                @", " + "ID_COMPONENT" +
                                @", " + "SEASON" +
                                @", " + "FC" +
                                @") VALUES" + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Substring(0, query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Length - 1) + ";";
                        break;
                    default:
                        break;
                }
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
            {
                // запись для этого часа имеется, модифицируем её
                if (IsHaveDates(CONN_SETT_TYPE.ADMIN, i) == true)
                {
                    switch (m_typeFields)
                    {
                        case AdminTS.TYPE_FIELDS.STATIC:
                            string name = t.NameFieldOfAdminRequest(comp);
                            query[(int)DbTSQLInterface.QUERY_TYPE.DELETE] += @"DELETE FROM " + t.m_arNameTableAdminValues[(int)m_typeFields] +
                                        @"' WHERE " +
                                        @"DATE = '" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                                        @"'; ";
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            query[(int)DbTSQLInterface.QUERY_TYPE.DELETE] += @"DELETE FROM " + t.m_arNameTableAdminValues[(int)m_typeFields] +
                                        @" WHERE " +
                                        @"DATE = '" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                                        @"'" +
                                        @" AND ID_COMPONENT = " + comp.m_id + "; ";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                }
            }

            Logging.Logg().Debug("AdminTS::ClearAdminValuesRequest ()");

            //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, requestUpdate + requestInsert + requestDelete);
            Request(m_dictIdListeners[t.m_id][(int)CONN_SETT_TYPE.ADMIN], query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]);
        }

        protected virtual string[] setPPBRQuery(TEC t, TECComponent comp, DateTime date)
        {
            string[] resQuery = new string[(int)DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE] { string.Empty, string.Empty, string.Empty };

            date = date.Date;
            int currentHour = getCurrentHour (date);

            for (int i = currentHour; i < 24; i++)
            {
                // запись для этого часа имеется, модифицируем её
                if (IsHaveDates(CONN_SETT_TYPE.PBR, i) == true)
                {
                    switch (m_typeFields)
                    {
                        case AdminTS.TYPE_FIELDS.STATIC:
                            string name = t.NameFieldOfPBRRequest(comp);
                            /*requestUpdate += @"UPDATE " + t.m_strUsedPPBRvsPBR + " SET " + name + @"_" + t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.REC] + "='" + m_curRDGValues[i].plan.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                                        @"'; ";*/
                            if (t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR].Equals (string.Empty) == false)  {
                                resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + t.m_arNameTableUsedPPBRvsPBR[(int)m_typeFields] + " SET " +
                                @"PBR_NUMBER='";
                                if ((! (m_curRDGValues[i].pbr_number == null)) && (m_curRDGValues[i].pbr_number.Length > 3))
                                    resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += m_curRDGValues[i].pbr_number;
                                else
                                    resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += GetPBRNumber();
                                resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"'" +
                                            @", " + name + @"_" + t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR] + "='" + m_curRDGValues[i].pbr.ToString("F1", CultureInfo.InvariantCulture) + @"'" +
                                            @", " + name + @"_" + @"Pmin" + "='" + m_curRDGValues[i].pmin.ToString("F1", CultureInfo.InvariantCulture) + @"'" +
                                            @", " + name + @"_" + @"Pmax" + "='" + m_curRDGValues[i].pmax.ToString("F1", CultureInfo.InvariantCulture) + @"'" +
                                            @" WHERE " +
                                            t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_DATETIME] + @" = '" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                                            @"'" +
                                            @" AND " +
                                            @"ID = " + m_arHaveDates[(int)CONN_SETT_TYPE.PBR, i] +
                                            @"; ";
                            }
                            else
                                ;
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            bool bUpdate = m_ignore_date;
                            int pbr_number = -1;

                            if ((!(m_curRDGValues[i].pbr_number == null)) && (m_curRDGValues[i].pbr_number.Length > @"ПБР".Length))
                                pbr_number = Int32.Parse (m_curRDGValues[i].pbr_number.Substring (@"ПБР".Length));
                            else
                                pbr_number = getPBRNumber();

                            if (bUpdate == false)
                                if (m_iHavePBR_Number < pbr_number)
                                    //Обновляемый старше
                                    bUpdate = true;
                                else
                                    if (m_iHavePBR_Number == pbr_number)
                                        if (m_sOwner_PBR == 1)
                                            //ПБР одинаков - требование пользователя
                                            bUpdate = true;
                                        else
                                            //ПБР одинаков - работает программа
                                            ;
                                    else
                                        //Обновляемый новый
                                        ;
                            else
                                ;                                

                            if (bUpdate == true) {
                                resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE [" + t.m_arNameTableUsedPPBRvsPBR[(int)m_typeFields] + @"]" +
                                            " SET " +
                                            @"PBR_NUMBER='";
                                if ((!(m_curRDGValues[i].pbr_number == null)) && (m_curRDGValues[i].pbr_number.Length > 3))
                                    resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += m_curRDGValues[i].pbr_number;
                                else
                                    resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += GetPBRNumber();
                                resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += "'" +
                                            @", WR_DATE_TIME=GETADTE()" +
                                            @", OWNER=" + m_sOwner_PBR +
                                            @", PBR='" + m_curRDGValues[i].pbr.ToString("F2", CultureInfo.InvariantCulture) + "'" +
                                            @", Pmin='" + m_curRDGValues[i].pmin.ToString("F2", CultureInfo.InvariantCulture) + "'" +
                                            @", Pmax='" + m_curRDGValues[i].pmax.ToString("F2", CultureInfo.InvariantCulture) + "'" +
                                            @" WHERE " +
                                            @"DATE_TIME" + @" = '" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                                            @"'" +
                                            @" AND ID_COMPONENT = " + comp.m_id +
                                            @" AND " +
                                            @"ID = " + m_arHaveDates[(int)CONN_SETT_TYPE.PBR, i] +
                                            @"; ";
                            }
                            else
                                ;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    string strPBRNumber = GetPBRNumber(DateTime.Now.Hour);
                    // запись отсутствует, запоминаем значения
                    switch (m_typeFields)
                    {
                        case AdminTS.TYPE_FIELDS.STATIC:
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                                        @"', '" + serverTime.Date.ToString("yyyyMMdd HH:mm:ss") +
                                        @"', '" + strPBRNumber +
                                        @"', '" + m_sOwner_PBR +
                                        @"', '" + m_curRDGValues[i].pbr.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"', '" + m_curRDGValues[i].pmin.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"', '" + m_curRDGValues[i].pmax.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"'),";
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            if (!(m_curRDGValues[i].pbr < 0))
                                resQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                                            //@"', '" + serverTime.ToString("yyyyMMdd HH:mm:ss") +
                                            @"', 'GETDATE()" +
                                            @"', '" + strPBRNumber +
                                            @"', " + comp.m_id +
                                            @", '" + m_sOwner_PBR + "'" +
                                            @", '" + m_curRDGValues[i].pbr.ToString("F1", CultureInfo.InvariantCulture) + "'" +
                                            @", '" + m_curRDGValues[i].pmin.ToString("F1", CultureInfo.InvariantCulture) + "'" +
                                            @", '" + m_curRDGValues[i].pmax.ToString("F1", CultureInfo.InvariantCulture) + "'" +
                                            @"),";
                            else
                                ; //Нельзя записывать значения "-1"
                            break;
                        default:
                            break;
                    }
                }
            }

            return resQuery;
        }

        protected /*virtual*/ void SetPPBRRequest(TEC t, TECComponent comp, DateTime date)
        {
            string[] query = setPPBRQuery(t, comp, date);

            // добавляем все записи, не найденные в базе
            if (query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Equals (string.Empty) == false)
            {
                switch (m_typeFields)
                {
                    case AdminTS.TYPE_FIELDS.STATIC:
                        string name = t.NameFieldOfPBRRequest(comp);
                        query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = @"INSERT INTO " + t.m_arNameTableUsedPPBRvsPBR[(int)m_typeFields] + " (DATE_TIME, WR_DATE_TIME, PBR_NUMBER, IS_COMDISP, " + name + @"_PBR," + name + "_Pmin," + name + "_Pmax) VALUES" + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Substring(0, query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Length - 1) + ";";
                        break;
                    case AdminTS.TYPE_FIELDS.DYNAMIC:
                        query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = @"INSERT INTO [" + t.m_arNameTableUsedPPBRvsPBR[(int)m_typeFields] + "] (DATE_TIME, WR_DATE_TIME, PBR_NUMBER, ID_COMPONENT, OWNER, PBR, Pmin, Pmax) VALUES" + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Substring(0, query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Length - 1) + ";";
                        break;
                    default:
                        break;
                }
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

            Logging.Logg().Debug("AdminTS::SetPPBRRequest ()");

            if ((query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE].Equals(string.Empty) == false) ||
                (query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Equals(string.Empty) == false))
                //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, requestUpdate + requestInsert + requestDelete);
                Request(m_dictIdListeners[t.m_id][(int)CONN_SETT_TYPE.PBR], query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]);
            else
                Logging.Logg().Debug("AdminTS::SetPPBRRequest () - Empty"); //Запрос пуст
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
            {
                // запись для этого часа имеется, модифицируем её
                if (IsHaveDates(CONN_SETT_TYPE.PBR, i) == true)
                {
                    switch (m_typeFields)
                    {
                        case AdminTS.TYPE_FIELDS.STATIC:
                            string name = t.NameFieldOfPBRRequest(comp);
                            query[(int)DbTSQLInterface.QUERY_TYPE.DELETE] += @"DELETE FROM " + t.m_arNameTableUsedPPBRvsPBR[(int)m_typeFields] +
                                        @"' WHERE " +
                                        t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_DATETIME] + @" = '" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                                        @"'; ";
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            query[(int)DbTSQLInterface.QUERY_TYPE.DELETE] += @"DELETE FROM [" + t.m_arNameTableUsedPPBRvsPBR[(int)m_typeFields] + @"]" +
                                        @" WHERE " +
                                        @"DATE_TIME" + @" = '" + date.AddHours(i + 1).ToString("yyyyMMdd HH:mm:ss") +
                                        @"'" +
                                        @" AND ID_COMPONENT = " + comp.m_id + "; ";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                }
            }

            Logging.Logg().Debug("ClearPPBRRequest");

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
                                m_listDbInterfaces.Add(new DbTSQLInterface(dbType, "Интерфейс: " + dbNameType + "-БД, " + "ТЭЦ: " + t.name_shr));
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
                Logging.Logg().Error(@"AdminTS::Start () - m_list_tec == null");

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
                Logging.Logg().Error(@"AdminTS::Stop () - m_list_tec == null");
        }

        protected override bool StateRequest(int /*StatesMachine*/ state)
        {
            bool result = true;
            string strRep = string.Empty;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    strRep = @"Получение текущего времени сервера.";
                    GetCurrentTimeRequest();
                    break;
                case (int)StatesMachine.PPBRValues:
                    strRep = @"Получение данных плана.";
                    if (indxTECComponents < allTECComponents.Count)
                        GetPPBRValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate.Date, m_typeFields);
                    else
                        ; //result = false;
                    break;
                case (int)StatesMachine.AdminValues:
                    strRep = @"Получение административных данных.";
                    if ((indxTECComponents < allTECComponents.Count) && (allTECComponents[indxTECComponents].tec.m_markQueries.IsMarked ((int)CONN_SETT_TYPE.ADMIN) == true))
                        GetAdminValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate.Date, m_typeFields);
                    else
                        ; //result = false;

                    //this.BeginInvoke(delegateCalendarSetDate, m_prevDatetime);
                    break;
                case (int)StatesMachine.ImpRDGExcelValues:
                    strRep = @"Импорт РДГ из Excel.";
                    delegateImportForeignValuesRequuest();
                    break;
                case (int)StatesMachine.ExpRDGExcelValues:
                    strRep = @"Экспорт РДГ в книгу Excel.";
                    delegateExportForeignValuesRequuest();
                    break;
                 case (int)StatesMachine.PPBRCSVValues:
                    strRep = @"Импорт из формата CSV.";
                    delegateImportForeignValuesRequuest();
                    break;
                case (int)StatesMachine.PPBRDates:
                    if ((serverTime.Date > m_curDate.Date) && (m_ignore_date == false))
                    {
                        //Останавливаем сохранение
                        saveResult = Errors.InvalidValue;
                        try
                        {
                            semaDBAccess.Release(1);
                        }
                        catch
                        {
                        }
                        result = false;
                        break;
                    }
                    else
                        ;                        
                    strRep = @"Получение списка сохранённых часовых значений.";
                    GetPPBRDatesRequest(m_curDate);
                    break;
                case (int)StatesMachine.AdminDates:
                    if ((serverTime.Date > m_curDate.Date) && (m_ignore_date == false))
                    {
                        //Останавливаем сохранение
                        saveResult = Errors.InvalidValue;
                        try
                        {
                            semaDBAccess.Release(1);
                        }
                        catch
                        {
                        }
                        result = false;
                        break;
                    }
                    else
                        ;
                    strRep = @"Получение списка сохранённых часовых значений.";
                    if (allTECComponents[indxTECComponents].tec.m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true)
                        GetAdminDatesRequest(m_curDate);
                    else
                        ;
                    break;
                case (int)StatesMachine.SaveAdminValues:
                    strRep = @"Сохранение административных данных.";
                    if ((indxTECComponents < allTECComponents.Count) && (allTECComponents[indxTECComponents].tec.m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true))
                        SetAdminValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    else
                        ; //result = false;
                    break;
                case (int)StatesMachine.SavePPBRValues:
                    strRep = @"Сохранение ПЛАНА.";
                    if (indxTECComponents < allTECComponents.Count)
                        SetPPBRRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    else
                        ; //result = false;
                    break;
                //case StatesMachine.LayoutGet:
                //    ActionReport("Получение административных данных макета.");
                //    GetLayoutRequest(m_curDate);
                //    break;
                //case StatesMachine.LayoutSet:
                //    ActionReport("Сохранение административных данных макета.");
                //    SetLayoutRequest(m_curDate);
                //    break;
                case (int)StatesMachine.ClearAdminValues:
                    strRep = @"Сохранение административных данных.";
                    if (indxTECComponents < allTECComponents.Count)
                        ClearAdminValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    else
                        ; //result = false;
                    break;
                case (int)StatesMachine.ClearPPBRValues:
                    strRep = @"Сохранение ПЛАНА.";
                    if (indxTECComponents < allTECComponents.Count)
                        ClearPPBRRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    else
                        ; //result = false;
                    break;
                default:
                    break;
            }

            FormMainBaseWithStatusStrip.m_report.ActionReport(strRep);

            //Logging.Logg().Debug(@"AdminTS::StateRequest () - state=" + state.ToString() + @" - вЫход...");

            return result;
        }

        protected override bool StateCheckResponse(int /*StatesMachine*/ state, out bool error, out DataTable table)
        {
            bool bRes = false;

            error = true;
            table = null;

            if (((state == (int)StatesMachine.ImpRDGExcelValues) || (state == (int)StatesMachine.ExpRDGExcelValues)) ||
                (state == (int)StatesMachine.PPBRCSVValues) ||
                /*((!(m_indxDbInterfaceCurrent < 0)) && (m_listListenerIdCurrent.Count > 0))*/
                (!(m_IdListenerCurrent < 0)))
            {
                switch (state)
                {
                    case (int)StatesMachine.ImpRDGExcelValues:
                        if ((!(m_tableRDGExcelValuesResponse == null)) && (m_tableRDGExcelValuesResponse.Rows.Count > 24))
                        {
                            error = false;

                            bRes = true;
                        }
                        else
                            ;
                        break;
                    case (int)StatesMachine.ExpRDGExcelValues:
                            //??? Всегда успех ???
                            error = false;
                            bRes = true;
                        break;
                     case (int)StatesMachine.PPBRCSVValues:
                        if ((!(m_tablePPBRValuesResponse == null)) && (m_tablePPBRValuesResponse.Rows.Count > 0))
                        {
                            error = false;

                            bRes = true;
                        }
                        else
                            ;
                        break;
                    case (int)StatesMachine.AdminDates:
                        if (allTECComponents [indxTECComponents].tec.m_markQueries.IsMarked ((int)CONN_SETT_TYPE.ADMIN) == true)
                            bRes = Response(m_IdListenerCurrent, out error, out table/*, false*/);
                        else {
                            error = false;
                            table = null;

                            bRes = true;
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
                        bRes = Response(m_IdListenerCurrent, out error, out table/*, false*/);
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
                //Ошибка???

                error = true;
                table = null;

                bRes = false;
            }

            return bRes;
        }

        protected override bool StateResponse(int /*StatesMachine*/ state, DataTable table)
        {
            bool result = false;
            string strRep = string.Empty;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    result = GetCurrentTimeResponse(table);
                    if (result == true)
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
                    result = GetPPBRValuesResponse(table, m_curDate);
                    if (result == true)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.AdminValues:
                    if (allTECComponents[indxTECComponents].tec.m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true)
                        result = GetAdminValuesResponse(table, m_curDate);
                    else {
                        table = null;

                        if (allTECComponents[indxTECComponents].tec.m_markQueries.IsMarked((int)CONN_SETT_TYPE.PBR) == true)
                            //result = GetAdminValuesResponseWithoutAdminValues(m_curDate);
                            result = GetAdminValuesResponse(null, m_curDate);
                        else
                            result = false;
                    }

                    if (result == true)
                    {
                        readyData(m_prevDate);
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.ImpRDGExcelValues:
                    ActionReport("Импорт РДГ из Excel.");
                    //result = GetRDGExcelValuesResponse(table, m_curDate);
                    result = delegateImportForeignValuesResponse();
                    if (result == true)
                    {
                        readyData(m_prevDate);
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.ExpRDGExcelValues:
                    ActionReport("Экспорт РДГ в книгу Excel.");
                    //??? Всегда успех ???
                    saveResult = Errors.NoError;
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch
                    {
                    }
                    result = true;
                    break;
                case (int)StatesMachine.PPBRCSVValues:
                    ActionReport("Импорт ПБР из формата CSV.");
                    //result = GetRDGExcelValuesResponse(table, m_curDate);
                    result = delegateImportForeignValuesResponse();
                    if (result)
                    {
                        readyData(m_prevDate);
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.PPBRDates:
                    ClearPPBRDates();
                    result = GetPPBRDatesResponse(table, m_curDate);
                    if (result == true)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.AdminDates:
                    ClearAdminDates();
                    if (allTECComponents[indxTECComponents].tec.m_markQueries.IsMarked((int)CONN_SETT_TYPE.ADMIN) == true)
                        result = GetAdminDatesResponse(table, m_curDate);
                    else
                        result = true;

                    if (result == true)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.SaveAdminValues:
                    saveResult = Errors.NoError;
                    //Если состояние крайнее, то освободить доступ к БД
                    if (states.IndexOf (state) == (states.Count - 1))
                        try { semaDBAccess.Release(1); }
                        catch { }
                    else
                        ;
                    result = true;
                    if (result == true) { }
                    else ;
                    break;
                case (int)StatesMachine.SavePPBRValues:
                    saveResult = Errors.NoError;
                    //Если состояние крайнее, то освободить доступ к БД
                    if (states.IndexOf(state) == (states.Count - 1))
                        try { semaDBAccess.Release(1); }
                        catch (Exception e) {
                            Logging.Logg().Exception(e, @"AdminTS::StateResponse () - semaDBAccess.Release(1) - StatesMachine.SavePPBRValues...");
                        }
                    else
                        ;
                    result = true;
                    if (result == true)
                    {
                        Logging.Logg().Debug(@"AdminTS::StateResponse () - saveComplete is set=" + (saveComplete == null ? false.ToString () : true.ToString ()) + @" - вЫход...");

                        //Вызов завершения операции сохранения изменений - НЕЛьЗЯ операция не завершена
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
                    result = true;
                    if (result == true) { }
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
                    result = true;
                    if (result == true)
                    {
                    }
                    break;
                default:
                    break;
            }

            if (result == true)
                FormMainBaseWithStatusStrip.m_report.ClearStates ();
            else
                ;

            //Logging.Logg().Debug(@"AdminTS::StateResponse () - state=" + state.ToString() + @", result=" + result.ToString() + @" - вЫход...");

            return result;
        }

        protected override void StateErrors(int /*StatesMachine*/ state, bool response)
        {
            bool bClear = false;

            string error = string.Empty,
                reason = string.Empty,
                waiting = string.Empty;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    if (response == true)
                    {
                        reason = @"разбора";

                        if (saving == true)
                            saveResult = Errors.ParseError;
                        else
                            ;
                    }
                    else
                    {
                        reason = @"получения";

                        if (saving == true)
                            saveResult = Errors.NoAccess;
                        else
                            ;
                    }

                    reason += @" текущего времени сервера";
                    waiting = @"Переход в ожидание";

                    if (saving == true)
                    {
                        try
                        {
                            semaDBAccess.Release(1);
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, "AdminTS::StateErrors () - semaDBAccess.Release(1)");
                        }
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.PPBRValues:
                    if (response == true)
                        reason = @"разбора";
                    else {
                        reason = @"получения";
                        bClear = true;
                    }

                    reason += @" данных плана";
                    waiting = @"Переход в ожидание";

                    break;
                case (int)StatesMachine.AdminValues:
                    if (response)
                        reason = @"разбора";
                    else {
                        reason = @"получения";
                        bClear = true;
                    }

                    reason += @" административных данных";
                    waiting = @"Переход в ожидание";

                    break;
                case (int)StatesMachine.ImpRDGExcelValues:
                    reason = @"импорта РДГ из книги Excel";
                    waiting = @"Переход в ожидание";

                    // ???
                    break;
                case (int)StatesMachine.ExpRDGExcelValues:
                    reason = @"экспорта РДГ из книги Excel";
                    waiting = @"Переход в ожидание";
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
                case (int)StatesMachine.PPBRCSVValues:
                    reason = @"импорта из формата CSV";
                    waiting = @"Переход в ожидание";

                    // ???
                    break;
                case (int)StatesMachine.PPBRDates:
                    if (response == true)
                    {
                        reason = @"разбора";
                        saveResult = Errors.ParseError;
                    }
                    else
                    {
                        reason = @"получения";
                        saveResult = Errors.NoAccess;
                    }
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "AdminTS::StateErrors () - semaDBAccess.Release(1)");
                    }

                    reason += @" сохранённых часовых значений (PPBR)";
                    waiting = @"Переход в ожидание";

                    break;
                case (int)StatesMachine.AdminDates:
                    if (response)
                    {
                        reason = @"разбора";
                        saveResult = Errors.ParseError;
                    }
                    else
                    {
                        reason = @"получения";
                        saveResult = Errors.NoAccess;
                    }
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "AdminTS::StateErrors () - semaDBAccess.Release(1)");
                    }

                    reason += @" сохранённых часовых значений (AdminValues)";
                    waiting = @"Переход в ожидание";

                    break;
                case (int)StatesMachine.SaveAdminValues:
                    saveResult = Errors.NoAccess;
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "AdminTS::StateErrors () - semaDBAccess.Release(1)");
                    }

                    reason = @"сохранения административных данных";
                    waiting = @"Переход в ожидание";
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

                    reason = @"сохранения данных ПЛАНа";
                    waiting = @"Переход в ожидание";

                    break;
                //case (int)StatesMachine.LayoutGet:
                //    if (response)
                //    {
                //        ErrorReport("Ошибка разбора административных данных макета. Переход в ожидание.");
                //        loadLayoutResult = Errors.ParseError;
                //    }
                //    else
                //    {
                //        ErrorReport("Ошибка получения административных данных макета. Переход в ожидание.");
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
                //    ErrorReport("Ошибка сохранения административных данных макета. Переход в ожидание.");
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
                    reason = @"удаления административных данных";
                    waiting = @"Переход в ожидание";
                    break;
                case (int)StatesMachine.ClearPPBRValues:
                    reason = @"удаления данных ПЛАНа";
                    waiting = @"Переход в ожидание";
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

            error = "Ошибка " + reason + ".";

            if (waiting.Equals(string.Empty) == false)
                error += " " + waiting + ".";
            else
                ;

            ErrorReport(error);

            if (! (errorData == null)) errorData (); else ;

            Logging.Logg().Error(@"AdminTS::StateErrors () - error=" + error + @" - вЫход...");
        }

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

                        Logging.Logg().Debug("AdminTS::SaveRDGValues () - states.Clear()");

                        //states.Add((int)StatesMachine.CurrentTime);
                        states.Add((int)StatesMachine.PPBRValues);
                        states.Add((int)StatesMachine.AdminValues);

                        try
                        {
                            semaState.Release(1);
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, "AdminTS::SaveRDGValues () - semaState.Release(1)");
                        }
                    }
                else
                    ;
            }
            else
            {
                if (resultSaving == Errors.InvalidValue)
                    //MessageBox.Show(this, "Изменение ретроспективы недопустимо!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    base.MessageBox("Изменение ретроспективы недопустимо!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                else
                    //MessageBox.Show(this, "Не удалось сохранить изменения, возможно отсутствует связь с базой данных.", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    base.MessageBox("Не удалось сохранить изменения, возможно отсутствует связь с базой данных.");
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

        //            states.Add((int)StatesMachine.CurrentTime);
        //            states.Add((int)StatesMachine.PPBRValues);
        //            states.Add((int)StatesMachine.AdminValues);

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
        //            //MessageBox.Show(this, "Изменение ретроспективы недопустимо!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        //            MessageBox("Изменение ретроспективы недопустимо!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        //        else
        //            //MessageBox.Show(this, "Не удалось сохранить изменения, возможно отсутствует связь с базой данных.", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            MessageBox("Не удалось сохранить изменения, возможно отсутствует связь с базой данных.");
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

                    //states.Add((int)StatesMachine.CurrentTime);
                    states.Add((int)StatesMachine.PPBRValues);
                    states.Add((int)StatesMachine.AdminValues);

                    try
                    {
                        semaState.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "AdminTS::ClearRDGValues () - semaState.Release(1)");
                    }
                }
            }
            else
            {
                MessageBox("Не удалось удалить значения РДГ, возможно отсутствует связь с базой данных.");
            }
        }

        public void ReConnSettingsRDGSource(int idListaener, int [] arIdSource)
        {
            int err = -1;

            for (int i = 0; i < m_list_tec.Count; i ++) {
                if (m_list_tec[i].type () == TEC.TEC_TYPE.COMMON) {
                    //TYPE_DATABASE_CFG.CFG_200 = ???
                    m_list_tec[i].connSettings(StatisticCommon.InitTECBase.getConnSettingsOfIdSource(HClassLibrary.TYPE_DATABASE_CFG.CFG_200, idListaener, arIdSource[(int)TEC.TEC_TYPE.COMMON], -1, out err), (int)CONN_SETT_TYPE.ADMIN);
                    m_list_tec[i].connSettings(StatisticCommon.InitTECBase.getConnSettingsOfIdSource(HClassLibrary.TYPE_DATABASE_CFG.CFG_200, idListaener, arIdSource[(int)TEC.TEC_TYPE.COMMON], -1, out err), (int)CONN_SETT_TYPE.PBR);
                }
                else {
                    if (m_list_tec[i].type() == TEC.TEC_TYPE.BIYSK)
                    {
                        //TYPE_DATABASE_CFG.CFG_200 = ???
                        m_list_tec[i].connSettings(StatisticCommon.InitTECBase.getConnSettingsOfIdSource(TYPE_DATABASE_CFG.CFG_200, idListaener, arIdSource[(int)TEC.TEC_TYPE.BIYSK], -1, out err), (int)CONN_SETT_TYPE.ADMIN);
                        m_list_tec[i].connSettings(StatisticCommon.InitTECBase.getConnSettingsOfIdSource(TYPE_DATABASE_CFG.CFG_200, idListaener, arIdSource[(int)TEC.TEC_TYPE.BIYSK], -1, out err), (int)CONN_SETT_TYPE.PBR);
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
        /// Идентификатор компонента ТЭЦ
        /// </summary>
        /// <param name="indx">индекс в массиве 'все компоненты'</param>
        /// <returns>идентификатор</returns>
        public int GetIdTECComponent(int indx = -1)
        {
            if (indx < 0) indx = indxTECComponents; else ;

            int iRes = -1;

            if (indx < allTECComponents.Count) iRes = allTECComponents[indx].m_id; else ;

            return iRes;
        }
        
        /// <summary>
        /// Наименование (краткое) компонента ТЭЦ
        /// </summary>
        /// <param name="indx">индекс в массиве 'все компоненты'</param>
        /// <returns>наименование</returns>
        public string GetNameTECComponent(int indx = -1)
        {
            if (indx < 0) indx = indxTECComponents; else ;

            string strRes = string.Empty;

            if (indx < allTECComponents.Count) strRes = allTECComponents[indx].name_shr; else ;

            return strRes;
        }

        /// <summary>
        /// Сохранение текущих значений (ПБР + рекомендации = РДГ) для последующего изменения
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
                m_prevRDGValues[i].recomendation = m_curRDGValues[i].recomendation;
                m_prevRDGValues[i].deviationPercent = m_curRDGValues[i].deviationPercent;
                m_prevRDGValues[i].deviation = m_curRDGValues[i].deviation;
            }
        }

        /// <summary>
        /// Копирование значений (ПБР + рекомендации = РДГ) из источника
        /// </summary>
        /// <param name="source">источник</param>
        public override void getCurRDGValues(HAdmin source)
        {
            base.getCurRDGValues (source);

            for (int i = 0; i < source.m_curRDGValues.Length; i++)
            {
                m_curRDGValues[i].pbr = ((HAdmin)source).m_curRDGValues[i].pbr;
                m_curRDGValues[i].pmin = ((HAdmin)source).m_curRDGValues[i].pmin;
                m_curRDGValues[i].pmax = ((HAdmin)source).m_curRDGValues[i].pmax;
                m_curRDGValues[i].pbr_number = ((HAdmin)source).m_curRDGValues[i].pbr_number;
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
