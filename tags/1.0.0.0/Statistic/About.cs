using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Statistic
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        private void llblMailTo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:" + llblMailTo.Text);
            btnClose.Select();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}