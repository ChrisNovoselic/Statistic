using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Globalization;


using StatisticCommon;

namespace Statistic
{
    /// <summary>
    /// ����������� ����� HDataGridViewBase "�������� ��� ��������� ������"
    /// </summary>
    public abstract class HDataGridViewBase : HDataGridViewTables
    {
        ASUTP.Core.HDateTime.INTERVAL m_IdInterval;

        public class DataValuesEventArgs : EventArgs
        {
            public double m_value1;
            public double m_value2;
        }

        public void PerformDataValues (DataValuesEventArgs ev)
        {
            EventDataValues(ev);
        }
        public delegate void DataValuesEventHandler(DataValuesEventArgs ev);

        public event DataValuesEventHandler EventDataValues;

        /// <summary>
        /// ����� "�������� �������"
        /// </summary>
        public class ColumnProperies
        {
            /// <summary>
            /// ����������� ������
            /// </summary>
            public int minWidth;
            public int widthPerc;
            /// <summary>
            /// ����� ���������
            /// </summary>
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

        protected ColumnProperies[] m_arColumns;
        //protected int m_iWIdthDefault;

        protected void setFirstDisplayedScrollingRowIndex(int lastIndx, bool bScrollingRowIndex)
        {//����� ������ ��� ������� � �������� ����������...
            int iFirstDisplayedScrollingRowIndex = -1;

            if (lastIndx < DisplayedRowCount(true))
            {
                iFirstDisplayedScrollingRowIndex = 0;
            }
            else
            {
                iFirstDisplayedScrollingRowIndex = lastIndx - DisplayedRowCount(true) + 1;

                if (bScrollingRowIndex == true)
                    //���� ������������ ��� ���� ������ ���...
                    iFirstDisplayedScrollingRowIndex++;
                else
                    ;
            }
            FirstDisplayedScrollingRowIndex = iFirstDisplayedScrollingRowIndex;
        }

        //protected DataGridViewTables (int [] arWidthColiumns) {
        protected HDataGridViewBase(ASUTP.Core.HDateTime.INTERVAL interval, ColumnProperies[] arColuumns, bool bIsItogo)
            : base (new Color [] { FormMain.formGraphicsSettings.BackgroundColor                                                                    // , ����� ���������� ���� ��������� �������
                    , Color.Yellow
                    , FormMain.formGraphicsSettings.COLOR (FormGraphicsSettings.INDEX_COLOR.DIVIATION) }
                , bIsItogo)
        {
            m_IdInterval = interval;

            //m_arColumns = new ColumnProperies[arWidthColiumns.Length];
            m_arColumns = new ColumnProperies[arColuumns.Length];

            int i = -1;
            //m_iWIdthDefault = 0;
            for (i = 0; i < m_arColumns.Length; i++)
            {
                m_arColumns[i] = arColuumns[i]; // new ColumnProperies();

                m_arColumns[i].obj = new DataGridViewTextBoxColumn();

                //������� �1
                //m_arColumns[i].width = arWidthColiumns[i];
                //m_iWIdthDefault += m_arColumns[i].width;

                ////������� �2
                //m_arColumns[i].widthPerc = arWidthPercColiumns[i];

                ////������� �3
                //m_arColumns[i].minWidth = arColuumns[i].minWidth;
                //m_arColumns[i].widthPerc = arColuumns[i].widthPerc;
                //m_arColumns[i].headerText = arColuumns[i].headerText;
                //m_arColumns[i].name = arColuumns[i].name;
            }

            InitializeComponents();

            this.ClientSizeChanged += new EventHandler(DataGridViewTables_ClientSizeChanged);

            //this.FirstDisplayedScrollingRowIndex = 0;
            //this.DisplayedRowCount (true);
        }

        protected void DataGridViewTables_ClientSizeChanged(object obj_, EventArgs ev_)
        {
            int csWidth = this.ClientSize.Width - this.VerticalScrollBar.Width
                , i = -1 //����
                , avgPercAdding = 0 //��������, �������, ��� ���������� � ������ ���������� ������������ ��������
                , width = -1;

            //�������� ������� ����./���� ����������� ��������
            if (!(this.ContextMenuStrip == null))
            {
                int freePerc = 0; //��������, �� ������������ ��������

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

        private void InitializeComponents()
        {
            int i = -1;

            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();

            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) |
                                                            System.Windows.Forms.AnchorStyles.Left)));
            dataGridViewCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle.BackColor = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON].BackColor; // System.Drawing.SystemColors.Window;
            dataGridViewCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;

            //ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
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
            // dgwMins
            // 
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            //this.dgwMins.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            //            | System.Windows.Forms.AnchorStyles.Left)));
            this.Dock = DockStyle.Fill;
            this.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            System.Windows.Forms.DataGridViewColumn[] arDataGridViewColumns = new DataGridViewColumn[m_arColumns.Length];
            for (i = 0; i < m_arColumns.Length; i++)
                arDataGridViewColumns[i] = this.m_arColumns[i].obj;
            this.Columns.AddRange(
                //new System.Windows.Forms.DataGridViewColumn[] {
                //this.m_arColumns [(int)INDEX_COLUMNS.PART_TIME].obj,
                //this.m_arColumns [(int)INDEX_COLUMNS.FACT].obj,
                //this.m_arColumns [(int)INDEX_COLUMNS.PBR].obj,
                //this.m_arColumns [(int)INDEX_COLUMNS.PBRe].obj,
                //this.m_arColumns [(int)INDEX_COLUMNS.UDGe].obj,
                //this.m_arColumns [(int)INDEX_COLUMNS.DEVIATION].obj
                //}
                arDataGridViewColumns
            );
            //this.dgwMins.Location = arPlacement[(int)CONTROLS.dgwMins].pt;
            //this.Name = "dgwMin";
            this.ReadOnly = true;
            this.RowHeadersVisible = false;
            //this.dgwMins.Size = arPlacement[(int)CONTROLS.dgwMins].sz;
            this.TabIndex = 0;
            this.RowTemplate.Resizable = DataGridViewTriState.False;
        }

        protected void RowsAdd()
        {
            switch (m_IdInterval)
            {
                case ASUTP.Core.HDateTime.INTERVAL.HOURS:
                    Rows.Add(24 + (_bIsItogo == true ? 1 : 0));
                    break;
                case ASUTP.Core.HDateTime.INTERVAL.MINUTES:
                    Rows.Add(20 + (_bIsItogo == true ? 1 : 0));
                    break;
                default:
                    throw new Exception(@"HDataGridViewBase::RowsAdd () - ����������� ��� ���������");
            }
        }

        public abstract void Fill(TecView.valuesTEC[] values, params object[] pars);

        public virtual void Fill(params object[] pars)
        {
            for (int i = 0; i < Rows.Count; i++)
                for (int c = 0; c < Columns.Count; c++) {
                    Rows [i].Cells [c].Value = string.Empty;
                    Rows [i].Cells [c].Style.BackColor = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON].BackColor;
                }
        }
    }

    public abstract class HDataGridViewStandard : HDataGridViewBase
    {
        public enum INDEX_COLUMNS : int { PART_TIME, FACT, PBR, PBRe, UDGe, DEVIATION, LAST_MINUTES
            , COUNT_INDEX_COLUMNS };

        public HDataGridViewStandard(ASUTP.Core.HDateTime.INTERVAL interval, ColumnProperies[] arColumns, bool bIsItogo)
            : base(interval, arColumns, bIsItogo)
        {
        }
    }

    public class DataGridViewStandardHours : HDataGridViewStandard
    {       
        private void InitializeComponents () {
            int i = -1;

            this.ContextMenuStrip = new ContextMenuStrip ();
            for (i = 0; i < m_arColumns.Length - 1; i++)
            {
                this.ContextMenuStrip.Items.Add(this.m_arColumns[i].headerText);
                this.ContextMenuStrip.Items[(int)this.ContextMenuStrip.Items.Count - 1].Enabled = false;
                ((ToolStripMenuItem)this.ContextMenuStrip.Items[(int)this.ContextMenuStrip.Items.Count - 1]).CheckState = CheckState.Checked;
            }

            //��� �������� ������� "59���"
            this.ContextMenuStrip.Items.Add(this.m_arColumns[i].headerText);
            this.ContextMenuStrip.Items[(int)this.ContextMenuStrip.Items.Count - 1].Enabled = true;
            CheckState chState = CheckState.Unchecked;
            if (HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUCONTEXTITEM_TABLEHOURS_COLUMN_59MIN) == true)
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
        public DataGridViewStandardHours()
            //: base(new int[] { 8, 15, 15, 15, 15, 15, 15 })
            : base(ASUTP.Core.HDateTime.INTERVAL.HOURS
                , new ColumnProperies[] { new ColumnProperies (27, 8, @"���", @"Hour")
                    , new ColumnProperies (47, 15, @"����", @"FactHour")
                    , new ColumnProperies (47, 15, @"���", @"PBRHour")
                    , new ColumnProperies (47, 15, @"����", @"PBReHour")
                    , new ColumnProperies (47, 15, @"����", @"UDGeHour")
                    , new ColumnProperies (42, 15, @"+/-", @"DeviationHour")
                    , new ColumnProperies (46, 15, @"59���", @"")
            }, true)
        {
            InitializeComponents ();

            Name = "m_dgwTableHours";
            RowHeadersVisible = false;
            RowTemplate.Resizable = DataGridViewTriState.False;

            RowsAdd ();
        }

        public override void Fill(TecView.valuesTEC[] values, params object []pars)
        {
            double sumFact = 0, sumUDGe = 0, sumDiviation = 0;
            Hd2PercentControl d2PercentControl = new Hd2PercentControl();
            int lastHour = (int)pars[0]; //m_tecView.lastHour;
            int receivedHour = (int)pars[1]; //m_tecView.lastReceivedHour;
            int itemscount = (int)pars[2]; //m_tecView.m_valuesHours.Length;
            int warn = -1,
                cntWarn = -1;
            string strWarn = string.Empty;
            bool bPmin = (int)pars[3] == 5
                , bCurrHour = (bool)pars[4] //m_tecView.currHour
                , bIsTypeConnSettAISKUEHour = (bool)pars[5]; //m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_AISKUE
            DateTime serverTime = (DateTime)pars[6]; //m_tecView.serverTime

            DataGridViewCellStyle curCellStyle;
            cntWarn = 0;
            for (int i = 0; i < itemscount; i++)
            {
                d2PercentControl.Calculate(values[i], bPmin, out warn);

                if ((!(warn == 0)) &&
                   (values[i + 0].valuesLastMinutesTM > 1))
                    cntWarn++;
                else
                    cntWarn = 0;

                if (!(cntWarn == 0))
                {
                    if (cntWarn > 3)
                        curCellStyle = s_dgvCellStyles[(int)INDEX_CELL_STYLE.ERROR];
                    else
                        curCellStyle = s_dgvCellStyles[(int)INDEX_CELL_STYLE.WARNING];
                }
                else
                    curCellStyle = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON];
                Rows[i + 0].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.LAST_MINUTES].Style = curCellStyle;

                if (values[i + 0].valuesLastMinutesTM > 1)
                {
                    if (cntWarn > 0)
                        strWarn = cntWarn + @":";
                    else
                        strWarn = string.Empty;

                    Rows[i + 0].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.LAST_MINUTES].Value = strWarn + values[i + 0].valuesLastMinutesTM.ToString("F2");
                }
                else
                    Rows[i + 0].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.LAST_MINUTES].Value = 0.ToString("F2");

                bool bDevVal = false;
                if (bCurrHour == true)
                    if ((i < (receivedHour + 1)) && ((!(values[i].valuesUDGe == 0)) && (values[i].valuesFact > 0)))
                    {
                        if ((bIsTypeConnSettAISKUEHour == true)
                            || (i < receivedHour))
                            bDevVal = true;
                        else
                            ;
                    }
                    else
                    {
                    }
                else
                    if (serverTime.Date.Equals(ASUTP.Core.HDateTime.ToMoscowTimeZone(DateTime.Now.Date)) == true)
                        if ((i < (receivedHour + 1)) && (!(values[i].valuesUDGe == 0)) && (values[i].valuesFact > 0))
                        {
                            bDevVal = true;
                        }
                        else
                        {
                        }
                    else
                        if ((!(values[i].valuesUDGe == 0)) && (values[i].valuesFact > 0))
                        {
                            bDevVal = true;
                        }
                        else
                        {
                        }

                Rows[i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.FACT].Value = values[i].valuesFact.ToString("F2");
                if (bDevVal == true)
                    sumFact += values[i].valuesFact;
                else
                    ;

                Rows [i].Cells [(int)DataGridViewStandardHours.INDEX_COLUMNS.PART_TIME].Style = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON];
                Rows [i].Cells [(int)DataGridViewStandardHours.INDEX_COLUMNS.FACT].Style = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON];

                Rows [i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.PBR].Value = values[i].valuesPBR.ToString("F2");
                Rows [i].Cells [(int)DataGridViewStandardHours.INDEX_COLUMNS.PBR].Style = s_dgvCellStyles[(int)INDEX_CELL_STYLE.COMMON];
                Rows [i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.PBRe].Value = values[i].valuesPBRe.ToString("F2");
                Rows [i].Cells [(int)DataGridViewStandardHours.INDEX_COLUMNS.PBRe].Style = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON];
                Rows [i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.UDGe].Value = values[i].valuesUDGe.ToString("F2");
                Rows [i].Cells [(int)DataGridViewStandardHours.INDEX_COLUMNS.UDGe].Style = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON];
                sumUDGe += values[i].valuesUDGe;

                if (bDevVal == true)
                {
                    Rows[i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.DEVIATION].Value = ((double)(values[i].valuesFact - values[i].valuesUDGe)).ToString("F2");
                    if ((Math.Round (Math.Abs (values [i].valuesFact - values [i].valuesUDGe), 2) > Math.Round (values [i].valuesDiviation, 2))
                        && (!(values [i].valuesDiviation == 0))) {
                        //s_dgvCellStyles[(int)INDEX_CELL_STYLE.ERROR].BackColor = FormMain.formGraphicsSettings.COLOR (FormGraphicsSettings.INDEX_COLOR.DIVIATION);
                        Rows [i].Cells [(int)DataGridViewStandardHours.INDEX_COLUMNS.DEVIATION].Style = s_dgvCellStyles[(int)INDEX_CELL_STYLE.ERROR];
                    } else
                        Rows [i].Cells [(int)DataGridViewStandardHours.INDEX_COLUMNS.DEVIATION].Style = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON];
                    sumDiviation += Math.Abs(values[i].valuesFact - values[i].valuesUDGe);
                }
                else
                {
                    Rows[i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.DEVIATION].Value = 0.ToString("F2");
                    Rows[i].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.DEVIATION].Style = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON];
                }

            }

            Rows [itemscount].Cells [(int)DataGridViewStandardHours.INDEX_COLUMNS.PART_TIME].Style = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON];
            Rows [itemscount].Cells [(int)DataGridViewStandardHours.INDEX_COLUMNS.PBR].Style = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON];
            Rows [itemscount].Cells [(int)DataGridViewStandardHours.INDEX_COLUMNS.PBRe].Style = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON];
            Rows [itemscount].Cells [(int)DataGridViewStandardHours.INDEX_COLUMNS.LAST_MINUTES].Style = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON];

            Rows [itemscount].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.FACT].Value = sumFact.ToString("F2");
            Rows [itemscount].Cells [(int)DataGridViewStandardHours.INDEX_COLUMNS.FACT].Style = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON];
            Rows [itemscount].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.UDGe].Value = sumUDGe.ToString("F2");
            Rows [itemscount].Cells [(int)DataGridViewStandardHours.INDEX_COLUMNS.UDGe].Style = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON];
            Rows [itemscount].Cells[(int)DataGridViewStandardHours.INDEX_COLUMNS.DEVIATION].Value = sumDiviation.ToString("F2");
            Rows [itemscount].Cells [(int)DataGridViewStandardHours.INDEX_COLUMNS.DEVIATION].Style = s_dgvCellStyles [(int)INDEX_CELL_STYLE.COMMON];            

            setFirstDisplayedScrollingRowIndex (lastHour, !bIsTypeConnSettAISKUEHour);
        }
        public override void Fill(params object []pars)
        {
            int count = (int)pars[1]
                , hour = -1
                , offset = -1
                , i = -1, c = -1;
            DateTime dtCurrent = (DateTime)pars[0];
            bool bSeasonDate = (bool)pars[2];

            Rows.Clear();

            //Rows.Add(count + 1);
            RowsAdd();

            for (i = 0; i < count; i++)
            {
                hour = i + 1;
                if (bSeasonDate == true)
                {
                    offset = HAdmin.GetSeasonHourOffset(dtCurrent, hour);

                    Rows[i].Cells[(int)INDEX_COLUMNS.PART_TIME].Value = (hour - offset).ToString();
                    if ((hour - 1) == HAdmin.SeasonDateTime.Hour)
                        Rows[i].Cells[0].Value += @"*";
                    else
                        ;
                }
                else
                    Rows[i].Cells[(int)INDEX_COLUMNS.PART_TIME].Value = (hour).ToString();

                for (c = 1; c < m_arColumns.Length; c ++)
                    Rows[i].Cells[c].Value = 0.ToString("F2");
            }

            Rows[count].Cells[0].Value = "�����";
            for (c = 1; c < m_arColumns.Length; c++)
                switch ((INDEX_COLUMNS)c)
                {
                    case INDEX_COLUMNS.PBR:
                    case INDEX_COLUMNS.PBRe:
                        Rows[i].Cells[c].Value = @"-".ToString();
                        break;
                    default:
                        Rows[i].Cells[c].Value = 0.ToString("F2");
                        break;
                }
        }
    }
    
  }
