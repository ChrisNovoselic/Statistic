using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;

namespace trans_rdg
{
    public partial class FormMain : FormMainBase
    {
        Admin m_admin;

        public FormMain()
        {
            InitializeComponent();

            FormConnectionSettings formConnSett = new FormConnectionSettings();
            ConnectionSettings connSett = formConnSett.getConnSett();
            m_admin = new Admin(new InitTEC(connSett, (short)FormChangeMode.MODE_TECCOMPONENT.GTP).tec);
            m_admin.connSettConfigDB = formConnSett.getConnSett();

            m_admin.SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
            m_admin.SetDelegateReport(ErrorReport, ActionReport);

            m_admin.StartDbInterface ();

            //panelMain.Visible = false;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            m_admin.StopDbInterface ();

            Close ();
        }

        private void groupBoxFocus (GroupBox groupBox) {
            GroupBox groupBoxOther = null;
            bool bBackColorChange = false;
            if (! (groupBox.BackColor == SystemColors.Info)) {
                groupBox.BackColor = SystemColors.Info;

                bBackColorChange = true;
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

            if (bBackColorChange)
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

        private void ErrorReport (string msg) {
            statusStripMain.Text = msg;
        }

        private void ActionReport(string msg)
        {
            statusStripMain.Text = msg;
        }
    }
}
