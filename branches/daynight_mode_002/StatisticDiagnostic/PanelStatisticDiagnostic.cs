﻿using HClassLibrary;
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
    /// Абстрактный класс "Панель Диагностика"
    /// </summary>
    public abstract partial class PanelDiagnostic : HPanelCommon
    {
        public PanelDiagnostic(string nameLabel)
            : base(-1, -1)
        {
            initialize();
        }

        /// <summary>
        /// Конструктор "Панель Диагностика"
        /// </summary>
        /// <param name="container">контейнер</param>
        public PanelDiagnostic(IContainer container)
            : base(container, -1, -1)
        {
            container.Add(this);

            initialize();
        }

        protected abstract void LoadValue();

        protected abstract void Activated(bool activated);

        protected abstract void ClearGrid();

        protected abstract void CreateForm();

        protected abstract string FormatTime(string datetime);
    }

    public abstract partial class PanelDiagnostic
    {
        private void initialize()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Метод "Инициализовать стиль макета"
        /// </summary>
        /// <param name="cols">столбцы</param>
        /// <param name="rows">строки</param>
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
            this.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;

            this.SuspendLayout();

            this.ResumeLayout(false);
        }

        #endregion
    }

    /// <summary>
    /// Класс для описания панели с информацией
    ///  по дианостированию состояния ИС
    /// </summary>
    public partial class PanelStatisticDiagnostic
    {
        private enum ID_CONTAINER_PANEL : short { UNKNOWN = -1
            //Область оценки работоспособности источников значений (АИИСКУЭ, СОТИАССО)
            , TEC
            //Область оценки работоспособности источников значений ПБР (Модес-центр, терминал)
            , MODES
            //Область оценки работоспособности компонентов вспомогательного ПО
            , TASK
            //Область оценки размеров БД
            , SIZE
            ,
        }

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
            //Задать диапазон столбцов и строк
            this.Controls.Add(m_tecdb, 0, 0); this.SetColumnSpan(m_tecdb, 8); this.SetRowSpan(m_tecdb, m_Mode == Mode.DEFAULT ? 6 : 16);
            //Если режим по умолчанию
            if (m_Mode == Mode.DEFAULT) {
                this.Controls.Add(m_modesdb, 0, 6); this.SetColumnSpan(m_modesdb, 8); this.SetRowSpan(m_modesdb, 6);
                this.Controls.Add(m_taskdb, 0, 12); this.SetColumnSpan(m_taskdb, 5); this.SetRowSpan(m_taskdb, 4);
                this.Controls.Add(m_sizedb, 5, 12); this.SetColumnSpan(m_sizedb, 3); this.SetRowSpan(m_sizedb, 4);
            } else
                ;

            this.ResumeLayout();

            initializeLayoutStyle();
        }

        #endregion
    }

    /// <summary>
    /// Класс для описания панели с информацией
    ///  по дианостированию состояния ИС
    /// </summary>
    public partial class PanelStatisticDiagnostic : PanelStatistic
    {
        //Перечисление "Режим: по умолчанию, только источник"
        public enum Mode : short { DEFAULT, SOURCE_ONLY }

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
        /// Структура для хранения получаемых значений из таблицы-результата запроса
        /// </summary>
        private struct Values
        {
            /// <summary>
            /// Метка времени значения
            /// </summary>
            public DateTime m_dtValue;
            /// <summary>
            /// Значение одного из диагностических параметров
            /// </summary>
            public object m_value;

            public string m_strLink;

            public string m_name_shr;
        }

        public Mode m_Mode { get; set; }
        /// <summary>
        /// Экземпляр класса 
        ///  для подключения/отправления/получения запросов к БД
        /// </summary>
        private HDataSource m_DataSource;
        /// <summary>
        /// Таймер для обновления панелей
        /// </summary>
        private System.Timers.Timer m_timerUpdate;
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
        /// <summary>
        /// Характеристики ячейки
        /// </summary>
        private struct CELL_STATE
        {
            /// <summary>
            /// Текст(содержимое) ячейки
            /// </summary>
            public string m_Text;
            /// <summary>
            /// Цвет фона ячейки
            /// </summary>
            public Color m_Color;

            public string m_Detail;
        }
        /// <summary>
        /// Перечисление - индексы для доступа к элементам статического массива 's_CellState'
        /// </summary>
        private enum INDEX_CELL_STATE : short { OK = 0, WARNING, ERROR, UNKNOWN, DISABLED }

        private static CELL_STATE[] s_CellState = new CELL_STATE[] {
            new CELL_STATE() { m_Text = @"Да", m_Color = HDataGridViewTables.s_dgvCellStyles == null ? SystemColors.Window : HDataGridViewTables.s_dgvCellStyles[(int)HDataGridViewTables.INDEX_CELL_STYLE.COMMON].BackColor }
            , new CELL_STATE() { m_Text = string.Empty, m_Color = HDataGridViewTables.s_dgvCellStyles == null ? Color.Yellow : HDataGridViewTables.s_dgvCellStyles[(int)HDataGridViewTables.INDEX_CELL_STYLE.WARNING].BackColor, m_Detail = @"Продолжительное выполнение" }
            , new CELL_STATE() { m_Text = @"Нет", m_Color = HDataGridViewTables.s_dgvCellStyles == null ? Color.Red : HDataGridViewTables.s_dgvCellStyles[(int)HDataGridViewTables.INDEX_CELL_STYLE.ERROR].BackColor, m_Detail = @"Превышено ожидание" }
            , new CELL_STATE() { m_Text = @"н/д", m_Color = Color.LightGray }
            , new CELL_STATE() { m_Text = string.Empty, m_Color = Color.DarkGray, m_Detail = @"Запрещено" }
        };
        /// <summary>
        /// К полям UPDATE_TIME и  VALIDATE_ASKUE_TM обращается несколько потоков
        /// </summary>
        public static volatile int UPDATE_TIME,
            VALIDATE_ASKUE_TM;
        //Cерверное время
        public static DateTime SERVER_TIME;

        /// <summary>
        /// Интерфейс "Строки таблицы панели Диагностика"
        /// </summary>
        private interface IDataGridViewDiagnosticRow
        {
            //Автоматически реализуемое свойство
            string Name { get; set; }
        }

        /// <summary>
        /// Элемент интерфейса - строка в представлении для отображения данных
        /// </summary>
        private abstract class DataGridViewDiagnosticRow : DataGridViewRow, IDataGridViewDiagnosticRow
        {
            public abstract string Name { get; set; }

            /// <summary>
            /// Метод "Форматировать значение даты времени"
            /// </summary>
            /// <param name="indxCell">Номер(индекс) столбца</param>
            /// <param name="dtFormated">Значение даты/времени для форматирования</param>
            /// <returns>Строка с датой/временем</returns>
            protected string formatDateTime(DateTime dtFormated)
            {
                string strRes = string.Empty;

                if (SERVER_TIME.Date > dtFormated.Date)
                    strRes = dtFormated.ToString(@"dd.MM.yyyy HH:mm:ss");
                else
                    strRes = dtFormated.ToString(@"HH:mm:ss");

                return strRes;
            }
            /// <summary>
            /// Признак актуальности даты/времени
            /// </summary>
            /// <param name="indxCell">Номер(индекс) столбца</param>
            /// <param name="dtChecked">Значение даты/времени для проверки</param>
            /// <returns>Признак актуальности</returns>
            protected abstract INDEX_CELL_STATE isRelevanceDateTime(int iColumn, DateTime dtChecked);

            protected abstract INDEX_CELL_STATE isRelevanceValue(int iColumn, double value);
        }

        /// <summary>
        /// Создание и настройка таймера 
        /// для обновления данных на форме
        /// </summary>
        private void timerUp()
        {
            m_timerUpdate = new System.Timers.Timer();
            //m_timerUpdate.Enabled = true;
            m_timerUpdate.AutoReset = true;
            m_timerUpdate.Elapsed += new ElapsedEventHandler(updateTimer_OnElapsed);
            m_timerUpdate.Interval = Convert.ToDouble(UPDATE_TIME);
            GC.KeepAlive(m_timerUpdate);
        }

        /// <summary>
        /// Обработчик события таймера
        /// </summary>
        /// <param name="source">Объект, инициировавший событие(таймер)</param>
        /// <param name="e">Аргумент события</param>
        private void updateTimer_OnElapsed(object source, ElapsedEventArgs e)
        {
            m_DataSource.Command();
        }

        /// <summary>
        /// Инициировать немедленно событие таймера
        /// </summary>
        /// <param name="result">Контекст асинхронного вызова</param>
        private void updateTimer_StartElapsed (IAsyncResult result)
        {
            ElapsedEventHandler handler = result.AsyncState as ElapsedEventHandler;
            if (Equals (handler, null) == false) {
                handler.EndInvoke (result);
            } else
                ;
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

                _filterListTEC = new int[] { 0, 10 };

                ConnectionSettings connSett = new ConnectionSettings(InitTECBase.getConnSettingsOfIdSource(ListenerId, FormMainBase.s_iMainSourceData, -1, out err).Rows[0], -1);

                m_connSett = new ConnectionSettings[2];//??? why number
                m_connSett[(int)CONN_SETT_TYPE.LIST_SOURCE] = connSett;
                m_connSett[(int)CONN_SETT_TYPE.CONFIG_DB] = FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett();
                m_connSett[(int)CONN_SETT_TYPE.CONFIG_DB].id = ConnectionSettings.UN_ENUMERABLE_ID - 6;

                //??? разбор err
                if (err < 0)
                    throw new Exception(@"PanelStatisticDiagnostic::HDataSource () - ctor () - ...");
                else
                    ;
            }

            /// <summary>
            /// Установить необходимые для работы соединения с БД
            /// </summary>
            public override void StartDbInterfaces()
            {
                if (m_dictIdListeners.ContainsKey(0) == false)
                    m_dictIdListeners.Add(0, new int[] { -1, -1 });
                else
                    ;

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
            /// <summary>
            /// Зарегистрировать(установить) временное соединение с БД конфигурации
            /// </summary>
            /// <param name="err">Признак ошибки при выполнении операции</param>
            public static void RegisterConfigDb(out int err)
            {
                // зарегистрировать соединение/получить идентификатор соединения
                _iListenerId = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");

                _connConfigDb = DbSources.Sources().GetConnection(_iListenerId, out err);
            }
            /// <summary>
            /// Отменить регистрацию(разорвать) соединения с БД конфигурации
            /// </summary>
            public static void UnregisterConfigDb()
            {
                DbSources.Sources().UnRegister(ListenerId);

                _connConfigDb = null;
                _iListenerId = -1;
            }
            /// <summary>
            /// Возвратить таблицу с контролируемыми источниками данных
            /// </summary>
            /// <param name="err">Признак ошибки при выполнении операции</param>
            /// <returns>Таблица с данными - результат запроса</returns>
            public DataTable GetDiagnosticSource(out int err)
            {
                err = _connConfigDb == null ? -1 : 0;

                if (err == 0)
                    return DbTSQLInterface.Select(ref _connConfigDb, "SELECT * FROM DIAGNOSTIC_SOURCES", null, null, out err);
                else
                    return new DataTable();
            }
            /// <summary>
            /// Возвратить таблицу со всеми источниками данных
            /// </summary>
            /// <param name="err">Признак ошибки при выполнении операции</param>
            /// <returns>Таблица с данными - результат запроса</returns>
            public DataTable GetSource(out int err)
            {
                err = _connConfigDb == null ? -1 : 0;

                if (err == 0)
                    return DbTSQLInterface.Select(ref _connConfigDb, "SELECT * FROM SOURCE", null, null, out err);
                else
                    return new DataTable();
            }
            ///// <summary>
            ///// Возвратить таблицу с перечнем ГТП
            ///// </summary>
            ///// <param name="err">Признак ошибки при выполнении операции</param>
            ///// <returns>Таблица с данными - результат запроса</returns>
            //public DataTable GetListGTP(out int err)
            //{
            //    err = _connConfigDb == null ? -1 : 0;

            //    if (err == 0)
            //        return DbTSQLInterface.Select(ref _connConfigDb, "SELECT * FROM GTP_LIST", null, null, out err);
            //    else
            //        return new DataTable();
            //}
            /// <summary>
            /// Возвратить таблицу с перечнем парметров дигностики
            /// </summary>
            /// <param name="err">Признак ошибки при выполнении операции</param>
            /// <returns>Таблица с данными - результат запроса</returns>
            public DataTable GetDiagnosticParameter(out int err)
            {
                err = _connConfigDb == null ? -1 : 0;

                if (err == 0)
                    return DbTSQLInterface.Select(ref _connConfigDb, "SELECT * FROM DIAGNOSTIC_PARAM", null, null, out err);
                else
                    return new DataTable();
            }
            /// <summary>
            /// Фильтр для выборки только ТЭЦ (без ЛК)
            /// </summary>
            private readonly int[] _filterListTEC;
            /// <summary>
            /// Возвратить таблицу с перечнем ТЭЦ
            /// </summary>
            /// <param name="err">Признак ошибки при выполнении операции</param>
            /// <returns>Таблица с данными - результат запроса</returns>
            public DataTable GetDataTableTEC(out int err)
            {
                err = _connConfigDb == null ? -1 : 0;

                if (err == 0)
                    return InitTEC_200.getListTEC(ref _connConfigDb, false, _filterListTEC, out err);
                else
                    return new DataTable();
            }
            /// <summary>
            /// Возвратить список ТЭЦ
            /// </summary>
            /// <param name="err">Признак ошибки при выполнении операции</param>
            /// <returns>Список ТЭЦ - результат обработки запроса</returns>
            public List<TEC> GetListTEC(out int err)
            {
                err = _connConfigDb == null ? -1 : 0;

                if (err == 0)
                    return new InitTEC_200(_iListenerId, true, _filterListTEC, false).tec; //.getListTEC(ref _connConfigDb, false, _filterListTEC, out err);
                else
                    return new List<TEC> ();
            }
            /// <summary>
            /// Изменить идентификатор активного источника данных (СОТИАССО)
            /// </summary>
            /// <param name="ev">Параметры для изменения, например новый идентификатор источника данных</param>
            public void UpdateSourceId(PanelContainerTec.EventSourceIdChangedArgs ev)
            {
                int err = -1;

                RegisterConfigDb(out err);

                err = _connConfigDb == null ? -1 : 0;

                if (err == 0)
                    DbTSQLInterface.ExecNonQuery(ref _connConfigDb
                        , string.Format("UPDATE [TEC_LIST] SET  [ID_LINK_SOURCE_DATA_TM] = {0} WHERE [ID] = {1}", ev.m_iNewSourceId, ev.m_tec.m_id)
                        , null, null, 
                        out err);
                else
                    ;

                UnregisterConfigDb();
            }

            /// <summary>
            /// Событие - получение данных 
            /// </summary>
            public event DelegateObjectFunc EvtRecievedTable;
            /// <summary>
            /// Событие - получение активных источников
            /// </summary>
            public event DelegateObjectFunc EvtRecievedActiveSource;

            /// <summary>
            /// Обработка УСПЕШНО полученного результата
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

        private const int COUNT_LAYOUT_COLUMN = 8
            , COUNT_LAYOUT_ROW = 16;

        /// <summary>
        /// constructor
        /// </summary>
        public PanelStatisticDiagnostic(Mode mode = Mode.DEFAULT)
            : base (MODE_UPDATE_VALUES.AUTO, COUNT_LAYOUT_COLUMN, COUNT_LAYOUT_ROW)
        {
            m_Mode = mode;

            initialize();
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="container">Контейнер для панели</param>
        public PanelStatisticDiagnostic(IContainer container, Mode mode = Mode.DEFAULT)
            : base(MODE_UPDATE_VALUES.AUTO, COUNT_LAYOUT_COLUMN, COUNT_LAYOUT_ROW)
        {
            container.Add(this);

            m_Mode = mode;

            initialize();
        }

        /// <summary>
        /// Инициализация подключения к БД
        /// и компонентов панели.
        /// </summary>
        /// <returns>Признак результата(успех/ошибка) выполнения метода</returns>
        private int initialize()
        {
            int err = -1; //Признак выполнения метода/функции
            List<TEC> listTEC;
            ListDiagnosticParameter listDiagnosticParameter;
            ListDiagnosticSource listDiagnosticSource;

            HDataSource.RegisterConfigDb(out err);

            // зарегистрировать синхронное соединение с БД_конфигурации
            m_DataSource = new HDataSource();

            listTEC = m_DataSource.GetListTEC(out err);

            listDiagnosticSource = new ListDiagnosticSource(m_DataSource.GetDiagnosticSource(out err));

            listDiagnosticParameter = new ListDiagnosticParameter(m_DataSource.GetDiagnosticParameter(out err));

            //m_tableTECList = m_DataSource.GetDataTableTEC(out err);

            m_tecdb = new PanelContainerTec(listTEC
                , listDiagnosticParameter.FindAll(item => { return item.m_id_container.Equals(ID_CONTAINER_PANEL.TEC.ToString()); })
                , panelContainerTec_onSourceIdChanged);
            // проверить режим отображения панели
            if (m_Mode == Mode.DEFAULT) {
            // только в режиме по умолчанию
                m_modesdb = new PanelContainerModes(listTEC
                    , listDiagnosticParameter.FindAll(item => { return item.m_id_container.Equals(ID_CONTAINER_PANEL.MODES.ToString()); })
                    , listDiagnosticSource);
                m_taskdb = new PanelTask(listDiagnosticSource);
                m_sizedb = new SizeDb(listDiagnosticSource);
            } else
                ;

            HDataSource.UnregisterConfigDb();

            InitializeComponent();

            return err;
        }

        /// <summary>
        /// Делегат для изменения идентификатора источника данных
        /// </summary>
        /// <param name="obj">Объект в контексте которого был вызван делегат</param>
        /// <param name="ev">Параметры для изменения идентикатора источника данных</param>
        private void panelContainerTec_onSourceIdChanged(object obj, PanelContainerTec.EventSourceIdChangedArgs ev)
        {
            PanelContainerTec panelContainerTec = obj as PanelContainerTec;
            TEC tec = ev.m_tec;
            CONN_SETT_TYPE connSettType = ev.m_connSettType;
            int iNewSourceId = ev.m_iNewSourceId;

            m_DataSource.UpdateSourceId(ev);

            m_DataSource.Command();
        }

        /// <summary>
        /// Обработчик события - получение данных при запросе к БД
        /// </summary>
        /// <param name="table">Результат выполнения запроса - таблица с данными</param>
        private void dataSource_OnEvtRecievedTable(object table)
        {
        }

        /// <summary>
        /// Обработчик события - получение данных при запросе к БД
        /// получение списка активных источников
        /// </summary>
        /// <param name="table">Результат выполнения запроса - таблица с данными</param>
        private void dataSource_OnEvtRecievedActiveSource(object table)
        {
        }

        /// <summary>
        /// Получение серверного времени
        /// </summary>
        /// <param name="dtTime">Дата/время, полученное от сервера (MS SQLServer, T-SQL  запрос)</param>
        public static void GetTimeServer(DateTime serverTime)
        {
            SERVER_TIME = serverTime;
        }

        /// <summary>
        /// Загрузить данные из БД
        /// </summary>
        private void dbStart()
        {
            m_DataSource.Start();
            m_DataSource.StartDbInterfaces();
            // ??? самостоятельная обработка не требуется
            //m_DataSource.EvtRecievedActiveSource += new DelegateObjectFunc(dataSource_OnEvtRecievedActiveSource);
            //m_DataSource.EvtRecievedTable += new DelegateObjectFunc(dataSource_OnEvtRecievedTable);
            // обработчики события получения данных
            m_DataSource.EvtRecievedActiveSource += new DelegateObjectFunc(m_tecdb.Update);
            m_DataSource.EvtRecievedTable += new DelegateObjectFunc(m_tecdb.Update);
            if (m_Mode == Mode.DEFAULT) {
                m_DataSource.EvtRecievedTable += new DelegateObjectFunc(m_modesdb.Update);
                m_DataSource.EvtRecievedTable += new DelegateObjectFunc(m_taskdb.Update);
                m_DataSource.EvtRecievedTable += new DelegateObjectFunc(m_sizedb.Update);
            } else
                ;            
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

            public ListDiagnosticSource(IEnumerable<DIAGNOSTIC_SOURCE> list) : base (list) { }

            public ListDiagnosticSource() : base() { }
        }

        //private ListDiagnosticSource m_listDiagnosticSource;        

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

        private struct DIAGNOSTIC_PARAMETER
        {
            public int m_id;

            public string m_id_container
                , m_name_shr
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
                        , m_id_container = r.Field<string>(@"ID_CONTAINER_PANEL").Trim()
                        , m_name_shr = r.Field<string>(@"NAME_SHR").Trim()
                        , m_description = r.Field<string>(@"DESCRIPTION")?.Trim()
                        , m_source_data = r.Field<string>(@"SOURCE_DATA")?.Trim()
                    });
            }

            public ListDiagnosticParameter(IEnumerable<DIAGNOSTIC_PARAMETER> list) : base(list) { }

            public ListDiagnosticParameter() : base() { }
        }

        private void clear()
        {
            m_tecdb.Clear();
            if (m_Mode == Mode.DEFAULT) {
                m_modesdb.Clear();
                m_taskdb.Clear();
                //m_sizedb.Clear();
            } else
                ;
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
        /// Функция активация вкладки
        /// </summary>
        /// <param name="activated">параметр</param>
        /// <returns>Признак результата: изменилось состояние/не_изменилось)</returns>
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

        public override void UpdateGraphicsCurrent (int type)
        {
            ElapsedEventHandler handlerTimerElapsed = new ElapsedEventHandler (updateTimer_OnElapsed);
            handlerTimerElapsed.BeginInvoke (this, null, new AsyncCallback (updateTimer_StartElapsed), handlerTimerElapsed);
        }

        private class DataGridViewDiagnostic : System.Windows.Forms.DataGridView
        {
            public DataGridViewDiagnostic ()
            {
            }

            public override Color BackColor
            {
                get
                {
                    return base.BackColor;
                }

                set
                {
                    base.BackColor = value;

                    s_CellState [(int)INDEX_CELL_STATE.OK].m_Color = value == SystemColors.Control ? SystemColors.Window : value;

                    //for (int j = 0; j < ColumnCount; j++)
                    //    for (int i = 0; i < RowCount; i++)
                    //        Rows [i].Cells [j].Style.BackColor = s_CellState [(int)INDEX_CELL_STATE.OK].m_Color;
                }
            }
        }
    }
}