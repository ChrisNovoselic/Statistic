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
        private Admin admin;
        private bool closing;

        public SetPassword(Admin a)
        {
            InitializeComponent();
            admin = a;
            closing = false;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (tbxNewPassword.Text == tbxNewPasswordAgain.Text)
            {
                admin.SetPassword(tbxNewPassword.Text, true);
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