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
        DataTable arraySourceServer = new DataTable();
        HDataSource m_DataSource;
        private object m_lockTimerGetData;
        ConnectionSettings m_connSett;
        public DataRow[] rows;
        public Tec[] m_arPanelsTEC;
        public DataTable arrayTEC;
        DataSet dsTEC = new DataSet();
        public DataRow[] rowsTec;
        //private System.Threading.Timer m_timer;
        

        public enum ID_ASKED_DATAHOST
        {
            CONN_SETT
                ,} 
        
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
                //register(0, 0, m_connSett, m_connSett.name);
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
                        Parent.Response(state,table as DataTable);
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

         /*private void TaskCheckedListBox_Click(object sender, System.EventArgs e)
         {
 
         }*/

         public void TaskCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
         {
             //MessageBox.Show("You are in the CheckedListBox.Click event.");

             PanelStatisticDiagnostic1 panelDiag = new PanelStatisticDiagnostic1();

             string curItem = taskdb.TaskCheckedListBox.SelectedItem.ToString();
             DataRow[] task_row;
             task_row = panelDiag.arraySourceDataTask.Select(curItem);

             for (int i = 0; i < task_row.Length; i++)
             {
                 panelDiag.taskdb.TaskDataGridView.DataSource = task_row;
             }     
         }  

        public void RunTimer()
         {
             System.Threading.TimerCallback TimerDelegate = new System.Threading.TimerCallback(tm);
             System.Threading.Timer TimerItem = new System.Threading.Timer(TimerDelegate, null, 2000, 2000);
         }

         private void tm(object state)
         {
             //PingSourceData();
         }

         /*public void PingSourceData()
         {
             Ping pingsender = new Ping();
             for (int k = 0; k < arraySource.Rows.Count; k++)
             {
                 if (arraySourceServer.Rows[k][@"ID"] == dsTEC.Tables[k].Rows[k][@"ID_Source"])
                 {
                     PingReply reply = pingsender.Send((string)arraySourceServer.Rows[k][@"IP"]);
                     DataSet dataset = new DataSet();

                     m_arPanelsTEC[k].TECDataGridView.Rows[k].Cells["Link"].Value = reply.Status.ToString();
                 }
             }
         }  

         private void gridLinkCells()
         {
             for (int i = 0; i < tecdb.TECDataGridView.ColumnCount; i++)
             {
                 m_arPanelsTEC[i].TECDataGridView.Rows[i].Cells["Link"].Value = "";
             }
         }*/

         /// <summary>
         /// Фукнция заполнения чекбокса задачи
         /// </summary>
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
                  //this.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
                  //this.initializeLayoutStyle(2, 2);
                  this.TECTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();

                  this.SuspendLayout();

                  this.TECTableLayoutPanel.Dock = DockStyle.Fill;
                  this.TECTableLayoutPanel.Controls.Add(this.LabelTec, 0, 0);
                  this.TECTableLayoutPanel.Controls.Add(this.TECDataGridView, 0, 1);
                  this.TECTableLayoutPanel.ColumnCount =1;
                  this.TECTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
                  this.TECTableLayoutPanel.RowCount = 2;
                  this.TECTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
                  this.TECTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));

                  this.TECDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                  this.TECDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                  this.TECDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                  this.TECDataGridView.AllowUserToAddRows = false;
                  this.TECDataGridView.Dock = DockStyle.Fill;
                  this.TECDataGridView.RowHeadersVisible = false;
                  this.TECDataGridView.Name = "TECDataGridView";
                  this.TECDataGridView.TabIndex = 0;

                  this.LabelTec.AutoSize = true;
                  this.LabelTec.Size = new System.Drawing.Size(5, 13);
                  //this.LabelTec.Dock = System.Windows.Forms.DockStyle.Fill;
                  this.LabelTec.Name = "LabelTec";
                  this.LabelTec.TabIndex = 1;
                  this.LabelTec.Text = "TEC - 1";
                  //this.LabelTec.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                  this.LabelTec.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

                  this.ResumeLayout(false);
              }

              #endregion
              public System.Windows.Forms.TableLayoutPanel TECTableLayoutPanel;
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
                  //this.initializeLayoutStyle(2, 2);
                  
                  this.MODESTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();

                  this.SuspendLayout();

                  this.MODESTableLayoutPanel.Dock = DockStyle.Fill;
                  this.MODESTableLayoutPanel.Controls.Add(LabelModes, 0, 0);
                  this.MODESTableLayoutPanel.Controls.Add(ModesDataGridView, 0, 1);
                  this.MODESTableLayoutPanel.ColumnCount = 1;
                  this.MODESTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));            
                  this.MODESTableLayoutPanel.RowCount = 2;
                  this.MODESTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
                  this.MODESTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));

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
                  this.TaskTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.74641F));
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
          
          Tasklabel = new System.Windows.Forms.Label();
          TEClabel = new System.Windows.Forms.Label();
          Modeslabel = new System.Windows.Forms.Label();
          
          //TopLeftHeaderCell.Value 

          this.MainTecTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
          this.MainTecTableLayoutPanel.ColumnCount = 3;
          this.MainTecTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
          this.MainTecTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
          this.MainTecTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
          this.MainTecTableLayoutPanel.RowCount = 2;
          this.MainTecTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
          this.MainTecTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
          taskdb.TaskCheckedListBox.SelectedIndexChanged += new EventHandler(TaskCheckedListBox_SelectedIndexChanged);
          this.MainModesTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
          this.MainModesTableLayoutPanel.ColumnCount = 3;
          this.MainModesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
          this.MainModesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
          this.MainModesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
          this.MainModesTableLayoutPanel.RowCount = 2;
          this.MainModesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
          this.MainModesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));

          this.Controls.Add(TEClabel, 0, 0);
          this.Controls.Add(Modeslabel, 0, 2);
          this.Controls.Add(MainModesTableLayoutPanel, 0, 3);
          this.Controls.Add(MainTecTableLayoutPanel, 0, 1);
          this.Controls.Add(Tasklabel, 0, 4);
          this.Controls.Add(taskdb.TaskTableLayoutPanel, 0, 5);

          this.Modeslabel.AutoSize = true;
          this.Modeslabel.Dock = System.Windows.Forms.DockStyle.Fill;
          this.Modeslabel.Name = "Modeslabel";
          this.Modeslabel.TabIndex = 0;
          this.Modeslabel.Text = "Modes_List";
          this.Modeslabel.Size = new System.Drawing.Size(10, 10);
          this.Modeslabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)))); 

          //this.TEClabel.Dock = System.Windows.Forms.DockStyle.Fill;
          this.TEClabel.Name = "TEClabel";
          this.TEClabel.TabIndex = 1;
          this.TEClabel.Text = "TEC_List";
          this.TEClabel.Size = new System.Drawing.Size(15, 10);
          this.TEClabel.AutoSize = true;
          this.TEClabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top| System.Windows.Forms.AnchorStyles.Left)))); 

          this.Tasklabel.Size = new System.Drawing.Size(10, 10);
          this.Tasklabel.Dock = System.Windows.Forms.DockStyle.Fill;
          this.Tasklabel.Name = "Tasklabel";
          this.Tasklabel.TabIndex = 2;
          this.Tasklabel.AutoSize = true;
          this.Tasklabel.Text = "Список задач";
          this.Modeslabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)))); 

          Dock = DockStyle.Fill;
          //initializeLayoutStyle(1, 3);
          this.ColumnCount = 1;
          this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));         
          this.RowCount = 6;
          this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 3F));
          this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
          this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 3F));
          this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
          this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 3F));
          this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
          
         GetCurrentData();
         Create_arPanelTEC();
         GetDataTask();

          this.MainTecTableLayoutPanel.Controls.Add(m_arPanelsTEC[0], 0, 0);
          int i = -1;
          int indx = -1
             , col = -1
             , row = -1;

          for (i = 0; i < m_arPanelsTEC.Length; i++)
          {
              indx = i;
              //if (! (indx < this.RowCount))
              indx += (int)(indx / MainTecTableLayoutPanel.RowCount);
              //else ;

              col = (int)(indx / MainTecTableLayoutPanel.RowCount);
              row = indx % (MainTecTableLayoutPanel.RowCount - 0);
              //if (row == 0) row = 1; else ;
             MainTecTableLayoutPanel.Controls.Add(m_arPanelsTEC[i], col, row);
          }
        
          this.MainTecTableLayoutPanel.Controls.Add(m_arPanelsTEC[5], 1, 0);

          ResumeLayout();
      }
        
      #endregion

      private System.Windows.Forms.Label Tasklabel;
      private System.Windows.Forms.Label TEClabel;
      private System.Windows.Forms.Label Modeslabel;
      private System.Windows.Forms.TableLayoutPanel MainTecTableLayoutPanel;
      private System.Windows.Forms.TableLayoutPanel MainModesTableLayoutPanel;

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
          m_DataSource.Start();
          m_DataSource.Command();         
          m_lockTimerGetData = new object();
      }
          
      public override void Start()
        {
            base.Start();

            Start (new ConnectionSettings()
            {
                id = -1
                ,name = @"DB_CONFIG"
                ,server = @"10.105.1.107"
                ,port = 1433
                ,dbName = @"techsite-2.X.X"
                ,userName = @"client1"
                ,password = @"client"
                ,ignore = false
            });          
        }
      /// <summary>
      /// Функция взятия инфрации из конф.БД
      /// </summary>
      public void GetCurrentData()
      {
          string connstr = "Data Source=ITC288; Initial Catalog=techsite_cfg-2.X.X;User ID=client1;Pwd=client; ";
          SqlConnection conn;         
          conn = new SqlConnection(connstr);
          SqlDataAdapter adapterTEC;
          DataSet dstec = new DataSet();
          SqlDataAdapter adapterTASK;
          //SqlDataAdapter adapterServer;
         
          try
          {
              conn.Open();
              adapterTEC = new SqlDataAdapter("SELECT * FROM TEC_LIST", conn);
              adapterTASK = new SqlDataAdapter("SELECT * FROM TASK_LIST", conn);
              //adapterServer = new SqlDataAdapter("SELECT * FROM Source", conn);

              //DataSet dataSet = new DataSet();
              // Заполнение таблицы
              //DataTable dttec = new DataTable();
              adapterTEC.Fill(arraySource);
              adapterTASK.Fill(arraySourceDataTask);
              //adapterServer.Fill(arraySourceServer);
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

    /*public void Create_modesPanel()
      {
          rows = arraySource.Select();

          for (int i = 0; i < rows.Length; i++)
          {  
              m_arPanelsMODES[i] = new Modes();
          }
      }*/

      /// <summary>
      /// Создание панелей ТЭЦ
      /// </summary>
    public void Create_arPanelTEC()
      {
          rows = arraySource.Select();
          m_arPanelsTEC = new Tec[rows.Length];

          for (int i = 0; i < rows.Length; i++)
          {
              m_arPanelsTEC[i] = new Tec();
          }
      }

    public delegate void MyDelegate(DataTable dt);

    public void Response(int state, DataTable table)
    {
        this.BeginInvoke(new MyDelegate(AddSourceDataTEC), table);
    }
    /// <summary>
    /// Заполнения панелей ТЭЦ
    /// </summary>
    public void AddSourceDataTEC(DataTable table)
       {
          for (int i = 0; i < m_arPanelsTEC.Length; i++)
          {
              string filter = String.Format("ID_EXT = " + Convert.ToInt32(rows[i][0]));

              foreach (DataRow rowsTec in table.Select(filter))
              {
                  arrayTEC.ImportRow(rowsTec);
              }

              dsTEC.Tables.Add(arrayTEC);
              m_arPanelsTEC[i].TECDataGridView.DataSource = dsTEC.Tables;
          }
      }
    /// <summary>
    /// Заполнение чекбокса задачами
    /// </summary>
    public void GetDataTask()
     {
         for (int j = 0; j < arraySourceDataTask.Rows.Count; j++)
         {
             AddSourceDataTask(arraySourceDataTask.Rows[j][@"NAME_SHR"].ToString());
         }
     }

    public override void Stop()
        {
            Activate(false);
        }

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



