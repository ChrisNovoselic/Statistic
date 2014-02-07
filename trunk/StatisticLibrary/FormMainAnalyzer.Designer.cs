namespace StatisticCommon
{
    partial class FormMainAnalyzer
    {
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

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMainAnalyzer));
            this.tabControlAnalyzer = new System.Windows.Forms.TabControl();
            this.tabPageLogging = new System.Windows.Forms.TabPage();
            this.dgvTypeMessage = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn2 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.labelFilterTypeMessage = new System.Windows.Forms.Label();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.labelDatetimeStart = new System.Windows.Forms.Label();
            this.dgvDatetimeStart = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPageTabes = new System.Windows.Forms.TabPage();
            this.labelFilterActives = new System.Windows.Forms.Label();
            this.labelFilterRoles = new System.Windows.Forms.Label();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.dgvFilterActives = new System.Windows.Forms.DataGridView();
            this.dataGridViewActivesCheckBoxColumnUse = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewActivesTextBoxColumnDesc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvFilterRoles = new System.Windows.Forms.DataGridView();
            this.dataGridViewRolesCheckBoxColumnUse = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewRolesTextBoxColumnDesc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvClient = new System.Windows.Forms.DataGridView();
            this.dataGridViewClientCheckBoxColumnActive = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewClientTextBoxColumnDesc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabControlAnalyzer.SuspendLayout();
            this.tabPageLogging.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTypeMessage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDatetimeStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilterActives)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilterRoles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvClient)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControlAnalyzer
            // 
            this.tabControlAnalyzer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlAnalyzer.Controls.Add(this.tabPageLogging);
            this.tabControlAnalyzer.Controls.Add(this.tabPageTabes);
            this.tabControlAnalyzer.Location = new System.Drawing.Point(195, 12);
            this.tabControlAnalyzer.Name = "tabControlAnalyzer";
            this.tabControlAnalyzer.SelectedIndex = 0;
            this.tabControlAnalyzer.Size = new System.Drawing.Size(561, 324);
            this.tabControlAnalyzer.TabIndex = 3;
            // 
            // tabPageLogging
            // 
            this.tabPageLogging.Controls.Add(this.dgvTypeMessage);
            this.tabPageLogging.Controls.Add(this.labelFilterTypeMessage);
            this.tabPageLogging.Controls.Add(this.textBoxLog);
            this.tabPageLogging.Controls.Add(this.labelDatetimeStart);
            this.tabPageLogging.Controls.Add(this.dgvDatetimeStart);
            this.tabPageLogging.Location = new System.Drawing.Point(4, 22);
            this.tabPageLogging.Name = "tabPageLogging";
            this.tabPageLogging.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLogging.Size = new System.Drawing.Size(553, 298);
            this.tabPageLogging.TabIndex = 0;
            this.tabPageLogging.Text = "Лог-файл";
            this.tabPageLogging.ToolTipText = "Лог-файл пользователя";
            this.tabPageLogging.UseVisualStyleBackColor = true;
            // 
            // dgvTypeMessage
            // 
            this.dgvTypeMessage.AllowUserToAddRows = false;
            this.dgvTypeMessage.AllowUserToDeleteRows = false;
            this.dgvTypeMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dgvTypeMessage.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTypeMessage.ColumnHeadersVisible = false;
            this.dgvTypeMessage.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn2,
            this.dataGridViewTextBoxColumn2,
            this.ColumnCount});
            this.dgvTypeMessage.Location = new System.Drawing.Point(6, 206);
            this.dgvTypeMessage.MultiSelect = false;
            this.dgvTypeMessage.Name = "dgvTypeMessage";
            this.dgvTypeMessage.RowHeadersVisible = false;
            this.dgvTypeMessage.RowTemplate.Height = 18;
            this.dgvTypeMessage.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvTypeMessage.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTypeMessage.Size = new System.Drawing.Size(170, 92);
            this.dgvTypeMessage.TabIndex = 14;
            // 
            // dataGridViewCheckBoxColumn2
            // 
            this.dataGridViewCheckBoxColumn2.Frozen = true;
            this.dataGridViewCheckBoxColumn2.HeaderText = "Use";
            this.dataGridViewCheckBoxColumn2.Name = "dataGridViewCheckBoxColumn2";
            this.dataGridViewCheckBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewCheckBoxColumn2.Width = 25;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.Frozen = true;
            this.dataGridViewTextBoxColumn2.HeaderText = "Desc";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn2.Width = 105;
            // 
            // ColumnCount
            // 
            this.ColumnCount.Frozen = true;
            this.ColumnCount.HeaderText = "Count";
            this.ColumnCount.Name = "ColumnCount";
            this.ColumnCount.Width = 20;
            // 
            // labelFilterTypeMessage
            // 
            this.labelFilterTypeMessage.AutoSize = true;
            this.labelFilterTypeMessage.Location = new System.Drawing.Point(3, 190);
            this.labelFilterTypeMessage.Name = "labelFilterTypeMessage";
            this.labelFilterTypeMessage.Size = new System.Drawing.Size(130, 13);
            this.labelFilterTypeMessage.TabIndex = 13;
            this.labelFilterTypeMessage.Text = "Фильтр: тип сообщения";
            // 
            // textBoxLog
            // 
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.Location = new System.Drawing.Point(182, 6);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLog.Size = new System.Drawing.Size(371, 292);
            this.textBoxLog.TabIndex = 15;
            // 
            // labelDatetimeStart
            // 
            this.labelDatetimeStart.AutoSize = true;
            this.labelDatetimeStart.Location = new System.Drawing.Point(3, 7);
            this.labelDatetimeStart.Name = "labelDatetimeStart";
            this.labelDatetimeStart.Size = new System.Drawing.Size(157, 13);
            this.labelDatetimeStart.TabIndex = 11;
            this.labelDatetimeStart.Text = "Фильтр: дата/время запуска";
            // 
            // dgvDatetimeStart
            // 
            this.dgvDatetimeStart.AllowUserToAddRows = false;
            this.dgvDatetimeStart.AllowUserToDeleteRows = false;
            this.dgvDatetimeStart.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDatetimeStart.ColumnHeadersVisible = false;
            this.dgvDatetimeStart.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn1,
            this.dataGridViewTextBoxColumn1});
            this.dgvDatetimeStart.Location = new System.Drawing.Point(6, 23);
            this.dgvDatetimeStart.MultiSelect = false;
            this.dgvDatetimeStart.Name = "dgvDatetimeStart";
            this.dgvDatetimeStart.ReadOnly = true;
            this.dgvDatetimeStart.RowHeadersVisible = false;
            this.dgvDatetimeStart.RowTemplate.Height = 18;
            this.dgvDatetimeStart.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDatetimeStart.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDatetimeStart.Size = new System.Drawing.Size(170, 164);
            this.dgvDatetimeStart.TabIndex = 10;
            // 
            // dataGridViewCheckBoxColumn1
            // 
            this.dataGridViewCheckBoxColumn1.Frozen = true;
            this.dataGridViewCheckBoxColumn1.HeaderText = "Use";
            this.dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            this.dataGridViewCheckBoxColumn1.ReadOnly = true;
            this.dataGridViewCheckBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewCheckBoxColumn1.Width = 25;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.Frozen = true;
            this.dataGridViewTextBoxColumn1.HeaderText = "Desc";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn1.Width = 145;
            // 
            // tabPageTabes
            // 
            this.tabPageTabes.Location = new System.Drawing.Point(4, 22);
            this.tabPageTabes.Name = "tabPageTabes";
            this.tabPageTabes.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTabes.Size = new System.Drawing.Size(553, 298);
            this.tabPageTabes.TabIndex = 1;
            this.tabPageTabes.Text = "Вкладки";
            this.tabPageTabes.ToolTipText = "Отображаемые вкладки";
            this.tabPageTabes.UseVisualStyleBackColor = true;
            // 
            // labelFilterActives
            // 
            this.labelFilterActives.AutoSize = true;
            this.labelFilterActives.Location = new System.Drawing.Point(9, 12);
            this.labelFilterActives.Name = "labelFilterActives";
            this.labelFilterActives.Size = new System.Drawing.Size(111, 13);
            this.labelFilterActives.TabIndex = 4;
            this.labelFilterActives.Text = "Фильтр: активность";
            // 
            // labelFilterRoles
            // 
            this.labelFilterRoles.AutoSize = true;
            this.labelFilterRoles.Location = new System.Drawing.Point(9, 71);
            this.labelFilterRoles.Name = "labelFilterRoles";
            this.labelFilterRoles.Size = new System.Drawing.Size(77, 13);
            this.labelFilterRoles.TabIndex = 5;
            this.labelFilterRoles.Text = "Фильтр: роли";
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUpdate.Location = new System.Drawing.Point(540, 351);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(100, 26);
            this.buttonUpdate.TabIndex = 6;
            this.buttonUpdate.Text = "Обновить";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.Location = new System.Drawing.Point(656, 351);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(100, 26);
            this.buttonClose.TabIndex = 7;
            this.buttonClose.Text = "Закрыть";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // dgvFilterActives
            // 
            this.dgvFilterActives.AllowUserToAddRows = false;
            this.dgvFilterActives.AllowUserToDeleteRows = false;
            this.dgvFilterActives.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFilterActives.ColumnHeadersVisible = false;
            this.dgvFilterActives.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewActivesCheckBoxColumnUse,
            this.dataGridViewActivesTextBoxColumnDesc});
            this.dgvFilterActives.Location = new System.Drawing.Point(12, 28);
            this.dgvFilterActives.MultiSelect = false;
            this.dgvFilterActives.Name = "dgvFilterActives";
            this.dgvFilterActives.ReadOnly = true;
            this.dgvFilterActives.RowHeadersVisible = false;
            this.dgvFilterActives.RowTemplate.Height = 18;
            this.dgvFilterActives.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFilterActives.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvFilterActives.Size = new System.Drawing.Size(170, 39);
            this.dgvFilterActives.TabIndex = 8;
            // 
            // dataGridViewActivesCheckBoxColumnUse
            // 
            this.dataGridViewActivesCheckBoxColumnUse.Frozen = true;
            this.dataGridViewActivesCheckBoxColumnUse.HeaderText = "Use";
            this.dataGridViewActivesCheckBoxColumnUse.Name = "dataGridViewActivesCheckBoxColumnUse";
            this.dataGridViewActivesCheckBoxColumnUse.ReadOnly = true;
            this.dataGridViewActivesCheckBoxColumnUse.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewActivesCheckBoxColumnUse.Width = 25;
            // 
            // dataGridViewActivesTextBoxColumnDesc
            // 
            this.dataGridViewActivesTextBoxColumnDesc.Frozen = true;
            this.dataGridViewActivesTextBoxColumnDesc.HeaderText = "Desc";
            this.dataGridViewActivesTextBoxColumnDesc.Name = "dataGridViewActivesTextBoxColumnDesc";
            this.dataGridViewActivesTextBoxColumnDesc.ReadOnly = true;
            this.dataGridViewActivesTextBoxColumnDesc.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewActivesTextBoxColumnDesc.Width = 145;
            // 
            // dgvFilterRoles
            // 
            this.dgvFilterRoles.AllowUserToAddRows = false;
            this.dgvFilterRoles.AllowUserToDeleteRows = false;
            this.dgvFilterRoles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFilterRoles.ColumnHeadersVisible = false;
            this.dgvFilterRoles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewRolesCheckBoxColumnUse,
            this.dataGridViewRolesTextBoxColumnDesc});
            this.dgvFilterRoles.Location = new System.Drawing.Point(12, 88);
            this.dgvFilterRoles.MultiSelect = false;
            this.dgvFilterRoles.Name = "dgvFilterRoles";
            this.dgvFilterRoles.ReadOnly = true;
            this.dgvFilterRoles.RowHeadersVisible = false;
            this.dgvFilterRoles.RowTemplate.Height = 18;
            this.dgvFilterRoles.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFilterRoles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvFilterRoles.Size = new System.Drawing.Size(170, 51);
            this.dgvFilterRoles.TabIndex = 9;
            this.dgvFilterRoles.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvFilterRoles_CellClick);
            // 
            // dataGridViewRolesCheckBoxColumnUse
            // 
            this.dataGridViewRolesCheckBoxColumnUse.Frozen = true;
            this.dataGridViewRolesCheckBoxColumnUse.HeaderText = "Use";
            this.dataGridViewRolesCheckBoxColumnUse.Name = "dataGridViewRolesCheckBoxColumnUse";
            this.dataGridViewRolesCheckBoxColumnUse.ReadOnly = true;
            this.dataGridViewRolesCheckBoxColumnUse.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewRolesCheckBoxColumnUse.Width = 25;
            // 
            // dataGridViewRolesTextBoxColumnDesc
            // 
            this.dataGridViewRolesTextBoxColumnDesc.Frozen = true;
            this.dataGridViewRolesTextBoxColumnDesc.HeaderText = "Desc";
            this.dataGridViewRolesTextBoxColumnDesc.Name = "dataGridViewRolesTextBoxColumnDesc";
            this.dataGridViewRolesTextBoxColumnDesc.ReadOnly = true;
            this.dataGridViewRolesTextBoxColumnDesc.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewRolesTextBoxColumnDesc.Width = 145;
            // 
            // dgvClient
            // 
            this.dgvClient.AllowUserToAddRows = false;
            this.dgvClient.AllowUserToDeleteRows = false;
            this.dgvClient.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvClient.ColumnHeadersVisible = false;
            this.dgvClient.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewClientCheckBoxColumnActive,
            this.dataGridViewClientTextBoxColumnDesc});
            this.dgvClient.Location = new System.Drawing.Point(12, 150);
            this.dgvClient.MultiSelect = false;
            this.dgvClient.Name = "dgvClient";
            this.dgvClient.ReadOnly = true;
            this.dgvClient.RowHeadersVisible = false;
            this.dgvClient.RowTemplate.Height = 18;
            this.dgvClient.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvClient.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvClient.Size = new System.Drawing.Size(170, 224);
            this.dgvClient.TabIndex = 10;
            this.dgvClient.SelectionChanged += new System.EventHandler(this.dgvClient_SelectionChanged);
            // 
            // dataGridViewClientCheckBoxColumnActive
            // 
            this.dataGridViewClientCheckBoxColumnActive.Frozen = true;
            this.dataGridViewClientCheckBoxColumnActive.HeaderText = "Active";
            this.dataGridViewClientCheckBoxColumnActive.Name = "dataGridViewClientCheckBoxColumnActive";
            this.dataGridViewClientCheckBoxColumnActive.ReadOnly = true;
            this.dataGridViewClientCheckBoxColumnActive.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewClientCheckBoxColumnActive.Width = 25;
            // 
            // dataGridViewClientTextBoxColumnDesc
            // 
            this.dataGridViewClientTextBoxColumnDesc.Frozen = true;
            this.dataGridViewClientTextBoxColumnDesc.HeaderText = "Desc";
            this.dataGridViewClientTextBoxColumnDesc.Name = "dataGridViewClientTextBoxColumnDesc";
            this.dataGridViewClientTextBoxColumnDesc.ReadOnly = true;
            this.dataGridViewClientTextBoxColumnDesc.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewClientTextBoxColumnDesc.Width = 145;
            // 
            // FormMainAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(768, 386);
            this.Controls.Add(this.dgvClient);
            this.Controls.Add(this.dgvFilterRoles);
            this.Controls.Add(this.dgvFilterActives);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonUpdate);
            this.Controls.Add(this.labelFilterRoles);
            this.Controls.Add(this.labelFilterActives);
            this.Controls.Add(this.tabControlAnalyzer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMainAnalyzer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FormMainAnalyzer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMainAnalyzer_FormClosing);
            this.tabControlAnalyzer.ResumeLayout(false);
            this.tabPageLogging.ResumeLayout(false);
            this.tabPageLogging.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTypeMessage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDatetimeStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilterActives)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilterRoles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvClient)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlAnalyzer;
        private System.Windows.Forms.TabPage tabPageLogging;
        private System.Windows.Forms.TabPage tabPageTabes;
        private System.Windows.Forms.Label labelFilterActives;
        private System.Windows.Forms.Label labelFilterRoles;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.DataGridView dgvFilterActives;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewActivesCheckBoxColumnUse;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewActivesTextBoxColumnDesc;
        private System.Windows.Forms.DataGridView dgvFilterRoles;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewRolesCheckBoxColumnUse;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewRolesTextBoxColumnDesc;
        private System.Windows.Forms.DataGridView dgvClient;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewClientCheckBoxColumnActive;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewClientTextBoxColumnDesc;
        private System.Windows.Forms.Label labelDatetimeStart;
        private System.Windows.Forms.DataGridView dgvDatetimeStart;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridView dgvTypeMessage;
        private System.Windows.Forms.Label labelFilterTypeMessage;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnCount;
    }
}

