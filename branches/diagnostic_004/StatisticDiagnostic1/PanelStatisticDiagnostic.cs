using System.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
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
        public DataTable m_tableSourceData;
        //private DataTable m_CheckList;
        public DataTable arraySourceDataTask = new DataTable();
        public DataTable arraySource = new DataTable();
        public DataTable arraySourceHT = new DataTable();
        public DataTable tbTask = new DataTable();
        public DataTable tbTec = new DataTable();
        public DataTable tbHost = new DataTable();
        public DataTable arraySourceS = new DataTable();
        DataTable arraySourceInfo = new DataTable();
        //private System.ComponentModel.BackgroundWorker backgroundWorker1;
        public DataTable Table_Task = new DataTable();
        HDataSource m_DataSource;
        private object m_lockTimerGetData;
        ConnectionSettings m_connSett;
        public DataRow[] rows;
        public Tec[] m_arPanelsTEC;
        public Task[] m_arPanelsTask;
        public DataTable arrayTEC = new DataTable();
        public DataTable arraySourceDS = new DataTable();
        public DataRow[] rowsTec;
        //private System.Threading.Timer m_timer;

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
            /// Делегат функции заполнения панелей ТЭЦ
            /// </summary>
            public event DelegateObjectFunc EvtRecievedTable;

            /// <summary>
            /// Делегат функции извлечения информации о задач
            /// </summary>
            public DelegateObjectFunc TaskTable;

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
                        TaskTable(table);
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
        //Modes modesdb = new Modes();
        Task taskdb = new Task();

        /// <summary>
        /// Событие для чекбокса
        /// </summary>
        public void TaskCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //MessageBox.Show("You are in the CheckedListBox.Click event.");

            //PanelStatisticDiagnostic1 panelDiag = new PanelStatisticDiagnostic1();
            if (taskdb.TaskCheckedListBox.GetItemChecked(taskdb.TaskCheckedListBox.SelectedIndex) == true)
            {
                string curItem = taskdb.TaskCheckedListBox.SelectedItem.ToString();
                string filter_task = "NAME_SHR = '" + curItem + "'";
                //DataRow[] row_task;
                //row_task = panelDiag.arraySourceDataTask.Select(filter_task);
                DataTable table_task = new DataTable();
                table_task = tbTec.Clone();

                foreach (DataRow rowsTec in tbTask.Select(filter_task))
                {
                    table_task.ImportRow(rowsTec);

                    taskdb.TaskDataGridView.DataSource = table_task;
                    //taskdb.TaskDataGridView.Rows.Add(rowsTec);
                    taskdb.TaskDataGridView.Update();
                    ColumTimeTask();
                    /*if (taskdb.TaskDataGridView.InvokeRequired)
                    {
                        taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.DataSource = table_task));
                    }

                    if (taskdb.TaskDataGridView.InvokeRequired)
                        taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.Update()));*/
                }
            }
        }

        /// <summary>
        /// Добавление результата пропинговки в грид
        /// </summary>
        /// <param name="x"></param>
        public void CellsPing(object x)
        {
            string status;

            for (int k = 0; k < m_arPanelsTEC.Length; k++ )
            {
                for (int i = 0; i < m_arPanelsTEC[k].TECDataGridView.Rows.Count; i++)
                {
                    int s = -1;
                    string text1;
                    string text2 = m_arPanelsTEC[k].TECDataGridView.Rows[i].Cells[7].Value.ToString();  

                    do
                    {
                        s++;
                        text1 = arraySourceHT.Rows[s][@"NAME_SHR"].ToString();
                    }

                    while(text1 != text2);
                        
                        string server = (string)arraySourceHT.Rows[s][@"IP"];
                        status = PingSourceData(server);

                        if (m_arPanelsTEC[k].TECDataGridView.InvokeRequired)
                            m_arPanelsTEC[k].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[k].TECDataGridView.Rows[i].Cells[6].Value = status));
                }
            }
        }

        /// <summary>
        /// Пропинговка ИД
        /// </summary>
        public string PingSourceData(string server)
        {
            string str = null;

            Ping pingsender = new Ping();

                //IPAddressCollection IpAdress
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
        /// Запуск таймера для пинговки ИД
        /// </summary>
        public void PingTimerThread()
        {
            var timer = new System.Threading.Timer(CellsPing, null, 0, 50000);
        }

        /// <summary>
        /// Создание потока таймера
        /// </summary>
        /// <param name="x"></param>
        public void ThreadPing()
        {
            Thread thread_ping = new Thread(CellsPing);
            thread_ping.Start();
        }

        /// <summary>
        /// Фукнция заполнения чекбокса задачи
        /// </summary>
        public void AddSourceDataTask(string desc)
        {
            if (taskdb.TaskCheckedListBox.InvokeRequired)
                taskdb.TaskCheckedListBox.Invoke(new Action(() => taskdb.TaskCheckedListBox.Items.Add(desc)));
                
            //taskdb.TaskCheckedListBox.Items.Add(desc);
        }

        /*public string GetSelectedCheckBoxListTask()
        {
            return taskdb.TaskCheckedListBox.SelectedItem.ToString();
        }*/

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

                this.TECDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.TECDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.TECDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
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
            private DataGridView ModesDataGridView = new DataGridView();
            private Label LabelModes = new Label();

            private void InitializeComponentModes()
            {
                //this.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
                this.initializeLayoutStyle(1, 2);
                //this.RowStyles.
                this.Controls.Add(LabelModes, 0, 0);
                this.Controls.Add(ModesDataGridView, 0, 1);

                this.SuspendLayout();

                this.Dock = DockStyle.Fill;
                this.ModesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.ModesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.ModesDataGridView.Dock = DockStyle.Fill;
                this.ModesDataGridView.RowHeadersVisible = false;
                this.ModesDataGridView.Name = "ModesDataGridView";
                this.ModesDataGridView.TabIndex = 0;

                this.LabelModes.AutoSize = true;
                //this.LabelModes.Dock = System.Windows.Forms.DockStyle.Fill;
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
                this.TaskTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23.74641F));
                this.TaskTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 83.25359F));
                this.TaskTableLayoutPanel.Dock = DockStyle.Fill;
                //TaskTableLayoutPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
                this.TaskTableLayoutPanel.Name = "TaskTableLayoutPanel";
                this.TaskTableLayoutPanel.Controls.Add(TaskCheckedListBox, 0, 0);
                this.TaskTableLayoutPanel.Controls.Add(TaskDataGridView, 1, 0);

                this.TaskCheckedListBox.Dock = DockStyle.Fill;
                this.TaskCheckedListBox.FormattingEnabled = true;
                //this.TaskCheckedListBox.SelectedIndexChanged += new EventHandler(TaskCheckedListBox_SelectedIndexChanged);
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

            //TopLeftHeaderCell.Value 
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

            this.TaskTableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.TaskTableLayoutPanel1.Dock = DockStyle.Fill;
            this.TaskTableLayoutPanel1.AutoSize = true;
            this.TaskTableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.TaskTableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TaskTableLayoutPanel1.ColumnCount = 2;
            this.TaskTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23.74641F));
            this.TaskTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 83.25359F));
            this.TaskTableLayoutPanel1.RowCount = 1;
            this.TaskTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
    

            /*this.ModesTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.ModesTableLayoutPanel.Dock = DockStyle.Fill;
            this.ModesTableLayoutPanel.ColumnCount = 3;
            this.ModesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.ModesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.ModesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.ModesTableLayoutPanel.RowCount = 2;
            this.ModesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.ModesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));*/

            this.Controls.Add(TEClabel, 0, 0);
            this.Controls.Add(TecTableLayoutPanel, 0, 1);
            this.Controls.Add(Modeslabel, 0, 2);
            //this.Controls.Add(ModesTableLayoutPanel, 0, 3);
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
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();

            this.ResumeLayout();
        }

        #endregion

        private System.Windows.Forms.Label Tasklabel;
        private System.Windows.Forms.Label TEClabel;
        private System.Windows.Forms.Label Modeslabel;
        private System.Windows.Forms.TableLayoutPanel TecTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel TaskTableLayoutPanel1;

        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        //private System.Windows.Forms.TableLayoutPanel ModesTableLayoutPanel;

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
            ConnTEC();
           
        }

        /// <summary>
        /// versia 2.0???
        /// </summary>
        /// <param name="table"></param>
        public void Create_Task(DataTable table)
        {
            DataRow[] rows_task; 
               rows_task = table.Select();

            m_arPanelsTask = new Task[rows_task.Length];

            for (int i = 0; i < rows.Length; i++)
            {
                m_arPanelsTask[i] = new Task();
            }
        }

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

        public void ConnTEC()
        {
           
            GetCurrentData();
            Create_arPanelTEC(arraySource);
            Start();
        
        }

        /// <summary>
        /// Прикрепляет гриды к панеле Задачи
        /// </summary>
        public void FillPanelTask()
        {
            int i = -1;
            int indx = -1
               , col = -1
               , row = -1;

            for (i = 0; i < m_arPanelsTask.Length; i++)
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

        private void dataSource_TableTec(object table)
        {
            AddSourceDataTEC((DataTable)table);
            tbTec = (DataTable)table;
        }

        /// <summary>
        /// функция получения данных о времени выполнения задач
        /// </summary>
        private void dataSource_TaskTable(object table)
        {
            tbTask = (DataTable)table;
        }

        public void Start(ConnectionSettings connSett)
        {
            m_connSett = connSett;
            m_DataSource = new HDataSource((ConnectionSettings)m_connSett);
            m_DataSource.EvtRecievedTable += new DelegateObjectFunc(dataSource_TableTec);
            m_DataSource.TaskTable += new DelegateObjectFunc(dataSource_TaskTable);
            taskdb.TaskCheckedListBox.SelectedIndexChanged += new EventHandler(TaskCheckedListBox_SelectedIndexChanged);
            m_DataSource.StartDbInterfaces();
            m_DataSource.Start();
            m_DataSource.Command();
            m_lockTimerGetData = new object();
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
        /// Старт thread
        /// </summary>
        public void start(object i)
        {
            RefreshPanel();
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
            //SqlDataAdapter adapterS;

            try
            {
                conn.Open();
                adapterTEC = new SqlDataAdapter("SELECT * FROM TEC_LIST", conn);
                adapterTASK = new SqlDataAdapter("SELECT * FROM TASK_LIST", conn);
                adapterDS = new SqlDataAdapter("SELECT * FROM Keys_Datasheet", conn);
                adapterHT = new SqlDataAdapter("SELECT * FROM SOURCE", conn);
                //adapterS = new SqlDataAdapter("SELECT * FROM Host_Table", conn);

                adapterTASK.Fill(arraySourceDataTask);
                adapterHT.Fill(arraySourceHT);
                adapterDS.Fill(arraySourceDS);
                adapterTEC.Fill(arraySource);
                //adapterS.Fill(arraySourceS);

                //int i = arraySource.Rows.Count;
                
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
        public void Create_arPanelTEC(DataTable table)
        {
            rows = table.Select();

            m_arPanelsTEC = new Tec[rows.Length];

            for (int i = 0; i < rows.Length; i++)
            {
                m_arPanelsTEC[i] = new Tec();
            }
        }

        /// <summary>
        /// Звпуск заполнения гридов
        /// </summary>
        /// <param name="tbT"></param>
        public void AddSourceDataTEC(DataTable tbT)
        {
            AddTecItemMethod(tbT);
            Method();
            PingTimerThread();
            GetDataTask();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Method()
        {
            TextColumnTec();
            ColumTimeTEC();
            AddPanelTEC();
        }

        /// <summary>
        /// Запуск потока для панели
        /// </summary>
        public void ThreadStartPanelTec(object x)
        {
                var thread = new System.Threading.Thread(start);
                thread.Start();
        }

        /// <summary>
        /// Таймер обновления панелей
        /// </summary>
        public void TimerFillPanel()
        {
            var timerTEC = new System.Threading.Timer(ThreadStartPanelTec, null, 5000, 5000);
        }

        /// <summary>
        /// Функция Заполнения панелей ТЭЦ
        /// </summary>
        public void AddTecItemMethod(DataTable tableTec)
        {
            for (int i = 0; i < m_arPanelsTEC.Length; i++)
            {
                string filter = "ID_EXT = " + Convert.ToInt32(rows[i][0]);
                string str = arraySource.Rows[i][@"NAME_SHR"].ToString();

                DataTable cloneTable = new DataTable();
                cloneTable = tableTec.Clone();

                foreach (DataRow row in tableTec.Select(filter))
                {
                    cloneTable.ImportRow(row);
                }

                if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                {
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.DataSource = cloneTable));
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[7].HeaderText = "ТЭЦ"));
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[0].Visible = false));
                    //m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[1] = ));
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[1].HeaderText = "Источник данных"));
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[2].HeaderText = "Крайнее значение"));
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[3].Visible = false));
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[6].HeaderText = "Связь"));
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[5].Visible = false));
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[4].HeaderText = "Время проверки"));
                    //m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[4] = ));

                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[7].DisplayIndex = 0));
                }

                if (m_arPanelsTEC[i].LabelTec.InvokeRequired)
                    m_arPanelsTEC[i].LabelTec.Invoke(new Action(() => m_arPanelsTEC[i].LabelTec.Text = str));


                if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Update()));
            }
        }

        /// <summary>
        /// Функция Заполнения ячейки датагрид TEC
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
            //MessageBox.Show("true");
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
                    if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                        m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Rows[j].Cells[@"UPDATEDATETIME"].Value = text.ToString()));
                }
            }

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
        /// Список адресов источников данных
        /// </summary>
        List<string> hosts = new List<string>();

        /// <summary>
        /// Функция заполнения списка адресами
        /// </summary>
        public void GetHost()
        {
            hosts.Sort();

            foreach (DataRow row in arraySourceHT.Select("IP"))
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
            ConnTEC();

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



