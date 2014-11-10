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
    public delegate void DelegateDateFunc(DateTime date);

    public abstract class HAdmin : object
    {
        public static int SEASON_BASE = 5;
        public enum seasonJumpE
        {
            None,
            WinterToSummer,
            SummerToWinter,
        }

        protected int GetSeasonValue (int h) {
            int iRes = SEASON_BASE;

            if (m_curDate.Date.Equals (HAdmin.SeasonDateTime.Date) == true) {
                    
            } else {
                if (m_curDate.Date.CompareTo (HAdmin.SeasonDateTime.Date) < 0) {
                    //ДО перехода
                    iRes += (int)seasonJumpE.SummerToWinter;
                } else {
                    //ПОСЛЕ перехода
                    iRes += (int)seasonJumpE.WinterToSummer;
                }
            }

            return iRes;
        }
        
        /// <summary>
        /// структура дл яхранения данных элемента (час) расписания диспетчерского графика (РДГ)
        /// </summary>
        public struct /*class*/ RDGStruct
        {
            //public double [] ppbr;
            public double pbr;
            public double pmin;
            public double pmax;
            public double recomendation;
            public bool deviationPercent;
            public double deviation;
            public bool fc;

            public string pbr_number;
        }

        public volatile RDGStruct[] m_prevRDGValues;
        public RDGStruct[] m_curRDGValues;

        protected DelegateFunc delegateStartWait;
        protected DelegateFunc delegateStopWait;
        protected DelegateFunc delegateEventUpdate;

        protected DelegateFunc errorReport;
        protected DelegateFunc actionReport;

        protected DelegateFunc saveComplete = null;
        protected DelegateDateFunc readyData = null;
        protected DelegateFunc errorData = null;

        protected DelegateDateFunc setDatetime;

        /// <summary>
        /// Список объектов 'TEC'
        /// </summary>
        public volatile List<StatisticCommon.TEC> m_list_tec;
        /// <summary>
        /// Список 
        /// </summary>
        public volatile List<TECComponent> allTECComponents;
        /// <summary>
        /// Текущий индекс компонента из списка 'allTECComponents' (для сохранения между вызовами функций)
        /// </summary>
        public int indxTECComponents;
        /// <summary>
        /// Текущий идентификатор соединения с БД (для сохранения между вызовами функций)
        /// </summary>
        protected int m_IdListenerCurrent;
        /// <summary>
        /// Хранения значений дыты/времени
        /// </summary>
        public DateTime m_prevDate
            , serverTime
            , m_curDate;

        protected volatile bool using_date;
        public bool m_ignore_date;
        /// <summary>
        /// Массив меток дат/времени, имеющихся в БД
        /// элементу, соответствующему часу, устанавливается значение идентификатора записи в таблице БД
        /// 1-я размерность - тип значений (ПБР, АДМИН), 2-я - идентификаторы записей
        /// </summary>
        protected int[,] m_arHaveDates;
        protected int m_iHavePBR_Number;
        /// <summary>
        /// Объект для синхронизации изменения списка состояний
        /// </summary>
        protected Object m_lockState;

        protected Thread taskThread;
        protected Semaphore semaState;
        public enum INDEX_WAITHANDLE_REASON { SUCCESS, ERROR, BREAK, COUNT_INDEX_WAITHANDLE_REASON }
        protected WaitHandle [] m_waitHandleState;
        //protected AutoResetEvent evStateEnd;
        public volatile int threadIsWorking;
        protected volatile bool newState;
        protected volatile List<int /*StatesMachine*/> states;

        protected Dictionary <int, int []> m_dictIdListeners;

        private bool actived;
        public bool m_bIsActive { get { return actived; } }

        private static int m_iSeasonAction;
        public static int SeasonAction {
            get { return m_iSeasonAction; } set { m_iSeasonAction = value; }
        }

        private static DateTime m_dtSeason;
        public static DateTime SeasonDateTime {
            get {
                return m_dtSeason;
            }

            set {
                m_dtSeason = value;
            }
        }

        public HAdmin()
        {
            m_IdListenerCurrent = -1;

            m_dictIdListeners = new Dictionary<int,int[]> ();

            m_iHavePBR_Number = -1;

            //m_iSeasonHour = -1;

            Initialize ();
        }

        protected virtual void Initialize () {
            actived = false;
            threadIsWorking = -1;

            using_date = false;
            m_ignore_date = false;
            //m_ignore_connsett_data = false;

            m_arHaveDates = new int[(int)CONN_SETT_TYPE.PBR + 1, 24];

            m_lockState = new Object();

            states = new List<int /*StatesMachine*/>();

            allTECComponents = new List<TECComponent>();

            m_curRDGValues = new RDGStruct[24];
            m_prevRDGValues = new RDGStruct[24];

            //for (int i = 0; i < 24; i++)
            //{
            //    m_curRDGValues[i].ppbr = new double[3 /*4 для SN???*/];
            //    m_prevRDGValues[i].ppbr = new double[3 /*4 для SN???*/];
            //}
        }

        /// <summary>
        /// Удалить ТЭЦ из списка по идентификатору
        /// </summary>
        /// <param name="id_tec">идентификатор ТЭЦ</param>
        public void RemoveTEC(int id_tec)
        {
            foreach (TEC t in this.m_list_tec) {
                if (t.m_id == id_tec) {
                    this.m_list_tec.Remove (t);
                    break;
                }
                else
                    ;
            }

            for (int i = 0; i < allTECComponents.Count; i ++) {
                if (allTECComponents [i].tec.m_id == id_tec)
                {
                    allTECComponents.RemoveAt (i);

                    i --;
                }
                else
                    ;
            }
        }

        public virtual void InitTEC(List <StatisticCommon.TEC> listTEC, HMark markQueries)
        {
            this.m_list_tec = new List<TEC> ();
            foreach (TEC t in listTEC)
            {
                //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == t.m_id))
                    this.m_list_tec.Add (t);
                //else ;
            }

            initTEC(markQueries);
        }

        public void InitTEC(int idListener, FormChangeMode.MODE_TECCOMPONENT mode, TYPE_DATABASE_CFG typeCfg, HMark markQueries, bool bIgnoreTECInUse)
        {
            //Logging.Logg().Debug("Admin::InitTEC () - вход...");

            //m_ignore_connsett_data = ! bUseData;

            if (!(idListener < 0))
                if (mode == FormChangeMode.MODE_TECCOMPONENT.UNKNOWN)
                    switch (typeCfg) {
                        case TYPE_DATABASE_CFG.CFG_190:
                            this.m_list_tec = new InitTEC_190(idListener, bIgnoreTECInUse, false).tec;
                            break;
                        case TYPE_DATABASE_CFG.CFG_200:
                            this.m_list_tec = new InitTEC_200(idListener, bIgnoreTECInUse, false).tec;
                            break;
                        default:
                            break;
                    }
                else
                    switch (typeCfg) {
                        case TYPE_DATABASE_CFG.CFG_190:
                            this.m_list_tec = new InitTEC_190(idListener, (short)mode, bIgnoreTECInUse, false).tec;
                            break;
                        case TYPE_DATABASE_CFG.CFG_200:
                            this.m_list_tec = new InitTEC_200(idListener, (short)mode, bIgnoreTECInUse, false).tec;
                            break;
                        default:
                            break;
                    }
            else
                this.m_list_tec = new List <TEC> ();

            initTEC(markQueries);
        }

        private void initTEC(HMark markQueries)
        {
            //comboBoxTecComponent.Items.Clear ();
            allTECComponents.Clear();

            foreach (StatisticCommon.TEC t in this.m_list_tec)
            {
                //Logging.Logg().Debug("Admin::InitTEC () - формирование компонентов для ТЭЦ:" + t.name);

                if (t.m_markQueries == null)
                    t.m_markQueries = markQueries;
                else
                    t.m_markQueries.Add (markQueries);

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

        public abstract bool WasChanged();

        public virtual void Activate(bool active)
        {
            if (active == true) threadIsWorking++; else ;

            if (actived == active)
            {
                return ;
            }
            else
            {
                actived = active;
            }
        }

        private void register(int id, ConnectionSettings connSett, string name, CONN_SETT_TYPE type)
        {
            m_dictIdListeners[id][(int)type] = DbSources.Sources().Register(connSett, true, @"ТЭЦ=" + name + @", DESC=" + type.ToString());
        }

        public void StartDbInterfaces()
        {
            if (!(m_list_tec == null))
                foreach (TEC t in m_list_tec)
                    if (!(t.connSetts == null))
                    {
                        CONN_SETT_TYPE i = CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE;

                        if (m_dictIdListeners.ContainsKey(t.m_id) == false) {
                            m_dictIdListeners.Add(t.m_id, new int[(int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE]);

                            for (i = CONN_SETT_TYPE.ADMIN; i < CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                                m_dictIdListeners[t.m_id][(int)i] = -1;
                        } else
                            ;

                        for (i = CONN_SETT_TYPE.ADMIN; i < CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                        {
                            if ((!(t.connSetts[(int)i] == null)) && (t.m_markQueries.IsMarked ((int)i) == true))
                            {
                                if (m_dictIdListeners[t.m_id][(int)i] < 0)
                                    ;
                                else
                                    DbSources.Sources().UnRegister(m_dictIdListeners[t.m_id][(int)i]);

                                register(t.m_id, t.connSetts[(int)i], t.name_shr, i);
                            }
                            else
                                ;
                        }

                        if (((t.m_markQueries.IsMarked ((int)CONN_SETT_TYPE.DATA_ASKUE) == true) || (t.m_markQueries.IsMarked ((int)CONN_SETT_TYPE.DATA_SOTIASSO) == true) || (t.m_markQueries.IsMarked ((int)CONN_SETT_TYPE.MTERM) == true)) &&
                            (t.m_bSensorsStrings == false))
                            t.InitSensorsTEC();
                        else
                            ;
                    }
                    else
                        //Вообще нельзя что-либо инициализировать
                        Logging.Logg().Error(@"HAdmin::StartDbInterfaces () - connSetts == null ...");
            else
                //Вообще нельзя что-либо инициализировать
                Logging.Logg().Error(@"HAdmin::StartDbInterfaces () - m_list_tec == null ...");
        }

        private void stopDbInterfaces()
        {
            if (!(m_list_tec == null))
                foreach (TEC t in m_list_tec)
                    for (int i = (int)CONN_SETT_TYPE.ADMIN; i < (int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                    {
                        if ((m_dictIdListeners.ContainsKey (t.m_id) == true) && (!(m_dictIdListeners[t.m_id][i] < 0)))
                        {
                            DbSources.Sources().UnRegister(m_dictIdListeners[t.m_id][i]);
                            m_dictIdListeners[t.m_id][i] = -1;
                        }
                        else
                            ;
                    }
            else
                //Вообще нельзя что-либо инициализировать
                Logging.Logg().Error(@"HAdmin::stopDbInterfaces () - m_list_tec == null ...");
        }

        public void StopDbInterfaces()
        {
            stopDbInterfaces();
        }

        public void RefreshConnectionSettings()
        {
            if (threadIsWorking > 0)
            {
                foreach (TEC t in m_list_tec) {
                    for (int i = (int)CONN_SETT_TYPE.ADMIN; i < (int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                    {
                        if (!(m_dictIdListeners [t.m_id][i] < 0))
                            DbSources.Sources().SetConnectionSettings(m_dictIdListeners[t.m_id][i], t.connSetts[i], true);
                        else
                            ;
                    }
                }
            }
            else
                ;
        }

        public void SetDelegateWait(DelegateFunc dStart, DelegateFunc dStop, DelegateFunc dStatus)
        {
            this.delegateStartWait = dStart;
            this.delegateStopWait = dStop;
            this.delegateEventUpdate = dStatus;
        }

        public void SetDelegateReport(DelegateFunc ferr, DelegateFunc fact)
        {
            this.errorReport = ferr;
            this.actionReport = fact;
        }

        public void SetDelegateSaveComplete(DelegateFunc f) {            
            saveComplete = f;

            Logging.Logg().Debug(@"HAdmin::SetDelegateSaveComplete () - saveComplete is set=" + saveComplete == null ? false.ToString() : true.ToString() + @" - вЫход");
        }

        public void SetDelegateData(DelegateDateFunc s, DelegateFunc e) { readyData = s; errorData = e; }

        //public void SetDelegateTECComponent(DelegateFunc f) { fillTECComponent = f; }

        public void SetDelegateDatetime(DelegateDateFunc f) { setDatetime = f; }

        protected void MessageBox(string msg, MessageBoxButtons btn = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Error)
        {
            //MessageBox.Show(this, msg, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            Logging.Logg().Error(msg);
        }

        //public virtual void ClearValues(int cnt = -1) {
        public virtual void ClearValues()
        {
            int cntHours = 24;

            //if (cnt < 0) {
                //Проверка признака "обычного" размера массива
                if (m_curDate.Date.Equals (HAdmin.SeasonDateTime.Date) == false)
                {
                    if (m_curRDGValues.Length > 24)
                    {
                        m_curRDGValues = null;
                    }
                    else
                    {
                    }
                }
                else
                {
                    if (m_curRDGValues.Length < 25)
                    {
                        m_curRDGValues = null;
                        cntHours = 25;
                    }
                    else
                    {
                    }
                }
            //} else {
            //    m_curRDGValues = null;
            //    cntHours = cnt;
            //}

            if (m_curRDGValues == null)
                m_curRDGValues = new RDGStruct[cntHours];
            else
                ;
        }

        public abstract void GetRDGValues(int /*TYPE_FIELDS*/ mode, int indx, DateTime date);

        protected abstract void GetPPBRDatesRequest(DateTime date);

        protected abstract bool GetPPBRDatesResponse(DataTable table, DateTime date);

        protected abstract void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode);

        protected abstract bool GetPPBRValuesResponse(DataTable table, DateTime date);        

        protected virtual void ClearDates(CONN_SETT_TYPE type)
        {
            int i = 1
                , cntHours = 24
                , length = m_arHaveDates.Length / m_arHaveDates.Rank;

            if (m_curDate.Date.Equals (HAdmin.SeasonDateTime.Date) == false)
                if (length > 24)
                    m_arHaveDates = null;
                else
                    ;
            else
                if (length < 25)
                {
                    m_arHaveDates = null;
                    cntHours = 25;
                }
                else
                    ;

            if (m_arHaveDates == null)
                m_arHaveDates = new int[(int)CONN_SETT_TYPE.PBR + 1, cntHours];
            else
                ;

            for (i = 0; i < cntHours; i++)
            {
                m_arHaveDates[(int)type, i] = 0; //false;
            }
        }

        protected void ClearPPBRDates()
        {
            ClearDates(CONN_SETT_TYPE.PBR);
        }

        public TECComponent FindTECComponent(int id)
        {
            foreach (TECComponent tc in allTECComponents)
            {
                if (tc.m_id == id)
                    return tc;
                else ;
            }

            return null;
        }

        protected string GetPBRNumber (int hour = -1) {
            return @"ПБР" + getPBRNumber (hour);
        }

        protected int getPBRNumber(int hour = -1)
        {
            int iNum = -1;

            if (hour < 0)
            {
                if (m_ignore_date == true)
                    hour = DateTime.Now.Hour;
                else
                    hour = serverTime.Hour;
            }
            else
                ;

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

        protected virtual void InitializeSyncState ()
        {
            if (m_waitHandleState == null)
                m_waitHandleState = new WaitHandle [1];
            else
                ;

            m_waitHandleState [(int)INDEX_WAITHANDLE_REASON.SUCCESS] = new AutoResetEvent(true);
        }

        //protected abstract bool InitDbInterfaces ();

        public void Request(int idListener, string request)
        {
            DbSources.Sources().Request(m_IdListenerCurrent = idListener, request);
        }

        public virtual bool Response(int idListener, out bool error, out DataTable table/*, bool bIsTec*/)
        {
            return DbSources.Sources().Response(idListener, out error, out table);
        }

        public virtual bool Response(out bool error, out DataTable table/*, bool bIsTec*/)
        {
            return DbSources.Sources().Response(m_IdListenerCurrent, out error, out table);
        }

        protected abstract bool StateRequest(int /*StatesMachine*/ state);

        protected abstract bool StateCheckResponse(int /*StatesMachine*/ state, out bool error, out DataTable table);

        protected abstract bool StateResponse(int /*StatesMachine*/ state, DataTable table);

        protected abstract void StateErrors(int /*StatesMachine*/ state, bool response);

        public virtual void Start()
        {
            threadIsWorking = 0;
            taskThread = new Thread(new ParameterizedThreadStart(TecView_ThreadFunction));
            taskThread.Name = "Интерфейс к РДГ";
            taskThread.IsBackground = true;

            semaState = new Semaphore(1, 1);

            InitializeSyncState ();

            semaState.WaitOne();
            taskThread.Start();
        }

        public void ClearStates()
        {
            //lock (m_lockState)
            //{
                newState = true;
                states.Clear ();

                if (!(FormMainBaseWithStatusStrip.m_report == null))
                    FormMainBaseWithStatusStrip.m_report.ClearStates();
                else
                    Logging.Logg().Error(@"HAdmin::ClearStates () - m_report=null");
            //}
        }

        public virtual void Stop()
        {
            bool joined;
            threadIsWorking = -1;

            StopDbInterfaces ();
            
            ClearStates ();

            if ((!(taskThread == null)) && taskThread.IsAlive)
            {
                try { semaState.Release(1); }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, "HAdmin::StopThreadSourceData () - semaState.Release(1)");
                }

                joined = taskThread.Join(666);
                if (joined == false)
                    taskThread.Abort();
                else
                    ;
            }
            else ;
        }

        protected void TecView_ThreadFunction(object data)
        {
            int index;
            int /*StatesMachine*/ currentState;

            while (! (threadIsWorking < 0))
            {
                semaState.WaitOne();

                index = 0;

                lock (m_lockState)
                {
                    if (states.Count == 0)
                        continue;
                    else
                        ;

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
                            lock (m_lockState)
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
                        lock (m_lockState)
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

                    lock (m_lockState)
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

                //Закончена обработка всех событий
                completeHandleStates();
            }
            try
            {
                semaState.Release(1);
            }
            catch (Exception e)
            { //System.Threading.SemaphoreFullException
                Logging.Logg().Exception(e, "HAdmin::TecView_ThreadFunction () - semaState.Release(1)");
            }
        }

        /// <summary>
        /// Установить признак окончания обработки всех событий
        /// </summary>
        protected void completeHandleStates () {
            try { ((AutoResetEvent)m_waitHandleState[0]).Set (); }
            catch (Exception e) {
                Logging.Logg().Exception(e, "TecView_ThreadFunction () - m_waitHandleState[0]).Set()");
            }
        }

        public FormChangeMode.MODE_TECCOMPONENT modeTECComponent(int indx)
        {
            FormChangeMode.MODE_TECCOMPONENT modeRes = FormChangeMode.MODE_TECCOMPONENT.UNKNOWN;

            if ((allTECComponents[indx].m_id > 0) && (allTECComponents[indx].m_id < 100))
                modeRes = FormChangeMode.MODE_TECCOMPONENT.TEC;
            else
                if ((allTECComponents[indx].m_id > 100) && (allTECComponents[indx].m_id < 500))
                    modeRes = FormChangeMode.MODE_TECCOMPONENT.GTP;
                else
                    if ((allTECComponents[indx].m_id > 500) && (allTECComponents[indx].m_id < 1000))
                        modeRes = FormChangeMode.MODE_TECCOMPONENT.PC;
                    else
                        if ((allTECComponents[indx].m_id > 1000) && (allTECComponents[indx].m_id < 10000))
                            modeRes = FormChangeMode.MODE_TECCOMPONENT.TG;
                        else
                            ;

            return modeRes;
        }

        public virtual void CopyCurToPrevRDGValues() {
            if (!(m_prevRDGValues.Length == m_curRDGValues.Length))
            {
                m_prevRDGValues = null;
                m_prevRDGValues = new RDGStruct[m_curRDGValues.Length];
            }
            else
            {
            }
        }

        public virtual void getCurRDGValues (HAdmin source) {
            if (!(m_curRDGValues.Length == source.m_curRDGValues.Length))
            {
                m_prevRDGValues = null;
                m_prevRDGValues = new RDGStruct[source.m_curRDGValues.Length];
            }
            else
            {
            }
        }

        protected virtual bool GetCurrentTimeResponse(DataTable table)
        {
            if (table.Rows.Count == 1)
            {
                serverTime = (DateTime)table.Rows[0][0];
            }
            else
            {
                DaylightTime daylight = TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year);
                int timezone_offset = allTECComponents[indxTECComponents].tec.m_timezone_offset_msc;
                if (TimeZone.IsDaylightSavingTime(DateTime.Now, daylight) == true)
                    timezone_offset++;
                else
                    ;

                serverTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(3);

                ErrorReport("Ошибка получения текущего времени сервера. Используется локальное время.");
            }

            return true;
        }

        public virtual void ResetRDGExcelValues()
        {
            if (m_waitHandleState.Length > 1)
                ((ManualResetEvent)m_waitHandleState[1]).Reset();
            else
                ;
        }

        protected void GetCurrentTimeRequest(DbInterface.DB_TSQL_INTERFACE_TYPE typeDB, int idListatener)
        {
            string query = string.Empty;

            switch (typeDB)
            {
                case DbInterface.DB_TSQL_INTERFACE_TYPE.MySQL:
                    query = @"SELECT now()";
                    break;
                case DbInterface.DB_TSQL_INTERFACE_TYPE.MSSQL:
                    query = @"SELECT GETDATE()";
                    break;
                default:
                    break;
            }

            if (query.Equals(string.Empty) == false)
                Request(idListatener, query);
            else
                ;
        }

        protected bool IsCanUseTECComponents()
        {
            //bool bRes = false;
            return (!(indxTECComponents < 0)) && (indxTECComponents < allTECComponents.Count);
            //return bRes;
        }

        public virtual void AbortThreadRDGValues(INDEX_WAITHANDLE_REASON reason)
        {
            if (m_waitHandleState.Length > (int)reason)
            {
                ((ManualResetEvent)m_waitHandleState[(int)reason]).Set();
            }
            else
                ;
        }

        protected bool IsHaveDates(CONN_SETT_TYPE type, int indx)
        {
            return m_arHaveDates[(int)type, indx] > 0 ? true : false;
        }

        public static int GetSeasonHourOffset(DateTime dt, int h)
        {
            int iRes = 0;

            if (dt.Date.Equals(HAdmin.SeasonDateTime.Date) == true)
            {
                //if (! (h < HAdmin.SeasonDateTime.Hour))
                if (h > HAdmin.SeasonDateTime.Hour)
                    iRes = 1;
                else
                    ;
            }
            else
            {
            }

            return iRes;
            //return HourSeason < 0 ? 0 : !(h < HourSeason) ? 1 : 0;
        }

        public int GetSeasonHourOffset(int h)
        {
            return GetSeasonHourOffset(m_curDate, h);
        }

        protected void GetSeasonHourIndex(int ssn, ref int h) //Это ссылки на ИНДЕКСЫ, НЕ на ЧАСЫ
        {
            //Проверка перехода сезонов
            if (m_curDate.Date.Equals(HAdmin.SeasonDateTime.Date) == true)
                if (h == HAdmin.SeasonDateTime.Hour)
                    //Проверить сезон
                    if ((ssn - (int)SEASON_BASE) == (int)seasonJumpE.WinterToSummer)
                        h++;
                    else
                        if ((ssn - (int)SEASON_BASE) == (int)seasonJumpE.SummerToWinter)
                        {
                        }
                        else
                            if ((ssn - (int)SEASON_BASE) == (int)seasonJumpE.None)
                                ;
                            else
                                ;
                else
                    if (h > HAdmin.SeasonDateTime.Hour)
                        h++;
                    else
                        ;
            else
                ;
        }

        //protected void GetSeasonHours(ref int prev_h, ref int h) //Это ссылки на ИНДЕКСЫ, НЕ на ЧАСЫ
        //{
        //    int offset = 0;
            
        //    //Проверка перехода сезонов
        //    if (m_curDate.Date.Equals(HAdmin.SeasonDateTime.Date) == true)
        //    {
        //        //Необходимо искать одинаковые часы
        //        if (prev_h < 0)
        //            ; //Не было ни одного предыдущего часа                                
        //        else
        //        {
        //            if (prev_h == h)
        //            {
        //                //Найден одинаковый
        //                offset++;
        //            }
        //            else
        //            {
        //                if (prev_h < h)
        //                    //Норма
        //                    //if (HAdmin.SeasonDateTime.Hour < h)
        //                    if (! (HAdmin.SeasonDateTime.Hour > h))
        //                        offset ++;
        //                    else
        //                        ;
        //                else
        //                    ; //Ошибка ???
        //            }
        //        }
        //    }
        //    else
        //    {
        //        //Оставить "как есть"
        //    }

        //    prev_h = h; //Запомнить текущий
        //    h += offset;
        //}

        public string GetFmtDatetime(int h)
        {
            string strRes = @"dd-MM-yyyy HH";

            if (m_curDate.Date.Equals(HAdmin.SeasonDateTime.Date) == true)
            {
                if ((h) == (HAdmin.SeasonDateTime.Hour))
                    strRes += @"*";
                else
                    ;
            }
            else
                ;

            strRes += @":00";

            return strRes;
        }

        private static string s_Name_Moscow_TimeZone = @"Russian Standard Time";
        public static DateTime ToMoscowTimeZone(DateTime dt)
        //public static DateTime ToCurrentTimeZone(DateTime dt, int offset_msc)
        {
            DateTime dtRes;

            if (! (dt.Kind == DateTimeKind.Local)) {
            //    dtRes = TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.FindSystemTimeZoneById(s_Name_Moscow_TimeZone));
                dtRes = dt.Add(GetUTCOffsetOfMoscowTimeZone ());
            } else {
                dtRes = dt - TimeZoneInfo.Local.GetUtcOffset (dt);
                if (dtRes.IsDaylightSavingTime () == true) {
                    dtRes = dtRes.AddHours(-1);
                } else { }

                dtRes = dtRes.Add(GetUTCOffsetOfMoscowTimeZone());
            //    //dtRes = dtRes.Add(GetUTCOffsetOfCurrentTimeZone(offset_msc));
            }

            return dtRes;
        }

        //public static TimeSpan GetOffsetOfCurrentTimeZone()
        //{
        //    return DateTime.Now - HAdmin.ToCurrentTimeZone(TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now));
        //}

        public static TimeSpan GetUTCOffsetOfMoscowTimeZone()
        {
            ////Перечисление всех зо ОС
            //System.Collections.ObjectModel.ReadOnlyCollection <TimeZoneInfo> tzi = TimeZoneInfo.GetSystemTimeZones ();
            //foreach (TimeZoneInfo tz in tzi) {
            //    Console.WriteLine (tz.DisplayName + @", " +  tz.StandardName + @", " + tz.Id);
            //}

            ////Вариант №1 - работает, если у пользователя установлено обновление (сезонный переход 26.10.2014)
            //return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, HAdmin.s_Name_Moscow_TimeZone) - DateTime.UtcNow;
            ////Вариант №2 - работает, если у пользователя установлено обновление (сезонный переход 26.10.2014)
            //return TimeZoneInfo.FindSystemTimeZoneById(HAdmin.s_Name_Moscow_TimeZone).GetUtcOffset(DateTime.Now);
            ////Вариант №3 - работает, если у пользователя установлено обновление (сезонный переход 26.10.2014) + известно смещение зоны пользователя от МСК
            //return DateTime.UtcNow - DateTime.Now - TimeSpan.FromHours(offset_msc);
            //Вариант №4
            return TimeSpan.FromHours (3);
        }

        public void ErrorReport (string msg) {
            if (!(errorReport == null))
            {
                FormMainBaseWithStatusStrip.m_report.ErrorReport (msg);
                errorReport ();
            }
            else
                ;
        }

        public void ActionReport(string msg)
        {
            if (! (actionReport == null))
            {
                FormMainBaseWithStatusStrip.m_report.ActionReport(msg);
                actionReport();
            }
            else
                ;
        }

        public static int CountHoursOfDate (DateTime dtReq) {
            int iRes = -1;

            if (dtReq.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0)
                iRes = 25;
            else
                iRes = 24;

            return iRes;
        }
    }
}
