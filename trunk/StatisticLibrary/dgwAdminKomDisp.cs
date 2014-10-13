using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
using System.Globalization;

namespace StatisticCommon
{
    public class DataGridViewAdminKomDisp : DataGridViewAdmin
    {
        public enum DESC_INDEX : ushort { DATE_HOUR, PLAN, RECOMENDATION, DEVIATION_TYPE, DEVIATION, TO_ALL, COUNT_COLUMN };
        private static string[] arDescStringIndex = { "DateHour", "Plan", "Recomendation", "DeviationType", "Deviation", "ToAll" };
        private static string[] arDescRusStringIndex = { "Дата, час", "План", "Рекомендация", "Отклонение в процентах", "Величина максимального отклонения", "Дозаполнить" };

        public DataGridViewAdminKomDisp()
        {
        }
        
        protected override void InitializeComponents()
        {
            base.InitializeComponents();

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
            Columns[(int)DESC_INDEX.PLAN].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            Columns[(int)DESC_INDEX.PLAN].Width = 70;
            Columns[(int)DESC_INDEX.PLAN].ReadOnly = true;
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
        }

        protected override void dgwAdminTable_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            double value;
            bool valid;

            switch (e.ColumnIndex)
            {
                case (int)DESC_INDEX.PLAN: // План
                    //cellValidated(e.RowIndex, (int)DESC_INDEX.PLAN);

                    valid = double.TryParse((string)Rows[e.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value, out value);
                    if ((valid == false) || (value > maxRecomendationValue))
                    {
                        //m_curRDGValues[e.RowIndex].plan = 0;
                        Rows[e.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value = 0.ToString("F2");
                    }
                    else
                    {
                        //m_curRDGValues[e.RowIndex].plan = value;
                        Rows[e.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value = value.ToString("F2");
                    }
                    break;
                case (int)DESC_INDEX.RECOMENDATION: // Рекомендация
                    {
                        //cellValidated(e.RowIndex, (int)DESC_INDEX.RECOMENDATION);

                        valid = double.TryParse((string)Rows[e.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value, out value);
                        if ((valid == false) || (value > maxRecomendationValue))
                        {
                            //m_curRDGValues[e.RowIndex].recomendation = 0;
                            Rows[e.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value = 0.ToString("F2");
                        }
                        else
                        {
                            //m_curRDGValues[e.RowIndex].recomendation = value;
                            Rows[e.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value = value.ToString("F2");
                        }
                        break;
                    }
                case (int)DESC_INDEX.DEVIATION_TYPE:
                    {
                        //m_curRDGValues[e.RowIndex].deviationPercent = bool.Parse(this.dgwAdminTable.Rows[e.RowIndex].Cells[(int)DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                        break;
                    }
                case (int)DESC_INDEX.DEVIATION: // Максимальное отклонение
                    {
                        valid = double.TryParse((string)Rows[e.RowIndex].Cells[(int)DESC_INDEX.DEVIATION].Value, out value);
                        bool isPercent = bool.Parse(Rows[e.RowIndex].Cells[(int)DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                        double maxValue;
                        double recom = double.Parse((string)Rows[e.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value);

                        if (isPercent)
                            maxValue = maxDeviationPercentValue;
                        else
                            maxValue = maxDeviationValue; // вообще эти значения не суммируются, но для максимальной границы нормально

                        if (!valid || value < 0 || value > maxValue)
                        {
                            //m_curRDGValues[e.RowIndex].deviation = 0;
                            Rows[e.RowIndex].Cells[(int)DESC_INDEX.DEVIATION].Value = 0.ToString("F2");
                        }
                        else
                        {
                            //m_curRDGValues[e.RowIndex].deviation = value;
                            Rows[e.RowIndex].Cells[(int)DESC_INDEX.DEVIATION].Value = value.ToString("F2");
                        }
                        break;
                    }
            }
        }

        public override void ClearTables()
        {
            for (int i = 0; i < 24; i++)
            {
                Rows[i].Cells[(int)DESC_INDEX.DATE_HOUR].Value = "";
                Rows[i].Cells[(int)DESC_INDEX.PLAN].Value = "";
                Rows[i].Cells[(int)DESC_INDEX.RECOMENDATION].Value = "";
                Rows[i].Cells[(int)DESC_INDEX.DEVIATION_TYPE].Value = "false";
                Rows[i].Cells[(int)DESC_INDEX.DEVIATION].Value = "";
            }
        }
    }
}
