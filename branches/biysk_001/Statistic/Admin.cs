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
            //public double[] SN;
            public double[] PBR;
            public double[] Pmax;
            public double[] Pmin;

            public TecPPBRValues(int t)
            {
                //this.SN = new double[25];
                this.PBR = new double[25];
                this.Pmax = new double[24];
                this.Pmin = new double[24];
            }
        }

        public enum INDEX_TEC_PBR_VALUES {BTEC_TG1, BTEC_TG2, BTEC_TG35, BTEC_TG4, TEC2, TEC3_TG1, TEC3_TG712, TEC3_TG5, TEC3_TG1314, TEC4_TG3, TEC4_TG48, TEC5_TG12, TEC5_TG36,
                                            COUNT_TEC_PBR_VALUES};
        
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
            public TecPPBRValues[] m_arGTPs;
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
                this.m_arGTPs = new TecPPBRValues[(int) INDEX_TEC_PBR_VALUES.COUNT_TEC_PBR_VALUES];
                this.existingHours = new bool[24];
            }

            public void ClearValues()
            {
                hour_start = 0;
                hour_end = 0;
                for (int i = 0; i < (int) INDEX_TEC_PBR_VALUES.COUNT_TEC_PBR_VALUES; i++)
                {
                    for (int j = 0; j < 24; j++)
                    {
                        existingHours[j] = false;
                        m_arGTPs[i].PBR[j] = m_arGTPs[i].Pmax[j] = m_arGTPs[i].Pmin[j] = 0.0;
                    }
                    m_arGTPs [i].PBR[24] = 0.0;
                }
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
        //private System.Windows.Forms.Button btnLoadLayout;
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

        //Для особенной ТЭЦ (Бийск)
        public ConnectionSettings connSettBiysk;
        public MySqlConnection connection;
        public MySqlCommand command;
        public MySqlDataAdapter adapter;

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

        private Semaphore semaMain;

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

        public ConnectionSettings connSettCommon;

        public volatile string m_strUsedAdminValues;
        public volatile string m_strUsedPPBRvsPBR;

        private DbInterface dbInterface;
        private int listenerIdTec;
        private int listenerIdAdmin;

        private enum StatesMachine
        {
            CurrentTime,
            AdminValues, //Получение административных данных
            AdminDates, //Получение списка сохранённых часовых значений
            PPBRDates, 
            SaveValues, //Сохранение административных данных
            SaveValuesPPBR, //Сохранение PPBR
            //UpdateValuesPPBR, //Обновление PPBR после 'SaveValuesPPBR'
            GetPass,
            SetPassInsert,
            SetPassUpdate,
            LayoutGet,
            LayoutSet,
        }

        //Для особенной ТЭЦ (Бийск)
        private System.Threading.Timer timer;

        private enum StatesMachineBiysk
        {
            Idle,
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

        private enum StateActionsBiysk
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
        private bool[] PPBRDates;

        private volatile StatesMachineBiysk state;
        private volatile StatesMachineBiysk nextState;
        private volatile StateActionsBiysk action;
        private volatile DbDataInterface dataInterface;
        private int tryes;
        private int wait_times;
        private const int MAX_TRYES = 1;
        private const int RETRY_TIME = 500;
        private const int WAIT_TIME = 50;
        private int MAX_WAIT_TIME = 1000; // сделать защиту от залипания когда нет данных и не выставлен признак ошибки

        private Thread dbThreadBiysk;
        private Semaphore semaDBThreadBiysk;
        private volatile bool workDBTreadBiysk;

        private bool started;

        public bool isActive;

        private void InitializeComponents()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnSet = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            //this.btnLoadLayout = new System.Windows.Forms.Button();
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
            //this.Controls.Add(this.btnLoadLayout);
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
            //this.btnLoadLayout.Location = new System.Drawing.Point(10, 282);
            //this.btnLoadLayout.Name = "btnLoad";
            //this.btnLoadLayout.Size = new System.Drawing.Size(154, 23);
            //this.btnLoadLayout.TabIndex = 2;
            //this.btnLoadLayout.Text = "Загрузить макет";
            //this.btnLoadLayout.UseVisualStyleBackColor = true;
            //this.btnLoadLayout.Click += new System.EventHandler(this.btnLoadLayout_Click);
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

            m_strUsedAdminValues = "AdminValuesNew";
            m_strUsedPPBRvsPBR = "PPBRvsPBRnew";

            started = false;

            this.tec = tec;

            allGtps = new List<GTP>();
            oldValues = new OldValuesStruct[24];

            md5 = new MD5CryptoServiceProvider();

            //Для особенной ТЭЦ (Бийск)
            connection = new MySqlConnection();

            command = new MySqlCommand();
            command.Connection = connection;
            command.CommandType = CommandType.Text;

            adapter = new MySqlDataAdapter();
            adapter.SelectCommand = command;

            is_data_error = is_connection_error = false;

            isActive = false;

            using_date = false;
            
            values = new GtpsAdminStruct(24);

            adminDates = new bool[24];
            PPBRDates = new bool[24];

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

            semaMain = new Semaphore(1, 1);
            
            semaSave = new Semaphore(1, 1);
            semaGetPass = new Semaphore(1, 1);
            semaSetPass = new Semaphore(1, 1);
            semaLoadLayout = new Semaphore(1, 1);            

            //Для особенной ТЭЦ (Бийск) поток чтения административных данных
            semaDBThreadBiysk = new Semaphore(1, 1);
            dbThreadBiysk = new Thread(new ParameterizedThreadStart(dbThread_Function));

            delegateFillData = new DelegateFunctionDate(FillData);
            delegateCalendarSetDate = new DelegateFunctionDate(CalendarSetDate);

            dataInterface = new DbDataInterface();
            stsStrip = sts;

            state = StatesMachineBiysk.Idle;
            nextState = StatesMachineBiysk.Idle;
            action = StateActionsBiysk.Request;

            TimerCallback timerCallback = new TimerCallback(timer_Tick);
            timer = new System.Threading.Timer(timerCallback, null, 0, Timeout.Infinite);

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
                //??? Состояния позволяют НАЧать процесс разработки возможности редактирования ПЛАНа на вкладке 'Редактирование ПБР'
                //states.Add(StatesMachine.PPBRDates);
                states.Add(StatesMachine.SaveValues);
                //states.Add(StatesMachine.SaveValuesPPBR);
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
                if (pos2 <= 0) {
                    sr.Close();
                    return false;
                }

                layoutForLoading.name = "ПБР" + layoutForLoading.hour_start.ToString();
                layoutForLoading.hour_start--;
                layoutForLoading.hour_end = 24;//layoutForLoading.hour_start + 3;
            }
            else {
                pos2 = str.IndexOf("++", pos1);
                if (pos2 <= 0) {
                    sr.Close();
                    return false;
                }

                layoutForLoading.name = "ППБР";
                layoutForLoading.hour_start = 0;
                layoutForLoading.hour_end = 24;
            }

            while ((str = sr.ReadLine()) != null) {
                pos1 = 0;
                pos2 = str.IndexOf('(', pos1);
                if (pos2 < 0)
                {
                    sr.Close();
                    return false;
                }
                pos1 = pos2 + 1;
                pos2 = str.IndexOf(')', pos1);
                if (pos2 <= 0) {
                    sr.Close();
                    return false;
                }
                if (!int.TryParse(str.Substring(pos1, pos2 - pos1), out tmp_int)) {
                    sr.Close();
                    return false;
                }
                switch (tmp_int % 100) { 
                    case 1: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2]; break;
                    case 2: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1]; break;
                    case 21: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1]; break;
                    case 22: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5]; break;
                    case 23: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712]; break;
                    case 24: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314]; break;
                    case 3: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3]; break;
                    case 31: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3]; break;
                    case 32: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48]; break;
                    case 4: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12]; break;
                    case 41: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12]; break;
                    case 42: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36]; break;
                    case 6: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1]; break;
                    case 61: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1]; break;
                    case 62: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2]; break;
                    case 63: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35]; break;
                    case 64: tecPPBRValues = layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4]; break;
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
                    case 100: /*values = tecPPBRValues.SN; break;*/
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
                while((pos2 = str.IndexOf(divider, pos1)) > 0) {
                    tmp_str = SetNumberSeparator(str.Substring(pos1, pos2 - pos1));
                    if (! double.TryParse(tmp_str, out values[hour++])) {
                        sr.Close();
                        return false;
                    }
                    pos1 = pos2 + 1;
                }

                if ((pos2 = str.IndexOf("==", pos1)) > 0) {
                    tmp_str = SetNumberSeparator(str.Substring(pos1, pos2 - pos1));
                    if (! double.TryParse(tmp_str, out values[hour++])) {
                        sr.Close();
                        return false;
                    }
                    pos1 = pos2 + 1;
                }
                else {
                    tmp_str = SetNumberSeparator(str.Substring(pos1, str.Length - pos1));
                    if (!double.TryParse(tmp_str, out values[hour++])) {
                        sr.Close();
                        return false;
                    }
                    pos1 = pos2 + 1;
                }
            }
            sr.Close();
            return true;
        }

        private void btnLoadLayout_Click(object sender, EventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK) {
                if (!ParseLayout(ofd.FileName)) {
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

            semaMain.WaitOne();
            {
                oldTecIndex = 0;
                using_date = true;
                cbxTec.SelectedIndex = oldTecIndex;
                state = StatesMachineBiysk.CurrentTime;
                action = StateActionsBiysk.Request;
                nextState = StatesMachineBiysk.AdminValues;
                tryes = 0;
                wait_times = 0;
                timer.Change(0, Timeout.Infinite);
            }
            semaMain.Release(1);
        }

        public void Reinit()
        {
            if (!started)
                return;

            dbInterface.SetConnectionSettings(connSettCommon);
            
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

            semaMain.WaitOne();
            {
                dateForValues = mcldrDate.SelectionStart;
                saving = false;
                state = StatesMachineBiysk.CurrentTime;
                action = StateActionsBiysk.Request;
                nextState = StatesMachineBiysk.Idle;
                tryes = 0;
                wait_times = 0;
                timer.Change(0, Timeout.Infinite);
            }
            semaMain.Release(1);
        }

        public void Stop()
        {
            if (!started)
                return;

            started = false;

            semaMain.WaitOne();
            {
                state = StatesMachineBiysk.Idle;
                errored_state = false;
            }
            semaMain.Release(1);
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
                    break;
                case "ТЭЦ-2":
                    select1 = "TEC2";
                    break;
                case "ТЭЦ-3":
                    select1 = "TEC3";
                    break;
                case "ТЭЦ-4":
                    select1 = "TEC4";
                    break;
                case "ТЭЦ-5":
                    select1 = "TEC5";
                    break;
                default:
                    break;
            }

            if (gtp.field.Length > 0) {
                select1 += "_" + gtp.field;
                select2 += select1 + "_PBR";
            }
            else {
                select2 += select1 + "_PBR";
            }

            string request = @"SELECT " + m_strUsedAdminValues + ".DATE AS DATE1, " + m_strUsedAdminValues + "." + select1 + @"_REC, " + 
                             m_strUsedAdminValues + "." + @select1 + @"_IS_PER, " + 
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
                             m_strUsedPPBRvsPBR + ".DATE_TIME AS DATE2, " + m_strUsedPPBRvsPBR + "." + select2 +
                             @" FROM " + m_strUsedAdminValues + " RIGHT JOIN " + m_strUsedPPBRvsPBR + " ON " + m_strUsedAdminValues + ".DATE = " + m_strUsedPPBRvsPBR + ".DATE_TIME " +
                             @"WHERE " + m_strUsedPPBRvsPBR + ".DATE_TIME > '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
                             @"' AND " + m_strUsedPPBRvsPBR + ".DATE_TIME <= '" + date.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                             @"' AND MINUTE(" + m_strUsedPPBRvsPBR + ".DATE_TIME) = 0 AND " + m_strUsedAdminValues + ".DATE IS NULL ORDER BY DATE1, DATE2 ASC";


            Request(request, false);
//SELECT AdminValues.date as date1, AdminValues.BTEC_REC, AdminValues.BTEC_IS_PER, AdminValues.BTEC_DIVIAT, PPBRvsPBR.date_time as date2, PPBRvsPBR.BTEC_PPBR
//FROM AdminValues
//LEFT JOIN PPBRvsPBR ON AdminValues.date = PPBRvsPBR.date_time
//WHERE 
//AdminValues.date > '2009-12-08 00:00:00'
//AND AdminValues.date <= '2009-12-09 00:00:00'
//UNION
//SELECT AdminValues.date as date1, AdminValues.BTEC_REC, AdminValues.BTEC_IS_PER, AdminValues.BTEC_DIVIAT, PPBRvsPBR.date_time as date2, PPBRvsPBR.BTEC_PPBR
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

        private void GetPPBRDatesRequest(DateTime date)
        {
            string request;

            if (mcldrDate.SelectionStart.Date > date.Date)
            {
                date = mcldrDate.SelectionStart.Date;
            }

            request = @"SELECT DATE_TIME FROM " + m_strUsedPPBRvsPBR + " WHERE " +
                      @"DATE_TIME > '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
                      @"' AND DATE_TIME <= '" + date.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                      @"' ORDER BY DATE_TIME ASC";

            Request(request, false);
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
                    adminDates[hour - 1] = true;
                }
                catch
                {
                }
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
                    PPBRDates[hour - 1] = true;
                }
                catch
                {
                }
            }
            return true;
        }

        private string NameFieldOfRequest(TEC t, GTP gtp)
        {
            string strRes = @"";

            switch (t.name)
            {
                case "БТЭЦ":
                    strRes = "BTEC";
                    break;
                case "ТЭЦ-2":
                    strRes = "TEC2";
                    break;
                case "ТЭЦ-3":
                    strRes = "TEC3";
                    break;
                case "ТЭЦ-4":
                    strRes = "TEC4";
                    break;
                case "ТЭЦ-5":
                    strRes = "TEC5";
                    break;
                default:
                    break;
            }

            if (gtp.field.Length > 0) {
                strRes += "_" + gtp.field;
            }
            else
                ;

            return strRes;
        }

        private void SetAdminValuesRequest(TEC t, GTP gtp, DateTime date)
        {
            int currentHour = serverTime.Hour;

            date = date.Date;

            if (serverTime.Date < date)
                currentHour = 0;

            string requestUpdate = "", requestInsert = "";

            string name = NameFieldOfRequest (t, gtp);

            for (int i = currentHour; i < 24; i++)
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

            // добавляем все записи, не найденные в базе
            if (requestInsert != "")
            {
                requestInsert = @"INSERT INTO " + m_strUsedAdminValues + " (DATE, " + name + @"_REC" +
                                @", " + name + "_IS_PER" +
                                @", " + name + "_DIVIAT) VALUES" + requestInsert.Substring(0, requestInsert.Length - 1) + ";";
            }
            string requestDelete = @"DELETE FROM " + m_strUsedAdminValues + " WHERE " +
                                   @"BTEC_TG1_REC = 0 AND BTEC_TG1_IS_PER = 0 AND BTEC_TG1_DIVIAT = 0 AND " +
                                   @"BTEC_TG2_REC = 0 AND BTEC_TG2_IS_PER = 0 AND BTEC_TG2_DIVIAT = 0 AND " +
                                   @"BTEC_TG35_REC = 0 AND BTEC_TG35_IS_PER = 0 AND BTEC_TG35_DIVIAT = 0 AND " +
                                   @"BTEC_TG4_REC = 0 AND BTEC_TG4_IS_PER = 0 AND BTEC_TG4_DIVIAT = 0 AND " +
                                   @"TEC2_REC = 0 AND TEC2_IS_PER = 0 AND TEC2_DIVIAT = 0 AND " +
                                   @"TEC3_TG1_REC = 0 AND TEC3_TG1_IS_PER = 0 AND TEC3_TG1_DIVIAT = 0 AND " +
                                   @"TEC3_TG5_REC = 0 AND TEC3_TG5_IS_PER = 0 AND TEC3_TG5_DIVIAT = 0 AND " +
                                   @"TEC3_TG712_REC = 0 AND TEC3_TG712_IS_PER = 0 AND TEC3_TG712_DIVIAT = 0 AND " +
                                   @"TEC3_TG1314_REC = 0 AND TEC3_TG1314_IS_PER = 0 AND TEC3_TG1314_DIVIAT = 0 AND " +
                                   @"TEC4_TG3_REC = 0 AND TEC4_TG3_IS_PER = 0 AND TEC4_TG3_DIVIAT = 0 AND " +
                                   @"TEC4_TG48_REC = 0 AND TEC4_TG48_IS_PER = 0 AND TEC4_TG48_DIVIAT = 0 AND " +
                                   @"TEC5_TG12_REC = 0 AND TEC5_TG12_IS_PER = 0 AND TEC5_TG12_DIVIAT = 0 AND " +
                                   @"TEC5_TG36_REC = 0 AND TEC5_TG36_IS_PER = 0 AND TEC5_TG36_DIVIAT = 0 AND " +
                                   @"DATE > '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
                                   @"' AND DATE <= '" + date.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss") + 
                                   @"';";

            Request(requestUpdate + requestInsert + requestDelete, false);
        }

        private int getPBRNumber (int hour) {
            int iNum = -1;

            switch (hour) {
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

        private void SetPPBRRequest(TEC t, GTP gtp, DateTime date)
        {
            int currentHour = serverTime.Hour;

            date = date.Date;

            if (serverTime.Date < date)
                currentHour = 0;

            string requestUpdate = "", requestInsert = "";

            string name = NameFieldOfRequest(t, gtp);

            for (int i = currentHour; i < 24; i++)
            {
                // запись для этого часа имеется, модифицируем её
                if (PPBRDates[i])
                {
                    requestUpdate += @"UPDATE " + m_strUsedPPBRvsPBR + " SET " + name + @"_PBR='" + values.plan[i].ToString("F1", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";
                }
                else
                {
                    // запись отсутствует, запоминаем значения
                    requestInsert += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + serverTime.Date.ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + "ПБР" + getPBRNumber (i) +
                                        @"', '" + "0" + 
                                        @"', '" + values.plan[i].ToString("F1", CultureInfo.InvariantCulture) +
                                        @"'),";
                }
            }

            // добавляем все записи, не найденные в базе
            if (requestInsert != "")
            {
                requestInsert = @"INSERT INTO " + m_strUsedPPBRvsPBR + " (DATE_TIME, WR_DATE_TIME, PBR_NUMBER, IS_COMDISP, " + name + @"_PBR) VALUES" + requestInsert.Substring(0, requestInsert.Length - 1) + ";";
            }
            else
                ;

            string requestDelete = @"DELETE FROM " + m_strUsedPPBRvsPBR + " WHERE " +
                                   @"BTEC_PBR = 0 AND BTEC_Pmax = 0 AND BTEC_Pmin = 0 AND " +
                                   @"BTEC_TG1_PBR = 0 AND BTEC_TG1_Pmax = 0 AND BTEC_TG1_Pmin = 0 AND " +
                                   @"BTEC_TG2_PBR = 0 AND BTEC_TG2_Pmax = 0 AND BTEC_TG2_Pmin = 0 AND " +
                                   @"BTEC_TG35_PBR = 0 AND BTEC_TG35_Pmax = 0 AND BTEC_TG35_Pmin = 0 AND " +
                                   @"BTEC_TG4_PBR = 0 AND BTEC_TG4_Pmax = 0 AND BTEC_TG4_Pmin = 0 AND " +
                                   @"TEC2_PBR = 0 AND TEC2_Pmax = 0 AND TEC2_Pmin = 0 AND " +
                                   @"TEC3_PBR = 0 AND TEC3_TG1_Pmax = 0 AND TEC3_TG1_Pmin = 0 AND " +
                                   @"TEC3_TG1_PBR = 0 AND TEC3_TG1_Pmax = 0 AND TEC3_TG1_Pmin = 0 AND " +
                                   @"TEC3_TG5_PBR = 0 AND TEC3_TG5_Pmax = 0 AND TEC3_TG5_Pmin = 0 AND " +
                                   @"TEC3_TG712_PBR = 0 AND TEC3_TG712_Pmax = 0 AND TEC3_TG712_Pmin = 0 AND " +
                                   @"TEC3_TG1314_PBR = 0 AND TEC3_TG1314_Pmax = 0 AND TEC3_TG1314_Pmin = 0 AND " +
                                   @"TEC4_PBR = 0 AND TEC4_TG3_Pmax = 0 AND TEC4_TG3_Pmin = 0 AND " +
                                   @"TEC4_TG3_PBR = 0 AND TEC4_TG3_Pmax = 0 AND TEC4_TG3_Pmin = 0 AND " +
                                   @"TEC4_TG48_PBR = 0 AND TEC4_TG48_Pmax = 0 AND TEC4_TG48_Pmin = 0 AND " +
                                   @"TEC5_PBR = 0 AND TEC5_TG12_Pmax = 0 AND TEC5_TG12_Pmin = 0 AND " +
                                   @"TEC5_TG12_PBR = 0 AND TEC5_TG12_Pmax = 0 AND TEC5_TG12_Pmin = 0 AND " +
                                   @"TEC5_TG36_PBR = 0 AND TEC5_TG36_Pmax = 0 AND TEC5_TG36_Pmin = 0 AND " +
                                   @"DATE_TIME > '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
                                   @"' AND DATE_TIME <= '" + date.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss") +
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
            string str_btec_tg1 = INDEX_TEC_PBR_VALUES.BTEC_TG1.ToString ();

            for (int i = layoutForLoading.hour_start; i < layoutForLoading.hour_end; i++) {
                // запись для этого часа имеется, модифицируем её
                if (layoutForLoading.existingHours[i]) {
                    if (layoutForLoading.name == "ППБР") {
                        requestUpdate += @"UPDATE " + m_strUsedPPBRvsPBR + " SET IS_COMDISP = 1, PBR_NUMBER = '" + layoutForLoading.name +
                                         @"', DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                         @"', WR_DATE_TIME = now()" +
                                         //@"', SN_BTEC = '" + "0".ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_PPBR = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_PMAX = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_PMIN = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG1_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG1_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG1_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG2_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG2_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG2_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG35_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG35_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG35_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG4_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG4_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG4_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         //@", SN_TEC2 = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2].SN[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC2_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC2_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC2_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         //@"', SN_TEC3 = '" + layoutForLoading.TEC3_110.SN[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_PPBR = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_PMAX = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_PMIN = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG1_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG1_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG1_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG5_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG5_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG5_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG712_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG712_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG712_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG1314_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG1314_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG1314_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         //@"', SN_TEC4 = '" + layoutForLoading.TEC4.SN[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_PPBR = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_PMAX = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_PMIN = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_TG3_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_TG3_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_TG3_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_TG48_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_TG48_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_TG48_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         //@"', SN_TEC5 = '" + layoutForLoading.TEC5_110.SN[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_PPBR = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_PMAX = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_PMIN = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_TG12_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_TG12_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_TG12_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_TG36_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_TG36_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_TG36_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"' WHERE " +
                                         @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                         @"'; ";
                    }
                    else {
                        requestUpdate += @"UPDATE " + m_strUsedPPBRvsPBR + " SET IS_COMDISP = 1, PBR_NUMBER = '" + layoutForLoading.name +
                                         @"', DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                         @"', WR_DATE_TIME = now()" +
                                         //@"', SN_BTEC = '" + "0".ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_PPBR = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_PMAX = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_PMIN = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG1_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG1_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG1_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG2_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG2_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG2_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG35_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG35_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG35_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG4_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG4_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', BTEC_TG4_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         //@", SN_TEC2 = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2].SN[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC2_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC2_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC2_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         //@"', SN_TEC3 = '" + layoutForLoading.TEC3_110.SN[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_PPBR = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_PMAX = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_PMIN = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG1_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG1_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG1_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG5_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG5_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG5_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG712_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG712_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG712_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG1314_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG1314_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC3_TG1314_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         //@"', SN_TEC4 = '" + layoutForLoading.TEC4.SN[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_PPBR = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_PMAX = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_PMIN = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_TG3_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_TG3_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_TG3_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_TG48_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_TG48_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC4_TG48_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         //@"', SN_TEC5 = '" + layoutForLoading.TEC5_110.SN[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_PPBR = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_PMAX = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_PMIN = '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_TG12_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_TG12_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_TG12_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_TG36_PPBR = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_TG36_PMAX = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                         @"', TEC5_TG36_PMIN = '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmin[i].ToString(CultureInfo.InvariantCulture) +
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
                                     //HHH БТЭЦ
                                     //@"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].SN[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG1].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG2].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG35].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.BTEC_TG4].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     //HHH
                                     //ТЭЦ-2
                                     //@", '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2].SN[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     //ТЭЦ-3
                                     //@"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].SN[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG5].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG712].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC3_TG1314].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     //ТЭЦ-4
                                     //@"', '" + layoutForLoading.TEC4.SN[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG3].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC4_TG48].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     //ТЭЦ-5
                                     //@"', '" + layoutForLoading.TEC5_110.SN[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].PBR[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].PBR[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmax[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmax[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + (layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmin[i] + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmin[i]).ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG12].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].PBR[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmax[i].ToString(CultureInfo.InvariantCulture) +
                                     @"', '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC5_TG36].Pmin[i].ToString(CultureInfo.InvariantCulture) +
                                     @"'),";
                }
            }

            if (requestInsert != "")
            {
                requestInsert = @"INSERT INTO " + m_strUsedPPBRvsPBR + " (IS_COMDISP, PBR_NUMBER, DATE_TIME, WR_DATE_TIME, " +
                                //БТЭЦ
                                @"BTEC_PPBR, BTEC_PMAX, BTEC_PMIN, " +
                                @"BTEC_TG1_PBR, BTEC_TG1_PMAX, BTEC_TG1_PMIN, " +
                                @"BTEC_TG2_PPBR, BTEC_TG2_PMAX, BTEC_TG2_PMIN" +
                                @"BTEC_TG1_PBR, BTEC_TG1_PMAX, BTEC_TG1_PMIN, " +
                                @"BTEC_TG2_PPBR, BTEC_TG2_PMAX, BTEC_TG2_PMIN" +
                                //ТЭЦ-2
                                @"TEC2_PPBR, TEC2_PMAX, TEC2_PMIN, " +
                                //ТЭЦ-3
                                @"TEC3_PPBR, TEC3_PMAX, TEC3_PMIN, " +
                                @"TEC3_TG1_PPBR, TEC3_TG1_PMAX, TEC3_TG1_PMIN, " +
                                @"TEC3_TG5_PPBR, TEC3_TG5_PMAX, TEC3_TG5_PMIN, " +
                                @"TEC3_TG712_PPBR, TEC3_TG712_PMAX, TEC3_TG712_PMIN, " +
                                @"TEC3_TG1314_PPBR, TEC3_TG1314_PMAX, TEC3_TG1314_PMIN, " +
                                //ТЭЦ-4
                                @"TEC4_PPBR, TEC4_PMAX, TEC4_PMIN, " +
                                @"TEC4_TG3_PBR, TEC4_TG3_PMAX, TEC4_TG3_PMIN, " +
                                @"TEC4_TG48_PPBR, TEC4_TG48_PMAX, TEC4_TG48_PMIN" +
                                //ТЭЦ-5
                                @"TEC5_PPBR, TEC5_PMAX, TEC5_PMIN, " +
                                @"TEC5_TG12_PBR, TEC5_TG12_PMAX, TEC5_TG12_PMIN, " +
                                @"TEC5_TG12_PPBR, TEC5_TG12_PMAX, TEC5_TG12_PMIN" +
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

        public void Request(string request, TEC t, int gtp)
        {
            if (t == null)
            {
                lock (dataInterface.lockData)
                {
                    dataInterface.request[1] = request;
                    dataInterface.dataPresent = false;
                    dataInterface.dataError = false;
                }
            }
            else
            {
                if (gtp < 0)
                {
                    lock (t.dataInterfaceAdmin.lockData)
                    {
                        t.dataInterfaceAdmin.request[1] = request;
                        t.dataInterfaceAdmin.dataPresent = false;
                        t.dataInterfaceAdmin.dataError = false;
                    }
                }
                else
                {
                    lock (t.GTP[gtp].dataInterfaceAdmin.lockData)
                    {
                        t.GTP[gtp].dataInterfaceAdmin.request[1] = request;
                        t.GTP[gtp].dataInterfaceAdmin.dataPresent = false;
                        t.GTP[gtp].dataInterfaceAdmin.dataError = false;
                    }
                }
            }
            try
            {
                semaDBThreadBiysk.Release(1);
            }
            catch
            {
            }
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

            dbInterface.SetConnectionSettings(connSettCommon);

            threadIsWorking = true;

            taskThread = new Thread(new ParameterizedThreadStart(TecView_ThreadFunction));
            taskThread.Name = "Интерфейс к данным";
            taskThread.IsBackground = true;

            sem = new Semaphore(1, 1);

            sem.WaitOne();
            taskThread.Start();

            //Для особенной ТЭЦ (Бийск)
            lock (dataInterface.lockData)
            {
                dataInterface.dataPresent = false;
                dataInterface.request[1] = "";
            }
            foreach (TEC t in tec)
            {
                if (t.name.Contains ("Бийск") == true) {
                    lock (t.dataInterfaceAdmin.lockData)
                    {
                        t.dataInterfaceAdmin.dataPresent = false;
                        t.dataInterfaceAdmin.request[1] = "";
                    }

                    foreach (GTP g in t.GTP)
                    {
                        lock (g.dataInterfaceAdmin.lockData)
                        {
                            g.dataInterfaceAdmin.dataPresent = false;
                            g.dataInterfaceAdmin.request[1] = "";
                        }
                    }
                }
                else
                    ; //Не особенная ТЭЦ (Бийск)
            }
            workDBTreadBiysk = true;
            semaDBThreadBiysk.WaitOne();
            dbThreadBiysk = new Thread(new ParameterizedThreadStart(dbThread_Function));
            dbThreadBiysk.Name = "РедактированиеПБР (Бийск)";
            dbThreadBiysk.IsBackground = true;
            dbThreadBiysk.Start(this);
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

            //bool joined;
            workDBTreadBiysk = false;
            lock (dataInterface.lockData)
            {
                dataInterface.request[1] = "";
            }
            foreach (TEC t in tec)
            {
                if (t.type () == TEC.TEC_TYPE.BIYSK) {
                    lock (t.dataInterfaceAdmin.lockData)
                    {
                        t.dataInterfaceAdmin.request[1] = "";
                    }

                    foreach (GTP g in t.GTP)
                    {
                        lock (g.dataInterfaceAdmin.lockData)
                        {
                            g.dataInterfaceAdmin.request[1] = "";
                        }
                    }
                }
                else
                    ; //Не Бийск
            }
            if (dbThreadBiysk.IsAlive)
            {
                try
                {
                    semaDBThreadBiysk.Release(1);
                }
                catch
                {
                }

                joined = dbThreadBiysk.Join(5000);
                if (!joined)
                    dbThreadBiysk.Abort();
                else
                    ;
            }
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
                    ActionReport("Получение списка сохранённых часовых значений.");
                    GetPPBRDatesRequest(dateForValues);
                    break;
                case StatesMachine.SaveValues:
                    ActionReport("Сохранение административных данных.");
                    SetAdminValuesRequest(allGtps[oldTecIndex].tec, allGtps[oldTecIndex], dateForValues);
                    break;
                case StatesMachine.SaveValuesPPBR:
                    ActionReport("Сохранение ПЛАНА.");
                    SetPPBRRequest(allGtps[oldTecIndex].tec, allGtps[oldTecIndex], dateForValues);
                    break;
                //case StatesMachine.UpdateValuesPPBR:
                //    ActionReport("Обровление ПЛАНА.");
                //    break;
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
                case StatesMachine.PPBRDates:
                case StatesMachine.SaveValues:
                case StatesMachine.SaveValuesPPBR:
                //case StatesMachine.UpdateValuesPPBR:
                case StatesMachine.GetPass:
                    return GetResponse(out error, out table, false);
                case StatesMachine.SetPassInsert:
                case StatesMachine.SetPassUpdate:
                case StatesMachine.LayoutGet:
                case StatesMachine.LayoutSet:
                    return GetResponse(out error, out table, true);
                default:
                    error = true;
                    table = null;
                    return false;       
            }
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
                case StatesMachine.PPBRDates:
                    ClearPPBRDates();
                    result = GetPPBRDatesResponse(table, dateForValues);
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
                case StatesMachine.SaveValuesPPBR:
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
                        ErrorReport("Ошибка разбора сохранённых часовых значений (AdminValues). Переход в ожидание.");
                        saveResult = Errors.ParseError;
                    }
                    else
                    {
                        ErrorReport("Ошибка получения сохранённых часовых значений (AdminValues). Переход в ожидание.");
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
                case StatesMachine.PPBRDates:
                    if (response)
                    {
                        ErrorReport("Ошибка разбора сохранённых часовых значений (PPBR). Переход в ожидание.");
                        saveResult = Errors.ParseError;
                    }
                    else
                    {
                        ErrorReport("Ошибка получения сохранённых часовых значений (PPBR). Переход в ожидание.");
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
                case StatesMachine.SaveValuesPPBR:
                    ErrorReport("Ошибка сохранения данных ПЛАНа. Переход в ожидание.");
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
                //    ErrorReport("Ошибка обновления данных ПЛАНа. Переход в ожидание.");
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

        private void dbThread_Function(object data)
        {
            Admin a = (Admin)data;
            bool result;
            while (a.workDBTreadBiysk)
            {
                a.semaDBThreadBiysk.WaitOne();

                lock (a.dataInterface.lockData)
                {
                    a.dataInterface.request[0] = a.dataInterface.request[1];
                }

                if (a.dataInterface.request[0] != "")
                {
                    try
                    {
                        result = a.GetData(a.dataInterface.tableData, a.dataInterface.request[0]);
                    }
                    catch
                    {
                        result = false;
                    }

                    lock (a.dataInterface.lockData)
                    {
                        if (a.dataInterface.request[0] == a.dataInterface.request[1])
                        {
                            if (result)
                            {
                                a.dataInterface.dataPresent = true;
                            }
                            else
                            {
                                a.dataInterface.dataError = true;
                            }
                            a.dataInterface.request[1] = "";
                        }
                    }
                }

                foreach (TEC t in a.tec)
                {
                    if (t.type () == TEC.TEC_TYPE.BIYSK) {
                        lock (t.dataInterfaceAdmin.lockData)
                        {
                            t.dataInterfaceAdmin.request[0] = t.dataInterfaceAdmin.request[1];
                        }

                        if (t.dataInterfaceAdmin.request[0] != "")
                        {
                            try
                            {
                                result = a.GetData(t.dataInterfaceAdmin.tableData, t.dataInterfaceAdmin.request[0]);
                            }
                            catch
                            {
                                result = false;
                            }

                            lock (t.dataInterfaceAdmin.lockData)
                            {
                                if (t.dataInterfaceAdmin.request[0] == t.dataInterfaceAdmin.request[1])
                                {
                                    if (result)
                                    {
                                        t.dataInterfaceAdmin.dataPresent = true;
                                    }
                                    else
                                    {
                                        t.dataInterfaceAdmin.dataError = true;
                                    }
                                    t.dataInterfaceAdmin.request[1] = "";
                                }
                            }
                        }
                        else
                            ;

                        if (t.GTP.Count > 1)
                        {
                            foreach (GTP g in t.GTP)
                            {
                                lock (g.dataInterfaceAdmin.lockData)
                                {
                                    g.dataInterfaceAdmin.request[0] = g.dataInterfaceAdmin.request[1];
                                }

                                if (g.dataInterfaceAdmin.request[0] != "")
                                {
                                    try
                                    {
                                        result = a.GetData(g.dataInterfaceAdmin.tableData, g.dataInterfaceAdmin.request[0]);
                                    }
                                    catch
                                    {
                                        result = false;
                                    }

                                    lock (g.dataInterfaceAdmin.lockData)
                                    {
                                        if (g.dataInterfaceAdmin.request[0] == g.dataInterfaceAdmin.request[1])
                                        {
                                            if (result)
                                            {
                                                g.dataInterfaceAdmin.dataPresent = true;
                                            }
                                            else
                                            {
                                                g.dataInterfaceAdmin.dataError = true;
                                            }
                                            g.dataInterfaceAdmin.request[1] = "";
                                        }
                                    }
                                }
                                else
                                    ; //Пустой запрос
                            }
                        }
                        else
                            ; //Один ГТП
                    }
                    else
                        ; //Не Бийск
                }
            }
            try
            {
                a.semaDBThreadBiysk.Release(1);
            }
            catch
            {
            }
        }

        private void timer_Tick(Object stateInfo)
        {
            semaMain.WaitOne();
            {
                switch (state)
                {
                    case StatesMachineBiysk.Idle:
                        break;

                    case StatesMachineBiysk.CurrentTime:
                        switch (action)
                        {
                            case StateActionsBiysk.Request:
                                ActionReport("Получение текущего времени сервера.");
                                GetDataTime();
                                action = StateActionsBiysk.Data;
                                timer.Change(WAIT_TIME, Timeout.Infinite);
                                break;
                            case StateActionsBiysk.Data:
                                if (dataInterface.dataPresent)
                                {
                                    if (UpdateTime(dataInterface.tableData))
                                    {
                                        state = nextState;
                                        action = StateActionsBiysk.Request;
                                        tryes = 0;
                                        wait_times = 0;
                                        if (using_date)
                                        {
                                            dateForValues = serverTime;
                                        }
                                        timer.Change(0, Timeout.Infinite);

                                        errored_state = actioned_state = false;
                                    }
                                    else
                                    {
                                        tryes++;
                                        if (tryes < MAX_TRYES)
                                        {
                                            ErrorReport("Ошибка получения текущего времени сервера. Повторная попытка.");
                                            action = StateActionsBiysk.Request;
                                            timer.Change(RETRY_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            ErrorReport("Ошибка получения текущего времени сервера. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            if (saving)
                                            {
                                                saveResult = Errors.ParseError;
                                                try
                                                {
                                                    semaSave.Release(1);
                                                }
                                                catch
                                                {
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                    if (dataInterface.dataError)
                                    {
                                        tryes++;
                                        if (tryes < MAX_TRYES)
                                        {
                                            ErrorReport("Ошибка получения текущего времени сервера. Повторная попытка.");
                                            action = StateActionsBiysk.Request;
                                            timer.Change(RETRY_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            ErrorReport("Ошибка получения текущего времени сервера. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            if (saving)
                                            {
                                                saveResult = Errors.NoAccess;
                                                try
                                                {
                                                    semaSave.Release(1);
                                                }
                                                catch
                                                {
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        wait_times++;
                                        if (wait_times < MAX_WAIT_TIME)
                                        {
                                            timer.Change(WAIT_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            tryes = 0;
                                            wait_times = 0;
                                            ErrorReport("Ошибка получения текущего времени сервера. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            if (saving)
                                            {
                                                saveResult = Errors.NoAccess;
                                                try
                                                {
                                                    semaSave.Release(1);
                                                }
                                                catch
                                                {
                                                }
                                            }
                                        }
                                    }
                                break;
                        }
                        break;

                    case StatesMachineBiysk.AdminValues:
                        switch (action)
                        {
                            case StateActionsBiysk.Request:
                                ActionReport("Получение значений ПБР.");
                                GetDataAdminValues(allGtps[oldTecIndex].tec, allGtps[oldTecIndex], dateForValues.Date);

                                oldDate = dateForValues.Date;
                                this.BeginInvoke(delegateCalendarSetDate, oldDate);

                                action = StateActionsBiysk.Data;
                                timer.Change(WAIT_TIME, Timeout.Infinite);
                                break;
                            case StateActionsBiysk.Data:
                                if (dataInterface.dataPresent)
                                {
                                    if (UpdateAdminValues(dataInterface.tableData, dateForValues))
                                    {
                                        this.BeginInvoke(delegateFillData, oldDate);

                                        state = StatesMachineBiysk.Idle;
                                        action = StateActionsBiysk.Request;
                                        timer.Change(0, Timeout.Infinite);

                                        errored_state = actioned_state = false;
                                    }
                                    else
                                    {
                                        tryes++;
                                        if (tryes < MAX_TRYES)
                                        {
                                            ErrorReport("Ошибка разбора административных данных. Повторная попытка.");
                                            action = StateActionsBiysk.Request;
                                            timer.Change(RETRY_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            ErrorReport("Ошибка разбора административных данных. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                        }
                                    }
                                }
                                else
                                    if (dataInterface.dataError)
                                    {
                                        tryes++;
                                        if (tryes < MAX_TRYES)
                                        {
                                            ErrorReport("Ошибка получения административных данных. Повторная попытка.");
                                            action = StateActionsBiysk.Request;
                                            timer.Change(RETRY_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            ErrorReport("Ошибка получения административных данных. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                        }
                                    }
                                    else
                                    {
                                        wait_times++;
                                        if (wait_times < MAX_WAIT_TIME)
                                        {
                                            timer.Change(WAIT_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            tryes = 0;
                                            wait_times = 0;
                                            ErrorReport("Ошибка получения административных данных. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                        }
                                    }
                                break;
                        }
                        break;

                    case StatesMachineBiysk.AdminDates:
                        switch (action)
                        {
                            case StateActionsBiysk.Request:
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
                                    state = StatesMachineBiysk.Idle;
                                    break;
                                }

                                ActionReport("Получение списка сохранённых часовых значений.");
                                GetDataAdminDates(dateForValues);
                                action = StateActionsBiysk.Data;
                                timer.Change(WAIT_TIME, Timeout.Infinite);
                                break;
                            case StateActionsBiysk.Data:
                                if (dataInterface.dataPresent)
                                {
                                    ClearAdminDates();
                                    if (UpdateAdminDates(dataInterface.tableData, dateForValues))
                                    {
                                        state = StatesMachineBiysk.SaveValues;
                                        action = StateActionsBiysk.Request;
                                        tryes = 0;
                                        wait_times = 0;
                                        timer.Change(0, Timeout.Infinite);

                                        errored_state = actioned_state = false;
                                    }
                                    else
                                    {
                                        tryes++;
                                        if (tryes < MAX_TRYES)
                                        {
                                            ErrorReport("Ошибка разбора сохранённых часовых значений. Повторная попытка.");
                                            action = StateActionsBiysk.Request;
                                            timer.Change(RETRY_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            ErrorReport("Ошибка разбора сохранённых часовых значений. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            saveResult = Errors.ParseError;
                                            try
                                            {
                                                semaSave.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                }
                                else
                                    if (dataInterface.dataError)
                                    {
                                        tryes++;
                                        if (tryes < MAX_TRYES)
                                        {
                                            ErrorReport("Ошибка получения сохранённых часовых значений. Повторная попытка.");
                                            action = StateActionsBiysk.Request;
                                            timer.Change(RETRY_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            ErrorReport("Ошибка получения сохранённых часовых значений. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            saveResult = Errors.NoAccess;
                                            try
                                            {
                                                semaSave.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                    else
                                    {
                                        wait_times++;
                                        if (wait_times < MAX_WAIT_TIME)
                                        {
                                            timer.Change(WAIT_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            tryes = 0;
                                            wait_times = 0;
                                            ErrorReport("Ошибка получения сохранённых часовых значений. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            saveResult = Errors.NoAccess;
                                            try
                                            {
                                                semaSave.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                break;
                        }
                        break;

                    case StatesMachineBiysk.SaveValues:
                        switch (action)
                        {
                            case StateActionsBiysk.Request:
                                ActionReport("Сохранение данных.");
                                SetDataAdminValues(allGtps[oldTecIndex].tec, allGtps[oldTecIndex], dateForValues);
                                action = StateActionsBiysk.Data;
                                timer.Change(WAIT_TIME, Timeout.Infinite);
                                break;
                            case StateActionsBiysk.Data:
                                if (dataInterface.dataPresent)
                                {
                                    saveResult = Errors.NoError;
                                    try
                                    {
                                        semaSave.Release(1);
                                    }
                                    catch
                                    {
                                    }
                                    state = StatesMachineBiysk.Idle;
                                }
                                else
                                    if (dataInterface.dataError)
                                    {
                                        tryes++;
                                        if (tryes < MAX_TRYES)
                                        {
                                            ErrorReport("Ошибка сохранения административных данных. Повторная попытка.");
                                            action = StateActionsBiysk.Request;
                                            timer.Change(RETRY_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            ErrorReport("Ошибка сохранения административных данных. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            saveResult = Errors.NoAccess;
                                            try
                                            {
                                                semaSave.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                    else
                                    {
                                        wait_times++;
                                        if (wait_times < MAX_WAIT_TIME)
                                        {
                                            timer.Change(WAIT_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            tryes = 0;
                                            wait_times = 0;
                                            ErrorReport("Ошибка сохранения административных данных. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            saveResult = Errors.NoAccess;
                                            try
                                            {
                                                semaSave.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                break;
                        }
                        break;

                    case StatesMachineBiysk.GetPass:
                        switch (action)
                        {
                            case StateActionsBiysk.Request:
                                if (dispatcherPass)
                                    ActionReport("Получение пароля диспетчера.");
                                else
                                    ActionReport("Получение пароля администратора.");
                                GetDataAdminPass(dispatcherPass);
                                action = StateActionsBiysk.Data;
                                timer.Change(WAIT_TIME, Timeout.Infinite);
                                break;
                            case StateActionsBiysk.Data:
                                if (dataInterface.dataPresent)
                                {
                                    if (UpdateAdminPass(dataInterface.tableData))
                                    {
                                        passResult = Errors.NoError;
                                        state = StatesMachineBiysk.Idle;
                                        errored_state = actioned_state = false;
                                        try
                                        {
                                            semaGetPass.Release(1);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    else
                                    {
                                        tryes++;
                                        if (tryes < MAX_TRYES)
                                        {
                                            if (dispatcherPass)
                                                ErrorReport("Ошибка получения пароля диспетчера. Повторная попытка.");
                                            else
                                                ErrorReport("Ошибка получения пароля администратора. Повторная попытка.");
                                            action = StateActionsBiysk.Request;
                                            timer.Change(RETRY_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            if (dispatcherPass)
                                                ErrorReport("Ошибка получения пароля диспетчера. Переход в ожидание.");
                                            else
                                                ErrorReport("Ошибка получения пароля администратора. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            passResult = Errors.ParseError;
                                            try
                                            {
                                                semaGetPass.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                }
                                else
                                    if (dataInterface.dataError)
                                    {
                                        tryes++;
                                        if (tryes < MAX_TRYES)
                                        {
                                            if (dispatcherPass)
                                                ErrorReport("Ошибка получения пароля диспетчера. Повторная попытка.");
                                            else
                                                ErrorReport("Ошибка получения пароля администратора. Повторная попытка.");
                                            action = StateActionsBiysk.Request;
                                            timer.Change(RETRY_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            if (dispatcherPass)
                                                ErrorReport("Ошибка получения пароля диспетчера. Переход в ожидание.");
                                            else
                                                ErrorReport("Ошибка получения пароля администратора. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            passResult = Errors.NoAccess;
                                            try
                                            {
                                                semaGetPass.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                    else
                                    {
                                        wait_times++;
                                        if (wait_times < MAX_WAIT_TIME)
                                        {
                                            timer.Change(WAIT_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            tryes = 0;
                                            wait_times = 0;
                                            if (dispatcherPass)
                                                ErrorReport("Ошибка получения пароля диспетчера. Переход в ожидание.");
                                            else
                                                ErrorReport("Ошибка получения пароля администратора. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            passResult = Errors.NoAccess;
                                            try
                                            {
                                                semaGetPass.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                break;
                        }
                        break;

                    case StatesMachineBiysk.SetPassInsert:
                        switch (action)
                        {
                            case StateActionsBiysk.Request:
                                if (dispatcherPass)
                                    ActionReport("Сохранение пароля диспетчера.");
                                else
                                    ActionReport("Сохранение пароля администратора.");
                                SetDataAdminPass(passReceive, dispatcherPass, true);
                                action = StateActionsBiysk.Data;
                                timer.Change(WAIT_TIME, Timeout.Infinite);
                                break;
                            case StateActionsBiysk.Data:
                                if (dataInterface.dataPresent)
                                {
                                    state = StatesMachineBiysk.Idle;
                                    passResult = Errors.NoError;
                                    try
                                    {
                                        semaSetPass.Release(1);
                                    }
                                    catch
                                    {
                                    }
                                    errored_state = actioned_state = false;
                                }
                                else
                                    if (dataInterface.dataError)
                                    {
                                        tryes++;
                                        if (tryes < MAX_TRYES)
                                        {
                                            if (dispatcherPass)
                                                ErrorReport("Ошибка сохранения пароля диспетчера. Повторная попытка.");
                                            else
                                                ErrorReport("Ошибка сохранения пароля администратора. Повторная попытка.");
                                            action = StateActionsBiysk.Request;
                                            timer.Change(RETRY_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            if (dispatcherPass)
                                                ErrorReport("Ошибка сохранения пароля диспетчера. Переход в ожидание.");
                                            else
                                                ErrorReport("Ошибка сохранения пароля администратора. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            passResult = Errors.NoAccess;
                                            try
                                            {
                                                semaSetPass.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                    else
                                    {
                                        wait_times++;
                                        if (wait_times < MAX_WAIT_TIME)
                                        {
                                            timer.Change(WAIT_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            tryes = 0;
                                            wait_times = 0;
                                            if (dispatcherPass)
                                                ErrorReport("Ошибка сохранения пароля диспетчера. Переход в ожидание.");
                                            else
                                                ErrorReport("Ошибка сохранения пароля администратора. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            passResult = Errors.NoAccess;
                                            try
                                            {
                                                semaSetPass.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                break;
                        }
                        break;

                    case StatesMachineBiysk.SetPassUpdate:
                        switch (action)
                        {
                            case StateActionsBiysk.Request:
                                if (dispatcherPass)
                                    ActionReport("Сохранение пароля диспетчера.");
                                else
                                    ActionReport("Сохранение пароля администратора.");
                                SetDataAdminPass(passReceive, dispatcherPass, false);
                                action = StateActionsBiysk.Data;
                                timer.Change(WAIT_TIME, Timeout.Infinite);
                                break;
                            case StateActionsBiysk.Data:
                                if (dataInterface.dataPresent)
                                {
                                    state = StatesMachineBiysk.Idle;
                                    passResult = Errors.NoError;
                                    try
                                    {
                                        semaSetPass.Release(1);
                                    }
                                    catch
                                    {
                                    }
                                    errored_state = actioned_state = false;
                                }
                                else
                                    if (dataInterface.dataError)
                                    {
                                        tryes++;
                                        if (tryes < MAX_TRYES)
                                        {
                                            if (dispatcherPass)
                                                ErrorReport("Ошибка сохранения пароля диспетчера. Повторная попытка.");
                                            else
                                                ErrorReport("Ошибка сохранения пароля администратора. Повторная попытка.");
                                            action = StateActionsBiysk.Request;
                                            timer.Change(RETRY_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            if (dispatcherPass)
                                                ErrorReport("Ошибка сохранения пароля диспетчера. Переход в ожидание.");
                                            else
                                                ErrorReport("Ошибка сохранения пароля администратора. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            passResult = Errors.NoAccess;
                                            try
                                            {
                                                semaSetPass.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                    else
                                    {
                                        wait_times++;
                                        if (wait_times < MAX_WAIT_TIME)
                                        {
                                            timer.Change(WAIT_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            tryes = 0;
                                            wait_times = 0;
                                            if (dispatcherPass)
                                                ErrorReport("Ошибка сохранения пароля диспетчера. Переход в ожидание.");
                                            else
                                                ErrorReport("Ошибка сохранения пароля администратора. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            passResult = Errors.NoAccess;
                                            try
                                            {
                                                semaSetPass.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                break;
                        }
                        break;

                    case StatesMachineBiysk.LayoutGet:
                        switch (action)
                        {
                            case StateActionsBiysk.Request:
                                ActionReport("Получение административных данных.");
                                GetDataLayout(dateForValues);
                                action = StateActionsBiysk.Data;
                                timer.Change(WAIT_TIME, Timeout.Infinite);
                                break;
                            case StateActionsBiysk.Data:
                                if (dataInterface.dataPresent)
                                {
                                    if (UpdateLayoutValues(dataInterface.tableData, dateForValues))
                                    {
                                        loadLayoutResult = Errors.NoError;
                                        state = StatesMachineBiysk.Idle;
                                        errored_state = actioned_state = false;
                                        try
                                        {
                                            semaLoadLayout.Release(1);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    else
                                    {
                                        tryes++;
                                        if (tryes < MAX_TRYES)
                                        {
                                            ErrorReport("Ошибка разбора административных данных. Повторная попытка.");
                                            action = StateActionsBiysk.Request;
                                            timer.Change(RETRY_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            ErrorReport("Ошибка разбора административных данных. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            loadLayoutResult = Errors.ParseError;
                                            try
                                            {
                                                semaLoadLayout.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                }
                                else
                                    if (dataInterface.dataError)
                                    {
                                        tryes++;
                                        if (tryes < MAX_TRYES)
                                        {
                                            ErrorReport("Ошибка получения административных данных. Повторная попытка.");
                                            action = StateActionsBiysk.Request;
                                            timer.Change(RETRY_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            ErrorReport("Ошибка получения административных данных. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            loadLayoutResult = Errors.NoAccess;
                                            try
                                            {
                                                semaLoadLayout.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                    else
                                    {
                                        wait_times++;
                                        if (wait_times < MAX_WAIT_TIME)
                                        {
                                            timer.Change(WAIT_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            tryes = 0;
                                            wait_times = 0;
                                            ErrorReport("Ошибка получения административных данных. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            loadLayoutResult = Errors.NoAccess;
                                            try
                                            {
                                                semaLoadLayout.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                break;
                        }
                        break;

                    case StatesMachineBiysk.LayoutSet:
                        switch (action)
                        {
                            case StateActionsBiysk.Request:
                                ActionReport("Сохранение административных данных.");
                                SetDataLayout(dateForValues);
                                action = StateActionsBiysk.Data;
                                timer.Change(WAIT_TIME, Timeout.Infinite);
                                break;
                            case StateActionsBiysk.Data:
                                if (dataInterface.dataPresent)
                                {
                                    state = StatesMachineBiysk.Idle;
                                    loadLayoutResult = Errors.NoError;
                                    try
                                    {
                                        semaLoadLayout.Release(1);
                                    }
                                    catch
                                    {
                                    }
                                    errored_state = actioned_state = false;
                                }
                                else
                                    if (dataInterface.dataError)
                                    {
                                        tryes++;
                                        if (tryes < MAX_TRYES)
                                        {
                                            ErrorReport("Ошибка сохранения административных данных. Повторная попытка.");
                                            action = StateActionsBiysk.Request;
                                            timer.Change(RETRY_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            ErrorReport("Ошибка сохранения административных данных. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            loadLayoutResult = Errors.NoAccess;
                                            try
                                            {
                                                semaLoadLayout.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                    else
                                    {
                                        wait_times++;
                                        if (wait_times < MAX_WAIT_TIME)
                                        {
                                            timer.Change(WAIT_TIME, Timeout.Infinite);
                                        }
                                        else
                                        {
                                            tryes = 0;
                                            wait_times = 0;
                                            ErrorReport("Ошибка сохранения административных данных. Переход в ожидание.");
                                            state = StatesMachineBiysk.Idle;
                                            loadLayoutResult = Errors.NoAccess;
                                            try
                                            {
                                                semaLoadLayout.Release(1);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                break;
                        }
                        break;

                    default:
                        ErrorReport("Нейзвестное состояние. Переход в ожидание.");
                        state = StatesMachineBiysk.Idle;
                        break;
                }
            }
            semaMain.Release(1);
        }

        public bool GetData(DataTable table, string query)
        {
            bool result = true;

            if (!Connect())
                return false;

            if (connection.State == ConnectionState.Open)
            {
                lock (lockValue)
                {
                    command.CommandText = query;

                    table.Reset();
                    table.Locale = System.Globalization.CultureInfo.InvariantCulture;

                    try
                    {
                        adapter.Fill(table);
                        is_data_error = false;
                    }
                    catch (MySqlException e)
                    {
                        string s;
                        int pos;
                        pos = adapter.SelectCommand.Connection.ConnectionString.IndexOf("Password");
                        if (pos < 0)
                            s = adapter.SelectCommand.Connection.ConnectionString;
                        else
                            s = adapter.SelectCommand.Connection.ConnectionString.Substring(0, pos);

                        MainForm.log.LogToFile("Ошибка получения данных: " + s, true, true, false);
                        MainForm.log.LogToFile("Запрос " + adapter.SelectCommand.CommandText, false, false, false);
                        MainForm.log.LogToFile("Ошибка " + e.Message, false, false, false);
                        MainForm.log.LogToFile(e.ToString(), false, false, false);

                        //if (!is_data_error)
                        //    MessageBox.Show("Ошибка получения административных данных.\nПроверьте правильность настроек и состояние сети.\n\nДанные об ошибке сохранены в файл logDB.txt", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        is_data_error = true;
                        result = false;
                    }
                    catch
                    {
                        string s;
                        int pos;
                        pos = adapter.SelectCommand.Connection.ConnectionString.IndexOf("Password");
                        if (pos < 0)
                            s = adapter.SelectCommand.Connection.ConnectionString;
                        else
                            s = adapter.SelectCommand.Connection.ConnectionString.Substring(0, pos);

                        MainForm.log.LogToFile("Ошибка получения данных: " + s, true, true, false);
                        MainForm.log.LogToFile("Запрос " + adapter.SelectCommand.CommandText, false, false, false);

                        //if (!is_data_error)
                        //    MessageBox.Show("Ошибка получения административных данных.\nПроверьте правильность настроек и состояние сети.\n\nДанные об ошибке сохранены в файл logDB.txt", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        is_data_error = true;
                        result = false;
                    }
                }
            }

            return result;
        }

        private void GetDataTime()
        {
            Request("SELECT now()", null, 0);
        }

        private bool UpdateTime(DataTable table)
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

        private void GetDataAdminValues(TEC t, GTP gtp, DateTime date)
        {
            string select1 = "";
            string select2 = "";

            switch (t.name)
            {
                //case "БТЭЦ":
                //    select1 = "BTEC";
                //    select2 = "PPBR_BTEC";
                //    break;
                //case "ТЭЦ-2":
                //    select1 = "TEC2";
                //    select2 = "PPBR_TEC2";
                //    break;
                //case "ТЭЦ-3":
                //    select1 = "TEC3";
                //    select2 = "PPBR_TEC3";
                //    break;
                //case "ТЭЦ-4":
                //    select1 = "TEC4";
                //    select2 = "PPBR_TEC4";
                //    break;
                //case "ТЭЦ-5":
                //    select1 = "TEC5";
                //    select2 = "PPBR_TEC5";
                //    break;
                case "Бийск-ТЭЦ":
                    select1 = "TEC_BIYSK";
                    select2 = "PPBR_TEC_BIYSK";
                    break;
                default:
                    break;
            }

            switch (gtp.name)
            {
                case "ГТП ТГ1,2":
                    select1 += "_110";
                    select2 += "_110";
                    break;
                case "ГТП ТГ3-8":
                    select1 += "_35";
                    select2 += "_35";
                    break;
                default:
                    break;
            }

            string request = @"SELECT DATE, " + select1 + @"_REC, " + select1 + @"_IS_PER, " + select1 + @"_DIVIAT " +
                             @" FROM ADMINVALUES " +
                             @"WHERE DATE >= '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
                             @"' AND DATE <= '" + date.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                             @"' ORDER BY DATE";

            Request(request, null, 0);
        }

        private bool UpdateAdminValues(DataTable table, DateTime date)
        {
            for (int i = 0, hour; i < table.Rows.Count; i++)
            {
                if (table.Rows[i][0] is System.DBNull)
                {
                    try
                    {
                        hour = ((DateTime)table.Rows[i][0]).Hour;
                        if (hour == 0 && ((DateTime)table.Rows[i][0]).Day != date.Day)
                            hour = 24;
                        else
                            if (hour == 0)
                                continue;

                        values.plan[hour - 1] = 0/*(double)table.Rows[i][5]*/;
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
                        /*if (!(table.Rows[i][4] is System.DBNull))
                            values.plan[hour - 1] = (double)table.Rows[i][5];
                        else*/
                        values.plan[hour - 1] = 0;
                    }
                    catch
                    {
                    }
                }
            }
            return true;
        }

        private void GetDataAdminDates(DateTime date)
        {
            string request;

            if (mcldrDate.SelectionStart.Date > date.Date)
            {
                date = mcldrDate.SelectionStart.Date;
            }

            request = @"SELECT DATE FROM ADMINVALUES WHERE " +
                      @"DATE > '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
                      @"' AND DATE <= '" + date.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                      @"' ORDER BY DATE ASC";

            Request(request, null, 0);
        }

        private bool UpdateAdminDates(DataTable table, DateTime date)
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

        private void SetDataAdminValues(TEC t, GTP gtp, DateTime date)
        {
            int currentHour = serverTime.Hour;

            date = date.Date;

            if (serverTime.Date < date)
                currentHour = 0;

            string requestUpdate = "", requestInsert = "";

            string name = "";

            switch (t.name)
            {
                //case "БТЭЦ":
                //    name = "BTEC";
                //    break;
                //case "ТЭЦ-2":
                //    name = "TEC2";
                //    break;
                //case "ТЭЦ-3":
                //    name = "TEC3";
                //    break;
                //case "ТЭЦ-4":
                //    name = "TEC4";
                //    break;
                //case "ТЭЦ-5":
                //    name = "TEC5";
                //    break;
                case "Бийск-ТЭЦ":
                    name = "TEC_BIYSK";
                    break;
                default:
                    break;
            }

            switch (gtp.name)
            {
                case "ГТП ТГ3-8":
                    name += "_110";
                    break;
                case "ГТП ТГ1,2":
                    name += "_35";
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
                        requestUpdate += @"UPDATE ADMINVALUES SET " + name + @"_REC='" + values.recommendations[i].ToString("F2", CultureInfo.InvariantCulture) +
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
                requestInsert = @"INSERT INTO ADMINVALUES (DATE, " + name + @"_REC" +
                                @", " + name + "_IS_PER" +
                                @", " + name + "_DIVIAT) VALUES" + requestInsert.Substring(0, requestInsert.Length - 1) + ";";
            }
            string requestDelete = @"DELETE FROM ADMINVALUES WHERE " +
                /*@"BTEC_REC = 0 AND BTEC_IS_PER = 0 AND BTEC_DIVIAT = 0 AND " +
                @"TEC2_REC = 0 AND TEC2_IS_PER = 0 AND TEC2_DIVIAT = 0 AND " +
                @"TEC3_110_REC = 0 AND TEC3_110_IS_PER = 0 AND TEC3_110_DIVIAT = 0 AND " +
                @"TEC3_220_REC = 0 AND TEC3_220_IS_PER = 0 AND TEC3_220_DIVIAT = 0 AND " +
                @"TEC4_REC = 0 AND TEC4_IS_PER = 0 AND TEC4_DIVIAT = 0 AND " +
                @"TEC5_110_REC = 0 AND TEC5_110_IS_PER = 0 AND TEC5_110_DIVIAT = 0 AND " +
                @"TEC5_220_REC = 0 AND TEC5_220_IS_PER = 0 AND TEC5_220_DIVIAT = 0 AND " +*/
                                   @"TEC_BIYSK_35_REC = 0 AND TEC_BIYSK_35_IS_PER = 0 AND TEC_BIYSK_35_DIVIAT = 0 AND " +
                                   @"TEC_BIYSK_110_REC = 0 AND TEC_BIYSK_110_IS_PER = 0 AND TEC_BIYSK_110_DIVIAT = 0 AND " +
                                   @"DATE > '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
                                   @"' AND DATE <= '" + date.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss") +
                                   @"';";

            Request(requestUpdate + requestInsert + requestDelete, null, 0);
        }

        private void GetDataAdminPass(bool disp)
        {
            if (disp)
                Request("SELECT PASSWORD_DISP FROM TOOLS", null, 0);
            else
                Request("SELECT PASSWORD_ADMIN FROM TOOLS", null, 0);
        }

        private bool UpdateAdminPass(DataTable table)
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

        private void SetDataAdminPass(string password, bool disp, bool insert)
        {
            if (insert)
                if (disp)
                    Request("INSERT INTO TOOLS (PASSWORD_DISP) VALUES ('" + password + "')", null, 0);
                else
                    Request("INSERT INTO TOOLS (PASSWORD_ADMIN) VALUES ('" + password + "')", null, 0);
            else
                if (disp)
                    Request("UPDATE TOOLS SET PASSWORD_DISP='" + password + "'", null, 0);
                else
                    Request("UPDATE TOOLS SET PASSWORD_ADMIN='" + password + "'", null, 0);
        }

        private void GetDataLayout(DateTime date)
        {
            string request = @"SELECT PPBRVSPBR.DATE_TIME, PPBRVSPBR.PBR_NUMBER FROM PPBRVSPBR " +
                             @"WHERE PPBRVSPBR.DATE_TIME >= '" + date.ToString("yyyy-MM-dd HH:mm:ss") +
                             @"' AND PPBRVSPBR.DATE_TIME <= '" + date.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss") +
                             @"' AND MINUTE(PPBRVSPBR.DATE_TIME) = 0 ORDER BY PPBRVSPBR.DATE_TIME ASC";
            Request(request, null, 0);
        }

        private bool UpdateLayoutValues(DataTable table, DateTime date)
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
                    switch (name)
                    {
                        case "ППБР": num = 0; break;
                        case "ПБР1": num = 0; break;
                        case "ПБР4": num = 3; break;
                        case "ПБР7": num = 6; break;
                        case "ПБР10": num = 9; break;
                        case "ПБР13": num = 12; break;
                        case "ПБР16": num = 15; break;
                        case "ПБР19": num = 18; break;
                        case "ПБР22": num = 21; break;
                        default: num = 0; break;
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

        private void SetDataLayout(DateTime date)
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
                    }
                    else
                    {
                    }
                }
                else
                {
                    // запись отсутствует, запоминаем значения
                }
            }

            if (requestInsert != "")
            {

            }

            //Request(requestInsert + requestUpdate, null, 0);
        }

        public bool Connect()
        {
            bool result = true;
            string connectionStringNew = @"Server=" + connSettBiysk.server +
                                         @";Database=" + connSettBiysk.dbName +
                                         @";User Id=" + connSettBiysk.userName +
                                         @";Password=" + connSettBiysk.password + @";";
            string connectionStringOld;

            //connectionStringNew = @"Server=localhost;Database=Statistic;User id=root;Password=;";

            if (connectionStringNew != connection.ConnectionString)
            {
                if (!Disconnect())
                    return false;

                lock (lockValue)
                {
                    connectionStringOld = connection.ConnectionString;
                    connection.ConnectionString = connectionStringNew;

                    try
                    {
                        connection.Open();
                        is_connection_error = false;
                    }
                    catch (MySqlException e)
                    {
                        MainForm.log.LogLock();
                        string s;
                        int pos;
                        pos = connectionStringNew.IndexOf("Password");
                        if (pos < 0)
                            s = connectionStringNew;
                        else
                            s = connectionStringNew.Substring(0, pos);

                        MainForm.log.LogToFile("Ошибка открытия соединения", true, true, false);
                        MainForm.log.LogToFile("Строка соединения " + s, false, false, false);
                        MainForm.log.LogToFile("Ошибка " + e.Message, false, false, false);
                        MainForm.log.LogToFile(e.ToString(), false, false, false);
                        MainForm.log.LogUnlock();

                        //if (!is_connection_error)
                        //    MessageBox.Show("Не удаётся соединиться с базой административных данных.\nПроверьте настройки подключения и состояние сети.\n\nДанные об ошибке сохранены в файл logDB.txt", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        is_connection_error = true;
                        connection.ConnectionString = connectionStringOld;
                        result = false;
                    }
                    catch
                    {
                        MainForm.log.LogLock();
                        string s;
                        int pos;
                        pos = connectionStringNew.IndexOf("Password");
                        if (pos < 0)
                            s = connectionStringNew;
                        else
                            s = connectionStringNew.Substring(0, pos);

                        MainForm.log.LogToFile("Ошибка открытия соединения", true, true, false);
                        MainForm.log.LogToFile("Строка соединения " + s, false, false, false);
                        MainForm.log.LogUnlock();

                        //if (!is_connection_error)
                        //    MessageBox.Show("Не удаётся соединиться с базой административных данных.\nПроверьте настройки подключения и состояние сети.\n\nДанные об ошибке сохранены в файл logDB.txt", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        is_connection_error = true;
                        connection.ConnectionString = connectionStringOld;
                        result = false;
                    }
                }
            }
            return result;
        }

        public bool Disconnect()
        {
            bool result = true;
            if (connection.State == ConnectionState.Open)
            {
                lock (lockValue)
                {
                    try
                    {
                        connection.Close();
                    }
                    catch (MySqlException e)
                    {
                        MainForm.log.LogLock();
                        MainForm.log.LogToFile("Ошибка закрытия соединения: " + e.Message, true, true, false);
                        MainForm.log.LogToFile(e.ToString(), false, false, false);
                        MainForm.log.LogUnlock();

                        //MessageBox.Show("Не удаётся закрыть соединение с базой административных данных.\n\nДанные об ошибке сохранены в файл logDB.txt.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        result = false;
                    }
                    catch
                    {
                        MainForm.log.LogLock();
                        MainForm.log.LogToFile("Ошибка закрытия соединения", true, true, false);
                        MainForm.log.LogUnlock();

                        //MessageBox.Show("Не удаётся закрыть соединение с базой административных данных.\n\nДанные об ошибке сохранены в файл logDB.txt.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        result = false;
                    }
                }
                is_data_error = is_connection_error = false;
                errored_state = false;
            }
            return result;
        }
    }
}
