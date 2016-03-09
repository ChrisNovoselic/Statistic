using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Drawing;

using HClassLibrary;
using StatisticCommon;
using StatisticAlarm;
using GemBox.Spreadsheet;

namespace Statistic
{
    class PanelAdminLK : PanelAdmin
    {
        private System.Windows.Forms.Button btnImportExcel;
        private System.Windows.Forms.Button btnExportExcel;
        
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

        public PanelAdminLK(int idListener, HMark markQueries)
            : base(idListener, FormChangeMode.MANAGER.LK, markQueries)
        {
            m_admin.SetDelegateSaveComplete(null);

        }

        private int GetIndexGTPOwner(int indx_tg)
        {
            int iRes = -1
                , id_gtp_owner = ((DataGridViewAdminLK)dgwAdminTable).GetIdGTPOwner(indx_tg);

            foreach (int indx in ((AdminTS_LK)m_admin).m_listTECComponentIndexDetail)
            {
                if (m_admin.allTECComponents[indx].m_id == id_gtp_owner) {
                    return ((AdminTS_LK)m_admin).m_listTECComponentIndexDetail.IndexOf(indx);
                }
                else
                    ;
            }

            return iRes;
        }

        protected override void getDataGridViewAdmin()
        {
            double value;
            bool valid;

            foreach (int indx in ((AdminTS_LK)m_admin).m_listTECComponentIndexDetail)
            {
                if (m_admin.modeTECComponent(indx) == FormChangeMode.MODE_TECCOMPONENT.TG)
                {
                    int indx_tg = ((AdminTS_LK)m_admin).m_listTECComponentIndexDetail.IndexOf(indx),
                        indx_gtp = GetIndexGTPOwner(indx_tg);

                    if ((!(indx_tg < 0)) && (!(indx_gtp < 0)))
                        for (int i = 0; i < 24; i++)
                        {
                            foreach (DataGridViewColumn col in dgwAdminTable.Columns)
                                if (m_admin.GetNameTECComponent(indx) == col.HeaderText)
                                    if (dgwAdminTable.Rows[i].Cells[col.Index].Value == null)
                                        ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_tg][i].pbr = Convert.ToDouble(0.ToString("F2"));
                                    else
                                        ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_tg][i].pbr = Convert.ToDouble(dgwAdminTable.Rows[i].Cells[col.Index].Value); // '+ 1' за счет DateTime
                            ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_tg][i].pbr_number = "ППБР";
                            AdminTS.m_sOwner_PBR = 0;
                        }
                    else
                        ;
                }
                else
                {
                    if (m_admin.modeTECComponent(indx) == FormChangeMode.MODE_TECCOMPONENT.GTP)
                    {
                        int indx_gtp = ((AdminTS_LK)m_admin).m_listTECComponentIndexDetail.IndexOf(indx);

                        if (!(indx_gtp < 0))
                            for (int i = 0; i < 24; i++)
                            {
                                ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_gtp][i].pbr = Convert.ToDouble(dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminLK.DESC_INDEX.PLAN].Value); // '+ 1' за счет DateTime
                                ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_gtp][i].pmin = Convert.ToDouble(dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminLK.DESC_INDEX.PLAN_T].Value);
                                
                                if (!(this.dgwAdminTable.Rows[i].Cells[this.dgwAdminTable.Columns.Count - 3].Value == null))
                                    ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_gtp][i].deviationPercent = bool.Parse(this.dgwAdminTable.Rows[i].Cells[this.dgwAdminTable.Columns.Count - 3].Value.ToString());
                                else
                                    ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_gtp][i].deviationPercent = false;

                                valid = double.TryParse((string)this.dgwAdminTable.Rows[i].Cells[this.dgwAdminTable.Columns.Count - 2].Value, out value);
                                ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_gtp][i].deviation = value;
                                ((AdminTS_LK)m_admin).m_listCurRDGValues[indx_gtp][i].pbr_number = "ППБР";
                                AdminTS.m_sOwner_PBR = 0;
                            }
                        else
                            ;
                    }
                }
            }

            //m_admin.CopyCurRDGValues();
        }

        public override void setDataGridViewAdmin(DateTime date)
        {
            if (IsHandleCreated/*InvokeRequired*/ == true)
            {
                this.BeginInvoke(new DelegateDateFunc(addTextBoxColumn), date);
            }
            else
                Logging.Logg().Error(@"PanelTecCurPower::setDataGridViewAdmin () - ... BeginInvoke (addTextBoxColumn) - ...", Logging.INDEX_MESSAGE.D_001);
        }

        private void addTextBoxColumn(DateTime date)
        {
            int indx = ((AdminTS_LK)m_admin).indxTECComponents;
            //((AdminTS_LK)m_admin).set_CurComponent(0, m_admin.m_curDate);
            //indx = ((AdminTS_LK)m_admin).m_listTECComponentIndexDetail[this.dgwAdminTable.Columns.Count - ((int)DataGridViewAdminLK.DESC_INDEX.COUNT_COLUMN)];

            
            if (m_admin.GetIdTECComponent(indx) > (int)TECComponent.ID.TG)
            {
                ((DataGridViewAdminLK)this.dgwAdminTable).addTextBoxColumn(m_admin.GetNameTECComponent(indx),
                                                                    m_admin.GetIdTECComponent(indx),
                                                                    m_admin.GetIdGTPOwnerTECComponent(indx),
                                                                    date);
                DataGridViewCellEventArgs ev;

                for (int i = 0; i < 24; i++)
                {

                    if (this.dgwAdminTable.Columns.Count == ((int)DataGridViewAdminLK.DESC_INDEX.COUNT_COLUMN + 1)) //Только при добавлении 1-го столбца
                    {
                        this.dgwAdminTable.Rows[i].Cells[0].Value = date.AddHours(i + 1).ToString(@"dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
                    }
                    else
                        ;

                    try
                    {
                        this.dgwAdminTable.Rows[i].Cells[this.dgwAdminTable.Columns.Count - 4].Value = ((AdminTS_LK)m_admin).m_listCurRDGValues[indx][i].pbr.ToString("F2");
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Warning("PanelAdminLK : addTextBoxColumn - нет листа с суточными значениями(снова потерялся индекс)" + e.Message, Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    ev = new DataGridViewCellEventArgs(this.dgwAdminTable.Columns.Count - 4, i);

                    ((DataGridViewAdminLK)this.dgwAdminTable).DataGridViewAdminLK_CellValueChanged(null, ev);

                }
                
                visibleControlRDGExcel(((AdminTS_LK)m_admin).GetIdTECOwnerTECComponent(indx));

            }
            if (m_admin.GetIdTECComponent(indx) > (int)TECComponent.ID.GTP & m_admin.GetIdTECComponent(indx) < (int)TECComponent.ID.PC)
            {
                DataGridViewCellEventArgs ev;
                for (int i = 0; i < 24; i++)
                {

                    if (this.dgwAdminTable.Columns.Count == ((int)DataGridViewAdminLK.DESC_INDEX.COUNT_COLUMN + 1)) //Только при добавлении 1-го столбца
                        this.dgwAdminTable.Rows[i].Cells[0].Value = date.AddHours(i + 1).ToString(@"dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
                    else
                        ;
                    try
                    {
                        this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminLK.DESC_INDEX.PLAN].Value = ((AdminTS_LK)m_admin).m_listCurRDGValues[indx][i].pbr.ToString("F2");
                        this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminLK.DESC_INDEX.PLAN_T].Value = ((AdminTS_LK)m_admin).m_listCurRDGValues[indx][i].pmin.ToString("F2");
                        this.dgwAdminTable.Rows[i].Cells[dgwAdminTable.Columns.Count - 3].Value = (bool)(((AdminTS_LK)m_admin).m_listCurRDGValues[indx][i].deviationPercent);
                        this.dgwAdminTable.Rows[i].Cells[dgwAdminTable.Columns.Count - 2].Value = ((AdminTS_LK)m_admin).m_listCurRDGValues[indx][i].deviation.ToString("F2");
                    
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().Warning("PanelAdminLK : addTextBoxColumn - нет листа с суточными значениями(снова потерялся индекс)" + e.Message, Logging.INDEX_MESSAGE.NOT_SET);
                    }
                        ev = new DataGridViewCellEventArgs(this.dgwAdminTable.Columns.Count - 4, i);


                    ((DataGridViewAdminLK)this.dgwAdminTable).DataGridViewAdminLK_CellValueChanged(null, ev);

                }
                
            }
            m_admin.CopyCurToPrevRDGValues();
            ((AdminTS_LK)m_admin).m_listPrevRDGValues.Add(m_admin.m_prevRDGValues);
            ((AdminTS_LK)m_admin).m_semaIndxTECComponents.Release();
        }

        public override void ClearTables()
        {
            ((DataGridViewAdminLK)this.dgwAdminTable).ClearTables();
            ((AdminTS_LK)m_admin).m_listPrevRDGValues.Clear();
        }

        public override void InitializeComboBoxTecComponent(FormChangeMode.MODE_TECCOMPONENT mode)
        {
            base.InitializeComboBoxTecComponent(mode);

            for (int i = 0; i < m_listTECComponentIndex.Count; i++)
            {

                comboBoxTecComponent.Items.Add(m_admin.allTECComponents[m_listTECComponentIndex[i]].tec.name_shr + " - " + m_admin.GetNameTECComponent(m_listTECComponentIndex[i]));
            }

            if (comboBoxTecComponent.Items.Count > 0)
            {
                m_admin.indxTECComponents = m_listTECComponentIndex[0];
                comboBoxTecComponent.SelectedIndex = 0;
            }
            else ;
        }

        public override bool Activate(bool active)
        {
            bool bRes = false;

            if (active == true)
            {
            }
            else 
            {
            }

            bRes = base.Activate(active);

            return bRes;
        }

        public override void Stop()
        {
            ClearTables ();

            base.Stop ();
        }
        
        private void visibleControlRDGExcel(int id_tec)
        {
            bool bImpExpButtonVisible = false;

            if ((!(m_listTECComponentIndex == null)) && (m_listTECComponentIndex.Count > 0) && (!(comboBoxTecComponent.SelectedIndex < 0)) && (m_admin.IsRDGExcel(id_tec /*m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex]*/) == true))
                bImpExpButtonVisible = true;
            else
                ;

            btnImportExcel.Visible = bImpExpButtonVisible;
            //btnExportExcel.Visible = 
        }

        private void btnImportExcel_Click(object sender, EventArgs e)
        {
            //ClearTables();

            //m_admin.ImpRDGExcelValues(m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
            DataTable import_csv = new DataTable();
            OpenFileDialog files = new OpenFileDialog();
            files.Multiselect = false;
            files.InitialDirectory = FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.KOMDISP_FOLDER_CSV]; //@"\\ne2844\2.X.X\ПБР-csv"; //@"E:\Temp\ПБР-csv";
            files.DefaultExt = @"xls";
            files.Filter = @"xls файлы (*.xls)|*.xls|xlsx файлы(*.xlsx)|*.xlsx";
            files.Title = "Выберите файл с ПБР...";

            if (files.ShowDialog(FormMain.formParameters) == DialogResult.OK)
            {
                import_csv = Get_DataTable_From_Excel.getDT(files.FileName, m_admin.m_curDate.Date);
            }

            //dgwAdminTable.Rows.Clear();
            for (int i = 0; i < import_csv.Rows.Count; i++)
            {
                for (int b = 0; b < import_csv.Columns.Count; b++)
                {
                    if (b != (int)Get_DataTable_From_Excel.INDEX_COLUMN.Time)
                    {
                        dgwAdminTable.Rows[i].Cells[b].Value = (Convert.ToDouble(import_csv.Rows[i][b].ToString().Trim())).ToString("F2");
                    }
                }
            }
        }

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
    }

    public class DataGridViewAdminLK : DataGridViewAdmin
    {
        public enum DESC_INDEX : ushort { DATE_HOUR, PLAN, PLAN_T, DEVIATION_TYPE, DEVIATION, TO_ALL, COUNT_COLUMN };
        private static string[] arDescStringIndex = { "DateHour", "Plan", @"PLAN_T", "DEVIATION_TYPE", "DEVIATION", "TO_ALL" };
        private static string[] arDescRusStringIndex = { "Дата, час", "План", @"План t", "Отклонение в процентах", "Величина максимального отклонения", "Дозаполнить" };
        private static string[] arDefaultValueIndex = { string.Empty, string.Empty, string.Empty, false.ToString(), string.Empty };

       private enum ID_TYPE : ushort { ID, ID_OWNER, COUNT_ID_TYPE };

        private List <int []> m_listIds;

        DataGridViewCellStyle dgvCellStyleError,
                             dgvCellStyleGTP;

        public DataGridViewAdminLK()
        {
            m_listIds = new List<int[]>();

            dgvCellStyleError = new DataGridViewCellStyle();
            dgvCellStyleError.BackColor = Color.Red;

            dgvCellStyleGTP = new DataGridViewCellStyle();
            dgvCellStyleGTP.BackColor = Color.Yellow;

            //this.Anchor |= (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Right);
            this.Dock = DockStyle.Fill;

            this.CellValueChanged +=new DataGridViewCellEventHandler(DataGridViewAdminLK_CellValueChanged);

            this.HorizontalScrollBar.Visible = true;
        }
        
        protected override void InitializeComponents () 
        {
            base.InitializeComponents ();

            int col = -1;
            Columns.AddRange(new DataGridViewColumn[(int)DESC_INDEX.COUNT_COLUMN] { new DataGridViewTextBoxColumn(), new DataGridViewTextBoxColumn(), new DataGridViewTextBoxColumn(), new DataGridViewCheckBoxColumn(), new DataGridViewTextBoxColumn(), new DataGridViewButtonColumn() });
            col = 0;
            for (col = 0; col < (int)DESC_INDEX.PLAN; col++)
            {
                    Columns[col].Frozen = true;
                    Columns[col].HeaderText = arDescRusStringIndex[col];
                    Columns[col].Name = arDescStringIndex[col];
                    Columns[col].ReadOnly = true;
                    Columns[col].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            }
            for (col = (int)DESC_INDEX.PLAN; col < (int)DESC_INDEX.DEVIATION_TYPE; col++)
            {
                Columns[col].Frozen = true;
                Columns[col].HeaderText = arDescRusStringIndex[col];
                Columns[col].Name = arDescStringIndex[col];
                Columns[col].ReadOnly = false;
                Columns[col].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            }
            for (col = (int)DESC_INDEX.DEVIATION_TYPE; col < (int)DESC_INDEX.COUNT_COLUMN; col++)
            {
                Columns[col].Frozen = false;
                Columns[col].HeaderText = arDescRusStringIndex[col];
                Columns[col].Name = arDescStringIndex[col];
                Columns[col].ReadOnly = true;
                Columns[col].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            }
        }

        public void DataGridViewAdminLK_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
                if ((m_listIds.Count == Columns.Count - 4) && (Columns[e.ColumnIndex].ReadOnly == false) && (e.ColumnIndex > 0) && (e.ColumnIndex < Columns.Count - 3))
                {
                    int id_gtp = m_listIds[e.ColumnIndex - 3][(int)ID_TYPE.ID_OWNER],
                        col_gtp = -1;
                    List<int> list_col_tg = new List<int>();
                    foreach (int[] ids in m_listIds)
                    {
                        //Поиск номера столбца ГТП (только ОДИН раз)
                        if ((col_gtp < 0) && (id_gtp == ids[(int)ID_TYPE.ID]) && (ids[(int)ID_TYPE.ID_OWNER] < 0))
                            col_gtp = m_listIds.IndexOf(ids) + 3; // '+ 1' за счт столбца "Дата, время"
                        else
                            ;

                        //Все столбцы для ГТП с id_gtp == ...
                        if (id_gtp == ids[(int)ID_TYPE.ID_OWNER])
                            list_col_tg.Add(m_listIds.IndexOf(ids) + 3); // '+ 1' за счт столбца "Дата, время"
                        else
                            ;
                    }

                    if (list_col_tg.Count > 0)
                    {
                        double plan_gtp = 0.0;
                        foreach (int col in list_col_tg)
                        {
                            plan_gtp += Convert.ToDouble(Rows[e.RowIndex].Cells[col].Value);
                        }

                        if (Convert.ToDouble(Rows[e.RowIndex].Cells[col_gtp].Value).Equals(plan_gtp) == false)
                        {
                            Rows[e.RowIndex].Cells[col_gtp].Style = dgvCellStyleError;
                        }
                        else
                            if (Rows[e.RowIndex].Cells[col_gtp].Style.BackColor == dgvCellStyleError.BackColor)
                                Rows[e.RowIndex].Cells[col_gtp].Style = dgvCellStyleGTP;
                            else
                                ;
                    }
                    else
                        ;
                }
                else
                    ;
        }
        
        protected override void dgwAdminTable_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            double value;
            bool valid;

            if ((e.ColumnIndex > 0) && (e.ColumnIndex < Columns.Count - 3) /*&& (e.ColumnIndex != (int)DESC_INDEX.PLAN_T)*/)
            {
                valid = double.TryParse((string)Rows[e.RowIndex].Cells[e.ColumnIndex].Value, out value);
                if ((valid == false) || (value > DataGridViewAdmin.maxRecomendationValue))
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0.ToString("F2");
                }
                else
                {
                    Rows[e.RowIndex].Cells[e.ColumnIndex].Value = value.ToString("F2");
                }

                DataGridViewAdminLK_CellValueChanged (sender, e);
            }
            else
                ;
        }

        protected override void dgwAdminTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((this.Columns[e.ColumnIndex].Name == arDescStringIndex[(int)DESC_INDEX.TO_ALL]) && (!(e.RowIndex < 0))) // кнопка применение для всех
            {
                DataGridViewCellEventArgs ev;

                for (int j = 1; j < Columns.Count - 3; j++)
                {
                    if (Columns[j].ReadOnly == false)
                        for (int i = e.RowIndex + 1; i < 24; i++)
                        {
                            Rows[i].Cells[j].Value = Rows[e.RowIndex].Cells[j].Value;

                            ev = new DataGridViewCellEventArgs(j, i);
                            DataGridViewAdminLK_CellValueChanged(null, ev);
                        }
                    else
                        ;
                }
            }
            else
                ;
        }

        public void addTextBoxColumn(string name, int id, int id_owner, DateTime date)
        {
            if (id > (int)FormChangeMode.MODE_TECCOMPONENT.TG)
            {
                DataGridViewTextBoxColumn insColumn = new DataGridViewTextBoxColumn();
                insColumn.Frozen = false;
                insColumn.Width = 66;
                insColumn.HeaderText = name;
                insColumn.Name = "column" + (Columns.Count - 3);
                insColumn.ReadOnly = false;
                insColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                try
                {
                    Columns.Insert(Columns.Count - 3, insColumn);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, "dgwAdminLK - addTextBoxColumn () - Columns.Insert", Logging.INDEX_MESSAGE.NOT_SET);
                }

                m_listIds.Add(new int[(int)ID_TYPE.COUNT_ID_TYPE] { id, id_owner });

                Columns[Columns.Count - 3 - 1].Frozen = true;
                Columns[Columns.Count - 3 - 1].ReadOnly = true;
            }
        }

        public override void ClearTables () {
            int i = -1;

            m_listIds.Clear ();
            
            while (Columns.Count > (int)DESC_INDEX.COUNT_COLUMN)
            {
                Columns.RemoveAt(Columns.Count - (int)DESC_INDEX.DEVIATION_TYPE - 1);
            }
            
            this.Rows.Clear();
            for (i = 0; i < 24; i++)
            {
                this.Rows.Add();
            }
        }

        public int GetIdGTPOwner(int i)
        {
            int iRes = -1;

            if ((i < m_listIds.Count) && ((int)ID_TYPE.ID_OWNER < m_listIds[i].Length))
                iRes = m_listIds [i][(int)ID_TYPE.ID_OWNER];
            else
                ;

            return iRes;
        }
    }

    public class Get_DataTable_From_Excel
    {
        public static string[] mounth = {"UNKNOWN","ЯНВАРЬ","ФЕВРАЛЬ","МАРТ","АПРЕЛЬ","МАЙ","ИЮНЬ","ИЮЛЬ","АВГУСТ","СЕНТЯБРЬ","ОКТЯБРЬ","НОЯБРЬ","ДЕКАБРЬ" };
        public static string[] col_name = { "Час", "Значение", "Температура" };
        public enum INDEX_COLUMN : int {Time, Value, Temp }
        public Get_DataTable_From_Excel()
        {
        }

        public static DataTable getDT(string path, DateTime data) 
        {
            DataTable dtExportCSV = new DataTable();

            dtExportCSV = getCSV(path, data);

            return dtExportCSV;
        }

        private static DataTable getCSV(string path, DateTime date)
        {
            DataTable dataTableRes = new DataTable();
            string name_worksheet = string.Empty;
            DataRow[] rows = new DataRow[25];

            for (int i = 0; i < col_name.Length; i++)
                dataTableRes.Columns.Add(col_name[i]);

            //object[] column;
            //Открыть поток чтения файла...
            try
            {
                ExcelFile excel = new ExcelFile();
                excel.LoadXls(path);
                name_worksheet = "Расчет " + mounth[date.Month];
                foreach (ExcelWorksheet w in excel.Worksheets)
                {
                    if (w.Name.Equals(name_worksheet, StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (ExcelRow r in w.Rows)
                        {
                            if(r.Cells[0].Value!=null)
                            if (r.Cells[0].Value.ToString() == date.Date.ToString())
                            {
                                for (int i = 0; i < 24; i++)
                                {
                                    object[] row = new object[3];
                                    row[0] = i.ToString();
                                    if (r.Cells[i + 2].Value == null)
                                        row[1] = 0.ToString("F2");
                                    else
                                        row[1] = r.Cells[i + 2].Value.ToString().Trim();

                                    if (w.Rows[r.Index + 1].Cells[i + 2].Value == null)
                                        row[2] = string.Empty;
                                    else
                                        row[2] = w.Rows[r.Index + 1].Cells[i + 2].Value.ToString().Trim();
                                 
                                    dataTableRes.Rows.Add(row);
                                }
                            }
                            if (dataTableRes.Rows.Count >= 24) 
                                break;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Logging.Logg().Error("PanelAdminLK : getCSV - ошибка при открытии потока" + e.Message, Logging.INDEX_MESSAGE.NOT_SET);
            }

            return dataTableRes;
        }
    }
}
