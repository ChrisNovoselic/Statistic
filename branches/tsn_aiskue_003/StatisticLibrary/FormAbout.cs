using System;
using System.Drawing;
using System.Windows.Forms;

using HClassLibrary;

/*
����� �������: ��������� �.�. 2012 �. - 
������������: ������� ������ 2012 - 2013 �.
������� �.�. (��) 2014 �. - 
������ �.�. 2013 �. - 
��������� �.�. 2016 �.
��������� �.�. 2015 - 2016 �.
*/

namespace StatisticCommon
{
    public partial class FormAbout : Form
    {
        public FormAbout(Image pic)
        {
            InitializeComponent();

            this.pictureBox1.Image = pic;

            m_lblProductVersion.Text = ProgramBase.AppProductVersion; //Application.ProductVersion; //Properties.Resources.TradeMarkVersion;

            m_lblDomainMashineUserName.Text = HUsers.UserDomainName;
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