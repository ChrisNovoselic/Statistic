using System;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Globalization; //CultureInfo

using HClassLibrary;

namespace StatisticCommon
{
    public class ViewAlarm : HHandlerQueue
    {
        private class ViewAlarmHandlerDb : HHandlerDb
        {
            /// <summary>
            /// Перечисление
            /// </summary>
            private enum StatesMachine { CurrentTime, ListEvents, InsertEventMain, InsertEventDetail, UpdateEventFixed, UpdateEventConfirm }
            /// <summary>
            /// Параметры соединения с БД_значений
            /// </summary>
            private ConnectionSettings m_connSett;
            /// <summary>
            /// Идентификатор установленного, активного соединения с БД_значений
            /// </summary>
            private int IdListener { get { return m_dictIdListeners[0][0]; } }

            public override void Start()
            {
                //ClearValues();

                StartDbInterfaces();

                base.Start();
            }

            public override void Stop()
            {
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

                switch ((StatesMachine)state)
                {
                    case StatesMachine.CurrentTime:
                        GetCurrentTimeResponse(obj as DataTable);
                        break;
                    case StatesMachine.ListEvents:
                        GetListEventsResponse(obj as DataTable);
                        break;
                    case StatesMachine.InsertEventMain:
                        GetInsertEventMainResponse(obj as DataTable);
                        break;
                    case StatesMachine.InsertEventDetail:
                    case StatesMachine.UpdateEventFixed:
                    case StatesMachine.UpdateEventConfirm:
                        ;
                        break;
                    default:
                        break;
                }

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

            public override void ClearValues()
            {//??? - необязательное наличие - удалить из 'HHandlerDb'
                m_idEventMain = -1;
                m_EventRegEventArg = null;
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
            /// Функция обработки результатов запроса
            /// </summary>
            /// <param name="tableRes">Таблица - результат запроса</param>
            private void GetListEventsResponse(DataTable tableRes)
            {
                Console.WriteLine(@"Событий за " + m_dtCurrent.ToShortDateString() + @" (" + m_iHourBegin + @"-" + m_iHourEnd + @" ч): " + tableRes.Rows.Count);
                EvtGetData(tableRes);
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
                string query = "INSERT INTO [dbo].[AlarmEvent] ([ID_COMPONENT], [TYPE], [ID_USER], [VALUE], [DATETIME_REGISTRED], [DATETIME_FIXED], [DATETIME_CONFIRM], [INSERT_DATETIME], [MESSAGE]) VALUES"
                    + @" ("
                        + id + @", " //ID_COMPONENT
                        + typeAlarm + @", " //TYPE
                        + id_user + @", " //ID_USER
                        + val.ToString(@"F3", CultureInfo.InvariantCulture) + @", " //VALUE
                        + @"'" + strDTRegistred + @"', " //DATETIME_REGISTRED
                        + "NULL" + @", " //DATETIME_FIXED
                        + "NULL" + @", " //DATETIME_CONFIRM
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
                    + @" AND [ID_USER]=" + id_user
                    + @" AND [DATETIME_REGISTRED]='" + strDTRegistred + @"'";
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

            public void OnEventDateChanged(object obj, DateRangeEventArgs ev)
            {
                m_dtCurrent = ev.Start.Date; //End.Date - эквивалентно, при 'MaxSelectionCount = 1'

                Refresh();
            }

            public void Refresh()
            {
                ClearStates();

                states.Add((int)StatesMachine.CurrentTime);
                states.Add((int)StatesMachine.ListEvents);

                Run(@"ViewAlarm::Refresh");
            }

            public void Refresh(DateTime dtCurrent, int iHourBegin, int iHourEnd)
            {
                m_dtCurrent = dtCurrent;
                m_iHourBegin = iHourBegin;
                m_iHourEnd = iHourEnd;

                Refresh();
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

        private ViewAlarmHandlerDb m_handlerDb;
        /// <summary>
        /// Событие для отправки списка событий сигнализаций клиентам
        /// </summary>
        public event DelegateObjectFunc EvtGetData;        
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
        /// <summary>
        /// Конструктор - основной (с параметрами)
        /// </summary>
        /// <param name="connSett">Параметры соединения с БД_значений</param>
        public ViewAlarm(ConnectionSettings connSett)
            : base()
        {
            m_connSett = connSett;
        }
    }
}
