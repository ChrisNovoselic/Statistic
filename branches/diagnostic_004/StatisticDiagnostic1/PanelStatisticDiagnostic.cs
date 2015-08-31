using System.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Timers;
using System.Threading;
using System.Windows.Forms; //TableLayoutPanel
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using HClassLibrary;
using StatisticCommon;
using System.Data.SqlClient;

namespace StatisticDiagnostic1
{
    public class PanelStatisticDiagnostic1 : PanelStatistic
    {
        private static int[] INDEX_SOURCE_GETDATA = {
            26 //Эталон - ne2844
            //Вариант №1
            , 1, 4, 7, 10, 13, /*16*/-1
            , 2, 5, 8, 11, 14, 17
            , 3, 6, 9, 12, 15, -1
            ////Вариант №2
            //, -1, -1, -1, -1, -1, /*16*/-1
            //, -1, -1, -1, -1, -1, 17
            //, -1, -1, -1, -1, -1, -1
                                      };

        public Modes[] m_arPanelsMODES;
        public Tec[] m_arPanelsTEC;
        public Task[] m_arPanelsTask;
        string[,] massiveServ;
        public DataTable tbModes = new DataTable();
        public DataTable m_tableSourceData;
        public DataTable table_task = new DataTable();
        public DataTable arraySourceDataTask = new DataTable();
        public DataTable arraySource = new DataTable();
        public DataTable arraySourceHT = new DataTable();
        public DataTable arraySourceDS = new DataTable();
        public DataTable tbTask = new DataTable();
        HDataSource m_DataSource;
        int counter = -1;
        private object m_lockTimerGetData;
        private object m_lockGetData;
        ConnectionSettings m_connSett;
        private System.Timers.Timer aTimer;
        public DataRow[] rows;

        public class HDataSource : HHandlerDb
        {
            PanelStatisticDiagnostic1 Parent;
            ConnectionSettings m_connSett;

            protected enum State
            {
                Command
            }

            public HDataSource(PanelStatisticDiagnostic1 parent)
            {
                this.Parent = parent;
            }

            public HDataSource(ConnectionSettings connSett)
            {
                m_connSett = connSett;
            }

            public override void StartDbInterfaces()
            {
                m_dictIdListeners.Add(0, new int[] { -1 });
                register(0, 0, m_connSett, m_connSett.name);
            }

            public override void ClearValues()
            {

            }

            public void Command()
            {
                lock (m_lockState)
                {
                    ClearStates();
                    AddState((int)State.Command);
                    Run(@"Command");
                }
            }

            /// <summary>
            /// Запросить результат для события
            /// </summary>
            /// <param name="state">Событие запроса</param>
            /// <returns>Признак отправления результата</returns>
            protected override int StateRequest(int state)
            {
                int iRes = 0;

                switch (state)
                {
                    case (int)State.Command:
                        Request(m_dictIdListeners[0][0], @"Select * from Source_Diagnostic");
                        break;
                    default:
                        break;
                }
                return iRes;
            }

            /// Получить результат обработки события
            /// </summary>
            /// <param name="state">Событие для получения результата</param>
            /// <param name="error">Признак ошибки при получении результата</param>
            /// <param name="outobj">Результат запроса</param>
            /// <returns>Признак получения результата</returns>
            protected override int StateCheckResponse(int state, out bool error, out object table)
            {
                int iRes = 0;
                error = true;
                table = null;

                switch (state)
                {
                    case (int)State.Command:
                        iRes = response(m_IdListenerCurrent, out error, out table);
                        break;
                    default:
                        break;
                }
                return iRes;
            }

            /// <summary>
            /// Делегат фун-ии 
            /// </summary>
            public event DelegateObjectFunc EvtRecievedTable;

            /// <summary>
            /// Обработка УСЕШНО полученного результата
            /// </summary>
            /// <param name="state">Состояние для результата</param>
            /// <param name="obj">Значение результата</param>
            /// <returns>Признак обработки результата</returns>
            protected override int StateResponse(int state, object table)
            {
                int iRes = 0;

                switch (state)
                {
                    case (int)State.Command:
                        EvtRecievedTable(table);
                        //TaskTable(table);
                        break;
                    default:
                        break;
                }
                return iRes;
            }

            protected override INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
            {
                INDEX_WAITHANDLE_REASON iRes = INDEX_WAITHANDLE_REASON.SUCCESS;
                return iRes;
            }

            protected override void StateWarnings(int state, int req, int res)
            {
                throw new NotImplementedException();
            }
        }

        //Tec tecdb = new Tec();
        //Modes modesdb;
        Task taskdb = new Task();

        /// <summary>
        /// Класс ТЭЦ
        /// </summary>
        public class Tec : HPanelCommon
        {
            public DataGridView TECDataGridView = new DataGridView();
            public Label LabelTec = new Label();

            public Tec()
                : base(-1, -1)
            {
                initialize();
            }

            public Tec(IContainer container)
                : base(container, -1, -1)
            {
                container.Add(this);

                initialize();
            }

            private void initialize()
            {
                InitializeComponentTEC();
            }

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly(cols, rows);
            }

            /// <summary>
            /// Требуется переменная конструктора.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary> 
            /// Освободить все используемые ресурсы.
            /// </summary>
            /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            #region Код, автоматически созданный конструктором компонентов

            /// <summary>
            /// Обязательный метод для поддержки конструктора - не изменяйте
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            private void InitializeComponentTEC()
            {
                this.Controls.Add(LabelTec, 0, 0);
                this.Controls.Add(TECDataGridView, 0, 1);
                this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
                this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.74641F));

                this.SuspendLayout();

                //this.TECDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.TECDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.TECDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
                this.TECDataGridView.AllowUserToAddRows = false;
                this.TECDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
                this.TECDataGridView.RowHeadersVisible = false;
                //this.TECDataGridView.AutoGenerateColumns = false;                  
                this.TECDataGridView.Name = "TECDataGridView";
                this.TECDataGridView.TabIndex = 0;

                this.LabelTec.AutoSize = true;
                //this.LabelTec.Size = new System.Drawing.Size(1, 5);
                //this.LabelTec.Dock = System.Windows.Forms.DockStyle.Fill;
                this.LabelTec.Name = "LabelTec";
                this.LabelTec.TabIndex = 0;
                this.LabelTec.Text = "Unknow_TEC";
                //this.LabelTec.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                this.LabelTec.TextAlign = System.Drawing.ContentAlignment.TopCenter;

                this.ResumeLayout(false);
            }

            #endregion
        }

        /// <summary>
        /// Класс МОДЕС
        /// </summary>
        public class Modes : HPanelCommon
        {
            public Modes()
                : base(-1, -1)
            {
                initialize();
            }

            public Modes(IContainer container)
                : base(container, -1, -1)
            {
                container.Add(this);

                initialize();
            }

            private void initialize()
            {
                InitializeComponentModes();
            }

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly(cols, rows);
            }

            /// <summary>
            /// Требуется переменная конструктора.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary> 
            /// Освободить все используемые ресурсы.
            /// </summary>
            /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            #region Код, автоматически созданный конструктором компонентов

            /// <summary>
            /// Обязательный метод для поддержки конструктора - не изменяйте
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            public DataGridView ModesDataGridView = new DataGridView();
            public Label LabelModes = new Label();

            private void InitializeComponentModes()
            {
                //this.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.OutsetPartial;
                //this.initializeLayoutStyle(1, 2);
                this.Controls.Add(LabelModes, 0, 0);
                this.Controls.Add(ModesDataGridView, 0, 1);
                this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
                this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.74641F));

                this.SuspendLayout();

                this.Dock = DockStyle.Fill;
                this.ModesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.ModesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.ModesDataGridView.Dock = DockStyle.Fill;
                this.ModesDataGridView.AllowUserToAddRows = false;
                this.ModesDataGridView.RowHeadersVisible = false;
                this.ModesDataGridView.Name = "ModesDataGridView";
                this.ModesDataGridView.TabIndex = 0;

                this.LabelModes.AutoSize = true;
                this.LabelModes.Dock = System.Windows.Forms.DockStyle.Fill;
                this.LabelModes.Name = "LabelModes";
                this.LabelModes.TabIndex = 1;
                this.LabelModes.Text = " ";
                //this.LabelModes.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                this.LabelModes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

                this.ResumeLayout(false);
            }
            #endregion
            public System.Windows.Forms.TableLayoutPanel MODESTableLayoutPanel;
        }

        /// <summary>
        /// Класс Задачи 
        /// </summary>
        public class Task : HPanelCommon
        {
            public DataGridView TaskDataGridView = new DataGridView();
            public CheckedListBox TaskCheckedListBox = new CheckedListBox();

            public Task()
                : base(-1, -1)
            {
                initialize();
            }

            public Task(IContainer container)
                : base(container, -1, -1)
            {
                container.Add(this);

                initialize();
            }

            private void initialize()
            {
                InitializeComponentTask();
            }

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly(cols, rows);
            }

            /// <summary>
            /// Требуется переменная конструктора.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary> 
            /// Освободить все используемые ресурсы.
            /// </summary>
            /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            #region Код, автоматически созданный конструктором компонентов

            /// <summary>
            /// Обязательный метод для поддержки конструктора - не изменяйте
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            private void InitializeComponentTask()
            {
                TaskCheckedListBox = new System.Windows.Forms.CheckedListBox();
                TaskDataGridView = new System.Windows.Forms.DataGridView();
                TaskTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();

                this.SuspendLayout();

                this.TaskTableLayoutPanel.RowCount = 1;
                this.TaskTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
                this.TaskTableLayoutPanel.ColumnCount = 2;
                this.TaskTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.74641F));
                this.TaskTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 77.25359F));
                this.TaskTableLayoutPanel.Dock = DockStyle.Fill;
                this.TaskTableLayoutPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
                this.TaskTableLayoutPanel.Name = "TaskTableLayoutPanel";
                this.TaskTableLayoutPanel.Controls.Add(TaskCheckedListBox, 0, 0);
                this.TaskTableLayoutPanel.Controls.Add(TaskDataGridView, 1, 0);

                this.TaskCheckedListBox.Dock = DockStyle.Fill;
                this.TaskCheckedListBox.FormattingEnabled = true;
                this.TaskCheckedListBox.Name = "TaskChekedListBox";
                this.TaskCheckedListBox.CheckOnClick = true;
                this.TaskCheckedListBox.TabIndex = 1;
                this.TaskCheckedListBox.Width = 700;

                this.TaskDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.TaskDataGridView.Dock = DockStyle.Fill;
                this.TaskDataGridView.Name = "TaskDataGridView";
                this.TaskDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.TaskDataGridView.RowHeadersVisible = false;
                this.TaskDataGridView.TabIndex = 0;
                this.TaskDataGridView.AllowUserToAddRows = false;

                this.ResumeLayout();
            }
            #endregion;

            public System.Windows.Forms.TableLayoutPanel TaskTableLayoutPanel;
        }

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly(cols, rows);
        }

        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            this.SuspendLayout();

            Tasklabel = new System.Windows.Forms.Label();
            TEClabel = new System.Windows.Forms.Label();
            Modeslabel = new System.Windows.Forms.Label();

            this.TecTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.TecTableLayoutPanel.Dock = DockStyle.Fill;
            this.TecTableLayoutPanel.AutoSize = true;
            this.TecTableLayoutPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.TecTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TecTableLayoutPanel.ColumnCount = 3;
            this.TecTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TecTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TecTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TecTableLayoutPanel.RowCount = 2;
            this.TecTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TecTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));

            /*this.TaskTableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.TaskTableLayoutPanel1.Dock = DockStyle.Fill;
            this.TaskTableLayoutPanel1.AutoSize = true;
            this.TaskTableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.TaskTableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TaskTableLayoutPanel1.ColumnCount = 2;
            this.TaskTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.74641F));
            this.TaskTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70.25359F));
            this.TaskTableLayoutPanel1.RowCount = 1;
            this.TaskTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));*/

            this.ModesTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.ModesTableLayoutPanel.Dock = DockStyle.Fill;
            this.ModesTableLayoutPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.ModesTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ModesTableLayoutPanel.AutoSize = true;
            this.ModesTableLayoutPanel.ColumnCount = 3;
            this.ModesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ModesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ModesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ModesTableLayoutPanel.RowCount = 2;
            this.ModesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ModesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));

            this.Controls.Add(TEClabel, 0, 0);
            this.Controls.Add(TecTableLayoutPanel, 0, 1);
            this.Controls.Add(Modeslabel, 0, 2);
            this.Controls.Add(ModesTableLayoutPanel, 0, 3);
            this.Controls.Add(Tasklabel, 0, 4);
            this.Controls.Add(taskdb.TaskTableLayoutPanel, 0, 5);

            this.Modeslabel.AutoSize = true;
            this.Modeslabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Modeslabel.Name = "Modeslabel";
            this.Modeslabel.TabIndex = 0;
            this.Modeslabel.Text = "Панель данных - МОДЕС";
            this.Modeslabel.Size = new System.Drawing.Size(10, 10);
            this.Modeslabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left))));

            //this.TEClabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TEClabel.Name = "TEClabel";
            this.TEClabel.TabIndex = 0;
            this.TEClabel.Text = "Панель данных - ТЭЦ";
            this.TEClabel.Size = new System.Drawing.Size(10, 10);
            this.TEClabel.AutoSize = true;
            this.TEClabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));

            this.Tasklabel.Size = new System.Drawing.Size(10, 10);
            this.Tasklabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tasklabel.Name = "Tasklabel";
            this.Tasklabel.TabIndex = 0;
            this.Tasklabel.AutoSize = true;
            this.Tasklabel.Text = "Список задач";
            this.Modeslabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left))));

            this.Dock = DockStyle.Fill;

            this.ColumnCount = 1;
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.RowCount = 6;
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));

            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();

            this.ResumeLayout();
        }

        #endregion

        private System.Windows.Forms.Label Tasklabel;
        private System.Windows.Forms.Label TEClabel;
        private System.Windows.Forms.Label Modeslabel;
        private System.Windows.Forms.TableLayoutPanel TecTableLayoutPanel;
        // private System.Windows.Forms.TableLayoutPanel TaskTableLayoutPanel1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.TableLayoutPanel ModesTableLayoutPanel;

        public PanelStatisticDiagnostic1()
        {
            initialize();
        }

        public PanelStatisticDiagnostic1(IContainer container)
        {
            initialize();
            container.Add(this);
        }

        /// <summary>
        /// Инициализация панели
        /// </summary>
        private void initialize()
        {
            InitializeComponent();
            m_lockTimerGetData = new object();
            m_lockGetData = new object();

            Conn();
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeBackgroundWorker()
        {
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            /*backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;*/
        }

        public void Start_BGW()
        {
            if (backgroundWorker1.IsBusy != true)
            {
                // Start the asynchronous operation.
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            System.Threading.Thread.Sleep(500);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Conn()
        {
            GetCurrentData();
            Start();
            GetDataTask();
        }

        /// <summary>
        /// Прикрепляет гриды к панеле Задачи
        /// </summary>
        /* public void AddPanelTask()
         {
             int i = -1;
             int indx = -1
                , col = -1
                , row = -1;

             for (i = 0; i < m_arPanelsTask.Length; i++)
             {
                 indx = i;
                 if (!(indx < this.RowCount))
                     indx += (int)(indx / TaskTableLayoutPanel1.RowCount);
                 //else ;

                 col = (int)(indx / TaskTableLayoutPanel1.RowCount);
                 row = indx % (TaskTableLayoutPanel1.RowCount - 0);
                 //if (row == 0) row = 1; else ;
                 if (TaskTableLayoutPanel1.InvokeRequired)
                     TaskTableLayoutPanel1.Invoke(new Action(() => TaskTableLayoutPanel1.Controls.Add(m_arPanelsTEC[i], col, row)));
                 //TaskTableLayoutPanel1.Controls.Add(m_arPanelsTask[i], col, row);
             }
         }*/

        /// <summary>
        /// Прикрепляет гриды к панеле ТЭЦ
        /// </summary>
        public void AddPanelTEC()
        {
            int i = -1;
            int indx = -1
               , col = -1
               , row = -1;

            for (i = 0; i < m_arPanelsTEC.Length; i++)
            {
                indx = i;
                if (!(indx < this.RowCount))
                    indx += (int)(indx / TecTableLayoutPanel.RowCount);
                //else ;

                col = (int)(indx / TecTableLayoutPanel.RowCount);
                row = indx % (TecTableLayoutPanel.RowCount - 0);
                //if (row == 0) row = 1; else ;
                if (TecTableLayoutPanel.InvokeRequired)
                    TecTableLayoutPanel.Invoke(new Action(() => TecTableLayoutPanel.Controls.Add(m_arPanelsTEC[i], col, row)));
                //TecTableLayoutPanel.Controls.Add(m_arPanelsTEC[i], col, row);
            }
        }

        /// <summary>
        /// Прикрепляет гриды к панеле МОДЕС
        /// </summary>
        public void AddPanelModes()
        {
            int i = -1;
            int indx = -1
               , col = -1
               , row = -1;

            for (i = 0; i < m_arPanelsMODES.Length; i++)
            {
                if (ModesTableLayoutPanel.RowCount * ModesTableLayoutPanel.ColumnCount < m_arPanelsMODES.Length)
                {
                    ModesTableLayoutPanel.Invoke(new Action(() => ModesTableLayoutPanel.RowCount++));
                    ModesTableLayoutPanel.Invoke(new Action(() => ModesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F))));
                }

                indx = i;
                if (!(indx < this.RowCount))
                {
                    //indx += (int)(indx / ModesTableLayoutPanel.RowCount);

                }
                //else ;

                col = (int)(indx / ModesTableLayoutPanel.RowCount);
                row = indx % (ModesTableLayoutPanel.RowCount - 0);
                //if (row == 0) row = 1; else ;
                //row = 5 / 1;
                if (ModesTableLayoutPanel.InvokeRequired)
                    ModesTableLayoutPanel.Invoke(new Action(() => ModesTableLayoutPanel.Controls.Add(m_arPanelsMODES[i], col, row)));
                ModesTableLayoutPanel.Invoke(new Action(() => ModesTableLayoutPanel.AutoScroll = true));
                //ModesTableLayoutPanel.Controls.Add(m_arPanelsMODES[i], col, row);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        private void dataSource_TableTec(object table)
        {
            m_tableSourceData = (DataTable)(table);
            tbTask = (DataTable)table;
            start_TEC();
            start_MODES();
            TimerPing();
        }

        /// <summary>
        /// Коннект к БД. создание экземпляра ДБинтерфейса
        /// </summary>
        /// <param name="connSett"></param>
        public void Start(ConnectionSettings connSett)
        {
            m_connSett = connSett;
            m_DataSource = new HDataSource((ConnectionSettings)m_connSett);
            m_DataSource.EvtRecievedTable += new DelegateObjectFunc(dataSource_TableTec);
            //m_DataSource.TaskTable += new DelegateObjectFunc(dataSource_TaskTable);
            taskdb.TaskCheckedListBox.SelectedIndexChanged += new EventHandler(TaskCheckedListBox_SelectedIndexChanged);
            m_DataSource.StartDbInterfaces();
            m_DataSource.Start();
            m_DataSource.Command();
            //m_lockTimerGetData = new object();
        }

        /// <summary>
        /// Запуск коннекта к БД
        /// </summary>
        public override void Start()
        {
            base.Start();

            Start(new ConnectionSettings()
            {
                id = -1
                ,
                name = @"DB_CONFIG"
                ,
                server = @"10.105.1.107"
                ,
                port = 1433
                ,
                dbName = @"techsite-2.X.X"
                ,
                userName = @"client1"
                ,
                password = @"client"
                ,
                ignore = false
            });
        }

        /// <summary>
        /// Старт TEC
        /// </summary>
        public void start_TEC()
        {
            Create_PanelTEC();
        }

        /// <summary>
        /// Старт MODES
        /// </summary>
        public void start_MODES()
        {
            Create_Modes();
        }

        /// <summary>
        /// Функция взятия информации из конф.БД
        /// </summary>
        public void GetCurrentData()
        {
            string connstr = "Data Source=ITC288; Initial Catalog=techsite_cfg-2.X.X;User ID=client1;Pwd=client; ";
            SqlConnection conn;
            conn = new SqlConnection(connstr);
            SqlDataAdapter adapterTEC;
            SqlDataAdapter adapterTASK;
            SqlDataAdapter adapterDS;
            SqlDataAdapter adapterHT;
            SqlDataAdapter adapterModes;

            try
            {
                conn.Open();
                adapterTEC = new SqlDataAdapter("SELECT * FROM TEC_LIST", conn);
                adapterTASK = new SqlDataAdapter("SELECT * FROM TASK_LIST", conn);
                adapterDS = new SqlDataAdapter("SELECT * FROM Keys_Datasheet", conn);
                adapterHT = new SqlDataAdapter("SELECT * FROM SOURCE", conn);
                adapterModes = new SqlDataAdapter("SELECT * FROM Modes_List", conn);

                adapterTASK.Fill(arraySourceDataTask);
                adapterHT.Fill(arraySourceHT);
                adapterDS.Fill(arraySourceDS);
                adapterTEC.Fill(arraySource);
                adapterModes.Fill(tbModes);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Создание панелей ТЭЦ
        /// </summary>
        public void Create_PanelTEC()
        {
            rows = arraySource.Select();

            m_arPanelsTEC = new Tec[rows.Length];

            for (int i = 0; i < rows.Length; i++)
            {
                m_arPanelsTEC[i] = new Tec();
            }
            AddPanelTEC();
            AddItemTec();
        }

        /// <summary>
        /// Создание панелей Модес
        /// </summary>
        /// <param name="table"></param>
        public void Create_Modes()
        {
            DataRow[] rows_modes;
            rows_modes = tbModes.Select();

            m_arPanelsMODES = new Modes[rows_modes.Length];

            for (int i = 0; i < rows_modes.Length; i++)
            {
                m_arPanelsMODES[i] = new Modes();
            }

            AddPanelModes();
            AddItemModes();
        }

        /// <summary>
        /// 
        /// </summary>
        public void MethodTEC()
        {
            TextColumnTec();
            ColumTimeTEC();
        }

        /// <summary>
        /// 
        /// </summary>
        public void MethodModes()
        {
            TextColumnModes();
            TimeModes();
        }

        /// <summary>
        /// 
        /// </summary>
        public void MethodTask(int x)
        {
            TextColumnTask(x);
            ColumTimeTask();
        }

        /// <summary>
        /// Запуск потока для панели TEC
        /// </summary>
        public void ThreadStartPanelTec()
        {
            var thread_TEC = new System.Threading.Thread(start_TEC);
            thread_TEC.Start();
        }

        /// <summary>
        /// Запуск потока для панели Task
        /// </summary>
        public void ThreadTASK()
        {
            //Thread thread_TASK = new System.Threading.Thread(start);
            //thread_TASK.Start();
        }

        /// <summary>
        /// Запуск потока для панели MODES
        /// </summary>
        public void ThreadModes()
        {
            //Thread thread_MODES = new System.Threading.Thread(start_Modes);
            //thread_MODES.Start();
        }

        /// <summary>
        /// Таймер обновления панелей
        /// </summary>
        public void TimerFillPanel()
        {
            //var timerTEC = new System.Threading.Timer(, null, 5000, 5000);
        }

        /// <summary>
        /// Функция Заполнения панелей Модес
        /// </summary>
        public void AddItemModes()
        {
            for (int i = 0; i < m_arPanelsMODES.Length; i++)
            {
                string filter = "ID_EXT = " + Convert.ToInt32(tbModes.Rows[i][0]);

                DataTable cloneTable = new DataTable();
                cloneTable = m_tableSourceData.Clone();

                foreach (DataRow row in m_tableSourceData.Select(filter))
                {
                    cloneTable.ImportRow(row);
                }

                if (m_arPanelsMODES[i].ModesDataGridView.InvokeRequired)
                {
                    m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.DataSource = cloneTable));
                }
            }

            HeaderTextModes();
            MethodModes();
        }

        /// <summary>
        /// Функция заполнения гридов
        /// </summary>
        public void AddItemTec()
        {
            for (int i = 0; i < m_arPanelsTEC.Length; i++)
            {
                string filter = "ID_EXT = " + Convert.ToInt32(rows[i][0]);
                //string str = arraySource.Rows[i][@"NAME_SHR"].ToString();

                DataTable cloneTable = new DataTable();
                cloneTable = m_tableSourceData.Clone();

                foreach (DataRow row in m_tableSourceData.Select(filter))
                {
                    cloneTable.ImportRow(row);
                }

                BindingSource bs = new BindingSource();
                bs.DataSource = cloneTable;

                if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                {
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.DataSource = cloneTable));
                    //m_arPanelsTEC[i].TECDataGridView.DataSource = bs;
                }
                int a = m_arPanelsTEC[i].TECDataGridView.Rows.Count;
                int c = cloneTable.Rows.Count;
            }
            MethodTEC();
        }

        /// <summary>
        /// Функция изменения заголовков грида Tec
        /// </summary>
        public void HeaderGridTEC()
        {
            for (int i = 0; i < m_arPanelsTEC.Length; i++)
            {
                string str = arraySource.Rows[i][@"NAME_SHR"].ToString();

                if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                {
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns["NAME_SHR"].HeaderText = "ТЭЦ"));
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns["ID"].Visible = false));
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[1].HeaderText = "Источник данных"));
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[2].HeaderText = "Крайнее значение"));
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[3].Visible = false));
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[6].HeaderText = "Связь"));
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[5].Visible = false));
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[4].HeaderText = "Время проверки"));
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[7].DisplayIndex = 0));
                }

                m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].LabelTec.Text = str));

                if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Update()));

            }

        }

        /// <summary>
        /// Функция изменения заголовков грида Modes
        /// </summary>
        public void HeaderTextModes()
        {
            for (int i = 0; i < m_arPanelsMODES.Length; i++)
            {
                string str = tbModes.Rows[i][@"NAME_SHR"].ToString();

                m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[7].HeaderText = "ТЭЦ"));
                m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[0].Visible = false));
                m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[1].HeaderText = "Источник данных"));
                m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[2].HeaderText = "Крайнее значение"));
                m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[3].Visible = false));
                m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[6].HeaderText = "Связь"));
                m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[5].Visible = false));
                m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[4].HeaderText = "Время проверки"));
                m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[7].DisplayIndex = 0));

                if (m_arPanelsMODES[i].LabelModes.InvokeRequired)
                    m_arPanelsMODES[i].LabelModes.Invoke(new Action(() => m_arPanelsMODES[i].LabelModes.Text = str));
            }
        }

        /// <summary>
        /// Функция изменения заголовков грида Task
        /// </summary>
        public void HeaderTextTask()
        {
            taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.Columns[7].HeaderText = "Имя задачи"));
            taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.Columns[0].Visible = false));
            taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.Columns[1].HeaderText = "Тип"));
            taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.Columns[2].HeaderText = "Крайнее значение(сек)"));
            taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.Columns[3].Visible = false));
            taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.Columns[6].Visible = false));
            taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.Columns[5].Visible = false));
            taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.Columns[4].HeaderText = "Время проверки"));
            taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.Columns[7].DisplayIndex = 0));
        }

        /// <summary>
        /// Функция перемеинования ячейки датагрид TEC
        /// </summary>
        public void TextColumnTec()
        {
            for (int k = 0; k < arraySource.Rows.Count; k++)
            {
                for (int j = 0; j < m_arPanelsTEC[k].TECDataGridView.Rows.Count; j++)
                {
                    string text1;
                    string text2 = m_arPanelsTEC[k].TECDataGridView.Rows[j].Cells[@"ID_Value"].Value.ToString();

                    int s = 0;

                    do
                    {
                        text1 = arraySourceDS.Rows[s][@"ID"].ToString();

                        s++;
                    }

                    while (text1 != text2);

                    string text = arraySourceDS.Rows[s - 1][@"DESCRIPTION"].ToString();

                    if (m_arPanelsTEC[k].TECDataGridView.InvokeRequired)
                        m_arPanelsTEC[k].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[k].TECDataGridView.Rows[j].Cells[@"ID_Value"].Value = text.ToString()));

                    //m_arPanelsTEC[k].TECDataGridView.Rows[j].Cells[@"ID_Value"].Value = text.ToString();
                }
            }
        }

        /// <summary>
        /// Функция перемеинования ячейки датагрид Modes
        /// </summary>
        public void TextColumnModes()
        {
            for (int k = 0; k < m_arPanelsMODES.Length; k++)
            {
                for (int j = 0; j < m_arPanelsMODES[k].ModesDataGridView.Rows.Count; j++)
                {
                    string text1;
                    string text2 = m_arPanelsMODES[k].ModesDataGridView.Rows[j].Cells[@"ID_Value"].Value.ToString();

                    int s = 0;

                    do
                    {
                        text1 = arraySourceDS.Rows[s][@"ID"].ToString();

                        s++;
                    }

                    while (text1 != text2);

                    string text = arraySourceDS.Rows[s - 1][@"DESCRIPTION"].ToString();

                    if (m_arPanelsMODES[k].ModesDataGridView.InvokeRequired)
                        m_arPanelsMODES[k].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[k].ModesDataGridView.Rows[j].Cells[@"ID_Value"].Value = text.ToString()));

                    //m_arPanelsMODES[k].ModesDataGridView.Rows[j].Cells[@"ID_Value"].Value = text.ToString();
                }
            }
        }

        /// <summary>
        ///  Функция перемеинования ячейки датагрид Task
        /// </summary>
        public void TextColumnTask(int indx)
        {
            string text1;
            string text2 = taskdb.TaskDataGridView.Rows[indx].Cells[@"ID_Value"].Value.ToString();

            int s = 0;

            do
            {
                text1 = arraySourceDS.Rows[s][@"ID"].ToString();

                s++;
            }

            while (text1 != text2);

            string text = arraySourceDS.Rows[s - 1][@"DESCRIPTION"].ToString();

            /*if (taskdb.TaskDataGridView.InvokeRequired)
                taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.Rows[j].Cells[@"ID_Value"].Value = text.ToString()));*/

            taskdb.TaskDataGridView.Rows[indx].Cells[@"ID_Value"].Value = text.ToString();

        }

        /// <summary>
        /// Функция заполнения ячеек грида ТЭЦ верменем
        /// </summary>
        /// <param name="table"></param>
        public void ColumTimeTEC()
        {
            string text = DateTimeNow();

            for (int i = 0; i < m_arPanelsTEC.Length; i++)
            {
                for (int j = 0; j < m_arPanelsTEC[i].TECDataGridView.Rows.Count; j++)
                {
                    //m_arPanelsTEC[i].TECDataGridView.Rows[j].Cells[@"UPDATEDATETIME"].Value = text.ToString();
                    if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                        m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Rows[j].Cells[@"UPDATEDATETIME"].Value = text.ToString()));
                }
            }

            HeaderGridTEC();
        }

        /// <summary>
        /// Функция заполнения ячеек грида Task верменем
        /// </summary>
        public void ColumTimeTask()
        {
            string text = DateTimeNow();

            for (int j = 0; j < taskdb.TaskDataGridView.Rows.Count; j++)
            {
                taskdb.TaskDataGridView.Rows[j].Cells[@"UPDATEDATETIME"].Value = text.ToString();
            }
        }

        /// <summary>
        /// Функция заполнения ячеек грида MODES верменем
        /// </summary>
        public void TimeModes()
        {
            string text = DateTimeNow();

            for (int i = 0; i < m_arPanelsMODES.Length; i++)
            {
                for (int j = 0; j < m_arPanelsMODES[i].ModesDataGridView.Rows.Count; j++)
                {
                    m_arPanelsMODES[i].ModesDataGridView.Rows[j].Cells[@"UPDATEDATETIME"].Value = text.ToString();
                    /* if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                         m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Rows[j].Cells[@"UPDATEDATETIME"].Value = text.ToString()));*/
                }
            }
        }

        /// <summary>
        /// Список адресов источников данных
        /// </summary>
        List<string> hosts = new List<string>();

        /// <summary>
        /// Функция заполнения списка адресами
        /// </summary>
        public void GetHost()
        {
            hosts.Sort();

            foreach (DataRow row in arraySourceHT.Select())
            {
                hosts.Add(row.ToString());
            }
        }

        /// <summary>
        /// Функция нахождения актуального времени
        /// </summary>
        public string DateTimeNow()
        {
            string time_now;
            //string timeutc_now = DateTimeOffset.UtcNow.UtcDateTime.ToString();
            return time_now = DateTime.Now.ToString();
        }

        /// <summary>
        /// Выборка из таблицы задачи
        /// </summary>
        public void GetDataTask()
        {
            for (int j = 0; j < arraySourceDataTask.Rows.Count; j++)
            {
                AddSourceDataTask(arraySourceDataTask.Rows[j][@"NAME_SHR"].ToString());
            }


        }

        /// <summary>
        /// Обновление гридов
        /// </summary>
        public void RefreshPanel()
        {
            //ConnTEC();

            for (int i = 0; i < m_arPanelsTEC.Length; i++)
            {
                if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Refresh()));

                //m_arPanelsTEC[i].TECDataGridView.Refresh();
            }
        }

        /// <summary>
        /// Остановка работы панели
        /// </summary>
        public override void Stop()
        {
            Activate(false);
        }

        /// <summary>
        /// Событие для чекбокса
        /// </summary>
        public void TaskCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string curItem = taskdb.TaskCheckedListBox.SelectedItem.ToString();
            string filter_task = "NAME_SHR = '" + curItem + "'";

            if (taskdb.TaskCheckedListBox.GetItemChecked(taskdb.TaskCheckedListBox.SelectedIndex) == true)
            {
                if (taskdb.TaskCheckedListBox.CheckedIndices.Count > 1)
                {
                    foreach (DataRow rowTask in tbTask.Select(filter_task))
                    {
                        table_task.ImportRow(rowTask);
                    }

                    taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.DataSource = table_task));
                    counter++;
                }

                else
                {
                    foreach (DataRow rowsTec in tbTask.Select(filter_task))
                    {
                        counter++;
                        table_task = tbTask.Clone();
                        table_task.ImportRow(rowsTec);

                        taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.DataSource = table_task));

                        taskdb.TaskDataGridView.Update();
                    }

                }

                MethodTask(counter);
                HeaderTextTask();
            }

            else
            {
                string curItemDel = taskdb.TaskCheckedListBox.SelectedItem.ToString();
                string filterDel = "NAME_SHR = '" + curItemDel + "'";
                //int indexDel = taskdb.TaskCheckedListBox.SelectedIndex;
                counter--;

                foreach (DataRow rowDel in table_task.Select(filterDel))
                {
                    table_task.Rows.Remove(rowDel);
                }

                taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.DataSource = table_task));
            }
        }

        /// <summary>
        /// Добавление результата пропинговки в грид
        /// </summary>
        /// <param name="x"></param>
        public void CellsPing(object x)
        {
            for (int k = 0; k < m_arPanelsTEC.Length; k++)
            {
                for (int i = 0; i < m_arPanelsTEC[k].TECDataGridView.Rows.Count; i++)
                {
                    int s = -1;
                    string text1;
                    string text2 = massiveServ[i,1];

                    do
                    {
                        s++;
                        text1 = arraySourceHT.Rows[s][@"NAME_SHR"].ToString();
                    }

                    while (text1 != text2);

                    if (m_arPanelsTEC[k].TECDataGridView.InvokeRequired)
                        m_arPanelsTEC[k].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[k].TECDataGridView.Rows[i].Cells[6].Value = massiveServ[i,0]));
                }
            }
        }

        /// <summary>
        /// Пропинговка ИД
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public string Ping(string server)
        {
            string str = null;
            Ping pingsender = new Ping();

            IPStatus status = IPStatus.Unknown;

            try
            {
                status = pingsender.Send(server).Status;

                if (status == IPStatus.Success)
                {
                    return str = "В сети";
                }

                else
                {
                    return status.ToString();
                }
            }

            catch
            {
                return str = "Проверка не удалась";
            }
        }

        /// <summary>
        /// Формирование списка ответов ip
        /// </summary>
        public void PingSourceData()
        {
            string server;
            DataTable testTB = new DataTable();
            string strokavst;
            string[,] massiveServ = new string[arraySourceHT.Rows.Count,2];
            string[] massive = new string[arraySourceHT.Rows.Count];
            //GetHost();

            for (int s = 0; s < arraySourceHT.Rows.Count; s++)
            {
                server = (string)arraySourceHT.Rows[s][@"IP"];
                strokavst = Ping(server);
                massive.SetValue(strokavst, s);
            }

            for (int c = 0; c < 2; c++)
            {
                if (c == 0)
                {
                    for (int r = 0; r < arraySourceHT.Rows.Count; r++)
                    {
                        massiveServ[r, c] = massive[r];
                    }
                }

                else
                {
                    for (int r = 0; r < arraySourceHT.Rows.Count; r++)
                    {
                        massiveServ[r, c] = (string)arraySourceHT.Rows[r][@"NAME_SHR"];
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void TimerPing()
        {
          aTimer = new System.Timers.Timer(10000);
          aTimer.Enabled = true;
          aTimer.AutoReset = true;
          aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
          aTimer.Interval = 300000;
          GC.KeepAlive(aTimer);
        }

        /// <summary>
        /// Событие таймера по пропинговки
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void OnTimedEvent(object source, ElapsedEventArgs e)
        {
           PingSourceData();
        }

        /// <summary>
        /// Фукнция заполнения чекбокса задачи
        /// </summary>
        public void AddSourceDataTask(string desc)
        {
            /*if (taskdb.TaskCheckedListBox.InvokeRequired)
                taskdb.TaskCheckedListBox.Invoke(new Action(() =>
            taskdb.TaskCheckedListBox.Items.Add(desc)));*/

            taskdb.TaskCheckedListBox.Items.Add(desc);
        }

        /// <summary>
        /// Класс пропинговки источников данных
        /// </summary>
        class MultiplePing
        {
            readonly int _repeat;
            static byte[] defaultPingData;
            const int TIMEOUT = 5000;

            static MultiplePing()
            {
                defaultPingData = new byte[32];
                for (int i = 0; i < defaultPingData.Length; i++)
                    defaultPingData[i] = (byte)(97 + i % 23);
            }

            public MultiplePing(int repeat = 4)
            {
                if (repeat < 1) throw new ArgumentOutOfRangeException("repeat", "repeat must be greater than zero");

                _repeat = repeat;
            }

            public MultiplePingReply Send(string hostNameOrAddress)
            {
                using (var ping = new Ping())
                {
                    int countSuccess = 0, countFailure = 0;
                    for (int i = 0; i < _repeat; i++)
                    {
                        try
                        {
                            PingReply reply = ping.Send(hostNameOrAddress, TIMEOUT, defaultPingData);
                            if (reply.Status == IPStatus.Success) countSuccess++;
                            else countFailure++;
                        }
                        catch (PingException)
                        {
                            countFailure++;
                        }
                    }

                    Debug.Assert(countSuccess + countFailure == _repeat);

                    return new MultiplePingReply(countSuccess, countFailure);
                }
            }
        }

        /// <summary>
        /// Класс для повтора пинга источников данных
        /// </summary>
        class MultiplePingReply
        {
            int _countSuccess, _countFailure;
            public MultiplePingReply(int success, int failure)
            {
                if (success < 0) throw new ArgumentOutOfRangeException("success", "HAPPY");
                if (failure < 0) throw new ArgumentOutOfRangeException("failure", "failure must be positive number or zero");
                if (success == 0 && failure == 0) throw new ArgumentException("success and failure cannot be both zero");

                _countSuccess = success;
                _countFailure = failure;
            }

            public bool AllSuccess { get { return _countFailure == 0; } }
            public bool AllFailed { get { return _countSuccess == 0; } }
        }
    }
}



