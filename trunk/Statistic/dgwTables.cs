using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Globalization;

namespace StatisticCommon
{
    public abstract class DataGridViewHours : DataGridView
    {

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

        public DataGridViewHours()
        {
            InitializeComponents ();

            Name = "m_dgwTableHours";
            RowHeadersVisible = false;
            RowTemplate.Resizable = DataGridViewTriState.False;

            RowsAdd ();
        }

        protected void RowsAdd () { Rows.Add(25); }
    }

    public abstract class DataGridViewMins : DataGridView
    {

        protected virtual void InitializeComponents()
        {
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

        public DataGridViewMins()
        {
            InitializeComponents();

            Name = "m_dgwTableMins";
            RowHeadersVisible = false;
            RowTemplate.Resizable = DataGridViewTriState.False;

            RowsAdd();
        }

        protected void RowsAdd() { Rows.Add(21); }
    }
}
