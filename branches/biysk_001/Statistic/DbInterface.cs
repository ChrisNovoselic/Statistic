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

        private struct DbInterfaceListener
        {
            public volatile bool listenerActive; 
            public volatile bool dataPresent;
            public volatile bool dataError;
            public volatile string requestDB;
            public volatile DataTable dataTable;
        }
        private DbInterfaceListener[] listeners;
        private int maxListeners;

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

        private ConnectionSettings connectionSettings;
        private bool needReconnect;
        private DbInterfaceType connectionType;
        private bool connected;

        public DbInterface(DbInterfaceType type, int maxListenersAllowed)
        {
            maxListeners = maxListenersAllowed;
            connectionType = type;
            lockListeners = new object();
            lockConnectionSettings = new object();
            listeners = new DbInterfaceListener[maxListeners];
            for (int i = 0; i < maxListeners; i++)
            {
                listeners[i] = new DbInterfaceListener();
                listeners[i].dataTable = new DataTable();
                listeners[i].listenerActive = false;
                listeners[i].dataPresent = false;
                listeners[i].dataError = false;
                listeners[i].requestDB = "";
            }

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
                for (int i = 0; i < maxListeners; i++)
                {
                    if (!listeners[i].listenerActive)
                    {
                        listeners[i].listenerActive = true;
                        listeners[i].dataPresent = false;
                        listeners[i].dataError = false; 
                        listeners[i].requestDB = "";
                        return i;
                    }
                }
            }

            return -1;
        }

        public void ListenerUnregister(int listenerId)
        {
            if (listenerId >= maxListeners || listenerId < 0)
                return;

            lock (lockListeners)
            {
                listeners[listenerId].listenerActive = false;
                listeners[listenerId].dataPresent = false;
                listeners[listenerId].dataError = false;
                listeners[listenerId].requestDB = "";
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
                for (int i = 0; i < maxListeners; i++)
                {
                    listeners[i].requestDB = "";
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
            if (listenerId >= maxListeners || listenerId < 0)
                return;

            lock (lockListeners)
            {
                listeners[listenerId].requestDB = request;
                listeners[listenerId].dataPresent = false;
                listeners[listenerId].dataError = false;
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
            bool bRes = true;

            if (listenerId >= maxListeners || listenerId < 0) {
                error = true;
                table = null;
                
                return false;
            }
            
            error = listeners[listenerId].dataError;
            table = listeners[listenerId].dataTable;
            
            return listeners[listenerId].dataPresent; 

            //return bRes;
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
            bool reconnection;

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

                for (int i = 0; i < maxListeners; i++)
                {
                    lock (lockListeners)
                    {
                        if (!listeners[i].listenerActive)
                            continue;
                        request = listeners[i].requestDB;
                        if (request == "")
                            continue;
                    }

                    try
                    {
                        result = GetData(listeners[i].dataTable, request);
                    }
                    catch
                    {
                        result = false;
                    }

                    lock (lockListeners)
                    {
                        if (!listeners[i].listenerActive)
                            continue;

                        if (request == listeners[i].requestDB)
                        {
                            if (result)
                            {
                                listeners[i].dataPresent = true;
                            }
                            else
                            {
                                listeners[i].dataError = true;
                            }
                            listeners[i].requestDB = "";
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
            if (connectionMySQL.State == ConnectionState.Open)
                return true;
            if (connectionMySQL.State != ConnectionState.Closed)
                return false;

            bool result = false;

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
                MainForm.log.LogToFile("Соединение с базой установлено (" + s + ")", true, true, true);

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
                MainForm.log.LogToFile("Соединение с базой разорвано (" + s + ")", true, true, true);

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
                MainForm.log.LogToFile("Ошибка закрытия соединения", true, true, true);
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
                MainForm.log.LogToFile("Соединение с базой установлено (" + s + ")", true, true, true);
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
                MainForm.log.LogToFile("Соединение с базой разорвано (" + s + ")", true, true, true);
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
                MainForm.log.LogToFile("Ошибка закрытия соединения", true, true, true);
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
    }
}
