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
        public List<int> tec_index,
                        TECComponent_index,
                        across_index,
                        m_listAcrossIndexCheckedIndices;
        private CheckBox[] m_arCheckBoxTECComponent;
        public List<int> was_checked;
        public bool /*[]*/ admin_was_checked;
        public bool closing;

        //private ConnectionSettings m_connSet;

        public enum MODE_TECCOMPONENT : ushort { TEC, GTP, PC, TG, UNKNOWN };
        public enum MANAGER : ushort { DISP, NSS, COUNT_MANAGER };

        public FormChangeMode(List <TEC> tec)
        {
            InitializeComponent();

            this.tec = tec;

            m_arCheckBoxTECComponent = new CheckBox[(int)MODE_TECCOMPONENT.UNKNOWN] { checkBoxTEC,
                                                                                        checkBoxGTP,
                                                                                        checkBoxPC,
                                                                                        checkBoxTG };

            admin_was_checked = false; //new bool [2] {false, false};

            tec_index = new List<int>();
            TECComponent_index = new List<int>();
            across_index = new List<int>();
            m_listAcrossIndexCheckedIndices = new List <int> ();
            was_checked = new List<int>();
            
            m_arCheckBoxTECComponent[(int)MODE_TECCOMPONENT.GTP].Checked = true;

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

        public bool IsModeTECComponent (MODE_TECCOMPONENT mode) {
            bool bRes = false;
            int offset = 0;

            if ((getModeTECComponent() & ((int)Math.Pow(2, (int)(mode) + offset))) == (int)Math.Pow(2, (int)(mode) + offset))
            {
                bRes = true;
            }
            else
                ;

            return bRes;
        }

        public static string getPrefixMode(int indx)
        {
            String[] arPREFIX_COMPONENT = { "TEC", "GTP", "PC", "TG" };

            return arPREFIX_COMPONENT[indx];
        }

        public static string getNameMode (Int16 indx) {
            string [] nameModes = {"ТЭЦ", "ГТП", "ЩУ", "Поблочно", "Неизвестно"};

            return nameModes[indx];
        }

        public string getNameAdminValues (MODE_TECCOMPONENT mode) {
            string[] arNameAdminValues = { "ДИС", "Диспетчер", "ДИС", "ДИС" };

            return @"ПБР - " + arNameAdminValues[(int)mode];
        }

        private void FillListBoxTab()
        {
            if (!(tec == null))
            {
                int tec_indx = 0, comp_indx = 0, across_indx = -1;

                foreach (int indx in clbMode.CheckedIndices)
                {
                    if (m_listAcrossIndexCheckedIndices.IndexOf(across_index[indx]) < 0)
                        m_listAcrossIndexCheckedIndices.Add(across_index[indx]);
                    else
                        ;
                }

                clbMode.Items.Clear();

                tec_index.Clear();
                TECComponent_index.Clear();
                across_index.Clear();
                //was_checked.Clear ();

                foreach (TEC t in tec)
                {
                    across_indx++;

                    if (IsModeTECComponent(MODE_TECCOMPONENT.TEC) == true)
                    {
                        clbMode.Items.Add(t.name);

                        tec_index.Add(tec_indx);
                        TECComponent_index.Add(-1);

                        across_index.Add (across_indx);

                        if (!(m_listAcrossIndexCheckedIndices.IndexOf(across_indx) < 0))
                            clbMode.SetItemChecked(clbMode.Items.Count - 1, true);
                        else
                            ;
                    }
                    else
                        ;

                    if (t.list_TECComponents.Count > 0)
                    {
                        comp_indx = 0;
                        foreach (TECComponent g in t.list_TECComponents)
                        {
                            across_indx++;

                            if ((((g.m_id > 100) && (g.m_id < 500)) && (IsModeTECComponent (MODE_TECCOMPONENT.GTP))) ||
                                (((g.m_id > 500) && (g.m_id < 1000)) && (IsModeTECComponent (MODE_TECCOMPONENT.PC))) ||
                                (((g.m_id > 1000) && (g.m_id < 10000)) && (IsModeTECComponent (MODE_TECCOMPONENT.TG))))
                            {
                                clbMode.Items.Add(t.name + " - " + g.name);

                                tec_index.Add(tec_indx);
                                TECComponent_index.Add(comp_indx);

                                across_index.Add(across_indx);

                                if (!(m_listAcrossIndexCheckedIndices.IndexOf(across_indx) < 0))
                                    clbMode.SetItemChecked(clbMode.Items.Count - 1, true);
                                else
                                    ;
                            }
                            else
                                ;

                            comp_indx++;
                        }
                    }
                    else
                        ;

                    tec_indx++;
                }

                if ((getModeTECComponent() > 0) && (tec.Count > 0))
                    if (IsModeTECComponent (MODE_TECCOMPONENT.GTP)) {
                        clbMode.Items.Add(getNameAdminValues(MODE_TECCOMPONENT.GTP));
                    }
                    else
                        clbMode.Items.Add(getNameAdminValues((short)MODE_TECCOMPONENT.TEC)); //PC, TG - не важно
                else
                    ;
            }
            else
                ;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            int i;
            //if (clbMode.CheckedIndices.Count == 0)
            //{
            //    MessageBox.Show("Вы не выбрали отображаемые объекты, выберите хотя бы один", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            //    return;
            //}

            was_checked.Clear();
            admin_was_checked = false;

            for (i = 0; i < clbMode.CheckedIndices.Count; i++) {
                //(IsModeTECComponent (MODE_TECCOMPONENT.GTP) == true)) || (IsModeTECComponent (MODE_TECCOMPONENT.TG) == true)
                //if (((IsModeTECComponent(MODE_TECCOMPONENT.GTP) == true) || (IsModeTECComponent(MODE_TECCOMPONENT.TG) == true)) && (clbMode.CheckedIndices[i] == clbMode.Items.Count - 1))
                if ((getModeTECComponent () > 0) && (clbMode.CheckedIndices[i] == clbMode.Items.Count - 1))
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
            //if ((IsModeTECComponent(MODE_TECCOMPONENT.GTP) == true) || (IsModeTECComponent(MODE_TECCOMPONENT.TG) == true))
            if ((getModeTECComponent() > 0) && (tec.Count > 0))
                clbMode.SetItemChecked(clbMode.Items.Count - 1, admin_was_checked);
            else
                ;
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            FillListBoxTab();
        }
    }
}