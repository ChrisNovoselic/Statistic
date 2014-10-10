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

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    partial class PanelCurPower
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

    public partial class PanelCurPower : PanelStatisticView
    {
        enum INDEX_LABEL : int { NAME,
                                DATETIME_TM,
                                DATETIME_TM_SN,
                                VALUE_TM,
                                VALUE_TM_SN,
                                NAME_COMPONENT,
                                VALUE_COMPONENT,
                                NAME_TG,
                                VALUE_TG,
                                COUNT_INDEX_LABEL
        };
        const int COUNT_FIXED_ROWS = (int)INDEX_LABEL.VALUE_TM_SN - 1;

        static Color s_clrBackColorLabel = Color.FromArgb(212, 208, 200), s_clrBackColorLabelVal_TM = Color.FromArgb(219, 223, 227), s_clrBackColorLabelVal_TM_SN = Color.FromArgb(219, 223, 247);
        static HLabelStyles[] s_arLabelStyles = { new HLabelStyles(Color.Black, s_clrBackColorLabel, 22F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM, 18F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM_SN, 18F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM, 18F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM_SN, 18F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabel, 14F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM, 14F, ContentAlignment.MiddleRight),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabel, 14F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM, 14F, ContentAlignment.MiddleRight)};
        
        //enum StatesMachine : int {Init_TM, Current_TM_Gen, Current_TM_SN};

        public int m_msecPeriodUpdate;

        //HReports m_report;
        
        public bool m_bIsActive;

        public PanelCurPower(List<StatisticCommon.TEC> listTec, DelegateFunc fErrRep, DelegateFunc fActRep)
        {
            InitializeComponent();

            m_msecPeriodUpdate = Int32.Parse (FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME]) * 1000;

            this.Dock = DockStyle.Fill;

            //this.Location = new System.Drawing.Point(40, 58);
            //this.Name = "pnlView";
            //this.Size = new System.Drawing.Size(705, 747);

            this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle;

            PanelTecCurPower ptcp;

            int i = -1;

            this.ColumnCount = listTec.Count / 2;
            if (this.ColumnCount == 0) this.ColumnCount ++ ; else ;
            this.RowCount = listTec.Count / this.ColumnCount;

            for (i = 0; i < listTec.Count; i++)
            {
                ptcp = new PanelTecCurPower(listTec[i], fErrRep, fActRep);
                this.Controls.Add(ptcp, i % this.ColumnCount, i / this.ColumnCount);
            }

            for (i = 0; i < this.ColumnCount; i++)
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / this.ColumnCount));

            for (i = 0; i < this.RowCount; i++)
                this.RowStyles.Add(new RowStyle(SizeType.Percent, 100 / this.RowCount));
        }

        public PanelCurPower(IContainer container, List<StatisticCommon.TEC> listTec, DelegateFunc fErrRep, DelegateFunc fActRep)
            : this(listTec, fErrRep, fActRep)
        {
            container.Add(this);
        }

        public override void Start () {
            int i = 0;
            foreach (Control ctrl in this.Controls) {
                if (ctrl is PanelTecCurPower) {
                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecCurPower)ctrl).m_tecView.m_tec.m_id))
                        ((PanelTecCurPower)ctrl).Start();
                    //else ;
                    i++;
                }
                else
                    ;
            }
        }

        public override void Stop () {
            int i = 0;
            foreach (Control ctrl in this.Controls) {
                if (ctrl is PanelTecCurPower) {
                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecCurPower)ctrl).m_tecView.m_tec.m_id))
                        ((PanelTecCurPower)ctrl).Stop();
                    //else ;
                    i ++;
                }
                else
                    ;
            }
        }

        public override void Activate (bool active) {
            if (m_bIsActive == active)
                return;
            else
                ;

            m_bIsActive = active;

            //TypeConverter conv;
            //dynamic dynObj = null;
            Type typeChildren; //PanelTecCurPower
            typeChildren = typeof (PanelTecCurPower);

            int i = 0;
            foreach (Control ctrl in this.Controls) {
                if (ctrl.GetType ().Equals (typeChildren) == true)
                {
                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecCurPower)ctrl).m_tecView.m_tec.m_id))
                        ((PanelTecCurPower)ctrl).Activate(active);
                    //else ;
                    i ++;
                }
                else
                    ;
            }
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
            Label[] m_arLabel;
            Dictionary<int, Label> m_dictLabelVal;

            public TecView m_tecView;

            private object m_lockRep;
            private ManualResetEvent m_evTimerCurrent;
            private System.Threading.Timer m_timerCurrent;

            //private DelegateFunc delegateUpdateGUI;

            public PanelTecCurPower(StatisticCommon.TEC tec, DelegateFunc fErrRep, DelegateFunc fActRep)
            {
                InitializeComponent();

                m_tecView = new TecView (null, TecView.TYPE_PANEL.CUR_POWER, -1, -1);

                HMark markQueries = new HMark ();
                markQueries.Marked ((int)CONN_SETT_TYPE.DATA_SOTIASSO);

                m_tecView.InitTEC (new List <StatisticCommon.TEC> () { tec }, markQueries);
                m_tecView.SetDelegateReport(fErrRep, fActRep);

                m_tecView.updateGUI_TM_Gen = new DelegateFunc (showTMGenPower);
                m_tecView.updateGUI_TM_SN = new DelegateFunc(showTMSNPower);

                Initialize();
            }

            public PanelTecCurPower(IContainer container, StatisticCommon.TEC tec, DelegateFunc fErrRep, DelegateFunc fActRep)
                : this(tec, fErrRep, fActRep)
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
                cntnt = m_tecView.m_tec.name_shr;
                m_arLabel[i] = HLabel.createLabel(cntnt, PanelCurPower.s_arLabelStyles[i]);
                ////Предусмотрим обработчик при изменении значения
                //if (i == (int)INDEX_LABEL.VALUE_TOTAL)
                //    m_arLabel[i].TextChanged += new EventHandler(PanelTecCurPower_TextChangedValue);
                //else
                //    ;
                this.Controls.Add(m_arLabel[i], 0, i);
                this.SetColumnSpan(m_arLabel[i], this.ColumnCount);

                //Наименование ТЭЦ, Дата/время, Значение для всех ГТП/ТГ
                for (i = (int)INDEX_LABEL.DATETIME_TM; i < (int)INDEX_LABEL.NAME_COMPONENT; i++)
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
                    m_arLabel[i] = HLabel.createLabel(cntnt, PanelCurPower.s_arLabelStyles[i]);
                    ////Предусмотрим обработчик при изменении значения
                    //if (i == (int)INDEX_LABEL.VALUE_TOTAL)
                    //    m_arLabel[i].TextChanged += new EventHandler(PanelTecCurPower_TextChangedValue);
                    //else
                    //    ;
                    this.Controls.Add(m_arLabel[i], (i % 2) * 2, i / 2);
                    this.SetColumnSpan(m_arLabel[i], 2);
                }

                this.RowCount = COUNT_FIXED_ROWS;

                //m_list_TECComponents = new List <TECComponentBase> ();
                foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
                {
                    if ((g.m_id > 100) && (g.m_id < 500))
                    {
                        //Добавить ГТП в список компонентов
                        //m_list_TECComponents.Add(g);

                        //Добавить наименование ГТП
                        Label lblTECComponent = HLabel.createLabel(g.name_shr, PanelCurPower.s_arLabelStyles[(int)INDEX_LABEL.NAME_COMPONENT]);
                        this.Controls.Add(lblTECComponent, 0, this.RowCount);
                        m_dictLabelVal.Add(g.m_id, HLabel.createLabel(@"---", PanelCurPower.s_arLabelStyles[(int)INDEX_LABEL.VALUE_TG]));
                        this.Controls.Add(m_dictLabelVal[g.m_id], 1, this.RowCount);
                        //m_dictLabelVal[g.m_id].TextChanged += new EventHandler(PanelTecCurPower_TextChangedValue);

                        foreach (TG tg in g.m_listTG)
                        {
                            //Добавить наименование ТГ
                            this.Controls.Add(HLabel.createLabel(tg.name_shr, PanelCurPower.s_arLabelStyles[(int)INDEX_LABEL.NAME_TG]), 2, this.RowCount);
                            //Добавить значение ТГ
                            m_dictLabelVal.Add(tg.m_id, HLabel.createLabel(@"---", PanelCurPower.s_arLabelStyles[(int)INDEX_LABEL.VALUE_TG]));
                            this.Controls.Add(m_dictLabelVal[tg.m_id], 3, this.RowCount);
                            //m_dictLabelVal[tg.m_id].TextChanged += new EventHandler(PanelTecCurPower_TextChangedValue);

                            this.RowCount++;
                        }

                        this.SetRowSpan(lblTECComponent, g.m_listTG.Count);
                        this.SetRowSpan(m_dictLabelVal[g.m_id], g.m_listTG.Count);
                    }
                    else
                        ;
                }

                //Свойства зафиксированных строк
                for (i = 0; i < COUNT_FIXED_ROWS; i++)
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, 10));

                //Свойства НЕзафиксированных строк
                //this.RowCount = m_dictLabelVal.Count + COUNT_FIXED_ROWS;
                for (i = 0; i < this.RowCount - COUNT_FIXED_ROWS; i++)
                {
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, (float)Math.Round((double)(100 - (10 * COUNT_FIXED_ROWS)) / (this.RowCount - COUNT_FIXED_ROWS), 1)));
                }

                m_lockRep = new object();
            }

            public void Start()
            {
                if (! (m_tecView.threadIsWorking < 0))
                    return;
                else
                    ;
                
                m_tecView.Start ();

                m_evTimerCurrent = new ManualResetEvent(true);
                m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, ((PanelCurPower)Parent).m_msecPeriodUpdate - 1, ((PanelCurPower)Parent).m_msecPeriodUpdate - 1);
            }

            public void Stop()
            {
                if (m_tecView.threadIsWorking < 0)
                    return;
                else
                    ;

                m_tecView.Stop();

                m_evTimerCurrent.Reset();
                m_timerCurrent.Dispose();

                lock (m_lockRep)
                {
                    FormMainBaseWithStatusStrip.m_report.ClearStates ();
                }
            }

            private void ChangeState()
            {
                m_tecView.ChangeState ();
            }

            public void Activate(bool active)
            {
                m_tecView.Activate(active);

                if (m_tecView.m_bIsActive == true)
                {
                    m_tecView.ChangeState();
                }
                else
                {
                    FormMainBaseWithStatusStrip.m_report.ClearStates ();
                }
            }

            private void showTMGenPower()
            {
                this.BeginInvoke(new DelegateFunc(ShowTMGenPower));
            }

            private void showTMSNPower()
            {
                this.BeginInvoke(new DelegateFunc(ShowTMSNPower));
            }

            private void ShowTMGenPower () {
                double dblTotalPower_TM = 0.0
                        , dblTECComponentPower_TM = 0.0;
                foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
                {
                    if ((g.m_id > 100) && (g.m_id < 500))
                    {
                        dblTECComponentPower_TM = 0.0;

                        foreach (TG tg in g.m_listTG)
                        {
                            if (tg.id_tm > 0) {
                                dblTECComponentPower_TM += setTextToLabelVal(m_dictLabelVal[tg.m_id], tg.power_TM);
                            }
                            else
                                m_dictLabelVal[tg.m_id].Text = @"---";
                        }

                        dblTotalPower_TM += setTextToLabelVal(m_dictLabelVal[g.m_id], dblTECComponentPower_TM);
                    }
                    else
                        ;
                }

                //???
                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM], m_tecView.m_dblTotalPower_TM_SN);
                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM], dblTotalPower_TM);

                m_arLabel[(int)INDEX_LABEL.DATETIME_TM].Text = m_tecView.m_dtLastChangedAt_TM_Gen.ToString(@"HH:mm:ss");
            }

            private void ShowTMSNPower()
            {
                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM_SN], m_tecView.m_dblTotalPower_TM_SN);
                //try { m_dtLastChangedAt = HAdmin.ToCurrentTimeZone (m_dtLastChangedAt); }
                //catch (Exception e) { Logging.Logg ().Exception (e, @"PanelTMSNPower::ShowTMSNPower () - ..."); }
                m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN].Text = m_tecView.m_dtLastChangedAt_TM_SN.ToString(@"HH:mm:ss");
            }

            private double setTextToLabelVal (Label lblVal, double val) {
                if (val > 1)
                {
                    lblVal.Text = val.ToString(@"F2");
                    return val;
                }
                else
                    lblVal.Text = 0.ToString(@"F0");

                return 0;
            }

            private void PanelTecCurPower_TextChangedValue (object sender, EventArgs ev) {
                double val = -1.0;
                int ext = 2;
                Color clr;
                if (double.TryParse(((Label)sender).Text, out val) == true) {
                    if (val > 1)
                        clr = Color.LimeGreen;
                    else {
                        clr = Color.Green;
                        ext = 0;
                    }

                    ((Label)sender).Text = val.ToString (@"F" + ext.ToString ());
                }
                else
                    clr = Color.Green;

                ((Label)sender).ForeColor = clr;
            }

            //private void GetCurrentTMRequest()
            //{
            //    m_tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, m_tec.currentTMRequest(sensorsString_TM));
            //}

            //private bool GetCurrentTMResponse(DataTable table)
            //{
            //    bool bRes = true;
            //    int i = -1,
            //        id = -1;
            //    double value = -1;
            //    DateTime dtLastChangedAt =
            //        m_dtLastChangedAt_TM = DateTime.Now;
            //    TG tgTmp;

            //    foreach (TECComponent g in m_tec.list_TECComponents)
            //    {
            //        foreach (TG t in g.m_listTG)
            //        {
            //            t.power_TM = 0;
            //        }
            //    }

            //    for (i = 0; i < table.Rows.Count; i++)
            //    {
            //        if (int.TryParse(table.Rows[i]["ID"].ToString(), out id) == false)
            //            return false;
            //        else
            //            ;

            //        tgTmp = m_tec.FindTGById(id, TG.INDEX_VALUE.TM, (TG.ID_TIME)(-1));

            //        if (tgTmp == null)
            //            return false;
            //        else
            //            ;

            //        if (! (table.Rows[i]["value"] is DBNull))
            //            if (double.TryParse(table.Rows[i]["value"].ToString(), out value) == false)
            //                return false;
            //            else
            //                ;
            //        else
            //            value = 0.0;

            //        if ((! (value < 1)) && (DateTime.TryParse(table.Rows[i]["last_changed_at"].ToString(), out dtLastChangedAt) == false))
            //            return false;
            //        else
            //            ;

            //        if (m_dtLastChangedAt_TM > dtLastChangedAt)
            //            m_dtLastChangedAt_TM = dtLastChangedAt;
            //        else
            //            ;

            //        switch (m_tec.type())
            //        {
            //            case TEC.TEC_TYPE.COMMON:
            //                break;
            //            case TEC.TEC_TYPE.BIYSK:
            //                //value *= 20;
            //                break;
            //            default:
            //                break;
            //        }

            //        tgTmp.power_TM = value;
            //    }

            //    return bRes;
            //}

            //private void GetCurrentTMSNRequest()
            //{
            //    m_tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, m_tec.currentTMSNRequest());
            //}

            //private bool GetCurrentTMSNResponse(DataTable table)
            //{
            //    bool bRes = true;
            //    int id = -1;

            //    m_dtLastChangedAt_TM_SN = DateTime.Now;

            //    if (table.Rows.Count == 1)
            //    {
            //        if (int.TryParse(table.Rows[0]["ID_TEC"].ToString(), out id) == false)
            //            return false;
            //        else
            //            ;

            //        if (!(table.Rows[0]["SUM_P_SN"] is DBNull))
            //            if (double.TryParse(table.Rows[0]["SUM_P_SN"].ToString(), out m_dblTotalPower_TM_SN) == false)
            //                return false;
            //            else
            //                ;
            //        else
            //            m_dblTotalPower_TM_SN = 0.0;

            //        if ((!(m_dblTotalPower_TM_SN < 1)) && (DateTime.TryParse(table.Rows[0]["LAST_UPDATE"].ToString(), out m_dtLastChangedAt_TM_SN) == false))
            //            return false;
            //        else
            //            ;
            //    }
            //    else
            //    {
            //        bRes = false;
            //    }

            //    return bRes;
            //}

            //private bool GetSensorsTEC()
            //{
            //    bool bRes = true;

            //    int j = -1;
            //    for (j = 0; j < m_tec.list_TECComponents.Count; j++)
            //        if (m_tec.list_TECComponents [j].m_id > 1000) m_listSensorId2TG.Add(m_tec.list_TECComponents[j].m_listTG [0]); else ;                

            //    sensorsString_TM = string.Empty;

            //    for (int i = 0; i < m_listSensorId2TG.Count; i++)
            //    {
            //        if (!(m_listSensorId2TG[i] == null))
            //        {
            //            if (sensorsString_TM.Equals(string.Empty) == false)
            //                switch (m_tec.m_arTypeSourceData [(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_ASKUE]) {
            //                    case TEC.INDEX_TYPE_SOURCE_DATA.COMMON:
            //                        //Общий источник для всех ТЭЦ
            //                        sensorsString_TM += @", "; //@" OR ";
            //                        break;
            //                    case TEC.INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
            //                        //Источник для каждой ТЭЦ свой
            //                        sensorsString_TM += @" OR ";
            //                        break;
            //                    default:
            //                        break;
            //                }
            //            else
            //                ;

            //            switch (m_tec.m_arTypeSourceData[(int)CONN_SETT_TYPE.DATA_SOTIASSO - (int)CONN_SETT_TYPE.DATA_ASKUE])
            //            {
            //                case TEC.INDEX_TYPE_SOURCE_DATA.COMMON:
            //                    //Общий источник для всех ТЭЦ
            //                    sensorsString_TM += m_listSensorId2TG[i].id_tm.ToString();
            //                    break;
            //                case TEC.INDEX_TYPE_SOURCE_DATA.INDIVIDUAL:
            //                    //Источник для каждой ТЭЦ свой
            //                    sensorsString_TM += @"[dbo].[NAME_TABLE].[ID] = " + m_listSensorId2TG[i].id_tm.ToString();
            //                    break;
            //                default:
            //                    break;
            //            }
            //        }
            //        else
            //        {
            //            //ErrorReportSensors(ref table);

            //            return false;
            //        }
            //    }

            //    return bRes;
            //}

            //private void ErrorReportSensors(ref DataTable src)
            //{
            //    string error = "Ошибка определения идентификаторов датчиков в строке ";
            //    for (int j = 0; j < src.Rows.Count; j++)
            //        error += src.Rows[j][0].ToString() + " = " + src.Rows[j][1].ToString() + ", ";

            //    error = error.Substring(0, error.LastIndexOf(","));
            //    ErrorReport(error);
            //}

            //private void StateRequest(StatesMachine state)
            //{
            //    switch (state)
            //    {
            //        case StatesMachine.Init_TM:
            //            ActionReport("Получение идентификаторов датчиков (" + m_tec.name_shr + @")");
            //            break;
            //        case StatesMachine.Current_TM_Gen:
            //            ActionReport("Получение текущих значений (" + m_tec.name_shr + @"- генерация).");
            //            GetCurrentTMRequest();
            //            break;
            //        case StatesMachine.Current_TM_SN:
            //            ActionReport("Получение текущих значений (" + m_tec.name_shr + @"- собственные нужды).");
            //            GetCurrentTMSNRequest();
            //            break;
            //        default:
            //            break;
            //    }
            //}

            //private bool StateCheckResponse(StatesMachine state, out bool error, out DataTable table)
            //{
            //    error = false;
            //    table = null;

            //    switch (state)
            //    {
            //        case StatesMachine.Init_TM:
            //            return true;
            //            break;
            //        case StatesMachine.Current_TM_Gen:
            //        case StatesMachine.Current_TM_SN:
            //            return m_tec.Response(CONN_SETT_TYPE.DATA_SOTIASSO, out error, out table);
            //        default:
            //            break;
            //    }

            //    error = true;

            //    return false;
            //}

            //private bool StateResponse(StatesMachine state, DataTable table)
            //{
            //    bool result = false;
            //    switch (state)
            //    {
            //        case StatesMachine.Init_TM:
            //            switch (m_tec.type())
            //            {
            //                case TEC.TEC_TYPE.COMMON:
            //                case TEC.TEC_TYPE.BIYSK:
            //                    result = GetSensorsTEC();
            //                    break;
            //                //case TEC.TEC_TYPE.BIYSK:
            //                    //result = true;
            //                    //break;
            //            }
            //            if (result == true)
            //            {
            //            }
            //            else
            //                ;
            //            break;
            //        case StatesMachine.Current_TM_Gen:
            //            result = GetCurrentTMResponse(table);
            //            break;
            //        case StatesMachine.Current_TM_SN:
            //            result = GetCurrentTMSNResponse(table);
            //            if (result == true)
            //            {
            //                this.BeginInvoke(delegateUpdateGUI);
            //            }
            //            else
            //                ;
            //            break;
            //    }

            //    if (result == true)
            //        lock (lockValue)
            //        {
            //            ((PanelCurPower)Parent).m_report.errored_state =
            //            ((PanelCurPower)Parent).m_report.actioned_state = false;
            //        }
            //    else
            //        ;

            //    return result;
            //}

            //private void StateErrors(StatesMachine state, bool response)
            //{
            //    string  error = string.Empty,
            //            reason = string.Empty,
            //            waiting = string.Empty;

            //    switch (state)
            //    {
            //        case StatesMachine.Init_TM:
            //            reason = @"идентификаторов датчиков (" + m_tec.name_shr + @"- телемеханика)";
            //            waiting = @"Переход в ожидание";
            //            break;
            //        case StatesMachine.Current_TM_SN:
            //            reason = @"текущих значений (" + m_tec.name_shr + @"- собственные нужды)";
            //            waiting = @"Ожидание " + (((PanelCurPower)Parent).m_msecPeriodUpdate / 1000).ToString() + " секунд";
            //            break;
            //        case StatesMachine.Current_TM_Gen:
            //            reason = @"текущих значений (" + m_tec.name_shr + @"- генерация)";
            //            waiting = @"Ожидание " + (((PanelCurPower)Parent).m_msecPeriodUpdate / 1000).ToString() + " секунд";
            //            break;
            //        default:
            //            break;
            //    }

            //    if (response)
            //        reason = @"разбора " + reason;
            //    else
            //        reason = @"получения " + reason;

            //    error = "Ошибка " + reason + ".";

            //    if (waiting.Equals(string.Empty) == true)
            //        error += " " + waiting + ".";
            //    else
            //        ;

            //    ErrorReport(error);
            //}

            private void TimerCurrent_Tick(Object stateInfo)
            {
                if (m_tecView.m_bIsActive == true)
                    m_tecView.ChangeState ();
                else
                    ;
            }
        }
    }
}
