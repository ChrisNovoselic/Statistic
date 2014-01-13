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
        private MD5CryptoServiceProvider md5;

        private DelegateFunc delegateStartWait;
        private DelegateFunc delegateStopWait;
        private DelegateFunc delegateEventUpdate;

        private DelegateStringFunc errorReport;
        private DelegateStringFunc actionReport;

        private string getOwnerPass()
        {
            string[] ownersPass = { "диспетчера", "администратора", "НССа" };

            return ownersPass[m_idPass - 1];
        }

        private bool is_connection_error;
        private bool is_data_error;

        public volatile string last_error;
        public DateTime last_time_error;
        public volatile bool errored_state;

        public volatile string last_action;
        public DateTime last_time_action;
        public volatile bool actioned_state;

        private volatile HAdmin.Errors passResult;
        private volatile string passReceive;
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

            is_data_error = is_connection_error = false;

            m_lockObj = new Object();
        }

        public void SetDelegateWait(DelegateFunc dStart, DelegateFunc dStop, DelegateFunc dStatus)
        {
            this.delegateStartWait = dStart;
            this.delegateStopWait = dStop;
            this.delegateEventUpdate = dStatus;
        }

        public void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fact)
        {
            this.errorReport = ferr;
            this.actionReport = fact;
        }

        void MessageBox(string msg, MessageBoxButtons btn = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Error)
        {
            //MessageBox.Show(this, msg, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            Logging.Logg().LogLock();
            Logging.Logg().LogToFile(msg, true, true, false);
            Logging.Logg().LogUnlock();
        }

        private void GetPassword(uint id, out int er)
        {
            DataTable passTable = DbTSQLInterface.Select(connSettConfigDB, GetPassRequest(m_idPass), out er);
            if ((er == 0) && (!(passTable.Rows[0][0] is DBNull)))
            {
                passReceive = passTable.Rows[0][0].ToString();
            }
            else
            {
                passResult = HAdmin.Errors.NoAccess;
            }
        }

        public bool SetPassword(string password, uint idPass)
        {
            int err = -1;
            passResult = HAdmin.Errors.NoError;

            m_idPass = idPass;

            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

            StringBuilder hashedString = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                hashedString.Append(hash[i].ToString("x2"));

            delegateStartWait();
            GetPassword(idPass, out err);

            if (!(passResult == HAdmin.Errors.NoError))
            {
                delegateStopWait();

                //MessageBox.Show(this, "Ошибка получения пароля " + getOwnerPass () + ". Пароль не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Ошибка получения пароля " + getOwnerPass() + ". Пароль не сохранён.");

                return false;
            }
            else
                ;

            if (passReceive == null)
                DbTSQLInterface.ExecNonQuery(connSettConfigDB, SetPassRequest(hashedString.ToString(), idPass, true), out err);
            else
                DbTSQLInterface.ExecNonQuery(connSettConfigDB, SetPassRequest(hashedString.ToString(), idPass, false), out err);

            delegateStopWait();

            if (passResult != HAdmin.Errors.NoError)
            {
                //MessageBox.Show(this, "Ошибка сохранения пароля " + getOwnerPass () + ". Пароль не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Ошибка сохранения пароля " + getOwnerPass() + ". Пароль не сохранён.");
                return false;
            }

            return true;
        }

        public HAdmin.Errors ComparePassword(string password, uint id)
        {
            int err = -1;
            passResult = HAdmin.Errors.NoError;
            passReceive = null;

            //if (connSettConfigDB == null)
            //    return HAdmin.Errors.NoAccess;
            //else
            //    ;

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

            delegateStartWait();
            GetPassword(id, out err);

            delegateStopWait();
            if (!(passResult == HAdmin.Errors.NoError))
            {
                //MessageBox.Show(this, "Ошибка получения пароля " + getOwnerPass () + ".", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("Ошибка получения пароля " + getOwnerPass() + ".");

                return HAdmin.Errors.ParseError;
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

        private string GetPassRequest(uint id)
        {
            return "SELECT HASH FROM passwords WHERE ID_ROLE=" + id;
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

        private string SetPassRequest(string password, uint id, bool insert)
        {
            string query = string.Empty;

            if (insert)
                switch (m_idPass)
                {
                    case 1:
                        query = "INSERT INTO passwords (ID_ROLE, HASH) VALUES (" + id + ", '" + password + "')";
                        break;
                    case 2:
                        query = "INSERT INTO passwords (ID_ROLE, HASH) VALUES (" + id + ", '" + password + "')";
                        break;
                    case 3:
                        query = "INSERT INTO passwords (ID_ROLE, HASH) VALUES (" + id + ", '" + password + "')";
                        break;
                    default:
                        break;
                }
            else
            {
                switch (m_idPass)
                {
                    case 1:
                        query = "UPDATE passwords SET HASH='" + password + "'";
                        break;
                    case 2:
                        query = "UPDATE passwords SET HASH='" + password + "'";
                        break;
                    case 3:
                        query = "UPDATE passwords SET HASH='" + password + "'";
                        break;
                    default:
                        break;
                }

                query += " WHERE ID_USER=ID_ROLE AND ID_ROLE=" + id;
            }

            return query;
        }

        private void ErrorReport(string error_string)
        {
            last_error = error_string;
            last_time_error = DateTime.Now;
            errored_state = true;

            errorReport(error_string);
        }

        private void ActionReport(string action_string)
        {
            last_action = action_string;
            last_time_action = DateTime.Now;
            actioned_state = true;

            //stsStrip.BeginInvoke(delegateEventUpdate);
            //delegateEventUpdate ();
            actionReport(action_string);
        }
    }
}
