namespace Statistic
{
    partial class FormTECComponent
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTECComponent));
            this.comboBoxMode = new System.Windows.Forms.ComboBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.dataGridViewTEC = new System.Windows.Forms.DataGridView();
            this.ColumnCheckBoxTECInUse = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnTextBoxTECName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnButtonTECDel = new System.Windows.Forms.DataGridViewButtonColumn();
            this.dataGridViewTECComponent = new System.Windows.Forms.DataGridView();
            this.ColumnTECComponentName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnButtonTECComponentDel = new System.Windows.Forms.DataGridViewButtonColumn();
            this.dataGridViewTG = new System.Windows.Forms.DataGridView();
            this.ColumnTGName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnTGDel = new System.Windows.Forms.DataGridViewButtonColumn();
            this.textBoxTECAdd = new System.Windows.Forms.TextBox();
            this.comboBoxTGAdd = new System.Windows.Forms.ComboBox();
            this.buttonTECAdd = new System.Windows.Forms.Button();
            this.buttonTECComponentAdd = new System.Windows.Forms.Button();
            this.buttonTGAdd = new System.Windows.Forms.Button();
            this.textBoxTECComponentAdd = new System.Windows.Forms.TextBox();
            this.timerUIControl = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTEC)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTECComponent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTG)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxMode
            // 
            this.comboBoxMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMode.FormattingEnabled = true;
            this.comboBoxMode.Location = new System.Drawing.Point(164, 12);
            this.comboBoxMode.Name = "comboBoxMode";
            this.comboBoxMode.Size = new System.Drawing.Size(136, 21);
            this.comboBoxMode.TabIndex = 4;
            this.comboBoxMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxMode_SelectedIndexChanged);
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(271, 251);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(89, 23);
            this.buttonOk.TabIndex = 7;
            this.buttonOk.Text = "Принять";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(369, 251);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(89, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "Отмена";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // dataGridViewTEC
            // 
            this.dataGridViewTEC.AllowUserToAddRows = false;
            this.dataGridViewTEC.AllowUserToDeleteRows = false;
            this.dataGridViewTEC.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTEC.ColumnHeadersVisible = false;
            this.dataGridViewTEC.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnCheckBoxTECInUse,
            this.ColumnTextBoxTECName,
            this.ColumnButtonTECDel});
            this.dataGridViewTEC.Location = new System.Drawing.Point(12, 12);
            this.dataGridViewTEC.MultiSelect = false;
            this.dataGridViewTEC.Name = "dataGridViewTEC";
            this.dataGridViewTEC.RowHeadersVisible = false;
            this.dataGridViewTEC.Size = new System.Drawing.Size(138, 195);
            this.dataGridViewTEC.TabIndex = 16;
            this.dataGridViewTEC.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewTEC_CellClick);
            // 
            // ColumnCheckBoxTECInUse
            // 
            this.ColumnCheckBoxTECInUse.Frozen = true;
            this.ColumnCheckBoxTECInUse.HeaderText = "Используется";
            this.ColumnCheckBoxTECInUse.Name = "ColumnCheckBoxTECInUse";
            this.ColumnCheckBoxTECInUse.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnCheckBoxTECInUse.Width = 23;
            // 
            // ColumnTextBoxTECName
            // 
            this.ColumnTextBoxTECName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnTextBoxTECName.Frozen = true;
            this.ColumnTextBoxTECName.HeaderText = "Наименование";
            this.ColumnTextBoxTECName.Name = "ColumnTextBoxTECName";
            this.ColumnTextBoxTECName.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnTextBoxTECName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ColumnButtonTECDel
            // 
            this.ColumnButtonTECDel.Frozen = true;
            this.ColumnButtonTECDel.HeaderText = "Действие";
            this.ColumnButtonTECDel.Name = "ColumnButtonTECDel";
            this.ColumnButtonTECDel.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnButtonTECDel.Width = 23;
            // 
            // dataGridViewTECComponent
            // 
            this.dataGridViewTECComponent.AllowUserToAddRows = false;
            this.dataGridViewTECComponent.AllowUserToDeleteRows = false;
            this.dataGridViewTECComponent.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTECComponent.ColumnHeadersVisible = false;
            this.dataGridViewTECComponent.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnTECComponentName,
            this.ColumnButtonTECComponentDel});
            this.dataGridViewTECComponent.Location = new System.Drawing.Point(164, 39);
            this.dataGridViewTECComponent.MultiSelect = false;
            this.dataGridViewTECComponent.Name = "dataGridViewTECComponent";
            this.dataGridViewTECComponent.RowHeadersVisible = false;
            this.dataGridViewTECComponent.Size = new System.Drawing.Size(136, 168);
            this.dataGridViewTECComponent.TabIndex = 17;
            this.dataGridViewTECComponent.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewTECComponent_CellClick);
            this.dataGridViewTECComponent.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewTECComponent_CellEndEdit);
            // 
            // ColumnTECComponentName
            // 
            this.ColumnTECComponentName.FillWeight = 66F;
            this.ColumnTECComponentName.HeaderText = "Наименование";
            this.ColumnTECComponentName.MaxInputLength = 16;
            this.ColumnTECComponentName.Name = "ColumnTECComponentName";
            this.ColumnTECComponentName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ColumnTECComponentName.ToolTipText = "Наименование";
            // 
            // ColumnButtonTECComponentDel
            // 
            this.ColumnButtonTECComponentDel.HeaderText = "Действие";
            this.ColumnButtonTECComponentDel.Name = "ColumnButtonTECComponentDel";
            this.ColumnButtonTECComponentDel.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnButtonTECComponentDel.Width = 23;
            // 
            // dataGridViewTG
            // 
            this.dataGridViewTG.AllowUserToAddRows = false;
            this.dataGridViewTG.AllowUserToDeleteRows = false;
            this.dataGridViewTG.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewTG.ColumnHeadersVisible = false;
            this.dataGridViewTG.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnTGName,
            this.ColumnTGDel});
            this.dataGridViewTG.Location = new System.Drawing.Point(319, 12);
            this.dataGridViewTG.MultiSelect = false;
            this.dataGridViewTG.Name = "dataGridViewTG";
            this.dataGridViewTG.RowHeadersVisible = false;
            this.dataGridViewTG.Size = new System.Drawing.Size(136, 195);
            this.dataGridViewTG.TabIndex = 18;
            this.dataGridViewTG.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewTG_CellClick);
            this.dataGridViewTG.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewTG_CellEndEdit);
            // 
            // ColumnTGName
            // 
            this.ColumnTGName.HeaderText = "Наименование";
            this.ColumnTGName.Name = "ColumnTGName";
            this.ColumnTGName.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // ColumnTGDel
            // 
            this.ColumnTGDel.HeaderText = "Действие";
            this.ColumnTGDel.Name = "ColumnTGDel";
            this.ColumnTGDel.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnTGDel.Width = 23;
            // 
            // textBoxTECAdd
            // 
            this.textBoxTECAdd.Location = new System.Drawing.Point(12, 220);
            this.textBoxTECAdd.Name = "textBoxTECAdd";
            this.textBoxTECAdd.Size = new System.Drawing.Size(108, 20);
            this.textBoxTECAdd.TabIndex = 19;
            // 
            // comboBoxTGAdd
            // 
            this.comboBoxTGAdd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTGAdd.FormattingEnabled = true;
            this.comboBoxTGAdd.Location = new System.Drawing.Point(319, 220);
            this.comboBoxTGAdd.Name = "comboBoxTGAdd";
            this.comboBoxTGAdd.Size = new System.Drawing.Size(110, 21);
            this.comboBoxTGAdd.TabIndex = 24;
            // 
            // buttonTECAdd
            // 
            this.buttonTECAdd.BackColor = System.Drawing.SystemColors.Control;
            this.buttonTECAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonTECAdd.Location = new System.Drawing.Point(124, 219);
            this.buttonTECAdd.Name = "buttonTECAdd";
            this.buttonTECAdd.Size = new System.Drawing.Size(26, 23);
            this.buttonTECAdd.TabIndex = 22;
            this.buttonTECAdd.Text = "+";
            this.buttonTECAdd.UseVisualStyleBackColor = false;
            this.buttonTECAdd.Click += new System.EventHandler(this.buttonTECAdd_Click);
            // 
            // buttonTECComponentAdd
            // 
            this.buttonTECComponentAdd.BackColor = System.Drawing.SystemColors.Control;
            this.buttonTECComponentAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonTECComponentAdd.Location = new System.Drawing.Point(274, 219);
            this.buttonTECComponentAdd.Name = "buttonTECComponentAdd";
            this.buttonTECComponentAdd.Size = new System.Drawing.Size(26, 23);
            this.buttonTECComponentAdd.TabIndex = 25;
            this.buttonTECComponentAdd.Text = "+";
            this.buttonTECComponentAdd.UseVisualStyleBackColor = false;
            this.buttonTECComponentAdd.Click += new System.EventHandler(this.buttonTECComponentAdd_Click);
            // 
            // buttonTGAdd
            // 
            this.buttonTGAdd.BackColor = System.Drawing.SystemColors.Control;
            this.buttonTGAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonTGAdd.Location = new System.Drawing.Point(432, 219);
            this.buttonTGAdd.Name = "buttonTGAdd";
            this.buttonTGAdd.Size = new System.Drawing.Size(26, 23);
            this.buttonTGAdd.TabIndex = 26;
            this.buttonTGAdd.Text = "+";
            this.buttonTGAdd.UseVisualStyleBackColor = false;
            this.buttonTGAdd.Click += new System.EventHandler(this.buttonTGAdd_Click);
            // 
            // textBoxTECComponentAdd
            // 
            this.textBoxTECComponentAdd.Location = new System.Drawing.Point(163, 220);
            this.textBoxTECComponentAdd.Name = "textBoxTECComponentAdd";
            this.textBoxTECComponentAdd.Size = new System.Drawing.Size(108, 20);
            this.textBoxTECComponentAdd.TabIndex = 27;
            // 
            // timerUIControl
            // 
            this.timerUIControl.Tick += new System.EventHandler(this.timerUIControl_Tick);
            // 
            // FormTECComponent
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(467, 282);
            this.ControlBox = false;
            this.Controls.Add(this.textBoxTECComponentAdd);
            this.Controls.Add(this.buttonTGAdd);
            this.Controls.Add(this.buttonTECComponentAdd);
            this.Controls.Add(this.comboBoxTGAdd);
            this.Controls.Add(this.buttonTECAdd);
            this.Controls.Add(this.textBoxTECAdd);
            this.Controls.Add(this.dataGridViewTG);
            this.Controls.Add(this.dataGridViewTECComponent);
            this.Controls.Add(this.dataGridViewTEC);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.comboBoxMode);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormTECComponent";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Настройка состава ТЭЦ, ГТП, ЩУ";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTEC)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTECComponent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewTG)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxMode;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.DataGridView dataGridViewTEC;
        private System.Windows.Forms.DataGridView dataGridViewTECComponent;
        private System.Windows.Forms.DataGridView dataGridViewTG;
        private System.Windows.Forms.TextBox textBoxTECAdd;
        private System.Windows.Forms.Button buttonTECAdd;
        private System.Windows.Forms.ComboBox comboBoxTGAdd;
        private System.Windows.Forms.Button buttonTECComponentAdd;
        private System.Windows.Forms.Button buttonTGAdd;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTECComponentName;
        private System.Windows.Forms.DataGridViewButtonColumn ColumnButtonTECComponentDel;
        private System.Windows.Forms.TextBox textBoxTECComponentAdd;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTGName;
        private System.Windows.Forms.DataGridViewButtonColumn ColumnTGDel;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnCheckBoxTECInUse;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTextBoxTECName;
        private System.Windows.Forms.DataGridViewButtonColumn ColumnButtonTECDel;
        private System.Windows.Forms.Timer timerUIControl;
    }
}