using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
//using System.Security.Cryptography;
using System.IO;
using System.Threading; //��� ManualResetEvent
using System.Globalization;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    public class PanelAdmin : PanelStatisticWithTableHourRows
    {
        //protected static AdminTS.TYPE_FIELDS s_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;

        protected Panel m_panelManagement, m_panelRDGValues;

        protected System.Windows.Forms.MonthCalendar mcldrDate;

        protected ManualResetEvent m_evtAdminTableRowCount;
        protected DataGridViewAdmin dgwAdminTable;

        private System.Windows.Forms.Button btnSet;
        protected System.Windows.Forms.Button btnRefresh;

        protected System.Windows.Forms.ComboBox comboBoxTecComponent;
        private System.Windows.Forms.GroupBox gbxDividerChoice;

        protected AdminTS m_admin;

        protected List <int>m_listTECComponentIndex;
        protected volatile int prevSelectedIndex;

        public List <StatisticCommon.TEC> m_list_tec { get { return m_admin.m_list_tec; } }

        protected static int m_iSizeY = 22
            , m_iMarginY = 3;

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            this.ColumnCount = cols;
            this.RowCount = rows;            

            this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 172));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            this.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        protected virtual void InitializeComponents()
        {
            this.m_panelManagement = new Panel ();
            this.m_panelRDGValues = new Panel();
            
            this.btnSet = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();

            //this.dgwAdminTable = new DataGridViewAdmin();
            this.mcldrDate = new System.Windows.Forms.MonthCalendar();
            this.comboBoxTecComponent = new System.Windows.Forms.ComboBox();
            this.gbxDividerChoice = new System.Windows.Forms.GroupBox();

            initializeLayoutStyle (2, 1);

            this.SuspendLayout();

            //((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).BeginInit();

            //
            //m_panelManagement
            //
            this.m_panelManagement.Dock = DockStyle.Fill;

            //
            //m_panelRDGValues
            //
            this.m_panelRDGValues.Dock = DockStyle.Fill;

            this.Controls.Add (m_panelManagement, 0, 0);
            this.Controls.Add(m_panelRDGValues, 1, 0);            

            this.m_panelManagement.Controls.Add(this.mcldrDate);
            this.m_panelManagement.Controls.Add(this.comboBoxTecComponent);
            this.m_panelManagement.Controls.Add(this.gbxDividerChoice);

            this.m_panelManagement.Controls.Add(this.btnSet);
            this.m_panelManagement.Controls.Add(this.btnRefresh);

            //this.Controls.Add(this.dgwAdminTable);

            this.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.Location = new System.Drawing.Point(8, 8);
            this.Name = "pnlAdmin";
            //this.Size = new System.Drawing.Size(760, 610);
            this.TabIndex = 1;
            // 
            // mcldrDate
            // 
            this.mcldrDate.Location = new System.Drawing.Point(10, 38);
            this.mcldrDate.Name = "mcldrDate";
            this.mcldrDate.TabIndex = 0;
            this.mcldrDate.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.mcldrDate_DateSelected);
            this.mcldrDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.mcldrDate.MaxSelectionCount = 1;
            this.mcldrDate.ShowToday = false;
            this.mcldrDate.ShowTodayCircle = false;

            int posY = 207;
            // 
            // btnSet
            // 
            this.btnSet.Location = new System.Drawing.Point(10, posY);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(154, m_iSizeY);
            this.btnSet.TabIndex = 2;
            this.btnSet.Text = "��������� � ����";
            this.btnSet.UseVisualStyleBackColor = true;
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(10, posY + 1 * (m_iSizeY + m_iMarginY));
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(154, m_iSizeY);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "�������� �� ����";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // gbxDividerChoice
            // 
            this.gbxDividerChoice.Location = new System.Drawing.Point(10, posY + 2 * (m_iSizeY + m_iMarginY));
            this.gbxDividerChoice.Name = "gbxDividerChoice";
            this.gbxDividerChoice.Size = new System.Drawing.Size(154, 8);
            this.gbxDividerChoice.TabIndex = 4;
            this.gbxDividerChoice.TabStop = false;
            // 
            // btnLoadLayout
            // 
            //this.btnLoadLayout.Location = new System.Drawing.Point(10, 282);
            //this.btnLoadLayout.Name = "btnLoad";
            //this.btnLoadLayout.Size = new System.Drawing.Size(154, 23);
            //this.btnLoadLayout.TabIndex = 2;
            //this.btnLoadLayout.Text = "��������� �����";
            //this.btnLoadLayout.UseVisualStyleBackColor = true;
            //this.btnLoadLayout.Click += new System.EventHandler(this.btnLoadLayout_Click);
            // 
            // comboBoxTecComponent
            // 
            this.comboBoxTecComponent.FormattingEnabled = true;
            this.comboBoxTecComponent.Location = new System.Drawing.Point(10, 10);
            this.comboBoxTecComponent.Name = "comboBoxTecComponent";
            this.comboBoxTecComponent.Size = new System.Drawing.Size(154, 21);
            this.comboBoxTecComponent.TabIndex = 3;
            this.comboBoxTecComponent.SelectionChangeCommitted += new System.EventHandler(this.comboBoxTecComponent_SelectionChangeCommitted);
            this.comboBoxTecComponent.DropDownStyle = ComboBoxStyle.DropDownList;
            //((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).EndInit();
            this.ResumeLayout();
        }

        public PanelAdmin(int idListener, FormChangeMode.MANAGER type, HMark markQueries, int [] arTECLimit)
        {
            preInitialize (type);

            try { m_admin.InitTEC(idListener, FormChangeMode.MODE_TECCOMPONENT.ANY, /*TYPE_DATABASE_CFG.CFG_200, */markQueries, false, arTECLimit); }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, "PanelAdmin::Initialize () - m_admin.InitTEC ()...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            if (!(m_admin.m_list_tec.Count > 0))
            {
                Logging.Logg().Error(@"PanelAdmin::PanelAdmin () - ������ ��� ����...", Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
                ;

            initialize ();
        }

        public PanelAdmin(List<StatisticCommon.TEC> tec, FormChangeMode.MANAGER type)
        {
            preInitialize (type);
            
            //��� ��������� ����� ���������� (����������� ���-�� ���������� � ��)
            HMark markQueries = new HMark(new int [] {(int)CONN_SETT_TYPE.ADMIN, (int)CONN_SETT_TYPE.PBR});
            //markQueries.Marked ((int)CONN_SETT_TYPE.ADMIN);
            //markQueries.Marked((int)CONN_SETT_TYPE.PBR);

            try { m_admin.InitTEC(tec, markQueries); }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, "PanelAdmin::Initialize () - m_admin.InitTEC ()...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            if (!(m_admin.m_list_tec.Count > 0))
            {
                Logging.Logg().Error(@"PanelAdmin::PanelAdmin () - ������ ��� ����...", Logging.INDEX_MESSAGE.NOT_SET);
            }
            else
                ;

            initialize ();
        }

        private void preInitialize(FormChangeMode.MANAGER type)
        {
            switch (type)
            {
                case FormChangeMode.MANAGER.DISP:
                    //����������� �������������� �������� ���: ��������� ���������� (��������� ���������� �� ������), ������ �� ���������
                    m_admin = new AdminTS_KomDisp(new bool[] { true, false });
                    break;
                case FormChangeMode.MANAGER.NSS:
                    //����������� �������������� �������� ���: �� ��������� ���������� (��������� ���������� �� ������), ������ ���������
                    m_admin = new AdminTS_NSS(new bool[] { false, true });
                    break;
                case FormChangeMode.MANAGER.LK:
                    //����������� �������������� �������� ���: �� ��������� ���������� (��������� ���������� �� ������), ������ ���������
                    m_admin = new PanelAdminLK.AdminTS_LK(new bool[] { false, true });
                    break;
                case FormChangeMode.MANAGER.TEPLOSET:
                    //����������� �������������� �������� ���: ��������� ���������� (��������� ���������� �� ������), ������ �� ���������
                    m_admin = new PanelAdminVyvod.AdminTS_Vyvod(new bool[] { true, false });
                    break;
                default:
                    break;
            }
        }

        private void initialize () {
            m_evtAdminTableRowCount = new ManualResetEvent (false);

            m_admin.SetDelegateData(this.setDataGridViewAdmin, null);
            m_admin.SetDelegateDatetime(this.CalendarSetDate);

            //m_admin.m_typeFields = s_typeFields;

            InitializeComponents();
        }

        public override void SetDelegateWait(DelegateFunc fstart, DelegateFunc fstop, DelegateFunc fev)
        {
            base.SetDelegateWait(fstart, fstop, fev);

            m_admin.SetDelegateWait(fstart, fstop, fev);
        }

        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            m_admin.SetDelegateReport(ferr, fwar, fact, fclr);
        }

        public override void Start () {
            base.Start ();

            initTableHourRows();
            
            m_admin.Start ();
        }

        public override void Stop()
        {
            CalendarSetDate(DateTime.Now.Date);

            if (m_admin.IsStarted == true) m_admin.Stop(); else ;

            base.Stop ();
        }

        protected virtual void getDataGridViewAdmin() {}

        public virtual void setDataGridViewAdmin(DateTime date) {}

        /// <summary>
        /// ��������� �������� ����/������� � �������� ���������� '���������' �
        /// ���������� �������, ��������� � ���������� �������� ����/�������
        /// </summary>
        /// <param name="dt">��������������� ����</param>
        private void setDate(DateTime dt)
        {
            mcldrDate.SetDate(dt);

            initTableHourRows();
        }

        /// <summary>
        /// ������� ��� ��������� �������� � �������� ���������� '���������'
        /// ��� ������ �� '�������' ������
        /// </summary>
        /// <param name="date"></param>
        public void CalendarSetDate(DateTime date)
        {
            if (IsHandleCreated/*InvokeRequired*/ == true)
                BeginInvoke(new DelegateDateFunc(setDate), date);
            else
                ;
        }

        /// <summary>
        /// ������������� ����/������� ��� ����������� ������� ������� � ������� �
        /// ���-�� ����� ������� � ������������ � ���� ��������
        /// </summary>
        protected override void initTableHourRows()
        {
            //���������� ������� "[��]��������" ������� ������� 'm_curRDGValues'
            m_admin.m_curDate = mcldrDate.SelectionStart.Date;

            if (m_admin.m_curDate.Date.Equals(HAdmin.SeasonDateTime.Date) == false)
            {
                dgwAdminTable.InitRows(24, false);
            }
            else {
                dgwAdminTable.InitRows(25, true);                
            }
        }

        /// <summary>
        /// ���������� ���-�� ����� ������� � ������������ � ���-�� ��������� � ������� � �������
        /// ������� (�� ������� ������������� 'PanelAdminKomDisp::setDataGridViewAdmin () - ...') ������ �� �������, �� ��������� ???
        /// </summary>
        protected void normalizedTableHourRows () {
            if (!(this.dgwAdminTable.Rows.Count == m_admin.m_curRDGValues.Length))
                if (this.dgwAdminTable.Rows.Count < m_admin.m_curRDGValues.Length)
                    this.dgwAdminTable.InitRows(m_admin.m_curRDGValues.Length, true);
                else
                    this.dgwAdminTable.InitRows(m_admin.m_curRDGValues.Length, false);

            m_evtAdminTableRowCount.Set();
        }

        private void mcldrDate_DateSelected(object sender, DateRangeEventArgs e)
        {
            DialogResult result;
            Errors resultSaving;

            bool bRequery = false;

            getDataGridViewAdmin ();

            if (m_admin.WasChanged())
            {
                result = MessageBox.Show(this, "������ ���� �������� �� �� �����������.\n���� �� ������ ��������� ���������, ������� \"��\".\n���� �� �� ������ ��������� ���������, ������� \"���\".\n��� ������ �������� ������� \"������\".", "��������", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
            }
            else
                result = DialogResult.No;

            switch (result)
            {
                case DialogResult.Yes:
                    resultSaving = m_admin.SaveChanges();
                    if (resultSaving == Errors.NoError)
                    {
                        bRequery = true;
                    }
                    else
                    {
                        if (resultSaving == Errors.InvalidValue)
                            MessageBox.Show(this, "��������� ������������� �����������!", "��������", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        else
                            MessageBox.Show(this, "�� ������� ��������� ���������, �������� ����������� ����� � ����� ������.", "������ ����������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        mcldrDate.SetDate(m_admin.m_prevDate);
                    }
                    break;
                case DialogResult.No:
                    bRequery = true;
                    break;
                case DialogResult.Cancel:
                    mcldrDate.SetDate(m_admin.m_prevDate);
                    break;
            }

            if (bRequery == true) {
                ClearTables();

                initTableHourRows();

                m_admin.GetRDGValues(/*(int)m_admin.m_typeFields,*/ m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
            }
            else
                ;
        }

        public virtual void InitializeComboBoxTecComponent (FormChangeMode.MODE_TECCOMPONENT mode) 
        {
            m_listTECComponentIndex = m_admin.GetListIndexTECComponent(mode, !(this is PanelAdminLK));

            //m_admin.m_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;

            comboBoxTecComponent.Items.Clear ();
        }

        protected virtual void comboBoxTecComponent_SelectionChangeCommitted(object sender, EventArgs e)
        {
            DialogResult result;
            Errors resultSaving;

            bool bRequery = false;

            getDataGridViewAdmin();

            if (m_admin.WasChanged() == true)
            {
                result = MessageBox.Show(this, "������ ���� �������� �� �� �����������.\n���� �� ������ ��������� ���������, ������� \"��\".\n���� �� �� ������ ��������� ���������, ������� \"���\".\n��� ������ �������� ������� \"������\".", "��������", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
            }
            else
                result = DialogResult.No;

            switch (result)
            {
                case DialogResult.Yes:
                    resultSaving = m_admin.SaveChanges();
                    if (resultSaving == Errors.NoError)
                    {
                        bRequery = true;
                    }
                    else
                    {
                        if (resultSaving == Errors.InvalidValue)
                            MessageBox.Show(this, "��������� ������������� �����������!", "��������", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        else
                            MessageBox.Show(this, "�� ������� ��������� ���������, �������� ����������� ����� � ����� ������.", "������ ����������", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        comboBoxTecComponent.SelectedIndex = prevSelectedIndex;
                    }
                    break;
                case DialogResult.No:
                    bRequery = true;
                    break;
                case DialogResult.Cancel:
                    comboBoxTecComponent.SelectedIndex = prevSelectedIndex;
                    break;
            }

            if (bRequery == true) {
                ClearTables();

                m_admin.GetRDGValues(/*(int)m_admin.m_typeFields,*/ m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
            }
            else
                ;
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            getDataGridViewAdmin();

            Errors resultSaving = m_admin.SaveChanges();
            if (resultSaving == Errors.NoError)
            {
                ClearTables();

                m_admin.GetRDGValues(/*(int)m_admin.m_typeFields,*/ m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
            }
            else
            {
                if (resultSaving == Errors.InvalidValue)
                    MessageBox.Show(this, "��������� ������������� �����������!", "��������", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                else
                    MessageBox.Show(this, "�� ������� ��������� ���������, �������� ����������� ����� � ����� ������.", "������ ����������", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearTables();

            m_admin.GetRDGValues(/*(int)m_admin.m_typeFields,*/ m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
        }

        //private string SetNumberSeparator(string current_str)
        //{
        //    if (current_str.IndexOf(".") > 0)
        //    {
        //        if (System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator != ".")
        //            return current_str.Replace(".", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);
        //        else
        //            ;
        //    }
        //    else
        //    {
        //        if (current_str.IndexOf(",") > 0)
        //        {
        //            if (System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator != ",")
        //                return current_str.Replace(",", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);
        //            else
        //                ;
        //        }
        //        else
        //            ;
        //    }
        //    return current_str;
        //}

        public override bool MayToClose()
        {
            DialogResult result;
            Errors resultSaving;

            getDataGridViewAdmin();

            if (m_admin.WasChanged())
            {
                result = MessageBox.Show(this, "������ ���� �������� �� �� �����������.\n���� �� ������ ��������� ���������, ������� \"��\".\n���� �� �� ������ ��������� ���������, ������� \"���\".\n��� ������ �������� ������� \"������\".", "��������", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
            }
            else
                result = DialogResult.No;

            switch (result)
            {
                case DialogResult.Yes:
                    resultSaving = m_admin.SaveChanges();
                    if (resultSaving == Errors.NoError)
                        return true;
                    else
                    {
                        if (resultSaving == Errors.InvalidValue)
                            if (MessageBox.Show(this, "��������� ������������� �����������!\n���������� �����?", "��������", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                                return true;
                            else
                                return false;
                        else
                            if (MessageBox.Show(this, "�� ������� ��������� ���������, �������� ����������� ����� � ����� ������.\n����� ��� ����������?", "������ ����������", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                                return true;
                            else
                                return false;
                    }
                case DialogResult.No:
                    return true;
                case DialogResult.Cancel:
                    return false;
            }
            return false;
        }

        public virtual void ClearTables() {}

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate (active);
            
            if (bRes == true)
            {
                m_admin.Activate(active);
            }
            else
                ;

            return bRes;
        }
    }
}
