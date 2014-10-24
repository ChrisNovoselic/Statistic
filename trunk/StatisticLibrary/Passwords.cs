using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
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
            string[] ownersPass = { "����������", "��������������", "����" };

            return ownersPass[id_role - 1];
        }

        private volatile HAdmin.Errors passResult;
        private volatile string passReceive;
        private volatile uint m_idRolePass;
        private volatile uint m_idExtPass;
        private Object m_lockObj;

        private int m_idListener;

        public Passwords()
        {
            Initialize();
        }

        private void Initialize()
        {
            md5 = new MD5CryptoServiceProvider();

            m_lockObj = new Object();
        }

        public void SetIdListener (int idListener) { m_idListener = idListener; }

        void MessageBox(string msg, MessageBoxButtons btn = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Error)
        {
            //MessageBox.Show(this, msg, "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Logging.Logg().Error(msg);
        }

        private void GetPassword(out int er)
        {
            DbConnection conn = DbSources.Sources ().GetConnection (m_idListener, out er);
            DataTable passTable = DbTSQLInterface.Select(ref conn, GetPassRequest(), null, null, out er);
            if (er == 0)
                if (!(passTable.Rows[0][0] is DBNull))
                    passReceive = passTable.Rows[0][0].ToString();
                else
                    passResult = HAdmin.Errors.ParseError;
            else
                passResult = HAdmin.Errors.NoAccess;
        }

        public bool SetPassword(string password, uint idExtPass, uint idRolePass)
        {
            int err = -1;
            passResult = HAdmin.Errors.NoError;

            m_idExtPass = idExtPass;
            m_idRolePass = idRolePass;

            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

            StringBuilder hashedString = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                hashedString.Append(hash[i].ToString("x2"));

            GetPassword(out err);

            if (!(passResult == HAdmin.Errors.NoError))
            {
                //MessageBox.Show(this, "������ ��������� ������ " + getOwnerPass () + ". ������ �� �������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("������ ��������� ������ " + getOwnerPass((int)m_idRolePass) + ". ������ �� �������.");

                return false;
            }
            else
                ;

            DbConnection conn = DbSources.Sources().GetConnection(m_idListener, out err);
            if (passReceive == null)
                DbTSQLInterface.ExecNonQuery(ref conn, SetPassRequest(hashedString.ToString(), true), null, null,out err);
            else
                DbTSQLInterface.ExecNonQuery(ref conn, SetPassRequest(hashedString.ToString(), false), null, null, out err);

            if (passResult != HAdmin.Errors.NoError)
            {
                //MessageBox.Show(this, "������ ���������� ������ " + getOwnerPass () + ". ������ �� �������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("������ ���������� ������ " + getOwnerPass((int)m_idRolePass) + ". ������ �� �������.");
                return false;
            }

            return true;
        }

        public HAdmin.Errors ComparePassword(string password, uint id_ext, uint id_role)
        {
            int err = -1;
            passResult = HAdmin.Errors.NoError;
            passReceive = null;

            //if (connSettConfigDB == null)
            //    return HAdmin.Errors.NoAccess;
            //else
            //    ;

            m_idExtPass = id_ext;
            m_idRolePass = id_role;

            if (password.Length < 1)
            {
                //MessageBox.Show(this, "����� ������ ������ ����������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("����� ������ ������ ����������.");

                return HAdmin.Errors.InvalidValue;
            }
            else
                ;

            string hashFromForm = "";
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

            StringBuilder hashedString = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                hashedString.Append(hash[i].ToString("x2"));

            GetPassword(out err);

            if (!(passResult == HAdmin.Errors.NoError))
            {
                //MessageBox.Show(this, "������ ��������� ������ " + getOwnerPass () + ".", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("������ ��������� ������ " + getOwnerPass((int)m_idRolePass) + ".");

                return passResult;
            }
            else
                ;

            if (passReceive == null)
            {
                //MessageBox.Show(this, "������ �� ����������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox("������ �� ����������.");

                return HAdmin.Errors.NoSet;
            }
            else
            {
                hashFromForm = hashedString.ToString();

                if (hashFromForm != passReceive)
                {
                    //MessageBox.Show(this, "������ ����� �������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox("������ ����� �������.");

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

            if (! (m_idExtPass < 0))
                strRes += " AND ID_EXT =" + m_idExtPass;
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
                query = "INSERT INTO passwords (ID_EXT, ID_ROLE, HASH) VALUES (" + m_idExtPass +  ", " + m_idRolePass + ", '" + password + "')";
            else
            {
                query = "UPDATE passwords SET HASH='" + password + "'";
                query += " WHERE ID_EXT=" + m_idExtPass + " AND ID_ROLE=" + m_idRolePass;
            }

            return query;
        }
    }
}
