using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;

namespace Statistic
{
    public partial class FormUser : Form
    {
        private ConnectionSettings m_connSett;
        DataTable m_users_origin;
        DataTable m_users_edit;
        DataRow [] m_userRows;

        List <int> m_listUserID;

        public FormUser(ConnectionSettings connSett)
        {
            InitializeComponent();

            m_connSett = connSett;

            m_listUserID = new List<int> ();

            int err = 0,
                i = -1;

            Users.GetUsers(m_connSett, "", out m_users_origin, out err);
            m_users_edit = m_users_origin.Copy ();
            m_userRows = m_users_edit.Select();

            for (i = 0; i < m_userRows.Length; i++)
            {
                listBoxUsers.Items.Add(m_userRows[i]["DESCRIPTION"].ToString());
                m_listUserID.Add(Convert.ToInt32(m_userRows[i]["ID"]));
            }

            if (listBoxUsers.Items.Count > 0)
            {
                listBoxUsers.SelectedIndex = 0;
            }
            else
                ;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close ();
        }

        private void listBoxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(listBoxUsers.SelectedIndex < 0))
            {
                textBoxComputerName.Text = m_userRows[listBoxUsers.SelectedIndex]["COMPUTER_NAME"].ToString();
                string domain_name_full = m_userRows[listBoxUsers.SelectedIndex]["DOMAIN_NAME"].ToString();
                textBoxUserName.Text = domain_name_full.Substring (domain_name_full.IndexOf ('\\') + 1);
                textBoxDomain.Text = domain_name_full.Substring(0, domain_name_full.IndexOf('\\'));
                textBoxIP.Text = m_userRows[listBoxUsers.SelectedIndex]["IP"].ToString();
                textBoxUserDesc.Text = listBoxUsers.Text;
            }
            else
                ;
        }

        private void buttonUserDel_Click(object sender, EventArgs e)
        {
            m_users_edit.Rows.RemoveAt (listBoxUsers.SelectedIndex);

            m_userRows = m_users_edit.Select();

            int indx_sel = listBoxUsers.SelectedIndex;
            listBoxUsers.Items.RemoveAt(indx_sel);
            if (indx_sel == 0)
                listBoxUsers.SelectedIndex = 0;
            else
                listBoxUsers.SelectedIndex = indx_sel - 1;
        }

        private void buttonUserAdd_Click(object sender, EventArgs e)
        {
            int i = -1;
            object [] addRow = new object [m_users_edit.Columns.Count];
            object val;

            for (i = 0; i < m_users_edit.Columns.Count; i ++)
            {
                switch (m_users_edit.Columns [i].ColumnName)
                {
                    case "DESCRIPTION":
                        val = textBoxUserDesc.Text;
                        break;
                    case "COMPUTER_NAME":
                        val = "neNNNN.ne.ru";
                        break;
                    case "DOMAIN_NAME":
                        val = "NE\\FamilyIO";
                        break;
                    case "IP":
                        val = "255.255.255.255";
                        break;
                    case "ID_ROLE":
                        val = 3;
                        break;
                    case "ID_TEC":
                        val = 0;
                        break;
                    default:
                        val = null;
                        break;
                }

                addRow[m_users_edit.Columns[i].Ordinal] = val;
            }

            listBoxUsers.Items.Add(textBoxUserDesc.Text);

            //DataRow row = new DataRow ();
            //row.ItemArray = addRow;
            //m_users_edit.Rows.InsertAt(row, listBoxUsers.SelectedIndex + 1);

            m_users_edit.Rows.Add(addRow);

            m_userRows = m_users_edit.Select();

            listBoxUsers.SelectedIndex = m_userRows.Length - 1; //listBoxUsers.Items.Count - 1; //m_users_edit.Rows.Count - 1 
        }
    }
}
