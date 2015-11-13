using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
using System.Globalization;

using HClassLibrary;

namespace StatisticCommon
{
    public class DataGridViewAdminKomDisp : DataGridViewAdmin
    {
        public enum DESC_INDEX : ushort { DATE_HOUR, PLAN, UDGe, RECOMENDATION, FOREIGN_CMD, DEVIATION_TYPE, DEVIATION, TO_ALL, COUNT_COLUMN };
        private static string[] arDescStringIndex = { "DateHour", "Plan", @"UDGe", @"ForeignCmd", "Recomendation", "DeviationType", "Deviation", "ToAll" };
        private static string[] arDescRusStringIndex = { "Дата, час", "План", @"УДГэ", "Рекомендация", @"Внешн. ком-да", "Отклонение в процентах", "Величина максимального отклонения", "Дозаполнить" };
        private static string[] arDefaultValueIndex = { string.Empty, string.Empty, string.Empty, string.Empty, false.ToString(), false.ToString(), string.Empty };

        public double m_PBR_0;

        public DataGridViewAdminKomDisp()
        {
            this.CellMouseMove += new DataGridViewCellMouseEventHandler (dgwAdminTable_CellMouseMove);
        }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            Columns.AddRange(new DataGridViewColumn[(int)DESC_INDEX.COUNT_COLUMN] {new DataGridViewTextBoxColumn (),
                                                                                    new DataGridViewTextBoxColumn (),
                                                                                    new DataGridViewTextBoxColumn (),
                                                                                    new DataGridViewTextBoxColumn (),
                                                                                    new DataGridViewCheckBoxColumn (),
                                                                                    new DataGridViewCheckBoxColumn (),
                                                                                    new DataGridViewTextBoxColumn (),
                                                                                    new DataGridViewButtonColumn ()});
            int i = -1;
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
            i = (int)DESC_INDEX.PLAN;
            //Columns[(int)DESC_INDEX.PLAN].Frozen = true;
            Columns[i].HeaderText = arDescRusStringIndex[i];
            Columns[i].Name = arDescStringIndex[i];
            Columns[i].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            Columns[i].Width = 56;
            Columns[i].ReadOnly = true;
            // 
            // UDGe
            // 
            i = (int)DESC_INDEX.UDGe;
            //Columns[(int)DESC_INDEX.PLAN].Frozen = true;
            Columns[i].HeaderText = arDescRusStringIndex[i];
            Columns[i].Name = arDescStringIndex[i];
            Columns[i].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            Columns[i].Width = 56;
            Columns[i].ReadOnly = true;
            // 
            // Recommendation
            // 
            Columns[(int)DESC_INDEX.RECOMENDATION].HeaderText = arDescRusStringIndex[(int)DESC_INDEX.RECOMENDATION];
            Columns[(int)DESC_INDEX.RECOMENDATION].Name = arDescStringIndex[(int)DESC_INDEX.RECOMENDATION];
            Columns[(int)DESC_INDEX.RECOMENDATION].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // ForeignCmd
            // 
            Columns[(int)DESC_INDEX.FOREIGN_CMD].HeaderText = arDescRusStringIndex[(int)DESC_INDEX.FOREIGN_CMD];
            Columns[(int)DESC_INDEX.FOREIGN_CMD].Name = arDescStringIndex[(int)DESC_INDEX.FOREIGN_CMD];
            Columns[(int)DESC_INDEX.FOREIGN_CMD].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
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

        protected override void dgwAdminTable_CellValidated(object sender, DataGridViewCellEventArgs ev)
        {
            double value;
            bool valid;

            switch (ev.ColumnIndex)
            {
                case (int)DESC_INDEX.PLAN: // План
                    //cellValidated(ev.RowIndex, (int)DESC_INDEX.PLAN);

                    valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value, out value);
                    if ((valid == false) || (value > maxRecomendationValue))
                    {
                        //m_curRDGValues[ev.RowIndex].plan = 0;
                        Rows[ev.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value = 0.ToString("F2");
                    }
                    else
                    {
                        //m_curRDGValues[ev.RowIndex].plan = value;
                        Rows[ev.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value = value.ToString("F2");
                    }
                    break;
                case (int)DESC_INDEX.UDGe: //Не редактируется
                    break;
                case (int)DESC_INDEX.RECOMENDATION: // Рекомендация
                    {
                        //cellValidated(ev.RowIndex, (int)DESC_INDEX.RECOMENDATION);

                        valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value, out value);
                        if ((valid == false) || (value > maxRecomendationValue))
                        {
                            //m_curRDGValues[ev.RowIndex].recomendation = 0;
                            Rows[ev.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value = 0.ToString("F2");
                        }
                        else
                        {
                            //m_curRDGValues[ev.RowIndex].recomendation = value;
                            Rows[ev.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value = value.ToString("F2");

                            double prevPbr
                                , Pbr = double.Parse(Rows[ev.RowIndex].Cells[(int)DESC_INDEX.PLAN].Value.ToString ());
                            if (ev.RowIndex > 1)
                                prevPbr = double.Parse(Rows[ev.RowIndex - 1].Cells[(int)DESC_INDEX.PLAN].Value.ToString());
                            else
                                prevPbr = m_PBR_0;

                            Rows[ev.RowIndex].Cells[(int)DESC_INDEX.UDGe].Value = (((Pbr + prevPbr) / 2) + value).ToString("F2");
                        }
                        break;
                    }
                case (int)DESC_INDEX.FOREIGN_CMD:
                    bool fCmd = false;
                    try {
                        //fCmd = bool.Parse((string)Rows[ev.RowIndex].Cells[(int)DESC_INDEX.FOREIGN_CMD].Value);                        
                        //fCmd = (bool)Rows[ev.RowIndex].Cells[(int)DESC_INDEX.FOREIGN_CMD].Value;
                        fCmd = bool.Parse(Rows[ev.RowIndex].Cells[(int)DESC_INDEX.FOREIGN_CMD].Value.ToString());
                    }
                    catch (Exception e) {
                        Logging.Logg().Exception(e, @"DataGridViewAdminKomDisp::CellValidate () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                    }
                    valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value, out value);
                    if ((valid == false) /*|| (value == 0F)*/ || (value > maxRecomendationValue))
                        fCmd = false;
                    else
                        ;
                    Rows[ev.RowIndex].Cells[(int)DESC_INDEX.FOREIGN_CMD].Value = fCmd;
                    break;
                case (int)DESC_INDEX.DEVIATION_TYPE:
                    {
                        //m_curRDGValues[ev.RowIndex].deviationPercent = bool.Parse(this.dgwAdminTable.Rows[ev.RowIndex].Cells[(int)DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                        break;
                    }
                case (int)DESC_INDEX.DEVIATION: // Максимальное отклонение
                    {
                        valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)DESC_INDEX.DEVIATION].Value, out value);
                        bool isPercent = bool.Parse(Rows[ev.RowIndex].Cells[(int)DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                        double maxValue = -1F;

                        if (isPercent == true)
                            maxValue = maxDeviationPercentValue;
                        else
                            maxValue = maxDeviationValue; // вообще эти значения не суммируются, но для максимальной границы нормально

                        if ((valid == false) || (value < 0) || (value > maxValue))
                        {
                            //m_curRDGValues[ev.RowIndex].deviation = 0;
                            Rows[ev.RowIndex].Cells[(int)DESC_INDEX.DEVIATION].Value = 0.ToString("F2");
                        }
                        else
                        {
                            //m_curRDGValues[ev.RowIndex].deviation = value;
                            Rows[ev.RowIndex].Cells[(int)DESC_INDEX.DEVIATION].Value = value.ToString("F2");
                        }

                        //Зачем преобразование еще и этой ячейки ???
                        //double recom = -1F;
                        //valid = double.TryParse((string)Rows[ev.RowIndex].Cells[(int)DESC_INDEX.RECOMENDATION].Value, out recom);
                    }
                    break;
            }
        }

        public override void ClearTables()
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                for (int j = (int)DESC_INDEX.DATE_HOUR; j < ((int)DESC_INDEX.TO_ALL + 0); j++)
                {
                    Rows[i].Cells[j].Value = arDefaultValueIndex[j];
                }
            }
        }

        private void dgwAdminTable_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case (int)DESC_INDEX.DATE_HOUR:
                case (int)DESC_INDEX.PLAN:
                case (int)DESC_INDEX.UDGe:
                    Cursor = Cursors.Help;
                    break;
                case (int)DESC_INDEX.RECOMENDATION:
                case (int)DESC_INDEX.DEVIATION:
                    Cursor = Cursors.IBeam;
                    break;
                case (int)DESC_INDEX.FOREIGN_CMD:
                case (int)DESC_INDEX.DEVIATION_TYPE:
                case (int)DESC_INDEX.TO_ALL:
                    Cursor = Cursors.Hand;
                    break;
                default:
                    Cursor = Cursors.Default;
                    break;
            }
        }
    }
}
