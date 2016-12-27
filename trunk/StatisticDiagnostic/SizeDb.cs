﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

using HClassLibrary;
using System.Data;
using System.Drawing;

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
                    iNewRow = m_dgvValues.Rows.Add(new DataGridViewDiagnosticDbRow ());

                    m_dgvValues.Rows[iNewRow].Tag = new DataGridViewDiagnosticDbRow.KEY_TAG() {
                        m_id = src.m_id_component
                        , m_id_value = (KEY_DIAGNOSTIC_PARAMETER.ID_VALUE)src.m_id
                    };

                    (m_dgvValues.Rows[iNewRow] as DataGridViewDiagnosticDbRow).Name = src.m_name_shr;
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
                this.m_dgvValues.Columns[(int)INDEX_CELL.NAME].Name = "Имя базы данных";
                this.m_dgvValues.Columns[(int)INDEX_CELL.VALUE].Name = "Размер базы данных, МБ";
                this.m_dgvValues.Columns[(int)INDEX_CELL.DATETIME_VERIFICATION].Name = "Время проверки";
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
            private enum INDEX_CELL : short { NAME, VALUE, DATETIME_VERIFICATION
                , COUNT
            }

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
                    , SIZE_DB = 27, SIZE_DB_LOG = 26
                    //, AVG_TIME_TASK = 28
                }

                public ID_UNIT m_id_unit;

                public ID_VALUE m_id_value;
            }

            private class DictionaryDbValues : Dictionary<int, Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values>>
            {
                /// <summary>
                /// Конструктор - основной (с парметром)
                /// </summary>
                /// <param name="tableRecieved">Таблица - результат запроса значений</param>
                public DictionaryDbValues(DataTable tableRecieved)
                {
                    int id = -1;

                    try {
                        foreach (DataRow r in tableRecieved.Rows) {
                            id = r.Field<int>(@"ID_EXT"); // читать как ИД ТЭЦ

                            if ((!(id < (int)INDEX_SOURCE.SIZEDB))
                                && (id < (int)INDEX_SOURCE.MODES)) {
                                if (this.Keys.Contains(id) == false)
                                    Add(id, new Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values>());
                                else
                                    ;

                                this[id].Add(
                                    new KEY_DIAGNOSTIC_PARAMETER() {
                                        m_id_unit = (KEY_DIAGNOSTIC_PARAMETER.ID_UNIT)r.Field<int>(@"ID_Units")
                                        //??? читать как ИД ГТП
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

            private class DataGridViewDiagnosticDbRow : DataGridViewDiagnosticRow
            {
                /// <summary>
                /// Идентификатор строки
                /// </summary>
                public struct KEY_TAG
                {
                    public int m_id;

                    public KEY_DIAGNOSTIC_PARAMETER.ID_VALUE m_id_value;

                    public override bool Equals(object key)
                    {
                        return (this.m_id == ((KEY_TAG)key).m_id)
                            && (this.m_id_value == ((KEY_TAG)key).m_id_value);
                    }

                    public override int GetHashCode()
                    {
                        return base.GetHashCode();
                    }

                    public static bool operator ==(KEY_TAG key1, KEY_TAG key2)
                    {
                        return key1.Equals(key2);
                    }

                    public static bool operator !=(KEY_TAG key1, KEY_TAG key2)
                    {
                        return !key1.Equals(key2);
                    }
                }

                public override string Name
                {
                    get { return Cells[(int)INDEX_CELL.NAME].Value.ToString(); }

                    set { Cells[(int)INDEX_CELL.NAME].Value = value; }
                }

                public bool Enabled { get; set; }

                public void SetValueCells(object []values)
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
                                    case INDEX_CELL.NAME:
                                    case INDEX_CELL.COUNT:
                                        continue;
                                        break;
                                    //case INDEX_CELL.STATE:
                                    //    indxState = ((string)values[(int)i])?.Equals(1.ToString()) == true ?
                                    //        INDEX_CELL_STATE.OK :
                                    //            INDEX_CELL_STATE.ERROR;
                                    //    value = s_CellState[(int)indxState].m_Text;
                                    //    clrCell = s_CellState[(int)indxState].m_Color;
                                    //    break;
                                    //case INDEX_CELL.DATETIME_VALUE:
                                    case INDEX_CELL.DATETIME_VERIFICATION:
                                        value = values[(int)i] is DateTime ?
                                            formatDateTime((DateTime)values[(int)i]) :
                                                values[(int)i];
                                        clrCell = s_CellState[(int)isRelevanceDateTime(i, (DateTime)values[(int)i])].m_Color;
                                        break;
                                    case INDEX_CELL.VALUE:
                                        value = values[(int)i];
                                        clrCell = s_CellState[(int)isRelevanceValue((double)values[(int)i])].m_Color;
                                        break;
                                    default:
                                        indxState = /*((string)values[(int)i])?.Equals(EtalonPBR) ==*/ true ?
                                            INDEX_CELL_STATE.OK :
                                                INDEX_CELL_STATE.ERROR;
                                        value = values[(int)i];
                                        break;
                                }

                                Cells[(int)i].Value = value;
                                // изменить цвет ячейки
                                Cells[(int)i].Style.BackColor = clrCell;
                            }
                            else
                                ; // значение == null
                        } catch (Exception e) {
                            Logging.Logg().Exception(e, @"PanelTask.DataGridViewDbRow::SetValueCells () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    } // INDEX_CELL i in Enum.GetValues(typeof(INDEX_CELL))
                }

                private INDEX_CELL_STATE isRelevanceValue(double value)
                {
                    return //value < 3 ?
                        INDEX_CELL_STATE.OK //:
                            //value < 3 ?
                            //    INDEX_CELL_STATE.WARNING :
                            //        INDEX_CELL_STATE.ERROR
                        ;
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
                        switch (indxCell) {
                            //case INDEX_CELL.DATETIME_VALUE:
                            case INDEX_CELL.DATETIME_VERIFICATION:
                                stateRes = (tsDifference.TotalMinutes > 11) ?
                                    (tsDifference.TotalMinutes > 61) ?
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
            }

            /// <summary>
            /// Загрузка значений
            /// </summary>
            private void update(DictionaryDbValues values)
            {
                object value;
                object[] rowValues;
                DataGridViewDiagnosticDbRow.KEY_TAG key;

                foreach (DataGridViewDiagnosticDbRow r in m_dgvValues.Rows) {
                    rowValues = new object[(int)INDEX_CELL.COUNT];

                    foreach (KeyValuePair<int, Dictionary<KEY_DIAGNOSTIC_PARAMETER, Values>> pairTask in values) {
                        foreach (KeyValuePair<KEY_DIAGNOSTIC_PARAMETER, Values> pair in pairTask.Value) {
                            try {
                                // сопоставление ???
                                key = new DataGridViewDiagnosticDbRow.KEY_TAG() { m_id = pairTask.Key, m_id_value = pair.Key.m_id_value };
                                if ((((DataGridViewDiagnosticDbRow.KEY_TAG)r.Tag) == key)
                                    && (!(pairTask.Value == null))) {
                                    switch (pair.Key.m_id_unit) {
                                        case KEY_DIAGNOSTIC_PARAMETER.ID_UNIT.FLOAT:
                                            rowValues[(int)INDEX_CELL.VALUE] = HMath.doubleParse((string)pair.Value.m_value);
                                        //    break;
                                        //case KEY_DIAGNOSTIC_PARAMETER.ID_UNIT.DATETIME:
                                            rowValues[(int)INDEX_CELL.DATETIME_VERIFICATION] =
                                                Convert.ChangeType(pair.Value.m_dtValue, typeof(DateTime));
                                            break;
                                        default:
                                            break;
                                    }
                                } else
                                    ;
                            } catch (Exception e) {
                                Logging.Logg().Exception(e, @"PanelSizeDb::update () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                            }
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
                if (InvokeRequired == true)
                    Invoke(new Action<DictionaryDbValues>(update), new DictionaryDbValues((rec as DataTable)));
                else
                    update(new DictionaryDbValues((rec as DataTable)));
            }

            /// <summary>
            /// Снятие выделения с ячеек
            /// </summary>
            /// <param name="sender">Объект, инициироввавший событие</param>
            /// <param name="e">Аргумент события</param>
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
