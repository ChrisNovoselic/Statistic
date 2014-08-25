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
    partial class PanelTMSNPower
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

    public partial class PanelTMSNPower : PanelStatisticView
    {
        enum INDEX_LABEL : int
        {
            NAME,
            DATETIME_TM,
            DATETIME_TM_SN,
            VALUE_TM,
            VALUE_TM_SN,
            COUNT_INDEX_LABEL
        };
        const int COUNT_FIXED_ROWS = (int)INDEX_LABEL.VALUE_TM_SN - 1;

        static Color s_clrBackColorLabel = Color.FromArgb(212, 208, 200), s_clrBackColorLabelVal_TM = Color.FromArgb(219, 223, 227), s_clrBackColorLabelVal_TM_SN = Color.FromArgb(219, 223, 247);
        static HLabelStyles[] s_arLabelStyles = { new HLabelStyles(Color.Black, s_clrBackColorLabel, 22F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM, 18F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM_SN, 18F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM, 18F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM_SN, 18F, ContentAlignment.MiddleCenter)};

        enum StatesMachine : int { Init_TM, Current_TM_Gen, Current_TM_SN };

        public int m_msecPeriodUpdate;

        //HReports m_report;

        public bool m_bIsActive;

        public StatusStrip m_stsStrip;

        public PanelTMSNPower(List<StatisticCommon.TEC> listTec, StatusStrip stsStrip, FormParameters par, HReports rep)
        {
            InitializeComponent();

            m_report = rep;

            m_stsStrip = stsStrip;
            m_msecPeriodUpdate = Int32.Parse(par.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME]);

            this.Dock = DockStyle.Fill;

            //this.Location = new System.Drawing.Point(40, 58);
            //this.Name = "pnlView";
            //this.Size = new System.Drawing.Size(705, 747);

            this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle;

            PanelTecTMSNPower ptcp;

            int i = -1;

            this.ColumnCount = listTec.Count / 2;
            if (this.ColumnCount == 0) this.ColumnCount++; else ;
            this.RowCount = listTec.Count / this.ColumnCount;

            for (i = 0; i < listTec.Count; i++)
            {
                ptcp = new PanelTecTMSNPower(listTec[i]);
                this.Controls.Add(ptcp, i % this.ColumnCount, i / this.ColumnCount);
            }

            for (i = 0; i < this.ColumnCount; i++)
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / this.ColumnCount));

            for (i = 0; i < this.RowCount; i++)
                this.RowStyles.Add(new RowStyle(SizeType.Percent, 100 / this.RowCount));
        }

        public PanelTMSNPower(IContainer container, List<StatisticCommon.TEC> listTec, StatusStrip stsStrip, FormParameters par, HReports rep)
            : this(listTec, stsStrip, par, rep)
        {
            container.Add(this);
        }

        public override void Start()
        {
            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecTMSNPower)
                {
                    if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecTMSNPower)ctrl).m_tec.m_id)) ((PanelTecTMSNPower)ctrl).Start(); else ;
                    i++;
                }
                else
                    ;
            }
        }

        public override void Stop()
        {
            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecTMSNPower)
                {
                    if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecTMSNPower)ctrl).m_tec.m_id)) ((PanelTecTMSNPower)ctrl).Stop(); else ;
                    i++;
                }
                else
                    ;
            }
        }

        public override void Activate(bool active)
        {
            if (m_bIsActive == active)
                return;
            else
                ;

            m_bIsActive = active;

            //TypeConverter conv;
            //dynamic dynObj = null;
            Type typeChildren; //PanelTecCurPower
            typeChildren = typeof(PanelTecTMSNPower);

            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl.GetType().Equals(typeChildren) == true)
                {
                    if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == (((PanelTecTMSNPower)ctrl).m_tec.m_id)))
                    {
                        ((PanelTecTMSNPower)ctrl).Activate(active);
                    }
                    else
                        ;
                    i++;
                }
                else
                    ;
            }
        }

        partial class PanelTecTMSNPower
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

        private partial class PanelTecTMSNPower : TableLayoutPanel
        {
            Label[] m_arLabel;
            Dictionary<int, Label> m_dictLabelVal;

            public StatisticCommon.TEC m_tec;

            private List<TG> m_listSensorId2TG;

            private object lockValue;

            private bool m_bIsActive,
                        m_bIsStarted,
                        m_bUpdate;

            private volatile string sensorsString_TM;
            double m_dblTotalPower_TM_SN;
            DateTime m_dtLastChangedAt_TM;
            DateTime m_dtLastChangedAt_TM_SN;

            private Thread m_taskThread;
            private Semaphore m_semaState;
            private volatile bool m_bThreadIsWorking;
            private volatile bool m_bIsNewState;
            private volatile List<StatesMachine> m_states;
            private ManualResetEvent m_evTimerCurrent;
            private System.Threading.Timer m_timerCurrent;

            private DelegateFunc delegateUpdateGUI;

            public PanelTecTMSNPower(StatisticCommon.TEC tec)
            {
                InitializeComponent();

                m_tec = tec;
                Initialize();
            }

            public PanelTecTMSNPower(IContainer container, StatisticCommon.TEC tec)
                : this(tec)
            {
                container.Add(this);
            }

            private void Initialize()
            {
                int i = -1;

                m_dictLabelVal = new Dictionary<int, Label>();
                m_arLabel = new Label[(int)INDEX_LABEL.VALUE_TM_SN + 1];

                this.Dock = DockStyle.Fill;
                //Свойства колонок
                this.ColumnCount = 4;
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

                //Видимая граница для отладки
                this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle;

                string cntnt = string.Empty;

                i = (int)INDEX_LABEL.NAME;
                cntnt = m_tec.name_shr;
                m_arLabel[i] = HLabel.createLabel(cntnt, PanelTMSNPower.s_arLabelStyles[i]);
                ////Предусмотрим обработчик при изменении значения
                //if (i == (int)INDEX_LABEL.VALUE_TOTAL)
                //    m_arLabel[i].TextChanged += new EventHandler(PanelTecCurPower_TextChangedValue);
                //else
                //    ;
                this.Controls.Add(m_arLabel[i], 0, i);
                this.SetColumnSpan(m_arLabel[i], this.ColumnCount);

                //Наименование ТЭЦ, Дата/время, Значение для всех ГТП/ТГ
                for (i = (int)INDEX_LABEL.DATETIME_TM; i < (int)INDEX_LABEL.VALUE_TM_SN + 1; i++)
                {
                    switch (i)
                    {
                        case (int)INDEX_LABEL.DATETIME_TM:
                        case (int)INDEX_LABEL.DATETIME_TM_SN:
                            cntnt = @"--:--:--";
                            break;
                        case (int)INDEX_LABEL.VALUE_TM:
                        case (int)INDEX_LABEL.VALUE_TM_SN:
                            cntnt = @"---";
                            break;
                        default:
                            break;
                    }
                    m_arLabel[i] = HLabel.createLabel(cntnt, PanelTMSNPower.s_arLabelStyles[i]);
                    ////Предусмотрим обработчик при изменении значения
                    //if (i == (int)INDEX_LABEL.VALUE_TOTAL)
                    //    m_arLabel[i].TextChanged += new EventHandler(PanelTecCurPower_TextChangedValue);
                    //else
                    //    ;
                    this.Controls.Add(m_arLabel[i], (i % 2) * 2, i / 2);
                    this.SetColumnSpan(m_arLabel[i], 2);
                }

                this.RowCount = COUNT_FIXED_ROWS;

                //Свойства зафиксированных строк
                for (i = 0; i < COUNT_FIXED_ROWS; i++)
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, 10));

                lockValue = new object();
                m_listSensorId2TG = new List<TG>(); //[this.RowCount - COUNT_FIXED_ROWS];
                sensorsString_TM = string.Empty;
                m_states = new List<StatesMachine>();
                delegateUpdateGUI = ShowPower;
            }

            public void Start()
            {
                if (m_bIsStarted == true)
                    return;
                else
                    ;

                m_bIsStarted = true;

                m_tec.StartDbInterfaces(CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE);

                m_bThreadIsWorking = true;

                m_taskThread = new Thread(new ParameterizedThreadStart(TecView_ThreadFunction));
                m_taskThread.Name = @"Интерфейс к данным (" + GetType().Name + "): " + m_tec.name_shr + @" - текущие значения...";
                m_taskThread.IsBackground = true;

                m_semaState = new Semaphore(1, 1);

                m_semaState.WaitOne();
                m_taskThread.Start();

                m_evTimerCurrent = new ManualResetEvent(true);
                m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, ((PanelTMSNPower)Parent).m_msecPeriodUpdate - 1, ((PanelTMSNPower)Parent).m_msecPeriodUpdate - 1);

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
                    catch (Exception excpt) { Logging.Logg().LogExceptionToFile(excpt, "catch - PanelTMSNPower.Stop () - sem.Release(1)"); }

                    joined = m_taskThread.Join(1000);
                    if (!joined)
                        m_taskThread.Abort();
                    else
                        ;
                }

                m_tec.StopDbInterfaces();

                lock (lockValue)
                {
                    ((PanelTMSNPower)Parent).m_report.errored_state = false;
                }
            }

            private void ChangeState()
            {
                m_bIsNewState = true;
                m_states.Clear();

                if ((sensorsString_TM.Equals(string.Empty) == true))
                    m_states.Add(StatesMachine.Init_TM);
                else ;

                m_states.Add(StatesMachine.Current_TM_Gen);
                m_states.Add(StatesMachine.Current_TM_SN);
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
                        ((PanelTMSNPower)Parent).m_report.errored_state =
                        ((PanelTMSNPower)Parent).m_report.actioned_state = false;
                    }
                }
            }

            private void ErrorReport(string error_string)
            {
                lock (lockValue)
                {
                    ((PanelTMSNPower)Parent).m_report.last_error = error_string;
                    ((PanelTMSNPower)Parent).m_report.last_time_error = DateTime.Now;
                    ((PanelTMSNPower)Parent).m_report.errored_state = true;
                    ((PanelTMSNPower)Parent).m_stsStrip.BeginInvoke(((PanelTMSNPower)Parent).delegateEventUpdate);
                }
            }

            private void ActionReport(string action_string)
            {
                lock (lockValue)
                {
                    ((PanelTMSNPower)Parent).m_report.last_action = action_string;
                    ((PanelTMSNPower)Parent).m_report.last_time_action = DateTime.Now;
                    ((PanelTMSNPower)Parent).m_report.actioned_state = true;
                    ((PanelTMSNPower)Parent).m_stsStrip.BeginInvoke(((PanelTMSNPower)Parent).delegateEventUpdate);
                }
            }

            private void ShowPower()
            {
                ShowTMPower();
                ShowTMSNPower();
            }

            private void ShowTMPower()
            {
                double dblTotalPower_TM = 0.0
                        , dblTECComponentPower_TM = 0.0;
                foreach (TECComponent g in m_tec.list_TECComponents)
                {
                    if ((g.m_id > 100) && (g.m_id < 500))
                    {
                        dblTECComponentPower_TM = 0.0;

                        foreach (TG tg in g.m_listTG)
                        {
                            if (tg.id_tm > 0)
                            {
                                dblTECComponentPower_TM += setTextToLabelVal(null, tg.power_TM);
                            }
                            else
                                m_dictLabelVal[tg.m_id].Text = @"---";
                        }

                        dblTotalPower_TM += setTextToLabelVal(null, dblTECComponentPower_TM);
                    }
                    else
                        ;
                }

                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM], m_dblTotalPower_TM_SN);
                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM], dblTotalPower_TM);
                try { m_dtLastChangedAt_TM = HAdmin.ToCurrentTimeZone(m_dtLastChangedAt_TM); }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, @"PanelTecCurPower::ShowTMPower () - HAdmin.ToCurrentTimeZone () - ...");
                }
                m_arLabel[(int)INDEX_LABEL.DATETIME_TM].Text = m_dtLastChangedAt_TM.ToString(@"HH:mm:ss");
            }

            private void ShowTMSNPower()
            {
                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM_SN], m_dblTotalPower_TM_SN);
                //try { m_dtLastChangedAt = HAdmin.ToCurrentTimeZone (m_dtLastChangedAt); }
                //catch (Exception e) { Logging.Logg ().LogExceptionToFile (e, @"PanelTMSNPower::ShowTMSNPower () - ..."); }
                m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN].Text = m_dtLastChangedAt_TM_SN.ToString(@"HH:mm:ss");
            }

            private double setTextToLabelVal(Label lblVal, double val)
            {
                if (val > 1)
                {
                    if (! (lblVal == null)) lblVal.Text = val.ToString(@"F2"); else ;
                    return val;
                }
                else
                    if (!(lblVal == null)) lblVal.Text = 0.ToString(@"F0"); else ;

                return 0;
            }

            private void PanelTecCurPower_TextChangedValue(object sender, EventArgs ev)
            {
                double val = -1.0;
                int ext = 2;
                Color clr;
                if (double.TryParse(((Label)sender).Text, out val) == true)
                {
                    if (val > 1)
                        clr = Color.LimeGreen;
                    else
                    {
                        clr = Color.Green;
                        ext = 0;
                    }

                    ((Label)sender).Text = val.ToString(@"F" + ext.ToString());
                }
                else
                    clr = Color.Green;

                ((Label)sender).ForeColor = clr;
            }

            private void GetCurrentTMRequest()
            {
                m_tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, m_tec.currentTMRequest(sensorsString_TM));
            }

            private bool GetCurrentTMResponse(DataTable table)
            {
                bool bRes = true;
                int i = -1,
                    id = -1;
                double value = -1;
                DateTime dtLastChangedAt =
                    m_dtLastChangedAt_TM = DateTime.Now;
                TG tgTmp;

                foreach (TECComponent g in m_tec.list_TECComponents)
                {
                    foreach (TG t in g.m_listTG)
                    {
                        t.power_TM = 0;
                    }
                }

                for (i = 0; i < table.Rows.Count; i++)
                {
                    if (int.TryParse(table.Rows[i]["ID"].ToString(), out id) == false)
                        return false;
                    else
                        ;

                    tgTmp = m_tec.FindTGById(id, TG.INDEX_VALUE.TM, (TG.ID_TIME)(-1));

                    if (tgTmp == null)
                        return false;
                    else
                        ;

                    if (!(table.Rows[i]["value"] is DBNull))
                        if (double.TryParse(table.Rows[i]["value"].ToString(), out value) == false)
                            return false;
                        else
                            ;
                    else
                        value = 0.0;

                    if ((!(value < 1)) && (DateTime.TryParse(table.Rows[i]["last_changed_at"].ToString(), out dtLastChangedAt) == false))
                        return false;
                    else
                        ;

                    if (m_dtLastChangedAt_TM > dtLastChangedAt)
                        m_dtLastChangedAt_TM = dtLastChangedAt;
                    else
                        ;

                    switch (m_tec.type())
                    {
                        case StatisticCommon.TEC.TEC_TYPE.COMMON:
                            break;
                        case StatisticCommon.TEC.TEC_TYPE.BIYSK:
                            //value *= 20;
                            break;
                        default:
                            break;
                    }

                    tgTmp.power_TM = value;
                }

                return bRes;
            }

            private void GetCurrentTMSNRequest()
            {
                m_tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, m_tec.currentTMSNRequest());
            }

            private bool GetCurrentTMSNResponse(DataTable table)
            {
                bool bRes = true;
                int id = -1;

                m_dtLastChangedAt_TM_SN = DateTime.Now;

                if (table.Rows.Count == 1)
                {
                    if (int.TryParse(table.Rows[0]["ID_TEC"].ToString(), out id) == false)
                        return false;
                    else
                        ;

                    if (!(table.Rows[0]["SUM_P_SN"] is DBNull))
                        if (double.TryParse(table.Rows[0]["SUM_P_SN"].ToString(), out m_dblTotalPower_TM_SN) == false)
                            return false;
                        else
                            ;
                    else
                        m_dblTotalPower_TM_SN = 0.0;

                    if ((!(m_dblTotalPower_TM_SN < 1)) && (DateTime.TryParse(table.Rows[0]["LAST_UPDATE"].ToString(), out m_dtLastChangedAt_TM_SN) == false))
                        return false;
                    else
                        ;
                }
                else
                {
                    bRes = false;
                }

                return bRes;
            }

            private bool GetSensorsTEC()
            {
                bool bRes = true;

                int j = -1;
                for (j = 0; j < m_tec.list_TECComponents.Count; j++)
                    if (m_tec.list_TECComponents[j].m_id > 1000) m_listSensorId2TG.Add(m_tec.list_TECComponents[j].m_listTG[0]); else ;

                sensorsString_TM = string.Empty;

                for (int i = 0; i < m_listSensorId2TG.Count; i++)
                {
                    if (!(m_listSensorId2TG[i] == null))
                    {
                        if (sensorsString_TM.Equals(string.Empty) == false)
                            switch (m_tec.m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_ASKUE])
                            {
                                case StatisticCommon.TEC.INDEX_TYPE_SOURCE_DATA.COMMON:
                                    //Общий источник для всех ТЭЦ
                                    sensorsString_TM += @", "; //@" OR ";
                                    break;
                                case StatisticCommon.TEC.INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
                                    //Источник для каждой ТЭЦ свой
                                    sensorsString_TM += @" OR ";
                                    break;
                                default:
                                    break;
                            }
                        else
                            ;

                        switch (m_tec.m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_ASKUE])
                        {
                            case StatisticCommon.TEC.INDEX_TYPE_SOURCE_DATA.COMMON:
                                //Общий источник для всех ТЭЦ
                                sensorsString_TM += m_listSensorId2TG[i].id_tm.ToString();
                                break;
                            case StatisticCommon.TEC.INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
                                //Источник для каждой ТЭЦ свой
                                sensorsString_TM += @"[dbo].[NAME_TABLE].[ID] = " + m_listSensorId2TG[i].id_tm.ToString();
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        //ErrorReportSensors(ref table);

                        return false;
                    }
                }

                return bRes;
            }

            //private void ErrorReportSensors(ref DataTable src)
            //{
            //    string error = "Ошибка определения идентификаторов датчиков в строке ";
            //    for (int j = 0; j < src.Rows.Count; j++)
            //        error += src.Rows[j][0].ToString() + " = " + src.Rows[j][1].ToString() + ", ";

            //    error = error.Substring(0, error.LastIndexOf(","));
            //    ErrorReport(error);
            //}

            private void StateRequest(StatesMachine state)
            {
                switch (state)
                {
                    case StatesMachine.Init_TM:
                        ActionReport("Получение идентификаторов датчиков (" + m_tec.name_shr + @")");
                        break;
                    case StatesMachine.Current_TM_Gen:
                        ActionReport("Получение текущих значений (" + m_tec.name_shr + @"- генерация).");
                        GetCurrentTMRequest();
                        break;
                    case StatesMachine.Current_TM_SN:
                        ActionReport("Получение текущих значений (" + m_tec.name_shr + @"- собственные нужды).");
                        GetCurrentTMSNRequest();
                        break;
                    default:
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
                        return true;
                        break;
                    case StatesMachine.Current_TM_Gen:
                    case StatesMachine.Current_TM_SN:
                        return m_tec.Response(CONN_SETT_TYPE.DATA_SOTIASSO, out error, out table);
                    default:
                        break;
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
                            case StatisticCommon.TEC.TEC_TYPE.COMMON:
                            case StatisticCommon.TEC.TEC_TYPE.BIYSK:
                                result = GetSensorsTEC();
                                break;
                            //case TEC.TEC_TYPE.BIYSK:
                            //result = true;
                            //break;
                        }
                        if (result == true)
                        {
                        }
                        else
                            ;
                        break;
                    case StatesMachine.Current_TM_Gen:
                        result = GetCurrentTMResponse(table);
                        break;
                    case StatesMachine.Current_TM_SN:
                        result = GetCurrentTMSNResponse(table);
                        if (result == true)
                        {
                            this.BeginInvoke(delegateUpdateGUI);
                        }
                        else
                            ;
                        break;
                }

                if (result == true)
                    lock (lockValue)
                    {
                        ((PanelTMSNPower)Parent).m_report.errored_state =
                        ((PanelTMSNPower)Parent).m_report.actioned_state = false;
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
                        reason = @"идентификаторов датчиков (" + m_tec.name_shr + @"- телемеханика)";
                        waiting = @"Переход в ожидание";
                        break;
                    case StatesMachine.Current_TM_SN:
                        reason = @"текущих значений (" + m_tec.name_shr + @"- собственные нужды)";
                        waiting = @"Ожидание " + (((PanelTMSNPower)Parent).m_msecPeriodUpdate / 1000).ToString() + " секунд";
                        break;
                    case StatesMachine.Current_TM_Gen:
                        reason = @"текущих значений (" + m_tec.name_shr + @"- генерация)";
                        waiting = @"Ожидание " + (((PanelTMSNPower)Parent).m_msecPeriodUpdate / 1000).ToString() + " секунд";
                        break;
                    default:
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
        }
    }
}
