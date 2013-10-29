using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using StatisticCommon;

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

        private DataTable[] m_list_data_original = null,
                            m_list_data = null;
        private List <DataRow[]> m_list_dataRow = null;

        //Текущие элементы управления
        private List<System.Windows.Forms.Control> m_list_UIControl = null;

        private DataRow[] m_list_dataRow_comboBoxAddTG;

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
            
            lock (m_lockObj)
            {
            }

            //Пока добавлять/удалять ТЭЦ нельзя
            m_list_UIControl [(int)INDEX_UICONTROL.TEXTBOX_TEC_ADD].Enabled = false;
            m_list_UIControl[(int)INDEX_UICONTROL.BUTTON_TEC_ADD].Enabled = false;

            // + 1 для ТГ
            m_list_data_original = new DataTable[(int)(FormChangeMode.MODE_TECCOMPONENT.UNKNOWN + 1)];
            m_list_data = new DataTable[(int)(FormChangeMode.MODE_TECCOMPONENT.UNKNOWN + 1)];
            
            int i = -1;
            for (i = (int)(FormChangeMode.MODE_TECCOMPONENT.TEC); i < (int)(FormChangeMode.MODE_TECCOMPONENT.UNKNOWN + 1); i++)
            {
                if (m_list_data_original[i] == null)
                    m_list_data_original[i] = DbInterface.Request(m_connectionSetttings, "SELECT * FROM " + FormChangeMode.getPrefixMode (i) + "_LIST");
                else ;

                m_list_data[i] = m_list_data_original[i].Copy ();
            }

            m_list_dataRow = new List <DataRow[]> (); //[(int)(INDEX_UICONTROL.DATAGRIDVIEW_TG - INDEX_UICONTROL.DATAGRIDVIEW_TEC + 1)];
            for (i = (int)(INDEX_UICONTROL.DATAGRIDVIEW_TEC - INDEX_UICONTROL.DATAGRIDVIEW_TEC); i < (int)(INDEX_UICONTROL.DATAGRIDVIEW_TG - INDEX_UICONTROL.DATAGRIDVIEW_TEC + 1); i++)
            {
                m_list_dataRow.Add(null);
            }

            fillDataGridView(INDEX_UICONTROL.DATAGRIDVIEW_TEC);

            for (i = (int)FormChangeMode.MODE_TECCOMPONENT.TEC; i < (int)FormChangeMode.MODE_TECCOMPONENT.UNKNOWN; i++)
            {
                comboBoxMode.Items.Add(FormChangeMode.getNameMode((short)i));
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
                    case (int)FormChangeMode.MODE_TECCOMPONENT.TEC:
                        //Только в режиме ТЭЦ
                        if (m_list_UIControl [(int)INDEX_UICONTROL.TEXTBOX_TECCOMPONENT_ADD].Enabled == true) m_list_UIControl [(int)INDEX_UICONTROL.TEXTBOX_TECCOMPONENT_ADD].Enabled = false; else ;
                        if (! (m_list_UIControl[(int)INDEX_UICONTROL.BUTTON_TECCOMPONENT_ADD].Enabled == m_list_UIControl[(int)INDEX_UICONTROL.TEXTBOX_TECCOMPONENT_ADD].Enabled))
                            m_list_UIControl[(int)INDEX_UICONTROL.BUTTON_TECCOMPONENT_ADD].Enabled = m_list_UIControl[(int)INDEX_UICONTROL.TEXTBOX_TECCOMPONENT_ADD].Enabled;
                        else
                            ;
                        break;
                    case (int)FormChangeMode.MODE_TECCOMPONENT.GTP:
                    case (int)FormChangeMode.MODE_TECCOMPONENT.PC:
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

        private Int32 getIdSelectedDataRow(INDEX_UICONTROL indx_list_dataRow)
        {
            return getIdDataRow(indx_list_dataRow, ((DataGridView)m_list_UIControl[(int)indx_list_dataRow]).SelectedRows[0].Index);
        }

        private Int32 getIdDataRow(INDEX_UICONTROL indx_list_dataRow, int indx)
        {
            int iRes = -1,
                indx_mode = -1;

            switch (comboBoxMode.SelectedIndex)
            {
                case (int)FormChangeMode.MODE_TECCOMPONENT.TEC:
                    indx_mode = comboBoxMode.SelectedIndex;
                    break;
                case (int)FormChangeMode.MODE_TECCOMPONENT.GTP:
                case (int)FormChangeMode.MODE_TECCOMPONENT.PC:
                    indx_mode = comboBoxMode.SelectedIndex - 1;
                    break;
                default:
                    break;
            }

            if (((DataGridView)m_list_UIControl[(int)indx_list_dataRow]).Rows.Count > 0)
                if (indx_mode > -1)
                    if (indx < m_list_dataRow[(int)indx_list_dataRow].Length)
                        iRes = Convert.ToInt32(m_list_dataRow[(int)indx_list_dataRow][indx]["ID"]);
                    else
                        ;
                else
                    ;
            else
                ;

            return iRes;
        }

        private int getIndexDataRow(INDEX_UICONTROL indx_list_dataRow, int id)
        {
            int iRes = -1,
                indx_mode = - 1;

            switch (comboBoxMode.SelectedIndex)
            {
                case (int)FormChangeMode.MODE_TECCOMPONENT.TEC:
                    indx_mode = comboBoxMode.SelectedIndex;
                    break;
                case (int)FormChangeMode.MODE_TECCOMPONENT.GTP:
                case (int)FormChangeMode.MODE_TECCOMPONENT.PC:
                    indx_mode = comboBoxMode.SelectedIndex - 1;
                    break;
                default:
                    break;
            }

            if (((DataGridView)m_list_UIControl[(int)indx_list_dataRow]).Rows.Count > 0)
                if (indx_mode > -1)
                    for (int i = 0; (i < m_list_dataRow[(int)indx_list_dataRow].Length) && (iRes < 0); i++)
                    {
                        if (Convert.ToInt32(m_list_dataRow[(int)indx_list_dataRow][i]["ID"]) == id)
                            iRes = i;
                        else
                            ;
                    }
                else
                    ;
            else
                ;

            return iRes;
        }

        private void getListTEC()
        {
            m_list_dataRow[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC] = m_list_data[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Select ("");
        }

        private void getListTECComponent()
        {
            int id_tec = getIdSelectedDataRow (INDEX_UICONTROL.DATAGRIDVIEW_TEC),
                indx_tec = getIndexDataRow(INDEX_UICONTROL.DATAGRIDVIEW_TEC, id_tec);

            m_list_dataRow[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT] = null;
            switch (comboBoxMode.SelectedIndex)
            {
                case (int)FormChangeMode.MODE_TECCOMPONENT.TEC:
                    break;
                case (int)FormChangeMode.MODE_TECCOMPONENT.GTP:
                case (int)FormChangeMode.MODE_TECCOMPONENT.PC:
                    m_list_dataRow[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT] = m_list_data[comboBoxMode.SelectedIndex].Select("ID_TEC=" + id_tec);
                    break;
                default:
                    break;
            }
        }

        private void getListTG()
        {
            int id_tec = getIdSelectedDataRow(INDEX_UICONTROL.DATAGRIDVIEW_TEC),
                id_teccomp = -1;

            m_list_dataRow[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG] = null;
            switch (comboBoxMode.SelectedIndex)
            {
                case (int)FormChangeMode.MODE_TECCOMPONENT.TEC:
                    m_list_dataRow[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG] = m_list_data[comboBoxMode.Items.Count].Select("ID_TEC=" + id_tec);
                    break;
                case (int)FormChangeMode.MODE_TECCOMPONENT.GTP:
                case (int)FormChangeMode.MODE_TECCOMPONENT.PC:
                    id_teccomp = getIdSelectedDataRow(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT);
                    m_list_dataRow[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG] = m_list_data[comboBoxMode.Items.Count].Select("ID_TEC=" + id_tec + " AND ID_" + FormChangeMode.getPrefixMode (comboBoxMode.SelectedIndex) + "=" + id_teccomp);
                    break;
                default:
                    break;
            }
        }

        Int32 getIdNext(FormChangeMode.MODE_TECCOMPONENT indx)
        {
            Int32 idRes = -1;

            idRes = Convert.ToInt32 (DbInterface.Request(m_connectionSetttings, "SELECT MAX(ID) FROM " + FormChangeMode.getPrefixMode((int)indx) + "_LIST").Rows [0][0]);

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
            DataRow []dataRow = null;
            System.Collections.IEnumerator enumColumns = null;
            object col = null;

            switch (indx_datagridview)
            {
                case INDEX_UICONTROL.DATAGRIDVIEW_TEC:
                    getListTEC();
                    break;
                case INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT:
                    getListTECComponent();
                    break;
                case INDEX_UICONTROL.DATAGRIDVIEW_TG:
                    getListTG();
                    break;
                default:
                    break;
            }

            dataRow = m_list_dataRow[(int)indx_datagridview];

            dataGridView.Rows.Clear();

            if (!(dataRow == null))
            {
                for (int i = 0; i < dataRow.Length; i++)
                {
                    dataGridView.Rows.Add();

                    enumColumns = dataGridView.Columns.GetEnumerator();

                    while (enumColumns.MoveNext())
                    {
                        col = enumColumns.Current;
                        switch (col.GetType().Name)
                        {
                            case "DataGridViewCheckBoxColumn":
                                dataGridView.Rows[i].Cells[((DataGridViewCheckBoxColumn)col).Index].Value = dataRow[i]["InUse"];
                                break;
                            case "DataGridViewTextBoxColumn":
                                dataGridView.Rows[i].Cells[((DataGridViewTextBoxColumn)col).Index].Value = dataRow[i]["NAME_SHR"].ToString();
                                break;
                            case "DataGridViewButtonColumn":
                                dataGridView.Rows[i].Cells[((DataGridViewButtonColumn)col).Index].Value = "-"; //global::Statistic.Properties.Resources.btnDel
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else
                ;

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
            int i = 1,
                id_tec = getIdSelectedDataRow(INDEX_UICONTROL.DATAGRIDVIEW_TEC);

            switch (comboBoxMode.SelectedIndex)
            {
                case (int)INDEX_UICONTROL.DATAGRIDVIEW_TEC:
             
                    break;
                case (int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT:
                case (int)INDEX_UICONTROL.DATAGRIDVIEW_TG:
                    m_list_dataRow_comboBoxAddTG = m_list_data [comboBoxMode.Items.Count].Select ("ID_TEC=" + id_tec + " AND ID_" + FormChangeMode.getPrefixMode (comboBoxMode.SelectedIndex) + "=" + 0);
                    break;
                default:
                    break;
            }

            if (comboBoxTGAdd.Enabled == true) comboBoxTGAdd.Items.Clear(); else ;

            if ((!(m_list_dataRow_comboBoxAddTG == null)) && (m_list_dataRow_comboBoxAddTG.Length > 0))
            {
                for (i = 0; i < m_list_dataRow_comboBoxAddTG.Length; i++)
                {
                    comboBoxTGAdd.Items.Add(m_list_dataRow_comboBoxAddTG[i]["NAME_SHR"]);
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
                case (int)FormChangeMode.MODE_TECCOMPONENT.TEC:
                    //Только в режиме ТЭЦ
                    ((DataGridView)m_list_UIControl[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT]).Rows.Clear();
                    m_list_UIControl[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT].Enabled = false;

                    fillDataGridView(INDEX_UICONTROL.DATAGRIDVIEW_TG);
                    fillComboBoxTGAdd();
                    break;
                case (int)FormChangeMode.MODE_TECCOMPONENT.GTP:
                case (int)FormChangeMode.MODE_TECCOMPONENT.PC:
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
                        //Удаление GTP/PC
                        m_list_data[comboBoxMode.SelectedIndex].Rows.Remove(m_list_dataRow[(int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT][e.RowIndex]);

                        //Установка в '0' всех соответствующих GTP/PC ТГ
                        for (int i = 0; i < m_list_dataRow [(int)INDEX_UICONTROL.DATAGRIDVIEW_TG].Length; i ++) {
                            deleteTG (i);
                        }

                        fillDataGridView(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT);
                        break;
                    default:
                        break;
                }
            }
            else {
            }

            //switch (comboBoxMode.SelectedIndex)
            //{
            //    case (int)FormChangeMode.MODE_TECCOMPONENT.TEC:
            //        //Только в режиме ТЭЦ             
            //    case (int)FormChangeMode.MODE_TECCOMPONENT.GTP:
            //    case (int)FormChangeMode.MODE_TECCOMPONENT.PC:
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
                    deleteTG(e.RowIndex);

                    fillDataGridView(INDEX_UICONTROL.DATAGRIDVIEW_TG);
                    fillComboBoxTGAdd();
                    break;
                default:
                    break;
            }
        }

        private void deleteTG (int sel_indx) {
            //Установка в '0' поля в соответствии с 'comboBoxMode.SelectedIndex'
            switch (comboBoxMode.SelectedIndex) {
                case (int)FormChangeMode.MODE_TECCOMPONENT.TEC:
                    m_list_data[comboBoxMode.Items.Count].Rows.Remove(m_list_dataRow[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG][sel_indx]);
                    break;
                case (int)FormChangeMode.MODE_TECCOMPONENT.GTP:
                case (int)FormChangeMode.MODE_TECCOMPONENT.PC:
                    int indx_col = m_list_data[comboBoxMode.Items.Count].Columns["ID_" + FormChangeMode.getPrefixMode(comboBoxMode.SelectedIndex)].Ordinal;
                    m_list_dataRow[(int)INDEX_UICONTROL.DATAGRIDVIEW_TG][sel_indx][indx_col] = 0;
                    break;
                default:
                    break;
            }
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
                switch (indx_uicontrol)
                {
                    case (int)INDEX_UICONTROL.DATAGRIDVIEW_TEC:
                        
                        break;
                    case (int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT:
                        
                        break;
                    case (int)INDEX_UICONTROL.DATAGRIDVIEW_TG:
                        
                        break;
                    default:
                        break;
                }

                int indx_row = -1;
                switch (indx_uicontrol) {
                    case (int)INDEX_UICONTROL.DATAGRIDVIEW_TEC:
                        indx_row = getIndexDataRow(INDEX_UICONTROL.DATAGRIDVIEW_TEC, getIdSelectedDataRow(INDEX_UICONTROL.DATAGRIDVIEW_TEC));
                        break;
                    case (int)INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT:
                        indx_row = getIndexDataRow(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT, getIdSelectedDataRow(INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT));
                        break;
                    case (int)INDEX_UICONTROL.DATAGRIDVIEW_TG:
                        indx_row = getIndexDataRow(INDEX_UICONTROL.DATAGRIDVIEW_TG, getIdSelectedDataRow(INDEX_UICONTROL.DATAGRIDVIEW_TG));
                        break;
                    default:
                        break;
                }

            }
            else
                ;
        }

        private void buttonTECAdd_Click(object sender, EventArgs e)
        {

        }

        private void buttonTECComponentAdd_Click(object sender, EventArgs e)
        {
            m_list_data[comboBoxMode.SelectedIndex].Rows.Add();
            m_list_data[comboBoxMode.SelectedIndex].Rows[m_list_data[comboBoxMode.SelectedIndex].Rows.Count - 1]["ID"] = getIdNext ((FormChangeMode.MODE_TECCOMPONENT)comboBoxMode.SelectedIndex);
            m_list_data[comboBoxMode.SelectedIndex].Rows[m_list_data[comboBoxMode.SelectedIndex].Rows.Count - 1]["ID_TEC"] = getIdSelectedDataRow (INDEX_UICONTROL.DATAGRIDVIEW_TEC);
            m_list_data[comboBoxMode.SelectedIndex].Rows[m_list_data[comboBoxMode.SelectedIndex].Rows.Count - 1]["NAME_SHR"] = m_list_UIControl [(int)INDEX_UICONTROL.TEXTBOX_TECCOMPONENT_ADD].Text;

            fillDataGridView (INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT);
        }

        private void buttonTGAdd_Click(object sender, EventArgs e)
        {
            m_list_dataRow_comboBoxAddTG [comboBoxTGAdd.SelectedIndex] ["ID_" + FormChangeMode.getPrefixMode (comboBoxMode.SelectedIndex)] = getIdSelectedDataRow (INDEX_UICONTROL.DATAGRIDVIEW_TEC_COMPONENT);
            
            fillDataGridView(INDEX_UICONTROL.DATAGRIDVIEW_TG);
            fillComboBoxTGAdd();
        }
    }
}
