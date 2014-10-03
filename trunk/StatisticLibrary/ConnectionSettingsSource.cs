using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Data;
using System.Data.Common;

using MySql.Data.MySqlClient;

//namespace Statistic
namespace StatisticCommon
{
    /// <summary>
    /// ����� ��� ������ � ����������� ������ ���������� ������
    /// </summary>
    public class ConnectionSettingsSource
    {
        /// <summary>
        /// ������������� �� ������������
        /// </summary>
        private int m_idListener;

        public ConnectionSettingsSource (int idListener)
        {
            m_idListener = idListener;
        }

        /// <summary>
        /// ������ ��� ��������� ������� (���� ������) � ����������� ��������� ������
        /// </summary>
        /// <param name="id">������������� ��������� ������ � ������� 'SOURCE'</param>
        /// <returns>������ � ����������� ��������� ������</returns>
        private static string ConnectionSettingsRequest (int id)
        {
            return "SELECT src.* FROM SOURCE src WHERE src.ID = " + id.ToString();
        }

        /// <summary>
        /// ������ ������ (������������ ��� 2.�.� � �� ������������ ��� 1.9.�)
        /// </summary>
        /// <param name="typeDB_CFG">��� ���� ������ ������������ (1.9.�/2.�.�)</param>
        /// <param name="id">������������� ������������(����) - ����� ���������� �����</param>
        /// <param name="id_role">���� ������������ (������ ��� 2.�.�), "����" ��������� ������ 501</param>
        /// <returns>����� �������</returns>
        private static string PasswordRequest(InitTECBase.TYPE_DATABASE_CFG typeDB_CFG, int id, int id_role)
        {
            string strRes = string.Empty;
            
            if (id_role < 0)
                id_role = 501;
            else
                ;

            strRes = "SELECT psw.* FROM passwords psw WHERE ";
            if (typeDB_CFG == InitTECBase.TYPE_DATABASE_CFG.CFG_200)
                strRes += @"psw.ID_EXT = " + id.ToString() + " AND ";
            else
                strRes += string.Empty;

            strRes +=  "ID_ROLE = " + id_role.ToString();

            return strRes;
        }

        /// <summary>
        /// ��������� ������� � ����������� ���������� ��������� ������ ������������� �������
        /// </summary>
        /// <param name="src">������� - ���-� ���������� ������� 'GetConnectionSettings'</param>
        /// <param name="row_src"></param>
        /// <param name="psw"></param>
        /// <param name="row_psw"></param>
        /// <returns></returns>
        private static DataTable GetConnectionSettings(ref DataTable src, int row_src, ref DataTable psw, int row_psw)
        {
            string errMsg, strPsw;

            errMsg = strPsw = string.Empty;

            if (psw.Rows.Count == 1) {
                //hash = psw.Rows[row_psw]["HASH"].ToString ().ToCharArray ();
                //len_hash = psw.Rows[row_psw]["HASH"].ToString().Length;

                strPsw = Crypt.Crypting ().Decrypt (psw.Rows[row_psw]["HASH"].ToString (), Crypt.KEY);
            }
            else
                ;

            //�������� � ����� ��������� �� ���������� ������
            if (src.Columns.IndexOf ("PASSWORD") < 0) {
                src.Columns.Add("PASSWORD", typeof(string));
            }
            else
                ;

            if (row_src < src.Rows.Count)
                src.Rows[row_src]["PASSWORD"] = strPsw;
            else
                ;

            return src;
        }

        /*public static DataTable GetConnectionSettings (ConnectionSettings connSett, int id_ext, int id_role, out int er)
        {
            er = 0;

            DataTable tableRes = DbTSQLInterface.Select(connSett, ConnectionSettingsRequest(id_ext), out er),
                    tablePsw = DbTSQLInterface.Select(connSett, PasswordRequest(id_ext, id_role), out er);

            return GetConnectionSettings (ref tableRes, 0, ref tablePsw, 0);
        }*/

        public static DataTable GetConnectionSettings(InitTECBase.TYPE_DATABASE_CFG typeDB_CFG, int idListener, int id_ext, int id_role, out int er)
        {
            DbConnection conn = DbSources.Sources ().GetConnection (idListener, out er);
            if (er == 0)
                return GetConnectionSettings (typeDB_CFG, ref conn, id_ext, id_role, out er);
            else
                return null;
        }

        public static DataTable GetConnectionSettings(InitTECBase.TYPE_DATABASE_CFG typeDB_CFG, ref DbConnection conn, int id_ext, int id_role, out int er)
        {
            er = 0;

            DataTable tableRes = DbTSQLInterface.Select(ref conn, ConnectionSettingsRequest(id_ext), null, null, out er),
                    tablePsw = DbTSQLInterface.Select(ref conn, PasswordRequest(typeDB_CFG, id_ext, id_role), null, null, out er);

            if ((tableRes.Rows.Count > 0) && (tablePsw.Rows.Count > 0))
                tableRes = GetConnectionSettings(ref tableRes, 0, ref tablePsw, 0);
            else {
                if ((!(tablePsw.Rows.Count > 0)) && (tableRes.Columns.IndexOf (@"PASSWORD") < 0))
                {
                    er = -1;
                }
                else {
                    if (tableRes.Columns.IndexOf (@"PASSWORD") < 0)
                        tableRes.Columns.Add (@"PASSWORD", typeof (string));
                    else
                        ;

                    er = 0;
                }
            }

            return tableRes;
        }

        public void Read(int idListener, out List<ConnectionSettings> listConnSett, out int err, out string mes)
        {
            listConnSett = new List<ConnectionSettings> ();
            err = 0;
            mes = string.Empty;

            int i = -1;

            DbConnection conn = DbSources.Sources().GetConnection(idListener, out err);
            //DbConnection conn = DbTSQLInterface.GetConnection (m_ConnectionSettings, out err);

            if (err == 0)
            {
                DataTable tableSource = DbTSQLInterface.Select(ref conn, "SELECT * FROM SOURCE", null, null, out err),
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
                        listConnSett[i].ignore = Convert.ToInt32 (tableSource.Rows[i]["IGNORE"].ToString()) == 1;

                        //TYPE_DATABASE_CFG.CFG_200 = ???
                        tablePsw = DbTSQLInterface.Select(ref conn, PasswordRequest(InitTECBase.TYPE_DATABASE_CFG.CFG_200, Convert.ToInt32(tableSource.Rows[i]["ID"]), 501), null, null, out err);

                        tableSource = GetConnectionSettings(ref tableSource, i, ref tablePsw, 0);
                        //Password
                        listConnSett[i].password = tableSource.Rows[i]["PASSWORD"].ToString();
                    }
                }
                else
                    ;
            }
            else
                ;

            //DbTSQLInterface.CloseConnection (conn, out err);
        }

        public void Save(int idListener, List<ConnectionSettings> listConnSett, out int err)
        {
            err = 1;
            int i = -1,c = -1;
            string strQuery, psw
                , hash;
            //char []hash;
            //StringBuilder sb;
            
            strQuery = psw = string.Empty;

            DbConnection conn = DbSources.Sources().GetConnection(idListener, out err);
            //DbConnection conn = DbTSQLInterface.GetConnection (m_ConnectionSettings, out err);

            if (err == 0)
            {
                DataTable tableSource = null,
                            tablePsw = null;

                if (err == 0)
                {
                    for (i = 0; i < listConnSett.Count; i++)
                    {
                        tableSource = DbTSQLInterface.Select(ref conn, ConnectionSettingsRequest(listConnSett[i].id), null, null, out err);
                        //InitTECBase.TYPE_DATABASE_CFG.CFG_200 = ???
                        tablePsw = DbTSQLInterface.Select(ref conn, PasswordRequest(InitTECBase.TYPE_DATABASE_CFG.CFG_200, listConnSett[i].id, 501), null, null, out err);

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
                                    ; //������ �� ����������

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
                                        ; //������ ���������� ������ ��� ��� ������
                                }
                                else
                                    ; //��� ������
                            }
                            else
                                ;
                    }

                    DbTSQLInterface.ExecNonQuery (ref conn, strQuery, null, null, out err);
                }
                else
                    ;
            }
            else
                ;

            //DbTSQLInterface.CloseConnection (conn, out err);
        }
    }
}
