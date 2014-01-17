using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;

//using Statistic;

namespace StatisticCommon
{
    public delegate void DelegateReadConnSettFunc(out List <ConnectionSettings> listConnSett, out int err, out string mes);
    public delegate void DelegateSaveConnSettFunc(List<ConnectionSettings> listConnSett, out int err);

    public partial class FormConnectionSettings : Form
    {
        private List<ConnectionSettings> m_connectionSettingsEdit, m_connectionSettings;
        int m_iReady;

        private bool closing;
        private int oldSelectedIndex;

        DelegateSaveConnSettFunc SaveSettings;
        DelegateReadConnSettFunc ReadSettings;

        //public FormConnectionSettings(List<TEC> tec)
        public FormConnectionSettings(DelegateReadConnSettFunc r, DelegateSaveConnSettFunc s)
        {
            int i = -1;
            
            InitializeComponent();

            ReadSettings = r;
            SaveSettings = s;

            //this.tec = tec;
            m_connectionSettingsEdit = new List<ConnectionSettings>();
            m_connectionSettings = new List<ConnectionSettings>();

            cbxConnFor.DropDownStyle = ComboBoxStyle.DropDownList;

            cbxConnFor.Items.Add("�� ������������");

            closing = false;

            string msgErr = string.Empty;
            ReadSettings (out m_connectionSettingsEdit, out m_iReady, out msgErr);
            if ((!(m_iReady == 0)) && (msgErr.Length > 0)) MessageBox.Show(this, msgErr, "������", MessageBoxButtons.OK, MessageBoxIcon.Error); else ;

            if (!(m_iReady == 0))
            {
                addConnSett(new ConnectionSettings());
            }
            else
            {
                for (i = 0; i < m_connectionSettingsEdit.Count; i ++)
                {
                    addConnSett (i);
                }

                tbxServer.Text =
                m_connectionSettings[0].server =
                m_connectionSettingsEdit[0].server;
                nudnPort.Value =
                m_connectionSettings[0].port =
                m_connectionSettingsEdit[0].port;
                tbxDataBase.Text =
                m_connectionSettings[0].dbName = 
                m_connectionSettingsEdit[0].dbName;
                tbxUserId.Text =
                m_connectionSettings[0].userName =
                m_connectionSettingsEdit[0].userName;
                mtbxPass.Text =
                m_connectionSettings[0].password =
                m_connectionSettingsEdit[0].password;
                cbxIgnore.Checked =
                m_connectionSettings[0].ignore =
                m_connectionSettingsEdit[0].ignore;
            }

            cbxConnFor.SelectedIndex = oldSelectedIndex = 0;
        }

        public ConnectionSettings getConnSett(int indx = -1) {
            if (indx < 0)
                indx = cbxConnFor.SelectedIndex;
            else
                ;

            //return m_connectionSettings[m_connectionSettings.Count - 1];
            return m_connectionSettings[indx];
        }

        public void setConnSett(int indx, ConnectionSettings connSett)
        {
            if (indx < m_connectionSettingsEdit.Count)
            {
            }
            else
                addConnSett(connSett);

            SelectedIndex = indx;

            tbxServer.Text = connSett.server;
            tbxDataBase.Text = connSett.dbName;
            nudnPort.Value = connSett.port;
            tbxUserId.Text = connSett.userName;
            mtbxPass.Text = connSett.password;
            cbxIgnore.Checked = connSett.ignore;
        }

        private void SetItemText ()
        {
            int i = m_connectionSettings.Count - 1;
            
            if (m_connectionSettings[i].name.Length > 0)
                if (i < cbxConnFor.Items.Count)
                    cbxConnFor.Items[i] = m_connectionSettings[i].name;
                else
                    cbxConnFor.Items.Add(m_connectionSettings[i].name);
            else
                if (m_connectionSettings.Count > 1)
                    cbxConnFor.Items.Add("�� ������������" + " - ������.(" + (cbxConnFor.Items.Count - 0) + ")");
                else ;

            if ((cbxConnFor.Items.Count > 1) && (cbxConnFor.Enabled == false))
                cbxConnFor.Enabled = true;
            else ;
        }

        public void addConnSett(int indx)
        {
            //if (indx < m_connectionSettingsEdit.Count)
            //{ } else ;

            m_connectionSettings.Add(new ConnectionSettings ());
            
            int i = m_connectionSettings.Count - 1;
            m_connectionSettings[i].name = m_connectionSettingsEdit[indx].name;

            m_connectionSettings[i].server = m_connectionSettingsEdit[indx].server;
            m_connectionSettings[i].port = m_connectionSettingsEdit[indx].port;
            m_connectionSettings[i].dbName = m_connectionSettingsEdit[indx].dbName;
            m_connectionSettings[i].userName = m_connectionSettingsEdit[indx].userName;
            m_connectionSettings[i].password = m_connectionSettingsEdit[indx].password;
            m_connectionSettings[i].ignore = m_connectionSettingsEdit[indx].ignore;

            SetItemText ();
        }

        public void addConnSett(ConnectionSettings connSett)
        {
            m_connectionSettings.Add(connSett);
            m_connectionSettingsEdit.Add(connSett);

            m_connectionSettings[m_connectionSettings.Count - 1].port =
            m_connectionSettingsEdit[m_connectionSettingsEdit.Count - 1].port = 3306;

            SetItemText ();
        }

        public void btnOk_Click(object obj, EventArgs ev)
        {
            ConnectionSettings.ConnectionSettingsError error;

            m_connectionSettingsEdit[cbxConnFor.SelectedIndex].server = tbxServer.Text;
            m_connectionSettingsEdit[cbxConnFor.SelectedIndex].port = (int)nudnPort.Value;
            m_connectionSettingsEdit[cbxConnFor.SelectedIndex].dbName = tbxDataBase.Text;
            m_connectionSettingsEdit[cbxConnFor.SelectedIndex].userName = tbxUserId.Text;
            m_connectionSettingsEdit[cbxConnFor.SelectedIndex].password = mtbxPass.Text;
            m_connectionSettingsEdit[cbxConnFor.SelectedIndex].ignore = cbxIgnore.Checked;

            for (int i = 0; i < m_connectionSettingsEdit.Count; i++ )
            {
                if ((error = m_connectionSettingsEdit[i].Validate()) != ConnectionSettings.ConnectionSettingsError.NoError)
                {
                    string msgError = string.Empty;
                    switch (error)
                    {
                        case ConnectionSettings.ConnectionSettingsError.WrongIp:
                            msgError = "������������ IP-�����";
                            break;
                        case ConnectionSettings.ConnectionSettingsError.WrongPort:
                           msgError = "���� ������ ������ � �������� [0:65535].";
                            break;
                        case ConnectionSettings.ConnectionSettingsError.WrongDbName:
                            msgError = "�� ������ ��� ���� ������.";
                            break;
                        case ConnectionSettings.ConnectionSettingsError.IllegalSymbolDbName:
                            msgError = "������������ ������ � ����� ����.";
                            break;
                        case ConnectionSettings.ConnectionSettingsError.IllegalSymbolUserName:
                            msgError = "������������ ������ � ����� ������������.";
                            break;
                        case ConnectionSettings.ConnectionSettingsError.IllegalSymbolPassword:
                            msgError = "������������ ������ � ������ ������������.";
                            break;
                        case ConnectionSettings.ConnectionSettingsError.NotConnect:
                            msgError = "������������ ������ � ������ ������������.";
                            break;
                    }
                    msgError += "\n��������� ������������ �������� ��� ���������� \"" + cbxConnFor.Items[i].ToString() + "\".";
                    MessageBox.Show(this, msgError, "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    closing = false;
                    return;
                }

                m_connectionSettings[i].server = m_connectionSettingsEdit[i].server;
                m_connectionSettings[i].port = m_connectionSettingsEdit[i].port;
                m_connectionSettings[i].dbName = m_connectionSettingsEdit[i].dbName;
                m_connectionSettings[i].userName = m_connectionSettingsEdit[i].userName;
                m_connectionSettings[i].password = m_connectionSettingsEdit[i].password;
                m_connectionSettings[i].ignore = m_connectionSettingsEdit[i].ignore;
            }

            SaveSettings(m_connectionSettings, out m_iReady);

            closing = true;
            this.DialogResult = DialogResult.Yes;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < m_connectionSettingsEdit.Count; i++)
            {
                m_connectionSettingsEdit[i].server = m_connectionSettings[i].server;
                m_connectionSettingsEdit[i].port = m_connectionSettings[i].port;
                m_connectionSettingsEdit[i].dbName = m_connectionSettings[i].dbName;
                m_connectionSettingsEdit[i].userName = m_connectionSettings[i].userName;
                m_connectionSettingsEdit[i].password = m_connectionSettings[i].password;
                m_connectionSettingsEdit[i].ignore = m_connectionSettings[i].ignore;
            }
            tbxServer.Text = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].server;
            nudnPort.Value = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].port;
            tbxDataBase.Text = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].dbName;
            tbxUserId.Text = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].userName;
            mtbxPass.Text = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].password;
            cbxIgnore.Checked = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].ignore;

            
            closing = true;
            this.DialogResult = DialogResult.No;
            Close();
        }

        public ConnectionSettings ConnectionSettingsEdit {
            get {
                return m_connectionSettingsEdit[cbxConnFor.SelectedIndex];
            }

            set {
                tbxServer.Text = value.server;
                nudnPort.Value = value.port;
                tbxDataBase.Text = value.dbName;
                tbxUserId.Text = value.userName;
                mtbxPass.Text = value.password;
                cbxIgnore.Checked = false; //value.ignore;
            }
        }

        public int SelectedIndex
        {
            get
            {
                return cbxConnFor.SelectedIndex;
            }

            set
            {
                cbxConnFor.SelectedIndex = value;
            }
        }

        private void cbxConnFor_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_connectionSettingsEdit[oldSelectedIndex].server = tbxServer.Text;
            m_connectionSettingsEdit[oldSelectedIndex].port = (int)nudnPort.Value;
            m_connectionSettingsEdit[oldSelectedIndex].dbName = tbxDataBase.Text;
            m_connectionSettingsEdit[oldSelectedIndex].userName = tbxUserId.Text;
            m_connectionSettingsEdit[oldSelectedIndex].password = mtbxPass.Text;
            m_connectionSettingsEdit[oldSelectedIndex].ignore = cbxIgnore.Checked;

            tbxServer.Text = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].server;
            nudnPort.Value = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].port;
            tbxDataBase.Text = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].dbName;
            tbxUserId.Text = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].userName;
            mtbxPass.Text = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].password;
            cbxIgnore.Checked = m_connectionSettingsEdit[cbxConnFor.SelectedIndex].ignore;

            oldSelectedIndex = cbxConnFor.SelectedIndex;

            //if (cbxConnFor.SelectedIndex == cbxConnFor.Items.Count - 1)
                cbxIgnore.Enabled = false;
            //else
            //    cbxIgnore.Enabled = true;
        }

        private void ConnectionSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closing)
                e.Cancel = true;
            else
                closing = false;
        }

        public int Count
        {
            get { return m_connectionSettings.Count; }
            //get { return m_connectionSettingsEdit.Count; }
        }

        private void component_Changed(object sender, EventArgs e)
        {
        }

        public int Ready
        {
            get { return m_iReady; }
        }
    }
}