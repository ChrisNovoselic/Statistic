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


using StatisticCommon;
using ASUTP.Core;
using Microsoft.Office.Interop.Excel;
using ASUTP;
using ASUTP.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Statistic
{
    public abstract class PanelAdmin : PanelStatisticWithTableHourRows
    {
        /// <summary>
        /// ����� ������ ������ (�����. + ���) ��
        ///  , ��� ����������� �������� ������ �� ���
        ///  , ���� ��� ��� �������� �������� � ���� ��� ���./���� � ����� ��������� �������� ��� � ������������ ���������� �� ������ ����������
        /// </summary>
        public virtual AdminTS.MODE_GET_RDG_VALUES ModeGetRDGValues
        {
            get
            {
                return m_admin.ModeGetRDGValues;
            }

            set
            {
                if (!(m_admin.ModeGetRDGValues == value)) {
                    comboBoxTecComponent.Enabled =
                    mcldrDate.Enabled =
                    btnSet.Enabled =
                    btnRefresh.Enabled =
                        (value & AdminTS.MODE_GET_RDG_VALUES.DISPLAY) == AdminTS.MODE_GET_RDG_VALUES.DISPLAY;

                    m_admin.ModeGetRDGValues = value;
                } else
                    ;
            }
        }
        /// <summary>
        /// ����� ��� �����������(����������) ����� ��������� ���������� �� ������ � ������������ ���������� ����������
        /// </summary>
        protected class GroupBoxDividerChoice : System.Windows.Forms.GroupBox
        {
            private static int counter = 0;

            public GroupBoxDividerChoice()
                : base ()
            {
                counter++;

                InitializeComponent();
            }

            private void InitializeComponent()
            {
                Name = string.Format("gbxDividerChoice_{0}", (counter + 1));
                Size = new System.Drawing.Size(154, 8);
                TabIndex = (counter + 1) * 4;
                TabStop = false;
            }

            public void Initialize(int posY)
            {
                Location = new System.Drawing.Point(10, posY + 2 * (m_iSizeY + m_iMarginY));
            }
        }

        //protected static AdminTS.TYPE_FIELDS s_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;
        /// <summary>
        /// ������ ��� ���������� ��������(�����������), ��������� ��������� ����������
        /// </summary>
        protected Panel m_panelManagement
            , m_panelRDGValues;
        /// <summary>
        /// ��������� ��� ������ ����, �� ������. ��������� ���������(����������/��������������) �������� �� ��
        /// </summary>
        protected System.Windows.Forms.MonthCalendar mcldrDate;
        ///// <summary>
        ///// ������ ������������� ��� ��������� ���-�� ������ � ������������� ��� ����������� ��������
        //!!! ������� �� IAsyncResult; KhryapinAN, 2018-01-30
        ///// </summary>
        //protected ManualResetEvent m_evtAdminTableRowCount;
        /// <summary>
        /// ������� ����������(�������������) ��� ����������� ��������
        /// </summary>
        protected DataGridViewAdmin dgwAdminTable;
        /// <summary>
        /// ������� ����������(�����������) ��� ������������� �������� ���������� �������� � �� �� �������������
        /// </summary>
        private System.Windows.Forms.Button btnSet;
        /// <summary>
        /// ������� ����������(�����������) ��� ������������� �������� ���������� �������� � �������������
        /// </summary>
        protected System.Windows.Forms.Button btnRefresh;
        /// <summary>
        /// ������� ����������(�����������) ��� ������ ���������� ���(���), ��� �������� ���������
        /// </summary>
        protected System.Windows.Forms.ComboBox comboBoxTecComponent;
        /// <summary>
        /// ������ ����������� ����� ����������� ��������� ����������
        /// </summary>
        private GroupBoxDividerChoice gbxDividerChoice;
        /// <summary>
        /// ������ ��� ��������� � ��
        /// </summary>
        protected AdminTS m_admin;
        /// <summary>
        /// ������ �������� � ������ ��������������� ����������� ���, 
        /// </summary>
        protected List <int>m_listTECComponentIndex;
        /// <summary>
        /// ������
        /// </summary>
        protected volatile int prevSelectedIndex;
        /// <summary>
        /// ������ ���, ���������� ������������� ��� ������� ����������
        ///  , ���������� � ������ ��� �������� ������� ������
        /// </summary>
        public List <StatisticCommon.TEC> m_list_tec { get { return m_admin.m_list_tec; } }
        /// <summary>
        /// �������� ��� ���������������� ��������� ����������
        /// </summary>
        protected static int m_iSizeY = 22
            , m_iMarginY = 3;
        /// <summary>
        /// ������������� �������������, ������ ������ ��� ���������� �������� ��������� ����������
        ///  (������ ���� ������� ����)
        /// </summary>
        /// <param name="col">���������� �������� � ������</param>
        /// <param name="row">���������� ����� � ������</param>
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
            this.gbxDividerChoice = new GroupBoxDividerChoice();

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
            this.gbxDividerChoice.Initialize(posY);
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

        public PanelAdmin(HMark markQueries, int [] arTECLimit)
            : base(MODE_UPDATE_VALUES.ACTION, FormMain.formGraphicsSettings.FontColor, FormMain.formGraphicsSettings.BackgroundColor)
        {
            createAdmin ();

            try { m_admin.InitTEC(FormChangeMode.MODE_TECCOMPONENT.ANY, /*TYPE_DATABASE_CFG.CFG_200, */markQueries, false, arTECLimit); }
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

        public PanelAdmin(List<StatisticCommon.TEC> tec)
            : base (MODE_UPDATE_VALUES.ACTION, FormMain.formGraphicsSettings.FontColor, FormMain.formGraphicsSettings.BackgroundColor)
        {
            createAdmin ();
            
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

        protected abstract void createAdmin ();

        private void initialize () {
            //m_evtAdminTableRowCount = new ManualResetEvent (false);

            m_admin.SetDelegateData(this.SetDataGridViewAdmin, null);
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

        public override bool Activate (bool active)
        {
            bool bRes = base.Activate (active);

            if (bRes == true) {
                m_admin.Activate (active);
            } else
                ;

            return bRes;
        }

        protected virtual void getDataGridViewAdmin() {}

        public abstract void SetDataGridViewAdmin (DateTime date, bool bNewValues);

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
        /// <param name="date">����/����� ������ ��������</param>
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
        /// <param name="bSyncReq">������� ������������� �������������</param>
        /// </summary>
        protected void normalizedTableHourRows (bool bSyncReq)
        {
            if (!(this.dgwAdminTable.Rows.Count == m_admin.m_curRDGValues.Length))
                if (this.dgwAdminTable.Rows.Count < m_admin.m_curRDGValues.Length)
                    this.dgwAdminTable.InitRows(m_admin.m_curRDGValues.Length, true);
                else
                    this.dgwAdminTable.InitRows(m_admin.m_curRDGValues.Length, false);

            //if (bSyncReq == true)
            //    m_evtAdminTableRowCount.Set();
            //else
            //    ;
        }

        private void mcldrDate_DateSelected(object sender, DateRangeEventArgs e)
        {
            DialogResult result;
            ASUTP.Helper.Errors resultSaving;

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
                    if (resultSaving == ASUTP.Helper.Errors.NoError)
                    {
                        bRequery = true;
                    }
                    else
                    {
                        if (resultSaving == ASUTP.Helper.Errors.InvalidValue)
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

        [TestMethod]
        public void PerformComboBoxTECComponentSelectedIndex(int newIndex)
        {
            comboBoxTecComponent.SelectedIndex = newIndex;

            comboBoxTecComponent_SelectionChangeCommitted(this, EventArgs.Empty);
        }

        protected virtual void comboBoxTecComponent_SelectionChangeCommitted(object sender, EventArgs e)
        {
            DialogResult result;
            ASUTP.Helper.Errors resultSaving;

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
                    if (resultSaving == ASUTP.Helper.Errors.NoError)
                    {
                        bRequery = true;
                    }
                    else
                    {
                        if (resultSaving == ASUTP.Helper.Errors.InvalidValue)
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

        #region ��������� ���� ���������� ���������������� ��������
        /// <summary>
        /// ��� �������� ������ ��� ������������� � ��������� ������
        /// </summary>
        /// <param name="nextIndex">��������� ������</param>
        /// <param name="t">������ ��� - �������� ���������� ����������-������� � ������ (���, ����������, ��� ������, ����� ��������� = null)</param>
        /// <param name="comp">���������-������ ��������� � ������</param>
        /// <param name="date">����, �� ������� ��������� ��������/��������� ��������</param>
        /// <param name="listIdRec">������ ��������������� ������� � ������� �� ��� ����������</param>
        /// <param name="nextIndex">��������� ������ �� ������ ��������-�����������</param>
        public delegate void DelegateUnitTestNextIndexSetValuesRequest(int nextIndex, TEC t, TECComponent comp, DateTime date, CONN_SETT_TYPE type, IEnumerable<int> listIdRec, string[]queries);

        private DelegateUnitTestNextIndexSetValuesRequest _eventUnitTestNextIndexSetValuesRequest;

        public event DelegateUnitTestNextIndexSetValuesRequest EventUnitTestNextIndexSetValuesRequest
        {
            add
            {
                if (Equals(_eventUnitTestNextIndexSetValuesRequest, null) == true)
                    _eventUnitTestNextIndexSetValuesRequest += value;
                else
                    ;
            }

            remove
            {
                if (Equals(_eventUnitTestNextIndexSetValuesRequest, null) == false) {
                    _eventUnitTestNextIndexSetValuesRequest -= value;
                    _eventUnitTestNextIndexSetValuesRequest = null;
                } else
                    ;
            }
        }

        [TestMethod]
        public void PerformButtonSetClick (DelegateUnitTestNextIndexSetValuesRequest fUnitTestNextIndexSetValuesRequest)
        {
            m_admin.EventUnitTestSetValuesRequest += new AdminTS.DelegateUnitTestSetValuesRequest(admin_onEventUnitTestSetValuesRequest);
            EventUnitTestNextIndexSetValuesRequest += new DelegateUnitTestNextIndexSetValuesRequest (fUnitTestNextIndexSetValuesRequest);

            btnSet.PerformClick();
        }

        private void admin_onEventUnitTestSetValuesRequest(TEC t, TECComponent comp, DateTime date, CONN_SETT_TYPE type, string[]queries, IEnumerable<int> listIdRec)
        {
            _eventUnitTestNextIndexSetValuesRequest?.Invoke(comboBoxTecComponent.SelectedIndex + 1 < comboBoxTecComponent.Items.Count ? comboBoxTecComponent.SelectedIndex + 1 : -1, t, comp, date, type, listIdRec, queries);
        }
        #endregion

        private void btnSet_Click(object sender, EventArgs e)
        {
            //??? ������ �������� ������ � ���� ������
            //ModeGetRDGValues = AdminTS.MODE_GET_RDG_VALUES.DISPLAY;

            getDataGridViewAdmin();

            ASUTP.Helper.Errors resultSaving = m_admin.SaveChanges();
            if (resultSaving == ASUTP.Helper.Errors.NoError)
            {
                ClearTables();

                m_admin.GetRDGValues(/*(int)m_admin.m_typeFields,*/ m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
            }
            else
            {
                if (resultSaving == ASUTP.Helper.Errors.InvalidValue)
                    MessageBox.Show(this, "��������� ������������� �����������!", "��������", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                else
                    MessageBox.Show(this, "�� ������� ��������� ���������, �������� ����������� ����� � ����� ������.", "������ ����������", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            //??? ������ �������� ������ � ���� ������
            //ModeGetRDGValues = AdminTS.MODE_GET_RDG_VALUES.DISPLAY;

            ClearTables ();

            m_admin.GetRDGValues(/*(int)m_admin.m_typeFields,*/ m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
        }

        public override int MayToClose()
        {
            DialogResult result;
            ASUTP.Helper.Errors resultSaving;

            if ((Equals(m_admin, null) == false)
                && (m_admin.IsStarted == false))
            // ��� ���������� �����
                return 1;

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
                    if (resultSaving == ASUTP.Helper.Errors.NoError)
                        return 0;
                    else
                    {
                        if (resultSaving == ASUTP.Helper.Errors.InvalidValue)
                            if (MessageBox.Show(this, "��������� ������������� �����������!\n���������� �����?", "��������", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                                return 0;
                            else
                                return -1;
                        else
                            if (MessageBox.Show(this, "�� ������� ��������� ���������, �������� ����������� ����� � ����� ������.\n����� ��� ����������?", "������ ����������", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                                return 0;
                            else
                                return -1;
                    }
                case DialogResult.No:
                    return 0;
                case DialogResult.Cancel:
                    return -1;
            }

            return -1;
        }

        public virtual void ClearTables() { }

        public override void UpdateGraphicsCurrent (int type)
        {
            SetDataGridViewAdmin (mcldrDate.SelectionStart.Date, false);
        }
    }
}
