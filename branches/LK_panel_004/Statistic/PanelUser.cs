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
            this.dgvProp = new StatisticCommon.DataGridView_Prop_ComboBoxCell();
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
        /// Идентификаторы для типов компонента ТЭЦ
        /// </summary>
        public enum ID_Table : int { Unknown = -1, Role, User, Profiles, Count }

        private DB_Sostav_TEC db_sostav = new DB_Sostav_TEC();

        /// <summary>
        /// Возвратить наименование компонента 
        /// </summary>
        /// <param name="indx">Индекс </param>
        /// <returns>Строка - наименование</returns>
        protected static string getNameMode(ID_Table id)
        {
            string[] nameModes = { "roles", "users", "profiles" };

            return nameModes[(int)id];
        }


        #endregion

        public PanelUser()
            : base()
        {

            m_arr_origTable = new DataTable[(int)ID_Table.Count];
            m_arr_editTable = new DataTable[(int)ID_Table.Count];
            db_sostav = new DB_Sostav_TEC();
            
            InitializeComponent();

            m_AllUnits = HUsers.GetTableProfileUnits;
            dgvProfile = new DataGridView_Prop_Text_Check(m_AllUnits);
            dgvProfile.Dock = DockStyle.Fill;
            this.Controls.Add(this.dgvProfile, 7, 6); this.SetColumnSpan(this.dgvProfile, 13); this.SetRowSpan(this.dgvProfile, 13);

            dgvProp.EventCellValueChanged += new StatisticCommon.DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventHandler(this.dgvProp_EndCellEdit);

            dgvProfile.EventCellValueChanged += new StatisticCommon.DataGridView_Prop_Text_Check.DataGridView_Prop_ValuesCellValueChangedEventHandler(this.dgvProfile_EndCellEdit);

            fillDataTable();
            resetDataTable();

            treeView_Users.Update_tree(m_arr_editTable[(int)ID_Table.User], m_arr_editTable[(int)ID_Table.Role]);

            treeView_Users.GetID += new TreeView_Users.intGetID(this.GetNextID);
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
        /// <returns></returns>
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
        /// <returns>Возвращает ID</returns>
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

            if (validate_saving(m_arr_editTable, out warning) == false)
            {
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

                    db_sostav.Edit(getNameMode(i), keys, m_arr_origTable[(int)i], m_arr_editTable[(int)i], out err);
                }

                fillDataTable();
                resetDataTable();
                treeView_Users.Update_tree(m_arr_editTable[(int)ID_Table.User], m_arr_editTable[(int)ID_Table.Role]);
                btnOK.Enabled = false;
                btnBreak.Enabled = false;

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
        private int GetNextID(object sender, TreeView_Users.GetIDEventArgs e)
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
            /// <returns></returns>
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
        /// <param name="dbConn"></param>
        /// <returns></returns>
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
