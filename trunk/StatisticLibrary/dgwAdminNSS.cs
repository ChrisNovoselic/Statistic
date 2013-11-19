using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Drawing;

namespace StatisticCommon
{
    public class DataGridViewAdminNSS : DataGridViewAdmin
    {
        private enum ID_TYPE : ushort { ID, ID_OWNER, COUNT_ID_TYPE };

        private List <int []> m_listIds;

        DataGridViewCellStyle dgvCellStyleError,
                             dgvCellStyleGTP;
        
        protected override void InitializeComponents () {
            base.InitializeComponents ();

            int col = -1;
            Columns.AddRange(new DataGridViewColumn[2] { new DataGridViewTextBoxColumn(), new DataGridViewButtonColumn() });
            col = 0;
            // 
            // DateHour
            // 
            Columns[col].Frozen = true;
            Columns[col].HeaderText = "Дата, час";
            Columns[col].Name = "DateHour";
            Columns[col].ReadOnly = true;
            Columns[col].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;

            col = Columns.Count -1;
            Columns[col].Frozen = false;
            Columns[col].HeaderText = "Для всех";
            Columns[col].Name = "ToAll";
            Columns[col].ReadOnly = true;
            Columns[col].SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
        }

        public DataGridViewAdminNSS () {
            m_listIds = new List<int []> ();

            dgvCellStyleError =
            dgvCellStyleGTP = new DataGridViewCellStyle();

            dgvCellStyleError.BackColor = Color.Red;
            dgvCellStyleGTP.BackColor = Color.Yellow;

            Anchor |= (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Right);
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

        public void addTextBoxColumn(string name, int id, int id_owner, DateTime date)
        {
            DataGridViewTextBoxColumn insColumn = new DataGridViewTextBoxColumn ();
            insColumn.Frozen = false;
            insColumn.HeaderText = name;
            insColumn.Name = "column" + (Columns.Count - 1);
            insColumn.ReadOnly = false;
            insColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            Columns.Insert(Columns.Count - 1, insColumn);

            m_listIds.Add (new int [(int)ID_TYPE.COUNT_ID_TYPE] {id, id_owner});

            if (id_owner < 0) {
                Columns [Columns.Count - 1 - 1].ReadOnly = true;
                Columns [Columns.Count - 1 - 1].DefaultCellStyle = dgvCellStyleGTP;
            }
            else
                ;
        }

        public void ClearTables () {
            int i = -1;

            m_listIds.Clear ();
            
            while (Columns.Count > 2)
            {
                Columns.RemoveAt(Columns.Count - 1 - 1);
            }            
            
            for (i = 0; i < 24; i++)
            {
                Rows[i].Cells[0].Value = string.Empty;
            }
        }
    }
}
