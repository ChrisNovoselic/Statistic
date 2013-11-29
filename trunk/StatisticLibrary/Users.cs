using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;

namespace StatisticCommon
{    
    public class Users : object
    {
        private ConnectionSettings connSettConfigDB;
        private int role;
        private int id_tec;

        private bool compareIpVal (int [] ip_trust, int [] ip)
        {
            bool bRes = true;
            int j = -1;

            for (j = 0; j < ip_trust.Length; j ++) {
                if (ip_trust [j] == ip [j])
                    continue;
                else {
                    bRes = false;
                    break;
                }
                    
            }

            return bRes;
        }

        private int [] strIpToVal (string [] str) {
            int j = -1;
            int [] val = new int[str.Length];
            
            for (j = 0; j < str.Length; j++)
            {
                val[j] = Convert.ToInt32(str[j]);
            }

            return val;
        }

        public Users(ConnectionSettings connSett)
        {
            connSettConfigDB = connSett;

            int err = 0,
                i = -1, j = -1;
            DataTable dataUsers;
            List<int[]> list_ip_val_parts = new List<int[]> (2);
            string domain_name;
            System.Net.IPAddress [] listIP = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList;
            if (listIP.Length > 1)
                list_ip_val_parts.Add(strIpToVal(listIP[1].ToString().Split('.')));
            else
                if (listIP.Length > 0)
                    list_ip_val_parts.Add(strIpToVal(listIP[0].ToString().Split('.')));
                else
                    throw new Exception ("Не удалось получить список IP-адресов клиента");

            GetUsers ("", out dataUsers, out err);

            if ((err == 0) && (dataUsers.Rows.Count > 0))
            {
                for (i = 0; i < dataUsers.Rows.Count; i ++)
                {
                    if (list_ip_val_parts.Count == 1)
                        list_ip_val_parts.Add (strIpToVal(dataUsers.Rows[i]["IP"].ToString().Split('.')));
                    else
                        list_ip_val_parts[1] = strIpToVal(dataUsers.Rows[i]["IP"].ToString().Split('.'));

                    if (compareIpVal(list_ip_val_parts[0], list_ip_val_parts[1]) == true)
                    {
                        break;
                    }
                    else
                        ;
                }

                if (i < dataUsers.Rows.Count)
                {
                    role = Convert.ToInt32 (dataUsers.Rows [i]["ID_ROLE"]);
                    id_tec = Convert.ToInt32(dataUsers.Rows[i]["ID_TEC"]);
                }
                else
                    throw new Exception("Пользователь не найден в списке БД конфигурации");
            }
            else
            {
                if (err == 0)
                    throw new Exception ("Список пользователей в БД конфигурации пуст");
                else
                    throw new Exception("Ошибка получения списка пользователей из БД конфигурации");
            }

            Initialize ();
        }

        private void Initialize () {
            Logging.Logg().LogLock();
            Logging.Logg().LogToFile("Users::Initialize ()", true, false, false);
            Logging.Logg().LogUnlock();   
        }

        public int Role {
            get {
                return role;
            }
        }

        public int allTEC
        {
            get
            {
                return id_tec;
            }
        }

        public void GetUsers(string where, out DataTable users, out int err)
        {
            err = 0;            
            users = new DataTable ();

            string strQuery = string.Empty;
            //strQuer//strQuery =  "SELECT * FROM users WHERE DOMAIN_NAME='" + Environment.UserDomainName + "\\" + Environment.UserName + "'";
            //strQuery =  "SELECT * FROM users WHERE DOMAIN_NAME='NE\\ChrjapinAN'";
            strQuery = "SELECT * FROM users" + where;

            users = DbInterface.Select(connSettConfigDB, strQuery, out err);
        }

        public static void GetUsers(ConnectionSettings connSett, string where, out DataTable users, out int err)
        {
            err = 0;
            users = new DataTable();

            string strQuery = string.Empty;
            strQuery = "SELECT * FROM users";
            if ((!(where == null)) && (where.Length > 0))
                strQuery += " WHERE " + where;
            else
                ;

            strQuery += " ORDER BY DESCRIPTION";

            users = DbInterface.Select(connSett, strQuery, out err);
        }
    }
}
