using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Data;
//using System.Data.SqlClient;
using System.Drawing;
using System.Threading;
using System.Globalization;

using ZedGraph;
using GemBox.Spreadsheet;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    public class Hd2PercentControl
    {
        //private double m_valuesBaseCalculate;

        //public double valuesBaseCalculate { get { return m_valuesBaseCalculate; } }
        //public double difference { get { return m_valuesBaseCalculate; } }

        public Hd2PercentControl() { }

        public string Calculate(TecView.values values, bool bPmin, out int err)
        {
            string strRes = string.Empty;
            double valuesBaseCalculate = -1F;

            double dblRel = 0.0
                    , delta = -1.0
                    , dbl2AbsPercentControl = -1.0
                    ;
            int iReverse = 0;
            bool bAbs = false;

            if (values.valuesPBR == values.valuesPmax)
            {
                valuesBaseCalculate = values.valuesPBR;
                iReverse = 1;
            }
            else
            {
                //Вычисление "ВК"
                //if (values.valuesUDGe[hour] == values.valuesPBR[hour])
                //if (!(values.valuesREC[hour] == 0))
                if (values.valuesREC == 0)
                    values.valuesForeignCommand = false;
                else
                    ;

                if (values.valuesForeignCommand == true)
                {
                    valuesBaseCalculate = values.valuesUDGe;
                    iReverse = 1;
                    bAbs = true;
                }
                else
                {
                    if (bPmin == true)
                        if (values.valuesPBR == values.valuesPmin)
                        {
                            valuesBaseCalculate = values.valuesPBR;
                            iReverse = -1;
                        }
                        else
                        {
                        }
                    else
                        ;
                }
            }

            if (valuesBaseCalculate > 1) {
                strRes += @"Уров=" + valuesBaseCalculate.ToString(@"F2");
                strRes += @"; ПБР=" + values.valuesPBR.ToString(@"F2") + @"; Pmax=" + values.valuesPmax.ToString(@"F2");
                if (bPmin == true) {
                    strRes += @"; Pmin=" + values.valuesPmin.ToString(@"F2");
                } else ;

                if (values.valuesLastMinutesTM > 1)
                {
                    if (!(iReverse == 0))
                    {
                        delta = iReverse * (valuesBaseCalculate - values.valuesLastMinutesTM);
                        if (bAbs == true)
                            delta = Math.Abs(delta);
                        else
                            ;
                    }
                    else
                        ;

                    dbl2AbsPercentControl = valuesBaseCalculate / 100 * 2;

                    if (dbl2AbsPercentControl < 1)
                        dbl2AbsPercentControl = 1;
                    else
                        ;

                    if (valuesBaseCalculate > 1)
                        dblRel = delta - dbl2AbsPercentControl;
                    else
                        ;

                    if ((dblRel > 0) && (!(iReverse == 0)))
                        err = 1;
                    else
                        err = 0;

                    strRes += @"; Откл=" + (dbl2AbsPercentControl + dblRel).ToString(@"F1") + @"(" + (((dbl2AbsPercentControl + dblRel) / valuesBaseCalculate) * 100).ToString(@"F1") + @"%)";
                }
                else {
                    err = 0;

                    strRes += @";Откл=" + 0.ToString(@"F1") + @"(" + 0.ToString(@"F1") + @"%)";
                }
            }
            else {
                err = 0;

                strRes += @"Уров=---.-";
                strRes += @"; ПБР=" + values.valuesPBR.ToString(@"F2") + @"; Pmax=" + values.valuesPmax.ToString(@"F2");
                if (bPmin == true)
                {
                    strRes += @"; Pmin=" + values.valuesPmin.ToString(@"F2");
                }
                else ;

                strRes += @"; Откл=--(--%)";
            }           

            return strRes;
        }

        public static string StringToolTipEmpty = @"Уров=---.-; Откл=--(--%)";
    }

    public abstract class PanelTecViewBase : PanelStatisticView
    {
        protected static AdminTS.TYPE_FIELDS s_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;

        protected abstract class HZedGraphControl : ZedGraph.ZedGraphControl
        {
            // контекстные меню
            private class HContextMenuStripZedGraph : System.Windows.Forms.ContextMenuStrip
            {

                public System.Windows.Forms.ToolStripMenuItem показыватьЗначенияToolStripMenuItem;
                public System.Windows.Forms.ToolStripMenuItem копироватьToolStripMenuItem;
                public System.Windows.Forms.ToolStripMenuItem параметрыПечатиToolStripMenuItem;
                public System.Windows.Forms.ToolStripMenuItem распечататьToolStripMenuItem;
                public System.Windows.Forms.ToolStripMenuItem сохранитьToolStripMenuItem;
                public System.Windows.Forms.ToolStripMenuItem эксельToolStripMenuItem;
                public System.Windows.Forms.ToolStripMenuItem источникАСКУЭToolStripMenuItem;
                public System.Windows.Forms.ToolStripMenuItem источникСОТИАССОToolStripMenuItem;

                public HContextMenuStripZedGraph()
                {
                    InitializeComponent();
                }

                private void InitializeComponent()
                {
                    this.показыватьЗначенияToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                    this.копироватьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                    this.сохранитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                    this.эксельToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                    this.параметрыПечатиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                    this.распечататьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                    this.источникАСКУЭToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                    this.источникСОТИАССОToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

                    // 
                    // contextMenuStrip
                    // 
                    this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    this.показыватьЗначенияToolStripMenuItem,
                    new System.Windows.Forms.ToolStripSeparator(),
                    this.копироватьToolStripMenuItem,
                    this.сохранитьToolStripMenuItem,
                    this.эксельToolStripMenuItem,
                    new System.Windows.Forms.ToolStripSeparator(),
                    this.параметрыПечатиToolStripMenuItem,
                    this.распечататьToolStripMenuItem
                    , new System.Windows.Forms.ToolStripSeparator()
                    , источникАСКУЭToolStripMenuItem
                    , источникСОТИАССОToolStripMenuItem});
                    this.Name = "contextMenuStripMins";
                    this.Size = new System.Drawing.Size(198, 148);
                    // 
                    // показыватьЗначенияToolStripMenuItemMins
                    // 
                    this.показыватьЗначенияToolStripMenuItem.Name = "показыватьЗначенияToolStripMenuItem";
                    this.показыватьЗначенияToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
                    this.показыватьЗначенияToolStripMenuItem.Text = "Показывать значения";
                    this.показыватьЗначенияToolStripMenuItem.Checked = true;

                    //// 
                    //// toolStripSeparator1Mins
                    //// 
                    //this.toolStripSeparator1.Name = "toolStripSeparator1Mins";
                    //this.toolStripSeparator1.Size = new System.Drawing.Size(194, 6);
                    // 
                    // копироватьToolStripMenuItemMins
                    // 
                    this.копироватьToolStripMenuItem.Name = "копироватьToolStripMenuItem";
                    this.копироватьToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
                    this.копироватьToolStripMenuItem.Text = "Копировать";

                    // 
                    // сохранитьToolStripMenuItemMins
                    // 
                    this.сохранитьToolStripMenuItem.Name = "сохранитьToolStripMenuItem";
                    this.сохранитьToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
                    this.сохранитьToolStripMenuItem.Text = "Сохранить график";

                    // 
                    // эксельToolStripMenuItemMins
                    // 
                    this.эксельToolStripMenuItem.Name = "эксельToolStripMenuItem";
                    this.эксельToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
                    this.эксельToolStripMenuItem.Text = "Сохранить в MS Excel";

                    //// 
                    //// toolStripSeparator2Mins
                    //// 
                    //this.toolStripSeparator2.Name = "toolStripSeparator2";
                    //this.toolStripSeparator2.Size = new System.Drawing.Size(194, 6);
                    // 
                    // параметрыПечатиToolStripMenuItemMins
                    // 
                    this.параметрыПечатиToolStripMenuItem.Name = "параметрыПечатиToolStripMenuItem";
                    this.параметрыПечатиToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
                    this.параметрыПечатиToolStripMenuItem.Text = "Параметры печати";
                    // 
                    // распечататьToolStripMenuItemMins
                    // 
                    this.распечататьToolStripMenuItem.Name = "распечататьToolStripMenuItem";
                    this.распечататьToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
                    this.распечататьToolStripMenuItem.Text = "Распечатать";

                    // 
                    // источникАСКУЭToolStripMenuItem
                    // 
                    this.источникАСКУЭToolStripMenuItem.Name = "источникАСКУЭToolStripMenuItem";
                    this.источникАСКУЭToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
                    this.источникАСКУЭToolStripMenuItem.Text = "Источник: БД АСКУЭ (3 мин)";
                    this.источникАСКУЭToolStripMenuItem.Checked = true;
                    this.источникАСКУЭToolStripMenuItem.Enabled = false;
                    // 
                    // источникСОТИАССОToolStripMenuItem
                    // 
                    this.источникСОТИАССОToolStripMenuItem.Name = "источникСОТИАССОToolStripMenuItem";
                    this.источникСОТИАССОToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
                    this.источникСОТИАССОToolStripMenuItem.Text = "Источник: БД СОТИАССО (1 мин)";
                    this.источникСОТИАССОToolStripMenuItem.Checked = false;
                    this.источникСОТИАССОToolStripMenuItem.Enabled = false;
                }
            }

            private object m_lockValue;

            public HZedGraphControl(object lockVal)
            {
                InitializeComponent();

                m_lockValue = lockVal;
            }

            private void InitializeComponent()
            {
                this.ContextMenuStrip = new HContextMenuStripZedGraph();

                // 
                // zedGraphMin
                // 
                this.Dock = System.Windows.Forms.DockStyle.Fill;
                //this.Location = arPlacement[(int)CONTROLS.zedGraphMins].pt;
                this.Name = "zedGraph";
                this.ScrollGrace = 0;
                this.ScrollMaxX = 0;
                this.ScrollMaxY = 0;
                this.ScrollMaxY2 = 0;
                this.ScrollMinX = 0;
                this.ScrollMinY = 0;
                this.ScrollMinY2 = 0;
                //this.Size = arPlacement[(int)CONTROLS.zedGraphMins].sz;
                this.TabIndex = 0;
                this.IsEnableHEdit = false;
                this.IsEnableHPan = false;
                this.IsEnableHZoom = false;
                this.IsEnableSelection = false;
                this.IsEnableVEdit = false;
                this.IsEnableVPan = false;
                this.IsEnableVZoom = false;
                this.IsShowPointValues = true;

                ((HContextMenuStripZedGraph)this.ContextMenuStrip).показыватьЗначенияToolStripMenuItem.Click += new System.EventHandler(показыватьЗначенияToolStripMenuItem_Click);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).копироватьToolStripMenuItem.Click += new System.EventHandler(копироватьToolStripMenuItem_Click);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).сохранитьToolStripMenuItem.Click += new System.EventHandler(сохранитьToolStripMenuItem_Click);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).параметрыПечатиToolStripMenuItem.Click += new System.EventHandler(параметрыПечатиToolStripMenuItem_Click);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).распечататьToolStripMenuItem.Click += new System.EventHandler(распечататьToolStripMenuItem_Click);

                this.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.OnPointValueEvent);
                this.DoubleClickEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.OnDoubleClickEvent);
            }

            public void InitializeEventHandler(EventHandler fToExcel, EventHandler fSourceData)
            {
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).эксельToolStripMenuItem.Click += new System.EventHandler(fToExcel);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).источникАСКУЭToolStripMenuItem.Click += new System.EventHandler(fSourceData);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).источникСОТИАССОToolStripMenuItem.Click += new System.EventHandler(fSourceData);
            }

            private void показыватьЗначенияToolStripMenuItem_Click(object sender, EventArgs e)
            {
                ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
                this.IsShowPointValues = ((ToolStripMenuItem)sender).Checked;
            }

            private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
            {
                lock (m_lockValue)
                {
                    this.Copy(false);
                }
            }

            private void параметрыПечатиToolStripMenuItem_Click(object sender, EventArgs e)
            {
                PageSetupDialog pageSetupDialog = new PageSetupDialog();
                pageSetupDialog.Document = this.PrintDocument;
                pageSetupDialog.ShowDialog();
            }

            private void распечататьToolStripMenuItem_Click(object sender, EventArgs e)
            {
                lock (m_lockValue)
                {
                    this.PrintDocument.Print();
                }
            }

            private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
            {
                lock (m_lockValue)
                {
                    this.SaveAs();
                }
            }

            private string OnPointValueEvent(object sender, GraphPane pane, CurveItem curve, int iPt)
            {
                return curve[iPt].Y.ToString("f2");
            }

            private bool OnDoubleClickEvent(ZedGraphControl sender, MouseEventArgs e)
            {
                FormMain.formGraphicsSettings.SetScale();

                return true;
            }
        }

        protected class HZedGraphControlHours : HZedGraphControl
        {
            public HZedGraphControlHours(object obj) : base(obj) { }
        }

        protected class HZedGraphControlMins : HZedGraphControl
        {
            public HZedGraphControlMins(object obj) : base(obj) { InitializeComponent(); }

            private void InitializeComponent()
            {
                this.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XScaleFormatEvent);
            }

            public string XScaleFormatEvent(GraphPane pane, Axis axis, double val, int index)
            {
                return ((val) * 3).ToString();
            }
        }

        protected PanelQuickData m_pnlQuickData;

        protected System.Windows.Forms.SplitContainer stctrView;
        protected System.Windows.Forms.SplitContainer stctrViewPanel1, stctrViewPanel2;
        protected HZedGraphControl m_ZedGraphMins;
        protected HZedGraphControl m_ZedGraphHours;

        protected DataGridViewHours m_dgwHours;
        protected DataGridViewMins m_dgwMins;

        private DataGridViewCellStyle dgvCellStyleError;
        private DataGridViewCellStyle dgvCellStyleCommon;

        //protected DelegateFunc delegateSetNowDate;

        //private DelegateIntIntFunc delegateUpdateGUI_Fact;
        //private DelegateFunc delegateUpdateGUI_TM;

        //protected object m_lockValue;

        //private Thread taskThread;
        //protected Semaphore m_sem;
        //private volatile bool threadIsWorking;
        //protected volatile bool m_newState;
        //protected volatile List<StatesMachine> m_states;
        //private int currValuesPeriod = 0;
        private ManualResetEvent m_evTimerCurrent;
        private System.Threading.Timer m_timerCurrent;
        //private System.Windows.Forms.Timer timerCurrent;
        private DelegateObjectFunc delegateTickTime;

        //private AdminTS m_admin;
        //protected FormGraphicsSettings graphSettings;
        //protected FormParameters parameters;

        public volatile bool isActive;

        public TecView m_tecView;

        int currValuesPeriod;

        public int indx_TEC { get { return m_tecView.m_indx_TEC; } }
        public int indx_TECComponent { get { return m_tecView.indxTECComponents; } }

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        //public TG[] sensorId2TG;        

        //public volatile TEC tec;

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        //public List<TECComponentBase> m_list_TECComponents;

        private bool update;

        protected virtual void InitializeComponent()
        {
            //this.m_pnlQuickData = new PanelQuickData(); Выполнено в конструкторе

            this.m_dgwHours = new DataGridViewHours();
            this.m_dgwMins = new DataGridViewMins();

            ((System.ComponentModel.ISupportInitialize)(this.m_dgwHours)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwMins)).BeginInit();

            this.m_pnlQuickData.RestructControl();
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.Location = arPlacement[(int)CONTROLS.THIS].pt;
            this.Name = "pnlTecView";
            //this.Size = arPlacement[(int)CONTROLS.THIS].sz;
            this.TabIndex = 0;

            this.m_pnlQuickData.Dock = DockStyle.Fill;
            this.m_pnlQuickData.btnSetNow.Click += new System.EventHandler(this.btnSetNow_Click);
            this.m_pnlQuickData.dtprDate.ValueChanged += new System.EventHandler(this.dtprDate_ValueChanged);

            ((System.ComponentModel.ISupportInitialize)(this.m_dgwHours)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwMins)).EndInit();

            this.m_ZedGraphMins = new HZedGraphControlMins(m_tecView.m_lockValue);
            this.m_ZedGraphHours = new HZedGraphControlHours(m_tecView.m_lockValue);

            this.stctrViewPanel1 = new System.Windows.Forms.SplitContainer();
            this.stctrViewPanel2 = new System.Windows.Forms.SplitContainer();

            this.stctrView = new System.Windows.Forms.SplitContainer();
            //this.stctrView.IsSplitterFixed = true;

            this.m_pnlQuickData.SuspendLayout();

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
            //this.stctrViewPanel1.SplitterDistance = 301;
            this.stctrViewPanel1.SplitterMoved += new SplitterEventHandler(stctrViewPanel1_SplitterMoved);
            // 
            // stctrView.Panel2
            // 
            this.stctrViewPanel2.Dock = DockStyle.Fill;
            //this.stctrViewPanel2.SplitterDistance = 291;
            this.stctrViewPanel2.SplitterMoved += new SplitterEventHandler(stctrViewPanel2_SplitterMoved);
            //this.stctrView.Size = arPlacement[(int)CONTROLS.stctrView].sz;
            //this.stctrView.SplitterDistance = 301;
            this.stctrView.TabIndex = 7;

            this.m_pnlQuickData.ResumeLayout(false);
            this.m_pnlQuickData.PerformLayout();

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

        public PanelTecViewBase(TEC tec, int indx_tec, int indx_comp, DelegateFunc fErrRep, DelegateFunc fActRep)
        {
            //InitializeComponent();

            m_tecView = new TecView(null, TecView.TYPE_PANEL.VIEW, indx_tec, indx_comp);

            HMark markQueries = new HMark();
            markQueries.Marked((int)CONN_SETT_TYPE.ADMIN);
            markQueries.Marked((int)CONN_SETT_TYPE.PBR);
            markQueries.Marked((int)CONN_SETT_TYPE.DATA_ASKUE);
            markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO);

            m_tecView.InitTEC(new List<StatisticCommon.TEC>() { tec }, markQueries);
            m_tecView.SetDelegateReport(fErrRep, fActRep);

            m_tecView.setDatetimeView = new DelegateFunc(setNowDate);

            m_tecView.updateGUI_Fact = new DelegateIntIntFunc(updateGUI_Fact);
            m_tecView.updateGUI_TM_Gen = new DelegateFunc(updateGUI_TM_Gen);

            this.m_pnlQuickData = new PanelQuickData(); //Предвосхищая вызов 'InitializeComponent'
            if (m_tecView.listTG == null) //m_tecView.m_tec.m_bSensorsStrings == false
                m_tecView.m_tec.InitSensorsTEC();
            else
                ;

            foreach (TG tg in m_tecView.listTG)
            {
                m_pnlQuickData.addTGView(ref tg.name_shr);
            }

            dgvCellStyleError = new DataGridViewCellStyle();
            dgvCellStyleError.BackColor = Color.Red;
            dgvCellStyleCommon = new DataGridViewCellStyle();

            if (tec.type() == TEC.TEC_TYPE.BIYSK)
                ; //this.parameters = FormMain.papar;
            else
                ;

            isActive = false;

            update = false;

            delegateTickTime = new DelegateObjectFunc(TickTime);
        }

        private void FillDefaultMins()
        {
            for (int i = 0; i < 20; i++)
            {
                this.m_dgwMins.Rows[i].Cells[0].Value = ((i + 1) * 3).ToString();
                this.m_dgwMins.Rows[i].Cells[1].Value = 0.ToString("F2");
                this.m_dgwMins.Rows[i].Cells[2].Value = 0.ToString("F2");
                this.m_dgwMins.Rows[i].Cells[3].Value = 0.ToString("F2");
                this.m_dgwMins.Rows[i].Cells[4].Value = 0.ToString("F2");
                this.m_dgwMins.Rows[i].Cells[5].Value = 0.ToString("F2");
            }
            this.m_dgwMins.Rows[20].Cells[0].Value = "Итог";
            this.m_dgwMins.Rows[20].Cells[1].Value = 0.ToString("F2");
            this.m_dgwMins.Rows[20].Cells[2].Value = "-";
            this.m_dgwMins.Rows[20].Cells[3].Value = "-";
            this.m_dgwMins.Rows[20].Cells[4].Value = 0.ToString("F2");
            this.m_dgwMins.Rows[20].Cells[5].Value = 0.ToString("F2");
        }

        private void FillDefaultHours()
        {
            int count
                , hour;

            this.m_dgwHours.Rows.Clear();

            count = m_tecView.m_valuesHours.Length;

            this.m_dgwHours.Rows.Add(count + 1);

            int offset = 0;
            bool bSeasobDate = false;
            if (m_tecView.m_curDate.Date.CompareTo (HAdmin.SeasonDateTime.Date) == 0)
                bSeasobDate = true;
            else
                ;                

            for (int i = 0; i < count; i++)
            {
                hour = i + 1;                
                if (bSeasobDate == true) {
                    offset = m_tecView.GetSeasonHourOffset (hour);
                    
                    this.m_dgwHours.Rows[i].Cells[0].Value = (hour - offset).ToString();
                    if ((hour - 1) == HAdmin.SeasonDateTime.Hour)
                        this.m_dgwHours.Rows[i].Cells[0].Value += @"*";
                    else
                        ;
                }
                else
                    this.m_dgwHours.Rows[i].Cells[0].Value = (hour).ToString();

                this.m_dgwHours.Rows[i].Cells[1].Value = 0.ToString("F2");
                this.m_dgwHours.Rows[i].Cells[2].Value = 0.ToString("F2");
                this.m_dgwHours.Rows[i].Cells[3].Value = 0.ToString("F2");
                this.m_dgwHours.Rows[i].Cells[4].Value = 0.ToString("F2");
                this.m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                this.m_dgwHours.Rows[i].Cells[6].Value = 0.ToString("F2");
            }

            this.m_dgwHours.Rows[count].Cells[0].Value = "Сумма";
            this.m_dgwHours.Rows[count].Cells[1].Value = 0.ToString("F2");
            this.m_dgwHours.Rows[count].Cells[2].Value = "-";
            this.m_dgwHours.Rows[count].Cells[3].Value = "-";
            this.m_dgwHours.Rows[count].Cells[4].Value = 0.ToString("F2");
            this.m_dgwHours.Rows[count].Cells[5].Value = 0.ToString("F2");
            this.m_dgwHours.Rows[count].Cells[6].Value = 0.ToString("F2");
        }

        public override void Start()
        {
            FillDefaultMins();
            FillDefaultHours();

            DaylightTime daylight = TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year);
            int timezone_offset = m_tecView.m_tec.m_timezone_offset_msc;
            if (TimeZone.IsDaylightSavingTime(DateTime.Now, daylight))
                timezone_offset++;
            else
                ;

            //Время д.б. МСК ???
            m_pnlQuickData.dtprDate.Value = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(timezone_offset);

            initTableHourRows ();

            //В зависимости от установленных признаков в контекстном меню
            // , расположение пунктов меню постоянно: 1-ый, 2-ой снизу
            // , если установлен один, то обязательно снят другой
            setTypeSourceData(TG.ID_TIME.MINUTES, ((ToolStripMenuItem)m_ZedGraphMins.ContextMenuStrip.Items[m_ZedGraphMins.ContextMenuStrip.Items.Count - 2]).Checked == true ? CONN_SETT_TYPE.DATA_ASKUE : CONN_SETT_TYPE.DATA_SOTIASSO);
            setTypeSourceData(TG.ID_TIME.HOURS, ((ToolStripMenuItem)m_ZedGraphHours.ContextMenuStrip.Items[m_ZedGraphHours.ContextMenuStrip.Items.Count - 2]).Checked == true ? CONN_SETT_TYPE.DATA_ASKUE : CONN_SETT_TYPE.DATA_SOTIASSO);

            m_tecView.Start();

            m_evTimerCurrent = new ManualResetEvent(true);
            //timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), evTimerCurrent, 0, Timeout.Infinite);
            m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, 0, 1000);

            //timerCurrent = new System.Windows.Forms.Timer ();
            //timerCurrent.Tick += TimerCurrent_Tick;

            update = false;
            SetNowDate(true);

            DrawGraphMins(0);
            DrawGraphHours();
        }

        public override void Stop()
        {
            m_tecView.Stop ();

            if (! (m_evTimerCurrent == null)) m_evTimerCurrent.Reset(); else ;
            if (!(m_timerCurrent == null)) { m_timerCurrent.Dispose(); m_timerCurrent = null; } else ;

            FormMainBaseWithStatusStrip.m_report.ClearStates ();
        }

        protected override void initTableHourRows()
        {
            m_tecView.m_curDate = m_pnlQuickData.dtprDate.Value.Date;
            m_tecView.serverTime = m_tecView.m_curDate;

            if (m_tecView.m_curDate.Date.Equals(HAdmin.SeasonDateTime.Date) == false)
            {
                m_dgwHours.InitRows(24, false);                
            }
            else
            {
                m_dgwHours.InitRows(25, true);
            }
        }

        private void updateGUI_TM_Gen()
        {
            if (InvokeRequired == true)
                this.BeginInvoke(new DelegateFunc(UpdateGUI_TM_Gen));
            else
                Logging.Logg().Error(@"PanelTecViewBase::updateGUI_TM_Gen () - ... BeginInvoke (UpdateGUI_TM_Gen) - ...");
        }

        private void UpdateGUI_TM_Gen()
        {
            lock (m_tecView.m_lockValue)
            {
                m_pnlQuickData.ShowTMValues();
            }
        }

        private void updateGUI_Fact(int hour, int min)
        {
            if (InvokeRequired == true)
                this.BeginInvoke(new DelegateIntIntFunc(UpdateGUI_Fact), hour, min);
            else
                Logging.Logg().Error(@"PanelTecViewBase::updateGUI_Fact () - ... BeginInvoke (UpdateGUI_Fact) - ...");
        }

        protected virtual void UpdateGUI_Fact(int hour, int min)
        {
            lock (m_tecView.m_lockValue)
            {
                try
                {
                    FillGridHours();

                    FillGridMins(hour);

                    m_pnlQuickData.ShowFactValues();

                    DrawGraphMins(hour);
                    DrawGraphHours();
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelTecViewBase::UpdateGUI_Fact () - ...");
                }
            }
        }

        private void setFirstDisplayedScrollingRowIndex(DataGridView dgv, int lastIndx)
        {
            int iFirstDisplayedScrollingRowIndex = -1;

            if (lastIndx > dgv.DisplayedRowCount(true))
            {
                iFirstDisplayedScrollingRowIndex = lastIndx - dgv.DisplayedRowCount(true);
            }
            else
                iFirstDisplayedScrollingRowIndex = 0;

            dgv.FirstDisplayedScrollingRowIndex = iFirstDisplayedScrollingRowIndex;
        }

        private void FillGridMins(int hour)
        {
            double sumFact = 0, sumUDGe = 0, sumDiviation = 0;
            int min = m_tecView.lastMin;

            if (min != 0)
                min--;
            else
                ;

            for (int i = 0; i < m_tecView.m_valuesMins.Length - 1; i++)
            {
                m_dgwMins.Rows[i].Cells[1].Value = m_tecView.m_valuesMins[i + 1].valuesFact.ToString("F2");
                sumFact += m_tecView.m_valuesMins[i + 1].valuesFact;

                m_dgwMins.Rows[i].Cells[2].Value = m_tecView.m_valuesMins[i].valuesPBR.ToString("F2");
                m_dgwMins.Rows[i].Cells[3].Value = m_tecView.m_valuesMins[i].valuesPBRe.ToString("F2");
                m_dgwMins.Rows[i].Cells[4].Value = m_tecView.m_valuesMins[i].valuesUDGe.ToString("F2");
                sumUDGe += m_tecView.m_valuesMins[i].valuesUDGe;
                if (i < min && m_tecView.m_valuesMins[i].valuesUDGe != 0)
                {
                    m_dgwMins.Rows[i].Cells[5].Value = ((double)(m_tecView.m_valuesMins[i + 1].valuesFact - m_tecView.m_valuesMins[i].valuesUDGe)).ToString("F2");
                    //if (Math.Abs(m_tecView.m_valuesMins.valuesFact[i + 1] - m_tecView.m_valuesMins.valuesUDGe[i]) > m_tecView.m_valuesMins.valuesDiviation[i]
                    //    && m_tecView.m_valuesMins.valuesDiviation[i] != 0)
                    //    m_dgwMins.Rows[i].Cells[5].Style = dgvCellStyleError;
                    //else
                    m_dgwMins.Rows[i].Cells[5].Style = dgvCellStyleCommon;

                    sumDiviation += m_tecView.m_valuesMins[i + 1].valuesFact - m_tecView.m_valuesMins[i].valuesUDGe;
                }
                else
                {
                    m_dgwMins.Rows[i].Cells[5].Value = 0.ToString("F2");
                    m_dgwMins.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                }
            }
            if (min <= 0)
            {
                m_dgwMins.Rows[20].Cells[1].Value = 0.ToString("F2");
                m_dgwMins.Rows[20].Cells[4].Value = 0.ToString("F2");
                m_dgwMins.Rows[20].Cells[5].Value = 0.ToString("F2");
            }
            else
            {
                if (min > 20)
                    min = 20;
                else
                    ;

                m_dgwMins.Rows[20].Cells[1].Value = (sumFact / min).ToString("F2");
                m_dgwMins.Rows[20].Cells[4].Value = m_tecView.m_valuesMins[0].valuesUDGe.ToString("F2");
                m_dgwMins.Rows[20].Cells[5].Value = (sumDiviation / min).ToString("F2");
            }

            setFirstDisplayedScrollingRowIndex(m_dgwMins, m_tecView.lastMin);

            //Logging.Logg().Debug(@"PanelTecViewBase::FillGridMins () - ...");
        }

        private void FillGridHours()
        {
            FillDefaultHours();

            double sumFact = 0, sumUDGe = 0, sumDiviation = 0;
            Hd2PercentControl d2PercentControl = new Hd2PercentControl();
            int hour = m_tecView.lastHour;
            int receivedHour = m_tecView.lastReceivedHour;
            int itemscount = m_tecView.m_valuesHours.Length;
            int warn = -1,
                cntWarn = -1;
            string strWarn = string.Empty;

            DataGridViewCellStyle curCellStyle;
            cntWarn = 0;
            for (int i = 0; i < itemscount; i++)
            {
                bool bPmin = false;
                if (m_tecView.m_tec.m_id == 5) bPmin = true; else ;
                d2PercentControl.Calculate(m_tecView.m_valuesHours[i], bPmin, out warn);

                if ((!(warn == 0)) &&
                   (m_tecView.m_valuesHours[i].valuesLastMinutesTM > 1))
                    cntWarn++;
                else
                    cntWarn = 0;

                if (! (cntWarn == 0))
                    curCellStyle = dgvCellStyleError;
                else
                    curCellStyle = dgvCellStyleCommon;
                m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.LAST_MINUTES].Style = curCellStyle;

                if (m_tecView.m_valuesHours[i].valuesLastMinutesTM > 1)
                {
                    if (cntWarn > 0)
                        strWarn = cntWarn + @":";
                    else
                        strWarn = string.Empty;

                    m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.LAST_MINUTES].Value = strWarn + m_tecView.m_valuesHours[i].valuesLastMinutesTM.ToString("F2");
                }
                else
                    m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.LAST_MINUTES].Value = 0.ToString("F2");

                //if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.SummerToWinter)
                //{
                    //if (i <= m_tecView.m_valuesHours.hourAddon)
                    //{
                        m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.FACT].Value = m_tecView.m_valuesHours[i].valuesFact.ToString("F2");
                        sumFact += m_tecView.m_valuesHours[i].valuesFact;

                        m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.PBR].Value = m_tecView.m_valuesHours[i].valuesPBR.ToString("F2");
                        m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.PBRe].Value = m_tecView.m_valuesHours[i].valuesPBRe.ToString("F2");
                        m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.UDGe].Value = m_tecView.m_valuesHours[i].valuesUDGe.ToString("F2");
                        sumUDGe += m_tecView.m_valuesHours[i].valuesUDGe;
                        if ((i < (receivedHour + 1)) && ((!(m_tecView.m_valuesHours[i].valuesUDGe == 0)) && (m_tecView.m_valuesHours[i].valuesFact > 0)))
                        {
                            m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.DEVIATION].Value = ((double)(m_tecView.m_valuesHours[i].valuesFact - m_tecView.m_valuesHours[i].valuesUDGe)).ToString("F2");
                            if (Math.Abs(m_tecView.m_valuesHours[i].valuesFact - m_tecView.m_valuesHours[i].valuesUDGe) > m_tecView.m_valuesHours[i].valuesDiviation
                                && (! (m_tecView.m_valuesHours[i].valuesDiviation == 0)))
                                m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.DEVIATION].Style = dgvCellStyleError;
                            else
                                m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.DEVIATION].Style = dgvCellStyleCommon;
                            sumDiviation += Math.Abs(m_tecView.m_valuesHours[i].valuesFact - m_tecView.m_valuesHours[i].valuesUDGe);
                        }
                        else
                        {
                            m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.DEVIATION].Value = 0.ToString("F2");
                            m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.DEVIATION].Style = dgvCellStyleCommon;
                        }
                    //}
                    //else
                        //if (i == m_tecView.m_valuesHours.hourAddon + 1)
                        //{
                        //    m_dgwHours.Rows[i].Cells[1].Value = m_tecView.m_valuesHours.valuesFactAddon.ToString("F2");
                        //    sumFact += m_tecView.m_valuesHours.valuesFactAddon;

                        //    m_dgwHours.Rows[i].Cells[2].Value = m_tecView.m_valuesHours.valuesPBRAddon.ToString("F2");
                        //    m_dgwHours.Rows[i].Cells[3].Value = m_tecView.m_valuesHours.valuesPBReAddon.ToString("F2");
                        //    m_dgwHours.Rows[i].Cells[4].Value = m_tecView.m_valuesHours.valuesUDGeAddon.ToString("F2");
                        //    sumUDGe += m_tecView.m_valuesHours.valuesUDGeAddon;
                        //    if (i <= receivedHour && m_tecView.m_valuesHours.valuesUDGeAddon != 0)
                        //    {
                        //        m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_tecView.m_valuesHours.valuesFactAddon - m_tecView.m_valuesHours.valuesUDGeAddon)).ToString("F2");
                        //        if (Math.Abs(m_tecView.m_valuesHours.valuesFactAddon - m_tecView.m_valuesHours.valuesUDGeAddon) > m_tecView.m_valuesHours.valuesDiviationAddon
                        //            && m_tecView.m_valuesHours.valuesDiviationAddon != 0)
                        //            m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                        //        else
                        //            m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                        //        sumDiviation += Math.Abs(m_tecView.m_valuesHours.valuesFactAddon - m_tecView.m_valuesHours.valuesUDGeAddon);
                        //    }
                        //    else
                        //    {
                        //        m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                        //        m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                        //    }
                        //}
                        //else
                        //{
                        //    m_dgwHours.Rows[i].Cells[1].Value = m_tecView.m_valuesHours.valuesFact[i - 1].ToString("F2");
                        //    sumFact += m_tecView.m_valuesHours.valuesFact[i - 1];

                        //    m_dgwHours.Rows[i].Cells[2].Value = m_tecView.m_valuesHours.valuesPBR[i - 1].ToString("F2");
                        //    m_dgwHours.Rows[i].Cells[3].Value = m_tecView.m_valuesHours.valuesPBRe[i - 1].ToString("F2");
                        //    m_dgwHours.Rows[i].Cells[4].Value = m_tecView.m_valuesHours.valuesUDGe[i - 1].ToString("F2");
                        //    sumUDGe += m_tecView.m_valuesHours.valuesUDGe[i - 1];
                        //    if (i <= receivedHour && m_tecView.m_valuesHours.valuesUDGe[i - 1] != 0)
                        //    {
                        //        m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_tecView.m_valuesHours.valuesFact[i - 1] - m_tecView.m_valuesHours.valuesUDGe[i - 1])).ToString("F2");
                        //        if (Math.Abs(m_tecView.m_valuesHours.valuesFact[i - 1] - m_tecView.m_valuesHours.valuesUDGe[i - 1]) > m_tecView.m_valuesHours.valuesDiviation[i - 1]
                        //            && m_tecView.m_valuesHours.valuesDiviation[i - 1] != 0)
                        //            m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                        //        else
                        //            m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                        //        sumDiviation += Math.Abs(m_tecView.m_valuesHours.valuesFact[i - 1] - m_tecView.m_valuesHours.valuesUDGe[i - 1]);
                        //    }
                        //    else
                        //    {
                        //        m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                        //        m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                        //    }
                        //}

                //}
                //else
                //    if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.WinterToSummer)
                //    {
                //        if (i < m_tecView.m_valuesHours.hourAddon)
                //        {
                //            m_dgwHours.Rows[i].Cells[1].Value = m_tecView.m_valuesHours.valuesFact[i].ToString("F2");
                //            sumFact += m_tecView.m_valuesHours.valuesFact[i];

                //            m_dgwHours.Rows[i].Cells[2].Value = m_tecView.m_valuesHours.valuesPBR[i].ToString("F2");
                //            m_dgwHours.Rows[i].Cells[3].Value = m_tecView.m_valuesHours.valuesPBRe[i].ToString("F2");
                //            m_dgwHours.Rows[i].Cells[4].Value = m_tecView.m_valuesHours.valuesUDGe[i].ToString("F2");
                //            sumUDGe += m_tecView.m_valuesHours.valuesUDGe[i];
                //            if (i < receivedHour && m_tecView.m_valuesHours.valuesUDGe[i] != 0)
                //            {
                //                m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_tecView.m_valuesHours.valuesFact[i] - m_tecView.m_valuesHours.valuesUDGe[i])).ToString("F2");
                //                if (Math.Abs(m_tecView.m_valuesHours.valuesFact[i] - m_tecView.m_valuesHours.valuesUDGe[i]) > m_tecView.m_valuesHours.valuesDiviation[i]
                //                    && m_tecView.m_valuesHours.valuesDiviation[i] != 0)
                //                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                //                else
                //                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                //                sumDiviation += Math.Abs(m_tecView.m_valuesHours.valuesFact[i] - m_tecView.m_valuesHours.valuesUDGe[i]);
                //            }
                //            else
                //            {
                //                m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                //                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                //            }
                //        }
                //        else
                //        {
                //            m_dgwHours.Rows[i].Cells[1].Value = m_tecView.m_valuesHours.valuesFact[i + 1].ToString("F2");
                //            sumFact += m_tecView.m_valuesHours.valuesFact[i + 1];

                //            m_dgwHours.Rows[i].Cells[2].Value = m_tecView.m_valuesHours.valuesPBR[i + 1].ToString("F2");
                //            m_dgwHours.Rows[i].Cells[3].Value = m_tecView.m_valuesHours.valuesPBRe[i + 1].ToString("F2");
                //            m_dgwHours.Rows[i].Cells[4].Value = m_tecView.m_valuesHours.valuesUDGe[i + 1].ToString("F2");
                //            sumUDGe += m_tecView.m_valuesHours.valuesUDGe[i + 1];
                //            if (i < receivedHour - 1 && m_tecView.m_valuesHours.valuesUDGe[i + 1] != 0)
                //            {
                //                m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_tecView.m_valuesHours.valuesFact[i + 1] - m_tecView.m_valuesHours.valuesUDGe[i + 1])).ToString("F2");
                //                if (Math.Abs(m_tecView.m_valuesHours.valuesFact[i + 1] - m_tecView.m_valuesHours.valuesUDGe[i + 1]) > m_tecView.m_valuesHours.valuesDiviation[i + 1]
                //                    && m_tecView.m_valuesHours.valuesDiviation[i + 1] != 0)
                //                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                //                else
                //                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                //                sumDiviation += Math.Abs(m_tecView.m_valuesHours.valuesFact[i + 1] - m_tecView.m_valuesHours.valuesUDGe[i + 1]);
                //            }
                //            else
                //            {
                //                m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                //                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                //            }
                //        }
                //    }
                //    else
                //    {
                //        m_dgwHours.Rows[i].Cells[1].Value = m_tecView.m_valuesHours.valuesFact[i].ToString("F2");
                //        sumFact += m_tecView.m_valuesHours.valuesFact[i];

                //        m_dgwHours.Rows[i].Cells[2].Value = m_tecView.m_valuesHours.valuesPBR[i].ToString("F2");
                //        m_dgwHours.Rows[i].Cells[3].Value = m_tecView.m_valuesHours.valuesPBRe[i].ToString("F2");
                //        m_dgwHours.Rows[i].Cells[4].Value = m_tecView.m_valuesHours.valuesUDGe[i].ToString("F2");
                //        sumUDGe += m_tecView.m_valuesHours.valuesUDGe[i];
                //        if (i < receivedHour && m_tecView.m_valuesHours.valuesUDGe[i] != 0)
                //        {
                //            m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_tecView.m_valuesHours.valuesFact[i] - m_tecView.m_valuesHours.valuesUDGe[i])).ToString("F2");
                //            if (Math.Abs(m_tecView.m_valuesHours.valuesFact[i] - m_tecView.m_valuesHours.valuesUDGe[i]) > m_tecView.m_valuesHours.valuesDiviation[i]
                //                && m_tecView.m_valuesHours.valuesDiviation[i] != 0)
                //                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                //            else
                //                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                //            sumDiviation += Math.Abs(m_tecView.m_valuesHours.valuesFact[i] - m_tecView.m_valuesHours.valuesUDGe[i]);
                //        }
                //        else
                //        {
                //            m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                //            m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                //        }
                //    }
            }
            m_dgwHours.Rows[itemscount].Cells[(int)DataGridViewTables.INDEX_COLUMNS.FACT].Value = sumFact.ToString("F2");
            m_dgwHours.Rows[itemscount].Cells[(int)DataGridViewTables.INDEX_COLUMNS.UDGe].Value = sumUDGe.ToString("F2");
            m_dgwHours.Rows[itemscount].Cells[(int)DataGridViewTables.INDEX_COLUMNS.DEVIATION].Value = sumDiviation.ToString("F2");

            setFirstDisplayedScrollingRowIndex(m_dgwHours, m_tecView.lastHour);

            //Logging.Logg().Debug(@"PanelTecViewBase::FillGridHours () - ...");
        }

        private void NewDateRefresh()
        {
            //delegateStartWait ();
            if (!(delegateStartWait == null)) delegateStartWait(); else ;
            
            ChangeState();

            //delegateStopWait ();
            if (!(delegateStopWait == null)) delegateStopWait(); else ;
        }

        private void dtprDate_ValueChanged(object sender, EventArgs e)
        {
            if (update == true)
            {
                //Сравниваем даты/время ????
                if (!(m_pnlQuickData.dtprDate.Value.Date.CompareTo (m_tecView.m_curDate.Date) == 0))
                    m_tecView.currHour = false;
                else
                    ;

                //В этом методе даты/время приравниваем ???
                initTableHourRows ();

                NewDateRefresh();

                setRetroTickTime(m_tecView.lastHour, (m_tecView.lastMin - 1) * 3);
            }
            else
                update = true;
        }

        private void setNowDate()
        {
            //true, т.к. всегда вызов при result=true
            if (InvokeRequired == true)
                this.BeginInvoke (new DelegateBoolFunc (SetNowDate), true);
            else
                Logging.Logg().Error(@"PanelTecViewBase::setNowDate () - ... BeginInvoke (SetNowDate) - ...");
        }

        private void SetNowDate(bool received)
        {
            m_tecView.currHour = true;

            if (received)
            {
                update = false;
                m_pnlQuickData.dtprDate.Value = m_tecView.m_curDate;
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

        private void ChangeState()
        {
            m_tecView.m_curDate = m_pnlQuickData.dtprDate.Value;
            
            m_tecView.ChangeState ();
        }

        protected bool Started
        {
            get { return ! (m_timerCurrent == null); }
        }

        public override void Activate(bool active)
        {
            int err = 0;
            
            if (Started == true)
            {
                isActive = active;

                if (isActive == true)
                {
                    currValuesPeriod = 0;

                    ChangeState();

                    //m_pnlQuickData.OnSizeChanged (null, EventArgs.Empty);

                    m_timerCurrent.Change(0, 1000);
                }
                else
                {
                    m_tecView.ClearStates();

                    if (!(m_timerCurrent == null))
                        m_timerCurrent.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    else
                        ;
                }
            }
            else
                err = -1; //Ошибка
        }

        private void ShowValues(string caption)
        {
            /*MessageBox.Show(this, "state = " + state + "\naction = " + action + "\ndate = " + dtprDate.Value.ToString() +
                            "\nnow_date = " + DateTime.Now.ToString() + "\ngmt0 = " + System.TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).ToString() +
                            "\nselectedTime = " + selectedTime.ToString() + "lastHour = " + lastHour + "\nlastMin = " + lastMin + "\ncurrHour = " + currHour + 
                            "\nadminValuesReceived = " + adminValuesReceived, caption, MessageBoxButtons.OK);*/

            MessageBox.Show(this, "", caption, MessageBoxButtons.OK);
        }

        protected void setRetroTickTime(int hour, int min)
        {
            DateTime dt = m_pnlQuickData.dtprDate.Value.Date;
            dt = dt.AddHours(hour);
            dt = dt.AddMinutes(min);

            TickTime(dt);
        }

        /// <summary>
        /// Делегат обновления поля 'время сервера'
        /// </summary>
        /// <param name="dt">дата/время для отображения</param>
        private void TickTime(object dt)
        {
            m_pnlQuickData.lblServerTime.Text = ((DateTime)dt).ToString("HH:mm:ss");
        }

        /// <summary>
        /// Метод обратного вызова объекта 'timerCurrent'
        /// </summary>
        /// <param name="stateInfo">объкт синхронизации</param>
        private void TimerCurrent_Tick(Object stateInfo)
        {
            if ((m_tecView.currHour == true) && (isActive == true))
            {
                m_tecView.serverTime = m_tecView.serverTime.AddSeconds(1);

                if (InvokeRequired == true)
                    Invoke(delegateTickTime, m_tecView.serverTime);
                else
                    return;

                //if (!(((currValuesPeriod++) * 1000) < Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME]) * 1000))
                if (!(currValuesPeriod++ < Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME])))
                {
                    currValuesPeriod = 0;
                    NewDateRefresh();
                }
                else
                    ;
            }
            else
                ;

            //((ManualResetEvent)stateInfo).WaitOne();
            //try
            //{
            //    timerCurrent.Change(1000, Timeout.Infinite);
            //}
            //catch (Exception e)
            //{
            //    Logging.Logg().Exception(e, "Обращение к переменной 'timerCurrent'");
            //}
        }

        private void DrawGraphMins(int hour)
        {
            if (!(hour < m_tecView.m_valuesHours.Length))
                hour = m_tecView.m_valuesHours.Length - 1;
            else
                ;

            GraphPane pane = m_ZedGraphMins.GraphPane;

            pane.CurveList.Clear();

            int itemscount = 20;

            string[] names = new string[itemscount];

            double[] valuesRecommend = new double[itemscount];

            double[] valuesUDGe = new double[itemscount];

            double[] valuesFact = new double[itemscount];

            for (int i = 0; i < itemscount; i++)
            {
                valuesFact[i] = m_tecView.m_valuesMins[i + 1].valuesFact;
                valuesUDGe[i] = m_tecView.m_valuesMins[i + 1].valuesUDGe;
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
                    if ((i < (m_tecView.lastMin - 1)) || (!(m_tecView.adminValuesReceived == true)))
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

            if (!(FormMain.formGraphicsSettings.scale == true))
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

            if (HAdmin.SeasonDateTime.Date == m_tecView.m_curDate.Date) {
                int offset = m_tecView.GetSeasonHourOffset(hour + 1);
                pane.Title.Text = //"Средняя мощность на " + /*System.TimeZone.CurrentTimeZone.ToUniversalTime(*/dtprDate.Value/*)*/.ToShortDateString() + " " + 
                                    (hour + 1 - offset).ToString();
                if (HAdmin.SeasonDateTime.Hour == hour)
                    pane.Title.Text += "*";
                else
                    ;

                pane.Title.Text += @" час";
            }
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

            m_ZedGraphMins.AxisChange();

            m_ZedGraphMins.Invalidate();
        }

        private void DrawGraphHours()
        {
            GraphPane pane = m_ZedGraphHours.GraphPane;

            pane.CurveList.Clear();

            int itemscount = m_tecView.m_valuesHours.Length;

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
                if (m_tecView.m_curDate.Date.CompareTo (HAdmin.SeasonDateTime.Date) == 0) {
                    names[i] = (i + 1 - m_tecView.GetSeasonHourOffset(i + 1)).ToString();

                    if ((i + 0) == HAdmin.SeasonDateTime.Hour)
                        names[i] += @"*";
                    else
                        ;
                }
                else
                    names[i] = (i + 1).ToString();

                valuesPDiviation[i] = m_tecView.m_valuesHours[i].valuesUDGe + m_tecView.m_valuesHours[i].valuesDiviation;
                valuesODiviation[i] = m_tecView.m_valuesHours[i].valuesUDGe - m_tecView.m_valuesHours[i].valuesDiviation;
                valuesUDGe[i] = m_tecView.m_valuesHours[i].valuesUDGe;
                valuesFact[i] = m_tecView.m_valuesHours[i].valuesFact;

                if ((minimum > valuesPDiviation[i]) && (! (valuesPDiviation[i] == 0)))
                {
                    minimum = valuesPDiviation[i];
                    noValues = false;
                }

                if ((minimum > valuesODiviation[i]) && (! (valuesODiviation[i] == 0)))
                {
                    minimum = valuesODiviation[i];
                    noValues = false;
                }

                if ((minimum > valuesUDGe[i]) && (! (valuesUDGe[i] == 0)))
                {
                    minimum = valuesUDGe[i];
                    noValues = false;
                }

                if ((minimum > valuesFact[i]) && (! (valuesFact[i] == 0)))
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

            if (!(FormMain.formGraphicsSettings.scale == true))
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

                    //if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.SummerToWinter)
                    //    valuescount = m_tecView.lastHour + 1;
                    //else
                    //    if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.WinterToSummer)
                    //        valuescount = m_tecView.lastHour - 1;
                    //    else
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

            m_ZedGraphHours.AxisChange();

            m_ZedGraphHours.Invalidate();
        }

        public void UpdateGraphicsCurrent()
        {
            lock (m_tecView.m_lockValue)
            {
                DrawGraphMins(m_tecView.lastHour);
                DrawGraphHours();
            }
        }

        private void stctrViewPanel1_SplitterMoved(object sender, SplitterEventArgs e)
        {
        }

        private void stctrViewPanel2_SplitterMoved(object sender, SplitterEventArgs e)
        {
        }

        private void setTypeSourceData(TG.ID_TIME indx_time, CONN_SETT_TYPE type)
        {
            m_tecView.m_arTypeSourceData[(int)indx_time] = type;
        }

        private void sourceData_Click(ContextMenuStrip cms, ToolStripMenuItem sender, TG.ID_TIME indx_time)
        {
            if (sender.Checked == false)
            {
                ToolStripMenuItem itemASKUE = (ToolStripMenuItem)cms.Items[cms.Items.Count - 2] //Постоянно размещение пункта меню (2-ой снизу)
                    , itemSOTIASSO = (ToolStripMenuItem)cms.Items[cms.Items.Count - 1]; ////Постоянно размещение пункта меню (1-ый снизу)

                if (sender.Equals(itemASKUE) == true)
                {
                    setTypeSourceData(indx_time, CONN_SETT_TYPE.DATA_ASKUE);

                    itemASKUE.Checked = true;
                }
                else
                    if (sender.Equals(itemSOTIASSO) == true)
                    {
                        setTypeSourceData(indx_time, CONN_SETT_TYPE.DATA_SOTIASSO);

                        itemASKUE.Checked = false;
                    }
                    else
                        ;

                itemSOTIASSO.Checked = !itemASKUE.Checked;
            }
            else
                ;
        }

        protected void sourceDataMins_Click(object sender, EventArgs e)
        {
            sourceData_Click(m_ZedGraphMins.ContextMenuStrip, (ToolStripMenuItem)sender, TG.ID_TIME.MINUTES);
        }

        protected void sourceDataHours_Click(object sender, EventArgs e)
        {
            sourceData_Click(m_ZedGraphHours.ContextMenuStrip, (ToolStripMenuItem)sender, TG.ID_TIME.HOURS);
        }
    }
}
