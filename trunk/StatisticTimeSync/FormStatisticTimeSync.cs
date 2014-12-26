using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StatisticTimeSync
{
    public partial class FormStatisticTimeSync : Form
    {
        public FormStatisticTimeSync()
        {
            InitializeComponent();
        }

        private void FormStatisticTimeSync_Load (object obj, EventArgs ev) {
            m_panelMain.OnLoad ();
        }
    }
}
