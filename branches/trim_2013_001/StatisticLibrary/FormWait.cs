using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace StatisticCommon
{
    public partial class FormWait : Form
    {
        private bool started;
        public FormWait()
        {
            InitializeComponent();
            started = false;
        }

        public void StartWaitForm()
        {
            if (started == false)
            {
                started = true;
                this.ShowDialog(Parent);
            }
            else
                ;
        }

        public void StopWaitForm()
        {
            if (started == true)
            {
                started = false;
                this.Close();
            }
            else
                ;
        }

        private void WaitForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (started == true)
                e.Cancel = true;
            else
                ;
        }
    }
}