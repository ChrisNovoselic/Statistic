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

        protected DataTable m_tablePPBRValuesResponse,
                    m_tableRDGExcelValuesResponse;

        protected enum StatesMachine
        {
            CurrentTime,
            AdminValues, //Получение административных данных
            PPBRValues,
            AdminDates, //Получение списка сохранённых часовых значений
            PPBRDates,
            RDGExcelValues,
            SaveAdminValues, //Сохранение административных данных
            SavePPBRValues, //Сохранение PPBR
            SaveRDGExcelValues,
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

        //public bool isActive;

        public AdminTS() : base ()
        {
        }

        protected override void Initialize () {
            base.Initialize ();

            //layoutForLoading = new LayoutData(1);
        }

        public virtual Errors SaveChanges()
        {
            delegateStartWait();
            semaDBAccess.WaitOne();
            lock (m_lockObj)
            {
                saveResult = Errors.NoAccess;
                saving = true;
                using_date = false;
                m_curDate = m_prevDate;

                newState = true;
                states.Clear();

                Logging.Logg().LogLock();
                Logging.Logg().LogToFile("SaveChanges () - states.Clear()", true, true, false);
                Logging.Logg().LogUnlock();

                states.Add((int)StatesMachine.CurrentTime);
                states.Add((int)StatesMachine.AdminDates);
                //??? Состояния позволяют НАЧать процесс разработки возможности редактирования ПЛАНа на вкладке 'Редактирование ПБР'
                states.Add((int)StatesMachine.PPBRDates);
                states.Add((int)StatesMachine.SaveAdminValues);
                states.Add((int)StatesMachine.SavePPBRValues);
                //states.Add((int)StatesMachine.UpdateValuesPPBR);

                try
                {
                    semaState.Release(1);
                }
                catch
                {
                    Logging.Logg().LogLock();
                    Logging.Logg().LogToFile("catch - SaveChanges () - semaState.Release(1)", true, true, false);
                    Logging.Logg().LogUnlock();
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
            delegateStopWait();
            saving = false;

            return saveResult;
        }

        protected virtual Errors ClearRDG()
        {
            Errors errClearResult;

            delegateStartWait();
            semaDBAccess.WaitOne();
            lock (m_lockObj)
            {
                errClearResult = Errors.NoError;
                using_date = false;
                m_curDate = m_prevDate;

                newState = true;
                states.Clear();

                Logging.Logg().LogLock();
                Logging.Logg().LogToFile("ClearRDG () - states.Clear()", true, true, false);
                Logging.Logg().LogUnlock();

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
                catch
                {
                    Logging.Logg().LogLock();
                    Logging.Logg().LogToFile("catch - ClearRDG () - semaState.Release(1)", true, true, false);
                    Logging.Logg().LogUnlock();
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

        public override void Resume()
        {
            base.Resume ();

            GetRDGValues (m_typeFields, indxTECComponents);
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

            InitDbInterfaces ();

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
                catch
                {
                    Logging.Logg().LogLock();
                    Logging.Logg().LogToFile("catch - Reinit () - semaState.Release(1)", true, true, false);
                    Logging.Logg().LogUnlock();
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

                Logging.Logg().LogLock();
                Logging.Logg().LogToFile("GetCurrentTime () - states.Clear()", true, true, false);
                Logging.Logg().LogUnlock();

                states.Add((int)StatesMachine.CurrentTime);

                try
                {
                    semaState.Release(1);
                }
                catch
                {
                    Logging.Logg().LogLock();
                    Logging.Logg().LogToFile("catch - GetCurrentTime () - semaState.Release(1)", true, true, false);
                    Logging.Logg().LogUnlock();
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
            if (IsCanUseTECComponents ())
                //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, "SELECT now()");
                Request(allTECComponents[indxTECComponents].tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], allTECComponents[indxTECComponents].tec.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], "SELECT now()");
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
                if (TimeZone.IsDaylightSavingTime(DateTime.Now, daylight))
                    serverTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(3 + 1);
                else
                    serverTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(3);
                ErrorReport("Ошибка получения текущего времени сервера. Используется локальное время.");
            }

            return true;
        }
        
        public virtual void GetRDGValues (TYPE_FIELDS mode, int indx) {
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
                catch
                {
                    Logging.Logg().LogLock();
                    Logging.Logg().LogToFile("catch - GetRDGValues () - semaState.Release(1)", true, true, false);
                    Logging.Logg().LogUnlock();
                }
            }
        }

        public override void GetRDGValues(int /*TYPE_FIELDS*/ mode, int indx, DateTime date)
        {
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
                    Logging.Logg().LogLock();
                    Logging.Logg().LogToFile("catch - GetRDGValues () - semaState.Release(1)", true, true, false);
                    Logging.Logg().LogToFile("Исключение обращения к переменной (semaState)", false, false, false);
                    Logging.Logg().LogToFile("Исключение " + e.Message, false, false, false);
                    Logging.Logg().LogToFile(e.ToString(), false, false, false);
                    Logging.Logg().LogUnlock();
                }
            }
        }

        protected override void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode)
        {
            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.PBR], t.m_arListenerIds[(int)CONN_SETT_TYPE.PBR], t.GetPBRValueQuery(comp, date, mode));
        }

        private void GetAdminValuesRequest(TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode) {
            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], t.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], t.GetAdminValueQuery(comp, date, mode));
        }

        public virtual void GetRDGExcelValues(int indx, DateTime date)
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
                states.Add((int)StatesMachine.RDGExcelValues);

                try
                {
                    semaState.Release(1);
                }
                catch
                {
                    Logging.Logg().LogLock();
                    Logging.Logg().LogToFile("catch - GetRDGExcelValues () - semaState.Release(1)", true, true, false);
                    Logging.Logg().LogUnlock();
                }
            }
        }

        private void GetRDGExcelValuesRequest () {
            int err = 0;
            string path_rdg_excel = allTECComponents[indxTECComponents].tec.m_path_rdg_excel;
            if (!(m_tableRDGExcelValuesResponse == null)) m_tableRDGExcelValuesResponse.Clear(); else ;
            
            delegateStartWait ();
            if ((IsCanUseTECComponents () == true) && (path_rdg_excel.Length > 0))
                m_tableRDGExcelValuesResponse = DbTSQLInterface.Select(path_rdg_excel + "\\" + m_curDate.Date.GetDateTimeFormats()[4] + ".xls",
                                                                        @"SELECT * FROM [Лист1$]", out err);
            else
                ;

            //Logging.Logg ().LogLock ();
            //Logging.Logg().LogToFile("Admin.cs - GetRDGExcelValuesRequest () - (path_rdg_excel = " + path_rdg_excel + ")", false, false, false);
            //Logging.Logg().LogUnlock();

            delegateStopWait();
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

            int offsetPBR = m_tablePPBRValuesResponse.Columns.IndexOf("PBR");
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
                        /*
                        Logging.Logg().LogLock();
                        Logging.Logg().LogToFile("Remove(\"ID_COMPONENT\")", true, true, false);
                        Logging.Logg().LogToFile("Ошибка " + e.Message, false, false, false);
                        Logging.Logg().LogToFile(e.ToString(), false, false, false);
                        Logging.Logg().LogUnlock();
                        */ 
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
            }

            return true;
        }

        protected void setRDGExcelValuesItem(out RDGStruct item, int iRows)
        {
            int j = -1;
            item = new RDGStruct();

            for (j = 0; j < allTECComponents[indxTECComponents].TG.Count; j++)
                item.pbr += (double)m_tableRDGExcelValuesResponse.Rows[iRows][allTECComponents[indxTECComponents].TG[j].m_indx_col_rdg_excel - 1];

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

        protected virtual bool GetRDGExcelValuesResponse()
        {
            bool bRes = IsCanUseTECComponents ();

            if (bRes) {
                int i = -1,
                    iTimeZoneOffset = allTECComponents[indxTECComponents].tec.m_timezone_offset_msc,
                    rowRDGExcelStart = 1 + iTimeZoneOffset,
                    hour = -1;

                if (m_tableRDGExcelValuesResponse.Rows.Count > 0) bRes = true; else ;

                if (bRes) {
                    for (i = rowRDGExcelStart; i < 24 + 1; i++)
                    {
                        hour = i - iTimeZoneOffset;
                        setRDGExcelValuesItem(out m_curRDGValues[hour - 1], i);
                    }

                    /*for (i = hour; i < 24 + 1; i++)
                    {
                        hour = i;

                        m_curRDGValues.plan[hour - 1] = 0;
                        m_curRDGValues.recommendations[hour - 1] = 0;
                        m_curRDGValues.deviationPercent[hour - 1] = false;
                        m_curRDGValues.diviation[hour - 1] = 0;
                    }*/
                }
                else
                    ;
            }
            else
                ;

            return bRes;
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
                Request(allTECComponents[indxTECComponents].tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], allTECComponents[indxTECComponents].tec.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], allTECComponents[indxTECComponents].tec.GetAdminDatesQuery(date, m_typeFields, allTECComponents[indxTECComponents]));
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
                Request(allTECComponents[indxTECComponents].tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], allTECComponents[indxTECComponents].tec.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], allTECComponents[indxTECComponents].tec.GetPBRDatesQuery(date, m_typeFields, allTECComponents[indxTECComponents]));
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

                    m_arHaveDates[(int)type, hour - 1] = true;

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

            for (int i = currentHour; i < 24; i++)
            {
                // запись для этого часа имеется, модифицируем её
                if (m_arHaveDates[(int)CONN_SETT_TYPE.ADMIN, i])
                {
                    switch (m_typeFields)
                    {
                        case AdminTS.TYPE_FIELDS.STATIC:
                            //name = t.NameFieldOfAdminRequest(comp);
                            string name = t.NameFieldOfAdminRequest(comp);
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + t.m_arNameTableAdminValues[(int)TYPE_FIELDS.STATIC] + " SET " + name + @"_REC='" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + name + @"_IS_PER=" + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                        @", " + name + "_DIVIAT='" + m_curRDGValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + t.m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + " SET " + @"REC='" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + @"IS_PER=" + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                        @", " + "DIVIAT='" + m_curRDGValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
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
            Logging.Logg().LogLock();
            Logging.Logg().LogToFile("SetAdminValuesRequest", true, true, false);
            Logging.Logg().LogUnlock();

            string[] query = setAdminValuesQuery(t, comp, date);

            // добавляем все записи, не найденные в базе
            if (! (query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] == ""))
            {
                switch (m_typeFields)
                {
                    case AdminTS.TYPE_FIELDS.STATIC:
                        string name = t.NameFieldOfAdminRequest(comp);
                        query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = @"INSERT INTO " + t.m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] + " (DATE, " + name + @"_REC" +
                                @", " + name + "_IS_PER" +
                                @", " + name + "_DIVIAT) VALUES" + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Substring(0, query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Length - 1) + ";";
                        break;
                    case AdminTS.TYPE_FIELDS.DYNAMIC:
                        query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = @"INSERT INTO " + t.m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + " (DATE, " + @"REC" +
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

            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], t.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]);
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
                if (m_arHaveDates[(int)CONN_SETT_TYPE.ADMIN, i])
                {
                    switch (m_typeFields)
                    {
                        case AdminTS.TYPE_FIELDS.STATIC:
                            string name = t.NameFieldOfAdminRequest(comp);
                            query[(int)DbTSQLInterface.QUERY_TYPE.DELETE] += @"DELETE FROM " + t.m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.STATIC] +
                                        @"' WHERE " +
                                        @"DATE = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            query[(int)DbTSQLInterface.QUERY_TYPE.DELETE] += @"DELETE FROM " + t.m_arNameTableAdminValues[(int)AdminTS.TYPE_FIELDS.DYNAMIC] +
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

            Logging.Logg().LogLock();
            Logging.Logg().LogToFile("ClearAdminValuesRequest", true, true, false);
            Logging.Logg().LogUnlock();

            //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, requestUpdate + requestInsert + requestDelete);
            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], t.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]);
        }

        protected int getPBRNumber(int hour)
        {
            int iNum = -1;

            switch (hour)
            {
                case 0:
                case 1:
                    iNum = 1;
                    break;
                case 2:
                case 3:
                    iNum = 3;
                    break;
                case 4:
                case 5:
                    iNum = 5;
                    break;
                case 6:
                case 7:
                    iNum = 7;
                    break;
                case 8:
                case 9:
                    iNum = 9;
                    break;
                case 10:
                case 11:
                    iNum = 11;
                    break;
                case 12:
                case 13:
                    iNum = 13;
                    break;
                case 14:
                case 15:
                    iNum = 15;
                    break;
                case 16:
                case 17:
                    iNum = 17;
                    break;
                case 18:
                case 19:
                    iNum = 19;
                    break;
                default:
                    iNum = 21;
                    break;
            }

            return iNum;
        }

        protected virtual string[] setPPBRQuery(TEC t, TECComponent comp, DateTime date)
        {
            string[] resQuery = new string[(int)DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE] { string.Empty, string.Empty, string.Empty };

            int currentHour = -1;

            date = date.Date;

            if ((serverTime.Date < date) || (m_ignore_date == true))
                currentHour = 0;
            else
                currentHour = serverTime.Hour;

            for (int i = currentHour; i < 24; i++)
            {
                // запись для этого часа имеется, модифицируем её
                if (m_arHaveDates[(int)CONN_SETT_TYPE.PBR, i])
                {
                    switch (m_typeFields)
                    {
                        case AdminTS.TYPE_FIELDS.STATIC:
                            string name = t.NameFieldOfPBRRequest(comp);
                            /*requestUpdate += @"UPDATE " + t.m_strUsedPPBRvsPBR + " SET " + name + @"_" + t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.REC] + "='" + m_curRDGValues[i].plan.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";*/
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + t.m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] + " SET " +
                                        name + @"_" + t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR] + "='" + m_curRDGValues[i].pbr.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += @"UPDATE " + t.m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.DYNAMIC] +
                                        " SET " +
                                        @"PBR='" + m_curRDGValues[i].pbr.ToString("F2", CultureInfo.InvariantCulture) + "'" +
                                        @", Pmin='" + m_curRDGValues[i].pmin.ToString("F2", CultureInfo.InvariantCulture) + "'" +
                                        @", Pmax='" + m_curRDGValues[i].pbr.ToString("F2", CultureInfo.InvariantCulture) + "'" +
                                        @" WHERE " +
                                        @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'" +
                                        @" AND ID_COMPONENT = " + comp.m_id + "; ";
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
                                        @"', '" + serverTime.Date.ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + "ПБР" + getPBRNumber(i) +
                                        @"', '" + "0" +
                                        @"', '" + m_curRDGValues[i].pbr.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"'),";
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            resQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + serverTime.ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + "ПБР" + getPBRNumber(i) +
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
                        query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = @"INSERT INTO " + t.m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] + " (DATE_TIME, WR_DATE_TIME, PBR_NUMBER, IS_COMDISP, " + name + @"_PBR) VALUES" + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Substring(0, query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Length - 1) + ";";
                        break;
                    case AdminTS.TYPE_FIELDS.DYNAMIC:
                        query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = @"INSERT INTO " + t.m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.DYNAMIC] + " (DATE_TIME, WR_DATE_TIME, PBR_NUMBER, ID_COMPONENT, OWNER, PBR, Pmin, Pmax) VALUES" + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Substring(0, query[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Length - 1) + ";";
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

            Logging.Logg().LogLock();
            Logging.Logg().LogToFile("Admin - SetPPBRRequest", true, true, false);
            Logging.Logg().LogUnlock();
            
            //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, requestUpdate + requestInsert + requestDelete);
            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], t.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]);
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
                if (m_arHaveDates[(int)CONN_SETT_TYPE.PBR, i])
                {
                    switch (m_typeFields)
                    {
                        case AdminTS.TYPE_FIELDS.STATIC:
                            string name = t.NameFieldOfPBRRequest(comp);
                            query[(int)DbTSQLInterface.QUERY_TYPE.DELETE] += @"DELETE FROM " + t.m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.STATIC] +
                                        @"' WHERE " +
                                        @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";
                            break;
                        case AdminTS.TYPE_FIELDS.DYNAMIC:
                            query[(int)DbTSQLInterface.QUERY_TYPE.DELETE] += @"DELETE FROM " + t.m_arNameTableUsedPPBRvsPBR[(int)AdminTS.TYPE_FIELDS.DYNAMIC] +
                                        @" WHERE " +
                                        @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
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

            Logging.Logg().LogLock();
            Logging.Logg().LogToFile("ClearPPBRRequest", true, true, false);
            Logging.Logg().LogUnlock();

            //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, requestUpdate + requestInsert + requestDelete);
            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], t.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], query[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + query[(int)DbTSQLInterface.QUERY_TYPE.INSERT] + query[(int)DbTSQLInterface.QUERY_TYPE.DELETE]);
        }

        public void Request(int indxDbInterface, int listenerId, string request)
        {
            m_indxDbInterfaceCurrent = indxDbInterface;
            m_listListenerIdCurrent[indxDbInterface] = listenerId;
            m_listDbInterfaces[indxDbInterface].Request(m_listListenerIdCurrent[indxDbInterface], request);
        }

        public override bool GetResponse(int indxDbInterface, int listenerId, out bool error, out DataTable table/*, bool isTec*/)
        {
            if ((!(m_indxDbInterfaceCurrent < 0)) && (m_listListenerIdCurrent.Count > 0) && (!(m_indxDbInterfaceCurrent < 0))) {
                //m_listListenerIdCurrent [m_indxDbInterfaceCurrent] = -1;
                //m_indxDbInterfaceCurrent = -1;
                ;
            }
            else
                ;

            return m_listDbInterfaces[indxDbInterface].GetResponse(listenerId, out error, out table);

            //if (isTec)
            //    return dbInterface.GetResponse(listenerIdTec, out error, out table);
            //else
            //    return dbInterface.GetResponse(listenerIdAdmin, out error, out table);
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

        protected override bool InitDbInterfaces () {
            bool bRes = true;

            m_listDbInterfaces.Clear ();

            m_listListenerIdCurrent.Clear();
            m_indxDbInterfaceCurrent = -1;

            Int16 connSettType = -1;
            Int16 dbType = -1; 
            foreach (TEC t in m_list_tec)
            {
                for (connSettType = (int)CONN_SETT_TYPE.ADMIN; connSettType < (int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; connSettType++) {
                    if ((m_ignore_connsett_data == true) && (connSettType == (int)CONN_SETT_TYPE.DATA))
                        continue;
                    else
                        ;

                    dbType = -1;

                    bool isAlready = false;

                    foreach (DbTSQLInterface dbi in m_listDbInterfaces) {
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

                    if (isAlready == false) {
                        switch (connSettType) {
                            case (int)CONN_SETT_TYPE.ADMIN:
                            case (int)CONN_SETT_TYPE.PBR:
                                dbType = (int)DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.MySQL;
                                break;
                            case (int)CONN_SETT_TYPE.DATA:
                                dbType = (int)DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.MSSQL;
                                break;
                            default:
                                break; 
                        }

                        if (! (dbType < 0)) {
                            m_listDbInterfaces.Add(new DbTSQLInterface((DbTSQLInterface.DB_TSQL_INTERFACE_TYPE)dbType, "Интерфейс MySQL-БД: Администратор"));
                            m_listListenerIdCurrent.Add (-1);

                            t.m_arIndxDbInterfaces[connSettType] = m_listDbInterfaces.Count - 1;
                            t.m_arListenerIds[connSettType] = m_listDbInterfaces[m_listDbInterfaces.Count - 1].ListenerRegister();

                            m_listDbInterfaces [m_listDbInterfaces.Count - 1].Start ();

                            m_listDbInterfaces[m_listDbInterfaces.Count - 1].SetConnectionSettings(t.connSetts[connSettType]);
                        }
                        else
                            ;
                    }
                    else
                        ;
                }
            }

            return bRes;
        }

        public override void StartThreadSourceData()
        {
            InitDbInterfaces ();

            semaDBAccess = new Semaphore(1, 1);

            base.StartThreadSourceData();
        }

        public override void StopThreadSourceData()
        {
            base.StopThreadSourceData();

            if (m_listDbInterfaces.Count > 0)
            {
                foreach (TEC t in m_list_tec)
                {
                    for (CONN_SETT_TYPE connSettType = CONN_SETT_TYPE.ADMIN; connSettType < CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; connSettType ++) {
                        if ((m_ignore_connsett_data == true) && (connSettType == CONN_SETT_TYPE.DATA))
                            continue;
                        else
                            ;

                        m_listDbInterfaces[t.m_arIndxDbInterfaces[(int)connSettType]].ListenerUnregister(t.m_arListenerIds[(int)connSettType]);
                    }
                }

                foreach (DbInterface dbi in m_listDbInterfaces)
                {
                    dbi.Stop ();
                }
            }
            else
                ;
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
                    GetPPBRValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate.Date, m_typeFields);
                    break;
                case (int)StatesMachine.AdminValues:
                    ActionReport("Получение административных данных.");
                    GetAdminValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate.Date, m_typeFields);
                    //this.BeginInvoke(delegateCalendarSetDate, m_prevDatetime);
                    break;
                case (int)StatesMachine.RDGExcelValues:
                    ActionReport("Импорт РДГ из Excel.");
                    GetRDGExcelValuesRequest();
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
                    SetAdminValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    break;
                case (int)StatesMachine.SavePPBRValues:
                    ActionReport("Сохранение ПЛАНА.");
                    SetPPBRRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
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
                    ClearAdminValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    break;
                case (int)StatesMachine.ClearPPBRValues:
                    ActionReport("Сохранение ПЛАНА.");
                    ClearPPBRRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    break;
                default:
                    break;
            }

            return result;
        }

        protected override bool StateCheckResponse(int /*StatesMachine*/ state, out bool error, out DataTable table)
        {
            bool bRes = false;

            error = true;
            table = null;

            if ((state == (int)StatesMachine.RDGExcelValues) || ((!(m_indxDbInterfaceCurrent < 0)) && (m_listListenerIdCurrent.Count > 0)))
            {
                switch (state)
                {
                    case (int)StatesMachine.RDGExcelValues:
                        if ((!(m_tableRDGExcelValuesResponse == null)) && (m_tableRDGExcelValuesResponse.Rows.Count > 24))
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
                        bRes = GetResponse(m_indxDbInterfaceCurrent, m_listListenerIdCurrent[m_indxDbInterfaceCurrent], out error, out table/*, false*/);
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
                    if (result)
                    {
                        if (using_date) {
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
                    if (result)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.AdminValues:
                    result = GetAdminValuesResponse(table, m_curDate);
                    if (result)
                    {
                        fillData(m_prevDate);
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.RDGExcelValues:
                    ActionReport("Импорт РДГ из Excel.");
                    //result = GetRDGExcelValuesResponse(table, m_curDate);
                    result = GetRDGExcelValuesResponse();
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
                    if (result)
                    {
                    }
                    else
                        ;
                    break;
                case (int)StatesMachine.AdminDates:
                    ClearAdminDates();
                    result = GetAdminDatesResponse(table, m_curDate);
                    if (result)
                    {
                    }
                    break;
                case (int)StatesMachine.SaveAdminValues:
                    saveResult = Errors.NoError;
                    //try { semaDBAccess.Release(1); }
                    //catch { }
                    result = true;
                    if (result) { }
                    else ;
                    break;
                case (int)StatesMachine.SavePPBRValues:
                    saveResult = Errors.NoError;
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch
                    {
                    }
                    result = true;
                    if (result)
                    {
                        if (!(saveComplete == null)) saveComplete(); else ;
                    }
                    break;
                //case (int)StatesMachine.LayoutGet:
                //    result = GetLayoutResponse(table, m_curDate);
                //    if (result)
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
                //    if (result)
                //    {
                //    }
                //    break;
                case (int)StatesMachine.ClearAdminValues:
                    result = true;
                    if (result) { }
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
                    if (result)
                    {
                    }
                    break;
                default:
                    break;
            }

            if (result == true)
                errored_state = actioned_state = false;
            else
                ;

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
                case (int)StatesMachine.RDGExcelValues:
                    ErrorReport("Ошибка импорта РДГ из книги Excel. Переход в ожидание.");

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

                        Logging.Logg().LogLock();
                        Logging.Logg().LogToFile("SaveRDGValues () - states.Clear()", true, true, false);
                        Logging.Logg().LogUnlock();

                        //states.Add((int)StatesMachine.CurrentTime);
                        states.Add((int)StatesMachine.PPBRValues);
                        states.Add((int)StatesMachine.AdminValues);

                        try
                        {
                            semaState.Release(1);
                        }
                        catch
                        {
                            Logging.Logg().LogLock();
                            Logging.Logg().LogToFile("catch - SaveRDGValues () - semaState.Release(1)", true, true, false);
                            Logging.Logg().LogUnlock();
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
                    catch
                    {
                        Logging.Logg().LogLock();
                        Logging.Logg().LogToFile("catch - ClearRDGValues () - semaState.Release(1)", true, true, false);
                        Logging.Logg().LogUnlock();
                    }
                }
            }
            else
            {
                MessageBox("Не удалось удалить значения РДГ, возможно отсутствует связь с базой данных.");
            }
        }

        public void ReConnSettingsRDGSource (ConnectionSettings connSett, int id_source) {
            for (int i = 0; i < m_list_tec.Count; i ++) {
                if (m_list_tec[i].type () == TEC.TEC_TYPE.COMMON) {
                    m_list_tec[i].connSettings(StatisticCommon.InitTEC.getConnSettingsOfIdSource(connSett, id_source), (int)CONN_SETT_TYPE.ADMIN);
                    m_list_tec[i].connSettings(StatisticCommon.InitTEC.getConnSettingsOfIdSource(connSett, id_source), (int)CONN_SETT_TYPE.PBR);
                }
                else {
                    if (m_list_tec[i].type() == TEC.TEC_TYPE.BIYSK)
                    {
                        m_list_tec[i].connSettings(StatisticCommon.InitTEC.getConnSettingsOfIdSource(connSett, 101), (int)CONN_SETT_TYPE.ADMIN);
                        m_list_tec[i].connSettings(StatisticCommon.InitTEC.getConnSettingsOfIdSource(connSett, 101), (int)CONN_SETT_TYPE.PBR);
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

        public int GetIdTECComponent(int indx = -1)
        {
            if (indx < 0) indx = indxTECComponents; else ;

            int iRes = -1;

            if (indx < allTECComponents.Count) iRes = allTECComponents[indx].m_id; else ;

            return iRes;
        }
        
        public string GetNameTECComponent(int indx = -1)
        {
            if (indx < 0) indx = indxTECComponents; else ;

            string strRes = string.Empty;

            if (indx < allTECComponents.Count) strRes = allTECComponents[indx].name; else ;

            return strRes;
        }

        public override void CopyCurToPrevRDGValues()
        {
            for (int i = 0; i < 24; i++)
            {
                m_prevRDGValues[i].pbr = Math.Round(m_curRDGValues[i].pbr, 2);
                m_prevRDGValues[i].pmin = Math.Round(m_curRDGValues[i].pmin, 2);
                m_prevRDGValues[i].pmax = Math.Round(m_curRDGValues[i].pmax, 2);
                m_prevRDGValues[i].recomendation = m_curRDGValues[i].recomendation;
                m_prevRDGValues[i].deviationPercent = m_curRDGValues[i].deviationPercent;
                m_prevRDGValues[i].deviation = m_curRDGValues[i].deviation;
            }
        }

        public override void getCurRDGValues(HAdmin source)
        {
            for (int i = 0; i < 24; i++)
            {
                m_curRDGValues[i].pbr = ((HAdmin)source).m_curRDGValues[i].pbr;
                m_curRDGValues[i].pmin = ((HAdmin)source).m_curRDGValues[i].pmin;
                m_curRDGValues[i].pmax = ((HAdmin)source).m_curRDGValues[i].pmax;
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
