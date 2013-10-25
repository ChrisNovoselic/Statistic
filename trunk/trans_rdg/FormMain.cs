using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Statistic;

namespace trans_rdg
{
    public partial class FormMain : Form
    {
        Statistic.Admin m_panelAdmin;

        public FormMain()
        {
            InitializeComponent();

            //m_panelAdmin = new Admin (new InitTEC (), statusStripMain);

            //panelMain.Visible = false;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close ();
        }

        private void groupBoxFocus (GroupBox groupBox) {
            GroupBox groupBoxOther = null;
            bool bBackCoorChange = false;
            if (! (groupBox.BackColor == SystemColors.Info)) {
                groupBox.BackColor = SystemColors.Info;

                bBackCoorChange = true;
            }
            else
                ;

            switch (groupBox.Name) {
                case "groupBoxSource":
                    groupBoxOther = groupBoxDest;
                    break;
                case "groupBoxDest":
                    groupBoxOther = groupBoxSource;
                    break;
                default:
                    break;
            }

            if (bBackCoorChange)
                groupBoxOther.BackColor = SystemColors.Control;
            else
                ;
        }

        private void groupBox_MouseClick(object sender, MouseEventArgs e)
        {
            groupBoxFocus(((GroupBox)sender));
        }

        private void groupBox_Enter(object sender, EventArgs e)
        {
            groupBoxFocus(((GroupBox)sender));
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonClose_Click (null, null);
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About ();
            about.ShowDialog ();
        }

    }
}
