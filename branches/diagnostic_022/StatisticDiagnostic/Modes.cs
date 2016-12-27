﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Drawing;

using HClassLibrary;
using StatisticCommon;

namespace StatisticDiagnostic
{
    partial class PanelStatisticDiagnostic
    {
        private class PanelContainerModes : HPanelCommon
        {
            /// <summary>
            /// Количество столбцов, строк в сетке макета
            /// </summary>
            private const int COUNT_LAYOUT_COLUMN = 4
                , COUNT_LAYOUT_ROW = 2;
            /// <summary>
            /// Сложный ключ для соваря со значениями при обновлении дочерних панелей
            /// </summary>
            private struct KEY_DIAGNOSTIC_PARAMETER
            {
                /// <summary>
                /// Перечисление - типов значений
                /// </summary>
                public enum ID_UNIT : short { UNKNOWN = -1, PBR = 12, DATETIME = 13 }
                /// <summary>
                /// Словарь с информацией о CLR-типах значений 
                /// </summary>
                public static Dictionary<ID_UNIT, Type> TypeOf = new Dictionary<ID_UNIT, Type>() {
                    { ID_UNIT.PBR, typeof(string) }
                    , { ID_UNIT.DATETIME, typeof(DateTime) }
                };

                public ID_UNIT m_id_unit;

                public int m_id_value;
            }
            /// <summary>
            /// Массив дочерних панелей для каждой из Модес-источников
            /// </summary>
            private PanelModes[] m_arPanels;
            /// <summary>
            /// Конструктор основной (с парметрами)
            /// </summary>
            /// <param name="listTEC">Список ТЭЦ</param>
            /// <param name="listDiagParam">Список с параметрами диагностики (БД конфигурации)</param>
            /// <param name="listDiagSource">Список контролируемых источников данных (БД конфигурации)</param>
            public PanelContainerModes(List<TEC> listTEC
                , List<DIAGNOSTIC_PARAMETER> listDiagParam
                , ListDiagnosticSource listDiagSource)
                    : base(COUNT_LAYOUT_COLUMN, COUNT_LAYOUT_ROW)
            {
                initialize(listTEC, listDiagParam, listDiagSource);
            }
            /// <summary>
            /// Конструктор дополнительный (с параметрами)
            /// </summary>
            /// <param name="container">Объект - владелец</param>
            /// <param name="listTEC">Список ТЭЦ</param>
            /// <param name="listDiagnosticParameter">Список с параметрами диагностики (БД конфигурации)</param>
            /// <param name="listDiagSource">Список контролируемых источников данных (БД конфигурации)</param>
            public PanelContainerModes(IContainer container
                , List<TEC> listTEC
                , List<DIAGNOSTIC_PARAMETER> listDiagnosticParameter
                , ListDiagnosticSource listDiagSource)
                    : base(container, COUNT_LAYOUT_COLUMN, COUNT_LAYOUT_ROW)
            {
                initialize(listTEC, listDiagnosticParameter, listDiagSource);
            }
            /// <summary>
            /// Инициализация (создание/размещение) дочерних элементов управления
            /// </summary>
            private void InitComponents()
            {
                this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

                initializeLayoutStyle();
            }
            /// <summary>
            /// Инициализация характеристик, стилей макета для размещения дочерних элементов интерфейса
            ///  (должна быть вызвана явно)
            /// </summary>
            /// <param name="col">Количество столбцов в макете</param>
            /// <param name="row">Количество строк в макете</param>
            protected override void initializeLayoutStyle(int col = -1, int row = -1)
            {
                initializeLayoutStyleEvenly(col, row);
            }
            /// <summary>
            /// Инициализация (создание/размещение) дочерних элементов управления
            /// </summary>
            /// <param name="listTEC">Список ТЭЦ</param>
            /// <param name="listDiagnosticParameter">Список с параметрами диагностики (БД конфигурации)</param>
            /// <param name="listDiagnosticSource">Список контролируемых источников данных (БД конфигурации)</param>
            private void initialize(List<TEC> listTEC
                , List<DIAGNOSTIC_PARAMETER> listDiagnosticParameter
                , ListDiagnosticSource listDiagnosticSource)
            {
                int i = -1;
                ListDiagnosticParameter listDiagParam;
                ListDiagnosticSource listDiagSrc;

                InitComponents();
                // 1 DataGridView(Модес-Центр) + на каждую ТЭЦ по DataGridView(Модес-Терминал)
                m_arPanels = new PanelModes[listTEC.Count + 1];
                // добавляем 'DataGridView' Modes-Centre - подготовка конфигурациооных списков
                i = 0;
                listDiagParam = new ListDiagnosticParameter(listDiagnosticParameter.FindAll(item => { return item.m_name_shr == @"Modes-Centre"; }));
                listDiagParam = new ListDiagnosticParameter(listDiagParam.OrderBy(item => item.m_id));
                listDiagSrc = new ListDiagnosticSource(listDiagnosticSource.FindAll(item => { return item.m_description == @"Modes-Centre"; }));
                listDiagSrc = new ListDiagnosticSource(listDiagSrc.OrderBy(item => item.m_id_component));
                // добавляем 'DataGridView' Modes-Centre - создание панели
                m_arPanels[i] = new PanelModes(listTEC, listDiagParam, listDiagSrc);
                // добавляем 'DataGridView' Modes-Centre - размещение панели
                this.Controls.Add(m_arPanels[i], 0, 0); this.SetRowSpan(m_arPanels[i], 2);

                for (i = 1; i < m_arPanels.Length; i++)
                    if (m_arPanels[i] == null) {
                        // добавляем 'DataGridView' Modes-Terminal - подготовка конфигурациооных списков
                        listDiagParam = new ListDiagnosticParameter(listDiagnosticParameter.FindAll(item => { return item.m_name_shr == @"Modes-term"; }));
                        listDiagParam = new ListDiagnosticParameter(listDiagParam.OrderBy(item => item.m_id));
                        listDiagSrc = new ListDiagnosticSource();
                        foreach (DIAGNOSTIC_SOURCE diagSrc in listDiagnosticSource)
                            // "-1", т.к. начинали с "1"
                            foreach (TECComponent comp in listTEC[i - 1].list_TECComponents)
                                if ((comp.IsGTP == true)
                                    && (diagSrc.m_id_component == comp.m_id))
                                    listDiagSrc.Add(diagSrc);
                                else
                                    ;
                        listDiagSrc = new ListDiagnosticSource(listDiagSrc.OrderBy(item => item.m_id_component));
                        // добавляем 'DataGridView' Modes-Terminal - создание панели
                        m_arPanels[i] = new PanelModes(new List<TEC>() { listTEC[i - 1] }, listDiagParam, listDiagSrc);
                        // добавляем 'DataGridView' Modes-Terminal - размещение панели
                        this.Controls.Add(m_arPanels[i], ((i - 1) % (COUNT_LAYOUT_COLUMN - 1)) + 1, (i - 1) / (COUNT_LAYOUT_COLUMN - 1));
                    } else
                        ;

                //try {
                //    IEnumerable<DIAGNOSTIC_SOURCE> enumModes = (from r in m_listDiagnosticSource
                //        where r.m_id >= (int)INDEX_SOURCE.MODES && r.m_id < (int)INDEX_SOURCE.TASK
                //        orderby r.m_id
                //        select new DIAGNOSTIC_SOURCE()
                //        {
                //            m_id = r.m_id
                //            , m_name_shr = r.m_name_shr
                //            , m_id_component = r.m_id_component
                //            , m_description = r.m_description
                //        }).Distinct();

                //    foreach (var item in enumModes)
                //        if (item.m_description.Equals(@"Modes-Centre") == true)
                //            createItemModesCentre();
                //        else
                //            createItemModesTerminal(item.m_id);
                //} catch (Exception e) {
                //    Logging.Logg().Exception(e, @"", Logging.INDEX_MESSAGE.NOT_SET);
                //}
            }

            public void Clear()
            {
                m_arPanels?.ToList().ForEach(panel => panel.Clear());
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

            private class DictionaryGTPValues : Dictionary<int, Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values>>
            {
                /// <summary>
                /// Конструктор - основной (с парметром)
                /// </summary>
                /// <param name="tableRecieved">Таблица - результат запроса значений</param>
                public DictionaryGTPValues(DataTable tableRecieved)
                {
                    int id = -1;

                    try {
                        foreach (DataRow r in tableRecieved.Rows) {
                            id = r.Field<int>(@"ID_EXT"); // читать как ИД ТЭЦ

                            if ((id > (int)INDEX_SOURCE.MODES)
                                && (id < (int)INDEX_SOURCE.TASK)) {
                                if (this.Keys.Contains(id) == false)
                                    Add(id, new Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values>());
                                else
                                    ;

                                this[id].Add(
                                    new KEY_DIAGNOSTIC_PARAMETER() {
                                        m_id_unit = (KEY_DIAGNOSTIC_PARAMETER.ID_UNIT)r.Field<int>(@"ID_Units")
                                        //??? читать как ИД ГТП
                                        , m_id_value = Convert.ToInt32(r.Field<string>(@"ID_Value"))
                                    }
                                    , new Values() {
                                        m_value = r.Field<string>(@"Value")
                                        , m_strLink = r.Field<string>(@"Link")
                                        , m_name_shr = r.Field<string>(@"NAME_SHR")
                                        , m_dtValue = r.Field<DateTime>(@"UPDATE_DATETIME")
                                    }
                                );
                            } else
                            // 'INDEX_SOURCE.SIZEDB' - минимальное значение
                            // идентификатор ТЭЦ в диапазоне 1 - 10 
                                ;
                        }                        
                    } catch (Exception e) {
                        Logging.Logg().Exception(e, @"PanelContainerModes.DictionaryGTPValues::ctor () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                }
            }

            /// <summary>
            /// Обработчик события панели-родителя - обновление значений
            /// </summary>
            /// <param name="rec">Таблица с результатами запрос</param>
            public void Update(object rec)
            {
                PanelModes panelModes;
                DictionaryGTPValues dictTecValues = null;

                //m_semUpdateHandler.WaitOne();

                dictTecValues = new DictionaryGTPValues((rec as DataTable));

                foreach (KeyValuePair<int, Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values>> pair in dictTecValues) {
                    panelModes = m_arPanels.ToList().Find(panel => (int)panel.Tag == pair.Key);

                    if (!(panelModes == null)) {
                        m_arPanels[0].Update(pair.Value);
                        panelModes.Update(pair.Value);
                    } else
                        ;
                }

                //m_semUpdateHandler.Release(1);
            }

            /// <summary>
            /// Класс для описания элемента панели с информацией
            /// значений параметров диагностики работоспособности 
            /// источников значений ПБР (Модес-, центр, терминал)
            /// </summary>
            private partial class PanelModes : HPanelCommon
            {
                /// <summary>
                /// Количество столбцов, строк в сетке макета
                /// </summary>
                private const int COUNT_LAYOUT_COLUMN = 1
                    , COUNT_LAYOUT_ROW = 7;
                /// <summary>
                /// Конструктор - основной (с параметрами)
                /// </summary>
                public PanelModes(List<TEC> listTEC, List<DIAGNOSTIC_PARAMETER>listDiagParam, List<DIAGNOSTIC_SOURCE> listDiagSrc)
                    : base(COUNT_LAYOUT_COLUMN, COUNT_LAYOUT_ROW)
                {
                    initialize(listTEC, listDiagParam, listDiagSrc);
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="container"></param>
                public PanelModes(IContainer container, List<TEC> listTEC, List<DIAGNOSTIC_PARAMETER> listDiagParam, ListDiagnosticSource listDiagSrc)
                    : base(container, COUNT_LAYOUT_COLUMN, COUNT_LAYOUT_ROW)
                {
                    container.Add(this);

                    initialize(listTEC, listDiagParam, listDiagSrc);
                }
                /// <summary>
                /// Инициализация (создание/размещение) дочерних компонентов
                /// </summary>
                private void initialize(List<TEC>listTEC, List<DIAGNOSTIC_PARAMETER> listDiagParam, List<DIAGNOSTIC_SOURCE> listDiagSrc)
                {
                    int indxNewRow = -1
                        , iTec = -1;
                    TECComponent gtp;

                    //Tag = listTEC;
                    // идентификатор ТЭЦ (??? еще один, надо было использовать в [techsite_cfg-2.X.X]...[DIAGNOSTIC_SOURCES] из [techsite_cfg-2.X.X]...[TEC_LIST])
                    //  (можно взять любой элемент списка, например [0])
                    Tag = listDiagSrc[0].m_id;

                    initializeLayoutStyle();

                    InitializeComponent();

                    //if (listTEC.Count > 1) {
                    //    m_dgvValues.Columns.Add("TEC", "ТЭЦ");
                    //    m_dgvValues.Columns["TEC"].DisplayIndex = 0;
                    //} else
                    //    ;

                    // добавить строки, указать их наименования
                    foreach (DIAGNOSTIC_SOURCE src in listDiagSrc)
                        if (src.m_id_component > 0) {
                            indxNewRow = m_dgvValues.Rows.Add(new DataGridViewDiagnosticGTPRow());

                            (m_dgvValues.Rows[indxNewRow] as DataGridViewDiagnosticGTPRow).Tag = src.m_id_component;

                            iTec = 0; gtp = null;
                            while ((gtp == null)
                                && (iTec < listTEC.Count)) {
                                gtp = listTEC[iTec].list_TECComponents.Find(comp => { return comp.m_id == src.m_id_component; });

                                iTec++;
                            }

                            (m_dgvValues.Rows[indxNewRow] as DataGridViewDiagnosticGTPRow).Name =
                                //src.m_name_shr
                                string.Format(@"{0}-{1}", gtp.tec.name_shr, gtp.name_shr);
                                ;

                            (m_dgvValues.Rows[indxNewRow] as DataGridViewDiagnosticGTPRow).Enabled = false;
                        } else
                            ;
                    //// идентификатор ТЭЦ (??? еще один, надо было использовать в [techsite_cfg-2.X.X]...[DIAGNOSTIC_SOURCES] из [techsite_cfg-2.X.X]...[TEC_LIST])
                    ////  (можно взять любой элемент списка, например [0])
                    //m_dgvValues.Tag = listDiagSrc[0].m_id;
                    // заголовок 'DataGridView' (можно взять любой элемент списка, например [0])
                    setTextDescription(listDiagSrc[0].m_name_shr);
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
                public Label m_labelDescription = new Label();

                private enum INDEX_CELL : short { NAME_GTP = 0, DATETIME_VALUE, VALUE, DATETIME_VERIFICATION, STATE
                    , COUNT
                }

                private void InitializeComponent()
                {
                    this.Controls.Add(m_labelDescription, 0, 0);
                    this.Controls.Add(m_dgvValues, 0, 1); this.SetRowSpan(m_dgvValues, 6);

                    this.SuspendLayout();
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
                    this.m_dgvValues.ColumnCount = (int)INDEX_CELL.COUNT;
                    this.m_dgvValues.Columns[(int)INDEX_CELL.NAME_GTP].Name = "Источник данных"; this.m_dgvValues.Columns[(int)INDEX_CELL.NAME_GTP].Width = 22;
                    this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VALUE].Name = "Крайнее время"; this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VALUE].Width = 17;
                    this.m_dgvValues.Columns[(int)INDEX_CELL.VALUE].Name = "Крайнее значение"; this.m_dgvValues.Columns[(int)INDEX_CELL.VALUE].Width = 23;
                    this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VERIFICATION].Name = "Время проверки"; this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VERIFICATION].Width = 18;
                    this.m_dgvValues.Columns[(int)INDEX_CELL.STATE].Name = "Связь"; this.m_dgvValues.Columns[(int)INDEX_CELL.STATE].Width = 25;

                    this.m_dgvValues.CellValueChanged += new DataGridViewCellEventHandler(dgv_CellCancel);
                    this.m_dgvValues.CellClick += new DataGridViewCellEventHandler(dgv_CellCancel);
                    //
                    //LabelModes
                    //
                    this.m_labelDescription.AutoSize = true;
                    this.m_labelDescription.Dock = System.Windows.Forms.DockStyle.Fill;
                    this.m_labelDescription.Name = "LabelModes";
                    this.m_labelDescription.TabIndex = 1;
                    this.m_labelDescription.Text = " ";
                    this.m_labelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

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
                /// Структура для описания идентификатора строк в 'm_dgvValues'
                /// </summary>
                private class DataGridViewDiagnosticGTPRow : DataGridViewDiagnosticRow
                {
                    /// <summary>
                    /// Наименование источника - постоянная величина, устанавливается при создании строки
                    /// </summary>
                    public string Name {
                        get {
                            return (string)Cells[(int)INDEX_CELL.NAME_GTP].Value;
                        }

                        set {
                            if (!(Index < 0))
                                Cells[(int)INDEX_CELL.NAME_GTP].Value = value;
                            else
                                ;
                        }
                    }

                    public bool Enabled { get; set; }

                    public void SetValueCells(object[] values)
                    {
                        object value;
                        INDEX_CELL_STATE indxState = INDEX_CELL_STATE.ERROR;
                        Color clrCell = Color.Empty;
                        bool enableValuePrevious = Enabled
                            , enableValueCurrrent = false;

                        foreach (INDEX_CELL i in Enum.GetValues(typeof(INDEX_CELL))) {
                            try {
                                if (((int)i < values.Length)
                                    && (!(values[(int)i] == null))
                                    //&& (string.IsNullOrEmpty((string)values[(int)i]) == false)
                                    ) {
                                    indxState = INDEX_CELL_STATE.ERROR;
                                    clrCell = Color.Empty;

                                    switch (i) {
                                        case INDEX_CELL.NAME_GTP:
                                        case INDEX_CELL.COUNT:
                                            continue;
                                            break;
                                        case INDEX_CELL.STATE:
                                            indxState = ((string)values[(int)i])?.Equals(1.ToString()) == true ?
                                                INDEX_CELL_STATE.OK :
                                                    INDEX_CELL_STATE.ERROR;
                                            value = s_CellState[(int)indxState].m_Text;
                                            clrCell = s_CellState[(int)indxState].m_Color;
                                            break;
                                        case INDEX_CELL.DATETIME_VALUE:
                                        case INDEX_CELL.DATETIME_VERIFICATION:
                                            value = values[(int)i] is DateTime ?
                                                formatDateTime((DateTime)values[(int)i]) :
                                                    values[(int)i];
                                            clrCell = s_CellState[(int)isRelevanceDateTime(i, (DateTime)values[(int)i])].m_Color;
                                            break;
                                        case INDEX_CELL.VALUE:
                                            value = values[(int)i];
                                            break;
                                        default:
                                            indxState = ((string)values[(int)i])?.Equals(EtalonPBR) == true ?
                                                INDEX_CELL_STATE.OK :
                                                    INDEX_CELL_STATE.ERROR;
                                            value = values[(int)i];
                                            break;
                                    }

                                    Cells[(int)i].Value = value;
                                    // изменить цвет ячейки
                                    Cells[(int)i].Style.BackColor = clrCell;
                                } else
                                    ; // значение == null
                            } catch (Exception e) {
                                Logging.Logg().Exception(e, @"PanelContainerModes.PanelModes.DataGridViewGTPRow::SetValueCells () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                            }
                        } // INDEX_CELL i in Enum.GetValues(typeof(INDEX_CELL))
                    }
                    /// <summary>
                    /// Признак актуальности даты/времени
                    /// </summary>
                    /// <param name="indxCell">Номер(индекс) столбца</param>
                    /// <param name="dtChecked">Значение даты/времени для проверки</param>
                    /// <returns>Признак актуальности</returns>
                    private INDEX_CELL_STATE isRelevanceDateTime(INDEX_CELL indxCell, DateTime dtChecked)
                    {
                        INDEX_CELL_STATE stateRes = INDEX_CELL_STATE.ERROR;

                        TimeSpan tsDifference = SERVER_TIME - dtChecked;

                        if (tsDifference.TotalSeconds > 0)
                            switch (indxCell)
                            {
                                case INDEX_CELL.DATETIME_VALUE:
                                    stateRes = (tsDifference.TotalMinutes > 76) ?
                                        (tsDifference.TotalMinutes > 121) ?
                                            INDEX_CELL_STATE.ERROR :
                                                INDEX_CELL_STATE.WARNING :
                                                    INDEX_CELL_STATE.OK;
                                    break;
                                case INDEX_CELL.DATETIME_VERIFICATION:
                                    stateRes = (tsDifference.TotalMinutes > 3) ?
                                        (tsDifference.TotalMinutes > 9) ?
                                            INDEX_CELL_STATE.ERROR :
                                                INDEX_CELL_STATE.WARNING :
                                                    INDEX_CELL_STATE.OK;
                                    break;
                                default:
                                    break;
                            }
                        else
                            // оставить 'ERROR'
                            ;

                        return stateRes;
                    }
                    /// <summary>
                    /// Функция для нахождения ПБР на текущее время
                    /// </summary>
                    /// <returns>возвращает ПБР на текущее время</returns>
                    private string EtalonPBR
                    {
                        get {
                            string strRes = string.Empty;
                            int iNowMinute =
                                    TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").Minute
                                , iNowHour =
                                    TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").Hour;

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
                            strRes = string.Format("ПБР{0}", iNowHour);
                            //else
                            //    m_etalon_pbr = "ПБР" + Convert.ToInt32(m_DTHour);

                            return strRes;
                            //}
                        }
                    }
                }

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
                /// Обновить значения в представлении
                /// </summary>
                /// <param name="values">Значения для отображения</param>
                private void update(Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values> values)
                {
                    object value;
                    object[] rowValues;

                    foreach (DataGridViewDiagnosticGTPRow r in m_dgvValues.Rows) {
                        rowValues = new object[(int)INDEX_CELL.COUNT];

                        foreach (KeyValuePair<KEY_DIAGNOSTIC_PARAMETER, Values> pair in values) {
                            try {
                                // сопоставление ???
                                if (((int)r.Tag == pair.Key.m_id_value)
                                    && (!(pair.Value.m_value == null))
                                    && (string.IsNullOrEmpty((string)pair.Value.m_value) == false)) {
                                    switch (pair.Key.m_id_unit) {
                                        case KEY_DIAGNOSTIC_PARAMETER.ID_UNIT.PBR:
                                            rowValues[(int)INDEX_CELL.VALUE] =
                                                Convert.ChangeType(pair.Value.m_value, KEY_DIAGNOSTIC_PARAMETER.TypeOf[pair.Key.m_id_unit]);
                                            rowValues[(int)INDEX_CELL.DATETIME_VERIFICATION] =
                                                Convert.ChangeType(pair.Value.m_dtValue, typeof(DateTime));
                                            rowValues[(int)INDEX_CELL.STATE] =
                                                Convert.ChangeType(pair.Value.m_strLink, typeof(string));
                                            break;
                                        case KEY_DIAGNOSTIC_PARAMETER.ID_UNIT.DATETIME:
                                            rowValues[(int)INDEX_CELL.DATETIME_VALUE] =
                                                Convert.ChangeType(pair.Value.m_value, KEY_DIAGNOSTIC_PARAMETER.TypeOf[pair.Key.m_id_unit]);
                                            break;
                                        default:                                            
                                            break;
                                    }
                                } else
                                    ;
                            } catch (Exception e) {
                                Logging.Logg().Exception(e, @"PanelContainerModes.PanelModes::update () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                            }
                        } // KeyValuePair<KEY_DIAGNOSTIC_PARAMETER, Values> pair in values

                        r.SetValueCells(rowValues);
                    } // DataGridViewGTPRow r in m_dgvValues.Rows
                }

                public void Update(Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values> values)
                {
                    if (InvokeRequired == true)
                        Invoke(new Action<Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values>>(update), values);
                    else
                        update(values);
                }

                /// <summary>
                /// Функция изменения заголовков грида Modes
                /// </summary>
                private void setTextDescription(string name_shr)
                {
                    if (m_labelDescription.InvokeRequired)
                        m_labelDescription.Invoke(new Action(() => m_labelDescription.Text = name_shr));
                    else
                        m_labelDescription.Text = name_shr;
                }

                /// <summary>
                /// снятие выделения ячейки
                /// </summary>
                /// <param name="sender">параметр</param>
                /// <param name="e">событие</param>
                private void dgv_CellCancel(object sender, EventArgs e)
                {
                    try {
                        if (m_dgvValues.SelectedCells.Count > 0)
                            m_dgvValues.SelectedCells[0].Selected = false;
                        else;
                    } catch {
                    }
                }
            }
        }
    }
}
