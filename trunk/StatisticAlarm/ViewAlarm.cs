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
        /// ����� ������ �������
        /// </summary>
        public MODE Mode;
        /// <summary>
        /// ����� ��� �������� �������� � ������� ������������
        /// </summary>
        public class AlarmDbEventArgs : AlarmNotifyEventArgs
        {
            public int m_id_user_registred
                , m_id_user_fixed
                , m_id_user_confirm;
            public DateTime? m_dtRegistred
                , m_dtFixed
                , m_dtConfirm;

            public AlarmDbEventArgs(DataRow rowEvt)
            //public EventRegEventArgs(int id_comp
            //                        , int idUsrReg, DateTime dtReg
            //                        , int idUsrFix, DateTime dtFix
            //                        , int idUsrConfirm, DateTime dtConfirm
            //                        , int s, string mes)
            {
                ////������� �1
                //m_id_comp = id_comp;
                //m_dtRegistred = dtReg;
                //m_situation = s;
                //m_message = mes;
                //������� �2
                int id_comp = (int)rowEvt[@"ID_COMPONENT"];
                if (id_comp > 100 && id_comp < 500)
                    m_id_gtp = id_comp;
                else
                    if (id_comp > 1000 && id_comp < 1000)
                        m_id_tg = id_comp;
                    else
                        ;
                //����������� �������
                m_id_user_registred = (int)rowEvt[@"ID_USER_REGISTRED"];
                m_dtRegistred = (DateTime)rowEvt[@"DATETIME_REGISTRED"];
                //�������� �������
                if (! (rowEvt[@"ID_USER_FIXED"] is System.DBNull))
                    m_id_user_fixed = (int)rowEvt[@"ID_USER_FIXED"];
                else
                    m_id_user_fixed = -1;
                if (!(rowEvt[@"DATETIME_FIXED"] is System.DBNull))
                    m_dtFixed = (DateTime)rowEvt[@"DATETIME_FIXED"];
                else
                    m_dtFixed = null;
                //������������� �������
                if (!(rowEvt[@"ID_USER_CONFIRM"] is System.DBNull))
                    m_id_user_confirm = (int)rowEvt[@"ID_USER_CONFIRM"];
                else
                    m_id_user_confirm = -1;
                if (!(rowEvt[@"DATETIME_CONFIRM"] is System.DBNull))
                    m_dtConfirm = (DateTime)rowEvt[@"DATETIME_CONFIRM"];
                else
                    m_dtConfirm = null;
                m_situation = AlarmNotifyEventArgs.GetSituation((string)rowEvt[@"MESSAGE"]);
                m_message = (string)rowEvt[@"MESSAGE"];
            }
        }
        /// <summary>
        /// ������������ - ������� ��������� ��� ��������� ���������
        /// </summary>
        private enum StatesMachine { Unknown = -1, List, Insert, Fixed, Confirm }
        /// <summary>
        /// ������ ��� ����� ������� ������������ � �� ���������
        /// </summary>
        private DictAlarmObject m_dictAlarmObject;
        /// <summary>
        /// ��� ������� - ��� ��������� ������� ����������� ������� (������) ������������ �� ��
        /// </summary>
        /// <param name="ev">�������� ...</param>
        public delegate void AlarmDbEventHandler(AlarmDbEventArgs ev);
        /// <summary>
        /// ������� ��� ���������� ������
        /// </summary>
        public event AlarmNotifyEventHandler EventAdd
            , EventRetry;
        /// <summary>
        /// ������� ����������� ������� (������) ������������ �� ��
        /// </summary>
        private event AlarmDbEventHandler EventReg;
        /// <summary>
        /// ������ ��� ������� ����������� ������� ������� ������������
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
        /// ����� ��� ��������� ������ �� ��
        /// </summary>
        private class HandlerDb : HClassLibrary.HHandlerDb
        {
            /// <summary>
            /// ������������
            /// </summary>
            public enum StatesMachine { CurrentTime, ListEvents, InsertEventMain, InsertEventDetail, UpdateEventFixed, UpdateEventConfirm }
            /// <summary>
            /// ��������� ���������� � ��_��������
            /// </summary>
            private ConnectionSettings m_connSett;
            /// <summary>
            /// ������������� ��������������, ��������� ���������� � ��_��������
            /// </summary>
            private int IdListener { get { return m_dictIdListeners[0][0]; } }
            
            public enum INDEX_SYNC_STATECHECKRESPONSE { UNKNOWN = -1, RESPONSE, ERROR, WARNING
                ,  COUNT_INDEX_SYNC_STATECHECKRESPONSE }
            public AutoResetEvent [] m_arSyncStateCheckResponse;

            DataTable m_tableResponse;
            /// <summary>
            /// ����/����� (����) ������� ������ �������
            /// </summary>
            private DateTime m_dtCurrent
                /// <summary>
                /// ����/����� �������
                /// </summary>
                , m_dtServer;
            /// <summary>
            /// ������ ���� ������ ������� ������� ������ ������� � ������
            /// </summary>
            private int m_iHourBegin
                /// <summary>
                /// ������ ���� ��������� ������� ������� ������ ������� � ������
                /// </summary>
                , m_iHourEnd;
            /// <summary>
            /// ������������� ������ � ������� ��_�������� � ������� ������������
            /// </summary>        
            private long m_idEventMain;
            /// <summary>
            /// ���������� � ������� ������������
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

                //??? ������ ������, ��� ����� �������� ������
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

                //Logging.Logg().Debug(@"ViewAlarm::StateRequest () - state=" + ((StatesMachine)state).ToString() + @", result=" + bRes.ToString() + @" - �����...");

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
                        // ������ �� ���������
                        break;
                    default:
                        break;
                }

                if (isLastState (state) == true)
                {
                    //��������� ������������� ���������� ���������� �������
                    if (! (obj == null))
                        //��������� ��������� � "��������" ����������
                        m_tableResponse = (obj as DataTable).Copy ();
                    else
                        ;
                    //�������, ��� ����� �����
                    m_arSyncStateCheckResponse[(int)INDEX_SYNC_STATECHECKRESPONSE.RESPONSE].Set();
                }
                else
                    ;

                //Logging.Logg().Debug(@"ViewAlarm::StateRequest () - state=" + ((StatesMachine)state).ToString() + @", result=" + bRes.ToString() + @" - �����...");

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
            {//??? - �������������� ������� - ������� �� 'HHandlerDb'
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
            /// ������������ ���������� ������� � �� � ��������� ��� ����������
            ///  , �� ����������� ��������, �������� �� 'Get', ��� ������������� �������� 'void'
            /// </summary>
            private void GetInsertEventMainRequest()
            {
                int id = m_EventRegEventArg.m_id_tg < 0 ? m_EventRegEventArg.m_id_gtp : m_EventRegEventArg.m_id_tg //ID_COMPONENT
                    , typeAlarm = (m_EventRegEventArg.m_id_tg < 0 ? 1 : 2) //TYPE
                    , id_user = 0; //ID_USER
                double val = m_EventRegEventArg.m_id_tg < 0 ? -1 : m_EventRegEventArg.m_listEventDetail[0].value; //VALUE
                string strDTRegistred = m_EventRegEventArg.m_dtRegistred.ToString(@"yyyyMMdd HH:mm:ss.fff"); //DATETIME_REGISTRED
                //������ ��� ������� ������ � ������� ������������
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
                //??? ������� �� ����� ������
                //query += "\r\n";
                query += Environment.NewLine;
                //������ �� ��������� �������������� ����������� ������
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
                //�� ��������� ������� �������
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
        /// ������ ��� ��������� ������ �� ��
        /// </summary>
        private AdminAlarm.HandlerDb m_handlerDb;
        /// <summary>
        /// ������� ��� �������� ������ �������/����������� ������������ ��������
        /// </summary>
        public event DelegateObjectFunc EvtGetDataMain
            , EvtGetDataDetail;
        /// <summary>
        /// ������ ������ ��� ��������� ���-�� �������
        ///  �� ��������� ������ ������� ������������
        /// </summary>
        private BackgroundWorker m_threadListEventsResponse;
        /// <summary>
        /// ����������� - �������� (� �����������)
        /// </summary>
        /// <param name="connSett">��������� ���������� � ��_��������</param>
        /// <param name="mode">����� ������ �������</param>
        /// <param name="ev">������������� �������� ����, ����� ������, ��������� ������� ������ �������</param>
        public AdminAlarm(ConnectionSettings connSett, MODE mode, DatetimeCurrentEventArgs ev, bool bWorkCheked)
            : base()
        {
            Mode = mode;
            m_dtCurrent = ev;

            lockValue = new object();

            m_dictAlarmObject = new DictAlarmObject();
            EventReg += new AlarmDbEventHandler(onEventReg);

            m_handlerDb = new AdminAlarm.HandlerDb(connSett);

            //���������������� ������ ��� ����������_������/����������_������
            m_timerView = new System.Threading.Timer(new TimerCallback(fTimerView_Tick), null, Timeout.Infinite, Timeout.Infinite);

            m_timerAlarm =
                new System.Threading.Timer(new TimerCallback(TimerAlarm_Tick), null, Timeout.Infinite, Timeout.Infinite)
                //new System.Windows.Forms.Timer ()
                ;
            //m_timerAlarm.Tick += new EventHandler(TimerAlarm_Tick);

            m_threadListEventsResponse = new BackgroundWorker ();
            m_threadListEventsResponse.DoWork += new DoWorkEventHandler(fThreadListEventsResponse_DoWork);
            m_threadListEventsResponse.RunWorkerCompleted += new RunWorkerCompletedEventHandler(fThreadListEventsResponse_RunWorkerCompleted);
        }

        public override void Start()
        {
            base.Start();

            m_handlerDb.Start ();

            if (Mode == MODE.SERVICE)
                foreach (TecViewAlarm tv in m_listTecView)
                    tv.Start(); //StartDbInterfaces (CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE);
            else ;
        }

        public override void Stop()
        {
            m_handlerDb.Stop ();

            foreach (TecViewAlarm tv in m_listTecView)
                tv.Stop ();

            if (! (m_timerAlarm == null))
            {
                m_timerAlarm.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                m_timerAlarm.Dispose ();
                m_timerAlarm = null;
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
                    if (Mode == MODE.SERVICE)
                    {
                        foreach (TecViewAlarm tv in m_listTecView)
                            tv.Activate(active);

                        m_timerAlarm.Change(0, System.Threading.Timeout.Infinite);
                    }
                    else
                        ;
                }
                else
                    if (Mode == MODE.SERVICE)
                        //������� �0
                        m_timerAlarm.Change(Timeout.Infinite, Timeout.Infinite);
                        ////������� �1
                        //m_timerAlarm.Stop ();
                    else
                        ;
            else
                ;

            return bRes;
        }
        /// <summary>
        /// ���������� ������� ��������� �������� "��������/���������"
        /// </summary>
        /// <param name="obj">������, ������������� �������</param>
        public void OnWorkCheckedChanged(bool bChecked)
        {
            //??? - ������������ ������ ������/������/���������� ������ �������
            Activate(bChecked);
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
        /// ��������� � ������� ��������� ������� - ������ ������� ������� ������������ �� ��
        /// </summary>
        private void pushListEvents()
        {
            //��������� � ������� ���������
            push(new object [] //�������� ������� ��� ���������
                    { new object [] //1-� �������
                        {
                            StatesMachine.List
                            , m_dtCurrent.Date
                            , m_dtCurrent.HourBegin
                            , m_dtCurrent.HourEnd
                        }
                    });
        }

        private void push(object []pars)
        {
            //��������� � ������� ���������
            // null - ����������/�����������
            Push(null, new object[] //???
                { pars });
        }
        /// <summary>
        /// ����� ��������� ������ ��� ������� ���������� �������� � �������
        /// </summary>
        /// <param name="obj">������, �������������� �������</param>
        /// <param name="ev">�������� �������</param>
        private void fTimerView_Tick(object obj)
        {
            pushListEvents ();

            m_timerView.Change(PanelStatistic.POOL_TIME, System.Threading.Timeout.Infinite);
        }
        /// <summary>
        /// ���������� ������� - ��������� ����, ������ ���� ������ � ���������
        /// </summary>
        /// <param name="ev"></param>
        public void OnEventDatetimeChanged(DatetimeCurrentEventArgs ev)
        {
            m_dtCurrent = ev;

            pushListEvents ();
        }
        /// <summary>
        /// ��������� ������� ��������� ���������� ������� ������ ������� ������������
        /// </summary>
        /// <param name="obj">������, �������������� ������� (??? �����)</param>
        /// <param name="ev">�������� ������� ������ ���������� ������</param>
        private void fThreadListEventsResponse_DoWork (object obj, DoWorkEventArgs ev)
        {
            DataTable tableRes = ev.Argument as DataTable;
            //DataRow []rowsUnFixed = tableRes.Select (@"[DATETIME_FIXED] IS NULL", @"[DATETIME_REGISTRED]");
            foreach (DataRow r in tableRes.Rows)
                EventReg(new AlarmDbEventArgs(r));

            ev.Result = @"Ok";
        }
        /// <summary>
        /// ���������� ������� - ���������� ���������� ��������� ������� �� ���������
        ///  ����������� ������� ������ ������� ������������
        /// </summary>
        /// <param name="obj">������, �������������� ������� (??? �����)</param>
        /// <param name="ev">�������� ������� ��������� ���������� ��������� �������</param>
        private void fThreadListEventsResponse_RunWorkerCompleted(object obj, RunWorkerCompletedEventArgs ev)
        {
            if (ev.Error == null)
                Console.WriteLine(@"ViewAlarm::fThreadListEventsResponse_RunWorkerCompleted () - " + ev.Result + @"...");
            else
                Logging.Logg().Exception(ev.Error, Logging.INDEX_MESSAGE.NOT_SET, @"ViewAlarm::fThreadListEventsResponse_DoWork () - ...");
        }
        /// <summary>
        /// ������� ��������� ����������� �������
        /// </summary>
        /// <param name="tableRes">������� - ��������� �������</param>
        private void GetListEventsResponse(ItemQueue itemQueue, DataTable tableRes)
        {
            Console.WriteLine(@"������� �� " + ((DateTime)itemQueue.Pars[0]).ToShortDateString()
                + @" (" + ((int)itemQueue.Pars[1]) + @"-" + ((int)itemQueue.Pars[2]) + @" �): "
                + tableRes.Rows.Count);

            m_threadListEventsResponse.RunWorkerAsync(tableRes);
        }
        /// <summary>
        /// ���������� ������� ����������� ������� �� ��
        /// </summary>
        /// <param name="ev">�������� ������� - �������� ������� ������������</param>
        private void onEventReg(AlarmDbEventArgs ev)
        {
            INDEX_ACTION iAction = m_dictAlarmObject.Registred (ev);
            if (iAction == INDEX_ACTION.ERROR)
                throw new Exception(@"ViewAlarm::onEventReg () - ...");
            else
                if (iAction == INDEX_ACTION.ADD)
                    EventAdd(ev);
                else
                    if (iAction == INDEX_ACTION.RETRY)
                        EventRetry(ev);
                    else
                        ;
        }
        /// <summary>
        /// ���������� ������� - ������������� ������� ������������ �� ������ (������������)
        /// </summary>
        /// <param name="id_comp">����� ���������� �����: ������������� ���</param>
        /// <param name="id_tg">����� ���������� �����: ������������� ��</param>
        public void OnEventConfirm (int id_comp, int id_tg)
        {
            Logging.Logg().Debug(@"ViewAlarm::OnEventConfirm () - id=" + id_comp.ToString() + @"; id_tg=" + id_tg.ToString(), Logging.INDEX_MESSAGE.NOT_SET);

            if (!(m_dictAlarmObject.Confirmed(id_comp, id_tg) < 0))
            {
                push(new object[] //�������� �������
                    {
                        new object [] //1-� �������
                        {
                            StatesMachine.Confirm
                            , id_comp
                            , id_tg
                        }
                    });

                if (!(id_tg < 0))
                {
                    tgConfirm(id_tg);
                }
                else
                    ;
            }
            else
                Logging.Logg().Error(@"ViewAlarm::OnEventConfirm () - id=" + id_comp.ToString() + @"; id_tg=" + id_tg.ToString() + @", �� ������!", Logging.INDEX_MESSAGE.NOT_SET);
        }
        /// <summary>
        /// ���������� ������� "������������" ��� ������� ������������
        /// </summary>
        /// <param name="id_comp">����� ���������� �����: ������������� ���</param>
        /// <param name="id_tg">����� ���������� �����: ������������� ��</param>
        /// <returns>���������: ������� ����������/��_����������)</returns>
        public bool IsConfirmed(int id_comp, int id_tg)
        {
            return m_dictAlarmObject.IsConfirmed(id_comp, id_tg);
        }
        /// <summary>
        /// �������� ����/����� ����������� ������� ������������ ��� ��
        /// </summary>
        /// <param name="id_comp">��������� ����� �����: ������������� ���</param>
        /// <param name="id_tg">��������� ����� �����: ������������� ���</param>
        /// <returns></returns>
        public DateTime TGAlarmDatetimeReg(int id_comp, int id_tg)
        {
            return m_dictAlarmObject.TGAlarmDatetimeReg(id_comp, id_tg);
        }
        /// <summary>
        /// �������� ��������� ������� ��� �������
        /// </summary>
        /// <param name="state">������������� �������</param>
        /// <param name="error">������� ��������� ���������� �������</param>
        /// <param name="table">��������� ������� - ������� ('DataTable')</param>
        /// <returns>������� ���������� ��������</returns>
        protected override int StateCheckResponse(int state, out bool error, out object table)
        {
            int iRes = 0;
            HandlerDb.INDEX_SYNC_STATECHECKRESPONSE indxSync = HandlerDb.INDEX_SYNC_STATECHECKRESPONSE.UNKNOWN;

            error = false;
            table = null;

            switch ((StatesMachine)state)
            {
                case StatesMachine.List:
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
        /// ��������� ������ ��� �������
        /// </summary>
        /// <param name="state">������������� ���������</param>
        /// <returns>��������� ����������� �������</returns>
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
                    m_handlerDb.Insert(itemQueue.Pars[0] as TecViewAlarm.AlarmTecViewEventArgs);
                    break;
                default:
                    break;
            }

            //Logging.Logg().Debug(@"ViewAlarm::StateRequest () - state=" + ((StatesMachine)state).ToString() + @", result=" + bRes.ToString() + @" - �����...");

            return iRes;
        }
        /// <summary>
        /// ���������� ��������� ������� ��� ���������
        /// </summary>
        /// <param name="state">������������� ���������</param>
        /// <param name="obj">��������� ���������� �������</param>
        /// <returns>������� ��������� ���������� �������</returns>
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
                case StatesMachine.Fixed:
                case StatesMachine.Confirm:
                    //���������� ��� (���-� �����������/����������� ������)
                    break;
                default:
                    break;
            }

            //������� �1
            //??? ������ ����� ������-�����������..., ??? ������������� �������, ����������� � �������� ���������... - ����� ���������, �, �������� - �����������
            // ��� �����������/���������� ���������� ������
            //itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(new EventArgsDataHost(-1, new object[] { (StatesMachine)state, tableRes }));

            //Logging.Logg().Debug(@"ViewAlarm::StateRequest () - state=" + ((StatesMachine)state).ToString() + @", result=" + bRes.ToString() + @" - �����...");

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
