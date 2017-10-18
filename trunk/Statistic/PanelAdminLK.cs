﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;
using System.Globalization;
using System.Drawing;

using HClassLibrary;
using StatisticCommon;

namespace Statistic {
    partial class PanelAdminLK : PanelAdmin
    {
        private System.Windows.Forms.Button btnImportExcel;
        private System.Windows.Forms.Button btnExportExcel;
        
        /// <summary>
        /// Инициализация компонентов на форме
        /// </summary>
        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            this.btnImportExcel = new System.Windows.Forms.Button();
            this.btnExportExcel = new System.Windows.Forms.Button();

            this.m_panelManagement.Controls.Add(this.btnImportExcel);
            this.m_panelManagement.Controls.Add(this.btnExportExcel);

            int posY = 276;
            // 
            // btnImportExcel
            // 
            this.btnImportExcel.Location = new System.Drawing.Point(10, posY);
            this.btnImportExcel.Name = "btnImportExcel";
            this.btnImportExcel.Size = new System.Drawing.Size(154, m_iSizeY);
            this.btnImportExcel.TabIndex = 667;
            this.btnImportExcel.Text = "Импорт из Excel";
            this.btnImportExcel.UseVisualStyleBackColor = true;
            this.btnImportExcel.Click += new System.EventHandler(this.btnImportExcel_Click);
            // 
            // btnExportExcel
            // 
            //this.btnExportExcel.Location = new System.Drawing.Point(10, posY + 1 * (m_iSizeY + 2 * m_iMarginY));
            this.btnExportExcel.Location = new System.Drawing.Point(10, posY + 1 * (m_iSizeY + m_iMarginY));
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(154, m_iSizeY);
            this.btnExportExcel.TabIndex = 668;
            this.btnExportExcel.Text = "Экспорт в Excel";
            this.btnExportExcel.UseVisualStyleBackColor = true;
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
            this.btnExportExcel.Enabled = true;
            this.btnExportExcel.Visible = false;

            this.dgwAdminTable = new DataGridViewAdminLK();
            this.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).BeginInit();
            this.m_panelRDGValues.Controls.Add(this.dgwAdminTable);
            // 
            // dgwAdminTable
            //
            this.dgwAdminTable.Location = new System.Drawing.Point(9, 9);
            this.dgwAdminTable.Size = new System.Drawing.Size(574, 591);
            this.dgwAdminTable.TabIndex = 1;
            //this.dgwAdminTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellClick);
            //this.dgwAdminTable.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellValidated);
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).EndInit();
            this.ResumeLayout();
        }

        //public override Color BackColor
        //{
        //    get
        //    {
        //        return base.BackColor;
        //    }

        //    set
        //    {
        //        base.BackColor = value;

        //        dgwAdminTable.BackColor = value == SystemColors.Control ? SystemColors.Window : value;
        //    }
        //}

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="idListener">ИД слушателя</param>
        /// <param name="markQueries"></param>
        public PanelAdminLK(int idListener, HMark markQueries)
            : base(idListener, markQueries, new int[] { (int)TECComponent.ID.LK, (int)TECComponent.ID.GTP })
        {
            m_admin.SetDelegateSaveComplete(null);
        }

        /// <summary>
        /// Возврв=атить идентификатор (m_id) родительской ГТП для ТГ
        /// </summary>
        /// <param name="indx_tg">Идентификатор ТГ</param>
        /// <returns>Идентификатор ГТП</returns>
        private int getIndexGTPOwner(int indx_tg)
        {
            int iRes = -1
                , id_gtp_owner = ((DataGridViewAdminLK)dgwAdminTable).GetIdGTPOwner(indx_tg);

            foreach (int indx in ((AdminTS_LK)m_admin).m_listTECComponentIndexDetail)
                if (m_admin.allTECComponents[indx].m_id == id_gtp_owner) {
                    return ((AdminTS_LK)m_admin).m_listTECComponentIndexDetail.IndexOf(indx);
                }
                else
                    ;

            return iRes;
        }

        /// <summary>
        /// Метод экспорта значений из DataGridView в список значений компонентов
        /// </summary>
        protected override void getDataGridViewAdmin()
        {
            double value = -1F;
            bool valid = false;
            
            foreach (int indx in ((AdminTS_LK)m_admin).m_listTECComponentIndexDetail) //Перебор компонентов
            {
                if (m_admin.modeTECComponent(indx) == FormChangeMode.MODE_TECCOMPONENT.TG) {
                //Если ТГ, то
                    int indx_tg = ((AdminTS_LK)m_admin).m_listTECComponentIndexDetail.IndexOf(indx),
                        indx_gtp = getIndexGTPOwner(indx_tg);

                    if ((!(indx_tg < 0)) && (!(indx_gtp < 0)))
                        for (int i = 0; i < 24; i++)//Перебор часовых значений ТГ
                        {
                            foreach (DataGridViewColumn col in dgwAdminTable.Columns) //Перебор колонок DataGridView
                                if (m_admin.GetNameTECComponent(indx) == col.HeaderText) //Если имя ТГ соответствует имени колонки
                                    if (dgwAdminTable.Rows[i].Cells[col.Index].Value == null) // , то проверка на пустое поле и запись значения
                                        ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_tg][i].pbr = Convert.ToDouble(0.ToString("F2"));
                                    else
                                        ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_tg][i].pbr = Convert.ToDouble(dgwAdminTable.Rows[i].Cells[col.Index].Value); // '+ 1' за счет DateTime
                                ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_tg][i].pbr_number = "ППБР";
                                //AdminTS.m_sOwner_PBR = 0;
                        }
                    else
                        ;
                }  else if (m_admin.modeTECComponent(indx) == FormChangeMode.MODE_TECCOMPONENT.GTP) {
                //Если ГТП то
                    int indx_gtp = ((AdminTS_LK)m_admin).m_listTECComponentIndexDetail.IndexOf(indx);

                    if (!(indx_gtp < 0))
                        for (int i = 0; i < 24; i++)//Перебор часовых значений ГТП
                        {
                            try {
                                ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_gtp][i].pbr = Convert.ToDouble(dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminLK.DESC_INDEX.PLAN_POWER].Value); // '+ 1' за счет DateTime
                                ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_gtp][i].pmin = Convert.ToDouble(dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminLK.DESC_INDEX.PLAN_TEMPERATURE].Value);

                                if (!(this.dgwAdminTable.Rows[i].Cells[this.dgwAdminTable.Columns.Count - 3].Value == null))
                                    ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_gtp][i].deviationPercent = bool.Parse(this.dgwAdminTable.Rows[i].Cells[this.dgwAdminTable.Columns.Count - 3].Value.ToString());
                                else
                                    ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_gtp][i].deviationPercent = false;

                                valid = double.TryParse((string)this.dgwAdminTable.Rows[i].Cells[this.dgwAdminTable.Columns.Count - 2].Value, out value);
                                ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_gtp][i].deviation = value;
                                ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_gtp][i].pbr_number = "ППБР";

                                //AdminTS.m_sOwner_PBR = 0;
                            } catch (Exception e) {
                                Logging.Logg().Exception(e, string.Format(@"PanelAdminLK::getDataGridViewAdmin () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
                            }
                        }
                    else
                        ;
                }
            }

            //m_admin.CopyCurRDGValues();
        }

        /// <summary>
        /// Заполнить представление значениями 
        ///  , при необходимости переносит выполнение в собственный поток
        ///  для регулирования доступа к элементам управления
        /// </summary>
        /// <param name="date">Дата отображаемых значений</param>
        /// <param name="bNewValues">Признак наличия новых значений, иначе требуется изменить оформление представления</param>
        public override void setDataGridViewAdmin(DateTime date, bool bNewValues)
        {
            if (IsHandleCreated == true)
            {
                if (InvokeRequired == true)
                    this.BeginInvoke (new Action<DateTime, bool, bool> (addTextBoxColumn), date, bNewValues, InvokeRequired);
                else
                    addTextBoxColumn (date, bNewValues, InvokeRequired);
            }
            else
                Logging.Logg().Error(@"PanelTecCurPower::setDataGridViewAdmin () - ... BeginInvoke (addTextBoxColumn) - ...", Logging.INDEX_MESSAGE.D_001);
        }

        /// <summary>
        /// Метод добавления колонок в DataGridView + заполнение значениями ячеек
        /// </summary>
        /// <param name="date">Дата отображаемых значений</param>
        /// <param name="bNewValues">Признак наличия новых значений (false - обновление оформления представления при изменении цветовой схемы)</param>
        /// <param name="bSyncReq">Признак необходимости синхронизации по окончании выполнения метода</param>        
        private void addTextBoxColumn(DateTime date, bool bNewValues, bool bSyncReq)
        {
            int indx = ((AdminTS_LK)m_admin).indxTECComponents;
            //((AdminTS_LK)m_admin).set_CurComponent(0, m_admin.m_curDate);
            //indx = ((AdminTS_LK)m_admin).m_listTECComponentIndexDetail[this.dgwAdminTable.Columns.Count - ((int)DataGridViewAdminLK.DESC_INDEX.COUNT_COLUMN)];
            Color dgwBackColor;

            dgwBackColor = this.dgwAdminTable.BackColor;

            if (m_admin.GetIdTECComponent(indx) > (int)TECComponent.ID.TG) {
            // новый столбец
                ((DataGridViewAdminLK)this.dgwAdminTable).AddTextBoxColumn(m_admin.GetNameTECComponent(indx),
                                                                    m_admin.GetIdTECComponent(indx),
                                                                    m_admin.GetIdGTPOwnerTECComponent(indx),
                                                                    date);

                for (int i = 0; i < 24; i++)
                {
                    //Дата/время
                    if (this.dgwAdminTable.Columns.Count == ((int)DataGridViewAdminLK.DESC_INDEX.COUNT_COLUMN + 1)) //Только при добавлении 1-го столбца
                        this.dgwAdminTable.Rows[i].Cells[0].Value = date.AddHours(i + 1).ToString(@"dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
                    else
                        ;
                    // цвет 0-го столбца изменяем всегда, на случай изменения цветовой гаммы приложения
                    this.dgwAdminTable.Rows [i].Cells [0].Style.BackColor = dgwBackColor;

                    try
                    {
                        this.dgwAdminTable.Rows[i].Cells[this.dgwAdminTable.Columns.Count - 4].Value = ((AdminTS_LK)m_admin).m_listCurRDGValues[indx][i].pbr.ToString("F2");
                        this.dgwAdminTable.Rows [i].Cells [this.dgwAdminTable.Columns.Count - 4].Style.BackColor = dgwBackColor;
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Warning(string.Format("PanelAdminLK : addTextBoxColumn () - нет листа с суточными значениями(снова потерян индекс): {0}", e.Message)
                            , Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    ((DataGridViewAdminLK)this.dgwAdminTable).DataGridViewAdminLK_CellValueChanged(null
                        , new DataGridViewCellEventArgs(this.dgwAdminTable.Columns.Count - 4, i));
                }
                
                visibleControlRDGExcel(((AdminTS_LK)m_admin).GetIdTECOwnerTECComponent(indx));
            } else
                ;

            if ((m_admin.GetIdTECComponent(indx) > (int)TECComponent.ID.GTP)
                && (m_admin.GetIdTECComponent(indx) < (int)TECComponent.ID.PC))
            {                
                for (int i = 0; i < 24; i++)
                {
                    //Дата/время
                    if (this.dgwAdminTable.Columns.Count == ((int)DataGridViewAdminLK.DESC_INDEX.COUNT_COLUMN + 1)) { //Только при добавлении 1-го столбца
                        this.dgwAdminTable.Rows [i].Cells [0].Value = date.AddHours (i + 1).ToString (@"dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);                        
                    } else
                        ;
                    // цвет 0-го столбца изменяем на случай изменения цветовой гаммы приложения
                    this.dgwAdminTable.Rows [i].Cells [0].Style.BackColor = dgwBackColor;

                    try
                    {
                        this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminLK.DESC_INDEX.PLAN_POWER].Value = ((AdminTS_LK)m_admin).m_listCurRDGValues[indx][i].pbr.ToString("F2");
                        this.dgwAdminTable.Rows [i].Cells [(int)DataGridViewAdminLK.DESC_INDEX.PLAN_POWER].Style.BackColor = dgwBackColor;
                        this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminLK.DESC_INDEX.PLAN_TEMPERATURE].Value = ((AdminTS_LK)m_admin).m_listCurRDGValues[indx][i].pmin.ToString("F2");
                        this.dgwAdminTable.Rows [i].Cells [(int)DataGridViewAdminLK.DESC_INDEX.PLAN_TEMPERATURE].Style.BackColor = dgwBackColor;
                        this.dgwAdminTable.Rows[i].Cells[dgwAdminTable.Columns.Count - 3].Value = (bool)(((AdminTS_LK)m_admin).m_listCurRDGValues[indx][i].deviationPercent);
                        this.dgwAdminTable.Rows [i].Cells [dgwAdminTable.Columns.Count - 3].Style.BackColor = dgwBackColor;
                        this.dgwAdminTable.Rows[i].Cells[dgwAdminTable.Columns.Count - 2].Value = ((AdminTS_LK)m_admin).m_listCurRDGValues[indx][i].deviation.ToString("F2");
                        this.dgwAdminTable.Rows [i].Cells [dgwAdminTable.Columns.Count - 2].Style.BackColor = dgwBackColor;
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Warning(string.Format("PanelAdminLK : addTextBoxColumn - нет листа с суточными значениями(снова потерян индекс): {0}", e.Message)
                            , Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    ((DataGridViewAdminLK)this.dgwAdminTable).DataGridViewAdminLK_CellValueChanged(null
                        , new DataGridViewCellEventArgs (this.dgwAdminTable.Columns.Count - 4, i));
                }                
            }

            if (bNewValues == true) {
                m_admin.CopyCurToPrevRDGValues ();
                ((AdminTS_LK)m_admin).m_listPrevRDGValues.Add (m_admin.m_prevRDGValues);
            } else
                ;

            if (bSyncReq == true)
                ((AdminTS_LK)m_admin).m_semaIndxTECComponents.Release ();
            else
                ;
        }

        /// <summary>
        /// Метод для очистки таблиц
        /// </summary>
        public override void ClearTables()
        {
            ((DataGridViewAdminLK)this.dgwAdminTable).ClearTables();//Очистка DataGridView  

            if (((AdminTS_LK)m_admin).m_listPrevRDGValues != null)
                ((AdminTS_LK)m_admin).m_listPrevRDGValues.Clear();//Очистка списка предыдущих значений
        }

        /// <summary>
        /// Заполнение ComboBox значениями ГТП
        /// </summary>
        /// <param name="mode">Переменная типа отображаемых значений</param>
        public override void InitializeComboBoxTecComponent(FormChangeMode.MODE_TECCOMPONENT mode)
        {
            base.InitializeComboBoxTecComponent(mode);

            for (int i = 0; i < m_listTECComponentIndex.Count; i++)
                comboBoxTecComponent.Items.Add(m_admin.allTECComponents[m_listTECComponentIndex[i]].tec.name_shr + " - " + m_admin.GetNameTECComponent(m_listTECComponentIndex[i]));

            if (comboBoxTecComponent.Items.Count > 0)
            {
                m_admin.indxTECComponents = m_listTECComponentIndex[0];
                comboBoxTecComponent.SelectedIndex = 0;
            }
            else ;
        }

        /// <summary>
        /// Метод активации панели
        /// </summary>
        /// <param name="active">Флаг активности панели</param>
        /// <returns>Возвращает текущее состояние активности</returns>
        public override bool Activate(bool active)
        {
            bool bRes = false;

            bRes = base.Activate (active);

            if (bRes == true)
                if (active == true) {
                } else {
                } else
                ;

            return bRes;
        }

        /// <summary>
        /// Метод остановки панели
        /// </summary>
        public override void Stop()
        {
            ClearTables ();

            base.Stop ();
        }
        
        /// <summary>
        /// Метод для активации отображения кнопок импорта и экспорта
        /// </summary>
        /// <param name="id_tec">ID ТЭЦ</param>
        private void visibleControlRDGExcel(int id_tec)
        {
            bool bImpExpButtonVisible = false;

            if ((!(m_listTECComponentIndex == null))
                && (m_listTECComponentIndex.Count > 0)
                && (!(comboBoxTecComponent.SelectedIndex < 0))
                && (m_admin.IsRDGExcel(id_tec /*m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex]*/) == true))
                bImpExpButtonVisible = true;
            else
                ;

            btnImportExcel.Visible = bImpExpButtonVisible;
            //btnExportExcel.Visible = 
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку импорта из Excel
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">Аргумент события</param>
        private void btnImportExcel_Click(object sender, EventArgs e)
        {
            int err = -1;

            //m_admin.ImpRDGExcelValues(m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
            DataTable import_csv = new DataTable();
            OpenFileDialog files = new OpenFileDialog();
            files.Multiselect = false;
            files.InitialDirectory = FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.LK_FOLDER_CSV]; //@"\\ne2844\2.X.X\ПБР-csv"; //@"E:\Temp\ПБР-csv";
            files.DefaultExt = @"xls";
            files.Filter = @"xls файлы (*.xls)|*.xls|xlsx файлы(*.xlsx)|*.xlsx";
            files.Title = "Выберите файл с ПБР...";

            if (files.ShowDialog(FormMain.formParameters) == DialogResult.OK)
            {
                import_csv = ((AdminTS_LK)m_admin).ImportExcel(files.FileName, out err);
            }

            //dgwAdminTable.Rows.Clear();
            for (int i = 0; i < import_csv.Rows.Count; i++)
            {
                for (int b = 0; b < import_csv.Columns.Count; b++)
                {
                    if (b != (int)DataGridViewAdminLK.DESC_INDEX.DATE_HOUR)
                    {
                        dgwAdminTable.Rows[i].Cells[b].Value = (Convert.ToDouble(import_csv.Rows[i][b].ToString().Trim())).ToString("F2");
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку экспорта в Excel
        /// </summary>
        /// <param name="sender">Объект, инициировавший событие</param>
        /// <param name="e">Аргумент события</param>
        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            //FolderBrowserDialog exportFolder = new FolderBrowserDialog();
            //exportFolder.ShowDialog(this);

            //if (exportFolder.SelectedPath.Length > 0) {
                getDataGridViewAdmin();

                Errors resultSaving = m_admin.ExpRDGExcelValues(m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
                if (resultSaving == Errors.NoError)
                {
                    btnRefresh.PerformClick ();
                }
                else
                {
                    if (resultSaving == Errors.InvalidValue)
                        MessageBox.Show(this, "Изменение ретроспективы недопустимо!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        MessageBox.Show(this, "Не удалось сохранить изменения, возможно отсутствует связь с базой данных.", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            //} else ;
        }

        protected override void createAdmin ()
        {
            //Возможность редактирования значений ПБР: НЕ разрешено управление (изменение разрешения на запись), запись разрешена
            m_admin = new PanelAdminLK.AdminTS_LK (new bool[] { false, true });                    
        }

        /// <summary>
        /// Класс для представления административных данных ЛК-дисптчера в табличном виде
        /// </summary>
        public class DataGridViewAdminLK : DataGridViewAdmin {
            /// <summary>
            /// Идентификаторы колонок
            /// </summary>
            public enum DESC_INDEX : ushort { DATE_HOUR, PLAN_TEMPERATURE, PLAN_POWER, DEVIATION_TYPE, DEVIATION, TO_ALL, COUNT_COLUMN };
            /// <summary>
            /// Массив имен колонок
            /// </summary>
            private static string [] arDescStringIndex = { "DateHour", @"PLAN_T", "Plan_P", "DEVIATION_TYPE", "DEVIATION", "TO_ALL" };
            /// <summary>
            /// Массив заголовков колонок
            /// </summary>
            private static string [] arDescRusStringIndex = { "Дата, час", @"Прогн. t", "План P", "Отклонение в процентах", "Величина максимального отклонения", "Дозаполнить" };

            /// <summary>
            /// Массив значений по умолчанию
            /// </summary>
            private static string [] arDefaultValueIndex = { string.Empty, string.Empty, string.Empty, false.ToString (), string.Empty };
            /// <summary>
            /// Перечисление - возможные типы идентификаторов
            /// </summary>
            private enum ID_TYPE : ushort { ID, ID_OWNER, COUNT_ID_TYPE };
            /// <summary>
            /// Список массивов идентификторов:
            ///  1-ый индекс (в списке) - тип идентификатора (ID_TYPE)
            /// </summary>
            private List<int []> m_listIds;
            /// <summary>
            /// Стиль ячеек для плановых значений ГТП (цвет особый, т.к. их редактировать нельзя)
            /// </summary>
            private DataGridViewCellStyle dgvCellStyleReadOnly;

            protected override int INDEX_COLUMN_BUTTON_TO_ALL
            {
                get
                {
                    return (int)DESC_INDEX.TO_ALL;
                }
            }

            /// <summary>
            /// Конструктор
            /// </summary>
            public DataGridViewAdminLK () : base (new Color [] { SystemColors.Window, Color.Yellow, FormMain.formGraphicsSettings.COLOR (FormGraphicsSettings.INDEX_COLOR.DIVIATION) })
            {
                m_listIds = new List<int []> ();
            }

            /// <summary>
            /// Инициализация компонентов DataGridView
            /// </summary>
            protected override void InitializeComponents ()
            {
                base.InitializeComponents ();

                int col = -1;
                Columns.AddRange (new DataGridViewColumn [(int)DESC_INDEX.COUNT_COLUMN] { new DataGridViewTextBoxColumn()
                , new DataGridViewTextBoxColumn()
                , new DataGridViewTextBoxColumn()
                , new DataGridViewCheckBoxColumn()
                , new DataGridViewTextBoxColumn()
                , new DataGridViewButtonColumn() });

                col = 0;
                for (col = 0; col < (int)DESC_INDEX.PLAN_POWER; col++) {
                    Columns [col].Frozen = true;
                    Columns [col].HeaderText = arDescRusStringIndex [col];
                    Columns [col].Name = arDescStringIndex [col];
                    Columns [col].ReadOnly = true;
                    Columns [col].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                }

                for (col = (int)DESC_INDEX.PLAN_POWER; col < (int)DESC_INDEX.DEVIATION_TYPE; col++) {
                    Columns [col].Frozen = true;
                    Columns [col].HeaderText = arDescRusStringIndex [col];
                    Columns [col].Name = arDescStringIndex [col];
                    Columns [col].ReadOnly = false;
                    Columns [col].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                }

                for (col = (int)DESC_INDEX.DEVIATION_TYPE; col < (int)DESC_INDEX.COUNT_COLUMN; col++) {
                    Columns [col].Frozen = false;
                    Columns [col].HeaderText = arDescRusStringIndex [col];
                    Columns [col].Name = arDescStringIndex [col];
                    Columns [col].ReadOnly = false;
                    Columns [col].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                }

                dgvCellStyleReadOnly = new DataGridViewCellStyle ();
                dgvCellStyleReadOnly.BackColor = Color.Yellow;

                this.Dock = DockStyle.Fill;

                this.Columns [INDEX_COLUMN_BUTTON_TO_ALL].DefaultCellStyle.BackColor = SystemColors.Control;
                //this.BackColor = SystemColors.Window;

                this.CellValueChanged += new DataGridViewCellEventHandler (DataGridViewAdminLK_CellValueChanged);

                this.HorizontalScrollBar.Visible = true;
            }

            /// <summary>
            /// Обработчик события выбора ячейки
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="e">Аргумент события</param>
            public void DataGridViewAdminLK_CellValueChanged (object sender, DataGridViewCellEventArgs e)
            {
                int CNT_FIXED_COLUMNS = -1;

                CNT_FIXED_COLUMNS = 3; // за счт столбца "Дата, время", "План t", "План P"

                if (Columns.Count == INDEX_COLUMN_BUTTON_TO_ALL + m_listIds.Count + 1) {
                    // повторный проход, когда все столбцы добавлены
                    for (int col = 0; col < m_listIds.Count; col++)
                        if ((Rows [e.RowIndex].Cells [col + CNT_FIXED_COLUMNS].Style.Equals (s_dgvCellStyles [(int)INDEX_CELL_STYLE.ERROR]) == false)
                            && (Rows [e.RowIndex].Cells [col + CNT_FIXED_COLUMNS].Style.Equals (dgvCellStyleReadOnly) == false))
                            Rows [e.RowIndex].Cells [col + CNT_FIXED_COLUMNS].Style.BackColor = BackColor;
                        else
                            ;
                } else if ((m_listIds.Count == Columns.Count - 4)
                    && (Columns [e.ColumnIndex].ReadOnly == false)
                    && (e.ColumnIndex > 0)
                    && (e.ColumnIndex < Columns.Count - CNT_FIXED_COLUMNS)) {
                    // 1-ый проход при добавлении столбца
                    int id_gtp = m_listIds [e.ColumnIndex - CNT_FIXED_COLUMNS] [(int)ID_TYPE.ID_OWNER],
                        col_gtp = -1;
                    List<int> list_col_tg = new List<int> ();

                    foreach (int [] ids in m_listIds) {
                        //Поиск номера столбца ГТП (только ОДИН раз)
                        if ((col_gtp < 0)
                            && (id_gtp == ids [(int)ID_TYPE.ID])
                            && (ids [(int)ID_TYPE.ID_OWNER] < 0))
                            col_gtp = m_listIds.IndexOf (ids) + CNT_FIXED_COLUMNS; // '+ 3' за счт столбца "Дата, время", "План t", "План P"
                        else
                            ;

                        //Все столбцы для ГТП с id_gtp == ...
                        if (id_gtp == ids [(int)ID_TYPE.ID_OWNER])
                            list_col_tg.Add (m_listIds.IndexOf (ids) + CNT_FIXED_COLUMNS); // '+ 3' за счт столбца "Дата, время", "План t", "План P"
                        else
                            ;
                    }

                    if (list_col_tg.Count > 0) {
                        double plan_gtp = 0.0;
                        foreach (int col in list_col_tg)
                            plan_gtp += Convert.ToDouble (Rows [e.RowIndex].Cells [col].Value);

                        if (Convert.ToDouble (Rows [e.RowIndex].Cells [col_gtp].Value).Equals (plan_gtp) == false)
                            Rows [e.RowIndex].Cells [col_gtp].Style = s_dgvCellStyles [(int)INDEX_CELL_STYLE.ERROR];
                        else if (Rows [e.RowIndex].Cells [col_gtp].Style.BackColor == s_dgvCellStyles [(int)INDEX_CELL_STYLE.ERROR].BackColor)
                            Rows [e.RowIndex].Cells [col_gtp].Style = dgvCellStyleReadOnly;
                        else
                            Rows [e.RowIndex].Cells [col_gtp].Style.BackColor = BackColor;
                    } else
                        ;
                } else
                    ;
            }

            /// <summary>
            /// Метод проверки введенного значения в ячейку
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="e">Аргумент события</param>
            protected override void dgwAdminTable_CellValidated (object sender, DataGridViewCellEventArgs e)
            {
                double value;
                bool valid;

                if ((e.ColumnIndex > 0) && (e.ColumnIndex < Columns.Count - 3)) {
                    valid = double.TryParse ((string)Rows [e.RowIndex].Cells [e.ColumnIndex].Value, out value);
                    if ((valid == false) || (value > DataGridViewAdmin.maxRecomendationValue)) {
                        Rows [e.RowIndex].Cells [e.ColumnIndex].Value = 0.ToString ("F2");
                    } else {
                        Rows [e.RowIndex].Cells [e.ColumnIndex].Value = value.ToString ("F2");
                    }

                    DataGridViewAdminLK_CellValueChanged (sender, e);
                }
            }

            /// <summary>
            /// Обработчик события нажатия на кнопку в ячейке 
            /// </summary>
            /// <param name="sender">Объект, инициировавший событие</param>
            /// <param name="e">Аргумент события</param>
            protected override void dgwAdminTable_CellClick (object sender, DataGridViewCellEventArgs e)
            {
                if ((this.Columns [e.ColumnIndex].Name == arDescStringIndex [(int)DESC_INDEX.TO_ALL]) && (!(e.RowIndex < 0))) // кнопка применение для всех
                {
                    DataGridViewCellEventArgs ev;

                    for (int j = 1; j < Columns.Count - 3; j++) {
                        if (Columns [j].ReadOnly == false)
                            for (int i = e.RowIndex + 1; i < 24; i++) {
                                Rows [i].Cells [j].Value = Rows [e.RowIndex].Cells [j].Value;

                                ev = new DataGridViewCellEventArgs (j, i);
                                DataGridViewAdminLK_CellValueChanged (null, ev);
                            } else
                            ;
                    }
                } else
                    ;
            }

            /// <summary>
            /// Добавить новый столбец/колонку в представление
            /// </summary>
            /// <param name="name">Имя колонки</param>
            /// <param name="id">m_id компонента</param>
            /// <param name="id_owner">m_id компонента-родителя</param>
            /// <param name="date">Дата выбранного значения</param>
            public void AddTextBoxColumn (string name, int id, int id_owner, DateTime date)
            {
                if (id > (int)FormChangeMode.MODE_TECCOMPONENT.TG) {
                    DataGridViewTextBoxColumn insColumn = new DataGridViewTextBoxColumn ();
                    insColumn.Frozen = false;
                    insColumn.Width = 66;
                    insColumn.HeaderText = name;
                    insColumn.Name = "column" + (Columns.Count - 3);
                    insColumn.ReadOnly = false;
                    insColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                    try {
                        Columns.Insert (Columns.Count - 3, insColumn);
                    } catch (Exception e) {
                        Logging.Logg ().Exception (e, "dgwAdminLK - addTextBoxColumn () - Columns.Insert", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    m_listIds.Add (new int [(int)ID_TYPE.COUNT_ID_TYPE] { id, id_owner });

                    Columns [Columns.Count - 3 - 1].Frozen = true;
                    Columns [Columns.Count - 3 - 1].ReadOnly = true;
                }
            }

            /// <summary>
            /// Метод очистки таблицы
            /// </summary>
            public override void ClearTables ()
            {
                int i = -1;

                m_listIds.Clear ();

                while (Columns.Count > (int)DESC_INDEX.COUNT_COLUMN) {
                    Columns.RemoveAt (Columns.Count - (int)DESC_INDEX.DEVIATION_TYPE - 1);
                }

                this.Rows.Clear ();
                for (i = 0; i < 24; i++) {
                    this.Rows.Add ();
                }
            }

            /// <summary>
            /// Найти идентификатор компонента по m_id родителя
            /// </summary>
            /// <param name="id">Идентификатор родительского компонента</param>
            /// <returns>Идентификатор компонента</returns>
            public int GetIdGTPOwner (int id)
            {
                int iRes = -1;

                if ((id < m_listIds.Count) && ((int)ID_TYPE.ID_OWNER < m_listIds [id].Length))
                    iRes = m_listIds [id] [(int)ID_TYPE.ID_OWNER];
                else
                    ;

                return iRes;
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

                    if ((INDEX_COLUMN_BUTTON_TO_ALL > 0)
                        && (RowCount > 0))
                        for (int col = 0; col < ColumnCount - 1; col++)
                            for (int i = 0; i < RowCount; i++) {
                                // ограничений на изменение цвета фона в ячейке нет
                                // например, сигнализация о выходе за пределы некоторых значений - цвет таких ячеек изменять нельзя
                                Rows [i].Cells [col].Style.BackColor = value == SystemColors.Control ? SystemColors.Window : value;
                            } else
                        // нет столбцов/строк - нет действий по изменению цвета фона ячеек
                        ;
                }
            }
        }
    }
}
