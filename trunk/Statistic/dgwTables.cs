using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
using System.Globalization;

using StatisticCommon;

namespace Statistic
{
    public class DataGridViewTables : DataGridViewBase
    {
        protected class ColumnProperies {
            public int minWidth;
            public int widthPerc;
            public string headerText;
            public string name;

            public DataGridViewTextBoxColumn obj;

            public ColumnProperies(int mw, int wp, string ht, string n)
            {
                minWidth = mw;
                widthPerc = wp;
                headerText = ht;
                name = n;
            }
        };

        public enum INDEX_COLUMNS : int { PART_TIME, FACT, PBR, PBRe, UDGe, DEVIATION, LAST_MINUTES, COUNT_INDEX_COLUMNS };
        protected ColumnProperies [] m_arColumns;
        //protected int m_iWIdthDefault;

        //protected DataGridViewTables (int [] arWidthColiumns) {
        protected DataGridViewTables(ColumnProperies[] arColuumns)
        {
            //m_arColumns = new ColumnProperies[arWidthColiumns.Length];
            m_arColumns = new ColumnProperies[arColuumns.Length];

            int i = -1;
            //m_iWIdthDefault = 0;
            for (i = 0; i < m_arColumns.Length; i++)
            {
                m_arColumns[i] = arColuumns[i]; // new ColumnProperies();

                m_arColumns[i].obj = new DataGridViewTextBoxColumn();

                //Вариант №1
                //m_arColumns[i].width = arWidthColiumns[i];
                //m_iWIdthDefault += m_arColumns[i].width;

                ////Вариант №2
                //m_arColumns[i].widthPerc = arWidthPercColiumns[i];

                ////Вариант №3
                //m_arColumns[i].minWidth = arColuumns[i].minWidth;
                //m_arColumns[i].widthPerc = arColuumns[i].widthPerc;
                //m_arColumns[i].headerText = arColuumns[i].headerText;
                //m_arColumns[i].name = arColuumns[i].name;
            }

            this.ClientSizeChanged += new EventHandler(DataGridViewTables_ClientSizeChanged);

            //this.FirstDisplayedScrollingRowIndex = 0;
            //this.DisplayedRowCount (true);
        }

        protected void DataGridViewTables_ClientSizeChanged(object obj_, EventArgs ev_)
        {
            int csWidth = this.ClientSize.Width - this.VerticalScrollBar.Width
                , i = -1 //цикл
                , avgPercAdding = 0 //проценты, среднее, для добавления к ширине оставшимся отображаемым столбцам
                , width = -1;

            //Проверка наличия конт./меню отображения столбцов
            if (!(this.ContextMenuStrip == null))
            {
                int freePerc = 0; //проценты, не отображаемых столбцов

                List<int> listNumVisibleColumns = new List<int>();

                for (i = 0; i < this.ContextMenuStrip.Items.Count /*m_arColumns.Length*/; i++)
                {
                    if (((ToolStripMenuItem)this.ContextMenuStrip.Items[i]).CheckState == CheckState.Checked)
                        listNumVisibleColumns.Add(i);
                    else
                        freePerc += m_arColumns[i].widthPerc;
                }

                if (this.ContextMenuStrip.Items.Count > listNumVisibleColumns.Count)
                    //avgPercAdding = (int)Math.Ceiling((double)freePerc / (this.ContextMenuStrip.Items.Count - listNumVisibleColumns.Count));
                    avgPercAdding = (int)Math.Ceiling((double)freePerc / listNumVisibleColumns.Count);
                else
                    ;
            }
            else
            {
            }

            for (i = 0; i < m_arColumns.Length; i++)
            {
                width = (int)Math.Ceiling((double)(m_arColumns[i].widthPerc + avgPercAdding) / 100 * csWidth);
                if (width < m_arColumns[i].minWidth)
                    width = m_arColumns[i].minWidth;
                else
                    ;

                m_arColumns[i].obj.Width = width;
            }
        }
    }

    public class DataGridViewHours : DataGridViewTables
    {
        protected virtual void InitializeComponents () {
            int i = -1;

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

            for (i = 0; i < this.m_arColumns.Length; i++)
            {
                this.m_arColumns[i].obj.HeaderText = m_arColumns[i].headerText;
                this.m_arColumns[i].obj.Name = m_arColumns[i].name;
                this.m_arColumns[i].obj.ReadOnly = true;
                //this.m_arColumns[i].obj.Width = m_arColumns[i].width;
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

            this.ContextMenuStrip = new ContextMenuStrip ();
            for (i = 0; i < m_arColumns.Length - 1; i++)
            {
                this.ContextMenuStrip.Items.Add(this.m_arColumns[i].headerText);
                this.ContextMenuStrip.Items[(int)this.ContextMenuStrip.Items.Count - 1].Enabled = false;
                ((ToolStripMenuItem)this.ContextMenuStrip.Items[(int)this.ContextMenuStrip.Items.Count - 1]).CheckState = CheckState.Checked;
            }

            //Для крайнено столбца "59мин"
            this.ContextMenuStrip.Items.Add(this.m_arColumns[i].headerText);
            this.ContextMenuStrip.Items[(int)this.ContextMenuStrip.Items.Count - 1].Enabled = true;
            CheckState chState = CheckState.Unchecked;
            if ((!(HStatisticUsers.Role == HStatisticUsers.ID_ROLES.NSS)) &&
                (!(HStatisticUsers.Role == HStatisticUsers.ID_ROLES.MAJOR_MASHINIST)) &&
                (!(HStatisticUsers.Role == HStatisticUsers.ID_ROLES.MASHINIST)))
                chState = CheckState.Checked;
            else
                ;
            ((ToolStripMenuItem)this.ContextMenuStrip.Items[(int)this.ContextMenuStrip.Items.Count - 1]).CheckState = chState;

            visibleColumns();

            this.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(DataGridViewHours_ContextMenuStrip_Click);
        }

        private void visibleColumns ()
        {
            for (int i = 0; i < this.ContextMenuStrip.Items.Count /*m_arColumns.Length*/; i++)
                this.m_arColumns [i].obj.Visible = ((ToolStripMenuItem)this.ContextMenuStrip.Items[i]).CheckState == CheckState.Checked;
        }

        void DataGridViewHours_ContextMenuStrip_Click(object sender, ToolStripItemClickedEventArgs e)
        {
            //throw new NotImplementedException();

            CheckState chState = ((ToolStripMenuItem)this.ContextMenuStrip.Items[this.ContextMenuStrip.Items.IndexOf((ToolStripMenuItem)e.ClickedItem)]).CheckState;
            if (chState == CheckState.Checked)
                chState = CheckState.Unchecked;
            else
                if (chState == CheckState.Unchecked)
                    chState = CheckState.Checked;
                else
                    ;
            ((ToolStripMenuItem)this.ContextMenuStrip.Items[this.ContextMenuStrip.Items.IndexOf((ToolStripMenuItem)e.ClickedItem)]).CheckState = chState;

            visibleColumns();

            DataGridViewTables_ClientSizeChanged(this, EventArgs.Empty);
        }

        //public DataGridViewHours() : base (new int [] {27, 47, 47, 47, 47, 42, 46})
        public DataGridViewHours()
            //: base(new int[] { 8, 15, 15, 15, 15, 15, 15 })
            : base(new ColumnProperies[] { new ColumnProperies (27, 8, @"Час", @"Hour")
                                            , new ColumnProperies (47, 15, @"Факт", @"FactHour")
                                            , new ColumnProperies (47, 15, @"ПБР", @"PBRHour")
                                            , new ColumnProperies (47, 15, @"ПБРэ", @"PBReHour")
                                            , new ColumnProperies (47, 15, @"УДГэ", @"UDGeHour")
                                            , new ColumnProperies (42, 15, @"+/-", @"DeviationHour")
                                            , new ColumnProperies (46, 15, @"59мин", @"")
            })
        {
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
                //this.m_arColumns[i].obj.Width = m_arColumns[i].width;
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

        public DataGridViewMins()
            //: base (new int [] {15, 16, 16, 16, 19, 16})
            : base(new ColumnProperies[] { new ColumnProperies (50, 15, @"Мин.", @"Min")
                                            , new ColumnProperies (50, 16, @"Факт", @"FactMin")
                                            , new ColumnProperies (50, 16, @"ПБР", @"PBRMin")
                                            , new ColumnProperies (50, 16, @"ПБРэ", @"PBReMin")
                                            , new ColumnProperies (50, 19, @"УДГэ", @"UDGeMin")
                                            , new ColumnProperies (50, 16, @"+/-", @"DeviationMin")
            })
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
