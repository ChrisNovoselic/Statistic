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

namespace Statistic
{
    public class TecView : Panel
    {
        private System.Windows.Forms.Panel pnlGraphHours;
        private System.Windows.Forms.Panel pnlGraphMins;
        private System.Windows.Forms.Panel pnlTG;
        private System.Windows.Forms.DateTimePicker dtprDate;
        private System.Windows.Forms.Panel pnlCommon;
        private System.Windows.Forms.Label lblCommonPVal;
        private System.Windows.Forms.Label lblCommonP;
        private System.Windows.Forms.Label lblPBRRecVal;
        private System.Windows.Forms.Label lblPBRrec;
        private System.Windows.Forms.Label lblAverPVal;
        private System.Windows.Forms.Label lblAverP;
        private System.Windows.Forms.DataGridView dgwHours;
        private System.Windows.Forms.DataGridViewTextBoxColumn Hour;
        private System.Windows.Forms.DataGridViewTextBoxColumn FactHour;
        private System.Windows.Forms.DataGridViewTextBoxColumn PBRHour;
        private System.Windows.Forms.DataGridViewTextBoxColumn PBReHour;
        private System.Windows.Forms.DataGridViewTextBoxColumn UDGeHour;
        private System.Windows.Forms.DataGridViewTextBoxColumn DeviationHour;
        private System.Windows.Forms.DataGridView dgwMins;
        private System.Windows.Forms.DataGridViewTextBoxColumn Min;
        private System.Windows.Forms.DataGridViewTextBoxColumn FactMin;
        private System.Windows.Forms.DataGridViewTextBoxColumn PBRMin;
        private System.Windows.Forms.DataGridViewTextBoxColumn PBReMin;
        private System.Windows.Forms.DataGridViewTextBoxColumn UDGeMin;
        private System.Windows.Forms.DataGridViewTextBoxColumn DeviationMin;
        private System.Windows.Forms.SplitContainer stctrView;
        private System.Windows.Forms.Button btnSetNow;
        private ZedGraph.ZedGraphControl zedGraphMins;
        private ZedGraph.ZedGraphControl zedGraphHours;
        private System.Windows.Forms.Label lblServerTime;
        private System.Windows.Forms.Label lblLayoutNumber;

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

        private DelegateFunc delegateFillDefMins;
        private DelegateFunc delegateFillDefHours;

        private DelegateIntFunc delegateFillMins;
        private DelegateFunc delegateFillHours;
        private DelegateIntFunc delegateDrawMins;
        private DelegateFunc delegateDrawHours;

        private DelegateBoolFunc delegateSetNowDate;

        private DelegateFunc delegateShowTGPower;
        private DelegateFunc delegateShowCommonPower;

        private DelegateIntIntFunc delegateUpdateGUI;

        private DelegateStringFunc delegateShowValues;

        private object lockValue;
        
        private Thread taskThread;
        private Semaphore sem;
        private volatile bool threadIsWorking;
        private volatile bool newState;
        private volatile List<StatesMachine> states;
        private int currValuesPeriod = 0;
        private System.Threading.Timer timerCurrent;
        private DelegateFunc delegateTickTime;

        private Admin admin;
        private GraphicsSettings graphSettings;
        private Parameters parameters;        

        private volatile bool currHour;
        private volatile int lastHour;
        private volatile int lastReceivedHour;
        private volatile int lastMin;
        private volatile bool lastMinError;
        private volatile bool lastHourError;
        private volatile bool lastHourHalfError;
        private volatile string lastLayout;

        public volatile string last_error;
        public DateTime last_time_error;
        public volatile bool errored_state;

        public volatile string last_action;
        public DateTime last_time_action;
        public volatile bool actioned_state;

        private enum StatesMachine
        { 
            Init,
            CurrentTime,
            CurrentHours,
            CurrentMins,
            RetroHours,
            RetroMins,
            PBRValues,
            AdminValues,
        }

        private enum seasonJumpE
        {
            None,
            WinterToSummer,
            SummerToWinter,
        }
        
        private struct valuesS
        {
            public volatile double[] valuesFact;
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
        }

        private valuesS valuesMins;
        private valuesS valuesHours;
        private DateTime selectedTime;
        private DateTime serverTime;

        DataTable m_tablePBRResponse;

        private double recomendation;

        private volatile string sensorsString = "";
        private volatile string [] sensorsStrings = {"", ""}; //Только для особенной ТЭЦ (Бийск)

        private TG[] sensorId2TG;

        private List<System.Windows.Forms.Label> tgsName;
        private List<System.Windows.Forms.Label> tgsValue;

        public volatile TEC tec;
        private volatile int num_TECComponent;

        private volatile int countTG;
        private bool update;

        private Admin.TECComponentsAdminStruct adminValues;

        public volatile bool isActive;

        private StatusStrip stsStrip;

        private bool started;

        private volatile bool adminValuesReceived;

        private volatile bool recalcAver;

        private void InitializeComponent()
        {
            this.lblServerTime = new System.Windows.Forms.Label();
            this.lblLayoutNumber = new System.Windows.Forms.Label();
            this.zedGraphMins = new ZedGraphControl();
            this.zedGraphHours = new ZedGraphControl();
            this.dgwHours = new System.Windows.Forms.DataGridView();
            this.Hour = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FactHour = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PBRHour = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PBReHour = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UDGeHour = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DeviationHour = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlCommon = new System.Windows.Forms.Panel();
            this.lblPBRRecVal = new System.Windows.Forms.Label();
            this.lblPBRrec = new System.Windows.Forms.Label();
            this.lblCommonPVal = new System.Windows.Forms.Label();
            this.lblCommonP = new System.Windows.Forms.Label();
            this.lblAverPVal = new System.Windows.Forms.Label();
            this.lblAverP = new System.Windows.Forms.Label();
            this.pnlTG = new System.Windows.Forms.Panel();
            this.dtprDate = new System.Windows.Forms.DateTimePicker();
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
            this.btnSetNow = new System.Windows.Forms.Button();

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
            this.pnlCommon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgwMins)).BeginInit();
            this.stctrView.Panel1.SuspendLayout();
            this.stctrView.Panel2.SuspendLayout();
            this.stctrView.SuspendLayout();
            this.SuspendLayout();

            this.Controls.Add(this.stctrView);
            this.Controls.Add(this.pnlCommon);
            this.Controls.Add(this.pnlTG);
            this.Controls.Add(this.dtprDate);
            this.Controls.Add(this.btnSetNow);
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Location = new System.Drawing.Point(40, 58);
            this.Name = "pnlView";
            this.Size = new System.Drawing.Size(705, 727);
            this.TabIndex = 0;
            // 
            // zedGraphMin
            // 
            this.zedGraphMins.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraphMins.Location = new System.Drawing.Point(0, 0);
            this.zedGraphMins.Name = "zedGraphMin";
            this.zedGraphMins.ScrollGrace = 0;
            this.zedGraphMins.ScrollMaxX = 0;
            this.zedGraphMins.ScrollMaxY = 0;
            this.zedGraphMins.ScrollMaxY2 = 0;
            this.zedGraphMins.ScrollMinX = 0;
            this.zedGraphMins.ScrollMinY = 0;
            this.zedGraphMins.ScrollMinY2 = 0;
            this.zedGraphMins.Size = new System.Drawing.Size(592, 471);
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
            this.zedGraphHours.Location = new System.Drawing.Point(0, 0);
            this.zedGraphHours.Name = "zedGraphHour";
            this.zedGraphHours.ScrollGrace = 0;
            this.zedGraphHours.ScrollMaxX = 0;
            this.zedGraphHours.ScrollMaxY = 0;
            this.zedGraphHours.ScrollMaxY2 = 0;
            this.zedGraphHours.ScrollMinX = 0;
            this.zedGraphHours.ScrollMinY = 0;
            this.zedGraphHours.ScrollMinY2 = 0;
            this.zedGraphHours.Size = new System.Drawing.Size(592, 471);
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
            this.dgwHours.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.dgwHours.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgwHours.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Hour,
            this.FactHour,
            this.PBRHour,
            this.PBReHour,
            this.UDGeHour,
            this.DeviationHour});
            this.dgwHours.Location = new System.Drawing.Point(3, 3);
            this.dgwHours.Name = "dgwHour";
            this.dgwHours.ReadOnly = true;
            this.dgwHours.RowHeadersVisible = false;
            this.dgwHours.Size = new System.Drawing.Size(329, 299);
            this.dgwHours.TabIndex = 7;
            this.dgwHours.RowTemplate.Resizable = DataGridViewTriState.False;
            // 
            // Hour
            // 
            this.Hour.HeaderText = "Час";
            this.Hour.Name = "Hour";
            this.Hour.ReadOnly = true;
            this.Hour.Width = 50;
            this.Hour.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // FactHour
            // 
            this.FactHour.HeaderText = "Факт";
            this.FactHour.Name = "FactHour";
            this.FactHour.ReadOnly = true;
            this.FactHour.Width = 50;
            this.FactHour.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // PBRHour
            // 
            this.PBRHour.HeaderText = "ПБР";
            this.PBRHour.Name = "PBRHour";
            this.PBRHour.ReadOnly = true;
            this.PBRHour.Width = 50;
            this.PBRHour.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // PBReHour
            // 
            this.PBReHour.HeaderText = "ПБРэ";
            this.PBReHour.Name = "PBReHour";
            this.PBReHour.ReadOnly = true;
            this.PBReHour.Width = 50;
            this.PBReHour.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // UDGeHour
            // 
            this.UDGeHour.HeaderText = "УДГэ";
            this.UDGeHour.Name = "UDGeHour";
            this.UDGeHour.ReadOnly = true;
            this.UDGeHour.Width = 60;
            this.UDGeHour.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // DeviationHour
            // 
            this.DeviationHour.HeaderText = "+/-";
            this.DeviationHour.Name = "DeviationHour";
            this.DeviationHour.ReadOnly = true;
            this.DeviationHour.Width = 50;
            this.DeviationHour.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            //
            // btnSetNow
            //
            this.btnSetNow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSetNow.Location = new System.Drawing.Point(6, 643);
            this.btnSetNow.Name = "btnSetNow";
            this.btnSetNow.Size = new System.Drawing.Size(93, 23);
            this.btnSetNow.TabIndex = 2;
            this.btnSetNow.Text = "Текущий час";
            this.btnSetNow.UseVisualStyleBackColor = true;
            this.btnSetNow.Click += new System.EventHandler(this.btnSetNow_Click);
            // 
            // pnlCommon
            // 
            this.pnlCommon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlCommon.Controls.Add(this.lblPBRRecVal);
            this.pnlCommon.Controls.Add(this.lblPBRrec);
            this.pnlCommon.Controls.Add(this.lblServerTime);
            this.pnlCommon.Controls.Add(this.lblLayoutNumber);
            this.pnlCommon.Controls.Add(this.lblCommonPVal);
            this.pnlCommon.Controls.Add(this.lblCommonP);
            this.pnlCommon.Controls.Add(this.lblAverPVal);
            this.pnlCommon.Controls.Add(this.lblAverP);
            this.pnlCommon.Location = new System.Drawing.Point(140, 620);
            this.pnlCommon.Name = "pnlCommon";
            this.pnlCommon.Size = new System.Drawing.Size(562, 46);
            this.pnlCommon.TabIndex = 6;
            // 
            // lblCommonPVal
            // 
            this.lblCommonPVal.AutoSize = true;
            this.lblCommonPVal.BackColor = System.Drawing.Color.Black;
            this.lblCommonPVal.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblCommonPVal.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblCommonPVal.ForeColor = System.Drawing.Color.LimeGreen;
            this.lblCommonPVal.Location = new System.Drawing.Point(120, 0);
            this.lblCommonPVal.Name = "lblCommonPVal";
            this.lblCommonPVal.AutoSize = false;
            this.lblCommonPVal.Size = new System.Drawing.Size(74, 27);
            this.lblCommonPVal.TabIndex = 1;
            this.lblCommonPVal.Text = "0";
            // 
            // lblCommonP
            // 
            this.lblCommonP.AutoSize = true;
            this.lblCommonP.Location = new System.Drawing.Point(80, 6);
            this.lblCommonP.Name = "lblCommonP";
            this.lblCommonP.Size = new System.Drawing.Size(53, 13);
            this.lblCommonP.TabIndex = 0;
            this.lblCommonP.Text = "P сум";//"Суммарная мощность генераторов";
            // 
            // lblAverPVal
            // 
            this.lblAverPVal.AutoSize = true;
            this.lblAverPVal.BackColor = System.Drawing.Color.Black;
            this.lblAverPVal.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblAverPVal.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblAverPVal.ForeColor = System.Drawing.Color.LimeGreen;
            this.lblAverPVal.Location = new System.Drawing.Point(240, 0);
            this.lblAverPVal.Name = "lblAverPVal";
            this.lblAverPVal.AutoSize = false;
            this.lblAverPVal.Size = new System.Drawing.Size(74, 27);
            this.lblAverPVal.TabIndex = 1;
            this.lblAverPVal.Text = "0";
            // 
            // lblAverP
            // 
            this.lblAverP.AutoSize = true;
            this.lblAverP.Location = new System.Drawing.Point(210, 6);
            this.lblAverP.Name = "lblCommonP";
            this.lblAverP.Size = new System.Drawing.Size(53, 13);
            this.lblAverP.TabIndex = 0;
            this.lblAverP.Text = "P ср";//"Средняя мощность за час";
            // 
            // lblPBRRecVal
            // 
            this.lblPBRRecVal.AutoSize = true;
            this.lblPBRRecVal.BackColor = System.Drawing.Color.Black;
            this.lblPBRRecVal.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblPBRRecVal.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblPBRRecVal.ForeColor = System.Drawing.Color.Yellow;
            this.lblPBRRecVal.Location = new System.Drawing.Point(360, 0);
            this.lblPBRRecVal.Name = "lblPBRRecVal";
            this.lblPBRRecVal.AutoSize = false;
            this.lblPBRRecVal.Size = new System.Drawing.Size(74, 27);
            this.lblPBRRecVal.TabIndex = 3;
            this.lblPBRRecVal.Text = "0";
            // 
            // lblPBRrec
            // 
            this.lblPBRrec.AutoSize = true;
            this.lblPBRrec.Location = new System.Drawing.Point(325, 6);
            this.lblPBRrec.Name = "lblPBRrec";
            this.lblPBRrec.Size = new System.Drawing.Size(43, 13);
            this.lblPBRrec.TabIndex = 2;
            this.lblPBRrec.Text = "P рек";//"Рекомендуемая мощность";
            // 
            // pnlTG
            // 
            this.pnlTG.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTG.Location = new System.Drawing.Point(3, 666);
            this.pnlTG.Name = "pnlTG";
            this.pnlTG.Size = new System.Drawing.Size(699, 58);
            this.pnlTG.TabIndex = 5;
            // 
            // dtprDate
            // 
            this.dtprDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dtprDate.Location = new System.Drawing.Point(6, 622);
            this.dtprDate.Name = "dtprDate";
            this.dtprDate.Size = new System.Drawing.Size(130, 20);
            this.dtprDate.TabIndex = 4;
            this.dtprDate.ValueChanged += new System.EventHandler(this.dtprDate_ValueChanged);
            // 
            // pnlGraphHour
            // 
            this.pnlGraphHours.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlGraphHours.Location = new System.Drawing.Point(338, 3);
            this.pnlGraphHours.Name = "pnlGraphHour";
            this.pnlGraphHours.Size = new System.Drawing.Size(377, 299);
            this.pnlGraphHours.TabIndex = 3;
            this.pnlGraphHours.Controls.Add(zedGraphHours);
            // 
            // pnlGraphMin
            // 
            this.pnlGraphMins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlGraphMins.Location = new System.Drawing.Point(338, 3);
            this.pnlGraphMins.Name = "pnlGraphMin";
            this.pnlGraphMins.Size = new System.Drawing.Size(377, 294);
            this.pnlGraphMins.TabIndex = 2;
            this.pnlGraphMins.Controls.Add(zedGraphMins);
            // 
            // dgwMins
            // 
            this.dgwMins.AllowUserToAddRows = false;
            this.dgwMins.AllowUserToDeleteRows = false;
            this.dgwMins.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.dgwMins.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgwMins.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Min,
            this.FactMin,
            this.PBRMin,
            this.PBReMin,
            this.UDGeMin,
            this.DeviationMin});
            this.dgwMins.Location = new System.Drawing.Point(3, 3);
            this.dgwMins.Name = "dgwMin";
            this.dgwMins.ReadOnly = true;
            this.dgwMins.RowHeadersVisible = false;
            this.dgwMins.Size = new System.Drawing.Size(329, 294);
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
            this.stctrView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.stctrView.Location = new System.Drawing.Point(0, 3);
            this.stctrView.Name = "stctrView";
            this.stctrView.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // stctrView.Panel1
            // 
            this.stctrView.Panel1.Controls.Add(this.dgwMins);
            this.stctrView.Panel1.Controls.Add(this.pnlGraphMins);
            // 
            // stctrView.Panel2
            // 
            this.stctrView.Panel2.Controls.Add(this.dgwHours);
            this.stctrView.Panel2.Controls.Add(this.pnlGraphHours);
            this.stctrView.Size = new System.Drawing.Size(702, 613);
            this.stctrView.SplitterDistance = 301;
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
            
            // 
            // lblServerTime
            // 
            this.lblServerTime.AutoSize = false;
            this.lblServerTime.Location = new System.Drawing.Point(0, 1);
            this.lblServerTime.Name = "lblServerTime";
            this.lblServerTime.Size = new System.Drawing.Size(67, 20);
            this.lblServerTime.TabIndex = 5;
            this.lblServerTime.Text = "--:--:--";
            this.lblServerTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblServerTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblServerTime.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblLayoutNumber
            // 
            this.lblLayoutNumber.AutoSize = false;
            this.lblLayoutNumber.Location = new System.Drawing.Point(0, 22);
            this.lblLayoutNumber.Name = "lblLayoutNumber";
            this.lblLayoutNumber.Size = new System.Drawing.Size(67, 20);
            this.lblLayoutNumber.TabIndex = 5;
            this.lblLayoutNumber.Text = "---";
            this.lblLayoutNumber.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblLayoutNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblLayoutNumber.TextAlign = ContentAlignment.MiddleCenter;

            ((System.ComponentModel.ISupportInitialize)(this.dgwHours)).EndInit();
            this.pnlCommon.ResumeLayout(false);
            this.pnlCommon.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgwMins)).EndInit();
            this.stctrView.Panel1.ResumeLayout(false);
            this.stctrView.Panel2.ResumeLayout(false);
            this.stctrView.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        public TecView(TEC tec, int num_comp, Admin admin, StatusStrip sts, GraphicsSettings gs, Parameters par)
        {
            InitializeComponent();

            started = false;

            dgvCellStyleError = new DataGridViewCellStyle();
            dgvCellStyleError.BackColor = Color.Red;
            dgvCellStyleCommon = new DataGridViewCellStyle();

            this.admin = admin;
            this.graphSettings = gs;
            this.parameters = par;

            if (tec.type () == TEC.TEC_TYPE.BIYSK)
                ; //this.parameters = MainForm.papar;
            else
                ;

            currHour = true;
            lastHour = 0;
            lastMin = 0;
            update = false;
            isActive = false;
            errored_state = false;
            actioned_state = false;
            recalcAver = true;

            lockValue = new object();

            adminValues = new Admin.TECComponentsAdminStruct(24);

            valuesMins = new valuesS();
            valuesMins.valuesFact = new double[21];
            valuesMins.valuesPBR = new double[21];
            valuesMins.valuesPBRe = new double[21];
            valuesMins.valuesUDGe = new double[21];
            valuesMins.valuesDiviation = new double[21];

            valuesHours = new valuesS();
            valuesHours.valuesFact = new double[24];
            valuesHours.valuesPBR = new double[24];
            valuesHours.valuesPBRe = new double[24];
            valuesHours.valuesUDGe = new double[24];
            valuesHours.valuesDiviation = new double[24];

            stsStrip = sts;

            tgsName = new List<System.Windows.Forms.Label>();
            tgsValue = new List<System.Windows.Forms.Label>();
            this.tec = tec;
            this.num_TECComponent = num_comp;

            int positionXName = 15, positionXValue = 4, positionYName = 6, positionYValue = 19;
            float value = 0;
            countTG = 0;
            if (num_comp < 0) // значит этот view будет суммарным для всех ГТП
            {
                foreach (TECComponent g in tec.list_TECComponents)
                {
                    foreach (TG t in g.TG)
                    {
                        countTG++;
                        System.Windows.Forms.Label lblName = new System.Windows.Forms.Label();

                        lblName.AutoSize = true;
                        lblName.Location = new System.Drawing.Point(positionXName, positionYName);
                        lblName.Name = "lblName" + t.name;
                        lblName.AutoSize = false;
                        lblName.Size = new System.Drawing.Size(32, 13);
                        lblName.TabIndex = 4;
                        lblName.Text = t.name;

                        tgsName.Add(lblName);

                        System.Windows.Forms.Label lblValue = new System.Windows.Forms.Label();

                        lblValue.AutoSize = true;
                        lblValue.BackColor = System.Drawing.Color.Black;
                        lblValue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        lblValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                        lblValue.ForeColor = System.Drawing.Color.LimeGreen;
                        lblValue.Location = new System.Drawing.Point(positionXValue, positionYValue);
                        lblValue.Name = "lblValue" + t.name;
                        lblValue.Size = new System.Drawing.Size(63, 27);
                        lblValue.AutoSize = false;
                        lblValue.TabIndex = 5;
                        lblValue.Text = value.ToString();

                        tgsValue.Add(lblValue);

                        positionXName += 69;
                        positionXValue += 69;


                        this.pnlTG.Controls.Add(lblName);
                        this.pnlTG.Controls.Add(lblValue);
                    }
                }
            }
            else
            {
                foreach (TG t in tec.list_TECComponents[num_comp].TG)
                {
                    countTG++;
                    System.Windows.Forms.Label lblName = new System.Windows.Forms.Label();

                    lblName.AutoSize = true;
                    lblName.Location = new System.Drawing.Point(positionXName, positionYName);
                    lblName.Name = "lblName" + t.name;
                    lblName.AutoSize = false;
                    lblName.Size = new System.Drawing.Size(32, 13);
                    lblName.TabIndex = 4;
                    lblName.Text = t.name;

                    tgsName.Add(lblName);

                    System.Windows.Forms.Label lblValue = new System.Windows.Forms.Label();

                    lblValue.AutoSize = true;
                    lblValue.BackColor = System.Drawing.Color.Black;
                    lblValue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                    lblValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                    lblValue.ForeColor = System.Drawing.Color.LimeGreen;
                    lblValue.Location = new System.Drawing.Point(positionXValue, positionYValue);
                    lblValue.Name = "lblValue" + t.name;
                    lblValue.Size = new System.Drawing.Size(63, 27);
                    lblValue.AutoSize = false;
                    lblValue.TabIndex = 5;
                    lblValue.Text = value.ToString();

                    tgsValue.Add(lblValue);

                    positionXName += 69;
                    positionXValue += 69;


                    this.pnlTG.Controls.Add(lblName);
                    this.pnlTG.Controls.Add(lblValue);
                }
            }

            sensorId2TG = new TG[countTG];

            this.dgwMins.Rows.Add(21);
            this.dgwHours.Rows.Add(25);

            delegateFillDefMins = new DelegateFunc(FillDefaultMins);
            delegateFillDefHours = new DelegateFunc(FillDefaultHours);

            delegateFillMins = new DelegateIntFunc(FillGridMins);
            delegateFillHours = new DelegateFunc(FillGridHours);
            delegateDrawMins = new DelegateIntFunc(DrawGraphMins);
            delegateDrawHours = new DelegateFunc(DrawGraphHours);

            delegateSetNowDate = new DelegateBoolFunc(SetNowDate);

            delegateShowTGPower = new DelegateFunc(ShowTGPower);
            delegateShowCommonPower = new DelegateFunc(ShowCommonPower);

            delegateUpdateGUI = new DelegateIntIntFunc(UpdateGUI);

            delegateShowValues = new DelegateStringFunc(ShowValues);

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

            if (valuesHours.season == seasonJumpE.SummerToWinter)
                count = 25;
            else
                if (valuesHours.season == seasonJumpE.WinterToSummer)
                    count = 23;
                else
                    count = 24;

            this.dgwHours.Rows.Add(count + 1);

            for (int i = 0; i < count; i++)
            {
                if (valuesHours.season == seasonJumpE.SummerToWinter)
                {
                    if (i <= valuesHours.hourAddon)
                        this.dgwHours.Rows[i].Cells[0].Value = (i + 1).ToString();
                    else
                        if (i == valuesHours.hourAddon + 1)
                            this.dgwHours.Rows[i].Cells[0].Value = i.ToString() + "*";
                        else
                            this.dgwHours.Rows[i].Cells[0].Value = i.ToString();
                }
                else
                    if (valuesHours.season == seasonJumpE.WinterToSummer)
                    {
                        if (i < valuesHours.hourAddon)
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
                valuesFact[i] = valuesMins.valuesFact[i + 1];
                valuesUDGe[i] = valuesMins.valuesUDGe[i + 1];
            }

            //double[] valuesPDiviation = new double[itemscount];

            //double[] valuesODiviation = new double[itemscount];

            double minimum = double.MaxValue, minimum_scale;
            double maximum = 0, maximum_scale;
            bool noValues = true;

            for (int i = 0; i < itemscount; i++)
            {
                names[i] = ((i + 1) * 3).ToString();
                //valuesPDiviation[i] = valuesMins.valuesUDGe[i] + valuesMins.valuesDiviation[i];
                //valuesODiviation[i] = valuesMins.valuesUDGe[i] - valuesMins.valuesDiviation[i];

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

            if (graphSettings.graphTypes == GraphicsSettings.GraphTypes.Bar)
            {
                BarItem curve1 = pane.AddBar("Мощность", null, valuesFact, graphSettings.pColor);

                BarItem curve0 = pane.AddBar("Рекомендуемая мощность", null, valuesRecommend, graphSettings.recColor);
            }
            else
            {
                if (graphSettings.graphTypes == GraphicsSettings.GraphTypes.Linear)
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
            
            if (valuesHours.addonValues && hour == valuesHours.hourAddon)
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

            if (valuesHours.season == seasonJumpE.SummerToWinter)
                itemscount = 25;
            else
                if (valuesHours.season == seasonJumpE.WinterToSummer)
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
                if (valuesHours.season == seasonJumpE.SummerToWinter)
                {
                    if (i <= valuesHours.hourAddon)
                    {
                        names[i] = (i + 1).ToString();
                        valuesPDiviation[i] = valuesHours.valuesUDGe[i] + valuesHours.valuesDiviation[i];
                        valuesODiviation[i] = valuesHours.valuesUDGe[i] - valuesHours.valuesDiviation[i];
                        valuesUDGe[i] = valuesHours.valuesUDGe[i];
                        valuesFact[i] = valuesHours.valuesFact[i];
                    }
                    else
                        if (i == valuesHours.hourAddon + 1)
                        {
                            names[i] = i.ToString() + "*";
                            valuesPDiviation[i] = valuesHours.valuesUDGeAddon + valuesHours.valuesDiviationAddon;
                            valuesODiviation[i] = valuesHours.valuesUDGeAddon - valuesHours.valuesDiviationAddon;
                            valuesUDGe[i] = valuesHours.valuesUDGeAddon;
                            valuesFact[i] = valuesHours.valuesFactAddon;
                        }
                        else
                        {
                            this.dgwHours.Rows[i].Cells[0].Value = i.ToString();
                            names[i] = i.ToString();
                            valuesPDiviation[i] = valuesHours.valuesUDGe[i - 1] + valuesHours.valuesDiviation[i - 1];
                            valuesODiviation[i] = valuesHours.valuesUDGe[i - 1] - valuesHours.valuesDiviation[i - 1];
                            valuesUDGe[i] = valuesHours.valuesUDGe[i - 1];
                            valuesFact[i] = valuesHours.valuesFact[i - 1];
                        }

                }
                else
                    if (valuesHours.season == seasonJumpE.WinterToSummer)
                    {
                        if (i < valuesHours.hourAddon)
                        {
                            names[i] = (i + 1).ToString();
                            valuesPDiviation[i] = valuesHours.valuesUDGe[i] + valuesHours.valuesDiviation[i];
                            valuesODiviation[i] = valuesHours.valuesUDGe[i] - valuesHours.valuesDiviation[i];
                            valuesUDGe[i] = valuesHours.valuesUDGe[i];
                            valuesFact[i] = valuesHours.valuesFact[i];
                        }
                        else
                        {
                            names[i] = (i + 2).ToString();
                            valuesPDiviation[i] = valuesHours.valuesUDGe[i + 1] + valuesHours.valuesDiviation[i + 1];
                            valuesODiviation[i] = valuesHours.valuesUDGe[i + 1] - valuesHours.valuesDiviation[i + 1];
                            valuesUDGe[i] = valuesHours.valuesUDGe[i + 1];
                            valuesFact[i] = valuesHours.valuesFact[i + 1];
                        }
                    }
                    else
                    {
                        names[i] = (i + 1).ToString();
                        valuesPDiviation[i] = valuesHours.valuesUDGe[i] + valuesHours.valuesDiviation[i];
                        valuesODiviation[i] = valuesHours.valuesUDGe[i] - valuesHours.valuesDiviation[i];
                        valuesUDGe[i] = valuesHours.valuesUDGe[i];
                        valuesFact[i] = valuesHours.valuesFact[i];
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


            if (graphSettings.graphTypes == GraphicsSettings.GraphTypes.Bar)
            {
                BarItem curve1 = pane.AddBar("Мощность", null, valuesFact, graphSettings.pColor);
            }
            else
            {
                if (graphSettings.graphTypes == GraphicsSettings.GraphTypes.Linear)
                {
                    int valuescount;

                    if (valuesHours.season == seasonJumpE.SummerToWinter)
                        valuescount = lastHour + 1;
                    else
                        if (valuesHours.season == seasonJumpE.WinterToSummer)
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
            pane.Title.Text = "Мощность на " + dtprDate.Value.ToShortDateString();

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
                    ShowCommonPower();
                    ShowTGPower();
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
                    if (valuesHours.season == seasonJumpE.SummerToWinter)
                    {
                        if (index <= valuesHours.hourAddon)
                        {
                            lastHour = index;
                            valuesHours.addonValues = false;
                        }
                        else
                        {
                            if (index == valuesHours.hourAddon + 1)
                            {
                                lastHour = index - 1;
                                valuesHours.addonValues = true;
                            }
                            else
                            {
                                lastHour = index - 1;
                                valuesHours.addonValues = false;
                            }
                        }
                    }
                    else
                        if (valuesHours.season == seasonJumpE.WinterToSummer)
                        {
                            if (index < valuesHours.hourAddon)
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
                    catch
                    {
                    }

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
                            ws.Cells[0, 0].Value = tec.name;
                        else
                        {
                            ws.Cells[0, 0].Value = tec.name;
                            foreach (TECComponent g in tec.list_TECComponents)
                                ws.Cells[0, 0].Value += ", " + g.name;
                        }
                    }
                    else
                    {
                        ws.Cells[0, 0].Value = tec.name + ", " + tec.list_TECComponents[num_TECComponent].name;
                    }

                    if (valuesHours.addonValues && hour == valuesHours.hourAddon)
                        ws.Cells[1, 0].Value = "Мощность на " + (hour + 1).ToString() + "* час " + dtprDate.Value.ToShortDateString();
                    else
                        ws.Cells[1, 0].Value = "Мощность на " + (hour + 1).ToString() + " час " + dtprDate.Value.ToShortDateString();

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
                            ws.Cells[0, 0].Value = tec.name;
                        else
                        {
                            ws.Cells[0, 0].Value = tec.name;
                            foreach (TECComponent g in tec.list_TECComponents)
                                ws.Cells[0, 0].Value += ", " + g.name;
                        }
                    }
                    else
                    {
                        ws.Cells[0, 0].Value = tec.name + ", " + tec.list_TECComponents[num_TECComponent].name;
                    }
                    
                    ws.Cells[1, 0].Value = "Мощность на " + dtprDate.Value.ToShortDateString();

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
            currValuesPeriod = parameters.poll_time - 1;
            started = true;

            tec.StartDbInterface();
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

            //TimerCallback timerCallbackCurrent = new TimerCallback(TimerCurrent_Tick);
            timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), null, 0, Timeout.Infinite);

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

        public void Stop()
        {
            if (!started)
                return;

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
                catch
                {
                }

                joined = taskThread.Join(1000);
                if (!joined)
                    taskThread.Abort();
            }

            tec.StopDbInterface();

            errored_state = false;
        }

        private void UpdateGUI(int hour, int min)
        {
            lock (lockValue)
            {
                FillGridHours();
                DrawGraphHours();

                FillGridMins(hour);
                DrawGraphMins(hour);

                ShowCommonPower();
                ShowTGPower();
            }
        }

        private void GetCurrentTimeRequest()
        {
            tec.Request("SELECT getdate()");
        }

        private void GetSensorsRequest()
        {
            string request = @"SELECT DISTINCT SENSORS.NAME, SENSORS.ID " +
                             @"FROM DEVICES " +
                             @"INNER JOIN SENSORS ON " +
                             @"DEVICES.ID = SENSORS.STATIONID " +
                             @"INNER JOIN DATA ON " +
                             @"DEVICES.CODE = DATA.OBJECT AND " +
                             @"SENSORS.CODE = DATA.ITEM " +
                             @"WHERE DATA.PARNUMBER = 12 AND " +
                             @"SENSORS.NAME LIKE 'ТГ%P%+'"; 

            tec.Request(request);
        }
        
        private void GetHoursRequest(DateTime date)
        {
            DateTime usingDate = date;
            string request;

            switch (tec.type ())
            {
                case TEC.TEC_TYPE.COMMON:
                    //request = @"SELECT DEVICES.NAME, DATA.OBJECT, SENSORS.NAME, DATA.ITEM, DATA.PARNUMBER, DATA.VALUE0, DATA.DATA_DATE, SENSORS.ID, DATA.SEASON " +
                    request = @"SELECT SENSORS.ID, DATA.DATA_DATE, DATA.SEASON, DATA.VALUE0 " + //, DEVICES.NAME, DATA.OBJECT, SENSORS.NAME, DATA.ITEM, DATA.PARNUMBER " +
                                @"FROM DEVICES " +
                                @"INNER JOIN SENSORS ON " +
                                @"DEVICES.ID = SENSORS.STATIONID " +
                                @"INNER JOIN DATA ON " +
                                @"DEVICES.CODE = DATA.OBJECT AND " +
                                @"SENSORS.CODE = DATA.ITEM AND " +
                                @"DATA.DATA_DATE > '" + usingDate.ToString("yyyy.MM.dd") +
                                @"' AND " +
                                @"DATA.DATA_DATE <= '" + usingDate.AddDays(1).ToString("yyyy.MM.dd") +
                                @"' " +
                                @"WHERE DATA.PARNUMBER = 12 AND (" + sensorsString +
                                @") " +
                                @"ORDER BY DATA.DATA_DATE, DATA.SEASON";
                    break;
                case TEC.TEC_TYPE.BIYSK:
                    //request = @"SELECT IZM_TII.IDCHANNEL, IZM_TII.PERIOD, DEVICES.NAME_DEVICE, CHANNELS.CHANNEL_NAME, IZM_TII.VALUE_UNIT, IZM_TII.TIME, IZM_TII.WINTER_SUMMER " +
                    request = @"SELECT IZM_TII.IDCHANNEL, IZM_TII.TIME, IZM_TII.WINTER_SUMMER, IZM_TII.VALUE_UNIT " + //, IZM_TII.PERIOD, DEVICES.NAME_DEVICE, CHANNELS.CHANNEL_NAME " +
                             @"FROM IZM_TII " +
                             @"INNER JOIN CHANNELS ON " +
                             @"IZM_TII.IDCHANNEL = CHANNELS.IDCHANNEL " +
                             @"INNER JOIN DEVICES ON " +
                             @"CHANNELS.IDDEVICE = DEVICES.IDDEVICE AND " +
                             @"IZM_TII.TIME > '" + usingDate.ToString("yyyyMMdd") +
                             @"' AND " +
                             @"IZM_TII.TIME <= '" + usingDate.AddDays(1).ToString("yyyyMMdd") +
                             @"' WHERE IZM_TII.PERIOD = 1800 AND " +
                             @"IZM_TII.IDCHANNEL IN(" + sensorsStrings[(int)TG.ID_TIME.HOURS] +
                             @") " +
                             //@"ORDER BY IZM_TII.TIME";
                             @"ORDER BY IZM_TII.TIME, IZM_TII.WINTER_SUMMER";
                    break;
                default:
                    request = string.Empty;
                    break;
            }

            tec.Request(request);
        }

        private void GetMinsRequest(int hour)
        {
            if (hour == 24)
                hour = 23;
            else
                ;

            DateTime usingDate = selectedTime.Date.AddHours(hour);
            string request = string.Empty;

            switch (tec.type())
            {
                case TEC.TEC_TYPE.COMMON:
                    //request = @"SELECT DEVICES.NAME, DATA.OBJECT, SENSORS.NAME, DATA.ITEM, DATA.PARNUMBER, DATA.VALUE0, DATA.DATA_DATE, SENSORS.ID, DATA.SEASON " +
                    request = @"SELECT SENSORS.ID, DATA.DATA_DATE, DATA.SEASON, DATA.VALUE0 " + //, DEVICES.NAME, DATA.OBJECT, SENSORS.NAME, DATA.ITEM, DATA.PARNUMBER " +
                             @"FROM DEVICES " +
                             @"INNER JOIN SENSORS ON " +
                             @"DEVICES.ID = SENSORS.STATIONID " +
                             @"INNER JOIN DATA ON " +
                             @"DEVICES.CODE = DATA.OBJECT AND " +
                             @"SENSORS.CODE = DATA.ITEM AND " +
                             @"DATA.DATA_DATE >= '" + usingDate.ToString("yyyy.MM.dd HH:00:00") +
                             @"' AND " +
                             @"DATA.DATA_DATE <= '" + usingDate.AddHours(1).ToString("yyyy.MM.dd HH:00:00") +
                             @"' " +
                             @"WHERE DATA.PARNUMBER = 2 AND (" + sensorsString +
                             @") " +
                             @"ORDER BY DATA.DATA_DATE, DATA.SEASON";
                    break;
                case TEC.TEC_TYPE.BIYSK:
                    //request = @"SELECT IZM_TII.IDCHANNEL, IZM_TII.PERIOD, DEVICES.NAME_DEVICE, CHANNELS.CHANNEL_NAME, IZM_TII.VALUE_UNIT, IZM_TII.TIME, IZM_TII.WINTER_SUMMER " +
                    request = @"SELECT IZM_TII.IDCHANNEL, IZM_TII.TIME, IZM_TII.WINTER_SUMMER, IZM_TII.VALUE_UNIT " + //, IZM_TII.PERIOD, DEVICES.NAME_DEVICE, CHANNELS.CHANNEL_NAME " +
                             @"FROM IZM_TII " +
                             @"INNER JOIN CHANNELS ON " +
                             @"IZM_TII.IDCHANNEL = CHANNELS.IDCHANNEL " +
                             @"INNER JOIN DEVICES ON " +
                             @"CHANNELS.IDDEVICE = DEVICES.IDDEVICE AND " +
                             @"IZM_TII.TIME >= '" + usingDate.ToString("yyyyMMdd HH:00:00") +
                             @"' AND " +
                             @"IZM_TII.TIME <= '" + usingDate.AddHours(1).ToString("yyyyMMdd HH:00:00") +
                             @"' WHERE IZM_TII.PERIOD = 180 AND " +
                             @"IZM_TII.IDCHANNEL IN(" + sensorsStrings[(int)TG.ID_TIME.MINUTES] +
                             @") " +
                             @"ORDER BY IZM_TII.TIME";
                    break;
                default:
                    request = string.Empty;
                    break;
            }

            tec.Request(request);
        }

        private void GetPBRValuesRequest () {
            admin.Request(tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.PBR], tec.m_arListenerIds[(int)CONN_SETT_TYPE.PBR], tec.GetPBRValueQuery(num_TECComponent, dtprDate.Value.Date, (ChangeMode.MODE_TECCOMPONENT) admin.mode()));
        }

        private void GetAdminValuesRequest () {
            admin.Request(tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], tec.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], tec.GetAdminValueQuery(num_TECComponent, dtprDate.Value.Date, (ChangeMode.MODE_TECCOMPONENT) admin.mode ()));
        }

        private void FillGridMins(int hour)
        {
            double sumFact = 0, sumUDGe = 0, sumDiviation = 0;
            int min = lastMin;
            if (min != 0)
                min--;
            for (int i = 0; i < valuesMins.valuesFact.Length - 1; i++)
            {
                dgwMins.Rows[i].Cells[1].Value = valuesMins.valuesFact[i + 1].ToString("F2");
                sumFact += valuesMins.valuesFact[i + 1];

                dgwMins.Rows[i].Cells[2].Value = valuesMins.valuesPBR[i].ToString("F2");
                dgwMins.Rows[i].Cells[3].Value = valuesMins.valuesPBRe[i].ToString("F2");
                dgwMins.Rows[i].Cells[4].Value = valuesMins.valuesUDGe[i].ToString("F2");
                sumUDGe += valuesMins.valuesUDGe[i];
                if (i < min && valuesMins.valuesUDGe[i] != 0)
                {
                    dgwMins.Rows[i].Cells[5].Value = ((double)(valuesMins.valuesFact[i + 1] - valuesMins.valuesUDGe[i])).ToString("F2");
                    //if (Math.Abs(valuesMins.valuesFact[i + 1] - valuesMins.valuesUDGe[i]) > valuesMins.valuesDiviation[i]
                    //    && valuesMins.valuesDiviation[i] != 0)
                    //    dgwMins.Rows[i].Cells[5].Style = dgvCellStyleError;
                    //else
                    dgwMins.Rows[i].Cells[5].Style = dgvCellStyleCommon;

                    sumDiviation += valuesMins.valuesFact[i + 1] - valuesMins.valuesUDGe[i];
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
                dgwMins.Rows[20].Cells[4].Value = valuesMins.valuesUDGe[0].ToString("F2");
                dgwMins.Rows[20].Cells[5].Value = (sumDiviation / min).ToString("F2");
            }
        }

        private void FillGridHours()
        {
            FillDefaultHours();

            double sumFact = 0, sumUDGe = 0, sumDiviation = 0;
            int hour = lastHour;
            int receivedHour = lastReceivedHour;
            int itemscount;

            if (valuesHours.season == seasonJumpE.SummerToWinter)
            {
                itemscount = 25;
            }
            else
                if (valuesHours.season == seasonJumpE.WinterToSummer)
                {
                    itemscount = 23;
                }
                else
                {
                    itemscount = 24;
                }

            for (int i = 0; i < itemscount; i++)
            {
                if (valuesHours.season == seasonJumpE.SummerToWinter)
                {
                    if (i <= valuesHours.hourAddon)
                    {
                        dgwHours.Rows[i].Cells[1].Value = valuesHours.valuesFact[i].ToString("F2");
                        sumFact += valuesHours.valuesFact[i];

                        dgwHours.Rows[i].Cells[2].Value = valuesHours.valuesPBR[i].ToString("F2");
                        dgwHours.Rows[i].Cells[3].Value = valuesHours.valuesPBRe[i].ToString("F2");
                        dgwHours.Rows[i].Cells[4].Value = valuesHours.valuesUDGe[i].ToString("F2");
                        sumUDGe += valuesHours.valuesUDGe[i];
                        if (i < receivedHour && valuesHours.valuesUDGe[i] != 0)
                        {
                            dgwHours.Rows[i].Cells[5].Value = ((double)(valuesHours.valuesFact[i] - valuesHours.valuesUDGe[i])).ToString("F2");
                            if (Math.Abs(valuesHours.valuesFact[i] - valuesHours.valuesUDGe[i]) > valuesHours.valuesDiviation[i]
                                && valuesHours.valuesDiviation[i] != 0)
                                dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                            else
                                dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            sumDiviation += Math.Abs(valuesHours.valuesFact[i] - valuesHours.valuesUDGe[i]);
                        }
                        else
                        {
                            dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                            dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                        }
                    }
                    else
                        if (i == valuesHours.hourAddon + 1)
                        {
                            dgwHours.Rows[i].Cells[1].Value = valuesHours.valuesFactAddon.ToString("F2");
                            sumFact += valuesHours.valuesFactAddon;

                            dgwHours.Rows[i].Cells[2].Value = valuesHours.valuesPBRAddon.ToString("F2");
                            dgwHours.Rows[i].Cells[3].Value = valuesHours.valuesPBReAddon.ToString("F2");
                            dgwHours.Rows[i].Cells[4].Value = valuesHours.valuesUDGeAddon.ToString("F2");
                            sumUDGe += valuesHours.valuesUDGeAddon;
                            if (i <= receivedHour && valuesHours.valuesUDGeAddon != 0)
                            {
                                dgwHours.Rows[i].Cells[5].Value = ((double)(valuesHours.valuesFactAddon - valuesHours.valuesUDGeAddon)).ToString("F2");
                                if (Math.Abs(valuesHours.valuesFactAddon - valuesHours.valuesUDGeAddon) > valuesHours.valuesDiviationAddon
                                    && valuesHours.valuesDiviationAddon != 0)
                                    dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                                else
                                    dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                                sumDiviation += Math.Abs(valuesHours.valuesFactAddon - valuesHours.valuesUDGeAddon);
                            }
                            else
                            {
                                dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                                dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            }
                        }
                        else
                        {
                            dgwHours.Rows[i].Cells[1].Value = valuesHours.valuesFact[i - 1].ToString("F2");
                            sumFact += valuesHours.valuesFact[i - 1];

                            dgwHours.Rows[i].Cells[2].Value = valuesHours.valuesPBR[i - 1].ToString("F2");
                            dgwHours.Rows[i].Cells[3].Value = valuesHours.valuesPBRe[i - 1].ToString("F2");
                            dgwHours.Rows[i].Cells[4].Value = valuesHours.valuesUDGe[i - 1].ToString("F2");
                            sumUDGe += valuesHours.valuesUDGe[i - 1];
                            if (i <= receivedHour && valuesHours.valuesUDGe[i - 1] != 0)
                            {
                                dgwHours.Rows[i].Cells[5].Value = ((double)(valuesHours.valuesFact[i - 1] - valuesHours.valuesUDGe[i - 1])).ToString("F2");
                                if (Math.Abs(valuesHours.valuesFact[i - 1] - valuesHours.valuesUDGe[i - 1]) > valuesHours.valuesDiviation[i - 1]
                                    && valuesHours.valuesDiviation[i - 1] != 0)
                                    dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                                else
                                    dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                                sumDiviation += Math.Abs(valuesHours.valuesFact[i - 1] - valuesHours.valuesUDGe[i - 1]);
                            }
                            else
                            {
                                dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                                dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            }
                        }

                }
                else
                    if (valuesHours.season == seasonJumpE.WinterToSummer)
                    {
                        if (i < valuesHours.hourAddon)
                        {
                            dgwHours.Rows[i].Cells[1].Value = valuesHours.valuesFact[i].ToString("F2");
                            sumFact += valuesHours.valuesFact[i];

                            dgwHours.Rows[i].Cells[2].Value = valuesHours.valuesPBR[i].ToString("F2");
                            dgwHours.Rows[i].Cells[3].Value = valuesHours.valuesPBRe[i].ToString("F2");
                            dgwHours.Rows[i].Cells[4].Value = valuesHours.valuesUDGe[i].ToString("F2");
                            sumUDGe += valuesHours.valuesUDGe[i];
                            if (i < receivedHour && valuesHours.valuesUDGe[i] != 0)
                            {
                                dgwHours.Rows[i].Cells[5].Value = ((double)(valuesHours.valuesFact[i] - valuesHours.valuesUDGe[i])).ToString("F2");
                                if (Math.Abs(valuesHours.valuesFact[i] - valuesHours.valuesUDGe[i]) > valuesHours.valuesDiviation[i]
                                    && valuesHours.valuesDiviation[i] != 0)
                                    dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                                else
                                    dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                                sumDiviation += Math.Abs(valuesHours.valuesFact[i] - valuesHours.valuesUDGe[i]);
                            }
                            else
                            {
                                dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                                dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            }
                        }
                        else
                        {
                            dgwHours.Rows[i].Cells[1].Value = valuesHours.valuesFact[i + 1].ToString("F2");
                            sumFact += valuesHours.valuesFact[i + 1];

                            dgwHours.Rows[i].Cells[2].Value = valuesHours.valuesPBR[i + 1].ToString("F2");
                            dgwHours.Rows[i].Cells[3].Value = valuesHours.valuesPBRe[i + 1].ToString("F2");
                            dgwHours.Rows[i].Cells[4].Value = valuesHours.valuesUDGe[i + 1].ToString("F2");
                            sumUDGe += valuesHours.valuesUDGe[i + 1];
                            if (i < receivedHour - 1 && valuesHours.valuesUDGe[i + 1] != 0)
                            {
                                dgwHours.Rows[i].Cells[5].Value = ((double)(valuesHours.valuesFact[i + 1] - valuesHours.valuesUDGe[i + 1])).ToString("F2");
                                if (Math.Abs(valuesHours.valuesFact[i + 1] - valuesHours.valuesUDGe[i + 1]) > valuesHours.valuesDiviation[i + 1]
                                    && valuesHours.valuesDiviation[i + 1] != 0)
                                    dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                                else
                                    dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                                sumDiviation += Math.Abs(valuesHours.valuesFact[i + 1] - valuesHours.valuesUDGe[i + 1]);
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
                        dgwHours.Rows[i].Cells[1].Value = valuesHours.valuesFact[i].ToString("F2");
                        sumFact += valuesHours.valuesFact[i];

                        dgwHours.Rows[i].Cells[2].Value = valuesHours.valuesPBR[i].ToString("F2");
                        dgwHours.Rows[i].Cells[3].Value = valuesHours.valuesPBRe[i].ToString("F2");
                        dgwHours.Rows[i].Cells[4].Value = valuesHours.valuesUDGe[i].ToString("F2");
                        sumUDGe += valuesHours.valuesUDGe[i];
                        if (i < receivedHour && valuesHours.valuesUDGe[i] != 0)
                        {
                            dgwHours.Rows[i].Cells[5].Value = ((double)(valuesHours.valuesFact[i] - valuesHours.valuesUDGe[i])).ToString("F2");
                            if (Math.Abs(valuesHours.valuesFact[i] - valuesHours.valuesUDGe[i]) > valuesHours.valuesDiviation[i]
                                && valuesHours.valuesDiviation[i] != 0)
                                dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                            else
                                dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            sumDiviation += Math.Abs(valuesHours.valuesFact[i] - valuesHours.valuesUDGe[i]);
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

        private void ShowTGPower()
        {
            int min = lastMin;
            if (min != 0)
                min--;

            int i = 0;
            if (num_TECComponent < 0) // значит этот view будет суммарным для всех ГТП
            {
                foreach (TECComponent g in tec.list_TECComponents)
                {
                    foreach (TG t in g.TG)
                    {
                        if (t.receivedMin[min])
                        {
                            tgsValue[i].Text = t.power[min].ToString("F2");
                            if (currHour)
                                tgsValue[i].ForeColor = System.Drawing.Color.LimeGreen;
                            else
                                tgsValue[i].ForeColor = System.Drawing.Color.OrangeRed;
                        }
                        else
                        {
                            tgsValue[i].Text = "---";
                            tgsValue[i].ForeColor = System.Drawing.Color.OrangeRed;
                        }
                        i++;
                    }
                }
            }
            else
            {
                foreach (TG t in tec.list_TECComponents[num_TECComponent].TG)
                {
                    if (t.receivedMin[min])
                    {
                        tgsValue[i].Text = t.power[min].ToString("F2");
                        if (currHour)
                            tgsValue[i].ForeColor = System.Drawing.Color.LimeGreen;
                        else
                            tgsValue[i].ForeColor = System.Drawing.Color.OrangeRed;
                    }
                    else
                    {
                        tgsValue[i].Text = "---";
                        tgsValue[i].ForeColor = System.Drawing.Color.OrangeRed;
                    }
                    i++;
                }
            }
        }

        private void ShowCommonPower()
        {
            int min = lastMin;
            if (min != 0)
                min--;
            double value = 0;
            for (int i = 0; i < sensorId2TG.Length; i++)
                value += sensorId2TG[i].power[min];

            lblCommonPVal.Text = value.ToString("F2");

            if (adminValuesReceived && currHour)
                lblPBRRecVal.Text = recomendation.ToString("F2");
            else
                lblPBRRecVal.Text = "---";

            double summ = 0;
            bool recalcAverOld = recalcAver;

            if (currHour && min == 0)
                recalcAver = false;

            if (recalcAver)
            {
                if (currHour)
                {
                    for (int i = 1; i < lastMin; i++)
                        summ += valuesMins.valuesFact[i];
                    if (min != 0)
                        lblAverPVal.Text = (summ / min).ToString("F2");
                    else
                        lblAverPVal.Text = 0.ToString("F2");
                }
                else
                {
                    int hour = lastHour;
                    if (hour == 24)
                        hour = 23;

                    if (valuesHours.addonValues && hour == valuesHours.hourAddon)
                        summ = valuesHours.valuesFactAddon;
                    else
                        summ = valuesHours.valuesFact[hour];

                    lblAverPVal.Text = summ.ToString("F2");
                }
            }

            if (currHour && min == 0)
                recalcAver = recalcAverOld;

            if (currHour)
            {
                if (lastHourError)
                {
                    ErrorReport("По текущему часу значений не найдено!");
                }
                else
                {
                    if (lastHourHalfError)
                    {
                        ErrorReport("За текущий час не получены некоторые получасовые значения!");
                    }
                    else
                    {
                        if (lastMinError)
                        {
                            ErrorReport("По текущему трёхминутному отрезку значений не найдено!");
                            lblAverPVal.ForeColor = System.Drawing.Color.OrangeRed;
                            lblCommonPVal.ForeColor = System.Drawing.Color.OrangeRed;
                        }
                        else
                        {
                            lblAverPVal.ForeColor = System.Drawing.Color.LimeGreen;
                            lblCommonPVal.ForeColor = System.Drawing.Color.LimeGreen;
                        }
                    }
                }
            }
            else
            {
                lblAverPVal.ForeColor = System.Drawing.Color.OrangeRed;
                lblCommonPVal.ForeColor = System.Drawing.Color.OrangeRed;
            }

            lblLayoutNumber.Text = lastLayout;
        }

        private void ClearValues()
        {
            ClearValuesMins();
            ClearValuesHours();
        }

        private void ClearValuesMins()
        {
            for (int i = 0; i < 21; i++)
                valuesMins.valuesFact[i] =
                valuesMins.valuesDiviation[i] =
                valuesMins.valuesPBR[i] =
                valuesMins.valuesPBRe[i] =
                valuesMins.valuesUDGe[i] = 0;
        }

        private void ClearValuesHours()
        {
            for (int i = 0; i < 24; i++)
                valuesHours.valuesFact[i] =
                valuesHours.valuesDiviation[i] =
                valuesHours.valuesPBR[i] =
                valuesHours.valuesPBRe[i] =
                valuesHours.valuesUDGe[i] = 0;
            
            valuesHours.valuesFactAddon =
            valuesHours.valuesDiviationAddon =
            valuesHours.valuesPBRAddon =
            valuesHours.valuesPBReAddon =
            valuesHours.valuesUDGeAddon = 0;
            valuesHours.season = seasonJumpE.None;
            valuesHours.hourAddon = 0;
            valuesHours.addonValues = false;
        }

        private void ClearPBRValues () {
        }

        private void ClearAdminValues()
        {
            for (int i = 0; i < 24; i++)
                valuesHours.valuesDiviation[i] =
                valuesHours.valuesPBR[i] =
                valuesHours.valuesPBRe[i] =
                valuesHours.valuesUDGe[i] = 0;

            valuesHours.valuesDiviationAddon =
            valuesHours.valuesPBRAddon =
            valuesHours.valuesPBReAddon =
            valuesHours.valuesUDGeAddon = 0;
            
            for (int i = 0; i < 21; i++)
                valuesMins.valuesDiviation[i] =
                valuesMins.valuesPBR[i] =
                valuesMins.valuesPBRe[i] =
                valuesMins.valuesUDGe[i] = 0;
        }

        private TG FindTGById(int id, int id_type)
        {
            for (int i = 0; i < sensorId2TG.Length; i++)
                if (sensorId2TG[i].ids [id_type] == id)
                    return sensorId2TG[i];

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
                catch
                {
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

        private bool GetSensorsResponse(DataTable table)
        {
            string s;
            int t = 0, pos;
            for (int i = 0; i < table.Rows.Count; i++)
            {
                // формирую правильное имя турбиногенератора
                s = table.Rows[i][0].ToString().ToUpper();
                s = s.Substring(2, s.IndexOf("P") - 2);

                pos = 0;
                while (pos < s.Length)
                {
                    if (s[pos] >= '0' && s[pos] <= '9')
                        break;
                    pos++;
                }
                if (pos >= s.Length)
                    continue;

                s = s.Substring(pos);

                pos = 0;
                while (pos < s.Length)
                {
                    if (s[pos] < '0' || s[pos] > '9')
                        break;
                    pos++;
                }

                s = s.Substring(0, pos);

                s = "ТГ" + s;

                if (num_TECComponent < 0)
                {
                    bool found = false;
                    int j, k;
                    for (j = 0; j < tec.list_TECComponents.Count && !found; j++)
                    {
                        for (k = 0; k < tec.list_TECComponents[j].TG.Count; k++)
                        {
                            if (tec.list_TECComponents[j].TG[k].name == s)
                            {
                                found = true;
                                tec.list_TECComponents[j].TG[k].ids[(int)TG.ID_TIME.MINUTES] =
                                tec.list_TECComponents[j].TG[k].ids[(int)TG.ID_TIME.HOURS] =
                                int.Parse(table.Rows[i][1].ToString());
                                sensorId2TG[t] = tec.list_TECComponents[j].TG[k];
                                t++;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    for (int k = 0; k < tec.list_TECComponents[num_TECComponent].TG.Count; k++)
                    {
                        if (tec.list_TECComponents[num_TECComponent].TG[k].name == s)
                        {
                            tec.list_TECComponents[num_TECComponent].TG[k].ids[(int)TG.ID_TIME.MINUTES] =
                            tec.list_TECComponents[num_TECComponent].TG[k].ids[(int)TG.ID_TIME.HOURS] =
                            int.Parse(table.Rows[i][1].ToString());
                            sensorId2TG[t] = tec.list_TECComponents[num_TECComponent].TG[k];
                            t++;
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < sensorId2TG.Length; i++)
            {
                if (sensorId2TG[i] != null)
                {
                    if (sensorsString == "")
                    {
                        sensorsString = "SENSORS.ID = " + sensorId2TG[i].ids [(int) TG.ID_TIME.MINUTES/*HOURS*/].ToString();
                    }
                    else
                    {
                        sensorsString += " OR SENSORS.ID = " + sensorId2TG[i].ids [(int) TG.ID_TIME.MINUTES/*HOURS*/].ToString();
                    }
                }
                else
                {
                    string error = "Ошибка определения идентификаторов датчиков в строке ";
                    for (int j = 0; j < table.Rows.Count; j++)
                        error += table.Rows[j][0].ToString() + " = " + table.Rows[j][1].ToString() + ", ";
                    error = error.Substring(0, error.LastIndexOf(","));
                    ErrorReport(error);
                    return false;
                }
            }
            return true;
        }

        private void GetSensors()
        {
            Dictionary<string, int>[] tgs = new Dictionary<string, int>[(int)TG.ID_TIME.COUNT_ID_TIME];
            tgs [(int) TG.ID_TIME.MINUTES] = new Dictionary<string, int>();
            tgs[(int)TG.ID_TIME.HOURS] = new Dictionary<string, int>();

            int count_tg = 0;
            for (int i = 0; i < tec.list_TECComponents.Count; i++) {
                count_tg += tec.list_TECComponents[i].TG.Count;
            }
            bool bMinutes = true;
            for (int i = (int) TG.ID_TIME.MINUTES; i < (int) TG.ID_TIME.COUNT_ID_TIME; i++)
            {
                if (i > (int)TG.ID_TIME.MINUTES) bMinutes = false; else ;
                for (int j = 0; j < count_tg; j++)
                {
                    tgs[i].Add("ТГ" + (j + 1).ToString(), tec.parametersTGForm.ParamsGetTgId(j, bMinutes));
                }
            }

            int t = 0;
            if (num_TECComponent < 0)
            {
                for (int i = 0; i < tec.list_TECComponents.Count; i++)
                {
                    for (int j = 0; j < tec.list_TECComponents[i].TG.Count; j++)
                    {
                        tec.list_TECComponents[i].TG[j].ids[(int)TG.ID_TIME.MINUTES] = tgs[(int) TG.ID_TIME.MINUTES][tec.list_TECComponents[i].TG[j].name];
                        tec.list_TECComponents[i].TG[j].ids[(int)TG.ID_TIME.HOURS] = tgs[(int)TG.ID_TIME.HOURS][tec.list_TECComponents[i].TG[j].name];
                        sensorId2TG[t] = tec.list_TECComponents[i].TG[j];
                        //sensorId2TGHours[t] = tec.list_TECComponents[i].TG[j];
                        t++;

                        if (sensorsStrings[(int)TG.ID_TIME.MINUTES] == "")
                            sensorsStrings[(int)TG.ID_TIME.MINUTES] = tec.list_TECComponents[i].TG[j].ids[(int)TG.ID_TIME.MINUTES].ToString();
                        else
                            sensorsStrings[(int)TG.ID_TIME.MINUTES] += ", " + tec.list_TECComponents[i].TG[j].ids[(int)TG.ID_TIME.MINUTES].ToString();

                        if (sensorsStrings[(int)TG.ID_TIME.HOURS] == "")
                            sensorsStrings[(int)TG.ID_TIME.HOURS] = tec.list_TECComponents[i].TG[j].ids[(int)TG.ID_TIME.HOURS].ToString();
                        else
                            sensorsStrings[(int)TG.ID_TIME.HOURS] += ", " + tec.list_TECComponents[i].TG[j].ids[(int)TG.ID_TIME.HOURS].ToString();
                    }
                }
            }
            else
            {
                for (int i = 0; i < tec.list_TECComponents[num_TECComponent].TG.Count; i++)
                {
                    tec.list_TECComponents[num_TECComponent].TG[i].ids[(int)TG.ID_TIME.MINUTES] = tgs[(int)TG.ID_TIME.MINUTES][tec.list_TECComponents[num_TECComponent].TG[i].name];
                    tec.list_TECComponents[num_TECComponent].TG[i].ids[(int)TG.ID_TIME.HOURS] = tgs[(int)TG.ID_TIME.HOURS][tec.list_TECComponents[num_TECComponent].TG[i].name];
                    sensorId2TG[t] = tec.list_TECComponents[num_TECComponent].TG[i];
                    //sensorId2TGHours[t] = tec.list_TECComponents[num_gtp].TG[i];
                    t++;

                    if (sensorsStrings[(int)TG.ID_TIME.MINUTES] == "")
                        sensorsStrings[(int)TG.ID_TIME.MINUTES] = tec.list_TECComponents[num_TECComponent].TG[i].ids[(int)TG.ID_TIME.MINUTES].ToString();
                    else
                        sensorsStrings[(int)TG.ID_TIME.MINUTES] += ", " + tec.list_TECComponents[num_TECComponent].TG[i].ids[(int)TG.ID_TIME.MINUTES].ToString();

                    if (sensorsStrings[(int)TG.ID_TIME.HOURS] == "")
                        sensorsStrings[(int)TG.ID_TIME.HOURS] = tec.list_TECComponents[num_TECComponent].TG[i].ids[(int)TG.ID_TIME.HOURS].ToString();
                    else
                        sensorsStrings[(int)TG.ID_TIME.HOURS] += ", " + tec.list_TECComponents[num_TECComponent].TG[i].ids[(int)TG.ID_TIME.HOURS].ToString();
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
        }
        
        private bool GetHoursResponse(DataTable table)
        {
            int i, j, half, hour = 0, halfAddon;
            double hourVal = 0, halfVal = 0, value, hourValAddon = 0;
            double[] oldValuesTG = new double[countTG];
            int[] oldIdTG = new int[countTG];
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
                /*f2.FillHourValues(lastHour, selectedTime, valuesHours.valuesFact);
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
                            valuesHours.hourAddon = hour; // уточнить
                        valuesHours.valuesFact[hour] = hourVal / 2000;
                        hour++;
                        half = 0;
                        hourVal = 0;
                    }
                    else
                    {
                        valuesHours.valuesFactAddon = hourValAddon / 2000;
                        valuesHours.hourAddon = hour - 1;
                        hourValAddon = 0;
                        prev_season = season;
                        halfAddon++;
                    }
                    lastHour = lastReceivedHour = hour;
                }

                halfVal = 0;

                jump_forward = false;
                jump_backward = false;

                for (j = 0; j < countTG; j++, i++)
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

                    tgTmp = FindTGById(id, (int) TG.ID_TIME.HOURS);

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
                        valuesHours.season = seasonJumpE.WinterToSummer;

                    if (!end)
                        half++;

                    hourVal += halfVal;
                }
                else
                {
                    valuesHours.season = seasonJumpE.SummerToWinter;
                    valuesHours.addonValues = true;

                    if (!end)
                        halfAddon++;

                    hourValAddon += halfVal;
                }
            }

            /*f2.FillHourValues(lastHour, selectedTime, valuesHours.valuesFact);
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
                /*f2.FillMinValues(lastMin, selectedTime, valuesMins.valuesFact);
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
                    valuesHours.addonValues = true;
                    valuesHours.hourAddon = lastHour - 1;
                    need_season = max_season;
                }
            }
            else
            {
                if (valuesHours.addonValues)
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
                    valuesMins.valuesFact[min] = 0;
                    minVal = 0;
                }

                /*MessageBox.Show("min " + min.ToString() + ", lastMin " + lastMin.ToString() + ", i " + i.ToString() +
                                 ", table.Rows.Count " + table.Rows.Count.ToString());*/
                jump = false;
                for (j = 0; j < countTG; j++, i++)
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

                    tgTmp = FindTGById(id, (int) TG.ID_TIME.MINUTES);

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
                        valuesMins.valuesFact[min] = minVal / 1000;
                        lastMin = min + 1;
                    }
                }
            }

            /*f2.FillMinValues(lastMin, selectedTime, valuesMins.valuesFact);
            f2.ShowDialog();*/

            if (lastMin <= ((selectedTime.Minute - 1) / 3))
            {
                lastMinError = true;
                lastMin = ((selectedTime.Minute - 1) / 3) + 1;
            }

            return true;
        }

        private bool LayoutIsBiggerByName(string l1, string l2)
        {
            int num1, num2;
            switch (l1)
            {
                case "ППБР": num1 = 0; break;
                default:
                    {
                        if (l1.Substring(0, 3) != "ПБР" || int.TryParse(l1.Substring(3), out num1) == false || num1 <= 0 || num1 > 24)
                            num1 = -1;
                        break;
                    }
            }

            switch (l2)
            {
                case "ППБР": num2 = 0; break;
                default:
                    {
                        if (l2.Substring(0, 3) != "ПБР" || int.TryParse(l2.Substring(3), out num2) == false || num2 <= 0 || num2 > 24)
                            num2 = -1;
                        break;
                    }
            }

            if (num2 > num1)
                return true;
            
            return false;
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

        private bool GetAdminValuesResponse(DataTable table_in)
        {
            //DataTable table = table_in.Copy();
            //table.Merge(m_tablePBRResponse, false);
            
            DateTime date = dtprDate.Value.Date;
            int hour;

            double currPBRe;
            int offsetPrev = -1,
            tableRowsCount = table_in.Rows.Count; //tableRowsCount = m_tablePBRResponse.Rows.Count
            int offsetUDG, offsetPlan, offsetLayout;

            lastLayout = "---";

            //switch (tec.type ()) {
            //    case TEC.TEC_TYPE.COMMON:
            //        offsetPrev = -1;

                    if (num_TECComponent < 0)
                    {
                        double[,] valuesPBR = new double[tec.list_TECComponents.Count, 25];
                        double[,] valuesREC = new double[tec.list_TECComponents.Count, 25];
                        int[,] valuesISPER = new int[tec.list_TECComponents.Count, 25];
                        double[,] valuesDIV = new double[tec.list_TECComponents.Count, 25];

                        offsetUDG = 1;
                        offsetPlan = /*offsetUDG + 3 * tec.list_TECComponents.Count +*/ 1;
                        offsetLayout = offsetPlan + tec.list_TECComponents.Count;

                        // поиск в таблице записи по предыдущим суткам (мало ли, вдруг нету)
                        for (int i = 0; i < tableRowsCount && offsetPrev < 0; i++)
                        {
                            if (!(m_tablePBRResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                            {
                                try
                                {
                                    hour = ((DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"]).Hour;
                                    if (hour == 0 && ((DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"]).Day == date.Day)
                                    {
                                        offsetPrev = i;
                                        int j = 0;
                                        foreach (TECComponent g in tec.list_TECComponents)
                                        {
                                            valuesPBR[j, 24] = (double)m_tablePBRResponse.Rows[i][offsetPlan + j];
                                            j++;
                                        }
                                    }
                                }
                                catch {
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
                                    else
                                        ;
                                }
                                catch
                                {
                                }
                            }
                        }

                        // разбор остальных значений
                        for (int i = 0; i < tableRowsCount; i++)
                        {
                            if (i == offsetPrev)
                                continue;

                            if (!(m_tablePBRResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                            {
                                try
                                {
                                    hour = ((DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"]).Hour;
                                    if (hour == 0 && ((DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"]).Day != date.Day)
                                        hour = 24;
                                    else
                                        if (hour == 0)
                                            continue;

                                    int j = 0;
                                    foreach (TECComponent g in tec.list_TECComponents)
                                    {
                                        try
                                        {
                                            if (!(m_tablePBRResponse.Rows[i][offsetPlan + j] is System.DBNull))
                                                valuesPBR[j, hour - 1] = (double)m_tablePBRResponse.Rows[i][offsetPlan + j];
                                            else
                                                ;

                                            //if (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull))
                                            if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull)))
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
                                        catch
                                        {
                                        }
                                        j++;
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

                                    int j = 0;
                                    foreach (TECComponent g in tec.list_TECComponents)
                                    {
                                        try
                                        {
                                            valuesPBR[j, hour - 1] = 0;
                                            //if (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull))
                                            if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull)))
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
                                        catch
                                        {
                                        }
                                        j++;
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

                        for (int i = 0; i < 24; i++)
                        {
                            for (int j = 0; j < tec.list_TECComponents.Count; j++)
                            {
                                valuesHours.valuesPBR[i] += valuesPBR[j, i];
                                if (i == 0)
                                {
                                    currPBRe = (valuesPBR[j, i] + valuesPBR[j, 24]) / 2;
                                    valuesHours.valuesPBRe[i] += currPBRe;
                                }
                                else
                                {
                                    currPBRe = (valuesPBR[j, i] + valuesPBR[j, i - 1]) / 2;
                                    valuesHours.valuesPBRe[i] += currPBRe;
                                }

                                valuesHours.valuesUDGe[i] += currPBRe + valuesREC[j, i];

                                if (valuesISPER[j, i] == 1)
                                    valuesHours.valuesDiviation[i] += (currPBRe + valuesREC[j, i]) * valuesDIV[j, i] / 100;
                                else
                                    valuesHours.valuesDiviation[i] += valuesDIV[j, i];
                            }
                            /*valuesHours.valuesPBR[i] = 0.20;
                            valuesHours.valuesPBRe[i] = 0.20;
                            valuesHours.valuesUDGe[i] = 0.20;
                            valuesHours.valuesDiviation[i] = 0.05;*/
                        }
                        if (valuesHours.season == seasonJumpE.SummerToWinter)
                        {
                            valuesHours.valuesPBRAddon = valuesHours.valuesPBR[valuesHours.hourAddon];
                            valuesHours.valuesPBReAddon = valuesHours.valuesPBRe[valuesHours.hourAddon];
                            valuesHours.valuesUDGeAddon = valuesHours.valuesUDGe[valuesHours.hourAddon];
                            valuesHours.valuesDiviationAddon = valuesHours.valuesDiviation[valuesHours.hourAddon];
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
                        offsetLayout = offsetPlan + 1;

                        // поиск в таблице записи по предыдущим суткам (мало ли, вдруг нету)
                        for (int i = 0; i < tableRowsCount && offsetPrev < 0; i++)
                        {
                            if (!(m_tablePBRResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                            {
                                try
                                {
                                    hour = ((DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"]).Hour;
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
                        for (int i = 0; i < tableRowsCount; i++)
                        {
                            if (i == offsetPrev)
                                continue;
                            else
                                ;

                            if (!(m_tablePBRResponse.Rows[i]["DATE_PBR"] is System.DBNull))
                            {
                                try
                                {
                                    hour = ((DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"]).Hour;
                                    if (hour == 0 && ((DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"]).Day != date.Day)
                                        hour = 24;
                                    else
                                        if (hour == 0)
                                            continue;
                                        else
                                            ;

                                    if (!(m_tablePBRResponse.Rows[i][offsetPlan] is System.DBNull))
                                        valuesPBR[hour - 1] = (double)m_tablePBRResponse.Rows[i][offsetPlan];
                                    else
                                        ;

                                    //if (!(table_in.Rows[i][offsetUDG] is System.DBNull))
                                    if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG] is System.DBNull)))
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
                                    //if (!(table_in.Rows[i][offsetUDG + 0] is System.DBNull))
                                    if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + 0] is System.DBNull)))
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

                        for (int i = 0; i < 24; i++)
                        {
                            valuesHours.valuesPBR[i] = valuesPBR[i];
                            if (i == 0)
                            {
                                currPBRe = (valuesPBR[i] + valuesPBR[24]) / 2;
                                valuesHours.valuesPBRe[i] = currPBRe;
                            }
                            else
                            {
                                currPBRe = (valuesPBR[i] + valuesPBR[i - 1]) / 2;
                                valuesHours.valuesPBRe[i] = currPBRe;
                            }

                            valuesHours.valuesUDGe[i] = currPBRe + valuesREC[i];

                            if (valuesISPER[i] == 1)
                                valuesHours.valuesDiviation[i] = (currPBRe + valuesREC[i]) * valuesDIV[i] / 100;
                            else
                                valuesHours.valuesDiviation[i] = valuesDIV[i];
                        }

                        if (valuesHours.season == seasonJumpE.SummerToWinter)
                        {
                            valuesHours.valuesPBRAddon = valuesHours.valuesPBR[valuesHours.hourAddon];
                            valuesHours.valuesPBReAddon = valuesHours.valuesPBRe[valuesHours.hourAddon];
                            valuesHours.valuesUDGeAddon = valuesHours.valuesUDGe[valuesHours.hourAddon];
                            valuesHours.valuesDiviationAddon = valuesHours.valuesDiviation[valuesHours.hourAddon];
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
            //                    /*valuesHours.valuesPBR[i] += valuesPBR[j, i];
            //                    if (i == 0)
            //                    {
            //                        currPBRe = (valuesPBR[j, i] + valuesPBR[j, 24]) / 2;
            //                        valuesHours.valuesPBRe[i] += currPBRe;
            //                    }
            //                    else
            //                    {
            //                        currPBRe = (valuesPBR[j, i] + valuesPBR[j, i - 1]) / 2;
            //                        valuesHours.valuesPBRe[i] += currPBRe;
            //                    }*/
            //                    currPBRe = 0;

            //                    valuesHours.valuesUDGe[i] += currPBRe + valuesREC[j, i];

            //                    if (valuesISPER[j, i] == 1)
            //                        valuesHours.valuesDiviation[i] += (currPBRe + valuesREC[j, i]) * valuesDIV[j, i] / 100;
            //                    else
            //                        valuesHours.valuesDiviation[i] += valuesDIV[j, i];
            //                }
            //                /*valuesHours.valuesPBR[i] = 0.20;
            //                valuesHours.valuesPBRe[i] = 0.20;
            //                valuesHours.valuesUDGe[i] = 0.20;
            //                valuesHours.valuesDiviation[i] = 0.05;*/
            //            }
            //            if (valuesHours.season == seasonJumpE.SummerToWinter)
            //            {
            //                valuesHours.valuesPBRAddon = valuesHours.valuesPBR[valuesHours.hourAddon];
            //                valuesHours.valuesPBReAddon = valuesHours.valuesPBRe[valuesHours.hourAddon];
            //                valuesHours.valuesUDGeAddon = valuesHours.valuesUDGe[valuesHours.hourAddon];
            //                valuesHours.valuesDiviationAddon = valuesHours.valuesDiviation[valuesHours.hourAddon];
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
            //                /*valuesHours.valuesPBR[i] = valuesPBR[i];
            //                if (i == 0)
            //                {
            //                    currPBRe = (valuesPBR[i] + valuesPBR[24]) / 2;
            //                    valuesHours.valuesPBRe[i] = currPBRe;
            //                }
            //                else
            //                {
            //                    currPBRe = (valuesPBR[i] + valuesPBR[i - 1]) / 2;
            //                    valuesHours.valuesPBRe[i] = currPBRe;
            //                }*/
            //                currPBRe = 0;

            //                valuesHours.valuesUDGe[i] = currPBRe + valuesREC[i];

            //                if (valuesISPER[i] == 1)
            //                    valuesHours.valuesDiviation[i] = (currPBRe + valuesREC[i]) * valuesDIV[i] / 100;
            //                else
            //                    valuesHours.valuesDiviation[i] = valuesDIV[i];
            //            }

            //            if (valuesHours.season == seasonJumpE.SummerToWinter)
            //            {
            //                valuesHours.valuesPBRAddon = valuesHours.valuesPBR[valuesHours.hourAddon];
            //                valuesHours.valuesPBReAddon = valuesHours.valuesPBRe[valuesHours.hourAddon];
            //                valuesHours.valuesUDGeAddon = valuesHours.valuesUDGe[valuesHours.hourAddon];
            //                valuesHours.valuesDiviationAddon = valuesHours.valuesDiviation[valuesHours.hourAddon];
            //            }
            //        }
            //        break;
            //    default:
            //        break;
            //}
            
            hour = lastHour;
            if (hour == 24)
                hour = 23;

            for (int i = 0; i < 21; i++)
            {
                valuesMins.valuesPBR[i] = valuesHours.valuesPBR[hour];
                valuesMins.valuesPBRe[i] = valuesHours.valuesPBRe[hour];
                valuesMins.valuesUDGe[i] = valuesHours.valuesUDGe[hour];
                valuesMins.valuesDiviation[i] = valuesHours.valuesDiviation[hour];
            }

            return true;
        }
        
        private void ComputeRecomendation(int hour)
        {
            if (hour == 24)
                hour = 23;

            if (valuesHours.valuesUDGe[hour] == 0)
            {
                recomendation = 0;
                return;
            }

            if (!currHour)
            {
                recomendation = valuesHours.valuesUDGe[hour];
                return;
            }

            if (lastMin < 2)
            {
                recomendation = valuesHours.valuesUDGe[hour];
                return;
            }

            double factSum = 0;
            for (int i = 1; i < lastMin; i++)
                factSum += valuesMins.valuesFact[i];

            if (lastMin == 21)
                recomendation = 0;
            else
                recomendation = (valuesHours.valuesUDGe[hour] * 20 - factSum) / (20 - (lastMin - 1));

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
                if (dtprDate.Value.Date != selectedTime.Date)
                    currHour = false;
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
                dtprDate.Value = selectedTime;
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

            if (sensorsString != "")
            {
                if (currHour)
                {
                    states.Add(StatesMachine.CurrentTime);
                }
                else
                {
                    selectedTime = dtprDate.Value.Date;
                }
            }
            else
            {
                states.Add(StatesMachine.Init);
                states.Add(StatesMachine.CurrentTime);
            }

            states.Add(StatesMachine.CurrentHours);
            states.Add(StatesMachine.CurrentMins);
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
                    errored_state = actioned_state = false;
                }
            }
        }

        private void ErrorReport(string error_string)
        {
            last_error = error_string;
            last_time_error = DateTime.Now;
            errored_state = true;
            stsStrip.BeginInvoke(delegateEventUpdate);
        }

        private void ActionReport(string action_string)
        {
            last_action = action_string;
            last_time_action = DateTime.Now;
            actioned_state = true;
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
            //resValues[7] = sensorId2TG[indx_tg].ids[indx_id_time];

            resValues[0] = sensorId2TG[indx_tg].ids[indx_id_time];
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
                    for (int j = 0; j < countTG; j++)
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
                        for (int j = 0; j < countTG; j++)
                        {
                            table.Rows.Add(generateValues(date, j, i, (int)TG.ID_TIME.HOURS, -1));
                        }
                        if (i == 4 || i == 5)
                        {
                            for (int j = 0; j < countTG; j++)
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
                            for (int j = 0; j < countTG; j++)
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
                    for (int j = 0; j < countTG; j++)
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
                        for (int j = 0; j < countTG; j++)
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
                        for (int j = 0; j < countTG; j++)
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
                        for (int j = 0; j < countTG; j++)
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
                case StatesMachine.Init:
                    ActionReport("Получение идентификаторов датчиков.");
                    switch (tec.type ()) {
                        case TEC.TEC_TYPE.COMMON:
                            GetSensorsRequest();
                            break;
                        case TEC.TEC_TYPE.BIYSK:
                            GetSensors ();
                            break;
                        default:
                            break;
                    }
                    break;
                case StatesMachine.CurrentTime:
                    ActionReport("Получение текущего времени сервера.");
                    GetCurrentTimeRequest();
                    break;
                case StatesMachine.CurrentHours:
                    ActionReport("Получение получасовых значений.");
                    adminValuesReceived = false;
                    GetHoursRequest(selectedTime.Date);
                    break;
                case StatesMachine.CurrentMins:
                    ActionReport("Получение трёхминутных значений.");
                    adminValuesReceived = false;
                    GetMinsRequest(lastHour);
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
                            GetAdminValuesRequest();
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
                case StatesMachine.Init:
                    switch (tec.type ()) {
                        case TEC.TEC_TYPE.COMMON:
                            return tec.GetResponse(out error, out table);
                        case TEC.TEC_TYPE.BIYSK:
                            return true;
                    }
                    break;
                case StatesMachine.CurrentTime:
                case StatesMachine.CurrentHours:
                case StatesMachine.CurrentMins:
                case StatesMachine.RetroHours:
                case StatesMachine.RetroMins:
                    return tec.GetResponse(out error, out table);
                case StatesMachine.PBRValues:
                    return admin.GetResponse(tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.PBR], tec.m_arListenerIds[(int)CONN_SETT_TYPE.PBR], out error, out table);
                    //return true; //Имитация получения данных плана
                case StatesMachine.AdminValues:
                    //return admin.GetResponse(out error, out table, true);
                    return admin.GetResponse(tec.m_arIndxDbInterfaces[(int)CONN_SETT_TYPE.ADMIN], tec.m_arListenerIds[(int)CONN_SETT_TYPE.ADMIN], out error, out table);
            }

            error = true;

            return false;
        }

        private bool StateResponse(StatesMachine state, DataTable table)
        {
            bool result = false;
            switch (state)
            {
                case StatesMachine.Init:
                    switch (tec.type ()) {
                        case TEC.TEC_TYPE.COMMON:
                            result = GetSensorsResponse(table);
                            break;
                        case TEC.TEC_TYPE.BIYSK:
                            result = true;
                            break;
                    }
                    if (result)
                    {
                    }
                    break;
                case StatesMachine.CurrentTime:
                    result = GetCurrentTimeReponse(table);
                    if (result)
                    {
                        //this.BeginInvoke(delegateShowValues, "StatesMachine.CurrentTime");
                        selectedTime = selectedTime.AddSeconds(-parameters.error_delay);
                        this.BeginInvoke(delegateSetNowDate, true);
                    }
                    break;
                case StatesMachine.CurrentHours:
                    ClearValues();
                    //GenerateHoursTable(seasonJumpE.SummerToWinter, 3, table);
                    result = GetHoursResponse(table);
                    if (result)
                    {
                        //this.BeginInvoke(delegateShowValues, "StatesMachine.CurrentHours");
                    }
                    break;
                case StatesMachine.CurrentMins:
                    //GenerateMinsTable(seasonJumpE.None, 5, table);
                    result = GetMinsResponse(table);
                    if (result)
                    {
                        //this.BeginInvoke(delegateShowValues, "StatesMachine.CurrentMins");
                        this.BeginInvoke(delegateUpdateGUI, lastHour, lastMin);
                    }
                    break;
                case StatesMachine.RetroHours:
                    ClearValues();
                    result = GetHoursResponse(table);
                    if (result)
                    {
                        //this.BeginInvoke(delegateShowValues, "StatesMachine.RetroHours");
                    }
                    break;
                case StatesMachine.RetroMins:
                    result = GetMinsResponse(table);
                    if (result)
                    {
                        //this.BeginInvoke(delegateShowValues, "StatesMachine.RetroMins");
                        this.BeginInvoke(delegateUpdateGUI, lastHour, lastMin);
                    }
                    break;
                case StatesMachine.PBRValues:
                    ClearPBRValues();
                    result = GetPBRValuesResponse(table);
                    if (result)
                    {
                    }
                    else
                        ;
                    break;
                case StatesMachine.AdminValues:
                    ClearAdminValues();
                    result = GetAdminValuesResponse(table);
                    if (result)
                    {
                        //this.BeginInvoke(delegateShowValues, "StatesMachine.AdminValues");
                        ComputeRecomendation(lastHour);
                        adminValuesReceived = true;
                        this.BeginInvoke(delegateUpdateGUI, lastHour, lastMin);
                    }
                    else
                        ;
                    break;
            }

            if (result)
                errored_state = actioned_state = false;

            return result;
        }

        private void StateErrors(StatesMachine state, bool response)
        {
            switch (state)
            {
                case StatesMachine.Init:
                    if (response)
                        ErrorReport("Ошибка разбора идентификаторов датчиков. Переход в ожидание.");
                    else
                        ErrorReport("Ошибка получения идентификаторов датчиков. Переход в ожидание.");
                    break;
                case StatesMachine.CurrentTime:
                    if (response)
                        ErrorReport("Ошибка разбора текущего времени сервера. Ожидание " + parameters.poll_time.ToString() + " секунд.");
                    else
                        ErrorReport("Ошибка получения текущего времени сервера. Ожидание " + parameters.poll_time.ToString() + " секунд.");
                    break;
                case StatesMachine.CurrentHours:
                    if (response)
                        ErrorReport("Ошибка разбора получасовых значений. Ожидание " + parameters.poll_time.ToString() + " секунд.");
                    else
                        ErrorReport("Ошибка получения получасовых значений. Ожидание " + parameters.poll_time.ToString() + " секунд.");
                    break;
                case StatesMachine.CurrentMins:
                    if (response)
                        ErrorReport("Ошибка разбора трёхминутных значений. Ожидание " + parameters.poll_time.ToString() + " секунд.");
                    else
                        ErrorReport("Ошибка получения трёхминутных значений. Ожидание " + parameters.poll_time.ToString() + " секунд.");
                    break;
                case StatesMachine.RetroHours:
                    if (response)
                        ErrorReport("Ошибка разбора получасовых значений. Переход в ожидание.");
                    else
                        ErrorReport("Ошибка получения получасовых значений. Переход в ожидание.");
                    break;
                case StatesMachine.RetroMins:
                    if (response)
                        ErrorReport("Ошибка разбора трёхминутных значений. Переход в ожидание.");
                    else
                        ErrorReport("Ошибка получения трёхминутных значений. Переход в ожидание.");
                    break;
                case StatesMachine.PBRValues:
                    if (response)
                        ErrorReport("Ошибка разбора данных плана.");
                    else
                        ErrorReport("Ошибка получения данных плана.");
                    break;
                case StatesMachine.AdminValues:
                    if (response)
                        ErrorReport("Ошибка разбора административных данных.");
                    else
                        ErrorReport("Ошибка получения административных данных.");
                    break;
            }
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
                    currentState = states[index];
                    newState = false;
                }

                while (true)
                {
                    bool error = true;
                    bool dataPresent = false;
                    DataTable table = null;
                    for (int i = 0; i < MainForm.MAX_RETRY && !dataPresent && !newState; i++)
                    {
                        if (error)
                            StateRequest(currentState);

                        error = false;
                        for (int j = 0; j < MainForm.MAX_WAIT_COUNT && !dataPresent && !error && !newState; j++)
                        {
                            System.Threading.Thread.Sleep(MainForm.WAIT_TIME_MS);
                            dataPresent = StateCheckResponse(currentState, out error, out table);
                        }
                    }

                    bool responseIsOk = true;
                    if (dataPresent && !error && !newState)
                        responseIsOk = StateResponse(currentState, table);

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
                        }
                    }

                    index++;

                    lock (lockValue)
                    {
                        if (index == states.Count)
                            break;
                        if (newState)
                            break;
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
            lblServerTime.Text = serverTime.ToString("HH:mm:ss");
        }

        private void TimerCurrent_Tick(Object stateInfo)
        {
            Invoke(delegateTickTime);
            if (currHour && isActive) {
                if (((currValuesPeriod++) * 1000) >= parameters.poll_time) {
                    currValuesPeriod = 0;
                    NewDateRefresh();
                }
            }
            try {
                timerCurrent.Change(1000, Timeout.Infinite);
            }
            catch (Exception e) {
                MainForm.log.LogLock();
                MainForm.log.LogToFile("Исключение обращения к переменной (timerCurrent)", true, true, false);
                MainForm.log.LogToFile("Имя ТЭЦ: " + tec.name, false, false, false);
                MainForm.log.LogToFile("Исключение " + e.Message, false, false, false);
                MainForm.log.LogToFile(e.ToString(), false, false, false);
                MainForm.log.LogUnlock();
            }
        }
    }
}
