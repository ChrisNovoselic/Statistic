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
    public class DbTSQLInterface : DbInterface
    {
        public enum DB_TSQL_INTERFACE_TYPE
        {
            MySQL,
            MSSQL,
            MSExcel
        }

        public enum QUERY_TYPE { UPDATE, INSERT, DELETE, COUNT_QUERY_TYPE };

        private DbConnection m_dbConnection;
        private DbCommand m_dbCommand;
        private DbDataAdapter m_dbAdapter;

        private DB_TSQL_INTERFACE_TYPE m_connectionType;

        public DbTSQLInterface(DB_TSQL_INTERFACE_TYPE type, string name)
            : base(name)
        {
            m_connectionType = type;

            m_connectionSettings = new ConnectionSettings();

            switch (m_connectionType)
            {
                case DB_TSQL_INTERFACE_TYPE.MySQL:
                    m_dbConnection = new MySqlConnection();

                    m_dbCommand = new MySqlCommand();
                    m_dbCommand.Connection = m_dbConnection;
                    m_dbCommand.CommandType = CommandType.Text;

                    m_dbAdapter = new MySqlDataAdapter();
                    break;
                case DB_TSQL_INTERFACE_TYPE.MSSQL:
                    m_dbConnection = new SqlConnection();

                    m_dbCommand = new SqlCommand();
                    m_dbCommand.Connection = m_dbConnection;
                    m_dbCommand.CommandType = CommandType.Text;

                    m_dbAdapter = new SqlDataAdapter();
                    break;
                case DB_TSQL_INTERFACE_TYPE.MSExcel:
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
        }

        protected override bool Connect()
        {
            if (((ConnectionSettings)m_connectionSettings).Validate() != ConnectionSettings.ConnectionSettingsError.NoError)
                return false;
            else
                ;

            bool result = false, bRes = false;

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
                string msg = string.Empty;
                msg += "!Исключение! обращения к переменной" + Environment.NewLine;
                msg += "Исключение " + e.Message + Environment.NewLine;
                msg += e.ToString();
                Logging.Logg().LogToFile(msg, true, true, true);
            }

            if (! (m_dbConnection.State == ConnectionState.Closed))
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

                if (((ConnectionSettings)m_connectionSettings).ignore)
                    return false;
                else
                    ;

                //string connStr = string.Empty;
                switch (m_connectionType) {
                    case DB_TSQL_INTERFACE_TYPE.MSSQL:
                        //connStr = connectionSettings.GetConnectionStringMSSQL();
                        ((SqlConnection)m_dbConnection).ConnectionString = ((ConnectionSettings)m_connectionSettings).GetConnectionStringMSSQL();
                        break;
                    case DB_TSQL_INTERFACE_TYPE.MySQL:
                        //connStr = connectionSettings.GetConnectionStringMySQL();
                        ((MySqlConnection)m_dbConnection).ConnectionString = ((ConnectionSettings)m_connectionSettings).GetConnectionStringMySQL();
                        break;
                    case DB_TSQL_INTERFACE_TYPE.MSExcel:
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

                Logging.Logg().LogToFile("Соединение с базой установлено (" + s + ")", true, true, true);
            }
            catch (Exception e)
            {
                logging_catch_db(m_dbConnection, e);
            }

            return result;
        }

        public override void SetConnectionSettings(object cs)
        {
            lock (lockConnectionSettings)
            {
                ((ConnectionSettings)m_connectionSettings).server = ((ConnectionSettings)cs).server;
                ((ConnectionSettings)m_connectionSettings).port = ((ConnectionSettings)cs).port;
                ((ConnectionSettings)m_connectionSettings).dbName = ((ConnectionSettings)cs).dbName;
                ((ConnectionSettings)m_connectionSettings).userName = ((ConnectionSettings)cs).userName;
                ((ConnectionSettings)m_connectionSettings).password = ((ConnectionSettings)cs).password;
                ((ConnectionSettings)m_connectionSettings).ignore = ((ConnectionSettings)cs).ignore;

                needReconnect = true;
            }

            //base.SetConnectionSettings (cs); //базовой function 'cs' не нужен
            SetConnectionSettings();
        }

        protected override bool Disconnect()
        {
            if (m_dbConnection.State == ConnectionState.Closed)
                return true; 
            else
                ;

            bool result = false;

            try
            {
                //lock (lockListeners)
                //{
                //    m_listListeners.Clear ();
                //}
                
                m_dbConnection.Close();
                result = true;
                string s;
                int pos;
                pos = m_dbConnection.ConnectionString.IndexOf("Password", StringComparison.CurrentCultureIgnoreCase);
                if (pos < 0)
                    s = m_dbConnection.ConnectionString;
                else
                    s = m_dbConnection.ConnectionString.Substring(0, pos);
                Logging.Logg().LogToFile("Соединение с базой разорвано (" + s + ")", true, true, true);

            }
            catch (Exception e)
            {
                logging_catch_db (m_dbConnection, e);
            }
            
            return result;
        }

        protected override bool GetData(DataTable table, object query)
        {
            if (m_dbConnection.State != ConnectionState.Open)
                return false;
            else
                ;

            bool result = false;

            try { m_dbCommand.CommandText = query.ToString (); }
            catch (Exception e)
            {
                Console.Write (e.Message);
            }

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
                string msg = string.Empty, s;
                int pos;
                pos = m_dbAdapter.SelectCommand.Connection.ConnectionString.IndexOf("Password", StringComparison.CurrentCultureIgnoreCase);
                if (pos < 0)
                    s = m_dbAdapter.SelectCommand.Connection.ConnectionString;
                else
                    s = m_dbAdapter.SelectCommand.Connection.ConnectionString.Substring(0, pos);

                msg += "Ошибка получения данных" + Environment.NewLine;
                msg += "Строка соединения " + s + Environment.NewLine;
                msg += "Запрос " + Environment.NewLine;
                msg += "Ошибка " + e.Message + Environment.NewLine;
                msg += e.ToString();

                Logging.Logg().LogToFile(msg, true, true, true);
            }
            catch (Exception e)
            {
                needReconnect = true;
                logging_catch_db (m_dbConnection, e);
            }

            return result;
        }

        private static void logging_catch_db(DbConnection conn, Exception e)
        {
            string msg = string.Empty, s;
            int pos;
            pos = conn.ConnectionString.IndexOf("Password", StringComparison.CurrentCultureIgnoreCase);
            if (pos < 0)
                s = conn.ConnectionString;
            else
                s = conn.ConnectionString.Substring(0, pos);

            msg += "Обработка исключения при работе с БД" + Environment.NewLine;
            msg += "Строка соединения: " + s + Environment.NewLine;
            if (!(e == null))
            {
                msg += "Ошибка: " + e.Message + Environment.NewLine;
                msg += e.ToString();
            }
            else
                ;

            Logging.Logg().LogToFile(msg, true, true, true);
        }

        public static string valueForQuery (DataTable table, int row, int col) {
            string strRes = string.Empty;
            bool bQuote = false;

            if (table.Columns[col].DataType.IsPrimitive == true)
            //if (table.Columns[col].DataType.IsByRef == false)
                bQuote = false;
            else
                bQuote = true;

            strRes = (bQuote ? "'" : string.Empty) + (table.Rows[row][col].ToString ().Length > 0 ? table.Rows[row][col] : "NULL") + (bQuote ? "'" : string.Empty);

            return strRes;
        }

        public static DataTable Select (string path, string query, out int er) {
            er = 0;

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
                    logging_catch_db(connectionOleDB, e);

                    er = -1;
                }

                connectionOleDB.Close();
            }
            else
                ;

            return dataTableRes;
        }

        public static DataTable Select(MySqlConnection connectionMySQL, string query, DbType [] types, object [] parametrs, out int er)
        {
            er = 0;

            DataTable dataTableRes = new DataTable();

            ParametrsValidate (types, parametrs, out er);

            if (er == 0)
            {
                MySqlCommand commandMySQL;
                MySqlDataAdapter adapterMySQL;

                commandMySQL = new MySqlCommand();
                commandMySQL.Connection = connectionMySQL;
                commandMySQL.CommandType = CommandType.Text;

                adapterMySQL = new MySqlDataAdapter();
                adapterMySQL.SelectCommand = commandMySQL;

                commandMySQL.CommandText = query;
                ParametrsAdd(commandMySQL, types, parametrs);

                dataTableRes.Reset();
                dataTableRes.Locale = System.Globalization.CultureInfo.InvariantCulture;

                try
                {
                    if (connectionMySQL.State == ConnectionState.Open)
                    {
                        adapterMySQL.Fill(dataTableRes);
                    }
                    else
                        er = -1; //
                }
                catch (Exception e)
                {
                    logging_catch_db(connectionMySQL, e);

                    er = -1;
                }
            }
            else {
                // Логгирование в 'ParametrsValidate'
            }

            return dataTableRes;
        }

        public static DataTable Select(ConnectionSettings connSett, string query, out int er)
        {
            er = 0;

            DataTable dataTableRes = null;
            MySqlConnection connectionMySQL;
            connectionMySQL = new MySqlConnection(connSett.GetConnectionStringMySQL());

            try {
                connectionMySQL.Open();
            }
            catch (Exception e)
            {
                logging_catch_db(connectionMySQL, e);

                er = -1;
            }

            if (er == 0)
            {
                dataTableRes = Select(connectionMySQL, query, null, null, out er);

                connectionMySQL.Close();
            }
            else
                dataTableRes = new DataTable();

            return dataTableRes;
        }

        private static void ParametrsAdd(MySqlCommand commandMySQL, DbType[] types, object[] parametrs)
        {
            if ((!(types == null)) && (!(parametrs == null)))
            {
                int i = -1;
                //commandMySQL.Parameters.Clear ();

                //foreach (DbType type in types)
                for (i = 0; i < types.Length; i ++)
                {
                    //Вариант №1
                    //commandMySQL.Parameters.Add("", type);
                    //commandMySQL.Parameters[commandMySQL.Parameters.Count - 1] = (MySqlParameter)parametrs[commandMySQL.Parameters.Count - 1];

                    //Вариант №2
                    //commandMySQL.Parameters.AddWithValue(string.Empty, parametrs[commandMySQL.Parameters.Count]);
                    commandMySQL.Parameters.Add(@"" + i.ToString(), parametrs[i]);
                    //commandMySQL.Parameters.Add(@"", parametrs[i]);

                    //Вариант №3
                    //commandMySQL.Parameters.Add(parametrs[commandMySQL.Parameters.Count]);

                    //Вариант №4
                    //commandMySQL.Parameters.Add(@"DateTime", type).Value = parametrs[/*commandMySQL.Parameters.Count*/0];
                    //commandMySQL.Parameters.Add(@"" + i.ToString (), types[i]).Value = parametrs[i];
                }
            }
            else
                ;
        }

        private static void ParametrsValidate (DbType[] types, object[] parametrs, out int err)
        {
            err = 0;

            if ((types == null) && (parametrs == null))
                ; //Проверка не нужна
            else
                //if ((!(types == null)) || (!(parametrs == null)))
                //    err = -1;
                //else
                    if ((!(types == null)) && (!(parametrs == null)))
                    {
                        if (!(types.Length == parametrs.Length))
                        {
                            err = -1;
                        }
                        else
                            ;
                    }
                    else
                        ;

            if (!(err == 0))
            {
                Logging.Logg().LogToFile("Ошибка! static DbTSQLInterface::ParametrsValidate () - types OR parametrs не корректны", true, true, true);
            }
            else
                ;
        }

        public static void ExecNonQuery(MySqlConnection connectionMySQL, string query, DbType[] types, object[] parametrs, out int er)
        {
            er = 0;

            MySqlCommand commandMySQL;

            ParametrsValidate (types, parametrs, out er);

            if (er == 0)
            {
                commandMySQL = new MySqlCommand();
                commandMySQL.Connection = connectionMySQL;
                commandMySQL.CommandType = CommandType.Text;

                commandMySQL.CommandText = query;
                ParametrsAdd(commandMySQL, types, parametrs);

                try
                {
                    if (connectionMySQL.State == ConnectionState.Open)
                    {
                        commandMySQL.ExecuteNonQuery();
                    }
                    else
                        er = -1; //
                }
                catch (Exception e)
                {
                    logging_catch_db(connectionMySQL, e);

                    er = -1;
                }
            }
            else
                ;
        }

        public static void ExecNonQuery(ConnectionSettings connSett, string query, out int er)
        {
            er = 0;

            MySqlConnection connectionMySQL;

            connectionMySQL = new MySqlConnection(connSett.GetConnectionStringMySQL());

            try
            {
                connectionMySQL.Open();
            }
            catch (Exception e)
            {
                logging_catch_db(connectionMySQL, e);

                er = -1;
            }

            if (er == 0)
            {
                ExecNonQuery(connectionMySQL, query, null, null, out er);

                connectionMySQL.Close();
            }
            else
                ;
        }

        public static void ExecNonQuery(string path, string query, out int er)
        {
            er = 0;
            
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

                        string msg = string.Empty;
                        msg += @"!Отладка!" + connectionOleDB.ConnectionString + Environment.NewLine;
                        msg += connectionOleDB.ConnectionString;
                        Logging.Logg().LogToFile(commandOleDB.CommandText, true, true, true);
                    }
                    else
                        ; //
                }
                catch (Exception e)
                {
                    logging_catch_db(connectionOleDB, e);

                    er = -1;
                }

                connectionOleDB.Close();
            }
            else
                ;
        }

        public static Int32 getIdNext(ConnectionSettings connSett, string nameTable)
        {
            Int32 idRes = -1,
                err = 0;

            idRes = Convert.ToInt32(Select(connSett, "SELECT MAX(ID) FROM " + nameTable, out err).Rows[0][0]);

            return ++idRes;
        }

        //Изменение (вставка), удаление
        public static void RecUpdateInsertDelete(ConnectionSettings connSett, string nameTable, DataTable origin, DataTable data, out int err)
        {
            if (!(data.Rows.Count < origin.Rows.Count))
            {
                //UPDATE, INSERT
                RecUpdateInsert(connSett, nameTable, origin, data, out err);
            }
            else
            {
                //DELETE
                RecDelete(connSett, nameTable, origin, data, out err);
            }
        }

        //Изменение (вставка) в оригинальную таблицу записей измененных (добавленных) в измененную таблицу (обязательно наличие поля: ID)
        public static void RecUpdateInsert(ConnectionSettings connSett, string nameTable, DataTable origin, DataTable data, out int err)
        {
            err = 0;

            int j = -1, k = -1;
            bool bUpdate = false;
            DataRow[] dataRows;
            string[] strQuery = new string[(int)DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE];
            string valuesForInsert = string.Empty;

            for (j = 0; j < data.Rows.Count; j++)
            {
                dataRows = origin.Select("ID=" + data.Rows[j]["ID"]);
                if (dataRows.Length == 0)
                {
                    //INSERT
                    strQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = string.Empty;
                    valuesForInsert = string.Empty;
                    strQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = "INSERT INTO " + nameTable + " (";
                    for (k = 0; k < data.Columns.Count; k++)
                    {
                        strQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += data.Columns[k].ColumnName + ",";
                        valuesForInsert += DbTSQLInterface.valueForQuery(data, j, k) + ",";
                    }
                    strQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] = strQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Substring(0, strQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT].Length - 1);
                    valuesForInsert = valuesForInsert.Substring(0, valuesForInsert.Length - 1);
                    strQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += ") VALUES (";
                    strQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT] += valuesForInsert + ")";
                    DbTSQLInterface.ExecNonQuery(connSett, strQuery[(int)DbTSQLInterface.QUERY_TYPE.INSERT], out err);
                }
                else
                {
                    if (dataRows.Length == 1)
                    {//UPDATE
                        bUpdate = false;
                        strQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] = string.Empty;
                        for (k = 0; k < data.Columns.Count; k++)
                        {
                            if (!(data.Rows[j][k].Equals(origin.Rows[j][k]) == true))
                                if (bUpdate == false) bUpdate = true; else ;
                            else
                                ;

                            strQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += data.Columns[k].ColumnName + "="; // + data.Rows[j][k] + ",";

                            strQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] += DbTSQLInterface.valueForQuery(data, j, k) + ",";
                        }

                        if (bUpdate == true)
                        {//UPDATE
                            strQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] = strQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE].Substring(0, strQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE].Length - 1);
                            strQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] = "UPDATE " + nameTable + " SET " + strQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE] + " WHERE ID=" + data.Rows[j]["ID"];

                            DbTSQLInterface.ExecNonQuery(connSett, strQuery[(int)DbTSQLInterface.QUERY_TYPE.UPDATE], out err);
                        }
                        else
                            ;
                    }
                    else
                        throw new Exception("Невозможно определить тип изменения таблицы " + nameTable);
                }
            }
        }

        //Удаление из оригинальной таблицы записей не существующих в измененной таблице (обязательно наличие поля: ID)
        public static void RecDelete(ConnectionSettings connSett, string nameTable, DataTable origin, DataTable data, out int err)
        {
            err = 0;

            int j = -1;
            DataRow[] dataRows;
            string[] strQuery = new string[(int)DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE];

            for (j = 0; j < origin.Rows.Count; j++)
            {
                dataRows = data.Select("ID=" + origin.Rows[j]["ID"]);
                if (dataRows.Length == 0)
                {
                    //DELETE
                    strQuery[(int)DbTSQLInterface.QUERY_TYPE.DELETE] = string.Empty;
                    strQuery[(int)DbTSQLInterface.QUERY_TYPE.DELETE] = "DELETE FROM " + nameTable + " WHERE ID=" + origin.Rows[j]["ID"];
                    DbTSQLInterface.ExecNonQuery(connSett, strQuery[(int)DbTSQLInterface.QUERY_TYPE.DELETE], out err);
                }
                else
                {  //Ничего удалять не надо
                    if (dataRows.Length == 1)
                    {
                    }
                    else
                    {
                    }
                }
            }
        }

        public static bool IsConnected(DbConnection obj)
        {
            return (!(obj == null)) && (!(obj.State == ConnectionState.Closed)) && (!(obj.State == ConnectionState.Broken));
        }
    }
}
