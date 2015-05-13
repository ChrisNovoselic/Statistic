using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using HClassLibrary;
using System.Windows.Forms;


namespace StatisticDiagnostic
{
    public partial class FormStatisticDiagnostic : Form
    {
        public FormStatisticDiagnostic()
        {
            InitializeComponent();
        }

        private void FormStatisticDiagnostic_Load(object sender, EventArgs e)
        {

        }
        private void FormStatisticDiagnostic_Activate(object obj, EventArgs ev)
        {
            panelMain.Activate(true);
        }

        private void FormStatisticDiagnostic_Deactivate(object obj, EventArgs ev)
        {
            panelMain.Activate(false);
        }
                            
    }
}
