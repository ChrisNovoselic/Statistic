﻿namespace StatisticCommon
{
    partial class FormMainTrans
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
            //System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMainTrans));
            //this.m_dgwAdminTable = new StatisticCommon.DataGridViewAdminKomDisp();
            this.buttonClose = new System.Windows.Forms.Button();
            this.panelMain = new System.Windows.Forms.Panel();
            this.comboBoxTECComponent = new System.Windows.Forms.ComboBox();
            this.labelTECComponent = new System.Windows.Forms.Label();
            this.labelDate = new System.Windows.Forms.Label();
            this.dateTimePickerMain = new System.Windows.Forms.DateTimePicker();
            this.groupBoxDest = new System.Windows.Forms.GroupBox();
            this.buttonDestClear = new System.Windows.Forms.Button();
            this.buttonDestSave = new System.Windows.Forms.Button();
            this.labelDestPort = new System.Windows.Forms.Label();
            this.nudnDestPort = new System.Windows.Forms.NumericUpDown();
            this.labelDestPass = new System.Windows.Forms.Label();
            this.labelDestUserID = new System.Windows.Forms.Label();
            this.labelDestNameDatabase = new System.Windows.Forms.Label();
            this.labelDestServerIP = new System.Windows.Forms.Label();
            this.mtbxDestPass = new System.Windows.Forms.MaskedTextBox();
            this.tbxDestUserId = new System.Windows.Forms.TextBox();
            this.tbxDestNameDatabase = new System.Windows.Forms.TextBox();
            this.tbxDestServerIP = new System.Windows.Forms.TextBox();
            this.groupBoxSource = new System.Windows.Forms.GroupBox();
            this.buttonSourceExport = new System.Windows.Forms.Button();
            this.buttonSourceSave = new System.Windows.Forms.Button();
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
            this.notifyIconMain = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStripNotifyIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.развернутьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorNotifyIcon = new System.Windows.Forms.ToolStripSeparator();
            this.закрытьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            //((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).BeginInit();
            this.panelMain.SuspendLayout();
            this.groupBoxDest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudnDestPort)).BeginInit();
            this.groupBoxSource.SuspendLayout();
            
            this.menuStripMain.SuspendLayout();
            this.statusStripMain.SuspendLayout();
            this.contextMenuStripNotifyIcon.SuspendLayout();
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
            //this.panelMain.Controls.Add(this.m_dgwAdminTable);
            this.panelMain.Controls.Add(this.dateTimePickerMain);
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
            // groupBoxDest
            // 
            this.groupBoxDest.BackColor = System.Drawing.SystemColors.Control;
            this.groupBoxDest.Controls.Add(this.buttonDestClear);
            this.groupBoxDest.Controls.Add(this.buttonDestSave);
            this.groupBoxDest.Controls.Add(this.labelDestPort);
            this.groupBoxDest.Controls.Add(this.nudnDestPort);
            this.groupBoxDest.Controls.Add(this.labelDestPass);
            this.groupBoxDest.Controls.Add(this.labelDestUserID);
            this.groupBoxDest.Controls.Add(this.labelDestNameDatabase);
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
            // buttonDestClear
            // 
            this.buttonDestClear.Location = new System.Drawing.Point(10, 166);
            this.buttonDestClear.Name = "buttonDestClear";
            this.buttonDestClear.Size = new System.Drawing.Size(137, 23);
            this.buttonDestClear.TabIndex = 35;
            this.buttonDestClear.Text = "Удалить";
            this.buttonDestClear.UseVisualStyleBackColor = true;
            this.buttonDestClear.Click += new System.EventHandler(this.buttonClear_Click);
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
            // labelDestNameDatabase
            // 
            this.labelDestNameDatabase.AutoSize = true;
            this.labelDestNameDatabase.Location = new System.Drawing.Point(10, 84);
            this.labelDestNameDatabase.Name = "labelDestNameDatabase";
            this.labelDestNameDatabase.Size = new System.Drawing.Size(98, 13);
            this.labelDestNameDatabase.TabIndex = 32;
            this.labelDestNameDatabase.Text = "Имя базы данных";
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
            
            //Source...
            // 
            // groupBoxSource
            // 
            this.groupBoxSource.BackColor = System.Drawing.SystemColors.Info;
            this.groupBoxSource.Controls.Add(this.buttonSourceExport);
            this.groupBoxSource.Controls.Add(this.buttonSourceSave);
            //Source...
            this.groupBoxSource.Location = new System.Drawing.Point(3, 65);
            this.groupBoxSource.Name = "groupBoxSource";
            this.groupBoxSource.Size = new System.Drawing.Size(300, 200);
            this.groupBoxSource.TabIndex = 25;
            this.groupBoxSource.TabStop = false;
            this.groupBoxSource.Text = "Источник";
            this.groupBoxSource.MouseClick += new System.Windows.Forms.MouseEventHandler(this.groupBox_MouseClick);
            this.groupBoxSource.Enter += new System.EventHandler(this.groupBox_Enter);
            // 
            // buttonSourceExport
            // 
            this.buttonSourceExport.Location = new System.Drawing.Point(8, 163);
            this.buttonSourceExport.Name = "buttonSourceExport";
            this.buttonSourceExport.Size = new System.Drawing.Size(137, 23);
            this.buttonSourceExport.TabIndex = 37;
            this.buttonSourceExport.Text = "Экспорт данных";
            this.buttonSourceExport.UseVisualStyleBackColor = true;
            this.buttonSourceExport.Click += new System.EventHandler(this.buttonSourceExport_Click);
            // 
            // buttonSourceSave
            // 
            this.buttonSourceSave.Location = new System.Drawing.Point(151, 163);
            this.buttonSourceSave.Name = "buttonSourceSave";
            this.buttonSourceSave.Size = new System.Drawing.Size(137, 23);
            this.buttonSourceSave.TabIndex = 36;
            this.buttonSourceSave.Text = "Сохранить";
            this.buttonSourceSave.UseVisualStyleBackColor = true;
            this.buttonSourceSave.Click += new System.EventHandler(this.buttonSave_Click);

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
            this.menuStripMain.Text = "menuStripMain";
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
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
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
            this.оПрограммеToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
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
            this.timerMain.Tick += new System.EventHandler(this.timerMain_Tick);
            // 
            // notifyIconMain
            //
            //this.notifyIconMain.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.notifyIconMain.Text = "Статистика: конвертер";
            // 
            // contextMenuStripNotifyIcon
            // 
            this.contextMenuStripNotifyIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.развернутьToolStripMenuItem,
            this.toolStripSeparatorNotifyIcon,
            this.закрытьToolStripMenuItem});
            this.contextMenuStripNotifyIcon.Name = "contextMenuStripNotifyIcon";
            this.contextMenuStripNotifyIcon.Size = new System.Drawing.Size(153, 76);
            // 
            // развернутьToolStripMenuItem
            // 
            this.развернутьToolStripMenuItem.Name = "развернутьToolStripMenuItem";
            this.развернутьToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.развернутьToolStripMenuItem.Text = "Развернуть";
            this.развернутьToolStripMenuItem.Click += new System.EventHandler(this.развернутьToolStripMenuItem_Click);
            // 
            // toolStripSeparatorNotifyIcon
            // 
            this.toolStripSeparatorNotifyIcon.Name = "toolStripSeparatorNotifyIcon";
            this.toolStripSeparatorNotifyIcon.Size = new System.Drawing.Size(149, 6);
            // 
            // закрытьToolStripMenuItem
            // 
            this.закрытьToolStripMenuItem.Name = "закрытьToolStripMenuItem";
            this.закрытьToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.закрытьToolStripMenuItem.Text = "Закрыть";
            this.закрытьToolStripMenuItem.Click += new System.EventHandler(this.закрытьToolStripMenuItem_Click);
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
            //this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStripMain;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            //this.Text = "Конвертер данных плана и административных данных";
            //((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).EndInit();
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.groupBoxDest.ResumeLayout(false);
            this.groupBoxDest.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudnDestPort)).EndInit();
            //Source...
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            this.contextMenuStripNotifyIcon.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        protected System.Windows.Forms.Button buttonClose;
        protected System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Label labelTECComponent;
        private System.Windows.Forms.Label labelDate;
        protected System.Windows.Forms.DateTimePicker dateTimePickerMain;
        protected System.Windows.Forms.GroupBox groupBoxSource;
        protected System.Windows.Forms.GroupBox groupBoxDest;
        protected System.Windows.Forms.Label labelDestPort;
        protected System.Windows.Forms.NumericUpDown nudnDestPort;
        protected System.Windows.Forms.Label labelDestPass;
        protected System.Windows.Forms.Label labelDestUserID;
        protected System.Windows.Forms.Label labelDestNameDatabase;
        protected System.Windows.Forms.Label labelDestServerIP;
        protected System.Windows.Forms.MaskedTextBox mtbxDestPass;
        protected System.Windows.Forms.TextBox tbxDestUserId;
        protected System.Windows.Forms.TextBox tbxDestNameDatabase;
        protected System.Windows.Forms.TextBox tbxDestServerIP;
        //Source...
        protected System.Windows.Forms.ComboBox comboBoxTECComponent;
        private System.Windows.Forms.Button buttonDestClear;
        private System.Windows.Forms.Button buttonDestSave;
        protected System.Windows.Forms.Button buttonSourceSave;
        protected System.Windows.Forms.Button buttonSourceExport;
        private System.Windows.Forms.MenuStrip menuStripMain;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem помощьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem оПрограммеToolStripMenuItem;
        protected System.Windows.Forms.StatusStrip statusStripMain;
        protected System.Windows.Forms.Timer timerMain;
        private System.Windows.Forms.ToolStripStatusLabel lblMainState;
        private System.Windows.Forms.ToolStripStatusLabel lblDescError;
        private System.Windows.Forms.ToolStripStatusLabel lblDateError;
        protected System.Windows.Forms.NotifyIcon notifyIconMain;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripNotifyIcon;
        private System.Windows.Forms.ToolStripMenuItem развернутьToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorNotifyIcon;
        private System.Windows.Forms.ToolStripMenuItem закрытьToolStripMenuItem;
    }
}
