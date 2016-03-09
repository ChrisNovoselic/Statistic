﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Data;
using System.Globalization;

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
        /// <summary>
        /// Индексы Label'ов
        /// </summary>
        enum INDEX_LABEL : int
        {
            NAME,
            DATETIME_TM_SN,
            VALUE_TM_SN,
            COUNT_INDEX_LABEL
        };

        /// <summary>
        /// Количество строк
        /// </summary>
        const int COUNT_FIXED_ROWS = (int)INDEX_LABEL.VALUE_TM_SN - 0;

        /// <summary>
        /// Цвета Label'ов
        /// </summary>
        static Color s_clrBackColorLabel = Color.FromArgb(212, 208, 200), s_clrBackColorLabelVal_TM = Color.FromArgb(219, 223, 227), s_clrBackColorLabelVal_TM_SN = Color.FromArgb(219, 223, 247);
        
        /// <summary>
        /// Стили Label'ов
        /// </summary>
        static HLabelStyles[] s_arLabelStyles = { new HLabelStyles(Color.Black, s_clrBackColorLabel, 15F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM_SN, 11F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM_SN, 11F, ContentAlignment.MiddleCenter)};

        enum StatesMachine : int { Init_TM, Current_TM_Gen, Current_TM_SN };

        /// <summary>
        /// Конструктор-основной
        /// </summary>
        /// <param name="listTec">Лист ТЭЦ</param>
        public PanelSobstvNyzhdy(List<StatisticCommon.TEC> listTec)
        {
            InitializeComponent();

            PanelTecSobstvNyzhdy ptcp;

            int i = -1;

            initializeLayoutStyle(listTec.Count / 2
                , listTec.Count);

            for (i = 0; i < listTec.Count; i++)
            {
                if (listTec[i].m_id < AdminTS_LK.Index_LK)
                {
                    ptcp = new PanelTecSobstvNyzhdy(listTec[i]);
                    this.Controls.Add(ptcp, i % this.ColumnCount, i / this.ColumnCount);
                }
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="container"></param>
        /// <param name="listTec">Лист ТЭЦ</param>
        public PanelSobstvNyzhdy(IContainer container, List<StatisticCommon.TEC> listTec)
            : this(listTec)
        {
            container.Add(this);
        }

        /// <summary>
        /// Инициализация стиля слоя
        /// </summary>
        /// <param name="cols">Кол-во колоной</param>
        /// <param name="rows">Кол-во строк</param>
        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            this.ColumnCount = cols;
            if (this.ColumnCount == 0) this.ColumnCount++; else ;
            this.RowCount = rows / this.ColumnCount;

            initializeLayoutStyleEvenly ();
        }

        /// <summary>
        /// Метод задания делегатов для передачии сообщения
        /// </summary>
        /// <param name="ferr">Делегат ошибки</param>
        /// <param name="fwar">Делегат предупреждения</param>
        /// <param name="fact">Делегат выполняемого действия</param>
        /// <param name="fclr">Делегат очистки строки</param>
        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            foreach (Control ptcp in this.Controls)
                if (ptcp is PanelTecSobstvNyzhdy)
                    (ptcp as PanelTecSobstvNyzhdy).SetDelegateReport(ferr, fwar, fact, fclr);
                else
                    ;
        }

        /// <summary>
        /// Старт опроса
        /// </summary>
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

        /// <summary>
        /// Остановка опроса
        /// </summary>
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

        /// <summary>
        /// Активация панели
        /// </summary>
        /// <param name="active">Установка состояния панели</param>
        /// <returns>Возвращает состояние после выполнения операции</returns>
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

        /// <summary>
        /// Метод ререрисовки графика
        /// </summary>
        /// <param name="type"></param>
        public void UpdateGraphicsCurrent(int type)
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecSobstvNyzhdy)
                {
                    ((PanelTecSobstvNyzhdy)ctrl).DrawGraphHours();
                }
                else
                    ;
            }
        }

        protected override void initTableHourRows()
        {
            throw new NotImplementedException();
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
                this.m_zedGraphHours = new HZedGraphControlSNHours(m_tecView.m_lockValue);
                
                this.m_zedGraphHours.MouseUpEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zedGraphHours_MouseUpEvent);
                this.m_zedGraphHours.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.zedGraphHours_PointValueEvent);
                this.m_zedGraphHours.DoubleClickEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zedGraphHours_DoubleClickEvent);
                this.m_zedGraphHours.ContextMenuStrip.Items[(int)StatisticCommon.HZedGraphControl.INDEX_CONTEXTMENU_ITEM.VISIBLE_TABLE].Click += new System.EventHandler(отобразитьВТаблицеToolStripMenuItem_Click);
                //this.m_zedGraphHours.InitializeEventHandler(this.эксельToolStripMenuItemHours_Click, this.sourceDataHours_Click);

            }

            #endregion
        }

        private partial class PanelTecSobstvNyzhdy : HPanelCommon
        {
            System.Windows.Forms.Label[] m_arLabel;
            System.Windows.Forms.DateTimePicker dtCurrDate;
            Dictionary<int, System.Windows.Forms.Label> m_dictLabelVal;
            DataGridView dgvSNHour;

            //bool isActive;

            public TecView m_tecView;

            /// <summary>
            /// Текущий индекс компонента из списка 'allTECComponents' (для сохранения между вызовами функций)
            /// </summary>
            public int indx_TECComponent { get { return m_tecView.indxTECComponents; } }
            private ManualResetEvent m_evTimerCurrent;

            /// <summary>
            /// Таймер для опроса
            /// </summary>
            private
                System.Threading.Timer //Вариант №0
                //System.Windows.Forms.Timer //Вариант №1
                    m_timerCurrent,
                    startChangeValue;


            /// <summary>
            /// Экземплят графика
            /// </summary>
            StatisticCommon.HZedGraphControl m_zedGraphHours;

            /// <summary>
            /// Коструктор-основной
            /// </summary>
            /// <param name="tec">Лист ТЭЦ</param>
            public PanelTecSobstvNyzhdy(StatisticCommon.TEC tec)
                : base (-1, -1)
            {
                m_tecView = new TecView(TecView.TYPE_PANEL.SOBSTV_NYZHDY, -1, -1);
                
                InitializeComponent();

                HMark markQueries = new HMark(new int[] { (int)CONN_SETT_TYPE.DATA_AISKUE, (int)CONN_SETT_TYPE.DATA_SOTIASSO });

                m_tecView.InitTEC (new List <TEC> () { tec }, markQueries);

                m_tecView.updateGUI_TM_SN = new DelegateFunc(showTMSNPower);

                Initialize();

                dtCurrDate.Value = DateTime.Now;

                for (int i = (int)StatisticCommon.HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; i < (int)StatisticCommon.HZedGraphControl.INDEX_CONTEXTMENU_ITEM.VISIBLE_TABLE; i++)
                {
                    m_zedGraphHours.ContextMenuStrip.Items[i].Visible = false;
                }

                dgvSNHour.MultiSelect = false;
                dgvSNHour.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvSNHour.ReadOnly = true;
                dgvSNHour.AllowUserToResizeRows = false;
                dgvSNHour.AllowUserToResizeColumns = false;
                dgvSNHour.AllowUserToDeleteRows = false;

                this.m_zedGraphHours.InitializeEventHandler(this.эксельToolStripMenuItemHours_Click, null);

                dgvSNHour.MouseUp += new MouseEventHandler(this.dgvSNHour_SelectionChanged);
            }

            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="container"></param>
            /// <param name="tec">Лист ТЭЦ</param>
            public PanelTecSobstvNyzhdy(IContainer container, StatisticCommon.TEC tec)
                : this(tec)
            {
                container.Add(this);
            }

            /// <summary>
            /// Инициализация стиля слоя
            /// </summary>
            /// <param name="cols">Кол-во колоной</param>
            /// <param name="rows">Кол-во строк</param>
            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Инициализация компонентов
            /// </summary>
            private void Initialize()
            {
                int i = -1;

                m_dictLabelVal = new Dictionary<int, System.Windows.Forms.Label>();
                dtCurrDate = new DateTimePicker();
                m_arLabel = new System.Windows.Forms.Label[(int)INDEX_LABEL.VALUE_TM_SN + 1];
                dgvSNHour = new DataGridView();
                
                this.Dock = DockStyle.Fill;
                dtCurrDate.Dock = DockStyle.Fill;
                dgvSNHour.Dock = DockStyle.Fill;
                dgvSNHour.Columns.Add("Hour", "Час");
                dgvSNHour.Columns["Hour"].Width = 45;
                dgvSNHour.Columns.Add("Value", "Значение");
                dgvSNHour.Columns["Value"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvSNHour.RowHeadersVisible = false;
                dgvSNHour.AllowUserToAddRows = false;


                //Свойства колонок
                this.ColumnCount = 6;
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17)); 
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17));

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
                this.Controls.Add(m_arLabel[i], 0, i); 
                this.SetRowSpan(m_arLabel[i], COUNT_FIXED_ROWS); 
                this.SetColumnSpan(m_arLabel[i], 3);

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
                }
                this.Controls.Add(m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN], 3, 1);
                this.SetRowSpan(m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN], 1);
                this.SetColumnSpan(m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN], 2);

                this.Controls.Add(m_arLabel[(int)INDEX_LABEL.VALUE_TM_SN], 5, 1);
                this.SetRowSpan(m_arLabel[(int)INDEX_LABEL.VALUE_TM_SN], 1);
                this.SetColumnSpan(m_arLabel[(int)INDEX_LABEL.VALUE_TM_SN], 1);

                this.Controls.Add(dtCurrDate, 3, 0); this.SetRowSpan(dtCurrDate, 1); this.SetColumnSpan(dtCurrDate, 3);
                
                dtCurrDate.ValueChanged += new EventHandler(dtCurrDate_ChangeValue);

                this.RowCount = COUNT_FIXED_ROWS;

                //Свойства зафиксированных строк
                for (i = 0; i < COUNT_FIXED_ROWS; i++)
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, 5));

                
                this.Controls.Add(m_zedGraphHours, 0, COUNT_FIXED_ROWS);
                this.SetColumnSpan(m_zedGraphHours, this.ColumnCount);
                this.RowStyles.Add(new RowStyle(SizeType.Percent, 90));

                //isActive = false;
            }

            /// <summary>
            /// Метод задания делегатов для передачии сообщения
            /// </summary>
            /// <param name="fErrRep">Делегат ошибки</param>
            /// <param name="fWarRep">Делегат предупреждения</param>
            /// <param name="fActRep">Делегат выполняемого действия</param>
            /// <param name="fRepClr">Делегат очистки строки</param>
            public void SetDelegateReport(DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr)
            {
                m_tecView.SetDelegateReport(fErrRep, fWarRep, fActRep, fRepClr);
            }

            /// <summary>
            /// Старт опроса
            /// </summary>
            public override void Start()
            {
                base.Start ();
                
                m_tecView.Start();

                m_evTimerCurrent = new ManualResetEvent(true);
                m_timerCurrent = new
                    System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, PanelStatistic.POOL_TIME * 1000 - 1, PanelStatistic.POOL_TIME * 1000 - 1)
                    //System.Windows.Forms.Timer ()
                    ;
                startChangeValue = new 
                    System.Threading.Timer(new TimerCallback(startChangeValue_Tick), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                //Вариант №1
                //m_timerCurrent.Interval = ProgramBase.TIMER_START_INTERVAL; // для реализации задержки выполнения итерации
                //m_timerCurrent.Tick += new EventHandler(TimerCurrent_Tick);
                //m_timerCurrent.Start ();

                //isActive = false;
            }

            /// <summary>
            /// Остановка опроса
            /// </summary>
            public override void Stop()
            {
                m_tecView.Stop ();

                if (!(m_evTimerCurrent == null)) m_evTimerCurrent.Reset(); else ;
                if (!(m_timerCurrent == null)) m_timerCurrent.Dispose(); else ;

                base.Stop ();
            }

            /// <summary>
            /// Обработчик события выбора даты
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void dtCurrDate_ChangeValue(object sender, EventArgs e)
            {
                if (HDateTime.ToMoscowTimeZone(dtCurrDate.Value.Date) == HDateTime.ToMoscowTimeZone(DateTime.Now.Date))
                    setCurrDateHour(HDateTime.ToMoscowTimeZone(DateTime.Now));
                else
                    setCurrDateHour(HDateTime.ToMoscowTimeZone(dtCurrDate.Value.Date));
                

                if ((m_tecView.m_curDate.Date.Equals(HDateTime.ToMoscowTimeZone(DateTime.Now).Date) == true)
                    && (m_tecView.lastHour.Equals(HDateTime.ToMoscowTimeZone(DateTime.Now).Hour) == true))
                {
                    m_tecView.adminValuesReceived = false; //Чтобы не выполнилась ветвь - переход к след./часу
                    m_tecView.currHour = true;
                    if(m_timerCurrent!=null)
                        m_timerCurrent.Change(0, System.Threading.Timeout.Infinite);
                }
                else
                {
                    ChangeState();
                    m_tecView.currHour = false;
                }
            }
            
            /// <summary>
            /// Установить дату/час для объекта обработки запросов к БД
            /// </summary>
            /// <param name="dtNew">Дата/час</param>
            private void setCurrDateHour(DateTime dtNew)
            {
                m_tecView.m_curDate = dtNew;
                m_tecView.lastHour =
                    dtNew.Hour// - 1; //- (int)HDateTime.GetUTCOffsetOfMoscowTimeZone().TotalHours //- 3
                    ;
                if (m_tecView.lastHour < 0)
                {
                    m_tecView.m_curDate = m_tecView.m_curDate.AddDays(-1);
                    m_tecView.lastHour += 24;
                }
                else
                    ;
            }

            /// <summary>
            /// Установка состояния для текущей панели
            /// </summary>
            private void ChangeState()
            {
                m_tecView.ChangeState ();
            }

            /// <summary>
            /// Активация панели
            /// </summary>
            /// <param name="active">Устанавливаемое значение активности</param>
            /// <returns>Значение после выполнения</returns>
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

            /// <summary>
            /// Метод вызова отдельно потока для отображения значения за выьранный час в Label'е
            /// </summary>
            private void showTMSNPower()
            {
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new DelegateFunc(ShowTMSNPower));
                else
                    Logging.Logg().Error(@"PanelTecSobstvNyzhdy::showTMSNPower () - ... BeginInvoke (ShowTMSNPower) - ...", Logging.INDEX_MESSAGE.D_001);
            }

            /// <summary>
            /// Метод для отображения выбранного часа и значения за этот час в Label'ах
            /// </summary>
            private void ShowTMSNPower()
            {
                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM_SN], m_tecView.m_dblTotalPower_TM_SN);
                //try { m_dtLastChangedAt = HAdmin.ToCurrentTimeZone (m_dtLastChangedAt); }
                //catch (Exception e) { Logging.Logg ().Exception (e, @"PanelSobstvNyzhdy::ShowTMSNPower () - ...", Logging.INDEX_MESSAGE.NOT_SET); }
                m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN].Text = m_tecView.m_dtLastChangedAt_TM_SN.ToString(@"HH:mm:ss");

                DrawGraphHours();
            }

            /// <summary>
            /// Изменения текста Label'ов
            /// </summary>
            /// <param name="lblVal">Label</param>
            /// <param name="val">Значение</param>
            /// <returns></returns>
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

            /// <summary>
            /// Обработчик таймера
            /// </summary>
            /// <param name="stateInfo"></param>
            private void startChangeValue_Tick(Object stateInfo)
            {
                dtCurrDate_ChangeValue(null, null);
                startChangeValue.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            }

            
            private void TimerCurrent_Tick(Object stateInfo)
            {
                if (Actived == true)
                {
                    if (m_tecView.currHour == true)
                    {
                        m_timerCurrent.Change(PanelStatistic.POOL_TIME * 1000 - 1, System.Threading.Timeout.Infinite);
                        setCurrDateHour(HDateTime.ToMoscowTimeZone(DateTime.Now));
                        ChangeState();
                        Debug.Print(m_arLabel[0] + " обновление");
                    }
                    else                        
                        ;
                }
                else
                    ;
            }

            /// <summary>
            /// Обработчик выбора значения на графике
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            /// <returns></returns>
            private bool zedGraphHours_MouseUpEvent(ZedGraphControl sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left)
                    return true;
                else
                    ;

                object obj;
                PointF p = new PointF(e.X, e.Y);
                bool found;
                int index;

                found = sender.GraphPane.FindNearestObject(p, CreateGraphics(), out obj, out index);
                
                if (!(obj is BarItem) && !(obj is LineItem))
                    return true;
                else
                    ;

                if (found == true)
                {
                    //delegateStartWait();

                    bool bRetroHour = true;//m_tecView.zedGraphHours_MouseUpEvent(index);


                    if (bRetroHour == true)
                    {
                        m_tecView.currHour = false;
                        DateTime dt = new DateTime(dtCurrDate.Value.Year, dtCurrDate.Value.Month, dtCurrDate.Value.Day, index+1, 0, 0);

                        m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN].Text = dt.ToString(@"HH:mm:ss");
                        setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM_SN], m_tecView.m_valuesHours[index].valuesTMSNPsum);

                        dgvSNHour.Rows[index].Selected = true;
                        foreach (System.Windows.Forms.Label l in m_arLabel)
                        {
                            l.Refresh();
                        }
                        dgvSNHour.Refresh();
                        Debug.Print(m_arLabel[0] + " ретро");
                        startChangeValue.Change(10000, System.Threading.Timeout.Infinite);
                    }                   

                    //delegateStopWait();
                }

                return true;
            }

            /// <summary>
            /// Обработчик выбора строки
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void dgvSNHour_SelectionChanged(object sender, MouseEventArgs e)
            {
                int index;
                if (e.Button == System.Windows.Forms.MouseButtons.Left)

                    if (dgvSNHour.SelectedRows.Count == 1)
                    {
                        index = dgvSNHour.SelectedRows[0].Index;

                        //delegateStartWait();
                        if (index <= dgvSNHour.Rows.Count - 2)
                        {
                            bool bRetroHour = true;//m_tecView.zedGraphHours_MouseUpEvent(index);


                            if (bRetroHour == true)
                            {
                                DateTime dt;
                                m_tecView.currHour = false;
                                if (index == dgvSNHour.Rows.Count - 2)
                                {
                                    dt = new DateTime(dtCurrDate.Value.Year, dtCurrDate.Value.Month, dtCurrDate.Value.Day, 0, 0, 0);
                                }
                                else
                                {
                                    dt = new DateTime(dtCurrDate.Value.Year, dtCurrDate.Value.Month, dtCurrDate.Value.Day, index + 1, 0, 0);
                                }

                                m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN].Text = dt.ToString(@"HH:mm:ss");
                                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM_SN], m_tecView.m_valuesHours[index].valuesTMSNPsum);

                                foreach (System.Windows.Forms.Label l in m_arLabel)
                                {
                                    l.Refresh();
                                }
                                dgvSNHour.Refresh();
                                Debug.Print(m_arLabel[0] + " ретро");
                                startChangeValue.Change(10000, System.Threading.Timeout.Infinite);
                            }
                        }
                    }
            }

            private string zedGraphHours_PointValueEvent(object sender, GraphPane pane, CurveItem curve, int iPt)
            {
                return curve[iPt].Y.ToString("f2");
            }

            /// <summary>
            /// Обработчик двойного нажатия на графике
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            /// <returns></returns>
            private bool zedGraphHours_DoubleClickEvent(ZedGraphControl sender, MouseEventArgs e)
            {
                FormMain.formGraphicsSettings.SetScale();

                return true;
            }

            /// <summary>
            /// Метод для отрисовки графика
            /// </summary>
            public void DrawGraphHours()
            {
                GraphPane pane = m_zedGraphHours.GraphPane;

                dgvSNHour.Rows.Clear();
                pane.CurveList.Clear();

                int itemscount = m_tecView.m_valuesHours.Length;

                string[] names = new string[itemscount];

                double[] valuesTMSNPsum = new double[itemscount];

                TecView.valuesTEC[] valuesHours = new TecView.valuesTEC[m_tecView.m_valuesHours.Length];
                m_tecView.m_valuesHours.CopyTo(valuesHours, 0);

                double minimum = double.MaxValue, minimum_scale;
                double maximum = 0, maximum_scale;
                bool noValues = true;
                double summ = 0;
                int index_last_not_null = 0;
                for (int i = 0; i < itemscount+1; i++)
                {
                    if (i != itemscount)
                    {
                        object[] hourValue = new object[2];
                        double val;
                        if (HAdmin.SeasonDateTime.Date == m_tecView.m_curDate.Date)
                        {
                            int offset = m_tecView.GetSeasonHourOffset(i + 1);
                            names[i] = (i + 1 - offset).ToString();
                            if (HAdmin.SeasonDateTime.Hour == i)
                                names[i] += "*";
                            else
                                ;
                        }
                        else
                            names[i] = (i + 1).ToString();

                        valuesTMSNPsum[i] = valuesHours[i].valuesTMSNPsum;
                        hourValue[0] = i + 1;
                        val = Convert.ToDouble(valuesTMSNPsum[i]);
                        summ = summ + val;

                        if (val > 1)
                        {
                            hourValue[1] = val.ToString(@"F2");
                        }
                        else
                            hourValue[1] = 0.ToString(@"F0");

                        dgvSNHour.Rows.Add(hourValue);

                        if (valuesTMSNPsum[i] != 0)
                        {
                            noValues = false;

                            if (minimum > valuesTMSNPsum[i])
                            {
                                minimum = valuesTMSNPsum[i];
                            }
                            else
                                ;

                            if (maximum < valuesTMSNPsum[i])
                                maximum = valuesTMSNPsum[i];
                            else
                                ;
                            index_last_not_null = i;
                        }
                        else
                            ;
                    }
                    else
                    {
                        object[] hourValue = new object[2];
                        hourValue[0] = "Сумма";
                        hourValue[1] = summ.ToString(@"F2");
                        dgvSNHour.Rows.Add(hourValue);
                    }
                }
                dgvSNHour.FirstDisplayedScrollingRowIndex = index_last_not_null;

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

            /// <summary>
            /// Обработчик нажатия на пункт контекстного меню "Отобразить в таблице"
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void отобразитьВТаблицеToolStripMenuItem_Click(object sender, EventArgs e)
            {
                if (((ToolStripMenuItem)sender).Checked == false)//Если не активно то перерисовываем сначала вместе с таблицей
                {
                    this.Controls.Remove(m_zedGraphHours);
                    this.Controls.Add(m_zedGraphHours, 2, COUNT_FIXED_ROWS);
                    this.SetColumnSpan(m_zedGraphHours, this.ColumnCount - 2);
                    this.Controls.Add(dgvSNHour, 0, COUNT_FIXED_ROWS);
                    this.SetColumnSpan(dgvSNHour, 2);
                    ((ToolStripMenuItem)sender).Checked = true;
                    
                }
                else//Если активно то перерисовываем сначала без таблицы
                {
                    this.Controls.Remove(m_zedGraphHours);
                    this.Controls.Remove(dgvSNHour);
                    this.Controls.Add(m_zedGraphHours, 0, COUNT_FIXED_ROWS);
                    this.SetColumnSpan(m_zedGraphHours, this.ColumnCount);
                    ((ToolStripMenuItem)sender).Checked = false;
                }
            }

            /// <summary>
            /// Обработчик события нажатия на кнопку экспорта
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void эксельToolStripMenuItemHours_Click(object sender, EventArgs e)
            {
                lock (m_tecView.m_lockValue)
                {
                    SaveFileDialog sf = new SaveFileDialog();
                    sf.CheckPathExists = true;
                    sf.DefaultExt = ".xls";
                    sf.Filter = "Файл Microsoft Excel (.xls) | *.xls";
                    if (sf.ShowDialog() == DialogResult.OK)
                    {
                        string strSheetName = "Часовые_знач";

                        ExcelFile ef = new ExcelFile();
                        ef.Worksheets.Add(strSheetName);
                        ExcelWorksheet ws = ef.Worksheets[0];
                        if (indx_TECComponent < 0)
                        {
                            ws.Cells[0, 0].Value = "Собственные нужды " + m_tecView.m_tec.name_shr;
                            if (m_tecView.m_tec.list_TECComponents.Count != 1)
                                foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
                                    ws.Cells[0, 0].Value += ", " + g.name_shr;
                        }
                        else
                        {
                            ws.Cells[0, 0].Value = "Собственные нужды " + m_tecView.m_tec.name_shr + ", " + m_tecView.m_tec.list_TECComponents[indx_TECComponent].name_shr;
                        }

                        ws.Cells[1, 0].Value = "Мощность на " + m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN].Text;

                        ws.Cells[2, 0].Value = "Час";
                        ws.Cells[2, 1].Value = "Факт";

                        bool valid;
                        double res_double;
                        int res_int;

                        for (int i = 0; i < dgvSNHour.Rows.Count-1; i++)
                        {
                            valid = int.TryParse(dgvSNHour.Rows[i].Cells[0].Value.ToString(), out res_int);
                            if (valid)
                                ws.Cells[3 + i, 0].Value = res_int;
                            else
                                ws.Cells[3 + i, 0].Value = dgvSNHour.Rows[i].Cells[0].Value;

                            valid = double.TryParse(dgvSNHour.Rows[i].Cells[1].Value.ToString(), out res_double);
                            if (valid)
                                ws.Cells[3 + i, 1].Value = res_double;
                            else
                                ws.Cells[3 + i, 1].Value = dgvSNHour.Rows[i].Cells[1].Value;
                        }

                        int tryes = 5;
                        while (tryes > 0)
                        {
                            try
                            {
                                ef.SaveXls(sf.FileName);
                                break;
                            }
                            catch
                            {
                                FileInfo fi = new FileInfo(sf.FileName);
                                sf.FileName = fi.DirectoryName + "\\Copy " + fi.Name;
                            }
                            tryes--;
                            if (tryes == 0)
                                MessageBox.Show(this, "Не удалось сохранить файл.\nВозможно нет доступа, либо файл занят другим приложением.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        protected class HZedGraphControlSNHours : StatisticCommon.HZedGraphControl
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            /// <param name="obj"></param>
            public HZedGraphControlSNHours(object obj)
                : base(obj)
            {
                InitializeComponent();
            }

            /// <summary>
            /// Инициализация компонентов
            /// </summary>
            private void InitializeComponent()
            {
                this.Dock = System.Windows.Forms.DockStyle.Fill;
                //this.m_zedGraphHours.Location = arPlacement[(int)CONTROLS.m_zedGraphHours].pt;
                this.Name = "zedGraphSNHour";
                this.ScrollGrace = 0;
                this.ScrollMaxX = 0;
                this.ScrollMaxY = 0;
                this.ScrollMaxY2 = 0;
                this.ScrollMinX = 0;
                this.ScrollMinY = 0;
                this.ScrollMinY2 = 0;
                //this.m_zedGraphHours.Size = arPlacement[(int)CONTROLS.m_zedGraphHours].sz;
                this.TabIndex = 0;
                this.IsEnableHEdit = false;
                this.IsEnableHPan = false;
                this.IsEnableHZoom = false;
                this.IsEnableSelection = false;
                this.IsEnableVEdit = false;
                this.IsEnableVPan = false;
                this.IsEnableVZoom = false;
                this.IsShowPointValues = true;
            }
        }
    }
}
