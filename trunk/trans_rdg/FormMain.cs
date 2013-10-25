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
        Statistic.PanelAdmin m_panelAdmin;

        public FormMain()
        {
            InitializeComponent();

            FormConnectionSettings formConnectionSettings = new FormConnectionSettings ();
            ConnectionSettings connSett = formConnectionSettings.getConnSett ();
            m_panelAdmin = new PanelAdmin(new InitTEC(connSett, (short)FormChangeMode.MODE_TECCOMPONENT.GTP).tec, statusStripMain);

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
            FormAbout formAbout = new FormAbout ();
            formAbout.ShowDialog();
        }

    }
}
