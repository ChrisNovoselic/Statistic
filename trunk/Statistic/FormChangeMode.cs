using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace StatisticCommon
{
    public partial class FormChangeMode : Form
    {
        public List<TEC> tec;
        public List<int> tec_index;
        public List<int> TECComponent_index;
        private CheckBox[] m_arCheckBoxTECComponent;
        public List<int> was_checked;
        public bool admin_was_checked;
        public bool closing;

        private ConnectionSettings m_connSet;

        public enum MODE_TECCOMPONENT : ushort { TEC, GTP, PC, TG, UNKNOWN };

        public FormChangeMode(ConnectionSettings connSet)
        {
            InitializeComponent();

            m_arCheckBoxTECComponent = new CheckBox[(int)MODE_TECCOMPONENT.UNKNOWN] { checkBoxTEC,
                                                                                        checkBoxGTP,
                                                                                        checkBoxPC,
                                                                                        checkBoxTG };

            InitTEC(connSet);
            
            m_arCheckBoxTECComponent[(int)MODE_TECCOMPONENT.PC].Checked = true;

            //FillListBoxTab();

            //clbMode.Items.Add("���������� ���");

            closing = false;
        }

        public int getModeTECComponent() {
            int iMode = 0;

            for (int i = (int)MODE_TECCOMPONENT.TEC; i < (int)MODE_TECCOMPONENT.UNKNOWN; i++)
            {
                if (m_arCheckBoxTECComponent[i].Checked == true) iMode |= (int)Math.Pow (2, i); else ;
            }

            return iMode;
        }

        public static string getPrefixMode(int indx)
        {
            String[] arPREFIX_COMPONENT = { "TEC", "GTP", "PC", "TG" };

            return arPREFIX_COMPONENT[indx];
        }

        public static string getNameMode (Int16 indx) {
            string [] nameModes = {"���", "���", "��", "��������", "����������"};

            return nameModes[indx];
        }
        
        public string getNameAdminValues (Int16 indx) {
            string[] arNameAdminValues = { "���", "���������", "���" };

            return @"��� - " + arNameAdminValues[indx];
        }

        public void InitTEC (ConnectionSettings connSet) {
            m_connSet = connSet;

            //this.tec = new InitTEC(m_connSet, (short) getModeTECComponent ()).tec;
            this.tec = new InitTEC(m_connSet).tec;
        }

        private void FillListBoxTab()
        {
            if (!(tec == null))
            {
                int index_tec = 0, index_gtp = 0;

                clbMode.Items.Clear();

                tec_index = new List<int>();
                TECComponent_index = new List<int>();
                was_checked = new List<int>();
                admin_was_checked = false;

                int iMode = getModeTECComponent(),
                    iSheet = 0;

                foreach (TEC t in tec)
                {
                    if ((iMode & ((int)Math.Pow(2, (int)(MODE_TECCOMPONENT.TEC + 0)))) == (int)Math.Pow(2, (int)(MODE_TECCOMPONENT.TEC + 0)))
                    {
                        clbMode.Items.Add(t.name);
                        tec_index.Add(index_tec);
                        TECComponent_index.Add(-1);
                    }
                    else
                        ;

                    if (t.list_TECComponents.Count > 0)
                    {
                        index_gtp = 0;
                        foreach (TECComponent g in t.list_TECComponents)
                        {
                            if ((((g.m_id > 100) && (g.m_id < 500)) && ((iMode & ((int)Math.Pow(2, (int)(MODE_TECCOMPONENT.GTP + 0)))) == ((int)Math.Pow(2, (int)(MODE_TECCOMPONENT.GTP + 0)))) ||
                                (((g.m_id > 500) && (g.m_id < 1000)) && ((iMode & ((int)Math.Pow(2, (int)(MODE_TECCOMPONENT.PC + 0)))) == ((int)Math.Pow(2, (int)(MODE_TECCOMPONENT.PC + 0))))) ||
                                (((g.m_id > 1000) && (g.m_id < 10000)) && ((iMode & ((int)Math.Pow(2, (int)(MODE_TECCOMPONENT.TG + 0)))) == ((int)Math.Pow(2, (int)(MODE_TECCOMPONENT.TG + 0)))))))
                            {
                                clbMode.Items.Add(t.name + " - " + g.name);
                                tec_index.Add(index_tec);
                                TECComponent_index.Add(index_gtp);
                            }
                            else
                                ;

                            index_gtp++;
                        }
                    }
                    index_tec++;
                }

                //clbMode.Items.Add("�������������� ���");
                //clbMode.Items.Add(getNameAdminValues((short)getModeTECComponent()));
            }
            else
                ;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            int i;
            //if (clbMode.CheckedIndices.Count == 0)
            //{
            //    MessageBox.Show("�� �� ������� ������������ �������, �������� ���� �� ����", "������", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            //    return;
            //}

            was_checked.Clear();
            admin_was_checked = false;

            for (i = 0; i < clbMode.CheckedIndices.Count; i++) {
                if (clbMode.CheckedIndices[i] == clbMode.Items.Count - 1)
                    admin_was_checked = true;
                else
                    was_checked.Add(clbMode.CheckedIndices[i]);
            }
            this.DialogResult = DialogResult.OK;
            closing = true;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            btnOk.Focus();
            this.DialogResult = DialogResult.Cancel;
            closing = true;
            Close();
        }

        private void ChangeMode_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closing)
                e.Cancel = true;
            else
                closing = false;
        }

        private void btnSetAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbMode.Items.Count; i++)
                clbMode.SetItemChecked(i, true);
            btnOk.Focus();
        }

        public void btnClearAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbMode.Items.Count; i++)
                clbMode.SetItemChecked(i, false);
            btnOk.Focus();
        }

        private void ChangeMode_Shown(object sender, EventArgs e)
        {
            clbMode.SetItemChecked(clbMode.Items.Count - 1, admin_was_checked);
        }

        private void comboBoxModeTEC_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.InitTEC (m_connSet);

            closing = false;
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            FillListBoxTab();
        }
    }
}