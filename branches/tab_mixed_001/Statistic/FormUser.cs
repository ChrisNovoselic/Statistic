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

        private ConnectionSettings m_connectionSetttings;
        DataTable m_users_origin;
        DataTable m_users_edit;
        //DataRow [] m_userRows;
        Control [] m_arUIControl;
        INDEX_UICONTROL m_curIndexUIControl;

        List <int> m_listUserID,
                    m_listRolesID,
                    m_listTECID;

        int m_prevComboBoxAccessSelectedIndex;

        public FormUser(ConnectionSettings connSett)
        {
            InitializeComponent();

            m_connectionSetttings = connSett;

            m_listUserID = new List<int> ();

            m_arUIControl = new Control[] { textBoxUserDesc, maskedTextBoxIP, textBoxDomain, textBoxUserName, textBoxComputerName };
            m_curIndexUIControl = INDEX_UICONTROL.COUNT_INDEX_UICONTROL;

            int err = 0,
                i = -1;

            InitTEC.m_connConfigDB = DbTSQLInterface.GetConnection(connSett, out err);

            Users.GetUsers(InitTEC.m_connConfigDB, @"", @"DESCRIPTION", out m_users_origin, out err);
            m_users_edit = m_users_origin.Copy ();
            //m_userRows = m_users_edit.Select();

            //for (i = 0; i < m_userRows.Length; i++)
            for (i = 0; i < m_users_edit.Rows.Count; i++)
            {
                //listBoxUsers.Items.Add(m_userRows[i]["DESCRIPTION"].ToString());
                dgvUsers.Rows.Add();
                dgvUsers.Rows[i].Cells[0].Value = m_users_edit.Rows[i]["DESCRIPTION"].ToString();
                m_listUserID.Add(Convert.ToInt32(m_users_edit.Rows[i]["ID"]));
            }

            comboBoxRole.SelectedIndexChanged -= comboBoxRole_SelectedIndexChanged;

            m_listRolesID = new List<int>();
            DataTable roles;
            //roles = DbTSQLInterface.Select(m_connectionSetttings, "SELECT * FROM roles WHERE ID < 500", out err);
            Users.GetRoles(InitTEC.m_connConfigDB, @"", @"DESCRIPTION", out roles, out err);
            for (i = 0; i < roles.Rows.Count; i++)
            {
                m_listRolesID.Add(Convert.ToInt32 (roles.Rows[i]["ID"]));
                comboBoxRole.Items.Add(roles.Rows[i]["DESCRIPTION"]);
            }

            comboBoxRole.SelectedIndexChanged += comboBoxRole_SelectedIndexChanged;

            m_prevComboBoxAccessSelectedIndex = -1;

            comboBoxAccess.SelectionChangeCommitted -= comboBoxAccess_SelectionChangeCommitted;

            m_listTECID = new List<int>();
            DataTable tec = InitTEC.getListTEC (true, out err); //Игнорировать столбец 'InUse' - использовать

            m_listTECID.Add(0);
            comboBoxAccess.Items.Add("Все станции");

            for (i = 0; i < tec.Rows.Count; i++)
            {
                m_listTECID.Add(Convert.ToInt32(tec.Rows[i]["ID"]));
                comboBoxAccess.Items.Add(tec.Rows[i]["NAME_SHR"]);
            }

            comboBoxAccess.SelectionChangeCommitted += comboBoxAccess_SelectionChangeCommitted;

            if (dgvUsers.Rows.Count > 0)
            {
                //dgvUsers_RowSelectedChanged(0);
                dgvUsers.Rows[0].Selected = true;
            }
            else
                ;

            buttonUserAdd.Enabled = false;

            dgvUsers.RowEnter +=new DataGridViewCellEventHandler(dgvUsers_RowEnter);

            DbTSQLInterface.CloseConnection(InitTEC.m_connConfigDB, out err);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            int err = 0;

            DbTSQLInterface.RecUpdateInsertDelete(m_connectionSetttings, "users", m_users_origin, m_users_edit, out err);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close ();
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
                    case "ID":
                        val = DbTSQLInterface.getIdNext(m_connectionSetttings, "users");
                        break;
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
                        val = "010.100.255.255";
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

            dgvUsers.Rows.Add();
            dgvUsers.Rows[dgvUsers.Rows.Count - 1].Cells[0].Value = textBoxUserDesc.Text;

            //DataRow row = new DataRow ();
            //row.ItemArray = addRow;
            //m_users_edit.Rows.InsertAt(row, listBoxUsers.SelectedIndex + 1);

            m_users_edit.Rows.Add(addRow);

            //m_userRows = m_users_edit.Select();

            //dgvUsers_RowSelectedChanged(dgvUsers.Rows.Count - 1); //listBoxUsers.Items.Count - 1; //m_users_edit.Rows.Count - 1 
            //dgvUsers.Rows[dgvUsers.Rows.Count - 1].Selected = true;
            dgvUsers_RowEnter(null, new DataGridViewCellEventArgs(0, dgvUsers.Rows.Count - 1));
            dgvUsers.FirstDisplayedScrollingRowIndex = dgvUsers.Rows.Count - 1;

            textBoxUserDesc.Clear();
            buttonUserAdd.Enabled = false;
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            string name_field = string.Empty;
            
            if ((!(m_curIndexUIControl < 0)) && (m_curIndexUIControl < INDEX_UICONTROL.COUNT_INDEX_UICONTROL))
            {
                int indx_sel = dgvUsers.SelectedRows [0].Index;

                switch (m_curIndexUIControl) {
                    case INDEX_UICONTROL.TEXTBOX_DESCRIPTION:
                        if (m_arUIControl[(int)m_curIndexUIControl].Text.Length > 0)
                            buttonUserAdd.Enabled = true;
                        else
                            buttonUserAdd.Enabled = false;
                        break;
                    case INDEX_UICONTROL.TEXTBOX_IP:
                        name_field = "IP";
                        m_users_edit.Rows[indx_sel][name_field] = m_arUIControl[(int)m_curIndexUIControl].Text;
                        break;
                    case INDEX_UICONTROL.TEXTBOX_DOMAIN:
                    case INDEX_UICONTROL.TEXTBOX_USERNAME:
                        name_field = "DOMAIN_NAME";
                        m_users_edit.Rows[indx_sel][name_field] = m_arUIControl[(int)INDEX_UICONTROL.TEXTBOX_DOMAIN].Text + @"\\" +
                                                                    m_arUIControl[(int)INDEX_UICONTROL.TEXTBOX_USERNAME].Text;
                        break;
                    case INDEX_UICONTROL.TEXTBOX_COMPUTERNAME:
                        name_field = "COMPUTER_NAME";
                        m_users_edit.Rows[indx_sel][name_field] = m_arUIControl[(int)m_curIndexUIControl].Text;
                        break;
                    default:
                        break;
                }

                if (name_field.Equals (string.Empty) == false)
                {                    
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
            //for (i = INDEX_UICONTROL.TEXTBOX_IP; i < INDEX_UICONTROL.COUNT_INDEX_UICONTROL; i++)
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

        //private void dgvUsers_RowSelectedChanged(int indx)
        //{
        //    dgvUsers.Rows[indx].Selected = true;

        //    comboBoxRole.SelectedIndexChanged -= comboBoxRole_SelectedIndexChanged;

        //    comboBoxRole.SelectedIndex = m_listRolesID.IndexOf(Convert.ToInt32(m_users_edit.Rows[indx]["ID_ROLE"]));

        //    //if (dgvUsers.SelectedRows.Count > 0)
        //    //{
        //        textBoxComputerName.Text = m_users_edit.Rows[indx]["COMPUTER_NAME"].ToString();

        //        string domain_name_full = m_users_edit.Rows[indx]["DOMAIN_NAME"].ToString();
        //        if (!(domain_name_full.IndexOf('\\') < 0))
        //        {
        //            textBoxUserName.Text = domain_name_full.Substring(domain_name_full.IndexOf('\\') + 1);
        //            textBoxDomain.Text = domain_name_full.Substring(0, domain_name_full.IndexOf('\\'));
        //        }
        //        else
        //        {
        //            textBoxUserName.Text = domain_name_full;
        //            textBoxDomain.Text = domain_name_full;
        //        }

        //        maskedTextBoxIP.Clear();    

        //        string [] ip_parts = m_users_edit.Rows[indx]["IP"].ToString().Split ('.');
        //        string ip = string.Empty;
        //        int i = -1;
        //        for (i = 0; i < ip_parts.Length; i ++ )
        //        {
        //            while (ip_parts [i].Length < 3)
        //            {
        //                ip_parts[i] = "0" + ip_parts[i];
        //            }

        //            ip += ip_parts[i];
        //            if (i + 1 < ip_parts.Length) ip += '.'; else ;
        //        }
        //        maskedTextBoxIP.Text = ip;

        //        //textBoxUserDesc.Text = dgvUsers.SelectedRows[0].Cells[0].Value.ToString();
        //    //}
        //    //else
        //    //    ;

        //    comboBoxRole.SelectedIndexChanged += comboBoxRole_SelectedIndexChanged;
        //}

        private void dgvUsers_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            //dgvUsers_RowSelectedChanged(e.RowIndex);

            dgvUsers.Rows[e.RowIndex].Selected = true;

            comboBoxRole.SelectedIndexChanged -= comboBoxRole_SelectedIndexChanged;
            comboBoxAccess.SelectionChangeCommitted -= comboBoxAccess_SelectionChangeCommitted;

            comboBoxRole.SelectedIndex = m_listRolesID.IndexOf(Convert.ToInt32(m_users_edit.Rows[e.RowIndex]["ID_ROLE"]));
            comboBoxAccess.SelectedIndex = m_listTECID.IndexOf(Convert.ToInt32(m_users_edit.Rows[e.RowIndex]["ID_TEC"]));
            if ((comboBoxAccess.SelectedIndex == 0) && (m_listRolesID[comboBoxRole.SelectedIndex] < 3))
                comboBoxAccess.Enabled = false;
            else
                comboBoxAccess.Enabled = true;

            if (m_prevComboBoxAccessSelectedIndex < 0)
                m_prevComboBoxAccessSelectedIndex = comboBoxAccess.SelectedIndex;
            else
                ;

            //if (dgvUsers.SelectedRows.Count > 0)
            //{
                textBoxComputerName.Text = m_users_edit.Rows[e.RowIndex]["COMPUTER_NAME"].ToString();

                string domain_name_full = m_users_edit.Rows[e.RowIndex]["DOMAIN_NAME"].ToString();
                if (!(domain_name_full.IndexOf('\\') < 0))
                {
                    textBoxUserName.Text = domain_name_full.Substring(domain_name_full.IndexOf('\\') + 1);
                    textBoxDomain.Text = domain_name_full.Substring(0, domain_name_full.IndexOf('\\'));
                }
                else
                {
                    textBoxUserName.Text = domain_name_full;
                    textBoxDomain.Text = domain_name_full;
                }

                maskedTextBoxIP.Clear();

                string[] ip_parts = m_users_edit.Rows[e.RowIndex]["IP"].ToString().Split('.');
                string ip = string.Empty;
                int i = -1;
                for (i = 0; i < ip_parts.Length; i++)
                {
                    while (ip_parts[i].Length < 3)
                    {
                        ip_parts[i] = "0" + ip_parts[i];
                    }

                    ip += ip_parts[i];
                    if (i + 1 < ip_parts.Length) ip += '.'; else ;
                }
                maskedTextBoxIP.Text = ip;

                //textBoxUserDesc.Text = dgvUsers.SelectedRows[0].Cells[0].Value.ToString();
            //}
            //else
            //    ;

            comboBoxRole.SelectedIndexChanged += comboBoxRole_SelectedIndexChanged;
            comboBoxAccess.SelectionChangeCommitted += comboBoxAccess_SelectionChangeCommitted;
        }

        private void dgvUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 0:
                    //dgvUsers_RowSelectedChanged(e.RowIndex);
                    break;
                case 1:
                    m_users_edit.Rows.RemoveAt(e.RowIndex);

                    //m_userRows = m_users_edit.Select();

                    int indx_sel = e.RowIndex;
                    dgvUsers.Rows.RemoveAt(indx_sel);
                    if (indx_sel == 0)
                        //dgvUsers_RowSelectedChanged(0);
                        dgvUsers.Rows[0].Selected = true;
                    else
                        //dgvUsers_RowSelectedChanged(indx_sel - 1);
                        dgvUsers.Rows[indx_sel - 1].Selected = true;
                    break;
                default:
                    break;
            }
        }

        private void dgvUsers_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
                m_users_edit.Rows[e.RowIndex]["DESCRIPTION"] = dgvUsers.Rows[e.RowIndex].Cells[0].Value;
            else
                ;
        }

        private void comboBoxRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_users_edit.Rows[dgvUsers.SelectedRows [0].Index]["ID_ROLE"] = m_listRolesID [comboBoxRole.SelectedIndex];
            if (m_listRolesID[comboBoxRole.SelectedIndex] > 100)
            {
                if (comboBoxAccess.Enabled == false) comboBoxAccess.Enabled = true; else ;
                if (comboBoxAccess.SelectedIndex == 0) comboBoxAccess.SelectedIndex = 1; else ;

                comboBoxAccess_SelectionChangeCommitted(null, EventArgs.Empty);
            }
            else
            {
                comboBoxAccess.SelectedIndex = 0;
                if (comboBoxAccess.Enabled == true) comboBoxAccess.Enabled = false; else ;
            }
        }

        private void comboBoxAccess_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (((m_listRolesID[comboBoxRole.SelectedIndex] > 100) && (comboBoxAccess.SelectedIndex > 0)) ||
                (m_listRolesID[comboBoxRole.SelectedIndex] < 100))
            {
                m_users_edit.Rows[dgvUsers.SelectedRows[0].Index]["ID_TEC"] = m_listTECID[comboBoxAccess.SelectedIndex];

                m_prevComboBoxAccessSelectedIndex = comboBoxAccess.SelectedIndex;
            }
            else
            {
                //Недопустимо для ДИС... назначить все станции
                comboBoxAccess.SelectedIndex = m_prevComboBoxAccessSelectedIndex;
            }
        }
    }
}
