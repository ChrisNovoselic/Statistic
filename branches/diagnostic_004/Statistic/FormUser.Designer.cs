namespace Statistic
{
    partial class FormUser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormUser));
            this.comboBoxRole = new System.Windows.Forms.ComboBox();
            this.labelRole = new System.Windows.Forms.Label();
            this.labelIP = new System.Windows.Forms.Label();
            this.labelUserName = new System.Windows.Forms.Label();
            this.labelDomain = new System.Windows.Forms.Label();
            this.labelComputerName = new System.Windows.Forms.Label();
            this.labelAccess = new System.Windows.Forms.Label();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBoxDomain = new System.Windows.Forms.TextBox();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.textBoxComputerName = new System.Windows.Forms.TextBox();
            this.comboBoxAccess = new System.Windows.Forms.ComboBox();
            this.buttonUserAdd = new System.Windows.Forms.Button();
            this.textBoxUserDesc = new System.Windows.Forms.TextBox();
            this.dgvUsers = new System.Windows.Forms.DataGridView();
            this.ColumnDesc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnButtonDel = new System.Windows.Forms.DataGridViewButtonColumn();
            this.maskedTextBoxIP = new System.Windows.Forms.MaskedTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxRole
            // 
            this.comboBoxRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRole.FormattingEnabled = true;
            this.comboBoxRole.Location = new System.Drawing.Point(306, 12);
            this.comboBoxRole.Name = "comboBoxRole";
            this.comboBoxRole.Size = new System.Drawing.Size(149, 21);
            this.comboBoxRole.TabIndex = 1;
            this.comboBoxRole.SelectedIndexChanged += new System.EventHandler(this.comboBoxRole_SelectedIndexChanged);
            // 
            // labelRole
            // 
            this.labelRole.AutoSize = true;
            this.labelRole.Location = new System.Drawing.Point(235, 20);
            this.labelRole.Name = "labelRole";
            this.labelRole.Size = new System.Drawing.Size(32, 13);
            this.labelRole.TabIndex = 2;
            this.labelRole.Text = "Роль";
            // 
            // labelIP
            // 
            this.labelIP.AutoSize = true;
            this.labelIP.Location = new System.Drawing.Point(235, 56);
            this.labelIP.Name = "labelIP";
            this.labelIP.Size = new System.Drawing.Size(50, 13);
            this.labelIP.TabIndex = 3;
            this.labelIP.Text = "IP-адрес";
            // 
            // labelUserName
            // 
            this.labelUserName.AutoSize = true;
            this.labelUserName.Location = new System.Drawing.Point(235, 127);
            this.labelUserName.Name = "labelUserName";
            this.labelUserName.Size = new System.Drawing.Size(59, 13);
            this.labelUserName.TabIndex = 4;
            this.labelUserName.Text = "Уч.запись";
            // 
            // labelDomain
            // 
            this.labelDomain.AutoSize = true;
            this.labelDomain.Location = new System.Drawing.Point(235, 92);
            this.labelDomain.Name = "labelDomain";
            this.labelDomain.Size = new System.Drawing.Size(42, 13);
            this.labelDomain.TabIndex = 5;
            this.labelDomain.Text = "Домен";
            // 
            // labelComputerName
            // 
            this.labelComputerName.AutoSize = true;
            this.labelComputerName.Location = new System.Drawing.Point(235, 164);
            this.labelComputerName.Name = "labelComputerName";
            this.labelComputerName.Size = new System.Drawing.Size(61, 13);
            this.labelComputerName.TabIndex = 6;
            this.labelComputerName.Text = "Имя комп.";
            // 
            // labelAccess
            // 
            this.labelAccess.AutoSize = true;
            this.labelAccess.Location = new System.Drawing.Point(235, 198);
            this.labelAccess.Name = "labelAccess";
            this.labelAccess.Size = new System.Drawing.Size(44, 13);
            this.labelAccess.TabIndex = 7;
            this.labelAccess.Text = "Доступ";
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(285, 229);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 8;
            this.buttonOk.Text = "Применить";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(380, 229);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "Закрыть";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // textBoxDomain
            // 
            this.textBoxDomain.Location = new System.Drawing.Point(306, 86);
            this.textBoxDomain.Name = "textBoxDomain";
            this.textBoxDomain.Size = new System.Drawing.Size(149, 20);
            this.textBoxDomain.TabIndex = 11;
            this.textBoxDomain.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            this.textBoxDomain.Enter += new System.EventHandler(this.textBox_Enter);
            this.textBoxDomain.Leave += new System.EventHandler(this.textBox_Leave);
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Location = new System.Drawing.Point(306, 120);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(149, 20);
            this.textBoxUserName.TabIndex = 12;
            this.textBoxUserName.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            this.textBoxUserName.Enter += new System.EventHandler(this.textBox_Enter);
            this.textBoxUserName.Leave += new System.EventHandler(this.textBox_Leave);
            // 
            // textBoxComputerName
            // 
            this.textBoxComputerName.Location = new System.Drawing.Point(306, 157);
            this.textBoxComputerName.Name = "textBoxComputerName";
            this.textBoxComputerName.Size = new System.Drawing.Size(149, 20);
            this.textBoxComputerName.TabIndex = 13;
            this.textBoxComputerName.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            this.textBoxComputerName.Enter += new System.EventHandler(this.textBox_Enter);
            this.textBoxComputerName.Leave += new System.EventHandler(this.textBox_Leave);
            // 
            // comboBoxAccess
            // 
            this.comboBoxAccess.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAccess.FormattingEnabled = true;
            this.comboBoxAccess.Location = new System.Drawing.Point(306, 193);
            this.comboBoxAccess.Name = "comboBoxAccess";
            this.comboBoxAccess.Size = new System.Drawing.Size(149, 21);
            this.comboBoxAccess.TabIndex = 14;
            this.comboBoxAccess.SelectionChangeCommitted += new System.EventHandler(this.comboBoxAccess_SelectionChangeCommitted);
            // 
            // buttonUserAdd
            // 
            this.buttonUserAdd.Location = new System.Drawing.Point(205, 193);
            this.buttonUserAdd.Name = "buttonUserAdd";
            this.buttonUserAdd.Size = new System.Drawing.Size(23, 23);
            this.buttonUserAdd.TabIndex = 16;
            this.buttonUserAdd.Text = "+";
            this.buttonUserAdd.UseVisualStyleBackColor = true;
            this.buttonUserAdd.Click += new System.EventHandler(this.buttonUserAdd_Click);
            // 
            // textBoxUserDesc
            // 
            this.textBoxUserDesc.Location = new System.Drawing.Point(12, 196);
            this.textBoxUserDesc.Name = "textBoxUserDesc";
            this.textBoxUserDesc.Size = new System.Drawing.Size(187, 20);
            this.textBoxUserDesc.TabIndex = 17;
            this.textBoxUserDesc.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            this.textBoxUserDesc.Enter += new System.EventHandler(this.textBox_Enter);
            this.textBoxUserDesc.Leave += new System.EventHandler(this.textBox_Leave);
            // 
            // dgvUsers
            // 
            this.dgvUsers.AllowUserToAddRows = false;
            this.dgvUsers.AllowUserToDeleteRows = false;
            this.dgvUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUsers.ColumnHeadersVisible = false;
            this.dgvUsers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnDesc,
            this.ColumnButtonDel});
            this.dgvUsers.Location = new System.Drawing.Point(12, 11);
            this.dgvUsers.MultiSelect = false;
            this.dgvUsers.Name = "dgvUsers";
            this.dgvUsers.RowHeadersVisible = false;
            this.dgvUsers.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgvUsers.Size = new System.Drawing.Size(216, 166);
            this.dgvUsers.TabIndex = 18;
            this.dgvUsers.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvUsers_CellClick);
            this.dgvUsers.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvUsers_CellEndEdit);
            // 
            // ColumnDesc
            // 
            this.ColumnDesc.Frozen = true;
            this.ColumnDesc.HeaderText = "Описание";
            this.ColumnDesc.MaxInputLength = 64;
            this.ColumnDesc.MinimumWidth = 169;
            this.ColumnDesc.Name = "ColumnDesc";
            this.ColumnDesc.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnDesc.ToolTipText = "Пользователь";
            this.ColumnDesc.Width = 169;
            // 
            // ColumnButtonDel
            // 
            this.ColumnButtonDel.FillWeight = 40F;
            this.ColumnButtonDel.Frozen = true;
            this.ColumnButtonDel.HeaderText = "Удалить";
            this.ColumnButtonDel.Name = "ColumnButtonDel";
            this.ColumnButtonDel.ReadOnly = true;
            this.ColumnButtonDel.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnButtonDel.Text = "-";
            this.ColumnButtonDel.ToolTipText = "Удалить";
            this.ColumnButtonDel.Width = 29;
            // 
            // maskedTextBoxIP
            // 
            this.maskedTextBoxIP.Location = new System.Drawing.Point(306, 50);
            this.maskedTextBoxIP.Mask = "000\\.000\\.000\\.000";
            this.maskedTextBoxIP.Name = "maskedTextBoxIP";
            this.maskedTextBoxIP.Size = new System.Drawing.Size(149, 20);
            this.maskedTextBoxIP.TabIndex = 19;
            this.maskedTextBoxIP.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            this.maskedTextBoxIP.Enter += new System.EventHandler(this.textBox_Enter);
            this.maskedTextBoxIP.Leave += new System.EventHandler(this.textBox_Leave);
            // 
            // FormUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(466, 264);
            this.Controls.Add(this.maskedTextBoxIP);
            this.Controls.Add(this.dgvUsers);
            this.Controls.Add(this.textBoxUserDesc);
            this.Controls.Add(this.buttonUserAdd);
            this.Controls.Add(this.comboBoxAccess);
            this.Controls.Add(this.textBoxComputerName);
            this.Controls.Add(this.textBoxUserName);
            this.Controls.Add(this.textBoxDomain);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.labelAccess);
            this.Controls.Add(this.labelComputerName);
            this.Controls.Add(this.labelDomain);
            this.Controls.Add(this.labelUserName);
            this.Controls.Add(this.labelIP);
            this.Controls.Add(this.labelRole);
            this.Controls.Add(this.comboBoxRole);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormUser";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Пользователи";
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxRole;
        private System.Windows.Forms.Label labelRole;
        private System.Windows.Forms.Label labelIP;
        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.Label labelDomain;
        private System.Windows.Forms.Label labelComputerName;
        private System.Windows.Forms.Label labelAccess;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textBoxDomain;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.TextBox textBoxComputerName;
        private System.Windows.Forms.ComboBox comboBoxAccess;
        private System.Windows.Forms.Button buttonUserAdd;
        private System.Windows.Forms.TextBox textBoxUserDesc;
        private System.Windows.Forms.DataGridView dgvUsers;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnDesc;
        private System.Windows.Forms.DataGridViewButtonColumn ColumnButtonDel;
        private System.Windows.Forms.MaskedTextBox maskedTextBoxIP;
    }
}