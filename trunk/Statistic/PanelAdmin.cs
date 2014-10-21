using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
//using System.Security.Cryptography;
using System.IO;
//using System.Threading;
using System.Globalization;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    public class PanelAdmin : PanelStatistic
    {
        protected static AdminTS.TYPE_FIELDS s_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;

        protected Panel m_panelManagement, m_panelRDGValues;

        protected System.Windows.Forms.MonthCalendar mcldrDate;

        protected DataGridViewAdmin dgwAdminTable;

        private System.Windows.Forms.Button btnSet;
        protected System.Windows.Forms.Button btnRefresh;

        protected System.Windows.Forms.ComboBox comboBoxTecComponent;
        private System.Windows.Forms.GroupBox gbxDividerChoice;

        protected AdminTS m_admin;

        protected List <int>m_listTECComponentIndex;
        protected volatile int prevSelectedIndex;

        public bool isActive;

        public List <StatisticCommon.TEC> m_list_tec { get { return m_admin.m_list_tec; } }

        //public string last_error { get { return m_admin.last_error; } }
        //public DateTime last_time_error { get { return m_admin.last_time_error; } }
        //public bool errored_state { get { return m_admin.errored_state; } }

        //public string last_action { get { return m_admin.last_action; } }
        //public DateTime last_time_action { get { return m_admin.last_time_action; } }
        //public bool actioned_state { get { return m_admin.actioned_state; } }

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

            this.RowCount = 1;
            this.ColumnCount = 2;

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

            this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 172));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            this.RowStyles.Add(new RowStyle(SizeType.AutoSize));

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
            // 
            // btnSet
            // 
            this.btnSet.Location = new System.Drawing.Point(10, 207);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(154, 23);
            this.btnSet.TabIndex = 2;
            this.btnSet.Text = "Сохранить в базу";
            this.btnSet.UseVisualStyleBackColor = true;
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(10, 237);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(154, 23);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "Обновить из базы";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // gbxDividerChoice
            // 
            this.gbxDividerChoice.Location = new System.Drawing.Point(10, 264);
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
            //this.btnLoadLayout.Text = "Загрузить макет";
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

        public PanelAdmin(int idListener, FormChangeMode.MANAGER type, HMark markQueries)
        {
            //bool bUseData = true;
            switch (type) {
                case FormChangeMode.MANAGER.DISP:
                    m_admin = new AdminTS_KomDisp(new bool[] { true, false });
                    break;
                case FormChangeMode.MANAGER.NSS:
                    m_admin = new AdminTS_NSS(new bool[] { false, true });
                    break;
                default:
                    break;
            }

            try { m_admin.InitTEC(idListener, FormChangeMode.MODE_TECCOMPONENT.UNKNOWN, TYPE_DATABASE_CFG.CFG_200, markQueries, false); }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, "PanelAdmin::Initialize () - m_admin.InitTEC ()...");
            }

            if (!(m_admin.m_list_tec.Count > 0))
            {
                Logging.Logg().Error(@"PanelAdmin::PanelAdmin () - список ТЭЦ пуст...");
            }
            else
                ;

            m_admin.SetDelegateData(this.setDataGridViewAdmin, null);
            m_admin.SetDelegateDatetime(this.CalendarSetDate);

            m_admin.m_typeFields = s_typeFields;
            
            InitializeComponents();
            
            isActive = false;
        }

        public PanelAdmin(List<StatisticCommon.TEC> tec, FormChangeMode.MANAGER type)
        {
            switch (type)
            {
                case FormChangeMode.MANAGER.DISP:
                    m_admin = new AdminTS_KomDisp(new bool[] { true, false });
                    break;
                case FormChangeMode.MANAGER.NSS:
                    m_admin = new AdminTS_NSS(new bool[] { false, true });
                    break;
                default:
                    break;
            }

            HMark markQueries = new HMark ();
            markQueries.Marked ((int)CONN_SETT_TYPE.ADMIN);
            markQueries.Marked((int)CONN_SETT_TYPE.PBR);

            try { m_admin.InitTEC(tec, markQueries); }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, "PanelAdmin::Initialize () - m_admin.InitTEC ()...");
            }

            if (!(m_admin.m_list_tec.Count > 0))
            {
                Logging.Logg().Error(@"PanelAdmin::PanelAdmin () - список ТЭЦ пуст...");
            }
            else
                ;

            m_admin.SetDelegateData(this.setDataGridViewAdmin, null);
            m_admin.SetDelegateDatetime(this.CalendarSetDate);

            m_admin.m_typeFields = s_typeFields;

            InitializeComponents();

            isActive = false;
        }

        public void SetDelegateWait(DelegateFunc fstart, DelegateFunc fstop, DelegateFunc fev)
        {
            m_admin.SetDelegateWait(fstart, fstop, fev);
        }

        public void SetDelegateReport(DelegateFunc ferr, DelegateFunc fact)
        {
            m_admin.SetDelegateReport(ferr, fact);
        }

        public override void Start () {
            initAdminTableRows();
            
            m_admin.Start ();
        }

        public override void Stop()
        {
            if (! (m_admin.threadIsWorking < 0)) m_admin.Stop(); else ;
        }

        protected virtual void getDataGridViewAdmin() {}

        public virtual void setDataGridViewAdmin(DateTime date) {}

        public void CalendarSetDate(DateTime date)
        {
            BeginInvoke(new DelegateDateFunc(mcldrDate.SetDate), date); //mcldrDate.SetDate(date);
        }

        protected void initAdminTableRows()
        {
            if (DateTime.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.SEASON_DATETIME]).Date == mcldrDate.SelectionStart.Date)
                m_admin.HourSeason = DateTime.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.SEASON_DATETIME]).Hour - 1;
            else
                m_admin.HourSeason = -1;

            if (m_admin.HourSeason < 0)
                dgwAdminTable.InitRows(24, false);
            else
                dgwAdminTable.InitRows(25, true);
        }

        private void mcldrDate_DateSelected(object sender, DateRangeEventArgs e)
        {
            DialogResult result;
            Errors resultSaving;

            bool bRequery = false;

            getDataGridViewAdmin ();

            if (m_admin.WasChanged())
            {
                result = MessageBox.Show(this, "Данные были изменены но не сохранялись.\nЕсли Вы хотите сохранить изменения, нажмите \"да\".\nЕсли Вы не хотите сохранять изменения, нажмите \"нет\".\nДля отмены действия нажмите \"отмена\".", "Внимание", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
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
                            MessageBox.Show(this, "Изменение ретроспективы недопустимо!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        else
                            MessageBox.Show(this, "Не удалось сохранить изменения, возможно отсутствует связь с базой данных.", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                initAdminTableRows();

                m_admin.GetRDGValues((int)m_admin.m_typeFields, m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
            }
            else
                ;
        }

        public virtual void InitializeComboBoxTecComponent (FormChangeMode.MODE_TECCOMPONENT mode) {
            m_listTECComponentIndex = m_admin.GetListIndexTECComponent (mode);

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
                result = MessageBox.Show(this, "Данные были изменены но не сохранялись.\nЕсли Вы хотите сохранить изменения, нажмите \"да\".\nЕсли Вы не хотите сохранять изменения, нажмите \"нет\".\nДля отмены действия нажмите \"отмена\".", "Внимание", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
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
                            MessageBox.Show(this, "Изменение ретроспективы недопустимо!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        else
                            MessageBox.Show(this, "Не удалось сохранить изменения, возможно отсутствует связь с базой данных.", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);

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

                m_admin.GetRDGValues((int)m_admin.m_typeFields, m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
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

                m_admin.GetRDGValues((int)m_admin.m_typeFields, m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
            }
            else
            {
                if (resultSaving == Errors.InvalidValue)
                    MessageBox.Show(this, "Изменение ретроспективы недопустимо!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                else
                    MessageBox.Show(this, "Не удалось сохранить изменения, возможно отсутствует связь с базой данных.", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearTables();

            m_admin.GetRDGValues((int)m_admin.m_typeFields, m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
        }

        private string SetNumberSeparator(string current_str)
        {
            if (current_str.IndexOf(".") > 0)
            {
                if (System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator != ".")
                    return current_str.Replace(".", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);
                else
                    ;
            }
            else
            {
                if (current_str.IndexOf(",") > 0)
                {
                    if (System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator != ",")
                        return current_str.Replace(",", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);
                    else
                        ;
                }
                else
                    ;
            }
            return current_str;
        }

        public bool MayToClose()
        {
            DialogResult result;
            Errors resultSaving;

            getDataGridViewAdmin();

            if (m_admin.WasChanged())
            {
                result = MessageBox.Show(this, "Данные были изменены но не сохранялись.\nЕсли Вы хотите сохранить изменения, нажмите \"да\".\nЕсли Вы не хотите сохранять изменения, нажмите \"нет\".\nДля отмены действия нажмите \"отмена\".", "Внимание", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
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
                            if (MessageBox.Show(this, "Изменение ретроспективы недопустимо!\nПродолжить выход?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                                return true;
                            else
                                return false;
                        else
                            if (MessageBox.Show(this, "Не удалось сохранить изменения, возможно отсутствует связь с базой данных.\nВыйти без сохранения?", "Ошибка сохранения", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
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

        public override void Activate(bool active)
        {
            isActive = active;

            m_admin.Activate (active);
        }
    }
}
