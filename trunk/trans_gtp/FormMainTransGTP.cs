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
        private System.Windows.Forms.Label labelSourcePort;
        private System.Windows.Forms.NumericUpDown nudnSourcePort;
        private System.Windows.Forms.Label labelSourcePass;
        private System.Windows.Forms.Label labelSourceUserId;
        private System.Windows.Forms.Label labelSourceNameDatabase;
        private System.Windows.Forms.Label labelSourceServerIP;
        private System.Windows.Forms.MaskedTextBox mtbxSourcePass;
        private System.Windows.Forms.TextBox tbxSourceUserId;
        private System.Windows.Forms.TextBox tbxSourceNameDatabase;
        private System.Windows.Forms.TextBox tbxSourceServerIP;

        public FormMainTransGTP()
        {
            InitializeComponentTransGTP();

            this.Text = "Конвертер данных плана и административных данных (ГТП)";

            this.m_dgwAdminTable = new StatisticCommon.DataGridViewAdminKomDisp();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).BeginInit();
            this.SuspendLayout();
            // 
            // m_dgwAdminTable
            // 
            this.m_dgwAdminTable.Location = new System.Drawing.Point(319, 5);
            this.m_dgwAdminTable.Name = "m_dgwAdminTable";
            this.m_dgwAdminTable.RowHeadersVisible = false;
            this.m_dgwAdminTable.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.m_dgwAdminTable.Size = new System.Drawing.Size(498, 471);
            this.m_dgwAdminTable.TabIndex = 27;
            this.panelMain.Controls.Add(this.m_dgwAdminTable);
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).EndInit();
            this.ResumeLayout(false);

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMainTrans));
            this.notifyIconMain.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic4"))); //$this.Icon
            this.notifyIconMain.Text = "Статистика: конвертер (ГТП)";
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic4"))); //$this.Icon

            m_modeTECComponent = FormChangeMode.MODE_TECCOMPONENT.GTP;

            m_arUIControlDB = new System.Windows.Forms.Control[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE, (Int16)INDX_UICONTROL_DB.COUNT_INDX_UICONTROL_DB]
            { { tbxSourceServerIP, nudnSourcePort, tbxSourceNameDatabase, tbxSourceUserId, mtbxSourcePass },
            { tbxDestServerIP, nudnDestPort, tbxDestNameDatabase, tbxDestUserId, mtbxDestPass} };

            //Созжание массива для объектов получения данных
            m_arAdmin = new AdminTS[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            Start();
        }

        private void InitializeComponentTransGTP()
        {
            this.labelSourcePort = new System.Windows.Forms.Label();
            this.nudnSourcePort = new System.Windows.Forms.NumericUpDown();
            this.labelSourcePass = new System.Windows.Forms.Label();
            this.labelSourceUserId = new System.Windows.Forms.Label();
            this.labelSourceNameDatabase = new System.Windows.Forms.Label();
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
            this.groupBoxSource.Controls.Add(this.labelSourceNameDatabase);
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
            // labelSourceNameDatabase
            // 
            this.labelSourceNameDatabase.AutoSize = true;
            this.labelSourceNameDatabase.Location = new System.Drawing.Point(11, 82);
            this.labelSourceNameDatabase.Name = "labelSourceNameDatabase";
            this.labelSourceNameDatabase.Size = new System.Drawing.Size(98, 13);
            this.labelSourceNameDatabase.TabIndex = 22;
            this.labelSourceNameDatabase.Text = "Имя базы данных";
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
            this.mtbxSourcePass.Enabled = false; 
            this.mtbxSourcePass.Location = new System.Drawing.Point(129, 131);
            this.mtbxSourcePass.Name = "mtbxSourcePass";
            this.mtbxSourcePass.PasswordChar = '#';
            this.mtbxSourcePass.Size = new System.Drawing.Size(160, 20);
            this.mtbxSourcePass.TabIndex = 19;
            this.mtbxSourcePass.TextChanged += new System.EventHandler(component_Changed);
            // 
            // tbxSourceUserId
            // 
            this.tbxSourceUserId.Enabled = false;
            this.tbxSourceUserId.Location = new System.Drawing.Point(129, 105);
            this.tbxSourceUserId.Name = "tbxSourceUserId";
            this.tbxSourceUserId.Size = new System.Drawing.Size(160, 20);
            this.tbxSourceUserId.TabIndex = 18;
            this.tbxSourceUserId.TextChanged += new System.EventHandler(base.component_Changed);
            // 
            // tbxSourceNameDatabase
            // 
            this.tbxSourceNameDatabase.Enabled = false;
            this.tbxSourceNameDatabase.Location = new System.Drawing.Point(129, 79);
            this.tbxSourceNameDatabase.Name = "tbxSourceNameDatabase";
            this.tbxSourceNameDatabase.Size = new System.Drawing.Size(160, 20);
            this.tbxSourceNameDatabase.TabIndex = 17;
            this.tbxSourceNameDatabase.TextChanged += new System.EventHandler(this.component_Changed);
            // 
            // tbxSourceServerIP
            // 
            this.tbxSourceServerIP.Enabled = false;
            this.tbxSourceServerIP.Location = new System.Drawing.Point(129, 25);
            this.tbxSourceServerIP.Name = "tbxSourceServerIP";
            this.tbxSourceServerIP.Size = new System.Drawing.Size(160, 20);
            this.tbxSourceServerIP.TabIndex = 15;
            this.tbxSourceServerIP.TextChanged += new System.EventHandler(this.component_Changed);

            this.groupBoxSource.ResumeLayout(false);
            this.groupBoxSource.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudnSourcePort)).EndInit();
        }

        protected override void Start()
        {
            int i = -1;

            CreateFormConnectionSettingsConfigDB("connsett_gtp.ini");

            //Инициализация объектов получения данных
            for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                m_arAdmin[i] = new AdminTS_KomDisp();
                try { ((AdminTS_KomDisp)m_arAdmin[i]).InitTEC(m_formConnectionSettingsConfigDB.getConnSett((Int16)CONN_SETT_TYPE.DEST), m_modeTECComponent, true, false); }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, "FormMainTransGTP::FormMainTransGTP ()");
                    //ErrorReport("Ошибка соединения. Перехож в ожидание.");
                    //setUIControlConnectionSettings(i);
                    break;
                }
                
                //((AdminTS)m_arAdmin[i]).connSettConfigDB = m_formConnectionSettings.getConnSett(i);
                
                if (i == (Int16)CONN_SETT_TYPE.SOURCE)
                {
                    ((AdminTS_KomDisp)m_arAdmin[i]).ReConnSettingsRDGSource(m_formConnectionSettingsConfigDB.getConnSett((Int16)CONN_SETT_TYPE.DEST), 103);
                    ((AdminTS_KomDisp)m_arAdmin[i]).m_typeFields = AdminTS.TYPE_FIELDS.STATIC;
                    //Для отладки получить ГТП из БД с новой структурой
                    //((AdminTS_KomDisp)m_arAdmin[i]).m_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;
                }
                else
                    ((AdminTS_KomDisp)m_arAdmin[i]).m_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;

                m_arAdmin[i].m_ignore_date = true;
                //m_arAdmin[i].m_ignore_connsett_data = true; //-> в конструктор

                setUIControlConnectionSettings(i);

                m_arAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                m_arAdmin[i].SetDelegateReport(ErrorReport, ActionReport);

                m_arAdmin[i].SetDelegateData(setDataGridViewAdmin);
                m_arAdmin[i].SetDelegateSaveComplete(saveDataGridViewAdminComplete);

                m_arAdmin[i].SetDelegateDatetime(setDatetimePicker);

                //m_arAdmin [i].mode (FormChangeMode.MODE_TECCOMPONENT.GTP);

                m_arAdmin[i].StartThreadSourceData();
            }

            if (!(i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE))
            {
                timerMain.Interval = 666; //Признак первой итерации
                timerMain.Start();
            }
            else
                ;
        }

        protected override void getDataGridViewAdmin(int indxDB)
        {
            //int indxDB = m_IndexDB;

            double value;
            bool valid;

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < (int)DataGridViewAdminKomDisp.DESC_INDEX.TO_ALL; j++)
                {
                    switch (j)
                    {
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN: // План
                            valid = double.TryParse((string)m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].Value, out value);
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr = value;
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pmin = ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curRDGValues[i].pmin;
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr = ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curRDGValues[i].pbr;
                            break;
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION: // Рекомендация
                            {
                                //cellValidated(e.RowIndex, (int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION);

                                valid = double.TryParse((string)m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value, out value);
                                ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].recomendation = value;

                                break;
                            }
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE:
                            {
                                ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].deviationPercent = bool.Parse(this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                                break;
                            }
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION: // Максимальное отклонение
                            {
                                valid = double.TryParse((string)this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION].Value, out value);
                                ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].deviation = value;

                                break;
                            }
                        default:
                            break;
                    }
                }
            }

            m_arAdmin[indxDB].CopyCurToPrevRDGValues();
        }

        protected override void updateDataGridViewAdmin(DateTime date)
        {
            int indxDB = m_IndexDB;

            for (int i = 0; i < 24; i++)
            {
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DATE_HOUR].Value = date.AddHours(i + 1).ToString("yyyy-MM-dd HH");
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr.ToString("F2");
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].recomendation.ToString("F2");
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].deviationPercent.ToString();
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].deviation.ToString("F2");
            }

            //m_arAdmin[indxDB].CopyCurToPrevRDGValues ();

            //this.m_dgwAdminTable.Invalidate();
        }

        protected override void comboBoxTECComponent_SelectedIndexChanged(object cbx, EventArgs ev)
        {
            if (IsCanSelectedIndexChanged () == true)
            {
                ClearTables();

                short indexDB = m_IndexDB;
                
                switch (m_modeTECComponent)
                {
                    case FormChangeMode.MODE_TECCOMPONENT.GTP:
                        ((AdminTS)m_arAdmin[indexDB]).GetRDGValues((int)((AdminTS)m_arAdmin[indexDB]).m_typeFields, m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value.Date);
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.TG:
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.TEC:
                        break;
                    default:
                        break;
                }

                setUIControlConnectionSettings((int)CONN_SETT_TYPE.SOURCE);
                setUIControlConnectionSettings((int)CONN_SETT_TYPE.DEST);
            }
            else
                ;
        }

        protected override void CreateFormConnectionSettingsConfigDB(string connSettFileName)
        {
            base.CreateFormConnectionSettingsConfigDB(connSettFileName);

            if ((!(m_formConnectionSettingsConfigDB.Ready == 0)) || (m_formConnectionSettingsConfigDB.Count < 2))
            {
                while (m_formConnectionSettingsConfigDB.Count < 2)
                    m_formConnectionSettingsConfigDB.addConnSett(new ConnectionSettings());
            }
            else
                ;
        }
    }
}
