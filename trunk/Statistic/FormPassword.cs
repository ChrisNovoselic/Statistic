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
    public partial class FormPassword : FormMainBase
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

            string[] ownersPass = { "коммерческого диспетчера", "администратора", "НССа" };

            labelOwnerPassword.Text = ownersPass[(int)m_idPass - 1];
        }

        public uint GetIdPass() { return (uint)m_idPass; }

        private void tbxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //StartWait();
                HAdmin.Errors errRes = m_pass.ComparePassword(tbxPassword.Text, (uint)m_idPass);
                tbxPassword.Clear ();
                //StopWait();
                switch (errRes)
                {
                    case HAdmin.Errors.NoAccess:
                        this.DialogResult = DialogResult.Abort;
                        closing = true;
                        Close();
                        break;
                    case HAdmin.Errors.NoSet:
                        if (MessageBox.Show(this, "Хотите установить?", "Ошибка", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                        {
                            this.DialogResult = DialogResult.Retry;
                            closing = true;
                            Close();
                        }
                        else
                        {
                        }
                        break;
                    case HAdmin.Errors.InvalidValue:
                        if (MessageBox.Show(this, "Хотите попробовать снова?", "Ошибка", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                        {
                        }
                        else
                        {
                            this.DialogResult = DialogResult.No;
                            closing = true;
                            Close();
                        }
                        break;
                    case HAdmin.Errors.ParseError:
                    case HAdmin.Errors.NoError:
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

        protected override void EventRaised()
        {
        }
    }
}