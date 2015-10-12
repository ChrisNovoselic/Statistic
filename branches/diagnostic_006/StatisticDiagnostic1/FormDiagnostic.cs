using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Drawing;
using System.Text;
using StatisticCommon;
using System.Windows.Forms;
using HClassLibrary;

namespace StatisticDiagnostic1
{
    public partial class FormDiagnostic : FormMainBase
    {
        public FormDiagnostic()
        {
            InitializeComponent();
            panelMain.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);
        }

        public void FormDiagnostic_Load(object obj, EventArgs ev)
        {
            panelMain.start();
        }

        public void FormDiagnostic_Close(object obj, EventArgs ev)
        {
            panelMain.Activate(false);
            panelMain.Stop();
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
