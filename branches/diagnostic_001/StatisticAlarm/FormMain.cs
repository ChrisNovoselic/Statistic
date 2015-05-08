using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;

namespace StatisticAlarm
{
    public partial class FormMain : Form //FormMainBaseWithStatusStrip
    {
        public static FormParameters formParameters;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormStatisticAlarm_Load(object obj, EventArgs ev)
        {
            m_panelMain.Start();
        }

        private void FormStatisticTimeSync_Activate(object obj, EventArgs ev)
        {
            m_panelMain.Activate(true);
        }

        private void FormStatisticTimeSync_Deactivate(object obj, EventArgs ev)
        {
            m_panelMain.Activate(false);
        }
    }

    partial class FormMain
    {
        PanelAlarmJournal m_panelMain;
        
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
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = "FormStatisticAlarm";
        }

        #endregion
    }
}
