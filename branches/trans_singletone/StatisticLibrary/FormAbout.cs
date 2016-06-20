using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;

namespace StatisticCommon
{
    public partial class FormAbout : Form
    {
        public FormAbout(Image pic)
        {
            InitializeComponent();

            this.pictureBox1.Image = pic;

            m_lblProductVersion.Text = ProgramBase.AppProductVersion; //Application.ProductVersion; //Properties.Resources.TradeMarkVersion;

            m_lblDomainMashineUserName.Text = Environment.UserDomainName + @"\" + Environment.UserName
                + @" на " + Environment.MachineName;
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