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
    public delegate void DelegateDateFunction(DateTime date);
    
    public abstract class HAdmin : object
    {
        public struct /*class*/ RDGStruct
        {
            //public double [] ppbr;
            public double pbr;
            public double pmin;
            public double pmax;
            public double recomendation;
            public bool deviationPercent;
            public double deviation;
        }

        public volatile RDGStruct[] m_prevRDGValues;
        public RDGStruct[] m_curRDGValues;
        
        protected DelegateFunc delegateStartWait;
        protected DelegateFunc delegateStopWait;
        protected DelegateFunc delegateEventUpdate;

        protected DelegateStringFunc errorReport;
        protected DelegateStringFunc actionReport;

        protected DelegateFunc saveComplete = null;
        protected DelegateDateFunction fillData = null;

        protected DelegateDateFunction setDatetime;

        public volatile List<TEC> m_list_tec;
        public volatile List<TECComponent> allTECComponents;
        public int indxTECComponents;

        protected List<DbInterface> m_listDbInterfaces;
        protected List<int> m_listListenerIdCurrent;
        protected int m_indxDbInterfaceCurrent; //Индекс в списке 'm_listDbInterfaces'

        protected bool is_connection_error;
        protected bool is_data_error;

        public volatile string last_error;
        public DateTime last_time_error;
        public volatile bool errored_state;

        public volatile string last_action;
        public DateTime last_time_action;
        public bool actioned_state {
            get { return m_actioned_state; }
            set {
                m_actioned_state = value;
            }
        }

        protected bool m_actioned_state;

        public DateTime m_prevDate;
        protected DateTime serverTime,
                            m_curDate;

        protected volatile bool using_date;
        public bool m_ignore_date;
        protected bool m_ignore_connsett_data;

        protected int[,] m_arHaveDates;

        protected Object m_lockObj;

        protected Thread taskThread;
        protected Semaphore semaState;
        protected WaitHandle [] m_waitHandleState;
        //protected AutoResetEvent evStateEnd;
        public volatile bool threadIsWorking;
        protected volatile bool newState;
        protected volatile List<int /*StatesMachine*/> states;

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
            NoSet,
            ParseError,
        }

        //Для особкнной ТЭЦ (Бийск)
        //private volatile DbDataInterface dataInterface;
        
        //private Thread dbThread;
        //private Semaphore sema;
        //private volatile bool workTread;
        //-------------------------

        protected bool started;

        //public bool isActive;

        public HAdmin()
        {
            Initialize ();
        }

        protected virtual void Initialize () {
            started = false;

            is_data_error = is_connection_error = false;

            using_date = false;
            m_ignore_date = false;
            m_ignore_connsett_data = false;

            //m_arHaveDates = new bool[(int)CONN_SETT_TYPE.PBR + 1, 24];
            m_arHaveDates = new int[(int)CONN_SETT_TYPE.PBR + 1, 24];

            m_lockObj = new Object();

            states = new List<int /*StatesMachine*/>();

            allTECComponents = new List<TECComponent>();

            m_curRDGValues = new RDGStruct[24];
            m_prevRDGValues = new RDGStruct[24];

            //for (int i = 0; i < 24; i++)
            //{
            //    m_curRDGValues[i].ppbr = new double[3 /*4 для SN???*/];
            //    m_prevRDGValues[i].ppbr = new double[3 /*4 для SN???*/];
            //}

            m_listDbInterfaces = new List<DbInterface>();
            m_listListenerIdCurrent = new List<int>();
        }

        public void InitTEC(ConnectionSettings connSett, FormChangeMode.MODE_TECCOMPONENT mode, bool bIgnoreTECInUse, bool bUseData)
        {
            //connSettConfigDB = connSett;

            m_ignore_connsett_data = !bUseData;

            if (mode == FormChangeMode.MODE_TECCOMPONENT.UNKNOWN)
                this.m_list_tec = new InitTEC(connSett, bIgnoreTECInUse, bUseData).tec;
            else
            {
                this.m_list_tec = new InitTEC(connSett, (short)mode, bIgnoreTECInUse, bUseData).tec;
            }

            //comboBoxTecComponent.Items.Clear ();
            allTECComponents.Clear();

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

        public abstract bool WasChanged();

        public virtual void Resume()
        {
            if (started == false)
            {
                started = true;
            }
            else
            {
            }
        }

        public void Suspend()
        {
            if (started == true)
                started = false;
            else
                ;
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

        public void SetDelegateSaveComplete(DelegateFunc f) { saveComplete = f; }
        
        public void SetDelegateData(DelegateDateFunction f) { fillData = f; }

        //public void SetDelegateTECComponent(DelegateFunc f) { fillTECComponent = f; }

        public void SetDelegateDatetime(DelegateDateFunction f) { setDatetime = f; }

        protected void MessageBox(string msg, MessageBoxButtons btn = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Error)
        {
            //MessageBox.Show(this, msg, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            Logging.Logg().LogToFile(msg, true, true, true);
        }

        public abstract void ClearValues();

        public abstract void GetRDGValues(int /*TYPE_FIELDS*/ mode, int indx, DateTime date);

        protected abstract void GetPPBRDatesRequest(DateTime date);

        protected abstract bool GetPPBRDatesResponse(DataTable table, DateTime date);

        protected abstract void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date, AdminTS.TYPE_FIELDS mode);

        protected abstract bool GetPPBRValuesResponse(DataTable table, DateTime date);        

        protected virtual void ClearDates(CONN_SETT_TYPE type)
        {
            int i = 1;

            for (i = 0; i < 24; i++)
            {
                m_arHaveDates[(int)type, i] = 0; //false;
            }
        }

        protected void ClearPPBRDates()
        {
            ClearDates(CONN_SETT_TYPE.PBR);
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

        protected void ErrorReport(string error_string)
        {
            last_error = error_string;
            last_time_error = DateTime.Now;
            errored_state = true;
            
            errorReport (error_string);
        }

        protected void ActionReport(string action_string)
        {
            last_action = action_string;
            last_time_action = DateTime.Now;
            m_actioned_state = true;

            //Console.WriteLine(@"HAdmin::ActionReport () - actioned_state = {0}", m_actioned_state);
            
            //stsStrip.BeginInvoke(delegateEventUpdate);
            //delegateEventUpdate ();
            actionReport(action_string);
        }

        protected virtual void InitializeSyncState ()
        {
            m_waitHandleState = new WaitHandle [1] { new AutoResetEvent(true) };
        }

        protected abstract bool InitDbInterfaces ();

        public abstract bool GetResponse(int indxDbInterface, int listenerId, out bool error, out DataTable table/*, bool isTec*/);

        protected abstract bool StateRequest(int /*StatesMachine*/ state);

        protected abstract bool StateCheckResponse(int /*StatesMachine*/ state, out bool error, out DataTable table);

        protected abstract bool StateResponse(int /*StatesMachine*/ state, DataTable table);

        protected abstract void StateErrors(int /*StatesMachine*/ state, bool response);

        public virtual void StartThreadSourceData()
        {
            threadIsWorking = true;
            taskThread = new Thread(new ParameterizedThreadStart(TecView_ThreadFunction));
            taskThread.Name = "Интерфейс к данным";
            taskThread.IsBackground = true;

            semaState = new Semaphore(1, 1);

            InitializeSyncState ();

            semaState.WaitOne();
            taskThread.Start();
        }

        public virtual void StopThreadSourceData()
        {
            bool joined;
            threadIsWorking = false;
            lock (m_lockObj)
            {
                newState = true;
                states.Clear();
                errored_state = false;
            }

            if ((!(taskThread == null)) && taskThread.IsAlive)
            {
                try { semaState.Release(1); }
                catch
                {
                    Logging.Logg().LogToFile("catch - StopThreadSourceData () - semaState.Release(1)", true, true, true);
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

            while (threadIsWorking)
            {
                semaState.WaitOne();

                index = 0;

                lock (m_lockObj)
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

                try { ((AutoResetEvent)m_waitHandleState[0]).Set (); }
                catch (Exception e) {
                    Logging.Logg().LogExceptionToFile(e, "TecView_ThreadFunction () - evStateEnd.Set ()");
                }
            }
            try
            {
                semaState.Release(1);
            }
            catch (System.Threading.SemaphoreFullException e) //(Exception e)
            {
                string msg = string.Empty;
                msg += "catch - (Admin)TecView_ThreadFunction () - semaState.Release(1)" + Environment.NewLine;
                msg += "Исключение обращения к переменной (semaState)" + Environment.NewLine;
                msg += "Исключение: " + e.Message + Environment.NewLine;
                msg += e.ToString();

                Logging.Logg().LogToFile(msg, true, true, true);
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

        public abstract void CopyCurToPrevRDGValues();

        public abstract void getCurRDGValues (HAdmin source);

        public virtual void ResetRDGExcelValues()
        {
            if (m_waitHandleState.Length > 1)
                ((ManualResetEvent)m_waitHandleState[1]).Reset();
            else
                ;
        }

        public virtual void AbortRDGExcelValues()
        {
            if (m_waitHandleState.Length > 1)
                ((ManualResetEvent)m_waitHandleState[1]).Set();
            else
                ;
        }

        protected bool IsHaveDates (CONN_SETT_TYPE type, int indx) {
            return m_arHaveDates [(int)type, indx] > 0 ? true : false;
        }

        public static DateTime ToCurrentTimeZone (DateTime dt) {
            DateTime dtRes;
            int hourAdding = 0;
            DaylightTime daylight = TimeZone.CurrentTimeZone.GetDaylightChanges(dt.Year);
            if (TimeZone.IsDaylightSavingTime(dt, daylight) == true)
                hourAdding = 1;
            else
                ;

            //dtRes = TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.FindSystemTimeZoneById (TimeZone.CurrentTimeZone.StandardName));
            dtRes = TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.FindSystemTimeZoneById (@"Russian Standard Time"));
            return dtRes; 
        }
    }
}
