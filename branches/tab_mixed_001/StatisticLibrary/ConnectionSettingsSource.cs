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

        private static DataTable GetConnectionSettings(DataTable src, int row_src, DataTable psw, int row_psw)
        {
            string errMsg, strPsw;

            errMsg = strPsw = string.Empty;

            if (psw.Rows.Count == 1) {
                //hash = psw.Rows[row_psw]["HASH"].ToString ().ToCharArray ();
                //len_hash = psw.Rows[row_psw]["HASH"].ToString().Length;

                strPsw = Crypt.Crypting ().Decrypt (psw.Rows[row_psw]["HASH"].ToString (), "AsDfGhJkL;");
            }
            else
                ;

            if (src.Columns.IndexOf ("PASSWORD") < 0)
                src.Columns.Add("PASSWORD", typeof(string));
            else
                ;

            //strPsw = Crypt.Crypting().un(hash, len_hash, out errMsg).ToString();
            if (errMsg.Length > 0)
                strPsw = string.Empty;
            else
                ;

            src.Rows[row_src]["PASSWORD"] = strPsw;

            return src;
        }

        public static DataTable GetConnectionSettings (ConnectionSettings connSett, int id_ext, int id_role, out int er)
        {
            er = 0;

            DataTable tableRes = DbTSQLInterface.Select(connSett, ConnectionSettingsRequest(id_ext), out er),
                    tablePsw = DbTSQLInterface.Select(connSett, PasswordRequest(id_ext, id_role), out er);

            return GetConnectionSettings (tableRes, 0, tablePsw, 0);
        }

        public static DataTable GetConnectionSettings(MySqlConnection conn, int id_ext, int id_role, out int er)
        {
            er = 0;

            DataTable tableRes = DbTSQLInterface.Select(conn, ConnectionSettingsRequest(id_ext), null, null, out er),
                    tablePsw = DbTSQLInterface.Select(conn, PasswordRequest(id_ext, id_role), null, null, out er);

            return GetConnectionSettings(tableRes, 0, tablePsw, 0);
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
                DataTable tableSource = DbTSQLInterface.Select(conn, "SELECT * FROM SOURCE", null, null, out err),
                            tablePsw;

                if (err == 0)
                {
                    for (i = 0; i < tableSource.Rows.Count; i ++)
                    {
                        listConnSett.Add (new ConnectionSettings ());

                        listConnSett[i].id = Convert.ToInt32(tableSource.Rows[i]["ID"]);
                        listConnSett[i].name = tableSource.Rows[i]["NAME_SHR"].ToString();

                        listConnSett [i].server = tableSource.Rows [i] ["IP"].ToString ();
                        listConnSett[i].port = Convert.ToInt32 (tableSource.Rows[i]["PORT"]);
                        listConnSett[i].dbName = tableSource.Rows[i]["DB_NAME"].ToString();
                        listConnSett[i].userName = tableSource.Rows[i]["UID"].ToString();
                        //Password
                        listConnSett[i].ignore = Convert.ToBoolean (tableSource.Rows[i]["IGNORE"].ToString());

                        tablePsw = DbTSQLInterface.Select(conn, PasswordRequest(Convert.ToInt32(tableSource.Rows[i]["ID"]), 501), null, null, out err);

                        tableSource = GetConnectionSettings(tableSource, i, tablePsw, 0);
                        //Password
                        listConnSett[i].password = tableSource.Rows[i]["PASSWORD"].ToString();
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
            int i = -1,c = -1;
            string strQuery, psw
                , hash;
            //char []hash;
            //StringBuilder sb;
            
            strQuery = psw = string.Empty;


            MySqlConnection conn = DbTSQLInterface.GetConnection (DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.MySQL, m_ConnectionSettings, out err);

            if (err == 0)
            {
                DataTable tableSource = null,
                            tablePsw = null;

                if (err == 0)
                {
                    for (i = 0; i < listConnSett.Count; i++)
                    {
                        tableSource = DbTSQLInterface.Select(conn, ConnectionSettingsRequest(listConnSett[i].id), null, null, out err);
                        tablePsw = DbTSQLInterface.Select(conn, PasswordRequest(listConnSett [i].id, 501), null, null, out err);

                        if (tableSource.Rows.Count == 0)
                        {//INSERT
                        }
                        else
                            if (tableSource.Rows.Count == 1)
                            {//UPDATE
                                if ((listConnSett[i].server.Equals (tableSource.Rows [0]["IP"].ToString ()) == false) ||
                                    (listConnSett[i].dbName.Equals(tableSource.Rows[0]["DB_NAME"].ToString()) == false) ||
                                    (listConnSett[i].userName.Equals(tableSource.Rows[0]["UID"].ToString()) == false))
                                {
                                    strQuery += "UPDATE SOURCE SET ";

                                    strQuery += "IP='" + listConnSett[i].server + "',";
                                    strQuery += "DB_NAME='" + listConnSett[i].dbName + "',";
                                    strQuery += "UID='" + listConnSett[i].userName + "'";

                                    strQuery += " WHERE ID=" + listConnSett[i].id + ";";
                                }
                                else
                                    ; //Ничего не изменилось

                                if (listConnSett[i].password.Length > 0)
                                {
                                    //sb = new StringBuilder(listConnSett[i].password);
                                    //hash = Crypt.Crypting().to(sb, out err);
                                    hash = Crypt.Crypting().Encrypt(listConnSett[i].password, Crypt.KEY);

                                    //if (err > 0)
                                    if (hash.Length > 0)
                                    {
                                        if (tablePsw.Rows.Count == 0)
                                        {//INSERT
                                            strQuery += "INSERT INTO passwords (ID_EXT, ID_ROLE, HASH) VALUES (";

                                            strQuery += listConnSett[i].id + ", ";
                                            strQuery += 501 + ", ";
                                            strQuery += "'" + hash + "'";

                                            strQuery += ");";
                                        }
                                        else
                                            if (tablePsw.Rows.Count == 1)
                                            {//UPDATE
                                                strQuery += "UPDATE passwords SET ";

                                                strQuery += "HASH='" + hash + "'";

                                                strQuery += " WHERE ID_EXT=" + listConnSett[i].id;
                                                strQuery += " AND ";
                                                strQuery += "ID_ROLE=" + 501 + ";";
                                            }
                                            else
                                                ;
                                    }
                                    else
                                        ; //Ошибка шифрования пароля ИЛИ нет пароля
                                }
                                else
                                    ; //Нет пароля
                            }
                            else
                                ;
                    }

                    DbTSQLInterface.ExecNonQuery (conn, strQuery, null, null, out err);
                }
                else
                    ;
            }
            else
                ;

            DbTSQLInterface.CloseConnection (conn, out err);
        }
    }
}
