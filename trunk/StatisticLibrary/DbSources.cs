using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;

namespace StatisticCommon
{
    public class DbSources
    {
        protected class DbSourceListener
        {
            public volatile bool listenerActive;
            public volatile int indxDbInterface;
            public volatile int indxListenerId;

            public DbSourceListener()
            {
            }
        }
        protected List<DbSourceListener> m_listListeners;

        protected List<DbInterface> m_listDbInterfaces;
        
        public DbSources()
        {
        }

        public int Register(ConnectionSettings connSett)
        {
            int iRes = -1;

            return iRes;
        }

        public void UnRegister(int id)
        {
        }

        public void Request (int id) {
        }

        public void Response()
        {
        }
    }
}