using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace StatisticCommon
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();

            m_lblProductVersion.Text = Application.ProductVersion/*Properties.Resources.TradeMarkVersion*/;
        }

        private void llblMailTo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:" + ((LinkLabel)sender).Text);
            btnClose.Select();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}