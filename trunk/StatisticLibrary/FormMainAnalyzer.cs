using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;

using System.IO;

namespace StatisticCommon
{
    public partial class FormMainAnalyzer : Form //: FormMainBaseWithStatusStrip
    {
        TCPClient m_tcpClient;

        DataTable m_tableUsers
                    , m_tableRoles;

        FileInfo m_fi;
        StreamReader m_sr;

        public FormMainAnalyzer(ConnectionSettings connSett)
        {
            InitializeComponent();
            /*
            //При наследовании от ''
            // m_statusStripMain
            this.m_statusStripMain.Location = new System.Drawing.Point(0, 546);
            this.m_statusStripMain.Size = new System.Drawing.Size(841, 22);
            // m_lblMainState
            this.m_lblMainState.Size = new System.Drawing.Size(166, 17);
            // m_lblDateError
            this.m_lblDateError.Size = new System.Drawing.Size(166, 17);
            // m_lblDescError
            this.m_lblDescError.Size = new System.Drawing.Size(463, 17);
            */

            m_tcpClient = new TCPClient();
            
            dgvFilterActives.Rows.Add (2);
            dgvFilterActives.Rows[0].Cells[0].Value = true; dgvFilterActives.Rows[0].Cells[1].Value = "Активные";
            dgvFilterActives.Rows[1].Cells[0].Value = true; dgvFilterActives.Rows[1].Cells[1].Value = "Не активные";
            dgvFilterActives.Enabled = false;

            int err = -1;
            
            MySql.Data.MySqlClient.MySqlConnection connDB = DbTSQLInterface.GetConnection (DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.MySQL, connSett, out err);

            Users.GetRoles(connDB, string.Empty, string.Empty, out m_tableRoles, out err);
            FillDataGridViews(ref dgvFilterRoles, m_tableRoles, "DESCRIPTION", err, true);
            
            Users.GetUsers(connDB, string.Empty, @"DESCRIPTION", out m_tableUsers, out err);
            FillDataGridViews(ref dgvClient, m_tableUsers, "DESCRIPTION", err);

            m_tcpClient.Init("ne1150.ne.ru");

            DbTSQLInterface.CloseConnection (connDB, out err);
        }

        private void FormMainAnalyzer_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_tcpClient.Close();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close ();
        }

        private void FillDataGridViews(ref DataGridView ctrl, DataTable src, string nameField, int run, bool checkDefault = false)
        {
            if (run == 0)
            {
                bool bCheckedItem = checkDefault;
                ctrl.Rows.Clear ();
                ctrl.Rows.Add (src.Rows.Count);

                for (int i = 0; i < src.Rows.Count; i ++)
                {
                    //Проверка активности
                    //m_tcpSender.Init(m_tableUsers.Rows[i]["COMPUTER_NAME"].ToString ());
                    //bCheckedItem = m_tcpSender.Connected;
                    //m_tcpSender.Close ();

                    ctrl.Rows[i].Cells[0].Value = bCheckedItem;
                    ctrl.Rows[i].Cells[1].Value = src.Rows[i]["DESCRIPTION"].ToString ();
                }
            }
            else
                ;
        }

        /*
        //При наследовании от ''
        protected override bool UpdateStatusString()
        {
            bool have_eror = true;

            return have_eror;
        }
        */
    }
}
