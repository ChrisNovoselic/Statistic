using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

using HClassLibrary;
using System.Data;

namespace StatisticDiagnostic
{
    partial class PanelStatisticDiagnostic
    {
        /// <summary>
        /// Класс для описания элемента панели с информацией
        /// отображения значений параметров диагностики 
        /// размера баз данных
        /// </summary>
        private partial class SizeDb : HPanelCommon
        {
            /// <summary>
            /// Количество столбцов, строк в сетке макета
            /// </summary>
            private const int COUNT_LAYOUT_COLUMN = 1
                , COUNT_LAYOUT_ROW = 9;

            private DataGridView m_dgvValues;

            private Label m_labelDescription;

            public SizeDb(ListDiagnosticSource listDiagnosticSource)
                : base(COUNT_LAYOUT_COLUMN, COUNT_LAYOUT_ROW)
            {
                initialize(listDiagnosticSource);
            }

            public SizeDb(IContainer container, ListDiagnosticSource listDiagnosticSource)
                : base(container, COUNT_LAYOUT_COLUMN, COUNT_LAYOUT_ROW)
            {
                container.Add(this);

                initialize(listDiagnosticSource);
            }

            private void initialize(ListDiagnosticSource listDiagnosticSource)
            {
                ListDiagnosticSource listDiagSrc;
                int iNewRow = -1;

                initializeLayoutStyle();

                InitializeComponent();

                listDiagSrc = new ListDiagnosticSource (listDiagnosticSource.FindAll(item => { return (!(item.m_id_component < (int)INDEX_SOURCE.SIZEDB))
                    && (item.m_id_component < (int)INDEX_SOURCE.MODES - 100); }));

                foreach (DIAGNOSTIC_SOURCE src in listDiagSrc) {
                    iNewRow = m_dgvValues.Rows.Add(new DataGridViewDbRow ());

                    m_dgvValues.Rows[iNewRow].Tag = src.m_id_component;

                    (m_dgvValues.Rows[iNewRow] as DataGridViewDbRow).Name = src.m_name_shr;
                }
            }

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly(cols, rows);
            }

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

            #region Код, автоматически созданный конструктором компонентов

            /// <summary>
            /// Обязательный метод для поддержки конструктора - не изменяйте
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            private void InitializeComponent()
            {
                m_dgvValues = new System.Windows.Forms.DataGridView(); this.Controls.Add(m_dgvValues, 0, 1); this.SetRowSpan(m_dgvValues, COUNT_LAYOUT_ROW - 1);
                m_labelDescription = new Label(); this.Controls.Add(m_labelDescription, 0, 0);

                this.SuspendLayout();

                this.m_dgvValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.m_dgvValues.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
                this.m_dgvValues.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.m_dgvValues.Dock = DockStyle.Fill;
                this.m_dgvValues.ClearSelection();
                this.m_dgvValues.Name = "SizeDbDataGridView";
                this.m_dgvValues.ColumnCount = 3;
                this.m_dgvValues.Columns[0].Name = "Имя базы данных";
                this.m_dgvValues.Columns[1].Name = "Размер базы данных, МБ";
                this.m_dgvValues.Columns[2].Name = "Время проверки";
                this.m_dgvValues.RowHeadersVisible = false;
                this.m_dgvValues.TabIndex = 0;
                this.m_dgvValues.AllowUserToAddRows = false;
                this.m_dgvValues.ReadOnly = true;

                m_labelDescription.Text = @"Размер БД";

                this.m_dgvValues.CellClick += dgv_CellCancel;
                this.m_dgvValues.CellValueChanged += dgv_CellCancel;
                this.ResumeLayout();
            }

            #endregion;
        }

        /// <summary>
        /// Класс для описания элемента панели с информацией
        /// отображения значений параметров диагностики 
        /// размера баз данных
        /// </summary>
        partial class SizeDb
        {
            private enum INDEX_CELL : short { NAME }

            private class DataGridViewDbRow : DataGridViewRow
            {
                public string Name
                {
                    get { return Cells[(int)INDEX_CELL.NAME].Value.ToString(); }

                    set { Cells[(int)INDEX_CELL.NAME].Value = value; }
                }
            }
            /// <summary>
            /// Загрузка значений
            /// </summary>
            public void Update(object rec)
            {
                DataTable tableRecieved = rec as DataTable;
            }

            /// <summary>
            /// Добавление данных в грид
            /// </summary>
            /// <param name="dr"></param>
            private void AddItem(DataRow[] dr, int row)
            {
                for (int i = 0; i < dr.Count(); i++)
                {
                    if (m_dgvValues.InvokeRequired)
                    {
                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[row].Cells[1].Value = dr[i]["Value"]));
                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[row].Cells[2].Value = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("hh:mm:ss:fff")));
                        row++;
                    }
                    else
                    {
                        m_dgvValues.Rows[row].Cells[1].Value = dr[i]["Value"];
                        m_dgvValues.Rows[row].Cells[2].Value = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("hh:mm:ss:fff");
                        row++;
                    }
                }
            }

            /// <summary>
            /// Форматирование даты
            /// “HH:mm:ss.fff”
            /// </summary>
            /// <param name="datetime">дата</param>
            /// <returns>форматированная дата</returns>
            private string formatTime(string datetime)
            {
                DateTime result;
                string strDTRes = string.Empty;
                bool bDTRes = DateTime.TryParse(datetime, out result);

                if (bDTRes != false)
                {
                    if (Convert.ToInt32(result.Day - DateTime.Now.Day) < 0)
                        strDTRes = DateTime.Parse(datetime).ToString("dd.MM.yy HH:mm:ss");
                    else
                        strDTRes = DateTime.Parse(datetime).ToString("HH:mm:ss.fff");
                }
                else
                    strDTRes = datetime;

                return strDTRes;
            }

            /// <summary>
            /// Поименование источников информации
            /// </summary>
            /// <param name="drMain"></param>
            /// <param name="drSource"></param>
            private void NameDb(DataRow[] drMain, List<DIAGNOSTIC_SOURCE> listDiagSource, int nextrow)
            {
                for (int i = 0; i < listDiagSource.Count; i++)
                {
                    for (int j = 0; j < listDiagSource.Count(); j++)
                    {
                        if (drMain[i]["ID_Value"].ToString() == listDiagSource[j].m_id.ToString())
                        {
                            m_dgvValues.Rows[nextrow].Cells[0].Value = listDiagSource[j].m_description;
                            nextrow++;
                        }
                    }
                }
            }

            /// <summary>
            /// Снятие выделения с ячеек
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            void dgv_CellCancel(object sender, DataGridViewCellEventArgs e)
            {
                try
                {
                    if (m_dgvValues.SelectedCells.Count > 0)
                        m_dgvValues.SelectedCells[0].Selected = false;
                    else
                        ;
                }
                catch { }
            }
        }
    }
}
