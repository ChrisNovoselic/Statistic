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
            public DateTime? 
                m_dtFixed
                , m_dtConfirm;

            public AlarmDbEventArgs(DataRow rowEvt)
                : base((int)rowEvt[@"ID_COMPONENT"]
                    , (DateTime)rowEvt[@"DATETIME_REGISTRED"]
                    , (int)rowEvt[@"SITUATION"])
            {
                //����������� �������
                //m_id_comp, [ID_COMP]
                m_id_user_registred = (int)rowEvt[@"ID_USER_REGISTRED"];
                //m_dtRegistred, [DATETIME_REGISTRED]
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
                //m_situation, [SUTUATION]
                //m_message_shr, [SUTUATION]
            }
        }
        /// <summary>
        /// ������������ - ������� ��������� ��� ��������� ���������
        /// </summary>
        public enum StatesMachine { Unknown = -1, List, Notify, Detail, Insert, Fixed, Confirm }
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
        /// ������� ������������ ������������������ ������� ������������ �� ��
        /// </summary>
        bool m_bAlarmDbEventUpdated;
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
            /// <summary>
            /// ������������ - ������� �������� ������������� ��� ������ ������� (�����������) ��������� �������
            /// </summary>
            public enum INDEX_SYNC_STATECHECKRESPONSE { UNKNOWN = -1, RESPONSE, ERROR, WARNING
                ,  COUNT_INDEX_SYNC_STATECHECKRESPONSE }
            /// <summary>
            /// ������� ������������� ��� ������ ������� (�����������) ��������� �������
            /// </summary>
            public AutoResetEvent [] m_arSyncStateCheckResponse;
            /// <summary>
            /// ������� ��� ������ ������� (�����������) ��������� �������
            /// </summary>
            private DataTable m_tableResponse;
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
            private object m_objIntermediateValue;
            /// <summary>
            /// ���������� � ������� ������������
            /// </summary>
            private object m_objArgument;

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
                    case StatesMachine.UpdateEventFixed:
                    case StatesMachine.UpdateEventConfirm:
                        iRes = response(out error, out table);
                        break;
                    case StatesMachine.InsertEventDetail:                    
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
                m_objIntermediateValue = null;
                m_objArgument = null;
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
                TecViewAlarm.AlarmTecViewEventArgs arg = m_objArgument as TecViewAlarm.AlarmTecViewEventArgs;
                int id_user = 0; //ID_USER
                double val = (arg.Mode == FormChangeMode.MODE_TECCOMPONENT.GTP) ? -1F :
                    (arg.Mode == FormChangeMode.MODE_TECCOMPONENT.TG) ? arg.m_listEventDetail[0].value :
                        -2F; //VALUE
                string strDTRegistred = arg.m_dtRegistred.GetValueOrDefault().ToString(@"yyyyMMdd HH:mm:ss.fff"); //DATETIME_REGISTRED
                //������ ��� ������� ������ � ������� ������������
                string query = "INSERT INTO [dbo].[AlarmEvent] ([ID_COMPONENT],[TYPE],[VALUE],[DATETIME_REGISTRED],[ID_USER_REGISTRED],[DATETIME_FIXED],[ID_USER_FIXED],[DATETIME_CONFIRM],[ID_USER_CONFIRM],[CNT_RETRY],[INSERT_DATETIME],[SITUATION]) VALUES"
                    + @" ("
                        + arg.m_id_comp + @", " //ID_COMPONENT
                        + (int)arg.type + @", " //TYPE                        
                        + val.ToString(@"F3", CultureInfo.InvariantCulture) + @", " //VALUE
                        + @"'" + strDTRegistred + @"', " //DATETIME_REGISTRED
                        + id_user + @", " //ID_USER_REGISTRED
                        + "NULL" + @", " //DATETIME_FIXED
                        + "NULL" + @", " //ID_USER_FIXED
                        + "NULL" + @", " //DATETIME_CONFIRM
                        + "NULL" + @", " //ID_USER_CONFIRM
                        + 0 + @", " //CNT_RETRY
                        + "GETDATE ()" + @", " //INSERT_DATETIME
                        + @"'" + arg.m_situation + @"'" //SITUATION
                    + @")";
                query += @";";
                //??? ������� �� ����� ������
                //query += "\r\n";
                query += Environment.NewLine;
                //������ �� ��������� �������������� ����������� ������
                query += @"SELECT * FROM [dbo].[AlarmEvent] WHERE "
                    + @"[ID_COMPONENT]=" + arg.m_id_comp
                    + @" AND [TYPE]=" + (int)arg.type                    
                    + @" AND [DATETIME_REGISTRED]='" + strDTRegistred + @"'"
                    + @" AND [ID_USER_REGISTRED]=" + id_user;
                query += @";";

                Request(IdListener, query);
            }

            private void GetInsertEventMainResponse(object obj)
            {
                m_objIntermediateValue = (long)(obj as DataTable).Rows[0][@"ID"];
            }

            private void GetInsertEventDetailRequest()
            {
                TecViewAlarm.AlarmTecViewEventArgs arg = m_objArgument as TecViewAlarm.AlarmTecViewEventArgs;
                long id = (long)m_objIntermediateValue;
                string query = @"INSERT INTO [dbo].[AlarmDetail] VALUES ";

                foreach (TecViewAlarm.AlarmTecViewEventArgs.EventDetail detail in arg.m_listEventDetail)
                {
                    if (detail.value > 0)
                    {
                        if (!(id < 0))
                        {
                            query += @"(";

                            query += id + @", ";
                            query += detail.id + @", ";
                            query += detail.value.ToString(@"F3", CultureInfo.InvariantCulture) + @", ";
                            query += @"'" + detail.last_changed_at.ToString(@"yyyyMMdd HH:mm:ss.fff") + @"', ";
                            query += detail.id_tm + @", ";
                            query += @"GETDATE()";

                            query += @"),";
                        }
                        else
                            throw new Exception(@"ViewAlarm.HandlerDb::GetInsertEventDetailRequest () - idEventMain=" + id);
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
                AlarmDbEventArgs arg = m_objArgument as AlarmDbEventArgs;
                string query = string.Empty
                    , where = @"WHERE [ID_COMPONENT]=" + arg.m_id_comp
                        + @" AND [DATETIME_REGISTRED]='" + arg.m_dtRegistred.GetValueOrDefault().ToString (@"yyyyMMdd HH:mm:ss.fff") + @"'"
                        + @" AND [TYPE]=" + (int)arg.type;

                query = @"UPDATE [dbo].[AlarmEvent] SET [ID_USER_FIXED]=" + HUsers.Id + @", [DATETIME_FIXED]=GETUTCDATE() "
                    + where;
                query += @"; ";
                query += Environment.NewLine;
                query += @"SELECT * FROM [dbo].[AlarmEvent] "
                    + where;

                Request(IdListener, query);
            }

            private void GetUpdateEventConfirmRequest()
            {
                long id = (long)m_objArgument;
                string query = string.Empty
                    , where = @"WHERE [ID]=" + id;

                query = @"UPDATE [dbo].[AlarmEvent] SET [ID_USER_CONFIRM]=" + HUsers.Id + @", [DATETIME_CONFIRM]=GETUTCDATE() "
                    + where;
                query += @"; ";
                query += Environment.NewLine;
                query += @"SELECT * FROM [dbo].[AlarmEvent] "
                    + where;

                Request(IdListener, query);
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

                    m_objArgument = ev;

                    states.Add((int)StatesMachine.CurrentTime);
                    states.Add((int)StatesMachine.InsertEventMain);
                    states.Add((int)StatesMachine.InsertEventDetail);

                    Run(@"ViewAlarm::Insert");
                }
            }

            public void Fixed(AlarmNotifyEventArgs ev)
            {
                lock (this)
                {
                    ClearValues();
                    ClearStates();

                    m_objArgument = ev;

                    states.Add((int)StatesMachine.CurrentTime);
                    states.Add((int)StatesMachine.UpdateEventFixed);

                    Run(@"ViewAlarm::Fixed");
                }
            }

            public void Confirm(object id)
            {
                lock (this)
                {
                    ClearValues();
                    ClearStates();

                    m_objArgument = id;

                    states.Add((int)StatesMachine.CurrentTime);
                    states.Add((int)StatesMachine.UpdateEventConfirm);

                    Run(@"ViewAlarm::Fixed");
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
        private BackgroundWorker m_threadNotifyResponse;
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

            m_bAlarmDbEventUpdated = false;

            m_handlerDb = new AdminAlarm.HandlerDb(connSett);

            //���������������� ������ ��� ����������_������/����������_������
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
                // ������ ��� ��� ���������� � 'activateAdminAlarm'
                m_timerAlarm.Dispose ();
                m_timerAlarm = null;
            }
            else
                ;

            activateViewAlarm(false);
            if (! (m_timerView == null))
            {
                // ������ ��� ��� ���������� � 'activateViewAlarm'
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
        /// ���������� ������� ��������� �������� "��������/���������"
        /// </summary>
        /// <param name="obj">������, ������������� �������</param>
        public void OnWorkCheckedChanged(bool bChecked)
        {
            //??? - ������������ ������ ������/������/���������� ������ �������
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
        /// ��������� � ������� ��������� ������� - ������ ������� ������� ������������ �� ��
        /// </summary>
        private void pushList()
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

        /// <summary>
        /// ��������� � ������� ��������� ������� - ������ ������� ������� ������������ �� �� - ������!!!
        /// </summary>
        private void pushNotify()
        {
            //DateTime dtNotify = HAdmin.ToMoscowTimeZone();
            //��������� � ������� ���������
            push(new object[] //�������� ������� ��� ���������
                    { new object [] //1-� �������
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
            // ��������� � ������� ������� ��������� ������ ������� ������������
            //  �� ��������� ����/���� ������, ���� ���� �������
            if (isToday == true)                
                pushList();
            else
                ;
            // ��������� � ������� ������� ��������� ������ ������� ������� ������������
            pushNotify();

            m_timerView.Change(PanelStatistic.POOL_TIME * 1000, System.Threading.Timeout.Infinite);
        }
        /// <summary>
        /// ���������� ������� - ��������� ����, ������ ���� ������ � ���������
        /// </summary>
        /// <param name="ev"></param>
        public void OnEventDatetimeChanged(DatetimeCurrentEventArgs ev)
        {
            m_dtCurrent = ev;

            pushList ();
            ////��������� ��������� ������������� ���� � �������
            //// � ������ ������ - ���������� ����� ��������� ������� �������
            //if (isToday == true)
            //    m_timerView.Change(PanelStatistic.POOL_TIME, System.Threading.Timeout.Infinite);
            //else
            //    ;
        }
        /// <summary>
        /// ������� ���������� � ������� ��������� ������� - ������� �� ��������� ������ ������ ������������
        ///  �� �������� ������������� ����
        /// </summary>
        private bool isToday { get { return m_dtCurrent.Date.Equals(HAdmin.ToMoscowTimeZone ().Date); } }
        /// <summary>
        /// ��������� ������� ��������� ���������� ������� ������ ������� ������������
        /// </summary>
        /// <param name="obj">������, �������������� ������� (??? �����)</param>
        /// <param name="ev">�������� ������� ������ ���������� ������</param>
        private void fThreadNotifyResponse_DoWork (object obj, DoWorkEventArgs ev)
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
        /// ������� ��������� ����������� ������� �� ��������� ����/�����
        /// </summary>
        /// <param name="tableRes">������� - ��������� �������</param>
        private void GetListEventsResponse(ItemQueue itemQueue, DataTable tableRes)
        {
            Console.WriteLine(@"������� �� " + ((DateTime)itemQueue.Pars[0]).ToShortDateString()
                + @" (" + ((int)itemQueue.Pars[1]) + @"-" + ((int)itemQueue.Pars[2]) + @" �): "
                + tableRes.Rows.Count);

            List<PanelAlarm.ViewAlarmJournal> listRes = new List<PanelAlarm.ViewAlarmJournal>(tableRes.Rows.Count);
            
            foreach (DataRow r in tableRes.Rows)
            {
                listRes.Add (new PanelAlarm.ViewAlarmJournal ()
                {
                    m_id = (long)r[@"ID"]
                    , m_id_component = (int)r[@"ID_COMPONENT"]
                    , m_str_name_shr_component = getNameComponent((int)r[@"ID_COMPONENT"])
                    , m_type = (INDEX_TYPE_ALARM)r[@"TYPE"]
                    , m_str_name_shr_type = (INDEX_TYPE_ALARM)r[@"TYPE"] == INDEX_TYPE_ALARM.CUR_POWER ? @"���.����." :
                        (INDEX_TYPE_ALARM)r[@"TYPE"] == INDEX_TYPE_ALARM.TGTurnOnOff ? @"�����/����" : string.Empty
                    , m_value = (float)r[@"VALUE"]                    
                    , m_id_user_registred = (int)r[@"ID_USER_REGISTRED"]
                    , m_dt_registred = (DateTime)r[@"DATETIME_REGISTRED"]
                    , m_id_user_fixed = (int)r[@"ID_USER_FIXED"]
                    , m_dt_fixed = (DateTime?)r[@"DATETIME_FIXED"]
                    , m_id_user_confirmed = (int)r[@"ID_USER_CONFIRM"]
                    , m_dt_confirmed = (DateTime?)r[@"DATETIME_CONFIRM"]
                });
            }

            if (Actived == true)
                //EvtGetDataMain(tableRes);
                EvtGetDataMain(listRes);
            else
                ;
        }
        /// <summary>
        /// ������� ��������� ����������� ������� �������!!! ������� ������������
        /// </summary>
        /// <param name="tableRes">������� - ��������� �������</param>
        private void GetNotifyResponse(ItemQueue itemQueue, DataTable tableRes)
        {
            m_threadNotifyResponse.RunWorkerAsync(tableRes);
        }

        private void GetEventDetailResponse(ItemQueue itemQueue, DataTable tableRes)
        {
            EvtGetDataDetail(tableRes);
        }

        private void GetUpdateFixedResponse(ItemQueue itemQueue, DataTable tableRes)
        {
            DataRow r = tableRes.Rows [0];
            m_dictAlarmObject.Fixed ((int)r[@"ID_COMPONENT"], (DateTime)r[@"DATETIME_REGISTRED"], (DateTime)r[@"DATETIME_FIXED"]);
            itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(new EventArgsDataHost(-1, new object[] { StatesMachine.Fixed, (long)r[@"ID"], (DateTime)r[@"DATETIME_FIXED"] }));            
        }

        private void GetUpdateConfirmResponse(ItemQueue itemQueue, DataTable tableRes)
        {
            DataRow r = tableRes.Rows[0];
            m_dictAlarmObject.Confirmed((int)r[@"ID_COMPONENT"], (DateTime)r[@"DATETIME_REGISTRED"], (DateTime)r[@"DATETIME_CONFIRM"]);
            itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(new EventArgsDataHost(-1, new object[] { StatesMachine.Fixed, (long)r[@"ID"], (DateTime)r[@"DATETIME_CONFIRM"] }));
        }

        private string getEventGUIMessage(AlarmNotifyEventArgs ev)
        {
            string strRes = getNameComponent (ev.m_id_comp);

            if (strRes.Equals (string.Empty) == false)
            {
                strRes += Environment.NewLine;
                strRes += ev.m_dtRegistred.GetValueOrDefault().ToString();
                strRes += Environment.NewLine;
                strRes += ev.m_message_shr;
            }
            else
                ;

            return strRes;
        }

        private string getNameComponent(int id_comp)
        {
            string strRes = string.Empty;

            foreach (TecView tv in m_listTecView)
                foreach (TECComponent tc in tv.m_tec.list_TECComponents)
                    if (tc.m_id == id_comp)
                    {
                        strRes += tv.m_tec.name_shr;
                        strRes += @", ";
                        strRes += tc.name_shr;
                        
                        break;
                    }
                    else
                        ;

            return strRes;
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
                //��������� ����� ������ - ���������� ������ ��� 'ADMIN'
                if ((Mode == MODE.ADMIN)
                    //|| (Mode == MODE.SERVICE) //??? ��� �������
                    )
                {
                    ev.m_messageGUI = getEventGUIMessage (ev);
                    if (iAction == INDEX_ACTION.NEW)
                        EventAdd(ev);
                    else
                        if (iAction == INDEX_ACTION.RETRY)
                            EventRetry(ev);
                        else
                            ;
                }
                else
                    ;
        }
        ///// <summary>
        ///// ���������� ������� - ������������� ������� ������������ �� ������ (������������)
        ///// </summary>
        ///// <param name="id_comp">����� ���������� �����: ������������� ���</param>
        ///// <param name="id_tg">����� ���������� �����: ������������� ��</param>
        //public void OnEventConfirm (AlarmNotifyEventArgs ev)
        //{
        //    Logging.Logg().Debug(@"ViewAlarm::OnEventConfirm (id=" + ev.m_id_comp.ToString() + @") - ...", Logging.INDEX_MESSAGE.NOT_SET);

        //    if (!(m_dictAlarmObject.Confirmed(ev.m_id_comp, ev.m_dtRegistred.GetValueOrDefault()) < 0))
        //    {
        //        push(new object[] //�������� �������
        //            {
        //                new object [] //1-� �������
        //                {
        //                    StatesMachine.Confirm
        //                    , ev.m_id_comp
        //                    , ev.m_dtRegistred
        //                }
        //            });

        //        if (StatisticCommon.TECComponent.Mode(ev.m_id_comp) == FormChangeMode.MODE_TECCOMPONENT.TG)
        //            tgConfirm(ev.m_id_comp);
        //        else
        //            ;
        //    }
        //    else
        //        Logging.Logg().Error(@"ViewAlarm::OnEventConfirm (id=" + ev.m_id_comp.ToString() + @") - �� ������!", Logging.INDEX_MESSAGE.NOT_SET);
        //}
        /// <summary>
        /// ���������� ������� "������������" ��� ������� ������������
        /// </summary>
        /// <param name="id_comp">����� ���������� �����: ������������� ���</param>
        /// <param name="id_tg">����� ���������� �����: ������������� ��</param>
        /// <returns>���������: ������� ����������/��_����������)</returns>
        public bool IsConfirmed(int id_comp, DateTime dtReg)
        {
            return m_dictAlarmObject.IsConfirmed(id_comp, dtReg);
        }
        /// <summary>
        /// �������� ����/����� ����������� ������� ������������ ��� ��
        /// </summary>
        /// <param name="id_comp">��������� ����� �����: ������������� ���</param>
        /// <param name="id_tg">��������� ����� �����: ������������� ���</param>
        /// <returns></returns>
        public DateTime TGAlarmDatetimeReg(int id_comp, DateTime dtReg)
        {
            return m_dictAlarmObject.TGAlarmDatetimeReg(id_comp, dtReg);
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
                case StatesMachine.Notify:
                case StatesMachine.Insert:
                case StatesMachine.Fixed:
                case StatesMachine.Confirm:
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
                case StatesMachine.Notify:
                    m_handlerDb.Refresh ((DateTime)itemQueue.Pars[0], (int)itemQueue.Pars[1], (int)itemQueue.Pars[2]);
                    break;
                case StatesMachine.Insert:
                    m_handlerDb.Insert(itemQueue.Pars[0] as TecViewAlarm.AlarmTecViewEventArgs);
                    break;
                case StatesMachine.Fixed:
                    m_handlerDb.Fixed(itemQueue.Pars[0] as AlarmNotifyEventArgs);
                    break;
                case StatesMachine.Confirm:
                    m_handlerDb.Confirm((long)itemQueue.Pars[0]);
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
                case StatesMachine.Notify:
                    GetNotifyResponse(itemQueue, tableRes);
                    break;
                case StatesMachine.Detail:
                    GetEventDetailResponse(itemQueue, tableRes);
                    break;
                case StatesMachine.Fixed:
                    GetUpdateFixedResponse(itemQueue, tableRes);
                    break;
                case StatesMachine.Confirm:
                    GetUpdateConfirmResponse(itemQueue, tableRes);
                    break;
                case StatesMachine.Insert:                
                    //���������� ��� (���-� �����������/����������� ������)
                    break;
                default:
                    break;
            }

            //������� �1
            //??? ������ ����� ������-�����������..., ??? ������������� �������, ����������� � �������� ���������... - ����� ���������, �, �������� - �����������
            // ��� �����������/���������� ���������� ������
            //itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(new EventArgsDataHost(-1, new object[] { (StatesMachine)state, tableRes }));

            //Logging.Logg().Debug(@"ViewAlarm::StateResponse () - state=" + ((StatesMachine)state).ToString() + @", result=" + bRes.ToString() + @" - �����...");

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
        /// <summary>
        /// ���������� �������� �� ��������� ������ �� 'PanelAlarm'
        ///  � ������� ���������� ��������� 'IDataHost'
        /// </summary>
        /// <param name="obj"></param>
        public void OnEvtDataAskedHost (object obj)
        {
            Push ((obj as EventArgsDataHost).reciever, new object [] { (obj as EventArgsDataHost).par });
        }
    }
}
