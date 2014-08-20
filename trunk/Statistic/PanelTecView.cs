using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading;
using System.Globalization;

using ZedGraph;
using GemBox.Spreadsheet;

using StatisticCommon;

namespace Statistic
{
    public class PanelTecViewTable : PanelTecViewBase
    {
        PanelCustomTecView.HLabelEmpty m_label;

        public PanelTecViewTable(StatisticCommon.TEC tec, int num_tec, int num_comp, StatusStrip sts, FormGraphicsSettings gs, FormParameters par, HReports rep, PanelCustomTecView.HLabelEmpty label)
            : base(tec, num_tec, num_comp, sts, gs, par, rep)
        {
            m_label = label;

            InitializeComponent ();
        }

        protected override void InitializeComponent () {
            base.InitializeComponent ();

            stctrView.Panel1.Controls.Add(m_dgwMins);
            stctrView.Panel2.Controls.Add(m_dgwHours);

            stctrView.Dock = DockStyle.Fill;

            stctrView.SplitterDistance = 76;

            this.RowStyles.Clear ();
            this.RowStyles.Add(new RowStyle(SizeType.Percent, 5));
            this.RowStyles.Add(new RowStyle(SizeType.Percent, 71));
            this.RowStyles.Add(new RowStyle(SizeType.Percent, 24));

            m_label.Text = tec.name_shr;
            if (!(indx_TECComponent < 0))
                m_label.Text += @" - " + tec.list_TECComponents [indx_TECComponent].name_shr;
            else
                ;

            this.Controls.Add(m_label, 0, 0);
            this.Controls.Add(this.stctrView, 0, 1);

            this.Controls.Add(m_pnlQuickData, 0, 2);

            //this.ContextMenu = m_contextMenu;
        }
    }

    public class PanelTecViewGraph : PanelTecViewBase
    {
        private System.Windows.Forms.Panel pnlGraphHours;
        private System.Windows.Forms.Panel pnlGraphMins;
        
        private System.Windows.Forms.SplitContainer stctrViewPanel1, stctrViewPanel2;
        private ZedGraph.ZedGraphControl zedGraphMins;
        private ZedGraph.ZedGraphControl zedGraphHours;

        // ����������� ����
        private System.Windows.Forms.ContextMenuStrip contextMenuStripMins;
        private System.Windows.Forms.ToolStripMenuItem ������������������ToolStripMenuItemMins;
        private System.Windows.Forms.ToolStripMenuItem ����������ToolStripMenuItemMins;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1Mins;
        private System.Windows.Forms.ToolStripMenuItem ���������������ToolStripMenuItemMins;
        private System.Windows.Forms.ToolStripMenuItem �����������ToolStripMenuItemMins;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2Mins;
        private System.Windows.Forms.ToolStripMenuItem ���������ToolStripMenuItemMins;
        private System.Windows.Forms.ToolStripMenuItem ������ToolStripMenuItemMins;

        private System.Windows.Forms.ContextMenuStrip contextMenuStripHours;
        private System.Windows.Forms.ToolStripMenuItem ������������������ToolStripMenuItemHours;
        private System.Windows.Forms.ToolStripMenuItem ����������ToolStripMenuItemHours;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1Hours;
        private System.Windows.Forms.ToolStripMenuItem ���������������ToolStripMenuItemHours;
        private System.Windows.Forms.ToolStripMenuItem �����������ToolStripMenuItemHours;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2Hours;
        private System.Windows.Forms.ToolStripMenuItem ���������ToolStripMenuItemHours;
        private System.Windows.Forms.ToolStripMenuItem ������ToolStripMenuItemHours;

        public PanelTecViewGraph(StatisticCommon.TEC tec, int num_tec, int num_comp, StatusStrip sts, FormGraphicsSettings gs, FormParameters par, HReports rep)
            : base(tec, num_tec, num_comp, sts, gs, par, rep)
        {
            InitializeComponent ();
        }

        protected override void InitializeComponent () {
            base.InitializeComponent ();
            
            this.zedGraphMins = new ZedGraphControl();
            this.zedGraphHours = new ZedGraphControl();

            this.pnlGraphHours = new System.Windows.Forms.Panel();
            this.pnlGraphMins = new System.Windows.Forms.Panel();

            this.stctrViewPanel1 = new System.Windows.Forms.SplitContainer();
            this.stctrViewPanel2 = new System.Windows.Forms.SplitContainer();

            this.contextMenuStripMins = new System.Windows.Forms.ContextMenuStrip();
            this.������������������ToolStripMenuItemMins = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1Mins = new System.Windows.Forms.ToolStripSeparator();
            this.����������ToolStripMenuItemMins = new System.Windows.Forms.ToolStripMenuItem();
            this.���������ToolStripMenuItemMins = new System.Windows.Forms.ToolStripMenuItem();
            this.������ToolStripMenuItemMins = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2Mins = new System.Windows.Forms.ToolStripSeparator();
            this.���������������ToolStripMenuItemMins = new System.Windows.Forms.ToolStripMenuItem();
            this.�����������ToolStripMenuItemMins = new System.Windows.Forms.ToolStripMenuItem();

            this.contextMenuStripHours = new System.Windows.Forms.ContextMenuStrip();
            this.������������������ToolStripMenuItemHours = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1Hours = new System.Windows.Forms.ToolStripSeparator();
            this.����������ToolStripMenuItemHours = new System.Windows.Forms.ToolStripMenuItem();
            this.���������ToolStripMenuItemHours = new System.Windows.Forms.ToolStripMenuItem();
            this.������ToolStripMenuItemHours = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2Hours = new System.Windows.Forms.ToolStripSeparator();
            this.���������������ToolStripMenuItemHours = new System.Windows.Forms.ToolStripMenuItem();
            this.�����������ToolStripMenuItemHours = new System.Windows.Forms.ToolStripMenuItem();

            this.stctrViewPanel1.Panel1.SuspendLayout();
            this.stctrViewPanel1.Panel2.SuspendLayout();
            this.stctrViewPanel2.Panel1.SuspendLayout();
            this.stctrViewPanel2.Panel2.SuspendLayout();
            this.stctrViewPanel1.SuspendLayout();
            this.stctrViewPanel2.SuspendLayout();
            this.stctrView.Panel1.SuspendLayout();
            this.stctrView.Panel2.SuspendLayout();
            this.stctrView.SuspendLayout();

            this.SuspendLayout();

            this.Controls.Add(this.stctrView, 0, 0);
            
            // 
            // zedGraphMin
            // 
            this.zedGraphMins.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.zedGraphMins.Location = arPlacement[(int)CONTROLS.zedGraphMins].pt;
            this.zedGraphMins.Name = "zedGraphMin";
            this.zedGraphMins.ScrollGrace = 0;
            this.zedGraphMins.ScrollMaxX = 0;
            this.zedGraphMins.ScrollMaxY = 0;
            this.zedGraphMins.ScrollMaxY2 = 0;
            this.zedGraphMins.ScrollMinX = 0;
            this.zedGraphMins.ScrollMinY = 0;
            this.zedGraphMins.ScrollMinY2 = 0;
            //this.zedGraphMins.Size = arPlacement[(int)CONTROLS.zedGraphMins].sz;
            this.zedGraphMins.TabIndex = 0;
            this.zedGraphMins.IsEnableHEdit = false;
            this.zedGraphMins.IsEnableHPan = false;
            this.zedGraphMins.IsEnableHZoom = false;
            this.zedGraphMins.IsEnableSelection = false;
            this.zedGraphMins.IsEnableVEdit = false;
            this.zedGraphMins.IsEnableVPan = false;
            this.zedGraphMins.IsEnableVZoom = false;
            this.zedGraphMins.IsShowPointValues = true;
            this.zedGraphMins.MouseUpEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zedGraphMins_MouseUpEvent);
            this.zedGraphMins.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.zedGraphMins_PointValueEvent);
            this.zedGraphMins.DoubleClickEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zedGraphMins_DoubleClickEvent);
            this.zedGraphMins.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XScaleFormatEvent);

            // 
            // zedGraphHour
            // 
            this.zedGraphHours.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.zedGraphHours.Location = arPlacement[(int)CONTROLS.zedGraphHours].pt;
            this.zedGraphHours.Name = "zedGraphHour";
            this.zedGraphHours.ScrollGrace = 0;
            this.zedGraphHours.ScrollMaxX = 0;
            this.zedGraphHours.ScrollMaxY = 0;
            this.zedGraphHours.ScrollMaxY2 = 0;
            this.zedGraphHours.ScrollMinX = 0;
            this.zedGraphHours.ScrollMinY = 0;
            this.zedGraphHours.ScrollMinY2 = 0;
            //this.zedGraphHours.Size = arPlacement[(int)CONTROLS.zedGraphMins].sz;
            this.zedGraphHours.TabIndex = 0;
            this.zedGraphHours.IsEnableHEdit = false;
            this.zedGraphHours.IsEnableHPan = false;
            this.zedGraphHours.IsEnableHZoom = false;
            this.zedGraphHours.IsEnableSelection = false;
            this.zedGraphHours.IsEnableVEdit = false;
            this.zedGraphHours.IsEnableVPan = false;
            this.zedGraphHours.IsEnableVZoom = false;
            this.zedGraphHours.IsShowPointValues = true;
            this.zedGraphHours.MouseUpEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zedGraphHours_MouseUpEvent);
            this.zedGraphHours.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.zedGraphHours_PointValueEvent);
            this.zedGraphHours.DoubleClickEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zedGraphHours_DoubleClickEvent);

            // 
            // pnlGraphHour
            // 
            //this.pnlGraphHours.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            //            | System.Windows.Forms.AnchorStyles.Left)
            //            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlGraphHours.Dock = DockStyle.Fill;
            //this.pnlGraphHours.Location = arPlacement[(int)CONTROLS.pnlGraphHours].pt;
            this.pnlGraphHours.Name = "pnlGraphHour";
            //this.pnlGraphHours.Size = arPlacement[(int)CONTROLS.pnlGraphHours].sz;
            this.pnlGraphHours.TabIndex = 3;
            this.pnlGraphHours.Controls.Add(zedGraphHours);
            // 
            // pnlGraphMin
            // 
            //this.pnlGraphMins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            //            | System.Windows.Forms.AnchorStyles.Left)
            //            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlGraphMins.Dock = DockStyle.Fill;
            //this.pnlGraphMins.Dock = DockStyle.Right;
            //this.pnlGraphMins.Width = 600;
            //this.pnlGraphMins.Location = arPlacement[(int)CONTROLS.pnlGraphMins].pt;
            this.pnlGraphMins.Name = "pnlGraphMin";
            //this.pnlGraphMins.Size = arPlacement[(int)CONTROLS.pnlGraphMins].sz;
            this.pnlGraphMins.TabIndex = 2;
            this.pnlGraphMins.Controls.Add(zedGraphMins);

            // 
            // stctrView
            // 
            //this.stctrView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            //            | System.Windows.Forms.AnchorStyles.Left)
            //            | System.Windows.Forms.AnchorStyles.Right)));
            //this.stctrView.Location = arPlacement[(int)CONTROLS.stctrView].pt;
            this.stctrView.Dock = DockStyle.Fill;
            this.stctrView.Name = "stctrView";
            this.stctrView.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // stctrView.Panel1
            // 
            this.stctrViewPanel1.Dock = DockStyle.Fill;
            this.stctrViewPanel1.Orientation = Orientation.Vertical;
            this.stctrViewPanel1.Panel1.Controls.Add(this.m_dgwMins);
            this.stctrViewPanel1.Panel2.Controls.Add(this.pnlGraphMins);
            //this.stctrViewPanel1.SplitterDistance = 301;
            this.stctrViewPanel1.SplitterMoved += new SplitterEventHandler(stctrViewPanel1_SplitterMoved);
            this.stctrView.Panel1.Controls.Add(this.stctrViewPanel1);
            // 
            // stctrView.Panel2
            // 
            this.stctrViewPanel2.Dock = DockStyle.Fill;
            this.stctrViewPanel2.Orientation = Orientation.Vertical;
            this.stctrViewPanel2.Panel1.Controls.Add(this.m_dgwHours);
            this.stctrViewPanel2.Panel2.Controls.Add(this.pnlGraphHours);
            //this.stctrViewPanel2.SplitterDistance = 291;
            this.stctrViewPanel2.SplitterMoved += new SplitterEventHandler(stctrViewPanel2_SplitterMoved);
            this.stctrView.Panel2.Controls.Add(this.stctrViewPanel2);
            //this.stctrView.Size = arPlacement[(int)CONTROLS.stctrView].sz;
            //this.stctrView.SplitterDistance = 301;
            this.stctrView.TabIndex = 7;
            // 
            // contextMenuStripMins
            // 
            this.contextMenuStripMins.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.������������������ToolStripMenuItemMins,
            this.toolStripSeparator1Mins,
            this.����������ToolStripMenuItemMins,
            this.���������ToolStripMenuItemMins,
            this.������ToolStripMenuItemMins,
            this.toolStripSeparator2Mins,
            this.���������������ToolStripMenuItemMins,
            this.�����������ToolStripMenuItemMins});
            this.contextMenuStripMins.Name = "contextMenuStripMins";
            this.contextMenuStripMins.Size = new System.Drawing.Size(198, 148);
            // 
            // ������������������ToolStripMenuItemMins
            // 
            this.������������������ToolStripMenuItemMins.Name = "������������������ToolStripMenuItemMins";
            this.������������������ToolStripMenuItemMins.Size = new System.Drawing.Size(197, 22);
            this.������������������ToolStripMenuItemMins.Text = "���������� ��������";
            this.������������������ToolStripMenuItemMins.Checked = true;
            this.������������������ToolStripMenuItemMins.Click += new System.EventHandler(this.������������������ToolStripMenuItemMins_Click);
            // 
            // toolStripSeparator1Mins
            // 
            this.toolStripSeparator1Mins.Name = "toolStripSeparator1Mins";
            this.toolStripSeparator1Mins.Size = new System.Drawing.Size(194, 6);
            // 
            // ����������ToolStripMenuItemMins
            // 
            this.����������ToolStripMenuItemMins.Name = "����������ToolStripMenuItemMins";
            this.����������ToolStripMenuItemMins.Size = new System.Drawing.Size(197, 22);
            this.����������ToolStripMenuItemMins.Text = "����������";
            this.����������ToolStripMenuItemMins.Click += new System.EventHandler(this.����������ToolStripMenuItemMins_Click);
            // 
            // ���������ToolStripMenuItemMins
            // 
            this.���������ToolStripMenuItemMins.Name = "���������ToolStripMenuItemMins";
            this.���������ToolStripMenuItemMins.Size = new System.Drawing.Size(197, 22);
            this.���������ToolStripMenuItemMins.Text = "��������� ������";
            this.���������ToolStripMenuItemMins.Click += new System.EventHandler(this.���������ToolStripMenuItemMins_Click);
            // 
            // ������ToolStripMenuItemMins
            // 
            this.������ToolStripMenuItemMins.Name = "������ToolStripMenuItemMins";
            this.������ToolStripMenuItemMins.Size = new System.Drawing.Size(197, 22);
            this.������ToolStripMenuItemMins.Text = "��������� � MS Excel";
            this.������ToolStripMenuItemMins.Click += new System.EventHandler(this.������ToolStripMenuItemMins_Click);
            // 
            // toolStripSeparator2Mins
            // 
            this.toolStripSeparator2Mins.Name = "toolStripSeparator2Mins";
            this.toolStripSeparator2Mins.Size = new System.Drawing.Size(194, 6);
            // 
            // ���������������ToolStripMenuItemMins
            // 
            this.���������������ToolStripMenuItemMins.Name = "���������������ToolStripMenuItemMins";
            this.���������������ToolStripMenuItemMins.Size = new System.Drawing.Size(197, 22);
            this.���������������ToolStripMenuItemMins.Text = "��������� ������";
            this.���������������ToolStripMenuItemMins.Click += new System.EventHandler(this.���������������ToolStripMenuItemMins_Click);
            // 
            // �����������ToolStripMenuItemMins
            // 
            this.�����������ToolStripMenuItemMins.Name = "�����������ToolStripMenuItemMins";
            this.�����������ToolStripMenuItemMins.Size = new System.Drawing.Size(197, 22);
            this.�����������ToolStripMenuItemMins.Text = "�����������";
            this.�����������ToolStripMenuItemMins.Click += new System.EventHandler(this.�����������ToolStripMenuItemMins_Click);

            zedGraphMins.ContextMenuStrip = contextMenuStripMins;

            // 
            // contextMenuStripHours
            // 
            this.contextMenuStripHours.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.������������������ToolStripMenuItemHours,
            this.toolStripSeparator1Hours,
            this.����������ToolStripMenuItemHours,
            this.���������ToolStripMenuItemHours,
            this.������ToolStripMenuItemHours,
            this.toolStripSeparator2Hours,
            this.���������������ToolStripMenuItemHours,
            this.�����������ToolStripMenuItemHours});
            this.contextMenuStripHours.Name = "contextMenuStripHours";
            this.contextMenuStripHours.Size = new System.Drawing.Size(198, 148);
            // 
            // ������������������ToolStripMenuItemHours
            // 
            this.������������������ToolStripMenuItemHours.Name = "������������������ToolStripMenuItemHours";
            this.������������������ToolStripMenuItemHours.Size = new System.Drawing.Size(197, 22);
            this.������������������ToolStripMenuItemHours.Text = "���������� ��������";
            this.������������������ToolStripMenuItemHours.Checked = true;
            this.������������������ToolStripMenuItemHours.Click += new System.EventHandler(this.������������������ToolStripMenuItemHours_Click);
            // 
            // toolStripSeparator1Hours
            // 
            this.toolStripSeparator1Hours.Name = "toolStripSeparator1Hours";
            this.toolStripSeparator1Hours.Size = new System.Drawing.Size(194, 6);
            // 
            // ����������ToolStripMenuItemHours
            // 
            this.����������ToolStripMenuItemHours.Name = "����������ToolStripMenuItemHours";
            this.����������ToolStripMenuItemHours.Size = new System.Drawing.Size(197, 22);
            this.����������ToolStripMenuItemHours.Text = "����������";
            this.����������ToolStripMenuItemHours.Click += new System.EventHandler(this.����������ToolStripMenuItemHours_Click);
            // 
            // ���������ToolStripMenuItemHours
            // 
            this.���������ToolStripMenuItemHours.Name = "���������ToolStripMenuItemHours";
            this.���������ToolStripMenuItemHours.Size = new System.Drawing.Size(197, 22);
            this.���������ToolStripMenuItemHours.Text = "��������� ������";
            this.���������ToolStripMenuItemHours.Click += new System.EventHandler(this.���������ToolStripMenuItemHours_Click);
            // 
            // ������ToolStripMenuItemHours
            // 
            this.������ToolStripMenuItemHours.Name = "������ToolStripMenuItemHours";
            this.������ToolStripMenuItemHours.Size = new System.Drawing.Size(197, 22);
            this.������ToolStripMenuItemHours.Text = "��������� � MS Excel";
            this.������ToolStripMenuItemHours.Click += new System.EventHandler(this.������ToolStripMenuItemHours_Click);
            // 
            // toolStripSeparator2Hours
            // 
            this.toolStripSeparator2Hours.Name = "toolStripSeparator2Hours";
            this.toolStripSeparator2Hours.Size = new System.Drawing.Size(194, 6);
            // 
            // ���������������ToolStripMenuItemHours
            // 
            this.���������������ToolStripMenuItemHours.Name = "���������������ToolStripMenuItemHours";
            this.���������������ToolStripMenuItemHours.Size = new System.Drawing.Size(197, 22);
            this.���������������ToolStripMenuItemHours.Text = "��������� ������";
            this.���������������ToolStripMenuItemHours.Click += new System.EventHandler(this.���������������ToolStripMenuItemHours_Click);
            // 
            // �����������ToolStripMenuItemHours
            // 
            this.�����������ToolStripMenuItemHours.Name = "�����������ToolStripMenuItemHours";
            this.�����������ToolStripMenuItemHours.Size = new System.Drawing.Size(197, 22);
            this.�����������ToolStripMenuItemHours.Text = "�����������";
            this.�����������ToolStripMenuItemHours.Click += new System.EventHandler(this.�����������ToolStripMenuItemHours_Click);

            zedGraphHours.ContextMenuStrip = contextMenuStripHours;

            this.stctrViewPanel1.Panel1.ResumeLayout(false);
            this.stctrViewPanel1.Panel2.ResumeLayout(false);
            this.stctrViewPanel2.Panel1.ResumeLayout(false);
            this.stctrViewPanel2.Panel2.ResumeLayout(false);
            this.stctrViewPanel1.ResumeLayout(false);
            this.stctrViewPanel2.ResumeLayout(false);
            this.stctrView.Panel1.ResumeLayout(false);
            this.stctrView.Panel2.ResumeLayout(false);
            this.stctrView.ResumeLayout(false);

            this.ResumeLayout(false);
        }

        private bool zedGraphMins_DoubleClickEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            graphSettings.SetScale();
            return true;
        }

        public string XScaleFormatEvent(GraphPane pane, Axis axis, double val, int index)
        {
            return ((val) * 3).ToString();
        }

        private bool zedGraphHours_DoubleClickEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            graphSettings.SetScale();
            return true;
        }

        private string zedGraphMins_PointValueEvent(object sender, GraphPane pane, CurveItem curve, int iPt)
        {
            return curve[iPt].Y.ToString("f2");
        }

        private string zedGraphHours_PointValueEvent(object sender, GraphPane pane, CurveItem curve, int iPt)
        {
            return curve[iPt].Y.ToString("f2");
        }

        private void ������������������ToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            ������������������ToolStripMenuItemMins.Checked = !������������������ToolStripMenuItemMins.Checked;
            zedGraphMins.IsShowPointValues = ������������������ToolStripMenuItemMins.Checked;
        }

        private void ����������ToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            lock (m_lockValue)
            {
                zedGraphMins.Copy(false);
            }
        }

        private void ���������������ToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.PageSetupDialog pageSetupDialog = new PageSetupDialog();
            pageSetupDialog.Document = zedGraphMins.PrintDocument;
            pageSetupDialog.ShowDialog();
        }

        private void �����������ToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            lock (m_lockValue)
            {
                zedGraphMins.PrintDocument.Print();
            }
        }

        private void ���������ToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            lock (m_lockValue)
            {
                zedGraphMins.SaveAs();
            }
        }

        private void ������ToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            lock (m_lockValue)
            {
                SaveFileDialog sf = new SaveFileDialog();
                int hour = lastHour;
                if (hour == 24)
                    hour = 23;

                sf.CheckPathExists = true;
                sf.DefaultExt = ".xls";
                sf.Filter = "���� Microsoft Excel (.xls) | *.xls";
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    ExcelFile ef = new ExcelFile();
                    ef.Worksheets.Add("����������� ������");
                    ExcelWorksheet ws = ef.Worksheets[0];
                    if (indx_TECComponent < 0)
                    {
                        if (tec.list_TECComponents.Count == 1)
                            ws.Cells[0, 0].Value = tec.name_shr;
                        else
                        {
                            ws.Cells[0, 0].Value = tec.name_shr;
                            foreach (TECComponent g in tec.list_TECComponents)
                                ws.Cells[0, 0].Value += ", " + g.name_shr;
                        }
                    }
                    else
                    {
                        ws.Cells[0, 0].Value = tec.name_shr + ", " + tec.list_TECComponents[indx_TECComponent].name_shr;
                    }

                    if (m_valuesHours.addonValues && hour == m_valuesHours.hourAddon)
                        ws.Cells[1, 0].Value = "�������� �� " + (hour + 1).ToString() + "* ��� " + m_pnlQuickData.dtprDate.Value.ToShortDateString();
                    else
                        ws.Cells[1, 0].Value = "�������� �� " + (hour + 1).ToString() + " ��� " + m_pnlQuickData.dtprDate.Value.ToShortDateString();

                    ws.Cells[2, 0].Value = "������";
                    ws.Cells[2, 1].Value = "����";
                    ws.Cells[2, 2].Value = "���";
                    ws.Cells[2, 3].Value = "����";
                    ws.Cells[2, 4].Value = "����";
                    ws.Cells[2, 5].Value = "+/-";

                    bool valid;
                    double res_double;
                    int res_int;

                    for (int i = 0; i < 21; i++)
                    {
                        valid = int.TryParse((string)m_dgwMins.Rows[i].Cells[0].Value, out res_int);
                        if (valid)
                            ws.Cells[3 + i, 0].Value = res_int;
                        else
                            ws.Cells[3 + i, 0].Value = m_dgwMins.Rows[i].Cells[0].Value;

                        valid = double.TryParse((string)m_dgwMins.Rows[i].Cells[1].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 1].Value = res_double;
                        else
                            ws.Cells[3 + i, 1].Value = m_dgwMins.Rows[i].Cells[1].Value;

                        valid = double.TryParse((string)m_dgwMins.Rows[i].Cells[2].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 2].Value = res_double;
                        else
                            ws.Cells[3 + i, 2].Value = m_dgwMins.Rows[i].Cells[2].Value;

                        valid = double.TryParse((string)m_dgwMins.Rows[i].Cells[3].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 3].Value = res_double;
                        else
                            ws.Cells[3 + i, 3].Value = m_dgwMins.Rows[i].Cells[3].Value;

                        valid = double.TryParse((string)m_dgwMins.Rows[i].Cells[4].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 4].Value = res_double;
                        else
                            ws.Cells[3 + i, 4].Value = m_dgwMins.Rows[i].Cells[4].Value;

                        valid = double.TryParse((string)m_dgwMins.Rows[i].Cells[5].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 5].Value = res_double;
                        else
                            ws.Cells[3 + i, 5].Value = m_dgwMins.Rows[i].Cells[5].Value;
                    }

                    int tryes = 5;
                    while (tryes > 0)
                    {
                        try
                        {
                            ef.SaveXls(sf.FileName);
                            break;
                        }
                        catch
                        {
                            FileInfo fi = new FileInfo(sf.FileName);
                            sf.FileName = fi.DirectoryName + "\\Copy " + fi.Name;
                        }
                        tryes--;
                        if (tryes == 0)
                            MessageBox.Show(this, "�� ������� ��������� ����.\n�������� ��� �������, ���� ���� ����� ������ �����������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ������������������ToolStripMenuItemHours_Click(object sender, EventArgs e)
        {
            ������������������ToolStripMenuItemHours.Checked = !������������������ToolStripMenuItemHours.Checked;
            zedGraphHours.IsShowPointValues = ������������������ToolStripMenuItemHours.Checked;
        }

        private void ����������ToolStripMenuItemHours_Click(object sender, EventArgs e)
        {
            lock (m_lockValue)
            {
                zedGraphHours.Copy(false);
            }
        }

        private void ���������������ToolStripMenuItemHours_Click(object sender, EventArgs e)
        {
            PageSetupDialog pageSetupDialog = new PageSetupDialog();
            pageSetupDialog.Document = zedGraphHours.PrintDocument;
            pageSetupDialog.ShowDialog();
        }

        private void �����������ToolStripMenuItemHours_Click(object sender, EventArgs e)
        {
            lock (m_lockValue)
            {
                zedGraphHours.PrintDocument.Print();
            }
        }

        private void ���������ToolStripMenuItemHours_Click(object sender, EventArgs e)
        {
            lock (m_lockValue)
            {
                zedGraphHours.SaveAs();
            }
        }

        private void ������ToolStripMenuItemHours_Click(object sender, EventArgs e)
        {
            lock (m_lockValue)
            {
                SaveFileDialog sf = new SaveFileDialog();
                sf.CheckPathExists = true;
                sf.DefaultExt = ".xls";
                sf.Filter = "���� Microsoft Excel (.xls) | *.xls";
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    ExcelFile ef = new ExcelFile();
                    ef.Worksheets.Add("������� ������");
                    ExcelWorksheet ws = ef.Worksheets[0];
                    if (indx_TECComponent < 0)
                    {
                        if (tec.list_TECComponents.Count == 1)
                            ws.Cells[0, 0].Value = tec.name_shr;
                        else
                        {
                            ws.Cells[0, 0].Value = tec.name_shr;
                            foreach (TECComponent g in tec.list_TECComponents)
                                ws.Cells[0, 0].Value += ", " + g.name_shr;
                        }
                    }
                    else
                    {
                        ws.Cells[0, 0].Value = tec.name_shr + ", " + tec.list_TECComponents[indx_TECComponent].name_shr;
                    }

                    ws.Cells[1, 0].Value = "�������� �� " + m_pnlQuickData.dtprDate.Value.ToShortDateString();

                    ws.Cells[2, 0].Value = "���";
                    ws.Cells[2, 1].Value = "����";
                    ws.Cells[2, 2].Value = "���";
                    ws.Cells[2, 3].Value = "����";
                    ws.Cells[2, 4].Value = "����";
                    ws.Cells[2, 5].Value = "+/-";

                    bool valid;
                    double res_double;
                    int res_int;

                    for (int i = 0; i < m_dgwHours.Rows.Count; i++)
                    {
                        valid = int.TryParse((string)m_dgwHours.Rows[i].Cells[0].Value, out res_int);
                        if (valid)
                            ws.Cells[3 + i, 0].Value = res_int;
                        else
                            ws.Cells[3 + i, 0].Value = m_dgwHours.Rows[i].Cells[0].Value;

                        valid = double.TryParse((string)m_dgwHours.Rows[i].Cells[1].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 1].Value = res_double;
                        else
                            ws.Cells[3 + i, 1].Value = m_dgwHours.Rows[i].Cells[1].Value;

                        valid = double.TryParse((string)m_dgwHours.Rows[i].Cells[2].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 2].Value = res_double;
                        else
                            ws.Cells[3 + i, 2].Value = m_dgwHours.Rows[i].Cells[2].Value;

                        valid = double.TryParse((string)m_dgwHours.Rows[i].Cells[3].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 3].Value = res_double;
                        else
                            ws.Cells[3 + i, 3].Value = m_dgwHours.Rows[i].Cells[3].Value;

                        valid = double.TryParse((string)m_dgwHours.Rows[i].Cells[4].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 4].Value = res_double;
                        else
                            ws.Cells[3 + i, 4].Value = m_dgwHours.Rows[i].Cells[4].Value;

                        valid = double.TryParse((string)m_dgwHours.Rows[i].Cells[5].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 5].Value = res_double;
                        else
                            ws.Cells[3 + i, 5].Value = m_dgwHours.Rows[i].Cells[5].Value;
                    }

                    int tryes = 5;
                    while (tryes > 0)
                    {
                        try
                        {
                            ef.SaveXls(sf.FileName);
                            break;
                        }
                        catch
                        {
                            FileInfo fi = new FileInfo(sf.FileName);
                            sf.FileName = fi.DirectoryName + "\\Copy " + fi.Name;
                        }
                        tryes--;
                        if (tryes == 0)
                            MessageBox.Show(this, "�� ������� ��������� ����.\n�������� ��� �������, ���� ���� ����� ������ �����������.", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private bool zedGraphMins_MouseUpEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return true;

            object obj;
            PointF p = new PointF(e.X, e.Y);
            bool found;
            int index;

            found = sender.GraphPane.FindNearestObject(p, CreateGraphics(), out obj, out index);

            if (!(obj is BarItem) && !(obj is LineItem))
                return true;

            if (lastMin <= index + 1)
                return true;

            if (found)
            {
                lock (m_lockValue)
                {
                    int oldLastMin = lastMin;
                    recalcAver = false;
                    lastMin = index + 2;
                    m_pnlQuickData.ShowFactValues();
                    recalcAver = true;
                    lastMin = oldLastMin;
                }
            }

            return true;
        }

        private bool zedGraphHours_MouseUpEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return true;

            object obj;
            PointF p = new PointF(e.X, e.Y);
            bool found;
            int index;

            found = sender.GraphPane.FindNearestObject(p, CreateGraphics(), out obj, out index);

            if (!(obj is BarItem) && !(obj is LineItem))
                return true;

            if (found)
            {
                delegateStartWait();
                lock (m_lockValue)
                {
                    currHour = false;
                    if (m_valuesHours.season == seasonJumpE.SummerToWinter)
                    {
                        if (index <= m_valuesHours.hourAddon)
                        {
                            lastHour = index;
                            m_valuesHours.addonValues = false;
                        }
                        else
                        {
                            if (index == m_valuesHours.hourAddon + 1)
                            {
                                lastHour = index - 1;
                                m_valuesHours.addonValues = true;
                            }
                            else
                            {
                                lastHour = index - 1;
                                m_valuesHours.addonValues = false;
                            }
                        }
                    }
                    else
                        if (m_valuesHours.season == seasonJumpE.WinterToSummer)
                        {
                            if (index < m_valuesHours.hourAddon)
                                lastHour = index;
                            else
                                lastHour = index + 1;
                        }
                        else
                            lastHour = index;
                    ClearValuesMins();

                    m_newState = true;
                    m_states.Clear();
                    m_states.Add(StatesMachine.RetroMins);
                    m_states.Add(StatesMachine.PPBRValues);
                    m_states.Add(StatesMachine.AdminValues);

                    try
                    {
                        m_sem.Release(1);
                    }
                    catch (Exception excpt) { Logging.Logg().LogExceptionToFile(excpt, "catch - zedGraphHours_MouseUpEvent () - sem.Release(1)"); }

                }
                delegateStopWait();
            }

            return true;
        }

        private void DrawGraphMins(int hour)
        {
            if (hour == 24)
                hour = 23;

            GraphPane pane = zedGraphMins.GraphPane;

            pane.CurveList.Clear();

            int itemscount = 20;

            string[] names = new string[itemscount];

            double[] valuesRecommend = new double[itemscount];

            double[] valuesUDGe = new double[itemscount];

            double[] valuesFact = new double[itemscount];

            for (int i = 0; i < itemscount; i++)
            {
                valuesFact[i] = m_valuesMins.valuesFact[i + 1];
                valuesUDGe[i] = m_valuesMins.valuesUDGe[i + 1];
            }

            //double[] valuesPDiviation = new double[itemscount];

            //double[] valuesODiviation = new double[itemscount];

            double minimum = double.MaxValue, minimum_scale;
            double maximum = 0, maximum_scale;
            bool noValues = true;

            for (int i = 0; i < itemscount; i++)
            {
                names[i] = ((i + 1) * 3).ToString();
                //valuesPDiviation[i] = m_valuesMins.valuesUDGe[i] + m_valuesMins.valuesDiviation[i];
                //valuesODiviation[i] = m_valuesMins.valuesUDGe[i] - m_valuesMins.valuesDiviation[i];

                if (currHour)
                {
                    if (i < lastMin - 1 || !adminValuesReceived)
                        valuesRecommend[i] = 0;
                    else
                        valuesRecommend[i] = recomendation;
                }

                //if (minimum > valuesPDiviation[i] && valuesPDiviation[i] != 0)
                //{
                //    minimum = valuesPDiviation[i];
                //    noValues = false;
                //}

                //if (minimum > valuesODiviation[i] && valuesODiviation[i] != 0)
                //{
                //    minimum = valuesODiviation[i];
                //    noValues = false;
                //}

                if (currHour)
                {
                    if (minimum > valuesRecommend[i] && valuesRecommend[i] != 0)
                    {
                        minimum = valuesRecommend[i];
                        noValues = false;
                    }
                }

                if (minimum > valuesUDGe[i] && valuesUDGe[i] != 0)
                {
                    minimum = valuesUDGe[i];
                    noValues = false;
                }

                if (minimum > valuesFact[i] && valuesFact[i] != 0)
                {
                    minimum = valuesFact[i];
                    noValues = false;
                }

                //if (maximum < valuesPDiviation[i])
                //    maximum = valuesPDiviation[i];

                //if (maximum < valuesODiviation[i])
                //    maximum = valuesODiviation[i];

                if (currHour)
                {
                    if (maximum < valuesRecommend[i])
                        maximum = valuesRecommend[i];
                }

                if (maximum < valuesUDGe[i])
                    maximum = valuesUDGe[i];

                if (maximum < valuesFact[i])
                    maximum = valuesFact[i];
            }

            if (!graphSettings.scale)
                minimum = 0;

            if (noValues)
            {
                minimum_scale = 0;
                maximum_scale = 10;
            }
            else
            {
                if (minimum != maximum)
                {
                    minimum_scale = minimum - (maximum - minimum) * 0.2;
                    if (minimum_scale < 0)
                        minimum_scale = 0;
                    maximum_scale = maximum + (maximum - minimum) * 0.2;
                }
                else
                {
                    minimum_scale = minimum - minimum * 0.2;
                    maximum_scale = maximum + maximum * 0.2;
                }
            }


            pane.Chart.Fill = new Fill(graphSettings.bgColor);

            LineItem curve2 = pane.AddCurve("����", null, valuesUDGe, graphSettings.udgColor);
            //LineItem curve4 = pane.AddCurve("", null, valuesODiviation, graphSettings.divColor);
            //LineItem curve3 = pane.AddCurve("��������� ����������", null, valuesPDiviation, graphSettings.divColor);

            if (graphSettings.graphTypes == FormGraphicsSettings.GraphTypes.Bar)
            {
                BarItem curve1 = pane.AddBar("��������", null, valuesFact, graphSettings.pColor);

                BarItem curve0 = pane.AddBar("������������� ��������", null, valuesRecommend, graphSettings.recColor);
            }
            else
            {
                if (graphSettings.graphTypes == FormGraphicsSettings.GraphTypes.Linear)
                {
                    if (lastMin > 1)
                    {
                        double[] valuesFactLast = new double[lastMin - 1];
                        for (int i = 0; i < lastMin - 1; i++)
                            valuesFactLast[i] = valuesFact[i];

                        LineItem curve1 = pane.AddCurve("��������", null, valuesFactLast, graphSettings.pColor);

                        PointPairList valuesRecList = new PointPairList();
                        if (adminValuesReceived && currHour)
                            for (int i = lastMin - 1; i < itemscount; i++)
                                valuesRecList.Add((double)(i + 1), valuesRecommend[i]);

                        LineItem curve0 = pane.AddCurve("������������� ��������", valuesRecList, graphSettings.recColor);
                    }
                    else
                    {
                        LineItem curve1 = pane.AddCurve("��������", null, null, graphSettings.pColor);
                        LineItem curve0 = pane.AddCurve("������������� ��������", null, valuesRecommend, graphSettings.recColor);
                    }
                }
            }

            pane.BarSettings.Type = BarType.Overlay;

            pane.XAxis.Type = AxisType.Linear;

            pane.XAxis.Title.Text = "";
            pane.YAxis.Title.Text = "";

            if (m_valuesHours.addonValues && hour == m_valuesHours.hourAddon)
                pane.Title.Text = //"������� �������� �� " + /*System.TimeZone.CurrentTimeZone.ToUniversalTime(*/dtprDate.Value/*)*/.ToShortDateString() + " " + 
                                    (hour + 1).ToString() + "* ���";
            else
                pane.Title.Text = //"������� �������� �� " + /*System.TimeZone.CurrentTimeZone.ToUniversalTime(*/dtprDate.Value/*)*/.ToShortDateString() + " " + 
                                    (hour + 1).ToString() + " ���";

            pane.XAxis.Scale.Min = 0.5;
            pane.XAxis.Scale.Max = 20.5;
            pane.XAxis.Scale.MinorStep = 1;
            pane.XAxis.Scale.MajorStep = 1;

            pane.XAxis.Scale.TextLabels = names;
            pane.XAxis.Scale.IsPreventLabelOverlap = false;

            // �������� ����������� ����� �������� ������� ����� �� ��� X
            pane.XAxis.MajorGrid.IsVisible = true;
            // ������ ��� ���������� ����� ��� ������� ����� �� ��� X:
            // ����� ������� ����� 10 ��������, ... 
            pane.XAxis.MajorGrid.DashOn = 10;
            // ����� 5 �������� - �������
            pane.XAxis.MajorGrid.DashOff = 5;
            // ������� �����
            pane.XAxis.MajorGrid.PenWidth = 0.1F;
            pane.XAxis.MajorGrid.Color = graphSettings.gridColor;

            // �������� ����������� ����� �������� ������� ����� �� ��� Y
            pane.YAxis.MajorGrid.IsVisible = true;
            // ���������� ������ ��� ���������� ����� ��� ������� ����� �� ��� Y
            pane.YAxis.MajorGrid.DashOn = 10;
            pane.YAxis.MajorGrid.DashOff = 5;
            // ������� �����
            pane.YAxis.MajorGrid.PenWidth = 0.1F;
            pane.YAxis.MajorGrid.Color = graphSettings.gridColor;

            // �������� ����������� ����� �������� ������ ����� �� ��� Y
            pane.YAxis.MinorGrid.IsVisible = true;
            // ����� ������� ����� ������ �������, ... 
            pane.YAxis.MinorGrid.DashOn = 1;
            pane.YAxis.MinorGrid.DashOff = 2;
            // ������� �����
            pane.YAxis.MinorGrid.PenWidth = 0.1F;
            pane.YAxis.MinorGrid.Color = graphSettings.gridColor;


            // ������������� ������������ ��� �������� �� ��� Y
            pane.YAxis.Scale.Min = minimum_scale;
            pane.YAxis.Scale.Max = maximum_scale;

            zedGraphMins.AxisChange();

            zedGraphMins.Invalidate();
        }

        private void DrawGraphHours()
        {
            GraphPane pane = zedGraphHours.GraphPane;

            pane.CurveList.Clear();

            int itemscount;

            if (m_valuesHours.season == seasonJumpE.SummerToWinter)
                itemscount = 25;
            else
                if (m_valuesHours.season == seasonJumpE.WinterToSummer)
                    itemscount = 23;
                else
                    itemscount = 24;

            string[] names = new string[itemscount];

            double[] valuesPDiviation = new double[itemscount];
            double[] valuesODiviation = new double[itemscount];
            double[] valuesUDGe = new double[itemscount];
            double[] valuesFact = new double[itemscount];

            double minimum = double.MaxValue, minimum_scale;
            double maximum = 0, maximum_scale;
            bool noValues = true;
            for (int i = 0; i < itemscount; i++)
            {
                if (m_valuesHours.season == seasonJumpE.SummerToWinter)
                {
                    if (i <= m_valuesHours.hourAddon)
                    {
                        names[i] = (i + 1).ToString();
                        valuesPDiviation[i] = m_valuesHours.valuesUDGe[i] + m_valuesHours.valuesDiviation[i];
                        valuesODiviation[i] = m_valuesHours.valuesUDGe[i] - m_valuesHours.valuesDiviation[i];
                        valuesUDGe[i] = m_valuesHours.valuesUDGe[i];
                        valuesFact[i] = m_valuesHours.valuesFact[i];
                    }
                    else
                        if (i == m_valuesHours.hourAddon + 1)
                        {
                            names[i] = i.ToString() + "*";
                            valuesPDiviation[i] = m_valuesHours.valuesUDGeAddon + m_valuesHours.valuesDiviationAddon;
                            valuesODiviation[i] = m_valuesHours.valuesUDGeAddon - m_valuesHours.valuesDiviationAddon;
                            valuesUDGe[i] = m_valuesHours.valuesUDGeAddon;
                            valuesFact[i] = m_valuesHours.valuesFactAddon;
                        }
                        else
                        {
                            this.m_dgwHours.Rows[i].Cells[0].Value = i.ToString();
                            names[i] = i.ToString();
                            valuesPDiviation[i] = m_valuesHours.valuesUDGe[i - 1] + m_valuesHours.valuesDiviation[i - 1];
                            valuesODiviation[i] = m_valuesHours.valuesUDGe[i - 1] - m_valuesHours.valuesDiviation[i - 1];
                            valuesUDGe[i] = m_valuesHours.valuesUDGe[i - 1];
                            valuesFact[i] = m_valuesHours.valuesFact[i - 1];
                        }

                }
                else
                    if (m_valuesHours.season == seasonJumpE.WinterToSummer)
                    {
                        if (i < m_valuesHours.hourAddon)
                        {
                            names[i] = (i + 1).ToString();
                            valuesPDiviation[i] = m_valuesHours.valuesUDGe[i] + m_valuesHours.valuesDiviation[i];
                            valuesODiviation[i] = m_valuesHours.valuesUDGe[i] - m_valuesHours.valuesDiviation[i];
                            valuesUDGe[i] = m_valuesHours.valuesUDGe[i];
                            valuesFact[i] = m_valuesHours.valuesFact[i];
                        }
                        else
                        {
                            names[i] = (i + 2).ToString();
                            valuesPDiviation[i] = m_valuesHours.valuesUDGe[i + 1] + m_valuesHours.valuesDiviation[i + 1];
                            valuesODiviation[i] = m_valuesHours.valuesUDGe[i + 1] - m_valuesHours.valuesDiviation[i + 1];
                            valuesUDGe[i] = m_valuesHours.valuesUDGe[i + 1];
                            valuesFact[i] = m_valuesHours.valuesFact[i + 1];
                        }
                    }
                    else
                    {
                        names[i] = (i + 1).ToString();
                        valuesPDiviation[i] = m_valuesHours.valuesUDGe[i] + m_valuesHours.valuesDiviation[i];
                        valuesODiviation[i] = m_valuesHours.valuesUDGe[i] - m_valuesHours.valuesDiviation[i];
                        valuesUDGe[i] = m_valuesHours.valuesUDGe[i];
                        valuesFact[i] = m_valuesHours.valuesFact[i];
                    }

                if (minimum > valuesPDiviation[i] && valuesPDiviation[i] != 0)
                {
                    minimum = valuesPDiviation[i];
                    noValues = false;
                }

                if (minimum > valuesODiviation[i] && valuesODiviation[i] != 0)
                {
                    minimum = valuesODiviation[i];
                    noValues = false;
                }

                if (minimum > valuesUDGe[i] && valuesUDGe[i] != 0)
                {
                    minimum = valuesUDGe[i];
                    noValues = false;
                }

                if (minimum > valuesFact[i] && valuesFact[i] != 0)
                {
                    minimum = valuesFact[i];
                    noValues = false;
                }

                if (maximum < valuesPDiviation[i])
                    maximum = valuesPDiviation[i];

                if (maximum < valuesODiviation[i])
                    maximum = valuesODiviation[i];

                if (maximum < valuesUDGe[i])
                    maximum = valuesUDGe[i];

                if (maximum < valuesFact[i])
                    maximum = valuesFact[i];
            }

            if (!graphSettings.scale)
                minimum = 0;

            if (noValues)
            {
                minimum_scale = 0;
                maximum_scale = 10;
            }
            else
            {
                if (minimum != maximum)
                {
                    minimum_scale = minimum - (maximum - minimum) * 0.2;
                    if (minimum_scale < 0)
                        minimum_scale = 0;
                    maximum_scale = maximum + (maximum - minimum) * 0.2;
                }
                else
                {
                    minimum_scale = minimum - minimum * 0.2;
                    maximum_scale = maximum + maximum * 0.2;
                }
            }

            pane.Chart.Fill = new Fill(graphSettings.bgColor);

            LineItem curve2 = pane.AddCurve("����", null, valuesUDGe, graphSettings.udgColor);
            LineItem curve4 = pane.AddCurve("", null, valuesODiviation, graphSettings.divColor);
            LineItem curve3 = pane.AddCurve("��������� ����������", null, valuesPDiviation, graphSettings.divColor);


            if (graphSettings.graphTypes == FormGraphicsSettings.GraphTypes.Bar)
            {
                BarItem curve1 = pane.AddBar("��������", null, valuesFact, graphSettings.pColor);
            }
            else
            {
                if (graphSettings.graphTypes == FormGraphicsSettings.GraphTypes.Linear)
                {
                    int valuescount;

                    if (m_valuesHours.season == seasonJumpE.SummerToWinter)
                        valuescount = lastHour + 1;
                    else
                        if (m_valuesHours.season == seasonJumpE.WinterToSummer)
                            valuescount = lastHour - 1;
                        else
                            valuescount = lastHour;

                    double[] valuesFactNew = new double[valuescount];
                    for (int i = 0; i < valuescount; i++)
                        valuesFactNew[i] = valuesFact[i];

                    LineItem curve1 = pane.AddCurve("��������", null, valuesFactNew, graphSettings.pColor);
                }
            }

            pane.XAxis.Type = AxisType.Text;
            pane.XAxis.Title.Text = "";
            pane.YAxis.Title.Text = "";
            pane.Title.Text = "�������� �� " + m_pnlQuickData.dtprDate.Value.ToShortDateString();

            pane.XAxis.Scale.TextLabels = names;
            pane.XAxis.Scale.IsPreventLabelOverlap = false;

            // �������� ����������� ����� �������� ������� ����� �� ��� X
            pane.XAxis.MajorGrid.IsVisible = true;
            // ������ ��� ���������� ����� ��� ������� ����� �� ��� X:
            // ����� ������� ����� 10 ��������, ... 
            pane.XAxis.MajorGrid.DashOn = 10;
            // ����� 5 �������� - �������
            pane.XAxis.MajorGrid.DashOff = 5;
            // ������� �����
            pane.XAxis.MajorGrid.PenWidth = 0.1F;
            pane.XAxis.MajorGrid.Color = graphSettings.gridColor;

            // �������� ����������� ����� �������� ������� ����� �� ��� Y
            pane.YAxis.MajorGrid.IsVisible = true;
            // ���������� ������ ��� ���������� ����� ��� ������� ����� �� ��� Y
            pane.YAxis.MajorGrid.DashOn = 10;
            pane.YAxis.MajorGrid.DashOff = 5;
            // ������� �����
            pane.YAxis.MajorGrid.PenWidth = 0.1F;
            pane.YAxis.MajorGrid.Color = graphSettings.gridColor;

            // �������� ����������� ����� �������� ������ ����� �� ��� Y
            pane.YAxis.MinorGrid.IsVisible = true;
            // ����� ������� ����� ������ �������, ... 
            pane.YAxis.MinorGrid.DashOn = 1;
            pane.YAxis.MinorGrid.DashOff = 2;
            // ������� �����
            pane.YAxis.MinorGrid.PenWidth = 0.1F;
            pane.YAxis.MinorGrid.Color = graphSettings.gridColor;

            // ������������� ������������ ��� �������� �� ��� Y
            pane.YAxis.Scale.Min = minimum_scale;
            pane.YAxis.Scale.Max = maximum_scale;

            zedGraphHours.AxisChange();

            zedGraphHours.Invalidate();
        }

        public override void Start()
        {
            base.Start ();

            DrawGraphMins(0);
            DrawGraphHours();
        }

        protected override void UpdateGUI_Fact(int hour, int min)
        {
            base.UpdateGUI_Fact(hour, min);

            lock (m_lockValue)
            {
                DrawGraphHours();

                DrawGraphMins(hour);
            }
        }

        public void UpdateGraphicsCurrent()
        {
            DrawGraphHours();
            DrawGraphMins(lastHour);
        }

        private void stctrViewPanel1_SplitterMoved(object sender, SplitterEventArgs e)
        {
        }

        private void stctrViewPanel2_SplitterMoved(object sender, SplitterEventArgs e)
        {
        }
    }
}
