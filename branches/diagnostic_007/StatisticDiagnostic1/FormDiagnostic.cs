using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Drawing;
using System.Text;
using StatisticCommon;
using System.Windows.Forms;
using HClassLibrary;

namespace StatisticDiagnostic
{
    public partial class FormDiagnostic : FormMainBase
    {
        public FormDiagnostic()
        {
            InitializeComponent();
        }

        public void FormDiagnostic_Load(object obj, EventArgs ev)
        {
            panelMain.Start();
        }

        private void FormDiagnostic_Activate(object obj, EventArgs ev)
        {
            panelMain.Activate(true);
        }

        private void FormDiagnostic_Deactivate(object obj, EventArgs ev)
        {
            panelMain.Activate(false);
        }
    }

}
