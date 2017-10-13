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

namespace Statistic
{
    public class PanelUser : PanelStatistic
    {
        #region Дизайн

        private TreeView_Users treeView_Users;
        private DataGridView_Prop_ComboBoxCell dgvProp;
        private DataGridView_Prop_Text_Check dgvProfile;
        private Button btnOK;
        private Button btnBreak;

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly(cols, rows);
        }

        private void InitializeComponent()
        {
            //System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormUser));
           
            this.treeView_Users = new TreeView_Users();
            this.dgvProp = new DataGridView_Prop_ComboBoxCell();
            this.btnOK = new Button();
            this.btnBreak = new Button();
            this.SuspendLayout();

            treeView_Users.Dock = DockStyle.Fill;
            dgvProp.Dock = DockStyle.Fill;

            btnOK.Dock = DockStyle.Fill;
            //OK.AutoSize = true;
            btnOK.Text = "Применить";
            btnOK.MouseClick += new MouseEventHandler(this.buttonOK_click);
            btnOK.Enabled = false;

            btnBreak.Dock = DockStyle.Fill;
            //Break.AutoSize = true;
            btnBreak.Text = "Отмена";
            btnBreak.MouseClick += new MouseEventHandler(this.buttonBreak_click);
            btnBreak.Enabled = false;

            initializeLayoutStyle(20, 20);
            
            this.Controls.Add(this.treeView_Users, 0, 0); this.SetColumnSpan(this.treeView_Users, 7); this.SetRowSpan(this.treeView_Users, 20);
            this.Controls.Add(this.dgvProp, 7, 0); this.SetColumnSpan(this.dgvProp, 13); this.SetRowSpan(this.dgvProp, 6);
            this.Controls.Add(this.btnOK, 12, 19); this.SetColumnSpan(this.btnOK, 4); this.SetRowSpan(this.btnOK, 1);
            this.Controls.Add(this.btnBreak, 16, 19); this.SetColumnSpan(this.btnBreak, 4); this.SetRowSpan(this.btnBreak, 1);


            this.Name = "PanelUser";

            this.Text = "Пользователи";

            this.ResumeLayout(false);
            this.PerformLayout();

        }

        List<TEC> m_list_TEC = new List<TEC>();

        DataTable m_AllUnits;

        #endregion

        #region Переменные

        DelegateStringFunc delegateErrorReport;
        DelegateStringFunc delegateWarningReport;
        DelegateStringFunc delegateActionReport;
        DelegateBoolFunc delegateReportClear;

        int m_sel_comp;
        TreeView_Users.ID_Comp m_list_id;
        DataTable table_TEC = new DataTable();

        DataTable[] m_arr_origTable;

        DataTable[] m_arr_editTable;

        TreeView_Users.Type_Comp m_type_sel_node;

        /// <summary>
        /// Идентификаторы для типов данных (таблиц с настраиваемыми параметрами)
        /// </summary>
        public enum ID_Table : int { Unknown = -1, Role, User, Profiles, Count }

        //private DB_Sostav_TEC db_sostav;

        /// <summary>
        /// Возвратить наименование таблиц
        /// </summary>
        /// <param name="indx">Индекс(идентификатор) таблицы</param>
        /// <returns>Строка - наименование</returns>
        protected static string getTableName(ID_Table id)
        {
            string[] nameModes = { "roles", "users", "profiles" };

            return nameModes[(int)id];
        }
        #endregion

        #region DataGridView
        public class DataGridView_Prop : DataGridView {
            private void InitializeComponent ()
            {
                this.Columns.Add ("Значение", "Значение");
                this.ColumnHeadersVisible = true;
                this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.Columns [0].SortMode = DataGridViewColumnSortMode.NotSortable;
                this.RowHeadersVisible = true;
                this.AllowUserToAddRows = false;
                this.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                //this.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                //this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                this.MultiSelect = false;
                this.RowHeadersWidth = 250;

                BackColor = HDataGridViewTables.s_dgvCellStyles[(int)HDataGridViewTables.INDEX_CELL_STYLE.COMMON].BackColor;
            }

            public DataGridView_Prop ()
                : base ()
            {
                InitializeComponent ();//инициализация компонентов

                this.CellValueChanged += new DataGridViewCellEventHandler (this.cell_EndEdit);
            }

            /// <summary>
            /// Запрос на получение таблицы со свойствами
            /// </summary>
            /// <param name="id_list">Лист с идентификаторами компонентов</param>
            public virtual void Update_dgv (int id_component, DataTable [] tables)
            {
                this.Rows.Clear ();
                DataRow [] rowsSel = tables [0].Select (@"ID=" + id_component);

                if (rowsSel.Length == 1)
                    foreach (DataColumn col in tables [0].Columns) {
                        this.Rows.Add (rowsSel [0] [col.ColumnName]);
                        this.Rows [this.Rows.Count - 1].HeaderCell.Value = col.ColumnName.ToString ();
                    } else
                        Logging.Logg ().Error (@"Ошибка....", Logging.INDEX_MESSAGE.NOT_SET);

                //cell_ID_Edit_ReadOnly();
            }

            /// <summary>
            /// Метод для присваивания ячейке свойства ReadOnly
            /// </summary>
            /// <param name="id_cell">id ячейки</param>
            private void cell_ID_Edit_ReadOnly ()
            {
                for (int i = 0; i < this.Rows.Count; i++) {
                    if (this.Rows [i].HeaderCell.Value.ToString () [0] == 'I' && this.Rows [i].HeaderCell.Value.ToString () [1] == 'D') {
                        this.Rows [i].Cells ["Значение"].ReadOnly = true;
                        this.Rows [i].Cells ["Значение"].ToolTipText = "Только для чтения";
                    }
                }
            }

            /// <summary>
            /// Обработчик события окончания изменения ячейки
            /// </summary>
            protected void cell_EndEdit (object sender, DataGridViewCellEventArgs e)
            {
                int n_row = -1;
                for (int i = 0; i < this.Rows.Count; i++) {
                    if (this.Rows [i].HeaderCell.Value.ToString () == "ID") {
                        n_row = (int)this.Rows [i].Cells [0].Value;
                    }
                }

                if (Rows [e.RowIndex].Cells [0].Value != null) {
                    if (EventCellValueChanged != null)
                        EventCellValueChanged (this, new DataGridView_Prop.DataGridView_Prop_ValuesCellValueChangedEventArgs (n_row//Идентификатор компонента
                                            , Rows [e.RowIndex].HeaderCell.Value.ToString () //Идентификатор компонента
                                            , Rows [e.RowIndex].Cells [0].Value.ToString () //Идентификатор параметра с учетом периода расчета
                                            ));
                } else {
                }
            }

            /// <summary>
            /// Класс для описания аргумента события - изменения значения ячейки
            /// </summary>
            public class DataGridView_Prop_ValuesCellValueChangedEventArgs : EventArgs {
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

                public DataGridView_Prop_ValuesCellValueChangedEventArgs ()
                    : base ()
                {
                    m_IdComp = -1;
                    m_Header_name = string.Empty;
                    m_Value = string.Empty;
                }

                public DataGridView_Prop_ValuesCellValueChangedEventArgs (int id_comp, string header_name, string value)
                    : this ()
                {
                    m_IdComp = id_comp;
                    m_Header_name = header_name;
                    m_Value = value;
                }
            }

            /// <summary>
            /// Тип делегата для обработки события - изменение значения в ячейке
            /// </summary>
            public delegate void DataGridView_Prop_ValuesCellValueChangedEventHandler (object obj, DataGridView_Prop_ValuesCellValueChangedEventArgs e);

            /// <summary>
            /// Событие - изменение значения ячейки
            /// </summary>
            public DataGridView_Prop_ValuesCellValueChangedEventHandler EventCellValueChanged;
        }

        public class DataGridView_Prop_ComboBoxCell : DataGridView_Prop {
            enum INDEX_TABLE {
                user, role, tec
            }
            /// <summary>
            /// Запрос на получение таблицы со свойствами и ComboBox
            /// </summary>
            /// <param name="id_list">Лист с идентификаторами компонентов</param>
            public override void Update_dgv (int id_component, DataTable [] tables)
            {
                this.CellValueChanged -= cell_EndEdit;
                this.Rows.Clear ();
                DataRow [] rowsSel = tables [(int)INDEX_TABLE.user].Select (@"ID=" + id_component);

                if (rowsSel.Length == 1)
                    foreach (DataColumn col in tables [(int)INDEX_TABLE.user].Columns) {
                        if (col.ColumnName == "ID_ROLE") {
                            DataGridViewRow row = new DataGridViewRow ();
                            DataGridViewComboBoxCell combo = new DataGridViewComboBoxCell ();
                            combo.AutoComplete = true;
                            row.Cells.Add (combo);
                            //combo.Items.Clear();
                            this.Rows.Add (row);
                            ArrayList roles = new ArrayList ();
                            foreach (DataRow row_role in tables [(int)INDEX_TABLE.role].Rows) {
                                roles.Add (new role (row_role ["DESCRIPTION"].ToString (), row_role ["ID"].ToString ()));
                            }
                            combo.DataSource = roles;
                            combo.DisplayMember = "NameRole";
                            combo.ValueMember = "IdRole";
                            this.Rows [Rows.Count - 1].HeaderCell.Value = "ID_ROLE";
                            this.Rows [Rows.Count - 1].Cells [0].Value = rowsSel [0] [col.ColumnName].ToString ();
                        } else
                            if (col.ColumnName == "ID_TEC") {
                            DataGridViewRow row = new DataGridViewRow ();
                            DataGridViewComboBoxCell combo = new DataGridViewComboBoxCell ();
                            combo.AutoComplete = true;
                            row.Cells.Add (combo);
                            //combo.Items.Clear();
                            this.Rows.Add (row);
                            ArrayList TEC = new ArrayList ();
                            foreach (DataRow row_tec in tables [(int)INDEX_TABLE.tec].Rows) {
                                TEC.Add (new role (row_tec ["DESCRIPTION"].ToString (), row_tec ["ID"].ToString ()));
                            }
                            TEC.Add (new role ("Все ТЭЦ", "0"));
                            combo.DataSource = TEC;
                            combo.DisplayMember = "NameRole";
                            combo.ValueMember = "IdRole";
                            this.Rows [Rows.Count - 1].HeaderCell.Value = "ID_TEC";
                            this.Rows [Rows.Count - 1].Cells [0].Value = rowsSel [0] [col.ColumnName].ToString ();
                        } else {
                            this.Rows.Add (rowsSel [0] [col.ColumnName]);
                            this.Rows [this.Rows.Count - 1].HeaderCell.Value = col.ColumnName.ToString ();
                        }

                        this.Rows [this.Rows.Count - 1].Cells [0].Style.BackColor = this.BackColor;
                    } else
                        Logging.Logg ().Error (@"Ошибка....", Logging.INDEX_MESSAGE.NOT_SET);

                this.CellValueChanged += cell_EndEdit;
            }

            public class role {
                private string Name;
                private string ID;

                public role (string name, string id)
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

        public class DataGridView_Prop_Text_Check : DataGridView_Prop {
            enum INDEX_TABLE {
                user, role, tec
            }

            public DataGridView_Prop_Text_Check (DataTable tables)
                : base ()
            {
                create_dgv (tables);
                this.RowHeadersWidth = 500;
            }

            /// <summary>
            /// Запрос на получение таблицы со свойствами и ComboBox
            /// </summary>
            /// <param name="id_list">Лист с идентификаторами компонентов</param>
            private void create_dgv (DataTable tables)
            {
                this.CellValueChanged -= cell_EndEdit;
                this.Rows.Clear ();

                foreach (DataRow r in tables.Rows) {
                    DataGridViewRow row = new DataGridViewRow ();

                    if (r ["ID_UNIT"].ToString ().Trim () == "8") {
                        DataGridViewCheckBoxCell check = new DataGridViewCheckBoxCell ();
                        row.Cells.Add (check);
                        check.Value = false;
                        this.Rows.Add (row);
                        this.Rows [this.Rows.Count - 1].HeaderCell.Value = r ["DESCRIPTION"].ToString ().Trim ();
                    } else {
                        this.Rows.Add ();
                        this.Rows [this.Rows.Count - 1].HeaderCell.Value = r ["DESCRIPTION"].ToString ().Trim ();
                        this.Rows [this.Rows.Count - 1].Cells [0].Value = "";
                    }
                }
                this.CellValueChanged += cell_EndEdit;
            }

            public override void Update_dgv (int id_component, DataTable [] tables)
            {
                this.CellValueChanged -= cell_EndEdit;

                for (int i = 0; i < this.Rows.Count; i++) {
                    if (this.Rows [i].Cells [0] is DataGridViewCheckBoxCell) {
                        if (Convert.ToInt32 (tables [0].Rows [i] ["VALUE"]) == 0)
                            this.Rows [i].Cells [0].Value = false;
                        else
                            this.Rows [i].Cells [0].Value = true;
                    } else
                        this.Rows [i].Cells [0].Value = tables [0].Rows [i] ["VALUE"];
                }

                this.CellValueChanged += cell_EndEdit;
            }
        }

        public class TreeView_Users : TreeView {
            #region Переменные

            /// <summary>
            /// Идентификаторы для типов объектов
            /// </summary>
            public enum ID_OBJ : int { Role = 0, User };

            string m_warningReport;

            public struct ID_Comp {
                public int id_role;
                public int id_user;
                public Type_Comp type;
            }


            ID_Comp m_selNode_id;

            public enum Type_Comp : int {
                Role, User
            }

            /// <summary>
            /// Идентификаторы для типов компонента ТЭЦ
            /// </summary>
            public enum ID_Operation : int {
                Insert = 0, Delete, Update, Select
            }

            /// <summary>
            /// Возвратить наименование операции
            /// </summary>
            /// <param name="indx">Индекс режима</param>
            /// <returns>Строка - наименование режима</returns>
            protected static string getNameOperation (Int16 indx)
            {
                string [] nameModes = { "Insert", "Delete", "Update", "Select" };

                return nameModes [indx];
            }

            /// <summary>
            /// Идентификаторы для типов компонента ТЭЦ
            /// </summary>
            public enum ID_Menu : int {
                AddRole = 0, AddUser, Delete
            }


            List<string> m_open_node = new List<string> ();
            string selected_user = string.Empty;

            #endregion

            private System.Windows.Forms.ContextMenuStrip contextMenu_TreeView;

            private void InitializeComponent ()
            {
                this.Dock = DockStyle.Fill;

                System.Windows.Forms.ToolStripMenuItem добавитьРольToolStripMenuItem;

                contextMenu_TreeView = new System.Windows.Forms.ContextMenuStrip ();
                добавитьРольToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
                this.ContextMenuStrip = contextMenu_TreeView;

                #region Context add TEC
                // 
                // contextMenu_TreeView
                // 
                this.contextMenu_TreeView.Items.AddRange (new System.Windows.Forms.ToolStripItem [] {
            добавитьРольToolStripMenuItem});
                this.contextMenu_TreeView.Name = "contextMenu_TreeView";
                // 
                // добавитьТЭЦToolStripMenuItem
                // 
                добавитьРольToolStripMenuItem.Name = "добавитьРольToolStripMenuItem";
                добавитьРольToolStripMenuItem.Text = "Добавить роль";
                #endregion

                this.HideSelection = false;
            }

            public TreeView_Users ()
                : base ()
            {
                InitializeComponent ();

                this.NodeMouseClick += new TreeNodeMouseClickEventHandler (this.tree_NodeClick);
                this.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler (this.add_New_Role);

                this.AfterSelect += new TreeViewEventHandler (this.tree_NodeSelect);
            }

            /// <summary>
            /// Возвратить наименование компонента контекстного меню
            /// </summary>
            /// <param name="indx">Индекс режима</param>
            /// <returns>Строка - наименование режима</returns>
            protected static string getNameMode (int indx)
            {
                string [] nameModes = { "Добавить роль", "Добавить пользователя", "Удалить" };

                return nameModes [indx];
            }

            /// <summary>
            /// Для возвращения имена по умолчанию для компонентов
            /// </summary>
            /// <param name="indx">Идентификатор типа компонента</param>
            /// <returns>Имя по умолчанию</returns>
            public static string Mass_NewVal_Comp (int indx)
            {
                String [] arPREFIX_COMPONENT = { "Новая роль", "Новый пользователь" };

                return arPREFIX_COMPONENT [indx];
            }

            /// <summary>
            /// Метод для сохранения открытых элементов дерева при обновлении
            /// </summary>
            /// <param name="node">Массив Node с которых нужно начать</param>
            /// <param name="i">Начальное значение счетчика</param>
            /// <param name="set_check">Флаг для установки значений</param>
            private void checked_node (TreeNodeCollection node, int i, bool set_check = false)
            {
                if (set_check == false) {
                    if (this.SelectedNode != null) {
                        selected_user = this.SelectedNode.Text;
                    }
                    foreach (TreeNode n in node) {
                        if (n.IsExpanded == true) {
                            m_open_node.Add (n.Name);
                            if (n.FirstNode != null)
                                checked_node (n.Nodes, i);
                        }
                    }
                }
                if (set_check == true) {
                    foreach (TreeNode n in node) {
                        if (m_open_node.Count > 0 & i < m_open_node.Count)

                            if (m_open_node [i] == n.Name) {
                                n.Expand ();
                                i++;
                                if (n.FirstNode != null)
                                    checked_node (n.Nodes, i, true);
                            }

                        if (n.Text == selected_user) {
                            this.SelectedNode = n;
                        }
                    }
                }
            }

            /// <summary>
            /// Заполнение TreeView компонентами
            /// </summary>
            public void Update_tree (DataTable table_users, DataTable table_role)
            {
                checked_node (this.Nodes, 0, false);

                this.Nodes.Clear ();
                int num_node = 0;
                foreach (DataRow r in table_role.Rows) {
                    Nodes.Add (r ["DESCRIPTION"].ToString ());
                    num_node = Nodes.Count - 1;
                    Nodes [num_node].Name = r ["ID"].ToString ();
                    DataRow [] rows = table_users.Select ("ID_ROLE=" + r ["ID"].ToString ());
                    foreach (DataRow r_u in rows) {
                        Nodes [num_node].Nodes.Add (r_u ["DESCRIPTION"].ToString ());
                        Nodes [num_node].Nodes [Nodes [num_node].Nodes.Count - 1].Name = Nodes [num_node].Name + ":" + r_u ["ID"].ToString ();
                    }
                }

                checked_node (this.Nodes, 0, true);

                //foreach (TreeNode n in this.Nodes)
                //{
                //    if (n.IsExpanded == true)
                //    {
                //        this.SelectedNode = n;
                //    }
                //}
            }

            /// <summary>
            /// Обработчик события выбора элемента в TreeView
            /// </summary>
            private void tree_NodeSelect (object sender, TreeViewEventArgs e)
            {
                int idComp = 0;
                m_selNode_id = get_m_id_list (e.Node.Name);

                if (m_selNode_id.type == Type_Comp.Role)
                    idComp = m_selNode_id.id_role;
                if (m_selNode_id.type == Type_Comp.User)
                    idComp = m_selNode_id.id_user;

                if (EditNode != null)
                    EditNode (this, new EditNodeEventArgs (m_selNode_id, ID_Operation.Select, idComp));
            }

            /// <summary>
            /// Метод для переименования ноды
            /// </summary>
            /// <param name="id_comp"></param>
            /// <param name="name"></param>
            public void Rename_Node (ID_Comp id_comp, string name)
            {
                if (id_comp.id_user.Equals (-1) == true & id_comp.id_role.Equals (-1) == false) {
                    foreach (TreeNode n in this.Nodes) {
                        if (get_m_id_list (n.Name).id_role == id_comp.id_role) {
                            n.Text = name;
                        }
                    }
                }
                if (id_comp.id_user.Equals (-1) == false) {
                    foreach (TreeNode n in this.Nodes) {
                        if (get_m_id_list (n.Name).id_role == id_comp.id_role) {
                            foreach (TreeNode u in n.Nodes) {
                                if (get_m_id_list (u.Name).id_user == id_comp.id_user) {
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
            private ID_Comp get_m_id_list (string id_string)
            {
                ID_Comp id_comp = new ID_Comp ();
                id_comp.id_role = -1;
                id_comp.id_user = -1;

                if (id_string != "") {
                    string [] path = id_string.Split (':');
                    if (path.Length == 2) {
                        id_comp.id_user = Convert.ToInt32 (path [1].Trim ());
                        id_comp.type = Type_Comp.User;
                    } else {
                        id_comp.id_user = -1;
                        id_comp.type = Type_Comp.Role;
                    }

                    id_comp.id_role = Convert.ToInt32 (path [0].Trim ());

                }
                return id_comp;
            }

            /// <summary>
            /// Обработчик добавления новой роли
            /// </summary>
            private void add_New_Role (object sender, ToolStripItemClickedEventArgs e)
            {
                if (Report != null)
                    Report (this, new ReportEventArgs (string.Empty, string.Empty, string.Empty, true));

                int id_newRole = 0;
                if (e.ClickedItem.Text == (string)getNameMode ((int)ID_Menu.AddRole)) {
                    this.Nodes.Add (Mass_NewVal_Comp ((int)ID_OBJ.Role));

                    if (GetID != null)
                        id_newRole = GetID (this, new GetIDEventArgs (m_selNode_id, (int)ID_OBJ.Role));

                    Nodes [Nodes.Count - 1].Name = Convert.ToString (id_newRole);

                    ID_Comp id = new ID_Comp ();

                    id.id_role = -1;
                    id.id_user = -1;

                    id.id_role = id_newRole;

                    if (EditNode != null)
                        EditNode (this, new EditNodeEventArgs (id, ID_Operation.Insert, id.id_role));
                }
            }

            /// <summary>
            /// Обработчик добавления нового пользователя
            /// </summary>
            private void add_New_User (object sender, ToolStripItemClickedEventArgs e)
            {
                if (Report != null)
                    Report (this, new ReportEventArgs (string.Empty, string.Empty, string.Empty, true));

                if (e.ClickedItem.Text == (string)getNameMode ((int)ID_Menu.AddUser))//Добавление нового пользователя
                {
                    int id_newUser = 0;

                    ID_Comp id = new ID_Comp ();

                    id.id_role = -1;
                    id.id_user = -1;

                    if (GetID != null)
                        id_newUser = GetID (this, new GetIDEventArgs (m_selNode_id, (int)ID_OBJ.User));

                    id.id_role = m_selNode_id.id_role;
                    id.id_user = id_newUser;

                    if (EditNode != null)
                        EditNode (this, new EditNodeEventArgs (id, ID_Operation.Insert, id.id_user));

                    foreach (TreeNode role in Nodes) {
                        if (Convert.ToInt32 (role.Name) == m_selNode_id.id_role) {
                            role.Nodes.Add (Mass_NewVal_Comp ((int)ID_OBJ.User));
                            role.Nodes [role.Nodes.Count - 1].Name = Convert.ToString (id.id_role) + ":" + Convert.ToString (id_newUser);
                        }
                    }
                } else {
                    if (e.ClickedItem.Text == (string)getNameMode ((int)ID_Menu.Delete))//Удаление роли
                    {
                        bool del = false;


                        if (SelectedNode.FirstNode == null) {
                            del = true;
                        }
                        if (del == true) {
                            if (EditNode != null)
                                EditNode (this, new EditNodeEventArgs (m_selNode_id, ID_Operation.Delete, m_selNode_id.id_role));

                            SelectedNode.Remove ();
                        } else {
                            m_warningReport = "У роли " + SelectedNode.Text + " имеются пользователи!";
                            if (Report != null)
                                Report (this, new ReportEventArgs (string.Empty, string.Empty, m_warningReport, false));
                            //MessageBox.Show("Имеются не выведенные из состава компоненты в " + SelectedNode.Text,"Внимание!",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                        }
                    }
                }
            }

            /// <summary>
            /// Обработчик удаления пользователя
            /// </summary>
            private void del_user (object sender, ToolStripItemClickedEventArgs e)
            {
                if (Report != null)
                    Report (this, new ReportEventArgs (string.Empty, string.Empty, string.Empty, true));

                if (e.ClickedItem.Text == (string)getNameMode ((int)ID_Menu.Delete))//Удаление роли
                {
                    if (EditNode != null)
                        EditNode (this, new EditNodeEventArgs (m_selNode_id, ID_Operation.Delete, m_selNode_id.id_user));

                    SelectedNode.Remove ();
                }
            }

            /// <summary>
            /// Обработчик события нажатия на элемент в TreeView
            /// </summary>
            private void tree_NodeClick (object sender, TreeNodeMouseClickEventArgs e)
            {
                System.Windows.Forms.ContextMenuStrip contextMenu_TreeNode = new System.Windows.Forms.ContextMenuStrip ();

                System.Windows.Forms.ToolStripMenuItem УдалитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
                System.Windows.Forms.ToolStripMenuItem добавитьПользователяToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();

                #region Нажатие правой кнопкой мыши

                if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                    this.SelectedNode = e.Node;//Выбор компонента при нажатии на него правой кнопкой мыши

                    #region Добавление компонентов

                    if (m_selNode_id.id_user != -1)//выбран ли элемент пользователь
                    {
                        #region Context delete TG
                        // 
                        // contextMenu_TreeNode
                        // 
                        contextMenu_TreeNode.Items.AddRange (new System.Windows.Forms.ToolStripItem [] {
                            УдалитьToolStripMenuItem});
                        contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                        // 
                        // УдалитьToolStripMenuItem
                        //
                        УдалитьToolStripMenuItem.Name = "УдалитьToolStripMenuItem";
                        УдалитьToolStripMenuItem.Text = "Удалить";
                        #endregion

                        this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                        this.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler (this.del_user);
                    }

                    if (m_selNode_id.id_user == -1 & m_selNode_id.id_role != -1)//Выбрана ли роль
                    {
                        #region Добавление в ТЭЦ компонентов

                        #region Context TEC
                        // 
                        // contextMenu_TreeView_TEC
                        // 
                        contextMenu_TreeNode.Items.AddRange (new System.Windows.Forms.ToolStripItem [] {
                            добавитьПользователяToolStripMenuItem
                            , УдалитьToolStripMenuItem});
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

                        this.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler (this.add_New_User);

                        #endregion
                    }

                    #endregion
                }

                #endregion
            }

            /// <summary>
            /// Класс для описания аргумента события - изменения компонента
            /// </summary>
            public class EditNodeEventArgs : EventArgs {
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

                public EditNodeEventArgs (ID_Comp pathComp, ID_Operation operation, int idComp, string value = null)
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
            public delegate void EditNodeEventHandler (object obj, EditNodeEventArgs e);

            /// <summary>
            /// Событие - редактирование компонента
            /// </summary>
            public event EditNodeEventHandler EditNode;

            /// <summary>
            /// Класс для описания аргумента события - получение ID компонента
            /// </summary>
            public class GetIDEventArgs : EventArgs {
                /// <summary>
                /// Список ID компонента
                /// </summary>
                public ID_Comp PathComp;

                /// <summary>
                /// ID компонента
                /// </summary>
                public int IdComp;

                public GetIDEventArgs (ID_Comp path, int id_comp)
                {
                    PathComp = path;
                    IdComp = id_comp;
                }
            }

            /// <summary>
            /// Тип делегата для обработки события - получение ID компонента
            /// </summary>
            public delegate int intGetID (object obj, GetIDEventArgs e);

            /// <summary>
            /// Событие - получение ID компонента
            /// </summary>
            public intGetID GetID;

            /// <summary>
            /// Класс для описания аргумента события - передачи сообщения для строки состояния
            /// </summary>
            public class ReportEventArgs : EventArgs {
                /// <summary>
                /// ID компонента
                /// </summary>
                public string Action;

                public string Error;

                public string Warning;

                public bool Clear;

                public ReportEventArgs (string action, string error, string warning, bool clear)
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
            public delegate void ReportEventHandler (object obj, ReportEventArgs e);

            /// <summary>
            /// Событие - получение репорта
            /// </summary>
            public ReportEventHandler Report;
        }
        #endregion

        public PanelUser ()
            : base(MODE_UPDATE_VALUES.ACTION, FormMain.formGraphicsSettings.BackgroundColor)
        {
            m_arr_origTable = new DataTable[(int)ID_Table.Count];
            m_arr_editTable = new DataTable[(int)ID_Table.Count];
            //db_sostav = new DB_Sostav_TEC();
            
            InitializeComponent();

            m_AllUnits = HUsers.GetTableProfileUnits;
            dgvProfile = new DataGridView_Prop_Text_Check(m_AllUnits);
            dgvProfile.Dock = DockStyle.Fill;
            this.Controls.Add(this.dgvProfile, 7, 6); this.SetColumnSpan(this.dgvProfile, 13); this.SetRowSpan(this.dgvProfile, 13);

            dgvProp.EventCellValueChanged += new DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventHandler(this.dgvProp_EndCellEdit);

            dgvProfile.EventCellValueChanged += new DataGridView_Prop_Text_Check.DataGridView_Prop_ValuesCellValueChangedEventHandler(this.dgvProfile_EndCellEdit);

            fillDataTable();
            resetDataTable();

            treeView_Users.Update_tree(m_arr_editTable[(int)ID_Table.User], m_arr_editTable[(int)ID_Table.Role]);

            treeView_Users.GetID += new TreeView_Users.intGetID(this.getNextID);
            treeView_Users.EditNode += new TreeView_Users.EditNodeEventHandler(this.get_operation_tree);
            treeView_Users.Report += new TreeView_Users.ReportEventHandler(this.tree_report);
            
        }

        /// <summary>
        /// Получение таблиц
        /// </summary>
        private void fillDataTable()
        {
            int idListener;
            DbConnection connConfigDB;

            int err = -1;

            idListener = register_idListenerConfDB(out err);
            connConfigDB = DbSources.Sources().GetConnection(idListener, out err);
            if (table_TEC.Columns.Count == 0)
            {
                DataColumn[] columns = { new DataColumn("ID"), new DataColumn("DESCRIPTION") };
                table_TEC.Columns.AddRange(columns);
            }

            m_list_TEC = new InitTEC_200(idListener, true, new int [] { 0, (int)TECComponent.ID.GTP }, false).tec;
            table_TEC.Rows.Clear();

            foreach (TEC t in m_list_TEC)
            {
                object[] row = { t.m_id.ToString(), t.name_shr.ToString() };

                table_TEC.Rows.Add(row);
            }

            HStatisticUsers.GetUsers(ref connConfigDB, @"", @"DESCRIPTION", out m_arr_origTable[(int)ID_Table.User], out err);
            m_arr_origTable[(int)ID_Table.User].DefaultView.Sort = "ID";

            HStatisticUsers.GetRoles(ref connConfigDB, @"", @"DESCRIPTION", out m_arr_origTable[(int)ID_Table.Role], out err);
            m_arr_origTable[(int)ID_Table.Role].DefaultView.Sort = "ID";

            m_arr_origTable[(int)ID_Table.Profiles] = User.GetTableAllProfile(connConfigDB);

            unregister_idListenerConfDB(idListener);
        }

        /// <summary>
        /// Сброс таблиц
        /// </summary>
        private void resetDataTable()
        {
            for (ID_Table i = ID_Table.Unknown + 1; i < ID_Table.Count; i++)
                m_arr_editTable[(int)i] = m_arr_origTable[(int)i].Copy();
        }

        /// <summary>
        /// Получение таблицы профайла
        /// </summary>
        /// <param name="tableAllProfiles">Таблица со всеми профайлами</param>
        /// <param name="id_role">ИД роли</param>
        /// <param name="id_user">ИД пользователя</param>
        /// <param name="bIsRole">Это роль</param>
        /// <returns>Возвращает таблицу</returns>
        private DataTable getProfileTable(DataTable tableAllProfiles, int id_role, int id_user, bool bIsRole)
        {
            DataTable profileTable = new DataTable();
            profileTable.Columns.Add("ID");
            profileTable.Columns.Add("VALUE");
            profileTable.Columns.Add("ID_UNIT");
            DbConnection connConfigDB;
            int idListener
                , err;
            Dictionary<int, User.UNIT_VALUE> profile = null;

            idListener = register_idListenerConfDB(out err);
            connConfigDB = DbSources.Sources().GetConnection(idListener, out err);

            profile = User.GetDictProfileItem(connConfigDB, id_role, id_user, bIsRole, tableAllProfiles);

            unregister_idListenerConfDB(idListener);

            for (int i = 0; i < profile.Count; i++)
            {
                object[] obj = new object[3];
                obj[0] = i + 1;
                obj[1] = profile[i + 1].m_value;
                obj[2] = profile[i + 1].m_idType;
                profileTable.Rows.Add(obj);
            }

            return profileTable;
        }

        /// <summary>
        /// Активировать/деактивировать панель
        /// </summary>
        /// <param name="active">Признак активации/деактивации</param>
        /// <returns>Результат выполнения метода (изменено состояние панели/не изменено)</returns>
        public override bool Activate(bool activated)
        {
            bool bRes = base.Activate(activated);
            int err = -1;

            if (activated == true)
            {
                //Start();
                //if(m_idListener==0)
                //m_idListener = register_idListenerConfDB(out err, out m_connConfigDB);
            }

            if (activated == false)
            {
                //unregister_idListenerConfDB(m_idListener);
                delegateReportClear(true);
            }

            return bRes;
        }

        /// <summary>
        /// Метод для передачи сообщений на форму
        /// </summary>
        /// <param name="ferr">Делегат для передачи сообщения об ошибке</param>
        /// <param name="fwar">Делегат для передачи предупреждения</param>
        /// <param name="fact">Делегат для передачи сообщения о выполняемом действии</param>
        /// <param name="fclr">Делегат для передачи комады об очистке строки статуса</param>
        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            delegateErrorReport = ferr;
            delegateWarningReport = fwar;
            delegateActionReport = fact;
            delegateReportClear = fclr;
        }

        /// <summary>
        /// Обработчик события получения сообщения от TreeView
        /// </summary>
        private void tree_report(object sender, TreeView_Users.ReportEventArgs e)
        {
            if (e.Action != string.Empty)
                delegateActionReport(e.Action);

            if (e.Warning != string.Empty)
                delegateWarningReport(e.Warning);

            if (e.Error != string.Empty)
                delegateErrorReport(e.Error);

            if (e.Clear != false)
                delegateReportClear(e.Clear);
        }

        /// <summary>
        /// Регистрация ID
        /// </summary>
        /// <param name="err">Ошибка в процессе регистрации</param>
        /// <returns>Идентификатор подписчика на события получения данных от объекта обращения к БД</returns>
        protected int register_idListenerConfDB(out int err)
        {
            err = -1;
            int idListener = -1;

            ConnectionSettings connSett = FormMainBaseWithStatusStrip.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett();
            idListener = DbSources.Sources().Register(connSett, false, CONN_SETT_TYPE.CONFIG_DB.ToString());

            return idListener;
        }

        /// <summary>
        /// Отмена регистрации ID
        /// </summary>
        /// <param name="idListener">ID</param>
        protected void unregister_idListenerConfDB(int idListener)
        {
            DbSources.Sources().UnRegister(idListener);
        }

        /// <summary>
        /// Обработчик получения данных от TreeView
        /// </summary>
        private void get_operation_tree(object sender, TreeView_Users.EditNodeEventArgs e)
        {
            if (e.Operation == TreeView_Users.ID_Operation.Select)
            {
                select(e.PathComp, e.IdComp);
            }
            if (e.Operation == TreeView_Users.ID_Operation.Delete)
            {
                delete(e.PathComp);
            }
            if (e.Operation == TreeView_Users.ID_Operation.Insert)
            {
                insert(e.PathComp);
            }
            if (e.Operation == TreeView_Users.ID_Operation.Update)
            {
                update(e.PathComp, e.Value);
            }
        }

        /// <summary>
        /// Метод удаления компонента из таблицы
        /// </summary>
        /// <param name="list_id">Список идентификаторов объекта</param>
        private void delete(TreeView_Users.ID_Comp list_id)
        {
            int iRes = 0;

            if (list_id.id_user.Equals(-1) == false)
            {
                m_arr_editTable[(int)ID_Table.User].Rows.Remove(m_arr_editTable[(int)ID_Table.User].Select("ID=" + list_id.id_user)[0]);
                iRes = 1;
            }

            if (list_id.id_user.Equals(-1) == true & list_id.id_role.Equals(-1) == false)
            {
                m_arr_editTable[(int)ID_Table.Role].Rows.Remove(m_arr_editTable[(int)ID_Table.Role].Select("ID=" + list_id.id_role)[0]);
                foreach (DataRow r in m_arr_editTable[(int)ID_Table.Profiles].Select("ID_EXT=" + list_id.id_role + "and IS_ROLE = 1"))
                {
                    m_arr_editTable[(int)ID_Table.Profiles].Rows.Remove(r);
                }
                

                iRes = 1;
            }

            if (iRes == 1)
            {
                btnOK.Enabled = true;
                btnBreak.Enabled = true;
            }
        }

        /// <summary>
        /// Метод обновления связей компонента
        /// </summary>
        /// <param name="list_id">Идентификаторы компонента</param>
        /// <param name="obj">Тип изменяемого объекта ИД=1</param>
        private void update(TreeView_Users.ID_Comp list_id, string type_op)
        {
            string type = type_op;
            int iRes = 0;
            if (list_id.id_user.Equals(-1) == false)
            {
                if (iRes == 1)
                {
                    btnOK.Enabled = true;
                    btnBreak.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Метод добавления нового компонента
        /// </summary>
        /// <param name="list_id">Идентификатор нового компонента</param>
        /// <param name="obj"></param>
        private void insert(TreeView_Users.ID_Comp list_id)
        {
            int iRes = 0;
            if (list_id.id_user.Equals(-1) == false)//Добавление нового пользователя
            {
                object[] obj = new object[m_arr_editTable[(int)ID_Table.User].Columns.Count];

                for (int i = 0; i < m_arr_editTable[(int)ID_Table.User].Columns.Count; i++)
                {
                    if (m_arr_editTable[(int)ID_Table.User].Columns[i].ColumnName == "ID")
                    {
                        obj[i] = list_id.id_user;
                    }
                    else
                        if (m_arr_editTable[(int)ID_Table.User].Columns[i].ColumnName == "ID_ROLE")
                        {
                            obj[i] = list_id.id_role;
                        }
                        else
                            if (m_arr_editTable[(int)ID_Table.User].Columns[i].ColumnName == "IP")
                            {
                                obj[i] = "255.255.255.255";
                            }
                            else
                                if (m_arr_editTable[(int)ID_Table.User].Columns[i].ColumnName == "DESCRIPTION")
                                {
                                    obj[i] = TreeView_Users.Mass_NewVal_Comp((int)ID_Table.User);
                                }
                                else
                                    if (m_arr_editTable[(int)ID_Table.User].Columns[i].ColumnName == "ID_TEC")
                                    {
                                        obj[i] = 0;
                                    }
                                    else
                                        obj[i] = -1;
                }

                m_arr_editTable[(int)ID_Table.User].Rows.Add(obj);
                iRes = 1;
            }

            if (list_id.id_user.Equals(-1) == true & list_id.id_role.Equals(-1) == false)//Добавление новой роли
            {
                object[] obj_role = new object[m_arr_editTable[(int)ID_Table.Role].Columns.Count];
                object[] obj_prof = new object[m_arr_editTable[(int)ID_Table.Profiles].Columns.Count];

                for (int i = 0; i < m_arr_editTable[(int)ID_Table.Role].Columns.Count; i++)
                {
                    if (m_arr_editTable[(int)ID_Table.Role].Columns[i].ColumnName == "ID")
                    {
                        obj_role[i] = list_id.id_role;
                    }
                    else
                        if (m_arr_editTable[(int)ID_Table.Role].Columns[i].ColumnName == "DESCRIPTION")
                        {
                            obj_role[i] = TreeView_Users.Mass_NewVal_Comp((int)ID_Table.Role);
                        }
                }

                m_arr_editTable[(int)ID_Table.Role].Rows.Add(obj_role);

                for (int i = 0; i < m_AllUnits.Rows.Count; i++)
                {
                    obj_prof[0] = list_id.id_role;
                    obj_prof[1] = "1";
                    obj_prof[2] = i+1;
                    obj_prof[3] = "0";
                    m_arr_editTable[(int)ID_Table.Profiles].Rows.Add(obj_prof);
                }
                
                iRes = 1;
            }
            
            if (iRes == 1)
            {
                btnOK.Enabled = true;
                btnBreak.Enabled = true;
            }
        }

        /// <summary>
        /// Обработчик события выбора элемента в TreeView
        /// </summary>
        private void select(TreeView_Users.ID_Comp list_id, int IdComp)
        {
            DataTable[] massTable = new DataTable[1];
            DataTable[] tables = new DataTable[3];
            bool bIsRole = false;
            m_sel_comp = IdComp;
            m_list_id = list_id;

            if (list_id.id_user.Equals(-1) == false)
            {
                tables[0] = m_arr_editTable[(int)TreeView_Users.Type_Comp.User];
                tables[1] = m_arr_editTable[(int)TreeView_Users.Type_Comp.Role];
                tables[2] = table_TEC;
                dgvProp.Update_dgv(list_id.id_user, tables);
                bIsRole = false;
                m_type_sel_node = TreeView_Users.Type_Comp.User;
                
            }

            if (list_id.id_user.Equals(-1) == true & list_id.id_role.Equals(-1) == false)
            {
                tables[0] = m_arr_editTable[(int)TreeView_Users.Type_Comp.Role];
                dgvProp.Update_dgv(list_id.id_role, tables);
                bIsRole = true;
                m_type_sel_node = TreeView_Users.Type_Comp.Role;
            }

            massTable[0] = getProfileTable(m_arr_editTable[(int)ID_Table.Profiles], list_id.id_role, list_id.id_user, bIsRole);
            dgvProfile.Update_dgv(IdComp, massTable);
        }

        /// <summary>
        /// Обработчик события окончания изменения ячейки свойств
        /// </summary>
        private void dgvProp_EndCellEdit(object sender, DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventArgs e)
        {
            delegateReportClear(true);

            if (m_type_sel_node == TreeView_Users.Type_Comp.Role)
            {
                edit_table(e.m_IdComp, e.m_Header_name, e.m_Value, m_arr_editTable[(int)ID_Table.Role], m_list_id);
            }
            if (m_type_sel_node == TreeView_Users.Type_Comp.User)
            {
                edit_table(e.m_IdComp, e.m_Header_name, e.m_Value, m_arr_editTable[(int)ID_Table.User], m_list_id);
            }
        }

        /// <summary>
        /// Обработчик события окончания изменения ячейки профайла
        /// </summary>
        private void dgvProfile_EndCellEdit(object sender, DataGridView_Prop_Text_Check.DataGridView_Prop_ValuesCellValueChangedEventArgs e)
        {
            delegateReportClear(true);
            int iRes = 0;

            string id = m_AllUnits.Select(@"DESCRIPTION='" + e.m_Header_name + @"'")[0]["ID"].ToString();

            DataRow[] rows = m_arr_editTable[(int)ID_Table.Profiles].Select("ID_UNIT=" + id + " and ID_EXT=" + m_sel_comp.ToString() + " and IS_ROLE=" + Convert.ToInt32(!Convert.ToBoolean((int)m_type_sel_node)));

            if (rows.Length != 0)
            {
                foreach (DataRow r in rows)
                {
                    if (e.m_Value == "True")
                        r["VALUE"] = 1;
                    else
                        if (e.m_Value == "False")
                            r["VALUE"] = 0;
                        else
                            r["VALUE"] = e.m_Value;

                    iRes = 1;

                    break;
                }
            }
            else
            {
                object[] obj = { m_sel_comp.ToString(), Convert.ToInt32(!Convert.ToBoolean((int)m_type_sel_node)), id, e.m_Value };
                if (e.m_Value == "True")
                    obj[3] = 1;
                else
                    if (e.m_Value == "False")
                        obj[3] = 0;
                    else
                        obj[3] = e.m_Value;

                m_arr_editTable[(int)ID_Table.Profiles].Rows.Add(obj);
                iRes = 1;
            }

            if (iRes == 1)
            {
                btnOK.Enabled = true;
                btnBreak.Enabled = true;
            }
        }

        /// <summary>
        /// Внесени изменений в измененную таблицу со списком компонентов
        /// </summary>
        /// <param name="id_comp">ID компонента</param>
        /// <param name="header">Заголовок изменяемой ячейки</param>
        /// <param name="value">Новое значение изменяемой ячейки</param>
        /// <param name="table_edit">Таблицу в которую поместить изменения</param>
        private void edit_table(int id_comp, string header, string value, DataTable table_edit, TreeView_Users.ID_Comp list_id)
        {
            for (int i = 0; i < table_edit.Rows.Count; i++)
            {
                if ((int)table_edit.Rows[i]["ID"] == id_comp)
                {
                    for (int b = 0; b < table_edit.Columns.Count; b++)
                    {
                        if (table_edit.Columns[b].ColumnName.ToString() == header)
                        {
                            if (table_edit.Rows[i][b].ToString() != value)
                            {
                                table_edit.Rows[i][b] = value;

                                if (header == "DESCRIPTION")
                                {
                                    treeView_Users.Rename_Node(list_id, value);
                                }
                                btnOK.Enabled = true;
                                btnBreak.Enabled = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик события кнопки "Применить"
        /// </summary>
        private void buttonOK_click(object sender, MouseEventArgs e)
        {
            delegateReportClear(true);
            int err = -1;
            string[] warning;
            string keys = string.Empty;

            ConnectionSettings connSett;
            int idListener = -1;
            DbConnection dbConn;

            if (validate_saving (m_arr_editTable, out warning) == false)
            {
                connSett = FormMainBaseWithStatusStrip.s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett ();
                idListener = DbSources.Sources ().Register (connSett, false, CONN_SETT_TYPE.CONFIG_DB.ToString ());
                dbConn = DbSources.Sources ().GetConnection (idListener, out err);

                if (err == 0) {
                    for (ID_Table i = ID_Table.Unknown + 1; i < ID_Table.Count; i++)
                    {
                        switch (i)
                        {
                            case ID_Table.Role:
                            case ID_Table.User:
                                keys = @"ID";
                                break;
                            case ID_Table.Profiles:
                                keys = "ID_EXT,IS_ROLE,ID_UNIT";
                                break;
                            default:
                                break;
                        }

                        //db_sostav.Edit(getTableName(i), keys, m_arr_origTable[(int)i], m_arr_editTable[(int)i], out err);                    
                        DbTSQLInterface.RecUpdateInsertDelete (ref dbConn, getTableName (i), keys, string.Empty, m_arr_origTable [(int)i], m_arr_editTable [(int)i], out err);

                        if (!(err == 0))
                            Logging.Logg ().Error (string.Format ("PanelUser::buttonOK_click () - не сохранены данные для {0} [ключевые_поля={1}]..."
                                    , getTableName (i), keys)
                                , Logging.INDEX_MESSAGE.NOT_SET);
                        else
                            ;
                    }

                    fillDataTable ();
                    resetDataTable();

                    treeView_Users.Update_tree(m_arr_editTable[(int)ID_Table.User], m_arr_editTable[(int)ID_Table.Role]);

                    btnOK.Enabled = false;
                    btnBreak.Enabled = false;
                } else
                    Logging.Logg().Error(string.Format("PanelUser::buttonOK_click () - данные не сохранены..."), Logging.INDEX_MESSAGE.NOT_SET);

                DbSources.Sources ().UnRegister (idListener);
            }
            else
            {
                delegateWarningReport(warning[(int)ID_Table.Role] + warning[(int)ID_Table.User] + warning[(int)ID_Table.Profiles]);
                //MessageBox.Show(warning[0] + warning[1] + warning[2] + warning[3], "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            //db_sostav.Write_Audit(m_table_audit);
        }

        /// <summary>
        /// Обработчик события кнопки "Отмена"
        /// </summary>
        private void buttonBreak_click(object sender, MouseEventArgs e)
        {
            delegateReportClear(true);
            resetDataTable();

            treeView_Users.Update_tree(m_arr_editTable[(int)ID_Table.User], m_arr_editTable[(int)ID_Table.Role]);
            btnOK.Enabled = false;
            btnBreak.Enabled = false;
        }

        /// <summary>
        /// Проверка критичных параметров перед сохранением
        /// </summary>
        /// <param name="mass_table">Таблица для проверки</param>
        /// <param name="warning">Строка с описанием ошибки</param>
        /// <returns>Возвращает переменную показывающую наличие не введенных параметров</returns>
        private bool validate_saving(DataTable[] mass_table, out string[] warning)
        {
            bool have = false;
            int indx = -1;
            warning = new String[mass_table.Length];

            foreach (DataTable table in mass_table)
            {
                indx++;
                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        if (Convert.ToString(row[i]) == "-1")
                        {
                            have = true;
                            warning[indx] += "Для пользователя " + row["DESCRIPTION"] + " параметр " + table.Columns[i].ColumnName + " равен '-1'." + '\n';
                        }
                    }
                }
            }
            return have;
        }

        /// <summary>
        /// Обработчик для получения следующего идентификатора
        /// </summary>
        /// <returns>Возвращает идентификатор</returns>
        private int getNextID(object sender, TreeView_Users.GetIDEventArgs e)
        {
            int ID = 0;
            int err = 0;

            if (e.IdComp == (int)ID_Table.Role)
            {
                ID = DbTSQLInterface.GetIdNext(m_arr_editTable[(int)ID_Table.Role], out err);
            }
            if (e.IdComp == (int)ID_Table.User)
            {
                ID = DbTSQLInterface.GetIdNext(m_arr_editTable[(int)ID_Table.User], out err);
            }

            return ID;
        }

        public override void UpdateGraphicsCurrent (int type)
        {
            //??? ничего не надо делать
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

                Color backColor = value == SystemColors.Control ? SystemColors.Window : value;

                treeView_Users.BackColor = backColor;

                for (int j = 0; j < dgvProfile.ColumnCount; j++)
                    for (int i = 0; i < dgvProfile.RowCount; i++)
                        dgvProfile.Rows [i].Cells[j].Style.BackColor = backColor;

                for (int j = 0; j < dgvProp.ColumnCount; j++)
                    for (int i = 0; i < dgvProp.RowCount; i++)
                        dgvProp.Rows [i].Cells [j].Style.BackColor = backColor;
            }
        }
    }

    public class User
    {
        public static string m_nameTableProfilesData = @"profiles";

        /// <summary>
        /// Таблица со значениями параметров для пользователя и роли
        /// </summary>
        static DataTable m_tblValues = new DataTable();
        static DataTable m_allProfile;

        /// <summary>
        /// Структура со значением и типом значения
        /// </summary>
        public struct UNIT_VALUE
        {
            public object m_value;
            public int m_idType;
        }

        protected class Profile
        {
            DbConnection m_dbConn;
            bool m_bIsRole;
            int m_id_role
                , m_id_user;
            
            public Profile(DbConnection dbConn, int id_role, int id_user, bool bIsRole, DataTable allProfiles)
            {
                m_dbConn = dbConn;
                m_bIsRole = bIsRole;
                m_id_role = id_role;
                m_id_user = id_user;
                Update(true, allProfiles);
            }

            /// <summary>
            /// Метод для получения словаря с параметрами Profil'а для пользователя
            /// </summary>
            /// <param name="id_ext">ИД пользователя</param>
            /// <param name="bIsRole">Флаг для определения роли</param>
            /// <returns>Словарь с параметрами</returns>
            public Dictionary<int, UNIT_VALUE> GetProfileItem
            {
                get
                {
                    int id_unit = -1;
                    DataRow[] unitRows = new DataRow[1]; ;

                    Dictionary<int, UNIT_VALUE> dictRes = new Dictionary<int, UNIT_VALUE>();

                    foreach (DataRow r in HUsers.GetTableProfileUnits.Rows)
                    {
                        id_unit = (int)r[@"ID"];

                        unitRows[0] = GetRowAllowed(id_unit);

                        if (unitRows.Length == 1)
                        {
                            dictRes.Add(id_unit, new UNIT_VALUE() { m_value = unitRows[0][@"VALUE"].ToString().Trim(), m_idType = Convert.ToInt32(unitRows[0][@"ID_UNIT"]) });
                        }
                        else
                            Logging.Logg().Warning(@"", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    return dictRes;
                }
            }

            /// <summary>
            /// Обновление таблиц
            /// </summary>
            /// <param name="id_role">ИД роли</param>
            /// <param name="id_user">ИД пользователя</param>
            private void Update(bool bThrow, DataTable allProfiles)
            {
                string query = string.Empty
                    , errMsg = string.Empty;
                DataRow[] rows;
                DataTable table = new DataTable();

                foreach (DataColumn r in allProfiles.Columns)
                {
                    table.Columns.Add(r.ColumnName);
                }

                if(m_bIsRole==true)
                    rows = allProfiles.Select("ID_EXT=" + m_id_role + @" AND IS_ROLE=1");
                else
                    rows = allProfiles.Select("(ID_EXT=" + m_id_role + @" AND IS_ROLE=1)" + @" OR (ID_EXT=" + m_id_user + @" AND IS_ROLE=0)");

                foreach (DataRow r in rows)
                {
                    table.Rows.Add(r.ItemArray);
                    //foreach (DataColumn c in allProfiles.Columns)
                    //{
                    //    m_tblValues.Rows[m_tblValues.Rows.Count - 1][c.ColumnName] = r[c.ColumnName];
                    //}
                }
                m_tblValues = table.Copy();
            }

            /// <summary>
            /// Метод получения строки со значениями прав доступа
            /// </summary>
            /// <param name="id">ИД типа</param>
            /// <param name="bIsRole"></param>
            /// <returns>Строка таблицы со значениями прав доступа (или др. параметров конфигурации)</returns>
            private DataRow GetRowAllowed(int id)
            {
                DataRow objRes = null;

                DataRow[] rowsAllowed = m_tblValues.Select("ID_UNIT='" + id+"'");
                switch (rowsAllowed.Length)
                {
                    case 1:
                        objRes = rowsAllowed[0];
                        break;
                    case 2:
                        //В табл. с настройками возможность 'id' определена как для "роли", так и для "пользователя"
                        // требуется выбрать строку с 'IS_ROLE' == 0 (пользователя)
                        // ...
                        foreach (DataRow r in rowsAllowed)
                            if (Int16.Parse(r[@"IS_ROLE"].ToString()) == Convert.ToInt32(m_bIsRole))
                            {
                                objRes = r;
                                break;
                            }
                            else
                                ;
                        break;
                    default: //Ошибка - исключение
                        throw new Exception(@"HUsers.HProfiles::GetAllowed (id=" + id + @") - не найдено ни одной записи...");
                }

                return objRes;
            }

            public static DataTable GetAllProfile(DbConnection dbConn)
            {
                int err = -1;
                string query = string.Empty
                    , errMsg = string.Empty;

                query = @"SELECT * FROM " + m_nameTableProfilesData;
                m_allProfile = DbTSQLInterface.Select(ref dbConn, query, null, null, out err);

                return m_allProfile;
            }
        }

        /// <summary>
        /// Метод для получения таблицы со всеми профайлами
        /// </summary>
        /// <param name="dbConn">Объект для соединения с БД</param>
        /// <returns>??? Таблица с профилями всех пользователей</returns>
        public static DataTable GetTableAllProfile(DbConnection dbConn) 
        {
            return Profile.GetAllProfile(dbConn);
        }

        /// <summary>
        /// Метод для получения словаря со значениями прав доступа
        /// </summary>
        /// <param name="iListenerId">Идентификатор для подключения к БД</param>
        /// <param name="id_role">ИД роли</param>
        /// <param name="id_user">ИД пользователя</param>
        /// <param name="bIsRole">Пользователь или роль</param>
        /// <returns>Словарь со значениями</returns>
        public static Dictionary<int, UNIT_VALUE> GetDictProfileItem(DbConnection dbConn, int id_role, int id_user, bool bIsRole, DataTable allProfiles)
        {
            Dictionary<int, UNIT_VALUE> dictPrifileItem = null;
            Profile profile = new Profile(dbConn, id_role, id_user, bIsRole, allProfiles);

            dictPrifileItem = profile.GetProfileItem;

            return dictPrifileItem;
        }
    }
}
