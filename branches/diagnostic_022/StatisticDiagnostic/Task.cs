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
            public DataGridView TaskDataGridView = new DataGridView();

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
                TaskDataGridView = new System.Windows.Forms.DataGridView();
                this.SuspendLayout();

                this.TaskDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                this.TaskDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
                this.TaskDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.TaskDataGridView.Dock = DockStyle.Fill;
                this.TaskDataGridView.ClearSelection();
                this.TaskDataGridView.Name = "TaskDataGridView";
                this.TaskDataGridView.ColumnCount = 6;
                this.TaskDataGridView.Columns[0].Name = "Имя задачи";
                this.TaskDataGridView.Columns[1].Name = "Среднее время выполнения";
                this.TaskDataGridView.Columns[3].Name = "Время проверки";
                this.TaskDataGridView.Columns[2].Name = "Время выполнения задачи";
                this.TaskDataGridView.Columns[4].Name = "Описание ошибки";
                this.TaskDataGridView.Columns[5].Name = "Статус задачи";
                this.TaskDataGridView.Columns[0].Width = 30;
                this.TaskDataGridView.Columns[1].Width = 12;
                this.TaskDataGridView.Columns[3].Width = 5;
                this.TaskDataGridView.Columns[2].Width = 15;
                this.TaskDataGridView.Columns[4].Width = 20;
                this.TaskDataGridView.Columns[5].Width = 15;
                this.TaskDataGridView.RowHeadersVisible = false;
                this.TaskDataGridView.TabIndex = 0;
                this.TaskDataGridView.AllowUserToAddRows = false;
                this.TaskDataGridView.ReadOnly = true;

                this.TaskDataGridView.CellClick += TaskDataGridView_CellClick;
                this.TaskDataGridView.CellValueChanged += TaskDataGridView_CellClick;
                this.ResumeLayout();
            }

            #endregion;

            public System.Windows.Forms.TableLayoutPanel TaskTableLayoutPanel;
        }

        /// <summary>
        /// Класс для описания элемента панели с информацией
        /// отображения значений параметров диагностики 
        /// работоспособности задач по расписанию
        /// </summary>
        partial class PanelTask
        {
            /// <summary>
            /// Функция активации
            /// </summary>
            /// <param name="?">параметр активации</param>
            public void ActivateTask(bool activated)
            {
                if (activated == true)
                {
                    if (!(TaskTableLayoutPanel == null))
                        TaskTableLayoutPanel.Focus();
                }
                else;
            }

            /// <summary>
            /// очистка панелей
            /// </summary>
            public void Clear()
            {
                if (!(TaskDataGridView == null))
                    TaskDataGridView.Rows.Clear();
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

                    if (TaskDataGridView.Rows.Count < enumCnt)
                        addRowsTask(enumCnt);

                    for (int i = 0; i < enumCnt; i++)
                    {
                        filter = "NAME_SHR = '" + m_enumIDtask.ElementAt(i).NAME + "'";
                        drNameTask = m_tableSourceData.Select(filter);

                        if (TaskDataGridView.InvokeRequired)
                        {
                            columTimeTask(i);
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[i].Cells[1].Value = ToDateTime(drNameTask[0]["Value"])));
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[i].Cells[2].Value = formatTime(drNameTask[1]["Value"].ToString())));
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[i].Cells[0].Value = drNameTask[0]["NAME_SHR"]));
                        }
                        else
                        {
                            columTimeTask(i);
                            TaskDataGridView.Rows[i].Cells[1].Value = drNameTask[0]["Value"];
                            TaskDataGridView.Rows[i].Cells[2].Value = formatTime(drNameTask[1]["Value"].ToString());
                            TaskDataGridView.Rows[i].Cells[0].Value = drNameTask[0]["NAME_SHR"];
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
                    if (TaskDataGridView.SelectedCells.Count > 0)
                        TaskDataGridView.SelectedCells[0].Selected = false;
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
                TaskDataGridView.Rows[i].Cells[3].Value = m_timeNow;
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
                    if (TaskDataGridView.InvokeRequired)
                        TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows.Add()));
                    else
                        TaskDataGridView.Rows.Add();
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
                    if (TaskDataGridView.Rows[m_check].Cells[0].Value.ToString() == "Усреднитель данных из СОТИАССО")
                        m_lim = limTaskAvg;
                    else m_lim = limTask;

                    if (int.Parse(drTask[i]["Link"].ToString()) == 1)
                    {
                        if (drTask[i]["Value"].ToString() == "")
                        {
                            if (TaskDataGridView.Columns[4].Visible == false)
                                TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[4].Visible = true));

                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[4].Value = "Задача не выполняется"));
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[5].Value = ""));
                            upselectrow(m_check);
                            m_counter--;
                        }
                        else
                            if (interruptTask(drTask[i + 1]["Value"].ToString()))
                        {
                            if (TaskDataGridView.Columns[4].Visible == false)
                                TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[4].Visible = true));

                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[4].Value = "Задача не выполняется"));
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[5].Value = ""));
                            upselectrow(m_check);
                            m_counter--;
                        }
                        else
                                if (TimeSpan.FromSeconds(Convert.ToDouble(drTask[i]["Value"])) > m_lim)
                        {
                            if (TaskDataGridView.Columns[4].Visible == false)
                                TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[4].Visible = true));

                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[4].Value = "Превышено время выполнения задачи"));
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[5].Value = ""));
                            upselectrow(m_check);
                            m_counter--;
                        }
                        else
                        {
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].DefaultCellStyle.BackColor = Color.White));
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[4].Value = ""));
                            TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[5].Value = ""));

                            if (m_counter == TaskDataGridView.Rows.Count)
                                if (TaskDataGridView.Columns[4].Visible == true)
                                    TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Columns[4].Visible = false));
                                else;
                            else
                                m_counter++;
                        }
                    }
                    else
                    {
                        TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[m_check].Cells[5].Value = "Запрещена"));
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
                if (TaskDataGridView.InvokeRequired)
                {
                    TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows.Insert(0, 1)));

                    for (int i = 0; i < TaskDataGridView.Rows[indxrow + 1].Cells.Count; i++)
                        TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[0].Cells[i].Value = TaskDataGridView.Rows[indxrow + 1].Cells[i].Value));

                    if (Convert.ToString(TaskDataGridView.Rows[0].Cells[4].Value) == "Задача не выполняется")
                        TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[0].DefaultCellStyle.BackColor = Color.Firebrick));
                    else
                        if (TaskDataGridView.Rows[0].Cells[4].Value.ToString() == "Превышено время выполнения задачи")
                        TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[0].DefaultCellStyle.BackColor = Color.Sienna));
                    else
                            if (TaskDataGridView.Rows[0].Cells[5].Value.ToString() == "Запрещена")
                        TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows[0].DefaultCellStyle.BackColor = Color.White));
                    TaskDataGridView.Invoke(new Action(() => TaskDataGridView.Rows.RemoveAt(indxrow + 1)));
                }
                else
                {
                    TaskDataGridView.Rows.InsertCopy(indxrow, 0);
                    TaskDataGridView.Rows.RemoveAt(indxrow);
                }
            }
        }
    }
}
