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

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    public partial class PanelAdminNSS : PanelAdmin
    {
        private System.Windows.Forms.Button btnImportExcel;
        private System.Windows.Forms.Button btnExportExcel;
        
        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            this.btnImportExcel = new System.Windows.Forms.Button();
            this.btnExportExcel = new System.Windows.Forms.Button();

            this.m_panelManagement.Controls.Add(this.btnImportExcel);
            this.m_panelManagement.Controls.Add(this.btnExportExcel);

            int posY = 276;
            // 
            // btnImportExcel
            // 
            this.btnImportExcel.Location = new System.Drawing.Point(10, posY);
            this.btnImportExcel.Name = "btnImportExcel";
            this.btnImportExcel.Size = new System.Drawing.Size(154, m_iSizeY);
            this.btnImportExcel.TabIndex = 667;
            this.btnImportExcel.Text = "������ �� Excel";
            this.btnImportExcel.UseVisualStyleBackColor = true;
            this.btnImportExcel.Click += new System.EventHandler(this.btnImportExcel_Click);
            // 
            // btnExportExcel
            // 
            //this.btnExportExcel.Location = new System.Drawing.Point(10, posY + 1 * (m_iSizeY + 2 * m_iMarginY));
            this.btnExportExcel.Location = new System.Drawing.Point(10, posY + 1 * (m_iSizeY + m_iMarginY));
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(154, m_iSizeY);
            this.btnExportExcel.TabIndex = 668;
            this.btnExportExcel.Text = "������� � Excel";
            this.btnExportExcel.UseVisualStyleBackColor = true;
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
            this.btnExportExcel.Enabled = true;

            this.dgwAdminTable = new DataGridViewAdminNSS(new System.Drawing.Color [] { System.Drawing.SystemColors.Window, System.Drawing.Color.Yellow, FormMain.formGraphicsSettings.COLOR (FormGraphicsSettings.INDEX_COLOR.DIVIATION) });
            this.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).BeginInit();
            this.m_panelRDGValues.Controls.Add(this.dgwAdminTable);
            // 
            // dgwAdminTable
            //
            this.dgwAdminTable.Location = new System.Drawing.Point(9, 9);
            this.dgwAdminTable.Size = new System.Drawing.Size(574, 591);
            this.dgwAdminTable.TabIndex = 1;
            //this.dgwAdminTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellClick);
            //this.dgwAdminTable.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellValidated);
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).EndInit();
            this.ResumeLayout();
        }

        public PanelAdminNSS(int idListener, HMark markQueries)
            : base(idListener, markQueries, new int[] { 0, (int)TECComponent.ID.LK })
        {
            m_admin.SetDelegateSaveComplete(null);
        }

        private int getIndexGTPOwner(int indx_tg)
        {
            int iRes = -1
                , id_gtp_owner = ((DataGridViewAdminNSS)dgwAdminTable).GetIdGTPOwner(indx_tg);
            
            foreach (int indx in ((AdminTS_NSS)m_admin).m_listTECComponentIndexDetail)
            {
                if (m_admin.allTECComponents[indx].m_id == id_gtp_owner) {
                    return ((AdminTS_NSS)m_admin).m_listTECComponentIndexDetail.IndexOf(indx);
                }
                else
                    ;
            }

            return iRes;
        }
        /// <summary>
        /// ��������� � ��� �������� � �����/������ (����� ������ ����� 'PanelAdminVyvod')
        /// </summary>
        protected override void getDataGridViewAdmin()
        {
            //double value = 0.0;
            //bool valid = false;

            foreach (int indx in ((AdminTS_TG)m_admin).m_listTECComponentIndexDetail)
                if (m_admin.modeTECComponent(indx) == FormChangeMode.MODE_TECCOMPONENT.TG)
                {
                    int indx_tg = ((AdminTS_NSS)m_admin).m_listTECComponentIndexDetail.IndexOf(indx),
                        indx_gtp = getIndexGTPOwner(indx_tg);

                    if ((!(indx_tg < 0)) && (!(indx_gtp < 0)))
                        for (int i = 0; i < 24; i++)
                        {
                            ((AdminTS_NSS)m_admin).m_listCurRDGValues[indx_tg][i].pbr = Convert.ToDouble(dgwAdminTable.Rows[i].Cells[indx_tg + 1].Value); // '+ 1' �� ���� DateTime
                            //((AdminTS_NSS)m_admin).m_listCurRDGValues[indx_tg][i].pmin =
                            //((AdminTS_NSS)m_admin).m_listCurRDGValues[indx_tg][i].pmax = 

                            ((AdminTS_NSS)m_admin).m_listCurRDGValues[indx_tg][i].recomendation = 0.0;
                            ((AdminTS_NSS)m_admin).m_listCurRDGValues[indx_tg][i].deviationPercent = ((AdminTS_NSS)m_admin).m_listCurRDGValues[indx_gtp][i].deviationPercent;
                            ((AdminTS_NSS)m_admin).m_listCurRDGValues[indx_tg][i].deviation = ((AdminTS_NSS)m_admin).m_listCurRDGValues[indx_gtp][i].deviation;
                        }
                    else
                        ;
                }
                else
                    ;
        }
        /// <summary>
        /// �������� ��������� ������� ��� ����������(�������������) ����������-��
        /// </summary>
        /// <param name="date"></param>
        private void addTextBoxColumn (DateTime date) {
            DataGridViewCellEventArgs ev;
            int indx = ((AdminTS_NSS)m_admin).m_listTECComponentIndexDetail[this.dgwAdminTable.Columns.Count - 2];
            ((DataGridViewAdminNSS)this.dgwAdminTable).addTextBoxColumn(m_admin.GetNameTECComponent(indx),
                                                                        m_admin.GetIdTECComponent (indx),
                                                                        m_admin.GetIdGTPOwnerTECComponent(indx),
                                                                        date);

            for (int i = 0; i < 24; i++)
            {
                if (this.dgwAdminTable.Columns.Count == 3) //������ ��� ���������� 1-�� �������
                    this.dgwAdminTable.Rows[i].Cells[0].Value = date.AddHours(i + 1).ToString(@"dd.MM.yyyy-HH", CultureInfo.InvariantCulture);
                else
                    ;

                this.dgwAdminTable.Rows[i].Cells[this.dgwAdminTable.Columns.Count - 2].Value = ((AdminTS_NSS)m_admin).m_listCurRDGValues[this.dgwAdminTable.Columns.Count - 3][i].pbr.ToString("F2");
                ev = new DataGridViewCellEventArgs(this.dgwAdminTable.Columns.Count - 2, i);
                ((DataGridViewAdminNSS)this.dgwAdminTable).DataGridViewAdminNSS_CellValueChanged(null, ev);
            }

            m_admin.CopyCurToPrevRDGValues();

            //this.dgwAdminTable.Invalidate();
        }

        private void updateTextBoxColumn()
        {
            for (int i = 0; i < 24; i++)
            {
            }

            //m_admin.CopyCurToPrevRDGValues();
        }

        public override void setDataGridViewAdmin(DateTime date)
        {
            //if (this.dgwAdminTable.Columns.Count < ((AdminTS_NSS)m_admin).m_listTECComponentIndexDetail.Count)
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new DelegateDateFunc(addTextBoxColumn), date);
                else
                    Logging.Logg().Error(@"PanelTecCurPower::setDataGridViewAdmin () - ... BeginInvoke (addTextBoxColumn) - ...", Logging.INDEX_MESSAGE.D_001);
            //else
            //    this.BeginInvoke(new DelegateFunc(updateTextBoxColumn));
        }

        public override void ClearTables()
        {
            ((DataGridViewAdminNSS)this.dgwAdminTable).ClearTables();
        }

        public override void InitializeComboBoxTecComponent(FormChangeMode.MODE_TECCOMPONENT mode)
        {
            base.InitializeComboBoxTecComponent (mode);

            if (m_listTECComponentIndex.Count > 0) {
                comboBoxTecComponent.Items.AddRange (((AdminTS_TG)m_admin).GetListNameTEC ());

                if (comboBoxTecComponent.Items.Count > 0)
                {
                    m_admin.indxTECComponents = m_listTECComponentIndex[0];
                    comboBoxTecComponent.SelectedIndex = 0;
                }
                else
                    ;
            }
            else
                ;
        }

        public override bool Activate(bool active)
        {
            bool bRes = false;
            
            visibleControlRDGExcel();

            if (active == true)
                ; //m_admin.Resume ();
            else {
                //m_admin.Suspend();

                //((DataGridViewAdminNSS)this.dgwAdminTable).ClearTables ();
            }

            bRes = base.Activate(active);

            //ClearTables ();

            //������� �1
            //EventArgs.m_admin.ResetRDGExcelValues();
            //base.comboBoxTecComponent_SelectionChangeCommitted(this, EventArgs.Empty);

            //������� �2
            //comboBoxTecComponent.SelectedIndex = prevSelectedIndex;

            return bRes;
        }

        public override void Stop()
        {
            ClearTables ();

            base.Stop ();
        }

        protected override void comboBoxTecComponent_SelectionChangeCommitted(object sender, EventArgs e)
        {
            base.comboBoxTecComponent_SelectionChangeCommitted (sender, e);

            visibleControlRDGExcel();
        }

        private void visibleControlRDGExcel()
        {
            bool bImpExpButtonVisible = false;
            if ((!(m_listTECComponentIndex == null))
                && (m_listTECComponentIndex.Count > 0)
                && (!(comboBoxTecComponent.SelectedIndex < 0))
                && (m_admin.IsRDGExcel(m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex]) == true))
                bImpExpButtonVisible = true;
            else
                ;

            btnImportExcel.Visible =
            btnExportExcel.Visible =
                bImpExpButtonVisible;
        }

        private void btnImportExcel_Click(object sender, EventArgs e)
        {
            ClearTables();

            m_admin.ImpRDGExcelValues(m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            //FolderBrowserDialog exportFolder = new FolderBrowserDialog();
            //exportFolder.ShowDialog(this);

            //if (exportFolder.SelectedPath.Length > 0) {
                getDataGridViewAdmin();

                Errors resultSaving = m_admin.ExpRDGExcelValues(m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
                if (resultSaving == Errors.NoError)
                {
                    btnRefresh.PerformClick ();
                }
                else
                {
                    if (resultSaving == Errors.InvalidValue)
                        MessageBox.Show(this, "��������� ������������� �����������!", "��������", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        MessageBox.Show(this, "�� ������� ��������� ���������, �������� ����������� ����� � ����� ������.", "������ ����������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            //} else ;
        }

        protected override void createAdmin ()
        {
            //����������� �������������� �������� ���: �� ��������� ���������� (��������� ���������� �� ������), ������ ���������
            m_admin = new AdminTS_NSS (new bool[] { false, true });                    
        }
    }
}
