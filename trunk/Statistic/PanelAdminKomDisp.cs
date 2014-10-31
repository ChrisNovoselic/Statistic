using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Drawing;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    public class PanelAdminKomDisp : PanelAdmin
    {
        private System.Windows.Forms.Button btnImportCSV;

        public System.Windows.Forms.CheckBox m_cbxAlarm;
        private System.Windows.Forms.GroupBox m_gbxDividerAlarm;
        private Label lblKoeffAlarmCurPower;
        private NumericUpDown m_nudnKoeffAlarmCurPower;
        private System.Windows.Forms.Button m_btnAlarmCurPower;
        private System.Windows.Forms.Button m_btnAlarmTGTurnOnOff;

        private class PanelLabelAlarm : TableLayoutPanel {
            private Dictionary<KeyValuePair<int, int>, System.Windows.Forms.Label> m_dictLabel;
            
            public PanelLabelAlarm () {
                m_dictLabel = new Dictionary<KeyValuePair<int,int>,Label> ();
                this.ColumnCount = 1;
                this.RowCount = 6;

                for (int i = 0; i < this.RowCount; i ++)
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, this.Height / this.RowCount));
            }

            public void Add(string text, int id, int id_tg)
            {
                KeyValuePair <int, int> cKey = new KeyValuePair <int, int> (id, id_tg);
                //m_dictLabel.Add(cKey, HLabel.createLabel (@"---", new HLabelStyles (Color.Red, Color.LightGray, 8F, ContentAlignment.MiddleLeft)));
                m_dictLabel.Add(cKey, new HLabel (new HLabelStyles (Color.Red, Color.LightGray, 8F, ContentAlignment.MiddleLeft)));
                m_dictLabel[cKey].Text = text; //??? - Наименование ГТП (ГТП + ТГ)

                //if (m_dictLabel.Count < this.RowCount)
                    this.Controls.Add (m_dictLabel [cKey], 0, m_dictLabel.Count - 1);
                //else
                //    ;
            }

            public void Remove(int id, int id_tg)
            {
                KeyValuePair <int, int> cKey = new KeyValuePair <int, int> (id, id_tg);
                int indx = this.Controls.IndexOf(m_dictLabel[cKey])
                    , i = -1;
                this.Controls.Remove(m_dictLabel[cKey]);
                m_dictLabel.Remove(cKey);
                //if (indx > 0)
                    for (i = indx; i < this.RowCount; i ++)
                        if ((i < this.Controls.Count) && (!(this.Controls[i] == null)))
                            this.SetRow(this.Controls [i], i);
                        else
                            break;
                //else
                //    ;                
            }
        }

        public static bool ALARM_USE = true;
        public AdminAlarm m_adminAlarm;
        private PanelLabelAlarm m_panelLabelAlarm;

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

            m_panelLabelAlarm = new PanelLabelAlarm();

            this.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).BeginInit();

            this.m_panelManagement.Controls.Add(this.btnImportCSV);
            this.m_panelRDGValues.Controls.Add(this.dgwAdminTable);

            this.m_panelManagement.Controls.Add(m_cbxAlarm);
            this.m_panelManagement.Controls.Add(m_gbxDividerAlarm);
            this.m_panelManagement.Controls.Add(lblKoeffAlarmCurPower);
            this.m_panelManagement.Controls.Add(m_nudnKoeffAlarmCurPower);
            this.m_panelManagement.Controls.Add(m_btnAlarmCurPower);
            this.m_panelManagement.Controls.Add(m_panelLabelAlarm);

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
            this.btnImportCSV.Enabled = false;
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
            this.m_cbxAlarm.Checked = false; //PanelAdminKomDisp.ALARM_USE;

            this.m_cbxAlarm.Location = new System.Drawing.Point(12, 323);
            this.m_cbxAlarm.Name = "cbxAlarm";
            //this.m_cbxAlarm.Size = new System.Drawing.Size(154, 8);
            this.m_cbxAlarm.AutoSize = true;
            //this.m_cbxAlarm.TabIndex = 4;
            this.m_cbxAlarm.TabStop = false;
            this.m_cbxAlarm.Text = @"Синализация вкл.";
            this.m_cbxAlarm.CheckedChanged += new EventHandler(cbxAlarm_CheckedChanged);

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
            m_nudnKoeffAlarmCurPower.Minimum = 2M;
            m_nudnKoeffAlarmCurPower.Maximum = 90M;
            m_nudnKoeffAlarmCurPower.Value = 20M;
            //m_nudnKoeffAlarmCurPower.DecimalPlaces = 2;
            m_nudnKoeffAlarmCurPower.Increment = 2M;

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

            // 
            // m_panelLabelAlarm
            // 
            this.m_panelLabelAlarm.Enabled = false;
            this.m_panelLabelAlarm.Location = new System.Drawing.Point(10, 410 + offsetPosY);
            this.m_panelLabelAlarm.Size = new System.Drawing.Size(154, 6 * 29);
            //this.m_panelLabelAlarm.Anchor = ((AnchorStyles)((AnchorStyles.Left | AnchorStyles.Right) | (AnchorStyles.Bottom)));
            //this.m_panelLabelAlarm.Anchor = ((AnchorStyles)((AnchorStyles.Left | AnchorStyles.Bottom) | (AnchorStyles.Right)));
            //this.m_panelLabelAlarm.Anchor = ((AnchorStyles)(AnchorStyles.Left | AnchorStyles.Bottom));
            //this.m_panelLabelAlarm.Anchor = ((AnchorStyles)(AnchorStyles.Right));
            this.m_panelLabelAlarm.Name = "panelLabelAlarm";
            this.m_panelLabelAlarm.TabIndex = 6;
            this.m_panelLabelAlarm.Text = "Сигнализация";
            //this.m_panelLabelAlarm.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetPartial;

            this.ResumeLayout();
        }

        public PanelAdminKomDisp(int idListener, HMark markQueries)
            : base(idListener, FormChangeMode.MANAGER.DISP, markQueries)
        {
            //if (((HStatisticUsers.RoleIsKomDisp == true) || (HStatisticUsers.RoleIsAdmin == true)) && ALARM_USE == true)
            if ((HStatisticUsers.RoleIsKomDisp == true) && ALARM_USE == true)
            {
                initAdminAlarm();

                m_adminAlarm.Start();
            } else ;

            this.m_nudnKoeffAlarmCurPower.ReadOnly = true;
            this.m_nudnKoeffAlarmCurPower.ValueChanged += new EventHandler(NudnKoeffAlarmCurPower_ValueChanged);
        }

        public override void Activate(bool activate)
        {
            //Значит пользователь администратор
            if (m_adminAlarm == null) initAdminAlarm(); else ;

            if (m_adminAlarm.IsStarted == false) m_adminAlarm.Start(); else ;

            base.Activate (activate);
        }

        private void initAdminAlarm()
        {
            m_adminAlarm = new AdminAlarm();
            m_adminAlarm.InitTEC(m_admin.m_list_tec);

            m_adminAlarm.EventAdd += new AdminAlarm.DelegateOnEventReg(OnAdminAlarm_EventAdd);
            m_adminAlarm.EventRetry += new AdminAlarm.DelegateOnEventReg(OnAdminAlarm_EventRetry);

            this.EventConfirm += new DelegateIntIntFunc(m_adminAlarm.OnEventConfirm);
        }

        private void EnabledButtonAlarm(int id_comp)
        {
            if (m_cbxAlarm.Checked == true)
            {
                m_btnAlarmCurPower.Enabled = m_adminAlarm.IsEnabledButtonAlarm(id_comp, -1);
                m_btnAlarmTGTurnOnOff.Enabled = false;
                foreach (TG tg in m_admin.allTECComponents[m_admin.indxTECComponents].m_listTG)
                    if (m_adminAlarm.IsEnabledButtonAlarm(id_comp, tg.m_id) == true)
                    {
                        m_btnAlarmTGTurnOnOff.Enabled = true;

                        break;
                    }
                    else
                        ;
            }
            else
                ;
        }

        private void EnabledButtonAlarm(int id_comp, int id_tg)
        {
            if ((m_cbxAlarm.Checked == true) && (id_comp == m_admin.allTECComponents [m_admin.indxTECComponents].m_id)) {
                m_btnAlarmCurPower.Enabled = m_adminAlarm.IsEnabledButtonAlarm(id_comp, -1);
                if (!(id_tg < 0))
                    m_btnAlarmTGTurnOnOff.Enabled = m_adminAlarm.IsEnabledButtonAlarm(id_comp, id_tg);
                else
                    ; //m_btnAlarmTGTurnOnOff.Enabled = false;
            }
            else
                ;
        }

        private TECComponent findTECComponent(int id)
        {
            foreach (TECComponent tc in m_admin.allTECComponents)
            {
                if (tc.m_id == id)
                    return tc;
                else ;
            }

            return null;
        }

        private void AddLabelAlarm(int id, int id_tg)
        {
            TECComponent tc = null;
            string text = string.Empty;
            int id_find = -1;
            if (id_tg < 0)
                id_find = id;
            else
                id_find = id_tg;

            tc = findTECComponent(id_find);
            text = tc.tec.name_shr + @" - " + tc.name_shr;

            m_panelLabelAlarm.Add(text, id, id_tg);
        }

        private void RemoveLabelAlarm(int id, int id_tg)
        {
            m_panelLabelAlarm.Remove(id, id_tg);
        }

        //public event DelegateStringFunc EventGUIReg;
        public DelegateStringFunc EventGUIReg;
        public void EventGUIConfirm () {
            m_adminAlarm.Activate(true);
        }

        private void toEventGUIReg(TecView.EventRegEventArgs ev)
        {
            string msg = string.Empty;

            //Деактивация m_adminAlarm
            m_adminAlarm.Activate(false);

            int id_evt = -1;
            if (ev.m_id_tg < 0)
            {
                id_evt = ev.m_id_gtp;

                if (ev.m_situation == 1)
                    msg = @"вверх";
                else
                    if (ev.m_situation == -1)
                        msg = @"вниз";
                    else
                        msg = @"нет";
            }
            else
            {
                id_evt = ev.m_id_tg;

                if (ev.m_situation == (int)TG.INDEX_TURNOnOff.ON) //TGTurnOnOff = ON
                    msg = @"вкл.";
                else
                    if (ev.m_situation == (int)TG.INDEX_TURNOnOff.OFF) //TGTurnOnOff = OFF
                        msg = @"выкл.";
                    else
                        msg = @"нет";
            }

            TECComponent tc = findTECComponent(id_evt);
            msg = tc.tec.name_shr + @"::" +  tc.name_shr + Environment.NewLine + @"Информация: " + msg;
            EventGUIReg(msg);
        }

        private void OnAdminAlarm_EventAdd (TecView.EventRegEventArgs ev) {
            if (InvokeRequired == true)
            {
                this.BeginInvoke(new DelegateIntIntFunc(EnabledButtonAlarm), ev.m_id_gtp, ev.m_id_tg);

                this.BeginInvoke(new DelegateIntIntFunc(AddLabelAlarm), ev.m_id_gtp, ev.m_id_tg);

                toEventGUIReg(ev);
            }
            else
                Logging.Logg().Error(@"PanelAdminKomDisp::OnAdminAlarm_EventAdd () - ... BeginInvoke (EnabledButtonAlarm, AddLabelAlarm) - ...");
        }

        private void OnAdminAlarm_EventRetry(TecView.EventRegEventArgs ev)
        {
            toEventGUIReg(ev);
        }

        protected override void getDataGridViewAdmin()
        {
            double value;
            bool valid;
            //int offset = -1;

            for (int i = 0; i < dgwAdminTable.Rows.Count; i++)
            {
                //offset = m_admin.GetSeasonHourOffset(i);
                
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
            int offset = -1;
            string strFmtDatetime = string.Empty;

            //??? не очень изящное решение
            if (InvokeRequired == true)
            {
                m_evtAdminTableRowCount.Reset();
                this.BeginInvoke(new DelegateFunc(normalizedTableHourRows));
                m_evtAdminTableRowCount.WaitOne(System.Threading.Timeout.Infinite);
            }
            else
                Logging.Logg().Error(@"PanelTAdminKomDisp::setDataGridViewAdmin () - ... BeginInvoke (normalizedTableHourRows) - ...");

            for (int i = 0; i < m_admin.m_curRDGValues.Length; i++)
            {
                strFmtDatetime = m_admin.GetFmtDatetime (i);
                offset = m_admin.GetSeasonHourOffset (i + 1);

                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DATE_HOUR].Value = date.AddHours(i + 1 - offset).ToString(strFmtDatetime);

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
            for (int i = 0; i < dgwAdminTable.Rows.Count; i++)
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

                setNudnKoeffAlarmCurPowerValue ();
            }
            else
                ;
        }

        private void btnImportCSV_Click(object sender, EventArgs e)
        {
            //Вариант №1 (каталог)
            //FolderBrowserDialog folders = new FolderBrowserDialog();
            //folders.ShowNewFolderButton = false;
            //folders.RootFolder = Environment.SpecialFolder.Desktop;
            //folders.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); //@"D:\Temp";

            //if (folders.ShowDialog(FormMain.formParameters) == DialogResult.OK)
            //    ((AdminTS_KomDisp)m_admin).ImpPPBRCSVValues(mcldrDate.SelectionStart, folders.SelectedPath + @"\");
            //else
            //    ;

            //Вариант №2 (файл)
            OpenFileDialog files = new OpenFileDialog ();
            files.Multiselect = false;
            //files.InitialDirectory = Environment.GetFolderPath (Environment.SpecialFolder.Desktop);
            files.InitialDirectory = @"V:\Statistic\ПБР-csv";
            files.DefaultExt = @"csv";
            files.Filter = @"csv файлы (*.csv)|*.csv";
            files.Title = "Выберите файл с ПБР...";

            if (files.ShowDialog(FormMain.formParameters) == DialogResult.OK) {
                int iRes = ((AdminTS_KomDisp)m_admin).ImpPPBRCSVValues(mcldrDate.SelectionStart, files.FileName);

                if (!(iRes == 0))
                {
                    //Дата ПБР, номер ПБР из наименования файла
                    object[] prop = ((AdminTS_KomDisp)m_admin).GetPropertiesOfNameFilePPBRCSVValues();
                    //Текущий номер ПБР
                    int curPBRNumber = Int32.Parse(m_admin.m_curRDGValues[m_admin.m_curRDGValues.Length - 1].pbr_number.Substring(3));
                    string strMsg = string.Empty;

                    if (iRes == -1)
                    {
                        strMsg = string.Format(@"Дата загружаемого [{0}] набора ПБР не соответствует тек./дате [{1}]", ((DateTime)prop[0]).ToString(@"dd.MM.yyyy"), DateTime.Now.Date.ToString(@"dd.MM.yyyy"));
                        MessageBox.Show(this, strMsg, @"Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        //Сравнить с текущим номером ПБР
                        if (iRes == -2)
                        {
                            strMsg = string.Format(@"Номер загружаемого набора [{0}] ПБР не выше, чем текущий [{1}]. Продолжить?", (int)prop[1], curPBRNumber);
                            if (MessageBox.Show(this, strMsg, @"Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                            {
                                ((AdminTS_KomDisp)m_admin).ImpPPBRCSVValues(mcldrDate.SelectionStart, files.FileName, false);
                            }
                            else
                            {
                            }
                        }
                        else
                            ;
                    }
                }
                else
                    ;
            }
            else
                ;
        }

        private event DelegateIntIntFunc EventConfirm;

        private void btnAlarm_Click(int id_gtp, int id_tg)
        {
            EventConfirm(id_gtp, id_tg);

            RemoveLabelAlarm(id_gtp, id_tg);

            EnabledButtonAlarm(id_gtp, id_tg);
        }
        
        private void btnAlarmCurPower_Click(object sender, EventArgs e)
        {
            btnAlarm_Click(m_admin.allTECComponents[m_admin.indxTECComponents].m_id, -1);
        }

        private void btnAlarmTGTurnOnOff_Click(object sender, EventArgs e)
        {
            TG tg_find = null; //???
            DateTime dt, dt_find = DateTime.Now;
            int id_comp = m_admin.allTECComponents[m_admin.indxTECComponents].m_id
                , id_tg = -1;

            //Найти ТГ для "подтверждения" сигнализации
            foreach (TG tg in m_admin.allTECComponents[m_admin.indxTECComponents].m_listTG) {
                dt = m_adminAlarm.TGAlarmDatetimeReg (id_comp, tg.m_id);
                if ((dt_find.CompareTo (dt) > 0) && (m_adminAlarm.Confirm (id_comp, tg.m_id)) == false) {
                    dt_find = dt;
                    tg_find = tg;
                }
                else
                    ;
            }

            if ((! (tg_find == null)) && (dt_find.CompareTo (DateTime.Now) < 0))
                btnAlarm_Click(id_comp, tg_find.m_id);
            else
                ;
        }

        private void cbxAlarm_CheckedChanged(object sender, EventArgs e)
        {
            this.lblKoeffAlarmCurPower.Enabled =
            this.m_nudnKoeffAlarmCurPower.Enabled = ((CheckBox)sender).Checked;
            this.m_btnAlarmCurPower.Enabled = 
            this.m_btnAlarmTGTurnOnOff.Enabled = false;

            if (PanelAdminKomDisp.ALARM_USE == true)
            {
                if (m_adminAlarm == null)
                {
                    if (((CheckBox)sender).Checked == true)
                    {
                        initAdminAlarm();

                        m_adminAlarm.Start();

                        m_adminAlarm.Activate(true);
                    }
                    else
                    {
                        if (m_adminAlarm.IsStarted == false)
                            m_adminAlarm.Start();
                        else
                            ;

                        m_adminAlarm.Activate(false);
                    }
                }
                else
                {
                    if (m_adminAlarm.IsStarted == false)
                        m_adminAlarm.Start();
                    else
                        ;

                    m_adminAlarm.Activate(((CheckBox)sender).Checked);
                }
            }
            else ;

            EnabledButtonAlarm(m_admin.allTECComponents[m_admin.indxTECComponents].m_id);
        }

        protected override void comboBoxTecComponent_SelectionChangeCommitted(object sender, EventArgs e)
        {
            base.comboBoxTecComponent_SelectionChangeCommitted (sender, e);

            EnabledButtonAlarm(m_admin.allTECComponents[m_admin.indxTECComponents].m_id);
            
            setNudnKoeffAlarmCurPowerValue ();
        }

        private void setNudnKoeffAlarmCurPowerValue () {
            this.m_nudnKoeffAlarmCurPower.ValueChanged -= new EventHandler(NudnKoeffAlarmCurPower_ValueChanged);
            m_nudnKoeffAlarmCurPower.Value = m_admin.allTECComponents [m_admin.indxTECComponents].m_dcKoeffAlarmPcur;
            this.m_nudnKoeffAlarmCurPower.ValueChanged += new EventHandler(NudnKoeffAlarmCurPower_ValueChanged);
        }

        private void NudnKoeffAlarmCurPower_ValueChanged (object obj, EventArgs ev) {
            m_admin.allTECComponents [m_admin.indxTECComponents].m_dcKoeffAlarmPcur = m_nudnKoeffAlarmCurPower.Value;

            int err = -1
                , idListenerConfigDB = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
            System.Data.Common.DbConnection dbConn = DbSources.Sources ().GetConnection (idListenerConfigDB, out err);
            //DbTSQLInterface.ExecNonQuery(ref dbConn, @"UPDATE [dbo].[GTP_LIST] SET [KoeffAlarmPcur] = {0} WHERE [ID] = {1} ", new System.Data.DbType[] { System.Data.DbType.Decimal, System.Data.DbType.Int16 }, new object[] { m_admin.allTECComponents[m_admin.indxTECComponents].m_dcKoeffAlarmPcur, m_admin.allTECComponents [m_admin.indxTECComponents].m_id }, out err);
            //DbTSQLInterface.ExecNonQuery(ref dbConn, @"UPDATE [dbo].[GTP_LIST] SET [KoeffAlarmPcur]=? WHERE [ID]=?", new System.Data.DbType[] { System.Data.DbType.Decimal, System.Data.DbType.Int16 }, new object[] { m_admin.allTECComponents[m_admin.indxTECComponents].m_dcKoeffAlarmPcur, m_admin.allTECComponents[m_admin.indxTECComponents].m_id }, out err);
            DbTSQLInterface.ExecNonQuery(ref dbConn, @"UPDATE [dbo].[GTP_LIST] SET [KoeffAlarmPcur] = " + m_admin.allTECComponents[m_admin.indxTECComponents].m_dcKoeffAlarmPcur + @" WHERE [ID] = " + m_admin.allTECComponents[m_admin.indxTECComponents].m_id, null, null, out err);
            DbSources.Sources().UnRegister(idListenerConfigDB);
        }
    }
}

