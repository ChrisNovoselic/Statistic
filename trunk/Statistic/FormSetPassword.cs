using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    public partial class FormSetPassword : FormPasswordBase
    {
        public FormSetPassword(Passwords p) : base (p)
        {
            InitializeComponent();
        }

        public void SetIdPass(int idListener, int id_ext, Passwords.ID_ROLES id_role)
        {
            base.SetIdPass(id_ext, id_role);

            this.Text = Passwords.getOwnerPass ((int)m_idRolePassword);
            this.Text = this.Text.Substring (0, this.Text.Length - 1);
        }

        protected override void bntOk_Click(object sender, EventArgs e)
        {
            if (tbxNewPassword.Text == tbxNewPasswordAgain.Text)
            {
                delegateStartWait ();
                m_pass.SetPassword(tbxNewPassword.Text, (uint)m_idExtPassword, (uint)m_idRolePassword);
                delegateStopWait();
                
                closing = true;
                Close();
            }
            else
            {
                tbxNewPassword.Text = tbxNewPasswordAgain.Text = "";
                MessageBox.Show(this, "�� ����������� ��������� ������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                tbxNewPassword.Text = tbxNewPasswordAgain.Text = string.Empty;
            }
        }

        private void tbxNewPasswordAgain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                //btnOk_Click(sender, new EventArgs());
                btnOk.PerformClick ();
            else
                ;
        }

        protected override void FormPasswordBase_Shown(object sender, EventArgs e)
        {
            tbxNewPassword.Focus();
        }
    }
}