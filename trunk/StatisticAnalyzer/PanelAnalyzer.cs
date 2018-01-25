//using HClassLibrary;
using StatisticCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common; //DbConnection
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using ASUTP.Core;
using ASUTP.Database;
using ASUTP;
using ASUTP.Network;
using ASUTP.Helper;
using StatisticAnalyzer;

namespace StatisticAnalyzer
{
    public abstract partial class PanelAnalyzer : PanelStatistic
    {
        protected enum StatesMachine {
            ServerTime
            , ProcCheckedState
            , ProcCheckedFilter
            , ListMessageToUserByDate
            , ListDateByUser
            , CounterToTypeByFilter
        }

        protected interface ILoggingReadHandler : ASUTP.Helper.IHHandler
        {
            event Action<REQUEST, DataTable> EventCommandCompleted;
        }

        protected abstract class REQUEST
        {
            public enum STATE {
                Unknown, Ready, Ok, Error
            }

            public StatesMachine Key;

            public object [] Args;

            public STATE State;

            public abstract string Query
            {
                get;
            }

            public REQUEST (StatesMachine key, object arg)
            {
                Key = key;

                if (Equals (arg, null) == false)
                    if (arg is Array) {
                        Args = new object [(arg as object []).Length];

                        for (int i = 0; i < Args.Length; i++)
                            Args [i] = (arg as object []) [i];
                    } else
                        Args = new object [] { arg };
                else
                    Args = new object [] { };

                State = STATE.Ready;

                Logging.Logg ().Debug ($"PanelAnalyzer.HLoggingReadHandlerDb.REQUEST::ctor (Key={Key}) - новое args:{toString ()}...", Logging.INDEX_MESSAGE.NOT_SET);
            }

            public bool IsEmpty
            {
                get
                {
                    return Equals (Args, null); //== true ? true : Args.Length == 0;
                }
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
                        strRes = $"IdUser={Args [0]}, Type={Args [1]}, Period=[{(DateTime)Args [2]}, {(DateTime)Args [3]}]";
                        break;
                    case StatesMachine.ListDateByUser:
                        strRes = $"IdUser={Args [0]}";
                        break;
                    case StatesMachine.CounterToTypeByFilter:
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
                case StatesMachine.CounterToTypeByFilter:
                    _handlers [req.Key] (req, tableRes);
                    break;
                default:
                    throw new InvalidOperationException ("PanelAnalyzer::loggingReadHandler_onCommandCompleted () - неизвестный тип запроса...");
                    break;
            }
        }

        protected abstract void handlerCommandCounterToTypeByFilter (PanelAnalyzer.REQUEST req, DataTable tableRes);

        protected abstract void handlerCommandListMessageToUserByDate (PanelAnalyzer.REQUEST req, DataTable tableLogging);

        protected abstract void handlerCommandListDateByUser(PanelAnalyzer.REQUEST req, DataTable tableRes);

        protected abstract void handlerCommandProcChecked (PanelAnalyzer.REQUEST req, DataTable tableRes);

        #region Design

        private System.Windows.Forms.TabPage tabPageLogging;
        private System.Windows.Forms.TabPage tabPageTabes;
        private TableLayoutPanel panelTabPageTabes;
        private TableLayoutPanel panelTabPageLogging;

        protected System.Windows.Forms.CheckBox[] arrayCheckBoxModeTECComponent;
        protected System.Windows.Forms.ListBox listBoxTabVisible;

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
            this.dgvFilterRoleView = new DataGridView();
            this.dgvUserStatistic = new DataGridView();

            System.Windows.Forms.Label labelFilterActives = new System.Windows.Forms.Label();
            this.dgvFilterActiveView = new System.Windows.Forms.DataGridView();

            System.Windows.Forms.Label labelFilterRoles = new System.Windows.Forms.Label();
            this.dgvFilterRoleStatistic = new System.Windows.Forms.DataGridView();

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
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilterRoleStatistic)).BeginInit();
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
            this.dgvFilterRoleStatistic.AllowUserToAddRows = false;
            this.dgvFilterRoleStatistic.AllowUserToDeleteRows = false;
            this.dgvFilterRoleStatistic.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFilterRoleStatistic.ColumnHeadersVisible = false;
            this.dgvFilterRoleStatistic.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                new DataGridViewCheckBoxColumn (),
                new DataGridViewTextBoxColumn ()});
            //this.dgvFilterRoles.Location = new System.Drawing.Point(12, 88);
            this.dgvFilterRoleStatistic.Dock = DockStyle.Fill;
            this.dgvFilterRoleStatistic.MultiSelect = false;
            this.dgvFilterRoleStatistic.Name = "dgvFilterRoles";
            this.dgvFilterRoleStatistic.ReadOnly = true;
            this.dgvFilterRoleStatistic.RowHeadersVisible = false;
            this.dgvFilterRoleStatistic.RowTemplate.Height = 18;
            this.dgvFilterRoleStatistic.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFilterRoleStatistic.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //this.dgvFilterRoles.Size = new System.Drawing.Size(190, 111);
            this.dgvFilterRoleStatistic.TabIndex = 9;
            this.dgvFilterRoleStatistic.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvFilterRoleStatistic_CellClick);
            // 
            // dataGridViewRolesCheckBoxColumnUse
            // 
            i = 0;
            this.dgvFilterRoleStatistic.Columns[i].Frozen = true;
            this.dgvFilterRoleStatistic.Columns[i].HeaderText = "Use";
            this.dgvFilterRoleStatistic.Columns[i].Name = "dataGridViewRolesCheckBoxColumnUse";
            this.dgvFilterRoleStatistic.Columns[i].ReadOnly = true;
            this.dgvFilterRoleStatistic.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFilterRoleStatistic.Columns[i].Width = 25;
            // 
            // dataGridViewRolesTextBoxColumnDesc
            // 
            i = 1;
            //this.dgvFilterRoles.Columns[i].Frozen = true;
            this.dgvFilterRoleStatistic.Columns[i].HeaderText = "Desc";
            this.dgvFilterRoleStatistic.Columns[i].Name = "dataGridViewRolesTextBoxColumnDesc";
            this.dgvFilterRoleStatistic.Columns[i].ReadOnly = true;
            this.dgvFilterRoleStatistic.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFilterRoleStatistic.Columns[i].Width = 145;
            this.dgvFilterRoleStatistic.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
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
            this.dgvUserStatistic.SelectionChanged += new EventHandler(this.dgvUserStatistic_SelectionChanged);

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
            this.dgvFilterRoleView.Name = "dgvRole";
            this.dgvFilterRoleView.Dock = DockStyle.Fill;
            //this.listUser.TabIndex = 4;

            this.dgvFilterRoleView.AllowUserToAddRows = false;
            this.dgvFilterRoleView.AllowUserToDeleteRows = false;
            this.dgvFilterRoleView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFilterRoleView.ColumnHeadersVisible = false;
            this.dgvFilterRoleView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                new DataGridViewCheckBoxColumn (),
                new DataGridViewTextBoxColumn ()});
            this.dgvFilterRoleView.MultiSelect = false;
            this.dgvFilterRoleView.ReadOnly = true;
            this.dgvFilterRoleView.RowHeadersVisible = false;
            this.dgvFilterRoleView.RowTemplate.Height = 18;
            this.dgvFilterRoleView.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFilterRoleView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //this.dgvFilterRoles.Size = new System.Drawing.Size(190, 111);
            this.dgvFilterRoleView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvFilterRoleView_CellClick);

            i = 0;
            this.dgvFilterRoleView.Columns[i].Frozen = true;
            this.dgvFilterRoleView.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFilterRoleView.Columns[i].Width = 20;
            // 
            // dataGridViewTextBoxColumnCounter
            // 
            i = 1;
            this.dgvFilterRoleView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dgvFilterRoleView.Columns[i].Width = 140;

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
            panelUser.Controls.Add(this.dgvFilterRoleStatistic, 0, 5); panelUser.SetColumnSpan(this.dgvFilterRoleStatistic, 3); panelUser.SetRowSpan(this.dgvFilterRoleStatistic, 6);

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
            panelMessage.Controls.Add(this.dgvFilterRoleView, 0, 7); this.SetColumnSpan(this.dgvFilterRoleView, 1); this.SetRowSpan(this.dgvFilterRoleView, 7);

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
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilterRoleStatistic)).EndInit();
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
        /// <summary>
        /// Таблицы пользователей для лога сообщений и таблица с ролями 
        /// </summary>
        protected DataTable m_tableUsers, m_tableUsers_unfiltered,
            m_tableRoles;
        /// <summary>
        /// Таблицы пользователей для статистики сообщений 
        /// </summary>
        protected DataTable m_tableUsers_stat,
            m_tableUsers_stat_unfiltered;
        /// <summary>
        /// Массив CheckBox'ов для фильтра типов вкладок
        /// </summary>
        CheckBox[] m_arCheckBoxMode;
        /// <summary>
        /// Номер предыдущей строки Даты/Времени 
        /// </summary>
        int m_prevDatetimeRowIndex;
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
                , { StatesMachine.CounterToTypeByFilter, handlerCommandCounterToTypeByFilter }
            };

            m_loggingReadHandler = newLoggingRead ();
            m_loggingReadHandler.EventCommandCompleted += new Action<REQUEST, DataTable> (loggingReadHandler_onCommandCompleted);
            m_LogParse = newLogParse();

            InitializeComponent();

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
                m_LogParse.Exit = LogParseExit;
            else {
                string strErr = @"Не создан объект разбора сообщений (класс 'LogParse')...";
                throw new Exception(strErr);
            }
        }

        protected abstract ILoggingReadHandler newLoggingRead ();

        /// <summary>
        /// Метод для разбора строки с лог сообщениями
        /// </summary>
        /// <returns>Возвращает строку</returns>
        protected abstract LogParse newLogParse();

        #region Объявление абстрактных методов

        /// <summary>
        /// Проверка запуска пользователем ПО
        /// </summary>
        /// <return>Массив занчений активности каждого пользователя</return>
        protected abstract bool [] procChecked();

        /// <summary>
        /// Запись значений активности в CheckBox на DataGridView с пользователями
        /// </summary>
        /// <param name="obj">???</param>
        protected abstract void procChecked(object obj);

        /// <summary>
        /// ???
        /// </summary>
        protected abstract void disconnect();

        /// <summary>
        /// Начало разбора строки с логом
        /// </summary>
        /// <param name="par">Строка для разбора</param>
        protected abstract void startLogParse(string par);

        /// <summary>
        /// Выборка лог-сообщений по параметрам
        /// </summary>
        /// <param name="id_user">Идентификатор пользователя</param>
        /// <param name="type">Тип сообщений</param>
        /// <param name="beg">Начало периода</param>
        /// <param name="end">Окончание периода</param>
        /// <param name="funcResult">Функция обратного вызова с массивом сообщений</param>
        protected abstract void selectLogMessage(int id_user, string type, DateTime beg, DateTime end, Action<DataRow[]> funcResult);

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

            base.Start();

            DbConnection connConfigDB = DbSources.Sources ().GetConnection (DbTSQLConfigDatabase.DbConfig ().ListenerId, out err);

            if ((!(connConfigDB == null))
                && (err == 0)) {
                HStatisticUsers.GetRoles (ref connConfigDB, string.Empty, string.Empty, out m_tableRoles, out err);
                dgvFilterRoleView.Fill (m_tableRoles, @"DESCRIPTION", err, true);
                dgvFilterRoleStatistic.Fill (m_tableRoles, @"DESCRIPTION", err, true);

                HStatisticUsers.GetUsers (ref connConfigDB, string.Empty, c_list_sorted, out m_tableUsers, out err);
                dgvUserStatistic.SelectionChanged -= dgvUserStatistic_SelectionChanged;
                dgvUserStatistic.Fill (m_tableUsers, @"DESCRIPTION", err, true);
                dgvUserStatistic.SelectionChanged += dgvUserStatistic_SelectionChanged;
                dgvUserStatistic.Rows [0].Selected = true;

                dgvUserView.Fill (m_tableUsers, @"DESCRIPTION", err);

                m_tableUsers_stat = m_tableUsers.Copy ();
                m_tableUsers_unfiltered = m_tableUsers.Copy ();
                m_tableUsers_stat_unfiltered = m_tableUsers.Copy ();
            } else
                throw new Exception("PanelAnalyzer::Start () - нет соединения с БД конфигурации...");

            m_bThreadTimerCheckedAllowed = true;

            m_timerProcChecked =
                new System.Threading.Timer(new TimerCallback(procChecked), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite)
                //new System.Windows.Forms.Timer()
                ;
            ////Вариант №1
            //m_timerChecked.Interval = MSEC_TIMERCHECKED_STANDARD;
            //m_timerChecked.Tick += new EventHandler(ProcChecked);
            //m_timerChecked.Start ();
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

            activateTimerChecked (activated);

            return bRes;
        }

        private void activateTimerChecked (bool activated)
        {
            m_timerProcChecked?.Change (activated == true ? 0 : System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
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
        protected void tabLoggingClearDatetimeStart() { dgvListDateView.Rows.Clear(); }

        /// <summary>
        /// Очистка таблицы с лог-сообщениями
        /// </summary>
        /// <param name="bClearCounter"></param>
        protected void tabLoggingClearText(bool bClearCounter)
        {
            m_LogParse.Clear();
            /*textBoxLog.Clear();*/
            dgvMessageView.Rows.Clear();
            //if (bClearCounter == true)
            //    for (int i = 0; i < dgvFilterTypeMessage.Rows.Count; i++)
            //        dgvFilterTypeMessage.Rows[(int)i].Cells[2].Value = 0;
            //else
            //    ;
        }

        /// <summary>
        /// Метод для перемещения фокуса в таблице с лог-сообщениями на первую строку.
        /// </summary>
        void TabLoggingPositionText()
        {
            /*
            textBoxLog.Select(0, 0);
            textBoxLog.ScrollToCaret();
            */

            if (dgvMessageView.Rows.Count > 0)
                dgvMessageView.FirstDisplayedScrollingRowIndex = 0;
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
        /// <param name="stop_date">Окончание периода</param>
        /// <param name="users">Список пользователей</param>
        protected abstract void updateCounter(DATAGRIDVIEW_LOGCOUNTER tag, DateTime start_date, DateTime end_date, string users);

        /// <summary>
        /// Метод заполнения таблиц с типами сообщений и лог-сообщениями
        /// </summary>
        void TabLoggingAppendText(object data)
        {
            HTabLoggingAppendTextPars tlatPars;
            DateTime start_date
                , end_date;

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
                    //создание списка с идентификаторами типов сообщений
                    List<int> listIdTypeMessages = LogParse.s_ID_LOGMESSAGES.ToList();

                    //Получение массива состояний CheckBox'ов на DataGridView с типами сообщений
                    bool[] arCheckedTypeMessages = new bool[dgvTypeToView.Rows.Count];
                    List<bool> check = new List<bool>();
                    check = dgvTypeToView.Checked();
                    for (int i = 0; i < dgvTypeToView.Rows.Count; i++)
                    {
                        arCheckedTypeMessages[i] = check[i];
                    }
                    //Преобразование строки в массив строк с сообщениями
                    string[] messages = tlatPars.rows.Split(new string[] { s_chDelimeters[(int)INDEX_DELIMETER.ROW] }, StringSplitOptions.None);
                    string[] parts;
                    int indxTypeMessage = -1;

                    //Помещение массива строк с сообщениями в DataGridView с сообщениями
                    foreach (string text in messages) {
                        parts = text.Split(new string[] { s_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None);
                        indxTypeMessage = listIdTypeMessages.IndexOf(Int32.Parse(parts[1]));

                        //Фильтрация сообщений в зависимости от включенных CheckBox'ов в DataGridView с типами сообщений
                        if (arCheckedTypeMessages[indxTypeMessage] == true)
                            dgvMessageView.Rows.Add(parts);
                        else
                            ;
                    }

                    start_date = DateTime.Parse(dgvListDateView.Rows[m_prevDatetimeRowIndex].Cells[1].Value.ToString());
                    end_date = start_date.AddDays(1);
                    check.Clear();
                    updateCounter(DATAGRIDVIEW_LOGCOUNTER.TYPE_TO_VIEW, start_date, end_date, get_users(m_tableUsers, dgvUserView, false));
                }

                delegateReportClear(true);
            } catch (Exception e) {
                Logging.Logg().Exception(e, "PanelAnalyzer:TabLoggingAppendText () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        /// <summary>
        /// Метод заполнения таблицы с датой для фильтрации лог-сообщений
        /// </summary>
        void TabLoggingAppendDatetimeStart(string rows)
        {
            string[] strDatetimeStartRows = rows.Split(new string[] { s_chDelimeters[(int)INDEX_DELIMETER.ROW] }, StringSplitOptions.None);

            bool bRowChecked = false;

            foreach (string row in strDatetimeStartRows)
            {
                bRowChecked = bool.Parse(row.Split(new string[] { s_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None)[0]);

                if (bRowChecked == true)
                {
                    dgvListDateView.SelectionChanged += dgvDatetimeStart_SelectionChanged;
                    m_prevDatetimeRowIndex = dgvListDateView.Rows.Count;
                }
                else
                    ;

                dgvListDateView.Rows.Add(new object[] { bRowChecked, row.Split(new string[] { s_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None)[1] });
            }

            if (bRowChecked == true)
            {
                dgvListDateView.Rows[dgvListDateView.Rows.Count - 1].Selected = bRowChecked;
                dgvListDateView.FirstDisplayedScrollingRowIndex = dgvListDateView.Rows.Count - 1;
            }
            else
                ;
        }

        /// <summary>
        /// Получение списка дата/время запуска приложения
        /// </summary>
        public virtual void LogParseExit()
        {
            int i = -1;
            DataRow[] rows = new DataRow[] { };

            //DelegateObjectFunc delegateAppendText = TabLoggingAppendText;
            //DelegateStringFunc delegateAppendDatetimeStart = TabLoggingAppendDatetimeStart;
            BeginInvoke(new DelegateFunc(tabLoggingClearDatetimeStart));
            BeginInvoke(new DelegateBoolFunc(tabLoggingClearText), true);

            //Получение списка дата/время запуска приложения
            bool rowChecked = false;
            string strDatetimeStart = string.Empty;
            //Нюанс для 'LogParse_File' и 'LogParse_DB' - "0" индекс в массиве типов сообщений (зарезервирован для "СТАРТ")
            //...для 'LogParse_File': m_tblLog содержит ВСЕ записи в файле
            //...для 'LogParse_DB': m_tblLog содержит ТОЛЛЬКО записи из БД с датами, за которые найдено хотя бы одно сообщение
            //      тип сообщения "СТАРТ" устанавливается "программно" (метод 'LogParse_DB::Thread_Proc')
            //0 - индекс в массиве идентификаторов зарезервирован для сообщений типа "СТАРТ"
            rows = m_LogParse.ByDate(@"true" + s_chDelimeters[(int)INDEX_DELIMETER.ROW] + ((int)LogParse.INDEX_START_MESSAGE).ToString(), DateTime.MaxValue, DateTime.MaxValue);
            for (i = 0; i < rows.Length; i++)
            {
                if (i == (rows.Length - 1))
                    rowChecked = true;
                else
                    ;

                strDatetimeStart += rowChecked.ToString()
                        + s_chDelimeters[(int)INDEX_DELIMETER.PART]
                        + rows[i]["DATE_TIME"].ToString()
                        + s_chDelimeters[(int)INDEX_DELIMETER.ROW];
            }

            if (strDatetimeStart.Length > 0)
            {
                strDatetimeStart = strDatetimeStart.Substring(0, strDatetimeStart.Length - s_chDelimeters[(int)INDEX_DELIMETER.ROW].Length);
                BeginInvoke(new DelegateStringFunc(TabLoggingAppendDatetimeStart), strDatetimeStart);
            }
            else
            {
                //dgvFilterTypeMessage.UpdateCounter(m_idListenerLoggingDB, DateTime.Today, DateTime.Today.AddDays(1), " ");
                updateCounter(DATAGRIDVIEW_LOGCOUNTER.TYPE_TO_VIEW, DateTime.Today, DateTime.Today.AddDays(1), "");
            }
        }

        /// <summary>
        /// Обновление списка лог-сообщений
        /// </summary>
        private void updatedgvLogMessages(object data)
        {
            DataRow[] rows = new DataRow[] { };
            //получение количества строк лог-сообщений
            rows = m_LogParse.Select();

            filldgvLogMessages(rows);
        }

        /// <summary>
        /// Формирование списка лог-сообщений
        /// </summary>
        protected void filldgvLogMessages(object data)
        {
            //DateTime [] arDT = (DateTime [])data;
            object[] pars = (object[])data;
            DataRow[] rows = new DataRow[] { };

            // 0 - тип сообщение
            // 1 - дата/время - начало
            // 2 - дата/время - окончание
            // 3 - 
            // 4 - идентификатор пользователя
            selectLogMessage((int)pars[4], ((string)pars[0]), ((DateTime)pars[1]), ((DateTime)pars[1]).AddDays(1), null);
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
                    if (((i % 1000) == 0) && (strRowsTodgvLogMessages.Length > s_chDelimeters[(int)INDEX_DELIMETER.ROW].Length))
                    {
                        //Сбор строки из лог-сообщений
                        strRowsTodgvLogMessages = strRowsTodgvLogMessages.Substring(0, strRowsTodgvLogMessages.Length - s_chDelimeters[(int)INDEX_DELIMETER.ROW].Length);
                        //Асинхронное выполнение метода заполнения DataGridView с лог-сообщениями
                        this.BeginInvoke(delegateAppendText, new HTabLoggingAppendTextPars(strRowsTodgvLogMessages));
                        //this.BeginInvoke(new DelegateObjectFunc(TabLoggingAppendText), strRowsTodgvLogMessages);
                        strRowsTodgvLogMessages = string.Empty;
                    }
                    else

                        try
                        {
                            strRowsTodgvLogMessages += getTabLoggingTextRow(rows[i]) + s_chDelimeters[(int)INDEX_DELIMETER.ROW];
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, "PanelAnalyzer:filldgvLogMessages () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                }

                if (strRowsTodgvLogMessages.Length > s_chDelimeters[(int)INDEX_DELIMETER.ROW].Length)
                {
                    //Остаток...                    
                    strRowsTodgvLogMessages = strRowsTodgvLogMessages.Substring(0, strRowsTodgvLogMessages.Length - s_chDelimeters[(int)INDEX_DELIMETER.ROW].Length);
                    this.BeginInvoke(delegateAppendText, new HTabLoggingAppendTextPars(strRowsTodgvLogMessages));
                    //this.BeginInvoke(new DelegateObjectFunc(TabLoggingAppendText), strRowsTodgvLogMessages);
                    strRowsTodgvLogMessages = string.Empty;
                }
                else
                    ;
            }
            else
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
        private void startFilldgvLogMessages(bool bClearTypeMessageCounter)
        {
            tabLoggingClearText(bClearTypeMessageCounter);

            DateTime dtBegin = DateTime.Parse(dgvListDateView.Rows[m_prevDatetimeRowIndex].Cells[1].Value.ToString())
                , dtEnd = DateTime.MaxValue;
            if ((m_prevDatetimeRowIndex + 1) < dgvListDateView.Rows.Count)
                dtEnd = DateTime.Parse(dgvListDateView.Rows[m_prevDatetimeRowIndex + 1].Cells[1].Value.ToString());
            else
                ;

            Thread threadFilldgvLogMessages = new Thread(new ParameterizedThreadStart(filldgvLogMessages));
            threadFilldgvLogMessages.IsBackground = true;
            threadFilldgvLogMessages.Start(new object[] { getIndexTypeMessages(), dtBegin, dtEnd, bClearTypeMessageCounter, dgvUserView.SelectedRows [0].Tag });
        }
        
        /// <summary>
        /// Старт обновления DataGridView с лог-сообщениями 
        /// </summary>
        /// <param name="bClearTypeMessageCounter">Флаг очистки DataGridView с лог-сообщениями </param>
        private void startUpdatedgvLogMessages(bool bClearTypeMessageCounter)
        {
            tabLoggingClearText(bClearTypeMessageCounter);

            Thread threadFilldgvLogMessages = new Thread(new ParameterizedThreadStart(updatedgvLogMessages));
            threadFilldgvLogMessages.IsBackground = true;
            threadFilldgvLogMessages.Start();
        }

        /// <summary>
        /// Класс для работы со строкой лог сообщений
        /// </summary>
        protected abstract class LogParse
        {
            public const int INDEX_START_MESSAGE = 0;

            /// <summary>
            /// Массив с идентификаторами типов сообщений
            /// </summary>
            public static int[] s_ID_LOGMESSAGES;

            /// <summary>
            /// Список типов сообщений
            /// </summary>
            public static string[] s_DESC_LOGMESSAGE;

            private Thread m_thread;
            protected DataTable m_tableLog;
            Semaphore m_semAllowed;

            public DelegateFunc Exit;

            public LogParse()
            {
                m_semAllowed = new Semaphore(1, 1);

                m_tableLog = new DataTable("ContentLog");
                DataColumn[] cols = new DataColumn[] {
                    new DataColumn("DATE_TIME", typeof(DateTime)),
                    new DataColumn("TYPE", typeof(Int32)),
                    new DataColumn ("MESSAGE", typeof (string))
                };
                m_tableLog.Columns.AddRange(cols);

                m_thread = null;
            }

            /// <summary>
            /// Инициализация потока разбора лог-файла
            /// </summary>
            public void Start(object param)
            {
                m_semAllowed.WaitOne();

                m_thread = new Thread(new ParameterizedThreadStart(thread_Proc));
                m_thread.IsBackground = true;
                m_thread.Name = "Разбор лог-файла";

                m_thread.CurrentCulture =
                m_thread.CurrentUICulture =
                    ProgramBase.ss_MainCultureInfo;

                m_tableLog.Clear();

                m_thread.Start(param);
            }

            /// <summary>
            /// Остановка потока разбора лог-файла
            /// </summary>
            public void Stop()
            {
                //if (m_bAllowed == false)
                //    return;
                //else
                //    ;

                bool joined = false;
                if ((!(m_thread == null)))
                {
                    if (m_thread.IsAlive == true)
                    {
                        //m_bAllowed = false;
                        joined = m_thread.Join(6666);
                        if (joined == false)
                            m_thread.Abort();
                        else
                            ;
                    }
                    else
                        ;

                    try { m_semAllowed.Release(); }
                    catch (Exception e)
                    {
                        Console.WriteLine("LogParse::Stop () - m_semAllowed.Release() - поток не был запущен или штатно завршился");
                    }
                }
                else
                    ;

                //m_bAllowed = true;
            }

            /// <summary>
            /// Метод для разбора лог-сообщений в потоке
            /// </summary>
            protected virtual void thread_Proc(object data)
            {
                Console.WriteLine("Окончание обработки лог-файла. Обработано строк: {0}", (int)data);
               
                Exit();
            }

            /// <summary>
            /// Очистка
            /// </summary>
            public void Clear()
            {
                //m_tableLog.Clear();
            }

            /// <summary>
            /// Добавление типов сообщений
            /// </summary>
            /// <param name="nameFieldTypeMessage">Наименование колонки типов сообщений</param>
            /// <param name="strIndxType">Стартовы индекс</param>
            /// <returns>Строка для условия выборки сообщений по их типам</returns>
            protected string addingWhereTypeMessage(string nameFieldTypeMessage, string strIndxType)
            {
                string strRes = string.Empty;

                //if (!(indxType < 0))
                if (strIndxType.Equals(string.Empty) == false)
                {
                    List<int> listIndxType;
                    //listIndxType = strIndxType.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.ROW] }, StringSplitOptions.None)[1].Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None).ToList().ConvertAll<int>(new Converter<string, int>(delegate(string strIn) { return Int32.Parse(strIn); }));
                    string[] pars = strIndxType.Split(new string[] { s_chDelimeters[(int)INDEX_DELIMETER.ROW] }, StringSplitOptions.None);

                    if (pars.Length == 2)
                    {
                        bool bUse = false;
                        //if ((pars[0].Equals(true.ToString()) == true) || (pars[0].Equals(false.ToString()) == true))
                        if (bool.TryParse(pars[0], out bUse) == true)
                        {
                            if (pars[1].Length > 0)
                            {
                                listIndxType = pars[1].Split(new string[] { s_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None).ToList().ConvertAll<int>(new Converter<string, int>(delegate(string strIn) { return Int32.Parse(strIn); }));

                                strRes += nameFieldTypeMessage + @" ";

                                if (bUse == false)
                                    strRes += @"NOT ";
                                else
                                    ;

                                strRes += @"IN (";

                                foreach (int indx in listIndxType)
                                    strRes += s_ID_LOGMESSAGES[indx] + @",";

                                strRes = strRes.Substring(0, strRes.Length - 1);

                                strRes += @")";
                            }
                            else
                                ;
                        }
                        else
                            ;
                    }
                    else
                        ;
                }
                else
                    ;

                return strRes;
            }

            /// <summary>
            /// Сортировка выборки по дате
            /// </summary>
            public DataRow[] Select()
            {
                return m_tableLog.Select(string.Empty, "DATE_TIME");
            }

            //public int Select(int indxType, DateTime beg, DateTime end, ref DataRow[] res)
            /// <summary>
            /// Выборка лог-сообщений
            /// </summary>
            /// <param name="strIndxType">Индекс типа сообщения</param>
            /// <param name="beg">Начало периода</param>
            /// <param name="end">Оончание периода</param>
            /// <returns>Массив строк с сообщениями за период</returns>
            public DataRow[] ByDate(string strIndxType, DateTime beg, DateTime end)
            {
                string where = string.Empty;

                //m_tableLog.Clear();

                if (beg.Equals(DateTime.MaxValue) == false)
                {
                    where = $"DATE_TIME>='{beg.ToString("yyyyMMdd HH:mm:ss")}'";
                    if (end.Equals(DateTime.MaxValue) == false)
                        where += $" AND DATE_TIME<'{end.ToString("yyyyMMdd HH:mm:ss")}'";
                    else
                        ;
                }
                else
                    ;

                if (where.Equals(string.Empty) == false)
                    where += @" AND ";
                else
                    ;
                where += addingWhereTypeMessage(@"TYPE", strIndxType);

                return
                    //(from DataRow row in m_tableLog.Rows where ((DateTime)row ["DATE_TIME"]) >= beg && ((DateTime)row ["DATE_TIME"]) < end select row).ToArray()
                    m_tableLog.Select (where);
                    ;
            }
        }
        
        #endregion

        #region Обработчики событий для элементов панели лог-сообщений

        /// <summary>
        /// Обработчик выбора активных/неактивных пользователей
        /// </summary>
        private void dgvFilterActiveView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dgvFilterActiveView.Rows[e.RowIndex].Cells[0].Value = !bool.Parse(dgvFilterActiveView.Rows[e.RowIndex].Cells[0].Value.ToString());
            get_FilteredUsers();
        }

        /// <summary>
        /// Обработчик выбора ролей пользователей
        /// </summary>
        private void dgvFilterRoleStatistic_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dgvFilterRoleStatistic.Rows[e.RowIndex].Cells[0].Value = !bool.Parse(dgvFilterRoleStatistic.Rows[e.RowIndex].Cells[0].Value.ToString());
            get_FilteredUsers();
        }

        /// <summary>
        /// Метод формирования списка пользователей согласно выбранным фильтрам
        /// </summary>
        private void get_FilteredUsers()
        {
            m_tableUsers = m_tableUsers_unfiltered.Copy();
            int i = -1, err = 0;

            string where = string.Empty;

            activateTimerChecked (false);

            for (i = 0; i < m_tableRoles.Rows.Count; i++)
            {
                if (bool.Parse(dgvFilterRoleStatistic.Rows[i].Cells[0].Value.ToString()) == false)
                {
                    if (where.Equals(string.Empty) == true)
                        where = "ID_ROLE NOT IN (";
                    else
                        where += ",";

                    where += m_tableRoles.Rows[i]["ID"];
                }
                else
                    ;
            }

            if (where.Equals(string.Empty) == false)
                where += ")";
            else
                ;

            m_tableUsers.Rows.Clear();

            if (m_tableUsers_unfiltered.Select(where, c_list_sorted).Length != 0)
                m_tableUsers = m_tableUsers_unfiltered.Select(where, c_list_sorted).CopyToDataTable();
            else
                m_tableUsers.Rows.Clear();

            if (dgvFilterActiveView.Rows[0].Cells[0].Value == dgvFilterActiveView.Rows[1].Cells[0].Value)
                if (bool.Parse(dgvFilterActiveView.Rows[0].Cells[0].Value.ToString()) == true)
                //Отображать всех...
                {
                }
                else
                //Пустой список...
                {
                    m_tableUsers.Clear();
                }
            else
                filterActived (
                    procChecked ()
                );

            dgvUserView.Fill (m_tableUsers, c_list_sorted, err);

            activateTimerChecked (true);
        }

        protected void filterActived (bool [] arbActives)
        {
            if ((!(arbActives == null))
                && (arbActives.Length > 0)) {
                List<int> listIndexToRemoveUsers = new List<int> ();

                for (int i = 0;
                    (i < m_tableUsers.Rows.Count)
                        && (i < arbActives.Length);
                        i++) {
                    if (((arbActives [i] == true)
                            && (bool.Parse (dgvFilterActiveView.Rows [0].Cells [0].Value.ToString ()) == false))
                        || ((arbActives [i] == false)
                            && (bool.Parse (dgvFilterActiveView.Rows [1].Cells [0].Value.ToString ()) == false))) {
                        listIndexToRemoveUsers.Add (i);
                    } else
                        ;
                }

                if (listIndexToRemoveUsers.Count > 0) {
                    listIndexToRemoveUsers.Sort (delegate (int i1, int i2) {
                        return i1 > i2 ? -1 : 1;
                    });

                    //Удалить строки с неактивными пользователями
                    foreach (int indx in listIndexToRemoveUsers)
                        m_tableUsers.Rows.RemoveAt (indx);

                    m_tableUsers.AcceptChanges ();
                } else
                    ;
            } else
                ;
        }

        /// <summary>
        /// Обработчик выбора типа лог сообщений
        /// </summary>
        private void dgvFilterTypeMessage_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                dgvTypeToView.Rows[e.RowIndex].Cells[0].Value = !bool.Parse(dgvTypeToView.Rows[e.RowIndex].Cells[0].Value.ToString());

                startUpdatedgvLogMessages(false);
            }
            else
                ;
        }

        /// <summary>
        /// Обновление данных пользователя
        /// </summary>
        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            dgvUserStatistic_SelectionChanged(null, null);
        }

        /// <summary>
        /// Обработчик выбора Даты/Времени для лог сообщений
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">Аргумент события</param>
        protected void dgvDatetimeStart_SelectionChanged(object sender, EventArgs e)
        {
            int rowIndex = dgvListDateView.SelectedRows[0].Index;

            dgvListDateView.Rows[m_prevDatetimeRowIndex].Cells[0].Value = false;
            dgvListDateView.Rows[rowIndex].Cells[0].Value = true;
            m_prevDatetimeRowIndex = rowIndex;

            startFilldgvLogMessages(true);
        }

        #endregion
        
        #region Панель статистики

        /// <summary>
        /// Запрос на обновление статистики сообщений
        /// </summary>
        /// <param name="table_user">Таблица с пользователями который нужно опросить</param>
        /// <param name="dgv_user">DataGridView с пользователями который нужно опросить</param>
        /// <param name="multi_select">Выбирается много пользователей</param>
        protected string get_users(DataTable table_user, DataGridView dgv_user, bool multi_select)
        {
            delegateActionReport("Получаем список пользователей");

            int i = -1;
            string list_user = string.Empty;//Строка со списком выбранных пользователей
            string where = string.Empty;

            #region Получение строки со списком идентификаторов пользователей, выбранных в списке

            if (multi_select == true)
            {
                for (i = 0; i < table_user.Rows.Count; i++)//перебор строк таблицы
                {
                    if (bool.Parse(dgv_user.Rows[i].Cells[0].Value.ToString()) == true)//сравнение состояния CheckBox'а
                    {
                        if (list_user.Equals(string.Empty) == true)
                        {
                            list_user = " ";
                        }
                        else
                            list_user += ",";

                        list_user += table_user.Rows[i]["ID"];//добавление пользователя в список
                    }
                }
            }
            else
            {
                list_user += table_user.Rows[dgv_user.SelectedRows[0].Index]["ID"];//добавление одного пользователя
            }

            #endregion

            delegateReportClear(true);

            return list_user;
        }

        /// <summary>
        /// Обработчик события выбора пользователя для формирования списка сообщений
        /// </summary>
        protected abstract void dgvUserStatistic_SelectionChanged (object sender, EventArgs e);

        /// <summary>
        /// Обновление статистики сообщений из лога за выбранный период
        /// </summary>
        protected virtual void dgvUserView_SelectionChanged (object sender, EventArgs e)
        {
            bool bUpdate = false;

            bUpdate = (dgvUserView.SelectedRows.Count > 0)
                && (!(dgvUserView.SelectedRows[0].Index < 0));

            bUpdate = (dgvListDateView.Rows.Count > 0)
                && (dgvListDateView.SelectedRows[0].Index < (dgvListDateView.Rows.Count - 1))
                && !Equals(e, null);

            if (bUpdate == true) {
                //Останов потока разбора лог-файла пред. пользователя
                m_LogParse.Stop();

                dgvListDateView.SelectionChanged -= dgvDatetimeStart_SelectionChanged;

                //Очистить элементы управления с данными от пред. лог-файла
                if (IsHandleCreated == true)
                    if (InvokeRequired == true) {
                        BeginInvoke(new DelegateFunc(tabLoggingClearDatetimeStart));
                        BeginInvoke(new DelegateBoolFunc(tabLoggingClearText), true);
                    } else {
                        tabLoggingClearDatetimeStart();
                        tabLoggingClearText(true);
                    } else
                    Logging.Logg().Error(@"FormMainAnalyzer::dgvUserStatistic_SelectionChanged () - ... BeginInvoke (TabLoggingClearDatetimeStart, TabLoggingClearText) - ...", Logging.INDEX_MESSAGE.D_001);

                startLogParse(((int)dgvUserView.SelectedRows[0].Tag).ToString().Trim());

                listBoxTabVisible.Items.Clear();//очистка списка активных вкладок

                DbTSQLConfigDatabase.DbConfig().SetConnectionSettings();
                DbTSQLConfigDatabase.DbConfig().Register();
                //Заполнение ListBox'а активными вкладками
                fillListBoxTabVisible(
                    fill_active_tabs((int)m_tableUsers.Rows[dgvUserView.SelectedRows[0].Index][0]).ToList()
                );

                DbTSQLConfigDatabase.DbConfig().UnRegister();
            } else
                ;
        }

        /// <summary>
        /// Обновление списка пользователей при выборе ролей
        /// </summary>
        private void dgvFilterRoleView_CellClick (object sender, DataGridViewCellEventArgs e)
        {
            m_tableUsers_stat = m_tableUsers_stat_unfiltered.Copy();

            int i = -1, err = 0;
            string where = string.Empty;

            if (e.ColumnIndex == 0)
            {
                dgvFilterRoleView.Rows[e.RowIndex].Cells[0].Value = !bool.Parse(dgvFilterRoleView.Rows[e.RowIndex].Cells[0].Value.ToString());//фиксация значения

                for (i = 0; i < m_tableRoles.Rows.Count; i++)//перебор строк таблицы
                {
                    if (bool.Parse(dgvFilterRoleView.Rows[i].Cells[0].Value.ToString()) == false)
                    {
                        if (where.Equals(string.Empty) == true)
                        {
                            where = "ID_ROLE NOT IN (";
                        }
                        else
                            where += ",";

                        where += m_tableRoles.Rows[i]["ID"];//добавление ИД роли в условие выборки
                    }
                }

                if (where.Equals(string.Empty) == false)
                    where += ")";

                m_tableUsers_stat.Rows.Clear();

                if (m_tableUsers_stat_unfiltered.Select(where, c_list_sorted).Length != 0)
                    m_tableUsers_stat = m_tableUsers_stat_unfiltered.Select(where, c_list_sorted).CopyToDataTable();
                else
                    m_tableUsers_stat.Rows.Clear();
                //Отображение пользователей в DataGridView
                dgvUserStatistic.Fill (m_tableUsers_stat, c_list_sorted, err, true);
                ////обновление списка со статистикой сообщений
                //updateCounter(DATAGRIDVIEW_LOGCOUNTER.TYPE_TO_VIEW
                //    , HDateTime.ToMoscowTimeZone(StartCalendar.Value.Date)
                //    , HDateTime.ToMoscowTimeZone(EndCalendar.Value.Date)
                //    , get_users(m_tableUsers_stat, dgvUserView, true));
            }
        }

        /// <summary>
        /// Обновление списка пользователей при выборе начальной даты
        /// </summary>
        private void startCalendar_ChangeValue(object sender, EventArgs e)
        {
            updateCounter(DATAGRIDVIEW_LOGCOUNTER.TYPE_TO_STATISTIC
                , HDateTime.ToMoscowTimeZone(StartCalendar.Value.Date)
                , HDateTime.ToMoscowTimeZone(EndCalendar.Value.Date)
                , get_users(m_tableUsers_stat, dgvUserStatistic, true));
        }

        /// <summary>
        /// Обновление списка пользователей при выборе конечной даты
        /// </summary>
        private void stopCalendar_ChangeValue(object sender, EventArgs e)
        {
            updateCounter(DATAGRIDVIEW_LOGCOUNTER.TYPE_TO_STATISTIC
                , HDateTime.ToMoscowTimeZone(StartCalendar.Value.Date)
                , HDateTime.ToMoscowTimeZone(EndCalendar.Value.Date)
                , get_users(m_tableUsers_stat, dgvUserStatistic, true));
        }

        #endregion

        #region Панель вкладок

        /// <summary>
        /// Абстрактный метод для получения из БД списка активных вкладок для выбранного пользователя
        /// </summary>
        /// <param name="user">Идентификатор пользователя для выборки</param>
        protected abstract IEnumerable<int> fill_active_tabs(int user);

        /// <summary>
        /// Заполнение ListBox активными вкладками
        /// </summary>
        /// <param name="ID_tabs">Список активных вкладок</param>
        protected abstract void fillListBoxTabVisible(List<int> ID_tabs);

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

                //Обнуление счётчиков сообщений на панели статистики
                clearValues ();

                if (table_statMessage.Rows.Count > 0) {
                    foreach (DataGridViewRow r in this.Rows) {
                        try {
                            strCount = (from DataRow dataRow in table_statMessage.Rows
                                where (int)dataRow["ID_LOGMSG"] == (int)r.Tag
                                select dataRow)
                                    .ToArray()[0][@"COUNT"].ToString();
                        } catch {
                            strCount = 0.ToString();
                        } finally {
                            r.Cells[lastIndex].Value = strCount;
                        }
                    }
                } else
                    ;
            }

            /// <summary>
            /// Возвращает список состояния CheckBox'ов
            /// </summary>
            /// <return>Возвращает список состояния CheckBox'ов</return>
            public List<bool> Checked()
            {
                List<bool> listRes = new List<bool>();

                if (_type == TYPE.WITH_CHECKBOX)
                    for (int i = 0; i < this.Rows.Count; i++)
                    //добавление состояния CheckBox'а в список
                        listRes.Add((bool)this.Rows[i].Cells[lastIndex - 2].Value);
                else
                    ;

                return listRes;
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
            protected void clearValues()
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
            int iNewRow = -1
                , indxFieldTag = -1;

            if (run == 0) {
                bCheckedItem = checkDefault;
                ctrl.Rows.Clear ();

                if (src.Rows.Count > 0) {
                    indxFieldTag = src.Columns.IndexOf ("ID");

                    foreach(DataRow r in src.Rows) {
                        iNewRow = ctrl.Rows.Add (new object [] { bCheckedItem, r [nameField].ToString () });

                        if (!(indxFieldTag < 0))
                            ctrl.Rows[iNewRow].Tag = int.Parse (r [indxFieldTag].ToString ());
                        else
                            ;
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

                indx = tags.IndexOf((int)r.Tag);

                if (!(indx < 0))
                    active = arbActives[indx];
                else
                    active = false;

                r.Cells[0].Value = active;
            }
        }
    }
}
