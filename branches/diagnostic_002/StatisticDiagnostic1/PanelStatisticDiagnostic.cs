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
        public Tec[] m_arPanelsTEC;
        public Modes[] m_arPanelsMODES;
        public DataTable m_tableSourceData;
        private DataTable m_CheckList;
        public DataTable arraySourceDataTask;
        //public DataTable arrayTEC;
        private DataTable arraySource;
        DataTable arraySourceServer;
        HDataSource m_DataSource;
        private object m_lockTimerGetData;
        ConnectionSettings m_connSett;
        public DataRow[] rows;
        private System.Threading.Timer m_timer;
        DataSet dsTEC = new DataSet();
        public enum ID_ASKED_DATAHOST
        {
            CONN_SETT
                ,}
     
        
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
                AddState((int)State.Command);
                Run(@"Command");
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
            /// <summary>
            /// Получает рез-т запроса  - таблицу, от источника с идентификатором, с признаком ошибки
            /// </summary>
            /// <param name="id">идентификатор</param>
            /// <param name="err">признак ошибки</param>
            /// <param name="tableRes">результирующая таблица</param>
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
                       // iRes = GetCurrentData(table as DataTable);
                        break;
                    default:
                        break;
                }
                return iRes;
            }

            protected override void StateErrors(int state, int req, int res)
            {
                throw new NotImplementedException();
            }

            protected override void StateWarnings(int state, int req, int res)
            {
                throw new NotImplementedException();
            }
        }

      Tec tecdb = new Tec();
      Modes modesdb = new Modes();
      Task taskdb = new Task();

         public void SelectItemCheckBox()
          {
             //string expressoin = "Task";    
          }

         private void TaskCheckedListBox_Click(object sender, System.EventArgs e)
         {
 
         }

         private void TaskCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
         {
             for (int i = 0; i < taskdb.TaskCheckedListBox.Items.Count; i++)
             {
                 if ((bool)taskdb.TaskCheckedListBox.CheckedItems[i])
                 {
                     if ((bool)taskdb.TaskCheckedListBox.Items[i] == (bool)arraySourceDataTask.Rows[i][@"NameSourceTask"])
                     {

                         taskdb.TaskDataGridView.DataSource = arraySourceDataTask;
                     }
                 }
             }
         }  

        /* public void OnEvtDataRecievedHost(EventArgsDataHost ev)
         {
             switch (ev.id)
             {
                 case (int)ID_ASKED_DATAHOST.CONN_SETT:
                     //Установить соедиение                    
                     m_DataSource = new HDataSource((ConnectionSettings)ev.par[0]);
                     //Запустить поток
                     m_DataSource.StartDbInterfaces();
                     m_DataSource.Start();
                     break;
                 default:
                     break;
             }
         }

         private void onEvtQueryAskedData(object ev)
         {
             switch (((EventArgsDataHost)ev).id)
             {
                 case (int)PanelStatisticDiagnostic1.ID_ASKED_DATAHOST.CONN_SETT:
                     int iListenerId = DbSources.Sources().Register(m_connSett, false, m_connSett.name)
                         , id = Int32.Parse(m_tableSourceData.Select(@"NAME_SHR = '" + ((PanelStatisticDiagnostic1)((EventArgsDataHost)ev).par[0]).GetSelectedCheckBoxListTask() + @"'")[0][@"ID"].ToString())
                         , err = -1;
                     DataRow rowConnSett = ConnectionSettingsSource.GetConnectionSettings(TYPE_DATABASE_CFG.CFG_200, iListenerId, id, 501, out err).Rows[0];
                     ConnectionSettings connSett = new ConnectionSettings(rowConnSett, -1);
                     ((PanelStatisticDiagnostic1)((EventArgsDataHost)ev).par[0]).OnEvtDataRecievedHost(new EventArgsDataHost(((EventArgsDataHost)ev).id, new object[] { connSett }));
                     DbSources.Sources().UnRegister(iListenerId);
                     break;
                 default:
                     break;
             }
         }*/


         public void RunTimer()
         {
             System.Threading.TimerCallback TimerDelegate = new System.Threading.TimerCallback(tm);
             System.Threading.Timer TimerItem = new System.Threading.Timer(TimerDelegate, null, 2000, 2000);
         }

         private void tm(object state)
         {
             //PingSourceData();
         }

         public void PingSourceData(string adress)
         {
             Ping pingsender = new Ping();      
             PingReply reply = pingsender.Send(adress);
         }

         private void gridLinkCells(DataGridViewCellEventArgs e)
         {
             tecdb.TECDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 5.ToString("");
             modesdb.ModesDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 5.ToString("");
         }

        public void AddSourceDataTask(string desc)
          {
              taskdb.TaskCheckedListBox.Items.Add(desc);
          }
      
         public string GetSelectedCheckBoxListTask()
          {
              return taskdb.TaskCheckedListBox.SelectedItem.ToString();
          }

         //Функция потока
        /*public void ThreadFunction()
         {
             AddSourceDataTEC();
             MessageBox.Show("Reload");
             Thread.Sleep(60000);//для наглядности
        }*/
  
         public class Tec : HPanelCommon
          {
              public DataGridView TECDataGridView = new DataGridView();

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
                  TECDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                  TECDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                  TECDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                  TECDataGridView.AllowUserToAddRows = false; 
                  /*TECDataGridView.ColumnCount = 5;
                  TECDataGridView.Columns[0].Name = "ТЭЦ";
                  TECDataGridView.Columns[1].Name = "Источник данных";
                  TECDataGridView.Columns[2].Name = "Связь";
                  TECDataGridView.Columns[3].HeaderText = "Крайнее значение";
                  TECDataGridView.Columns[4].HeaderText = "Время проверки";*/
                  TECDataGridView.Dock = DockStyle.Fill;
                  //TECDataGridView.Location = new System.Drawing.Point(0, 0);
                  TECDataGridView.RowHeadersVisible = false;
                  TECDataGridView.Name = "TECDataGridView";
                  TECDataGridView.TabIndex = 0;
        

              }

              #endregion
          }

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

              private void InitializeComponentModes()
              {
                  ModesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                  ModesDataGridView.ColumnCount = 4;
                  ModesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                  ModesDataGridView.Columns[0].HeaderText = "Модес";
                  ModesDataGridView.Columns[1].HeaderText = "Связь";
                  ModesDataGridView.Columns[2].HeaderText = "Крайнее значение";
                  ModesDataGridView.Columns[3].HeaderText = "Время проверки";
                  ModesDataGridView.Dock = DockStyle.Fill;
                  ModesDataGridView.RowHeadersVisible = false;
                  //ModesDataGridView.Location = new System.Drawing.Point(0, 0);
                  ModesDataGridView.Name = "ModesDataGridView";
                  ModesDataGridView.TabIndex = 0;
              }
              #endregion
          }

         public class Task : HPanelCommon
          {
              public DataGridView TaskDataGridView = new DataGridView();
              public CheckedListBox TaskCheckedListBox = new CheckedListBox();
              public TableLayoutPanel TaskTableLayoutPanel = new TableLayoutPanel();

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

                  TaskTableLayoutPanel.SuspendLayout();      
                  TaskCheckedListBox.Dock = DockStyle.Fill;
                  TaskCheckedListBox.FormattingEnabled = true;
                  //TaskCheckedListBox.Location = new System.Drawing.Point(12, 12);
                  TaskCheckedListBox.Name = "TaskChekedListBox";
                  TaskCheckedListBox.CheckOnClick = true;
                  TaskCheckedListBox.TabIndex = 0;
                  TaskDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                  TaskDataGridView.Dock = DockStyle.Fill;
                  //TaskDataGridView.Location = new System.Drawing.Point(0, 0);
                  TaskDataGridView.Name = "TaskDataGridView";
                  TaskDataGridView.ColumnCount = 2;
                  TaskDataGridView.Columns[0].Name = "Задача";
                  TaskDataGridView.Columns[1].HeaderText = "Среднее время выполнения";
                  TaskDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                  TaskDataGridView.RowHeadersVisible = false;
                  TaskDataGridView.TabIndex = 0;
                  TaskTableLayoutPanel.ResumeLayout();
                 
              }
              #endregion;
          }

      protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly (cols, rows);
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
          SuspendLayout();


          TaskTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
          TECTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
          MODESTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
          Tasklabel = new System.Windows.Forms.Label();
          TEClabel = new System.Windows.Forms.Label();
          Modeslabel = new System.Windows.Forms.Label();

          //this.Controls.Add(TEClabel, 0, 0);
          this.Controls.Add(TECTableLayoutPanel, 0, 0);
          //this.Controls.Add(Modeslabel, 0, 2);
          this.Controls.Add(MODESTableLayoutPanel, 0, 1);
          //this.Controls.Add(Tasklabel, 0, 4);
          this.Controls.Add(TaskTableLayoutPanel, 0, 2);

          this.TaskTableLayoutPanel.RowCount = 1;
          this.TaskTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
          this.TaskTableLayoutPanel.ColumnCount = 2;
          this.TaskTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.74641F));
          this.TaskTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 83.25359F));
          this.TaskTableLayoutPanel.Dock = DockStyle.Fill;
          //TaskTableLayoutPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
          this.TaskTableLayoutPanel.Name = "TaskTableLayoutPanel";
          this.taskdb.TaskDataGridView.Dock = DockStyle.Fill;
          this.taskdb.TaskCheckedListBox.Dock = DockStyle.Fill;
          this.TaskTableLayoutPanel.Controls.Add(taskdb.TaskCheckedListBox, 0, 0);
          this.TaskTableLayoutPanel.Controls.Add(taskdb.TaskDataGridView, 1, 0);

          //TopLeftHeaderCell.Value 

          TECTableLayoutPanel.Dock = DockStyle.Fill;
          TECTableLayoutPanel.ColumnCount = 3;
          TECTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
          TECTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
          TECTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
          TECTableLayoutPanel.RowCount = 2;
          TECTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
          TECTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));

          Modeslabel.AutoSize = true;
          //Modeslabel.Dock = System.Windows.Forms.DockStyle.Fill;
          Modeslabel.Name = "Modeslabel";
          Modeslabel.TabIndex = 1;
          Modeslabel.Text = "ModesList";
          Modeslabel.AutoSize = true;

          //TEClabel.Dock = System.Windows.Forms.DockStyle.Fill;
          TEClabel.Name = "TEClabel";
          TEClabel.TabIndex = 1;

          TEClabel.Text = "TECList";
          TEClabel.AutoSize = true;

          Tasklabel.AutoSize = true;
          //Tasklabel.Dock = System.Windows.Forms.DockStyle.Fill;
          Tasklabel.Name = "Tasklabel";
          Tasklabel.TabIndex = 1;
          Tasklabel.Text = "TaskList";


          MODESTableLayoutPanel.Dock = DockStyle.Fill;
          MODESTableLayoutPanel.Controls.Add(modesdb.ModesDataGridView, 0, 0);
          MODESTableLayoutPanel.ColumnCount = 3;
          MODESTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
          MODESTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
          MODESTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
          MODESTableLayoutPanel.RowCount = 2;
          MODESTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
          MODESTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));

          this.Dock = DockStyle.Fill;
          Start();
          GetCurrentData();

          initializeLayoutStyle(1, 3);
          
          TECTableLayoutPanel.Controls.Add(m_arPanelsTEC[0].TECDataGridView, 0, 0);
        
          int i = -1;
          int indx = -1
             , col = -1
             , row = -1;

          for (i = 0; i < m_arPanelsTEC.Length; i++)
          {
              indx = i;
              //if (! (indx < this.RowCount))
              indx += (int)(indx / TECTableLayoutPanel.RowCount);
              //else ;

              col = (int)(indx / TECTableLayoutPanel.RowCount);
              row = indx % (TECTableLayoutPanel.RowCount - 0);
              //if (row == 0) row = 1; else ;
              TECTableLayoutPanel.Controls.Add(m_arPanelsTEC[i].TECDataGridView, col, row);
          }
          //onEvtQueryAskedData(i);

          TECTableLayoutPanel.Controls.Add(m_arPanelsTEC[5].TECDataGridView, 1, 0);
          
          AddSourceDataTEC();

          GetDataTask();
         
          ResumeLayout();
      }
        
      #endregion
      private System.Windows.Forms.TableLayoutPanel TaskTableLayoutPanel;
      private System.Windows.Forms.TableLayoutPanel TECTableLayoutPanel;
      private System.Windows.Forms.TableLayoutPanel MODESTableLayoutPanel;
      private System.Windows.Forms.Label Tasklabel;
      private System.Windows.Forms.Label TEClabel;
      private System.Windows.Forms.Label Modeslabel;

      public PanelStatisticDiagnostic1()
        {
            initialize();
        }

      public PanelStatisticDiagnostic1(IContainer container)
        {
            initialize();

            container.Add(this);
        }

      private void initialize()
      {
          InitializeComponent();
      }
        
      public void Start(ConnectionSettings connSett)
      {         
          m_connSett = connSett;
          m_DataSource = new HDataSource((ConnectionSettings)m_connSett);
          m_DataSource.StartDbInterfaces();        
          m_lockTimerGetData = new object();
      }
          
      public override void Start()
        {
            base.Start();

            Start (new ConnectionSettings()
            {
                id = -1
                ,name = @"DB_CONFIG"
                ,server = @"10.105.2.39"
                ,port = 1433
                ,dbName = @"techsite_cfg-2.X.X"
                ,userName = @"client1"
                ,password = @"client"
                ,ignore = false
            });          
        }

      public void GetCurrentData()
      {
          int iListenerId = DbSources.Sources().Register(m_connSett, false, m_connSett.name)
           , err = -1;
          //DbConnection dbConn = null;
         DbConnection dbConn = DbSources.Sources().GetConnection(iListenerId, out err);
         arraySource = DbTSQLInterface.Select(ref dbConn, @"SELECT [ТЭЦ] FROM Diagnostic WHERE [ТЭЦ] IS NOT NULL group by [ТЭЦ]", null, null, out err);
         arraySourceDataTask = DbTSQLInterface.Select(ref dbConn, @"SELECT Task,NameSourceTask, AverageTimeTask FROM InfoDiagnostic", null, null, out err);
         arraySourceServer = DbTSQLInterface.Select(ref dbConn, @"SELECT AdressSourceData FROM InfoDiagnostic", null, null, out err);
         
         rows = arraySource.Select();
         m_arPanelsTEC = new Tec[rows.Length];
         //m_arPanelsMODES = new Modes[rows.Length];

         for (int i = 0; i < rows.Length; i++)
          {
              //m_tableSourceData = DbTSQLInterface.Select(ref dbConn, @"SELECT [ТЭЦ], [Источник данных], [Крайнее значение], [Время], [Связь с источником данных] FROM Diagnostic where [ТЭЦ]= " + Convert.ToInt32(rows[i][0]), null, null, out err);                
              //dsTEC.Tables.Add( m_tableSourceData);
              m_arPanelsTEC[i] = new Tec();
              //m_arPanelsMODES[i] = new Modes();
          }
          //throw new Exception(@"Нет соединения с БД");
          DbSources.Sources().UnRegister(iListenerId);
      }

     public void AddSourceDataTEC()
      {
          int iListenerId = DbSources.Sources().Register(m_connSett, false, m_connSett.name)
         , err = -1;

          DbConnection dbConn = null;
          dbConn = DbSources.Sources().GetConnection(iListenerId, out err);

          for (int i = 0; i < m_arPanelsTEC.Length; i++)
          {
              m_tableSourceData = DbTSQLInterface.Select(ref dbConn, @"SELECT [ТЭЦ], [Источник данных], [Крайнее значение], [Время], [Связь с источником данных] FROM Diagnostic where [ТЭЦ]= " + Convert.ToInt32(rows[i][0]), null, null, out err);
              dsTEC.Tables.Add(m_tableSourceData);
              m_arPanelsTEC[i].TECDataGridView.DataSource = dsTEC.Tables[i];
          }
      }

      public void GetDataTask()
     {
         int iListenerId = DbSources.Sources().Register(m_connSett, false, m_connSett.name)
             , err = -1;

         DbConnection dbConn = null;
         dbConn = DbSources.Sources().GetConnection(iListenerId, out err);

         m_CheckList = DbTSQLInterface.Select(ref dbConn, @"SELECT NAME_SHR FROM source ", null, null, out err);

         for (int j = 0; j < m_CheckList.Rows.Count; j++)
         {
             AddSourceDataTask(m_CheckList.Rows[j][@"NAME_SHR"].ToString());
         }
     }

      public override void Stop()
        {
            Activate(false);
        }
   }
}



