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
        public List<TEC> m_list_tec;
        public List<int> m_list_tec_index,
                        m_list_TECComponent_index
                        , m_list_across_index
                        , m_listAcrossIndexCheckedIndices
                        , m_list_IdItem
                        ;
        private CheckBox[] m_arCheckBoxTECComponent;
        public List<int> was_checked;
        public bool /*[]*/ admin_was_checked;
        public bool closing;

        public System.Windows.Forms.ContextMenuStrip m_MainFormContextMenuStripListTecViews;
        DelegateFunc f_������������;

        //private ConnectionSettings m_connSet;

        public enum MODE_TECCOMPONENT : ushort { TEC, GTP, PC, TG, UNKNOWN };
        public enum MANAGER : ushort { DISP, NSS, COUNT_MANAGER };

        public FormChangeMode(List <TEC> tec, System.Windows.Forms.ContextMenuStrip FormMainContextMenuStrip /*= null*/, DelegateFunc changeMode)
        {
            InitializeComponent();
            this.Text = @"����� ������";

            m_MainFormContextMenuStripListTecViews = FormMainContextMenuStrip;
            m_MainFormContextMenuStripListTecViews.ItemClicked += new ToolStripItemClickedEventHandler(MainFormContextMenuStripListTecViews_ItemClicked);
            f_������������ = changeMode;

            this.m_list_tec = tec;

            m_arCheckBoxTECComponent = new CheckBox[(int)MODE_TECCOMPONENT.UNKNOWN] { checkBoxTEC,
                                                                                        checkBoxGTP,
                                                                                        checkBoxPC,
                                                                                        checkBoxTG };

            admin_was_checked = false; //new bool [2] {false, false};

            m_list_tec_index = new List<int>();
            m_list_TECComponent_index = new List<int>();
            m_list_across_index = new List<int>();
            m_listAcrossIndexCheckedIndices = new List <int> ();
            m_list_IdItem = new List<int>();
            was_checked = new List<int>();

            m_arCheckBoxTECComponent[(int)MODE_TECCOMPONENT.TEC].Checked = true;
            m_arCheckBoxTECComponent[(int)MODE_TECCOMPONENT.GTP].Checked = true;

            closing = false;
        }

        /// <summary>
        /// ���������� ����� (int), ��������� �������������
        /// ��� ������������ ������ �����������
        /// </summary>
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

        /// <summary>
        /// ����� (�����������) �������� �������� �� ��� ������ ������������ ��� ������
        /// ���������� ������������� ��� ����������� ������ �������
        /// </summary>
        /// <param name="checkMode">����� ��� ��������</param>
        /// <param name="mode">��� ��� ������</param>
        /// <returns></returns>
        public static bool IsModeTECComponent(int checkMode, MODE_TECCOMPONENT mode)
        {
            bool bRes = false;
            int offset = 0;

            if ((checkMode & ((int)Math.Pow(2, (int)(mode) + offset))) == (int)Math.Pow(2, (int)(mode) + offset))
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
            string [] nameModes = {"���", "���", "��", "��������", "����������"};

            return nameModes[indx];
        }

        public string getNameAdminValues (MODE_TECCOMPONENT mode) {
            string[] arNameAdminValues = { "���", "���������", "���", "���" };

            return @"��� - " + arNameAdminValues[(int)mode];
        }

        private void FillListAcrossIndexCheckedIndicies ()
        {
            foreach (int indx in m_list_across_index)
            {
                if (clbMode.CheckedIndices.IndexOf(m_list_across_index.IndexOf(indx)) < 0)
                    if (!(m_listAcrossIndexCheckedIndices.IndexOf(indx) < 0))
                        m_listAcrossIndexCheckedIndices.RemoveAt(m_listAcrossIndexCheckedIndices.IndexOf(indx));
                    else
                        ;
                else                    
                    if (m_listAcrossIndexCheckedIndices.IndexOf(indx) < 0)
                        m_listAcrossIndexCheckedIndices.Add(indx);
                    else
                        ;
            }
        }

        private void FillListBoxTab()
        {
            if (! (m_MainFormContextMenuStripListTecViews == null)) m_MainFormContextMenuStripListTecViews.Items.Clear(); else ;
            
            if (!(m_list_tec == null))
            {
                int tec_indx = 0, comp_indx = 0, across_indx = -1;

                FillListAcrossIndexCheckedIndicies ();

                clbMode.Items.Clear();

                m_list_IdItem.Clear ();

                m_list_tec_index.Clear();
                m_list_TECComponent_index.Clear();
                
                m_list_across_index.Clear();
                
                //was_checked.Clear ();

                foreach (TEC t in m_list_tec)
                {
                    across_indx++;

                    if (IsModeTECComponent(MODE_TECCOMPONENT.TEC) == true)
                    {
                        clbMode.Items.Add(t.name_shr);
                        if (!(m_MainFormContextMenuStripListTecViews == null)) m_MainFormContextMenuStripListTecViews.Items.Add(t.name_shr); else ;

                        m_list_IdItem.Add(t.m_id);

                        m_list_tec_index.Add(tec_indx);
                        m_list_TECComponent_index.Add(-1);

                        m_list_across_index.Add(across_indx);

                        if (!(m_listAcrossIndexCheckedIndices.IndexOf(across_indx) < 0)) {
                            clbMode.SetItemChecked(clbMode.Items.Count - 1, true);
                        }
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
                                clbMode.Items.Add(t.name_shr + " - " + g.name_shr);
                                if (!(m_MainFormContextMenuStripListTecViews == null)) m_MainFormContextMenuStripListTecViews.Items.Add(t.name_shr + " - " + g.name_shr); else ;

                                m_list_IdItem.Add(g.m_id);

                                m_list_tec_index.Add(tec_indx);
                                m_list_TECComponent_index.Add(comp_indx);

                                m_list_across_index.Add(across_indx);

                                if (!(m_listAcrossIndexCheckedIndices.IndexOf(across_indx) < 0)) {
                                    clbMode.SetItemChecked(clbMode.Items.Count - 1, true);
                                }
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

                if ((getModeTECComponent() > 0) && (m_list_tec.Count > 0))
                    if (IsModeTECComponent (MODE_TECCOMPONENT.GTP)) {
                        clbMode.Items.Add(getNameAdminValues(MODE_TECCOMPONENT.GTP));
                    }
                    else
                        clbMode.Items.Add(getNameAdminValues((short)MODE_TECCOMPONENT.TEC)); //PC, TG - �� �����
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
            //    MessageBox.Show("�� �� ������� ������������ �������, �������� ���� �� ����", "������", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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

            FillListAcrossIndexCheckedIndicies ();

            //��������� ����������� ������ ������
            if (! (e == EventArgs.Empty)) {
                this.DialogResult = DialogResult.OK;
                closing = true;
                Close();
            }
            else
                ;

            f_������������();
        }

        private void clbMode_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!(m_MainFormContextMenuStripListTecViews == null))
                ((ToolStripMenuItem)m_MainFormContextMenuStripListTecViews.Items[e.Index]).CheckState = e.NewValue;
            else ;
        }

        private void MainFormContextMenuStripListTecViews_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int indx = m_MainFormContextMenuStripListTecViews.Items.IndexOf (e.ClickedItem);
            bool bChecked = false;
            switch (((ToolStripMenuItem)m_MainFormContextMenuStripListTecViews.Items[indx]).CheckState) {
                case CheckState.Checked:
                    break;
                case CheckState.Indeterminate:
                    break;
                case CheckState.Unchecked:
                    bChecked = true;
                    break;
                default:
                    break;
            }
            clbMode.SetItemChecked(indx, bChecked);

            btnOk_Click (null, EventArgs.Empty);
        }

        public void SetItemChecked(int indxCheckedIndicies, bool bChecked)
        {
            clbMode.SetItemChecked(clbMode.CheckedIndices[indxCheckedIndicies], bChecked);

            btnOk_Click(null, EventArgs.Empty);
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
            if (closing == false)
                e.Cancel = true;
            else {
                closing = false;

                //if (this.DialogResult == System.Windows.Forms.DialogResult.OK)
                //    f_������������();
                //else
                //    ;
            }
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
            if ((getModeTECComponent() > 0) && (m_list_tec.Count > 0) && (clbMode.Items.Count > 0))
                clbMode.SetItemChecked(clbMode.Items.Count - 1, admin_was_checked);
            else
                ;
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            FillListBoxTab();
        }

        public string getIdItemsCheckedIndicies ()
        {
            string strRes = string.Empty;
            int i = -1;

            for (i = 0; i < was_checked.Count; i ++)
            {
                strRes += m_list_IdItem[was_checked[i]];
                if ((i + 1) < was_checked.Count)
                    strRes += ",";
                else
                    ;
            }

            return strRes;
        }
    }
}