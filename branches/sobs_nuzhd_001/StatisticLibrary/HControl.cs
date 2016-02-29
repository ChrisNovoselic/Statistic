﻿using HClassLibrary;
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
using ZedGraph;


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
            //this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.MultiSelect = false;
            this.RowHeadersWidth = 250;
        }

        public DataGridView_Prop()
            : base()
        {
            InitializeComponent();//инициализация компонентов

            this.CellValueChanged += new DataGridViewCellEventHandler (this.cell_EndEdit);
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
        protected void cell_EndEdit(object sender, DataGridViewCellEventArgs e)
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

    public class DataGridView_Prop_ComboBoxCell : StatisticCommon.DataGridView_Prop
    {
        enum INDEX_TABLE { user, role, tec }
        /// <summary>
        /// Запрос на получение таблицы со свойствами и ComboBox
        /// </summary>
        /// <param name="id_list">Лист с идентификаторами компонентов</param>
        public override void Update_dgv(int id_component, DataTable[] tables)
        {
            this.CellValueChanged -= cell_EndEdit;
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
                        //combo.Items.Clear();
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
                            //combo.Items.Clear();
                            this.Rows.Add(row);
                            ArrayList TEC = new ArrayList();
                            foreach (DataRow row_tec in tables[(int)INDEX_TABLE.tec].Rows)
                            {
                                TEC.Add(new role(row_tec["DESCRIPTION"].ToString(), row_tec["ID"].ToString()));
                            }
                            TEC.Add(new role("Все ТЭЦ", "0"));
                            combo.DataSource = TEC;
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

            this.CellValueChanged += cell_EndEdit;
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

    public class DataGridView_Prop_Text_Check : StatisticCommon.DataGridView_Prop
    {
        enum INDEX_TABLE { user, role, tec }

        public DataGridView_Prop_Text_Check(DataTable tables)
            : base()
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
            this.CellValueChanged -= cell_EndEdit;
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
            this.CellValueChanged += cell_EndEdit;
        }

        public override void Update_dgv(int id_component, DataTable[] tables)
        {
            this.CellValueChanged -= cell_EndEdit;
            for (int i = 0; i < this.Rows.Count; i++)
            {
                if (this.Rows[i].Cells[0] is DataGridViewCheckBoxCell)
                {
                    if (Convert.ToInt32(tables[0].Rows[i]["VALUE"]) == 0)
                        this.Rows[i].Cells[0].Value = false;
                    else
                        this.Rows[i].Cells[0].Value = true;
                }
                else
                    this.Rows[i].Cells[0].Value = tables[0].Rows[i]["VALUE"];
            }
            this.CellValueChanged += cell_EndEdit;
        }

    }

    public class TreeView_Users : TreeView
    {
        #region Переменные

        /// <summary>
        /// Идентификаторы для типов объектов
        /// </summary>
        public enum ID_OBJ : int { Role = 0, User };

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

        /// <summary>
        /// Идентификаторы для типов компонента ТЭЦ
        /// </summary>
        public enum ID_Menu : int { AddRole = 0, AddUser, Delete}


        List<string> m_open_node = new List<string>();

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenu_TreeView;

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;

            System.Windows.Forms.ToolStripMenuItem добавитьРольToolStripMenuItem;

            contextMenu_TreeView = new System.Windows.Forms.ContextMenuStrip();
            добавитьРольToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextMenuStrip = contextMenu_TreeView;

            #region Context add TEC
            // 
            // contextMenu_TreeView
            // 
            this.contextMenu_TreeView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            добавитьРольToolStripMenuItem});
            this.contextMenu_TreeView.Name = "contextMenu_TreeView";
            // 
            // добавитьТЭЦToolStripMenuItem
            // 
            добавитьРольToolStripMenuItem.Name = "добавитьРольToolStripMenuItem";
            добавитьРольToolStripMenuItem.Text = "Добавить роль";
            #endregion
        }

        public TreeView_Users()
            : base()
        {
            InitializeComponent();

            this.NodeMouseClick += new TreeNodeMouseClickEventHandler(this.tree_NodeClick);
            this.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(this.add_New_Role);

            this.AfterSelect += new TreeViewEventHandler(this.tree_NodeSelect);
        }

        /// <summary>
        /// Возвратить наименование компонента контекстного меню
        /// </summary>
        /// <param name="indx">Индекс режима</param>
        /// <returns>Строка - наименование режима</returns>
        protected static string getNameMode(int indx)
        {
            string[] nameModes = { "Добавить роль", "Добавить пользователя", "Удалить"};

            return nameModes[indx];
        }

        /// <summary>
        /// Для возвращения имена по умолчанию для компонентов
        /// </summary>
        /// <param name="indx">Идентификатор типа компонента</param>
        /// <returns>Имя по умолчанию</returns>
        public static string Mass_NewVal_Comp(int indx)
        {
            String[] arPREFIX_COMPONENT = { "Новая роль", "Новый пользователь"};

            return arPREFIX_COMPONENT[indx];
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
            checked_node(this.Nodes, 0, false);

            this.Nodes.Clear();
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

            checked_node(this.Nodes, 0, true);

            foreach (TreeNode n in this.Nodes)
            {
                if (n.IsExpanded == true)
                {
                    this.SelectedNode = n;
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

        /// <summary>
        /// Метод для переименования ноды
        /// </summary>
        /// <param name="id_comp"></param>
        /// <param name="name"></param>
        public void Rename_Node(ID_Comp id_comp, string name)
        {
            if (id_comp.id_user.Equals(-1) == true & id_comp.id_role.Equals(-1) == false)
            {
                foreach (TreeNode n in this.Nodes)
                {
                    if (get_m_id_list(n.Name).id_role == id_comp.id_role)
                    {
                        n.Text = name;
                    }
                }
            }
            if (id_comp.id_user.Equals(-1) == false)
            {
                foreach (TreeNode n in this.Nodes)
                {
                    if (get_m_id_list(n.Name).id_role == id_comp.id_role)
                    {
                        foreach (TreeNode u in n.Nodes)
                        {
                            if (get_m_id_list(u.Name).id_user == id_comp.id_user)
                            {
                                u.Text = name;
                            }
                        }
                    }
                }
            }
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
        /// Обработчик добавления новой роли
        /// </summary>
        private void add_New_Role(object sender, ToolStripItemClickedEventArgs e)
        {
            if(Report!=null)
                Report(this, new ReportEventArgs(string.Empty, string.Empty, string.Empty, true));

            int id_newRole = 0;
            if (e.ClickedItem.Text == (string)getNameMode((int)ID_Menu.AddRole))
            {
                this.Nodes.Add(Mass_NewVal_Comp((int)ID_OBJ.Role));
                
                if(GetID != null)
                    id_newRole = GetID(this, new GetIDEventArgs(m_selNode_id, (int)ID_OBJ.Role));

                Nodes[Nodes.Count - 1].Name = Convert.ToString(id_newRole);

                ID_Comp id = new ID_Comp();

                id.id_role = -1;
                id.id_user = -1;

                id.id_role = id_newRole;

                if(EditNode != null)
                    EditNode(this, new EditNodeEventArgs(id, ID_Operation.Insert, id.id_role));
            }
        }

        /// <summary>
        /// Обработчик добавления нового пользователя
        /// </summary>
        private void add_New_User(object sender, ToolStripItemClickedEventArgs e)
        {
            if (Report != null)
                Report(this, new ReportEventArgs(string.Empty, string.Empty, string.Empty, true));

            if (e.ClickedItem.Text == (string)getNameMode((int)ID_Menu.AddUser))//Добавление нового пользователя
            {
                int id_newUser = 0;

                ID_Comp id = new ID_Comp();

                id.id_role = -1;
                id.id_user = -1;

                if (GetID != null)
                    id_newUser = GetID(this, new GetIDEventArgs(m_selNode_id,(int)ID_OBJ.User));

                id.id_role = m_selNode_id.id_role;
                id.id_user = id_newUser;

                if (EditNode != null)
                    EditNode(this, new EditNodeEventArgs(id, ID_Operation.Insert, id.id_user));

                foreach (TreeNode role in Nodes)
                {
                    if (Convert.ToInt32(role.Name) == m_selNode_id.id_role)
                    {
                            role.Nodes.Add(Mass_NewVal_Comp((int)ID_OBJ.User));
                            role.Nodes[role.Nodes.Count - 1].Name = Convert.ToString(id.id_role) + ":" + Convert.ToString(id_newUser);
                    }
                }
            }
            else
            {
                if (e.ClickedItem.Text == (string)getNameMode((int)ID_Menu.Delete))//Удаление роли
                {
                    bool del = false;


                    if (SelectedNode.FirstNode == null)
                        {
                            del = true;
                        }
                    if (del == true)
                    {
                        if (EditNode != null)
                            EditNode(this, new EditNodeEventArgs(m_selNode_id, ID_Operation.Delete, m_selNode_id.id_role));

                        SelectedNode.Remove();
                    }
                    else
                    {
                        m_warningReport = "У роли " + SelectedNode.Text + " имеются пользователи!";
                        if (Report != null)
                            Report(this, new ReportEventArgs(string.Empty, string.Empty, m_warningReport, false));
                        //MessageBox.Show("Имеются не выведенные из состава компоненты в " + SelectedNode.Text,"Внимание!",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик удаления пользователя
        /// </summary>
        private void del_user(object sender, ToolStripItemClickedEventArgs e)
        {
            if (Report != null)
                Report(this, new ReportEventArgs(string.Empty, string.Empty, string.Empty, true));

            if (e.ClickedItem.Text == (string)getNameMode((int)ID_Menu.Delete))//Удаление роли
            {
                if (EditNode != null)
                    EditNode(this, new EditNodeEventArgs(m_selNode_id, ID_Operation.Delete, m_selNode_id.id_user));

                SelectedNode.Remove();
            }
        }

        /// <summary>
        /// Обработчик события нажатия на элемент в TreeView
        /// </summary>
        private void tree_NodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            System.Windows.Forms.ContextMenuStrip contextMenu_TreeNode = new System.Windows.Forms.ContextMenuStrip();

            System.Windows.Forms.ToolStripMenuItem УдалитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            System.Windows.Forms.ToolStripMenuItem добавитьПользователяToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            #region Нажатие правой кнопкой мыши

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                this.SelectedNode = e.Node;//Выбор компонента при нажатии на него правой кнопкой мыши
                
                #region Добавление компонентов

                if (m_selNode_id.id_user != -1)//выбран ли элемент пользователь
                {
                            #region Context delete TG
                            // 
                            // contextMenu_TreeNode
                            // 
                            contextMenu_TreeNode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                УдалитьToolStripMenuItem});
                            contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                            // 
                            // УдалитьToolStripMenuItem
                            //
                            УдалитьToolStripMenuItem.Name = "УдалитьToolStripMenuItem";
                            УдалитьToolStripMenuItem.Text = "Удалить";
                            #endregion

                            this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                            this.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(this.del_user);
                }

                if (m_selNode_id.id_user == -1 & m_selNode_id.id_role != -1)//Выбрана ли роль
                {
                    #region Добавление в ТЭЦ компонентов

                    #region Context TEC
                    // 
                    // contextMenu_TreeView_TEC
                    // 
                    contextMenu_TreeNode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    добавитьПользователяToolStripMenuItem,
                    УдалитьToolStripMenuItem});
                    contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                    // 
                    // добавитьПользователяToolStripMenuItem
                    // 
                    добавитьПользователяToolStripMenuItem.Name = "добавитьПользователяToolStripMenuItem";
                    добавитьПользователяToolStripMenuItem.Text = "Добавить пользователя";
                    // 
                    // УдалитьToolStripMenuItem
                    // 
                    УдалитьToolStripMenuItem.Name = "УдалитьToolStripMenuItem";
                    УдалитьToolStripMenuItem.Text = "Удалить";
                    #endregion

                    this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;

                    this.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(this.add_New_User);

                    #endregion
                }

                #endregion
            }

            #endregion
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
            /// Список ID компонента
            /// </summary>
            public ID_Comp PathComp;

            /// <summary>
            /// ID компонента
            /// </summary>
            public int IdComp;

            public GetIDEventArgs(ID_Comp path, int id_comp)
            {
                PathComp = path;
                IdComp = id_comp;
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
        /// Класс для описания аргумента события - передачи сообщения для строки состояния
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

    public class HZedGraphControl : ZedGraph.ZedGraphControl
    {
        public enum INDEX_CONTEXTMENU_ITEM
        {
            SHOW_VALUES,
            SEPARATOR_1
                , COPY, SAVE, TO_EXCEL,
            SEPARATOR_2
                , SETTINGS_PRINT, PRINT,
            SEPARATOR_3
                , AISKUE_PLUS_SOTIASSO, AISKUE, SOTIASSO_3_MIN,
            SOTIASSO_1_MIN
                ,VISIBLE_TABLE
                , COUNT
        };

        // контекстные меню
        protected class HContextMenuStripZedGraph : System.Windows.Forms.ContextMenuStrip
        {
            public HContextMenuStripZedGraph()
            {
                InitializeComponent();
            }

            private void InitializeComponent()
            {
                // 
                // contextMenuStrip
                // 
                this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    new System.Windows.Forms.ToolStripMenuItem()
                    , new System.Windows.Forms.ToolStripSeparator(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem()
                    , new System.Windows.Forms.ToolStripSeparator(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem()
                    , new System.Windows.Forms.ToolStripSeparator(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem()
                    });
                this.Name = "contextMenuStripMins";
                this.Size = new System.Drawing.Size(198, 148);

                int indx = -1;
                // 
                // показыватьЗначенияToolStripMenuItemMins
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.SHOW_VALUES; ;
                this.Items[indx].Name = "показыватьЗначенияToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = "Показывать значения";
                ((ToolStripMenuItem)this.Items[indx]).Checked = true;

                // 
                // копироватьToolStripMenuItemMins
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.COPY;
                this.Items[indx].Name = "копироватьToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = "Копировать";

                // 
                // сохранитьToolStripMenuItemMins
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.SAVE;
                this.Items[indx].Name = "сохранитьToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = "Сохранить график";

                // 
                // эксельToolStripMenuItemMins
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.TO_EXCEL;
                this.Items[indx].Name = "эксельToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = "Сохранить в MS Excel";

                // 
                // параметрыПечатиToolStripMenuItemMins
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.SETTINGS_PRINT;
                this.Items[indx].Name = "параметрыПечатиToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = "Параметры печати";
                // 
                // распечататьToolStripMenuItemMins
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.PRINT;
                this.Items[indx].Name = "распечататьToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = "Распечатать";

                // 
                // источникАИСКУЭиСОТИАССОToolStripMenuItem
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO;
                this.Items[indx].Name = "источникАИСКУЭиСОТИАССОToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = @"АИСКУЭ+СОТИАССО"; //"Источник: БД АИСКУЭ+СОТИАССО - 3 мин";
                ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                this.Items[indx].Enabled = false; //HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_ASKUE_PLUS_SOTIASSO) == true;
                // 
                // источникАИСКУЭToolStripMenuItem
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.AISKUE;
                this.Items[indx].Name = "источникАИСКУЭToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                //Установлено в конструкторе "родителя"
                //this.источникАИСКУЭToolStripMenuItem.Text = "Источник: БД АИСКУЭ - 3 мин";
                ((ToolStripMenuItem)this.Items[indx]).Checked = true;
                this.Items[indx].Enabled = false;
                // 
                // источникСОТИАССО3минToolStripMenuItem
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.SOTIASSO_3_MIN;
                this.Items[indx].Name = "источникСОТИАССО3минToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = @"СОТИАССО(3 мин)"; //"Источник: БД СОТИАССО - 3 мин";
                ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                this.Items[indx].Enabled = false;
                // 
                // источникСОТИАССО1минToolStripMenuItem
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.SOTIASSO_1_MIN;
                this.Items[indx].Name = "источникСОТИАССО1минToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = @"СОТИАССО(1 мин)"; //"Источник: БД СОТИАССО - 1 мин";
                ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                this.Items[indx].Enabled = false;
                // 
                // отобразитьВТаблицеToolStripMenuItem
                // 
                indx = (int)INDEX_CONTEXTMENU_ITEM.VISIBLE_TABLE;
                this.Items[indx].Name = "отобразитьВТаблицеToolStripMenuItem";
                this.Items[indx].Size = new System.Drawing.Size(197, 22);
                this.Items[indx].Text = @"Отобразить в таблице"; //"Источник: БД СОТИАССО - 1 мин";
                ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                this.Items[indx].Enabled = true;
            }
        }

        private object m_lockValue;

        public string SourceDataText
        {
            get
            {
                for (HZedGraphControl.INDEX_CONTEXTMENU_ITEM indx = INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; indx < HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT; indx++)
                    if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)indx]).Checked == true)
                        return ((ToolStripMenuItem)ContextMenuStrip.Items[(int)indx]).Text;
                    else
                        ;

                return string.Empty;
            }
        }

        public HZedGraphControl(object lockVal)
        {
            this.ContextMenuStrip = new HContextMenuStripZedGraph();

            InitializeComponent();

            m_lockValue = lockVal;
        }

        private void InitializeComponent()
        {
            // 
            // zedGraph
            // 
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.Location = arPlacement[(int)CONTROLS.zedGraphMins].pt;
            this.Name = "zedGraph";
            this.ScrollGrace = 0;
            this.ScrollMaxX = 0;
            this.ScrollMaxY = 0;
            this.ScrollMaxY2 = 0;
            this.ScrollMinX = 0;
            this.ScrollMinY = 0;
            this.ScrollMinY2 = 0;
            //this.Size = arPlacement[(int)CONTROLS.zedGraphMins].sz;
            this.TabIndex = 0;
            this.IsEnableHEdit = false;
            this.IsEnableHPan = false;
            this.IsEnableHZoom = false;
            this.IsEnableSelection = false;
            this.IsEnableVEdit = false;
            this.IsEnableVPan = false;
            this.IsEnableVZoom = false;
            this.IsShowPointValues = true;

            InitializeEventHandler();

            this.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.OnPointValueEvent);
            this.DoubleClickEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.OnDoubleClickEvent);
        }

        private void InitializeEventHandler()
        {
            ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.SHOW_VALUES].Click += new System.EventHandler(показыватьЗначенияToolStripMenuItem_Click);
            ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.COPY].Click += new System.EventHandler(копироватьToolStripMenuItem_Click);
            ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.SAVE].Click += new System.EventHandler(сохранитьToolStripMenuItem_Click);
            ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.SETTINGS_PRINT].Click += new System.EventHandler(параметрыПечатиToolStripMenuItem_Click);
            ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.PRINT].Click += new System.EventHandler(распечататьToolStripMenuItem_Click);
        }

        public void InitializeEventHandler(EventHandler fToExcel, EventHandler fSourceData)
        {
            ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.TO_EXCEL].Click += new System.EventHandler(fToExcel);
            for (int i = (int)INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; i < this.ContextMenuStrip.Items.Count; i++)
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[i].Click += new System.EventHandler(fSourceData);
        }

        private void показыватьЗначенияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
            this.IsShowPointValues = ((ToolStripMenuItem)sender).Checked;
        }

        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (m_lockValue)
            {
                this.Copy(false);
            }
        }

        private void параметрыПечатиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PageSetupDialog pageSetupDialog = new PageSetupDialog();
            pageSetupDialog.Document = this.PrintDocument;
            pageSetupDialog.ShowDialog();
        }

        private void распечататьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (m_lockValue)
            {
                this.PrintDocument.Print();
            }
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lock (m_lockValue)
            {
                this.SaveAs();
            }
        }

        private string OnPointValueEvent(object sender, GraphPane pane, CurveItem curve, int iPt)
        {
            return curve[iPt].Y.ToString("f2");
        }

        private bool OnDoubleClickEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            //FormMain.formGraphicsSettings.SetScale();

            return true;
        }

    }


}
