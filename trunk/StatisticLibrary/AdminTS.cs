using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.IO;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Globalization;

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

        public enum INDEX_MARK_PPBRVALUES { ENABLED, MARK };
        protected bool [] m_arMarkSavePPBRValues = new bool [] { false, false};

        public AdminTS(HReports rep, bool[] arMarkSavePPBRValues)
            : base(rep)
        {
            if (! (arMarkSavePPBRValues == null))
                arMarkSavePPBRValues.CopyTo(m_arMarkSavePPBRValues, 0);
            else
                ;
        }

        protected override void Initialize () {
            base.Initialize ();

            //layoutForLoading = new LayoutData(1);
        }

        public virtual Errors SaveChanges()
        {
            Logging.Logg().LogDebugToFile("AdminTS::SaveChanges () - вХод...");
            
            delegateStartWait();

            Logging.Logg().LogDebugToFile("AdminTS::SaveChanges () - delegateStartWait() - Интервал ожидания для semaDBAccess=" + DbInterface.MAX_WATING);
            
            //if (semaDBAccess.WaitOne(6666) == true) {
            if (semaDBAccess.WaitOne(DbInterface.MAX_WATING) == true)
            {
                lock (m_lockObj)
                {
                    saveResult = Errors.NoAccess;
                    saving = true;
                    using_date = false;
                    m_curDate = m_prevDate;

                    newState = true;
                    states.Clear();

                    Logging.Logg().LogDebugToFile("AdminTS::SaveChanges () - states.Clear()");

                    states.Add((int)StatesMachine.CurrentTime);
                    states.Add((int)StatesMachine.AdminDates);
                    //??? Состояния позволяют НАЧать процесс разработки возможности редактирования ПЛАНа на вкладке 'Редактирование ПБР'
                    states.Add((int)StatesMachine.PPBRDates);
                    states.Add((int)StatesMachine.SaveAdminValues);
                    if (m_arMarkSavePPBRValues[(int)INDEX_MARK_PPBRVALUES.MARK] == true) states.Add((int)StatesMachine.SavePPBRValues); else ;
                    //states.Add((int)StatesMachine.UpdateValuesPPBR);

                    try
                    {
                        semaState.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().LogExceptionToFile(e, "AdminTS::SaveChanges () - semaState.Release(1)");
                    }
                }

                semaDBAccess.WaitOne();
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
                Logging.Logg().LogDebugToFile("AdminTS::SaveChanges () - semaDBAccess.WaitOne()=false");

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

            Logging.Logg().LogDebugToFile("AdminTS::SaveChanges () - delegateStartWait() - Интервал ожидания для semaDBAccess=" + DbInterface.MAX_WATING);

            //if (semaDBAccess.WaitOne(6666) == true) {
            if (semaDBAccess.WaitOne(DbInterface.MAX_WATING) == true)
            {
                lock (m_lockObj)
                {
                    errClearResult = Errors.NoError;
                    using_date = false;
                    m_curDate = m_prevDate;

                    newState = true;
                    states.Clear();

                    Logging.Logg().LogDebugToFile("AdminTS::ClearRDG () - states.Clear()");

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
                        Logging.Logg().LogExceptionToFile(e, "AdminTS::ClearRDG () - semaState.Release(1)");
                    }
                }

                semaDBAccess.WaitOne();
                try
                {
                    semaDBAccess.Release(1);
                }
                catch
                {
                }
            }
            else {
                Logging.Logg().LogDebugToFile("AdminTS::ClearRDG () - semaDBAccess.WaitOne()=false");

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
            if (!started)
                return;
            else
                ;

            /*InitDbInterfaces ();*/

            lock (m_lockObj)
            {
                //m_curDate = mcldrDate.SelectionStart;
                m_curDate = m_prevDate;
                saving = false;

                using_date = true; //???

                newState = true;
                states.Clear();
                states.Add((int)StatesMachine.CurrentTime);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, "AdminTS::Reinit () - semaState.Release(1)");
                }
            }
        }

        public void GetCurrentTime(int indx)
        {
            lock (m_lockObj)
            {
                indxTECComponents = indx;

                newState = true;
                states.Clear();

                Logging.Logg().LogDebugToFile("AdminTS::GetCurrentTime () - states.Clear()");

                states.Add((int)StatesMachine.CurrentTime);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, "AdminTS::GetCurrentTime () - semaState.Release(1)");
                }
            }
        }

        protected bool IsCanUseTECComponents () {
            bool bRes = false;
            if ((! (indxTECComponents < 0)) && (indxTECComponents < allTECComponents.Count)) bRes = true; else ;
            return bRes;
        }

        private void GetCurrentTimeRequest()
        {
            string query = string.Empty;
            DbInterface.DB_TSQL_INTERFACE_TYPE typeDB = DbInterface.DB_TSQL_INTERFACE_TYPE.UNKNOWN;
            
            if (IsCanUseTECComponents ()) {
                typeDB = DbTSQLInterface.getTypeDB (allTECComponents[indxTECComponents].tec.connSetts[(int)CONN_SETT_TYPE.ADMIN].port);
                switch (typeDB) {
                    case DbInterface.DB_TSQL_INTERFACE_TYPE.MySQL:
                        query = @"SELECT now()";
                        break;
                    case DbInterface.DB_TSQL_INTERFACE_TYPE.MSSQL:
                        query =  @"SELECT GETDATE()";
                        break;
                    default:
                        break;
                }

                if (query.Equals (string.Empty) == false)
                    Request(allTECComponents[indxTECComponents].tec.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], query);
                else
                    ;
            }
            else
                ;
        }

        protected bool GetCurrentTimeResponse(DataTable table)
        {
            if (table.Rows.Count == 1)
            {
                serverTime = (DateTime)table.Rows[0][0];
            }
            else
            {
                DaylightTime daylight = TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year);
                int timezone_offset = allTECComponents [indxTECComponents].tec.m_timezone_offset_msc;
                if (TimeZone.IsDaylightSavingTime(DateTime.Now, daylight))
                    timezone_offset ++;
                else
                    ;

                serverTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(3);

                ErrorReport("Ошибка получения текущего времени сервера. Используется локальное время.");
            }

            return true;
        }
        
        public virtual void GetRDGValues (TYPE_FIELDS mode, int indx) {
            //Запретить запись ПБР-значений
            if (m_arMarkSavePPBRValues[(int)INDEX_MARK_PPBRVALUES.ENABLED] == true) m_arMarkSavePPBRValues[(int)INDEX_MARK_PPBRVALUES.MARK] = false; else ;
            
            lock (m_lockObj)
            {
                indxTECComponents = indx;
                
                ClearValues();

                using_date = true;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_typeFields = mode;

                newState = true;
                states.Clear();
                states.Add((int)StatesMachine.CurrentTime);
                states.Add((int)StatesMachine.PPBRValues);
                states.Add((int)StatesMachine.AdminValues);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, "AdminTS::GetRDGValues () - semaState.Release(1)");
                }
            }
        }

        public override void GetRDGValues(int /*TYPE_FIELDS*/ mode, int indx, DateTime date)
        {
            //Запретить запись ПБР-значений
            if (m_arMarkSavePPBRValues[(int)INDEX_MARK_PPBRVALUES.ENABLED] == true) m_arMarkSavePPBRValues[(int)INDEX_MARK_PPBRVALUES.MARK] = false; else ;
            
            lock (m_lockObj)
            {
                indxTECComponents = indx;
                
                ClearValues();

                using_date = false;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_prevDate = date.Date;
                m_curDate = m_prevDate;

                m_typeFields = (TYPE_FIELDS)mode;

                newState = true;
                states.Clear();
                states.Add((int)StatesMachine.PPBRValues);
                states.Add((int)StatesMachine.AdminValues);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, "AdminTS::GetRDGValues () - semaState.Release(1)");
                }
            }
        }

        protected override void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode)
        {
            Request(t.m_arIdListeners[(int)CONN_SETT_TYPE.PBR], t.GetPBRValueQuery(comp, date, mode));
        }

        private void GetAdminValuesRequest(TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode) {
            Request(t.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], t.GetAdminValueQuery(comp, date, mode));
        }

        public virtual void ImpRDGExcelValues(int indx, DateTime date)
        {
            lock (m_lockObj)
            {
                indxTECComponents = indx;
                
                ClearValues();

                using_date = false;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_prevDate = date.Date;
                m_curDate = m_prevDate;

                newState = true;
                states.Clear();
                states.Add((int)StatesMachine.ImpRDGExcelValues);

                try
                {
                    semaState.Release(1);
                }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, "AdminTS::ImpRDGExcelValues () - semaState.Release(1)");
                }
            }
        }

        public virtual HAdmin.Errors ExpRDGExcelValues(int indx, DateTime date)
        {
            delegateStartWait();
            Logging.Logg().LogDebugToFile("AdminTS::SaveChanges () - delegateStartWait() - Интервал ожидания для semaDBAccess=" + DbInterface.MAX_WATING);

            //if (semaDBAccess.WaitOne(6666) == true) {
            if (semaDBAccess.WaitOne(DbInterface.MAX_WATING) == true)
            {
                lock (m_lockObj)
                {
                    indxTECComponents = indx;

                    saveResult = Errors.NoAccess;
                    saving = true;
                    using_date = false;
                    m_curDate = m_prevDate;

                    newState = true;
                    states.Clear();

                    states.Add((int)StatesMachine.ExpRDGExcelValues);

                    try
                    {
                        semaState.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().LogExceptionToFile(e, "AdminTS::SaveRDGExcelValues () - semaState.Release(1)");
                    }
                }

                semaDBAccess.WaitOne();
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
                lock (m_lockObj)
                {
                    Logging.Logg().LogDebugToFile("AdminTS::ExpRDGExcelValues () - semaDBAccess.WaitOne()=false");
                    
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

        protected virtual bool GetAdminValuesResponse(DataTable tableAdminValuesResponse, DateTime date)
        {
            DataTable table = null;
            DataTable[] arTable = { m_tablePPBRValuesResponse, tableAdminValuesResponse };
            int [] arIndexTables = {0, 1},
                arFieldsCount = {-1, -1};

            int i = -1, j = -1, k = -1,
                hour = -1;

            //int offsetPBR_NUMBER = m_tablePPBRValuesResponse.Columns.IndexOf ("PBR_NUMBER");
            //if (offsetPBR_NUMBER > 0) offsetPBR_NUMBER = 0; else ;

            int offsetPBR = m_tablePPBRValuesResponse.Columns.IndexOf("PBR"),
                offsetPBRNumber = -1;
            if (offsetPBR > 0) offsetPBR = 0; else ;

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
                    catch (ArgumentException e) {
                        Logging.Logg().LogExceptionToFile(e, "Remove(\"ID_COMPONENT\")");
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
            
            //0 - DATE_ADMIN, 1 - REC, 2 - IS_PER, 3 - DIVIAT, 4 - DATE_PBR, 5 - PBR, 6 - PBR_NUMBER
            for (i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i][0] is System.DBNull)
                {
                    try
                    {
                        hour = ((DateTime)table.Rows[i]["DATE_PBR"]).Hour;
                        if (hour == 0 && ((DateTime)table.Rows[i]["DATE_PBR"]).Day != date.Day)
                            hour = 24;
                        else
                            if (hour == 0)
                                continue;
                            else
                                ;

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

                        m_curRDGValues[hour - 1].recomendation = 0;
                        m_curRDGValues[hour - 1].deviationPercent = false;
                        m_curRDGValues[hour - 1].deviation = 0;
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        hour = ((DateTime)table.Rows[i]["DATE_ADMIN"]).Hour;
                        if (hour == 0 && ((DateTime)table.Rows[i]["DATE_ADMIN"]).Day != date.Day)
                            hour = 24;
                        else
                            if (hour == 0)
                                continue;
                            else
                                ;

                        m_curRDGValues[hour - 1].recomendation = (double)table.Rows[i][arIndexTables[1] * arFieldsCount [0] + 1 /*+ offsetPBR_NUMBER*/ /*+ offsetPBR*/ /*"REC"*/];
                        m_curRDGValues[hour - 1].deviationPercent = (int)table.Rows[i][arIndexTables[1] * arFieldsCount[0] + 2 /*+ offsetPBR_NUMBER*/ /*+ offsetPBR*/ /*"IS_PER"*/] == 1;
                        m_curRDGValues[hour - 1].deviation = (double)table.Rows[i][arIndexTables[1] * arFieldsCount[0] + 3 /*+ offsetPBR_NUMBER*/ /*+ offsetPBR*/ /*"DIVIAT"*/];
                        if ((!(table.Rows[i]["DATE_PBR"] is System.DBNull)) && (offsetPBR == 0)) {
                            //for (j = 0; j < 3 /*4 для SN???*/; j ++)
                            //{
                                j = 0;
                                if (! (table.Rows[i][arIndexTables[0] * arFieldsCount[1] + (j + 1)/* + offsetPBR_NUMBER*//*"PBR"*/] is DBNull))
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
                        else {
                            m_curRDGValues[hour - 1].pbr =
                            m_curRDGValues[hour - 1].pmin = m_curRDGValues[hour - 1].pmax = 
                            0.0;
                        }
                    }
                    catch { }
                }

                if (! (offsetPBRNumber < 0))
                    m_curRDGValues[hour - 1].pbr_number = table.Rows[i]["PBR_NUMBER"].ToString ();
                else
                    m_curRDGValues[hour - 1].pbr_number = GetPBRNumber (hour - 1);
            }

            return true;
        }

        protected void setRDGExcelValuesItem(out RDGStruct item, int iRows)
        {
            int j = -1;
            item = new RDGStruct();

            for (j = 0; j < allTECComponents[indxTECComponents].TG.Count; j++)
                if (allTECComponents[indxTECComponents].TG[j].m_indx_col_rdg_excel > 1)
                    item.pbr += (double)m_tableRDGExcelValuesResponse.Rows[iRows][allTECComponents[indxTECComponents].TG[j].m_indx_col_rdg_excel - 1];
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
                Request(allTECComponents[indxTECComponents].tec.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], allTECComponents[indxTECComponents].tec.GetAdminDatesQuery(date, m_typeFields, allTECComponents[indxTECComponents]));
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

            if (IsCanUseTECComponents ())
                //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, allTECComponents[indxTECComponents].tec.GetPBRDatesQuery(date));
                Request(allTECComponents[indxTECComponents].tec.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], allTECComponents[indxTECComponents].tec.GetPBRDatesQuery(date, m_typeFields, allTECComponents[indxTECComponents]));
            else
                ;
        }

        private void ClearAdminDates()
        {
            ClearDates(CONN_SETT_TYPE.ADMIN);
        }

        protected virtual bool GetDatesResponse(CONN_SETT_TYPE type, DataTable table, DateTime date)
        {
            for (int i = 0, hour; i < table.Rows.Count; i++)
            {
                try
                {
                    hour = ((DateTime)table.Rows[i][0]).Hour;
                    if ((hour == 0) && (!(((DateTime)table.Rows[i][0]).Day == date.Day)))
                        hour = 24;
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

        protected virtual string [] setAdminValuesQuery(TEC t, TECComponent comp, DateTime date)
        {
            string[] resQuery = new string[(int)DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE] { string.Empty, string.Empty, string.Empty };
            
            int currentHour = -1;

            date = date.Date;

            if ((serverTime.Date < date) || (m_ignore_date == true))
                currentHour = 0;
            else
                currentHour = serverTime.Hour;

            //Возможность изменять рекомендации за тек./час
            if (currentHour > 0)
                currentHour --;
            else
                ;
                

            for (int i = currentHour; i < 24; i++)
            {
                // запись для этого часа имеется, модифицируем её
                if (IsHaveDates(CONN_SETT_TYPE.ADMIN, i) == true)
                {
                    switch (m_typeFields)
                    {
                        case AdminTS.TYPE_FIELDS.STATIC:
                            //name = t.NameFieldOfAdminRequest(comp);
                            string name = t.NameFieldOfAdminRequest(comp);
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + t.m_arNameTableAdminValues[(int)m_typeFields] + " SET " + name + @"_REC='" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + name + @"_IS_PER=" + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                        @", " + name + "_DIVIAT='" + m_curRDGValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'" +
                                        @" AND " +
                                        @"ID = " + m_arHaveDates[(int)CONN_SETT_TYPE.ADMIN, i] +
                                        @"; ";
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + t.m_arNameTableAdminValues[(int)m_typeFields] + " SET " + @"REC='" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + @"IS_PER=" + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                        @", " + "DIVIAT='" + m_curRDGValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'" +
                                        @" AND ID_COMPONENT = " + comp.m_id +
                                        @" AND " +
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
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                        @", '" + m_curRDGValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"'),";
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                        @", '" + m_curRDGValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + (comp.m_id) +
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
            //@"DATE > '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
            //@"' AND DATE <= '" + date.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss") +
            //@"';";

            return resQuery;
        }
        
        protected virtual void SetAdminValuesRequest(TEC t, TECComponent comp, DateTime date)
        {
            Logging.Logg().LogDebugToFile("AdminTS::SetAdminValuesRequest ()");

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
                                @") VALUES" + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Substring(0, query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Length - 1) + ";";
                        break;
                    default:
                        break;
                }
            }
            else
                ;

            Request(t.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]);
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
                                        @"DATE = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            query[(int)DbTSQLInterface.QUERY_TYPE.DELETE] += @"DELETE FROM " + t.m_arNameTableAdminValues[(int)m_typeFields] +
                                        @" WHERE " +
                                        @"DATE = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
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

            Logging.Logg().LogDebugToFile("AdminTS::ClearAdminValuesRequest ()");

            //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, requestUpdate + requestInsert + requestDelete);
            Request(t.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]);
        }

        protected virtual string[] setPPBRQuery(TEC t, TECComponent comp, DateTime date)
        {
            string[] resQuery = new string[(int)DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE] { string.Empty, string.Empty, string.Empty };

            int currentHour = -1;

            date = date.Date;

            if ((serverTime.Date < date) || (m_ignore_date == true))
                currentHour = 0;
            else
                currentHour = serverTime.Hour; // - (int)HAdmin.GetOffsetOfCurrentTimeZone ().TotalHours;

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
                                        @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";*/
                            if (t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR].Equals (string.Empty) == false)  {
                                resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + t.m_arNameTableUsedPPBRvsPBR[(int)m_typeFields] + " SET " +
                                @"PBR_NUMBER='";
                                if ((! (m_curRDGValues[i].pbr_number == null)) && (m_curRDGValues[i].pbr_number.Length > 3))
                                    resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += m_curRDGValues[i].pbr_number;
                                else
                                    resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += GetPBRNumber();
                                resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"'" +
                                            @", " + name + @"_" + t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR] + "='" + m_curRDGValues[i].pbr.ToString("F1", CultureInfo.InvariantCulture) +
                                            @"' WHERE " +
                                            t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_DATETIME] + @" = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                            @"'" +
                                            @" AND " +
                                            @"ID = " + m_arHaveDates[(int)CONN_SETT_TYPE.PBR, i] +
                                            @"; ";
                            }
                            else
                                ;
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + t.m_arNameTableUsedPPBRvsPBR[(int)m_typeFields] +
                                        " SET " +
                                        @"PBR_NUMBER='";
                            if ((! (m_curRDGValues[i].pbr_number == null)) && (m_curRDGValues[i].pbr_number.Length > 3))
                                resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += m_curRDGValues[i].pbr_number;
                            else
                                resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += GetPBRNumber();
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += "'" +
                                        @", PBR='" + m_curRDGValues[i].pbr.ToString("F2", CultureInfo.InvariantCulture) + "'" +
                                        @", Pmin='" + m_curRDGValues[i].pmin.ToString("F2", CultureInfo.InvariantCulture) + "'" +
                                        @", Pmax='" + m_curRDGValues[i].pbr.ToString("F2", CultureInfo.InvariantCulture) + "'" +
                                        @" WHERE " +
                                        @"DATE_TIME" + @" = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'" +
                                        @" AND ID_COMPONENT = " + comp.m_id +
                                        @" AND " +
                                        @"ID = " + m_arHaveDates[(int)CONN_SETT_TYPE.PBR, i] +
                                        @"; ";
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
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + serverTime.Date.ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + strPBRNumber +
                                        @"', '" + "0" +
                                        @"', '" + m_curRDGValues[i].pbr.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"'),";
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + serverTime.ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + strPBRNumber +
                                        @"', " + comp.m_id +
                                        @", '" + "0" + "'" +
                                        @", '" + m_curRDGValues[i].pbr.ToString("F1", CultureInfo.InvariantCulture) + "'" +
                                        @", '" + m_curRDGValues[i].pmin.ToString("F1", CultureInfo.InvariantCulture) + "'" +
                                        @", '" + m_curRDGValues[i].pmax.ToString("F1", CultureInfo.InvariantCulture) + "'" +
                                        @"),";
                            break;
                        default:
                            break;
                    }
                }
            }
            
            return resQuery;
        }

        protected virtual void SetPPBRRequest(TEC t, TECComponent comp, DateTime date)
        {
            string[] query = setPPBRQuery(t, comp, date);

            // добавляем все записи, не найденные в базе
            if (!(query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] == ""))
            {
                switch (m_typeFields)
                {
                    case AdminTS.TYPE_FIELDS.STATIC:
                        string name = t.NameFieldOfPBRRequest(comp);
                        query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = @"INSERT INTO " + t.m_arNameTableUsedPPBRvsPBR[(int)m_typeFields] + " (DATE_TIME, WR_DATE_TIME, PBR_NUMBER, IS_COMDISP, " + name + @"_PBR) VALUES" + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Substring(0, query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Length - 1) + ";";
                        break;
                    case AdminTS.TYPE_FIELDS.DYNAMIC:
                        query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = @"INSERT INTO " + t.m_arNameTableUsedPPBRvsPBR[(int)m_typeFields] + " (DATE_TIME, WR_DATE_TIME, PBR_NUMBER, ID_COMPONENT, OWNER, PBR, Pmin, Pmax) VALUES" + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Substring(0, query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Length - 1) + ";";
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
                                   //@"DATE_TIME > '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
                                   //@"' AND DATE_TIME <= '" + date.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss") +
                                   //@"';";

            Logging.Logg().LogDebugToFile("AdminTS::SetPPBRRequest ()");
            
            //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, requestUpdate + requestInsert + requestDelete);
            Request(t.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]);
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
                                        t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_DATETIME] + @" = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            query[(int)DbTSQLInterface.QUERY_TYPE.DELETE] += @"DELETE FROM " + t.m_arNameTableUsedPPBRvsPBR[(int)m_typeFields] +
                                        @" WHERE " +
                                        @"DATE_TIME" + @" = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
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

            Logging.Logg().LogDebugToFile("ClearPPBRRequest");

            //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, requestUpdate + requestInsert + requestDelete);
            Request(t.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]);
        }

        public void Request(int idListener, string request)
        {
            m_IdListenerCurrent = idListener;
            DbSources.Sources ().Request (idListener, request);
        }

        public override bool Response(int idListener, out bool error, out DataTable table/*, bool isTec*/)
        {
            return DbSources.Sources ().Response (idListener, out error, out table);
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
                foreach (TEC t in m_list_tec) {
                    t.StartDbInterfaces ();
                }
            else
                Logging.Logg().LogErrorToFile(@"AdminTS::Start () - m_list_tec == null");

            semaDBAccess = new Semaphore(1, 1);

            base.Start();
        }

        public override void Stop()
        {
            base.Stop();

            if (! (m_list_tec == null))
                foreach (TEC t in m_list_tec)
                {
                    t.StopDbInterfaces();
                }
            else
                Logging.Logg().LogErrorToFile(@"AdminTS::Stop () - m_list_tec == null");
        }

        protected override bool StateRequest(int /*StatesMachine*/ state)
        {
            bool result = true;
            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    ActionReport("Получение текущего времени сервера.");
                    GetCurrentTimeRequest();
                    break;
                case (int)StatesMachine.PPBRValues:
                    ActionReport("Получение данных плана.");
                    if (indxTECComponents < allTECComponents.Count)
                        GetPPBRValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate.Date, m_typeFields);
                    else
                        ; //result = false;
                    break;
                case (int)StatesMachine.AdminValues:
                    ActionReport("Получение административных данных.");
                    if (indxTECComponents < allTECComponents.Count)
                        GetAdminValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate.Date, m_typeFields);
                    else
                        ; //result = false;

                    //this.BeginInvoke(delegateCalendarSetDate, m_prevDatetime);
                    break;
                case (int)StatesMachine.ImpRDGExcelValues:
                    ActionReport("Импорт РДГ из Excel.");
                    delegateImportForeignValuesRequuest();
                    break;
                case (int)StatesMachine.ExpRDGExcelValues:
                    ActionReport("Экспорт РДГ в книгу Excel.");
                    delegateExportForeignValuesRequuest();
                    break;
                 case (int)StatesMachine.PPBRCSVValues:
                    ActionReport("Импорт из формата CSV.");
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
                    ActionReport("Получение списка сохранённых часовых значений.");
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
                    ActionReport("Получение списка сохранённых часовых значений.");
                    GetAdminDatesRequest(m_curDate);
                    break;
                case (int)StatesMachine.SaveAdminValues:
                    ActionReport("Сохранение административных данных.");
                    if (indxTECComponents < allTECComponents.Count)
                        SetAdminValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    else
                        ; //result = false;
                    break;
                case (int)StatesMachine.SavePPBRValues:
                    ActionReport("Сохранение ПЛАНА.");
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
                    ActionReport("Сохранение административных данных.");
                    if (indxTECComponents < allTECComponents.Count)
                        ClearAdminValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    else
                        ; //result = false;
                    break;
                case (int)StatesMachine.ClearPPBRValues:
                    ActionReport("Сохранение ПЛАНА.");
                    if (indxTECComponents < allTECComponents.Count)
                        ClearPPBRRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    else
                        ; //result = false;
                    break;
                default:
                    break;
            }

            Logging.Logg().LogDebugToFile(@"AdminTS::StateRequest () - state=" + state.ToString() + @" - вЫход...");

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
                    case (int)StatesMachine.CurrentTime:
                    case (int)StatesMachine.PPBRValues:
                    case (int)StatesMachine.AdminValues:
                    case (int)StatesMachine.PPBRDates:
                    case (int)StatesMachine.AdminDates:                    
                    case (int)StatesMachine.SaveAdminValues:
                    case (int)StatesMachine.SavePPBRValues:
                    //case (int)StatesMachine.UpdateValuesPPBR:
                    case (int)StatesMachine.ClearAdminValues:
                    case (int)StatesMachine.ClearPPBRValues:
                    //case (int)StatesMachine.GetPass:
                        bRes = DbSources.Sources ().Response(m_IdListenerCurrent, out error, out table/*, false*/);
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
                    result = GetAdminValuesResponse(table, m_curDate);
                    if (result == true)
                    {
                        fillData(m_prevDate);
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
                        fillData(m_prevDate);
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
                        fillData(m_prevDate);
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
                    result = GetAdminDatesResponse(table, m_curDate);
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
                            Logging.Logg().LogExceptionToFile(e, @"AdminTS::StateResponse () - semaDBAccess.Release(1) - StatesMachine.SavePPBRValues...");
                        }
                    else
                        ;
                    result = true;
                    if (result == true)
                    {
                        Logging.Logg().LogDebugToFile(@"AdminTS::StateResponse () - saveComplete is set=" + (saveComplete == null ? false.ToString () : true.ToString ()) + @" - вЫход...");

                        if (!(saveComplete == null))
                            saveComplete();
                        else
                            ;
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
                m_report.errored_state = m_report.actioned_state = false;
            else
                ;

            Logging.Logg().LogDebugToFile(@"AdminTS::StateResponse () - state=" + state.ToString() + @", result=" + result.ToString() + @" - вЫход...");

            return result;
        }

        protected override void StateErrors(int /*StatesMachine*/ state, bool response)
        {
            bool bClear = false;

            switch (state)
            {
                case (int)StatesMachine.CurrentTime:
                    if (response)
                    {
                        ErrorReport("Ошибка разбора текущего времени сервера. Переход в ожидание.");
                        if (saving)
                            saveResult = Errors.ParseError;
                        else
                            ;
                    }
                    else
                    {
                        ErrorReport("Ошибка получения текущего времени сервера. Переход в ожидание.");
                        if (saving)
                            saveResult = Errors.NoAccess;
                        else
                            ;
                    }

                    if (saving)
                    {
                        try
                        {
                            semaDBAccess.Release(1);
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().LogExceptionToFile(e, "AdminTS::StateErrors () - semaDBAccess.Release(1)");
                        }
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.PPBRValues:
                    if (response)
                        ErrorReport("Ошибка разбора данных плана. Переход в ожидание.");
                    else {
                        ErrorReport("Ошибка получения данных плана. Переход в ожидание.");

                        bClear = true;
                    }
                    break;
                case (int)StatesMachine.AdminValues:
                    if (response)
                        ErrorReport("Ошибка разбора административных данных. Переход в ожидание.");
                    else {
                        ErrorReport("Ошибка получения административных данных. Переход в ожидание.");

                        bClear = true;
                    }
                    break;
                case (int)StatesMachine.ImpRDGExcelValues:
                    ErrorReport("Ошибка импорта РДГ из книги Excel. Переход в ожидание.");

                    // ???
                    break;
                case (int)StatesMachine.ExpRDGExcelValues:
                    ErrorReport("Ошибка экспорта РДГ в книгу Excel. Переход в ожидание.");
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
                    ErrorReport("Ошибка импорта из формата CSV. Переход в ожидание.");

                    // ???
                    break;
                case (int)StatesMachine.PPBRDates:
                    if (response)
                    {
                        ErrorReport("Ошибка разбора сохранённых часовых значений (PPBR). Переход в ожидание.");
                        saveResult = Errors.ParseError;
                    }
                    else
                    {
                        ErrorReport("Ошибка получения сохранённых часовых значений (PPBR). Переход в ожидание.");
                        saveResult = Errors.NoAccess;
                    }
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().LogExceptionToFile(e, "AdminTS::StateErrors () - semaDBAccess.Release(1)");
                    }
                    break;
                case (int)StatesMachine.AdminDates:
                    if (response)
                    {
                        ErrorReport("Ошибка разбора сохранённых часовых значений (AdminValues). Переход в ожидание.");
                        saveResult = Errors.ParseError;
                    }
                    else
                    {
                        ErrorReport("Ошибка получения сохранённых часовых значений (AdminValues). Переход в ожидание.");
                        saveResult = Errors.NoAccess;
                    }
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().LogExceptionToFile(e, "AdminTS::StateErrors () - semaDBAccess.Release(1)");
                    }
                    break;
                case (int)StatesMachine.SaveAdminValues:
                    ErrorReport("Ошибка сохранения административных данных. Переход в ожидание.");
                    saveResult = Errors.NoAccess;
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().LogExceptionToFile(e, "AdminTS::StateErrors () - semaDBAccess.Release(1)");
                    }
                    break;
                case (int)StatesMachine.SavePPBRValues:
                    ErrorReport("Ошибка сохранения данных ПЛАНа. Переход в ожидание.");
                    saveResult = Errors.NoAccess;
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch
                    {
                    }
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
                    ErrorReport("Ошибка удаления административных данных. Переход в ожидание.");
                    break;
                case (int)StatesMachine.ClearPPBRValues:
                    ErrorReport("Ошибка удаления данных ПЛАНа. Переход в ожидание.");
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
        }

        public virtual void SaveRDGValues(/*TYPE_FIELDS mode, */int indx, DateTime date, bool bCallback)
        {
            lock (m_lockObj)
            {
                indxTECComponents = indx;
                m_prevDate = date.Date;
            }
            
            Errors resultSaving = SaveChanges();
            if (resultSaving == Errors.NoError)
            {
                if (bCallback)
                    lock (m_lockObj)
                    {
                        ClearValues();

                        //m_prevDate = date.Date;
                        m_curDate = m_prevDate;
                        using_date = false;

                        newState = true;
                        states.Clear();

                        Logging.Logg().LogDebugToFile("AdminTS::SaveRDGValues () - states.Clear()");

                        //states.Add((int)StatesMachine.CurrentTime);
                        states.Add((int)StatesMachine.PPBRValues);
                        states.Add((int)StatesMachine.AdminValues);

                        try
                        {
                            semaState.Release(1);
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().LogExceptionToFile(e, "AdminTS::SaveRDGValues () - semaState.Release(1)");
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
        //            Logging.Logg().LogToFile("SaveRDGValues () - states.Clear()", true, true, false);
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
                        //Logging.Logg().LogToFile("catch - SaveRDGValues () - semaState.Release(1)", true, true, false);
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
                lock (m_lockObj)
                {
                    ClearValues();

                    m_prevDate = date.Date;
                    m_curDate = m_prevDate;
                    using_date = false;

                    newState = true;
                    states.Clear();

                    //states.Add((int)StatesMachine.CurrentTime);
                    states.Add((int)StatesMachine.PPBRValues);
                    states.Add((int)StatesMachine.AdminValues);

                    try
                    {
                        semaState.Release(1);
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().LogExceptionToFile(e, "AdminTS::ClearRDGValues () - semaState.Release(1)");
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
                    m_list_tec[i].connSettings(StatisticCommon.InitTEC_200.getConnSettingsOfIdSource(idListaener, arIdSource[(int)TEC.TEC_TYPE.COMMON], -1, out err), (int)CONN_SETT_TYPE.ADMIN);
                    m_list_tec[i].connSettings(StatisticCommon.InitTEC_200.getConnSettingsOfIdSource(idListaener, arIdSource[(int)TEC.TEC_TYPE.COMMON], -1, out err), (int)CONN_SETT_TYPE.PBR);
                }
                else {
                    if (m_list_tec[i].type() == TEC.TEC_TYPE.BIYSK)
                    {
                        m_list_tec[i].connSettings(StatisticCommon.InitTEC_200.getConnSettingsOfIdSource(idListaener, arIdSource[(int)TEC.TEC_TYPE.BIYSK], -1, out err), (int)CONN_SETT_TYPE.ADMIN);
                        m_list_tec[i].connSettings(StatisticCommon.InitTEC_200.getConnSettingsOfIdSource(idListaener, arIdSource[(int)TEC.TEC_TYPE.BIYSK], -1, out err), (int)CONN_SETT_TYPE.PBR);
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
                        foreach (TG tg in comp.TG)
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
            for (int i = 0; i < 24; i++)
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
            for (int i = 0; i < 24; i++)
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

        public override void ClearValues()
        {
            for (int i = 0; i < 24; i++)
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
            for (int i = 0; i < 24; i++)
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
