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
        /// <summary>
        /// Класс для обработки запросов к источникам данных
        /// </summary>
        private class HandlerDbSignalValue : HHandlerDb
        {
            private IEnumerable<CONN_SETT_TYPE> _types = new List<CONN_SETT_TYPE>() { CONN_SETT_TYPE.DATA_AISKUE, CONN_SETT_TYPE.DATA_SOTIASSO };

            private struct KEY
            {
                public int _current_id_tec;

                public CONN_SETT_TYPE _current_type;

                public int _current_index;
            }

            private static KEY _key = new KEY() { _current_id_tec = -1, _current_type = CONN_SETT_TYPE.UNKNOWN, _current_index = -1 };

            public struct USER_DATE
            {
                public int UTC_OFFSET;

                public DateTime Value;
            }

            private USER_DATE _userDate;
            public USER_DATE UserDate
            {
                get { return _userDate; }

                set {
                    // при 1-ом присваивании значения - опрос не выполняется
                    bool bRequest = false;

                    if (_userDate.Equals(value) == false) {
                        // проверить 1-ая установка значения? Или повторная
                        bRequest = _userDate.Equals(DateTime.MinValue) == true;

                        _userDate = value;

                        if (bRequest == true)
                            Request();
                        else
                            ;
                    } else
                        ;
                }
            }
            /// <summary>
            /// Словарь с параметрами соединения с БД
            ///  для всех типов источников данных (АИИСКУЭ, СОТИАССО)
            ///  для всех ТЭЦ из списка в аргументе конструктора
            /// </summary>
            private Dictionary<int, Dictionary<CONN_SETT_TYPE, ConnectionSettings>> m_dictConnSett;
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
                /// Индекс 30-ти мин интервала
                /// </summary>
                public int index_stamp;
                /// <summary>
                /// Значение
                /// </summary>
                public float value;
                /// <summary>
                /// Статус или качество/достоверность значения
                /// </summary>
                public short quality;
            }

            public /*struct*/ class VALUES
            {
                public DateTime serverTime;

                public IList<VALUE> m_valuesHours;

                //public void SetServerTime(DateTime serverTime)
                //{
                //    this.serverTime = serverTime;
                //}
            }
            /// <summary>
            /// Конструктор - основной (с аргументами)
            /// </summary>
            /// <param name="iListenerConfigDbId">Идентификатор установленного соединения с БД конфигурации</param>
            /// <param name="listTEC">Список ТЭЦ</param>
            public HandlerDbSignalValue(int iListenerConfigDbId, IEnumerable<TEC> listTEC)
                : base()
            {
                int err = -1
                    , id_tec = -1
                    , id_source = -1;
                DbConnection dbConn;

                _key._current_type = CONN_SETT_TYPE.UNKNOWN;
                _key._current_id_tec = -1;

                m_dictConnSett = new Dictionary<int, Dictionary<CONN_SETT_TYPE, ConnectionSettings>>();
                // в ~ от списка ТЭЦ инициализация словаря с параметрами соединения с БД
                dbConn = DbSources.Sources().GetConnection(iListenerConfigDbId, out err);

                if (err == 0)
                    foreach (TEC tec in listTEC) {
                        id_tec = tec.m_id;

                        if (m_dictConnSett.ContainsKey(id_tec) == false)
                            m_dictConnSett.Add(id_tec, new Dictionary<CONN_SETT_TYPE, ConnectionSettings>());
                        else
                            ;

                        foreach (CONN_SETT_TYPE type in _types) {
                            id_source = id_tec * 10
                                + (type == CONN_SETT_TYPE.DATA_AISKUE ? 1
                                    : type == CONN_SETT_TYPE.DATA_SOTIASSO ? 2
                                        : -1); //??? "-1" - ошибка

                            if (m_dictConnSett[id_tec].ContainsKey(type) == false) {
                                m_dictConnSett[id_tec].Add(type
                                    , new ConnectionSettings(InitTECBase.getConnSettingsOfIdSource(iListenerConfigDbId, id_source, -1, out err)?.Rows[0], -1));

                                if (!(err == 0)) {
                                    Logging.Logg().Error(string.Format(@"HandlerDbSignalValue::ctor () - ошибка инициализации источника данных {0} для ТЭЦ.ID={1}, идентификатор источника данных={2}..."
                                            , type, tec.m_id, id_source)
                                        , Logging.INDEX_MESSAGE.NOT_SET);

                                    err = 0;
                                } else
                                    ;
                            } else
                                ;
                        }
                    } else
                    throw new Exception(string.Format(@"HandlerDbSignalValue::ctor () - ошибка при получения объекта с соединением с БД конфигурации по идентификатору в аргументе..."));
                //// на этапе отладки (без БД конфигурации)
                //m_dictConnSett = new Dictionary<CONN_SETT_TYPE, Dictionary<int, ConnectionSettings>> {
                //    { CONN_SETT_TYPE.DATA_AISKUE, new Dictionary<int, ConnectionSettings>() {
                //            { 1, new ConnectionSettings() }
                //            , { 2, new ConnectionSettings() }
                //            , { 3, new ConnectionSettings() }
                //            , { 4, new ConnectionSettings() }
                //            , { 5, new ConnectionSettings() }
                //        }
                //    }
                //    , { CONN_SETT_TYPE.DATA_SOTIASSO, new Dictionary<int, ConnectionSettings>() {
                //            { 1, new ConnectionSettings() }
                //            , { 2, new ConnectionSettings() }
                //            , { 3, new ConnectionSettings() }
                //            , { 4, new ConnectionSettings() }
                //            , { 5, new ConnectionSettings() }
                //        }
                //    }
                //};

                request_handlers = new Dictionary<StatesMachine, Func<string>>() { { StatesMachine.LIST_SIGNAL, getListSignalRequest }, { StatesMachine.VALUES, getValuesRequest } };
                response_handlers = new Dictionary<StatesMachine, Func<DataTable, bool>>() { { StatesMachine.LIST_SIGNAL, getListSignalResponse }, { StatesMachine.VALUES, getValuesResponse } };

                Values = new /*DictionaryValues*/Dictionary<CONN_SETT_TYPE, VALUES>();
                _dictSignals = new Dictionary<CONN_SETT_TYPE, IList<SIGNAL>>();
                foreach (CONN_SETT_TYPE type in _types) {
                    Values.Add(type, new VALUES() { serverTime = DateTime.MinValue, m_valuesHours = new List<VALUE>() });

                    _dictSignals.Add(type, new List<SIGNAL>());
                }
            }

            private static Dictionary<StatesMachine, Func<string>> request_handlers;

            private static Dictionary<StatesMachine, Func<DataTable, bool>> response_handlers;

            //public class DictionaryValues : Dictionary<CONN_SETT_TYPE, VALUES>
            //{
            //    //public void SetServerTime(CONN_SETT_TYPE type, DateTime serverTime)
            //    //{
            //    //    this[type].SetServerTime(serverTime);
            //    //}
            //}

            public /*DictionaryValues*/Dictionary<CONN_SETT_TYPE, VALUES> Values;

            public IntDelegateIntIntFunc UpdateGUI_Fact;

            private Dictionary<CONN_SETT_TYPE, IList<SIGNAL>> _dictSignals;
            public Dictionary<CONN_SETT_TYPE, IList<SIGNAL>> Signals { get { return _dictSignals; } }
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
                if (!(_key._current_id_tec == id)) {
                    _key._current_id_tec = id;

                    ClearValues();

                    //Поток запроса значений для 
                    new Thread(new ParameterizedThreadStart((param) => {
                        int indxEv = -1;
                        // чтобы в 1-ый проход "пройти" WaitHandle.WaitAny и перейти к выполнению запроса
                        ((AutoResetEvent)m_waitHandleState[(int)INDEX_WAITHANDLE_REASON.SUCCESS]).Set();

                        for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON; i++)
                            ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

                        foreach (CONN_SETT_TYPE type in _types) {
                            indxEv = WaitHandle.WaitAny(m_waitHandleState);
                            if (indxEv == (int)INDEX_WAITHANDLE_REASON.BREAK)
                                // прекратить выполнение досрочно
                                break;
                            else {
                                if (!(indxEv == (int)INDEX_WAITHANDLE_REASON.SUCCESS))
                                    // в сигнальное состояние установлен отличный от BREAK, SUCCESS объект синхронизации
                                    // восстановить его состояние для очередной итерации
                                    ((ManualResetEvent)m_waitHandleState[indxEv]).Reset();
                                else
                                    ;

                                ClearStates();

                                _key._current_type = type;

                                getListSignals(); //getvalues();
                            }
                        }
                    })).Start();
                } else
                    ;
            }

            public void Request()
            {

            }
            /// <summary>
            /// Отправить запрос на получение значений сигналов АИИСКУЭ, СОТИАССО
            /// </summary>
            public void Request(CONN_SETT_TYPE type, int indx)
            {
                clearValues(type, false);
                ClearStates();

                _key._current_type = type;
                _key._current_index = indx;

                getValues();
            }

            private void getValues()
            {
                AddState((int)StatesMachine.SERVER_TIME);
                AddState((int)StatesMachine.VALUES);

                Run(string.Format(@"HandlerDbSignalValues::GetValues (type={0}, indx={1}) - KKS_CODE={2}...", _key._current_type, _key._current_index, Signals[_key._current_type].ElementAt(_key._current_index).kks_code));
            }

            public override void Start()
            {
                base.Start();

                StartDbInterfaces();
            }

            //public override void Stop()
            //{
            //    StopDbInterfaces();

            //    base.Stop();
            //}

            public override bool Activate(bool active)
            {
                bool bRes = base.Activate(active);

                if ((bRes == true)
                    && (IsFirstActivated == true))
                    ;
                else
                    ;

                return bRes;
            }
            /// <summary>
            /// Очистить полученные ранее значения
            /// </summary>
            public override void ClearValues()
            {
                foreach (CONN_SETT_TYPE type in _types)
                    clearValues(type, true);
            }

            private void clearValues(CONN_SETT_TYPE type, bool bIsSignalClear)
            {
                Values[type].serverTime = DateTime.MinValue;
                Values[type].m_valuesHours.Clear();

                if (bIsSignalClear == true)
                    _dictSignals[type].Clear();
                else
                    ;
            }
            /// <summary>
            /// Установить/зарегистрировать соединения с необходимыми источниками данных
            /// </summary>
            public override void StartDbInterfaces()
            {
                foreach (KeyValuePair<int, Dictionary<CONN_SETT_TYPE, ConnectionSettings>> pair in m_dictConnSett) {
                    foreach (CONN_SETT_TYPE type in pair.Value.Keys) {
                        if (m_dictIdListeners.ContainsKey((int)pair.Key) == false) {
                            m_dictIdListeners.Add((int)pair.Key, new int[(int)_types.Max() + 1]);

                            for (int j = 0; j < m_dictIdListeners[(int)pair.Key].Length; j++)
                                m_dictIdListeners[(int)pair.Key][j] = -1;
                            //m_dictIdListeners[(int)pair.Key].SetValue(-1, ...);
                        } else
                            ;

                        register((int)pair.Key, (int)type, pair.Value[type], pair.Value[type].name);
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
                errorReport(string.Format(@"HandlerDbStateValue::StateErrors(type={0}) - state={1} ...", _key._current_type, (StatesMachine)state));

                return INDEX_WAITHANDLE_REASON.ERROR;
            }

            protected override int StateRequest(int state)
            {
                INDEX_WAITHANDLE_REASON indxReasonRes =
                    //!(IndexDbSource < 0)
                    !(_key._current_type == CONN_SETT_TYPE.UNKNOWN)
                        ? INDEX_WAITHANDLE_REASON.SUCCESS : INDEX_WAITHANDLE_REASON.ERROR;

                string query = string.Empty;

                //actionReport(string.Format(@"HandlerDbStateValue::StateRequest(type={0}) - state={1}, IndexDbSource={2} ...", _key._current_type, (StatesMachine)state, IndexDbSource));
                actionReport(string.Format(@"HandlerDbStateValue::StateRequest(type={0}) - state={1} ...", _key._current_type, (StatesMachine)state));

                if (indxReasonRes == INDEX_WAITHANDLE_REASON.SUCCESS)
                    switch ((StatesMachine)state) {
                        case StatesMachine.SERVER_TIME:
                            GetCurrentTimeRequest(DbInterface.DB_TSQL_INTERFACE_TYPE.MSSQL, m_dictIdListeners[_key._current_id_tec][(int)_key._current_type]);
                            break;
                        case StatesMachine.LIST_SIGNAL:
                        case StatesMachine.VALUES:
                            indxReasonRes = (string.IsNullOrEmpty(query = request_handlers[(StatesMachine)state]()) == false)
                                ? INDEX_WAITHANDLE_REASON.SUCCESS : INDEX_WAITHANDLE_REASON.ERROR;

                            if (indxReasonRes == INDEX_WAITHANDLE_REASON.SUCCESS)
                                Request(m_dictIdListeners[_key._current_id_tec][(int)_key._current_type], query);
                            else
                                ;
                            break;
                        default:
                            break;
                    } else
                    ;

                return (int)indxReasonRes;
            }

            protected override int StateResponse(int state, object obj)
            {
                INDEX_WAITHANDLE_REASON indxReasonRes = !(_key._current_type == CONN_SETT_TYPE.UNKNOWN) ? INDEX_WAITHANDLE_REASON.SUCCESS : INDEX_WAITHANDLE_REASON.ERROR;

                DataTable tableResponse = null;

                //actionReport(string.Format(@"HandlerDbStateValue::StateResponse(type={0}) - state={1}, IndexDbSource={2} ...", _key._current_type, (StatesMachine)state, IndexDbSource));

                if (indxReasonRes == INDEX_WAITHANDLE_REASON.SUCCESS) {
                    tableResponse = obj as DataTable;

                    switch ((StatesMachine)state) {
                        case StatesMachine.SERVER_TIME:
                            //Values.SetServerTime(_key._current_type, (DateTime)tableResponse.Rows[0][0]);
                            Values[_key._current_type].serverTime = (DateTime)tableResponse.Rows[0][0];
                            break;
                        case StatesMachine.LIST_SIGNAL:
                            indxReasonRes = response_handlers[(StatesMachine)state](tableResponse) == true
                                 ? INDEX_WAITHANDLE_REASON.SUCCESS : INDEX_WAITHANDLE_REASON.ERROR;

                            Logging.Logg().Debug(string.Format(@"::StateResponse () - [id_tec={0}, type={1}] получено строк={2}, сигналов={3}; Рез-т={4}"
                                    , _key._current_id_tec, _key._current_type, tableResponse.Rows.Count, Signals[_key._current_type].Count, indxReasonRes)
                                , Logging.INDEX_MESSAGE.NOT_SET);
                            break;
                        case StatesMachine.VALUES:
                            indxReasonRes = response_handlers[(StatesMachine)state](tableResponse) == true
                                 ? INDEX_WAITHANDLE_REASON.SUCCESS : INDEX_WAITHANDLE_REASON.ERROR;
                            break;
                        default:
                            break;
                    }
                } else
                    ;

                if (isLastState(state) == true) {
                    UpdateGUI_Fact?.Invoke((int)_key._current_type, state);

                    (m_waitHandleState[(int)INDEX_WAITHANDLE_REASON.SUCCESS] as AutoResetEvent).Set();

                    ReportClear(true);
                } else
                    ;

                return (int)indxReasonRes;
            }

            protected override void StateWarnings(int state, int req, int res)
            {
                warningReport(string.Format(@"HandlerDbStateValue::StateWarnings(type={0}) - state={1} ...", _key._current_type, (StatesMachine)state));
            }
            /// <summary>
            /// Инициализация дополнительных объектов синхронизации
            ///  для возможности последовательных запросов АИИСКУЭ, СОТИАССО
            /// </summary>
            protected override void InitializeSyncState()
            {
                m_waitHandleState = new WaitHandle[(int)INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON];
                base.InitializeSyncState();

                for (int i = (int)INDEX_WAITHANDLE_REASON.SUCCESS + 1; i < (int)INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON; i++) {
                    m_waitHandleState[i] = new ManualResetEvent(false);
                }
            }

            private class AIISKUE_KKSCODE
            {
                public struct SENSOR
                {
                    public int id_object;

                    public int num_item;
                }

                public static string ToKKSCode(int iObject, int iItem)
                {
                    string strRes = string.Empty;

                    strRes = string.Format(@"OBJECT{0}_ITEM{1}", iObject, iItem);

                    return strRes;
                }

                public static SENSOR ToSensor(string kks_code)
                {
                    SENSOR sensorRes = new SENSOR() { id_object = -1, num_item = -1 };

                    int value = -1;

                    if ((kks_code.Contains("OBJECT") == true)
                        && (kks_code.Contains("ITEM") == true)
                        && (kks_code.Contains("_") == true)) {
                        if (int.TryParse(kks_code.Split('_')[0].Substring("OBJECT".Length), out value) == true) {
                            sensorRes.id_object = value;

                            if (int.TryParse(kks_code.Split('_')[1].Substring("ITEM".Length), out value) == true) {
                                sensorRes.num_item = value;
                            } else
                                ;
                        } else
                            ;
                    } else
                        ;

                    return sensorRes;
                }
            }

            private void getListSignals()
            {
                AddState((int)StatesMachine.SERVER_TIME);
                AddState((int)StatesMachine.LIST_SIGNAL);

                Run(string.Format(@"HandlerDbSignalValue::getListSignals (id_tec={0}, type={1}) - ...", _key._current_id_tec, _key._current_type));
            }

            private string getListSignalRequest()
            {
                return _key._current_type == CONN_SETT_TYPE.DATA_AISKUE ? string.Format(@"SELECT *, {3}.[CODE] as [{6}] FROM [{0}] as {1} JOIN [{2}] {3} ON {1}.[{4}] = {3}.[{5}]"
                        , "SENSORS", "sen", "DEVICES", "dev", "STATIONID", "ID", "USPD_CODE")
                    : _key._current_type == CONN_SETT_TYPE.DATA_SOTIASSO ? string.Format(@"SELECT * FROM [{0}] WHERE [{1}] LIKE '{2}'"
                        , "reals_v", "DESCRIPTION", "%Сум%")
                        : string.Empty;
            }

            private bool getListSignalResponse(DataTable tableListSignals)
            {
                bool bRes = true;

                SIGNAL sgnl;

                foreach (DataRow r in tableListSignals.Rows) {
                    try {
                        switch (_key._current_type) {
                            case CONN_SETT_TYPE.DATA_AISKUE:
                                sgnl = new SIGNAL() {
                                    id = (int)r[@"ID"]
                                    , kks_code = AIISKUE_KKSCODE.ToKKSCode((int)r[@"USPD_CODE"], (int)r[@"CODE"])
                                    , name_shr = string.Format(@"{0}", (string)r[@"NAME"])
                                    , name = string.Format(@"{0}", (string)r[@"NAME"])
                                };
                                break;
                            case CONN_SETT_TYPE.DATA_SOTIASSO:
                                sgnl = new SIGNAL() {
                                    id = (int)(decimal)r[@"id"]
                                    , kks_code = string.Format(@"{0}", (string)r[@"name"])
                                    , name_shr = string.Format(@"{0}", ((string)r[@"description"]).Split(new char[] { '.' })[1].Split(' ')[1])
                                    , name = string.Format(@"{0}", (string)r[@"description"])
                                };
                                break;
                            default:
                                sgnl = new SIGNAL();
                                break;
                        }
                    } catch (Exception e) {
                        bRes = false;
                        sgnl = new SIGNAL();

                        Logging.Logg().Exception(e, string.Format(@"::getListSignalResponse () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    if (sgnl.id > 0)
                        _dictSignals[_key._current_type].Add(sgnl);
                    else
                        ; // Logging.Logg().Error(string.Format(@""), Logging.INDEX_MESSAGE.NOT_SET)
                }

                return bRes;
            }

            private string getValuesRequest()
            {
                string strRes = string.Empty;

                string db_table = string.Empty
                    , kks_code = string.Empty;
                AIISKUE_KKSCODE.SENSOR sensor;
                int parnumber = -1
                    , i_agregate = -1
                    , UTC_OFFSET = -1;
                DateTime sql_datetime_start = DateTime.MinValue
                    , sql_datetime_end = DateTime.MinValue;

                switch (_key._current_type) {
                    case CONN_SETT_TYPE.DATA_AISKUE:
                        sensor = AIISKUE_KKSCODE.ToSensor(Signals[_key._current_type].ElementAt(_key._current_index).kks_code);
                        parnumber = 12;
                        sql_datetime_start = UserDate.Value.Date;
                        sql_datetime_end = sql_datetime_start.AddDays(1F);
                        UTC_OFFSET = 0;

                        strRes = string.Format("SELECT [OBJECT], [ITEM], [DATA_DATE], [VALUE0]" +
                            " FROM [DATA]" +
                            " WHERE" +
                                " PARNUMBER={2}" +
                                " AND [OBJECT]={0}" +
                                " AND [ITEM]={1}" +
                                " AND [DATA_DATE] > '{3:yyyyMMdd HH:mm:ss}' AND [DATA_DATE] <= '{4:yyyyMMdd HH:mm:ss}'" +
                            " ORDER BY [DATA_DATE]"
                            , sensor.id_object, sensor.num_item, parnumber, sql_datetime_start, sql_datetime_end);
                        break;
                    case CONN_SETT_TYPE.DATA_SOTIASSO:
                        db_table = "states_real_his_0";
                        kks_code = Signals[_key._current_type].ElementAt(_key._current_index).kks_code;
                        sql_datetime_start = UserDate.Value.Date;
                        sql_datetime_end = sql_datetime_start.AddDays(1F);
                        i_agregate = 30;
                        UTC_OFFSET = 0;

                        strRes = string.Format("SELECT [id], DATEADD(MINUTE, {5}, dateadd(MINUTE, (datediff(MINUTE, '{2:yyyyMMdd HH:mm:ss}', [last_changed_at]) / {4}) * {4}, '{2:yyyyMMdd HH:mm:ss}')) as [last_changed_at]" +
                            ", AVG([Value]) * 1000 AS [VALUE]" +
                            ", COUNT(*)" +
                            " FROM (" +
                                "SELECT [id], dateadd(SECOND, (datediff(SECOND, '{2:yyyyMMdd HH:mm:ss}', [last_changed_at]) / 60) * 60, '{2:yyyyMMdd HH:mm:ss}') as [last_changed_at]" +
                                    ", CONVERT(DECIMAL(10, 6), (SUM(CONVERT(DECIMAL(10, 6), [Value]) * [tmdelta]) / SUM([tmdelta]))) AS [VALUE]" +
                                    ", COUNT(*) as [COUNT]" +
                                    " FROM [dbo].[{0}]" +
                                    " WHERE ID IN (" +
                                        "SELECT [ID]" +
                                            " FROM [dbo].[reals_v]" +
                                            " WHERE [NAME] LIKE '%{1}%'" +
                                        ")" +
                                        " AND [last_changed_at] >= '{2:yyyyMMdd HH:mm:ss}' AND [last_changed_at] < '{3:yyyyMMdd HH:mm:ss}'" +
                                " GROUP BY dateadd(SECOND, (datediff(SECOND, '{2:yyyyMMdd HH:mm:ss}', [last_changed_at]) / 60) * 60, '{2:yyyyMMdd HH:mm:ss}'), id" +
                            ") res" +
                            " GROUP BY dateadd(MINUTE, (datediff(MINUTE, '{2:yyyyMMdd HH:mm:ss}', [last_changed_at]) / {4}) * {4}, '{2:yyyyMMdd HH:mm:ss}'), id" +
                            " ORDER BY [last_changed_at], [id]"
                            , db_table, kks_code, sql_datetime_start, sql_datetime_end, i_agregate, (UTC_OFFSET * 60 + 30));
                        break;
                    default:
                        break;
                }

                return strRes;
            }

            private bool getValuesResponse(DataTable tableValues)
            {
                bool bRes = true;

                DateTime stamp = DateTime.MinValue
                    , min_stamp = DateTime.MaxValue;

                try {
                    foreach (DataRow r in tableValues.Rows) {
                        stamp = (DateTime)r[@"DATA_DATE"];

                        if ((min_stamp - stamp).TotalSeconds > 0)
                            min_stamp = stamp;
                        else
                            ;

                        Values[_key._current_type].m_valuesHours.Add(new VALUE() {
                            stamp = stamp
                            , index_stamp = (int)((stamp - min_stamp).TotalMinutes / 30)
                            , value = (float)(double)r[@"VALUE0"]
                            , quality = -1 });
                    }
                } catch (Exception e) {
                    bRes = false;

                    Logging.Logg().Exception(e, string.Format(@"HandlerDbSignalValue::getValuesResponse (id_tec={0}, type={1}, signal.kks={2}) - ..."
                            , _key._current_id_tec, _key._current_type, Signals[_key._current_type].ElementAt(_key._current_index).kks_code)
                        , Logging.INDEX_MESSAGE.NOT_SET);
                }

                return bRes;
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
        /// <summary>
        /// Словарь с предствалениями для отображения значений выбранных (на панели управления) сигналов
        /// </summary>
        private Dictionary<CONN_SETT_TYPE, HDataGridView> m_dictDataGridViewValues;
        /// <summary>
        /// Панели графической интерпретации значений
        /// 1) АИИСКУЭ "сутки - по-часам для выбранных сигналов, 2) СОТИАССО "сутки - по-часам для выбранных сигналов"
        /// </summary>
        private Dictionary<CONN_SETT_TYPE, ZedGraph.ZedGraphControl> m_dictZGraphValues;

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
            m_dictZGraphValues = new Dictionary<CONN_SETT_TYPE, ZedGraphControl>();

            //Создать, разместить дочерние элементы управления
            initializeComponent();

            if (m_listTEC.Count > 0) {
                //Создать объект обработки запросов - установить первоначальные индексы для ТЭЦ, компонента
                m_HandlerDb = new HandlerDbSignalValue(iListenerConfigId, m_listTEC);
                m_HandlerDb.UpdateGUI_Fact += new IntDelegateIntIntFunc(onEvtHandlerStatesCompleted);
                m_HandlerDb.UserDate = new HandlerDbSignalValue.USER_DATE() { UTC_OFFSET = m_panelManagement.CurUtcOffset, Value = m_panelManagement.CurDateTime };
            } else
                Logging.Logg().Error(@"PanelSOTIASSODay::ctor () - кол-во ТЭЦ = 0...", Logging.INDEX_MESSAGE.NOT_SET);

            //m_listIdAIISKUEAdvised = new List<int>();
            //m_listIdSOTIASSOAdvised = new List<int>();

            #region Дополнительная инициализация панели управления
            m_panelManagement.SetTECList(m_listTEC);
            m_panelManagement.EvtTECListSelectionIndexChanged += new Action<int>(panelManagement_TECListOnSelectionChanged);
            m_panelManagement.EvtDateTimeChanged += new Action<ActionDateTime>(panelManagement_OnEvtDateTimeChanged);
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
            m_dictZGraphValues.Add(type, new ZedGraphControl()); // графическая панель для отображения СОТИАССО-значений
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

        private void panelManagement_OnEvtSignalItemChecked(CONN_SETT_TYPE type, ActionSignal action, int indxSignal)
        {
            bool bCheckStateReverse = action == ActionSignal.CHECK;

            switch (action) {
                case ActionSignal.CHECK:
                    if (m_dictDataGridViewValues[type].ActionColumn(m_HandlerDb.Signals[type].ElementAt(indxSignal).kks_code
                        , m_HandlerDb.Signals[type].ElementAt(indxSignal).name_shr) == true) {
                        // запросить значения для заполнения нового столбца
                        m_HandlerDb.Request(type, indxSignal);
                    } else
                    // столбец удален - ничего не делаем
                        ;
                    break;
                case ActionSignal.SELECT:
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

                    clb.Items.Clear();                    
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

                    clb.Items.AddRange(listSignalNameShr.ToArray());

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

            public void Fill(IEnumerable<HandlerDbSignalValue.VALUE> values)
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
        private void panelManagement_OnEvtDateTimeChanged(ActionDateTime action_changed)
        {
            //??? либо автоматический опрос в 'm_HandlerDb'
            m_HandlerDb.UserDate = new HandlerDbSignalValue.USER_DATE() { UTC_OFFSET = m_panelManagement.CurUtcOffset, Value = m_panelManagement.CurDateTime };
            // , либо организация цикла опроса в этой функции
            //...
            // , либо вызов метода с аргументами
            //m_HandlerDb.Request(...);
        }
        /// <summary>
        /// Обработчик события - все состояния 'ChangeState_SOTIASSO' обработаны
        /// </summary>
        /// <param name="hour">Номер часа в запросе</param>
        /// <param name="min">Номер минуты в звпросе</param>
        /// <returns>Признак результата выполнения функции</returns>
        private int onEvtHandlerStatesCompleted(int conn_sett_type, int state_machine)
        {
            int iRes = ((!((CONN_SETT_TYPE)conn_sett_type == CONN_SETT_TYPE.UNKNOWN)) // тип источника данных известен
                && (!(state_machine < 0))) // кол-во строк "не меньше 0"
                ? 0
                    : -1;

            IAsyncResult iar = BeginInvoke(new Action<CONN_SETT_TYPE, int>(onStatesCompleted), (CONN_SETT_TYPE)conn_sett_type, state_machine);

            return iRes;
        }

        private void onStatesCompleted(CONN_SETT_TYPE type, int state_machine)
        {
            switch ((StatesMachine)state_machine) {
                case StatesMachine.LIST_SIGNAL:
                    m_panelManagement.InitializeSignalList(type, m_HandlerDb.GetListSignals(type).Select(sgnl => { return sgnl.name_shr; }));
                    break;
                case StatesMachine.VALUES:
                    m_dictDataGridViewValues[type].Fill(m_HandlerDb.Values[type].m_valuesHours);
                    break;
                default:
                    break;
            }
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

            foreach (CONN_SETT_TYPE conn_sett_type in new CONN_SETT_TYPE[] { CONN_SETT_TYPE.DATA_AISKUE, CONN_SETT_TYPE.DATA_SOTIASSO }) {
                // получить цветовую гамму
                getColorZEDGraph(conn_sett_type, out colorChart, out colorPCurve);
                // отобразить
                (m_dictZGraphValues[conn_sett_type] as HZedGraphControl).Draw(m_HandlerDb.Values[conn_sett_type].m_valuesHours
                    , textGraphCurDateTime
                    , colorChart, colorPCurve);
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
                //Очистить списки с сигналами
                foreach (CONN_SETT_TYPE conn_sett_type in new CONN_SETT_TYPE[] { CONN_SETT_TYPE.DATA_AISKUE, CONN_SETT_TYPE.DATA_SOTIASSO })
                    m_panelManagement.ClearSignalList(conn_sett_type);
                //Инициализировать список ТЭЦ для 'TecView' - указать ТЭЦ в соответствии с указанным ранее индексом (0)
                m_HandlerDb.InitTEC(m_listTEC[indxTEC].m_id);
                //Добавить строки(сигналы) на дочернюю панель(список АИИСКУЭ, СОТИАССО-сигналов)
                // - по возникновению сигнала окончания заппроса                
            } else
                ;            
        }
    }
}