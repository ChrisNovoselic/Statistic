using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Data.OleDb;
using System.IO;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Globalization;

namespace StatisticCommon
{
    public class Admin : object
    {
        public enum TYPE_FIELDS : uint {STATIC, DYNAMIC, COUNT_TYPE_FIELDS};
        
        public struct RDGStruct
        {
            public double plan;
            public double recomendation;
            public bool deviationPercent;
            public double deviation;
        }

        private struct TecPPBRValues
        {
            //public double[] SN;
            public double[] PBR;
            //public double[] Pmax;
            //public double[] Pmin;

            public TecPPBRValues(int t)
            {
                //this.SN = new double[25];
                this.PBR = new double[25];
                //this.Pmax = new double[24];
                //this.Pmin = new double[24];
            }
        }

        private MD5CryptoServiceProvider md5;

        private DelegateFunc delegateStartWait;
        private DelegateFunc delegateStopWait;
        private DelegateFunc delegateEventUpdate;

        private DelegateStringFunc errorReport;
        private DelegateStringFunc actionReport;

        //private DelegateFunc fillTECComponent = null;
        private DelegateDateFunction fillData = null;

        private DelegateDateFunction setDatetime;
        /*
        FormChangeMode.MODE_TECCOMPONENT m_modeTECComponent;
        //public int mode(int new_mode = (int) FormChangeMode.MODE_TECCOMPONENT.UNKNOWN)
        public int mode(FormChangeMode.MODE_TECCOMPONENT new_mode = FormChangeMode.MODE_TECCOMPONENT.UNKNOWN)
        {
            int prev_mode = (int) m_modeTECComponent;

            if (new_mode == FormChangeMode.MODE_TECCOMPONENT.UNKNOWN)
                ;
            else
                m_modeTECComponent = (FormChangeMode.MODE_TECCOMPONENT) new_mode;

            return prev_mode;
        }
        */
        
        public Admin.TYPE_FIELDS m_typeFields;

        public string getOwnerPass () {
            string[] ownersPass = { "диспетчера", "администратора", "ДИСа" };

            return ownersPass [m_idPass - 1];            
        }

        public enum DESC_INDEX : ushort { DATE_HOUR, PLAN, RECOMENDATION, DEVIATION_TYPE, DEVIATION, TO_ALL };

        public volatile RDGStruct[] m_prevRDGValues;
        public RDGStruct[] m_curRDGValues;

        public volatile List<TEC> m_list_tec;
        public volatile List<TECComponent> allTECComponents;
        public int indxTECComponents;

        private bool is_connection_error;
        private bool is_data_error;

        public volatile string last_error;
        public DateTime last_time_error;
        public volatile bool errored_state;

        public volatile string last_action;
        public DateTime last_time_action;
        public volatile bool actioned_state;

        public DateTime m_prevDate;
        private DateTime serverTime,
                        m_curDate;

        private Semaphore semaDBAccess;
        private volatile Errors saveResult;
        private volatile bool saving;

        private Semaphore semaGetPass;
        private Semaphore semaSetPass;
        private volatile Errors passResult;
        private volatile string passReceive;
        private volatile uint m_idPass;

        //private Semaphore semaLoadLayout;
        //private volatile Errors loadLayoutResult;
        //private LayoutData layoutForLoading;

        private Object m_lockObj;

        private Thread taskThread;
        private Semaphore semaState;
        public volatile bool threadIsWorking;
        private volatile bool newState;
        private volatile List<StatesMachine> states;

        private List <DbInterface> m_listDbInterfaces;
        private List <int> m_listListenerIdCurrent;
        private int m_indxDbInterfaceCurrent; //Индекс в списке 'm_listDbInterfaces'

        public ConnectionSettings connSettConfigDB;
        int m_indxDbInterfaceConfigDB,
            m_listenerIdConfigDB;

        DataTable m_tablePPBRValuesResponse,
                    m_tableRDGExcelValuesResponse;

        private enum StatesMachine
        {
            CurrentTime,
            AdminValues, //Получение административных данных
            PPBRValues,
            AdminDates, //Получение списка сохранённых часовых значений
            PPBRDates,
            RDGExcelValues,
            SaveAdminValues, //Сохранение административных данных
            SavePPBRValues, //Сохранение PPBR
            //UpdateValuesPPBR, //Обновление PPBR после 'SaveValuesPPBR'
            GetPass,
            SetPassInsert,
            SetPassUpdate,
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

        public enum Errors
        {
            NoError,
            InvalidValue,
            NoAccess,
            ParseError,
        }

        private volatile bool using_date;

        private bool[] adminDates;
        private bool[] PPBRDates;

        //Для особкнной ТЭЦ (Бийск)
        //private volatile DbDataInterface dataInterface;
        
        //private Thread dbThread;
        //private Semaphore sema;
        //private volatile bool workTread;
        //-------------------------

        private bool started;

        //public bool isActive;

        public Admin()
        {
            Initialize ();
        }

        private void Initialize () {
            //m_strUsedAdminValues = "AdminValuesNew";
            //m_strUsedPPBRvsPBR = "PPBRvsPBRnew";

            m_listDbInterfaces = new List<DbInterface>();
            m_listListenerIdCurrent = new List<int>();

            started = false;

            m_prevRDGValues = new RDGStruct[24];

            md5 = new MD5CryptoServiceProvider();

            is_data_error = is_connection_error = false;

            //TecView tecView = FormMain.selectedTecViews [FormMain.stclTecViews.SelectedIndex];

            //isActive = false;

            using_date = false;

            m_curRDGValues = new RDGStruct[24];

            adminDates = new bool[24];
            PPBRDates = new bool[24];

            //layoutForLoading = new LayoutData(1);

            allTECComponents = new List<TECComponent>();

            m_lockObj = new Object();

            semaDBAccess = new Semaphore(1, 1);
            semaGetPass = new Semaphore(1, 1);
            semaSetPass = new Semaphore(1, 1);
            //semaLoadLayout = new Semaphore(1, 1);

            //delegateFillData = new DelegateFunctionDate(FillData);

            states = new List<StatesMachine>();
        }

        public bool WasChanged()
        {
            for (int i = 0; i < 24; i++)
            {
                if (m_prevRDGValues[i].plan != m_curRDGValues[i].plan /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.PLAN].Value.ToString())*/)
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

        public Errors SaveChanges()
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

                states.Add(StatesMachine.CurrentTime);
                states.Add(StatesMachine.AdminDates);
                //??? Состояния позволяют НАЧать процесс разработки возможности редактирования ПЛАНа на вкладке 'Редактирование ПБР'
                states.Add(StatesMachine.PPBRDates);
                states.Add(StatesMachine.SaveAdminValues);
                states.Add(StatesMachine.SavePPBRValues);
                //states.Add(StatesMachine.UpdateValuesPPBR);

                try
                {
                    semaState.Release(1);
                }
                catch
                {
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

        private Errors ClearRDG()
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

                states.Add(StatesMachine.CurrentTime);
                states.Add(StatesMachine.AdminDates);
                //??? Состояния позволяют НАЧать процесс разработки возможности редактирования ПЛАНа на вкладке 'Редактирование ПБР'
                states.Add(StatesMachine.PPBRDates);
                states.Add(StatesMachine.ClearAdminValues);
                states.Add(StatesMachine.ClearPPBRValues);
                //states.Add(StatesMachine.UpdateValuesPPBR);

                try
                {
                    semaState.Release(1);
                }
                catch
                {
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

        public List <int>GetListIndexTECComponent (bool bDisp) {
            List <int>listIndex = new List <int> ();

            int indx = -1;
            foreach (TECComponent comp in allTECComponents) {
                indx ++;
                if (comp.m_id < 100) {
                }
                else
                    if ((comp.m_id < 500) && (bDisp == true)) {
                        listIndex.Add (indx);
                    }
                    else
                        if (comp.m_id < 1000) {
                        }
                        else
                            if ((comp.m_id < 10000) && (bDisp == false))
                            {
                                listIndex.Add(indx);
                            }
                            else
                                ;
            }

            return listIndex;
        }

        public void Start()
        {
            if (started)
                ;
            else
                started = true;

            GetRDGValues (m_typeFields, indxTECComponents);
        }

        public bool IsRDGExcel (int indx) {
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
                states.Add(StatesMachine.CurrentTime);

                try
                {
                    semaState.Release(1);
                }
                catch
                {
                }
            }
        }

        public void Stop()
        {
            if (!started)
                return;
            else
                ;

            started = false;
        }

        public void SetDelegateWait(DelegateFunc dStart, DelegateFunc dStop, DelegateFunc dStatus)
        {
            this.delegateStartWait = dStart;
            this.delegateStopWait = dStop;
            this.delegateEventUpdate = dStatus;
        }

        public void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fact)
        {
            this.errorReport = ferr;
            this.actionReport = fact;
        }

        public void SetDelegateData(DelegateDateFunction f) { fillData = f; }

        //public void SetDelegateTECComponent(DelegateFunc f) { fillTECComponent = f; }

        public void SetDelegateDatetime(DelegateDateFunction f) { setDatetime = f; }

        void MessageBox(string msg, MessageBoxButtons btn = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Error)
        {
            //MessageBox.Show(this, msg, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public bool SetPassword(string password, uint idPass)
        {
            m_idPass = idPass;

            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

            StringBuilder hashedString = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                hashedString.Append(hash[i].ToString("x2"));

            semaGetPass.WaitOne();
            lock (m_lockObj)
            {
                passResult = Errors.NoAccess;

                newState = true;
                states.Clear();
                states.Add(StatesMachine.GetPass);

                try
                {
                    semaState.Release(1);
                }
                catch
                {
                }
            }
            delegateStartWait();
            semaGetPass.WaitOne();
            try
            {
                semaGetPass.Release(1);
            }
            catch
            {
            }

            if (passResult != Errors.NoError)
            {
                delegateStopWait();
                
                //MessageBox.Show(this, "Ошибка получения пароля " + getOwnerPass () + ". Пароль не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Ошибка получения пароля " + getOwnerPass() + ". Пароль не сохранён.");
                
                return false;
            }


            semaSetPass.WaitOne();
            lock (m_lockObj)
            {
                passResult = Errors.NoAccess;

                newState = true;
                states.Clear();

                if (passReceive == null)
                    states.Add(StatesMachine.SetPassInsert);
                else
                    states.Add(StatesMachine.SetPassUpdate);

                if (password != "")
                    passReceive = hashedString.ToString();

                try
                {
                    semaState.Release(1);
                }
                catch
                {
                }
            }
            semaSetPass.WaitOne();
            try
            {
                semaSetPass.Release(1);
            }
            catch
            {
            }
            delegateStopWait();

            if (passResult != Errors.NoError)
            {
                //MessageBox.Show(this, "Ошибка сохранения пароля " + getOwnerPass () + ". Пароль не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Ошибка сохранения пароля " + getOwnerPass() + ". Пароль не сохранён.");                
                return false;
            }

            return true;
        }

        public Errors ComparePassword(string password, uint id)
        {
            if (password.Length < 1)
            {
                //MessageBox.Show(this, "Длина пароля меньше допустимой.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Длина пароля меньше допустимой.");

                return Errors.InvalidValue;
            }
            else
                ;
            
            m_idPass = id;

            string hashFromForm = "";
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

            StringBuilder hashedString = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                hashedString.Append(hash[i].ToString("x2"));


            delegateStartWait();
            semaGetPass.WaitOne();
            lock (m_lockObj)
            {
                passResult = Errors.NoAccess;

                newState = true;
                states.Clear();
                states.Add(StatesMachine.GetPass);

                try
                {
                    semaState.Release(1);
                }
                catch
                {
                }
            }
            semaGetPass.WaitOne();
            try
            {
                semaGetPass.Release(1);
            }
            catch
            {
            }

            //???
            //passResult = Errors.NoError;

            delegateStopWait();
            if (passResult != Errors.NoError)
            {                
                //MessageBox.Show(this, "Ошибка получения пароля " + getOwnerPass () + ".", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Ошибка получения пароля " + getOwnerPass() + ".");
                
                return Errors.ParseError;
            }

            if (passReceive == null)
            {
                //MessageBox.Show(this, "Пароль не установлен.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Пароль не установлен.");
                    
                return Errors.NoAccess;
            }
            else
            {
                hashFromForm = hashedString.ToString();
             
                if (hashFromForm != passReceive)
                {
                    //MessageBox.Show(this, "Пароль введён неверно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox("Пароль введён неверно.");
                    
                    return Errors.InvalidValue;
                }
                else
                    return Errors.NoError;
            }
        }

        public void ClearValues()
        {
            for (int i = 0; i < 24; i++)
            {
                m_curRDGValues[i].plan = m_curRDGValues[i].recomendation = m_curRDGValues[i].deviation = 0;
                m_curRDGValues[i].deviationPercent = false;
            }
            
            //FillOldValues();
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

                states.Add(StatesMachine.CurrentTime);

                try
                {
                    semaState.Release(1);
                }
                catch
                {
                }
            }
        }

        bool IsCanUseTECComponents () {
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

        private bool GetCurrentTimeResponse(DataTable table)
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

        public void GetRDGValues (TYPE_FIELDS mode, int indx) {
            lock (m_lockObj)
            {
                ClearValues();

                indxTECComponents = indx;
                using_date = true;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_typeFields = mode;

                newState = true;
                states.Clear();
                states.Add(StatesMachine.CurrentTime);
                states.Add(StatesMachine.PPBRValues);
                states.Add(StatesMachine.AdminValues);

                try
                {
                    semaState.Release(1);
                }
                catch
                {
                }
            }
        }

        public void GetRDGValues(TYPE_FIELDS mode, int indx, DateTime date)
        {
            lock (m_lockObj)
            {
                ClearValues();

                indxTECComponents = indx;
                using_date = false;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_prevDate = date;
                m_curDate = m_prevDate;

                m_typeFields = mode;

                newState = true;
                states.Clear();
                states.Add(StatesMachine.PPBRValues);
                states.Add(StatesMachine.AdminValues);

                try
                {
                    semaState.Release(1);
                }
                catch
                {
                }
            }
        }

        private void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date, Admin.TYPE_FIELDS mode)
        {
            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.PBR], t.m_arListenerIds[(int)CONN_SETT_TYPE.PBR], t.GetPBRValueQuery(comp, date, mode));
        }

        private void GetAdminValuesRequest(TEC t, TECComponent comp, DateTime date, Admin.TYPE_FIELDS mode) {
            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], t.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], t.GetAdminValueQuery(comp, date, mode));
        }

        public void GetRDGExcelValues(int indx, DateTime date)
        {
            lock (m_lockObj)
            {
                ClearValues();

                indxTECComponents = indx;
                using_date = false;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_prevDate = date;
                m_curDate = m_prevDate;

                newState = true;
                states.Clear();
                states.Add(StatesMachine.RDGExcelValues);

                try
                {
                    semaState.Release(1);
                }
                catch
                {
                }
            }
        }

        private void GetRDGExcelValuesRequest () {
            delegateStartWait ();
            if (IsCanUseTECComponents ())
                m_tableRDGExcelValuesResponse = DbInterface.Request(allTECComponents[indxTECComponents].tec.m_path_rdg_excel + "\\" + m_curDate.Date.GetDateTimeFormats()[4] + ".xls",
                                                                        @"SELECT * FROM [Лист1$]");
            else
                ;
            delegateStopWait();
        }

        private bool GetPPBRValuesResponse(DataTable table, DateTime date)
        {
            bool bRes = true;

            m_tablePPBRValuesResponse = table.Copy ();

            return bRes;
        }

        private bool GetAdminValuesResponse(DataTable tableAdminValuesResponse, DateTime date)
        {
            DataTable table = null;
            DataTable[] arTable = { m_tablePPBRValuesResponse, tableAdminValuesResponse };
            int [] arIndexTables = {0, 1};

            int i = -1, j = -1, k = -1,
                hour = -1;

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
            }

            if (arTable[0].Rows.Count < arTable[1].Rows.Count) {
                arIndexTables[0] = 1;
                arIndexTables[1] = 0;
            }
            else {
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
                    }
                    else
                        ;
                }
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

                        m_curRDGValues[hour - 1].plan = (double)table.Rows[i][arIndexTables[1] * 4 + 1/*"PBR"*/];
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

                        m_curRDGValues[hour - 1].recomendation = (double)table.Rows[i][arIndexTables[1] * 3 + 1/*"REC"*/];
                        m_curRDGValues[hour - 1].deviationPercent = (int)table.Rows[i][arIndexTables[1] * 3 + 2/*"IS_PER"*/] == 1;
                        m_curRDGValues[hour - 1].deviation = (double)table.Rows[i][arIndexTables[1] * 3 + 3/*"DIVIAT"*/];
                        if (!(table.Rows[i]["DATE_PBR"] is System.DBNull))
                            m_curRDGValues[hour - 1].plan = (double)table.Rows[i][arIndexTables[0] * 4 + 1/*"PBR"*/];
                        else
                            m_curRDGValues[hour - 1].plan = 0;
                    }
                    catch { }
                }
            }

            return true;
        }

        private bool GetRDGExcelValuesResponse()
        {
            bool bRes = IsCanUseTECComponents ();

            if (bRes) {
                int i = -1, j = -1,
                    iTimeZoneOffset = allTECComponents[indxTECComponents].tec.m_timezone_offset_msc,
                    rowRDGExcelStart = 1 + iTimeZoneOffset,
                    hour = -1;

                if (m_tableRDGExcelValuesResponse.Rows.Count > 0) bRes = true; else ;

                if (bRes) {
                    for (i = rowRDGExcelStart; i < 24 + 1; i++)
                    {
                        hour = i - iTimeZoneOffset;

                        for (j = 0; j < allTECComponents[indxTECComponents].TG.Count; j ++)
                            m_curRDGValues[hour - 1].plan += (double)m_tableRDGExcelValuesResponse.Rows[i][allTECComponents[indxTECComponents].TG[j].m_indx_col_rdg_excel - 1];
                        m_curRDGValues[hour - 1].recomendation = 0;
                        m_curRDGValues[hour - 1].deviationPercent = false;
                        m_curRDGValues[hour - 1].deviation = 0;
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

        private void GetAdminDatesRequest(DateTime date)
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

        private void GetPPBRDatesRequest(DateTime date)
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
            for (int i = 0; i < 24; i++)
                adminDates[i] = false;
        }

        private void ClearPPBRDates()
        {
            for (int i = 0; i < 24; i++)
                PPBRDates[i] = false;
        }

        private bool GetAdminDatesResponse(DataTable table, DateTime date)
        {
            for (int i = 0, hour; i < table.Rows.Count; i++)
            {
                try
                {
                    hour = ((DateTime)table.Rows[i][0]).Hour;
                    if (hour == 0 && ((DateTime)table.Rows[i][0]).Day != date.Day)
                        hour = 24;
                    else
                        ;

                    adminDates[hour - 1] = true;
                }
                catch { }
            }
            return true;
        }

        private bool GetPPBRDatesResponse(DataTable table, DateTime date)
        {
            for (int i = 0, hour; i < table.Rows.Count; i++)
            {
                try
                {
                    hour = ((DateTime)table.Rows[i][0]).Hour;
                    if (hour == 0 && ((DateTime)table.Rows[i][0]).Day != date.Day)
                        hour = 24;
                    else
                        ;

                    PPBRDates[hour - 1] = true;
                }
                catch { }
            }
            return true;
        }

        private void SetAdminValuesRequest(TEC t, TECComponent comp, DateTime date)
        {
            int currentHour = serverTime.Hour;

            date = date.Date;

            if (serverTime.Date < date)
                currentHour = 0;
            else
                ;

            string requestUpdate = string.Empty,
                    requestInsert = string.Empty,
                    name = string.Empty; //t.NameFieldOfAdminRequest(comp);

            for (int i = currentHour; i < 24; i++)
            {
                // запись для этого часа имеется, модифицируем её
                if (adminDates[i])
                {
                    switch (m_typeFields) {
                        case Admin.TYPE_FIELDS.STATIC:
                            //name = t.NameFieldOfAdminRequest(comp);
                            name = t.NameFieldOfAdminRequest(comp);
                            requestUpdate += @"UPDATE " + t.m_arNameTableAdminValues[(int)TYPE_FIELDS.STATIC] + " SET " + name + @"_REC='" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + name + @"_IS_PER=" + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                        @", " + name + "_DIVIAT='" + m_curRDGValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";
                            break;
                        case Admin.TYPE_FIELDS.DYNAMIC:
                            requestUpdate += @"UPDATE " + t.m_arNameTableAdminValues[(int)Admin.TYPE_FIELDS.DYNAMIC] + " SET " + @"REC='" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
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
                        case Admin.TYPE_FIELDS.STATIC:
                            requestInsert += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                        @", '" + m_curRDGValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"'),";
                            break;
                        case Admin.TYPE_FIELDS.DYNAMIC:
                            requestInsert += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
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

            // добавляем все записи, не найденные в базе
            if (requestInsert != "")
            {
                switch (m_typeFields)
                {
                    case Admin.TYPE_FIELDS.STATIC:
                        requestInsert = @"INSERT INTO " + t.m_arNameTableAdminValues[(int)Admin.TYPE_FIELDS.STATIC] + " (DATE, " + name + @"_REC" +
                                @", " + name + "_IS_PER" +
                                @", " + name + "_DIVIAT) VALUES" + requestInsert.Substring(0, requestInsert.Length - 1) + ";";
                        break;
                    case Admin.TYPE_FIELDS.DYNAMIC:
                        requestInsert = @"INSERT INTO " + t.m_arNameTableAdminValues[(int)Admin.TYPE_FIELDS.DYNAMIC] + " (DATE, " + @"REC" +
                                @", " + "IS_PER" +
                                @", " + "DIVIAT" +
                                @", " + "ID_COMPONENT" +
                                @") VALUES" + requestInsert.Substring(0, requestInsert.Length - 1) + ";";
                        break;
                    default:
                        break;
                }
            }
            else
                ;

            string requestDelete = string.Empty;
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

            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], t.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], requestUpdate + requestInsert + requestDelete);
        }

        private void ClearAdminValuesRequest(TEC t, TECComponent comp, DateTime date)
        {
            int currentHour = serverTime.Hour;

            date = date.Date;

            if (serverTime.Date < date)
                currentHour = 0;
            else
                ;

            string requestUpdate = string.Empty, requestInsert = string.Empty, requestDelete = string.Empty,
                    name = string.Empty; //t.NameFieldOfPBRRequest(comp);

            for (int i = currentHour; i < 24; i++)
            {
                // запись для этого часа имеется, модифицируем её
                if (adminDates[i])
                {
                    switch (m_typeFields)
                    {
                        case Admin.TYPE_FIELDS.STATIC:
                            name = t.NameFieldOfAdminRequest(comp);
                            requestDelete += @"DELETE FROM " + t.m_arNameTableAdminValues[(int)Admin.TYPE_FIELDS.STATIC] +
                                        @"' WHERE " +
                                        @"DATE = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";
                            break;
                        case Admin.TYPE_FIELDS.DYNAMIC:
                            requestDelete += @"DELETE FROM " + t.m_arNameTableAdminValues[(int)Admin.TYPE_FIELDS.DYNAMIC] +
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
            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], t.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], requestUpdate + requestInsert + requestDelete);
        }

        private int getPBRNumber(int hour)
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

        private void SetPPBRRequest(TEC t, TECComponent comp, DateTime date)
        {
            int currentHour = serverTime.Hour;

            date = date.Date;

            if (serverTime.Date < date)
                currentHour = 0;
            else
                ;

            string requestUpdate = "", requestInsert = "",
                    name = string.Empty; //t.NameFieldOfPBRRequest(comp);

            for (int i = currentHour; i < 24; i++)
            {
                // запись для этого часа имеется, модифицируем её
                if (PPBRDates[i])
                {
                    switch (m_typeFields)
                    {
                        case Admin.TYPE_FIELDS.STATIC:
                            name = t.NameFieldOfPBRRequest(comp);
                            /*requestUpdate += @"UPDATE " + t.m_strUsedPPBRvsPBR + " SET " + name + @"_" + t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.REC] + "='" + m_curRDGValues[i].plan.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";*/
                            requestUpdate += @"UPDATE " + t.m_arNameTableUsedPPBRvsPBR[(int)Admin.TYPE_FIELDS.STATIC] + " SET " + name + @"_" + t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR] + "='" + m_curRDGValues[i].plan.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";
                            break;
                        case Admin.TYPE_FIELDS.DYNAMIC:
                            requestUpdate += @"UPDATE " + t.m_arNameTableUsedPPBRvsPBR[(int)Admin.TYPE_FIELDS.DYNAMIC] + " SET " + @"PBR='" + m_curRDGValues[i].plan.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
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
                        case Admin.TYPE_FIELDS.STATIC:
                            requestInsert += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + serverTime.Date.ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + "ПБР" + getPBRNumber(i) +
                                        @"', '" + "0" +
                                        @"', '" + m_curRDGValues[i].plan.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"'),";
                            break;
                        case Admin.TYPE_FIELDS.DYNAMIC:
                            requestInsert += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + serverTime.Date.ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + "ПБР" + getPBRNumber(i) +
                                        @"', " + comp.m_id +
                                        @", '" + "0" +
                                        @"', " + m_curRDGValues[i].plan.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"),";
                            break;
                        default:
                            break;
                    }
                }
            }

            // добавляем все записи, не найденные в базе
            if (requestInsert != "")
            {
                switch (m_typeFields)
                {
                    case Admin.TYPE_FIELDS.STATIC:
                        requestInsert = @"INSERT INTO " + t.m_arNameTableUsedPPBRvsPBR[(int)Admin.TYPE_FIELDS.STATIC] + " (DATE_TIME, WR_DATE_TIME, PBR_NUMBER, IS_COMDISP, " + name + @"_PBR) VALUES" + requestInsert.Substring(0, requestInsert.Length - 1) + ";";
                        break;
                    case Admin.TYPE_FIELDS.DYNAMIC:
                        requestInsert = @"INSERT INTO " + t.m_arNameTableUsedPPBRvsPBR[(int)Admin.TYPE_FIELDS.DYNAMIC] + " (DATE_TIME, WR_DATE_TIME, PBR_NUMBER, ID_COMPONENT, OWNER, PBR) VALUES" + requestInsert.Substring(0, requestInsert.Length - 1) + ";";
                        break;
                    default:
                        break;
                }
            }
            else
                ;

            string requestDelete = @"";
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
            Logging.Logg().LogToFile("SetPPBRRequest", true, true, false);
            Logging.Logg().LogUnlock();
            
            //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, requestUpdate + requestInsert + requestDelete);
            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], t.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], requestUpdate + requestInsert + requestDelete);
        }

        private void ClearPPBRRequest(TEC t, TECComponent comp, DateTime date)
        {
            int currentHour = serverTime.Hour;

            date = date.Date;

            if (serverTime.Date < date)
                currentHour = 0;
            else
                ;

            string requestUpdate = string.Empty, requestInsert = string.Empty, requestDelete = string.Empty,
                    name = string.Empty; //t.NameFieldOfPBRRequest(comp);

            for (int i = currentHour; i < 24; i++)
            {
                // запись для этого часа имеется, модифицируем её
                if (PPBRDates[i])
                {
                    switch (m_typeFields)
                    {
                        case Admin.TYPE_FIELDS.STATIC:
                            name = t.NameFieldOfPBRRequest(comp);
                            requestDelete += @"DELETE FROM " + t.m_arNameTableUsedPPBRvsPBR[(int)Admin.TYPE_FIELDS.STATIC] +
                                        @"' WHERE " +
                                        @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";
                            break;
                        case Admin.TYPE_FIELDS.DYNAMIC:
                            requestDelete += @"DELETE FROM " + t.m_arNameTableUsedPPBRvsPBR[(int)Admin.TYPE_FIELDS.DYNAMIC] +
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
            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], t.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], requestUpdate + requestInsert + requestDelete);
        }

        private void GetPassRequest(uint id)
        {
            string request = "SELECT HASH FROM passwords WHERE ID_ROLE=" + id;
            
            Request(m_indxDbInterfaceConfigDB, m_listenerIdConfigDB, request);
        }

        private bool GetPassResponse(DataTable table)
        {
            if (table.Rows.Count != 0)
                try
                {
                    if (table.Rows[0][0] is System.DBNull)
                        passReceive = "";
                    else
                        passReceive = (string)table.Rows[0][0];
                }
                catch
                {
                    return false;
                }
            else
                passReceive = null;

            return true;
        }

        private void SetPassRequest(string password, uint id, bool insert)
        {
            string query = string.Empty;
            
            if (insert)
                switch (m_idPass) {
                    case 1:
                        query = "INSERT INTO passwords (ID_ROLE, HASH) VALUES (" + id + ", '" + password + "')";
                        break;
                    case 2:
                        query = "INSERT INTO passwords (ID_ROLE, HASH) VALUES (" + id + ", '" + password + "')";
                        break;
                    case 3:
                        query = "INSERT INTO passwords (ID_ROLE, HASH) VALUES (" + id + ", '" + password + "')";
                        break;
                    default:
                        break;
                }
            else {
                switch (m_idPass)
                {
                    case 1:
                        query = "UPDATE passwords SET HASH='" + password + "'";
                        break;
                    case 2:
                        query = "UPDATE passwords SET HASH='" + password + "'";
                        break;
                    case 3:
                        query = "UPDATE passwords SET HASH='" + password + "'";
                        break;
                    default:
                        break;
                }

                query += " WHERE ID_USER=ID_ROLE AND ID_ROLE=" + id;
            }

            Request(m_indxDbInterfaceConfigDB, m_listenerIdConfigDB, query);
        }

        private void ErrorReport(string error_string)
        {
            last_error = error_string;
            last_time_error = DateTime.Now;
            errored_state = true;
            errorReport (error_string);
        }

        private void ActionReport(string action_string)
        {
            last_action = action_string;
            last_time_action = DateTime.Now;
            actioned_state = true;

            //stsStrip.BeginInvoke(delegateEventUpdate);
            //delegateEventUpdate ();
            actionReport(action_string);
        }

        public void Request(int indxDbInterface, int listenerId, string request)
        {
            m_indxDbInterfaceCurrent = indxDbInterface;
            m_listListenerIdCurrent[indxDbInterface] = listenerId;
            m_listDbInterfaces[indxDbInterface].Request(m_listListenerIdCurrent[indxDbInterface], request);
        }

        public bool GetResponse(int indxDbInterface, int listenerId, out bool error, out DataTable table/*, bool isTec*/)
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

        public void InitTEC (ConnectionSettings connSett, FormChangeMode.MODE_TECCOMPONENT mode, bool bAll) {
            //connSettConfigDB = connSett;

            if (mode == FormChangeMode.MODE_TECCOMPONENT.UNKNOWN)
                this.m_list_tec = new InitTEC(connSett, bAll).tec;
            else {
                this.m_list_tec = new InitTEC(connSett, (short)mode, bAll).tec;
            }

            //comboBoxTecComponent.Items.Clear ();
            allTECComponents.Clear ();

            foreach (TEC t in this.m_list_tec)
            {
                if (t.list_TECComponents.Count > 0)
                    foreach (TECComponent g in t.list_TECComponents)
                    {
                        //comboBoxTecComponent.Items.Add(t.name + " - " + g.name);
                        allTECComponents.Add(g);
                    }
                else
                {
                    //comboBoxTecComponent.Items.Add(t.name);
                    allTECComponents.Add(t.list_TECComponents[0]);
                }
            }

            /*if (! (fillTECComponent == null))
                fillTECComponent ();
            else
                ;*/
        }

        public int GetIndexTECComponent (int indxTEC, int indxComp) {
            int iRes = -1;

            return iRes;
        }

        private bool InitDbInterfaces () {
            bool bRes = true;
            
            m_listDbInterfaces.Clear ();

            m_listListenerIdCurrent.Clear();
            m_indxDbInterfaceCurrent = -1;

            m_listDbInterfaces.Add(new DbInterface(DbInterface.DbInterfaceType.MySQL, "Интерфейс MySQL-БД: Конфигурация"));
            m_listListenerIdCurrent.Add(-1);

            m_indxDbInterfaceConfigDB = m_listDbInterfaces.Count - 1;
            m_listenerIdConfigDB = m_listDbInterfaces[m_indxDbInterfaceConfigDB].ListenerRegister();

            m_listDbInterfaces[m_listDbInterfaces.Count - 1].Start();

            m_listDbInterfaces[m_listDbInterfaces.Count - 1].SetConnectionSettings(connSettConfigDB);

            Int16 connSettType = -1; 
            foreach (TEC t in m_list_tec)
            {
                for (connSettType = (int)CONN_SETT_TYPE.ADMIN; connSettType < (int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; connSettType++) {
                    bool isAlready = false;

                    foreach (DbInterface dbi in m_listDbInterfaces) {
                        //if (! (t.connSetts [0] == cs))
                        //if (dbi.connectionSettings.Equals(t.connSetts[(int)CONN_SETT_TYPE.ADMIN]) == true)
                        if (dbi.connectionSettings == t.connSetts[connSettType])
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
                        m_listDbInterfaces.Add(new DbInterface(DbInterface.DbInterfaceType.MySQL, "Интерфейс MySQL-БД: Администратор"));
                        m_listListenerIdCurrent.Add (-1);

                        t.m_arIndxDbInterfaces[connSettType] = m_listDbInterfaces.Count - 1;
                        t.m_arListenerIds[connSettType] = m_listDbInterfaces[m_listDbInterfaces.Count - 1].ListenerRegister();

                        if (m_indxDbInterfaceConfigDB < 0) {
                            m_indxDbInterfaceConfigDB = m_listDbInterfaces.Count - 1;
                            m_listenerIdConfigDB = m_listDbInterfaces[m_indxDbInterfaceConfigDB].ListenerRegister();
                        }
                        else
                            ;

                        m_listDbInterfaces [m_listDbInterfaces.Count - 1].Start ();

                        m_listDbInterfaces[m_listDbInterfaces.Count - 1].SetConnectionSettings(t.connSetts[connSettType]);
                    }
                    else
                        ;
                }
            }

            return bRes;
        }

        public void StartDbInterface()
        {
            InitDbInterfaces ();

            threadIsWorking = true;

            taskThread = new Thread (new ParameterizedThreadStart(TecView_ThreadFunction));
            taskThread.Name = "Интерфейс к данным";
            taskThread.IsBackground = true;

            semaState = new Semaphore(1, 1);

            semaState.WaitOne();
            taskThread.Start();
        }

        public void StopDbInterface()
        {
            bool joined;
            threadIsWorking = false;
            lock (m_lockObj)
            {
                newState = true;
                states.Clear();
                errored_state = false;
            }

            if (taskThread.IsAlive)
            {
                try { semaState.Release(1); }
                catch { }

                joined = taskThread.Join(1000);
                if (!joined)
                    taskThread.Abort();
                else
                    ;
            }
            else ;

            if ((m_listDbInterfaces.Count > 0) && (!(m_indxDbInterfaceConfigDB < 0)) && (!(m_listenerIdConfigDB < 0)))
            {
                m_listDbInterfaces[m_indxDbInterfaceConfigDB].ListenerUnregister(m_listenerIdConfigDB);
                m_indxDbInterfaceConfigDB = -1;
                m_listenerIdConfigDB = -1;

                foreach (TEC t in m_list_tec)
                {
                    for (CONN_SETT_TYPE connSettType = CONN_SETT_TYPE.ADMIN; connSettType < CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; connSettType ++) {
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

        private bool StateRequest(StatesMachine state)
        {
            bool result = true;
            switch (state)
            {
                case StatesMachine.CurrentTime:
                    ActionReport("Получение текущего времени сервера.");
                    GetCurrentTimeRequest();
                    break;
                case StatesMachine.PPBRValues:
                    ActionReport("Получение данных плана.");
                    GetPPBRValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate.Date, m_typeFields);
                    break;
                case StatesMachine.AdminValues:
                    ActionReport("Получение административных данных.");
                    GetAdminValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate.Date, m_typeFields);
                    //this.BeginInvoke(delegateCalendarSetDate, m_prevDatetime);
                    break;
                case StatesMachine.RDGExcelValues:
                    ActionReport("Импорт РДГ из Excel.");
                    GetRDGExcelValuesRequest();
                    break;
                case StatesMachine.PPBRDates:
                    if (serverTime.Date > m_curDate.Date)
                    {
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
                case StatesMachine.AdminDates:
                    if (serverTime.Date > m_curDate.Date)
                    {
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
                case StatesMachine.SaveAdminValues:
                    ActionReport("Сохранение административных данных.");
                    SetAdminValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    break;
                case StatesMachine.SavePPBRValues:
                    ActionReport("Сохранение ПЛАНА.");
                    SetPPBRRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    break;
                //case StatesMachine.UpdateValuesPPBR:
                //    ActionReport("Обровление ПЛАНА.");
                //    break;
                case StatesMachine.GetPass:
                    ActionReport("Получение пароля " + getOwnerPass () + ".");

                    GetPassRequest(m_idPass);
                    break;
                case StatesMachine.SetPassInsert:
                    ActionReport("Сохранение пароля " + getOwnerPass() + ".");
                    
                    SetPassRequest(passReceive, m_idPass, true);
                    break;
                case StatesMachine.SetPassUpdate:
                    ActionReport("Обновление пароля " + getOwnerPass() + ".");

                    SetPassRequest(passReceive, m_idPass, false);
                    break;
                //case StatesMachine.LayoutGet:
                //    ActionReport("Получение административных данных макета.");
                //    GetLayoutRequest(m_curDate);
                //    break;
                //case StatesMachine.LayoutSet:
                //    ActionReport("Сохранение административных данных макета.");
                //    SetLayoutRequest(m_curDate);
                //    break;
                case StatesMachine.ClearAdminValues:
                    ActionReport("Сохранение административных данных.");
                    ClearAdminValuesRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    break;
                case StatesMachine.ClearPPBRValues:
                    ActionReport("Сохранение ПЛАНА.");
                    ClearPPBRRequest(allTECComponents[indxTECComponents].tec, allTECComponents[indxTECComponents], m_curDate);
                    break;
            }

            return result;
        }

        private bool StateCheckResponse(StatesMachine state, out bool error, out DataTable table)
        {
            bool bRes = false;
            
            if ((!(m_indxDbInterfaceCurrent < 0)) && (m_listListenerIdCurrent.Count > 0) && (! (m_indxDbInterfaceCurrent < 0)))
            {
                switch (state)
                {
                    case StatesMachine.RDGExcelValues:
                        error = false;
                        table = null;

                        bRes = true;
                        break;
                    case StatesMachine.CurrentTime:
                    case StatesMachine.PPBRValues:
                    case StatesMachine.AdminValues:
                    case StatesMachine.PPBRDates:
                    case StatesMachine.AdminDates:                    
                    case StatesMachine.SaveAdminValues:
                    case StatesMachine.SavePPBRValues:
                    //case StatesMachine.UpdateValuesPPBR:
                    case StatesMachine.ClearAdminValues:
                    case StatesMachine.ClearPPBRValues:
                    case StatesMachine.GetPass:
                        bRes = GetResponse(m_indxDbInterfaceCurrent, m_listListenerIdCurrent[m_indxDbInterfaceCurrent], out error, out table/*, false*/);
                        break;
                    case StatesMachine.SetPassInsert:
                    case StatesMachine.SetPassUpdate:
                    //case StatesMachine.LayoutGet:
                    //case StatesMachine.LayoutSet:
                        bRes = GetResponse(m_indxDbInterfaceCurrent, m_listListenerIdCurrent[m_indxDbInterfaceCurrent], out error, out table/*, true*/);
                        break;
                    default:
                        error = true;
                        table = null;
                        
                        bRes = false;
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

        private bool StateResponse(StatesMachine state, DataTable table)
        {
            bool result = false;
            switch (state)
            {
                case StatesMachine.CurrentTime:
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
                case StatesMachine.PPBRValues:
                    result = GetPPBRValuesResponse(table, m_curDate);
                    if (result)
                    {
                    }
                    else
                        ;
                    break;
                case StatesMachine.AdminValues:
                    result = GetAdminValuesResponse(table, m_curDate);
                    if (result)
                    {
                        fillData(m_prevDate);
                    }
                    else
                        ;
                    break;
                case StatesMachine.RDGExcelValues:
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
                case StatesMachine.PPBRDates:
                    ClearPPBRDates();
                    result = GetPPBRDatesResponse(table, m_curDate);
                    if (result)
                    {
                    }
                    else
                        ;
                    break;
                case StatesMachine.AdminDates:
                    ClearAdminDates();
                    result = GetAdminDatesResponse(table, m_curDate);
                    if (result)
                    {
                    }
                    break;
                case StatesMachine.SaveAdminValues:
                    saveResult = Errors.NoError;
                    //try { semaDBAccess.Release(1); }
                    //catch { }
                    result = true;
                    if (result) { }
                    else ;
                    break;
                case StatesMachine.SavePPBRValues:
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
                    }
                    break;
                //case StatesMachine.UpdateValuesPPBR:
                //    saveResult = Errors.NoError;
                //    try
                //    {
                //        semaDBAccess.Release(1);
                //    }
                //    catch
                //    {
                //    }
                //    result = true;
                //    if (result)
                //    {
                //    }
                //    break;
                case StatesMachine.GetPass:
                    result = GetPassResponse(table);
                    if (result)
                    {
                        passResult = Errors.NoError;
                        try
                        {
                            semaGetPass.Release(1);
                        }
                        catch
                        {
                        }
                    }
                    break;
                case StatesMachine.SetPassInsert:
                    passResult = Errors.NoError;
                    try
                    {
                        semaSetPass.Release(1);
                    }
                    catch
                    {
                    }
                    result = true;
                    if (result)
                    {
                    }
                    break;
                case StatesMachine.SetPassUpdate:
                    passResult = Errors.NoError;
                    try
                    {
                        semaSetPass.Release(1);
                    }
                    catch
                    {
                    }
                    result = true;
                    if (result)
                    {
                    }
                    break;
                //case StatesMachine.LayoutGet:
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
                //    if (result)
                //    {
                //    }
                //    break;
                case StatesMachine.ClearAdminValues:
                    result = true;
                    if (result) { }
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
                    result = true;
                    if (result)
                    {
                    }
                    break;
            }

            if (result)
                errored_state = actioned_state = false;
            else
                ;

            return result;
        }

        private void StateErrors(StatesMachine state, bool response)
        {
            bool bClear = false;
            
            switch (state)
            {
                case StatesMachine.CurrentTime:
                    if (response)
                    {
                        ErrorReport("Ошибка разбора текущего времени сервера. Переход в ожидание.");
                        if (saving)
                            saveResult = Errors.ParseError;
                    }
                    else
                    {
                        ErrorReport("Ошибка получения текущего времени сервера. Переход в ожидание.");
                        if (saving)
                            saveResult = Errors.NoAccess;
                    }
                    if (saving)
                    {
                        try
                        {
                            semaDBAccess.Release(1);
                        }
                        catch
                        {
                        }
                    }
                    break;
                case StatesMachine.PPBRValues:
                    if (response)
                        ErrorReport("Ошибка разбора данных плана. Переход в ожидание.");
                    else {
                        ErrorReport("Ошибка получения данных плана. Переход в ожидание.");

                        bClear = true;
                    }
                    break;
                case StatesMachine.AdminValues:
                    if (response)
                        ErrorReport("Ошибка разбора административных данных. Переход в ожидание.");
                    else {
                        ErrorReport("Ошибка получения административных данных. Переход в ожидание.");

                        bClear = true;
                    }
                    break;
                case StatesMachine.RDGExcelValues:
                    ErrorReport("Ошибка импорта РДГ из книги Excel. Переход в ожидание.");

                    // ???
                    break;
                case StatesMachine.PPBRDates:
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
                    catch
                    {
                    }
                    break;
                case StatesMachine.AdminDates:
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
                    catch
                    {
                    }
                    break;
                case StatesMachine.SaveAdminValues:
                    ErrorReport("Ошибка сохранения административных данных. Переход в ожидание.");
                    saveResult = Errors.NoAccess;
                    try
                    {
                        semaDBAccess.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                case StatesMachine.SavePPBRValues:
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
                //case StatesMachine.UpdateValuesPPBR:
                //    ErrorReport("Ошибка обновления данных ПЛАНа. Переход в ожидание.");
                //    saveResult = Errors.NoAccess;
                //    try
                //    {
                //        semaDBAccess.Release(1);
                //    }
                //    catch
                //    {
                //    }
                //    break;
                case StatesMachine.GetPass:
                    if (response)
                    {
                        ErrorReport("Ошибка разбора пароля " + getOwnerPass() + ". Переход в ожидание.");

                        passResult = Errors.ParseError;
                    }
                    else
                    {
                        ErrorReport("Ошибка получения пароля " + getOwnerPass() + ". Переход в ожидание.");

                        passResult = Errors.NoAccess;
                    }
                    try
                    {
                        semaGetPass.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                case StatesMachine.SetPassInsert:
                case StatesMachine.SetPassUpdate:
                    ErrorReport("Ошибка сохранения пароля " + getOwnerPass() + ". Переход в ожидание.");

                    passResult = Errors.NoAccess;
                    try
                    {
                        semaSetPass.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                //case StatesMachine.LayoutGet:
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
                //case StatesMachine.LayoutSet:
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
                case StatesMachine.ClearAdminValues:
                    ErrorReport("Ошибка удаления административных данных. Переход в ожидание.");
                    break;
                case StatesMachine.ClearPPBRValues:
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

        private void TecView_ThreadFunction(object data)
        {
            int index;
            StatesMachine currentState;

            while (threadIsWorking)
            {
                semaState.WaitOne();

                index = 0;

                lock (m_lockObj)
                {
                    if (states.Count == 0)
                        continue;
                    currentState = states[index];
                    newState = false;
                }

                while (true)
                {
                    bool requestIsOk = true;
                    bool error = true;
                    bool dataPresent = false;
                    DataTable table = null;
                    for (int i = 0; i < DbInterface.MAX_RETRY && !dataPresent && !newState; i++)
                    {
                        if (error)
                        {
                            requestIsOk = StateRequest(currentState);
                            if (!requestIsOk)
                                break;
                            else
                                ;
                        }
                        else
                            ;

                        error = false;
                        for (int j = 0; j < DbInterface.MAX_WAIT_COUNT && !dataPresent && !error && !newState; j++)
                        {
                            System.Threading.Thread.Sleep(DbInterface.WAIT_TIME_MS);
                            dataPresent = StateCheckResponse(currentState, out error, out table);
                        }
                    }

                    if (requestIsOk)
                    {
                        bool responseIsOk = true;
                        if ((dataPresent == true) && (error == false) && (newState == false))
                            responseIsOk = StateResponse(currentState, table);
                        else
                            ;

                        if (((responseIsOk == false) || (dataPresent == false) || (error == true)) && (newState == false))
                        {
                            StateErrors(currentState, !responseIsOk);
                            lock (m_lockObj)
                            {
                                if (newState == false)
                                {
                                    states.Clear();
                                    break;
                                }
                                else
                                    ;
                            }
                        }
                        else
                            ;
                    }
                    else
                    {
                        lock (m_lockObj)
                        {
                            if (newState == false)
                            {
                                states.Clear();
                                break;
                            }
                            else
                                ;
                        }
                    }

                    index++;

                    lock (m_lockObj)
                    {
                        if (index == states.Count)
                            break;
                        else
                            ;

                        if (newState)
                            break;
                        else
                            ;
                        currentState = states[index];
                    }
                }
            }
            try
            {
                semaState.Release(1);
            }
            catch
            {
            }
        }

        public void SaveRDGValues(DateTime date)
        {
            Errors resultSaving;
            if ((resultSaving = SaveChanges()) == Errors.NoError)
            {
                lock (m_lockObj)
                {
                    ClearValues();

                    m_curDate = date;
                    using_date = false;

                    newState = true;
                    states.Clear();
                    states.Add(StatesMachine.CurrentTime);
                    states.Add(StatesMachine.PPBRValues);
                    states.Add(StatesMachine.AdminValues);

                    try
                    {
                        semaState.Release(1);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                if (resultSaving == Errors.InvalidValue)
                    //MessageBox.Show(this, "Изменение ретроспективы недопустимо!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    MessageBox("Изменение ретроспективы недопустимо!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                else
                    //MessageBox.Show(this, "Не удалось сохранить изменения, возможно отсутствует связь с базой данных.", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox("Не удалось сохранить изменения, возможно отсутствует связь с базой данных.");
            }
        }

        public void ClearRDGValues(DateTime date)
        {
            if (ClearRDG() == Errors.NoError)
            {
                lock (m_lockObj)
                {
                    ClearValues();

                    m_curDate = date;
                    using_date = false;

                    newState = true;
                    states.Clear();
                    states.Add(StatesMachine.CurrentTime);
                    states.Add(StatesMachine.PPBRValues);
                    states.Add(StatesMachine.AdminValues);

                    try
                    {
                        semaState.Release(1);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                MessageBox("Не удалось удалить значения РДГ, возможно отсутствует связь с базой данных.");
            }
        }

        public void CopyCurRDGValues () {
            for (int i = 0; i < 24; i++)
            {
                m_prevRDGValues[i].plan = m_curRDGValues[i].plan;
                m_prevRDGValues[i].recomendation = m_curRDGValues[i].recomendation;
                m_prevRDGValues[i].deviationPercent = m_curRDGValues[i].deviationPercent;
                m_prevRDGValues[i].deviation = m_curRDGValues[i].deviation;
            }
        }

        public void ReConnSettingsRDGSource (ConnectionSettings connSett, int id_source) {
            for (int i = 0; i < m_list_tec.Count; i ++) {
                m_list_tec[i].connSettings(StatisticCommon.InitTEC.getConnSettingsOfIdSource(connSett, id_source), (int)CONN_SETT_TYPE.ADMIN);
                m_list_tec[i].connSettings(StatisticCommon.InitTEC.getConnSettingsOfIdSource(connSett, id_source), (int)CONN_SETT_TYPE.PBR);
            }
        }
    }
}
