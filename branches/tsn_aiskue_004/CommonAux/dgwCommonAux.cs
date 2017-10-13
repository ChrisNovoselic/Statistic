﻿using System;
using System.Data.Common;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Data;
using System.Globalization;

using ZedGraph;
using GemBox.Spreadsheet;

using HClassLibrary;
using StatisticCommon;
using System.Reflection;

namespace CommonAux
{
    class DataGridViewValues : DataGridView
    {
        /// <summary>
        /// Идентификатор итоговой строки
        /// </summary>
        private string groupRowTag = @"Группа";
        /// <summary>
        /// Конструктор - основной (без параметров)
        /// </summary>
        public DataGridViewValues() : base()
        {
            addColumns();

            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
            DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        /// <summary>
        /// Добавить столбцы по количеству часов + столбец для итогового для сигнала значения
        /// </summary>
        private void addColumns()
        {
            int iHour = -1;

            for (iHour = 0; iHour < 24; iHour++)
            {
                Columns.Add((iHour + 1).ToString(@"00"), (iHour + 1).ToString(@"00"));
                // отобразить номер часа в заголовке столбца
                //Columns[ColumnCount - 1].HeaderCell.Value = string.Format(@"{0:HH:mm}", iHour < 23 ? TimeSpan.FromHours(iHour + 1) : TimeSpan.Zero);
            }
            // добавить столбец для итогового для сигнала значения
            Columns.Add(@"DATE", @"Сутки");
        }
        /// <summary>
        /// Добавить строки для сигналов
        /// </summary>
        /// <param name="listSgnls">Срисок сигналов</param>
        public void AddRowData(List<SIGNAL> listSgnls)
        {
            foreach (SIGNAL sgnl in listSgnls)
            {
                Rows.Add();
                // отобразить наименование сигнала
                Rows[RowCount - 1].HeaderCell.Value = sgnl.m_strDesc;
                //Rows[RowCount - 1].HeaderCell.Size.Width = 200;
                // назначить идентификатор строки
                Rows[RowCount - 1].Tag = new SIGNAL.KEY(sgnl.m_key);

                //Rows[RowCount-1].

                Rows[RowCount - 1].DefaultCellStyle.BackColor = sgnl.m_bUse == true ?
                    System.Drawing.Color.Empty :
                        sgnl.m_bUse == false ? System.Drawing.Color.LightGray :
                            System.Drawing.Color.Gray;
            }
            // добавить строку итого
            Rows.Add();
            Rows[RowCount - 1].HeaderCell.Value = groupRowTag;
            // назначить идентификатор итоговой строки
            Rows[RowCount - 1].Tag = groupRowTag;
        }
        /// <summary>
        /// Удалить все строки(сигналы)
        /// </summary>
        public void ClearRows()
        {
            Rows.Clear();
        }
        /// <summary>
        /// Очистить значения во всех ячейках
        /// </summary>
        public void ClearValues()
        {
            foreach (DataGridViewRow r in Rows)
                foreach (DataGridViewCell c in r.Cells)
                    c.Value = string.Empty;
        }
        /// <summary>
        /// Отобразить значения
        /// </summary>
        /// <param name="values">Значения для отображения</param>
        public void Update(TEC_LOCAL.VALUES_DATE.VALUES_GROUP values)
        {
            int iHour = -1 // номер столбца (часа)
                , iRow = -1; // номер строки для сигнала
            double value = -1F;
            SIGNAL.KEY key; // ключ сигнала - идетификатор строки

            foreach (DataGridViewRow r in Rows)
            {
                key = values.Keys.ToList().Find(item => { if (r.Tag is SIGNAL.KEY) return item == (SIGNAL.KEY)r.Tag; else return false; });
                // проверить найден ли ключ
                if ((key.m_object > 0)
                    && (key.m_item > 0))
                {
                    iRow = Rows.IndexOf(r);
                    // заполнить значения для сигнала по часам в сутках
                    for (iHour = 0; iHour < 24; iHour++)
                    {
                        value = values[key].m_data[iHour];
                        // отобразить значение
                        r.Cells[iHour].Value = value.ToString(@"F3");
                    }
                    // отобразить значение для сигнала за сутки
                    r.Cells[iHour].Value = values.m_summaSgnls[values.Keys.ToList().IndexOf(key)].ToString(@"F3");
                }
                else
                // ключ для строки не найден
                    if ((r.Tag is string)
                        && (((string)r.Tag).Equals(groupRowTag) == true))
                {
                    // итоговая строка
                    for (iHour = 0; iHour < 24; iHour++)
                        r.Cells[iHour].Value = values.m_summaHours[iHour].ToString(@"F3");

                    r.Cells[iHour].Value = values.m_Summa;
                }
                else
                    Logging.Logg().Error(
                        string.Format(@"View.DataGridViewValues::Update () - не найден столбец для KEY_SIGNAL=[object={0}, item={1}]"
                            , ((SIGNAL.KEY)r.Tag).m_object, ((SIGNAL.KEY)r.Tag).m_item)
                        , Logging.INDEX_MESSAGE.NOT_SET);
            }
        }
    }
}
