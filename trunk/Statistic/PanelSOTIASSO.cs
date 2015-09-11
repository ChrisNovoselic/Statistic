using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel; //IContainer
using System.Threading; //ManualResetEvent
using System.Drawing; //Color

using ZedGraph;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    /// <summary>
    /// Панель для отображения значений СОТИАССО (телеметрия)
    ///  для контроля
    /// </summary>
    public class PanelSOTIASSO : PanelStatistic
    {
        private enum KEY_CONTROLS { UNKNOWN = -1
            , DTP_CUR_DATE_HOUR
            /*, LABEL_CUR_TIME*/, BTN_SET_NOWHOUR
            , CB_GTP, LABEL_GTP_KOEFF
            , DGV_GTP_VALUE, ZGRAPH_GTP
            , CLB_TG
            , DGV_TG_VALUE, ZGRAPH_TG
            , COUNT_KEY_CONTROLS }

        private HMark m_markQueries;
        private TecView m_tecView;

        System.Windows.Forms.SplitContainer stctrMain
            , stctrView;
        ZedGraph.ZedGraphControl m_zGraph_GTP
            , m_zGraph_TG;
        List <StatisticCommon.TEC> m_listTEC;

        private ManualResetEvent m_evTimerCurrent;
        private System.Threading.Timer m_timerCurrent;

        private event DelegateObjectFunc EvtValuesMins;

        private List <int> m_listIndexTGAdvised;

        private event DelegateDateFunc EvtSetDatetimeHour;
        private DelegateDateFunc delegateSetDatetimeHour;

        /// <summary>
        /// Панель для активных элементов управления
        /// </summary>
        private PanelManagement m_panelManagement;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public PanelSOTIASSO(List<StatisticCommon.TEC> listTec, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr)
            : base()
        {
            m_listTEC = listTec;
            //Создать объект с признаками обработки тех типов значений
            // , которые будут использоваться фактически
            m_markQueries = new HMark();
            m_markQueries.Marked((int)CONN_SETT_TYPE.ADMIN); //Для получения даты/времени
            m_markQueries.Marked((int)CONN_SETT_TYPE.PBR); //Для получения даты/времени
            m_markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO);
            //Создать объект обработки запросов - установить первоначальные индексы для ТЭЦ, компонента
            m_tecView = new TecView(TecView.TYPE_PANEL.SOTIASSO, 0, 0);
            //Инициализировать список ТЭЦ для 'TecView' - указать ТЭЦ в соответствии с указанным ранее индексом (0)
            m_tecView.InitTEC(new List<StatisticCommon.TEC>() { m_listTEC[0] }, m_markQueries);
            m_tecView.SetDelegateReport(fErrRep, fWarRep, fActRep, fRepClr);
            //Установить тип значений
            m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] = CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN;
            m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.HOURS] = CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN;
            //Делегат для установки текущего времени на панели 'PanelManagement'
            //m_tecView.SetDelegateDatetime(...);
            //Делегат по окончанию обработки всех состояний 'TecView::ChangeState_SOTIASSO'
            m_tecView.updateGUI_Fact = new IntDelegateIntIntFunc(onEvtHandlerStatesCompleted);

            m_listIndexTGAdvised = new List<int> ();

            //Создать, разместить дочерние элементы управления
            initializeComponent ();
            //Назначить обработчики события - создание дескриптора панели
            this.HandleCreated += new EventHandler(OnHandleCreated);
            // сообщить дочернему элементу, что дескриптор родительской панели создан
            this.HandleCreated += new EventHandler(m_panelManagement.Parent_OnHandleCreated);

            EvtSetDatetimeHour += new DelegateDateFunc(m_panelManagement.Parent_OnEvtSetDatetimeHour);
            delegateSetDatetimeHour = new DelegateDateFunc (setDatetimeHour);

            this.m_zGraph_GTP.MouseUpEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zedGraphMins_MouseUpEvent);
        }
        /// <summary>
        /// Конструктор - вспомогательный (с параметрами)
        /// </summary>
        /// <param name="container">Владелец текущего объекта</param>
        public PanelSOTIASSO(IContainer container, List<StatisticCommon.TEC> listTec, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr)
            : this(listTec, fErrRep, fWarRep, fActRep, fRepClr)
        {
            container.Add (this);
        }
        ///// <summary>
        ///// Деструктор
        ///// </summary>
        //~PanelSOTIASSO ()
        //{
        //    m_tecView = null;
        //}
        /// <summary>
        /// Инициализация панели с установкой кол-ва столбцов, строк
        /// </summary>
        /// <param name="cols">Количество столбцов</param>
        /// <param name="rows">Количество строк</param>
        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Инициализация и размещение собственных элементов управления
        /// </summary>
        private void initializeComponent ()
        {
            //Создать дочерние элементы управления
            m_panelManagement = new PanelManagement (); // панель для размещения элементов управления
            m_panelManagement.EvtDatetimeHourChanged += new DelegateDateFunc(panelManagement_OnEvtDatetimeHourChanged);
            m_panelManagement.EvtGTPSelectionIndexChanged += new DelegateIntFunc(panelManagement_OnEvtGTPSelectionIndexChanged);
            m_panelManagement.EvtTGItemChecked += new DelegateIntFunc(panelManagement_OnEvtTGItemChecked);
            //m_panelManagement.EvtSetNowHour += new DelegateFunc(panelManagement_OnEvtSetNowHour);
            m_zGraph_GTP = new HZEdGraphControlGTP(); // графическая панель для отображения значений ГТП
            m_zGraph_GTP.Name = KEY_CONTROLS.ZGRAPH_GTP.ToString ();
            m_zGraph_TG = new HZEdGraphControlTG(); // графическая панель для отображения значений ТГ
            m_zGraph_TG.Name = KEY_CONTROLS.ZGRAPH_TG.ToString();

            //Создать сплиттеры
            stctrMain = new SplitContainer (); // для главного контейнера (вертикальный)
            stctrView = new SplitContainer(); // для вспомогательного (2 графические панели) контейнера (горизонтальный)
            //Настроить размещение главного контейнера
            stctrMain.Dock = DockStyle.Fill;
            stctrMain.Orientation = Orientation.Vertical;
            //Настроить размещение вспомогательного контейнера
            stctrView.Dock = DockStyle.Fill;
            stctrView.Orientation = Orientation.Horizontal;
            //Настроить размещение графических панелей
            m_zGraph_GTP.Dock = DockStyle.Fill;
            m_zGraph_TG.Dock = DockStyle.Fill;

            //Приостановить прорисовку текущей панели
            this.SuspendLayout ();

            //Добавить во вспомогательный контейнер графические панели
            stctrView.Panel1.Controls.Add(m_zGraph_GTP);
            stctrView.Panel2.Controls.Add(m_zGraph_TG);
            //Добавить элементы управления к главному контейнеру
            stctrMain.Panel1.Controls.Add(m_panelManagement);
            stctrMain.Panel2.Controls.Add(stctrView);

            //stctrMain.FixedPanel = FixedPanel.Panel1;
            stctrMain.SplitterDistance = 43;

            //Добавить к текущей панели единственный дочерний (прямой) элемент управления - главный контейнер-сплиттер
            this.Controls.Add(stctrMain);
            //Возобновить прорисовку текущей панели
            this.ResumeLayout (false);
            //Принудительное применение логики макета
            this.PerformLayout();
        }
        /// <summary>
        /// Класс для размещения активных элементов управления
        /// </summary>
        private class PanelManagement : HPanelCommon
        {
            /// <summary>
            /// Событие изменения текущих даты, номера часа
            /// </summary>
            public event DelegateDateFunc EvtDatetimeHourChanged;
            /// <summary>
            /// Событие изменения текущего индекса ГТП
            /// </summary>
            public event DelegateIntFunc EvtGTPSelectionIndexChanged;
            /// <summary>
            /// Событие изменения перечня ТГ для отображения выбранного ГТП
            /// </summary>
            public event DelegateIntFunc EvtTGItemChecked;

            //public event DelegateFunc EvtSetNowHour;
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public PanelManagement() : base (4, 24)
            {
                //Инициализировать равномерные высоту/ширину столбцов/строк
                initializeLayoutStyleEvenly ();

                initializeComponent ();                
            }
            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец объекта</param>
            public PanelManagement(IContainer container)
                : this()
            {
                container.Add(this);
            }
            /// <summary>
            /// Инициализация панели с установкой кол-ва столбцов, строк
            /// </summary>
            /// <param name="cols">Количество столбцов</param>
            /// <param name="rows">Количество строк</param>
            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                throw new System.NotImplementedException();
            }
            /// <summary>
            /// Инициализация, размещения собственных элементов управления
            /// </summary>
            private void initializeComponent()
            {
                Control ctrl = null;

                //Приостановить прорисовку текущей панели
                // ??? корректней приостановить прорисовку после создания всех дочерних элементов
                // ??? при этом потребуется объявить переменные для каждого из элементов управления
                this.SuspendLayout();

                //Создать дочерние элементы управления
                // календарь для установки текущих даты, номера часа
                ctrl = new DateTimePicker();
                ctrl.Name = KEY_CONTROLS.DTP_CUR_DATE_HOUR.ToString();
                (ctrl as DateTimePicker).DropDownAlign = LeftRightAlignment.Right;
                (ctrl as DateTimePicker).Format = DateTimePickerFormat.Custom;
                (ctrl as DateTimePicker).CustomFormat = @"HH-й час, dd MMMM, yyyy";
                //(ctrl as DateTimePicker).Value = ((ctrl as DateTimePicker).Value - HAdmin.GetUTCOffsetOfMoscowTimeZone()).AddHours (1);
                (ctrl as DateTimePicker).ValueChanged += new EventHandler(onDatetimeHour_ValueChanged);
                ctrl.Dock = DockStyle.Fill;
                //Добавить к текущей панели календарь
                this.Controls.Add(ctrl, 0, 0);
                this.SetColumnSpan(ctrl, 2); this.SetRowSpan(ctrl, 1);

                //// подпись текущего времени
                //ctrl = new System.Windows.Forms.Label ();
                //(ctrl as System.Windows.Forms.Label).BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                //(ctrl as System.Windows.Forms.Label).TextAlign = ContentAlignment.MiddleCenter;
                //(ctrl as System.Windows.Forms.Label).Text = @"--:--:--";
                //ctrl.Name = KEY_CONTROLS.LABEL_CUR_TIME.ToString ();
                //ctrl.Dock = DockStyle.Fill;
                ////Добавить к текущей панели подпись
                //this.Controls.Add(ctrl, 0, 1);
                //this.SetColumnSpan(ctrl, 2); this.SetRowSpan(ctrl, 1);
                // кнопка перехода к актуальному часу
                ctrl = new System.Windows.Forms.Button();
                ctrl.Name = KEY_CONTROLS.BTN_SET_NOWHOUR.ToString();
                ctrl.Text = @"Тек./час";
                (ctrl as Button).Click += new EventHandler(onSetNowHour_Click);
                ctrl.Dock = DockStyle.Fill;
                //Добавить к текущей панели кнопку
                this.Controls.Add(ctrl, 2, 0);
                this.SetColumnSpan(ctrl, 2); this.SetRowSpan(ctrl, 1);

                // раскрывающийся список для выбора ГТП
                ctrl = new ComboBox ();
                ctrl.Name = KEY_CONTROLS.CB_GTP.ToString ();
                (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                (ctrl as ComboBox).SelectedIndexChanged += new EventHandler (onGTP_SelectionIndexChanged);
                ctrl.Dock = DockStyle.Fill;
                //Добавить к текущей панели список ГТП
                this.Controls.Add(ctrl, 0, 1);
                this.SetColumnSpan(ctrl, 2); this.SetRowSpan(ctrl, 1);

                // коэффициент для текущего ГТП
                ctrl = new System.Windows.Forms.Label();
                ctrl.Name = KEY_CONTROLS.LABEL_GTP_KOEFF.ToString ();
                (ctrl as System.Windows.Forms.Label).BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                (ctrl as System.Windows.Forms.Label).TextAlign = ContentAlignment.MiddleCenter;
                ctrl.Text = @"Коэфф-т: -1";
                //ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Top);
                ctrl.Dock = DockStyle.Fill;
                this.Controls.Add(ctrl, 2, 1);
                this.SetColumnSpan(ctrl, 2); this.SetRowSpan(ctrl, 1);

                // таблица для отображения значений ГТП
                ctrl = new PanelSOTIASSO.DataGridViewGTP();
                ctrl.Name = KEY_CONTROLS.DGV_GTP_VALUE.ToString();
                ctrl.Dock = DockStyle.Fill;
                //ctrl.Anchor = (AnchorStyles)((AnchorStyles.Left | AnchorStyles.Top) | AnchorStyles.Right);
                //ctrl.Height = 240; // RowSpan = ...                
                //Добавить к текущей панели таблицу значений ГТП
                this.Controls.Add(ctrl, 0, 2);
                this.SetColumnSpan(ctrl, 4); this.SetRowSpan(ctrl, 9);

                // разделительная линия
                ctrl = new GroupBox ();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                ctrl.Height = 3;
                //Добавить к текущей панели раздел./линию
                this.Controls.Add(ctrl, 0, 11);
                this.SetColumnSpan(ctrl, 4); this.SetRowSpan(ctrl, 1);
                
                // список для выбора ТГ
                ctrl = new CheckedListBox();
                ctrl.Name = KEY_CONTROLS.CLB_TG.ToString ();
                (ctrl as CheckedListBox).CheckOnClick = true;
                (ctrl as CheckedListBox).ItemCheck += new ItemCheckEventHandler(onTG_ItemCheck);
                ctrl.Dock = DockStyle.Fill;
                //Добавить к текущей панели список для выбора ТГ
                this.Controls.Add(ctrl, 0, 12);
                this.SetColumnSpan(ctrl, 4); this.SetRowSpan(ctrl, 3);
                
                // таблица для отображения значений ГТП
                ctrl = new PanelSOTIASSO.DataGridViewTG();
                ctrl.Name = KEY_CONTROLS.DGV_TG_VALUE.ToString();
                ctrl.Dock = DockStyle.Fill;
                //ctrl.Anchor = (AnchorStyles)((AnchorStyles.Left | AnchorStyles.Top) | AnchorStyles.Right);
                //ctrl.Height = 240; // RowSpan = ...                
                //Добавить к текущей панели таблицу значений ГТП
                this.Controls.Add(ctrl, 0, 15);
                this.SetColumnSpan(ctrl, 4); this.SetRowSpan(ctrl, 9);

                ////Приостановить прорисовку текущей панели
                //this.SuspendLayout();

                //Возобновить прорисовку текущей панели
                this.ResumeLayout(false);
                //Принудительное применение логики макета
                this.PerformLayout();
            }
            ///// <summary>
            ///// Присвоить исходные дату/номер часа
            ///// </summary>
            //private void initDatetimeHourValue ()
            //{
            //    DateTimePicker dtpCurDatetimeHour = this.Controls.Find(KEY_CONTROLS.DTP_CUR_DATE_HOUR.ToString(), true)[0] as DateTimePicker;
            //    DateTime curDatetimeHour = dtpCurDatetimeHour.Value;
            //    curDatetimeHour = curDatetimeHour.AddMilliseconds(-1 * (curDatetimeHour.Minute * 60 * 1000 + curDatetimeHour.Second * 1000 + curDatetimeHour.Millisecond));
            //    dtpCurDatetimeHour.Value = curDatetimeHour;
            //}
            /// <summary>
            /// Изменить дату/номер часа
            /// </summary>
            private void initDatetimeHourValue(DateTime dtVal)
            {
                DateTimePicker dtpCurDatetimeHour = this.Controls.Find(KEY_CONTROLS.DTP_CUR_DATE_HOUR.ToString(), true)[0] as DateTimePicker;
                DateTime curDatetimeHour;
                curDatetimeHour = dtVal.AddMilliseconds(-1 * (dtVal.Minute * 60 * 1000 + dtVal.Second * 1000 + dtVal.Millisecond));
                dtpCurDatetimeHour.Value = curDatetimeHour;
            }
            /// <summary>
            /// Обработчик события - дескриптор элемента управления создан
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            public void Parent_OnHandleCreated (object obj, EventArgs ev)
            {
                initDatetimeHourValue((DateTime.Now - HAdmin.GetUTCOffsetOfMoscowTimeZone())/*.AddHours(1)*/);
            }

            public void InitializeGTPList (List <string> listGTPNameShr)
            {
                ComboBox cbxGTP = (this.Controls.Find (KEY_CONTROLS.CB_GTP.ToString (), true))[0] as ComboBox;

                cbxGTP.Items.AddRange (listGTPNameShr.ToArray ());

                if (cbxGTP.Items.Count > 0)
                    cbxGTP.SelectedIndex = 0;
                else
                    ;
            }

            public void InitializeTGList(List<string> listTGNameShr)
            {
                CheckedListBox clbTG = (this.Controls.Find(KEY_CONTROLS.CLB_TG.ToString(), true))[0] as CheckedListBox;

                clbTG.Items.Clear ();
                clbTG.Items.AddRange(listTGNameShr.ToArray());
            }

            private void onDatetimeHour_ValueChanged(object obj, EventArgs ev)
            {
                //EvtDatetimeHourChanged (((this.Controls.Find(KEY_CONTROLS.CUR_DATETIME_HOUR.ToString(), true))[0] as DateTimePicker).Value);
                EvtDatetimeHourChanged((obj as DateTimePicker).Value);
            }

            public void Parent_OnEvtSetDatetimeHour (DateTime dtVal)
            {
                initDatetimeHourValue(dtVal/*.AddHours (1)*/);
            }

            /// <summary>
            /// Обработчик события - изменение выбранного элемента 'ComboBox' - текущий ГТП
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            private void onGTP_SelectionIndexChanged(object obj, EventArgs ev)
            {
                EvtGTPSelectionIndexChanged (((this.Controls.Find(KEY_CONTROLS.CB_GTP.ToString(), true))[0] as ComboBox).SelectedIndex);
            }

            private void onTG_ItemCheck (object obj, ItemCheckEventArgs ev)
            {
                EvtTGItemChecked (ev.Index);
            }

            private void onSetNowHour_Click(object obj, EventArgs ev)
            {
                //EvtSetNowHour();
                initDatetimeHourValue((DateTime.Now - HAdmin.GetUTCOffsetOfMoscowTimeZone())/*.AddHours(1)*/);
            }
            /// <summary>
            /// Обработчик события - отобразить полученные значения
            /// </summary>
            /// <param name="valuesMins">Массив значений для отображения</param>
            public void Parent_OnEvtValuesMins(object obj)
            {
                if (IsHandleCreated == true)
                    if (InvokeRequired == true)
                        this.BeginInvoke(new DelegateObjectFunc  (onEvtValuesMins), new object [] { obj });
                    else
                        onEvtValuesMins(obj);
                else
                    ;
            }

            private void onEvtValuesMins(object obj)
            {
                TecView.valuesTEC [] valuesMins = obj as TecView.valuesTEC [];
                DataGridViewGTP dgvGTP = this.Controls.Find (KEY_CONTROLS.DGV_GTP_VALUE.ToString (), true)[0] as DataGridViewGTP;

                for (int i = 1; i < valuesMins.Length; i++)
                    dgvGTP.Rows[i - 1].Cells[1].Value = valuesMins[i].valuesFact.ToString (@"F3");
                                        
            }
        }

        private class HZEdGraphControl : ZedGraph.ZedGraphControl
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public HZEdGraphControl () : base ()
            {
                initializeComponent ();
            }
            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец объекта</param>
            public HZEdGraphControl(IContainer container)
                : this()
            {
                container.Add (this);
            }

            private void initializeComponent ()
            {
                this.ScrollGrace = 0;
                this.ScrollMaxX = 0;
                this.ScrollMaxY = 0;
                this.ScrollMaxY2 = 0;
                this.ScrollMinX = 0;
                this.ScrollMinY = 0;
                this.ScrollMinY2 = 0;
                //this.Size = arPlacement[(int)CONTROLS.zedGraphMins].sz;
                this.TabIndex = 0;
                this.IsEnableHEdit = false;
                this.IsEnableHPan = false;
                this.IsEnableHZoom = false;
                this.IsEnableSelection = false;
                this.IsEnableVEdit = false;
                this.IsEnableVPan = false;
                this.IsEnableVZoom = false;
                this.IsShowPointValues = true;

                this.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.onPointValueEvent);
                this.DoubleClickEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.onDoubleClickEvent);
            }

            private string onPointValueEvent(object sender, GraphPane pane, CurveItem curve, int iPt)
            {
                return curve[iPt].Y.ToString("F2");
            }

            private bool onDoubleClickEvent(ZedGraphControl sender, MouseEventArgs e)
            {
                FormMain.formGraphicsSettings.SetScale();

                return true;
            }
        }
        /// <summary>
        /// Класс для отображения в графическом представлении
        ///  значений за укзанный (дата/номер часа) 1 час для выбранного ГТП
        /// </summary>
        private class HZEdGraphControlGTP : HZEdGraphControl
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public HZEdGraphControlGTP () : base ()
            {
                initializeComponent ();
            }
            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец объекта</param>
            public HZEdGraphControlGTP(IContainer container)
                : this()
            {
                container.Add (this);
            }

            private void initializeComponent ()
            {
            }

            ///// <summary>
            ///// Обработчик события - отобразить полученные значения
            ///// </summary>
            ///// <param name="obj">Массив значений для отображения</param>
            //public void Parent_OnEvtValuesMins(object obj)
            //{
            //    if (IsHandleCreated == true)
            //        if (InvokeRequired == true)
            //            this.BeginInvoke(new DelegateObjectFunc(onEvtValuesMins), new object[] { obj });
            //        else
            //            onEvtValuesMins(obj);
            //    else
            //        ;
            //}
            ///// <summary>
            ///// Делегат для отображения значений
            ///// </summary>
            ///// <param name="obj">Массив значений для отображения</param>
            //private void onEvtValuesMins(object obj)
            //{
            //}
        }
        /// <summary>
        /// Класс для отображения в графическом представлении
        ///  значений за указанную (дата/номер часа/номер минуты) 1 мин для выбранных ТГ, выбранного ГТП
        /// </summary>
        private class HZEdGraphControlTG : HZEdGraphControl
        {
        }

        private class DataGridViewGTP : DataGridView
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public DataGridViewGTP () : base ()
            {
                initializeComponent ();
            }
            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец текущего объекта</param>
            public DataGridViewGTP(IContainer container)
                : this()
            {
                container.Add (this);
            }

            private void initializeComponent ()
            {
                this.Columns.AddRange (new DataGridViewColumn [] {
                        new DataGridViewTextBoxColumn ()
                        , new DataGridViewTextBoxColumn ()
                        , new DataGridViewTextBoxColumn ()
                    });

                this.Columns[0].HeaderText = @"Мин.";
                this.Columns[1].HeaderText = @"Значение";
                this.Columns[2].HeaderText = @"Отклонение";

                this.ReadOnly = true;
                //this.ColumnHeadersVisible =
                this.RowHeadersVisible =
                this.AllowUserToAddRows =
                this.AllowUserToDeleteRows =
                this.AllowUserToOrderColumns =
                this.AllowUserToResizeRows =
                    false;
                this.MultiSelect = false;
                this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                this.Columns[0].Width = 38;
                this.Columns[0].Frozen = true;
                this.Columns[1].Frozen = true;
                this.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                //Добавить строки по числу мин. в часе
                for (int i = 0; i < 60; i ++)
                    this.Rows.Add (new object [] { i + 1 });
            }
        }

        private class DataGridViewTG : DataGridView
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public DataGridViewTG () : base ()
            {
                initializeComponent ();
            }
            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец текущего объекта</param>
            public DataGridViewTG(IContainer container)
                : this()
            {
                container.Add (this);
            }

            private void initializeComponent ()
            {
                this.Columns.AddRange(new DataGridViewColumn[] {
                        new DataGridViewTextBoxColumn ()
                    });

                this.Columns[0].HeaderText = @"Сек.";

                this.ReadOnly = true;
                //this.ColumnHeadersVisible =
                this.RowHeadersVisible =
                this.AllowUserToAddRows =
                this.AllowUserToDeleteRows =
                this.AllowUserToOrderColumns =
                this.AllowUserToResizeRows =
                    false;
                this.MultiSelect = false;
                this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                this.Columns[0].Width = 38;

                //Добавить строки по числу сек. в мин.
                for (int i = 0; i < 60; i++)
                    this.Rows.Add(new object[] { i });
            }
        }
        /// <summary>
        /// Переопределение наследуемой функции - запуск объекта
        /// </summary>
        public override void Start()
        {
            base.Start ();

            m_tecView.Start ();

            m_evTimerCurrent = new ManualResetEvent(true);
            m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, PanelStatistic.POOL_TIME * 1000 - 1, System.Threading.Timeout.Infinite);
        }
        /// <summary>
        /// Переопределение наследуемой функции - останов объекта
        /// </summary>
        public override void Stop()
        {
            //Проверить актуальность объекта обработки запросов
            if (! (m_tecView == null))
            {
                if (m_tecView.Actived == true)
                    //Если активен - деактивировать
                    m_tecView.Activate(false);
                else
                    ;

                if (m_tecView.IsStarted == true)
                    //Если выполняется - остановить
                    m_tecView.Stop();
                else
                    ;

                //m_tecView = null;
            }
            else
                ;
            //Проверить актуальность объекта синхронизации таймера
            if (!(m_evTimerCurrent == null))
                //Сбросить флаг ожидания
                m_evTimerCurrent.Reset();
            else
                ;
            //Проверить актуальность объекта таймера
            if (! (m_timerCurrent ==null))
            {
                //Освободить ресурсы таймера
                m_timerCurrent.Dispose();

                m_timerCurrent = null;
            }
            else
                ;
            //Остановить базовый объект
            base.Stop ();
        }
        /// <summary>
        /// Переопределение наследуемой функции - активация/деактивация объекта
        /// </summary>
        public override bool Activate(bool active)
        {
            bool bRes = false;
            int dueTime = System.Threading.Timeout.Infinite;

            bRes = base.Activate(active);

            m_tecView.Activate(active);

            if (m_tecView.Actived == true)
            {
                dueTime = 0;
            }
            else
            {
                m_tecView.ReportClear(true);
            }

            if (! (m_timerCurrent == null))
                m_timerCurrent.Change(dueTime, System.Threading.Timeout.Infinite);
            else
                ;

            return bRes;
        }
        /// <summary>
        /// Обработчик события - создание дескриптора панели
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>        
        private void OnHandleCreated (object obj, EventArgs ev)
        {
            //Список строк - наименований ГТП
            // для передачи дочерней панели на отображение
            List <string> listGTPNameShr = new List<string> ();
            //Сформировать список строк - наименований ГТП
            foreach (TEC t in m_listTEC)
                foreach (TECComponent tc in t.list_TECComponents)
                    if ((tc.m_id > 100) && (tc.m_id < 500))
                        //Наименование ТЭЦ + наименование ГТП
                        listGTPNameShr.Add(t.name_shr + @" " + tc.name_shr);
                    else
                        ;
            //Добавить строки на дочернюю панель
            m_panelManagement.InitializeGTPList(listGTPNameShr);

            EvtValuesMins += new DelegateObjectFunc(m_panelManagement.Parent_OnEvtValuesMins);
            //EvtValuesMins += new DelegateObjectFunc((m_zGraph_GTP as HZEdGraph_GTP).Parent_OnEvtValuesMins); //???отображать значения будем в функции на панели
        }

        private void setDatetimeHour(DateTime val)
        {
            EvtSetDatetimeHour (val);
        }
        /// <summary>
        /// Метод обратного вызова для таймера 'm_timerCurrent'
        /// </summary>
        /// <param name="obj">Параметр при вызове метода</param>
        private void TimerCurrent_Tick (object obj)
        {
            if (m_tecView.Actived == true)
            {
                if (m_tecView.currHour == true)
                {
                    if ((m_tecView.adminValuesReceived == true) //Признак успешного выполнения операций для состояния 'TecView.AdminValues'
                        && ((m_tecView.lastMin > 60) && (m_tecView.serverTime.Minute > 1)))
                    {
                        if (IsHandleCreated/*InvokeRequired*/ == true)
                            Invoke(delegateSetDatetimeHour, m_tecView.serverTime);
                        else
                            return;
                    }
                    else
                    {
                        m_tecView.ChangeState();

                        m_timerCurrent.Change(PanelStatistic.POOL_TIME * 1000 - 1, System.Threading.Timeout.Infinite);
                    }
                }
                else
                    ; //m_tecView.ChangeState();
            }
            else
                ;
        }
        /// <summary>
        /// Обработчик события - изменения даты/номера часа на панели с управляющими элементами
        /// </summary>
        /// <param name="dtNew">Новые дата/номер часа</param>
        private void panelManagement_OnEvtDatetimeHourChanged(DateTime dtNew)
        {
            m_tecView.m_curDate = dtNew;
            m_tecView.lastHour =
                dtNew.Hour// - 1; //- (int)HAdmin.GetUTCOffsetOfMoscowTimeZone().TotalHours //- 3
                ;
            if (m_tecView.lastHour < 0)
            {
                m_tecView.m_curDate = m_tecView.m_curDate.AddDays(-1);
                m_tecView.lastHour += 24;
            }
            else
                ;

            if (m_tecView.serverTime.Equals(DateTime.MinValue) == false)
                if ((m_tecView.m_curDate.Date.Equals (m_tecView.serverTime.Date) == true)
                    && (m_tecView.lastHour.Equals (m_tecView.serverTime.Hour) == true))
                {
                    m_tecView.adminValuesReceived = false; //Чтобы не выполнилась ветвь - переход к след./часу
                    m_tecView.currHour = true;

                    if (! (m_timerCurrent == null))
                        m_timerCurrent.Change(0, System.Threading.Timeout.Infinite);
                    else
                        ;
                }
                else
                {
                    m_tecView.currHour = false;

                    m_tecView.ChangeState ();
                }
            else
                ;
        }
        /// <summary>
        /// Обработчик события - выбор компонента ТЭЦ (ГТП) на панели с управляющими элементами
        /// </summary>
        /// <param name="indx"></param>
        private void panelManagement_OnEvtGTPSelectionIndexChanged (int indx)
        {
            //Передать информацию 'PanelManagement' для заполнения списка ТГ
            List <string> listTGNameShr = new List<string> ();
            int indxTEC = -1 //Индекс ТЭЦ в списке из БД конфигурации
                , indxGTP = -1 //Индекс ГТП сквозной
                , indxTECComponent = -1 //Индекс компонента ТЭЦ (ГТП) - локальный в пределах ТЭЦ
                ;

            indxTEC =
            indxGTP =
                0;
            foreach (TEC t in m_listTEC)
            {
                //В каждой ТЭЦ индекс локальный - обнулить
                indxTECComponent = 0;
                //Цикл для поиска выбранного пользователем компонента ТЭЦ (ГТП)
                // заполнения списка наименований подчиненных (ТГ) элементов
                foreach (TECComponent tc in t.list_TECComponents)
                {
                    //Определить тип компонента (по диапазону идентификатора)
                    if ((tc.m_id > 100) && (tc.m_id < 500))
                    {//Только ГТП
                        if (indxGTP == indx)
                        {
                            foreach (TG tg in tc.m_listTG)
                                listTGNameShr.Add(/*tc.name_shr + @" " + */tg.name_shr);

                            indxGTP = -1; //Признак завершения внешнего цикла
                            break;
                        }
                        else
                            ;
                        //Увеличить индекс ГТП сквозной
                        indxGTP++;                        
                    }
                    else
                        ; // не ГТП

                    indxTECComponent++;
                }
                //Проверить признак прекращения выполнения цикла
                if (indxGTP < 0)
                {
                    indxGTP = indx; //Возвратить найденное значение
                    // прекратить выполнение цикла
                    break;
                }
                else
                    ;

                indxTEC ++;
            }
            //Инициализировать элементами список с наименованиями ТГ
            m_panelManagement.InitializeTGList(listTGNameShr);
            //Очистить список с отмеченными ТГ для отображения
            m_listIndexTGAdvised.Clear ();
            //Проверить актуальность объекта обработки запросов
            if (! (m_tecView == null))
                //Проверить наличие изменений при новом выборе компонента ТЭЦ
                if ((!(m_tecView.m_indx_TEC == indxTEC))
                    || (!(m_tecView.indxTECComponents == indxTECComponent)))
                {//Только, если есть изменения
                    //Деактивация/останов объекта обработки запросов
                    m_tecView.Activate (false);
                    m_tecView.Stop ();

                    //m_tecView = null;
                    
                    //Инициализация объекта обработки запросов еовым компонентом
                    m_tecView.InitTEC(m_listTEC[indxTEC], indxTECComponent, m_markQueries);
                    //Запуск/активация объекта обработки запросов
                    m_tecView.Start();
                    m_tecView.Activate(true);

                    //???при 1-й активации некорректно повторный вызов
                    if (! (m_timerCurrent == null))
                        m_timerCurrent.Change(0, System.Threading.Timeout.Infinite);
                    else
                        ;
                }
                else
                    ;
            else
                ;
        }

        private void panelManagement_OnEvtTGItemChecked (int indx)
        {
            if (m_listIndexTGAdvised.IndexOf (indx) < 0)
                m_listIndexTGAdvised.Add (indx);
            else
                m_listIndexTGAdvised.Remove (indx);

            drawGraphMinDetail ();
        }

        //private void panelManagement_OnEvtSetNowHour ()
        //{
        //    //m_tecView.currHour = true;
        //}
        /// <summary>
        /// Обработчик события - все состояния 'ChangeState_SOTIASSO' обработаны
        /// </summary>
        /// <param name="hour">Номер часа в запросе</param>
        /// <param name="min">Номер минуты в звпросе</param>
        /// <returns>Признак результата выполнения функции</returns>
        private int onEvtHandlerStatesCompleted (int hour, int min)
        {
            int iRes = 0;

            //string msg = @"PanelSOTIASSO::updateGUI_Fact () - lastHour=" + hour + @", lastMin=" + min + @" ...";
            //Console.WriteLine (msg);
            //Logging.Logg ().Debug (msg, Logging.INDEX_MESSAGE.NOT_SET);

            EvtValuesMins (m_tecView.m_valuesMins);

            drawGraphMins ();
            drawGraphMinDetail();

            return iRes;
        }

        private void getColorZEDGraph(out Color colChart, out Color colP)
        {
            //Значения по умолчанию
            colChart = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.BG_SOTIASSO);
            colP = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.SOTIASSO);
        }

        private void drawGraphMins ()
        {
            double [] valsMins = null
                , valsUDGe = null
                , valsOAlarm = null
                , valsPAlarm = null;
            int itemscount = -1;
            string[] names = null;
            double minimum
                , minimum_scale
                , maximum
                , maximum_scale;
            bool noValues = false;

            itemscount = m_tecView.m_valuesMins.Length - 1;

            names = new string[itemscount];

            valsMins = new double[itemscount];
            valsUDGe = new double[itemscount];
            valsOAlarm = new double[itemscount];
            valsPAlarm = new double[itemscount];

            minimum = double.MaxValue;
            maximum = 0;
            noValues = true;

            for (int i = 0; i < itemscount; i++)
            {
                names[i] = (i + 1).ToString();

                valsMins[i] = m_tecView.m_valuesMins[i + 1].valuesFact;
                valsPAlarm[i] = m_tecView.m_valuesMins[i + 1].valuesUDGe + m_tecView.m_valuesMins[i + 1].valuesDiviation;
                valsOAlarm[i] = m_tecView.m_valuesMins[i + 1].valuesUDGe - m_tecView.m_valuesMins[i + 1].valuesDiviation;
                valsUDGe[i] = m_tecView.m_valuesMins[i + 1].valuesUDGe;                

                if ((minimum > valsPAlarm[i]) && (!(valsPAlarm[i] == 0)))
                {
                    minimum = valsPAlarm[i];
                    noValues = false;
                }

                if ((minimum > valsOAlarm[i]) && (!(valsOAlarm[i] == 0)))
                {
                    minimum = valsOAlarm[i];
                    noValues = false;
                }

                if ((minimum > valsUDGe[i]) && (!(valsUDGe[i] == 0)))
                {
                    minimum = valsUDGe[i];
                    noValues = false;
                }

                if ((minimum > valsMins[i]) && (!(valsMins[i] == 0)))
                {
                    minimum = valsMins[i];
                    noValues = false;
                }

                if (maximum < valsPAlarm[i])
                    maximum = valsPAlarm[i];
                else
                    ;

                if (maximum < valsOAlarm[i])
                    maximum = valsOAlarm[i];
                else
                    ;

                if (maximum < valsUDGe[i])
                    maximum = valsUDGe[i];
                else
                    ;

                if (maximum < valsMins[i])
                    maximum = valsMins[i];
                else
                    ;
            }

            if (!(FormMain.formGraphicsSettings.scale == true))
                minimum = 0;
            else
                ;

            if (noValues)
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

            Color colorChart = Color.Empty
                , colorPCurve = Color.Empty;
            getColorZEDGraph(out colorChart, out colorPCurve);

            GraphPane pane = m_zGraph_GTP.GraphPane;
            pane.CurveList.Clear();
            pane.Chart.Fill = new Fill(colorChart);

            //LineItem
            pane.AddCurve("УДГэ", null, valsUDGe, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.UDG));
            //LineItem
            pane.AddCurve("", null, valsOAlarm, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.DIVIATION));
            //LineItem
            pane.AddCurve("Граница для сигнализации", null, valsPAlarm, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.DIVIATION));

            if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Bar)
            { 
                pane.AddBar("Мощность", null, valsMins, colorPCurve);
            }
            else
                if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Linear)
                {
                    ////Вариант №1
                    //double[] valuesFactLinear = new double[itemscount];
                    //for (int i = 0; i < itemscount; i++)
                    //    valuesFactLinear[i] = valsMins[i];
                    //Вариант №2
                    PointPairList ppl = new PointPairList ();
                    for (int i = 0; i < itemscount; i++)
                        if (valsMins[i] > 0)
                            ppl.Add(i, valsMins[i]);
                        else
                            ;
                    //LineItem
                    pane.AddCurve("Мощность"
                                    ////Вариант №1
                                    //, null, valuesFactLinear
                                    //Вариант №2
                                    , ppl
                                    , colorPCurve);
                }
                else
                    ;

            //Для размещения в одной позиции ОДНого значения
            pane.BarSettings.Type = BarType.Overlay;

            //...из minutes
            pane.XAxis.Scale.Min = 0.5;
            pane.XAxis.Scale.Max = pane.XAxis.Scale.Min + itemscount;
            pane.XAxis.Scale.MinorStep = 1;
            pane.XAxis.Scale.MajorStep = itemscount / 20;

            pane.XAxis.Type = AxisType.Linear; //...из minutes
            //pane.XAxis.Type = AxisType.Text;
            pane.XAxis.Title.Text = "t, мин";
            pane.YAxis.Title.Text = "P, МВт";
            pane.Title.Text = @"СОТИАССО";
            pane.Title.Text += new string(' ', 29);
            pane.Title.Text += m_tecView.m_curDate.ToShortDateString() + @", "
                //+ ((m_tecView.lastMin > 60) ? m_tecView.m_curDate.Hour : (m_tecView.m_curDate.Hour + 1))
                //+ (m_tecView.m_curDate.Hour + 1)
                + ((m_tecView.lastMin < 60) ? (m_tecView.m_curDate.Hour + 1) : (m_tecView.m_curDate.Hour))
                + @"-й ч";

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

            m_zGraph_GTP.AxisChange ();

            m_zGraph_GTP.Invalidate();
        }

        private void drawGraphMinDetail()
        {
            double[,] valsSecs = null;
            //double[] valsUDGe = null
            //    , valsOAlarm = null
            //    , valsPAlarm = null;
            int tgcount = -1
                , itemscount = -1
                , min = -1;
            string[] names = null;
            double minimum
                , minimum_scale
                , maximum
                , maximum_scale;
            bool noValues = false;

            tgcount = m_tecView.m_localTECComponents.Count;
            itemscount = 60;

            names = new string[itemscount];

            valsSecs = new double[tgcount, itemscount];
            //valsUDGe = new double[itemscount];
            //valsOAlarm = new double[itemscount];
            //valsPAlarm = new double[itemscount];

            minimum = double.MaxValue;
            maximum = 0;
            noValues = true;

            min = m_tecView.lastMin < itemscount ? m_tecView.lastMin : itemscount;

            for (int i = 0; i < itemscount; i++)
            {
                names[i] = (i + 1).ToString();

                for (int j = 0; j < tgcount; j ++)
                {
                    if (! (m_listIndexTGAdvised.IndexOf (j) < 0))
                    {
                        valsSecs[j, i] = m_tecView.m_dictValuesTG[m_tecView.m_localTECComponents[j].m_id].m_powerSeconds[i];
                        if (valsSecs[j, i] < 0)
                            valsSecs[j, i] = 0;
                        else
                            ;

                        if ((minimum > valsSecs[j, i]) && (valsSecs[j, i] > 0))
                        {
                            minimum = valsSecs[j, i];
                            noValues = false;
                        }
                        else
                            ;
 
                        if (maximum < valsSecs[j, i])
                            maximum = valsSecs[j, i];
                        else
                            ;
                    }
                }
            }

            if (!(FormMain.formGraphicsSettings.scale == true))
                minimum = 0;
            else
                ;

            if (noValues)
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

            Color colorChart = Color.Empty
                , colorPCurve = Color.Empty;
            getColorZEDGraph(out colorChart, out colorPCurve);

            GraphPane pane = m_zGraph_TG.GraphPane;
            pane.CurveList.Clear();
            pane.Chart.Fill = new Fill(colorChart);

            ////LineItem
            //pane.AddCurve("УДГэ", null, valsUDGe, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.UDG));
            ////LineItem
            //pane.AddCurve("", null, valsOAlarm, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.DIVIATION));
            ////LineItem
            //pane.AddCurve("Граница для сигнализации", null, valsPAlarm, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.DIVIATION));

            PointPairList[] ppl = new PointPairList[tgcount];
            for (int j = 0; j < tgcount; j ++)
                if (! (m_listIndexTGAdvised.IndexOf (j) < 0))
                {
                    ppl[j] = new PointPairList();

                    for (int i = 0; i < itemscount; i++)
                        if (valsSecs[j, i] > 0)
                            ppl[j].Add(i + 1, valsSecs[j, i]);
                        else
                            ;
                    //LineItem
                    pane.AddCurve(m_tecView.m_localTECComponents[j].name_shr, ppl[j], colorPCurve);
                }
                else
                    ;

            //Для размещения в одной позиции ОДНого значения
            pane.BarSettings.Type = BarType.Overlay;

            //...из minutes
            pane.XAxis.Scale.Min = 0.5;
            pane.XAxis.Scale.Max = pane.XAxis.Scale.Min + itemscount;
            pane.XAxis.Scale.MinorStep = 1;
            pane.XAxis.Scale.MajorStep = itemscount / 20;

            pane.XAxis.Type = AxisType.Linear; //...из minutes
            //pane.XAxis.Type = AxisType.Text;
            pane.XAxis.Title.Text = "t, сек";
            pane.YAxis.Title.Text = "P, МВт";
            pane.Title.Text = @"СОТИАССО";
            pane.Title.Text += new string(' ', 29);
            pane.Title.Text += ((m_tecView.lastMin < 60) ? (m_tecView.m_curDate.Hour + 1) : (m_tecView.m_curDate.Hour)) + @"-й ч"
                + @", " + ((m_tecView.lastMin > 60) ? m_tecView.lastMin - 60 : m_tecView.lastMin) + @"-я мин";

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

            m_zGraph_TG.AxisChange();

            m_zGraph_TG.Invalidate();
        }

        public void UpdateGraphicsCurrent(int type)
        {
            drawGraphMins();
            drawGraphMinDetail();
        }

        private bool zedGraphMins_MouseUpEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return true;

            object obj;
            PointF p = new PointF(e.X, e.Y);
            bool found;
            int index;

            found = sender.GraphPane.FindNearestObject(p, CreateGraphics(), out obj, out index);

            if (!(obj is BarItem) && !(obj is LineItem))
                return true;

            if ((!(m_tecView == null)) && (found == true))
            {
                //if (!(delegateStartWait == null)) delegateStartWait(); else ;

                bool bRetroValues = m_tecView.zedGraphMins_MouseUpEvent(index);

                if (bRetroValues == true)
                    ;
                else
                    (this.Controls.Find (KEY_CONTROLS.BTN_SET_NOWHOUR.ToString (), true)[0] as System.Windows.Forms.Button).PerformClick ();

                //if (!(delegateStopWait == null)) delegateStopWait(); else ;
            }

            return true;
        }
    }
}