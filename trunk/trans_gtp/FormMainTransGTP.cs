using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;

namespace trans_gtp
{
    public partial class FormMainTransGTP : FormMainTrans
    {
        System.Windows.Forms.Control[,] m_arUIControlDB;

        private System.Windows.Forms.Label labelSourcePort;
        private System.Windows.Forms.NumericUpDown nudnSourcePort;
        private System.Windows.Forms.Label labelSourcePass;
        private System.Windows.Forms.Label labelSourceUserId;
        private System.Windows.Forms.Label labelSourceDBName;
        private System.Windows.Forms.Label labelSourceServerIP;
        private System.Windows.Forms.MaskedTextBox mtbxSourcePass;
        private System.Windows.Forms.TextBox tbxSourceUserId;
        private System.Windows.Forms.TextBox tbxSourceNameDatabase;
        private System.Windows.Forms.TextBox tbxSourceServerIP;

        public FormMainTransGTP()
        {
            InitializeComponentTransGTP();
        }

        private void InitializeComponentTransGTP()
        {
            
            this.labelSourcePort = new System.Windows.Forms.Label();
            this.nudnSourcePort = new System.Windows.Forms.NumericUpDown();
            this.labelSourcePass = new System.Windows.Forms.Label();
            this.labelSourceUserId = new System.Windows.Forms.Label();
            this.labelSourceDBName = new System.Windows.Forms.Label();
            this.labelSourceServerIP = new System.Windows.Forms.Label();
            this.mtbxSourcePass = new System.Windows.Forms.MaskedTextBox();
            this.tbxSourceUserId = new System.Windows.Forms.TextBox();
            this.tbxSourceNameDatabase = new System.Windows.Forms.TextBox();
            this.tbxSourceServerIP = new System.Windows.Forms.TextBox();

            ((System.ComponentModel.ISupportInitialize)(this.nudnSourcePort)).BeginInit();

            base.groupBoxSource.Controls.Add(this.labelSourcePort);
            this.groupBoxSource.Controls.Add(this.nudnSourcePort);
            this.groupBoxSource.Controls.Add(this.labelSourcePass);
            this.groupBoxSource.Controls.Add(this.labelSourceUserId);
            this.groupBoxSource.Controls.Add(this.labelSourceDBName);
            this.groupBoxSource.Controls.Add(this.labelSourceServerIP);
            this.groupBoxSource.Controls.Add(this.mtbxSourcePass);
            this.groupBoxSource.Controls.Add(this.tbxSourceUserId);
            this.groupBoxSource.Controls.Add(this.tbxSourceNameDatabase);
            this.groupBoxSource.Controls.Add(this.tbxSourceServerIP);

            // 
            // labelSourcePort
            // 
            this.labelSourcePort.AutoSize = true;
            this.labelSourcePort.Location = new System.Drawing.Point(12, 55);
            this.labelSourcePort.Name = "labelSourcePort";
            this.labelSourcePort.Size = new System.Drawing.Size(32, 13);
            this.labelSourcePort.TabIndex = 21;
            this.labelSourcePort.Text = "Порт";
            // 
            // nudnSourcePort
            // 
            this.nudnSourcePort.Enabled = false;
            this.nudnSourcePort.Location = new System.Drawing.Point(129, 53);
            this.nudnSourcePort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudnSourcePort.Name = "nudnSourcePort";
            this.nudnSourcePort.Size = new System.Drawing.Size(69, 20);
            this.nudnSourcePort.TabIndex = 16;
            this.nudnSourcePort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudnSourcePort.Value = new decimal(new int[] {
            3306,
            0,
            0,
            0});
            // 
            // labelSourcePass
            // 
            this.labelSourcePass.AutoSize = true;
            this.labelSourcePass.Location = new System.Drawing.Point(11, 134);
            this.labelSourcePass.Name = "labelSourcePass";
            this.labelSourcePass.Size = new System.Drawing.Size(45, 13);
            this.labelSourcePass.TabIndex = 24;
            this.labelSourcePass.Text = "Пароль";
            // 
            // labelSourceUserId
            // 
            this.labelSourceUserId.AutoSize = true;
            this.labelSourceUserId.Location = new System.Drawing.Point(11, 108);
            this.labelSourceUserId.Name = "labelSourceUserId";
            this.labelSourceUserId.Size = new System.Drawing.Size(103, 13);
            this.labelSourceUserId.TabIndex = 23;
            this.labelSourceUserId.Text = "Имя пользователя";
            // 
            // labelSourceDBName
            // 
            this.labelSourceDBName.AutoSize = true;
            this.labelSourceDBName.Location = new System.Drawing.Point(11, 82);
            this.labelSourceDBName.Name = "labelSourceDBName";
            this.labelSourceDBName.Size = new System.Drawing.Size(98, 13);
            this.labelSourceDBName.TabIndex = 22;
            this.labelSourceDBName.Text = "Имя базы данных";
            // 
            // labelSourceServerIP
            // 
            this.labelSourceServerIP.AutoSize = true;
            this.labelSourceServerIP.Location = new System.Drawing.Point(11, 28);
            this.labelSourceServerIP.Name = "labelSourceServerIP";
            this.labelSourceServerIP.Size = new System.Drawing.Size(95, 13);
            this.labelSourceServerIP.TabIndex = 20;
            this.labelSourceServerIP.Text = "IP адрес сервера";
            // 
            // mtbxSourcePass
            // 
            this.mtbxSourcePass.Location = new System.Drawing.Point(129, 131);
            this.mtbxSourcePass.Name = "mtbxSourcePass";
            this.mtbxSourcePass.PasswordChar = '#';
            this.mtbxSourcePass.Size = new System.Drawing.Size(160, 20);
            this.mtbxSourcePass.TabIndex = 19;
            this.mtbxSourcePass.TextChanged += new System.EventHandler(component_Changed);
            // 
            // tbxSourceUserId
            // 
            this.tbxSourceUserId.Location = new System.Drawing.Point(129, 105);
            this.tbxSourceUserId.Name = "tbxSourceUserId";
            this.tbxSourceUserId.Size = new System.Drawing.Size(160, 20);
            this.tbxSourceUserId.TabIndex = 18;
            this.tbxSourceUserId.TextChanged += new System.EventHandler(base.component_Changed);
            // 
            // tbxSourceNameDatabase
            // 
            this.tbxSourceNameDatabase.Location = new System.Drawing.Point(129, 79);
            this.tbxSourceNameDatabase.Name = "tbxSourceNameDatabase";
            this.tbxSourceNameDatabase.Size = new System.Drawing.Size(160, 20);
            this.tbxSourceNameDatabase.TabIndex = 17;
            this.tbxSourceNameDatabase.TextChanged += new System.EventHandler(this.component_Changed);
            // 
            // tbxSourceServerIP
            // 
            this.tbxSourceServerIP.Location = new System.Drawing.Point(129, 25);
            this.tbxSourceServerIP.Name = "tbxSourceServerIP";
            this.tbxSourceServerIP.Size = new System.Drawing.Size(160, 20);
            this.tbxSourceServerIP.TabIndex = 15;
            this.tbxSourceServerIP.TextChanged += new System.EventHandler(this.component_Changed);

            this.groupBoxSource.ResumeLayout(false);
            this.groupBoxSource.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudnSourcePort)).EndInit();

            m_arUIControlDB = new System.Windows.Forms.Control[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE, (Int16)INDX_UICONTROL_DB.COUNT_INDX_UICONTROL_DB]
            { { tbxSourceServerIP, nudnSourcePort, tbxSourceNameDatabase, tbxSourceUserId, mtbxSourcePass },
            { tbxDestServerIP, nudnDestPort, tbxDestNameDatabase, tbxDestUserId, mtbxDestPass} };

            m_arAdmin = new Admin[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            //Источник
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE] = new Admin();
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].InitTEC(m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.DEST), FormChangeMode.MODE_TECCOMPONENT.GTP, true);
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].connSettConfigDB = m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.SOURCE);
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].ReConnSettingsRDGSource(m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.DEST), 103);
            m_arAdmin[(Int16)CONN_SETT_TYPE.SOURCE].m_typeFields = Admin.TYPE_FIELDS.STATIC;

            //Получатель
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST] = new Admin();
            //m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].SetDelegateTECComponent(FillComboBoxTECComponent);
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].InitTEC(m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.DEST), FormChangeMode.MODE_TECCOMPONENT.GTP, true);
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].connSettConfigDB = m_formConnectionSettings.getConnSett((Int16)CONN_SETT_TYPE.DEST);
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].m_typeFields = Admin.TYPE_FIELDS.DYNAMIC;
            m_arAdmin[(Int16)CONN_SETT_TYPE.DEST].m_ignore_date = true;

            for (int i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                setUIControlConnectionSettings(i);

                m_arAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                m_arAdmin[i].SetDelegateReport(ErrorReport, ActionReport);

                m_arAdmin[i].SetDelegateData(setDataGridViewAdmin);

                m_arAdmin[i].SetDelegateDatetime(setDatetimePicker);

                //m_arAdmin [i].mode (FormChangeMode.MODE_TECCOMPONENT.GTP);

                m_arAdmin[i].StartDbInterface();
            }

            //panelMain.Visible = false;

            timerMain.Interval = 666; //Признак первой итерации
            timerMain.Start();
        }

        private void setUIControlConnectionSettings(int i)
        {
            for (int j = 0; j < (Int16)INDX_UICONTROL_DB.COUNT_INDX_UICONTROL_DB; j++)
            {
                switch (j)
                {
                    case (Int16)FormMainTrans.INDX_UICONTROL_DB.SERVER_IP:
                        ((TextBox)m_arUIControlDB[i, j]).Text = m_arAdmin[i].connSettConfigDB.server;
                        break;
                    case (Int16)INDX_UICONTROL_DB.PORT:
                        if (m_arUIControlDB[i, j].Enabled)
                            ((NumericUpDown)m_arUIControlDB[i, j]).Text = m_arAdmin[i].connSettConfigDB.port.ToString();
                        else
                            ;
                        break;
                    case (Int16)INDX_UICONTROL_DB.NAME_DATABASE:
                        ((TextBox)m_arUIControlDB[i, j]).Text = m_arAdmin[i].connSettConfigDB.dbName;
                        break;
                    case (Int16)INDX_UICONTROL_DB.USER_ID:
                        ((TextBox)m_arUIControlDB[i, j]).Text = m_arAdmin[i].connSettConfigDB.userName;
                        break;
                    case (Int16)INDX_UICONTROL_DB.PASS:
                        ((MaskedTextBox)m_arUIControlDB[i, j]).Text = m_arAdmin[i].connSettConfigDB.password;
                        break;
                    default:
                        break;
                }
            }
        }

        protected override void component_Changed(object sender, EventArgs e)
        {
            uint indxDB = (uint)m_IndexDB;
            ConnectionSettings connSett = new ConnectionSettings();

            connSett.server = m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.SERVER_IP].Text;
            connSett.port = (Int32)((NumericUpDown)m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.PORT]).Value;
            connSett.dbName = m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.NAME_DATABASE].Text;
            connSett.userName = m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.USER_ID].Text;
            connSett.password = m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.PASS].Text;
            connSett.ignore = false;

            m_formConnectionSettings.ConnectionSettingsEdit = connSett;
        }
    }
}
