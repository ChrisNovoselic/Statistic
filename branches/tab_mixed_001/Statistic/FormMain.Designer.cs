﻿namespace Statistic
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            m_ContextMenuStripListTecViews = new System.Windows.Forms.ContextMenuStrip ();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.видToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.панельГрафическихToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.настройкиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сменитьРежимToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.настройкиСоединенияБДКонфToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.настройкиСоединенияБДИсточникToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.текущееСостояниеПользовательToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.администрированиеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.изменитьПарольДиспетчераToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.изменитьПарольАдминистратораToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemИзменитьПарольНСС = new System.Windows.Forms.ToolStripMenuItem();
            this.изментьСоставТЭЦГТПЩУToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.изментьСоставПользовательToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.параметрыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.параметрыПриложенияToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.параметрыТГБийскToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.оПрограммеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tclTecViews = new HTabCtrlEx (); //System.Windows.Forms.TabControl();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.lblLabel = new System.Windows.Forms.Label();
            this.menuStrip.SuspendLayout();
            this.m_ContextMenuStripListTecViews.SuspendLayout ();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem,
            this.видToolStripMenuItem,
            this.настройкиToolStripMenuItem,
            this.оПрограммеToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(982, 24);
            this.menuStrip.TabIndex = 2;
            this.menuStrip.Text = "Главное меню";
            // 
            // ContextMenuStrip
            // 
            this.m_ContextMenuStripListTecViews.Location = new System.Drawing.Point(0, 0);
            this.m_ContextMenuStripListTecViews.Name = "ContextMenuStripListTecViews";
            this.m_ContextMenuStripListTecViews.Text = "Контекстное меню";
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
            // видToolStripMenuItem
            // 
            this.видToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.панельГрафическихToolStripMenuItem});
            this.видToolStripMenuItem.Name = "видToolStripMenuItem";
            this.видToolStripMenuItem.Size = new System.Drawing.Size(38, 20);
            this.видToolStripMenuItem.Text = "Вид";
            // 
            // панельГрафическихToolStripMenuItem
            // 
            this.панельГрафическихToolStripMenuItem.CheckOnClick = true;
            this.панельГрафическихToolStripMenuItem.Name = "панельГрафическихToolStripMenuItem";
            this.панельГрафическихToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.панельГрафическихToolStripMenuItem.Text = "Панель графических параметров";
            this.панельГрафическихToolStripMenuItem.CheckedChanged += new System.EventHandler(this.панельГрафическихToolStripMenuItem_CheckedChanged);
            // 
            // настройкиToolStripMenuItem
            // 
            this.настройкиToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.сменитьРежимToolStripMenuItem,
            this.администрированиеToolStripMenuItem,
            this.параметрыToolStripMenuItem});
            this.настройкиToolStripMenuItem.Name = "настройкиToolStripMenuItem";
            this.настройкиToolStripMenuItem.Size = new System.Drawing.Size(73, 20);
            this.настройкиToolStripMenuItem.Text = "Настройки";
            // 
            // сменитьРежимToolStripMenuItem
            // 
            this.сменитьРежимToolStripMenuItem.Name = "сменитьРежимToolStripMenuItem";
            this.сменитьРежимToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.сменитьРежимToolStripMenuItem.Text = "Сменить режим";
            this.сменитьРежимToolStripMenuItem.Click += new System.EventHandler(this.сменитьРежимToolStripMenuItem_Click);
            // 
            // настройкиСоединенияБДКонфToolStripMenuItem
            // 
            this.настройкиСоединенияБДКонфToolStripMenuItem.Name = "настройкиСоединенияБДКонфToolStripMenuItem";
            this.настройкиСоединенияБДКонфToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.настройкиСоединенияБДКонфToolStripMenuItem.Text = "Настройки соединения БД конфигурации";
            this.настройкиСоединенияБДКонфToolStripMenuItem.Click += new System.EventHandler(this.настройкиСоединенияБДКонфToolStripMenuItem_Click);
            // 
            // настройкиСоединенияБДИсточникToolStripMenuItem
            // 
            this.настройкиСоединенияБДИсточникToolStripMenuItem.Name = "настройкиСоединенияБДИсточникToolStripMenuItem";
            this.настройкиСоединенияБДИсточникToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.настройкиСоединенияБДИсточникToolStripMenuItem.Text = "Настройки соединения БД источников данных";
            this.настройкиСоединенияБДИсточникToolStripMenuItem.Click += new System.EventHandler(this.настройкиСоединенияБДИсточникToolStripMenuItem_Click);
            // 
            // администрированиеПользовательToolStripMenuItem
            // 
            this.текущееСостояниеПользовательToolStripMenuItem.Name = "текущееСостояниеПользовательToolStripMenuItem";
            this.текущееСостояниеПользовательToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.текущееСостояниеПользовательToolStripMenuItem.Text = "Текущее состояние пользователей";
            this.текущееСостояниеПользовательToolStripMenuItem.Click += new System.EventHandler(this.текущееСостояниеПользовательToolStripMenuItem_Click);
            // 
            // администрированиеToolStripMenuItem
            // 
            this.администрированиеToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.настройкиСоединенияБДКонфToolStripMenuItem,
            this.настройкиСоединенияБДИсточникToolStripMenuItem,
            this.текущееСостояниеПользовательToolStripMenuItem,
            this.изменитьПарольДиспетчераToolStripMenuItem,
            this.изменитьПарольАдминистратораToolStripMenuItem,
            this.toolStripMenuItemИзменитьПарольНСС,
            this.изментьСоставТЭЦГТПЩУToolStripMenuItem,
            this.изментьСоставПользовательToolStripMenuItem});
            this.администрированиеToolStripMenuItem.Name = "администрированиеToolStripMenuItem";
            this.администрированиеToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.администрированиеToolStripMenuItem.Text = "Администрирование";
            // 
            // изменитьПарольДиспетчераToolStripMenuItem
            // 
            this.изменитьПарольДиспетчераToolStripMenuItem.Name = "изменитьПарольДиспетчераToolStripMenuItem";
            this.изменитьПарольДиспетчераToolStripMenuItem.Size = new System.Drawing.Size(303, 22);
            this.изменитьПарольДиспетчераToolStripMenuItem.Text = "Изменить пароль коммерческого диспетчера";
            this.изменитьПарольДиспетчераToolStripMenuItem.Click += new System.EventHandler(this.изменитьПарольКоммерческогоДиспетчераToolStripMenuItem_Click);
            // 
            // изменитьПарольАдминистратораToolStripMenuItem
            // 
            this.изменитьПарольАдминистратораToolStripMenuItem.Name = "изменитьПарольАдминистратораToolStripMenuItem";
            this.изменитьПарольАдминистратораToolStripMenuItem.Size = new System.Drawing.Size(303, 22);
            this.изменитьПарольАдминистратораToolStripMenuItem.Text = "Изменить пароль администратора";
            this.изменитьПарольАдминистратораToolStripMenuItem.Click += new System.EventHandler(this.изменитьПарольАдминистратораToolStripMenuItem_Click);
            // 
            // toolStripMenuItemИзменитьПарольНСС
            // 
            this.toolStripMenuItemИзменитьПарольНСС.Name = "toolStripMenuItemИзменитьПарольНСС";
            this.toolStripMenuItemИзменитьПарольНСС.Size = new System.Drawing.Size(303, 22);
            this.toolStripMenuItemИзменитьПарольНСС.Text = "Изменить пароль начальника смены станции";
            this.toolStripMenuItemИзменитьПарольНСС.Click += new System.EventHandler(this.изменитьПарольНСС_Click);
            // 
            // изментьСоставТЭЦГТПЩУToolStripMenuItem
            // 
            this.изментьСоставТЭЦГТПЩУToolStripMenuItem.Name = "изментьСоставТЭЦГТПЩУToolStripMenuItem";
            this.изментьСоставТЭЦГТПЩУToolStripMenuItem.Size = new System.Drawing.Size(303, 22);
            this.изментьСоставТЭЦГТПЩУToolStripMenuItem.Text = "Изменть состав ТЭЦ (ГТП, ЩУ)";
            this.изментьСоставТЭЦГТПЩУToolStripMenuItem.Click += new System.EventHandler(this.изментьСоставТЭЦГТПЩУToolStripMenuItem_Click);
            // 
            // изментьСоставПользовательToolStripMenuItem
            // 
            this.изментьСоставПользовательToolStripMenuItem.Name = "изментьСоставПользовательToolStripMenuItem";
            this.изментьСоставПользовательToolStripMenuItem.Size = new System.Drawing.Size(303, 22);
            this.изментьСоставПользовательToolStripMenuItem.Text = "Изменть состав пользователей";
            this.изментьСоставПользовательToolStripMenuItem.Click += new System.EventHandler(this.изментьСоставПользовательToolStripMenuItem_Click);
            // 
            // параметрыToolStripMenuItem
            // 
            this.параметрыToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.параметрыПриложенияToolStripMenuItem,
            this.параметрыТГБийскToolStripMenuItem});
            this.параметрыToolStripMenuItem.Name = "параметрыToolStripMenuItem";
            this.параметрыToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.параметрыToolStripMenuItem.Text = "Параметры";
            // 
            // параметрыПриложенияToolStripMenuItem
            // 
            this.параметрыПриложенияToolStripMenuItem.Name = "параметрыПриложенияToolStripMenuItem";
            this.параметрыПриложенияToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.параметрыПриложенияToolStripMenuItem.Text = "Параметры приложения";
            this.параметрыПриложенияToolStripMenuItem.Click += new System.EventHandler(this.параметрыПриложенияToolStripMenuItem_Click);
            // 
            // параметрыТГБийскToolStripMenuItem
            // 
            this.параметрыТГБийскToolStripMenuItem.Name = "параметрыТГБийскToolStripMenuItem";
            this.параметрыТГБийскToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.параметрыТГБийскToolStripMenuItem.Text = "Параметры ТГ Бийск";
            this.параметрыТГБийскToolStripMenuItem.Visible = false;
            this.параметрыТГБийскToolStripMenuItem.Click += new System.EventHandler(this.параметрыТГБийскToolStripMenuItem_Click);
            // 
            // оПрограммеToolStripMenuItem
            // 
            this.оПрограммеToolStripMenuItem.Name = "оПрограммеToolStripMenuItem";
            this.оПрограммеToolStripMenuItem.Size = new System.Drawing.Size(83, 20);
            this.оПрограммеToolStripMenuItem.Text = "О программе";
            this.оПрограммеToolStripMenuItem.Click += new System.EventHandler(this.оПрограммеToolStripMenuItem_Click);
            // 
            // tclTecViews
            // 
            this.tclTecViews.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tclTecViews.Location = new System.Drawing.Point(0, 24);
            this.tclTecViews.Name = "tclTecViews";
            this.tclTecViews.SelectedIndex = 0;
            this.tclTecViews.Size = new System.Drawing.Size(982, 735);
            this.tclTecViews.TabIndex = 3;
            this.tclTecViews.SelectedIndexChanged += new System.EventHandler(this.tclTecViews_SelectedIndexChanged);
            // 
            // timer
            // 
            this.timer.Interval = 1000;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // lblLabel
            // 
            this.lblLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLabel.AutoSize = true;
            this.lblLabel.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblLabel.Location = new System.Drawing.Point(772, 4);
            this.lblLabel.Name = "lblLabel";
            this.lblLabel.Size = new System.Drawing.Size(206, 17);
            this.lblLabel.TabIndex = 5;
            this.lblLabel.Text = "ОАО \"Новосибирскэнерго\"";
            this.lblLabel.Visible = false;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(982, 784);
            this.Controls.Add(this.lblLabel);
            this.Controls.Add(this.tclTecViews);
            this.Controls.Add(this.menuStrip);
            //this.Controls.Add(this.m_ContextMenuStripListTecViews);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.ContextMenuStrip = this.m_ContextMenuStripListTecViews;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Статистика";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.m_ContextMenuStripListTecViews.ResumeLayout(false);
            this.m_ContextMenuStripListTecViews.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ContextMenuStrip m_ContextMenuStripListTecViews;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem настройкиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem сменитьРежимToolStripMenuItem;
        private HTabCtrlEx /*System.Windows.Forms.TabControl*/ tclTecViews;
        private System.Windows.Forms.ToolStripMenuItem настройкиСоединенияБДКонфToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem настройкиСоединенияБДИсточникToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem текущееСостояниеПользовательToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem администрированиеToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem изменитьПарольДиспетчераToolStripMenuItem;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.ToolStripMenuItem изменитьПарольАдминистратораToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem оПрограммеToolStripMenuItem;
        private System.Windows.Forms.Label lblLabel;
        private System.Windows.Forms.ToolStripMenuItem видToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem панельГрафическихToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem параметрыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem параметрыПриложенияToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem параметрыТГБийскToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem изментьСоставТЭЦГТПЩУToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemИзменитьПарольНСС;
        private System.Windows.Forms.ToolStripMenuItem изментьСоставПользовательToolStripMenuItem;
    }
}
