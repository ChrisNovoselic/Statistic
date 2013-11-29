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
        private enum INDEX_UICONTROL { TEXTBOX_DESCRIPTION, TEXTBOX_IP, TEXTBOX_DOMAIN, TEXTBOX_USERNAME, TEXTBOX_COMPUTERNAME, COUNT_INDEX_UICONTROL };
        
        private ConnectionSettings m_connSett;
        DataTable m_users_origin;
        DataTable m_users_edit;
        DataRow [] m_userRows;
        Control [] m_arUIControl;
        INDEX_UICONTROL m_curIndexUIControl;

        List <int> m_listUserID;

        public FormUser(ConnectionSettings connSett)
        {
            InitializeComponent();

            m_connSett = connSett;

            m_listUserID = new List<int> ();

            m_arUIControl = new Control [] { textBoxUserDesc, textBoxIP, textBoxDomain, textBoxUserName, textBoxComputerName};
            m_curIndexUIControl = INDEX_UICONTROL.COUNT_INDEX_UICONTROL;

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

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            string name_field = string.Empty;
            
            if ((!(m_curIndexUIControl < 0)) && (m_curIndexUIControl < INDEX_UICONTROL.COUNT_INDEX_UICONTROL))
            {
                int indx_sel = listBoxUsers.SelectedIndex;

                switch (m_curIndexUIControl) {
                    case INDEX_UICONTROL.TEXTBOX_DESCRIPTION:
                        //listBoxUsers.SelectedItem = m_arUIControl[(int)m_curIndexUIControl].Text;
                        listBoxUsers.Items.RemoveAt(indx_sel);
                        listBoxUsers.Items.Insert(indx_sel, m_arUIControl[(int)m_curIndexUIControl].Text);
                        listBoxUsers.SelectedIndex = indx_sel;

                        name_field = "DESCRIPTION";
                        break;
                    case INDEX_UICONTROL.TEXTBOX_IP:
                        name_field = "IP";
                        break;
                    case INDEX_UICONTROL.TEXTBOX_DOMAIN:
                        name_field = "DOMAIN_NAME";
                        break;
                    case INDEX_UICONTROL.TEXTBOX_USERNAME:
                        name_field = "USER_NAME";
                        break;
                    case INDEX_UICONTROL.TEXTBOX_COMPUTERNAME:
                        name_field = "COMPUTER_NAME";
                        break;
                    default:
                        break;
                }

                if (name_field.Equals (string.Empty) == false)
                {
                    m_userRows[indx_sel][name_field] = m_arUIControl[(int)m_curIndexUIControl].Text;
                }
                else
                    ;
            }
            else
                ;
        }

        private void textBox_Enter(object sender, EventArgs e)
        {
            INDEX_UICONTROL i = INDEX_UICONTROL.COUNT_INDEX_UICONTROL;
            
            for (i = INDEX_UICONTROL.TEXTBOX_DESCRIPTION; i < INDEX_UICONTROL.COUNT_INDEX_UICONTROL; i ++)
            {
                if (sender.Equals (m_arUIControl [(int)i])) {
                    m_curIndexUIControl = i;
                    break;
                }
                else
                    ;
            }
        }

        private void textBox_Leave(object sender, EventArgs e)
        {
            m_curIndexUIControl = INDEX_UICONTROL.COUNT_INDEX_UICONTROL;
        }
    }
}
