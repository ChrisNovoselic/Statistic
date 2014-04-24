using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Security.Cryptography;
using System.Data.OleDb;
using System.IO;
using MySql.Data.MySqlClient;
using System.Globalization;

namespace StatisticCommon
{
    public class Passwords : object
    {
        MD5CryptoServiceProvider md5;

        public enum ID_ROLES : uint { COM_DISP = 1, ADMIN, NSS };
        public static string getOwnerPass(int id_role)
        {
            string[] ownersPass = { "диспетчера", "администратора", "НССа" };

            return ownersPass[id_role - 1];
        }

        private volatile HAdmin.Errors passResult;
        private volatile string passReceive;
        private volatile uint m_idRolePass;
        private volatile uint m_idPass;
        private Object m_lockObj;

        public ConnectionSettings connSettConfigDB;

        public Passwords()
        {
            Initialize();
        }

        private void Initialize()
        {
            md5 = new MD5CryptoServiceProvider();

            m_lockObj = new Object();
        }

        void MessageBox(string msg, MessageBoxButtons btn = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Error)
        {
            //MessageBox.Show(this, msg, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Logging.Logg().LogToFile(msg, true, true, true);
        }

        private void GetPassword(out int er)
        {
            DataTable passTable = DbTSQLInterface.Select(connSettConfigDB, GetPassRequest(), out er);
            if (er == 0)
                if (!(passTable.Rows[0][0] is DBNull))
                    passReceive = passTable.Rows[0][0].ToString();
                else
                    passResult = HAdmin.Errors.ParseError;
            else
                passResult = HAdmin.Errors.NoAccess;
        }

        public bool SetPassword(string password, uint id, uint idRolePass)
        {
            int err = -1;
            passResult = HAdmin.Errors.NoError;

            m_idPass = id;
            m_idRolePass = idRolePass;

            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

            StringBuilder hashedString = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                hashedString.Append(hash[i].ToString("x2"));

            GetPassword(out err);

            if (!(passResult == HAdmin.Errors.NoError))
            {
                //MessageBox.Show(this, "Ошибка получения пароля " + getOwnerPass () + ". Пароль не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Ошибка получения пароля " + getOwnerPass((int)m_idRolePass) + ". Пароль не сохранён.");

                return false;
            }
            else
                ;

            if (passReceive == null)
                DbTSQLInterface.ExecNonQuery(connSettConfigDB, SetPassRequest(hashedString.ToString(), true), out err);
            else
                DbTSQLInterface.ExecNonQuery(connSettConfigDB, SetPassRequest(hashedString.ToString(), false), out err);

            if (passResult != HAdmin.Errors.NoError)
            {
                //MessageBox.Show(this, "Ошибка сохранения пароля " + getOwnerPass () + ". Пароль не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Ошибка сохранения пароля " + getOwnerPass((int)m_idRolePass) + ". Пароль не сохранён.");
                return false;
            }

            return true;
        }

        public HAdmin.Errors ComparePassword(string password, uint id, uint id_role)
        {
            int err = -1;
            passResult = HAdmin.Errors.NoError;
            passReceive = null;

            //if (connSettConfigDB == null)
            //    return HAdmin.Errors.NoAccess;
            //else
            //    ;

            m_idPass = id;
            m_idRolePass = id_role;

            if (password.Length < 1)
            {
                //MessageBox.Show(this, "Длина пароля меньше допустимой.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Длина пароля меньше допустимой.");

                return HAdmin.Errors.InvalidValue;
            }
            else
                ;

            m_idPass = id;

            string hashFromForm = "";
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

            StringBuilder hashedString = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                hashedString.Append(hash[i].ToString("x2"));

            GetPassword(out err);

            if (!(passResult == HAdmin.Errors.NoError))
            {
                //MessageBox.Show(this, "Ошибка получения пароля " + getOwnerPass () + ".", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Ошибка получения пароля " + getOwnerPass((int)m_idRolePass) + ".");

                return passResult;
            }
            else
                ;

            if (passReceive == null)
            {
                //MessageBox.Show(this, "Пароль не установлен.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Пароль не установлен.");

                return HAdmin.Errors.NoSet;
            }
            else
            {
                hashFromForm = hashedString.ToString();

                if (hashFromForm != passReceive)
                {
                    //MessageBox.Show(this, "Пароль введён неверно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox("Пароль введён неверно.");

                    return HAdmin.Errors.InvalidValue;
                }
                else
                    return HAdmin.Errors.NoError;
            }
        }

        private string GetPassRequest()
        {
            string strRes = string.Empty;
            strRes = "SELECT HASH FROM passwords WHERE ID_ROLE=" + m_idRolePass;

            if (m_idPass > 0)
                strRes += " AND ID =" + m_idPass;
            else
                ;

            return strRes;
        }

        private bool GetPassResponse(DataTable table)
        {
            if (table.Rows.Count != 0)
                try
                {
                    if (table.Rows[0][0] is System.DBNull)
                        passReceive = "";
                    else
                        passReceive = (string)table.Rows[0][0];
                }
                catch
                {
                    return false;
                }
            else
                passReceive = null;

            return true;
        }

        private string SetPassRequest(string password, bool insert)
        {
            string query = string.Empty;

            if (insert)
                query = "INSERT INTO passwords (ID_EXT, ID_ROLE, HASH) VALUES (" + m_idRolePass +  ", " + m_idRolePass + ", '" + password + "')";
            else
            {
                query = "UPDATE passwords SET HASH='" + password + "'";
                query += " WHERE ID_EXT=" + m_idPass + " AND ID_ROLE=" + m_idRolePass;
            }

            return query;
        }
    }
}
