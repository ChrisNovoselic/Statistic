using System.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Timers;
using System.Threading;
using System.Windows.Forms; //TableLayoutPanel
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using HClassLibrary;
using StatisticCommon;

namespace StatisticDiagnostic1
{
    public partial class PanelStatisticDiagnostic1
    {
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
            this.TEClabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left))));

            this.Tasklabel.Size = new System.Drawing.Size(10, 10);
            this.Tasklabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tasklabel.Name = "Tasklabel";
            this.Tasklabel.TabIndex = 0;
            this.Tasklabel.AutoSize = true;
            this.Tasklabel.Text = "Список задач";
            this.Tasklabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left))));

            this.Dock = DockStyle.Fill;

            this.ColumnCount = 1;
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.RowCount = 6;
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));

            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();

            this.ResumeLayout();
        }

        #endregion

        private System.Windows.Forms.Label Tasklabel;
        private System.Windows.Forms.Label TEClabel;
        private System.Windows.Forms.Label Modeslabel;
        private System.Windows.Forms.TableLayoutPanel TecTableLayoutPanel;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.TableLayoutPanel ModesTableLayoutPanel;
    }

    public partial class PanelStatisticDiagnostic1 : PanelStatistic
    {
        static string[,] massiveServ;
        static object[,] massivetext;
        static object[,] massTM;
        static Modes[] m_arPanelsMODES;
        static Tec[] m_arPanelsTEC;
        enum Units : int { Value = 12, Date };
        static DataTable tbModes = new DataTable();
        static DataTable m_tableSourceData;
        public DataTable table_task = new DataTable();
        public DataTable arraySourceDataTask = new DataTable();
        static DataTable arraySource = new DataTable();
        static DataTable arraySourceTEC = new DataTable();
        static DataTable arrayKeys_DataSheet = new DataTable();

        static HDataSource m_DataSource;
        int counter = -1;
        protected static FileINI m_sFileINI;
        static System.Timers.Timer UpdateTimer;
        ConnectionSettings m_connSett;

        FormWait fw = new FormWait();
        Task taskdb = new Task();
        static Modes modesdb = new Modes();
        static Tec tecdb = new Tec();

        /// <summary>
        /// Класс для передачи данных в классы
        /// </summary>
        public static class TransferData
        {
            public delegate void MyEvent(object data);
            public static MyEvent EventHandler;
        }

        /// <summary>
        /// 
        /// </summary>
        public void startUp()
        {
            RefreshDataSource();
            GridsUp();
        }

        /// <summary>
        /// Перегрузка информации из бд
        /// </summary>
        public void RefreshDataSource()
        {
            Start();
            PingSourceData();
        }

        /// <summary>
        /// Обновление данных гридов
        /// </summary>
        public void GridsUp()
        {
            tecdb.AddItemTec();
            modesdb.AddItemModes();
        }

        /// <summary>
        /// Таймер для update
        /// </summary>
        public void TimerUp()
        {
            UpdateTimer = new System.Timers.Timer();
            UpdateTimer.Enabled = true;
            UpdateTimer.AutoReset = true;
            UpdateTimer.Elapsed += new ElapsedEventHandler(UpdateTimer_Elapsed);
            UpdateTimer.Interval = 67000;
            //GC.KeepAlive(UpdateTimer);
        }

        /// <summary>
        /// обрабтчик события
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void UpdateTimer_Elapsed(object source, ElapsedEventArgs e)
        {
            startUp();
        }

        /// <summary>
        /// класс для обращения к БД 
        /// </summary>
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
                        // m_DataSource_EvtRecievedTable(table);
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

        /// <summary>
        /// Класс ТЭЦ
        /// </summary>
        public partial class Tec : HPanelCommon
        {
            private DataGridView TECDataGridView = new DataGridView();
            private Label LabelTec = new Label();
            //private ContextMenuStrip menuChangeState = new ContextMenuStrip();
            private ContextMenuStrip ContextmenuChangeState;
            private ToolStripMenuItem toolStripMenuItemActivate;
            private ToolStripMenuItem toolStripMenuItemDeactivate;
  
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

                this.ContextmenuChangeState = new System.Windows.Forms.ContextMenuStrip(this.components);
                this.toolStripMenuItemActivate = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripMenuItemDeactivate = new System.Windows.Forms.ToolStripMenuItem();

                this.ContextmenuChangeState.SuspendLayout();

                this.SuspendLayout();

                this.TECDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.TECDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.TECDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
                this.TECDataGridView.AllowUserToAddRows = false;
                this.TECDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
                this.TECDataGridView.RowHeadersVisible = false;
                this.TECDataGridView.Name = "TECDataGridView";
                this.TECDataGridView.TabIndex = 0;
                this.TECDataGridView.ColumnCount = 6;
                this.TECDataGridView.Columns[0].Name = "Источник данных";
                this.TECDataGridView.Columns[1].Name = "Крайнее время";
                this.TECDataGridView.Columns[2].Name = "Крайнее значение";
                this.TECDataGridView.Columns[3].Name = "Время проверки";
                this.TECDataGridView.Columns[4].Name = "Связь";
                this.TECDataGridView.Columns[5].Name = "ТЭЦ";
                this.TECDataGridView.ContextMenuStrip = ContextmenuChangeState;

                this.ContextmenuChangeState.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemActivate,
            this.toolStripMenuItemDeactivate});
                this.toolStripMenuItemActivate.Name = "contextMenuStrip1";
                this.ContextmenuChangeState.Size = new System.Drawing.Size(180, 70);
                // 
                // toolStripMenuItem1
                // 
                this.toolStripMenuItemActivate.Name = "toolStripMenuItem1";
                this.toolStripMenuItemActivate.Size = new System.Drawing.Size(179, 22);
                this.toolStripMenuItemActivate.Text = "Activate";
                // 
                // toolStripMenuItemDeactivate
                // 
                this.toolStripMenuItemDeactivate.Name = "toolStripMenuItem2";
                this.toolStripMenuItemDeactivate.Size = new System.Drawing.Size(179, 22);
                this.toolStripMenuItemDeactivate.Text = "Deactivate";


                this.LabelTec.AutoSize = true;
                this.LabelTec.Name = "LabelTec";
                this.LabelTec.TabIndex = 0;
                this.LabelTec.Text = "Unknow_TEC";

                this.LabelTec.TextAlign = System.Drawing.ContentAlignment.TopCenter;

                this.ResumeLayout(false);
            }

            #endregion
        }

        partial class Tec
        {
            public DataRow[] rows;
            int[,] massTM12;
            enum TM { TM1 = 2, TM2 };

            /// <summary>
            /// Старт TEC
            /// </summary>
            public void start_TEC()
            {
                Create_PanelTEC();
            }

            /// <summary>
            /// Создание панелей ТЭЦ
            /// </summary>
            public void Create_PanelTEC()
            {
                m_arPanelsTEC = new Tec[arraySourceTEC.Rows.Count];

                for (int i = 0; i < arraySourceTEC.Rows.Count; i++)
                {
                    m_arPanelsTEC[i] = new Tec();
                }
                AddItemTec();
                HeaderGridTEC();
            }

            /// <summary>
            /// 
            /// </summary>
            public void MethodTEC()
            {
                TextColumnTec();
                ColumTimeTEC();
                CellsPingTEC();
            }

            /// <summary>
            /// Очистка гирда
            /// </summary>
            /// <param name="i">номер панели</param>
            public void ClearGridsTEC(int i)
            {
                if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                {
                    m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Rows.Clear()));
                }
            }

            /// <summary>
            /// Добавление строк
            /// </summary>
            /// <param name="x"></param>
            public void AddRowsTEC(int x, int countrow)
            {
                for (int i = 0; i < countrow; i++)
                {
                    if (m_arPanelsTEC[x].TECDataGridView.InvokeRequired)
                    {
                        m_arPanelsTEC[x].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[x].TECDataGridView.Rows.Add()));
                    }
                    else
                    {
                        m_arPanelsTEC[x].TECDataGridView.Rows.Add();
                    }
                }
            }

            /// <summary>
            /// Заполнение данными грида
            /// </summary>
            public void InsertDataTEC(string filter12, int i)
            {
                DataRow[] DR;

                DR = m_tableSourceData.Select(filter12);

                for (int r = 0; r < m_tableSourceData.Select(filter12).Length; r++)
                {
                    for (int c = 0; c < m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells.Count; c++)
                    {
                        if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                        {
                            m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells[0].Value = DR[r][1]));
                            m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells[2].Value = DR[r][2]));
                        }
                        else
                        {
                            m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells[0].Value = DR[r][1];
                            m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells[2].Value = DR[r][2];
                            m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells[5].Value = DR[r][7];
                            m_arPanelsTEC[i].TECDataGridView.Columns[5].Visible = false;
                        }
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="filter13"></param>
            public void InsetExtremeTime(string filter13, int i)
            {
                DataRow[] DR;

                DR = m_tableSourceData.Select(filter13);

                for (int r = 0; r < m_tableSourceData.Select(filter13).Length; r++)
                {
                    for (int c = 0; c < m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells.Count; c++)
                    {
                        if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                        {
                            m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells[1].Value = DR[r][2]));
                        }
                        else
                        {
                            m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells[1].Value = DR[r][2];
                        }
                    }
                }
            }

            /// <summary>
            /// Функция заполнения гридов
            /// </summary>
            public void AddItemTec()
            {
                for (int i = 0; i < m_arPanelsTEC.Length; i++)
                {
                    string filter12 = "ID_Units = " + (int)Units.Value + " and ID_EXT = " + Convert.ToInt32(arraySourceTEC.Rows[i][0]);
                    string filter13 = "ID_Units = " + (int)Units.Date + " and ID_EXT = " + Convert.ToInt32(arraySourceTEC.Rows[i][0]);

                    //проверка на пустоту(ибо рано срабатывает)
                    while (m_tableSourceData == null)
                    {

                    }

                    ClearGridsTEC(i);

                    AddRowsTEC(i, m_tableSourceData.Select(filter12).Length);

                    InsertDataTEC(filter12, i);
                    InsetExtremeTime(filter13, i);
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
                    string str = arraySourceTEC.Rows[i][@"NAME_SHR"].ToString();

                    if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                    {
                        m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].LabelTec.Text = str));
                        m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Update()));
                    }
                    else
                    {
                        m_arPanelsTEC[i].LabelTec.Text = str;
                        m_arPanelsTEC[i].TECDataGridView.Update();
                    }
                }

            }

            /// <summary>
            /// Функция перемеинования ячейки датагрид TEC
            /// </summary>
            public void TextColumnTec()
            {
                for (int k = 0; k < arraySourceTEC.Rows.Count; k++)
                {
                    for (int j = 0; j < m_arPanelsTEC[k].TECDataGridView.Rows.Count; j++)
                    {
                        string text1 = null;
                        string text2 = null;
                        text2 = m_arPanelsTEC[k].TECDataGridView.Rows[j].Cells[0].Value.ToString();

                        int s = -1;
                        int l = 0;

                        do
                        {
                            s++;
                            text1 = (string)massivetext[s, l].ToString();

                            if (s == arrayKeys_DataSheet.Rows.Count - 1)
                            {
                                l++;
                                s = -1;
                            }
                        }

                        while (text1 != text2);

                        string text = arrayKeys_DataSheet.Rows[s][@"NAME_SHR"].ToString();

                        if (m_arPanelsTEC[k].TECDataGridView.InvokeRequired)
                        {
                            m_arPanelsTEC[k].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[k].TECDataGridView.Rows[j].Cells[0].Value = text.ToString()));
                            //PaintigCells(k, j);
                        }
                        else
                        {
                            m_arPanelsTEC[k].TECDataGridView.Rows[j].Cells[0].Value = text.ToString();
                            PaintigCells(k, j);
                        }
                    }
                }
            }

            /// <summary>
            /// Функция заполнения ячеек грида ТЭЦ верменем
            /// </summary>
            /// <param name="table"></param>
            public void ColumTimeTEC()
            {
                string text = DateTime.Now.ToString();

                for (int i = 0; i < m_arPanelsTEC.Length; i++)
                {
                    for (int j = 0; j < m_arPanelsTEC[i].TECDataGridView.Rows.Count; j++)
                    {
                        if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                            m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Rows[j].Cells[3].Value = text.ToString()));
                        else
                        {
                            m_arPanelsTEC[i].TECDataGridView.Rows[j].Cells[3].Value = text.ToString();
                        }
                    }
                }

            }

            /// <summary>
            /// Добавление результата пропинговки в грид
            /// </summary>
            /// <param name="x"></param>
            public void CellsPingTEC()
            {
                for (int k = 0; k < m_arPanelsTEC.Length; k++)
                {
                    for (int i = 0; i < m_arPanelsTEC[k].TECDataGridView.Rows.Count; i++)
                    {
                        int s = -1;
                        string text1;
                        string text2 = massiveServ[i, 1];

                        do
                        {
                            s++;
                            text1 = arraySource.Rows[s][@"NAME_SHR"].ToString();
                            if (s == 39)
                            {
                                s = 0;
                            }
                        }

                        while (text1 != text2);

                        if (m_arPanelsTEC[k].TECDataGridView.InvokeRequired)
                        {
                            m_arPanelsTEC[k].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[k].TECDataGridView.Rows[i].Cells[4].Value = massiveServ[i, 0]));
                            m_arPanelsTEC[k].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[k].TECDataGridView.Refresh()));
                            m_arPanelsTEC[k].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[k].TECDataGridView.ClearSelection()));
                        }
                        else
                        {
                            m_arPanelsTEC[k].TECDataGridView.Rows[i].Cells[4].Value = massiveServ[i, 0];
                            m_arPanelsTEC[k].TECDataGridView.Refresh();
                            m_arPanelsTEC[k].TECDataGridView.ClearSelection();
                        }
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            public void PaintCell(int x, int y)
            {
                if (m_arPanelsTEC[x].TECDataGridView.InvokeRequired)
                {
                    m_arPanelsTEC[x].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[x].TECDataGridView.Rows[y].Cells[0].Style.BackColor = System.Drawing.Color.LightSalmon));
                }
                else
                {
                    m_arPanelsTEC[x].TECDataGridView.Rows[y].Cells[0].Style.BackColor = System.Drawing.Color.LightSalmon;
                }
            }

            /// <summary>
            /// Закраска ячейки ТМ
            /// </summary>
            public void PaintigCells(int x, int y)
            {
                string a = m_arPanelsTEC[x].TECDataGridView.Rows[y].Cells[5].Value.ToString();
                string b;

                for (int i = 0; i < massTM.Length/2; i++)
                {
                    b = massTM[i, 1].ToString();
                    if (a == b)
                    {
                        PaintCell(x, y);
                        break;
                    }
                    
                } 
            }

            /// <summary>
            /// сборка тм строки
            /// </summary>
            public void AssemblyList(DataTable table)
            {
                massTM12 = new int[table.Rows.Count, 2];

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    string filter1 = String.Concat(table.Rows[i][0].ToString(), (int)TM.TM1);
                    massTM12.SetValue(Convert.ToInt32(filter1), i, 0);
                }

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    string filter1 = String.Concat(table.Rows[i][0].ToString(), (int)TM.TM2);
                    massTM12.SetValue(Convert.ToInt32(filter1), i, 1);
                }
            }
        }

        /// <summary>
        /// Класс МОДЕС
        /// </summary>
        public partial class Modes : HPanelCommon
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
                this.Controls.Add(LabelModes, 0, 0);
                this.Controls.Add(ModesDataGridView, 0, 1);
                this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
                this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.74641F));

                this.SuspendLayout();

                this.Dock = DockStyle.Fill;
                this.ModesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.ModesDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
                this.ModesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.ModesDataGridView.Dock = DockStyle.Fill;
                this.ModesDataGridView.AllowUserToAddRows = false;
                this.ModesDataGridView.RowHeadersVisible = false;
                this.ModesDataGridView.Name = "ModesDataGridView";
                this.ModesDataGridView.TabIndex = 0;
                this.ModesDataGridView.ColumnCount = 5;
                this.ModesDataGridView.Columns[0].Name = "Источник данных";
                this.ModesDataGridView.Columns[1].Name = "Крайнее время";
                this.ModesDataGridView.Columns[3].Name = "Время проверки";
                this.ModesDataGridView.Columns[2].Name = "Крайнее значение";
                this.ModesDataGridView.Columns[4].Name = "Связь";

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
        }

        partial class Modes
        {
            DataRow[] rows_modes;

            /// <summary>
            /// Старт MODES
            /// </summary>
            public void start_MODES()
            {
                Create_Modes();
            }

            /// <summary>
            /// Создание панелей Модес
            /// </summary>
            /// <param name="table"></param>
            public void Create_Modes()
            {
                rows_modes = tbModes.Select();
                m_arPanelsMODES = new Modes[rows_modes.Length];

                for (int i = 0; i < rows_modes.Length; i++)
                {
                    m_arPanelsMODES[i] = new Modes();
                }

                AddItemModes();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="i"></param>
            public void ClearGridsModes(int i)
            {
                if (m_arPanelsMODES[i].ModesDataGridView.InvokeRequired)
                {
                    m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows.Clear()));
                }
                else
                {
                    m_arPanelsMODES[i].ModesDataGridView.Rows.Clear();
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void AddRowsModes(int i, int counter)
            {
                for (int x = 0; x < counter; x++)
                {
                    if (m_arPanelsMODES[i].ModesDataGridView.InvokeRequired)
                    {
                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows.Add()));
                    }
                    else
                    {
                        m_arPanelsMODES[i].ModesDataGridView.Rows.Add();
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="filter12"></param>
            /// <param name="i"></param>
            public void InsertDataModes(string filter12, int i)
            {
                DataRow[] DR;

                DR = m_tableSourceData.Select(filter12);

                for (int r = 0; r < m_tableSourceData.Select(filter12).Length; r++)
                {
                    for (int c = 0; c < m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells.Count; c++)
                    {
                        if (m_arPanelsMODES[i].ModesDataGridView.InvokeRequired)
                        {
                            m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[0].Value = DR[r][1]));
                            m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[2].Value = DR[r][2]));
                        }
                        else
                        {
                            m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[0].Value = DR[r][1];
                            m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[2].Value = DR[r][2];
                        }
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="filter12"></param>
            /// <param name="i"></param>
            public void InsertExtremeDateModes(string filter13, int i)
            {
                DataRow[] DR;

                DR = m_tableSourceData.Select(filter13);

                for (int r = 0; r < m_tableSourceData.Select(filter13).Length; r++)
                {
                    for (int c = 0; c < m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells.Count; c++)
                    {
                        if (m_arPanelsMODES[i].ModesDataGridView.InvokeRequired)
                        {
                            m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[1].Value = DR[r][2]));
                        }
                        else
                        {
                            m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[1].Value = DR[r][2];
                        }
                    }
                }
            }

            /// <summary>
            /// Функция Заполнения панелей Модес
            /// </summary>
            public void AddItemModes()
            {
                for (int i = 0; i < m_arPanelsMODES.Length; i++)
                {
                    string filter12 = "ID_Units = " + (int)Units.Value + " and ID_EXT = " + Convert.ToInt32(tbModes.Rows[i][0]);
                    string filter13 = "ID_Units = " + (int)Units.Date + " and ID_EXT = " + Convert.ToInt32(tbModes.Rows[i][0]);

                    //проверка на пустоту
                    while (m_tableSourceData == null)
                    {

                    }
                    ClearGridsModes(i);
                    AddRowsModes(i, m_tableSourceData.Select(filter12).Length);
                    InsertDataModes(filter12, i);
                    InsertExtremeDateModes(filter13, i);
                }

                HeaderTextModes();
                MethodModes();
            }

            /// <summary>
            /// 
            /// </summary>
            public void MethodModes()
            {
                TextColumnModes();
                TimeModes();
                CellsPingMODES();
            }

            /// <summary>
            /// Функция изменения заголовков грида Modes
            /// </summary>
            public void HeaderTextModes()
            {
                for (int i = 0; i < m_arPanelsMODES.Length; i++)
                {
                    string str = tbModes.Rows[i][@"NAME_SHR"].ToString();

                    if (m_arPanelsMODES[i].LabelModes.InvokeRequired)
                        m_arPanelsMODES[i].LabelModes.Invoke(new Action(() => m_arPanelsMODES[i].LabelModes.Text = str));
                    else
                    {
                        m_arPanelsMODES[i].LabelModes.Text = str;
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
                        string text2 = m_arPanelsMODES[k].ModesDataGridView.Rows[j].Cells[0].Value.ToString();

                        int s = -1;

                        do
                        {
                            s++;
                            text1 = arrayKeys_DataSheet.Rows[s][@"ID"].ToString();

                            if (s == 10)
                            { s = 0; }
                        }

                        while (text1 != text2);

                        string text = arrayKeys_DataSheet.Rows[s][@"NAME_SHR"].ToString();

                        if (m_arPanelsMODES[k].ModesDataGridView.InvokeRequired)
                            m_arPanelsMODES[k].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[k].ModesDataGridView.Rows[j].Cells[0].Value = text.ToString()));
                        else
                        {
                            m_arPanelsMODES[k].ModesDataGridView.Rows[j].Cells[0].Value = text.ToString();
                        }
                    }
                }
            }

            /// <summary>
            /// Добавление результата пропинговки в грид MODES
            /// </summary>
            /// <param name="x"></param>
            public void CellsPingMODES()
            {
                for (int k = 0; k < m_arPanelsMODES.Length; k++)
                {
                    for (int i = 0; i < m_arPanelsMODES[k].ModesDataGridView.Rows.Count; i++)
                    {
                        int s = -1;
                        string text1;

                        string text2 = tbModes.Rows[k][@"NAME_SHR"].ToString();

                        do
                        {
                            s++;
                            text1 = massiveServ[s, 1];
                        }

                        while (text1 != text2);

                        if (m_arPanelsMODES[k].ModesDataGridView.InvokeRequired)
                            m_arPanelsMODES[k].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[k].ModesDataGridView.Rows[i].Cells[4].Value = massiveServ[s, 0]));
                        else
                        {
                            m_arPanelsMODES[k].ModesDataGridView.Rows[i].Cells[4].Value = massiveServ[s, 0];
                        }
                    }
                }
            }


            /// <summary>
            /// Функция заполнения ячеек грида MODES верменем
            /// </summary>
            public void TimeModes()
            {
                string text = DateTime.Now.ToString();

                for (int i = 0; i < m_arPanelsMODES.Length; i++)
                {
                    for (int j = 0; j < m_arPanelsMODES[i].ModesDataGridView.Rows.Count; j++)
                    {
                        m_arPanelsMODES[i].ModesDataGridView.Rows[j].Cells[3].Value = text.ToString();

                        if (m_arPanelsMODES[i].ModesDataGridView.InvokeRequired)
                            m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[j].Cells[3].Value = text.ToString()));
                    }
                }
            }
        }

        /// <summary>
        /// Класс Задачи 
        /// </summary>
        public partial class Task : HPanelCommon
        {
            public DataGridView TaskDataGridView = new DataGridView();
            public CheckedListBox TaskCheckedListBox = new CheckedListBox();
            DataTable table = new DataTable();

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

        partial class Task
        {
            /// <summary>
            /// Выборка из таблицы задачи
            /// </summary>
            public void GetDataTask(DataTable table)
            {
                for (int j = 0; j < table.Rows.Count; j++)
                {
                    AddSourceDataTask(table.Rows[j][@"NAME_SHR"].ToString());
                }
            }

            /// <summary>
            /// Фукнция заполнения чекбокса задачи
            /// </summary>
            public void AddSourceDataTask(string desc)
            {
                TaskCheckedListBox.Items.Add(desc);
            }

            /// <summary>
            /// Функция заполнения ячеек грида Task верменем
            /// </summary>
            public void ColumTimeTask()
            {
                string text = DateTime.Now.ToString();

                for (int j = 0; j < TaskDataGridView.Rows.Count; j++)
                {
                    TaskDataGridView.Rows[j].Cells[@"UPDATEDATETIME"].Value = text.ToString();
                }
            }

            /// <summary>
            ///  Функция перемеинования ячейки датагрид Task
            /// </summary>
            public void TextColumnTask(int indx, DataTable table)
            {
                string text1;
                string text2 = TaskDataGridView.Rows[indx].Cells[@"ID_Value"].Value.ToString();

                int s = 0;

                do
                {
                    text1 = table.Rows[s][@"ID"].ToString();
                    s++;
                }

                while (text1 != text2);

                string text = table.Rows[s - 1][@"DESCRIPTION"].ToString();

                /*if (taskdb.TaskDataGridView.InvokeRequired)
                    taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.Rows[j].Cells[@"ID_Value"].Value = text.ToString()));*/

                TaskDataGridView.Rows[indx].Cells[@"ID_Value"].Value = text.ToString();
            }

            /// <summary>
            /// Функция изменения заголовков грида Task
            /// </summary>
            public void HeaderTextTask()
            {
                TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[7].HeaderText = "Имя задачи"));
                TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[0].Visible = false));
                TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[1].HeaderText = "Тип"));
                TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[2].HeaderText = "Крайнее значение(сек)"));
                TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[3].Visible = false));
                TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[6].Visible = false));
                TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[5].Visible = false));
                TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[4].HeaderText = "Время проверки"));
                TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[7].DisplayIndex = 0));
            }

            /// <summary>
            /// 
            /// </summary>
            public void MethodTask(int x, DataTable table)
            {
                TextColumnTask(x, table);
                ColumTimeTask();
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        public PanelStatisticDiagnostic1()
        {
            initialize();
        }

        public PanelStatisticDiagnostic1(IContainer container)
        {
            initialize();
            container.Add(this);
            m_sFileINI = new FileINI(@"setup.ini", false);
            TimerUp();
        }

        /// <summary>
        /// Инициализация панели
        /// </summary>
        private void initialize()
        {
            InitializeComponent();
            collection_data();
        }

        /// <summary>
        /// функция ожидания загрузки всех данных на форму
        /// </summary>
        public void WaitFunc(int i)
        {
            Thread th = new Thread(fw.StartWaitForm);

            fw.StartPosition = FormStartPosition.CenterScreen;
            switch (i)
            {
                case 1:
                    th.Start();
                    break;
                case 2:
                    fw.StopWaitForm();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddPanel()
        {
            AddPanelModes();
            AddPanelTEC();
            WaitFunc(2);
        }

        /*/// <summary>
        /// инициализация Бэкграунда
        /// </summary>
        private void InitializeBackgroundWorker()
        {
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start_BGW()
        {
            if (backgroundWorker1.IsBusy != true)
            {
                // Start the asynchronous operation.
                backgroundWorker1.RunWorkerAsync();
            }
        }

        /// <summary>
        /// This event handler updates the progress.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            //result.Text = (e.ProgressPercentage.ToString() + "%");
        }

        /// <summary>
        /// This event handler deals with the results of the background operation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                //result.Text = "Canceled!";
            }
            else if (e.Error != null)
            {
                //result.Text = "Error: " + e.Error.Message;
            }
            else
            {
                //result.Text = "Done!";
            }
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            System.Threading.Thread.Sleep(500);
        }*/

        /// <summary>
        /// Формирование сиска источников
        /// </summary>
        public void MassiveSourceName()
        {
            massivetext = new object[arrayKeys_DataSheet.Rows.Count, 2];
            int r = 0, c = 0;
            for (c = 0; c < 2; c++)
            {
                if (c == 0)
                {
                    for (r = 0; r < arrayKeys_DataSheet.Rows.Count; r++)
                    {
                        massivetext.SetValue(arrayKeys_DataSheet.Rows[r][c], r, c);
                    }
                }

                else
                {
                    for (r = 0; r < arrayKeys_DataSheet.Rows.Count; r++)
                    {
                        massivetext.SetValue(arrayKeys_DataSheet.Rows[r][c], r, c);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void start()
        {
            StartModes();
            StartPanelTec();
            AddPanel();
        }

        /// <summary>
        /// Сбор информации с источников данных
        /// </summary>
        public void collection_data()
        {
            WaitFunc(1);
            GetCurrentData();
            PingSourceData();
            Start();
        }

        /// <summary>
        /// Коннект к БД. создание экземпляра ДБинтерфейса
        /// </summary>
        /// <param name="connSett"></param>
        public void Start(ConnectionSettings connSett)
        {
            m_connSett = connSett;
            m_DataSource = new HDataSource((ConnectionSettings)m_connSett);
            m_DataSource.StartDbInterfaces();
            m_DataSource.Start();
            m_DataSource.Command();
            m_DataSource.EvtRecievedTable += new DelegateObjectFunc(m_DataSource_EvtRecievedTable);
        }

        /// <summary>
        /// Запуск коннекта к БД
        /// </summary>
        public override void Start()
        {
            base.Start();

            Start(new ConnectionSettings()
            {
                id = 665
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
        /// Функция взятия информации из конф.БД
        /// </summary>
        public void GetCurrentData()
        {
            ConnectionSettings CS = new ConnectionSettings
            {
                id = 664
                ,
                name = @"DB_CONFIG"
                ,
                server = @"10.105.1.107"
                ,
                port = 1433
                ,
                dbName = @"techsite_cfg-2.X.X"
                ,
                userName = @"client1"
                ,
                password = @"client"
                ,
                ignore = false
            };

            int ListenerId = 0;
            ListenerId = DbSources.Sources().Register(CS, false, CS.name);
            int err = -1;

            DbConnection dbconn = null;
            dbconn = DbSources.Sources().GetConnection(ListenerId, out err);

            if ((err == 0) && (!(dbconn == null)))
            {
                arraySourceDataTask = DbTSQLInterface.Select(ref dbconn, "SELECT * FROM TASK_LIST", null, null, out err);
                arrayKeys_DataSheet = DbTSQLInterface.Select(ref dbconn, "SELECT * FROM Keys_Datasheet", null, null, out err);
                tbModes = DbTSQLInterface.Select(ref dbconn, "SELECT * FROM Modes_List", null, null, out err);
                arraySource = DbTSQLInterface.Select(ref dbconn, "SELECT * FROM SOURCE", null, null, out err);
                arraySourceTEC = InitTEC_200.getListTEC(ref dbconn, false, out err);
            }
            else
                throw new Exception(@"Нет соединения с БД");

            DbSources.Sources().UnRegister(ListenerId);
            taskdb.GetDataTask(arraySourceDataTask);
            MassiveSourceName();
            CreateListTM(arraySourceTEC);
            taskdb.TaskCheckedListBox.SelectedIndexChanged += new EventHandler(TaskCheckedListBox_SelectedIndexChanged);
        }

        /// <summary>
        /// обработка события
        /// </summary>
        /// <param name="table"></param>
        static void m_DataSource_EvtRecievedTable(object table)
        {
            m_tableSourceData = (DataTable)table;
        }

        /// <summary>
        /// Запуск потока для панели TEC
        /// </summary>
        public void StartPanelTec()
        {
            tecdb.start_TEC();
        }

        /// <summary>
        /// Запуск потока для панели MODES
        /// </summary>
        public void StartModes()
        {
            modesdb.start_MODES();
        }

        /// <summary>
        /// Прикрепляет гриды к панеле МОДЕС
        /// </summary>
        public void AddPanelModes()
        {
            // int i = -1;
            int indx = -1
               , col = -1
               , row = -1;

            for (int i = 0; i < m_arPanelsMODES.Length; i++)
            {
                if (ModesTableLayoutPanel.RowCount * ModesTableLayoutPanel.ColumnCount < m_arPanelsMODES.Length)
                {
                    if (ModesTableLayoutPanel.InvokeRequired)
                    {
                        ModesTableLayoutPanel.Invoke(new Action(() => ModesTableLayoutPanel.RowCount++));
                        ModesTableLayoutPanel.Invoke(new Action(() => ModesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F))));
                    }
                    else
                    {
                        ModesTableLayoutPanel.RowCount++;
                        ModesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
                    }
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
                {
                    ModesTableLayoutPanel.Invoke(new Action(() => ModesTableLayoutPanel.Controls.Add(m_arPanelsMODES[i], col, row)));
                    ModesTableLayoutPanel.Invoke(new Action(() => ModesTableLayoutPanel.AutoScroll = true));
                }
                else
                {
                    ModesTableLayoutPanel.Controls.Add(m_arPanelsMODES[i], col, row);
                    ModesTableLayoutPanel.AutoScroll = true;
                }
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
                else
                {
                    TecTableLayoutPanel.Controls.Add(m_arPanelsTEC[i], col, row);
                }
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
                    foreach (DataRow rowTask in m_tableSourceData.Select(filter_task))
                    {
                        table_task.ImportRow(rowTask);
                    }

                    taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.DataSource = table_task));
                    counter++;
                }

                else
                {
                    foreach (DataRow rowsTec in m_tableSourceData.Select(filter_task))
                    {
                        counter++;
                        table_task = m_tableSourceData.Clone();
                        table_task.ImportRow(rowsTec);

                        taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.DataSource = table_task));

                        taskdb.TaskDataGridView.Update();
                    }
                }

                taskdb.MethodTask(counter, arrayKeys_DataSheet);
                taskdb.HeaderTextTask();
            }

            else
            {
                string curItemDel = taskdb.TaskCheckedListBox.SelectedItem.ToString();
                string filterDel = "NAME_SHR = '" + curItemDel + "'";
                counter--;

                foreach (DataRow rowDel in table_task.Select(filterDel))
                {
                    table_task.Rows.Remove(rowDel);
                }

                taskdb.TaskDataGridView.Invoke(new Action(() => taskdb.TaskDataGridView.DataSource = table_task));
            }
        }

        /// <summary>
        /// Пропинговка ИстД
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public string Ping(string server)
        {
            string str;
            Ping pingsender = new Ping();

            IPStatus status = IPStatus.Unknown;

            try
            {
                status = pingsender.Send(server, 100).Status;

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
            string strokavst;
            massiveServ = new string[arraySource.Rows.Count, 2];
            string[] massive = new string[arraySource.Rows.Count];

            for (int s = 0; s < arraySource.Rows.Count; s++)
            {
                server = (string)arraySource.Rows[s][@"IP"];
                strokavst = Ping(server);
                massive.SetValue(strokavst, s);
            }

            for (int c = 0; c < 2; c++)
            {
                if (c == 0)
                {
                    for (int r = 0; r < arraySource.Rows.Count; r++)
                    {
                        massiveServ[r, c] = massive[r];
                    }
                }

                else
                {
                    for (int r = 0; r < arraySource.Rows.Count; r++)
                    {
                        massiveServ[r, c] = (string)arraySource.Rows[r][@"NAME_SHR"];
                    }
                }
            }
        }

        /// <summary>
        /// Создание списка ТМ
        /// </summary>
        public void CreateListTM(DataTable table)
        {
            massTM = new object[table.Rows.Count, 2];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                massTM.SetValue(table.Rows[i][23], i, 0);
            }

            int t = -1;
            int id;

            for (int j = 0; j < table.Rows.Count; j++)
            {
                do
                {
                    t++;
                    id = (int)arraySource.Rows[t][@"ID"];
                }

                while ((int)massTM[j, 0] != id);

                massTM.SetValue(arraySource.Rows[t][@"NAME_SHR"].ToString(), j, 1);
            }

            tecdb.AssemblyList(table);
        }
    }
}



