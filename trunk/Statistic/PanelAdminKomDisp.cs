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
using StatisticAlarm;

namespace Statistic
{
    public class PanelAdminKomDisp : PanelAdmin
    {
        private System.Windows.Forms.Button btnImportCSV_PBRValues;
        private System.Windows.Forms.Button btnImportCSV_AdminDefaultValues;
        //private System.Windows.Forms.CheckBox btnImportCSV_AdminDefaultValues;

        //private System.Windows.Forms.CheckBox m_cbxAlarm;
        //private System.Windows.Forms.GroupBox m_gbxDividerAlarm;
        //private Label lblKoeffAlarmCurPower;
        //private NumericUpDown m_nudnKoeffAlarmCurPower;
        //private System.Windows.Forms.Button m_btnAlarmCurPower;
        //private System.Windows.Forms.Button m_btnAlarmTGTurnOnOff;

        //private class PanelLabelAlarm : HPanelCommon {
        //    private Dictionary<KeyValuePair<int, int>, System.Windows.Forms.Label> m_dictLabel;
            
        //    public PanelLabelAlarm () : base (-1, -1) {
        //        m_dictLabel = new Dictionary<KeyValuePair<int,int>,Label> ();

        //        initializeLayoutStyle (1, 6);
        //    }

        //    protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        //    {
        //        this.ColumnCount = cols;
        //        this.RowCount = rows;

        //        for (int i = 0; i < this.RowCount; i++)
        //            this.RowStyles.Add(new RowStyle(SizeType.Percent, this.Height / this.RowCount));
        //    }

        //    public void Add(string text, int id, int id_tg)
        //    {
        //        KeyValuePair <int, int> cKey = new KeyValuePair <int, int> (id, id_tg);
        //        //m_dictLabel.Add(cKey, HLabel.createLabel (@"---", new HLabelStyles (Color.Red, Color.LightGray, 8F, ContentAlignment.MiddleLeft)));
        //        m_dictLabel.Add(cKey, new HLabel (new HLabelStyles (Color.Red, Color.LightGray, 8F, ContentAlignment.MiddleLeft)));
        //        m_dictLabel[cKey].Text = text; //??? - ������������ ��� (��� + ��)

        //        //if (m_dictLabel.Count < this.RowCount)
        //            this.Controls.Add (m_dictLabel [cKey], 0, m_dictLabel.Count - 1);
        //        //else
        //        //    ;
        //    }

        //    public void Remove(int id, int id_tg)
        //    {
        //        KeyValuePair <int, int> cKey = new KeyValuePair <int, int> (id, id_tg);
        //        int indx = this.Controls.IndexOf(m_dictLabel[cKey])
        //            , i = -1;
        //        this.Controls.Remove(m_dictLabel[cKey]);
        //        m_dictLabel.Remove(cKey);
        //        //if (indx > 0)
        //            for (i = indx; i < this.RowCount; i ++)
        //                if ((i < this.Controls.Count) && (!(this.Controls[i] == null)))
        //                    this.SetRow(this.Controls [i], i);
        //                else
        //                    break;
        //        //else
        //        //    ;                
        //    }
        //}

        //public static bool ALARM_USE = true;
        //public AdminAlarm m_adminAlarm;
        //private PanelLabelAlarm m_panelLabelAlarm;

        private enum INDEX_CONTROL_UI
        {
            UNKNOWN = -1
            , BUTTON_CSV_IMPORT_PBR, BUTTON_CSV_IMPORT_ADMINVALUESDEFAULT
            , COUNT };

        protected override void InitializeComponents()
        {
            base.InitializeComponents ();

            int posY = 271
                , offsetPosY = m_iSizeY + 2 * m_iMarginY
                , indx = -1;
            Rectangle[] arRectControlUI = new Rectangle[] {
                new Rectangle (new Point (10, posY), new Size (154, m_iSizeY)) //BUTTON_CSV_IMPORT_PBR
                , new Rectangle (new Point (10, posY + 1 * (m_iSizeY + m_iMarginY)), new Size (154, m_iSizeY)) //, BUTTON_CSV_IMPORT_ADMINVALUESDEFAULT
            };

            this.btnImportCSV_PBRValues = new System.Windows.Forms.Button();
            this.btnImportCSV_AdminDefaultValues = new System.Windows.Forms.Button();
            this.dgwAdminTable = new DataGridViewAdminKomDisp();

            this.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).BeginInit();

            this.m_panelManagement.Controls.Add(this.btnImportCSV_PBRValues);
            this.m_panelManagement.Controls.Add(this.btnImportCSV_AdminDefaultValues);
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
            // dgwAdminTable
            //
            this.dgwAdminTable.Location = new System.Drawing.Point(9, 9);
            this.dgwAdminTable.Size = new System.Drawing.Size(714, 591);
            this.dgwAdminTable.TabIndex = 1;
            //this.dgwAdminTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellClick);
            //this.dgwAdminTable.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellValidated);
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).EndInit();

            this.ResumeLayout();
        }

        public PanelAdminKomDisp(int idListener, HMark markQueries)
            : base(idListener, FormChangeMode.MANAGER.DISP, markQueries, new int[] { 0, (int)TECComponent.ID.GTP })
        {
        }

        public override bool Activate(bool activate)
        {
            return base.Activate (activate);
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
                
                for (int j = 0; j < (int)DataGridViewAdminKomDisp.DESC_INDEX.TO_ALL; j++)
                {
                    switch (j)
                    {
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN: // ����
                            valid = double.TryParse((string)dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].Value, out value);
                            m_admin.m_curRDGValues[i].pbr = value;
                            //m_admin.m_curRDGValues[i].pmin = 0.0;
                            //m_admin.m_curRDGValues[i].pmax = 0.0;
                            break;
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.UDGe: // ����
                            //valid = double.TryParse((string)dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.UDGe].Value, out value);
                            //m_admin.m_curRDGValues[i]. = value;
                            break;
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION: // ������������
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
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION: // ������������ ����������
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

            //??? �� ����� ������� �������
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
            files.InitialDirectory = FormMain.formParameters.m_arParametrSetup [(int)FormParameters.PARAMETR_SETUP.KOMDISP_FOLDER_CSV]; //@"\\ne2844\2.X.X\���-csv"; //@"E:\Temp\���-csv";
            files.DefaultExt = @"csv";
            files.Filter = @"csv ����� (*.csv)|*.csv";
            files.Title = "�������� ���� � ���...";

            if (files.ShowDialog(FormMain.formParameters) == DialogResult.OK) {
                int iRes = 0
                    , curPBRNumber = m_admin.GetPBRNumber (); //������� ����� ���
                //���� ���, ����� ��� �� ������������ �����
                object[] prop = AdminTS_KomDisp.GetPropertiesOfNameFilePPBRCSVValues(files.FileName);

                //if (!((DateTime)prop[0] == DateTime.Now.Date))
                if (!((DateTime)prop[0] == m_admin.m_curDate.Date))
                {
                    iRes = -1;
                }
                else
                {
                    //�������� � ������� ������� ���
                    if (!((int)prop[1] > curPBRNumber))
                        iRes = -2;
                    else
                        ; //iRes = 0
                }

                //�������� �� ������
                if (!(iRes == 0))
                {
                    string strMsg = string.Empty;
                    //������ �� ����
                    if (iRes == -1)
                    {
                        strMsg = string.Format(@"���� ������������ [{0}] ������ ��� �� ������������� ��������./���� [{1}]", ((DateTime)prop[0]).ToString(@"dd.MM.yyyy"), DateTime.Now.Date.ToString(@"dd.MM.yyyy"));
                        MessageBox.Show(this, strMsg, @"����������", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        //������ �� ������ ���
                        if (iRes == -2)
                        {
                            strMsg = string.Format(@"����� ������������ ������ [{0}] ��� �� ����, ��� ������� [{1}].{2}����������?", (int)prop[1], curPBRNumber, Environment.NewLine);
                            if (MessageBox.Show(this, strMsg, @"�������������", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
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

                //��� ���� �������� �� ������ (�.�. ���� ����������� �� �����������)
                if (iRes == 0)
                    ((AdminTS_KomDisp)m_admin).ImpCSVValues(mcldrDate.SelectionStart, files.FileName);
                else
                    ;
            }
            else
                ;
        }

        private string SharedFolderRun {
            get {
                return Path.GetPathRoot(Application.ExecutablePath);
            }
        }

        private void btnImportCSV_AdminValuesDefault_Click(object sender, EventArgs e)
        {
            int days = (m_admin.m_curDate.Date - HDateTime.ToMoscowTimeZone(DateTime.Now).Date).Days;
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
                files.InitialDirectory = FormMain.formParameters.m_arParametrSetup [(int)FormParameters.PARAMETR_SETUP.KOMDISP_FOLDER_CSV]; //@"\\ne2844\2.X.X\���-csv"; //@"E:\Temp\���-csv";
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
    }
}

