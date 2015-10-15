using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using HClassLibrary;

namespace StatisticTimeSync
{
    public partial class FormStatisticTimeSync : Form //FormMainBaseWithStatusStrip
    {
        public FormStatisticTimeSync()
        {
            InitializeComponent(); 
        }

        /// <summary>
        /// Запуск старта панели
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void FormStatisticTimeSync_Load(object obj, EventArgs ev)
        {
            m_panelMain.Start();
        }

        /// <summary>
        /// Активация формы
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void FormStatisticTimeSync_Activate(object obj, EventArgs ev)
        {
           m_panelMain.Activate(true);
        }

        /// <summary>
        /// Деактивация формы
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void FormStatisticTimeSync_Deactivate(object obj, EventArgs ev)
        {
           m_panelMain.Activate(false);  
        }
    }
}
