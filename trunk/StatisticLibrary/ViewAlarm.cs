using System;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading;

using HClassLibrary;

namespace StatisticCommon
{
    public class ViewAlarm : HHandlerDb
    {
        public event DelegateObjectFunc EvtGetData;
        
        private enum StatesMachine { CurrentTime, ListEvents, InsertEvent, UpdateEventFixed, UpdateEventConfirm }
        private ConnectionSettings m_connSett;
        private DateTime m_dtCurrent
            , m_dtServer;
        private int m_iHourBegin
            , m_iHourEnd;

        private int IdListener { get { return m_dictIdListeners[0][0]; } }

        private TecView.EventRegEventArgs m_EventRegEventArg;

        public ViewAlarm(ConnectionSettings connSett)
            : base()
        {
            m_connSett = connSett;
        }

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
            m_dictIdListeners.Add (0, new int [] { -1 });
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
                    iRes = response(out error, out table);
                    break;
                case StatesMachine.InsertEvent:
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
                case StatesMachine.InsertEvent:
                    GetInsertEventRequest();
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

            //Logging.Logg().Debug(@"ViewAlarm::StateRequest () - state=" + ((StatesMachine)state).ToString() + @", result=" + bRes.ToString() + @" - вџход...");

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
                case StatesMachine.InsertEvent:
                case StatesMachine.UpdateEventFixed:
                case StatesMachine.UpdateEventConfirm:
                    ;
                    break;
                default:
                    break;
            }

            //Logging.Logg().Debug(@"ViewAlarm::StateRequest () - state=" + ((StatesMachine)state).ToString() + @", result=" + bRes.ToString() + @" - вџход...");

            return iRes;
        }

        protected override HHandler.INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
        {
            throw new NotImplementedException();
        }

        protected override void StateWarnings(int state, int req, int res)
        {
            throw new NotImplementedException();
        }

        public override void ClearValues()
        {//??? - необ€зательное наличие - удалить из 'HHandlerDb'
            throw new NotImplementedException();
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

        private void GetListEventsResponse(DataTable tableRes)
        {
            Console.WriteLine (@"—обытий за " + m_dtCurrent.ToShortDateString () + @" (" + m_iHourBegin + @"-" + m_iHourEnd + @" ч): " + tableRes.Rows.Count);
            EvtGetData (tableRes);
        }

        private void GetInsertEventRequest()
        {
            int id = m_EventRegEventArg.m_id_tg < 0 ? m_EventRegEventArg.m_id_gtp : m_EventRegEventArg.m_id_tg //ID_COMPONENT
                , typeAlarm = (m_EventRegEventArg.m_id_tg < 0 ? 1 : 2) //TYPE
                , id_user = 0; //ID_USER
            double val = m_EventRegEventArg.m_id_tg < 0 ? -1 : m_EventRegEventArg.m_listEventDetail[0].value; //VALUE
            string strDTRegistred = m_EventRegEventArg.m_dtRegistred.ToString(@"yyyyMMdd HH:mm:ss.ffffffff"); //DATETIME_REGISTRED
            
            string query = "INSERT INTO [techsite-2.X.X].[dbo].[AlarmEvent] ([ID_COMPONENT], [TYPE], [ID_USER], [VALUE], [DATETIME_REGISTRED], [DATETIME_FIXED], [DATETIME_CONFIRM], [INSERT_DATETIME], [MESSAGE]) VALUES"
                + @"("
                    + id + @", " //ID_COMPONENT
                    + typeAlarm + @", " //TYPE
                    + id_user + @", " //ID_USER
                    + val + @", " //VALUE
                    + strDTRegistred + @", " //DATETIME_REGISTRED
                    + "NULL" + @", " //DATETIME_FIXED
                    + "NULL" + @", " //DATETIME_CONFIRM
                    + "GETDATE ()" + @", " //INSERT_DATETIME
                    + @"'" + m_EventRegEventArg.m_message + @"'" //MESSAGE
                + @")";
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

            Refresh ();
        }

        public void Refresh ()
        {
            ClearStates ();

            states.Add ((int)StatesMachine.CurrentTime);
            states.Add ((int)StatesMachine.ListEvents);

            Run (@"ViewAlarm::Refresh");
        }

        public void Refresh (DateTime dtCurrent, int iHourBegin, int iHourEnd)
        {
            m_dtCurrent = dtCurrent;
            m_iHourBegin = iHourBegin;            
            m_iHourEnd = iHourEnd;

            Refresh ();
        }

        public void Insert (TecView.EventRegEventArgs ev)
        {
            m_EventRegEventArg = ev;

            ClearStates();

            states.Add((int)StatesMachine.CurrentTime);
            states.Add((int)StatesMachine.InsertEvent);

            Run(@"ViewAlarm::Insert");
        }
    }
}
