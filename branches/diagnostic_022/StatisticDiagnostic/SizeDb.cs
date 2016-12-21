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
            public DataGridView SizeDbDataGridView = new DataGridView();

            public SizeDb(ListDiagnosticSource listDiagSource)
                : base(-1, -1)
            {
                initialize(listDiagSource);
            }

            public SizeDb(IContainer container, ListDiagnosticSource listDiagSource)
                : base(container, -1, -1)
            {
                container.Add(this);

                initialize(listDiagSource);
            }

            private void initialize(ListDiagnosticSource listDiagSource)
            {
                m_listDiagnosticSource = listDiagSource;

                InitializeComponent();
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
                SizeDbDataGridView = new System.Windows.Forms.DataGridView();
                this.SuspendLayout();

                this.SizeDbDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.SizeDbDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
                this.SizeDbDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.SizeDbDataGridView.Dock = DockStyle.Fill;
                this.SizeDbDataGridView.ClearSelection();
                this.SizeDbDataGridView.Name = "SizeDbDataGridView";
                this.SizeDbDataGridView.ColumnCount = 3;
                this.SizeDbDataGridView.Columns[0].Name = "Имя базы данных";
                this.SizeDbDataGridView.Columns[1].Name = "Размер базы данных, МБ";
                this.SizeDbDataGridView.Columns[2].Name = "Время проверки";
                this.SizeDbDataGridView.RowHeadersVisible = false;
                this.SizeDbDataGridView.TabIndex = 0;
                this.SizeDbDataGridView.AllowUserToAddRows = false;
                this.SizeDbDataGridView.ReadOnly = true;

                this.SizeDbDataGridView.CellClick += SizeDbDataGridView_CellClick;
                this.SizeDbDataGridView.CellValueChanged += SizeDbDataGridView_CellClick;
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
            private ListDiagnosticSource m_listDiagnosticSource;

            /// <summary>
            /// Загрузка значений
            /// </summary>
            public void Update(object rec)
            {
                DataTable tableRecieved = rec as DataTable;
                DataRow[] arSelSizeOF;
                string filter = string.Empty;
                int countID;

                var m_enumIDEXTDB = (from r in m_listDiagnosticSource
                                     where r.m_id_component >= (int)INDEX_SOURCE.SIZEDB && r.m_id_component < (int)INDEX_SOURCE.MODES - 100
                                     select new
                                     {
                                         COMPONENT = r.m_id_component,
                                     }).Distinct();

                countID = m_enumIDEXTDB.Count();

                for (int j = 0, countRow = 0; j < countID; j++, countRow += 2) {
                    filter = "ID_EXT = '" + m_enumIDEXTDB.ElementAt(j).COMPONENT + "'";
                    arSelSizeOF = tableRecieved.Select(filter);

                    if (SizeDbDataGridView.RowCount < (countID * 2))
                        AddRows(countID);
                    else
                        ;

                    AddItem(arSelSizeOF, countRow);
                    NameDb(arSelSizeOF, m_listDiagnosticSource.FindAll(item => { return item.m_id_component == m_enumIDEXTDB.ElementAt(j).COMPONENT; }), countRow);
                }
            }

            /// <summary>
            /// Добавление данных в грид
            /// </summary>
            /// <param name="dr"></param>
            private void AddItem(DataRow[] dr, int row)
            {
                for (int i = 0; i < dr.Count(); i++)
                {
                    if (SizeDbDataGridView.InvokeRequired)
                    {
                        SizeDbDataGridView.Invoke(new Action(() => SizeDbDataGridView.Rows[row].Cells[1].Value = dr[i]["Value"]));
                        SizeDbDataGridView.Invoke(new Action(() => SizeDbDataGridView.Rows[row].Cells[2].Value = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("hh:mm:ss:fff")));
                        row++;
                    }
                    else
                    {
                        SizeDbDataGridView.Rows[row].Cells[1].Value = dr[i]["Value"];
                        SizeDbDataGridView.Rows[row].Cells[2].Value = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("hh:mm:ss:fff");
                        row++;
                    }
                }
            }

            /// <summary>
            /// Добавление строк в датагрид
            /// </summary>
            /// <param name="countRows">кол-во строк</param>
            private void AddRows(int countRows)
            {
                for (int i = 0; i < countRows; i++)
                {
                    if (SizeDbDataGridView.InvokeRequired)
                        SizeDbDataGridView.Invoke(new Action(() => SizeDbDataGridView.Rows.Add()));
                    else
                        SizeDbDataGridView.Rows.Add();
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
                            SizeDbDataGridView.Rows[nextrow].Cells[0].Value = listDiagSource[j].m_description;
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
            void SizeDbDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
            {
                try
                {
                    if (SizeDbDataGridView.SelectedCells.Count > 0)
                        SizeDbDataGridView.SelectedCells[0].Selected = false;
                    else
                        ;
                }
                catch { }
            }
        }
    }
}
