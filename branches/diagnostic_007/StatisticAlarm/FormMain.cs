using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using StatisticCommon;

namespace StatisticAlarm
{
    public partial class FormMain : FormMainBaseWithStatusStrip
    {
        public static FormParameters formParameters;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object obj, EventArgs ev)
        {
            m_panelAlarm.Start();
        }

        private void FormMain_Activated(object obj, EventArgs ev)
        {
            m_panelAlarm.Activate(true);
        }

        private void FormMain_Deactivated(object obj, EventArgs ev)
        {
            m_panelAlarm.Activate(false);
        }

        private void fMenuItemAbout_Click(object obj, EventArgs ev)
        {
            using (FormAbout formAbout = new FormAbout ())
            {
                formAbout.ShowDialog ();
            }
        }

        private void FormMain_FormClosing(object obj, EventArgs ev)
        {
        }

        protected override void timer_Start()
        {
            throw new NotImplementedException();
        }

        protected override int UpdateStatusString()
        {
            throw new NotImplementedException();
        }

        protected override void HideGraphicsSettings()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateActiveGui(int type)
        {
            throw new NotImplementedException();
        }

        private void fMenuItemExit_Click (object obj, EventArgs ev)
        {
            Close ();
        }

        private void fMenuItemDBConfig_Click(object obj, EventArgs ev)
        {
        }
    }

    partial class FormMain
    {
        PanelAlarmJournal m_panelAlarm;
        Panel _panelMain;
        
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));            
            this.components = new System.ComponentModel.Container();

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = "FormStatisticAlarm";
            this.MinimumSize = new Size (800, 600);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject(@"StatisticAlarm")));

            this.MainMenuStrip = new MenuStrip ();
            this.MainMenuStrip.Items.AddRange (
                new ToolStripMenuItem [] {
                    new ToolStripMenuItem (@"Файл")
                    , new ToolStripMenuItem (@"Настройка")
                    , new ToolStripMenuItem (@"О программе")
                }
            );
            (this.MainMenuStrip.Items[0] as ToolStripMenuItem).DropDownItems.Add(new ToolStripMenuItem(@"Выход"));
            (this.MainMenuStrip.Items[0] as ToolStripMenuItem).DropDownItems[0].Click += new EventHandler(fMenuItemExit_Click);
            (this.MainMenuStrip.Items[1] as ToolStripMenuItem).DropDownItems.Add(new ToolStripMenuItem(@"БД конфигурации"));
            (this.MainMenuStrip.Items[1] as ToolStripMenuItem).DropDownItems[0].Click += new EventHandler(fMenuItemDBConfig_Click);
            (this.MainMenuStrip.Items[2] as ToolStripMenuItem).Click += new EventHandler(fMenuItemAbout_Click);

            m_panelAlarm = new PanelAlarmJournal(new ConnectionSettings(@"CONFIG_DB", @"10.100.104.18", 1433, @"techsite_cfg-2.X.X", @"client", @"client"), PanelAlarmJournal.MODE.SERVICE);

            _panelMain = new Panel ();
            _panelMain.Location = new Point(0, this.MainMenuStrip.Height);
            _panelMain.Size = new System.Drawing.Size(this.ClientSize.Width, this.ClientSize.Height - this.MainMenuStrip.Height - this.m_statusStripMain.Height);
            _panelMain.Anchor = (AnchorStyles)(((AnchorStyles.Left | AnchorStyles.Top) | AnchorStyles.Right) | AnchorStyles.Bottom);            
            _panelMain.Controls.Add (m_panelAlarm);

            this.SuspendLayout ();

            this.Controls.Add(this.MainMenuStrip);
            this.Controls.Add (_panelMain);

            this.ResumeLayout (false);
            this.PerformLayout ();

            this.Load += new EventHandler(FormMain_Load);
            this.Activated += new EventHandler(FormMain_Activated);
            this.FormClosing += new FormClosingEventHandler(FormMain_FormClosing);
        }

        #endregion
    }
}
