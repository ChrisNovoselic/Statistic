using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data; //DataTable

using HClassLibrary; //HStates

namespace StatisticTimeSync
{
    class HGetDate : HStates
    {
        ConnectionSettings m_ConnSett;

        public HGetDate (ConnectionSettings connSett) {
            m_ConnSett = connSett;
        }

        public override void StartDbInterfaces()
        {
            m_dictIdListeners.Add(0, new int[] { -1 });

            register(0, m_ConnSett, m_ConnSett.name, 0);
        }

        public override void ClearValues()
        {
        }

        protected override bool StateRequest(int state) {
            bool bRes = true;

            return bRes;
        }

        protected override bool StateCheckResponse(int state, out bool error, out DataTable table)
        {
            bool bRes = true;
            error = false;
            table = null;

            return bRes;
        }

        protected override bool StateResponse(int state, DataTable table)
        {
            bool bRes = true;

            return bRes;
        }

        protected override void StateErrors(int state, bool response)
        {
        }
    }
}
