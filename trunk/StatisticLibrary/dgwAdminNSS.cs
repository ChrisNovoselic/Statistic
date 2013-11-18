using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Globalization;

namespace StatisticCommon
{
    public class DataGridViewAdminNSS : DataGridViewAdmin
    {
        protected override void InitializeComponents () {
            base.InitializeComponents ();

            Columns.AddRange(new DataGridViewColumn[1] {new DataGridViewTextBoxColumn ()});
            // 
            // DateHour
            // 
            Columns[0].Frozen = true;
            Columns[0].HeaderText = "Дата, час";
            Columns[0].Name = "DateHour";
            Columns[0].ReadOnly = true;
            Columns[0].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
        }

        public DataGridViewAdminNSS () {
        }

        protected override void dgwAdminTable_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            double value;
            bool valid;

            switch (e.ColumnIndex)
            {
                default:
                    break;
            }
        }
    }
}
