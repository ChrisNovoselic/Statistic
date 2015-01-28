﻿using System;
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

        protected override void initTableHourRows () {
            //Ничего не делаем, т.к. нет таблиц с часовыми значениями
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

                m_tecView = new TecView(TecView.TYPE_PANEL.CUR_POWER, -1, -1
                                , Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME])
                                , Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ERROR_DELAY]));

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

            public void InitTableHourRows (DateTime dt) {
                m_tecView.m_curDate = dt;
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
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new DelegateFunc(ShowTMGenPower));
                else
                    Logging.Logg().Error(@"PanelTecCurPower::showTMGenPower () - ... BeginInvoke (ShowTMGenPower) - ...");
            }

            private void showTMSNPower()
            {
                if (InvokeRequired)
                    this.BeginInvoke(new DelegateFunc(ShowTMSNPower));
                else
                    Logging.Logg().Error(@"PanelTecCurPower::showTMSNPower () - ... BeginInvoke (ShowTMSNPower) - ...");
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
                                dblTECComponentPower_TM += setTextToLabelVal(m_dictLabelVal[tg.m_id], m_tecView.m_dictValuesTG[tg.m_id].m_powerCurrent_TM);
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

                switch (m_tecView.m_dtLastChangedAt_TM_Gen.Kind) {
                }

                //m_tecView.m_dtLastChangedAt_TM_Gen = HAdmin.ToMoscowTimeZone(m_tecView.m_dtLastChangedAt_TM_Gen);

                if ((m_tecView.serverTime - m_tecView.m_dtLastChangedAt_TM_Gen).TotalMinutes < 3)
                {
                    m_arLabel[(int)INDEX_LABEL.DATETIME_TM].Text = m_tecView.m_dtLastChangedAt_TM_Gen.ToString(@"HH:mm:ss");
                    m_arLabel[(int)INDEX_LABEL.DATETIME_TM].ForeColor = Color.Black;
                }
                else
                {
                    m_arLabel[(int)INDEX_LABEL.DATETIME_TM].Text = m_tecView.m_dtLastChangedAt_TM_Gen.ToString(@"dd.MM.yyyy HH:mm:ss");
                    m_arLabel[(int)INDEX_LABEL.DATETIME_TM].ForeColor = Color.Red;
                }
            }

            private void ShowTMSNPower()
            {
                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM_SN], m_tecView.m_dblTotalPower_TM_SN);

                if ((m_tecView.serverTime - m_tecView.m_dtLastChangedAt_TM_SN).TotalMinutes < 3)
                {
                    m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN].Text = m_tecView.m_dtLastChangedAt_TM_SN.ToString(@"HH:mm:ss");
                    m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN].ForeColor = Color.Black;
                }
                else
                {
                    m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN].Text = m_tecView.m_dtLastChangedAt_TM_SN.ToString(@"dd.MM.yyyy HH:mm:ss");
                    m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN].ForeColor = Color.Red;
                }
            }

            /// <summary>
            /// Отобразить значение аналог 'PanelQuickData::showTMValue'
            /// </summary>
            /// <param name="lblVal">элемент управления для отображения значения</param>
            /// <param name="val">значение для отображения</param>
            /// <returns>значение с ограничением в 1 МВт</returns>
            private double setTextToLabelVal (Label lblVal, double val) {
                if (val > 1)
                {
                    lblVal.Text = val.ToString(@"F2");
                    return val;
                }
                else
                    if (! (val < 0))
                        lblVal.Text = 0.ToString(@"F0");
                    else
                        lblVal.Text = @"---";

                return 0;
            }

            //private void PanelTecCurPower_TextChangedValue (object sender, EventArgs ev) {
            //    double val = -1.0;
            //    int ext = 2;
            //    Color clr;
            //    if (double.TryParse(((Label)sender).Text, out val) == true) {
            //        if (val > 1)
            //            clr = Color.LimeGreen;
            //        else {
            //            clr = Color.Green;
            //            ext = 0;
            //        }

            //        ((Label)sender).Text = val.ToString (@"F" + ext.ToString ());
            //    }
            //    else
            //        clr = Color.Green;

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