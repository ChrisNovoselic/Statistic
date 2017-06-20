using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel; //IContainer
using System.Threading; //ManualResetEvent
using System.Drawing; //Color
using System.Data;

using ZedGraph;

using HClassLibrary;
using StatisticCommon;
using System.Linq;

namespace Statistic
{
    /// <summary>
    /// Панель для отображения значений СОТИАССО (телеметрия)
    ///  для контроля
    /// </summary>
    public class PanelSOTIASSODay : PanelStatistic
    {
        private class TecViewSOTIASSODay : TecView
        {
            public TecViewSOTIASSODay(int indx_tec, int indx_comp, TECComponentBase.TYPE type) : base(indx_tec, indx_comp, type)
            {
            }

            public override void GetRDGValues(int indx, DateTime date)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Перечисление - целочисленные идентификаторы дочерних элементов управления
        /// </summary>
        private enum KEY_CONTROLS
        {
            UNKNOWN = -1
                , DTP_CUR_DATE, CBX_TEC_LIST /*, CBX_TIMEZONE*/, BTN_EXPORT
                , CLB_AIISKUE_SIGNAL, CLB_SOTIASSO_SIGNAL
                , DGV_AIISKUE_VALUE, ZGRAPH_AIISKUE
                , DGV_SOTIASSO_VALUE, ZGRAPH_SOTIASSO
                , COUNT_KEY_CONTROLS
        }
        /// <summary>
        /// Объект с признаками обработки типов значений
        /// , которые будут использоваться фактически (PBR, Admin, AIISKUE, SOTIASSO)
        /// </summary>
        private HMark m_markQueries;
        /// <summary>
        /// Объект для обработки запросов/получения данных из/в БД
        /// </summary>
        private TecViewSOTIASSODay m_tecView;

        System.Windows.Forms.SplitContainer stctrMain
            , stctrView;
        /// <summary>
        /// Панели графической интерпретации значений СОТИАССО
        /// 1) "час - по-минутно для выбранного ГТП", 2) "минута - по-секундно для выбранных ТГ"
        /// </summary>
        private ZedGraph.ZedGraphControl m_zGraph_AIISKUE
            , m_zGraph_SOTIASSO;
        private List<StatisticCommon.TEC> m_listTEC;
        /// <summary>
        /// Список индексов компонентов ТЭЦ (ТГ)
        ///  для отображения в субобласти графической интерпретации значений СОТИАССО "минута - по-секундно"
        /// </summary>
        private List<int> m_listIdAIISKUEAdvised
            , m_listIdSOTIASSOAdvised;
        ///// <summary>
        ///// Событие выбора даты
        ///// </summary>
        //private event Action<DateTime> EvtSetDatetimeHour;
        ///// <summary>
        ///// Делегат для установки даты на панели управляющих элементов управления
        ///// </summary>
        //private Action<DateTime> delegateSetDatetimeHour;
        /// <summary>
        /// Панель для активных элементов управления
        /// </summary>
        private PanelManagement m_panelManagement;
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public PanelSOTIASSODay(List<StatisticCommon.TEC> listTec)
            : base()
        {
            //m_listTEC = listTec;
            // фильтр ТЭЦ-ЛК
            m_listTEC = new List<TEC>();
            foreach (TEC tec in listTec)
                if (!(tec.m_id > (int)TECComponent.ID.LK))
                    m_listTEC.Add(tec);
                else
                    ;
            //Создать объект с признаками обработки тех типов значений
            // , которые будут использоваться фактически
            m_markQueries = new HMark(new int[] { (int)CONN_SETT_TYPE.DATA_AISKUE, (int)CONN_SETT_TYPE.DATA_SOTIASSO });
            //Создать объект обработки запросов - установить первоначальные индексы для ТЭЦ, компонента
            m_tecView = new TecViewSOTIASSODay(0, 0);
            //Инициализировать список ТЭЦ для 'TecView' - указать ТЭЦ в соответствии с указанным ранее индексом (0)
            m_tecView.InitTEC(new List<StatisticCommon.TEC>() { m_listTEC[0] }, m_markQueries);
            //Установить тип значений
            m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] = CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN;
            m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] = CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN;
            //Делегат для установки текущего времени на панели 'PanelManagement'
            //m_tecView.SetDelegateDatetime(...);
            //Делегат по окончанию обработки всех состояний 'TecView::ChangeState_SOTIASSO'
            m_tecView.updateGUI_Fact = new IntDelegateIntIntFunc(onEvtHandlerStatesCompleted);

            m_listIdAIISKUEAdvised = new List<int>();
            m_listIdSOTIASSOAdvised = new List<int>();

            //Создать, разместить дочерние элементы управления
            initializeComponent();
            m_panelManagement.EvtTECListSelectionIndexChanged += new Action<int>(panelManagement_TECListOnSelectionChanged);
            m_panelManagement.EvtDatetimeHourChanged += new DelegateDateFunc(panelManagement_OnEvtDatetimeHourChanged);
            m_panelManagement.EvtSignalItemChecked += new Action<CONN_SETT_TYPE, int>(panelManagement_OnEvtSignalItemChecked);
            //m_panelManagement.EvtSetNowHour += new DelegateFunc(panelManagement_OnEvtSetNowHour);
            // сообщить дочернему элементу, что дескриптор родительской панели создан
            this.HandleCreated += new EventHandler(m_panelManagement.Parent_OnHandleCreated);
        }
        /// <summary>
        /// Конструктор - вспомогательный (с параметрами)
        /// </summary>
        /// <param name="container">Владелец текущего объекта</param>
        public PanelSOTIASSODay(IContainer container, List<StatisticCommon.TEC> listTec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
            : this(listTec)
        {
            container.Add(this);
        }
        ///// <summary>
        ///// Деструктор
        ///// </summary>
        //~PanelSOTIASSODay ()
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
        private void initializeComponent()
        {
            //Создать дочерние элементы управления
            m_panelManagement = new PanelManagement(); // панель для размещения элементов управления
            
            m_zGraph_AIISKUE = new ZedGraphControlSignalDayValues(); // графическая панель для отображения значений ГТП
            m_zGraph_AIISKUE.Name = KEY_CONTROLS.ZGRAPH_AIISKUE.ToString();
            m_zGraph_SOTIASSO = new ZedGraphControlSignalDayValues(); // графическая панель для отображения значений ТГ
            m_zGraph_SOTIASSO.Name = KEY_CONTROLS.ZGRAPH_SOTIASSO.ToString();

            //Создать сплиттеры
            stctrMain = new SplitContainer(); // для главного контейнера (вертикальный)
            stctrView = new SplitContainer(); // для вспомогательного (2 графические панели) контейнера (горизонтальный)
            //Настроить размещение главного контейнера
            stctrMain.Dock = DockStyle.Fill;
            stctrMain.Orientation = Orientation.Vertical;
            //Настроить размещение вспомогательного контейнера
            stctrView.Dock = DockStyle.Fill;
            stctrView.Orientation = Orientation.Horizontal;
            //Настроить размещение графических панелей
            m_zGraph_AIISKUE.Dock = DockStyle.Fill;
            m_zGraph_SOTIASSO.Dock = DockStyle.Fill;

            //Приостановить прорисовку текущей панели
            this.SuspendLayout();

            //Добавить во вспомогательный контейнер графические панели
            stctrView.Panel1.Controls.Add(m_zGraph_AIISKUE);
            stctrView.Panel2.Controls.Add(m_zGraph_SOTIASSO);
            //Добавить элементы управления к главному контейнеру
            stctrMain.Panel1.Controls.Add(m_panelManagement);
            stctrMain.Panel2.Controls.Add(stctrView);

            //stctrMain.FixedPanel = FixedPanel.Panel1;
            stctrMain.SplitterDistance = 43;

            //Добавить к текущей панели единственный дочерний (прямой) элемент управления - главный контейнер-сплиттер
            this.Controls.Add(stctrMain);
            //Возобновить прорисовку текущей панели
            this.ResumeLayout(false);
            //Принудительное применение логики макета
            this.PerformLayout();
        }

        private void panelManagement_OnEvtSignalItemChecked(CONN_SETT_TYPE typeSignal, int idSignal)
        {
            throw new NotImplementedException();
        }

        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            m_tecView.SetDelegateReport(ferr, fwar, fact, fclr);
        }
        /// <summary>
        /// Класс для размещения активных элементов управления
        /// </summary>
        private class PanelManagement : HPanelCommon
        {
            public event DelegateDateFunc EvtDatetimeHourChanged;
            /// <summary>
            /// Событие изменения текущего индекса ГТП
            /// </summary>
            public event Action<int> EvtTECListSelectionIndexChanged;
            /// <summary>
            /// Событие выбора сигнала (АИИСКУЭ/СОТИАССО) для отображения И экспорта
            /// </summary>
            public event Action<CONN_SETT_TYPE, int> EvtSignalItemChecked;
            //public event DelegateFunc EvtSetNowHour;
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public PanelManagement()
                : base(6, 24)
            {
                //Инициализировать равномерные высоту/ширину столбцов/строк
                initializeLayoutStyleEvenly();

                initializeComponent();
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


                //Возобновить прорисовку текущей панели
                this.ResumeLayout(false);
                //Принудительное применение логики макета
                this.PerformLayout();
            }
            /// <summary>
            /// Обработчик события - дескриптор элемента управления создан
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            public void Parent_OnHandleCreated(object obj, EventArgs ev)
            {
            }
            /// <summary>
            /// Текущее (указанное пользователем) дата/время
            /// ??? учитывать часовой пояс
            /// </summary>
            public DateTime CurDateTime
            {
                get
                {
                    return (findControl(KEY_CONTROLS.DTP_CUR_DATE.ToString()) as DateTimePicker).Value;
                }
            }
            /// <summary>
            /// Заполнить список с игналами значениями в аргументе
            /// </summary>
            /// <param name="clb_id">Идентификатор списка</param>
            /// <param name="listAIISKUESignalNameShr">Список </param>
            private void initializeSignalList(KEY_CONTROLS clb_id, IEnumerable<TECComponentBase> listAIISKUESignalNameShr)
            {
                CheckedListBox clb = (findControl(clb_id.ToString())) as CheckedListBox;

                clb.Items.AddRange((from comp in listAIISKUESignalNameShr select new { comp.name_shr }).ToArray() );

                if (clb.Items.Count > 0)
                    clb.SelectedIndex = 0;
                else
                    ;
            }

            public void InitializeAIISKUESignalList(IEnumerable<TECComponentBase> listAIISKUESignals)
            {
                initializeSignalList(KEY_CONTROLS.CLB_AIISKUE_SIGNAL, listAIISKUESignals);
            }

            public void InitializeSOTIASSOSignalList(IEnumerable<TECComponentBase> listSOTIASSOSignals)
            {
                initializeSignalList(KEY_CONTROLS.CLB_SOTIASSO_SIGNAL, listSOTIASSOSignals);
            }

            private void onCurDatetime_ValueChanged(object obj, EventArgs ev)
            {
            }
            /// <summary>
            /// Обработчик события - изменение выбранного элемента 'ComboBox' - текущая ТЭЦ
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            private void cbxTECList_OnSelectionIndexChanged(object obj, EventArgs ev)
            {
                EvtTECListSelectionIndexChanged(Convert.ToInt32(((this.Controls.Find(KEY_CONTROLS.CBX_TEC_LIST.ToString(), true))[0] as ComboBox).SelectedValue));
            }

            private void onAIISKUESignal_ItemCheck(object obj, ItemCheckEventArgs ev)
            {
            }

            private void onSOTIASSOSignal_ItemCheck(object obj, ItemCheckEventArgs ev)
            {
            }           
        }
        /// <summary>
        /// Класс - общий для графического представления значений СОТИАССО на вкладке
        /// </summary>
        private class HZedGraphControl : ZedGraph.ZedGraphControl
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public HZedGraphControl()
                : base()
            {
                initializeComponent();
            }
            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец объекта</param>
            public HZedGraphControl(IContainer container)
                : this()
            {
                container.Add(this);
            }
            /// <summary>
            /// Инициализация собственных компонентов элемента управления
            /// </summary>
            private void initializeComponent()
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
            /// <summary>
            /// Обработчик события - отобразить значения точек
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие - this</param>
            /// <param name="pane">Контекст графического представления (полотна)</param>
            /// <param name="curve">Коллекция точек для отображения на полотне</param>
            /// <param name="iPt">Индекс точки в наборе точек для отображения</param>
            /// <returns>Значение для отображения для точки с индексом</returns>
            private string onPointValueEvent(object sender, GraphPane pane, CurveItem curve, int iPt)
            {
                return curve[iPt].Y.ToString("F2");
            }
            /// <summary>
            /// Обработчик события - двойной "щелчок" мыши
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие - this</param>
            /// <param name="e">Вргумент события</param>
            /// <returns>Признак продолжения обработки события</returns>
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
        private class ZedGraphControlSignalDayValues : HZedGraphControl
        {
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public ZedGraphControlSignalDayValues()
                : base()
            {
                initializeComponent();
            }
            /// <summary>
            /// Конструктор - вспомогательный (с параметрами)
            /// </summary>
            /// <param name="container">Владелец объекта</param>
            public ZedGraphControlSignalDayValues(IContainer container)
                : this()
            {
                container.Add(this);
            }
            /// <summary>
            /// Инициализация собственных компонентов элемента управления
            /// </summary>
            private void initializeComponent()
            {
            }

            /// <summary>
            /// Обновить содержание в графической субобласти "час по-минутно"
            /// </summary>
            public void Draw(IEnumerable<TecView.valuesTEC> srcValues
                , string text
                , Color colorChart
                , Color colorPCurve)
            {
                double[] values = null;
                int itemscount = -1;
                string[] names = null;
                double minimum
                    , minimum_scale
                    , maximum
                    , maximum_scale;
                bool noValues = false;

                itemscount = srcValues.Count() - 1;

                names = new string[itemscount];

                values = new double[itemscount];

                minimum = double.MaxValue;
                maximum = 0;
                noValues = true;

                for (int i = 0; i < itemscount; i++) {
                    names[i] = (i + 1).ToString();

                    values[i] = srcValues.ElementAt(i + 1).valuesFact;

                    if ((minimum > values[i]) && (!(values[i] == 0))) {
                        minimum = values[i];
                        noValues = false;
                    } else
                        ;

                    if (maximum < values[i])
                        maximum = values[i];
                    else
                        ;
                }

                if (!(FormMain.formGraphicsSettings.scale == true))
                    minimum = 0;
                else
                    ;

                if (noValues) {
                    minimum_scale = 0;
                    maximum_scale = 10;
                } else {
                    if (minimum != maximum) {
                        minimum_scale = minimum - (maximum - minimum) * 0.2;
                        if (minimum_scale < 0)
                            minimum_scale = 0;
                        maximum_scale = maximum + (maximum - minimum) * 0.2;
                    } else {
                        minimum_scale = minimum - minimum * 0.2;
                        maximum_scale = maximum + maximum * 0.2;
                    }
                }

                GraphPane pane = GraphPane;
                pane.CurveList.Clear();
                pane.Chart.Fill = new Fill(colorChart);

                if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Bar) {
                    pane.AddBar("Мощность", null, values, colorPCurve);
                } else
                    if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Linear) {
                    ////Вариант №1
                    //double[] valuesFactLinear = new double[itemscount];
                    //for (int i = 0; i < itemscount; i++)
                    //    valuesFactLinear[i] = valsMins[i];
                    //Вариант №2
                    PointPairList ppl = new PointPairList();
                    for (int i = 0; i < itemscount; i++)
                        if (values[i] > 0)
                            ppl.Add(i, values[i]);
                        else
                            ;
                    //LineItem
                    pane.AddCurve("Мощность"
                        ////Вариант №1
                        //, null, valuesFactLinear
                        //Вариант №2
                        , ppl
                        , colorPCurve);
                } else
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
                pane.Title.Text += text;

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

                AxisChange();

                Invalidate();
            }
        }
        /// <summary>
        /// Переопределение наследуемой функции - запуск объекта
        /// </summary>
        public override void Start()
        {
            base.Start();

            m_tecView.Start();
        }
        /// <summary>
        /// Переопределение наследуемой функции - останов объекта
        /// </summary>
        public override void Stop()
        {
            //Проверить актуальность объекта обработки запросов
            if (!(m_tecView == null))
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

            //Остановить базовый объект
            base.Stop();
        }
        /// <summary>
        /// Переопределение наследуемой функции - активация/деактивация объекта
        /// </summary>
        public override bool Activate(bool active)
        {
            bool bRes = false;

            int dueTime = System.Threading.Timeout.Infinite;
            ComboBox cbxTECList;

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

            if (m_tecView.IsFirstActivated == true & IsFirstActivated == true) {
                cbxTECList = findControl(KEY_CONTROLS.CBX_TEC_LIST.ToString()) as ComboBox;
                // инициировать начало заполнения дочерних элементов содержанием
                cbxTECList.SelectedIndex = -1;
                cbxTECList.SelectedIndex = 0;
            } else
                ;            

            return bRes;
        }
        /// <summary>
        /// Обработчик события - изменения даты/номера часа на панели с управляющими элементами
        /// </summary>
        /// <param name="dtNew">Новые дата/номер часа</param>
        private void panelManagement_OnEvtDatetimeHourChanged(DateTime dtNew)
        {
            //Проверить наличие даты/времени полученного на сервере (хотя бы один раз)
            if (m_tecView.serverTime.Equals(DateTime.MinValue) == false)
                if ((m_tecView.m_curDate.Date.Equals(m_tecView.serverTime.Date) == true)
                    && (m_tecView.lastHour.Equals(m_tecView.serverTime.Hour) == true))
                {
                    m_tecView.adminValuesReceived = false; //Чтобы не выполнилась ветвь - переход к след./часу
                    m_tecView.currHour = true;
                }
                else
                {
                    m_tecView.currHour = false;

                    m_tecView.ChangeState();
                }
            else
                // не выполнен НИ один успешный запрос к БД_значений
                ;
        }

        public void panelManagement_OnEvtSetDatetimeHour(DateTime dtVal)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Обработчик события - все состояния 'ChangeState_SOTIASSO' обработаны
        /// </summary>
        /// <param name="hour">Номер часа в запросе</param>
        /// <param name="min">Номер минуты в звпросе</param>
        /// <returns>Признак результата выполнения функции</returns>
        private int onEvtHandlerStatesCompleted(int hour, int min)
        {
            int iRes = 0;

            return iRes;
        }

        private void getColorZEDGraph(out Color colChart, out Color colP)
        {
            //Значения по умолчанию
            colChart = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.BG_SOTIASSO);
            colP = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.SOTIASSO);
        }
        /// <summary>
        /// Текст (часть) заголовка для графической субобласти
        /// </summary>
        private string textGraphCurDateTime
        {
            get
            {
                return m_panelManagement.CurDateTime.ToShortDateString();
            }
        }
        /// <summary>
        /// Перерисовать объекты с графическим представлением данных
        ///  , в зависимости от типа графического представления (гистограмма, график)
        /// </summary>
        /// <param name="type">Тип изменений, выполненных пользователем</param>
        public void UpdateGraphicsCurrent(int type)
        {
            Color colorChart = Color.Empty
                    , colorPCurve = Color.Empty;
            getColorZEDGraph(out colorChart, out colorPCurve);

            (m_zGraph_AIISKUE as ZedGraphControlSignalDayValues).Draw(m_tecView.m_valuesMins
                , textGraphCurDateTime
                , colorChart, colorPCurve);
        }
        /// <summary>
        /// Обработчик события - изменение выбора строки в 'DataGridViewGTP'
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void panelManagement_TECListOnSelectionChanged (int indxTEC)
        {
            IEnumerable<TECComponentBase> listAIISKUESignalNameShr = new List <TECComponentBase>()
                , listSOTIASSOSignalNameShr = new List<TECComponentBase>();

            if (!(indxTEC < 0)
                && (indxTEC < m_listTEC.Count)) {
                //Добавить строки(сигналы) на дочернюю панель(список АИИСКУЭ-сигналов)
                m_panelManagement.InitializeAIISKUESignalList(listAIISKUESignalNameShr);
                //Добавить строки(сигналы) на дочернюю панель(список СОТИАССО-сигналов)
                m_panelManagement.InitializeSOTIASSOSignalList(listSOTIASSOSignalNameShr);
            } else
                ;            
        }
    }
}