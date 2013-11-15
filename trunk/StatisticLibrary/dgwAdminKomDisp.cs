using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Globalization;

namespace StatisticCommon
{
    public class DataGridViewAdminKomDisp : DataGridViewAdmin
    {
        public enum DESC_INDEX : ushort { DATE_HOUR, PLAN, RECOMENDATION, DEVIATION_TYPE, DEVIATION, TO_ALL, COUNT_COLUMN };
        private static string[] arDescStringIndex = { "DateHour", "Plan", "Recomendation", "DeviationType", "Deviation", "ToAll" };
        private static string[] arDescRusStringIndex = { "Дата, час", "План", "Рекомендация", "Отклонение в процентах", "Величина максимального отклонения", "Дозаполнить" };
        
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
        }
        
        public DataGridViewAdminKomDisp () {
        }
    }
}
