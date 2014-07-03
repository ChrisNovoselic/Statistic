using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Globalization;

namespace StatisticCommon
{
    public class DataGridViewTables : DataGridView
    {
        protected class ColumnProperies {
            public int width;
            public float widthRel;
            public string headerText;
            public string name;

            public DataGridViewTextBoxColumn obj;
        };
        
        protected enum INDEX_COLUMNS : int { PART_TIME, FACT, PBR, PBRe, UDGe, DEVIATION, LAST_MINUTES, COUNT_INDEX_COLUMNS };
        protected ColumnProperies [] m_arColumns;
        protected int m_iWIdthDefault;

        protected DataGridViewTables (int [] arWidthColiumns) {
            m_arColumns = new ColumnProperies[arWidthColiumns.Length];

            int i = -1;
            m_iWIdthDefault = 0;
            for (i = 0; i < m_arColumns.Length; i++)
            {
                m_arColumns[i] = new ColumnProperies ();

                m_arColumns[i].obj = new DataGridViewTextBoxColumn();

                m_arColumns[i].width = arWidthColiumns[i];
                m_iWIdthDefault += m_arColumns[i].width;
            }

            for (i = 0; i < m_arColumns.Length; i++)
            {
                m_arColumns[i].widthRel = (float)m_arColumns[i].width / m_iWIdthDefault;
            }

            this.ClientSizeChanged += new EventHandler(DataGridViewTables_ClientSizeChanged);
        }

        private void DataGridViewTables_ClientSizeChanged(object obj, EventArgs ev)
        {
            bool bWidthDefault = true;
            if (((DataGridViewTables)obj).ClientSize.Width > (m_iWIdthDefault + 19)) {
                bWidthDefault = false;
            }
            else {
            }

            for (int i = 0; i < m_arColumns.Length; i ++) {
                if (bWidthDefault == true) {
                    m_arColumns [i].obj.Width = m_arColumns [i].width;
                }
                else {
                    m_arColumns[i].obj.Width = (int)Math.Ceiling (m_arColumns [i].widthRel * ((DataGridViewTables)obj).ClientSize.Width);
                }
            }
        }
    }

    public class DataGridViewHours : DataGridViewTables
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

            for (int i = 0; i < this.m_arColumns.Length; i++)
            {
                this.m_arColumns[i].obj.HeaderText = m_arColumns[i].headerText;
                this.m_arColumns[i].obj.Name = m_arColumns[i].name;
                this.m_arColumns[i].obj.ReadOnly = true;
                this.m_arColumns[i].obj.Width = m_arColumns[i].width;
                this.m_arColumns[i].obj.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            }

            // 
            // dgwHours
            // 
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            //this.dgwHours.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            //this.dgwHours.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            //            | System.Windows.Forms.AnchorStyles.Left)));
            this.Dock = DockStyle.Fill;
            this.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.m_arColumns[(int)INDEX_COLUMNS.PART_TIME].obj,
            this.m_arColumns[(int)INDEX_COLUMNS.FACT].obj,
            this.m_arColumns[(int)INDEX_COLUMNS.PBR].obj,
            this.m_arColumns[(int)INDEX_COLUMNS.PBRe].obj,
            this.m_arColumns[(int)INDEX_COLUMNS.UDGe].obj,
            this.m_arColumns[(int)INDEX_COLUMNS.DEVIATION].obj,
            this.m_arColumns[(int)INDEX_COLUMNS.LAST_MINUTES].obj});
            //this.dgwHours.Location = arPlacement[(int)CONTROLS.dgwHours].pt;
            this.Name = "dgwHour";
            this.ReadOnly = true;
            this.RowHeadersVisible = false;
            //this.dgwHours.Size = arPlacement[(int)CONTROLS.dgwHours].sz;
            this.TabIndex = 7;
            this.RowTemplate.Resizable = DataGridViewTriState.False;
        }

        public DataGridViewHours() : base (new int [] {25, 48, 48, 49, 49, 42, 49})
        {
            m_arColumns[(int)INDEX_COLUMNS.PART_TIME].headerText = @"×àñ"; m_arColumns[(int)INDEX_COLUMNS.PART_TIME].name = @"Hour";
            m_arColumns[(int)INDEX_COLUMNS.FACT].headerText = @"Ôàêò"; m_arColumns[(int)INDEX_COLUMNS.FACT].name = @"FactHour";
            m_arColumns[(int)INDEX_COLUMNS.PBR].headerText = @"ÏÁÐ"; m_arColumns[(int)INDEX_COLUMNS.PBR].name = @"PBRHour";
            m_arColumns[(int)INDEX_COLUMNS.PBRe].headerText = @"ÏÁÐý"; m_arColumns[(int)INDEX_COLUMNS.PBRe].name = @"PBReHour";
            m_arColumns[(int)INDEX_COLUMNS.UDGe].headerText = @"ÓÄÃý"; m_arColumns[(int)INDEX_COLUMNS.UDGe].name = @"UDGeHour";
            m_arColumns[(int)INDEX_COLUMNS.DEVIATION].headerText = @"+/-"; m_arColumns[(int)INDEX_COLUMNS.DEVIATION].name = @"DeviationHour";
            m_arColumns[(int)INDEX_COLUMNS.LAST_MINUTES].headerText = @"59ìèí"; m_arColumns[(int)INDEX_COLUMNS.LAST_MINUTES].name = @"";
            
            InitializeComponents ();

            Name = "m_dgwTableHours";
            RowHeadersVisible = false;
            RowTemplate.Resizable = DataGridViewTriState.False;

            RowsAdd ();
        }

        protected void RowsAdd () { Rows.Add(25); }
    }

    public class DataGridViewMins : DataGridViewTables
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

            for (int i = 0; i < this.m_arColumns.Length; i ++) {
                this.m_arColumns[i].obj.HeaderText = m_arColumns[i].headerText;
                this.m_arColumns[i].obj.Name = m_arColumns[i].name;
                this.m_arColumns[i].obj.ReadOnly = true;
                this.m_arColumns[i].obj.Width = m_arColumns[i].width;
                this.m_arColumns[i].obj.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            }            

            // 
            // dgwMins
            // 
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            //this.dgwMins.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            //            | System.Windows.Forms.AnchorStyles.Left)));
            this.Dock = DockStyle.Fill;
            this.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.m_arColumns [(int)INDEX_COLUMNS.PART_TIME].obj,
            this.m_arColumns [(int)INDEX_COLUMNS.FACT].obj,
            this.m_arColumns [(int)INDEX_COLUMNS.PBR].obj,
            this.m_arColumns [(int)INDEX_COLUMNS.PBRe].obj,
            this.m_arColumns [(int)INDEX_COLUMNS.UDGe].obj,
            this.m_arColumns [(int)INDEX_COLUMNS.DEVIATION].obj});
            //this.dgwMins.Location = arPlacement[(int)CONTROLS.dgwMins].pt;
            this.Name = "dgwMin";
            this.ReadOnly = true;
            this.RowHeadersVisible = false;
            //this.dgwMins.Size = arPlacement[(int)CONTROLS.dgwMins].sz;
            this.TabIndex = 0;
            this.RowTemplate.Resizable = DataGridViewTriState.False;
        }

        public DataGridViewMins() : base (new int [] {50, 50, 50, 50, 60, 50})
        {
            m_arColumns[(int)INDEX_COLUMNS.PART_TIME].headerText = @"Ìèí."; m_arColumns[(int)INDEX_COLUMNS.PART_TIME].name = @"Min";
            m_arColumns[(int)INDEX_COLUMNS.FACT].headerText = @"Ôàêò"; m_arColumns[(int)INDEX_COLUMNS.FACT].name = @"FactMin";
            m_arColumns[(int)INDEX_COLUMNS.PBR].headerText = @"ÏÁÐ"; m_arColumns[(int)INDEX_COLUMNS.PBR].name = @"PBRMin";
            m_arColumns[(int)INDEX_COLUMNS.PBRe].headerText = @"ÏÁÐý"; m_arColumns[(int)INDEX_COLUMNS.PBRe].name = @"PBReMin";
            m_arColumns[(int)INDEX_COLUMNS.UDGe].headerText = @"ÓÄÃý"; m_arColumns[(int)INDEX_COLUMNS.UDGe].name = @"UDGeMin";
            m_arColumns[(int)INDEX_COLUMNS.DEVIATION].headerText = @"+/-"; m_arColumns[(int)INDEX_COLUMNS.DEVIATION].name = @"DeviationMin";
            
            InitializeComponents();

            Name = "m_dgwTableMins";
            RowHeadersVisible = false;
            RowTemplate.Resizable = DataGridViewTriState.False;

            RowsAdd();
        }

        protected void RowsAdd() { Rows.Add(21); }
    }
}
