using HClassLibrary;
using StatisticCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common; //DbConnection
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace StatisticCommon
{
    public class DataGridView_Prop : DataGridView
    {
        private void InitializeComponent()
        {
            this.Columns.Add("Значение", "Значение");
            this.ColumnHeadersVisible = true;
            this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            this.RowHeadersVisible = true;
            this.AllowUserToAddRows = false;
            this.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            //this.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            this.MultiSelect = false;
            this.RowHeadersWidth = 250;
        }

        public DataGridView_Prop()
            : base()
        {
            InitializeComponent();//инициализация компонентов

            this.CellEndEdit += new DataGridViewCellEventHandler(this.cell_EndEdit);
        }

        /// <summary>
        /// Запрос на получение таблицы со свойствами
        /// </summary>
        /// <param name="id_list">Лист с идентификаторами компонентов</param>
        public virtual void Update_dgv(int id_component, DataTable [] tables)
        {
            this.Rows.Clear();
            DataRow[] rowsSel = tables[0].Select(@"ID=" + id_component);

            if (rowsSel.Length == 1)
                foreach (DataColumn col in tables[0].Columns)
                {
                    this.Rows.Add(rowsSel[0][col.ColumnName]);
                    this.Rows[this.Rows.Count - 1].HeaderCell.Value = col.ColumnName.ToString();
                }
            else
                Logging.Logg().Error(@"Ошибка....", Logging.INDEX_MESSAGE.NOT_SET);

            //cell_ID_Edit_ReadOnly();
        }
        
        /// <summary>
        /// Метод для присваивания ячейке свойства ReadOnly
        /// </summary>
        /// <param name="id_cell">id ячейки</param>
        private void cell_ID_Edit_ReadOnly()
        {
            for (int i = 0; i < this.Rows.Count; i++)
            {
                if (this.Rows[i].HeaderCell.Value.ToString()[0] == 'I' && this.Rows[i].HeaderCell.Value.ToString()[1] == 'D')
                {
                    this.Rows[i].Cells["Значение"].ReadOnly = true;
                    this.Rows[i].Cells["Значение"].ToolTipText = "Только для чтения";
                }
            }
        }

        /// <summary>
        /// Обработчик события окончания изменения ячейки
        /// </summary>
        private void cell_EndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int n_row = -1;
            for (int i = 0; i < this.Rows.Count; i++)
            {
                if (this.Rows[i].HeaderCell.Value.ToString() == "ID")
                {
                    n_row = (int)this.Rows[i].Cells[0].Value;
                }
            }
            if (Rows[e.RowIndex].Cells[0].Value != null)
            {
                if(EventCellValueChanged!=null)
                EventCellValueChanged(this, new DataGridView_Prop.DataGridView_Prop_ValuesCellValueChangedEventArgs(n_row//Идентификатор компонента
                                    , Rows[e.RowIndex].HeaderCell.Value.ToString() //Идентификатор компонента
                                    , Rows[e.RowIndex].Cells[0].Value.ToString() //Идентификатор параметра с учетом периода расчета
                                    ));
            }
            else
            {
            }
        }

        /// <summary>
        /// Класс для описания аргумента события - изменения значения ячейки
        /// </summary>
        public class DataGridView_Prop_ValuesCellValueChangedEventArgs : EventArgs
        {
            /// <summary>
            /// ID компонента
            /// </summary>
            public int m_IdComp;

            /// <summary>
            /// Имя изменяемого параметра
            /// </summary>
            public string m_Header_name;

            /// <summary>
            /// Значение изменяемого параметра
            /// </summary>
            public string m_Value;

            public DataGridView_Prop_ValuesCellValueChangedEventArgs()
                : base()
            {
                m_IdComp = -1;
                m_Header_name = string.Empty;
                m_Value = string.Empty;

            }

            public DataGridView_Prop_ValuesCellValueChangedEventArgs(int id_comp, string header_name, string value)
                : this()
            {
                m_IdComp = id_comp;
                m_Header_name = header_name;
                m_Value = value;
            }
        }

        /// <summary>
        /// Тип делегата для обработки события - изменение значения в ячейке
        /// </summary>
        public delegate void DataGridView_Prop_ValuesCellValueChangedEventHandler(object obj, DataGridView_Prop_ValuesCellValueChangedEventArgs e);

        /// <summary>
        /// Событие - изменение значения ячейки
        /// </summary>
        public DataGridView_Prop_ValuesCellValueChangedEventHandler EventCellValueChanged;

    }

    public class DataGridView_Prop_ComboBoxCell : DataGridView_Prop
    {
        enum INDEX_TABLE { user, role, tec }
        /// <summary>
        /// Запрос на получение таблицы со свойствами и ComboBox
        /// </summary>
        /// <param name="id_list">Лист с идентификаторами компонентов</param>
        public override void Update_dgv(int id_component, DataTable[] tables)
        {
            this.Rows.Clear();
            DataRow[] rowsSel = tables[(int)INDEX_TABLE.user].Select(@"ID=" + id_component);

            if (rowsSel.Length == 1)
                foreach (DataColumn col in tables[(int)INDEX_TABLE.user].Columns)
                {
                    if (col.ColumnName == "ID_ROLE")
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        DataGridViewComboBoxCell combo = new DataGridViewComboBoxCell();
                        combo.AutoComplete = true;
                        row.Cells.Add(combo);
                        this.Rows.Add(row);
                        ArrayList roles = new ArrayList();
                        foreach (DataRow row_role in tables[(int)INDEX_TABLE.role].Rows)
                        {
                            roles.Add(new role(row_role["DESCRIPTION"].ToString(), row_role["ID"].ToString()));
                        }
                        combo.DataSource = roles;
                        combo.DisplayMember = "NameRole";
                        combo.ValueMember = "IdRole";
                        this.Rows[Rows.Count - 1].HeaderCell.Value = "ID_ROLE";
                        this.Rows[Rows.Count - 1].Cells[0].Value = rowsSel[0][col.ColumnName].ToString();
                    }
                    else
                        if (col.ColumnName == "ID_TEC")
                        {
                            DataGridViewRow row = new DataGridViewRow();
                            DataGridViewComboBoxCell combo = new DataGridViewComboBoxCell();
                            combo.AutoComplete = true;
                            row.Cells.Add(combo);
                            this.Rows.Add(row);
                            ArrayList roles = new ArrayList();
                            foreach (DataRow row_tec in tables[(int)INDEX_TABLE.tec].Rows)
                            {
                                roles.Add(new role(row_tec["DESCRIPTION"].ToString(), row_tec["ID"].ToString()));
                            }
                            roles.Add(new role("Все ТЭЦ", "0"));
                            combo.DataSource = roles;
                            combo.DisplayMember = "NameRole";
                            combo.ValueMember = "IdRole";
                            this.Rows[Rows.Count - 1].HeaderCell.Value = "ID_TEC";
                            this.Rows[Rows.Count - 1].Cells[0].Value = rowsSel[0][col.ColumnName].ToString();
                        }

                        else
                        {
                            this.Rows.Add(rowsSel[0][col.ColumnName]);
                            this.Rows[this.Rows.Count - 1].HeaderCell.Value = col.ColumnName.ToString();
                        }
                }
            else
                Logging.Logg().Error(@"Ошибка....", Logging.INDEX_MESSAGE.NOT_SET);

            //cell_ID_Edit_ReadOnly();
        }

        public class role
        {
            private string Name;
            private string ID;

            public role(string name, string id)
            {

                this.Name = name;
                this.ID = id;
            }

            public string NameRole
            {
                get
                {
                    return Name;
                }
            }

            public string IdRole
            {

                get
                {
                    return ID;
                }
            }

        }

    }

    public class DataGridView_Prop_Text_Check : DataGridView_Prop
    {
        enum INDEX_TABLE { user, role, tec }

        public DataGridView_Prop_Text_Check(DataTable tables)
            :base()
        {
            create_dgv(tables);
            this.RowHeadersWidth = 500;
        }

        /// <summary>
        /// Запрос на получение таблицы со свойствами и ComboBox
        /// </summary>
        /// <param name="id_list">Лист с идентификаторами компонентов</param>
        private void create_dgv(DataTable tables)
        {
            this.Rows.Clear();

            foreach (DataRow r in tables.Rows)
            {
                DataGridViewRow row = new DataGridViewRow();

                if (r["ID_UNIT"].ToString().Trim() == "8")
                {
                    DataGridViewCheckBoxCell check = new DataGridViewCheckBoxCell();
                    row.Cells.Add(check);
                    check.Value = false;
                    this.Rows.Add(row);
                    this.Rows[this.Rows.Count - 1].HeaderCell.Value = r["DESCRIPTION"].ToString().Trim();
                }
                else
                    {
                        this.Rows.Add();
                        this.Rows[this.Rows.Count - 1].HeaderCell.Value = r["DESCRIPTION"].ToString().Trim();
                        this.Rows[this.Rows.Count - 1].Cells[0].Value = "";
                    }
            }
        }

        public override void Update_dgv(int id_component, DataTable[] tables)
        {
            for (int i = 0; i < this.Rows.Count; i++)
            {
                if (this.Rows[i].Cells[0] is DataGridViewCheckBoxCell)
                {
                    if(Convert.ToInt32(tables[0].Rows[i]["VALUE"])==0)
                        this.Rows[i].Cells[0].Value = false;
                    else
                        this.Rows[i].Cells[0].Value = true;
                }
                else
                    this.Rows[i].Cells[0].Value = tables[0].Rows[i]["VALUE"];
            }
        }

    }

    public class TreeView_Users : TreeView
    {
        #region Переменные

        string m_warningReport;

        public struct ID_Comp
        {
            public int id_role;
            public int id_user;
            public Type_Comp type;
        }


        ID_Comp m_selNode_id;

        public enum Type_Comp : int { Role, User }

        /// <summary>
        /// Идентификаторы для типов компонента ТЭЦ
        /// </summary>
        public enum ID_Operation : int { Insert = 0, Delete, Update, Select }

        /// <summary>
        /// Возвратить наименование операции
        /// </summary>
        /// <param name="indx">Индекс режима</param>
        /// <returns>Строка - наименование режима</returns>
        protected static string getNameOperation(Int16 indx)
        {
            string[] nameModes = { "Insert", "Delete", "Update", "Select" };

            return nameModes[indx];
        }

        List<string> m_open_node = new List<string>();

        #endregion

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
        }

        public TreeView_Users()
            : base()
        {
            InitializeComponent();

            this.AfterSelect += new TreeViewEventHandler(this.tree_NodeSelect);
        }

        /// <summary>
        /// Метод для сохранения открытых элементов дерева при обновлении
        /// </summary>
        /// <param name="node">Массив Node с которых нужно начать</param>
        /// <param name="i">Начальное значение счетчика</param>
        /// <param name="set_check">Флаг для установки значений</param>
        private void checked_node(TreeNodeCollection node, int i, bool set_check=false)
        {
            if (set_check == false)
            {
                foreach (TreeNode n in node)
                {
                    if (n.IsExpanded == true)
                    {
                        m_open_node.Add(n.Name);
                        if (n.FirstNode != null)
                            checked_node(n.Nodes,i);
                    }
                }
            }
            if (set_check == true)
            {
                foreach (TreeNode n in node)
                {
                    if (m_open_node.Count > 0 & i < m_open_node.Count)

                    if (m_open_node[i]==n.Name)
                    {
                        n.Expand();
                        i++;
                        if (n.FirstNode != null)
                            checked_node(n.Nodes,i,true);
                    }
                }
            }
        }

        /// <summary>
        /// Заполнение TreeView компонентами
        /// </summary>
        public void Update_tree(DataTable table_users, DataTable table_role)
        {
            int num_node = 0;
            foreach (DataRow r in table_role.Rows)
            {
                Nodes.Add(r["DESCRIPTION"].ToString());
                num_node = Nodes.Count - 1;
                Nodes[num_node].Name = r["ID"].ToString();
                DataRow[] rows = table_users.Select("ID_ROLE="+r["ID"].ToString());
                foreach (DataRow r_u in rows)
                {
                    Nodes[num_node].Nodes.Add(r_u["DESCRIPTION"].ToString());
                    Nodes[num_node].Nodes[Nodes[num_node].Nodes.Count - 1].Name = Nodes[num_node].Name + ":" + r_u["ID"].ToString();
                }
            }
        }

        /// <summary>
        /// Обработчик события выбора элемента в TreeView
        /// </summary>
        private void tree_NodeSelect(object sender, TreeViewEventArgs e)
        {
            int idComp = 0;
            m_selNode_id = get_m_id_list(e.Node.Name);

            if (m_selNode_id.type == Type_Comp.Role)
                idComp = m_selNode_id.id_role;
            if (m_selNode_id.type == Type_Comp.User)
                idComp = m_selNode_id.id_user;

            if (EditNode != null)
            EditNode(this, new EditNodeEventArgs(m_selNode_id, ID_Operation.Select, idComp));
        }
        
        public void Rename_Node(int id_comp, string name)
        {
            
        }

        /// <summary>
        /// Метод для запроса ID компонента в TreeView
        /// </summary>
        /// <param name="id_string">Строка с идентификаторами</param>
        /// <returns>Список с ID</returns>
        private ID_Comp get_m_id_list(string id_string)
        {
            ID_Comp id_comp = new ID_Comp();
            id_comp.id_role = -1;
            id_comp.id_user = -1;

            if (id_string != "")
            {
                string[] path = id_string.Split(':');
                if (path.Length == 2)
                {
                    id_comp.id_user = Convert.ToInt32(path[1].Trim());
                    id_comp.type = Type_Comp.User;
                }
                else
                {
                    id_comp.id_user = -1;
                    id_comp.type = Type_Comp.Role;
                }

                id_comp.id_role = Convert.ToInt32(path[0].Trim());
                
            }
            return id_comp;
        }



        /// <summary>
        /// Класс для описания аргумента события - изменения компонента
        /// </summary>
        public class EditNodeEventArgs : EventArgs
        {
            /// <summary>
            /// Список ID компонента
            /// </summary>
            public ID_Comp PathComp;

            /// <summary>
            /// Тип производимой операции
            /// </summary>
            public ID_Operation Operation;

            public int IdComp;

            /// <summary>
            /// Значение изменяемого параметра
            /// </summary>
            public string Value;

            public EditNodeEventArgs(ID_Comp pathComp, ID_Operation operation, int idComp, string value = null)
            {
                PathComp = pathComp;
                IdComp = idComp;
                Operation = operation;
                Value = value;
            }
        }

        /// <summary>
        /// Тип делегата для обработки события - изменение компонента
        /// </summary>
        public delegate void EditNodeEventHandler(object obj, EditNodeEventArgs e);

        /// <summary>
        /// Событие - редактирование компонента
        /// </summary>
        public event EditNodeEventHandler EditNode;



        /// <summary>
        /// Класс для описания аргумента события - получение ID компонента
        /// </summary>
        public class GetIDEventArgs : EventArgs
        {
            /// <summary>
            /// ID компонента
            /// </summary>
            public int m_IdComp;
            
            public GetIDEventArgs(int id_comp)
            {
                m_IdComp = id_comp;
            }
        }

        /// <summary>
        /// Тип делегата для обработки события - получение ID компонента
        /// </summary>
        public delegate int intGetID(object obj, GetIDEventArgs e);

        /// <summary>
        /// Событие - получение ID компонента
        /// </summary>
        public intGetID GetID;



        /// <summary>
        /// Класс для описания аргумента события - получение репорта
        /// </summary>
        public class ReportEventArgs : EventArgs
        {
            /// <summary>
            /// ID компонента
            /// </summary>
            public string Action;

            public string Error;

            public string Warning;

            public bool Clear;

            public ReportEventArgs(string action, string error, string warning, bool clear)
            {
                Action = action;
                Error = error;
                Warning = warning;
                Clear = clear;
            }
        }

        /// <summary>
        /// Тип делегата для обработки события - получение репорта
        /// </summary>
        public delegate void ReportEventHandler(object obj, ReportEventArgs e);

        /// <summary>
        /// Событие - получение репорта
        /// </summary>
        public ReportEventHandler Report;
    }
}
