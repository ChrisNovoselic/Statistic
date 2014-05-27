using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using System.Drawing;
using System.Threading;
using System.Data;

using StatisticCommon;

namespace Statistic
{
    public partial class PanelCurPower : TableLayoutPanel
    {       
        private List<PanelTecCurPower> m_listTECCurrentPower;

        enum StatesMachine : int {Init_TM, Current_TM};

        private DelegateFunc delegateStartWait;
        private DelegateFunc delegateStopWait;
        private DelegateFunc delegateEventUpdate;

        FormParameters m_parameters;

        public volatile string last_error;
        public DateTime last_time_error;
        public volatile bool errored_state;

        public volatile string last_action;
        public DateTime last_time_action;
        public volatile bool actioned_state;

        private object lockValue;

        private bool m_bIsActive,
                    m_bIsStarted,
                    m_bUpdate;

        private volatile string sensorsString_TM;

        private Thread m_taskThread;
        private Semaphore m_semaphore;
        private volatile bool m_bThreadIsWorking;
        private volatile bool m_bIsNewState;
        private volatile List<StatesMachine> m_states;
        private ManualResetEvent m_evTimerCurrent;
        private System.Threading.Timer m_timerCurrent;

        private StatusStrip m_stsStrip;

        public PanelCurPower(List<TEC> listTec, StatusStrip stsStrip, FormParameters par)
        {
            InitializeComponent();

            m_stsStrip = stsStrip;
            m_parameters = par;

            this.RowStyles.Add(new RowStyle (SizeType.AutoSize));

            this.Dock = DockStyle.Fill;

            //this.Location = new System.Drawing.Point(40, 58);
            //this.Name = "pnlView";
            //this.Size = new System.Drawing.Size(705, 747);

            this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle;

            m_listTECCurrentPower = new List<PanelTecCurPower> ();
            PanelTecCurPower ptcp;

            this.ColumnCount = listTec.Count;
            for (int i = 0; i < listTec.Count; i++)
            {
                ptcp = new PanelTecCurPower(listTec[i]);
                this.Controls.Add(ptcp, i, 0);
                
                m_listTECCurrentPower.Add(ptcp);
                //ptcp.Location = new Point (1 + i * widthTec, 1);
                //ptcp.Size = new Size(widthTec, this.Height);

                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / listTec.Count));                
            }
        }

        public PanelCurPower(IContainer container, List<TEC> listTec, StatusStrip stsStrip, FormParameters par)
            : this(listTec, stsStrip, par)
        {
            container.Add(this);
        }

        public void SetDelegate(DelegateFunc dStart, DelegateFunc dStop, DelegateFunc dStatus)
        {
            this.delegateStartWait = dStart;
            this.delegateStopWait = dStop;
            this.delegateEventUpdate = dStatus;
        }

        public void Start()
        {
            if (m_bIsStarted == true)
                return;
            else
                ;

            m_bIsStarted = true;

            foreach (PanelTecCurPower ptcp in m_listTECCurrentPower) {
                ptcp.m_tec.StartDbInterfaces();
            }
            m_bThreadIsWorking = true;

            m_taskThread = new Thread(new ParameterizedThreadStart(TecView_ThreadFunction));
            m_taskThread.Name = @"Интерфейс к данным: " + @"текущие значения...";
            m_taskThread.IsBackground = true;

            m_semaphore = new Semaphore(1, 1);

            m_semaphore.WaitOne();
            m_taskThread.Start();

            m_evTimerCurrent = new ManualResetEvent(true);
            m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, 0, (m_parameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME] - 1) * 1000);

            m_bUpdate = false;
        }

        public void Stop()
        {
            if (m_bIsStarted == false)
                return;
            else
                ;

            m_evTimerCurrent.Reset();
            m_timerCurrent.Dispose();

            m_bIsStarted = false;
            bool joined;
            m_bThreadIsWorking = false;
            lock (lockValue)
            {
                m_bIsNewState = true;
                m_states.Clear();
            }

            if (m_taskThread.IsAlive)
            {
                try
                {
                    m_semaphore.Release(1);
                }
                catch (Exception excpt) { Logging.Logg().LogExceptionToFile(excpt, "catch - TecView.Stop () - sem.Release(1)"); }

                joined = m_taskThread.Join(1000);
                if (!joined)
                    m_taskThread.Abort();
                else
                    ;
            }

            foreach (PanelTecCurPower ptcp in m_listTECCurrentPower)
            {
                ptcp.m_tec.StopDbInterfaces();
            }

            errored_state = false;
        }

        private void ChangeState()
        {
            m_bIsNewState = true;
            m_states.Clear();

            if ((sensorsString_TM.Equals(string.Empty) == false))
            {
            }
            else
            {
                m_states.Add(StatesMachine.Init_TM);
            }

            m_states.Add(StatesMachine.Current_TM);
        }

        public void Activate(bool active)
        {
            if (active)
            {
                m_bIsActive = true;
                lock (lockValue)
                {
                    ChangeState();

                    try
                    {
                        m_semaphore.Release(1);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                m_bIsActive = false;
                lock (lockValue)
                {
                    m_bIsNewState = true;
                    m_states.Clear();
                    errored_state = actioned_state = false;
                }
            }
        }

        private void ErrorReport(string error_string)
        {
            last_error = error_string;
            last_time_error = DateTime.Now;
            errored_state = true;
            m_stsStrip.BeginInvoke(delegateEventUpdate);
        }

        private void ActionReport(string action_string)
        {
            last_action = action_string;
            last_time_action = DateTime.Now;
            actioned_state = true;
            m_stsStrip.BeginInvoke(delegateEventUpdate);
        }

        private void GetCurrentTMRequest()
        {
            tec.Request(CONN_SETT_TYPE.DATA_TM, tec.currentTMRequest (sensorsString_TM));
        }

        private bool GetCurrentTMResponse(DataTable table)
        {
            bool bRes = true;
            int i = -1,
                id = -1;
            double value = -1;
            TG tgTmp;

            foreach (TECComponent g in tec.list_TECComponents)
            {
                foreach (TG t in g.TG)
                {
                    for (i = 0; i < t.power.Length; i++)
                    {
                        t.power_TM = 0;
                    }
                }
            }

            for (i = 0; i < table.Rows.Count; i++)
            {
                if (int.TryParse(table.Rows[i]["ID"].ToString(), out id) == false)
                    return false;
                else
                    ;

                tgTmp = FindTGById(id, TG.INDEX_VALUE.TM, (TG.ID_TIME)(-1));

                if (tgTmp == null)
                    return false;
                else
                    ;

                if (double.TryParse(table.Rows[i]["value"].ToString(), out value) == false)
                    return false;
                else
                    ;

                switch (tec.type())
                {
                    case TEC.TEC_TYPE.COMMON:
                        break;
                    case TEC.TEC_TYPE.BIYSK:
                        value *= 20;
                        break;
                    default:
                        break;
                }

                tgTmp.power_TM = value;
            }

            return bRes;
        }

        private void StateRequest(StatesMachine state)
        {
            switch (state)
            {
                case StatesMachine.Init_TM:
                    ActionReport("Получение идентификаторов датчиков.");
                    switch (tec.type())
                    {
                        case TEC.TEC_TYPE.COMMON:
                            GetSensorsTMRequest();
                            break;
                        case TEC.TEC_TYPE.BIYSK:
                            break;
                        default:
                            break;
                    }
                    break;
                case StatesMachine.Current_TM:
                    ActionReport("Получение текущих значений.");
                    //adminValuesReceived = false;
                    GetCurrentTMRequest();
                    break;
                
            }
        }

        private bool StateCheckResponse(StatesMachine state, out bool error, out DataTable table)
        {
            error = false;
            table = null;

            switch (state)
            {
                case StatesMachine.Init_TM:
                    switch (tec.type())
                    {
                        case TEC.TEC_TYPE.COMMON:
                            return tec.GetResponse(CONN_SETT_TYPE.DATA_TM, out error, out table);
                        case TEC.TEC_TYPE.BIYSK:
                            return true;
                    }
                    break;
                case StatesMachine.Current_TM:
                    return tec.GetResponse(CONN_SETT_TYPE.DATA_TM, out error, out table);
            }

            error = true;

            return false;
        }

        private bool StateResponse(StatesMachine state, DataTable table)
        {
            bool result = false;
            switch (state)
            {
                case StatesMachine.Init_TM:
                    switch (tec.type())
                    {
                        case TEC.TEC_TYPE.COMMON:
                            result = GetSensorsTMResponse(table);
                            break;
                        case TEC.TEC_TYPE.BIYSK:
                            result = true;
                            break;
                    }
                    if (result)
                    {
                    }
                    break;
                case StatesMachine.Current_TM:
                    //GenerateMinsTable(seasonJumpE.None, 5, table);
                    result = GetCurrentTMResponse(table);
                    if (result)
                    {
                        this.BeginInvoke(delegateUpdateGUI_TM);
                    }
                    else
                        ;
                    break;
            }

            if (result)
                errored_state = actioned_state = false;

            return result;
        }

        private void StateErrors(StatesMachine state, bool response)
        {
            string reason = string.Empty,
                    waiting = string.Empty;

            switch (state)
            {
                case StatesMachine.Init_TM:
                    reason = @"идентификаторов датчиков (телемеханика)";
                    waiting = @"Переход в ожидание";
                    break;
                case StatesMachine.Current_TM:
                    reason = @"текущих значений";
                    waiting = @"Ожидание " + m_parameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME].ToString() + " секунд";
                    break;
            }

            if (response)
                reason = @"разбора " + reason;
            else
                reason = @"получения " + reason;

            if (waiting.Equals(string.Empty) == true)
                ErrorReport("Ошибка " + reason + ". " + waiting + ".");
            else
                ErrorReport("Ошибка " + reason + ".");
        }

        private void TecView_ThreadFunction(object data)
        {
            int index;
            StatesMachine currentState;

            while (m_bThreadIsWorking)
            {
                m_semaphore.WaitOne();

                index = 0;

                lock (lockValue)
                {
                    if (m_states.Count == 0)
                        continue;
                    currentState = m_states[index];
                    m_bIsNewState = false;
                }

                while (true)
                {
                    bool error = true;
                    bool dataPresent = false;
                    DataTable table = null;
                    for (int i = 0; i < DbInterface.MAX_RETRY && !dataPresent && !m_bIsNewState; i++)
                    {
                        if (error)
                            StateRequest(currentState);

                        error = false;
                        for (int j = 0; j < DbInterface.MAX_WAIT_COUNT && !dataPresent && !error && !m_bIsNewState; j++)
                        {
                            System.Threading.Thread.Sleep(DbInterface.WAIT_TIME_MS);
                            dataPresent = StateCheckResponse(currentState, out error, out table);
                        }
                    }

                    bool responseIsOk = true;
                    if (dataPresent && !error && !m_bIsNewState)
                        responseIsOk = StateResponse(currentState, table);

                    if ((!responseIsOk || !dataPresent || error) && !m_bIsNewState)
                    {
                        StateErrors(currentState, !responseIsOk);
                        lock (lockValue)
                        {
                            if (m_bIsNewState == false)
                            {
                                m_states.Clear();
                                m_bIsNewState = true;
                            }
                        }
                    }

                    index++;

                    lock (lockValue)
                    {
                        if (index == m_states.Count)
                            break;
                        if (m_bIsNewState)
                            break;
                        currentState = m_states[index];
                    }
                }
            }
            try
            {
                m_semaphore.Release(1);
            }
            catch
            {
            }
        }

        private void TimerCurrent_Tick(Object stateInfo)
        {
            delegateStartWait();
            lock (lockValue)
            {
                ChangeState();

                try
                {
                    m_semaphore.Release(1);
                }
                catch
                {
                }

            }
            delegateStopWait();
        }
    }
}
