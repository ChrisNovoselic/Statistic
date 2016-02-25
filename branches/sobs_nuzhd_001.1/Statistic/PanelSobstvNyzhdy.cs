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

using ZedGraph;
using GemBox.Spreadsheet;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    partial class PanelSobstvNyzhdy
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

    public partial class PanelSobstvNyzhdy : PanelStatisticWithTableHourRows
    {
        enum INDEX_LABEL : int
        {
            NAME,
            DATETIME_TM_SN,
            VALUE_TM_SN,
            COUNT_INDEX_LABEL
        };
        const int COUNT_FIXED_ROWS = (int)INDEX_LABEL.VALUE_TM_SN - 0;

        static Color s_clrBackColorLabel = Color.FromArgb(212, 208, 200), s_clrBackColorLabelVal_TM = Color.FromArgb(219, 223, 227), s_clrBackColorLabelVal_TM_SN = Color.FromArgb(219, 223, 247);
        static HLabelStyles[] s_arLabelStyles = { new HLabelStyles(Color.Black, s_clrBackColorLabel, 15F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM_SN, 11F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM_SN, 11F, ContentAlignment.MiddleCenter)};

        enum StatesMachine : int { Init_TM, Current_TM_Gen, Current_TM_SN };

        public PanelSobstvNyzhdy(List<StatisticCommon.TEC> listTec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
        {
            InitializeComponent();

            //this.Dock = DockStyle.Fill;

            ////this.Location = new System.Drawing.Point(40, 58);
            ////this.Name = "pnlView";
            ////this.Size = new System.Drawing.Size(705, 747);

            //this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle;

            PanelTecSobstvNyzhdy ptcp;

            int i = -1;

            initializeLayoutStyle(listTec.Count / 2
                , listTec.Count);

            for (i = 1; i < 2; i++)
            {
                ptcp = new PanelTecSobstvNyzhdy(listTec[i]/*, fErrRep, fWarRep, fActRep, fRepClr*/);
                this.Controls.Add(ptcp, i % this.ColumnCount, i / this.ColumnCount);
            }
        }

        public PanelSobstvNyzhdy(IContainer container, List<StatisticCommon.TEC> listTec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fREpClr*/)
            : this(listTec/*, fErrRep, fWarRep, fActRep, fREpClr*/)
        {
            container.Add(this);
        }

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            this.ColumnCount = cols;
            if (this.ColumnCount == 0) this.ColumnCount++; else ;
            this.RowCount = rows / this.ColumnCount;

            initializeLayoutStyleEvenly ();
        }

        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            foreach (Control ptcp in this.Controls)
                if (ptcp is PanelTecSobstvNyzhdy)
                    (ptcp as PanelTecSobstvNyzhdy).SetDelegateReport(ferr, fwar, fact, fclr);
                else
                    ;
        }

        public override void Start()
        {
            base.Start();
            
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecSobstvNyzhdy)
                {
                    ((PanelTecSobstvNyzhdy)ctrl).Start();
                }
                else
                    ;
            }
        }

        public override void Stop()
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecSobstvNyzhdy)
                {
                    ((PanelTecSobstvNyzhdy)ctrl).Stop();
                }
                else
                    ;
            }

            base.Stop();
        }

        protected override void initTableHourRows()
        {
            //Ничего не делаем (на данный момент), т.к. собственные нужды отображаем только тек./сутки
        }

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);

            if (bRes == false)
                return bRes;
            else
                ;

            //TypeConverter conv;
            //dynamic dynObj = null;
            Type typeChildren; //PanelTecCurPower
            typeChildren = typeof(PanelTecSobstvNyzhdy);

            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl.GetType().Equals(typeChildren) == true)
                {
                    ((PanelTecSobstvNyzhdy)ctrl).Activate(active);
                }
                else
                    ;
            }

            return bRes;
        }

        public void UpdateGraphicsCurrent(int type)
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecSobstvNyzhdy)
                {
                    ((PanelTecSobstvNyzhdy)ctrl).drawGraphHours();
                }
                else
                    ;
            }
        }

        partial class PanelTecSobstvNyzhdy
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

                this.m_zedGraphHours = new ZedGraphControl();
                this.m_zedGraphHours.Dock = System.Windows.Forms.DockStyle.Fill;
                //this.m_zedGraphHours.Location = arPlacement[(int)CONTROLS.m_zedGraphHours].pt;
                this.m_zedGraphHours.Name = "zedGraphHour";
                this.m_zedGraphHours.ScrollGrace = 0;
                this.m_zedGraphHours.ScrollMaxX = 0;
                this.m_zedGraphHours.ScrollMaxY = 0;
                this.m_zedGraphHours.ScrollMaxY2 = 0;
                this.m_zedGraphHours.ScrollMinX = 0;
                this.m_zedGraphHours.ScrollMinY = 0;
                this.m_zedGraphHours.ScrollMinY2 = 0;
                //this.m_zedGraphHours.Size = arPlacement[(int)CONTROLS.m_zedGraphHours].sz;
                this.m_zedGraphHours.TabIndex = 0;
                this.m_zedGraphHours.IsEnableHEdit = false;
                this.m_zedGraphHours.IsEnableHPan = false;
                this.m_zedGraphHours.IsEnableHZoom = false;
                this.m_zedGraphHours.IsEnableSelection = false;
                this.m_zedGraphHours.IsEnableVEdit = false;
                this.m_zedGraphHours.IsEnableVPan = false;
                this.m_zedGraphHours.IsEnableVZoom = false;
                this.m_zedGraphHours.IsShowPointValues = true;
                this.m_zedGraphHours.MouseUpEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zedGraphHours_MouseUpEvent);
                this.m_zedGraphHours.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.zedGraphHours_PointValueEvent);
                this.m_zedGraphHours.DoubleClickEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zedGraphHours_DoubleClickEvent);
            }

            #endregion
        }

        private partial class PanelTecSobstvNyzhdy : HPanelCommon
        {
            System.Windows.Forms.Label[] m_arLabel;
            System.Windows.Forms.DateTimePicker dtCurrDate;
            Dictionary<int, System.Windows.Forms.Label> m_dictLabelVal;
            DateTime m_datetime;

            //bool isActive;

            public TecView m_tecView;

            private ManualResetEvent m_evTimerCurrent;
            private
                System.Threading.Timer //Вариант №0
                //System.Windows.Forms.Timer //Вариант №1
                    m_timerCurrent;

            ZedGraphControl m_zedGraphHours;

            public PanelTecSobstvNyzhdy(StatisticCommon.TEC tec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
                : base (-1, -1)
            {
                InitializeComponent();

                m_tecView = new TecView(TecView.TYPE_PANEL.SOBSTV_NYZHDY, -1, -1);

                HMark markQueries = new HMark(new int[] { (int)CONN_SETT_TYPE.DATA_AISKUE, (int)CONN_SETT_TYPE.DATA_SOTIASSO });
                //markQueries.Marked((int)CONN_SETT_TYPE.DATA_AISKUE); //Только для определения сезона ???
                //markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO);

                m_tecView.InitTEC (new List <TEC> () { tec }, markQueries);
                //m_tecView.SetDelegateReport(fErrRep, fWarRep, fActRep, fRepClr);

                m_tecView.updateGUI_TM_SN = new DelegateFunc(showTMSNPower);

                Initialize();

                dtCurrDate.Value = DateTime.Now.Date;
            }

            public PanelTecSobstvNyzhdy(IContainer container, StatisticCommon.TEC tec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
                : this(tec/*, fErrRep, fWarRep, fActRep, fRepClr*/)
            {
                container.Add(this);
            }

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                throw new NotImplementedException();
            }

            private void Initialize()
            {
                int i = -1;

                m_dictLabelVal = new Dictionary<int, System.Windows.Forms.Label>();
                dtCurrDate = new DateTimePicker();
                m_arLabel = new System.Windows.Forms.Label[(int)INDEX_LABEL.VALUE_TM_SN + 1];

                this.Dock = DockStyle.Fill;
                //Свойства колонок
                this.ColumnCount = 4;
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

                //Видимая граница для отладки
                this.BorderStyle = BorderStyle.FixedSingle; //BorderStyle.None;

                string cntnt = string.Empty;

                i = (int)INDEX_LABEL.NAME;
                cntnt = m_tecView.m_tec.name_shr;
                m_arLabel[i] = HLabel.createLabel(cntnt, PanelSobstvNyzhdy.s_arLabelStyles[i]);
                ////Предусмотрим обработчик при изменении значения
                //if (i == (int)INDEX_LABEL.VALUE_TOTAL)
                //    m_arLabel[i].TextChanged += new EventHandler(PanelTecCurPower_TextChangedValue);
                //else
                //    ;
                //this.Controls.Add(m_arLabel[i], 0, i);
                //this.SetRowSpan(m_arLabel[i], COUNT_FIXED_ROWS);
                this.Controls.Add(m_arLabel[i], 0, i); this.SetRowSpan(m_arLabel[i], COUNT_FIXED_ROWS); this.SetColumnSpan(m_arLabel[i], 2);

                //Наименование ТЭЦ, Дата/время, Значение для всех ГТП/ТГ
                for (i = (int)INDEX_LABEL.DATETIME_TM_SN; i < (int)INDEX_LABEL.VALUE_TM_SN + 1; i++)
                {
                    switch (i)
                    {
                        case (int)INDEX_LABEL.DATETIME_TM_SN:
                            cntnt = @"--:--:--";
                            break;
                        case (int)INDEX_LABEL.VALUE_TM_SN:
                            cntnt = @"---";
                            break;
                        default:
                            break;
                    }
                    m_arLabel[i] = HLabel.createLabel(cntnt, PanelSobstvNyzhdy.s_arLabelStyles[i]);

                    this.Controls.Add(m_arLabel[i], 1*i+1, 1); this.SetRowSpan(m_arLabel[i], 1); this.SetColumnSpan(m_arLabel[i], 1);
                }

                this.Controls.Add(dtCurrDate, 2, 0); this.SetRowSpan(dtCurrDate, 1); this.SetColumnSpan(dtCurrDate, 2);
                
                //dtCurrDate.ValueChanged += new EventHandler(dtCurrDate_ChangeValue);

                this.RowCount = COUNT_FIXED_ROWS;

                //Свойства зафиксированных строк
                for (i = 0; i < COUNT_FIXED_ROWS; i++)
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, 5));

                
                this.Controls.Add(m_zedGraphHours, 0, COUNT_FIXED_ROWS);
                this.SetColumnSpan(m_zedGraphHours, this.ColumnCount);
                this.RowStyles.Add(new RowStyle(SizeType.Percent, 90));

                //isActive = false;
            }

            public void SetDelegateReport(DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr)
            {
                m_tecView.SetDelegateReport(fErrRep, fWarRep, fActRep, fRepClr);
            }

            public override void Start()
            {
                base.Start ();
                
                m_tecView.Start();

                m_evTimerCurrent = new ManualResetEvent(true);
                m_timerCurrent = new
                    System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, PanelStatistic.POOL_TIME * 1000 - 1, PanelStatistic.POOL_TIME * 1000 - 1)
                    //System.Windows.Forms.Timer ()
                    ;
                //Вариант №1
                //m_timerCurrent.Interval = ProgramBase.TIMER_START_INTERVAL; // для реализации задержки выполнения итерации
                //m_timerCurrent.Tick += new EventHandler(TimerCurrent_Tick);
                //m_timerCurrent.Start ();

                //isActive = false;
            }

            public override void Stop()
            {
                m_tecView.Stop ();

                if (!(m_evTimerCurrent == null)) m_evTimerCurrent.Reset(); else ;
                if (!(m_timerCurrent == null)) m_timerCurrent.Dispose(); else ;

                base.Stop ();
            }

            private void dtCurrDate_ChangeValue(object sender, EventArgs e)
            {
            }

            private void ChangeState()
            {
                if (dtCurrDate.Value.Date != DateTime.Now.Date)
                    m_tecView.currHour = false;
                else
                    m_tecView.currHour = true;

                m_tecView.m_curDate = dtCurrDate.Value.Date; //DateTime.Now;

                m_tecView.ChangeState ();
            }

            public override bool Activate(bool active)
            {
                bool bRes = base.Activate (active);

                if (bRes == false)
                    return false;
                else
                    ;

                if (Actived == true)
                {
                    ChangeState();
                }
                else
                {
                    m_tecView.ClearStates ();
                }

                return bRes;
            }

            private void showTMSNPower()
            {
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new DelegateFunc(ShowTMSNPower));
                else
                    Logging.Logg().Error(@"PanelTecSobstvNyzhdy::showTMSNPower () - ... BeginInvoke (ShowTMSNPower) - ...", Logging.INDEX_MESSAGE.D_001);
            }

            private void ShowTMSNPower()
            {
                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM_SN], m_tecView.m_dblTotalPower_TM_SN);
                //try { m_dtLastChangedAt = HAdmin.ToCurrentTimeZone (m_dtLastChangedAt); }
                //catch (Exception e) { Logging.Logg ().Exception (e, @"PanelSobstvNyzhdy::ShowTMSNPower () - ...", Logging.INDEX_MESSAGE.NOT_SET); }
                m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN].Text = m_tecView.m_dtLastChangedAt_TM_SN.ToString(@"HH:mm:ss");

                DrawGraphHours();
            }

            private double setTextToLabelVal(System.Windows.Forms.Label lblVal, double val)
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

            private void TimerCurrent_Tick(Object stateInfo)
            //private void TimerCurrent_Tick(Object stateInfo, EventArgs ev)
            {
                if (Actived == true)
                {
                    //Вариант №1
                    //if (m_timerCurrent.Interval == ProgramBase.TIMER_START_INTERVAL)
                    //{
                    //    m_timerCurrent.Interval = PanelStatistic.POOL_TIME * 1000 - 1;

                    //    return ;
                    //}
                    //else
                    //    ;

                    ChangeState();
                }
                else
                    ;


            }

            private bool zedGraphHours_MouseUpEvent(ZedGraphControl sender, MouseEventArgs e)
            {
                //if (e.Button != MouseButtons.Left)
                //    return true;
                //else
                //    ;

                //object obj;
                //PointF p = new PointF(e.X, e.Y);
                //bool found;
                //int index;

                //found = sender.GraphPane.FindNearestObject(p, CreateGraphics(), out obj, out index);

                //if (!(obj is BarItem) && !(obj is LineItem))
                //    return true;
                //else
                //    ;

                //if (found == true)
                //{
                //    //delegateStartWait();

                //    bool bRetroHour = m_tecView.zedGraphHours_MouseUpEvent(index);

                //    //delegateStopWait();
                //}

                return true;
            }

            private string zedGraphHours_PointValueEvent(object sender, GraphPane pane, CurveItem curve, int iPt)
            {
                return curve[iPt].Y.ToString("f2");
            }

            private bool zedGraphHours_DoubleClickEvent(ZedGraphControl sender, MouseEventArgs e)
            {
                FormMain.formGraphicsSettings.SetScale();

                return true;
            }

            public void drawGraphHours()
            {
                DrawGraphHours ();
            }

            private void DrawGraphHours()
            {
                GraphPane pane = m_zedGraphHours.GraphPane;

                pane.CurveList.Clear();

                int itemscount = m_tecView.m_valuesHours.Length;

                string[] names = new string[itemscount];

                double[] valuesTMSNPsum = new double[itemscount];

                double minimum = double.MaxValue, minimum_scale;
                double maximum = 0, maximum_scale;
                bool noValues = true;
                TecView.valuesTEC[] valTEC = m_tecView.m_valuesHours;
                for (int i = 0; i < itemscount; i++)
                {
                    
                    if (HAdmin.SeasonDateTime.Date == m_tecView.m_curDate.Date) {
                        int offset = m_tecView.GetSeasonHourOffset(i + 1);
                        names[i] = (i + 1 - offset).ToString();
                        if (HAdmin.SeasonDateTime.Hour == i)
                            names[i] += "*";
                        else
                            ;
                    }
                    else
                        names[i] = (i + 1).ToString();

                    valuesTMSNPsum[i] = valTEC[i].valuesTMSNPsum;

                    if ((minimum > valuesTMSNPsum[i]) && (! (valuesTMSNPsum[i] == 0)))
                        minimum = valuesTMSNPsum[i];
                    else
                        ;

                    //noValues = false; //???
                    noValues = ! (valuesTMSNPsum[i] == 0);

                    if (maximum < valuesTMSNPsum[i])
                        maximum = valuesTMSNPsum[i];
                    else
                        ;
                }

                if (!(FormMain.formGraphicsSettings.scale == true))
                    minimum = 0;
                else
                    ;

                if (noValues == true)
                {
                    minimum_scale = 0;
                    maximum_scale = 10;
                }
                else
                {
                    if (minimum != maximum)
                    {
                        minimum_scale = minimum - (maximum - minimum) * 0.2;
                        if (minimum_scale < 0)
                            minimum_scale = 0;
                        maximum_scale = maximum + (maximum - minimum) * 0.2;
                    }
                    else
                    {
                        minimum_scale = minimum - minimum * 0.2;
                        maximum_scale = maximum + maximum * 0.2;
                    }
                }

                pane.Chart.Fill = new Fill(FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.BG_SOTIASSO));

                if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Bar)
                {
                    BarItem curve1 = pane.AddBar("Мощность", null, valuesTMSNPsum, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.SOTIASSO));
                }
                else
                {
                    if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Linear)
                    {
                        int valuescount;

                        //if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.SummerToWinter)
                        //    valuescount = m_tecView.lastHour + 1;
                        //else
                        //    if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.WinterToSummer)
                        //        valuescount = m_tecView.lastHour - 1;
                        //    else
                                valuescount = m_tecView.lastHour;

                        double[] valuesTMSNPsum1 = new double[valuescount];
                        for (int i = 0; i < valuescount; i++)
                            valuesTMSNPsum1[i] = valuesTMSNPsum[i];

                        LineItem curve1 = pane.AddCurve("Мощность", null, valuesTMSNPsum1, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.SOTIASSO));
                    }
                    else
                        ;
                }

                pane.XAxis.Type = AxisType.Text;
                pane.XAxis.Title.Text = "";
                pane.YAxis.Title.Text = "";
                //pane.Title.Text = "Мощность на " + m_pnlQuickData.dtprDate.Value.ToShortDateString();
                pane.Title.Text = "Собственные нужды на " + dtCurrDate.Value.ToShortDateString();

                pane.XAxis.Scale.TextLabels = names;
                pane.XAxis.Scale.IsPreventLabelOverlap = false;

                // Включаем отображение сетки напротив крупных рисок по оси X
                pane.XAxis.MajorGrid.IsVisible = true;
                // Задаем вид пунктирной линии для крупных рисок по оси X:
                // Длина штрихов равна 10 пикселям, ... 
                pane.XAxis.MajorGrid.DashOn = 10;
                // затем 5 пикселей - пропуск
                pane.XAxis.MajorGrid.DashOff = 5;
                // толщина линий
                pane.XAxis.MajorGrid.PenWidth = 0.1F;
                pane.XAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

                // Включаем отображение сетки напротив крупных рисок по оси Y
                pane.YAxis.MajorGrid.IsVisible = true;
                // Аналогично задаем вид пунктирной линии для крупных рисок по оси Y
                pane.YAxis.MajorGrid.DashOn = 10;
                pane.YAxis.MajorGrid.DashOff = 5;
                // толщина линий
                pane.YAxis.MajorGrid.PenWidth = 0.1F;
                pane.YAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

                // Включаем отображение сетки напротив мелких рисок по оси Y
                pane.YAxis.MinorGrid.IsVisible = true;
                // Длина штрихов равна одному пикселю, ... 
                pane.YAxis.MinorGrid.DashOn = 1;
                pane.YAxis.MinorGrid.DashOff = 2;
                // толщина линий
                pane.YAxis.MinorGrid.PenWidth = 0.1F;
                pane.YAxis.MinorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

                // Устанавливаем интересующий нас интервал по оси Y
                pane.YAxis.Scale.Min = minimum_scale;
                pane.YAxis.Scale.Max = maximum_scale;

                m_zedGraphHours.AxisChange();

                m_zedGraphHours.Invalidate();
            }
        }
    }
}
