﻿using System.Windows.Forms;

namespace StatisticAnalyzer
{
    partial class FormMain
    {        
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private PanelAnalyzer m_panel;

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

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));

            this.SuspendLayout();

            this.Controls.Add(m_panel);

            // 
            // FormMainAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);

            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMainAnalyzer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Просмотр сообщений журнала";
            //this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMainAnalyzer_FormClosed); //...после закрытия
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMainAnalyzer_FormClosed); //...перед закрытием

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        //partial class PanelAnalyzer
        //{
            //private System.Windows.Forms.Label labelFilterActives;
            //private System.Windows.Forms.DataGridView dgvFilterActives;

            //private System.Windows.Forms.Label labelFilterRoles;
            //private System.Windows.Forms.DataGridView dgvFilterRoles;

            //protected System.Windows.Forms.DataGridView dgvClient;

            //protected System.Windows.Forms.TabControl tabControlAnalyzer;
            //private System.Windows.Forms.TabPage tabPageLogging;
            //private TableLayoutPanel panelTabPageLogging;
            //private System.Windows.Forms.TabPage tabPageTabes;
            //private TableLayoutPanel panelTabPageTabes;

            //private System.Windows.Forms.Label labelDatetimeStart;
            //protected System.Windows.Forms.DataGridView dgvDatetimeStart;

            //private System.Windows.Forms.Label labelFilterTypeMessage;
            //protected System.Windows.Forms.DataGridView dgvFilterTypeMessage;

            //private System.Windows.Forms.DataGridView dgvLogMessage;

            //private System.Windows.Forms.CheckBox checkBoxAdmin;
            //private System.Windows.Forms.CheckBox checkBoxPC;
            //private System.Windows.Forms.CheckBox checkBoxGTP;
            //private System.Windows.Forms.CheckBox checkBoxTG;
            //private System.Windows.Forms.CheckBox checkBoxTEC;
            //protected System.Windows.Forms.DataGridView dgvTabVisible;

            //private System.Windows.Forms.Button buttonUpdate;
            //private System.Windows.Forms.Button buttonClose;

            //private void InitializeComponent()
            //{
            //    this.labelFilterActives = new System.Windows.Forms.Label();
            //    this.dgvFilterActives = new System.Windows.Forms.DataGridView();

            //    this.labelFilterRoles = new System.Windows.Forms.Label();
            //    this.dgvFilterRoles = new System.Windows.Forms.DataGridView();

            //    this.dgvClient = new System.Windows.Forms.DataGridView();

            //    this.tabControlAnalyzer = new System.Windows.Forms.TabControl();
            //    this.tabPageLogging = new System.Windows.Forms.TabPage();
            //    this.panelTabPageLogging = new TableLayoutPanel ();                
            //    this.dgvLogMessage = new System.Windows.Forms.DataGridView();
            //    this.tabPageTabes = new System.Windows.Forms.TabPage();
            //    this.panelTabPageTabes = new TableLayoutPanel();                
            //    this.dgvTabVisible = new System.Windows.Forms.DataGridView();
            //    this.checkBoxTEC = new System.Windows.Forms.CheckBox();
            //    this.checkBoxTG = new System.Windows.Forms.CheckBox();
            //    this.checkBoxGTP = new System.Windows.Forms.CheckBox();
            //    this.checkBoxPC = new System.Windows.Forms.CheckBox();
            //    this.checkBoxAdmin = new System.Windows.Forms.CheckBox();

            //    this.labelDatetimeStart = new System.Windows.Forms.Label();
            //    this.dgvDatetimeStart = new System.Windows.Forms.DataGridView();                

            //    this.labelFilterTypeMessage = new System.Windows.Forms.Label();
            //    this.dgvFilterTypeMessage = new System.Windows.Forms.DataGridView();

            //    this.buttonUpdate = new System.Windows.Forms.Button();
            //    this.buttonClose = new System.Windows.Forms.Button();

            //    ((System.ComponentModel.ISupportInitialize)(this.dgvFilterActives)).BeginInit();
            //    ((System.ComponentModel.ISupportInitialize)(this.dgvFilterRoles)).BeginInit();
            //    ((System.ComponentModel.ISupportInitialize)(this.dgvClient)).BeginInit();

            //    this.tabControlAnalyzer.SuspendLayout();
            //    this.tabPageLogging.SuspendLayout();
            //    this.panelTabPageLogging.SuspendLayout ();
            //    ((System.ComponentModel.ISupportInitialize)(this.dgvFilterTypeMessage)).BeginInit();
            //    ((System.ComponentModel.ISupportInitialize)(this.dgvDatetimeStart)).BeginInit();
            //    ((System.ComponentModel.ISupportInitialize)(this.dgvLogMessage)).BeginInit();
            //    this.tabPageTabes.SuspendLayout();
            //    this.panelTabPageTabes.SuspendLayout ();                
            //    ((System.ComponentModel.ISupportInitialize)(this.dgvTabVisible)).BeginInit();
            //    this.SuspendLayout();

            //    this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            //    initializeLayoutStyle (12, 24);

                 
            //     labelFilterActives
                 
            //    this.labelFilterActives.AutoSize = true;
            //    this.labelFilterActives.Location = new System.Drawing.Point(9, 12);
            //    this.labelFilterActives.Name = "labelFilterActives";
            //    this.labelFilterActives.Size = new System.Drawing.Size(111, 13);
            //    this.labelFilterActives.TabIndex = 4;
            //    this.labelFilterActives.Text = "Фильтр: активность";
                 
            //     dgvFilterActives
                 
            //    this.dgvFilterActives.AllowUserToAddRows = false;
            //    this.dgvFilterActives.AllowUserToDeleteRows = false;
            //    this.dgvFilterActives.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            //    this.dgvFilterActives.ColumnHeadersVisible = false;
            //    this.dgvFilterActives.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            //        new DataGridViewCheckBoxColumn (),
            //        new DataGridViewTextBoxColumn ()});
            //    this.dgvFilterActives.Location = new System.Drawing.Point(12, 28);
            //    this.dgvFilterActives.Dock = DockStyle.Fill;
            //    this.dgvFilterActives.MultiSelect = false;
            //    this.dgvFilterActives.Name = "dgvFilterActives";
            //    this.dgvFilterActives.ReadOnly = true;
            //    this.dgvFilterActives.RowHeadersVisible = false;
            //    this.dgvFilterActives.RowTemplate.Height = 18;
            //    this.dgvFilterActives.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvFilterActives.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //    this.dgvFilterActives.Size = new System.Drawing.Size(170, 39);
            //    this.dgvFilterActives.TabIndex = 8;
                 
            //     dataGridViewActivesCheckBoxColumnUse
                 
            //    int i = 0;
            //    this.dgvFilterActives.Columns[i].Frozen = true;
            //    this.dgvFilterActives.Columns[i].HeaderText = "Use";
            //    this.dgvFilterActives.Columns[i].Name = "dataGridViewActivesCheckBoxColumnUse";
            //    this.dgvFilterActives.Columns[i].ReadOnly = true;
            //    this.dgvFilterActives.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvFilterActives.Columns[i].Width = 25;
                 
            //     dataGridViewActivesTextBoxColumnDesc
                 
            //    i = 1;
            //    this.dgvFilterActives.Columns[i].Frozen = true;
            //    this.dgvFilterActives.Columns[i].HeaderText = "Desc";
            //    this.dgvFilterActives.Columns[i].Name = "dataGridViewActivesTextBoxColumnDesc";
            //    this.dgvFilterActives.Columns[i].ReadOnly = true;
            //    this.dgvFilterActives.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvFilterActives.Columns[i].Width = 145;

                 
            //     labelFilterRoles
                 
            //    this.labelFilterRoles.AutoSize = true;
            //    this.labelFilterRoles.Location = new System.Drawing.Point(9, 71);
            //    this.labelFilterRoles.Name = "labelFilterRoles";
            //    this.labelFilterRoles.Size = new System.Drawing.Size(77, 13);
            //    this.labelFilterRoles.TabIndex = 5;
            //    this.labelFilterRoles.Text = "Фильтр: роли";
                 
            //     dgvFilterRoles
                 
            //    this.dgvFilterRoles.AllowUserToAddRows = false;
            //    this.dgvFilterRoles.AllowUserToDeleteRows = false;
            //    this.dgvFilterRoles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            //    this.dgvFilterRoles.ColumnHeadersVisible = false;
            //    this.dgvFilterRoles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            //        new DataGridViewCheckBoxColumn (),
            //        new DataGridViewTextBoxColumn ()});
            //    this.dgvFilterRoles.Location = new System.Drawing.Point(12, 88);
            //    this.dgvFilterRoles.Dock = DockStyle.Fill;
            //    this.dgvFilterRoles.MultiSelect = false;
            //    this.dgvFilterRoles.Name = "dgvFilterRoles";
            //    this.dgvFilterRoles.ReadOnly = true;
            //    this.dgvFilterRoles.RowHeadersVisible = false;
            //    this.dgvFilterRoles.RowTemplate.Height = 18;
            //    this.dgvFilterRoles.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvFilterRoles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //    this.dgvFilterRoles.Size = new System.Drawing.Size(170, 51);
            //    this.dgvFilterRoles.TabIndex = 9;
            //    this.dgvFilterRoles.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvFilterRoles_CellClick);
                 
            //     dataGridViewRolesCheckBoxColumnUse
                 
            //    i = 0;
            //    this.dgvFilterRoles.Columns[i].Frozen = true;
            //    this.dgvFilterRoles.Columns[i].HeaderText = "Use";
            //    this.dgvFilterRoles.Columns[i].Name = "dataGridViewRolesCheckBoxColumnUse";
            //    this.dgvFilterRoles.Columns[i].ReadOnly = true;
            //    this.dgvFilterRoles.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvFilterRoles.Columns[i].Width = 25;
                 
            //     dataGridViewRolesTextBoxColumnDesc
                 
            //    i = 1;
            //    this.dgvFilterRoles.Columns[i].Frozen = true;
            //    this.dgvFilterRoles.Columns[i].HeaderText = "Desc";
            //    this.dgvFilterRoles.Columns[i].Name = "dataGridViewRolesTextBoxColumnDesc";
            //    this.dgvFilterRoles.Columns[i].ReadOnly = true;
            //    this.dgvFilterRoles.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvFilterRoles.Columns[i].Width = 145;
            //    this.dgvFilterRoles.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                 
            //     dgvClient
                 
            //    this.dgvClient.AllowUserToAddRows = false;
            //    this.dgvClient.AllowUserToDeleteRows = false;
            //    this.dgvClient.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            //    this.dgvClient.ColumnHeadersVisible = false;
            //    this.dgvClient.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            //        new DataGridViewCheckBoxColumn (),
            //        new DataGridViewTextBoxColumn ()});
            //    this.dgvClient.Location = new System.Drawing.Point(12, 150);
            //    this.dgvClient.Dock = DockStyle.Fill;
            //    this.dgvClient.MultiSelect = false;
            //    this.dgvClient.Name = "dgvClient";
            //    this.dgvClient.ReadOnly = true;
            //    this.dgvClient.RowHeadersVisible = false;
            //    this.dgvClient.RowTemplate.Height = 18;
            //    this.dgvClient.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvClient.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //    this.dgvClient.Size = new System.Drawing.Size(170, 224);
            //    this.dgvClient.TabIndex = 10;
            //    this.dgvClient.SelectionChanged += new System.EventHandler(this.dgvClient_SelectionChanged);
                 
            //     dataGridViewClientCheckBoxColumnActive
                 
            //    i = 0;
            //    this.dgvClient.Columns[i].Frozen = true;
            //    this.dgvClient.Columns[i].HeaderText = "Active";
            //    this.dgvClient.Columns[i].Name = "dataGridViewClientCheckBoxColumnActive";
            //    this.dgvClient.Columns[i].ReadOnly = true;
            //    this.dgvClient.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvClient.Columns[i].Width = 25;
                 
            //     dataGridViewClientTextBoxColumnDesc
                 
            //    i = 1;
            //    this.dgvClient.Columns[i].Frozen = true;
            //    this.dgvClient.Columns[i].HeaderText = "Desc";
            //    this.dgvClient.Columns[i].Name = "dataGridViewClientTextBoxColumnDesc";
            //    this.dgvClient.Columns[i].ReadOnly = true;
            //    this.dgvClient.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvClient.Columns[i].Width = 145;

                 
            //     tabControlAnalyzer
                 
            //    this.tabControlAnalyzer.Dock = DockStyle.Fill;
            //    this.tabControlAnalyzer.Controls.Add(this.tabPageLogging);
            //    this.tabControlAnalyzer.Controls.Add(this.tabPageTabes);
            //    this.tabControlAnalyzer.Location = new System.Drawing.Point(195, 12);
            //    this.tabControlAnalyzer.Name = "tabControlAnalyzer";
            //    this.tabControlAnalyzer.SelectedIndex = 0;
            //    this.tabControlAnalyzer.Size = new System.Drawing.Size(561, 324);
            //    this.tabControlAnalyzer.TabIndex = 3;
            //    this.tabControlAnalyzer.SelectedIndexChanged += new System.EventHandler(this.tabControlAnalyzer_SelectedIndexChanged);
            //    this.tabControlAnalyzer.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControlAnalyzer_Selected);
                 
            //     tabPageLogging
                 
            //    this.tabPageLogging.Location = new System.Drawing.Point(4, 22);
            //    this.tabPageLogging.Controls.Add (this.panelTabPageLogging);
            //    this.tabPageLogging.Name = "tabPageLogging";
            //    this.tabPageLogging.Padding = new System.Windows.Forms.Padding(3);
            //    this.tabPageLogging.Size = new System.Drawing.Size(553, 298);
            //    this.tabPageLogging.TabIndex = 0;
            //    this.tabPageLogging.Text = "Лог-файл";
            //    this.tabPageLogging.ToolTipText = "Лог-файл пользователя";
            //    this.tabPageLogging.UseVisualStyleBackColor = true;
            //     panelPageLogging
            //    panelTabPageLogging.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            //    this.panelTabPageLogging.ColumnCount = 6; this.panelTabPageLogging.RowCount = 24;
            //    for (i = 0; i < this.panelTabPageLogging.ColumnCount; i++)
            //        this.panelTabPageLogging.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / this.panelTabPageLogging.ColumnCount));
            //    for (i = 0; i < this.panelTabPageLogging.RowCount; i++)
            //        this.panelTabPageLogging.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / this.panelTabPageLogging.RowCount));
            //    this.panelTabPageLogging.Dock = DockStyle.Fill;
            //    this.panelTabPageLogging.Controls.Add(this.labelDatetimeStart, 0, 0); this.SetColumnSpan(this.labelDatetimeStart, 2);
            //    this.panelTabPageLogging.Controls.Add(this.dgvDatetimeStart, 0, 1); this.SetColumnSpan(this.dgvDatetimeStart, 2); this.SetRowSpan(this.dgvDatetimeStart, 11);
            //    this.panelTabPageLogging.Controls.Add(this.labelFilterTypeMessage, 0, 12); this.SetColumnSpan(this.labelFilterTypeMessage, 2);
            //    this.panelTabPageLogging.Controls.Add(this.dgvFilterTypeMessage, 0, 13); this.SetColumnSpan(this.dgvFilterTypeMessage, 2); this.SetRowSpan(this.dgvFilterTypeMessage, 11);
            //    this.panelTabPageLogging.Controls.Add(this.dgvLogMessage, 2, 0); this.SetColumnSpan(this.dgvLogMessage, 4); this.SetRowSpan(this.dgvLogMessage, 24);

                 
            //     labelDatetimeStart
                 
            //    this.labelDatetimeStart.AutoSize = true;
            //    this.labelDatetimeStart.Location = new System.Drawing.Point(3, 7);
            //    this.labelDatetimeStart.Name = "labelDatetimeStart";
            //    this.labelDatetimeStart.Size = new System.Drawing.Size(157, 13);
            //    this.labelDatetimeStart.TabIndex = 11;
            //    this.labelDatetimeStart.Text = "Фильтр: дата/время запуска";
                 
            //     dgvDatetimeStart
                 
            //    this.dgvDatetimeStart.Dock = DockStyle.Fill;
            //    this.dgvDatetimeStart.AllowUserToAddRows = false;
            //    this.dgvDatetimeStart.AllowUserToDeleteRows = false;
            //    this.dgvDatetimeStart.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            //    this.dgvDatetimeStart.ColumnHeadersVisible = false;
            //    this.dgvDatetimeStart.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            //        new DataGridViewCheckBoxColumn (),
            //        new DataGridViewTextBoxColumn ()});
            //    this.dgvDatetimeStart.Location = new System.Drawing.Point(6, 23);
            //    this.dgvDatetimeStart.MultiSelect = false;
            //    this.dgvDatetimeStart.Name = "dgvDatetimeStart";
            //    this.dgvDatetimeStart.ReadOnly = true;
            //    this.dgvDatetimeStart.RowHeadersVisible = false;
            //    this.dgvDatetimeStart.RowTemplate.Height = 18;
            //    this.dgvDatetimeStart.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvDatetimeStart.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //    this.dgvDatetimeStart.Size = new System.Drawing.Size(170, 164);
            //    this.dgvDatetimeStart.TabIndex = 10;
                 
            //     dataGridViewCheckBoxColumnDatetimeStartUse
                 
            //    i = 0;
            //    this.dgvDatetimeStart.Columns[i].Frozen = true;
            //    this.dgvDatetimeStart.Columns[i].HeaderText = "Use";
            //    this.dgvDatetimeStart.Columns[i].Name = "dataGridViewCheckBoxColumn1";
            //    this.dgvDatetimeStart.Columns[i].ReadOnly = true;
            //    this.dgvDatetimeStart.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvDatetimeStart.Columns[i].Width = 25;
                 
            //     dataGridViewTextBoxColumnDatetimeStartDesc
                 
            //    i = 1;
            //    this.dgvDatetimeStart.Columns[i].Frozen = true;
            //    this.dgvDatetimeStart.Columns[i].HeaderText = "Desc";
            //    this.dgvDatetimeStart.Columns[i].Name = "dataGridViewTextBoxColumnDatetimeStartDesc";
            //    this.dgvDatetimeStart.Columns[i].ReadOnly = true;
            //    this.dgvDatetimeStart.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvDatetimeStart.Columns[i].Width = 145;
                 
            //     labelFilterTypeMessage
                 
            //    this.labelFilterTypeMessage.AutoSize = true;
            //    this.labelFilterTypeMessage.Location = new System.Drawing.Point(3, 190);
            //    this.labelFilterTypeMessage.Name = "labelFilterTypeMessage";
            //    this.labelFilterTypeMessage.Size = new System.Drawing.Size(130, 13);
            //    this.labelFilterTypeMessage.TabIndex = 13;
            //    this.labelFilterTypeMessage.Text = "Фильтр: тип сообщения";
                 
            //     dgvFilterTypeMessage
                 
            //    this.dgvFilterTypeMessage.AllowUserToAddRows = false;
            //    this.dgvFilterTypeMessage.AllowUserToDeleteRows = false;
            //    dgvFilterTypeMessage.ReadOnly = true;
            //    dgvFilterTypeMessage.Dock = DockStyle.Fill;
            //    this.dgvFilterTypeMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            //    this.dgvFilterTypeMessage.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            //    this.dgvFilterTypeMessage.ColumnHeadersVisible = false;
            //    this.dgvFilterTypeMessage.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            //        new DataGridViewCheckBoxColumn (),
            //        new DataGridViewTextBoxColumn (),
            //        new DataGridViewTextBoxColumn ()});
            //    this.dgvFilterTypeMessage.Location = new System.Drawing.Point(6, 206);
            //    this.dgvFilterTypeMessage.MultiSelect = false;
            //    this.dgvFilterTypeMessage.Name = "dgvTypeMessage";
            //    this.dgvFilterTypeMessage.RowHeadersVisible = false;
            //    this.dgvFilterTypeMessage.RowTemplate.Height = 18;
            //    this.dgvFilterTypeMessage.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvFilterTypeMessage.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //    this.dgvFilterTypeMessage.Size = new System.Drawing.Size(170, 92);
            //    this.dgvFilterTypeMessage.TabIndex = 14;
                 
            //     dataGridViewCheckBoxColumnTypeMessageUse
                 
            //    i = 0;
            //    this.dgvFilterTypeMessage.Columns[i].Frozen = true;
            //    this.dgvFilterTypeMessage.Columns[i].HeaderText = "Use";
            //    this.dgvFilterTypeMessage.Columns[i].Name = "dataGridViewCheckBoxColumnTypeMessageUse";
            //    this.dgvFilterTypeMessage.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvFilterTypeMessage.Columns[i].Width = 25;
                 
            //     dataGridViewTextBoxColumnTypeMessageDesc
                 
            //    i = 1;
            //    this.dgvFilterTypeMessage.Columns[i].Frozen = true;
            //    this.dgvFilterTypeMessage.Columns[i].HeaderText = "Desc";
            //    this.dgvFilterTypeMessage.Columns[i].Name = "dataGridViewTextBoxColumnTypeMessageDesc";
            //    this.dgvFilterTypeMessage.Columns[i].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvFilterTypeMessage.Columns[i].Width = 105;
                 
            //     dataGridViewTextBoxColumnCounter
                 
            //    i = 2;
            //    this.dgvFilterTypeMessage.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //    this.dgvFilterTypeMessage.Columns[i].Frozen = true;
            //    this.dgvFilterTypeMessage.Columns[i].HeaderText = "Count";
            //    this.dgvFilterTypeMessage.Columns[i].Name = "ColumnCount";
            //    this.dgvFilterTypeMessage.Columns[i].Width = 20;                
                 
            //     dgvLogMessage
                 
            //    this.dgvLogMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            //    | System.Windows.Forms.AnchorStyles.Left)
            //    | System.Windows.Forms.AnchorStyles.Right)));
            //    this.dgvLogMessage.Location = new System.Drawing.Point(182, 6);
            //    this.dgvLogMessage.Dock = DockStyle.Fill;
            //    this.dgvLogMessage.MultiSelect = false;
            //    this.dgvLogMessage.Name = "dgvLogMessage";
            //    this.dgvLogMessage.ReadOnly = true;
            //    this.dgvLogMessage.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            //    this.dgvLogMessage.Size = new System.Drawing.Size(371, 292);
            //    this.dgvLogMessage.TabIndex = 15;
            //    this.dgvLogMessage.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            //    new System.Windows.Forms.DataGridViewTextBoxColumn ()
            //        , new System.Windows.Forms.DataGridViewTextBoxColumn ()
            //        , new System.Windows.Forms.DataGridViewTextBoxColumn ()
            //    });
            //    this.dgvLogMessage.AllowUserToAddRows =
            //    this.dgvLogMessage.AllowUserToDeleteRows =
            //        false;
            //    this.dgvLogMessage.ColumnHeadersVisible = false;
            //    this.dgvLogMessage.RowHeadersVisible = false;
            //    this.dgvLogMessage.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvLogMessage.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //    this.dgvLogMessage.Columns[0].Width = 85; this.dgvLogMessage.Columns[0].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvLogMessage.Columns[1].Width = 30; this.dgvLogMessage.Columns[1].Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvLogMessage.Columns[2].Width = 254; this.dgvLogMessage.Columns[2].Resizable = System.Windows.Forms.DataGridViewTriState.False;                
                 
            //     tabPageTabes
                 
            //    this.tabPageTabes.Controls.Add(this.panelTabPageTabes);
            //    this.tabPageTabes.Location = new System.Drawing.Point(4, 22);
            //    this.tabPageTabes.Name = "tabPageTabes";
            //    this.tabPageTabes.Padding = new System.Windows.Forms.Padding(3);
            //    this.tabPageTabes.Size = new System.Drawing.Size(553, 298);
            //    this.tabPageTabes.TabIndex = 1;
            //    this.tabPageTabes.Text = "Вкладки";
            //    this.tabPageTabes.ToolTipText = "Отображаемые вкладки";
            //    this.tabPageTabes.UseVisualStyleBackColor = true;
            //     panelTabPageTabes
            //    panelTabPageTabes.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            //    this.panelTabPageTabes.Dock = DockStyle.Fill;
            //    this.panelTabPageTabes.ColumnCount = 6; this.panelTabPageTabes.RowCount = 24;
            //    for (i = 0; i < this.panelTabPageTabes.ColumnCount; i++)
            //        this.panelTabPageTabes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / this.panelTabPageTabes.ColumnCount));
            //    for (i = 0; i < this.panelTabPageTabes.RowCount; i++)
            //        this.panelTabPageTabes.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / this.panelTabPageTabes.RowCount));
            //    this.panelTabPageTabes.Controls.Add(this.checkBoxAdmin, 0, 0); this.SetRowSpan(this.checkBoxAdmin, 2);
            //    this.panelTabPageTabes.Controls.Add(this.checkBoxPC, 1, 0); this.SetRowSpan(this.checkBoxPC, 2);
            //    this.panelTabPageTabes.Controls.Add(this.checkBoxGTP, 1, 2); this.SetRowSpan(this.checkBoxGTP, 2);
            //    this.panelTabPageTabes.Controls.Add(this.checkBoxTG, 2, 0); this.SetRowSpan(this.checkBoxTG, 2);
            //    this.panelTabPageTabes.Controls.Add(this.checkBoxTEC, 2, 2); this.SetRowSpan(this.checkBoxTEC, 2);
            //    this.panelTabPageTabes.Controls.Add(this.dgvTabVisible, 0, 4); this.SetColumnSpan(this.dgvTabVisible, 6); this.SetRowSpan(this.dgvTabVisible, 20);
                 
            //     dgvTabVisible
                 
            //    this.dgvTabVisible.Dock = DockStyle.Fill;
            //    this.dgvTabVisible.AllowUserToAddRows = false;
            //    this.dgvTabVisible.AllowUserToDeleteRows = false;
            //    this.dgvTabVisible.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            //    this.dgvTabVisible.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            //    this.dgvTabVisible.ColumnHeadersVisible = false;
            //    this.dgvTabVisible.Location = new System.Drawing.Point(6, 54);
            //    this.dgvTabVisible.MultiSelect = false;
            //    this.dgvTabVisible.Name = "dgvTabVisible";
            //    this.dgvTabVisible.RowHeadersVisible = false;
            //    this.dgvTabVisible.RowTemplate.Height = 18;
            //    this.dgvTabVisible.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            //    this.dgvTabVisible.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            //    this.dgvTabVisible.Size = new System.Drawing.Size(547, 211);
            //    this.dgvTabVisible.TabIndex = 15;
                 
            //     checkBoxTEC
                 
            //    this.checkBoxTEC.AutoSize = true;
            //    this.checkBoxTEC.Enabled = false;
            //    this.checkBoxTEC.Location = new System.Drawing.Point(6, 6);
            //    this.checkBoxTEC.Name = "checkBoxTEC";
            //    this.checkBoxTEC.Size = new System.Drawing.Size(48, 17);
            //    this.checkBoxTEC.TabIndex = 16;
            //    this.checkBoxTEC.Text = "ТЭЦ";
            //    this.checkBoxTEC.UseVisualStyleBackColor = true;
                 
            //     checkBoxTG
                 
            //    this.checkBoxTG.AutoSize = true;
            //    this.checkBoxTG.Enabled = false;
            //    this.checkBoxTG.Location = new System.Drawing.Point(6, 29);
            //    this.checkBoxTG.Name = "checkBoxTG";
            //    this.checkBoxTG.Size = new System.Drawing.Size(39, 17);
            //    this.checkBoxTG.TabIndex = 17;
            //    this.checkBoxTG.Text = "ТГ";
            //    this.checkBoxTG.UseVisualStyleBackColor = true;
                 
            //     checkBoxGTP
                 
            //    this.checkBoxGTP.AutoSize = true;
            //    this.checkBoxGTP.Enabled = false;
            //    this.checkBoxGTP.Location = new System.Drawing.Point(112, 6);
            //    this.checkBoxGTP.Name = "checkBoxGTP";
            //    this.checkBoxGTP.Size = new System.Drawing.Size(47, 17);
            //    this.checkBoxGTP.TabIndex = 18;
            //    this.checkBoxGTP.Text = "ГТП";
            //    this.checkBoxGTP.UseVisualStyleBackColor = true;
                 
            //     checkBoxPC
                 
            //    this.checkBoxPC.AutoSize = true;
            //    this.checkBoxPC.Enabled = false;
            //    this.checkBoxPC.Location = new System.Drawing.Point(112, 29);
            //    this.checkBoxPC.Name = "checkBoxPC";
            //    this.checkBoxPC.Size = new System.Drawing.Size(44, 17);
            //    this.checkBoxPC.TabIndex = 19;
            //    this.checkBoxPC.Text = "ЩУ";
            //    this.checkBoxPC.UseVisualStyleBackColor = true;
                 
            //     checkBoxAdmin
                 
            //    this.checkBoxAdmin.AutoSize = true;
            //    this.checkBoxAdmin.Enabled = false;
            //    this.checkBoxAdmin.Location = new System.Drawing.Point(6, 275);
            //    this.checkBoxAdmin.Name = "checkBoxAdmin";
            //    this.checkBoxAdmin.Size = new System.Drawing.Size(48, 17);
            //    this.checkBoxAdmin.TabIndex = 20;
            //    this.checkBoxAdmin.Text = "ПБР";
            //    this.checkBoxAdmin.UseVisualStyleBackColor = true;

                 
            //     buttonUpdate
                 
            //    this.buttonUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            //    this.buttonUpdate.Dock = DockStyle.Fill;
            //    this.buttonUpdate.Location = new System.Drawing.Point(540, 351);
            //    this.buttonUpdate.Name = "buttonUpdate";
            //    this.buttonUpdate.Size = new System.Drawing.Size(100, 26);
            //    this.buttonUpdate.TabIndex = 6;
            //    this.buttonUpdate.Text = "Обновить";
            //    this.buttonUpdate.UseVisualStyleBackColor = true;
            //    this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
                 
            //     buttonClose
                 
            //    this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            //    this.buttonClose.Dock = DockStyle.Fill;
            //    this.buttonClose.Location = new System.Drawing.Point(656, 351);
            //    this.buttonClose.Name = "buttonClose";
            //    this.buttonClose.Size = new System.Drawing.Size(100, 26);
            //    this.buttonClose.TabIndex = 7;
            //    this.buttonClose.Text = "Закрыть";
            //    this.buttonClose.UseVisualStyleBackColor = true;
            //    this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);

            //    this.Controls.Add(this.labelFilterActives, 0, 0); this.SetColumnSpan(this.labelFilterActives, 3);
            //    this.Controls.Add(this.dgvFilterActives, 0, 1); this.SetColumnSpan(this.dgvFilterActives, 3); this.SetRowSpan(this.dgvFilterActives, 3);

            //    this.Controls.Add(this.labelFilterRoles, 0, 4); this.SetColumnSpan(this.labelFilterRoles, 3);
            //    this.Controls.Add(this.dgvFilterRoles, 0, 5); this.SetColumnSpan(this.dgvFilterRoles, 3); this.SetRowSpan(this.dgvFilterRoles, 7);

            //    this.Controls.Add(this.dgvClient, 0, 12); this.SetColumnSpan(this.dgvClient, 3); this.SetRowSpan(this.dgvClient, 12);

            //    this.Controls.Add(this.buttonUpdate, 8, 22); this.SetColumnSpan(this.buttonUpdate, 2); this.SetRowSpan(this.buttonUpdate, 2);
            //    this.Controls.Add(this.buttonClose, 10, 22); this.SetColumnSpan(this.buttonClose, 2); this.SetRowSpan(this.buttonClose, 2);

            //    this.Controls.Add(this.tabControlAnalyzer, 3, 0); this.SetColumnSpan(this.tabControlAnalyzer, 9); this.SetRowSpan(this.tabControlAnalyzer, 22);

            //    ((System.ComponentModel.ISupportInitialize)(this.dgvFilterActives)).EndInit();
            //    ((System.ComponentModel.ISupportInitialize)(this.dgvFilterRoles)).EndInit();
            //    ((System.ComponentModel.ISupportInitialize)(this.dgvClient)).EndInit();
            //    this.tabControlAnalyzer.ResumeLayout(false);
            //    this.tabPageLogging.ResumeLayout(false);
            //    this.tabPageLogging.PerformLayout();
            //    this.panelTabPageLogging.ResumeLayout(false);
            //    this.panelTabPageLogging.PerformLayout ();
            //    ((System.ComponentModel.ISupportInitialize)(this.dgvFilterTypeMessage)).EndInit();                
            //    ((System.ComponentModel.ISupportInitialize)(this.dgvDatetimeStart)).EndInit();
            //    ((System.ComponentModel.ISupportInitialize)(this.dgvLogMessage)).EndInit();
            //    this.tabPageTabes.ResumeLayout(false);
            //    this.tabPageTabes.PerformLayout();
            //    this.panelTabPageTabes.ResumeLayout(false);
            //    this.panelTabPageTabes.PerformLayout ();
            //    ((System.ComponentModel.ISupportInitialize)(this.dgvTabVisible)).EndInit();
            //    this.tabControlAnalyzer.PerformLayout();

            //    this.Dock = System.Windows.Forms.DockStyle.Fill;

            //    this.ResumeLayout(false);
            //    this.PerformLayout();
            //}

            //protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            //{
            //    initializeLayoutStyleEvenly(cols, rows);
            //}
        //}
    }
}

