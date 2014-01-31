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
    public partial class FormPassword : FormPasswordBase
    {
        public FormPassword(Passwords p)
            : base(p)
        {
            InitializeComponent();
        }

        public override void SetIdPass(int id, Passwords.ID_ROLES id_role)
        {
            base.SetIdPass (id, id_role);

            labelOwnerPassword.Text = Passwords.getOwnerPass((int)m_idRolePassword);
        }

        private void tbxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                delegateStartWait();
                HAdmin.Errors errRes = m_pass.ComparePassword(tbxPassword.Text, (uint)m_idPassword, (uint)m_idRolePassword);
                delegateStopWait();
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

        protected override void FormPasswordBase_FormClosing(object sender, FormClosingEventArgs e)
        {
            base.FormPasswordBase_FormClosing (sender, e);
        }

        protected override void bntOk_Click(object sender, EventArgs e)
        {
            KeyEventArgs ee = new KeyEventArgs(Keys.Enter);
            tbxPassword_KeyDown(sender, ee);
        }

        protected override void btnCancel_Click(object sender, EventArgs e)
        {
            tbxPassword.Text = "";
            this.DialogResult = DialogResult.No;
            
            base.btnCancel_Click (sender, e);
        }

        protected override void FormPasswordBase_Shown(object sender, EventArgs e)
        {
            tbxPassword.Focus();
        }
    }
}