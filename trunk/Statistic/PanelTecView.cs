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

        public PanelTecViewTable(StatisticCommon.TEC tec, int num_tec, int num_comp, PanelCustomTecView.HLabelEmpty label, DelegateFunc fErrRep, DelegateFunc fActRep)
            : base(tec, num_tec, num_comp, fErrRep, fActRep)
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

            m_label.Text = m_tecView.m_tec.name_shr;
            if (!(indx_TECComponent < 0))
                m_label.Text += @" - " + m_tecView.m_tec.list_TECComponents[indx_TECComponent].name_shr;
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

        // контекстные меню
        private System.Windows.Forms.ContextMenuStrip contextMenuStripMins;
        private System.Windows.Forms.ToolStripMenuItem показыватьЗначенияToolStripMenuItemMins;
        private System.Windows.Forms.ToolStripMenuItem копироватьToolStripMenuItemMins;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1Mins;
        private System.Windows.Forms.ToolStripMenuItem параметрыПечатиToolStripMenuItemMins;
        private System.Windows.Forms.ToolStripMenuItem распечататьToolStripMenuItemMins;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2Mins;
        private System.Windows.Forms.ToolStripMenuItem сохранитьToolStripMenuItemMins;
        private System.Windows.Forms.ToolStripMenuItem эксельToolStripMenuItemMins;

        private System.Windows.Forms.ContextMenuStrip contextMenuStripHours;
        private System.Windows.Forms.ToolStripMenuItem показыватьЗначенияToolStripMenuItemHours;
        private System.Windows.Forms.ToolStripMenuItem копироватьToolStripMenuItemHours;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1Hours;
        private System.Windows.Forms.ToolStripMenuItem параметрыПечатиToolStripMenuItemHours;
        private System.Windows.Forms.ToolStripMenuItem распечататьToolStripMenuItemHours;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2Hours;
        private System.Windows.Forms.ToolStripMenuItem сохранитьToolStripMenuItemHours;
        private System.Windows.Forms.ToolStripMenuItem эксельToolStripMenuItemHours;

        public PanelTecViewGraph(StatisticCommon.TEC tec, int num_tec, int num_comp, DelegateFunc fErrRep, DelegateFunc fActRep)
            : base(tec, num_tec, num_comp, fErrRep, fActRep)
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
            this.показыватьЗначенияToolStripMenuItemMins = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1Mins = new System.Windows.Forms.ToolStripSeparator();
            this.копироватьToolStripMenuItemMins = new System.Windows.Forms.ToolStripMenuItem();
            this.сохранитьToolStripMenuItemMins = new System.Windows.Forms.ToolStripMenuItem();
            this.эксельToolStripMenuItemMins = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2Mins = new System.Windows.Forms.ToolStripSeparator();
            this.параметрыПечатиToolStripMenuItemMins = new System.Windows.Forms.ToolStripMenuItem();
            this.распечататьToolStripMenuItemMins = new System.Windows.Forms.ToolStripMenuItem();

            this.contextMenuStripHours = new System.Windows.Forms.ContextMenuStrip();
            this.показыватьЗначенияToolStripMenuItemHours = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1Hours = new System.Windows.Forms.ToolStripSeparator();
            this.копироватьToolStripMenuItemHours = new System.Windows.Forms.ToolStripMenuItem();
            this.сохранитьToolStripMenuItemHours = new System.Windows.Forms.ToolStripMenuItem();
            this.эксельToolStripMenuItemHours = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2Hours = new System.Windows.Forms.ToolStripSeparator();
            this.параметрыПечатиToolStripMenuItemHours = new System.Windows.Forms.ToolStripMenuItem();
            this.распечататьToolStripMenuItemHours = new System.Windows.Forms.ToolStripMenuItem();

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
            this.показыватьЗначенияToolStripMenuItemMins,
            this.toolStripSeparator1Mins,
            this.копироватьToolStripMenuItemMins,
            this.сохранитьToolStripMenuItemMins,
            this.эксельToolStripMenuItemMins,
            this.toolStripSeparator2Mins,
            this.параметрыПечатиToolStripMenuItemMins,
            this.распечататьToolStripMenuItemMins});
            this.contextMenuStripMins.Name = "contextMenuStripMins";
            this.contextMenuStripMins.Size = new System.Drawing.Size(198, 148);
            // 
            // показыватьЗначенияToolStripMenuItemMins
            // 
            this.показыватьЗначенияToolStripMenuItemMins.Name = "показыватьЗначенияToolStripMenuItemMins";
            this.показыватьЗначенияToolStripMenuItemMins.Size = new System.Drawing.Size(197, 22);
            this.показыватьЗначенияToolStripMenuItemMins.Text = "Показывать значения";
            this.показыватьЗначенияToolStripMenuItemMins.Checked = true;
            this.показыватьЗначенияToolStripMenuItemMins.Click += new System.EventHandler(this.показыватьЗначенияToolStripMenuItemMins_Click);
            // 
            // toolStripSeparator1Mins
            // 
            this.toolStripSeparator1Mins.Name = "toolStripSeparator1Mins";
            this.toolStripSeparator1Mins.Size = new System.Drawing.Size(194, 6);
            // 
            // копироватьToolStripMenuItemMins
            // 
            this.копироватьToolStripMenuItemMins.Name = "копироватьToolStripMenuItemMins";
            this.копироватьToolStripMenuItemMins.Size = new System.Drawing.Size(197, 22);
            this.копироватьToolStripMenuItemMins.Text = "Копировать";
            this.копироватьToolStripMenuItemMins.Click += new System.EventHandler(this.копироватьToolStripMenuItemMins_Click);
            // 
            // сохранитьToolStripMenuItemMins
            // 
            this.сохранитьToolStripMenuItemMins.Name = "сохранитьToolStripMenuItemMins";
            this.сохранитьToolStripMenuItemMins.Size = new System.Drawing.Size(197, 22);
            this.сохранитьToolStripMenuItemMins.Text = "Сохранить график";
            this.сохранитьToolStripMenuItemMins.Click += new System.EventHandler(this.сохранитьToolStripMenuItemMins_Click);
            // 
            // эксельToolStripMenuItemMins
            // 
            this.эксельToolStripMenuItemMins.Name = "эксельToolStripMenuItemMins";
            this.эксельToolStripMenuItemMins.Size = new System.Drawing.Size(197, 22);
            this.эксельToolStripMenuItemMins.Text = "Сохранить в MS Excel";
            this.эксельToolStripMenuItemMins.Click += new System.EventHandler(this.эксельToolStripMenuItemMins_Click);
            // 
            // toolStripSeparator2Mins
            // 
            this.toolStripSeparator2Mins.Name = "toolStripSeparator2Mins";
            this.toolStripSeparator2Mins.Size = new System.Drawing.Size(194, 6);
            // 
            // параметрыПечатиToolStripMenuItemMins
            // 
            this.параметрыПечатиToolStripMenuItemMins.Name = "параметрыПечатиToolStripMenuItemMins";
            this.параметрыПечатиToolStripMenuItemMins.Size = new System.Drawing.Size(197, 22);
            this.параметрыПечатиToolStripMenuItemMins.Text = "Параметры печати";
            this.параметрыПечатиToolStripMenuItemMins.Click += new System.EventHandler(this.параметрыПечатиToolStripMenuItemMins_Click);
            // 
            // распечататьToolStripMenuItemMins
            // 
            this.распечататьToolStripMenuItemMins.Name = "распечататьToolStripMenuItemMins";
            this.распечататьToolStripMenuItemMins.Size = new System.Drawing.Size(197, 22);
            this.распечататьToolStripMenuItemMins.Text = "Распечатать";
            this.распечататьToolStripMenuItemMins.Click += new System.EventHandler(this.распечататьToolStripMenuItemMins_Click);

            zedGraphMins.ContextMenuStrip = contextMenuStripMins;

            // 
            // contextMenuStripHours
            // 
            this.contextMenuStripHours.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.показыватьЗначенияToolStripMenuItemHours,
            this.toolStripSeparator1Hours,
            this.копироватьToolStripMenuItemHours,
            this.сохранитьToolStripMenuItemHours,
            this.эксельToolStripMenuItemHours,
            this.toolStripSeparator2Hours,
            this.параметрыПечатиToolStripMenuItemHours,
            this.распечататьToolStripMenuItemHours});
            this.contextMenuStripHours.Name = "contextMenuStripHours";
            this.contextMenuStripHours.Size = new System.Drawing.Size(198, 148);
            // 
            // показыватьЗначенияToolStripMenuItemHours
            // 
            this.показыватьЗначенияToolStripMenuItemHours.Name = "показыватьЗначенияToolStripMenuItemHours";
            this.показыватьЗначенияToolStripMenuItemHours.Size = new System.Drawing.Size(197, 22);
            this.показыватьЗначенияToolStripMenuItemHours.Text = "Показывать значения";
            this.показыватьЗначенияToolStripMenuItemHours.Checked = true;
            this.показыватьЗначенияToolStripMenuItemHours.Click += new System.EventHandler(this.показыватьЗначенияToolStripMenuItemHours_Click);
            // 
            // toolStripSeparator1Hours
            // 
            this.toolStripSeparator1Hours.Name = "toolStripSeparator1Hours";
            this.toolStripSeparator1Hours.Size = new System.Drawing.Size(194, 6);
            // 
            // копироватьToolStripMenuItemHours
            // 
            this.копироватьToolStripMenuItemHours.Name = "копироватьToolStripMenuItemHours";
            this.копироватьToolStripMenuItemHours.Size = new System.Drawing.Size(197, 22);
            this.копироватьToolStripMenuItemHours.Text = "Копировать";
            this.копироватьToolStripMenuItemHours.Click += new System.EventHandler(this.копироватьToolStripMenuItemHours_Click);
            // 
            // сохранитьToolStripMenuItemHours
            // 
            this.сохранитьToolStripMenuItemHours.Name = "сохранитьToolStripMenuItemHours";
            this.сохранитьToolStripMenuItemHours.Size = new System.Drawing.Size(197, 22);
            this.сохранитьToolStripMenuItemHours.Text = "Сохранить график";
            this.сохранитьToolStripMenuItemHours.Click += new System.EventHandler(this.сохранитьToolStripMenuItemHours_Click);
            // 
            // эксельToolStripMenuItemHours
            // 
            this.эксельToolStripMenuItemHours.Name = "эксельToolStripMenuItemHours";
            this.эксельToolStripMenuItemHours.Size = new System.Drawing.Size(197, 22);
            this.эксельToolStripMenuItemHours.Text = "Сохранить в MS Excel";
            this.эксельToolStripMenuItemHours.Click += new System.EventHandler(this.эксельToolStripMenuItemHours_Click);
            // 
            // toolStripSeparator2Hours
            // 
            this.toolStripSeparator2Hours.Name = "toolStripSeparator2Hours";
            this.toolStripSeparator2Hours.Size = new System.Drawing.Size(194, 6);
            // 
            // параметрыПечатиToolStripMenuItemHours
            // 
            this.параметрыПечатиToolStripMenuItemHours.Name = "параметрыПечатиToolStripMenuItemHours";
            this.параметрыПечатиToolStripMenuItemHours.Size = new System.Drawing.Size(197, 22);
            this.параметрыПечатиToolStripMenuItemHours.Text = "Параметры печати";
            this.параметрыПечатиToolStripMenuItemHours.Click += new System.EventHandler(this.параметрыПечатиToolStripMenuItemHours_Click);
            // 
            // распечататьToolStripMenuItemHours
            // 
            this.распечататьToolStripMenuItemHours.Name = "распечататьToolStripMenuItemHours";
            this.распечататьToolStripMenuItemHours.Size = new System.Drawing.Size(197, 22);
            this.распечататьToolStripMenuItemHours.Text = "Распечатать";
            this.распечататьToolStripMenuItemHours.Click += new System.EventHandler(this.распечататьToolStripMenuItemHours_Click);

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
            FormMain.formGraphicsSettings.SetScale();

            return true;
        }

        public string XScaleFormatEvent(GraphPane pane, Axis axis, double val, int index)
        {
            return ((val) * 3).ToString();
        }

        private bool zedGraphHours_DoubleClickEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            FormMain.formGraphicsSettings.SetScale();

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

        private void показыватьЗначенияToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            показыватьЗначенияToolStripMenuItemMins.Checked = !показыватьЗначенияToolStripMenuItemMins.Checked;
            zedGraphMins.IsShowPointValues = показыватьЗначенияToolStripMenuItemMins.Checked;
        }

        private void копироватьToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            lock (m_tecView.m_lockValue)
            {
                zedGraphMins.Copy(false);
            }
        }

        private void параметрыПечатиToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.PageSetupDialog pageSetupDialog = new PageSetupDialog();
            pageSetupDialog.Document = zedGraphMins.PrintDocument;
            pageSetupDialog.ShowDialog();
        }

        private void распечататьToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            lock (m_tecView.m_lockValue)
            {
                zedGraphMins.PrintDocument.Print();
            }
        }

        private void сохранитьToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            lock (m_tecView.m_lockValue)
            {
                zedGraphMins.SaveAs();
            }
        }

        private void эксельToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            lock (m_tecView.m_lockValue)
            {
                SaveFileDialog sf = new SaveFileDialog();
                int hour = m_tecView.lastHour;
                if (hour == 24)
                    hour = 23;

                sf.CheckPathExists = true;
                sf.DefaultExt = ".xls";
                sf.Filter = "Файл Microsoft Excel (.xls) | *.xls";
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    ExcelFile ef = new ExcelFile();
                    ef.Worksheets.Add("Трёхминутные данные");
                    ExcelWorksheet ws = ef.Worksheets[0];
                    if (indx_TECComponent < 0)
                    {
                        if (m_tecView.m_tec.list_TECComponents.Count == 1)
                            ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr;
                        else
                        {
                            ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr;
                            foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
                                ws.Cells[0, 0].Value += ", " + g.name_shr;
                        }
                    }
                    else
                    {
                        ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr + ", " + m_tecView.m_tec.list_TECComponents[indx_TECComponent].name_shr;
                    }

                    if (m_tecView.m_valuesHours.addonValues && hour == m_tecView.m_valuesHours.hourAddon)
                        ws.Cells[1, 0].Value = "Мощность на " + (hour + 1).ToString() + "* час " + m_pnlQuickData.dtprDate.Value.ToShortDateString();
                    else
                        ws.Cells[1, 0].Value = "Мощность на " + (hour + 1).ToString() + " час " + m_pnlQuickData.dtprDate.Value.ToShortDateString();

                    ws.Cells[2, 0].Value = "Минута";
                    ws.Cells[2, 1].Value = "Факт";
                    ws.Cells[2, 2].Value = "ПБР";
                    ws.Cells[2, 3].Value = "ПБРэ";
                    ws.Cells[2, 4].Value = "УДГэ";
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
                            MessageBox.Show(this, "Не удалось сохранить файл.\nВозможно нет доступа, либо файл занят другим приложением.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void показыватьЗначенияToolStripMenuItemHours_Click(object sender, EventArgs e)
        {
            показыватьЗначенияToolStripMenuItemHours.Checked = !показыватьЗначенияToolStripMenuItemHours.Checked;
            zedGraphHours.IsShowPointValues = показыватьЗначенияToolStripMenuItemHours.Checked;
        }

        private void копироватьToolStripMenuItemHours_Click(object sender, EventArgs e)
        {
            lock (m_tecView.m_lockValue)
            {
                zedGraphHours.Copy(false);
            }
        }

        private void параметрыПечатиToolStripMenuItemHours_Click(object sender, EventArgs e)
        {
            PageSetupDialog pageSetupDialog = new PageSetupDialog();
            pageSetupDialog.Document = zedGraphHours.PrintDocument;
            pageSetupDialog.ShowDialog();
        }

        private void распечататьToolStripMenuItemHours_Click(object sender, EventArgs e)
        {
            lock (m_tecView.m_lockValue)
            {
                zedGraphHours.PrintDocument.Print();
            }
        }

        private void сохранитьToolStripMenuItemHours_Click(object sender, EventArgs e)
        {
            lock (m_tecView.m_lockValue)
            {
                zedGraphHours.SaveAs();
            }
        }

        private void эксельToolStripMenuItemHours_Click(object sender, EventArgs e)
        {
            lock (m_tecView.m_lockValue)
            {
                SaveFileDialog sf = new SaveFileDialog();
                sf.CheckPathExists = true;
                sf.DefaultExt = ".xls";
                sf.Filter = "Файл Microsoft Excel (.xls) | *.xls";
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    ExcelFile ef = new ExcelFile();
                    ef.Worksheets.Add("Часовые данные");
                    ExcelWorksheet ws = ef.Worksheets[0];
                    if (indx_TECComponent < 0)
                    {
                        if (m_tecView.m_tec.list_TECComponents.Count == 1)
                            ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr;
                        else
                        {
                            ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr;
                            foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
                                ws.Cells[0, 0].Value += ", " + g.name_shr;
                        }
                    }
                    else
                    {
                        ws.Cells[0, 0].Value = m_tecView.m_tec.name_shr + ", " + m_tecView.m_tec.list_TECComponents[indx_TECComponent].name_shr;
                    }

                    ws.Cells[1, 0].Value = "Мощность на " + m_pnlQuickData.dtprDate.Value.ToShortDateString();

                    ws.Cells[2, 0].Value = "Час";
                    ws.Cells[2, 1].Value = "Факт";
                    ws.Cells[2, 2].Value = "ПБР";
                    ws.Cells[2, 3].Value = "ПБРэ";
                    ws.Cells[2, 4].Value = "УДГэ";
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
                            MessageBox.Show(this, "Не удалось сохранить файл.\nВозможно нет доступа, либо файл занят другим приложением.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private bool zedGraphMins_MouseUpEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                //Выход, если событие не от "Левой" кн.
                return true;

            object obj;
            PointF p = new PointF(e.X, e.Y);
            bool found;
            int index;

            //Поиск объекта
            found = sender.GraphPane.FindNearestObject(p, CreateGraphics(), out obj, out index);

            if (!(obj is BarItem) && !(obj is LineItem))
                //Выход, если объект не "требуемого" типа
                return true;
            
            if (m_tecView.lastMin <= index + 1)
                //Выход, если выбранный объект находится "в будущем"
                return true;

            if (found == true)
            {
                //Пересчет, перерисовка панели с оперативной информацией с "выбранным" 3-х мин интервалом
                lock (m_tecView.m_lockValue)
                {
                    int prevLastMin = m_tecView.lastMin;
                    m_tecView.recalcAver = true;
                    m_tecView.lastMin = index + 2;
                    m_pnlQuickData.ShowFactValues();
                    //m_tecView.recalcAver = true;
                    m_tecView.lastMin = prevLastMin;
                }
            }
            else
                ;

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

            if (found == true)
            {
                delegateStartWait();

                m_tecView.zedGraphHours_MouseUpEvent(index);

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
                valuesFact[i] = m_tecView.m_valuesMins.valuesFact[i + 1];
                valuesUDGe[i] = m_tecView.m_valuesMins.valuesUDGe[i + 1];
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

                if (m_tecView.currHour == true)
                {
                    if ((i < (m_tecView.lastMin - 1)) || (! (m_tecView.adminValuesReceived == true)))
                        valuesRecommend[i] = 0;
                    else
                        valuesRecommend[i] = m_tecView.recomendation;
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

                if (m_tecView.currHour == true)
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

                if (m_tecView.currHour == true)
                {
                    if (maximum < valuesRecommend[i])
                        maximum = valuesRecommend[i];
                    else
                        ;
                }

                if (maximum < valuesUDGe[i])
                    maximum = valuesUDGe[i];
                else
                    ;

                if (maximum < valuesFact[i])
                    maximum = valuesFact[i];
                else
                    ;
            }

            if (! (FormMain.formGraphicsSettings.scale == true))
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

            pane.Chart.Fill = new Fill(FormMain.formGraphicsSettings.bgColor);

            LineItem curve2 = pane.AddCurve("УДГэ", null, valuesUDGe, FormMain.formGraphicsSettings.udgColor);
            //LineItem curve4 = pane.AddCurve("", null, valuesODiviation, graphSettings.divColor);
            //LineItem curve3 = pane.AddCurve("Возможное отклонение", null, valuesPDiviation, graphSettings.divColor);

            if (FormMain.formGraphicsSettings.graphTypes == FormGraphicsSettings.GraphTypes.Bar)
            {
                BarItem curve1 = pane.AddBar("Мощность", null, valuesFact, FormMain.formGraphicsSettings.pColor);

                BarItem curve0 = pane.AddBar("Рекомендуемая мощность", null, valuesRecommend, FormMain.formGraphicsSettings.recColor);
            }
            else
            {
                if (FormMain.formGraphicsSettings.graphTypes == FormGraphicsSettings.GraphTypes.Linear)
                {
                    if (m_tecView.lastMin > 1)
                    {
                        double[] valuesFactLast = new double[m_tecView.lastMin - 1];
                        for (int i = 0; i < m_tecView.lastMin - 1; i++)
                            valuesFactLast[i] = valuesFact[i];

                        LineItem curve1 = pane.AddCurve("Мощность", null, valuesFactLast, FormMain.formGraphicsSettings.pColor);

                        PointPairList valuesRecList = new PointPairList();
                        if ((m_tecView.adminValuesReceived == true) && (m_tecView.currHour == true))
                            for (int i = m_tecView.lastMin - 1; i < itemscount; i++)
                                valuesRecList.Add((double)(i + 1), valuesRecommend[i]);

                        LineItem curve0 = pane.AddCurve("Рекомендуемая мощность", valuesRecList, FormMain.formGraphicsSettings.recColor);
                    }
                    else
                    {
                        LineItem curve1 = pane.AddCurve("Мощность", null, null, FormMain.formGraphicsSettings.pColor);
                        LineItem curve0 = pane.AddCurve("Рекомендуемая мощность", null, valuesRecommend, FormMain.formGraphicsSettings.recColor);
                    }
                }
            }

            pane.BarSettings.Type = BarType.Overlay;

            pane.XAxis.Type = AxisType.Linear;

            pane.XAxis.Title.Text = "";
            pane.YAxis.Title.Text = "";

            if (m_tecView.m_valuesHours.addonValues && hour == m_tecView.m_valuesHours.hourAddon)
                pane.Title.Text = //"Средняя мощность на " + /*System.TimeZone.CurrentTimeZone.ToUniversalTime(*/dtprDate.Value/*)*/.ToShortDateString() + " " + 
                                    (hour + 1).ToString() + "* час";
            else
                pane.Title.Text = //"Средняя мощность на " + /*System.TimeZone.CurrentTimeZone.ToUniversalTime(*/dtprDate.Value/*)*/.ToShortDateString() + " " + 
                                    (hour + 1).ToString() + " час";

            pane.XAxis.Scale.Min = 0.5;
            pane.XAxis.Scale.Max = 20.5;
            pane.XAxis.Scale.MinorStep = 1;
            pane.XAxis.Scale.MajorStep = 1;

            pane.XAxis.Scale.TextLabels = names;
            pane.XAxis.Scale.IsPreventLabelOverlap = false;

            // Включаем отображение сетки напротив крупных рисок по оси X
            pane.XAxis.MajorGrid.IsVisible = true;
            // Задаем вид пунктирной линии для крупных рисок по оси X:
            // Длина штрихов равна 10 пикселям, ... 
            pane.XAxis.MajorGrid.DashOn = 10;
            // затем 5 пикселей - пропуск
            pane.XAxis.MajorGrid.DashOff = 5;
            // толщина линий
            pane.XAxis.MajorGrid.PenWidth = 0.1F;
            pane.XAxis.MajorGrid.Color = FormMain.formGraphicsSettings.gridColor;

            // Включаем отображение сетки напротив крупных рисок по оси Y
            pane.YAxis.MajorGrid.IsVisible = true;
            // Аналогично задаем вид пунктирной линии для крупных рисок по оси Y
            pane.YAxis.MajorGrid.DashOn = 10;
            pane.YAxis.MajorGrid.DashOff = 5;
            // толщина линий
            pane.YAxis.MajorGrid.PenWidth = 0.1F;
            pane.YAxis.MajorGrid.Color = FormMain.formGraphicsSettings.gridColor;

            // Включаем отображение сетки напротив мелких рисок по оси Y
            pane.YAxis.MinorGrid.IsVisible = true;
            // Длина штрихов равна одному пикселю, ... 
            pane.YAxis.MinorGrid.DashOn = 1;
            pane.YAxis.MinorGrid.DashOff = 2;
            // толщина линий
            pane.YAxis.MinorGrid.PenWidth = 0.1F;
            pane.YAxis.MinorGrid.Color = FormMain.formGraphicsSettings.gridColor;


            // Устанавливаем интересующий нас интервал по оси Y
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

            if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.SummerToWinter)
                itemscount = 25;
            else
                if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.WinterToSummer)
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
                if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.SummerToWinter)
                {
                    if (i <= m_tecView.m_valuesHours.hourAddon)
                    {
                        names[i] = (i + 1).ToString();
                        valuesPDiviation[i] = m_tecView.m_valuesHours.valuesUDGe[i] + m_tecView.m_valuesHours.valuesDiviation[i];
                        valuesODiviation[i] = m_tecView.m_valuesHours.valuesUDGe[i] - m_tecView.m_valuesHours.valuesDiviation[i];
                        valuesUDGe[i] = m_tecView.m_valuesHours.valuesUDGe[i];
                        valuesFact[i] = m_tecView.m_valuesHours.valuesFact[i];
                    }
                    else
                        if (i == m_tecView.m_valuesHours.hourAddon + 1)
                        {
                            names[i] = i.ToString() + "*";
                            valuesPDiviation[i] = m_tecView.m_valuesHours.valuesUDGeAddon + m_tecView.m_valuesHours.valuesDiviationAddon;
                            valuesODiviation[i] = m_tecView.m_valuesHours.valuesUDGeAddon - m_tecView.m_valuesHours.valuesDiviationAddon;
                            valuesUDGe[i] = m_tecView.m_valuesHours.valuesUDGeAddon;
                            valuesFact[i] = m_tecView.m_valuesHours.valuesFactAddon;
                        }
                        else
                        {
                            this.m_dgwHours.Rows[i].Cells[0].Value = i.ToString();
                            names[i] = i.ToString();
                            valuesPDiviation[i] = m_tecView.m_valuesHours.valuesUDGe[i - 1] + m_tecView.m_valuesHours.valuesDiviation[i - 1];
                            valuesODiviation[i] = m_tecView.m_valuesHours.valuesUDGe[i - 1] - m_tecView.m_valuesHours.valuesDiviation[i - 1];
                            valuesUDGe[i] = m_tecView.m_valuesHours.valuesUDGe[i - 1];
                            valuesFact[i] = m_tecView.m_valuesHours.valuesFact[i - 1];
                        }

                }
                else
                    if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.WinterToSummer)
                    {
                        if (i < m_tecView.m_valuesHours.hourAddon)
                        {
                            names[i] = (i + 1).ToString();
                            valuesPDiviation[i] = m_tecView.m_valuesHours.valuesUDGe[i] + m_tecView.m_valuesHours.valuesDiviation[i];
                            valuesODiviation[i] = m_tecView.m_valuesHours.valuesUDGe[i] - m_tecView.m_valuesHours.valuesDiviation[i];
                            valuesUDGe[i] = m_tecView.m_valuesHours.valuesUDGe[i];
                            valuesFact[i] = m_tecView.m_valuesHours.valuesFact[i];
                        }
                        else
                        {
                            names[i] = (i + 2).ToString();
                            valuesPDiviation[i] = m_tecView.m_valuesHours.valuesUDGe[i + 1] + m_tecView.m_valuesHours.valuesDiviation[i + 1];
                            valuesODiviation[i] = m_tecView.m_valuesHours.valuesUDGe[i + 1] - m_tecView.m_valuesHours.valuesDiviation[i + 1];
                            valuesUDGe[i] = m_tecView.m_valuesHours.valuesUDGe[i + 1];
                            valuesFact[i] = m_tecView.m_valuesHours.valuesFact[i + 1];
                        }
                    }
                    else
                    {
                        names[i] = (i + 1).ToString();
                        valuesPDiviation[i] = m_tecView.m_valuesHours.valuesUDGe[i] + m_tecView.m_valuesHours.valuesDiviation[i];
                        valuesODiviation[i] = m_tecView.m_valuesHours.valuesUDGe[i] - m_tecView.m_valuesHours.valuesDiviation[i];
                        valuesUDGe[i] = m_tecView.m_valuesHours.valuesUDGe[i];
                        valuesFact[i] = m_tecView.m_valuesHours.valuesFact[i];
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

            if (! (FormMain.formGraphicsSettings.scale == true))
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

            pane.Chart.Fill = new Fill(FormMain.formGraphicsSettings.bgColor);

            LineItem curve2 = pane.AddCurve("УДГэ", null, valuesUDGe, FormMain.formGraphicsSettings.udgColor);
            LineItem curve4 = pane.AddCurve("", null, valuesODiviation, FormMain.formGraphicsSettings.divColor);
            LineItem curve3 = pane.AddCurve("Возможное отклонение", null, valuesPDiviation, FormMain.formGraphicsSettings.divColor);


            if (FormMain.formGraphicsSettings.graphTypes == FormGraphicsSettings.GraphTypes.Bar)
            {
                BarItem curve1 = pane.AddBar("Мощность", null, valuesFact, FormMain.formGraphicsSettings.pColor);
            }
            else
            {
                if (FormMain.formGraphicsSettings.graphTypes == FormGraphicsSettings.GraphTypes.Linear)
                {
                    int valuescount;

                    if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.SummerToWinter)
                        valuescount = m_tecView.lastHour + 1;
                    else
                        if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.WinterToSummer)
                            valuescount = m_tecView.lastHour - 1;
                        else
                            valuescount = m_tecView.lastHour;

                    double[] valuesFactNew = new double[valuescount];
                    for (int i = 0; i < valuescount; i++)
                        valuesFactNew[i] = valuesFact[i];

                    LineItem curve1 = pane.AddCurve("Мощность", null, valuesFactNew, FormMain.formGraphicsSettings.pColor);
                }
            }

            pane.XAxis.Type = AxisType.Text;
            pane.XAxis.Title.Text = "";
            pane.YAxis.Title.Text = "";
            pane.Title.Text = "Мощность на " + m_pnlQuickData.dtprDate.Value.ToShortDateString();

            pane.XAxis.Scale.TextLabels = names;
            pane.XAxis.Scale.IsPreventLabelOverlap = false;

            // Включаем отображение сетки напротив крупных рисок по оси X
            pane.XAxis.MajorGrid.IsVisible = true;
            // Задаем вид пунктирной линии для крупных рисок по оси X:
            // Длина штрихов равна 10 пикселям, ... 
            pane.XAxis.MajorGrid.DashOn = 10;
            // затем 5 пикселей - пропуск
            pane.XAxis.MajorGrid.DashOff = 5;
            // толщина линий
            pane.XAxis.MajorGrid.PenWidth = 0.1F;
            pane.XAxis.MajorGrid.Color = FormMain.formGraphicsSettings.gridColor;

            // Включаем отображение сетки напротив крупных рисок по оси Y
            pane.YAxis.MajorGrid.IsVisible = true;
            // Аналогично задаем вид пунктирной линии для крупных рисок по оси Y
            pane.YAxis.MajorGrid.DashOn = 10;
            pane.YAxis.MajorGrid.DashOff = 5;
            // толщина линий
            pane.YAxis.MajorGrid.PenWidth = 0.1F;
            pane.YAxis.MajorGrid.Color = FormMain.formGraphicsSettings.gridColor;

            // Включаем отображение сетки напротив мелких рисок по оси Y
            pane.YAxis.MinorGrid.IsVisible = true;
            // Длина штрихов равна одному пикселю, ... 
            pane.YAxis.MinorGrid.DashOn = 1;
            pane.YAxis.MinorGrid.DashOff = 2;
            // толщина линий
            pane.YAxis.MinorGrid.PenWidth = 0.1F;
            pane.YAxis.MinorGrid.Color = FormMain.formGraphicsSettings.gridColor;

            // Устанавливаем интересующий нас интервал по оси Y
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

            lock (m_tecView.m_lockValue)
            {
                DrawGraphHours();

                DrawGraphMins(hour);
            }
        }

        public void UpdateGraphicsCurrent()
        {
            DrawGraphHours();
            DrawGraphMins(m_tecView.lastHour);
        }

        private void stctrViewPanel1_SplitterMoved(object sender, SplitterEventArgs e)
        {
        }

        private void stctrViewPanel2_SplitterMoved(object sender, SplitterEventArgs e)
        {
        }
    }
}
