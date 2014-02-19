using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

//namespace Statistic
namespace StatisticCommon
{
    public abstract class DbInterface
    {
        public static int MAX_RETRY = 3;
        public static int MAX_WAIT_COUNT = 25;
        public static int WAIT_TIME_MS = 66;

        protected class DbInterfaceListener
        {
            public volatile bool listenerActive; 
            public volatile bool dataPresent;
            public volatile bool dataError;
            public volatile object requestDB;
            public volatile DataTable dataTable;

            public DbInterfaceListener () {
                listenerActive =
                dataPresent =
                dataError =
                false;

                requestDB = null; //new object ();
                dataTable = new DataTable ();
            }
        }
        protected List <DbInterfaceListener> m_listListeners;
        //private int maxListeners;

        public object m_connectionSettings;

        protected object lockListeners;
        protected object lockConnectionSettings;
        private Thread dbThread;
        private Semaphore sem;
        private volatile bool threadIsWorking;
        private string m_ThreadName;

        protected bool needReconnect;
        private bool connected;

        public DbInterface(string name)
        {
            m_ThreadName = name;

            lockListeners = new object();
            lockConnectionSettings = new object();
            
            //listeners = new DbInterfaceListener[maxListeners];
            m_listListeners = new List <DbInterfaceListener> ();

            connected = false;
            needReconnect = false;

            dbThread = new Thread(new ParameterizedThreadStart(DbInterface_ThreadFunction));
            dbThread.Name = m_ThreadName;
            dbThread.IsBackground = true;

            sem = new Semaphore(1, 1);
        }

        public bool Connected
        {
            get { return connected; }
        }

        public int ListenerRegister()
        {
            lock (lockListeners)
            {
                m_listListeners.Add (new DbInterfaceListener ());
                m_listListeners [m_listListeners.Count - 1].listenerActive = true;
                return m_listListeners.Count - 1;
            }

            //return -1;
        }

        public void ListenerUnregister(int listenerId)
        {
            if ((! (listenerId < m_listListeners.Count)) || listenerId < 0)
                return;

            lock (lockListeners)
            {
                m_listListeners.RemoveAt(listenerId);
            }
        }

        public void Start()
        {
            threadIsWorking = true;
            sem.WaitOne();
            dbThread.Start();
        }

        public void Stop()
        {
            bool joined;
            threadIsWorking = false;
            lock (lockListeners)
            {
                for (int i = 0; i < m_listListeners.Count; i++)
                {
                    m_listListeners [i].requestDB = null;
                }
            }
            if (dbThread.IsAlive)
            {
                try
                {
                    sem.Release(1);
                }
                catch
                {
                }

                joined = dbThread.Join(5000);
                if (joined == false)
                {
                    Disconnect ();
                    dbThread.Abort();
                }
                else
                    ;
            }
            else
                ;
        }

        public void Request(int listenerId, string request)
        {
            if ((!(listenerId < m_listListeners.Count)) || (listenerId < 0) || (request.Length == 0))
                return;
            else
                ;

            lock (lockListeners)
            {
                m_listListeners[listenerId].requestDB = request;
                m_listListeners[listenerId].dataPresent = false;
                m_listListeners[listenerId].dataError = false;
            }

            try
            {
                sem.Release(1);
            }
            catch
            {
            }
        }

        public bool GetResponse(int listenerId, out bool error, out DataTable table)
        {
            if ((!(listenerId < m_listListeners.Count)) || listenerId < 0)
            {
                error = true;
                table = null;

                return false;
            }
            else
                ;

            error = m_listListeners[listenerId].dataError;
            table = m_listListeners[listenerId].dataTable;

            return m_listListeners[listenerId].dataPresent;
        }

        protected void SetConnectionSettings()
        {
            try
            {
                sem.Release(1);
            }
            catch (Exception e)
            {
                string msg = string.Empty;
                msg += "!Исключение! обращения к переменной sem (вызов: sem.Release ())" + Environment.NewLine;
                msg += "Исключение " + e.Message + Environment.NewLine;
                msg += e.ToString();
                Logging.Logg().LogToFile(msg, true, true, true);
            }
        }

        public abstract void SetConnectionSettings(object cs);

        private void DbInterface_ThreadFunction(object data)
        {
            object request;
            bool result;
            bool reconnection/* = false*/;

            while (threadIsWorking)
            {
                sem.WaitOne();

                lock (lockConnectionSettings) // атомарно читаю и сбрасываю флаг, чтобы при параллельной смене настроек не сбросить повторно выставленный флаг
                {
                    reconnection = needReconnect;
                    needReconnect = false;
                }

                if (reconnection == true)
                {
                    Disconnect();
                    connected = false;
                    if (threadIsWorking && Connect())
                        connected = true;
                    else
                        needReconnect = true; // выставлять флаг можно без блокировки
                }
                else
                    ;

                if (connected == false) // не удалось подключиться - не пытаемся получить данные
                    continue;
                else
                    ;

                for (int i = 0; i < m_listListeners.Count; i++)
                {
                    lock (lockListeners)
                    {
                        if (! m_listListeners [i].listenerActive)
                            continue;
                        else
                            ;

                        request = m_listListeners [i].requestDB;

                        if ((request == null) || (!(request.ToString ().Length > 0)))
                            continue;
                        else
                            ;
                    }

                    try
                    {
                        result = GetData(m_listListeners [i].dataTable, request);
                    }
                    catch (DbException e)
                    {
                        Logging.Logg().LogExceptionToFile(e, "DbInterface::DbInterface_ThreadFunction () - result = GetData(...) - request = " + request);
                        
                        result = false;
                    }

                    lock (lockListeners)
                    {
                        if ((!(i < m_listListeners.Count)) || (m_listListeners[i].listenerActive == false))
                            continue;
                        else
                            ;

                        if (request == m_listListeners[i].requestDB)
                        {
                            if (result)
                            {
                                m_listListeners[i].dataPresent = true;
                            }
                            else
                            {
                                m_listListeners[i].dataError = true;
                            }

                            m_listListeners[i].requestDB = null;
                        }
                    }
                } //for (int i = 0; i < m_listListeners.Count; i++)
            } //while (threadIsWorking)
            
            try
            {
                sem.Release(1);
            }
            catch
            {
            }

            Disconnect ();
        }

        protected abstract bool Connect();

        protected abstract bool Disconnect();

        protected abstract bool GetData(DataTable table, object query);
    }
}
