using System;
using System.Collections;
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
using System.Data.Common;

namespace Statistic
{
    /// <summary>
    /// Панель для отображения значений СОТИАССО (телеметрия)
    ///  для контроля
    /// </summary>
    public class PanelSOTIASSODay : PanelStatistic
    {
        /// <summary>
        /// Перечисление - состояния для обработки обращений к БД
        /// </summary>
        private enum StatesMachine {
            /// <summary>
            /// Получить текущее время сервера БД
            /// </summary>
            SERVER_TIME
            /// <summary>
            /// Получить список сигналов для источника данных
            /// </summary>
            , LIST_SIGNAL
            /// <summary>
            /// Получить значения для выбранных сигналов источника данных
            /// </summary>
            , VALUES
        }
        /// <summary>
        /// Перечисление - возможные действия с сигналами в списке на панели управления
        /// </summary>
        private enum ActionSignal { SELECT, CHECK }
        /// <summary>
        /// Перечисление - возможные изменения даты/времени, часового пояса
        /// </summary>
        [Flags]
        private enum ActionDateTime { UNKNOWN = 0, VALUE = 0x1, TIMEZONE = 0x2 }
        /// <summary>
        /// Структура для описания сигнала
        /// </summary>
        private struct SIGNAL
        {
            public int id;
            /// <summary>
            /// Уникальный строковый идентификатор сигнала
            ///  , для АИИСКУЭ - формируется динамически по правилу [PREFIX_TEC]#OBJECT[идентификатор_УСПД]_ITEM[номер_канала_в_УСПД]
            ///  , для СОТИАСОО - из источника данных
            /// </summary>
            public string kks_code;
            /// <summary>
            /// Полное наименование сигнала
            /// </summary>
            public string name;
            /// <summary>
            /// Краткое наименование сигнала
            ///  , может быть одинаковое для АИИКУЭ и СОТИАССО
            /// </summary>
            public string name_shr;
        }

        private BackgroundWorker m_threadDraw;
        /// <summary>
        /// Перечисление - целочисленные идентификаторы дочерних элементов управления
        /// </summary>
        private enum KEY_CONTROLS
        {
            UNKNOWN = -1
                , DTP_CUR_DATE, CBX_TEC_LIST, CBX_TIMEZONE, BTN_EXPORT
                , CLB_AIISKUE_SIGNAL, CLB_SOTIASSO_SIGNAL
                , DGV_AIISKUE_VALUE, ZGRAPH_AIISKUE
                , DGV_SOTIASSO_VALUE, ZGRAPH_SOTIASSO
                , COUNT_KEY_CONTROLS
        }
        ///// <summary>
        ///// Объект с признаками обработки типов значений
        ///// , которые будут использоваться фактически (PBR, Admin, AIISKUE, SOTIASSO)
        ///// </summary>
        //private HMark m_markQueries;
        /// <summary>
        /// Объект для обработки запросов/получения данных из/в БД
        /// </summary>
        private HandlerSignalQueue m_HandlerQueue;
        /// <summary>
        /// Словарь с предствалениями для отображения значений выбранных (на панели управления) сигналов
        /// </summary>
        private Dictionary<CONN_SETT_TYPE, HDataGridView> m_dictDataGridViewValues;
        /// <summary>
        /// Панели графической интерпретации значений
        /// 1) АИИСКУЭ "сутки - по-часам для выбранных сигналов, 2) СОТИАССО "сутки - по-часам для выбранных сигналов"
        /// </summary>
        private Dictionary<CONN_SETT_TYPE, HZedGraphControl> m_dictZGraphValues;

        private List<StatisticCommon.TEC> m_listTEC;
        ///// <summary>
        ///// Список индексов компонентов ТЭЦ (ТГ)
        /////  для отображения в субобласти графической интерпретации значений СОТИАССО "минута - по-секундно"
        ///// </summary>
        //private List<int> m_listIdAIISKUEAdvised
        //    , m_listIdSOTIASSOAdvised;
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
        public PanelSOTIASSODay(int iListenerConfigId, List<StatisticCommon.TEC> listTec)
            : base()
        {
            // фильтр ТЭЦ
            m_listTEC = listTec.FindAll(tec => { return (tec.Type == TEC.TEC_TYPE.COMMON) && (tec.m_id < (int)TECComponent.ID.LK); });

            m_dictDataGridViewValues = new Dictionary<CONN_SETT_TYPE, HDataGridView>();
            m_dictZGraphValues = new Dictionary<CONN_SETT_TYPE, HZedGraphControl>();

            //Создать, разместить дочерние элементы управления
            initializeComponent();

            if (m_listTEC.Count > 0) {
                //Создать объект обработки запросов - установить первоначальные индексы для ТЭЦ, компонента
                m_HandlerQueue = new HandlerSignalQueue(iListenerConfigId, m_listTEC);                
                m_HandlerQueue.UserDate = new HandlerSignalQueue.USER_DATE() { UTC_OFFSET = m_panelManagement.CurUtcOffset, Value = m_panelManagement.CurDateTime };
            } else
                Logging.Logg().Error(@"PanelSOTIASSODay::ctor () - кол-во ТЭЦ = 0...", Logging.INDEX_MESSAGE.NOT_SET);

            m_threadDraw = new BackgroundWorker();
            m_threadDraw.DoWork += threadDraw_DoWork;
            m_threadDraw.RunWorkerCompleted += threadDraw_RunWorkerCompleted;
            m_threadDraw.WorkerReportsProgress = false;
            m_threadDraw.WorkerSupportsCancellation = true;

            #region Дополнительная инициализация панели управления
            m_panelManagement.SetTECList(m_listTEC);
            m_panelManagement.EvtTECListSelectionIndexChanged += new Action<int>(panelManagement_TECListOnSelectionChanged);
            m_panelManagement.EvtDateTimeChanged += new Action<ActionDateTime>(panelManagement_OnEvtDateTimeChanged);
            m_panelManagement.EvtActionSignalItem += new Action<CONN_SETT_TYPE, ActionSignal, int>(panelManagement_OnEvtActionSignalItem);
            //m_panelManagement.EvtSetNowHour += new DelegateFunc(panelManagement_OnEvtSetNowHour);
            #endregion

            // сообщить дочернему элементу, что дескриптор родительской панели создан
            this.HandleCreated += new EventHandler(m_panelManagement.Parent_OnHandleCreated);
        }

        private void threadDraw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // throw new NotImplementedException();
        }

        private void threadDraw_DoWork(object sender, DoWorkEventArgs e)
        {
            draw((CONN_SETT_TYPE)e.Argument);

            e.Result = 0;
        }

        /// <summary>
        /// Конструктор - вспомогательный (с параметрами)
        /// </summary>
        /// <param name="container">Владелец текущего объекта</param>
        public PanelSOTIASSODay(IContainer container, int iListenerConfigId, List<StatisticCommon.TEC> listTec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
            : this(iListenerConfigId, listTec)
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
            CONN_SETT_TYPE type = CONN_SETT_TYPE.UNKNOWN;
            System.Windows.Forms.SplitContainer stctrMain
                , stctrView
                , stctrAIISKUE, stctrSOTIASSO;

            //Создать дочерние элементы управления
            m_panelManagement = new PanelManagement(); // панель для размещения элементов управления

            //Создать, настроить размещение таблиц для отображения значений
            type = CONN_SETT_TYPE.DATA_AISKUE;
            m_dictDataGridViewValues.Add(type, new HDataGridView()); // АИИСКУЭ-значения
            m_dictDataGridViewValues[type].Name = KEY_CONTROLS.DGV_AIISKUE_VALUE.ToString();
            m_dictDataGridViewValues[type].Dock = DockStyle.Fill;
            type = CONN_SETT_TYPE.DATA_SOTIASSO;
            m_dictDataGridViewValues.Add(type, new HDataGridView()); // СОТИАССО-значения
            m_dictDataGridViewValues[type].Name = KEY_CONTROLS.DGV_SOTIASSO_VALUE.ToString();
            m_dictDataGridViewValues[type].Dock = DockStyle.Fill;

            //Создать, настроить размещение графических панелей
            type = CONN_SETT_TYPE.DATA_AISKUE;
            m_dictZGraphValues.Add(type, new HZedGraphControl()); // графическая панель для отображения АИИСКУЭ-значений
            m_dictZGraphValues[type].Name = KEY_CONTROLS.ZGRAPH_AIISKUE.ToString();
            m_dictZGraphValues[type].Dock = DockStyle.Fill;
            type = CONN_SETT_TYPE.DATA_SOTIASSO;
            m_dictZGraphValues.Add(type, new HZedGraphControl()); // графическая панель для отображения СОТИАССО-значений
            m_dictZGraphValues[type].Name = KEY_CONTROLS.ZGRAPH_SOTIASSO.ToString();
            m_dictZGraphValues[type].Dock = DockStyle.Fill;

            //Создать контейнеры-сплиттеры, настроить размещение 
            stctrMain = new SplitContainer(); // для главного контейнера (вертикальный)
            stctrMain.Dock = DockStyle.Fill;
            stctrMain.Orientation = Orientation.Vertical;
            stctrView = new SplitContainer(); // для вспомогательного (2 панели) контейнера (горизонтальный)
            stctrView.Dock = DockStyle.Fill;
            stctrView.Orientation = Orientation.Horizontal;
            stctrAIISKUE = new SplitContainer(); // для вспомогательного (таблица + график) контейнера (вертикальный)
            stctrAIISKUE.Dock = DockStyle.Fill;
            stctrAIISKUE.Orientation = Orientation.Vertical;
            stctrSOTIASSO = new SplitContainer(); // для вспомогательного (таблица + график) контейнера (вертикальный)            
            stctrSOTIASSO.Dock = DockStyle.Fill;
            stctrSOTIASSO.Orientation = Orientation.Vertical;

            //Приостановить прорисовку текущей панели
            this.SuspendLayout();

            //Добавить во вспомогательный контейнер элементы управления АИИСКУЭ
            type = CONN_SETT_TYPE.DATA_AISKUE;
            stctrAIISKUE.Panel1.Controls.Add(m_dictDataGridViewValues[type]);
            stctrAIISKUE.Panel2.Controls.Add(m_dictZGraphValues[type]);
            //Добавить во вспомогательный контейнер элементы управления СОТИАССО
            type = CONN_SETT_TYPE.DATA_SOTIASSO;
            stctrSOTIASSO.Panel1.Controls.Add(m_dictDataGridViewValues[type]);
            stctrSOTIASSO.Panel2.Controls.Add(m_dictZGraphValues[type]);
            //Добавить вспомогательные контейнеры
            stctrView.Panel1.Controls.Add(stctrAIISKUE);
            stctrView.Panel2.Controls.Add(stctrSOTIASSO);
            //Добавить элементы управления к главному контейнеру
            stctrMain.Panel1.Controls.Add(m_panelManagement);
            stctrMain.Panel2.Controls.Add(stctrView);

            stctrMain.SplitterDistance = 43;

            //Добавить к текущей панели единственный дочерний (прямой) элемент управления - главный контейнер-сплиттер
            this.Controls.Add(stctrMain);
            //Возобновить прорисовку текущей панели
            this.ResumeLayout(false);
            //Принудительное применение логики макета
            this.PerformLayout();
        }

        private void panelManagement_OnEvtActionSignalItem(CONN_SETT_TYPE type, ActionSignal action, int indxSignal)
        {
            bool bCheckStateReverse = action == ActionSignal.CHECK;

            switch (action) {
                case ActionSignal.CHECK:
                    if (m_dictDataGridViewValues[type].ActionColumn(m_HandlerDb.Signals[type].ElementAt(indxSignal).kks_code
                        , m_HandlerDb.Signals[type].ElementAt(indxSignal).name_shr) == true) {
                        // запросить значения для заполнения нового столбца
                        //??? оптимизация, сравнение с предыдущим, полученным по 'SELECT', набором значений - должны совпадать => запрос не требуется
                        m_dictDataGridViewValues[type].Fill(m_HandlerDb.Values[type].m_valuesHours);
                    } else
                    // столбец удален - ничего не делаем
                        ;
                    break;
                case ActionSignal.SELECT:
                    //??? оптимизация, поиск в табличном представлении ранее запрошенных/полученных наборов значений
                    m_HandlerDb.Request(type, indxSignal);
                    break;
                default:
                    break;
            }
        }

        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            m_HandlerDb.SetDelegateReport(ferr, fwar, fact, fclr);
        }
        /// <summary>
        /// Класс для размещения активных элементов управления
        /// </summary>
        private class PanelManagement : HPanelCommon
        {
            public event Action<int, DateTime> EvtExportDo;

            public event Action<ActionDateTime> EvtDateTimeChanged;
            /// <summary>
            /// Событие изменения текущего индекса ГТП
            /// </summary>
            public event Action<int> EvtTECListSelectionIndexChanged;
            /// <summary>
            /// Событие выбора сигнала (АИИСКУЭ/СОТИАССО) для отображения И экспорта
            /// </summary>
            public event Action<CONN_SETT_TYPE, ActionSignal, int> EvtActionSignalItem;

            private Dictionary<CONN_SETT_TYPE, int> m_dictPreviousSignalItemSelected;
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public PanelManagement()
                : base(6, 24)
            {
                //Инициализировать равномерные высоту/ширину столбцов/строк
                initializeLayoutStyleEvenly();

                m_dictPreviousSignalItemSelected = new Dictionary<CONN_SETT_TYPE, int>() { { CONN_SETT_TYPE.DATA_AISKUE, -1 }, { CONN_SETT_TYPE.DATA_SOTIASSO, -1 } };

                initializeComponent();

                ComboBox ctrl = findControl(KEY_CONTROLS.CBX_TIMEZONE.ToString()) as ComboBox;
                ctrl.Items.AddRange (new object []{ "UTC", "Москва", "Новосибирск" });
                ctrl.SelectedIndex = 1;
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
                SplitContainer stctrSignals;

                //Приостановить прорисовку текущей панели
                // ??? корректней приостановить прорисовку после создания всех дочерних элементов
                // ??? при этом потребуется объявить переменные для каждого из элементов управления
                this.SuspendLayout();

                //Создать дочерние элементы управления
                // календарь для установки текущих даты, номера часа
                ctrl = new DateTimePicker();
                ctrl.Name = KEY_CONTROLS.DTP_CUR_DATE.ToString();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as DateTimePicker).DropDownAlign = LeftRightAlignment.Right;
                (ctrl as DateTimePicker).Format = DateTimePickerFormat.Custom;
                (ctrl as DateTimePicker).CustomFormat = "dd MMM, yyyy";
                (ctrl as DateTimePicker).Value = DateTime.Now.Date.AddDays(-1);
                //Добавить к текущей панели календарь
                this.Controls.Add(ctrl, 0, 0);
                this.SetColumnSpan(ctrl, 3);
                this.SetRowSpan(ctrl, 1);
                // Обработчики событий
                (ctrl as DateTimePicker).ValueChanged += new EventHandler(curDatetime_OnValueChanged);                

                // список для выбора ТЭЦ
                ctrl = new ComboBox();
                ctrl.Name = KEY_CONTROLS.CBX_TEC_LIST.ToString();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                //Добавить к текущей панели список выбра ТЭЦ
                this.Controls.Add(ctrl, 3, 0);
                this.SetColumnSpan(ctrl, 3);
                this.SetRowSpan(ctrl, 1);
                // Обработчики событий
                (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTECList_OnSelectionIndexChanged);                

                // список для часовых поясов
                ctrl = new ComboBox();
                ctrl.Name = KEY_CONTROLS.CBX_TIMEZONE.ToString();
                ctrl.Dock = DockStyle.Fill;
                (ctrl as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                ctrl.Enabled = false;
                //Добавить к текущей панели список для часовых поясов
                this.Controls.Add(ctrl, 0, 1);
                this.SetColumnSpan(ctrl, 3);
                this.SetRowSpan(ctrl, 1);
                //// Обработчики событий
                (ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTimezone_OnSelectedIndexChanged);

                // кнопка для инициирования экспорта
                ctrl = new Button();
                ctrl.Name = KEY_CONTROLS.BTN_EXPORT.ToString();
                ctrl.Dock = DockStyle.Fill;
                ctrl.Text = @"Экспорт";
                //Добавить к текущей панели кнопку "Экспорт"
                this.Controls.Add(ctrl, 3, 1);
                this.SetColumnSpan(ctrl, 3);
                this.SetRowSpan(ctrl, 1);
                // Обработчики событий
                (ctrl as Button).Click += new EventHandler(btnExport_OnClick);

                // панель для управления размером списков с сигналами
                stctrSignals = new SplitContainer();
                stctrSignals.Dock = DockStyle.Fill;
                stctrSignals.Orientation = Orientation.Horizontal;
                //stctrSignals.Panel1MinSize = -1;
                //stctrSignals.Panel2MinSize = -1;
                stctrSignals.SplitterDistance = 46;
                //Добавить сплитер на панель управления
                this.Controls.Add(stctrSignals, 0, 2);
                this.SetColumnSpan(stctrSignals, 6);
                this.SetRowSpan(stctrSignals, 22);

                // список сигналов АИИСКУЭ
                ctrl = new CheckedListBox();
                ctrl.Name = KEY_CONTROLS.CLB_AIISKUE_SIGNAL.ToString();
                ctrl.Dock = DockStyle.Fill;
                ////Добавить к текущей панели список сигналов АИИСКУЭ
                //this.Controls.Add(ctrl, 0, 2);
                //this.SetColumnSpan(ctrl, 6);
                //this.SetRowSpan(ctrl, 10);
                //Добавить с сплиттеру
                stctrSignals.Panel1.Controls.Add(ctrl);
                // Обработчики событий
                (ctrl as CheckedListBox).SelectedIndexChanged += new EventHandler(clbAIISKUESignal_OnSelectedIndexChanged);
                (ctrl as CheckedListBox).ItemCheck += new ItemCheckEventHandler(clbAIISKUESignal_OnItemChecked);

                // список сигналов СОТИАССО
                ctrl = new CheckedListBox();
                ctrl.Name = KEY_CONTROLS.CLB_SOTIASSO_SIGNAL.ToString();
                ctrl.Dock = DockStyle.Fill;
                ////Добавить к текущей панели список сигналов СОТИАССО
                //this.Controls.Add(ctrl, 0, 12);
                //this.SetColumnSpan(ctrl, 6);
                //this.SetRowSpan(ctrl, 12);
                //Добавить с сплиттеру
                stctrSignals.Panel2.Controls.Add(ctrl);
                // Обработчики событий
                (ctrl as CheckedListBox).SelectedIndexChanged += new EventHandler(clbSOTIASSOSignal_OnSelectedIndexChanged);
                (ctrl as CheckedListBox).ItemCheck += new ItemCheckEventHandler(clbSOTIASSOSignal_OnItemChecked);


                //Возобновить прорисовку текущей панели
                this.ResumeLayout(false);
                //Принудительное применение логики макета
                this.PerformLayout();
            }

            private void btnExport_OnClick(object sender, EventArgs e)
            {
                EvtExportDo?.Invoke(-1, DateTime.MinValue);
            }

            private void clbAIISKUESignal_OnItemChecked(object sender, ItemCheckEventArgs e)
            {
                EvtActionSignalItem?.Invoke(CONN_SETT_TYPE.DATA_AISKUE, ActionSignal.CHECK, (sender as CheckedListBox).SelectedIndex);
            }

            private void clbAIISKUESignal_OnSelectedIndexChanged(object sender, EventArgs e)
            {
                CONN_SETT_TYPE type = CONN_SETT_TYPE.DATA_AISKUE;

                if (!(m_dictPreviousSignalItemSelected[type] == (sender as CheckedListBox).SelectedIndex)) {
                    EvtActionSignalItem?.Invoke(type, ActionSignal.SELECT, (sender as CheckedListBox).SelectedIndex);

                    m_dictPreviousSignalItemSelected[type] = (sender as CheckedListBox).SelectedIndex;
                } else
                    ;
            }

            private void clbSOTIASSOSignal_OnItemChecked(object sender, ItemCheckEventArgs e)
            {
                EvtActionSignalItem?.Invoke(CONN_SETT_TYPE.DATA_SOTIASSO, ActionSignal.CHECK, (sender as CheckedListBox).SelectedIndex);
            }

            private void clbSOTIASSOSignal_OnSelectedIndexChanged(object sender, EventArgs e)
            {
                CONN_SETT_TYPE type = CONN_SETT_TYPE.DATA_SOTIASSO;

                if (!(m_dictPreviousSignalItemSelected[type] == (sender as CheckedListBox).SelectedIndex)) {
                    EvtActionSignalItem?.Invoke(type, ActionSignal.SELECT, (sender as CheckedListBox).SelectedIndex);

                    m_dictPreviousSignalItemSelected[type] = (sender as CheckedListBox).SelectedIndex;
                } else
                    ;
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

            public int CurUtcOffset
            {
                get {
                    int iRes = 0;

                    switch ((findControl(KEY_CONTROLS.CBX_TIMEZONE.ToString()) as ComboBox).SelectedIndex) {
                        case 0:
                            // UTC
                            break;
                        case 1:
                            iRes = 3; // Москва
                            break;
                        case 2:
                            iRes = 7; // Новосибирск
                            break;
                        default:
                            break;
                    }

                    return iRes;
                }
            }

            private void enableSelectedIndexchanged(CheckedListBox clb, CONN_SETT_TYPE key, bool bEnabled)
            {
                if (bEnabled == true) {
                    if (key == CONN_SETT_TYPE.DATA_AISKUE)
                        clb.SelectedIndexChanged += /*new EventHandler(*/clbAIISKUESignal_OnSelectedIndexChanged/*)*/;
                    else if (key == CONN_SETT_TYPE.DATA_SOTIASSO)
                        clb.SelectedIndexChanged += /*new EventHandler(*/clbSOTIASSOSignal_OnSelectedIndexChanged/*)*/;
                    else
                        ;
                } else if (bEnabled == false) {
                    if (key == CONN_SETT_TYPE.DATA_AISKUE)
                        clb.SelectedIndexChanged -= /*new EventHandler(*/clbAIISKUESignal_OnSelectedIndexChanged/*)*/;
                    else if (key == CONN_SETT_TYPE.DATA_SOTIASSO)
                        clb.SelectedIndexChanged -= /*new EventHandler(*/clbSOTIASSOSignal_OnSelectedIndexChanged/*)*/;
                    else
                        ;
                } else
                    ;
            }

            public void ClearSignalList(CONN_SETT_TYPE key)
            {
                KEY_CONTROLS keyCtrl = KEY_CONTROLS.UNKNOWN;
                CheckedListBox clb;

                switch (key) {
                    case CONN_SETT_TYPE.DATA_AISKUE:
                        keyCtrl = KEY_CONTROLS.CLB_AIISKUE_SIGNAL;
                        break;
                    case CONN_SETT_TYPE.DATA_SOTIASSO:
                        keyCtrl = KEY_CONTROLS.CLB_SOTIASSO_SIGNAL;
                        break;
                    default:
                        break;
                }

                if (!(keyCtrl == KEY_CONTROLS.UNKNOWN)) {
                    clb = (findControl(keyCtrl.ToString())) as CheckedListBox;

                    // отменить регистрацию обработчика
                    enableSelectedIndexchanged(clb, key, false);

                    m_dictPreviousSignalItemSelected[key] = -1;
                    clb.Items.Clear();

                    // восстановить регистрацию обработчика
                    enableSelectedIndexchanged(clb, key, true);
                } else
                    Logging.Logg().Error(string.Format(@"PanelSOTIASSODay.PanelManagement::InitializeSignalList (key={0}) - ", key.ToString())
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }

            public void InitializeSignalList(CONN_SETT_TYPE key, IEnumerable<string> listSignalNameShr)
            {
                KEY_CONTROLS keyCtrl = KEY_CONTROLS.UNKNOWN;
                CheckedListBox clb;

                switch (key) {
                    case CONN_SETT_TYPE.DATA_AISKUE:
                        keyCtrl = KEY_CONTROLS.CLB_AIISKUE_SIGNAL;
                        break;
                    case CONN_SETT_TYPE.DATA_SOTIASSO:
                        keyCtrl = KEY_CONTROLS.CLB_SOTIASSO_SIGNAL;
                        break;
                    default:
                        break;
                }

                if (!(keyCtrl == KEY_CONTROLS.UNKNOWN)) {
                    clb = (findControl(keyCtrl.ToString())) as CheckedListBox;

                    // отменить регистрацию обработчика
                    enableSelectedIndexchanged(clb, key, false);

                    clb.Items.AddRange(listSignalNameShr.ToArray());

                    // восстановить регистрацию обработчика
                    enableSelectedIndexchanged(clb, key, true);

                    if (clb.Items.Count > 0)
                        clb.SelectedIndex = 0;
                    else
                        ;
                } else
                    Logging.Logg().Error(string.Format(@"PanelSOTIASSODay.PanelManagement::InitializeSignalList (key={0}) - ", key.ToString())
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }

            private void curDatetime_OnValueChanged(object obj, EventArgs ev)
            {
                EvtDateTimeChanged?.Invoke(ActionDateTime.VALUE);
            }

            public void SetTECList(IEnumerable<TEC> listTEC)
            {
                ComboBox ctrl;

                ctrl = findControl(KEY_CONTROLS.CBX_TEC_LIST.ToString()) as ComboBox;

                foreach (TEC t in listTEC)
                    ctrl.Items.Add(t.name_shr);
            }
            /// <summary>
            /// Обработчик события - изменение выбранного элемента 'ComboBox' - текущая ТЭЦ
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            private void cbxTECList_OnSelectionIndexChanged(object obj, EventArgs ev)
            {
                EvtTECListSelectionIndexChanged?.Invoke(Convert.ToInt32(((this.Controls.Find(KEY_CONTROLS.CBX_TEC_LIST.ToString(), true))[0] as ComboBox).SelectedIndex));
            }

            private void cbxTimezone_OnSelectedIndexChanged(object obj, EventArgs ev)
            {
                EvtDateTimeChanged?.Invoke(ActionDateTime.TIMEZONE);
            }        
        }

        private class HDataGridView : DataGridView
        {
            public HDataGridView()
                : base()
            {
                initializeComponent();
            }

            private void initializeComponent()
            {
                TimeSpan tsRow = TimeSpan.Zero;

                Columns.Add("Unknown", string.Empty);
                Columns[0].Visible = false;
                Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;

                for (int i = 0; i < 49; i++) {
                    Rows.Add();

                    Rows[i].Tag = i;
                    if (i < 48) {
                        Rows[i].HeaderCell.Value =
                        Rows[i].HeaderCell.ToolTipText =
                            string.Format("{0}", new DateTime((tsRow = tsRow.Add(TimeSpan.FromMinutes(30))).Ticks).ToString(@"HH:mm"));
                    } else
                        Rows[i].HeaderCell.Value =
                        Rows[i].HeaderCell.ToolTipText =
                            string.Format("{0}", @"Итог:");
                }

                AllowUserToAddRows = false;
                AllowUserToDeleteRows = false;
                AllowUserToResizeColumns = false;

                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders | DataGridViewRowHeadersWidthSizeMode.DisableResizing;
                MultiSelect = false;
                AutoSizeColumnsMode = /*DataGridViewAutoSizeColumnsMode.ColumnHeader |*/ DataGridViewAutoSizeColumnsMode.AllCells;
                SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
            }
            /// <summary>
            /// Действие со столбцом: при наличии - удалить, при отсутствии - добавить
            /// </summary>
            /// <param name="name">Идентификатор столбца</param>
            /// <param name="headerText">Заголовок столбца</param>
            /// <returns>Признак удаления/добавления столбца</returns>
            public bool ActionColumn(string name, string headerText)
            {
                bool bRes = !Columns.Contains(name);

                if (bRes == false)
                // столбец найден
                    Columns.Remove(name);
                else {
                // столбец не найден - добавить
                    SelectionMode = DataGridViewSelectionMode.CellSelect;

                    Columns.Add(name, headerText);
                    Columns[ColumnCount - 1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    Columns[ColumnCount - 1].SortMode = DataGridViewColumnSortMode.NotSortable;

                    SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
                }

                return bRes;
            }

            public void Fill(IEnumerable<HandlerSignalQueue.VALUE> values)
            {
                int iColumn = ColumnCount - 1;

                foreach (DataGridViewRow row in Rows) {
                    if (row.Index < values.Count())
                        try {
                            row.Cells[iColumn].Value = (from value in values where value.index_stamp == (int)row.Tag select value.value).ElementAt(0);
                        } catch (Exception e) {
                            Logging.Logg().Error(string.Format(@"PanelSOTIASSODay.HDataGridView::Fill () - не найдено значение для строки Index={0}, Tag={1}", row.Index, row.Tag), Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    else
                        row.Cells[iColumn].Value = values.Sum(v => v.value);
                }
            }

            public void Clear()
            {
                while (ColumnCount > 1)
                    Columns.RemoveAt(ColumnCount - 1);
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
            /// <summary>
            /// Обновить содержание в графической субобласти "сутки по-часам"
            /// </summary>
            public void Draw(IEnumerable<HandlerSignalQueue.VALUE> srcValues
                , string textConnSettType, string textDate
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

                    values[i] = srcValues.ElementAt(i + 1).value;

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
                pane.Title.Text = textConnSettType;
                pane.Title.Text += new string(' ', 29);
                pane.Title.Text += textDate;

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

            public void Clear()
            {
            }
        }
        /// <summary>
        /// Переопределение наследуемой функции - запуск объекта
        /// </summary>
        public override void Start()
        {
            base.Start();

            m_HandlerDb.Start();
        }
        /// <summary>
        /// Переопределение наследуемой функции - останов объекта
        /// </summary>
        public override void Stop()
        {
            //Проверить актуальность объекта обработки запросов
            if (!(m_HandlerDb == null))
            {
                if (m_HandlerDb.Actived == true)
                    //Если активен - деактивировать
                    m_HandlerDb.Activate(false);
                else
                    ;

                if (m_HandlerDb.IsStarted == true)
                    //Если выполняется - остановить
                    m_HandlerDb.Stop();
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

            m_HandlerDb.Activate(active);

            if (m_HandlerDb.Actived == true) {
                dueTime = 0;
            } else {
                m_HandlerDb.ReportClear(true);
            }

            // признак 1-ой активации можно получить у любого объекта в словаре
            if ((m_HandlerDb.IsFirstActivated == true)
                    & (IsFirstActivated == true)) {
                cbxTECList = findControl(KEY_CONTROLS.CBX_TEC_LIST.ToString()) as ComboBox;
                // инициировать начало заполнения дочерних элементов содержанием
                cbxTECList.SelectedIndex = -1;
                if (cbxTECList.Items.Count > 0)
                    cbxTECList.SelectedIndex = 0;
                else
                    Logging.Logg().Error(@"PanelSOTIASSODay::Activate () - не заполнен список с ТЭЦ...", Logging.INDEX_MESSAGE.NOT_SET);
            } else
                ;            

            return bRes;
        }
        /// <summary>
        /// Обработчик события - изменения даты/номера часа на панели с управляющими элементами
        /// </summary>
        /// <param name="dtNew">Новые дата/номер часа</param>
        private void panelManagement_OnEvtDateTimeChanged(ActionDateTime action_changed)
        {
            //??? либо автоматический опрос в 'm_HandlerDb'
            m_HandlerQueue.UserDate = new HandlerSignalQueue.USER_DATE() { UTC_OFFSET = m_panelManagement.CurUtcOffset, Value = m_panelManagement.CurDateTime };
            // , либо организация цикла опроса в этой функции
            //...
            // , либо вызов метода с аргументами
            //m_HandlerDb.Request(...);
        }        

        private void onStatesCompleted(CONN_SETT_TYPE type, int state_machine)
        {
            switch ((StatesMachine)state_machine) {
                case StatesMachine.LIST_SIGNAL:
                    m_panelManagement.InitializeSignalList(type, m_HandlerDb.GetListSignals(type).Select(sgnl => { return sgnl.name_shr; }));
                    break;
                case StatesMachine.VALUES:
                    //draw(type);
                    //m_threadDraw.RunWorkerAsync(type);                    
                    break;
                default:
                    break;
            }
        }

        private void draw(CONN_SETT_TYPE type)
        {
            Color colorChart = Color.Empty
                , colorPCurve = Color.Empty;

            // получить цветовую гамму
            getColorZEDGraph(type, out colorChart, out colorPCurve);
            // отобразить
            m_dictZGraphValues[type].Draw(m_HandlerDb.Values[type].m_valuesHours
                , type == CONN_SETT_TYPE.DATA_AISKUE ? @"АИИСКУЭ" : type == CONN_SETT_TYPE.DATA_SOTIASSO ? @"СОТИАССО" : @"Неизвестный тип", textGraphCurDateTime
                , colorChart, colorPCurve);
        }

        private void getColorZEDGraph(CONN_SETT_TYPE type, out Color colorChart, out Color colValue)
        {
            FormGraphicsSettings.INDEX_COLOR indxBackGround = FormGraphicsSettings.INDEX_COLOR.COUNT_INDEX_COLOR
                , indxChart = FormGraphicsSettings.INDEX_COLOR.COUNT_INDEX_COLOR;

            //Значения по умолчанию
            switch (type) {
                default:
                case CONN_SETT_TYPE.DATA_AISKUE:
                    indxBackGround = FormGraphicsSettings.INDEX_COLOR.BG_ASKUE;
                    indxChart = FormGraphicsSettings.INDEX_COLOR.ASKUE;
                    break;
                case CONN_SETT_TYPE.DATA_SOTIASSO:
                    indxBackGround = FormGraphicsSettings.INDEX_COLOR.BG_SOTIASSO;
                    indxChart = FormGraphicsSettings.INDEX_COLOR.SOTIASSO;
                    break;
            }

            colorChart = FormMain.formGraphicsSettings.COLOR(indxBackGround);
            colValue = FormMain.formGraphicsSettings.COLOR(indxChart);
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
            foreach (CONN_SETT_TYPE conn_sett_type in new CONN_SETT_TYPE[] { CONN_SETT_TYPE.DATA_AISKUE, CONN_SETT_TYPE.DATA_SOTIASSO }) {
                draw(conn_sett_type);
            }
        }
        /// <summary>
        /// Обработчик события - изменение выбора строки в списке ТЭЦ
        /// </summary>
        /// <param name="indxTEC">Индекс выбранного элемента</param>
        private void panelManagement_TECListOnSelectionChanged (int indxTEC)
        {
            IEnumerable<TECComponentBase> listAIISKUESignalNameShr = new List <TECComponentBase>()
                , listSOTIASSOSignalNameShr = new List<TECComponentBase>();

            if (!(indxTEC < 0)
                && (indxTEC < m_listTEC.Count)) {                
                foreach (CONN_SETT_TYPE conn_sett_type in new CONN_SETT_TYPE[] { CONN_SETT_TYPE.DATA_AISKUE, CONN_SETT_TYPE.DATA_SOTIASSO }) {
                    //Очистить графические представления
                    m_dictZGraphValues[conn_sett_type].Clear();
                    //Очистить табличные представления значений
                    m_dictDataGridViewValues[conn_sett_type].Clear();
                    //Очистить списки с сигналами
                    m_panelManagement.ClearSignalList(conn_sett_type);
                }
                //Инициализировать список ТЭЦ для 'TecView' - указать ТЭЦ в соответствии с указанным ранее индексом (0)
                m_HandlerDb.InitTEC(m_listTEC[indxTEC].m_id);
                //Добавить строки(сигналы) на дочернюю панель(список АИИСКУЭ, СОТИАССО-сигналов)
                // - по возникновению сигнала окончания заппроса                
            } else
                ;            
        }
    }
}