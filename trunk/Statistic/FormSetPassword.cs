using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using StatisticCommon;

namespace Statistic
{
    public partial class FormSetPassword : Form
    {
        private uint m_idPass;

        private PanelAdmin m_panelAdmin;
        private bool closing;

        public FormSetPassword(PanelAdmin a)
        {
            InitializeComponent();
            m_panelAdmin = a;
            closing = false;
        }

        public void SetIdPass(uint id)
        {
            m_idPass = id;

            string[] ownersPass = { "Коммерческий диспетчер", "Администратор", "ДИС" };

            this.Text = ownersPass[m_idPass - 1];
        }

        public uint GetIdPass() { return m_idPass; }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (tbxNewPassword.Text == tbxNewPasswordAgain.Text)
            {
                m_panelAdmin.FormSetPassword(tbxNewPassword.Text, m_idPass);
                closing = true;
                Close();
            }
            else
            {
                tbxNewPassword.Text = tbxNewPasswordAgain.Text = "";
                MessageBox.Show(this, "Вы неправильно повторили пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            closing = true;
            Close();
        }

        private void SetPassword_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closing)
            {
                e.Cancel = true;
            }
            else
            {
                closing = false;
                tbxNewPassword.Text = tbxNewPasswordAgain.Text = "";
            }
        }

        private void tbxNewPasswordAgain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnOk_Click(sender, new EventArgs());
        }

        private void SetPassword_Shown(object sender, EventArgs e)
        {
            tbxNewPassword.Focus();
        }
    }
}