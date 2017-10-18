﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

using HClassLibrary;
using System.Threading;
using System.Collections;

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
            /// <summary>
            /// Перечисление - индексы/количество столбцов в представлении для отображения значений
            /// </summary>
            private enum INDEX_CELL : short {
                NAME, VALUE, DATETIME_VERIFICATION, DATETIME_VALUE, ERROR_DESCRIPTION, STATE
                , COUNT
            }
            /// <summary>
            /// Элемент интерфейса пользователя - представление для отображения значений
            ///  , контролируемых параметров
            /// </summary>
            private DataGridViewDiagnostic m_dgvValues;
            /// <summary>
            /// Элементт интерфейса пользователя - описание(наименование) панели
            /// </summary>
            private Label m_labelDescription;
            /// <summary>
            /// Объект синхронизации при обработке события обновления дочерних панелей
            /// </summary>
            private Semaphore m_semUpdateHandler;
            /// <summary>
            /// Конструктор основной (с параметрами)
            /// </summary>
            /// <param name="listDiagnosticSource">Список контролируемых источников данных (БД конфигурации)</param>
            public PanelTask(ListDiagnosticSource listDiagnosticSource)
                : base(COUNT_LAYOUT_COLUMN, COUNT_LAYOUT_ROW)
            {
                initialize(listDiagnosticSource);
            }
            /// <summary>
            /// Конструктор дополнительный (с параметрами)
            /// </summary>
            /// <param name="container">Родительский элемент управления</param>
            /// <param name="listDiagnosticSource">Список контролируемых источников данных (БД конфигурации)</param>
            public PanelTask(IContainer container, ListDiagnosticSource listDiagnosticSource)
                : base(container, COUNT_LAYOUT_COLUMN, COUNT_LAYOUT_ROW)
            {
                container.Add(this);

                initialize(listDiagnosticSource);
            }

            /// <summary>
            /// Инициализация (создание/размещение) дочерних элементов управления
            /// </summary>
            /// <param name="listDiagnosticSource">Список контролируемых источников данных (БД конфигурации)</param>
            private void initialize(ListDiagnosticSource listDiagnosticSource)
            {
                ListDiagnosticSource listDiagSrc;
                int iNewRow = -1;

                initializeLayoutStyle();

                InitializeComponent();

                listDiagSrc = new ListDiagnosticSource(listDiagnosticSource.FindAll(item => {
                    return !(item.m_id < (int)INDEX_SOURCE.TASK);
                }));

                foreach (DIAGNOSTIC_SOURCE src in listDiagSrc) {
                    //if (src.m_id > 0) {
                        iNewRow = m_dgvValues.Rows.Add(new DataGridViewDiagnosticTaskRow());

                        m_dgvValues.Rows[iNewRow].Tag = src.m_id;

                        (m_dgvValues.Rows[iNewRow] as DataGridViewDiagnosticTaskRow).Name = src.m_name_shr;

                        (m_dgvValues.Rows[iNewRow] as DataGridViewDiagnosticTaskRow).EventUp += onEventUp;
                    //} else
                    //    ;
                }

                m_semUpdateHandler = new Semaphore(1, 1);
            }

            /// <summary>
            /// Обработчик события - запрос на изменение позиции строки: N -> 0
            /// </summary>
            /// <param name="indx">Номер строки, которую требуется "поднять" вверх</param>
            private void onEventUp(int indx)
            {
                DataGridViewDiagnosticTaskRow row;
                // проверить корректность номера строки
                if (indx > 0) {
                    row = m_dgvValues.Rows[indx] as DataGridViewDiagnosticTaskRow;

                    m_dgvValues.Rows.Remove(row);
                    m_dgvValues.Rows.Insert(0, row);
                    // очистить последствия выбора строки
                    dgv_CellCancel(null, new DataGridViewCellEventArgs(0, 0));
                } else
                    ;
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
            private void InitializeComponent()
            {
                //this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Outset;

                m_dgvValues = new DataGridViewDiagnostic(); this.Controls.Add(m_dgvValues, 0, 1); this.SetRowSpan(m_dgvValues, COUNT_LAYOUT_ROW - 1);
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
                this.m_dgvValues.Columns[(int)INDEX_CELL.VALUE].Name = "Среднее время выполнения"; this.m_dgvValues.Columns[(int)INDEX_CELL.VALUE].Width = 10;
                this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VERIFICATION].Name = "Время проверки"; this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VERIFICATION].Width = 10;
                this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VALUE].Name = "Время выполнения задачи"; this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VALUE].Width = 10;
                this.m_dgvValues.Columns[(int)INDEX_CELL.ERROR_DESCRIPTION].Name = "Описание ошибки"; this.m_dgvValues.Columns[(int)INDEX_CELL.ERROR_DESCRIPTION].Width = 22;
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
            /// <summary>
            /// Сложный ключ для соваря со значениями при обновлении дочерних панелей
            /// </summary>
            private struct KEY_DIAGNOSTIC_PARAMETER
            {
                /// <summary>
                /// Перечисление - типов значений
                /// </summary>
                public enum ID_UNIT : short { UNKNOWN = -1, FLOAT = 12, DATETIME = 13 }
                /// <summary>
                /// Словарь с информацией о CLR-типах значений 
                /// </summary>
                public static Dictionary<ID_UNIT, Type> TypeOf = new Dictionary<ID_UNIT, Type>() {
                    { ID_UNIT.FLOAT, typeof(float) }
                    , { ID_UNIT.DATETIME, typeof(DateTime) }
                };
                /// <summary>
                /// Идентификаторы значений из БД конфигурации, таблица [DIAGNOSTIC_PARAM].[ID]
                /// </summary>
                public enum ID_VALUE : short
                {
                    UNKNOWN = -1
                    //, AIISKUE_VALUE = 1, AIISKUE_DATETIME = 4
                    //, SOTIASSO_1_VALUE = 2, SOTIASSO_2_VALUE, SOTIASSO_1_DATETIME = 5, SOTIASSO_2_DATETIME
                    //, SOTIASSO_1_TORIS_VALUE = 11, SOTIASSO_1_TORIS_DATETIME, SOTIASSO_2_TORIS_VALUE, SOTIASSO_2_TORIS_DATETIME
                    //, MODES_CENTRE_VALUE = 7, MODES_CENTRE_DATETIME
                    //, MODES_TERMINAL_VALUE = 9, MODES_TERMINAL_DATETIME
                    //, SIZE_DB = 27, SIZE_DB_LOG = 26
                    , AVG_TIME_TASK = 28
                }

                public ID_UNIT m_id_unit;

                public ID_VALUE m_id_value;
            }

            /// <summary>
            /// Словарь со значенями для отображения - преобразуется из таблицы-результата запроса
            /// </summary>
            private class DictionaryTaskValues : Dictionary<int, Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values>>
            {
                /// <summary>
                /// Конструктор - основной (с парметром)
                /// </summary>
                /// <param name="tableRecieved">Таблица - результат запроса значений</param>
                public DictionaryTaskValues(DataTable tableRecieved)
                {
                    int id = -1;

                    try {
                        foreach (DataRow r in tableRecieved.Rows) {
                            id = r.Field<int>(@"ID_EXT"); // читать как ИД ТЭЦ

                            if (!(id < (int)INDEX_SOURCE.TASK)) {
                                if (this.Keys.Contains(id) == false)
                                    Add(id, new Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values>());
                                else
                                    ;

                                this[id].Add(
                                    new KEY_DIAGNOSTIC_PARAMETER() {
                                        m_id_unit = (KEY_DIAGNOSTIC_PARAMETER.ID_UNIT)r.Field<int>(@"ID_Units")
                                        //??? читать как ???
                                        , m_id_value = (KEY_DIAGNOSTIC_PARAMETER.ID_VALUE)Convert.ToInt32(r.Field<string>(@"ID_Value"))
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
                        Logging.Logg().Exception(e, @"PanelTask.DictionaryTaskValues::ctor () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                }
            }

            /// <summary>
            /// Элемент интерфейса - строка в представлении для отображения данных
            /// </summary>
            private class DataGridViewDiagnosticTaskRow : DataGridViewDiagnosticRow
            {
                /// <summary>
                /// Конструктор основной (без параметров)
                /// </summary>
                public DataGridViewDiagnosticTaskRow() : base()
                {
                    //_states = new CELL_STATES((int)INDEX_CELL.COUNT);
                    m_cell_states = new INDEX_CELL_STATE[(int)INDEX_CELL.COUNT];
                }
                /// <summary>
                /// Описание строки - значение 0-го столбца
                /// </summary>
                public override string Name
                {
                    get { return Cells[(int)INDEX_CELL.NAME].Value.ToString(); }

                    set { Cells[(int)INDEX_CELL.NAME].Value = value; }
                }

                public bool Enabled { get; set; }

                //struct CELL_STATES : IEnumerable<INDEX_CELL_STATE>, IEnumerator
                //{
                //    private INDEX_CELL_STATE[] _states;
                //    private int index;

                //    public CELL_STATES(int capacity)
                //    {
                //        index = -1;
                //        _states = new INDEX_CELL_STATE[capacity];
                //    }

                //    // Реализуем интерфейс IEnumerator
                //    public bool MoveNext()
                //    {
                //        if (index == _states.Length - 1)
                //        {
                //            Reset();
                //            return false;
                //        }

                //        index++;
                //        return true;
                //    }

                //    public void Reset()
                //    {
                //        index = -1;
                //    }

                //    IEnumerator<INDEX_CELL_STATE> IEnumerable<INDEX_CELL_STATE>.GetEnumerator()
                //    {
                //        yield return this;
                //    }

                //    public object Current
                //    {
                //        get
                //        {
                //            return _states[index];
                //        }
                //    }
                //}

                /// <summary>
                /// Массив с стостояниями каждого из столбцов представления для отображения данных
                /// </summary>
                //private CELL_STATES _states;
                private INDEX_CELL_STATE[] m_cell_states;

                /// <summary>
                /// Событие для инициирования изменения текущего индекса строки
                /// </summary>
                public event DelegateIntFunc EventUp;

                /// <summary>
                /// Установить значения в столбцы для отображения
                /// </summary>
                /// <param name="values">Значения для отображения</param>
                public void SetValueCells(object []values)
                {
                    object value;
                    Color clrCell = Color.Empty;
                    bool enableValuePrevious = Enabled;
                    float fValue = -1F
                        , mm = -1 // минуты
                        , ss = -1; // секунды - среднее время выполнения задачи

                    foreach (INDEX_CELL i in Enum.GetValues(typeof(INDEX_CELL))) {
                        try {
                            if (((int)i < values.Length)
                                && (!(values[(int)i] == null))
                                //&& (string.IsNullOrEmpty((string)values[(int)i]) == false)
                                ) {
                                m_cell_states[(int)i] = INDEX_CELL_STATE.ERROR;
                                clrCell = s_CellState[(int)INDEX_CELL_STATE.OK].m_Color;

                                switch (i) {
                                    case INDEX_CELL.COUNT:
                                    // пропустить индекс
                                        continue;
                                    case INDEX_CELL.NAME:
                                    // пропустить применение значения к ячейке, т.к. значение(наименование источника данных) постоянное
                                        m_cell_states [(int)i] = INDEX_CELL_STATE.OK;
                                        value = null;
                                        break;
                                    case INDEX_CELL.STATE:
                                        m_cell_states[(int)i] = ((string)values[(int)i])?.Equals(1.ToString()) == true ?
                                            INDEX_CELL_STATE.OK :
                                                INDEX_CELL_STATE.DISABLED;
                                        value = s_CellState[(int)m_cell_states[(int)i]].m_Text;
                                        break;
                                    case INDEX_CELL.DATETIME_VALUE:
                                    case INDEX_CELL.DATETIME_VERIFICATION:
                                        m_cell_states[(int)i] = ((string)values [(int)INDEX_CELL.STATE])?.Equals (1.ToString ()) == true
                                            ? isRelevanceDateTime ((int)i, (DateTime)values[(int)i])
                                                : INDEX_CELL_STATE.DISABLED;
                                        value = values[(int)i] is DateTime ?
                                            formatDateTime((DateTime)values[(int)i]) :
                                                values[(int)i];
                                        break;
                                    case INDEX_CELL.VALUE:
                                        if (float.TryParse (values [(int)i].ToString(), out fValue) == true) {
                                            m_cell_states [(int)i] = isRelevanceValue (fValue);
                                            if (!(m_cell_states [(int)i] == INDEX_CELL_STATE.DISABLED)) {
                                                ss = fValue;
                                                mm = ss / 60;
                                                value = string.Format (@"{0:00}:{1:00}", mm, ss % 60);
                                            } else
                                                value = string.Format (@"--:--");
                                        } else
                                            value = string.Format (@"--:--");
                                        break;
                                    default:
                                        m_cell_states[(int)i] = /*((string)values[(int)i])?.Equals(EtalonPBR) ==*/ true ?
                                            INDEX_CELL_STATE.OK :
                                                INDEX_CELL_STATE.ERROR;
                                        value = values[(int)i];
                                        break;
                                }

                                if (!(value == null))
                                    Cells [(int)i].Value = value;
                                else
                                    ;
                                // изменить цвет ячейки ~ от состояния
                                Cells[(int)i].Style.BackColor = s_CellState[(int)m_cell_states[(int)i]].m_Color;

                                //if (!(m_cell_states[(int)i] == INDEX_CELL_STATE.OK))
                                //    EventUp?.Invoke((int)Tag);
                                //else
                                //    ;
                            }
                            else
                                ; // значение == null
                        } catch (Exception e) {
                            Logging.Logg().Exception(e, @"PanelTask.DataGridViewTaskRow::SetValueCells () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    } // INDEX_CELL i in Enum.GetValues(typeof(INDEX_CELL))

                    // проверить состояние ячеек, при необходимости "поднять вверх"
                    if ((Index > 0)
                        && ((!(m_cell_states[(int)INDEX_CELL.VALUE] == INDEX_CELL_STATE.OK))
                            || (!(m_cell_states[(int)INDEX_CELL.DATETIME_VALUE] == INDEX_CELL_STATE.OK))
                            || (!(m_cell_states[(int)INDEX_CELL.STATE] == INDEX_CELL_STATE.OK)))
                        ) {
                        EventUp?.Invoke(Index);
                    } else
                        ;
                }

                /// <summary>
                /// Коэффициент при определении достоверности значения в ячейке
                /// </summary>
                private float KoefRelevanceValue { get { return (int)Tag == 300 ? 3 : 1; } }

                /// <summary>
                /// Возвратить признак(состояние) достоверности значения
                /// </summary>
                /// <param name="value">Проверяемое значение</param>
                /// <returns>Состояние, соответствующее значению</returns>
                private INDEX_CELL_STATE isRelevanceValue(double value)
                {
                    return value < 0 ?
                        INDEX_CELL_STATE.DISABLED :
                            value < KoefRelevanceValue * 3 ?
                                INDEX_CELL_STATE.OK :
                                    value < KoefRelevanceValue * 6 ?
                                        INDEX_CELL_STATE.WARNING :
                                            INDEX_CELL_STATE.ERROR
                        ;
                }

                protected override INDEX_CELL_STATE isRelevanceValue(int iColumn, double value)
                {
                    throw new NotImplementedException();
                }

                /// <summary>
                /// Признак актуальности даты/времени
                /// </summary>
                /// <param name="indxCell">Номер(индекс) столбца</param>
                /// <param name="dtChecked">Значение даты/времени для проверки</param>
                /// <returns>Признак актуальности</returns>
                protected override INDEX_CELL_STATE isRelevanceDateTime(int iColumn, DateTime dtChecked)
                {
                    INDEX_CELL indxCell = (INDEX_CELL)iColumn;
                    INDEX_CELL_STATE stateRes = INDEX_CELL_STATE.OK;

                    TimeSpan tsDifference = SERVER_TIME - dtChecked;

                    if (tsDifference.TotalSeconds > 0)
                        switch (indxCell) {
                            case INDEX_CELL.DATETIME_VALUE:
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
                    // оставить 'OK' (дата/время обновления новее, чем время сервера)
                        ;

                    return stateRes;
                }
            }

            /// <summary>
            /// Загрузка значений
            /// </summary>
            private void update(Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values> values)
            {
            }

            /// <summary>
            /// Загрузка значений
            /// </summary>
            private void update(DictionaryTaskValues values)
            {
                object value;
                object[] rowValues;

                foreach (DataGridViewDiagnosticTaskRow r in m_dgvValues.Rows) {
                    rowValues = new object[(int)INDEX_CELL.COUNT];

                    foreach (KeyValuePair<int, Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values>> pairTask in values) {
                        try {
                            // сопоставление ???
                            if (((int)r.Tag == pairTask.Key)
                                && (!(pairTask.Value == null))) {
                                foreach (KeyValuePair<KEY_DIAGNOSTIC_PARAMETER, Values> pair in pairTask.Value) {
                                    switch (pair.Key.m_id_unit) {
                                        case KEY_DIAGNOSTIC_PARAMETER.ID_UNIT.FLOAT:
                                            // Хряпин А.Н. 12.10.2017 - начало (для возможности изменения цвета фона ячеек с наименованием)
                                            rowValues [(int)INDEX_CELL.NAME] =
                                                 Convert.ChangeType (pair.Value.m_name_shr, typeof (string));
                                            // Хряпин А.Н. 12.10.2017 - окончание блока
                                            if (string.IsNullOrEmpty((string)pair.Value.m_value) == true)
                                            // признак отсутствия значения
                                                rowValues[(int)INDEX_CELL.VALUE] = -1;
                                            else
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
                                }
                            } else
                                ;
                        } catch (Exception e) {
                            Logging.Logg().Exception(e, @"PanelTask::update () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    } // KeyValuePair<KEY_DIAGNOSTIC_PARAMETER, Values> pair in values

                    r.SetValueCells(rowValues);
                } // DataGridViewGTPRow r in m_dgvValues.Rows
            }

            /// <summary>
            /// Обработчик события панели-родителя - обновление значений
            /// </summary>
            /// <param name="rec">Таблица с результатами запрос</param>
            public void Update(object rec)
            {
                m_semUpdateHandler.WaitOne();

                if (InvokeRequired == true)
                    Invoke(new Action<DictionaryTaskValues>(update), new DictionaryTaskValues((rec as DataTable)));
                else
                    update(new DictionaryTaskValues((rec as DataTable)));

                m_semUpdateHandler.Release(1);
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
        }
    }
}
