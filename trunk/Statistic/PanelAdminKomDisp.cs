using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Globalization;

using StatisticCommon;

namespace Statistic
{
    public class PanelAdminKomDisp : PanelAdmin
    {
        private System.Windows.Forms.Button btnImportCSV;

        private System.Windows.Forms.CheckBox m_cbxAlarm;
        private System.Windows.Forms.GroupBox m_gbxDividerAlarm;
        private Label lblKoeffAlarmCurPower;
        private NumericUpDown m_nudnKoeffAlarmCurPower;
        private System.Windows.Forms.Button m_btnAlarmCurPower;
        private System.Windows.Forms.Button m_btnAlarmTGTurnOnOff;

        public static bool ALARM_USE = true;
        public AdminAlarm m_adminAlarm;

        protected override void InitializeComponents()
        {
            base.InitializeComponents ();

            this.btnImportCSV = new System.Windows.Forms.Button();
            this.dgwAdminTable = new DataGridViewAdminKomDisp();

            this.m_cbxAlarm = new CheckBox();
            this.m_gbxDividerAlarm = new System.Windows.Forms.GroupBox();
            this.lblKoeffAlarmCurPower = new Label();
            this.m_nudnKoeffAlarmCurPower = new NumericUpDown();
            this.m_btnAlarmCurPower = new System.Windows.Forms.Button();
            this.m_btnAlarmTGTurnOnOff = new System.Windows.Forms.Button();

            this.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).BeginInit();

            this.m_panelManagement.Controls.Add(this.btnImportCSV);
            this.m_panelRDGValues.Controls.Add(this.dgwAdminTable);

            this.m_panelManagement.Controls.Add(m_cbxAlarm);
            this.m_panelManagement.Controls.Add(m_gbxDividerAlarm);
            this.m_panelManagement.Controls.Add(lblKoeffAlarmCurPower);
            this.m_panelManagement.Controls.Add(m_nudnKoeffAlarmCurPower);
            this.m_panelManagement.Controls.Add(m_btnAlarmCurPower);
            this.m_panelManagement.Controls.Add(m_btnAlarmTGTurnOnOff);

            // 
            // btnImportCSV
            // 
            this.btnImportCSV.Location = new System.Drawing.Point(10, 281);
            this.btnImportCSV.Name = "btnImportCSV";
            this.btnImportCSV.Size = new System.Drawing.Size(154, 23);
            this.btnImportCSV.TabIndex = 2;
            this.btnImportCSV.Text = "Импорт из формата CSV";
            this.btnImportCSV.UseVisualStyleBackColor = true;
            this.btnImportCSV.Click += new System.EventHandler(this.btnImportCSV_Click);
            // 
            // dgwAdminTable
            //
            this.dgwAdminTable.Location = new System.Drawing.Point(9, 9);
            this.dgwAdminTable.Size = new System.Drawing.Size(574, 591);
            this.dgwAdminTable.TabIndex = 1;
            //this.dgwAdminTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellClick);
            //this.dgwAdminTable.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellValidated);
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).EndInit();

            // 
            // gbxDividerChoice
            // 
            this.m_gbxDividerAlarm.Location = new System.Drawing.Point(10, 307);
            this.m_gbxDividerAlarm.Name = "gbxDividerAlarm";
            this.m_gbxDividerAlarm.Size = new System.Drawing.Size(154, 8);
            this.m_gbxDividerAlarm.TabIndex = 4;
            this.m_gbxDividerAlarm.TabStop = false;

            // 
            // m_cbxAlarm
            // 
            this.m_cbxAlarm.Enabled = PanelAdminKomDisp.ALARM_USE;
            this.m_cbxAlarm.Checked = false;
            this.m_cbxAlarm.Location = new System.Drawing.Point(12, 323);
            this.m_cbxAlarm.Name = "cbxAlarm";
            //this.m_cbxAlarm.Size = new System.Drawing.Size(154, 8);
            this.m_cbxAlarm.AutoSize = true;
            //this.m_cbxAlarm.TabIndex = 4;
            this.m_cbxAlarm.TabStop = false;
            this.m_cbxAlarm.Text = @"Синализация вкл.";
            this.m_cbxAlarm.CheckedChanged += new EventHandler(m_cbxAlarm_CheckedChanged);

            int offsetPosY = 28;
            //
            // lblKoeffAlarmCurPower
            //
            this.lblKoeffAlarmCurPower.Enabled = this.m_cbxAlarm.Checked;
            this.lblKoeffAlarmCurPower.Location = new System.Drawing.Point(10, 326 + offsetPosY);
            this.lblKoeffAlarmCurPower.Name = "lblKoeffAlarmCurPower";
            this.lblKoeffAlarmCurPower.AutoSize = true;
            //this.lblKoeffAlarmCurPower.Size = new System.Drawing.Size(70, 20);
            //this.lblKoeffAlarmCurPower.TabIndex = 4;
            this.lblKoeffAlarmCurPower.TabStop = false;
            this.lblKoeffAlarmCurPower.Text = @"Коэфф. сигн. Pтек";

            // 
            // m_nudnKoeffAlarmCurPower
            // 
            m_nudnKoeffAlarmCurPower.Enabled = this.m_cbxAlarm.Checked;
            m_nudnKoeffAlarmCurPower.Location = new System.Drawing.Point(116, 323 + offsetPosY);
            m_nudnKoeffAlarmCurPower.Name = "nudnKoeffAlarmCurPower";
            m_nudnKoeffAlarmCurPower.Size = new System.Drawing.Size(48, 20);
            m_nudnKoeffAlarmCurPower.TabIndex = 26;
            m_nudnKoeffAlarmCurPower.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            m_nudnKoeffAlarmCurPower.Minimum = 0.05M;
            m_nudnKoeffAlarmCurPower.Maximum = 0.90M;
            m_nudnKoeffAlarmCurPower.Value = 0.25M;
            m_nudnKoeffAlarmCurPower.DecimalPlaces = 2;
            m_nudnKoeffAlarmCurPower.Increment = 0.05M;

            // 
            // m_btnAlarmCurPower
            // 
            this.m_btnAlarmCurPower.Enabled = false;
            this.m_btnAlarmCurPower.Location = new System.Drawing.Point(10, 352 + offsetPosY);
            this.m_btnAlarmCurPower.Name = "btnAlarmCurPower";
            this.m_btnAlarmCurPower.Size = new System.Drawing.Size(154, 23);
            this.m_btnAlarmCurPower.TabIndex = 2;
            this.m_btnAlarmCurPower.Text = "Подтв. сигн. Pтек";
            this.m_btnAlarmCurPower.UseVisualStyleBackColor = true;
            this.m_btnAlarmCurPower.Click += new System.EventHandler(this.btnAlarmCurPower_Click);

            // 
            // m_btnAlarmTGTurnOnOff
            // 
            this.m_btnAlarmTGTurnOnOff.Enabled = false;
            this.m_btnAlarmTGTurnOnOff.Location = new System.Drawing.Point(10, 381 + offsetPosY);
            this.m_btnAlarmTGTurnOnOff.Name = "btnAlarmTGTurnOnOff";
            this.m_btnAlarmTGTurnOnOff.Size = new System.Drawing.Size(154, 23);
            this.m_btnAlarmTGTurnOnOff.TabIndex = 2;
            this.m_btnAlarmTGTurnOnOff.Text = "Подтв. сигн. ТГвкл/откл";
            this.m_btnAlarmTGTurnOnOff.UseVisualStyleBackColor = true;
            this.m_btnAlarmTGTurnOnOff.Click += new System.EventHandler(this.btnAlarmTGTurnOnOff_Click);

            this.ResumeLayout();
        }

        public PanelAdminKomDisp(int idListener, HReports rep)
            : base(idListener, FormChangeMode.MANAGER.DISP, rep)
        {
            if ((Users.Role < (int)Users.ID_ROLES.USER) && ALARM_USE == true) {
                m_adminAlarm = new AdminAlarm(rep);
                m_adminAlarm.InitTEC(m_admin.m_list_tec);
            } else ;
        }

        protected override void getDataGridViewAdmin()
        {
            double value;
            bool valid;

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < (int)DataGridViewAdminKomDisp.DESC_INDEX.TO_ALL; j++)
                {
                    switch (j)
                    {
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN: // План
                            valid = double.TryParse((string)dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].Value, out value);
                            m_admin.m_curRDGValues[i].pbr = value;
                            //m_admin.m_curRDGValues[i].pmin = 0.0;
                            //m_admin.m_curRDGValues[i].pmax = 0.0;
                            break;
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION: // Рекомендация
                            {
                                //cellValidated(e.RowIndex, (int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION);

                                valid = double.TryParse((string)dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value, out value);
                                m_admin.m_curRDGValues[i].recomendation = value;

                                break;
                            }
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE:
                            {
                                if (!(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE].Value == null))
                                    m_admin.m_curRDGValues[i].deviationPercent = bool.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                                else
                                    m_admin.m_curRDGValues[i].deviationPercent = false;

                                break;
                            }
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION: // Максимальное отклонение
                            {
                                valid = double.TryParse((string)this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION].Value, out value);
                                m_admin.m_curRDGValues[i].deviation = value;

                                break;
                            }
                    }
                }
            }

            //m_admin.CopyCurRDGValues();
        }

        public override void setDataGridViewAdmin(DateTime date)
        {
            for (int i = 0; i < 24; i++)
            {
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DATE_HOUR].Value = date.AddHours(i + 1).ToString("yyyy-MM-dd HH");
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].Value = m_admin.m_curRDGValues[i].pbr.ToString("F2");
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value = m_admin.m_curRDGValues[i].recomendation.ToString("F2");
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE].Value = m_admin.m_curRDGValues[i].deviationPercent.ToString();
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION].Value = m_admin.m_curRDGValues[i].deviation.ToString("F2");
            }

            //this.dgwAdminTable.Invalidate();

            m_admin.CopyCurToPrevRDGValues();
        }

        public override void ClearTables()
        {
            for (int i = 0; i < 24; i++)
            {
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DATE_HOUR].Value = "";
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].Value = "";
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value = "";
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE].Value = "false";
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION].Value = "";
            }
        }

        public override void InitializeComboBoxTecComponent(FormChangeMode.MODE_TECCOMPONENT mode)
        {
            base.InitializeComboBoxTecComponent(mode);

            for (int i = 0; i < m_listTECComponentIndex.Count; i++)
            {
                comboBoxTecComponent.Items.Add(m_admin.allTECComponents[m_listTECComponentIndex[i]].tec.name_shr + " - " + m_admin.GetNameTECComponent(m_listTECComponentIndex[i]));
            }

            if (comboBoxTecComponent.Items.Count > 0)
            {
                m_admin.indxTECComponents = m_listTECComponentIndex[0];
                comboBoxTecComponent.SelectedIndex = 0;
            }
            else
                ;
        }

        private void btnImportCSV_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folders = new FolderBrowserDialog();
            folders.ShowNewFolderButton = false;
            folders.RootFolder = Environment.SpecialFolder.Desktop;
            folders.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); //@"D:\Temp";

            if (folders.ShowDialog(((FormMain)Parent.Parent.Parent).formParameters) == DialogResult.OK)
                ((AdminTS_KomDisp)m_admin).ImpPPBRCSVValues(m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart, folders.SelectedPath + @"\");
            else
                ;
        }

        private void btnAlarmCurPower_Click(object sender, EventArgs e)
        {
        }

        private void btnAlarmTGTurnOnOff_Click(object sender, EventArgs e)
        {
        }

        private void m_cbxAlarm_CheckedChanged(object sender, EventArgs e)
        {
            this.lblKoeffAlarmCurPower.Enabled =
            this.m_nudnKoeffAlarmCurPower.Enabled = ((CheckBox)sender).Checked;
            this.m_btnAlarmCurPower.Enabled = 
            this.m_btnAlarmTGTurnOnOff.Enabled = false;

            if (PanelAdminKomDisp.ALARM_USE == true) m_adminAlarm.Activate(((CheckBox)sender).Checked); else ;
        }
    }
}
