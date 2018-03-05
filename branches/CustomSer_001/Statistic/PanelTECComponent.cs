﻿using System;
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

using StatisticCommon;
using ASUTP.Core;
using ASUTP.Database;
using ASUTP;

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
            dgvProp = new DataGridView_Prop(ForeColor, BackColor == SystemColors.Control ? SystemColors.Window : BackColor);
            btnOK = new Button();
            btnBreak = new Button();

            this.SuspendLayout();

            initializeLayoutStyle(6, 37);

            // 
            // treeView_TECComponent
            //
            treeView_TECComponent = new TreeView_TECComponent(true, ForeColor, BackColor == SystemColors.Control ? SystemColors.Window : BackColor);
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
            : base(MODE_UPDATE_VALUES.AUTO, FormMain.formGraphicsSettings.FontColor, FormMain.formGraphicsSettings.BackgroundColor)
        {
            InitializeComponent();

            m_arr_originalTable = new DataTable[(int)FormChangeMode.MODE_TECCOMPONENT.ANY];
            m_arr_editTable = new DataTable[(int)FormChangeMode.MODE_TECCOMPONENT.ANY];
            fill_DataTable_ComponentsTEC();
            
            treeView_TECComponent.GetNextID += new Func<TECComponent.ID, int>(this.getNextID);
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
            int err = -1;

            FormChangeMode.MODE_TECCOMPONENT i = FormChangeMode.MODE_TECCOMPONENT.Unknown;

            // установить режим "удержание соединения" при нескольких подряд вызовах статических методов 'DbTSQLInterface::Select'
#if MODE_STATIC_CONNECTION_LEAVING
            DbTSQLInterface.ModeStaticConnectionLeave = DbTSQLInterface.ModeStaticConnectionLeaving.Yes;
#endif
            // несколько подряд вызовов статических методов 'DbTSQLInterface::Select'
            for (i = (FormChangeMode.MODE_TECCOMPONENT)0; i < FormChangeMode.MODE_TECCOMPONENT.ANY; i++) {
                m_arr_originalTable [(short)i] = db_sostav.GetTableTECComponent (i, out err);

                if ((!(err == 0))
                    || (m_arr_originalTable [(short)i].Columns.Count == 0))
                    break;
                else
                    ;
            }
            // возвратить режим "удержание соединения - обычный" (разрыв соединения после каждого вызова статического метода 'DbTSQLInterface::Select')
#if MODE_STATIC_CONNECTION_LEAVING
            DbTSQLInterface.ModeStaticConnectionLeave = DbTSQLInterface.ModeStaticConnectionLeaving.No;
#endif
            if (i < FormChangeMode.MODE_TECCOMPONENT.ANY)
                throw new InvalidOperationException ($"PanelTECComponent::fill_DataTable_ComponentsTEC () - ошибка приполучении данных для {i.ToString()}...");
            else
                ;

            reset_DataTable_ComponentsTEC ();
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
        /// Событие для оповещения внешних клиентов при добавлении/удалении ГТП
        /// </summary>
        public event Action<int, DbTSQLInterface.QUERY_TYPE> EventConfigGTPChanged;

        /// <summary>
        /// Обработчик получения данных от TreeView
        /// </summary>
        private void get_operation_tree (object sender, TreeView_TECComponent.EditNodeEventArgs e)
        {
            if (e.m_Operation == DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE) {
                select (e.m_IdComp);
            } else
                ;

            if (e.m_Operation == DbTSQLInterface.QUERY_TYPE.DELETE) {
                delete (e.m_IdComp);
            } else
                ;

            if (e.m_Operation == DbTSQLInterface.QUERY_TYPE.INSERT) {
                insert (e.m_IdComp);
            } else
                ;

            if (e.m_Operation == DbTSQLInterface.QUERY_TYPE.UPDATE) {
                update (e.m_IdComp, e.m_Value);
            } else
                ;

            // дополн. действия по вставке/удалению ГТП
            if (e.m_IdComp.ID == TECComponentBase.ID.GTP)
                if ((e.m_Operation == DbTSQLInterface.QUERY_TYPE.INSERT)
                    || (e.m_Operation == DbTSQLInterface.QUERY_TYPE.DELETE))
                    EventConfigGTPChanged?.Invoke (e.m_IdComp[TECComponentBase.ID.GTP], e.m_Operation);
                else
                    ;
            else
                ;
        }

        private bool ButtonEnabled
        {
            get
            {
                // не используется
                throw new NotImplementedException();
            }

            set
            {
                btnOK.Enabled =
                btnBreak.Enabled =
                    value;
            }
        }

        /// <summary>
        /// Метод удаления компонента из таблицы
        /// </summary>
        /// <param name="list_id">Список идентификаторов объекта</param>
        private void delete(TreeView_TECComponent.ID_Comp list_id)
        {
            int id = -1
                , indx = -1;

            if (list_id[TECComponent.ID.TG].Equals (-1) == false) {
                id = list_id [TECComponent.ID.TG];
                indx = (int)FormChangeMode.MODE_TECCOMPONENT.TG;
            } else if (list_id [TECComponent.ID.PC].Equals (-1) == false) {
                id = list_id [TECComponent.ID.PC];
                indx = (int)FormChangeMode.MODE_TECCOMPONENT.PC;
            } else if (list_id [TECComponent.ID.GTP].Equals (-1) == false) {
                id = list_id [TECComponent.ID.GTP];
                indx = (int)FormChangeMode.MODE_TECCOMPONENT.GTP;
            } else if (list_id [TECComponent.ID.TEC].Equals (-1) == false) {
                id = list_id [TECComponent.ID.TEC];
                indx = (int)FormChangeMode.MODE_TECCOMPONENT.TEC;
            } else
                ;

            if ((!(id < 0))
                && (!(indx < 0))) {
                m_arr_editTable [indx].Rows.Remove (m_arr_editTable [indx].Select ("ID=" + id) [0]);
                // включить кнопки
                ButtonEnabled = true;
            } else
                ;
        }

        /// <summary>
        /// Метод обновления связей компонента
        /// </summary>
        /// <param name="list_id">Идентификаторы компонента</param>
        /// <param name="type">Тип изменяемого объекта ИД=1</param>
        private void update(TreeView_TECComponent.ID_Comp list_id, string type)
        {
            int id = -1;
            string field = string.Empty;

            if (list_id[TECComponent.ID.TG].Equals (-1) == false) {
                if (type == FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.GTP)) {
                    id = list_id[TECComponent.ID.GTP];
                    field = "ID_GTP";
                } else
                    ;

                if (type == FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.PC)) {
                    id = (list_id[TECComponent.ID.PC] < 0) ? 0 : list_id[TECComponent.ID.PC];
                    field = "ID_PC";
                } else
                    ;

                if ((!(id < 0))
                    && (field.Equals(string.Empty) == false)) {
                    m_arr_editTable [(int)FormChangeMode.MODE_TECCOMPONENT.TG].Select ($"ID={id}") [0] [field] = id;
                    // включить кнопки
                    ButtonEnabled = true;
                } else
                    ;
            } else
                ;
        }

        /// <summary>
        /// Метод добавления нового компонента
        /// </summary>
        /// <param name="list_id">Идентификатор нового компонента</param>
        private void insert(TreeView_TECComponent.ID_Comp list_id)
        {
            object [] row = null;

            if (list_id[TECComponent.ID.TG].Equals(-1) == false) {
            //Добавление нового ТГ
                row = new object[m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Columns.Count];

                for (int i = 0; i < m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Columns.Count; i++)
                    switch (m_arr_editTable [(int)FormChangeMode.MODE_TECCOMPONENT.TG].Columns [i].ColumnName) {
                        case "NAME_SHR":
                            row [i] = TreeView_TECComponent.Mass_NewVal_Comp (FormChangeMode.MODE_TECCOMPONENT.TG);
                            break;
                        case "ID_PC":
                            row [i] = 0;
                            break;
                        case "ID":
                            row [i] = list_id[TECComponent.ID.TG];
                            break;
                        case "ID_TEC":
                            row [i] = list_id[TECComponent.ID.TEC];
                            break;
                        default:
                            row [i] = -1;
                            break;
                    }

                m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TG].Rows.Add(row);
            } else if (list_id[TECComponent.ID.PC].Equals(-1) == false) {
            //Добавление нового ЩУ
                row = new object[m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Columns.Count];

                for (int i = 0; i < m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Columns.Count; i++)
                    switch (m_arr_editTable [(int)FormChangeMode.MODE_TECCOMPONENT.PC].Columns [i].ColumnName) {
                        case "NAME_SHR":
                            row [i] = TreeView_TECComponent.Mass_NewVal_Comp(FormChangeMode.MODE_TECCOMPONENT.PC);
                            break;
                        case "ID_TEC":
                            row [i] = list_id[TECComponent.ID.TEC];
                            break;
                        case "ID":
                            row [i] = list_id[TECComponent.ID.PC];
                            break;
                        default:
                            row [i] = -1;
                            break;
                    }

                m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.PC].Rows.Add(row);
            } else if (list_id[TECComponent.ID.GTP].Equals(-1) == false) {
            //Добавление новой ГТП
                row = new object[m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Columns.Count];

                for (int i = 0; i < m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Columns.Count; i++)
                    switch (m_arr_editTable [(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Columns [i].ColumnName) {
                        case "NAME_SHR":
                            row [i] = TreeView_TECComponent.Mass_NewVal_Comp (FormChangeMode.MODE_TECCOMPONENT.GTP);
                            break;
                        case "ID_TEC":
                            row [i] = list_id[TECComponent.ID.TEC];
                            break;
                        case "ID":
                            row [i] = list_id[TECComponent.ID.GTP];
                            break;
                        default:
                            row [i] = -1;
                            break;
                    }

                m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].Rows.Add(row);
            } else if (list_id[TECComponent.ID.TEC].Equals(-1) == false) {
            //Добавление новой ТЭЦ
                row = new object[m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Columns.Count];

                for (int i = 0; i < m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Columns.Count; i++)
                    switch (m_arr_editTable [(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Columns [i].ColumnName) {
                        case "NAME_SHR":
                            row [i] = TreeView_TECComponent.Mass_NewVal_Comp (FormChangeMode.MODE_TECCOMPONENT.TEC);
                            break;
                        case "ID":
                            row [i] = list_id[TECComponent.ID.TEC];
                            break;
                        case "InIse":
                            row [i] = 0;
                            break;
                        default:
                            row [i] = -1;
                            break;
                    }

                m_arr_editTable[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].Rows.Add(row);
            }

            ButtonEnabled = !Equals(row, null);
        }

        /// <summary>
        /// Обработчик события выбора элемента в TreeView
        /// </summary>
        private void select(TreeView_TECComponent.ID_Comp list_id)
        {
            int id = -1
                , indx = -1;

            if (list_id[TECComponent.ID.TG].Equals (-1) == false) {
                id = list_id[TECComponent.ID.TG];
                indx = (int)FormChangeMode.MODE_TECCOMPONENT.TG;
            } else if (list_id[TECComponent.ID.PC].Equals (-1) == false) {
                id = list_id[TECComponent.ID.PC];
                indx = (int)FormChangeMode.MODE_TECCOMPONENT.PC;
            } else if (list_id[TECComponent.ID.GTP].Equals (-1) == false) {
                id = list_id[TECComponent.ID.GTP];
                indx = (int)FormChangeMode.MODE_TECCOMPONENT.GTP;
            } else if (list_id[TECComponent.ID.TEC].Equals (-1) == false) {
                id = list_id[TECComponent.ID.TEC];
                indx = (int)FormChangeMode.MODE_TECCOMPONENT.TEC;
            } else
                ;

            if ((!(id < 0))
                && ((!(indx < 0))
                && (indx < m_arr_editTable.Length)))
                dgvProp.update_dgv (id, m_arr_editTable [indx]);
            else
                ;
        }

        /// <summary>
        /// Обработчик события кнопки "Применить"
        /// </summary>
        private void buttonOK_click(object sender, MouseEventArgs e)
        {
            delegateReportClear (true);

            int err = -1;
            string [] warning;
            string nameTable = string.Empty
                , fields = string.Empty;

            if (validate_saving (m_arr_editTable, out warning) == false) {
                for (FormChangeMode.MODE_TECCOMPONENT i = (FormChangeMode.MODE_TECCOMPONENT)0; i < FormChangeMode.MODE_TECCOMPONENT.ANY; i++) {
                    nameTable = $"{FormChangeMode.getPrefixMode (i)}_LIST";
                    fields = (i == FormChangeMode.MODE_TECCOMPONENT.PC) ? "ID_TEC,ID" : "ID";

                    db_sostav.Edit (nameTable, fields, m_arr_originalTable [(short)i], m_arr_editTable [(short)i], out err);
                }

                if (err == 0) {
                    fill_DataTable_ComponentsTEC ();
                    treeView_TECComponent.Update_tree ();

                    FormMain.formParameters.IncIGOVersion ();//Повышение версии состава ТЭЦ
                } else
                    ;

                ButtonEnabled = !(err == 0);
            } else
                delegateWarningReport (string.Concat(warning));

            //TODO: audit
        }

        /// <summary>
        /// Обработчик события кнопки "Отмена"
        /// </summary>
        private void buttonBreak_click(object sender, MouseEventArgs e)
        {
            delegateReportClear(true);
            reset_DataTable_ComponentsTEC();

            treeView_TECComponent.Update_tree();
            ButtonEnabled = false;
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

                                ButtonEnabled = true;
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
        /// <param name="warning">Строка с описанием ошибки</param>
        /// <returns>Возвращает переменную показывающую наличие не введенных параметров</returns>
        private bool validate_saving(DataTable[] mass_table, out string[] warning)
        {
            bool bRes = false;
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
                            bRes = true;
                            warning[indx] += $"Для объекта {row["NAME_SHR"]} параметр {table.Columns[i].ColumnName} равен '-1';{Environment.NewLine}";
                        }
                    }
                }
            }

            return bRes;
        }

        /// <summary>
        /// Обработчик для получения следующего идентификатора
        /// </summary>
        /// <returns>Возвращает идентификатор</returns>
        private int getNextID(TECComponent.ID id)
        {
            int iRes = 0;

            int indxEditTable = -1
                , err = -1;

            switch (id) {
                case TECComponent.ID.TEC:
                    indxEditTable = (int)FormChangeMode.MODE_TECCOMPONENT.TEC;
                    break;
                case TECComponent.ID.PC:
                    indxEditTable = (int)FormChangeMode.MODE_TECCOMPONENT.PC;
                    break;
                case TECComponent.ID.GTP:
                    indxEditTable = (int)FormChangeMode.MODE_TECCOMPONENT.GTP;
                    break;
                case TECComponent.ID.TG:
                    indxEditTable = (int)FormChangeMode.MODE_TECCOMPONENT.TG;
                    break;
                default:
                    break;
            }

            if (!(indxEditTable < 0))
                iRes = DbTSQLInterface.GetIdNext (m_arr_editTable [indxEditTable], out err);
            else
                ;

            return iRes;
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

        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }

            set
            {
                base.ForeColor = value;

                if (Equals (treeView_TECComponent, null) == false)
                    treeView_TECComponent.ForeColor = value;
                else
                    ;

                if (Equals (dgvProp, null) == false)
                    dgvProp.ForeColor = value;
                else
                    ;
            }
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

                if (Equals (treeView_TECComponent, null) == false)
                    treeView_TECComponent.BackColor = value == SystemColors.Control ? SystemColors.Window : value;
                else
                    ;

                if (Equals (dgvProp, null) == false)
                    dgvProp.BackColor =
                    dgvProp.DefaultCellStyle.BackColor =
                        value == SystemColors.Control ? SystemColors.Window : value;
                else
                    ;
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

                List<TEC> listTECRes = DbTSQLConfigDatabase.DbConfig ().InitTEC(true
                    , new int [] { 0, (int)TECComponent.ID.GTP }
                    , false);
                //Инициализация объектов ТГ в каждой ТЭЦ из полученного списка
                foreach (StatisticCommon.TEC t in listTECRes)
                    t.InitSensorsTEC ();

                return listTECRes;
            }

            /// <summary>
            /// Метод выполнения запроса
            /// </summary>
            /// <param name="query">Текст запроса</param>
            /// <param name="err">Возвращаемая ошибка</param>
            /// <returns>Возвращает таблицу с результатом</returns>
            public DataTable Request (string query, out int err)
            {
                return DbTSQLConfigDatabase.DataSource().Select (query
                    , out err);
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
                DbConnection dbConn;

                try {
                    DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings ();
                    DbTSQLConfigDatabase.DbConfig ().Register ();
                    err = 0;
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, "PanelTECComponent.DB_Sostav_TEC::Edit () - получение идентификатора ...", Logging.INDEX_MESSAGE.NOT_SET);
                }

                if (err == 0) {
                    dbConn = DbSources.Sources ().GetConnection (DbTSQLConfigDatabase.DbConfig ().ListenerId, out err);

                    if (err == 0)
                        DbTSQLInterface.RecUpdateInsertDelete (ref dbConn, nameTable, keyField, string.Empty, table_origin, table_edit, out err);
                    else
                        Logging.Logg().Error($"DB_Sostav_TEC::Edit () - получение объекта соединения с БД...", Logging.INDEX_MESSAGE.NOT_SET);

                    DbTSQLConfigDatabase.DbConfig ().UnRegister ();
                } else
                    ;
            }

            /// <summary>
            /// Получение таблицы с компонентами ТЭЦ
            /// </summary>
            /// <param name="mode_TecComp">Идентификатор типа компонента</param>
            /// <returns>Таблицу для компонента</returns>
            public DataTable GetTableTECComponent (FormChangeMode.MODE_TECCOMPONENT mode_TecComp, out int err)
            {
                return Request ($"SELECT * FROM [dbo].[{FormChangeMode.getPrefixMode (mode_TecComp)}_LIST]", out err);
            }

#region Audit

            /// <summary>
            /// Получение последней ревизии аудита
            /// </summary>
            /// <returns>Последняя ревизия аудита</returns>
            public int Get_LastRevision_Audit (out int err)
            {
                err = -1;

                return DbTSQLInterface.GetIdNext (select_table_audit (out err), out err, "REV");
            }

            /// <summary>
            /// Метод для получения из ДБ таблицы Audit
            /// </summary>
            /// <returns>Возвращает таблицу</returns>
            public DataTable select_table_audit (out int err)
            {
                return Request("SELECT [ID],[DATETIME_WR],[ID_USER],[ID_ITEM],[DESCRIPTION],[PREV_VAL],[NEW_VAL],[REV] FROM [dbo].[audit]", out err);
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
                        table_audit.Rows [i] ["ID_USER"] = ASUTP.Helper.HUsers.Id;
                        table_audit.Rows [i] ["REV"] = Get_LastRevision_Audit (out err) + 1;
                    }

                    Edit ("audit", "ID", select_table_audit (out err), table_audit, out err);
                } catch (Exception e) {
                    Logging.Logg ().Exception (e, "PanelTECComponent.DB_Sostav_TEC::Write_Audit () - сохранение таблицы [audit] ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

#endregion

            ///// <summary>
            ///// Регистрация ID
            ///// </summary>
            ///// <param name="err">Ошибка в процессе регистрации</param>
            ///// <returns>Возвращает ID</returns>
            //protected int register_idListenerMainDB (out int err)
            //{
            //    err = -1;
            //    int idListener = -1;

            //    try {
            //        ConnectionSettings connSett = ASUTP.Forms.FormMainBaseWithStatusStrip.s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett ();

            //        idListener = DbSources.Sources ().Register (connSett, false, CONN_SETT_TYPE.CONFIG_DB.ToString ());

            //        err = 0;
            //    } catch (Exception E) {
            //        Logging.Logg ().Exception (E, "Ошибка получения idListener PanelTECComponent : DB_Sostav_TEC : register_idListenerMainDB - ..." + "err = " + err.ToString (), Logging.INDEX_MESSAGE.NOT_SET);
            //    }

            //    return idListener;
            //}
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

            public DataGridView_Prop (Color foreColor, Color backColor)
                : base ()
            {
                BackColor = backColor;
                ForeColor = foreColor;

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

            public class ID_Comp : Dictionary<TECComponent.ID, int>
            {
                private ID_Comp ()
                    : base ()
                {
                    Add (TECComponent.ID.TG, -1);
                    Add (TECComponent.ID.PC, -1);
                    Add (TECComponent.ID.GTP, -1);
                    Add (TECComponent.ID.TEC, -1);
                }

                public ID_Comp (TECComponent.ID key, int value)
                    : this()
                {
                    if (key < TECComponentBase.ID.MAX)
                        if (ContainsKey (key) == true)
                            this [key] = value;
                        else
                            throw new InvalidEnumArgumentException ($"Создание объекта идентификатор компонента: {key.ToString ()}");
                    else
                        ;
                }

                public bool isTG
                {
                    get
                    {
                        return !(this [TECComponent.ID.TG] < 0)
                            && !(this [TECComponent.ID.TEC] < 0);
                    }
                }

                public bool isPC
                {
                    get
                    {
                        return (this [TECComponent.ID.TG] < 0)
                            && !(this [TECComponent.ID.PC] < 0)
                            && (this [TECComponent.ID.GTP] < 0)
                            && !(this [TECComponent.ID.TEC] < 0);
                    }
                }

                public bool isGTP
                {
                    get
                    {
                        return (this [TECComponent.ID.TG] < 0)
                            && (this [TECComponent.ID.PC] < 0)
                            && !(this [TECComponent.ID.GTP] < 0)
                            && !(this [TECComponent.ID.TEC] < 0);
                    }
                }

                public bool isTEC
                {
                    get
                    {
                        return (this [TECComponent.ID.TG] < 0)
                            && (this [TECComponent.ID.PC] < 0)
                            && (this [TECComponent.ID.GTP] < 0)
                            && !(this [TECComponent.ID.TEC] < 0);
                    }
                }

                public TECComponent.ID ID
                {
                    get
                    {
                        return isTEC == true ? TECComponentBase.ID.TEC
                            : isPC == true ? TECComponentBase.ID.PC
                                : isGTP == true ? TECComponentBase.ID.GTP
                                    : isTG == true ? TECComponentBase.ID.TG
                                        : TECComponentBase.ID.MAX;
                    }
                }
            }

            ID_Comp m_selNode_idComp;

            /// <summary>
            /// Список ТЭЦ
            /// </summary>
            List<TEC> m_list_TEC = new List<TEC> ();

            DB_Sostav_TEC db_sostav = new DB_Sostav_TEC ();

            /// <summary>
            /// Идентификаторы для типов компонента ТЭЦ
            /// </summary>
            public enum ID_Menu : int {
                AddGTP = 0, AddPC, AddTG, AddTo, DoRemove, AddTEC
            }

            /// <summary>
            /// Возвратить наименование компонента контекстного меню
            /// </summary>
            /// <param name="indx">Индекс режима</param>
            /// <returns>Строка - наименование режима</returns>
            protected static string getNameMode (ID_Menu indx)
            {
                string [] nameModes = { "Добавить ГТП", "Добавить ЩУ", "Добавить ТГ", "Ввести в состав", "Вывести из состава", "Добавить ТЭЦ" };

                return nameModes [(int)indx];
            }


            public static string NOT_RESOLVED = "Не введенные в состав";

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

            public TreeView_TECComponent (bool contextEnable, Color foreColor, Color backColor)
                : base ()
            {
                BackColor = backColor;
                ForeColor = foreColor;

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
            public static string Mass_NewVal_Comp (FormChangeMode.MODE_TECCOMPONENT indx)
            {
                return new String []{
                        "Новая ТЭЦ", "Новая ГТП", "Новый ЩУ", "Новая ТГ"
                    } [(int)indx];
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
                            else
                                ;
                        } else
                            ;
                    }
                } else
                    ;

                if (set_check == true) {
                    foreach (TreeNode n in node) {
                        if (m_open_node.Count > 0 & i < m_open_node.Count)

                            if (m_open_node [i] == n.Name) {
                                n.Expand ();
                                i++;

                                if (n.FirstNode != null)
                                    checked_node (n.Nodes, i, true);
                                else
                                    ;
                            } else
                                ;
                    }
                } else
                    ;
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

                            for (FormChangeMode.MODE_TECCOMPONENT i = FormChangeMode.MODE_TECCOMPONENT.GTP; i < FormChangeMode.MODE_TECCOMPONENT.ANY; i++) {
                                this.Nodes [tec_indx].Nodes.Add (FormChangeMode.getNameMode (i));
                            }

                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Name = path + ':' + Convert.ToString ((int)TECComponent.ID.GTP);
                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.TG - 1].Name = path + ':' + Convert.ToString ((int)TECComponent.ID.TG);
                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Name = path + ':' + Convert.ToString ((int)TECComponent.ID.PC);

                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes.Add (NOT_RESOLVED);
                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes [0].Name = path + ':' + "0";
                            this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes.Add (NOT_RESOLVED);
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

                                    if ((g.ListLowPointDev [0] as TG).m_id_owner_pc == -1) {
                                        string pc_tg_path = path;

                                        pc_node_null_indx++;
                                        this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes [0].Nodes.Add (g.ListLowPointDev [0].name_shr);
                                        pc_tg_path += ":" + g.ListLowPointDev [0].m_id.ToString ();
                                        this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.PC - 1].Nodes [0].Nodes [pc_node_null_indx].Name = pc_tg_path;
                                    }
                                    if ((g.ListLowPointDev [0] as TG).m_id_owner_gtp == -1) {
                                        string gtp_tg_path = path;

                                        gtp_node_null_indx++;
                                        this.Nodes [tec_indx].Nodes [(int)FormChangeMode.MODE_TECCOMPONENT.GTP - 1].Nodes [0].Nodes.Add (g.ListLowPointDev [0].name_shr);
                                        gtp_tg_path += ":" + g.ListLowPointDev [0].m_id.ToString ();
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

                                    foreach (StatisticCommon.TG h in g.ListLowPointDev) {
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

                                    foreach (StatisticCommon.TG h in g.ListLowPointDev) {
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
                System.Windows.Forms.ToolStripMenuItem вывестиИзСоставаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
                System.Windows.Forms.ToolStripMenuItem добавитьГТПToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
                System.Windows.Forms.ToolStripMenuItem добавитьЩУToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
                System.Windows.Forms.ToolStripMenuItem добавитьТГToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();

#region Нажатие правой кнопкой мыши

                if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                    this.SelectedNode = e.Node;//Выбор компонента при нажатии на него правой кнопкой мыши

                    if (!(m_selNode_idComp[TECComponent.ID.TG] < 0))//выбран ли элемент ТГ
                    {
#region Не введенные
                        if (this.SelectedNode.Parent.Text == TreeView_TECComponent.NOT_RESOLVED) {
#region Context add TG from PC or GTP
                            // 
                            // contextMenu_TreeView_TG_PC
                            // 
                            contextMenu_TreeNode.Items.AddRange (new System.Windows.Forms.ToolStripItem [] {
                                ввестиВСоставToolStripMenuItem
                            });
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

                        if (this.SelectedNode.Parent.Text != TreeView_TECComponent.NOT_RESOLVED) {
                            if (m_selNode_idComp[TECComponent.ID.TG] > (int)TECComponent.ID.TG) {
#region Context delete TG
                                // 
                                // contextMenu_TreeNode
                                // 
                                contextMenu_TreeNode.Items.AddRange (new System.Windows.Forms.ToolStripItem [] {
                                вывестиИзСоставаToolStripMenuItem});
                                contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                                // 
                                // вывыстиИзСоставаToolStripMenuItem
                                //
                                вывестиИзСоставаToolStripMenuItem.Name = "вывыстиИзСоставаToolStripMenuItem";
                                вывестиИзСоставаToolStripMenuItem.Text = "Вывести из состава";
#endregion

                                this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                                this.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler (this.del_TG);
                            }
                        }

#endregion
                    }

#region Добавление компонентов

                    if (m_selNode_idComp[TECComponent.ID.TG] == -1
                        & m_selNode_idComp[TECComponent.ID.PC] == -1
                        & m_selNode_idComp[TECComponent.ID.GTP] == -1
                        & (!(m_selNode_idComp[TECComponent.ID.TEC] < -1)))
                    {
                    //Выбрана ли ТЭЦ
#region Добавление в ТЭЦ компонентов

#region Context TEC
                        // 
                        // contextMenu_TreeView_TEC
                        // 
                        contextMenu_TreeNode.Items.AddRange (new System.Windows.Forms.ToolStripItem [] {
                    добавитьГТПToolStripMenuItem,
                    добавитьЩУToolStripMenuItem,
                    добавитьТГToolStripMenuItem,
                    вывестиИзСоставаToolStripMenuItem});
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
                        вывестиИзСоставаToolStripMenuItem.Name = "вывыстиИзСоставаToolStripMenuItem";
                        вывестиИзСоставаToolStripMenuItem.Text = "Вывести из состава";
#endregion

                        this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;

                        this.SelectedNode.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler (this.add_New_TEC_COMP);

#endregion
                    }

                    if (m_selNode_idComp[TECComponent.ID.GTP] == (int)TECComponent.ID.GTP)//Выбран корень ГТП
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

                    if (m_selNode_idComp[TECComponent.ID.PC] == (int)TECComponent.ID.PC)//Выбран корень ЩУ
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

                    if (m_selNode_idComp[TECComponent.ID.TG] == (int)TECComponent.ID.TG)//Выбран "Поблочно"
                    {
                        this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                    }

                    if (this.SelectedNode.Text == NOT_RESOLVED)//Выбран "Не введенные в состав"
                    {
                        this.SelectedNode.ContextMenuStrip = contextMenu_TreeNode;
                    }

#endregion

#region Удаление из состава

                    if ((m_selNode_idComp[TECComponent.ID.GTP] > (int)TECComponent.ID.GTP & m_selNode_idComp[TECComponent.ID.GTP] < (int)TECComponent.ID.PC & m_selNode_idComp[TECComponent.ID.TG] == -1) || (m_selNode_idComp[TECComponent.ID.PC] > (int)TECComponent.ID.PC & m_selNode_idComp[TECComponent.ID.PC] < (int)TECComponent.ID.TG & m_selNode_idComp[TECComponent.ID.TG] == -1))//Выбран конкретный ЩУ или ГТП
                    {
#region Context delete PC,GTP
                        // 
                        // contextMenu_TreeNode
                        // 
                        contextMenu_TreeNode.Items.AddRange (new System.Windows.Forms.ToolStripItem [] {
                    вывестиИзСоставаToolStripMenuItem});
                        contextMenu_TreeNode.Name = "contextMenu_TreeNode";
                        // 
                        // вывыстиИзСоставаToolStripMenuItem
                        //
                        вывестиИзСоставаToolStripMenuItem.Name = "вывыстиИзСоставаToolStripMenuItem";
                        вывестиИзСоставаToolStripMenuItem.Text = "Вывести из состава";
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
                if (e.ClickedItem.Text == getNameMode (ID_Menu.AddTEC)) {
                    this.Nodes.Add (Mass_NewVal_Comp ((int)FormChangeMode.MODE_TECCOMPONENT.TEC));
                    id_newTEC = GetNextID (TECComponent.ID.TEC);
                    Nodes [Nodes.Count - 1].Name = Convert.ToString (id_newTEC);

                    ID_Comp id = new ID_Comp (TECComponent.ID.TEC, id_newTEC);

                    EditNode (this, new EditNodeEventArgs (id, DbTSQLInterface.QUERY_TYPE.INSERT));
                }
            }

            /// <summary>
            /// Обработчик добавления нового компонента ТЭЦ
            /// </summary>
            private void add_New_TEC_COMP (object sender, ToolStripItemClickedEventArgs e)
            {
                ID_Comp id = null;

                Report (this, new ReportEventArgs (string.Empty, string.Empty, string.Empty, true));

                if (e.ClickedItem.Text == (string)getNameMode (0)) {
                //Добавление новой ГТП
                    id = new ID_Comp (TECComponent.ID.GTP, GetNextID (TECComponent.ID.GTP));
                    id [TECComponent.ID.TEC] = m_selNode_idComp[TECComponent.ID.TEC];

                    EditNode (this, new EditNodeEventArgs (id, DbTSQLInterface.QUERY_TYPE.INSERT));

                    foreach (TreeNode ownerNode in Nodes) {
                        if (Convert.ToInt32 (ownerNode.Name) == m_selNode_idComp[TECComponent.ID.TEC]) {
                            if (ownerNode.FirstNode == null) {
                                ownerNode.Nodes.Add (FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.GTP));
                                ownerNode.Nodes [ownerNode.Nodes.Count - 1].Name = $"{ownerNode.Name}:{(int)TECComponent.ID.GTP}";
                                ownerNode.Nodes [ownerNode.Nodes.Count - 1].Nodes.Add (NOT_RESOLVED);
                                ownerNode.Nodes [ownerNode.Nodes.Count - 1].Nodes [0].Name = $"{ownerNode.Name}:{0}";

                                ownerNode.Nodes.Add (FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.PC));
                                ownerNode.Nodes [ownerNode.Nodes.Count - 1].Name = ownerNode.Name + ':' + (int)TECComponent.ID.PC;
                                ownerNode.Nodes [ownerNode.Nodes.Count - 1].Nodes.Add (NOT_RESOLVED);
                                ownerNode.Nodes [ownerNode.Nodes.Count - 1].Nodes [0].Name = $"{ownerNode.Name}:{0}";

                                ownerNode.Nodes.Add (FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.TG));
                                ownerNode.Nodes [ownerNode.Nodes.Count - 1].Name = $"{ownerNode.Name}:{(int)TECComponent.ID.TG}";
                            }

                            foreach (TreeNode comp in ownerNode.Nodes) {
                                //TODO: по тексту определяется тип узла
                                if (comp.Text == FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.GTP)) {
                                    comp.Nodes.Add (Mass_NewVal_Comp (FormChangeMode.MODE_TECCOMPONENT.GTP));
                                    comp.Nodes [comp.Nodes.Count - 1].Name = $"{ownerNode.Name}:{id[TECComponent.ID.GTP]}";
                                }
                            }
                        }
                    }
                } else if (e.ClickedItem.Text == getNameMode (ID_Menu.AddPC)) {
                //Добавление нового ЩУ
                    id = new ID_Comp (TECComponent.ID.PC, GetNextID (TECComponent.ID.PC));
                    id [TECComponent.ID.TEC] = m_selNode_idComp[TECComponent.ID.TEC];

                    EditNode (this, new EditNodeEventArgs (id, DbTSQLInterface.QUERY_TYPE.INSERT));

                    foreach (TreeNode tec in Nodes) {
                        if (Convert.ToInt32 (tec.Name) == m_selNode_idComp[TECComponent.ID.TEC]) {
                            if (tec.FirstNode == null) {
                                tec.Nodes.Add (FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.GTP));
                                tec.Nodes [tec.Nodes.Count - 1].Name = tec.Name + ':' + (int)TECComponent.ID.GTP;
                                tec.Nodes [tec.Nodes.Count - 1].Nodes.Add (NOT_RESOLVED);
                                tec.Nodes [tec.Nodes.Count - 1].Nodes [0].Name = $"{tec.Name}:{0}";

                                tec.Nodes.Add (FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.PC));
                                tec.Nodes [tec.Nodes.Count - 1].Name = $"{tec.Name}:{(int)TECComponent.ID.PC}";
                                tec.Nodes [tec.Nodes.Count - 1].Nodes.Add (NOT_RESOLVED);
                                tec.Nodes [tec.Nodes.Count - 1].Nodes [0].Name = $"{tec.Name}:{0}";

                                tec.Nodes.Add (FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.TG));
                                tec.Nodes [tec.Nodes.Count - 1].Name = $"{tec.Name}:{(int)TECComponent.ID.TG}";
                            }
                            foreach (TreeNode n in tec.Nodes) {
                                if (n.Text == FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.PC)) {
                                    n.Nodes.Add (Mass_NewVal_Comp (FormChangeMode.MODE_TECCOMPONENT.PC));
                                    n.Nodes [n.Nodes.Count - 1].Name = $"{tec.Name}:{id[TECComponent.ID.PC]}";
                                }
                            }
                        }
                    }
                } else {
                    if (e.ClickedItem.Text == getNameMode (ID_Menu.AddTG)) {
                    //Добавление нового ТГ
                        id = new ID_Comp (TECComponent.ID.TG, GetNextID (TECComponent.ID.TG));
                        id [TECComponent.ID.TEC] = m_selNode_idComp[TECComponent.ID.TEC];
                        id [TECComponent.ID.PC] = 0;

                        EditNode (this, new EditNodeEventArgs (id, DbTSQLInterface.QUERY_TYPE.INSERT));

                        foreach (TreeNode tec in Nodes) {
                            if (Convert.ToInt32 (tec.Name) == m_selNode_idComp[TECComponent.ID.TEC]) {
                                if (tec.FirstNode == null) {
                                    tec.Nodes.Add (FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.GTP));
                                    tec.Nodes [tec.Nodes.Count - 1].Name = $"{tec.Name}:{(int)TECComponent.ID.GTP}";
                                    tec.Nodes [tec.Nodes.Count - 1].Nodes.Add (NOT_RESOLVED);
                                    tec.Nodes [tec.Nodes.Count - 1].Nodes [0].Name = $"{tec.Name}:{0}";

                                    tec.Nodes.Add (FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.PC));
                                    tec.Nodes [tec.Nodes.Count - 1].Name = tec.Name + ':' + (int)TECComponent.ID.PC;
                                    tec.Nodes [tec.Nodes.Count - 1].Nodes.Add (NOT_RESOLVED);
                                    tec.Nodes [tec.Nodes.Count - 1].Nodes [0].Name = $"{tec.Name}:{0}";

                                    tec.Nodes.Add (FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.TG));
                                    tec.Nodes [tec.Nodes.Count - 1].Name = $"{tec.Name}:{(int)TECComponent.ID.TG}";
                                } else
                                    ;

                                foreach (TreeNode n in tec.Nodes) {
                                    if (n.Text == FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.TG)) {
                                        n.Nodes.Add (Mass_NewVal_Comp (FormChangeMode.MODE_TECCOMPONENT.TG));
                                        n.Nodes [n.Nodes.Count - 1].Name = Convert.ToString (n.Parent.Name + ':' + id [TECComponent.ID.TG]);

                                        foreach (TreeNode comp in tec.Nodes) {
                                            if (comp.Text == FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.GTP)) {
                                                if (comp.Nodes.Count == 0)
                                                    comp.Nodes.Add (NOT_RESOLVED);
                                                else
                                                    ;

                                                comp.Nodes [0].Nodes.Add (Mass_NewVal_Comp (FormChangeMode.MODE_TECCOMPONENT.TG));
                                                comp.Nodes [0].Nodes [comp.Nodes [0].Nodes.Count - 1].Name = n.Parent.Name + ':' + id [TECComponent.ID.TG];
                                            }
                                            if (comp.Text == FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.PC)) {
                                                if (comp.Nodes.Count == 0)
                                                    comp.Nodes.Add (NOT_RESOLVED);
                                                else
                                                    ;

                                                comp.Nodes [0].Nodes.Add (Mass_NewVal_Comp (FormChangeMode.MODE_TECCOMPONENT.TG));
                                                comp.Nodes [0].Nodes [comp.Nodes [0].Nodes.Count - 1].Name = n.Parent.Name + ':' + id [TECComponent.ID.TG];
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    } else
                        if (e.ClickedItem.Text == getNameMode (ID_Menu.DoRemove)) {
                        //Выведение (???ТЭЦ) из состава
                            bool del = true;

                            foreach (TreeNode tec in Nodes) {
                                if (Convert.ToInt32 (tec.Name) == m_selNode_idComp[TECComponent.ID.TEC]) {

                                    foreach (TreeNode tc in tec.Nodes) {
                                        if (tc.Nodes.Count > 1
                                            & tc.Text != FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.TG)) {
                                            del = false;
                                            break;
                                        } else
                                            ;

                                        if (tc.Nodes.Count > 0
                                            & tc.Text == FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.TG)) {
                                            del = false;
                                            break;
                                        } else
                                            ;
                                    }

                                }
                            }

                            if (del == true) {
                                EditNode (this, new EditNodeEventArgs (m_selNode_idComp, DbTSQLInterface.QUERY_TYPE.INSERT));
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

                if (SelectedNode.Parent.Parent.Text == (string)FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.GTP)) {
                //Введение ТГ в состав ГТП
                    int id_gtp = 0;

                    foreach (TreeNode t in SelectedNode.Parent.Parent.Nodes) {
                        if (e.ClickedItem.Text == t.Text) {
                            ID_Comp id = get_m_id_list (t.Name);
                            id_gtp = id[TECComponent.ID.GTP];
                        }
                    }

                    foreach (TreeNode tec in Nodes) {
                        if (Convert.ToInt32 (tec.Name) == m_selNode_idComp [TECComponent.ID.TEC]) {
                            foreach (TreeNode n in tec.Nodes) {
                                if (n.Text == FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.GTP)) {
                                    foreach (TreeNode gtp in n.Nodes) {
                                        if (get_m_id_list (gtp.Name) [TECComponent.ID.GTP] == id_gtp) {
                                            string obj = (string)FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.GTP);

                                            gtp.Nodes.Add (SelectedNode.Text);
                                            gtp.Nodes [gtp.Nodes.Count - 1].Name = $"{gtp.Name}:{m_selNode_idComp [TECComponent.ID.TG]}";

                                            EditNode (this, new EditNodeEventArgs (get_m_id_list (gtp.Nodes [gtp.Nodes.Count - 1].Name), DbTSQLInterface.QUERY_TYPE.UPDATE, obj));

                                            SelectedNode.Remove ();
                                        } else
                                            ;
                                    }
                                } else
                                    ;
                            }
                        } else
                            ;
                    }
                } else {
                    if (SelectedNode.Parent.Parent.Text == (string)FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.PC)) {
                    //Введение ТГ в состав ЩУ
                        int id_pc = 0;

                        foreach (TreeNode t in SelectedNode.Parent.Parent.Nodes) {
                            if (e.ClickedItem.Text == t.Text) {
                                ID_Comp id = get_m_id_list (t.Name);
                                id_pc = id [TECComponent.ID.PC];
                            } else
                                ;
                        }

                        foreach (TreeNode tec in Nodes) {
                            if (Convert.ToInt32 (tec.Name) == m_selNode_idComp [TECComponent.ID.TEC]) {
                                foreach (TreeNode n in tec.Nodes) {
                                    if (n.Text == FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.PC)) {
                                        foreach (TreeNode pc in n.Nodes) {
                                            if (get_m_id_list (pc.Name) [TECComponent.ID.PC] == id_pc) {
                                                string obj = (string)FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.PC);

                                                pc.Nodes.Add (SelectedNode.Text);
                                                pc.Nodes [pc.Nodes.Count - 1].Name = $"{pc.Name}:{m_selNode_idComp [TECComponent.ID.TG]}";

                                                EditNode (this, new EditNodeEventArgs (get_m_id_list (pc.Nodes [pc.Nodes.Count - 1].Name), DbTSQLInterface.QUERY_TYPE.UPDATE, obj));

                                                SelectedNode.Remove ();
                                            } else
                                                ;
                                        }
                                    } else
                                        ;
                                }
                            } else
                                ;
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

                if (e.ClickedItem.Text == (string)getNameMode (ID_Menu.DoRemove)) {
                    if (m_selNode_idComp[TECComponent.ID.GTP].Equals (-1) == false & m_selNode_idComp[TECComponent.ID.TG].Equals (-1) == true)//TG
                    {
                        if (SelectedNode.FirstNode == null) {
                            EditNode (this, new EditNodeEventArgs (m_selNode_idComp, DbTSQLInterface.QUERY_TYPE.DELETE));

                            SelectedNode.Remove ();

                        } else {
                            m_warningReport = "Имеются не выведенные из состава компоненты в " + SelectedNode.Text;
                            Report (this, new ReportEventArgs (string.Empty, string.Empty, m_warningReport, false));
                            //MessageBox.Show("Имеются не выведенные из состава компоненты в " + SelectedNode.Text, "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    if (m_selNode_idComp[TECComponent.ID.PC].Equals (-1) == false & m_selNode_idComp[TECComponent.ID.TG].Equals (-1) == true)//PC
                    {
                        if (SelectedNode.FirstNode == null) {
                            EditNode (this, new EditNodeEventArgs (m_selNode_idComp, DbTSQLInterface.QUERY_TYPE.DELETE));

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
                string nameMode = string.Empty;

                Report (this, new ReportEventArgs (string.Empty, string.Empty, string.Empty, true));

                if (SelectedNode.Parent.Parent.Text == (string)FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.GTP))//Выведение ТГ из состава ГТП
                {
                    nameMode = (string)FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.GTP);
                    SelectedNode.Parent.Parent.Nodes [0].Nodes.Add (SelectedNode.Text);
                    SelectedNode.Parent.Parent.Nodes [0].Nodes [SelectedNode.Parent.Parent.Nodes [0].Nodes.Count - 1].Name = Convert.ToString (m_selNode_idComp[TECComponent.ID.TEC]) + ':' + m_selNode_idComp[TECComponent.ID.TG];

                    EditNode (this, new EditNodeEventArgs (get_m_id_list (SelectedNode.Parent.Parent.Nodes [0].Nodes [SelectedNode.Parent.Parent.Nodes [0].Nodes.Count - 1].Name)
                        , DbTSQLInterface.QUERY_TYPE.UPDATE
                        , nameMode));

                    SelectedNode.Remove ();
                }

                if (SelectedNode.Parent.Parent.Text == (string)FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.PC)) {
                //Выведение ТГ из состава ЩУ
                    nameMode = (string)FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.PC);
                    SelectedNode.Parent.Parent.Nodes [0].Nodes.Add (SelectedNode.Text);
                    SelectedNode.Parent.Parent.Nodes [0].Nodes [SelectedNode.Parent.Parent.Nodes [0].Nodes.Count - 1].Name = Convert.ToString (m_selNode_idComp[TECComponent.ID.TEC]) + ':' + m_selNode_idComp[TECComponent.ID.TG];

                    EditNode (this, new EditNodeEventArgs (get_m_id_list (SelectedNode.Parent.Parent.Nodes [0].Nodes [SelectedNode.Parent.Parent.Nodes [0].Nodes.Count - 1].Name)
                        , DbTSQLInterface.QUERY_TYPE.UPDATE
                        , nameMode));


                    SelectedNode.Remove ();
                } else
                    ;

                if (SelectedNode.Parent.Text == (string)FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.TG)) {
                //Выведение ТГ из состава ТЭЦ
                    bool not_delete = false;
                    string warning = string.Empty;

                    foreach (TreeNode tec in Nodes) {
                        if (Convert.ToInt32 (tec.Name) == m_selNode_idComp[TECComponent.ID.TEC]) {
                            foreach (TreeNode tc in tec.Nodes) {
                                if (tc.Text == FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.GTP)
                                    || tc.Text == FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.PC)) {
                                    foreach (TreeNode comp in tc.Nodes) {
                                        if (comp.Text != NOT_RESOLVED) {
                                            foreach (TreeNode tg in comp.Nodes) {
                                                if (get_m_id_list (tg.Name) [TECComponent.ID.TG] == m_selNode_idComp [TECComponent.ID.TG]) {
                                                    not_delete = true;
                                                    warning += SelectedNode.Text + " входит в состав " + comp.Text + "." + '\n';
                                                } else
                                                    ;
                                            }
                                        } else
                                            ;
                                    }
                                } else
                                // не ЩУ, и не ГТП
                                    ;
                            }
                        }

                    }

                    if (not_delete == false) {
                        foreach (TreeNode tec in Nodes) {
                            //TODO: по имени определяется тип. Как?
                            if (Convert.ToInt32 (tec.Name) == m_selNode_idComp [TECComponent.ID.TEC]) {
                                foreach (TreeNode tc in tec.Nodes) {
                                    //TODO: по тексту определяется тип. Как?
                                    if (tc.Text == FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.GTP)
                                        || tc.Text == FormChangeMode.getNameMode (FormChangeMode.MODE_TECCOMPONENT.PC)) {
                                        foreach (TreeNode comp in tc.Nodes) {
                                            if (comp.Text == NOT_RESOLVED) {
                                                foreach (TreeNode tg in comp.Nodes) {
                                                    if (get_m_id_list (tg.Name) [TECComponent.ID.TG] == m_selNode_idComp [TECComponent.ID.TG]) {
                                                        tg.Remove ();
                                                    } else
                                                    // не ТГ
                                                        ;
                                                }
                                            } else
                                            // в составе
                                                ;
                                        } // foreach
                                    } else
                                    // не ЩУ, и не ГТП
                                        ;
                                } // foreach
                            } else
                            // не ТЭЦ
                                ;
                        } // foreach

                        EditNode (this, new EditNodeEventArgs (m_selNode_idComp, DbTSQLInterface.QUERY_TYPE.DELETE));
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

                EditNode (this, new TreeView_TECComponent.EditNodeEventArgs (m_selNode_idComp, DbTSQLInterface.QUERY_TYPE.COUNT_QUERY_TYPE));
            }

            public void Rename_Node (int id_comp, string name)
            {
                foreach (TreeNode tec in Nodes) {
                    if (get_m_id_list (tec.Name)[TECComponent.ID.TEC] == id_comp) {
                        tec.Text = name;
                    } else
                        if (tec.FirstNode != null)
                        foreach (TreeNode tc in tec.Nodes) {
                            if (Equals(tc.FirstNode, null) == false)
                                foreach (TreeNode comp in tc.Nodes) {
                                    if (get_m_id_list (comp.Name) [TECComponent.ID.GTP] == id_comp) {
                                        comp.Text = name;
                                    } else
                                        if (get_m_id_list (comp.Name) [TECComponent.ID.PC] == id_comp) {
                                            comp.Text = name;
                                        } else
                                            if (get_m_id_list (comp.Name) [TECComponent.ID.TG] == id_comp) {
                                                comp.Text = name;
                                            } else
                                                if (Equals(tc.FirstNode, null) == false)
                                                    foreach (TreeNode tg in comp.Nodes)
                                                        if (get_m_id_list (tg.Name) [TECComponent.ID.TG] == id_comp) {
                                                            tg.Text = name;
                                                        } else
                                                            ;
                                                    // foreach
                                                else
                                                    ;
                                } // foreach
                            else
                                ;
                        } // foreach
                } // foreach
            }

            /// <summary>
            /// Метод для запроса ID компонента в TreeView
            /// </summary>
            /// <param name="id_string">Строка с идентификаторами</param>
            /// <returns>Список с ID</returns>
            private ID_Comp get_m_id_list (string id_string)
            {
                ID_Comp id_comp;
                int [] path;

                id_comp = new ID_Comp (TECComponent.ID.MAX, int.MaxValue);

                //id_list[2] = 0;
                if (string.IsNullOrEmpty(id_string) == false) {
                    path = id_string.Split (':').AsEnumerable().Select(id => {
                            return Convert.ToInt32 (id);
                        }).ToArray();

                    for (int i = 0; i < path.Length; i++) {
                        if (!((int)path [i] < (int)TECComponentBase.ID.TEC)
                            & path [i] < (int)TECComponentBase.ID.GTP) {
                            id_comp [TECComponent.ID.TEC] = path [i];
                        } else
                            ;

                        if (!(path [i] < (int)TECComponentBase.ID.GTP)
                            & path [i] < (int)TECComponentBase.ID.PC) {
                            id_comp [TECComponent.ID.GTP] = path [i];
                        } else
                            ;

                        if (!(path [i] < (int)TECComponentBase.ID.PC)
                            & path [i] < (int)TECComponentBase.ID.TG) {
                            id_comp [TECComponent.ID.PC] = path [i];
                        } else
                            ;

                        if (!(path [i] < (int)TECComponentBase.ID.TG)
                            & path [i] < (int)TECComponentBase.ID.MAX) {
                            id_comp [TECComponent.ID.TG] = path [i];
                        } else
                            ;
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
                public DbTSQLInterface.QUERY_TYPE m_Operation;

                /// <summary>
                /// Значение изменяемого параметра
                /// </summary>
                public string m_Value;

                public EditNodeEventArgs (ID_Comp idComp, DbTSQLInterface.QUERY_TYPE operation, string value = null)
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
            /// Событие - получение ID компонента
            /// </summary>
            public event Func<TECComponent.ID, int> GetNextID;

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
