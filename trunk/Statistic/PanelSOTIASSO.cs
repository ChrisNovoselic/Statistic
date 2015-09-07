using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel; //IContainer
using System.Threading; //ManualResetEvent

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
            , CUR_DATETIME_HOUR
            , CB_GTP, DGV_GTP_VALUE, TB_GTP_KOEFF, ZGRAPH_GTP
            , CLB_TG, DGV_TG_VALUE, ZGRAPH_TG
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

            m_markQueries = new HMark();
            m_markQueries.Marked((int)CONN_SETT_TYPE.ADMIN); //Для получения даты/времени
            m_markQueries.Marked((int)CONN_SETT_TYPE.PBR); //Для получения даты/времени
            m_markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO);

            m_tecView = new TecView(TecView.TYPE_PANEL.SOTIASSO, 0, 0);

            m_tecView.InitTEC(new List<StatisticCommon.TEC>() { m_listTEC[0] }, m_markQueries);
            m_tecView.SetDelegateReport(fErrRep, fWarRep, fActRep, fRepClr);

            m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] = CONN_SETT_TYPE.DATA_SOTIASSO;
            m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.HOURS] = CONN_SETT_TYPE.DATA_SOTIASSO;

            m_tecView.SetDelegateDatetime(panelManagement_OnEvtDatetimeHourChanged);

            m_tecView.updateGUI_TM_Gen = new DelegateFunc(showTMGenPower);

            initializeComponent ();

            this.HandleCreated += new EventHandler(OnHandleCreated);
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
            Control ctrl = null;
            //Создать дочерние элементы управления
            m_panelManagement = new PanelManagement (); // панель для размещения элементов управления
            m_panelManagement.EvtGTPSelectionIndexChanged += new DelegateIntFunc(panelManagement_OnEvtGTPSelectionIndexChanged);
            m_panelManagement.EvtDatetimeHourChanged += new DelegateDateFunc(panelManagement_OnEvtDatetimeHourChanged);
            m_zGraph_GTP = new HZEdGraph_GTP(); // графическая панель для отображения значений ГТП
            m_zGraph_GTP.Name = KEY_CONTROLS.ZGRAPH_GTP.ToString ();
            m_zGraph_TG = new HZEdGraph_TG(); // графическая панель для отображения значений ТГ
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
            stctrMain.SplitterDistance = 40;

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
            /// Событие изменения текущего индекса ГТП
            /// </summary>
            public event DelegateIntFunc EvtGTPSelectionIndexChanged;
            /// <summary>
            /// Событие изменения текущих даты, номера часа
            /// </summary>
            public event DelegateDateFunc EvtDatetimeHourChanged;
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
                ctrl.Name = KEY_CONTROLS.CUR_DATETIME_HOUR.ToString ();
                (ctrl as DateTimePicker).DropDownAlign = LeftRightAlignment.Right;
                (ctrl as DateTimePicker).Format = DateTimePickerFormat.Custom;
                (ctrl as DateTimePicker).CustomFormat = @"HH-й час, dd MMMM, yyyy";
                (ctrl as DateTimePicker).ValueChanged += new EventHandler(OnDatetimeHourValueChanged);
                ctrl.Dock = DockStyle.Fill;
                //Добавить к текущей панели календарь
                this.Controls.Add(ctrl, 0, 0);
                this.SetColumnSpan(ctrl, 4); this.SetRowSpan(ctrl, 1);

                // раскрывающийся список для выбора ГТП
                ctrl = new ComboBox ();
                ctrl.Name = KEY_CONTROLS.CB_GTP.ToString ();
                (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                (ctrl as ComboBox).SelectedIndexChanged += new EventHandler (OnGTPSelectionIndexChanged);
                ctrl.Dock = DockStyle.Fill;
                //Добавить к текущей панели список ГТП
                this.Controls.Add(ctrl, 0, 1);
                this.SetColumnSpan(ctrl, 4); this.SetRowSpan(ctrl, 1);

                // коэффициент для текущего ГТП
                ctrl = new Label();
                ctrl.Text = @"Текущий коэфф-т:";
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Top);
                this.Controls.Add(ctrl, 0, 2);
                this.SetColumnSpan(ctrl, 2); this.SetRowSpan(ctrl, 1);
                ctrl = new TextBox();
                ctrl.Name = KEY_CONTROLS.TB_GTP_KOEFF.ToString();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Right | AnchorStyles.Top);
                this.Controls.Add(ctrl, 2, 2);
                this.SetColumnSpan(ctrl, 2); this.SetRowSpan(ctrl, 1);

                // таблица для отображения значений ГТП
                ctrl = new PanelSOTIASSO.DataGridViewGTP();
                ctrl.Name = KEY_CONTROLS.DGV_GTP_VALUE.ToString();
                ctrl.Dock = DockStyle.Fill;
                //ctrl.Anchor = (AnchorStyles)((AnchorStyles.Left | AnchorStyles.Top) | AnchorStyles.Right);
                //ctrl.Height = 240; // RowSpan = ...                
                //Добавить к текущей панели таблицу значений ГТП
                this.Controls.Add(ctrl, 0, 3);
                this.SetColumnSpan(ctrl, 4); this.SetRowSpan(ctrl, 9);

                // разделительная линия
                ctrl = new GroupBox ();
                ctrl.Anchor = (AnchorStyles)(AnchorStyles.Left | AnchorStyles.Right);
                ctrl.Height = 3;
                //Добавить к текущей панели раздел./линию
                this.Controls.Add(ctrl, 0, 12);
                this.SetColumnSpan(ctrl, 4); this.SetRowSpan(ctrl, 1);
                
                // список для выбора ТГ
                ctrl = new CheckedListBox();
                ctrl.Name = KEY_CONTROLS.CLB_TG.ToString ();
                ctrl.Dock = DockStyle.Fill;
                //Добавить к текущей панели список для выбора ТГ
                this.Controls.Add(ctrl, 0, 13);
                this.SetColumnSpan(ctrl, 4); this.SetRowSpan(ctrl, 3);
                
                // таблица для отображения значений ГТП
                ctrl = new PanelSOTIASSO.DataGridViewTG();
                ctrl.Name = KEY_CONTROLS.DGV_TG_VALUE.ToString();
                ctrl.Dock = DockStyle.Fill;
                //ctrl.Anchor = (AnchorStyles)((AnchorStyles.Left | AnchorStyles.Top) | AnchorStyles.Right);
                //ctrl.Height = 240; // RowSpan = ...                
                //Добавить к текущей панели таблицу значений ГТП
                this.Controls.Add(ctrl, 0, 16);
                this.SetColumnSpan(ctrl, 4); this.SetRowSpan(ctrl, 8);

                ////Приостановить прорисовку текущей панели
                //this.SuspendLayout();

                //Возобновить прорисовку текущей панели
                this.ResumeLayout(false);
                //Принудительное применение логики макета
                this.PerformLayout();
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
            /// <summary>
            /// Обработчик события - изменение выбранного элемента 'ComboBox' - текущий ГТП
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            private void OnGTPSelectionIndexChanged(object obj, EventArgs ev)
            {
                EvtGTPSelectionIndexChanged (((this.Controls.Find(KEY_CONTROLS.CB_GTP.ToString(), true))[0] as ComboBox).SelectedIndex);
            }

            private void OnDatetimeHourValueChanged (object obj, EventArgs ev)
            {
                EvtDatetimeHourChanged (((this.Controls.Find(KEY_CONTROLS.CUR_DATETIME_HOUR.ToString(), true))[0] as DateTimePicker).Value);
            }
        }

        private class HZEdGraph_GTP : ZedGraph.ZedGraphControl
        {
        }

        private class HZEdGraph_TG : ZedGraph.ZedGraphControl
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
                    false;
                this.MultiSelect = false;
                this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                this.Columns[0].Width = 38;
                this.Columns[0].Frozen = true;
                this.Columns[1].Frozen = true;
                this.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
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
                    false;
                this.MultiSelect = false;
                this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                this.Columns[0].Width = 38;
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
            if (! (m_tecView == null))
            {
                if (m_tecView.Actived == true)
                    m_tecView.Activate(false);
                else
                    ;

                if (m_tecView.IsStarted == true)
                    m_tecView.Stop();
                else
                    ;

                m_tecView = null;
            }
            else
                ;

            if (!(m_evTimerCurrent == null))
                m_evTimerCurrent.Reset();
            else
                ;

            if (! (m_timerCurrent ==null))
            {
                m_timerCurrent.Dispose();

                m_timerCurrent = null;
            }
            else
                ;

            base.Stop ();
        }
        /// <summary>
        /// Переопределение наследуемой функции - активация/деактивация объекта
        /// </summary>
        public override bool Activate(bool active)
        {
            bool bRes = false;

            bRes = base.Activate(active);

            m_tecView.Activate(active);

            if (m_tecView.Actived == true)
                m_timerCurrent.Change(0, System.Threading.Timeout.Infinite);
            else
                m_tecView.ReportClear(true);

            return bRes;
        }
        /// <summary>
        /// Обработчик события - создание дескриптора панели
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>        
        private void OnHandleCreated (object obj, EventArgs ev)
        {
            List <string> listGTPNameShr = new List<string> ();
            
            foreach (TEC t in m_listTEC)
                foreach (TECComponent tc in t.list_TECComponents)
                    if ((tc.m_id > 100) && (tc.m_id < 500))
                        listGTPNameShr.Add(t.name_shr + @" " + tc.name_shr);
                    else
                        ;

            m_panelManagement.InitializeGTPList(listGTPNameShr);

            panelManagement_OnEvtDatetimeHourChanged (DateTime.Now);
        }

        private void TimerCurrent_Tick (object obj)
        {
            if (m_tecView.Actived == true)
            {
                m_tecView.ChangeState ();

                m_timerCurrent.Change(PanelStatistic.POOL_TIME * 1000 - 1, System.Threading.Timeout.Infinite);
            }
            else
                ;
        }

        private void panelManagement_OnEvtGTPSelectionIndexChanged (int indx)
        {
            //Передать информацию 'PanelManagement' для заполнения списка ТГ
            List <string> listTGNameShr = new List<string> ();
            int indxTEC = -1
                , indxTECComponent = -1
                , indxTECComponents = -1;

            indxTEC =
            indxTECComponent =
            indxTECComponents =
                0;
            foreach (TEC t in m_listTEC)
            {
                indxTECComponent = 0;
                
                foreach (TECComponent tc in t.list_TECComponents)
                    if ((tc.m_id > 100) && (tc.m_id < 500))
                    {
                        if (indxTECComponents == indx)
                        {
                            foreach (TG tg in tc.m_listTG)
                                listTGNameShr.Add(/*tc.name_shr + @" " + */tg.name_shr);

                            indxTECComponents = -1; //Признак завершения внешнего цикла
                            break;
                        }
                        else
                            ;

                        indxTECComponent++;
                        indxTECComponents ++;
                    }
                    else
                        ;

                if (indxTECComponents < 0)
                {
                    indxTECComponents = indx; //Возвратить найденное значение
                    break;
                }
                else
                    ;

                indxTEC ++;
            }

            m_panelManagement.InitializeTGList(listTGNameShr);

            if (! (m_tecView == null))
            {
                if ((!(m_tecView.m_indx_TEC == indxTEC))
                    || (!(m_tecView.indxTECComponents == indxTECComponent)))
                {
                    m_tecView.Activate (false);
                    m_tecView.Stop ();

                    //m_tecView = null;
                }
                else
                    ;
            }
            else
                ;

            if (m_tecView.IsStarted == false)
            {
                m_tecView.InitTEC(m_listTEC[indxTEC], indxTECComponent, m_markQueries);

                m_tecView.Start ();
                m_tecView.Activate(true);
            }
            else
                ;
        }

        private void panelManagement_OnEvtDatetimeHourChanged(DateTime dtNew)
        {
            m_tecView.m_curDate = dtNew.Date;
        }

        private void showTMGenPower ()
        {
        }
    }
}