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
            public DataGridView m_dgvValues;

            public PanelTask()
                : base(-1, -1)
            {
                initialize();
            }

            public PanelTask(IContainer container)
                : base(container, -1, -1)
            {
                container.Add(this);

                initialize();
            }

            private void initialize()
            {
                InitializeComponentTask();
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

            #region Код, автоматически созданный конструктором компонентов

            /// <summary>
            /// Обязательный метод для поддержки конструктора - не изменяйте
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            private void InitializeComponentTask()
            {
                m_dgvValues = new System.Windows.Forms.DataGridView();
                this.SuspendLayout();

                this.m_dgvValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.m_dgvValues.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
                this.m_dgvValues.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.m_dgvValues.Dock = DockStyle.Fill;
                this.m_dgvValues.ClearSelection();
                this.m_dgvValues.Name = "TaskDataGridView";
                this.m_dgvValues.ColumnCount = 6;
                this.m_dgvValues.Columns[0].Name = "Имя задачи";
                this.m_dgvValues.Columns[1].Name = "Среднее время выполнения";
                this.m_dgvValues.Columns[3].Name = "Время проверки";
                this.m_dgvValues.Columns[2].Name = "Время выполнения задачи";
                this.m_dgvValues.Columns[4].Name = "Описание ошибки";
                this.m_dgvValues.Columns[5].Name = "Статус задачи";
                this.m_dgvValues.Columns[0].Width = 30;
                this.m_dgvValues.Columns[1].Width = 12;
                this.m_dgvValues.Columns[3].Width = 5;
                this.m_dgvValues.Columns[2].Width = 15;
                this.m_dgvValues.Columns[4].Width = 20;
                this.m_dgvValues.Columns[5].Width = 15;
                this.m_dgvValues.RowHeadersVisible = false;
                this.m_dgvValues.TabIndex = 0;
                this.m_dgvValues.AllowUserToAddRows = false;
                this.m_dgvValues.ReadOnly = true;

                this.m_dgvValues.CellClick += TaskDataGridView_CellClick;
                this.m_dgvValues.CellValueChanged += TaskDataGridView_CellClick;
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
            /// Функция для заполнения 
            /// грида информацией о задачах
            /// </summary>
            public void AddItem()
            {
                try
                {
                    DataRow[] drNameTask;
                    string filter;
                    int enumCnt;

                    var m_enumIDtask = (from r in m_tableSourceData.AsEnumerable()
                                        where r.Field<string>("ID_Value") == "28"
                                        select new
                                        {
                                            NAME = r.Field<string>("NAME_SHR"),
                                        }).Distinct();

                    enumCnt = m_enumIDtask.Count();

                    if (m_dgvValues.Rows.Count < enumCnt)
                        addRowsTask(enumCnt);

                    for (int i = 0; i < enumCnt; i++)
                    {
                        filter = "NAME_SHR = '" + m_enumIDtask.ElementAt(i).NAME + "'";
                        drNameTask = m_tableSourceData.Select(filter);

                        if (m_dgvValues.InvokeRequired)
                        {
                            columTimeTask(i);
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[i].Cells[1].Value = ToDateTime(drNameTask[0]["Value"])));
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[i].Cells[2].Value = formatTime(drNameTask[1]["Value"].ToString())));
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[i].Cells[0].Value = drNameTask[0]["NAME_SHR"]));
                        }
                        else
                        {
                            columTimeTask(i);
                            m_dgvValues.Rows[i].Cells[1].Value = drNameTask[0]["Value"];
                            m_dgvValues.Rows[i].Cells[2].Value = formatTime(drNameTask[1]["Value"].ToString());
                            m_dgvValues.Rows[i].Cells[0].Value = drNameTask[0]["NAME_SHR"];
                        }
                    }

                    overLimit();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Ошибка заполнения субобласти Задачи" + e + "");
                }
            }

            /// <summary>
            /// Снятие выделения с ячеек
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            void TaskDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
            {
                try
                {
                    if (m_dgvValues.SelectedCells.Count > 0)
                        m_dgvValues.SelectedCells[0].Selected = false;
                }
                catch { }
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
                string m_timeNow =
                    TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("hh:mm:ss:fff");
                m_dgvValues.Rows[i].Cells[3].Value = m_timeNow;
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
                DataRow[] drTask = m_tableSourceData.Select(@"ID_Value = '28'");
                int m_counter = 1;

                for (int i = 0; i < drTask.Count(); i++)
                {
                    if (m_dgvValues.Rows[m_check].Cells[0].Value.ToString() == "Усреднитель данных из СОТИАССО")
                        m_lim = limTaskAvg;
                    else m_lim = limTask;

                    if (int.Parse(drTask[i]["Link"].ToString()) == 1)
                    {
                        if (drTask[i]["Value"].ToString() == "")
                        {
                            if (m_dgvValues.Columns[4].Visible == false)
                                m_dgvValues.Invoke(new Action(() => m_dgvValues.Columns[4].Visible = true));

                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[m_check].Cells[4].Value = "Задача не выполняется"));
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[m_check].Cells[5].Value = ""));
                            upselectrow(m_check);
                            m_counter--;
                        }
                        else
                            if (interruptTask(drTask[i + 1]["Value"].ToString()))
                        {
                            if (m_dgvValues.Columns[4].Visible == false)
                                m_dgvValues.Invoke(new Action(() => m_dgvValues.Columns[4].Visible = true));

                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[m_check].Cells[4].Value = "Задача не выполняется"));
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[m_check].Cells[5].Value = ""));
                            upselectrow(m_check);
                            m_counter--;
                        }
                        else
                                if (TimeSpan.FromSeconds(Convert.ToDouble(drTask[i]["Value"])) > m_lim)
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
