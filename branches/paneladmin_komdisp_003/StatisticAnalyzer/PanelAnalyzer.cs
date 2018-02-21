﻿//using HClassLibrary;
using ASUTP;
using ASUTP.Core;
using ASUTP.Database;
using StatisticCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common; //DbConnection
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace StatisticAnalyzer
{
    public abstract partial class PanelAnalyzer : PanelStatistic
    {
        protected enum StatesMachine {
            Unknown = -1
            , ServerTime
            , ProcCheckedState
            , ProcCheckedFilter
            , ListMessageToUserByDate
            , ListDateByUser
            , CounterToTypeMessageByDate
            , TODO
        }

        /// <summary>
        /// Интерфейс для взаимодействия с объектом чтения сообщений журнала
        /// </summary>
        protected interface ILoggingReadHandler : ASUTP.Helper.IHHandler
        {
            /// <summary>
            /// Запросить асинхронное выплнение команды
            /// </summary>
            /// <param name="state">Состояние(идентификатор), определяющее команду</param>
            /// <param name="args">Аргументы для выполнения команды</param>
            /// <param name="bNewState">Признак принудительного прекращения всех уже выполняющихся команд</param>
            void Command (StatesMachine state, object args, bool bNewState);

            /// <summary>
            /// Событие завершения выполнения команды-заппроса
            /// </summary>
            event Action<REQUEST, DataTable> EventCommandCompleted;

            /// <summary>
            /// Инициировать событие из-вне "выполнение команды-заппроса завершено"
            /// </summary>
            /// <param name="req">Объект выполненной команды-запроса</param>
            /// <param name="tableRes">Результат запроса</param>
            void PerformCommandCompleted(REQUEST req, DataTable tableRes);
        }

        /// <summary>
        /// Класс для формирования запроса к журналу с сообщениями
        /// </summary>
        protected class REQUEST
        {
            public enum STATE {
                Unknown, Ready, Ok, Error
            }

            public StatesMachine Key;

            public object [] Args;

            public STATE State;

            private REQUEST ()
            {
                Key = StatesMachine.Unknown;
            }

            public REQUEST (StatesMachine key, object arg)
            {
                Key = key;

                // копировать аргументы
                if (Equals (arg, null) == false)
                    if (arg is Array) {
                        Args = new object [(arg as object []).Length];

                        for (int i = 0; i < Args.Length; i++) {
                            if (((arg as object []) [i] is string)
                                && (!(((string)(arg as object []) [i]).IndexOf (s_chDelimeters [(int)INDEX_DELIMETER.PART]) < 0))) {
                                Args [i] = ((string)(arg as object []) [i]).Split (s_chDelimeters, StringSplitOptions.RemoveEmptyEntries);
                            } else {
                                Args [i] = (arg as object []) [i];
                            }
                        }
                    } else
                        Args = new object [] { arg };
                else
                    Args = new object [] { };

                State = STATE.Ready;

                Logging.Logg ().Debug ($"PanelAnalyzer.HLoggingReadHandlerDb.REQUEST::ctor (Key={Key}) - новое; <args>:{toString ()}...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            /// <summary>
            /// Признак наличия аргументов команды-запроса
            /// , отсутствие показывает, что был вызван конструктор по умолчанию
            /// </summary>
            public bool IsEmpty
            {
                get
                {
                    return Equals (Args, null); //== true ? true : Args.Length == 0;
                }
            }

            private string getTypeMessages ()
            {
                string strRes = string.Empty;

                string [] args;

                if ((Key == StatesMachine.ListMessageToUserByDate)
                    && (Args [1] is string [])) {
                    args = Args [1] as string [];

                    strRes = $@"[{args[0]}:<{string.Join(",", args, 1, args.Length - 1)}>]";
                } else
                //TODO: некорректный вызов
                    ;

                return strRes;
            }

            private string toString ()
            {
                string strRes = string.Empty;

                switch (Key) {
                    case StatesMachine.ProcCheckedFilter:
                    case StatesMachine.ProcCheckedState:
                        strRes = $"не требуется";
                        break;
                    case StatesMachine.ListMessageToUserByDate:
                        strRes = $"IdUser={Args [0]}, Type={getTypeMessages()}, Period=[{(DateTime)Args [2]}, {(DateTime)Args [3]}]";
                        break;
                    case StatesMachine.ListDateByUser:
                        strRes = $"IdUser={Args [0]}";
                        break;
                    case StatesMachine.CounterToTypeMessageByDate:
                        strRes = $"Tag={((DATAGRIDVIEW_LOGCOUNTER)Args [0]).ToString ()}, Period=[{(DateTime)Args [1]}, {(DateTime)Args [2]}], Users={Args [3]}";
                        break;
                    default:
                        break;
                }

                return strRes;
            }
        }

        private Dictionary<StatesMachine, Action<REQUEST, DataTable>> _handlers;

        private void loggingReadHandler_onCommandCompleted (REQUEST req, DataTable tableRes)
        {
            switch (req.Key) {
                case StatesMachine.ProcCheckedState:
                case StatesMachine.ProcCheckedFilter:
                case StatesMachine.ListMessageToUserByDate:
                case StatesMachine.ListDateByUser:
                case StatesMachine.CounterToTypeMessageByDate:
                    _handlers [req.Key] (req, tableRes);
                    break;
                default:
                    throw new InvalidOperationException ("PanelAnalyzer::loggingReadHandler_onCommandCompleted () - неизвестный тип запроса...");
                    break;
            }
        }

        protected virtual void handlerCommandCounterToTypeMessageByDate(PanelAnalyzer.REQUEST req, DataTable tableRes)
        {
            m_dictDataGridViewLogCounter[(DATAGRIDVIEW_LOGCOUNTER)req.Args[0]].Fill(tableRes);

            delegateReportClear?.Invoke(true);
        }

        protected virtual void handlerCommandListMessageToUserByDate(PanelAnalyzer.REQUEST req, DataTable tableLogging)
        {
            if (req.State == PanelAnalyzer.REQUEST.STATE.Ok) {
                m_LogParse.Start(new LogParse.PARAM_THREAD_PROC { Mode = LogParse.MODE.MESSAGE, Delimeter = string.Empty, Table = tableLogging });
            } else
                ;
        }

        protected virtual void handlerCommandListDateByUser(PanelAnalyzer.REQUEST req, DataTable tableRes)
        {
            m_LogParse.Start(new PanelAnalyzer.LogParse.PARAM_THREAD_PROC {
                Mode = LogParse.MODE.LIST_DATE
                ,
                Delimeter = s_chDelimeters[(int)INDEX_DELIMETER.PART] //??? зачем передавать статическую величину
                ,
                Table = tableRes
            });
        }

        protected virtual void handlerCommandProcChecked (PanelAnalyzer.REQUEST req, DataTable tableRes)
        {
            int i = -1;
            CheckState? modeDueTime = null; // System.Threading.Timeout.Infinite
            int [] ariTags;
            bool [] arbActives;
            IAsyncResult iar;

            if (req.State == PanelAnalyzer.REQUEST.STATE.Ok) {
                ariTags = new int [tableRes.Rows.Count];
                arbActives = new bool [tableRes.Rows.Count];

                for (i = 0;
                    (i < tableRes.Rows.Count)
                        && (m_bThreadTimerCheckedAllowed == true);
                    i++) {
                    ariTags [i] = int.Parse (tableRes.Rows [i] [@"ID_USER"].ToString ().Trim ());
                    //Проверка активности
                    arbActives[i] = (HDateTime.ToMoscowTimeZone(DateTime.Now) - DateTime.Parse(tableRes.Rows[i][@"MAX_DATETIME_WR"].ToString())).TotalSeconds < 66;
                }

                if (Equals (arbActives, null) == false) {
                    if (req.Key == StatesMachine.ProcCheckedState)
                        if (InvokeRequired == true) {
                            iar = BeginInvoke((Action<DataGridView, int[], bool[]>) delegate (DataGridView arg0, int[] arg1, bool[] arg2) {
                                arg0.Update(arg1, arg2);
                            }
                            , dgvUserView, ariTags, arbActives);

                            WaitHandle.WaitAny(new WaitHandle[] { iar.AsyncWaitHandle });
                            EndInvoke(iar);
                        } else
                            dgvUserView.Update(ariTags, arbActives);
                    else if (req.Key == StatesMachine.ProcCheckedFilter) {
                        if (InvokeRequired == true) {
                            iar = BeginInvoke((Action<int[], bool[]>)delegate (int[] arg1, bool[]arg2)
                            {
                                applyFilterView(arg1, arg2);
                            }
                            , ariTags, arbActives);

                            WaitHandle.WaitAny(new WaitHandle[] { iar.AsyncWaitHandle });
                            EndInvoke(iar);
                        } else
                            applyFilterView(ariTags, arbActives);
                    } else
                        ;

                    modeDueTime = CheckState.Unchecked; // MSEC_TIMERCHECKED_STANDARD;
                } else
                //Нет соединения с БД...
                    modeDueTime = CheckState.Indeterminate; // MSEC_TIMERCHECKED_FORCE;
            } else
            //Ошибка при выборке данных...
                modeDueTime = CheckState.Indeterminate;

            activateTimerChecked(modeDueTime);

            delegateReportClear (true);
        }

        #region Design

        private System.Windows.Forms.TabPage tabPageLogging;
        private System.Windows.Forms.TabPage tabPageTabes;
        private TableLayoutPanel panelTabPageTabes;
        private TableLayoutPanel panelTabPageLogging;

        private System.Windows.Forms.CheckBox[] arrayCheckBoxModeTECComponent;
        private System.Windows.Forms.ListBox listBoxTabVisible;

        private System.Windows.Forms.Button buttonUpdate;

        private System.Windows.Forms.DataGridView dgvFilterActiveView;
        private System.Windows.Forms.DataGridView dgvFilterRoleView;
        private System.Windows.Forms.DataGridView dgvListDateView;
        private DataGridView_LogMessageCounter dgvTypeToView;
        private System.Windows.Forms.DataGridView dgvMessageView;
        private System.Windows.Forms.DataGridView dgvUserView;
        // Групповая статистика
        private System.Windows.Forms.DateTimePicker StartCalendar;
        private System.Windows.Forms.DateTimePicker EndCalendar;
        private System.Windows.Forms.DataGridView dgvFilterRoleStatistic;
        private DataGridView_LogMessageCounter dgvTypeToStatistic;
        private System.Windows.Forms.DataGridView dgvUserStatistic;

        //protected abstract DataGridView_LogMessageCounter create_LogMessageCounter(DataGridView_LogMessageCounter.TYPE type);

        private void InitializeComponent()
        {
            this.dgvTypeToStatistic = new DataGridView_LogMessageCounter (DataGridView_LogMessageCounter.TYPE.WITHOUT_CHECKBOX);
            System.Windows.Forms.GroupBox groupUser = new System.Windows.Forms.GroupBox();
            System.Windows.Forms.GroupBox groupMessage = new System.Windows.Forms.GroupBox();
            System.Windows.Forms.GroupBox groupTabs = new System.Windows.Forms.GroupBox();
            TableLayoutPanel  panelUser = new TableLayoutPanel();
            TableLayoutPanel  panelMessage = new TableLayoutPanel();
            TableLayoutPanel  panelTabs = new TableLayoutPanel();
            System.Windows.Forms.Label labelMessage = new Label();
            System.Windows.Forms.Label labelStartCalendar = new Label();
            System.Windows.Forms.Label labelStopCalendar = new Label();
            System.Windows.Forms.Label labelRole = new Label();
            System.Windows.Forms.Label labelUser = new Label();
            this.StartCalendar = new DateTimePicker();
            this.EndCalendar = new DateTimePicker();
            this.dgvFilterRoleStatistic = new DataGridView();
            this.dgvUserStatistic = new DataGridView();

            System.Windows.Forms.Label labelFilterActives = new System.Windows.Forms.Label();
            this.dgvFilterActiveView = new System.Windows.Forms.DataGridView();

            System.Windows.Forms.Label labelFilterRoles = new System.Windows.Forms.Label();
            this.dgvFilterRoleView = new System.Windows.Forms.DataGridView();

            this.dgvUserView = new System.Windows.Forms.DataGridView();

            this.tabPageLogging = new System.Windows.Forms.TabPage();
            this.panelTabPageLogging = new TableLayoutPanel ();
            this.dgvMessageView = new System.Windows.Forms.DataGridView();
            this.tabPageTabes = new System.Windows.Forms.TabPage();
            this.panelTabPageTabes = new TableLayoutPanel();
            this.listBoxTabVisible = new System.Windows.Forms.ListBox();
            this.arrayCheckBoxModeTECComponent=new CheckBox[(int)FormChangeMode.MODE_TECCOMPONENT.ANY +1];
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TEC] = new CheckBox();
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TG] = new CheckBox();
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.GTP] = new CheckBox();
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.PC] = new CheckBox();
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.ANY] = new CheckBox();

            System.Windows.Forms.Label labelDatetimeStart = new System.Windows.Forms.Label();
            this.dgvListDateView = new System.Windows.Forms.DataGridView();

            System.Windows.Forms.Label labelFilterTypeMessage = new System.Windows.Forms.Label();
            this.dgvTypeToView = new DataGridView_LogMessageCounter(DataGridView_LogMessageCounter.TYPE.WITH_CHECKBOX);

            this.buttonUpdate = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.dgvFilterActiveView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilterRoleView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUserView)).BeginInit();

            this.tabPageLogging.SuspendLayout();
            this.panelTabPageLogging.SuspendLayout ();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTypeToView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvListDateView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMessageView)).BeginInit();
            this.tabPageTabes.SuspendLayout();
            this.panelTabPageTabes.SuspendLayout ();
            //((System.ComponentModel.ISupportInitialize)(this.listTabVisible)).BeginInit();
            this.SuspendLayout();

            //this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            initializeLayoutStyle (12, 24);
            // 
            // groupUser
            //
            groupUser.Dock = DockStyle.Fill;
            // 
            // groupMessage
            //
            groupMessage.Dock = DockStyle.Fill;
            // 
            // groupTabs
            //
            groupTabs.Dock = DockStyle.Fill;
            // 
            // labelFilterActives
            // 
            labelFilterActives.AutoSize = true;
            //this.labelFilterActives.Location = new System.Drawing.Point(9, 12);
            labelFilterActives.Name = "labelFilterActives";
            //this.labelFilterActives.Size = new System.Drawing.Size(111, 13);
            labelFilterActives.TabIndex = 4;
            labelFilterActives.Text = "Фильтр: активность";
            // 
            // dgvFilterActives
            // 
            this.dgvFilterActiveView.AllowUserToAddRows = false;
            this.dgvFilterActiveView.AllowUserToDeleteRows = false;
            this.dgvFilterActiveView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFilterActiveView.ColumnHeadersVisible = false;
            this.dgvFilterActiveView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                new DataGridViewCheckBoxColumn (),
                new DataGridViewTextBoxColumn ()});
            //this.dgvFilterActives.Location = new System.Drawing.Point(12, 28);
            this.dgvFilterActiveView.Dock = DockStyle.Fill;
            //this.dgvFilterActives.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top));
            this.dgvFilterActiveView.MultiSelect = false;
            this.dgvFilterActiveView.Name = "dgvFilterActives";
            this.dgvFilterActiveView.ReadOnly = true;
            this.dgvFilterActiveView.RowHeadersVisible = false;
            this.dgvFilterActiveView.RowTemplate.Height = 18;
            this.dgvFilterActiveView.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFilterActiveView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //this.dgvFilterActives.Size = new System.Drawing.Size(190, 39);
            this.dgvFilterActiveView.TabIndex = 8;
            // 
            // dataGridViewActivesCheckBoxColumnUse
            // 
            int i = 0;
            this.dgvFilterActiveView.Columns[i].Frozen = true;
            this.dgvFilterActiveView.Columns[i].HeaderText = "Use";
            this.dgvFilterActiveView.Columns[i].Name = "dataGridViewActivesCheckBoxColumnUse";
            this.dgvFilterActiveView.Columns[i].ReadOnly = true;
            this.dgvFilterActiveView.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFilterActiveView.Columns[i].Width = 25;
            // 
            // dataGridViewActivesTextBoxColumnDesc
            // 
            i = 1;
            //this.dgvFilterActives.Columns[i].Frozen = true;
            this.dgvFilterActiveView.Columns[i].HeaderText = "Desc";
            this.dgvFilterActiveView.Columns[i].Name = "dataGridViewActivesTextBoxColumnDesc";
            this.dgvFilterActiveView.Columns[i].ReadOnly = true;
            this.dgvFilterActiveView.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFilterActiveView.Columns[i].Width = 165;
            this.dgvFilterActiveView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // 
            // labelFilterRoles
            // 
            labelFilterRoles.AutoSize = true;
            //this.labelFilterRoles.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top));
            //this.labelFilterRoles.Location = new System.Drawing.Point(9, 50);
            labelFilterRoles.Name = "labelFilterRoles";
            //this.labelFilterRoles.Size = new System.Drawing.Size(77, 13);
            labelFilterRoles.TabIndex = 5;
            labelFilterRoles.Text = "Фильтр: роли";
            // 
            // dgvFilterRoles
            // 
            this.dgvFilterRoleView.AllowUserToAddRows = false;
            this.dgvFilterRoleView.AllowUserToDeleteRows = false;
            this.dgvFilterRoleView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFilterRoleView.ColumnHeadersVisible = false;
            this.dgvFilterRoleView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                new DataGridViewCheckBoxColumn (),
                new DataGridViewTextBoxColumn ()});
            //this.dgvFilterRoles.Location = new System.Drawing.Point(12, 88);
            this.dgvFilterRoleView.Dock = DockStyle.Fill;
            this.dgvFilterRoleView.MultiSelect = false;
            this.dgvFilterRoleView.Name = "dgvFilterRoles";
            this.dgvFilterRoleView.ReadOnly = true;
            this.dgvFilterRoleView.RowHeadersVisible = false;
            this.dgvFilterRoleView.RowTemplate.Height = 18;
            this.dgvFilterRoleView.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFilterRoleView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //this.dgvFilterRoles.Size = new System.Drawing.Size(190, 111);
            this.dgvFilterRoleView.TabIndex = 9;
            this.dgvFilterRoleView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvFilterRoleView_CellClick);
            // 
            // dataGridViewRolesCheckBoxColumnUse
            // 
            i = 0;
            this.dgvFilterRoleView.Columns[i].Frozen = true;
            this.dgvFilterRoleView.Columns[i].HeaderText = "Use";
            this.dgvFilterRoleView.Columns[i].Name = "dataGridViewRolesCheckBoxColumnUse";
            this.dgvFilterRoleView.Columns[i].ReadOnly = true;
            this.dgvFilterRoleView.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFilterRoleView.Columns[i].Width = 25;
            // 
            // dataGridViewRolesTextBoxColumnDesc
            // 
            i = 1;
            //this.dgvFilterRoles.Columns[i].Frozen = true;
            this.dgvFilterRoleView.Columns[i].HeaderText = "Desc";
            this.dgvFilterRoleView.Columns[i].Name = "dataGridViewRolesTextBoxColumnDesc";
            this.dgvFilterRoleView.Columns[i].ReadOnly = true;
            this.dgvFilterRoleView.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFilterRoleView.Columns[i].Width = 145;
            this.dgvFilterRoleView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            // 
            // dgvClient
            // 
            this.dgvUserView.AllowUserToAddRows = false;
            this.dgvUserView.AllowUserToDeleteRows = false;
            this.dgvUserView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUserView.ColumnHeadersVisible = false;
            this.dgvUserView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                new DataGridViewCheckBoxColumn (),
                new DataGridViewTextBoxColumn ()});
            //this.dgvClient.Location = new System.Drawing.Point(12, 150);
            this.dgvUserView.Dock = DockStyle.Fill;
            this.dgvUserView.MultiSelect = false;
            this.dgvUserView.Name = "dgvClient";
            this.dgvUserView.ReadOnly = true;
            this.dgvUserView.RowHeadersVisible = false;
            this.dgvUserView.RowTemplate.Height = 18;
            this.dgvUserView.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvUserView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //this.dgvClient.Size = new System.Drawing.Size(190, 400);
            this.dgvUserView.TabIndex = 10;
            this.dgvUserView.SelectionChanged += new System.EventHandler(this.dgvUserView_SelectionChanged);
            // 
            // dataGridViewClientCheckBoxColumnActive
            // 
            i = 0;
            this.dgvUserView.Columns[i].Frozen = true;
            this.dgvUserView.Columns[i].HeaderText = "Active";
            this.dgvUserView.Columns[i].Name = "dataGridViewClientCheckBoxColumnActive";
            this.dgvUserView.Columns[i].ReadOnly = true;
            this.dgvUserView.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvUserView.Columns[i].Width = 25;
            // 
            // dataGridViewClientTextBoxColumnDesc
            // 
            i = 1;
            //this.dgvClient.Columns[i].Frozen = true;
            this.dgvUserView.Columns[i].HeaderText = "Desc";
            this.dgvUserView.Columns[i].Name = "dataGridViewClientTextBoxColumnDesc";
            this.dgvUserView.Columns[i].ReadOnly = true;
            this.dgvUserView.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvUserView.Columns[i].Width = 145;
            this.dgvUserView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // 
            // tabPageLogging
            // 
            //this.tabPageLogging.Location = new System.Drawing.Point(4, 22);
            this.tabPageLogging.Controls.Add (this.panelTabPageLogging);
            this.tabPageLogging.Name = "tabPageLogging";
            this.tabPageLogging.Padding = new System.Windows.Forms.Padding(3);
            //this.tabPageLogging.Size = new System.Drawing.Size(553, 298);
            this.tabPageLogging.TabIndex = 0;
            this.tabPageLogging.Text = "Лог-файл";
            this.tabPageLogging.ToolTipText = "Лог-файл пользователя";
            this.tabPageLogging.UseVisualStyleBackColor = true;
            // panelPageLogging
            //panelTabPageLogging.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            this.panelTabPageLogging.ColumnCount = 6; this.panelTabPageLogging.RowCount = 24;
            for (i = 0; i < this.panelTabPageLogging.ColumnCount; i++)
                this.panelTabPageLogging.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / this.panelTabPageLogging.ColumnCount));
            for (i = 0; i < this.panelTabPageLogging.RowCount; i++)
                this.panelTabPageLogging.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / this.panelTabPageLogging.RowCount));
            this.panelTabPageLogging.Dock = DockStyle.Fill;
            this.panelTabPageLogging.Controls.Add(labelDatetimeStart, 0, 0); this.SetColumnSpan(labelDatetimeStart, 2);
            this.panelTabPageLogging.Controls.Add(this.dgvListDateView, 0, 1); this.SetColumnSpan(this.dgvListDateView, 2); this.SetRowSpan(this.dgvListDateView, 11);
            this.panelTabPageLogging.Controls.Add(labelFilterTypeMessage, 0, 12); this.SetColumnSpan(labelFilterTypeMessage, 2);
            this.panelTabPageLogging.Controls.Add(this.dgvTypeToView, 0, 13); this.SetColumnSpan(this.dgvTypeToView, 2); this.SetRowSpan(this.dgvTypeToView, 11);
            this.panelTabPageLogging.Controls.Add(this.dgvMessageView, 2, 0); this.SetColumnSpan(this.dgvMessageView, 4); this.SetRowSpan(this.dgvMessageView, 24);

            // 
            // labelDatetimeStart
            // 
            labelDatetimeStart.AutoSize = true;
            //this.labelDatetimeStart.Location = new System.Drawing.Point(3, 7);
            labelDatetimeStart.Name = "labelDatetimeStart";
            //this.labelDatetimeStart.Size = new System.Drawing.Size(157, 13);
            labelDatetimeStart.TabIndex = 11;
            labelDatetimeStart.Text = "Фильтр: дата/время запуска";
            // 
            // dgvDatetimeStart
            // 
            this.dgvListDateView.Dock = DockStyle.Fill;
            this.dgvListDateView.AllowUserToAddRows = false;
            this.dgvListDateView.AllowUserToDeleteRows = false;
            this.dgvListDateView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvListDateView.ColumnHeadersVisible = false;
            this.dgvListDateView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                new DataGridViewCheckBoxColumn (),
                new DataGridViewTextBoxColumn ()});
            //this.dgvDatetimeStart.Location = new System.Drawing.Point(6, 23);
            this.dgvListDateView.MultiSelect = false;
            this.dgvListDateView.Name = "dgvDatetimeStart";
            this.dgvListDateView.ReadOnly = true;
            this.dgvListDateView.RowHeadersVisible = false;
            this.dgvListDateView.RowTemplate.Height = 18;
            this.dgvListDateView.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvListDateView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //this.dgvDatetimeStart.Size = new System.Drawing.Size(170, 164);
            this.dgvListDateView.BackColor = this.BackColor;
            this.dgvListDateView.TabIndex = 10;
            // 
            // dataGridViewCheckBoxColumnDatetimeStartUse
            // 
            i = 0;
            this.dgvListDateView.Columns[i].Frozen = true;
            this.dgvListDateView.Columns[i].HeaderText = "Use";
            this.dgvListDateView.Columns[i].Name = "dataGridViewCheckBoxColumn1";
            this.dgvListDateView.Columns[i].ReadOnly = true;
            this.dgvListDateView.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvListDateView.Columns[i].Width = 25;
            // 
            // dataGridViewTextBoxColumnDatetimeStartDesc
            // 
            i = 1;
            //this.dgvDatetimeStart.Columns[i].Frozen = true;
            this.dgvListDateView.Columns[i].HeaderText = "Desc";
            this.dgvListDateView.Columns[i].Name = "dataGridViewTextBoxColumnDatetimeStartDesc";
            this.dgvListDateView.Columns[i].ReadOnly = true;
            this.dgvListDateView.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvListDateView.Columns[i].Width = 145;
            this.dgvListDateView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            // 
            // labelFilterTypeMessage
            // 
            labelFilterTypeMessage.AutoSize = true;
            //this.labelFilterTypeMessage.Location = new System.Drawing.Point(3, 190);
            labelFilterTypeMessage.Name = "labelFilterTypeMessage";
            //this.labelFilterTypeMessage.Size = new System.Drawing.Size(130, 13);
            labelFilterTypeMessage.TabIndex = 13;
            labelFilterTypeMessage.Text = "Фильтр: тип сообщения";

            // 
            // dgvLogMessage
            //                 
            this.dgvMessageView.Dock = DockStyle.Fill;
            this.dgvMessageView.MultiSelect = false;
            this.dgvMessageView.Name = "dgvLogMessage";
            this.dgvMessageView.ReadOnly = true;
            this.dgvMessageView.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            //this.dgvLogMessage.Size = new System.Drawing.Size(371, 292);
            this.dgvMessageView.TabIndex = 15;
            this.dgvMessageView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            new System.Windows.Forms.DataGridViewTextBoxColumn ()
                , new System.Windows.Forms.DataGridViewTextBoxColumn ()
                , new System.Windows.Forms.DataGridViewTextBoxColumn ()
            });
            this.dgvMessageView.AllowUserToAddRows =
            this.dgvMessageView.AllowUserToDeleteRows =
                false;
            this.dgvMessageView.ColumnHeadersVisible = false;
            this.dgvMessageView.RowHeadersVisible = false;
            this.dgvMessageView.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvMessageView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMessageView.Columns[0].Width = 85; this.dgvMessageView.Columns[0].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvMessageView.Columns[1].Width = 30; this.dgvMessageView.Columns[1].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvMessageView.Columns[2].Width = 254; this.dgvMessageView.Columns[2].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvMessageView.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            // 
            // tabPageTabes
            // 
            this.tabPageTabes.Controls.Add(this.panelTabPageTabes);
            //this.tabPageTabes.Location = new System.Drawing.Point(4, 22);
            this.tabPageTabes.Name = "tabPageTabes";
            this.tabPageTabes.Padding = new System.Windows.Forms.Padding(3);
            //this.tabPageTabes.Size = new System.Drawing.Size(553, 298);
            this.tabPageTabes.TabIndex = 1;
            this.tabPageTabes.Text = "Вкладки";
            this.tabPageTabes.ToolTipText = "Отображаемые вкладки";
            this.tabPageTabes.UseVisualStyleBackColor = true;
            // panelTabPageTabes
            //panelTabPageTabes.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            this.panelTabPageTabes.Dock = DockStyle.Fill;
            this.panelTabPageTabes.ColumnCount = 6; this.panelTabPageTabes.RowCount = 24;
            for (i = 0; i < this.panelTabPageTabes.ColumnCount; i++)
                this.panelTabPageTabes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / this.panelTabPageTabes.ColumnCount));
            for (i = 0; i < this.panelTabPageTabes.RowCount; i++)
                this.panelTabPageTabes.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / this.panelTabPageTabes.RowCount));
            this.panelTabPageTabes.Controls.Add(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.ANY], 0, 0); this.SetRowSpan(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.ANY], 2);
            this.panelTabPageTabes.Controls.Add(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.PC], 1, 0); this.SetRowSpan(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.PC], 2);
            this.panelTabPageTabes.Controls.Add(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.GTP], 1, 2); this.SetRowSpan(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.GTP], 2);
            this.panelTabPageTabes.Controls.Add(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TG], 2, 0); this.SetRowSpan(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TG], 2);
            this.panelTabPageTabes.Controls.Add(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TEC], 2, 2); this.SetRowSpan(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TEC], 2);
            this.panelTabPageTabes.Controls.Add(this.listBoxTabVisible, 0, 4); this.SetColumnSpan(this.listBoxTabVisible, 6); this.SetRowSpan(this.listBoxTabVisible, 20);
            // 
            // listTabVisible
            // 
            this.listBoxTabVisible.Dock = DockStyle.Fill;
                
            //this.dgvTabVisible.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            //this.dgvTabVisible.Location = new System.Drawing.Point(6, 54);
            this.listBoxTabVisible.Name = "listTabVisible";
            //this.dgvTabVisible.Size = new System.Drawing.Size(547, 211);
            this.listBoxTabVisible.TabIndex = 15;
            // 
            // checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC]
            // 
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].AutoSize = true;
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Enabled = true;
            //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Location = new System.Drawing.Point(6, 6);
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Name = "checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC]";
            //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Size = new System.Drawing.Size(48, 17);
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].TabIndex = 16;
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Text = "ТЭЦ";
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].UseVisualStyleBackColor = true;
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Checked = true;
            // 
            // checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG]
            // 
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TG].AutoSize = true;
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Enabled = true;
            //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Location = new System.Drawing.Point(6, 29);
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Name = "checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG]";
            //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Size = new System.Drawing.Size(39, 17);
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TG].TabIndex = 17;
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Text = "ТГ";
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TG].UseVisualStyleBackColor = true;
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Checked = true;
            // 
            // checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP]
            // 
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].AutoSize = true;
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Enabled = true;
            //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Location = new System.Drawing.Point(112, 6);
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Name = "checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP]";
            //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Size = new System.Drawing.Size(47, 17);
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].TabIndex = 18;
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Text = "ГТП";
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].UseVisualStyleBackColor = true;
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Checked = true;
            // 
            // checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC]
            // 
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.PC].AutoSize = true;
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Enabled = true;
            //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Location = new System.Drawing.Point(112, 29);
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Name = "checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC]";
            //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Size = new System.Drawing.Size(44, 17);
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.PC].TabIndex = 19;
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Text = "ЩУ";
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.PC].UseVisualStyleBackColor = true;
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Checked = true;
            // 
            // checkBoxAdmin
            // 
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].AutoSize = true;
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].Enabled = true;
            //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].Location = new System.Drawing.Point(6, 275);
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].Name = "checkBoxAdmin";
            //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].Size = new System.Drawing.Size(48, 17);
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].TabIndex = 20;
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].Text = HAdmin.PBR_PREFIX;
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].UseVisualStyleBackColor = true;
            this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].Checked = true;

            // 
            // buttonUpdate
            // 
            //this.buttonUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUpdate.Dock = DockStyle.Fill;
            this.buttonUpdate.Location = new System.Drawing.Point(656, 351);
            this.buttonUpdate.Name = "buttonUpdate";
            //this.buttonUpdate.Size = new System.Drawing.Size(100, 26);
            this.buttonUpdate.TabIndex = 6;
            this.buttonUpdate.Text = "Обновить";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
                
            #region TypeMessage
            // 
            // labelMessage
            // 
            labelMessage.AutoSize = true;
            //this.labelFilterRoles.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top));
            //this.labelFilterRoles.Location = new System.Drawing.Point(9, 50);
            labelMessage.Name = "labelMessage";
            //this.labelFilterRoles.Size = new System.Drawing.Size(77, 13);
            //this.labelMessage.TabIndex = 5;
            labelMessage.Text = "Статистика сообщений";
                
            #endregion

            #region User

            // 
            // labelUser
            // 
            labelUser.AutoSize = true;
            //this.labelFilterRoles.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top));
            //this.labelFilterRoles.Location = new System.Drawing.Point(9, 50);
            labelUser.Name = "labelUser";
            //this.labelFilterRoles.Size = new System.Drawing.Size(77, 13);
            //this.labelMessage.TabIndex = 5;
            labelUser.Text = "Фильтр: пользователи";

            // 
            // dgvUser
            //
            this.dgvUserStatistic.Name = "dgvUser";
            this.dgvUserStatistic.Dock = DockStyle.Fill;
            //this.listUser.TabIndex = 4;

            this.dgvUserStatistic.AllowUserToAddRows = false;
            this.dgvUserStatistic.AllowUserToDeleteRows = false;
            this.dgvUserStatistic.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUserStatistic.ColumnHeadersVisible = false;
            this.dgvUserStatistic.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                new DataGridViewCheckBoxColumn (),
                new DataGridViewTextBoxColumn ()});
            this.dgvUserStatistic.MultiSelect = false;
            this.dgvUserStatistic.ReadOnly = true;
            this.dgvUserStatistic.RowHeadersVisible = false;
            this.dgvUserStatistic.RowTemplate.Height = 18;
            this.dgvUserStatistic.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvUserStatistic.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            ////this.dgvFilterRoles.Size = new System.Drawing.Size(190, 111);
            //this.dgvUserStatistic.SelectionChanged += new EventHandler(this.dgvUserStatistic_SelectionChanged);

            i = 0;
            this.dgvUserStatistic.Columns[i].Frozen = true;
            this.dgvUserStatistic.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvUserStatistic.Columns[i].Width = 20;
            // 
            // dataGridViewTextBoxColumnCounter
            // 
            i = 1;
            this.dgvUserStatistic.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dgvUserStatistic.Columns[i].Width = 140;

            #endregion

            #region Role

            // 
            // labelRole
            // 
            labelRole.AutoSize = true;
            //this.labelFilterRoles.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top));
            //this.labelFilterRoles.Location = new System.Drawing.Point(9, 50);
            labelRole.Name = "labelRole";
            //this.labelFilterRoles.Size = new System.Drawing.Size(77, 13);
            //this.labelMessage.TabIndex = 5;
            labelRole.Text = "Фильтр: роли";


            // 
            // listRole
            //
            this.dgvFilterRoleStatistic.Name = "dgvRole";
            this.dgvFilterRoleStatistic.Dock = DockStyle.Fill;
            //this.listUser.TabIndex = 4;

            this.dgvFilterRoleStatistic.AllowUserToAddRows = false;
            this.dgvFilterRoleStatistic.AllowUserToDeleteRows = false;
            this.dgvFilterRoleStatistic.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFilterRoleStatistic.ColumnHeadersVisible = false;
            this.dgvFilterRoleStatistic.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                new DataGridViewCheckBoxColumn (),
                new DataGridViewTextBoxColumn ()});
            this.dgvFilterRoleStatistic.MultiSelect = false;
            this.dgvFilterRoleStatistic.ReadOnly = true;
            this.dgvFilterRoleStatistic.RowHeadersVisible = false;
            this.dgvFilterRoleStatistic.RowTemplate.Height = 18;
            this.dgvFilterRoleStatistic.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFilterRoleStatistic.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //this.dgvFilterRoles.Size = new System.Drawing.Size(190, 111);
            this.dgvFilterRoleStatistic.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvFilterRoleStatistic_CellClick);

            i = 0;
            this.dgvFilterRoleStatistic.Columns[i].Frozen = true;
            this.dgvFilterRoleStatistic.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFilterRoleStatistic.Columns[i].Width = 20;
            // 
            // dataGridViewTextBoxColumnCounter
            // 
            i = 1;
            this.dgvFilterRoleStatistic.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dgvFilterRoleStatistic.Columns[i].Width = 140;

            #endregion

            #region StartCalendar

            // 
            // labelStartCalendar
            // 
            labelStartCalendar.AutoSize = true;
            //this.labelFilterRoles.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top));
            //this.labelFilterRoles.Location = new System.Drawing.Point(9, 50);
            labelStartCalendar.Name = "labelStartCalendar";
            //this.labelFilterRoles.Size = new System.Drawing.Size(77, 13);
            //this.labelMessage.TabIndex = 5;
            labelStartCalendar.Text = "Начало периода";


            // 
            // StartCalendar
            //
            this.StartCalendar.Name = "StartCalendar";
            this.StartCalendar.Dock = DockStyle.Fill;
            //this.listUser.TabIndex = 4;
            //this.StartCalendar.Value = DateTime.Now.Date.AddDays(-1);
            this.StartCalendar.ValueChanged += new System.EventHandler(this.startCalendar_ChangeValue);

            #endregion

            #region StopCalendar

            // 
            // labelStopCalendar
            // 
            labelStopCalendar.AutoSize = true;
            //this.labelFilterRoles.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top));
            //this.labelFilterRoles.Location = new System.Drawing.Point(9, 50);
            labelStopCalendar.Name = "labelStopCalendar";
            //this.labelFilterRoles.Size = new System.Drawing.Size(77, 13);
            //this.labelMessage.TabIndex = 5;
            labelStopCalendar.Text = "Окончание периода";

            // 
            // StartCalendar
            //
            this.EndCalendar.Name = "StopCalendar";
            this.EndCalendar.Dock = DockStyle.Fill;
            //this.listUser.TabIndex = 4;
            this.EndCalendar.ValueChanged += new System.EventHandler(this.stopCalendar_ChangeValue);

            #endregion

            #region panelUser
            // 
            // panelUser
            //
            panelUser.ColumnCount = 12; panelUser.RowCount = 24;
            for (i = 0; i < panelUser.ColumnCount; i++)
                panelUser.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / panelUser.ColumnCount));
            for (i = 0; i < panelUser.RowCount; i++)
                panelUser.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / panelUser.RowCount));
            panelUser.Dock = DockStyle.Fill;

            panelUser.Controls.Add(labelFilterActives, 0, 0); panelUser.SetColumnSpan(labelFilterActives, 2);
            panelUser.Controls.Add(this.dgvFilterActiveView, 0, 1); panelUser.SetColumnSpan(this.dgvFilterActiveView, 3); panelUser.SetRowSpan(this.dgvFilterActiveView, 3);

            panelUser.Controls.Add(labelFilterRoles, 0, 4); panelUser.SetColumnSpan(labelFilterRoles, 3);
            panelUser.Controls.Add(this.dgvFilterRoleView, 0, 5); panelUser.SetColumnSpan(this.dgvFilterRoleView, 3); panelUser.SetRowSpan(this.dgvFilterRoleView, 6);

            panelUser.Controls.Add(this.dgvUserView, 0, 11); panelUser.SetColumnSpan(this.dgvUserView, 3); panelUser.SetRowSpan(this.dgvUserView, 13);
                
            panelUser.Controls.Add(labelDatetimeStart, 3, 0); this.SetColumnSpan(labelDatetimeStart, 3);
            panelUser.Controls.Add(this.dgvListDateView, 3, 1); this.SetColumnSpan(this.dgvListDateView, 3); this.SetRowSpan(this.dgvListDateView, 11);
            panelUser.Controls.Add(labelFilterTypeMessage, 3, 12); this.SetColumnSpan(labelFilterTypeMessage, 3);
            panelUser.Controls.Add(this.dgvTypeToView, 3, 13); this.SetColumnSpan(this.dgvTypeToView, 3); this.SetRowSpan(this.dgvTypeToView, 9);
            panelUser.Controls.Add(this.dgvMessageView, 6, 0); this.SetColumnSpan(this.dgvMessageView, 6); this.SetRowSpan(this.dgvMessageView, 24);
            //this.panelUser.Controls.Add(this.buttonUpdate, 10, 23); this.SetColumnSpan(this.buttonUpdate, 2); this.SetRowSpan(this.buttonUpdate, 2);

            panelUser.ResumeLayout(false);
            panelUser.PerformLayout();

            groupUser.Controls.Add(panelUser);
            groupUser.Text = "События";


            #endregion

            #region panelTabs
            // 
            // panelTabs
            //
            panelTabs.ColumnCount = 12; panelUser.RowCount = 12;
            for (i = 0; i < panelTabs.ColumnCount; i++)
                panelTabs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / panelTabs.ColumnCount));
            for (i = 0; i < panelTabs.RowCount; i++)
                panelTabs.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / panelTabs.RowCount));
            panelTabs.Dock = DockStyle.Fill;

            panelTabs.Controls.Add(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.ANY], 0, 8); this.SetRowSpan(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.ANY], 2);
            panelTabs.Controls.Add(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.PC], 0,6); this.SetRowSpan(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.PC], 2);
            panelTabs.Controls.Add(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.GTP], 0, 4); this.SetRowSpan(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.GTP], 2);
            panelTabs.Controls.Add(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TG], 0, 2); this.SetRowSpan(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TG], 2);
            panelTabs.Controls.Add(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TEC], 0, 0); this.SetRowSpan(this.arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TEC], 2);
            panelTabs.Controls.Add(this.listBoxTabVisible, 2, 0); this.SetColumnSpan(this.listBoxTabVisible, 10); this.SetRowSpan(this.listBoxTabVisible, 12);
            panelTabs.Controls.Add(this.buttonUpdate, 0, 10); this.SetColumnSpan(this.buttonUpdate, 2); this.SetRowSpan(this.buttonUpdate, 2);

            panelTabs.ResumeLayout(false);
            panelTabs.PerformLayout();

            groupTabs.Controls.Add(panelTabs);
            groupTabs.Text = "Отображаемые вкладки";

            #endregion

            #region panelMessage
            // 
            // panelMessage
            //
            panelMessage.ColumnCount = 1; panelMessage.RowCount = 36;
            for (i = 0; i < panelMessage.ColumnCount; i++)
                panelMessage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / panelMessage.ColumnCount));
            for (i = 0; i < panelMessage.RowCount; i++)
                panelMessage.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / panelMessage.RowCount));
            panelMessage.Dock = DockStyle.Fill;

            panelMessage.Controls.Add(labelStartCalendar, 0, 0); this.SetColumnSpan(labelStartCalendar, 1); this.SetRowSpan(labelStartCalendar, 1);
            panelMessage.Controls.Add(this.StartCalendar, 0, 1); this.SetColumnSpan(this.StartCalendar, 1); this.SetRowSpan(this.StartCalendar, 2);

            panelMessage.Controls.Add(labelStopCalendar, 0, 3); this.SetColumnSpan(labelStopCalendar, 1); this.SetRowSpan(labelStopCalendar, 1);
            panelMessage.Controls.Add(this.EndCalendar, 0, 4); this.SetColumnSpan(this.EndCalendar, 1); this.SetRowSpan(this.EndCalendar, 2);


            panelMessage.Controls.Add(labelRole, 0, 6); this.SetColumnSpan(labelRole, 1); this.SetRowSpan(labelRole, 1);
            panelMessage.Controls.Add(this.dgvFilterRoleStatistic, 0, 7); this.SetColumnSpan(this.dgvFilterRoleStatistic, 1); this.SetRowSpan(this.dgvFilterRoleStatistic, 7);

            panelMessage.Controls.Add(labelUser, 0, 14); this.SetColumnSpan(labelUser, 1); this.SetRowSpan(labelUser, 1);
            panelMessage.Controls.Add(this.dgvUserStatistic, 0, 15); this.SetColumnSpan(this.dgvUserStatistic, 1); this.SetRowSpan(this.dgvUserStatistic, 12);

            panelMessage.Controls.Add(labelMessage, 0, 27); this.SetColumnSpan(labelMessage, 1); this.SetRowSpan(labelMessage, 1);
            panelMessage.Controls.Add(this.dgvTypeToStatistic, 0, 28); this.SetColumnSpan(this.dgvTypeToStatistic, 1); this.SetRowSpan(this.dgvTypeToStatistic, 8);

            panelMessage.ResumeLayout(false);
            panelMessage.PerformLayout();
            //this.panelMessage.

            groupMessage.Controls.Add(panelMessage);
            groupMessage.Text = "Статистика";

            #endregion


            this.Controls.Add(groupMessage, 9, 0); this.SetColumnSpan(groupMessage, 3); this.SetRowSpan(groupMessage, 24);
                
            this.Controls.Add(groupUser, 0, 0); this.SetColumnSpan(groupUser, 9); this.SetRowSpan(groupUser, 18);

            this.Controls.Add(groupTabs, 0, 18); this.SetColumnSpan(groupTabs, 9); this.SetRowSpan(groupTabs, 6);

            ((System.ComponentModel.ISupportInitialize)(this.dgvFilterActiveView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilterRoleView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUserView)).EndInit();
                
            this.tabPageLogging.ResumeLayout(false);
            this.tabPageLogging.PerformLayout();
            this.panelTabPageLogging.ResumeLayout(false);
            this.panelTabPageLogging.PerformLayout ();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTypeToView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvListDateView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMessageView)).EndInit();
            this.tabPageTabes.ResumeLayout(false);
            this.tabPageTabes.PerformLayout();
            this.panelTabPageTabes.ResumeLayout(false);
            this.panelTabPageTabes.PerformLayout ();
            //((System.ComponentModel.ISupportInitialize)(this.listTabVisible)).EndInit();

            this.Dock = System.Windows.Forms.DockStyle.Fill;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly (cols, rows);
        }

        #endregion

        #region Объявление переменных и констант

        /// <summary>
        /// Экземпляр класса 
        ///  для подключения/отправления/получения запросов к БД
        /// </summary>
        protected ILoggingReadHandler m_loggingReadHandler;

        protected enum DATAGRIDVIEW_LOGCOUNTER {
            TYPE_TO_VIEW, TYPE_TO_STATISTIC
        }

        protected Dictionary<DATAGRIDVIEW_LOGCOUNTER, PanelAnalyzer.DataGridView_LogMessageCounter> m_dictDataGridViewLogCounter;
        /// <summary>
        /// Время для запуска таймера
        /// </summary>
        protected int MSEC_TIMERCHECKED_STANDARD = 66666, 
            MSEC_TIMERCHECKED_FORCE = 666;
        /// <summary>
        /// Таймер
        /// </summary>
        private
            System.Threading.Timer
                m_timerProcChecked;

        protected bool m_bThreadTimerCheckedAllowed;
        /// <summary>
        /// Класс для работы с логом сообщений
        /// </summary>
        protected LogParse m_LogParse;
        ///// <summary>
        ///// Таблицы пользователей для лога сообщений и таблица с ролями 
        ///// </summary>
        //protected DataTable m_tableUsers, m_tableUsers_unfiltered,
        //    m_tableRoles;
        ///// <summary>
        ///// Таблицы пользователей для статистики сообщений 
        ///// </summary>
        //protected DataTable m_tableUsers_stat,
        //    m_tableUsers_stat_unfiltered;
        /// <summary>
        /// Массив CheckBox'ов для фильтра типов вкладок
        /// </summary>
        CheckBox[] m_arCheckBoxMode;
        /// <summary>
        /// Номер предыдущей строки Даты/Времени 
        /// </summary>
        int m_prevListDateViewRowIndex;
        /// <summary>
        /// Параметр для сортировки в таблице
        /// </summary>
        protected const string c_list_sorted = @"DESCRIPTION";
        /// <summary>
        /// Имя АРМ для подключения
        /// </summary>
        protected const string c_NameFieldToConnect = "COMPUTER_NAME";
        /// <summary>
        /// Список ТЭЦ
        /// </summary>
        protected List<StatisticCommon.TEC> m_listTEC;
        /// <summary>
        /// Индексы разделителей
        /// </summary>
        protected enum INDEX_DELIMETER { PART, ROW };
        /// <summary>
        /// Массив строк с разделителями на столбцы и строки
        /// </summary>
        protected static string[] s_chDelimeters = { @"DELIMETER_PART", "DELIMETER_ROW" };
        /// <summary>
        /// Делегат для передачи сообщения о ошибке
        /// </summary>
        protected DelegateStringFunc delegateErrorReport;
        /// <summary>
        /// Делегат для передачи предупреждения
        /// </summary>
        protected DelegateStringFunc delegateWarningReport;
        /// <summary>
        /// Делегат для передачи сообщения о выполняемом действии
        /// </summary>
        protected DelegateStringFunc delegateActionReport;
        /// <summary>
        /// Делегат для передачи команды об очистке
        /// </summary>
        protected DelegateBoolFunc delegateReportClear;

        #endregion

        public PanelAnalyzer(/*int idListener,*/ List<StatisticCommon.TEC> tec, Color foreColor, Color backColor)
            : base(MODE_UPDATE_VALUES.ACTION, foreColor, backColor)
        {
            _handlers = new Dictionary<StatesMachine, Action<REQUEST, DataTable>> () {
                { StatesMachine.ProcCheckedState, handlerCommandProcChecked }
                , { StatesMachine.ProcCheckedFilter, handlerCommandProcChecked }
                , { StatesMachine.ListMessageToUserByDate, handlerCommandListMessageToUserByDate }
                , { StatesMachine.ListDateByUser, handlerCommandListDateByUser }
                , { StatesMachine.CounterToTypeMessageByDate, handlerCommandCounterToTypeMessageByDate }
            };

            m_filterView = new Filter ();
            m_filterView.Changed += new Action (filterView_Changed);
            m_filterStatistic = new Filter();
            m_filterStatistic.Changed += new Action(filterStatistic_Changed);

            m_listTEC = tec;

            m_loggingReadHandler = newLoggingRead ();
            m_loggingReadHandler.EventCommandCompleted += new Action<REQUEST, DataTable> (loggingReadHandler_onCommandCompleted);
            m_LogParse = newLogParse();

            InitializeComponent();

            arrayCheckBoxModeTECComponent.ToList ()
                .ForEach (checkBoxMode => checkBoxMode.CheckedChanged += new EventHandler (checkBoxModeTECComponent_CheckedChanged));

            this.dgvTypeToStatistic.Tag = DATAGRIDVIEW_LOGCOUNTER.TYPE_TO_STATISTIC;
            this.dgvTypeToView.Tag = DATAGRIDVIEW_LOGCOUNTER.TYPE_TO_VIEW;
            m_dictDataGridViewLogCounter = new Dictionary<DATAGRIDVIEW_LOGCOUNTER, DataGridView_LogMessageCounter> () {
                { DATAGRIDVIEW_LOGCOUNTER.TYPE_TO_STATISTIC, this.dgvTypeToStatistic }
                , { DATAGRIDVIEW_LOGCOUNTER.TYPE_TO_VIEW, this.dgvTypeToView }
            };

            m_arCheckBoxMode = new CheckBox[] { arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TEC], arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.GTP], arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.PC], arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TG] };

            m_listTEC = tec;

            dgvFilterActiveView.Rows.Add(2);
            dgvFilterActiveView.Rows[0].Cells[0].Value = true; dgvFilterActiveView.Rows[0].Cells[1].Value = "Активные";
            dgvFilterActiveView.Rows[1].Cells[0].Value = true; dgvFilterActiveView.Rows[1].Cells[1].Value = "Не активные";
            dgvFilterActiveView.Columns[0].ReadOnly = false;
            dgvFilterActiveView.CellClick += new DataGridViewCellEventHandler(dgvFilterActiveView_CellClick);
            dgvFilterActiveView.Enabled = true;

            dgvTypeToView.CellClick += new DataGridViewCellEventHandler(dgvFilterTypeMessage_CellClick);

            #region Изменение при создании цвета фона-шрифта
            getTypedControls (this, new Type [] { typeof (DataGridView) }).Cast<DataGridView> ().ToList ().ForEach (dgv => {
                dgv.DefaultCellStyle.BackColor = BackColor == SystemColors.Control ? SystemColors.Window : BackColor;
                dgv.DefaultCellStyle.ForeColor = ForeColor;
            });

            if (Equals (listBoxTabVisible, null) == false) {
                listBoxTabVisible.BackColor = BackColor == SystemColors.Control ? SystemColors.Window : BackColor;
                listBoxTabVisible.ForeColor = ForeColor;
            } else
                ;
            #endregion

            int err = -1;

            if (!(m_LogParse == null))
                m_LogParse.Exit = LogParse_ThreadExit;
            else {
                string strErr = @"Не создан объект разбора сообщений (класс 'LogParse')...";
                throw new Exception(strErr);
            }
        }

        private class Filter
        {
            private Dictionary<HStatisticUsers.ID_ROLES, bool> _roles;

            public event Action Changed;

            public void Set (HStatisticUsers.ID_ROLES key, bool value)
            {
                if (_roles.ContainsKey (key) == true)
                    _roles [key] = value;
                else
                    throw new InvalidOperationException ($"PanelAnalyzer.Filter::Set (Key={key.ToString()}, Value={value.ToString()}) - ключ отсутствует в словаре");

                Changed?.Invoke ();
            }

            public List<HStatisticUsers.ID_ROLES> IsAllowedRoles
            {
                get
                {
                    return (from role in _roles where role.Value == true select role.Key).ToList ();
                }
            }

            private CheckState? _actived;

            public CheckState? Actived
            {
                get
                {
                    return _actived;
                }

                set
                {
                    _actived = value;

                    Changed?.Invoke ();
                }
            }

            public Filter ()
            {
                _roles = new Dictionary<HStatisticUsers.ID_ROLES, bool> {
                    { HStatisticUsers.ID_ROLES.KOM_DISP, true }
                    , { HStatisticUsers.ID_ROLES.ADMIN, true }
                    , { HStatisticUsers.ID_ROLES.USER, true }
                    , { HStatisticUsers.ID_ROLES.NSS, true }
                    , { HStatisticUsers.ID_ROLES.MAJOR_MASHINIST, true }
                    , { HStatisticUsers.ID_ROLES.MASHINIST, true }
                    , { HStatisticUsers.ID_ROLES.LK_DISP, true }
                };
                // отображать все
                Actived = CheckState.Checked;
            }
        }

        private Filter m_filterView
            , m_filterStatistic;

        #region Объявление абстрактных методов

        /// <summary>
        /// Создать объект для чтения сообщений журнала
        /// </summary>
        /// <returns></returns>
        protected abstract ILoggingReadHandler newLoggingRead ();

        /// <summary>
        /// Метод для разбора строки с лог сообщениями
        /// </summary>
        /// <returns>Возвращает строку</returns>
        protected abstract LogParse newLogParse ();

        /// <summary>
        /// Проверка запуска пользователем ПО
        /// </summary>
        /// <return>Массив занчений активности каждого пользователя</return>
        private void procChecked()
        {
            m_loggingReadHandler.Command(StatesMachine.ProcCheckedFilter, null, false);
            //// фиктивный результат для совместимости с синхронным вариантом
            //return new bool[] { };
        }

        /// <summary>
        /// Запись значений активности в CheckBox на DataGridView с пользователями
        /// </summary>
        /// <param name="obj">???</param>
        private void procChecked (object obj)
        {
            delegateActionReport ("Получаем список активных пользователей");

            m_loggingReadHandler.Command (StatesMachine.ProcCheckedState, null, false);
        }

        /// <summary>
        /// ???
        /// </summary>
        protected abstract void disconnect();

        /// <summary>
        /// Старт разбора лог-сообщений
        /// </summary>
        /// <param name="id">ID пользователя</param>
        protected virtual void startLogParse (string id)
        {
            dgvListDateView.SelectionChanged -= dgvListDateView_SelectionChanged;

            m_loggingReadHandler.Command (StatesMachine.ListDateByUser, new object [] { int.Parse (id) }, true);
            //m_loggingReadHandler.Command (StatesMachine.ListMessageToUserByDate, new object [] { int.Parse (id) }, true);
        }

        /// <summary>
        /// Получение первой строки лог-сообщений
        /// </summary>
        /// <param name="r">Строка из таблицы</param>
        /// <returns>Вызвращает строку string</returns>
        protected abstract string getTabLoggingTextRow(DataRow r);

        #endregion

        /// <summary>
        /// Метод для передачи сообщений на форму
        /// </summary>
        /// <param name="ferr">Делегат для передачи сообщения об ошибке</param>
        /// <param name="fwar">Делегат для передачи предупреждения</param>
        /// <param name="fact">Делегат для передачи сообщения о выполняемом действии</param>
        /// <param name="fclr">Делегат для передачи комады об очистке строки статуса</param>
        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            delegateErrorReport = ferr;
            delegateWarningReport = fwar;
            delegateActionReport = fact;
            delegateReportClear = fclr;
        }

        /// <summary>
        /// Запуск таймера обновления данных
        /// </summary>
        public override void Start()
        {
            int err = -1;
            DataTable tableOut;

            base.Start();

            DbConnection connConfigDB = DbSources.Sources ().GetConnection (DbTSQLConfigDatabase.DbConfig ().ListenerId, out err);

            if ((!(connConfigDB == null))
                && (err == 0)) {
                HStatisticUsers.GetRoles (ref connConfigDB, string.Empty, string.Empty, out tableOut, out err);
                dgvFilterRoleStatistic.Fill (tableOut, @"DESCRIPTION", err, true);
                dgvFilterRoleView.Fill (tableOut, @"DESCRIPTION", err, true);

                HStatisticUsers.GetUsers (ref connConfigDB, string.Empty, c_list_sorted, out tableOut, out err);
                dgvUserStatistic.Fill (tableOut, @"DESCRIPTION", err, true);
                dgvUserView.Fill (tableOut, @"DESCRIPTION", err);

                m_bThreadTimerCheckedAllowed = true;

                //Вариант №2
                m_timerProcChecked =
                    new System.Threading.Timer (new TimerCallback (procChecked), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite)
                    //new System.Windows.Forms.Timer()
                    ;
                ////Вариант №1
                //m_timerChecked.Interval = MSEC_TIMERCHECKED_STANDARD;
                //m_timerChecked.Tick += new EventHandler(ProcChecked);
                //m_timerChecked.Start ();
            } else
                throw new Exception("PanelAnalyzer::Start () - нет соединения с БД конфигурации...");
        }

        /// <summary>
        /// Остановка таймера обновления данных
        /// </summary>
        public override void Stop()
        {
            m_loggingReadHandler.Activate (false);
            m_loggingReadHandler.Stop ();

            m_bThreadTimerCheckedAllowed = false;

            base.Stop();
        }

        /// <summary>
        /// Активировать/деактивировать панель
        /// </summary>
        /// <param name="active">Признак активации/деактивации</param>
        /// <returns>Признак ошибки/успеха выполнения</returns>
        public override bool Activate(bool activated)
        {
            bool bRes = base.Activate(activated);

            if (IsFirstActivated == false)
                activateTimerChecked(CheckState.Checked);
            else
                ;

            return bRes;
        }

        private void activateTimerChecked (CheckState? modeDueTime)
        {
            int iDueTime = System.Threading.Timeout.Infinite;

            if (Equals(modeDueTime, null) == false)
                switch (modeDueTime) {
                    case CheckState.Checked:
                    // немедленно
                        iDueTime = 0;
                        break;
                    case CheckState.Unchecked:
                        iDueTime = MSEC_TIMERCHECKED_STANDARD;
                        break;
                    case CheckState.Indeterminate:
                        iDueTime = MSEC_TIMERCHECKED_FORCE;
                        break;
                    default:
                        break;
                }
            else
                ;

            m_timerProcChecked?.Change (iDueTime, System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Отработка ошибки соединения для пользователя УЖЕ отсутствующего в списке
        /// </summary>
        /// <param name="ValueToCreate">Имя пользователя</param>
        protected void errorConnect(string ValueToCreate)
        {
            int indx = Convert.ToInt32(ValueToCreate.Split(';')[1]);

            Logging.Logg().Error(string.Format("FormAnalyzer::ErrorConnect () - {0}, индекс: {1}", ValueToCreate.Split(';')[0], ValueToCreate.Split(';')[1]), Logging.INDEX_MESSAGE.NOT_SET);

            Console.WriteLine("FormAnalyzer::ErrorConnect () - {0}, индекс: {1}", ValueToCreate.Split(';')[0], ValueToCreate.Split(';')[1]);
            if (indx < dgvUserView.Rows.Count) //m_tableUsers, m_listTcpClient
                dgvUserView.Rows[indx].Cells[0].Value = false;
            else
                ; //Отработка ошибки соединения для пользователя УЖЕ отсутствующего в списке
        }

        #region Панель лог-сообщений

        /// <summary>
        /// Очистка таблицы с датами
        /// </summary>
        protected void clearListDateView() { dgvListDateView.Rows.Clear(); }

        /// <summary>
        /// Очистка таблицы с лог-сообщениями
        /// </summary>
        /// <param name="bClearCounter">Признак очистки статистики</param>
        protected void clearMessageView(bool bClearCounter)
        {
            //if (bClearCounter)
            //    m_LogCounter.Clear();
            //else
            //    ;

            dgvMessageView.Rows.Clear();
        }

        /// <summary>
        /// Метод для перемещения фокуса в таблице с лог-сообщениями на первую строку.
        /// </summary>
        void TabLoggingPositionText()
        {
            if (dgvMessageView.Rows.GetRowCount (DataGridViewElementStates.Visible) > 0)
                dgvMessageView.FirstDisplayedScrollingRowIndex = dgvMessageView.Rows.GetFirstRow(DataGridViewElementStates.Visible);
            else
                ;
        }

        /// <summary>
        /// Класс для получения строки с лог-сообщениями
        /// </summary>
        class HTabLoggingAppendTextPars
        {
            //public bool bClearCounter;
            public string rows;

            public HTabLoggingAppendTextPars(string rs)
            {
                //bClearCounter = bcc;
                rows = rs;
            }
        }


        /// <summary>
        /// Обновление счетчика типов сообщений
        /// </summary>
        /// <param name="dgv">DataGridView в которую поместить результаты</param>
        /// <param name="start_date">Начало периода</param>
        /// <param name="end_date">Окончание периода</param>
        /// <param name="users">Список пользователей</param>
        protected void updateCounter(DATAGRIDVIEW_LOGCOUNTER tag, DateTime start_date, DateTime end_date, string users)
        {
            m_loggingReadHandler.Command(StatesMachine.CounterToTypeMessageByDate, new object[] { tag, start_date, end_date, users }, false);
        }

        /// <summary>
        /// Метод заполнения таблиц с типами сообщений и лог-сообщениями
        /// </summary>
        void TabLoggingAppendText(object data)
        {
            HTabLoggingAppendTextPars tlatPars;
            int idTypeMessage = -1
                , iNewRow = -1;
            Dictionary<int, bool> dictTypeChecked;
            string[] messages
                , parts;

            try
            {
                delegateActionReport("Получаем список сообщений");

                tlatPars = data as HTabLoggingAppendTextPars;

                if (tlatPars.rows.Equals(string.Empty) == true)
                {
                    TabLoggingPositionText();
                    //textBoxLog.Focus();
                    //textBoxLog.AutoScrollOffset = new Point (0, 0);
                }
                else
                {
                    //Получение массива состояний CheckBox'ов на DataGridView с типами сообщений
                    dictTypeChecked = dgvTypeToView.Checked();
                    //Преобразование строки в массив строк с сообщениями
                    messages = tlatPars.rows.Split(new string[] { s_chDelimeters[(int)INDEX_DELIMETER.ROW] }, StringSplitOptions.None);

                    //Помещение массива строк с сообщениями в DataGridView с сообщениями
                    foreach (string text in messages) {
                        parts = text.Split(new string[] { s_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None);
                        idTypeMessage = Int32.Parse(parts[1]);

                        iNewRow = dgvMessageView.Rows.Add(parts);
                        dgvMessageView.Rows[iNewRow].Tag = idTypeMessage;
                        //Фильтрация сообщений в зависимости от включенных CheckBox'ов в DataGridView с типами сообщений
                        dgvMessageView.Rows[iNewRow].Visible = dictTypeChecked[idTypeMessage];
                    }
                }

                delegateReportClear(true);
            } catch (Exception e) {
                Logging.Logg().Exception(e, "PanelAnalyzer:TabLoggingAppendText () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        /// <summary>
        /// Метод заполнения таблицы с датой для фильтрации лог-сообщений
        /// </summary>
        void TabLoggingAppendDate(List<DateTime> listDate)
        {
            int iNewRow = -1;
            bool bRowChecked = false;
            DataGridViewRow newRow;

            foreach (DateTime date in listDate) {
                bRowChecked = listDate.IndexOf(date) == listDate.Count - 1;

                if (bRowChecked == true) {
                    dgvListDateView.SelectionChanged += dgvListDateView_SelectionChanged;
                    m_prevListDateViewRowIndex = dgvListDateView.Rows.Count;
                } else
                    ;

                newRow = dgvListDateView.RowTemplate.Clone () as DataGridViewRow;
                newRow.Tag = date;
                iNewRow  = dgvListDateView.Rows.Add(newRow);
                dgvListDateView.Rows[iNewRow].SetValues (new object [] { bRowChecked, date });
            }

            if (bRowChecked == true) {
                dgvListDateView.Rows[dgvListDateView.Rows.Count - 1].Selected = bRowChecked;
                dgvListDateView.FirstDisplayedScrollingRowIndex = 0;
            } else
                ;
        }

        private DateTime LogMessageViewDate
        {
            get
            {
                return
                    // (DateTime)dgvListDateView.Rows [dgvListDateView.SelectedRows [0].Index].Tag
                    (DateTime)dgvListDateView.SelectedRows [0].Tag
                    ;
            }
        }

        /// <summary>
        /// Получение списка дата/время запуска приложения
        /// </summary>
        protected virtual void LogParse_ThreadExit(LogParse.PARAM_THREAD_PROC param)
        {
            int i = -1;
            DataRow[] rows = new DataRow[] { };

            switch (param.Mode) {
                case LogParse.MODE.LIST_DATE:
                    if (m_LogParse.ListDate.Count > 0) {
                        BeginInvoke (new Action<List<DateTime>> (TabLoggingAppendDate), m_LogParse.ListDate);
                    } else {
                        Logging.Logg ().Warning ($"PanalAnalyzer::LogParseExit () - сообщения отсутствуют...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                    break;
                case LogParse.MODE.MESSAGE:
                    //Нюанс для 'LogParse_File' и 'LogParse_DB' - "0" индекс в массиве типов сообщений (зарезервирован для "СТАРТ")
                    //...для 'LogParse_File': m_tblLog содержит ВСЕ записи в файле
                    //...для 'LogParse_DB': m_tblLog содержит ТОЛЛЬКО записи из БД с датами, за которые найдено хотя бы одно сообщение
                    //      тип сообщения "СТАРТ" устанавливается "программно" (метод 'LogParse_DB::Thread_Proc')
                    //0 - индекс в массиве идентификаторов зарезервирован для сообщений типа "СТАРТ"
                    rows = m_LogParse.ByDate (@"true" + s_chDelimeters [(int)INDEX_DELIMETER.ROW] + ((int)LogParse.INDEX_START_MESSAGE).ToString (), DateTime.MaxValue, DateTime.MaxValue);

                    filldgvLogMessages (m_LogParse.Sort (@"DATE_TIME"));

                    updateCounter (DATAGRIDVIEW_LOGCOUNTER.TYPE_TO_VIEW
                        , LogMessageViewDate 
                        , LogMessageViewDate.AddDays (1)
                        , IdCurrentUserView.ToString ());

                    // немедленно обновить состояния активности пользователей
                    activateTimerChecked (CheckState.Checked);
                    break;
                case LogParse.MODE.COUNTER:
                    break;
                default:
                //TODO: проверка на неизвестный режим уже была выполнена
                    break;
            }
        }

        /// <summary>
        /// Вызов метода заполнения таблицы с лог-сообщениями
        /// </summary>
        /// <param name="rows">Перечень строк с  сообщениями журнала</param>
        protected void filldgvLogMessages (DataRow[] rows)
        {
            DelegateObjectFunc delegateAppendText = new DelegateObjectFunc(TabLoggingAppendText);
            string strRowsTodgvLogMessages = string.Empty;

            if (rows.Length > 0)
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    if (((i % 1000) == 0)
                        && (strRowsTodgvLogMessages.Length > s_chDelimeters[(int)INDEX_DELIMETER.ROW].Length)) {
                        //Сбор строки из лог-сообщений
                        strRowsTodgvLogMessages = strRowsTodgvLogMessages.Substring(0, strRowsTodgvLogMessages.Length - s_chDelimeters[(int)INDEX_DELIMETER.ROW].Length);
                        //Асинхронное выполнение метода заполнения DataGridView с лог-сообщениями
                        this.BeginInvoke(delegateAppendText, new HTabLoggingAppendTextPars(strRowsTodgvLogMessages));
                        //this.BeginInvoke(new DelegateObjectFunc(TabLoggingAppendText), strRowsTodgvLogMessages);
                        strRowsTodgvLogMessages = string.Empty;
                    } else
                        try {
                            strRowsTodgvLogMessages += getTabLoggingTextRow(rows[i]) + s_chDelimeters[(int)INDEX_DELIMETER.ROW];
                        } catch (Exception e) {
                            Logging.Logg().Exception(e, "PanelAnalyzer:filldgvLogMessages () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                }

                if (strRowsTodgvLogMessages.Length > s_chDelimeters[(int)INDEX_DELIMETER.ROW].Length) {
                    //Остаток...
                    strRowsTodgvLogMessages = strRowsTodgvLogMessages.Substring(0, strRowsTodgvLogMessages.Length - s_chDelimeters[(int)INDEX_DELIMETER.ROW].Length);
                    this.BeginInvoke(delegateAppendText, new HTabLoggingAppendTextPars(strRowsTodgvLogMessages));
                    //this.BeginInvoke(new DelegateObjectFunc(TabLoggingAppendText), strRowsTodgvLogMessages);
                    strRowsTodgvLogMessages = string.Empty;
                } else
                    ;
            } else
                ;

            //Крайняя строка...
            this.BeginInvoke(delegateAppendText, new HTabLoggingAppendTextPars(strRowsTodgvLogMessages));
            //this.BeginInvoke(new DelegateObjectFunc(TabLoggingAppendText), string.Empty);
        }

        /// <summary>
        /// Получение индексов типов сообщений
        /// </summary>
        /// <returns></returns>
        private string getIndexTypeMessages()
        {
            string[] strRes = new string[] { string.Empty, string.Empty };
            int indxRes = -1
                , cntTrue = 0;

            foreach (DataGridViewRow row in dgvTypeToView.Rows)
            {
                if (((DataGridViewCheckBoxCell)row.Cells[0]).Value.Equals(true) == true)
                {
                    strRes[0] += dgvTypeToView.Rows.IndexOf(row) + s_chDelimeters[(int)INDEX_DELIMETER.PART];
                    cntTrue++;
                }
                else
                    if (((DataGridViewCheckBoxCell)row.Cells[0]).Value.Equals(false) == true)
                    {
                        strRes[1] += dgvTypeToView.Rows.IndexOf(row) + s_chDelimeters[(int)INDEX_DELIMETER.PART];
                    }
                    else
                        ;
            }

            if (strRes[0].Length > 0)
            {
                indxRes = 0;

                if (cntTrue == dgvTypeToView.Rows.Count)
                    //Все типы "отмечены"
                    strRes[0] = string.Empty;
                else
                {
                    strRes[0] = true.ToString() + s_chDelimeters[(int)INDEX_DELIMETER.ROW] + strRes[0];
                    strRes[0] = strRes[0].Substring(0, strRes[0].Length - s_chDelimeters[(int)INDEX_DELIMETER.PART].Length);
                }
            }
            else
            {
                indxRes = 1;

                strRes[1] = false.ToString() + s_chDelimeters[(int)INDEX_DELIMETER.ROW] + strRes[1];
                strRes[1] = strRes[1].Substring(0, strRes[1].Length - s_chDelimeters[(int)INDEX_DELIMETER.PART].Length);
            }

            return strRes[indxRes];
        }

        /// <summary>
        /// Старт формирования DataGridView с лог-сообщениями 
        /// </summary>
        /// <param name="bClearTypeMessageCounter">Флаг очистки DataGridView с лог-сообщениями </param>
        private void startSelectdgvLogMessages(bool bClearTypeMessageCounter)
        {
            clearMessageView(bClearTypeMessageCounter);

            DateTime dtBegin = DateTime.MaxValue
                //, dtEnd = DateTime.MaxValue
                ;
            if ((!(m_prevListDateViewRowIndex < 0))
                && (m_prevListDateViewRowIndex < dgvListDateView.Rows.Count)) {
                dtBegin = (DateTime)dgvListDateView.Rows [m_prevListDateViewRowIndex].Tag;
            } else
                ;
            ////TODO: возможно указание периода более, чем одни сутки
            //if ((!(m_prevListDateViewRowIndex < 0))
            //    && ((m_prevListDateViewRowIndex + 1) < dgvListDateView.Rows.Count)) {
            //    dtEnd = (DateTime)dgvListDateView.Rows [m_prevListDateViewRowIndex + 1].Tag;
            //} else
            //    ;

            if (dtBegin.Equals(DateTime.MaxValue) == false)
            // 0 - идентификатор пользователя
            // 1 - тип сообщение
            // 2 - дата/время - начало
            // 3 - дата/время - окончание
            // 4 - признак
                m_loggingReadHandler.Command(StatesMachine.ListMessageToUserByDate
                    , new object[] {
                        IdCurrentUserView
                        , getIndexTypeMessages()
                        , dtBegin
                        , /*dtEnd*/ dtBegin.AddDays(1)
                        , bClearTypeMessageCounter
                    }
                    , false);
            else
                ;
        }
        
        /// <summary>
        /// Старт обновления DataGridView с лог-сообщениями 
        /// </summary>
        /// <param name="bClearTypeMessageCounter">Флаг очистки DataGridView с лог-сообщениями </param>
        private void applyFilterLogMessages()
        {
            //Thread threadApplyFilterLogMessage = new Thread(new ParameterizedThreadStart ((obj) => {
                Dictionary<int, bool> dictChecked;

                dictChecked = dgvTypeToView.Checked();
                dgvMessageView.Rows.Cast<DataGridViewRow>().ToList().ForEach(r => {
                    r.Visible = dictChecked[(int)r.Tag];
                });
            //}));
            //threadApplyFilterLogMessage.IsBackground = true;
            //threadApplyFilterLogMessage.Start();
        }

        #endregion

        #region Обработчики событий для элементов панели лог-сообщений

        /// <summary>
        /// Обработчик выбора активных/неактивных пользователей
        /// </summary>
        private void dgvFilterActiveView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CheckState? checkStateActived = null;

            dgvFilterActiveView.Rows[e.RowIndex].Cells[0].Value = !bool.Parse(dgvFilterActiveView.Rows[e.RowIndex].Cells[0].Value.ToString());

            if (((bool)dgvFilterActiveView.Rows [0].Cells [0].Value == true)
                && ((bool)dgvFilterActiveView.Rows [1].Cells [0].Value == true))
            // все
                checkStateActived = CheckState.Checked;
            else if (((bool)dgvFilterActiveView.Rows [0].Cells [0].Value == true)
                && ((bool)dgvFilterActiveView.Rows [1].Cells [0].Value == false))
            // только активные
                checkStateActived = CheckState.Indeterminate;
            else if (((bool)dgvFilterActiveView.Rows [0].Cells [0].Value == false)
                && ((bool)dgvFilterActiveView.Rows [1].Cells [0].Value == true))
            // только не активные
                checkStateActived = CheckState.Unchecked;
            else
            // оставить "как есть" - ничего не отображать
                ;

            m_filterView.Actived = checkStateActived;
            //!!! обработка события 
        }

        /// <summary>
        /// Обработчик выбора ролей пользователей
        /// </summary>
        private void dgvFilterRoleView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dgvFilterRoleView.Rows[e.RowIndex].Cells[0].Value = !bool.Parse(dgvFilterRoleView.Rows[e.RowIndex].Cells[0].Value.ToString());

            m_filterView.Set ((HStatisticUsers.ID_ROLES)dgvFilterRoleView.Rows [e.RowIndex].Tag, (bool)dgvFilterRoleView.Rows [e.RowIndex].Cells [0].Value);
            //!!! обработка события 
        }

        protected void applyFilterView (int []ariValuesTags, bool [] arbValuesActived)
        {
            //CheckState.Checked - все
            //CheckState.Indeterminate - только активные
            //CheckState.Unchecked - только не активные

            bool bVisibled = false
                , bValuesActived = false;
            int id_user = -1
                , indx_values = -1;

            dgvUserView.Rows.Cast<DataGridViewRow>().ToList().ForEach(r => {
                bVisibled = false;

                id_user = ((HStatisticUser)r.Tag).Id;
                indx_values = ariValuesTags.ToList().IndexOf(id_user);

                bValuesActived = !(indx_values < 0)
                    ? arbValuesActived[indx_values]
                        : false;

                if (Equals(m_filterView.Actived, null) == false) {
                    if (m_filterView.Actived == CheckState.Checked)
                        bVisibled = true;
                    else if ((m_filterView.Actived == CheckState.Unchecked)
                        && (bValuesActived == false))
                        bVisibled = true;
                    else if ((m_filterView.Actived == CheckState.Indeterminate)
                        && (bValuesActived == true))
                        bVisibled = true;
                    else
                        ;

                    bVisibled &= m_filterView.IsAllowedRoles.Contains(((HStatisticUser)r.Tag).Role);
                } else
                    ;

                if (id_user == IdCurrentUserView)
                    if (bVisibled == false) {
                        clearListDateView();
                        clearMessageView(true);
                        dgvTypeToView.ClearValues();
                    } else {
                        dgvUserView_SelectionChanged(dgvUserView, EventArgs.Empty);
                    }

                r.Visible = bVisibled;

            });
        }

        private void filterView_Changed ()
        {
            // при этом инициируется выполнение команды ProcCheck
            if (Equals (m_filterView.Actived, null) == false)
            // асинхронный вызов 'procChecked ()' - возвращает фиктивный/пустой список
                procChecked ();
            else {
                // синхронно - пустой список пользователей
                dgvUserView.Rows.Cast<DataGridViewRow>().ToList().ForEach(r => r.Visible = false);
            }
        }

        private void filterStatistic_Changed()
        {
        }

        /// <summary>
        /// Обработчик выбора типа лог сообщений
        /// </summary>
        private void dgvFilterTypeMessage_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                dgvTypeToView.Rows[e.RowIndex].Cells[0].Value = !bool.Parse(dgvTypeToView.Rows[e.RowIndex].Cells[0].Value.ToString());

                applyFilterLogMessages();
            }
            else
                ;
        }

        /// <summary>
        /// Обновление данных пользователя
        /// </summary>
        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            dgvUserView_SelectionChanged(dgvUserView, EventArgs.Empty);
        }

        /// <summary>
        /// Обработчик выбора Даты/Времени для лог сообщений
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">Аргумент события</param>
        protected void dgvListDateView_SelectionChanged(object sender, EventArgs e)
        {
            int rowIndex =
                //IndexCurrentUserView
                dgvListDateView.SelectedRows.Count > 0 ? dgvListDateView.SelectedRows[0].Index : -1
                ;

            if (!(rowIndex < 0)) {
                dgvListDateView.Rows[m_prevListDateViewRowIndex].Cells[0].Value = false;
                dgvListDateView.Rows[rowIndex].Cells[0].Value = true;
                m_prevListDateViewRowIndex = rowIndex;

                startSelectdgvLogMessages(true);
            } else
                m_prevListDateViewRowIndex = -1;
        }

        #endregion
        
        #region Панель статистики

        /// <summary>
        /// Запрос на обновление статистики сообщений
        /// </summary>
        /// <param name="dgv_user">DataGridView с пользователями который нужно опросить</param>
        /// <param name="multi_select">Выбирается много пользователей</param>
        protected string get_users(DataGridView dgv_user, bool multi_select)
        {
            delegateActionReport("Получаем список пользователей");

            int i = -1;
            string list_user = string.Empty;//Строка со списком выбранных пользователей
            string where = string.Empty;

            #region Получение строки со списком идентификаторов пользователей, выбранных в списке

            if (multi_select == true)
                //перебор строк таблицы
                for (i = 0; i < dgv_user.Rows.Count; i++)
                    //сравнение состояния CheckBox'а
                    if ((dgv_user.Rows[i].Visible == true)
                        && (bool.Parse(dgv_user.Rows[i].Cells[0].Value.ToString()) == true)) {
                        if (list_user.Equals (string.Empty) == true)
                            list_user = " ";
                        else
                            list_user += ",";
                        //добавление пользователя в список
                        list_user += ((HStatisticUser)dgv_user.Rows [i].Tag).Id;
                    } else
                        ;
            else
            //добавление одного пользователя
                list_user += ((HStatisticUser)dgv_user.SelectedRows[0].Tag).Id;

            #endregion

            delegateReportClear(true);

            return list_user;
        }

        /// <summary>
        /// Обновление статистики сообщений из лога за выбранный период
        /// </summary>
        protected virtual void dgvUserView_SelectionChanged (object sender, EventArgs ev)
        {
            bool bUpdate = false;

            try {
                bUpdate = !(IdCurrentUserView < 0);
            } catch (Exception e) {
                Logging.Logg().Exception(e, $"PanelAnalyzer::dgvUserView_SelectionChanged () - call 'IdCurrentUserView'...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            //bUpdate = (dgvListDateView.Rows.Count > 0)
            //    && (dgvListDateView.SelectedRows [0].Index < (dgvListDateView.Rows.Count))
            //    && !Equals (e, null);

            if (bUpdate == true) {
                dgvListDateView.SelectionChanged -= dgvListDateView_SelectionChanged;
                clearListDateView ();
                //dgvListDateView.SelectionChanged += dgvDateView_SelectionChanged;
                //Останов потока разбора лог-файла пред. пользователя
                m_LogParse.Stop();
                //очистка списка активных вкладок
                listBoxTabVisible.Items.Clear();
                //Обнуление счётчиков сообщений на панели статистики
                dgvTypeToView.ClearValues();
            } else
                throw new InvalidOperationException ("PanelAnalyzer::dgvUserView_SelectionChanged () - не удается получить идентификатор пользователя...");
        }

        protected int IdCurrentUserView
        {
            get
            {
                if (Equals(dgvUserView, null) == false)
                    return dgvUserView.RowCount > 0
                        ? dgvUserView.SelectedRows.Count > 0
                            ? ((HStatisticUser)dgvUserView.SelectedRows[0].Tag).Id
                                : -1
                                    : -1;
                else
                    throw new InvalidOperationException($"PanelAnalyzer.IdCurrentUserView::get - [dgvUserView] is null...");
            }
        }

        protected HStatisticUsers.ID_ROLES RoleCurrentUserView
        {
            get
            {
                if (Equals(dgvUserView, null) == false)
                    return dgvUserView.RowCount > 0
                        ? dgvUserView.SelectedRows.Count > 0
                            ? ((HStatisticUser)dgvUserView.SelectedRows[0].Tag).Role
                                : HStatisticUsers.ID_ROLES.UNKNOWN
                                    : HStatisticUsers.ID_ROLES.UNKNOWN;
                else
                    throw new InvalidOperationException($"PanelAnalyzer.IdCurrentUserView::get - [dgvUserView] is null...");
            }
        }

        /// <summary>
        /// Индекс выбранной/текущей строки в списке с пользователями
        /// </summary>
        [System.Obsolete ("Использовать IdCurrentUserView вместо", false)]
        protected int IndexCurrentUserView
        {
            get
            {
                return dgvUserView.RowCount > 0
                    ? dgvUserView.SelectedRows.Count > 0
                        ? dgvUserView.SelectedRows [0].Index
                            : -1
                                : -1;
            }
        }

        /// <summary>
        /// Обработчик события изменения состояния CheckBox'ов
        /// </summary>
        private void checkBoxModeTECComponent_CheckedChanged (object sender, EventArgs e)
        {
            listBoxTabVisible.Items.Clear ();

            DbTSQLConfigDatabase.DbConfig().SetConnectionSettings();
            DbTSQLConfigDatabase.DbConfig().Register();
            //Заполнение ListBox'а активными вкладками
            fillListBoxTabVisible(
                getTabActived ((int)RoleCurrentUserView, IdCurrentUserView).ToList ()
            );
            DbTSQLConfigDatabase.DbConfig().UnRegister();
        }

        /// <summary>
        /// Обновление списка пользователей при выборе ролей
        /// </summary>
        private void dgvFilterRoleStatistic_CellClick (object sender, DataGridViewCellEventArgs e)
        {
            (sender as DataGridView).Rows[e.RowIndex].Cells[0].Value =
                !(bool)(sender as DataGridView).Rows[e.RowIndex].Cells[0].Value;
            m_filterStatistic.Set((HStatisticUsers.ID_ROLES)(sender as DataGridView).Rows[e.RowIndex].Tag
                , (bool)(sender as DataGridView).Rows[e.RowIndex].Cells[0].Value);

            (dgvUserStatistic as DataGridView).Rows.Cast<DataGridViewRow>().ToList().ForEach(r => {
                r.Visible = m_filterStatistic.IsAllowedRoles.Contains(((HStatisticUser)r.Tag).Role);
            });

            //TODO: обновить статистику
            updateCounter(DATAGRIDVIEW_LOGCOUNTER.TYPE_TO_STATISTIC
                , HDateTime.ToMoscowTimeZone(StartCalendar.Value.Date)
                , HDateTime.ToMoscowTimeZone(EndCalendar.Value.Date)
                , get_users(dgvUserStatistic, true));
        }

        /// <summary>
        /// Обновление списка пользователей при выборе начальной даты
        /// </summary>
        private void startCalendar_ChangeValue(object sender, EventArgs e)
        {
            updateCounter(DATAGRIDVIEW_LOGCOUNTER.TYPE_TO_STATISTIC
                , HDateTime.ToMoscowTimeZone(StartCalendar.Value.Date)
                , HDateTime.ToMoscowTimeZone(EndCalendar.Value.Date)
                , get_users(dgvUserStatistic, true));
        }

        /// <summary>
        /// Обновление списка пользователей при выборе конечной даты
        /// </summary>
        private void stopCalendar_ChangeValue(object sender, EventArgs e)
        {
            updateCounter(DATAGRIDVIEW_LOGCOUNTER.TYPE_TO_STATISTIC
                , HDateTime.ToMoscowTimeZone(StartCalendar.Value.Date)
                , HDateTime.ToMoscowTimeZone(EndCalendar.Value.Date)
                , get_users(dgvUserStatistic, true));
        }

        #endregion

        #region Панель вкладок

        /// <summary>
        /// Абстрактный метод для получения из БД списка активных вкладок для выбранного пользователя
        /// </summary>
        /// <param name="role">Идентификатор роли(группы) пользователя</param>
        /// <param name="user">Идентификатор пользователя для выборки</param>
        protected abstract IEnumerable<int> getTabActived(int role, int user);

        /// <summary>
        /// Заполнение ListBox активными вкладками
        /// </summary>
        /// <param name="arIdTabs">Список активных вкладок</param>
        protected virtual void fillListBoxTabVisible (List<int> arIdTabs)
        {
            FormChangeMode.MODE_TECCOMPONENT modeTECComponent;

            for (int i = 0; i < arIdTabs.Count; i++) {
                //получение типа вкладки
                modeTECComponent = TECComponentBase.Mode (arIdTabs [i]);
                //проверка состояния фильтра для вкладки
                if (arrayCheckBoxModeTECComponent [(int)modeTECComponent].Checked == true) {
                    if (Equals (m_listTEC, null) == false)
                        foreach (StatisticCommon.TEC t in m_listTEC) {
                            if (t.m_id == arIdTabs [i]
                                && (modeTECComponent == FormChangeMode.MODE_TECCOMPONENT.TEC)) {
                                //добавление ТЭЦ в ListBox
                                listBoxTabVisible.Items.Add (t.name_shr);

                                break;
                            } else
                                ;

                            if (t.list_TECComponents.Count > 0)
                                foreach (StatisticCommon.TECComponent g in t.list_TECComponents)
                                    if (g.m_id == arIdTabs [i])
                                        //Добавление вкладки в ListBox
                                        listBoxTabVisible.Items.Add ($"{t.name_shr} - {g.name_shr}");
                                    else
                                        ;
                            else
                                ;
                        } else
                        ;
                }
            }
        }

        #endregion

        public override void UpdateGraphicsCurrent (int type)
        {
            //??? ничегно не делаем
        }

        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }

            set
            {
                base.ForeColor = value;

                getTypedControls (this, new Type [] { typeof (DataGridView) }).Cast<DataGridView> ().ToList ().ForEach (dgv => {
                    dgv.DefaultCellStyle.ForeColor = value;
                });

                if (Equals (listBoxTabVisible, null) == false)
                    listBoxTabVisible.ForeColor = value;
                else
                    ;
            }
        }

        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }

            set
            {
                base.BackColor = value;

                getTypedControls (this, new Type [] { typeof (DataGridView) }).Cast<DataGridView> ().ToList ().ForEach (dgv => {
                    dgv.DefaultCellStyle.BackColor = BackColor == SystemColors.Control ? SystemColors.Window : BackColor;
                });

                if (Equals (listBoxTabVisible, null) == false)
                    listBoxTabVisible.BackColor = BackColor == SystemColors.Control ? SystemColors.Window : BackColor;
                else
                    ;
            }
        }

        public class DataGridView_LogMessageCounter : DataGridView
        {
            /// <summary>
            /// Перечисление типов DataGridView
            /// </summary>
            public enum TYPE { UNKNOWN = -1, WITHOUT_CHECKBOX, WITH_CHECKBOX, COUNT }

            /// <summary>
            /// Тип экземпляра DataGridView
            /// </summary>
            private TYPE _type;

            private void InitializeComponent()
            {
                // 
                // dgvFilterTypeMessage
                // 
                this.AllowUserToAddRows = false;
                this.AllowUserToDeleteRows = false;
                this.ReadOnly = true;
                this.Dock = DockStyle.Fill;
                this.ColumnHeadersVisible = false;
                this.MultiSelect = false;
                this.RowHeadersVisible = false;
                this.RowTemplate.Height = 18;
                this.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            }

            public DataGridView_LogMessageCounter(TYPE type)
                : base()
            {
                DataGridViewCheckBoxColumn check_state = new DataGridViewCheckBoxColumn();
                DataGridViewColumn type_message = new DataGridViewTextBoxColumn();
                DataGridViewColumn count_message = new DataGridViewTextBoxColumn();
                
                _type = type;

                //инициализация компонентов
                InitializeComponent ();

                if (!(_type == TYPE.UNKNOWN)) {
                    if (_type == TYPE.WITH_CHECKBOX)
                        this.Columns.Add (check_state);
                    else
                        ;

                    this.Columns.Add (type_message);
                    this.Columns.Add (count_message);
                } else
                    ;
                
                // 
                // dataGridViewCheckBoxColumnTypeMessageUse
                //
                check_state.Frozen = true;
                check_state.Name = "dataGridViewCheckBoxColumnTypeMessageUse";
                check_state.Resizable = System.Windows.Forms.DataGridViewTriState.False;
                check_state.Width = 25;
                // 
                // dataGridViewTextBoxColumnTypeMessageDesc
                //
                type_message.Frozen = true;
                type_message.Name = "dataGridViewTextBoxColumnTypeMessageDesc";
                type_message.Resizable = System.Windows.Forms.DataGridViewTriState.False;
                type_message.Width = 105;
                // 
                // dataGridViewTextBoxColumnCounter
                //
                count_message.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                count_message.HeaderText = "Count";
                count_message.Name = "ColumnCount";
                count_message.Width = 20;
                //заполнение DataGridView типами сообщений
                initialize (LogParse.s_ID_LOGMESSAGES, LogParse.s_DESC_LOGMESSAGE);
            }

            /// <summary>
            /// Получение индекса последнего столбца
            /// </summary>
            private int lastIndex { get { return this.ColumnCount - 1; } }

            /// <summary>
            /// Обновление списка со статистикой сообщений 
            /// </summary>
            /// <param name="table_statMessage">таблица с количествами сообщений каждого типа</param>
            public void Fill(DataTable table_statMessage)
            {
                string strCount = string.Empty;

                if (table_statMessage.Rows.Count > 0) {
                    foreach (DataGridViewRow r in this.Rows) {
                        try {
                            var counter = (from DataRow dataRow in table_statMessage.Rows
                                where (int)dataRow["ID_LOGMSG"] == (int)r.Tag
                                select new { Id = dataRow["ID_LOGMSG"], Count = dataRow["COUNT"] });

                            if (counter.Count() > 0)
                                if (counter.Count() == 1)
                                    strCount = counter.ToArray()[0].Count.ToString();
                                else
                                    throw new InvalidOperationException($"PanelAnalyzer.DataGridView_LogMessageCounter::Fill () - для типа сообщений <{(int)r.Tag}> более, чем 1({counter.Count()}) запись...");
                            else
                                strCount = 0.ToString();
                        } catch (Exception e) {
                            Logging.Logg().Exception(e, $"PanelAnalyzer.DataGridView_LogMessageCounter::Fill () - для типа сообщений <{(int)r.Tag}> ..."
                                , Logging.INDEX_MESSAGE.NOT_SET);
                        } finally {
                            r.Cells[lastIndex].Value = strCount;
                        }
                    }
                } else
                    ;
            }

            /// <summary>
            /// Возвращает словарь с парами значений: идентификатор типа сообщений, признак для учета в фильтре
            /// </summary>
            /// <return>Возвращает список состояния CheckBox'ов</return>
            public Dictionary<int, bool> Checked()
            {
                Dictionary<int, bool> dictRes = new Dictionary<int, bool>();

                if (_type == TYPE.WITH_CHECKBOX)
                    for (int i = 0; i < this.Rows.Count; i++)
                    //добавление состояния CheckBox'а в список
                        dictRes.Add((int)Rows[i].Tag, (bool)this.Rows[i].Cells[lastIndex - 2].Value);
                else
                    ;

                return dictRes;
            }

            /// <summary>
            /// Заполнение DataGridView типами сообщений
            /// </summary>
            /// <param name="strTypeMessages">Массив с типами сообщений</param>
            protected void initialize(int []idTypeMessages,  string[] strTypeMessages)
            {
                this.Rows.Add(strTypeMessages.Length);

                for (int i = 0; i < strTypeMessages.Length; i++) {
                    this.Rows[i].Tag = idTypeMessages[i];

                    if (_type == TYPE.WITH_CHECKBOX)
                        this.Rows [i].Cells [lastIndex - 2].Value = true;
                    else
                        ;

                    this.Rows[i].Cells[lastIndex - 1].Value = strTypeMessages[i];
                    this.Rows[i].Cells[lastIndex].Value = 0;
                }
            }

            /// <summary>
            /// Обнуление статистики типов сообщений в DataGridView 
            /// </summary>
            public void ClearValues()
            {
                foreach (DataGridViewRow r in this.Rows)
                    r.Cells [lastIndex].Value = 0; 
            }
        }
    }

    public static class DataGridViewExtensions
    {
        /// <summary>
        /// Метод заполнения DataGridView данными
        /// </summary>
        /// <param name="ctrl">Объект DataGridView для заполнения</param>
        /// <param name="src">Источник заполнения</param>
        /// <param name="nameField">Имя столбца с данными</param>
        /// <param name="run">Для получения установить = 0</param>
        /// <param name="checkDefault">CheckBox по умолчанию вкл/выкл</param>
        public static void Fill (this DataGridView ctrl, DataTable src, string nameField, int run, bool checkDefault = false)
        {
            bool bCheckedItem = false;
            object tagNewRow = null;
            DataGridViewRow newRow;
            int iNewRow = -1
                , indxFieldRole = -1, indxFieldId = -1;

            if (run == 0) {
                bCheckedItem = checkDefault;
                ctrl.Rows.Clear ();

                if (src.Rows.Count > 0) {
                    indxFieldRole = src.Columns.IndexOf ("ID_ROLE");
                    indxFieldId = src.Columns.IndexOf ("ID");

                    //ctrl.RowsAdded += delegate (object sender, DataGridViewRowsAddedEventArgs e) {
                    //    for (int i = 0; i < e.RowCount; i++) {
                    //        (sender as DataGridView).Rows[i + e.RowIndex].Tag = 
                    //    }
                    //};

                    foreach (DataRow r in src.Rows) {
                        try {
                            newRow = ctrl.RowTemplate.Clone () as DataGridViewRow;

                            if ((!(indxFieldRole < 0))
                                && (!(indxFieldId < 0)))
                                tagNewRow = new HStatisticUser {
                                    Role = (HStatisticUsers.ID_ROLES)int.Parse (r [indxFieldRole].ToString ())
                                    , Id = int.Parse (r [indxFieldId].ToString ())
                                };
                            else if (!(indxFieldId < 0))
                                tagNewRow = int.Parse (r [indxFieldId].ToString ());
                            else
                            // такой вариант не рассматривается
                                ;

                            newRow.Tag = tagNewRow;
                            iNewRow = ctrl.Rows.Add (newRow);
                            ctrl.Rows[iNewRow].SetValues (new object [] { bCheckedItem, r [nameField].ToString () });
                        } catch (Exception e) {
                            Logging.Logg().Exception(e, $"DataGridViewExcensions::Fill () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    }
                } else
                    ;
            } else
                ;
        }

        public static void Update(this DataGridView ctrl, IList<int>tags, IList<bool> arbActives)
        {
            int indx = -1;
            bool active = false;

            foreach (DataGridViewRow r in ctrl.Rows) {
                //TODO: проверять 'm_bThreadTimerCheckedAllowed' для возможности прерывания
                try {
                    if (Equals (r.Tag, null) == false) {
                        indx = tags.IndexOf (r.Tag.GetType ().Equals (typeof (HStatisticUser)) == true ? ((HStatisticUser)r.Tag).Id : (int)r.Tag);

                        if (!(indx < 0))
                            active = arbActives [indx];
                        else
                            active = false;

                        r.Cells [0].Value = active;
                    } else
                        Logging.Logg().Error($@"StatisticAnalyzer.DataGridViewExtensions::Update () - для строки не установлен идентификатор(Tag)...", Logging.INDEX_MESSAGE.NOT_SET);
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, $@"StatisticAnalyzer.DataGridViewExtensions::Update () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
        }
    }
}
