using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

using HClassLibrary;

namespace StatisticDiagnostic
{
    partial class PanelStatisticDiagnostic
    {
        /// <summary>
        /// Класс для описания элемента панели с информацией
        /// отображения значений параметров диагностики 
        /// работоспособности задач по расписанию
        /// </summary>
        private partial class PanelTask : HPanelCommon
        {
            /// <summary>
            /// Количество столбцов, строк в сетке макета
            /// </summary>
            private const int COUNT_LAYOUT_COLUMN = 1
                , COUNT_LAYOUT_ROW = 9;

            private DataGridView m_dgvValues;

            private Label m_labelDescription;

            public PanelTask(ListDiagnosticSource listDiagnosticSource)
                : base(COUNT_LAYOUT_COLUMN, COUNT_LAYOUT_ROW)
            {
                initialize(listDiagnosticSource);
            }

            public PanelTask(IContainer container, ListDiagnosticSource listDiagnosticSource)
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

                listDiagSrc = new ListDiagnosticSource(listDiagnosticSource.FindAll(item => {
                    return item.m_id_component < (int)INDEX_SOURCE.SIZEDB;
                }));

                foreach (DIAGNOSTIC_SOURCE src in listDiagSrc) {
                    iNewRow = m_dgvValues.Rows.Add(new DataGridViewTaskRow());

                    m_dgvValues.Rows[iNewRow].Tag = src.m_id_component;

                    (m_dgvValues.Rows[iNewRow] as DataGridViewTaskRow).Name = src.m_name_shr;
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
                    components.Dispose();
                base.Dispose(disposing);
            }

            private enum INDEX_CELL : short { NAME, AVG_RUNTIME, DATETIME_VERIFICATION, DATETIME_RUN, ERROR_DESCRIPTION, STATE
                , COUNT
            }

            #region Код, автоматически созданный конструктором компонентов

            /// <summary>
            /// Обязательный метод для поддержки конструктора - не изменяйте
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            private void InitializeComponent()
            {
                //this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Outset;

                m_dgvValues = new System.Windows.Forms.DataGridView(); this.Controls.Add(m_dgvValues, 0, 1); this.SetRowSpan(m_dgvValues, COUNT_LAYOUT_ROW - 1);
                m_labelDescription = new Label(); this.Controls.Add(m_labelDescription, 0, 0);

                this.SuspendLayout();

                this.m_dgvValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.m_dgvValues.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
                this.m_dgvValues.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.m_dgvValues.Dock = DockStyle.Fill;
                this.m_dgvValues.ClearSelection();
                this.m_dgvValues.Name = "TaskDataGridView";
                this.m_dgvValues.ColumnCount = (int)INDEX_CELL.COUNT;
                this.m_dgvValues.Columns[(int)INDEX_CELL.NAME].Name = "Имя задачи"; this.m_dgvValues.Columns[(int)INDEX_CELL.NAME].Width = 30;
                this.m_dgvValues.Columns[(int)INDEX_CELL.AVG_RUNTIME].Name = "Среднее время выполнения"; this.m_dgvValues.Columns[(int)INDEX_CELL.AVG_RUNTIME].Width = 12;
                this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VERIFICATION].Name = "Время проверки"; this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VERIFICATION].Width = 5;
                this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_RUN].Name = "Время выполнения задачи"; this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_RUN].Width = 15;
                this.m_dgvValues.Columns[(int)INDEX_CELL.ERROR_DESCRIPTION].Name = "Описание ошибки"; this.m_dgvValues.Columns[(int)INDEX_CELL.ERROR_DESCRIPTION].Width = 20;
                this.m_dgvValues.Columns[(int)INDEX_CELL.STATE].Name = "Статус задачи"; this.m_dgvValues.Columns[(int)INDEX_CELL.STATE].Width = 15;
                this.m_dgvValues.RowHeadersVisible = false;
                this.m_dgvValues.TabIndex = 0;
                this.m_dgvValues.AllowUserToAddRows = false;
                this.m_dgvValues.ReadOnly = true;

                m_labelDescription.Text = @"Задачи по расписанию";
                m_labelDescription.Dock = DockStyle.Fill;

                this.m_dgvValues.CellClick += dgv_CellCancel;
                this.m_dgvValues.CellValueChanged += dgv_CellCancel;

                this.ResumeLayout();
            }

            #endregion;
        }

        /// <summary>
        /// Класс для описания элемента панели с информацией
        /// отображения значений параметров диагностики 
        /// работоспособности задач по расписанию
        /// </summary>
        partial class PanelTask
        {
            private class DataGridViewTaskRow : DataGridViewRow
            {
                public string Name
                {
                    get { return Cells[(int)INDEX_CELL.NAME].Value.ToString(); }

                    set { Cells[(int)INDEX_CELL.NAME].Value = value; }
                }
            }

            public void Update(object table)
            {
            }

            /// <summary>
            /// очистка панелей
            /// </summary>
            public void Clear()
            {
                if (!(m_dgvValues == null))
                    m_dgvValues.Rows.Clear();
                else
                    ;
            }

            /// <summary>
            /// Снятие выделения с ячеек
            /// </summary>
            /// <param name="sender">объект, инициировавший событие</param>
            /// <param name="e">Аргумент события</param>
            void dgv_CellCancel(object sender, DataGridViewCellEventArgs e)
            {
                try {
                    if (m_dgvValues.SelectedCells.Count > 0)
                        m_dgvValues.SelectedCells[0].Selected = false;
                    else
                        ;
                } catch {
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
                string m_dt;
                string m_dt2Time = DateTime.TryParse(datetime, out result).ToString();

                if (m_dt2Time != "False")
                {
                    if (Convert.ToInt32(result.Day - DateTime.Now.Day) < 0)
                        return m_dt = DateTime.Parse(datetime).ToString("dd.MM.yy HH:mm:ss");
                    else
                        return m_dt = DateTime.Parse(datetime).ToString("HH:mm:ss.fff");
                }
                else
                    m_dt = datetime;

                return m_dt;
            }

            /// <summary>
            /// Функция заполенния ячеек грида временем
            /// </summary>
            /// <param name="i">номер строки</param>
            private void columTimeTask(int i)
            {
                string strNow =
                    TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("hh:mm:ss:fff");
                m_dgvValues.Rows[i].Cells[3].Value = strNow;
            }

            /// <summary>
            /// Преобразование времени выполнения задач
            /// </summary>
            /// <param name="m_strTime">значение ячейки</param>
            /// <returns>возврат даты или ошибки</returns>
            private string ToDateTime(object m_strTime)
            {
                string parseStr;

                if (m_strTime.ToString() != "")
                {
                    TimeSpan time = TimeSpan.FromSeconds(Convert.ToDouble(m_strTime));
                    parseStr = DateTime.Parse(Convert.ToString(time)).ToString("mm:ss");
                }
                else
                    parseStr = "Ошибка!";
                return parseStr;
            }

            /// <summary>
            /// Добавление строк
            /// </summary>
            /// <param name="counter">кол-во строк</param>
            private void addRowsTask(int counter)
            {
                for (int x = 0; x < counter; x++)
                {
                    if (m_dgvValues.InvokeRequired)
                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows.Add()));
                    else
                        m_dgvValues.Rows.Add();
                }
            }

            /// <summary>
            ///  Проверка работоспособности задач
            /// </summary>
            /// <param name="dtime">время задачи</param>
            /// <returns></returns>
            private bool interruptTask(string dtime)
            {
                if ((DateTime.Parse(SERVER_TIME.ToString()) - DateTime.Parse(dtime)).TotalHours > 1.0)
                    return true;
                else
                    return false;
            }

            /// <summary>
            /// выделение строки с превышением лимита выполенния задачи
            /// </summary>
            private void overLimit()
            {
                TimeSpan m_lim;
                int m_check = 0;
                DataRow[] arSelTask = null; // m_tableSourceData.Select(@"ID_Value = '28'");
                int m_counter = 1;

                for (int i = 0; i < arSelTask.Count(); i++)
                {
                    if (m_dgvValues.Rows[m_check].Cells[0].Value.ToString() == "Усреднитель данных из СОТИАССО")
                        m_lim = limTaskAvg;
                    else m_lim = limTask;

                    if (int.Parse(arSelTask[i]["Link"].ToString()) == 1)
                    {
                        if (arSelTask[i]["Value"].ToString() == "")
                        {
                            if (m_dgvValues.Columns[4].Visible == false)
                                m_dgvValues.Invoke(new Action(() => m_dgvValues.Columns[4].Visible = true));

                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[m_check].Cells[4].Value = "Задача не выполняется"));
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[m_check].Cells[5].Value = ""));
                            upselectrow(m_check);
                            m_counter--;
                        }
                        else
                            if (interruptTask(arSelTask[i + 1]["Value"].ToString()))
                        {
                            if (m_dgvValues.Columns[4].Visible == false)
                                m_dgvValues.Invoke(new Action(() => m_dgvValues.Columns[4].Visible = true));

                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[m_check].Cells[4].Value = "Задача не выполняется"));
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[m_check].Cells[5].Value = ""));
                            upselectrow(m_check);
                            m_counter--;
                        }
                        else
                                if (TimeSpan.FromSeconds(Convert.ToDouble(arSelTask[i]["Value"])) > m_lim)
                        {
                            if (m_dgvValues.Columns[4].Visible == false)
                                m_dgvValues.Invoke(new Action(() => m_dgvValues.Columns[4].Visible = true));

                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[m_check].Cells[4].Value = "Превышено время выполнения задачи"));
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[m_check].Cells[5].Value = ""));
                            upselectrow(m_check);
                            m_counter--;
                        }
                        else
                        {
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[m_check].DefaultCellStyle.BackColor = Color.White));
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[m_check].Cells[4].Value = ""));
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[m_check].Cells[5].Value = ""));

                            if (m_counter == m_dgvValues.Rows.Count)
                                if (m_dgvValues.Columns[4].Visible == true)
                                    m_dgvValues.Invoke(new Action(() => m_dgvValues.Columns[4].Visible = false));
                                else;
                            else
                                m_counter++;
                        }
                    }
                    else
                    {
                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[m_check].Cells[5].Value = "Запрещена"));
                        upselectrow(m_check);
                        m_counter++;
                    }
                    m_check++;
                    i++;
                }

            }

            /// <summary>
            /// Перенос строки на вверх грида 
            /// при ошибки выполнения задачи
            /// </summary>
            /// <param name="row">индекс строки</param>
            private void upselectrow(int indxrow)
            {
                if (m_dgvValues.InvokeRequired)
                {
                    m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows.Insert(0, 1)));

                    for (int i = 0; i < m_dgvValues.Rows[indxrow + 1].Cells.Count; i++)
                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[0].Cells[i].Value = m_dgvValues.Rows[indxrow + 1].Cells[i].Value));

                    if (Convert.ToString(m_dgvValues.Rows[0].Cells[4].Value) == "Задача не выполняется")
                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[0].DefaultCellStyle.BackColor = Color.Firebrick));
                    else
                        if (m_dgvValues.Rows[0].Cells[4].Value.ToString() == "Превышено время выполнения задачи")
                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[0].DefaultCellStyle.BackColor = Color.Sienna));
                    else
                            if (m_dgvValues.Rows[0].Cells[5].Value.ToString() == "Запрещена")
                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[0].DefaultCellStyle.BackColor = Color.White));
                    m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows.RemoveAt(indxrow + 1)));
                }
                else
                {
                    m_dgvValues.Rows.InsertCopy(indxrow, 0);
                    m_dgvValues.Rows.RemoveAt(indxrow);
                }
            }
        }
    }
}
