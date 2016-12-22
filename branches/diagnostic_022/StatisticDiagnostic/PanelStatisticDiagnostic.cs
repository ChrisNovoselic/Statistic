using HClassLibrary;
using StatisticCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Drawing;
using System.Linq;
using System.Timers;
using System.Windows.Forms; //TableLayoutPanel

namespace StatisticDiagnostic
{
    /// <summary>
    /// общий класс для панелей(???)
    /// </summary>
    public abstract partial class PanelDiagnostic : HPanelCommon
    {
        public PanelDiagnostic(string nameLabel)
            : base(-1, -1)
        {
            initialize();
        }

        public PanelDiagnostic(IContainer container)
            : base(container, -1, -1)
        {
            container.Add(this);

            initialize();
        }

        protected abstract void LoadValue();

        protected abstract void Activated(bool activated);

        protected abstract void AddRows(int numP, int countRow);

        protected abstract void ClearGrid();

        protected abstract void CreateForm();

        protected abstract void CellsPingAdd();

        protected abstract string FormatTime(string datetime);
    }

    public abstract partial class PanelDiagnostic
    {
        //private Label lblName;
        //private DataGridView dgvCommon;

        private void initialize()
        {
            InitializeComponent();
        }

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly(cols, rows);
        }

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
            //this.Controls.Add(lblName, 0, 0);
            //this.Controls.Add(dgvCommon, 0, 1);
            //this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            //this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.74641F));

            //this.dgvCommon = new DataGridView();
            //this.lblName = new Label();
            this.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;

            this.SuspendLayout();

            //this.dgvCommon.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            //this.dgvCommon.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            //this.dgvCommon.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            //this.dgvCommon.AllowUserToAddRows = false;
            //this.dgvCommon.ClearSelection();
            //this.dgvCommon.AllowUserToDeleteRows = false;
            //this.dgvCommon.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.dgvCommon.RowHeadersVisible = false;
            //this.dgvCommon.ReadOnly = true;
            //this.dgvCommon.CellValueChanged += dgvCommon_CellValueChanged;
            //this.dgvCommon.CellClick += dgvCommon_CellValueChanged;

            ////this.dgvCommon.ColumnCount = ;
            ////this.dgvCommon.RowCount = ;

            //this.lblName.AutoSize = true;
            //this.lblName.Name = "Label";
            //this.lblName.TabIndex = 0;
            //this.lblName.Text = "Unknow";
            //this.lblName.TextAlign = System.Drawing.ContentAlignment.TopCenter;

            this.ResumeLayout(false);
        }

        /// <summary>
        /// Обработчик события - при "щелчке" по любой части ячейки
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие - (???ячейка, скорее - 'DataGridView')</param>
        /// <param name="e">Аргумент события</param>
        void dgvCommon_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try {
                //if (dgvCommon.SelectedCells.Count > 0)
                //    dgvCommon.SelectedCells[0].Selected = false;
                //else
                //    ;
            } catch {
            }
        }

        #endregion
    }

    /// <summary>
    /// Класс для описания панели с информацией
    ///  по дианостированию состояния ИС
    /// </summary>
    public partial class PanelStatisticDiagnostic
    {
        /// <summary>
        /// Определить размеры ячеек макета панели
        /// </summary>
        /// <param name="cols">Количество столбцов в макете</param>
        /// <param name="rows">Количество строк в макете</param>
        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly(cols, rows);
        }

        /// <summary>
        /// Требуется переменная конструктора
        /// </summary>
        private IContainer components = null;

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

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            this.SuspendLayout();

            this.Controls.Add(m_tecdb, 0, 0); this.SetColumnSpan(m_tecdb, 8); this.SetRowSpan(m_tecdb, 6);
            this.Controls.Add(m_modesdb, 0, 6); this.SetColumnSpan(m_tecdb, 8); this.SetRowSpan(m_tecdb, 5);
            this.Controls.Add(m_taskdb, 0, 11); this.SetColumnSpan(m_tecdb, 5); this.SetRowSpan(m_tecdb, 5);
            this.Controls.Add(m_sizedb, 5, 11); this.SetColumnSpan(m_tecdb, 3); this.SetRowSpan(m_tecdb, 5);

            this.ResumeLayout();
        }

        #endregion

        ///// <summary>
        ///// Подпись к вложенной панели с задачами
        ///// </summary>
        //private Label Tasklabel;
        ///// <summary>
        ///// Подпись к вложенной панели с параметрами диагностирования источников данных для ТЭЦ
        ///// </summary>
        //private Label TEClabel;
        ///// <summary>
        ///// Подпись к вложенной панели с параметрами диагностирования сервисов Модес
        ///// </summary>
        //private Label Modeslabel;
        ///// <summary>
        ///// Подпись к вложенной панели с параметрами Баз Данных
        ///// </summary>
        //private Label SizeBDlabel;
        ///// <summary>
        ///// Панель для размещения групповых элементов интерфейса с пользователем
        /////  с параметрами диагностирования источников данных для ТЭЦ
        ///// </summary>
        //private TableLayoutPanel TecTableLayoutPanel;
        ///// <summary>
        ///// Панель для размещения групповых элементов интерфейса с пользователем
        /////  с параметрами диагностирования сервисов Модес
        ///// </summary>
        //private TableLayoutPanel ModesTableLayoutPanel;
        ///// <summary>
        ///// Панель для размещения групповых элементов интерфейса с пользователем
        /////  с параметрами диагностирования сервисов Модес
        ///// </summary>
        //private TableLayoutPanel TaskTableLayoutPanel;
        ///// <summary>
        ///// Панель для размещения подписей для информационных окон
        ///// </summary>
        //private TableLayoutPanel LabelTableLayoutPanel;
    }

    /// <summary>
    /// Класс для описания панели с информацией
    ///  по дианостированию состояния ИС
    /// </summary>
    public partial class PanelStatisticDiagnostic : PanelStatistic
    {
        /// <summary>
        /// Список активных источников
        /// </summary>
        static object[,] m_arrayActiveSource;

        //private static DataTable m_tableSourceData;

        //public static DataTable m_tableSourceDiagnostic = new DataTable(); //m_listDiagnosticSource
        private static DataTable m_tableSourceList = new DataTable();
        private static DataTable m_tableGTPList = new DataTable();
        private static DataTable m_tableTECList = new DataTable();
        private static DataTable m_tableParamDiagnostic = new DataTable();
        /// <summary>
        /// Перечесления типов источников
        /// </summary>
        protected enum INDEX_SOURCE
        {
            UNKNOW = -1,
            SIZEDB = 10,
            MODES = 200,
            TASK = 300
        }
        /// <summary>
        /// 
        /// </summary>
        protected enum INDEX_UNITS
        {
            UNKNOW = 0,
            VALUE = 12, DATA = 13
        }
        /// <summary>
        /// экземпляр класса 
        /// для подклчения к бд
        /// </summary>
        private HDataSource m_DataSource;
        private System.Timers.Timer m_timerUpdate; //таймер для обновления панелей
        /// <summary>
        /// Экземпляры класса Task
        /// </summary>
        private PanelTask m_taskdb;
        /// <summary>
        /// Экземпляры класса Modes
        /// </summary>
        private PanelContainerModes m_modesdb;
        /// <summary>
        /// Экземпляры класса SizeDb
        /// </summary>
        private SizeDb m_sizedb;
        /// <summary>
        /// Экземпляры класса Tec
        /// </summary>
        private PanelContainerTec m_tecdb;
        
        private struct STATE_SOURCE
        {
            public string m_Text;

            public Color m_Color;
        }

        private enum INDEX_STATE : short { OK = 0, ERROR }

        private static STATE_SOURCE[] s_StateSources = new STATE_SOURCE[] {
            new STATE_SOURCE() { m_Text = @"Да", m_Color = Color.White }
            , new STATE_SOURCE() { m_Text = @"Нет", m_Color = Color.Red }
        };
        /// <summary>
        /// ???
        /// </summary>
        public static volatile int UPDATE_TIME,
            VALIDATE_ASKUE_TM;
        public static DateTime SERVER_TIME;//серверное время
        /// <summary>
        /// крайняя граница времени выполения задач
        /// </summary>
        static TimeSpan limTaskAvg = TimeSpan.FromSeconds(145);
        static TimeSpan limTask = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Создание и настройка таймера 
        /// для обновления данных на форме
        /// </summary>
        private void timerUp()
        {
            m_timerUpdate = new System.Timers.Timer();
            //m_timerUpdate.Enabled = true;
            m_timerUpdate.AutoReset = true;
            m_timerUpdate.Elapsed += new ElapsedEventHandler(UpdateTimer_Elapsed);
            m_timerUpdate.Interval = Convert.ToDouble(UPDATE_TIME);
            GC.KeepAlive(m_timerUpdate);
        }

        /// <summary>
        /// Обработчик события таймера
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void UpdateTimer_Elapsed(object source, ElapsedEventArgs e)
        {
            m_DataSource.Command();
        }

        /// <summary>
        /// Класс для обращения 
        /// к БД (чтение значений параметров для отображения)
        /// </summary>
        private class HDataSource : HHandlerDb
        {
            ConnectionSettings[] m_connSett;
            /// <summary>
            /// Перечисление типов состояний опроса данных
            /// </summary>
            protected enum State
            {
                ServerTime,
                Command,
                UpdateSource
            }
            /// <summary>
            /// Конструктор основной (без параметров)
            /// </summary>
            public HDataSource()
            {
                int err = -1;

                ConnectionSettings connSett = new ConnectionSettings(InitTECBase.getConnSettingsOfIdSource(ListenerId, FormMainBase.s_iMainSourceData, -1, out err).Rows[0], -1);

                m_connSett = new ConnectionSettings[2];//??? why number
                m_connSett[(int)CONN_SETT_TYPE.LIST_SOURCE] = connSett;
                m_connSett[(int)CONN_SETT_TYPE.CONFIG_DB] = FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett();
                m_connSett[(int)CONN_SETT_TYPE.CONFIG_DB].id = ConnectionSettings.UN_ENUMERABLE_ID - 6;

                //??? анализ err
                if (err < 0)
                    throw new Exception(@"PanelStatisticDiagnostic::HDataSource () - ctor () - ...");
                else
                    ;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void StartDbInterfaces()
            {
                if (m_dictIdListeners.ContainsKey(0) == false)
                    m_dictIdListeners.Add(0, new int[] { -1, -1 });

                register(0, (int)CONN_SETT_TYPE.LIST_SOURCE, m_connSett[(int)CONN_SETT_TYPE.LIST_SOURCE], m_connSett[(int)CONN_SETT_TYPE.LIST_SOURCE].name);
                //register(0, (int)CONN_SETT_TYPE.CONFIG_DB, m_connSett[(int)CONN_SETT_TYPE.CONFIG_DB], m_connSett[(int)CONN_SETT_TYPE.CONFIG_DB].name);
            }

            public override void ClearValues()
            {
            }

            /// <summary>
            /// Добавить состояния в набор для обработки
            /// данных 
            /// </summary>
            public void Command()
            {
                lock (m_lockState)
                {
                    ClearStates();

                    AddState((int)State.UpdateSource);
                    AddState((int)State.ServerTime);
                    AddState((int)State.Command);

                    Run(@"StatisticDiagnostic.HHandlerDb :: Command");
                }
            }

            /// <summary>
            /// Запросить результат для события
            /// </summary>
            /// <param name="state">Событие запроса</param>
            /// <returns>Признак отправления результата</returns>
            protected override int StateRequest(int state)
            {
                int iRes = 0;

                switch (state)
                {
                    case (int)State.ServerTime:
                        GetCurrentTimeRequest(DbInterface.DB_TSQL_INTERFACE_TYPE.MSSQL, m_dictIdListeners[0][(int)CONN_SETT_TYPE.LIST_SOURCE]);
                        actionReport(@"Получение времени с сервера БД - состояние: " + ((State)state).ToString());
                        break;
                    case (int)State.Command:
                        Request(m_dictIdListeners[0][(int)CONN_SETT_TYPE.LIST_SOURCE], @"SELECT * FROM [dbo].[Diagnostic]");
                        actionReport(@"Получение значений из БД - состояние: " + ((State)state).ToString());
                        break;
                    case (int)State.UpdateSource:
                        Request(m_dictIdListeners[0][(int)CONN_SETT_TYPE.LIST_SOURCE], @"SELECT * FROM [dbo].[v_CURR_ID_LINK_SOURCE_DATA_TM]");
                        actionReport(@"Обновление списка активных источников - состояние: " + ((State)state).ToString());
                        break;
                    default:
                        break;
                }

                return iRes;
            }

            /// <summary>
            /// Получить результат обработки события
            /// </summary>
            /// <param name="state">Событие для получения результата</param>
            /// <param name="error">Признак ошибки при получении результата</param>
            /// <param name="outobj">Результат запроса</param>
            /// <returns>Признак получения результата</returns>
            protected override int StateCheckResponse(int state, out bool error, out object table)
            {
                int iRes = 0;
                error = true;
                table = null;

                switch (state)
                {
                    case (int)State.ServerTime:
                        iRes = response(m_IdListenerCurrent, out error, out table);
                        break;
                    case (int)State.Command:
                        iRes = response(m_IdListenerCurrent, out error, out table);
                        break;
                    case (int)State.UpdateSource:
                        iRes = response(m_IdListenerCurrent, out error, out table);
                        break;
                    default:
                        break;
                }
                return iRes;
            }

            private static int _iListenerId;

            private static DbConnection _connConfigDb;

            public static int ListenerId { get { return _iListenerId; } }

            public bool IsRegisterConfogDb { get { return ListenerId > 0; } }

            public static void RegisterConfigDb(out int err)
            {
                // зарегистрировать соединение/получить идентификатор соединения
                _iListenerId = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");

                _connConfigDb = DbSources.Sources().GetConnection(_iListenerId, out err);
            }

            public static void UnregisterConfigDb()
            {
                DbSources.Sources().UnRegister(ListenerId);

                _connConfigDb = null;
                _iListenerId = -1;
            }

            public DataTable GetDiagnosticSource(out int err)
            {
                err = _connConfigDb == null ? -1 : 0;

                if (err == 0)
                    return DbTSQLInterface.Select(ref _connConfigDb, "SELECT * FROM DIAGNOSTIC_SOURCES", null, null, out err);
                else
                    return new DataTable();
            }

            public DataTable GetSources(out int err)
            {
                err = _connConfigDb == null ? -1 : 0;

                if (err == 0)
                    return DbTSQLInterface.Select(ref _connConfigDb, "SELECT * FROM SOURCE", null, null, out err);
                else
                    return new DataTable();
            }

            public DataTable GetListGTP(out int err)
            {
                err = _connConfigDb == null ? -1 : 0;

                if (err == 0)
                    return DbTSQLInterface.Select(ref _connConfigDb, "SELECT * FROM GTP_LIST", null, null, out err);
                else
                    return new DataTable();
            }

            public DataTable GetDiagnosticParameter(out int err)
            {
                err = _connConfigDb == null ? -1 : 0;

                if (err == 0)
                    return DbTSQLInterface.Select(ref _connConfigDb, "SELECT * FROM DIAGNOSTIC_PARAM", null, null, out err);
                else
                    return new DataTable();
            }

            public DataTable GetListTEC(out int err)
            {
                err = _connConfigDb == null ? -1 : 0;

                if (err == 0)
                    return InitTEC_200.getListTEC(ref _connConfigDb, false, new int[] { 0, 10 }, out err);
                else
                    return new DataTable();
            }

            /// <summary>
            /// Событие - получение данных 
            /// </summary>
            public event DelegateObjectFunc EvtRecievedTable;
            /// <summary>
            /// 
            /// </summary>
            public event DelegateObjectFunc EvtRecievedActiveSource;

            /// <summary>
            /// Обработка УСЕШНО полученного результата
            /// </summary>
            /// <param name="state">Состояние для результата</param>
            /// <param name="obj">Значение результата</param>
            /// <returns>Признак обработки результата</returns>
            protected override int StateResponse(int state, object table)
            {
                int iRes = 0;

                switch (state)
                {
                    case (int)State.ServerTime:
                        GetTimeServer((DateTime)(table as DataTable).Rows[0][0]);
                        break;
                    case (int)State.Command:
                        EvtRecievedTable(table);
                        break;
                    case (int)State.UpdateSource:
                        EvtRecievedActiveSource(table);
                        break;
                    default:
                        break;
                }

                //Проверить признак крайнего в наборе состояний для обработки
                if (isLastState(state) == true)
                    //Удалить все сообщения в строке статуса
                    ReportClear(true);
                else
                    ;

                return iRes;
            }

            /// <summary>
            /// Функция обратного вызова при возникновения ситуации "ошибка"
            ///  при обработке списка состояний
            /// </summary>
            /// <param name="state">Состояние при котором возникла ситуация</param>
            /// <param name="req">Признак результата выполнения запроса</param>
            /// <param name="res">Признак возвращения результата при запросе</param>
            /// <returns>Индекс массива объектов синхронизации</returns>
            protected override INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
            {
                INDEX_WAITHANDLE_REASON iRes = INDEX_WAITHANDLE_REASON.SUCCESS;

                errorReport(@"Получение значений из БД - состояние: " + ((State)state).ToString());

                return iRes;
            }

            /// <summary>
            /// Функция обратного вызова при возникновения ситуации "предупреждение"
            ///  при обработке списка состояний
            /// </summary>
            /// <param name="state">Состояние при котором возникла ситуация</param>
            /// <param name="req">Признак результата выполнения запроса</param>
            /// <param name="res">Признак возвращения результата при запросе</param>
            protected override void StateWarnings(int state, int req, int res)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        public PanelStatisticDiagnostic() : base (-1, -1)
        {
            initialize();
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="container"></param>
        public PanelStatisticDiagnostic(IContainer container) : base(-1, -1)
        {
            container.Add(this);

            initialize();
        }

        /// <summary>
        /// Инициализация подключения к БД
        /// и компонентов панели.
        /// </summary>
        /// <returns></returns>
        private int initialize()
        {
            int err = -1; //Признак выполнения метода/функции

            initializeLayoutStyle(8, 16);

            HDataSource.RegisterConfigDb(out err);

            // зарегистрировать синхронное соединение с БД_конфигурации
            m_DataSource = new HDataSource();
            // назначить обработчик события - получение данных
            m_listDiagnosticSource = new ListDiagnosticSource(m_DataSource.GetDiagnosticSource(out err));
            m_tableSourceList = m_DataSource.GetSources(out err);
            m_tableGTPList = m_DataSource.GetListGTP(out err);
            m_tableParamDiagnostic = m_DataSource.GetDiagnosticParameter (out err);
            m_tableTECList = m_DataSource.GetListTEC(out err);

            HDataSource.UnregisterConfigDb();

            m_tecdb = new PanelContainerTec();
            m_modesdb = new PanelContainerModes(m_listDiagnosticSource);
            m_taskdb = new PanelTask();
            m_sizedb = new SizeDb(m_listDiagnosticSource);

            InitializeComponent();

            return err;
        }

        /// <summary>
        /// Обработчик события - получение данных при запросе к БД
        /// </summary>
        /// <param name="table">Результат выполнения запроса - таблица с данными</param>
        private void dataSource_OnEvtRecievedTable(object table)
        {
            //m_tableSourceData = (DataTable)table;
            //// обновить значения
            //try {
            //    m_tecdb.Update();
            //    m_taskdb.Update();
            //    m_modesdb.Update();
            //    m_sizedb.LoadValues();
            //} catch (Exception e) {
            //    Logging.Logg().Exception(e, @"PanelStatisticDiagnostic::_OnEvtRecievedTable () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            //}
        }

        /// <summary>
        /// Обработчик события - получение данных при запросе к БД
        /// получение списка активных источников
        /// </summary>
        /// <param name="table">Результат выполнения запроса - таблица с данными</param>
        private void dataSource_OnEvtRecievedActiveSource(object table)
        {
            createListActiveSource((DataTable)table);
        }

        /// <summary>
        /// Получение серверного времени
        /// </summary>
        /// <param name="dtTime"></param>
        public static void GetTimeServer(DateTime dtTime)
        {
            SERVER_TIME = dtTime;
        }

        /// <summary>
        /// Загрузить данные из БД
        /// </summary>
        private void dbStart()
        {
            m_DataSource.Start();
            m_DataSource.StartDbInterfaces();
            //m_DataSource.EvtRecievedActiveSource += new DelegateObjectFunc(dataSource_OnEvtRecievedActiveSource);
            m_DataSource.EvtRecievedActiveSource += new DelegateObjectFunc(m_tecdb.Update);
            m_DataSource.EvtRecievedTable += new DelegateObjectFunc(dataSource_OnEvtRecievedTable);
            m_DataSource.EvtRecievedTable += new DelegateObjectFunc(m_tecdb.Update);
            m_DataSource.EvtRecievedTable += new DelegateObjectFunc(m_modesdb.Update);
            m_DataSource.EvtRecievedTable += new DelegateObjectFunc(m_taskdb.Update);
            m_DataSource.EvtRecievedTable += new DelegateObjectFunc(m_sizedb.Update);
        }

        /// <summary>
        /// Назначить делегаты по отображению сообщений в строке статуса
        /// </summary>
        /// <param name="ferr">Делегат для отображения в строке статуса ошибки</param>
        /// <param name="fwar">Делегат для отображения в строке статуса предупреждения</param>
        /// <param name="fact">Делегат для отображения в строке статуса описания действия</param>
        /// <param name="fclr">Делегат для удаления из строки статуса сообщений</param>
        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            m_DataSource.SetDelegateReport(ferr, fwar, fact, fclr);
        }

        /// <summary>
        /// Фукнция вызова старта программы
        /// (создание таймера и получения данных)
        /// </summary>
        public override void Start()
        {
            base.Start();
            timerUp();
            dbStart();
        }

        private struct DIAGNOSTIC_SOURCE
        {
            public int m_id
                , m_id_component;

            public string m_name_shr
                , m_description;
        }

        private class ListDiagnosticSource : List<DIAGNOSTIC_SOURCE>
        {
            public ListDiagnosticSource(DataTable tableDb)
            {
                foreach (DataRow r in tableDb.Rows)
                    Add(new DIAGNOSTIC_SOURCE() {
                        m_id = (int)r[@"ID"]
                        , m_id_component = r.Field<int>(@"Component")
                        , m_name_shr = r.Field<string>(@"NAME_SHR").Trim()
                        , m_description = r.Field<string>(@"DESCRIPTION").Trim()
                    });
            }
        }

        private ListDiagnosticSource m_listDiagnosticSource;

        private struct SOURCE
        {
            public int m_id;

            public string m_name_shr;
        }

        private class ListSource : List<SOURCE>
        {
            public ListSource(DataTable tableDb)
            {
                foreach (DataRow r in tableDb.Rows)
                    Add(new SOURCE()
                    {
                        m_id = r.Field<int>(@"ID")
                        ,
                        m_name_shr = r.Field<string>(@"NAME_SHR").Trim()
                    });
            }
        }

        private ListSource m_listSource;

        private struct DIAGNOSTIC_PARAMETER
        {
            public int m_id;

            public string m_name_shr
                , m_description
                , m_source_data;
        }

        private struct DIAGNOSTIC_DATA
        {
            public int m_id;

            public string m_value
                , m_dtValue
                , m_dtVerification;
        }

        private class ListDiagnosticParameter : List<DIAGNOSTIC_PARAMETER>
        {
            public ListDiagnosticParameter(DataTable tableDb)
            {
                foreach (DataRow r in tableDb.Rows)
                    Add(new DIAGNOSTIC_PARAMETER() {
                        m_id = r.Field<int>(@"ID")
                        , m_name_shr = r.Field<string>(@"NAME_SHR").Trim()
                        , m_description = r.Field<string>(@"DESCRIPTION").Trim()
                        , m_source_data = r.Field<string>(@"SOURCE_DATA").Trim()
                    });
            }
        }

        private void clear()
        {
            m_tecdb.Clear();
            m_modesdb.Clear();
            m_taskdb.Clear();
        }

        /// <summary>
        /// Остановка работы панели
        /// </summary>
        public override void Stop()
        {
            if (Started == true)
            {
                clear();

                stop();                

                base.Stop();
            }
        }

        /// <summary>
        /// Остановка работы таймера
        /// и обработки запроса к БД
        /// </summary>
        private void stop()
        {
            if (!(m_DataSource == null))
            {
                m_timerUpdate.Stop();
                m_DataSource.StopDbInterfaces();
                m_DataSource.Stop();
            }
        }

        /// <summary>
        /// Функция активация Вкладки
        /// </summary>
        /// <param name="activated">параметр</param>
        /// <returns>результат</returns>
        public override bool Activate(bool activated)
        {
            bool bRes = base.Activate(activated);

            if (activated == true) {
                m_timerUpdate.Start();
                m_DataSource.Command();
            } else
                if (!(m_timerUpdate == null))
                m_timerUpdate.Stop();

            return bRes;
        }

        /// <summary>
        /// Создание списка активных 
        /// источников СОТИАССО для всех ТЭЦ
        /// </summary>
        /// <param name="table">список тэц</param>
        private void createListActiveSource(DataTable table)
        {
            m_arrayActiveSource = new object[table.Rows.Count, 2];

            for (int i = 0; i < table.Rows.Count; i++)
                m_arrayActiveSource.SetValue(table.Rows[i]["ID_LINK_SOURCE_DATA_TM"], i, 0);

            int t = -1;
            int id;

            for (int j = 0; j < table.Rows.Count; j++) {
                do {
                    t++;
                    id = (int)m_tableSourceList.Rows[t][@"ID"];

                    if ((int)m_arrayActiveSource[j, 0] == 0)
                        break;
                    else
                        ;
                } while ((int)m_arrayActiveSource[j, 0] != id);

                m_arrayActiveSource.SetValue(m_tableSourceList.Rows[t][@"NAME_SHR"].ToString(), j, 1);
            }
        }

        /// <summary>
        /// Формирование строки запроса с параметром 
        /// активного источника СОТИАССО
        /// </summary>
        /// <param name="TM">новый параметр</param>
        /// <param name="tec">тэц</param>
        /// <returns>строка запроса</returns>
        static private string stringQuery(int TM, int tec)
        {
            string query = string.Empty;
            query = "update TEC_LIST set  ID_LINK_SOURCE_DATA_TM = " + TM + " where ID =" + tec;
            return query;
        }

        /// <summary>
        /// Функция на обновление парамтера активного 
        /// источника СОТИАССО в таблице TEC_LIST
        /// </summary>
        /// <param name="query">строка запроса</param>
        static void updateTecTM(string query)
        {
            int err = -1;
            int iListernID = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");

            DbConnection m_dbConn = DbSources.Sources().GetConnection(iListernID, out err);
            DbTSQLInterface.ExecNonQuery(ref m_dbConn, query, null, null, out err);
            DbSources.Sources().UnRegister(iListernID);
        }
    }
}