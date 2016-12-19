using HClassLibrary;
using StatisticCommon;
using System;
using System.ComponentModel;
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using System.Drawing;
using System.Linq;
using System.Timers;
using System.Windows.Forms; //TableLayoutPanel

namespace StatisticDiagnostic
{
    /// <summary>
    /// общий класс для панелей(???)
    /// </summary>
    public abstract partial class PanelDiagnostic : HPanelCommon
    {
        public PanelDiagnostic(string nameLabel)
            : base(-1, -1)
        {
            initialize();
        }

        public PanelDiagnostic(IContainer container)
            : base(container, -1, -1)
        {
            container.Add(this);

            initialize();
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override bool Activate(bool active)
        {
            return base.Activate(active);
        }

        protected abstract void LoadValue();

        protected abstract void Activated(bool activated);

        protected abstract void AddRows(int numP, int countRow);

        protected abstract void ClearGrid();

        protected abstract void CreateForm();

        protected abstract void CellsPingAdd();

        protected abstract string FormatTime(string datetime);
    }

    public abstract partial class PanelDiagnostic
    {
        private Label lblName;
        private DataGridView dgvCommon;

        private void initialize()
        {
            InitializeComponent();
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
        private void InitializeComponent()
        {
            this.Controls.Add(lblName, 0, 0);
            this.Controls.Add(dgvCommon, 0, 1);
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.74641F));

            this.dgvCommon = new DataGridView();
            this.lblName = new Label();
            this.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.SuspendLayout();
            this.dgvCommon.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCommon.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvCommon.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            this.dgvCommon.AllowUserToAddRows = false;
            this.dgvCommon.ClearSelection();
            this.dgvCommon.AllowUserToDeleteRows = false;
            this.dgvCommon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCommon.RowHeadersVisible = false;
            this.dgvCommon.ReadOnly = true;
            this.dgvCommon.CellValueChanged += dgvCommon_CellValueChanged;
            this.dgvCommon.CellClick += dgvCommon_CellValueChanged;

            //this.dgvCommon.ColumnCount = ;
            //this.dgvCommon.RowCount = ;

            this.lblName.AutoSize = true;
            this.lblName.Name = "Label";
            this.lblName.TabIndex = 0;
            this.lblName.Text = "Unknow";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Обработчик события - при "щелчке" по любой части ячейки
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие - (???ячейка, скорее - 'DataGridView')</param>
        /// <param name="e">Аргумент события</param>
        void dgvCommon_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgvCommon.SelectedCells.Count > 0)
                    dgvCommon.SelectedCells[0].Selected = false;
                else
                    ;
            }
            catch { }
        }

        #endregion
    }

    /// <summary>
    /// Класс для описания панели с информацией
    ///  по дианостированию состояния ИС
    /// </summary>
    public partial class PanelStatisticDiagnostic
    {
        /// <summary>
        /// Определить размеры ячеек макета панели
        /// </summary>
        /// <param name="cols">Количество столбцов в макете</param>
        /// <param name="rows">Количество строк в макете</param>
        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly(cols, rows);
        }

        /// <summary>
        /// Требуется переменная конструктора
        /// </summary>
        private IContainer components = null;

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
            SizeBDlabel = new System.Windows.Forms.Label();
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
            //
            //TaskTableLayoutPanel
            //
            this.TaskTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.TaskTableLayoutPanel.Dock = DockStyle.Fill;
            this.TaskTableLayoutPanel.AutoSize = true;
            this.TaskTableLayoutPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.TaskTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TaskTableLayoutPanel.ColumnCount = 2;
            this.TaskTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.TaskTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.TaskTableLayoutPanel.RowCount = 1;
            this.TaskTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TaskTableLayoutPanel.Controls.Add(m_taskdb.TaskDataGridView, 0, 0);
            this.TaskTableLayoutPanel.Controls.Add(m_sizedb.SizeDbDataGridView, 1, 0);
            //
            //LabelTableLayoutPanel
            //
            this.LabelTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.LabelTableLayoutPanel.Dock = DockStyle.Fill;
            this.LabelTableLayoutPanel.AutoSize = true;
            this.LabelTableLayoutPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None;
            this.LabelTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.LabelTableLayoutPanel.ColumnCount = 2;
            this.LabelTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.LabelTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.LabelTableLayoutPanel.RowCount = 1;
            this.LabelTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.LabelTableLayoutPanel.Controls.Add(Tasklabel, 0, 0);
            this.LabelTableLayoutPanel.Controls.Add(SizeBDlabel, 1, 0);

            this.Controls.Add(TEClabel, 0, 0);
            this.Controls.Add(TecTableLayoutPanel, 0, 1);
            this.Controls.Add(Modeslabel, 0, 2);
            this.Controls.Add(ModesTableLayoutPanel, 0, 3);
            this.Controls.Add(LabelTableLayoutPanel, 0, 4);
            this.Controls.Add(TaskTableLayoutPanel, 0, 5);

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
            //this.Tasklabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tasklabel.Name = "Tasklabel";
            this.Tasklabel.TabIndex = 0;
            this.Tasklabel.AutoSize = true;
            this.Tasklabel.Text = "Список задач";
            this.Tasklabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left))));

            this.SizeBDlabel.Size = new System.Drawing.Size(10, 10);
            //this.SizeBDlabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SizeBDlabel.Name = "SizeBDlabel";
            this.SizeBDlabel.TabIndex = 0;
            this.SizeBDlabel.AutoSize = true;
            this.SizeBDlabel.Text = "Информация БД";
            this.SizeBDlabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left))));

            this.Dock = DockStyle.Fill;

            this.ColumnCount = 1;
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.RowCount = 6;
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 87F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));

            createPanels();
            addPanel();

            this.ResumeLayout();
        }

        #endregion

        /// <summary>
        /// Подпись к вложенной панели с задачами
        /// </summary>
        private Label Tasklabel;
        /// <summary>
        /// Подпись к вложенной панели с параметрами диагностирования источников данных для ТЭЦ
        /// </summary>
        private Label TEClabel;
        /// <summary>
        /// Подпись к вложенной панели с параметрами диагностирования сервисов Модес
        /// </summary>
        private Label Modeslabel;
        /// <summary>
        /// Подпись к вложенной панели с параметрами Баз Данных
        /// </summary>
        private Label SizeBDlabel;
        /// <summary>
        /// Панель для размещения групповых элементов интерфейса с пользователем
        ///  с параметрами диагностирования источников данных для ТЭЦ
        /// </summary>
        private TableLayoutPanel TecTableLayoutPanel;
        /// <summary>
        /// Панель для размещения групповых элементов интерфейса с пользователем
        ///  с параметрами диагностирования сервисов Модес
        /// </summary>
        private TableLayoutPanel ModesTableLayoutPanel;
        /// <summary>
        /// Панель для размещения групповых элементов интерфейса с пользователем
        ///  с параметрами диагностирования сервисов Модес
        /// </summary>
        private TableLayoutPanel TaskTableLayoutPanel;
        /// <summary>
        /// Панель для размещения подписей для информационных окон
        /// </summary>
        private TableLayoutPanel LabelTableLayoutPanel;
    }

    /// <summary>
    /// Класс для описания панели с информацией
    ///  по дианостированию состояния ИС
    /// </summary>
    public partial class PanelStatisticDiagnostic : PanelStatistic
    {
        /// <summary>
        /// список активных источников
        /// </summary>
        static object[,] m_arrayActiveSource;
        static Modes[] m_arPanelsMODES;
        static Tec[] m_arPanelsTEC;

        public static DataTable m_tableSourceDiag = new DataTable();

        private static DataTable m_tableSourceData;        
        private static DataTable m_tableSourceList = new DataTable();
        private static DataTable m_tableGTPList = new DataTable();
        private static DataTable m_tableTECList = new DataTable();
        private static DataTable m_tableParamDiagnostic = new DataTable();
        /// <summary>
        /// Перечесления типов источников
        /// </summary>
        protected enum INDEX_SOURCE
        {
            UNKNOW = -1,
            SIZEDB = 10,
            MODES = 200,
            TASK = 300
        }
        /// <summary>
        /// 
        /// </summary>
        protected enum INDEX_UNITS
        {
            UNKNOW = 0,
            VALUE = 12, DATA = 13
        }
        /// <summary>
        /// экземпляр класса 
        /// для подклчения к бд
        /// </summary>
        static HDataSource m_DataSource;
        static System.Timers.Timer m_timerUpdate; //таймер для обновления панелей
        /// <summary>
        /// Экземпляры класса Task
        /// </summary>
        static Task m_taskdb = new Task();
        /// <summary>
        /// Экземпляры класса Modes
        /// </summary>
        static Modes m_modesdb = new Modes();
        /// <summary>
        /// Экземпляры класса SizeDb
        /// </summary>
        static SizeDb m_sizedb = new SizeDb();
        /// <summary>
        /// Экземпляры класса Tec
        /// </summary>
        public Tec m_tecdb = new Tec();

        /// <summary>
        /// 
        /// </summary>
        public static volatile int UPDATE_TIME,
            VALIDATE_ASKUE_TM;
        public static DateTime SERVER_TIME;//серверное время
        /// <summary>
        /// крайняя граница времени выполения задач
        /// </summary>
        static TimeSpan limTaskAvg = TimeSpan.FromSeconds(145);
        static TimeSpan limTask = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Создание и настройка таймера 
        /// для обновления данных на форме
        /// </summary>
        private void timerUp()
        {
            m_timerUpdate = new System.Timers.Timer();
            //m_timerUpdate.Enabled = true;
            m_timerUpdate.AutoReset = true;
            m_timerUpdate.Elapsed += new ElapsedEventHandler(UpdateTimer_Elapsed);
            m_timerUpdate.Interval = Convert.ToDouble(UPDATE_TIME);
            GC.KeepAlive(m_timerUpdate);
        }

        /// <summary>
        /// Обработчик события таймера
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void UpdateTimer_Elapsed(object source, ElapsedEventArgs e)
        {
            m_DataSource.Command();
        }

        /// <summary>
        /// Класс для обращения 
        /// к БД (чтение значений параметров для отображения)
        /// </summary>
        public class HDataSource : HHandlerDb
        {
            ConnectionSettings[] m_connSett;
            /// <summary>
            /// Перечисление типов состояний опроса данных
            /// </summary>
            protected enum State
            {
                ServerTime,
                Command,
                UpdateSource
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="connSett"></param>
            public HDataSource(ConnectionSettings connSett)
            {
                m_connSett = new ConnectionSettings[2];//??? why number
                m_connSett[(int)CONN_SETT_TYPE.LIST_SOURCE] = connSett;
                m_connSett[(int)CONN_SETT_TYPE.CONFIG_DB] = FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett();
                m_connSett[(int)CONN_SETT_TYPE.CONFIG_DB].id = ConnectionSettings.UN_ENUMERABLE_ID - 6;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void StartDbInterfaces()
            {
                if (m_dictIdListeners.ContainsKey(0) == false)
                    m_dictIdListeners.Add(0, new int[] { -1, -1 });

                register(0, (int)CONN_SETT_TYPE.LIST_SOURCE, m_connSett[(int)CONN_SETT_TYPE.LIST_SOURCE], m_connSett[(int)CONN_SETT_TYPE.LIST_SOURCE].name);
                register(0, (int)CONN_SETT_TYPE.CONFIG_DB, m_connSett[(int)CONN_SETT_TYPE.CONFIG_DB], m_connSett[(int)CONN_SETT_TYPE.CONFIG_DB].name);
            }

            public override void ClearValues()
            {
            }

            /// <summary>
            /// Добавить состояния в набор для обработки
            /// данных 
            /// </summary>
            public void Command()
            {
                lock (m_lockState)
                {
                    ClearStates();
                    AddState((int)State.UpdateSource);
                    AddState((int)State.ServerTime);
                    AddState((int)State.Command);
                    Run(@"StatisticDiagnostic.HHandlerDb :: Command");
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
                    case (int)State.ServerTime:
                        GetCurrentTimeRequest(DbInterface.DB_TSQL_INTERFACE_TYPE.MSSQL, m_dictIdListeners[0][(int)CONN_SETT_TYPE.LIST_SOURCE]);
                        actionReport(@"Получение времени с сервера БД - состояние: " + ((State)state).ToString());
                        break;
                    case (int)State.Command:
                        Request(m_dictIdListeners[0][(int)CONN_SETT_TYPE.LIST_SOURCE], @"SELECT * FROM Diagnostic");
                        actionReport(@"Получение значений из БД - состояние: " + ((State)state).ToString());
                        break;
                    case (int)State.UpdateSource:
                        Request(m_dictIdListeners[0][(int)CONN_SETT_TYPE.CONFIG_DB], InitTECBase.getQueryListTEC(false, new int[] { 0, 10 }));
                        actionReport(@"Обновление списка активных источников - состояние: " + ((State)state).ToString());
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
                    case (int)State.ServerTime:
                        iRes = response(m_IdListenerCurrent, out error, out table);
                        break;
                    case (int)State.Command:
                        iRes = response(m_IdListenerCurrent, out error, out table);
                        break;
                    case (int)State.UpdateSource:
                        iRes = response(m_IdListenerCurrent, out error, out table);
                        break;
                    default:
                        break;
                }
                return iRes;
            }

            /// <summary>
            /// Событие - получение данных 
            /// </summary>
            public event DelegateObjectFunc EvtRecievedTable;
            /// <summary>
            /// 
            /// </summary>
            public event DelegateObjectFunc EvtRecievedActiveSource;

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
                    case (int)State.ServerTime:
                        GetTimeServer((DateTime)(table as DataTable).Rows[0][0]);
                        break;
                    case (int)State.Command:
                        EvtRecievedTable(table);
                        break;
                    case (int)State.UpdateSource:
                        EvtRecievedActiveSource(table);
                        break;
                    default:
                        break;
                }

                //Проверить признак крайнего в наборе состояний для обработки
                if (isLastState(state) == true)
                    //Удалить все сообщения в строке статуса
                    ReportClear(true);
                else
                    ;

                return iRes;
            }

            /// <summary>
            /// Функция обратного вызова при возникновения ситуации "ошибка"
            ///  при обработке списка состояний
            /// </summary>
            /// <param name="state">Состояние при котором возникла ситуация</param>
            /// <param name="req">Признак результата выполнения запроса</param>
            /// <param name="res">Признак возвращения результата при запросе</param>
            /// <returns>Индекс массива объектов синхронизации</returns>
            protected override INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
            {
                INDEX_WAITHANDLE_REASON iRes = INDEX_WAITHANDLE_REASON.SUCCESS;

                errorReport(@"Получение значений из БД - состояние: " + ((State)state).ToString());

                return iRes;
            }

            /// <summary>
            /// Функция обратного вызова при возникновения ситуации "предупреждение"
            ///  при обработке списка состояний
            /// </summary>
            /// <param name="state">Состояние при котором возникла ситуация</param>
            /// <param name="req">Признак результата выполнения запроса</param>
            /// <param name="res">Признак возвращения результата при запросе</param>
            protected override void StateWarnings(int state, int req, int res)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Класс для описания элемента панели с информацией
        /// по дианостированию работоспособности 
        /// источников фактических, телеметрических значений (АИИС КУЭ, СОТИАССО) 
        /// </summary>
        public partial class Tec : HPanelCommon
        {
            private DataGridView m_dgvTesting = new DataGridView();
            private Label LabelTec = new Label();
            private ContextMenuStrip ContextmenuChangeState;
            //private ToolStripMenuItem toolStripMenuItemActivate;
            //private ToolStripMenuItem toolStripMenuItemDeactivate;

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
            private IContainer components = null;

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

            private enum INDEX_CONTEXTMENU_ITEM : short { ACTIVATED = 0, DEACTIVATED }

            #region Код, автоматически созданный конструктором компонентов

            /// <summary>
            /// Обязательный метод для поддержки конструктора - не изменяйте
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            private void InitializeComponentTEC()
            {
                this.Controls.Add(LabelTec, 0, 0);
                this.Controls.Add(m_dgvTesting, 0, 1);
                this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
                this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.74641F));

                this.ContextmenuChangeState = new System.Windows.Forms.ContextMenuStrip();
                this.ContextmenuChangeState.SuspendLayout();

                this.SuspendLayout();

                this.m_dgvTesting.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.m_dgvTesting.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.m_dgvTesting.ClearSelection();
                this.m_dgvTesting.AllowUserToDeleteRows = false;
                this.m_dgvTesting.Dock = System.Windows.Forms.DockStyle.Fill;
                this.m_dgvTesting.RowHeadersVisible = false;
                this.m_dgvTesting.ReadOnly = true;
                //this.TECDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.Fill);
                this.m_dgvTesting.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
                this.m_dgvTesting.AllowUserToAddRows = false;
                this.m_dgvTesting.Name = "TECDataGridView";
                this.m_dgvTesting.TabIndex = 0;
                this.m_dgvTesting.ColumnCount = (int)INDEX_CELL.COUNT;
                this.m_dgvTesting.Columns[(int)INDEX_CELL.NAME_SOURCE].Name = "Источник данных"; this.m_dgvTesting.Columns[(int)INDEX_CELL.NAME_SOURCE].Width = 43;
                this.m_dgvTesting.Columns[(int)INDEX_CELL.DATETIME_VALUE].Name = "Крайнее время"; this.m_dgvTesting.Columns[(int)INDEX_CELL.DATETIME_VALUE].Width = 57;
                this.m_dgvTesting.Columns[(int)INDEX_CELL.VALUE].Name = "Крайнее значение"; this.m_dgvTesting.Columns[(int)INDEX_CELL.VALUE].Width = 35;
                this.m_dgvTesting.Columns[(int)INDEX_CELL.DATETIME_NOW].Name = "Время проверки"; this.m_dgvTesting.Columns[(int)INDEX_CELL.DATETIME_NOW].Width = 57;
                this.m_dgvTesting.Columns[(int)INDEX_CELL.STATE].Name = "Связь"; this.m_dgvTesting.Columns[(int)INDEX_CELL.STATE].Width = 35;

                this.m_dgvTesting.CellClick += new DataGridViewCellEventHandler(TECDataGridView_Cell);
                this.m_dgvTesting.CellValueChanged += new DataGridViewCellEventHandler(TECDataGridView_Cell);
                this.m_dgvTesting.CellMouseDown += new DataGridViewCellMouseEventHandler(TECDataGridView_CellMouseDown);
                //
                //ContextmenuChangeState
                //
                this.ContextmenuChangeState.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    new ToolStripMenuItem () //Activated
                    , new ToolStripMenuItem() }); //Deactivated
                this.ContextmenuChangeState.Size = new System.Drawing.Size(180, 70);
                this.ContextmenuChangeState.ShowCheckMargin = true;
                // 
                // toolStripMenuItemActivate
                // 
                this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED].Name = "toolStripMenuItem1";
                this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED].Size = new System.Drawing.Size(179, 22);
                this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED].Text = "Activate";
                this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED].Click += new EventHandler(toolStripMenuItemActivate_Click);
                (this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED] as ToolStripMenuItem).CheckOnClick = true;
                this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED].Tag = INDEX_CONTEXTMENU_ITEM.ACTIVATED;
                // 
                // toolStripMenuItemDeactivate
                // 
                this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED].Name = "toolStripMenuItem2";
                this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED].Size = new System.Drawing.Size(179, 22);
                this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED].Text = "Deactivate";
                this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED].Click += new EventHandler(toolStripMenuItemDeactivate_Click);
                (this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED] as ToolStripMenuItem).CheckOnClick = false;
                this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED].Tag = INDEX_CONTEXTMENU_ITEM.DEACTIVATED;
                //
                // LabelTec
                //
                this.LabelTec.AutoSize = true;
                this.LabelTec.Name = "LabelTec";
                this.LabelTec.TabIndex = 1;
                this.LabelTec.Text = "Unknow_TEC";
                this.LabelTec.Dock = System.Windows.Forms.DockStyle.Fill;
                this.LabelTec.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                this.ContextmenuChangeState.ResumeLayout(false);
                this.ResumeLayout(false);
            }

            private enum INDEX_CELL : short { NAME_SOURCE = 0, DATETIME_VALUE, VALUE, DATETIME_NOW, STATE
                , COUNT  }

            /// <summary>
            /// Обработчик события - нажатие кнопки "мыши"
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие (DataGridView)</param>
            /// <param name="e">Аргумент события</param>
            void TECDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
            {
                if ((e.Button == MouseButtons.Right) && (e.RowIndex > -1))
                // только по нажатию правой кнопки и выбранной строки
                    if (m_dgvTesting.Rows[e.RowIndex].Cells[(int)INDEX_CELL.NAME_SOURCE].Value.ToString() != "АИИСКУЭ") {//??
                    // только для источников СОТИАССО 
                        RowIndex = e.RowIndex;
                        initContextMenu(m_dgvTesting.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor == s_ColorSOTIASSOState[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED]);
                        if ((sender as DataGridView).Rows[e.RowIndex].Cells[e.ColumnIndex].ContextMenuStrip == null)
                            (sender as DataGridView).Rows[e.RowIndex].Cells[e.ColumnIndex].ContextMenuStrip = ContextmenuChangeState;
                        else
                            ;
                    } else
                        ; // строки не являются описанием источников СОТИАССО
                else
                    ; // нажата не правая кнопка ИЛИ не выбрана строка
            }

            #endregion
        }

        /// <summary>
        /// Класс для описания элемента панели с информацией
        /// по диагностированию работоспособности 
        /// источников фактических, телеметрических значений (АИИС КУЭ, СОТИАССО)
        /// </summary>
        partial class Tec
        {
            //static PanelDiagnostic[] m_arPanelsTEC;
            /// <summary>
            /// Список номер истчоников СОТИАССО
            /// </summary>
            enum TM { TM1 = 2, TM2, TM1T, TM2T };

            /// <summary>
            /// Номер строки вызова контекстного меню
            /// </summary>
            private int RowIndex;

            ///// <summary>
            ///// Экземпляр класса для получения 
            ///// координатов мышки
            ///// </summary>
            //private Point point = new Point();

            /// <summary>
            /// Функция активации панелей ТЭЦ
            /// </summary>
            /// <param name="activated"></param>
            public void ActivateTEC(bool activated)
            {
                if (activated == true)
                    if (!(m_arPanelsTEC == null))
                        for (int i = 0; i < m_arPanelsTEC.Length; i++)
                            m_arPanelsTEC[i].Focus();
                    else
                        ;
                else
                    ;
            }

            /// <summary>
            /// очистка панелей
            /// </summary>
            public void Clear()
            {
                if (!(m_arPanelsTEC == null))
                    clearGrid();
            }

            /// <summary>
            /// Изменение в массиве активного 
            /// источника СОТИАССО
            /// </summary>
            /// <param name="tm">номер истчоника</param>
            /// <param name="nameTM">имя источника</param>
            /// <param name="pos">позиция в массиве</param>
            private void changenumSource(int tm, string nameTM, int pos)
            {
                m_arrayActiveSource.SetValue((tm), pos, 0);
                m_arrayActiveSource.SetValue(nameTM, pos, 1);
            }

            /// <summary>
            /// Функция нахождения источника СОТИАССО
            /// </summary>
            /// <param name="nameTec">имя источника СОТИАССО</param>
            /// <returns>номер источника СОТИАССО</returns>
            private object selectionArraySource(string nameTec)
            {
                DataRow[] m_foundrow = m_tableSourceList.Select("NAME_SHR = '" + nameTec + "'");
                object a = null;

                for (int i = 0; i < m_foundrow.Count(); i++)
                    a = m_foundrow[i]["ID"].ToString();

                return a;
            }

            private static Color[] s_ColorSOTIASSOState = new Color[] { Color.DeepSkyBlue, Color.Empty };

            /// <summary>
            /// Обработка события клика по пункту меню "Active"
            /// для активации нового источника СОТИАССО
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void toolStripMenuItemActivate_Click(object sender, EventArgs e)
            {
                string a = m_dgvTesting.Rows[RowIndex].Tag.ToString();
                int t = Convert.ToInt32(selectionArraySource(a));
                int numberPanel = (t / 10) - 1;

                updateTecTM(stringQuery(t, numberPanel + 1));

                for (int i = 0; i < m_arPanelsTEC[numberPanel].m_dgvTesting.Rows.Count; i++)
                    if (m_arPanelsTEC[numberPanel].m_dgvTesting.Rows[i].Cells[(int)INDEX_CELL.NAME_SOURCE].Style.BackColor == s_ColorSOTIASSOState[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED])
                        paintCellDeactive(numberPanel, i);
                    else
                        ;

                paintCellActive(numberPanel, RowIndex);
                changenumSource(t, a, numberPanel);
            }

            /// <summary>
            /// Обработка события клика по пункту меню "Deactive"
            /// для деактивации активного итсочника СОТИАССО
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void toolStripMenuItemDeactivate_Click(object sender, EventArgs e)
            {                
                string a = m_dgvTesting.Rows[RowIndex].Tag.ToString();
                int t = Convert.ToInt32(selectionArraySource(a));
                int numberPanel = (t / 10) - 1;

                paintCellDeactive(numberPanel, RowIndex);
                updateTecTM(stringQuery(0, numberPanel + 1));
            }

            /// <summary>
            /// Создание панелей ТЭЦ
            /// </summary>
            public void Create_Panel()
            {
                m_arPanelsTEC = new Tec[m_tableTECList.Rows.Count];

                for (int i = 0; i < m_arPanelsTEC.Length; i++)
                    if (m_arPanelsTEC[i] == null)
                        m_arPanelsTEC[i] = new Tec();
                    else
                        ;

                SourceNameTEC();
            }

            /// <summary>
            /// Добавление строк
            /// </summary>
            /// <param name="x">номер панели</param>
            /// <param name="cntRow">кол-во строк</param>
            private void addRows(int x, int cntRow)
            {
                Action<int> actionAddRows = new Action<int>((cnt) => { m_arPanelsTEC[x].m_dgvTesting.Rows.Add(cnt); });

                if (m_arPanelsTEC[x].m_dgvTesting.RowCount < cntRow / 2)
                    if (m_arPanelsTEC[x].m_dgvTesting.InvokeRequired)
                        m_arPanelsTEC[x].m_dgvTesting.Invoke(actionAddRows, cntRow / 2);
                    else
                        actionAddRows(cntRow / 2);
                else
                    ;
            }

            /// <summary>
            /// Функция проверки на пустоту значений
            /// </summary>
            /// <param name="sourceDR">набор проверяемых данных</param>
            /// <returns></returns>
            private bool testingNull(ref DataRow[] sourceDR)
            {
                bool bRes = false;

                for (int i = 0; i < sourceDR.Count(); i++)
                    if (string.IsNullOrEmpty(sourceDR[i]["Value"].ToString()) == true) {
                        sourceDR[i]["Value"] = "Нет данных в БД";
                        /*ничего не делаем*/
                        //bRes = false;
                    }
                    else
                        if (bRes == false) bRes = true; else /*ничего не делаем*/;

                return bRes;
            }

            /// <summary>
            /// Функция заполнения данными элементов ТЭЦ
            /// </summary>
            /// <param name="filter">фильтр для обработки данных</param>
            /// <param name="i">номер панели</param>
            private void fillValues(string filter, int i)
            {
                DataRow[] drTecSource;
                string nameSource
                    , shortTime;

                drTecSource = m_tableSourceData.Select(filter, "NAME_SHR DESC");

                setTextColumn(i);

                for (int r = 0, t = 0; r < m_arPanelsTEC[i].m_dgvTesting.Rows.Count; r++, t += 2) {
                    nameSource = m_arPanelsTEC[i].m_dgvTesting.Rows[r].Cells[(int)INDEX_CELL.NAME_SOURCE].Value.ToString();

                    shortTime = formatTime(drTecSource[t + 1]["Value"].ToString(), i, nameSource);
                    m_arPanelsTEC[i].m_dgvTesting.Invoke(new Action(() => m_arPanelsTEC[i].m_dgvTesting.Rows[r].Cells[(int)INDEX_CELL.DATETIME_VALUE].Value = shortTime));
                    m_arPanelsTEC[i].m_dgvTesting.Invoke(new Action(() => m_arPanelsTEC[i].m_dgvTesting.Rows[r].Cells[(int)INDEX_CELL.VALUE].Value = drTecSource[t]["Value"]));                    
                    m_arPanelsTEC[i].m_dgvTesting.Invoke(new Action(() => m_arPanelsTEC[i].m_dgvTesting.Rows[r].Cells[(int)INDEX_CELL.DATETIME_NOW].Value =
                        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("hh:mm:ss:fff")));

                    m_arPanelsTEC[i].m_dgvTesting.Invoke(new Action(() => m_arPanelsTEC[i].m_dgvTesting.Rows[r].Tag = drTecSource[t]["NAME_SHR"]));

                    paintingCells(i, r);

                    if (testingNull(ref drTecSource) == true)
                        checkRelevanceValues(DateTime.Parse(shortTime), i, r);
                    else
                        ;
                }
            }

            /// <summary>
            /// Функция заполнения гридов
            /// </summary>
            public void AddItem()
            {
                string filter;

                for (int i = 0; i < m_tableTECList.Rows.Count; i++)
                {
                    filter = "ID_EXT = " + Convert.ToInt32(m_tableTECList.Rows[i][0]);

                    addRows(i, m_tableSourceData.Select(filter).Length);

                    fillValues(filter, i);
                }

                cellsPingTEC();
            }

            /// <summary>
            /// Функция для подписи элементов 
            /// внутри элемента панели ТЭЦ
            /// </summary>
            private void SourceNameTEC()
            {
                for (int i = 0; i < m_arPanelsTEC.Length; i++)
                {
                    if (m_arPanelsTEC[i].LabelTec.InvokeRequired)
                        m_arPanelsTEC[i].LabelTec.Invoke(new Action(() => m_arPanelsTEC[i].LabelTec.Text =
                            m_tableTECList.Rows[i][@"NAME_SHR"].ToString()));
                    else
                        m_arPanelsTEC[i].LabelTec.Text = m_tableTECList.Rows[i][@"NAME_SHR"].ToString();
                }
            }

            /// <summary>
            /// Функция перемеинования ячейки датагрид TEC
            /// </summary>
            /// <param name="panelNum"></param>
            private void setTextColumn(int panelNum)
            {
                string filterSourceData, filterParamDiagnostic;
                DataRow[] arSelSourceData = null
                    , arSelParamDiagnostic = null;

                Action<int, string> setTextNameSource =
                    new Action<int, string>((indx, name_shr) => { m_arPanelsTEC[panelNum].m_dgvTesting.Rows[indx].Cells[(int)INDEX_CELL.NAME_SOURCE].Value = name_shr; });

                //for (int k = 0; k < m_dtTECList.Rows.Count; k++)
                //{
                    filterSourceData = "ID_Units = 12 and ID_EXT = '" + (panelNum + 1) + "'";

                    for (int j = 0; j < m_arPanelsTEC[panelNum].m_dgvTesting.Rows.Count; j++) {
                        arSelSourceData = m_tableSourceData.Select(filterSourceData, "NAME_SHR DESC");

                        filterParamDiagnostic = "ID = '" + arSelSourceData[j]["ID_VALUE"] + "'";
                        arSelParamDiagnostic = m_tableParamDiagnostic.Select(filterParamDiagnostic);

                        if (m_arPanelsTEC[panelNum].m_dgvTesting.InvokeRequired)
                            m_arPanelsTEC[panelNum].m_dgvTesting.Invoke(setTextNameSource, j, arSelParamDiagnostic[0]["NAME_SHR"].ToString());
                        else
                            setTextNameSource (j, arSelParamDiagnostic[0]["NAME_SHR"].ToString());
                    }
                //}
            }

            private struct STATE_SOURCE
            {
                public string m_Text;

                public Color m_Color;
            }

            private enum INDEX_STATE : short { OK = 0, ERROR }

            private static STATE_SOURCE[] s_StateSources = new STATE_SOURCE[] {
                new STATE_SOURCE() { m_Text = @"Да", m_Color = Color.White }
                , new STATE_SOURCE() { m_Text = @"Нет", m_Color = Color.Red }
            };

            /// <summary>
            /// Заполнение элемента панели 
            /// информацией о связи с истчоником ТЭЦ
            /// </summary>
            private void cellsPingTEC()
            {
                DataRow[] arSel = null;

                Action<int, int, INDEX_STATE> setState = new Action<int, int, INDEX_STATE>((num, iRow, iState) => {
                    m_arPanelsTEC[num].m_dgvTesting.Rows[iRow].Cells[(int)INDEX_CELL.STATE].Value = s_StateSources[(int)iState].m_Text;
                    m_arPanelsTEC[num].m_dgvTesting.Rows[iRow].Cells[(int)INDEX_CELL.STATE].Style.BackColor = s_StateSources[(int)iState].m_Color;
                });

                for (int j = 0; j < m_arPanelsTEC.Count(); j++)
                {
                    arSel = m_tableSourceData.Select(@"ID_EXT = " + Convert.ToInt32(m_tableTECList.Rows[j][0]));

                    for (int i = 0, t = 0; i < m_arPanelsTEC[j].m_dgvTesting.Rows.Count; i++, t += 2)
                        if (m_arPanelsTEC[j].m_dgvTesting.InvokeRequired == true)
                            m_arPanelsTEC[j].m_dgvTesting.Invoke(setState, j, i, (arSel[t]["Link"].ToString().Equals("1") == true) ? INDEX_STATE.OK : INDEX_STATE.ERROR);
                        else
                            setState(j, i, (arSel[t]["Link"].ToString().Equals("1") == true) ? INDEX_STATE.OK : INDEX_STATE.ERROR);
                }
            }

            /// <summary>
            /// Функция выделение 
            /// неактивного истчоника СОТИАССО
            /// </summary>
            /// <param name="x">номер панели</param>
            /// <param name="y">номер строки</param>
            private void paintCellDeactive(int x, int y)
            {
                paintCell(x, y, Color.Empty);
            }

            /// <summary>
            /// Функция выделения 
            /// активного источника СОТИАССО
            /// </summary>
            /// <param name="x">индекс панели</param>
            /// <param name="y">номер строки</param>
            private void paintCellActive(int x, int y)
            {
                paintCell(x, y, s_ColorSOTIASSOState[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED]);
            }

            private void paintCell(int x, int y, Color clrCell)
            {
                if (m_arPanelsTEC[x].m_dgvTesting.InvokeRequired)
                    m_arPanelsTEC[x].m_dgvTesting.Invoke(new Action(() => m_arPanelsTEC[x].m_dgvTesting.Rows[y].Cells[(int)INDEX_CELL.NAME_SOURCE].Style.BackColor = clrCell));
                else
                    m_arPanelsTEC[x].m_dgvTesting.Rows[y].Cells[(int)INDEX_CELL.NAME_SOURCE].Style.BackColor = clrCell;
            }

            /// <summary>
            /// Функция для выделения источника
            /// </summary>
            /// <param name="x">индекс панели</param>
            /// <param name="y">номер строки</param>
            private void paintingCells(int x, int y)
            {
                string a = m_arPanelsTEC[x].m_dgvTesting.Rows[y].Tag.ToString();
                string b;

                for (int i = 0; i < m_arrayActiveSource.Length / 2; i++)
                {
                    b = m_arrayActiveSource[i, 1].ToString();

                    if (a == b)
                    {
                        paintCellActive(x, y);
                        break;
                    }
                    else
                        paintCellDeactive(x, y);
                }
            }

            /// <summary>
            /// Подключение к ячейке контекстного меню
            /// </summary>
            /// <param name="y">номер строки</param>
            private void initContextMenu(bool bActived)
            {
                (this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED] as ToolStripMenuItem).CheckState = bActived == true ? CheckState.Checked : CheckState.Unchecked;
                (this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED] as ToolStripMenuItem).CheckState = bActived == false ? CheckState.Checked : CheckState.Unchecked;
            }

            /// <summary>
            /// Обработчик события - при "щелчке" по любой части ячейки
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие - (???ячейка, скорее - 'DataGridView')</param>
            /// <param name="e">Аргумент события</param>
            private void TECDataGridView_Cell(object sender, EventArgs e)
            {
                try {
                    if (m_dgvTesting.SelectedCells.Count > 0)
                        m_dgvTesting.SelectedCells[0].Selected = false;
                    else
                        ;
                } catch {
                }
            }

            /// <summary>
            /// очищение строк грида
            /// </summary>
            private void clearGrid()
            {
                for (int i = 0; i < m_arPanelsTEC.Length; i++)
                    for (int j = 0; j < m_arPanelsTEC[i].m_dgvTesting.Rows.Count; j++)
                        if (m_arPanelsTEC[i].m_dgvTesting.Rows.Count > 0)
                            m_arPanelsTEC[i].m_dgvTesting.Rows.Clear();
                        else
                            ;
            }

            /// <summary>
            /// Проверка актуальности времени 
            /// СОТИАССО и АИИСКУЭ
            /// </summary>
            /// <param name="time"></param>
            /// <param name="i">номер панели</param>
            /// <param name="r">индекс строки</param>
            private void checkRelevanceValues(DateTime time, int i, int r)
            {
                string nameSource = m_arPanelsTEC[i].m_dgvTesting.Rows[r].Cells[(int)INDEX_CELL.NAME_SOURCE].Value.ToString();

                if ((!(nameSource == "АИИСКУЭ"))
                    && m_arPanelsTEC[i].m_dgvTesting.Rows[r].Cells[(int)INDEX_CELL.NAME_SOURCE].Style.BackColor == System.Drawing.Color.Empty)
                    paintValuesSource(false, i, r);
                else
                    paintValuesSource(selectInvalidValue(nameSource, time, i), i, r);
            }

            /// <summary>
            /// Проверка разницы времени между эталоном и источником
            /// </summary>
            /// <param name="timeEtalon">эталонное время</param>
            /// <param name="timeSource">время источника</param>
            /// <returns>флаг о правильности времени</returns>
            private bool diffTime(DateTime timeEtalon, DateTime timeSource)
            {
                TimeSpan VALIDATE_TM = TimeSpan.FromSeconds(VALIDATE_ASKUE_TM);
                TimeSpan ts = timeEtalon - (timeSource + VALIDATE_TM);
                TimeSpan validateTime = TimeSpan.FromSeconds(180);

                if (ts > validateTime)
                    return true;
                else
                    return false;
            }
            /// <summary>
            /// Проверка актуальности времени источника
            /// </summary>
            /// <param name="nameS">имя источника</param>
            /// <param name="time">время источника</param>
            /// <param name="numberPanel">нопмер панели</param>
            /// <returns></returns>
            private bool selectInvalidValue(string nameS, DateTime time, int numberPanel)
            {
                DateTime DTnowAISKUE = SERVER_TIME;
                TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
                DateTime DTnowSOTIASSO;

                if ((numberPanel + 1) == 6)
                    DTnowSOTIASSO = TimeZoneInfo.ConvertTime(SERVER_TIME, TimeZoneInfo.Local);
                else
                    DTnowSOTIASSO = TimeZoneInfo.ConvertTimeToUtc(SERVER_TIME, tzInfo);

                bool bFL = true; ;

                switch (nameS)
                {
                    case "АИИСКУЭ":
                        if (diffTime(DTnowAISKUE, time))
                            bFL = true;
                        else
                            bFL = false;
                        break;
                    case "СОТИАССО":
                    case "СОТИАССО_TorIs":
                    case "СОТИАССО_0":
                        if (diffTime(DTnowSOTIASSO, time))
                            bFL = true;
                        else
                            bFL = false;
                        break;
                    default:
                        break;
                }

                return bFL;
            }

            /// <summary>
            /// Выделение значения источника
            /// </summary>
            /// <param name="bflag"></param>
            /// <param name="i">номер панели</param>
            /// <param name="r">номер строки</param>
            private void paintValuesSource(bool bflag, int i, int r)
            {
                m_arPanelsTEC[i].m_dgvTesting.Rows[r].Cells[2].Style.BackColor = bflag == true ? Color.Firebrick : Color.Empty;
            }

            /// <summary>
            /// Форматирование даты
            /// “HH:mm:ss.fff”
            /// </summary>
            /// <param name="datetime">дата входная</param>
            /// <param name="Npanel">номер панели</param>
            /// <param name="nameSource">имя источника</param>
            /// <returns>дата сформированная</returns>
            private string formatTime(string datetime, int Npanel, string nameSource)
            {
                DateTime result;
                string m_dt;
                string m_dt2Time = DateTime.TryParse(datetime, out result).ToString();

                switch (nameSource)
                {
                    case "СОТИАССО":
                    case "СОТИАССО_TorIs":
                    case "СОТИАССО_0":
                        if ((Npanel + 1) == 6)
                            result = result.AddHours(TimeZoneInfo.Local.BaseUtcOffset.Hours);
                        break;

                    default:
                        break;
                }

                if (m_dt2Time != "False")
                {
                    if (Convert.ToInt32(result.Day - DateTime.Now.Day) < 0)
                        return m_dt = result.ToString("dd.MM.yy HH:mm:ss");
                    else
                        return m_dt = result.ToString("HH:mm:ss.fff");
                }
                else
                    m_dt = result.ToString();

                return m_dt;
            }
        }

        /// <summary>
        /// Класс для описания элемента панели с информацией
        /// значений параметров диагностики работоспособности 
        /// источников значений ПБР (Модес-, центр, терминал)
        /// </summary>
        public partial class Modes : HPanelCommon
        {
            /// <summary>
            /// 
            /// </summary>
            public Modes()
                : base(-1, -1)
            {
                initialize();
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="container"></param>
            public Modes(IContainer container)
                : base(container, -1, -1)
            {
                container.Add(this);

                initialize();
            }
            /// <summary>
            /// 
            /// </summary>
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
                this.ModesDataGridView.ReadOnly = true;
                this.ModesDataGridView.ColumnCount = 5;
                this.ModesDataGridView.Columns[0].Name = "Источник данных";
                this.ModesDataGridView.Columns[1].Name = "Крайнее значение";
                this.ModesDataGridView.Columns[2].Name = "Крайнее время";
                this.ModesDataGridView.Columns[3].Name = "Время проверки";
                this.ModesDataGridView.Columns[4].Name = "Связь";
                this.ModesDataGridView.Columns[0].Width = 22;
                this.ModesDataGridView.Columns[1].Width = 15;
                this.ModesDataGridView.Columns[2].Width = 23;
                this.ModesDataGridView.Columns[3].Width = 20;
                this.ModesDataGridView.Columns[4].Width = 25;

                this.ModesDataGridView.CellValueChanged += new DataGridViewCellEventHandler(m_arPanelsMODES_Cell);
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

        /// <summary>
        /// Класс для описания элемента панели с информацией
        /// значений параметров диагностики работоспособности 
        /// источников значений ПБР (Модес-, центр, терминал)
        /// </summary>
        partial class Modes
        {
            /// <summary>
            /// Функция активации панелей модес
            /// </summary>
            /// <param name="activated">параметр активации</param>
            public void ActivateMODES(bool activated)
            {
                if (activated == true)
                    if (!(m_arPanelsMODES == null))
                        for (int i = 0; i < m_arPanelsMODES.Length; i++)
                            m_arPanelsMODES[i].Focus();
            }

            /// <summary>
            /// очистка панелей
            /// </summary>
            public void Clear()
            {
                if (!(m_arPanelsMODES == null))
                    clearGrid();
            }

            /// <summary>
            /// Очистка колекции грида
            /// </summary>
            private void clearGrid()
            {
                for (int i = 0; i < m_arPanelsMODES.Length; i++)
                    for (int j = 0; j < m_arPanelsMODES[i].ModesDataGridView.Rows.Count; j++)
                        if (m_arPanelsMODES[i].ModesDataGridView.Rows.Count > 0)
                            m_arPanelsMODES[i].ModesDataGridView.Rows.Clear();
            }

            /// <summary>
            /// Создание панелей Модес
            /// </summary>
            public void Create_Modes()
            {
                m_arPanelsMODES = new Modes[m_tableTECList.Rows.Count + 1];

                for (int i = 0; i < m_arPanelsMODES.Length; i++)
                {
                    if (m_arPanelsMODES[i] == null)
                        m_arPanelsMODES[i] = new Modes();
                }
            }

            /// <summary>
            /// Добавление строк в грид
            /// </summary>
            /// <param name="i">номер панели</param>
            /// <param name="counter">кол-во строк</param>
            private void addRowsModes(int i, int counter)
            {
                for (int x = 0; x < counter; x++)
                    if (m_arPanelsMODES[i].ModesDataGridView.InvokeRequired)
                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows.Add()));
                    else
                        m_arPanelsMODES[i].ModesDataGridView.Rows.Add();
            }

            /// <summary>
            /// Функция проверки на пустоту значений
            /// </summary>
            /// <param name="sourceDR">набор проверяемых данных</param>
            /// <returns></returns>
            private bool IsNUll(ref DataRow[] sourceDR)
            {
                bool blflag = false;

                for (int i = 0; i < sourceDR.Length; i++)
                    if (sourceDR[i]["Value"].ToString() == "")
                    {
                        sourceDR[i]["Value"] = "Нет данных в БД";
                        blflag = false;
                    }

                    else
                        blflag = true;

                return blflag;
            }

            /// <summary>
            /// заполненеи панели МС данными
            /// </summary>
            /// <param name="i">номер панели</param>
            /// <param name="filter">фильтр для отбора записей</param>
            private void insertDataMC(int i, string filter)
            {
                string m_filter1 = string.Empty
                    , sortOrderBy = "Component ASC"
                    , time;
                DataRow[] arSelIDModes, arSelIDComponent;
                int m_tic = -1;

                if (m_arPanelsMODES[i].ModesDataGridView.ColumnCount < 6)
                {
                    m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns.Add("TEC", "ТЭЦ")));
                    m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Columns["TEC"].DisplayIndex = 1));
                }

                arSelIDModes = m_tableSourceDiag.Select(filter, sortOrderBy);

                for (int d = 0; d < m_tableSourceDiag.Select(filter).Length - 1; d++)
                {
                    m_filter1 = @"ID_Value = '" + arSelIDModes[d + 1][@"Component"] + "'";

                    if (m_arPanelsMODES[i].ModesDataGridView.Rows.Count < m_tableSourceDiag.Select(filter).Length - 1)
                        addRowsModes(i, 1);
                    else;

                    arSelIDComponent = m_tableSourceData.Select(m_filter1);
                    time = formatTime(arSelIDComponent[1]["Value"].ToString());
                    m_tic++;

                    if (m_arPanelsMODES[i].ModesDataGridView.InvokeRequired)
                    {
                        if (IsNUll(ref arSelIDComponent))
                        {
                            if (checkPBR() == arSelIDComponent[0]["Value"].ToString())
                                paintPbrTrue(i, m_tic);
                            else
                                paintPbrError(i, m_tic);
                        }

                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[m_tic].Cells[5].Value = arSelIDComponent[0][5]));
                        //m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[m_tic].Cells[0].Value = m_drComponent[0]["ID_Value"]));
                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[m_tic].Cells[2].Value = time));
                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[m_tic].Cells[1].Value = arSelIDComponent[0]["Value"]));
                        m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[m_tic].Cells[3].Value = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("hh:mm:ss:fff")));

                        cellsPing(m_filter1, i, m_tic);
                    }
                    else
                    {
                        if (checkPBR() == arSelIDComponent[0]["Value"].ToString())
                            paintPbrTrue(i, m_tic);
                        else
                            paintPbrError(i, m_tic);

                        //m_arPanelsMODES[i].ModesDataGridView.Rows[m_tic].Cells[0].Value = m_drComponent[0]["ID_Value"];
                        m_arPanelsMODES[i].ModesDataGridView.Rows[m_tic].Cells[2].Value = time;
                        m_arPanelsMODES[i].ModesDataGridView.Rows[m_tic].Cells[1].Value = arSelIDComponent[0]["Value"];
                        m_arPanelsMODES[i].ModesDataGridView.Rows[m_tic].Cells[3].Value = DateTime.Now.ToString("HH:mm:ss.fff");

                        cellsPing(m_filter1, i, m_tic);
                    }
                    nameComponentGTP(arSelIDComponent, m_tic, i);
                }
            }

            /// <summary>
            /// добавление записей в грид
            /// </summary>
            /// <param name="filterSource">фильтр для отбора данных</param>
            /// <param name="i">индекс панели</param>
            private void insertDataModes(string filterSource, int i)
            {
                string textDateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("hh:mm:ss:fff")
                    , filterComp;
                DataRow[] arSelSourceModes;
                DataRow[] arSelComponentSource;
                string m_sortOrderBy = "Component ASC";

                if (m_tableSourceDiag.Rows[i][@"NAME_SHR"].ToString() == "Modes-Centre")
                    insertDataMC(i, "DESCRIPTION = 'Modes-Centre'");
                else {
                    arSelComponentSource = m_tableSourceDiag.Select(filterSource, m_sortOrderBy);

                    for (int r = 0; r < m_tableSourceDiag.Select(filterSource).Length; r++)
                    {
                        filterComp = "ID_Value = '" + arSelComponentSource[r][3].ToString() + "'";

                        if (m_arPanelsMODES[i].ModesDataGridView.Rows.Count < m_tableSourceDiag.Select(filterSource).Length)
                            addRowsModes(i, 1);
                        else
                            ;

                        arSelSourceModes = m_tableSourceData.Select(filterComp);

                        if (m_arPanelsMODES[i].ModesDataGridView.InvokeRequired) {
                            if (IsNUll(ref arSelSourceModes))
                                if (checkPBR() == arSelSourceModes[0]["Value"].ToString())
                                    paintPbrTrue(i, r);
                                else paintPbrError(i, r);
                            else
                                ;

                            //m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[0].Value = m_drSourceModes[0]["ID_Value"]));
                            m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[1].Value = arSelSourceModes[0]["Value"]));
                            m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[2].Value = formatTime(arSelSourceModes[1]["Value"].ToString())));
                            m_arPanelsMODES[i].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[3].Value = textDateTime));

                            cellsPing(filterComp, i, r);
                        } else {
                            if (checkPBR() == arSelSourceModes[0]["Value"].ToString())
                                paintPbrTrue(i, r);
                            else
                                paintPbrError(i, r);

                            //m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[0].Value = m_drSourceModes[0]["ID_Value"];
                            m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[1].Value = arSelSourceModes[0]["Value"];
                            //m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[2].Value = formatTime(m_drSourceModes[1]["Value"].ToString());
                            m_arPanelsMODES[i].ModesDataGridView.Rows[r].Cells[3].Value = textDateTime;

                            cellsPing(filterComp, i, r);
                        }

                        nameComponentGTP(arSelSourceModes, r, i);
                    }
                }
            }

            /// <summary>
            /// Функция Заполнения панелей Модес
            /// </summary>
            public void AddItem()
            {
                try {
                    var m_enumModes = (from r in m_tableSourceDiag.AsEnumerable()
                                       where r.Field<int>("ID") >= (int)INDEX_SOURCE.MODES && r.Field<int>("ID") < (int)INDEX_SOURCE.TASK
                                       orderby r.Field<int>("ID")
                                       select new
                                       {
                                           ID = r.Field<int>("ID"),
                                       }).Distinct();

                    for (int i = 0; i < m_arPanelsMODES.Length; i++)
                        insertDataModes("ID = " + Convert.ToInt32(m_enumModes.ElementAt(i).ID), i);

                    SourceNameText();
                } catch (Exception e) {
                    MessageBox.Show(e.ToString());
                }
            }

            /// <summary>
            /// Функция изменения заголовков грида Modes
            /// </summary>
            private void SourceNameText()
            {
                string m_nameshr;
                var m_enumModes = (from r in m_tableSourceDiag.AsEnumerable()
                                   where r.Field<int>("ID") >= (int)INDEX_SOURCE.MODES && r.Field<int>("ID") < (int)INDEX_SOURCE.TASK
                                   orderby r.Field<int>("ID")
                                   select new
                                   {
                                       NAME_SHR = r.Field<string>("NAME_SHR"),
                                   }).Distinct();

                for (int i = 0; i < m_enumModes.Count(); i++) {
                    m_nameshr = m_enumModes.ToArray().ElementAt(i).NAME_SHR;

                    if (m_arPanelsMODES[i].LabelModes.InvokeRequired)
                        m_arPanelsMODES[i].LabelModes.Invoke(new Action(() => m_arPanelsMODES[i].LabelModes.Text = m_nameshr));
                    else
                        m_arPanelsMODES[i].LabelModes.Text = m_nameshr;
                }
            }

            /// <summary>
            /// Функция подписи источников ПБР
            /// </summary>
            private void nameComponentGTP(DataRow[] dtComp, int rownext, int numPanel)
            {
                for (int j = 0; j < m_tableGTPList.Rows.Count; j++)
                    if (m_tableGTPList.Rows[j]["ID"].ToString() == dtComp[0]["ID_Value"].ToString()) {
                        if (m_arPanelsMODES[numPanel].ModesDataGridView.InvokeRequired)
                            m_arPanelsMODES[numPanel].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[numPanel].ModesDataGridView.Rows[rownext].Cells[0].Value = m_tableGTPList.Rows[j]["NAME_SHR"]));
                        else
                            m_arPanelsMODES[numPanel].ModesDataGridView.Rows[rownext].Cells[0].Value = m_tableGTPList.Rows[j]["NAME_SHR"];
                    } else
                        ;
            }

            /// <summary>
            /// Заполнение панели данными о связи 
            /// с источниками для МОДЕС
            /// </summary>
            /// <param name="f">фильтр для отбора данных</param>
            /// <param name="k">номер панели</param>
            /// <param name="r">номер строки</param>
            private void cellsPing(string f, int k, int r)
            {
                DataRow[] m_drLink = m_tableSourceData.Select(f);

                if (m_arPanelsMODES[k].ModesDataGridView.InvokeRequired)
                {
                    if (m_drLink[0]["Link"].ToString() == "1") {
                        m_arPanelsMODES[k].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[k].ModesDataGridView.Rows[r].Cells[4].Value = "Да"));
                        m_arPanelsMODES[k].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[k].ModesDataGridView.Rows[r].Cells[4].Style.BackColor = System.Drawing.Color.White));
                    } else {
                        m_arPanelsMODES[k].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[k].ModesDataGridView.Rows[r].Cells[4].Value = "Нет"));
                        m_arPanelsMODES[k].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[k].ModesDataGridView.Rows[r].Cells[4].Style.BackColor = System.Drawing.Color.OrangeRed));
                    }
                }
                else
                {
                    if (m_drLink[0]["Link"].ToString() == "1")
                    {
                        m_arPanelsMODES[k].ModesDataGridView.Rows[r].Cells[4].Value = "Да";
                        m_arPanelsMODES[k].ModesDataGridView.Rows[r].Cells[4].Style.BackColor = System.Drawing.Color.White;
                    }
                    else
                    {
                        m_arPanelsMODES[k].ModesDataGridView.Rows[r].Cells[4].Value = "Нет";
                        m_arPanelsMODES[k].ModesDataGridView.Rows[r].Cells[4].Style.BackColor = System.Drawing.Color.OrangeRed;
                    }
                }
            }

            /// <summary>
            /// снятие выделения ячейки
            /// </summary>
            /// <param name="sender">параметр</param>
            /// <param name="e">событие</param>
            private void m_arPanelsMODES_Cell(object sender, EventArgs e)
            {
                try
                {
                    if (ModesDataGridView.SelectedCells.Count > 0)
                        ModesDataGridView.SelectedCells[0].Selected = false;
                    else;
                }
                catch { }
            }

            /// <summary>
            /// Выделение верного ПБР
            /// </summary>
            /// <param name="numP">номер панели</param>
            /// <param name="numR">номер строки</param>
            private void paintPbrTrue(int numP, int numR)
            {
                if (m_arPanelsMODES[numP].ModesDataGridView.InvokeRequired)
                    m_arPanelsMODES[numP].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[numP].ModesDataGridView.Rows[numR].Cells[1].Style.BackColor = Color.White));
                else
                    m_arPanelsMODES[numP].ModesDataGridView.Rows[numR].Cells[1].Style.BackColor = Color.White;
            }

            /// <summary>
            /// Выделение ячейки с ошибочным ПБР
            /// </summary>
            /// <param name="numP">номер панели</param>
            /// <param name="numR">номер строки</param>
            private void paintPbrError(int numP, int numR)
            {
                if (m_arPanelsMODES[numP].ModesDataGridView.InvokeRequired)
                    m_arPanelsMODES[numP].ModesDataGridView.Invoke(new Action(() => m_arPanelsMODES[numP].ModesDataGridView.Rows[numR].Cells[1].Style.BackColor = Color.OrangeRed));
                else
                    m_arPanelsMODES[numP].ModesDataGridView.Rows[numR].Cells[1].Style.BackColor = Color.OrangeRed;
            }

            /// <summary>
            /// Функция для нахрждения ПБР на текущее время
            /// </summary>
            /// <returns>возвращает ПБР на текущее время</returns>
            private string checkPBR()
            {
                string m_etalon_pbr = string.Empty,
                 m_DTMin = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("mm"),
                 m_DTHour = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("HH");

                //if ((Convert.ToInt32(m_DTMin)) > 41)
                //{
                //    if ((Convert.ToInt32(m_DTHour)) % 2 == 0)
                //        m_etalon_pbr = "ПБР" + (Convert.ToInt32(m_DTHour) + 1);
                //    else
                //        m_etalon_pbr = "ПБР" + ((Convert.ToInt32(m_DTHour) + 2));

                //    return m_etalon_pbr;
                //}

                //else
                //{
                    //if ((Convert.ToInt32(m_DTHour)) % 2 == 0)
                        m_etalon_pbr = "ПБР" + (Convert.ToInt32(m_DTHour) + 1);
                    //else
                    //    m_etalon_pbr = "ПБР" + Convert.ToInt32(m_DTHour);

                    return m_etalon_pbr;
                //}
            }

            /// <summary>
            /// Форматирование даты
            /// “HH:mm:ss.fff”
            /// </summary>
            /// <param name="datetime">дата/время</param>
            /// <returns>отформатированная дата</returns>
            private string formatTime(string datetime)
            {
                DateTime result;
                string m_dt, m_dt2Time = DateTime.TryParse(datetime, out result).ToString();

                if (m_dt2Time != "False")
                {
                    if (Convert.ToInt32(result.Day - DateTime.Now.Day) < 0)
                        return m_dt = DateTime.Parse(datetime).ToString("dd.MM.yy HH:mm:ss");
                    else
                        return m_dt = DateTime.Parse(datetime).ToString("HH:mm:ss.fff");
                }
                else
                    m_dt = datetime;

                return m_dt;
            }
        }

        /// <summary>
        /// Класс для описания элемента панели с информацией
        /// отображения значений параметров диагностики 
        /// работоспособности задач по расписанию
        /// </summary>
        public partial class Task : HPanelCommon
        {
            public DataGridView TaskDataGridView = new DataGridView();

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
                    components.Dispose();
                base.Dispose(disposing);
            }

            #region Код, автоматически созданный конструктором компонентов

            /// <summary>
            /// Обязательный метод для поддержки конструктора - не изменяйте
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            private void InitializeComponentTask()
            {
                TaskDataGridView = new System.Windows.Forms.DataGridView();
                this.SuspendLayout();

                this.TaskDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.TaskDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
                this.TaskDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.TaskDataGridView.Dock = DockStyle.Fill;
                this.TaskDataGridView.ClearSelection();
                this.TaskDataGridView.Name = "TaskDataGridView";
                this.TaskDataGridView.ColumnCount = 6;
                this.TaskDataGridView.Columns[0].Name = "Имя задачи";
                this.TaskDataGridView.Columns[1].Name = "Среднее время выполнения";
                this.TaskDataGridView.Columns[3].Name = "Время проверки";
                this.TaskDataGridView.Columns[2].Name = "Время выполнения задачи";
                this.TaskDataGridView.Columns[4].Name = "Описание ошибки";
                this.TaskDataGridView.Columns[5].Name = "Статус задачи";
                this.TaskDataGridView.Columns[0].Width = 30;
                this.TaskDataGridView.Columns[1].Width = 12;
                this.TaskDataGridView.Columns[3].Width = 5;
                this.TaskDataGridView.Columns[2].Width = 15;
                this.TaskDataGridView.Columns[4].Width = 20;
                this.TaskDataGridView.Columns[5].Width = 15;
                this.TaskDataGridView.RowHeadersVisible = false;
                this.TaskDataGridView.TabIndex = 0;
                this.TaskDataGridView.AllowUserToAddRows = false;
                this.TaskDataGridView.ReadOnly = true;

                this.TaskDataGridView.CellClick += TaskDataGridView_CellClick;
                this.TaskDataGridView.CellValueChanged += TaskDataGridView_CellClick;
                this.ResumeLayout();
            }

            #endregion;

            public System.Windows.Forms.TableLayoutPanel TaskTableLayoutPanel;
        }

        /// <summary>
        /// Класс для описания элемента панели с информацией
        /// отображения значений параметров диагностики 
        /// работоспособности задач по расписанию
        /// </summary>
        partial class Task
        {
            /// <summary>
            /// Функция активации
            /// </summary>
            /// <param name="?">параметр активации</param>
            public void ActivateTask(bool activated)
            {
                if (activated == true)
                {
                    if (!(TaskTableLayoutPanel == null))
                        TaskTableLayoutPanel.Focus();
                }
                else;
            }

            /// <summary>
            /// очистка панелей
            /// </summary>
            public void Clear()
            {
                if (!(TaskDataGridView == null))
                    TaskDataGridView.Rows.Clear();
            }

            /// <summary>
            /// Функция для заполнения 
            /// грида информацией о задачах
            /// </summary>
            public void AddItem()
            {
                try
                {
                    DataRow[] drNameTask;
                    string filter;
                    int enumCnt;

                    var m_enumIDtask = (from r in m_tableSourceData.AsEnumerable()
                                        where r.Field<string>("ID_Value") == "28"
                                        select new
                                        {
                                            NAME = r.Field<string>("NAME_SHR"),
                                        }).Distinct();

                    enumCnt = m_enumIDtask.Count();

                    if (TaskDataGridView.Rows.Count < enumCnt)
                        addRowsTask(enumCnt);

                    for (int i = 0; i < enumCnt; i++)
                    {
                        filter = "NAME_SHR = '" + m_enumIDtask.ElementAt(i).NAME + "'";
                        drNameTask = m_tableSourceData.Select(filter);

                        if (TaskDataGridView.InvokeRequired)
                        {
                            columTimeTask(i);
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[i].Cells[1].Value = ToDateTime(drNameTask[0]["Value"])));
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[i].Cells[2].Value = formatTime(drNameTask[1]["Value"].ToString())));
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[i].Cells[0].Value = drNameTask[0]["NAME_SHR"]));
                        }
                        else
                        {
                            columTimeTask(i);
                            TaskDataGridView.Rows[i].Cells[1].Value = drNameTask[0]["Value"];
                            TaskDataGridView.Rows[i].Cells[2].Value = formatTime(drNameTask[1]["Value"].ToString());
                            TaskDataGridView.Rows[i].Cells[0].Value = drNameTask[0]["NAME_SHR"];
                        }
                    }
                    overLimit();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Ошибка заполнения субобласти Задачи" + e + "");
                }
            }

            /// <summary>
            /// Снятие выделения с ячеек
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            void TaskDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
            {
                try
                {
                    if (TaskDataGridView.SelectedCells.Count > 0)
                        TaskDataGridView.SelectedCells[0].Selected = false;
                }
                catch { }
            }

            /// <summary>
            /// Форматирование даты
            /// “HH:mm:ss.fff”
            /// </summary>
            /// <param name="datetime">дата</param>
            /// <returns>форматированная дата</returns>
            private string formatTime(string datetime)
            {
                DateTime result;
                string m_dt;
                string m_dt2Time = DateTime.TryParse(datetime, out result).ToString();

                if (m_dt2Time != "False")
                {
                    if (Convert.ToInt32(result.Day - DateTime.Now.Day) < 0)
                        return m_dt = DateTime.Parse(datetime).ToString("dd.MM.yy HH:mm:ss");
                    else
                        return m_dt = DateTime.Parse(datetime).ToString("HH:mm:ss.fff");
                }
                else
                    m_dt = datetime;

                return m_dt;
            }

            /// <summary>
            /// Функция заполенния ячеек грида временем
            /// </summary>
            /// <param name="i">номер строки</param>
            private void columTimeTask(int i)
            {
                string m_timeNow =
                    TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("hh:mm:ss:fff");
                TaskDataGridView.Rows[i].Cells[3].Value = m_timeNow;
            }

            /// <summary>
            /// Преобразование времени выполнения задач
            /// </summary>
            /// <param name="m_strTime">значение ячейки</param>
            /// <returns>возврат даты или ошибки</returns>
            private string ToDateTime(object m_strTime)
            {
                string parseStr;

                if (m_strTime.ToString() != "")
                {
                    TimeSpan time = TimeSpan.FromSeconds(Convert.ToDouble(m_strTime));
                    parseStr = DateTime.Parse(Convert.ToString(time)).ToString("mm:ss");
                }
                else
                    parseStr = "Ошибка!";
                return parseStr;
            }

            /// <summary>
            /// Добавление строк
            /// </summary>
            /// <param name="counter">кол-во строк</param>
            private void addRowsTask(int counter)
            {
                for (int x = 0; x < counter; x++)
                {
                    if (TaskDataGridView.InvokeRequired)
                        TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows.Add()));
                    else
                        TaskDataGridView.Rows.Add();
                }
            }

            /// <summary>
            ///  Проверка работоспособности задач
            /// </summary>
            /// <param name="dtime">время задачи</param>
            /// <returns></returns>
            private bool interruptTask(string dtime)
            {
                if ((DateTime.Parse(SERVER_TIME.ToString()) - DateTime.Parse(dtime)).TotalHours > 1.0)
                    return true;
                else
                    return false;
            }

            /// <summary>
            /// выделение строки с превышением лимита выполенния задачи
            /// </summary>
            private void overLimit()
            {
                TimeSpan m_lim;
                int m_check = 0;
                DataRow[] drTask = m_tableSourceData.Select(@"ID_Value = '28'");
                int m_counter = 1;

                for (int i = 0; i < drTask.Count(); i++)
                {
                    if (TaskDataGridView.Rows[m_check].Cells[0].Value.ToString() == "Усреднитель данных из СОТИАССО")
                        m_lim = limTaskAvg;
                    else m_lim = limTask;

                    if (int.Parse(drTask[i]["Link"].ToString()) == 1)
                    {
                        if (drTask[i]["Value"].ToString() == "")
                        {
                            if (TaskDataGridView.Columns[4].Visible == false)
                                TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[4].Visible = true));

                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[4].Value = "Задача не выполняется"));
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[5].Value = ""));
                            upselectrow(m_check);
                            m_counter--;
                        }
                        else
                            if (interruptTask(drTask[i + 1]["Value"].ToString()))
                        {
                            if (TaskDataGridView.Columns[4].Visible == false)
                                TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[4].Visible = true));

                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[4].Value = "Задача не выполняется"));
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[5].Value = ""));
                            upselectrow(m_check);
                            m_counter--;
                        }
                        else
                                if (TimeSpan.FromSeconds(Convert.ToDouble(drTask[i]["Value"])) > m_lim)
                        {
                            if (TaskDataGridView.Columns[4].Visible == false)
                                TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[4].Visible = true));

                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[4].Value = "Превышено время выполнения задачи"));
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[5].Value = ""));
                            upselectrow(m_check);
                            m_counter--;
                        }
                        else
                        {
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].DefaultCellStyle.BackColor = Color.White));
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[4].Value = ""));
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[5].Value = ""));

                            if (m_counter == TaskDataGridView.Rows.Count)
                                if (TaskDataGridView.Columns[4].Visible == true)
                                    TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[4].Visible = false));
                                else;
                            else
                                m_counter++;
                        }
                    }
                    else
                    {
                        TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[5].Value = "Запрещена"));
                        upselectrow(m_check);
                        m_counter++;
                    }
                    m_check++;
                    i++;
                }

            }

            /// <summary>
            /// Перенос строки на вверх грида 
            /// при ошибки выполнения задачи
            /// </summary>
            /// <param name="row">индекс строки</param>
            private void upselectrow(int indxrow)
            {
                if (TaskDataGridView.InvokeRequired)
                {
                    TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows.Insert(0, 1)));

                    for (int i = 0; i < TaskDataGridView.Rows[indxrow + 1].Cells.Count; i++)
                        TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[0].Cells[i].Value = TaskDataGridView.Rows[indxrow + 1].Cells[i].Value));

                    if (Convert.ToString(TaskDataGridView.Rows[0].Cells[4].Value) == "Задача не выполняется")
                        TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[0].DefaultCellStyle.BackColor = Color.Firebrick));
                    else
                        if (TaskDataGridView.Rows[0].Cells[4].Value.ToString() == "Превышено время выполнения задачи")
                        TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[0].DefaultCellStyle.BackColor = Color.Sienna));
                    else
                            if (TaskDataGridView.Rows[0].Cells[5].Value.ToString() == "Запрещена")
                        TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[0].DefaultCellStyle.BackColor = Color.White));
                    TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows.RemoveAt(indxrow + 1)));
                }
                else
                {
                    TaskDataGridView.Rows.InsertCopy(indxrow, 0);
                    TaskDataGridView.Rows.RemoveAt(indxrow);
                }
            }
        }

        /// <summary>
        /// Класс для описания элемента панели с информацией
        /// отображения значений параметров диагностики 
        /// размера баз данных
        /// </summary>
        partial class SizeDb : HPanelCommon
        {
            public DataGridView SizeDbDataGridView = new DataGridView();

            public SizeDb()
                : base(-1, -1)
            {
                initialize();
            }

            public SizeDb(IContainer container)
                : base(container, -1, -1)
            {
                container.Add(this);

                initialize();
            }

            private void initialize()
            {
                InitializeComponentSizeDb();
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
            private void InitializeComponentSizeDb()
            {
                SizeDbDataGridView = new System.Windows.Forms.DataGridView();
                this.SuspendLayout();

                this.SizeDbDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.SizeDbDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
                this.SizeDbDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.SizeDbDataGridView.Dock = DockStyle.Fill;
                this.SizeDbDataGridView.ClearSelection();
                this.SizeDbDataGridView.Name = "SizeDbDataGridView";
                this.SizeDbDataGridView.ColumnCount = 3;
                this.SizeDbDataGridView.Columns[0].Name = "Имя базы данных";
                this.SizeDbDataGridView.Columns[1].Name = "Размер базы данных, МБ";
                this.SizeDbDataGridView.Columns[2].Name = "Время проверки";
                this.SizeDbDataGridView.RowHeadersVisible = false;
                this.SizeDbDataGridView.TabIndex = 0;
                this.SizeDbDataGridView.AllowUserToAddRows = false;
                this.SizeDbDataGridView.ReadOnly = true;

                this.SizeDbDataGridView.CellClick += SizeDbDataGridView_CellClick;
                this.SizeDbDataGridView.CellValueChanged += SizeDbDataGridView_CellClick;
                this.ResumeLayout();
            }

            #endregion;
        }

        /// <summary>
        /// Класс для описания элемента панели с информацией
        /// отображения значений параметров диагностики 
        /// размера баз данных
        /// </summary>
        partial class SizeDb
        {
            /// <summary>
            /// Загрузка значений
            /// </summary>
            public void LoadValues()
            {
                DataRow[] drSizeOF;
                int countID,
                 countrow = 0;

                var m_enumIDEXTDB = (from r in m_tableSourceDiag.AsEnumerable()
                                     where r.Field<int>("COMPONENT") >= (int)INDEX_SOURCE.SIZEDB && r.Field<int>("COMPONENT") < (int)INDEX_SOURCE.MODES - 100
                                     select new
                                     {
                                         COMPONENT = r.Field<int>("COMPONENT"),
                                     }).Distinct();

                countID = m_enumIDEXTDB.Count();

                for (int j = 0; j < countID; j++)
                {
                    string filter = "ID_EXT = '" + m_enumIDEXTDB.ElementAt(j).COMPONENT + "'";
                    drSizeOF = m_tableSourceData.Select(filter);

                    if (SizeDbDataGridView.RowCount < (countID * 2))
                        AddRows(countID);

                    AddItem(drSizeOF, countrow);
                    NameBD(drSizeOF, m_tableSourceDiag.Select("COMPONENT = '" + m_enumIDEXTDB.ElementAt(j).COMPONENT + "'"), countrow);
                    countrow = countrow + 2;
                }
            }

            /// <summary>
            /// Добавление данных в грид
            /// </summary>
            /// <param name="dr"></param>
            private void AddItem(DataRow[] dr, int row)
            {
                for (int i = 0; i < dr.Count(); i++)
                {
                    if (SizeDbDataGridView.InvokeRequired)
                    {
                        SizeDbDataGridView.Invoke(new Action(() => SizeDbDataGridView.Rows[row].Cells[1].Value = dr[i]["Value"]));
                        SizeDbDataGridView.Invoke(new Action(() => SizeDbDataGridView.Rows[row].Cells[2].Value = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("hh:mm:ss:fff")));
                        row++;
                    }
                    else
                    {
                        SizeDbDataGridView.Rows[row].Cells[1].Value = dr[i]["Value"];
                        SizeDbDataGridView.Rows[row].Cells[2].Value = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("hh:mm:ss:fff");
                        row++;
                    }
                }
            }

            /// <summary>
            /// Добавление строк в датагрид
            /// </summary>
            /// <param name="countRows">кол-во строк</param>
            private void AddRows(int countRows)
            {
                for (int i = 0; i < countRows; i++)
                {
                    if (SizeDbDataGridView.InvokeRequired)
                        SizeDbDataGridView.Invoke(new Action(() => SizeDbDataGridView.Rows.Add()));
                    else
                        SizeDbDataGridView.Rows.Add();
                }
            }

            /// <summary>
            /// Форматирование даты
            /// “HH:mm:ss.fff”
            /// </summary>
            /// <param name="datetime">дата</param>
            /// <returns>форматированная дата</returns>
            private string formatTime(string datetime)
            {
                DateTime result;
                string strDTRes = string.Empty;
                bool bDTRes = DateTime.TryParse(datetime, out result);

                if (bDTRes != false)
                {
                    if (Convert.ToInt32(result.Day - DateTime.Now.Day) < 0)
                        strDTRes = DateTime.Parse(datetime).ToString("dd.MM.yy HH:mm:ss");
                    else
                        strDTRes = DateTime.Parse(datetime).ToString("HH:mm:ss.fff");
                }
                else
                    strDTRes = datetime;

                return strDTRes;
            }

            /// <summary>
            /// Поименование источников информации
            /// </summary>
            /// <param name="drMain"></param>
            /// <param name="drSource"></param>
            private void NameBD(DataRow[] drMain, DataRow[] drSource, int nextrow)
            {
                for (int i = 0; i < drSource.Count(); i++)
                {
                    for (int j = 0; j < drSource.Count(); j++)
                    {
                        if (drMain[i]["ID_Value"].ToString() == drSource[j]["ID"].ToString())
                        {
                            SizeDbDataGridView.Rows[nextrow].Cells[0].Value = drSource[j]["DESCRIPTION"].ToString();
                            nextrow++;
                        }
                    }
                }
            }

            /// <summary>
            /// Снятие выделения с ячеек
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            void SizeDbDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
            {
                try
                {
                    if (SizeDbDataGridView.SelectedCells.Count > 0)
                        SizeDbDataGridView.SelectedCells[0].Selected = false;
                    else
                        ;
                }
                catch { }
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        public PanelStatisticDiagnostic()
        {
            initialize();
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="container"></param>
        public PanelStatisticDiagnostic(IContainer container)
        {
            container.Add(this);
            initialize();
        }

        /// <summary>
        /// Инициализация подключения к БД
        /// и компонентов панели.
        /// </summary>
        /// <returns></returns>
        private int initialize()
        {
            int err = -1; //Признак выполнения метода/функции
            // зарегистрировать соединение/получить идентификатор соединения
            int iListernID = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
            // зарегистрировать синхронное соединение с БД_конфигурации
            m_DataSource = new HDataSource(new ConnectionSettings(InitTECBase.getConnSettingsOfIdSource(iListernID, FormMainBase.s_iMainSourceData, -1, out err).Rows[0], -1));
            // назначить обработчик события - получение данных
            getCurrentData(iListernID);
            DbSources.Sources().UnRegister(iListernID);

            InitializeComponent();

            return err;
        }

        /// <summary>
        /// Обработчик события - получение данных при запросе к БД
        /// </summary>
        /// <param name="table">Результат выполнения запроса - таблица с данными</param>
        private void dataSource_OnEvtRecievedTable(object table)
        {
            m_tableSourceData = (DataTable)table;

            start();
        }

        /// <summary>
        /// Обработчик события - получение данных при запросе к БД
        /// получение списка активных источников
        /// </summary>
        /// <param name="table">Результат выполнения запроса - таблица с данными</param>
        private void dataSource_OnEvtRecievedActiveSource(object table)
        {
            createListActiveSource((DataTable)table);
        }

        /// <summary>
        /// Получение серверного времени
        /// </summary>
        /// <param name="dtTime"></param>
        public static void GetTimeServer(DateTime dtTime)
        {
            SERVER_TIME = dtTime;
        }

        /// <summary>
        /// Загрузить данные из БД
        /// </summary>
        private void dbStart()
        {
            m_DataSource.Start();
            m_DataSource.StartDbInterfaces();
            m_DataSource.EvtRecievedActiveSource += new DelegateObjectFunc(dataSource_OnEvtRecievedActiveSource);
            m_DataSource.EvtRecievedTable += new DelegateObjectFunc(dataSource_OnEvtRecievedTable);
        }

        /// <summary>
        /// Создать панели для отображения диагностических параметров ТЭЦ, Модес
        /// </summary>
        private void createPanels()
        {
            m_tecdb.Create_Panel();
            m_modesdb.Create_Modes();
        }

        /// <summary>
        /// Назначить делегаты по отображению сообщений в строке статуса
        /// </summary>
        /// <param name="ferr">Делегат для отображения в строке статуса ошибки</param>
        /// <param name="fwar">Делегат для отображения в строке статуса предупреждения</param>
        /// <param name="fact">Делегат для отображения в строке статуса описания действия</param>
        /// <param name="fclr">Делегат для удаления из строки статуса сообщений</param>
        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            m_DataSource.SetDelegateReport(ferr, fwar, fact, fclr);
        }

        /// <summary>
        /// Фукнция вызова старта программы
        /// (создание таймера и получения данных)
        /// </summary>
        public override void Start()
        {
            base.Start();
            timerUp();
            dbStart();
        }

        /// <summary>
        /// Вызов функций для заполнения 
        /// элементов панели данными
        /// </summary>
        private void start()
        {
            try
            {
                m_tecdb.AddItem();
                m_taskdb.AddItem();
                m_modesdb.AddItem();
                m_sizedb.LoadValues();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Получить значения из БД_конфигурации
        /// </summary>
        /// <param name="iListernID">индентификатор</param>
        private void getCurrentData(int iListernID)
        {
            int err = -1;
            DbConnection dbconn = null;
            dbconn = DbSources.Sources().GetConnection(iListernID, out err);

            if ((err == 0) && (!(dbconn == null)))
            {
                m_tableSourceDiag = DbTSQLInterface.Select(ref dbconn, "SELECT * FROM DIAGNOSTIC_SOURCES", null, null, out err);//task modes size
                m_tableSourceList = DbTSQLInterface.Select(ref dbconn, "SELECT * FROM SOURCE", null, null, out err);
                m_tableGTPList = DbTSQLInterface.Select(ref dbconn, "SELECT * FROM GTP_LIST", null, null, out err);
                m_tableParamDiagnostic = DbTSQLInterface.Select(ref dbconn, "SELECT * FROM DIAGNOSTIC_PARAM", null, null, out err);
                m_tableTECList = InitTEC_200.getListTEC(ref dbconn, false, new int[] { 0, 10 }, out err);
            }
            else
                throw new Exception(@"Нет соединения с БД"); ;
        }

        /// <summary>
        /// Вызов функций прикрепления панелей ТЭЦ и Модес
        /// </summary>
        private void addPanel()
        {
            addPanelModes();
            addPanelTEC();
        }

        /// <summary>
        /// Размещение datagridview
        /// на элементе панели Модес
        /// </summary>
        private void addPanelModes()
        {
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

                row = (int)(indx / ModesTableLayoutPanel.RowCount);
                col = indx % (ModesTableLayoutPanel.RowCount - 0);

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
        /// Размещение datagridview
        /// на элементе панели ТЭЦ
        /// </summary>
        private void addPanelTEC()
        {
            int i = -1,
             indx = -1
               , col = -1
               , row = -1;

            for (i = 0; i < m_arPanelsTEC.Length; i++)
            {
                indx = i;
                if (!(indx < RowCount))
                    //indx += (int)(indx / TecTableLayoutPanel.RowCount);

                    row = (int)(indx / TecTableLayoutPanel.RowCount);
                col = indx % (TecTableLayoutPanel.RowCount - 0);

                if (TecTableLayoutPanel.InvokeRequired)
                    TecTableLayoutPanel.Invoke(new Action(() => TecTableLayoutPanel.Controls.Add(m_arPanelsTEC[i], col, row)));
                else
                    TecTableLayoutPanel.Controls.Add(m_arPanelsTEC[i], col, row);
            }
        }

        private void clear()
        {
            m_tecdb.Clear();
            m_modesdb.Clear();
            m_taskdb.Clear();
        }

        /// <summary>
        /// Остановка работы панели
        /// </summary>
        public override void Stop()
        {
            if (Started == true)
            {
                clear();

                stop();                

                base.Stop();
            }
        }

        /// <summary>
        /// Остановка работы таймера
        /// и обработки запроса к БД
        /// </summary>
        private void stop()
        {
            if (!(m_DataSource == null))
            {
                m_timerUpdate.Stop();
                m_DataSource.StopDbInterfaces();
                m_DataSource.Stop();
            }
        }

        /// <summary>
        /// Функция активация Вкладки
        /// </summary>
        /// <param name="activated">параметр</param>
        /// <returns>результат</returns>
        public override bool Activate(bool activated)
        {
            bool bRes = base.Activate(activated);

            if (activated == true)
            {
                m_timerUpdate.Start();
                m_DataSource.Command();
            }
            else
                if (!(m_timerUpdate == null))
                m_timerUpdate.Stop();

            return bRes;
        }

        /// <summary>
        /// Создание списка активных 
        /// источников СОТИАССО для всех ТЭЦ
        /// </summary>
        /// <param name="table">список тэц</param>
        private void createListActiveSource(DataTable table)
        {
            m_arrayActiveSource = new object[table.Rows.Count, 2];

            for (int i = 0; i < table.Rows.Count; i++)
                m_arrayActiveSource.SetValue(table.Rows[i]["ID_LINK_SOURCE_DATA_TM"], i, 0);

            int t = -1;
            int id;

            for (int j = 0; j < table.Rows.Count; j++)
            {
                do
                {
                    t++;
                    id = (int)m_tableSourceList.Rows[t][@"ID"];

                    if ((int)m_arrayActiveSource[j, 0] == 0)
                        break;
                }

                while ((int)m_arrayActiveSource[j, 0] != id);

                m_arrayActiveSource.SetValue(m_tableSourceList.Rows[t][@"NAME_SHR"].ToString(), j, 1);
            }
        }

        /// <summary>
        /// Формирование строки запроса с параметром 
        /// активного источника СОТИАССО
        /// </summary>
        /// <param name="TM">новый параметр</param>
        /// <param name="tec">тэц</param>
        /// <returns>строка запроса</returns>
        static private string stringQuery(int TM, int tec)
        {
            string query = string.Empty;
            query = "update TEC_LIST set  ID_LINK_SOURCE_DATA_TM = " + TM + " where ID =" + tec;
            return query;
        }

        /// <summary>
        /// Функция на обновление парамтера активного 
        /// источника СОТИАССО в таблице TEC_LIST
        /// </summary>
        /// <param name="query">строка запроса</param>
        static void updateTecTM(string query)
        {
            int err = -1;
            int iListernID = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");

            DbConnection m_dbConn = DbSources.Sources().GetConnection(iListernID, out err);
            DbTSQLInterface.ExecNonQuery(ref m_dbConn, query, null, null, out err);
            DbSources.Sources().UnRegister(iListernID);
        }
    }
}