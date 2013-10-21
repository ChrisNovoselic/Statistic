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
        private DataTable m_tg_original = null;
        //Текущие 
        private List<System.Windows.Forms.Control> m_list_UIControl = null;

        private enum ID_ACTION : short { UPDATE, ADD, DELETE };
        private struct Action
        {
            public short id_tec;
            public short mode;
            public short id_teccomp;
            public int id_tg;
            public ID_ACTION act;
            public string val;
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
                        else ;
                        break;
                    default:
                        break;
                }
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
            return Convert.ToInt32(m_list_tec_original.Rows[dataGridViewTEC.SelectedRows[0].Index]["ID"]);
        }

        private Int32 getIdTEC(int indx)
        {
            return Convert.ToInt32(m_list_tec_original.Rows[indx]["ID"]);
        }

        private int getIndexTEC (int id) {
            int iResIndx = -1;

            for (int i = 0; (i < m_list_tec_original.Rows.Count) && (iResIndx < 0); i++)
            {
                if (Convert.ToInt32 (m_list_tec_original.Rows [i]["ID"]) == id)
                    iResIndx = i;
                else
                    ;
            }

            return iResIndx;
        }

        private Int32 getIdSelectedTECComponent()
        {
            int indx_mode = comboBoxMode.SelectedIndex - 1,
                indx_tec = getIndexTEC(getIdSelectedTEC());

            if (indx_mode > -1)
                return Convert.ToInt32(m_list_TECComponents_original[indx_mode, indx_tec].Rows[((DataGridView)m_list_UIControl[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT]).SelectedRows[0].Index]["ID"]);
            else
                return indx_mode;
        }

        private Int32 getIdTECComponent(int indx)
        {
            int indx_mode = comboBoxMode.SelectedIndex - 1,
                indx_tec = getIndexTEC(getIdSelectedTEC());

            if (indx_mode > -1)
                return Convert.ToInt32(m_list_TECComponents_original[indx_mode, indx_tec].Rows[indx]["ID"]);
            else
                return indx_mode;
        }

        private int getIndexTECComponent(int id)
        {
            int iResIndx = -1,
                indx_mode = comboBoxMode.SelectedIndex - 1,
                indx_tec = getIndexTEC(getIdSelectedTEC());

            if (indx_mode > -1)
                for (int i = 0; (i < m_list_TECComponents_original[indx_mode, indx_tec].Rows.Count) && (iResIndx < 0); i++)
                {
                    if (Convert.ToInt32(m_list_TECComponents_original[indx_mode, indx_tec].Rows[i]["ID"]) == id)
                        iResIndx = i;
                    else
                        ;
                }
            else
                ;

            return iResIndx;
        }

        private Int32 getIdSelectedTG()
        {
            int indx_tec = getIndexTEC(getIdSelectedTEC());

            return Convert.ToInt32(m_tg_original.Rows[((DataGridView)m_list_UIControl[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG]).SelectedRows[0].Index]["ID"]);
        }

        private Int32 getIdTG(int indx)
        {
            int indx_tec = getIndexTEC(getIdSelectedTEC());

            return Convert.ToInt32(m_tg_original.Rows[indx]["ID"]);
        }

        private int getIndexTG(int id)
        {
            int iResIndx = -1,
                indx_tec = getIndexTEC(getIdSelectedTEC());

            for (int i = 0; (i < m_tg_original.Rows.Count) && (iResIndx < 0); i++)
            {
                if (Convert.ToInt32(m_tg_original.Rows[i]["ID"]) == id)
                    iResIndx = i;
                else
                    ;
            }

            return iResIndx;
        }

        private DataTable getListTEC()
        {
            DataTable dataTableRes = null;

            if (m_list_tec_original == null)
                m_list_tec_original = InitTEC.getListTEC(m_connectionSetttings, true);
            else ;

            dataTableRes = m_list_tec_original;

            return dataTableRes;
        }

        private DataTable getListTECComponent(int id_tec)
        {
            DataTable dataTableRes = null;
            int indx_tec = getIndexTEC(id_tec);

            if (m_list_TECComponents_original[comboBoxMode.SelectedIndex - 1, indx_tec] == null)
                m_list_TECComponents_original[comboBoxMode.SelectedIndex - 1, indx_tec] = InitTEC.getListTECComponent(m_connectionSetttings, ChangeMode.getPrefixMode(comboBoxMode.SelectedIndex), id_tec);
            else ;

            dataTableRes = m_list_TECComponents_original[comboBoxMode.SelectedIndex - 1, indx_tec];

            return dataTableRes;
        }

        private DataTable getListTG(ChangeMode.MODE_TECCOMPONENT mode = ChangeMode.MODE_TECCOMPONENT.UNKNOWN, int id = -1)
        {
            DataTable dataTableRes = null;

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

            m_tg_original = InitTEC.getListTG(m_connectionSetttings, ChangeMode.getPrefixMode((int)mode), id);

            dataTableRes = m_tg_original;

            for (int i = 0; (! (m_list_action == null)) && (i < m_list_action.Count); i++)
            {
                if ((m_list_action[i].id_tec == getIdSelectedTEC())
                    //&& (m_list_action[i].mode == comboBoxMode.SelectedIndex)
                    // && (m_list_action[i].id_teccomp == getIdSelectedTECComponent())
                    )
                {
                    int j = -1;
                    switch (m_list_action[i].act)
                    {
                        case ID_ACTION.DELETE:
                            bool bDelete = false;
                            
                            for (j = 0; (j < dataTableRes.Rows.Count) && (bDelete == false); j++)
                            {                                
                                if (Convert.ToInt32(dataTableRes.Rows[j]["ID"]) == m_list_action[i].id_tg)
                                {
                                    switch (m_list_action[i].mode)
                                    {
                                        case (int)ChangeMode.MODE_TECCOMPONENT.TEC:
                                            bDelete = true;
                                            break;
                                        case (int)ChangeMode.MODE_TECCOMPONENT.GTP:
                                        case (int)ChangeMode.MODE_TECCOMPONENT.PC:
                                            if (m_list_action[i].id_teccomp == getIdSelectedTECComponent())
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
                                    dataTableRes.Rows.RemoveAt(j);

                                    break;
                                }
                                else
                                    ;
                            }
                            break;
                        case ID_ACTION.ADD:
                            break;
                        case ID_ACTION.UPDATE:
                            for (j = 0; j < dataTableRes.Rows.Count; j++)
                            {
                                if (Convert.ToInt32(dataTableRes.Rows[j]["ID"]) == m_list_action[i].id_tg)
                                {
                                    dataTableRes.Rows[j]["NAME_SHR"] = m_list_action[i].val;
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
            
            return dataTableRes;
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
                    data = getListTEC();
                    break;
                case INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT:
                    data = getListTECComponent(getIdSelectedTEC());
                    break;
                case INDEX_UICONTROL.DATAGRIDVIEW_TG:
                    data = getListTG();
                    break;
                default:
                    break;
            }

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
            List <DataRow>listDataRowComboBoxAddTG = new List <DataRow> ();

            if (comboBoxTGAdd.Enabled == true) comboBoxTGAdd.Items.Clear(); else ;

            for (i = 0; i < m_list_action.Count; i ++) {
                for (j = 0; j < data.Rows.Count; j ++) {
                    if (m_list_action[i].id_tg == Convert.ToInt32 (data.Rows[j]["ID"])) {
                        switch (m_list_action[i].act)
                        {
                            case ID_ACTION.DELETE:
                                switch (m_list_action[i].mode)
                                {
                                    case (int)ChangeMode.MODE_TECCOMPONENT.TEC:
                                        for (k = 0; k < listDataRowComboBoxAddTG.Count; k ++) {
                                            if (Convert.ToInt32 (listDataRowComboBoxAddTG[k]["ID"]) == m_list_action[i].id_tg)
                                                listDataRowComboBoxAddTG.RemoveAt (k);
                                            else
                                                ;
                                        }
                                        break;
                                    case (int)ChangeMode.MODE_TECCOMPONENT.GTP:
                                    case (int)ChangeMode.MODE_TECCOMPONENT.PC:
                                        if (m_list_action[i].mode == comboBoxMode.SelectedIndex) {
                                            listDataRowComboBoxAddTG.Add(data.Rows[j]);
                                        }
                                        else
                                            ;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case ID_ACTION.ADD:
                                break;
                            default:
                                break;
                        }
                    }
                    else
                        ;
                }
            }

            if (listDataRowComboBoxAddTG.Count > 0)
            {
                if (comboBoxTGAdd.Enabled == false) comboBoxTGAdd.Enabled = true; else ;

                for (k = 0; k < listDataRowComboBoxAddTG.Count; k++)
                {
                    comboBoxTGAdd.Items.Add(listDataRowComboBoxAddTG [k]["NAME_SHR"]);
                }

                comboBoxTGAdd.SelectedIndex = 0;
            }
            else
            {
                if (comboBoxTGAdd.Enabled == true) comboBoxTGAdd.Enabled = false; else ;
            }

            buttonTGAdd.Enabled = comboBoxTGAdd.Enabled;
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
            if ((!(sender == null)) && (!(e == null))) dataGridView_CellClick(sender, e); else ;

            switch (((DataGridView)sender).Columns[e.ColumnIndex].GetType().Name) {
                case "DataGridViewTextBoxColumn":
                    switch (comboBoxMode.SelectedIndex)
                    {
                        case (int)ChangeMode.MODE_TECCOMPONENT.TEC:
                        //Только в режиме ТЭЦ             
                        case (int)ChangeMode.MODE_TECCOMPONENT.GTP:
                        case (int)ChangeMode.MODE_TECCOMPONENT.PC:
                            //Только в режимах ГТП, ЩУ
                            fillDataGridView(INDEX_UICONTROL.DATAGRIDVIEW_TG);
                            fillComboBoxTGAdd();
                            break;
                        default:
                            break;
                    }
                    break;
                case "DataGridViewButtonColumn":
                    Action action = new Action ();
                    action.mode = (short)comboBoxMode.SelectedIndex;
                    action.id_tec = (short)getIdSelectedTEC ();
                    action.id_teccomp = (short)getIdSelectedTECComponent ();
                    action.id_tg = -1;
                    action.act = ID_ACTION.DELETE;

                    m_list_action.Add(action);

                    fillDataGridView(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT);
                    break;
                default:
                    break;
            }
        }

        private void dataGridViewTG_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView_CellClick(sender, e);

            if (e.ColumnIndex == ((DataGridView)sender).Columns.Count - 1)
            {
                Action action = new Action ();
                action.mode = (short)comboBoxMode.SelectedIndex;
                action.id_tec = (short)getIdSelectedTEC ();
                if (action.mode > 0)
                    action.id_teccomp = (short)getIdSelectedTECComponent ();
                else
                    action.id_teccomp = -1;
                action.id_tg = (short)getIdSelectedTG();
                action.act = ID_ACTION.DELETE;

                m_list_action.Add(action);

                fillDataGridView(INDEX_UICONTROL.DATAGRIDVIEW_TG);
                fillComboBoxTGAdd();
            }
            else
                ;
        }

        private void dataGridViewTECComponent_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridViewTG_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewTG.Columns[e.ColumnIndex].GetType().Name == "DataGridViewTextBoxColumn")
            {
                Action action = new Action();
                action.mode = (short)comboBoxMode.SelectedIndex;
                action.id_tec = (short)getIdSelectedTEC();
                action.id_teccomp = (short)getIdSelectedTECComponent();
                action.id_tg = (short)getIdSelectedTG();
                action.act = ID_ACTION.UPDATE;

                action.val = dataGridViewTG[e.ColumnIndex, e.RowIndex].Value.ToString();

                m_list_action.Add(action);
            }
            else
                ;
        }
    }
}
