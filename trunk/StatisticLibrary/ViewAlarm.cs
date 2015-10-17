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
        private enum StatesMachine { CurrentTime, ListEvents, InsertEvent, UpdateEventFixed, UpdateEventConfirm }
        private ConnectionSettings m_connSett;

        public ViewAlarm(ConnectionSettings connSett)
            : base()
        {
            m_connSett = connSett;
        }

        public override void Start()
        {
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
            GetCurrentTimeRequest(DbTSQLInterface.getTypeDB(m_connSett.port), m_dictIdListeners[0][0]);
        }

        private void GetCurrentTimeResponse(DataTable tableRes)
        {
        }

        private void GetListEventsRequest()
        {
        }

        private void GetListEventsResponse(DataTable tableRes)
        {
        }

        private void GetInsertEventRequest()
        {
        }

        private void GetUpdateEventFixedRequest()
        {
        }

        private void GetUpdateEventConfirmRequest()
        {
        }

        public void OnEventDateChanged(DateTime date)
        {
        }
    }
}
