using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Data.OleDb;
using System.IO;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Globalization;

namespace Statistic
{
    public class Admin : Panel
    {
        private delegate void DelegateFunctionDate(DateTime date);

        private struct OldValuesStruct
        {
            //public double plan;
            public double recomendation;
            public bool deviationType;
            public double deviation;
        }

        public struct GtpsAdminStruct
        {
            public double[] recommendations;
            public double[] plan;
            public double[] diviation;
            public bool[] diviationPercent;

            public GtpsAdminStruct(int count)
            {
                this.recommendations = new double[count];
                this.plan = new double[count];
                this.diviation = new double[count];
                this.diviationPercent = new bool[count];
            }
        }

        private struct TecPPBRValues
        {
            public double[] SN;
            public double[] PBR;
            public double[] Pmax;
            public double[] Pmin;

            public TecPPBRValues(int t)
            {
                this.SN = new double[25];
                this.PBR = new double[25];
                this.Pmax = new double[24];
                this.Pmin = new double[24];
            }
        }

        private struct LayoutData
        {
            public int number;
            public DateTime date;
            public int code;
            public int hour_start_in_db;
            public int hour_start;
            public int hour_end;
            public string name_in_db;
            public string name;
            //public TecPPBRValues BTEC;
            public TecPPBRValues BTEC_TG1;
            public TecPPBRValues BTEC_TG2;
            public TecPPBRValues BTEC_TG35;
            public TecPPBRValues BTEC_TG4;
            public TecPPBRValues TEC2;
            public TecPPBRValues TEC3_110;
            public TecPPBRValues TEC3_220;
            public TecPPBRValues TEC4;
            public TecPPBRValues TEC5_110;
            public TecPPBRValues TEC5_220;
            public bool[] existingHours;

            public LayoutData(int t)
            {
                this.number = 0;
                this.date = DateTime.Now;
                this.code = 0;
                this.hour_start_in_db = 0;
                this.hour_start = 0;
                this.hour_end = 0;
                this.name_in_db = "";
                this.name = "";
                //this.BTEC = new TecPPBRValues(1);
                this.BTEC_TG1 = new TecPPBRValues(1);
                this.BTEC_TG2 = new TecPPBRValues(1);
                this.BTEC_TG35 = new TecPPBRValues(1);
                this.BTEC_TG4 = new TecPPBRValues(1);
                this.TEC2 = new TecPPBRValues(1);
                this.TEC3_110 = new TecPPBRValues(1);
                this.TEC3_220 = new TecPPBRValues(1);
                this.TEC4 = new TecPPBRValues(1);
                this.TEC5_110 = new TecPPBRValues(1);
                this.TEC5_220 = new TecPPBRValues(1);
                this.existingHours = new bool[24];
            }

            public void ClearValues()
            {
                hour_start = 0;
                hour_end = 0;
                for (int i = 0; i < 24; i++)
                {
                    existingHours[i] = false;
                    //BTEC.PBR[i] = BTEC.Pmax[i] = BTEC.Pmin[i] = BTEC.SN[i] =
                    BTEC_TG1.PBR[i] = BTEC_TG1.Pmax[i] = BTEC_TG1.Pmin[i] = BTEC_TG1.SN[i] =
                    BTEC_TG2.PBR[i] = BTEC_TG2.Pmax[i] = BTEC_TG2.Pmin[i] = BTEC_TG2.SN[i] =
                    BTEC_TG35.PBR[i] = BTEC_TG35.Pmax[i] = BTEC_TG35.Pmin[i] = BTEC_TG35.SN[i] =
                    BTEC_TG4.PBR[i] = BTEC_TG4.Pmax[i] = BTEC_TG4.Pmin[i] = BTEC_TG4.SN[i] =
                    TEC2.PBR[i] = TEC2.Pmax[i] = TEC2.Pmin[i] = TEC2.SN[i] =
                    TEC3_110.PBR[i] = TEC3_110.Pmax[i] = TEC3_110.Pmin[i] = TEC3_110.SN[i] =
                    TEC3_220.PBR[i] = TEC3_220.Pmax[i] = TEC3_220.Pmin[i] = TEC3_220.SN[i] =
                    TEC4.PBR[i] = TEC4.Pmax[i] = TEC4.Pmin[i] = TEC4.SN[i] =
                    TEC5_110.PBR[i] = TEC5_110.Pmax[i] = TEC5_110.Pmin[i] = TEC5_110.SN[i] =
                    TEC5_220.PBR[i] = TEC5_220.Pmax[i] = TEC5_220.Pmin[i] = TEC5_220.SN[i] = 0.0;
                }
                //BTEC.PBR[24] = BTEC.SN[24] =
                BTEC_TG1.PBR[24] = BTEC_TG1.SN[24] =
                BTEC_TG2.PBR[24] = BTEC_TG2.SN[24] =
                BTEC_TG35.PBR[24] = BTEC_TG35.SN[24] =
                BTEC_TG4.PBR[24] = BTEC_TG4.SN[24] =
                TEC2.PBR[24] = TEC2.SN[24] =
                TEC3_110.PBR[24] = TEC3_110.SN[24] =
                TEC3_220.PBR[24] = TEC3_220.SN[24] =
                TEC4.PBR[24] = TEC4.SN[24] =
                TEC5_110.PBR[24] = TEC5_110.SN[24] =
                TEC5_220.PBR[24] = TEC5_220.SN[24] = 0.0;
            }
        }

        private System.Windows.Forms.MonthCalendar mcldrDate;
        private System.Windows.Forms.DataGridView dgwAdminTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn DateHour;
        private System.Windows.Forms.DataGridViewTextBoxColumn Plan;
        private System.Windows.Forms.DataGridViewTextBoxColumn Recommendation;
        private System.Windows.Forms.DataGridViewCheckBoxColumn DeviationType;
        private System.Windows.Forms.DataGridViewTextBoxColumn Deviation;
        private System.Windows.Forms.Button btnSet;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnLoadLayout;
        private System.Windows.Forms.DataGridViewButtonColumn ToAll;
        private System.Windows.Forms.ComboBox cbxTec;
        private System.Windows.Forms.GroupBox gbxDivider;
        private MD5CryptoServiceProvider md5;

        private DelegateFunc delegateStartWait;
        private DelegateFunc delegateStopWait;
        private DelegateFunc delegateEventUpdate;

        private DelegateFunctionDate delegateFillData;
        private DelegateFunctionDate delegateCalendarSetDate;
        
        private const double maxRecomendationValue = 1500;
        private const double maxDeviationValue = 1500;
        private const double maxDeviationPercentValue = 100;

        private const string dateHourStringIndex = "DateHour";
        private const string planStringIndex = "Plan";
        private const string recomendationStringIndex = "Recomendation";
        private const string deviationTypeStringIndex = "DeviationType";
        private const string deviationStringIndex = "Deviation";
        private const string toAllStringIndex = "ToAll";

        private const int dateHourIndex = 0;
        private const int planIndex = 1;
        private const int recomendationIndex = 2;
        private const int diviationTypeIndex = 3;
        private const int diviationIndex = 4;
        private const int toAllIndex = 5;
        
        private volatile OldValuesStruct[] oldValues;
        private GtpsAdminStruct values;
        private DateTime oldDate;
        private volatile List<GTP> allGtps;
        private volatile int oldTecIndex;
        private volatile List<TEC> tec;

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
        private volatile bool dispatcherPass;

        private Semaphore semaLoadLayout;
        private volatile Errors loadLayoutResult;
        private LayoutData layoutForLoading;
        
        private Object lockValue;

        private Thread taskThread;
        private Semaphore sem;
        private volatile bool threadIsWorking;
        private volatile bool newState;
        private volatile List<StatesMachine> states;

        public ConnectionSettings connSett;

        public volatile string m_strUsedAdminValues;
        public volatile string m_strUsedPPBRvsPBR;

        private DbInterface dbInterface;
        private int listenerIdTec;
        private int listenerIdAdmin;

        private enum StatesMachine
        {
            CurrentTime,
            AdminValues,
            AdminDates,
            SaveValues,
            GetPass,
            SetPassInsert,
            SetPassUpdate,
            LayoutGet,
            LayoutSet,
        }

        private enum StateActions
        {
            Request,
            Data,
        }

        private enum Errors
        { 
            NoError,
            InvalidValue,
            NoAccess,
            ParseError,
        }

        private volatile bool using_date;

        private bool[] adminDates;

        private bool started;

        public bool isActive;

        private void InitializeComponents()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnSet = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnLoadLayout = new System.Windows.Forms.Button();
            this.dgwAdminTable = new System.Windows.Forms.DataGridView();
            this.DateHour = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Plan = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Recommendation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DeviationType = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Deviation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ToAll = new System.Windows.Forms.DataGridViewButtonColumn();
            this.mcldrDate = new System.Windows.Forms.MonthCalendar();
            this.cbxTec = new System.Windows.Forms.ComboBox();
            this.gbxDivider = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).BeginInit();

            this.Controls.Add(this.btnSet);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnLoadLayout);
            this.Controls.Add(this.dgwAdminTable);
            this.Controls.Add(this.mcldrDate);
            this.Controls.Add(this.cbxTec);
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
            this.dgwAdminTable.AllowUserToAddRows = false;
            this.dgwAdminTable.AllowUserToDeleteRows = false;
            this.dgwAdminTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            dataGridViewCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgwAdminTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
            this.dgwAdminTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgwAdminTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DateHour,
            this.Plan,
            this.Recommendation,
            this.DeviationType,
            this.Deviation,
            this.ToAll});
            this.dgwAdminTable.Location = new System.Drawing.Point(170, 9);
            this.dgwAdminTable.Name = "dgwAdminTable";
            this.dgwAdminTable.RowHeadersVisible = false;
            this.dgwAdminTable.Size = new System.Drawing.Size(574, 591);
            this.dgwAdminTable.TabIndex = 1;
            this.dgwAdminTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellClick);
            this.dgwAdminTable.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellValidated);
            this.dgwAdminTable.RowTemplate.Resizable = DataGridViewTriState.False;
            // 
            // DateHour
            // 
            this.DateHour.Frozen = true;
            this.DateHour.HeaderText = "Дата, Час";
            this.DateHour.Name = dateHourStringIndex;
            this.DateHour.ReadOnly = true;
            this.DateHour.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Plan
            // 
            this.Plan.Frozen = true;
            this.Plan.HeaderText = "План";
            this.Plan.Name = planStringIndex;
            this.Plan.ReadOnly = true;
            this.Plan.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Plan.Width = 70;
            // 
            // Recommendation
            // 
            this.Recommendation.HeaderText = "Рекомендация";
            this.Recommendation.Name = recomendationStringIndex;
            this.Recommendation.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // DeviationType
            // 
            this.DeviationType.HeaderText = "Отклонение в процентах";
            this.DeviationType.Name = deviationTypeStringIndex;
            this.DeviationType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Deviation
            // 
            this.Deviation.HeaderText = "Величина максимального отклонения";
            this.Deviation.Name = deviationStringIndex;
            this.Deviation.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Deviation.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // btnSet
            // 
            this.btnSet.Location = new System.Drawing.Point(10, 204);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(154, 23);
            this.btnSet.TabIndex = 2;
            this.btnSet.Text = "Сохранить в базу";
            this.btnSet.UseVisualStyleBackColor = true;
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(10, 234);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(154, 23);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "Обновить из базы";
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
            this.btnLoadLayout.Location = new System.Drawing.Point(10, 282);
            this.btnLoadLayout.Name = "btnLoad";
            this.btnLoadLayout.Size = new System.Drawing.Size(154, 23);
            this.btnLoadLayout.TabIndex = 2;
            this.btnLoadLayout.Text = "Загрузить макет";
            this.btnLoadLayout.UseVisualStyleBackColor = true;
            this.btnLoadLayout.Click += new System.EventHandler(this.btnLoadLayout_Click);
            // 
            // ToAll
            // 
            this.ToAll.HeaderText = "Дозаполнить";
            this.ToAll.Name = toAllStringIndex;
            // 
            // cbxTec
            // 
            this.cbxTec.FormattingEnabled = true;
            this.cbxTec.Location = new System.Drawing.Point(10, 10);
            this.cbxTec.Name = "cbxTec";
            this.cbxTec.Size = new System.Drawing.Size(154, 21);
            this.cbxTec.TabIndex = 3;
            this.cbxTec.SelectionChangeCommitted += new System.EventHandler(this.cbxTec_SelectionChangeCommitted);
            this.cbxTec.DropDownStyle = ComboBoxStyle.DropDownList;
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).EndInit();
            this.ResumeLayout();
        }

        public Admin(List<TEC> tec, StatusStrip sts)
        {
            InitializeComponents();

            m_strUsedAdminValues = "";
            m_strUsedPPBRvsPBR = "";

            started = false;

            this.tec = tec;

            allGtps = new List<GTP>();
            oldValues = new OldValuesStruct[24];

            md5 = new MD5CryptoServiceProvider();

            is_data_error = is_connection_error = false;

            isActive = false;

            using_date = false;
            
            values = new GtpsAdminStruct(24);

            adminDates = new bool[24];

            layoutForLoading = new LayoutData(1);

            foreach (TEC t in tec)
            {
                if (t.GTP.Count > 1)
                    foreach (GTP g in t.GTP)
                    {
                        cbxTec.Items.Add(t.name + " - " + g.name);
                        allGtps.Add(g);
                    }
                else
                {
                    cbxTec.Items.Add(t.name);
                    allGtps.Add(t.GTP[0]);
                }
            }

            lockValue = new Object();

            semaSave = new Semaphore(1, 1);
            semaGetPass = new Semaphore(1, 1);
            semaSetPass = new Semaphore(1, 1);
            semaLoadLayout = new Semaphore(1, 1);

            delegateFillData = new DelegateFunctionDate(FillData);
            delegateCalendarSetDate = new DelegateFunctionDate(CalendarSetDate);

            stsStrip = sts;

            this.dgwAdminTable.Rows.Add(24);

            states = new List<StatesMachine>();
        }

        private bool WasChanged()
        {
            for (int i = 0; i < 24; i++)
            {
                if (oldValues[i].recomendation != values.recommendations[i] /*double.Parse(this.dgwAdminTable.Rows[i].Cells[recomendationIndex].Value.ToString())*/)
                    return true;
                if (oldValues[i].deviationType != values.diviationPercent[i] /*bool.Parse(this.dgwAdminTable.Rows[i].Cells[diviationTypeIndex].Value.ToString())*/)
                    return true;
                if (oldValues[i].deviation != values.diviation[i] /*double.Parse(this.dgwAdminTable.Rows[i].Cells[diviationIndex].Value.ToString())*/)
                    return true;
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
                dateForValues = oldDate;

                newState = true;
                states.Clear();
                states.Add(StatesMachine.CurrentTime);
                states.Add(StatesMachine.AdminDates);
                states.Add(StatesMachine.SaveValues);

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
                oldValues[i].recomendation = values.recommendations[i];
                oldValues[i].deviationType = values.diviationPercent[i];
                oldValues[i].deviation = values.diviation[i];
            }
        }

        private void FillData(DateTime date)
        {
            for (int i = 0; i < 24; i++)
            {
                this.dgwAdminTable.Rows[i].Cells[dateHourIndex].Value = date.AddHours(i + 1).ToString("yyyy-MM-dd HH");
                this.dgwAdminTable.Rows[i].Cells[planIndex].Value = values.plan[i].ToString("F2");
                this.dgwAdminTable.Rows[i].Cells[recomendationIndex].Value = values.recommendations[i].ToString("F2");
                this.dgwAdminTable.Rows[i].Cells[diviationTypeIndex].Value = values.diviationPercent[i].ToString();
                this.dgwAdminTable.Rows[i].Cells[diviationIndex].Value = values.diviation[i].ToString("F2");
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

            if (WasChanged())
            {
                result = MessageBox.Show(this, "Данные были изменены но не сохранялись.\nЕсли Вы хотите сохранить изменения, нажмите \"да\".\nЕсли Вы не хотите сохранять изменения, нажмите \"нет\".\nДля отмены действия нажмите \"отмена\".", "Внимание", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
            }
            else
                result = DialogResult.No;

            switch (result)
            {
                case DialogResult.Yes:
                    if ((resultSaving = SaveChanges()) == Errors.NoError)
                    {
                        lock (lockValue)
                        {
                            ClearValues();
                            ClearTables();
                            oldDate = e.Start;
                            dateForValues = oldDate;

                            newState = true;
                            states.Clear();
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
                            MessageBox.Show(this, "Изменение ретроспективы недопустимо!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        else
                            MessageBox.Show(this, "Не удалось сохранить изменения, возможно отсутствует связь с базой данных.", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        mcldrDate.SetDate(oldDate);
                    }
                    break;
                case DialogResult.No:
                    lock (lockValue)
                    {
                        ClearValues();
                        ClearTables();
                        oldDate = e.Start;
                        dateForValues = oldDate;

                        newState = true;
                        states.Clear();
                        states.Add(StatesMachine.AdminValues);

                        try
                        {
                            sem.Release(1);
                        }
                        catch
                        {
                        }
                    }
                    break;
                case DialogResult.Cancel:
                    mcldrDate.SetDate(oldDate);
                    break;
            }
        }

        private void cbxTec_SelectionChangeCommitted(object sender, EventArgs e)
        {
            DialogResult result;
            Errors resultSaving;

            if (WasChanged())
            {
                result = MessageBox.Show(this, "Данные были изменены но не сохранялись.\nЕсли Вы хотите сохранить изменения, нажмите \"да\".\nЕсли Вы не хотите сохранять изменения, нажмите \"нет\".\nДля отмены действия нажмите \"отмена\".", "Внимание", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
            }
            else
                result = DialogResult.No;

            switch (result)
            {
                case DialogResult.Yes:
                    if ((resultSaving = SaveChanges()) == Errors.NoError)
                    {
                        lock (lockValue)
                        {
                            ClearValues();
                            ClearTables();
                            oldTecIndex = cbxTec.SelectedIndex;
                            dateForValues = oldDate;
                            using_date = false;

                            newState = true;
                            states.Clear();
                            states.Add(StatesMachine.CurrentTime);
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
                            MessageBox.Show(this, "Изменение ретроспективы недопустимо!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        else
                            MessageBox.Show(this, "Не удалось сохранить изменения, возможно отсутствует связь с базой данных.", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        cbxTec.SelectedIndex = oldTecIndex;
                    }
                    break;
                case DialogResult.No:
                    lock (lockValue)
                    {
                        ClearValues();
                        ClearTables();
                        oldTecIndex = cbxTec.SelectedIndex;
                        dateForValues = oldDate;
                        using_date = false;

                        newState = true;
                        states.Clear();
                        states.Add(StatesMachine.CurrentTime);
                        states.Add(StatesMachine.AdminValues);

                        try
                        {
                            sem.Release(1);
                        }
                        catch
                        {
                        }
                    }
                    break;
                case DialogResult.Cancel:
                    cbxTec.SelectedIndex = oldTecIndex;
                    break;
            }
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
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
                    MessageBox.Show(this, "Изменение ретроспективы недопустимо!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                else
                    MessageBox.Show(this, "Не удалось сохранить изменения, возможно отсутствует связь с базой данных.", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        
        private bool ParseLayout(string filename)
        {
            StreamReader sr = new StreamReader(filename);
            string tmp_str, str = sr.ReadLine();
            char divider = ':';
            int pos1, pos2, tmp_int, hour;
            TecPPBRValues tecPPBRValues;
            double[] values;

            layoutForLoading.ClearValues();

            if (str == null)
            {
                sr.Close();
                return false;
            }

            if (str[0] != '/' && str[1] != '/')
            {
                sr.Close();
                return false;
            }

            pos1 = 2;
            pos2 = str.IndexOf(divider, pos1);
            if (pos2 <= 0)
            {
                sr.Close();
                return false;
            }
            if (!int.TryParse(str.Substring(pos1, pos2 - pos1), out layoutForLoading.number))
            {
                sr.Close();
                return false;
            }
            pos1 = pos2 + 1;
            pos2 = str.IndexOf(divider, pos1);
            if (pos2 <= 0)
            {
                sr.Close();
                return false;
            }
            if (!int.TryParse(str.Substring(pos1, pos2 - pos1), out tmp_int))
            {
                sr.Close();
                return false;
            }
            tmp_str = (tmp_int % 100).ToString() + "." + ((tmp_int / 100) % 100).ToString() + "." + ((tmp_int / 10000) % 100).ToString() + " 00:00:00";
            if (!DateTime.TryParse(tmp_str, out layoutForLoading.date))
            {
                sr.Close();
                return false;
            }
            pos1 = pos2 + 1;
            pos2 = str.IndexOf(divider, pos1);
            if (pos2 <= 0)
            {
                sr.Close();
                return false;
            }
            if (!int.TryParse(str.Substring(pos1, pos2 - pos1), out layoutForLoading.code))
            {
                sr.Close();
                return false;
            }
            pos1 = pos2 + 1;
            pos2 = str.IndexOf('-', pos1);
            if (pos2 > 0)
            {
                if (!int.TryParse(str.Substring(pos1, pos2 - pos1), out layoutForLoading.hour_start))
                {
                    sr.Close();
                    return false;
                }
                if (layoutForLoading.hour_start > 24)
                {
                    sr.Close();
                    return false;
                }

                pos1 = pos2 + 1;
                pos2 = str.IndexOf("++", pos1);
                if (pos2 <= 0)
                {
                    sr.Close();
                    return false;
                }

                layoutForLoading.name = "ПБР" + layoutForLoading.hour_start.ToString();
                layoutForLoading.hour_start--;
                layoutForLoading.hour_end = 24;//layoutForLoading.hour_start + 3;
            }
            else
            {
                pos2 = str.IndexOf("++", pos1);
                if (pos2 <= 0)
                {
                    sr.Close();
                    return false;
                }

                layoutForLoading.name = "ППБР";
                layoutForLoading.hour_start = 0;
                layoutForLoading.hour_end = 24;
            }

            while ((str = sr.ReadLine()) != null)
            {
                pos1 = 0;
                pos2 = str.IndexOf('(', pos1);
                if (pos2 < 0)
                {
                    sr.Close();
                    return false;
                }
                pos1 = pos2 + 1;
                pos2 = str.IndexOf(')', pos1);
                if (pos2 <= 0)
                {
                    sr.Close();
                    return false;
                }
                if (!int.TryParse(str.Substring(pos1, pos2 - pos1), out tmp_int))
                {
                    sr.Close();
                    return false;
                }
                switch (tmp_int % 100)
                { 
                    case 1: tecPPBRValues = layoutForLoading.TEC2; break;
                    case 2: tecPPBRValues = layoutForLoading.TEC3_110; break;
                    case 21: tecPPBRValues = layoutForLoading.TEC3_110; break;
                    case 22: tecPPBRValues = layoutForLoading.TEC3_220; break;
                    case 3: tecPPBRValues = layoutForLoading.TEC4; break;
                    case 4: tecPPBRValues = layoutForLoading.TEC5_110; break;
                    case 41: tecPPBRValues = layoutForLoading.TEC5_220; break;
                    case 42: tecPPBRValues = layoutForLoading.TEC5_110; break;
                    case 6: //HHH
                        //tecPPBRValues = layoutForLoading.BTEC;
                        tecPPBRValues = layoutForLoading.BTEC_TG1;
                        break;
                    default:
                        {
                            sr.Close();
                            return false;
                        }
                }
                if (tmp_int / 10000 > 0)
                    tmp_int /= 100;
                else
                    tmp_int /= 10;
                switch (tmp_int % 1000)
                {
                    case 100: values = tecPPBRValues.SN; break;
                    case 200: values = tecPPBRValues.PBR; break;
                    case 300: values = tecPPBRValues.Pmax; break;
                    case 400: values = tecPPBRValues.Pmin; break;
                    default:
                        {
                            sr.Close();
                            return false;
                        }
                }
                pos1 = pos2 + 2;

                hour = 0;
                while((pos2 = str.IndexOf(divider, pos1)) > 0)
                {
                    tmp_str = SetNumberSeparator(str.Substring(pos1, pos2 - pos1));
                    if (!double.TryParse(tmp_str, out values[hour++]))
                    {
                        sr.Close();
                        return false;
                    }
                    pos1 = pos2 + 1;
                }

                if ((pos2 = str.IndexOf("==", pos1)) > 0)
                {
                    tmp_str = SetNumberSeparator(str.Substring(pos1, pos2 - pos1));
                    if (!double.TryParse(tmp_str, out values[hour++]))
                    {
                        sr.Close();
                        return false;
                    }
                    pos1 = pos2 + 1;
                }
                else
                {
                    tmp_str = SetNumberSeparator(str.Substring(pos1, str.Length - pos1));
                    if (!double.TryParse(tmp_str, out values[hour++]))
                    {
                        sr.Close();
                        return false;
                    }
                    pos1 = pos2 + 1;
                }
            }
            sr.Close();
            return true;
        }

        private void btnLoadLayout_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (!ParseLayout(ofd.FileName))
                {
                    MessageBox.Show(this, "Неправильный формат макета. Загрузка невзможна.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                semaLoadLayout.WaitOne();
                lock (lockValue)
                {
                    loadLayoutResult = Errors.NoAccess;
                    using_date = false;
                    dateForValues = layoutForLoading.date.Date;

                    newState = true;
                    states.Clear();
                    states.Add(StatesMachine.CurrentTime);
                    states.Add(StatesMachine.LayoutGet);

                    try
                    {
                        sem.Release(1);
                    }
                    catch
                    {
                    }
                }
                
                delegateStartWait();
                semaLoadLayout.WaitOne();
                try
                {
                    semaLoadLayout.Release(1);
                }
                catch
                {
                }
                delegateStopWait();

                if (loadLayoutResult != Errors.NoError)
                {
                    MessageBox.Show(this, "Ошибка получения административных данных. Макет не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (serverTime.Date.CompareTo(layoutForLoading.date) > 0)
                {
                    MessageBox.Show(this, "Данные не относятся к текущим суткам. Макет не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (layoutForLoading.hour_start < layoutForLoading.hour_start_in_db)
                {
                    DialogResult result = MessageBox.Show(this, "В базе данных обнаружен более поздний макет " + layoutForLoading.name_in_db + ".\nПродолжить сохранение?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No)
                        return;
                }
                else
                {
                    if (serverTime.Date.CompareTo(layoutForLoading.date) == 0)
                    {
                        if (layoutForLoading.hour_start <= serverTime.Hour)
                        {
                            string hours = "";
                            for (int i = layoutForLoading.hour_start; i < serverTime.Hour; i++)
                                hours += (i + 1).ToString() + ", ";
                            hours += (serverTime.Hour + 1).ToString();

                            DialogResult result = MessageBox.Show(this, "Макет содержит данные о прошедших часах и текущем часе.\nЗаписать в базу значения для " + hours + " часов?\n", "Внимание", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                            if (result == DialogResult.Cancel)
                                return;
                            if (result == DialogResult.No)
                                layoutForLoading.hour_start = serverTime.Hour + 1;
                        }
                    }
                }

                if (layoutForLoading.hour_start >= layoutForLoading.hour_end)
                    return;

                delegateStartWait();
                semaLoadLayout.WaitOne();
                lock (lockValue)
                {
                    loadLayoutResult = Errors.NoAccess;

                    newState = true;
                    states.Clear();
                    states.Add(StatesMachine.LayoutSet);

                    try
                    {
                        sem.Release(1);
                    }
                    catch
                    {
                    }
                }
                
                semaLoadLayout.WaitOne();
                try
                {
                    semaLoadLayout.Release(1);
                }
                catch
                {
                }
                delegateStopWait();

                if (loadLayoutResult != Errors.NoError)
                {
                    MessageBox.Show(this, "Ошибка сохранения административных данных. Макет не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void dgwAdminTable_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            double value;
            bool valid;

            switch (e.ColumnIndex)
            {
                case recomendationIndex: // Рекомендация
                    {
                        valid = double.TryParse((string)this.dgwAdminTable.Rows[e.RowIndex].Cells[recomendationIndex].Value, out value);
                        if (!valid || value > maxRecomendationValue)
                        {
                            values.recommendations[e.RowIndex] = 0;
                            this.dgwAdminTable.Rows[e.RowIndex].Cells[recomendationIndex].Value = 0.ToString("F2");
                        }
                        else
                        {
                            values.recommendations[e.RowIndex] = value;
                            this.dgwAdminTable.Rows[e.RowIndex].Cells[recomendationIndex].Value = value.ToString("F2");
                        }
                        break;
                    }
                case diviationTypeIndex:
                    {
                        values.diviationPercent[e.RowIndex] = bool.Parse(this.dgwAdminTable.Rows[e.RowIndex].Cells[diviationTypeIndex].Value.ToString());
                        break;
                    }
                case diviationIndex: // Максимальное отклонение
                    {
                        valid = double.TryParse((string)this.dgwAdminTable.Rows[e.RowIndex].Cells[diviationIndex].Value, out value);
                        bool isPercent = bool.Parse(this.dgwAdminTable.Rows[e.RowIndex].Cells[diviationTypeIndex].Value.ToString());
                        double maxValue;
                        double recom = double.Parse((string)this.dgwAdminTable.Rows[e.RowIndex].Cells[recomendationIndex].Value);

                        if (isPercent)
                            maxValue = maxDeviationPercentValue;
                        else
                            maxValue = maxDeviationValue; // вообще эти значения не суммируются, но для максимальной границы нормально

                        if (!valid || value < 0 || value > maxValue)
                        {
                            values.diviation[e.RowIndex] = 0;
                            this.dgwAdminTable.Rows[e.RowIndex].Cells[diviationIndex].Value = 0.ToString("F2");
                        }
                        else
                        {
                            values.diviation[e.RowIndex] = value;
                            this.dgwAdminTable.Rows[e.RowIndex].Cells[diviationIndex].Value = value.ToString("F2");
                        }
                        break;
                    }
            }
        }

        private void dgwAdminTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == toAllIndex && e.RowIndex >= 0) // кнопка применение для всех
            {
                for (int i = e.RowIndex + 1; i < 24; i++)
                {
                    values.recommendations[i] = values.recommendations[e.RowIndex];
                    values.diviationPercent[i] = values.diviationPercent[e.RowIndex];
                    values.diviation[i] = values.diviation[e.RowIndex];

                    this.dgwAdminTable.Rows[i].Cells[recomendationIndex].Value = this.dgwAdminTable.Rows[e.RowIndex].Cells[recomendationIndex].Value;
                    this.dgwAdminTable.Rows[i].Cells[diviationTypeIndex].Value = this.dgwAdminTable.Rows[e.RowIndex].Cells[diviationTypeIndex].Value;
                    this.dgwAdminTable.Rows[i].Cells[diviationIndex].Value = this.dgwAdminTable.Rows[e.RowIndex].Cells[diviationIndex].Value;
                }
            }
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
                cbxTec.SelectedIndex = oldTecIndex;

                newState = true;
                states.Clear();
                states.Add(StatesMachine.CurrentTime);
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

            dbInterface.SetConnectionSettings(connSett);
            
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

            if (WasChanged())
            {
                result = MessageBox.Show(this, "Данные были изменены но не сохранялись.\nЕсли Вы хотите сохранить изменения, нажмите \"да\".\nЕсли Вы не хотите сохранять изменения, нажмите \"нет\".\nДля отмены действия нажмите \"отмена\".", "Внимание", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
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

        public bool SetPassword(string password, bool disp)
        {
            dispatcherPass = disp;

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
                if (dispatcherPass)
                    MessageBox.Show(this, "Ошибка получения пароля диспетчера. Пароль не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(this, "Ошибка получения пароля администратора. Пароль не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                if (dispatcherPass)
                    MessageBox.Show(this, "Ошибка сохранения пароля диспетчера. Пароль не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(this, "Ошибка сохранения пароля администратора. Пароль не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        public bool ComparePassword(string password, bool disp)
        {
            dispatcherPass = disp;

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
                if (dispatcherPass)
                    MessageBox.Show(this, "Ошибка получения пароля диспетчера.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(this, "Ошибка получения пароля администратора.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (passReceive == null)
            {
                if (password != "")
                {
                    MessageBox.Show(this, "Пароль введён неверно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else
                    return true;
            }
            else
            {
                if (password != "")
                    hashFromForm = hashedString.ToString();

                if (hashFromForm != passReceive)
                {
                    MessageBox.Show(this, "Пароль введён неверно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else
                    return true;
            }
        }

        public void ClearValues()
        { 
            for (int i = 0; i < 24; i++)
            {
                values.plan[i] = values.recommendations[i] = values.diviation[i] = 0;
                values.diviationPercent[i] = false;
            }
            FillOldValues();
        }

        public void ClearTables()
        {
            for (int i = 0; i < 24; i++)
            {
                this.dgwAdminTable.Rows[i].Cells[dateHourIndex].Value = "";
                this.dgwAdminTable.Rows[i].Cells[planIndex].Value = "";
                this.dgwAdminTable.Rows[i].Cells[recomendationIndex].Value = "";
                this.dgwAdminTable.Rows[i].Cells[diviationTypeIndex].Value = "false";
                this.dgwAdminTable.Rows[i].Cells[diviationIndex].Value = "";
            }
        }

        private void GetCurrentTimeRequest()
        {
            Request("SELECT now()", false);
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
                ErrorReport("Ошибка получения текущего времени сервера. Используется локальное время.");
            }

            return true;
        }

        private void GetAdminValuesRequest(TEC t, GTP gtp, DateTime date)
        {
            string select1 = "";
            string select2 = "";

            switch (t.name)
            {
                case "БТЭЦ":
                    select1 = "BTEC";
                    select2 = "PPBR_BTEC";
                    break;
                case "ТЭЦ-2":
                    select1 = "TEC2";
                    select2 = "PPBR_TEC2";
                    break;
                case "ТЭЦ-3":
                    select1 = "TEC3";
                    select2 = "PPBR_TEC3";
                    break;
                case "ТЭЦ-4":
                    select1 = "TEC4";
                    select2 = "PPBR_TEC4";
                    break;
                case "ТЭЦ-5":
                    select1 = "TEC5";
                    select2 = "PPBR_TEC5";
                    break;
                default:
                    break;
            }

            switch (gtp.name)
            {
                case "ГТП 110 кВ":
                    select1 += "_110";
                    select2 += "_110";
                    break;
                case "ГТП 220 кВ":
                    select1 += "_220";
                    select2 += "_220";
                    break;
                default:
                    break;
            }

            string request = @"SELECT " + m_strUsedAdminValues + ".DATE AS DATE1, " + m_strUsedAdminValues + "." + select1 + @"_REC, " + 
                             m_strUsedAdminValues + "." + select1 + @"_IS_PER, " + 
                             m_strUsedAdminValues + "." + select1 + @"_DIVIAT, " +
                             m_strUsedPPBRvsPBR + ".DATE_TIME AS DATE2, " + m_strUsedPPBRvsPBR + "." + select2 +
                             @" FROM " + m_strUsedAdminValues + " LEFT JOIN " + m_strUsedPPBRvsPBR + " ON " + m_strUsedAdminValues + ".DATE = " + m_strUsedPPBRvsPBR + ".DATE_TIME " +
                             @"WHERE " + m_strUsedAdminValues + ".DATE > '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
                             @"' AND " + m_strUsedAdminValues + ".DATE <= '" + date.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                             @"'" +
                             @" UNION " +
                             @"SELECT " + m_strUsedAdminValues + ".DATE AS DATE1, " + m_strUsedAdminValues + "." + select1 + @"_REC, " +
                             m_strUsedAdminValues + "." + select1 + @"_IS_PER, " +
                             m_strUsedAdminValues + "." + select1 + @"_DIVIAT, " +
                             m_strUsedPPBRvsPBR + ".DATE_TIME AS DATE2, PPBRVSPBR." + select2 +
                             @" FROM " + m_strUsedAdminValues + " RIGHT JOIN " + m_strUsedPPBRvsPBR + " ON " + m_strUsedAdminValues + ".DATE = " + m_strUsedPPBRvsPBR + ".DATE_TIME " +
                             @"WHERE " + m_strUsedPPBRvsPBR + ".DATE_TIME > '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
                             @"' AND " + m_strUsedPPBRvsPBR + ".DATE_TIME <= '" + date.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                             @"' AND MINUTE(" + m_strUsedPPBRvsPBR + ".DATE_TIME) = 0 AND " + m_strUsedAdminValues + ".DATE IS NULL ORDER BY DATE1, DATE2 ASC";


            Request(request, false);
//SELECT AdminValues.date as date1, AdminValues.BTEC_REC, AdminValues.BTEC_IS_PER, AdminValues.BTEC_DIVIAT, PPBRvsPBR.date_time as date2, PPBRvsPBR.PPBR_BTEC
//FROM AdminValues
//LEFT JOIN PPBRvsPBR ON AdminValues.date = PPBRvsPBR.date_time
//WHERE 
//AdminValues.date > '2009-12-08 00:00:00'
//AND AdminValues.date <= '2009-12-09 00:00:00'
//UNION
//SELECT AdminValues.date as date1, AdminValues.BTEC_REC, AdminValues.BTEC_IS_PER, AdminValues.BTEC_DIVIAT, PPBRvsPBR.date_time as date2, PPBRvsPBR.PPBR_BTEC
//FROM AdminValues
//RIGHT JOIN PPBRvsPBR ON AdminValues.date = PPBRvsPBR.date_time
//WHERE PPBRvsPBR.date_time > '2009-12-08 00:00:00'
//AND PPBRvsPBR.date_time <= '2009-12-09 00:00:00'
//AND MINUTE(PPBRvsPBR.date_time) = 0
//AND AdminValues.date is NULL
//ORDER BY date1, date2 ASC
        }

        private bool GetAdminValuesResponse(DataTable table, DateTime date)
        {
            for (int i = 0, hour; i < table.Rows.Count; i++)
            {
                if (table.Rows[i][0] is System.DBNull)
                {
                    try
                    {
                        hour = ((DateTime)table.Rows[i][4]).Hour;
                        if (hour == 0 && ((DateTime)table.Rows[i][4]).Day != date.Day)
                            hour = 24;
                        else
                            if (hour == 0)
                                continue;

                        values.plan[hour - 1] = (double)table.Rows[i][5];
                        values.recommendations[hour - 1] = 0;
                        values.diviationPercent[hour - 1] = false;
                        values.diviation[hour - 1] = 0;
                    }
                    catch
                    { 
                    }
                }
                else
                {
                    try
                    {
                        hour = ((DateTime)table.Rows[i][0]).Hour;
                        if (hour == 0 && ((DateTime)table.Rows[i][0]).Day != date.Day)
                            hour = 24;
                        else
                            if (hour == 0)
                                continue;

                        values.recommendations[hour - 1] = (double)table.Rows[i][1];
                        values.diviationPercent[hour - 1] = (int)table.Rows[i][2] == 1;
                        values.diviation[hour - 1] = (double)table.Rows[i][3];
                        if (!(table.Rows[i][4] is System.DBNull))
                            values.plan[hour - 1] = (double)table.Rows[i][5];
                        else
                            values.plan[hour - 1] = 0;
                    }
                    catch
                    {
                    }
                }
            }
            return true;
        }

        private void GetAdminDatesRequest(DateTime date)
        {
            string request;

            if (mcldrDate.SelectionStart.Date > date.Date)
            {
                date = mcldrDate.SelectionStart.Date;
            }

            request = @"SELECT DATE FROM " + m_strUsedAdminValues + " WHERE " + 
                      @"DATE > '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
                      @"' AND DATE <= '" + date.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                      @"' ORDER BY DATE ASC";

            Request(request, false);
        }

        private void ClearAdminDates()
        {
            for (int i = 0; i < 24; i++)
                adminDates[i] = false;
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
                    adminDates[hour - 1] = true;
                }
                catch
                {
                }
            }
            return true;
        }

        private void SetAdminValuesRequest(TEC t, GTP gtp, DateTime date)
        {
            int currentHour = serverTime.Hour;

            date = date.Date;

            if (serverTime.Date < date)
                currentHour = 0;

            string requestUpdate = "", requestInsert = "";

            string name = "";

            switch (t.name)
            {
                case "БТЭЦ":
                    name = "BTEC";
                    break;
                case "ТЭЦ-2":
                    name = "TEC2";
                    break;
                case "ТЭЦ-3":
                    name = "TEC3";
                    break;
                case "ТЭЦ-4":
                    name = "TEC4";
                    break;
                case "ТЭЦ-5":
                    name = "TEC5";
                    break;
                default:
                    break;
            }

            switch (gtp.name)
            {
                case "ГТП 110 кВ":
                    name += "_110";
                    break;
                case "ГТП 220 кВ":
                    name += "_220";
                    break;
                default:
                    break;
            }

            for (int i = 0; i < 24; i++)
            {
                // перематываем записи на текущий час
                if (i >= currentHour)
                {
                    // запись для этого часа имеется, модифицируем её
                    if (adminDates[i])
                    {
                        requestUpdate += @"UPDATE " + m_strUsedAdminValues + " SET " + name + @"_REC='" + values.recommendations[i].ToString("F2", CultureInfo.InvariantCulture) +
                                         @"', " + name + @"_IS_PER=" + (values.diviationPercent[i] ? "1" : "0") +
                                         @", " + name + "_DIVIAT='" + values.diviation[i].ToString("F2", CultureInfo.InvariantCulture) + 
                                         @"' WHERE " +
                                         @"DATE = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                         @"'; ";
                    }
                    else
                    {
                        // запись отсутствует, запоминаем значения
                        requestInsert += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                         @"', '" + values.recommendations[i].ToString("F2", CultureInfo.InvariantCulture) +
                                         @"', " + (values.diviationPercent[i] ? "1" : "0") +
                                         @", '" + values.diviation[i].ToString("F2", CultureInfo.InvariantCulture) + 
                                         @"'),";
                    }
                }
            }

            // добавляем все записи, не найденные в базе
            if (requestInsert != "")
            {
                requestInsert = @"INSERT INTO " + m_strUsedAdminValues + " (DATE, " + name + @"_REC" +
                                @", " + name + "_IS_PER" +
                                @", " + name + "_DIVIAT) VALUES" + requestInsert.Substring(0, requestInsert.Length - 1) + ";";
            }
            string requestDelete = @"DELETE FROM " + m_strUsedAdminValues + " WHERE " +
                                   @"BTEC_REC = 0 AND BTEC_IS_PER = 0 AND BTEC_DIVIAT = 0 AND " +
                                   @"TEC2_REC = 0 AND TEC2_IS_PER = 0 AND TEC2_DIVIAT = 0 AND " +
                                   @"TEC3_110_REC = 0 AND TEC3_110_IS_PER = 0 AND TEC3_110_DIVIAT = 0 AND " +
                                   @"TEC3_220_REC = 0 AND TEC3_220_IS_PER = 0 AND TEC3_220_DIVIAT = 0 AND " +
                                   @"TEC4_REC = 0 AND TEC4_IS_PER = 0 AND TEC4_DIVIAT = 0 AND " +
                                   @"TEC5_110_REC = 0 AND TEC5_110_IS_PER = 0 AND TEC5_110_DIVIAT = 0 AND " +
                                   @"TEC5_220_REC = 0 AND TEC5_220_IS_PER = 0 AND TEC5_220_DIVIAT = 0 AND " +
                                   @"DATE > '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
                                   @"' AND DATE <= '" + date.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss") + 
                                   @"';";

            Request(requestUpdate + requestInsert + requestDelete, false);
        }

        private void GetPassRequest(bool disp)
        {
            if (disp)
                Request("SELECT PASSWORD_DISP FROM TOOLS", false);
            else
                Request("SELECT PASSWORD_ADMIN FROM TOOLS", false);
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

        private void SetPassRequest(string password, bool disp, bool insert)
        {
            if (insert)
                if (disp)
                    Request("INSERT INTO TOOLS (PASSWORD_DISP) VALUES ('" + password + "')", false);
                else
                    Request("INSERT INTO TOOLS (PASSWORD_ADMIN) VALUES ('" + password + "')", false);
            else
                if (disp)
                    Request("UPDATE TOOLS SET PASSWORD_DISP='" + password + "'", false);
                else
                    Request("UPDATE TOOLS SET PASSWORD_ADMIN='" + password + "'", false);
        }

        private void GetLayoutRequest(DateTime date)
        {
            string request = @"SELECT " + m_strUsedPPBRvsPBR + ".DATE_TIME, " + m_strUsedPPBRvsPBR + ".PBR_NUMBER FROM " + m_strUsedPPBRvsPBR + " " +
                             @"WHERE " + m_strUsedPPBRvsPBR + ".DATE_TIME >= '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
                             @"' AND " + m_strUsedPPBRvsPBR + ".DATE_TIME <= '" + date.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                             @"' AND MINUTE(" + m_strUsedPPBRvsPBR + ".DATE_TIME) = 0 ORDER BY " + m_strUsedPPBRvsPBR + ".DATE_TIME ASC";
            Request(request, false);
        }

        private bool GetLayoutResponse(DataTable table, DateTime date)
        {
            int num = 0;
            layoutForLoading.hour_start_in_db = 0;
            layoutForLoading.name_in_db = "";
            string name;
            for (int i = 0, hour; i < table.Rows.Count; i++)
            {
                try
                {
                    hour = ((DateTime)table.Rows[i][0]).Hour;
                    if (hour == 0 && ((DateTime)table.Rows[i][0]).Day != date.Day)
                        hour = 24;
                    layoutForLoading.existingHours[hour - 1] = true;
                    name = (string)table.Rows[i][1];
                    switch(name)
                    {
                        case "ППБР": num = 0; break;
                        default:
                            {
                                if (name.Substring(0, 3) == "ПБР" && int.TryParse(name.Substring(3), out num) == true && num > 0)
                                    num--;
                                else
                                    num = 0;
                                break;
                            }
                    }

                    if (layoutForLoading.hour_start_in_db < num)
                    {
                        layoutForLoading.hour_start_in_db = num;
                        layoutForLoading.name_in_db = name;
                    }
                }
                catch
                {
                }
            }
            return true;
        }

        private void SetLayoutRequest(DateTime date)
        {
            string requestInsert = "";
            string requestUpdate = "";

            for (int i = layoutForLoading.hour_start; i < layoutForLoading.hour_end; i++)
            {
                // запись для этого часа имеется, модифицируем её
                if (layoutForLoading.existingHours[i])
                {
                    if (layoutForLoading.name == "ППБР")
                    {
                        requestUpdate += @"UPDATE " + m_strUsedPPBRvsPBR + " SET IS_COMDISP = 1, PBR_NUMBER = '" + layoutForLoading.name +
                                         @"', DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                         @"', WR_DATE_TIME = now()" +
                                         @", SN_TEC2 = '" + layoutForLoading.TEC2.SN[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TEC2 = '" + layoutForLoading.TEC2.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TEC2 = '" + layoutForLoading.TEC2.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TEC2 = '" + layoutForLoading.TEC2.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', SN_BTEC = '" + "0".ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_BTEC = '" + (layoutForLoading.BTEC_TG1.PBR[i] + layoutForLoading.BTEC_TG2.PBR[i] + layoutForLoading.BTEC_TG35.PBR[i] + layoutForLoading.BTEC_TG4.PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_BTEC = '" + (layoutForLoading.BTEC_TG1.Pmax[i] + layoutForLoading.BTEC_TG2.Pmax[i] + layoutForLoading.BTEC_TG35.Pmax[i] + layoutForLoading.BTEC_TG4.Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_BTEC = '" + (layoutForLoading.BTEC_TG1.Pmin[i] + layoutForLoading.BTEC_TG2.Pmin[i] + layoutForLoading.BTEC_TG35.Pmin[i] + layoutForLoading.BTEC_TG4.Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TG1_BTEC = '" + layoutForLoading.BTEC_TG1.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TG1_BTEC = '" + layoutForLoading.BTEC_TG1.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TG1_BTEC = '" + layoutForLoading.BTEC_TG1.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TG2_BTEC = '" + layoutForLoading.BTEC_TG2.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TG2_BTEC = '" + layoutForLoading.BTEC_TG2.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TG2_BTEC = '" + layoutForLoading.BTEC_TG2.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TG35_BTEC = '" + layoutForLoading.BTEC_TG35.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TG35_BTEC = '" + layoutForLoading.BTEC_TG35.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TG35_BTEC = '" + layoutForLoading.BTEC_TG35.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TG4_BTEC = '" + layoutForLoading.BTEC_TG4.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TG4_BTEC = '" + layoutForLoading.BTEC_TG4.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TG4_BTEC = '" + layoutForLoading.BTEC_TG4.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', SN_TEC4 = '" + layoutForLoading.TEC4.SN[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TEC4 = '" + layoutForLoading.TEC4.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TEC4 = '" + layoutForLoading.TEC4.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TEC4 = '" + layoutForLoading.TEC4.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', SN_TEC3 = '" + layoutForLoading.TEC3_110.SN[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TEC3 = '" + (layoutForLoading.TEC3_110.PBR[i] + layoutForLoading.TEC3_220.PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TEC3 = '" + (layoutForLoading.TEC3_110.Pmax[i] + layoutForLoading.TEC3_220.Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TEC3 = '" + (layoutForLoading.TEC3_110.Pmin[i] + layoutForLoading.TEC3_220.Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TEC3_110 = '" + layoutForLoading.TEC3_110.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TEC3_110 = '" + layoutForLoading.TEC3_110.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TEC3_110 = '" + layoutForLoading.TEC3_110.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TEC3_220 = '" + layoutForLoading.TEC3_220.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TEC3_220 = '" + layoutForLoading.TEC3_220.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TEC3_220 = '" + layoutForLoading.TEC3_220.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', SN_TEC5 = '" + layoutForLoading.TEC5_110.SN[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TEC5 = '" + (layoutForLoading.TEC5_110.PBR[i] + layoutForLoading.TEC5_220.PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TEC5 = '" + (layoutForLoading.TEC5_110.Pmax[i] + layoutForLoading.TEC5_220.Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TEC5 = '" + (layoutForLoading.TEC5_110.Pmin[i] + layoutForLoading.TEC5_220.Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TEC5_110 = '" + layoutForLoading.TEC5_110.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TEC5_110 = '" + layoutForLoading.TEC5_110.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TEC5_110 = '" + layoutForLoading.TEC5_110.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TEC5_220 = '" + layoutForLoading.TEC5_220.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TEC5_220 = '" + layoutForLoading.TEC5_220.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TEC5_220 = '" + layoutForLoading.TEC5_220.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"' WHERE " +
                                         @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                         @"'; ";
                    }
                    else {
                        requestUpdate += @"UPDATE " + m_strUsedPPBRvsPBR + " SET IS_COMDISP = 1, PBR_NUMBER = '" + layoutForLoading.name +
                                         @"', DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                         @"', WR_DATE_TIME = now()" +
                                         @", PPBR_TEC2 = '" + layoutForLoading.TEC2.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TEC2 = '" + layoutForLoading.TEC2.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TEC2 = '" + layoutForLoading.TEC2.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         //HHH
                                         @"', PPBR_BTEC = '" + (layoutForLoading.BTEC_TG1.PBR[i] + layoutForLoading.BTEC_TG2.PBR[i] + layoutForLoading.BTEC_TG35.PBR[i] + layoutForLoading.BTEC_TG4.PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_BTEC = '" + (layoutForLoading.BTEC_TG1.Pmax[i] + layoutForLoading.BTEC_TG2.Pmax[i] + layoutForLoading.BTEC_TG35.Pmax[i] + layoutForLoading.BTEC_TG4.Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_BTEC = '" + (layoutForLoading.BTEC_TG1.Pmin[i] + layoutForLoading.BTEC_TG2.Pmin[i] + layoutForLoading.BTEC_TG35.Pmin[i] + layoutForLoading.BTEC_TG4.Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TG1_BTEC = '" + layoutForLoading.BTEC_TG1.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TG1_BTEC = '" + layoutForLoading.BTEC_TG1.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TG1_BTEC = '" + layoutForLoading.BTEC_TG1.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TG2_BTEC = '" + layoutForLoading.BTEC_TG2.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TG2_BTEC = '" + layoutForLoading.BTEC_TG2.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TG2_BTEC = '" + layoutForLoading.BTEC_TG2.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TG35_BTEC = '" + layoutForLoading.BTEC_TG35.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TG35_BTEC = '" + layoutForLoading.BTEC_TG35.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TG35_BTEC = '" + layoutForLoading.BTEC_TG35.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TG4_BTEC = '" + layoutForLoading.BTEC_TG4.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TG4_BTEC = '" + layoutForLoading.BTEC_TG4.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TG4_BTEC = '" + layoutForLoading.BTEC_TG4.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         //HHH
                                         @"', PPBR_TEC4 = '" + layoutForLoading.TEC4.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TEC4 = '" + layoutForLoading.TEC4.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TEC4 = '" + layoutForLoading.TEC4.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TEC3 = '" + (layoutForLoading.TEC3_110.PBR[i] + layoutForLoading.TEC3_220.PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TEC3 = '" + (layoutForLoading.TEC3_110.Pmax[i] + layoutForLoading.TEC3_220.Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TEC3 = '" + (layoutForLoading.TEC3_110.Pmin[i] + layoutForLoading.TEC3_220.Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TEC3_110 = '" + layoutForLoading.TEC3_110.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TEC3_110 = '" + layoutForLoading.TEC3_110.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TEC3_110 = '" + layoutForLoading.TEC3_110.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TEC3_220 = '" + layoutForLoading.TEC3_220.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TEC3_220 = '" + layoutForLoading.TEC3_220.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TEC3_220 = '" + layoutForLoading.TEC3_220.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TEC5 = '" + (layoutForLoading.TEC5_110.PBR[i] + layoutForLoading.TEC5_220.PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TEC5 = '" + (layoutForLoading.TEC5_110.Pmax[i] + layoutForLoading.TEC5_220.Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TEC5 = '" + (layoutForLoading.TEC5_110.Pmin[i] + layoutForLoading.TEC5_220.Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TEC5_110 = '" + layoutForLoading.TEC5_110.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TEC5_110 = '" + layoutForLoading.TEC5_110.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TEC5_110 = '" + layoutForLoading.TEC5_110.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PPBR_TEC5_220 = '" + layoutForLoading.TEC5_220.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMAX_TEC5_220 = '" + layoutForLoading.TEC5_220.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', PMIN_TEC5_220 = '" + layoutForLoading.TEC5_220.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"' WHERE " +
                                         @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                         @"'; ";
                    }
                }
                else
                {
                    // запись отсутствует, запоминаем значения
                    requestInsert += @" (1, '" + layoutForLoading.name +
                                     @"', '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                     @"', now()" +
                                     @", '" + layoutForLoading.TEC2.SN[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC2.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC2.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC2.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     //HHH
                                     @"', '" + layoutForLoading.BTEC_TG1.SN[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.BTEC_TG1.PBR[i] + layoutForLoading.BTEC_TG2.PBR[i] + layoutForLoading.BTEC_TG35.PBR[i] + layoutForLoading.BTEC_TG4.PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.BTEC_TG1.Pmax[i] + layoutForLoading.BTEC_TG2.Pmax[i] + layoutForLoading.BTEC_TG35.Pmax[i] + layoutForLoading.BTEC_TG4.Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.BTEC_TG1.Pmin[i] + layoutForLoading.BTEC_TG2.Pmin[i] + layoutForLoading.BTEC_TG35.Pmin[i] + layoutForLoading.BTEC_TG4.Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.BTEC_TG1.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.BTEC_TG1.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.BTEC_TG1.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.BTEC_TG2.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.BTEC_TG2.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.BTEC_TG2.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.BTEC_TG35.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.BTEC_TG35.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.BTEC_TG35.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.BTEC_TG4.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.BTEC_TG4.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.BTEC_TG4.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     //HHH
                                     @"', '" + layoutForLoading.TEC4.SN[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC4.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC4.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC4.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC3_110.SN[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.TEC3_110.PBR[i] + layoutForLoading.TEC3_220.PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.TEC3_110.Pmax[i] + layoutForLoading.TEC3_220.Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.TEC3_110.Pmin[i] + layoutForLoading.TEC3_220.Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC3_110.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC3_110.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC3_110.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC3_220.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC3_220.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC3_220.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC5_110.SN[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.TEC5_110.PBR[i] + layoutForLoading.TEC5_220.PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.TEC5_110.Pmax[i] + layoutForLoading.TEC5_220.Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.TEC5_110.Pmin[i] + layoutForLoading.TEC5_220.Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC5_110.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC5_110.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC5_110.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC5_220.PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC5_220.Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.TEC5_220.Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"'),";
                }
            }

            if (requestInsert != "")
            {
                requestInsert = @"INSERT INTO " + m_strUsedPPBRvsPBR + " (IS_COMDISP, PBR_NUMBER, DATE_TIME, WR_DATE_TIME, " +
                                @"SN_TEC2, PPBR_TEC2, PMAX_TEC2, PMIN_TEC2, " +
                                @"SN_BTEC, PPBR_BTEC, PMAX_BTEC, PMIN_BTEC, " +
                                @"SN_TEC4, PPBR_TEC4, PMAX_TEC4, PMIN_TEC4, " +
                                @"SN_TEC3, PPBR_TEC3, PMAX_TEC3, PMIN_TEC3, " +
                                @"PPBR_TEC3_110, PMAX_TEC3_110, PMIN_TEC3_110, " +
                                @"PPBR_TEC3_220, PMAX_TEC3_220, PMIN_TEC3_220, " +
                                @"SN_TEC5, PPBR_TEC5, PMAX_TEC5, PMIN_TEC5, " +
                                @"PPBR_TEC5_110, PMAX_TEC5_110, PMIN_TEC5_110, " +
                                @"PPBR_TEC5_220, PMAX_TEC5_220, PMIN_TEC5_220" +
                                @") VALUES" + requestInsert.Substring(0, requestInsert.Length - 1) + ";";
            }

            Request(requestInsert + requestUpdate, false);
        }

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

        public void Activate(bool active)
        {
            isActive = active;
        }

        public void Request(string request, bool tec)
        {
            if (tec)
                dbInterface.Request(listenerIdTec, request);
            else
                dbInterface.Request(listenerIdAdmin, request);
        }

        public bool GetResponse(out bool error, out DataTable table, bool tec)
        {
            if (tec)
                return dbInterface.GetResponse(listenerIdTec, out error, out table);
            else
                return dbInterface.GetResponse(listenerIdAdmin, out error, out table);
        }

        public void StartDbInterface()
        {
            //dbInterface = new DbInterface(DbInterface.DbInterfaceType.MSSQL, 2);
            dbInterface = new DbInterface(DbInterface.DbInterfaceType.MySQL, 2);
            listenerIdTec = dbInterface.ListenerRegister();
            listenerIdAdmin = dbInterface.ListenerRegister();
            dbInterface.Start();

            dbInterface.SetConnectionSettings(connSett);

            threadIsWorking = true;

            taskThread = new Thread(new ParameterizedThreadStart(TecView_ThreadFunction));
            taskThread.Name = "Интерфейс к данным";
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
                try
                {
                    sem.Release(1);
                }
                catch
                {
                }

                joined = taskThread.Join(1000);
                if (!joined)
                    taskThread.Abort();
            }

            dbInterface.Stop();
            dbInterface.ListenerUnregister(listenerIdTec);
            dbInterface.ListenerUnregister(listenerIdAdmin);
        }

        private bool StateRequest(StatesMachine state)
        {
            bool result = true;
            switch (state)
            {
                case StatesMachine.CurrentTime:
                    ActionReport("Получение текущего времени сервера.");
                    GetCurrentTimeRequest();
                    break;
                case StatesMachine.AdminValues:
                    ActionReport("Получение административных данных.");
                    GetAdminValuesRequest(allGtps[oldTecIndex].tec, allGtps[oldTecIndex], dateForValues.Date);
                    oldDate = dateForValues.Date;
                    this.BeginInvoke(delegateCalendarSetDate, oldDate);
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
                    ActionReport("Получение списка сохранённых часовых значений.");
                    GetAdminDatesRequest(dateForValues);
                    break;
                case StatesMachine.SaveValues:
                    ActionReport("Сохранение административных данных.");
                    SetAdminValuesRequest(allGtps[oldTecIndex].tec, allGtps[oldTecIndex], dateForValues);
                    break;
                case StatesMachine.GetPass:
                    if (dispatcherPass)
                        ActionReport("Получение пароля диспетчера.");
                    else
                        ActionReport("Получение пароля администратора.");
                    GetPassRequest(dispatcherPass);
                    break;
                case StatesMachine.SetPassInsert:
                    if (dispatcherPass)
                        ActionReport("Сохранение пароля диспетчера.");
                    else
                        ActionReport("Сохранение пароля администратора.");
                    SetPassRequest(passReceive, dispatcherPass, true);
                    break;
                case StatesMachine.SetPassUpdate:
                    if (dispatcherPass)
                        ActionReport("Сохранение пароля диспетчера.");
                    else
                        ActionReport("Сохранение пароля администратора.");
                    SetPassRequest(passReceive, dispatcherPass, false);
                    break;
                case StatesMachine.LayoutGet:
                    ActionReport("Получение административных данных макета.");
                    GetLayoutRequest(dateForValues);
                    break;
                case StatesMachine.LayoutSet:
                    ActionReport("Сохранение административных данных макета.");
                    SetLayoutRequest(dateForValues);
                    break;
            }

            return result;
        }

        private bool StateCheckResponse(StatesMachine state, out bool error, out DataTable table)
        {
            switch (state)
            {
                case StatesMachine.CurrentTime:
                case StatesMachine.AdminValues:
                case StatesMachine.AdminDates:
                case StatesMachine.SaveValues:
                case StatesMachine.GetPass:
                    return GetResponse(out error, out table, false);
                case StatesMachine.SetPassInsert:
                case StatesMachine.SetPassUpdate:
                case StatesMachine.LayoutGet:
                case StatesMachine.LayoutSet:
                default:
                    return GetResponse(out error, out table, true);
            }

            error = true;
            table = null;
            return false;
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
                        if (using_date)
                            dateForValues = serverTime;
                    }
                    break;
                case StatesMachine.AdminValues:
                    result = GetAdminValuesResponse(table, dateForValues);
                    if (result)
                    {
                        this.BeginInvoke(delegateFillData, oldDate);
                    }
                    break;
                case StatesMachine.AdminDates:
                    ClearAdminDates();
                    result = GetAdminDatesResponse(table, dateForValues);
                    if (result)
                    {
                    }
                    break;
                case StatesMachine.SaveValues:
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
                case StatesMachine.LayoutGet:
                    result = GetLayoutResponse(table, dateForValues);
                    if (result)
                    {
                        loadLayoutResult = Errors.NoError;
                        try
                        {
                            semaLoadLayout.Release(1);
                        }
                        catch
                        {
                        }
                    }
                    break;
                case StatesMachine.LayoutSet:
                    loadLayoutResult = Errors.NoError;
                    try
                    {
                        semaLoadLayout.Release(1);
                    }
                    catch
                    {
                    }
                    result = true;
                    if (result)
                    {
                    }
                    break;
            }

            if (result)
                errored_state = actioned_state = false;

            return result;
        }

        private void StateErrors(StatesMachine state, bool response)
        {
            switch (state)
            {
                case StatesMachine.CurrentTime:
                    if (response)
                    {
                        ErrorReport("Ошибка разбора текущего времени сервера. Переход в ожидание.");
                        if (saving)
                            saveResult = Errors.ParseError;
                    }
                    else
                    {
                        ErrorReport("Ошибка получения текущего времени сервера. Переход в ожидание.");
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
                case StatesMachine.AdminValues:
                    if (response)
                        ErrorReport("Ошибка разбора административных данных. Переход в ожидание.");
                    else
                        ErrorReport("Ошибка получения административных данных. Переход в ожидание.");
                    break;
                case StatesMachine.AdminDates:
                    if (response)
                    {
                        ErrorReport("Ошибка разбора сохранённых часовых значений. Переход в ожидание.");
                        saveResult = Errors.ParseError;
                    }
                    else
                    {
                        ErrorReport("Ошибка получения сохранённых часовых значений. Переход в ожидание.");
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
                case StatesMachine.SaveValues:
                    ErrorReport("Ошибка сохранения административных данных. Переход в ожидание.");
                    saveResult = Errors.NoAccess;
                    try
                    {
                        semaSave.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                case StatesMachine.GetPass:
                    if (response)
                    {
                        if (dispatcherPass)
                            ErrorReport("Ошибка разбора пароля диспетчера. Переход в ожидание.");
                        else
                            ErrorReport("Ошибка разбора пароля администратора. Переход в ожидание.");
                        passResult = Errors.ParseError;
                    }
                    else
                    {
                        if (dispatcherPass)
                            ErrorReport("Ошибка получения пароля диспетчера. Переход в ожидание.");
                        else
                            ErrorReport("Ошибка получения пароля администратора. Переход в ожидание.");
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
                    if (dispatcherPass)
                        ErrorReport("Ошибка сохранения пароля диспетчера. Переход в ожидание.");
                    else
                        ErrorReport("Ошибка сохранения пароля администратора. Переход в ожидание.");
                    passResult = Errors.NoAccess;
                    try
                    {
                        semaSetPass.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                case StatesMachine.LayoutGet:
                    if (response)
                    {
                        ErrorReport("Ошибка разбора административных данных макета. Переход в ожидание.");
                        loadLayoutResult = Errors.ParseError;
                    }
                    else
                    {
                        ErrorReport("Ошибка получения административных данных макета. Переход в ожидание.");
                        loadLayoutResult = Errors.ParseError;
                    }
                    try
                    {
                        semaLoadLayout.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                case StatesMachine.LayoutSet:
                    ErrorReport("Ошибка сохранения административных данных макета. Переход в ожидание.");
                    loadLayoutResult = Errors.NoAccess;
                    try
                    {
                        semaLoadLayout.Release(1);
                    }
                    catch
                    {
                    }
                    break;
            }
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
                    for (int i = 0; i < MainForm.MAX_RETRY && !dataPresent && !newState; i++)
                    {
                        if (error)
                        {
                            requestIsOk = StateRequest(currentState);
                            if (!requestIsOk)
                                break;
                        }

                        error = false;
                        for (int j = 0; j < MainForm.MAX_WAIT_COUNT && !dataPresent && !error && !newState; j++)
                        {
                            System.Threading.Thread.Sleep(MainForm.WAIT_TIME_MS);
                            dataPresent = StateCheckResponse(currentState, out error, out table);
                        }
                    }

                    if (requestIsOk)
                    {
                        bool responseIsOk = true;
                        if (dataPresent && !error && !newState)
                            responseIsOk = StateResponse(currentState, table);

                        if ((!responseIsOk || !dataPresent || error) && !newState)
                        {
                            StateErrors(currentState, !responseIsOk);
                            lock (lockValue)
                            {
                                if (!newState)
                                {
                                    states.Clear();
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        lock (lockValue)
                        {
                            if (!newState)
                            {
                                states.Clear();
                                break;
                            }
                        }
                    }

                    index++;

                    lock (lockValue)
                    {
                        if (index == states.Count)
                            break;
                        if (newState)
                            break;
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
