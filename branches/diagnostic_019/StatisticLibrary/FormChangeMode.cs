using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using HClassLibrary;

namespace StatisticCommon
{
    public partial class FormChangeMode : Form
    {
        public event DelegateFunc EventMenuItemsClear;
        public event DelegateStringFunc EventMenuItemAdd;
        public event DelegateObjectFunc EventChangeMode;

        public event DelegateFunc ev_������������;
        /// <summary>
        /// ����� ��� �������� �������� � ������ ����������� ���
        /// </summary>
        public class Item
        {
            public int id;
            public string name_shr;
            public bool bChecked
                , bVisibled;

            public Item(int id, string name_shr, bool bChecked)
            {
                this.id = id;
                this.name_shr = name_shr;
                this.bChecked = bChecked;
                bVisibled = false;
            }
        }

        public List<TEC> m_list_tec;
        public List<Item> m_listItems;
        private List<Item> m_list_change_items;
        /// <summary>
        /// ������ ���������-������������� ��� ������ ���� ������
        ///  (������ ��� ����������� ����������� ���)
        /// </summary>
        private List <CheckBox> m_listCheckBoxTECComponent;

        public bool closing;

        public System.Windows.Forms.ContextMenuStrip m_MainFormContextMenuStripListTecViews;

        /// <summary>
        /// �������������� ����������� �������
        /// </summary>
        public static int [] ID_SPECIAL_TAB = { 10001, 10002, 10011, 10012 };
        /// <summary>
        /// ������������ - ��� ������
        /// </summary>
        public enum MODE_TECCOMPONENT : ushort { TEC, GTP, PC, TG, UNKNOWN };
        /// <summary>
        /// ��� �������  �� �������������� "�������������-���������"
        /// </summary>
        public enum MANAGER : ushort { DISP, NSS, ALARM, LK, COUNT_MANAGER, UNKNOWN };

        private HMark m_modeTECComponent;

        public HMark m_markTabAdminChecked;

        public FormChangeMode(List<TEC> tec, List<int> listIDsProfileCheckedIndices, System.Windows.Forms.ContextMenuStrip FormMainContextMenuStrip /*= null*//*, DelegateFunc changeMode*/)
        {
            InitializeComponent();
            this.Text = @"����� ������";

            if (listIDsProfileCheckedIndices.Count > 0)
            {
                if (!(m_MainFormContextMenuStripListTecViews == null))
                {
                    m_MainFormContextMenuStripListTecViews.ItemClicked -= new ToolStripItemClickedEventHandler(MainFormContextMenuStripListTecViews_ItemClicked);
                    m_MainFormContextMenuStripListTecViews = null;
                }
                else
                    ;
            }
            else
                ; //��� ID ��� ������������

            m_MainFormContextMenuStripListTecViews = FormMainContextMenuStrip;
            m_MainFormContextMenuStripListTecViews.ItemClicked += new ToolStripItemClickedEventHandler(MainFormContextMenuStripListTecViews_ItemClicked);

            this.m_list_tec = new List<TEC> ();
            foreach (TEC t in tec) {
                //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == t.m_id))
                    this.m_list_tec.Add (t);
                //else ;
            }

            m_modeTECComponent = new HMark (0);

            m_listCheckBoxTECComponent = new List <CheckBox> ()  { checkBoxTEC,
                                                                    checkBoxGTP,
                                                                    checkBoxPC,
                                                                    checkBoxTG };

            m_markTabAdminChecked = new HMark (0);
            m_listItems = new List<Item>();
            m_list_change_items = new List<Item>();

            if (! (m_list_tec == null))
            {
                bool bChecked = false;
                foreach (TEC t in m_list_tec)
                {
                    bChecked = false;
                    if (listIDsProfileCheckedIndices.IndexOf(t.m_id) > -1)
                        bChecked = true;
                    else
                        ;
                    m_listItems.Add(new Item(t.m_id, t.name_shr, bChecked));

                    if (t.list_TECComponents.Count > 0)
                    {
                        foreach (TECComponent g in t.list_TECComponents)
                        {
                            bChecked = false;
                            if (listIDsProfileCheckedIndices.IndexOf(g.m_id) > -1)
                                bChecked = true;
                            else
                                ;
                            m_listItems.Add(new Item(g.m_id, t.name_shr + " - " + g.name_shr, bChecked));                         
                        }
                    }
                    else
                        ;
                }

                bChecked = listIDsProfileCheckedIndices.IndexOf(ID_SPECIAL_TAB[(int)MANAGER.DISP]) > -1;
                m_listItems.Add(new Item(ID_SPECIAL_TAB[(int)MANAGER.DISP], getNameAdminValues(MANAGER.DISP, MODE_TECCOMPONENT.GTP), bChecked));
                //m_markTabAdminChecked.Set ((int)MANAGER.DISP, bChecked);

                bChecked = listIDsProfileCheckedIndices.IndexOf(ID_SPECIAL_TAB[(int)MANAGER.NSS]) > -1;
                m_listItems.Add(new Item(ID_SPECIAL_TAB[(int)MANAGER.NSS], getNameAdminValues(MANAGER.NSS, MODE_TECCOMPONENT.TG), bChecked));
                //m_markTabAdminChecked.Set((int)MANAGER.NSS, bChecked);

                bChecked = listIDsProfileCheckedIndices.IndexOf(ID_SPECIAL_TAB[(int)MANAGER.ALARM]) > -1;
                m_listItems.Add(new Item(ID_SPECIAL_TAB[(int)MANAGER.ALARM], getNameAdminValues(MANAGER.ALARM, MODE_TECCOMPONENT.GTP), bChecked));
                //m_markTabAdminChecked.Set((int)MANAGER.ALARM, bChecked);

                bChecked = listIDsProfileCheckedIndices.IndexOf(ID_SPECIAL_TAB[(int)MANAGER.LK]) > -1;
                m_listItems.Add(new Item(ID_SPECIAL_TAB[(int)MANAGER.LK], getNameAdminValues(MANAGER.LK, MODE_TECCOMPONENT.TG), bChecked));

            }
            else {
            }

            m_listCheckBoxTECComponent[(int)MODE_TECCOMPONENT.TEC].Checked = true;
            m_listCheckBoxTECComponent[(int)MODE_TECCOMPONENT.GTP].Checked = true;

            closing = false;
        }

        /// <summary>
        /// ���������� ����� (int), ��������� �������������
        /// ��� ������������ ������ �����������
        /// </summary>
        public int getModeTECComponent() {
            //int iMode = 0;

            //m_modeTECComponent.UnMarked ();

            //for (int i = (int)MODE_TECCOMPONENT.TEC; i < (int)MODE_TECCOMPONENT.UNKNOWN; i++)
            //{
            //    if (m_listCheckBoxTECComponent[i].Checked == true)
            //        m_modeTECComponent.Marked (i);
            //    else ;
            //}

            //return iMode;

            return m_modeTECComponent.Value;
        }

        public bool IsModeTECComponent (MODE_TECCOMPONENT mode) {
            //bool bRes = false;
            //int offset = 0;

            //if ((getModeTECComponent() & ((int)Math.Pow(2, (int)(mode) + offset))) == (int)Math.Pow(2, (int)(mode) + offset))
            //{
            //    bRes = true;
            //}
            //else
            //    ;

            //return bRes;

            return m_modeTECComponent.IsMarked ((int) mode);
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
            return HMark.IsMarked (checkMode, (int) mode);
        }

        public static string getPrefixMode(int indx)
        {
            String[] arPREFIX_COMPONENT = { "TEC", "GTP", "PC", "TG" };

            return arPREFIX_COMPONENT[indx];
        }
        /// <summary>
        /// ���������� ������������ ������ ����������� ��� �� �������
        /// </summary>
        /// <param name="indx">������ ������</param>
        /// <returns>������ - ������������ ������</returns>
        public static string getNameMode (Int16 indx) {
            string [] nameModes = {"���", "���", "��", "��������", "����������"};

            return nameModes[indx];
        }
        /// <summary>
        /// ���������� ������������ �������� ������ (����������� �������)
        /// </summary>
        /// <param name="modeManager">��� �������</param>
        /// <param name="modeComponent">����� ����������� ���</param>
        /// <returns>������ - ������� ��� �������� ������</returns>
        public string getNameAdminValues (MANAGER modeManager, MODE_TECCOMPONENT modeComponent) {
            string[] arNameAdminValues = { "���" /*TEC*/, "���������" /*GTP*/, "���" /*PC*/, "���" /*TG*/ };
            string prefix = ((modeManager == MANAGER.DISP) || (modeManager == MANAGER.NSS) || (modeManager == MANAGER.LK)) ? @"���" : (modeManager == MANAGER.ALARM) ? @"����." : @"����������";

            if(modeManager == MANAGER.LK)
                prefix += @" - ��";
            else
                prefix += @" - " + arNameAdminValues[(int)modeComponent];

            return prefix;
        }
        /// <summary>
        /// ���������� ��������� �������� ������ � ��������� �� �������������� �������������� ���������
        ///  ���������������� ���� ������ 
        /// </summary>
        /// <param name="item">�������������� �������������� �������� ������</param>
        private void itemSetStates(Item item)
        {
            //���
            if (itemSetState(item, 0, (int)TECComponent.ID.GTP, MODE_TECCOMPONENT.TEC) == false)
                //���
                if (itemSetState(item, (int)TECComponent.ID.GTP, (int)TECComponent.ID.PC, MODE_TECCOMPONENT.GTP) == false)
                    //��
                    if (itemSetState(item, (int)TECComponent.ID.PC, (int)TECComponent.ID.TG, MODE_TECCOMPONENT.PC) == false)
                        //��
                        if (itemSetState(item, (int)TECComponent.ID.TG, (int)TECComponent.ID.MAX, MODE_TECCOMPONENT.TG) == false)
                            //����������� �������...
                            itemSetState(item);
                        else
                            ;
                    else
                        ;
                else
                    ;
            else
                ;
        }
        /// <summary>
        /// ���������� ��������� �������� ������ �� ��������� ���������������
        /// </summary>
        /// <param name="item">�������������� �������������� �������� ������</param>
        /// <param name="idMinVal">����������� �������� �������������� ���������� ���, ������������ ��� ���������</param>
        /// <param name="idMaxVal">������������ �������� �������������� ���������� ���, ������������ ��� ���������</param>
        /// <param name="mode">��� ������</param>
        /// <returns>������� ���������� �������</returns>
        private bool itemSetState(Item item, int idMinVal = -1, int idMaxVal = -1, MODE_TECCOMPONENT mode = MODE_TECCOMPONENT.UNKNOWN)
        {
            bool bRes = false;

            if ((idMinVal == -1) || (idMaxVal == -1))
            {
                int idAllowed = -1;
                if (item.id == ID_SPECIAL_TAB[(int)MANAGER.DISP])
                    idAllowed = (int)HStatisticUsers.ID_ALLOWED.TAB_PBR_KOMDISP;
                else
                    if (item.id == ID_SPECIAL_TAB[(int)MANAGER.NSS])
                        idAllowed = (int)HStatisticUsers.ID_ALLOWED.TAB_PBR_NSS;
                    else
                        if (item.id == ID_SPECIAL_TAB[(int)MANAGER.ALARM])
                            idAllowed = (int)HStatisticUsers.ID_ALLOWED.ALARM_KOMDISP;
                        else
                            if (item.id == ID_SPECIAL_TAB[(int)MANAGER.LK])
                                idAllowed = (int)HStatisticUsers.ID_ALLOWED.TAB_LK_ADMIN;
                            else
                                ;


                bRes = !(idAllowed < 0);
                if (bRes == true)
                    if (HStatisticUsers.IsAllowed(idAllowed) == true)
                    {
                        clbMode.Items.Add(item.name_shr);

                        clbMode.SetItemChecked(clbMode.Items.Count - 1, item.bChecked);
                        item.bVisibled = true;
                    }
                    else
                        item.bVisibled = false;
                else
                    ;
            }
            else
            {
                bRes = (item.id > idMinVal) && (item.id < idMaxVal);
                if (bRes == true)
                    if (IsModeTECComponent(mode) == true)
                    {
                        clbMode.Items.Add(item.name_shr);
                        //����������� ���� - ������� �����
                        if (!(m_MainFormContextMenuStripListTecViews == null))
                        {
                            m_MainFormContextMenuStripListTecViews.Items.Add(item.name_shr);
                            if ((item.id >= (int)TECComponent.ID.GTP_LK & item.id < (int)TECComponent.ID.PC) || (item.id >= (int)TECComponent.ID.LK & item.id < (int)TECComponent.ID.GTP))
                            {
                            }
                            else
                            {
                                foreach (TEC t in m_list_tec)
                                {
                                    if(t.m_id==item.id)
                                        m_list_change_items.Add(item);
                                    else
                                        foreach (TECComponent tc in t.list_TECComponents)
                                        {
                                            if (tc.m_id == item.id)
                                                if (tc.tec.m_id >= (int)TECComponent.ID.LK & tc.tec.m_id < (int)TECComponent.ID.GTP)
                                                {
                                                }
                                                else
                                                {
                                                    if (tc.IsGTP == true)
                                                    {
                                                        m_list_change_items.Add(item);
                                                    }
                                                    if (tc.IsPC == true)
                                                    {
                                                        m_list_change_items.Add(item);
                                                    }
                                                }
                                        }
                                }
                            }
                        }
                        else ;
                        if (!(EventMenuItemAdd == null)) EventMenuItemAdd(item.id + @";" + item.name_shr);

                        clbMode.SetItemChecked(clbMode.Items.Count - 1, item.bChecked);
                        item.bVisibled = true;
                    }
                    else
                        item.bVisibled = false;
                else
                    ;
            }

            return bRes;
        }

        private void FillListBoxTab()
        {
            //����������� ���� - ������� �����
            if (!(m_MainFormContextMenuStripListTecViews == null))
            {
                m_MainFormContextMenuStripListTecViews.Items.Clear();
            }
            else ;
            //����������� ���� - ������� �����
            if (!(EventMenuItemsClear == null)) EventMenuItemsClear(); else ;
            
            if (!(m_listItems == null))
            {
                clbMode.Items.Clear();

                foreach (Item item in m_listItems)
                    itemSetStates(item);
            }
            else
                ;
        }

        private Item findItemOfId(int id)
        {
            Item itemRes = null;

            foreach (Item item in m_listItems)
            {
                if (item.id == id)
                {
                    itemRes = item;
                    break;
                }
                else
                    ;
            }

            return itemRes;
        }

        private Item findItemOfText(string text)
        {
            Item itemRes = null;

            foreach (Item item in m_listItems) {
                if (item.name_shr.Equals(text) == true)
                {
                    itemRes = item;
                    break;
                }
                else
                    ;
            }

            return itemRes;
        }

        private void btnOk_Click(object sender, EventArgs ev)
        {
            int i;
            Item item = null;

            for (i = 0; i < clbMode.Items.Count; i++) {
                item = findItemOfText(clbMode.GetItemText(clbMode.Items[i]));
                item.bChecked = ! (clbMode.CheckedIndices.IndexOf(i) < 0);

                if (item.id == ID_SPECIAL_TAB[(int)MANAGER.DISP])
                    m_markTabAdminChecked.Set((int)MANAGER.DISP, item.bChecked);
                else
                    if (item.id == ID_SPECIAL_TAB[(int)MANAGER.NSS])
                        m_markTabAdminChecked.Set((int)MANAGER.NSS, item.bChecked);
                    else
                        if (item.id == ID_SPECIAL_TAB[(int)MANAGER.ALARM])
                            m_markTabAdminChecked.Set((int)MANAGER.ALARM, item.bChecked);
                        else
                            if (item.id == ID_SPECIAL_TAB[(int)MANAGER.LK])
                                m_markTabAdminChecked.Set((int)MANAGER.LK, item.bChecked);
                            else
                                ;
            }

            try {
                //��������� ����������� ������ ������
                if (! (ev == EventArgs.Empty)) {
                    this.DialogResult = DialogResult.OK;
                    closing = true;
                    Close();
                }
                else
                    ev_������������();
            }
            catch (Exception e) {
                Logging.Logg().Exception(e, @"FormChangeMode::btnOk_Click () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        public void LoadProfile(string ids)
        {
            if (ids.Equals(string.Empty) == false)
            {
                string[] arId = ids.Split(';');
                Item item;
                foreach (string id in arId)
                {
                    item = findItemOfId(Int32.Parse(id));

                    if (item.bVisibled == true)
                        clbMode.SetItemChecked(clbMode.Items.IndexOf(item.name_shr), true);
                    else
                        item.bChecked = true;
                }
            }
            else
                ;

            //btnOk.PerformClick();
            btnOk_Click(null, EventArgs.Empty);
        }

        public string SaveProfile()
        {
            string ids = string.Empty;

            foreach (Item item in m_listItems)
                if ((item.bChecked == true) && (item.id < ID_SPECIAL_TAB [0]))
                    ids += item.id + @";";
                else
                    ;

            if (ids.Length > 0)
                ids = ids.Substring(0, ids.Length - 1);
            else
                ;

            return ids;
        }

        private void clbMode_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if ((!(m_MainFormContextMenuStripListTecViews == null)) && (e.Index < m_MainFormContextMenuStripListTecViews.Items.Count))
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

            //��� ������ � ��������������� �� ������������
            // ??? � ���� ������ ������� ������� ��������� ������
            // �� 2-� ������ �������� ��������� "�������������" �� ���������
            if (bChecked == clbMode.GetItemChecked(indx))
                bChecked = !bChecked;
            else
                ;

            clbMode.SetItemChecked(indx, bChecked);

            btnOk_Click(null, EventArgs.Empty);
        }

        public void SetItemChecked(int indxCheckedIndicies, bool bChecked)
        {
            int indx = -1;

            if ((clbMode.CheckedIndices.Count > 0) && (!(indxCheckedIndicies < 0)) && (indxCheckedIndicies < clbMode.CheckedIndices.Count)) {
                indx = clbMode.CheckedIndices[indxCheckedIndicies];
            }
            else {
                //indx = clbMode.CheckedIndices[clbMode.CheckedIndices.Count - 1];
                switch (indxCheckedIndicies)
                {
                    case -1: //KOM_DISP
                        indx = clbMode.Items.IndexOf(getNameAdminValues(MANAGER.DISP, MODE_TECCOMPONENT.GTP));
                        break;
                    case -2: //NSS
                        indx = clbMode.Items.IndexOf(getNameAdminValues(MANAGER.NSS, MODE_TECCOMPONENT.TEC)); //TG, PC - �� ����� ��������...
                        break;
                    case -3: //Alarm
                        indx = clbMode.Items.IndexOf(getNameAdminValues(MANAGER.ALARM, MODE_TECCOMPONENT.GTP)); //TG, PC - �� ����� ��������...
                        break;
                    case -4: //LK
                        indx = clbMode.Items.IndexOf(getNameAdminValues(MANAGER.LK, MODE_TECCOMPONENT.TEC)); //TG, PC - �� ����� ��������...
                        break;
                    default:
                        break;
                }
            }

            if ((!(indx < 0)) && (indx < clbMode.Items.Count))
            {
                clbMode.SetItemChecked(indx, bChecked);
                btnOk_Click(null, EventArgs.Empty);
            }
            else
                ;
        }

        public void SetItemChecked(string textItem, bool bChecked)
        {
            SetItemChecked (clbMode.CheckedItems.IndexOf (textItem), bChecked);
        }

        public int GetTECIndex(int id)
        {
            int indxRes = -1;

            foreach (TEC t in m_list_tec)
            {
                if (id > 100)
                {
                    foreach (TECComponent c in t.list_TECComponents)
                    {
                        if (id == c.m_id)
                        {
                            indxRes = m_list_tec.IndexOf(t);
                            break;
                        }
                        else
                            ;
                    }
                }
                else
                {
                    if (id == t.m_id)
                    {
                        indxRes = m_list_tec.IndexOf(t);
                        break;
                    }
                    else
                        ;
                }
            }

            return indxRes;
        }

        public int GetTECComponentIndex(int id, int TECIndex = -1)
        {
            int indxRes = -1;

            if (id > 100)
            {
                foreach (TEC t in m_list_tec)
                {
                    foreach (TECComponent c in t.list_TECComponents)
                    {
                        if (id == c.m_id)
                        {
                            indxRes = t.list_TECComponents.IndexOf(c);
                            break;
                        }
                        else
                            ;
                    }
                }
            }
            else
                ;

            return indxRes;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            btnOk.Focus();
            this.DialogResult = DialogResult.Cancel;
            closing = true;
            Close();
        }

        private void ChangeMode_HandleCreated(object sender, EventArgs e)
        {
        }

        private void ChangeMode_Load(object sender, EventArgs e)
        {
        }

        private void ChangeMode_Shown(object sender, EventArgs e)
        {
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

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            m_list_change_items.Clear();
            //m_modeTECComponent.Marked (m_listCheckBoxTECComponent.IndexOf ((CheckBox)sender));
            m_modeTECComponent.Set(m_listCheckBoxTECComponent.IndexOf((CheckBox)sender), ((CheckBox)sender).Checked);

            FillListBoxTab();

            CallEventChangeMode();
        }

        public void CallEventChangeMode()
        {
            if (EventChangeMode != null)
            {
                EventChangeMode(m_list_change_items);
            }
        }

        public string getIdsOfCheckedIndicies ()
        {
            string strRes = string.Empty;
            int i = -1;

            for (i = 0; i < m_listItems.Count; i ++)
            {
                if (m_listItems[i].bChecked == true)
                {
                    strRes += m_listItems[i].name_shr;
                    strRes += ",";
                }
                else
                    ;
            }

            if (strRes.Length > 1)
                strRes = strRes.Substring(0, strRes.Length - 1);
            else
                ;

            return strRes;
        }
    }
}