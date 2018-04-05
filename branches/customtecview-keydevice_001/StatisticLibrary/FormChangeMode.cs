using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ASUTP.Core;
using ASUTP;
using System.Linq;

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
        public class ListBoxItem
        {
            /// <summary>
            /// ������������� ���������� ���
            /// </summary>
            public KeyDevice key;
            /// <summary>
            /// ������� ������������ ���������� (��� �����������)
            /// </summary>
            public string name_shr;
            /// <summary>
            /// ������� ������ �� ����������� �������������� �������� �������
            ///  , ������� ����������� ������ �������� � ������ 
            /// </summary>
            public bool bChecked
                , bVisibled;

            public ListBoxItem(KeyDevice key, string name_shr, bool bChecked)
            {
                this.key = key;
                this.name_shr = name_shr;
                this.bChecked = bChecked;
                bVisibled = false;
            }
        }

        /// <summary>
        /// ������ ���, ���������� ������������� ��� ������� ����������
        ///  , ���������� � ������ ��� �������� ������� ������
        /// </summary>
        public List<TEC> m_list_tec;
        /// <summary>
        /// ������ ��������� ��� ������������� ��-���
        /// </summary>
        public List<ListBoxItem> m_listItems;
        /// <summary>
        /// ������ ���������� ���������
        /// </summary>
        private List<ListBoxItem> m_list_change_items;
        /// <summary>
        /// ������ ���������-������������� ��� ������ ���� ������
        ///  (������ ��� ����������� ����������� ���)
        /// </summary>
        private List <CheckBox> m_listCheckBoxTECComponent;
        /// <summary>
        /// ������� - ������������� ������������� ������ � ����������� ����������� ����
        /// </summary>
        public bool closing;
        /// <summary>
        /// ���������� ���� �������� ���� ����������
        /// </summary>
        public System.Windows.Forms.ContextMenuStrip m_MainFormContextMenuStripListTecViews;

        public enum ID_ADMIN_TAB { DISP = 10001, NSS, ALARM = 10011, LK, TEPLOSET = 10021 }
        /// <summary>
        /// ������ ��������������� ����������� �������, �������������� �������� ����������� ����������
        ///  , ����������� ��� �. ������ � ���� "����� ������". ������ ������������� ������������� 'MANAGER'.
        ///  ���������� �������� 'ID_MANAGER_TAB'
        /// </summary>
        public static int [] ID_ADMIN_TABS = { (int)ID_ADMIN_TAB.DISP, (int)ID_ADMIN_TAB.NSS, (int)ID_ADMIN_TAB.ALARM, (int)ID_ADMIN_TAB.LK, (int)ID_ADMIN_TAB.TEPLOSET };
        /// <summary>
        /// ������������ - ��� ������
        /// </summary>
        public enum MODE_TECCOMPONENT : short { Unknown = -3
            , ADMIN = -2
            , VYVOD = -1
            , TEC, GTP, PC, TG
                , ANY
        };

        [Serializable]
        public struct KeyDevice : IComparable<KeyDevice>
        {
            public int Id { get; set; }

            public FormChangeMode.MODE_TECCOMPONENT Mode { get; set; }

            public TECComponentBase.TYPE TECComponentType
            {
                get
                {
                    return TECComponent.GetType (Id);
                }
            }

            //public TECComponentBase.TYPE 

            public override string ToString ()
            {
                return string.Format ("KeyTECComponent=[Id={0}, Mode={1}]", Id, Mode);
            }

            public static bool operator == (KeyDevice key1, KeyDevice key2)
            {
                return (key1.Id == key2.Id)
                    && (key1.Mode == key2.Mode);
            }

            public static bool operator != (KeyDevice key1, KeyDevice key2)
            {
                return (!(key1.Id == key2.Id))
                    || (!(key1.Mode == key2.Mode));
            }

            public override bool Equals (object obj)
            {
                return (obj is KeyDevice) ? this == (KeyDevice)obj : false;
            }


            public override int GetHashCode ()
            {
                return base.GetHashCode ();
            }

            public int CompareTo (KeyDevice key)
            {
                return Id.CompareTo (key.Id);
            }

            public static KeyDevice Empty = new KeyDevice() { Id = -1, Mode = MODE_TECCOMPONENT.Unknown };

            public static KeyDevice Service = new KeyDevice () { Id = -666, Mode = MODE_TECCOMPONENT.ANY };
        }
        /// <summary>
        /// ��� �������  �� �������������� "�������������-���������"
        /// </summary>
        public enum MANAGER : short { UNKNOWN = -1, DISP, NSS, ALARM, LK, TEPLOSET, COUNT_MANAGER, };
        /// <summary>
        /// ������������ ������������� ��������� ��� ����������� �������
        /// </summary>
        private ASUTP.Core.HMark m_modeTECComponent;
        /// <summary>
        /// ������������ ��������� ��� ���������������� �������
        /// </summary>
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
            m_listItems = new List<ListBoxItem>();
            m_list_change_items = new List<ListBoxItem>();

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
                    m_listItems.Add(new ListBoxItem(new KeyDevice { Id = t.m_id, Mode = MODE_TECCOMPONENT.TEC }, t.name_shr, bChecked));

                    if (t.ListTECComponents.Count > 0)
                    {
                        foreach (TECComponent g in t.ListTECComponents)
                        {
                            bChecked = false;
                            if (listIDsProfileCheckedIndices.IndexOf(g.m_id) > -1)
                                bChecked = true;
                            else
                                ;
                            m_listItems.Add(new ListBoxItem(new KeyDevice { Id = g.m_id, Mode = TECComponent.GetMode (g.m_id) }, t.name_shr + " - " + g.name_shr, bChecked));
                        }
                    }
                    else
                        ;
                }

                bChecked = listIDsProfileCheckedIndices.IndexOf(ID_ADMIN_TABS[(int)MANAGER.DISP]) > -1;
                m_listItems.Add(new ListBoxItem(new KeyDevice { Id = ID_ADMIN_TABS [(int)MANAGER.DISP], Mode = MODE_TECCOMPONENT.ADMIN }, getNameAdminValues(MANAGER.DISP, MODE_TECCOMPONENT.GTP), bChecked));
                //m_markTabAdminChecked.Set ((int)MANAGER.DISP, bChecked);

                bChecked = listIDsProfileCheckedIndices.IndexOf(ID_ADMIN_TABS[(int)MANAGER.NSS]) > -1;
                m_listItems.Add(new ListBoxItem(new KeyDevice { Id = ID_ADMIN_TABS [(int)MANAGER.NSS], Mode = MODE_TECCOMPONENT.ADMIN }, getNameAdminValues(MANAGER.NSS, MODE_TECCOMPONENT.TEC), bChecked)); //TEC, TG, PC - �� ����� ��������...
                //m_markTabAdminChecked.Set((int)MANAGER.NSS, bChecked);

                bChecked = listIDsProfileCheckedIndices.IndexOf(ID_ADMIN_TABS[(int)MANAGER.ALARM]) > -1;
                m_listItems.Add(new ListBoxItem(new KeyDevice { Id = ID_ADMIN_TABS [(int)MANAGER.ALARM], Mode = MODE_TECCOMPONENT.ADMIN }, getNameAdminValues(MANAGER.ALARM, MODE_TECCOMPONENT.GTP), bChecked));
                //m_markTabAdminChecked.Set((int)MANAGER.ALARM, bChecked);

                bChecked = listIDsProfileCheckedIndices.IndexOf(ID_ADMIN_TABS[(int)MANAGER.LK]) > -1;
                m_listItems.Add(new ListBoxItem(new KeyDevice { Id = ID_ADMIN_TABS [(int)MANAGER.LK], Mode = MODE_TECCOMPONENT.ADMIN }, getNameAdminValues(MANAGER.LK, MODE_TECCOMPONENT.TEC), bChecked)); //TEC, TG, PC - �� ����� ��������...

                bChecked = listIDsProfileCheckedIndices.IndexOf(ID_ADMIN_TABS[(int)MANAGER.TEPLOSET]) > -1;
                m_listItems.Add(new ListBoxItem(new KeyDevice { Id = ID_ADMIN_TABS [(int)MANAGER.TEPLOSET], Mode = MODE_TECCOMPONENT.ADMIN }, getNameAdminValues(MANAGER.TEPLOSET, MODE_TECCOMPONENT.TEC), bChecked)); //TEC, TG, PC - �� ����� ��������...

            }
            else {
            }

            m_listCheckBoxTECComponent[(int)MODE_TECCOMPONENT.TEC].Checked = true;
            m_listCheckBoxTECComponent[(int)MODE_TECCOMPONENT.GTP].Checked = true;

            closing = false;
        }

        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }

            set
            {
                base.ForeColor =
                clbMode.ForeColor =
                    value;
            }
        }

        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }

            set
            {
                base.BackColor = value;

                clbMode.BackColor = value == SystemColors.Control ? SystemColors.Window : value;
            }
        }

        /// <summary>
        /// ���������� ����� (int), ��������� �������������
        /// ��� ������������ ������ �����������
        /// </summary>
        public int getModeTECComponent()
        {//TODO: �������� �� ��������, �������� ������������ � ���������� �����
            //int iMode = 0;

            //m_modeTECComponent.UnMarked ();

            //for (int i = (int)MODE_TECCOMPONENT.TEC; i < (int)MODE_TECCOMPONENT.ANY; i++)
            //{
            //    if (m_listCheckBoxTECComponent[i].Checked == true)
            //        m_modeTECComponent.Marked (i);
            //    else ;
            //}

            //return iMode;

            return m_modeTECComponent.Value;
        }

        /// <summary>
        /// ��������� ������� �������� ����������� ���� ��� ����� ���� ����������� ���
        /// </summary>
        /// <param name="mode">����� ����������� ����������� ���</param>
        /// <returns>������� ����������� ���� �����������</returns>
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
        /// <returns>��������� ��������</returns>
        public static bool IsModeTECComponent(int checkMode, MODE_TECCOMPONENT mode)
        {
            return HMark.IsMarked (checkMode, (int) mode);
        }

        public static string getPrefixMode(MODE_TECCOMPONENT mode)
        {
            //??? ��� ������������� mode, ������ 'Unknown'!!!
            return !(mode < 0) ? mode.ToString() : @"VYVOD";
        }
        /// <summary>
        /// ���������� ������������ ������ ����������� ��� �� �������
        /// </summary>
        /// <param name="mode">������ ������</param>
        /// <returns>������ - ������������ ������</returns>
        public static string getNameMode (MODE_TECCOMPONENT mode) {
            string [] nameModes = {"���", "���", "��", "��������", "����������"};

            return !(mode < 0)
                ? (int)mode < nameModes.Length
                    ? nameModes[(int)mode]
                        : nameModes [(int)mode - 1]
                            : @"������";
        }
        /// <summary>
        /// ���������� ������������ �������� ������ (����������� �������)
        /// </summary>
        /// <param name="modeManager">��� �������</param>
        /// <param name="modeComponent">����� ����������� ���</param>
        /// <returns>������ - ������� ��� �������� ������</returns>
        public string getNameAdminValues (MANAGER modeManager, MODE_TECCOMPONENT modeComponent) {
            string[] arNameAdminValues = { "���" /*TEC*/, "���������" /*GTP*/, "���" /*PC*/, "���" /*TG*/ };
            string prefix = new List<MANAGER>() { MANAGER.NSS, MANAGER.DISP, MANAGER.LK, MANAGER.TEPLOSET }.Contains (modeManager) == true ? HAdmin.PBR_PREFIX
                : (modeManager == MANAGER.ALARM) ? @"����."
                    : @"����������";

            switch (modeManager)
            {
                case MANAGER.LK:
                    prefix += @" - ��";
                    break;
                case MANAGER.TEPLOSET:
                    prefix += @" - ���������";
                    break;
                default:
                    prefix += @" - " + arNameAdminValues[(int)modeComponent];
                    break;
            }

            return prefix;
        }
        /// <summary>
        /// ���������� ��������� �������� ������ � ��������� �� �������������� �������������� ���������
        ///  ���������������� ���� ������ 
        /// </summary>
        /// <param name="item">�������������� �������������� �������� ������</param>
        private void itemSetStates(ListBoxItem item)
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
        private bool itemSetState(ListBoxItem item, int idMinVal = -1, int idMaxVal = -1, MODE_TECCOMPONENT mode = MODE_TECCOMPONENT.ANY)
        {
            bool bRes = false;

            int idAllowed = -1;
            //!!! ������������������ ������ �� �������� 'ID_SPECIAL_TAB'
            HStatisticUsers.ID_ALLOWED [] alloweds = {
                HStatisticUsers.ID_ALLOWED.TAB_PBR_KOMDISP
                , HStatisticUsers.ID_ALLOWED.TAB_PBR_NSS
                , HStatisticUsers.ID_ALLOWED.ALARM_KOMDISP
                , HStatisticUsers.ID_ALLOWED.TAB_LK_ADMIN
                , HStatisticUsers.ID_ALLOWED.TAB_TEPLOSET_ADMIN
                ,
            };

            if ((idMinVal == -1) || (idMaxVal == -1))
            {
                idAllowed = (int)alloweds [ID_ADMIN_TABS.ToList ().IndexOf (item.key.Id)];

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
                bRes = (item.key.Id > idMinVal) && (item.key.Id < idMaxVal);
                if (bRes == true)
                    if (IsModeTECComponent(mode) == true) {
                        if (TECComponent.VerifyID(item.key.Id
                            , TECComponent.ID.TEC, TECComponent.ID.LK, TECComponent.ID.GTP, TECComponent.ID.GTP_LK, TECComponent.ID.PC, TECComponent.ID.TG) == true) {
                            clbMode.Items.Add(item.name_shr);
                            //����������� ���� - ������� �����
                            if (!(m_MainFormContextMenuStripListTecViews == null)) {
                                m_MainFormContextMenuStripListTecViews.Items.Add(item.name_shr);

                                if (TECComponent.VerifyID(item.key.Id, TECComponent.ID.LK, TECComponent.ID.GTP_LK) == false)
                                    foreach (TEC t in m_list_tec) {
                                        if (t.m_id == item.key.Id)
                                            m_list_change_items.Add(item);
                                        else
                                            foreach (TECComponent tc in t.ListTECComponents)
                                                if (tc.m_id == item.key.Id)
                                                    if (tc.tec.m_id >= (int)TECComponent.ID.LK & tc.tec.m_id < (int)TECComponent.ID.GTP)
                                                        ;
                                                    else {
                                                        if (tc.IsGTP == true)
                                                            m_list_change_items.Add(item);
                                                        else
                                                            ;

                                                        if (tc.IsPC == true)
                                                            m_list_change_items.Add(item);
                                                        else
                                                            ;
                                                    }
                                    }
                                else
                                    ;
                            } else
                                ;

                            if (!(EventMenuItemAdd == null)) EventMenuItemAdd(item.key.Id + @";" + item.name_shr);

                            clbMode.SetItemChecked(clbMode.Items.Count - 1, item.bChecked);
                            item.bVisibled = true;
                        } else
                            ;
                    } else
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

                foreach (ListBoxItem item in m_listItems)
                    itemSetStates(item);
            }
            else
                ;
        }

        private ListBoxItem findItemOfId(int id)
        {
            ListBoxItem itemRes = null;

            foreach (ListBoxItem item in m_listItems)
            {
                if (item.key.Id == id)
                {
                    itemRes = item;
                    break;
                }
                else
                    ;
            }

            return itemRes;
        }

        private ListBoxItem findItemOfText(string text)
        {
            ListBoxItem itemRes = null;

            foreach (ListBoxItem item in m_listItems) {
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
            int i = -1
                , iManagerMode = -1;
            ListBoxItem item = null;

            for (i = 0; i < clbMode.Items.Count; i++) {
                item = findItemOfText(clbMode.GetItemText(clbMode.Items[i]));
                item.bChecked = ! (clbMode.CheckedIndices.IndexOf(i) < 0);

                iManagerMode = ID_ADMIN_TABS.ToList ().IndexOf (item.key.Id);

                if (!(iManagerMode < 0))
                    m_markTabAdminChecked.Set(iManagerMode, item.bChecked);
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
                ListBoxItem item;
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

            foreach (ListBoxItem item in m_listItems)
                if ((item.bChecked == true) && (item.key.Id < ID_ADMIN_TABS [0]))
                    ids += item.key.Id + @";";
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

            if ((clbMode.CheckedIndices.Count > 0)
                && (!(indxCheckedIndicies < 0))
                && (indxCheckedIndicies < clbMode.CheckedIndices.Count)) {
                indx = clbMode.CheckedIndices[indxCheckedIndicies];
            }
            else {
                //indx = clbMode.CheckedIndices[clbMode.CheckedIndices.Count - 1];
                switch (indxCheckedIndicies)
                {
                    case -1: //KOM_DISP
                        indx = clbMode.Items.IndexOf(getNameAdminValues(MANAGER.DISP, MODE_TECCOMPONENT.GTP)); //������ ���
                        break;
                    case -2: //NSS
                        indx = clbMode.Items.IndexOf(getNameAdminValues(MANAGER.NSS, MODE_TECCOMPONENT.TEC)); //TEC, TG, PC - �� ����� ��������...
                        break;
                    case -3: //Alarm
                        indx = clbMode.Items.IndexOf(getNameAdminValues(MANAGER.ALARM, MODE_TECCOMPONENT.GTP)); //������ ���
                        break;
                    case -4: //LK
                        indx = clbMode.Items.IndexOf(getNameAdminValues(MANAGER.LK, MODE_TECCOMPONENT.TEC)); //TEC, TG, PC - �� ����� ��������...
                        break;
                    case -5: //VYVOD(TEPLOSET)
                        indx = clbMode.Items.IndexOf(getNameAdminValues(MANAGER.TEPLOSET, MODE_TECCOMPONENT.TEC)); //TEC, TG, PC - �� ����� ��������...
                        break;
                    default:
                        break;
                }
            }

            if ((!(indx < 0))
                && (indx < clbMode.Items.Count))
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

        //public int GetTECIndex(int id)
        //{
        //    int indxRes = -1;

        //    foreach (TEC t in m_list_tec)
        //    {
        //        if (id > (int)TECComponent.ID.GTP)
        //        {
        //            foreach (TECComponent c in t.list_TECComponents)
        //            {
        //                if (id == c.m_id)
        //                {
        //                    indxRes = m_list_tec.IndexOf(t);
        //                    break;
        //                }
        //                else
        //                    ;
        //            }
        //        }
        //        else
        //        {
        //            if (id == t.m_id)
        //            {
        //                indxRes = m_list_tec.IndexOf(t);
        //                break;
        //            }
        //            else
        //                ;
        //        }
        //    }

        //    return indxRes;
        //}

        //public int GetTECComponentIndex(int id, int TECIndex = -1)
        //{
        //    int indxRes = -1;

        //    if (id > 100)
        //    {
        //        foreach (TEC t in m_list_tec)
        //        {
        //            foreach (TECComponent c in t.list_TECComponents)
        //            {
        //                if (id == c.m_id)
        //                {
        //                    indxRes = t.list_TECComponents.IndexOf(c);
        //                    break;
        //                }
        //                else
        //                    ;
        //            }
        //        }
        //    }
        //    else
        //        ;

        //    return indxRes;
        //}

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

            PerformChangeMode();
        }

        public void PerformChangeMode()
        {
            EventChangeMode?.Invoke(m_list_change_items);
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