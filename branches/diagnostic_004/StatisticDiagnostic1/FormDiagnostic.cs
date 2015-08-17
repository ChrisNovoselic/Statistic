using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HClassLibrary;

namespace StatisticDiagnostic1
{
    public partial class FormDiagnostic : Form
    {
        
        public FormDiagnostic()
        {
            InitializeComponent();            
        }

        public void FormDiagnostic_Load(object obj, EventArgs ev)
        {
            panelMain.Start();
            //panelMain.PingTimerThread();
        }

        public void FormDiagnostic_Close(object obj, EventArgs ev)
        {
            panelMain.Activate(false);
            panelMain.Stop();
        }

        private void FormDiagnostic_Activate(object obj, EventArgs ev)
        {
            panelMain.Activate(true);
            //panelMain.TimerFillPanel();
           
        }

        /*private void FormDiagnostic_Deactivate(object obj, EventArgs ev)
        {
            panelMain.Activate(false);
        }*/
    }
}
