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
    public partial class FormPassword : Form
    {
        public enum ID_ROLES : uint {COM_DISP = 1, ADMIN, NSS};

        private ID_ROLES m_idPass;

        private Passwords m_pass;
        private bool closing;

        public FormPassword(Passwords p)
        {
            InitializeComponent();
            m_pass = p;
            closing = false;
        }

        public void SetIdPass(ID_ROLES id)
        {
            m_idPass = id;

            string[] ownersPass = { "������������� ����������", "��������������", "����" };

            labelOwnerPassword.Text = ownersPass[(int)m_idPass - 1];
        }

        public uint GetIdPass() { return (uint)m_idPass; }

        private void tbxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                tbxPassword.Text = string.Empty;

                switch (m_pass.ComparePassword(tbxPassword.Text, (uint)m_idPass))
                {
                    case Admin.Errors.NoAccess:
                        this.DialogResult = DialogResult.None;
                        closing = true;
                        Close();
                        break;
                    case Admin.Errors.NoSet:
                        if (MessageBox.Show(this, "������ ����������?", "������", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                        {
                            this.DialogResult = DialogResult.Retry;
                            closing = true;
                            Close();
                        }
                        else
                        {
                        }
                        break;
                    case Admin.Errors.InvalidValue:
                        if (MessageBox.Show(this, "������ ����������� �����?", "������", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                        {
                        }
                        else
                        {
                            this.DialogResult = DialogResult.No;
                            closing = true;
                            Close();
                        }
                        break;
                    case Admin.Errors.ParseError:
                    case Admin.Errors.NoError:
                        this.DialogResult = DialogResult.Yes;
                        closing = true;
                        Close();
                        break;
                    default:
                        break;
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