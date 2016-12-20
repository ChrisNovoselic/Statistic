using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Drawing;

using HClassLibrary;

namespace StatisticDiagnostic
{
    partial class PanelStatisticDiagnostic
    {
        private class PanelContainerModes : HPanelCommon
        {
            private PanelModes[] m_arPanels;

            public PanelContainerModes(ListDiagnosticSource listDiagSource) : base(-1, -1)
            {
            }

            public PanelContainerModes(IContainer container) : base(container, -1, -1)
            {
            }

            private void InitComponents()
            {
            }

            protected override void initializeLayoutStyle(int col = -1, int row = -1)
            {
            }

            public void Clear()
            {
                m_arPanels.ToList().ForEach(panel => panel.Clear());
            }

            /// <summary>
            /// Функция активации панелей модес
            /// </summary>
            /// <param name="activated">параметр активации</param>
            public override bool Activate(bool activated)
            {
                bool bRes = base.Activate(activated);

                if (activated == true)
                    if (!(m_arPanels == null))
                        for (int i = 0; i < m_arPanels.Length; i++)
                            m_arPanels[i].Focus();
                    else
                        ;
                else
                    ;

                return bRes;
            }

            /// <summary>
            /// Создание панелей Модес
            /// </summary>
            private void create(ListDiagnosticSource listDiagSource)
            {
                m_arPanels = new PanelModes[m_tableTECList.Rows.Count + 1];

                for (int i = 0; i < m_arPanels.Length; i++)
                    if (m_arPanels[i] == null)
                        m_arPanels[i] = new PanelModes(listDiagSource);
                    else
                        ;
            }

            /// <summary>
            /// Класс для описания элемента панели с информацией
            /// значений параметров диагностики работоспособности 
            /// источников значений ПБР (Модес-, центр, терминал)
            /// </summary>
            private partial class PanelModes : HPanelCommon
            {
                ListDiagnosticSource m_listDiagnosticSource;

                /// <summary>
                /// Конструктор - основной (с параметрами)
                /// </summary>
                public PanelModes(ListDiagnosticSource listDiagSource)
                    : base(-1, -1)
                {
                    initialize(listDiagSource);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="container"></param>
                public PanelModes(IContainer container, ListDiagnosticSource listDiagSource)
                    : base(container, -1, -1)
                {
                    container.Add(this);

                    initialize(listDiagSource);
                }
                /// <summary>
                /// 
                /// </summary>
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
                public DataGridView m_dgvValues = new DataGridView();
                public Label LabelModes = new Label();

                private void InitializeComponent()
                {
                    this.Controls.Add(LabelModes, 0, 0);
                    this.Controls.Add(m_dgvValues, 0, 1);
                    this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
                    this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.74641F));

                    this.SuspendLayout();

                    this.Dock = DockStyle.Fill;
                    //
                    //ModesDataGridView
                    //
                    this.m_dgvValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                    this.m_dgvValues.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
                    this.m_dgvValues.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    this.m_dgvValues.Dock = DockStyle.Fill;
                    this.m_dgvValues.ClearSelection();
                    this.m_dgvValues.AllowUserToAddRows = false;
                    this.m_dgvValues.RowHeadersVisible = false;
                    this.m_dgvValues.Name = "ModesDataGridView";
                    this.m_dgvValues.CurrentCell = null;
                    this.m_dgvValues.TabIndex = 0;
                    this.m_dgvValues.ReadOnly = true;
                    this.m_dgvValues.ColumnCount = 5;
                    this.m_dgvValues.Columns[0].Name = "Источник данных";
                    this.m_dgvValues.Columns[1].Name = "Крайнее значение";
                    this.m_dgvValues.Columns[2].Name = "Крайнее время";
                    this.m_dgvValues.Columns[3].Name = "Время проверки";
                    this.m_dgvValues.Columns[4].Name = "Связь";
                    this.m_dgvValues.Columns[0].Width = 22;
                    this.m_dgvValues.Columns[1].Width = 15;
                    this.m_dgvValues.Columns[2].Width = 23;
                    this.m_dgvValues.Columns[3].Width = 20;
                    this.m_dgvValues.Columns[4].Width = 25;

                    this.m_dgvValues.CellValueChanged += new DataGridViewCellEventHandler(m_arPanelsMODES_Cell);
                    this.m_dgvValues.CellClick += new DataGridViewCellEventHandler(m_arPanelsMODES_Cell);
                    //
                    //LabelModes
                    //
                    this.LabelModes.AutoSize = true;
                    this.LabelModes.Dock = System.Windows.Forms.DockStyle.Fill;
                    this.LabelModes.Name = "LabelModes";
                    this.LabelModes.TabIndex = 1;
                    this.LabelModes.Text = " ";
                    this.LabelModes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

                    this.ResumeLayout(false);
                }

                #endregion
            }

            /// <summary>
            /// Класс для описания элемента панели с информацией
            /// значений параметров диагностики работоспособности 
            /// источников значений ПБР (Модес-, центр, терминал)
            /// </summary>
            partial class PanelModes
            {
                /// <summary>
                /// очистка панелей
                /// </summary>
                public void Clear()
                {
                    if (m_dgvValues.Rows.Count > 0)
                        m_dgvValues.Rows.Clear();
                    else
                        ;
                }

                /// <summary>
                /// Добавление строк в грид
                /// </summary>
                /// <param name="counter">кол-во строк</param>
                private void addRows(int counter)
                {
                    if (m_dgvValues.InvokeRequired)
                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows.Add(counter)));
                    else
                        m_dgvValues.Rows.Add(counter);
                }

                /// <summary>
                /// Функция проверки на пустоту значений
                /// </summary>
                /// <param name="arSource">набор проверяемых данных</param>
                /// <returns></returns>
                private bool testingNull(ref DataRow[] arSource)
                {
                    bool bRes = false;

                    for (int i = 0; i < arSource.Length; i++)
                        if (arSource[i]["Value"].ToString() == "")
                        {
                            arSource[i]["Value"] = "Нет данных в БД";
                            //bRes = false; // ничего не делать
                        } else
                            if (bRes == false) bRes = true; else /*ничего не делать*/;

                    return bRes;
                }

                /// <summary>
                /// заполненеи панели МС данными
                /// </summary>
                private void insertDataMC()
                {
                    string filterComp = string.Empty
                        , sortOrderBy = "Component ASC"
                        , time;
                    DataRow[] arSelIDModes, arSelIDComponent;
                    List<DIAGNOSTIC_SOURCE> listDiagnosticSource;
                    int iRow = -1;

                    if (m_dgvValues.ColumnCount < 6) {
                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Columns.Add("TEC", "ТЭЦ")));
                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Columns["TEC"].DisplayIndex = 1));
                    } else
                        ;

                    listDiagnosticSource = m_listDiagnosticSource.FindAll(item => { return item.m_description == @"Modes-Centre"; });
                    listDiagnosticSource = listDiagnosticSource.OrderBy(item => item.m_id_component).ToList();

                    for (int d = 0; d < listDiagnosticSource.Count - 1; d++)
                    {
                        filterComp = @"ID_Value = '" + listDiagnosticSource[d + 1].m_id_component + "'";

                        if (m_dgvValues.Rows.Count < listDiagnosticSource.Count - 1)
                            addRows(1);
                        else;

                        arSelIDComponent = m_tableSourceData.Select(filterComp);
                        time = formatTime(arSelIDComponent[1]["Value"].ToString());
                        iRow++;

                        if (m_dgvValues.InvokeRequired) {
                            if (testingNull(ref arSelIDComponent) == true) {
                                paintPbr(iRow, !(checkPBR() == arSelIDComponent[0]["Value"].ToString()));
                            } else
                                ;

                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[iRow].Cells[5].Value = arSelIDComponent[0][5]));
                            //m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[m_tic].Cells[0].Value = m_drComponent[0]["ID_Value"]));
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[iRow].Cells[2].Value = time));
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[iRow].Cells[1].Value = arSelIDComponent[0]["Value"]));
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[iRow].Cells[3].Value = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("hh:mm:ss:fff")));

                            cellsPing(filterComp, iRow);
                        } else {
                            paintPbr(iRow, !(checkPBR() == arSelIDComponent[0]["Value"].ToString()));

                            //m_dgvValues.Rows[m_tic].Cells[0].Value = m_drComponent[0]["ID_Value"];
                            m_dgvValues.Rows[iRow].Cells[2].Value = time;
                            m_dgvValues.Rows[iRow].Cells[1].Value = arSelIDComponent[0]["Value"];
                            m_dgvValues.Rows[iRow].Cells[3].Value = DateTime.Now.ToString("HH:mm:ss.fff");

                            cellsPing(filterComp, iRow);
                        }

                        nameComponentGTP(arSelIDComponent, iRow);
                    }
                }

                /// <summary>
                /// добавление записей в грид
                /// </summary>
                /// <param name="id">фильтр для отбора данных</param>
                private void insertDataModes(int id)
                {
                    string textDateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("hh:mm:ss:fff")
                        , filterComp;
                    List<DIAGNOSTIC_SOURCE> listDiagnosticSource;
                    DataRow[] arSelSourceModes = null;

                    if (m_listDiagnosticSource[i].m_name_shr == "Modes-Centre")
                        insertDataMC(i);
                    else
                    {
                        listDiagnosticSource = m_listDiagnosticSource.FindAll(item => { return item.m_id == id; });
                        listDiagnosticSource = listDiagnosticSource.OrderBy(item => item.m_id_component).ToList();

                        for (int r = 0; r < listDiagnosticSource.Count; r++)
                        {
                            filterComp = "ID_Value = '" + listDiagnosticSource[r].m_id_component + "'";

                            if (m_dgvValues.Rows.Count < listDiagnosticSource.Count)
                                addRows(i, 1);
                            else
                                ;

                            arSelSourceModes = m_tableSourceData.Select(filterComp);

                            if (m_dgvValues.InvokeRequired)
                            {
                                if (testingNull(ref arSelSourceModes))
                                    if (checkPBR() == arSelSourceModes[0]["Value"].ToString())
                                        paintPbr(r, false);
                                    else
                                        paintPbr(r, true);
                                else
                                    ;

                                //m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[r].Cells[0].Value = m_drSourceModes[0]["ID_Value"]));
                                m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[r].Cells[1].Value = arSelSourceModes[0]["Value"]));
                                m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[r].Cells[2].Value = formatTime(arSelSourceModes[1]["Value"].ToString())));
                                m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[r].Cells[3].Value = textDateTime));

                                cellsPing(filterComp, i, r);
                            }
                            else
                            {
                                if (checkPBR() == arSelSourceModes[0]["Value"].ToString())
                                    paintPbr(r, false);
                                else
                                    paintPbr(r, true);

                                //m_dgvValues.Rows[r].Cells[0].Value = m_drSourceModes[0]["ID_Value"];
                                m_dgvValues.Rows[r].Cells[1].Value = arSelSourceModes[0]["Value"];
                                //m_dgvValues.Rows[r].Cells[2].Value = formatTime(m_drSourceModes[1]["Value"].ToString());
                                m_dgvValues.Rows[r].Cells[3].Value = textDateTime;

                                cellsPing(filterComp, i, r);
                            }

                            nameComponentGTP(arSelSourceModes, r, i);
                        }
                    }
                }

                /// <summary>
                /// Функция Заполнения панелей Модес
                /// </summary>
                public void AddItem()
                {
                    try {
                        var m_enumModes = (from r in m_listDiagnosticSource
                                           where r.m_id >= (int)INDEX_SOURCE.MODES && r.m_id < (int)INDEX_SOURCE.TASK
                                           orderby r.m_id
                                           select new
                                           {
                                               ID = r.m_id,
                                           }).Distinct();

                        insertDataModes(Convert.ToInt32(m_enumModes.ElementAt(i).ID));

                        SetItemNameShr();
                    } catch (Exception e) {
                        MessageBox.Show(e.ToString());
                    }
                }

                /// <summary>
                /// Функция изменения заголовков грида Modes
                /// </summary>
                private void SetItemNameShr()
                {
                    string m_nameshr;
                    var m_enumModes = (from r in m_listDiagnosticSource.AsEnumerable()
                                       where r.m_id >= (int)INDEX_SOURCE.MODES && r.m_id < (int)INDEX_SOURCE.TASK
                                       orderby r.m_id
                                       select new
                                       {
                                           NAME_SHR = r.m_name_shr,
                                       }).Distinct();

                        m_nameshr = m_enumModes.ToArray().ElementAt(i).NAME_SHR;

                        if (LabelModes.InvokeRequired)
                            LabelModes.Invoke(new Action(() => LabelModes.Text = m_nameshr));
                        else
                            LabelModes.Text = m_nameshr;
                }

                /// <summary>
                /// Функция подписи источников ПБР
                /// </summary>
                private void nameComponentGTP(DataRow[] dtComp, int rownext)
                {
                    for (int j = 0; j < m_tableGTPList.Rows.Count; j++)
                        if (m_tableGTPList.Rows[j]["ID"].ToString() == dtComp[0]["ID_Value"].ToString())
                        {
                            if (m_dgvValues.InvokeRequired == true)
                                m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[rownext].Cells[0].Value = m_tableGTPList.Rows[j]["NAME_SHR"]));
                            else
                                m_dgvValues.Rows[rownext].Cells[0].Value = m_tableGTPList.Rows[j]["NAME_SHR"];
                        }
                        else
                            ;
                }

                /// <summary>
                /// Заполнение панели данными о связи 
                /// с источниками для МОДЕС
                /// </summary>
                /// <param name="f">фильтр для отбора данных</param>
                /// <param name="r">номер строки</param>
                private void cellsPing(string f, int k, int r)
                {
                    DataRow[] m_drLink = m_tableSourceData.Select(f);

                    if (m_dgvValues.InvokeRequired)
                    {
                        if (m_drLink[0]["Link"].ToString() == "1")
                        {
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[r].Cells[4].Value = "Да"));
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[r].Cells[4].Style.BackColor = System.Drawing.Color.White));
                        }
                        else
                        {
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[r].Cells[4].Value = "Нет"));
                            m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[r].Cells[4].Style.BackColor = System.Drawing.Color.OrangeRed));
                        }
                    }
                    else
                    {
                        if (m_drLink[0]["Link"].ToString() == "1")
                        {
                            m_dgvValues.Rows[r].Cells[4].Value = "Да";
                            m_dgvValues.Rows[r].Cells[4].Style.BackColor = System.Drawing.Color.White;
                        }
                        else
                        {
                            m_dgvValues.Rows[r].Cells[4].Value = "Нет";
                            m_dgvValues.Rows[r].Cells[4].Style.BackColor = System.Drawing.Color.OrangeRed;
                        }
                    }
                }

                /// <summary>
                /// снятие выделения ячейки
                /// </summary>
                /// <param name="sender">параметр</param>
                /// <param name="e">событие</param>
                private void m_arPanelsMODES_Cell(object sender, EventArgs e)
                {
                    try
                    {
                        if (m_dgvValues.SelectedCells.Count > 0)
                            m_dgvValues.SelectedCells[0].Selected = false;
                        else;
                    }
                    catch { }
                }

                /// <summary>
                /// Выделение верного ПБР
                /// </summary>
                /// <param name="numR">номер строки</param>
                private void paintPbr(int numR, bool bError)
                {
                    Color clrPbr = bError == true ? Color.Red : Color.Empty;

                    if (m_dgvValues.InvokeRequired == true)
                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[numR].Cells[1].Style.BackColor = Color.White));
                    else
                        m_dgvValues.Rows[numR].Cells[1].Style.BackColor = Color.White;
                }

                /// <summary>
                /// Функция для нахождения ПБР на текущее время
                /// </summary>
                /// <returns>возвращает ПБР на текущее время</returns>
                private string checkPBR()
                {
                    string m_etalon_pbr = string.Empty,
                     m_DTMin = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("mm"),
                     m_DTHour = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("HH");

                    //if ((Convert.ToInt32(m_DTMin)) > 41)
                    //{
                    //    if ((Convert.ToInt32(m_DTHour)) % 2 == 0)
                    //        m_etalon_pbr = "ПБР" + (Convert.ToInt32(m_DTHour) + 1);
                    //    else
                    //        m_etalon_pbr = "ПБР" + ((Convert.ToInt32(m_DTHour) + 2));

                    //    return m_etalon_pbr;
                    //}

                    //else
                    //{
                    //if ((Convert.ToInt32(m_DTHour)) % 2 == 0)
                    m_etalon_pbr = "ПБР" + (Convert.ToInt32(m_DTHour) + 1);
                    //else
                    //    m_etalon_pbr = "ПБР" + Convert.ToInt32(m_DTHour);

                    return m_etalon_pbr;
                    //}
                }

                /// <summary>
                /// Форматирование даты
                /// “HH:mm:ss.fff”
                /// </summary>
                /// <param name="datetime">дата/время</param>
                /// <returns>отформатированная дата</returns>
                private string formatTime(string datetime)
                {
                    DateTime result;
                    string m_dt, m_dt2Time = DateTime.TryParse(datetime, out result).ToString();

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
            }
        }
    }
}
