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
    public class PanelTECComponent : PanelStatistic
    {
        #region Design

        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private TreeView_TECComponent treeView_TECComponent;
        private DataGridView_Prop dgvProp;
        private Button btnOK;
        private Button btnBreak;
        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            initializeLayoutStyleEvenly(cols, rows);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            
        
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTECComponent));
            dgvProp = new DataGridView_Prop();
            btnOK = new Button();
            btnBreak = new Button();

            this.SuspendLayout();

            initializeLayoutStyle(6, 37);

            // 
            // treeView_TECComponent
            //
            treeView_TECComponent = new TreeView_TECComponent();
            treeView_TECComponent.Dock = DockStyle.Fill;
            treeView_TECComponent.NodeMouseClick += new TreeNodeMouseClickEventHandler(this.tree_NodeClick);
            treeView_TECComponent.AfterSelect += new TreeViewEventHandler(this.tree_NodeSelect);
            
            // 
            // dataGridViewTEC
            // 
            dgvProp.Dock = DockStyle.Fill;
            dgvProp.AllowUserToAddRows = false;
            dgvProp.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvProp.MultiSelect = false;
            dgvProp.Name = "dataGridViewTEC";
            dgvProp.TabIndex = 16;
            dgvProp.RowHeadersWidth = 250;
            dgvProp.EventCellValueChanged += new DataGridView_Prop.DataGridView_Prop_ValuesCellValueChangedEventHandler(this.dgvProp_EndCellEdit);


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

            this.Controls.Add(treeView_TECComponent, 0, 0); this.SetColumnSpan(treeView_TECComponent, 2); this.SetRowSpan(treeView_TECComponent, 37);
            
            this.Controls.Add(this.dgvProp, 2, 0); this.SetColumnSpan(this.dgvProp, 4); this.SetRowSpan(this.dgvProp, 35);

            this.Controls.Add(btnOK, 4, 35); this.SetColumnSpan(btnOK, 1); this.SetRowSpan(btnOK, 2);
            this.Controls.Add(btnBreak, 5, 35); this.SetColumnSpan(btnBreak, 1); this.SetRowSpan(btnBreak, 2);

            this.Name = "PanelTECComponent";
            this.Text = "Настройка состава ТЭЦ, ГТП, ЩУ";
            this.Dock = System.Windows.Forms.DockStyle.Fill;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        #endregion

        #region Переменные

        DelegateStringFunc delegateErrorReport;
        DelegateStringFunc delegateWarningReport;
        DelegateStringFunc delegateActionReport;
        DelegateBoolFunc delegateReportClear;

        TG new_tg = new TG();

        List<int> m_sel_node_list = new List<int>(4);

        List<TEC> m_list_TEC = new List<TEC>();

        DB_Sostav_TEC db_sostav = new DB_Sostav_TEC();

        /// <summary>
        /// Возвратить наименование компонента контекстного меню
        /// </summary>
        /// <param name="indx">Индекс режима</param>
        /// <returns>Строка - наименование режима</returns>
        public static string getNameMode(Int16 indx)
        {
            string[] nameModes = { "Добавить ГТП", "Добавить ЩУ", "Добавить ТГ", "Ввести в состав", "Вывести из состава", "Добавить ТЭЦ"};

            return nameModes[indx];
        }

        protected DataTable m_table_TG_original,
            m_table_GTP_original,
            m_table_PC_original,
            m_table_TEC_original,
            m_table_TG_edited,
            m_table_GTP_edited,
            m_table_PC_edited,
            m_table_TEC_edited;

        #endregion

        public PanelTECComponent(List<StatisticCommon.TEC> tec)
            : base()
        {
            m_list_TEC = db_sostav.get_list_tec();

            InitializeComponent();
            treeView_TECComponent.Update_tree(m_list_TEC);
            
            fill_DataTable_ComponentsTEC();

            treeView_TECComponent.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(this.add_New_TEC);
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
        /// Метод для получения таблиц компонентов ТЭЦ
        /// </summary>
        private void fill_DataTable_ComponentsTEC()
        {
            string query = null;

            query = @"SELECT [ID],[ID_TEC],[ID_GTP],[ID_PC],[NAME_SHR],[PREFIX],[INDX_COL_RDG_EXCEL] FROM [dbo].[TG_LIST]";

            m_table_TG_original = db_sostav.Request(query);
            m_table_TG_edited = m_table_TG_original.Copy();

            query = @"SELECT [ID_TEC],[ID],[NAME_SHR],[NAME_FUTURE] FROM [dbo].[PC_LIST]";

            m_table_PC_original = db_sostav.Request(query);
            m_table_PC_edited = m_table_PC_original.Copy();

            query = @"SELECT [ID],[ID_TEC],[ID_MC],[INDX_COL_RDG_EXCEL],[NAME_SHR],[NAME_FUTURE],[PREFIX_ADMIN],[PREFIX_PBR],[ID_MT],[KoeffAlarmPcur] FROM [dbo].[GTP_LIST]";

            m_table_GTP_original = db_sostav.Request(query);
            m_table_GTP_edited = m_table_GTP_original.Copy();

            query = @"SELECT [ID],[InUse],[NAME_SHR],[ID_SOURCE_DATA_TM],[ID_SOURCE_DATA],[ID_SOURCE_ADMIN],[ID_SOURCE_PBR],[TABLE_NAME_ADMIN],[TABLE_NAME_PBR],[PREFIX_ADMIN],[PREFIX_PBR],[ADMIN_DATETIME],[PBR_DATETIME],[ADMIN_DIVIAT],[ADMIN_IS_PER],[ADMIN_REC],[PPBRvsPBR],[PBR_NUMBER],[TIMEZONE_OFFSET_MOSCOW],[PATH_RDG_EXCEL],[TEMPLATE_NAME_SGN_DATA_TM],[TEMPLATE_NAME_SGN_DATA_FACT],[ID_SOURCE_MTERM],[ID_LINK_SOURCE_DATA_TM] FROM [dbo].[TEC_LIST]";

            m_table_TEC_original = db_sostav.Request(query);
            m_table_TEC_edited = m_table_TEC_original.Copy();
        }

        /// <summary>
        /// Метод для сброса отредактированных параметров
        /// </summary>
        private void reset_DataTable_ComponentsTEC()
        {
            m_table_TG_edited = m_table_TG_original.Copy();

            m_table_PC_edited = m_table_PC_original.Copy();

            m_table_GTP_edited = m_table_GTP_original.Copy();

            m_table_TEC_edited = m_table_TEC_original.Copy();
        }

        /// <summary>
        /// Обработчик добавления новой ТЭЦ
        /// </summary>
        private void add_New_TEC(object sender, ToolStripItemClickedEventArgs e)
        {
            int err = -1;
            if (e.ClickedItem.Text == (string)getNameMode(5))
            {
                int id_tec = Convert.ToInt32(DbTSQLInterface.GetIdNext(m_table_TEC_edited, out err));
                string name_shr = "Новая ТЭЦ";
                string table_name_admin = "AdminValuesOfID";
                string table_name_pbr = "PPBRvsPBROfID";
                m_table_TEC_edited.Rows.Add();

                m_table_TEC_edited.Rows[m_table_TEC_edited.Rows.Count - 1]["ID"] = id_tec;
                m_table_TEC_edited.Rows[m_table_TEC_edited.Rows.Count - 1]["InUse"] = "1";
                m_table_TEC_edited.Rows[m_table_TEC_edited.Rows.Count - 1]["NAME_SHR"] = name_shr;
                m_table_TEC_edited.Rows[m_table_TEC_edited.Rows.Count - 1]["ID_SOURCE_DATA_TM"] = "675";
                m_table_TEC_edited.Rows[m_table_TEC_edited.Rows.Count - 1]["ID_SOURCE_DATA"] = "675";
                m_table_TEC_edited.Rows[m_table_TEC_edited.Rows.Count - 1]["ID_SOURCE_ADMIN"] = "675";
                m_table_TEC_edited.Rows[m_table_TEC_edited.Rows.Count - 1]["ID_SOURCE_PBR"] = "675";
                m_table_TEC_edited.Rows[m_table_TEC_edited.Rows.Count - 1]["TABLE_NAME_ADMIN"] = table_name_admin;
                m_table_TEC_edited.Rows[m_table_TEC_edited.Rows.Count - 1]["TABLE_NAME_PBR"] = table_name_pbr;

                TEC new_tec = new TEC(id_tec, name_shr, table_name_admin, table_name_pbr, true);//Создание новой ТЭЦ

                m_list_TEC.Add(new_tec);//Добавление новой ТЭЦ в список ТЭЦ

                treeView_TECComponent.Update_tree(m_list_TEC);//Обновление дерева

                btnBreak.Enabled = true;
                btnOK.Enabled = true;
            }
        }

        /// <summary>
        /// Обработчик добавления нового компонента ТЭЦ
        /// </summary>
        private void add_New_TEC_COMP(object sender, ToolStripItemClickedEventArgs e)
        {
                int err = -1;
                int id_TEC = m_sel_node_list[0] - 1;//Идентификатор ТЭЦ

                if (e.ClickedItem.Text == (string)getNameMode(0))//Добавление новой ГТП
                {
                    m_table_GTP_edited.Rows.Add();
                    //Заполнение таблицы
                    m_table_GTP_edited.Rows[m_table_GTP_edited.Rows.Count - 1]["ID"] = DbTSQLInterface.GetIdNext(m_table_GTP_edited, out err).ToString();
                    m_table_GTP_edited.Rows[m_table_GTP_edited.Rows.Count - 1]["ID_TEC"] = m_sel_node_list[0].ToString();
                    m_table_GTP_edited.Rows[m_table_GTP_edited.Rows.Count - 1]["NAME_SHR"] = "Новая ГТП";

                    //Добавление нового компонента ГТП в ТЭЦ компоненты
                    m_list_TEC[id_TEC].list_TECComponents.Add(new TECComponent(m_list_TEC[id_TEC]));
                    int indx = m_list_TEC[id_TEC].list_TECComponents.Count - 1;
                    m_list_TEC[id_TEC].list_TECComponents[indx].name_shr = m_table_GTP_edited.Rows[m_table_GTP_edited.Rows.Count - 1]["NAME_SHR"].ToString();
                    m_list_TEC[id_TEC].list_TECComponents[indx].m_id = (int)m_table_GTP_edited.Rows[m_table_GTP_edited.Rows.Count - 1]["ID"];
                }
                else
                    if (e.ClickedItem.Text == (string)getNameMode(1))//Добавление нового ЩУ
                    {
                        m_table_PC_edited.Rows.Add();
                        //Заполнение таблицы
                        m_table_PC_edited.Rows[m_table_PC_edited.Rows.Count - 1]["ID"] = DbTSQLInterface.GetIdNext(m_table_PC_edited, out err).ToString();
                        m_table_PC_edited.Rows[m_table_PC_edited.Rows.Count - 1]["ID_TEC"] = m_sel_node_list[0].ToString();
                        m_table_PC_edited.Rows[m_table_PC_edited.Rows.Count - 1]["NAME_SHR"] = "Новый ЩУ";

                        //Добавление нового компонента ЩУ в ТЭЦ компоненты
                        m_list_TEC[id_TEC].list_TECComponents.Add(new TECComponent(m_list_TEC[id_TEC]));
                        int indx = m_list_TEC[id_TEC].list_TECComponents.Count - 1;
                        m_list_TEC[id_TEC].list_TECComponents[indx].name_shr = m_table_PC_edited.Rows[m_table_PC_edited.Rows.Count - 1]["NAME_SHR"].ToString();
                        m_list_TEC[id_TEC].list_TECComponents[indx].m_id = (int)m_table_PC_edited.Rows[m_table_PC_edited.Rows.Count - 1]["ID"];
                    }
                    else
                    {
                        if (e.ClickedItem.Text == (string)getNameMode(2))//Добавление нового ТГ
                        {
                            m_table_TG_edited.Rows.Add();
                            m_table_TG_edited.Rows[m_table_TG_edited.Rows.Count - 1]["ID"] = DbTSQLInterface.GetIdNext(m_table_TG_edited, out err).ToString();
                            m_table_TG_edited.Rows[m_table_TG_edited.Rows.Count - 1]["ID_TEC"] = m_sel_node_list[0].ToString();

                            m_table_TG_edited.Rows[m_table_TG_edited.Rows.Count - 1]["ID_GTP"] = "0";
                            m_table_TG_edited.Rows[m_table_TG_edited.Rows.Count - 1]["NAME_SHR"] = "Новый ТГ";


                            m_list_TEC[id_TEC].m_listTG.Add(new TG());
                            int indx = m_list_TEC[id_TEC].m_listTG.Count - 1;

                            new_tg.InitTG(m_list_TEC[id_TEC].m_listTG[indx], m_table_TG_edited.Rows[m_table_TG_edited.Rows.Count - 1], db_sostav.get_allParamTG(out err), out err);
                            m_list_TEC[id_TEC].m_listTG[indx].name_shr = m_table_TG_edited.Rows[m_table_TG_edited.Rows.Count - 1]["NAME_SHR"].ToString();
                            m_list_TEC[id_TEC].m_listTG[indx].m_id = (int)m_table_TG_edited.Rows[m_table_TG_edited.Rows.Count - 1]["ID"];
                            m_list_TEC[id_TEC].m_listTG[indx].m_id_owner_gtp = -1;
                            m_list_TEC[id_TEC].m_listTG[indx].m_id_owner_pc = -1;

                            m_list_TEC[id_TEC].list_TECComponents.Add(new TECComponent(m_list_TEC[id_TEC]));
                            int indx_comp = m_list_TEC[id_TEC].list_TECComponents.Count - 1;
                            m_list_TEC[id_TEC].list_TECComponents[indx_comp].m_id = m_list_TEC[id_TEC].m_listTG[indx].m_id;
                            m_list_TEC[id_TEC].list_TECComponents[indx_comp].name_shr = m_list_TEC[id_TEC].m_listTG[indx].name_shr;
                            m_list_TEC[id_TEC].list_TECComponents[indx_comp].m_listTG.Add(m_list_TEC[id_TEC].m_listTG[indx]);
                        }
                        else
                            if (e.ClickedItem.Text == (string)getNameMode(4))
                            {
                                if (treeView_TECComponent.SelectedNode.FirstNode == null)
                                {
                                    m_table_TEC_edited.Rows.Remove(m_table_TEC_edited.Select("ID=" + m_sel_node_list[0])[0]);

                                    m_list_TEC.RemoveAt(m_sel_node_list[0] - 1);

                                    btnBreak.Enabled = true;
                                    btnOK.Enabled = true;
                                }
                                else
                                    MessageBox.Show("Имеются не выведенные из состава компоненты в " + treeView_TECComponent.SelectedNode.Text);
                            }
                    }

                treeView_TECComponent.SelectedNode.ContextMenuStrip.ItemClicked -= this.add_New_TEC_COMP;
                treeView_TECComponent.Update_tree(m_list_TEC);//Обновление дерева

                btnBreak.Enabled = true;
                btnOK.Enabled = true;

        }

        /// <summary>
        /// Обработчик введения в состав ТГ
        /// </summary>
        private void add_TG_PC_GTP(object sender, ToolStripItemClickedEventArgs e)
        {
            if (treeView_TECComponent.SelectedNode.Parent.Parent.Text == (string)FormChangeMode.getNameMode(1))//Введение ТГ в состав ГТП
            {
                int id_gtp = 0;

                foreach (TreeNode t in treeView_TECComponent.SelectedNode.Parent.Parent.Nodes)
                {
                    if (e.ClickedItem.Text == t.Text)
                    {
                        List<int> id = get_m_id_list(t.Name);
                        id_gtp = id[1];
                    }
                }

                m_table_TG_edited.Select("ID=" + m_sel_node_list[3])[0]["ID_GTP"] = id_gtp;//Изменение ID ГТП у ТГ в таблице

                foreach (TECComponent tc in m_list_TEC[m_sel_node_list[0] - 1].list_TECComponents)
                {
                    if (tc.IsGTP == true & tc.m_id == id_gtp)//Является ли элемент ГТП и соответствует ли ID нашему
                    {
                        foreach (TG t in m_list_TEC[m_sel_node_list[0] - 1].m_listTG)//Перебор листа с ТГ для изменения параметра
                        {
                            if (t.m_id == m_sel_node_list[3])
                            {
                                t.m_id_owner_gtp = id_gtp;
                                tc.m_listTG.Add(t);//Добавление в лист компонентов ТЭЦ ТГ
                            }
                        }
                    }
                }
            }
            else
            {
                if (treeView_TECComponent.SelectedNode.Parent.Parent.Text == (string)FormChangeMode.getNameMode(2))//Введение ТГ в состав ЩУ
                {
                    int id_pc = 0;

                    foreach (TreeNode t in treeView_TECComponent.SelectedNode.Parent.Parent.Nodes)//Цикл для получения ID ЩУ в который помещать ТГ
                    {
                        if (e.ClickedItem.Text == t.Text)
                        {
                            List<int> id = get_m_id_list(t.Name);
                            id_pc = id[2];
                        }
                    }
                    m_table_TG_edited.Select("ID=" + m_sel_node_list[3])[0]["ID_PC"] = id_pc;//Изменение ID ЩУ у ТГ в таблице

                    foreach (TECComponent tc in m_list_TEC[m_sel_node_list[0] - 1].list_TECComponents)
                    {
                        if (tc.IsPC == true & tc.m_id == id_pc)//Является ли элемент ЩУ и соответствует ли ID нашему
                        {
                            foreach (TG t in m_list_TEC[m_sel_node_list[0] - 1].m_listTG)//Перебор листа с ТГ для изменения параметра
                            {
                                if (t.m_id == m_sel_node_list[3])
                                {
                                    t.m_id_owner_pc = id_pc;
                                    tc.m_listTG.Add(t);//Добавление в лист компонентов ТЭЦ ТГ
                                }
                            }
                        }
                    }
                }
            }
            
            (treeView_TECComponent.SelectedNode.ContextMenuStrip.Items["ввестиВСоставToolStripMenuItem"] as ToolStripDropDownItem).DropDownItemClicked -= this.add_TG_PC_GTP;

            treeView_TECComponent.Update_tree(m_list_TEC);
            btnBreak.Enabled = true;
            btnOK.Enabled = true;
        }

        /// <summary>
        /// Обработчик удаления компонента ТЭЦ (ГТП, ЩУ)
        /// </summary>
        private void del_Comp(object sender, ToolStripItemClickedEventArgs e)
        {
                int err = -1;
                if (e.ClickedItem.Text == (string)getNameMode(4))
                {
                    if (m_sel_node_list[1].Equals(-1) == false & m_sel_node_list[3].Equals(-1) == true)//TG
                    {
                        if (treeView_TECComponent.SelectedNode.FirstNode == null)
                        {
                            int id_stroki = 0;
                            m_table_GTP_edited.Rows.Remove(m_table_GTP_edited.Select("ID=" + m_sel_node_list[1])[0]);

                            for (int i = 0; i < m_list_TEC[m_sel_node_list[0] - 1].list_TECComponents.Count; i++)
                            {
                                if (m_list_TEC[m_sel_node_list[0] - 1].list_TECComponents[i].m_id == m_sel_node_list[1])
                                {
                                    id_stroki = i;
                                }
                            }

                            m_list_TEC[m_sel_node_list[0] - 1].list_TECComponents.RemoveAt(id_stroki);
                            btnBreak.Enabled = true;
                            btnOK.Enabled = true;
                        }
                        else
                            MessageBox.Show("Имеются не выведенные из состава компоненты в " + treeView_TECComponent.SelectedNode.Text);
                    }
                    if (m_sel_node_list[2].Equals(-1) == false & m_sel_node_list[3].Equals(-1) == true)//PC
                    {
                        if (treeView_TECComponent.SelectedNode.FirstNode == null)
                        {
                            int id_stroki = 0;
                            DataRow[] rows = m_table_PC_edited.Select("ID=" + m_sel_node_list[2]);
                            m_table_PC_edited.Rows.Remove(rows[0]);

                            for (int i = 0; i < m_list_TEC[m_sel_node_list[0] - 1].list_TECComponents.Count; i++)
                            {
                                if (m_list_TEC[m_sel_node_list[0] - 1].list_TECComponents[i].m_id == m_sel_node_list[2])
                                {
                                    id_stroki = i;
                                }
                            }

                            m_list_TEC[m_sel_node_list[0] - 1].list_TECComponents.RemoveAt(id_stroki);
                            btnBreak.Enabled = true;
                            btnOK.Enabled = true;
                        }
                        else
                            MessageBox.Show("Имеются не выведенные из состава компоненты в " + treeView_TECComponent.SelectedNode.Text);
                    }
                }

                treeView_TECComponent.SelectedNode.ContextMenuStrip.ItemClicked -= this.del_Comp;
                treeView_TECComponent.Update_tree(m_list_TEC);
        }

        /// <summary>
        /// Обработчик удаления компонента ТЭЦ (ТГ)
        /// </summary>
        private void del_TG(object sender, ToolStripItemClickedEventArgs e)
        {
            if (treeView_TECComponent.SelectedNode.Parent.Parent.Text == "ГТП")//Введение ТГ в состав ГТП
            {
                int id_gtp = 0;

                foreach (TreeNode t in treeView_TECComponent.SelectedNode.Parent.Parent.Nodes)
                {
                    if (e.ClickedItem.Text == t.Text)
                    {
                        List<int> id = get_m_id_list(t.Name);
                        id_gtp = id[1];
                    }
                }

                m_table_TG_edited.Select("ID=" + m_sel_node_list[3])[0]["ID_GTP"] = id_gtp;//Изменение ID ГТП у ТГ в таблице

                foreach (TECComponent tc in m_list_TEC[m_sel_node_list[0] - 1].list_TECComponents)
                {
                    if (tc.IsGTP == true & tc.m_id == id_gtp)//Является ли элемент ГТП и соответствует ли ID нашему
                    {
                        foreach (TG t in m_list_TEC[m_sel_node_list[0] - 1].m_listTG)//Перебор листа с ТГ для изменения параметра
                        {
                            if (t.m_id == m_sel_node_list[3])
                            {
                                t.m_id_owner_gtp = id_gtp;
                                tc.m_listTG.Add(t);//Добавление в лист компонентов ТЭЦ ТГ
                            }
                        }
                    }
                }

                treeView_TECComponent.Update_tree(m_list_TEC);
                btnBreak.Enabled = true;
                btnOK.Enabled = true;
            }
            else
            {

                if (treeView_TECComponent.SelectedNode.Parent.Parent.Text == "ЩУ")//Введение ТГ в состав ЩУ
                {
                    int id_pc = 0;

                    foreach (TreeNode t in treeView_TECComponent.SelectedNode.Parent.Parent.Nodes)//Цикл для получения ID ЩУ в который помещать ТГ
                    {
                        if (e.ClickedItem.Text == t.Text)
                        {
                            List<int> id = get_m_id_list(t.Name);
                            id_pc = id[2];
                        }
                    }
                    m_table_TG_edited.Select("ID=" + m_sel_node_list[3])[0]["ID_PC"] = id_pc;//Изменение ID ЩУ у ТГ в таблице

                    foreach (TECComponent tc in m_list_TEC[m_sel_node_list[0] - 1].list_TECComponents)
                    {
                        if (tc.IsPC == true & tc.m_id == id_pc)//Является ли элемент ЩУ и соответствует ли ID нашему
                        {
                            foreach (TG t in m_list_TEC[m_sel_node_list[0] - 1].m_listTG)//Перебор листа с ТГ для изменения параметра
                            {
                                if (t.m_id == m_sel_node_list[3])
                                {
                                    t.m_id_owner_pc = id_pc;
                                    tc.m_listTG.Add(t);//Добавление в лист компонентов ТЭЦ ТГ
                                }
                            }
                        }
                    }

                    treeView_TECComponent.Update_tree(m_list_TEC);
                    btnBreak.Enabled = true;
                    btnOK.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Обработчик события нажатия на элемент в TreeView
        /// </summary>
        private void tree_NodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            System.Windows.Forms.ContextMenuStrip contextMenu_TreeNode = new System.Windows.Forms.ContextMenuStrip();

            System.Windows.Forms.ToolStripMenuItem ввестиВСоставToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            System.Windows.Forms.ToolStripMenuItem вывыстиИзСоставаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            System.Windows.Forms.ToolStripMenuItem добавитьГТПToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            System.Windows.Forms.ToolStripMenuItem добавитьЩУToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            System.Windows.Forms.ToolStripMenuItem добавитьТГToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            
            #region Нажатие правой кнопкой мыши

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                treeView_TECComponent.SelectedNode = e.Node;//Выбор компонента при нажатии на него правой кнопкой мыши
                
                if (m_sel_node_list[3] != -1)//выбран ли элемент ТГ
                {
                    #region Не введенные
                    if (treeView_TECComponent.SelectedNode.Parent.Text == TreeView_TECComponent.not_add)
                    {
                        #region Context add TG from PC or GTP
                        // 
                        // contextMenu_TreeView_TG_PC
                        // 
                        contextMenu_TreeNode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                        ввестиВСоставToolStripMenuItem});
                        contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                        // 
                        // ввестиВСоставToolStripMenuItem
                        //
                        ввестиВСоставToolStripMenuItem.Name = "ввестиВСоставToolStripMenuItem";
                        ввестиВСоставToolStripMenuItem.Text = "Ввести в состав";
                        #endregion

                        treeView_TECComponent.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;

                        (treeView_TECComponent.SelectedNode.ContextMenuStrip.Items["ввестиВСоставToolStripMenuItem"] as ToolStripDropDownItem).DropDownItems.Clear();
                        for (int i = 1; i < treeView_TECComponent.SelectedNode.Parent.Parent.Nodes.Count; i++)
                        {
                            (treeView_TECComponent.SelectedNode.ContextMenuStrip.Items["ввестиВСоставToolStripMenuItem"] as ToolStripDropDownItem).DropDownItems.Add(treeView_TECComponent.SelectedNode.Parent.Parent.Nodes[i].Text);
                        }
                        (treeView_TECComponent.SelectedNode.ContextMenuStrip.Items["ввестиВСоставToolStripMenuItem"] as ToolStripDropDownItem).DropDownItems.Clear();
                        for (int i = 1; i < treeView_TECComponent.SelectedNode.Parent.Parent.Nodes.Count; i++)
                        {
                            (treeView_TECComponent.SelectedNode.ContextMenuStrip.Items["ввестиВСоставToolStripMenuItem"] as ToolStripDropDownItem).DropDownItems.Add(treeView_TECComponent.SelectedNode.Parent.Parent.Nodes[i].Text);
                        }

                        (treeView_TECComponent.SelectedNode.ContextMenuStrip.Items["ввестиВСоставToolStripMenuItem"] as ToolStripDropDownItem).DropDownItemClicked += new ToolStripItemClickedEventHandler(add_TG_PC_GTP);
                    }
                    #endregion

                    #region Введенные в состав

                    if (treeView_TECComponent.SelectedNode.Parent.Text != TreeView_TECComponent.not_add)
                    {
                        if (m_sel_node_list[3] > (int)TECComponent.ID.TG)
                        {
                            #region Context delete TG
                            // 
                            // contextMenu_TreeNode
                            // 
                            contextMenu_TreeNode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    вывыстиИзСоставаToolStripMenuItem});
                            contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                            // 
                            // вывыстиИзСоставаToolStripMenuItem
                            //
                            вывыстиИзСоставаToolStripMenuItem.Name = "вывыстиИзСоставаToolStripMenuItem";
                            вывыстиИзСоставаToolStripMenuItem.Text = "Вывести из состава";
                            #endregion

                            treeView_TECComponent.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                            treeView_TECComponent.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(this.del_TG);
                        }
                    }

                    #endregion
                }

                #region Добавление компонентов

                if (m_sel_node_list[3] == -1 & m_sel_node_list[2] == -1 & m_sel_node_list[1] == -1 & m_sel_node_list[0] != -1)//Выбрана ли ТЭЦ
                {
                    #region Добавление в ТЭЦ компонентов

                    #region Context TEC
                    // 
                    // contextMenu_TreeView_TEC
                    // 
                    contextMenu_TreeNode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    добавитьГТПToolStripMenuItem,
                    добавитьЩУToolStripMenuItem,
                    добавитьТГToolStripMenuItem,
                    вывыстиИзСоставаToolStripMenuItem});
                    contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                    // 
                    // добавитьГТПToolStripMenuItem
                    // 
                    добавитьГТПToolStripMenuItem.Name = "добавитьГТПToolStripMenuItem";
                    добавитьГТПToolStripMenuItem.Text = "Добавить ГТП";
                    // 
                    // добавитьЩУToolStripMenuItem
                    // 
                    добавитьЩУToolStripMenuItem.Name = "добавитьЩУToolStripMenuItem";
                    добавитьЩУToolStripMenuItem.Text = "Добавить ЩУ";
                    // 
                    // добавитьТГToolStripMenuItem
                    // 
                    добавитьТГToolStripMenuItem.Name = "добавитьТГToolStripMenuItem";
                    добавитьТГToolStripMenuItem.Text = "Добавить ТГ";
                    // 
                    // вывыстиИзСоставаToolStripMenuItem
                    // 
                    вывыстиИзСоставаToolStripMenuItem.Name = "вывыстиИзСоставаToolStripMenuItem";
                    вывыстиИзСоставаToolStripMenuItem.Text = "Вывести из состава";
                    #endregion

                    treeView_TECComponent.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;

                    treeView_TECComponent.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(this.add_New_TEC_COMP);

                    #endregion
                }

                if (m_sel_node_list[1] == (int)TECComponent.ID.GTP)//Выбран корень ГТП
                {
                    #region Context add GTP
                    //
                    // contextMenu_TreeNode
                    //
                    contextMenu_TreeNode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    добавитьГТПToolStripMenuItem});
                    contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                    // 
                    // добавитьГТПToolStripMenuItem
                    // 
                    добавитьГТПToolStripMenuItem.Name = "добавитьГТПToolStripMenuItem";
                    добавитьГТПToolStripMenuItem.Text = "Добавить ГТП";
                    #endregion

                    treeView_TECComponent.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                    treeView_TECComponent.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(this.add_New_TEC_COMP);
                }

                if (m_sel_node_list[2] == (int)TECComponent.ID.PC)//Выбран корень ЩУ
                {
                    #region Context add PC
                    // 
                    // contextMenu_TreeNode
                    // 
                    contextMenu_TreeNode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    добавитьЩУToolStripMenuItem});
                    contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                    // 
                    // добавитьЩУToolStripMenuItem
                    // 
                    добавитьЩУToolStripMenuItem.Name = "добавитьЩУToolStripMenuItem";
                    добавитьЩУToolStripMenuItem.Text = "Добавить ЩУ";
                    #endregion

                    treeView_TECComponent.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                    treeView_TECComponent.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(this.add_New_TEC_COMP);
                }

                if (m_sel_node_list[3] == (int)TECComponent.ID.TG)//Выбран "Поблочно"
                {
                    treeView_TECComponent.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                }

                if (treeView_TECComponent.SelectedNode.Text == TreeView_TECComponent.not_add)//Выбран "Не введенные в состав"
                {
                    treeView_TECComponent.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                }

                #endregion

                #region Удаление из состава

                if ((m_sel_node_list[1] > (int)TECComponent.ID.GTP & m_sel_node_list[1] < (int)TECComponent.ID.PC) || (m_sel_node_list[2] > (int)TECComponent.ID.PC & m_sel_node_list[2] < (int)TECComponent.ID.TG))//Выбран конкретный ЩУ или ГТП
                {
                    #region Context delete PC,GTP
                    // 
                    // contextMenu_TreeNode
                    // 
                    contextMenu_TreeNode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    вывыстиИзСоставаToolStripMenuItem});
                    contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                    // 
                    // вывыстиИзСоставаToolStripMenuItem
                    //
                    вывыстиИзСоставаToolStripMenuItem.Name = "вывыстиИзСоставаToolStripMenuItem";
                    вывыстиИзСоставаToolStripMenuItem.Text = "Вывести из состава";
                    #endregion

                    treeView_TECComponent.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                    treeView_TECComponent.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(this.del_Comp);
                }

                #endregion
            }

            #endregion
        }

        /// <summary>
        /// Метод для запроса ID компонента в TreeView
        /// </summary>
        /// <param name="id_string">Строка с идентификаторами</param>
        /// <returns>Список с ID</returns>
        private List<int> get_m_id_list(string id_string)
        {
            List<int> id_list = new List<int>(4);

            id_list.Clear();

            for (int i = 0; i < id_list.Capacity; i++)
                id_list.Add(-1);

            if (id_string != "")
            {
                string[] path = id_string.Split(':');

                for (int i = 0; i < path.Length; i++)
                {
                    if (Convert.ToInt32(path[i]) < (int)TECComponentBase.ID.GTP & Convert.ToInt32(path[i]) >= 0)
                    {
                        id_list[0] = Convert.ToInt32(path[i]);
                    }
                    if (Convert.ToInt32(path[i]) >= (int)TECComponentBase.ID.GTP & Convert.ToInt32(path[i]) < (int)TECComponentBase.ID.PC)
                    {
                        id_list[1] = Convert.ToInt32(path[i]);
                    }
                    if (Convert.ToInt32(path[i]) >= (int)TECComponentBase.ID.PC & Convert.ToInt32(path[i]) < (int)TECComponentBase.ID.TG)
                    {
                        id_list[2] = Convert.ToInt32(path[i]);
                    }
                    if (Convert.ToInt32(path[i]) >= (int)TECComponentBase.ID.TG & Convert.ToInt32(path[i]) < (int)TECComponentBase.ID.MAX)
                    {
                        id_list[3] = Convert.ToInt32(path[i]);
                    }
                }
            }
            return id_list;
        }

        /// <summary>
        /// Обработчик события выбора элемента в TreeView
        /// </summary>
        private void tree_NodeSelect(object sender, TreeViewEventArgs e)
        {
            m_sel_node_list.Clear();

            m_sel_node_list = get_m_id_list(e.Node.Name);

            if (m_sel_node_list[3].Equals(-1) == false)
            {
                dgvProp.update_dgv(m_sel_node_list[3], m_table_TG_edited);
            }

            if (m_sel_node_list[3].Equals(-1) == true & m_sel_node_list[2].Equals(-1) == false)
            {
                dgvProp.update_dgv(m_sel_node_list[2], m_table_PC_edited);
            }

            if (m_sel_node_list[3].Equals(-1) == true & m_sel_node_list[1].Equals(-1) == false)
            {
                dgvProp.update_dgv(m_sel_node_list[1], m_table_GTP_edited);
            }

            if (m_sel_node_list[2].Equals(-1) == true & m_sel_node_list[3].Equals(-1) == true & m_sel_node_list[1].Equals(-1) == true & m_sel_node_list[0].Equals(-1) == false)
            {
                dgvProp.update_dgv(m_sel_node_list[0], m_table_TEC_edited);
            }
        }

        /// <summary>
        /// Обработчик события кнопки "Применить"
        /// </summary>
        private void buttonOK_click(object sender, MouseEventArgs e)
        {
                int err = -1;
                db_sostav.Edit("TG_LIST", "ID", m_table_TG_original, m_table_TG_edited, out err);
                db_sostav.Edit("PC_LIST", "ID_TEC,ID", m_table_PC_original, m_table_PC_edited, out err);
                db_sostav.Edit("GTP_LIST", "ID", m_table_GTP_original, m_table_GTP_edited, out err);
                db_sostav.Edit("TEC_LIST", "ID", m_table_TEC_original, m_table_TEC_edited, out err);

                //fill_DataTable_ComponentsTEC();

                m_list_TEC = db_sostav.get_list_tec();

                treeView_TECComponent.Update_tree(m_list_TEC);
                btnOK.Enabled = false;
                btnBreak.Enabled = false;
        }

        /// <summary>
        /// Обработчик события кнопки "Отмена"
        /// </summary>
        private void buttonBreak_click(object sender, MouseEventArgs e)
        {
            reset_DataTable_ComponentsTEC();

            m_list_TEC = db_sostav.get_list_tec();

            treeView_TECComponent.Update_tree(m_list_TEC);
            btnOK.Enabled = false;
            btnBreak.Enabled = false;
        }

        /// <summary>
        /// Обработчик события окончания изменения ячейки
        /// </summary>
        private void dgvProp_EndCellEdit(object sender, DataGridView_Prop.DataGridView_Prop_ValuesCellValueChangedEventArgs e)
        {
            if (e.m_IdComp < (int)TECComponentBase.ID.GTP & e.m_IdComp > 0)
            {
                edit_table(e.m_IdComp, e.m_Header_name, e.m_Value, m_table_TEC_edited);
            }
            if (e.m_IdComp > (int)TECComponentBase.ID.GTP & e.m_IdComp < (int)TECComponentBase.ID.PC)
            {
                edit_table(e.m_IdComp, e.m_Header_name, e.m_Value, m_table_GTP_edited);
            }
            if (e.m_IdComp > (int)TECComponentBase.ID.PC & e.m_IdComp < (int)TECComponentBase.ID.TG)
            {
                edit_table(e.m_IdComp, e.m_Header_name, e.m_Value, m_table_PC_edited);
            }
            if (e.m_IdComp > (int)TECComponentBase.ID.TG & e.m_IdComp < (int)TECComponentBase.ID.MAX)
            {
                edit_table(e.m_IdComp, e.m_Header_name, e.m_Value, m_table_TG_edited);
            }
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

                                if (table_edit.Columns[b].ColumnName.ToString() == "NAME_SHR")
                                {
                                    if (!(m_list_TEC == null))
                                        
                                        foreach (StatisticCommon.TEC t in m_list_TEC)
                                        {
                                            if (t.m_id == id_comp)
                                                t.name_shr = value;
                                            
                                            foreach (StatisticCommon.TECComponent g in t.list_TECComponents)
                                            {
                                                if (g.m_id == id_comp)
                                                    g.name_shr = value;
                                                foreach (StatisticCommon.TG d in g.m_listTG)
                                                {
                                                    if (d.m_id == id_comp)
                                                        d.name_shr = value;
                                                }
                                            }
                                        }
                                    treeView_TECComponent.Update_tree(m_list_TEC);
                                }

                                btnOK.Enabled = true;
                                btnBreak.Enabled = true;
                            }
                        }
                    }
                }
            }
        }
    }

    public class DB_Sostav_TEC
    {
        /// <summary>
        /// Конструктор - основной
        /// </summary>
        public DB_Sostav_TEC()
            : base()
        {
            
        }

        public List<TEC> get_list_tec()
        {
            int err = -1;

            int idListener = register_idListenerMainDB(out err);

            List<TEC> TEC = new InitTEC_200(idListener, true, false).tec;

            foreach (StatisticCommon.TEC t in TEC)
            {
                t.InitSensorsTEC();
            }
            
            unregister_idListenerMainDB(idListener);

            return TEC;
        }

        /// <summary>
        /// Метод выполнения запроса
        /// </summary>
        /// <param name="query">Текст запроса</param>
        /// <returns>Возвращает таблицу с результатом</returns>
        public DataTable Request(string query)
        {
            int iRes = -1;
            int err = -1;
            DataTable table = new DataTable();

            int idListener = register_idListenerMainDB(out err);

            DbConnection connConfigDB = DbSources.Sources().GetConnection(idListener, out err);

            table = DbTSQLInterface.Select(ref connConfigDB, query, null, null, out iRes);

            unregister_idListenerMainDB(idListener);

            return table;
        }
        
        public DataTable get_allParamTG(out int err)
        {
            int idListener = register_idListenerMainDB(out err);

            DbConnection dbConn = DbSources.Sources().GetConnection(idListener, out err);

            DataTable allParamTG = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM [dbo].[ft_ALL_PARAM_TG_KKS] (0)", null, null, out err);

            unregister_idListenerMainDB(idListener);

            return allParamTG;
        }

        /// <summary>
        /// Запись изменений в базу
        /// </summary>
        /// <param name="nameTable">Наименование таблицы в БД</param>
        /// <param name="keyField">Поле "якорь" для сравнения таблиц</param>
        /// <param name="table_origin">Оригинальная таблица</param>
        /// <param name="table_edit">Измененная таблица</param>
        /// <param name="err">Возвращаемая ошибка</param>
        public void Edit(string nameTable, string keyField, DataTable table_origin, DataTable table_edit, out int err)
        {
            err = -1;
            int idListener = register_idListenerMainDB(out err);

            DbConnection dbConn = DbSources.Sources().GetConnection(idListener, out err);

            DbTSQLInterface.RecUpdateInsertDelete(ref dbConn,nameTable,keyField,table_origin,table_edit,out err);

            unregister_idListenerMainDB(idListener);
        }

        /// <summary>
        /// Получение последней ревизии аудита
        /// </summary>
        /// <returns>Последняя ревизия аудита</returns>
        public int Get_LastRevision_Audit()
        {
            int err = -1;
            int rev = -1;
            rev = DbTSQLInterface.GetIdNext(select_table_audit(), out err, "REV");
            return rev;
        }

        /// <summary>
        /// Метод для получения из ДБ таблицы Audit
        /// </summary>
        /// <returns>Возвращает таблицу</returns>
        private DataTable select_table_audit()
        {
            int iRes = -1;
            int err = -1;
            DataTable prev_audit = new DataTable();
            string query = "SELECT [ID],[DATETIME_WR],[ID_USER],[ID_ITEM],[DESCRIPTION],[PREV_VAL],[NEW_VAL],[REV] FROM [dbo].[audit]";
            int idListener = register_idListenerMainDB(out err);
            DbConnection connConfigDB = DbSources.Sources().GetConnection(idListener, out err);
            prev_audit = DbTSQLInterface.Select(ref connConfigDB, query, null, null, out iRes);
            unregister_idListenerMainDB(idListener);
            return prev_audit;
        }

        /// <summary>
        /// Запись в БД таблицы Audit
        /// </summary>
        /// <param name="table_audit">Таблица со списком новых изменений</param>
        public void Write_Audit(DataTable table_audit)
        {
            int err = -1;
            try
            {
                for (int i = 0; i < table_audit.Rows.Count; i++)
                {
                    table_audit.Rows[i]["DATETIME_WR"] = HDateTime.ToMoscowTimeZone(DateTime.Now);
                    table_audit.Rows[i]["ID_USER"] = HUsers.Id;
                    table_audit.Rows[i]["REV"] = Get_LastRevision_Audit();
                }

                Edit("audit", "ID", select_table_audit(), table_audit, out err);
            }
            catch (Exception E)
            {
                Logging.Logg().Exception(E, "Ошибка записи в таблицу audit PanelTECComponent : DB_Sostav_TEC : Write_Audit - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        /// <summary>
        /// Регистрация ID
        /// </summary>
        /// <param name="err">Ошибка в процессе регистрации</param>
        /// <returns>Возвращает ID</returns>
        protected int register_idListenerMainDB(out int err)
        {
            err = -1;
            int idListener = -1;

            try
            {
            ConnectionSettings connSett = FormMainBaseWithStatusStrip.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett();
            
            idListener = DbSources.Sources().Register(connSett, false, CONN_SETT_TYPE.CONFIG_DB.ToString());

            err = 0;
            }
            catch (Exception E)
            {
                Logging.Logg().Exception(E, "Ошибка получения idListener PanelTECComponent : DB_Sostav_TEC : register_idListenerMainDB - ..." + "err = "+err.ToString(), Logging.INDEX_MESSAGE.NOT_SET);
            }
            
            return idListener;
        }

        /// <summary>
        /// Отмена регистрации ID
        /// </summary>
        /// <param name="idListener">ID</param>
        protected void unregister_idListenerMainDB(int idListener)
        {
            DbSources.Sources().UnRegister(idListener);
        }
    }

    public class DataGridView_Prop : DataGridView
    {
        private void InitializeComponent()
            {
                this.Columns.Add("Значение", "Значение");
                this.ColumnHeadersVisible = true;
                this.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                this.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                this.RowHeadersVisible = true;
            }

        public DataGridView_Prop() : base()
            {
                InitializeComponent();//инициализация компонентов

                this.CellEndEdit += new DataGridViewCellEventHandler(this.cell_EndEdit);
            }
        
        /// <summary>
        /// Запрос на получение таблицы со свойствами
        /// </summary>
        /// <param name="id_list">Лист с идентификаторами компонентов</param>
        public void update_dgv(int id_component, DataTable table)
        {
            this.Rows.Clear();
            DataRow[] rowsSel = table.Select(@"ID=" + id_component);

            if (rowsSel.Length == 1)
                foreach (DataColumn col in table.Columns)
                {
                    this.Rows.Add(rowsSel[0][col.ColumnName]);
                    this.Rows[this.Rows.Count - 1].HeaderCell.Value = col.ColumnName;
                }
            else
                Logging.Logg().Error(@"Ошибка....", Logging.INDEX_MESSAGE.NOT_SET);

            cell_ID_Edit_ReadOnly();
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
                    n_row=(int)this.Rows[i].Cells[0].Value;
                }
            }
            EventCellValueChanged(this, new DataGridView_Prop.DataGridView_Prop_ValuesCellValueChangedEventArgs(n_row//Идентификатор компонента
                                , Rows[e.RowIndex].HeaderCell.Value.ToString() //Идентификатор компонента
                                , Rows[e.RowIndex].Cells[0].Value.ToString() //Идентификатор параметра с учетом периода расчета
                                ));
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

    public class TreeView_TECComponent : TreeView
    {
        public static string not_add = "Не введенные в состав";

        List<StatisticCommon.TEC> m_listTEC;
        List<StatisticCommon.TG> m_listTG = new List<TG>();
        List<StatisticCommon.TECComponent> m_listTECComponent = new List<TECComponent>();
        private System.Windows.Forms.ContextMenuStrip contextMenu_TreeView;

        private void InitializeComponent()
        {
            System.Windows.Forms.ToolStripMenuItem добавитьТЭЦToolStripMenuItem;

            contextMenu_TreeView = new System.Windows.Forms.ContextMenuStrip();
            добавитьТЭЦToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Dock = DockStyle.Fill;

            #region Context add TEC
            // 
            // contextMenu_TreeView
            // 
            this.contextMenu_TreeView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            добавитьТЭЦToolStripMenuItem});
            this.contextMenu_TreeView.Name = "contextMenu_TreeView";
            // 
            // добавитьТЭЦToolStripMenuItem
            // 
            добавитьТЭЦToolStripMenuItem.Name = "добавитьТЭЦToolStripMenuItem";
            добавитьТЭЦToolStripMenuItem.Text = "Добавить ТЭЦ";
            #endregion

        }

        public TreeView_TECComponent()
            : base()
        {
            InitializeComponent();

            this.ContextMenuStrip = contextMenu_TreeView;
            ContextMenuStrip.Items["добавитьТЭЦToolStripMenuItem"].Visible = true;
        }
        
        /// <summary>
        /// Заполнение TreeView компонентами
        /// </summary>
        public void Update_tree(List<StatisticCommon.TEC> tec)
        {
            m_listTEC = new List<TEC>(tec);

            this.Nodes.Clear();
            int tec_indx = -1;

            if (!(m_listTEC == null))
                foreach (StatisticCommon.TEC t in m_listTEC)
                {
                    string path = null;

                    tec_indx++;

                    this.Nodes.Add(t.name_shr);//добавление ТЭЦ в TreeView

                    path += t.m_id.ToString();
                    this.Nodes[tec_indx].Name = path;

                    if (t.list_TECComponents.Count > 0)
                    {
                        int gtp_indx = 0,
                            pc_indx = 0,
                            node_indx = -1;

                        for (int i = (int)FormChangeMode.MODE_TECCOMPONENT.GTP; i < (int)FormChangeMode.MODE_TECCOMPONENT.UNKNOWN; i++)
                        {
                           this.Nodes[tec_indx].Nodes.Add(FormChangeMode.getNameMode((short)i));
                        }

                        this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Name = path + ':' + Convert.ToString((int)TECComponent.ID.GTP);
                        this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.TG - 1].Name = path + ':' + Convert.ToString((int)TECComponent.ID.TG);
                        this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Name = path + ':' + Convert.ToString((int)TECComponent.ID.PC);

                        this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes.Add(not_add);
                        this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes.Add(not_add);

                        int gtp_node_null_indx = -1;

                        int pc_node_null_indx = -1;

                        foreach (StatisticCommon.TECComponent g in t.list_TECComponents)
                        {
                            if (g.IsTG == true)
                            {

                                string tg_path = path;
                                this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.TG - 1].Nodes.Add(g.name_shr);//Добавление компонента в TreeView
                                node_indx++;
                                tg_path += ':' + g.m_id.ToString();
                                this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.TG - 1].Nodes[node_indx].Name = tg_path;
                                
                                if (g.m_listTG[0].m_id_owner_pc == -1)
                                {
                                    string pc_tg_path = path;

                                    pc_node_null_indx++;
                                    this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes[0].Nodes.Add(g.m_listTG[0].name_shr);
                                    pc_tg_path += ":" + g.m_listTG[0].m_id.ToString();
                                    this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes[0].Nodes[pc_node_null_indx].Name = pc_tg_path;
                                }
                                if (g.m_listTG[0].m_id_owner_gtp == -1)
                                {
                                    string gtp_tg_path = path;

                                    gtp_node_null_indx++;
                                    this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes[0].Nodes.Add(g.m_listTG[0].name_shr);
                                    gtp_tg_path += ":" + g.m_listTG[0].m_id.ToString();
                                    this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes[0].Nodes[gtp_node_null_indx].Name = gtp_tg_path;
                                }

                            }
                            else
                                ;

                            if (g.IsGTP == true)
                            {
                                string gtp_path = path;
                                int gtp_node_indx = -1;
                                this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes.Add(g.name_shr);//Добавление компонента в TreeView
                                
                                gtp_indx++;
                                gtp_path += ':' + g.m_id.ToString();
                                this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes[gtp_indx].Name = gtp_path;

                                foreach (StatisticCommon.TG h in g.m_listTG)
                                {
                                    string gtp_tg_path = gtp_path;

                                    if (h.m_id_owner_gtp == g.m_id)
                                    {
                                        gtp_node_indx++;
                                        this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes[gtp_indx].Nodes.Add(h.name_shr);
                                        gtp_tg_path += ':' + h.m_id.ToString();
                                        this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes[gtp_indx].Nodes[gtp_node_indx].Name = gtp_tg_path;
                                    }
                                    else ;
                                }
                            }
                            else
                                ;

                            if (g.IsPC == true)
                            {
                                string pc_path = path;

                                int pc_node_indx = -1;

                                this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes.Add(g.name_shr);//Добавление компонента в TreeView
                                pc_indx++;
                                pc_path += ':' + g.m_id.ToString();
                                this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes[pc_indx].Name = pc_path;

                                foreach (StatisticCommon.TG h in g.m_listTG)
                                {
                                    string pc_tg_path = pc_path;

                                    if (h.m_id_owner_pc == g.m_id)
                                    {
                                        pc_node_indx++;
                                        this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes[pc_indx].Nodes.Add(h.name_shr);
                                        pc_tg_path += ':' + h.m_id.ToString();
                                        this.Nodes[tec_indx].Nodes[(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes[pc_indx].Nodes[pc_node_indx].Name = pc_tg_path;
                                    }
                                    else ;
                                }
                            }
                            else
                                ;
                        }
                    }
                }
        }

    }
}
