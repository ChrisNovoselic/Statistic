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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageLogging = new System.Windows.Forms.TabPage();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.tabPageTabes = new System.Windows.Forms.TabPage();
            this.labelFilterActives = new System.Windows.Forms.Label();
            this.labelFilterRoles = new System.Windows.Forms.Label();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.dgvFilterActives = new System.Windows.Forms.DataGridView();
            this.Checked = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvFilterRoles = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn1 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvClient = new System.Windows.Forms.DataGridView();
            this.dataGridViewCheckBoxColumn2 = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabControl1.SuspendLayout();
            this.tabPageLogging.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilterActives)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilterRoles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvClient)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPageLogging);
            this.tabControl1.Controls.Add(this.tabPageTabes);
            this.tabControl1.Enabled = false;
            this.tabControl1.Location = new System.Drawing.Point(195, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(450, 324);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPageLogging
            // 
            this.tabPageLogging.Controls.Add(this.richTextBox1);
            this.tabPageLogging.Location = new System.Drawing.Point(4, 22);
            this.tabPageLogging.Name = "tabPageLogging";
            this.tabPageLogging.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLogging.Size = new System.Drawing.Size(442, 298);
            this.tabPageLogging.TabIndex = 0;
            this.tabPageLogging.Text = "Лог-файл";
            this.tabPageLogging.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBox1.Location = new System.Drawing.Point(0, 53);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(442, 245);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // tabPageTabes
            // 
            this.tabPageTabes.Location = new System.Drawing.Point(4, 22);
            this.tabPageTabes.Name = "tabPageTabes";
            this.tabPageTabes.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTabes.Size = new System.Drawing.Size(442, 298);
            this.tabPageTabes.TabIndex = 1;
            this.tabPageTabes.Text = "Вкладки";
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
            this.buttonUpdate.Location = new System.Drawing.Point(429, 351);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(100, 26);
            this.buttonUpdate.TabIndex = 6;
            this.buttonUpdate.Text = "Обновить";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.Location = new System.Drawing.Point(545, 351);
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
            this.Checked,
            this.Description});
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
            // Checked
            // 
            this.Checked.Frozen = true;
            this.Checked.HeaderText = "Column1";
            this.Checked.Name = "Checked";
            this.Checked.ReadOnly = true;
            this.Checked.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Checked.Width = 25;
            // 
            // Description
            // 
            this.Description.Frozen = true;
            this.Description.HeaderText = "Column1";
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            this.Description.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Description.Width = 145;
            // 
            // dgvFilterRoles
            // 
            this.dgvFilterRoles.AllowUserToAddRows = false;
            this.dgvFilterRoles.AllowUserToDeleteRows = false;
            this.dgvFilterRoles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFilterRoles.ColumnHeadersVisible = false;
            this.dgvFilterRoles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn1,
            this.dataGridViewTextBoxColumn1});
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
            // 
            // dataGridViewCheckBoxColumn1
            // 
            this.dataGridViewCheckBoxColumn1.Frozen = true;
            this.dataGridViewCheckBoxColumn1.HeaderText = "Column1";
            this.dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            this.dataGridViewCheckBoxColumn1.ReadOnly = true;
            this.dataGridViewCheckBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewCheckBoxColumn1.Width = 25;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.Frozen = true;
            this.dataGridViewTextBoxColumn1.HeaderText = "Column1";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn1.Width = 145;
            // 
            // dgvClient
            // 
            this.dgvClient.AllowUserToAddRows = false;
            this.dgvClient.AllowUserToDeleteRows = false;
            this.dgvClient.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvClient.ColumnHeadersVisible = false;
            this.dgvClient.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewCheckBoxColumn2,
            this.dataGridViewTextBoxColumn2});
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
            // 
            // dataGridViewCheckBoxColumn2
            // 
            this.dataGridViewCheckBoxColumn2.Frozen = true;
            this.dataGridViewCheckBoxColumn2.HeaderText = "Column1";
            this.dataGridViewCheckBoxColumn2.Name = "dataGridViewCheckBoxColumn2";
            this.dataGridViewCheckBoxColumn2.ReadOnly = true;
            this.dataGridViewCheckBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewCheckBoxColumn2.Width = 25;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.Frozen = true;
            this.dataGridViewTextBoxColumn2.HeaderText = "Column1";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewTextBoxColumn2.Width = 145;
            // 
            // FormMainAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(657, 386);
            this.Controls.Add(this.dgvClient);
            this.Controls.Add(this.dgvFilterRoles);
            this.Controls.Add(this.dgvFilterActives);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonUpdate);
            this.Controls.Add(this.labelFilterRoles);
            this.Controls.Add(this.labelFilterActives);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "FormMainAnalyzer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FormMainAnalyzer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMainAnalyzer_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPageLogging.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilterActives)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFilterRoles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvClient)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageLogging;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TabPage tabPageTabes;
        private System.Windows.Forms.Label labelFilterActives;
        private System.Windows.Forms.Label labelFilterRoles;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.DataGridView dgvFilterActives;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Checked;
        private System.Windows.Forms.DataGridViewTextBoxColumn Description;
        private System.Windows.Forms.DataGridView dgvFilterRoles;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridView dgvClient;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    }
}

