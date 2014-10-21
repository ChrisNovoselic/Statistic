using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
using System.Globalization;

namespace StatisticCommon
{
    public abstract class DataGridViewAdmin : DataGridView
    {
        protected const double maxPlanValue = 1500;
        protected const double maxRecomendationValue = 1500;
        protected const double maxDeviationValue = 1500;
        protected const double maxDeviationPercentValue = 100;

        protected virtual void InitializeComponents () {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();

            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) |
                                                            System.Windows.Forms.AnchorStyles.Left)));

            dataGridViewCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;

            ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
            ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        }

        public DataGridViewAdmin () {
            InitializeComponents ();

            CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellClick);
            CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgwAdminTable_CellValidated);

            Name = "m_dgwAdminTable";
            RowHeadersVisible = false;
            RowTemplate.Resizable = DataGridViewTriState.False;

            RowsAdd ();

            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.dgwAdminTable_KeyUp);
        }

        public void InitRows(int cnt, bool bIns)
        {
            if (bIns == true)
                while (Rows.Count < cnt)
                    Rows.Insert(0, 1);
            else
                while (Rows.Count > cnt)
                    Rows.RemoveAt(0);
        }

        protected void RowsAdd () { Rows.Add(24); }

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

        protected abstract void dgwAdminTable_CellValidated(object sender, DataGridViewCellEventArgs e);

        protected virtual void dgwAdminTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == (int)DataGridViewAdminKomDisp.DESC_INDEX.TO_ALL && e.RowIndex >= 0) // кнопка применение для всех
            {
                int colStart = (int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN;
                if (Columns[colStart].ReadOnly == true)
                    colStart ++;
                else
                    ;

                for (int i = e.RowIndex + 1; i < 24; i++)
                {
                    //m_curRDGValues[i].plan = m_curRDGValues[e.RowIndex].plan;
                    //m_curRDGValues[i].recomendation = m_curRDGValues[e.RowIndex].recomendation;
                    //m_curRDGValues[i].deviationPercent = m_curRDGValues[e.RowIndex].deviationPercent;
                    //m_curRDGValues[i].deviation = m_curRDGValues[e.RowIndex].deviation;

                    for (int j = colStart; j < (int)DataGridViewAdminKomDisp.DESC_INDEX.TO_ALL; j ++) {
                        Rows[i].Cells[j].Value = Rows[e.RowIndex].Cells[j].Value;
                    }
                }
            }
            else
                ;
        }

        private void dgwAdminTable_KeyUp(object sender, KeyEventArgs e)
        {
            //get the row and column of selected cell in grid
            int rowSelectedCur = SelectedCells[0].RowIndex,
                colSelectedCur = SelectedCells[0].ColumnIndex,
                iRow = -1, iCol = -1;
            
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
                for (iRow = 0; iRow < rowsInClipboard.Length; iRow++)
                {
                    //split row into cell values
                    string[] valuesInRow = rowsInClipboard[iRow].Split(columnSplitter);

                    //cycle through cell values
                    for (iCol = 0; iCol < valuesInRow.Length; iCol++)
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

                            if ((bValid == true) && (Columns[colSelectedCur + iCol].ReadOnly == false))
                                Rows[rowSelectedCur + iRow].Cells[colSelectedCur + iCol].Value = valuesInRow[iCol];
                            else
                                ;
                        }
                        else
                            ;
                    }
                }
            }
            else
                if (e.KeyCode == Keys.Delete)
                {
                    iRow =
                    iCol =
                    0;

                    Rows[rowSelectedCur + iRow].Cells[colSelectedCur + iCol].Value = 0.ToString("F2"); 
                }
                else
                    ;
        }

        public abstract void ClearTables();
    }
}
