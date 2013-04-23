using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace Statistic
{
    public class DbInterface
    {
        public enum DbInterfaceType
        {
            MySQL,
            MSSQL
        }

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

        private MySqlConnection connectionMySQL;
        private MySqlCommand commandMySQL;
        private MySqlDataAdapter adapterMySQL;
        private SqlConnection connectionMSSQL;
        private SqlCommand commandMSSQL;
        private SqlDataAdapter adapterMSSQL;

        public ConnectionSettings connectionSettings;
        private bool needReconnect;
        private DbInterfaceType connectionType;
        private bool connected;

        public DbInterface(DbInterfaceType type)
        {
            //maxListeners = maxListenersAllowed;
            connectionType = type;
            lockListeners = new object();
            lockConnectionSettings = new object();
            
            //listeners = new DbInterfaceListener[maxListeners];
            m_listListeners = new List <DbInterfaceListener> ();

            connected = false;
            needReconnect = false;
            connectionSettings = new ConnectionSettings();

            if (connectionType == DbInterfaceType.MySQL)
            {
                connectionMySQL = new MySqlConnection();

                commandMySQL = new MySqlCommand();
                commandMySQL.Connection = connectionMySQL;
                commandMySQL.CommandType = CommandType.Text;

                adapterMySQL = new MySqlDataAdapter();
                adapterMySQL.SelectCommand = commandMySQL;
            }
            else
            {
                connectionMSSQL = new SqlConnection();

                commandMSSQL = new SqlCommand();
                commandMSSQL.Connection = connectionMSSQL;
                commandMSSQL.CommandType = CommandType.Text;

                adapterMSSQL = new SqlDataAdapter();
                adapterMSSQL.SelectCommand = commandMSSQL;
            }

            dbThread = new Thread(new ParameterizedThreadStart(DbInterface_ThreadFunction));
            dbThread.Name = "Интерфейс БД";
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
        }
        
        public void Request(int listenerId, string request)
        {
            if ((! (listenerId < m_listListeners.Count)) || listenerId < 0)
                return;

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
            catch
            {
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

                if (!connected) // не удалось подключиться - не пытаемся получить данные
                    continue;

                for (int i = 0; i < m_listListeners.Count; i++)
                {
                    lock (lockListeners)
                    {
                        if (! m_listListeners [i].listenerActive)
                            continue;
                        request = m_listListeners [i].requestDB;
                        if (request == "")
                            continue;
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
            if (connectionType == DbInterfaceType.MySQL)
                return ConnectMySQL();
            if (connectionType == DbInterfaceType.MSSQL)
                return ConnectMSSQL();
            return false;
        }

        private bool Disconnect()
        {
            if (connectionType == DbInterfaceType.MySQL)
                return DisconnectMySQL();
            if (connectionType == DbInterfaceType.MSSQL)
                return DisconnectMSSQL();
            return false;
        }

        private bool GetData(DataTable table, string query)
        {
            if (connectionType == DbInterfaceType.MySQL)
                return GetDataMySQL(table, query);
            if (connectionType == DbInterfaceType.MSSQL)
                return GetDataMSSQL(table, query);
            return false;
        }

        private bool ConnectMySQL()
        {
            bool result =false, bRes = false;

            if (connectionMySQL.State == ConnectionState.Open)
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
                MainForm.log.LogLock();
                MainForm.log.LogToFile("Исключение обращения к переменной", true, true, false);
                MainForm.log.LogToFile("Исключение " + e.Message, false, false, false);
                MainForm.log.LogToFile(e.ToString(), false, false, false);
                MainForm.log.LogUnlock();
            }
            catch
            {
                MainForm.log.LogLock();
                MainForm.log.LogToFile("Исключение обращения к переменной", true, true, false);
                MainForm.log.LogUnlock();
            }


            if (connectionMySQL.State != ConnectionState.Closed)
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
                if (connectionSettings.ignore)
                    return false;
                connectionMySQL.ConnectionString = connectionSettings.GetConnectionStringMySQL();
            }
            //connectionMySQL.ConnectionString = @"Server=localhost;Database=Statistic;User id=root;Password=;";

            try
            {
                connectionMySQL.Open();
                result = true;
                string s;
                int pos;
                pos = connectionMySQL.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = connectionMySQL.ConnectionString;
                else
                    s = connectionMySQL.ConnectionString.Substring(0, pos);

                MainForm.log.LogLock();
                MainForm.log.LogToFile("Соединение с базой установлено (" + s + ")", true, true, false);
                MainForm.log.LogUnlock();
            }
            catch (MySqlException e)
            {
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = connectionMySQL.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = connectionMySQL.ConnectionString;
                else
                    s = connectionMySQL.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("Ошибка открытия соединения", true, true, false);
                MainForm.log.LogToFile("Строка соединения " + s, false, false, false);
                MainForm.log.LogToFile("Ошибка " + e.Message, false, false, false);
                MainForm.log.LogToFile(e.ToString(), false, false, false);
                MainForm.log.LogUnlock();
            }
            catch
            {
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = connectionMySQL.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = connectionMySQL.ConnectionString;
                else
                    s = connectionMySQL.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("Ошибка открытия соединения", true, true, false);
                MainForm.log.LogToFile("Строка соединения " + s, false, false, false);
                MainForm.log.LogUnlock();
            }

            return result;
        }

        private bool DisconnectMySQL()
        {
            if (connectionMySQL.State == ConnectionState.Closed)
                return true; 
            
            bool result = false;

            try
            {
                connectionMySQL.Close();
                result = true;
                string s;
                int pos;
                pos = connectionMySQL.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = connectionMySQL.ConnectionString;
                else
                    s = connectionMySQL.ConnectionString.Substring(0, pos);
                MainForm.log.LogToFile("Соединение с базой разорвано (" + s + ")", true, true, false);

            }
            catch (MySqlException e)
            {
                MainForm.log.LogLock();
                MainForm.log.LogToFile("Ошибка закрытия соединения", true, true, false);
                MainForm.log.LogToFile("Ошибка " + e.Message, false, false, false);
                MainForm.log.LogToFile(e.ToString(), false, false, false);
                MainForm.log.LogUnlock();
            }
            catch
            {
                MainForm.log.LogToFile("Ошибка закрытия соединения", true, true, false);
            }
            
            return result;
        }

        private bool GetDataMySQL(DataTable table, string query)
        {
            if (connectionMySQL.State != ConnectionState.Open)
                return false;
            bool result = false;

            commandMySQL.CommandText = query;

            table.Reset();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;

            try
            {
                adapterMySQL.Fill(table);
                result = true;
            }
            catch (MySqlException e)
            {
                needReconnect = true;
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = adapterMySQL.SelectCommand.Connection.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = adapterMySQL.SelectCommand.Connection.ConnectionString;
                else
                    s = adapterMySQL.SelectCommand.Connection.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("Ошибка получения данных", true, true, false);
                MainForm.log.LogToFile("Строка соединения " + s, false, false, false);
                MainForm.log.LogToFile("Запрос " + adapterMySQL.SelectCommand.CommandText, false, false, false);
                MainForm.log.LogToFile("Ошибка " + e.Message, false, false, false);
                MainForm.log.LogToFile(e.ToString(), false, false, false);
                MainForm.log.LogUnlock();
            }
            catch
            {
                needReconnect = true;
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = adapterMySQL.SelectCommand.Connection.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = adapterMySQL.SelectCommand.Connection.ConnectionString;
                else
                    s = adapterMySQL.SelectCommand.Connection.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("Ошибка получения данных", true, true, false);
                MainForm.log.LogToFile("Строка соединения " + s, false, false, false);
                MainForm.log.LogToFile("Запрос " + adapterMySQL.SelectCommand.CommandText, false, false, false);
                MainForm.log.LogUnlock();
            }

            return result;
        }

        private bool ConnectMSSQL()
        {
            if (connectionMSSQL.State == ConnectionState.Open)
                return true;
            if (connectionMSSQL.State != ConnectionState.Closed)
                return false;

            bool result = false;

            lock (lockConnectionSettings)
            {
                if (needReconnect) // если перед приходом в данную точку повторно были изменены настройки, то подключения со старыми настройками не делаем
                    return false;
                connectionMSSQL.ConnectionString = connectionSettings.GetConnectionStringMSSQL();
            }
            //connectionMSSQL.ConnectionString = @"Data Source=.\SQLEXPRESS;AttachDbFilename=H:\Work\НовосибирскЭнерго\" + name + @"\Piramida2000-" + name + @".MDF;Integrated Security=True;Connect Timeout=30;User Instance=True";

            try
            {
                connectionMSSQL.Open();
                result = true;
                string s;
                int pos;
                pos = connectionMSSQL.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = connectionMSSQL.ConnectionString;
                else
                    s = connectionMSSQL.ConnectionString.Substring(0, pos);
                MainForm.log.LogToFile("Соединение с базой установлено (" + s + ")", true, true, false);
            }
            catch (SqlException e)
            {
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = connectionMSSQL.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = connectionMSSQL.ConnectionString;
                else
                    s = connectionMSSQL.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("Ошибка открытия соединения", true, true, false);
                MainForm.log.LogToFile("Строка соединения " + s, false, false, false);
                MainForm.log.LogToFile("Ошибка " + e.Message, false, false, false);
                MainForm.log.LogToFile(e.ToString(), false, false, false);
                MainForm.log.LogUnlock();
            }
            catch
            {
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = connectionMSSQL.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = connectionMSSQL.ConnectionString;
                else
                    s = connectionMSSQL.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("Ошибка открытия соединения", true, true, false);
                MainForm.log.LogToFile("Строка соединения " + s, false, false, false);
                MainForm.log.LogUnlock();
            }

            return result;
        }

        private bool DisconnectMSSQL()
        {
            if (connectionMSSQL.State == ConnectionState.Closed)
                return true;

            bool result = false;

            try
            {
                connectionMSSQL.Close();
                result = true;
                string s;
                int pos;
                pos = connectionMSSQL.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = connectionMSSQL.ConnectionString;
                else
                    s = connectionMSSQL.ConnectionString.Substring(0, pos);
                MainForm.log.LogToFile("Соединение с базой разорвано (" + s + ")", true, true, false);
            }
            catch (SqlException e)
            {
                MainForm.log.LogLock();
                MainForm.log.LogToFile("Ошибка закрытия соединения", true, true, false);
                MainForm.log.LogToFile("Ошибка " + e.Message, false, false, false);
                MainForm.log.LogToFile(e.ToString(), false, false, false);
                MainForm.log.LogUnlock();
            }
            catch
            {
                MainForm.log.LogToFile("Ошибка закрытия соединения", true, true, false);
            }

            return result;
        }

        private bool GetDataMSSQL(DataTable table, string query)
        {
            if (connectionMSSQL.State != ConnectionState.Open)
                return false;
            bool result = false;

            commandMSSQL.CommandText = query;

            table.Reset();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;

            try
            {
                adapterMSSQL.Fill(table);
                result = true;
            }
            catch (SqlException e)
            {
                needReconnect = true;
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = adapterMSSQL.SelectCommand.Connection.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = adapterMSSQL.SelectCommand.Connection.ConnectionString;
                else
                    s = adapterMSSQL.SelectCommand.Connection.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("Ошибка получения данных", true, true, false);
                MainForm.log.LogToFile("Строка соединения " + s, false, false, false);
                MainForm.log.LogToFile("Запрос " + adapterMSSQL.SelectCommand.CommandText, false, false, false);
                MainForm.log.LogToFile("Ошибка " + e.Message, false, false, false);
                MainForm.log.LogToFile(e.ToString(), false, false, false);
                MainForm.log.LogUnlock();
            }
            catch
            {
                needReconnect = true;
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = adapterMSSQL.SelectCommand.Connection.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = adapterMSSQL.SelectCommand.Connection.ConnectionString;
                else
                    s = adapterMSSQL.SelectCommand.Connection.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("Ошибка получения данных", true, true, false);
                MainForm.log.LogToFile("Строка соединения " + s, false, false, false);
                MainForm.log.LogToFile("Запрос " + adapterMSSQL.SelectCommand.CommandText, false, false, false);
                MainForm.log.LogUnlock();
            }

            return result;
        }

        public static DataTable Request (ConnectionSettings connSett, string query) {
            MySqlConnection m_connectionMySQL;
            MySqlCommand m_commandMySQL;
            MySqlDataAdapter m_adapterMySQL;

            DataTable dataTableRes = new DataTable ();

            m_connectionMySQL = new MySqlConnection(connSett.GetConnectionStringMySQL());

            m_commandMySQL = new MySqlCommand();
            m_commandMySQL.Connection = m_connectionMySQL;
            m_commandMySQL.CommandType = CommandType.Text;

            m_adapterMySQL = new MySqlDataAdapter();
            m_adapterMySQL.SelectCommand = m_commandMySQL;

            m_commandMySQL.CommandText = query;

            dataTableRes.Reset();
            dataTableRes.Locale = System.Globalization.CultureInfo.InvariantCulture;

            m_connectionMySQL.Open();

            if (m_connectionMySQL.State == ConnectionState.Open)
            {
                try
                {
                    m_adapterMySQL.Fill(dataTableRes);
                }
                catch (MySqlException e)
                {
                }
            }
            else
                ; //

            m_connectionMySQL.Close();

            return dataTableRes;
        }
    }
}
