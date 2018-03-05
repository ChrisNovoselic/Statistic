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


using StatisticCommon;
using ASUTP.Core;
using ASUTP.Control;
using ASUTP;

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

    public partial class PanelCurPower : PanelStatisticWithTableHourRows
    {
        enum INDEX_LABEL : int { NAME
            , DATETIME_TM_GEN
            , DATETIME_TM_SN
            , VALUE_TM_GEN
            , VALUE_TM_SN
            , NAME_COMPONENT
            , VALUE_COMPONENT
            , NAME_TG
            , VALUE_TG
                , COUNT_INDEX_LABEL
        };

        private const int COUNT_FIXED_ROWS = (int)INDEX_LABEL.VALUE_TM_SN - 1;
        /// <summary>
        /// 
        /// </summary>
        private static Color s_clrBackColorLabel = Color.FromArgb(212, 208, 200)
            , s_clrBackColorLabelVal_TM_Gen = Color.FromArgb(219, 223, 227)
            , s_clrBackColorLabelVal_TM_SN = Color.FromArgb(219, 223, 247);
        /// <summary>
        /// Стили для элементов интерфейса - подписей к полям с отображаемыми джанными
        /// </summary>
        private static HLabelStyles[] s_arLabelStyles = { new HLabelStyles(FormMain.formGraphicsSettings.FontColor // NAME
                , FormMain.formGraphicsSettings.BackgroundColor == SystemColors.Control ? s_clrBackColorLabel : FormMain.formGraphicsSettings.BackgroundColor
                , 22F
                , ContentAlignment.MiddleCenter),
            new HLabelStyles(FormMain.formGraphicsSettings.FontColor // DATETIME_TM
                , FormMain.formGraphicsSettings.BackgroundColor == SystemColors.Control ? s_clrBackColorLabelVal_TM_Gen : FormMain.formGraphicsSettings.BackgroundColor
                , 18F
                , ContentAlignment.MiddleCenter),
            new HLabelStyles(FormMain.formGraphicsSettings.FontColor // DATETIME_TM_SN
                , FormMain.formGraphicsSettings.BackgroundColor == SystemColors.Control ? s_clrBackColorLabelVal_TM_SN : FormMain.formGraphicsSettings.BackgroundColor
                , 18F
                , ContentAlignment.MiddleCenter),
            new HLabelStyles(FormMain.formGraphicsSettings.FontColor // VALUE_TM
                , FormMain.formGraphicsSettings.BackgroundColor == SystemColors.Control ? s_clrBackColorLabelVal_TM_Gen : FormMain.formGraphicsSettings.BackgroundColor
                , 18F
                , ContentAlignment.MiddleCenter),
            new HLabelStyles(FormMain.formGraphicsSettings.FontColor // VALUE_TM_SN
                , FormMain.formGraphicsSettings.BackgroundColor == SystemColors.Control ? s_clrBackColorLabelVal_TM_SN : FormMain.formGraphicsSettings.BackgroundColor
                , 18F
                , ContentAlignment.MiddleCenter),
            new HLabelStyles(FormMain.formGraphicsSettings.FontColor // NAME_COMPONENT
                , FormMain.formGraphicsSettings.BackgroundColor == SystemColors.Control ? s_clrBackColorLabel : FormMain.formGraphicsSettings.BackgroundColor
                , 14F
                , ContentAlignment.MiddleCenter),
            new HLabelStyles(FormMain.formGraphicsSettings.FontColor // VALUE_COMPONENT
                , FormMain.formGraphicsSettings.BackgroundColor == SystemColors.Control ? s_clrBackColorLabelVal_TM_Gen : FormMain.formGraphicsSettings.BackgroundColor
                , 14F
                , ContentAlignment.MiddleRight),
            new HLabelStyles(FormMain.formGraphicsSettings.FontColor // NAME_TG
                , FormMain.formGraphicsSettings.BackgroundColor == SystemColors.Control ? s_clrBackColorLabel : FormMain.formGraphicsSettings.BackgroundColor
                , 14F
                , ContentAlignment.MiddleCenter),
            new HLabelStyles(FormMain.formGraphicsSettings.FontColor // VALUE_TG
                , FormMain.formGraphicsSettings.BackgroundColor == SystemColors.Control ? s_clrBackColorLabelVal_TM_Gen : FormMain.formGraphicsSettings.BackgroundColor
                , 14F
                , ContentAlignment.MiddleRight)};

        /// <summary>
        /// Инициализация характеристик, стилей макета для размещения дочерних элементов интерфейса
        ///  (должна быть вызвана явно)
        /// </summary>
        /// <param name="col">Количество столбцов в макете</param>
        /// <param name="row">Количество строк в макете</param>
        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            this.ColumnCount = cols;
            if (this.ColumnCount == 0) this.ColumnCount++; else ;
            this.RowCount = rows / this.ColumnCount;

            initializeLayoutStyleEvenly ();
        }

        public PanelCurPower(List<StatisticCommon.TEC> listTec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
            : base (MODE_UPDATE_VALUES.AUTO, FormMain.formGraphicsSettings.FontColor, FormMain.formGraphicsSettings.BackgroundColor)
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;

            //this.Location = new System.Drawing.Point(40, 58);
            //this.Name = "pnlView";
            //this.Size = new System.Drawing.Size(705, 747);

            this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle;

            PanelTecCurPower ptcp;

            initializeLayoutStyle(listTec.Count / 2
                                , listTec.Count);
            // фильтр ТЭЦ-ЛК
            for (int i = 0; i < listTec.Count; i++)
                if (!(listTec[i].m_id > (int)TECComponent.ID.LK))
                {
                    ptcp = new PanelTecCurPower(listTec[i]/*, fErrRep, fWarRep, fActRep, fRepClr*/);
                    this.Controls.Add(ptcp, i % this.ColumnCount, i / this.ColumnCount);
                }
                else
                    ;    
        }

        public PanelCurPower(IContainer container, List<StatisticCommon.TEC> listTec, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr)
            : this(listTec/*, fErrRep, fWarRep, fActRep, fRepClr*/)
        {
            container.Add(this);
        }

        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            foreach (Control ptcp in this.Controls)
                if (ptcp is PanelTecCurPower)
                    (ptcp as PanelTecCurPower).SetDelegateReport(ferr, fwar, fact, fclr);
                else
                    ;
        }

        public override void Start () {
            base.Start ();

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

            base.Stop ();
        }

        public override bool Activate (bool active) {
            bool bRes = base.Activate (active);

            if (bRes == false)
                return bRes;
            else
                ;

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

            return bRes;
        }

        protected override void initTableHourRows () {
            //Ничего не делаем, т.к. нет таблиц с часовыми значениями
        }

        public override void UpdateGraphicsCurrent (int type)
        {
            getTypedControls(this, new Type [] { typeof(PanelTecCurPower) }).Cast<PanelTecCurPower> ().ToList().ForEach(panel => {
                panel.UpdateGraphicsCurrent ();
            });
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

        private partial class PanelTecCurPower : HPanelCommon
        {
            Label[] m_arLabel;
            Dictionary<int, Label> m_dictLabelVal;

            public StatisticCommon.TecViewTMPower m_tecView;

            private object m_lockRep;
            private ManualResetEvent m_evTimerCurrent;
            private
                System.Threading.Timer //Вариант №0
                //System.Windows.Forms.Timer //Вариант №1
                    m_timerCurrent
                    ;

            private struct TAG_LABEL {
                public INDEX_LABEL m_indexLabel;

                public int m_idTEComponent;
            }

            public PanelTecCurPower(StatisticCommon.TEC tec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
                : base (-1, -1)
            {
                InitializeComponent();

                m_tecView = new TecViewTMPower();

                HMark markQueries = new HMark(new int [] {(int)CONN_SETT_TYPE.ADMIN, (int)CONN_SETT_TYPE.DATA_SOTIASSO});
                markQueries.Marked((int)CONN_SETT_TYPE.ADMIN); //Для получения даты/времени
                markQueries.Marked ((int)CONN_SETT_TYPE.DATA_SOTIASSO);

                m_tecView.InitTEC (new List <StatisticCommon.TEC> () { tec }, markQueries);
                //m_tecView.SetDelegateReport(fErrRep, fWarRep, fActRep, fRepClr);

                m_tecView.updateGUI_TM_Gen = new DelegateFunc (updateGUI_TM_Gen);
                m_tecView.updateGUI_TM_SN = new DelegateFunc(updateGUI_TM_SN);

                ForeColorChanged += onForeColorChanged;
                BackColorChanged += onBackColorChanged;

                Initialize ();
            }

            public PanelTecCurPower(IContainer container, StatisticCommon.TEC tec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
                : this(tec/*, fErrRep, fWarRep, fActRep, fRepClr*/)
            {
                container.Add(this);
            }

            public void SetDelegateReport (DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr)
            {
                m_tecView.SetDelegateReport(fErrRep, fWarRep, fActRep, fRepClr);
            }

            private void Initialize()
            {
                int i = -1;
                Label lblNameTECComponent
                    , lblNameTG;

                m_dictLabelVal = new Dictionary<int, Label>();
                m_arLabel = new Label[(int)INDEX_LABEL.VALUE_TM_SN + 1];

                this.Dock = DockStyle.Fill;
                //Свойства колонок
                this.ColumnCount = 4;

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
                for (i = (int)INDEX_LABEL.DATETIME_TM_GEN; i < (int)INDEX_LABEL.NAME_COMPONENT; i++)
                {
                    switch (i)
                    {
                        case (int)INDEX_LABEL.DATETIME_TM_GEN:
                        case (int)INDEX_LABEL.DATETIME_TM_SN:
                            cntnt = @"--:--:--";
                            break;
                        case (int)INDEX_LABEL.VALUE_TM_GEN:
                        case (int)INDEX_LABEL.VALUE_TM_SN:
                            cntnt = @"---";
                            break;
                        default:
                            break;
                    }
                    m_arLabel[i] = HLabel.createLabel(cntnt, PanelCurPower.s_arLabelStyles[i]);

                    this.Controls.Add(m_arLabel[i], (i % 2) * 2, i / 2);
                    this.SetColumnSpan(m_arLabel[i], 2);
                }

                this.RowCount = COUNT_FIXED_ROWS;

                //m_list_TECComponents = new List <TECComponentBase> ();
                foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
                {
                    if (g.IsGTP == true)
                    {
                        //Добавить ГТП в список компонентов
                        //m_list_TECComponents.Add(g);

                        //Добавить наименование ГТП
                        lblNameTECComponent = HLabel.createLabel(g.name_shr, PanelCurPower.s_arLabelStyles[(int)INDEX_LABEL.NAME_COMPONENT]);
                        lblNameTECComponent.Tag = new TAG_LABEL () { m_indexLabel = INDEX_LABEL.NAME_COMPONENT, m_idTEComponent = g.m_id };
                        this.Controls.Add(lblNameTECComponent, 0, this.RowCount);
                        m_dictLabelVal.Add(g.m_id, HLabel.createLabel(@"---", PanelCurPower.s_arLabelStyles[(int)INDEX_LABEL.VALUE_TG]));
                        this.Controls.Add(m_dictLabelVal[g.m_id], 1, this.RowCount);
                        //m_dictLabelVal[g.m_id].TextChanged += new EventHandler(PanelTecCurPower_TextChangedValue);

                        foreach (TECComponentBase tc in g.ListLowPointDev)
                        {
                            //Добавить наименование ТГ
                            lblNameTG = HLabel.createLabel (tc.name_shr, PanelCurPower.s_arLabelStyles [(int)INDEX_LABEL.NAME_TG]);
                            lblNameTG.Tag = new TAG_LABEL () { m_indexLabel = INDEX_LABEL.NAME_TG, m_idTEComponent = tc.m_id };
                            this.Controls.Add(lblNameTG, 2, this.RowCount);
                            //Добавить значение ТГ
                            m_dictLabelVal.Add(tc.m_id, HLabel.createLabel(@"---", PanelCurPower.s_arLabelStyles[(int)INDEX_LABEL.VALUE_TG]));
                            this.Controls.Add(m_dictLabelVal[tc.m_id], 3, this.RowCount);
                            //m_dictLabelVal[tg.m_id].TextChanged += new EventHandler(PanelTecCurPower_TextChangedValue);

                            this.RowCount++;
                        }

                        this.SetRowSpan(lblNameTECComponent, g.ListLowPointDev.Count);
                        this.SetRowSpan(m_dictLabelVal[g.m_id], g.ListLowPointDev.Count);
                    }
                    else
                        ;
                }

                initializeLayoutStyle ();

                m_lockRep = new object();
            }
            /// <summary>
            /// Инициализация характеристик, стилей макета для размещения дочерних элементов интерфейса
            ///  (должна быть вызвана явно)
            /// </summary>
            /// <param name="col">Количество столбцов в макете</param>
            /// <param name="row">Количество строк в макете</param>
            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {                
                int i = -1;

                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

                //Свойства зафиксированных строк
                for (i = 0; i < COUNT_FIXED_ROWS; i++)
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, 10));

                //Свойства НЕзафиксированных строк
                //this.RowCount = m_dictLabelVal.Count + COUNT_FIXED_ROWS;
                for (i = 0; i < this.RowCount - COUNT_FIXED_ROWS; i++)
                {
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, (float)Math.Round((double)(100 - (10 * COUNT_FIXED_ROWS)) / (this.RowCount - COUNT_FIXED_ROWS), 1)));
                }
            }

            private void onForeColorChanged (object sender, EventArgs e)
            {
                // общие подписи, общие значения
                if (Equals (m_arLabel, null) == false)
                    foreach (Label label in m_arLabel)
                        label.ForeColor = ForeColor;
                else
                    ;
                // значения
                if (Equals (m_dictLabelVal, null) == false) {
                    foreach (Label label in m_dictLabelVal.Values) {
                        label.ForeColor = ForeColor;
                    }
                } else
                    ;
                // подписи 
                //TODO: все остальные подписи реализовать аналогично
                (from ctrl in this.Controls.Cast<Control> () where Equals (ctrl.Tag, null) == false select ctrl)
                    .ToList ()
                        .ForEach (ctrl => {
                            if (ctrl.Tag is TAG_LABEL) {
                                ctrl.ForeColor = ForeColor;
                            } else
                                ;
                        });
            }

            private void onBackColorChanged (object sender, EventArgs e)
            {
                // общие подписи, общие значения
                if (Equals (m_arLabel, null) == false)
                    for (int i = 0; i < m_arLabel.Length; i++) {
                        switch ((INDEX_LABEL)i) {
                            case INDEX_LABEL.DATETIME_TM_SN:
                            case INDEX_LABEL.VALUE_TM_SN:
                                s_arLabelStyles [i].m_backColor =
                                m_arLabel [i].BackColor =
                                    BackColor == SystemColors.Control
                                        ? s_clrBackColorLabelVal_TM_SN
                                            : BackColor;
                                break;
                            case INDEX_LABEL.DATETIME_TM_GEN:
                            case INDEX_LABEL.VALUE_TM_GEN:
                                s_arLabelStyles [i].m_backColor =
                                m_arLabel [i].BackColor =
                                    BackColor == SystemColors.Control
                                        ? s_clrBackColorLabelVal_TM_Gen
                                            : BackColor;
                                break;
                            case INDEX_LABEL.NAME:
                            case INDEX_LABEL.NAME_COMPONENT:
                            case INDEX_LABEL.NAME_TG:
                                m_arLabel [i].BackColor = BackColor == SystemColors.Control
                                        ? s_clrBackColorLabel
                                            : BackColor;
                                ;
                                break;
                        }
                    }
                else
                    ;
                // значения
                if (Equals (m_dictLabelVal, null) == false) {
                    foreach (Label label in m_dictLabelVal.Values) {
                        label.BackColor =
                            BackColor == SystemColors.Control
                                ? s_clrBackColorLabelVal_TM_Gen
                                    : BackColor;
                    }
                } else
                    ;
                // подписи 
                //TODO: все остальные подписи реализовать аналогично
                (from ctrl in this.Controls.Cast<Control>() where Equals (ctrl.Tag, null) == false select ctrl)
                    .ToList()
                        .ForEach(ctrl => {
                            if (ctrl.Tag is TAG_LABEL) {
                                ctrl.BackColor =
                                    BackColor == SystemColors.Control
                                        ? s_clrBackColorLabel
                                            : BackColor;
                            } else
                                ;
                        });
            }

            public override void Start()
            {
                base.Start ();

                if (m_tecView.IsStarted == true)
                    return;
                else
                    ;
                
                m_tecView.Start ();

                m_evTimerCurrent = new ManualResetEvent(true);
                //m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, PanelStatistic.POOL_TIME * 1000 - 1, PanelStatistic.POOL_TIME * 1000 - 1);
                m_timerCurrent =
                    new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, PanelStatistic.POOL_TIME * 1000 - 1, System.Threading.Timeout.Infinite)
                    //new System.Windows.Forms.Timer ()
                    ;
                ////Вариант №1
                //m_timerCurrent.Interval = ProgramBase.TIMER_START_INTERVAL; //для реализации задержки на PanelStatistic.POOL_TIME * 1000 - 1;
                //m_timerCurrent.Tick += new EventHandler(TimerCurrent_Tick);
                //m_timerCurrent.Start ();
            }

            public override void Stop()
            {
                if (m_tecView.IsStarted == false)
                    return;
                else
                    ;

                m_tecView.Stop();

                m_evTimerCurrent?.Reset();
                m_timerCurrent?.Dispose();

                lock (m_lockRep)
                {
                    m_tecView.ReportClear(true);
                }

                base.Stop ();
            }

            public void InitTableHourRows (DateTime dt) {
                m_tecView.m_curDate = dt;
            }

            private void changeState()
            {
                m_tecView.ChangeState ();
            }

            public override bool Activate(bool active)
            {
                bool bRes = base.Activate (active);
                
                m_tecView.Activate(active);

                if (m_tecView.Actived == true)
                {
                    //Вариант №0
                    m_timerCurrent.Change(0, System.Threading.Timeout.Infinite);
                    ////Вариант №0
                    //m_timerCurrent.Start ();
                }
                else
                {
                    m_tecView.ReportClear(true);
                }

                return bRes;
            }

            private void updateGUI_TM_Gen()
            {
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new DelegateFunc(showTMGenPower));
                else
                    Logging.Logg().Error(@"PanelTecCurPower::showTMGenPower () - ... BeginInvoke (ShowTMGenPower) - ...", Logging.INDEX_MESSAGE.D_001);
            }

            private void updateGUI_TM_SN()
            {
                if (InvokeRequired)
                    this.BeginInvoke(new DelegateFunc(showTMSNPower));
                else
                    Logging.Logg().Error(@"PanelTecCurPower::showTMSNPower () - ... BeginInvoke (ShowTMSNPower) - ...", Logging.INDEX_MESSAGE.D_001);
            }

            private void showTMGenPower () {
                double dblTotalPower_TM = 0.0
                        , dblTECComponentPower_TM = 0.0;
                foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
                {
                    if (g.IsGTP == true)
                    {
                        dblTECComponentPower_TM = 0.0;

                        foreach (TG tg in g.ListLowPointDev)
                        {
                            if (tg.m_strKKS_NAME_TM.Length > 0) {
                                dblTECComponentPower_TM += setTextToLabelVal(m_dictLabelVal[tg.m_id], m_tecView.m_dictValuesLowPointDev[tg.m_id].m_powerCurrent_TM);
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
                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM_GEN], m_tecView.m_dblTotalPower_TM_SN);
                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM_GEN], dblTotalPower_TM);

                switch (m_tecView.m_dtLastChangedAt_TM_Gen.Kind) {
                    default:
                        break;
                }

                //m_tecView.m_dtLastChangedAt_TM_Gen = HDateTime.ToMoscowTimeZone(m_tecView.m_dtLastChangedAt_TM_Gen);

                setTextToLabelDateTime(m_arLabel[(int)INDEX_LABEL.DATETIME_TM_GEN], m_tecView.m_dtLastChangedAt_TM_Gen);
            }

            private void showTMSNPower()
            {
                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM_SN], m_tecView.m_dblTotalPower_TM_SN);

                setTextToLabelDateTime(m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN], m_tecView.m_dtLastChangedAt_TM_SN);
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

            /// <summary>
            /// Отобразить значение дату/время (копия в PanelTMSNPower.PanelTecTMSNPower)
            /// </summary>
            /// <param name="dt">элемент управления для отображения значения</param>
            /// <param name="dt">Дата/время для отображения</param>
            private void setTextToLabelDateTime (Label label, DateTime dtVal)
            {
                Color clrDatetime = Color.Empty;
                string strFmtDatetime = @"HH:mm:ss";

                if (TecView.ValidateDatetimeTMValue (m_tecView.serverTime, dtVal) == false) {
                    strFmtDatetime = @"dd.MM.yyyy " + strFmtDatetime; // добавить дату
                    clrDatetime = Color.Red;
                } else
                // формат даты/времени без изменения (без даты)
                // цвет тоже без изменений
                    ;

                    label.Text = dtVal.ToString(strFmtDatetime);
                if (clrDatetime.Equals (Color.Empty) == false)
                    label.ForeColor = clrDatetime;
                else
                    ;
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
            //private void TimerCurrent_Tick(Object stateInfo, EventArgs ev)
            {
                if (m_tecView.Actived == true)
                {
                    ////Вариант №1
                    //if (m_timerCurrent.Interval == ProgramBase.TIMER_START_INTERVAL)
                    //{
                    //    m_timerCurrent.Interval = PanelStatistic.POOL_TIME * 1000 - 1;

                    //    return;
                    //}
                    //else
                    //    ;

                    m_tecView.ChangeState ();

                    //Вариант №0
                    m_timerCurrent.Change(PanelStatistic.POOL_TIME * 1000 - 1, System.Threading.Timeout.Infinite);
                }
                else
                    ;
            }

            public void UpdateGraphicsCurrent ()
            {
                showTMGenPower ();
                showTMSNPower ();
            }
        }
    }
}
