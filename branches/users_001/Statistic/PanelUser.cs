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

namespace Statistic
{
    public class PanelUser : PanelStatistic
    {

        #region Дизайн

        private TreeView_Users treeView_Users;
        private DataGridView_Prop_ComboBoxCell dgvProp;
        private DataGridView_Prop_Text_Check dgvProfile;

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly(cols, rows);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormUser));
           
            this.treeView_Users = new TreeView_Users();
            this.dgvProp = new StatisticCommon.DataGridView_Prop_ComboBoxCell();
            this.SuspendLayout();

            treeView_Users.Dock = DockStyle.Fill;
            dgvProp.Dock = DockStyle.Fill;

            initializeLayoutStyle(20, 20);
            
            this.Controls.Add(this.treeView_Users, 0, 0); this.SetColumnSpan(this.treeView_Users, 7); this.SetRowSpan(this.treeView_Users, 20);
            this.Controls.Add(this.dgvProp, 7, 0); this.SetColumnSpan(this.dgvProp, 13); this.SetRowSpan(this.dgvProp, 6);

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
        DataTable table_TEC = new DataTable();

        DataTable[] m_arr_origTable = new DataTable[(int)ID_Table.Unknow];

        DataTable[] m_arr_editTable = new DataTable[(int)ID_Table.Unknow];

        TreeView_Users.Type_Comp m_type_sel_node;

        /// <summary>
        /// Идентификаторы для типов компонента ТЭЦ
        /// </summary>
        public enum ID_Table : int { Role = 0, User, Profiles, Unknow }

        #endregion

        public PanelUser()
            : base()
        {
            int idListener;
            DbConnection connConfigDB;

            int err = -1;

            InitializeComponent();

            m_AllUnits = HUsers.GetTableProfileUnits;
            dgvProfile = new DataGridView_Prop_Text_Check(m_AllUnits);
            dgvProfile.Dock = DockStyle.Fill;
            this.Controls.Add(this.dgvProfile, 7, 6); this.SetColumnSpan(this.dgvProfile, 13); this.SetRowSpan(this.dgvProfile, 13);

            treeView_Users.EditNode += new TreeView_Users.EditNodeEventHandler(this.treeViewSelectNode);
            dgvProp.EventCellValueChanged += new StatisticCommon.DataGridView_Prop.DataGridView_Prop_ValuesCellValueChangedEventHandler(this.dgvProp_EndCellEdit);

            dgvProfile.EventCellValueChanged += new StatisticCommon.DataGridView_Prop_Text_Check.DataGridView_Prop_ValuesCellValueChangedEventHandler(this.dgvProfile_EndCellEdit);

            idListener = register_idListenerConfDB(out err);
            connConfigDB = DbSources.Sources().GetConnection(idListener, out err);
            DataColumn[] columns = { new DataColumn("ID"), new DataColumn("DESCRIPTION") };
            table_TEC.Columns.AddRange(columns);

            m_list_TEC = new InitTEC_200(idListener, true, false).tec;

            foreach (TEC t in m_list_TEC)
            {
                object[] row = { t.m_id.ToString(), t.name_shr.ToString() };

                table_TEC.Rows.Add(row);
            }

            HStatisticUsers.GetUsers(ref connConfigDB, @"", @"DESCRIPTION", out m_arr_origTable[(int)ID_Table.User], out err);
            m_arr_origTable[(int)ID_Table.User].DefaultView.Sort = "ID";
            m_arr_editTable[(int)ID_Table.User] = m_arr_origTable[(int)ID_Table.User].Copy();

            HStatisticUsers.GetRoles(ref connConfigDB, @"", @"DESCRIPTION", out m_arr_origTable[(int)ID_Table.Role], out err);
            m_arr_origTable[(int)ID_Table.Role].DefaultView.Sort = "ID";
            m_arr_editTable[(int)ID_Table.Role] = m_arr_origTable[(int)ID_Table.Role].Copy();

            m_arr_origTable[(int)ID_Table.Profiles] = User.GetTableAllProfile(connConfigDB);
            m_arr_editTable[(int)ID_Table.Profiles] = m_arr_origTable[(int)ID_Table.Profiles].Copy();

            unregister_idListenerConfDB(idListener);

            treeView_Users.Update_tree(m_arr_editTable[(int)ID_Table.User], m_arr_editTable[(int)ID_Table.Role]);
            
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

        protected void treeViewSelectNode(object sender, TreeView_Users.EditNodeEventArgs e)
        {

            DataTable[] massTable = new DataTable[1];
            m_type_sel_node = e.PathComp.type;
            DataTable[] tables = new DataTable[3];
            bool bIsRole = false;
            m_sel_comp = e.IdComp;

            if(e.PathComp.type==TreeView_Users.Type_Comp.User)
            {
                tables[0] = m_arr_editTable[(int)TreeView_Users.Type_Comp.User];
                tables[1] = m_arr_editTable[(int)TreeView_Users.Type_Comp.Role];
                tables[2] = table_TEC;
                dgvProp.Update_dgv(e.IdComp, tables);
                bIsRole = false;
            }
            else
            {
                tables[0] = m_arr_editTable[(int)TreeView_Users.Type_Comp.Role];
                dgvProp.Update_dgv(e.IdComp, tables);
                bIsRole = true;
            }

            massTable[0] = getProfileTable(m_arr_editTable[(int)ID_Table.Profiles], e.PathComp.id_role, e.PathComp.id_user, bIsRole);
            dgvProfile.Update_dgv(e.IdComp, massTable);
        }

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
        /// Внесени изменений в измененную таблицу со списком компонентов
        /// </summary>
        /// <param name="id_comp">ID компонента</param>
        /// <param name="header">Заголовок изменяемой ячейки</param>
        /// <param name="value">Новое значение изменяемой ячейки</param>
        /// <param name="table_edit">Таблицу в которую поместить изменения</param>
        private void edit_table(int id_comp, string header, string value, DataTable table_edit)
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

                                //btnOK.Enabled = true;
                                //btnBreak.Enabled = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик события окончания изменения ячейки
        /// </summary>
        private void dgvProp_EndCellEdit(object sender, DataGridView_Prop_ComboBoxCell.DataGridView_Prop_ValuesCellValueChangedEventArgs e)
        {
            delegateReportClear(true);
            if (m_type_sel_node == TreeView_Users.Type_Comp.Role)
            {
                edit_table(e.m_IdComp, e.m_Header_name, e.m_Value, m_arr_editTable[(int)ID_Table.Role]);
            }
            if (m_type_sel_node == TreeView_Users.Type_Comp.User)
            {
                edit_table(e.m_IdComp, e.m_Header_name, e.m_Value, m_arr_editTable[(int)ID_Table.User]);
            }
        }

        private void dgvProfile_EndCellEdit(object sender, DataGridView_Prop_Text_Check.DataGridView_Prop_ValuesCellValueChangedEventArgs e)
        {
            delegateReportClear(true);

            foreach (DataRow r in m_arr_editTable[(int)ID_Table.Profiles].Rows)
            {
                string id = m_AllUnits.Select(@"DESCRIPTION='" + e.m_Header_name+@"'")[0]["ID"].ToString();
                if (r["ID_UNIT"].ToString() == id)
                    if(r["ID_EXT"].ToString() == m_sel_comp.ToString())
                        if (Convert.ToBoolean(Convert.ToInt32(r["IS_ROLE"])) == !Convert.ToBoolean((int)m_type_sel_node))
                        {
                            if(e.m_Value=="True")
                                r["VALUE"] = 1;
                            else
                                if(e.m_Value=="False")
                                    r["VALUE"] = 0;
                                else
                                    r["VALUE"] = e.m_Value;
                        }
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
