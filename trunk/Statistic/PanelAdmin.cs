using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Globalization;

using StatisticCommon;

namespace Statistic
{
    public class PanelAdmin : Panel
    {
        private System.Windows.Forms.MonthCalendar mcldrDate;

        private DataGridViewAdminNSS dgwAdminTable;

        private System.Windows.Forms.Button btnSet;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnImportExcel;
        private System.Windows.Forms.Button btnExportExcel;

        private System.Windows.Forms.ComboBox comboBoxTecComponent;
        private System.Windows.Forms.GroupBox gbxDivider;

        private Admin m_admin;
        
        List <int>m_listTECComponentIndex;
        private volatile int prevSelectedIndex;

        public bool isActive;

        private void InitializeComponents()
        {
            this.btnSet = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnImportExcel = new System.Windows.Forms.Button();
            this.btnExportExcel = new System.Windows.Forms.Button();
            //this.btnLoadLayout = new System.Windows.Forms.Button();

            this.dgwAdminTable = new DataGridViewAdminNSS();
            this.mcldrDate = new System.Windows.Forms.MonthCalendar();
            this.comboBoxTecComponent = new System.Windows.Forms.ComboBox();
            this.gbxDivider = new System.Windows.Forms.GroupBox();
            
            this.SuspendLayout();
            
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).BeginInit();

            this.Controls.Add(this.btnSet);
            this.Controls.Add(this.btnRefresh);
            //this.Controls.Add(this.btnLoadLayout);
            this.Controls.Add(this.btnImportExcel);
            this.Controls.Add(this.btnExportExcel);
            this.Controls.Add(this.btnRefresh);
            
            this.Controls.Add(this.dgwAdminTable);
            
            this.Controls.Add(this.mcldrDate);
            this.Controls.Add(this.comboBoxTecComponent);
            this.Controls.Add(this.gbxDivider);
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Location = new System.Drawing.Point(8, 8);
            this.Name = "pnlAdmin";
            this.Size = new System.Drawing.Size(760, 610);
            this.TabIndex = 1;
            // 
            // mcldrDate
            // 
            this.mcldrDate.Location = new System.Drawing.Point(10, 38);
            this.mcldrDate.Name = "mcldrDate";
            this.mcldrDate.TabIndex = 0;
            this.mcldrDate.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.mcldrDate_DateSelected);
            this.mcldrDate.MaxSelectionCount = 1;
            this.mcldrDate.ShowToday = false;
            this.mcldrDate.ShowTodayCircle = false;
            // 
            // dgwAdminTable
            //
            this.dgwAdminTable.Location = new System.Drawing.Point(176, 9);
            this.dgwAdminTable.Size = new System.Drawing.Size(574, 591);
            this.dgwAdminTable.TabIndex = 1;
            //this.dgwAdminTable.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellClick);
            //this.dgwAdminTable.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellValidated);
            // 
            // btnSet
            // 
            this.btnSet.Location = new System.Drawing.Point(10, 207);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(154, 23);
            this.btnSet.TabIndex = 2;
            this.btnSet.Text = "Сохранить в базу";
            this.btnSet.UseVisualStyleBackColor = true;
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(10, 237);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(154, 23);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "Обновить из базы";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // gbxDivider
            // 
            this.gbxDivider.Location = new System.Drawing.Point(10, 264);
            this.gbxDivider.Name = "gbxDivider";
            this.gbxDivider.Size = new System.Drawing.Size(154, 8);
            this.gbxDivider.TabIndex = 4;
            this.gbxDivider.TabStop = false;
            // 
            // btnLoadLayout
            // 
            //this.btnLoadLayout.Location = new System.Drawing.Point(10, 282);
            //this.btnLoadLayout.Name = "btnLoad";
            //this.btnLoadLayout.Size = new System.Drawing.Size(154, 23);
            //this.btnLoadLayout.TabIndex = 2;
            //this.btnLoadLayout.Text = "Загрузить макет";
            //this.btnLoadLayout.UseVisualStyleBackColor = true;
            //this.btnLoadLayout.Click += new System.EventHandler(this.btnLoadLayout_Click);
            // 
            // btnImportExcel
            // 
            this.btnImportExcel.Location = new System.Drawing.Point(10, 279);
            this.btnImportExcel.Name = "btnImportExcel";
            this.btnImportExcel.Size = new System.Drawing.Size(154, 23);
            this.btnImportExcel.TabIndex = 667;
            this.btnImportExcel.Text = "Импорт из Excel";
            this.btnImportExcel.UseVisualStyleBackColor = true;
            this.btnImportExcel.Click += new System.EventHandler(this.btnImportExcel_Click);
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.Location = new System.Drawing.Point(10, 309);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(154, 23);
            this.btnExportExcel.TabIndex = 668;
            this.btnExportExcel.Text = "Экспорт в Excel";
            this.btnExportExcel.UseVisualStyleBackColor = true;
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
            this.btnExportExcel.Enabled = false;
            // 
            // comboBoxTecComponent
            // 
            this.comboBoxTecComponent.FormattingEnabled = true;
            this.comboBoxTecComponent.Location = new System.Drawing.Point(10, 10);
            this.comboBoxTecComponent.Name = "comboBoxTecComponent";
            this.comboBoxTecComponent.Size = new System.Drawing.Size(154, 21);
            this.comboBoxTecComponent.TabIndex = 3;
            this.comboBoxTecComponent.SelectionChangeCommitted += new System.EventHandler(this.comboBoxTecComponent_SelectionChangeCommitted);
            this.comboBoxTecComponent.DropDownStyle = ComboBoxStyle.DropDownList;
            ((System.ComponentModel.ISupportInitialize)(this.dgwAdminTable)).EndInit();
            this.ResumeLayout();
        }

        public PanelAdmin(Admin admin)
        {
            this.m_admin = admin;
            
            InitializeComponents();
            
            isActive = false;
        }

        private void getDataGridViewAdmin()
        {
            double value;
            bool valid;

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < (int)DataGridViewAdminNSS.DESC_INDEX.TO_ALL; j ++) {
                    switch (j)
                    {
                        case (int)DataGridViewAdminNSS.DESC_INDEX.PLAN: // План
                            valid = double.TryParse((string)dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminNSS.DESC_INDEX.PLAN].Value, out value);
                            m_admin.m_curRDGValues[i].plan = value;                            
                            break;
                        case (int)DataGridViewAdminNSS.DESC_INDEX.RECOMENDATION: // Рекомендация
                            {
                                //cellValidated(e.RowIndex, (int)DataGridViewAdminNSS.DESC_INDEX.RECOMENDATION);

                                valid = double.TryParse((string)dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminNSS.DESC_INDEX.RECOMENDATION].Value, out value);
                                m_admin.m_curRDGValues[i].recomendation = value;

                                break;
                            }
                        case (int)DataGridViewAdminNSS.DESC_INDEX.DEVIATION_TYPE:
                            {
                                if (! (this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminNSS.DESC_INDEX.DEVIATION_TYPE].Value == null))
                                    m_admin.m_curRDGValues[i].deviationPercent = bool.Parse(this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminNSS.DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                                else
                                    m_admin.m_curRDGValues[i].deviationPercent = false;

                                break;
                            }
                        case (int)DataGridViewAdminNSS.DESC_INDEX.DEVIATION: // Максимальное отклонение
                            {
                                valid = double.TryParse((string)this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminNSS.DESC_INDEX.DEVIATION].Value, out value);
                                m_admin.m_curRDGValues[i].deviation = value;
                                
                                break;
                            }
                    }
                }
            }

            //m_admin.CopyCurRDGValues();
        }

        public void setDataGridViewAdmin(DateTime date)
        {
            for (int i = 0; i < 24; i++)
            {
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminNSS.DESC_INDEX.DATE_HOUR].Value = date.AddHours(i + 1).ToString("yyyy-MM-dd HH");
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminNSS.DESC_INDEX.PLAN].Value = m_admin.m_curRDGValues[i].plan.ToString("F2");
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminNSS.DESC_INDEX.RECOMENDATION].Value = m_admin.m_curRDGValues[i].recomendation.ToString("F2");
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminNSS.DESC_INDEX.DEVIATION_TYPE].Value = m_admin.m_curRDGValues[i].deviationPercent.ToString();
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminNSS.DESC_INDEX.DEVIATION].Value = m_admin.m_curRDGValues[i].deviation.ToString("F2");
            }

            m_admin.CopyCurToPrevRDGValues();
        }

        public void CalendarSetDate(DateTime date)
        {
            BeginInvoke(new DelegateDateFunction(mcldrDate.SetDate), date); //mcldrDate.SetDate(date);
        }

        private void mcldrDate_DateSelected(object sender, DateRangeEventArgs e)
        {
            DialogResult result;
            Admin.Errors resultSaving;

            bool bRequery = false;

            getDataGridViewAdmin ();

            if (m_admin.WasChanged())
            {
                result = MessageBox.Show(this, "Данные были изменены но не сохранялись.\nЕсли Вы хотите сохранить изменения, нажмите \"да\".\nЕсли Вы не хотите сохранять изменения, нажмите \"нет\".\nДля отмены действия нажмите \"отмена\".", "Внимание", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
            }
            else
                result = DialogResult.No;

            switch (result)
            {
                case DialogResult.Yes:
                    if ((resultSaving = m_admin.SaveChanges()) == Admin.Errors.NoError)
                    {
                        bRequery = true;
                    }
                    else
                    {
                        if (resultSaving == Admin.Errors.InvalidValue)
                            MessageBox.Show(this, "Изменение ретроспективы недопустимо!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        else
                            MessageBox.Show(this, "Не удалось сохранить изменения, возможно отсутствует связь с базой данных.", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        mcldrDate.SetDate(m_admin.m_prevDate);
                    }
                    break;
                case DialogResult.No:
                    bRequery = true;
                    break;
                case DialogResult.Cancel:
                    mcldrDate.SetDate(m_admin.m_prevDate);
                    break;
            }

            if (bRequery == true) {
                ClearTables();

                m_admin.GetRDGValues(Admin.TYPE_FIELDS.DYNAMIC, m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
            }
            else
                ;
        }

        public void InitializeComboBoxTecComponent (FormChangeMode.MODE_TECCOMPONENT mode) {
            m_listTECComponentIndex = m_admin.GetListIndexTECComponent (mode);

            comboBoxTecComponent.Items.Clear ();

            for (int i = 0; i < m_listTECComponentIndex.Count; i ++) {
                comboBoxTecComponent.Items.Add(m_admin.allTECComponents[m_listTECComponentIndex [i]].tec.name + " - " + m_admin.allTECComponents[m_listTECComponentIndex [i]].name);
            }

            m_admin.m_typeFields = Admin.TYPE_FIELDS.DYNAMIC;

            m_admin.indxTECComponents = m_listTECComponentIndex [0];
            if (comboBoxTecComponent.Items.Count > 0) {
                comboBoxTecComponent.SelectedIndex = 0;
            }
            else
                ;
        }

        private void comboBoxTecComponent_SelectionChangeCommitted(object sender, EventArgs e)
        {
            DialogResult result;
            Admin.Errors resultSaving;

            bool bRequery = false;

            getDataGridViewAdmin();

            if (m_admin.WasChanged())
            {
                result = MessageBox.Show(this, "Данные были изменены но не сохранялись.\nЕсли Вы хотите сохранить изменения, нажмите \"да\".\nЕсли Вы не хотите сохранять изменения, нажмите \"нет\".\nДля отмены действия нажмите \"отмена\".", "Внимание", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
            }
            else
                result = DialogResult.No;

            switch (result)
            {
                case DialogResult.Yes:
                    if ((resultSaving = m_admin.SaveChanges()) == Admin.Errors.NoError)
                    {
                        bRequery = true;
                    }
                    else
                    {
                        if (resultSaving == Admin.Errors.InvalidValue)
                            MessageBox.Show(this, "Изменение ретроспективы недопустимо!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        else
                            MessageBox.Show(this, "Не удалось сохранить изменения, возможно отсутствует связь с базой данных.", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        comboBoxTecComponent.SelectedIndex = prevSelectedIndex;
                    }
                    break;
                case DialogResult.No:
                    bRequery = true;
                    break;
                case DialogResult.Cancel:
                    comboBoxTecComponent.SelectedIndex = prevSelectedIndex;
                    break;
            }

            if (bRequery) {
                ClearTables();

                m_admin.GetRDGValues(Admin.TYPE_FIELDS.DYNAMIC, m_listTECComponentIndex [comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
            }
            else
                ;

            visibleControlRDGExcel ();
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            getDataGridViewAdmin();
            
            Admin.Errors resultSaving;
            if ((resultSaving = m_admin.SaveChanges()) == Admin.Errors.NoError)
            {
                m_admin.GetRDGValues(Admin.TYPE_FIELDS.DYNAMIC, m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
            }
            else
            {
                if (resultSaving == Admin.Errors.InvalidValue)
                    MessageBox.Show(this, "Изменение ретроспективы недопустимо!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                else
                    MessageBox.Show(this, "Не удалось сохранить изменения, возможно отсутствует связь с базой данных.", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearTables();

            m_admin.GetRDGValues(Admin.TYPE_FIELDS.DYNAMIC, m_listTECComponentIndex [comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
        }

        private string SetNumberSeparator(string current_str)
        {
            if (current_str.IndexOf(".") > 0)
            {
                if (System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator != ".")
                    return current_str.Replace(".", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);
            }
            else
            {
                if (current_str.IndexOf(",") > 0)
                {
                    if (System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator != ",")
                        return current_str.Replace(",", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);
                }
            }
            return current_str;
        }

        private void btnImportExcel_Click(object sender, EventArgs e)
        {
            ClearTables();

            m_admin.GetRDGExcelValues(m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex], mcldrDate.SelectionStart);
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog exportFolder = new FolderBrowserDialog ();
            exportFolder.ShowDialog ();

            if (exportFolder.SelectedPath.Length > 0) {
            }
            else
                ;
        }

        public bool MayToClose()
        {
            DialogResult result;
            Admin.Errors resultSaving;

            getDataGridViewAdmin();

            if (m_admin.WasChanged())
            {
                result = MessageBox.Show(this, "Данные были изменены но не сохранялись.\nЕсли Вы хотите сохранить изменения, нажмите \"да\".\nЕсли Вы не хотите сохранять изменения, нажмите \"нет\".\nДля отмены действия нажмите \"отмена\".", "Внимание", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
            }
            else
                result = DialogResult.No;

            switch (result)
            {
                case DialogResult.Yes:
                    if ((resultSaving = m_admin.SaveChanges()) == Admin.Errors.NoError)
                        return true;
                    else
                    {
                        if (resultSaving == Admin.Errors.InvalidValue)
                            if (MessageBox.Show(this, "Изменение ретроспективы недопустимо!\nПродолжить выход?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                                return true;
                            else
                                return false;
                        else
                            if (MessageBox.Show(this, "Не удалось сохранить изменения, возможно отсутствует связь с базой данных.\nВыйти без сохранения?", "Ошибка сохранения", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                                return true;
                            else
                                return false;
                    }
                case DialogResult.No:
                    return true;
                case DialogResult.Cancel:
                    return false;
            }
            return false;
        }

        public void ClearTables()
        {
            for (int i = 0; i < 24; i++)
            {
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminNSS.DESC_INDEX.DATE_HOUR].Value = "";
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminNSS.DESC_INDEX.PLAN].Value = "";
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminNSS.DESC_INDEX.RECOMENDATION].Value = "";
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminNSS.DESC_INDEX.DEVIATION_TYPE].Value = "false";
                this.dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminNSS.DESC_INDEX.DEVIATION].Value = "";
            }
        }

        private void visibleControlRDGExcel () {
            bool bImpExpButtonVisible = false;
            if ((!(m_listTECComponentIndex == null)) && (m_listTECComponentIndex.Count > 0) && (!(comboBoxTecComponent.SelectedIndex < 0)) && (m_admin.IsRDGExcel(m_listTECComponentIndex[comboBoxTecComponent.SelectedIndex]) == true))
                bImpExpButtonVisible = true;
            else
                ;

            btnImportExcel.Visible =
            btnExportExcel.Visible = bImpExpButtonVisible;
        }

        public void Activate(bool active)
        {
            visibleControlRDGExcel ();

            isActive = active;
        }
    }
}
