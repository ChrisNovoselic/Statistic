using System;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using System.Globalization; //CultureInfo
using System.ComponentModel;

using HClassLibrary;
using StatisticCommon;

namespace StatisticAlarm
{
    public partial class AdminAlarm : HHandlerQueue
    {
        /// <summary>
        /// Режим работы вкладки
        /// </summary>
        public MODE Mode;
        /// <summary>
        /// Класс для хранения значений о событии сигнализации
        /// </summary>
        public class AlarmDbEventArgs : AlarmNotifyEventArgs
        {
            public int m_id_user_registred
                , m_id_user_fixed
                , m_id_user_confirm;
            public DateTime? 
                m_dtFixed
                , m_dtConfirm;

            public AlarmDbEventArgs(DataRow rowEvt)
                : base((int)rowEvt[@"ID_COMPONENT"]
                    , (DateTime)rowEvt[@"DATETIME_REGISTRED"]
                    , (int)rowEvt[@"SITUATION"])
            {
                //Регистрация события
                //m_id_comp, [ID_COMP]
                m_id_user_registred = (int)rowEvt[@"ID_USER_REGISTRED"];
                //m_dtRegistred, [DATETIME_REGISTRED]
                //Фиксация события
                if (! (rowEvt[@"ID_USER_FIXED"] is System.DBNull))
                    m_id_user_fixed = (int)rowEvt[@"ID_USER_FIXED"];
                else
                    m_id_user_fixed = -1;
                if (!(rowEvt[@"DATETIME_FIXED"] is System.DBNull))
                    m_dtFixed = (DateTime)rowEvt[@"DATETIME_FIXED"];
                else
                    m_dtFixed = null;
                //Подтверждение события
                if (!(rowEvt[@"ID_USER_CONFIRM"] is System.DBNull))
                    m_id_user_confirm = (int)rowEvt[@"ID_USER_CONFIRM"];
                else
                    m_id_user_confirm = -1;
                if (!(rowEvt[@"DATETIME_CONFIRM"] is System.DBNull))
                    m_dtConfirm = (DateTime)rowEvt[@"DATETIME_CONFIRM"];
                else
                    m_dtConfirm = null;
                //m_situation, [SUTUATION]
                //m_message_shr, [SUTUATION]
            }
        }
        /// <summary>
        /// Перечисление - индексы известных для обработки состояний
        /// </summary>
        private enum StatesMachine { Unknown = -1, List, Notify, Detail, Insert, Fixed, Confirm }
        /// <summary>
        /// Объект для учета событий сигнализации и их состояний
        /// </summary>
        private DictAlarmObject m_dictAlarmObject;
        /// <summary>
        /// Тип функции - для обработки события регистрации события (случая) сигнализаций из БД
        /// </summary>
        /// <param name="ev">Аргумент ...</param>
        public delegate void AlarmDbEventHandler(AlarmDbEventArgs ev);
        /// <summary>
        /// События для оповещения панели
        /// </summary>
        public event AlarmNotifyEventHandler EventAdd
            , EventRetry;
        /// <summary>
        /// Событие регистрации события (случая) сигнализаций из БД
        /// </summary>
        private event AlarmDbEventHandler EventReg;
        /// <summary>
        /// Признак актуальности зарегистрированных событий сигнализаций из БД
        /// </summary>
        bool m_bAlarmDbEventUpdated;
        /// <summary>
        /// Таймер для запроса актуального перечня событий сигнализаций
        /// </summary>
        private System.Threading.Timer m_timerView;

        public delegate void DatetimeCurrentEventHandler(DatetimeCurrentEventArgs ev);
        public class DatetimeCurrentEventArgs : EventArgs
        {
            public DateTime Date;
            public int HourBegin
                , HourEnd;

            public DatetimeCurrentEventArgs(DateTime date, int iHourBegin, int iHourEnd)
            {
                this.Date = date;
                this.HourBegin = iHourBegin;
                this.HourEnd = iHourEnd;
            }
        }

        private DatetimeCurrentEventArgs m_dtCurrent;
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
            private TecViewAlarm.AlarmTecViewEventArgs m_EventRegEventArg;

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

                switch ((StatesMachine)state)
                {
                    case StatesMachine.CurrentTime:
                        GetCurrentTimeResponse(obj as DataTable);
                        break;
                    case StatesMachine.ListEvents:
                        break;
                    case StatesMachine.InsertEventMain:
                        GetInsertEventMainResponse(obj as DataTable);
                        break;
                    case StatesMachine.InsertEventDetail:
                        break;
                    case StatesMachine.UpdateEventFixed:
                    case StatesMachine.UpdateEventConfirm:
                        // ответа не требуется
                        break;
                    default:
                        break;
                }

                if (isLastState (state) == true)
                {
                    //Проверить необходимость сохранения результата запроса
                    if (! (obj == null))
                        //Сохранить результат в "выходную" переменную
                        m_tableResponse = (obj as DataTable).Copy ();
                    else
                        ;
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
                StatisticCommon.FormChangeMode.MODE_TECCOMPONENT mode = StatisticCommon.TECComponent.Mode(m_EventRegEventArg.m_id_comp);
                int typeAlarm = (mode == FormChangeMode.MODE_TECCOMPONENT.GTP) ? 1 :
                        (mode == FormChangeMode.MODE_TECCOMPONENT.TG) ? 2 :
                            (int)FormChangeMode.MODE_TECCOMPONENT.UNKNOWN //TYPE
                    , id_user = 0; //ID_USER
                double val = (mode == FormChangeMode.MODE_TECCOMPONENT.GTP) ? -1F :
                    (mode == FormChangeMode.MODE_TECCOMPONENT.TG) ? m_EventRegEventArg.m_listEventDetail[0].value :
                        -2F; //VALUE
                string strDTRegistred = m_EventRegEventArg.m_dtRegistred.GetValueOrDefault().ToString(@"yyyyMMdd HH:mm:ss.fff"); //DATETIME_REGISTRED
                //Запрос для вставки записи о событии сигнализации
                string query = "INSERT INTO [dbo].[AlarmEvent] ([ID_COMPONENT],[TYPE],[VALUE],[DATETIME_REGISTRED],[ID_USER_REGISTRED],[DATETIME_FIXED],[ID_USER_FIXED],[DATETIME_CONFIRM],[ID_USER_CONFIRM],[CNT_RETRY],[INSERT_DATETIME],[SITUATION]) VALUES"
                    + @" ("
                        + m_EventRegEventArg.m_id_comp + @", " //ID_COMPONENT
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
                        + @"'" + m_EventRegEventArg.m_situation + @"'" //SITUATION
                    + @")";
                query += @";";
                //??? Переход на новую строку
                //query += "\r\n";
                query += Environment.NewLine;
                //Запрос на получение идентификатора вставленной записи
                query += @"SELECT * FROM [dbo].[AlarmEvent] WHERE "
                    + @"[ID_COMPONENT]=" + m_EventRegEventArg.m_id_comp
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

                foreach (TecViewAlarm.AlarmTecViewEventArgs.EventDetail detail in m_EventRegEventArg.m_listEventDetail)
                {
                    if (detail.value > 0)
                    {
                        if (! (m_idEventMain < 0))
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
                            throw new Exception(@"ViewAlarm.HandlerDb::GetInsertEventDetailRequest () - idEventMain=" + m_idEventMain);
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
                lock (this)
                {
                    ClearValues();
                    ClearStates();                    

                    states.Add((int)StatesMachine.CurrentTime);
                    states.Add((int)StatesMachine.ListEvents);

                    Run(@"ViewAlarm.HHandlerDb::Refresh");
                }
            }

            public void Insert(TecViewAlarm.AlarmTecViewEventArgs ev)
            {
                lock (this)
                {
                    ClearValues();
                    ClearStates();                    

                    m_EventRegEventArg = ev;

                    states.Add((int)StatesMachine.CurrentTime);
                    states.Add((int)StatesMachine.InsertEventMain);
                    states.Add((int)StatesMachine.InsertEventDetail);

                    Run(@"ViewAlarm::Insert");
                }
            }
        }
        /// <summary>
        /// Объект для получения данных из БД
        /// </summary>
        private AdminAlarm.HandlerDb m_handlerDb;
        /// <summary>
        /// Событие для отправки списка событий/детализации сигнализаций клиентам
        /// </summary>
        public event DelegateObjectFunc EvtGetDataMain
            , EvtGetDataDetail;
        /// <summary>
        /// Объект потока для обработки рез-та запроса
        ///  на получение списка событий сигнализаций
        /// </summary>
        private BackgroundWorker m_threadNotifyResponse;
        /// <summary>
        /// Конструктор - основной (с параметрами)
        /// </summary>
        /// <param name="connSett">Параметры соединения с БД_значений</param>
        /// <param name="mode">Режим работы объекта</param>
        /// <param name="ev">Инициализация значений даты, часов начало, окончания запроса списка событий</param>
        public AdminAlarm(ConnectionSettings connSett, MODE mode, DatetimeCurrentEventArgs ev, bool bWorkCheked)
            : base()
        {
            Mode = mode;
            m_dtCurrent = ev;

            lockValue = new object();

            m_dictAlarmObject = new DictAlarmObject();
            EventReg += new AlarmDbEventHandler(onEventReg);

            m_bAlarmDbEventUpdated = false;

            m_handlerDb = new AdminAlarm.HandlerDb(connSett);

            //Инициализировать таймер для оповещение_список/оповещение_сигнал
            m_timerView = new System.Threading.Timer(new TimerCallback(fTimerView_Tick), null, Timeout.Infinite, Timeout.Infinite);

            m_timerAlarm =
                new System.Threading.Timer(new TimerCallback(TimerAlarm_Tick), null, Timeout.Infinite, Timeout.Infinite)
                //new System.Windows.Forms.Timer ()
                ;
            //m_timerAlarm.Tick += new EventHandler(TimerAlarm_Tick);

            m_threadNotifyResponse = new BackgroundWorker ();
            m_threadNotifyResponse.DoWork += new DoWorkEventHandler(fThreadNotifyResponse_DoWork);
            m_threadNotifyResponse.RunWorkerCompleted += new RunWorkerCompletedEventHandler(fThreadNotifyResponse_RunWorkerCompleted);
        }

        public override void Start()
        {
            base.Start();

            m_handlerDb.Start ();

            if (Mode == MODE.SERVICE)
            {
                foreach (TecViewAlarm tv in m_listTecView)
                    tv.Start(); //StartDbInterfaces (CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE);
            }
            else ;

            OnWorkCheckedChanged(true);
        }

        public override void Stop()
        {
            m_handlerDb.Stop ();

            activateAdminAlarm(false);
            foreach (TecViewAlarm tv in m_listTecView)
                tv.Stop ();

            if (! (m_timerAlarm == null))
            {
                // таймер уже был остановлен в 'activateAdminAlarm'
                m_timerAlarm.Dispose ();
                m_timerAlarm = null;
            }
            else
                ;

            activateViewAlarm(false);
            if (! (m_timerView == null))
            {
                // таймер уже был остановлен в 'activateViewAlarm'
                m_timerView.Dispose();
                m_timerView = null;
            }
            else
                ;

            base.Stop();
        }

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate(active);

            if (bRes == true)
                if (active == true)
                {

                }
                else
                    ;
            else
                ;

            return bRes;
        }

        private void activateAdminAlarm(bool bChecked)
        {
            foreach (TecViewAlarm tv in m_listTecView)
                tv.Activate(bChecked);

            m_timerAlarm.Change(bChecked == true ? 0 : System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);            
        }

        private void activateViewAlarm(bool bChecked)
        {
            m_timerView.Change(bChecked == true ? 0 : System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            m_bAlarmDbEventUpdated = bChecked;
        }
        /// <summary>
        /// Обработчик события изменение признака "Включено/отключено"
        /// </summary>
        /// <param name="obj">Объект, иницировавший событие</param>
        public void OnWorkCheckedChanged(bool bChecked)
        {
            //??? - Активировать объект чтения/записи/обновления списка событий
            if (Mode == MODE.SERVICE)
                activateAdminAlarm(bChecked);
            else
                ;

            activateViewAlarm(bChecked);
        }

        private void startTimerView(int interval = 6)
        {
            m_timerView.Change(0, System.Threading.Timeout.Infinite);
        }

        private void stopTimerView()
        {
            m_timerView.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }
        /// <summary>
        /// Поставить в очередь обработки событие - запрос перечня событий сигнализаций из БД
        /// </summary>
        private void pushList()
        {
            //Поставить в очередь обработки
            push(new object [] //Перечень событий для обработки
                    { new object [] //1-е событие
                        {
                            StatesMachine.List
                            , m_dtCurrent.Date
                            , m_dtCurrent.HourBegin
                            , m_dtCurrent.HourEnd
                        }
                    });
        }

        /// <summary>
        /// Поставить в очередь обработки событие - запрос перечня событий сигнализаций из БД - ткущих!!!
        /// </summary>
        private void pushNotify()
        {
            //DateTime dtNotify = HAdmin.ToMoscowTimeZone();
            //Поставить в очередь обработки
            push(new object[] //Перечень событий для обработки
                    { new object [] //1-е событие
                        {
                            StatesMachine.Notify
                            , HAdmin.ToMoscowTimeZone().Date //dtNotify.Date
                            , 0
                            , 24
                        }
                    });
        }

        private void push(object []pars)
        {
            //Поставить в очередь обработки
            // null - получатель/отправитель
            Push(null, new object[] //???
                { pars });
        }
        /// <summary>
        /// Метод обратного вызова для таймера обновления значений в таблице
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void fTimerView_Tick(object obj)
        {
            // поставить в очередь событие получения списка событий сигнализаций
            //  за указанную дату/часы только, если дата текущая
            if (isToday == true)                
                pushList();
            else
                ;
            // поставить в очередь событие получения списка текущих событий сигнализаций
            pushNotify();

            m_timerView.Change(PanelStatistic.POOL_TIME * 1000, System.Threading.Timeout.Infinite);
        }
        /// <summary>
        /// Обработчик события - изменение даты, номера часа начала и окончания
        /// </summary>
        /// <param name="ev"></param>
        public void OnEventDatetimeChanged(DatetimeCurrentEventArgs ev)
        {
            m_dtCurrent = ev;

            pushList ();
            ////Проверить равенство установленной даты и текущей
            //// в случае успеха - отодвинуть вызов потоковой функции таймера
            //if (isToday == true)
            //    m_timerView.Change(PanelStatistic.POOL_TIME, System.Threading.Timeout.Infinite);
            //else
            //    ;
        }
        /// <summary>
        /// Признак постановки в очередь обработки событий - запроса на получение списка собтий сигнализаций
        ///  по заданной пользователем дате
        /// </summary>
        private bool isToday { get { return m_dtCurrent.Date.Equals(HAdmin.ToMoscowTimeZone ().Date); } }
        /// <summary>
        /// Потоковая функция обработки результата запроса списка событий сигнализаций
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (??? поток)</param>
        /// <param name="ev">Аргумент события начала выполнения потока</param>
        private void fThreadNotifyResponse_DoWork (object obj, DoWorkEventArgs ev)
        {
            DataTable tableRes = ev.Argument as DataTable;
            //DataRow []rowsUnFixed = tableRes.Select (@"[DATETIME_FIXED] IS NULL", @"[DATETIME_REGISTRED]");
            foreach (DataRow r in tableRes.Rows)
                EventReg(new AlarmDbEventArgs(r));

            ev.Result = @"Ok";
        }
        /// <summary>
        /// Обработчик события - завершения выполнения потоковой функции по обработке
        ///  результатов запроса списка событий сигнализаций
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (??? поток)</param>
        /// <param name="ev">Аргумент события окончанмя выполнения потоковой функции</param>
        private void fThreadNotifyResponse_RunWorkerCompleted(object obj, RunWorkerCompletedEventArgs ev)
        {
            if (ev.Error == null)
            {
                Console.WriteLine(@"ViewAlarm::fThreadNotifyResponse_RunWorkerCompleted () - " + ev.Result + @"...");

                m_bAlarmDbEventUpdated = true;
            }
            else
                Logging.Logg().Exception(ev.Error, Logging.INDEX_MESSAGE.NOT_SET, @"ViewAlarm::fThreadListEventsResponse_DoWork () - ...");
        }
        /// <summary>
        /// Функция обработки результатов запроса по указанной дате/часам
        /// </summary>
        /// <param name="tableRes">Таблица - результат запроса</param>
        private void GetListEventsResponse(ItemQueue itemQueue, DataTable tableRes)
        {
            Console.WriteLine(@"Событий за " + ((DateTime)itemQueue.Pars[0]).ToShortDateString()
                + @" (" + ((int)itemQueue.Pars[1]) + @"-" + ((int)itemQueue.Pars[2]) + @" ч): "
                + tableRes.Rows.Count);

            if (Actived == true)
                EvtGetDataMain(tableRes);
            else
                ;
        }
        /// <summary>
        /// Функция обработки результатов запроса текущих!!! событий сигнализаций
        /// </summary>
        /// <param name="tableRes">Таблица - результат запроса</param>
        private void GetNotifyResponse(ItemQueue itemQueue, DataTable tableRes)
        {
            m_threadNotifyResponse.RunWorkerAsync(tableRes);
        }

        private void GetEventDetailResponse(ItemQueue itemQueue, DataTable tableRes)
        {
            EvtGetDataDetail(tableRes);
        }
        /// <summary>
        /// Обработчик события регистрации события из БД
        /// </summary>
        /// <param name="ev">Аргумент события - описание события сигнализации</param>
        private void onEventReg(AlarmDbEventArgs ev)
        {
            INDEX_ACTION iAction = m_dictAlarmObject.Registred (ev);
            if (iAction == INDEX_ACTION.ERROR)
                throw new Exception(@"ViewAlarm::onEventReg () - ...");
            else
                //Проверить режим работы - оповещение только для 'ADMIN'
                if ((Mode == MODE.ADMIN)
                    || (Mode == MODE.SERVICE) //??? для отладки
                    )
                    if (iAction == INDEX_ACTION.NEW)
                        EventAdd(ev);
                    else
                        if (iAction == INDEX_ACTION.RETRY)
                            EventRetry(ev);
                        else
                            ;
                else
                    ;
        }
        /// <summary>
        /// Обработчик события - подтверждение события сигнализации от панели (пользователя)
        /// </summary>
        /// <param name="id_comp">Часть составного ключа: идентификатор ГТП</param>
        /// <param name="id_tg">Часть составного ключа: идентификатор ТГ</param>
        public void OnEventConfirm (AlarmNotifyEventArgs ev)
        {
            Logging.Logg().Debug(@"ViewAlarm::OnEventConfirm (id=" + ev.m_id_comp.ToString() + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);

            if (!(m_dictAlarmObject.Confirmed(ev.m_id_comp, ev.m_dtRegistred.GetValueOrDefault()) < 0))
            {
                push(new object[] //Перечень событий
                    {
                        new object [] //1-е событие
                        {
                            StatesMachine.Confirm
                            , ev.m_id_comp
                            , ev.m_dtRegistred
                        }
                    });

                if (StatisticCommon.TECComponent.Mode(ev.m_id_comp) == FormChangeMode.MODE_TECCOMPONENT.TG)
                    tgConfirm(ev.m_id_comp);
                else
                    ;
            }
            else
                Logging.Logg().Error(@"ViewAlarm::OnEventConfirm (id=" + ev.m_id_comp.ToString() + @") - НЕ НАЙДЕН!", Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// Возвратить признак "подтверждено" для события сигнализации
        /// </summary>
        /// <param name="id_comp">Часть составного ключа: идентификатор ГТП</param>
        /// <param name="id_tg">Часть составного ключа: идентификатор ТГ</param>
        /// <returns>Результат: признак установлен/не_установлен)</returns>
        public bool IsConfirmed(int id_comp, DateTime dtReg)
        {
            return m_dictAlarmObject.IsConfirmed(id_comp, dtReg);
        }
        /// <summary>
        /// Получить дату/время регистрации события сигнализации для ТГ
        /// </summary>
        /// <param name="id_comp">Составная часть ключа: идентификатор ГТП</param>
        /// <param name="id_tg">Составная часть ключа: идентификатор ГТП</param>
        /// <returns></returns>
        public DateTime TGAlarmDatetimeReg(int id_comp, DateTime dtReg)
        {
            return m_dictAlarmObject.TGAlarmDatetimeReg(id_comp, dtReg);
        }
        /// <summary>
        /// Получить результат запроса для события
        /// </summary>
        /// <param name="state">Идентификатор события</param>
        /// <param name="error">Признак получения результата запроса</param>
        /// <param name="table">Результат запроса - таблица ('DataTable')</param>
        /// <returns>Признак выполнения операции</returns>
        protected override int StateCheckResponse(int state, out bool error, out object table)
        {
            int iRes = 0;
            HandlerDb.INDEX_SYNC_STATECHECKRESPONSE indxSync = HandlerDb.INDEX_SYNC_STATECHECKRESPONSE.UNKNOWN;

            error = false;
            table = null;

            switch ((StatesMachine)state)
            {
                case StatesMachine.List:
                case StatesMachine.Notify:
                case StatesMachine.Insert:
                    indxSync = (HandlerDb.INDEX_SYNC_STATECHECKRESPONSE)WaitHandle.WaitAny(m_handlerDb.m_arSyncStateCheckResponse);
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
        /// <summary>
        /// Отправить запрос для события
        /// </summary>
        /// <param name="state">Идентификатор состояния</param>
        /// <returns>Результат отправления запроса</returns>
        protected override int StateRequest(int state)
        {
            int iRes = 0;
            ItemQueue itemQueue = Peek;

            switch ((StatesMachine)state)
            {
                case StatesMachine.List:
                case StatesMachine.Notify:
                    m_handlerDb.Refresh ((DateTime)itemQueue.Pars[0], (int)itemQueue.Pars[1], (int)itemQueue.Pars[2]);
                    break;
                case StatesMachine.Insert:
                    m_handlerDb.Insert(itemQueue.Pars[0] as TecViewAlarm.AlarmTecViewEventArgs);
                    break;
                default:
                    break;
            }

            //Logging.Logg().Debug(@"ViewAlarm::StateRequest () - state=" + ((StatesMachine)state).ToString() + @", result=" + bRes.ToString() + @" - вЫход...");

            return iRes;
        }
        /// <summary>
        /// Обработать результат запроса для состояния
        /// </summary>
        /// <param name="state">Идентификатор состояния</param>
        /// <param name="obj">Результат выполнения запроса</param>
        /// <returns>Признак обработки результата запроса</returns>
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
                case StatesMachine.Notify:
                    GetNotifyResponse(itemQueue, tableRes);
                    break;
                case StatesMachine.Detail:
                    GetEventDetailResponse(itemQueue, tableRes);
                    break;
                case StatesMachine.Insert:
                case StatesMachine.Fixed:
                case StatesMachine.Confirm:
                    //Результата нет (рез-т вставленные/обновленные записи)
                    break;
                default:
                    break;
            }

            //Вариант №1
            //??? прямой вызов метода-обработчика..., ??? использование объекта, полученного в качестве параметра... - очень некрасиво, и, возможно - неправильно
            // для отправителя/получателя результата панели
            //itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(new EventArgsDataHost(-1, new object[] { (StatesMachine)state, tableRes }));

            //Logging.Logg().Debug(@"ViewAlarm::StateResponse () - state=" + ((StatesMachine)state).ToString() + @", result=" + bRes.ToString() + @" - вЫход...");

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
