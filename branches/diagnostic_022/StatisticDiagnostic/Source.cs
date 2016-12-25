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
using System.Threading;

namespace StatisticDiagnostic
{
    partial class PanelStatisticDiagnostic
    {
        private interface IPanelContainerTec
        {
        }
        /// <summary>
        /// Панель для размещения подпанелей с отображением диагностических параметров/значений источников ланных для ТЭЦ
        /// </summary>
        private class PanelContainerTec : HPanelCommon, IPanelContainerTec
        {
            /// <summary>
            /// Количество столбцов, строк в сетке макета
            /// </summary>
            private const int COUNT_LAYOUT_COLUMN = 3
                , COUNT_LAYOUT_ROW = 2;
            /// <summary>
            /// Структура для хранения получаемых значений из таблицы-результата запроса
            /// </summary>
            private struct Values
            {
                /// <summary>
                /// Метка времени значения
                /// </summary>
                public DateTime m_dtValue;
                /// <summary>
                /// Значение одного из дианостических параметров
                /// </summary>
                public object m_value;

                public string m_strLink;

                public string m_name_shr;
            }

            private struct KEY_DIAGNOSTIC_PARAMETER
            {
                /// <summary>
                /// Перечисление - типов значений
                /// </summary>
                public enum ID_UNIT : short { UNKNOWN = -1, FLOAT = 12, DATETIME = 13 }
                /// <summary>
                /// Словарь с информацией о CLR-типах значений 
                /// </summary>
                public static Dictionary<ID_UNIT, Type> TypeOf = new Dictionary<ID_UNIT, Type> () {
                    { ID_UNIT.FLOAT, typeof(float) }
                    , { ID_UNIT.DATETIME, typeof(DateTime) }
                };
                /// <summary>
                /// Идентификаторы значений из БД конфигурации, таблица [DIAGNOSTIC_PARAM].[ID]
                /// </summary>
                public enum ID_VALUE : short { UNKNOWN = -1
                    , AIISKUE_VALUE = 1, AIISKUE_DATETIME = 4
                    , SOTIASSO_1_VALUE = 2, SOTIASSO_2_VALUE, SOTIASSO_1_DATETIME = 5, SOTIASSO_2_DATETIME
                    , SOTIASSO_1_TORIS_VALUE = 11, SOTIASSO_1_TORIS_DATETIME, SOTIASSO_2_TORIS_VALUE, SOTIASSO_2_TORIS_DATETIME
                    , MODES_CENTRE_VALUE = 7, MODES_CENTRE_DATETIME
                    , MODES_TERMINAL_VALUE = 9, MODES_TERMINAL_DATETIME
                    , SIZE_DB = 27, SIZE_DB_LOG = 26
                    , AVG_TIME_TASK = 28
                }

                public ID_UNIT m_id_unit;

                public ID_VALUE m_id_value;
            }
            /// <summary>
            /// Словарь для хранения коллекции значений диагностических параметров
            ///  ключ - идентификатор ТЭЦ
            /// </summary>
            private class DictionaryTecValues : Dictionary<int, Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values>>
            {
                /// <summary>
                /// Перечисление - типы значений (результптов запроса диагностических данных)
                /// </summary>
                public enum TYPE { UNKNOWN = -1, ACTIVE_SOURCE_SOTIASSO, DATA }
                /// <summary>
                /// Тип результата запроса
                /// </summary>
                public TYPE Type { get; set; }
                /// <summary>
                /// Конструктор - основной (с парметром)
                /// </summary>
                /// <param name="tableRecieved">Таблица - результат запроса значенйи</param>
                public DictionaryTecValues(DataTable tableRecieved)
                {
                    TYPE type = TYPE.UNKNOWN;
                    int id = -1;

                    // определить тип результата запроса
                    if ((tableRecieved.Columns.Count == 2)
                        && (tableRecieved.Columns.Contains(@"ID_TEC") == true)
                        && (tableRecieved.Columns.Contains(@"ID") == true))
                        type = TYPE.ACTIVE_SOURCE_SOTIASSO;
                    else
                        if (tableRecieved.Columns.Count == 8)
                            type = TYPE.DATA;
                        else
                            ;

                    try {
                    // рез-т м.б. 2-х типов: 1) источники данных
                        if (!(type == TYPE.UNKNOWN))
                            foreach (DataRow r in tableRecieved.Rows)
                                switch (type) {
                                    case TYPE.ACTIVE_SOURCE_SOTIASSO:
                                        id = r.Field<int>(@"ID_TEC");

                                        if (this.Keys.Contains(id) == false)
                                            Add(id, new Dictionary<KEY_DIAGNOSTIC_PARAMETER, PanelContainerTec.Values>());
                                        else
                                            ;

                                        this[id].Add(
                                            new KEY_DIAGNOSTIC_PARAMETER() {
                                                m_id_unit = KEY_DIAGNOSTIC_PARAMETER.ID_UNIT.UNKNOWN
                                                , m_id_value = KEY_DIAGNOSTIC_PARAMETER.ID_VALUE.UNKNOWN
                                            }
                                            , new PanelContainerTec.Values() {
                                                m_value = r.Field<int>(@"ID")
                                                , m_strLink = string.Empty
                                                , m_name_shr = string.Empty
                                                , m_dtValue = HDateTime.ToMoscowTimeZone()
                                            }
                                        );
                                        break;
                                    case TYPE.DATA:
                                        id = r.Field<int>(@"ID_EXT"); // читать как ИД ТЭЦ

                                        if (id < (int)INDEX_SOURCE.SIZEDB) {
                                            if (this.Keys.Contains(id) == false)
                                                Add(id, new Dictionary<KEY_DIAGNOSTIC_PARAMETER, PanelContainerTec.Values>());
                                            else
                                                ;

                                            this[id].Add(
                                                new KEY_DIAGNOSTIC_PARAMETER() {
                                                    m_id_unit = (KEY_DIAGNOSTIC_PARAMETER.ID_UNIT)r.Field<int>(@"ID_Units")
                                                    , m_id_value = (KEY_DIAGNOSTIC_PARAMETER.ID_VALUE)Convert.ToInt32(r.Field<string>(@"ID_Value"))
                                                }
                                                , new PanelContainerTec.Values() {
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
                                        break;
                                    default:
                                        break;
                                }
                        else
                            Logging.Logg().Error(@"PanelContainerTec.DictionaryTecValues::ctor () - неизвестный тип набора значений...", Logging.INDEX_MESSAGE.NOT_SET);
                    } catch (Exception e) {
                        Logging.Logg().Exception(e, @"PanelContainerTec.DictionaryTecValues::ctor () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    Type = type;
                }
            }            
            /// <summary>
            /// Массив дочерних панелей для каждой из ТЭЦ
            /// </summary>
            private PanelTec[] m_arPanels;
            /// <summary>
            /// Конструктор основной (с параметрами)
            /// </summary>
            /// <param name="listTEC">Список ТЭЦ</param>
            /// <param name="listDiagnosticParameter">Список диагностических параметров</param>
            public PanelContainerTec(List<TEC> listTEC
                , List<DIAGNOSTIC_PARAMETER> listDiagnosticParameter
                , EventSourceIdChangedHandler fSourceIdChangedHandler)
                    : base(COUNT_LAYOUT_COLUMN, COUNT_LAYOUT_ROW)
            {
                initialize(listTEC, listDiagnosticParameter, fSourceIdChangedHandler);
            }
            /// <summary>
            /// Конструктор дополнительный (с парметрами)
            /// </summary>
            /// <param name="container">Родительскимй объект</param>
            /// <param name="listTEC">Список ТЭЦ</param>
            /// <param name="listDiagnosticParameter">Список диагностических параметров</param>
            public PanelContainerTec(IContainer container
                , List<TEC> listTEC
                , List<DIAGNOSTIC_PARAMETER> listDiagnosticParameter
                , EventSourceIdChangedHandler fSourceIdChangedHandler)
                    : base(container, COUNT_LAYOUT_COLUMN, COUNT_LAYOUT_ROW)
            {
                initialize(listTEC, listDiagnosticParameter, fSourceIdChangedHandler);
            }
            /// <summary>
            /// Инициализация, размещение дочерних элементов управления
            ///  - Стандартная функция для сложного элемента интерфейса
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
            /// Инициализация
            /// </summary>
            /// <param name="listTEC">Список ТЭЦ</param>
            /// <param name="listDiagPar">Список параметров диагностики</param>
            private void initialize(List<TEC> listTEC
                , List<DIAGNOSTIC_PARAMETER> listDiagPar
                , EventSourceIdChangedHandler fSourceIdChangedHandler
                /*, List<DIAGNOSTIC_SOURCE> listDiagSrc*/
                )
            {
                //foreach (TimeZoneInfo tzi in TimeZoneInfo.GetSystemTimeZones())
                //    System.Diagnostics.Debug.WriteLine(string.Format(@"{0} --- {1}", tzi.Id, tzi.BaseUtcOffset.ToString()));

                IEnumerable<DIAGNOSTIC_PARAMETER> par = listDiagPar
                    .GroupBy(item => item.m_name_shr)
                    .Select(group => group.First());

                InitComponents();

                m_arPanels = new PanelTec[listTEC.Count];

                // добавляем строки в таблицы для каждой ТЭЦ для каждого источника
                for (int i = 0; i < listTEC.Count; i++) {
                    m_arPanels[i] = new PanelTec(listTEC[i], par.ToList());

                    this.Controls.Add(m_arPanels[i], (i + 0) % COUNT_LAYOUT_COLUMN, (i + 0) / COUNT_LAYOUT_COLUMN);
                    //this.SetColumnSpan(m_arPanels[i], 1);
                    //this.SetRowSpan(m_arPanels[i], 1);

                    m_arPanels[i].EventSourceIdChanged += new EventSourceIdChangedHandler(panelTec_onSourceIdChanged);
                }

                delegateSourceIdChanged = fSourceIdChangedHandler;

                m_semUpdateHandler = new Semaphore(1, 1);
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

            /// <summary>
            /// Объект синхронизации при обработке события обновления дочерних панелей
            /// </summary>
            private Semaphore m_semUpdateHandler;

            private void panelTec_onSourceIdChanged(object obj, EventSourceIdChangedArgs ev)
            {
                delegateSourceIdChanged?.Invoke(this, ev);
            }

            /// <summary>
            /// Обработчик события панели-родителя - обновление значений
            /// </summary>
            /// <param name="rec">Таблица с результатами запрос</param>
            public void Update(object rec)
            {
                DictionaryTecValues dictTecValues = null;

                m_semUpdateHandler.WaitOne();

                dictTecValues = new DictionaryTecValues((rec as DataTable));

                foreach (KeyValuePair<int, Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values>> pair in dictTecValues)
                    m_arPanels.ToList().Find(panel => (panel.Tag as TEC).m_id == pair.Key)
                        ?.Update(dictTecValues.Type, pair.Value);

                m_semUpdateHandler.Release(1);
            }

            public class EventSourceIdChangedArgs : EventArgs
            {
                public TEC m_tec;

                public CONN_SETT_TYPE m_connSettType;

                public int m_iNewSourceId;

                public EventSourceIdChangedArgs(object obj, CONN_SETT_TYPE connSettType, int iNewSourceId)
                {
                    m_tec = (TEC)(obj as PanelTec).Tag;

                    m_connSettType = connSettType;

                    m_iNewSourceId = iNewSourceId;
                }
            }

            public delegate void EventSourceIdChangedHandler(object obj, EventSourceIdChangedArgs ev);

            public EventSourceIdChangedHandler delegateSourceIdChanged;

            /// <summary>
            /// Класс для описания элемента панели с информацией
            /// по дианостированию работоспособности 
            /// источников фактических, телеметрических значений (АИИС КУЭ, СОТИАССО) 
            /// </summary>
            private partial class PanelTec : HPanelCommon, IPanelTec
            {
                /// <summary>
                /// Количество столбцов, строк в сетке макета
                /// </summary>
                private const int COUNT_LAYOUT_COLUMN = 1
                    , COUNT_LAYOUT_ROW = 12;

                /// <summary>
                /// Перечисление - возможные состояния п.п. контексного меню для строк с альтернативными источниками данных
                /// </summary>
                private enum INDEX_CONTEXTMENU_ITEM : short { ACTIVATED = 0, DEACTIVATED }

                /// <summary>
                /// Ключ для словаря с часовыми поясами для источников данных
                /// </summary>
                private struct KEY_TIMEZONE_SOURCE
                {
                    public CONN_SETT_TYPE m_ConnSettType;

                    public TEC.TEC_TYPE m_tecType;
                }

                /// <summary>
                /// Словарь с параметрами часового пояса известных источников данных
                /// </summary>
                private static Dictionary<KEY_TIMEZONE_SOURCE, TimeZoneInfo> s_dictTimeZoneSource = new Dictionary<KEY_TIMEZONE_SOURCE, TimeZoneInfo>() {
                    // ТЭЦ Новосибирского района
                    { new KEY_TIMEZONE_SOURCE () { m_ConnSettType = CONN_SETT_TYPE.DATA_AISKUE, m_tecType = TEC.TEC_TYPE.COMMON }, TimeZoneInfo.FindSystemTimeZoneById(@"Russian Standard Time") } 
                    , { new KEY_TIMEZONE_SOURCE () { m_ConnSettType = CONN_SETT_TYPE.DATA_SOTIASSO, m_tecType = TEC.TEC_TYPE.COMMON }, TimeZoneInfo.FindSystemTimeZoneById(@"Central Asia Standard Time") }
                    , { new KEY_TIMEZONE_SOURCE () { m_ConnSettType = CONN_SETT_TYPE.DATA_VZLET, m_tecType = TEC.TEC_TYPE.COMMON }, TimeZoneInfo.FindSystemTimeZoneById(@"SE Asia Standard Time") }
                    // Бийская ТЭЦ
                    , { new KEY_TIMEZONE_SOURCE () { m_ConnSettType = CONN_SETT_TYPE.DATA_AISKUE, m_tecType = TEC.TEC_TYPE.BIYSK }, TimeZoneInfo.FindSystemTimeZoneById(@"Russian Standard Time") }
                    , { new KEY_TIMEZONE_SOURCE () { m_ConnSettType = CONN_SETT_TYPE.DATA_SOTIASSO, m_tecType = TEC.TEC_TYPE.BIYSK }, TimeZoneInfo.FindSystemTimeZoneById(@"Central Asia Standard Time") }
                    , { new KEY_TIMEZONE_SOURCE () { m_ConnSettType = CONN_SETT_TYPE.DATA_VZLET, m_tecType = TEC.TEC_TYPE.BIYSK }, TimeZoneInfo.Local }
                };

                /// <summary>
                /// Объект - элемент интерфейса - табличное представление отображаемых данных
                /// </summary>
                private DataGridView m_dgvValues;

                /// <summary>
                /// Элемент интерфейса - подпись - наименование ТЭЦ
                /// </summary>
                private Label m_labelDescription;

                /// <summary>
                /// Элемент интерфейса - контекстное меню для строк в 'm_dgvValues'
                ///  , являющимися опосанием альтернативных источников по полю 'ID_ROW.m_source_type'
                /// </summary>
                private ContextMenuStrip ContextmenuChangeState;

                /// <summary>
                /// Структура для описания идентификатора строк в 'm_dgvValues'
                /// </summary>
                private class DataGridViewSourceRow : DataGridViewRow
                {
                    /// <summary>
                    /// Список номер источников СОТИАССО
                    /// </summary>
                    private enum TM { TM1 = 2, TM2, TM1T, TM2T };
                    /// <summary>
                    /// Перечисление - индексы для доступа к признакам по управлению отображением строки
                    /// </summary>
                    private enum INDEX_RULE : byte { ACTIVATED }
                    /// <summary>
                    /// См. описание типа поля
                    /// </summary>
                    private CONN_SETT_TYPE m_source_type;                    
                    /// <summary>
                    /// Целочисленный идентификатор источника данных
                    /// </summary>
                    private int m_source_id;
                    /// <summary>
                    /// Объект с признаками для управления отображением строки
                    /// </summary>
                    private HMark _markRule;
                    /// <summary>
                    /// Конструктор - основной (с параметрами)
                    /// </summary>
                    /// <param name="source_data">Обязательная часть описания источника данных</param>
                    public DataGridViewSourceRow()
                    {
                        m_source_type = CONN_SETT_TYPE.UNKNOWN;
                        m_source_id = -1;
                        Tag = string.Empty;

                        _markRule = new HMark(0);
                    }
                    /// <summary>
                    /// Обязательный постфикс в наименовании источника данных
                    ///  , по нему идентифицируем строку, так же отображающую значения для источника данных
                    /// </summary>
                    public string SourceData {
                        get {
                            return (string)Tag;
                        }

                        set {
                            // для определения идентфикатора ТЭЦ
                            TEC tec = _tec;

                            switch (value)
                            {
                                case @"(факт.)":
                                    m_source_type = CONN_SETT_TYPE.DATA_AISKUE;
                                    m_source_id = tec.m_id * 10 + 1; //??? используется не константа
                                    break;
                                case @"(ТМ-1)":
                                    m_source_type = CONN_SETT_TYPE.DATA_SOTIASSO;
                                    m_source_id = tec.m_id * 10 + (int)TM.TM1;
                                    break;
                                case @"(ТМ-2)":
                                    m_source_type = CONN_SETT_TYPE.DATA_SOTIASSO;
                                    m_source_id = tec.m_id * 10 + (int)TM.TM2;
                                    break;
                                case @"(ТМ-1Т)":
                                    m_source_type = CONN_SETT_TYPE.DATA_SOTIASSO;
                                    m_source_id = tec.m_id * 10 + (int)TM.TM1T;
                                    break;
                                case @"(ТМ-2Т)":
                                    m_source_type = CONN_SETT_TYPE.DATA_SOTIASSO;
                                    m_source_id = tec.m_id * 10 + (int)TM.TM2T;
                                    break;
                                default:
                                    m_source_type = CONN_SETT_TYPE.UNKNOWN;
                                    m_source_id = -1;
                                    break;
                            }

                            Tag = value;
                        }
                    }
                    /// <summary>
                    /// Наименование источника - постоянная величина, устанавливается при создании строки
                    /// </summary>
                    public string Name {
                        get {
                            return (string)Cells[(int)INDEX_CELL.NAME_SOURCE].Value;
                        }

                        set {
                            if (!(Index < 0))
                                Cells[(int)INDEX_CELL.NAME_SOURCE].Value = value;
                            else
                                ;
                        }
                    }
                    /// <summary>
                    /// Идентификатор родительского элемента (для формирования идентификаторов источников данных)
                    /// </summary>
                    private TEC _tec {
                        get {
                            TEC tecRes = null;
                            // родительский элемент
                            Control parent = this.DataGridView.Parent;
                            // искать среди родителей до тех пор, пока не найден объект с интерфейсом 'IPanelTec'
                            while ((!(parent is IPanelTec))
                                && (!(parent == null)))
                                parent = parent.Parent;

                            if ((parent is IPanelTec)
                                && (!(parent == null)))
                                tecRes = (parent as IPanelTec).m_tec;
                            else
                                ;

                            return tecRes;
                        }
                    }

                    public enum ENABLE_VALUE : short { UNKNOWN = -1, FALSE, TRUE, OUTDATE }

                    private ENABLE_VALUE _enabled;
                    /// <summary>
                    /// Признак возможности использования источника данных
                    /// </summary>
                    public ENABLE_VALUE Enabled
                    {
                        get { return _enabled; }

                        set {
                            // выбрать цвет для ячейки
                            Color clrCell = value == ENABLE_VALUE.TRUE ? Color.Empty : Color.LightGray;
                            // установить признак включения(доступности)
                            _enabled = value;

                            foreach (INDEX_CELL i in Enum.GetValues(typeof(INDEX_CELL))) {
                                switch (i) {
                                    case INDEX_CELL.COUNT:
                                        continue;
                                        break;
                                    case INDEX_CELL.NAME_SOURCE:
                                        if (Activated == true)
                                            continue;
                                        else
                                            ;
                                        break;
                                    default:
                                        break;
                                }
                                // установить цвет ячейки
                                Cells[(int)i].Style.BackColor = clrCell;
                                // при необходимости, удалить контекстное меню
                                if ((!(value == ENABLE_VALUE.TRUE))
                                    && (!(ContextMenuStrip == null)))
                                    ContextMenuStrip = null;
                                else
                                    ;
                            }
                        }
                    }
                    /// <summary>
                    /// Признак активности источника данных (используется/не_используется)
                    /// </summary>
                    public bool Activated
                    {
                        get { return _markRule.IsMarked((int)INDEX_RULE.ACTIVATED); }

                        set {
                            _markRule.Set((int)INDEX_RULE.ACTIVATED, value);

                            Cells[(int)INDEX_CELL.NAME_SOURCE].Style.BackColor = s_ColorSOTIASSOState[
                                value == true ? (int)INDEX_CONTEXTMENU_ITEM.ACTIVATED :
                                    value == false ? (int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED :
                                        (int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED
                            ];
                        }
                    }
                    /// <summary>
                    /// Тип источника данных
                    /// </summary>
                    public CONN_SETT_TYPE SourceType { get { return m_source_type; } }
                    /// <summary>
                    /// Идентификатор источника данных
                    /// </summary>
                    public int SourceID { get { return m_source_id; } }

                    private bool isSourceSOTIASSOToris {
                        get {
                            return (SourceID % 10 == (int)TM.TM1T)
                                || (SourceID % 10 == (int)TM.TM2T);
                        }
                    }

                    public void SetValueCells(object[]values)
                    {
                        object value;
                        INDEX_STATE indxState = INDEX_STATE.ERROR;
                        Color clrCell = Color.Empty;
                        ENABLE_VALUE enableValuePrevious = Enabled
                            , enableValueCurrrent = ENABLE_VALUE.UNKNOWN;

                        Func <ENABLE_VALUE> isEnabled = new Func<ENABLE_VALUE> (() => {
                            ENABLE_VALUE enableValueRes = ENABLE_VALUE.TRUE;

                            foreach (INDEX_CELL i in Enum.GetValues(typeof(INDEX_CELL))) {
                                switch (i) {
                                    case INDEX_CELL.NAME_SOURCE:
                                    case INDEX_CELL.COUNT:
                                        continue;
                                        break;
                                    case INDEX_CELL.DATETIME_VERIFICATION:
                                        enableValueRes = ((!(values[(int)i] == null))
                                            && ((values[(int)i] is DateTime) == true)) ?
                                                (isRelevanceDateTime(i, (DateTime)values[(int)i]) == true) ?
                                                    ENABLE_VALUE.TRUE :
                                                        ENABLE_VALUE.OUTDATE :
                                                            ENABLE_VALUE.FALSE;
                                        break;
                                    default:
                                        enableValueRes = (!(values[(int)i] == null) == true) ?
                                            ENABLE_VALUE.TRUE :
                                                ENABLE_VALUE.FALSE;
                                        break;
                                }

                                if (!(enableValueRes == ENABLE_VALUE.TRUE))
                                    break;
                                else
                                    ;
                            } // INDEX_CELL i in Enum.GetValues(typeof(INDEX_CELL))

                            return enableValueRes;
                        });

                        enableValueCurrrent = isEnabled();

                        if (!(enableValuePrevious == enableValueCurrrent))
                            Enabled = enableValueCurrrent;
                        else
                            ;

                        if ((Enabled == ENABLE_VALUE.TRUE)
                            || (Enabled == ENABLE_VALUE.OUTDATE))
                            foreach (INDEX_CELL i in Enum.GetValues(typeof(INDEX_CELL))) {
                                indxState = INDEX_STATE.ERROR;
                                clrCell = Color.Empty;

                                switch (i) {
                                    case INDEX_CELL.NAME_SOURCE:
                                    case INDEX_CELL.COUNT:
                                        continue;
                                        break;
                                    case INDEX_CELL.STATE:
                                        indxState = ((string)values[(int)i])?.Equals(1.ToString()) == true ?
                                            INDEX_STATE.OK :
                                                isSourceSOTIASSOToris == false ?
                                                    INDEX_STATE.ERROR :
                                                        INDEX_STATE.UNKNOWN;
                                        value = s_StateSources[(int)indxState].m_Text;
                                        clrCell = s_StateSources[(int)indxState].m_Color;
                                        break;
                                    case INDEX_CELL.DATETIME_VALUE:
                                    case INDEX_CELL.DATETIME_VERIFICATION:
                                        value = values[(int)i] is DateTime ?
                                            formatDateTime (i, (DateTime)values[(int)i]) :
                                                values[(int)i];
                                        break;
                                    default:
                                        value = values[(int)i];
                                        break;
                                }

                                Cells[(int)i].Value = value;
                                // изменить цвет ячейки
                                if (Enabled == ENABLE_VALUE.TRUE)
                                // только если источник включен
                                    Cells[(int)i].Style.BackColor = clrCell;
                                else
                                    ;
                            } // INDEX_CELL i in Enum.GetValues(typeof(INDEX_CELL))
                        else
                            ;
                    }

                    private string formatDateTime(INDEX_CELL indxCell, DateTime dtFormated)
                    {
                        string strRes = string.Empty;

                        KEY_TIMEZONE_SOURCE keyTimeZoneSource;
                        TimeZoneInfo timeZoneInfo;

                        if (indxCell == INDEX_CELL.DATETIME_VALUE) {
                            keyTimeZoneSource = new KEY_TIMEZONE_SOURCE() { m_ConnSettType = SourceType, m_tecType = _tec.Type };
                            timeZoneInfo = s_dictTimeZoneSource[keyTimeZoneSource];

                            dtFormated = TimeZoneInfo.ConvertTime(dtFormated, timeZoneInfo);
                        } else
                            ;

                        if (SERVER_TIME.Date > dtFormated.Date)
                            strRes = dtFormated.ToString(@"dd.MM.yyyy HH:mm:ss");
                        else
                            strRes = dtFormated.ToString(@"HH:mm:ss");

                        return strRes;
                    }

                    private bool isRelevanceDateTime(INDEX_CELL indxCell, DateTime dtChecked)
                    {
                        bool bRes = true;

                        switch (indxCell) {
                            case INDEX_CELL.DATETIME_VERIFICATION:
                                bRes = !((SERVER_TIME - dtChecked).TotalHours > 1);
                                break;
                            default:
                                break;
                        }

                        return bRes;
                    }
                }

                /// <summary>
                /// Конструктор основной (с парметром)
                /// </summary>
                /// <param name="listDiagnosticParameter">Список с параметрами диагностики</param>
                public PanelTec(TEC tag, List<DIAGNOSTIC_PARAMETER> listDiagnosticParameter)
                    : base(COUNT_LAYOUT_COLUMN, COUNT_LAYOUT_ROW)
                {
                    initialize(tag, listDiagnosticParameter);
                }

                /// <summary>
                /// Конструктор основной (с парметрами)
                /// </summary>
                /// <param name="container">Родительский контейнер</param>
                /// <param name="listDiagnosticParameter">Список с параметрами диагностики</param>
                public PanelTec(IContainer container, TEC tag, List<DIAGNOSTIC_PARAMETER> listDiagnosticParameter)
                    : base(container, COUNT_LAYOUT_COLUMN, COUNT_LAYOUT_ROW)
                {
                    container.Add(this);

                    initialize(tag, listDiagnosticParameter);
                }

                /// <summary>
                /// Инициализация полей/свойств
                /// </summary>
                /// <param name="listDiagnosticParameter">Список параметров диагностики</param>
                private void initialize(TEC tag, List<DIAGNOSTIC_PARAMETER> listDiagnosticParameter)
                {
                    /*DataGridViewSourceRow newRow = null;*/
                    int indxNewRow = -1;

                    Tag = tag;

                    // вызвать явно для корректного размещения дочерних элементов управления в макете
                    initializeLayoutStyle();

                    InitializeComponent();

                    setTextDescription();

                    // добавить строки в соответствии со списком диагностических параметров
                    listDiagnosticParameter.ForEach(item => {
                        m_dgvValues.Rows.Add(/*newRow = */new DataGridViewSourceRow());

                        indxNewRow = m_dgvValues.RowCount - 1;
                        // инициализация зависимых параметров от 'item.m_source_data'
                        (m_dgvValues.Rows[indxNewRow] as DataGridViewSourceRow).SourceData = item.m_source_data;
                        // неизменямое наименование источника данных
                        (m_dgvValues.Rows[indxNewRow] as DataGridViewSourceRow).Name = item.m_name_shr;
                        // исходное состояние строки - источник данных отключен
                        (m_dgvValues.Rows[indxNewRow] as DataGridViewSourceRow).Enabled = DataGridViewSourceRow.ENABLE_VALUE.FALSE;
                    });
                }

                /// <summary>
                /// Инициализация характеристик, стилей макета для размещения дочерних элементов интерфейса
                ///  (должна быть вызвана явно)
                /// </summary>
                /// <param name="col">Количество столбцов в макете</param>
                /// <param name="row">Количество строк в макете</param>
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

                #region Код, автоматически созданный конструктором компонентов

                /// <summary>
                /// Обязательный метод для поддержки конструктора - не изменяйте
                /// содержимое данного метода при помощи редактора кода.
                /// </summary>
                private void InitializeComponent()
                {
                    this.m_dgvValues = new DataGridView();
                    this.m_labelDescription = new Label();

                    this.Controls.Add(m_labelDescription, 0, 0); this.SetRowSpan(m_labelDescription, 2);
                    this.Controls.Add(m_dgvValues, 0, 2); this.SetRowSpan(m_dgvValues, 10);
                    
                    this.ContextmenuChangeState = new System.Windows.Forms.ContextMenuStrip();
                    this.ContextmenuChangeState.SuspendLayout();

                    this.SuspendLayout();

                    this.m_dgvValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                    this.m_dgvValues.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; //.AllCells
                    this.m_dgvValues.ClearSelection();
                    this.m_dgvValues.AllowUserToDeleteRows = false;
                    this.m_dgvValues.Dock = System.Windows.Forms.DockStyle.Fill;
                    this.m_dgvValues.RowHeadersVisible = false;
                    this.m_dgvValues.ReadOnly = true;
                    //this.TECDataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.Fill);
                    //this.m_dgvValues.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
                    this.m_dgvValues.AllowUserToAddRows = false;
                    this.m_dgvValues.AllowUserToResizeRows = false;
                    this.m_dgvValues.ScrollBars = ScrollBars.Vertical;
                    this.m_dgvValues.Name = "TECDataGridView";
                    this.m_dgvValues.TabIndex = 0;
                    this.m_dgvValues.ColumnCount = (int)INDEX_CELL.COUNT;
                    this.m_dgvValues.Columns[(int)INDEX_CELL.NAME_SOURCE].Name = "Источник данных"; this.m_dgvValues.Columns[(int)INDEX_CELL.NAME_SOURCE].Width = 43;
                    this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VALUE].Name = "Крайнее время"; this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VALUE].Width = 57;
                    this.m_dgvValues.Columns[(int)INDEX_CELL.VALUE].Name = "Крайнее значение"; this.m_dgvValues.Columns[(int)INDEX_CELL.VALUE].Width = 35;
                    this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VERIFICATION].Name = "Время проверки"; this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VERIFICATION].Width = 57;
                    this.m_dgvValues.Columns[(int)INDEX_CELL.STATE].Name = "Связь"; this.m_dgvValues.Columns[(int)INDEX_CELL.STATE].Width = 35;

                    this.m_dgvValues.CellClick += new DataGridViewCellEventHandler(dgv_OnCellSelectedCancel);
                    this.m_dgvValues.CellValueChanged += new DataGridViewCellEventHandler(dgv_OnCellSelectedCancel);
                    this.m_dgvValues.CellMouseDown += new DataGridViewCellMouseEventHandler(dgv_OnCellMouseDown);
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
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED].Click += new EventHandler(toolStripMenuItem_Click);
                    (this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED] as ToolStripMenuItem).CheckOnClick = true;
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.ACTIVATED].Tag = INDEX_CONTEXTMENU_ITEM.ACTIVATED;
                    // 
                    // toolStripMenuItemDeactivate
                    // 
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED].Name = "toolStripMenuItem2";
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED].Size = new System.Drawing.Size(179, 22);
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED].Text = "Deactivate";
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED].Click += new EventHandler(toolStripMenuItem_Click);
                    (this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED] as ToolStripMenuItem).CheckOnClick = false;
                    this.ContextmenuChangeState.Items[(int)INDEX_CONTEXTMENU_ITEM.DEACTIVATED].Tag = INDEX_CONTEXTMENU_ITEM.DEACTIVATED;
                    //
                    // LabelTec
                    //
                    this.m_labelDescription.AutoSize = true;
                    this.m_labelDescription.Name = "LabelTec";
                    this.m_labelDescription.TabIndex = 1;
                    this.m_labelDescription.Text = "Unknow_TEC";
                    this.m_labelDescription.Dock = System.Windows.Forms.DockStyle.Fill;
                    this.m_labelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                    this.ContextmenuChangeState.ResumeLayout(false);

                    this.ResumeLayout(false);
                }

                /// <summary>
                /// Перечисление - индексы для ячеек в строке
                /// </summary>
                private enum INDEX_CELL : short {
                    NAME_SOURCE = 0, DATETIME_VALUE, VALUE, DATETIME_VERIFICATION, STATE
                    , COUNT
                }

                private int indexRowContextMenu;

                /// <summary>
                /// Обработчик события - нажатие кнопки "мыши"
                /// </summary>
                /// <param name="sender">Объект, инициировавший событие (DataGridView)</param>
                /// <param name="e">Аргумент события</param>
                void dgv_OnCellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
                {
                    if ((e.Button == MouseButtons.Right) && (e.RowIndex > -1))
                    // только по нажатию правой кнопки и выбранной строки
                        if (((m_dgvValues.Rows[e.RowIndex] as DataGridViewSourceRow).SourceType == CONN_SETT_TYPE.DATA_SOTIASSO)
                            && ((m_dgvValues.Rows[e.RowIndex] as DataGridViewSourceRow).Enabled == DataGridViewSourceRow.ENABLE_VALUE.TRUE)
                            && (AlternativeSourceIdSOTIASSO.Count > 1)) {
                        //?? только для источников СОТИАССО И включенных И наличия альтернативы
                            initContextMenu((m_dgvValues.Rows[e.RowIndex] as DataGridViewSourceRow).Activated == true);
                            ContextmenuChangeState.Tag = e.RowIndex;
                            if ((sender as DataGridView).Rows[e.RowIndex].Cells[e.ColumnIndex].ContextMenuStrip == null) {                                
                                (sender as DataGridView).Rows[e.RowIndex].Cells[e.ColumnIndex].ContextMenuStrip = ContextmenuChangeState;
                            } else
                                ;
                        } else
                            ; // строки не являются описанием источников СОТИАССО
                    else
                        ; // нажата не правая кнопка ИЛИ не выбрана строка
                }

                #endregion
            }

            private interface IPanelTec
            {
                StatisticCommon.TEC m_tec { get; }
            }

            /// <summary>
            /// Класс для описания элемента панели с информацией
            /// по диагностированию работоспособности 
            /// источников фактических, телеметрических значений (АИИС КУЭ, СОТИАССО)
            /// </summary>
            partial class PanelTec
            {
                /// <summary>
                /// Реализация свойства наследуемого интерфейса - идентификатор объекта
                /// </summary>
                public StatisticCommon.TEC m_tec { get { return Tag as TEC; } }

                /// <summary>
                /// Массив с цветовой гаммой для обозначения активности источника данных (СОТИАССО)
                /// </summary>
                private static Color[] s_ColorSOTIASSOState = new Color[] { Color.DeepSkyBlue, Color.Empty };

                /// <summary>
                /// Очистить представление от строк (столбцы без изменений)
                /// </summary>
                public void Clear()
                {
                    for (int j = 0; j < m_dgvValues.Rows.Count; j++)
                        if (m_dgvValues.Rows.Count > 0)
                            m_dgvValues.Rows.Clear();
                        else
                            ;
                }

                //private int ActiveSourceIdSOTIASSO {
                //    get {
                //        return -1;
                //    }
                //}

                public event EventSourceIdChangedHandler EventSourceIdChanged;

                /// <summary>
                /// Обработка события клика по пункту меню "Active"
                /// для активации нового источника СОТИАССО
                /// </summary>
                /// <param name="sender"></param>
                /// <param name="e"></param>
                private void toolStripMenuItem_Click(object sender, EventArgs e)
                {
                    int iPrevSourceId = -1, iNewSourceId = -1
                        , indxPrevSourceId = -1, indxNewSourceId = -1;
                    List<int> alternativeSourceIdSOTIASSO = AlternativeSourceIdSOTIASSO;

                    if ((INDEX_CONTEXTMENU_ITEM)(sender as ToolStripMenuItem).Tag == INDEX_CONTEXTMENU_ITEM.ACTIVATED) {
                        iNewSourceId = (m_dgvValues.Rows[(int)ContextmenuChangeState.Tag] as DataGridViewSourceRow).SourceID;
                    } else {
                    // INDEX_CONTEXTMENU_ITEM.DEACTIVATED
                        iPrevSourceId = (m_dgvValues.Rows[(int)ContextmenuChangeState.Tag] as DataGridViewSourceRow).SourceID;
                        indxPrevSourceId = alternativeSourceIdSOTIASSO.IndexOf(iPrevSourceId);
                        // выбираем очередной источник данных - следующий за текущим
                        indxNewSourceId = indxPrevSourceId + 1;

                        if (!(indxNewSourceId < alternativeSourceIdSOTIASSO.Count))
                            indxNewSourceId = 0;
                        else
                            ;

                        iNewSourceId = alternativeSourceIdSOTIASSO[indxNewSourceId];
                    }

                    foreach (DataGridViewSourceRow r in m_dgvValues.Rows)
                        if ((r.Activated == true)
                            && (r.SourceType == CONN_SETT_TYPE.DATA_SOTIASSO)) {
                            r.Activated = false;
                            // прерываемся т.к. активный источник СОТИАССО только один
                            break;
                        } else
                            ;
                    // событие для сохранения нового источника и немедленного опроса состояния источников данных
                    EventSourceIdChanged(
                        null // панель передается через поле аргумента
                        , new EventSourceIdChangedArgs(this
                            , (m_dgvValues.Rows[(int)ContextmenuChangeState.Tag] as DataGridViewSourceRow).SourceType
                            , iNewSourceId)
                    );
                }

                /// <summary>
                /// Метод заполнения данными элементов ТЭЦ
                /// </summary>
                /// <param name="type">Тип результатов запроса</param>
                /// <param name="values">Значения запроса</param>
                public void Update(DictionaryTecValues.TYPE type, Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values> values)
                {
                    int err = -1; // признак ошибки при выполнении метода обновления

                    if (InvokeRequired == true)
                        Invoke(new Action<DictionaryTecValues.TYPE, Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values>>(update), type, values);
                    else
                        update(type, values/*, out err*/);
                }

                /// <summary>
                /// Метод заполнения данными элементов ТЭЦ
                /// </summary>
                /// <param name="type"></param>
                /// <param name="values"></param>
                private void update(DictionaryTecValues.TYPE type, Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values> values/*, out int err*/)
                {
                    //err = 0; // ошибок нет

                    object value;
                    object[] rowValues;

                    switch (type) {
                        case DictionaryTecValues.TYPE.ACTIVE_SOURCE_SOTIASSO:
                            activateSourceSOTIASSO(values);
                            break;
                        default:
                            foreach (DataGridViewSourceRow r in m_dgvValues.Rows) {
                                rowValues = new object[(int)INDEX_CELL.COUNT];

                                foreach (KeyValuePair<KEY_DIAGNOSTIC_PARAMETER, Values> pair in values) {
                                    try {
                                        // сопоставление 'r.source_name' && 'values.name_shr'
                                        if (pair.Value.m_name_shr?.IndexOf(r.SourceData) > 0) {
                                            switch (pair.Key.m_id_value) {
                                                case KEY_DIAGNOSTIC_PARAMETER.ID_VALUE.AIISKUE_VALUE:
                                                case KEY_DIAGNOSTIC_PARAMETER.ID_VALUE.SOTIASSO_1_VALUE:
                                                case KEY_DIAGNOSTIC_PARAMETER.ID_VALUE.SOTIASSO_2_VALUE:
                                                case KEY_DIAGNOSTIC_PARAMETER.ID_VALUE.SOTIASSO_1_TORIS_VALUE:
                                                case KEY_DIAGNOSTIC_PARAMETER.ID_VALUE.SOTIASSO_2_TORIS_VALUE:
                                                    switch (pair.Key.m_id_unit) {
                                                        case KEY_DIAGNOSTIC_PARAMETER.ID_UNIT.FLOAT:
                                                            value = HMath.doubleParse((string)pair.Value.m_value);
                                                            break;
                                                        default:
                                                            value = Convert.ChangeType(pair.Value.m_value, KEY_DIAGNOSTIC_PARAMETER.TypeOf[pair.Key.m_id_unit]);
                                                            break;
                                                    }
                                                    rowValues[(int)INDEX_CELL.VALUE] = value;
                                                    rowValues[(int)INDEX_CELL.DATETIME_VERIFICATION] =
                                                        Convert.ChangeType(pair.Value.m_dtValue, typeof(DateTime));
                                                    rowValues[(int)INDEX_CELL.STATE] =
                                                        Convert.ChangeType(pair.Value.m_strLink, typeof(string));
                                                    break;
                                                case KEY_DIAGNOSTIC_PARAMETER.ID_VALUE.AIISKUE_DATETIME:
                                                case KEY_DIAGNOSTIC_PARAMETER.ID_VALUE.SOTIASSO_1_DATETIME:
                                                case KEY_DIAGNOSTIC_PARAMETER.ID_VALUE.SOTIASSO_2_DATETIME:
                                                case KEY_DIAGNOSTIC_PARAMETER.ID_VALUE.SOTIASSO_1_TORIS_DATETIME:
                                                case KEY_DIAGNOSTIC_PARAMETER.ID_VALUE.SOTIASSO_2_TORIS_DATETIME:
                                                    rowValues[(int)INDEX_CELL.DATETIME_VALUE] =
                                                        Convert.ChangeType(pair.Value.m_value, KEY_DIAGNOSTIC_PARAMETER.TypeOf[pair.Key.m_id_unit]);
                                                    //r.Cells[(int)INDEX_CELL.DATETIME_VALUE].ToolTipText =
                                                    //    Convert.ChangeType(pair.Value.m_dtValue, typeof(DateTime)).ToString();
                                                    break;
                                                default:
                                                    break;
                                            }
                                        } else
                                            ;
                                    } catch (Exception e) {
                                        Logging.Logg().Exception(e, @"PanelContainerTec.PanelTec::update () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                                    }
                                } // KeyValuePair<KEY_DIAGNOSTIC_PARAMETER, Values> pair in values

                                r.SetValueCells(rowValues);
                            } // DataGridViewSourceRow r in m_dgvValues.Rows

                            //cellsPing();
                            break;
                    }
                }

                private List<int> AlternativeSourceIdSOTIASSO {
                    get {
                        List<int> listRes = new List<int>();

                        foreach (DataGridViewSourceRow r in m_dgvValues.Rows)
                            if ((r.SourceType == CONN_SETT_TYPE.DATA_SOTIASSO)
                                && (r.Enabled == DataGridViewSourceRow.ENABLE_VALUE.TRUE)) {
                            // только если: СОТИАССО И включен
                                listRes.Add(r.SourceID);
                            }
                            else
                                ;

                        return listRes;
                    }
                }

                private void activateSourceSOTIASSO(Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values> values)
                {
                    bool bChangeActivated = false;
                    int iSourceId = (int)values[new KEY_DIAGNOSTIC_PARAMETER() {
                        m_id_unit = KEY_DIAGNOSTIC_PARAMETER.ID_UNIT.UNKNOWN
                        , m_id_value = KEY_DIAGNOSTIC_PARAMETER.ID_VALUE.UNKNOWN}].m_value;

                    foreach (DataGridViewSourceRow r in m_dgvValues.Rows)
                        if ((r.SourceType == CONN_SETT_TYPE.DATA_SOTIASSO)
                            && (r.SourceID == iSourceId)
                            && (r.Activated == false)) {
                        // только если: СОТИАССО И совпадает 'SourceID' И не активен
                            // установить признак наличия изменений
                            bChangeActivated =
                            // активация источника
                            r.Activated =
                                true;

                            break;
                        }
                        else
                            ;
                    // деактивация при наличии изменений
                    if (bChangeActivated == true)
                        foreach (DataGridViewSourceRow r in m_dgvValues.Rows) {
                            if ((r.SourceType == CONN_SETT_TYPE.DATA_SOTIASSO)
                                && (!(r.SourceID == iSourceId))
                                && (r.Activated == true)) {
                                // деактивация источника
                                r.Activated = false;
                                // можно прервать цикл т.к. активный источник м.б. только один
                                break;
                            }
                            else
                                ;
                        }
                    else
                        ;
                }

                /// <summary>
                /// Функция для подписи элементов 
                /// внутри элемента панели ТЭЦ
                /// </summary>
                private void setTextDescription()
                {
                    int i = -1;

                    if (m_labelDescription.InvokeRequired)
                        m_labelDescription.Invoke(new Action(() => m_labelDescription.Text = m_tec.name_shr));
                    else
                        m_labelDescription.Text = m_tec.name_shr;
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
                private void dgv_OnCellSelectedCancel(object sender, EventArgs e)
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
            }
        }
    }
}
