using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace Statistic
{
    public class DbInterface
    {
        public enum DbInterfaceType
        {
            MySQL,
            MSSQL,
            MSExcel
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

        private DbConnection m_dbConnection;
        private DbCommand m_dbCommand;
        private DbDataAdapter m_dbAdapter;

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

            switch (connectionType)
            {
                case DbInterfaceType.MySQL:
                    m_dbConnection = new MySqlConnection();

                    m_dbCommand = new MySqlCommand();
                    m_dbCommand.Connection = m_dbConnection;
                    m_dbCommand.CommandType = CommandType.Text;

                    m_dbAdapter = new MySqlDataAdapter();
                    m_dbAdapter.SelectCommand = m_dbCommand;
                    break;
                case DbInterfaceType.MSSQL:
                    m_dbConnection = new SqlConnection();

                    m_dbCommand = new SqlCommand();
                    m_dbCommand.Connection = m_dbConnection;
                    m_dbCommand.CommandType = CommandType.Text;

                    m_dbAdapter = new SqlDataAdapter();
                    m_dbAdapter.SelectCommand = m_dbCommand;
                    break;
                case DbInterfaceType.MSExcel:
                    m_dbConnection = new OleDbConnection();

                    m_dbCommand = new OleDbCommand();
                    m_dbCommand.Connection = m_dbConnection;
                    m_dbCommand.CommandType = CommandType.Text;

                    m_dbAdapter = new SqlDataAdapter();
                    m_dbAdapter.SelectCommand = m_dbCommand;
                    break;
                default:
                    break;
            }

            dbThread = new Thread(new ParameterizedThreadStart(DbInterface_ThreadFunction));
            dbThread.Name = "��������� ��";
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
            lock (lockConnectionSettings) // ��������� �������� ����������� � ����������� ����� ��� ��������������� - ��������� ��������
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
                MainForm.log.LogLock();
                MainForm.log.LogToFile("���������� ��������� � ���������� (sem.Release ())", true, true, false);
                MainForm.log.LogToFile("���������� " + e.Message, false, false, false);
                MainForm.log.LogToFile(e.ToString(), false, false, false);
                MainForm.log.LogUnlock();
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
                
                lock (lockConnectionSettings) // �������� ����� � ��������� ����, ����� ��� ������������ ����� �������� �� �������� �������� ������������ ����
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
                        needReconnect = true; // ���������� ���� ����� ��� ����������
                }

                if (!connected) // �� ������� ������������ - �� �������� �������� ������
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
                MainForm.log.LogLock();
                MainForm.log.LogToFile("���������� ��������� � ����������", true, true, false);
                MainForm.log.LogToFile("���������� " + e.Message, false, false, false);
                MainForm.log.LogToFile(e.ToString(), false, false, false);
                MainForm.log.LogUnlock();
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
                if (needReconnect) // ���� ����� �������� � ������ ����� �������� ���� �������� ���������, �� ����������� �� ������� ����������� �� ������
                    return false;
                if (connectionSettings.ignore)
                    return false;
                m_dbConnection.ConnectionString = connectionSettings.GetConnectionStringMySQL();
            }
            //m_dbConnection.ConnectionString = @"Server=localhost;Database=Statistic;User id=root;Password=;";

            try
            {
                m_dbConnection.Open();
                result = true;
                string s;
                int pos;
                pos = m_dbConnection.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = m_dbConnection.ConnectionString;
                else
                    s = m_dbConnection.ConnectionString.Substring(0, pos);

                MainForm.log.LogLock();
                MainForm.log.LogToFile("���������� � ����� ����������� (" + s + ")", true, true, false);
                MainForm.log.LogUnlock();
            }
            catch (MySqlException e)
            {
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = m_dbConnection.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = m_dbConnection.ConnectionString;
                else
                    s = m_dbConnection.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("������ �������� ����������", true, true, false);
                MainForm.log.LogToFile("������ ���������� " + s, false, false, false);
                MainForm.log.LogToFile("������ " + e.Message, false, false, false);
                MainForm.log.LogToFile(e.ToString(), false, false, false);
                MainForm.log.LogUnlock();
            }
            catch
            {
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = m_dbConnection.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = m_dbConnection.ConnectionString;
                else
                    s = m_dbConnection.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("������ �������� ����������", true, true, false);
                MainForm.log.LogToFile("������ ���������� " + s, false, false, false);
                MainForm.log.LogUnlock();
            }

            return result;
        }

        private bool DisconnectMySQL()
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
                pos = m_dbConnection.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = m_dbConnection.ConnectionString;
                else
                    s = m_dbConnection.ConnectionString.Substring(0, pos);
                MainForm.log.LogToFile("���������� � ����� ��������� (" + s + ")", true, true, false);

            }
            catch (MySqlException e)
            {
                MainForm.log.LogLock();
                MainForm.log.LogToFile("������ �������� ����������", true, true, false);
                MainForm.log.LogToFile("������ " + e.Message, false, false, false);
                MainForm.log.LogToFile(e.ToString(), false, false, false);
                MainForm.log.LogUnlock();
            }
            catch
            {
                MainForm.log.LogToFile("������ �������� ����������", true, true, false);
            }
            
            return result;
        }

        private bool GetDataMySQL(DataTable table, string query)
        {
            if (m_dbConnection.State != ConnectionState.Open)
                return false;
            bool result = false;

            m_dbCommand.CommandText = query;

            table.Reset();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;

            try
            {
                m_dbAdapter.Fill(table);
                result = true;
            }
            catch (MySqlException e)
            {
                needReconnect = true;
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = m_dbAdapter.SelectCommand.Connection.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = m_dbAdapter.SelectCommand.Connection.ConnectionString;
                else
                    s = m_dbAdapter.SelectCommand.Connection.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("������ ��������� ������", true, true, false);
                MainForm.log.LogToFile("������ ���������� " + s, false, false, false);
                MainForm.log.LogToFile("������ " + m_dbAdapter.SelectCommand.CommandText, false, false, false);
                MainForm.log.LogToFile("������ " + e.Message, false, false, false);
                MainForm.log.LogToFile(e.ToString(), false, false, false);
                MainForm.log.LogUnlock();
            }
            catch
            {
                needReconnect = true;
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = m_dbAdapter.SelectCommand.Connection.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = m_dbAdapter.SelectCommand.Connection.ConnectionString;
                else
                    s = m_dbAdapter.SelectCommand.Connection.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("������ ��������� ������", true, true, false);
                MainForm.log.LogToFile("������ ���������� " + s, false, false, false);
                MainForm.log.LogToFile("������ " + m_dbAdapter.SelectCommand.CommandText, false, false, false);
                MainForm.log.LogUnlock();
            }

            return result;
        }

        private bool ConnectMSSQL()
        {
            if (m_dbConnection.State == ConnectionState.Open)
                return true;
            if (m_dbConnection.State != ConnectionState.Closed)
                return false;

            bool result = false;

            lock (lockConnectionSettings)
            {
                if (needReconnect) // ���� ����� �������� � ������ ����� �������� ���� �������� ���������, �� ����������� �� ������� ����������� �� ������
                    return false;
                m_dbConnection.ConnectionString = connectionSettings.GetConnectionStringMSSQL();
            }
            //m_dbConnection.ConnectionString = @"Data Source=.\SQLEXPRESS;AttachDbFilename=H:\Work\�����������������\" + name + @"\Piramida2000-" + name + @".MDF;Integrated Security=True;Connect Timeout=30;User Instance=True";

            try
            {
                m_dbConnection.Open();
                result = true;
                string s;
                int pos;
                pos = m_dbConnection.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = m_dbConnection.ConnectionString;
                else
                    s = m_dbConnection.ConnectionString.Substring(0, pos);
                MainForm.log.LogToFile("���������� � ����� ����������� (" + s + ")", true, true, false);
            }
            catch (SqlException e)
            {
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = m_dbConnection.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = m_dbConnection.ConnectionString;
                else
                    s = m_dbConnection.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("������ �������� ����������", true, true, false);
                MainForm.log.LogToFile("������ ���������� " + s, false, false, false);
                MainForm.log.LogToFile("������ " + e.Message, false, false, false);
                MainForm.log.LogToFile(e.ToString(), false, false, false);
                MainForm.log.LogUnlock();
            }
            catch
            {
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = m_dbConnection.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = m_dbConnection.ConnectionString;
                else
                    s = m_dbConnection.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("������ �������� ����������", true, true, false);
                MainForm.log.LogToFile("������ ���������� " + s, false, false, false);
                MainForm.log.LogUnlock();
            }

            return result;
        }

        private bool DisconnectMSSQL()
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
                pos = m_dbConnection.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = m_dbConnection.ConnectionString;
                else
                    s = m_dbConnection.ConnectionString.Substring(0, pos);
                MainForm.log.LogToFile("���������� � ����� ��������� (" + s + ")", true, true, false);
            }
            catch (SqlException e)
            {
                MainForm.log.LogLock();
                MainForm.log.LogToFile("������ �������� ����������", true, true, false);
                MainForm.log.LogToFile("������ " + e.Message, false, false, false);
                MainForm.log.LogToFile(e.ToString(), false, false, false);
                MainForm.log.LogUnlock();
            }
            catch
            {
                MainForm.log.LogToFile("������ �������� ����������", true, true, false);
            }

            return result;
        }

        private bool GetDataMSSQL(DataTable table, string query)
        {
            if (m_dbConnection.State != ConnectionState.Open)
                return false;
            bool result = false;

            m_dbCommand.CommandText = query;

            table.Reset();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;

            try
            {
                m_dbAdapter.Fill(table);
                result = true;
            }
            catch (SqlException e)
            {
                needReconnect = true;
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = m_dbAdapter.SelectCommand.Connection.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = m_dbAdapter.SelectCommand.Connection.ConnectionString;
                else
                    s = m_dbAdapter.SelectCommand.Connection.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("������ ��������� ������", true, true, false);
                MainForm.log.LogToFile("������ ���������� " + s, false, false, false);
                MainForm.log.LogToFile("������ " + m_dbAdapter.SelectCommand.CommandText, false, false, false);
                MainForm.log.LogToFile("������ " + e.Message, false, false, false);
                MainForm.log.LogToFile(e.ToString(), false, false, false);
                MainForm.log.LogUnlock();
            }
            catch
            {
                needReconnect = true;
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = m_dbAdapter.SelectCommand.Connection.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = m_dbAdapter.SelectCommand.Connection.ConnectionString;
                else
                    s = m_dbAdapter.SelectCommand.Connection.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("������ ��������� ������", true, true, false);
                MainForm.log.LogToFile("������ ���������� " + s, false, false, false);
                MainForm.log.LogToFile("������ " + m_dbAdapter.SelectCommand.CommandText, false, false, false);
                MainForm.log.LogUnlock();
            }

            return result;
        }

        public static DataTable Request (string path, string query) {
            DataTable dataTableRes = new DataTable();

            OleDbConnection connectionExcel;
            System.Data.OleDb.OleDbCommand commandExcel;
            System.Data.OleDb.OleDbDataAdapter adapterExcel;

            connectionExcel = new OleDbConnection (ConnectionSettings.GetConnectionStringExcel (path));

            commandExcel = new OleDbCommand();
            commandExcel.Connection = connectionExcel;
            commandExcel.CommandType = CommandType.Text;

            adapterExcel = new OleDbDataAdapter();
            adapterExcel.SelectCommand = commandExcel;

            commandExcel.CommandText = query;

            dataTableRes.Reset();
            dataTableRes.Locale = System.Globalization.CultureInfo.InvariantCulture;

            try
            {
                connectionExcel.Open();

                if (connectionExcel.State == ConnectionState.Open)
                {
                    adapterExcel.Fill(dataTableRes);
                }
                else
                    ; //
            }
            catch (OleDbException e)
            {
                MainForm.log.LogLock();
                string s;
                int pos;
                pos = connectionExcel.ConnectionString.IndexOf("Password");
                if (pos < 0)
                    s = connectionExcel.ConnectionString;
                else
                    s = connectionExcel.ConnectionString.Substring(0, pos);

                MainForm.log.LogToFile("������ �������� ����������", true, true, false);
                MainForm.log.LogToFile("������ ���������� " + s, false, false, false);
                MainForm.log.LogUnlock();
            }

            connectionExcel.Close();

            return dataTableRes;
        }

        public static DataTable Request (ConnectionSettings connSett, string query) {
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

                MainForm.log.LogToFile("������ �������� ����������", true, true, false);
                MainForm.log.LogToFile("������ ���������� " + s, false, false, false);
                MainForm.log.LogUnlock();
            }

            connectionMySQL.Close();

            return dataTableRes;
        }
    }
}
