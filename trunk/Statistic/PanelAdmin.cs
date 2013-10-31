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

namespace StatisticCommon
{
    public delegate void DelegateDateFunction(DateTime date);

    public class PanelAdmin : Panel
    {
        private struct RDGStruct
        {
            public double plan;
            public double recomendation;
            public bool deviationPercent;
            public double deviation;
        }

        private struct TecPPBRValues
        {
            //public double[] SN;
            public double[] PBR;
            //public double[] Pmax;
            //public double[] Pmin;

            public TecPPBRValues(int t)
            {
                //this.SN = new double[25];
                this.PBR = new double[25];
                //this.Pmax = new double[24];
                //this.Pmin = new double[24];
            }
        }

        //public enum INDEX_TEC
        //{
        //    BTEC, TEC2, TEC3, TEC4, TEC5,
        //    COUNT_INDEX_TEC
        //}

        //public enum INDEX_TEC_PBR_VALUES
        //{
        //    BTEC_TG1, BTEC_TG2, BTEC_TG35, BTEC_TG4,
        //    TEC2,
        //    TEC3_TG1, TEC3_TG712, TEC3_TG5, TEC3_TG1314,
        //    TEC4_TG3, TEC4_TG48,
        //    TEC5_TG12, TEC5_TG36,
        //    COUNT_TEC_PBR_VALUES
        //};

        //private struct LayoutData
        //{
        //    public int number;
        //    public DateTime date;
        //    public int code;
        //    public int hour_start_in_db;
        //    public int hour_start;
        //    public int hour_end;
        //    public string name_in_db;
        //    public string name;
        //    public TecPPBRValues[] m_arGTPs;
        //    public bool[] existingHours;

        //    public LayoutData(int t)
        //    {
        //        this.number = 0;
        //        this.date = DateTime.Now;
        //        this.code = 0;
        //        this.hour_start_in_db = 0;
        //        this.hour_start = 0;
        //        this.hour_end = 0;
        //        this.name_in_db = "";
        //        this.name = "";

        //        this.m_arGTPs = new TecPPBRValues[(int)INDEX_TEC_PBR_VALUES.COUNT_TEC_PBR_VALUES];
        //        this.existingHours = new bool[24];
        //    }

        //    public void ClearValues()
        //    {
        //        hour_start = 0;
        //        hour_end = 0;
        //        for (int i = 0; i < (int)INDEX_TEC_PBR_VALUES.COUNT_TEC_PBR_VALUES; i++)
        //        {
        //            for (int j = 0; j < 24; j++)
        //            {
        //                existingHours[j] = false;
        //                m_arGTPs[i].PBR[j] = m_arGTPs[i].Pmax[j] = m_arGTPs[i].Pmin[j] =
        //                0.0;
        //            }
        //            m_arGTPs[i].PBR[24] =
        //            0.0;
        //        }
        //    }
        //}

        private System.Windows.Forms.MonthCalendar mcldrDate;
        
        private DataGridViewAdmin dgwAdminTable;
        
        private System.Windows.Forms.Button btnSet;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnImportExcel;
        private System.Windows.Forms.Button btnExportExcel;

        //private System.Windows.Forms.Button btnLoadLayout;
        
        private System.Windows.Forms.ComboBox comboBoxTecComponent;
        private System.Windows.Forms.GroupBox gbxDivider;
        
        private MD5CryptoServiceProvider md5;

        private DelegateFunc delegateStartWait;
        private DelegateFunc delegateStopWait;
        private DelegateFunc delegateEventUpdate;

        private DelegateDateFunction delegateFillData;
        private DelegateDateFunction delegateCalendarSetDate;

        FormChangeMode.MODE_TECCOMPONENT m_modeTECComponent;
        public int mode(int new_mode = (int) FormChangeMode.MODE_TECCOMPONENT.UNKNOWN)
        {
            int prev_mode = (int) m_modeTECComponent;

            if (new_mode == (int) FormChangeMode.MODE_TECCOMPONENT.UNKNOWN)
                ;
            else
                m_modeTECComponent = (FormChangeMode.MODE_TECCOMPONENT) new_mode;

            return prev_mode;
        }

        public string getOwnerPass () {
            string[] ownersPass = { "����������", "��������������", "����" };

            return ownersPass [m_idPass - 1];            
        }

        private volatile RDGStruct[] m_prevRDGValues;
        private RDGStruct[] m_curRDGValues;
        private DateTime m_prevDatetime;
        private volatile List<TECComponent> allTECComponents;
        private volatile int oldTecIndex;
        private volatile List<TEC> m_list_tec;

        private bool is_connection_error;
        private bool is_data_error;

        public volatile string last_error;
        public DateTime last_time_error;
        public volatile bool errored_state;

        public volatile string last_action;
        public DateTime last_time_action;
        public volatile bool actioned_state;

        private StatusStrip stsStrip;

        private DateTime serverTime;
        private DateTime dateForValues;

        private Semaphore semaSave;
        private volatile Errors saveResult;
        private volatile bool saving;

        private Semaphore semaGetPass;
        private Semaphore semaSetPass;
        private volatile Errors passResult;
        private volatile string passReceive;
        private volatile uint m_idPass;

        //private Semaphore semaLoadLayout;
        //private volatile Errors loadLayoutResult;
        //private LayoutData layoutForLoading;

        private Object lockValue;

        private Thread taskThread;
        private Semaphore sem;
        private volatile bool threadIsWorking;
        private volatile bool newState;
        private volatile List<StatesMachine> states;

        private List <DbInterface> m_listDbInterfaces;
        private List <int> m_listListenerIdCurrent;
        private int m_indxDbInterfaceCurrent; //������ � ������ 'm_listDbInterfaces'

        public ConnectionSettings connSettConfigDB;
        int m_indxDbInterfaceConfigDB,
            m_listenerIdConfigDB;

        DataTable m_tablePPBRValuesResponse,
                    m_tableRDGExcelValuesResponse;

        private enum StatesMachine
        {
            CurrentTime,
            AdminValues, //��������� ���������������� ������
            PPBRValues,
            AdminDates, //��������� ������ ����������� ������� ��������
            PPBRDates,
            RDGExcelValues,
            SaveAdminValues, //���������� ���������������� ������
            SavePPBRValues, //���������� PPBR
            //UpdateValuesPPBR, //���������� PPBR ����� 'SaveValuesPPBR'
            GetPass,
            SetPassInsert,
            SetPassUpdate,
            //LayoutGet,
            //LayoutSet,
        }

        private enum StateActions
        {
            Request,
            Data,
        }

        public enum Errors
        {
            NoError,
            InvalidValue,
            NoAccess,
            ParseError,
        }

        private volatile bool using_date;

        private bool[] adminDates;
        private bool[] PPBRDates;

        //��� ��������� ��� (�����)
        //private volatile DbDataInterface dataInterface;
        
        //private Thread dbThread;
        //private Semaphore sema;
        //private volatile bool workTread;
        //-------------------------

        private bool started;

        public bool isActive;

        private void InitializeComponents()
        {
            this.btnSet = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnImportExcel = new System.Windows.Forms.Button();
            this.btnExportExcel = new System.Windows.Forms.Button();
            //this.btnLoadLayout = new System.Windows.Forms.Button();

            this.dgwAdminTable = new DataGridViewAdmin();
            this.mcldrDate = new System.Windows.Forms.MonthCalendar();
            this.comboBoxTecComponent = new System.Windows.Forms.ComboBox();
            this.gbxDivider = new System.Windows.Forms.GroupBox();
            
            this.SuspendLayout();
            
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).BeginInit();

            this.Controls.Add(this.btnSet);
            this.Controls.Add(this.btnRefresh);
            //this.Controls.Add(this.btnLoadLayout);
            this.Controls.Add(this.btnImportExcel);
            this.Controls.Add(this.btnExportExcel);
            this.Controls.Add(this.btnRefresh);
            
            this.Controls.Add(this.dgwAdminTable);
            
            this.Controls.Add(this.mcldrDate);
            this.Controls.Add(this.comboBoxTecComponent);
            this.Controls.Add(this.gbxDivider);
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Location = new System.Drawing.Point(8, 8);
            this.Name = "pnlAdmin";
            this.Size = new System.Drawing.Size(760, 610);
            this.TabIndex = 1;
            // 
            // mcldrDate
            // 
            this.mcldrDate.Location = new System.Drawing.Point(10, 38);
            this.mcldrDate.Name = "mcldrDate";
            this.mcldrDate.TabIndex = 0;
            this.mcldrDate.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.mcldrDate_DateSelected);
            this.mcldrDate.MaxSelectionCount = 1;
            this.mcldrDate.ShowToday = false;
            this.mcldrDate.ShowTodayCircle = false;
            // 
            // dgwAdminTable
            //
            this.dgwAdminTable.Location = new System.Drawing.Point(176, 9);
            this.dgwAdminTable.Name = "dgwAdminTable";
            this.dgwAdminTable.RowHeadersVisible = false;
            this.dgwAdminTable.Size = new System.Drawing.Size(574, 591);
            this.dgwAdminTable.TabIndex = 1;
            //this.dgwAdminTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellClick);
            //this.dgwAdminTable.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellValidated);
            this.dgwAdminTable.RowTemplate.Resizable = DataGridViewTriState.False; 
            // 
            // btnSet
            // 
            this.btnSet.Location = new System.Drawing.Point(10, 204);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(154, 23);
            this.btnSet.TabIndex = 2;
            this.btnSet.Text = "��������� � ����";
            this.btnSet.UseVisualStyleBackColor = true;
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(10, 234);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(154, 23);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "�������� �� ����";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // gbxDivider
            // 
            this.gbxDivider.Location = new System.Drawing.Point(10, 264);
            this.gbxDivider.Name = "gbxDivider";
            this.gbxDivider.Size = new System.Drawing.Size(154, 8);
            this.gbxDivider.TabIndex = 4;
            this.gbxDivider.TabStop = false;
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
            // btnImportExcel
            // 
            this.btnImportExcel.Location = new System.Drawing.Point(10, 284);
            this.btnImportExcel.Name = "btnImportExcel";
            this.btnImportExcel.Size = new System.Drawing.Size(154, 23);
            this.btnImportExcel.TabIndex = 667;
            this.btnImportExcel.Text = "������ �� Excel";
            this.btnImportExcel.UseVisualStyleBackColor = true;
            this.btnImportExcel.Click += new System.EventHandler(this.btnImportExcel_Click);
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.Location = new System.Drawing.Point(10, 314);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(154, 23);
            this.btnExportExcel.TabIndex = 668;
            this.btnExportExcel.Text = "������� � Excel";
            this.btnExportExcel.UseVisualStyleBackColor = true;
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
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
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).EndInit();
            this.ResumeLayout();
        }

        public PanelAdmin(List<TEC> tec, StatusStrip sts)
        {
            InitializeComponents();

            //m_strUsedAdminValues = "AdminValuesNew";
            //m_strUsedPPBRvsPBR = "PPBRvsPBRnew";

            m_listDbInterfaces = new List <DbInterface> ();
            m_listListenerIdCurrent = new List <int> ();

            started = false;

            m_prevRDGValues = new RDGStruct[24];

            md5 = new MD5CryptoServiceProvider();

            is_data_error = is_connection_error = false;

            //TecView tecView = FormMain.selectedTecViews [FormMain.stclTecViews.SelectedIndex];

            isActive = false;

            using_date = false;

            m_curRDGValues = new RDGStruct  [24];

            adminDates = new bool[24];
            PPBRDates = new bool[24];

            //layoutForLoading = new LayoutData(1);

            allTECComponents = new List <TECComponent> ();
            InitTEC (tec);

            lockValue = new Object();

            semaSave = new Semaphore(1, 1);
            semaGetPass = new Semaphore(1, 1);
            semaSetPass = new Semaphore(1, 1);
            //semaLoadLayout = new Semaphore(1, 1);

            delegateFillData = new DelegateDateFunction(setDataGridViewAdmin);
            delegateCalendarSetDate = new DelegateDateFunction(CalendarSetDate);

            //dataInterface = new DbDataInterface();

            stsStrip = sts;

            this.dgwAdminTable.Rows.Add(24);

            states = new List<StatesMachine>();
        }

        private bool WasChanged()
        {
            for (int i = 0; i < 24; i++)
            {
                if (m_prevRDGValues[i].plan != m_curRDGValues[i].plan /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.PLAN].Value.ToString())*/)
                    return true;
                else
                    ;
                if (m_prevRDGValues[i].recomendation != m_curRDGValues[i].recomendation /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION].Value.ToString())*/)
                    return true;
                else
                    ;
                if (m_prevRDGValues[i].deviationPercent != m_curRDGValues[i].deviationPercent /*bool.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION_TYPE].Value.ToString())*/)
                    return true;
                else
                    ;
                if (m_prevRDGValues[i].deviation != m_curRDGValues[i].deviation /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION].Value.ToString())*/)
                    return true;
                else
                    ;
            }
            return false;
        }

        private Errors SaveChanges()
        {
            delegateStartWait();
            semaSave.WaitOne();
            lock (lockValue)
            {
                saveResult = Errors.NoAccess;
                saving = true;
                using_date = false;
                dateForValues = m_prevDatetime;

                newState = true;
                states.Clear();

                Logging.Logg().LogLock();
                Logging.Logg().LogToFile("SaveChanges () - states.Clear()", true, true, false);
                Logging.Logg().LogUnlock();

                states.Add(StatesMachine.CurrentTime);
                states.Add(StatesMachine.AdminDates);
                //??? ��������� ��������� ������ ������� ���������� ����������� �������������� ����� �� ������� '�������������� ���'
                states.Add(StatesMachine.PPBRDates);
                states.Add(StatesMachine.SaveAdminValues);
                states.Add(StatesMachine.SavePPBRValues);
                //states.Add(StatesMachine.UpdateValuesPPBR);

                try
                {
                    sem.Release(1);
                }
                catch
                {
                }
            }

            semaSave.WaitOne();
            try
            {
                semaSave.Release(1);
            }
            catch
            {
            }
            delegateStopWait();
            saving = false;

            return saveResult;
        }

        private void FillOldValues()
        {
            for (int i = 0; i < 24; i++)
            {
                m_prevRDGValues[i].plan = m_curRDGValues[i].plan;
                m_prevRDGValues[i].recomendation = m_curRDGValues[i].recomendation;
                m_prevRDGValues[i].deviationPercent = m_curRDGValues[i].deviationPercent;
                m_prevRDGValues[i].deviation = m_curRDGValues[i].deviation;
            }
        }

        private void getDataGridViewAdmin()
        {
            double value;
            bool valid;

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < (int)DataGridViewAdmin.DESC_INDEX.TO_ALL; j ++) {
                    switch (j)
                    {
                        case (int)DataGridViewAdmin.DESC_INDEX.PLAN: // ����
                            valid = double.TryParse((string)dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.PLAN].Value, out value);
                            m_curRDGValues[i].plan = value;                            
                            break;
                        case (int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION: // ������������
                            {
                                //cellValidated(e.RowIndex, (int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION);

                                valid = double.TryParse((string)dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION].Value, out value);
                                m_curRDGValues[i].recomendation = value;

                                break;
                            }
                        case (int)DataGridViewAdmin.DESC_INDEX.DEVIATION_TYPE:
                            {
                                m_curRDGValues[i].deviationPercent = bool.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                                break;
                            }
                        case (int)DataGridViewAdmin.DESC_INDEX.DEVIATION: // ������������ ����������
                            {
                                valid = double.TryParse((string)this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION].Value, out value);
                                m_curRDGValues[i].deviation = value;
                                
                                break;
                            }
                    }
                }
            }

            FillOldValues();
        }

        private void setDataGridViewAdmin(DateTime date)
        {
            for (int i = 0; i < 24; i++)
            {
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DATE_HOUR].Value = date.AddHours(i + 1).ToString("yyyy-MM-dd HH");
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.PLAN].Value = m_curRDGValues[i].plan.ToString("F2");
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION].Value = m_curRDGValues[i].recomendation.ToString("F2");
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION_TYPE].Value = m_curRDGValues[i].deviationPercent.ToString();
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION].Value = m_curRDGValues[i].deviation.ToString("F2");
            }

            FillOldValues();
        }

        private void CalendarSetDate(DateTime date)
        {
            mcldrDate.SetDate(date);
        }

        private void mcldrDate_DateSelected(object sender, DateRangeEventArgs e)
        {
            DialogResult result;
            Errors resultSaving;

            bool bRequery = false;

            getDataGridViewAdmin ();

            if (WasChanged())
            {
                result = MessageBox.Show(this, "������ ���� �������� �� �� �����������.\n���� �� ������ ��������� ���������, ������� \"��\".\n���� �� �� ������ ��������� ���������, ������� \"���\".\n��� ������ �������� ������� \"������\".", "��������", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
            }
            else
                result = DialogResult.No;

            switch (result)
            {
                case DialogResult.Yes:
                    if ((resultSaving = SaveChanges()) == Errors.NoError)
                    {
                        bRequery = true;
                    }
                    else
                    {
                        if (resultSaving == Errors.InvalidValue)
                            MessageBox.Show(this, "��������� ������������� �����������!", "��������", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        else
                            MessageBox.Show(this, "�� ������� ��������� ���������, �������� ����������� ����� � ����� ������.", "������ ����������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        mcldrDate.SetDate(m_prevDatetime);
                    }
                    break;
                case DialogResult.No:
                    bRequery = true;
                    break;
                case DialogResult.Cancel:
                    mcldrDate.SetDate(m_prevDatetime);
                    break;
            }

            if (bRequery == true) {
                lock (lockValue)
                {
                    ClearValues();
                    ClearTables();
                    m_prevDatetime = e.Start;
                    dateForValues = m_prevDatetime;

                    newState = true;
                    states.Clear();
                    states.Add(StatesMachine.PPBRValues);
                    states.Add(StatesMachine.AdminValues);

                    try
                    {
                        sem.Release(1);
                    }
                    catch
                    {
                    }
                }
            }
            else
                ;
        }

        private void comboBoxTecComponent_SelectionChangeCommitted(object sender, EventArgs e)
        {
            DialogResult result;
            Errors resultSaving;

            bool bRequery = false;

            getDataGridViewAdmin();

            if (WasChanged())
            {
                result = MessageBox.Show(this, "������ ���� �������� �� �� �����������.\n���� �� ������ ��������� ���������, ������� \"��\".\n���� �� �� ������ ��������� ���������, ������� \"���\".\n��� ������ �������� ������� \"������\".", "��������", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
            }
            else
                result = DialogResult.No;

            switch (result)
            {
                case DialogResult.Yes:
                    if ((resultSaving = SaveChanges()) == Errors.NoError)
                    {
                        bRequery = true;
                    }
                    else
                    {
                        if (resultSaving == Errors.InvalidValue)
                            MessageBox.Show(this, "��������� ������������� �����������!", "��������", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        else
                            MessageBox.Show(this, "�� ������� ��������� ���������, �������� ����������� ����� � ����� ������.", "������ ����������", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        comboBoxTecComponent.SelectedIndex = oldTecIndex;
                    }
                    break;
                case DialogResult.No:
                    bRequery = true;
                    break;
                case DialogResult.Cancel:
                    comboBoxTecComponent.SelectedIndex = oldTecIndex;
                    break;
            }

            if (bRequery) {
                lock (lockValue)
                {
                    ClearValues();
                    ClearTables();
                    oldTecIndex = comboBoxTecComponent.SelectedIndex;
                    dateForValues = m_prevDatetime;
                    using_date = false; //true

                    newState = true;
                    states.Clear();
                    states.Add(StatesMachine.CurrentTime);
                    states.Add(StatesMachine.PPBRValues);
                    states.Add(StatesMachine.AdminValues);

                    try
                    {
                        sem.Release(1);
                    }
                    catch
                    {
                    }
                }
            }
            else
                ;

            visibleControlRDGExcel ();
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            getDataGridViewAdmin();
            
            Errors resultSaving;
            if ((resultSaving = SaveChanges()) == Errors.NoError)
            {
                lock (lockValue)
                {
                    ClearValues();
                    ClearTables();
                    dateForValues = mcldrDate.SelectionStart;
                    using_date = false;

                    newState = true;
                    states.Clear();
                    states.Add(StatesMachine.CurrentTime);
                    states.Add(StatesMachine.PPBRValues);
                    states.Add(StatesMachine.AdminValues);

                    try
                    {
                        sem.Release(1);
                    }
                    catch
                    {
                    }
                }
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
            lock (lockValue)
            {
                ClearValues();
                ClearTables();
                dateForValues = mcldrDate.SelectionStart;

                newState = true;
                states.Clear();
                states.Add(StatesMachine.PPBRValues);
                states.Add(StatesMachine.AdminValues);

                try
                {
                    sem.Release(1);
                }
                catch
                {
                }
            }
        }

        private string SetNumberSeparator(string current_str)
        {
            if (current_str.IndexOf(".") > 0)
            {
                if (System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator != ".")
                    return current_str.Replace(".", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);
            }
            else
            {
                if (current_str.IndexOf(",") > 0)
                {
                    if (System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator != ",")
                        return current_str.Replace(",", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);
                }
            }
            return current_str;
        }

        //private bool ParseLayout(string filename)
        //{
        //    StreamReader sr = new StreamReader(filename);
        //    string tmp_str, str = sr.ReadLine();
        //    char divider = ':';
        //    int pos1, pos2, tmp_int, hour;
        //    TecPPBRValues tecPPBRValues;
        //    double[] m_curRDGValues;

        //    layoutForLoading.ClearValues();

        //    if (str == null)
        //    {
        //        sr.Close();
        //        return false;
        //    }

        //    if (str[0] != '/' && str[1] != '/')
        //    {
        //        sr.Close();
        //        return false;
        //    }

        //    pos1 = 2;
        //    pos2 = str.IndexOf(divider, pos1);
        //    if (pos2 <= 0)
        //    {
        //        sr.Close();
        //        return false;
        //    }
        //    if (!int.TryParse(str.Substring(pos1, pos2 - pos1), out layoutForLoading.number))
        //    {
        //        sr.Close();
        //        return false;
        //    }
        //    pos1 = pos2 + 1;
        //    pos2 = str.IndexOf(divider, pos1);
        //    if (pos2 <= 0)
        //    {
        //        sr.Close();
        //        return false;
        //    }
        //    if (!int.TryParse(str.Substring(pos1, pos2 - pos1), out tmp_int))
        //    {
        //        sr.Close();
        //        return false;
        //    }
        //    tmp_str = (tmp_int % 100).ToString() + "." + ((tmp_int / 100) % 100).ToString() + "." + ((tmp_int / 10000) % 100).ToString() + " 00:00:00";
        //    if (!DateTime.TryParse(tmp_str, out layoutForLoading.date))
        //    {
        //        sr.Close();
        //        return false;
        //    }
        //    pos1 = pos2 + 1;
        //    pos2 = str.IndexOf(divider, pos1);
        //    if (pos2 <= 0)
        //    {
        //        sr.Close();
        //        return false;
        //    }
        //    if (!int.TryParse(str.Substring(pos1, pos2 - pos1), out layoutForLoading.code))
        //    {
        //        sr.Close();
        //        return false;
        //    }
        //    pos1 = pos2 + 1;
        //    pos2 = str.IndexOf('-', pos1);
        //    if (pos2 > 0)
        //    {
        //        if (!int.TryParse(str.Substring(pos1, pos2 - pos1), out layoutForLoading.hour_start))
        //        {
        //            sr.Close();
        //            return false;
        //        }
        //        if (layoutForLoading.hour_start > 24)
        //        {
        //            sr.Close();
        //            return false;
        //        }

        //        pos1 = pos2 + 1;
        //        pos2 = str.IndexOf("++", pos1);
        //        if (pos2 <= 0)
        //        {
        //            sr.Close();
        //            return false;
        //        }

        //        layoutForLoading.name = "���" + layoutForLoading.hour_start.ToString();
        //        layoutForLoading.hour_start--;
        //        layoutForLoading.hour_end = 24;//layoutForLoading.hour_start + 3;
        //    }
        //    else
        //    {
        //        pos2 = str.IndexOf("++", pos1);
        //        if (pos2 <= 0)
        //        {
        //            sr.Close();
        //            return false;
        //        }

        //        layoutForLoading.name = "����";
        //        layoutForLoading.hour_start = 0;
        //        layoutForLoading.hour_end = 24;
        //    }

        //    while ((str = sr.ReadLine()) != null)
        //    {
        //        pos1 = 0;
        //        pos2 = str.IndexOf('(', pos1);
        //        if (pos2 < 0)
        //        {
        //            sr.Close();
        //            return false;
        //        }
        //        pos1 = pos2 + 1;
        //        pos2 = str.IndexOf(')', pos1);
        //        if (pos2 <= 0)
        //        {
        //            sr.Close();
        //            return false;
        //        }
        //        if (!int.TryParse(str.Substring(pos1, pos2 - pos1), out tmp_int))
        //        {
        //            sr.Close();
        //            return false;
        //        }
        //        switch (tmp_int % 100)
        //        {
        //            case 1: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC2]; break;
        //            case 2: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1]; break;
        //            case 21: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1]; break;
        //            case 22: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5]; break;
        //            case 23: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712]; break;
        //            case 24: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314]; break;
        //            case 3: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3]; break;
        //            case 31: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3]; break;
        //            case 32: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48]; break;
        //            case 4: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12]; break;
        //            case 41: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12]; break;
        //            case 42: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36]; break;
        //            case 6: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1]; break;
        //            case 61: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1]; break;
        //            case 62: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2]; break;
        //            case 63: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35]; break;
        //            case 64: tecPPBRValues = layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4]; break;
        //            default:
        //                {
        //                    sr.Close();
        //                    return false;
        //                }
        //        }
        //        if (tmp_int / 10000 > 0)
        //            tmp_int /= 100;
        //        else
        //            tmp_int /= 10;
        //        switch (tmp_int % 1000)
        //        {
        //            case 100: /*m_curRDGValues = tecPPBRValues.SN; break;*/
        //            case 200: m_curRDGValues = tecPPBRValues.PBR; break;
        //            case 300: m_curRDGValues = tecPPBRValues.Pmax; break;
        //            case 400: m_curRDGValues = tecPPBRValues.Pmin; break;
        //            default:
        //                {
        //                    sr.Close();
        //                    return false;
        //                }
        //        }
        //        pos1 = pos2 + 2;

        //        hour = 0;
        //        while ((pos2 = str.IndexOf(divider, pos1)) > 0)
        //        {
        //            tmp_str = SetNumberSeparator(str.Substring(pos1, pos2 - pos1));
        //            if (!double.TryParse(tmp_str, out m_curRDGValues[hour++]))
        //            {
        //                sr.Close();
        //                return false;
        //            }
        //            pos1 = pos2 + 1;
        //        }

        //        if ((pos2 = str.IndexOf("==", pos1)) > 0)
        //        {
        //            tmp_str = SetNumberSeparator(str.Substring(pos1, pos2 - pos1));
        //            if (!double.TryParse(tmp_str, out m_curRDGValues[hour++]))
        //            {
        //                sr.Close();
        //                return false;
        //            }
        //            pos1 = pos2 + 1;
        //        }
        //        else
        //        {
        //            tmp_str = SetNumberSeparator(str.Substring(pos1, str.Length - pos1));
        //            if (!double.TryParse(tmp_str, out m_curRDGValues[hour++]))
        //            {
        //                sr.Close();
        //                return false;
        //            }
        //            pos1 = pos2 + 1;
        //        }
        //    }
        //    sr.Close();
        //    return true;
        //}

        //private void btnLoadLayout_Click(object sender, EventArgs e)
        //{
        //    OpenFileDialog ofd = new OpenFileDialog();
        //    ofd.Multiselect = false;
        //    ofd.RestoreDirectory = true;

        //    if (ofd.ShowDialog() == DialogResult.OK)
        //    {
        //        if (!ParseLayout(ofd.FileName))
        //        {
        //            MessageBox.Show(this, "������������ ������ ������. �������� ���������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return;
        //        }

        //        semaLoadLayout.WaitOne();
        //        lock (lockValue)
        //        {
        //            loadLayoutResult = Errors.NoAccess;
        //            using_date = false;
        //            dateForValues = layoutForLoading.date.Date;

        //            newState = true;
        //            states.Clear();
        //            states.Add(StatesMachine.CurrentTime);
        //            states.Add(StatesMachine.LayoutGet);

        //            try
        //            {
        //                sem.Release(1);
        //            }
        //            catch
        //            {
        //            }
        //        }

        //        delegateStartWait();
        //        semaLoadLayout.WaitOne();
        //        try
        //        {
        //            semaLoadLayout.Release(1);
        //        }
        //        catch
        //        {
        //        }
        //        delegateStopWait();

        //        if (loadLayoutResult != Errors.NoError)
        //        {
        //            MessageBox.Show(this, "������ ��������� ���������������� ������. ����� �� ��������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return;
        //        }

        //        if (serverTime.Date.CompareTo(layoutForLoading.date) > 0)
        //        {
        //            MessageBox.Show(this, "������ �� ��������� � ������� ������. ����� �� ��������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return;
        //        }

        //        if (layoutForLoading.hour_start < layoutForLoading.hour_start_in_db)
        //        {
        //            DialogResult result = MessageBox.Show(this, "� ���� ������ ��������� ����� ������� ����� " + layoutForLoading.name_in_db + ".\n���������� ����������?", "��������", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        //            if (result == DialogResult.No)
        //                return;
        //        }
        //        else
        //        {
        //            if (serverTime.Date.CompareTo(layoutForLoading.date) == 0)
        //            {
        //                if (layoutForLoading.hour_start <= serverTime.Hour)
        //                {
        //                    string hours = "";
        //                    for (int i = layoutForLoading.hour_start; i < serverTime.Hour; i++)
        //                        hours += (i + 1).ToString() + ", ";
        //                    hours += (serverTime.Hour + 1).ToString();

        //                    DialogResult result = MessageBox.Show(this, "����� �������� ������ � ��������� ����� � ������� ����.\n�������� � ���� �������� ��� " + hours + " �����?\n", "��������", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        //                    if (result == DialogResult.Cancel)
        //                        return;
        //                    if (result == DialogResult.No)
        //                        layoutForLoading.hour_start = serverTime.Hour + 1;
        //                }
        //            }
        //        }

        //        if (layoutForLoading.hour_start >= layoutForLoading.hour_end)
        //            return;

        //        delegateStartWait();
        //        semaLoadLayout.WaitOne();
        //        lock (lockValue)
        //        {
        //            loadLayoutResult = Errors.NoAccess;

        //            newState = true;
        //            states.Clear();
        //            states.Add(StatesMachine.LayoutSet);

        //            try
        //            {
        //                sem.Release(1);
        //            }
        //            catch
        //            {
        //            }
        //        }

        //        semaLoadLayout.WaitOne();
        //        try
        //        {
        //            semaLoadLayout.Release(1);
        //        }
        //        catch
        //        {
        //        }
        //        delegateStopWait();

        //        if (loadLayoutResult != Errors.NoError)
        //        {
        //            MessageBox.Show(this, "������ ���������� ���������������� ������. ����� �� ��������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return;
        //        }
        //    }
        //}

        private void btnImportExcel_Click(object sender, EventArgs e)
        {
            //OpenFileDialog importExcelBook = new OpenFileDialog ();
            //importExcelBook.Title = "����� ����� � ���";
            //importExcelBook.AddExtension = true;
            //importExcelBook.InitialDirectory = m_list_tec[0].m_path_rdg_excel;
            //importExcelBook.Multiselect = false;
            //importExcelBook.Filter = "����� Excel 97-2003 (*.xls)|*.xls|����� Excel 2010 (*.xlss)|*.xlsx";
            //importExcelBook.FilterIndex = 0;
            ////importExcelBook.DefaultExt = "";
            //importExcelBook.ShowDialog ();

            //DataTable dataExcel;
            //if (importExcelBook.FileName.Length > 0) {
            //    //dataExcel = DbInterface.Request(importExcelBook.FileName, "SELECT * FROM [����1$]");
            //    dataExcel = DbInterface.Request(allTECComponents[oldTecIndex].tec.m_path_rdg_excel + "\\" + dateForValues.GetDateTimeFormats () [5] + ".xls",
            //                @"SELECT * FROM [����1$]");
            //}
            //else
            //    ;

            lock (lockValue)
            {
                ClearValues ();
                ClearTables();

                dateForValues = m_prevDatetime;
                using_date = false;

                newState = true;
                states.Clear();
                states.Add (StatesMachine.RDGExcelValues);

                try
                {
                    sem.Release(1);
                }
                catch
                {
                }
            }
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog exportFolder = new FolderBrowserDialog ();
            exportFolder.ShowDialog ();

            if (exportFolder.SelectedPath.Length > 0) {
            }
            else
                ;
        }

        public void Start()
        {
            if (started)
                return;

            started = true;

            lock (lockValue)
            {
                oldTecIndex = 0;
                using_date = true;
                comboBoxTecComponent.SelectedIndex = oldTecIndex;

                newState = true;
                states.Clear();
                states.Add(StatesMachine.CurrentTime);
                states.Add(StatesMachine.PPBRValues);
                states.Add(StatesMachine.AdminValues);

                try
                {
                    sem.Release(1);
                }
                catch
                {
                }
            }
        }

        public void Reinit()
        {
            if (!started)
                return;
            else
                ;

            InitDbInterfaces ();

            lock (lockValue)
            {
                dateForValues = mcldrDate.SelectionStart;
                saving = false;

                newState = true;
                states.Clear();
                states.Add(StatesMachine.CurrentTime);

                try
                {
                    sem.Release(1);
                }
                catch
                {
                }
            }
        }

        public void Stop()
        {
            if (!started)
                return;

            started = false;
        }

        public void SetDelegate(DelegateFunc dStart, DelegateFunc dStop, DelegateFunc dStatus)
        {
            this.delegateStartWait = dStart;
            this.delegateStopWait = dStop;
            this.delegateEventUpdate = dStatus;
        }

        public bool MayToClose()
        {
            DialogResult result;
            Errors resultSaving;

            getDataGridViewAdmin();

            if (WasChanged())
            {
                result = MessageBox.Show(this, "������ ���� �������� �� �� �����������.\n���� �� ������ ��������� ���������, ������� \"��\".\n���� �� �� ������ ��������� ���������, ������� \"���\".\n��� ������ �������� ������� \"������\".", "��������", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
            }
            else
                result = DialogResult.No;

            switch (result)
            {
                case DialogResult.Yes:
                    if ((resultSaving = SaveChanges()) == Errors.NoError)
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

        public bool FormSetPassword(string password, uint idPass)
        {
            m_idPass = idPass;

            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

            StringBuilder hashedString = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                hashedString.Append(hash[i].ToString("x2"));

            semaGetPass.WaitOne();
            lock (lockValue)
            {
                passResult = Errors.NoAccess;

                newState = true;
                states.Clear();
                states.Add(StatesMachine.GetPass);

                try
                {
                    sem.Release(1);
                }
                catch
                {
                }
            }
            delegateStartWait();
            semaGetPass.WaitOne();
            try
            {
                semaGetPass.Release(1);
            }
            catch
            {
            }

            if (passResult != Errors.NoError)
            {
                delegateStopWait();
                
                MessageBox.Show(this, "������ ��������� ������ " + getOwnerPass () + ". ������ �� ��������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                return false;
            }


            semaSetPass.WaitOne();
            lock (lockValue)
            {
                passResult = Errors.NoAccess;

                newState = true;
                states.Clear();

                if (passReceive == null)
                    states.Add(StatesMachine.SetPassInsert);
                else
                    states.Add(StatesMachine.SetPassUpdate);

                if (password != "")
                    passReceive = hashedString.ToString();

                try
                {
                    sem.Release(1);
                }
                catch
                {
                }
            }
            semaSetPass.WaitOne();
            try
            {
                semaSetPass.Release(1);
            }
            catch
            {
            }
            delegateStopWait();

            if (passResult != Errors.NoError)
            {
                MessageBox.Show(this, "������ ���������� ������ " + getOwnerPass () + ". ������ �� ��������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                return false;
            }

            return true;
        }

        public Errors ComparePassword(string password, uint id)
        {
            if (password.Length < 1)
            {
                MessageBox.Show(this, "����� ������ ������ ����������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return Errors.InvalidValue;
            }
            else
                ;
            
            m_idPass = id;

            string hashFromForm = "";
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(password));

            StringBuilder hashedString = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                hashedString.Append(hash[i].ToString("x2"));


            delegateStartWait();
            semaGetPass.WaitOne();
            lock (lockValue)
            {
                passResult = Errors.NoAccess;

                newState = true;
                states.Clear();
                states.Add(StatesMachine.GetPass);

                try
                {
                    sem.Release(1);
                }
                catch
                {
                }
            }
            semaGetPass.WaitOne();
            try
            {
                semaGetPass.Release(1);
            }
            catch
            {
            }

            //???
            //passResult = Errors.NoError;

            delegateStopWait();
            if (passResult != Errors.NoError)
            {                
                MessageBox.Show(this, "������ ��������� ������ " + getOwnerPass () + ".", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                return Errors.ParseError;
            }

            if (passReceive == null)
            {
                MessageBox.Show(this, "������ �� ����������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                return Errors.NoAccess;
            }
            else
            {
                hashFromForm = hashedString.ToString();
             
                if (hashFromForm != passReceive)
                {
                    MessageBox.Show(this, "������ ����� �������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                    return Errors.InvalidValue;
                }
                else
                    return Errors.NoError;
            }
        }

        public void ClearValues()
        {
            for (int i = 0; i < 24; i++)
            {
                m_curRDGValues[i].plan = m_curRDGValues[i].recomendation = m_curRDGValues[i].deviation = 0;
                m_curRDGValues[i].deviationPercent = false;
            }
            FillOldValues();
        }

        public void ClearTables()
        {
            for (int i = 0; i < 24; i++)
            {
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DATE_HOUR].Value = "";
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.PLAN].Value = "";
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.RECOMENDATION].Value = "";
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION_TYPE].Value = "false";
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdmin.DESC_INDEX.DEVIATION].Value = "";
            }
        }

        private void GetCurrentTimeRequest()
        {
            //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, "SELECT now()");
            Request(allTECComponents[oldTecIndex].tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], allTECComponents[oldTecIndex].tec.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], "SELECT now()");
        }

        private bool GetCurrentTimeResponse(DataTable table)
        {
            if (table.Rows.Count == 1)
            {
                serverTime = (DateTime)table.Rows[0][0];
            }
            else
            {
                DaylightTime daylight = TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year);
                if (TimeZone.IsDaylightSavingTime(DateTime.Now, daylight))
                    serverTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(3 + 1);
                else
                    serverTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(3);
                ErrorReport("������ ��������� �������� ������� �������. ������������ ��������� �����.");
            }

            return true;
        }

        private void GetPPBRValuesRequest(TEC t, TECComponent comp, DateTime date)
        {
            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.PBR], t.m_arListenerIds[(int)CONN_SETT_TYPE.PBR], t.GetPBRValueQuery(comp, date, m_modeTECComponent));
        }

        private void GetAdminValuesRequest(TEC t, TECComponent comp, DateTime date) {
            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], t.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], t.GetAdminValueQuery(comp, date, m_modeTECComponent));
        }

        private void GetRDGExcelValuesRequest () {
            m_tableRDGExcelValuesResponse = DbInterface.Request(allTECComponents[oldTecIndex].tec.m_path_rdg_excel + "\\" + mcldrDate.SelectionStart.GetDateTimeFormats()[4] + ".xls",
                            @"SELECT * FROM [����1$]");
        }

        private bool GetPPBRValuesResponse(DataTable table, DateTime date)
        {
            bool bRes = true;

            m_tablePPBRValuesResponse = table.Copy ();

            return bRes;
        }

        private bool GetAdminValuesResponse(DataTable tableAdminValuesResponse, DateTime date)
        {
            DataTable table = null;
            DataTable[] arTable = { m_tablePPBRValuesResponse, tableAdminValuesResponse };
            int [] arIndexTables = {0, 1};

            int i = -1, j = -1, k = -1,
                hour = -1;

            //�������� �������� 'ID_COMPONENT'
            for (i = 0; i < arTable.Length; i++) {
                /*
                for (j = 0; j < arTable[i].Columns.Count; j++)
                {
                    if (arTable[i].Columns [j].ColumnName == "ID_COMPONENT") {
                        arTable[i].Columns.RemoveAt (j);
                        break;
                    }
                    else
                        ;
                }
                */
                try { arTable[i].Columns.Remove("ID_COMPONENT"); }
                catch (ArgumentException e) { }
            }

            if (arTable[0].Rows.Count < arTable[1].Rows.Count) {
                arIndexTables[0] = 1;
                arIndexTables[1] = 0;
            }
            else {
            }

            table = arTable[arIndexTables [0]].Copy();
            table.Merge(arTable[arIndexTables[1]].Clone (), false);

            for (i = 0; i < arTable[arIndexTables[0]].Rows.Count; i++)
            {
                for (j = 0; j < arTable[arIndexTables[1]].Rows.Count; j++)
                {
                    if (arTable[arIndexTables[0]].Rows[i][0].Equals (arTable[arIndexTables[1]].Rows[j][0])) {
                        for (k = 0; k < arTable[arIndexTables[1]].Columns.Count; k++)
                        {
                            table.Rows [i] [arTable[arIndexTables[1]].Columns [k].ColumnName] = arTable[arIndexTables[1]].Rows[j][k];
                        }
                    }
                    else
                        ;
                }
            }

            //0 - DATE_ADMIN, 1 - REC, 2 - IS_PER, 3 - DIVIAT, 4 - DATE_PBR, 5 - PBR, 6 - PBR_NUMBER
            for (i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i][0] is System.DBNull)
                {
                    try
                    {
                        hour = ((DateTime)table.Rows[i]["DATE_PBR"]).Hour;
                        if (hour == 0 && ((DateTime)table.Rows[i]["DATE_PBR"]).Day != date.Day)
                            hour = 24;
                        else
                            if (hour == 0)
                                continue;
                            else
                                ;

                        m_curRDGValues[hour - 1].plan = (double)table.Rows[i][arIndexTables[1] * 4 + 1/*"PBR"*/];
                        m_curRDGValues[hour - 1].recomendation = 0;
                        m_curRDGValues[hour - 1].deviationPercent = false;
                        m_curRDGValues[hour - 1].deviation = 0;
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        hour = ((DateTime)table.Rows[i]["DATE_ADMIN"]).Hour;
                        if (hour == 0 && ((DateTime)table.Rows[i]["DATE_ADMIN"]).Day != date.Day)
                            hour = 24;
                        else
                            if (hour == 0)
                                continue;
                            else
                                ;

                        m_curRDGValues[hour - 1].recomendation = (double)table.Rows[i][arIndexTables[1] * 3 + 1/*"REC"*/];
                        m_curRDGValues[hour - 1].deviationPercent = (int)table.Rows[i][arIndexTables[1] * 3 + 2/*"IS_PER"*/] == 1;
                        m_curRDGValues[hour - 1].deviation = (double)table.Rows[i][arIndexTables[1] * 3 + 3/*"DIVIAT"*/];
                        if (!(table.Rows[i]["DATE_PBR"] is System.DBNull))
                            m_curRDGValues[hour - 1].plan = (double)table.Rows[i][arIndexTables[0] * 4 + 1/*"PBR"*/];
                        else
                            m_curRDGValues[hour - 1].plan = 0;
                    }
                    catch { }
                }
            }

            return true;
        }

        private bool GetRDGExcelValuesResponse()
        {
            bool bRes = false;
            int i = -1, j = -1,
                iTimeZoneOffset = allTECComponents[oldTecIndex].tec.m_timezone_offset_msc,
                rowRDGExcelStart = 1 + iTimeZoneOffset,
                hour = -1;

            if (m_tableRDGExcelValuesResponse.Rows.Count > 0) bRes = true; else ;

            if (bRes) {
                for (i = rowRDGExcelStart; i < 24 + 1; i++)
                {
                    hour = i - iTimeZoneOffset;

                    for (j = 0; j < allTECComponents[oldTecIndex].TG.Count; j ++)
                        m_curRDGValues[hour - 1].plan += (double)m_tableRDGExcelValuesResponse.Rows[i][allTECComponents[oldTecIndex].TG[j].m_indx_col_rdg_excel - 1];
                    m_curRDGValues[hour - 1].recomendation = 0;
                    m_curRDGValues[hour - 1].deviationPercent = false;
                    m_curRDGValues[hour - 1].deviation = 0;
                }

                /*for (i = hour; i < 24 + 1; i++)
                {
                    hour = i;

                    m_curRDGValues.plan[hour - 1] = 0;
                    m_curRDGValues.recommendations[hour - 1] = 0;
                    m_curRDGValues.deviationPercent[hour - 1] = false;
                    m_curRDGValues.diviation[hour - 1] = 0;
                }*/
            }
            else
                ;

            return bRes;
        }

        private void GetAdminDatesRequest(DateTime date)
        {
            if (mcldrDate.SelectionStart.Date > date.Date)
            {
                date = mcldrDate.SelectionStart.Date;
            }
            else
                ;

            //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, allTECComponents[oldTecIndex].tec.GetAdminDatesQuery(date));
            Request(allTECComponents[oldTecIndex].tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], allTECComponents[oldTecIndex].tec.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], allTECComponents[oldTecIndex].tec.GetAdminDatesQuery(date, m_modeTECComponent, allTECComponents[oldTecIndex]));
        }

        private void GetPPBRDatesRequest(DateTime date)
        {
            if (mcldrDate.SelectionStart.Date > date.Date)
            {
                date = mcldrDate.SelectionStart.Date;
            }
            else
                ;

//            Request(m_indxDbInterfaceCommon, m_listenerIdCommon, allTECComponents[oldTecIndex].tec.GetPBRDatesQuery(date));
            Request(allTECComponents[oldTecIndex].tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], allTECComponents[oldTecIndex].tec.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], allTECComponents[oldTecIndex].tec.GetPBRDatesQuery(date, m_modeTECComponent, allTECComponents[oldTecIndex]));
        }

        private void ClearAdminDates()
        {
            for (int i = 0; i < 24; i++)
                adminDates[i] = false;
        }

        private void ClearPPBRDates()
        {
            for (int i = 0; i < 24; i++)
                PPBRDates[i] = false;
        }

        private bool GetAdminDatesResponse(DataTable table, DateTime date)
        {
            for (int i = 0, hour; i < table.Rows.Count; i++)
            {
                try
                {
                    hour = ((DateTime)table.Rows[i][0]).Hour;
                    if (hour == 0 && ((DateTime)table.Rows[i][0]).Day != date.Day)
                        hour = 24;
                    else
                        ;

                    adminDates[hour - 1] = true;
                }
                catch { }
            }
            return true;
        }

        private bool GetPPBRDatesResponse(DataTable table, DateTime date)
        {
            for (int i = 0, hour; i < table.Rows.Count; i++)
            {
                try
                {
                    hour = ((DateTime)table.Rows[i][0]).Hour;
                    if (hour == 0 && ((DateTime)table.Rows[i][0]).Day != date.Day)
                        hour = 24;
                    else
                        ;

                    PPBRDates[hour - 1] = true;
                }
                catch { }
            }
            return true;
        }

        private void SetAdminValuesRequest(TEC t, TECComponent comp, DateTime date)
        {
            int currentHour = serverTime.Hour;

            date = date.Date;

            if (serverTime.Date < date)
                currentHour = 0;
            else
                ;

            string strUsedAdminValues = "AdminValuesOfID",
                    requestUpdate = string.Empty,
                    requestInsert = string.Empty,
                    name = t.NameFieldOfAdminRequest(comp);

            for (int i = currentHour; i < 24; i++)
            {
                // ������ ��� ����� ���� �������, ������������ �
                if (adminDates[i])
                {
                    switch (m_modeTECComponent) {
                        case FormChangeMode.MODE_TECCOMPONENT.GTP:
                            //name = t.NameFieldOfAdminRequest(comp);
                            
                            requestUpdate += @"UPDATE " + t.m_strUsedAdminValues + " SET " + name + @"_REC='" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + name + @"_IS_PER=" + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                        @", " + name + "_DIVIAT='" + m_curRDGValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";
                            break;
                        case FormChangeMode.MODE_TECCOMPONENT.PC:
                            requestUpdate += @"UPDATE " + strUsedAdminValues + " SET " + @"REC='" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + @"IS_PER=" + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                        @", " + "DIVIAT='" + m_curRDGValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'" +
                                        @" AND ID_COMPONENT = " + comp.m_id + "; ";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    // ������ �����������, ���������� ��������
                    switch (m_modeTECComponent) {
                        case FormChangeMode.MODE_TECCOMPONENT.GTP:
                            requestInsert += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                        @", '" + m_curRDGValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"'),";
                            break;
                        case FormChangeMode.MODE_TECCOMPONENT.PC:
                            requestInsert += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + m_curRDGValues[i].recomendation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + (m_curRDGValues[i].deviationPercent ? "1" : "0") +
                                        @", '" + m_curRDGValues[i].deviation.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + (comp.m_id) +
                                        @"),";
                            break;
                        default:
                            break;
                    }
                }
            }

            // ��������� ��� ������, �� ��������� � ����
            if (requestInsert != "")
            {
                switch (m_modeTECComponent)
                {
                    case FormChangeMode.MODE_TECCOMPONENT.GTP:
                        requestInsert = @"INSERT INTO " + t.m_strUsedAdminValues + " (DATE, " + name + @"_REC" +
                                @", " + name + "_IS_PER" +
                                @", " + name + "_DIVIAT) VALUES" + requestInsert.Substring(0, requestInsert.Length - 1) + ";";
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.PC:
                        requestInsert = @"INSERT INTO " + strUsedAdminValues + " (DATE, " + @"REC" +
                                @", " + "IS_PER" +
                                @", " + "DIVIAT" +
                                @", " + "ID_COMPONENT" +
                                @") VALUES" + requestInsert.Substring(0, requestInsert.Length - 1) + ";";
                        break;
                    default:
                        break;
                }
            }
            else
                ;

            string requestDelete = string.Empty;
                                   //@"DELETE FROM " + t.m_strUsedAdminValues + " WHERE " +
                                   //@"BTEC_TG1_REC = 0 AND BTEC_TG1_IS_PER = 0 AND BTEC_TG1_DIVIAT = 0 AND " +
                                   //@"BTEC_TG2_REC = 0 AND BTEC_TG2_IS_PER = 0 AND BTEC_TG2_DIVIAT = 0 AND " +
                                   //@"BTEC_TG35_REC = 0 AND BTEC_TG35_IS_PER = 0 AND BTEC_TG35_DIVIAT = 0 AND " +
                                   //@"BTEC_TG4_REC = 0 AND BTEC_TG4_IS_PER = 0 AND BTEC_TG4_DIVIAT = 0 AND " +
                                   //@"TEC2_REC = 0 AND TEC2_IS_PER = 0 AND TEC2_DIVIAT = 0 AND " +
                                   //@"TEC3_TG1_REC = 0 AND TEC3_TG1_IS_PER = 0 AND TEC3_TG1_DIVIAT = 0 AND " +
                                   //@"TEC3_TG5_REC = 0 AND TEC3_TG5_IS_PER = 0 AND TEC3_TG5_DIVIAT = 0 AND " +
                                   //@"TEC3_TG712_REC = 0 AND TEC3_TG712_IS_PER = 0 AND TEC3_TG712_DIVIAT = 0 AND " +
                                   //@"TEC3_TG1314_REC = 0 AND TEC3_TG1314_IS_PER = 0 AND TEC3_TG1314_DIVIAT = 0 AND " +
                                   //@"TEC4_TG3_REC = 0 AND TEC4_TG3_IS_PER = 0 AND TEC4_TG3_DIVIAT = 0 AND " +
                                   //@"TEC4_TG48_REC = 0 AND TEC4_TG48_IS_PER = 0 AND TEC4_TG48_DIVIAT = 0 AND " +
                                   //@"TEC5_TG12_REC = 0 AND TEC5_TG12_IS_PER = 0 AND TEC5_TG12_DIVIAT = 0 AND " +
                                   //@"TEC5_TG36_REC = 0 AND TEC5_TG36_IS_PER = 0 AND TEC5_TG36_DIVIAT = 0 AND " +
                                   //@"DATE > '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
                                   //@"' AND DATE <= '" + date.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss") +
                                   //@"';";

            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], t.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], requestUpdate + requestInsert + requestDelete);
        }

        private int getPBRNumber(int hour)
        {
            int iNum = -1;

            switch (hour)
            {
                case 0:
                case 1:
                    iNum = 1;
                    break;
                case 2:
                case 3:
                    iNum = 3;
                    break;
                case 4:
                case 5:
                    iNum = 5;
                    break;
                case 6:
                case 7:
                    iNum = 7;
                    break;
                case 8:
                case 9:
                    iNum = 9;
                    break;
                case 10:
                case 11:
                    iNum = 11;
                    break;
                case 12:
                case 13:
                    iNum = 13;
                    break;
                case 14:
                case 15:
                    iNum = 15;
                    break;
                case 16:
                case 17:
                    iNum = 17;
                    break;
                case 18:
                case 19:
                    iNum = 19;
                    break;
                default:
                    iNum = 21;
                    break;
            }

            return iNum;
        }

        private void SetPPBRRequest(TEC t, TECComponent comp, DateTime date)
        {
            int currentHour = serverTime.Hour;

            date = date.Date;

            if (serverTime.Date < date)
                currentHour = 0;
            else
                ;

            string strUsedPPBRvsPBR = "PPBRvsPBROfID",
                    requestUpdate = "", requestInsert = "";

            string name = t.NameFieldOfPBRRequest(comp);

            for (int i = currentHour; i < 24; i++)
            {
                // ������ ��� ����� ���� �������, ������������ �
                if (PPBRDates[i])
                {
                    switch (m_modeTECComponent)
                    {
                        case FormChangeMode.MODE_TECCOMPONENT.GTP:
                            /*requestUpdate += @"UPDATE " + t.m_strUsedPPBRvsPBR + " SET " + name + @"_" + t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.REC] + "='" + m_curRDGValues[i].plan.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";*/
                            requestUpdate += @"UPDATE " + t.m_strUsedPPBRvsPBR + " SET " + name + @"_" + t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR] + "='" + m_curRDGValues[i].plan.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";
                            break;
                        case FormChangeMode.MODE_TECCOMPONENT.PC:
                            requestUpdate += @"UPDATE " + strUsedPPBRvsPBR + " SET " + @"PBR='" + m_curRDGValues[i].plan.ToString("F2", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'" +
                                        @" AND ID_COMPONENT = " + comp.m_id + "; ";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    // ������ �����������, ���������� ��������
                    switch (m_modeTECComponent)
                    {
                        case FormChangeMode.MODE_TECCOMPONENT.GTP:
                            requestInsert += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + serverTime.Date.ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + "���" + getPBRNumber(i) +
                                        @"', '" + "0" +
                                        @"', '" + m_curRDGValues[i].plan.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"'),";
                            break;
                        case FormChangeMode.MODE_TECCOMPONENT.PC:
                            requestInsert += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + serverTime.Date.ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + "���" + getPBRNumber(i) +
                                        @"', " + comp.m_id +
                                        @", '" + "0" +
                                        @"', " + m_curRDGValues[i].plan.ToString("F1", CultureInfo.InvariantCulture) +
                                        @"),";
                            break;
                        default:
                            break;
                    }
                }
            }

            // ��������� ��� ������, �� ��������� � ����
            if (requestInsert != "")
            {
                switch (m_modeTECComponent)
                {
                    case FormChangeMode.MODE_TECCOMPONENT.GTP:
                        requestInsert = @"INSERT INTO " + t.m_strUsedPPBRvsPBR + " (DATE_TIME, WR_DATE_TIME, PBR_NUMBER, IS_COMDISP, " + name + @"_PBR) VALUES" + requestInsert.Substring(0, requestInsert.Length - 1) + ";";
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.PC:
                        requestInsert = @"INSERT INTO " + strUsedPPBRvsPBR + " (DATE_TIME, WR_DATE_TIME, PBR_NUMBER, ID_COMPONENT, OWNER, PBR) VALUES" + requestInsert.Substring(0, requestInsert.Length - 1) + ";";
                        break;
                    default:
                        break;
                }
            }
            else
                ;

            string requestDelete = @"";
                                   //@"DELETE FROM " + m_strUsedPPBRvsPBR + " WHERE " +
                                   //@"BTEC_PBR = 0 AND BTEC_Pmax = 0 AND BTEC_Pmin = 0 AND " +
                                   //@"BTEC_TG1_PBR = 0 AND BTEC_TG1_Pmax = 0 AND BTEC_TG1_Pmin = 0 AND " +
                                   //@"BTEC_TG2_PBR = 0 AND BTEC_TG2_Pmax = 0 AND BTEC_TG2_Pmin = 0 AND " +
                                   //@"BTEC_TG35_PBR = 0 AND BTEC_TG35_Pmax = 0 AND BTEC_TG35_Pmin = 0 AND " +
                                   //@"BTEC_TG4_PBR = 0 AND BTEC_TG4_Pmax = 0 AND BTEC_TG4_Pmin = 0 AND " +
                                   //@"TEC2_PBR = 0 AND TEC2_Pmax = 0 AND TEC2_Pmin = 0 AND " +
                                   //@"TEC3_PBR = 0 AND TEC3_TG1_Pmax = 0 AND TEC3_TG1_Pmin = 0 AND " +
                                   //@"TEC3_TG1_PBR = 0 AND TEC3_TG1_Pmax = 0 AND TEC3_TG1_Pmin = 0 AND " +
                                   //@"TEC3_TG5_PBR = 0 AND TEC3_TG5_Pmax = 0 AND TEC3_TG5_Pmin = 0 AND " +
                                   //@"TEC3_TG712_PBR = 0 AND TEC3_TG712_Pmax = 0 AND TEC3_TG712_Pmin = 0 AND " +
                                   //@"TEC3_TG1314_PBR = 0 AND TEC3_TG1314_Pmax = 0 AND TEC3_TG1314_Pmin = 0 AND " +
                                   //@"TEC4_PBR = 0 AND TEC4_TG3_Pmax = 0 AND TEC4_TG3_Pmin = 0 AND " +
                                   //@"TEC4_TG3_PBR = 0 AND TEC4_TG3_Pmax = 0 AND TEC4_TG3_Pmin = 0 AND " +
                                   //@"TEC4_TG48_PBR = 0 AND TEC4_TG48_Pmax = 0 AND TEC4_TG48_Pmin = 0 AND " +
                                   //@"TEC5_PBR = 0 AND TEC5_TG12_Pmax = 0 AND TEC5_TG12_Pmin = 0 AND " +
                                   //@"TEC5_TG12_PBR = 0 AND TEC5_TG12_Pmax = 0 AND TEC5_TG12_Pmin = 0 AND " +
                                   //@"TEC5_TG36_PBR = 0 AND TEC5_TG36_Pmax = 0 AND TEC5_TG36_Pmin = 0 AND " +
                                   //@"DATE_TIME > '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
                                   //@"' AND DATE_TIME <= '" + date.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss") +
                                   //@"';";

            Logging.Logg().LogLock();
            Logging.Logg().LogToFile("SetPPBRRequest", true, true, false);
            Logging.Logg().LogUnlock();
            
            //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, requestUpdate + requestInsert + requestDelete);
            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], t.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], requestUpdate + requestInsert + requestDelete);
        }

        private void GetPassRequest(uint id)
        {
            string request = "SELECT HASH FROM passwords WHERE ID_ROLE=" + id;
            
            Request(m_indxDbInterfaceConfigDB, m_listenerIdConfigDB, request);
        }

        private bool GetPassResponse(DataTable table)
        {
            if (table.Rows.Count != 0)
                try
                {
                    if (table.Rows[0][0] is System.DBNull)
                        passReceive = "";
                    else
                        passReceive = (string)table.Rows[0][0];
                }
                catch
                {
                    return false;
                }
            else
                passReceive = null;

            return true;
        }

        private void SetPassRequest(string password, uint id, bool insert)
        {
            string query = string.Empty;
            
            if (insert)
                switch (m_idPass) {
                    case 1:
                        query = "INSERT INTO passwords (ID_ROLE, HASH) VALUES (" + id + ", '" + password + "')";
                        break;
                    case 2:
                        query = "INSERT INTO passwords (ID_ROLE, HASH) VALUES (" + id + ", '" + password + "')";
                        break;
                    case 3:
                        query = "INSERT INTO passwords (ID_ROLE, HASH) VALUES (" + id + ", '" + password + "')";
                        break;
                    default:
                        break;
                }
            else {
                switch (m_idPass)
                {
                    case 1:
                        query = "UPDATE passwords SET HASH='" + password + "'";
                        break;
                    case 2:
                        query = "UPDATE passwords SET HASH='" + password + "'";
                        break;
                    case 3:
                        query = "UPDATE passwords SET HASH='" + password + "'";
                        break;
                    default:
                        break;
                }

                query += " WHERE ID_USER=ID_ROLE AND ID_ROLE=" + id;
            }

            Request(m_indxDbInterfaceConfigDB, m_listenerIdConfigDB, query);
        }

        //private void GetLayoutRequest(DateTime date)
        //{
        //    string request = @"SELECT " + m_strUsedPPBRvsPBR + ".DATE_TIME, " + m_strUsedPPBRvsPBR + ".PBR_NUMBER FROM " + m_strUsedPPBRvsPBR + " " +
        //                     @"WHERE " + m_strUsedPPBRvsPBR + ".DATE_TIME >= '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
        //                     @"' AND " + m_strUsedPPBRvsPBR + ".DATE_TIME <= '" + date.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
        //                     @"' AND MINUTE(" + m_strUsedPPBRvsPBR + ".DATE_TIME) = 0 ORDER BY " + m_strUsedPPBRvsPBR + ".DATE_TIME ASC";
        //    Request(m_indxDbInterfaceCommon, m_listenerIdCommon, request);
        //}

        //private bool GetLayoutResponse(DataTable table, DateTime date)
        //{
        //    int num = 0;
        //    layoutForLoading.hour_start_in_db = 0;
        //    layoutForLoading.name_in_db = "";
        //    string name;
        //    for (int i = 0, hour; i < table.Rows.Count; i++)
        //    {
        //        try
        //        {
        //            hour = ((DateTime)table.Rows[i][0]).Hour;
        //            if (hour == 0 && ((DateTime)table.Rows[i][0]).Day != date.Day)
        //                hour = 24;
        //            layoutForLoading.existingHours[hour - 1] = true;
        //            name = (string)table.Rows[i][1];
        //            switch (name)
        //            {
        //                case "����": num = 0; break;
        //                default:
        //                    {
        //                        if (name.Substring(0, 3) == "���" && int.TryParse(name.Substring(3), out num) == true && num > 0)
        //                            num--;
        //                        else
        //                            num = 0;
        //                        break;
        //                    }
        //            }

        //            if (layoutForLoading.hour_start_in_db < num)
        //            {
        //                layoutForLoading.hour_start_in_db = num;
        //                layoutForLoading.name_in_db = name;
        //            }
        //        }
        //        catch
        //        {
        //        }
        //    }
        //    return true;
        //}

        //private void SetLayoutRequest(DateTime date)
        //{
        //    string requestInsert = "";
        //    string requestUpdate = "";

        //    for (int i = layoutForLoading.hour_start; i < layoutForLoading.hour_end; i++)
        //    {
        //        // ������ ��� ����� ���� �������, ������������ �
        //        if (layoutForLoading.existingHours[i])
        //        {
        //            if (layoutForLoading.name == "����")
        //            {
        //                requestUpdate += @"UPDATE " + m_strUsedPPBRvsPBR + " SET IS_COMDISP = 1, PBR_NUMBER = '" + layoutForLoading.name +
        //                                 @"', DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
        //                                 @"', WR_DATE_TIME = now()" +
        //                    //@"', SN_BTEC = '" + "0".ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_PPBR = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].PBR[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_PMAX = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_PMIN = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG1_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG1_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG1_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG2_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG2_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG2_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG35_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG35_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG35_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG4_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG4_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG4_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                    //@", SN_TEC2 = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2].SN[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC2_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC2].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC2_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC2].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC2_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC2].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                    //@"', SN_TEC3 = '" + layoutForLoading.TEC3_110.SN[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_PPBR = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].PBR[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_PMAX = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_PMIN = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG1_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG1_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG1_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG5_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG5_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG5_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG712_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG712_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG712_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG1314_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG1314_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG1314_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                    //@"', SN_TEC4 = '" + layoutForLoading.TEC4.SN[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_PPBR = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].PBR[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_PMAX = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_PMIN = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_TG3_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_TG3_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_TG3_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_TG48_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_TG48_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_TG48_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                    //@"', SN_TEC5 = '" + layoutForLoading.TEC5_110.SN[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_PPBR = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].PBR[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_PMAX = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_PMIN = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_TG12_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_TG12_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_TG12_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_TG36_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_TG36_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_TG36_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"' WHERE " +
        //                                 @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
        //                                 @"'; ";
        //            }
        //            else
        //            {
        //                requestUpdate += @"UPDATE " + m_strUsedPPBRvsPBR + " SET IS_COMDISP = 1, PBR_NUMBER = '" + layoutForLoading.name +
        //                                 @"', DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
        //                                 @"', WR_DATE_TIME = now()" +
        //                    //@"', SN_BTEC = '" + "0".ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_PPBR = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].PBR[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_PMAX = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_PMIN = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG1_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG1_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG1_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG2_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG2_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG2_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG35_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG35_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG35_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG4_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG4_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', BTEC_TG4_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                    //@", SN_TEC2 = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2].SN[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC2_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC2].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC2_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC2].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC2_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC2].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                    //@"', SN_TEC3 = '" + layoutForLoading.TEC3_110.SN[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_PPBR = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].PBR[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_PMAX = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_PMIN = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG1_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG1_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG1_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG5_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG5_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG5_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG712_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG712_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG712_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG1314_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG1314_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC3_TG1314_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                    //@"', SN_TEC4 = '" + layoutForLoading.TEC4.SN[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_PPBR = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].PBR[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_PMAX = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_PMIN = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_TG3_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_TG3_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_TG3_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_TG48_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_TG48_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC4_TG48_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                    //@"', SN_TEC5 = '" + layoutForLoading.TEC5_110.SN[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_PPBR = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].PBR[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_PMAX = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_PMIN = '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_TG12_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_TG12_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_TG12_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_TG36_PPBR = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', TEC5_TG36_PMAX = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"', " + INDEX_TEC_PBR_VALUES.TEC5_TG36.ToString () + "_PMIN = '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                                 @"' WHERE " +
        //                                 @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
        //                                 @"'; ";
        //            }
        //        }
        //        else
        //        {
        //            // ������ �����������, ���������� ��������
        //            requestInsert += @" (1, '" + layoutForLoading.name +
        //                             @"', '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
        //                             @"', now()" +
        //                //HHH ����
        //                //@"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].SN[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].PBR[i]).ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                //HHH
        //                //���-2
        //                //@", '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2].SN[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC2].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC2].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC2].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                //���-3
        //                //@"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].SN[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].PBR[i]).ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                //���-4
        //                //@"', '" + layoutForLoading.TEC4.SN[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].PBR[i]).ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                //���-5
        //                //@"', '" + layoutForLoading.TEC5_110.SN[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].PBR[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].PBR[i]).ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmax[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + (layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmin[i] + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"'),";
        //        }
        //    }

        //    if (requestInsert != "")
        //    {
        //        requestInsert = @"INSERT INTO " + m_strUsedPPBRvsPBR + " (IS_COMDISP, PBR_NUMBER, DATE_TIME, WR_DATE_TIME, " +
        //            //����
        //                        @"BTEC_PPBR, BTEC_PMAX, BTEC_PMIN, " +
        //                        @"BTEC_TG1_PBR, BTEC_TG1_PMAX, BTEC_TG1_PMIN, " +
        //                        @"BTEC_TG2_PPBR, BTEC_TG2_PMAX, BTEC_TG2_PMIN" +
        //                        @"BTEC_TG1_PBR, BTEC_TG1_PMAX, BTEC_TG1_PMIN, " +
        //                        @"BTEC_TG2_PPBR, BTEC_TG2_PMAX, BTEC_TG2_PMIN" +
        //            //���-2
        //                        @"TEC2_PPBR, TEC2_PMAX, TEC2_PMIN, " +
        //            //���-3
        //                        @"TEC3_PPBR, TEC3_PMAX, TEC3_PMIN, " +
        //                        @"TEC3_TG1_PPBR, TEC3_TG1_PMAX, TEC3_TG1_PMIN, " +
        //                        @"TEC3_TG5_PPBR, TEC3_TG5_PMAX, TEC3_TG5_PMIN, " +
        //                        @"TEC3_TG712_PPBR, TEC3_TG712_PMAX, TEC3_TG712_PMIN, " +
        //                        @"TEC3_TG1314_PPBR, TEC3_TG1314_PMAX, TEC3_TG1314_PMIN, " +
        //            //���-4
        //                        @"TEC4_PPBR, TEC4_PMAX, TEC4_PMIN, " +
        //                        @"TEC4_TG3_PBR, TEC4_TG3_PMAX, TEC4_TG3_PMIN, " +
        //                        @"TEC4_TG48_PPBR, TEC4_TG48_PMAX, TEC4_TG48_PMIN" +
        //            //���-5
        //                        @"TEC5_PPBR, TEC5_PMAX, TEC5_PMIN, " +
        //                        @"TEC5_TG12_PBR, TEC5_TG12_PMAX, TEC5_TG12_PMIN, " +
        //                        @"TEC5_TG12_PPBR, TEC5_TG12_PMAX, TEC5_TG12_PMIN" +
        //                        @") VALUES" + requestInsert.Substring(0, requestInsert.Length - 1) + ";";
        //    }

        //    Request(m_indxDbInterfaceCommon, m_listenerIdCommon, requestInsert + requestUpdate);
        //}

        private void ErrorReport(string error_string)
        {
            last_error = error_string;
            last_time_error = DateTime.Now;
            errored_state = true;
            stsStrip.BeginInvoke(delegateEventUpdate);
        }

        private void ActionReport(string action_string)
        {
            last_action = action_string;
            last_time_action = DateTime.Now;
            actioned_state = true;
            stsStrip.BeginInvoke(delegateEventUpdate);
        }

        private void visibleControlRDGExcel () {
            bool bImpExpButtonVisible = false;
            if (allTECComponents[oldTecIndex].tec.m_path_rdg_excel.Length > 0)
                switch (m_modeTECComponent)
                {
                    case FormChangeMode.MODE_TECCOMPONENT.TEC:
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.GTP:
                        ; //bImpExpButtonVisible = false;
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.PC:
                        bImpExpButtonVisible = true;
                        break;
                    default:
                        break;
                }
            else
                ;

            btnImportExcel.Visible =
            btnExportExcel.Visible = bImpExpButtonVisible;
        }

        public void Activate(bool active)
        {
            visibleControlRDGExcel ();

            isActive = active;
        }

        public void Request(int indxDbInterface, int listenerId, string request)
        {
            m_indxDbInterfaceCurrent = indxDbInterface;
            m_listListenerIdCurrent[indxDbInterface] = listenerId;
            m_listDbInterfaces[indxDbInterface].Request(m_listListenerIdCurrent[indxDbInterface], request);
        }

        public bool GetResponse(int indxDbInterface, int listenerId, out bool error, out DataTable table/*, bool isTec*/)
        {
            if ((!(m_indxDbInterfaceCurrent < 0)) && (m_listListenerIdCurrent.Count > 0) && (!(m_indxDbInterfaceCurrent < 0))) {
                //m_listListenerIdCurrent [m_indxDbInterfaceCurrent] = -1;
                //m_indxDbInterfaceCurrent = -1;
                ;
            }
            else
                ;

            return m_listDbInterfaces[indxDbInterface].GetResponse(listenerId, out error, out table);
            
            //if (isTec)
            //    return dbInterface.GetResponse(listenerIdTec, out error, out table);
            //else
            //    return dbInterface.GetResponse(listenerIdAdmin, out error, out table);
        }

        public void InitTEC (List <TEC> tec) {
            this.m_list_tec = tec;

            comboBoxTecComponent.Items.Clear ();
            allTECComponents.Clear ();

            foreach (TEC t in tec)
            {
                if (t.list_TECComponents.Count > 0)
                    foreach (TECComponent g in t.list_TECComponents)
                    {
                        comboBoxTecComponent.Items.Add(t.name + " - " + g.name);
                        allTECComponents.Add(g);
                    }
                else
                {
                    comboBoxTecComponent.Items.Add(t.name);
                    allTECComponents.Add(t.list_TECComponents[0]);
                }
            }
        }

        private void InitDbInterfaces () {
            m_listDbInterfaces.Clear ();

            m_listListenerIdCurrent.Clear();
            m_indxDbInterfaceCurrent = -1;

            m_listDbInterfaces.Add(new DbInterface(DbInterface.DbInterfaceType.MySQL, "��������� MySQL-��: ������������"));
            m_listListenerIdCurrent.Add(-1);

            m_indxDbInterfaceConfigDB = m_listDbInterfaces.Count - 1;
            m_listenerIdConfigDB = m_listDbInterfaces[m_indxDbInterfaceConfigDB].ListenerRegister();

            m_listDbInterfaces[m_listDbInterfaces.Count - 1].Start();

            m_listDbInterfaces[m_listDbInterfaces.Count - 1].SetConnectionSettings(connSettConfigDB);

            Int16 connSettType = -1; 
            foreach (TEC t in m_list_tec)
            {
                for (connSettType = (int)CONN_SETT_TYPE.ADMIN; connSettType < (int)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; connSettType++) {
                    bool isAlready = false;
                    
                    foreach (DbInterface dbi in m_listDbInterfaces) {
                        //if (! (t.connSetts [0] == cs))
                        //if (dbi.connectionSettings.Equals(t.connSetts[(int)CONN_SETT_TYPE.ADMIN]) == true)
                        if (dbi.connectionSettings == t.connSetts[connSettType])
                        //if (! (dbi.connectionSettings != t.connSetts[(int)CONN_SETT_TYPE.ADMIN]))
                        {
                            isAlready = true;

                            t.m_arIndxDbInterfaces[connSettType] = m_listDbInterfaces.IndexOf(dbi);
                            t.m_arListenerIds[connSettType] = m_listDbInterfaces[t.m_arIndxDbInterfaces[connSettType]].ListenerRegister();

                            break;
                        }
                        else
                            ;
                    }

                    if (isAlready == false) {
                        m_listDbInterfaces.Add(new DbInterface(DbInterface.DbInterfaceType.MySQL, "��������� MySQL-��: �������������"));
                        m_listListenerIdCurrent.Add (-1);

                        t.m_arIndxDbInterfaces[connSettType] = m_listDbInterfaces.Count - 1;
                        t.m_arListenerIds[connSettType] = m_listDbInterfaces[m_listDbInterfaces.Count - 1].ListenerRegister();

                        if (m_indxDbInterfaceConfigDB < 0) {
                            m_indxDbInterfaceConfigDB = m_listDbInterfaces.Count - 1;
                            m_listenerIdConfigDB = m_listDbInterfaces[m_indxDbInterfaceConfigDB].ListenerRegister();
                        }
                        else
                            ;

                        m_listDbInterfaces [m_listDbInterfaces.Count - 1].Start ();

                        m_listDbInterfaces[m_listDbInterfaces.Count - 1].SetConnectionSettings(t.connSetts[connSettType]);
                    }
                    else
                        ;
                }
            }
        }

        public void StartDbInterface()
        {
            InitDbInterfaces ();

            threadIsWorking = true;

            taskThread = new Thread (new ParameterizedThreadStart(TecView_ThreadFunction));
            taskThread.Name = "��������� � ������";
            taskThread.IsBackground = true;

            sem = new Semaphore(1, 1);

            sem.WaitOne();
            taskThread.Start();
        }

        public void StopDbInterface()
        {
            bool joined;
            threadIsWorking = false;
            lock (lockValue)
            {
                newState = true;
                states.Clear();
                errored_state = false;
            }

            if (taskThread.IsAlive)
            {
                try { sem.Release(1); }
                catch { }

                joined = taskThread.Join(1000);
                if (!joined)
                    taskThread.Abort();
                else
                    ;
            }
            else ;

            if ((m_listDbInterfaces.Count > 0) && (!(m_indxDbInterfaceConfigDB < 0)) && (!(m_listenerIdConfigDB < 0)))
            {
                m_listDbInterfaces[m_indxDbInterfaceConfigDB].ListenerUnregister(m_listenerIdConfigDB);
                m_indxDbInterfaceConfigDB = -1;
                m_listenerIdConfigDB = -1;

                foreach (TEC t in m_list_tec)
                {
                    for (CONN_SETT_TYPE connSettType = CONN_SETT_TYPE.ADMIN; connSettType < CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; connSettType ++) {
                        m_listDbInterfaces[t.m_arIndxDbInterfaces[(int)connSettType]].ListenerUnregister(t.m_arListenerIds[(int)connSettType]);
                    }
                }

                foreach (DbInterface dbi in m_listDbInterfaces)
                {
                    dbi.Stop ();
                }
            }
            else
                ;
        }

        private bool StateRequest(StatesMachine state)
        {
            bool result = true;
            switch (state)
            {
                case StatesMachine.CurrentTime:
                    ActionReport("��������� �������� ������� �������.");
                    GetCurrentTimeRequest();
                    break;
                case StatesMachine.PPBRValues:
                    ActionReport("��������� ������ �����.");
                    GetPPBRValuesRequest(allTECComponents[oldTecIndex].tec, allTECComponents[oldTecIndex], dateForValues.Date);
                    break;
                case StatesMachine.AdminValues:
                    ActionReport("��������� ���������������� ������.");
                    GetAdminValuesRequest(allTECComponents[oldTecIndex].tec, allTECComponents[oldTecIndex], dateForValues.Date);
                    this.BeginInvoke(delegateCalendarSetDate, m_prevDatetime);
                    break;
                case StatesMachine.RDGExcelValues:
                    ActionReport("������ ��� �� Excel.");
                    GetRDGExcelValuesRequest();
                    break;
                case StatesMachine.PPBRDates:
                    if (serverTime.Date > dateForValues.Date)
                    {
                        saveResult = Errors.InvalidValue;
                        try
                        {
                            semaSave.Release(1);
                        }
                        catch
                        {
                        }
                        result = false;
                        break;
                    }
                    ActionReport("��������� ������ ����������� ������� ��������.");
                    GetPPBRDatesRequest(dateForValues);
                    break;
                case StatesMachine.AdminDates:
                    if (serverTime.Date > dateForValues.Date)
                    {
                        saveResult = Errors.InvalidValue;
                        try
                        {
                            semaSave.Release(1);
                        }
                        catch
                        {
                        }
                        result = false;
                        break;
                    }
                    ActionReport("��������� ������ ����������� ������� ��������.");
                    GetAdminDatesRequest(dateForValues);
                    break;
                case StatesMachine.SaveAdminValues:
                    ActionReport("���������� ���������������� ������.");
                    SetAdminValuesRequest(allTECComponents[oldTecIndex].tec, allTECComponents[oldTecIndex], dateForValues);
                    break;
                case StatesMachine.SavePPBRValues:
                    ActionReport("���������� �����.");
                    SetPPBRRequest(allTECComponents[oldTecIndex].tec, allTECComponents[oldTecIndex], dateForValues);
                    break;
                //case StatesMachine.UpdateValuesPPBR:
                //    ActionReport("���������� �����.");
                //    break;
                case StatesMachine.GetPass:
                    ActionReport("��������� ������ " + getOwnerPass () + ".");

                    GetPassRequest(m_idPass);
                    break;
                case StatesMachine.SetPassInsert:
                    ActionReport("���������� ������ " + getOwnerPass() + ".");
                    
                    SetPassRequest(passReceive, m_idPass, true);
                    break;
                case StatesMachine.SetPassUpdate:
                    ActionReport("���������� ������ " + getOwnerPass() + ".");

                    SetPassRequest(passReceive, m_idPass, false);
                    break;
                //case StatesMachine.LayoutGet:
                //    ActionReport("��������� ���������������� ������ ������.");
                //    GetLayoutRequest(dateForValues);
                //    break;
                //case StatesMachine.LayoutSet:
                //    ActionReport("���������� ���������������� ������ ������.");
                //    SetLayoutRequest(dateForValues);
                //    break;
            }

            return result;
        }

        private bool StateCheckResponse(StatesMachine state, out bool error, out DataTable table)
        {
            bool bRes = false;
            
            if ((!(m_indxDbInterfaceCurrent < 0)) && (m_listListenerIdCurrent.Count > 0) && (! (m_indxDbInterfaceCurrent < 0)))
            {
                switch (state)
                {
                    case StatesMachine.RDGExcelValues:
                        error = false;
                        table = null;

                        bRes = true;
                        break;
                    case StatesMachine.CurrentTime:
                    case StatesMachine.PPBRValues:
                    case StatesMachine.AdminValues:
                    case StatesMachine.PPBRDates:
                    case StatesMachine.AdminDates:                    
                    case StatesMachine.SaveAdminValues:
                    case StatesMachine.SavePPBRValues:
                    //case StatesMachine.UpdateValuesPPBR:
                    case StatesMachine.GetPass:
                        bRes = GetResponse(m_indxDbInterfaceCurrent, m_listListenerIdCurrent[m_indxDbInterfaceCurrent], out error, out table/*, false*/);
                        break;
                    case StatesMachine.SetPassInsert:
                    case StatesMachine.SetPassUpdate:
                    //case StatesMachine.LayoutGet:
                    //case StatesMachine.LayoutSet:
                        bRes = GetResponse(m_indxDbInterfaceCurrent, m_listListenerIdCurrent[m_indxDbInterfaceCurrent], out error, out table/*, true*/);
                        break;
                    default:
                        error = true;
                        table = null;
                        
                        bRes = false;
                        break;
                }
            }
            else {
                //������???

                error = true;
                table = null;

                bRes = false;
            }

            return bRes;
        }

        private bool StateResponse(StatesMachine state, DataTable table)
        {
            bool result = false;
            switch (state)
            {
                case StatesMachine.CurrentTime:
                    result = GetCurrentTimeResponse(table);
                    if (result)
                    {
                        if (using_date) {
                            m_prevDatetime = serverTime.Date;
                            dateForValues = m_prevDatetime;
                        }
                        else
                            ;
                    }
                    else
                        ;
                    break;
                case StatesMachine.PPBRValues:
                    result = GetPPBRValuesResponse(table, dateForValues);
                    if (result)
                    {
                    }
                    else
                        ;
                    break;
                case StatesMachine.AdminValues:
                    result = GetAdminValuesResponse(table, dateForValues);
                    if (result)
                    {
                        this.BeginInvoke(delegateFillData, dateForValues);
                    }
                    else
                        ;
                    break;
                case StatesMachine.RDGExcelValues:
                    ActionReport("������ ��� �� Excel.");
                    //result = GetRDGExcelValuesResponse(table, dateForValues);
                    result = GetRDGExcelValuesResponse();
                    if (result)
                    {
                        this.BeginInvoke(delegateFillData, m_prevDatetime);
                    }
                    else
                        ;
                    break;
                case StatesMachine.PPBRDates:
                    ClearPPBRDates();
                    result = GetPPBRDatesResponse(table, dateForValues);
                    if (result)
                    {
                    }
                    else
                        ;
                    break;
                case StatesMachine.AdminDates:
                    ClearAdminDates();
                    result = GetAdminDatesResponse(table, dateForValues);
                    if (result)
                    {
                    }
                    break;
                case StatesMachine.SaveAdminValues:
                    saveResult = Errors.NoError;
                    //try { semaSave.Release(1); }
                    //catch { }
                    result = true;
                    if (result) { }
                    else ;
                    break;
                case StatesMachine.SavePPBRValues:
                    saveResult = Errors.NoError;
                    try
                    {
                        semaSave.Release(1);
                    }
                    catch
                    {
                    }
                    result = true;
                    if (result)
                    {
                    }
                    break;
                //case StatesMachine.UpdateValuesPPBR:
                //    saveResult = Errors.NoError;
                //    try
                //    {
                //        semaSave.Release(1);
                //    }
                //    catch
                //    {
                //    }
                //    result = true;
                //    if (result)
                //    {
                //    }
                //    break;
                case StatesMachine.GetPass:
                    result = GetPassResponse(table);
                    if (result)
                    {
                        passResult = Errors.NoError;
                        try
                        {
                            semaGetPass.Release(1);
                        }
                        catch
                        {
                        }
                    }
                    break;
                case StatesMachine.SetPassInsert:
                    passResult = Errors.NoError;
                    try
                    {
                        semaSetPass.Release(1);
                    }
                    catch
                    {
                    }
                    result = true;
                    if (result)
                    {
                    }
                    break;
                case StatesMachine.SetPassUpdate:
                    passResult = Errors.NoError;
                    try
                    {
                        semaSetPass.Release(1);
                    }
                    catch
                    {
                    }
                    result = true;
                    if (result)
                    {
                    }
                    break;
                //case StatesMachine.LayoutGet:
                //    result = GetLayoutResponse(table, dateForValues);
                //    if (result)
                //    {
                //        loadLayoutResult = Errors.NoError;
                //        try
                //        {
                //            semaLoadLayout.Release(1);
                //        }
                //        catch
                //        {
                //        }
                //    }
                //    break;
                //case StatesMachine.LayoutSet:
                //    loadLayoutResult = Errors.NoError;
                //    try
                //    {
                //        semaLoadLayout.Release(1);
                //    }
                //    catch
                //    {
                //    }
                //    result = true;
                //    if (result)
                //    {
                //    }
                //    break;
            }

            if (result)
                errored_state = actioned_state = false;

            return result;
        }

        private void StateErrors(StatesMachine state, bool response)
        {
            bool bClear = false;
            
            switch (state)
            {
                case StatesMachine.CurrentTime:
                    if (response)
                    {
                        ErrorReport("������ ������� �������� ������� �������. ������� � ��������.");
                        if (saving)
                            saveResult = Errors.ParseError;
                    }
                    else
                    {
                        ErrorReport("������ ��������� �������� ������� �������. ������� � ��������.");
                        if (saving)
                            saveResult = Errors.NoAccess;
                    }
                    if (saving)
                    {
                        try
                        {
                            semaSave.Release(1);
                        }
                        catch
                        {
                        }
                    }
                    break;
                case StatesMachine.PPBRValues:
                    if (response)
                        ErrorReport("������ ������� ������ �����. ������� � ��������.");
                    else {
                        ErrorReport("������ ��������� ������ �����. ������� � ��������.");

                        bClear = true;
                    }
                    break;
                case StatesMachine.AdminValues:
                    if (response)
                        ErrorReport("������ ������� ���������������� ������. ������� � ��������.");
                    else {
                        ErrorReport("������ ��������� ���������������� ������. ������� � ��������.");

                        bClear = true;
                    }
                    break;
                case StatesMachine.RDGExcelValues:
                    ErrorReport("������ ������� ��� �� ����� Excel. ������� � ��������.");

                    // ???
                    break;
                case StatesMachine.PPBRDates:
                    if (response)
                    {
                        ErrorReport("������ ������� ����������� ������� �������� (PPBR). ������� � ��������.");
                        saveResult = Errors.ParseError;
                    }
                    else
                    {
                        ErrorReport("������ ��������� ����������� ������� �������� (PPBR). ������� � ��������.");
                        saveResult = Errors.NoAccess;
                    }
                    try
                    {
                        semaSave.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                case StatesMachine.AdminDates:
                    if (response)
                    {
                        ErrorReport("������ ������� ����������� ������� �������� (AdminValues). ������� � ��������.");
                        saveResult = Errors.ParseError;
                    }
                    else
                    {
                        ErrorReport("������ ��������� ����������� ������� �������� (AdminValues). ������� � ��������.");
                        saveResult = Errors.NoAccess;
                    }
                    try
                    {
                        semaSave.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                case StatesMachine.SaveAdminValues:
                    ErrorReport("������ ���������� ���������������� ������. ������� � ��������.");
                    saveResult = Errors.NoAccess;
                    try
                    {
                        semaSave.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                case StatesMachine.SavePPBRValues:
                    ErrorReport("������ ���������� ������ �����. ������� � ��������.");
                    saveResult = Errors.NoAccess;
                    try
                    {
                        semaSave.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                //case StatesMachine.UpdateValuesPPBR:
                //    ErrorReport("������ ���������� ������ �����. ������� � ��������.");
                //    saveResult = Errors.NoAccess;
                //    try
                //    {
                //        semaSave.Release(1);
                //    }
                //    catch
                //    {
                //    }
                //    break;
                case StatesMachine.GetPass:
                    if (response)
                    {
                        ErrorReport("������ ������� ������ " + getOwnerPass() + ". ������� � ��������.");

                        passResult = Errors.ParseError;
                    }
                    else
                    {
                        ErrorReport("������ ��������� ������ " + getOwnerPass() + ". ������� � ��������.");

                        passResult = Errors.NoAccess;
                    }
                    try
                    {
                        semaGetPass.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                case StatesMachine.SetPassInsert:
                case StatesMachine.SetPassUpdate:
                    ErrorReport("������ ���������� ������ " + getOwnerPass() + ". ������� � ��������.");

                    passResult = Errors.NoAccess;
                    try
                    {
                        semaSetPass.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                //case StatesMachine.LayoutGet:
                //    if (response)
                //    {
                //        ErrorReport("������ ������� ���������������� ������ ������. ������� � ��������.");
                //        loadLayoutResult = Errors.ParseError;
                //    }
                //    else
                //    {
                //        ErrorReport("������ ��������� ���������������� ������ ������. ������� � ��������.");
                //        loadLayoutResult = Errors.ParseError;
                //    }
                //    try
                //    {
                //        semaLoadLayout.Release(1);
                //    }
                //    catch
                //    {
                //    }
                //    break;
                //case StatesMachine.LayoutSet:
                //    ErrorReport("������ ���������� ���������������� ������ ������. ������� � ��������.");
                //    loadLayoutResult = Errors.NoAccess;
                //    try
                //    {
                //        semaLoadLayout.Release(1);
                //    }
                //    catch
                //    {
                //    }
                //    break;
            }

            if (bClear) {
                ClearValues();
                ClearTables();
            }
            else
                ;
        }

        private void TecView_ThreadFunction(object data)
        {
            int index;
            StatesMachine currentState;

            while (threadIsWorking)
            {
                sem.WaitOne();

                index = 0;

                lock (lockValue)
                {
                    if (states.Count == 0)
                        continue;
                    currentState = states[index];
                    newState = false;
                }

                while (true)
                {
                    bool requestIsOk = true;
                    bool error = true;
                    bool dataPresent = false;
                    DataTable table = null;
                    for (int i = 0; i < DbInterface.MAX_RETRY && !dataPresent && !newState; i++)
                    {
                        if (error)
                        {
                            requestIsOk = StateRequest(currentState);
                            if (!requestIsOk)
                                break;
                            else
                                ;
                        }
                        else
                            ;

                        error = false;
                        for (int j = 0; j < DbInterface.MAX_WAIT_COUNT && !dataPresent && !error && !newState; j++)
                        {
                            System.Threading.Thread.Sleep(DbInterface.WAIT_TIME_MS);
                            dataPresent = StateCheckResponse(currentState, out error, out table);
                        }
                    }

                    if (requestIsOk)
                    {
                        bool responseIsOk = true;
                        if ((dataPresent == true) && (error == false) && (newState == false))
                            responseIsOk = StateResponse(currentState, table);
                        else
                            ;

                        if (((responseIsOk == false) || (dataPresent == false) || (error == true)) && (newState == false))
                        {
                            StateErrors(currentState, !responseIsOk);
                            lock (lockValue)
                            {
                                if (newState == false)
                                {
                                    states.Clear();
                                    break;
                                }
                                else
                                    ;
                            }
                        }
                        else
                            ;
                    }
                    else
                    {
                        lock (lockValue)
                        {
                            if (newState == false)
                            {
                                states.Clear();
                                break;
                            }
                            else
                                ;
                        }
                    }

                    index++;

                    lock (lockValue)
                    {
                        if (index == states.Count)
                            break;
                        else
                            ;

                        if (newState)
                            break;
                        else
                            ;
                        currentState = states[index];
                    }
                }
            }
            try
            {
                sem.Release(1);
            }
            catch
            {
            }
        }
    }
}
