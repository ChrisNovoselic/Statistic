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
        /// Структура для описания сигнала
        /// </summary>
        private struct SIGNAL
        {
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
        /// <summary>
        /// Класс для обработки запросов к источникам данных
        /// </summary>
        private class HandlerDbSignalValue : HHandlerDb
        {
            private IEnumerable<CONN_SETT_TYPE> _types = new List<CONN_SETT_TYPE>() { CONN_SETT_TYPE.DATA_AISKUE, CONN_SETT_TYPE.DATA_SOTIASSO };

            private CONN_SETT_TYPE _current_type;

            private int _current_id_tec;

            private Dictionary<CONN_SETT_TYPE, IEnumerable<ConnectionSettings>> m_dictConnSett;
            /// <summary>
            /// Структура для хранения значения
            /// </summary>
            public struct VALUE
            {
                /// <summary>
                /// Метка времени значения
                /// </summary>
                public DateTime stamp;
                /// <summary>
                /// Значение
                /// </summary>
                public float value;
                /// <summary>
                /// Статус или качество/достоверность значения
                /// </summary>
                public short quality;
            }

            public struct VALUES
            {
                public DateTime serverTime;

                public IEnumerable<VALUE> m_valuesHours;
            }
            /// <summary>
            /// Конструктор - основной (без аргументов)
            /// </summary>
            /// <param name="type"></param>
            public HandlerDbSignalValue(int iListenerConfigDbId, IEnumerable<TEC> listTEC)
                : base()
            {
                _current_type = CONN_SETT_TYPE.UNKNOWN;

                _current_id_tec = -1;
            }

            public Dictionary<CONN_SETT_TYPE, VALUES> Values;

            public IntDelegateIntIntFunc UpdateGUI_Fact;

            private Dictionary<CONN_SETT_TYPE, IEnumerable<SIGNAL>> _dictSignals;
            /// <summary>
            /// Перечень сигналов для текущей ТЭЦ
            /// </summary>
            public IEnumerable<SIGNAL> GetListSignals(CONN_SETT_TYPE type)
            {
                return ((!(_dictSignals == null)) && (_dictSignals.ContainsKey(type) == true)) ? _dictSignals[type] : new List<SIGNAL>();
            }
            /// <summary>
            /// Установить идентификатор выбранной в данный момент ТЭЦ
            /// </summary>
            /// <param name="id">Идентификатор ТЭЦ</param>
            public void InitTEC(int id)
            {
                if (!(_current_id_tec == id)) {
                    _current_id_tec = id;
                    _current_type = CONN_SETT_TYPE.DATA_AISKUE;

                    AddState((int)StatesMachine.SERVER_TIME);
                    AddState((int)StatesMachine.LIST_SIGNAL);

                    Run(string.Format(@"HandlerDbSignalValue::InitTEC (id_tec={0}, type={1}) - ...", _current_id_tec, _current_type));
                } else
                    ;
            }

            public void Request()
            {
                _current_type = CONN_SETT_TYPE.DATA_AISKUE;

                AddState((int)StatesMachine.SERVER_TIME);
                AddState((int)StatesMachine.VALUES);

                Run(string.Format(@"HandlerDbSignalValue::Request (id_tec={0}, type={1}) - ...", _current_id_tec, _current_type));
            }
            /// <summary>
            /// Очистить полученные ранее значения
            /// </summary>
            public override void ClearValues()
            {
                throw new NotImplementedException();
            }
            /// <summary>
            /// Установить/зарегистрировать соединения с необходимыми источниками данных
            /// </summary>
            public override void StartDbInterfaces()
            {
                int i = 1
                    , j = -1;
                ConnectionSettings connSett;

                foreach (KeyValuePair<CONN_SETT_TYPE, IEnumerable<ConnectionSettings>> pair in m_dictConnSett) {
                    for (j = 0; j < pair.Value.Count(); j ++) {
                        if (m_dictIdListeners.ContainsKey((int)pair.Key) == false) {
                            m_dictIdListeners.Add((int)pair.Key, new int[pair.Value.Count()]);

                            m_dictIdListeners[(int)pair.Key].ToList().ForEach(item => { item = -1; });
                        } else
                            ;

                        connSett = pair.Value.ElementAt(j);
                        register((int)pair.Key, j, connSett, connSett.name);
                    }
                }
            }
            /// <summary>
            /// Проверить наличие ответа на отправлеенный ранее запрос
            /// </summary>
            /// <param name="state">Состояние, ответ на запрос по которому выполняется проверка</param>
            /// <param name="error">Признак ошибки при проверке</param>
            /// <param name="outobj">Результат запроса</param>
            /// <returns>Признак выполнения функции проверки результата</returns>
            protected override int StateCheckResponse(int state, out bool error, out object outobj)
            {
                int iRes = 0;
                error = true;
                outobj = null;

                switch ((StatesMachine)state) {
                    case StatesMachine.SERVER_TIME:
                    case StatesMachine.LIST_SIGNAL:
                    case StatesMachine.VALUES:
                        iRes = response(m_IdListenerCurrent, out error, out outobj);
                        break;
                    default:
                        break;
                }

                return iRes;
            }
            /// <summary>
            /// Обработчик возникновения ошибки при обработке запроса
            /// </summary>
            /// <param name="state">Состояние, при обработке запроса по которому, произошла ошибка</param>
            /// <param name="req">Признак ошибки при отправлении запроса</param>
            /// <param name="res">Признак ошибки при обработке результатата запроса</param>
            /// <returns>Признак выполнения функции-обработчика возникновения ошибки</returns>
            protected override INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
            {
                errorReport(string.Format(@"HandlerDbStateValue::StateWarnings(type={0}) - state={1} ...", _current_type, (StatesMachine)state));

                return INDEX_WAITHANDLE_REASON.ERROR;
            }

            protected override int StateRequest(int state)
            {
                INDEX_WAITHANDLE_REASON indxReasonRes = INDEX_WAITHANDLE_REASON.SUCCESS;

                actionReport(string.Format(@"HandlerDbStateValue::StateRequest(type={0}) - state={1} ...", _current_type, (StatesMachine)state));

                switch ((StatesMachine)state) {
                    case StatesMachine.SERVER_TIME:
                        break;
                    case StatesMachine.LIST_SIGNAL:
                        break;
                    case StatesMachine.VALUES:
                        break;
                    default:
                        break;
                }

                return (int)indxReasonRes;
            }

            protected override int StateResponse(int state, object obj)
            {
                INDEX_WAITHANDLE_REASON indxReasonRes = INDEX_WAITHANDLE_REASON.SUCCESS;

                //actionReport(string.Format(@"HandlerDbStateValue::StateResponse(type={0}) - state={1} ...", _type, (StatesMachine)state));

                switch ((StatesMachine)state) {
                    case StatesMachine.SERVER_TIME:
                        break;
                    case StatesMachine.LIST_SIGNAL:
                        break;
                    case StatesMachine.VALUES:
                        break;
                    default:
                        break;
                }

                if (isLastState(state)) {
                    UpdateGUI_Fact?.Invoke(-1, -1);

                    (m_waitHandleState[(int)INDEX_WAITHANDLE_REASON.SUCCESS] as ManualResetEvent).Set();

                    ReportClear(true);
                } else
                    ;

                return (int)indxReasonRes;
            }

            protected override void StateWarnings(int state, int req, int res)
            {
                warningReport(string.Format(@"HandlerDbStateValue::StateWarnings(type={0}) - state={1} ...", _current_type, (StatesMachine)state));
            }

            protected override void InitializeSyncState()
            {
                m_waitHandleState = new WaitHandle[(int)INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON];
                base.InitializeSyncState();

                for (int i = (int)INDEX_WAITHANDLE_REASON.SUCCESS + 1; i < (int)INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON; i++) {
                    m_waitHandleState[i] = new ManualResetEvent(false);
                }
            }
        }

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
        private HandlerDbSignalValue m_HandlerDb;

        private DataGridView m_dgv_AIISKUE
            , m_dgv_SOTIASSO;
        /// <summary>
        /// Панели графической интерпретации значений
        /// 1) АИИСКУЭ "сутки - по-часам для выбранных сигналов, 2) СОТИАССО "сутки - по-часам для выбранных сигналов"
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
        public PanelSOTIASSODay(int iListenerConfigId, List<StatisticCommon.TEC> listTec)
            : base()
        {
            // фильтр ТЭЦ-ЛК
            m_listTEC = new List<TEC>();
            foreach (TEC tec in listTec)
                if (!(tec.m_id > (int)TECComponent.ID.LK))
                    m_listTEC.Add(tec);
                else
                    ;

            if (m_listTEC.Count > 0) {
                //Создать объект обработки запросов - установить первоначальные индексы для ТЭЦ, компонента
                m_HandlerDb = new HandlerDbSignalValue(iListenerConfigId, m_listTEC);              
                m_HandlerDb.UpdateGUI_Fact += new IntDelegateIntIntFunc(onEvtHandlerStatesCompleted);                
            } else
                Logging.Logg().Error(@"PanelSOTIASSODay::ctor () - кол-во ТЭЦ = 0...", Logging.INDEX_MESSAGE.NOT_SET);

            m_listIdAIISKUEAdvised = new List<int>();
            m_listIdSOTIASSOAdvised = new List<int>();

            //Создать, разместить дочерние элементы управления
            initializeComponent();

            #region Дополнительная инициализация панели управления
            m_panelManagement.SetTECList(listTec);
            m_panelManagement.EvtTECListSelectionIndexChanged += new Action<int>(panelManagement_TECListOnSelectionChanged);
            m_panelManagement.EvtDateTimeChanged += new DelegateDateFunc(panelManagement_OnEvtDateTimeChanged);
            m_panelManagement.EvtSignal += new Action<CONN_SETT_TYPE, ActionSignal, int>(panelManagement_OnEvtSignalItemChecked);
            //m_panelManagement.EvtSetNowHour += new DelegateFunc(panelManagement_OnEvtSetNowHour);
            #endregion

            // сообщить дочернему элементу, что дескриптор родительской панели создан
            this.HandleCreated += new EventHandler(m_panelManagement.Parent_OnHandleCreated);
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
            System.Windows.Forms.SplitContainer stctrMain
                , stctrView
                , stctrAIISKUE, stctrSOTIASSO;

            //Создать дочерние элементы управления
            m_panelManagement = new PanelManagement(); // панель для размещения элементов управления

            //Создать, настроить размещение таблиц для отображения значений
            m_dgv_AIISKUE = new DataGridView(); // АИИСКУЭ-значения
            m_dgv_AIISKUE.Name = KEY_CONTROLS.DGV_AIISKUE_VALUE.ToString();
            m_dgv_AIISKUE.Dock = DockStyle.Fill;
            m_dgv_SOTIASSO = new DataGridView(); // СОТИАССО-значения
            m_dgv_SOTIASSO.Name = KEY_CONTROLS.DGV_SOTIASSO_VALUE.ToString();
            m_dgv_SOTIASSO.Dock = DockStyle.Fill;

            //Создать, настроить размещение графических панелей
            m_zGraph_AIISKUE = new HZedGraphControl(); // графическая панель для отображения АИИСКУЭ-значений
            m_zGraph_AIISKUE.Name = KEY_CONTROLS.ZGRAPH_AIISKUE.ToString();
            m_zGraph_AIISKUE.Dock = DockStyle.Fill;
            m_zGraph_SOTIASSO = new ZedGraphControl(); // графическая панель для отображения СОТИАССО-значений
            m_zGraph_SOTIASSO.Name = KEY_CONTROLS.ZGRAPH_SOTIASSO.ToString();
            m_zGraph_SOTIASSO.Dock = DockStyle.Fill;

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
            stctrAIISKUE.Panel1.Controls.Add(m_dgv_AIISKUE);
            stctrAIISKUE.Panel2.Controls.Add(m_zGraph_AIISKUE);
            //Добавить во вспомогательный контейнер элементы управления СОТИАССО
            stctrSOTIASSO.Panel1.Controls.Add(m_dgv_SOTIASSO);
            stctrSOTIASSO.Panel2.Controls.Add(m_zGraph_SOTIASSO);
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

        private void panelManagement_OnEvtSignalItemChecked(CONN_SETT_TYPE typeSignal, ActionSignal action, int indxSignal)
        {
            bool bCheckStateReverse = action == ActionSignal.CHECK;

            throw new NotImplementedException();
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

            public event DelegateDateFunc EvtDateTimeChanged;
            /// <summary>
            /// Событие изменения текущего индекса ГТП
            /// </summary>
            public event Action<int> EvtTECListSelectionIndexChanged;
            /// <summary>
            /// Событие выбора сигнала (АИИСКУЭ/СОТИАССО) для отображения И экспорта
            /// </summary>
            public event Action<CONN_SETT_TYPE, ActionSignal, int> EvtSignal;
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
                (ctrl as DateTimePicker).ValueChanged += new EventHandler(onCurDatetime_ValueChanged);
                //Добавить к текущей панели календарь
                this.Controls.Add(ctrl, 0, 0);
                this.SetColumnSpan(ctrl, 3);
                this.SetRowSpan(ctrl, 1);
                // Обработчики событий
                (ctrl as DateTimePicker).ValueChanged += new EventHandler(curDate_OnValueChanged);                

                // список для выбора ТЭЦ
                ctrl = new ComboBox();
                ctrl.Name = KEY_CONTROLS.CBX_TEC_LIST.ToString();
                ctrl.Dock = DockStyle.Fill;
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
                ctrl.Enabled = false;
                //Добавить к текущей панели список для часовых поясов
                this.Controls.Add(ctrl, 0, 1);
                this.SetColumnSpan(ctrl, 3);
                this.SetRowSpan(ctrl, 1);
                //// Обработчики событий
                //(ctrl as ComboBox).SelectedIndexChanged += new EventHandler(cbxTimezone_OnSelectedIndexChanged);

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
                EvtSignal?.Invoke(CONN_SETT_TYPE.DATA_AISKUE, ActionSignal.CHECK, (sender as CheckedListBox).SelectedIndex);
            }

            private void clbAIISKUESignal_OnSelectedIndexChanged(object sender, EventArgs e)
            {
                EvtSignal?.Invoke(CONN_SETT_TYPE.DATA_AISKUE, ActionSignal.SELECT, (sender as CheckedListBox).SelectedIndex);
            }

            private void clbSOTIASSOSignal_OnItemChecked(object sender, ItemCheckEventArgs e)
            {
                EvtSignal?.Invoke(CONN_SETT_TYPE.DATA_SOTIASSO, ActionSignal.CHECK, (sender as CheckedListBox).SelectedIndex);
            }

            private void clbSOTIASSOSignal_OnSelectedIndexChanged(object sender, EventArgs e)
            {
                EvtSignal?.Invoke(CONN_SETT_TYPE.DATA_SOTIASSO, ActionSignal.SELECT, (sender as CheckedListBox).SelectedIndex);
            }

            private void curDate_OnValueChanged(object sender, EventArgs e)
            {
                throw new NotImplementedException();
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
            private void initializeSignalList(KEY_CONTROLS clb_id, IEnumerable<string> listSignalNameShr)
            {
                CheckedListBox clb = (findControl(clb_id.ToString())) as CheckedListBox;

                
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

                    clb.Items.AddRange(listSignalNameShr.ToArray());

                    if (clb.Items.Count > 0)
                        clb.SelectedIndex = 0;
                    else
                        ;
                } else
                    Logging.Logg().Error(string.Format(@"PanelSOTIASSODay.PanelManagement::InitializeSignalList (key={0}) - ", key.ToString())
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }

            private void onCurDatetime_ValueChanged(object obj, EventArgs ev)
            {
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
                EvtTECListSelectionIndexChanged(Convert.ToInt32(((this.Controls.Find(KEY_CONTROLS.CBX_TEC_LIST.ToString(), true))[0] as ComboBox).SelectedIndex));
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
            /// <summary>
            /// Обновить содержание в графической субобласти "час по-минутно"
            /// </summary>
            public void Draw(IEnumerable<HandlerDbSignalValue.VALUE> srcValues
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
        private void panelManagement_OnEvtDateTimeChanged(DateTime dtNew)
        {
            m_HandlerDb.Request();
        }
        /// <summary>
        /// Обработчик события - все состояния 'ChangeState_SOTIASSO' обработаны
        /// </summary>
        /// <param name="hour">Номер часа в запросе</param>
        /// <param name="min">Номер минуты в звпросе</param>
        /// <returns>Признак результата выполнения функции</returns>
        private int onEvtHandlerStatesCompleted(int iHour, int iMin)
        {
            int iRes = 0;

            return iRes;
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
            Color colorChart = Color.Empty
                    , colorPCurve = Color.Empty;

            getColorZEDGraph(CONN_SETT_TYPE.DATA_AISKUE, out colorChart, out colorPCurve);
            (m_zGraph_AIISKUE as HZedGraphControl).Draw(m_HandlerDb.Values[CONN_SETT_TYPE.DATA_AISKUE].m_valuesHours
                , textGraphCurDateTime
                , colorChart, colorPCurve);
            getColorZEDGraph(CONN_SETT_TYPE.DATA_SOTIASSO, out colorChart, out colorPCurve);
            (m_zGraph_SOTIASSO as HZedGraphControl).Draw(m_HandlerDb.Values[CONN_SETT_TYPE.DATA_SOTIASSO].m_valuesHours
                , textGraphCurDateTime
                , colorChart, colorPCurve);
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
                //Инициализировать список ТЭЦ для 'TecView' - указать ТЭЦ в соответствии с указанным ранее индексом (0)
                m_HandlerDb.InitTEC(m_listTEC[indxTEC].m_id);
                //Добавить строки(сигналы) на дочернюю панель(список АИИСКУЭ, СОТИАССО-сигналов)
                m_panelManagement.InitializeSignalList(CONN_SETT_TYPE.DATA_AISKUE, m_HandlerDb.GetListSignals(CONN_SETT_TYPE.DATA_AISKUE).Select(sgnl => { return sgnl.name_shr; }));
                m_panelManagement.InitializeSignalList(CONN_SETT_TYPE.DATA_AISKUE, m_HandlerDb.GetListSignals(CONN_SETT_TYPE.DATA_SOTIASSO).Select(sgnl => { return sgnl.name_shr; }));
            } else
                ;            
        }
    }
}