﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel; //IContainer
using System.Threading; //ManualResetEvent
using System.Drawing; //Color
using System.Data;

using ZedGraph;


using StatisticCommon;
using System.Linq;
using System.Data.Common;
using ASUTP.Helper;
using ASUTP.Database;
using ASUTP;

namespace Statistic
{
    /// <summary>
    /// Панель для отображения значений СОТИАССО (телеметрия)
    ///  для контроля
    /// </summary>
    partial class PanelAISKUESOTIASSODay
    {
        partial class HandlerSignalQueue
        {
            /// <summary>
            /// Класс для обработки запросов к источникам данных
            /// </summary>
            private class HandlerDbSignalValue : HHandlerDb
            {
                public struct KEY
                {
                    public int _current_id_tec;

                    public CONN_SETT_TYPE _current_type;

                    public string _kks_code;
                }

                private static KEY _key = new KEY() { _current_id_tec = -1, _current_type = CONN_SETT_TYPE.UNKNOWN, _kks_code = string.Empty };

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
                /// Конструктор - основной (с аргументами)
                /// </summary>
                /// <param name="iListenerConfigDbId">Идентификатор установленного соединения с БД конфигурации</param>
                /// <param name="listTEC">Список ТЭЦ</param>
                public HandlerDbSignalValue(int iListenerConfigDbId, IEnumerable<TEC> listTEC, IEnumerable<CONN_SETT_TYPE>types)
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

                            foreach (CONN_SETT_TYPE type in types) {
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
                        }
                    else
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

                    m_arSyncStateCheckResponse = new AutoResetEvent[(int)INDEX_SYNC_STATECHECKRESPONSE.COUNT_INDEX_SYNC_STATECHECKRESPONSE] {
                        new AutoResetEvent (false)
                        , new AutoResetEvent (false)
                        , new AutoResetEvent (false)
                    };

                    request_handlers = new Dictionary<StatesMachine, Func<string>>() { { StatesMachine.LIST_SIGNAL, getListSignalRequest }, { StatesMachine.VALUES, getValuesRequest } };
                    response_handlers = new Dictionary<StatesMachine, Func<DataTable, bool>>() { { StatesMachine.LIST_SIGNAL, getListSignalResponse }, { StatesMachine.VALUES, getValuesResponse } };

                    Values = new VALUES() { serverTime = DateTime.MinValue, m_valuesHours = new List<VALUE>() };
                    _signals = new List<SIGNAL>();
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

                //public IntDelegateIntIntFunc UpdateGUI_Fact;

                public VALUES Values;

                private IList<SIGNAL> _signals;
                public IList<SIGNAL> Signals { get { return _signals; } }
                ///// <summary>
                ///// Перечень сигналов для текущей ТЭЦ
                ///// </summary>
                //public IEnumerable<SIGNAL> GetListSignals(CONN_SETT_TYPE type)
                //{
                //    return ((!(_dictSignals == null)) && (_dictSignals.ContainsKey(type) == true)) ? _dictSignals[type] : new List<SIGNAL>();
                //}
                /// <summary>
                /// Перечисление - индексы объектов синхронизации для обмена данными (результатом) обработки событий
                /// </summary>
                public enum INDEX_SYNC_STATECHECKRESPONSE
                {
                    UNKNOWN = -1, RESPONSE, ERROR, WARNING
                    , COUNT_INDEX_SYNC_STATECHECKRESPONSE
                }
                /// <summary>
                /// Объекты синхронизации для обмена данными (результатом) обработки событий
                /// </summary>
                public AutoResetEvent[] m_arSyncStateCheckResponse;
                ///// <summary>
                ///// Установить идентификатор выбранной в данный момент ТЭЦ
                ///// </summary>
                ///// <param name="id">Идентификатор ТЭЦ</param>
                //public void InitTEC(int id)
                //{
                //    if (!(_key._current_id_tec == id)) {
                //        _key._current_id_tec = id;

                //        ClearValues();

                //        //Поток запроса значений для 
                //        new Thread(new ParameterizedThreadStart((param) => {
                //            int indxEv = -1;
                //            // чтобы в 1-ый проход "пройти" WaitHandle.WaitAny и перейти к выполнению запроса
                //            ((AutoResetEvent)m_waitHandleState[(int)INDEX_WAITHANDLE_REASON.SUCCESS]).Set();

                //            for (INDEX_WAITHANDLE_REASON i = INDEX_WAITHANDLE_REASON.ERROR; i < INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON; i++)
                //                ((ManualResetEvent)m_waitHandleState[(int)i]).Reset();

                //            foreach (CONN_SETT_TYPE type in _types) {
                //                indxEv = WaitHandle.WaitAny(m_waitHandleState);
                //                if (indxEv == (int)INDEX_WAITHANDLE_REASON.BREAK)
                //                    // прекратить выполнение досрочно
                //                    break;
                //                else {
                //                    if (!(indxEv == (int)INDEX_WAITHANDLE_REASON.SUCCESS))
                //                        // в сигнальное состояние установлен отличный от BREAK, SUCCESS объект синхронизации
                //                        // восстановить его состояние для очередной итерации
                //                        ((ManualResetEvent)m_waitHandleState[indxEv]).Reset();
                //                    else
                //                        ;

                //                    ClearStates();

                //                    _key._current_type = type;

                //                    getListSignals(); //getvalues();
                //                }
                //            }
                //        })).Start();
                //    } else
                //        ;
                //}

                public void Request()
                {

                }
                /// <summary>
                /// Отправить запрос на получение значений сигналов АИИСКУЭ, СОТИАССО
                /// </summary>
                public void Request(CONN_SETT_TYPE type, string kks_code)
                {
                    clearValues(false);
                    ClearStates();

                    _key._current_type = type;
                    _key._kks_code = kks_code;

                    getValues();
                }

                private void getValues()
                {
                    AddState((int)StatesMachine.SERVER_TIME);
                    AddState((int)StatesMachine.VALUES);

                    Run(string.Format(@"HandlerDbSignalValues::GetValues (type={0}) - KKS_CODE={1}...", _key._current_type, _key._kks_code));
                }

                public override void Start()
                {
                    base.Start();

                    StartDbInterfaces();
                }

                public override void Stop()
                {
                    m_arSyncStateCheckResponse[(int)INDEX_SYNC_STATECHECKRESPONSE.WARNING].Set();

                    base.Stop();
                }

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
                    clearValues(true);
                }

                private void clearValues(bool bIsSignalClear)
                {
                    Values.serverTime = DateTime.MinValue;
                    Values.m_valuesHours.Clear();

                    if (bIsSignalClear == true)
                        _signals.Clear();
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
                                m_dictIdListeners.Add((int)pair.Key, new int[(int)pair.Value.Keys.Max() + 1]);

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
                    m_arSyncStateCheckResponse[(int)INDEX_SYNC_STATECHECKRESPONSE.ERROR].Set();

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
                                Values.serverTime = (DateTime)tableResponse.Rows[0][0];
                                break;
                            case StatesMachine.LIST_SIGNAL:
                                indxReasonRes = response_handlers[(StatesMachine)state](tableResponse) == true
                                     ? INDEX_WAITHANDLE_REASON.SUCCESS : INDEX_WAITHANDLE_REASON.ERROR;

                                Logging.Logg().Debug(string.Format(@"::StateResponse () - [id_tec={0}, type={1}] получено строк={2}, сигналов={3}; Рез-т={4}"
                                        , _key._current_id_tec, _key._current_type, tableResponse.Rows.Count, Signals.Count, indxReasonRes)
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
                        m_arSyncStateCheckResponse[(int)INDEX_SYNC_STATECHECKRESPONSE.RESPONSE].Set();

                        ReportClear(true);
                    } else
                        ;

                    return (int)indxReasonRes;
                }

                protected override void StateWarnings(int state, int req, int res)
                {
                    m_arSyncStateCheckResponse[(int)INDEX_SYNC_STATECHECKRESPONSE.WARNING].Set();

                    warningReport(string.Format(@"HandlerDbStateValue::StateWarnings(type={0}) - state={1} ...", _key._current_type, (StatesMachine)state));
                }
                ///// <summary>
                ///// Инициализация дополнительных объектов синхронизации
                /////  для возможности последовательных запросов АИИСКУЭ, СОТИАССО
                ///// </summary>
                //protected override void InitializeSyncState()
                //{
                //    m_waitHandleState = new WaitHandle[(int)INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON];
                //    base.InitializeSyncState();

                //    for (int i = (int)INDEX_WAITHANDLE_REASON.SUCCESS + 1; i < (int)INDEX_WAITHANDLE_REASON.COUNT_INDEX_WAITHANDLE_REASON; i++) {
                //        m_waitHandleState[i] = new ManualResetEvent(false);
                //    }
                //}

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

                public void GetListSignals(int id_tec, CONN_SETT_TYPE type)
                {
                    clearValues(true);
                    ClearStates();

                    _key._current_id_tec = id_tec;
                    _key._current_type = type;

                    getListSignals();
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
                    string[] description;
                    string name_main = string.Empty
                        , name_detail = string.Empty;

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
                                    description = ((string)r[@"description"]).Split(new char[] { '.' });

                                    name_main = description[1].Split(' ')[1];
                                    name_detail = description[1].Split(' ')[description[1].Split(' ').Length - 1];

                                    sgnl = new SIGNAL() {
                                        id = (int)(decimal)r[@"id"]
                                        , kks_code = string.Format(@"{0}", (string)r[@"name"])
                                        , name_shr = string.Format(@"{0} {1}", name_main, name_detail.Equals(name_main) == false ? name_detail : string.Empty)
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
                            _signals.Add(sgnl);
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
                            sensor = AIISKUE_KKSCODE.ToSensor(_key._kks_code);
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
                            kks_code = _key._kks_code;
                            sql_datetime_start = UserDate.Value.Date.AddHours(-1 * UserDate.UTC_OFFSET);
                            sql_datetime_end = sql_datetime_start.AddDays(1F);
                            i_agregate = 30;
                            UTC_OFFSET = 0;

                            // [last_changed_at] переименовывается в [DATA_DATE], [Value] -> [VALUE0] для однообразия при обработке результатов (по аналогии с АИИСКУЭ)
                            strRes = string.Format("SELECT [id], DATEADD(MINUTE, {5}, dateadd(MINUTE, (datediff(MINUTE, '{2:yyyyMMdd HH:mm:ss}', [last_changed_at]) / {4}) * {4}, '{2:yyyyMMdd HH:mm:ss}')) as [last_changed_at]" + // last_changed_at -> DATA_DATE
                                ", AVG([Value]) * 1000 AS [Value]" + // Value -> VALUE0
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
                                " ORDER BY [last_changed_at], [id]" // last_changed_at -> DATA_DATE
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

                    //// для однообразия при обработке результатов (идентично с АИИСКУЭ)
                    //Dictionary<CONN_SETT_TYPE, IEnumerable<string>> dictNameFields = new Dictionary<CONN_SETT_TYPE, IEnumerable<string>>() {
                    //    { CONN_SETT_TYPE.DATA_AISKUE, new string[] { "DATA_DATE", "VALUE0" } }
                    //    , { CONN_SETT_TYPE.DATA_SOTIASSO, new string[] { "last_changed_at", "VALUE" } }
                    //};

                    DateTime stamp = DateTime.MinValue
                        , min_stamp = DateTime.MaxValue;

                    try {
                        foreach (DataRow r in tableValues.Rows) {
                            stamp = _key._current_type == CONN_SETT_TYPE.DATA_AISKUE ? (DateTime)r[@"DATA_DATE"]
                                : _key._current_type == CONN_SETT_TYPE.DATA_SOTIASSO ? (DateTime)r[@"last_changed_at"]
                                    : DateTime.MaxValue;

                            if ((min_stamp - stamp).TotalSeconds > 0)
                                min_stamp = stamp;
                            else
                                ;

                            Values.m_valuesHours.Add(new VALUE() {
                                stamp = stamp
                                , index_stamp = (int)((stamp - min_stamp).TotalMinutes / 30)
                                , value = _key._current_type == CONN_SETT_TYPE.DATA_AISKUE ? (float)(double)r[@"VALUE0"]
                                    : _key._current_type == CONN_SETT_TYPE.DATA_SOTIASSO ? (float)(decimal)r[@"Value"]
                                        : -1F
                                , quality = -1 });
                        }
                    } catch (Exception e) {
                        bRes = false;

                        Logging.Logg().Exception(e, string.Format(@"HandlerDbSignalValue::getValuesResponse (id_tec={0}, type={1}, signal.kks={2}) - ..."
                                , _key._current_id_tec, _key._current_type, _key._kks_code)
                            , Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    return bRes;
                }
            }
        }
    }
}