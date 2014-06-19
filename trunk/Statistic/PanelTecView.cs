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
    public class PanelTecView : PanelStatistic
    {
        protected static AdminTS.TYPE_FIELDS s_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;
                
        private System.Windows.Forms.Panel pnlGraphHours;
        private System.Windows.Forms.Panel pnlGraphMins;
        private PanelQuickData pnlQuickData;
        private System.Windows.Forms.DataGridView dgwHours;
        private System.Windows.Forms.DataGridViewTextBoxColumn Hour;
        private System.Windows.Forms.DataGridViewTextBoxColumn FactHour;
        private System.Windows.Forms.DataGridViewTextBoxColumn PBRHour;
        private System.Windows.Forms.DataGridViewTextBoxColumn PBReHour;
        private System.Windows.Forms.DataGridViewTextBoxColumn UDGeHour;
        private System.Windows.Forms.DataGridViewTextBoxColumn DeviationHour;
        private System.Windows.Forms.DataGridViewTextBoxColumn LastMinutes_TM;
        private System.Windows.Forms.DataGridView dgwMins;
        private System.Windows.Forms.DataGridViewTextBoxColumn Min;
        private System.Windows.Forms.DataGridViewTextBoxColumn FactMin;
        private System.Windows.Forms.DataGridViewTextBoxColumn PBRMin;
        private System.Windows.Forms.DataGridViewTextBoxColumn PBReMin;
        private System.Windows.Forms.DataGridViewTextBoxColumn UDGeMin;
        private System.Windows.Forms.DataGridViewTextBoxColumn DeviationMin;
        private System.Windows.Forms.SplitContainer stctrView, stctrViewPanel1, stctrViewPanel2;
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

        private DataGridViewCellStyle dgvCellStyleError;
        private DataGridViewCellStyle dgvCellStyleCommon;

        private DelegateFunc delegateStartWait;
        private DelegateFunc delegateStopWait;
        private DelegateFunc delegateEventUpdate;

        private DelegateBoolFunc delegateSetNowDate;

        private DelegateIntIntFunc delegateUpdateGUI_Fact;
        private DelegateFunc delegateUpdateGUI_TM;

        private object lockValue;
        
        private Thread taskThread;
        private Semaphore sem;
        private volatile bool threadIsWorking;
        private volatile bool newState;
        private volatile List<StatesMachine> states;
        private int currValuesPeriod = 0;
        private ManualResetEvent evTimerCurrent;
        private System.Threading.Timer timerCurrent;
        //private System.Windows.Forms.Timer timerCurrent;
        private DelegateFunc delegateTickTime;

        //private AdminTS m_admin;
        private FormGraphicsSettings graphSettings;
        private FormParameters parameters;        

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public volatile bool currHour;
        public volatile int lastHour;
        private volatile int lastReceivedHour;
        public volatile int lastMin;
        public volatile bool lastMinError;
        public volatile bool lastHourError;
        public volatile bool lastHourHalfError;
        public volatile string lastLayout;

        private enum StatesMachine
        {
            Init_Fact,
            Init_TM,
            CurrentTime,
            CurrentHours_Fact,
            CurrentMins_Fact,
            Current_TM,
            LastMinutes_TM,
            RetroHours,
            RetroMins,
            PBRValues,
            AdminValues,
        }

        public enum seasonJumpE
        {
            None,
            WinterToSummer,
            SummerToWinter,
        }

        public class valuesTECComponent
        {
            public volatile double[] valuesLastMinute;

            public volatile double[] valuesPBR;
            public volatile double[] valuesPBRe;
            public volatile double[] valuesUDGe;
            public volatile double[] valuesREC;
            public volatile double[] valuesISPER; //Признак ед.изм. 'valuesDIV'
            public volatile double[] valuesDIV; //Значение из БД
            public volatile double[] valuesDiviation; //Значение в ед.изм.

            public valuesTECComponent(int sz)
            {
                valuesLastMinute = new double[sz];

                valuesPBR = new double[sz];
                valuesPBRe = new double[sz];
                valuesUDGe = new double[sz];
                valuesREC = new double[sz];
                valuesISPER = new double[sz];
                valuesDIV = new double[sz];
                valuesDiviation = new double[sz];
            }
        }

        public class valuesTEC
        {
            public volatile double[] valuesFact;
            //public volatile double[] valuesCurrentTM;
            public volatile double[] valuesLastMinutesTM;
            public volatile double[] valuesPBR;
            public volatile double[] valuesPBRe;
            public volatile double[] valuesUDGe;
            public volatile double[] valuesDiviation;
            public double valuesFactAddon;
            public double valuesPBRAddon;
            public double valuesPBReAddon;
            public double valuesUDGeAddon;
            public double valuesDiviationAddon;
            public int hourAddon;
            public seasonJumpE season;
            public bool addonValues;

            public valuesTEC(int sz)
            {
                valuesFact = new double[sz];
                //valuesCurrentTM = new double[sz];
                valuesLastMinutesTM = new double[sz];
                valuesPBR = new double[sz];
                valuesPBRe = new double[sz];
                valuesUDGe = new double[sz];
                valuesDiviation = new double[sz];
                valuesFactAddon = 0.0;
                valuesPBRAddon = 0.0;
                valuesPBReAddon = 0.0;
                valuesUDGeAddon = 0.0;
                valuesDiviationAddon = 0.0;
                hourAddon = 0;
                season = seasonJumpE.None;
                addonValues = false;
            }
        }

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public valuesTEC m_valuesMins;
        public valuesTEC m_valuesHours;
        private DateTime selectedTime;
        private DateTime serverTime;

        DataTable m_tablePBRResponse;

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public double recomendation;

        private volatile string sensorsString_TM = "";
        private volatile string[] sensorsStrings_Fact = { "", "" }; //Только для особенной ТЭЦ (Бийск)

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public TG[] sensorId2TG;

        public volatile TEC tec;
        public volatile int num_TEC;
        public volatile int num_TECComponent;
        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public List <TECComponentBase> m_list_TECComponents;

        private int CountTG { get { return sensorId2TG.Length; } }
        private bool update;

        public volatile bool isActive;

        private StatusStrip stsStrip;

        private bool started;
        private HReports m_report;

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public volatile bool adminValuesReceived;

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public volatile bool recalcAver;

        private void InitializeComponent()
        {
            this.zedGraphMins = new ZedGraphControl();
            this.zedGraphHours = new ZedGraphControl();
            this.dgwHours = new System.Windows.Forms.DataGridView();
            this.Hour = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FactHour = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PBRHour = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PBReHour = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UDGeHour = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DeviationHour = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LastMinutes_TM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlQuickData = new PanelQuickData ();

            this.pnlGraphHours = new System.Windows.Forms.Panel();
            this.pnlGraphMins = new System.Windows.Forms.Panel();
            this.dgwMins = new System.Windows.Forms.DataGridView();
            this.Min = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FactMin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PBRMin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PBReMin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UDGeMin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DeviationMin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stctrView = new System.Windows.Forms.SplitContainer();
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

            ((System.ComponentModel.ISupportInitialize)(this.dgwHours)).BeginInit();
            this.pnlQuickData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgwMins)).BeginInit();
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
            this.RowStyles.Add(new RowStyle(SizeType.Percent, 86));
            this.Controls.Add(this.pnlQuickData, 0, 1);
            this.pnlQuickData.Initialize();
            this.RowStyles.Add(new RowStyle(SizeType.Percent, 14));
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.Location = arPlacement[(int)CONTROLS.THIS].pt;
            this.Name = "pnlTecView";
            //this.Size = arPlacement[(int)CONTROLS.THIS].sz;
            this.TabIndex = 0;
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
            // dgwHours
            // 
            this.dgwHours.AllowUserToAddRows = false;
            this.dgwHours.AllowUserToDeleteRows = false;
            //this.dgwHours.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            //this.dgwHours.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            //            | System.Windows.Forms.AnchorStyles.Left)));
            this.dgwHours.Dock = DockStyle.Fill;
            this.dgwHours.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgwHours.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Hour,
            this.FactHour,
            this.PBRHour,
            this.PBReHour,
            this.UDGeHour,
            this.DeviationHour,
            this.LastMinutes_TM});
            //this.dgwHours.Location = arPlacement[(int)CONTROLS.dgwHours].pt;
            this.dgwHours.Name = "dgwHour";
            this.dgwHours.ReadOnly = true;
            this.dgwHours.RowHeadersVisible = false;
            //this.dgwHours.Size = arPlacement[(int)CONTROLS.dgwHours].sz;
            this.dgwHours.TabIndex = 7;
            this.dgwHours.RowTemplate.Resizable = DataGridViewTriState.False;
            // 
            // Hour
            // 
            this.Hour.HeaderText = "Час";
            this.Hour.Name = "Hour";
            this.Hour.ReadOnly = true;
            this.Hour.Width = 25;
            this.Hour.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // FactHour
            // 
            this.FactHour.HeaderText = "Факт";
            this.FactHour.Name = "FactHour";
            this.FactHour.ReadOnly = true;
            this.FactHour.Width = 48;
            this.FactHour.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // PBRHour
            // 
            this.PBRHour.HeaderText = "ПБР";
            this.PBRHour.Name = "PBRHour";
            this.PBRHour.ReadOnly = true;
            this.PBRHour.Width = 48;
            this.PBRHour.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // PBReHour
            // 
            this.PBReHour.HeaderText = "ПБРэ";
            this.PBReHour.Name = "PBReHour";
            this.PBReHour.ReadOnly = true;
            this.PBReHour.Width = 48;
            this.PBReHour.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // UDGeHour
            // 
            this.UDGeHour.HeaderText = "УДГэ";
            this.UDGeHour.Name = "UDGeHour";
            this.UDGeHour.ReadOnly = true;
            this.UDGeHour.Width = 48;
            this.UDGeHour.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // DeviationHour
            // 
            this.DeviationHour.HeaderText = "+/-";
            this.DeviationHour.Name = "DeviationHour";
            this.DeviationHour.ReadOnly = true;
            this.DeviationHour.Width = 42;
            this.DeviationHour.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // LastMinutes_TM
            // 
            this.LastMinutes_TM.HeaderText = "Мин.59";
            this.LastMinutes_TM.Name = "LastMinutes_TM";
            this.LastMinutes_TM.ReadOnly = true;
            this.LastMinutes_TM.Width = 48;
            this.LastMinutes_TM.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;

            this.pnlQuickData.Dock = DockStyle.Fill;
            this.pnlQuickData.btnSetNow.Click += new System.EventHandler(this.btnSetNow_Click);
            this.pnlQuickData.dtprDate.ValueChanged += new System.EventHandler(this.dtprDate_ValueChanged);

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
            // dgwMins
            // 
            this.dgwMins.AllowUserToAddRows = false;
            this.dgwMins.AllowUserToDeleteRows = false;
            //this.dgwMins.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            //            | System.Windows.Forms.AnchorStyles.Left)));
            this.dgwMins.Dock = DockStyle.Fill;
            this.dgwMins.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgwMins.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Min,
            this.FactMin,
            this.PBRMin,
            this.PBReMin,
            this.UDGeMin,
            this.DeviationMin});
            //this.dgwMins.Location = arPlacement[(int)CONTROLS.dgwMins].pt;
            this.dgwMins.Name = "dgwMin";
            this.dgwMins.ReadOnly = true;
            this.dgwMins.RowHeadersVisible = false;
            //this.dgwMins.Size = arPlacement[(int)CONTROLS.dgwMins].sz;
            this.dgwMins.TabIndex = 0;
            this.dgwMins.RowTemplate.Resizable = DataGridViewTriState.False;
            // 
            // Min
            // 
            this.Min.HeaderText = "Мин";
            this.Min.Name = "Min";
            this.Min.ReadOnly = true;
            this.Min.Width = 50;
            this.Min.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // FactMin
            // 
            this.FactMin.HeaderText = "Факт";
            this.FactMin.Name = "FactMin";
            this.FactMin.ReadOnly = true;
            this.FactMin.Width = 50;
            this.FactMin.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // PBRMin
            // 
            this.PBRMin.HeaderText = "ПБР";
            this.PBRMin.Name = "PBRMin";
            this.PBRMin.ReadOnly = true;
            this.PBRMin.Width = 50;
            this.PBRMin.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // PBReMin
            // 
            this.PBReMin.HeaderText = "ПБРэ";
            this.PBReMin.Name = "PBReMin";
            this.PBReMin.ReadOnly = true;
            this.PBReMin.Width = 50;
            this.PBReMin.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // UDGeMin
            // 
            this.UDGeMin.HeaderText = "УДГэ";
            this.UDGeMin.Name = "UDGeMin";
            this.UDGeMin.ReadOnly = true;
            this.UDGeMin.Width = 60;
            this.UDGeMin.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // DeviationMin
            // 
            this.DeviationMin.HeaderText = "+/-";
            this.DeviationMin.Name = "DeviationMin";
            this.DeviationMin.ReadOnly = true;
            this.DeviationMin.Width = 50;
            this.DeviationMin.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
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
            this.stctrViewPanel1.Panel1.Controls.Add (this.dgwMins);
            this.stctrViewPanel1.Panel2.Controls.Add (this.pnlGraphMins);
            //this.stctrViewPanel1.SplitterDistance = 301;
            this.stctrViewPanel1.SplitterMoved +=new SplitterEventHandler(stctrViewPanel1_SplitterMoved);
            this.stctrView.Panel1.Controls.Add(this.stctrViewPanel1);
            // 
            // stctrView.Panel2
            // 
            this.stctrViewPanel2.Dock = DockStyle.Fill;
            this.stctrViewPanel2.Orientation = Orientation.Vertical;
            this.stctrViewPanel2.Panel1.Controls.Add(this.dgwHours);
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

            ((System.ComponentModel.ISupportInitialize)(this.dgwHours)).EndInit();
            this.pnlQuickData.ResumeLayout(false);
            this.pnlQuickData.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgwMins)).EndInit();
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

        public PanelTecView(TEC tec, int num_tec, int num_comp, StatusStrip sts, FormGraphicsSettings gs, FormParameters par, HReports rep)
        {
            this.tec = tec;
            this.num_TEC = num_tec;
            this.num_TECComponent = num_comp;
            m_report = rep;
            
            InitializeComponent();

            started = false;

            dgvCellStyleError = new DataGridViewCellStyle();
            dgvCellStyleError.BackColor = Color.Red;
            dgvCellStyleCommon = new DataGridViewCellStyle();

            //this.m_admin = admin;
            this.graphSettings = gs;
            this.parameters = par;

            if (tec.type () == TEC.TEC_TYPE.BIYSK)
                ; //this.parameters = FormMain.papar;
            else
                ;

            currHour = true;
            lastHour = 0;
            lastMin = 0;
            update = false;
            isActive = false;
            m_report.errored_state =
            m_report.actioned_state = false;
            recalcAver = true;

            lockValue = new object();

            m_valuesMins = new valuesTEC(21);
            m_valuesHours = new valuesTEC(24);

            stsStrip = sts;

            this.dgwMins.Rows.Add(21);
            this.dgwHours.Rows.Add(25);

            delegateSetNowDate = new DelegateBoolFunc(SetNowDate);

            delegateUpdateGUI_Fact = new DelegateIntIntFunc(UpdateGUI_Fact);
            delegateUpdateGUI_TM = new DelegateFunc(UpdateGUI_TM);
 
            delegateTickTime = new DelegateFunc(TickTime);

            states = new List<StatesMachine>();
        }        

        public void SetDelegate(DelegateFunc dStart, DelegateFunc dStop, DelegateFunc dStatus)
        {
            this.delegateStartWait = dStart;
            this.delegateStopWait = dStop;
            this.delegateEventUpdate = dStatus;
        }

        private void FillDefaultMins()
        {
            for (int i = 0; i < 20; i++)
            {
                this.dgwMins.Rows[i].Cells[0].Value = ((i + 1) * 3).ToString();
                this.dgwMins.Rows[i].Cells[1].Value = 0.ToString("F2");
                this.dgwMins.Rows[i].Cells[2].Value = 0.ToString("F2");
                this.dgwMins.Rows[i].Cells[3].Value = 0.ToString("F2");
                this.dgwMins.Rows[i].Cells[4].Value = 0.ToString("F2");
                this.dgwMins.Rows[i].Cells[5].Value = 0.ToString("F2");
            }
            this.dgwMins.Rows[20].Cells[0].Value = "Итог";
            this.dgwMins.Rows[20].Cells[1].Value = 0.ToString("F2");
            this.dgwMins.Rows[20].Cells[2].Value = "-";
            this.dgwMins.Rows[20].Cells[3].Value = "-";
            this.dgwMins.Rows[20].Cells[4].Value = 0.ToString("F2");
            this.dgwMins.Rows[20].Cells[5].Value = 0.ToString("F2");
        }

        private void FillDefaultHours()
        {
            int count;
            
            this.dgwHours.Rows.Clear();

            if (m_valuesHours.season == seasonJumpE.SummerToWinter)
                count = 25;
            else
                if (m_valuesHours.season == seasonJumpE.WinterToSummer)
                    count = 23;
                else
                    count = 24;

            this.dgwHours.Rows.Add(count + 1);

            for (int i = 0; i < count; i++)
            {
                if (m_valuesHours.season == seasonJumpE.SummerToWinter)
                {
                    if (i <= m_valuesHours.hourAddon)
                        this.dgwHours.Rows[i].Cells[0].Value = (i + 1).ToString();
                    else
                        if (i == m_valuesHours.hourAddon + 1)
                            this.dgwHours.Rows[i].Cells[0].Value = i.ToString() + "*";
                        else
                            this.dgwHours.Rows[i].Cells[0].Value = i.ToString();
                }
                else
                    if (m_valuesHours.season == seasonJumpE.WinterToSummer)
                    {
                        if (i < m_valuesHours.hourAddon)
                            this.dgwHours.Rows[i].Cells[0].Value = (i + 1).ToString();
                        else
                            this.dgwHours.Rows[i].Cells[0].Value = (i + 2).ToString();
                    }
                    else
                        this.dgwHours.Rows[i].Cells[0].Value = (i + 1).ToString();

                this.dgwHours.Rows[i].Cells[1].Value = 0.ToString("F2");
                this.dgwHours.Rows[i].Cells[2].Value = 0.ToString("F2");
                this.dgwHours.Rows[i].Cells[3].Value = 0.ToString("F2");
                this.dgwHours.Rows[i].Cells[4].Value = 0.ToString("F2");
                this.dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
            }

            this.dgwHours.Rows[count].Cells[0].Value = "Сумма";
            this.dgwHours.Rows[count].Cells[1].Value = 0.ToString("F2");
            this.dgwHours.Rows[count].Cells[2].Value = "-";
            this.dgwHours.Rows[count].Cells[3].Value = "-";
            this.dgwHours.Rows[count].Cells[4].Value = 0.ToString("F2");
            this.dgwHours.Rows[count].Cells[5].Value = 0.ToString("F2");
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

            LineItem curve2 = pane.AddCurve("УДГэ", null, valuesUDGe, graphSettings.udgColor);
            //LineItem curve4 = pane.AddCurve("", null, valuesODiviation, graphSettings.divColor);
            //LineItem curve3 = pane.AddCurve("Возможное отклонение", null, valuesPDiviation, graphSettings.divColor);

            if (graphSettings.graphTypes == FormGraphicsSettings.GraphTypes.Bar)
            {
                BarItem curve1 = pane.AddBar("Мощность", null, valuesFact, graphSettings.pColor);

                BarItem curve0 = pane.AddBar("Рекомендуемая мощность", null, valuesRecommend, graphSettings.recColor);
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

                        LineItem curve1 = pane.AddCurve("Мощность", null, valuesFactLast, graphSettings.pColor);

                        PointPairList valuesRecList = new PointPairList();
                        if (adminValuesReceived && currHour)
                            for (int i = lastMin - 1; i < itemscount; i++)
                                valuesRecList.Add((double)(i + 1), valuesRecommend[i]);

                        LineItem curve0 = pane.AddCurve("Рекомендуемая мощность", valuesRecList, graphSettings.recColor);
                    }
                    else
                    {
                        LineItem curve1 = pane.AddCurve("Мощность", null, null, graphSettings.pColor);
                        LineItem curve0 = pane.AddCurve("Рекомендуемая мощность", null, valuesRecommend, graphSettings.recColor);
                    }
                }
            }

            pane.BarSettings.Type = BarType.Overlay;

            pane.XAxis.Type = AxisType.Linear;

            pane.XAxis.Title.Text = "";
            pane.YAxis.Title.Text = "";
            
            if (m_valuesHours.addonValues && hour == m_valuesHours.hourAddon)
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
            pane.XAxis.MajorGrid.Color = graphSettings.gridColor;

            // Включаем отображение сетки напротив крупных рисок по оси Y
            pane.YAxis.MajorGrid.IsVisible = true;
            // Аналогично задаем вид пунктирной линии для крупных рисок по оси Y
            pane.YAxis.MajorGrid.DashOn = 10;
            pane.YAxis.MajorGrid.DashOff = 5;
            // толщина линий
            pane.YAxis.MajorGrid.PenWidth = 0.1F;
            pane.YAxis.MajorGrid.Color = graphSettings.gridColor;

            // Включаем отображение сетки напротив мелких рисок по оси Y
            pane.YAxis.MinorGrid.IsVisible = true;
            // Длина штрихов равна одному пикселю, ... 
            pane.YAxis.MinorGrid.DashOn = 1;
            pane.YAxis.MinorGrid.DashOff = 2;
            // толщина линий
            pane.YAxis.MinorGrid.PenWidth = 0.1F;
            pane.YAxis.MinorGrid.Color = graphSettings.gridColor;


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
                            this.dgwHours.Rows[i].Cells[0].Value = i.ToString();
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

            LineItem curve2 = pane.AddCurve("УДГэ", null, valuesUDGe, graphSettings.udgColor);
            LineItem curve4 = pane.AddCurve("", null, valuesODiviation, graphSettings.divColor);
            LineItem curve3 = pane.AddCurve("Возможное отклонение", null, valuesPDiviation, graphSettings.divColor);


            if (graphSettings.graphTypes == FormGraphicsSettings.GraphTypes.Bar)
            {
                BarItem curve1 = pane.AddBar("Мощность", null, valuesFact, graphSettings.pColor);
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

                    LineItem curve1 = pane.AddCurve("Мощность", null, valuesFactNew, graphSettings.pColor);
                }
            }

            pane.XAxis.Type = AxisType.Text;
            pane.XAxis.Title.Text = "";
            pane.YAxis.Title.Text = "";
            pane.Title.Text = "Мощность на " + pnlQuickData.dtprDate.Value.ToShortDateString();

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
            pane.XAxis.MajorGrid.Color = graphSettings.gridColor;

            // Включаем отображение сетки напротив крупных рисок по оси Y
            pane.YAxis.MajorGrid.IsVisible = true;
            // Аналогично задаем вид пунктирной линии для крупных рисок по оси Y
            pane.YAxis.MajorGrid.DashOn = 10;
            pane.YAxis.MajorGrid.DashOff = 5;
            // толщина линий
            pane.YAxis.MajorGrid.PenWidth = 0.1F;
            pane.YAxis.MajorGrid.Color = graphSettings.gridColor;

            // Включаем отображение сетки напротив мелких рисок по оси Y
            pane.YAxis.MinorGrid.IsVisible = true;
            // Длина штрихов равна одному пикселю, ... 
            pane.YAxis.MinorGrid.DashOn = 1;
            pane.YAxis.MinorGrid.DashOff = 2;
            // толщина линий
            pane.YAxis.MinorGrid.PenWidth = 0.1F;
            pane.YAxis.MinorGrid.Color = graphSettings.gridColor;

            // Устанавливаем интересующий нас интервал по оси Y
            pane.YAxis.Scale.Min = minimum_scale;
            pane.YAxis.Scale.Max = maximum_scale;

            zedGraphHours.AxisChange();

            zedGraphHours.Invalidate();
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
                lock (lockValue)
                {
                    int oldLastMin = lastMin;
                    recalcAver = false;
                    lastMin = index + 2;
                    pnlQuickData.ShowFactValues();
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
                lock (lockValue)
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

                    newState = true;
                    states.Clear();
                    states.Add(StatesMachine.RetroMins);
                    states.Add(StatesMachine.PBRValues);
                    states.Add(StatesMachine.AdminValues);

                    try
                    {
                        sem.Release(1);
                    }
                    catch (Exception excpt) { Logging.Logg().LogExceptionToFile(excpt, "catch - zedGraphHours_MouseUpEvent () - sem.Release(1)"); }

                }
                delegateStopWait();
            }

            return true;
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

        private void показыватьЗначенияToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            показыватьЗначенияToolStripMenuItemMins.Checked = !показыватьЗначенияToolStripMenuItemMins.Checked;
            zedGraphMins.IsShowPointValues = показыватьЗначенияToolStripMenuItemMins.Checked;
        }

        private void копироватьToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            lock (lockValue)
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
            lock (lockValue)
            {
                zedGraphMins.PrintDocument.Print();
            }
        }

        private void сохранитьToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            lock (lockValue)
            {
                zedGraphMins.SaveAs();
            }
        }

        private void эксельToolStripMenuItemMins_Click(object sender, EventArgs e)
        {
            lock (lockValue)
            {
                SaveFileDialog sf = new SaveFileDialog();
                int hour = lastHour;
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
                    if (num_TECComponent < 0)
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
                        ws.Cells[0, 0].Value = tec.name_shr + ", " + tec.list_TECComponents[num_TECComponent].name_shr;
                    }

                    if (m_valuesHours.addonValues && hour == m_valuesHours.hourAddon)
                        ws.Cells[1, 0].Value = "Мощность на " + (hour + 1).ToString() + "* час " + pnlQuickData.dtprDate.Value.ToShortDateString();
                    else
                        ws.Cells[1, 0].Value = "Мощность на " + (hour + 1).ToString() + " час " + pnlQuickData.dtprDate.Value.ToShortDateString();

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
                        valid = int.TryParse((string)dgwMins.Rows[i].Cells[0].Value, out res_int);
                        if (valid)
                            ws.Cells[3 + i, 0].Value = res_int;
                        else
                            ws.Cells[3 + i, 0].Value = dgwMins.Rows[i].Cells[0].Value;

                        valid = double.TryParse((string)dgwMins.Rows[i].Cells[1].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 1].Value = res_double;
                        else
                            ws.Cells[3 + i, 1].Value = dgwMins.Rows[i].Cells[1].Value;

                        valid = double.TryParse((string)dgwMins.Rows[i].Cells[2].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 2].Value = res_double;
                        else
                            ws.Cells[3 + i, 2].Value = dgwMins.Rows[i].Cells[2].Value;

                        valid = double.TryParse((string)dgwMins.Rows[i].Cells[3].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 3].Value = res_double;
                        else
                            ws.Cells[3 + i, 3].Value = dgwMins.Rows[i].Cells[3].Value;

                        valid = double.TryParse((string)dgwMins.Rows[i].Cells[4].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 4].Value = res_double;
                        else
                            ws.Cells[3 + i, 4].Value = dgwMins.Rows[i].Cells[4].Value;

                        valid = double.TryParse((string)dgwMins.Rows[i].Cells[5].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 5].Value = res_double;
                        else
                            ws.Cells[3 + i, 5].Value = dgwMins.Rows[i].Cells[5].Value;
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
            lock (lockValue)
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
            lock (lockValue)
            {
                zedGraphHours.PrintDocument.Print();
            }
        }

        private void сохранитьToolStripMenuItemHours_Click(object sender, EventArgs e)
        {
            lock (lockValue)
            {
                zedGraphHours.SaveAs();
            }
        }

        private void эксельToolStripMenuItemHours_Click(object sender, EventArgs e)
        {
            lock (lockValue)
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
                    if (num_TECComponent < 0)
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
                        ws.Cells[0, 0].Value = tec.name_shr + ", " + tec.list_TECComponents[num_TECComponent].name_shr;
                    }

                    ws.Cells[1, 0].Value = "Мощность на " + pnlQuickData.dtprDate.Value.ToShortDateString();

                    ws.Cells[2, 0].Value = "Час";
                    ws.Cells[2, 1].Value = "Факт";
                    ws.Cells[2, 2].Value = "ПБР";
                    ws.Cells[2, 3].Value = "ПБРэ";
                    ws.Cells[2, 4].Value = "УДГэ";
                    ws.Cells[2, 5].Value = "+/-";

                    bool valid;
                    double res_double;
                    int res_int;

                    for (int i = 0; i < dgwHours.Rows.Count; i++)
                    {
                        valid = int.TryParse((string)dgwHours.Rows[i].Cells[0].Value, out res_int);
                        if (valid)
                            ws.Cells[3 + i, 0].Value = res_int;
                        else
                            ws.Cells[3 + i, 0].Value = dgwHours.Rows[i].Cells[0].Value;

                        valid = double.TryParse((string)dgwHours.Rows[i].Cells[1].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 1].Value = res_double;
                        else
                            ws.Cells[3 + i, 1].Value = dgwHours.Rows[i].Cells[1].Value;

                        valid = double.TryParse((string)dgwHours.Rows[i].Cells[2].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 2].Value = res_double;
                        else
                            ws.Cells[3 + i, 2].Value = dgwHours.Rows[i].Cells[2].Value;

                        valid = double.TryParse((string)dgwHours.Rows[i].Cells[3].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 3].Value = res_double;
                        else
                            ws.Cells[3 + i, 3].Value = dgwHours.Rows[i].Cells[3].Value;

                        valid = double.TryParse((string)dgwHours.Rows[i].Cells[4].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 4].Value = res_double;
                        else
                            ws.Cells[3 + i, 4].Value = dgwHours.Rows[i].Cells[4].Value;

                        valid = double.TryParse((string)dgwHours.Rows[i].Cells[5].Value, out res_double);
                        if (valid)
                            ws.Cells[3 + i, 5].Value = res_double;
                        else
                            ws.Cells[3 + i, 5].Value = dgwHours.Rows[i].Cells[5].Value;
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

        public void Start()
        {
            if (started)
                return;

            adminValuesReceived = false;
            currHour = true;
            currValuesPeriod = parameters.m_arParametrSetup [(int)FormParameters.PARAMETR_SETUP.POLL_TIME] - 1;
            started = true;

            tec.StartDbInterfaces();
            threadIsWorking = true;

            taskThread = new Thread(new ParameterizedThreadStart(TecView_ThreadFunction));
            taskThread.Name = "Интерфейс к данным";
            taskThread.IsBackground = true;

            sem = new Semaphore(1, 1);

            sem.WaitOne();
            taskThread.Start();

            ClearValues();

            DrawGraphMins(0);
            DrawGraphHours();

            FillDefaultMins();
            FillDefaultHours();

            DaylightTime daylight = TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year);
            if (TimeZone.IsDaylightSavingTime(DateTime.Now, daylight))
                selectedTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(3 + 1);
            else
                selectedTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(3);

            serverTime = selectedTime;

            evTimerCurrent = new ManualResetEvent (true);
            timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), evTimerCurrent, 0, Timeout.Infinite);
            
            //timerCurrent = new System.Windows.Forms.Timer ();
            //timerCurrent.Tick += TimerCurrent_Tick;

            update = false;
            SetNowDate(true);
        }

        public void Reinit()
        {
            if (!started)
                return;

            tec.RefreshConnectionSettings();

            if (!isActive)
                return;

            NewDateRefresh();
        }

        public override void Stop()
        {
            if (started == false)
                return;
            else
                ;

            evTimerCurrent.Reset ();
            timerCurrent.Dispose();

            started = false;
            bool joined;
            threadIsWorking = false;
            lock (lockValue)
            {
                newState = true;
                states.Clear();
            }

            if (taskThread.IsAlive)
            {
                try
                {
                    sem.Release(1);
                }
                catch (Exception excpt) { Logging.Logg().LogExceptionToFile(excpt, "catch - PanelTecView.Stop () - sem.Release(1)"); }

                joined = taskThread.Join(1000);
                if (!joined)
                    taskThread.Abort();
                else
                    ;
            }

            tec.StopDbInterfaces();

            m_report.errored_state = false;
        }

        private void UpdateGUI_TM()
        {
            lock (lockValue)
            {
                pnlQuickData.ShowTMValues();
            }
        }

        private void UpdateGUI_Fact(int hour, int min)
        {
            lock (lockValue)
            {
                FillGridHours();
                DrawGraphHours();

                FillGridMins(hour);
                DrawGraphMins(hour);

                pnlQuickData.ShowFactValues();
            }
        }

        private void GetCurrentTimeRequest()
        {
            tec.Request(CONN_SETT_TYPE.DATA_FACT, @"SELECT getdate()");
        }

        private void GetSensorsFactRequest()
        {
            tec.Request(CONN_SETT_TYPE.DATA_FACT, tec.sensorsFactRequest());
        }

        private void GetSensorsTMRequest()
        {
            tec.Request(CONN_SETT_TYPE.DATA_TM, tec.sensorsTMRequest());
        }

        private void GetHoursRequest(DateTime date)
        {
            tec.Request(CONN_SETT_TYPE.DATA_FACT, tec.hoursRequest(date, sensorsStrings_Fact[(int)TG.ID_TIME.HOURS]));
        }

        private void GetMinsRequest(int hour)
        {
            tec.Request(CONN_SETT_TYPE.DATA_FACT, tec.minsRequest(selectedTime, hour, sensorsStrings_Fact[(int)TG.ID_TIME.MINUTES]));
        }

        private void GetCurrentTMRequest()
        {
            tec.Request(CONN_SETT_TYPE.DATA_TM, tec.currentTMRequest(sensorsString_TM));
        }

        private void GetLastMinutesTMRequest()
        {
            tec.Request(CONN_SETT_TYPE.DATA_TM, tec.lastMinutesTMRequest(DateTime.Now.Date, sensorsString_TM));
        }

        private void GetPBRValuesRequest () {
            //m_admin.Request(tec.m_arIdListeners[(int)CONN_SETT_TYPE.PBR], tec.GetPBRValueQuery(num_TECComponent, pnlQuickData.dtprDate.Value.Date, m_admin.m_typeFields));
            tec.Request(CONN_SETT_TYPE.PBR, tec.GetPBRValueQuery(num_TECComponent, pnlQuickData.dtprDate.Value.Date, s_typeFields));
        }

        private void GetAdminValuesRequest (AdminTS.TYPE_FIELDS mode) {
            //m_admin.Request(tec.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], tec.GetAdminValueQuery(num_TECComponent, pnlQuickData.dtprDate.Value.Date, mode));
            tec.Request(CONN_SETT_TYPE.ADMIN, tec.GetAdminValueQuery(num_TECComponent, pnlQuickData.dtprDate.Value.Date, mode));
        }

        private void FillGridMins(int hour)
        {
            double sumFact = 0, sumUDGe = 0, sumDiviation = 0;
            int min = lastMin;
            if (min != 0)
                min--;
            for (int i = 0; i < m_valuesMins.valuesFact.Length - 1; i++)
            {
                dgwMins.Rows[i].Cells[1].Value = m_valuesMins.valuesFact[i + 1].ToString("F2");
                sumFact += m_valuesMins.valuesFact[i + 1];

                dgwMins.Rows[i].Cells[2].Value = m_valuesMins.valuesPBR[i].ToString("F2");
                dgwMins.Rows[i].Cells[3].Value = m_valuesMins.valuesPBRe[i].ToString("F2");
                dgwMins.Rows[i].Cells[4].Value = m_valuesMins.valuesUDGe[i].ToString("F2");
                sumUDGe += m_valuesMins.valuesUDGe[i];
                if (i < min && m_valuesMins.valuesUDGe[i] != 0)
                {
                    dgwMins.Rows[i].Cells[5].Value = ((double)(m_valuesMins.valuesFact[i + 1] - m_valuesMins.valuesUDGe[i])).ToString("F2");
                    //if (Math.Abs(m_valuesMins.valuesFact[i + 1] - m_valuesMins.valuesUDGe[i]) > m_valuesMins.valuesDiviation[i]
                    //    && m_valuesMins.valuesDiviation[i] != 0)
                    //    dgwMins.Rows[i].Cells[5].Style = dgvCellStyleError;
                    //else
                    dgwMins.Rows[i].Cells[5].Style = dgvCellStyleCommon;

                    sumDiviation += m_valuesMins.valuesFact[i + 1] - m_valuesMins.valuesUDGe[i];
                }
                else
                {
                    dgwMins.Rows[i].Cells[5].Value = 0.ToString("F2");
                    dgwMins.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                }
            }
            if (min <= 0)
            {
                dgwMins.Rows[20].Cells[1].Value = 0.ToString("F2");
                dgwMins.Rows[20].Cells[4].Value = 0.ToString("F2");
                dgwMins.Rows[20].Cells[5].Value = 0.ToString("F2");
            }
            else
            {
                if (min > 20)
                    min = 20;
                dgwMins.Rows[20].Cells[1].Value = (sumFact / min).ToString("F2");
                dgwMins.Rows[20].Cells[4].Value = m_valuesMins.valuesUDGe[0].ToString("F2");
                dgwMins.Rows[20].Cells[5].Value = (sumDiviation / min).ToString("F2");
            }
        }

        private void FillGridHours()
        {
            FillDefaultHours();

            double sumFact = 0, sumUDGe = 0, sumDiviation = 0,
                    dblPercent = 0.0, dblUDGe = 0.0;
            int hour = lastHour;
            int receivedHour = lastReceivedHour;
            int itemscount;

            if (m_valuesHours.season == seasonJumpE.SummerToWinter)
            {
                itemscount = 25;
            }
            else
                if (m_valuesHours.season == seasonJumpE.WinterToSummer)
                {
                    itemscount = 23;
                }
                else
                {
                    itemscount = 24;
                }

            for (int i = 0; i < itemscount; i++)
            {
                dgwHours.Rows[i].Cells[6].Value = m_valuesHours.valuesLastMinutesTM[i].ToString("F2");
                dblUDGe = m_valuesHours.valuesUDGe[i];
                if ((m_valuesHours.valuesLastMinutesTM[i] > 1) && (dblUDGe > 1))
                    dblPercent = (m_valuesHours.valuesLastMinutesTM[i] - dblUDGe) / dblUDGe * 100;
                else
                    ;

                if ((dblUDGe > 1) &&
                   (m_valuesHours.valuesLastMinutesTM[i] > 1) &&
                   ((!(Math.Abs(dblPercent) < 2))))
                    dgwHours.Rows[i].Cells[6].Style = dgvCellStyleError;
                else
                    dgwHours.Rows[i].Cells[6].Style = dgvCellStyleCommon;

                if (m_valuesHours.season == seasonJumpE.SummerToWinter)
                {
                    if (i <= m_valuesHours.hourAddon)
                    {
                        dgwHours.Rows[i].Cells[1].Value = m_valuesHours.valuesFact[i].ToString("F2");
                        sumFact += m_valuesHours.valuesFact[i];

                        dgwHours.Rows[i].Cells[2].Value = m_valuesHours.valuesPBR[i].ToString("F2");
                        dgwHours.Rows[i].Cells[3].Value = m_valuesHours.valuesPBRe[i].ToString("F2");
                        dgwHours.Rows[i].Cells[4].Value = m_valuesHours.valuesUDGe[i].ToString("F2");
                        sumUDGe += m_valuesHours.valuesUDGe[i];
                        if (i < receivedHour && m_valuesHours.valuesUDGe[i] != 0)
                        {
                            dgwHours.Rows[i].Cells[5].Value = ((double)(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i])).ToString("F2");
                            if (Math.Abs(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i]) > m_valuesHours.valuesDiviation[i]
                                && m_valuesHours.valuesDiviation[i] != 0)
                                dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                            else
                                dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            sumDiviation += Math.Abs(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i]);
                        }
                        else
                        {
                            dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                            dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                        }
                    }
                    else
                        if (i == m_valuesHours.hourAddon + 1)
                        {
                            dgwHours.Rows[i].Cells[1].Value = m_valuesHours.valuesFactAddon.ToString("F2");
                            sumFact += m_valuesHours.valuesFactAddon;

                            dgwHours.Rows[i].Cells[2].Value = m_valuesHours.valuesPBRAddon.ToString("F2");
                            dgwHours.Rows[i].Cells[3].Value = m_valuesHours.valuesPBReAddon.ToString("F2");
                            dgwHours.Rows[i].Cells[4].Value = m_valuesHours.valuesUDGeAddon.ToString("F2");
                            sumUDGe += m_valuesHours.valuesUDGeAddon;
                            if (i <= receivedHour && m_valuesHours.valuesUDGeAddon != 0)
                            {
                                dgwHours.Rows[i].Cells[5].Value = ((double)(m_valuesHours.valuesFactAddon - m_valuesHours.valuesUDGeAddon)).ToString("F2");
                                if (Math.Abs(m_valuesHours.valuesFactAddon - m_valuesHours.valuesUDGeAddon) > m_valuesHours.valuesDiviationAddon
                                    && m_valuesHours.valuesDiviationAddon != 0)
                                    dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                                else
                                    dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                                sumDiviation += Math.Abs(m_valuesHours.valuesFactAddon - m_valuesHours.valuesUDGeAddon);
                            }
                            else
                            {
                                dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                                dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            }
                        }
                        else
                        {
                            dgwHours.Rows[i].Cells[1].Value = m_valuesHours.valuesFact[i - 1].ToString("F2");
                            sumFact += m_valuesHours.valuesFact[i - 1];

                            dgwHours.Rows[i].Cells[2].Value = m_valuesHours.valuesPBR[i - 1].ToString("F2");
                            dgwHours.Rows[i].Cells[3].Value = m_valuesHours.valuesPBRe[i - 1].ToString("F2");
                            dgwHours.Rows[i].Cells[4].Value = m_valuesHours.valuesUDGe[i - 1].ToString("F2");
                            sumUDGe += m_valuesHours.valuesUDGe[i - 1];
                            if (i <= receivedHour && m_valuesHours.valuesUDGe[i - 1] != 0)
                            {
                                dgwHours.Rows[i].Cells[5].Value = ((double)(m_valuesHours.valuesFact[i - 1] - m_valuesHours.valuesUDGe[i - 1])).ToString("F2");
                                if (Math.Abs(m_valuesHours.valuesFact[i - 1] - m_valuesHours.valuesUDGe[i - 1]) > m_valuesHours.valuesDiviation[i - 1]
                                    && m_valuesHours.valuesDiviation[i - 1] != 0)
                                    dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                                else
                                    dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                                sumDiviation += Math.Abs(m_valuesHours.valuesFact[i - 1] - m_valuesHours.valuesUDGe[i - 1]);
                            }
                            else
                            {
                                dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                                dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            }
                        }

                }
                else
                    if (m_valuesHours.season == seasonJumpE.WinterToSummer)
                    {
                        if (i < m_valuesHours.hourAddon)
                        {
                            dgwHours.Rows[i].Cells[1].Value = m_valuesHours.valuesFact[i].ToString("F2");
                            sumFact += m_valuesHours.valuesFact[i];

                            dgwHours.Rows[i].Cells[2].Value = m_valuesHours.valuesPBR[i].ToString("F2");
                            dgwHours.Rows[i].Cells[3].Value = m_valuesHours.valuesPBRe[i].ToString("F2");
                            dgwHours.Rows[i].Cells[4].Value = m_valuesHours.valuesUDGe[i].ToString("F2");
                            sumUDGe += m_valuesHours.valuesUDGe[i];
                            if (i < receivedHour && m_valuesHours.valuesUDGe[i] != 0)
                            {
                                dgwHours.Rows[i].Cells[5].Value = ((double)(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i])).ToString("F2");
                                if (Math.Abs(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i]) > m_valuesHours.valuesDiviation[i]
                                    && m_valuesHours.valuesDiviation[i] != 0)
                                    dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                                else
                                    dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                                sumDiviation += Math.Abs(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i]);
                            }
                            else
                            {
                                dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                                dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            }
                        }
                        else
                        {
                            dgwHours.Rows[i].Cells[1].Value = m_valuesHours.valuesFact[i + 1].ToString("F2");
                            sumFact += m_valuesHours.valuesFact[i + 1];

                            dgwHours.Rows[i].Cells[2].Value = m_valuesHours.valuesPBR[i + 1].ToString("F2");
                            dgwHours.Rows[i].Cells[3].Value = m_valuesHours.valuesPBRe[i + 1].ToString("F2");
                            dgwHours.Rows[i].Cells[4].Value = m_valuesHours.valuesUDGe[i + 1].ToString("F2");
                            sumUDGe += m_valuesHours.valuesUDGe[i + 1];
                            if (i < receivedHour - 1 && m_valuesHours.valuesUDGe[i + 1] != 0)
                            {
                                dgwHours.Rows[i].Cells[5].Value = ((double)(m_valuesHours.valuesFact[i + 1] - m_valuesHours.valuesUDGe[i + 1])).ToString("F2");
                                if (Math.Abs(m_valuesHours.valuesFact[i + 1] - m_valuesHours.valuesUDGe[i + 1]) > m_valuesHours.valuesDiviation[i + 1]
                                    && m_valuesHours.valuesDiviation[i + 1] != 0)
                                    dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                                else
                                    dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                                sumDiviation += Math.Abs(m_valuesHours.valuesFact[i + 1] - m_valuesHours.valuesUDGe[i + 1]);
                            }
                            else
                            {
                                dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                                dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            }
                        }
                    }
                    else
                    {
                        dgwHours.Rows[i].Cells[1].Value = m_valuesHours.valuesFact[i].ToString("F2");
                        sumFact += m_valuesHours.valuesFact[i];

                        dgwHours.Rows[i].Cells[2].Value = m_valuesHours.valuesPBR[i].ToString("F2");
                        dgwHours.Rows[i].Cells[3].Value = m_valuesHours.valuesPBRe[i].ToString("F2");
                        dgwHours.Rows[i].Cells[4].Value = m_valuesHours.valuesUDGe[i].ToString("F2");
                        sumUDGe += m_valuesHours.valuesUDGe[i];
                        if (i < receivedHour && m_valuesHours.valuesUDGe[i] != 0)
                        {
                            dgwHours.Rows[i].Cells[5].Value = ((double)(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i])).ToString("F2");
                            if (Math.Abs(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i]) > m_valuesHours.valuesDiviation[i]
                                && m_valuesHours.valuesDiviation[i] != 0)
                                dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                            else
                                dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            sumDiviation += Math.Abs(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i]);
                        }
                        else
                        {
                            dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                            dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                        }
                    }
            }
            dgwHours.Rows[itemscount].Cells[1].Value = sumFact.ToString("F2");
            dgwHours.Rows[itemscount].Cells[4].Value = sumUDGe.ToString("F2");
            dgwHours.Rows[itemscount].Cells[5].Value = sumDiviation.ToString("F2");
        }

        private void ClearValues()
        {
            ClearValuesMins();
            ClearValuesHours();
        }

        private void ClearValuesMins()
        {
            for (int i = 0; i < 21; i++)
                m_valuesMins.valuesFact[i] =
                m_valuesMins.valuesDiviation[i] =
                m_valuesMins.valuesPBR[i] =
                m_valuesMins.valuesPBRe[i] =
                m_valuesMins.valuesUDGe[i] = 0;
        }

        private void ClearValuesHours()
        {
            for (int i = 0; i < 24; i++)
                m_valuesHours.valuesFact[i] =
                m_valuesHours.valuesLastMinutesTM[i] =
                m_valuesHours.valuesDiviation[i] =
                m_valuesHours.valuesPBR[i] =
                m_valuesHours.valuesPBRe[i] =
                m_valuesHours.valuesUDGe[i] = 0;
            
            m_valuesHours.valuesFactAddon =
            m_valuesHours.valuesDiviationAddon =
            m_valuesHours.valuesPBRAddon =
            m_valuesHours.valuesPBReAddon =
            m_valuesHours.valuesUDGeAddon = 0;
            m_valuesHours.season = seasonJumpE.None;
            m_valuesHours.hourAddon = 0;
            m_valuesHours.addonValues = false;
        }

        private void ClearPBRValues () {
        }

        private void ClearAdminValues()
        {
            for (int i = 0; i < 24; i++)
                m_valuesHours.valuesDiviation[i] =
                m_valuesHours.valuesPBR[i] =
                m_valuesHours.valuesPBRe[i] =
                m_valuesHours.valuesUDGe[i] = 0;

            m_valuesHours.valuesDiviationAddon =
            m_valuesHours.valuesPBRAddon =
            m_valuesHours.valuesPBReAddon =
            m_valuesHours.valuesUDGeAddon = 0;
            
            for (int i = 0; i < 21; i++)
                m_valuesMins.valuesDiviation[i] =
                m_valuesMins.valuesPBR[i] =
                m_valuesMins.valuesPBRe[i] =
                m_valuesMins.valuesUDGe[i] = 0;
        }

        private TG FindTGById(int id, TG.INDEX_VALUE indxVal, TG.ID_TIME id_type)
        {
            for (int i = 0; i < sensorId2TG.Length; i++)
                switch (indxVal)
                {
                    case TG.INDEX_VALUE.FACT:
                        if (sensorId2TG[i].ids_fact[(int)id_type] == id)
                            return sensorId2TG[i];
                        else
                            ;
                        break;
                    case TG.INDEX_VALUE.TM:
                        if (sensorId2TG[i].id_tm == id)
                            return sensorId2TG[i];
                        else
                            ;
                        break;
                    default:
                        break;
                }

            return null;
        }

        private bool GetCurrentTimeReponse(DataTable table)
        {
            if (table.Rows.Count == 1)
            {
                try
                {
                    selectedTime = (DateTime)table.Rows[0][0];
                    serverTime = selectedTime;
                }
                catch (Exception excpt)
                {
                    Logging.Logg().LogExceptionToFile(excpt, "PanelTecView::GetCurrentTimeReponse () - (DateTime)table.Rows[0][0]");

                    return false;
                }
            }
            else
            {
                //selectedTime = System.TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(3 + 1);
                //ErrorReport("Ошибка получения текущего времени сервера. Используется локальное время.");
                return false;
            }

            return true;
        }

        private void ErrorReportSensors(ref DataTable src)
        {
            string error = "Ошибка определения идентификаторов датчиков в строке ";
            for (int j = 0; j < src.Rows.Count; j++)
                error += src.Rows[j][0].ToString() + " = " + src.Rows[j][1].ToString() + ", ";

            error = error.Substring(0, error.LastIndexOf(","));
            ErrorReport(error);
        }

        private bool GetSensorsFactResponse(DataTable table)
        {
            string s;
            int t = 0;

            for (int i = 0; i < table.Rows.Count; i++)
            {
                //Шаблон для '[0](["NAME"])' 'ТГ%P%+'
                //Формирование правильное имя турбиногенератора
                s = TEC.getNameTG(tec.m_strTemplateNameSgnDataFact, table.Rows[i][0].ToString().ToUpper());

                if (num_TECComponent < 0)
                {//ТЭЦ в полном составе
                    int j, k;
                    for (j = 0; j < tec.list_TECComponents.Count; j++)
                    {
                        for (k = 0; k < tec.list_TECComponents[j].TG.Count; k++)
                        {
                            if (tec.list_TECComponents[j].TG[k].name_shr.Equals(s) == true)
                            {
                                tec.list_TECComponents[j].TG[k].ids_fact[(int)TG.ID_TIME.MINUTES] =
                                tec.list_TECComponents[j].TG[k].ids_fact[(int)TG.ID_TIME.HOURS] =
                                    int.Parse(table.Rows[i][1].ToString());

                                sensorId2TG[t] = tec.list_TECComponents[j].TG[k];
                                t++;

                                //Прерывание внешнего цикла
                                j = tec.list_TECComponents.Count;
                                break;
                            }
                            else
                                ;
                        }
                    }
                }
                else
                {// Для не ТЭЦ в полном составе (ГТП, ЩУ, ТГ)
                    for (int k = 0; k < tec.list_TECComponents[num_TECComponent].TG.Count; k++)
                    {
                        if (tec.list_TECComponents[num_TECComponent].TG[k].name_shr == s)
                        {
                            tec.list_TECComponents[num_TECComponent].TG[k].ids_fact[(int)TG.ID_TIME.MINUTES] =
                            tec.list_TECComponents[num_TECComponent].TG[k].ids_fact[(int)TG.ID_TIME.HOURS] =
                                int.Parse(table.Rows[i][1].ToString());

                            sensorId2TG[t] = tec.list_TECComponents[num_TECComponent].TG[k];

                            t++;
                            break;
                        }
                        else
                            ;
                    }
                }
            }

            for (int i = 0; i < sensorId2TG.Length; i++)
            {
                if (!(sensorId2TG[i] == null))
                {
                    if (sensorsStrings_Fact[(int)TG.ID_TIME.MINUTES].Equals(string.Empty) == true)
                    {
                        sensorsStrings_Fact[(int)TG.ID_TIME.MINUTES] = "SENSORS.ID = " + sensorId2TG[i].ids_fact[(int)TG.ID_TIME.MINUTES/*HOURS*/].ToString();
                    }
                    else
                    {
                        sensorsStrings_Fact[(int)TG.ID_TIME.MINUTES] += " OR SENSORS.ID = " + sensorId2TG[i].ids_fact[(int)TG.ID_TIME.MINUTES/*HOURS*/].ToString();
                    }
                }
                else
                {
                    ErrorReportSensors(ref table);

                    return false;
                }
            }

            //Для обычной ТЭЦ - идентификаторы мин., час одинаковы
            sensorsStrings_Fact[(int)TG.ID_TIME.HOURS] = sensorsStrings_Fact[(int)TG.ID_TIME.MINUTES];

            return true;
        }

        private bool GetSensorsTMResponse(DataTable table)
        {
            bool bRes = true;

            string s;
            for (int i = 0; i < table.Rows.Count; i++)
            {
                //Шаблон для '[0](["NAME"])'
                //Формирование правильное имя турбиногенератора
                s = TEC.getNameTG(tec.m_strTemplateNameSgnDataTM, table.Rows[i]["NAME"].ToString().ToUpper());

                if (num_TECComponent < 0)
                {//ТЭЦ в полном составе
                    int j = -1;
                    for (j = 0; j < tec.list_TECComponents.Count; j++)
                    {
                        int k = -1;
                        for (k = 0; k < tec.list_TECComponents[j].TG.Count; k++)
                        {
                            if (tec.list_TECComponents[j].TG[k].name_shr.Equals(s) == true)
                            {
                                tec.list_TECComponents[j].TG[k].id_tm = int.Parse(table.Rows[i]["ID"].ToString());
                                //Прерывание внешнего цикла
                                j = tec.list_TECComponents.Count;
                                break;
                            }
                            else
                                ;
                        }
                    }
                }
                else
                {// Для ТЭЦ НЕ в полном составе (ГТП, ЩУ, ТГ)
                    int k = -1;
                    for (k = 0; k < tec.list_TECComponents[num_TECComponent].TG.Count; k++)
                    {
                        if (tec.list_TECComponents[num_TECComponent].TG[k].name_shr.Equals(s) == true)
                        {
                            tec.list_TECComponents[num_TECComponent].TG[k].id_tm = int.Parse(table.Rows[i]["ID"].ToString());
                            break;
                        }
                        else
                            ;
                    }
                }
            }

            sensorsString_TM = string.Empty;

            for (int i = 0; i < sensorId2TG.Length; i++)
            {
                if (!(sensorId2TG[i] == null))
                {
                    if (sensorsString_TM.Equals(string.Empty) == false)
                        switch (TEC.m_typeSourceTM) {
                            case TEC.INDEX_TYPE_SOURCE_TM.COMMON:
                                //Общий источник для всех ТЭЦ
                                sensorsString_TM += @", "; //@" OR ";
                                break;
                            case TEC.INDEX_TYPE_SOURCE_TM.INDIVIDUAL:
                                //Источник для каждой ТЭЦ свой
                                sensorsString_TM += @" OR ";
                                break;
                            default:
                                break;
                        }
                    else
                        ;

                    switch (TEC.m_typeSourceTM)
                    {
                        case TEC.INDEX_TYPE_SOURCE_TM.COMMON:
                            //Общий источник для всех ТЭЦ
                            sensorsString_TM += sensorId2TG[i].id_tm.ToString();
                            break;
                        case TEC.INDEX_TYPE_SOURCE_TM.INDIVIDUAL:
                            //Источник для каждой ТЭЦ свой
                            sensorsString_TM += @"[dbo].[NAME_TABLE].[ID] = " + sensorId2TG[i].id_tm.ToString();
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    ErrorReportSensors(ref table);

                    return false;
                }
            }

            return bRes;
        }

        private void GetSensors()
        {
            Dictionary<string, int>[] tgs = new Dictionary<string, int>[(int)TG.ID_TIME.COUNT_ID_TIME];
            tgs[(int)TG.ID_TIME.MINUTES] = new Dictionary<string, int>();
            tgs[(int)TG.ID_TIME.HOURS] = new Dictionary<string, int>();

            int t = 0;
            if (num_TECComponent < 0)
            {
                int indxTG = -1;
                for (TG.ID_TIME i = TG.ID_TIME.MINUTES; i < TG.ID_TIME.COUNT_ID_TIME; i++)
                {
                    indxTG = 0;
                    for (int j = 0; j < m_list_TECComponents.Count; j++)
                    {
                        for (int k = 0; k < ((TECComponent)m_list_TECComponents[j]).TG.Count; k++, indxTG++)
                        {
                            tgs[(int)i].Add(((TECComponent)m_list_TECComponents[j]).TG[k].name_shr.ToString(), tec.parametersTGForm.ParamsGetTgId(i, indxTG));
                        }
                    }
                }

                for (int i = 0; i < m_list_TECComponents.Count; i++)
                {
                    for (int j = 0; j < ((TECComponent)m_list_TECComponents[i]).TG.Count; j++)
                    {
                        ((TECComponent)m_list_TECComponents[i]).TG[j].ids_fact[(int)TG.ID_TIME.MINUTES] = tgs[(int)TG.ID_TIME.MINUTES][tec.list_TECComponents[i].TG[j].name_shr];
                        ((TECComponent)m_list_TECComponents[i]).TG[j].ids_fact[(int)TG.ID_TIME.HOURS] = tgs[(int)TG.ID_TIME.HOURS][tec.list_TECComponents[i].TG[j].name_shr];
                        sensorId2TG[t] = ((TECComponent)m_list_TECComponents[i]).TG[j];
                        //sensorId2TGHours[t] = tec.list_TECComponents[i].TG[j];
                        t++;

                        if (sensorsStrings_Fact[(int)TG.ID_TIME.MINUTES] == "")
                            sensorsStrings_Fact[(int)TG.ID_TIME.MINUTES] = ((TECComponent)m_list_TECComponents[i]).TG[j].ids_fact[(int)TG.ID_TIME.MINUTES].ToString();
                        else
                            sensorsStrings_Fact[(int)TG.ID_TIME.MINUTES] += ", " + ((TECComponent)m_list_TECComponents[i]).TG[j].ids_fact[(int)TG.ID_TIME.MINUTES].ToString();

                        if (sensorsStrings_Fact[(int)TG.ID_TIME.HOURS] == "")
                            sensorsStrings_Fact[(int)TG.ID_TIME.HOURS] = ((TECComponent)m_list_TECComponents[i]).TG[j].ids_fact[(int)TG.ID_TIME.HOURS].ToString();
                        else
                            sensorsStrings_Fact[(int)TG.ID_TIME.HOURS] += ", " + ((TECComponent)m_list_TECComponents[i]).TG[j].ids_fact[(int)TG.ID_TIME.HOURS].ToString();
                    }
                }
            }
            else
            {
                int indxTG = -1;
                List<int> listIdTGTEC = new List<int>();
                for (TG.ID_TIME i = TG.ID_TIME.MINUTES; i < TG.ID_TIME.COUNT_ID_TIME; i++)
                {
                    indxTG = 0;
                    for (int j = 0; j < tec.list_TECComponents.Count; j++)
                    {
                        for (int k = 0; k < ((TECComponent)tec.list_TECComponents[j]).TG.Count; k++)
                        {
                            if (listIdTGTEC.IndexOf(((TECComponent)tec.list_TECComponents[j]).TG[k].m_id) < 0)
                                listIdTGTEC.Add(((TECComponent)tec.list_TECComponents[j]).TG[k].m_id);
                            else
                                ;
                        }
                    }
                }

                for (TG.ID_TIME i = TG.ID_TIME.MINUTES; i < TG.ID_TIME.COUNT_ID_TIME; i++)
                    for (int j = 0; j < m_list_TECComponents.Count; j++)
                    {
                        indxTG = listIdTGTEC.IndexOf(m_list_TECComponents[j].m_id);
                        tgs[(int)i].Add(m_list_TECComponents[j].name_shr.ToString(), tec.parametersTGForm.ParamsGetTgId(i, indxTG));
                    }

                for (int i = 0; i < m_list_TECComponents.Count; i++)
                {
                    ((TG)m_list_TECComponents[i]).ids_fact[(int)TG.ID_TIME.MINUTES] = tgs[(int)TG.ID_TIME.MINUTES][((TG)m_list_TECComponents[i]).name_shr];
                    ((TG)m_list_TECComponents[i]).ids_fact[(int)TG.ID_TIME.HOURS] = tgs[(int)TG.ID_TIME.HOURS][((TG)m_list_TECComponents[i]).name_shr];
                    sensorId2TG[t] = ((TG)m_list_TECComponents[i]);
                    //sensorId2TGHours[t] = tec.list_TECComponents[num_gtp].TG[i];
                    t++;

                    if (sensorsStrings_Fact[(int)TG.ID_TIME.MINUTES].Equals(string.Empty) == true)
                        sensorsStrings_Fact[(int)TG.ID_TIME.MINUTES] = ((TG)m_list_TECComponents[i]).ids_fact[(int)TG.ID_TIME.MINUTES].ToString();
                    else
                        sensorsStrings_Fact[(int)TG.ID_TIME.MINUTES] += ", " + ((TG)m_list_TECComponents[i]).ids_fact[(int)TG.ID_TIME.MINUTES].ToString();

                    if (sensorsStrings_Fact[(int)TG.ID_TIME.HOURS].Equals(string.Empty) == true)
                        sensorsStrings_Fact[(int)TG.ID_TIME.HOURS] = ((TG)m_list_TECComponents[i]).ids_fact[(int)TG.ID_TIME.HOURS].ToString();
                    else
                        sensorsStrings_Fact[(int)TG.ID_TIME.HOURS] += ", " + ((TG)m_list_TECComponents[i]).ids_fact[(int)TG.ID_TIME.HOURS].ToString();
                }
            }
        }

        private void GetSeason(DateTime date, int db_season, out int season)
        {
            season = db_season - date.Year - date.Year;
            if (season < 0)
                season = 0;
            else
                if (season > 2)
                    season = 2;
                else
                    ;
        }
        
        private bool GetHoursResponse(DataTable table)
        {
            int i, j, half, hour = 0, halfAddon;
            double hourVal = 0, halfVal = 0, value, hourValAddon = 0;
            double[] oldValuesTG = new double[CountTG];
            int[] oldIdTG = new int[CountTG];
            int id;
            TG tgTmp;
            bool end = false;
            DateTime dt, dtNeeded;
            int season = 0, prev_season = 0;
            bool jump_forward = false, jump_backward = false;

            lastHour = lastReceivedHour = 0;
            half = 0;
            halfAddon = 0;

            /*Form2 f2 = new Form2();
            f2.FillHourTable(table);*/

            lastHourHalfError = lastHourError = false;

            foreach (TECComponent g in tec.list_TECComponents)
            {
                foreach (TG t in g.TG)
                {
                    for (i = 0; i < t.power.Length; i++)
                    {
                        t.receivedHourHalf1[i] = t.receivedHourHalf2[i] = false;
                    }
                }
            }

            if (table.Rows.Count > 0)
            {
                //if (!DateTime.TryParse(table.Rows[0][6].ToString(), out dt))
                if (!DateTime.TryParse(table.Rows[0][1].ToString(), out dt))
                    return false;
                //if (!int.TryParse(table.Rows[0][8].ToString(), out season))
                if (!int.TryParse(table.Rows[0][2].ToString(), out season))
                    return false;
                GetSeason(dt, season, out season);
                prev_season = season;
                hour = dt.Hour;
                dtNeeded = dt;
                if (dt.Minute == 0)
                    half++;
            }
            else
            {
                if (currHour)
                {
                    if (selectedTime.Hour != 0)
                    {
                        lastHour = lastReceivedHour = selectedTime.Hour;
                        lastHourError = true;
                    }
                }
                /*f2.FillHourValues(lastHour, selectedTime, m_valuesHours.valuesFact);
                f2.ShowDialog();*/
                return true;
            }

            for (i = 0; hour < 24 && !end; )
            {
                if (half == 2 || halfAddon == 2) // прошёл один час
                {
                    if (!jump_backward)
                    {
                        if (jump_forward)
                            m_valuesHours.hourAddon = hour; // уточнить
                        m_valuesHours.valuesFact[hour] = hourVal / 2000;
                        hour++;
                        half = 0;
                        hourVal = 0;
                    }
                    else
                    {
                        m_valuesHours.valuesFactAddon = hourValAddon / 2000;
                        m_valuesHours.hourAddon = hour - 1;
                        hourValAddon = 0;
                        prev_season = season;
                        halfAddon++;
                    }
                    lastHour = lastReceivedHour = hour;
                }

                halfVal = 0;

                jump_forward = false;
                jump_backward = false;

                for (j = 0; j < CountTG; j++, i++)
                {
                    if (i >= table.Rows.Count)
                    {
                        end = true;
                        break;
                    }

                    if (!DateTime.TryParse(table.Rows[i][1].ToString(), out dt))
                        return false;

                    if (!int.TryParse(table.Rows[i][2].ToString(), out season))
                        return false;

                    if (dt.CompareTo(dtNeeded) != 0)
                    {
                        GetSeason(dt, season, out season);
                        if (dt.CompareTo(dtNeeded.AddMinutes(-30)) == 0 && prev_season == 1 && season == 2)
                        {
                            dtNeeded = dtNeeded.AddMinutes(-30);
                            jump_backward = true;
                        }
                        else
                            if (dt.CompareTo(dtNeeded.AddMinutes(30)) == 0 && prev_season == 0 && season == 1)
                            {
                                jump_forward = true;
                                break;
                            }
                            else
                                break;
                    }

                    //if (!int.TryParse(table.Rows[i][7].ToString(), out id))
                    if (!int.TryParse(table.Rows[i][0].ToString(), out id))
                        return false;

                    tgTmp = FindTGById(id, TG.INDEX_VALUE.FACT, TG.ID_TIME.HOURS);

                    if (tgTmp == null)
                        return false;

                    //if (!double.TryParse(table.Rows[i][5].ToString(), out value))
                    if (!double.TryParse(table.Rows[i][3].ToString(), out value))
                        return false;
                    else
                        ;

                    switch (tec.type ()) {
                        case TEC.TEC_TYPE.COMMON:
                            break;
                        case TEC.TEC_TYPE.BIYSK:
                            value *= 2;
                            break;
                        default:
                            break;
                    }

                    halfVal += value;
                    if (!jump_backward)
                    {
                        if (half == 0)
                            tgTmp.receivedHourHalf1[hour] = true;
                        else
                            tgTmp.receivedHourHalf2[hour] = true;
                    }
                    else
                    {
                        if (halfAddon == 0)
                            tgTmp.receivedHourHalf1Addon = true;
                        else
                            tgTmp.receivedHourHalf2Addon = true;
                    }
                }

                dtNeeded = dtNeeded.AddMinutes(30);

                if (!jump_backward)
                {
                    if (jump_forward)
                        m_valuesHours.season = seasonJumpE.WinterToSummer;

                    if (!end)
                        half++;

                    hourVal += halfVal;
                }
                else
                {
                    m_valuesHours.season = seasonJumpE.SummerToWinter;
                    m_valuesHours.addonValues = true;

                    if (!end)
                        halfAddon++;

                    hourValAddon += halfVal;
                }
            }

            /*f2.FillHourValues(lastHour, selectedTime, m_valuesHours.valuesFact);
            f2.ShowDialog();*/

            if (currHour)
            {
                if (lastHour < selectedTime.Hour)
                {
                    lastHourError = true;
                    lastHour = selectedTime.Hour;
                }
                else
                {
                    if (selectedTime.Hour == 0 && lastHour != 24 && dtNeeded.Date != selectedTime.Date)
                    {
                        lastHourError = true;
                        lastHour = 24;
                    }
                    else
                    {
                        if (lastHour != 0)
                        {
                            for (i = 0; i < sensorId2TG.Length; i++)
                            {
                                if ((half & 1) == 1)
                                {
                                    //MessageBox.Show("sensor " + sensorId2TG[i].name + ", h1 " + sensorId2TG[i].receivedHourHalf1[lastHour - 1].ToString());
                                    if (!sensorId2TG[i].receivedHourHalf1[lastHour - 1])
                                    {
                                        lastHourHalfError = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    //MessageBox.Show("sensor " + sensorId2TG[i].name + ", h2 " + sensorId2TG[i].receivedHourHalf2[lastHour - 1].ToString());
                                    if (!sensorId2TG[i].receivedHourHalf2[lastHour - 1])
                                    {
                                        lastHourHalfError = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            lastReceivedHour = lastHour;

            return true;
        }

        private bool GetCurrentTMResponse(DataTable table)
        {
            bool bRes = true;
            int i = -1,
                id = -1;
            double value = -1;
            TG tgTmp;

            foreach (TECComponent g in tec.list_TECComponents)
            {
                foreach (TG t in g.TG)
                {
                    for (i = 0; i < t.power.Length; i++)
                    {
                        t.power_TM = 0;
                    }
                }
            }

            for (i = 0; i < table.Rows.Count; i++)
            {
                if (int.TryParse(table.Rows[i]["ID"].ToString(), out id) == false)
                    return false;
                else
                    ;

                tgTmp = FindTGById(id, TG.INDEX_VALUE.TM, (TG.ID_TIME)(-1));

                if (tgTmp == null)
                    return false;
                else
                    ;

                if (double.TryParse(table.Rows[i]["value"].ToString(), out value) == false)
                    return false;
                else
                    ;

                switch (tec.type())
                {
                    case TEC.TEC_TYPE.COMMON:
                        break;
                    case TEC.TEC_TYPE.BIYSK:
                        //value *= 20;
                        break;
                    default:
                        break;
                }

                tgTmp.power_TM = value;
            }

            return bRes;
        }

        private bool GetLastMinutesTMResponse(DataTable table_in)
        {
            bool bRes = true;
            int i = -1,
                hour = -1,
                offsetUTC = (int)HAdmin.GetUTCOffsetOfCurrentTimeZone().TotalHours;
            double value = -1;
            DateTime dtVal;
            DataRow[] tgRows = null;

            if (num_TECComponent < 0)
            {
                foreach (TECComponent g in m_list_TECComponents)
                {
                    foreach (TG tg in g.TG)
                    {
                        for (i = 0; i < tg.power_LastMinutesTM.Length; i++)
                        {
                            tg.power_LastMinutesTM[i] = 0;
                        }

                        tgRows = table_in.Select(@"[ID]=" + tg.id_tm);

                        for (i = 0; i < tgRows.Length; i++)
                        {
                            if (double.TryParse(tgRows[i]["value"].ToString(), out value) == false)
                                return false;
                            else
                                ;

                            if (DateTime.TryParse(tgRows[i]["last_changed_at"].ToString(), out dtVal) == false)
                                return false;
                            else
                                ;

                            hour = dtVal.Hour + offsetUTC + 1; //Т.к. мин.59 из прошедшего часа
                            if (!(hour < 24)) hour -= 24; else ;

                            tg.power_LastMinutesTM[hour] = value;

                            //Запрос с учетом значения перехода через сутки
                            if (hour > 0)
                                m_valuesHours.valuesLastMinutesTM[hour - 1] += value;
                            else
                                ;
                        }
                    }
                }
            }
            else
            {
                foreach (TG tg in m_list_TECComponents)
                {
                    for (i = 0; i < tg.power_LastMinutesTM.Length; i++)
                    {
                        tg.power_LastMinutesTM[i] = 0;
                    }

                    tgRows = table_in.Select(@"[ID]=" + tg.id_tm);

                    for (i = 0; i < tgRows.Length; i++)
                    {
                        if (double.TryParse(tgRows[i]["value"].ToString(), out value) == false)
                            return false;
                        else
                            ;

                        if (DateTime.TryParse(tgRows[i]["last_changed_at"].ToString(), out dtVal) == false)
                            return false;
                        else
                            ;

                        hour = dtVal.Hour + offsetUTC + 1;
                        if (!(hour < 24)) hour -= 24; else ;

                        tg.power_LastMinutesTM[hour] = value;

                        if (hour > 0)
                            m_valuesHours.valuesLastMinutesTM[hour - 1] += value;
                        else
                            ;
                    }
                }
            }

            return bRes;
        }

        private bool GetMinsResponse(DataTable table)
        {
            int i, j = 0, min = 0;
            double minVal = 0, value;
            TG tgTmp;
            int id;
            bool end = false;
            DateTime dt, dtNeeded;
            int season = 0, need_season = 0, max_season = 0;
            bool jump = false;

            lastMinError = false;

            /*Form2 f2 = new Form2();
            f2.FillMinTable(table);*/

            foreach (TECComponent g in tec.list_TECComponents)
            {
                foreach (TG t in g.TG)
                {
                    for (i = 0; i < t.power.Length; i++)
                    {
                        t.power[i] = 0;
                        t.receivedMin[i] = false;
                    }
                }
            }

            lastMin = 0;

            if (table.Rows.Count > 0)
            {
                if (!DateTime.TryParse(table.Rows[0][1].ToString(), out dt))
                    return false;
                if (!int.TryParse(table.Rows[0][2].ToString(), out season))
                    return false;
                need_season = max_season = season;
                min = (int)(dt.Minute / 3);
                dtNeeded = dt;
            }
            else
            {
                if (currHour)
                {
                    if ((selectedTime.Minute / 3) != 0)
                    {
                        lastMinError = true;
                        lastMin = ((selectedTime.Minute) / 3) + 1;
                    }
                }
                /*f2.FillMinValues(lastMin, selectedTime, m_valuesMins.valuesFact);
                f2.ShowDialog();*/
                return true;
            }

            for (i = 0; i < table.Rows.Count; i++)
            {
                if (!int.TryParse(table.Rows[i][2].ToString(), out season))
                    return false;
                if (season > max_season)
                    max_season = season;
            }

            if (currHour)
            {
                if (need_season != max_season)
                {
                    m_valuesHours.addonValues = true;
                    m_valuesHours.hourAddon = lastHour - 1;
                    need_season = max_season;
                }
            }
            else
            {
                if (m_valuesHours.addonValues)
                {
                    need_season = max_season;
                }
            }

            for (i = 0; !end && min < 21; min++)
            {
                if (jump)
                {
                    min--;
                }
                else
                {
                    m_valuesMins.valuesFact[min] = 0;
                    minVal = 0;
                }

                /*MessageBox.Show("min " + min.ToString() + ", lastMin " + lastMin.ToString() + ", i " + i.ToString() +
                                 ", table.Rows.Count " + table.Rows.Count.ToString());*/
                jump = false;
                for (j = 0; j < CountTG; j++, i++)
                {
                    if (i >= table.Rows.Count)
                    {
                        end = true;
                        break;
                    }

                    if (!DateTime.TryParse(table.Rows[i][1].ToString(), out dt))
                        return false;
                    if (!int.TryParse(table.Rows[i][2].ToString(), out season))
                        return false;

                    if (season != need_season)
                    {
                        jump = true;
                        i++;
                        break;
                    }

                    if (dt.CompareTo(dtNeeded) != 0)
                    {
                        break;
                    }

                    if (!int.TryParse(table.Rows[i][0].ToString(), out id))
                        return false;

                    tgTmp = FindTGById(id, TG.INDEX_VALUE.FACT, (int)TG.ID_TIME.MINUTES);

                    if (tgTmp == null)
                        return false;

                    if (!double.TryParse(table.Rows[i][3].ToString(), out value))
                        return false;
                    else
                        ;

                    switch (tec.type())
                    {
                        case TEC.TEC_TYPE.COMMON:
                            break;
                        case TEC.TEC_TYPE.BIYSK:
                            value *= 20;
                            break;
                        default:
                            break;
                    }

                    minVal += value;
                    tgTmp.power[min] = value / 1000;
                    tgTmp.receivedMin[min] = true;
                }

                if (!jump)
                {
                    dtNeeded = dtNeeded.AddMinutes(3);

                    //MessageBox.Show("end " + end.ToString() + ", minVal " + (minVal / 1000).ToString());

                    if (!end)
                    {
                        m_valuesMins.valuesFact[min] = minVal / 1000;
                        lastMin = min + 1;
                    }
                }
            }

            /*f2.FillMinValues(lastMin, selectedTime, m_valuesMins.valuesFact);
            f2.ShowDialog();*/

            if (lastMin <= ((selectedTime.Minute - 1) / 3))
            {
                lastMinError = true;
                lastMin = ((selectedTime.Minute - 1) / 3) + 1;
            }

            return true;
        }

        private int LayotByName (string l) {
            int iRes = -1;

            if (l.Length > 3)
                switch (l)
                {
                    case "ППБР": iRes = 0; break;
                    default:
                        {
                            if (l.Substring(0, 3) != "ПБР" || int.TryParse(l.Substring(3), out iRes) == false || iRes <= 0 || iRes > 24)
                                ;
                            else
                                ;
                        }
                        break;
                }
            else
                ;

            return iRes;
        }

        private bool LayoutIsBiggerByName(string l1, string l2)
        {
            bool bRes = false;

            int num1 = LayotByName (l1),
                num2 = LayotByName (l2);

            if (num2 > num1)
                bRes = true;
            else
                ;
            
            return bRes;
        }

        private bool GetPBRValuesResponse(DataTable table)
        {
            bool bRes = true;

            if (! (table == null))
                m_tablePBRResponse = table.Copy();
            else
                ;

            return bRes;
        }

        private DataTable restruct_table_pbrValues(DataTable table_in)
        {
            DataTable table_in_restruct = new DataTable();
            List<DataColumn> cols_data = new List<DataColumn>();
            DataRow[] dataRows;
            int i = -1, j = -1, k = -1;
            string nameFieldDate = "DATE_PBR"; // tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_DATETIME]

            for (i = 0; i < table_in.Columns.Count; i++)
            {
                if (table_in.Columns[i].ColumnName == "ID_COMPONENT")
                {
                    //Преобразование таблицы
                    break;
                }
                else
                    ;
            }

            if (i < table_in.Columns.Count)
            {
                List<TG> list_TG = null;
                List<TECComponent> list_TECComponents = null;
                int count_comp = -1;

                if (num_TECComponent < 0) {
                    list_TECComponents = new List<TECComponent>();
                    for (i = 0; i < tec.list_TECComponents.Count; i ++) {
                        if ((tec.list_TECComponents[i].m_id > 100) && (tec.list_TECComponents[i].m_id < 500))
                            list_TECComponents.Add(tec.list_TECComponents[i]);
                        else
                            ;
                    }
                }
                else
                    list_TG = tec.list_TECComponents[num_TECComponent].TG;                
                
                //Преобразование таблицы
                for (i = 0; i < table_in.Columns.Count; i++)
                {
                    if ((!(table_in.Columns[i].ColumnName.Equals ("ID_COMPONENT") == true))
                        && (!(table_in.Columns[i].ColumnName.Equals (nameFieldDate) == true))
                        && (!(table_in.Columns[i].ColumnName.Equals (tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]) == true)))
                    //if (!(table_in.Columns[i].ColumnName == "ID_COMPONENT"))
                    {
                        cols_data.Add(table_in.Columns[i]);
                    }
                    else
                        if ((table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)
                            || (table_in.Columns[i].ColumnName.Equals (tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]) == true))
                        {
                            table_in_restruct.Columns.Add(table_in.Columns[i].ColumnName, table_in.Columns[i].DataType);
                        }
                        else
                            ;
                }

                if (num_TECComponent < 0)
                    count_comp = list_TECComponents.Count;
                else
                    count_comp = list_TG.Count;

                for (i = 0; i < count_comp; i++)
                {
                    for (j = 0; j < cols_data.Count; j++)
                    {
                        table_in_restruct.Columns.Add(cols_data[j].ColumnName, cols_data[j].DataType);
                        if (num_TECComponent < 0)
                            table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TECComponents[i].m_id;
                        else
                            table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TG[i].m_id;                        
                    }
                }

                if (tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                    table_in_restruct.Columns[tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]].SetOrdinal(table_in_restruct.Columns.Count - 1);
                else
                    ;

                List<DataRow[]> listDataRows = new List<DataRow[]>();

                for (i = 0; i < count_comp; i++)
                {
                    if (num_TECComponent < 0)
                        dataRows = table_in.Select("ID_COMPONENT=" + list_TECComponents[i].m_id);
                    else
                        dataRows = table_in.Select("ID_COMPONENT=" + list_TG[i].m_id);
                    
                    listDataRows.Add(new DataRow[dataRows.Length]);
                    dataRows.CopyTo(listDataRows[i], 0);

                    int indx_row = -1;
                    for (j = 0; j < listDataRows[i].Length; j++)
                    {
                        for (k = 0; k < table_in_restruct.Rows.Count; k++)
                        {
                            if (table_in_restruct.Rows[k][nameFieldDate].Equals(listDataRows[i][j][nameFieldDate]) == true)
                                break;
                            else
                                ;
                        }

                        if (!(k < table_in_restruct.Rows.Count))
                        {
                            table_in_restruct.Rows.Add();

                            indx_row = table_in_restruct.Rows.Count - 1;

                            //Заполнение DATE_ADMIN (постоянные столбцы)
                            table_in_restruct.Rows[indx_row][nameFieldDate] = listDataRows[i][j][nameFieldDate];
                            if (tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                                table_in_restruct.Rows[indx_row][tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]] = listDataRows[i][j][tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]];
                            else
                                ;
                        }
                        else
                            indx_row = k;

                        for (k = 0; k < cols_data.Count; k++)
                        {
                            if (num_TECComponent < 0)
                                table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_TECComponents[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                            else
                                table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_TG[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                        }
                    }
                }
            }
            else
                table_in_restruct = table_in;

            return table_in_restruct;
        }

        private DataTable restruct_table_adminValues (DataTable table_in) {
            DataTable table_in_restruct = new DataTable();
            List<DataColumn> cols_data = new List<DataColumn>();
            DataRow[] dataRows;
            int i = -1, j = -1, k = -1;
            string nameFieldDate = "DATE_ADMIN"; // tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.ADMIN_DATETIME]

            for (i = 0; i < table_in.Columns.Count; i ++) {
                if (table_in.Columns[i].ColumnName == "ID_COMPONENT")
                {
                    //Преобразование таблицы
                    break;
                }
                else
                    ;
            }

            if (i < table_in.Columns.Count)
            {
                List<TG> list_TG = null;
                List<TECComponent> list_TECComponents = null;
                int count_comp = -1;

                if (num_TECComponent < 0)
                {
                    list_TECComponents = new List<TECComponent>();
                    for (i = 0; i < tec.list_TECComponents.Count; i++)
                    {
                        if ((tec.list_TECComponents[i].m_id > 100) && (tec.list_TECComponents[i].m_id < 500))
                            list_TECComponents.Add(tec.list_TECComponents[i]);
                        else
                            ;
                    }
                }
                else
                    list_TG = tec.list_TECComponents[num_TECComponent].TG;
                
                //Преобразование таблицы
                for (i = 0; i < table_in.Columns.Count; i++)
                {
                    if ((!(table_in.Columns[i].ColumnName == "ID_COMPONENT")) && (!(table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)))
                    //if (!(table_in.Columns[i].ColumnName == "ID_COMPONENT"))
                    {
                        cols_data.Add(table_in.Columns [i]);
                    }
                    else
                        if (table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)
                        {
                            table_in_restruct.Columns.Add(table_in.Columns[i].ColumnName, table_in.Columns[i].DataType);
                        }
                        else
                            ;
                }

                if (num_TECComponent < 0)
                    count_comp = list_TECComponents.Count;
                else
                    count_comp = list_TG.Count;

                for (i = 0; i < count_comp; i++)
                {
                    for (j = 0; j < cols_data.Count; j++)
                    {
                        table_in_restruct.Columns.Add(cols_data[j].ColumnName, cols_data[j].DataType);
                        if (num_TECComponent < 0)
                            table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TECComponents[i].m_id;
                        else
                            table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TG[i].m_id;
                    }
                }

                List<DataRow[]> listDataRows = new List<DataRow[]>();

                for (i = 0; i < count_comp; i++)
                {
                    if (num_TECComponent < 0)
                        dataRows = table_in.Select("ID_COMPONENT=" + list_TECComponents[i].m_id);
                    else
                        dataRows = table_in.Select("ID_COMPONENT=" + list_TG[i].m_id);
                    listDataRows.Add(new DataRow[dataRows.Length]);
                    dataRows.CopyTo(listDataRows[i], 0);

                    int indx_row = -1;
                    for (j = 0; j < listDataRows [i].Length; j ++) {
                        for (k = 0; k < table_in_restruct.Rows.Count; k ++) {
                            if (table_in_restruct.Rows[k][nameFieldDate].Equals(listDataRows[i][j][nameFieldDate]) == true)
                                break;
                            else
                                ;
                        }

                        if (! (k < table_in_restruct.Rows.Count)) {
                            table_in_restruct.Rows.Add();

                            indx_row = table_in_restruct.Rows.Count - 1;

                            //Заполнение DATE_ADMIN (постоянные столбцы)
                            table_in_restruct.Rows[indx_row][nameFieldDate] = listDataRows[i][j][nameFieldDate];
                        }
                        else
                            indx_row = k;

                        for (k = 0; k < cols_data.Count; k ++) {
                            if (num_TECComponent < 0)
                                table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_TECComponents[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                            else
                                table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_TG[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                        }
                    }
                }
            }
            else
                table_in_restruct = table_in;

            return table_in_restruct;
        }

        private bool GetAdminValuesResponse(DataTable table_in)
        {
            DateTime date = pnlQuickData.dtprDate.Value.Date
                    , dtPBR;
            int hour;

            double currPBRe;
            int offsetPrev = -1
                //, tableRowsCount = table_in.Rows.Count; //tableRowsCount = m_tablePBRResponse.Rows.Count
                , i = -1, j = -1,
                offsetUDG, offsetPlan, offsetLayout;

            lastLayout = "---";

            //switch (tec.type ()) {
            //    case TEC.TEC_TYPE.COMMON:
            //        offsetPrev = -1;

                    if ((num_TECComponent < 0) || ((!(num_TECComponent < 0)) && (tec.list_TECComponents[num_TECComponent].m_id > 500)))
                    {
                        double[,] valuesPBR = new double[/*tec.list_TECComponents.Count*/m_list_TECComponents.Count, 25];
                        double[,] valuesREC = new double[m_list_TECComponents.Count, 25];
                        int[,] valuesISPER = new int[m_list_TECComponents.Count, 25];
                        double[,] valuesDIV = new double[m_list_TECComponents.Count, 25];

                        offsetUDG = 1;
                        offsetPlan = /*offsetUDG + 3 * tec.list_TECComponents.Count +*/ 1; //ID_COMPONENT
                        offsetLayout = -1;

                        m_tablePBRResponse = restruct_table_pbrValues(m_tablePBRResponse);
                        offsetLayout = (!(m_tablePBRResponse.Columns.IndexOf("PBR_NUMBER") < 0)) ? (offsetPlan + m_list_TECComponents.Count * 3) : m_tablePBRResponse.Columns.Count;

                        table_in = restruct_table_adminValues(table_in);

                        //if (!(table_in.Columns.IndexOf("ID_COMPONENT") < 0))
                        //    try { table_in.Columns.Remove("ID_COMPONENT"); }
                        //    catch (Exception excpt)
                        //    {
                        //        /*
                        //        Logging.Logg().LogExceptionToFile(excpt, "catch - PanelTecView.GetAdminValuesResponse () - ...");
                        //        */
                        //    }
                        //else
                        //    ;

                        // поиск в таблице записи по предыдущим суткам (мало ли, вдруг нету)
                        for (i = 0; i < m_tablePBRResponse.Rows.Count && offsetPrev < 0; i++)
                        {
                            if (!(m_tablePBRResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                            {
                                try
                                {
                                    dtPBR = (DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"];
                                    
                                    hour = dtPBR.Hour;
                                    if ((hour == 0) && (dtPBR.Day == date.Day))
                                    {
                                        offsetPrev = i;
                                        //foreach (TECComponent g in tec.list_TECComponents)
                                        for (j = 0; j < m_list_TECComponents.Count; j ++)
                                        {
                                            if ((offsetPlan + j * 3) < m_tablePBRResponse.Columns.Count)
                                                valuesPBR[j, 24] = (double)m_tablePBRResponse.Rows[i][offsetPlan + j * 3];
                                            else
                                                valuesPBR[j, 24] = 0.0;
                                            //j++;
                                        }
                                    }
                                    else
                                        ;
                                }
                                catch (Exception excpt) { Logging.Logg().LogExceptionToFile(excpt, "catch - PanelTecView.GetAdminValuesResponse () - ..."); }
                            }
                            else
                            {
                                try
                                {
                                    hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
                                    if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day == date.Day)
                                    {
                                        offsetPrev = i;
                                    }
                                    else
                                        ;
                                }
                                catch
                                {
                                }
                            }
                        }

                        // разбор остальных значений
                        for (i = 0; i < m_tablePBRResponse.Rows.Count; i++)
                        {
                            if (i == offsetPrev)
                                continue;
                            else
                                ;

                            if (!(m_tablePBRResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                            {
                                try
                                {
                                    dtPBR = (DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"];

                                    hour = dtPBR.Hour;
                                    if (hour == 0 && ((DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"]).Day != date.Day)
                                        hour = 24;
                                    else
                                        if (hour == 0)
                                            continue;
                                        else
                                            ;

                                    //foreach (TECComponent g in tec.list_TECComponents)
                                    for (j = 0; j < m_list_TECComponents.Count; j++)
                                    {
                                        try
                                        {
                                            if ((offsetPlan + (j * 3) < m_tablePBRResponse.Columns.Count) && (!(m_tablePBRResponse.Rows[i][offsetPlan + (j * 3)] is System.DBNull)))
                                                valuesPBR[j, hour - 1] = (double)m_tablePBRResponse.Rows[i][offsetPlan + (j * 3)];
                                            else
                                                valuesPBR[j, hour - 1] = 0.0;

                                            DataRow[] row_in = table_in.Select("DATE_ADMIN = '" + dtPBR.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                                            //if (i < table_in.Rows.Count)
                                            if (row_in.Length > 0)
                                            {
                                                if (row_in.Length > 1)
                                                    ; //Ошибка....
                                                else
                                                    ;

                                                if (!(row_in[0][offsetUDG + j * 3] is System.DBNull))
                                                //if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull)))
                                                    valuesREC[j, hour - 1] = (double)row_in[0][offsetUDG + j * 3];
                                                else
                                                    valuesREC[j, hour - 1] = 0;

                                                if (!(row_in[0][offsetUDG + 1 + j * 3] is System.DBNull))
                                                    valuesISPER[j, hour - 1] = (int)row_in[0][offsetUDG + 1 + j * 3];
                                                else
                                                    ;

                                                if (!(row_in[0][offsetUDG + 2 + j * 3] is System.DBNull))
                                                    valuesDIV[j, hour - 1] = (double)row_in[0][offsetUDG + 2 + j * 3];
                                                else
                                                    ;
                                            }
                                            else
                                            {
                                                valuesREC[j, hour - 1] = 0;
                                            }
                                        }
                                        catch
                                        {
                                        }
                                        //j++;
                                    }

                                    string tmp = "";
                                    //if ((m_tablePBRResponse.Columns.Contains ("PBR_NUMBER")) && !(m_tablePBRResponse.Rows[i][offsetLayout] is System.DBNull))
                                    if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (! (m_tablePBRResponse.Rows[i][offsetLayout] is System.DBNull)))
                                        tmp = (string)m_tablePBRResponse.Rows[i][offsetLayout];
                                    else
                                        ;
                                    
                                    if (LayoutIsBiggerByName(lastLayout, tmp))
                                        lastLayout = tmp;
                                    else
                                        ;
                                }
                                catch
                                {
                                }
                            }
                            else
                            {
                                try
                                {
                                    hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
                                    if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day != date.Day)
                                        hour = 24;
                                    else
                                        if (hour == 0)
                                            continue;
                                        else
                                            ;

                                    //foreach (TECComponent g in tec.list_TECComponents)
                                    for (j = 0; j < m_list_TECComponents.Count; j++)
                                    {
                                        try
                                        {
                                            valuesPBR[j, hour - 1] = 0;

                                            if (i < table_in.Rows.Count)
                                            {
                                                if (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull))
                                                //if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull)))
                                                    valuesREC[j, hour - 1] = (double)table_in.Rows[i][offsetUDG + j * 3];
                                                else
                                                    valuesREC[j, hour - 1] = 0;
                                            
                                                if (!(table_in.Rows[i][offsetUDG + 1 + j * 3] is System.DBNull))
                                                    valuesISPER[j, hour - 1] = (int)table_in.Rows[i][offsetUDG + 1 + j * 3];
                                                else
                                                    ;
                                            
                                                if (!(table_in.Rows[i][offsetUDG + 2 + j * 3] is System.DBNull))
                                                    valuesDIV[j, hour - 1] = (double)table_in.Rows[i][offsetUDG + 2 + j * 3];
                                                else
                                                    ;
                                            }
                                            else
                                            {
                                                valuesREC[j, hour - 1] = 0;
                                            }
                                        }
                                        catch
                                        {
                                        }
                                        //j++;
                                    }

                                    string tmp = "";
                                    if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(m_tablePBRResponse.Rows[i][offsetLayout] is System.DBNull)))
                                        tmp = (string)m_tablePBRResponse.Rows[i][offsetLayout];
                                    else
                                        ;

                                    if (LayoutIsBiggerByName(lastLayout, tmp))
                                        lastLayout = tmp;
                                    else
                                        ;
                                }
                                catch
                                {
                                }
                            }
                        }

                        for (i = 0; i < 24; i++)
                        {
                            for (j = 0; j < m_list_TECComponents.Count; j++)
                            {
                                m_valuesHours.valuesPBR[i] += valuesPBR[j, i];
                                if (i == 0)
                                {
                                    currPBRe = (valuesPBR[j, i] + valuesPBR[j, 24]) / 2;
                                    m_valuesHours.valuesPBRe[i] += currPBRe;
                                }
                                else
                                {
                                    currPBRe = (valuesPBR[j, i] + valuesPBR[j, i - 1]) / 2;
                                    m_valuesHours.valuesPBRe[i] += currPBRe;
                                }

                                m_valuesHours.valuesUDGe[i] += currPBRe + valuesREC[j, i];

                                if (valuesISPER[j, i] == 1)
                                    m_valuesHours.valuesDiviation[i] += (currPBRe + valuesREC[j, i]) * valuesDIV[j, i] / 100;
                                else
                                    m_valuesHours.valuesDiviation[i] += valuesDIV[j, i];
                            }
                            /*m_valuesHours.valuesPBR[i] = 0.20;
                            m_valuesHours.valuesPBRe[i] = 0.20;
                            m_valuesHours.valuesUDGe[i] = 0.20;
                            m_valuesHours.valuesDiviation[i] = 0.05;*/
                        }
                        if (m_valuesHours.season == seasonJumpE.SummerToWinter)
                        {
                            m_valuesHours.valuesPBRAddon = m_valuesHours.valuesPBR[m_valuesHours.hourAddon];
                            m_valuesHours.valuesPBReAddon = m_valuesHours.valuesPBRe[m_valuesHours.hourAddon];
                            m_valuesHours.valuesUDGeAddon = m_valuesHours.valuesUDGe[m_valuesHours.hourAddon];
                            m_valuesHours.valuesDiviationAddon = m_valuesHours.valuesDiviation[m_valuesHours.hourAddon];
                        }
                    }
                    else
                    {
                        double[] valuesPBR = new double[25];
                        double[] valuesREC = new double[25];
                        int[] valuesISPER = new int[25];
                        double[] valuesDIV = new double[25];

                        offsetUDG = 1;
                        offsetPlan = 1;
                        offsetLayout = (!(m_tablePBRResponse.Columns.IndexOf("PBR_NUMBER") < 0)) ? offsetPlan + 3 : m_tablePBRResponse.Columns.Count;

                        // поиск в таблице записи по предыдущим суткам (мало ли, вдруг нету)
                        for (i = 0; i < m_tablePBRResponse.Rows.Count && offsetPrev < 0; i++)
                        {
                            if (!(m_tablePBRResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                            {
                                try
                                {
                                    dtPBR = (DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"];

                                    hour = dtPBR.Hour;
                                    if (hour == 0 && ((DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"]).Day == date.Day)
                                    {
                                        offsetPrev = i;
                                        valuesPBR[24] = (double)m_tablePBRResponse.Rows[i][offsetPlan];
                                    }
                                }
                                catch
                                {
                                }
                            }
                            else
                            {
                                try
                                {
                                    hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
                                    if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day == date.Day)
                                    {
                                        offsetPrev = i;
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }

                        // разбор остальных значений
                        for (i = 0; i < m_tablePBRResponse.Rows.Count; i++)
                        {
                            if (i == offsetPrev)
                                continue;
                            else
                                ;

                            if (!(m_tablePBRResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                            {
                                try
                                {
                                    dtPBR = (DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"];

                                    hour = dtPBR.Hour;
                                    if ((hour == 0) && (!(dtPBR.Day == date.Day)))
                                        hour = 24;
                                    else
                                        if (hour == 0)
                                            continue;
                                        else
                                            ;

                                    if ((offsetPlan < m_tablePBRResponse.Columns.Count) && (!(m_tablePBRResponse.Rows[i][offsetPlan] is System.DBNull)))
                                        valuesPBR[hour - 1] = (double)m_tablePBRResponse.Rows[i][offsetPlan];
                                    else
                                        ;

                                    DataRow[] row_in = table_in.Select("DATE_ADMIN = '" + dtPBR.ToString("yyyy-MM-dd HH:mm:ss") + "'");
                                    //if (i < table_in.Rows.Count)
                                    if (row_in.Length > 0)
                                    {
                                        if (row_in.Length > 1)
                                            ; //Ошибка....
                                        else
                                            ;

                                        if (!(row_in [0][offsetUDG] is System.DBNull))
                                        //if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG] is System.DBNull)))
                                            valuesREC[hour - 1] = (double)row_in[0][offsetUDG + 0];
                                        else
                                            valuesREC[hour - 1] = 0;

                                        if (!(row_in[0][offsetUDG + 1] is System.DBNull))
                                            valuesISPER[hour - 1] = (int)row_in[0][offsetUDG + 1];
                                        else
                                            ;

                                        if (!(row_in[0][offsetUDG + 2] is System.DBNull))
                                            valuesDIV[hour - 1] = (double)row_in[0][offsetUDG + 2];
                                        else
                                            ;
                                    }
                                    else
                                    {
                                        valuesREC[hour - 1] = 0;
                                        //valuesISPER[hour - 1] = 0;
                                        //valuesDIV[hour - 1] = 0;
                                    }

                                    string tmp = "";
                                    if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(m_tablePBRResponse.Rows[i][offsetLayout] is System.DBNull)))
                                        tmp = (string)m_tablePBRResponse.Rows[i][offsetLayout];
                                    else
                                        ;

                                    if (LayoutIsBiggerByName(lastLayout, tmp))
                                        lastLayout = tmp;
                                    else
                                        ;
                                }
                                catch (Exception e)
                                {
                                    Logging.Logg ().LogExceptionToFile (e, "PanelTecView::GetAdminValueResponse ()...");
                                }
                            }
                            else
                            {
                                try
                                {
                                    hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
                                    if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day != date.Day)
                                        hour = 24;
                                    else
                                        if (hour == 0)
                                            continue;
                                        else
                                            ;

                                    valuesPBR[hour - 1] = 0;

                                    if (i < table_in.Rows.Count)
                                    {
                                        if (!(table_in.Rows[i][offsetUDG + 0] is System.DBNull))
                                        //if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + 0] is System.DBNull)))
                                            valuesREC[hour - 1] = (double)table_in.Rows[i][offsetUDG + 0];
                                        else
                                            valuesREC[hour - 1] = 0;

                                        if (!(table_in.Rows[i][offsetUDG + 1] is System.DBNull))
                                            valuesISPER[hour - 1] = (int)table_in.Rows[i][offsetUDG + 1];
                                        else
                                            ;

                                        if (!(table_in.Rows[i][offsetUDG + 2] is System.DBNull))
                                            valuesDIV[hour - 1] = (double)table_in.Rows[i][offsetUDG + 2];
                                        else
                                            ;
                                    }
                                    else
                                    {
                                        valuesREC[hour - 1] = 0;
                                        //valuesISPER[hour - 1] = 0;
                                        //valuesDIV[hour - 1] = 0;
                                    }

                                    string tmp = "";
                                    if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(m_tablePBRResponse.Rows[i][offsetLayout] is System.DBNull)))
                                        tmp = (string)m_tablePBRResponse.Rows[i][offsetLayout];
                                    else
                                        ;

                                    if (LayoutIsBiggerByName(lastLayout, tmp))
                                        lastLayout = tmp;
                                    else
                                        ;
                                }
                                catch
                                {
                                }
                            }
                        }

                        for (i = 0; i < 24; i++)
                        {
                            m_valuesHours.valuesPBR[i] = valuesPBR[i];
                            if (i == 0)
                            {
                                currPBRe = (valuesPBR[i] + valuesPBR[24]) / 2;
                                m_valuesHours.valuesPBRe[i] = currPBRe;
                            }
                            else
                            {
                                currPBRe = (valuesPBR[i] + valuesPBR[i - 1]) / 2;
                                m_valuesHours.valuesPBRe[i] = currPBRe;
                            }

                            m_valuesHours.valuesUDGe[i] = currPBRe + valuesREC[i];

                            if (valuesISPER[i] == 1)
                                m_valuesHours.valuesDiviation[i] = (currPBRe + valuesREC[i]) * valuesDIV[i] / 100;
                            else
                                m_valuesHours.valuesDiviation[i] = valuesDIV[i];
                        }

                        if (m_valuesHours.season == seasonJumpE.SummerToWinter)
                        {
                            m_valuesHours.valuesPBRAddon = m_valuesHours.valuesPBR[m_valuesHours.hourAddon];
                            m_valuesHours.valuesPBReAddon = m_valuesHours.valuesPBRe[m_valuesHours.hourAddon];
                            m_valuesHours.valuesUDGeAddon = m_valuesHours.valuesUDGe[m_valuesHours.hourAddon];
                            m_valuesHours.valuesDiviationAddon = m_valuesHours.valuesDiviation[m_valuesHours.hourAddon];
                        }
                        else
                            ;
                    }
            //        break;
            //    case TEC.TEC_TYPE.BIYSK:
            //        if (num_gtp < 0)
            //        {
            //            offsetPrev = -1;
            //            offsetUDG = 1; //, offsetPlan, offsetLayout;
            //            //offsetPlan = offsetUDG + 3 * tec.list_TECComponents.Count;
            //            //offsetLayout = offsetPlan + tec.list_TECComponents.Count;

            //            double[,] valuesPBR = new double[tec.list_TECComponents.Count, 25];
            //            double[,] valuesREC = new double[tec.list_TECComponents.Count, 25];
            //            int[,] valuesISPER = new int[tec.list_TECComponents.Count, 25];
            //            double[,] valuesDIV = new double[tec.list_TECComponents.Count, 25];

            //            // поиск в таблице записи по предыдущим суткам (мало ли, вдруг нету)
            //            for (int i = 0; i < tableRowsCount && offsetPrev < 0; i++)
            //            {
            //                if (!(table_in.Rows[i]["DATE_ADMIN"] is System.DBNull))
            //                {
            //                    try
            //                    {
            //                        hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
            //                        if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day == date.Day)
            //                        {
            //                            offsetPrev = i;
            //                            int j = 0;
            //                            foreach (TECComponent g in tec.list_TECComponents)
            //                            {
            //                                valuesPBR[j, 24] = 0/*(double)table.Rows[i][offsetPlan + j]*/;
            //                                j++;
            //                            }
            //                        }
            //                    }
            //                    catch
            //                    {
            //                    }
            //                }
            //                else
            //                {
            //                    try
            //                    {
            //                        hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
            //                        if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day == date.Day)
            //                        {
            //                            offsetPrev = i;
            //                        }
            //                    }
            //                    catch
            //                    {
            //                    }
            //                }
            //            }

            //            // разбор остальных значений
            //            for (int i = 0; i < tableRowsCount; i++)
            //            {
            //                if (i == offsetPrev)
            //                    continue;

            //                if (!(table_in.Rows[i]["DATE_ADMIN"] is System.DBNull))
            //                {
            //                    try
            //                    {
            //                        hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
            //                        if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day != date.Day)
            //                            hour = 24;
            //                        else
            //                            if (hour == 0)
            //                                continue;

            //                        int j = 0;
            //                        foreach (TECComponent g in tec.list_TECComponents)
            //                        {
            //                            try
            //                            {
            //                                /*if (!(table.Rows[i][offsetPlan + j] is System.DBNull))*/
            //                                valuesPBR[j, hour - 1] = 0/*(double)table.Rows[i][offsetPlan + j]*/;
            //                                if (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull))
            //                                    valuesREC[j, hour - 1] = (double)table_in.Rows[i][offsetUDG + j * 3];
            //                                else
            //                                    ;

            //                                if (!(table_in.Rows[i][offsetUDG + 1 + j * 3] is System.DBNull))
            //                                    valuesISPER[j, hour - 1] = (int)table_in.Rows[i][offsetUDG + 1 + j * 3];
            //                                else
            //                                    ;

            //                                if (!(table_in.Rows[i][offsetUDG + 2 + j * 3] is System.DBNull))
            //                                    valuesDIV[j, hour - 1] = (double)table_in.Rows[i][offsetUDG + 2 + j * 3];
            //                                else
            //                                    ;
            //                            }
            //                            catch
            //                            {
            //                            }
            //                            j++;
            //                        }
            //                        /*string tmp = "";
            //                        if (!(table.Rows[i][offsetLayout] is System.DBNull))
            //                            tmp = (string)table.Rows[i][offsetLayout];
            //                        if (LayoutIsBiggerByName(lastLayout, tmp))
            //                            lastLayout = tmp;*/
            //                    }
            //                    catch
            //                    {
            //                    }
            //                }
            //                else
            //                {
            //                    try
            //                    {
            //                        hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
            //                        if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day != date.Day)
            //                            hour = 24;
            //                        else
            //                            if (hour == 0)
            //                                continue;
            //                            else
            //                                ;

            //                        int j = 0;
            //                        foreach (TECComponent g in tec.list_TECComponents)
            //                        {
            //                            try
            //                            {
            //                                valuesPBR[j, hour - 1] = 0;
            //                                if (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull))
            //                                    valuesREC[j, hour - 1] = (double)table_in.Rows[i][offsetUDG + j * 3];
            //                                else
            //                                    ;

            //                                if (!(table_in.Rows[i][offsetUDG + 1 + j * 3] is System.DBNull))
            //                                    valuesISPER[j, hour - 1] = (int)table_in.Rows[i][offsetUDG + 1 + j * 3];
            //                                else
            //                                    ;

            //                                if (!(table_in.Rows[i][offsetUDG + 2 + j * 3] is System.DBNull))
            //                                    valuesDIV[j, hour - 1] = (double)table_in.Rows[i][offsetUDG + 2 + j * 3];
            //                                else
            //                                    ;
            //                            }
            //                            catch
            //                            {
            //                            }
            //                            j++;
            //                        }
            //                        /*string tmp = "";
            //                        if (!(table.Rows[i][offsetLayout] is System.DBNull))
            //                            tmp = (string)table.Rows[i][offsetLayout];
            //                        if (LayoutIsBiggerByName(lastLayout, tmp))
            //                            lastLayout = tmp;*/
            //                    }
            //                    catch
            //                    {
            //                    }
            //                }
            //            }

            //            for (int i = 0; i < 24; i++)
            //            {
            //                for (int j = 0; j < tec.list_TECComponents.Count; j++)
            //                {
            //                    /*m_valuesHours.valuesPBR[i] += valuesPBR[j, i];
            //                    if (i == 0)
            //                    {
            //                        currPBRe = (valuesPBR[j, i] + valuesPBR[j, 24]) / 2;
            //                        m_valuesHours.valuesPBRe[i] += currPBRe;
            //                    }
            //                    else
            //                    {
            //                        currPBRe = (valuesPBR[j, i] + valuesPBR[j, i - 1]) / 2;
            //                        m_valuesHours.valuesPBRe[i] += currPBRe;
            //                    }*/
            //                    currPBRe = 0;

            //                    m_valuesHours.valuesUDGe[i] += currPBRe + valuesREC[j, i];

            //                    if (valuesISPER[j, i] == 1)
            //                        m_valuesHours.valuesDiviation[i] += (currPBRe + valuesREC[j, i]) * valuesDIV[j, i] / 100;
            //                    else
            //                        m_valuesHours.valuesDiviation[i] += valuesDIV[j, i];
            //                }
            //                /*m_valuesHours.valuesPBR[i] = 0.20;
            //                m_valuesHours.valuesPBRe[i] = 0.20;
            //                m_valuesHours.valuesUDGe[i] = 0.20;
            //                m_valuesHours.valuesDiviation[i] = 0.05;*/
            //            }
            //            if (m_valuesHours.season == seasonJumpE.SummerToWinter)
            //            {
            //                m_valuesHours.valuesPBRAddon = m_valuesHours.valuesPBR[m_valuesHours.hourAddon];
            //                m_valuesHours.valuesPBReAddon = m_valuesHours.valuesPBRe[m_valuesHours.hourAddon];
            //                m_valuesHours.valuesUDGeAddon = m_valuesHours.valuesUDGe[m_valuesHours.hourAddon];
            //                m_valuesHours.valuesDiviationAddon = m_valuesHours.valuesDiviation[m_valuesHours.hourAddon];
            //            }
            //        }
            //        else
            //        {
            //            offsetPrev = -1;
            //            offsetUDG = 1; //, offsetPlan, offsetLayout;
            //            /*offsetPlan = offsetUDG + 3;
            //            offsetLayout = offsetPlan + 1;*/
                        
            //            double[] valuesPBR = new double[25];
            //            double[] valuesREC = new double[25];
            //            int[] valuesISPER = new int[25];
            //            double[] valuesDIV = new double[25];

            //            // поиск в таблице записи по предыдущим суткам (мало ли, вдруг нету)
            //            for (int i = 0; i < tableRowsCount && offsetPrev < 0; i++)
            //            {
            //                if (!(table_in.Rows[i]["DATE_ADMIN"] is System.DBNull))
            //                {
            //                    try
            //                    {
            //                        hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
            //                        if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day == date.Day)
            //                        {
            //                            offsetPrev = i;
            //                            valuesPBR[24] = 0/*(double)table.Rows[i][offsetPlan]*/;
            //                        }
            //                        else
            //                            ;
            //                    }
            //                    catch
            //                    {
            //                    }
            //                }
            //                else
            //                {
            //                    try
            //                    {
            //                        hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
            //                        if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day == date.Day)
            //                        {
            //                            offsetPrev = i;
            //                        }
            //                    }
            //                    catch
            //                    {
            //                    }
            //                }
            //            }

            //            // разбор остальных значений
            //            for (int i = 0; i < tableRowsCount; i++)
            //            {
            //                if (i == offsetPrev)
            //                    continue;

            //                if (!(table_in.Rows[i]["DATE_ADMIN"] is System.DBNull))
            //                {
            //                    try
            //                    {
            //                        hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
            //                        if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day != date.Day)
            //                            hour = 24;
            //                        else
            //                            if (hour == 0)
            //                                continue;

            //                        /*if (!(table.Rows[i][offsetPlan] is System.DBNull))*/
            //                        valuesPBR[hour - 1] = 0/*(double)table.Rows[i][offsetPlan]*/;
            //                        if (!(table_in.Rows[i][offsetUDG] is System.DBNull))
            //                            valuesREC[hour - 1] = (double)table_in.Rows[i][offsetUDG];
            //                        else
            //                            ;

            //                        if (!(table_in.Rows[i][offsetUDG + 1] is System.DBNull))
            //                            valuesISPER[hour - 1] = (int)table_in.Rows[i][offsetUDG + 1];
            //                        else
            //                            ;
                                    
            //                        if (!(table_in.Rows[i][offsetUDG + 2] is System.DBNull))
            //                            valuesDIV[hour - 1] = (double)table_in.Rows[i][offsetUDG + 2];
            //                        else
            //                            ;

            //                        /*string tmp = "";
            //                        if (!(table.Rows[i][offsetLayout] is System.DBNull))
            //                            tmp = (string)table.Rows[i][offsetLayout];
            //                        if (LayoutIsBiggerByName(lastLayout, tmp))
            //                            lastLayout = tmp;*/
            //                    }
            //                    catch
            //                    {
            //                    }
            //                }
            //                else
            //                {
            //                    try
            //                    {
            //                        hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
            //                        if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day != date.Day)
            //                            hour = 24;
            //                        else
            //                            if (hour == 0)
            //                                continue;
            //                            else
            //                                ;

            //                        valuesPBR[hour - 1] = 0;
            //                        if (!(table_in.Rows[i][offsetUDG] is System.DBNull))
            //                            valuesREC[hour - 1] = (double)table_in.Rows[i][offsetUDG];
            //                        else
            //                            ;

            //                        if (!(table_in.Rows[i][offsetUDG + 1] is System.DBNull))
            //                            valuesISPER[hour - 1] = (int)table_in.Rows[i][offsetUDG + 1];
            //                        else
            //                            ;

            //                        if (!(table_in.Rows[i][offsetUDG + 2] is System.DBNull))
            //                            valuesDIV[hour - 1] = (double)table_in.Rows[i][offsetUDG + 2];
            //                        else
            //                            ;

            //                        /*string tmp = "";
            //                        if (!(table.Rows[i][offsetLayout] is System.DBNull))
            //                            tmp = (string)table.Rows[i][offsetLayout];
            //                        if (LayoutIsBiggerByName(lastLayout, tmp))
            //                            lastLayout = tmp;*/
            //                    }
            //                    catch
            //                    {
            //                    }
            //                }
            //            }

            //            for (int i = 0; i < 24; i++)
            //            {
            //                /*m_valuesHours.valuesPBR[i] = valuesPBR[i];
            //                if (i == 0)
            //                {
            //                    currPBRe = (valuesPBR[i] + valuesPBR[24]) / 2;
            //                    m_valuesHours.valuesPBRe[i] = currPBRe;
            //                }
            //                else
            //                {
            //                    currPBRe = (valuesPBR[i] + valuesPBR[i - 1]) / 2;
            //                    m_valuesHours.valuesPBRe[i] = currPBRe;
            //                }*/
            //                currPBRe = 0;

            //                m_valuesHours.valuesUDGe[i] = currPBRe + valuesREC[i];

            //                if (valuesISPER[i] == 1)
            //                    m_valuesHours.valuesDiviation[i] = (currPBRe + valuesREC[i]) * valuesDIV[i] / 100;
            //                else
            //                    m_valuesHours.valuesDiviation[i] = valuesDIV[i];
            //            }

            //            if (m_valuesHours.season == seasonJumpE.SummerToWinter)
            //            {
            //                m_valuesHours.valuesPBRAddon = m_valuesHours.valuesPBR[m_valuesHours.hourAddon];
            //                m_valuesHours.valuesPBReAddon = m_valuesHours.valuesPBRe[m_valuesHours.hourAddon];
            //                m_valuesHours.valuesUDGeAddon = m_valuesHours.valuesUDGe[m_valuesHours.hourAddon];
            //                m_valuesHours.valuesDiviationAddon = m_valuesHours.valuesDiviation[m_valuesHours.hourAddon];
            //            }
            //        }
            //        break;
            //    default:
            //        break;
            //}
            
            hour = lastHour;
            if (hour == 24)
                hour = 23;

            for (i = 0; i < 21; i++)
            {
                m_valuesMins.valuesPBR[i] = m_valuesHours.valuesPBR[hour];
                m_valuesMins.valuesPBRe[i] = m_valuesHours.valuesPBRe[hour];
                m_valuesMins.valuesUDGe[i] = m_valuesHours.valuesUDGe[hour];
                m_valuesMins.valuesDiviation[i] = m_valuesHours.valuesDiviation[hour];
            }

            return true;
        }
        
        private void ComputeRecomendation(int hour)
        {
            if (hour == 24)
                hour = 23;

            if (m_valuesHours.valuesUDGe[hour] == 0)
            {
                recomendation = 0;
                return;
            }

            if (!currHour)
            {
                recomendation = m_valuesHours.valuesUDGe[hour];
                return;
            }

            if (lastMin < 2)
            {
                recomendation = m_valuesHours.valuesUDGe[hour];
                return;
            }

            double factSum = 0;
            for (int i = 1; i < lastMin; i++)
                factSum += m_valuesMins.valuesFact[i];

            if (lastMin == 21)
                recomendation = 0;
            else
                recomendation = (m_valuesHours.valuesUDGe[hour] * 20 - factSum) / (20 - (lastMin - 1));

            if (recomendation < 0)
                recomendation = 0;
        }

        private void NewDateRefresh()
        {
            delegateStartWait();
            lock (lockValue)
            {
                ChangeState ();

                try
                {
                    sem.Release(1);
                }
                catch
                {
                }

            }
            delegateStopWait();
        }

        private void dtprDate_ValueChanged(object sender, EventArgs e)
        {
            if (update)
            {
                if (! (pnlQuickData.dtprDate.Value.Date == selectedTime.Date))
                    currHour = false;
                else
                    ;

                NewDateRefresh();
            }
            else
                update = true;
        }

        private void SetNowDate(bool received)
        {
            currHour = true;

            if (received)
            {
                update = false;
                pnlQuickData.dtprDate.Value = selectedTime;
            }
            else
            {
                NewDateRefresh();
            }
        }

        private void btnSetNow_Click(object sender, EventArgs e)
        {
            SetNowDate(false);
        }

        public void UpdateGraphicsCurrent()
        {
            DrawGraphHours();
            DrawGraphMins(lastHour);
        }

        private void ChangeState () {
            newState = true;
            states.Clear();

            if ((sensorsString_TM.Equals(string.Empty) == false) ||
                ((sensorsStrings_Fact[(int)TG.ID_TIME.MINUTES].Equals(string.Empty) == false) && (sensorsStrings_Fact[(int)TG.ID_TIME.HOURS].Equals(string.Empty) == false)))
            {
                if (currHour == true)
                {
                    states.Add(StatesMachine.CurrentTime);
                }
                else
                {
                    selectedTime = pnlQuickData.dtprDate.Value.Date;
                }
            }
            else
            {
                states.Add(StatesMachine.Init_Fact);
                states.Add(StatesMachine.Init_TM);
                states.Add(StatesMachine.CurrentTime);
            }

            states.Add(StatesMachine.CurrentHours_Fact);
            states.Add(StatesMachine.CurrentMins_Fact);
            states.Add(StatesMachine.Current_TM);
            states.Add(StatesMachine.LastMinutes_TM);
            states.Add(StatesMachine.PBRValues);
            states.Add(StatesMachine.AdminValues);            
        }

        public void Activate(bool active)
        {
            if (active)
            {
                isActive = true;
                currValuesPeriod = 0;
                lock (lockValue)
                {
                    ChangeState();

                    try
                    {
                        sem.Release(1);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                isActive = false;
                lock (lockValue)
                {
                    newState = true;
                    states.Clear();
                    m_report.errored_state =
                    m_report.actioned_state = false;
                }
            }
        }

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public void ErrorReport(string error_string)
        {
            m_report.last_error = error_string;
            m_report.last_time_error = DateTime.Now;
            m_report.errored_state = true;
            stsStrip.BeginInvoke(delegateEventUpdate);
        }

        private void ActionReport(string action_string)
        {
            m_report.last_action = action_string;
            m_report.last_time_action = DateTime.Now;
            m_report.actioned_state = true;
            stsStrip.BeginInvoke(delegateEventUpdate);
        }

        private void ShowValues(string caption)
        {
            /*MessageBox.Show(this, "state = " + state + "\naction = " + action + "\ndate = " + dtprDate.Value.ToString() +
                            "\nnow_date = " + DateTime.Now.ToString() + "\ngmt0 = " + System.TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).ToString() +
                            "\nselectedTime = " + selectedTime.ToString() + "lastHour = " + lastHour + "\nlastMin = " + lastMin + "\ncurrHour = " + currHour + 
                            "\nadminValuesReceived = " + adminValuesReceived, caption, MessageBoxButtons.OK);*/

            MessageBox.Show(this, "", caption, MessageBoxButtons.OK);
        }

        object[] generateValues(DateTime dt, int indx_tg, int indx_halfhours, int indx_id_time, int season)
        {
            //Согласно структуры запросов 'GetHoursRequest' и 'GetMinsRequest'
            int indx_season = 2; //8
            object [] resValues = new object [9];

            //resValues[0] = "ТГ-" + indx_tg;
            //resValues[1] = 0;
            //resValues[2] = 0;
            //resValues[3] = 0;
            //resValues[4] = 0;
            //resValues[5] = 30 + indx_halfhours * 2;
            //resValues[6] = dt;
            //resValues[7] = sensorId2TG[indx_tg].ids_fact[indx_id_time];

            resValues[0] = sensorId2TG[indx_tg].ids_fact[indx_id_time];
            resValues[1] = dt;
            //2 - season
            resValues[3] = 30 + indx_halfhours * 2;
            resValues[4] = "ТГ-" + indx_tg;
            resValues[5] = 0;
            resValues[6] = 0;
            resValues[7] = 0;
            resValues[8] = 0;

            if (season == 0) //Нет перехода на зимнее/летнее время
                resValues[indx_season] = dt.Year * 2 + 1;
            else
                if (season == -1) //С переходом на зимнее время
                    if (indx_halfhours < 6)
                        resValues[indx_season] = dt.Year * 2 + 1;
                    else
                        resValues[indx_season] = dt.Year * 2 + 2;
                else
                    if (season == 1) //С переходом на летнее время
                        if (indx_halfhours < 4)
                            resValues[indx_season] = dt.Year * 2;
                        else
                            resValues[indx_season] = dt.Year * 2 + 1;
                    else
                        ;                

            return resValues;
        }
        
        void GenerateHoursTable(seasonJumpE season, int hours, DataTable table)
        {
            int count = hours * 2;
            DateTime date = DateTime.Now.Date.AddMinutes(30);

            table.Clear();

            if (season == seasonJumpE.None)
            {
                // генерирую время без переходов
                for (int i = 0; i < count; i++)
                {
                    for (int j = 0; j < CountTG; j++)
                    {
                        table.Rows.Add(generateValues (date, j, i, (int) TG.ID_TIME.HOURS, 0));
                    }

                    date = date.AddMinutes(30);
                }
            }
            else
            {
                if (season == seasonJumpE.SummerToWinter)
                {
                    // генерирую время с переходом на зимнее время
                    for (int i = 0; i < count; i++)
                    {
                        for (int j = 0; j < CountTG; j++)
                        {
                            table.Rows.Add(generateValues(date, j, i, (int)TG.ID_TIME.HOURS, -1));
                        }
                        if (i == 4 || i == 5)
                        {
                            for (int j = 0; j < CountTG; j++)
                            {
                                //object[] values = new object[9];
                                //values[0] = "ТГ-" + j;
                                //values[1] = 0;
                                //values[2] = 0;
                                //values[3] = 0;
                                //values[4] = 0;
                                //values[5] = 30 + i * 2;
                                //values[6] = date;
                                //values[7] = sensorId2TG[j].id;
                                //values[8] = date.Year * 2 + 2;
                                table.Rows.Add(generateValues(date, j, i, (int)TG.ID_TIME.HOURS, -1));
                            }
                        }

                        date = date.AddMinutes(30);
                    }
                }
                else
                {
                    //генерирую время с переходом на летнее время
                    for (int i = 0; i < count; i++)
                    {
                        if (i != 4 && i != 5)
                        {
                            for (int j = 0; j < CountTG; j++)
                            {
                                //object[] values = new object[9];
                                //values[0] = "ТГ-" + j;
                                //values[1] = 0;
                                //values[2] = 0;
                                //values[3] = 0;
                                //values[4] = 0;
                                //values[5] = 30 + i * 2;
                                //values[6] = date;
                                //values[7] = sensorId2TG[j].id;
                                //if (i < 4)
                                //    values[8] = date.Year * 2;
                                //else
                                //    values[8] = date.Year * 2 + 1;
                                table.Rows.Add(generateValues(date, j, i, (int)TG.ID_TIME.HOURS, 1));
                            }
                        }

                        date = date.AddMinutes(30);
                    }
                }
            }
        }

        void GenerateMinsTable(seasonJumpE season, int mins, DataTable table)
        {
            int count = mins + 1;
            DateTime date = DateTime.Now.Date;

            table.Clear();

            if (season == seasonJumpE.None)
            {
                // генерирую время без переходов
                for (int i = 0; i < count; i++)
                {
                    for (int j = 0; j < CountTG; j++)
                    {
                        //object[] values = new object[9];
                        //values[0] = "ТГ-" + j;
                        //values[1] = 0;
                        //values[2] = 0;
                        //values[3] = 0;
                        //values[4] = 0;
                        //values[5] = 30 + i * 2;
                        //values[6] = date;
                        //values[7] = sensorId2TG[j].id;
                        //values[8] = date.Year * 2 + 1;
                        table.Rows.Add(generateValues(date, j, i, (int)TG.ID_TIME.MINUTES, 0));
                    }

                    date = date.AddMinutes(3);
                }
            }
            else
            {
                if (season == seasonJumpE.SummerToWinter)
                {
                    // генерирую время с переходом на зимнее время
                    for (int i = 0; i < count; i++)
                    {
                        for (int j = 0; j < CountTG; j++)
                        {
                            //object[] values = new object[9];
                            //values[0] = "ТГ-" + j;
                            //values[1] = 0;
                            //values[2] = 0;
                            //values[3] = 0;
                            //values[4] = 0;
                            //values[5] = 30 + i * 2;
                            //values[6] = date;
                            //values[7] = sensorId2TG[j].id;
                            //values[8] = date.Year * 2 + 1;
                            table.Rows.Add(generateValues(date, j, i, (int)TG.ID_TIME.MINUTES, -1));
                        }
                        for (int j = 0; j < CountTG; j++)
                        {
                            //object[] values = new object[9];
                            //values[0] = "ТГ-" + j;
                            //values[1] = 0;
                            //values[2] = 0;
                            //values[3] = 0;
                            //values[4] = 0;
                            //values[5] = 30 + i * 2;
                            //values[6] = date;
                            //values[7] = sensorId2TG[j].id;
                            //values[8] = date.Year * 2 + 2;
                            table.Rows.Add(generateValues(date, j, i, (int)TG.ID_TIME.MINUTES, -1));
                        }

                        date = date.AddMinutes(3);
                    }
                }
                else
                {
                    //генерирую время с переходом на летнее время
                    for (int i = 0; i < count; i++)
                    {
                        for (int j = 0; j < CountTG; j++)
                        {
                            //object[] values = new object[9];
                            //values[0] = "ТГ-" + j;
                            //values[1] = 0;
                            //values[2] = 0;
                            //values[3] = 0;
                            //values[4] = 0;
                            //values[5] = 30 + i * 2;
                            //values[6] = date;
                            //values[7] = sensorId2TG[j].id;
                            //values[8] = date.Year * 2 + 1;
                            table.Rows.Add(generateValues(date, j, i, (int)TG.ID_TIME.MINUTES, 1));
                        }

                        date = date.AddMinutes(3);
                    }
                }
            }
        }

        private void StateRequest(StatesMachine state)
        {
            switch (state)
            {
                case StatesMachine.Init_Fact:
                    ActionReport("Получение идентификаторов датчиков.");
                    switch (tec.type())
                    {
                        case TEC.TEC_TYPE.COMMON:
                            GetSensorsFactRequest();
                            break;
                        case TEC.TEC_TYPE.BIYSK:
                            GetSensors();
                            break;
                        default:
                            break;
                    }
                    break;
                case StatesMachine.Init_TM:
                    ActionReport("Получение идентификаторов датчиков.");
                    switch (tec.type())
                    {
                        case TEC.TEC_TYPE.COMMON:
                        case TEC.TEC_TYPE.BIYSK:
                            GetSensorsTMRequest();
                            break;
                        default:
                            break;
                    }
                    break;
                case StatesMachine.CurrentTime:
                    ActionReport("Получение текущего времени сервера.");
                    GetCurrentTimeRequest();
                    break;
                case StatesMachine.CurrentHours_Fact:
                    ActionReport("Получение получасовых значений.");
                    adminValuesReceived = false;
                    GetHoursRequest(selectedTime.Date);
                    break;
                case StatesMachine.CurrentMins_Fact:
                    ActionReport("Получение трёхминутных значений.");
                    adminValuesReceived = false;
                    GetMinsRequest(lastHour);
                    break;
                case StatesMachine.Current_TM:
                    ActionReport("Получение текущих значений.");
                    GetCurrentTMRequest();
                    break;
                case StatesMachine.LastMinutes_TM:
                    ActionReport("Получение текущих значений 59 мин.");
                    GetLastMinutesTMRequest();
                    break;
                case StatesMachine.RetroHours:
                    ActionReport("Получение получасовых значений.");
                    adminValuesReceived = false;
                    GetHoursRequest(selectedTime.Date);
                    break;
                case StatesMachine.RetroMins:
                    ActionReport("Получение трёхминутных значений.");
                    adminValuesReceived = false;
                    GetMinsRequest(lastHour);
                    break;
                case StatesMachine.PBRValues:
                    ActionReport("Получение данных плана.");
                    GetPBRValuesRequest();
                    break;
                case StatesMachine.AdminValues:
                    ActionReport("Получение административных данных.");
                    adminValuesReceived = false;
                    //switch (tec.type ())
                    //{
                    //    case TEC.TEC_TYPE.COMMON:
                    GetAdminValuesRequest(s_typeFields);
                    //        break;
                    //    case TEC.TEC_TYPE.BIYSK:
                    //        GetAdminValues();
                    //        break;
                    //    default:
                    //        break;
                    //}
                    break;
            }
        }

        private bool StateCheckResponse(StatesMachine state, out bool error, out DataTable table)
        {
            error = false;
            table = null;

            switch (state)
            {
                case StatesMachine.Init_Fact:
                    switch (tec.type())
                    {
                        case TEC.TEC_TYPE.COMMON:
                            return tec.Response(CONN_SETT_TYPE.DATA_FACT, out error, out table);
                        case TEC.TEC_TYPE.BIYSK:
                            return true;
                    }
                    break;
                case StatesMachine.Init_TM:
                    switch (tec.type())
                    {
                        case TEC.TEC_TYPE.COMMON:
                        case TEC.TEC_TYPE.BIYSK:
                            return tec.Response(CONN_SETT_TYPE.DATA_TM, out error, out table);
                            break;
                    }
                    break;
                case StatesMachine.CurrentTime:
                case StatesMachine.CurrentHours_Fact:
                case StatesMachine.CurrentMins_Fact:
                case StatesMachine.RetroHours:
                case StatesMachine.RetroMins:
                    return tec.Response(CONN_SETT_TYPE.DATA_FACT, out error, out table);
                case StatesMachine.Current_TM:
                case StatesMachine.LastMinutes_TM:
                    return tec.Response(CONN_SETT_TYPE.DATA_TM, out error, out table);
                case StatesMachine.PBRValues:
                    //return m_admin.Response(tec.m_arIdListeners[(int)CONN_SETT_TYPE.PBR], out error, out table);
                    return tec.Response(CONN_SETT_TYPE.PBR, out error, out table);
                    //return true; //Имитация получения данных плана
                case StatesMachine.AdminValues:
                    //return m_admin.Response(out error, out table, true);
                    //return m_admin.Response(tec.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], out error, out table);
                    return tec.Response(CONN_SETT_TYPE.ADMIN, out error, out table);
            }

            error = true;

            return false;
        }

        private bool StateResponse(StatesMachine state, DataTable table)
        {
            bool result = false;
            switch (state)
            {
                case StatesMachine.Init_Fact:
                    switch (tec.type())
                    {
                        case TEC.TEC_TYPE.COMMON:
                            result = GetSensorsFactResponse(table);
                            break;
                        case TEC.TEC_TYPE.BIYSK:
                            result = true;
                            break;
                    }
                    if (result == true)
                    {
                    }
                    break;
                case StatesMachine.Init_TM:
                    switch (tec.type())
                    {
                        case TEC.TEC_TYPE.COMMON:
                        case TEC.TEC_TYPE.BIYSK:
                            result = GetSensorsTMResponse(table);
                            break;
                    }
                    if (result == true)
                    {
                    }
                    break;
                case StatesMachine.CurrentTime:
                    result = GetCurrentTimeReponse(table);
                    if (result == true)
                    {
                        //this.BeginInvoke(delegateShowValues, "StatesMachine.CurrentTime");
                        selectedTime = selectedTime.AddSeconds(-parameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ERROR_DELAY]);
                        this.BeginInvoke(delegateSetNowDate, true);
                    }
                    break;
                case StatesMachine.CurrentHours_Fact:
                    ClearValues();
                    //GenerateHoursTable(seasonJumpE.SummerToWinter, 3, table);
                    result = GetHoursResponse(table);
                    if (result == true)
                    {
                    }
                    else
                        ;
                    break;
                case StatesMachine.CurrentMins_Fact:
                    //GenerateMinsTable(seasonJumpE.None, 5, table);
                    result = GetMinsResponse(table);
                    if (result == true)
                    {
                        //this.BeginInvoke(delegateUpdateGUI_Fact, lastHour, lastMin);
                    }
                    else
                        ;
                    break;
                case StatesMachine.Current_TM:
                    result = GetCurrentTMResponse(table);
                    if (result == true)
                    {
                        this.BeginInvoke(delegateUpdateGUI_TM);
                    }
                    else
                        ;
                    break;
                case StatesMachine.LastMinutes_TM:
                    result = GetLastMinutesTMResponse(table);
                    if (result == true)
                    {
                    }
                    else
                        ;
                    break;
                case StatesMachine.RetroHours:
                    ClearValues();
                    result = GetHoursResponse(table);
                    if (result == true)
                    {
                    }
                    else
                        ;
                    break;
                case StatesMachine.RetroMins:
                    result = GetMinsResponse(table);
                    if (result == true)
                    {
                        this.BeginInvoke(delegateUpdateGUI_Fact, lastHour, lastMin);
                    }
                    else
                        ;
                    break;
                case StatesMachine.PBRValues:
                    ClearPBRValues();
                    result = GetPBRValuesResponse(table);
                    if (result == true)
                    {
                    }
                    else
                        ;
                    break;
                case StatesMachine.AdminValues:
                    ClearAdminValues();
                    result = GetAdminValuesResponse(table);
                    if (result == true)
                    {
                        //this.BeginInvoke(delegateShowValues, "StatesMachine.AdminValues");
                        ComputeRecomendation(lastHour);
                        adminValuesReceived = true;
                        this.BeginInvoke(delegateUpdateGUI_Fact, lastHour, lastMin);
                    }
                    else
                        ;
                    break;
            }

            if (result == true)
                m_report.errored_state =
                m_report.actioned_state = false;
            else
                ;

            return result;
        }

        private void StateErrors(StatesMachine state, bool response)
        {
            string reason = string.Empty,
                    waiting = string.Empty;

            switch (state)
            {
                case StatesMachine.Init_Fact:
                    reason = @"идентификаторов датчиков (факт.)";
                    waiting = @"Переход в ожидание";
                    break;
                case StatesMachine.Init_TM:
                    reason = @"идентификаторов датчиков (телемеханика)";
                    waiting = @"Переход в ожидание";
                    break;
                case StatesMachine.CurrentTime:
                    reason = @"текущего времени сервера";
                    waiting = @"Ожидание " + parameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME].ToString() + " секунд";
                    break;
                case StatesMachine.CurrentHours_Fact:
                    reason = @"получасовых значений";
                    waiting = @"Ожидание " + parameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME].ToString() + " секунд";
                    break;
                case StatesMachine.CurrentMins_Fact:
                    reason = @"трёхминутных значений";
                    waiting = @"Ожидание " + parameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME].ToString() + " секунд";
                    break;
                case StatesMachine.Current_TM:
                    reason = @"текущих значений";
                    waiting = @"Ожидание " + parameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME].ToString() + " секунд";
                    break;
                case StatesMachine.LastMinutes_TM:
                    reason = @"текущих значений 59 мин.";
                    waiting = @"Ожидание " + parameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME].ToString() + " секунд";
                    break;
                case StatesMachine.RetroHours:
                    reason = @"получасовых значений";
                    waiting = @"Переход в ожидание";
                    break;
                case StatesMachine.RetroMins:
                    reason = @"трёхминутных значений";
                    waiting = @"Переход в ожидание";
                    break;
                case StatesMachine.PBRValues:
                    reason = @"данных плана";
                    break;
                case StatesMachine.AdminValues:
                    reason = @"административных значений";
                    break;
            }

            if (response)
                reason = @"разбора " + reason;
            else
                reason = @"получения " + reason;

            if (waiting.Equals(string.Empty) == true)
                ErrorReport("Ошибка " + reason + ". " + waiting + ".");
            else
                ErrorReport("Ошибка " + reason + ".");
        }

        private void TecView_ThreadFunction(object data)
        {
            int index;
            StatesMachine currentState;

            while (threadIsWorking)
            {
                sem.WaitOne();

                index = 0;

                lock (lockValue)
                {
                    if (states.Count == 0)
                        continue;
                    else
                        ;

                    currentState = states[index];
                    newState = false;
                }

                while (true)
                {
                    bool error = true;
                    bool dataPresent = false;
                    DataTable table = null;
                    for (int i = 0; i < DbInterface.MAX_RETRY && !dataPresent && !newState; i++)
                    {
                        if (error)
                            StateRequest(currentState);
                        else
                            ;

                        error = false;
                        for (int j = 0; j < DbInterface.MAX_WAIT_COUNT && !dataPresent && !error && !newState; j++)
                        {
                            System.Threading.Thread.Sleep(DbInterface.WAIT_TIME_MS);
                            dataPresent = StateCheckResponse(currentState, out error, out table);
                        }
                    }

                    bool responseIsOk = true;
                    if (dataPresent && !error && !newState)
                        responseIsOk = StateResponse(currentState, table);
                    else
                        ;

                    if ((!responseIsOk || !dataPresent || error) && !newState)
                    {
                        StateErrors(currentState, !responseIsOk);
                        lock (lockValue)
                        {
                            if (!newState)
                            {
                                states.Clear();
                                newState = true;
                            }
                            else
                                ;
                        }
                    }

                    index++;

                    lock (lockValue)
                    {
                        if (index == states.Count)
                            break;
                        else
                            ;

                        if (newState == true)
                            break;
                        else
                            ;

                        currentState = states[index];
                    }
                }
            }
            try
            {
                sem.Release(1);
            }
            catch
            {
            }
        }
        
        private void TickTime()
        {
            serverTime = serverTime.AddSeconds(1);
            pnlQuickData.lblServerTime.Text = serverTime.ToString("HH:mm:ss");
        }

        private void TimerCurrent_Tick(Object stateInfo)
        {
            Invoke(delegateTickTime);
            if (currHour && isActive)
                if (((currValuesPeriod++) * 1000) >= parameters.m_arParametrSetup [(int)FormParameters.PARAMETR_SETUP.POLL_TIME]) {
                    currValuesPeriod = 0;
                    NewDateRefresh();
                }
                else
                    ;
            else
                ;

            ((ManualResetEvent)stateInfo).WaitOne ();
            try
            {
                timerCurrent.Change(1000, Timeout.Infinite);
            }
            catch (Exception e)
            {
                Logging.Logg().LogExceptionToFile(e, "Обращение к переменной 'timerCurrent'");
            }
        }

        private void stctrViewPanel1_SplitterMoved(object sender, SplitterEventArgs e)
        {
        }

        private void stctrViewPanel2_SplitterMoved(object sender, SplitterEventArgs e)
        {
        }
    }
}
