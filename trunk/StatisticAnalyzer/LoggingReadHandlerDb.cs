﻿using ASUTP;
using ASUTP.Database;
using ASUTP.Helper;
using StatisticCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections;

namespace StatisticAnalyzer
{
    partial class PanelAnalyzer_DB
    {
        //private partial class HLoggingReadHandlerQueue
        //{
        private class HLoggingReadHandlerDb : HHandlerDb, ILoggingReadHandler
        {
            private ConnectionSettings m_connSett;

            private DateTime m_serverTime;

            public event Action<PanelAnalyzer.REQUEST, DataTable> EventCommandCompleted;

            private List<REQUEST> _requests;

            private class REQUEST : PanelAnalyzer.REQUEST
            {
                public REQUEST (StatesMachine key, object arg)
                    : base(key, arg)
                {
                }

                #region Запрос

                public string Query
                {
                    get
                    {
                        string strRes = string.Empty
                            , where = string.Empty;

                        if (((Key == StatesMachine.ProcCheckedFilter)
                                || (Key == StatesMachine.ProcCheckedState))
                            || (Args.Length > 0))
                            switch (Key) {
                                case StatesMachine.ProcCheckedFilter:
                                case StatesMachine.ProcCheckedState:
                                    strRes = @"SELECT [ID_USER], MAX ([DATETIME_WR]) as MAX_DATETIME_WR FROM logging GROUP BY [ID_USER] ORDER BY [ID_USER]";
                                    break;
                                case StatesMachine.ListMessageToUserByDate:
                                    strRes = @"SELECT DATETIME_WR as DATE_TIME, ID_LOGMSG as TYPE, MESSAGE FROM logging WHERE ID_USER=" + (int)Args [0];

                                    if (((DateTime)Args [2]).Equals (DateTime.MaxValue) == false) {
                                        //Вариант №1 диапазон даты/времени
                                        where = $"DATETIME_WR>='{((DateTime)Args [2]).ToString ("yyyyMMdd HH:mm:ss")}'";
                                        if (((DateTime)Args [3]).Equals (DateTime.MaxValue) == false)
                                            where += $" AND DATETIME_WR<'{((DateTime)Args [3]).ToString ("yyyyMMdd HH:mm:ss")}'";
                                        else
                                            ;
                                        ////Вариант №2 указанные сутки
                                        //where = "DATETIME_WR='" + beg.ToString("yyyyMMdd") + "'";
                                    } else
                                        ;

                                    if (where.Equals (string.Empty) == false)
                                        strRes += @" AND " + where;
                                    else
                                        ;
                                    break;
                                case StatesMachine.ListDateByUser:
                                    strRes = @"SELECT DATEADD(DAY, DATEDIFF(DAY, 0, [DATETIME_WR]), 0) as [DATE_TIME], COUNT(*) as [CNT]" // DATEPART (DD, [DATETIME_WR]) as DD, DATEPART (MM, [DATETIME_WR]) as MM, DATEPART (YYYY, [DATETIME_WR]) as [YYYY]
                                        + @" FROM [dbo].[logging]"
                                        + @" WHERE [ID_USER]=" + (int)Args [0]
                                        + @" GROUP BY DATEADD(DAY, DATEDIFF(DAY, 0, [DATETIME_WR]), 0)" // DATEPART (DD, [DATETIME_WR]), DATEPART (MM, [DATETIME_WR]), DATEPART (YYYY, [DATETIME_WR])
                                        + @" ORDER BY [DATE_TIME]";
                                    break;
                                case StatesMachine.CounterToTypeMessageByDate:
                                    bool byDate = !((DateTime)Args [1]).Equals (DateTime.MaxValue)
                                        , byUser = !string.IsNullOrEmpty (((string)Args [3]).Trim ());

                                    if (byDate == true) {
                                        //диапазон даты/времени
                                        where = "WHERE DATETIME_WR BETWEEN '" + ((DateTime)Args [1]).ToString ("yyyyMMdd HH:mm:ss") + "'";
                                        if (((DateTime)Args [2]).Equals (DateTime.MaxValue) == false) {
                                            where += " AND '" + ((DateTime)Args [2]).ToString ("yyyyMMdd HH:mm:ss") + "'";
                                        } else
                                            ;
                                    } else
                                        ;

                                    if (byUser == true) {
                                        //добавление идентификаторов пользователей к условиям выборки
                                        if (string.IsNullOrEmpty (where) == true)
                                            where += "WHERE";
                                        else
                                            where += " AND";
                                        where += " ID_USER in (" + ((string)Args [3]) + ")";
                                        strRes += where;
                                    } else
                                        ;

                                    strRes = $"SELECT [ID_LOGMSG], COUNT (*) as [COUNT] FROM [dbo].[logging] {where} GROUP BY [ID_LOGMSG] ORDER BY [ID_LOGMSG]";
                                    break;
                                default:
                                    break;
                            } else
                            Logging.Logg ().Error ($"PanelAnalyzer_DB.HLoggingReadHandlerDb.REQUEST::get - Key={Key}, Args.Length={Args.Length}", Logging.INDEX_MESSAGE.NOT_SET);

                        return strRes;
                    }
                }

                #endregion
            }

            public HLoggingReadHandlerDb ()
            {
                m_connSett = new ConnectionSettings();
                m_serverTime = DateTime.MinValue;
                _requests = new List<REQUEST>();
            }

            public void PerformCommandCompleted (PanelAnalyzer.REQUEST req, DataTable tableRes)
            {
                EventCommandCompleted?.Invoke (req, tableRes);
            }

            public void SetConnectionSettings (ConnectionSettings connSett)
            {
                m_connSett = connSett;
            }

            private int ListenerIdConfigDb
            {
                get
                {
                    return m_dictIdListeners[0][(int)CONN_SETT_TYPE.CONFIG_DB];
                }
            }

            private int ListenerIdMainDb
            {
                get
                {
                    return m_dictIdListeners [0] [(int)CONN_SETT_TYPE.LIST_SOURCE];
                }
            }

            public override void ClearStates ()
            {
                Logging.Logg ().Debug ($"PanelAnalyzer.HLoggingReadHandlerDb::ClearStates () - удаляются все состояния <{_requests.Count}> шт...", Logging.INDEX_MESSAGE.NOT_SET);
                _requests.Clear ();

                base.ClearStates();
            }

            public override void ClearValues ()
            {
                throw new NotImplementedException ();
            }

            public override void StartDbInterfaces ()
            {
                if (m_connSett.IsEmpty == false) {
                    if (m_dictIdListeners.ContainsKey(0) == false)
                        m_dictIdListeners.Add(0, new int[] { -1, -1 });
                    else
                        ;

                    //register(0, (int)CONN_SETT_TYPE.CONFIG_DB, FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), m_connSett.name);
                    register(0, (int)CONN_SETT_TYPE.LIST_SOURCE, m_connSett, m_connSett.name);
                } else
                    throw new InvalidOperationException("PanelAnalyzer_DB.HLoggingReadHandlerDb::StartDbInterfaces () - ");
            }

            /// <summary>
            /// Добавить состояния в набор для обработки
            /// данных 
            /// </summary>
            public void Command (StatesMachine state/*, Action <object, int> handlerCommand*/)
            {
                Command(state, null, false);
            }

            public void Command(StatesMachine state, object args, bool bNewState)
            {
                lock (m_lockState) {
                    if (bNewState == true) {
                        ClearStates ();
                        AddState ((int)StatesMachine.ServerTime);
                    } else
                        ;
                    AddState((int)state);

                    _requests.Add(new REQUEST (state, args));

                    Run(@"PanelAnalyzer.HLoggingReadHandlerDb::Command () - run...");
                }
            }

            /// <summary>
            /// Получить результат обработки события
            /// </summary>
            /// <param name="state">Событие для получения результата</param>
            /// <param name="error">Признак ошибки при получении результата</param>
            /// <param name="outobj">Результат запроса</param>
            /// <returns>Признак получения результата</returns>
            protected override int StateCheckResponse (int state, out bool error, out object outobj)
            {
                int iRes = 0;

                error = false;
                outobj = new DataTable();

                StatesMachine statesMachine = (StatesMachine)state;

                switch (statesMachine) {
                    case StatesMachine.ServerTime:
                    case StatesMachine.ProcCheckedFilter:
                    case StatesMachine.ProcCheckedState:
                    case StatesMachine.ListMessageToUserByDate:
                    case StatesMachine.ListDateByUser:
                    case StatesMachine.CounterToTypeMessageByDate:
                        iRes = response (m_IdListenerCurrent, out error, out outobj);
                        break;
                    default:
                        error = true;
                        outobj = null;
                        break;
                }

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
            protected override INDEX_WAITHANDLE_REASON StateErrors (int state, int req, int res)
            {
                INDEX_WAITHANDLE_REASON iRes = INDEX_WAITHANDLE_REASON.SUCCESS;

                func_Completed("StateErrors", (StatesMachine)state, new DataTable(), res);

                errorReport (@"Получение значений из БД - состояние: " + ((StatesMachine)state).ToString ());

                return iRes;
            }

            protected override int StateRequest (int state)
            {
                int iRes = 0;

                REQUEST req;
                StatesMachine stateMachine = (StatesMachine)state;

                switch (stateMachine) {
                    case StatesMachine.ServerTime:
                        GetCurrentTimeRequest (DbInterface.DB_TSQL_INTERFACE_TYPE.MSSQL, ListenerIdMainDb);
                        actionReport (@"Получение времени с сервера БД - состояние: " + ((StatesMachine)state).ToString ());
                        break;
                    case StatesMachine.ProcCheckedFilter:
                    case StatesMachine.ProcCheckedState:
                    case StatesMachine.ListMessageToUserByDate:
                    case StatesMachine.ListDateByUser:
                    case StatesMachine.CounterToTypeMessageByDate:
                        req = getFirstRequst (stateMachine);
                        if (req.IsEmpty == false) {
                            Request (ListenerIdMainDb, req.Query);
                            actionReport (@"Получение значений из БД - состояние: " + ((StatesMachine)state).ToString ());
                        } else {
                            iRes = 1;

                            Logging.Logg ().Error ($"PanelAnalyzer_DB.HLoggingReadHandlerDb::StateRequest (state={stateMachine}) - аргумент не валиден...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                        break;
                    default:
                        break;
                }

                return iRes;
            }

            /// <summary>
            /// Обработка УСПЕШНО полученного результата
            /// </summary>
            /// <param name="state">Состояние для результата</param>
            /// <param name="table">Значение результата</param>
            /// <returns>Признак обработки результата</returns>
            protected override int StateResponse (int state, object table)
            {
                int iRes = 0;

                StatesMachine stateMachine = (StatesMachine)state;

                switch (stateMachine) {
                    case (int)StatesMachine.ServerTime:
                        m_serverTime = ((DateTime)(table as DataTable).Rows [0] [0]);
                        break;
                    case StatesMachine.ProcCheckedFilter:
                    case StatesMachine.ProcCheckedState:
                    case StatesMachine.ListMessageToUserByDate:
                    case StatesMachine.ListDateByUser:
                    case StatesMachine.CounterToTypeMessageByDate:
                        func_Completed("StateResponse", (StatesMachine)state, table, iRes);
                        break;
                    default:
                        break;
                }

                //Проверить признак крайнего в наборе состояний для обработки
                if (isLastState (state) == true) {
                    //Удалить все сообщения в строке статуса
                    ReportClear(true);
                } else
                    ;

                return iRes;
            }

            protected override void StateWarnings (int state, int req, int res)
            {
                warningReport (@"Получение значений из БД - состояние: " + ((StatesMachine)state).ToString ());
            }

            private REQUEST getFirstRequst(StatesMachine state)
            {
                REQUEST reqRes;

                if (_requests.Select (h => h.Key).Contains<StatesMachine> (state) == true)
                    try {
                        reqRes = _requests.First (h => { return (h.Key == state)
                            && (h.State == REQUEST.STATE.Ready)
                            ;
                        });
                    } catch (Exception e) {
                        reqRes = new REQUEST (state, null);
                    }
                else
                    reqRes = new REQUEST (state, null);

                return reqRes;
            }

            private void func_Completed(string nameFunc, StatesMachine state, object obj, int err)
            {
                REQUEST req =
                    getFirstRequst(state)
                    //_handlers.Pop()
                    ;

                if (req.IsEmpty == false) {
                    req.State = err == 0 ? REQUEST.STATE.Ok : REQUEST.STATE.Error;

                    //handler.Function.Invoke(obj, err);
                    EventCommandCompleted?.Invoke (req, obj as DataTable);

                    //Logging.Logg ().Debug ($"PanelAnalyzer.HLoggingReadHandlerDb::Command () - удалено {state}...", Logging.INDEX_MESSAGE.NOT_SET);
                    //_requests.Remove (req);
                } else
                    ;
            }
        }
        //}
    }
}
