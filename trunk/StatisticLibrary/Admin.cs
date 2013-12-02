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
            public double plan;
            public double recomendation;
            public bool deviationPercent;
            public double deviation;
            /*
            public RDGStruct()
            {
                this.plan =
                this.recomendation =
                this.deviation = 0.0;

                this.deviationPercent = false;
            }
            */
        }

        protected struct TecPPBRValues
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

        protected DelegateFunc delegateStartWait;
        protected DelegateFunc delegateStopWait;
        protected DelegateFunc delegateEventUpdate;

        protected DelegateStringFunc errorReport;
        protected DelegateStringFunc actionReport;

        protected DelegateFunc saveComplete = null;
        protected DelegateDateFunction fillData = null;

        protected DelegateDateFunction setDatetime;

        public volatile RDGStruct[] m_prevRDGValues;
        //public RDGStruct[] m_curTimezoneOffsetRDGExcelValues;
        public RDGStruct[] m_curRDGValues;

        protected bool is_connection_error;
        protected bool is_data_error;

        public volatile string last_error;
        public DateTime last_time_error;
        public volatile bool errored_state;

        public volatile string last_action;
        public DateTime last_time_action;
        public volatile bool actioned_state;

        public DateTime m_prevDate;
        protected DateTime serverTime,
                            m_curDate;

        protected volatile bool using_date;
        public bool m_ignore_date;
        public bool m_ignore_connsett_data;

        protected bool[,] m_arHaveDates;

        private Semaphore semaDBAccess;
        private volatile Errors saveResult;
        private volatile bool saving;

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

        protected Object m_lockObj;

        protected Thread taskThread;
        protected Semaphore semaState;
        protected WaitHandle [] m_waitHandleState;
        //protected AutoResetEvent evStateEnd;
        public volatile bool threadIsWorking;
        protected volatile bool newState;
        protected volatile List<int /*StatesMachine*/> states;

        public ConnectionSettings connSettConfigDB;

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

        private void Initialize () {
            started = false;

            m_prevRDGValues = new RDGStruct[24];

            is_data_error = is_connection_error = false;

            //TecView tecView = FormMainTrans.selectedTecViews [FormMainTrans.stclTecViews.SelectedIndex];

            //isActive = false;

            using_date = false;
            m_ignore_date = false;
            m_ignore_connsett_data = false;

            m_curRDGValues = new RDGStruct[24];

            m_arHaveDates = new bool[(int)CONN_SETT_TYPE.PBR + 1, 24];

            m_lockObj = new Object();

            //semaLoadLayout = new Semaphore(1, 1);

            //delegateFillData = new DelegateFunctionDate(FillData);

            states = new List<int /*StatesMachine*/>();
        }

        public virtual bool WasChanged()
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

        public abstract Errors SaveChanges();

        public abstract void Start();

        public abstract bool IsRDGExcel (int indx);

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

        public void SetDelegateSaveComplete(DelegateFunc f) { saveComplete = f; }
        
        public void SetDelegateData(DelegateDateFunction f) { fillData = f; }

        //public void SetDelegateTECComponent(DelegateFunc f) { fillTECComponent = f; }

        public void SetDelegateDatetime(DelegateDateFunction f) { setDatetime = f; }

        void MessageBox(string msg, MessageBoxButtons btn = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Error)
        {
            //MessageBox.Show(this, msg, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            Logging.Logg().LogLock();
            Logging.Logg().LogToFile(msg, true, true, false);
            Logging.Logg().LogUnlock();
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

        protected abstract void GetAdminDatesRequest(DateTime date);

        protected abstract void GetPPBRDatesRequest(DateTime date);

        protected abstract void ClearDates(CONN_SETT_TYPE type);

        private void ClearAdminDates()
        {
            ClearDates(CONN_SETT_TYPE.ADMIN);
        }

        private void ClearPPBRDates()
        {
            ClearDates(CONN_SETT_TYPE.PBR);
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

        private bool GetPPBRDatesResponse(DataTable table, DateTime date)
        {
            return GetDatesResponse(CONN_SETT_TYPE.PBR, table, date);
        }

        protected abstract string [] setAdminValuesQuery(TEC t, TECComponent comp, DateTime date);
        
        protected abstract void SetAdminValuesRequest(TEC t, TECComponent comp, DateTime date);

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

        protected abstract string[] setPPBRQuery(TEC t, TECComponent comp, DateTime date);

        protected abstract void SetPPBRRequest(TEC t, TECComponent comp, DateTime date);

        protected abstract void ClearPPBRRequest(TEC t, TECComponent comp, DateTime date);

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

        protected virtual void InitializeSyncState ()
        {
            m_waitHandleState = new WaitHandle [1] { new AutoResetEvent(true) };
        }

        protected abstract bool StateRequest(int /*StatesMachine*/ state);

        protected abstract bool StateCheckResponse(int /*StatesMachine*/ state, out bool error, out DataTable table);

        protected abstract bool StateResponse(int /*StatesMachine*/ state, DataTable table);

        protected abstract void StateErrors(int /*StatesMachine*/ state, bool response);

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
                Logging.Logg().LogLock();
                Logging.Logg().LogToFile("catch - (Admin)TecView_ThreadFunction () - semaState.Release(1)", true, true, false);
                Logging.Logg().LogToFile("Исключение обращения к переменной (semaState)", true, true, false);
                Logging.Logg().LogToFile("Исключение: " + e.Message, false, false, false);
                Logging.Logg().LogToFile(e.ToString(), false, false, false);
                Logging.Logg().LogUnlock();
            }
        }

        public abstract void SaveRDGValues(/*TYPE_FIELDS mode, */int indx, DateTime date, bool bCallback);

        public abstract void ClearRDGValues(DateTime date);

        public void CopyCurToPrevRDGValues () {
            for (int i = 0; i < 24; i++)
            {
                m_prevRDGValues[i].plan = m_curRDGValues[i].plan;
                m_prevRDGValues[i].recomendation = m_curRDGValues[i].recomendation;
                m_prevRDGValues[i].deviationPercent = m_curRDGValues[i].deviationPercent;
                m_prevRDGValues[i].deviation = m_curRDGValues[i].deviation;
            }
        }

        public virtual void getCurRDGValues (HAdmin source) {
            for (int i = 0; i < 24; i++)
            {
                m_curRDGValues[i].plan = source.m_curRDGValues[i].plan;
                m_curRDGValues[i].recomendation = source.m_curRDGValues[i].recomendation;
                m_curRDGValues[i].deviationPercent = source.m_curRDGValues[i].deviationPercent;
                m_curRDGValues[i].deviation = source.m_curRDGValues[i].deviation;
            }
        }

        public virtual void ResetRDGExcelValues()
        {
            if (m_waitHandleState.Length > 0)
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
    }
}
