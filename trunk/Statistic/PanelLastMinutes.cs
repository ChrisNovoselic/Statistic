﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Data;

using HClassLibrary;
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

            //Создание панели с дата/время
            m_panelDateTime = new PanelDateTime();
        }

        #endregion
    }

    public partial class PanelLastMinutes : PanelStatisticView
    {
        protected static AdminTS.TYPE_FIELDS s_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;
        
        private List <Label> m_listLabelDateTime;
        private PanelDateTime m_panelDateTime;

        private ManualResetEvent m_evTimerCurrent;
        private System.Threading.Timer m_timerCurrent;

        enum INDEX_LABEL : int { NAME_TEC, NAME_COMPONENT, VALUE_COMPONENT, DATETIME, COUNT_INDEX_LABEL };
        static Color s_clrBakColorLabel = Color.FromArgb(212, 208, 200), s_clrBakColorLabelVal = Color.FromArgb(219, 223, 227);
        static HLabelStyles[] s_arLabelStyles = {new HLabelStyles(Color.Black, s_clrBakColorLabel, 14F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBakColorLabel, 12F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBakColorLabelVal, 10F, ContentAlignment.MiddleRight),
                                                new HLabelStyles(Color.Black, s_clrBakColorLabel, 12F, ContentAlignment.MiddleCenter)};
        static int COUNT_FIXED_ROWS = (int)INDEX_LABEL.NAME_COMPONENT + 1;
        //static int COUNT_HOURS = 24;

        static RowStyle fRowStyle () { return new RowStyle(SizeType.Percent, (float)Math.Round((double)100 / (24 + COUNT_FIXED_ROWS), 6)); }

        //AdminTS m_admin;

        enum StatesMachine : int
        {
            Init_TM,
            LastMinutes_TM,
            PBRValues,
            AdminValues
        };

        //public DelegateFunc delegateEventUpdate;

        public int m_msecPeriodUpdate;

        public bool m_bIsActive;

        private event DelegateObjectFunc EventChangeDateTime;

        public PanelLastMinutes(List<StatisticCommon.TEC> listTec, DelegateFunc fErrRep, DelegateFunc fActRep)
        {
            int i = -1;

            m_msecPeriodUpdate =
                //30 //Для отладки
                60 * 60
                * 1000;

            InitializeComponent();

            this.Dock = DockStyle.Fill;

            this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle

            this.ColumnCount = listTec.Count + 1;
            this.RowCount = 1;

            float fPercentColDatetime = 8F;
            this.Controls.Add(m_panelDateTime, 0, 0);
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, fPercentColDatetime));

            int iCountSubColumns = 0;
            
            for (i = 0; i < listTec.Count; i++)
            {
                this.Controls.Add(new PanelTecLastMinutes(listTec[i], fErrRep, fActRep), i + 1, 0);
                EventChangeDateTime += new DelegateObjectFunc(((PanelTecLastMinutes)this.Controls [i + 1]).OnEventChangeDateTime);
                iCountSubColumns += ((PanelTecLastMinutes)this.Controls [i + 1]).CountTECComponent; //Слева столбец дата/время
            }

            //Размеры столбцов после создания столбцов, т.к.
            //кол-во "подстолбцов" в столбцах до их создания неизвестно
            for (i = 0; i < listTec.Count; i++)
            {
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, ((float)100 - fPercentColDatetime) / iCountSubColumns * ((PanelTecLastMinutes)this.Controls[i + 1]).CountTECComponent));
            }
        }

        public PanelLastMinutes(IContainer container, List<StatisticCommon.TEC> listTec, DelegateFunc fErrRep, DelegateFunc fActRep)
            : this(listTec, fErrRep, fActRep)
        {
            container.Add(this);
        }

        public override void Start()
        {
            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecLastMinutes)
                {
                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecLastMinutes)ctrl).m_id_tec))
                        ((PanelTecLastMinutes)ctrl).Start();
                    //else ;
                    i++;
                }
                else
                    ;
            }

            //Милисекунды до первого запуска функции таймера
            double msecUpdate = (new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour + 1, 6, 0) - DateTime.Now).TotalMilliseconds;

            m_evTimerCurrent = new ManualResetEvent(true);
            m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, (Int64)msecUpdate, m_msecPeriodUpdate - 1);
            //Для отладки
            //m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, 0, m_msecPeriodUpdate - 1);
        }

        public override void Stop()
        {
            int i = 0;

            if (!(m_evTimerCurrent == null)) m_evTimerCurrent.Reset(); else ;
            if (!(m_timerCurrent == null)) m_timerCurrent.Dispose(); else ;

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecLastMinutes)
                {
                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecLastMinutes)ctrl).m_id_tec))
                        ((PanelTecLastMinutes)ctrl).Stop();
                    //else ;
                    i++;
                }
                else
                    ;
            }
        }

        protected override void initTableHourRows()
        {
            //Перестраиваем "шкалу" времени
        }

        public override void Activate(bool active)
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
                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecLastMinutes)ctrl).m_id_tec))
                        ((PanelTecLastMinutes)ctrl).Activate(active);
                    //else ;
                    i++;
                }
                else
                    ;
            }

            if (m_bIsActive == true)
                EventChangeDateTime (m_panelDateTime.m_dtprDate.Value);
            else
                ;
        }

        private void setDatetimePicker(DateTime dtSet)
        {
            m_panelDateTime.m_dtprDate.Value = dtSet;

            initTableHourRows ();
        }

        private void TimerCurrent_Tick (object obj) {
            if (m_bIsActive == true)
                this.BeginInvoke (new DelegateDateFunc (setDatetimePicker), DateTime.Now);
            else
                ;
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

                this.m_dtprDate = new DateTimePicker();
                this.m_dtprDate.Dock = DockStyle.Fill;
                //this.m_dtprDate.ValueChanged += new EventHandler(((PanelLastMinutes)Parent).OnDateTimeValueChanged);
                this.m_dtprDate.ValueChanged += new EventHandler(OnDateTimeValueChanged);
            }

            #endregion
        }

        private partial class PanelDateTime : TableLayoutPanel
        {
            public DateTimePicker m_dtprDate;
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

                DateTime dtSel = DateTime.Now.Date;

                //Добавить дату
                //Label lblDate = HLabel.createLabel(dtNow.ToString (@"dd.MM.yyyy"), PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_TEC]);
                this.Controls.Add(m_dtprDate, 0, 0);
                this.SetRowSpan(m_dtprDate, COUNT_FIXED_ROWS);

                m_dictLabelTime = new Dictionary<int,Label> ();

                dtSel = dtSel.AddMinutes(59);
                for (i = 1; i < 25; i++)
                {
                    m_dictLabelTime[i - 1] = HLabel.createLabel(dtSel.ToString(@"HH:mm"), PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.DATETIME]);
                    this.Controls.Add(m_dictLabelTime[i - 1], 0, (i - 1) + COUNT_FIXED_ROWS);

                    dtSel = dtSel.AddHours(1);
                }

                for (i = 0; i < (24 + COUNT_FIXED_ROWS - 1); i++)
                {
                    this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                }

                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 6F));
            }

            private void OnDateTimeValueChanged(object obj, EventArgs ev)
            {
                DateTime dt = m_dtprDate.Value;
                ((PanelLastMinutes)Parent).EventChangeDateTime(dt);
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
            public int m_id_tec { get { return m_tecView.m_tec.m_id; } }

            List<TECComponentBase> m_list_TECComponents;
            public int CountTECComponent { get { return m_list_TECComponents.Count; } }

            private Dictionary<int, Label[]> m_dictLabelVal;
            private Dictionary<int, ToolTip[]> m_dictToolTip;

            //private Dictionary<int, TecView.valuesTECComponent> m_dictValuesHours;
            TecView m_tecView;

            public PanelTecLastMinutes(StatisticCommon.TEC tec, DelegateFunc fErrRep, DelegateFunc fActRep)
            {
                InitializeComponent();

                m_tecView = new TecView (null, TecView.TYPE_PANEL.LAST_MINUTES, -1, -1);

                HMark markQueries = new HMark();
                markQueries.Marked((int)CONN_SETT_TYPE.ADMIN);
                markQueries.Marked((int)CONN_SETT_TYPE.PBR);
                markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO);

                m_tecView.InitTEC (new List <TEC> () { tec }, markQueries);
                m_tecView.SetDelegateReport(fErrRep, fActRep);

                m_tecView.updateGUI_LastMinutes = new DelegateFunc(showLastMinutesTM);

                Initialize();
            }

            public PanelTecLastMinutes(IContainer container, StatisticCommon.TEC tec, DelegateFunc fErrRep, DelegateFunc fActRep)
                : this(tec, fErrRep, fActRep)
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
                Label lblNameTEC = HLabel.createLabel(m_tecView.m_tec.name_shr, PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_TEC]);
                this.Controls.Add(lblNameTEC, 0, 0);

                foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
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
                            m_dictToolTip[g.m_id][i].SetToolTip(m_dictLabelVal[g.m_id][i], Hd2PercentControl.StringToolTipEmpty);

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
            }

            public void Start()
            {
                if (! (m_tecView.threadIsWorking < 0))
                    return;
                else
                    ;

                m_tecView.Start ();
            }

            public void Stop()
            {
                if (m_tecView.threadIsWorking < 0)
                    return;
                else
                    ;

                m_tecView.Stop();

                FormMainBaseWithStatusStrip.m_report.ClearStates();
            }

            private void ChangeState()
            {
                //m_tecView.m_curDate = ... получено при обработке события
                //m_tecView.m_curDate = m_tecView.m_curDate.Add(-HAdmin.GetUTCOffsetOfCurrentTimeZone ());

                m_tecView.ChangeState ();
            }

            public void Activate(bool active)
            {
                m_tecView.Activate(active);

                if (m_tecView.m_bIsActive == true)
                {
                    //ChangeState();
                }
                else
                {
                    m_tecView.ClearStates ();
                }
            }

            public void OnEventChangeDateTime (object obj) {
                m_tecView.m_curDate = (DateTime)obj;

                ChangeState ();
            }

            private void showLastMinutesTM()
            {
                this.BeginInvoke(new DelegateFunc(ShowLastMinutesTM));
            }

            private void ShowLastMinutesTM()
            {
                Color clrBackColor;
                int warn = -1
                    , cntWarn = -1
                    ;
                Hd2PercentControl d2PercentControl = new Hd2PercentControl ();
                string strToolTip = string.Empty,
                        strWarn = string.Empty;
                
                foreach (TECComponent g in m_list_TECComponents)
                {
                    cntWarn = 0;
                    for (int hour = 1; hour < 25; hour++) //Если значение за 00:00 пред./сут. запис. в [24]
                    {
                        clrBackColor = s_clrBakColorLabelVal;
                        strToolTip = string.Empty;

                        bool bPmin = false;
                        if (m_tecView.m_tec.m_id == 5) bPmin = true; else ;
                        strToolTip = d2PercentControl.Calculate(m_tecView.m_dictValuesTECComponent[hour - 1][g.m_id], bPmin, out warn);

                        m_dictToolTip[g.m_id][hour - 1].SetToolTip(m_dictLabelVal[g.m_id][hour - 1], strToolTip);

                        if (m_tecView.m_dictValuesTECComponent[g.m_id][hour - 1].valuesLastMinutesTM > 1)
                        {
                            if ((! (warn == 0)) &&
                                (m_tecView.m_dictValuesTECComponent[hour - 1][g.m_id].valuesLastMinutesTM > 1))
                            {
                                clrBackColor = Color.Red;
                                cntWarn ++;
                            }
                            else
                                cntWarn = 0;

                            if (cntWarn > 0) {
                                //strWarn = cntWarn + @":";
                            }
                            else
                                strWarn = string.Empty;

                            m_dictLabelVal[g.m_id][hour - 1].Text = strWarn + m_tecView.m_dictValuesTECComponent[hour - 1][g.m_id].valuesLastMinutesTM.ToString(@"F2");
                        }
                        else
                            m_dictLabelVal[g.m_id][hour - 1].Text = 0.ToString (@"F0");

                        m_dictLabelVal[g.m_id][hour - 1].BackColor = clrBackColor;
                    }
                }
            }
        }
    }
}
