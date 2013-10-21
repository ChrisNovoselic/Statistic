using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Statistic
{
    public partial class SetPassword : Form
    {
        private uint m_idPass;

        private Admin admin;
        private bool closing;

        public SetPassword(Admin a)
        {
            InitializeComponent();
            admin = a;
            closing = false;
        }

        public void SetIdPass(uint id)
        {
            m_idPass = id;

            string errMsg = string.Empty;
            switch (m_idPass)
            {
                case 1:
                    errMsg = "Коммерческий диспетчер";
                    break;
                case 2:
                    errMsg = "Администратор";
                    break;
                default:
                    break;
            }

            this.Text = errMsg;
        }

        public uint GetIdPass() { return m_idPass; }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (tbxNewPassword.Text == tbxNewPasswordAgain.Text)
            {
                admin.SetPassword(tbxNewPassword.Text, m_idPass);
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