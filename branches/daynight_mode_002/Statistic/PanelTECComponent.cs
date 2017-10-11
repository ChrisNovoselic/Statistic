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
            //System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTECComponent));
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

        DB_Sostav_TEC db_sostav = new DB_Sostav_TEC();

        /// <summary>
        /// Возвратить наименование компонента контекстного меню
        /// </summary>
        /// <param name="indx">Индекс режима</param>
        /// <returns>Строка - наименование режима</returns>
        protected static string getNameMode(Int16 indx)
        {
            string[] nameModes = { "Добавить ГТП", "Добавить ЩУ", "Добавить ТГ", "Ввести в состав", "Вывести из состава", "Добавить ТЭЦ"};

            return nameModes[indx];
        }

        protected DataTable[] m_arr_originalTable
            , m_arr_editTable;

        #endregion

        public PanelTECComponent(List<StatisticCommon.TEC> tec)
            : base(MODE_UPDATE_VALUES.AUTO)
        {
            InitializeComponent();

            m_arr_originalTable = new DataTable[(int)FormChangeMode.MODE_TECCOMPONENT.ANY];
            m_arr_editTable = new DataTable[(int)FormChangeMode.MODE_TECCOMPONENT.ANY];
            fill_DataTable_ComponentsTEC();
            
            treeView_TECComponent.GetID += new TreeView_TECComponent.intGetID(this.getNextID);
            treeView_TECComponent.EditNode += new TreeView_TECComponent.EditNodeEventHandler(this.get_operation_tree);
            treeView_TECComponent.Report += new TreeView_TECComponent.ReportEventHandler(this.tree_report);
        }

        /// <summary>
        /// Активировать/деактивировать панель
        /// </summary>
        /// <param name="active">Признак активации/деактивации</param>
        /// <returns>Признак применения указанного в аргументе признака</returns>
        public override bool Activate(bool activated)
        {
            bool bRes = base.Activate(activated);

            if (activated == false)
            {
                delegateReportClear(true);
            }
            else
                ;

            return bRes;
        }

        /// <summary>
        /// Обработчик события получения сообщения от TreeView
        /// </summary>
        private void tree_report(object sender, TreeView_TECComponent.ReportEventArgs e)
        {
            if(e.Action!=string.Empty)
                delegateActionReport(e.Action);
            if (e.Warning != string.Empty)
                delegateWarningReport(e.Warning);
            if (e.Error != string.Empty)
                delegateErrorReport(e.Error);
            if (e.Clear != false)
                delegateReportClear(e.Clear);
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
            for(int i = (int)FormChangeMode.MODE_TECCOMPONENT.TEC; i < (int)FormChangeMode.MODE_TECCOMPONENT.ANY; i++)
            {
                m_arr_originalTable[i]=db_sostav.GetTableCompTEC(i);
            }

            reset_DataTable_ComponentsTEC();
        }

        /// <summary>
        /// Метод для сброса отредактированных параметров
        /// </summary>
        private void reset_DataTable_ComponentsTEC()
        {
            int i = 0;
            foreach (DataTable table in m_arr_originalTable)
            {
                m_arr_editTable[i] = table.Copy();
                i++;
            }
        }

        /// <summary>
        /// Обработчик получения данных от TreeView
        /// </summary>
        private void get_operation_tree(object sender, TreeView_TECComponent.EditNodeEventArgs e)
        {
            if (e.m_Operation == TreeView_TECComponent.ID_Operation.Select)
            {
                select(e.m_IdComp);
            }
            if (e.m_Operation == TreeView_TECComponent.ID_Operation.Delete)
            {
                delete(e.m_IdComp);
            }
            if (e.m_Operation == TreeView_TECComponent.ID_Operation.Insert)
            {
                insert(e.m_IdComp);
            }
            if (e.m_Operation == TreeView_TECComponent.ID_Operation.Update)
            {
                update(e.m_IdComp, e.m_Value);
            }
        }

        /// <summary>
        /// Метод удаления компонента из таблицы
        /// </summary>
        /// <param name="list_id">Список идентификаторов объекта</param>
        private void delete(TreeView_TECComponent.ID_Comp list_id)
        {
            int iRes = 0;
            
            if (list_id.id_tg.Equals(-1) == false)
            {
                m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Rows.Remove(m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Select("ID=" + list_id.id_tg)[0]);
                iRes = 1;
            }

            if (list_id.id_tg.Equals(-1) == true & list_id.id_pc.Equals(-1) == false)
            {
                m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Rows.Remove(m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Select("ID=" + list_id.id_pc)[0]);
                iRes = 1;
            }

            if (list_id.id_tg.Equals(-1) == true & list_id.id_gtp.Equals(-1) == false)
            {
                m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Rows.Remove(m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Select("ID=" + list_id.id_gtp)[0]);
                
                iRes = 1;
            }

            if (list_id.id_pc.Equals(-1) == true & list_id.id_tg.Equals(-1) == true & list_id.id_gtp.Equals(-1) == true & list_id.id_tec.Equals(-1) == false)
            {
                m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Rows.Remove(m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Select("ID=" + list_id.id_tec)[0]);
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
        private void update(TreeView_TECComponent.ID_Comp list_id, string type_op)
        {
            string type = type_op;
            int iRes = 0;
            if (list_id.id_tg.Equals(-1) == false)
            {
                if (type == FormChangeMode.getNameMode((int)FormChangeMode.MODE_TECCOMPONENT.GTP))
                {
                    m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Select("ID=" + list_id.id_tg)[0]["ID_GTP"] = list_id.id_gtp;
                    iRes = 1;
                }
                if (type == FormChangeMode.getNameMode(2))
                {
                    if (list_id.id_pc== -1)
                    {
                        m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Select("ID=" + list_id.id_tg)[0]["ID_PC"] = 0;
                    }
                    else
                    {
                        m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Select("ID=" + list_id.id_tg)[0]["ID_PC"] = list_id.id_pc;
                    }
                    iRes = 1;
                }
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
        private void insert(TreeView_TECComponent.ID_Comp list_id)
        {
            int iRes = 0;
            if (list_id.id_tg.Equals(-1) == false)//Добавление нового ТГ
            {
                object[] obj = new object[m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Columns.Count];

                for (int i = 0; i < m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Columns.Count; i++)
                {
                    if (m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Columns[i].ColumnName == "NAME_SHR")
                    {
                        obj[i] = TreeView_TECComponent.Mass_NewVal_Comp((int)FormChangeMode.MODE_TECCOMPONENT.TG);
                    }
                    else
                        if (m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Columns[i].ColumnName == "ID_PC")
                        {
                            obj[i] = 0;
                        }
                        else
                            if (m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Columns[i].ColumnName == "ID")
                            {
                                obj[i] = list_id.id_tg;
                            }
                            else
                                if (m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Columns[i].ColumnName == "ID_TEC")
                                {
                                    obj[i] = list_id.id_tec;
                                }
                                else
                                    obj[i] = -1;
                }

                m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Rows.Add(obj);
                iRes = 1;
            }

            if (list_id.id_tg.Equals(-1) == true & list_id.id_pc.Equals(-1) == false)//Добавление нового ЩУ
            {
                object[] obj = new object[m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Columns.Count];

                for (int i = 0; i < m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Columns.Count; i++)
                {
                    if (m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Columns[i].ColumnName == "NAME_SHR")
                    {
                        obj[i] = "Новый ЩУ";
                    }
                    else
                        if (m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Columns[i].ColumnName == "ID_TEC")
                        {
                            obj[i] = list_id.id_tec;
                        }
                        else
                            if (m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Columns[i].ColumnName == "ID")
                            {
                                obj[i] = list_id.id_pc;
                            }
                            else
                            obj[i] = -1;
                }

                m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Rows.Add(obj);
                iRes = 1;
            }

            if (list_id.id_tg.Equals(-1) == true & list_id.id_gtp.Equals(-1) == false)//Добавление новой ГТП
            {
                object[] obj = new object[m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Columns.Count];

                for (int i = 0; i < m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Columns.Count; i++)
                {
                    if (m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Columns[i].ColumnName == "NAME_SHR")
                    {
                        obj[i] = "Новая ГТП";
                    }
                    else
                        if (m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Columns[i].ColumnName == "ID_TEC")
                        {
                            obj[i] = list_id.id_tec;
                        }
                        else
                            if (m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Columns[i].ColumnName == "ID")
                            {
                                obj[i] = list_id.id_gtp;
                            }
                            else
                                obj[i] = -1;
                }

                m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Rows.Add(obj);
                iRes = 1;
            }

            //Добавление новой ТЭЦ
            if (list_id.id_pc.Equals(-1) == true & list_id.id_tg.Equals(-1) == true & list_id.id_gtp.Equals(-1) == true & list_id.id_tec.Equals(-1) == false)
            {

                object[] obj = new object[m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Columns.Count];

                for (int i = 0; i < m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Columns.Count; i++)
                {
                    if (m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Columns[i].ColumnName == "NAME_SHR")
                    {
                        obj[i] = "Новая ТЭЦ";
                    }
                    else
                        if (m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Columns[i].ColumnName == "ID")
                        {
                            obj[i] = list_id.id_tec;
                        }
                        else
                            if (m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Columns[i].ColumnName == "InUse")
                            {
                                obj[i] = 0;
                            }
                            else
                            obj[i] = -1;
                }

                m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Rows.Add(obj);
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
        private void select(TreeView_TECComponent.ID_Comp list_id)
        {

            if (list_id.id_tg.Equals(-1) == false)
            {
                dgvProp.update_dgv(list_id.id_tg, m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG]);
            }

            if (list_id.id_tg.Equals(-1) == true & list_id.id_pc.Equals(-1) == false)
            {
                dgvProp.update_dgv(list_id.id_pc, m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.PC]);
            }

            if (list_id.id_tg.Equals(-1) == true & list_id.id_gtp.Equals(-1) == false)
            {
                dgvProp.update_dgv(list_id.id_gtp, m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.GTP]);
            }

            if (list_id.id_pc.Equals(-1) == true & list_id.id_tg.Equals(-1) == true & list_id.id_gtp.Equals(-1) == true & list_id.id_tec.Equals(-1) == false)
            {
                dgvProp.update_dgv(list_id.id_tec,m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TEC]);
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

            if (validate_saving(m_arr_editTable, out warning) == false)
            {
                for (int i = (int)FormChangeMode.MODE_TECCOMPONENT.TEC; i < (int)FormChangeMode.MODE_TECCOMPONENT.ANY; i++)
                {
                    if (i == (int)FormChangeMode.MODE_TECCOMPONENT.PC)
                        db_sostav.Edit(FormChangeMode.getPrefixMode((short)i) + "_LIST", "ID_TEC,ID", m_arr_originalTable[i], m_arr_editTable[i], out err);
                    else
                        db_sostav.Edit(FormChangeMode.getPrefixMode((short)i) + "_LIST", "ID", m_arr_originalTable[i], m_arr_editTable[i], out err);

                }

                fill_DataTable_ComponentsTEC();
                treeView_TECComponent.Update_tree();
                btnOK.Enabled = false;
                btnBreak.Enabled = false;

                if (err == 0)
                    FormMain.formParameters.IncIGOVersion();//Повышение версии состава ТЭЦ
            }
            else
            {
                delegateWarningReport ( warning[0] + warning[1] + warning[2] + warning[3]);
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
            reset_DataTable_ComponentsTEC();

            treeView_TECComponent.Update_tree();
            btnOK.Enabled = false;
            btnBreak.Enabled = false;
        }

        /// <summary>
        /// Обработчик события окончания изменения ячейки
        /// </summary>
        private void dgvProp_EndCellEdit(object sender, DataGridView_Prop.DataGridView_Prop_ValuesCellValueChangedEventArgs e)
        {
            delegateReportClear(true);
            if (e.m_IdComp < (int)TECComponentBase.ID.GTP & e.m_IdComp > 0)
            {
                edit_table(e.m_IdComp, e.m_Header_name, e.m_Value, m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TEC]);
            }
            if (e.m_IdComp > (int)TECComponentBase.ID.GTP & e.m_IdComp < (int)TECComponentBase.ID.PC)
            {
                edit_table(e.m_IdComp, e.m_Header_name, e.m_Value, m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.GTP]);
            }
            if (e.m_IdComp > (int)TECComponentBase.ID.PC & e.m_IdComp < (int)TECComponentBase.ID.TG)
            {
                edit_table(e.m_IdComp, e.m_Header_name, e.m_Value, m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.PC]);
            }
            if (e.m_IdComp > (int)TECComponentBase.ID.TG & e.m_IdComp < (int)TECComponentBase.ID.MAX)
            {
                edit_table(e.m_IdComp, e.m_Header_name, e.m_Value, m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG]);
            }
            if (e.m_Header_name == "NAME_SHR")
            {
                treeView_TECComponent.Rename_Node(e.m_IdComp, e.m_Value);
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

                                btnOK.Enabled = true;
                                btnBreak.Enabled = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Проверка критичных параметров перед сохранением
        /// </summary>
        /// <param name="mass_table">Таблица для проверки</param>
        /// <param name="warning">Строка с опмсанием ошибки</param>
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
                            warning[indx] += "Для объекта " + row["NAME_SHR"] + " параметр " + table.Columns[i].ColumnName + " равен '-1'." + '\n';
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
        private int getNextID(object sender, TreeView_TECComponent.GetIDEventArgs e)
        {
            int ID = 0;
            int err = 0;

            if (e.m_IdComp < (int)TECComponent.ID.GTP)
            {
                ID = DbTSQLInterface.GetIdNext(m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TEC], out err);
            }
            if (e.m_IdComp == (int)TECComponent.ID.GTP)
            {
                ID = DbTSQLInterface.GetIdNext(m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.GTP], out err);
            }
            if (e.m_IdComp == (int)TECComponent.ID.PC)
            {
                ID = DbTSQLInterface.GetIdNext(m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.PC], out err);
            }
            if (e.m_IdComp == (int)TECComponent.ID.TG)
            {
                ID = DbTSQLInterface.GetIdNext(m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG], out err);
            }

            return ID;
        }
        
        /// <summary>
        /// Метод формирования таблицы аудита
        /// </summary>
        /// <param name="type_operation">Тип операции</param>
        /// <param name="id_object">ИД объекта</param>
        /// <param name="new_value">Новое значение</param>
        /// <param name="prev_value">Предыдущее значение</param>
        private void set_audit(string type_operation, int id_object, string new_value, string prev_value)
        {

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

                treeView_TECComponent.BackColor = value == SystemColors.Control ? SystemColors.Window : value;
            }
        }

        private class DB_Sostav_TEC {
            /// <summary>
            /// Конструктор - основной
            /// </summary>
            public DB_Sostav_TEC ()
                : base ()
            {
            }

            /// <summary>
            /// Формирование ТЭЦ-листа
            /// </summary>
            /// <returns>Возвращает ТЭЦ-лист</returns>
            public List<TEC> get_list_tec ()
            {
                int err = -1;

                int idListener = register_idListenerMainDB (out err);

                List<TEC> TEC = new InitTEC_200 (idListener, true, new int [] { 0, (int)TECComponent.ID.GTP }, false).tec;

                foreach (StatisticCommon.TEC t in TEC) {
                    t.InitSensorsTEC ();//Инициализация ТГ листа в ТЭЦ
                }

                unregister_idListenerMainDB (idListener);

                return TEC;
            }

            /// <summary>
            /// Метод выполнения запроса
            /// </summary>
            /// <param name="query">Текст запроса</param>
            /// <returns>Возвращает таблицу с результатом</returns>
            public DataTable Request (string query)
            {
                int iRes = -1;
                int err = -1;
                DataTable table = new DataTable ();

                int idListener = register_idListenerMainDB (out err);

                DbConnection connConfigDB = DbSources.Sources ().GetConnection (idListener, out err);

                table = DbTSQLInterface.Select (ref connConfigDB, query, null, null, out iRes);

                unregister_idListenerMainDB (idListener);

                return table;
            }

            /// <summary>
            /// Получение параметров ТГ
            /// </summary>
            /// <param name="err"></param>
            /// <returns>Возвращает таблицу с параметрами</returns>
            public DataTable Get_allParamTG (out int err)
            {
                int idListener = register_idListenerMainDB (out err);

                DbConnection dbConn = DbSources.Sources ().GetConnection (idListener, out err);

                DataTable allParamTG = DbTSQLInterface.Select (ref dbConn, @"SELECT * FROM [dbo].[ft_ALL_PARAM_TG_KKS] (0)", null, null, out err);

                unregister_idListenerMainDB (idListener);

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
            public void Edit (string nameTable, string keyField, DataTable table_origin, DataTable table_edit, out int err)
            {
                err = -1;
                int idListener = register_idListenerMainDB (out err);

                DbConnection dbConn = DbSources.Sources ().GetConnection (idListener, out err);

                DbTSQLInterface.RecUpdateInsertDelete (ref dbConn, nameTable, keyField, string.Empty, table_origin, table_edit, out err);

                unregister_idListenerMainDB (idListener);
            }

            /// <summary>
            /// Получение таблицы с компонентами ТЭЦ
            /// </summary>
            /// <param name="id_TecComp">Идентификатор типа компонента</param>
            /// <returns>Таблицу для компонента</returns>
            public DataTable GetTableCompTEC (int id_TecComp)
            {
                DataTable tableComp = new DataTable ();

                string query = string.Empty;

                query = @"SELECT * FROM [dbo].[" + FormChangeMode.getPrefixMode (id_TecComp) + "_LIST]";

                tableComp = Request (query);

                return tableComp;
            }

            #region Audit

            /// <summary>
            /// Получение последней ревизии аудита
            /// </summary>
            /// <returns>Последняя ревизия аудита</returns>
            public int Get_LastRevision_Audit ()
            {
                int err = -1;
                int rev = -1;
                rev = DbTSQLInterface.GetIdNext (select_table_audit (), out err, "REV");
                return rev;
            }

            /// <summary>
            /// Метод для получения из ДБ таблицы Audit
            /// </summary>
            /// <returns>Возвращает таблицу</returns>
            public DataTable select_table_audit ()
            {
                int iRes = -1;
                int err = -1;
                DataTable prev_audit = new DataTable ();
                string query = "SELECT [ID],[DATETIME_WR],[ID_USER],[ID_ITEM],[DESCRIPTION],[PREV_VAL],[NEW_VAL],[REV] FROM [dbo].[audit]";
                int idListener = register_idListenerMainDB (out err);
                DbConnection connConfigDB = DbSources.Sources ().GetConnection (idListener, out err);
                prev_audit = DbTSQLInterface.Select (ref connConfigDB, query, null, null, out iRes);
                unregister_idListenerMainDB (idListener);
                return prev_audit;
            }

            /// <summary>
            /// Запись в БД таблицы Audit
            /// </summary>
            /// <param name="table_audit">Таблица со списком новых изменений</param>
            public void Write_Audit (DataTable table_audit)
            {
                int err = -1;
                try {
                    for (int i = 0; i < table_audit.Rows.Count; i++) {
                        table_audit.Rows [i] ["DATETIME_WR"] = HDateTime.ToMoscowTimeZone (DateTime.Now);
                        table_audit.Rows [i] ["ID_USER"] = HUsers.Id;
                        table_audit.Rows [i] ["REV"] = Get_LastRevision_Audit () + 1;
                    }

                    Edit ("audit", "ID", select_table_audit (), table_audit, out err);
                } catch (Exception E) {
                    Logging.Logg ().Exception (E, "Ошибка записи в таблицу audit PanelTECComponent : DB_Sostav_TEC : Write_Audit - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            #endregion

            /// <summary>
            /// Регистрация ID
            /// </summary>
            /// <param name="err">Ошибка в процессе регистрации</param>
            /// <returns>Возвращает ID</returns>
            protected int register_idListenerMainDB (out int err)
            {
                err = -1;
                int idListener = -1;

                try {
                    ConnectionSettings connSett = FormMainBaseWithStatusStrip.s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett ();

                    idListener = DbSources.Sources ().Register (connSett, false, CONN_SETT_TYPE.CONFIG_DB.ToString ());

                    err = 0;
                } catch (Exception E) {
                    Logging.Logg ().Exception (E, "Ошибка получения idListener PanelTECComponent : DB_Sostav_TEC : register_idListenerMainDB - ..." + "err = " + err.ToString (), Logging.INDEX_MESSAGE.NOT_SET);
                }

                return idListener;
            }

            /// <summary>
            /// Отмена регистрации ID
            /// </summary>
            /// <param name="idListener">ID</param>
            protected void unregister_idListenerMainDB (int idListener)
            {
                DbSources.Sources ().UnRegister (idListener);
            }

        }

        private class DataGridView_Prop : DataGridView {
            private void InitializeComponent ()
            {
                this.Columns.Add ("Значение", "Значение");
                this.ColumnHeadersVisible = true;
                this.Columns [0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                this.Columns [0].SortMode = DataGridViewColumnSortMode.NotSortable;
                this.RowHeadersVisible = true;
            }

            public DataGridView_Prop () : base ()
            {
                InitializeComponent ();//инициализация компонентов

                this.CellEndEdit += new DataGridViewCellEventHandler (this.cell_EndEdit);
            }

            /// <summary>
            /// Запрос на получение таблицы со свойствами
            /// </summary>
            /// <param name="id_list">Лист с идентификаторами компонентов</param>
            public void update_dgv (int id_component, DataTable table)
            {
                this.Rows.Clear ();
                DataRow [] rowsSel = table.Select (@"ID=" + id_component);

                if (rowsSel.Length == 1)
                    foreach (DataColumn col in table.Columns) {
                        this.Rows.Add (rowsSel [0] [col.ColumnName]);
                        this.Rows [this.Rows.Count - 1].HeaderCell.Value = col.ColumnName;
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
            private void cell_EndEdit (object sender, DataGridViewCellEventArgs e)
            {
                int n_row = -1;
                for (int i = 0; i < this.Rows.Count; i++) {
                    if (this.Rows [i].HeaderCell.Value.ToString () == "ID") {
                        n_row = (int)this.Rows [i].Cells [0].Value;
                    }
                }
                if (Rows [e.RowIndex].Cells [0].Value != null) {
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

            public override Color BackColor
            {
                get
                {
                    return base.BackColor;
                }

                set
                {
                    base.BackColor = value;

                    for (int j = 0; j < ColumnCount; j++)
                        for (int i = 0; i < RowCount; i++)
                            Rows [i].Cells [j].Style.BackColor = value == SystemColors.Control ? SystemColors.Window : value;
                }
            }
        }

        private class TreeView_TECComponent : TreeView {
            #region Переменные

            string m_warningReport;

            public struct ID_Comp {
                public int id_tec;
                public int id_gtp;
                public int id_pc;
                public int id_tg;
            }

            ID_Comp m_selNode_idComp = new ID_Comp ();

            /// <summary>
            /// Список ТЭЦ
            /// </summary>
            List<TEC> m_list_TEC = new List<TEC> ();

            DB_Sostav_TEC db_sostav = new DB_Sostav_TEC ();

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
                AddGTP = 0, AddPC, AddTG, AddTo, DelTo, AddTEC
            }

            /// <summary>
            /// Возвратить наименование компонента контекстного меню
            /// </summary>
            /// <param name="indx">Индекс режима</param>
            /// <returns>Строка - наименование режима</returns>
            protected static string getNameMode (int indx)
            {
                string [] nameModes = { "Добавить ГТП", "Добавить ЩУ", "Добавить ТГ", "Ввести в состав", "Вывести из состава", "Добавить ТЭЦ" };

                return nameModes [indx];
            }


            public static string not_add = "Не введенные в состав";

            List<StatisticCommon.TG> m_listTG = new List<TG> ();
            List<StatisticCommon.TECComponent> m_listTECComponent = new List<TECComponent> ();
            private System.Windows.Forms.ContextMenuStrip contextMenu_TreeView;

            List<string> m_open_node = new List<string> ();

            #endregion

            private void InitializeComponent ()
            {
                System.Windows.Forms.ToolStripMenuItem добавитьТЭЦToolStripMenuItem;

                contextMenu_TreeView = new System.Windows.Forms.ContextMenuStrip ();
                добавитьТЭЦToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
                this.Dock = DockStyle.Fill;

                #region Context add TEC
                // 
                // contextMenu_TreeView
                // 
                this.contextMenu_TreeView.Items.AddRange (new System.Windows.Forms.ToolStripItem [] {
            добавитьТЭЦToolStripMenuItem});
                this.contextMenu_TreeView.Name = "contextMenu_TreeView";
                // 
                // добавитьТЭЦToolStripMenuItem
                // 
                добавитьТЭЦToolStripMenuItem.Name = "добавитьТЭЦToolStripMenuItem";
                добавитьТЭЦToolStripMenuItem.Text = "Добавить ТЭЦ";
                #endregion

                //this.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(this.add_New_TEC);
            }

            public TreeView_TECComponent (bool contextEnable = true)
                : base ()
            {
                InitializeComponent ();

                m_list_TEC = db_sostav.get_list_tec ();

                this.ContextMenuStrip = contextMenu_TreeView;
                ContextMenuStrip.Items ["добавитьТЭЦToolStripMenuItem"].Visible = true;

                Update_tree ();

                if (contextEnable == true) {
                    this.NodeMouseClick += new TreeNodeMouseClickEventHandler (this.tree_NodeClick);
                }

                this.AfterSelect += new TreeViewEventHandler (this.tree_NodeSelect);
                this.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler (this.add_New_TEC);
            }

            /// <summary>
            /// Для возвращения имена по умолчанию для компонентов
            /// </summary>
            /// <param name="indx">Идентификатор типа компонента</param>
            /// <returns>Имя по умолчанию</returns>
            public static string Mass_NewVal_Comp (int indx)
            {
                String [] arPREFIX_COMPONENT = { "Новая ТЭЦ", "Новая ГТП", "Новый ЩУ", "Новая ТГ" };

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
                    }
                }
            }

            /// <summary>
            /// Заполнение TreeView компонентами
            /// </summary>
            public void Update_tree ()
            {
                int list_id = 0;

                m_list_TEC = db_sostav.get_list_tec ();

                m_open_node.Clear ();
                checked_node (this.Nodes, 0);//Получение открытых элементов дерева
                this.Nodes.Clear ();
                int tec_indx = -1;

                if (!(m_list_TEC == null))

                    foreach (StatisticCommon.TEC t in m_list_TEC) {
                        string path = null;

                        tec_indx++;

                        this.Nodes.Add (t.name_shr);//добавление ТЭЦ в TreeView

                        path += t.m_id.ToString ();
                        this.Nodes [tec_indx].Name = path;

                        if (t.list_TECComponents.Count > 0) {
                            int gtp_indx = 0,
                                pc_indx = 0,
                                node_indx = -1;

                            for (int i = (int)FormChangeMode.MODE_TECCOMPONENT.GTP; i < (int)FormChangeMode.MODE_TECCOMPONENT.ANY; i++) {
                                this.Nodes [tec_indx].Nodes.Add (FormChangeMode.getNameMode ((short)i));
                            }

                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Name = path + ':' + Convert.ToString ((int)TECComponent.ID.GTP);
                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.TG - 1].Name = path + ':' + Convert.ToString ((int)TECComponent.ID.TG);
                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Name = path + ':' + Convert.ToString ((int)TECComponent.ID.PC);

                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes.Add (not_add);
                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes [0].Name = path + ':' + "0";
                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes.Add (not_add);
                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes [0].Name = path + ':' + "0";

                            int gtp_node_null_indx = -1;

                            int pc_node_null_indx = -1;

                            foreach (StatisticCommon.TECComponent g in t.list_TECComponents) {
                                if (g.IsTG == true) {

                                    string tg_path = path;
                                    this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.TG - 1].Nodes.Add (g.name_shr);//Добавление компонента в TreeView
                                    node_indx++;
                                    tg_path += ':' + g.m_id.ToString ();
                                    this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.TG - 1].Nodes [node_indx].Name = tg_path;

                                    if ((g.m_listLowPointDev [0] as TG).m_id_owner_pc == -1) {
                                        string pc_tg_path = path;

                                        pc_node_null_indx++;
                                        this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes [0].Nodes.Add (g.m_listLowPointDev [0].name_shr);
                                        pc_tg_path += ":" + g.m_listLowPointDev [0].m_id.ToString ();
                                        this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes [0].Nodes [pc_node_null_indx].Name = pc_tg_path;
                                    }
                                    if ((g.m_listLowPointDev [0] as TG).m_id_owner_gtp == -1) {
                                        string gtp_tg_path = path;

                                        gtp_node_null_indx++;
                                        this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes [0].Nodes.Add (g.m_listLowPointDev [0].name_shr);
                                        gtp_tg_path += ":" + g.m_listLowPointDev [0].m_id.ToString ();
                                        this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes [0].Nodes [gtp_node_null_indx].Name = gtp_tg_path;
                                    }

                                } else
                                    ;

                                if (g.IsGTP == true) {
                                    string gtp_path = path;
                                    int gtp_node_indx = -1;
                                    this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes.Add (g.name_shr);//Добавление компонента в TreeView

                                    gtp_indx++;
                                    gtp_path += ':' + g.m_id.ToString ();
                                    this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes [gtp_indx].Name = gtp_path;

                                    foreach (StatisticCommon.TG h in g.m_listLowPointDev) {
                                        string gtp_tg_path = gtp_path;

                                        if (h.m_id_owner_gtp == g.m_id) {
                                            gtp_node_indx++;
                                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes [gtp_indx].Nodes.Add (h.name_shr);
                                            gtp_tg_path += ':' + h.m_id.ToString ();
                                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes [gtp_indx].Nodes [gtp_node_indx].Name = gtp_tg_path;
                                        } else
                                            ;
                                    }
                                } else
                                    ;

                                if (g.IsPC == true) {
                                    string pc_path = path;

                                    int pc_node_indx = -1;

                                    this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes.Add (g.name_shr);//Добавление компонента в TreeView
                                    pc_indx++;
                                    pc_path += ':' + g.m_id.ToString ();
                                    this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes [pc_indx].Name = pc_path;

                                    foreach (StatisticCommon.TG h in g.m_listLowPointDev) {
                                        string pc_tg_path = pc_path;

                                        if (h.m_id_owner_pc == g.m_id) {
                                            pc_node_indx++;
                                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes [pc_indx].Nodes.Add (h.name_shr);
                                            pc_tg_path += ':' + h.m_id.ToString ();
                                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes [pc_indx].Nodes [pc_node_indx].Name = pc_tg_path;
                                        } else
                                            ;
                                    }
                                } else
                                    ;
                            }
                        }
                    }
                list_id = 0;
                checked_node (this.Nodes, list_id, true);
                foreach (TreeNode n in this.Nodes) {
                    if (n.IsExpanded == true) {
                        this.SelectedNode = n;
                        break;
                    }
                }
            }

            /// <summary>
            /// Обработчик события нажатия на элемент в TreeView
            /// </summary>
            private void tree_NodeClick (object sender, TreeNodeMouseClickEventArgs e)
            {
                System.Windows.Forms.ContextMenuStrip contextMenu_TreeNode = new System.Windows.Forms.ContextMenuStrip ();

                System.Windows.Forms.ToolStripMenuItem ввестиВСоставToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
                System.Windows.Forms.ToolStripMenuItem вывыстиИзСоставаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
                System.Windows.Forms.ToolStripMenuItem добавитьГТПToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
                System.Windows.Forms.ToolStripMenuItem добавитьЩУToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
                System.Windows.Forms.ToolStripMenuItem добавитьТГToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();

                #region Нажатие правой кнопкой мыши

                if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                    this.SelectedNode = e.Node;//Выбор компонента при нажатии на него правой кнопкой мыши

                    if (m_selNode_idComp.id_tg != -1)//выбран ли элемент ТГ
                    {
                        #region Не введенные
                        if (this.SelectedNode.Parent.Text == TreeView_TECComponent.not_add) {
                            #region Context add TG from PC or GTP
                            // 
                            // contextMenu_TreeView_TG_PC
                            // 
                            contextMenu_TreeNode.Items.AddRange (new System.Windows.Forms.ToolStripItem [] {
                        ввестиВСоставToolStripMenuItem});
                            contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                            // 
                            // ввестиВСоставToolStripMenuItem
                            //
                            ввестиВСоставToolStripMenuItem.Name = "ввестиВСоставToolStripMenuItem";
                            ввестиВСоставToolStripMenuItem.Text = "Ввести в состав";
                            #endregion

                            this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;

                            (this.SelectedNode.ContextMenuStrip.Items ["ввестиВСоставToolStripMenuItem"] as ToolStripDropDownItem).DropDownItems.Clear ();
                            for (int i = 1; i < this.SelectedNode.Parent.Parent.Nodes.Count; i++) {
                                (this.SelectedNode.ContextMenuStrip.Items ["ввестиВСоставToolStripMenuItem"] as ToolStripDropDownItem).DropDownItems.Add (this.SelectedNode.Parent.Parent.Nodes [i].Text);
                            }
                            (this.SelectedNode.ContextMenuStrip.Items ["ввестиВСоставToolStripMenuItem"] as ToolStripDropDownItem).DropDownItems.Clear ();
                            for (int i = 1; i < this.SelectedNode.Parent.Parent.Nodes.Count; i++) {
                                (this.SelectedNode.ContextMenuStrip.Items ["ввестиВСоставToolStripMenuItem"] as ToolStripDropDownItem).DropDownItems.Add (this.SelectedNode.Parent.Parent.Nodes [i].Text);
                            }

                            (this.SelectedNode.ContextMenuStrip.Items ["ввестиВСоставToolStripMenuItem"] as ToolStripDropDownItem).DropDownItemClicked += new ToolStripItemClickedEventHandler (add_TG_PC_GTP);
                        }
                        #endregion

                        #region Введенные в состав

                        if (this.SelectedNode.Parent.Text != TreeView_TECComponent.not_add) {
                            if (m_selNode_idComp.id_tg > (int)TECComponent.ID.TG) {
                                #region Context delete TG
                                // 
                                // contextMenu_TreeNode
                                // 
                                contextMenu_TreeNode.Items.AddRange (new System.Windows.Forms.ToolStripItem [] {
                                вывыстиИзСоставаToolStripMenuItem});
                                contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                                // 
                                // вывыстиИзСоставаToolStripMenuItem
                                //
                                вывыстиИзСоставаToolStripMenuItem.Name = "вывыстиИзСоставаToolStripMenuItem";
                                вывыстиИзСоставаToolStripMenuItem.Text = "Вывести из состава";
                                #endregion

                                this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                                this.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler (this.del_TG);
                            }
                        }

                        #endregion
                    }

                    #region Добавление компонентов

                    if (m_selNode_idComp.id_tg == -1 & m_selNode_idComp.id_pc == -1 & m_selNode_idComp.id_gtp == -1 & m_selNode_idComp.id_tec != -1)//Выбрана ли ТЭЦ
                    {
                        #region Добавление в ТЭЦ компонентов

                        #region Context TEC
                        // 
                        // contextMenu_TreeView_TEC
                        // 
                        contextMenu_TreeNode.Items.AddRange (new System.Windows.Forms.ToolStripItem [] {
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

                        this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;

                        this.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler (this.add_New_TEC_COMP);

                        #endregion
                    }

                    if (m_selNode_idComp.id_gtp == (int)TECComponent.ID.GTP)//Выбран корень ГТП
                    {
                        #region Context add GTP
                        //
                        // contextMenu_TreeNode
                        //
                        contextMenu_TreeNode.Items.AddRange (new System.Windows.Forms.ToolStripItem [] {
                    добавитьГТПToolStripMenuItem});
                        contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                        // 
                        // добавитьГТПToolStripMenuItem
                        // 
                        добавитьГТПToolStripMenuItem.Name = "добавитьГТПToolStripMenuItem";
                        добавитьГТПToolStripMenuItem.Text = "Добавить ГТП";
                        #endregion

                        this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                        this.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler (this.add_New_TEC_COMP);
                    }

                    if (m_selNode_idComp.id_pc == (int)TECComponent.ID.PC)//Выбран корень ЩУ
                    {
                        #region Context add PC
                        // 
                        // contextMenu_TreeNode
                        // 
                        contextMenu_TreeNode.Items.AddRange (new System.Windows.Forms.ToolStripItem [] {
                    добавитьЩУToolStripMenuItem});
                        contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                        // 
                        // добавитьЩУToolStripMenuItem
                        // 
                        добавитьЩУToolStripMenuItem.Name = "добавитьЩУToolStripMenuItem";
                        добавитьЩУToolStripMenuItem.Text = "Добавить ЩУ";
                        #endregion

                        this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                        this.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler (this.add_New_TEC_COMP);
                    }

                    if (m_selNode_idComp.id_tg == (int)TECComponent.ID.TG)//Выбран "Поблочно"
                    {
                        this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                    }

                    if (this.SelectedNode.Text == not_add)//Выбран "Не введенные в состав"
                    {
                        this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                    }

                    #endregion

                    #region Удаление из состава

                    if ((m_selNode_idComp.id_gtp > (int)TECComponent.ID.GTP & m_selNode_idComp.id_gtp < (int)TECComponent.ID.PC & m_selNode_idComp.id_tg == -1) || (m_selNode_idComp.id_pc > (int)TECComponent.ID.PC & m_selNode_idComp.id_pc < (int)TECComponent.ID.TG & m_selNode_idComp.id_tg == -1))//Выбран конкретный ЩУ или ГТП
                    {
                        #region Context delete PC,GTP
                        // 
                        // contextMenu_TreeNode
                        // 
                        contextMenu_TreeNode.Items.AddRange (new System.Windows.Forms.ToolStripItem [] {
                    вывыстиИзСоставаToolStripMenuItem});
                        contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                        // 
                        // вывыстиИзСоставаToolStripMenuItem
                        //
                        вывыстиИзСоставаToolStripMenuItem.Name = "вывыстиИзСоставаToolStripMenuItem";
                        вывыстиИзСоставаToolStripMenuItem.Text = "Вывести из состава";
                        #endregion

                        this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                        this.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler (this.del_Comp);
                    }

                    #endregion
                }

                #endregion
            }

            /// <summary>
            /// Обработчик добавления новой ТЭЦ
            /// </summary>
            private void add_New_TEC (object sender, ToolStripItemClickedEventArgs e)
            {
                Report (this, new ReportEventArgs (string.Empty, string.Empty, string.Empty, true));

                int id_newTEC = 0;
                if (e.ClickedItem.Text == (string)getNameMode ((int)ID_Menu.AddTEC)) {
                    this.Nodes.Add (Mass_NewVal_Comp ((int)FormChangeMode.MODE_TECCOMPONENT.TEC));
                    id_newTEC = GetID (this, new GetIDEventArgs (0));
                    Nodes [Nodes.Count - 1].Name = Convert.ToString (id_newTEC);

                    ID_Comp id = new ID_Comp ();

                    id.id_tec = -1;
                    id.id_gtp = -1;
                    id.id_pc = -1;
                    id.id_tg = -1;

                    id.id_tec = id_newTEC;

                    EditNode (this, new EditNodeEventArgs (id, ID_Operation.Insert));
                }
            }

            /// <summary>
            /// Обработчик добавления нового компонента ТЭЦ
            /// </summary>
            private void add_New_TEC_COMP (object sender, ToolStripItemClickedEventArgs e)
            {
                Report (this, new ReportEventArgs (string.Empty, string.Empty, string.Empty, true));

                if (e.ClickedItem.Text == (string)getNameMode (0))//Добавление новой ГТП
                {
                    int id_newGTP = 0;

                    ID_Comp id = new ID_Comp ();

                    id.id_tec = -1;
                    id.id_gtp = -1;
                    id.id_pc = -1;
                    id.id_tg = -1;

                    id_newGTP = GetID (this, new GetIDEventArgs ((int)TECComponent.ID.GTP));

                    id.id_tec = m_selNode_idComp.id_tec;
                    id.id_gtp = id_newGTP;

                    EditNode (this, new EditNodeEventArgs (id, ID_Operation.Insert));

                    foreach (TreeNode tec in Nodes) {
                        if (Convert.ToInt32 (tec.Name) == m_selNode_idComp.id_tec) {
                            if (tec.FirstNode == null) {
                                tec.Nodes.Add (FormChangeMode.getNameMode ((int)FormChangeMode.MODE_TECCOMPONENT.GTP));
                                tec.Nodes [tec.Nodes.Count - 1].Name = tec.Name + ':' + (int)TECComponent.ID.GTP;
                                tec.Nodes [tec.Nodes.Count - 1].Nodes.Add (not_add);
                                tec.Nodes [tec.Nodes.Count - 1].Nodes [0].Name = tec.Name + ':' + "0";

                                tec.Nodes.Add (FormChangeMode.getNameMode ((int)FormChangeMode.MODE_TECCOMPONENT.PC));
                                tec.Nodes [tec.Nodes.Count - 1].Name = tec.Name + ':' + (int)TECComponent.ID.PC;
                                tec.Nodes [tec.Nodes.Count - 1].Nodes.Add (not_add);
                                tec.Nodes [tec.Nodes.Count - 1].Nodes [0].Name = tec.Name + ':' + "0";
                                tec.Nodes.Add (FormChangeMode.getNameMode ((int)FormChangeMode.MODE_TECCOMPONENT.TG));
                                tec.Nodes [tec.Nodes.Count - 1].Name = tec.Name + ':' + (int)TECComponent.ID.TG;
                            }
                            foreach (TreeNode com in tec.Nodes) {

                                if (com.Text == FormChangeMode.getNameMode (1)) {
                                    com.Nodes.Add (Mass_NewVal_Comp ((int)FormChangeMode.MODE_TECCOMPONENT.GTP));
                                    com.Nodes [com.Nodes.Count - 1].Name = Convert.ToString (tec.Name + ':' + id_newGTP);
                                }
                            }
                        }
                    }
                } else
                    if (e.ClickedItem.Text == (string)getNameMode (1))//Добавление нового ЩУ
                    {
                    int id_newPC = 0;

                    ID_Comp id = new ID_Comp ();

                    id.id_tec = -1;
                    id.id_gtp = -1;
                    id.id_pc = -1;
                    id.id_tg = -1;

                    id_newPC = GetID (this, new GetIDEventArgs ((int)TECComponent.ID.PC));

                    id.id_tec = m_selNode_idComp.id_tec;
                    id.id_pc = id_newPC;

                    EditNode (this, new EditNodeEventArgs (id, ID_Operation.Insert));

                    foreach (TreeNode tec in Nodes) {
                        if (Convert.ToInt32 (tec.Name) == m_selNode_idComp.id_tec) {
                            if (tec.FirstNode == null) {
                                tec.Nodes.Add (FormChangeMode.getNameMode ((int)FormChangeMode.MODE_TECCOMPONENT.GTP));
                                tec.Nodes [tec.Nodes.Count - 1].Name = tec.Name + ':' + (int)TECComponent.ID.GTP;
                                tec.Nodes [tec.Nodes.Count - 1].Nodes.Add (not_add);
                                tec.Nodes [tec.Nodes.Count - 1].Nodes [0].Name = tec.Name + ':' + "0";
                                tec.Nodes.Add (FormChangeMode.getNameMode ((int)FormChangeMode.MODE_TECCOMPONENT.PC));
                                tec.Nodes [tec.Nodes.Count - 1].Name = tec.Name + ':' + (int)TECComponent.ID.PC;
                                tec.Nodes [tec.Nodes.Count - 1].Nodes.Add (not_add);
                                tec.Nodes [tec.Nodes.Count - 1].Nodes [0].Name = tec.Name + ':' + "0";
                                tec.Nodes.Add (FormChangeMode.getNameMode ((int)FormChangeMode.MODE_TECCOMPONENT.TG));
                                tec.Nodes [tec.Nodes.Count - 1].Name = tec.Name + ':' + (int)TECComponent.ID.TG;
                            }
                            foreach (TreeNode n in tec.Nodes) {
                                if (n.Text == FormChangeMode.getNameMode (2)) {
                                    n.Nodes.Add (Mass_NewVal_Comp ((int)FormChangeMode.MODE_TECCOMPONENT.PC));
                                    n.Nodes [n.Nodes.Count - 1].Name = Convert.ToString (tec.Name + ':' + id_newPC);
                                }
                            }
                        }
                    }
                } else {
                    if (e.ClickedItem.Text == (string)getNameMode (2))//Добавление нового ТГ
                    {
                        int id_newTG = 0;

                        ID_Comp id = new ID_Comp ();

                        id.id_tec = -1;
                        id.id_gtp = -1;
                        id.id_pc = -1;
                        id.id_tg = -1;

                        id_newTG = GetID (this, new GetIDEventArgs ((int)TECComponent.ID.TG));

                        id.id_tec = m_selNode_idComp.id_tec;
                        id.id_pc = 0;
                        id.id_tg = id_newTG;

                        EditNode (this, new EditNodeEventArgs (id, ID_Operation.Insert));

                        foreach (TreeNode tec in Nodes) {
                            if (Convert.ToInt32 (tec.Name) == m_selNode_idComp.id_tec) {
                                if (tec.FirstNode == null) {
                                    tec.Nodes.Add (FormChangeMode.getNameMode ((int)FormChangeMode.MODE_TECCOMPONENT.GTP));
                                    tec.Nodes [tec.Nodes.Count - 1].Name = tec.Name + ':' + (int)TECComponent.ID.GTP;
                                    tec.Nodes [tec.Nodes.Count - 1].Nodes.Add (not_add);
                                    tec.Nodes [tec.Nodes.Count - 1].Nodes [0].Name = tec.Name + ':' + "0";
                                    tec.Nodes.Add (FormChangeMode.getNameMode ((int)FormChangeMode.MODE_TECCOMPONENT.PC));
                                    tec.Nodes [tec.Nodes.Count - 1].Name = tec.Name + ':' + (int)TECComponent.ID.PC;
                                    tec.Nodes [tec.Nodes.Count - 1].Nodes.Add (not_add);
                                    tec.Nodes [tec.Nodes.Count - 1].Nodes [0].Name = tec.Name + ':' + "0";
                                    tec.Nodes.Add (FormChangeMode.getNameMode ((int)FormChangeMode.MODE_TECCOMPONENT.TG));
                                    tec.Nodes [tec.Nodes.Count - 1].Name = tec.Name + ':' + (int)TECComponent.ID.TG;
                                }
                                foreach (TreeNode n in tec.Nodes) {
                                    if (n.Text == FormChangeMode.getNameMode (3)) {
                                        n.Nodes.Add (Mass_NewVal_Comp ((int)FormChangeMode.MODE_TECCOMPONENT.TG));
                                        n.Nodes [n.Nodes.Count - 1].Name = Convert.ToString (n.Parent.Name + ':' + id_newTG);

                                        foreach (TreeNode comp in tec.Nodes) {
                                            if (comp.Text == FormChangeMode.getNameMode (1)) {
                                                if (comp.Nodes.Count == 0)
                                                    comp.Nodes.Add (not_add);
                                                comp.Nodes [0].Nodes.Add (Mass_NewVal_Comp ((int)FormChangeMode.MODE_TECCOMPONENT.TG));
                                                comp.Nodes [0].Nodes [comp.Nodes [0].Nodes.Count - 1].Name = n.Parent.Name + ':' + id_newTG;
                                            }
                                            if (comp.Text == FormChangeMode.getNameMode (2)) {
                                                if (comp.Nodes.Count == 0)
                                                    comp.Nodes.Add (not_add);
                                                comp.Nodes [0].Nodes.Add (Mass_NewVal_Comp ((int)FormChangeMode.MODE_TECCOMPONENT.TG));
                                                comp.Nodes [0].Nodes [comp.Nodes [0].Nodes.Count - 1].Name = n.Parent.Name + ':' + id_newTG;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    } else
                        if (e.ClickedItem.Text == (string)getNameMode (4))//Выведение ТЭЦ из состава
                        {
                        bool del = true;

                        foreach (TreeNode tec in Nodes) {
                            if (Convert.ToInt32 (tec.Name) == m_selNode_idComp.id_tec) {

                                foreach (TreeNode tc in tec.Nodes) {
                                    if (tc.Nodes.Count > 1 & tc.Text != FormChangeMode.getNameMode (3)) {
                                        del = false;
                                        break;
                                    }
                                    if (tc.Nodes.Count > 0 & tc.Text == FormChangeMode.getNameMode (3)) {
                                        del = false;
                                        break;
                                    }
                                }

                            }
                        }
                        if (del == true) {
                            EditNode (this, new EditNodeEventArgs (m_selNode_idComp, ID_Operation.Delete));
                            SelectedNode.Remove ();
                        } else {
                            m_warningReport = "Имеются не выведенные из состава компоненты в " + SelectedNode.Text;
                            Report (this, new ReportEventArgs (string.Empty, string.Empty, m_warningReport, false));
                            //MessageBox.Show("Имеются не выведенные из состава компоненты в " + SelectedNode.Text,"Внимание!",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                        }
                    }
                }
            }

            /// <summary>
            /// Обработчик введения в состав ТГ
            /// </summary>
            private void add_TG_PC_GTP (object sender, ToolStripItemClickedEventArgs e)
            {
                Report (this, new ReportEventArgs (string.Empty, string.Empty, string.Empty, true));

                if (SelectedNode.Parent.Parent.Text == (string)FormChangeMode.getNameMode (1))//Введение ТГ в состав ГТП
                {
                    int id_gtp = 0;

                    foreach (TreeNode t in SelectedNode.Parent.Parent.Nodes) {
                        if (e.ClickedItem.Text == t.Text) {
                            ID_Comp id = get_m_id_list (t.Name);
                            id_gtp = id.id_gtp;
                        }
                    }

                    foreach (TreeNode tec in Nodes) {
                        if (Convert.ToInt32 (tec.Name) == m_selNode_idComp.id_tec) {
                            foreach (TreeNode n in tec.Nodes) {
                                if (n.Text == FormChangeMode.getNameMode (1)) {
                                    foreach (TreeNode gtp in n.Nodes) {
                                        if (get_m_id_list (gtp.Name).id_gtp == id_gtp) {
                                            string obj = (string)FormChangeMode.getNameMode (1);

                                            gtp.Nodes.Add (SelectedNode.Text);
                                            gtp.Nodes [gtp.Nodes.Count - 1].Name = gtp.Name + ':' + m_selNode_idComp.id_tg;

                                            EditNode (this, new EditNodeEventArgs (get_m_id_list (gtp.Nodes [gtp.Nodes.Count - 1].Name), ID_Operation.Update, obj));

                                            SelectedNode.Remove ();
                                        }
                                    }
                                }
                            }
                        }
                    }
                } else {
                    if (SelectedNode.Parent.Parent.Text == (string)FormChangeMode.getNameMode (2))//Введение ТГ в состав ЩУ
                    {
                        int id_pc = 0;

                        foreach (TreeNode t in SelectedNode.Parent.Parent.Nodes) {
                            if (e.ClickedItem.Text == t.Text) {
                                ID_Comp id = get_m_id_list (t.Name);
                                id_pc = id.id_pc;
                            }
                        }

                        foreach (TreeNode tec in Nodes) {
                            if (Convert.ToInt32 (tec.Name) == m_selNode_idComp.id_tec) {
                                foreach (TreeNode n in tec.Nodes) {
                                    if (n.Text == FormChangeMode.getNameMode (2)) {
                                        foreach (TreeNode pc in n.Nodes) {
                                            if (get_m_id_list (pc.Name).id_pc == id_pc) {
                                                string obj = (string)FormChangeMode.getNameMode (2);

                                                pc.Nodes.Add (SelectedNode.Text);
                                                pc.Nodes [pc.Nodes.Count - 1].Name = pc.Name + ':' + m_selNode_idComp.id_tg;

                                                EditNode (this, new EditNodeEventArgs (get_m_id_list (pc.Nodes [pc.Nodes.Count - 1].Name), ID_Operation.Update, obj));

                                                SelectedNode.Remove ();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Обработчик удаления компонента ТЭЦ (ГТП, ЩУ)
            /// </summary>
            private void del_Comp (object sender, ToolStripItemClickedEventArgs e)
            {
                Report (this, new ReportEventArgs (string.Empty, string.Empty, string.Empty, true));

                if (e.ClickedItem.Text == (string)getNameMode (4)) {
                    if (m_selNode_idComp.id_gtp.Equals (-1) == false & m_selNode_idComp.id_tg.Equals (-1) == true)//TG
                    {
                        if (SelectedNode.FirstNode == null) {
                            EditNode (this, new EditNodeEventArgs (m_selNode_idComp, ID_Operation.Delete));

                            SelectedNode.Remove ();

                        } else {
                            m_warningReport = "Имеются не выведенные из состава компоненты в " + SelectedNode.Text;
                            Report (this, new ReportEventArgs (string.Empty, string.Empty, m_warningReport, false));
                            //MessageBox.Show("Имеются не выведенные из состава компоненты в " + SelectedNode.Text, "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    if (m_selNode_idComp.id_pc.Equals (-1) == false & m_selNode_idComp.id_tg.Equals (-1) == true)//PC
                    {
                        if (SelectedNode.FirstNode == null) {
                            EditNode (this, new EditNodeEventArgs (m_selNode_idComp, ID_Operation.Delete));

                            SelectedNode.Remove ();
                        } else {
                            m_warningReport = "Имеются не выведенные из состава компоненты в " + SelectedNode.Text;
                            Report (this, new ReportEventArgs (string.Empty, string.Empty, m_warningReport, false));
                            //MessageBox.Show("Имеются не выведенные из состава компоненты в " + SelectedNode.Text, "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }

            /// <summary>
            /// Обработчик удаления компонента ТЭЦ (ТГ)
            /// </summary>
            private void del_TG (object sender, ToolStripItemClickedEventArgs e)
            {
                Report (this, new ReportEventArgs (string.Empty, string.Empty, string.Empty, true));

                if (SelectedNode.Parent.Parent.Text == (string)FormChangeMode.getNameMode (1))//Выведение ТГ из состава ГТП
                {
                    string obj = (string)FormChangeMode.getNameMode (1);
                    SelectedNode.Parent.Parent.Nodes [0].Nodes.Add (SelectedNode.Text);
                    SelectedNode.Parent.Parent.Nodes [0].Nodes [SelectedNode.Parent.Parent.Nodes [0].Nodes.Count - 1].Name = Convert.ToString (m_selNode_idComp.id_tec) + ':' + m_selNode_idComp.id_tg;

                    EditNode (this, new EditNodeEventArgs (get_m_id_list (SelectedNode.Parent.Parent.Nodes [0].Nodes [SelectedNode.Parent.Parent.Nodes [0].Nodes.Count - 1].Name), ID_Operation.Update, obj));

                    SelectedNode.Remove ();
                }

                if (SelectedNode.Parent.Parent.Text == (string)FormChangeMode.getNameMode (2))//Выведение ТГ из состава ЩУ
                {
                    string obj = (string)FormChangeMode.getNameMode (2);
                    SelectedNode.Parent.Parent.Nodes [0].Nodes.Add (SelectedNode.Text);
                    SelectedNode.Parent.Parent.Nodes [0].Nodes [SelectedNode.Parent.Parent.Nodes [0].Nodes.Count - 1].Name = Convert.ToString (m_selNode_idComp.id_tec) + ':' + m_selNode_idComp.id_tg;

                    EditNode (this, new EditNodeEventArgs (get_m_id_list (SelectedNode.Parent.Parent.Nodes [0].Nodes [SelectedNode.Parent.Parent.Nodes [0].Nodes.Count - 1].Name), ID_Operation.Update, obj));


                    SelectedNode.Remove ();
                }

                if (SelectedNode.Parent.Text == (string)FormChangeMode.getNameMode (3))//Выведение ТГ из состава ТЭЦ
                {
                    bool not_delete = false;
                    string warning = string.Empty;

                    foreach (TreeNode tec in Nodes) {
                        if (Convert.ToInt32 (tec.Name) == m_selNode_idComp.id_tec) {
                            foreach (TreeNode tc in tec.Nodes) {
                                if (tc.Text == FormChangeMode.getNameMode (1) || tc.Text == FormChangeMode.getNameMode (2)) {
                                    foreach (TreeNode comp in tc.Nodes) {
                                        if (comp.Text != not_add) {
                                            foreach (TreeNode tg in comp.Nodes) {
                                                if (get_m_id_list (tg.Name).id_tg == m_selNode_idComp.id_tg) {
                                                    not_delete = true;
                                                    warning += SelectedNode.Text + " входит в состав " + comp.Text + "." + '\n';
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                    if (not_delete == false) {
                        foreach (TreeNode tec in Nodes) {
                            if (Convert.ToInt32 (tec.Name) == m_selNode_idComp.id_tec) {
                                foreach (TreeNode tc in tec.Nodes) {
                                    if (tc.Text == FormChangeMode.getNameMode (1) || tc.Text == FormChangeMode.getNameMode (2)) {
                                        foreach (TreeNode comp in tc.Nodes) {
                                            if (comp.Text == not_add) {
                                                foreach (TreeNode tg in comp.Nodes) {
                                                    if (get_m_id_list (tg.Name).id_tg == m_selNode_idComp.id_tg) {
                                                        tg.Remove ();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        EditNode (this, new EditNodeEventArgs (m_selNode_idComp, ID_Operation.Delete));
                        SelectedNode.Remove ();
                    } else {
                        m_warningReport = warning + '\n' + "Выведение из состава ТЭЦ не возможно!";
                        Report (this, new ReportEventArgs (string.Empty, string.Empty, m_warningReport, false));
                        //MessageBox.Show(warning + '\n' + "Выведение из состава ТЭЦ не возможно!", "Выведение из состава ТЭЦ не возможно!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }

            /// <summary>
            /// Обработчик события выбора элемента в TreeView
            /// </summary>
            private void tree_NodeSelect (object sender, TreeViewEventArgs e)
            {
                m_selNode_idComp = get_m_id_list (e.Node.Name);

                EditNode (this, new TreeView_TECComponent.EditNodeEventArgs (m_selNode_idComp, ID_Operation.Select));
            }

            public void Rename_Node (int id_comp, string name)
            {
                foreach (TreeNode tec in Nodes) {
                    if (get_m_id_list (tec.Name).id_tec == id_comp) {
                        tec.Text = name;
                    } else
                        if (tec.FirstNode != null)
                        foreach (TreeNode tc in tec.Nodes) {
                            if (tc.FirstNode != null)
                                foreach (TreeNode comp in tc.Nodes) {
                                    if (get_m_id_list (comp.Name).id_gtp == id_comp) {
                                        comp.Text = name;
                                    } else
                                        if (get_m_id_list (comp.Name).id_pc == id_comp) {
                                        comp.Text = name;
                                    } else
                                            if (get_m_id_list (comp.Name).id_tg == id_comp) {
                                        comp.Text = name;
                                    } else
                                                if (tc.FirstNode != null)
                                        foreach (TreeNode tg in comp.Nodes) {
                                            if (get_m_id_list (tg.Name).id_tg == id_comp) {
                                                tg.Text = name;
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
                id_comp.id_tec = -1;
                id_comp.id_gtp = -1;
                id_comp.id_pc = -1;
                id_comp.id_tg = -1;

                //id_list[2] = 0;
                if (id_string != "") {
                    string [] path = id_string.Split (':');

                    for (int i = 0; i < path.Length; i++) {
                        if (Convert.ToInt32 (path [i]) < (int)TECComponentBase.ID.GTP & Convert.ToInt32 (path [i]) >= 0) {
                            id_comp.id_tec = Convert.ToInt32 (path [i]);
                        }
                        if (Convert.ToInt32 (path [i]) >= (int)TECComponentBase.ID.GTP & Convert.ToInt32 (path [i]) < (int)TECComponentBase.ID.PC) {
                            id_comp.id_gtp = Convert.ToInt32 (path [i]);
                        }
                        if (Convert.ToInt32 (path [i]) >= (int)TECComponentBase.ID.PC & Convert.ToInt32 (path [i]) < (int)TECComponentBase.ID.TG) {
                            id_comp.id_pc = Convert.ToInt32 (path [i]);
                        }
                        if (Convert.ToInt32 (path [i]) >= (int)TECComponentBase.ID.TG & Convert.ToInt32 (path [i]) < (int)TECComponentBase.ID.MAX) {
                            id_comp.id_tg = Convert.ToInt32 (path [i]);
                        }
                    }
                }
                return id_comp;
            }

            /// <summary>
            /// Класс для описания аргумента события - изменения компонента
            /// </summary>
            public class EditNodeEventArgs : EventArgs {
                /// <summary>
                /// Список ID компонента
                /// </summary>
                public ID_Comp m_IdComp;

                /// <summary>
                /// Тип производимой операции
                /// </summary>
                public ID_Operation m_Operation;

                /// <summary>
                /// Значение изменяемого параметра
                /// </summary>
                public string m_Value;

                public EditNodeEventArgs (ID_Comp idComp, ID_Operation operation, string value = null)
                {
                    m_IdComp = idComp;
                    m_Operation = operation;
                    m_Value = value;
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
                /// ID компонента
                /// </summary>
                public int m_IdComp;

                public GetIDEventArgs (int id_comp)
                {
                    m_IdComp = id_comp;
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
            /// Класс для описания аргумента события - получение репорта
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
    }
}
