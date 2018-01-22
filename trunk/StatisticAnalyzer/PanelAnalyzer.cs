﻿//using HClassLibrary;
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

namespace StatisticAnalyzer
{
    public abstract partial class PanelAnalyzer : PanelStatistic
    {
        #region Design

            private System.Windows.Forms.TabPage tabPageLogging;
            private System.Windows.Forms.TabPage tabPageTabes;
            private TableLayoutPanel panelTabPageTabes;
            private TableLayoutPanel panelTabPageLogging;

            protected System.Windows.Forms.CheckBox[] checkBoxs;
            protected System.Windows.Forms.ListBox listTabVisible;

            private System.Windows.Forms.Button buttonUpdate;
        
            protected DataGridView_LogMessageCounter dgvFilterTypeMessage;
            protected DataGridView_LogMessageCounter dgvMessage;
            protected System.Windows.Forms.DataGridView dgvUser;
            protected System.Windows.Forms.DataGridView dgvRole;
            protected System.Windows.Forms.DateTimePicker StartCalendar;
            protected System.Windows.Forms.DateTimePicker StopCalendar;
            protected System.Windows.Forms.DataGridView dgvClient;
            protected System.Windows.Forms.DataGridView dgvDatetimeStart;
            private System.Windows.Forms.DataGridView dgvLogMessage;
            private System.Windows.Forms.DataGridView dgvFilterRoles;
            private System.Windows.Forms.DataGridView dgvFilterActives;

            //protected abstract DataGridView_LogMessageCounter create_LogMessageCounter(DataGridView_LogMessageCounter.TYPE type);

            private void InitializeComponent()
            {
                this.dgvMessage = new DataGridView_LogMessageCounter (DataGridView_LogMessageCounter.TYPE.WITHOUT_CHECKBOX);
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
                this.StopCalendar = new DateTimePicker();
                this.dgvRole = new DataGridView();
                this.dgvUser = new DataGridView();

                System.Windows.Forms.Label labelFilterActives = new System.Windows.Forms.Label();
                this.dgvFilterActives = new System.Windows.Forms.DataGridView();

                System.Windows.Forms.Label labelFilterRoles = new System.Windows.Forms.Label();
                this.dgvFilterRoles = new System.Windows.Forms.DataGridView();

                this.dgvClient = new System.Windows.Forms.DataGridView();

                this.tabPageLogging = new System.Windows.Forms.TabPage();
                this.panelTabPageLogging = new TableLayoutPanel ();
                this.dgvLogMessage = new System.Windows.Forms.DataGridView();
                this.tabPageTabes = new System.Windows.Forms.TabPage();
                this.panelTabPageTabes = new TableLayoutPanel();
                this.listTabVisible = new System.Windows.Forms.ListBox();
                this.checkBoxs=new CheckBox[(int)FormChangeMode.MODE_TECCOMPONENT.ANY +1];
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC] = new CheckBox();
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG] = new CheckBox();
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP] = new CheckBox();
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC] = new CheckBox();
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY] = new CheckBox();

                System.Windows.Forms.Label labelDatetimeStart = new System.Windows.Forms.Label();
                this.dgvDatetimeStart = new System.Windows.Forms.DataGridView();

                System.Windows.Forms.Label labelFilterTypeMessage = new System.Windows.Forms.Label();
                this.dgvFilterTypeMessage = new DataGridView_LogMessageCounter(DataGridView_LogMessageCounter.TYPE.WITH_CHECKBOX);

                this.buttonUpdate = new System.Windows.Forms.Button();

                ((System.ComponentModel.ISupportInitialize)(this.dgvFilterActives)).BeginInit();
                ((System.ComponentModel.ISupportInitialize)(this.dgvFilterRoles)).BeginInit();
                ((System.ComponentModel.ISupportInitialize)(this.dgvClient)).BeginInit();

                this.tabPageLogging.SuspendLayout();
                this.panelTabPageLogging.SuspendLayout ();
                ((System.ComponentModel.ISupportInitialize)(this.dgvFilterTypeMessage)).BeginInit();
                ((System.ComponentModel.ISupportInitialize)(this.dgvDatetimeStart)).BeginInit();
                ((System.ComponentModel.ISupportInitialize)(this.dgvLogMessage)).BeginInit();
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
                this.dgvFilterActives.AllowUserToAddRows = false;
                this.dgvFilterActives.AllowUserToDeleteRows = false;
                this.dgvFilterActives.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.dgvFilterActives.ColumnHeadersVisible = false;
                this.dgvFilterActives.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                    new DataGridViewCheckBoxColumn (),
                    new DataGridViewTextBoxColumn ()});
                //this.dgvFilterActives.Location = new System.Drawing.Point(12, 28);
                this.dgvFilterActives.Dock = DockStyle.Fill;
                //this.dgvFilterActives.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top));
                this.dgvFilterActives.MultiSelect = false;
                this.dgvFilterActives.Name = "dgvFilterActives";
                this.dgvFilterActives.ReadOnly = true;
                this.dgvFilterActives.RowHeadersVisible = false;
                this.dgvFilterActives.RowTemplate.Height = 18;
                this.dgvFilterActives.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvFilterActives.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
                //this.dgvFilterActives.Size = new System.Drawing.Size(190, 39);
                this.dgvFilterActives.TabIndex = 8;
                // 
                // dataGridViewActivesCheckBoxColumnUse
                // 
                int i = 0;
                this.dgvFilterActives.Columns[i].Frozen = true;
                this.dgvFilterActives.Columns[i].HeaderText = "Use";
                this.dgvFilterActives.Columns[i].Name = "dataGridViewActivesCheckBoxColumnUse";
                this.dgvFilterActives.Columns[i].ReadOnly = true;
                this.dgvFilterActives.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvFilterActives.Columns[i].Width = 25;
                // 
                // dataGridViewActivesTextBoxColumnDesc
                // 
                i = 1;
                //this.dgvFilterActives.Columns[i].Frozen = true;
                this.dgvFilterActives.Columns[i].HeaderText = "Desc";
                this.dgvFilterActives.Columns[i].Name = "dataGridViewActivesTextBoxColumnDesc";
                this.dgvFilterActives.Columns[i].ReadOnly = true;
                this.dgvFilterActives.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvFilterActives.Columns[i].Width = 165;
                this.dgvFilterActives.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

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
                this.dgvFilterRoles.AllowUserToAddRows = false;
                this.dgvFilterRoles.AllowUserToDeleteRows = false;
                this.dgvFilterRoles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.dgvFilterRoles.ColumnHeadersVisible = false;
                this.dgvFilterRoles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                    new DataGridViewCheckBoxColumn (),
                    new DataGridViewTextBoxColumn ()});
                //this.dgvFilterRoles.Location = new System.Drawing.Point(12, 88);
                this.dgvFilterRoles.Dock = DockStyle.Fill;
                this.dgvFilterRoles.MultiSelect = false;
                this.dgvFilterRoles.Name = "dgvFilterRoles";
                this.dgvFilterRoles.ReadOnly = true;
                this.dgvFilterRoles.RowHeadersVisible = false;
                this.dgvFilterRoles.RowTemplate.Height = 18;
                this.dgvFilterRoles.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvFilterRoles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
                //this.dgvFilterRoles.Size = new System.Drawing.Size(190, 111);
                this.dgvFilterRoles.TabIndex = 9;
                this.dgvFilterRoles.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvFilterRoles_CellClick);
                // 
                // dataGridViewRolesCheckBoxColumnUse
                // 
                i = 0;
                this.dgvFilterRoles.Columns[i].Frozen = true;
                this.dgvFilterRoles.Columns[i].HeaderText = "Use";
                this.dgvFilterRoles.Columns[i].Name = "dataGridViewRolesCheckBoxColumnUse";
                this.dgvFilterRoles.Columns[i].ReadOnly = true;
                this.dgvFilterRoles.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvFilterRoles.Columns[i].Width = 25;
                // 
                // dataGridViewRolesTextBoxColumnDesc
                // 
                i = 1;
                //this.dgvFilterRoles.Columns[i].Frozen = true;
                this.dgvFilterRoles.Columns[i].HeaderText = "Desc";
                this.dgvFilterRoles.Columns[i].Name = "dataGridViewRolesTextBoxColumnDesc";
                this.dgvFilterRoles.Columns[i].ReadOnly = true;
                this.dgvFilterRoles.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvFilterRoles.Columns[i].Width = 145;
                this.dgvFilterRoles.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                // 
                // dgvClient
                // 
                this.dgvClient.AllowUserToAddRows = false;
                this.dgvClient.AllowUserToDeleteRows = false;
                this.dgvClient.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.dgvClient.ColumnHeadersVisible = false;
                this.dgvClient.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                    new DataGridViewCheckBoxColumn (),
                    new DataGridViewTextBoxColumn ()});
                //this.dgvClient.Location = new System.Drawing.Point(12, 150);
                this.dgvClient.Dock = DockStyle.Fill;
                this.dgvClient.MultiSelect = false;
                this.dgvClient.Name = "dgvClient";
                this.dgvClient.ReadOnly = true;
                this.dgvClient.RowHeadersVisible = false;
                this.dgvClient.RowTemplate.Height = 18;
                this.dgvClient.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvClient.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
                //this.dgvClient.Size = new System.Drawing.Size(190, 400);
                this.dgvClient.TabIndex = 10;
                this.dgvClient.SelectionChanged += new System.EventHandler(this.dgvClient_SelectionChanged);
                // 
                // dataGridViewClientCheckBoxColumnActive
                // 
                i = 0;
                this.dgvClient.Columns[i].Frozen = true;
                this.dgvClient.Columns[i].HeaderText = "Active";
                this.dgvClient.Columns[i].Name = "dataGridViewClientCheckBoxColumnActive";
                this.dgvClient.Columns[i].ReadOnly = true;
                this.dgvClient.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvClient.Columns[i].Width = 25;
                // 
                // dataGridViewClientTextBoxColumnDesc
                // 
                i = 1;
                //this.dgvClient.Columns[i].Frozen = true;
                this.dgvClient.Columns[i].HeaderText = "Desc";
                this.dgvClient.Columns[i].Name = "dataGridViewClientTextBoxColumnDesc";
                this.dgvClient.Columns[i].ReadOnly = true;
                this.dgvClient.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvClient.Columns[i].Width = 145;
                this.dgvClient.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

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
                this.panelTabPageLogging.Controls.Add(this.dgvDatetimeStart, 0, 1); this.SetColumnSpan(this.dgvDatetimeStart, 2); this.SetRowSpan(this.dgvDatetimeStart, 11);
                this.panelTabPageLogging.Controls.Add(labelFilterTypeMessage, 0, 12); this.SetColumnSpan(labelFilterTypeMessage, 2);
                this.panelTabPageLogging.Controls.Add(this.dgvFilterTypeMessage, 0, 13); this.SetColumnSpan(this.dgvFilterTypeMessage, 2); this.SetRowSpan(this.dgvFilterTypeMessage, 11);
                this.panelTabPageLogging.Controls.Add(this.dgvLogMessage, 2, 0); this.SetColumnSpan(this.dgvLogMessage, 4); this.SetRowSpan(this.dgvLogMessage, 24);

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
                this.dgvDatetimeStart.Dock = DockStyle.Fill;
                this.dgvDatetimeStart.AllowUserToAddRows = false;
                this.dgvDatetimeStart.AllowUserToDeleteRows = false;
                this.dgvDatetimeStart.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.dgvDatetimeStart.ColumnHeadersVisible = false;
                this.dgvDatetimeStart.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                    new DataGridViewCheckBoxColumn (),
                    new DataGridViewTextBoxColumn ()});
                //this.dgvDatetimeStart.Location = new System.Drawing.Point(6, 23);
                this.dgvDatetimeStart.MultiSelect = false;
                this.dgvDatetimeStart.Name = "dgvDatetimeStart";
                this.dgvDatetimeStart.ReadOnly = true;
                this.dgvDatetimeStart.RowHeadersVisible = false;
                this.dgvDatetimeStart.RowTemplate.Height = 18;
                this.dgvDatetimeStart.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvDatetimeStart.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
                //this.dgvDatetimeStart.Size = new System.Drawing.Size(170, 164);
                this.dgvDatetimeStart.BackColor = this.BackColor;
                this.dgvDatetimeStart.TabIndex = 10;
                // 
                // dataGridViewCheckBoxColumnDatetimeStartUse
                // 
                i = 0;
                this.dgvDatetimeStart.Columns[i].Frozen = true;
                this.dgvDatetimeStart.Columns[i].HeaderText = "Use";
                this.dgvDatetimeStart.Columns[i].Name = "dataGridViewCheckBoxColumn1";
                this.dgvDatetimeStart.Columns[i].ReadOnly = true;
                this.dgvDatetimeStart.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvDatetimeStart.Columns[i].Width = 25;
                // 
                // dataGridViewTextBoxColumnDatetimeStartDesc
                // 
                i = 1;
                //this.dgvDatetimeStart.Columns[i].Frozen = true;
                this.dgvDatetimeStart.Columns[i].HeaderText = "Desc";
                this.dgvDatetimeStart.Columns[i].Name = "dataGridViewTextBoxColumnDatetimeStartDesc";
                this.dgvDatetimeStart.Columns[i].ReadOnly = true;
                this.dgvDatetimeStart.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvDatetimeStart.Columns[i].Width = 145;
                this.dgvDatetimeStart.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
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
                this.dgvLogMessage.Dock = DockStyle.Fill;
                this.dgvLogMessage.MultiSelect = false;
                this.dgvLogMessage.Name = "dgvLogMessage";
                this.dgvLogMessage.ReadOnly = true;
                this.dgvLogMessage.ScrollBars = System.Windows.Forms.ScrollBars.Both;
                //this.dgvLogMessage.Size = new System.Drawing.Size(371, 292);
                this.dgvLogMessage.TabIndex = 15;
                this.dgvLogMessage.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                new System.Windows.Forms.DataGridViewTextBoxColumn ()
                    , new System.Windows.Forms.DataGridViewTextBoxColumn ()
                    , new System.Windows.Forms.DataGridViewTextBoxColumn ()
                });
                this.dgvLogMessage.AllowUserToAddRows =
                this.dgvLogMessage.AllowUserToDeleteRows =
                    false;
                this.dgvLogMessage.ColumnHeadersVisible = false;
                this.dgvLogMessage.RowHeadersVisible = false;
                this.dgvLogMessage.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvLogMessage.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
                this.dgvLogMessage.Columns[0].Width = 85; this.dgvLogMessage.Columns[0].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvLogMessage.Columns[1].Width = 30; this.dgvLogMessage.Columns[1].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvLogMessage.Columns[2].Width = 254; this.dgvLogMessage.Columns[2].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvLogMessage.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
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
                this.panelTabPageTabes.Controls.Add(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY], 0, 0); this.SetRowSpan(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY], 2);
                this.panelTabPageTabes.Controls.Add(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC], 1, 0); this.SetRowSpan(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC], 2);
                this.panelTabPageTabes.Controls.Add(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP], 1, 2); this.SetRowSpan(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP], 2);
                this.panelTabPageTabes.Controls.Add(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG], 2, 0); this.SetRowSpan(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG], 2);
                this.panelTabPageTabes.Controls.Add(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC], 2, 2); this.SetRowSpan(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC], 2);
                this.panelTabPageTabes.Controls.Add(this.listTabVisible, 0, 4); this.SetColumnSpan(this.listTabVisible, 6); this.SetRowSpan(this.listTabVisible, 20);
                // 
                // listTabVisible
                // 
                this.listTabVisible.Dock = DockStyle.Fill;
                
                //this.dgvTabVisible.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
                //this.dgvTabVisible.Location = new System.Drawing.Point(6, 54);
                this.listTabVisible.Name = "listTabVisible";
                //this.dgvTabVisible.Size = new System.Drawing.Size(547, 211);
                this.listTabVisible.TabIndex = 15;
                // 
                // checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC]
                // 
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].AutoSize = true;
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Enabled = true;
                //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Location = new System.Drawing.Point(6, 6);
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Name = "checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC]";
                //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Size = new System.Drawing.Size(48, 17);
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].TabIndex = 16;
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Text = "ТЭЦ";
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].UseVisualStyleBackColor = true;
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Checked = true;
                // 
                // checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG]
                // 
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG].AutoSize = true;
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Enabled = true;
                //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Location = new System.Drawing.Point(6, 29);
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Name = "checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG]";
                //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Size = new System.Drawing.Size(39, 17);
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG].TabIndex = 17;
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Text = "ТГ";
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG].UseVisualStyleBackColor = true;
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Checked = true;
                // 
                // checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP]
                // 
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].AutoSize = true;
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Enabled = true;
                //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Location = new System.Drawing.Point(112, 6);
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Name = "checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP]";
                //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Size = new System.Drawing.Size(47, 17);
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].TabIndex = 18;
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Text = "ГТП";
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].UseVisualStyleBackColor = true;
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Checked = true;
                // 
                // checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC]
                // 
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC].AutoSize = true;
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Enabled = true;
                //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Location = new System.Drawing.Point(112, 29);
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Name = "checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC]";
                //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Size = new System.Drawing.Size(44, 17);
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC].TabIndex = 19;
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Text = "ЩУ";
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC].UseVisualStyleBackColor = true;
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Checked = true;
                // 
                // checkBoxAdmin
                // 
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].AutoSize = true;
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].Enabled = true;
                //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].Location = new System.Drawing.Point(6, 275);
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].Name = "checkBoxAdmin";
                //this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].Size = new System.Drawing.Size(48, 17);
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].TabIndex = 20;
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].Text = HAdmin.PBR_PREFIX;
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].UseVisualStyleBackColor = true;
                this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].Checked = true;

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
                this.dgvUser.Name = "dgvUser";
                this.dgvUser.Dock = DockStyle.Fill;
                //this.listUser.TabIndex = 4;

                this.dgvUser.AllowUserToAddRows = false;
                this.dgvUser.AllowUserToDeleteRows = false;
                this.dgvUser.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.dgvUser.ColumnHeadersVisible = false;
                this.dgvUser.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                    new DataGridViewCheckBoxColumn (),
                    new DataGridViewTextBoxColumn ()});
                this.dgvUser.MultiSelect = false;
                this.dgvUser.ReadOnly = true;
                this.dgvUser.RowHeadersVisible = false;
                this.dgvUser.RowTemplate.Height = 18;
                this.dgvUser.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvUser.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
                //this.dgvFilterRoles.Size = new System.Drawing.Size(190, 111);
                this.dgvUser.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvUser_CellClick);

                i = 0;
                this.dgvUser.Columns[i].Frozen = true;
                this.dgvUser.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvUser.Columns[i].Width = 20;
                // 
                // dataGridViewTextBoxColumnCounter
                // 
                i = 1;
                this.dgvUser.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                this.dgvUser.Columns[i].Width = 140;

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
                this.dgvRole.Name = "dgvRole";
                this.dgvRole.Dock = DockStyle.Fill;
                //this.listUser.TabIndex = 4;

                this.dgvRole.AllowUserToAddRows = false;
                this.dgvRole.AllowUserToDeleteRows = false;
                this.dgvRole.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.dgvRole.ColumnHeadersVisible = false;
                this.dgvRole.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                    new DataGridViewCheckBoxColumn (),
                    new DataGridViewTextBoxColumn ()});
                this.dgvRole.MultiSelect = false;
                this.dgvRole.ReadOnly = true;
                this.dgvRole.RowHeadersVisible = false;
                this.dgvRole.RowTemplate.Height = 18;
                this.dgvRole.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvRole.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
                //this.dgvFilterRoles.Size = new System.Drawing.Size(190, 111);
                this.dgvRole.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvRole_CellClick);

                i = 0;
                this.dgvRole.Columns[i].Frozen = true;
                this.dgvRole.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                this.dgvRole.Columns[i].Width = 20;
                // 
                // dataGridViewTextBoxColumnCounter
                // 
                i = 1;
                this.dgvRole.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                this.dgvRole.Columns[i].Width = 140;

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
                this.StopCalendar.Name = "StopCalendar";
                this.StopCalendar.Dock = DockStyle.Fill;
                //this.listUser.TabIndex = 4;
                this.StopCalendar.ValueChanged += new System.EventHandler(this.stopCalendar_ChangeValue);

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
                panelUser.Controls.Add(this.dgvFilterActives, 0, 1); panelUser.SetColumnSpan(this.dgvFilterActives, 3); panelUser.SetRowSpan(this.dgvFilterActives, 3);

                panelUser.Controls.Add(labelFilterRoles, 0, 4); panelUser.SetColumnSpan(labelFilterRoles, 3);
                panelUser.Controls.Add(this.dgvFilterRoles, 0, 5); panelUser.SetColumnSpan(this.dgvFilterRoles, 3); panelUser.SetRowSpan(this.dgvFilterRoles, 6);

                panelUser.Controls.Add(this.dgvClient, 0, 11); panelUser.SetColumnSpan(this.dgvClient, 3); panelUser.SetRowSpan(this.dgvClient, 13);
                
                panelUser.Controls.Add(labelDatetimeStart, 3, 0); this.SetColumnSpan(labelDatetimeStart, 3);
                panelUser.Controls.Add(this.dgvDatetimeStart, 3, 1); this.SetColumnSpan(this.dgvDatetimeStart, 3); this.SetRowSpan(this.dgvDatetimeStart, 11);
                panelUser.Controls.Add(labelFilterTypeMessage, 3, 12); this.SetColumnSpan(labelFilterTypeMessage, 3);
                panelUser.Controls.Add(this.dgvFilterTypeMessage, 3, 13); this.SetColumnSpan(this.dgvFilterTypeMessage, 3); this.SetRowSpan(this.dgvFilterTypeMessage, 9);
                panelUser.Controls.Add(this.dgvLogMessage, 6, 0); this.SetColumnSpan(this.dgvLogMessage, 6); this.SetRowSpan(this.dgvLogMessage, 24);
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

                panelTabs.Controls.Add(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY], 0, 8); this.SetRowSpan(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY], 2);
                panelTabs.Controls.Add(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC], 0,6); this.SetRowSpan(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC], 2);
                panelTabs.Controls.Add(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP], 0, 4); this.SetRowSpan(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP], 2);
                panelTabs.Controls.Add(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG], 0, 2); this.SetRowSpan(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG], 2);
                panelTabs.Controls.Add(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC], 0, 0); this.SetRowSpan(this.checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC], 2);
                panelTabs.Controls.Add(this.listTabVisible, 2, 0); this.SetColumnSpan(this.listTabVisible, 10); this.SetRowSpan(this.listTabVisible, 12);
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
                panelMessage.Controls.Add(this.StopCalendar, 0, 4); this.SetColumnSpan(this.StopCalendar, 1); this.SetRowSpan(this.StopCalendar, 2);


                panelMessage.Controls.Add(labelRole, 0, 6); this.SetColumnSpan(labelRole, 1); this.SetRowSpan(labelRole, 1);
                panelMessage.Controls.Add(this.dgvRole, 0, 7); this.SetColumnSpan(this.dgvRole, 1); this.SetRowSpan(this.dgvRole, 7);

                panelMessage.Controls.Add(labelUser, 0, 14); this.SetColumnSpan(labelUser, 1); this.SetRowSpan(labelUser, 1);
                panelMessage.Controls.Add(this.dgvUser, 0, 15); this.SetColumnSpan(this.dgvUser, 1); this.SetRowSpan(this.dgvUser, 12);

                panelMessage.Controls.Add(labelMessage, 0, 27); this.SetColumnSpan(labelMessage, 1); this.SetRowSpan(labelMessage, 1);
                panelMessage.Controls.Add(this.dgvMessage, 0, 28); this.SetColumnSpan(this.dgvMessage, 1); this.SetRowSpan(this.dgvMessage, 8);

                panelMessage.ResumeLayout(false);
                panelMessage.PerformLayout();
                //this.panelMessage.

                groupMessage.Controls.Add(panelMessage);
                groupMessage.Text = "Статистика";

                #endregion


                this.Controls.Add(groupMessage, 9, 0); this.SetColumnSpan(groupMessage, 3); this.SetRowSpan(groupMessage, 24);
                
                this.Controls.Add(groupUser, 0, 0); this.SetColumnSpan(groupUser, 9); this.SetRowSpan(groupUser, 18);

                this.Controls.Add(groupTabs, 0, 18); this.SetColumnSpan(groupTabs, 9); this.SetRowSpan(groupTabs, 6);

                ((System.ComponentModel.ISupportInitialize)(this.dgvFilterActives)).EndInit();
                ((System.ComponentModel.ISupportInitialize)(this.dgvFilterRoles)).EndInit();
                ((System.ComponentModel.ISupportInitialize)(this.dgvClient)).EndInit();
                
                this.tabPageLogging.ResumeLayout(false);
                this.tabPageLogging.PerformLayout();
                this.panelTabPageLogging.ResumeLayout(false);
                this.panelTabPageLogging.PerformLayout ();
                ((System.ComponentModel.ISupportInitialize)(this.dgvFilterTypeMessage)).EndInit();
                ((System.ComponentModel.ISupportInitialize)(this.dgvDatetimeStart)).EndInit();
                ((System.ComponentModel.ISupportInitialize)(this.dgvLogMessage)).EndInit();
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
        /// Время для запуска таймера
        /// </summary>
        protected int MSEC_TIMERCHECKED_STANDARD = 66666, 
            MSEC_TIMERCHECKED_FORCE = 666;

        /// <summary>
        /// Таймер
        /// </summary>
        protected
            System.Threading.Timer
                m_timerChecked;

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
        protected static string[] m_chDelimeters = { @"DELIMETER_PART", "DELIMETER_ROW" };

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
            m_LogParse = newLogParse();

            InitializeComponent();

            m_arCheckBoxMode = new CheckBox[] { checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC], checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP], checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC], checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG] };

            m_listTEC = tec;

            dgvFilterActives.Rows.Add(2);
            dgvFilterActives.Rows[0].Cells[0].Value = true; dgvFilterActives.Rows[0].Cells[1].Value = "Активные";
            dgvFilterActives.Rows[1].Cells[0].Value = true; dgvFilterActives.Rows[1].Cells[1].Value = "Не активные";
            dgvFilterActives.Columns[0].ReadOnly = false;
            dgvFilterActives.CellClick += new DataGridViewCellEventHandler(dgvFilterActives_CellClick);
            dgvFilterActives.Enabled = true;

            dgvFilterTypeMessage.CellClick += new DataGridViewCellEventHandler(dgvFilterTypeMessage_CellClick);

            #region Изменение при создании цвета фона-шрифта
            getTypedControls (this, new Type [] { typeof (DataGridView) }).Cast<DataGridView> ().ToList ().ForEach (dgv => {
                dgv.DefaultCellStyle.BackColor = BackColor == SystemColors.Control ? SystemColors.Window : BackColor;
                dgv.DefaultCellStyle.ForeColor = ForeColor;
            });

            if (Equals (listTabVisible, null) == false) {
                listTabVisible.BackColor = BackColor == SystemColors.Control ? SystemColors.Window : BackColor;
                listTabVisible.ForeColor = ForeColor;
            } else
                ;
            #endregion

            int err = -1;

            DbConnection connConfigDB = DbSources.Sources().GetConnection(DbTSQLConfigDatabase.DbConfig ().ListenerId,out err);

            if ((!(connConfigDB == null))
                && (err == 0))
            {
                HStatisticUsers.GetRoles(ref connConfigDB, string.Empty, string.Empty, out m_tableRoles, out err);
                FillDataGridViews(ref dgvFilterRoles, m_tableRoles, @"DESCRIPTION", err, true);
                FillDataGridViews(ref dgvRole, m_tableRoles, @"DESCRIPTION", err, true);

                HStatisticUsers.GetUsers(ref connConfigDB, string.Empty, c_list_sorted, out m_tableUsers, out err);
                FillDataGridViews(ref dgvClient, m_tableUsers, @"DESCRIPTION", err);
                FillDataGridViews (ref dgvUser, m_tableUsers, @"DESCRIPTION", err, true);

                m_tableUsers_stat = m_tableUsers.Copy();
                m_tableUsers_unfiltered = m_tableUsers.Copy();
                m_tableUsers_stat_unfiltered = m_tableUsers.Copy ();
                
                if (!(m_LogParse == null))
                {
                    m_LogParse.Exit = LogParseExit;

                    //Start();
                }
                else
                {
                    string strErr = @"Не создан объект разбора сообщений (класс 'LogParse')...";
                    Logging.Logg().Error(strErr, Logging.INDEX_MESSAGE.NOT_SET);
                    throw new Exception(strErr);
                }
            } else
                ;
        }

        /// <summary>
        /// Метод для разбора строки с лог сообщениями
        /// </summary>
        /// <returns>Возвращает строку</returns>
        protected abstract LogParse newLogParse();

        #region Объявление абстрактных методов

        /// <summary>
        /// Проверка запуска пользователем ПО
        /// </summary>
        /// <param name="arbActives">Массив занчений активности каждого пользователя</param>
        /// <param name="err">Ошибка</param>
        protected abstract void procChecked(out bool[] arbActives, out int err);

        /// <summary>
        /// Запись значений активности в CheckBox на DataGridView с пользователями
        /// </summary>
        /// <param name="obj"></param>
        protected abstract void procChecked(object obj);

        /// <summary>
        /// 
        /// </summary>
        protected abstract void disconnect();

        /// <summary>
        /// Обработчик события выбора пользователя для формирования списка сообщений
        /// </summary>
        protected abstract void dgvClient_SelectionChanged(object sender, EventArgs e);

        /// <summary>
        /// Начало разбора строки с логом
        /// </summary>
        /// <param name="par">Строка для разбора</param>
        protected abstract void startLogParse(string par);

        /// <summary>
        /// Выборка лог-сообщений по параметрам
        /// </summary>
        /// <param name="type">Тип сообщений</param>
        /// <param name="beg">Начало периода</param>
        /// <param name="end">Окончание периода</param>
        /// <param name="rows">Массив сообщений</param>
        /// <returns></returns>
        protected abstract int selectLogMessage(string type, DateTime beg, DateTime end, ref DataRow[] rows);

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
            base.Start();

            m_bThreadTimerCheckedAllowed = true;

            m_timerChecked =
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
            m_bThreadTimerCheckedAllowed = false;

            if (!(m_timerChecked == null))
            {
                //Вариант №0
                m_timerChecked.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                ////Вариант №1
                //m_timerChecked.Stop ();
                //m_timerChecked.Dispose();
                //m_timerChecked = null;
            }
            else
                ;
            base.Stop();
        }

        /// <summary>
        /// Активировать/деактивировать панель
        /// </summary>
        /// <param name="active">Признак активации/деактивации</param>
        /// <returns></returns>
        public override bool Activate(bool activated)
        {
            bool bRes = base.Activate(activated);

            if (activated == true)
            {
                //Start();
                m_timerChecked.Change(0, System.Threading.Timeout.Infinite);
            }

            else
            {
                if (!(m_timerChecked == null))
                {
                    m_timerChecked.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                }
            }

            return bRes;
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
            if (indx < dgvClient.Rows.Count) //m_tableUsers, m_listTcpClient
                dgvClient.Rows[indx].Cells[0].Value = false;
            else
                ; //Отработка ошибки соединения для пользователя УЖЕ отсутствующего в списке
        }

        /// <summary>
        /// Метод заполнения DataGridView данными
        /// </summary>
        /// <param name="ctrl">Объект DataGridView для заполнения</param>
        /// <param name="src">Источник заполнения</param>
        /// <param name="nameField">Имя столбца с данными</param>
        /// <param name="run">Для получения установить = 0</param>
        /// <param name="checkDefault">CheckBox по умолчанию вкл/выкл</param>
        public void FillDataGridViews(ref DataGridView ctrl, DataTable src, string nameField, int run, bool checkDefault = false)
        {
            if (run == 0)
            {
                bool bCheckedItem = checkDefault;
                ctrl.Rows.Clear();

                if (src.Rows.Count > 0)
                {
                    ctrl.Rows.Add(src.Rows.Count);

                    for (int i = 0; i < src.Rows.Count; i++)
                    {
                        //Проверка активности
                        //m_tcpSender.Init(m_tableUsers.Rows[i]["COMPUTER_NAME"].ToString ());
                        //bCheckedItem = m_tcpSender.Connected;
                        //m_tcpSender.Close ();

                        ctrl.Rows[i].Cells[0].Value = bCheckedItem;
                        ctrl.Rows[i].Cells[1].Value = src.Rows[i][nameField].ToString();
                    }
                }
                else
                    ;
            }
            else
                ;
        }

        #region Панель лог-сообщений

        /// <summary>
        /// Очистка таблицы с датами
        /// </summary>
        protected void tabLoggingClearDatetimeStart() { dgvDatetimeStart.Rows.Clear(); }

        /// <summary>
        /// Очистка таблицы с лог-сообщениями
        /// </summary>
        /// <param name="bClearCounter"></param>
        protected void tabLoggingClearText(bool bClearCounter)
        {
            m_LogParse.Clear();
            /*textBoxLog.Clear();*/
            dgvLogMessage.Rows.Clear();
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

            if (dgvLogMessage.Rows.Count > 0)
                dgvLogMessage.FirstDisplayedScrollingRowIndex = 0;
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
        protected abstract void updateCounter(DataGridView_LogMessageCounter dgv, DateTime start_date, DateTime stop_date, string users);

        /// <summary>
        /// Метод заполнения таблиц с типами сообщений и лог-сообщениями
        /// </summary>
        void TabLoggingAppendText(object data)
        {
            try
            {
                delegateActionReport("Получаем список сообщений");

                HTabLoggingAppendTextPars tlatPars = data as HTabLoggingAppendTextPars;
                if (tlatPars.rows.Equals(string.Empty) == true)
                {
                    TabLoggingPositionText();
                    //textBoxLog.Focus();
                    //textBoxLog.AutoScrollOffset = new Point (0, 0);
                }
                else
                {
                    //int[] arTypeLogMsgCounter = new int[dgvFilterTypeMessage.Rows.Count];
                    List<int> listIdTypeMessages = LogParse.s_IdTypeMessages.ToList();//создание списка с идентификаторами типов сообщений

                    //Получение массива состояний CheckBox'ов на DataGridView с типами сообщений
                    bool[] arCheckedTypeMessages = new bool[dgvFilterTypeMessage.Rows.Count];
                    List<bool> check = new List<bool>();
                    dgvFilterTypeMessage.Checked(ref check);
                    for (int i = 0; i < dgvFilterTypeMessage.Rows.Count; i++)
                    {
                        arCheckedTypeMessages[i] = check[i];
                    }
                    //Преобразование строки в массив строк с сообщениями
                    string[] messages = tlatPars.rows.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.ROW] }, StringSplitOptions.None);
                    string[] parts;
                    int indxTypeMessage = -1;

                    //Помещение массива строк с сообщениями в DataGridView с сообщениями
                    foreach (string text in messages)
                    {
                        parts = text.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None);
                        indxTypeMessage = listIdTypeMessages.IndexOf(Int32.Parse(parts[1]));

                        //Фильтрация сообщений в зависимости от включенных CheckBox'ов в DataGridView с типами сообщений
                        if (arCheckedTypeMessages[indxTypeMessage] == true)
                            dgvLogMessage.Rows.Add(parts);
                        else
                            ;
                    }

                    DateTime start_date = DateTime.Parse(dgvDatetimeStart.Rows[m_prevDatetimeRowIndex].Cells[1].Value.ToString());
                    DateTime stop_date = start_date.AddDays(1);
                    check.Clear();
                    updateCounter(dgvFilterTypeMessage, start_date, stop_date, get_users(m_tableUsers, dgvClient, false));
                }

                delegateReportClear(true);
            }
            catch (Exception E)
            {
                Logging.Logg().Exception(E, "PanelAnalyzer:TabLoggingAppendText () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        /// <summary>
        /// Метод заполнения таблицы с датой для фильтрации лог-сообщений
        /// </summary>
        void TabLoggingAppendDatetimeStart(string rows)
        {
            string[] strDatetimeStartRows = rows.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.ROW] }, StringSplitOptions.None);

            bool bRowChecked = false;

            foreach (string row in strDatetimeStartRows)
            {
                bRowChecked = bool.Parse(row.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None)[0]);

                if (bRowChecked == true)
                {
                    dgvDatetimeStart.SelectionChanged += dgvDatetimeStart_SelectionChanged;
                    m_prevDatetimeRowIndex = dgvDatetimeStart.Rows.Count;
                }
                else
                    ;

                dgvDatetimeStart.Rows.Add(new object[] { bRowChecked, row.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None)[1] });
            }

            if (bRowChecked == true)
            {
                dgvDatetimeStart.Rows[dgvDatetimeStart.Rows.Count - 1].Selected = bRowChecked;
                dgvDatetimeStart.FirstDisplayedScrollingRowIndex = dgvDatetimeStart.Rows.Count - 1;
            }
            else
                ;
        }

        /// <summary>
        /// Получение списка дата/время запуска приложения
        /// </summary>
        public void LogParseExit()
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
            //Нюанс для 'LogParse_File' и 'LogParse_DB' - "0" индекс в массиве типов сообщений (зрезервирован для "СТАРТ")
            //...для 'LogParse_File': m_tblLog содержит ВСЕ записи в файле
            //...для 'LogParse_DB': m_tblLog содержит ТОЛЛЬКО записи из БД с датами, за которые найдено хотя бы одно сообщение
            //      тип сообщения "СТАРТ" устанавливается "программно" (метод 'LogParse_DB::Thread_Proc')

            i = m_LogParse.Select(@"true" + m_chDelimeters[(int)INDEX_DELIMETER.ROW] + ((int)LogParse.INDEX_START_MESSAGE).ToString(), DateTime.MaxValue, DateTime.MaxValue, ref rows); //0 - индекс в массиве идентификаторов зарезервирован для сообщений типа "СТАРТ"
            for (i = 0; i < rows.Length; i++)
            {
                if (i == (rows.Length - 1))
                    rowChecked = true;
                else
                    ;

                strDatetimeStart += rowChecked.ToString()
                        + m_chDelimeters[(int)INDEX_DELIMETER.PART]
                        + rows[i]["DATE_TIME"].ToString()
                        + m_chDelimeters[(int)INDEX_DELIMETER.ROW];
            }

            if (strDatetimeStart.Length > 0)
            {
                strDatetimeStart = strDatetimeStart.Substring(0, strDatetimeStart.Length - m_chDelimeters[(int)INDEX_DELIMETER.ROW].Length);
                BeginInvoke(new DelegateStringFunc(TabLoggingAppendDatetimeStart), strDatetimeStart);
            }
            else
            {
                //dgvFilterTypeMessage.UpdateCounter(m_idListenerLoggingDB, DateTime.Today, DateTime.Today.AddDays(1), " ");
                updateCounter(dgvFilterTypeMessage, DateTime.Today, DateTime.Today.AddDays(1), "");
            }
                ;
        }

        /// <summary>
        /// Обновление списка лог-сообщений
        /// </summary>
        private void updatedgvLogMessages(object data)
        {
            DataRow[] rows = new DataRow[] { };
            int cntRow = m_LogParse.Select(ref rows);//получение количества строк лог-сообщений
            filldgvLogMessages(rows);
        }

        /// <summary>
        /// Формирование списка лог-сообщений
        /// </summary>
        private void filldgvLogMessages(object data)
        {
            //DateTime [] arDT = (DateTime [])data;
            object[] pars = (object[])data;
            DataRow[] rows = new DataRow[] { };
            int cntRow = -1;

            cntRow = selectLogMessage(((string)pars[0]), ((DateTime)pars[1]), ((DateTime)pars[1]).AddDays(1), ref rows);

            filldgvLogMessages(rows);
        }

        /// <summary>
        /// Вызов метода заполнения таблицы с лог-сообщениями
        /// </summary>
        /// <param name="rows"></param>
        private void filldgvLogMessages(DataRow[] rows)
        {
            DelegateObjectFunc delegateAppendText = new DelegateObjectFunc(TabLoggingAppendText);
            string strRowsTodgvLogMessages = string.Empty;

            if (rows.Length > 0)
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    if (((i % 1000) == 0) && (strRowsTodgvLogMessages.Length > m_chDelimeters[(int)INDEX_DELIMETER.ROW].Length))
                    {
                        //Сбор строки из лог-сообщений
                        strRowsTodgvLogMessages = strRowsTodgvLogMessages.Substring(0, strRowsTodgvLogMessages.Length - m_chDelimeters[(int)INDEX_DELIMETER.ROW].Length);
                        //Асинхронное выполнение метода заполнения DataGridView с лог-сообщениями
                        this.BeginInvoke(delegateAppendText, new HTabLoggingAppendTextPars(strRowsTodgvLogMessages));
                        //this.BeginInvoke(new DelegateObjectFunc(TabLoggingAppendText), strRowsTodgvLogMessages);
                        strRowsTodgvLogMessages = string.Empty;
                    }
                    else

                        try
                        {
                            strRowsTodgvLogMessages += getTabLoggingTextRow(rows[i]) + m_chDelimeters[(int)INDEX_DELIMETER.ROW];

                        }
                        catch (Exception e)
                        {
                            Logging.Logg().Exception(e, "PanelAnalyzer:filldgvLogMessages () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                }

                if (strRowsTodgvLogMessages.Length > m_chDelimeters[(int)INDEX_DELIMETER.ROW].Length)
                {
                    //Остаток...                    
                    strRowsTodgvLogMessages = strRowsTodgvLogMessages.Substring(0, strRowsTodgvLogMessages.Length - m_chDelimeters[(int)INDEX_DELIMETER.ROW].Length);
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

            foreach (DataGridViewRow row in dgvFilterTypeMessage.Rows)
            {
                if (((DataGridViewCheckBoxCell)row.Cells[0]).Value.Equals(true) == true)
                {
                    strRes[0] += dgvFilterTypeMessage.Rows.IndexOf(row) + m_chDelimeters[(int)INDEX_DELIMETER.PART];
                    cntTrue++;
                }
                else
                    if (((DataGridViewCheckBoxCell)row.Cells[0]).Value.Equals(false) == true)
                    {
                        strRes[1] += dgvFilterTypeMessage.Rows.IndexOf(row) + m_chDelimeters[(int)INDEX_DELIMETER.PART];
                    }
                    else
                        ;
            }

            if (strRes[0].Length > 0)
            {
                indxRes = 0;

                if (cntTrue == dgvFilterTypeMessage.Rows.Count)
                    //Все типы "отмечены"
                    strRes[0] = string.Empty;
                else
                {
                    strRes[0] = true.ToString() + m_chDelimeters[(int)INDEX_DELIMETER.ROW] + strRes[0];
                    strRes[0] = strRes[0].Substring(0, strRes[0].Length - m_chDelimeters[(int)INDEX_DELIMETER.PART].Length);
                }
            }
            else
            {
                indxRes = 1;

                strRes[1] = false.ToString() + m_chDelimeters[(int)INDEX_DELIMETER.ROW] + strRes[1];
                strRes[1] = strRes[1].Substring(0, strRes[1].Length - m_chDelimeters[(int)INDEX_DELIMETER.PART].Length);
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

            DateTime dtBegin = DateTime.Parse(dgvDatetimeStart.Rows[m_prevDatetimeRowIndex].Cells[1].Value.ToString())
                , dtEnd = DateTime.MaxValue;
            if ((m_prevDatetimeRowIndex + 1) < dgvDatetimeStart.Rows.Count)
                dtEnd = DateTime.Parse(dgvDatetimeStart.Rows[m_prevDatetimeRowIndex + 1].Cells[1].Value.ToString());
            else
                ;

            Thread threadFilldgvLogMessages = new Thread(new ParameterizedThreadStart(filldgvLogMessages));
            threadFilldgvLogMessages.IsBackground = true;
            threadFilldgvLogMessages.Start(new object[] { getIndexTypeMessages(), dtBegin, dtEnd, bClearTypeMessageCounter });
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
            public static int[] s_IdTypeMessages;

            /// <summary>
            /// Список типов сообщений
            /// </summary>
            public static string[] s_DESC_LOGMESSAGE;

            private Thread m_thread;
            protected DataTable m_tableLog;
            protected DataTable m_tableStatMessage;
            Semaphore m_semAllowed;
            //bool m_bAllowed;

            public DelegateFunc Exit;

            public LogParse()
            {
                m_semAllowed = new Semaphore(1, 1);

                m_tableLog = new DataTable("ContentLog");
                DataColumn[] cols = new DataColumn[] { new DataColumn("DATE_TIME", typeof(DateTime)),
                                                    new DataColumn("TYPE", typeof(Int32)),
                                                    new DataColumn ("MESSAGE", typeof (string)) };
                m_tableLog.Columns.AddRange(cols);

                m_thread = null;
            }

            /// <summary>
            /// Инициализация потока разбора лог-файла
            /// </summary>
            public void Start(object text)
            {
                m_semAllowed.WaitOne();

                m_thread = new Thread(new ParameterizedThreadStart(thread_Proc));
                m_thread.IsBackground = true;
                m_thread.Name = "Разбор лог-файла";

                m_thread.CurrentCulture =
                m_thread.CurrentUICulture =
                    ProgramBase.ss_MainCultureInfo;

                m_tableLog.Clear();

                m_thread.Start(text);
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
            /// <returns></returns>
            protected string addingWhereTypeMessage(string nameFieldTypeMessage, string strIndxType)
            {
                string strRes = string.Empty;

                //if (!(indxType < 0))
                if (strIndxType.Equals(string.Empty) == false)
                {
                    List<int> listIndxType;
                    //listIndxType = strIndxType.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.ROW] }, StringSplitOptions.None)[1].Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None).ToList().ConvertAll<int>(new Converter<string, int>(delegate(string strIn) { return Int32.Parse(strIn); }));
                    string[] pars = strIndxType.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.ROW] }, StringSplitOptions.None);

                    if (pars.Length == 2)
                    {
                        bool bUse = false;
                        //if ((pars[0].Equals(true.ToString()) == true) || (pars[0].Equals(false.ToString()) == true))
                        if (bool.TryParse(pars[0], out bUse) == true)
                        {
                            if (pars[1].Length > 0)
                            {
                                listIndxType = pars[1].Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None).ToList().ConvertAll<int>(new Converter<string, int>(delegate(string strIn) { return Int32.Parse(strIn); }));

                                strRes += nameFieldTypeMessage + @" ";

                                if (bUse == false)
                                    strRes += @"NOT ";
                                else
                                    ;

                                strRes += @"IN (";

                                foreach (int indx in listIndxType)
                                    strRes += s_IdTypeMessages[indx] + @",";

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
            public int Select(ref DataRow[] res)
            {
                res = m_tableLog.Select(string.Empty, "DATE_TIME");

                return res.Length > 0 ? res.Length : -1;
            }

            //public int Select(int indxType, DateTime beg, DateTime end, ref DataRow[] res)
            /// <summary>
            /// Выборка лог-сообщений
            /// </summary>
            /// <param name="strIndxType">Индекс типа сообщения</param>
            /// <param name="beg">Начало периода</param>
            /// <param name="end">Оончание периода</param>
            /// <param name="res">Массив строк лог-сообщений</param>
            /// <returns></returns>
            public int Select(string strIndxType, DateTime beg, DateTime end, ref DataRow[] res)
            {
                int iRes = -1;
                string where = string.Empty;

                //m_tableLog.Clear();

                if (beg.Equals(DateTime.MaxValue) == false)
                {
                    where = "DATE_TIME>='" + beg.ToString("yyyyMMdd HH:mm:ss") + "'";
                    if (end.Equals(DateTime.MaxValue) == false)
                        where += " AND DATE_TIME<'" + end.ToString("yyyyMMdd HH:mm:ss") + "'";
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

                res = m_tableLog.Select(where, "DATE_TIME");

                return res.Length > 0 ? res.Length : -1;
            }
        }
        
        #endregion

        #region Обработчики событий для элементов панели лог-сообщений

        /// <summary>
        /// Обработчик выбора активных/неактивных пользователей
        /// </summary>
        private void dgvFilterActives_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dgvFilterActives.Rows[e.RowIndex].Cells[0].Value = !bool.Parse(dgvFilterActives.Rows[e.RowIndex].Cells[0].Value.ToString());
            get_FilteredUsers();
        }

        /// <summary>
        /// Обработчик выбора ролей пользователей
        /// </summary>
        private void dgvFilterRoles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dgvFilterRoles.Rows[e.RowIndex].Cells[0].Value = !bool.Parse(dgvFilterRoles.Rows[e.RowIndex].Cells[0].Value.ToString());
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

            m_timerChecked.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            for (i = 0; i < m_tableRoles.Rows.Count; i++)
            {
                if (bool.Parse(dgvFilterRoles.Rows[i].Cells[0].Value.ToString()) == false)
                {
                    if (where.Equals(string.Empty) == true)
                    {
                        where = "ID_ROLE NOT IN (";
                    }
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

            if (dgvFilterActives.Rows[0].Cells[0].Value == dgvFilterActives.Rows[1].Cells[0].Value)
                if (bool.Parse(dgvFilterActives.Rows[0].Cells[0].Value.ToString()) == true)
                //Отображать всех...
                {
                }
                else
                //Пустой список...
                {
                    m_tableUsers.Clear();
                }
            else
            {
                bool[] arbActives;
                procChecked(out arbActives, out err);

                if ((!(arbActives == null)) && (err == 0))
                {
                    List<int> listIndexToRemoveUsers = new List<int>();

                    for (i = 0; (i < m_tableUsers.Rows.Count) && (i < arbActives.Length); i++)
                    {
                        if (((arbActives[i] == true) && (bool.Parse(dgvFilterActives.Rows[0].Cells[0].Value.ToString()) == false))
                            || ((arbActives[i] == false) && (bool.Parse(dgvFilterActives.Rows[1].Cells[0].Value.ToString()) == false)))
                        {
                            listIndexToRemoveUsers.Add(i);
                        }
                        else
                            ;
                    }

                    if (listIndexToRemoveUsers.Count > 0)
                    {
                        listIndexToRemoveUsers.Sort(delegate(int i1, int i2) { return i1 > i2 ? -1 : 1; });

                        //Удалить обработанные сообщения
                        foreach (int indx in listIndexToRemoveUsers)
                            m_tableUsers.Rows.RemoveAt(indx);

                        m_tableUsers.AcceptChanges();
                    }
                    else
                        ;
                }
                else
                    ;
            }

            FillDataGridViews(ref dgvClient, m_tableUsers, c_list_sorted, err);

            m_timerChecked.Change(0, System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Обработчик выбора типа лог сообщений
        /// </summary>
        private void dgvFilterTypeMessage_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                dgvFilterTypeMessage.Rows[e.RowIndex].Cells[0].Value = !bool.Parse(dgvFilterTypeMessage.Rows[e.RowIndex].Cells[0].Value.ToString());

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
            dgvClient_SelectionChanged(null, null);
        }

        /// <summary>
        /// Обработчик выбора Даты/Времени для лог сообщений
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">Аргумент события</param>
        protected void dgvDatetimeStart_SelectionChanged(object sender, EventArgs e)
        {
            int rowIndex = dgvDatetimeStart.SelectedRows[0].Index;

            dgvDatetimeStart.Rows[m_prevDatetimeRowIndex].Cells[0].Value = false;
            dgvDatetimeStart.Rows[rowIndex].Cells[0].Value = true;
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
        /// Обновление статистики сообщений из лога за выбранный период
        /// </summary>
        private void dgvUser_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dgvUser.Rows[e.RowIndex].Cells[0].Value = !bool.Parse(dgvUser.Rows[e.RowIndex].Cells[0].Value.ToString());//фиксация положения



            updateCounter(dgvMessage, HDateTime.ToMoscowTimeZone(StartCalendar.Value.Date), HDateTime.ToMoscowTimeZone(StopCalendar.Value.Date), get_users(m_tableUsers_stat, dgvUser, true));//обновление списка со статистикой сообщений
        }
        
        /// <summary>
        /// Обновление списка пользователей при выборе ролей
        /// </summary>
        private void dgvRole_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            m_tableUsers_stat = m_tableUsers_stat_unfiltered.Copy();

            int i = -1, err = 0;
            string where = string.Empty;

            if (e.ColumnIndex == 0)
            {
                dgvRole.Rows[e.RowIndex].Cells[0].Value = !bool.Parse(dgvRole.Rows[e.RowIndex].Cells[0].Value.ToString());//фиксация значения

                for (i = 0; i < m_tableRoles.Rows.Count; i++)//перебор строк таблицы
                {
                    if (bool.Parse(dgvRole.Rows[i].Cells[0].Value.ToString()) == false)
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

                FillDataGridViews(ref dgvUser, m_tableUsers_stat, c_list_sorted, err, true);//Отображение пользователей в DataGridView

                updateCounter(dgvMessage, HDateTime.ToMoscowTimeZone(StartCalendar.Value.Date), HDateTime.ToMoscowTimeZone(StopCalendar.Value.Date), get_users(m_tableUsers_stat, dgvUser, true));//обновление списка со статистикой сообщений

            }
        }

        /// <summary>
        /// Обновление списка пользователей при выборе начальной даты
        /// </summary>
        private void startCalendar_ChangeValue(object sender, EventArgs e)
        {
            updateCounter(dgvMessage, HDateTime.ToMoscowTimeZone(StartCalendar.Value.Date), HDateTime.ToMoscowTimeZone(StopCalendar.Value.Date), get_users(m_tableUsers_stat, dgvUser, true));//обновление списка со статистикой сообщений
        }

        /// <summary>
        /// Обновление списка пользователей при выборе конечной даты
        /// </summary>
        private void stopCalendar_ChangeValue(object sender, EventArgs e)
        {
            updateCounter(dgvMessage, HDateTime.ToMoscowTimeZone(StartCalendar.Value.Date), HDateTime.ToMoscowTimeZone(StopCalendar.Value.Date), get_users(m_tableUsers_stat, dgvUser, true));//обновление списка со статистикой сообщений
        }

        #endregion

        #region Панель вкладок

        /// <summary>
        /// Абстрактный метод для получения из БД списка активных вкладок для выбранного пользователя
        /// </summary>
        /// <param name="user">Идентификатор пользователя для выборки</param>
        protected abstract void fill_active_tabs(int user);

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

                if (Equals (listTabVisible, null) == false)
                    listTabVisible.ForeColor = value;
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

                if (Equals (listTabVisible, null) == false)
                    listTabVisible.BackColor = BackColor == SystemColors.Control ? SystemColors.Window : BackColor;
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
                
                InitializeComponent();//инициализация компонентов

                if (_type == TYPE.WITH_CHECKBOX)
                {
                    this.Columns.Add(check_state);
                }

                if (_type == TYPE.WITH_CHECKBOX || _type == TYPE.WITHOUT_CHECKBOX)
                {
                    this.Columns.Add(type_message);
                    this.Columns.Add(count_message);
                }
                
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

                fillTypeMessage(LogParse.s_DESC_LOGMESSAGE);//заполнение DataGridView типами сообщений

            }

            /// <summary>
            /// Получение индекса последнего столбца
            /// </summary>
            private int lastIndex { get { return this.ColumnCount - 1; } }

            /// <summary>
            /// Обновление списка со статистикой сообщений 
            /// </summary>
            /// <param name="beg">Начальная дата</param>
            /// <param name="end">Конечая дата</param>
            /// <param name="users">Список пользователей</param>
            public void UpdateCounter(object objSrcMessages, DateTime beg, DateTime end, string users)
            {
                int iRes = -1;
                string where = string.Empty;
                int iListenerId = (int)objSrcMessages;
                DataTable table_statMessage;//таблица с количествами сообщений каждого типа

                if (users != "")
                {
                    updateTypeMessage(LogParse.s_DESC_LOGMESSAGE);//Обнуление счётчиков сообщений на панели статистики

                    #region Фомирование и выполнение запроса для получения количества сообщений разного типа

                    string query = @"SELECT ([ID_LOGMSG]),COUNT (*) as [COUNT] FROM [dbo].[logging] where ";

                    if (beg.Equals(DateTime.MaxValue) == false)
                    {
                        //диапазон даты/времени
                        where = "DATETIME_WR BETWEEN'" + beg.ToString("yyyyMMdd HH:mm:ss") + "'";
                        if (end.Equals(DateTime.MaxValue) == false)
                        {
                            where += " AND '" + end.ToString("yyyyMMdd HH:mm:ss") + "'";
                        }
                    }

                    if (users.Equals(string.Empty) == false)
                    {
                        //добавление идентификаторов пользователей к условиям выборки
                        where += "AND ID_USER in (" + users + ")";
                        where += "group by([ID_LOGMSG]) ORDER BY ID_LOGMSG";
                        query += where;

                        //DbConnection connLoggingDBn = DbSources.Sources().GetConnection(iListenerId, out iRes);

                        table_statMessage =
                            //DbTSQLInterface.Select(ref connLoggingDBn, query, null, null, out iRes)
                            DbTSQLDataSource.DataSource().Select (query, null, null, out iRes)
                            ;

                        if (table_statMessage.Rows.Count == 0)
                        {
                            updateTypeMessage(LogParse.s_DESC_LOGMESSAGE);//Обнуление счётчиков сообщений на панели статистики
                        }
                        fillDataGridViewsMessage(table_statMessage, @"COUNT", 0, true);//Заполнение таблицы со счётчиками на панели статистики

                    }
                    else
                    {
                        updateTypeMessage(LogParse.s_DESC_LOGMESSAGE);//Обнуление счётчиков сообщений на панели статистики
                    }
                    ;

                    #endregion
                }
                else
                    updateTypeMessage(LogParse.s_DESC_LOGMESSAGE);//Обнуление счётчиков сообщений на панели статистики
            }

            /// <summary>
            /// Возвращает список состояния CheckBox'ов
            /// </summary>
            /// <param name="Check">Возвращает список состояния CheckBox'ов</param>
            public void Checked(ref List<bool> Check)
            {
                if (_type == TYPE.WITH_CHECKBOX)
                {
                    for (int i = 0; i < this.Rows.Count; i++)
                    {
                        Check.Add((bool)this.Rows[i].Cells[lastIndex - 2].Value);//добавление состояния CheckBox'а в список
                    }
                }
            }

            /// <summary>
            /// Обновление списка со статистикой сообщений 
            /// </summary>
            /// <param name="src">Таблица с количеством сообщений каждого типа</param>
            /// <param name="nameField">Наименование колонки из которой брать данные</param>
            /// <param name="run">Для выполнения установить 0</param>
            /// <param name="checkDefault">Значения CheckBox'ов по умолчанию</param>
            private void fillDataGridViewsMessage(DataTable src, string nameField, int run, bool checkDefault = false)
            {
                if (run == 0)
                {
                    if (src.Rows.Count > 0)
                    {
                        int b = 0;
                        for (int i = 0; i < this.Rows.Count; i++)//Перебор строк DataGridView
                        {
                            if (b < src.Rows.Count)
                            {
                                if (Convert.ToInt32(src.Rows[b]["ID_LOGMSG"].ToString()) - 1 == i)
                                {
                                    this.Rows[i].Cells[lastIndex].Value = src.Rows[b][nameField].ToString();
                                    b++;
                                }
                            }
                            else
                            {
                                this.Rows[i].Cells[lastIndex].Value = "0";
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Заполнение DataGridView типами сообщений
            /// </summary>
            /// <param name="strTypeMessages">Массив с типами сообщений</param>
            protected void fillTypeMessage(string[] strTypeMessages)
            {
                this.Rows.Add(strTypeMessages.Length);

                for (int i = 0; i < strTypeMessages.Length; i++)
                {
                    if(_type == TYPE.WITH_CHECKBOX)
                        this.Rows[i].Cells[lastIndex - 2].Value = true;

                    this.Rows[i].Cells[lastIndex - 1].Value = strTypeMessages[i];
                    this.Rows[i].Cells[lastIndex].Value = 0;
                }
            }

            /// <summary>
            /// Обнуление статистики типов сообщений в DataGridView 
            /// </summary>
            /// <param name="strTypeMessages">Массив с типами сообщений</param>
            protected void updateTypeMessage(string[] strTypeMessages)
            {
                for (int i = 0; i < strTypeMessages.Length; i++)
                {
                    this.Rows[i].Cells[lastIndex].Value = 0;
                }
            }
        }
    }

    public class PanelAnalyzer_DB : PanelAnalyzer
    {
        private static string _nameShrMainDB = "MAIN_DB";

        private ConnectionSettings _connSettMainDB;

        private DelegateIntFunc delegateConnect;
        
        private DelegateFunc delegateErrorConnect;

        public PanelAnalyzer_DB(List<StatisticCommon.TEC> tec, Color foreColor, Color backColor)
            : base(tec, foreColor, backColor)
        {
            m_listTEC = tec;

            checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].CheckedChanged += new EventHandler(checkBox_click);
            checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC].CheckedChanged += new EventHandler(checkBox_click);
            checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].CheckedChanged += new EventHandler(checkBox_click);
            checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG].CheckedChanged += new EventHandler(checkBox_click);
            checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].CheckedChanged += new EventHandler(checkBox_click);
        }

        #region Наследуемые методы

        /// <summary>
        /// Метод для получения лога
        /// </summary>
        /// <returns>Возвращает лог сообщений</returns>
        protected override LogParse newLogParse() { return new LogParse_DB(); }

        /// <summary>
        /// Запуск таймера обновления данных
        /// </summary>
        public override void Start()
        {
            int err = -1, 
                idMainDB = -1;

            base.Start ();

            DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings ();
            DbTSQLConfigDatabase.DbConfig().Register();

            if (DbTSQLConfigDatabase.DbConfig ().ListenerId > 0) {
                idMainDB = Int32.Parse (DbTSQLConfigDatabase.DbConfig().Select (@"SELECT [VALUE] FROM [setup] WHERE [KEY]='" + @"Main DataSource" + @"'", null, null, out err).Rows [0] [@"VALUE"].ToString ());
                DataTable tblConnSettMainDB = ConnectionSettingsSource.GetConnectionSettings (DbTSQLConfigDatabase.DbConfig ().ListenerId, idMainDB, -1, out err);

                if ((tblConnSettMainDB.Columns.Count > 0)
                    && (tblConnSettMainDB.Rows.Count > 0))
                // "-1" - значит будет назначени идентификатор из БД
                    _connSettMainDB = new ConnectionSettings (tblConnSettMainDB.Rows [0], -1);
                else
                    throw new Exception ($"PanelAnalyzer_DB::Start () - нет параметров соединения с БД значений ID={idMainDB}...");

                base.Start ();
            } else
                throw new Exception (@"PanelAnalyzer_DB::Start () - нет соединения с БД конфигурации...");

            DbTSQLConfigDatabase.DbConfig ().UnRegister ();

            //dgvFilterTypeMessage.UpdateCounter(m_idListenerLoggingDB, DateTime.MinValue, DateTime.MaxValue, "");

            //dgvMessage.UpdateCounter(m_idListenerLoggingDB, StartCalendar.Value.Date, StopCalendar.Value.Date, get_users(m_tableUsers_stat, dgvUser, true));

        }

        ///// <summary>
        ///// Остановка таймера обновления данных
        ///// </summary>
        //public override void Stop()
        //{
        //    DbTSQLDataSource.DataSource ().UnRegister ();

        //    base.Stop();
        //}

        /// <summary>
        /// Проверка запуска пользователем ПО
        /// </summary>
        /// <param name="arbActives">Массив занчений активности каждого пользователя</param>
        /// <param name="err">Ошибка</param>
        protected override void procChecked(out bool[] arbActives, out int err)
        {
            delegateActionReport("Получаем список активных пользователей");

            err = 0;
            arbActives = null;
            int i = -1;

            DbTSQLDataSource.DataSource ().SetConnectionSettings (_connSettMainDB);
            DbTSQLDataSource.DataSource().Register(_nameShrMainDB);
            if (err == 0)
            {
                DataTable tblMaxDatetimeWR = DbTSQLDataSource.DataSource().Select(@"SELECT [ID_USER], MAX([DATETIME_WR]) as MAX_DATETIME_WR FROM logging GROUP BY [ID_USER] ORDER BY [ID_USER]", null, null, out err);
                DataRow[] rowsMaxDatetimeWR;
                if (err == 0)
                {
                    arbActives = new bool[m_tableUsers.Rows.Count];

                    for (i = 0; (i < m_tableUsers.Rows.Count) && (m_bThreadTimerCheckedAllowed == true); i++)
                    {
                        //Проверка активности
                        rowsMaxDatetimeWR = tblMaxDatetimeWR.Select(@"[ID_USER]=" + m_tableUsers.Rows[i][@"ID"]);

                        if (rowsMaxDatetimeWR.Length == 0)
                        { //В течении 2-х недель нет ни одного запуска на выполнение ППО
                        }
                        else
                        {
                            if (rowsMaxDatetimeWR.Length > 1)
                            { //Ошибка
                            }
                            else
                            {
                                //Обрабатываем...
                                arbActives[i] = (HDateTime.ToMoscowTimeZone(DateTime.Now) - DateTime.Parse(rowsMaxDatetimeWR[0][@"MAX_DATETIME_WR"].ToString())).TotalSeconds < 66;
                            }
                        }
                    }
                }
                else
                    err = -2; //Ошибка при выборке данных...
            }
            else
                err = -1; //Нет соединения с БД...

            DbTSQLDataSource.DataSource ().UnRegister ();

            delegateReportClear (true);
        }

        /// <summary>
        /// Запись значений активности в CheckBox на DataGridView с пользователями
        /// </summary>
        /// <param name="obj"></param>
        protected override void procChecked(object obj)
        {
            int err = -1
                , i = -1
                , msecSleep = System.Threading.Timeout.Infinite;
            bool[] arbActives;
            procChecked(out arbActives, out err);

            if (!(arbActives == null) && (err == 0))
            {
                for (i = 0; (i < m_tableUsers.Rows.Count) && (m_bThreadTimerCheckedAllowed == true) && (i < arbActives.Length); i++)
                    dgvClient.Rows[i].Cells[0].Value = arbActives[i];

                msecSleep = MSEC_TIMERCHECKED_STANDARD;
            }
            else
                msecSleep = MSEC_TIMERCHECKED_FORCE; //Нет соединения с БД...

            //Вариант №0
            m_timerChecked.Change(msecSleep, System.Threading.Timeout.Infinite);
            ////Вариант №1
            //if (! (m_timerChecked.Interval == msecSleep)) m_timerChecked.Interval = msecSleep; else ;
            //Debug.WriteLine("procChecked () - date/time:" + DateTime.Now.ToString());
        }

        /// <summary>
        /// Метод для получения из БД списка активных вкладок для выбранного пользователя
        /// </summary>
        /// <param name="user">Пользователь для выборки</param>
        protected override void fill_active_tabs(int user)
        {
            int err = -1;
            int iRes = -1;
            string[] us ;
            string where = string.Empty;
            List<int> ID_tabs = new List<int>();
            DataTable tableProfileTabs;

            delegateActionReport ("Получение активных вкладок пользователя");

            #region Фомирование и выполнение запроса для получения списка открытых вкладок у пользователя

            string query = @"SELECT [ID_EXT],[IS_ROLE],[ID_UNIT],[VALUE] FROM [techsite_cfg-2.X.X].[dbo].[profiles] where ";

            if (user.Equals(null) == false)
            {
                //Условие для поиска вкладок
                where = "((ID_EXT = " + (int)user + " and [IS_ROLE] =0) or (id_ext=(select [ID_ROLE] from dbo.[users] where id=" + user + ") and is_role=1)) AND ID_UNIT IN(" + (int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE + ", " + (int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS + ")";
                query += where;

                tableProfileTabs = DbTSQLConfigDatabase.DbConfig().Select(query, out iRes);

                if (tableProfileTabs.Rows.Count > 0)
                {
                    //Список строк с используемыми вкладками
                    DataRow[] table_role0 = tableProfileTabs.Select("IS_ROLE=0 and ID_UNIT in (" + (int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE + ", " + (int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS + ")");
                    string role0 = string.Empty;

                    //Список строк с вкладками по умолчанию
                    DataRow[] table_role1 = tableProfileTabs.Select("IS_ROLE=1 and ID_UNIT in (" + (int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE + ", " + (int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS + ")");
                    string role1 = string.Empty;

                    string tabs = string.Empty;

                    if (table_role0.Length <= 2 & table_role0.Length >= 1)//проверка выборки для используемых вкладок
                    {
                        for (int i = 0; i < table_role0.Length; i++)
                        {
                            if ((int)table_role0[i]["ID_UNIT"] == (int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE)
                            {
                                if (IsNumberContains(table_role0[i]["VALUE"].ToString()) == true)
                                {
                                    tabs += table_role0[i]["VALUE"].ToString();
                                }
                            }
                            if ((int)table_role0[i]["ID_UNIT"] == (int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS)
                            {
                                if (tabs.Equals(string.Empty) == false)
                                {
                                    tabs += ";";
                                }
                                if (IsNumberContains(table_role0[i]["VALUE"].ToString()) == true)
                                {
                                    tabs += table_role0[i]["VALUE"].ToString();
                                }
                            }
                        }
                    }
                    else
                        if (table_role1.Length <= 2 & table_role1.Length >= 1)//проверка выборки для вкладок по умолчанию
                        {
                            for (int i = 0; i < table_role1.Length; i++)
                            {
                                if ((int)table_role1[i]["ID_UNIT"] == (int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE)
                                {
                                    if (IsNumberContains(table_role1[i]["VALUE"].ToString()) == true)
                                    {
                                        tabs += table_role1[i]["VALUE"].ToString();
                                    }
                                }
                                if ((int)table_role1[i]["ID_UNIT"] == (int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS)
                                {
                                    if (IsNumberContains(table_role1[i]["VALUE"].ToString()) == true)
                                    {
                                        tabs += table_role1[i]["VALUE"].ToString();
                                    }
                                }
                            }
                        }
                        else
                        {
                            Logging.Logg().Error(@"PanelAnalyzer::PanelAnalyzer_DB () - нет данных по вкладкам для выбранного пользователя с ID = " + user, Logging.INDEX_MESSAGE.NOT_SET);
                        }

                    us = tabs.Split(';');//создание массива с ID вкладок

                    for (int i = 0; i < us.Length; i++)
                    {
                        if (IsPunctuationContains(us[i]) == true)//разбор в случае составного идентификатора
                        {
                            string[] count;
                            count = us[i].Split('=');
                            ID_tabs.Add(Convert.ToInt32(count[0]));//добавление идентификатора в список
                        }
                        else
                        {
                            if (us[i] != "" & us[i] != " ")//фильтрация пустых идентификаторов
                            {
                                if (IsNumberContains(us[i])==true)
                                ID_tabs.Add(Convert.ToInt32(us[i]));//добавление идентификатора в список
                            }
                        }
                    }

                    fillListBoxTabVisible(ID_tabs);//Заполнение ListBox'а активными вкладками
                }
            }
            else
                ;

            #endregion

            delegateReportClear (true);
        }

        /// <summary>
        /// Заполнение ListBox активными вкладками
        /// </summary>
        /// <param name="ID_tabs">Список активных вкладок</param>
        protected override void fillListBoxTabVisible(List<int> ID_tabs)
        {
            int tec_indx = 0,
               comp_indx = 0,
               across_indx = -1;
            bool start = false;

            for (int i = 0; i < ID_tabs.Count; i++)
            {
                FormChangeMode.MODE_TECCOMPONENT tec_component = TECComponentBase.Mode(ID_tabs[i]);//получение типа вкладки

                if (checkBoxs[(int)tec_component].Checked == true)//проверка состояния фильтра для вкладки
                    start = true;
                else
                    start = false;

                if (start == true)
                {
                    if (!(m_listTEC == null))
                        foreach (StatisticCommon.TEC t in m_listTEC)
                        {
                            tec_indx++;
                            comp_indx = 0;

                            if (t.m_id.ToString() == ID_tabs[i].ToString() & tec_component == FormChangeMode.MODE_TECCOMPONENT.TEC)
                                listTabVisible.Items.Add(t.name_shr);//добавление ТЭЦ в ListBox

                            across_indx++;

                            if (t.list_TECComponents.Count > 0)
                            {
                                foreach (StatisticCommon.TECComponent g in t.list_TECComponents)
                                {
                                    comp_indx++;

                                    across_indx++;

                                    if (g.m_id.ToString() == ID_tabs[i].ToString())
                                        listTabVisible.Items.Add(t.name_shr + " - " + g.name_shr);//Добавление вкладки в ListBox
                                }
                            }
                            else
                                ;
                        }
                    else
                        ;
                }
            }
        }

        /// <summary>
        /// Разорвть соединение с БД
        /// </summary>
        protected override void disconnect() { }

        /// <summary>
        /// Старт разбора лог-сообщений
        /// </summary>
        /// <param name="id">ID пользователя</param>
        protected override void startLogParse(string id)
        {
            dgvDatetimeStart.SelectionChanged -= dgvDatetimeStart_SelectionChanged;

            // завершается по выполнении потоковой ф-и 'm_LogParse::thread_Proc'
            DbTSQLDataSource.DataSource ().SetConnectionSettings (_connSettMainDB);
            DbTSQLDataSource.DataSource ().Register (_nameShrMainDB);

            m_LogParse.Start(m_chDelimeters[(int)INDEX_DELIMETER.PART].Length
                    + m_chDelimeters[(int)INDEX_DELIMETER.PART]
                    + DbTSQLDataSource.DataSource().ListenerId.ToString()
                    + m_chDelimeters[(int)INDEX_DELIMETER.PART] + id);
        }

        /// <summary>
        /// Выборка лог-сообщений по параметрам
        /// </summary>
        /// <param name="type">Тип сообщений</param>
        /// <param name="beg">Начало периода</param>
        /// <param name="end">Окончание периода</param>
        /// <param name="rows">Массив сообщений</param>
        /// <returns>Возвращает количество строк в выборке</returns>
        protected override int selectLogMessage(string type, DateTime beg, DateTime end, ref DataRow[] rows)
        {
            int iRes = -1;

            DbTSQLDataSource.DataSource ().SetConnectionSettings (_connSettMainDB);
            DbTSQLDataSource.DataSource ().Register (_nameShrMainDB);

            iRes = (m_LogParse as LogParse_DB).Select (DbTSQLDataSource.DataSource ().ListenerId, type, beg, end, ref rows);

            DbTSQLDataSource.DataSource ().UnRegister ();

            return iRes;
        }

        /// <summary>
        /// Получение первой строки лог-сообщений
        /// </summary>
        /// <param name="r">Строка из таблицы</param>
        /// <returns>Вызвращает строку string</returns>
        protected override string getTabLoggingTextRow(DataRow r)
        {
            string strRes = string.Empty;

            DateTime? dtVal = null;

            if (r["DATE_TIME"] is DateTime)
            {
                dtVal = r["DATE_TIME"] as DateTime?;


                strRes = string.Join(m_chDelimeters[(int)INDEX_DELIMETER.PART].ToString()
                                    , new string[] {
                                            ((DateTime)dtVal).ToString (@"HH:mm:ss.fff")
                                            , r["TYPE"].ToString ()
                                            , r["MESSAGE"].ToString()
                                        });
            }
            else
                ;

            return strRes;
        }

        #endregion

        /// <summary>
        /// Соединение для чтения лог-сообщений
        /// </summary>
        /// <param name="id">ID пользователя</param>
        private void connectToLogRead(int id)
        {
            startLogParse(id.ToString());
        }

        /// <summary>
        /// Вызов делегата для начала чтения лог-сообщений
        /// </summary>
        /// <param name="rowUser">Строка с данными пользователя</param>
        /// <param name="indxRowUser">Индекс строки с данными пользователя</param>
        private void getLogMsg(DataRow rowUser, int indxRowUser)
        {
            if (!(DbTSQLDataSource.DataSource ().ListenerId < 0))
                delegateConnect(Int32.Parse(rowUser[@"ID"].ToString()));
            else
                delegateErrorConnect();
        }

        /// <summary>
        /// Обновление счетчика типов сообщений
        /// </summary>
        /// <param name="dgv">DataGridView в которую поместить результаты</param>
        /// <param name="start_date">Начало периода</param>
        /// <param name="stop_date">Окончание периода</param>
        /// <param name="users">Список пользователей</param>
        protected override void updateCounter(DataGridView_LogMessageCounter dgv, DateTime start_date, DateTime stop_date, string users)
        {
            delegateActionReport?.Invoke("Обновление статистики сообщений");

            dgv.UpdateCounter(DbTSQLDataSource.DataSource ().ListenerId, start_date, stop_date, users);

            delegateReportClear?.Invoke (true);
        }

        private void logParse_OnThreadProcSelectCompleted ()
        {
            DbTSQLDataSource.DataSource ().UnRegister ();
        }

        /// <summary>
        /// Подключение к вкладке
        /// </summary>
        /// <param name="id">ID пользователя</param>
        private void connectToTab(int id)
        {
        }

        /// <summary>
        /// Ошибка подключения
        /// </summary>
        private void errorConnect()
        {
        }

        /// <summary>
        /// Наличие пунктуации в строке
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns>true-если есть</returns>
        static bool IsPunctuationContains(string input)
        {
            bool bRes = false;

            foreach (char c in input)
                if (Char.IsPunctuation(c))
                    bRes = true;

            return bRes;
        }

        /// <summary>
        /// Наличие цифр в строке
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns>true-если есть</returns>
        static bool IsNumberContains(string input)
        {
            bool bRes = false;

            foreach (char c in input)
                if (Char.IsNumber(c))
                    bRes = true;

            return bRes;
        }

        #region Обработчики событий

        /// <summary>
        /// Обработчик события выбора пользователя 
        /// </summary>
        protected override void dgvClient_SelectionChanged(object sender, EventArgs e)
        {
            if ((dgvClient.SelectedRows.Count > 0) && (!(dgvClient.SelectedRows[0].Index < 0)))
            {
                bool bUpdate = true;

                if ((dgvDatetimeStart.Rows.Count > 0) && (dgvDatetimeStart.SelectedRows[0].Index < (dgvDatetimeStart.Rows.Count - 1)))
                    if (e == null)
                        bUpdate = false;
                    else
                        ;
                else
                    ;

                if (bUpdate == true)
                {
                    //Останов потока разбора лог-файла пред. пользователя
                    m_LogParse.Stop();

                    dgvDatetimeStart.SelectionChanged -= dgvDatetimeStart_SelectionChanged;

                    //Очистить элементы управления с данными от пред. лог-файла
                    if (IsHandleCreated == true)
                        if (InvokeRequired == true)
                        {
                            BeginInvoke(new DelegateFunc(tabLoggingClearDatetimeStart));
                            BeginInvoke(new DelegateBoolFunc(tabLoggingClearText), true);
                        }
                        else
                        {
                            tabLoggingClearDatetimeStart();
                            tabLoggingClearText(true);
                        }
                    else
                        Logging.Logg().Error(@"FormMainAnalyzer_DB::dgvClient_SelectionChanged () - ... BeginInvoke (TabLoggingClearDatetimeStart, TabLoggingClearText) - ...", Logging.INDEX_MESSAGE.D_001);

                    delegateConnect = connectToLogRead;

                    delegateErrorConnect = errorConnect;

                    DbTSQLDataSource.DataSource ().SetConnectionSettings (_connSettMainDB);
                    DbTSQLDataSource.DataSource ().Register (_nameShrMainDB);

                    getLogMsg (m_tableUsers.Rows[dgvClient.SelectedRows[0].Index], dgvClient.SelectedRows[0].Index);

                    updateCounter(dgvFilterTypeMessage, DateTime.Today, DateTime.Today.AddDays(1), get_users(m_tableUsers, dgvClient, false));

                    DbTSQLDataSource.DataSource ().UnRegister ();

                    listTabVisible.Items.Clear();//очистка списка активных вкладок

                    DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings ();
                    DbTSQLConfigDatabase.DbConfig ().Register ();

                    fill_active_tabs ((int)m_tableUsers.Rows[dgvClient.SelectedRows[0].Index][0]);

                    DbTSQLConfigDatabase.DbConfig ().UnRegister ();

                }
                else
                    ;
            }
            else
                ;
        }

        /// <summary>
        /// Обработчик события изменения состояния CheckBox'ов
        /// </summary>
        protected void checkBox_click(object sender, EventArgs e)
        {
            listTabVisible.Items.Clear();

            fill_active_tabs((int)m_tableUsers.Rows[dgvClient.SelectedRows[0].Index][0]);
        }

        #endregion

        /// <summary>
        /// Класс для получения лог сообщений из БД
        /// </summary>
        class LogParse_DB : LogParse
        {
            /// <summary>
            /// Список идентификаторов типов сообщений
            /// </summary>
            public enum TYPE_LOGMESSAGE { START = LogParse.INDEX_START_MESSAGE, STOP, ACTION, DEBUG, EXCEPTION, EXCEPTION_DB, ERROR, WARNING, UNKNOWN, COUNT_TYPE_LOGMESSAGE };
            
            /// <summary>
            /// ID порльзователя
            /// </summary>
            private int m_idUser;

            public LogParse_DB()
                : base()
            {
                s_DESC_LOGMESSAGE = new string[] { "Запуск", "Выход",
                                                    "Действие", "Отладка", "Исключение",
                                                    "Исключение БД",
                                                    "Ошибка",
                                                    "Предупреждение",
                                                    "Неопределенный тип" };
                m_idUser = -1;

                //ID типов сообщений
                s_IdTypeMessages = new int[] {
                         1, 2, 3, 4, 5, 6, 7, 8, 9
                        , 10
                    };
                
                
            }

            private event Action EventThreadProcSelectCompleted;

            /// <summary>
            /// Потоковый метод опроса БД для выборки лог-сообщений
            /// </summary>
            /// <param name="data">Объект с данными для разборки методом</param>
            protected override void thread_Proc(object data)
            {
                int err = -1
                    , lDelimeter = 0;
                string strDelimeter
                    , text;
                //DbConnection connLoggingDB;
                string [] parameters;
                DataTable tblDatetimeStart;
                DateTime dtStart;

                while (Char.IsDigit((data as string).ToCharArray()[lDelimeter]) == true)
                    lDelimeter++;
                strDelimeter = (data as string).Substring(lDelimeter, Int32.Parse((data as string).Substring(0, lDelimeter)));
                text = (data as string).Substring(lDelimeter + strDelimeter.Length, (data as string).Length - (lDelimeter + strDelimeter.Length));
                parameters = text.Split (new string [] { strDelimeter }, StringSplitOptions.None);
                //connLoggingDB = DbSources.Sources().GetConnection(Int32.Parse(parameters [0]), out err);
                m_idUser = Int32.Parse(parameters [1]);

                string query = @"SELECT DATEPART (DD, [DATETIME_WR]) as DD, DATEPART (MM, [DATETIME_WR]) as MM, DATEPART (YYYY, [DATETIME_WR]) as [YYYY], COUNT(*) as CNT"
                        + @" FROM [dbo].[logging]"
                        + @" WHERE [ID_USER]=" + m_idUser
                        + @" GROUP BY DATEPART (DD, [DATETIME_WR]), DATEPART (MM, [DATETIME_WR]), DATEPART (YYYY, [DATETIME_WR])"
                        + @" ORDER BY [DD]";

                tblDatetimeStart = DbTSQLDataSource.DataSource().Select(query, null, null, out err);

                EventThreadProcSelectCompleted?.Invoke ();

                int cnt = 0;
                for (int i = 0; i < tblDatetimeStart.Rows.Count; i++)//перебор значений и добовление их в строку
                {
                    dtStart = new DateTime(Int32.Parse(tblDatetimeStart.Rows[i][@"YYYY"].ToString())
                                                    , Int32.Parse(tblDatetimeStart.Rows[i][@"MM"].ToString())
                                                    , Int32.Parse(tblDatetimeStart.Rows[i][@"DD"].ToString()));
                    m_tableLog.Rows.Add(new object[] { dtStart, s_IdTypeMessages[(int)TYPE_LOGMESSAGE.START], ProgramBase.MessageWellcome });

                    cnt += Int32.Parse(tblDatetimeStart.Rows[i][@"CNT"].ToString());
                }

                base.thread_Proc(cnt);//запрос разбора строки с лог-сообщениями
            }

            /// <summary>
            /// Выборка лог-сообщений
            /// </summary>
            /// <param name="iListenerId"></param>
            /// <param name="strIndxType">Индекс типа сообщения</param>
            /// <param name="beg">Начало периода</param>
            /// <param name="end">Оончание периода</param>
            /// <param name="res">Массив строк лог-сообщений</param>
            /// <returns></returns>
            public int Select(int iListenerId, string strIndxType, DateTime beg, DateTime end, ref DataRow[] res)
            {
                int iRes = -1;
                string where = string.Empty;

                m_tableLog.Clear();//очистка таблицы с лог-сообщениями

                DbConnection connLoggingDB = DbSources.Sources().GetConnection(iListenerId, out iRes);

                if (iRes == 0)
                {
                    string query = @"SELECT DATETIME_WR as DATE_TIME, ID_LOGMSG as TYPE, MESSAGE FROM logging WHERE ID_USER=" + m_idUser;

                    if (beg.Equals(DateTime.MaxValue) == false)
                    {
                        //Вариан №1 диапазон даты/времени
                        where = "DATETIME_WR>='" + beg.ToString("yyyyMMdd HH:mm:ss") + "'";
                        if (end.Equals(DateTime.MaxValue) == false)
                            where += " AND DATETIME_WR<'" + end.ToString("yyyyMMdd HH:mm:ss") + "'";
                        else ;
                        ////Вариан №2 указанные сутки
                        //where = "DATETIME_WR='" + beg.ToString("yyyyMMdd") + "'";
                    }
                    else
                        ;

                    if (where.Equals(string.Empty) == false)
                        query += @" AND " + where;
                    else
                        ;

                    //where = addingWhereTypeMessage(@"ID_LOGMSG", strIndxType);
                    //if (where.Equals(string.Empty) == false)
                    //    query += " AND " + where;
                    //else
                    //    ;

                    m_tableLog = DbTSQLInterface.Select(ref connLoggingDB, query, null, null, out iRes);
                }
                else
                    ;

                if (iRes == 0)
                {
                    iRes = m_tableLog.Rows.Count;
                    res = m_tableLog.Select(string.Empty, @"DATE_TIME");
                }
                else
                    ;

                return iRes;
            }
        }
    }

    public class PanelAnalyzer_TCPIP : PanelAnalyzer
    {
        TcpClientAsync m_tcpClient;
        List<TcpClientAsync> m_listTCPClientUsers;

        public PanelAnalyzer_TCPIP(List<StatisticCommon.TEC> tec, Color foreColor, Color backColor)
            : base(tec, foreColor,  backColor)
        {
        }

        protected override void updateCounter(PanelAnalyzer.DataGridView_LogMessageCounter dgv, DateTime start_date, DateTime stop_date, string users)
        {
            throw new NotImplementedException();
        }

        protected override void fill_active_tabs(int user)
        {
            throw new NotImplementedException();
        }

        protected override void fillListBoxTabVisible(List<int> ID_tabs)
        {
            throw new NotImplementedException();
        }

        protected override LogParse newLogParse() { return new LogParse_File(); }

        /// <summary>
        /// Обмен данными (чтение) с приложением "Статистика" пользователя
        /// </summary>
        /// <param name="res">соединение с приложением пользователя</param>
        /// <param name="rec">ответ от приложения пользователя</param>
        void Read(TcpClient res, string rec)
        {
            bool bResOk = rec.Split('=')[1].Split(';')[0].Equals("OK", StringComparison.InvariantCultureIgnoreCase);

            if (bResOk == true)
            {
                //Message from Analyzer CMD;ARG1, ARG2,...,ARGN=RESULT
                switch (rec.Split('=')[0].Split(';')[0])
                {
                    case "INIT":
                        int indxTcpClient = getIndexTcpClient(res);

                        if (indxTcpClient < m_listTCPClientUsers.Count)
                        {
                            dgvClient.Rows[indxTcpClient].Cells[0].Value = true;
                            m_listTCPClientUsers[indxTcpClient].Write(@"DISCONNECT");

                            m_listTCPClientUsers[indxTcpClient].Disconnect();
                        }
                        else
                            ;
                        break;
                    case "LOG_LOCK":
                        //rec.Split('=')[1].Split(';')[1] - полный путь лог-файла
                        startLogParse(rec.Split('=')[1].Split(';')[1]);

                        m_tcpClient.Write("LOG_UNLOCK=?");
                        break;
                    case "LOG_UNLOCK":
                        disconnect();
                        break;
                    case "TAB_VISIBLE":
                        string[] recParameters = rec.Split('=')[1].Split(';'); //список отображаемых вкладок пользователя
                        int i = -1,
                            mode = -1
                            //, key = -1
                            ;
                        int[] IdItems;
                        string[] indexes = null;

                        if (recParameters.Length > 1)
                        {
                            mode = Convert.ToInt32(recParameters[1]);

                            //BeginInvoke(new DelegateIntFunc(SetModeVisibleTabs)/*, mode*/);

                            if (recParameters.Length > 2)
                            {
                                indexes = recParameters[2].Split(',');
                                //arIndexes = recParameters[2].Split(',').ToArray <int> ();

                                IdItems = new int[indexes.Length];
                                for (i = 0; i < indexes.Length; i++)
                                    IdItems[i] = Convert.ToInt32(indexes[i]);
                            }
                            else
                                //Не отображается ни одна вкладка
                                ;
                        }
                        else
                        {// !Ошибка! Не переданы индексы отображаемых вкладок
                        }

                        disconnect();
                        break;
                    case "DISCONNECT":
                        break;
                    case "":
                        break;
                    default:
                        break;
                }
            }
            else
                ;
        }

        private int getIndexTcpClient(TcpClient obj)
        {
            int i = -1;
            for (i = 0; i < m_listTCPClientUsers.Count; i++)
            {
                if (m_listTCPClientUsers[i].Equals(obj) == true)
                    break;
                else
                    ;
            }

            return i;
        }

        protected void ConnectToChecked(TcpClient res, string data)
        {
            int indxTcpClient = getIndexTcpClient(res);

            if (indxTcpClient < m_listTCPClientUsers.Count)
                m_listTCPClientUsers[indxTcpClient].Write(@"INIT=?");
            else
                ;
        }

        protected void ConnectToLogRead(TcpClient res, string data)
        {
            m_tcpClient.Write("LOG_LOCK=?");
        }

        protected void ConnectToTab(TcpClient res, string data)
        {
            m_tcpClient.Write("TAB_VISIBLE=?");
        }

        public override void Start()
        {
            m_listTCPClientUsers = new List<TcpClientAsync>();
            for (int i = 0; i < m_tableUsers.Rows.Count; i++)
            {
                //Проверка активности
                m_listTCPClientUsers.Add(new TcpClientAsync());
                m_listTCPClientUsers[i].delegateConnect = ConnectToChecked;
                m_listTCPClientUsers[i].delegateErrorConnect = errorConnect;
                m_listTCPClientUsers[i].delegateRead = Read;
                //m_listTCPClientUsers[i].Connect (m_tableUsers.Rows[i][NameFieldToConnect].ToString(), 6666);
            }

            base.Start();
        }

        protected override void procChecked(out bool[] arbActives, out int err)
        {
            throw new NotImplementedException();
        }

        protected override void procChecked(object obj)
        //protected override void ProcChecked(object obj, EventArgs ev)
        {
            int i = -1;
            for (i = 0; (i < m_tableUsers.Rows.Count) && (m_bThreadTimerCheckedAllowed == true); i++)
            {
                //Проверка активности
                m_listTCPClientUsers[i].Connect(m_tableUsers.Rows[i][c_NameFieldToConnect].ToString() + ";" + i, 6666);
            }

            //Вариант №0
            m_timerChecked.Change(MSEC_TIMERCHECKED_STANDARD, System.Threading.Timeout.Infinite);
            ////Вариант №1
            //if (! (m_timerChecked.Interval == MSEC_TIMERCHECKED_STANDARD)) m_timerChecked.Interval = MSEC_TIMERCHECKED_STANDARD; else ;
        }

        public override void Stop()
        {
            base.Stop();

            if (!(m_listTCPClientUsers == null))
                m_listTCPClientUsers.Clear();
            else
                ;
        }

        protected override void disconnect()
        {
            if (!(m_tcpClient == null))
            {
                m_tcpClient.Write(@"DISCONNECT");
                m_tcpClient.Disconnect();

                m_tcpClient = null;
            }
            else
                ;
        }

        protected override void dgvClient_SelectionChanged(object sender, EventArgs e)
        {
            if (!(m_tcpClient == null))
            {
                disconnect();
            }
            else
                ;

            if ((dgvClient.SelectedRows.Count > 0) && (!(dgvClient.SelectedRows[0].Index < 0)))
            {
                bool bUpdate = true;
                if ((dgvDatetimeStart.Rows.Count > 0) && (dgvDatetimeStart.SelectedRows[0].Index < (dgvDatetimeStart.Rows.Count - 1)))
                    if (e == null)
                        bUpdate = false;
                    else
                        ;
                else
                    ;

                if (bUpdate == true)
                {
                    m_tcpClient = new TcpClientAsync();
                    m_tcpClient.delegateRead = Read;


                    //Останов потока разбора лог-файла пред. пользователя
                    m_LogParse.Stop();

                    dgvDatetimeStart.SelectionChanged -= dgvDatetimeStart_SelectionChanged;

                    //Очистить элементы управления с данными от пред. лог-файла
                    if (IsHandleCreated/*InvokeRequired*/ == true)
                    {
                        BeginInvoke(new DelegateFunc(tabLoggingClearDatetimeStart));
                        BeginInvoke(new DelegateBoolFunc(tabLoggingClearText), true);
                    }
                    else
                        Logging.Logg().Error(@"FormMainAnalyzer::dgvClient_SelectionChanged () - ... BeginInvoke (TabLoggingClearDatetimeStart, TabLoggingClearText) - ...", Logging.INDEX_MESSAGE.D_001);

                    //Если активна 0-я вкладка (лог-файл)
                    m_tcpClient.delegateConnect = ConnectToLogRead;

                    m_tcpClient.delegateErrorConnect = errorConnect;

                    //m_tcpClient.Connect("localhost", 6666);
                    m_tcpClient.Connect(m_tableUsers.Rows[dgvClient.SelectedRows[0].Index][c_NameFieldToConnect].ToString() + ";" + dgvClient.SelectedRows[0].Index, 6666);
                }
                else
                    ; //Обновлять нет необходимости
            }
            else
                ;
        }

        protected override void startLogParse(string full_path)
        {
            FileInfo fi = new FileInfo(full_path);
            FileStream fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("windows-1251"));

            dgvDatetimeStart.SelectionChanged -= dgvDatetimeStart_SelectionChanged;

            m_LogParse.Start(sr.ReadToEnd());

            sr.Close();
        }

        //protected override int SelectLogMessage(int type, DateTime beg, DateTime end, ref DataRow[] rows)
        protected override int selectLogMessage(string type, DateTime beg, DateTime end, ref DataRow[] rows)
        {
            return m_LogParse.Select(string.Empty, beg, end, ref rows);
        }

        protected override string getTabLoggingTextRow(DataRow r)
        {
            string strRes = string.Empty;

            if (Convert.ToInt32(r["TYPE"]) == (int)LogParse_File.TYPE_LOGMESSAGE.DETAIL)
                strRes = "    " + r["MESSAGE"].ToString() + Environment.NewLine;
            else
                strRes = "[" + r["DATE_TIME"] + "]: " + r["MESSAGE"].ToString() + Environment.NewLine;

            return strRes;
        }

        class LogParse_File : LogParse
        {
            public enum TYPE_LOGMESSAGE
            {
                START, STOP,
                DBOPEN, DBCLOSE, DBEXCEPTION,
                ERROR, DEBUG,
                DETAIL,
                SEPARATOR, SEPARATOR_DATETIME,
                UNKNOWN, COUNT_TYPE_LOGMESSAGE
            };

            public static string[] DESC_LOGMESSAGE = { "Запуск", "Выход",
                "БД открыть", "БД закрыть", "БД исключение",
                "Ошибка", "Отладка",
                "Детализация",
                "Раздел./сообщ.", "Раздел./дата/время",
                "Неопределенный тип" };

            protected string[] SIGNATURE_LOGMESSAGE = { ProgramBase.MessageWellcome, ProgramBase.MessageExit,
                DbTSQLInterface.MessageDbOpen, DbTSQLInterface.MessageDbClose, DbTSQLInterface.MessageDbException,
                "!Ошибка!", "!Отладка!",
                string.Empty,
                Logging.MessageSeparator, Logging.DatetimeStampSeparator,
                string.Empty, "Неопределенный тип" };

            public LogParse_File()
            {
                s_IdTypeMessages = new int[] {
                        0, 1
                        , 2, 3, 4
                        , 5, 6
                        , 7
                        , 8, 9
                        , 10
                        , 11
                    };
            }

            protected override void thread_Proc(object text)
            {
                string line;
                TYPE_LOGMESSAGE typeMsg;
                DateTime dtMsg;
                bool bValid = false;

                line = string.Empty;

                typeMsg = TYPE_LOGMESSAGE.UNKNOWN;

                StringReader content = new StringReader(text as string);

                Console.WriteLine("Начало обработки лог-файла. Размер: {0}", (text as string).Length);

                //Чтение 1-ой строки лог-файла
                line = content.ReadLine();

                while (!(line == null))
                {
                    //Проверка на разделитель сообщение-сообщение
                    if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.SEPARATOR]) == true)
                    {
                        //Следующая строка - дата/время
                        line = content.ReadLine();

                        //Попытка разбора дата/время
                        bValid = DateTime.TryParse(line, out dtMsg);
                        if (bValid == true)
                        {
                            //Следующая строка - разделитель дата/время-сообщение
                            content.ReadLine();
                            //Следующая строка - сообщение
                            line = content.ReadLine();
                        }
                        else
                        {
                            //Установка дата/время предыдущего сообщения
                            if (m_tableLog.Rows.Count > 0)
                                dtMsg = (DateTime)m_tableLog.Rows[m_tableLog.Rows.Count - 1]["DATE_TIME"];
                            else
                                ; //dtMsg = new DateTime(1666, 06, 06, 06, 06, 06);
                        }
                    }
                    else
                    {
                        //Попытка разбора дата/время
                        bValid = DateTime.TryParse(line, out dtMsg);
                        if (bValid == true)
                        {
                            //Следующая строка - разделитель дата/время-сообщение
                            content.ReadLine();
                            //Следующая строка - сообщение
                            line = content.ReadLine();
                        }
                        else
                        {
                            //Установка дата/время предыдущего сообщения
                            //dtMsg = new DateTime(1666, 06, 06, 06, 06, 06);
                            dtMsg = (DateTime)m_tableLog.Rows[m_tableLog.Rows.Count - 1]["DATE_TIME"];

                            if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.SEPARATOR_DATETIME]) == true)
                            {
                                //Следующая строка - сообщение
                                line = content.ReadLine();
                            }
                            else
                            {
                                //Строка содержит сообщение/детализация сообщения
                            }
                        }
                    }

                    if (line == null)
                    {
                        break;
                    }
                    else
                        ;

                    //Проверка на разделитель сообщение-сообщение
                    if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.SEPARATOR]) == true)
                    {
                        continue;
                    }
                    else
                    {
                        if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.START]) == true)
                        {
                            typeMsg = TYPE_LOGMESSAGE.START;
                        }
                        else
                            if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.STOP]) == true)
                            {
                                typeMsg = TYPE_LOGMESSAGE.STOP;
                            }
                            else
                                if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.DBOPEN]) == true)
                                {
                                    typeMsg = TYPE_LOGMESSAGE.DBOPEN;
                                }
                                else
                                    if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.DBCLOSE]) == true)
                                    {
                                        typeMsg = TYPE_LOGMESSAGE.DBCLOSE;
                                    }
                                    else
                                        if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.DBEXCEPTION]) == true)
                                        {
                                            typeMsg = TYPE_LOGMESSAGE.DBEXCEPTION;
                                        }
                                        else
                                            if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.ERROR]) == true)
                                            {
                                                typeMsg = TYPE_LOGMESSAGE.ERROR;
                                            }
                                            else
                                                if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.DEBUG]) == true)
                                                {
                                                    typeMsg = TYPE_LOGMESSAGE.DEBUG;
                                                }
                                                else
                                                    typeMsg = TYPE_LOGMESSAGE.DETAIL;

                        //Добавление строки в таблицу с собщениями
                        m_tableLog.Rows.Add(new object[] { dtMsg, (int)typeMsg, line });

                        //Чтение строки сообщения
                        line = content.ReadLine();

                        while ((!(line == null)) && (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.SEPARATOR]) == false))
                        {
                            //Добавление строки в таблицу с собщениями
                            m_tableLog.Rows.Add(new object[] { dtMsg, (int)TYPE_LOGMESSAGE.DETAIL, line });

                            //Чтение строки сообщения
                            line = content.ReadLine();
                        }
                    }
                }

                base.thread_Proc(m_tableLog.Rows.Count);
            }
        }
    }
}
