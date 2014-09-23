using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace StatisticCommon
{    
    public class Users : object
    {
        public enum ID_ROLES { KOM_DISP = 1, ADMIN, USER, NSS = 101, MAJOR_MASHINIST, MASHINIST, SOURCE_DATA = 501,
                            COUNT_ID_ROLES = 7};

        private static int m_id;
        private static string m_domain_name;
        private static int m_role;
        private static int m_id_tec;

        //private bool compareIpVal (int [] ip_trust, int [] ip)
        //{
        //    bool bRes = true;
        //    int j = -1;

        //    for (j = 0; j < ip_trust.Length; j ++) {
        //        if (ip_trust [j] == ip [j])
        //            continue;
        //        else {
        //            bRes = false;
        //            break;
        //        }

        //    }

        //    return bRes;
        //}

        //private int [] strIpToVal (string [] str) {
        //    int j = -1;
        //    int [] val = new int[str.Length];
            
        //    for (j = 0; j < str.Length; j++)
        //    {
        //        Logging.Logg().LogDebugToFile(@"val[" + j.ToString () + "] = " + val [j]);

        //        val[j] = Convert.ToInt32(str[j]);
        //    }

        //    return val;
        //}

        public Users(int idListener)
        {
            if (HAdmin.USERS_DOMAINNAME.Equals(@"Users DomainName") == true)
            {
                Logging.Logg ().LogDebugToFile (@"Режим отладки");

                m_domain_name = HAdmin.USERS_DOMAINNAME;
                m_id = HAdmin.USERS_ID;
                m_role = HAdmin.USERS_ID_ROLE;
                m_id_tec = HAdmin.USERS_ID_TEC;
            }
            else {
                int err = 0,
                    i = -1, j = -1;
                DataTable dataUsers;

                string domain_name = string.Empty;
                if (HAdmin.USERS_DOMAINNAME.Equals(string.Empty) == true) {
                    //Проверка ИМЯ_ПОЛЬЗОВАТЕЛЯ
                    try { domain_name = Environment.UserDomainName + @"\" + Environment.UserName; }
                    catch (Exception e) {
                        string [] args = Environment.GetCommandLineArgs ();
                        if (args.Length > 1) {
                            domain_name = args[1].Substring(args[1].IndexOf('=') + 1, args[1].Length - (args[1].IndexOf('=') + 1));
                        }
                        else
                            throw e;
                    }
                }
                else
                    domain_name = HAdmin.USERS_DOMAINNAME;
                //Проверка IP-адрес
                //System.Net.IPAddress [] listIP = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList;
                //int indxIP = -1;
                //if (listIP.Length == 0)
                //    throw new Exception ("Не удалось получить список IP-адресов клиента");
                //else
                //    ;

                DbConnection connDB = DbSources.Sources().GetConnection(idListener, out err);

                //Проверка ИМЯ_ПОЛЬЗОВАТЕЛЯ
                GetUsers(ref connDB, @"DOMAIN_NAME=" + @"'" + domain_name + @"'", string.Empty, out dataUsers, out err);
                //Проверка IP-адрес
                //GetUsers(ref connDB, string.Empty, "DESCRIPTION", out dataUsers, out err);

                if ((err == 0) && (dataUsers.Rows.Count > 0))
                {//Найдена хотя бы одна строка
                    for (i = 0; i < dataUsers.Rows.Count; i ++)
                    {
                        //Проверка IP-адрес                    
                        //for (indxIP = 0; indxIP < listIP.Length; indxIP ++) {
                        //    if (listIP[indxIP].Equals(System.Net.IPAddress.Parse (dataUsers.Rows[i][@"IP"].ToString())) == true) {
                        //        //IP найден
                        //        break;
                        //    }
                        //    else
                        //        ;
                        //}

                        //Проверка ИМЯ_ПОЛЬЗОВАТЕЛЯ
                        if (dataUsers.Rows[i][@"DOMAIN_NAME"].ToString ().Equals (domain_name, StringComparison.CurrentCultureIgnoreCase) == true) break; else ;
                    }

                    if (i < dataUsers.Rows.Count)
                    {
                        m_domain_name = dataUsers.Rows[i]["DOMAIN_NAME"].ToString ();
                        m_id = Convert.ToInt32(dataUsers.Rows[i]["ID"]);
                        m_role = Convert.ToInt32 (dataUsers.Rows [i]["ID_ROLE"]);
                        m_id_tec = Convert.ToInt32(dataUsers.Rows[i]["ID_TEC"]);
                    }
                    else
                        throw new Exception("Пользователь не найден в списке БД конфигурации");
                }
                else
                {//Не найдено ни одной строки
                    if (err == 0)
                        throw new Exception("Пользователь не найден в списке БД конфигурации");
                    else
                        throw new Exception("Ошибка получения списка пользователей из БД конфигурации");
                }

                //DbTSQLInterface.CloseConnection(connDB, out err);
            }

            Initialize ();
        }

        private void Initialize () {
            string strMes = string.Empty;

            strMes = @"Пользователь: " + DomainName + @", (id=" + Id + @"), роль: " + Role + @", id_tec=" + allTEC;

            System.Net.IPAddress[] listIP = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList;
            int indxIP = -1;
            for (indxIP = 0; indxIP < listIP.Length; indxIP ++) {
                strMes += @", ip[" + indxIP + @"]=" + listIP[indxIP].ToString ();
            }

            Logging.Logg().LogToFile(strMes, true, true, true);
        }

        public static int Role {
            get {
                return m_role;
            }
        }

        public static bool RoleIsDisp {
            get { return ((Users.Role == (int)Users.ID_ROLES.ADMIN) || (Users.Role == (int)Users.ID_ROLES.KOM_DISP) || (Users.Role == (int)Users.ID_ROLES.NSS)); }
        }

        public static int allTEC
        {
            get
            {
                return m_id_tec;
            }
        }

        public static int Id
        {
            get
            {
                return m_id;
            }
        }

        public static string DomainName
        {
            get
            {
                return m_domain_name;
            }
        }

        private static string getUsersRequest(string where, string orderby)
        {
            string strQuery = string.Empty;
            //strQuer//strQuery =  "SELECT * FROM users WHERE DOMAIN_NAME='" + Environment.UserDomainName + "\\" + Environment.UserName + "'";
            //strQuery =  "SELECT * FROM users WHERE DOMAIN_NAME='NE\\ChrjapinAN'";
            strQuery = "SELECT * FROM users";
            if ((!(where == null)) && (where.Length > 0))
                strQuery += " WHERE " + where;
            else
                ;

            if ((!(orderby == null)) && (orderby.Length > 0))
                strQuery += " ORDER BY " + orderby;
            else
                ;

            return strQuery;
        }

        /*public void GetUsers(string where, string orderby, out DataTable users, out int err)
        {
            err = 0;            
            users = new DataTable ();

            users = DbTSQLInterface.Select(connSettConfigDB, getUsersRequest(where, orderby), out err);
        }

        public static void GetUsers(ConnectionSettings connSett, string where, string orderby, out DataTable users, out int err)
        {
            err = 0;
            users = new DataTable();

            users = DbTSQLInterface.Select(connSett, getUsersRequest(where, orderby), out err);
        }*/

        public static void GetUsers(ref DbConnection conn, string where, string orderby, out DataTable users, out int err)
        {
            err = 0;
            users = new DataTable();

            users = DbTSQLInterface.Select(ref conn, getUsersRequest(where, orderby), null, null, out err);
        }

        public static void GetRoles(ref DbConnection conn, string where, string orderby, out DataTable roles, out int err)
        {
            err = 0;
            roles = new DataTable();

            roles = DbTSQLInterface.Select(ref conn, @"SELECT * FROM ROLES WHERE ID < 500", null, null, out err);
        }
    }
}
