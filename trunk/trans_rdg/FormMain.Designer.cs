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
            this.components = new System.ComponentModel.Container();
            this.buttonClose = new System.Windows.Forms.Button();
            this.panelMain = new System.Windows.Forms.Panel();
            this.comboBoxTECComponent = new System.Windows.Forms.ComboBox();
            this.labelTECComponent = new System.Windows.Forms.Label();
            this.labelDate = new System.Windows.Forms.Label();
            this.dateTimePickerMain = new System.Windows.Forms.DateTimePicker();
            this.m_dgwAdminTable = new System.Windows.Forms.DataGridView();
            this.groupBoxDest = new System.Windows.Forms.GroupBox();
            this.buttonDestImport = new System.Windows.Forms.Button();
            this.buttonDestSave = new System.Windows.Forms.Button();
            this.labelDestPort = new System.Windows.Forms.Label();
            this.nudnDestPort = new System.Windows.Forms.NumericUpDown();
            this.labelDestPass = new System.Windows.Forms.Label();
            this.labelDestUserID = new System.Windows.Forms.Label();
            this.labelDestDBName = new System.Windows.Forms.Label();
            this.labelDestServerIP = new System.Windows.Forms.Label();
            this.mtbxDestPass = new System.Windows.Forms.MaskedTextBox();
            this.tbxDestUserId = new System.Windows.Forms.TextBox();
            this.tbxDestNameDatabase = new System.Windows.Forms.TextBox();
            this.tbxDestServerIP = new System.Windows.Forms.TextBox();
            this.groupBoxSource = new System.Windows.Forms.GroupBox();
            this.buttonSourceSave = new System.Windows.Forms.Button();
            this.labelSourcePort = new System.Windows.Forms.Label();
            this.nudnSourcePort = new System.Windows.Forms.NumericUpDown();
            this.labelSourcePass = new System.Windows.Forms.Label();
            this.labelSourceUserId = new System.Windows.Forms.Label();
            this.labelSourceDBName = new System.Windows.Forms.Label();
            this.labelSourceServerIP = new System.Windows.Forms.Label();
            this.mtbxSourcePass = new System.Windows.Forms.MaskedTextBox();
            this.tbxSourceUserId = new System.Windows.Forms.TextBox();
            this.tbxSourceNameDatabase = new System.Windows.Forms.TextBox();
            this.tbxSourceServerIP = new System.Windows.Forms.TextBox();
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.помощьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.оПрограммеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.lblMainState = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblDateError = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblDescError = new System.Windows.Forms.ToolStripStatusLabel();
            this.timerMain = new System.Windows.Forms.Timer(this.components);
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).BeginInit();
            this.groupBoxDest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudnDestPort)).BeginInit();
            this.groupBoxSource.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudnSourcePort)).BeginInit();
            this.menuStripMain.SuspendLayout();
            this.statusStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(733, 516);
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
            this.panelMain.Controls.Add(this.labelTECComponent);
            this.panelMain.Controls.Add(this.labelDate);
            this.panelMain.Controls.Add(this.dateTimePickerMain);
            this.panelMain.Controls.Add(this.m_dgwAdminTable);
            this.panelMain.Controls.Add(this.groupBoxDest);
            this.panelMain.Controls.Add(this.groupBoxSource);
            this.panelMain.Location = new System.Drawing.Point(12, 28);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(822, 484);
            this.panelMain.TabIndex = 3;
            // 
            // comboBoxTECComponent
            // 
            this.comboBoxTECComponent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTECComponent.FormattingEnabled = true;
            this.comboBoxTECComponent.Location = new System.Drawing.Point(132, 37);
            this.comboBoxTECComponent.Name = "comboBoxTECComponent";
            this.comboBoxTECComponent.Size = new System.Drawing.Size(171, 21);
            this.comboBoxTECComponent.TabIndex = 33;
            // 
            // labelTECComponent
            // 
            this.labelTECComponent.AutoSize = true;
            this.labelTECComponent.Location = new System.Drawing.Point(11, 39);
            this.labelTECComponent.Name = "labelTECComponent";
            this.labelTECComponent.Size = new System.Drawing.Size(63, 13);
            this.labelTECComponent.TabIndex = 31;
            this.labelTECComponent.Text = "Компонент";
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
            // dateTimePickerMain
            // 
            this.dateTimePickerMain.Location = new System.Drawing.Point(132, 7);
            this.dateTimePickerMain.Name = "dateTimePickerMain";
            this.dateTimePickerMain.Size = new System.Drawing.Size(170, 20);
            this.dateTimePickerMain.TabIndex = 28;
            // 
            // m_dgwAdminTable
            // 
            this.m_dgwAdminTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_dgwAdminTable.Location = new System.Drawing.Point(319, 5);
            this.m_dgwAdminTable.Name = "m_dgwAdminTable";
            this.m_dgwAdminTable.Size = new System.Drawing.Size(498, 471);
            this.m_dgwAdminTable.TabIndex = 27;
            // 
            // groupBoxDest
            // 
            this.groupBoxDest.BackColor = System.Drawing.SystemColors.Control;
            this.groupBoxDest.Controls.Add(this.buttonDestImport);
            this.groupBoxDest.Controls.Add(this.buttonDestSave);
            this.groupBoxDest.Controls.Add(this.labelDestPort);
            this.groupBoxDest.Controls.Add(this.nudnDestPort);
            this.groupBoxDest.Controls.Add(this.labelDestPass);
            this.groupBoxDest.Controls.Add(this.labelDestUserID);
            this.groupBoxDest.Controls.Add(this.labelDestDBName);
            this.groupBoxDest.Controls.Add(this.labelDestServerIP);
            this.groupBoxDest.Controls.Add(this.mtbxDestPass);
            this.groupBoxDest.Controls.Add(this.tbxDestUserId);
            this.groupBoxDest.Controls.Add(this.tbxDestNameDatabase);
            this.groupBoxDest.Controls.Add(this.tbxDestServerIP);
            this.groupBoxDest.Location = new System.Drawing.Point(3, 276);
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
            // buttonDestSave
            // 
            this.buttonDestSave.Location = new System.Drawing.Point(153, 166);
            this.buttonDestSave.Name = "buttonDestSave";
            this.buttonDestSave.Size = new System.Drawing.Size(137, 23);
            this.buttonDestSave.TabIndex = 35;
            this.buttonDestSave.Text = "Сохранить";
            this.buttonDestSave.UseVisualStyleBackColor = true;
            this.buttonDestSave.Click += new System.EventHandler(this.buttonSave_Click);
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
            // nudnDestPort
            // 
            this.nudnDestPort.Enabled = false;
            this.nudnDestPort.Location = new System.Drawing.Point(128, 55);
            this.nudnDestPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudnDestPort.Name = "nudnDestPort";
            this.nudnDestPort.Size = new System.Drawing.Size(69, 20);
            this.nudnDestPort.TabIndex = 26;
            this.nudnDestPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudnDestPort.Value = new decimal(new int[] {
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
            // mtbxDestPass
            // 
            this.mtbxDestPass.Location = new System.Drawing.Point(128, 133);
            this.mtbxDestPass.Name = "mtbxDestPass";
            this.mtbxDestPass.PasswordChar = '#';
            this.mtbxDestPass.Size = new System.Drawing.Size(160, 20);
            this.mtbxDestPass.TabIndex = 29;
            this.mtbxDestPass.TextChanged += new System.EventHandler(this.component_Changed);
            // 
            // tbxDestUserId
            // 
            this.tbxDestUserId.Location = new System.Drawing.Point(128, 107);
            this.tbxDestUserId.Name = "tbxDestUserId";
            this.tbxDestUserId.Size = new System.Drawing.Size(160, 20);
            this.tbxDestUserId.TabIndex = 28;
            this.tbxDestUserId.TextChanged += new System.EventHandler(this.component_Changed);
            // 
            // tbxDestNameDatabase
            // 
            this.tbxDestNameDatabase.Location = new System.Drawing.Point(128, 81);
            this.tbxDestNameDatabase.Name = "tbxDestNameDatabase";
            this.tbxDestNameDatabase.Size = new System.Drawing.Size(160, 20);
            this.tbxDestNameDatabase.TabIndex = 27;
            this.tbxDestNameDatabase.TextChanged += new System.EventHandler(this.component_Changed);
            // 
            // tbxDestServerIP
            // 
            this.tbxDestServerIP.Location = new System.Drawing.Point(128, 27);
            this.tbxDestServerIP.Name = "tbxDestServerIP";
            this.tbxDestServerIP.Size = new System.Drawing.Size(160, 20);
            this.tbxDestServerIP.TabIndex = 25;
            this.tbxDestServerIP.TextChanged += new System.EventHandler(this.component_Changed);
            // 
            // groupBoxSource
            // 
            this.groupBoxSource.BackColor = System.Drawing.SystemColors.Info;
            this.groupBoxSource.Controls.Add(this.buttonSourceSave);
            this.groupBoxSource.Controls.Add(this.labelSourcePort);
            this.groupBoxSource.Controls.Add(this.nudnSourcePort);
            this.groupBoxSource.Controls.Add(this.labelSourcePass);
            this.groupBoxSource.Controls.Add(this.labelSourceUserId);
            this.groupBoxSource.Controls.Add(this.labelSourceDBName);
            this.groupBoxSource.Controls.Add(this.labelSourceServerIP);
            this.groupBoxSource.Controls.Add(this.mtbxSourcePass);
            this.groupBoxSource.Controls.Add(this.tbxSourceUserId);
            this.groupBoxSource.Controls.Add(this.tbxSourceNameDatabase);
            this.groupBoxSource.Controls.Add(this.tbxSourceServerIP);
            this.groupBoxSource.Location = new System.Drawing.Point(3, 65);
            this.groupBoxSource.Name = "groupBoxSource";
            this.groupBoxSource.Size = new System.Drawing.Size(300, 200);
            this.groupBoxSource.TabIndex = 25;
            this.groupBoxSource.TabStop = false;
            this.groupBoxSource.Text = "Источник";
            this.groupBoxSource.MouseClick += new System.Windows.Forms.MouseEventHandler(this.groupBox_MouseClick);
            this.groupBoxSource.Enter += new System.EventHandler(this.groupBox_Enter);
            // 
            // buttonSourceSave
            // 
            this.buttonSourceSave.Location = new System.Drawing.Point(80, 163);
            this.buttonSourceSave.Name = "buttonSourceSave";
            this.buttonSourceSave.Size = new System.Drawing.Size(137, 23);
            this.buttonSourceSave.TabIndex = 36;
            this.buttonSourceSave.Text = "Сохранить";
            this.buttonSourceSave.UseVisualStyleBackColor = true;
            this.buttonSourceSave.Click += new System.EventHandler(this.buttonSave_Click);
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
            // nudnSourcePort
            // 
            this.nudnSourcePort.Enabled = false;
            this.nudnSourcePort.Location = new System.Drawing.Point(129, 53);
            this.nudnSourcePort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudnSourcePort.Name = "nudnSourcePort";
            this.nudnSourcePort.Size = new System.Drawing.Size(69, 20);
            this.nudnSourcePort.TabIndex = 16;
            this.nudnSourcePort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudnSourcePort.Value = new decimal(new int[] {
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
            // mtbxSourcePass
            // 
            this.mtbxSourcePass.Location = new System.Drawing.Point(129, 131);
            this.mtbxSourcePass.Name = "mtbxSourcePass";
            this.mtbxSourcePass.PasswordChar = '#';
            this.mtbxSourcePass.Size = new System.Drawing.Size(160, 20);
            this.mtbxSourcePass.TabIndex = 19;
            this.mtbxSourcePass.TextChanged += new System.EventHandler(this.component_Changed);
            // 
            // tbxSourceUserId
            // 
            this.tbxSourceUserId.Location = new System.Drawing.Point(129, 105);
            this.tbxSourceUserId.Name = "tbxSourceUserId";
            this.tbxSourceUserId.Size = new System.Drawing.Size(160, 20);
            this.tbxSourceUserId.TabIndex = 18;
            this.tbxSourceUserId.TextChanged += new System.EventHandler(this.component_Changed);
            // 
            // tbxSourceNameDatabase
            // 
            this.tbxSourceNameDatabase.Location = new System.Drawing.Point(129, 79);
            this.tbxSourceNameDatabase.Name = "tbxSourceNameDatabase";
            this.tbxSourceNameDatabase.Size = new System.Drawing.Size(160, 20);
            this.tbxSourceNameDatabase.TabIndex = 17;
            this.tbxSourceNameDatabase.TextChanged += new System.EventHandler(this.component_Changed);
            // 
            // tbxSourceServerIP
            // 
            this.tbxSourceServerIP.Location = new System.Drawing.Point(129, 25);
            this.tbxSourceServerIP.Name = "tbxSourceServerIP";
            this.tbxSourceServerIP.Size = new System.Drawing.Size(160, 20);
            this.tbxSourceServerIP.TabIndex = 15;
            this.tbxSourceServerIP.TextChanged += new System.EventHandler(this.component_Changed);
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
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
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
            this.оПрограммеToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.оПрограммеToolStripMenuItem.Text = "О программе";
            this.оПрограммеToolStripMenuItem.Click += new System.EventHandler(this.оПрограммеToolStripMenuItem_Click);
            // 
            // statusStripMain
            // 
            this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblMainState,
            this.lblDateError,
            this.lblDescError});
            this.statusStripMain.Location = new System.Drawing.Point(0, 546);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Size = new System.Drawing.Size(841, 22);
            this.statusStripMain.SizingGrip = false;
            this.statusStripMain.TabIndex = 5;
            this.statusStripMain.Text = "Нет текста";
            // 
            // lblMainState
            // 
            this.lblMainState.AutoSize = false;
            this.lblMainState.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.lblMainState.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblMainState.ForeColor = System.Drawing.Color.Red;
            this.lblMainState.Name = "lblMainState";
            this.lblMainState.Size = new System.Drawing.Size(166, 17);
            // 
            // lblDateError
            // 
            this.lblDateError.AutoSize = false;
            this.lblDateError.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.lblDateError.Name = "lblDateError";
            this.lblDateError.Size = new System.Drawing.Size(166, 17);
            this.lblDateError.Text = "lblDateError";
            // 
            // lblDescError
            // 
            this.lblDescError.AutoSize = false;
            this.lblDescError.Name = "lblDescError";
            this.lblDescError.Size = new System.Drawing.Size(463, 17);
            this.lblDescError.Text = "lblDescError";
            this.lblDescError.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // timerMain
            // 
            this.timerMain.Interval = 1000;
            this.timerMain.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(841, 568);
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
            this.Activated += new System.EventHandler(this.FormMain_Activated);
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).EndInit();
            this.groupBoxDest.ResumeLayout(false);
            this.groupBoxDest.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudnDestPort)).EndInit();
            this.groupBoxSource.ResumeLayout(false);
            this.groupBoxSource.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudnSourcePort)).EndInit();
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Label labelTECComponent;
        private System.Windows.Forms.Label labelDate;
        private System.Windows.Forms.DateTimePicker dateTimePickerMain;
        private System.Windows.Forms.DataGridView m_dgwAdminTable;
        private System.Windows.Forms.GroupBox groupBoxDest;
        private System.Windows.Forms.Label labelDestPort;
        private System.Windows.Forms.NumericUpDown nudnDestPort;
        private System.Windows.Forms.Label labelDestPass;
        private System.Windows.Forms.Label labelDestUserID;
        private System.Windows.Forms.Label labelDestDBName;
        private System.Windows.Forms.Label labelDestServerIP;
        private System.Windows.Forms.MaskedTextBox mtbxDestPass;
        private System.Windows.Forms.TextBox tbxDestUserId;
        private System.Windows.Forms.TextBox tbxDestNameDatabase;
        private System.Windows.Forms.TextBox tbxDestServerIP;
        private System.Windows.Forms.GroupBox groupBoxSource;
        private System.Windows.Forms.Label labelSourcePort;
        private System.Windows.Forms.NumericUpDown nudnSourcePort;
        private System.Windows.Forms.Label labelSourcePass;
        private System.Windows.Forms.Label labelSourceUserId;
        private System.Windows.Forms.Label labelSourceDBName;
        private System.Windows.Forms.Label labelSourceServerIP;
        private System.Windows.Forms.MaskedTextBox mtbxSourcePass;
        private System.Windows.Forms.TextBox tbxSourceUserId;
        private System.Windows.Forms.TextBox tbxSourceNameDatabase;
        private System.Windows.Forms.TextBox tbxSourceServerIP;
        private System.Windows.Forms.ComboBox comboBoxTECComponent;
        private System.Windows.Forms.Button buttonDestImport;
        private System.Windows.Forms.Button buttonDestSave;
        private System.Windows.Forms.Button buttonSourceSave;
        private System.Windows.Forms.MenuStrip menuStripMain;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem помощьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem оПрограммеToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStripMain;
        private System.Windows.Forms.Timer timerMain;
        private System.Windows.Forms.ToolStripStatusLabel lblMainState;
        private System.Windows.Forms.ToolStripStatusLabel lblDescError;
        private System.Windows.Forms.ToolStripStatusLabel lblDateError;
    }
}

