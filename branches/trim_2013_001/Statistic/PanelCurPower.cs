using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Data;

using StatisticCommon;

namespace Statistic
{
    public partial class PanelCurPower : TableLayoutPanel
    {       
        private List<PanelTecCurPower> m_listTECCurrentPower;

        enum StatesMachine : int {Init_TM, Current_TM};

        public DelegateFunc delegateEventUpdate;

        public int m_iPeriodUpdate;

        public static volatile string last_error;
        public static DateTime last_time_error;
        public static volatile bool errored_state;

        public static volatile string last_action;
        public static DateTime last_time_action;
        public static volatile bool actioned_state;

        public StatusStrip m_stsStrip;

        public PanelCurPower(List<TEC> listTec, StatusStrip stsStrip, FormParameters par)
        {
            InitializeComponent();

            m_stsStrip = stsStrip;
            m_iPeriodUpdate = par.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME];

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

        public void SetDelegate(DelegateFunc dStatus)
        {
            this.delegateEventUpdate = dStatus;
        }

        partial class PanelTecCurPower
        {
            /// <summary>
            /// Требуется переменная конструктора.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary> 
            /// Освободить все используемые ресурсы.
            /// </summary>
            /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            #region Код, автоматически созданный конструктором компонентов

            /// <summary>
            /// Обязательный метод для поддержки конструктора - не изменяйте
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            private void InitializeComponent()
            {
                components = new System.ComponentModel.Container();
            }

            #endregion
        }

        private partial class PanelTecCurPower : TableLayoutPanel
        {
            enum INDEX_LABEL : int { NAME, DATETIME, VALUE_TOTAL, NAME_COMPONENT, NAME_TG, VALUE_TG, COUNT_INDEX_LABEL };
            const int COUNT_FIXED_ROWS = 3;

            Label[] m_arLabel;
            Dictionary<int, Label> m_dictLabelVal;
            HLabelStyles[] m_arLabelStyles;

            public TEC m_tec;

            private TG[] sensorId2TG;

            enum StatesMachine : int { Init_TM, Current_TM };

            //FormParameters m_parameters;

            private object lockValue;

            private bool m_bIsActive,
                        m_bIsStarted,
                        m_bUpdate;

            private volatile string sensorsString_TM;

            private Thread m_taskThread;
            private Semaphore m_semaState;
            private volatile bool m_bThreadIsWorking;
            private volatile bool m_bIsNewState;
            private volatile List<StatesMachine> m_states;
            private ManualResetEvent m_evTimerCurrent;
            private System.Threading.Timer m_timerCurrent;

            //private volatile string sensorsString_TM;

            private DelegateFunc delegateUpdateGUI_TM;

            public PanelTecCurPower(TEC tec)
            {
                InitializeComponent();

                m_tec = tec;
                Initialize();
            }

            public PanelTecCurPower(IContainer container, TEC tec)
                : this(tec)
            {
                container.Add(this);
            }

            private void Initialize()
            {
                int i = -1;

                m_arLabelStyles = new HLabelStyles[(int)INDEX_LABEL.COUNT_INDEX_LABEL];
                m_arLabelStyles[(int)INDEX_LABEL.NAME] = new HLabelStyles(Color.White, Color.Blue, 22F, ContentAlignment.MiddleCenter);
                m_arLabelStyles[(int)INDEX_LABEL.DATETIME] = new HLabelStyles(Color.LimeGreen, Color.Gray, 24F, ContentAlignment.MiddleCenter);
                m_arLabelStyles[(int)INDEX_LABEL.VALUE_TOTAL] = new HLabelStyles(Color.LimeGreen, Color.Black, 24F, ContentAlignment.MiddleCenter);
                m_arLabelStyles[(int)INDEX_LABEL.NAME_COMPONENT] = new HLabelStyles(Color.Yellow, Color.Green, 14F, ContentAlignment.TopLeft);
                m_arLabelStyles[(int)INDEX_LABEL.NAME_TG] = new HLabelStyles(Color.LightSteelBlue, Color.Yellow, 14F, ContentAlignment.MiddleLeft);
                m_arLabelStyles[(int)INDEX_LABEL.VALUE_TG] = new HLabelStyles(Color.LimeGreen, Color.Black, 14F, ContentAlignment.MiddleCenter);

                m_dictLabelVal = new Dictionary<int, Label>();
                m_arLabel = new Label[(int)INDEX_LABEL.VALUE_TOTAL + 1];

                this.Dock = DockStyle.Fill;
                //Свойства колонок
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

                //Видимая граница для отладки
                this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle;

                //Наименование ТЭЦ, Дата/время, Значение для всех ГТП/ТГ
                for (i = (int)INDEX_LABEL.NAME; i < (int)INDEX_LABEL.NAME_COMPONENT; i++)
                {
                    string cntnt = string.Empty;
                    switch (i)
                    {
                        case (int)INDEX_LABEL.NAME:
                            cntnt = m_tec.name;
                            break;
                        case (int)INDEX_LABEL.DATETIME:
                            cntnt = @"--:--:--";
                            break;
                        case (int)INDEX_LABEL.VALUE_TOTAL:
                            cntnt = 0.ToString("F2");
                            break;
                        default:
                            break;
                    }
                    m_arLabel[i] = HLabel.createLabel(cntnt, m_arLabelStyles[i]);
                    this.Controls.Add(m_arLabel[i], 0, i);
                    this.SetColumnSpan(m_arLabel[i], 3);
                }

                foreach (TECComponent g in m_tec.list_TECComponents)
                {
                    if ((g.m_id > 100) && (g.m_id < 500))
                    {
                        //Добавить наименование ГТП
                        Label lblTECComponent = HLabel.createLabel(g.name_shr, m_arLabelStyles[(int)INDEX_LABEL.NAME_COMPONENT]);
                        this.Controls.Add(lblTECComponent, 0, m_dictLabelVal.Count + COUNT_FIXED_ROWS);

                        foreach (TG tg in g.TG)
                        {
                            //Добавить наименование ТГ
                            this.Controls.Add(HLabel.createLabel(tg.name_shr, m_arLabelStyles[(int)INDEX_LABEL.NAME_TG]), 1, m_dictLabelVal.Count + COUNT_FIXED_ROWS);
                            //Добавить значение ТГ
                            m_dictLabelVal.Add(/*tg.id_tm*/m_dictLabelVal.Count, HLabel.createLabel(0.ToString("F2"), m_arLabelStyles[(int)INDEX_LABEL.VALUE_TG]));
                            this.Controls.Add(m_dictLabelVal[m_dictLabelVal.Count - 1], 2, m_dictLabelVal.Count - 1 + COUNT_FIXED_ROWS);
                        }

                        this.SetRowSpan(lblTECComponent, g.TG.Count);
                    }
                    else
                        ;
                }

                //Свойства зафиксированных строк
                for (i = 0; i < COUNT_FIXED_ROWS; i++)
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, 10));

                this.RowCount = m_dictLabelVal.Count + COUNT_FIXED_ROWS;
                for (i = 0; i < this.RowCount - COUNT_FIXED_ROWS; i++)
                {
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, (float)Math.Round((double)(100 - (10 * COUNT_FIXED_ROWS)) / (this.RowCount - COUNT_FIXED_ROWS), 1)));
                }
            }

            public void Start()
            {
                if (m_bIsStarted == true)
                    return;
                else
                    ;

                m_bIsStarted = true;

                m_tec.StartDbInterfaces();

                m_bThreadIsWorking = true;

                m_taskThread = new Thread(new ParameterizedThreadStart(TecView_ThreadFunction));
                m_taskThread.Name = @"Интерфейс к данным: " + @"текущие значения...";
                m_taskThread.IsBackground = true;

                m_semaState = new Semaphore(1, 1);

                m_semaState.WaitOne();
                m_taskThread.Start();

                m_evTimerCurrent = new ManualResetEvent(true);
                m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, 0, (((PanelCurPower)Parent).m_iPeriodUpdate - 1) * 1000);

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
                        m_semaState.Release(1);
                    }
                    catch (Exception excpt) { Logging.Logg().LogExceptionToFile(excpt, "catch - TecView.Stop () - sem.Release(1)"); }

                    joined = m_taskThread.Join(1000);
                    if (!joined)
                        m_taskThread.Abort();
                    else
                        ;
                }

                m_tec.StopDbInterfaces();

                lock (lockValue)
                {
                    errored_state = false;
                }
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
                            m_semaState.Release(1);
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
                        errored_state =
                        actioned_state = false;
                    }
                }
            }

            private void ErrorReport(string error_string)
            {
                lock (lockValue)
                {
                    last_error = error_string;
                    last_time_error = DateTime.Now;
                    errored_state = true;
                    ((PanelCurPower)Parent).m_stsStrip.BeginInvoke(((PanelCurPower)Parent).delegateEventUpdate);
                }
            }

            private void ActionReport(string action_string)
            {
                lock (lockValue)
                {
                    last_action = action_string;
                    last_time_action = DateTime.Now;
                    actioned_state = true;
                    ((PanelCurPower)Parent).m_stsStrip.BeginInvoke(((PanelCurPower)Parent).delegateEventUpdate);
                }
            }

            private void GetCurrentTMRequest()
            {
                m_tec.Request(CONN_SETT_TYPE.DATA_TM, m_tec.currentTMRequest(sensorsString_TM));
            }

            private bool GetCurrentTMResponse(DataTable table)
            {
                bool bRes = true;
                int i = -1,
                    id = -1;
                double value = -1;
                TG tgTmp;

                foreach (TECComponent g in m_tec.list_TECComponents)
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

                    switch (m_tec.type())
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

            private TG FindTGById(int id, TG.INDEX_VALUE indxVal, TG.ID_TIME id_type)
            {
                for (int i = 0; i < sensorId2TG.Length; i++)
                    switch (indxVal)
                    {
                        case TG.INDEX_VALUE.FACT:
                            if (sensorId2TG[i].ids_fact[(int)id_type] == id)
                                return sensorId2TG[i];
                            else
                                ;
                            break;
                        case TG.INDEX_VALUE.TM:
                            if (sensorId2TG[i].id_tm == id)
                                return sensorId2TG[i];
                            else
                                ;
                            break;
                        default:
                            break;
                    }

                return null;
            }

            private void GetSensorsTMRequest()
            {
                m_tec.Request(CONN_SETT_TYPE.DATA_TM, m_tec.sensorsTMRequest());
            }

            private bool GetSensorsTMResponse(DataTable table)
            {
                bool bRes = true;

                string s;
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    //Шаблон для '[0](["NAME"])'
                    //Формирование правильное имя турбиногенератора
                    s = TEC.getNameTG(m_tec.m_strTemplateNameSgnDataTM, table.Rows[i][0].ToString().ToUpper());

                    //if (num_TECComponent < 0)
                    {//ТЭЦ в полном составе
                        int j = -1;
                        for (j = 0; j < m_tec.list_TECComponents.Count; j++)
                        {
                            int k = -1;
                            for (k = 0; k < m_tec.list_TECComponents[j].TG.Count; k++)
                            {
                                if (m_tec.list_TECComponents[j].TG[k].name_shr.Equals(s) == true)
                                {
                                    m_tec.list_TECComponents[j].TG[k].id_tm = int.Parse(table.Rows[i][1].ToString());

                                    sensorId2TG[sensorId2TG.Length - 1] = m_tec.list_TECComponents[j].TG[k];

                                    //Прерывание внешнего цикла
                                    j = m_tec.list_TECComponents.Count;
                                    break;
                                }
                                else
                                    ;
                            }
                        }
                    }
                    /*else
                    {// Для ТЭЦ НЕ в полном составе (ГТП, ЩУ, ТГ)
                        int k = -1;
                        for (k = 0; k < tec.list_TECComponents[num_TECComponent].TG.Count; k++)
                        {
                            if (tec.list_TECComponents[num_TECComponent].TG[k].name_shr.Equals(s) == true)
                            {
                                tec.list_TECComponents[num_TECComponent].TG[k].id_tm = int.Parse(table.Rows[i][1].ToString());
                                break;
                            }
                            else
                                ;
                        }
                    }*/
                }

                for (int i = 0; i < sensorId2TG.Length; i++)
                {
                    if (!(sensorId2TG[i] == null))
                    {
                        if (sensorsString_TM.Equals(string.Empty) == true)
                        {
                            sensorsString_TM = "[dbo].[states_real_his].[ID] = " + sensorId2TG[i].id_tm.ToString();
                        }
                        else
                        {
                            sensorsString_TM += " OR [dbo].[states_real_his].[ID] = " + sensorId2TG[i].id_tm.ToString();
                        }
                    }
                    else
                    {
                        ErrorReportSensors(ref table);

                        return false;
                    }
                }

                return bRes;
            }

            private void ErrorReportSensors(ref DataTable src)
            {
                string error = "Ошибка определения идентификаторов датчиков в строке ";
                for (int j = 0; j < src.Rows.Count; j++)
                    error += src.Rows[j][0].ToString() + " = " + src.Rows[j][1].ToString() + ", ";

                error = error.Substring(0, error.LastIndexOf(","));
                ErrorReport(error);
            }

            private void StateRequest(StatesMachine state)
            {
                switch (state)
                {
                    case StatesMachine.Init_TM:
                        ActionReport("Получение идентификаторов датчиков.");
                        switch (m_tec.type())
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
                        switch (m_tec.type())
                        {
                            case TEC.TEC_TYPE.COMMON:
                                return m_tec.GetResponse(CONN_SETT_TYPE.DATA_TM, out error, out table);
                            case TEC.TEC_TYPE.BIYSK:
                                return true;
                        }
                        break;
                    case StatesMachine.Current_TM:
                        return m_tec.GetResponse(CONN_SETT_TYPE.DATA_TM, out error, out table);
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
                        switch (m_tec.type())
                        {
                            case TEC.TEC_TYPE.COMMON:
                                result = GetSensorsTMResponse(table);
                                break;
                            case TEC.TEC_TYPE.BIYSK:
                                result = true;
                                break;
                        }
                        if (result == true)
                        {
                        }
                        break;
                    case StatesMachine.Current_TM:
                        //GenerateMinsTable(seasonJumpE.None, 5, table);
                        result = GetCurrentTMResponse(table);
                        if (result == true)
                        {
                            this.BeginInvoke(delegateUpdateGUI_TM);
                        }
                        else
                            ;
                        break;
                }

                if (result == true)
                    lock (lockValue)
                    {
                        errored_state =
                        actioned_state = false;
                    }
                else
                    ;

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
                        waiting = @"Ожидание " + ((PanelCurPower)Parent).m_iPeriodUpdate.ToString() + " секунд";
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
                    m_semaState.WaitOne();

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
                    m_semaState.Release(1);
                }
                catch
                {
                }
            }

            private void TimerCurrent_Tick(Object stateInfo)
            {
                lock (lockValue)
                {
                    ChangeState();

                    try
                    {
                        m_semaState.Release(1);
                    }
                    catch
                    {
                    }

                }
            }
        }
    }
}
