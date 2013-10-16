using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Statistic
{
    public partial class FormTECComponent : Form
    {
        private ConnectionSettings m_connectionSetttings;

        public FormTECComponent(ConnectionSettings connSet)
        {
            m_connectionSetttings = connSet;

            InitializeComponent();

            for (int i = (int)ChangeMode.MODE_TECCOMPONENT.GTP; i < (int)ChangeMode.MODE_TECCOMPONENT.UNKNOWN; i++)
            {
                comboBoxMode.Items.Add(ChangeMode.getNameMode((short)i));
            }
            comboBoxMode.SelectedIndex = 0;
        }

        private void comboBoxMode_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            Close();
        }
    }
}
