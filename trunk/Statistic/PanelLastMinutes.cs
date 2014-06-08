using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Data;

using StatisticCommon;

namespace Statistic
{
    partial class PanelLastMinutes
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
    
    public partial class PanelLastMinutes : TableLayoutPanel
    {
        List <Label> m_listLabelDateTime;

        enum INDEX_LABEL : int { NAME_TEC, NAME_COMPONENT, VALUE_COMPONENT, DATETIME, COUNT_INDEX_LABEL };
        static Color s_clrBakColorLabel = Color.FromArgb(212, 208, 200), s_clrBakColorLabelVal = Color.FromArgb(219, 223, 227);
        static HLabelStyles[] s_arLabelStyles = {new HLabelStyles(Color.Black, s_clrBakColorLabel, 14F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBakColorLabel, 12F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBakColorLabelVal, 10F, ContentAlignment.MiddleRight),
                                                new HLabelStyles(Color.Black, s_clrBakColorLabel, 12F, ContentAlignment.MiddleCenter)};
        static int COUNT_FIXED_ROWS = (int)INDEX_LABEL.NAME_COMPONENT + 1;
        //static int COUNT_HOURS = 24;

        static RowStyle fRowStyle () { return new RowStyle(SizeType.Percent, (float)Math.Round((double)100 / (24 + COUNT_FIXED_ROWS), 6)); }

        AdminTS m_admin;
        
        enum StatesMachine : int
        {
            Init_TM,
            LastMinutes_TM,
            PBRValues,
            AdminValues
        };

        public DelegateFunc delegateEventUpdate;

        public int m_msecPeriodUpdate;

        public volatile string last_error;
        public DateTime last_time_error;
        public volatile bool errored_state;

        public volatile string last_action;
        public DateTime last_time_action;
        public volatile bool actioned_state;

        public bool m_bIsActive;

        public StatusStrip m_stsStrip;

        public PanelLastMinutes(List<TEC> listTec, StatusStrip stsStrip, AdminTS admin)
        {
            int i = -1;

            m_stsStrip = stsStrip;
            m_admin = admin;
            
            InitializeComponent();

            this.Dock = DockStyle.Fill;

            this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle

            this.ColumnCount = listTec.Count + 1;
            this.RowCount = 1;

            //Создание панели с дата/время
            PanelDateTime panelDateTime = new PanelDateTime();

            float fPercentColDatetime = 8F;
            this.Controls.Add(panelDateTime, 0, 0);
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, fPercentColDatetime));

            int iCountSubColumns = 0;
            
            for (i = 0; i < listTec.Count; i++)
            {
                this.Controls.Add(new PanelTecLastMinutes(listTec[i]), i + 1, 0);
                iCountSubColumns += ((PanelTecLastMinutes)this.Controls [i + 1]).CountTECComponent; //Слева столбец дата/время
            }

            //Размеры столбцов после создания столбцов, т.к.
            //кол-во "подстолбцов" в столбцах до их создания неизвестно
            for (i = 0; i < listTec.Count; i++)
            {
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, ((float)100 - fPercentColDatetime) / iCountSubColumns * ((PanelTecLastMinutes)this.Controls[i + 1]).CountTECComponent));
            }
        }

        public PanelLastMinutes(IContainer container, List<TEC> listTec, StatusStrip stsStrip, AdminTS admin)
            : this(listTec, stsStrip, admin)
        {
            container.Add(this);
        }

        public void SetDelegate(DelegateFunc dStatus)
        {
            this.delegateEventUpdate = dStatus;
        }

        public void Start()
        {
            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecLastMinutes)
                {
                    if ((HAdmin.DEBUG_INDEX_TEC == -1) || (i == HAdmin.DEBUG_INDEX_TEC)) ((PanelTecLastMinutes)ctrl).Start(); else ;
                    i++;
                }
                else
                    ;
            }
        }

        public void Stop()
        {
            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecLastMinutes)
                {
                    if ((HAdmin.DEBUG_INDEX_TEC == -1) || (i == HAdmin.DEBUG_INDEX_TEC)) ((PanelTecLastMinutes)ctrl).Stop(); else ;
                    i++;
                }
                else
                    ;
            }
        }

        public void Activate(bool active)
        {
            if (m_bIsActive == active)
                return;
            else
                ;

            m_bIsActive = active;

            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecLastMinutes)
                {
                    if ((HAdmin.DEBUG_INDEX_TEC == -1) || (i == HAdmin.DEBUG_INDEX_TEC)) ((PanelTecLastMinutes)ctrl).Activate(active); else ;
                    i++;
                }
                else
                    ;
            }
        }

        partial class PanelDateTime
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

        private partial class PanelDateTime : TableLayoutPanel
        {
            private Dictionary<int, Label> m_dictLabelTime;

            public PanelDateTime()
            {
                InitializeComponent();

                Initialize();
            }

            public PanelDateTime(IContainer container)
                : this()
            {
                container.Add(this);
            }

            private void Initialize()
            {
                int i = -1;
           
                this.Dock = DockStyle.Fill;
                this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle
                this.RowCount = 24 + COUNT_FIXED_ROWS;

                DateTime dtNow = DateTime.Now.Date;

                //Добавить дату
                Label lblDate = HLabel.createLabel(dtNow.ToString (@"dd.MM.yyyy"), PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_TEC]);
                this.Controls.Add(lblDate, 0, 0);
                this.SetRowSpan(lblDate, COUNT_FIXED_ROWS);

                m_dictLabelTime = new Dictionary<int,Label> ();

                dtNow = dtNow.AddMinutes(59);
                for (i = 1; i < 25; i++)
                {
                    m_dictLabelTime[i - 1] = HLabel.createLabel(dtNow.ToString (@"HH:mm"), PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.DATETIME]);
                    this.Controls.Add(m_dictLabelTime[i - 1], 0, (i - 1) + COUNT_FIXED_ROWS);

                    dtNow = dtNow.AddHours(1);
                }

                for (i = 0; i < (24 + COUNT_FIXED_ROWS - 1); i++)
                {
                    this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                }

                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 6F));
            }
        }

        partial class PanelTecLastMinutes
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

        public partial class PanelTecLastMinutes : TableLayoutPanel
        {
            private TEC m_tec;
            List<TECComponentBase> m_list_TECComponents;
            public int CountTECComponent { get { return m_list_TECComponents.Count; } }

            private List<TG> m_listSensorId2TG;

            private Dictionary<int, Label[]> m_dictLabelVal;
            private Dictionary<int, ToolTip[]> m_dictToolTip;

            private object lockValue;

            private bool m_bIsActive,
                        m_bIsStarted,
                        m_bUpdate;

            private Dictionary<int, PanelTecView.valuesTECComponent> m_dictValuesHours;

            DataTable m_tablePBRResponse;

            private volatile string sensorsString_TM;

            private Thread m_taskThread;
            private Semaphore m_semaState;
            private volatile bool m_bThreadIsWorking;
            private volatile bool m_bIsNewState;
            private volatile List<StatesMachine> m_states;
            private ManualResetEvent m_evTimerCurrent;
            private System.Threading.Timer m_timerCurrent;

            private DelegateFunc delegateUpdateGUI_TM;

            public PanelTecLastMinutes(TEC tec)
            {
                InitializeComponent();

                m_tec = tec;
                Initialize();
            }

            public PanelTecLastMinutes(IContainer container, TEC tec)
                : this(tec)
            {
                container.Add(this);
            }

            private void Initialize()
            {
                int i = -1;
                m_list_TECComponents = new List<TECComponentBase> ();
                m_dictLabelVal = new Dictionary<int, Label[]>();
                m_dictToolTip = new Dictionary<int, ToolTip[]>();

                this.Dock = DockStyle.Fill;
                this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle
                this.RowCount = 24 + COUNT_FIXED_ROWS;

                //Добавить наименование станции
                Label lblNameTEC = HLabel.createLabel(m_tec.name_shr, PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_TEC]);
                this.Controls.Add(lblNameTEC, 0, 0);
                
                foreach (TECComponent g in m_tec.list_TECComponents)
                {
                    if ((g.m_id > 100) && (g.m_id < 500))
                    {
                        //Добавить наименование ГТП
                        this.Controls.Add(HLabel.createLabel(g.name_shr.Split (' ')[1], PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_COMPONENT]), CountTECComponent, COUNT_FIXED_ROWS - 1);

                        //Память под ячейки со значениями
                        m_dictLabelVal.Add(g.m_id, new Label[24]);
                        m_dictToolTip.Add(g.m_id, new ToolTip[24]);

                        for (i = 0; i < 24; i ++)
                        {
                            m_dictLabelVal[g.m_id][i] = HLabel.createLabel (0.ToString (@"F2"), PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.VALUE_COMPONENT]);
                            m_dictToolTip[g.m_id][i] = new ToolTip();
                            m_dictToolTip[g.m_id][i].IsBalloon = true;
                            m_dictToolTip[g.m_id][i].ShowAlways = true;
                            m_dictToolTip[g.m_id][i].SetToolTip(m_dictLabelVal[g.m_id][i], @"ПБР: ---, d: --%");

                            this.Controls.Add(m_dictLabelVal[g.m_id][i], CountTECComponent, i + COUNT_FIXED_ROWS);
                        }

                        //Добавить компонент ТЭЦ (ГТП)
                        m_list_TECComponents.Add(g);
                    }
                    else
                        ;
                }

                for (i = 0; i < (24 + COUNT_FIXED_ROWS - 1); i++)
                {
                    this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                }

                for (i = 0; i < CountTECComponent; i++)
                {
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / CountTECComponent));
                }

                if (CountTECComponent > 0)
                    this.SetColumnSpan(lblNameTEC, CountTECComponent);
                else
                    ;

                lockValue = new object();
                m_listSensorId2TG = new List<TG>(); //[this.RowCount - COUNT_FIXED_ROWS];
                sensorsString_TM = string.Empty;
                m_states = new List<StatesMachine>();
                //delegateUpdateGUI_TM = ShowTMPower;

                m_dictValuesHours = new Dictionary<int,PanelTecView.valuesTECComponent> ();
                foreach (TECComponent c in m_list_TECComponents)
                {
                    m_dictValuesHours.Add(c.m_id, new PanelTecView.valuesTECComponent(24 + 1));
                }

                delegateUpdateGUI_TM = ShowLastMinutesTM;
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
                m_taskThread.Name = @"Интерфейс к данным (" + GetType ().Name + "): " + m_tec.name_shr + @" - текущие значения...";
                m_taskThread.IsBackground = true;

                m_semaState = new Semaphore(1, 1);

                m_semaState.WaitOne();
                m_taskThread.Start();

                ClearValues();

                //Милисекунды до первого запуска функции таймера
                double msecUpdate = (new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour + 1, 11, 0) - DateTime.Now).TotalMilliseconds;

                m_evTimerCurrent = new ManualResetEvent(true);
                m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, (Int64) msecUpdate, ((PanelLastMinutes)Parent).m_msecPeriodUpdate - 1);

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
                    catch (Exception excpt) { Logging.Logg().LogExceptionToFile(excpt, "catch - PanelTecView.Stop () - sem.Release(1)"); }

                    joined = m_taskThread.Join(1000);
                    if (!joined)
                        m_taskThread.Abort();
                    else
                        ;
                }

                m_tec.StopDbInterfaces();

                lock (lockValue)
                {
                    ((PanelLastMinutes)Parent).errored_state = false;
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

                m_states.Add(StatesMachine.PBRValues);
                m_states.Add(StatesMachine.AdminValues);
                m_states.Add(StatesMachine.LastMinutes_TM);
            }

            public void Activate(bool active)
            {
                if (m_bIsActive == active)
                    return;
                else
                    ;

                m_bIsActive = active;

                if (m_bIsActive == true)
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
                else
                {
                    lock (lockValue)
                    {
                        m_bIsNewState = true;
                        m_states.Clear();
                        ((PanelLastMinutes)Parent).errored_state =
                        ((PanelLastMinutes)Parent).actioned_state = false;
                    }
                }
            }

            private void ErrorReport(string error_string)
            {
                lock (lockValue)
                {
                    ((PanelLastMinutes)Parent).last_error = error_string;
                    ((PanelLastMinutes)Parent).last_time_error = DateTime.Now;
                    ((PanelLastMinutes)Parent).errored_state = true;
                    ((PanelLastMinutes)Parent).m_stsStrip.BeginInvoke(((PanelLastMinutes)Parent).delegateEventUpdate);
                }
            }

            private void ActionReport(string action_string)
            {
                lock (lockValue)
                {
                    ((PanelLastMinutes)Parent).last_action = action_string;
                    ((PanelLastMinutes)Parent).last_time_action = DateTime.Now;
                    ((PanelLastMinutes)Parent).actioned_state = true;
                    ((PanelLastMinutes)Parent).m_stsStrip.BeginInvoke(((PanelLastMinutes)Parent).delegateEventUpdate);
                }
            }

            private void ShowLastMinutesTM()
            {
                Color clrBackColor;
                double dblPercent = 0.0,
                    dblUDGe = 0.0;
                string strToolTip = string.Empty;
                
                foreach (TECComponent g in m_list_TECComponents)
                {
                    for (int hour = 1; hour < 25; hour++)
                    {
                        clrBackColor = s_clrBakColorLabelVal;
                        dblPercent = 0.0;
                        strToolTip = string.Empty;

                        //if ((i > 0) && ((i - 0) < m_dictValuesHours[g.m_id].valuesUDGe.Length))
                            dblUDGe = m_dictValuesHours[g.m_id].valuesUDGe[hour];
                        //else
                        //    dblUDGe = 0.0;

                        strToolTip += @"УДГе=" + dblUDGe.ToString (@"F2");

                        if ((m_dictValuesHours[g.m_id].valuesLastMinute[hour] > 1) && (dblUDGe > 1))
                            dblPercent = (m_dictValuesHours[g.m_id].valuesLastMinute[hour] - dblUDGe) / dblUDGe * 100;
                        else
                            ;
                        strToolTip += @";Мон2%:" + dblPercent.ToString (@"F1");

                        m_dictToolTip[g.m_id][hour - 1].SetToolTip(m_dictLabelVal[g.m_id][hour - 1], strToolTip);

                        if (m_dictValuesHours[g.m_id].valuesLastMinute[hour] > 1) {
                            m_dictLabelVal[g.m_id][hour - 1].Text = m_dictValuesHours[g.m_id].valuesLastMinute[hour].ToString(@"F2");

                            if ((dblUDGe > 1) &&
                                (m_dictValuesHours[g.m_id].valuesLastMinute[hour] > 1) &&
                                ((!(Math.Abs(dblPercent) < 2))))
                            {
                                clrBackColor = Color.Red;
                            }
                            else
                                ;
                        }
                        else
                            m_dictLabelVal[g.m_id][hour - 1].Text = 0.ToString (@"F0");

                        m_dictLabelVal[g.m_id][hour - 1].BackColor = clrBackColor;
                    }
                }
            }

            private void PanelTecCurPower_TextChangedValue(object sender, EventArgs ev)
            {
                double val = -1.0;
                Color clr;
                if (double.TryParse(((Label)sender).Text, out val) == true)
                {
                    if (val > 1)
                        clr = Color.LimeGreen;
                    else
                        clr = Color.Green;
                }
                else
                    clr = Color.Green;

                ((Label)sender).ForeColor = clr;
            }

            private void GetLastMinutesTMRequest()
            {
                m_tec.Request(CONN_SETT_TYPE.DATA_TM, m_tec.lastMinutesTMRequest(DateTime.Now.Date, sensorsString_TM));
            }

            private bool GetLastMinutesTMResponse(DataTable table_in)
            {
                bool bRes = true;
                int i = -1,
                    hour = -1,
                    offsetUTC = (int)HAdmin.GetUTCOffsetOfCurrentTimeZone ().TotalHours;
                double value = -1;
                DateTime dtVal;
                DataRow [] tgRows = null;

                foreach (TECComponent g in m_tec.list_TECComponents)
                {
                    foreach (TG tg in g.TG)
                    {
                        for (i = 0; i < tg.power_LastMinutesTM.Length; i++)
                        {
                            tg.power_LastMinutesTM[i] = 0;
                        }

                        tgRows = table_in.Select (@"[ID]=" + tg.id_tm);
                        
                        for (i = 0; i < tgRows.Length; i++)
                        {
                            if (double.TryParse(tgRows[i]["value"].ToString(), out value) == false)
                                return false;
                            else
                                ;

                            //if (DateTime.TryParse(table.Rows[i]["last_changed_at"].ToString(), out m_dtLastChangedAt) == false)
                            //    return false;
                            //else
                            //    ;

                            switch (m_tec.type())
                            {
                                case TEC.TEC_TYPE.COMMON:
                                    break;
                                case TEC.TEC_TYPE.BIYSK:
                                    //value *= 20;
                                    break;
                                default:
                                    break;
                            }

                            if (DateTime.TryParse(tgRows[i]["last_changed_at"].ToString(), out dtVal) == false)
                                return false;
                            else
                                ;

                            hour = dtVal.Hour + offsetUTC;
                            if (! (hour < 24)) {
                                hour -= 24;
                            }
                            else ;

                            tg.power_LastMinutesTM[hour] = value;
                            m_dictValuesHours [g.m_id].valuesLastMinute [hour] += value;
                        }
                    }
                }

                return bRes;
            }

            private TG FindTGById(int id, TG.INDEX_VALUE indxVal, TG.ID_TIME id_type)
            {
                for (int i = 0; i < m_listSensorId2TG.Count; i++)
                    switch (indxVal)
                    {
                        case TG.INDEX_VALUE.FACT:
                            if (m_listSensorId2TG[i].ids_fact[(int)id_type] == id)
                                return m_listSensorId2TG[i];
                            else
                                ;
                            break;
                        case TG.INDEX_VALUE.TM:
                            if (m_listSensorId2TG[i].id_tm == id)
                                return m_listSensorId2TG[i];
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

                                    m_listSensorId2TG.Add(m_tec.list_TECComponents[j].TG[k]); ;

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

                sensorsString_TM = string.Empty;

                for (int i = 0; i < m_listSensorId2TG.Count; i++)
                {
                    if (!(m_listSensorId2TG[i] == null))
                    {
                        if (sensorsString_TM.Equals(string.Empty) == false)
                            sensorsString_TM += @" OR ";
                        else
                            ;

                        sensorsString_TM += "[dbo].[NAME_TABLE].[ID] = " + m_listSensorId2TG[i].id_tm.ToString();
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
                            case TEC.TEC_TYPE.BIYSK:
                                GetSensorsTMRequest();
                                break;
                            //case TEC.TEC_TYPE.BIYSK:
                            //break;
                            default:
                                break;
                        }
                        break;
                    case StatesMachine.LastMinutes_TM:
                        ActionReport("Получение текущих значений.");
                        //adminValuesReceived = false;
                        GetLastMinutesTMRequest();
                        break;
                    case StatesMachine.PBRValues:
                        ActionReport("Получение данных плана.");
                        //adminValuesReceived = false;
                        GetPBRValuesRequest();
                        break;
                    case StatesMachine.AdminValues:
                        ActionReport("Получение административных значений.");
                        //adminValuesReceived = false;
                        GetAdminValuesRequest(((PanelLastMinutes)Parent).m_admin.m_typeFields);
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
                            case TEC.TEC_TYPE.BIYSK:
                                return m_tec.Response(CONN_SETT_TYPE.DATA_TM, out error, out table);
                        }
                        break;
                    case StatesMachine.LastMinutes_TM:
                        return m_tec.Response(CONN_SETT_TYPE.DATA_TM, out error, out table);
                    case StatesMachine.PBRValues:
                        return ((PanelLastMinutes)Parent).m_admin.Response(m_tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.PBR], m_tec.m_arListenerIds[(int)CONN_SETT_TYPE.PBR], out error, out table);
                    case StatesMachine.AdminValues:
                        return ((PanelLastMinutes)Parent).m_admin.Response(m_tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], m_tec.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], out error, out table);
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
                            case TEC.TEC_TYPE.BIYSK:
                                result = GetSensorsTMResponse(table);
                                break;
                        }
                        if (result == true)
                        {
                        }
                        break;
                    case StatesMachine.LastMinutes_TM:
                        ClearValues (); //Пока пустая???
                        result = GetLastMinutesTMResponse(table);
                        if (result == true)
                        {
                        }
                        else
                            ;
                        break;
                    case StatesMachine.PBRValues:
                        result = GetPBRValuesResponse(table);
                        if (result == true)
                        {
                        }
                        else
                            ;
                        break;
                    case StatesMachine.AdminValues:
                        result = GetAdminValuesResponse(table);
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
                        ((PanelLastMinutes)Parent).errored_state =
                        ((PanelLastMinutes)Parent).actioned_state = false;
                    }
                else
                    ;

                return result;
            }

            private void StateErrors(StatesMachine state, bool response)
            {
                string error = string.Empty,
                        reason = string.Empty,
                        waiting = string.Empty;

                switch (state)
                {
                    case StatesMachine.Init_TM:
                        reason = @"идентификаторов датчиков (телемеханика)";
                        waiting = @"Переход в ожидание";
                        break;
                    case StatesMachine.LastMinutes_TM:
                        reason = @"значений крайних минут часа";
                        //waiting = @"Ожидание " + (((PanelLastMinutes)Parent).m_msecPeriodUpdate / 1000).ToString() + " секунд";
                        waiting = @"Переход в ожидание";
                        break;
                    case StatesMachine.PBRValues:
                        reason = @"данных плана";
                        break;
                    case StatesMachine.AdminValues:
                        reason = @"административных значений";
                        break;
                }

                if (response)
                    reason = @"разбора " + reason;
                else
                    reason = @"получения " + reason;

                error = "Ошибка " + reason + ".";

                if (waiting.Equals(string.Empty) == true)
                    error += " " + waiting + ".";
                else
                    ;

                ErrorReport(error);
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
                            for (int j = 0; (j < DbInterface.MAX_WAIT_COUNT) && (dataPresent == false) && (error == false) && (m_bIsNewState == false); j++)
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
                    if (m_bIsActive == true)
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
                    else
                        ;
                }
            }

            private void GetPBRValuesRequest()
            {
                lock (lockValue)
                {
                    ((PanelLastMinutes)Parent).m_admin.Request(m_tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.PBR], m_tec.m_arListenerIds[(int)CONN_SETT_TYPE.PBR], m_tec.GetPBRValueQuery(-1, DateTime.Now.Date, AdminTS.TYPE_FIELDS.DYNAMIC));
                }
            }

            private bool GetPBRValuesResponse(DataTable table)
            {
                bool bRes = true;

                if (!(table == null))
                    m_tablePBRResponse = table.Copy();
                else
                    ;

                return bRes;
            }

            private void GetAdminValuesRequest(AdminTS.TYPE_FIELDS mode)
            {
                lock (lockValue)
                {
                    ((PanelLastMinutes)Parent).m_admin.Request(m_tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], m_tec.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], m_tec.GetAdminValueQuery(-1, DateTime.Now.Date, mode));
                }
            }

            private bool GetAdminValuesResponse(DataTable table_in)
            {
                DateTime date = DateTime.Now.Date
                         , dtPBR;
                int hour;

                int offsetPrev = -1
                    //, tableRowsCount = table_in.Rows.Count; //tableRowsCount = m_tablePBRResponse.Rows.Count
                    , i = -1, j = -1,
                    offsetUDG, offsetPlan, offsetLayout;

                offsetUDG = 1;
                offsetPlan = /*offsetUDG + 3 * tec.list_TECComponents.Count +*/ 1; //ID_COMPONENT
                offsetLayout = -1;

                m_tablePBRResponse = restruct_table_pbrValues(m_tablePBRResponse);
                offsetLayout = (!(m_tablePBRResponse.Columns.IndexOf("PBR_NUMBER") < 0)) ? (offsetPlan + m_list_TECComponents.Count * 3) : m_tablePBRResponse.Columns.Count;

                table_in = restruct_table_adminValues(table_in);

                //if (!(table_in.Columns.IndexOf("ID_COMPONENT") < 0))
                //    try { table_in.Columns.Remove("ID_COMPONENT"); }
                //    catch (Exception excpt)
                //    {
                //        /*
                //        Logging.Logg().LogExceptionToFile(excpt, "catch - PanelTecView.GetAdminValuesResponse () - ...");
                //        */
                //    }
                //else
                //    ;

                // поиск в таблице записи по предыдущим суткам (мало ли, вдруг нету)
                for (i = 0; i < m_tablePBRResponse.Rows.Count && offsetPrev < 0; i++)
                {
                    if (!(m_tablePBRResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                    {
                        try
                        {
                            hour = ((DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"]).Hour;
                            if (hour == 0 && ((DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"]).Day == date.Day)
                            {
                                offsetPrev = i;
                                //foreach (TECComponent g in tec.list_TECComponents)
                                for (j = 0; j < m_list_TECComponents.Count; j++)
                                {
                                    if ((offsetPlan + j * 3) < m_tablePBRResponse.Columns.Count)
                                        m_dictValuesHours [m_list_TECComponents [j].m_id].valuesPBR[0] = (double)m_tablePBRResponse.Rows[i][offsetPlan + j * 3];
                                    else
                                         m_dictValuesHours [m_list_TECComponents [j].m_id].valuesPBR[0] = 0.0;
                                    //j++;
                                }
                            }
                            else
                                ;
                        }
                        catch (Exception excpt) { Logging.Logg().LogExceptionToFile(excpt, "catch - PanelTecView.GetAdminValuesResponse () - ..."); }
                    }
                    else
                    {
                        try
                        {
                            hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
                            if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day == date.Day)
                            {
                                offsetPrev = i;
                            }
                            else
                                ;
                        }
                        catch
                        {
                        }
                    }
                }

                // разбор остальных значений
                for (i = 0; i < m_tablePBRResponse.Rows.Count; i++)
                {
                    if (i == offsetPrev)
                        continue;

                    if (!(m_tablePBRResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                    {
                        try
                        {
                            dtPBR = (DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"];

                            hour = dtPBR.Hour;
                            if ((hour == 0) && (!(dtPBR.Day == date.Day)))
                                hour = 24;
                            else
                                if (hour == 0)
                                    continue;
                                else
                                    ;

                            //foreach (TECComponent g in tec.list_TECComponents)
                            for (j = 0; j < m_list_TECComponents.Count; j++)
                            {
                                try
                                {
                                    if (((offsetPlan + (j * 3)) < m_tablePBRResponse.Columns.Count) && (!(m_tablePBRResponse.Rows[i][offsetPlan + (j * 3)] is System.DBNull)))
                                         m_dictValuesHours [m_list_TECComponents [j].m_id].valuesPBR[hour] = (double)m_tablePBRResponse.Rows[i][offsetPlan + (j * 3)];
                                    else
                                        ;

                                    DataRow[] row_in = table_in.Select("DATE_ADMIN = '" + dtPBR.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                                    if (row_in.Length > 0)
                                    {
                                        if (row_in.Length > 1)
                                            ; //Ошибка...
                                        else
                                            ;

                                        if (!(row_in[0][offsetUDG + j * 3] is System.DBNull))
                                            //if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull)))
                                            m_dictValuesHours[m_list_TECComponents[j].m_id].valuesREC[hour] = (double)row_in[0][offsetUDG + j * 3];
                                        else
                                            m_dictValuesHours[m_list_TECComponents[j].m_id].valuesREC[hour] = 0;

                                        if (!(row_in[0][offsetUDG + 1 + j * 3] is System.DBNull))
                                            m_dictValuesHours[m_list_TECComponents[j].m_id].valuesISPER[hour] = (int)row_in[0][offsetUDG + 1 + j * 3];
                                        else
                                            ;

                                        if (!(row_in[0][offsetUDG + 2 + j * 3] is System.DBNull))
                                            m_dictValuesHours [m_list_TECComponents[j].m_id].valuesDIV[hour] = (double)row_in[0][offsetUDG + 2 + j * 3];
                                        else
                                            ;
                                    }
                                    else
                                    {
                                        m_dictValuesHours[m_list_TECComponents[j].m_id].valuesREC[hour] = 0;
                                    }
                                }
                                catch
                                {
                                }
                                //j++;
                            }
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        try
                        {
                            hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
                            if ((hour == 0) && (!(((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day == date.Day)))
                                hour = 24;
                            else
                                ;

                            //foreach (TECComponent g in tec.list_TECComponents)
                            for (j = 0; j < m_list_TECComponents.Count; j++)
                            {
                                try
                                {
                                     m_dictValuesHours [m_list_TECComponents [j].m_id].valuesPBR[hour] = 0;

                                    if (i < table_in.Rows.Count)
                                    {
                                        if (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull))
                                            //if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull)))
                                            m_dictValuesHours[m_list_TECComponents[j].m_id].valuesREC[hour] = (double)table_in.Rows[i][offsetUDG + j * 3];
                                        else
                                            m_dictValuesHours[m_list_TECComponents[j].m_id].valuesREC[hour] = 0;

                                        if (!(table_in.Rows[i][offsetUDG + 1 + j * 3] is System.DBNull))
                                            m_dictValuesHours[m_list_TECComponents[j].m_id].valuesISPER[hour] = (int)table_in.Rows[i][offsetUDG + 1 + j * 3];
                                        else
                                            ;

                                        if (!(table_in.Rows[i][offsetUDG + 2 + j * 3] is System.DBNull))
                                            m_dictValuesHours[m_list_TECComponents[j].m_id].valuesDIV[hour] = (double)table_in.Rows[i][offsetUDG + 2 + j * 3];
                                        else
                                            ;
                                    }
                                    else
                                    {
                                        m_dictValuesHours[m_list_TECComponents[j].m_id].valuesREC[hour] = 0;
                                    }
                                }
                                catch
                                {
                                }
                                //j++;
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                for (i = 1; i < 25; i++)
                {
                    for (j = 0; j < m_list_TECComponents.Count; j++)
                    {
                        if (i == 1)
                        {
                            m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPBRe [i] = (m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPBR[i] + m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPBR[0]) / 2;
                        }
                        else
                        {
                            m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPBRe[i] = (m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPBR[i] + m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPBR[i - 1]) / 2;
                        }

                        m_dictValuesHours[m_list_TECComponents[j].m_id].valuesUDGe[i] = m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPBRe[i] + m_dictValuesHours[m_list_TECComponents[j].m_id].valuesREC[i];

                        if (m_dictValuesHours[m_list_TECComponents[j].m_id].valuesISPER[i] == 1)
                            m_dictValuesHours[m_list_TECComponents[j].m_id].valuesDiviation[i] = (m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPBRe[i] +
                                                                                                    m_dictValuesHours[m_list_TECComponents[j].m_id].valuesREC[i]) * m_dictValuesHours[m_list_TECComponents[j].m_id].valuesDIV[i] / 100;
                        else
                            m_dictValuesHours[m_list_TECComponents[j].m_id].valuesDiviation[i] = m_dictValuesHours[m_list_TECComponents[j].m_id].valuesDIV[i];
                    }
                }
                
                return true;
            }

            private DataTable restruct_table_pbrValues(DataTable table_in)
            {
                DataTable table_in_restruct = new DataTable();
                List<DataColumn> cols_data = new List<DataColumn>();
                DataRow[] dataRows;
                int i = -1, j = -1, k = -1;
                string nameFieldDate = "DATE_PBR"; // m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_DATETIME]

                for (i = 0; i < table_in.Columns.Count; i++)
                {
                    if (table_in.Columns[i].ColumnName == "ID_COMPONENT")
                    {
                        //Преобразование таблицы
                        break;
                    }
                    else
                        ;
                }

                if (i < table_in.Columns.Count)
                {
                    List<TG> list_TG = null;
                    List<TECComponent> list_TECComponents = null;
                    int count_comp = -1;

                        list_TECComponents = new List<TECComponent>();
                        for (i = 0; i < m_tec.list_TECComponents.Count; i++)
                        {
                            if ((m_tec.list_TECComponents[i].m_id > 100) && (m_tec.list_TECComponents[i].m_id < 500))
                                list_TECComponents.Add(m_tec.list_TECComponents[i]);
                            else
                                ;
                        }

                    //Преобразование таблицы
                    for (i = 0; i < table_in.Columns.Count; i++)
                    {
                        if ((!(table_in.Columns[i].ColumnName.Equals("ID_COMPONENT") == true))
                            && (!(table_in.Columns[i].ColumnName.Equals(nameFieldDate) == true))
                            && (!(table_in.Columns[i].ColumnName.Equals(m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]) == true)))
                        //if (!(table_in.Columns[i].ColumnName == "ID_COMPONENT"))
                        {
                            cols_data.Add(table_in.Columns[i]);
                        }
                        else
                            if ((table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)
                                || (table_in.Columns[i].ColumnName.Equals(m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]) == true))
                            {
                                table_in_restruct.Columns.Add(table_in.Columns[i].ColumnName, table_in.Columns[i].DataType);
                            }
                            else
                                ;
                    }

                        count_comp = list_TECComponents.Count;

                    for (i = 0; i < count_comp; i++)
                    {
                        for (j = 0; j < cols_data.Count; j++)
                        {
                            table_in_restruct.Columns.Add(cols_data[j].ColumnName, cols_data[j].DataType);
                                table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TECComponents[i].m_id;
                        }
                    }

                    if (m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                        table_in_restruct.Columns[m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]].SetOrdinal(table_in_restruct.Columns.Count - 1);
                    else
                        ;

                    List<DataRow[]> listDataRows = new List<DataRow[]>();

                    for (i = 0; i < count_comp; i++)
                    {
                            dataRows = table_in.Select("ID_COMPONENT=" + list_TECComponents[i].m_id);

                        listDataRows.Add(new DataRow[dataRows.Length]);
                        dataRows.CopyTo(listDataRows[i], 0);

                        int indx_row = -1;
                        for (j = 0; j < listDataRows[i].Length; j++)
                        {
                            for (k = 0; k < table_in_restruct.Rows.Count; k++)
                            {
                                if (table_in_restruct.Rows[k][nameFieldDate].Equals(listDataRows[i][j][nameFieldDate]) == true)
                                    break;
                                else
                                    ;
                            }

                            if (!(k < table_in_restruct.Rows.Count))
                            {
                                table_in_restruct.Rows.Add();

                                indx_row = table_in_restruct.Rows.Count - 1;

                                //Заполнение DATE_ADMIN (постоянные столбцы)
                                table_in_restruct.Rows[indx_row][nameFieldDate] = listDataRows[i][j][nameFieldDate];
                                if (m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                                    table_in_restruct.Rows[indx_row][m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]] = listDataRows[i][j][m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]];
                                else
                                    ;
                            }
                            else
                                indx_row = k;

                            for (k = 0; k < cols_data.Count; k++)
                            {
                                    table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_TECComponents[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                            }
                        }
                    }
                }
                else
                    table_in_restruct = table_in;

                return table_in_restruct;
            }

            private DataTable restruct_table_adminValues(DataTable table_in)
            {
                DataTable table_in_restruct = new DataTable();
                List<DataColumn> cols_data = new List<DataColumn>();
                DataRow[] dataRows;
                int i = -1, j = -1, k = -1;
                string nameFieldDate = "DATE_ADMIN"; // m_tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.ADMIN_DATETIME]

                for (i = 0; i < table_in.Columns.Count; i++)
                {
                    if (table_in.Columns[i].ColumnName == "ID_COMPONENT")
                    {
                        //Преобразование таблицы
                        break;
                    }
                    else
                        ;
                }

                if (i < table_in.Columns.Count)
                {
                    List<TG> list_TG = null;
                    List<TECComponent> list_TECComponents = null;
                    int count_comp = -1;

                        list_TECComponents = new List<TECComponent>();
                        for (i = 0; i < m_tec.list_TECComponents.Count; i++)
                        {
                            if ((m_tec.list_TECComponents[i].m_id > 100) && (m_tec.list_TECComponents[i].m_id < 500))
                                list_TECComponents.Add(m_tec.list_TECComponents[i]);
                            else
                                ;
                        }

                    //Преобразование таблицы
                    for (i = 0; i < table_in.Columns.Count; i++)
                    {
                        if ((!(table_in.Columns[i].ColumnName == "ID_COMPONENT")) && (!(table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)))
                        //if (!(table_in.Columns[i].ColumnName == "ID_COMPONENT"))
                        {
                            cols_data.Add(table_in.Columns[i]);
                        }
                        else
                            if (table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)
                            {
                                table_in_restruct.Columns.Add(table_in.Columns[i].ColumnName, table_in.Columns[i].DataType);
                            }
                            else
                                ;
                    }

                        count_comp = list_TECComponents.Count;

                    for (i = 0; i < count_comp; i++)
                    {
                        for (j = 0; j < cols_data.Count; j++)
                        {
                            table_in_restruct.Columns.Add(cols_data[j].ColumnName, cols_data[j].DataType);

                                table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TECComponents[i].m_id;
                        }
                    }

                    List<DataRow[]> listDataRows = new List<DataRow[]>();

                    for (i = 0; i < count_comp; i++)
                    {
                            dataRows = table_in.Select("ID_COMPONENT=" + list_TECComponents[i].m_id);
                        listDataRows.Add(new DataRow[dataRows.Length]);
                        dataRows.CopyTo(listDataRows[i], 0);

                        int indx_row = -1;
                        for (j = 0; j < listDataRows[i].Length; j++)
                        {
                            for (k = 0; k < table_in_restruct.Rows.Count; k++)
                            {
                                if (table_in_restruct.Rows[k][nameFieldDate].Equals(listDataRows[i][j][nameFieldDate]) == true)
                                    break;
                                else
                                    ;
                            }

                            if (!(k < table_in_restruct.Rows.Count))
                            {
                                table_in_restruct.Rows.Add();

                                indx_row = table_in_restruct.Rows.Count - 1;

                                //Заполнение DATE_ADMIN (постоянные столбцы)
                                table_in_restruct.Rows[indx_row][nameFieldDate] = listDataRows[i][j][nameFieldDate];
                            }
                            else
                                indx_row = k;

                            for (k = 0; k < cols_data.Count; k++)
                            {
                                    table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_TECComponents[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                            }
                        }
                    }
                }
                else
                    table_in_restruct = table_in;

                return table_in_restruct;
            }

            private void ClearValues()
            {
                foreach (TECComponent g in m_list_TECComponents)
                {
                    for (int i = 0; i < m_dictValuesHours[g.m_id].valuesLastMinute.Length; i++)
                        m_dictValuesHours[g.m_id].valuesLastMinute[i] = 0.0;
                }
            }
        }
    }
}
