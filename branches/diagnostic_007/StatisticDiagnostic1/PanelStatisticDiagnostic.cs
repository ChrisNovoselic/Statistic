﻿using System.Collections;
using System;
using System.Linq;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Timers;
using System.Threading;
using System.Drawing;
using System.Windows.Forms; //TableLayoutPanel
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using HClassLibrary;
using StatisticCommon;

namespace StatisticDiagnostic
{
    public partial class PanelStatisticDiagnostic
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
            //
            //TecTableLayoutPanel
            //
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
            //
            //ModesTableLayoutPanel
            //
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
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));

            this.ResumeLayout();
        }

        #endregion

        private System.Windows.Forms.Label Tasklabel;
        private System.Windows.Forms.Label TEClabel;
        private System.Windows.Forms.Label Modeslabel;
        private System.Windows.Forms.TableLayoutPanel TecTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel ModesTableLayoutPanel;
    }

    public partial class PanelStatisticDiagnostic : PanelStatistic
    {
        static object[,] massTM;
        static Modes[] m_arPanelsMODES;
        static Tec[] m_arPanelsTEC;
        static DataTable tbModes = new DataTable();
        static DataTable m_tableSourceData;
        public DataTable arraySourceDataTask = new DataTable();
        static DataTable arraySource = new DataTable();
        static DataTable array_GTP = new DataTable();
        static DataTable arraySourceTEC = new DataTable();
        static DataTable arrayParam = new DataTable();
        static HDataSource m_DataSource;
        static System.Timers.Timer UpdateTimer;
        Task taskdb = new Task();
        Modes modesdb = new Modes();
        Tec tecdb = new Tec();
        int iListernID;

        /// <summary>
        /// Класс для передачи данных в классы
        /// </summary>
        public static class TransferData
        {
            public delegate void MyEvent(object data);
            public static MyEvent EventHandler;
        }

        /// <summary>
        /// запуск обновления
        /// </summary>
        public void startUp()
        {
            RefreshDataSource();
        }

        /// <summary>
        /// Перегрузка информации из бд
        /// </summary>
        public void RefreshDataSource()
        {
            LoadData();
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
            ConnectionSettings m_connSett;

            protected enum State
            {
                Command
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
                ClearStates();
                AddState((int)State.Command);
                Run(@"Command");
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
                        Request(m_dictIdListeners[0][0], @"Select * from Diagnostic");
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

                this.ContextmenuChangeState = new System.Windows.Forms.ContextMenuStrip();
                this.toolStripMenuItemActivate = new System.Windows.Forms.ToolStripMenuItem();
                this.toolStripMenuItemDeactivate = new System.Windows.Forms.ToolStripMenuItem();

                this.ContextmenuChangeState.SuspendLayout();

                this.SuspendLayout();

                this.TECDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.TECDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.TECDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
                this.TECDataGridView.AllowUserToAddRows = false;
                this.TECDataGridView.ClearSelection();
                this.TECDataGridView.AllowUserToDeleteRows = false;
                this.TECDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
                this.TECDataGridView.RowHeadersVisible = false;
                this.TECDataGridView.ReadOnly = true;
                this.TECDataGridView.Name = "TECDataGridView";
                this.TECDataGridView.TabIndex = 0;
                this.TECDataGridView.ColumnCount = 6;
                this.TECDataGridView.Columns[0].Name = "Источник данных";
                this.TECDataGridView.Columns[1].Name = "Крайнее время";
                this.TECDataGridView.Columns[2].Name = "Крайнее значение";
                this.TECDataGridView.Columns[3].Name = "Время проверки";
                this.TECDataGridView.Columns[4].Name = "Связь";
                this.TECDataGridView.Columns[5].Name = "Связь";
                this.TECDataGridView.Columns[0].Width = 43;
                this.TECDataGridView.Columns[1].Width = 57;
                this.TECDataGridView.Columns[2].Width = 35;
                this.TECDataGridView.Columns[3].Width = 57;
                this.TECDataGridView.Columns[4].Width = 35;
                this.TECDataGridView.Resize += new EventHandler(TECDataGridView_Cell);
                this.TECDataGridView.CellClick += new DataGridViewCellEventHandler(TECDataGridView_Cell);
                this.TECDataGridView.CellMouseClick += new DataGridViewCellMouseEventHandler(TECDataGridView_CellMouseClick);
                this.TECDataGridView.CellMouseEnter += new DataGridViewCellEventHandler(TECDataGridView_CellMouseEnter);
                //
                //
                //
                this.ContextmenuChangeState.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.toolStripMenuItemActivate,
                this.toolStripMenuItemDeactivate});
                this.toolStripMenuItemActivate.Name = "contextMenuStrip1";
                this.ContextmenuChangeState.Size = new System.Drawing.Size(180, 70);
                // 
                // toolStripMenuItemActivate
                // 
                this.toolStripMenuItemActivate.Name = "toolStripMenuItem1";
                this.toolStripMenuItemActivate.Size = new System.Drawing.Size(179, 22);
                this.toolStripMenuItemActivate.Text = "Activate";
                this.toolStripMenuItemActivate.Click += new EventHandler(toolStripMenuItemActivate_Click);
                // 
                // toolStripMenuItemDeactivate
                // 
                this.toolStripMenuItemDeactivate.Name = "toolStripMenuItem2";
                this.toolStripMenuItemDeactivate.Size = new System.Drawing.Size(179, 22);
                this.toolStripMenuItemDeactivate.Text = "Deactivate";
                this.toolStripMenuItemDeactivate.Click += new EventHandler(toolStripMenuItemDeactivate_Click);
                //
                // LabelTec
                //
                this.LabelTec.AutoSize = true;
                this.LabelTec.Name = "LabelTec";
                this.LabelTec.TabIndex = 0;
                this.LabelTec.Text = "Unknow_TEC";

                this.LabelTec.TextAlign = System.Drawing.ContentAlignment.TopCenter;
                this.ContextmenuChangeState.ResumeLayout(false);
                this.ResumeLayout(false);
            }

            #endregion
        }

        partial class Tec
        {
            public DataRow[] rows;
            public int[,] massTM12;
            enum TM { TM1 = 2, TM2 };
            int ColumnIndex;
            public Point point = new Point();

            /// <summary>
            /// 
            /// </summary>
            /// <param name="activated"></param>
            public void ActivateTEC(bool activated)
            {
                if (activated == true)
                {
                    //AddItemTec();

                    for (int i = 0; i < m_arPanelsTEC.Length; i++)
                    {
                        m_arPanelsTEC[i].Focus();
                    }
                }
                else ;
            }

            /// <summary>
            /// уничтожение панелей
            /// </summary>
            public void stopTEC()
            {
                if (!(m_arPanelsTEC == null))
                {
                    for (int i = 0; i < m_arPanelsTEC.Length; i++)
                    {
                        m_arPanelsTEC[i].Dispose();
                    }
                }
                else ;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="nameTec"></param>
            /// <returns></returns>
            public object SelectionTM(string nameTec)
            {
                DataRow[] foundrow;

                string filter = "NAME_SHR = '" + nameTec + "'";
                foundrow = arraySource.Select(filter);
                object a = foundrow[0]["ID"].ToString();

                return a;
            }

            /// <summary>
            /// активация источника СОТИАССО
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void toolStripMenuItemActivate_Click(object sender, EventArgs e)
            {
                string a = TECDataGridView.Rows[ColumnIndex].Cells[5].Value.ToString();

                int t = Convert.ToInt32(SelectionTM(a));

                int numberPanel = (t / 10) - 1;

                UpdateTecTM(StringQuery(t, numberPanel + 1));

                for (int i = 0; i < m_arPanelsTEC[numberPanel].TECDataGridView.Rows.Count; i++)
                {
                    if (m_arPanelsTEC[numberPanel].TECDataGridView.Rows[i].Cells[0].Style.BackColor == System.Drawing.Color.SeaGreen)
                    {
                        PaintCellDeactive(numberPanel, i);
                    }
                }

                PaintCellActive(numberPanel, point.Y);
            }

            /// <summary>
            /// деактивация источника СОТИАССО
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void toolStripMenuItemDeactivate_Click(object sender, EventArgs e)
            {
                int c = 0;
                int t = point.Y;
                string a = TECDataGridView.Rows[ColumnIndex].Cells[5].Value.ToString();
                int numberPanel = ((Convert.ToInt32(SelectionTM(a))) / 10) - 1;

                PaintCellDeactive(numberPanel, point.Y);
                UpdateTecTM(StringQuery(c, numberPanel + 1));
            }

            /// <summary>
            /// Считывание координатов ячейки грида
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="ee"></param>
            public void TECDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs ee)
            {
                point.X = ee.ColumnIndex;
                point.Y = ee.RowIndex;
            }

            /// <summary>
            /// Вызов контекстного меню
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void TECDataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
            {
                if ((e.Button == MouseButtons.Left) && (point.Y > -1))
                {
                    if (TECDataGridView.Rows[point.Y].Cells[5] != TECDataGridView.Rows[0].Cells[5])
                    {
                        ColumnIndex = e.RowIndex;

                        if
                        (TECDataGridView.Rows[point.Y].Cells[0].Style.BackColor == System.Drawing.Color.SeaGreen)
                        {
                            this.TECDataGridView.Rows[point.Y].Cells[0].ContextMenuStrip = ContextmenuChangeState;
                            ContextmenuChangeState.Items[0].Enabled = false;
                            ContextmenuChangeState.Items[1].Enabled = true;
                            ContextmenuChangeState.Show(Cursor.Position.X, Cursor.Position.Y);
                            this.TECDataGridView.Rows[point.Y].Cells[0].ContextMenuStrip = null;
                        }

                        else
                        {
                            this.TECDataGridView.Rows[point.Y].Cells[0].ContextMenuStrip = ContextmenuChangeState;
                            this.ContextmenuChangeState.Items[1].Enabled = false;
                            ContextmenuChangeState.Items[0].Enabled = true;
                            ContextmenuChangeState.Show(Cursor.Position.X, Cursor.Position.Y);
                            this.TECDataGridView.Rows[point.Y].Cells[0].ContextMenuStrip = null;
                        }
                    }
                }
            }

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
                    if (m_arPanelsTEC[i] == null)
                    {
                        m_arPanelsTEC[i] = new Tec();
                    }
                }

                HeaderGridTEC();
            }

            /// <summary>
            /// Добавление строк
            /// </summary>
            /// <param name="x"></param>
            public void AddRowsTEC(int x, int countrow)
            {
                for (int i = 0; i < countrow / 2; i++)
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
            public void InsertDataTEC(string filter, int i)
            {
                DataRow[] DR;
                string text = DateTime.Now.ToString();
                int t = 0;
                DR = m_tableSourceData.Select(filter);

                for (int r = 0; r < m_tableSourceData.Select(filter).Length / 2; r++)
                {
                    if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                    {
                        m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells[2].Value = DR[t]["Value"]));
                        m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells[1].Value = DR[t + 1]["Value"]));
                        m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells[5].Value = DR[t]["NAME_SHR"]));
                        m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Columns[5].Visible = false));
                        CellsPingTEC(filter, i);
                        m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells[3].Value = text.ToString()));
                        PaintigCells(i, r);
                    }

                    else
                    {
                        m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells[2].Value = DR[t]["Value"];
                        m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells[5].Value = DR[t]["NAME_SHR"];
                        m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells[1].Value = DR[t + 1]["Value"];
                        m_arPanelsTEC[i].TECDataGridView.Columns[5].Visible = false;

                        m_arPanelsTEC[i].TECDataGridView.Rows[r].Cells[3].Value = text.ToString();
                        PaintigCells(i, r);
                        CellsPingTEC(filter, i);
                    }
                    t = t + 2;
                }
            }


            /// <summary>
            /// Функция заполнения гридов
            /// </summary>
            public void AddItemTec()
            {
                for (int i = 0; i < m_arPanelsTEC.Length; i++)
                {
                    string filter = "ID_EXT = " + Convert.ToInt32(arraySourceTEC.Rows[i][0]);

                    if (m_arPanelsTEC[i].TECDataGridView.RowCount < 1)
                    {
                        AddRowsTEC(i, m_tableSourceData.Select(filter).Length);
                        TextColumnTec();
                    }

                    InsertDataTEC(filter, i);
                }
            }

            /// <summary>
            /// Функция изменения заголовков грида Tec
            /// </summary>
            public void HeaderGridTEC()
            {
                for (int i = 0; i < m_arPanelsTEC.Length; i++)
                {
                    string str = arraySourceTEC.Rows[i][@"NAME_SHR"].ToString();

                    if (m_arPanelsTEC[i].LabelTec.InvokeRequired)
                    {
                        m_arPanelsTEC[i].LabelTec.Invoke(new Action(() => m_arPanelsTEC[i].LabelTec.Text = str));
                    }
                    else
                    {
                        m_arPanelsTEC[i].LabelTec.Text = str;
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
                    string filter1 = "ID_Units = 12 and ID_EXT = '" + (k + 1) + "'";

                    for (int j = 0; j < m_arPanelsTEC[k].TECDataGridView.Rows.Count; j++)
                    {
                        DataRow[] DR = m_tableSourceData.Select(filter1);
                        string filter2 = "ID = " + DR[j]["ID_VALUE"];
                        DataRow[] dr = arrayParam.Select(filter2);

                        if (m_arPanelsTEC[k].TECDataGridView.InvokeRequired)
                        {
                            m_arPanelsTEC[k].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[k].TECDataGridView.Rows[j].Cells[0].Value = dr[0]["NAME_SHR"]));

                        }
                        else
                        {
                            m_arPanelsTEC[k].TECDataGridView.Rows[j].Cells[0].Value = dr[0]["NAME_SHR"];
                        }
                    }
                }
            }

            /// <summary>
            /// Добавление результата пропинговки в грид
            /// </summary>
            /// <param name="x"></param>
            public void CellsPingTEC(string f, int k)
            {
                DataRow[] dt;
                dt = m_tableSourceData.Select(f);

                for (int i = 0; i < m_arPanelsTEC[k].TECDataGridView.Rows.Count; i++)
                {
                    if (m_arPanelsTEC[k].TECDataGridView.InvokeRequired)
                    {
                        if (dt[i]["Link"].ToString() == "1")
                        {
                            m_arPanelsTEC[k].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[k].TECDataGridView.Rows[i].Cells[4].Value = "В сети"));
                        }
                        else m_arPanelsTEC[k].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[k].TECDataGridView.Rows[i].Cells[4].Value = "Проблемы с подключением"));
                    }
                    else
                    {
                        if (dt[i]["Link"].ToString() == "1")
                        {
                            m_arPanelsTEC[k].TECDataGridView.Rows[i].Cells[4].Value = "В сети";
                        }
                        else m_arPanelsTEC[k].TECDataGridView.Rows[i].Cells[4].Value = "Проблемы с подключением";
                    }
                }
            }

            /// <summary>
            /// закрашивает фон неактивного источника
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            public void PaintCellDeactive(int x, int y)
            {
                if (m_arPanelsTEC[x].TECDataGridView.InvokeRequired)
                {
                    m_arPanelsTEC[x].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[x].TECDataGridView.Rows[y].Cells[0].Style.BackColor = System.Drawing.Color.White));
                }
                else
                {
                    m_arPanelsTEC[x].TECDataGridView.Rows[y].Cells[0].Style.BackColor = System.Drawing.Color.White;
                }
            }

            /// <summary>
            /// закрашивает фон активного источника
            /// </summary>
            /// <param name="x">индекс панели</param>
            /// <param name="y">номер строки</param>
            public void PaintCellActive(int x, int y)
            {
                if (m_arPanelsTEC[x].TECDataGridView.InvokeRequired)
                {
                    m_arPanelsTEC[x].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[x].TECDataGridView.Rows[y].Cells[0].Style.BackColor = System.Drawing.Color.SeaGreen));
                }
                else
                {
                    m_arPanelsTEC[x].TECDataGridView.Rows[y].Cells[0].Style.BackColor = System.Drawing.Color.SeaGreen;
                }
            }

            /// <summary>
            /// Закраска ячейки ТМ
            /// </summary>
            public void PaintigCells(int x, int y)
            {
                string a = m_arPanelsTEC[x].TECDataGridView.Rows[y].Cells[5].Value.ToString();
                string b;

                for (int i = 0; i < massTM.Length / 2; i++)
                {
                    b = massTM[i, 1].ToString();

                    if (a == b)
                    {
                        PaintCellActive(x, y);

                        break;
                    }
                }
            }

            /// <summary>
            /// сборка тм массива
            /// </summary>
            public int[,] AssemblyList(DataTable table)
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
                return massTM12;
            }

            /// <summary>
            /// снятие выделения
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            /// <param name="k"></param>
            private void TECDataGridView_Cell(object sender, EventArgs e)
            {
                try
                {
                    TECDataGridView.SelectedCells[0].Selected = false;
                }
                catch { }
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
                //
                //ModesDataGridView
                //
                this.ModesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.ModesDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
                this.ModesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.ModesDataGridView.Dock = DockStyle.Fill;
                this.ModesDataGridView.ClearSelection();
                this.ModesDataGridView.AllowUserToAddRows = false;
                this.ModesDataGridView.RowHeadersVisible = false;
                this.ModesDataGridView.Name = "ModesDataGridView";
                this.ModesDataGridView.CurrentCell = null;
                this.ModesDataGridView.TabIndex = 0;
                this.ModesDataGridView.ColumnCount = 5;
                this.ModesDataGridView.Columns[0].Name = "Источник данных";
                this.ModesDataGridView.Columns[1].Name = "Крайнее время";
                this.ModesDataGridView.Columns[2].Name = "Крайнее значение";
                this.ModesDataGridView.Columns[3].Name = "Время проверки";
                this.ModesDataGridView.Columns[4].Name = "Связь";
                this.ModesDataGridView.Columns[0].Width = 25;
                this.ModesDataGridView.Columns[1].Width = 20;
                this.ModesDataGridView.Columns[2].Width = 30;
                this.ModesDataGridView.Columns[3].Width = 30;
                this.ModesDataGridView.Columns[4].Width = 40;
                this.ModesDataGridView.Resize += new EventHandler(m_arPanelsMODES_Cell);
                this.ModesDataGridView.CellClick += new DataGridViewCellEventHandler(m_arPanelsMODES_Cell);
                //
                //LabelModes
                //
                this.LabelModes.AutoSize = true;
                this.LabelModes.Dock = System.Windows.Forms.DockStyle.Fill;
                this.LabelModes.Name = "LabelModes";
                this.LabelModes.TabIndex = 1;
                this.LabelModes.Text = " ";
                this.LabelModes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

                this.ResumeLayout(false);
            }

            #endregion
        }

        partial class Modes
        {
            /// <summary>
            /// Старт MODES
            /// </summary>
            public void start_MODES()
            {
                Create_Modes();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="?"></param>
            public void ActivateMODES(bool activated)
            {
                if (activated == true)
                {
                    //AddItemModes();
                }
                else ;
            }

            /// <summary>
            /// уничтожение панелей
            /// </summary>
            public void stopMODES()
            {
                if (!(m_arPanelsMODES == null))
                {
                    for (int i = 0; i < m_arPanelsMODES.Length; i++)
                    {
                        m_arPanelsMODES[i].Dispose();
                    }
                }
            }

            /// <summary>
            /// Создание панелей Модес
            /// </summary>
            /// <param name="table"></param>
            public void Create_Modes()
            {
                DataRow[] dr;
                string filter12 = string.Empty;

                var stdDetails = (from r in tbModes.AsEnumerable()
                                  select new
                                  {
                                      ID = r.Field<int>("ID"),
                                  }).Distinct();

                m_arPanelsMODES = new Modes[stdDetails.Count()];

                for (int i = 0; i < stdDetails.Count(); i++)
                {
                    if (m_arPanelsMODES[i] == null)
                    {
                        m_arPanelsMODES[i] = new Modes();
                    }


                    if (Convert.ToInt32(stdDetails.ElementAt(i).ID) == 200)
                    {
                        filter12 = "DESCRIPTION = 'Modes-Centre'";

                        dr = tbModes.Select(filter12);

                        if (m_arPanelsMODES[i].ModesDataGridView.RowCount < dr.Count())
                        {
                            for (int t = 1; t < dr.Count(); t++)
                            {
                                AddRowsModes(i, 1);
                            }

                            TextColumnModes(i, filter12);
                        }
                    }

                    else
                    {
                        filter12 = "ID = " + Convert.ToInt32(stdDetails.ElementAt(i).ID);

                        dr = tbModes.Select(filter12);

                        if (m_arPanelsMODES[i].ModesDataGridView.RowCount < dr.Count())
                        {
                            for (int t = 0; t < dr.Count(); t++)
                            {
                                AddRowsModes(i, 1);
                            }

                            TextColumnModes(i, filter12);
                        }
                    }
                }

                HeaderTextModes();
            }

            /// <summary>
            /// добавление строк
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
            /// заполненеи панели МС данными
            /// </summary>
            /// <param name="i"></param>
            /// <param name="filter"></param>
            public void InsertDataMC(int i, string filter)
            {
                string filter1 = string.Empty;
                DataRow[] DR;
                int tic = -1;
                string text = DateTime.Now.ToString();
                DataRow[] dr;

                DR = tbModes.Select(filter);

                for (int d = 0; d < tbModes.Select(filter).Length - 1; d++)
                {
                    filter1 = @"ID_Value = '" + DR[d + 1][@"Component"] + "'";

                    dr = m_tableSourceData.Select(filter1);

                    tic++;

                    if (m_arPanelsMODES[i].ModesDataGridView.InvokeRequired)
                    {
                        if (CheckPBR() == dr[0]["Value"].ToString())
                        {
                            PaintPbrTrue(i, tic);
                        }
                        else PaintPbrError(i, tic);

                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[tic].Cells[2].Value = dr[1]["Value"]));
                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[tic].Cells[1].Value = dr[0]["Value"]));
                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[tic].Cells[3].Value = text.ToString()));

                        CellsPingMODES(filter1, i, tic);
                    }
                    else
                    {
                        if (CheckPBR() == dr[0]["Value"].ToString())
                        {
                            PaintPbrTrue(i, tic);
                        }
                        else PaintPbrError(i, tic);

                        m_arPanelsMODES[i].ModesDataGridView.Rows[tic].Cells[2].Value = dr[1]["Value"];
                        m_arPanelsMODES[i].ModesDataGridView.Rows[tic].Cells[1].Value = dr[0]["Value"];
                        m_arPanelsMODES[i].ModesDataGridView.Rows[tic].Cells[3].Value = text.ToString();

                        CellsPingMODES(filter1, i, tic);
                    }
                }
            }

            /// <summary>
            /// заполнение панели данными
            /// </summary>
            /// <param name="filter12"></param>
            /// <param name="i"></param>
            public void InsertDataModes(string filter12, int i)
            {
                string text = DateTime.Now.ToString();

                if (tbModes.Rows[i][@"NAME_SHR"].ToString() == "Modes-Centre")
                {
                    string filter2 = "DESCRIPTION = 'Modes-Centre'";
                    InsertDataMC(i, filter2);
                }

                else
                {
                    DataRow[] DR;
                    DataRow[] dr;

                    dr = tbModes.Select(filter12);

                    for (int r = 0; r < tbModes.Select(filter12).Length; r++)
                    {
                        string filterComp = "ID_Value = '" + dr[r][3].ToString() + "'";

                        DR = m_tableSourceData.Select(filterComp);

                        if (m_arPanelsMODES[i].ModesDataGridView.InvokeRequired)
                        {
                            if (CheckPBR() == DR[0]["Value"].ToString())
                            {
                                PaintPbrTrue(i, r);
                            }
                            else PaintPbrError(i, r);

                            m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[1].Value = DR[0]["Value"]));
                            m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[2].Value = DR[1]["Value"]));
                            m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[3].Value = text.ToString()));

                            CellsPingMODES(filterComp, i, r);
                        }
                        else
                        {
                            if (CheckPBR() == DR[0]["Value"].ToString())
                            {
                                PaintPbrTrue(i, r);
                            }
                            else PaintPbrError(i, r);

                            m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[1].Value = DR[0]["Value"];
                            m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[2].Value = DR[1]["Value"];
                            m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[3].Value = text.ToString();

                            CellsPingMODES(filterComp, i, r);
                        }
                    }
                }
            }

            /// <summary>
            /// Функция Заполнения панелей Модес
            /// </summary>
            public void AddItemModes()
            {
                var stdDetails = (from r in tbModes.AsEnumerable()
                                  select new
                                  {
                                      ID = r.Field<int>("ID"),
                                  }).Distinct();

                for (int i = 0; i < m_arPanelsMODES.Length; i++)
                {
                    string filter12 = "ID = " + Convert.ToInt32(stdDetails.ElementAt(i).ID);

                    InsertDataModes(filter12, i);
                }
            }

            /// <summary>
            /// Функция изменения заголовков грида Modes
            /// </summary>
            public void HeaderTextModes()
            {
                var stdDetails = (from r in tbModes.AsEnumerable()
                                  select new
                                  {
                                      ID = r.Field<string>("NAME_SHR"),
                                  }).Distinct();

                for (int i = 0; i < stdDetails.Count(); i++)
                {
                    string str = stdDetails.ToArray().ElementAt(i).ID;

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
            public void TextColumnModes(int k, string f)
            {
                DataRow[] dr = tbModes.Select(f);

                if (dr.Count() > 3)
                {
                    for (int d = 1; d < dr.Count(); d++)
                    {
                        string filter = "ID = " + dr[d][@"Component"].ToString() + "";
                        DataRow[] DR = array_GTP.Select(filter);

                        if (m_arPanelsMODES[k].ModesDataGridView.InvokeRequired)
                            m_arPanelsMODES[k].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[k].ModesDataGridView.Rows[d - 1].Cells[0].Value = DR[0]["NAME_SHR"]));
                        else
                        {
                            m_arPanelsMODES[k].ModesDataGridView.Rows[(d - 1)].Cells[0].Value = DR[0]["NAME_SHR"];
                        }
                    }
                }

                else
                {
                    for (int d = 0; d < dr.Count(); d++)
                    {
                        string filter = "ID = " + dr[d][@"Component"].ToString() + "";
                        DataRow[] DR = array_GTP.Select(filter);

                        if (m_arPanelsMODES[k].ModesDataGridView.InvokeRequired)
                            m_arPanelsMODES[k].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[k].ModesDataGridView.Rows[d].Cells[0].Value = DR[0]["NAME_SHR"]));
                        else
                        {
                            m_arPanelsMODES[k].ModesDataGridView.Rows[d].Cells[0].Value = DR[0]["NAME_SHR"];
                        }
                    }
                }
            }

            /// <summary>
            /// Добавление результата пропинговки в грид MODES
            /// </summary>
            /// <param name="x"></param>
            public void CellsPingMODES(string f, int k, int r)
            {
                DataRow[] DR;
                DR = m_tableSourceData.Select(f);

                if (m_arPanelsMODES[k].ModesDataGridView.InvokeRequired)
                {
                    if (DR[0]["Link"].ToString() == "1")
                    {
                        m_arPanelsMODES[k].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[k].ModesDataGridView.Rows[r].Cells[4].Value = "В сети"));

                    }

                    else m_arPanelsMODES[k].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[k].ModesDataGridView.Rows[r].Cells[4].Value = "Проблема с соединением"));
                }

                else
                {
                    if (DR[0]["Link"].ToString() == "1")
                    {
                        m_arPanelsMODES[k].ModesDataGridView.Rows[r].Cells[4].Value = "В сети";
                    }

                    else m_arPanelsMODES[k].ModesDataGridView.Rows[r].Cells[4].Value = "Проблема с соединением";
                }
            }

            /// <summary>
            /// снятие выделения ячейки
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            /// <param name="k"></param>
            private void m_arPanelsMODES_Cell(object sender, EventArgs e)
            {
                try
                {
                    ModesDataGridView.SelectedCells[0].Selected = false;
                }
                catch { }
            }

            /// <summary>
            /// мигание ячейки с неверным ПБР
            /// </summary>
            public void BlinkCells(int numP, int numR)
            {
                if (m_arPanelsMODES[numP].ModesDataGridView.Rows[numR].Cells[0].Style.BackColor == System.Drawing.Color.White)
                {
                    PaintPbrError(numP, numR);
                }
                PaintPbrTrue(numP, numR);
            }

            /// <summary>
            /// выделения верного ПБР
            /// </summary>
            public void PaintPbrTrue(int numP, int numR)
            {
                if (m_arPanelsMODES[numP].ModesDataGridView.InvokeRequired)
                {
                    m_arPanelsMODES[numP].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[numP].ModesDataGridView.Rows[numR].Cells[0].Style.BackColor = System.Drawing.Color.White));
                }

                m_arPanelsMODES[numP].ModesDataGridView.Rows[numR].Cells[0].Style.BackColor = System.Drawing.Color.White;
            }

            /// <summary>
            /// выделения неверного ПБР
            /// </summary>
            public void PaintPbrError(int numP, int numR)
            {
                if (m_arPanelsMODES[numP].ModesDataGridView.InvokeRequired)
                {
                    m_arPanelsMODES[numP].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[numP].ModesDataGridView.Rows[numR].Cells[0].Style.BackColor = System.Drawing.Color.Sienna));
                }

                m_arPanelsMODES[numP].ModesDataGridView.Rows[numR].Cells[0].Style.BackColor = System.Drawing.Color.Sienna;
            }

            /// <summary>
            /// нахождение эталоного пбр
            /// </summary>
            /// <returns></returns>
            public string CheckPBR()
            {
                string etalon_pbr = string.Empty;

                if ((Convert.ToInt32(DateTime.Now.AddHours(-3).Minute)) > 42)
                {
                    if ((Convert.ToInt32(DateTime.Now.AddHours(-3).Hour)) % 2 == 0)
                    {
                        etalon_pbr = "ПБР" + (Convert.ToInt32(DateTime.Now.AddHours(-3).Hour) + 1);
                    }

                    else
                    {
                        etalon_pbr = "ПБР" + ((Convert.ToInt32(DateTime.Now.AddHours(-3).Hour + 2)));
                    }

                    return etalon_pbr;
                }

                else
                {
                    if ((Convert.ToInt32(DateTime.Now.AddHours(-3).Hour)) % 2 == 0)
                    {
                        etalon_pbr = "ПБР" + (Convert.ToInt32(DateTime.Now.AddHours(-3).Hour) + 1);
                    }

                    else
                    {
                        etalon_pbr = "ПБР" + Convert.ToInt32(DateTime.Now.AddHours(-3).Hour);

                    }
                    return etalon_pbr;
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
            private DataTable table_task = new DataTable();
            int counter = -1;

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
                TaskCheckedListBox.SelectedIndexChanged += new EventHandler(TaskCheckedListBox_SelectedIndexChanged);
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
                this.TaskDataGridView.ColumnCount = 5;
                this.TaskDataGridView.Columns[0].Name = "Имя задачи";
                this.TaskDataGridView.Columns[1].Name = "Среднее время выполнения";
                this.TaskDataGridView.Columns[3].Name = "Время проверки";
                this.TaskDataGridView.Columns[2].Name = "Время выполнения задачи";
                this.TaskDataGridView.Columns[4].Name = "Описание";
                this.TaskDataGridView.Columns[4].Visible = false;
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
            enum Limit : int { min = 10, max = 20, minAVG = 1, maxAVG = 2 };

            /// <summary>
            /// Выборка из таблицы задачи
            /// </summary
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
                if (TaskCheckedListBox.InvokeRequired)
                {
                    TaskCheckedListBox.Invoke(new Action(() => TaskCheckedListBox.Items.Add(desc)));
                }
                else TaskCheckedListBox.Items.Add(desc);
            }

            /// <summary>
            /// Функция заполнения ячеек грида Task верменем
            /// </summary>
            public void ColumTimeTask(int i)
            {
                string text = DateTime.Now.ToString();

                TaskDataGridView.Rows[i].Cells[3].Value = text.ToString();
            }

            /// <summary>
            /// Добавление строк в грид
            /// </summary>
            public void AddRowsTask(int counter)
            {
                for (int x = 0; x < counter; x++)
                {
                    if (TaskDataGridView.InvokeRequired)
                    {
                        TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows.Add()));
                    }
                    else
                    {
                        TaskDataGridView.Rows.Add();
                    }
                }
            }

            /// <summary>
            /// Событие для чекбокса
            /// </summary>
            public void TaskCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
            {
                string curItem = TaskCheckedListBox.SelectedItem.ToString();
                string filter_task = "NAME_SHR = '" + curItem + "'";
                DataRow[] DR;

                DR = m_tableSourceData.Select(filter_task);

                if (TaskCheckedListBox.GetItemChecked(TaskCheckedListBox.SelectedIndex) == true)
                {
                    counter++;
                    AddRowsTask(1);
                    TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[counter].Cells[1].Value = DR[0]["Value"]));
                    TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[counter].Cells[2].Value = DR[1]["Value"]));
                    TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[counter].Cells[0].Value = DR[0]["NAME_SHR"]));

                    TaskDataGridView.Update();

                    ColumTimeTask(counter);
                }

                else
                {
                    string curItemDel = TaskCheckedListBox.SelectedItem.ToString();
                    string filterDel = "NAME_SHR = '" + curItemDel + "'";

                    for (int i = 0; i < TaskDataGridView.Rows.Count; i++)
                    {
                        if (TaskDataGridView.Rows[i].Cells[0].Value.ToString() == curItemDel)
                        {
                            TaskDataGridView.Rows.RemoveAt(i);
                        }
                    }
                    counter--;
                }
            }

            /// <summary>
            /// обновление гриад задач
            /// </summary>
            public void UpdateTaskGrid()
            {
                for (int i = 0; i < TaskDataGridView.RowCount; i++)
                {
                    if (TaskDataGridView.Rows[i].Cells[0].Value.ToString() == null)
                    {
                        TaskDataGridView.Rows.RemoveAt(i);
                    }

                    string filterTask = null;
                    filterTask = "NAME_SHR ='";
                    filterTask += TaskDataGridView.Rows[i].Cells[0].Value.ToString() + "'";

                    DataRow[] dr = m_tableSourceData.Select(filterTask);

                    TaskDataGridView.Rows[i].Cells[0].Value = dr[0]["NAME_SHR"].ToString();
                    TaskDataGridView.Rows[i].Cells[1].Value = dr[0]["Value"].ToString();
                    TaskDataGridView.Rows[i].Cells[2].Value = dr[1]["Value"].ToString();
                    ColumTimeTask(i);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void CheckBorderTime()
            {
                string nmshr = "ID_Value = 28";
                DataRow[] dr = m_tableSourceData.Select(nmshr);
                int lim;

                for (int i = 0; i < dr.Count(); i++)
                {
                    int avgtime = Convert.ToInt32(dr[i]["Value"]);

                    if (avgtime > (lim = (int)Limit.min))
                    {
                        if (avgtime > (lim = (int)Limit.max))
                        {
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[4].Visible = true));
                            WhatIsChecked(dr[i]["NAME_SHR"].ToString());
                            //TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[].Cells[].Value = "";
                        }
                        else
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[4].Visible = true));
                        //TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[].Cells[].Value = "" ));
                    }
                    else
                        TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[4].Visible = false));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void PntngRowsTask()
            {
                foreach (var item in TaskCheckedListBox.CheckedItems)
                {

                }

            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="itemChecked"></param>
            /// <returns></returns>
            public bool WhatIsChecked(string itemChecked)
            {
                return TaskCheckedListBox.GetItemChecked(TaskCheckedListBox.Items.IndexOf(itemChecked));
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        public PanelStatisticDiagnostic()
        {
            InitializeComponent();
        }

        public PanelStatisticDiagnostic(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        /// <summary>
        /// обработка события
        /// </summary>
        /// <param name="table"></param>
        public void m_DataSource_EvtRecievedTable(object table)
        {
            m_tableSourceData = (DataTable)table;
            fillGrid();
            DbSources.Sources().UnRegister(iListernID);
            AddPanel();
        }

        public void LoadData()
        {
            int err = -1; //Признак выполнения метода/функции
            //Зарегистрировать соединение/получить идентификатор соединения
            iListernID = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
            GetCurrentData(iListernID);
            m_DataSource = new HDataSource(new ConnectionSettings(InitTECBase.getConnSettingsOfIdSource(TYPE_DATABASE_CFG.CFG_200, iListernID, FormMainBase.s_iMainSourceData, -1, out err).Rows[0], 0));
            m_DataSource.StartDbInterfaces();
            m_DataSource.Start();
            m_DataSource.Command();
            m_DataSource.EvtRecievedTable += new DelegateObjectFunc(m_DataSource_EvtRecievedTable);
        }

        /// <summary>
        /// Создание строки коннекта
        /// </summary>
        public override void Start()
        {
            base.Start();

            LoadData();

            start();
        }

        /// <summary>
        /// вызов функций старта МОДЕС, ТЭЦ
        /// </summary>
        public void start()
        {
            StartPanelTec();
            StartModes();
        }

        private void fillGrid()
        {
            tecdb.AddItemTec();
            modesdb.AddItemModes();
            taskdb.UpdateTaskGrid();
        }

        /// <summary>
        /// Функция взятия информации из конф.БД
        /// </summary>
        public void GetCurrentData(int iListernID)
        {
            int err = -1;
            DbConnection dbconn = null;
            dbconn = DbSources.Sources().GetConnection(iListernID, out err);

            if ((err == 0) && (!(dbconn == null)))
            {
                arraySourceDataTask = DbTSQLInterface.Select(ref dbconn, "SELECT * FROM DIAGNOSTIC_TASK_SOURCES", null, null, out err);
                tbModes = DbTSQLInterface.Select(ref dbconn, "SELECT * FROM DIAGNOSTIC_TASK_MODES", null, null, out err);
                arrayParam = DbTSQLInterface.Select(ref dbconn, "SELECT * FROM DIAGNOSTIC_PARAM", null, null, out err);
                array_GTP = DbTSQLInterface.Select(ref dbconn, "SELECT * FROM GTP_LIST", null, null, out err);
                arraySource = DbTSQLInterface.Select(ref dbconn, "SELECT * FROM SOURCE", null, null, out err);
                arraySourceTEC = InitTEC_200.getListTEC(ref dbconn, false, out err);
            }
            else
                throw new Exception(@"Нет соединения с БД");

            CreateListTM(arraySourceTEC);

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
        /// Вызов функций прикрепления панелей ТЭЦ и Модес(заполнение задач)
        /// </summary>
        public void AddPanel()
        {
            TimerUp();
            AddPanelModes();
            AddPanelTEC();
            taskdb.GetDataTask(arraySourceDataTask);
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

                if (ModesTableLayoutPanel.InvokeRequired)
                {
                    ModesTableLayoutPanel.Invoke(new Action(() => ModesTableLayoutPanel.Controls.Add(m_arPanelsMODES[i].ModesDataGridView, col, row)));
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

                col = (int)(indx / TecTableLayoutPanel.RowCount);
                row = indx % (TecTableLayoutPanel.RowCount - 0);

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
            stop();

            tecdb.stopTEC();
            modesdb.stopMODES();

            base.Stop();
        }

        /// <summary>
        /// -
        /// </summary>
        private void stop()
        {
            if (!(UpdateTimer == null))
            {
                UpdateTimer.Enabled = false;
                UpdateTimer.Dispose();
                UpdateTimer = null;
            }
            else ;

            if (!(m_DataSource == null))
            {
                m_DataSource.Stop();
                m_DataSource = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activated"></param>
        /// <returns></returns>
        public override bool Activate(bool activated)
        {
            bool bRes = base.Activate(activated);

            if (activated == true)
            {
                tecdb.ActivateTEC(activated);
                modesdb.ActivateMODES(activated);
            }

            else
            {
                tecdb.ActivateTEC(activated);
                modesdb.ActivateMODES(activated);
            }

            return bRes;
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

                    if ((int)massTM[j, 0] == 0)
                    {
                        break;
                    }
                }

                while ((int)massTM[j, 0] != id);

                massTM.SetValue(arraySource.Rows[t][@"NAME_SHR"].ToString(), j, 1);
            }
        }

        /// <summary>
        /// Формирование строки запроса
        /// </summary>
        /// <param name="TM">новый параметр</param>
        /// <param name="tec">тэц</param>
        /// <returns>строка запроса</returns>
        static string StringQuery(int TM, int tec)
        {
            int err = -1;
            string query = string.Empty;

            switch (TM)
            {
                case 0:
                    query = "update TEC_LIST set  ID_LINK_SOURCE_DATA_TM = " + TM + " where ID =" + tec;
                    break;

                default:
                    query = "update TEC_LIST set  ID_LINK_SOURCE_DATA_TM = " + TM + " where ID =" + tec;
                    break;
            }
            return query;
        }

        /// <summary>
        /// обновление активного источника СОТИАССО
        /// </summary>
        static void UpdateTecTM(string query)
        {
            int err = -1;

            int iListernID = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");

            DbConnection m_dbConn = DbSources.Sources().GetConnection(iListernID, out err);

            DbTSQLInterface.ExecNonQuery(ref m_dbConn, query, null, null, out err);

            DbSources.Sources().UnRegister(iListernID);
        }
    }
}