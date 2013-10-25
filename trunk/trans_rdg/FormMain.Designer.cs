namespace trans_rdg
{
    partial class FormMain
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
            this.buttonClose = new System.Windows.Forms.Button();
            this.panelMain = new System.Windows.Forms.Panel();
            this.comboBoxTECComponent = new System.Windows.Forms.ComboBox();
            this.comboBoxMode = new System.Windows.Forms.ComboBox();
            this.labelTECComponent = new System.Windows.Forms.Label();
            this.labelMode = new System.Windows.Forms.Label();
            this.labelDate = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.groupBoxDest = new System.Windows.Forms.GroupBox();
            this.buttonDestImport = new System.Windows.Forms.Button();
            this.buttonDestTest = new System.Windows.Forms.Button();
            this.labelDestPort = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.labelDestPass = new System.Windows.Forms.Label();
            this.labelDestUserID = new System.Windows.Forms.Label();
            this.labelDestDBName = new System.Windows.Forms.Label();
            this.labelDestServerIP = new System.Windows.Forms.Label();
            this.maskedTextBox1 = new System.Windows.Forms.MaskedTextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.groupBoxSource = new System.Windows.Forms.GroupBox();
            this.buttonSourceTest = new System.Windows.Forms.Button();
            this.labelSourcePort = new System.Windows.Forms.Label();
            this.nudnPort = new System.Windows.Forms.NumericUpDown();
            this.labelSourcePass = new System.Windows.Forms.Label();
            this.labelSourceUserId = new System.Windows.Forms.Label();
            this.labelSourceDBName = new System.Windows.Forms.Label();
            this.labelSourceServerIP = new System.Windows.Forms.Label();
            this.mtbxPass = new System.Windows.Forms.MaskedTextBox();
            this.tbxUserId = new System.Windows.Forms.TextBox();
            this.tbxDataBase = new System.Windows.Forms.TextBox();
            this.tbxServer = new System.Windows.Forms.TextBox();
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.помощьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.оПрограммеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBoxDest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.groupBoxSource.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudnPort)).BeginInit();
            this.menuStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(733, 539);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(100, 23);
            this.buttonClose.TabIndex = 2;
            this.buttonClose.Text = "Закрыть";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.comboBoxTECComponent);
            this.panelMain.Controls.Add(this.comboBoxMode);
            this.panelMain.Controls.Add(this.labelTECComponent);
            this.panelMain.Controls.Add(this.labelMode);
            this.panelMain.Controls.Add(this.labelDate);
            this.panelMain.Controls.Add(this.dateTimePicker1);
            this.panelMain.Controls.Add(this.dataGridView1);
            this.panelMain.Controls.Add(this.groupBoxDest);
            this.panelMain.Controls.Add(this.groupBoxSource);
            this.panelMain.Location = new System.Drawing.Point(12, 28);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(822, 501);
            this.panelMain.TabIndex = 3;
            // 
            // comboBoxTECComponent
            // 
            this.comboBoxTECComponent.FormattingEnabled = true;
            this.comboBoxTECComponent.Location = new System.Drawing.Point(132, 60);
            this.comboBoxTECComponent.Name = "comboBoxTECComponent";
            this.comboBoxTECComponent.Size = new System.Drawing.Size(171, 21);
            this.comboBoxTECComponent.TabIndex = 33;
            // 
            // comboBoxMode
            // 
            this.comboBoxMode.FormattingEnabled = true;
            this.comboBoxMode.Location = new System.Drawing.Point(132, 33);
            this.comboBoxMode.Name = "comboBoxMode";
            this.comboBoxMode.Size = new System.Drawing.Size(171, 21);
            this.comboBoxMode.TabIndex = 32;
            // 
            // labelTECComponent
            // 
            this.labelTECComponent.AutoSize = true;
            this.labelTECComponent.Location = new System.Drawing.Point(11, 62);
            this.labelTECComponent.Name = "labelTECComponent";
            this.labelTECComponent.Size = new System.Drawing.Size(63, 13);
            this.labelTECComponent.TabIndex = 31;
            this.labelTECComponent.Text = "Компонент";
            // 
            // labelMode
            // 
            this.labelMode.AutoSize = true;
            this.labelMode.Location = new System.Drawing.Point(11, 37);
            this.labelMode.Name = "labelMode";
            this.labelMode.Size = new System.Drawing.Size(42, 13);
            this.labelMode.TabIndex = 30;
            this.labelMode.Text = "Режим";
            // 
            // labelDate
            // 
            this.labelDate.AutoSize = true;
            this.labelDate.Location = new System.Drawing.Point(11, 12);
            this.labelDate.Name = "labelDate";
            this.labelDate.Size = new System.Drawing.Size(33, 13);
            this.labelDate.TabIndex = 29;
            this.labelDate.Text = "Дата";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(132, 7);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(170, 20);
            this.dateTimePicker1.TabIndex = 28;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(319, 5);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(498, 483);
            this.dataGridView1.TabIndex = 27;
            // 
            // groupBoxDest
            // 
            this.groupBoxDest.BackColor = System.Drawing.SystemColors.Control;
            this.groupBoxDest.Controls.Add(this.buttonDestImport);
            this.groupBoxDest.Controls.Add(this.buttonDestTest);
            this.groupBoxDest.Controls.Add(this.labelDestPort);
            this.groupBoxDest.Controls.Add(this.numericUpDown1);
            this.groupBoxDest.Controls.Add(this.labelDestPass);
            this.groupBoxDest.Controls.Add(this.labelDestUserID);
            this.groupBoxDest.Controls.Add(this.labelDestDBName);
            this.groupBoxDest.Controls.Add(this.labelDestServerIP);
            this.groupBoxDest.Controls.Add(this.maskedTextBox1);
            this.groupBoxDest.Controls.Add(this.textBox1);
            this.groupBoxDest.Controls.Add(this.textBox2);
            this.groupBoxDest.Controls.Add(this.textBox3);
            this.groupBoxDest.Location = new System.Drawing.Point(3, 299);
            this.groupBoxDest.Name = "groupBoxDest";
            this.groupBoxDest.Size = new System.Drawing.Size(300, 200);
            this.groupBoxDest.TabIndex = 26;
            this.groupBoxDest.TabStop = false;
            this.groupBoxDest.Text = "Получатель";
            this.groupBoxDest.MouseClick += new System.Windows.Forms.MouseEventHandler(this.groupBox_MouseClick);
            this.groupBoxDest.Enter += new System.EventHandler(this.groupBox_Enter);
            // 
            // buttonDestImport
            // 
            this.buttonDestImport.Location = new System.Drawing.Point(11, 166);
            this.buttonDestImport.Name = "buttonDestImport";
            this.buttonDestImport.Size = new System.Drawing.Size(137, 23);
            this.buttonDestImport.TabIndex = 36;
            this.buttonDestImport.Text = "Импорт данных";
            this.buttonDestImport.UseVisualStyleBackColor = true;
            // 
            // buttonDestTest
            // 
            this.buttonDestTest.Location = new System.Drawing.Point(152, 166);
            this.buttonDestTest.Name = "buttonDestTest";
            this.buttonDestTest.Size = new System.Drawing.Size(137, 23);
            this.buttonDestTest.TabIndex = 35;
            this.buttonDestTest.Text = "Тест соединения";
            this.buttonDestTest.UseVisualStyleBackColor = true;
            // 
            // labelDestPort
            // 
            this.labelDestPort.AutoSize = true;
            this.labelDestPort.Location = new System.Drawing.Point(11, 57);
            this.labelDestPort.Name = "labelDestPort";
            this.labelDestPort.Size = new System.Drawing.Size(32, 13);
            this.labelDestPort.TabIndex = 31;
            this.labelDestPort.Text = "Порт";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Enabled = false;
            this.numericUpDown1.Location = new System.Drawing.Point(128, 55);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(69, 20);
            this.numericUpDown1.TabIndex = 26;
            this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown1.Value = new decimal(new int[] {
            3306,
            0,
            0,
            0});
            // 
            // labelDestPass
            // 
            this.labelDestPass.AutoSize = true;
            this.labelDestPass.Location = new System.Drawing.Point(10, 136);
            this.labelDestPass.Name = "labelDestPass";
            this.labelDestPass.Size = new System.Drawing.Size(45, 13);
            this.labelDestPass.TabIndex = 34;
            this.labelDestPass.Text = "Пароль";
            // 
            // labelDestUserID
            // 
            this.labelDestUserID.AutoSize = true;
            this.labelDestUserID.Location = new System.Drawing.Point(10, 110);
            this.labelDestUserID.Name = "labelDestUserID";
            this.labelDestUserID.Size = new System.Drawing.Size(103, 13);
            this.labelDestUserID.TabIndex = 33;
            this.labelDestUserID.Text = "Имя пользователя";
            // 
            // labelDestDBName
            // 
            this.labelDestDBName.AutoSize = true;
            this.labelDestDBName.Location = new System.Drawing.Point(10, 84);
            this.labelDestDBName.Name = "labelDestDBName";
            this.labelDestDBName.Size = new System.Drawing.Size(98, 13);
            this.labelDestDBName.TabIndex = 32;
            this.labelDestDBName.Text = "Имя базы данных";
            // 
            // labelDestServerIP
            // 
            this.labelDestServerIP.AutoSize = true;
            this.labelDestServerIP.Location = new System.Drawing.Point(10, 30);
            this.labelDestServerIP.Name = "labelDestServerIP";
            this.labelDestServerIP.Size = new System.Drawing.Size(95, 13);
            this.labelDestServerIP.TabIndex = 30;
            this.labelDestServerIP.Text = "IP адрес сервера";
            // 
            // maskedTextBox1
            // 
            this.maskedTextBox1.Location = new System.Drawing.Point(128, 133);
            this.maskedTextBox1.Name = "maskedTextBox1";
            this.maskedTextBox1.PasswordChar = '#';
            this.maskedTextBox1.Size = new System.Drawing.Size(160, 20);
            this.maskedTextBox1.TabIndex = 29;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(128, 107);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(160, 20);
            this.textBox1.TabIndex = 28;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(128, 81);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(160, 20);
            this.textBox2.TabIndex = 27;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(128, 27);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(160, 20);
            this.textBox3.TabIndex = 25;
            // 
            // groupBoxSource
            // 
            this.groupBoxSource.BackColor = System.Drawing.SystemColors.Info;
            this.groupBoxSource.Controls.Add(this.buttonSourceTest);
            this.groupBoxSource.Controls.Add(this.labelSourcePort);
            this.groupBoxSource.Controls.Add(this.nudnPort);
            this.groupBoxSource.Controls.Add(this.labelSourcePass);
            this.groupBoxSource.Controls.Add(this.labelSourceUserId);
            this.groupBoxSource.Controls.Add(this.labelSourceDBName);
            this.groupBoxSource.Controls.Add(this.labelSourceServerIP);
            this.groupBoxSource.Controls.Add(this.mtbxPass);
            this.groupBoxSource.Controls.Add(this.tbxUserId);
            this.groupBoxSource.Controls.Add(this.tbxDataBase);
            this.groupBoxSource.Controls.Add(this.tbxServer);
            this.groupBoxSource.Location = new System.Drawing.Point(3, 88);
            this.groupBoxSource.Name = "groupBoxSource";
            this.groupBoxSource.Size = new System.Drawing.Size(300, 200);
            this.groupBoxSource.TabIndex = 25;
            this.groupBoxSource.TabStop = false;
            this.groupBoxSource.Text = "Источник";
            this.groupBoxSource.MouseClick += new System.Windows.Forms.MouseEventHandler(this.groupBox_MouseClick);
            this.groupBoxSource.Enter += new System.EventHandler(this.groupBox_Enter);
            // 
            // buttonSourceTest
            // 
            this.buttonSourceTest.Location = new System.Drawing.Point(151, 163);
            this.buttonSourceTest.Name = "buttonSourceTest";
            this.buttonSourceTest.Size = new System.Drawing.Size(137, 23);
            this.buttonSourceTest.TabIndex = 36;
            this.buttonSourceTest.Text = "Тест соединения";
            this.buttonSourceTest.UseVisualStyleBackColor = true;
            // 
            // labelSourcePort
            // 
            this.labelSourcePort.AutoSize = true;
            this.labelSourcePort.Location = new System.Drawing.Point(12, 55);
            this.labelSourcePort.Name = "labelSourcePort";
            this.labelSourcePort.Size = new System.Drawing.Size(32, 13);
            this.labelSourcePort.TabIndex = 21;
            this.labelSourcePort.Text = "Порт";
            // 
            // nudnPort
            // 
            this.nudnPort.Enabled = false;
            this.nudnPort.Location = new System.Drawing.Point(129, 53);
            this.nudnPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudnPort.Name = "nudnPort";
            this.nudnPort.Size = new System.Drawing.Size(69, 20);
            this.nudnPort.TabIndex = 16;
            this.nudnPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudnPort.Value = new decimal(new int[] {
            3306,
            0,
            0,
            0});
            // 
            // labelSourcePass
            // 
            this.labelSourcePass.AutoSize = true;
            this.labelSourcePass.Location = new System.Drawing.Point(11, 134);
            this.labelSourcePass.Name = "labelSourcePass";
            this.labelSourcePass.Size = new System.Drawing.Size(45, 13);
            this.labelSourcePass.TabIndex = 24;
            this.labelSourcePass.Text = "Пароль";
            // 
            // labelSourceUserId
            // 
            this.labelSourceUserId.AutoSize = true;
            this.labelSourceUserId.Location = new System.Drawing.Point(11, 108);
            this.labelSourceUserId.Name = "labelSourceUserId";
            this.labelSourceUserId.Size = new System.Drawing.Size(103, 13);
            this.labelSourceUserId.TabIndex = 23;
            this.labelSourceUserId.Text = "Имя пользователя";
            // 
            // labelSourceDBName
            // 
            this.labelSourceDBName.AutoSize = true;
            this.labelSourceDBName.Location = new System.Drawing.Point(11, 82);
            this.labelSourceDBName.Name = "labelSourceDBName";
            this.labelSourceDBName.Size = new System.Drawing.Size(98, 13);
            this.labelSourceDBName.TabIndex = 22;
            this.labelSourceDBName.Text = "Имя базы данных";
            // 
            // labelSourceServerIP
            // 
            this.labelSourceServerIP.AutoSize = true;
            this.labelSourceServerIP.Location = new System.Drawing.Point(11, 28);
            this.labelSourceServerIP.Name = "labelSourceServerIP";
            this.labelSourceServerIP.Size = new System.Drawing.Size(95, 13);
            this.labelSourceServerIP.TabIndex = 20;
            this.labelSourceServerIP.Text = "IP адрес сервера";
            // 
            // mtbxPass
            // 
            this.mtbxPass.Location = new System.Drawing.Point(129, 131);
            this.mtbxPass.Name = "mtbxPass";
            this.mtbxPass.PasswordChar = '#';
            this.mtbxPass.Size = new System.Drawing.Size(160, 20);
            this.mtbxPass.TabIndex = 19;
            // 
            // tbxUserId
            // 
            this.tbxUserId.Location = new System.Drawing.Point(129, 105);
            this.tbxUserId.Name = "tbxUserId";
            this.tbxUserId.Size = new System.Drawing.Size(160, 20);
            this.tbxUserId.TabIndex = 18;
            // 
            // tbxDataBase
            // 
            this.tbxDataBase.Location = new System.Drawing.Point(129, 79);
            this.tbxDataBase.Name = "tbxDataBase";
            this.tbxDataBase.Size = new System.Drawing.Size(160, 20);
            this.tbxDataBase.TabIndex = 17;
            // 
            // tbxServer
            // 
            this.tbxServer.Location = new System.Drawing.Point(129, 25);
            this.tbxServer.Name = "tbxServer";
            this.tbxServer.Size = new System.Drawing.Size(160, 20);
            this.tbxServer.TabIndex = 15;
            // 
            // menuStripMain
            // 
            this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem,
            this.помощьToolStripMenuItem});
            this.menuStripMain.Location = new System.Drawing.Point(0, 0);
            this.menuStripMain.Name = "menuStripMain";
            this.menuStripMain.Size = new System.Drawing.Size(841, 24);
            this.menuStripMain.TabIndex = 4;
            this.menuStripMain.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.выходToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // выходToolStripMenuItem
            // 
            this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.выходToolStripMenuItem.Text = "Выход";
            this.выходToolStripMenuItem.Click += new System.EventHandler(this.выходToolStripMenuItem_Click);
            // 
            // помощьToolStripMenuItem
            // 
            this.помощьToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.оПрограммеToolStripMenuItem});
            this.помощьToolStripMenuItem.Name = "помощьToolStripMenuItem";
            this.помощьToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.помощьToolStripMenuItem.Text = "Помощь";
            // 
            // оПрограммеToolStripMenuItem
            // 
            this.оПрограммеToolStripMenuItem.Name = "оПрограммеToolStripMenuItem";
            this.оПрограммеToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.оПрограммеToolStripMenuItem.Text = "О программе";
            this.оПрограммеToolStripMenuItem.Click += new System.EventHandler(this.оПрограммеToolStripMenuItem_Click);
            // 
            // statusStripMain
            // 
            this.statusStripMain.Location = new System.Drawing.Point(0, 570);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Size = new System.Drawing.Size(841, 22);
            this.statusStripMain.TabIndex = 5;
            this.statusStripMain.Text = "statusStrip1";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(841, 592);
            this.Controls.Add(this.statusStripMain);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.menuStripMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MainMenuStrip = this.menuStripMain;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Конвертер данных плана и административных данных";
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBoxDest.ResumeLayout(false);
            this.groupBoxDest.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.groupBoxSource.ResumeLayout(false);
            this.groupBoxSource.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudnPort)).EndInit();
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Label labelTECComponent;
        private System.Windows.Forms.Label labelMode;
        private System.Windows.Forms.Label labelDate;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.GroupBox groupBoxDest;
        private System.Windows.Forms.Label labelDestPort;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label labelDestPass;
        private System.Windows.Forms.Label labelDestUserID;
        private System.Windows.Forms.Label labelDestDBName;
        private System.Windows.Forms.Label labelDestServerIP;
        private System.Windows.Forms.MaskedTextBox maskedTextBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.GroupBox groupBoxSource;
        private System.Windows.Forms.Label labelSourcePort;
        private System.Windows.Forms.NumericUpDown nudnPort;
        private System.Windows.Forms.Label labelSourcePass;
        private System.Windows.Forms.Label labelSourceUserId;
        private System.Windows.Forms.Label labelSourceDBName;
        private System.Windows.Forms.Label labelSourceServerIP;
        private System.Windows.Forms.MaskedTextBox mtbxPass;
        private System.Windows.Forms.TextBox tbxUserId;
        private System.Windows.Forms.TextBox tbxDataBase;
        private System.Windows.Forms.TextBox tbxServer;
        private System.Windows.Forms.ComboBox comboBoxTECComponent;
        private System.Windows.Forms.ComboBox comboBoxMode;
        private System.Windows.Forms.Button buttonDestImport;
        private System.Windows.Forms.Button buttonDestTest;
        private System.Windows.Forms.Button buttonSourceTest;
        private System.Windows.Forms.MenuStrip menuStripMain;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem помощьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem оПрограммеToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStripMain;
    }
}

