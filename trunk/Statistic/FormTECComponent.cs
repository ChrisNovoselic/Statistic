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
        private enum INDEX_DATAGRIDVIEW : ushort { TEC, TEC_COMPONENT, TG, COUNT_DATAGRIDVIEW };

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
        private DataGridView[] m_list_datagridviews = null;

        private enum ID_ACTION : short { UPDATE, INSERT, DELETE };
        private struct Action
        {
            public short id_tec;
            public short mode;
            public short id_teccomp;
            public short id_tg;
            public ID_ACTION act;
            public string val;
        };
        
        private List <Action> m_list_action;

        public FormTECComponent(ConnectionSettings connSett)
        {
            m_connectionSetttings = connSett;

            InitializeComponent();

            m_list_datagridviews = new DataGridView[(int)INDEX_DATAGRIDVIEW.COUNT_DATAGRIDVIEW];
            m_list_datagridviews[(int)INDEX_DATAGRIDVIEW.TEC] = dataGridViewTEC;
            m_list_datagridviews[(int)INDEX_DATAGRIDVIEW.TEC_COMPONENT] = dataGridViewTECComponent;
            m_list_datagridviews[(int)INDEX_DATAGRIDVIEW.TG] = dataGridViewTG;

            this.ColumnTextBoxTECName.Width = m_list_datagridviews[(int)INDEX_DATAGRIDVIEW.TEC].Width - (2 * (23 + 1));
            this.ColumnTECComponentName.Width = m_list_datagridviews[(int)INDEX_DATAGRIDVIEW.TEC_COMPONENT].Width - (1 * (23 + 1) + 1);
            this.ColumnTGName.Width = m_list_datagridviews[(int)INDEX_DATAGRIDVIEW.TG].Width - (1 * (23 + 1) + 1);

            m_lockObj = new Object();

            m_list_action = new List<Action>();

            m_btnDelete = new System.Windows.Forms.Button();
            m_btnDelete.Text = "Удалить";

            lock (m_lockObj)
            {
            }

            //Пока добавлять/удалять ТЭЦ нельзя

            fillListBox(INDEX_DATAGRIDVIEW.TEC);

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
                return Convert.ToInt32(m_list_TECComponents_original[indx_mode, indx_tec].Rows[m_list_datagridviews[(int)INDEX_DATAGRIDVIEW.TEC_COMPONENT].SelectedRows[0].Index]["ID"]);
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

            return Convert.ToInt32(m_tg_original.Rows[m_list_datagridviews [(int)INDEX_DATAGRIDVIEW.TG].SelectedRows[0].Index]["ID"]);
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

        private DataTable getListTG()
        {
            DataTable dataTableRes = null;
            int id = -1;

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

            m_tg_original = InitTEC.getListTG(m_connectionSetttings, ChangeMode.getPrefixMode(comboBoxMode.SelectedIndex), id);

            dataTableRes = m_tg_original;

            if (comboBoxTGAdd.Enabled == true) comboBoxTGAdd.Items.Clear(); else ;

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
                                    if ((m_list_action[i].mode > 0)
                                        && (m_list_action[i].mode == comboBoxMode.SelectedIndex))
                                        comboBoxTGAdd.Items.Add(dataTableRes.Rows[j]["NAME_SHR"]);
                                    else
                                        ;

                                    dataTableRes.Rows.RemoveAt(j);

                                    break;
                                }
                                else
                                    ;
                            }
                            break;
                        case ID_ACTION.INSERT:
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

            if (comboBoxTGAdd.Items.Count > 0)
            {
                if (comboBoxTGAdd.Enabled == false) comboBoxTGAdd.Enabled = true; else ;
                comboBoxTGAdd.SelectedIndex = 0;
            }
            else
            {
                if (comboBoxTGAdd.Enabled == true) comboBoxTGAdd.Enabled = false; else ;
            }

            buttonTGAdd.Enabled = comboBoxTGAdd.Enabled;
            
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

        private /*static*/ void fillListBox(INDEX_DATAGRIDVIEW indx_datagridview)
        {
            DataGridView dataGridView = m_list_datagridviews [(int)indx_datagridview];
            DataTable data = null;

            switch (indx_datagridview)
            {
                case INDEX_DATAGRIDVIEW.TEC:
                    data = getListTEC();
                    break;
                case INDEX_DATAGRIDVIEW.TEC_COMPONENT:
                    data = getListTECComponent(getIdSelectedTEC());
                    break;
                case INDEX_DATAGRIDVIEW.TG:
                    data = getListTG();
                    break;
                default:
                    break;
            }

            dataGridView.Rows.Clear();
            dataGridView.Rows.Add(data.Rows.Count);

            System.Collections.IEnumerator enumColumns = dataGridView.Columns.GetEnumerator();
            while (enumColumns.MoveNext())
            {
                object col = enumColumns.Current;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    switch (col.GetType ().Name)
                    {
                        case "DataGridViewCheckBoxColumn":
                            dataGridView.Rows[i].Cells[((DataGridViewCheckBoxColumn)col).Index].Value = data.Rows[i]["InUse"];
                            break;
                        case "DataGridViewTextBoxColumn":
                            dataGridView.Rows[i].Cells[((DataGridViewTextBoxColumn)col).Index].Value = data.Rows[i]["NAME_SHR"].ToString();
                            break;
                        case "DataGridViewButtonColumn":
                            dataGridView.Rows[i].Cells[((DataGridViewButtonColumn)col).Index].Value = "-";
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
                    object col = enumColumns.Current;
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
            lock (m_lockObj)
            {
                switch (comboBoxMode.SelectedIndex)
                {
                    case (int)ChangeMode.MODE_TECCOMPONENT.TEC:
                        //Только в режиме ТЭЦ
                        break;
                    case (int)ChangeMode.MODE_TECCOMPONENT.GTP:
                    case (int)ChangeMode.MODE_TECCOMPONENT.PC:
                        //Только в режимах ГТП, ЩУ
                        break;
                    default:
                        break;
                }
            }

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
                    m_list_datagridviews [(int)INDEX_DATAGRIDVIEW.TEC_COMPONENT].Rows.Clear();
                    m_list_datagridviews [(int)INDEX_DATAGRIDVIEW.TEC_COMPONENT].Enabled = false;

                    fillListBox(INDEX_DATAGRIDVIEW.TG);
                    break;
                case (int)ChangeMode.MODE_TECCOMPONENT.GTP:
                case (int)ChangeMode.MODE_TECCOMPONENT.PC:
                    //Только в режимах ГТП, ЩУ
                    m_list_datagridviews [(int)INDEX_DATAGRIDVIEW.TEC_COMPONENT].Enabled = true;
                    fillListBox(INDEX_DATAGRIDVIEW.TEC_COMPONENT);

                    dataGridViewTECComponent_CellClick(null, null);
                    break;
                default:
                    break;
            }
        }

        private void dataGridViewTECComponent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((!(sender == null)) && (!(e == null))) dataGridView_CellClick(sender, e); else ;

            switch (comboBoxMode.SelectedIndex)
            {
                case (int)ChangeMode.MODE_TECCOMPONENT.TEC:
                    //Только в режиме ТЭЦ             
                case (int)ChangeMode.MODE_TECCOMPONENT.GTP:
                case (int)ChangeMode.MODE_TECCOMPONENT.PC:
                    //Только в режимах ГТП, ЩУ
                    fillListBox(INDEX_DATAGRIDVIEW.TG);
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

                fillListBox(INDEX_DATAGRIDVIEW.TG);
            }
            else
                ;
        }

        private void dataGridViewTG_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewTG.Columns[e.ColumnIndex].GetType().Name == "DataGridViewTextBoxColumn")
            {
                Action action = new Action ();
                action.mode = (short)comboBoxMode.SelectedIndex;
                action.id_tec = (short)getIdSelectedTEC ();
                action.id_teccomp = (short)getIdSelectedTECComponent ();
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
