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
    public class DbInterface
    {
        public static int MAX_RETRY = 2;
        public static int MAX_WAIT_COUNT = 25;
        public static int WAIT_TIME_MS = 100;

        public enum DBINTERFACE_TYPE
        {
            MySQL,
            MSSQL,
            MSExcel
        }

        public enum QUERY_TYPE { UPDATE, INSERT, DELETE, COUNT_QUERY_TYPE };

        private class DbInterfaceListener
        {
            public volatile bool listenerActive; 
            public volatile bool dataPresent;
            public volatile bool dataError;
            public volatile string requestDB;
            public volatile DataTable dataTable;

            public DbInterfaceListener () {
                listenerActive =
                dataPresent =
                dataError =
                false;

                requestDB = string.Empty;
                dataTable = new DataTable ();
            }
        }
        private List <DbInterfaceListener> m_listListeners;
        //private int maxListeners;

        private object lockListeners;
        private object lockConnectionSettings;
        private Thread dbThread;
        private Semaphore sem;
        private volatile bool threadIsWorking;
        private string m_ThreadName;

        private DbConnection m_dbConnection;
        private DbCommand m_dbCommand;
        private DbDataAdapter m_dbAdapter;

        public ConnectionSettings connectionSettings;
        private bool needReconnect;
        private DBINTERFACE_TYPE m_connectionType;
        private bool connected;

        public DbInterface(DBINTERFACE_TYPE type, string name)
        {
            m_ThreadName = name;
            
            //maxListeners = maxListenersAllowed;
            m_connectionType = type;
            lockListeners = new object();
            lockConnectionSettings = new object();
            
            //listeners = new DbInterfaceListener[maxListeners];
            m_listListeners = new List <DbInterfaceListener> ();

            connected = false;
            needReconnect = false;
            connectionSettings = new ConnectionSettings();

            switch (m_connectionType)
            {
                case DBINTERFACE_TYPE.MySQL:
                    m_dbConnection = new MySqlConnection();

                    m_dbCommand = new MySqlCommand();
                    m_dbCommand.Connection = m_dbConnection;
                    m_dbCommand.CommandType = CommandType.Text;

                    m_dbAdapter = new MySqlDataAdapter();
                    break;
                case DBINTERFACE_TYPE.MSSQL:
                    m_dbConnection = new SqlConnection();

                    m_dbCommand = new SqlCommand();
                    m_dbCommand.Connection = m_dbConnection;
                    m_dbCommand.CommandType = CommandType.Text;

                    m_dbAdapter = new SqlDataAdapter();
                    break;
                case DBINTERFACE_TYPE.MSExcel:
                    m_dbConnection = new OleDbConnection();

                    m_dbCommand = new OleDbCommand();
                    m_dbCommand.Connection = m_dbConnection;
                    m_dbCommand.CommandType = CommandType.Text;

                    m_dbAdapter = new OleDbDataAdapter();
                    break;
                default:
                    break;
            }

            m_dbAdapter.SelectCommand = m_dbCommand;

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
                    m_listListeners [i].requestDB = "";
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
                if (!joined)
                    dbThread.Abort();
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

        public void SetConnectionSettings(ConnectionSettings cs)
        { 
            lock (lockConnectionSettings) // изменение настроек подключения и выставление флага для переподключения - атомарная операция
            {
                connectionSettings.server = cs.server;
                connectionSettings.dbName = cs.dbName;
                connectionSettings.userName = cs.userName;
                connectionSettings.password = cs.password;
                connectionSettings.port = cs.port;
                connectionSettings.ignore = cs.ignore;
                needReconnect = true;
            }

            try
            {
                sem.Release(1);
            }
            catch (Exception e)
            {
                Logging.Logg().LogLock();
                Logging.Logg().LogToFile("Исключение обращения к переменной sem (вызов: sem.Release ())", true, true, false);
                Logging.Logg().LogToFile("Исключение " + e.Message, false, false, false);
                Logging.Logg().LogToFile(e.ToString(), false, false, false);
                Logging.Logg().LogUnlock();
            }
        }

        private void DbInterface_ThreadFunction(object data)
        {
            string request;
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

                if (reconnection)
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

                if (!connected) // не удалось подключиться - не пытаемся получить данные
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

                        if (request == "")
                            continue;
                        else
                            ;
                    }

                    try
                    {
                        result = GetData(m_listListeners [i].dataTable, request);
                    }
                    catch
                    {
                        result = false;
                    }

                    lock (lockListeners)
                    {
                        if (!m_listListeners[i].listenerActive)
                            continue;

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
                            m_listListeners[i].requestDB = "";
                        }
                    }
                }
            }
            try
            {
                sem.Release(1);
            }
            catch
            {
            }
        }

        private bool Connect()
        {
            if (connectionSettings.Validate() != ConnectionSettings.ConnectionSettingsError.NoError)
                return false;
            else
                ;

            bool result =false, bRes = false;

            if (m_dbConnection.State == ConnectionState.Open)
                bRes = true;
            else
                ;

            try
            {
                if (bRes == true)
                    return bRes;
                else
                    bRes = true;
            }
            catch (Exception e)
            {
                Logging.Logg().LogLock();
                Logging.Logg().LogToFile("Исключение обращения к переменной", true, true, false);
                Logging.Logg().LogToFile("Исключение " + e.Message, false, false, false);
                Logging.Logg().LogToFile(e.ToString(), false, false, false);
                Logging.Logg().LogUnlock();
            }

            if (m_dbConnection.State != ConnectionState.Closed)
                bRes = false;
            else
                ;

            if (bRes == false)
                return bRes;
            else
                ;

            lock (lockConnectionSettings)
            {
                if (needReconnect) // если перед приходом в данную точку повторно были изменены настройки, то подключения со старыми настройками не делаем
                    return false;
                else
                    ;

                if (connectionSettings.ignore)
                    return false;
                else
                    ;

                //string connStr = string.Empty;
                switch (m_connectionType) {
                    case DBINTERFACE_TYPE.MSSQL:
                        //connStr = connectionSettings.GetConnectionStringMSSQL();
                        ((SqlConnection)m_dbConnection).ConnectionString = connectionSettings.GetConnectionStringMSSQL();
                        break;
                    case DBINTERFACE_TYPE.MySQL:
                        //connStr = connectionSettings.GetConnectionStringMySQL();
                        ((MySqlConnection)m_dbConnection).ConnectionString = connectionSettings.GetConnectionStringMySQL();
                        break;
                    case DBINTERFACE_TYPE.MSExcel:
                        //((OleDbConnection)m_dbConnection).ConnectionString = ConnectionSettings.GetConnectionStringExcel ();
                        break;
                    default:
                        break;
                }
                //m_dbConnection.ConnectionString = connStr;
            }

            try
            {
                m_dbConnection.Open();
                result = true;
                string s;
                int pos;
                pos = m_dbConnection.ConnectionString.IndexOf("Password", StringComparison.CurrentCultureIgnoreCase);
                if (pos < 0)
                    s = m_dbConnection.ConnectionString;
                else
                    s = m_dbConnection.ConnectionString.Substring(0, pos);

                Logging.Logg().LogLock();
                Logging.Logg().LogToFile("Соединение с базой установлено (" + s + ")", true, true, false);
                Logging.Logg().LogUnlock();
            }
            catch (MySqlException e)
            {
                logging_catch_openDb(m_dbConnection, e);
            }
            catch
            {
                logging_catch_openDb(m_dbConnection, null);
            }

            return result;
        }

        private bool Disconnect()
        {
            if (m_dbConnection.State == ConnectionState.Closed)
                return true; 
            
            bool result = false;

            try
            {
                m_dbConnection.Close();
                result = true;
                string s;
                int pos;
                pos = m_dbConnection.ConnectionString.IndexOf("Password", StringComparison.CurrentCultureIgnoreCase);
                if (pos < 0)
                    s = m_dbConnection.ConnectionString;
                else
                    s = m_dbConnection.ConnectionString.Substring(0, pos);
                Logging.Logg().LogToFile("Соединение с базой разорвано (" + s + ")", true, true, false);

            }
            catch (DbException e)
            {
                Logging.Logg().LogLock();
                Logging.Logg().LogToFile("Ошибка закрытия соединения", true, true, false);
                Logging.Logg().LogToFile("Ошибка " + e.Message, false, false, false);
                Logging.Logg().LogToFile(e.ToString(), false, false, false);
                Logging.Logg().LogUnlock();
            }
            catch
            {
                Logging.Logg().LogToFile("Ошибка закрытия соединения", true, true, false);
            }
            
            return result;
        }

        private bool GetData(DataTable table, string query)
        {
            if (m_dbConnection.State != ConnectionState.Open)
                return false;
            else
                ;

            bool result = false;

            m_dbCommand.CommandText = query;

            table.Reset();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;

            try
            {
                m_dbAdapter.Fill(table);
                result = true;
            }
            catch (DbException e)
            {
                needReconnect = true;
                Logging.Logg().LogLock();
                string s;
                int pos;
                pos = m_dbAdapter.SelectCommand.Connection.ConnectionString.IndexOf("Password", StringComparison.CurrentCultureIgnoreCase);
                if (pos < 0)
                    s = m_dbAdapter.SelectCommand.Connection.ConnectionString;
                else
                    s = m_dbAdapter.SelectCommand.Connection.ConnectionString.Substring(0, pos);

                Logging.Logg().LogToFile("Ошибка получения данных", true, true, false);
                Logging.Logg().LogToFile("Строка соединения " + s, false, false, false);
                Logging.Logg().LogToFile("Запрос " + m_dbAdapter.SelectCommand.CommandText, false, false, false);
                Logging.Logg().LogToFile("Ошибка " + e.Message, false, false, false);
                Logging.Logg().LogToFile(e.ToString(), false, false, false);
                Logging.Logg().LogUnlock();
            }
            catch
            {
                needReconnect = true;
                logging_catch_openDb (m_dbConnection, null);
            }

            return result;
        }

        private static void logging_catch_openDb (DbConnection conn, Exception e) {
            Logging.Logg().LogLock();
            string s;
            int pos;
            pos = conn.ConnectionString.IndexOf("Password", StringComparison.CurrentCultureIgnoreCase);
            if (pos < 0)
                s = conn.ConnectionString;
            else
                s = conn.ConnectionString.Substring(0, pos);

            Logging.Logg().LogToFile("Ошибка открытия соединения", true, true, false);
            Logging.Logg().LogToFile("Строка соединения: " + s, false, false, false);
            if (!(e == null)) {
                Logging.Logg().LogToFile("Ошибка: " + e.Message, false, false, false);
                Logging.Logg().LogToFile(e.ToString(), false, false, false);
            }
            else
                ;
            Logging.Logg().LogUnlock();
        }

        public static DataTable Select (string path, string query) {
            DataTable dataTableRes = new DataTable();

            OleDbConnection connectionOleDB = null;
            System.Data.OleDb.OleDbCommand commandOleDB;
            System.Data.OleDb.OleDbDataAdapter adapterOleDB;

            if (path.IndexOf ("xls") > -1)
                connectionOleDB = new OleDbConnection (ConnectionSettings.GetConnectionStringExcel (path));
            else
                //if (path.IndexOf ("dbf") > -1)
                    connectionOleDB = new OleDbConnection (ConnectionSettings.GetConnectionStringDBF (path));
                //else
                //    ;

            if (! (connectionOleDB == null)) {
                commandOleDB = new OleDbCommand();
                commandOleDB.Connection = connectionOleDB;
                commandOleDB.CommandType = CommandType.Text;

                adapterOleDB = new OleDbDataAdapter();
                adapterOleDB.SelectCommand = commandOleDB;

                commandOleDB.CommandText = query;

                dataTableRes.Reset();
                dataTableRes.Locale = System.Globalization.CultureInfo.InvariantCulture;

                try
                {
                    connectionOleDB.Open();

                    if (connectionOleDB.State == ConnectionState.Open)
                    {
                        adapterOleDB.Fill(dataTableRes);
                    }
                    else
                        ; //
                }
                catch (OleDbException e)
                {
                    logging_catch_openDb (connectionOleDB, e);
                }

                connectionOleDB.Close();
            }
            else
                ;

            return dataTableRes;
        }

        public static DataTable Select(ConnectionSettings connSett, string query)
        {
            DataTable dataTableRes = new DataTable();

            MySqlConnection connectionMySQL;
            MySqlCommand commandMySQL;
            MySqlDataAdapter adapterMySQL;

            connectionMySQL = new MySqlConnection(connSett.GetConnectionStringMySQL());

            commandMySQL = new MySqlCommand();
            commandMySQL.Connection = connectionMySQL;
            commandMySQL.CommandType = CommandType.Text;

            adapterMySQL = new MySqlDataAdapter();
            adapterMySQL.SelectCommand = commandMySQL;

            commandMySQL.CommandText = query;

            dataTableRes.Reset();
            dataTableRes.Locale = System.Globalization.CultureInfo.InvariantCulture;

            try {
                connectionMySQL.Open();

                if (connectionMySQL.State == ConnectionState.Open)
                {
                    adapterMySQL.Fill(dataTableRes);
                }
                else
                    ; //
            }
            catch //(MySqlException e)
            {
                logging_catch_openDb (connectionMySQL, null);
            }

            connectionMySQL.Close();

            return dataTableRes;
        }

        public static void Insert(ConnectionSettings connSett, string query)
        {
        }

        public static void Insert(string path, string query)
        {
        }

        public static void Update(ConnectionSettings connSett, string query)
        {
            MySqlConnection connectionMySQL;
            MySqlCommand commandMySQL;

            connectionMySQL = new MySqlConnection(connSett.GetConnectionStringMySQL());

            commandMySQL = new MySqlCommand();
            commandMySQL.Connection = connectionMySQL;
            commandMySQL.CommandType = CommandType.Text;

            commandMySQL.CommandText = query;

            try
            {
                connectionMySQL.Open();

                if (connectionMySQL.State == ConnectionState.Open)
                {
                    commandMySQL.ExecuteNonQuery();
                }
                else
                    ; //
            }
            catch //(MySqlException e)
            {
                logging_catch_openDb(connectionMySQL, null);
            }

            connectionMySQL.Close();
        }

        public static void Update(string path, string query)
        {
            OleDbConnection connectionOleDB = null;
            System.Data.OleDb.OleDbCommand commandOleDB;

            if (path.IndexOf("xls") > -1)
                connectionOleDB = new OleDbConnection(ConnectionSettings.GetConnectionStringExcel(path));
            else
                //if (path.IndexOf ("dbf") > -1)
                    connectionOleDB = new OleDbConnection(ConnectionSettings.GetConnectionStringDBF(path));
                //else
                //    ;

            if (!(connectionOleDB == null))
            {
                commandOleDB = new OleDbCommand();
                commandOleDB.Connection = connectionOleDB;
                commandOleDB.CommandType = CommandType.Text;

                commandOleDB.CommandText = query;

                try
                {
                    connectionOleDB.Open();

                    if (connectionOleDB.State == ConnectionState.Open)
                    {
                        commandOleDB.ExecuteNonQuery();

                        Logging.Logg().LogLock();
                        Logging.Logg().LogToFile(connectionOleDB.ConnectionString, true, true, false);
                        Logging.Logg().LogToFile(commandOleDB.CommandText, true, false, false);
                        Logging.Logg().LogUnlock();
                    }
                    else
                        ; //
                }
                catch (OleDbException e)
                {
                    logging_catch_openDb(connectionOleDB, e);
                }

                connectionOleDB.Close();
            }
            else
                ;
        }
    }
}
