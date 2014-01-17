using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Data;

using MySql.Data.MySqlClient;

//namespace Statistic
namespace StatisticCommon
{
    public class ConnectionSettingsSource
    {
        private ConnectionSettings m_ConnectionSettings;

        public ConnectionSettingsSource (ConnectionSettings connsett)
        {
            m_ConnectionSettings = connsett;
        }

        private static string ConnectionSettingsRequest (int id)
        {
            return "SELECT src.* FROM SOURCE src WHERE src.ID = " + id.ToString();
        }

        private static string PasswordRequest(int id, int id_role)
        {
            if (id_role < 0)
                id_role = 501;
            else
                ;

            return "SELECT psw.* FROM passwords psw WHERE psw.ID_EXT = " + id.ToString() + " AND ID_ROLE = " + id_role.ToString();
        }

        private static DataTable GetConnectionSettings(DataTable src, DataTable psw)
        {
            string hash = string.Empty;
            if (psw.Rows.Count == 1)
                hash = psw.Rows[0]["HASH"].ToString ();
            else
                ;

            if (src.Columns.IndexOf ("PASSWORD") < 0)
                src.Columns.Add ("PASSWORD", Type.GetType ("string"));
            else
                ;

            src.Rows[0]["PASSWORD"] = Passwords.ToString (hash);

            return src;
        }

        public static DataTable GetConnectionSettings (ConnectionSettings connSett, int id_ext, int id_role, out int er)
        {
            er = 0;

            DataTable tableRes = DbTSQLInterface.Select(connSett, ConnectionSettingsRequest(id_ext), out er),
                    tablePsw = DbTSQLInterface.Select(connSett, PasswordRequest(id_ext, id_role),out er);

            return GetConnectionSettings (tableRes, tablePsw);
        }

        public static DataTable GetConnectionSettings(MySqlConnection conn, int id_ext, int id_role, out int er)
        {
            er = 0;

            DataTable tableRes = DbTSQLInterface.Select(conn, ConnectionSettingsRequest(id_ext), null, null, out er),
                    tablePsw = DbTSQLInterface.Select(conn, PasswordRequest(id_ext, id_role), null, null, out er);

            return GetConnectionSettings(tableRes, tablePsw);
        }

        public void Read(out List<ConnectionSettings> listConnSett, out int err, out string mes)
        {
            listConnSett = new List<ConnectionSettings> ();
            err = 0;
            mes = string.Empty;

            int i = -1;

            MySqlConnection conn = DbTSQLInterface.GetConnection (DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.MySQL, m_ConnectionSettings, out err);

            if (err == 0)
            {
                DataTable tableSource = DbTSQLInterface.Select(conn, "SELECT * FROM SOURCE", null, null, out err);

                if (err == 0)
                {
                    for (i = 0; i < tableSource.Rows.Count; i ++)
                    {
                        listConnSett.Add (new ConnectionSettings ());

                        listConnSett[i].name = tableSource.Rows[i]["NAME_SHR"].ToString();

                        listConnSett [i].server = tableSource.Rows [i] ["IP"].ToString ();
                        listConnSett[i].port = Convert.ToInt32 (tableSource.Rows[i]["PORT"]);
                        listConnSett[i].dbName = tableSource.Rows[i]["DB_NAME"].ToString();
                        listConnSett[i].userName = tableSource.Rows[i]["UID"].ToString();
                        listConnSett[i].password = tableSource.Rows[i]["PASSWORD"].ToString();
                        listConnSett[i].ignore = Convert.ToBoolean (tableSource.Rows[i]["IGNORE"].ToString());
                    }
                }
                else
                    ;
            }
            else
                ;

            DbTSQLInterface.CloseConnection (conn, out err);
        }

        public void Save(List<ConnectionSettings> listConnSett, out int err)
        {
            err = 1;

            err = 0;
        }
    }
}
