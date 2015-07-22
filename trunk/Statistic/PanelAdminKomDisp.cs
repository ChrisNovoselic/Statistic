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
        private System.Windows.Forms.Button btnImportCSV_PBRValues;
        private System.Windows.Forms.Button btnImportCSV_AdminDefaultValues;
        //private System.Windows.Forms.CheckBox btnImportCSV_AdminDefaultValues;

        private System.Windows.Forms.CheckBox m_cbxAlarm;
        private System.Windows.Forms.GroupBox m_gbxDividerAlarm;
        private Label lblKoeffAlarmCurPower;
        private NumericUpDown m_nudnKoeffAlarmCurPower;
        private System.Windows.Forms.Button m_btnAlarmCurPower;
        private System.Windows.Forms.Button m_btnAlarmTGTurnOnOff;

        private class PanelLabelAlarm : HPanelCommon {
            private Dictionary<KeyValuePair<int, int>, System.Windows.Forms.Label> m_dictLabel;
            
            public PanelLabelAlarm () : base (-1, -1) {
                m_dictLabel = new Dictionary<KeyValuePair<int,int>,Label> ();

                initializeLayoutStyle (1, 6);
            }

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                this.ColumnCount = cols;
                this.RowCount = rows;

                for (int i = 0; i < this.RowCount; i++)
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

        private enum INDEX_CONTROL_UI
        {
                                        BUTTON_CSV_IMPORT_PBR, BUTTON_CSV_IMPORT_ADMINVALUESDEFAULT //BUTTON_CSV_IMPORT_ADMINVALUESDEFAULT
                                        , GBX_DIVIDEALARM , CBX_ALARM
                                        , LABEL_KOEFFALARMCURPOWER, NUDN_KOEFFALARMCURPOWER, BUTTON_ALARMCURPOWER, BUTTON_ALARMTGTUTNONOFF
                                        , PANEL_ALARMSOURCES
                                        , COUNT };

        protected override void InitializeComponents()
        {
            base.InitializeComponents ();

            int posY = 271
                , offsetPosY = m_iSizeY + 2 * m_iMarginY
                , indx = -1;
            Rectangle[] arRectControlUI = new Rectangle[] {
                //new Rectangle (new Point (10, 281), new Size (154, 23)) //btnImportCSV_PBRValues, BUTTON_CSV_IMPORT_PBR
                //, new Rectangle (new Point (10, 307), new Size (154, 8)) //gbxDividerAlarm
                //, new Rectangle (new Point (12, 323), new Size (-1, -1)) //cbxAlarm
                //, new Rectangle (new Point (10, 326), new Size (-1, -1)) //lblKoeffAlarmCurPower
                //, new Rectangle (new Point (116, 323), new Size (48, 20)) //nudnKoeffAlarmCurPower
                //, new Rectangle (new Point (10, 352), new Size (154, 23)) //btnAlarmCurPower
                //, new Rectangle (new Point (10, 381), new Size (154, 23)) //btnAlarmTGTurnOnOff
                //, new Rectangle (new Point (10, 410), new Size (154, 6 * 29))
                new Rectangle (new Point (10, posY), new Size (154, m_iSizeY)) //btnImportCSV_PBRValues, BUTTON_CSV_IMPORT_PBR
                , new Rectangle (new Point (10, posY + 1 * (m_iSizeY + m_iMarginY)), new Size (154, m_iSizeY)) //ckbImportCSV_AdminDefaultValues, BUTTON_CSV_IMPORT_ADMINVALUESDEFAULT
                , new Rectangle (new Point (10, posY + 2 * (m_iSizeY + m_iMarginY)), new Size (154, 8)) //gbxDividerAlarm
                , new Rectangle (new Point (12, posY + 3 * (m_iSizeY + m_iMarginY) - m_iMarginY), new Size (-1, -1)) //cbxAlarm
                , new Rectangle (new Point (10, posY + 3 * (m_iSizeY + m_iMarginY) + (m_iSizeY + m_iMarginY) - m_iMarginY), new Size (-1, -1)) //lblKoeffAlarmCurPower
                , new Rectangle (new Point (116, posY + 3 * (m_iSizeY + m_iMarginY) + (m_iSizeY + 0) - m_iMarginY), new Size (48, (m_iSizeY - m_iMarginY))) //nudnKoeffAlarmCurPower
                , new Rectangle (new Point (10, posY + 4 * (m_iSizeY + m_iMarginY) + offsetPosY - m_iMarginY), new Size (154, m_iSizeY)) //btnAlarmCurPower
                , new Rectangle (new Point (10, posY + 5 * (m_iSizeY + m_iMarginY) + offsetPosY - m_iMarginY), new Size (154, m_iSizeY)) //btnAlarmTGTurnOnOff
                , new Rectangle (new Point (10, posY + 6 * (m_iSizeY + m_iMarginY) + offsetPosY - m_iMarginY), new Size (154, 6 * offsetPosY))
            };
            
            this.btnImportCSV_PBRValues = new System.Windows.Forms.Button();
            this.btnImportCSV_AdminDefaultValues = new System.Windows.Forms.Button();
            //this.cbxImportCSV_AdminDefaultValues = new System.Windows.Forms.CheckBox();
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

            this.m_panelManagement.Controls.Add(this.btnImportCSV_PBRValues);
            this.m_panelManagement.Controls.Add(this.btnImportCSV_AdminDefaultValues);
            this.m_panelRDGValues.Controls.Add(this.dgwAdminTable);

            this.m_panelManagement.Controls.Add(m_cbxAlarm);
            this.m_panelManagement.Controls.Add(m_gbxDividerAlarm);
            this.m_panelManagement.Controls.Add(lblKoeffAlarmCurPower);
            this.m_panelManagement.Controls.Add(m_nudnKoeffAlarmCurPower);
            this.m_panelManagement.Controls.Add(m_btnAlarmCurPower);
            this.m_panelManagement.Controls.Add(m_panelLabelAlarm);

            this.m_panelManagement.Controls.Add(m_btnAlarmTGTurnOnOff);

            // 
            // btnImportCSV_PBRValues
            //
            indx = (int)INDEX_CONTROL_UI.BUTTON_CSV_IMPORT_PBR;
            this.btnImportCSV_PBRValues.Location = arRectControlUI [indx].Location;
            this.btnImportCSV_PBRValues.Name = "btnImportCSV_PBRValues";
            this.btnImportCSV_PBRValues.Size = arRectControlUI[indx].Size;
            this.btnImportCSV_PBRValues.TabIndex = 2;
            this.btnImportCSV_PBRValues.Text = "Импорт из формата CSV";
            this.btnImportCSV_PBRValues.UseVisualStyleBackColor = true;
            this.btnImportCSV_PBRValues.Click += new System.EventHandler(this.btnImportCSV_PBRValues_Click);
            this.btnImportCSV_PBRValues.Enabled = true;
            // 
            // btnImportCSV_AdminDefaultValues
            // 
            indx = (int)INDEX_CONTROL_UI.BUTTON_CSV_IMPORT_ADMINVALUESDEFAULT;
            this.btnImportCSV_AdminDefaultValues.Location = arRectControlUI[indx].Location;
            this.btnImportCSV_AdminDefaultValues.Name = "btnImportCSV_AdminDefaultValues";
            this.btnImportCSV_AdminDefaultValues.Size = arRectControlUI[indx].Size;
            this.btnImportCSV_AdminDefaultValues.TabIndex = 2;
            this.btnImportCSV_AdminDefaultValues.Text = "Реком. по умолчанию";
            this.btnImportCSV_AdminDefaultValues.UseVisualStyleBackColor = true;
            this.btnImportCSV_AdminDefaultValues.Click += new System.EventHandler(this.btnImportCSV_AdminValuesDefault_Click);
            //this.ckbImportCSV_AdminDefaultValues.CheckedChanged += new EventHandler(ckbImportCSV_AdminDefaultValues_CheckedChanged);
            this.btnImportCSV_AdminDefaultValues.Enabled = true;
            // 
            // dgwAdminTable
            //
            this.dgwAdminTable.Location = new System.Drawing.Point(9, 9);
            this.dgwAdminTable.Size = new System.Drawing.Size(714, 591);
            this.dgwAdminTable.TabIndex = 1;
            //this.dgwAdminTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellClick);
            //this.dgwAdminTable.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellValidated);
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).EndInit();

            // 
            // gbxDividerChoice
            // 
            indx = (int)INDEX_CONTROL_UI.GBX_DIVIDEALARM;
            this.m_gbxDividerAlarm.Location = arRectControlUI[indx].Location;
            this.m_gbxDividerAlarm.Name = "gbxDividerAlarm";
            this.m_gbxDividerAlarm.Size = arRectControlUI[indx].Size;
            this.m_gbxDividerAlarm.TabIndex = 4;
            this.m_gbxDividerAlarm.TabStop = false;

            // 
            // m_cbxAlarm
            // 
            indx = (int)INDEX_CONTROL_UI.CBX_ALARM;
            this.m_cbxAlarm.Enabled = false ; //PanelAdminKomDisp.ALARM_USE;
            this.m_cbxAlarm.Checked = false; //HStatisticUsers.IsAllowed ((int)HStatisticUsers.ID_ALLOWED.AUTO_ALARM_KOMDISP); //false; //PanelAdminKomDisp.ALARM_USE;

            this.m_cbxAlarm.Location = arRectControlUI[indx].Location;
            this.m_cbxAlarm.Name = "cbxAlarm";
            //this.m_cbxAlarm.Size = new System.Drawing.Size(154, 8);
            this.m_cbxAlarm.AutoSize = true;
            //this.m_cbxAlarm.TabIndex = 4;
            this.m_cbxAlarm.TabStop = false;
            this.m_cbxAlarm.Text = @"Сигнализация вкл.";
            this.m_cbxAlarm.CheckedChanged += new EventHandler(cbxAlarm_CheckedChanged);

            offsetPosY = 26; //28
            //
            // lblKoeffAlarmCurPower
            //
            indx = (int)INDEX_CONTROL_UI.LABEL_KOEFFALARMCURPOWER;
            this.lblKoeffAlarmCurPower.Enabled = this.m_cbxAlarm.Checked;
            this.lblKoeffAlarmCurPower.Location = arRectControlUI[indx].Location;
            this.lblKoeffAlarmCurPower.Name = "lblKoeffAlarmCurPower";
            this.lblKoeffAlarmCurPower.AutoSize = true;
            //this.lblKoeffAlarmCurPower.Size = new System.Drawing.Size(70, 20);
            //this.lblKoeffAlarmCurPower.TabIndex = 4;
            this.lblKoeffAlarmCurPower.TabStop = false;
            this.lblKoeffAlarmCurPower.Text = @"Коэфф. сигн. Pтек";

            // 
            // m_nudnKoeffAlarmCurPower
            // 
            indx = (int)INDEX_CONTROL_UI.NUDN_KOEFFALARMCURPOWER;
            m_nudnKoeffAlarmCurPower.Enabled = this.m_cbxAlarm.Checked;
            m_nudnKoeffAlarmCurPower.Location = arRectControlUI[indx].Location;
            m_nudnKoeffAlarmCurPower.Name = "nudnKoeffAlarmCurPower";
            m_nudnKoeffAlarmCurPower.Size = arRectControlUI[indx].Size;
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
            indx = (int)INDEX_CONTROL_UI.BUTTON_ALARMCURPOWER;
            this.m_btnAlarmCurPower.Enabled = false;
            this.m_btnAlarmCurPower.Location = arRectControlUI[indx].Location;
            this.m_btnAlarmCurPower.Name = "btnAlarmCurPower";
            this.m_btnAlarmCurPower.Size = arRectControlUI[indx].Size;
            this.m_btnAlarmCurPower.TabIndex = 2;
            this.m_btnAlarmCurPower.Text = "Подтв. сигн. Pтек";
            this.m_btnAlarmCurPower.UseVisualStyleBackColor = true;
            this.m_btnAlarmCurPower.Click += new System.EventHandler(this.btnAlarmCurPower_Click);

            // 
            // m_btnAlarmTGTurnOnOff
            // 
            indx = (int)INDEX_CONTROL_UI.BUTTON_ALARMTGTUTNONOFF;
            this.m_btnAlarmTGTurnOnOff.Enabled = false;
            this.m_btnAlarmTGTurnOnOff.Location = arRectControlUI[indx].Location;
            this.m_btnAlarmTGTurnOnOff.Name = "btnAlarmTGTurnOnOff";
            this.m_btnAlarmTGTurnOnOff.Size = arRectControlUI[indx].Size;
            this.m_btnAlarmTGTurnOnOff.TabIndex = 2;
            this.m_btnAlarmTGTurnOnOff.Text = "Подтв. сигн. ТГвкл/откл";
            this.m_btnAlarmTGTurnOnOff.UseVisualStyleBackColor = true;
            this.m_btnAlarmTGTurnOnOff.Click += new System.EventHandler(this.btnAlarmTGTurnOnOff_Click);

            // 
            // m_panelLabelAlarm
            // 
            indx = (int)INDEX_CONTROL_UI.PANEL_ALARMSOURCES;
            this.m_panelLabelAlarm.Dock = DockStyle.None;
            this.m_panelLabelAlarm.Enabled = false;
            this.m_panelLabelAlarm.Location = arRectControlUI[indx].Location;
            this.m_panelLabelAlarm.Size = arRectControlUI[indx].Size;
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
            //this.HandleCreated += new EventHandler(panelAdminKomDisp_HandleCreated);

            this.m_cbxAlarm.Enabled = PanelAdminKomDisp.ALARM_USE;
            this.m_cbxAlarm.Checked = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.AUTO_ALARM_KOMDISP); //false; //PanelAdminKomDisp.ALARM_USE;

            //if ((m_cbxAlarm.Checked == true)
            //    && (ALARM_USE == true))
            //{
            //    initAdminAlarm();

            //    m_adminAlarm.Start();
            //} else ;

            this.m_nudnKoeffAlarmCurPower.ReadOnly = true;
            this.m_nudnKoeffAlarmCurPower.ValueChanged += new EventHandler(NudnKoeffAlarmCurPower_ValueChanged);
        }

        public override bool Activate(bool activate)
        {
            //Значит пользователь администратор
            if (m_adminAlarm == null) initAdminAlarm(); else ;

            if ((activate == true)
                && (m_adminAlarm.IsStarted == false))
                m_adminAlarm.Start();
            else ;

            return base.Activate (activate);
        }

        private void initAdminAlarm()
        {
            m_adminAlarm = new AdminAlarm();
            m_adminAlarm.InitTEC(m_admin.m_list_tec);

            m_adminAlarm.EventAdd += new AdminAlarm.DelegateOnEventReg(OnAdminAlarm_EventAdd);
            m_adminAlarm.EventRetry += new AdminAlarm.DelegateOnEventReg(OnAdminAlarm_EventRetry);

            this.EventConfirm += new DelegateIntIntFunc(m_adminAlarm.OnEventConfirm);
        }

        /// <summary>
        /// Вкл./выкл "доступности" в зависимости наличия события сигнализации для тек. ГТП
        /// </summary>
        /// <param name="id_comp">идентификатор ГТП</param>
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

        /// <summary>
        /// Вкл./выкл "доступности" в зависимости наличия события сигнализации для тек. ГТП
        /// </summary>
        /// <param name="id_comp">идентификатор ГТП</param>
        /// <param name="id_tg">идентификатор ТГ из состава ГТП</param>
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

        private TECComponent findTECComponent(int id) { return m_admin.FindTECComponent(id); }

        /// <summary>
        /// Добавить "напоминание" на панели о событии для ГТП (ТГ)
        /// </summary>
        /// <param name="id">идентификатор ГТП</param>
        /// <param name="id_tg">идентификатор ТГ</param>
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

        private void panelAdminKomDisp_HandleCreated (object obj, EventArgs ev)
        {
        }

        private void OnAdminAlarm_EventAdd(TecView.EventRegEventArgs ev)
        {
            if (IsHandleCreated/*InvokeRequired*/ == true)
            {
                this.BeginInvoke(new DelegateIntIntFunc(EnabledButtonAlarm), ev.m_id_gtp, ev.m_id_tg);

                this.BeginInvoke(new DelegateIntIntFunc(AddLabelAlarm), ev.m_id_gtp, ev.m_id_tg);
            }
            else {
                Logging.Logg().Error(@"PanelAdminKomDisp::OnAdminAlarm_EventAdd () - ... BeginInvoke (EnabledButtonAlarm, AddLabelAlarm) - ...", Logging.INDEX_MESSAGE.D_001);

                EnabledButtonAlarm(ev.m_id_gtp, ev.m_id_tg);

                AddLabelAlarm(ev.m_id_gtp, ev.m_id_tg);
            }

            toEventGUIReg(ev);
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
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.UDGe: // УДГэ
                            //valid = double.TryParse((string)dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.UDGe].Value, out value);
                            //m_admin.m_curRDGValues[i]. = value;
                            break;
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION: // Рекомендация
                            {
                                //cellValidated(e.RowIndex, (int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION);

                                valid = double.TryParse((string)dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value, out value);
                                m_admin.m_curRDGValues[i].recomendation = value;

                                break;
                            }
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.FOREIGN_CMD:
                            if (!(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.FOREIGN_CMD].Value == null))
                                m_admin.m_curRDGValues[i].fc = bool.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.FOREIGN_CMD].Value.ToString());
                            else
                                m_admin.m_curRDGValues[i].fc = false;

                            break;
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
            if (IsHandleCreated/*InvokeRequired*/ == true)
            {
                m_evtAdminTableRowCount.Reset();
                this.BeginInvoke(new DelegateFunc(normalizedTableHourRows));
                m_evtAdminTableRowCount.WaitOne(System.Threading.Timeout.Infinite);
            }
            else
                Logging.Logg().Error(@"PanelTAdminKomDisp::setDataGridViewAdmin () - ... BeginInvoke (normalizedTableHourRows) - ...", Logging.INDEX_MESSAGE.D_001);

            ((DataGridViewAdminKomDisp)this.dgwAdminTable).m_PBR_0 = m_admin.m_curRDGValues_PBR_0;

            for (int i = 0; i < m_admin.m_curRDGValues.Length; i++)
            {
                strFmtDatetime = m_admin.GetFmtDatetime (i);
                offset = m_admin.GetSeasonHourOffset (i + 1);

                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DATE_HOUR].Value = date.AddHours(i + 1 - offset).ToString(strFmtDatetime);

                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].Value = m_admin.m_curRDGValues[i].pbr.ToString("F2");
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].ToolTipText = m_admin.m_curRDGValues[i].pbr_number;
                if (i > 0)
                    this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.UDGe].Value = (((m_admin.m_curRDGValues[i].pbr + m_admin.m_curRDGValues[i - 1].pbr) / 2) + m_admin.m_curRDGValues[i].recomendation).ToString("F2");
                else
                    this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.UDGe].Value = (((m_admin.m_curRDGValues[i].pbr + m_admin.m_curRDGValues_PBR_0) / 2) + m_admin.m_curRDGValues[i].recomendation).ToString("F2");
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value = m_admin.m_curRDGValues[i].recomendation.ToString("F2");
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].ToolTipText = m_admin.m_curRDGValues[i].dtRecUpdate.ToString ();
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.FOREIGN_CMD].Value = m_admin.m_curRDGValues[i].fc.ToString();
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE].Value = m_admin.m_curRDGValues[i].deviationPercent.ToString();
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION].Value = m_admin.m_curRDGValues[i].deviation.ToString("F2");
            }

            //this.dgwAdminTable.Invalidate();

            m_admin.CopyCurToPrevRDGValues();
        }

        public override void ClearTables()
        {
            this.dgwAdminTable.ClearTables();
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

        private void btnImportCSV_PBRValues_Click(object sender, EventArgs e)
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
            files.InitialDirectory = FormMain.formParameters.m_arParametrSetup [(int)FormParameters.PARAMETR_SETUP.KOMDISP_FOLDER_CSV]; //@"\\ne2844\2.X.X\ПБР-csv"; //@"E:\Temp\ПБР-csv";
            files.DefaultExt = @"csv";
            files.Filter = @"csv файлы (*.csv)|*.csv";
            files.Title = "Выберите файл с ПБР...";

            if (files.ShowDialog(FormMain.formParameters) == DialogResult.OK) {
                int iRes = 0
                    , curPBRNumber = m_admin.GetPBRNumber (); //Текущий номер ПБР
                //Дата ПБР, номер ПБР из наименования файла
                object[] prop = AdminTS_KomDisp.GetPropertiesOfNameFilePPBRCSVValues(files.FileName);

                //if (!((DateTime)prop[0] == DateTime.Now.Date))
                if (!((DateTime)prop[0] == m_admin.m_curDate.Date))
                {
                    iRes = -1;
                }
                else
                {
                    //Сравнить с текущим номером ПБР
                    if (!((int)prop[1] > curPBRNumber))
                        iRes = -2;
                    else
                        ; //iRes = 0
                }

                //Проверка на ошибки
                if (!(iRes == 0))
                {
                    string strMsg = string.Empty;
                    //Ошибка по дате
                    if (iRes == -1)
                    {
                        strMsg = string.Format(@"Дата загружаемого [{0}] набора ПБР не соответствует установл./дате [{1}]", ((DateTime)prop[0]).ToString(@"dd.MM.yyyy"), DateTime.Now.Date.ToString(@"dd.MM.yyyy"));
                        MessageBox.Show(this, strMsg, @"Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        //Ошибка по номеру ПБР
                        if (iRes == -2)
                        {
                            strMsg = string.Format(@"Номер загружаемого набора [{0}] ПБР не выше, чем текущий [{1}].{2}Продолжить?", (int)prop[1], curPBRNumber, Environment.NewLine);
                            if (MessageBox.Show(this, strMsg, @"Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                            {
                                iRes = 0;
                            }
                            else
                                ;
                        }
                        else
                            ;
                    }
                }
                else
                    ;

                //Еще одна проверка на ошибки (т.к. была возможность ее подтвердить)
                if (iRes == 0)
                    ((AdminTS_KomDisp)m_admin).ImpCSVValues(mcldrDate.SelectionStart, files.FileName);
                else
                    ;
            }
            else
                ;
        }

        //private void ckbImportCSV_AdminDefaultValues_CheckedChanged(object sender, EventArgs e)
        //{
        //}

        private string getSharedFolderRun () {
            string strRes = string.Empty;

            strRes = Path.GetPathRoot(Application.ExecutablePath);

            return strRes;
        }

        private void btnImportCSV_AdminValuesDefault_Click(object sender, EventArgs e)
        {
            int days = (m_admin.m_curDate.Date - HAdmin.ToMoscowTimeZone(DateTime.Now).Date).Days;
            if (days < 0)
            {
                string strMsg = string.Format(@"Выбрана дата ретроспективных данных: {0}.", m_admin.m_curDate.Date.ToString(@"dd.MM.yyyy"));
                MessageBox.Show(this, strMsg, @"Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                OpenFileDialog files = new OpenFileDialog ();
                files.Multiselect = false;
                //files.InitialDirectory = Environment.GetFolderPath (Environment.SpecialFolder.Desktop);
                files.InitialDirectory = FormMain.formParameters.m_arParametrSetup [(int)FormParameters.PARAMETR_SETUP.KOMDISP_FOLDER_CSV]; //@"\\ne2844\2.X.X\ПБР-csv"; //@"E:\Temp\ПБР-csv";
                files.DefaultExt = @"csv";
                files.Filter = @"Рекомендации-по-умолчанию (AdminValuesDefault.csv)|AdminValuesDefault.csv";
                files.Title = "Выберите файл со рекомендациями по умолчанию...";

                int iRes = -1;
                if (files.ShowDialog(FormMain.formParameters) == DialogResult.OK) {
                    if (days > 0)
                    {
                        iRes = 0;
                    }
                    else
                    {
                        if (days == 0)
                        {
                            string strMsg = string.Format(@"Рекомендации по умолчанию будут загружены на текущие сутки: {0}.{1}Продолжить?", m_admin.m_curDate.Date.ToString(@"dd.MM.yyyy"), Environment.NewLine);
                            if (MessageBox.Show(this, strMsg, @"Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                                iRes = 0;
                            else
                                ; //По-прежнему ошибка...
                        }
                        else
                            ;
                    }

                    if (iRes == 0)
                        ((AdminTS_KomDisp)m_admin).ImpCSVValues(mcldrDate.SelectionStart, files.FileName);
                    else
                        ;
                } else { }
            }
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

