using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Globalization;

namespace StatisticCommon
{
    public class DataGridViewAdminKomDisp : DataGridView
    {
        public enum DESC_INDEX : ushort { DATE_HOUR, PLAN, RECOMENDATION, DEVIATION_TYPE, DEVIATION, TO_ALL, COUNT_COLUMN };
        private static string[] arDescStringIndex = { "DateHour", "Plan", "Recomendation", "DeviationType", "Deviation", "ToAll" };
        private static string[] arDescRusStringIndex = { "Дата, час", "План", "Рекомендация", "Отклонение в процентах", "Величина максимального отклонения", "Дозаполнить" };

        private const double maxPlanValue = 1500;
        private const double maxRecomendationValue = 1500;
        private const double maxDeviationValue = 1500;
        private const double maxDeviationPercentValue = 100;

        public DataGridViewAdminKomDisp () {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();

            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            
            dataGridViewCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;

            ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
            ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Columns.AddRange(new DataGridViewColumn[(int)DESC_INDEX.COUNT_COLUMN] {new DataGridViewTextBoxColumn (),
                                                                                    new DataGridViewTextBoxColumn (),
                                                                                    new DataGridViewTextBoxColumn (),
                                                                                    new DataGridViewCheckBoxColumn (),
                                                                                    new DataGridViewTextBoxColumn (),
                                                                                    new DataGridViewButtonColumn ()});
            // 
            // DateHour
            // 
            Columns[(int)DESC_INDEX.DATE_HOUR].Frozen = true;
            Columns[(int)DESC_INDEX.DATE_HOUR].HeaderText = arDescRusStringIndex[(int)DESC_INDEX.DATE_HOUR];
            Columns[(int)DESC_INDEX.DATE_HOUR].Name = arDescStringIndex[(int)DESC_INDEX.DATE_HOUR];
            Columns[(int)DESC_INDEX.DATE_HOUR].ReadOnly = true;
            Columns[(int)DESC_INDEX.DATE_HOUR].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Plan
            // 
            //Columns[(int)DESC_INDEX.PLAN].Frozen = true;
            Columns[(int)DESC_INDEX.PLAN].HeaderText = arDescRusStringIndex[(int)DESC_INDEX.PLAN];
            Columns[(int)DESC_INDEX.PLAN].Name = arDescStringIndex[(int)DESC_INDEX.PLAN];
            Columns[(int)DESC_INDEX.PLAN].ReadOnly = false;
            Columns[(int)DESC_INDEX.PLAN].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            Columns[(int)DESC_INDEX.PLAN].Width = 70;
            // 
            // Recommendation
            // 
            Columns[(int)DESC_INDEX.RECOMENDATION].HeaderText = arDescRusStringIndex[(int)DESC_INDEX.RECOMENDATION];
            Columns[(int)DESC_INDEX.RECOMENDATION].Name = arDescStringIndex[(int)DESC_INDEX.RECOMENDATION];
            Columns[(int)DESC_INDEX.RECOMENDATION].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // DeviationType
            // 
            Columns[(int)DESC_INDEX.DEVIATION_TYPE].HeaderText = arDescRusStringIndex[(int)DESC_INDEX.DEVIATION_TYPE];
            Columns[(int)DESC_INDEX.DEVIATION_TYPE].Name = arDescStringIndex[(int)DESC_INDEX.DEVIATION_TYPE];
            Columns[(int)DESC_INDEX.DEVIATION_TYPE].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // Deviation
            // 
            Columns[(int)DESC_INDEX.DEVIATION].HeaderText = arDescRusStringIndex[(int)DESC_INDEX.DEVIATION];
            Columns[(int)DESC_INDEX.DEVIATION].Name = arDescStringIndex[(int)DESC_INDEX.DEVIATION];
            Columns[(int)DESC_INDEX.DEVIATION].Resizable = System.Windows.Forms.DataGridViewTriState.True;
            Columns[(int)DESC_INDEX.DEVIATION].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ToAll
            // 
            Columns[(int)DESC_INDEX.TO_ALL].HeaderText = arDescRusStringIndex[(int)DESC_INDEX.TO_ALL];
            Columns[(int)DESC_INDEX.TO_ALL].Name = arDescStringIndex[(int)DESC_INDEX.TO_ALL];

            CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellClick);
            CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellValidated);

            Name = "m_dgwAdminTable";
            RowHeadersVisible = false;
            RowTemplate.Resizable = DataGridViewTriState.False;

            Rows.Add(24);

            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.dgwAdminTable_KeyUp);
        }

        /*
        private void cellValidated(int rowIndx, int colIndx)
        {
            double value;
            bool valid;

            valid = double.TryParse((string)this.dgwAdminTable.Rows[rowIndx].Cells[colIndx].Value, out value);
            if (!valid || value > maxRecomendationValue)
            {
                m_curRDGValues.recommendations[rowIndx] = 0;
                this.dgwAdminTable.Rows[rowIndx].Cells[colIndx].Value = 0.ToString("F2");
            }
            else
            {
                m_curRDGValues.recommendations[rowIndx] = value;
                this.dgwAdminTable.Rows[rowIndx].Cells[colIndx].Value = value.ToString("F2");
            }
        }
        */

        private void dgwAdminTable_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            double value;
            bool valid;

            switch (e.ColumnIndex)
            {
                case (int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN: // План
                    //cellValidated(e.RowIndex, (int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN);

                    valid = double.TryParse((string)Rows[e.RowIndex].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].Value, out value);
                    if (!valid || value > maxRecomendationValue)
                    {
                        //m_curRDGValues[e.RowIndex].plan = 0;
                        Rows[e.RowIndex].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].Value = 0.ToString("F2");
                    }
                    else
                    {
                        //m_curRDGValues[e.RowIndex].plan = value;
                        Rows[e.RowIndex].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].Value = value.ToString("F2");
                    }
                    break;
                case (int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION: // Рекомендация
                    {
                        //cellValidated(e.RowIndex, (int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION);

                        valid = double.TryParse((string)Rows[e.RowIndex].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value, out value);
                        if (!valid || value > maxRecomendationValue)
                        {
                            //m_curRDGValues[e.RowIndex].recomendation = 0;
                            Rows[e.RowIndex].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value = 0.ToString("F2");
                        }
                        else
                        {
                            //m_curRDGValues[e.RowIndex].recomendation = value;
                            Rows[e.RowIndex].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value = value.ToString("F2");
                        }
                        break;
                    }
                case (int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE:
                    {
                        //m_curRDGValues[e.RowIndex].deviationPercent = bool.Parse(this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                        break;
                    }
                case (int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION: // Максимальное отклонение
                    {
                        valid = double.TryParse((string)Rows[e.RowIndex].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION].Value, out value);
                        bool isPercent = bool.Parse(Rows[e.RowIndex].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                        double maxValue;
                        double recom = double.Parse((string)Rows[e.RowIndex].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value);

                        if (isPercent)
                            maxValue = maxDeviationPercentValue;
                        else
                            maxValue = maxDeviationValue; // вообще эти значения не суммируются, но для максимальной границы нормально

                        if (!valid || value < 0 || value > maxValue)
                        {
                            //m_curRDGValues[e.RowIndex].deviation = 0;
                            Rows[e.RowIndex].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION].Value = 0.ToString("F2");
                        }
                        else
                        {
                            //m_curRDGValues[e.RowIndex].deviation = value;
                            Rows[e.RowIndex].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION].Value = value.ToString("F2");
                        }
                        break;
                    }
            }
        }

        private void dgwAdminTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == (int)DataGridViewAdminKomDisp.DESC_INDEX.TO_ALL && e.RowIndex >= 0) // кнопка применение для всех
            {
                for (int i = e.RowIndex + 1; i < 24; i++)
                {
                    //m_curRDGValues[i].plan = m_curRDGValues[e.RowIndex].plan;
                    //m_curRDGValues[i].recomendation = m_curRDGValues[e.RowIndex].recomendation;
                    //m_curRDGValues[i].deviationPercent = m_curRDGValues[e.RowIndex].deviationPercent;
                    //m_curRDGValues[i].deviation = m_curRDGValues[e.RowIndex].deviation;

                    Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].Value = Rows[e.RowIndex].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].Value;
                    Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value = Rows[e.RowIndex].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value;
                    Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE].Value = Rows[e.RowIndex].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE].Value;
                    Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION].Value = Rows[e.RowIndex].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION].Value;
                }
            }
            else
                ;
        }

        private void dgwAdminTable_KeyUp(object sender, KeyEventArgs e)
        {
            //if user clicked Shift+Ins or Ctrl+V (paste from clipboard)
            if ((e.Shift && e.KeyCode == Keys.Insert) || (e.Control && e.KeyCode == Keys.V))
            {
                char[] rowSplitter = { '\r', '\n' };
                char[] columnSplitter = { '\t' };

                //get the text from clipboard
                IDataObject dataInClipboard = Clipboard.GetDataObject();
                string stringInClipboard = (string)dataInClipboard.GetData(DataFormats.Text);

                //split it into lines
                string[] rowsInClipboard = stringInClipboard.Split(rowSplitter, StringSplitOptions.RemoveEmptyEntries);

                if (rowsInClipboard.Length == 0)
                    return;
                else
                    ;

                //get the row and column of selected cell in grid
                int rowSelectedCur = SelectedCells[0].RowIndex;
                int colSelectedCur = SelectedCells[0].ColumnIndex;

                int colsInClipboard = rowsInClipboard[0].Split(columnSplitter).Length;

                //add rows into grid to fit clipboard lines
                if (Rows.Count < (rowSelectedCur + rowsInClipboard.Length))
                    //Rows.Add(r + rowsInClipboard.Length - Rows.Count);
                    return;
                else
                    ;

                //Если кол-во столбцов в БУФЕРе = 0 ИЛИ > кол-ва столбцов DataGridView (для редактирования) -> ВЫХОД
                if ((colSelectedCur < 1) || (colsInClipboard > (ColumnCount - 1 - 1)) || ((colSelectedCur + colsInClipboard) > (ColumnCount - 1)))
                    return;
                else
                    ;

                double dblValue;
                bool bValid = false, bValue;
                // loop through the lines, split them into cells and place the values in the corresponding cell.
                for (int iRow = 0; iRow < rowsInClipboard.Length; iRow++)
                {
                    //split row into cell values
                    string[] valuesInRow = rowsInClipboard[iRow].Split(columnSplitter);

                    //cycle through cell values
                    for (int iCol = 0; iCol < valuesInRow.Length; iCol++)
                    {
                        //assign cell value, only if it within columns of the grid
                        if (colSelectedCur + iCol < ColumnCount - 1)
                        {
                            switch (Columns[colSelectedCur + iCol].GetType().ToString())
                            {
                                case "System.Windows.Forms.DataGridViewTextBoxColumn":
                                    bValid = double.TryParse(valuesInRow[iCol], out dblValue);
                                    break;
                                case "System.Windows.Forms.DataGridViewCheckBoxColumn":
                                    bValid = bool.TryParse(valuesInRow[iCol], out bValue);
                                    break;
                                default:
                                    break;
                            }

                            if (bValid)
                                Rows[rowSelectedCur + iRow].Cells[colSelectedCur + iCol].Value = valuesInRow[iCol];
                            else
                                ;
                        }
                        else
                            ;
                    }
                }
            }
        } 
    }
}
