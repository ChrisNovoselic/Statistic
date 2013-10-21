using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Statistic
{
    public partial class Password : Form
    {
        private uint m_idPass;

        private Admin admin;
        private bool closing;

        public Password(Admin a)
        {
            InitializeComponent();
            admin = a;
            closing = false;
        }

        public void SetIdPass (uint id) {
            m_idPass = id;

            string errMsg = string.Empty;
            switch (m_idPass) {
                case 1:
                    errMsg = "коммерческого диспетчера";
                    break;
                case 2:
                    errMsg = "администратора";
                    break;
                default:
                    break;
            }

            labelOwnerPassword.Text = errMsg;
        }

        public uint GetIdPass() { return m_idPass; }

        private void tbxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (admin.ComparePassword(tbxPassword.Text, m_idPass))
                {
                    this.DialogResult = DialogResult.Yes;
                    tbxPassword.Text = "";
                    closing = true;
                    Close();
                }
                else
                {
                    if (MessageBox.Show(this, "Хотите попробовать снова?", "Ошибка", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                    {
                        tbxPassword.Text = "";
                    }
                    else
                    {
                        tbxPassword.Text = "";
                        this.DialogResult = DialogResult.No;
                        closing = true;
                        Close();
                    }
                }
            }
        }

        private void Password_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closing)
                e.Cancel = true;
            else
                closing = false;
        }

        private void bntOk_Click(object sender, EventArgs e)
        {
            KeyEventArgs ee = new KeyEventArgs(Keys.Enter);
            tbxPassword_KeyDown(sender, ee);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            tbxPassword.Text = "";
            this.DialogResult = DialogResult.No;
            closing = true;
            Close();
        }

        private void Password_Shown(object sender, EventArgs e)
        {
            tbxPassword.Focus();
        }
    }
}