using System;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Globalization; //CultureInfo
using System.ComponentModel;

using HClassLibrary;

namespace StatisticAlarm
{
    public class ViewAlarm : HHandlerQueue
    {
        /// <summary>
        /// Перечисление - индексы известных для обработки состояний
        /// </summary>
        public enum StatesMachine { Unknown = -1, List, Insert, Update }

        public delegate void DelegateOnEventReg (EventRegEventArgs e);
        public event DelegateOnEventReg EventAdd;
        /// <summary>
        /// Класс для получения данных из БД
        /// </summary>
        private class HandlerDb : HClassLibrary.HHandlerDb
        {
            /// <summary>
            /// Перечисление
            /// </summary>
            public enum StatesMachine { CurrentTime, ListEvents, InsertEventMain, InsertEventDetail, UpdateEventFixed, UpdateEventConfirm }
            /// <summary>
            /// Параметры соединения с БД_значений
            /// </summary>
            private ConnectionSettings m_connSett;
            /// <summary>
            /// Идентификатор установленного, активного соединения с БД_значений
            /// </summary>
            private int IdListener { get { return m_dictIdListeners[0][0]; } }
            
            public enum INDEX_SYNC_STATECHECKRESPONSE { UNKNOWN = -1, RESPONSE, ERROR, WARNING
                ,  COUNT_INDEX_SYNC_STATECHECKRESPONSE }
            public AutoResetEvent [] m_arSyncStateCheckResponse;

            DataTable m_tableResponse;
            /// <summary>
            /// Дата/время (дата) запроса списка событий
            /// </summary>
            private DateTime m_dtCurrent
                /// <summary>
                /// Дата/время сервера
                /// </summary>
                , m_dtServer;
            /// <summary>
            /// Индекс часа начала периода запроса списка событий в сутках
            /// </summary>
            private int m_iHourBegin
                /// <summary>
                /// Индекс часа окончания периода запроса списка событий в сутках
                /// </summary>
                , m_iHourEnd;
            /// <summary>
            /// Идентификатор записи в таблице БД_значений о событии сигнализации
            /// </summary>        
            private long m_idEventMain;
            /// <summary>
            /// Информация о событии сигнализации
            /// </summary>
            private TecView.EventRegEventArgs m_EventRegEventArg;

            public HandlerDb(ConnectionSettings connSett)
            {
                m_connSett = connSett;

                m_arSyncStateCheckResponse = new AutoResetEvent[(int)INDEX_SYNC_STATECHECKRESPONSE.COUNT_INDEX_SYNC_STATECHECKRESPONSE]
                {
                    new AutoResetEvent (false)
                    , new AutoResetEvent (false)
                    , new AutoResetEvent (false)
                };
            }

            public override void Start()
            {
                //ClearValues();

                //??? почему раньше, чем вызов базового метода
                StartDbInterfaces();

                base.Start();
            }

            public override void Stop()
            {
                m_arSyncStateCheckResponse[(int)INDEX_SYNC_STATECHECKRESPONSE.WARNING].Set ();
                
                base.Stop();
            }

            public override bool Activate(bool active)
            {
                bool bRes = base.Activate(active);

                return bRes;
            }

            public override void StartDbInterfaces()
            {
                m_dictIdListeners.Add(0, new int[] { -1 });
                m_dictIdListeners[0][0] = DbSources.Sources().Register(m_connSett, true, @"ViewAlarm");
            }

            protected override int StateCheckResponse(int state, out bool error, out object table)
            {
                int iRes = 0;

                error = false;
                table = null;

                switch ((StatesMachine)state)
                {
                    case StatesMachine.CurrentTime:
                    case StatesMachine.ListEvents:
                    case StatesMachine.InsertEventMain:
                        iRes = response(out error, out table);
                        break;
                    case StatesMachine.InsertEventDetail:
                    case StatesMachine.UpdateEventFixed:
                    case StatesMachine.UpdateEventConfirm:
                        break;
                    default:
                        iRes = -1;
                        break;
                }

                error = !(iRes == 0);

                return iRes;
            }

            protected override int StateRequest(int state)
            {
                int iRes = 0;

                switch ((StatesMachine)state)
                {
                    case StatesMachine.CurrentTime:
                        GetCurrentTimeRequest();
                        break;
                    case StatesMachine.ListEvents:
                        GetListEventsRequest();
                        break;
                    case StatesMachine.InsertEventMain:
                        GetInsertEventMainRequest();
                        break;
                    case StatesMachine.InsertEventDetail:
                        GetInsertEventDetailRequest();
                        break;
                    case StatesMachine.UpdateEventFixed:
                        GetUpdateEventFixedRequest();
                        break;
                    case StatesMachine.UpdateEventConfirm:
                        GetUpdateEventConfirmRequest();
                        break;
                    default:
                        break;
                }

                //Logging.Logg().Debug(@"ViewAlarm::StateRequest () - state=" + ((StatesMachine)state).ToString() + @", result=" + bRes.ToString() + @" - вЫход...");

                return iRes;
            }

            protected override int StateResponse(int state, object obj)
            {
                int iRes = 0;
                DataTable tableRes = null;
                bool bAnswer = true;

                switch ((StatesMachine)state)
                {
                    case StatesMachine.CurrentTime:
                        GetCurrentTimeResponse(obj as DataTable);
                        bAnswer = false;
                        break;
                    case StatesMachine.ListEvents:
                        break;
                    case StatesMachine.InsertEventMain:
                        GetInsertEventMainResponse(obj as DataTable);
                        break;
                    case StatesMachine.InsertEventDetail:
                    case StatesMachine.UpdateEventFixed:
                    case StatesMachine.UpdateEventConfirm:
                        // ответа не требуется
                        bAnswer = false;
                        break;
                    default:
                        break;
                }

                if (bAnswer == true)
                {
                    //Сохранить ответ
                    m_tableResponse = (obj as DataTable).Copy ();
                    //Указать, что ответ готов
                    m_arSyncStateCheckResponse[(int)INDEX_SYNC_STATECHECKRESPONSE.RESPONSE].Set();
                }
                else
                    ;

                //Logging.Logg().Debug(@"ViewAlarm::StateRequest () - state=" + ((StatesMachine)state).ToString() + @", result=" + bRes.ToString() + @" - вЫход...");

                return iRes;
            }

            protected override HHandler.INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
            {
                m_arSyncStateCheckResponse[(int)INDEX_SYNC_STATECHECKRESPONSE.ERROR].Set();
                
                Logging.Logg().Error(@"ViewAlarm::StateErrors () - state=" + ((StatesMachine)state).ToString() + @", req=" + req.ToString() + @", res=" + res.ToString(), Logging.INDEX_MESSAGE.NOT_SET);

                return INDEX_WAITHANDLE_REASON.SUCCESS;
            }

            protected override void StateWarnings(int state, int req, int res)
            {
                m_arSyncStateCheckResponse[(int)INDEX_SYNC_STATECHECKRESPONSE.WARNING].Set ();
                
                Logging.Logg().Warning(@"ViewAlarm::StateWarnings () - state=" + ((StatesMachine)state).ToString() + @", req=" + req.ToString() + @", res=" + res.ToString(), Logging.INDEX_MESSAGE.NOT_SET);
            }

            public override void ClearValues()
            {//??? - необязательное наличие - удалить из 'HHandlerDb'
                m_idEventMain = -1;
                m_EventRegEventArg = null;
            }

            public int Response(out bool error, out object table)
            {
                int iRes = 0;

                error = false;
                table = m_tableResponse;

                return iRes;
            }

            protected void GetCurrentTimeRequest()
            {
                GetCurrentTimeRequest(DbTSQLInterface.getTypeDB(m_connSett.port), IdListener);
            }

            private void GetCurrentTimeResponse(DataTable tableRes)
            {
                m_dtServer = (DateTime)tableRes.Rows[0][0];
            }

            private void GetListEventsRequest()
            {
                Request(IdListener
                    , @"SELECT * FROM [dbo].[AlarmEvent] WHERE [DATETIME_REGISTRED] BETWEEN '"
                        + m_dtCurrent.AddHours(m_iHourBegin).ToString(@"yyyyMMdd HH:mm") + @"' AND '"
                        + m_dtCurrent.AddHours(m_iHourEnd).ToString(@"yyyyMMdd HH:mm") + @"'");
            }            
            /// <summary>
            /// Сформировать содержимое запроса к БД и отправить для выполнения
            ///  , по сложивжейся традиции, несмотря на 'Get', тип возвращаемого значения 'void'
            /// </summary>
            private void GetInsertEventMainRequest()
            {
                int id = m_EventRegEventArg.m_id_tg < 0 ? m_EventRegEventArg.m_id_gtp : m_EventRegEventArg.m_id_tg //ID_COMPONENT
                    , typeAlarm = (m_EventRegEventArg.m_id_tg < 0 ? 1 : 2) //TYPE
                    , id_user = 0; //ID_USER
                double val = m_EventRegEventArg.m_id_tg < 0 ? -1 : m_EventRegEventArg.m_listEventDetail[0].value; //VALUE
                string strDTRegistred = m_EventRegEventArg.m_dtRegistred.ToString(@"yyyyMMdd HH:mm:ss.fff"); //DATETIME_REGISTRED
                //Запрос для вставки записи о событии сигнализации
                string query = "INSERT INTO [dbo].[AlarmEvent] ([ID_COMPONENT],[TYPE],[VALUE],[DATETIME_REGISTRED],[ID_USER_REGISTRED],[DATETIME_FIXED],[ID_USER_FIXED],[DATETIME_CONFIRM],[ID_USER_CONFIRM],[CNT_RETRY],[INSERT_DATETIME],[MESSAGE]) VALUES"
                    + @" ("
                        + id + @", " //ID_COMPONENT
                        + typeAlarm + @", " //TYPE                        
                        + val.ToString(@"F3", CultureInfo.InvariantCulture) + @", " //VALUE
                        + @"'" + strDTRegistred + @"', " //DATETIME_REGISTRED
                        + id_user + @", " //ID_USER_REGISTRED
                        + "NULL" + @", " //DATETIME_FIXED
                        + "NULL" + @", " //ID_USER_FIXED
                        + "NULL" + @", " //DATETIME_CONFIRM
                        + "NULL" + @", " //ID_USER_CONFIRM
                        + 0 + @", " //CNT_RETRY
                        + "GETDATE ()" + @", " //INSERT_DATETIME
                        + @"'" + m_EventRegEventArg.m_message + @"'" //MESSAGE
                    + @")";
                query += @";";
                //??? Переход на новую строку
                //query += "\r\n";
                //Запрос на получение идентификатора вставленной записи
                query += @"SELECT * FROM [dbo].[AlarmEvent] WHERE "
                    + @"[ID_COMPONENT]=" + id
                    + @" AND [TYPE]=" + typeAlarm                    
                    + @" AND [DATETIME_REGISTRED]='" + strDTRegistred + @"'"
                    + @" AND [ID_USER_REGISTRED]=" + id_user;
                query += @";";

                Request(IdListener, query);
            }

            private void GetInsertEventMainResponse(object obj)
            {
                m_idEventMain = (long)(obj as DataTable).Rows[0][@"ID"];
            }

            private void GetInsertEventDetailRequest()
            {
                string query = @"INSERT INTO [dbo].[AlarmDetail] VALUES ";

                foreach (TecView.EventRegEventArgs.EventDetail detail in m_EventRegEventArg.m_listEventDetail)
                {
                    if (detail.value > 0)
                    {
                        query += @"(";

                        query += m_idEventMain + @", ";
                        query += detail.id + @", ";
                        query += detail.value.ToString(@"F3", CultureInfo.InvariantCulture) + @", ";
                        query += @"'" + detail.last_changed_at.ToString(@"yyyyMMdd HH:mm:ss.fff") + @"', ";
                        query += detail.id_tm + @", ";
                        query += @"GETDATE()";

                        query += @"),";
                    }
                    else
                        ;
                }
                //Не учитывать крайнюю запятую
                query = query.Substring(0, query.Length - 1);

                Request(IdListener, query);
            }

            private void GetUpdateEventFixedRequest()
            {
            }

            private void GetUpdateEventConfirmRequest()
            {
            }

            public void Refresh(DateTime dtCurrent, int iHourBegin, int iHourEnd)
            {
                m_dtCurrent = dtCurrent;
                m_iHourBegin = iHourBegin;
                m_iHourEnd = iHourEnd;

                refresh ();
            }

            private void refresh()
            {
                ClearStates();

                states.Add((int)StatesMachine.CurrentTime);
                states.Add((int)StatesMachine.ListEvents);

                Run(@"ViewAlarm.HHandlerDb::Refresh");
            }

            public void Insert(TecView.EventRegEventArgs ev)
            {
                ClearStates();

                ClearValues();

                m_EventRegEventArg = ev;

                states.Add((int)StatesMachine.CurrentTime);
                states.Add((int)StatesMachine.InsertEventMain);
                states.Add((int)StatesMachine.InsertEventDetail);

                Run(@"ViewAlarm::Insert");
            }
        }
        /// <summary>
        /// Объект для получения данных из БД
        /// </summary>
        private ViewAlarm.HandlerDb m_handlerDb;
        ///// <summary>
        ///// Событие для отправки списка событий сигнализаций клиентам
        ///// </summary>
        //public event DelegateObjectFunc EvtGetData;

        private BackgroundWorker m_threadListEventsResponse;
        /// <summary>
        /// Конструктор - основной (с параметрами)
        /// </summary>
        /// <param name="connSett">Параметры соединения с БД_значений</param>
        public ViewAlarm(ConnectionSettings connSett)
            : base()
        {
            m_handlerDb = new ViewAlarm.HandlerDb (connSett);

            m_threadListEventsResponse = new BackgroundWorker ();
            m_threadListEventsResponse.DoWork += new DoWorkEventHandler(fThreadListEventsResponse_DoWork);
            m_threadListEventsResponse.RunWorkerCompleted += new RunWorkerCompletedEventHandler(fThreadListEventsResponse_RunWorkerCompleted);
        }

        public override void Start()
        {
            base.Start();

            m_handlerDb.Start ();
        }

        public override void Stop()
        {
            m_handlerDb.Stop ();
            
            base.Stop();
        }

        public class EventRegEventArgs
        {
            public int m_id_comp;
            public DateTime m_dtRegistred;
            public int m_situation;
            public string m_message;
            
            public EventRegEventArgs (int id_comp, DateTime dtReg, int s, string mes)
            {
                m_id_comp = id_comp;
                m_dtRegistred = dtReg;
                m_situation = s;
                m_message = mes;
            }
        }

        private void fThreadListEventsResponse_DoWork (object obj, DoWorkEventArgs ev)
        {
            DataTable tableRes = ev.Argument as DataTable;
            DataRow []rowsUnFixed = tableRes.Select (@"[DATETIME_FIXED] IS NULL", @"[DATETIME_REGISTRED]");
            foreach (DataRow r in rowsUnFixed)
                EventAdd (new EventRegEventArgs ((int)r[@"ID_COMPONENT"]
                                                , (DateTime)r[@"DATETIME_REGISTRED"]
                                                , TecView.EventRegEventArgs.GetSituation ((string)r[@"MESSAGE"])
                                                , (string)r[@"MESSAGE"]));
            
            Console.WriteLine("\tнезарегистрированных: {0}", rowsUnFixed.Length);
        }

        private void fThreadListEventsResponse_RunWorkerCompleted(object obj, RunWorkerCompletedEventArgs ev)
        {
            Console.WriteLine(@"ViewAlarm::fThreadListEventsResponse_RunWorkerCompleted () - Ok...");
        }

        private void GetInsertEventMainResponse(DataTable tableRes)
        {
            long idEventMain = (long)tableRes.Rows[0][@"ID"];
            Console.WriteLine(@"Добавлена запись о событии сигнализации: Id=" + idEventMain);            
        }
        /// <summary>
        /// Функция обработки результатов запроса
        /// </summary>
        /// <param name="tableRes">Таблица - результат запроса</param>
        private void GetListEventsResponse(ItemQueue itemQueue, DataTable tableRes)
        {
            Console.WriteLine(@"Событий за " + ((DateTime)itemQueue.Pars[0]).ToShortDateString()
                + @" (" + ((int)itemQueue.Pars[1]) + @"-" + ((int)itemQueue.Pars[2]) + @" ч): "
                + tableRes.Rows.Count);

            m_threadListEventsResponse.RunWorkerAsync(tableRes);
        }

        public void OnEvtDataAskedHost_PanelAlarmJournal(object obj)
        {
            EventArgsDataHost ev = obj as EventArgsDataHost;
            Push (ev.reciever, new object [] { ev.par as object [] });
        }

        protected override int StateCheckResponse(int state, out bool error, out object table)
        {
            int iRes = 0;
            ViewAlarm.HandlerDb.INDEX_SYNC_STATECHECKRESPONSE indxSync = ViewAlarm.HandlerDb.INDEX_SYNC_STATECHECKRESPONSE.UNKNOWN;

            error = false;
            table = null;

            switch ((StatesMachine)state)
            {
                case StatesMachine.List:
                case StatesMachine.Insert:
                    indxSync = (ViewAlarm.HandlerDb.INDEX_SYNC_STATECHECKRESPONSE)WaitHandle.WaitAny(m_handlerDb.m_arSyncStateCheckResponse);
                    switch (indxSync)
                    {
                        case HandlerDb.INDEX_SYNC_STATECHECKRESPONSE.RESPONSE:
                            iRes = m_handlerDb.Response (out error, out table);
                            break;
                        case HandlerDb.INDEX_SYNC_STATECHECKRESPONSE.ERROR:
                            iRes = -1;
                            error = true;
                            break;
                        case HandlerDb.INDEX_SYNC_STATECHECKRESPONSE.WARNING:
                            iRes = 1;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    iRes = -1;
                    break;
            }

            error = !(iRes == 0);

            return iRes;
        }

        protected override int StateRequest(int state)
        {
            int iRes = 0;
            ItemQueue itemQueue = Peek;

            switch ((StatesMachine)state)
            {
                case StatesMachine.List:
                    m_handlerDb.Refresh ((DateTime)itemQueue.Pars[0], (int)itemQueue.Pars[1], (int)itemQueue.Pars[2]);
                    break;
                case StatesMachine.Insert:
                    m_handlerDb.Insert (itemQueue.Pars[0] as TecView.EventRegEventArgs);
                    break;
                default:
                    break;
            }

            //Logging.Logg().Debug(@"ViewAlarm::StateRequest () - state=" + ((StatesMachine)state).ToString() + @", result=" + bRes.ToString() + @" - вЫход...");

            return iRes;
        }

        protected override int StateResponse(int state, object obj)
        {
            int iRes = 0;
            ItemQueue itemQueue = Peek;
            DataTable tableRes = obj as DataTable;

            switch ((StatesMachine)state)
            {
                case StatesMachine.List:
                    GetListEventsResponse(itemQueue, tableRes);
                    break;
                case StatesMachine.Insert:
                    GetInsertEventMainResponse (tableRes);
                    break;
                default:
                    break;
            }

            //??? прямой вызов метода-обработчика..., ??? использование объекта, полученного в качестве параметра... - очень некрасиво, и, возможно - неправильно
            itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(new EventArgsDataHost(-1, new object[] { (StatesMachine)state, tableRes }));

            //Logging.Logg().Debug(@"ViewAlarm::StateRequest () - state=" + ((StatesMachine)state).ToString() + @", result=" + bRes.ToString() + @" - вЫход...");

            return iRes;
        }

        protected override HHandler.INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
        {
            Logging.Logg().Error(@"ViewAlarm::StateErrors () - state=" + ((StatesMachine)state).ToString() + @", req=" + req.ToString() + @", res=" + res.ToString(), Logging.INDEX_MESSAGE.NOT_SET);

            return INDEX_WAITHANDLE_REASON.SUCCESS;
        }

        protected override void StateWarnings(int state, int req, int res)
        {
            Logging.Logg().Warning(@"ViewAlarm::StateWarnings () - state=" + ((StatesMachine)state).ToString() + @", req=" + req.ToString() + @", res=" + res.ToString(), Logging.INDEX_MESSAGE.NOT_SET);
        }
    }
}
