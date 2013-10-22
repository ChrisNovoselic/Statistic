using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Statistic
{
    public partial class FormTECComponent : Form
    {
        private enum INDEX_UICONTROL : ushort { DATAGRIDVIEW_TEC, DATAGRIDVIEW_TEC_COMPONENT, DATAGRIDVIEW_TG,
                                                TEXTBOX_TEC_ADD, BUTTON_TEC_ADD,
                                                TEXTBOX_TECCOMPONENT_ADD, BUTTON_TECCOMPONENT_ADD,
                                                COMBOBOX_TG_ADD, BUTTON_TG_ADD };

        private ConnectionSettings m_connectionSetttings;

        private Object m_lockObj;

        private Thread m_threadUIControl;
        private Semaphore m_semUIControl;
        private volatile bool m_bThreadUIControlIsWorking;

        private System.Windows.Forms.Button m_btnDelete;

        private DataTable m_list_tec_original = null;
        private DataTable [,] m_list_TECComponents_original = null;
        private DataTable m_list_tg_original = null;
        private DataTable[] m_list_data = new DataTable[INDEX_UICONTROL.DATAGRIDVIEW_TG - INDEX_UICONTROL.DATAGRIDVIEW_TEC + 1];
        //Текущие 
        private List<System.Windows.Forms.Control> m_list_UIControl = null;

        List<DataRow> m_listDataRowComboBoxAddTG = new List<DataRow>();

        private enum ID_ACTION : short { UPDATE, ADD, DELETE };
        private class Action
        {
            public short mode;
            private short[] ids;
            public List <ID_ACTION> list_act;
            public DataRow val;

            public Action () {
                ids = new short[INDEX_UICONTROL.DATAGRIDVIEW_TG - INDEX_UICONTROL.DATAGRIDVIEW_TEC + 1];
                list_act = new List <ID_ACTION> ();
            }

            public void Ids (INDEX_UICONTROL indx, short val) {
                ids[(int)(indx - INDEX_UICONTROL.DATAGRIDVIEW_TEC)] = val;
            }

            public short Ids(INDEX_UICONTROL indx)
            {
                return ids[(int)(indx - INDEX_UICONTROL.DATAGRIDVIEW_TEC)];
            }
        };
        
        private List <Action> m_list_action;

        public FormTECComponent(ConnectionSettings connSett)
        {
            m_connectionSetttings = connSett;

            InitializeComponent();

            m_list_UIControl = new List <System.Windows.Forms.Control> ();
            m_list_UIControl.Add(dataGridViewTEC); //INDEX_UICONTROL.TEC
            m_list_UIControl.Add(dataGridViewTECComponent); //INDEX_UICONTROL.TEC_COMPONENT
            m_list_UIControl.Add(dataGridViewTG); //INDEX_UICONTROL.TG

            this.ColumnTextBoxTECName.Width = m_list_UIControl[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC].Width - (2 * (23 + 1));
            //Пока добавлять/удалять ТЭЦ нельзя
            this.ColumnTextBoxTECName.Width = m_list_UIControl[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC].Width - (1 * (23 + 1) + 2);
            this.ColumnButtonTECDel.Visible = false;

            this.ColumnTECComponentName.Width = m_list_UIControl[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Width - (1 * (23 + 1) + 2);
            this.ColumnTGName.Width = m_list_UIControl[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Width - (1 * (23 + 1) + 2);

            m_list_UIControl.Add (textBoxTECAdd);
            m_list_UIControl.Add(buttonTECAdd);

            m_list_UIControl.Add(textBoxTECComponentAdd);
            m_list_UIControl.Add(buttonTECComponentAdd);

            m_list_UIControl.Add(comboBoxTGAdd);
            m_list_UIControl.Add(buttonTGAdd);

            m_lockObj = new Object();

            m_list_action = new List<Action>();

            m_btnDelete = new System.Windows.Forms.Button();
            m_btnDelete.Text = "Удалить";

            lock (m_lockObj)
            {
            }

            //Пока добавлять/удалять ТЭЦ нельзя
            m_list_UIControl [(int)INDEX_UICONTROL.TEXTBOX_TEC_ADD].Enabled = false;
            m_list_UIControl[(int)INDEX_UICONTROL.BUTTON_TEC_ADD].Enabled = false;

            fillDataGridView(INDEX_UICONTROL.DATAGRIDVIEW_TEC);

            m_list_TECComponents_original = new DataTable[(int)ChangeMode.MODE_TECCOMPONENT.UNKNOWN - 1, m_list_tec_original.Rows.Count];

            for (int i = (int)ChangeMode.MODE_TECCOMPONENT.TEC; i < (int)ChangeMode.MODE_TECCOMPONENT.UNKNOWN; i++)
            {
                comboBoxMode.Items.Add(ChangeMode.getNameMode((short)i));
            }
            comboBoxMode.SelectedIndex = 0;

            //StartThreadUIControl();
            timerUIControl.Start();
        }

        public void StartThreadUIControl()
        {
            m_bThreadUIControlIsWorking = true;

            m_threadUIControl = new Thread(new ParameterizedThreadStart(FunctionThreadUIControl));
            m_threadUIControl.Name = "Контроль интерфейса пользователя";
            m_threadUIControl.IsBackground = true;

            m_semUIControl = new Semaphore(1, 1);

            m_semUIControl.WaitOne();
            m_threadUIControl.Start();
        }

        public void StopThreadUIControl()
        {
            bool joined;
            m_bThreadUIControlIsWorking = false;
            lock (m_lockObj)
            {
            }

            if (m_threadUIControl.IsAlive)
            {
                try
                {
                    m_semUIControl.Release(1);
                }
                catch
                {
                }

                joined = m_threadUIControl.Join(1000);
                if (!joined)
                    m_threadUIControl.Abort();
                else
                    ;
            }
            else
                ;
        }

        private void EnabledUIControl()
        {
            lock (m_lockObj)
            {
                switch (comboBoxMode.SelectedIndex)
                {
                    case (int)ChangeMode.MODE_TECCOMPONENT.TEC:
                        //Только в режиме ТЭЦ
                        if (m_list_UIControl [(int)INDEX_UICONTROL.TEXTBOX_TECCOMPONENT_ADD].Enabled == true) m_list_UIControl [(int)INDEX_UICONTROL.TEXTBOX_TECCOMPONENT_ADD].Enabled = false; else ;
                        if (! (m_list_UIControl[(int)INDEX_UICONTROL.BUTTON_TECCOMPONENT_ADD].Enabled == m_list_UIControl[(int)INDEX_UICONTROL.TEXTBOX_TECCOMPONENT_ADD].Enabled))
                            m_list_UIControl[(int)INDEX_UICONTROL.BUTTON_TECCOMPONENT_ADD].Enabled = m_list_UIControl[(int)INDEX_UICONTROL.TEXTBOX_TECCOMPONENT_ADD].Enabled;
                        else
                            ;
                        break;
                    case (int)ChangeMode.MODE_TECCOMPONENT.GTP:
                    case (int)ChangeMode.MODE_TECCOMPONENT.PC:
                        //Только в режимах ГТП, ЩУ
                        if (m_list_UIControl [(int)INDEX_UICONTROL.TEXTBOX_TECCOMPONENT_ADD].Enabled == false) m_list_UIControl [(int)INDEX_UICONTROL.TEXTBOX_TECCOMPONENT_ADD].Enabled = true; else ;
                        if ((m_list_UIControl[(int)INDEX_UICONTROL.TEXTBOX_TECCOMPONENT_ADD].Enabled == true) &&
                            (m_list_UIControl [(int)INDEX_UICONTROL.TEXTBOX_TECCOMPONENT_ADD].Text.Length > 0))
                            if (! (m_list_UIControl[(int)INDEX_UICONTROL.BUTTON_TECCOMPONENT_ADD].Enabled == m_list_UIControl[(int)INDEX_UICONTROL.TEXTBOX_TECCOMPONENT_ADD].Enabled))
                                m_list_UIControl[(int)INDEX_UICONTROL.BUTTON_TECCOMPONENT_ADD].Enabled = m_list_UIControl[(int)INDEX_UICONTROL.TEXTBOX_TECCOMPONENT_ADD].Enabled;
                            else
                                ;
                        else
                            m_list_UIControl[(int)INDEX_UICONTROL.BUTTON_TECCOMPONENT_ADD].Enabled = false;
                        break;
                    default:
                        break;
                }

                if (((ComboBox)m_list_UIControl[(int)INDEX_UICONTROL.COMBOBOX_TG_ADD]).Items.Count > 0)
                    if (m_list_UIControl[(int)INDEX_UICONTROL.COMBOBOX_TG_ADD].Enabled == false) m_list_UIControl[(int)INDEX_UICONTROL.COMBOBOX_TG_ADD].Enabled = true; else ;
                else
                    if (m_list_UIControl[(int)INDEX_UICONTROL.COMBOBOX_TG_ADD].Enabled == true) m_list_UIControl[(int)INDEX_UICONTROL.COMBOBOX_TG_ADD].Enabled = false; else ;

                m_list_UIControl[(int)INDEX_UICONTROL.BUTTON_TG_ADD].Enabled = comboBoxTGAdd.Enabled;
            }
        }

        private void FunctionThreadUIControl(object data)
        {
            while (m_bThreadUIControlIsWorking)
            {
                //m_semUIControl.WaitOne();

                EnabledUIControl();

                //while (true) { }
            }

            try { m_semUIControl.Release(1); }
            catch { }
        }

        private Int32 getIdSelectedTEC () {
            return Convert.ToInt32(m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC].Rows[dataGridViewTEC.SelectedRows[0].Index]["ID"]);
        }

        private Int32 getIdTEC(int indx)
        {
            return Convert.ToInt32(m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC].Rows[indx]["ID"]);
        }

        private int getIndexTEC (int id) {
            int iResIndx = -1;

            for (int i = 0; (i < m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC].Rows.Count) && (iResIndx < 0); i++)
            {
                if (Convert.ToInt32(m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC].Rows[i]["ID"]) == id)
                    iResIndx = i;
                else
                    ;
            }

            return iResIndx;
        }

        private Int32 getIdSelectedTECComponent()
        {
            int indx_mode = comboBoxMode.SelectedIndex - 1,
                id_tec = getIdSelectedTEC(), indx_tec = getIndexTEC(id_tec),
                indx_teccomponent = -1,
                iRes = -1;

            if (((DataGridView)m_list_UIControl[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT]).Rows.Count > 0) {
                indx_teccomponent = ((DataGridView)m_list_UIControl[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT]).SelectedRows[0].Index;
                
                if (indx_mode > -1)
                    if (indx_teccomponent < m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows.Count)
                        iRes = Convert.ToInt32(m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows[indx_teccomponent]["ID"]);
                    else
                        ;
                else
                    ;
            }
            else
                ;

            return iRes;
        }

        private Int32 getIdTECComponent(int indx)
        {
            int indx_mode = comboBoxMode.SelectedIndex - 1,
                indx_tec = getIndexTEC(getIdSelectedTEC());

            if (indx_mode > -1)
                return Convert.ToInt32(m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows[indx]["ID"]);
            else
                return indx_mode;
        }

        private int getIndexTECComponent(int id)
        {
            int iRes = -1,
                indx_mode = comboBoxMode.SelectedIndex - 1,
                indx_tec = getIndexTEC(getIdSelectedTEC());

            if (indx_mode > -1)
                for (int i = 0; (i < m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows.Count) && (iRes < 0); i++)
                {
                    if (Convert.ToInt32(m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows[i]["ID"]) == id)
                        iRes = i;
                    else
                        ;
                }
            else
                ;

            return iRes;
        }

        private Int32 getIdSelectedTG()
        {
            int indx_tec = getIndexTEC(getIdSelectedTEC());

            return Convert.ToInt32(m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows[((DataGridView)m_list_UIControl[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG]).SelectedRows[0].Index]["ID"]);
        }

        private Int32 getIdTG(int indx)
        {
            int indx_tec = getIndexTEC(getIdSelectedTEC());

            return Convert.ToInt32(m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows[indx]["ID"]);
        }

        private int getIndexTG(int id)
        {
            int iResIndx = -1,
                indx_tec = getIndexTEC(getIdSelectedTEC());

            for (int i = 0; (i < m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows.Count) && (iResIndx < 0); i++)
            {
                if (Convert.ToInt32(m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows[i]["ID"]) == id)
                    iResIndx = i;
                else
                    ;
            }

            return iResIndx;
        }

        private void getListTEC()
        {
            if (m_list_tec_original == null)
                m_list_tec_original = InitTEC.getListTEC(m_connectionSetttings, true);
            else ;

            m_list_data [(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC] = m_list_tec_original.Copy ();
        }

        private void getListTECComponent(int id_tec)
        {
            int indx_tec = getIndexTEC(id_tec);

            if (m_list_TECComponents_original[comboBoxMode.SelectedIndex - 1, indx_tec] == null)
                m_list_TECComponents_original[comboBoxMode.SelectedIndex - 1, indx_tec] = InitTEC.getListTECComponent(m_connectionSetttings, ChangeMode.getPrefixMode(comboBoxMode.SelectedIndex), id_tec);
            else ;

            m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT] = m_list_TECComponents_original[comboBoxMode.SelectedIndex - 1, indx_tec].Copy();

            for (int i = 0; (! (m_list_action == null)) && (i < m_list_action.Count); i++)
            {
                if ((m_list_action[i].Ids (INDEX_UICONTROL.DATAGRIDVIEW_TEC) == (short)getIdSelectedTEC())
                    //&& (m_list_action[i].mode == comboBoxMode.SelectedIndex)
                    // && (m_list_action[i].Ids (INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT) == getIdSelectedTECComponent())
                    && (! (m_list_action[i].Ids (INDEX_UICONTROL.DATAGRIDVIEW_TG) > -1))
                    && (m_list_action[i].Ids (INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT) > -1)
                    )
                {
                    int j = -1;
                    switch (m_list_action[i].list_act[m_list_action[i].list_act.Count - 1])
                    {
                        case ID_ACTION.DELETE:
                            bool bDelete = false;

                            for (j = 0; (j < m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows.Count) && (bDelete == false); j++)
                            {
                                if (Convert.ToInt32(m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows[j]["ID"]) == m_list_action[i].Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT))
                                {
                                    switch (m_list_action[i].mode)
                                    {
                                        case (int)ChangeMode.MODE_TECCOMPONENT.TEC:
                                            bDelete = true;
                                            break;
                                        case (int)ChangeMode.MODE_TECCOMPONENT.GTP:
                                        case (int)ChangeMode.MODE_TECCOMPONENT.PC:
                                            if (m_list_action[i].Ids (INDEX_UICONTROL.DATAGRIDVIEW_TEC) == getIdSelectedTEC())
                                                bDelete = true;
                                            else
                                                ;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else
                                    ;

                                if (bDelete == true)
                                {
                                    m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows.RemoveAt(j);

                                    break;
                                }
                                else
                                    ;
                            }
                            break;
                        case ID_ACTION.ADD:
                            if (m_list_action[i].mode == comboBoxMode.SelectedIndex)
                            {
                                m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows.Add();

                                int indx = m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows.Count - 1;
                                m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows[indx]["ID"] = m_list_action[i].Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT);
                                m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows[indx]["ID_TEC"] = m_list_action[i].Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC);

                                m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows[indx]["NAME_SHR"] =
                                //m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows[indx]["NAME_FUTURE"] =
                                m_list_action[i].val ["NAME_SHR"];
                            }
                            else
                                ;
                            break;
                        case ID_ACTION.UPDATE:
                            for (j = 0; j < m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows.Count; j++)
                            {
                                if (Convert.ToInt32(m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows[j]["ID"]) == m_list_action[i].Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT))
                                {
                                    m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows[j]["NAME_SHR"] = m_list_action[i].val;
                                    break;
                                }
                                else
                                    ;
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                    ;
            }
        }

        private void getListTG(ChangeMode.MODE_TECCOMPONENT mode = ChangeMode.MODE_TECCOMPONENT.UNKNOWN, int id = -1)
        {
            if (mode == ChangeMode.MODE_TECCOMPONENT.UNKNOWN) mode = (ChangeMode.MODE_TECCOMPONENT)comboBoxMode.SelectedIndex; else ;
            
            if (id < 0) {
                switch (comboBoxMode.SelectedIndex)
                {
                    case (int)ChangeMode.MODE_TECCOMPONENT.TEC:
                        id = getIdSelectedTEC();
                        break;
                    case (int)ChangeMode.MODE_TECCOMPONENT.GTP:
                    case (int)ChangeMode.MODE_TECCOMPONENT.PC:
                        id = getIdSelectedTECComponent();
                        break;
                    default:
                        break;
                }
            }
            else
                ;

            m_list_tg_original = InitTEC.getListTG(m_connectionSetttings, ChangeMode.getPrefixMode((int)mode), id);

            m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG] = m_list_tg_original.Copy();

            for (int i = 0; (! (m_list_action == null)) && (i < m_list_action.Count); i++)
            {
                if ((m_list_action[i].Ids (INDEX_UICONTROL.DATAGRIDVIEW_TEC) == getIdSelectedTEC())
                    //&& (m_list_action[i].mode == comboBoxMode.SelectedIndex)
                    // && (m_list_action[i].Ids (INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT) == getIdSelectedTECComponent())
                    && (m_list_action[i].Ids (INDEX_UICONTROL.DATAGRIDVIEW_TG) > -1)
                    )
                {
                    int j = -1;
                    switch (m_list_action[i].list_act[m_list_action[i].list_act.Count - 1])
                    {
                        case ID_ACTION.DELETE:
                            bool bDelete = false;

                            for (j = 0; (j < m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows.Count) && (bDelete == false); j++)
                            {
                                if (Convert.ToInt32(m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows[j]["ID"]) == m_list_action[i].Ids(INDEX_UICONTROL.DATAGRIDVIEW_TG))
                                {
                                    switch (m_list_action[i].mode)
                                    {
                                        case (int)ChangeMode.MODE_TECCOMPONENT.TEC:
                                            bDelete = true;
                                            break;
                                        case (int)ChangeMode.MODE_TECCOMPONENT.GTP:
                                        case (int)ChangeMode.MODE_TECCOMPONENT.PC:
                                            if (m_list_action[i].Ids (INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT) == getIdSelectedTECComponent())
                                                bDelete = true;
                                            else
                                                ;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else
                                    ;

                                if (bDelete == true)
                                {
                                    m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows.RemoveAt(j);

                                    break;
                                }
                                else
                                    ;
                            }
                            break;
                        case ID_ACTION.ADD:
                            if (m_list_action[i].Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT) == getIdSelectedTECComponent())
                            {
                                m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows.Add ();
                                //m_list_action[i].val.ItemArray.(m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows[m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows.Count -1].ItemArray, 0);
                                //for (j = 0; j < m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows[m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows.Count -1].ItemArray.Length; j++)
                                for (j = 0; j < m_list_action[i].val.ItemArray.Length; j++)
                                {
                                    m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows[m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows.Count -1][j] = m_list_action[i].val[j];
                                }
                                switch (comboBoxMode.SelectedIndex/*m_list_action[i].mode*/) {
                                    case (int)ChangeMode.MODE_TECCOMPONENT.TEC:
                                        break;
                                    case (int)ChangeMode.MODE_TECCOMPONENT.GTP:
                                        m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows[m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows.Count - 1]["ID_" + ChangeMode.getPrefixMode(comboBoxMode.SelectedIndex)] = getIdSelectedTECComponent();
                                        break;
                                    case (int)ChangeMode.MODE_TECCOMPONENT.PC:
                                        m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows[m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows.Count - 1]["ID_" + ChangeMode.getPrefixMode(comboBoxMode.SelectedIndex)] = getIdSelectedTECComponent();
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                                ;
                            break;
                        case ID_ACTION.UPDATE:
                            for (j = 0; j < m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows.Count; j++)
                            {
                                if (Convert.ToInt32(m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows[j]["ID"]) == m_list_action[i].Ids(INDEX_UICONTROL.DATAGRIDVIEW_TG))
                                {
                                    m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows[j]["NAME_SHR"] = m_list_action[i].val;
                                    break;
                                }
                                else
                                    ;
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                    ;
            }
        }

        Int32 getIdNext(ChangeMode.MODE_TECCOMPONENT indx)
        {
            Int32 idRes = -1;

            idRes = Convert.ToInt32 (DbInterface.Request(m_connectionSetttings, "SELECT MAX(ID) FROM " + ChangeMode.getPrefixMode((int)indx) + "_LIST").Rows [0][0]);

            return ++idRes;
        }

        private void comboBoxMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (((ComboBox)sender).SelectedIndex) {
                default:
                    dataGridViewTEC_CellClick(null, null);
                    break;
            }
        }

        private /*static*/ void fillDataGridView(INDEX_UICONTROL indx_datagridview)
        {
            DataGridView dataGridView = ((DataGridView)m_list_UIControl [(int)indx_datagridview]);
            DataTable data = null;

            switch (indx_datagridview)
            {
                case INDEX_UICONTROL.DATAGRIDVIEW_TEC:
                    getListTEC();
                    break;
                case INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT:
                    getListTECComponent(getIdSelectedTEC());
                    break;
                case INDEX_UICONTROL.DATAGRIDVIEW_TG:
                    getListTG();
                    break;
                default:
                    break;
            }

            data = m_list_data[(int)indx_datagridview];

            dataGridView.Rows.Clear();

            System.Collections.IEnumerator enumColumns = null;
            object col = null;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                dataGridView.Rows.Add();

                enumColumns = dataGridView.Columns.GetEnumerator();

                while (enumColumns.MoveNext())
                {
                    col = enumColumns.Current;
                    switch (col.GetType ().Name)
                    {
                        case "DataGridViewCheckBoxColumn":
                            dataGridView.Rows[i].Cells[((DataGridViewCheckBoxColumn)col).Index].Value = data.Rows[i]["InUse"];
                            break;
                        case "DataGridViewTextBoxColumn":
                            dataGridView.Rows[i].Cells[((DataGridViewTextBoxColumn)col).Index].Value = data.Rows[i]["NAME_SHR"].ToString();
                            break;
                        case "DataGridViewButtonColumn":
                            dataGridView.Rows[i].Cells[((DataGridViewButtonColumn)col).Index].Value = "-"; //global::Statistic.Properties.Resources.btnDel
                            break;
                        default:
                            break;
                    }
                }
            }

            if (dataGridView.Rows.Count > 0)
            {
                dataGridView.Rows[0].Selected = true;

                bool bReadOnly = false;
                if (dataGridView.Rows.Count == 1) bReadOnly = true; else ;

                enumColumns = dataGridView.Columns.GetEnumerator();
                while (enumColumns.MoveNext())
                {
                    col = enumColumns.Current;
                    switch (col.GetType().Name)
                    {
                        case "DataGridViewButtonColumn":
                            if (!(((DataGridViewButtonColumn)col).ReadOnly == bReadOnly)) ((DataGridViewButtonColumn)col).ReadOnly = bReadOnly; else ;
                            break;
                        default:
                            break;
                    }
                }
            }
            else
                ;
        }

        private void fillComboBoxTGAdd()
        {
            int i = -1, j = -1, k = -1;
            DataTable data = InitTEC.getListTG(m_connectionSetttings, ChangeMode.getPrefixMode((int)ChangeMode.MODE_TECCOMPONENT.TEC), getIdSelectedTEC());
            //List <DataRow>listDataRowComboBoxAddTG = new List <DataRow> ();
            m_listDataRowComboBoxAddTG.Clear ();

            if (comboBoxTGAdd.Enabled == true) comboBoxTGAdd.Items.Clear(); else ;

            for (i = 0; i < m_list_action.Count; i ++) {
                for (j = 0; j < data.Rows.Count; j ++) {
                    if (m_list_action[i].Ids (INDEX_UICONTROL.DATAGRIDVIEW_TG) == Convert.ToInt32 (data.Rows[j]["ID"])) {
                        switch (m_list_action[i].list_act[m_list_action[i].list_act.Count - 1])
                        {
                            case ID_ACTION.DELETE:
                                switch (m_list_action[i].mode)
                                {
                                    case (int)ChangeMode.MODE_TECCOMPONENT.TEC:
                                        for (k = 0; k < m_listDataRowComboBoxAddTG.Count; k ++) {
                                            if (Convert.ToInt32 (m_listDataRowComboBoxAddTG[k]["ID"]) == m_list_action[i].Ids (INDEX_UICONTROL.DATAGRIDVIEW_TG))
                                                m_listDataRowComboBoxAddTG.RemoveAt (k);
                                            else
                                                ;
                                        }
                                        break;
                                    case (int)ChangeMode.MODE_TECCOMPONENT.GTP:
                                    case (int)ChangeMode.MODE_TECCOMPONENT.PC:
                                        if (m_list_action[i].mode == comboBoxMode.SelectedIndex) {
                                            m_listDataRowComboBoxAddTG.Add(data.Rows[j]);
                                        }
                                        else
                                            ;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case ID_ACTION.ADD:
                                switch (m_list_action[i].mode)
                                {
                                    case (int)ChangeMode.MODE_TECCOMPONENT.TEC:
                                        break;
                                    case (int)ChangeMode.MODE_TECCOMPONENT.GTP:
                                    case (int)ChangeMode.MODE_TECCOMPONENT.PC:
                                        for (k = 0; k < m_listDataRowComboBoxAddTG.Count; k ++) {
                                            if (Convert.ToInt32 (m_listDataRowComboBoxAddTG[k]["ID"]) == m_list_action[i].Ids (INDEX_UICONTROL.DATAGRIDVIEW_TG)
                                                && (m_list_action[i].mode == comboBoxMode.SelectedIndex))
                                                m_listDataRowComboBoxAddTG.RemoveAt (k);
                                            else
                                                ;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else
                        ;
                }
            }

            if (m_listDataRowComboBoxAddTG.Count > 0) {
                for (k = 0; k < m_listDataRowComboBoxAddTG.Count; k++)
                {
                    comboBoxTGAdd.Items.Add(m_listDataRowComboBoxAddTG [k]["NAME_SHR"]);
                }

                comboBoxTGAdd.SelectedIndex = 0;
            }
            else
                ;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            buttonClick(DialogResult.Yes);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            buttonClick(DialogResult.No);
        }

        private void buttonClick(DialogResult res)
        {
            //StopThreadUIControl();
            timerUIControl.Stop();

            this.DialogResult = res;
            Close();
        }

        private void timerUIControl_Tick(object sender, EventArgs e)
        {
            EnabledUIControl();
        }

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ((DataGridView)sender).Rows[e.RowIndex].Selected = true;
        }

        private void dataGridViewTEC_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((!(sender == null)) && (!(e == null))) dataGridView_CellClick(sender, e); else ;

            switch (comboBoxMode.SelectedIndex)
            {
                case (int)ChangeMode.MODE_TECCOMPONENT.TEC:
                    //Только в режиме ТЭЦ
                    ((DataGridView)m_list_UIControl[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT]).Rows.Clear();
                    m_list_UIControl[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Enabled = false;

                    fillDataGridView(INDEX_UICONTROL.DATAGRIDVIEW_TG);
                    fillComboBoxTGAdd();
                    break;
                case (int)ChangeMode.MODE_TECCOMPONENT.GTP:
                case (int)ChangeMode.MODE_TECCOMPONENT.PC:
                    //Только в режимах ГТП, ЩУ
                    m_list_UIControl[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Enabled = true;
                    fillDataGridView(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT);

                    dataGridViewTECComponent_CellClick(null, null);
                    break;
                default:
                    break;
            }
        }

        private void dataGridViewTECComponent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((!(sender == null)) && (!(e == null))) {
                dataGridView_CellClick(sender, e);

                //switch (((DataGridView)sender).Columns[e.ColumnIndex].GetType().Name) {
                switch (((DataGridView)m_list_UIControl[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT]).Columns[e.ColumnIndex].GetType().Name)
                {
                    //Копия 'else':
                    case "DataGridViewTextBoxColumn":
                        break;
                    case "DataGridViewButtonColumn":
                        Action action = new Action();
                        action.mode = (short)comboBoxMode.SelectedIndex;
                        action.Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC, (short)getIdSelectedTEC());
                        action.Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT, (short)getIdSelectedTECComponent());
                        action.Ids(INDEX_UICONTROL.DATAGRIDVIEW_TG, -1);
                        action.list_act.Add (ID_ACTION.DELETE);

                        m_list_action.Add(action);

                        fillDataGridView(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT);

                        //Необходимо сохранить предыдущую таблицу с ТГ ???
                        getListTG ((ChangeMode.MODE_TECCOMPONENT)comboBoxMode.SelectedIndex, action.Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT));
                        DataTable list_tg = m_list_data[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Copy ();
                        //Необходимо возвратить исходную таблицу с ТГ ???

                        for (int i = 0; i < list_tg.Rows.Count; i ++) {
                            deleteTG((short)(Convert.ToInt32 (list_tg.Rows[i]["ID"])));
                        }
                        break;
                    default:
                        break;
                }
            }
            else {
            }

            //switch (comboBoxMode.SelectedIndex)
            //{
            //    case (int)ChangeMode.MODE_TECCOMPONENT.TEC:
            //        //Только в режиме ТЭЦ             
            //    case (int)ChangeMode.MODE_TECCOMPONENT.GTP:
            //    case (int)ChangeMode.MODE_TECCOMPONENT.PC:
            //        //Только в режимах ГТП, ЩУ
                    fillDataGridView(INDEX_UICONTROL.DATAGRIDVIEW_TG);
                    fillComboBoxTGAdd();
            //        break;
            //    default:
            //        break;
            //}
        }

        private void dataGridViewTG_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView_CellClick(sender, e);

            switch (((DataGridView)sender).Columns[e.ColumnIndex].GetType().Name)
            //switch (((DataGridView)m_list_UIControl[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG]).Columns[e.ColumnIndex].GetType().Name)
            {
                case "DataGridViewTextBoxColumn":
                    break;
                case "DataGridViewButtonColumn":
                    deleteTG((short)getIdSelectedTG());

                    fillDataGridView(INDEX_UICONTROL.DATAGRIDVIEW_TG);
                    fillComboBoxTGAdd();
                    break;
                default:
                    break;
            }
        }

        private void deleteTG (short id_tg) {
            Action action = new Action();
            action.mode = (short)comboBoxMode.SelectedIndex;
            action.Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC, (short)getIdSelectedTEC());
            if (action.mode > 0)
                action.Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT, (short)getIdSelectedTECComponent());
            else
                action.Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT, -1);
            action.Ids(INDEX_UICONTROL.DATAGRIDVIEW_TG, id_tg);
            action.list_act.Add (ID_ACTION.DELETE);

            action.val = m_list_data [(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Rows [getIndexTG (getIdSelectedTG ())];

            m_list_action.Add(action);
        }

        private void dataGridViewTEC_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView_CellEndEdit(sender, e, (int)INDEX_UICONTROL.DATAGRIDVIEW_TEC);
        }

        private void dataGridViewTECComponent_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView_CellEndEdit(sender, e, (int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT);
        }

        private void dataGridViewTG_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView_CellEndEdit (sender, e, (int)INDEX_UICONTROL.DATAGRIDVIEW_TG);
        }

        private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e, int indx_uicontrol)
        {
            if (((DataGridView)m_list_UIControl[indx_uicontrol]).Columns[e.ColumnIndex].GetType().Name == "DataGridViewTextBoxColumn")
            //if (((DataGridView)sender).Columns[e.ColumnIndex].GetType().Name == "DataGridViewTextBoxColumn")
            {
                Action action = new Action();
                action.mode = (short)comboBoxMode.SelectedIndex;
                action.Ids (INDEX_UICONTROL.DATAGRIDVIEW_TEC, (short)getIdSelectedTEC());

                switch (indx_uicontrol)
                {
                    case (int)INDEX_UICONTROL.DATAGRIDVIEW_TEC:
                        action.Ids (INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT, -1);
                        action.Ids (INDEX_UICONTROL.DATAGRIDVIEW_TG, -1);
                        break;
                    case (int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT:
                        action.Ids (INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT, (short)getIdSelectedTECComponent());
                        action.Ids (INDEX_UICONTROL.DATAGRIDVIEW_TG, -1);
                        break;
                    case (int)INDEX_UICONTROL.DATAGRIDVIEW_TG:
                        action.Ids (INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT, (short)getIdSelectedTECComponent());
                        action.Ids (INDEX_UICONTROL.DATAGRIDVIEW_TG, (short)getIdSelectedTG());
                        break;
                    default:
                        break;
                }

                action.list_act.Add (ID_ACTION.UPDATE);

                int indx_row = -1;
                switch (indx_uicontrol) {
                    case (int)INDEX_UICONTROL.DATAGRIDVIEW_TEC:
                        indx_row = getIndexTEC(getIdSelectedTEC());
                        break;
                    case (int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT:
                        indx_row = getIndexTECComponent (getIdSelectedTECComponent ());
                        break;
                    case (int)INDEX_UICONTROL.DATAGRIDVIEW_TG:
                        indx_row = getIndexTG(getIdSelectedTG());
                        break;
                    default:
                        break;
                }

                //action.val = ((DataGridView)m_list_UIControl[indx_uicontrol])[e.ColumnIndex, e.RowIndex].Value.ToString();
                action.val = m_list_data [indx_uicontrol].Rows [indx_row];
                //action.val = ((DataGridView)sender)[e.ColumnIndex, e.RowIndex].Value.ToString();

                m_list_action.Add(action);
            }
            else
                ;
        }

        private void buttonTECAdd_Click(object sender, EventArgs e)
        {

        }

        private void buttonTECComponentAdd_Click(object sender, EventArgs e)
        {
            Action action = new Action();
            action.mode = (short)comboBoxMode.SelectedIndex;
            action.Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC, (short)getIdSelectedTEC());

            action.Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT, (short)getIdNext((ChangeMode.MODE_TECCOMPONENT)comboBoxMode.SelectedIndex));
            action.Ids(INDEX_UICONTROL.DATAGRIDVIEW_TG, -1);

            action.list_act.Add (ID_ACTION.ADD);

            //action.val = textBoxTECComponentAdd.Text;
            action.val = m_list_data [(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Rows [getIndexTECComponent (getIdSelectedTECComponent ())];
            action.val["ID"] = action.Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT);
            action.val["NAME_SHR"] = textBoxTECComponentAdd.Text;
            textBoxTECComponentAdd.Text = string.Empty;
            //Очитстить поля NAME_GNOVOS (NAME_FUTURE), PREFIX_ADMIN, PREFIX_PBR
            //action.val["NAME_GNOVOS"] = 
            //action.val["NAME_FUTURE"] =
            //action.val["PREFIX_ADMIN"] =
            //action.val["PREFIX_PBR"] = string.Empty;

            m_list_action.Add(action);

            fillDataGridView (INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT);
        }

        private void buttonTGAdd_Click(object sender, EventArgs e)
        {
            Action action = new Action();
            action.mode = (short)comboBoxMode.SelectedIndex;
            action.Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC, (short)getIdSelectedTEC());
            if (action.mode > 0)
                action.Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT, (short)getIdSelectedTECComponent());
            else
                action.Ids(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT, -1);
            action.Ids(INDEX_UICONTROL.DATAGRIDVIEW_TG, (short)Convert.ToInt32 (m_listDataRowComboBoxAddTG [comboBoxTGAdd.SelectedIndex]["ID"]));
            action.list_act.Add(ID_ACTION.ADD);

            action.val = m_listDataRowComboBoxAddTG [comboBoxTGAdd.SelectedIndex];

            m_list_action.Add(action);

            fillDataGridView(INDEX_UICONTROL.DATAGRIDVIEW_TG);
            fillComboBoxTGAdd();
        }
    }
}
