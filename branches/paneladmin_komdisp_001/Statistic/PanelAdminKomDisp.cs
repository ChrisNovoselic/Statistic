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


using StatisticCommon;
using StatisticAlarm;
using ASUTP;
using ASUTP.Core;

namespace Statistic
{
    public class PanelAdminKomDisp : PanelAdmin
    {
        private System.Windows.Forms.Button btnImportCSV_PBRValues;
        private System.Windows.Forms.Button btnImportCSV_AdminDefaultValues;
        private GroupBoxDividerChoice gbxDividerChoice;
        private System.Windows.Forms.Button btnExport_PBRValues;
        private System.Windows.Forms.CheckBox cbMSExcelVisibledExport_PBRValues;
        private System.Windows.Forms.CheckBox cbAutoExport_PBRValues;
        private System.Windows.Forms.Label labelSheduleExport_PBRValues;
        private System.Windows.Forms.DateTimePicker dtpSheduleStartExport_PBRValues;
        private System.Windows.Forms.Label labelPeriodExport_PBRValues;
        private System.Windows.Forms.DateTimePicker dtpShedulePeriodExport_PBRValues;

        private enum INDEX_CONTROL_UI
        {
            UNKNOWN = -1
            , BUTTON_CSV_IMPORT_PBR, BUTTON_CSV_IMPORT_ADMINVALUESDEFAULT
            , BUTTON_EXPORT_PBR, CB_MSECEL_VISIBLED_EXPORT_PBR, CB_AUTO_EXPORT_PBR, LABEL_SHEDULE_EXPORT_PBR, LABEL_PERIOD_EXPORT_PBR, DTP_SHEDULE_EXPORT_PBR, DTP_PERIOD_EXPORT_PBR
                , COUNT
        };

        protected override void InitializeComponents()
        {
            base.InitializeComponents ();

            int posY = 271
                , offsetPosY = m_iSizeY + 2 * m_iMarginY
                , iMarginX = m_iMarginY
                , width = 154
                , width2 = 154 / 2 - iMarginX
                , indx = -1;
            Rectangle[] arRectControlUI = new Rectangle[] {
                new Rectangle (new Point (10, posY), new Size (154, m_iSizeY)) //BUTTON_CSV_IMPORT_PBR
                , new Rectangle (new Point (10, posY + 1 * (m_iSizeY + m_iMarginY)), new Size (154, m_iSizeY)) //, BUTTON_CSV_IMPORT_ADMINVALUESDEFAULT
                // ------ ����������� ------
                , new Rectangle (new Point (10, posY + (int)(2.7 * (m_iSizeY + m_iMarginY))), new Size (width, m_iSizeY)) //, BUTTON_EXPORT_PBR
                , new Rectangle (new Point (10, posY + (int)(3.7 * (m_iSizeY + m_iMarginY))), new Size (width, m_iSizeY)) //, CB_MSEXCEL_VISIBLED
                , new Rectangle (new Point (10, posY + (int)(4.7 * (m_iSizeY + m_iMarginY))), new Size (width, m_iSizeY)) //, CB_AUTO_EXPORT_PBR
                , new Rectangle (new Point (10, posY + (int)(5.6 * (m_iSizeY + m_iMarginY))), new Size (width2, m_iSizeY)) //, LABEL_SHEDULE_EXPORT_PBR
                , new Rectangle (new Point (10 + width2 + 2 * iMarginX, posY + (int)(5.6 * (m_iSizeY + m_iMarginY))), new Size (width2, m_iSizeY)) //, LABEL_PERIOD_EXPORT_PBR
                , new Rectangle (new Point (10, posY + (int)(6.5 * (m_iSizeY + m_iMarginY))), new Size (width2, m_iSizeY)) //, DTP_SHEDULE_EXPORT_PBR
                , new Rectangle (new Point (10 + width2 + 2 * iMarginX, posY + (int)(6.5 * (m_iSizeY + m_iMarginY))), new Size (width2, m_iSizeY)) //, DTP_PERIOD_EXPORT_PBR
            };

            this.btnImportCSV_PBRValues = new Button();
            this.btnImportCSV_AdminDefaultValues = new Button();
            this.btnExport_PBRValues = new Button();
            this.cbMSExcelVisibledExport_PBRValues = new CheckBox();
            this.cbAutoExport_PBRValues = new CheckBox();
            this.labelSheduleExport_PBRValues = new Label();
            this.dtpSheduleStartExport_PBRValues = new DateTimePicker();
            this.dtpShedulePeriodExport_PBRValues = new DateTimePicker();
            this.labelPeriodExport_PBRValues = new Label();
            this.gbxDividerChoice = new GroupBoxDividerChoice();
            this.dgwAdminTable = new DataGridViewAdminKomDisp(FormMain.formGraphicsSettings.FontColor
                , new Color [] { FormMain.formGraphicsSettings.BackgroundColor == SystemColors.Control ? SystemColors.Window : FormMain.formGraphicsSettings.BackgroundColor
                    , Color.Yellow
                    , FormMain.formGraphicsSettings.COLOR (FormGraphicsSettings.INDEX_COLOR_VAUES.DIVIATION)
                });

            this.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).BeginInit();

            this.m_panelManagement.Controls.Add(this.btnImportCSV_PBRValues);
            this.m_panelManagement.Controls.Add(this.btnImportCSV_AdminDefaultValues);
            this.m_panelManagement.Controls.Add(this.gbxDividerChoice);
            this.m_panelManagement.Controls.Add(this.btnExport_PBRValues);
            this.m_panelManagement.Controls.Add(this.cbMSExcelVisibledExport_PBRValues);
            this.m_panelManagement.Controls.Add(this.cbAutoExport_PBRValues);
            this.m_panelManagement.Controls.Add(this.labelSheduleExport_PBRValues);
            this.m_panelManagement.Controls.Add(this.labelPeriodExport_PBRValues);
            this.m_panelManagement.Controls.Add(this.dtpSheduleStartExport_PBRValues);
            this.m_panelManagement.Controls.Add(this.dtpShedulePeriodExport_PBRValues);
            this.m_panelRDGValues.Controls.Add(this.dgwAdminTable);

            // 
            // btnImportCSV_PBRValues
            //
            indx = (int)INDEX_CONTROL_UI.BUTTON_CSV_IMPORT_PBR;
            this.btnImportCSV_PBRValues.Location = arRectControlUI [indx].Location;
            this.btnImportCSV_PBRValues.Name = "btnImportCSV_PBRValues";
            this.btnImportCSV_PBRValues.Size = arRectControlUI[indx].Size;
            this.btnImportCSV_PBRValues.TabIndex = 2;
            this.btnImportCSV_PBRValues.Text = "������ �� ������� CSV";
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
            this.btnImportCSV_AdminDefaultValues.Text = "�����. �� ���������";
            this.btnImportCSV_AdminDefaultValues.UseVisualStyleBackColor = true;
            this.btnImportCSV_AdminDefaultValues.Click += new System.EventHandler(this.btnImportCSV_AdminValuesDefault_Click);
            //this.ckbImportCSV_AdminDefaultValues.CheckedChanged += new EventHandler(ckbImportCSV_AdminDefaultValues_CheckedChanged);
            this.btnImportCSV_AdminDefaultValues.Enabled = true;
            //
            // gbxDividerChoice
            //
            gbxDividerChoice.Initialize(posY + 0 * (m_iSizeY + m_iMarginY));
            // 
            // btnExport_PBRValues
            //
            indx = (int)INDEX_CONTROL_UI.BUTTON_EXPORT_PBR;
            this.btnExport_PBRValues.Location = arRectControlUI[indx].Location;
            this.btnExport_PBRValues.Name = "btnExport_PBRValues";
            this.btnExport_PBRValues.Size = arRectControlUI[indx].Size;
            this.btnExport_PBRValues.TabIndex = 2;
            this.btnExport_PBRValues.Text = "������� ���";
            this.btnExport_PBRValues.UseVisualStyleBackColor = true;
            this.btnExport_PBRValues.Click += new System.EventHandler(this.btnExport_PBRValues_Click);
            this.btnExport_PBRValues.Enabled = EnabledExportPBRValues;
            // 
            // cbMSExcelVisibledExport_PBRValues
            //
            indx = (int)INDEX_CONTROL_UI.CB_MSECEL_VISIBLED_EXPORT_PBR;
            this.cbMSExcelVisibledExport_PBRValues.Location = arRectControlUI[indx].Location;
            this.cbMSExcelVisibledExport_PBRValues.Name = "cbMSExcelVisibledExport_PBRValues";
            this.cbMSExcelVisibledExport_PBRValues.Size = arRectControlUI[indx].Size;
            this.cbMSExcelVisibledExport_PBRValues.TabIndex = 2;
            this.cbMSExcelVisibledExport_PBRValues.Text = "�������� ����������";
            this.cbMSExcelVisibledExport_PBRValues.CheckedChanged += cbMSExcelVisibledExport_PBRValues_CheckedChanged;
            this.cbMSExcelVisibledExport_PBRValues.Enabled = AllowUserSetModeExportPBRValues;
            this.cbMSExcelVisibledExport_PBRValues.Appearance = Appearance.Button;
            this.cbMSExcelVisibledExport_PBRValues.TextAlign = this.cbMSExcelVisibledExport_PBRValues.Appearance == Appearance.Normal
                ? ContentAlignment.MiddleLeft
                    : this.cbMSExcelVisibledExport_PBRValues.Appearance == Appearance.Button
                        ? ContentAlignment.MiddleCenter
                            : ContentAlignment.MiddleLeft;
            // 
            // cbAutoExport_PBRValues
            //
            indx = (int)INDEX_CONTROL_UI.CB_AUTO_EXPORT_PBR;
            this.cbAutoExport_PBRValues.Location = arRectControlUI[indx].Location;
            this.cbAutoExport_PBRValues.Name = "cbAutoExport_PBRValues";
            this.cbAutoExport_PBRValues.Size = arRectControlUI[indx].Size;
            this.cbAutoExport_PBRValues.TabIndex = 2;
            this.cbAutoExport_PBRValues.Text = "������������� (���.)";
            this.cbAutoExport_PBRValues.CheckedChanged += cbAutoExport_PBRValues_CheckedChanged;
            this.cbAutoExport_PBRValues.Enabled = AllowUserSetModeExportPBRValues;
            // 
            // labelSheduleExport_PBRValues
            //
            indx = (int)INDEX_CONTROL_UI.LABEL_SHEDULE_EXPORT_PBR;
            this.labelSheduleExport_PBRValues.Location = arRectControlUI[indx].Location;
            this.labelSheduleExport_PBRValues.Name = "labelSheduleExport_PBRValues";
            this.labelSheduleExport_PBRValues.Size = arRectControlUI[indx].Size;
            this.labelSheduleExport_PBRValues.TabIndex = 2;
            this.labelSheduleExport_PBRValues.Text = "�������� �:";
            // 
            // labelPeriodExport_PBRValues
            //
            indx = (int)INDEX_CONTROL_UI.LABEL_PERIOD_EXPORT_PBR;
            this.labelPeriodExport_PBRValues.Location = arRectControlUI[indx].Location;
            this.labelPeriodExport_PBRValues.Name = "labelPeriodExport_PBRValues";
            this.labelPeriodExport_PBRValues.Size = arRectControlUI[indx].Size;
            this.labelPeriodExport_PBRValues.TabIndex = 2;
            this.labelPeriodExport_PBRValues.Text = "������:";
            // 
            // dtpSheduleStartExport_PBRValues
            //
            indx = (int)INDEX_CONTROL_UI.DTP_SHEDULE_EXPORT_PBR;
            this.dtpSheduleStartExport_PBRValues.Location = arRectControlUI[indx].Location;
            this.dtpSheduleStartExport_PBRValues.Name = "dtpSheduleStartExport_PBRValues";
            this.dtpSheduleStartExport_PBRValues.Size = arRectControlUI[indx].Size;
            this.dtpSheduleStartExport_PBRValues.TabIndex = 2;
            this.dtpSheduleStartExport_PBRValues.Format = DateTimePickerFormat.Custom;
            this.dtpSheduleStartExport_PBRValues.CustomFormat = "00:mm:ss";
            this.dtpSheduleStartExport_PBRValues.ShowUpDown = true;
            this.dtpSheduleStartExport_PBRValues.Value = new DateTime(1970, 1, 1).AddSeconds(AdminTS_KomDisp.SEC_SHEDULE_START_EXPORT_PBR % 3600);
            this.dtpSheduleStartExport_PBRValues.ValueChanged += dtpSheduleStartExport_PBRValues_ValueChanged;
            this.dtpSheduleStartExport_PBRValues.Enabled = AllowUserChangeSheduleStartExportPBRValues;
            // 
            // dtpShedulePeriodExport_PBRValues
            //
            indx = (int)INDEX_CONTROL_UI.DTP_PERIOD_EXPORT_PBR;
            this.dtpShedulePeriodExport_PBRValues.Location = arRectControlUI[indx].Location;
            this.dtpShedulePeriodExport_PBRValues.Name = "dtpShedulePeriodExport_PBRValues";
            this.dtpShedulePeriodExport_PBRValues.Size = arRectControlUI[indx].Size;
            this.dtpShedulePeriodExport_PBRValues.TabIndex = 2;
            this.dtpShedulePeriodExport_PBRValues.Format = DateTimePickerFormat.Custom;
            this.dtpShedulePeriodExport_PBRValues.CustomFormat = "HH:mm:ss";
            this.dtpShedulePeriodExport_PBRValues.ShowUpDown = true;
            this.dtpShedulePeriodExport_PBRValues.Value = new DateTime(1970, 1, 1).AddSeconds(AdminTS_KomDisp.SEC_SHEDULE_PERIOD_EXPORT_PBR);
            this.dtpShedulePeriodExport_PBRValues.ValueChanged += dtpShedulePeriodExport_PBRValues_ValueChanged; ;
            this.dtpShedulePeriodExport_PBRValues.Enabled = AllowUserChangeShedulePeriodExportPBRValues;
            // 
            // dgwAdminTable
            //
            this.dgwAdminTable.Location = new System.Drawing.Point(9, 9);
            this.dgwAdminTable.Size = new System.Drawing.Size(714, 591);
            this.dgwAdminTable.TabIndex = 1;
            //this.dgwAdminTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellClick);
            //this.dgwAdminTable.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellValidated);
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).EndInit();

            this.ResumeLayout();

            cbAutoExport_PBRValues.Checked = EnabledExportPBRValues 
                & (HStatisticUsers.IsAllowed ((int)HStatisticUsers.ID_ALLOWED.AUTO_EXPORT_PBRVALUES_KOMDISP));
        }

        private void admin_onEventExportPBRValues (AdminTS_KomDisp.MSExcelIOExportPBRValues.EventResultArgs ev)
        {
            switch (ev.Result) {
                case AdminTS_KomDisp.MSExcelIOExportPBRValues.RESULT.SHEDULE:
                    doExportPBRValues ();
                    break;
                default:
                    break;
            }
        }

        private void doExportPBRValues ()
        {
            DateTime date; // ���� ��� �������� �������� 

            // �� ���������� �������� �������� ��������� ������������ ����� � ��������(DISPLAY - �� ���������)
            ModeGetRDGValues = MODE_GET_RDG_VALUES.EXPORT;

            (m_admin as AdminTS_KomDisp).PrepareExportRDGValues (m_listTECComponentIndex);

            if (m_listTECComponentIndex.Count > 0) {
                date = Admin.DateDoExportPBRValues;

                if (date.Equals(DateTime.MinValue) == true)
                // 'Admin.DateDoExportPBRValues' �� ��������� ��������, ������ ����� 'MODE_GET_RDG_VALUES.MANUAL'
                // , ������� ����� �������� �� ����������� ����������
                    date = mcldrDate.SelectionStart.Date;
                else
                    ;

                m_admin.GetRDGValues(m_listTECComponentIndex[0], date);
            } else
                Logging.Logg().Error(string.Format("PanelAdin_KomDisp::doExportPBRValues () - �� ������� ��� ��� ��������..."), Logging.INDEX_MESSAGE.NOT_SET);
            ;
        }

        private void btnExport_PBRValues_Click(object sender, EventArgs e)
        {
            doExportPBRValues ();
        }

        public static bool EnabledExportPBRValues = false;

        public static bool AllowUserSetModeExportPBRValues = true;

        private static bool AllowUserChangeSheduleStartExportPBRValues = false;

        private static bool AllowUserChangeShedulePeriodExportPBRValues = false;

        private void cbAutoExport_PBRValues_CheckedChanged(object sender, EventArgs e)
        {
            bool bChecked = false;

            bChecked = (sender as CheckBox).Checked;

            Admin.SetModeExportPBRValues((bChecked == true) ? AdminTS_KomDisp.MODE_EXPORT_PBRVALUES.AUTO : AdminTS_KomDisp.MODE_EXPORT_PBRVALUES.MANUAL);

            cbMSExcelVisibledExport_PBRValues.Checked = bChecked == true
                ? !bChecked // �������������
                    : cbMSExcelVisibledExport_PBRValues.Checked; // �������� "��� ����"

            btnExport_PBRValues.Enabled =
            cbMSExcelVisibledExport_PBRValues.Enabled =
                !bChecked;
            dtpSheduleStartExport_PBRValues.Enabled =
                 AllowUserChangeSheduleStartExportPBRValues && !bChecked;
            dtpShedulePeriodExport_PBRValues.Enabled =
                 AllowUserChangeShedulePeriodExportPBRValues && !bChecked;
        }

        private void cbMSExcelVisibledExport_PBRValues_CheckedChanged(object sender, EventArgs e)
        {
            Admin.SetAllowMSExcelVisibledExportPBRValues((sender as CheckBox).Checked);
        }

        private void dtpSheduleStartExport_PBRValues_ValueChanged(object sender, EventArgs e)
        {
            AdminTS_KomDisp.SEC_SHEDULE_START_EXPORT_PBR =
                (int)new TimeSpan(dtpSheduleStartExport_PBRValues.Value.Hour, dtpSheduleStartExport_PBRValues.Value.Minute, dtpSheduleStartExport_PBRValues.Value.Second).TotalSeconds;
        }

        private void dtpShedulePeriodExport_PBRValues_ValueChanged(object sender, EventArgs e)
        {
            AdminTS_KomDisp.SEC_SHEDULE_PERIOD_EXPORT_PBR =
                (int)new TimeSpan(dtpShedulePeriodExport_PBRValues.Value.Hour, dtpSheduleStartExport_PBRValues.Value.Minute, dtpSheduleStartExport_PBRValues.Value.Second).TotalSeconds;
        }

        public PanelAdminKomDisp(ASUTP.Core.HMark markQueries)
            : base(markQueries, new int[] { 0, (int)TECComponent.ID.GTP })
        {
            //??? ���������� �� �������� ������
            //InitializeComponents ();
        }

        public override bool Activate(bool activate)
        {
            bool bRes = base.Activate (activate);

            /*if (bRes == true)
                if (activate == true) {
                    dgwAdminTable.DefaultCellStyle.BackColor =
                        //BackColor == SystemColors.Control ? SystemColors.Window : BackColor
                        FormMain.formGraphicsSettings.BackgroundColor
                        ;
                } else
                    ;
            else
                ;*/

            return bRes;
        }

        private TECComponent findTECComponent(int id) { return m_admin.FindTECComponent(id); }

        private void panelAdminKomDisp_HandleCreated (object obj, EventArgs ev)
        {
        }

        protected override void getDataGridViewAdmin()
        {
            double value;
            bool valid;
            //int offset = -1;

            for (int i = 0; i < dgwAdminTable.Rows.Count; i++)
            {
                //offset = m_admin.GetSeasonHourOffset(i);
                
                for (int j = 0; j < (int)DataGridViewAdminKomDisp.COLUMN_INDEX.TO_ALL; j++)
                {
                    switch (j)
                    {
                        case (int)DataGridViewAdminKomDisp.COLUMN_INDEX.PLAN: // ����
                            valid = double.TryParse((string)dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.PLAN].Value, out value);
                            m_admin.m_curRDGValues[i].pbr = value;
                            //m_admin.m_curRDGValues[i].pmin = 0.0;
                            //m_admin.m_curRDGValues[i].pmax = 0.0;
                            break;
                        case (int)DataGridViewAdminKomDisp.COLUMN_INDEX.UDGe: // ����
                            //valid = double.TryParse((string)dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.UDGe].Value, out value);
                            //m_admin.m_curRDGValues[i]. = value;
                            break;
                        case (int)DataGridViewAdminKomDisp.COLUMN_INDEX.RECOMENDATION: // ������������
                            {
                                //cellValidated(e.RowIndex, (int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION);

                                valid = double.TryParse((string)dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.RECOMENDATION].Value, out value);
                                m_admin.m_curRDGValues[i].recomendation = value;

                                break;
                            }
                        case (int)DataGridViewAdminKomDisp.COLUMN_INDEX.FOREIGN_CMD:
                            if (!(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.FOREIGN_CMD].Value == null))
                                m_admin.m_curRDGValues[i].fc = bool.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.FOREIGN_CMD].Value.ToString());
                            else
                                m_admin.m_curRDGValues[i].fc = false;

                            break;
                        case (int)DataGridViewAdminKomDisp.COLUMN_INDEX.DEVIATION_TYPE:
                            {
                                if (!(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.DEVIATION_TYPE].Value == null))
                                    m_admin.m_curRDGValues[i].deviationPercent = bool.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.DEVIATION_TYPE].Value.ToString());
                                else
                                    m_admin.m_curRDGValues[i].deviationPercent = false;

                                break;
                            }
                        case (int)DataGridViewAdminKomDisp.COLUMN_INDEX.DEVIATION: // ������������ ����������
                            {
                                valid = double.TryParse((string)this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.DEVIATION].Value, out value);
                                m_admin.m_curRDGValues[i].deviation = value;

                                break;
                            }
                    }
                }
            }

            //m_admin.CopyCurRDGValues();
        }

        private AdminTS_KomDisp Admin { get { return m_admin as AdminTS_KomDisp; } }

        /// <summary>
        /// ���������� �������� � �������������
        /// </summary>
        /// <param name="date">����, �� ������� �������� �������� ��� �����������</param>
        /// <param name="bNewValues">������� ������� ����� ��������, ����� ��������� �������� ���������� �������������</param>
        public override void setDataGridViewAdmin(DateTime date, bool bNewValues)
        {
            int offset = -1
                , nextIndx = -1;
            string strFmtDatetime = string.Empty;
            IAsyncResult iar;

            if ((ModeGetRDGValues & MODE_GET_RDG_VALUES.DISPLAY) == MODE_GET_RDG_VALUES.DISPLAY) {
                //??? �� ����� ������� �������
                if (IsHandleCreated == true)
                {
                    if (InvokeRequired == true) {
                        //m_evtAdminTableRowCount.Reset ();
                        // ���-�� ����� ����� ���� ��������(�������������) ������ � ��� ������,� ������� ���� ��������� �������� �������� ����������
                        iar = this.BeginInvoke (new DelegateBoolFunc (normalizedTableHourRows), InvokeRequired);
                        //??? �������, ���� �� ���������� ���������� ����������� ������
                        //m_evtAdminTableRowCount.WaitOne (System.Threading.Timeout.Infinite);
                        WaitHandle.WaitAny (new WaitHandle [] { iar.AsyncWaitHandle }, System.Threading.Timeout.Infinite);
                        this.EndInvoke (iar);
                    } else {
                        normalizedTableHourRows (InvokeRequired);
                    }
                }
                else
                    Logging.Logg().Error(@"PanelAdminKomDisp::setDataGridViewAdmin () - ... BeginInvoke (normalizedTableHourRows) - ...", Logging.INDEX_MESSAGE.D_001);

                ((DataGridViewAdminKomDisp)this.dgwAdminTable).m_PBR_0 = m_admin.m_curRDGValues_PBR_0;

                //??? ���������� �������� - ������ �� ������ ������-������� �������������
                for (int i = 0; i < m_admin.m_curRDGValues.Length; i++)
                {
                    strFmtDatetime = m_admin.GetFmtDatetime(i);
                    offset = m_admin.GetSeasonHourOffset(i + 1);

                    this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.DATE_HOUR].Value = date.AddHours(i + 1 - offset).ToString(strFmtDatetime);
                    //this.dgwAdminTable.Rows [i].Cells [(int)DataGridViewAdminKomDisp.COLUMN_INDEX.DATE_HOUR].Style.BackColor = this.dgwAdminTable.BackColor;

                    this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.PLAN].Value = m_admin.m_curRDGValues[i].pbr.ToString("F2");
                    this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.PLAN].ToolTipText = m_admin.m_curRDGValues[i].pbr_number;
                    //this.dgwAdminTable.Rows [i].Cells [(int)DataGridViewAdminKomDisp.COLUMN_INDEX.PLAN].Style.BackColor = this.dgwAdminTable.BackColor;
                    if (i > 0)
                        this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.UDGe].Value = (((m_admin.m_curRDGValues[i].pbr + m_admin.m_curRDGValues[i - 1].pbr) / 2) + m_admin.m_curRDGValues[i].recomendation).ToString("F2");
                    else
                        this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.UDGe].Value = (((m_admin.m_curRDGValues[i].pbr + m_admin.m_curRDGValues_PBR_0) / 2) + m_admin.m_curRDGValues[i].recomendation).ToString("F2");
                    //this.dgwAdminTable.Rows [i].Cells [(int)DataGridViewAdminKomDisp.COLUMN_INDEX.UDGe].Style.BackColor = this.dgwAdminTable.BackColor;
                    this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.RECOMENDATION].Value = m_admin.m_curRDGValues[i].recomendation.ToString("F2");
                    this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.RECOMENDATION].ToolTipText = m_admin.m_curRDGValues[i].dtRecUpdate.ToString();
                    //this.dgwAdminTable.Rows [i].Cells [(int)DataGridViewAdminKomDisp.COLUMN_INDEX.RECOMENDATION].Style.BackColor = this.dgwAdminTable.BackColor;
                    this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.FOREIGN_CMD].Value = m_admin.m_curRDGValues[i].fc.ToString();
                    //this.dgwAdminTable.Rows [i].Cells [(int)DataGridViewAdminKomDisp.COLUMN_INDEX.FOREIGN_CMD].Style.BackColor = this.dgwAdminTable.BackColor;
                    this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.DEVIATION_TYPE].Value = m_admin.m_curRDGValues[i].deviationPercent.ToString();
                    //this.dgwAdminTable.Rows [i].Cells [(int)DataGridViewAdminKomDisp.COLUMN_INDEX.DEVIATION_TYPE].Style.BackColor = this.dgwAdminTable.BackColor;
                    this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.COLUMN_INDEX.DEVIATION].Value = m_admin.m_curRDGValues[i].deviation.ToString("F2");
                    //this.dgwAdminTable.Rows [i].Cells [(int)DataGridViewAdminKomDisp.COLUMN_INDEX.DEVIATION].Style.BackColor = this.dgwAdminTable.BackColor;
                }

                if (bNewValues == true)
                    m_admin.CopyCurToPrevRDGValues ();
                else
                    ;
            } else if ((ModeGetRDGValues & MODE_GET_RDG_VALUES.EXPORT) == MODE_GET_RDG_VALUES.EXPORT) {
                nextIndx = Admin.AddValueToExportRDGValues(m_admin.m_curRDGValues, date);

                if (nextIndx < 0)
                    Invoke(new Action(btnRefresh.PerformClick));
                else
                    Admin.GetRDGValues(nextIndx, date);
            }
        }

        public override void ClearTables()
        {
            this.dgwAdminTable.ClearTables();
        }

        public override void InitializeComboBoxTecComponent(FormChangeMode.MODE_TECCOMPONENT mode)
        {
            base.InitializeComboBoxTecComponent(mode);

            m_listTECComponentIndex.ForEach(indx => comboBoxTecComponent.Items.Add(m_admin.allTECComponents[indx].tec.name_shr + " - " + m_admin.GetNameTECComponent(indx)));
            //for (int i = 0; i < m_listTECComponentIndex.Count; i++)
            //    comboBoxTecComponent.Items.Add(m_admin.allTECComponents[m_listTECComponentIndex[i]].tec.name_shr + " - " + m_admin.GetNameTECComponent(m_listTECComponentIndex[i]));

            if (comboBoxTecComponent.Items.Count > 0)
            {
                m_admin.indxTECComponents = m_listTECComponentIndex[0];
                comboBoxTecComponent.SelectedIndex = 0;
            }
            else
                ;
        }

        private void btnImportCSV_PBRValues_Click(object sender, EventArgs e)
        {
            int err = -1; // ������� ������ ��� ����������� ������ ���

            //������� �1 (�������)
            //FolderBrowserDialog folders = new FolderBrowserDialog();
            //folders.ShowNewFolderButton = false;
            //folders.RootFolder = Environment.SpecialFolder.Desktop;
            //folders.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); //@"D:\Temp";

            //if (folders.ShowDialog(FormMain.formParameters) == DialogResult.OK)
            //    ((AdminTS_KomDisp)m_admin).ImpPPBRCSVValues(mcldrDate.SelectionStart, folders.SelectedPath + @"\");
            //else
            //    ;

            //������� �2 (����)
            OpenFileDialog files = new OpenFileDialog ();
            files.Multiselect = false;
            //files.InitialDirectory = Environment.GetFolderPath (Environment.SpecialFolder.Desktop);
            files.InitialDirectory = AdminTS_KomDisp.Folder_CSV;
            files.DefaultExt = @"csv";
            files.Filter = @"csv ����� (*.csv)|*.csv";
            files.Title = "�������� ���� � ���...";

            if (files.ShowDialog(FormMain.formParameters) == DialogResult.OK) {
                Logging.Logg().Action(string.Format(@"PanelAdminKomDisp::btnImportCSV_PBRValues_Click () - ������ CSV-����� {0}...", files.FileName), Logging.INDEX_MESSAGE.NOT_SET);

                int iRes = 0
                    , curPBRNumber = m_admin.GetPBRNumber (out err); //������� ����� ���
                //���� ���, ����� ��� �� ������������ �����
                object[] prop = AdminTS_KomDisp.GetPropertiesOfNameFilePPBRCSVValues(files.FileName);

                //if (!((DateTime)prop[0] == DateTime.Now.Date))
                if (!(((DateTime)prop[0]).CompareTo(m_admin.m_curDate.Date) == 0)) {
                    iRes = -1;
                } else
                //�������� � ������� ������� ���
                // , ����� ��� �� ��������� �� ��������������� (err == 0)
                    if ((!((int)prop[1] > curPBRNumber))
                        && (err == 0))
                        iRes = -2;
                    else
                        ; //iRes = 0

                //�������� �� ������
                if (!(iRes == 0)) {
                    string strMsg = string.Empty;
                    //������ �� ����
                    if (iRes == -1) {
                        strMsg = string.Format(@"���� ������������ [{0:dd.MM.yyyy}] ������ ��� �� ������������� ��������./���� [{1:dd.MM.yyyy}]"
                            , ((DateTime)prop[0]), m_admin.m_curDate.Date);
                        MessageBox.Show(this, strMsg, @"����������", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    } else {
                        //������ �� ������ ���
                        if (iRes == -2) {
                            strMsg = string.Format(@"����� ������������ ������ [{0}] ��� �� ����, ��� ������� [{1}].{2}����������?", (int)prop[1], curPBRNumber, Environment.NewLine);
                            if (MessageBox.Show(this, strMsg, @"�������������", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes) {
                                iRes = 0;
                            } else
                                ;
                        } else
                            ;
                    }
                }
                else
                    ;

                //��� ���� �������� �� ������ (�.�. ���� ����������� �� �����������)
                if (iRes == 0)
                    ((AdminTS_KomDisp)m_admin).ImpCSVValues(mcldrDate.SelectionStart, files.FileName);
                else
                    Logging.Logg().Action(string.Format(@"PanelAdminKomDisp::btnImportCSV_PBRValues_Click () - ������ ������� �������� CSV-������, ������={0}...", iRes), Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
                Logging.Logg().Action(string.Format(@"PanelAdminKomDisp::btnImportCSV_PBRValues_Click () - ������ ������ CSV-������..."), Logging.INDEX_MESSAGE.NOT_SET);
        }

        private string SharedFolderRun {
            get {
                return Path.GetPathRoot(Application.ExecutablePath);
            }
        }

        private void btnImportCSV_AdminValuesDefault_Click(object sender, EventArgs e)
        {
            int days = (m_admin.m_curDate.Date - ASUTP.Core.HDateTime.ToMoscowTimeZone(DateTime.Now).Date).Days;
            if (days < 0)
            {
                string strMsg = string.Format(@"������� ���� ��������������� ������: {0}.", m_admin.m_curDate.Date.ToString(@"dd.MM.yyyy"));
                MessageBox.Show(this, strMsg, @"��������", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                OpenFileDialog files = new OpenFileDialog ();
                files.Multiselect = false;
                //files.InitialDirectory = Environment.GetFolderPath (Environment.SpecialFolder.Desktop);
                files.InitialDirectory = AdminTS_KomDisp.Folder_CSV;
                files.DefaultExt = @"csv";
                files.Filter = @"������������-��-��������� (AdminValuesDefault)|AdminValuesDefault*.csv";
                files.Title = "�������� ���� � �������������� �� ���������...";

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
                            string strMsg = string.Format(@"������������ �� ��������� ����� ��������� �� ������� �����: {0}.{1}����������?", m_admin.m_curDate.Date.ToString(@"dd.MM.yyyy"), Environment.NewLine);
                            if (MessageBox.Show(this, strMsg, @"�������������", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                                iRes = 0;
                            else
                                ; //��-�������� ������...
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

        protected override void comboBoxTecComponent_SelectionChangeCommitted(object sender, EventArgs e)
        {
            base.comboBoxTecComponent_SelectionChangeCommitted (sender, e);
        }

        protected override void createAdmin ()
        {
            EnabledExportPBRValues = (HStatisticUsers.IsAllowed ((int)HStatisticUsers.ID_ALLOWED.EXPORT_PBRVALUES_KOMDISP));
            //??? ���� ������� ������ �� �� ������������
            AdminTS_KomDisp.ModeDefaultExportPBRValues = AdminTS_KomDisp.MODE_EXPORT_PBRVALUES.MANUAL;
            AllowUserSetModeExportPBRValues = true & EnabledExportPBRValues;
            AllowUserChangeSheduleStartExportPBRValues = false & EnabledExportPBRValues;
            AllowUserChangeShedulePeriodExportPBRValues = false & EnabledExportPBRValues;
            //??? ���� ������� ������ �� �� ������������
            AdminTS_KomDisp.ConstantExportPBRValues.MaskDocument = @"���-����-����������";
            AdminTS_KomDisp.ConstantExportPBRValues.MaskExtension = @"xlsx";
            AdminTS_KomDisp.ConstantExportPBRValues.NumberRow_0 = 7;
            AdminTS_KomDisp.ConstantExportPBRValues.Format_Date = "dd.MM.yyyy HH:mm";
            AdminTS_KomDisp.ConstantExportPBRValues.NumberColumn_Date = 1;
            AdminTS_KomDisp.ConstantExportPBRValues.NumberRow_Date = 5;

            if ((Equals (FormMain.formParameters, null) == false)
                && (Equals (FormMain.formParameters.m_arParametrSetup, null) == false)
                && (FormMain.formParameters.m_arParametrSetup.Count > 0)) {
                AdminTS_KomDisp.SEC_SHEDULE_START_EXPORT_PBR = int.Parse (FormMain.formParameters.m_arParametrSetup [(int)FormParameters.PARAMETR_SETUP.KOMDISP_SHEDULE_START_EXPORT_PBR]);
                AdminTS_KomDisp.SEC_SHEDULE_PERIOD_EXPORT_PBR = int.Parse (FormMain.formParameters.m_arParametrSetup [(int)FormParameters.PARAMETR_SETUP.KOMDISP_SHEDULE_PERIOD_EXPORT_PBR]);
                //AdminTS_KomDisp.MS_WAIT_EXPORT_PBR_MAX = 6666; ���������� ��� ����������/�����������
                //AdminTS_KomDisp.MS_WAIT_EXPORT_PBR_ABORT = 666; ���������� ��� ����������/�����������
                AdminTS_KomDisp.Folder_CSV = FormMain.formParameters.m_arParametrSetup [(int)FormParameters.PARAMETR_SETUP.KOMDISP_FOLDER_CSV]; //@"\\ne2844\2.X.X\���-csv"; //@"E:\Temp\���-csv";
            } else
                ;
            //����������� �������������� �������� ���: ��������� ���������� (��������� ���������� �� ������), ������ �� ���������
            m_admin = new AdminTS_KomDisp (new bool[] { true, false });
            Admin.EventExportPBRValues += new Action<AdminTS_KomDisp.MSExcelIOExportPBRValues.EventResultArgs> (admin_onEventExportPBRValues);
        }
    }
}

