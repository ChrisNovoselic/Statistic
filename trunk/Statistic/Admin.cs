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
            public double plan;
            public double recomendation;
            public bool deviationType;
            public double deviation;
        }

        public struct TECComponentsAdminStruct
        {
            public double[] plan;
            public double[] recommendations;            
            public double[] diviation;
            public bool[] diviationPercent;

            public TECComponentsAdminStruct(int count)
            {
                this.plan = new double[count];
                this.recommendations = new double[count];                
                this.diviation = new double[count];
                this.diviationPercent = new bool[count];
            }
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

        ChangeMode.MODE_TECCOMPONENT m_modeTECComponent;
        public int mode(int new_mode = (int) ChangeMode.MODE_TECCOMPONENT.UNKNOWN)
        {
            int prev_mode = (int) m_modeTECComponent;

            if (new_mode == (int) ChangeMode.MODE_TECCOMPONENT.UNKNOWN)
                ;
            else
                m_modeTECComponent = (ChangeMode.MODE_TECCOMPONENT) new_mode;

            return prev_mode;
        }

        private const double maxPlanValue = 1500;
        private const double maxRecomendationValue = 1500;
        private const double maxDeviationValue = 1500;
        private const double maxDeviationPercentValue = 100;

        public enum DESC_INDEX : ushort { DATE_HOUR, PLAN, RECOMENDATION, DIVIATION_TYPE, DIVIATION, TO_ALL };
        private string [] arDescStringIndex = {"DateHour", "Plan", "Recomendation", "DeviationType", "Deviation", "ToAll"};
        private string[] arDescRusStringIndex = { "Дата, час", "План", "Рекомендация", "Отклонение в процентах", "Величина максимального отклонения", "Дозаполнить" };

        private volatile OldValuesStruct[] oldValues;
        private TECComponentsAdminStruct values;
        private DateTime oldDate;
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
        private volatile bool dispatcherPass;

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
        private int m_indxDbInterfaceCurrent; //Индекс в списке 'm_listDbInterfaces'

        public ConnectionSettings connSettConfigDB;
        int m_indxDbInterfaceConfigDB,
            m_listenerIdConfigDB;

        DataTable m_tablePPBRValuesResponse;

        private enum StatesMachine
        {
            CurrentTime,
            AdminValues, //Получение административных данных
            PPBRValues,
            AdminDates, //Получение списка сохранённых часовых значений
            PPBRDates,
            SaveAdminValues, //Сохранение административных данных
            SavePPBRValues, //Сохранение PPBR
            //UpdateValuesPPBR, //Обновление PPBR после 'SaveValuesPPBR'
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

        //Для особкнной ТЭЦ (Бийск)
        //private volatile DbDataInterface dataInterface;
        
        //private Thread dbThread;
        //private Semaphore sema;
        //private volatile bool workTread;
        //-------------------------

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
            this.DateHour.Name = arDescStringIndex [(int) Admin.DESC_INDEX.DATE_HOUR];
            this.DateHour.ReadOnly = true;
            this.DateHour.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Plan
            // 
            this.Plan.Frozen = true;
            this.Plan.HeaderText = arDescRusStringIndex[(int)Admin.DESC_INDEX.PLAN];
            this.Plan.Name = arDescStringIndex[(int)Admin.DESC_INDEX.PLAN];
            this.Plan.ReadOnly = false;
            this.Plan.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Plan.Width = 70;
            // 
            // Recommendation
            // 
            this.Recommendation.HeaderText = arDescRusStringIndex[(int)Admin.DESC_INDEX.RECOMENDATION];
            this.Recommendation.Name = arDescStringIndex[(int)Admin.DESC_INDEX.RECOMENDATION];
            this.Recommendation.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // DeviationType
            // 
            this.DeviationType.HeaderText = arDescRusStringIndex[(int)Admin.DESC_INDEX.DIVIATION_TYPE];
            this.DeviationType.Name = arDescStringIndex[(int)Admin.DESC_INDEX.DIVIATION_TYPE];
            this.DeviationType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Deviation
            // 
            this.Deviation.HeaderText = arDescRusStringIndex[(int)Admin.DESC_INDEX.DIVIATION];
            this.Deviation.Name = arDescStringIndex[(int)Admin.DESC_INDEX.DIVIATION];
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
            this.ToAll.HeaderText = arDescRusStringIndex[(int)Admin.DESC_INDEX.TO_ALL];
            this.ToAll.Name = arDescStringIndex [(int) Admin.DESC_INDEX.TO_ALL];
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

            //m_strUsedAdminValues = "AdminValuesNew";
            //m_strUsedPPBRvsPBR = "PPBRvsPBRnew";

            m_listDbInterfaces = new List <DbInterface> ();
            m_listListenerIdCurrent = new List <int> ();

            started = false;

            oldValues = new OldValuesStruct[24];

            md5 = new MD5CryptoServiceProvider();

            is_data_error = is_connection_error = false;

            //TecView tecView = MainForm.selectedTecViews [MainForm.stclTecViews.SelectedIndex];

            isActive = false;

            using_date = false;

            values = new TECComponentsAdminStruct(24);

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

            delegateFillData = new DelegateFunctionDate(FillData);
            delegateCalendarSetDate = new DelegateFunctionDate(CalendarSetDate);

            //dataInterface = new DbDataInterface();

            stsStrip = sts;

            this.dgwAdminTable.Rows.Add(24);

            states = new List<StatesMachine>();
        }

        private bool WasChanged()
        {
            for (int i = 0; i < 24; i++)
            {
                if (oldValues[i].plan != values.plan[i] /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.PLAN].Value.ToString())*/)
                    return true;
                else
                    ;
                if (oldValues[i].recomendation != values.recommendations[i] /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.RECOMENDATION].Value.ToString())*/)
                    return true;
                else
                    ;
                if (oldValues[i].deviationType != values.diviationPercent[i] /*bool.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.DIVIATION_TYPE].Value.ToString())*/)
                    return true;
                else
                    ;
                if (oldValues[i].deviation != values.diviation[i] /*double.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.DIVIATION].Value.ToString())*/)
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
                dateForValues = oldDate;

                newState = true;
                states.Clear();

                MainForm.log.LogLock();
                MainForm.log.LogToFile("SaveChanges () - states.Clear()", true, true, false);
                MainForm.log.LogUnlock();

                states.Add(StatesMachine.CurrentTime);
                states.Add(StatesMachine.AdminDates);
                //??? Состояния позволяют НАЧать процесс разработки возможности редактирования ПЛАНа на вкладке 'Редактирование ПБР'
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
                oldValues[i].plan = values.plan[i];
                oldValues[i].recomendation = values.recommendations[i];
                oldValues[i].deviationType = values.diviationPercent[i];
                oldValues[i].deviation = values.diviation[i];
            }
        }

        private void FillData(DateTime date)
        {
            for (int i = 0; i < 24; i++)
            {
                this.dgwAdminTable.Rows[i].Cells[(int) Admin.DESC_INDEX.DATE_HOUR].Value = date.AddHours(i + 1).ToString("yyyy-MM-dd HH");
                this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.PLAN].Value = values.plan[i].ToString("F2");
                this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.RECOMENDATION].Value = values.recommendations[i].ToString("F2");
                this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.DIVIATION_TYPE].Value = values.diviationPercent[i].ToString();
                this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.DIVIATION].Value = values.diviation[i].ToString("F2");
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
        //    double[] values;

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

        //        layoutForLoading.name = "ПБР" + layoutForLoading.hour_start.ToString();
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

        //        layoutForLoading.name = "ППБР";
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
        //            case 100: /*values = tecPPBRValues.SN; break;*/
        //            case 200: values = tecPPBRValues.PBR; break;
        //            case 300: values = tecPPBRValues.Pmax; break;
        //            case 400: values = tecPPBRValues.Pmin; break;
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
        //            if (!double.TryParse(tmp_str, out values[hour++]))
        //            {
        //                sr.Close();
        //                return false;
        //            }
        //            pos1 = pos2 + 1;
        //        }

        //        if ((pos2 = str.IndexOf("==", pos1)) > 0)
        //        {
        //            tmp_str = SetNumberSeparator(str.Substring(pos1, pos2 - pos1));
        //            if (!double.TryParse(tmp_str, out values[hour++]))
        //            {
        //                sr.Close();
        //                return false;
        //            }
        //            pos1 = pos2 + 1;
        //        }
        //        else
        //        {
        //            tmp_str = SetNumberSeparator(str.Substring(pos1, str.Length - pos1));
        //            if (!double.TryParse(tmp_str, out values[hour++]))
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
        //            MessageBox.Show(this, "Неправильный формат макета. Загрузка невзможна.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        //            MessageBox.Show(this, "Ошибка получения административных данных. Макет не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return;
        //        }

        //        if (serverTime.Date.CompareTo(layoutForLoading.date) > 0)
        //        {
        //            MessageBox.Show(this, "Данные не относятся к текущим суткам. Макет не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return;
        //        }

        //        if (layoutForLoading.hour_start < layoutForLoading.hour_start_in_db)
        //        {
        //            DialogResult result = MessageBox.Show(this, "В базе данных обнаружен более поздний макет " + layoutForLoading.name_in_db + ".\nПродолжить сохранение?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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

        //                    DialogResult result = MessageBox.Show(this, "Макет содержит данные о прошедших часах и текущем часе.\nЗаписать в базу значения для " + hours + " часов?\n", "Внимание", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
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
        //            MessageBox.Show(this, "Ошибка сохранения административных данных. Макет не сохранён.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return;
        //        }
        //    }
        //}

        /*
        private void cellValidated(int rowIndx, int colIndx)
        {
            double value;
            bool valid;

            valid = double.TryParse((string)this.dgwAdminTable.Rows[rowIndx].Cells[colIndx].Value, out value);
            if (!valid || value > maxRecomendationValue)
            {
                values.recommendations[rowIndx] = 0;
                this.dgwAdminTable.Rows[rowIndx].Cells[colIndx].Value = 0.ToString("F2");
            }
            else
            {
                values.recommendations[rowIndx] = value;
                this.dgwAdminTable.Rows[rowIndx].Cells[colIndx].Value = value.ToString("F2");
            }
        }
        */

        private void dgwAdminTable_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            double value;
            bool valid;

            switch (e.ColumnIndex)
            {
                case (int)DESC_INDEX.PLAN: // План
                    //cellValidated(e.RowIndex, (int)DESC_INDEX.PLAN);

                    valid = double.TryParse((string)this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value, out value);
                    if (!valid || value > maxRecomendationValue)
                    {
                        values.plan[e.RowIndex] = 0;
                        this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value = 0.ToString("F2");
                    }
                    else
                    {
                        values.plan[e.RowIndex] = value;
                        this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value = value.ToString("F2");
                    }
                    break;
                case (int) DESC_INDEX.RECOMENDATION: // Рекомендация
                    {
                        //cellValidated(e.RowIndex, (int)DESC_INDEX.RECOMENDATION);

                        valid = double.TryParse((string)this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value, out value);
                        if (!valid || value > maxRecomendationValue)
                        {
                            values.recommendations[e.RowIndex] = 0;
                            this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value = 0.ToString("F2");
                        }
                        else
                        {
                            values.recommendations[e.RowIndex] = value;
                            this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value = value.ToString("F2");
                        }
                        break;
                    }
                case (int)DESC_INDEX.DIVIATION_TYPE:
                    {
                        values.diviationPercent[e.RowIndex] = bool.Parse(this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.DIVIATION_TYPE].Value.ToString());
                        break;
                    }
                case (int)DESC_INDEX.DIVIATION: // Максимальное отклонение
                    {
                        valid = double.TryParse((string)this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.DIVIATION].Value, out value);
                        bool isPercent = bool.Parse(this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.DIVIATION_TYPE].Value.ToString());
                        double maxValue;
                        double recom = double.Parse((string)this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value);

                        if (isPercent)
                            maxValue = maxDeviationPercentValue;
                        else
                            maxValue = maxDeviationValue; // вообще эти значения не суммируются, но для максимальной границы нормально

                        if (!valid || value < 0 || value > maxValue)
                        {
                            values.diviation[e.RowIndex] = 0;
                            this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.DIVIATION].Value = 0.ToString("F2");
                        }
                        else
                        {
                            values.diviation[e.RowIndex] = value;
                            this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.DIVIATION].Value = value.ToString("F2");
                        }
                        break;
                    }
            }
        }

        private void dgwAdminTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == (int)DESC_INDEX.TO_ALL && e.RowIndex >= 0) // кнопка применение для всех
            {
                for (int i = e.RowIndex + 1; i < 24; i++)
                {
                    values.plan[i] = values.plan[e.RowIndex];
                    values.recommendations[i] = values.recommendations[e.RowIndex];
                    values.diviationPercent[i] = values.diviationPercent[e.RowIndex];
                    values.diviation[i] = values.diviation[e.RowIndex];

                    this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.PLAN].Value = this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value;
                    this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.RECOMENDATION].Value = this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value;
                    this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.DIVIATION_TYPE].Value = this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.DIVIATION_TYPE].Value;
                    this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.DIVIATION].Value = this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.DIVIATION].Value;
                }
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
                cbxTec.SelectedIndex = oldTecIndex;

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
                this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.DATE_HOUR].Value = "";
                this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.PLAN].Value = "";
                this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.RECOMENDATION].Value = "";
                this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.DIVIATION_TYPE].Value = "false";
                this.dgwAdminTable.Rows[i].Cells[(int)DESC_INDEX.DIVIATION].Value = "";
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
                ErrorReport("Ошибка получения текущего времени сервера. Используется локальное время.");
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

                        values.plan[hour - 1] = (double)table.Rows[i][arIndexTables[1] * 4 + 1/*"PBR"*/];
                        values.recommendations[hour - 1] = 0;
                        values.diviationPercent[hour - 1] = false;
                        values.diviation[hour - 1] = 0;
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

                        values.recommendations[hour - 1] = (double)table.Rows[i][arIndexTables[1] * 3 + 1/*"REC"*/];
                        values.diviationPercent[hour - 1] = (int)table.Rows[i][arIndexTables[1] * 3 + 2/*"IS_PER"*/] == 1;
                        values.diviation[hour - 1] = (double)table.Rows[i][arIndexTables[1] * 3 + 3/*"DIVIAT"*/];
                        if (!(table.Rows[i]["DATE_PBR"] is System.DBNull))
                            values.plan[hour - 1] = (double)table.Rows[i][arIndexTables[0] * 4 + 1/*"PBR"*/];
                        else
                            values.plan[hour - 1] = 0;
                    }
                    catch { }
                }
            }

            return true;
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
                // запись для этого часа имеется, модифицируем её
                if (adminDates[i])
                {
                    switch (m_modeTECComponent) {
                        case ChangeMode.MODE_TECCOMPONENT.GTP:
                            //name = t.NameFieldOfAdminRequest(comp);
                            
                            requestUpdate += @"UPDATE " + t.m_strUsedAdminValues + " SET " + name + @"_REC='" + values.recommendations[i].ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + name + @"_IS_PER=" + (values.diviationPercent[i] ? "1" : "0") +
                                        @", " + name + "_DIVIAT='" + values.diviation[i].ToString("F2", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";
                            break;
                        case ChangeMode.MODE_TECCOMPONENT.PC:
                            requestUpdate += @"UPDATE " + strUsedAdminValues + " SET " + @"REC='" + values.recommendations[i].ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + @"IS_PER=" + (values.diviationPercent[i] ? "1" : "0") +
                                        @", " + "DIVIAT='" + values.diviation[i].ToString("F2", CultureInfo.InvariantCulture) +
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
                    // запись отсутствует, запоминаем значения
                    switch (m_modeTECComponent) {
                        case ChangeMode.MODE_TECCOMPONENT.GTP:
                            requestInsert += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + values.recommendations[i].ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + (values.diviationPercent[i] ? "1" : "0") +
                                        @", '" + values.diviation[i].ToString("F2", CultureInfo.InvariantCulture) +
                                        @"'),";
                            break;
                        case ChangeMode.MODE_TECCOMPONENT.PC:
                            requestInsert += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + values.recommendations[i].ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + (values.diviationPercent[i] ? "1" : "0") +
                                        @", '" + values.diviation[i].ToString("F2", CultureInfo.InvariantCulture) +
                                        @"', " + (comp.m_id) +
                                        @"),";
                            break;
                        default:
                            break;
                    }
                }
            }

            // добавляем все записи, не найденные в базе
            if (requestInsert != "")
            {
                switch (m_modeTECComponent)
                {
                    case ChangeMode.MODE_TECCOMPONENT.GTP:
                        requestInsert = @"INSERT INTO " + t.m_strUsedAdminValues + " (DATE, " + name + @"_REC" +
                                @", " + name + "_IS_PER" +
                                @", " + name + "_DIVIAT) VALUES" + requestInsert.Substring(0, requestInsert.Length - 1) + ";";
                        break;
                    case ChangeMode.MODE_TECCOMPONENT.PC:
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
                // запись для этого часа имеется, модифицируем её
                if (PPBRDates[i])
                {
                    switch (m_modeTECComponent)
                    {
                        case ChangeMode.MODE_TECCOMPONENT.GTP:
                            /*requestUpdate += @"UPDATE " + t.m_strUsedPPBRvsPBR + " SET " + name + @"_" + t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.REC] + "='" + values.plan[i].ToString("F1", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";*/
                            requestUpdate += @"UPDATE " + t.m_strUsedPPBRvsPBR + " SET " + name + @"_" + t.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR] + "='" + values.plan[i].ToString("F1", CultureInfo.InvariantCulture) +
                                        @"' WHERE " +
                                        @"DATE_TIME = '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"'; ";
                            break;
                        case ChangeMode.MODE_TECCOMPONENT.PC:
                            requestUpdate += @"UPDATE " + strUsedPPBRvsPBR + " SET " + @"PBR='" + values.plan[i].ToString("F2", CultureInfo.InvariantCulture) +
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
                    // запись отсутствует, запоминаем значения
                    switch (m_modeTECComponent)
                    {
                        case ChangeMode.MODE_TECCOMPONENT.GTP:
                            requestInsert += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + serverTime.Date.ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + "ПБР" + getPBRNumber(i) +
                                        @"', '" + "0" +
                                        @"', '" + values.plan[i].ToString("F1", CultureInfo.InvariantCulture) +
                                        @"'),";
                            break;
                        case ChangeMode.MODE_TECCOMPONENT.PC:
                            requestInsert += @" ('" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + serverTime.Date.ToString("yyyy-MM-dd HH:mm:ss") +
                                        @"', '" + "ПБР" + getPBRNumber(i) +
                                        @"', " + comp.m_id +
                                        @", '" + "0" +
                                        @"', " + values.plan[i].ToString("F1", CultureInfo.InvariantCulture) +
                                        @"),";
                            break;
                        default:
                            break;
                    }
                }
            }

            // добавляем все записи, не найденные в базе
            if (requestInsert != "")
            {
                switch (m_modeTECComponent)
                {
                    case ChangeMode.MODE_TECCOMPONENT.GTP:
                        requestInsert = @"INSERT INTO " + t.m_strUsedPPBRvsPBR + " (DATE_TIME, WR_DATE_TIME, PBR_NUMBER, IS_COMDISP, " + name + @"_PBR) VALUES" + requestInsert.Substring(0, requestInsert.Length - 1) + ";";
                        break;
                    case ChangeMode.MODE_TECCOMPONENT.PC:
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

            MainForm.log.LogLock();
            MainForm.log.LogToFile("SetPPBRRequest", true, true, false);
            MainForm.log.LogUnlock();
            
            //Request(m_indxDbInterfaceCommon, m_listenerIdCommon, requestUpdate + requestInsert + requestDelete);
            Request(t.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], t.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], requestUpdate + requestInsert + requestDelete);
        }

        private void GetPassRequest(bool disp)
        {
            string request = "SELECT PASSWORD_";

            if (disp)
                request += "DISP";
            else
                request += "ADMIN";

            request += " FROM TOOLS";
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

        private void SetPassRequest(string password, bool disp, bool insert)
        {
            string query = string.Empty;
            
            if (insert)
                if (disp)
                    query = "INSERT INTO TOOLS (PASSWORD_DISP) VALUES ('" + password + "')";
                else
                    query = "INSERT INTO TOOLS (PASSWORD_ADMIN) VALUES ('" + password + "')";
            else
                if (disp)
                    query =  "UPDATE TOOLS SET PASSWORD_DISP='" + password + "'";
                else
                    query = "UPDATE TOOLS SET PASSWORD_ADMIN='" + password + "'";

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
        //                case "ППБР": num = 0; break;
        //                default:
        //                    {
        //                        if (name.Substring(0, 3) == "ПБР" && int.TryParse(name.Substring(3), out num) == true && num > 0)
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
        //        // запись для этого часа имеется, модифицируем её
        //        if (layoutForLoading.existingHours[i])
        //        {
        //            if (layoutForLoading.name == "ППБР")
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
        //            // запись отсутствует, запоминаем значения
        //            requestInsert += @" (1, '" + layoutForLoading.name +
        //                             @"', '" + date.AddHours(i + 1).ToString("yyyy-MM-dd HH:mm:ss") +
        //                             @"', now()" +
        //                //HHH БТЭЦ
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
        //                //ТЭЦ-2
        //                //@", '" + layoutForLoading.m_arGTPs [(int) INDEX_TEC_PBR_VALUES.TEC2].SN[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC2].PBR[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC2].Pmax[i].ToString(CultureInfo.InvariantCulture) +
        //                             @"', '" + layoutForLoading.m_arGTPs[(int)INDEX_TEC_PBR_VALUES.TEC2].Pmin[i].ToString(CultureInfo.InvariantCulture) +
        //                //ТЭЦ-3
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
        //                //ТЭЦ-4
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
        //                //ТЭЦ-5
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
        //            //БТЭЦ
        //                        @"BTEC_PPBR, BTEC_PMAX, BTEC_PMIN, " +
        //                        @"BTEC_TG1_PBR, BTEC_TG1_PMAX, BTEC_TG1_PMIN, " +
        //                        @"BTEC_TG2_PPBR, BTEC_TG2_PMAX, BTEC_TG2_PMIN" +
        //                        @"BTEC_TG1_PBR, BTEC_TG1_PMAX, BTEC_TG1_PMIN, " +
        //                        @"BTEC_TG2_PPBR, BTEC_TG2_PMAX, BTEC_TG2_PMIN" +
        //            //ТЭЦ-2
        //                        @"TEC2_PPBR, TEC2_PMAX, TEC2_PMIN, " +
        //            //ТЭЦ-3
        //                        @"TEC3_PPBR, TEC3_PMAX, TEC3_PMIN, " +
        //                        @"TEC3_TG1_PPBR, TEC3_TG1_PMAX, TEC3_TG1_PMIN, " +
        //                        @"TEC3_TG5_PPBR, TEC3_TG5_PMAX, TEC3_TG5_PMIN, " +
        //                        @"TEC3_TG712_PPBR, TEC3_TG712_PMAX, TEC3_TG712_PMIN, " +
        //                        @"TEC3_TG1314_PPBR, TEC3_TG1314_PMAX, TEC3_TG1314_PMIN, " +
        //            //ТЭЦ-4
        //                        @"TEC4_PPBR, TEC4_PMAX, TEC4_PMIN, " +
        //                        @"TEC4_TG3_PBR, TEC4_TG3_PMAX, TEC4_TG3_PMIN, " +
        //                        @"TEC4_TG48_PPBR, TEC4_TG48_PMAX, TEC4_TG48_PMIN" +
        //            //ТЭЦ-5
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

        public void Activate(bool active)
        {
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

            cbxTec.Items.Clear ();
            allTECComponents.Clear ();

            foreach (TEC t in tec)
            {
                if (t.list_TECComponents.Count > 1)
                    foreach (TECComponent g in t.list_TECComponents)
                    {
                        cbxTec.Items.Add(t.name + " - " + g.name);
                        allTECComponents.Add(g);
                    }
                else
                {
                    cbxTec.Items.Add(t.name);
                    allTECComponents.Add(t.list_TECComponents[0]);
                }
            }
        }

        private void InitDbInterfaces () {
            m_listDbInterfaces.Clear ();

            m_listListenerIdCurrent.Clear();
            m_indxDbInterfaceCurrent = -1;

            m_listDbInterfaces.Add(new DbInterface(DbInterface.DbInterfaceType.MySQL));
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
                        m_listDbInterfaces.Add (new DbInterface (DbInterface.DbInterfaceType.MySQL));
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
                    ActionReport("Получение текущего времени сервера.");
                    GetCurrentTimeRequest();
                    break;
                case StatesMachine.PPBRValues:
                    ActionReport("Получение данных плана.");
                    GetPPBRValuesRequest(allTECComponents[oldTecIndex].tec, allTECComponents[oldTecIndex], dateForValues.Date);
                    break;
                case StatesMachine.AdminValues:
                    ActionReport("Получение административных данных.");
                    GetAdminValuesRequest(allTECComponents[oldTecIndex].tec, allTECComponents[oldTecIndex], dateForValues.Date);
                    oldDate = dateForValues.Date;
                    this.BeginInvoke(delegateCalendarSetDate, oldDate);
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
                case StatesMachine.SaveAdminValues:
                    ActionReport("Сохранение административных данных.");
                    SetAdminValuesRequest(allTECComponents[oldTecIndex].tec, allTECComponents[oldTecIndex], dateForValues);
                    break;
                case StatesMachine.SavePPBRValues:
                    ActionReport("Сохранение ПЛАНА.");
                    SetPPBRRequest(allTECComponents[oldTecIndex].tec, allTECComponents[oldTecIndex], dateForValues);
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
                //case StatesMachine.LayoutGet:
                //    ActionReport("Получение административных данных макета.");
                //    GetLayoutRequest(dateForValues);
                //    break;
                //case StatesMachine.LayoutSet:
                //    ActionReport("Сохранение административных данных макета.");
                //    SetLayoutRequest(dateForValues);
                //    break;
            }

            return result;
        }

        private bool StateCheckResponse(StatesMachine state, out bool error, out DataTable table)
        {
            if ((!(m_indxDbInterfaceCurrent < 0)) && (m_listListenerIdCurrent.Count > 0) && (! (m_indxDbInterfaceCurrent < 0)))
            {
                switch (state)
                {
                    case StatesMachine.CurrentTime:
                    case StatesMachine.PPBRValues:
                    case StatesMachine.AdminValues:
                    case StatesMachine.PPBRDates:
                    case StatesMachine.AdminDates:                    
                    case StatesMachine.SaveAdminValues:
                    case StatesMachine.SavePPBRValues:
                    //case StatesMachine.UpdateValuesPPBR:
                    case StatesMachine.GetPass:
                        return GetResponse(m_indxDbInterfaceCurrent, m_listListenerIdCurrent[m_indxDbInterfaceCurrent], out error, out table/*, false*/);
                    case StatesMachine.SetPassInsert:
                    case StatesMachine.SetPassUpdate:
                    //case StatesMachine.LayoutGet:
                    //case StatesMachine.LayoutSet:
                        return GetResponse(m_indxDbInterfaceCurrent, m_listListenerIdCurrent[m_indxDbInterfaceCurrent], out error, out table/*, true*/);
                    default:
                        error = true;
                        table = null;
                        
                        return false;
                }
            }
            else {
                //Ошибка???

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
                        this.BeginInvoke(delegateFillData, oldDate);
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
                case StatesMachine.PPBRValues:
                    if (response)
                        ErrorReport("Ошибка разбора данных плана. Переход в ожидание.");
                    else
                        ErrorReport("Ошибка получения данных плана. Переход в ожидание.");
                    break;
                case StatesMachine.AdminValues:
                    if (response)
                        ErrorReport("Ошибка разбора административных данных. Переход в ожидание.");
                    else
                        ErrorReport("Ошибка получения административных данных. Переход в ожидание.");
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
                case StatesMachine.SaveAdminValues:
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
                case StatesMachine.SavePPBRValues:
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
                //case StatesMachine.LayoutGet:
                //    if (response)
                //    {
                //        ErrorReport("Ошибка разбора административных данных макета. Переход в ожидание.");
                //        loadLayoutResult = Errors.ParseError;
                //    }
                //    else
                //    {
                //        ErrorReport("Ошибка получения административных данных макета. Переход в ожидание.");
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
                //    ErrorReport("Ошибка сохранения административных данных макета. Переход в ожидание.");
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
                            else
                                ;
                        }
                        else
                            ;

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
