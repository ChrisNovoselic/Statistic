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
        private class PanelContainerTec : HPanelCommon
        {
            private PanelTec[] m_arPanels;

            public PanelContainerTec() : base(-1, -1)
            {
                initialize();
            }

            public PanelContainerTec(IContainer container) : base(container, -1, -1)
            {
                initialize();
            }

            private void InitComponents()
            {
            }

            protected override void initializeLayoutStyle(int col = -1, int row = -1)
            {
            }

            private void initialize()
            {
                string filter = string.Empty;

                InitComponents();

                // добавляем строки в таблицы для каждой ТЭЦ для каждого источника
                for (int i = 0; i < m_tableTECList.Rows.Count; i++) {
                    filter = "ID_EXT = " + Convert.ToInt32(m_tableTECList.Rows[i][0]);

                    m_arPanels[i].AddRows(m_tableSourceData.Select(filter).Length);
                }
            }

            /// <summary>
            /// Функция активации панелей ТЭЦ
            /// </summary>
            /// <param name="activated"></param>
            public override bool Activate(bool activated)
            {
                bool bRes = base.Activate(activated);

                if (bRes == true)
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
            /// Создание панелей ТЭЦ
            /// </summary>
            public void Create()
            {
                m_arPanels = new PanelTec[m_tableTECList.Rows.Count];

                for (int i = 0; i < m_arPanels.Length; i++)
                    if (m_arPanels[i] == null)
                        m_arPanels[i] = new PanelTec();
                    else
                        ;
            }

            /// <summary>
            /// очистка панелей
            /// </summary>
            public void Clear()
            {
                if (!(m_arPanels == null))
                    for (int i = 0; i < m_arPanels.Length; i++)
                        m_arPanels[i].Clear();
                else
                    ;
            }

            public void Update(object rec)
            {
                DataTable tableRecieved = rec as DataTable;

                string filter;

                for (int i = 0; i < m_tableTECList.Rows.Count; i++)
                {
                    filter = "ID_EXT = " + Convert.ToInt32(m_tableTECList.Rows[i][0]);

                    m_arPanels[i].Update(filter);
                }
            }

            /// <summary>
            /// Класс для описания элемента панели с информацией
            /// по дианостированию работоспособности 
            /// источников фактических, телеметрических значений (АИИС КУЭ, СОТИАССО) 
            /// </summary>
            private partial class PanelTec : HPanelCommon
            {
                private DataGridView m_dgvValues = new DataGridView();
                private Label LabelTec = new Label();
                private ContextMenuStrip ContextmenuChangeState;
                //private ToolStripMenuItem toolStripMenuItemActivate;
                //private ToolStripMenuItem toolStripMenuItemDeactivate;

                public PanelTec()
                    : base(-1, -1)
                {
                    initialize();
                }

                public PanelTec(IContainer container)
                    : base(container, -1, -1)
                {
                    container.Add(this);

                    initialize();
                }

                private void initialize()
                {
                    InitializeComponentTEC();
                }

                protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
                {
                    initializeLayoutStyleEvenly(cols, rows);
                }

                /// <summary>
                /// Требуется переменная конструктора.
                /// </summary>
                private IContainer components = null;

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

                private enum INDEX_CONTEXTMENU_ITEM : short { ACTIVATED = 0, DEACTIVATED }

                #region Код, автоматически созданный конструктором компонентов

                /// <summary>
                /// Обязательный метод для поддержки конструктора - не изменяйте
                /// содержимое данного метода при помощи редактора кода.
                /// </summary>
                private void InitializeComponentTEC()
                {
                    this.Controls.Add(LabelTec, 0, 0);
                    this.Controls.Add(m_dgvValues, 0, 1);
                    this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
                    this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.74641F));

                    this.ContextmenuChangeState = new System.Windows.Forms.ContextMenuStrip();
                    this.ContextmenuChangeState.SuspendLayout();

                    this.SuspendLayout();

                    this.m_dgvValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                    this.m_dgvValues.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    this.m_dgvValues.ClearSelection();
                    this.m_dgvValues.AllowUserToDeleteRows = false;
                    this.m_dgvValues.Dock = System.Windows.Forms.DockStyle.Fill;
                    this.m_dgvValues.RowHeadersVisible = false;
                    this.m_dgvValues.ReadOnly = true;
                    //this.TECDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.Fill);
                    this.m_dgvValues.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
                    this.m_dgvValues.AllowUserToAddRows = false;
                    this.m_dgvValues.Name = "TECDataGridView";
                    this.m_dgvValues.TabIndex = 0;
                    this.m_dgvValues.ColumnCount = (int)INDEX_CELL.COUNT;
                    this.m_dgvValues.Columns[(int)INDEX_CELL.NAME_SOURCE].Name = "Источник данных"; this.m_dgvValues.Columns[(int)INDEX_CELL.NAME_SOURCE].Width = 43;
                    this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VALUE].Name = "Крайнее время"; this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VALUE].Width = 57;
                    this.m_dgvValues.Columns[(int)INDEX_CELL.VALUE].Name = "Крайнее значение"; this.m_dgvValues.Columns[(int)INDEX_CELL.VALUE].Width = 35;
                    this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_NOW].Name = "Время проверки"; this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_NOW].Width = 57;
                    this.m_dgvValues.Columns[(int)INDEX_CELL.STATE].Name = "Связь"; this.m_dgvValues.Columns[(int)INDEX_CELL.STATE].Width = 35;

                    this.m_dgvValues.CellClick += new DataGridViewCellEventHandler(TECDataGridView_Cell);
                    this.m_dgvValues.CellValueChanged += new DataGridViewCellEventHandler(TECDataGridView_Cell);
                    this.m_dgvValues.CellMouseDown += new DataGridViewCellMouseEventHandler(TECDataGridView_CellMouseDown);
                    //
                    //ContextmenuChangeState
                    //
                    this.ContextmenuChangeState.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    new ToolStripMenuItem () //Activated
                    , new ToolStripMenuItem() }); //Deactivated
                    this.ContextmenuChangeState.Size = new System.Drawing.Size(180, 70);
                    this.ContextmenuChangeState.ShowCheckMargin = true;
                    // 
                    // toolStripMenuItemActivate
                    // 
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED].Name = "toolStripMenuItem1";
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED].Size = new System.Drawing.Size(179, 22);
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED].Text = "Activate";
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED].Click += new EventHandler(toolStripMenuItemActivate_Click);
                    (this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED] as ToolStripMenuItem).CheckOnClick = true;
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED].Tag = INDEX_CONTEXTMENU_ITEM.ACTIVATED;
                    // 
                    // toolStripMenuItemDeactivate
                    // 
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED].Name = "toolStripMenuItem2";
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED].Size = new System.Drawing.Size(179, 22);
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED].Text = "Deactivate";
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED].Click += new EventHandler(toolStripMenuItemDeactivate_Click);
                    (this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED] as ToolStripMenuItem).CheckOnClick = false;
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED].Tag = INDEX_CONTEXTMENU_ITEM.DEACTIVATED;
                    //
                    // LabelTec
                    //
                    this.LabelTec.AutoSize = true;
                    this.LabelTec.Name = "LabelTec";
                    this.LabelTec.TabIndex = 1;
                    this.LabelTec.Text = "Unknow_TEC";
                    this.LabelTec.Dock = System.Windows.Forms.DockStyle.Fill;
                    this.LabelTec.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                    this.ContextmenuChangeState.ResumeLayout(false);
                    this.ResumeLayout(false);
                }

                private enum INDEX_CELL : short
                {
                    NAME_SOURCE = 0, DATETIME_VALUE, VALUE, DATETIME_NOW, STATE
                    , COUNT
                }

                /// <summary>
                /// Обработчик события - нажатие кнопки "мыши"
                /// </summary>
                /// <param name="sender">Объект, инициировавший событие (DataGridView)</param>
                /// <param name="e">Аргумент события</param>
                void TECDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
                {
                    if ((e.Button == MouseButtons.Right) && (e.RowIndex > -1))
                        // только по нажатию правой кнопки и выбранной строки
                        if (m_dgvValues.Rows[e.RowIndex].Cells[(int)INDEX_CELL.NAME_SOURCE].Value.ToString() != "АИИСКУЭ")
                        {//??
                         // только для источников СОТИАССО 
                            RowIndex = e.RowIndex;
                            initContextMenu(m_dgvValues.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor == s_ColorSOTIASSOState[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED]);
                            if ((sender as DataGridView).Rows[e.RowIndex].Cells[e.ColumnIndex].ContextMenuStrip == null)
                                (sender as DataGridView).Rows[e.RowIndex].Cells[e.ColumnIndex].ContextMenuStrip = ContextmenuChangeState;
                            else
                                ;
                        }
                        else
                            ; // строки не являются описанием источников СОТИАССО
                    else
                        ; // нажата не правая кнопка ИЛИ не выбрана строка
                }

                #endregion
            }

            /// <summary>
            /// Класс для описания элемента панели с информацией
            /// по диагностированию работоспособности 
            /// источников фактических, телеметрических значений (АИИС КУЭ, СОТИАССО)
            /// </summary>
            partial class PanelTec
            {
                //static PanelDiagnostic[] m_arPanelsTEC;
                /// <summary>
                /// Список номер истчоников СОТИАССО
                /// </summary>
                enum TM { TM1 = 2, TM2, TM1T, TM2T };

                /// <summary>
                /// Номер строки вызова контекстного меню
                /// </summary>
                private int RowIndex;

                /// <summary>
                /// очистка панелей
                /// </summary>
                public void Clear()
                {
                    for (int j = 0; j < m_dgvValues.Rows.Count; j++)
                        if (m_dgvValues.Rows.Count > 0)
                            m_dgvValues.Rows.Clear();
                        else
                            ;
                }

                /// <summary>
                /// Изменение в массиве активного 
                /// источника СОТИАССО
                /// </summary>
                /// <param name="tm">номер истчоника</param>
                /// <param name="nameTM">имя источника</param>
                /// <param name="pos">позиция в массиве</param>
                private void changenumSource(int tm, string nameTM, int pos)
                {
                    m_arrayActiveSource.SetValue((tm), pos, 0);
                    m_arrayActiveSource.SetValue(nameTM, pos, 1);
                }

                /// <summary>
                /// Функция нахождения источника СОТИАССО
                /// </summary>
                /// <param name="nameTec">имя источника СОТИАССО</param>
                /// <returns>номер источника СОТИАССО</returns>
                private object selectionArraySource(string nameTec)
                {
                    DataRow[] arSel = m_tableSourceList.Select("NAME_SHR = '" + nameTec + "'");
                    object a = null;

                    for (int i = 0; i < arSel.Length; i++)
                        a = arSel[i]["ID"].ToString();

                    return a;
                }

                private static Color[] s_ColorSOTIASSOState = new Color[] { Color.DeepSkyBlue, Color.Empty };

                /// <summary>
                /// Обработка события клика по пункту меню "Active"
                /// для активации нового источника СОТИАССО
                /// </summary>
                /// <param name="sender"></param>
                /// <param name="e"></param>
                private void toolStripMenuItemActivate_Click(object sender, EventArgs e)
                {
                    string a = m_dgvValues.Rows[RowIndex].Tag.ToString();
                    int t = Convert.ToInt32(selectionArraySource(a));
                    int numberPanel = (t / 10) - 1;

                    updateTecTM(stringQuery(t, numberPanel + 1));

                    for (int i = 0; i < m_dgvValues.Rows.Count; i++)
                        if (m_dgvValues.Rows[i].Cells[(int)INDEX_CELL.NAME_SOURCE].Style.BackColor == s_ColorSOTIASSOState[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED])
                            paintCellDeactive(i);
                        else
                            ;

                    paintCellActive(RowIndex);
                    changenumSource(t, a, numberPanel);
                }

                /// <summary>
                /// Обработка события клика по пункту меню "Deactive"
                /// для деактивации активного итсочника СОТИАССО
                /// </summary>
                /// <param name="sender"></param>
                /// <param name="e"></param>
                private void toolStripMenuItemDeactivate_Click(object sender, EventArgs e)
                {
                    string a = m_dgvValues.Rows[RowIndex].Tag.ToString();
                    int t = Convert.ToInt32(selectionArraySource(a));
                    int numberPanel = (t / 10) - 1;

                    paintCellDeactive(RowIndex);
                    updateTecTM(stringQuery(0, numberPanel + 1));
                }

                /// <summary>
                /// Добавление строк
                /// </summary>
                /// <param name="cntRow">кол-во строк</param>
                public void AddRows(int cntRow)
                {
                    Action<int> actionAddRows = new Action<int>((cnt) => { m_dgvValues.Rows.Add(cnt); });

                    if (m_dgvValues.RowCount < cntRow / 2)
                        if (m_dgvValues.InvokeRequired)
                            m_dgvValues.Invoke(actionAddRows, cntRow / 2);
                        else
                            actionAddRows(cntRow / 2);
                    else
                        ;
                }

                /// <summary>
                /// Функция проверки на пустоту значений
                /// </summary>
                /// <param name="sourceDR">набор проверяемых данных</param>
                /// <returns></returns>
                private bool testingNull(ref DataRow[] sourceDR)
                {
                    bool bRes = false;

                    for (int i = 0; i < sourceDR.Count(); i++)
                        if (string.IsNullOrEmpty(sourceDR[i]["Value"].ToString()) == true)
                        {
                            sourceDR[i]["Value"] = "Нет данных в БД";
                            /*ничего не делаем*/
                            //bRes = false;
                        }
                        else
                            if (bRes == false) bRes = true; else /*ничего не делаем*/;

                    return bRes;
                }

                /// <summary>
                /// Функция заполнения данными элементов ТЭЦ
                /// </summary>
                /// <param name="filter">фильтр для обработки данных</param>
                public void Update(string filter)
                {
                    DataRow[] drTecSource;
                    string nameSource
                        , shortTime;

                    drTecSource = m_tableSourceData.Select(filter, "NAME_SHR DESC");

                    setTextColumn();

                    for (int r = 0, t = 0; r < m_dgvValues.Rows.Count; r++, t += 2)
                    {
                        nameSource = m_dgvValues.Rows[r].Cells[(int)INDEX_CELL.NAME_SOURCE].Value.ToString();

                        shortTime = formatTime(drTecSource[t + 1]["Value"].ToString(), nameSource);
                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[r].Cells[(int)INDEX_CELL.DATETIME_VALUE].Value = shortTime));
                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[r].Cells[(int)INDEX_CELL.VALUE].Value = drTecSource[t]["Value"]));
                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[r].Cells[(int)INDEX_CELL.DATETIME_NOW].Value =
                            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("hh:mm:ss:fff")));

                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[r].Tag = drTecSource[t]["NAME_SHR"]));

                        paintingCells(r);

                        if (testingNull(ref drTecSource) == true)
                            checkRelevanceValues(DateTime.Parse(shortTime), r);
                        else
                            ;
                    }

                    cellsPing();
                }

                /// <summary>
                /// Функция для подписи элементов 
                /// внутри элемента панели ТЭЦ
                /// </summary>
                private void SetTextDescription()
                {
                    int i = -1;

                    if (LabelTec.InvokeRequired)
                        LabelTec.Invoke(new Action(() => LabelTec.Text =
                            m_tableTECList.Rows[i][@"NAME_SHR"].ToString()));
                    else
                        LabelTec.Text = m_tableTECList.Rows[i][@"NAME_SHR"].ToString();
                }

                /// <summary>
                /// Функция перемеинования ячейки датагрид TEC
                /// </summary>
                /// <return></return>
                private void setTextColumn()
                {
                    string filterSourceData, filterParamDiagnostic;
                    DataRow[] arSelSourceData = null
                        , arSelParamDiagnostic = null;

                    Action<int, string> setTextNameSource =
                        new Action<int, string>((indx, name_shr) => { m_dgvValues.Rows[indx].Cells[(int)INDEX_CELL.NAME_SOURCE].Value = name_shr; });

                    //for (int k = 0; k < m_dtTECList.Rows.Count; k++)
                    //{
                        filterSourceData = "ID_Units = 12 and ID_EXT = '" + m_tec.m_id + "'";

                        for (int j = 0; j < m_dgvValues.Rows.Count; j++)
                        {
                            arSelSourceData = m_tableSourceData.Select(filterSourceData, "NAME_SHR DESC");

                            filterParamDiagnostic = "ID = '" + arSelSourceData[j]["ID_VALUE"] + "'";
                            arSelParamDiagnostic = m_tableParamDiagnostic.Select(filterParamDiagnostic);

                            if (m_dgvValues.InvokeRequired == true)
                                m_dgvValues.Invoke(setTextNameSource, j, arSelParamDiagnostic[0]["NAME_SHR"].ToString());
                            else
                                setTextNameSource(j, arSelParamDiagnostic[0]["NAME_SHR"].ToString());
                        }
                    //}
                }

                /// <summary>
                /// Заполнение элемента панели 
                /// информацией о связи с истчоником ТЭЦ
                /// </summary>
                private void cellsPing()
                {
                    DataRow[] arSel = null;

                    Action<int, INDEX_STATE> setState = new Action<int, INDEX_STATE>((iRow, iState) => {
                        m_dgvValues.Rows[iRow].Cells[(int)INDEX_CELL.STATE].Value = s_StateSources[(int)iState].m_Text;
                        m_dgvValues.Rows[iRow].Cells[(int)INDEX_CELL.STATE].Style.BackColor = s_StateSources[(int)iState].m_Color;
                    });

                    arSel = m_tableSourceData.Select(@"ID_EXT = " + Convert.ToInt32(m_tec.m_id));

                    for (int i = 0, t = 0; i < m_dgvValues.Rows.Count; i++, t += 2)
                        if (m_dgvValues.InvokeRequired == true)
                            m_dgvValues.Invoke(setState, i, (arSel[t]["Link"].ToString().Equals("1") == true) ? INDEX_STATE.OK : INDEX_STATE.ERROR);
                        else
                            setState(i, (arSel[t]["Link"].ToString().Equals("1") == true) ? INDEX_STATE.OK : INDEX_STATE.ERROR);
                }

                /// <summary>
                /// Функция выделение 
                /// неактивного истчоника СОТИАССО
                /// </summary>
                /// <param name="y">номер строки</param>
                private void paintCellDeactive(int y)
                {
                    paintCell(y, Color.Empty);
                }

                /// <summary>
                /// Функция выделения 
                /// активного источника СОТИАССО
                /// </summary>
                /// <param name="y">номер строки</param>
                private void paintCellActive(int y)
                {
                    paintCell(y, s_ColorSOTIASSOState[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED]);
                }

                private void paintCell(int y, Color clrCell)
                {
                    if (m_dgvValues.InvokeRequired)
                        m_dgvValues.Invoke(new Action(() => m_dgvValues.Rows[y].Cells[(int)INDEX_CELL.NAME_SOURCE].Style.BackColor = clrCell));
                    else
                        m_dgvValues.Rows[y].Cells[(int)INDEX_CELL.NAME_SOURCE].Style.BackColor = clrCell;
                }

                /// <summary>
                /// Функция для выделения источника
                /// </summary>
                /// <param name="y">номер строки</param>
                private void paintingCells(int y)
                {
                    string a = m_dgvValues.Rows[y].Tag.ToString();
                    string b;

                    for (int i = 0; i < m_arrayActiveSource.Length / 2; i++)
                    {
                        b = m_arrayActiveSource[i, 1].ToString();

                        if (a == b)
                        {
                            paintCellActive(y);
                            break;
                        }
                        else
                            paintCellDeactive(y);
                    }
                }

                /// <summary>
                /// Подключение к ячейке контекстного меню
                /// </summary>
                /// <param name="y">номер строки</param>
                private void initContextMenu(bool bActived)
                {
                    (this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED] as ToolStripMenuItem).CheckState = bActived == true ? CheckState.Checked : CheckState.Unchecked;
                    (this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED] as ToolStripMenuItem).CheckState = bActived == false ? CheckState.Checked : CheckState.Unchecked;
                }

                /// <summary>
                /// Обработчик события - при "щелчке" по любой части ячейки
                /// </summary>
                /// <param name="sender">Объект, инициировавший событие - (???ячейка, скорее - 'DataGridView')</param>
                /// <param name="e">Аргумент события</param>
                private void TECDataGridView_Cell(object sender, EventArgs e)
                {
                    try
                    {
                        if (m_dgvValues.SelectedCells.Count > 0)
                            m_dgvValues.SelectedCells[0].Selected = false;
                        else
                            ;
                    }
                    catch
                    {
                    }
                }

                /// <summary>
                /// Проверка актуальности времени 
                /// СОТИАССО и АИИСКУЭ
                /// </summary>
                /// <param name="time"></param>
                /// <param name="r">индекс строки</param>
                private void checkRelevanceValues(DateTime time, int r)
                {
                    string nameSource = m_dgvValues.Rows[r].Cells[(int)INDEX_CELL.NAME_SOURCE].Value.ToString();

                    if ((!(nameSource == "АИИСКУЭ"))
                        && m_dgvValues.Rows[r].Cells[(int)INDEX_CELL.NAME_SOURCE].Style.BackColor == System.Drawing.Color.Empty)
                        paintValuesSource(false, r);
                    else
                        paintValuesSource(selectInvalidValue(nameSource, time), r);
                }

                /// <summary>
                /// Проверка разницы времени между эталоном и источником
                /// </summary>
                /// <param name="timeEtalon">эталонное время</param>
                /// <param name="timeSource">время источника</param>
                /// <returns>флаг о правильности времени</returns>
                private bool diffTime(DateTime timeEtalon, DateTime timeSource)
                {
                    TimeSpan VALIDATE_TM = TimeSpan.FromSeconds(VALIDATE_ASKUE_TM);
                    TimeSpan ts = timeEtalon - (timeSource + VALIDATE_TM);
                    TimeSpan validateTime = TimeSpan.FromSeconds(180);

                    if (ts > validateTime)
                        return true;
                    else
                        return false;
                }
                /// <summary>
                /// Проверка актуальности времени источника
                /// </summary>
                /// <param name="nameS">имя источника</param>
                /// <param name="time">время источника</param>
                /// <param name="numberPanel">нопмер панели</param>
                /// <returns></returns>
                private bool selectInvalidValue(string nameS, DateTime time)
                {
                    DateTime DTnowAISKUE = SERVER_TIME;
                    TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
                    DateTime DTnowSOTIASSO;

                    if (m_tec.Type == StatisticCommon.TEC.TEC_TYPE.BIYSK)
                        DTnowSOTIASSO = TimeZoneInfo.ConvertTime(SERVER_TIME, TimeZoneInfo.Local);
                    else
                        DTnowSOTIASSO = TimeZoneInfo.ConvertTimeToUtc(SERVER_TIME, tzInfo);

                    bool bFL = true; ;

                    switch (nameS)
                    {
                        case "АИИСКУЭ":
                            if (diffTime(DTnowAISKUE, time))
                                bFL = true;
                            else
                                bFL = false;
                            break;
                        case "СОТИАССО":
                        case "СОТИАССО_TorIs":
                        case "СОТИАССО_0":
                            if (diffTime(DTnowSOTIASSO, time))
                                bFL = true;
                            else
                                bFL = false;
                            break;
                        default:
                            break;
                    }

                    return bFL;
                }

                /// <summary>
                /// Выделение значения источника
                /// </summary>
                /// <param name="bflag"></param>
                /// <param name="i">номер панели</param>
                /// <param name="r">номер строки</param>
                private void paintValuesSource(bool bflag, int r)
                {
                    m_dgvValues.Rows[r].Cells[(int)INDEX_CELL.VALUE].Style.BackColor = bflag == true ? Color.Firebrick : Color.Empty;
                }

                private StatisticCommon.TEC m_tec;

                /// <summary>
                /// Форматирование даты
                /// “HH:mm:ss.fff”
                /// </summary>
                /// <param name="datetime">дата входная</param>
                /// <param name="Npanel">номер панели</param>
                /// <param name="nameSource">имя источника</param>
                /// <returns>дата сформированная</returns>
                private string formatTime(string datetime, string nameSource)
                {
                    string strRes = string.Empty;

                    DateTime result;                    
                    bool dt2Time = DateTime.TryParse(datetime, out result);

                    switch (nameSource)
                    {
                        case "СОТИАССО":
                        case "СОТИАССО_TorIs":
                        case "СОТИАССО_0":
                            if (m_tec.Type == StatisticCommon.TEC.TEC_TYPE.BIYSK)
                                result = result.AddHours(TimeZoneInfo.Local.BaseUtcOffset.Hours);
                            break;
                        default:
                            break;
                    }

                    if (!(dt2Time == false))
                        if (Convert.ToInt32(result.Day - DateTime.Now.Day) < 0)
                            strRes = result.ToString("dd.MM.yy HH:mm:ss");
                        else
                            strRes = result.ToString("HH:mm:ss.fff");
                    else
                        strRes = result.ToString();

                    return strRes;
                }
            }
        }
    }
}
