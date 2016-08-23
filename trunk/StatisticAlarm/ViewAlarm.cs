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
        /// ������ ��� ����������/���������� ����/������� � ��
        /// </summary>
        private static string s_strDateTimeFormat = @"yyyyMMdd HH:mm:ss.fffffff";
        /// <summary>
        /// ����� ��� �������� �������� � ������� ������������
        /// </summary>
        public class AlarmDbEventArgs : AlarmNotifyEventArgs
        {
            public long m_id;
            public int m_id_user_registred
                , m_id_user_fixed
                , m_id_user_confirm;
            public DateTime? 
                m_dtFixed
                , m_dtConfirm;

            public AlarmDbEventArgs(DataRow rowEvt)
                : base((int)rowEvt[@"ID_COMPONENT"]
                    , new AlarmNotifyEventArgs.EventReason () { value = (float)rowEvt[@"value"], UDGe = float.NaN, koeff = decimal.MinusOne }
                    , (DateTime)rowEvt[@"DATETIME_REGISTRED"]
                    , (int)rowEvt[@"SITUATION"])
            {
                //����������� �������
                m_id = (long)rowEvt[@"ID"];
                m_id_user_registred = (int)rowEvt[@"ID_USER_REGISTRED"];
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
        public enum StatesMachine { Unknown = -1, List, Notify, Detail, Insert, Retry, Fixed, Confirm }
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
        private ManualResetEvent m_mEvtAlarmDbEventUpdated;
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
        /// ������� ���./����. �������
        /// </summary>
        private bool m_bWorkChecked;
        /// <summary>
        /// ����� ��� ��������� ������ �� ��
        /// </summary>
        private class HandlerDb : HClassLibrary.HHandlerDb
        {
            /// <summary>
            /// ������������
            /// </summary>
            public enum StatesMachine { CurrentTime, ListEvents, EventDetail, InsertEventMain, InsertEventDetail, RetryEvent, UpdateEventFixed, UpdateEventConfirm }
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
                Console.WriteLine(@"AdminAlarm.HHandlerDb::StartDbInterfaces (active=true) - iListenerId=" + m_dictIdListeners[0][0]);
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
                    case StatesMachine.EventDetail:
                    case StatesMachine.InsertEventMain:
                    case StatesMachine.UpdateEventFixed:
                    case StatesMachine.UpdateEventConfirm:
                    case StatesMachine.RetryEvent:
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
                    case StatesMachine.RetryEvent:
                        GetRetryEventRequest();
                        break;
                    case StatesMachine.UpdateEventFixed:
                        GetUpdateEventFixedRequest();
                        break;
                    case StatesMachine.UpdateEventConfirm:
                        GetUpdateEventConfirmRequest();
                        break;
                    case StatesMachine.EventDetail:
                        GetEventDetailRequest();
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
                    case StatesMachine.InsertEventMain:
                        GetInsertEventMainResponse(obj as DataTable);
                        break;
                    case StatesMachine.ListEvents:
                    case StatesMachine.EventDetail:
                    case StatesMachine.InsertEventDetail:
                    case StatesMachine.UpdateEventFixed:
                    case StatesMachine.UpdateEventConfirm:
                    case StatesMachine.RetryEvent:                    
                        // ��������� ������ �� ���������
                        break;
                    default:
                        break;
                }

                //��������� ������� �� �������������� ���������
                if (isLastState (state) == true)
                {
                    //��������� ����������� ���������� ���������� �������
                    if (! (obj == null))
                        //��������� ��������� � "��������" ����������
                        m_tableResponse = (obj as DataTable).Copy();
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
            /// <summary>
            /// ��������� ������ ����/������� �� �������
            /// </summary>
            protected void GetCurrentTimeRequest()
            {
                GetCurrentTimeRequest(DbTSQLInterface.getTypeDB(m_connSett.port), IdListener);
            }
            /// <summary>
            /// ���������� ���-� ��������� ����/������� �� �������
            /// </summary>
            /// <param name="tableRes"></param>
            private void GetCurrentTimeResponse(DataTable tableRes)
            {
                if (tableRes.Rows.Count == 1)
                    m_dtServer = (DateTime)tableRes.Rows[0][0];
                else
                    Logging.Logg().Error(@"AdminAlarm.HandlerDb::GetCurrentTimeResponse () - ������� �� �������� �� ����� ������ ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
            /// <summary>
            /// ��������� ������ ��� ��������� ������� ������� ������������
            ///  �� ��������� ������ ������� (����/���� ������/���������)
            /// </summary>
            private void GetListEventsRequest()
            {
                Request(IdListener
                    , @"SELECT * FROM [dbo].[AlarmEvent]"
                        + @" WHERE [DATETIME_REGISTRED] BETWEEN '"
                            + (m_dtCurrent.AddHours(m_iHourBegin) - HDateTime.TS_NSK_OFFSET_OF_MOSCOWTIMEZONE).ToString(@"yyyyMMdd HH:mm:ss") + @"' AND '"
                            + (m_dtCurrent.AddHours(m_iHourEnd) - HDateTime.TS_NSK_OFFSET_OF_MOSCOWTIMEZONE).ToString(@"yyyyMMdd HH:mm:ss") + @"'"
                        + @" ORDER BY [DATETIME_REGISTRED]");
            }

            private void GetEventDetailRequest()
            {
                long id = (long)m_objArgument;

                Request(IdListener
                    , @"SELECT * FROM [dbo].[AlarmDetail] WHERE [ID_EVENT]=" + id);
            }
            /// <summary>
            /// ������������ ���������� ������� � �� � ��������� ��� ����������
            ///  , �� ����������� ��������, �������� �� 'Get', ��� ������������� �������� 'void'
            /// </summary>
            private void GetInsertEventMainRequest()
            {
                TecViewAlarm.AlarmTecViewEventArgs arg = m_objArgument as TecViewAlarm.AlarmTecViewEventArgs;
                int id_user = HUsers.Id; //ID_USER
                double val =
                    //(arg.Mode == FormChangeMode.MODE_TECCOMPONENT.GTP) ? -1F :
                    //    (arg.Mode == FormChangeMode.MODE_TECCOMPONENT.TG) ? arg.m_listEventDetail[0].value :
                    //        -2F
                    arg.m_reason.value
                            ; //VALUE
                string strDTRegistred = arg.m_dtRegistred.GetValueOrDefault().ToString(s_strDateTimeFormat); //DATETIME_REGISTRED
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
            /// <summary>
            /// ���������� (�������������) ���-� ������� ������� ������ - �������� ���������� � ������� ������������
            ///  , �������� ������������� ����������� ������
            /// </summary>
            /// <param name="obj"></param>
            private void GetInsertEventMainResponse(object obj)
            {
                // ��������� ������� � ������������� ����������
                // ��� ������������� � ���������� �������
                // ��� ��������� ���������� ��������� ��� ������� ������ ���������
                m_objIntermediateValue = (long)(obj as DataTable).Rows[0][@"ID"];
            }
            /// <summary>
            /// ��������� ������ �� ������� �������������� ���������� ��� ������� ������������
            /// </summary>
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
                            query += @"'" + detail.last_changed_at.ToString(s_strDateTimeFormat) + @"', ";
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

            private void GetRetryEventRequest()
            {
                AlarmNotifyEventArgs arg = m_objArgument as AlarmNotifyEventArgs;
                string query = string.Empty
                    , where = @"WHERE [ID_COMPONENT]=" + arg.m_id_comp
                        + @" AND [DATETIME_REGISTRED]='" + arg.m_dtRegistred.GetValueOrDefault().ToString(s_strDateTimeFormat) + @"'"
                        + @" AND [TYPE]=" + (int)arg.type;

                query = @"UPDATE [dbo].[AlarmEvent] SET [ID_USER_FIXED]=NULL, [DATETIME_FIXED]=NULL "
                    + where;
                query += @"; ";
                query += Environment.NewLine;
                query += @"SELECT * FROM [dbo].[AlarmEvent] "
                    + where;

                Request(IdListener, query);
            }
            /// <summary>
            /// ��������� ������ �� ���������� ���������� � ������� ������������
            ///  ���������� ����/������� �������� �������
            /// </summary>
            private void GetUpdateEventFixedRequest()
            {
                AlarmNotifyEventArgs arg = m_objArgument as AlarmNotifyEventArgs;
                string query = string.Empty
                    , where = @"WHERE [ID_COMPONENT]=" + arg.m_id_comp
                        + @" AND [DATETIME_REGISTRED]='" + arg.m_dtRegistred.GetValueOrDefault().ToString (s_strDateTimeFormat) + @"'"
                        + @" AND [TYPE]=" + (int)arg.type;

                query = @"UPDATE [dbo].[AlarmEvent] SET [ID_USER_FIXED]=" + HUsers.Id + @", [DATETIME_FIXED]=GETUTCDATE() "
                    + where;
                query += @"; ";
                query += Environment.NewLine;
                query += @"SELECT * FROM [dbo].[AlarmEvent] "
                    + where;

                Request(IdListener, query);
            }
            /// <summary>
            /// ��������� ������ �� ���������� ���������� � ������� ������������
            ///  ���������� ����/������� ������������� �������
            /// </summary>
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
            /// <summary>
            /// ��������� ������ (������ ��������) ��� ��������� ����������� ������ ������� ������������
            ///  �� ��������� ���� � ���� (������/���������)
            /// </summary>
            /// <param name="dtCurrent">���� ��� �������</param>
            /// <param name="iHourBegin">����� ���� ������</param>
            /// <param name="iHourEnd">����� ���� ���������</param>
            public void Refresh(DateTime dtCurrent, int iHourBegin, int iHourEnd)
            {
                m_dtCurrent = dtCurrent;
                m_iHourBegin = iHourBegin;
                m_iHourEnd = iHourEnd;

                refresh ();
            }
            /// <summary>
            /// ������������ ������ ��������� ��� ������������ ������ ������� ������������
            /// </summary>
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
            /// <summary>
            /// ������������ ������ ��������� ��� ��������� �������������� ���������� �
            ///  ������� ������������
            /// </summary>
            /// <param name="id">������������� ������� ������������</param>
            public void Detail(object id)
            {
                lock (this)
                {
                    ClearValues();
                    ClearStates();

                    m_objArgument = id;

                    states.Add((int)StatesMachine.CurrentTime);
                    states.Add((int)StatesMachine.EventDetail);

                    Run(@"ViewAlarm::Detail");
                }
            }
            /// <summary>
            /// ������������ ������ ��������� ��� ������� �������� ���������� �
            ///  ������� ������������
            /// </summary>
            /// <param name="ev">���������� � ������� ������������</param>
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

            public void Retry(TecViewAlarm.AlarmTecViewEventArgs ev)
            {
                lock (this)
                {
                    ClearValues();
                    ClearStates();

                    m_objArgument = ev;

                    states.Add((int)StatesMachine.CurrentTime);
                    states.Add((int)StatesMachine.RetryEvent);

                    Run(@"ViewAlarm::Retry");
                }
            }
            /// <summary>
            /// ������������ ������ ��������� ��� ���������� �������� ���������� �
            ///  ������� ���������� (��������)
            /// </summary>
            /// <param name="ev">���������� � ������� ���������</param>
            public void Fixed(AlarmNotifyEventArgs ev)
            {
                lock (this)
                {
                    ClearValues();
                    ClearStates();

                    m_objArgument = ev;

                    states.Add((int)StatesMachine.CurrentTime);
                    states.Add((int)StatesMachine.UpdateEventFixed);

                    Run(@"AdminAlarm::Fixed");
                }
            }
            /// <summary>
            /// ������������ ������ ��������� ��� ���������� �������� ���������� �
            ///  ������� ������������ (�������������)
            /// </summary>
            /// <param name="id">������������� ������� ������������</param>
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
        public AdminAlarm(ConnectionSettings connSett, MODE mode, DatetimeCurrentEventArgs ev, bool bWorkChecked)
            : base()
        {
            Mode = mode;
            m_dtCurrent = ev;
            m_bWorkChecked = bWorkChecked;

            lockValue = new object();

            m_dictAlarmObject = new DictAlarmObject();
            EventReg += new AlarmDbEventHandler(onEventReg);

            m_mEvtAlarmDbEventUpdated = new ManualResetEvent (false);

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
                foreach (TecViewAlarm tv in m_listTecView)
                    tv.Start(); //StartDbInterfaces (CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE);
            else ;

            OnWorkCheckedChanged(m_bWorkChecked);
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
                    if ((m_bWorkChecked == false)
                        //&& (IsFirstActivated == true)
                        )
                        pushList ();
                    else
                        ;
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
            int due = 0;

            if (bChecked == false)
            {
                due = System.Threading.Timeout.Infinite;
                m_mEvtAlarmDbEventUpdated.Reset();
            }
            else
                ;

            m_timerView.Change(due, System.Threading.Timeout.Infinite);
        }
        /// <summary>
        /// ���������� ������� ��������� �������� "��������/���������"
        /// </summary>
        /// <param name="obj">������, ������������� �������</param>
        public void OnWorkCheckedChanged(bool bChecked)
        {
            //if (! (m_bWorkChecked == bChecked)) {
                //??? - ������������ ������ ������/������/���������� ������ �������
                if (Mode == MODE.SERVICE)
                    activateAdminAlarm(bChecked);
                else
                    ;

                activateViewAlarm(bChecked);
            //} else ;
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
        /// ��������� � ������� ��������� ������� - ������ ������� ������� ������������ �� �� - ������� (��� ����������)!!!
        /// </summary>
        private void pushNotify()
        {
            //DateTime dtNotify = HDateTime.ToMoscowTimeZone();
            //��������� � ������� ���������
            push(new object[] //�������� ������� ��� ���������
                    { new object [] //1-� �������
                        {
                            StatesMachine.Notify
                            , HDateTime.ToMoscowTimeZone() //.Date //dtNotify.Date
                            , -1 * DictAlarmObject.DEPTH_HOUR_OBJECTALRM
                            , 0 //24
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
            m_mEvtAlarmDbEventUpdated.Reset ();
            // ��������� � ������� ������� ��������� ������ ������� ������� ������������
            pushNotify();
            // ��� ���������� �������
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
        private bool isToday { get { return m_dtCurrent.Date.Equals(HDateTime.ToMoscowTimeZone ().Date); } }
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

                m_mEvtAlarmDbEventUpdated.Set ();
            }
            else
                Logging.Logg().Exception(ev.Error, @"ViewAlarm::fThreadListEventsResponse_DoWork () - ...", Logging.INDEX_MESSAGE.NOT_SET);
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

            try
            {
                foreach (DataRow r in tableRes.Rows)
                {
                    listRes.Add(new PanelAlarm.ViewAlarmJournal()
                    {
                        m_id = (long)r[@"ID"]
                        , m_id_owner = getIdOwner((int)r[@"ID_COMPONENT"])
                        , m_id_component = (int)r[@"ID_COMPONENT"]
                        , m_str_name_shr_component = getNameComponent((int)r[@"ID_COMPONENT"])
                        , m_type = (INDEX_TYPE_ALARM)r[@"TYPE"]
                        , m_str_name_shr_type = (INDEX_TYPE_ALARM)r[@"TYPE"] == INDEX_TYPE_ALARM.CUR_POWER ? @"���.����." :
                          (INDEX_TYPE_ALARM)r[@"TYPE"] == INDEX_TYPE_ALARM.TGTurnOnOff ? @"�����/����" : string.Empty
                        , m_value = (float)r[@"VALUE"]
                        , m_id_user_registred = (int)r[@"ID_USER_REGISTRED"]
                        , m_dt_registred = (DateTime)r[@"DATETIME_REGISTRED"]
                        , m_id_user_fixed = (!(r[@"ID_USER_FIXED"] is DBNull)) ? (int)r[@"ID_USER_FIXED"] : -1
                        , m_dt_fixed = (!(r[@"DATETIME_FIXED"] is DBNull)) ? (DateTime?)r[@"DATETIME_FIXED"] : null
                        , m_id_user_confirmed = (!(r[@"ID_USER_CONFIRM"] is DBNull)) ? (int)r[@"ID_USER_CONFIRM"] : -1
                        , m_dt_confirmed = (!(r[@"DATETIME_CONFIRM"] is DBNull)) ? (DateTime?)r[@"DATETIME_CONFIRM"] : null
                        , m_situation = (int)r[@"SITUATION"]
                    });
                }
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, @"AdminAlarm::GetListEventsResponse () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            if (Actived == true)
                EvtGetDataMain(listRes);
            else ;
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
            List<PanelAlarm.ViewAlarmDetail> listRes = new List<PanelAlarm.ViewAlarmDetail> ();

            foreach (DataRow r in tableRes.Rows)
            {
                listRes.Add(new PanelAlarm.ViewAlarmDetail()
                {
                    m_id = (long)r[@"ID"]
                    , m_id_owner = getIdOwner((int)r[@"ID_COMPONENT"])
                    , m_id_event = (long)r[@"ID_EVENT"]
                    , m_id_component = (int)r[@"ID_COMPONENT"]
                    , m_str_name_shr_component = getNameComponent((int)r[@"ID_COMPONENT"])
                    , m_value = (float)r[@"VALUE"]
                    , m_last_changed_at = (DateTime?)r[@"last_changed_at"]
                });
            }
            //������� �1
            EvtGetDataDetail(listRes);
            ////������� �2 (����� ���������� 'PanelAlarm::OnEvtDataRecievedHost')
            //itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(new EventArgsDataHost(-1, new object[] { StatesMachine.Detail, listRes }));
        }

        private void GetUpdateFixedResponse(ItemQueue itemQueue, DataTable tableRes)
        {
            DataRow r = tableRes.Rows [0];
            m_dictAlarmObject.Fixed ((int)r[@"ID_COMPONENT"], (DateTime)r[@"DATETIME_REGISTRED"], (DateTime)r[@"DATETIME_FIXED"]);
            //��������� ������� �������������� ��������
            if (!(itemQueue.m_dataHostRecieved == null))
                itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(new EventArgsDataHost(-1, -1, new object[] { StatesMachine.Fixed, (long)r[@"ID"], (DateTime)r[@"DATETIME_FIXED"] }));
            else
                ;
        }

        private void GetUpdateConfirmResponse(ItemQueue itemQueue, DataTable tableRes)
        {
            DataRow r = tableRes.Rows[0];
            m_dictAlarmObject.Confirmed((int)r[@"ID_COMPONENT"], (DateTime)r[@"DATETIME_REGISTRED"], (DateTime)r[@"DATETIME_CONFIRM"]);
            //��������� ������� ��������������� �������������
            if (!(itemQueue.m_dataHostRecieved == null))
                itemQueue.m_dataHostRecieved.OnEvtDataRecievedHost(new EventArgsDataHost(-1, -1, new object[] { StatesMachine.Confirm, (long)r[@"ID"], (DateTime)r[@"DATETIME_CONFIRM"] }));
            else
                ;
        }

        private string getEventGUIMessage(AlarmNotifyEventArgs ev)
        {
            string strRes = getNameComponent (ev.m_id_comp);

            if (strRes.Equals (string.Empty) == false)
            {
                strRes += Environment.NewLine;
                strRes += HDateTime.ToMoscowTimeZone(ev.m_dtRegistred.GetValueOrDefault()).ToString();
                strRes += Environment.NewLine;
                strRes += ev.m_message_shr;
            }
            else
                ;

            return strRes;
        }

        private int getIdOwner(int id_comp)
        {
            int iRes = -1;
            FormChangeMode.MODE_TECCOMPONENT mode = TECComponent.Mode (id_comp);
            bool bContinue = true;

            foreach (TecView tv in m_listTecView)
            {
                foreach (TECComponent tc in tv.m_tec.list_TECComponents)
                {
                    if (mode == FormChangeMode.MODE_TECCOMPONENT.GTP)
                        if (tc.m_id == id_comp)
                        {
                            //iRes �������� ������������
                            bContinue = false;
                            break;
                        }
                        else
                            ;
                    else
                        if (mode == FormChangeMode.MODE_TECCOMPONENT.TG)
                            if (tc.IsGTP == true)
                                foreach (TECComponentBase lpd in tc.m_listLowPointDev)
                                    if (lpd.m_id == id_comp)
                                    {
                                        iRes = tc.m_id;
                                        bContinue = false;
                                        break;
                                    }
                                    else
                                        ;
                            else
                                ;
                        else
                            ;

                    if (bContinue == false)
                        break;
                    else
                        ;
                }

                if (bContinue == false)
                    break;
                else
                    ;
            }

            return iRes;
        }

        private string getNameComponent(int id_comp)
        {
            string strRes = string.Empty;
            bool bContinue = true;

            foreach (TecView tv in m_listTecView)
            {
                foreach (TECComponent tc in tv.m_tec.list_TECComponents)
                    if (tc.m_id == id_comp)
                    {
                        strRes += tv.m_tec.name_shr;
                        strRes += @", ";
                        strRes += tc.name_shr;
                        
                        bContinue = false;
                        break;
                    }
                    else
                        ;

                if (bContinue == false)
                    break;
                else
                    ;
            }

            return strRes;
        }
        /// <summary>
        /// ���������� ������� ����������� ������� �� ��
        /// </summary>
        /// <param name="ev">�������� ������� - �������� ������� ������������</param>
        private void onEventReg(AlarmDbEventArgs ev)
        {
            INDEX_ACTION iAction = m_dictAlarmObject.Registred (ev, Mode);
            if (iAction == INDEX_ACTION.ERROR)
                throw new Exception(@"ViewAlarm::onEventReg () - ...");
            else
                switch (iAction)
                {
                    // ���������� ������ ��� 'ADMIN'
                    case INDEX_ACTION.NEW:
                    case INDEX_ACTION.RETRY:
                        ev.m_messageGUI = getEventGUIMessage (ev);
                        if (iAction == INDEX_ACTION.NEW)
                            EventAdd(ev);
                        else
                            if (iAction == INDEX_ACTION.RETRY)
                                EventRetry(ev);
                            else
                                ;
                        break;
                    case INDEX_ACTION.AUTO_FIXING: // ������������/������������� ������ ��� 'SERVICE'
                        Push(null, new object[] {
                            new object [] {
                                new object [] {
                                    AdminAlarm.StatesMachine.Fixed
                                    , ev as AlarmNotifyEventArgs
                                }
                            }
                        });
                        break;
                    case INDEX_ACTION.AUTO_CONFIRMING: // ������������/������������� ������ ��� 'SERVICE'
                        Push(null, new object[] {
                            new object[] {
                                new object [] {
                                    AdminAlarm.StatesMachine.Confirm
                                    , new object [] { ev.m_id, ev.m_id_comp, ev.m_situation }
                                }
                            }
                        });
                        break;
                    //case INDEX_ACTION.CONFIRMED_TG:
                    //    tgConfirm(ev.m_id_comp, (TG.INDEX_TURNOnOff)ev.m_situation);
                    //    break;
                    default:
                        break;
                }
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
        ///// <summary>
        ///// ���������� ������� "������������" ��� ������� ������������
        ///// </summary>
        ///// <param name="id_comp">����� ���������� �����: ������������� ���</param>
        ///// <param name="id_tg">����� ���������� �����: ������������� ��</param>
        ///// <returns>���������: ������� ����������/��_����������)</returns>
        //public bool IsConfirmed(int id_comp, DateTime dtReg)
        //{
        //    return m_dictAlarmObject.IsConfirmed(id_comp, dtReg);
        //}
        ///// <summary>
        ///// �������� ����/����� ����������� ������� ������������ ��� ��
        ///// </summary>
        ///// <param name="id_comp">��������� ����� �����: ������������� ���</param>
        ///// <param name="id_tg">��������� ����� �����: ������������� ���</param>
        ///// <returns></returns>
        //public DateTime TGAlarmDatetimeReg(int id_comp, DateTime dtReg)
        //{
        //    return m_dictAlarmObject.TGAlarmDatetimeReg(id_comp, dtReg);
        //}
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
                case StatesMachine.Detail:
                case StatesMachine.Notify:
                case StatesMachine.Insert:
                case StatesMachine.Retry:
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
                case StatesMachine.Detail:
                    m_handlerDb.Detail((long)itemQueue.Pars[0]);
                    break;
                case StatesMachine.Insert:
                    m_handlerDb.Insert(itemQueue.Pars[0] as TecViewAlarm.AlarmTecViewEventArgs);
                    break;
                case StatesMachine.Retry:
                    m_handlerDb.Retry(itemQueue.Pars[0] as TecViewAlarm.AlarmTecViewEventArgs);
                    break;
                case StatesMachine.Fixed:
                    AlarmNotifyEventArgs ev = itemQueue.Pars[0] as AlarmNotifyEventArgs;
                    if (TECComponent.Mode(ev.m_id_comp) == FormChangeMode.MODE_TECCOMPONENT.TG)
                        tgConfirm(ev.m_id_comp, (TG.INDEX_TURNOnOff)ev.m_situation);
                    else
                        ;
                    m_handlerDb.Fixed(ev);
                    break;
                case StatesMachine.Confirm:
                    object[] pars = itemQueue.Pars[0] as object[];
                    //long id_rec = (long)(pars)[0];
                    //int id_comp = (int)(pars)[1];
                    //if (TECComponent.Mode(id_comp) == FormChangeMode.MODE_TECCOMPONENT.TG)
                    //    tgConfirm(id_comp, (TG.INDEX_TURNOnOff)pars[2]);
                    //else
                    //    ;
                    //m_handlerDb.Confirm(id_rec);
                    m_handlerDb.Confirm((long)(pars)[0]);
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
                case StatesMachine.Retry: // ���������� ������ � ������ 'SERVICE'
                    //GetRetryResponse(itemQueue, tableRes);
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
