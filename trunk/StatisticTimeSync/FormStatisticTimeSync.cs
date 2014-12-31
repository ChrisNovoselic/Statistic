using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;

namespace StatisticTimeSync
{
    public partial class FormStatisticTimeSync : Form //FormMainBaseWithStatusStrip
    {
        public FormStatisticTimeSync()
        {
            InitializeComponent();
        }

        private void FormStatisticTimeSync_Load (object obj, EventArgs ev) {
            m_panelMain.Initialize ();
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
}
