using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;     
using System.Threading;

namespace Statistic
{
    public class TEC
    {
        public enum TEC_TYPE { COMMON, BIYSK };

        public string name;
        public List<GTP> GTP;

        public ConnectionSettings connSett;

        private bool is_connection_error;
        private bool is_data_error;

        private int used;

        //Для особенной ТЭЦ (Бийск)
        public SqlConnection connection;
        public SqlCommand command;
        public SqlDataAdapter adapter;

        private object lockValue;

        private Thread dbThread;
        private Semaphore sema;
        private volatile bool workTread;

        private DbInterface dbInterface;
        private int listenerId;

        public volatile DbDataInterface dataInterface;
        public volatile DbDataInterface dataInterfaceAdmin;

        public TEC (string name) {
            this.name = name;

            GTP = new List<GTP>();

            is_data_error = is_connection_error = false;

            used = 0;

            if (type () == TEC_TYPE.BIYSK) {
                connection = new SqlConnection();

                command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                adapter = new SqlDataAdapter();
                adapter.SelectCommand = command;

                lockValue = new object();

                sema = new Semaphore(1, 1);
                dbThread = new Thread(new ParameterizedThreadStart(dbThread_Function));

                dataInterface = new DbDataInterface();
                dataInterfaceAdmin = new DbDataInterface();
            }
            else
                ;
        }

        public void ClearFlags(int gtp)
        {
            if (gtp < 0)
            {
                lock (dataInterface.lockData)
                {
                    dataInterface.dataPresent = false;
                    dataInterface.dataError = false;
                }
            }
            else
            {
                lock (GTP[gtp].dataInterface.lockData)
                {
                    GTP[gtp].dataInterface.dataPresent = false;
                    GTP[gtp].dataInterface.dataError = false;
                }
            }
        }

        public void Request(string request)
        {
            dbInterface.Request(listenerId, request);
        }

        public void Request(string request, int gtp)
        {
            if (gtp < 0)
            {
                lock (dataInterface.lockData)
                {
                    dataInterface.request[1] = request;
                    dataInterface.dataPresent = false;
                    dataInterface.dataError = false;
                }
            }
            else
            {
                lock (GTP[gtp].dataInterface.lockData)
                {
                    GTP[gtp].dataInterface.request[1] = request;
                    GTP[gtp].dataInterface.dataPresent = false;
                    GTP[gtp].dataInterface.dataError = false;
                }
            }
            try
            {
                sema.Release(1);
            }
            catch
            {
            }
        }

        public bool GetResponse(out bool error, out DataTable table)
        {
            return dbInterface.GetResponse(listenerId, out error, out table);
        }

        public void StartDbInterface()
        {
            if (used == 0)
            {
                switch (type ()) {
                    case TEC_TYPE.COMMON:
                        dbInterface = new DbInterface(DbInterface.DbInterfaceType.MSSQL, 1);
                        listenerId = dbInterface.ListenerRegister();
                        dbInterface.Start();
                        dbInterface.SetConnectionSettings(connSett);
                        break;
                    case TEC_TYPE.BIYSK:
                        lock (dataInterface.lockData)
                        {
                            dataInterface.dataPresent = false;
                            dataInterface.request[1] = "";
                        }
                        foreach (GTP g in GTP)
                        {
                            lock (g.dataInterface.lockData)
                            {
                                g.dataInterface.dataPresent = false;
                                g.dataInterface.request[1] = "";
                            }
                        }
                        workTread = true;
                        sema.WaitOne();
                        dbThread = new Thread(new ParameterizedThreadStart(dbThread_Function));
                        dbThread.Name = this.name;
                        dbThread.IsBackground = true;
                        dbThread.Start(this);
                        break;
                    default:
                        break;
                }
            }
            used++;
            if (used > GTP.Count)
                used = GTP.Count;
        }

        public void StopDbInterface()
        {
            used--;

            switch (type ()) {
                case TEC_TYPE.COMMON:
                    
                    if (used == 0)
                    {
                        dbInterface.Stop();
                        dbInterface.ListenerUnregister(listenerId);
                    }
                    else
                        ;
                    break;
                case TEC_TYPE.BIYSK:
                    if (used == 0)
                    {
                        bool joined;
                        workTread = false;
                        lock (dataInterface.lockData)
                        {
                            dataInterface.request[1] = "";
                        }
                        foreach (GTP g in GTP)
                        {
                            lock (g.dataInterface.lockData)
                            {
                                g.dataInterface.request[1] = "";
                            }
                        }
                        if (dbThread.IsAlive)
                        {
                            try
                            {
                                sema.Release(1);
                            }
                            catch
                            { 
                            }
                    
                            joined = dbThread.Join(5000);
                            if (!joined)
                                dbThread.Abort();
                            else
                                ;
                        }
                    }
                    else
                        ;
                    break;
                default:
                    break;
            }
            if (used < 0)
                used = 0;
            else
                ;
        }

        public void StopDbInterfaceForce()
        {
            bool joined = false;

            switch (type ()) {
                case TEC_TYPE.COMMON:
                    if (used > 0)
                    {
                        dbInterface.Stop();
                        dbInterface.ListenerUnregister(listenerId);
                    }
                    else
                        ;
                    break;
                case TEC_TYPE.BIYSK:
                    workTread = false;
                    lock (dataInterface.lockData)
                    {
                        dataInterface.request[1] = "";
                    }
                    foreach (GTP g in GTP)
                    {
                        lock (g.dataInterface.lockData)
                        {
                            g.dataInterface.request[1] = "";
                        }
                    }
                    if (dbThread.IsAlive)
                    {
                        try
                        {
                            sema.Release(1);
                        }
                        catch
                        { 
                        }

                        joined = dbThread.Join(5000);
                        if (!joined)
                            dbThread.Abort();
                    }
                    break;
                default:
                    break;
            }
        }

        public void RefreshConnectionSettings()
        {
            if (used > 0)
            {
                dbInterface.SetConnectionSettings(connSett);
            }
        }

        //Для особенной ТЭЦ (Бийск)
        public TEC_TYPE type() {
            if (name.IndexOf ("Бийск") > -1)
                return TEC_TYPE.BIYSK;
            else
                return TEC_TYPE.COMMON;
        }

        //Для особенной ТЭЦ (Бийск)
        private void dbThread_Function(object data)
        {
            TEC t = (TEC)data;
            bool result;
            while (t.workTread)
            {
                t.sema.WaitOne();

                lock (t.dataInterface.lockData)
                {
                    t.dataInterface.request[0] = t.dataInterface.request[1];
                }

                if (t.dataInterface.request[0] != "")
                {
                    try
                    {
                        result = t.GetData(t.dataInterface.tableData, t.dataInterface.request[0]);
                    }
                    catch
                    {
                        result = false;
                    }

                    lock (t.dataInterface.lockData)
                    {
                        if (t.dataInterface.request[0] == t.dataInterface.request[1])
                        {
                            if (result)
                            {
                                t.dataInterface.dataPresent = true;
                            }
                            else
                            {
                                t.dataInterface.dataError = true;
                            }
                            t.dataInterface.request[1] = "";
                        }
                    }
                }

                if (t.GTP.Count > 1)
                {
                    foreach (GTP g in t.GTP)
                    {
                        lock (g.dataInterface.lockData)
                        {
                            g.dataInterface.request[0] = g.dataInterface.request[1];
                        }

                        if (g.dataInterface.request[0] != "")
                        {
                            try
                            {
                                result = t.GetData(g.dataInterface.tableData, g.dataInterface.request[0]);
                            }
                            catch
                            {
                                result = false;
                            }

                            lock (g.dataInterface.lockData)
                            {
                                if (g.dataInterface.request[0] == g.dataInterface.request[1])
                                {
                                    if (result)
                                    {
                                        g.dataInterface.dataPresent = true;
                                    }
                                    else
                                    {
                                        g.dataInterface.dataError = true;
                                    }
                                    g.dataInterface.request[1] = "";
                                }
                            }
                        }
                    }
                }
            }
            try
            {
                t.sema.Release(1);
            }
            catch
            {
            }
        }

        //Для особенной ТЭЦ (Бийск)
        public bool GetData(DataTable table, string query)
        {
            bool result = true;

            if (!Connect())
                return false;

            if (connection.State == ConnectionState.Open)
            {
                lock (lockValue)
                {
                    command.CommandText = query;

                    table.Reset();
                    table.Locale = System.Globalization.CultureInfo.InvariantCulture;

                    try
                    {
                        adapter.Fill(table);
                        is_data_error = false;
                    }
                    catch (SqlException e)
                    {
                        MainForm.log.LogLock();
                        string s;
                        int pos;
                        pos = adapter.SelectCommand.Connection.ConnectionString.IndexOf("Password");
                        if (pos < 0)
                            s = adapter.SelectCommand.Connection.ConnectionString;
                        else
                            s = adapter.SelectCommand.Connection.ConnectionString.Substring(0, pos);

                        MainForm.log.LogToFile("Ошибка получения данных: " + e.Message, true, true, false);
                        MainForm.log.LogToFile("Строка соединения " + s, false, false, false);
                        MainForm.log.LogToFile("Запрос " + adapter.SelectCommand.CommandText, false, false, false);
                        MainForm.log.LogToFile("Код ошибки " + e.ErrorCode.ToString(), false, false, false);
                        for (int i = 0; i < e.Errors.Count; i++) MainForm.log.LogToFile(e.Errors[i].ToString(), false, false, false);
                        MainForm.log.LogToFile(e.ToString(), false, false, false);
                        MainForm.log.LogUnlock();

                        //if (!is_data_error)
                        //    MessageBox.Show("Ошибка получения мониторинговых данных " + name + ".\nПроверьте правильность настроек и состояние сети.\n\nДанные об ошибке сохранены в файл logDB.txt", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        is_data_error = true;
                        result = false;
                    }
                }
            }

            return result;
        }

        public bool Connect()
        {
            if (connSett.ignore)
                return false;

            bool result = true;
            string connectionStringNew = connSett.GetConnectionStringMSSQL();
            string connectionStringOld;

            if (connectionStringNew != connection.ConnectionString)
            {
                if (!Disconnect())
                    return false;

                lock (lockValue)
                {
                    connectionStringOld = connection.ConnectionString;
                    connection.ConnectionString = connectionStringNew;

                    try
                    {
                        connection.Open();
                        is_connection_error = false;
                    }
                    catch (SqlException e)
                    {
                        MainForm.log.LogLock();
                        string s;
                        int pos;
                        pos = connectionStringNew.IndexOf("Password");
                        if (pos < 0)
                            s = connectionStringNew;
                        else
                            s = connectionStringNew.Substring(0, pos);

                        MainForm.log.LogToFile("Ошибка открытия соединения", true, true, false);
                        MainForm.log.LogToFile("Строка соединения " + s, false, false, false);
                        MainForm.log.LogToFile("Код ошибки " + e.ErrorCode.ToString(), false, false, false);
                        for (int i = 0; i < e.Errors.Count; i++) MainForm.log.LogToFile(e.Errors[i].ToString(), false, false, false);
                        MainForm.log.LogToFile(e.ToString(), false, false, false);
                        MainForm.log.LogUnlock();

                        connection.ConnectionString = connectionStringOld;
                        result = false;
                        is_connection_error = true;
                    }
                }
            }
            return result;
        }

        public bool Disconnect()
        {
            bool result = true;

            if (connection.State == ConnectionState.Open)
            {
                lock (lockValue)
                {
                    try
                    {
                        connection.Close();
                        connection.ConnectionString = "";
                    }
                    catch (SqlException e)
                    {
                        MainForm.log.LogLock();
                        MainForm.log.LogToFile("Ошибка закрытия соединения: " + e.Message, true, true, false);
                        MainForm.log.LogToFile("Код ошибки " + e.ErrorCode.ToString(), false, false, false);
                        for (int i = 0; i < e.Errors.Count; i++) MainForm.log.LogToFile(e.Errors[i].ToString(), false, false, false);
                        MainForm.log.LogToFile(e.ToString(), false, false, false);
                        MainForm.log.LogUnlock();

                        //MessageBox.Show("Не удаётся закрыть соединение с базой мониторинговых данных " + name + ".\n\nДанные об ошибке сохранены в файл logDB.txt", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        result = false;
                    }
                }
                is_data_error = is_connection_error = false;
            }
            return result;
        }
    }
}
