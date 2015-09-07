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
    public class PanelStatisticDiagnostic1 : PanelStatistic
    {
        static string[,] massiveServ;
        static Modes[] m_arPanelsMODES;
        static Tec[] m_arPanelsTEC;
        static DataTable tbModes = new DataTable();
        static DataTable m_tableSourceData;
        public DataTable table_task = new DataTable();
        public DataTable arraySourceDataTask = new DataTable();
        static DataTable arraySource = new DataTable();
        static DataTable arraySourceTEC = new DataTable();
        static DataTable arrayKeys_DataSheet = new DataTable();
        static HDataSource m_DataSource;
        int counter = -1;
        private System.Timers.Timer aTimer;
        static System.Timers.Timer UpdateTimer;
        private System.Object lockThis = new System.Object();
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
            public delegate void Event(object data);
            public static Event EventHandler1;
        }

        /// <summary>
        /// Класс для обновления данных
        /// </summary>
        public class UpdateSource
        {
            PanelStatisticDiagnostic1 Pd1 = new PanelStatisticDiagnostic1();

            //home get home
            public UpdateSource()
            {

            }

            /// <summary>
            /// Перегрузка информации из бд
            /// </summary>
            public void RefreshDataSource()
            {
                //m_DataSource.EvtRecievedTable+=new DelegateObjectFunc(m_DataSource_EvtRecievedTable);
                Pd1.collection_data();
            }

            /// <summary>
            /// 
            /// </summary>
            public void Grids()
            {
                tecdb.AddItemTec();
                modesdb.AddItemModes();
            }

            /// <summary>
            /// ТАймер для пинга
            /// </summary>
            public void TimerPing()
            {
                UpdateTimer = new System.Timers.Timer(10000);
                UpdateTimer.Enabled = true;
                UpdateTimer.AutoReset = true;
                UpdateTimer.Elapsed +=new ElapsedEventHandler(UpdateTimer_Elapsed);
                //aTimer.Interval = 300000;
                GC.KeepAlive(UpdateTimer);
            }

           public void UpdateTimer_Elapsed(object source, ElapsedEventArgs e)
            {

            }

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
                this.TECDataGridView.Name = "TECDataGridView";
                this.TECDataGridView.TabIndex = 0;
                this.TECDataGridView.ColumnCount = 8;
                this.TECDataGridView.Columns[0].Name = "ТЭЦ";
                this.TECDataGridView.Columns[1].Name = "Источник данных";
                this.TECDataGridView.Columns[2].Name = "Время проверки";
                this.TECDataGridView.Columns[3].Name = "Крайнее значение";
                this.TECDataGridView.Columns[4].Name = "Связь";

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

            /// <summary>
            /// Старт TEC
            /// </summary>
            public void start_TEC()
            {
                Create_PanelTEC();
                AddItemTec();
            }

            /// <summary>
            /// Создание панелей ТЭЦ
            /// </summary>
            public void Create_PanelTEC()
            {
                rows = arraySourceTEC.Select();

                m_arPanelsTEC = new Tec[rows.Length];

                for (int i = 0; i < rows.Length; i++)
                {
                    m_arPanelsTEC[i] = new Tec();
                }
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
            /// Функция заполнения гридов
            /// </summary>
            public void AddItemTec()
            {
                for (int i = 0; i < m_arPanelsTEC.Length; i++)
                {
                    string filter = "ID_EXT = " + Convert.ToInt32(rows[i][0]);

                    //проверка на пустоту(ибо рано срабатывает)
                    while (m_tableSourceData == null)
                    {

                    }

                    int d = 0;

                    foreach (DataRow row in m_tableSourceData.Select(filter))
                    {
                        m_arPanelsTEC[i].TECDataGridView.Rows.Add();

                        for (int c = 0; c < row.ItemArray.Length; c++)
                        {
                            m_arPanelsTEC[i].TECDataGridView.Rows[d].Cells[c].Value = row.ItemArray[c]; 
                        }

                        d++;
                    }

                    /*if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                    {
                        m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.DataSource = cloneTable));
                    }
                    else
                    {
                        m_arPanelsTEC[i].TECDataGridView.DataSource = cloneTable;
                    }*/
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
                    else
                    {
                        m_arPanelsTEC[i].TECDataGridView.Columns[7].HeaderText = "ТЭЦ";
                        m_arPanelsTEC[i].TECDataGridView.Columns[1].HeaderText = "Источник данных";
                        m_arPanelsTEC[i].TECDataGridView.Columns[2].HeaderText = "Крайнее значение";
                        m_arPanelsTEC[i].TECDataGridView.Columns[0].Visible = false;
                        m_arPanelsTEC[i].TECDataGridView.Columns[3].Visible = false;
                        m_arPanelsTEC[i].TECDataGridView.Columns[4].HeaderText = "Время проверки";
                        m_arPanelsTEC[i].TECDataGridView.Columns[5].Visible = false;
                        m_arPanelsTEC[i].TECDataGridView.Columns[6].HeaderText = "Связь";
                        m_arPanelsTEC[i].TECDataGridView.Columns[7].DisplayIndex = 0;
                    }

                    if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                    {
                        m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].LabelTec.Text = str));
                        m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Update()));
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
                    for (int j = 0; j < m_arPanelsTEC[k].TECDataGridView.Rows.Count; j++)
                    {
                        string text1;
                        string text2 = m_arPanelsTEC[k].TECDataGridView.Rows[j].Cells[1].Value.ToString();

                        int s = 0;

                        do
                        {
                            text1 = arrayKeys_DataSheet.Rows[s][@"ID"].ToString();

                            s++;
                        }

                        while (text1 != text2);

                        string text = arrayKeys_DataSheet.Rows[s - 1][@"DESCRIPTION"].ToString();

                        if (m_arPanelsTEC[k].TECDataGridView.InvokeRequired)
                            m_arPanelsTEC[k].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[k].TECDataGridView.Rows[j].Cells[@"ID_Value"].Value = text.ToString()));
                        else
                        {
                            m_arPanelsTEC[k].TECDataGridView.Rows[j].Cells[1].Value = text.ToString();
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
                            m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Rows[j].Cells[@"UPDATEDATETIME"].Value = text.ToString()));
                        else
                        {
                            m_arPanelsTEC[i].TECDataGridView.Rows[j].Cells[4].Value = text.ToString();
                        }
                    }
                }
                HeaderGridTEC();
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
                        }

                        while (text1 != text2);

                        if (m_arPanelsTEC[k].TECDataGridView.InvokeRequired)
                        {
                            m_arPanelsTEC[k].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[k].TECDataGridView.Rows[i].Cells[6].Value = massiveServ[i, 0]));
                        }
                        else
                        {
                            m_arPanelsTEC[k].TECDataGridView.Rows[i].Cells[6].Value = massiveServ[i, 0];
                        }
                    }
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
                this.ModesDataGridView.ColumnCount = 8;
                this.ModesDataGridView.Columns[0].Name = "ТЭЦ";
                this.ModesDataGridView.Columns[1].Name = "Источник данных";
                this.ModesDataGridView.Columns[2].Name = "Время проверки";
                this.ModesDataGridView.Columns[3].Name = "Крайнее значение";
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
                //HeaderTextModes();
            }

            /// <summary>
            /// Функция Заполнения панелей Модес
            /// </summary>
            public void AddItemModes()
            {
                for (int i = 0; i < m_arPanelsMODES.Length; i++)
                {
                    string filter = "ID_EXT = " + Convert.ToInt32(rows_modes[i][0]);

                    //проверка на пустоту
                    while (m_tableSourceData == null)
                    {

                    }

                    int d = 0;
           
                    foreach (DataRow row in m_tableSourceData.Select(filter))
                    {
                        m_arPanelsMODES[i].ModesDataGridView.Rows.Add();

                        for (int c = 0; c < row.ItemArray.Length; c++)
                        {
                            m_arPanelsMODES[i].ModesDataGridView.Rows[d].Cells[c].Value = row.ItemArray[c];
                        }

                        d++;
                    }

                    //m_arPanelsMODES[i].ModesDataGridView.Rows.Add((DataRow)massivetb[i]);
                    /*for (int num = 0; num < d; num++)
                    {
                        if (m_arPanelsMODES[i].ModesDataGridView.InvokeRequired)
                        {
                            m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows.Add(massivetb[num,0])));
                        }

                        else
                        {
                            m_arPanelsMODES[i].ModesDataGridView.Rows.Add((DataRow)massivetb[num,0]);
                        }
                    }*/
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
                    if (m_arPanelsMODES[i].ModesDataGridView.InvokeRequired)
                    {
                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[7].HeaderText = "ТЭЦ"));
                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[0].Visible = false));
                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[1].HeaderText = "Источник данных"));
                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[2].HeaderText = "Крайнее значение"));
                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[3].Visible = false));
                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[6].HeaderText = "Связь"));
                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[5].Visible = false));
                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[4].HeaderText = "Время проверки"));
                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns[7].DisplayIndex = 0));
                    }
                    else
                    {
                        m_arPanelsMODES[i].ModesDataGridView.Columns[7].Name = "ТЭЦ";
                        m_arPanelsMODES[i].ModesDataGridView.Columns[0].Visible = false;
                        m_arPanelsMODES[i].ModesDataGridView.Columns[1].HeaderText = "Источник данных";
                        m_arPanelsMODES[i].ModesDataGridView.Columns[2].HeaderText = "Крайнее значение";
                        m_arPanelsMODES[i].ModesDataGridView.Columns[3].Visible = false;
                        m_arPanelsMODES[i].ModesDataGridView.Columns[6].HeaderText = "Связь";
                        m_arPanelsMODES[i].ModesDataGridView.Columns[5].Visible = false;
                        m_arPanelsMODES[i].ModesDataGridView.Columns[4].HeaderText = "Время проверки";
                        m_arPanelsMODES[i].ModesDataGridView.Columns[7].DisplayIndex = 0;

                    }

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
                        string text2 = m_arPanelsMODES[k].ModesDataGridView.Rows[j].Cells[1].Value.ToString();

                        int s = 0;

                        do
                        {
                            text1 = arrayKeys_DataSheet.Rows[s][@"ID"].ToString();

                            s++;
                        }

                        while (text1 != text2);

                        string text = arrayKeys_DataSheet.Rows[s - 1][@"DESCRIPTION"].ToString();

                        if (m_arPanelsMODES[k].ModesDataGridView.InvokeRequired)
                            m_arPanelsMODES[k].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[k].ModesDataGridView.Rows[j].Cells[1].Value = text.ToString()));
                        else
                        {
                            m_arPanelsMODES[k].ModesDataGridView.Rows[j].Cells[1].Value = text.ToString();
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
                            m_arPanelsMODES[k].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[k].ModesDataGridView.Rows[i].Cells[6].Value = massiveServ[s, 0]));
                        else
                        {
                            m_arPanelsMODES[k].ModesDataGridView.Rows[i].Cells[6].Value = massiveServ[s, 0];
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
                        m_arPanelsMODES[i].ModesDataGridView.Rows[j].Cells[4].Value = text.ToString();
                        /* if (m_arPanelsTEC[i].TECDataGridView.InvokeRequired)
                             m_arPanelsTEC[i].TECDataGridView.Invoke(new Action(() => m_arPanelsTEC[i].TECDataGridView.Rows[j].Cells[@"UPDATEDATETIME"].Value = text.ToString()));*/
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
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        public System.Windows.Forms.TableLayoutPanel ModesTableLayoutPanel;

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
            collection_data();
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddPanel()
        {
            AddPanelModes();
            AddPanelTEC();
            fw.StopWaitForm();
        }

        /// <summary>
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
        }

        /// <summary>
        /// 
        /// </summary>
        public void start()
        {
            PingSourceData();
            StartModes();
            StartPanelTec();
            AddPanel();
        }

        /// <summary>
        /// функция ожидания загрузки всех данных на форму
        /// </summary>
        public void WaitFunc()
        {
            fw.StartPosition = FormStartPosition.CenterScreen;
            fw.StartWaitForm();
        }

        /// <summary>
        /// Сбор информации с источников данных
        /// </summary>
        public void collection_data()
        {
            GetCurrentData();
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
            taskdb.GetDataTask(arraySourceDataTask);
            taskdb.TaskCheckedListBox.SelectedIndexChanged += new EventHandler(TaskCheckedListBox_SelectedIndexChanged);
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
        /// Функция взятия информации из конф.БД
        /// </summary>
        public void GetCurrentData()
        {
            ConnectionSettings CS = new ConnectionSettings
            {
                id = -1
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

            int ListenerId = DbSources.Sources().Register(CS, false, CS.name)
                , err = -1; ;

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
        }

        /// <summary>
        /// 
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
            //return massiveServ[,];
        }

        /// <summary>
        /// ТАймер для пинга
        /// </summary>
        public void TimerPing()
        {
            aTimer = new System.Timers.Timer(10000);
            aTimer.Enabled = true;
            aTimer.AutoReset = true;
            aTimer.Elapsed += new ElapsedEventHandler(OnElapsed_PingSourceData);
            //aTimer.Interval = 300000;
            GC.KeepAlive(aTimer);
        }

        /// <summary>
        /// Обработка события
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void OnElapsed_PingSourceData(object source, ElapsedEventArgs e)
        {
            PingSourceData();
        }
    }
}



