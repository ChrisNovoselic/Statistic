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
                //10.11.2014 ВК редактируется ком./дисп.
                //if (values.valuesREC == 0)
                //    values.valuesForeignCommand = false;
                //else
                //    ;

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
            public enum INDEX_CONTEXTMENU_ITEM { SHOW_VALUES, SEPARATOR_1
                                                , COPY, SAVE, TO_EXCEL, SEPARATOR_2
                                                , SETTINGS_PRINT, PRINT, SEPARATOR_3
                                                , AISKUE_PLUS_SOTIASSO, AISKUE, SOTIASSO_3_MIN, SOTIASSO_1_MIN
                                                , COUNT };
            
            // контекстные меню
            protected class HContextMenuStripZedGraph : System.Windows.Forms.ContextMenuStrip
            {
                public HContextMenuStripZedGraph()
                {
                    InitializeComponent();
                }

                private void InitializeComponent()
                {
                    // 
                    // contextMenuStrip
                    // 
                    this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    new System.Windows.Forms.ToolStripMenuItem()
                    , new System.Windows.Forms.ToolStripSeparator(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem()
                    , new System.Windows.Forms.ToolStripSeparator(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem()
                    , new System.Windows.Forms.ToolStripSeparator(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem(),
                    new System.Windows.Forms.ToolStripMenuItem()
                    });
                    this.Name = "contextMenuStripMins";
                    this.Size = new System.Drawing.Size(198, 148);
                    
                    int indx = -1;
                    // 
                    // показыватьЗначенияToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.SHOW_VALUES;;
                    this.Items [indx].Name = "показыватьЗначенияToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "Показывать значения";
                    ((ToolStripMenuItem)this.Items[indx]).Checked = true;

                    // 
                    // копироватьToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.COPY;
                    this.Items[indx].Name = "копироватьToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "Копировать";

                    // 
                    // сохранитьToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.SAVE;
                    this.Items[indx].Name = "сохранитьToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "Сохранить график";

                    // 
                    // эксельToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.TO_EXCEL;
                    this.Items[indx].Name = "эксельToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "Сохранить в MS Excel";

                    // 
                    // параметрыПечатиToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.SETTINGS_PRINT;
                    this.Items[indx].Name = "параметрыПечатиToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "Параметры печати";
                    // 
                    // распечататьToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.PRINT;
                    this.Items[indx].Name = "распечататьToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "Распечатать";

                    // 
                    // источникАСКУЭиСОТИАССОToolStripMenuItem
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO;
                    this.Items[indx].Name = "источникАСКУЭиСОТИАССОToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = @"АИСКУЭ+СОТИАССО"; //"Источник: БД АИСКУЭ+СОТИАССО - 3 мин";
                    ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                    this.Items[indx].Enabled = false; //HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_ASKUE_PLUS_SOTIASSO) == true;
                    // 
                    // источникАСКУЭToolStripMenuItem
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.AISKUE;
                    this.Items[indx].Name = "источникАСКУЭToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    //Установлено в конструкторе "родителя"
                    //this.источникАСКУЭToolStripMenuItem.Text = "Источник: БД АСКУЭ - 3 мин";
                    ((ToolStripMenuItem)this.Items[indx]).Checked = true;
                    this.Items[indx].Enabled = false;
                    // 
                    // источникСОТИАССО3минToolStripMenuItem
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.SOTIASSO_3_MIN;
                    this.Items[indx].Name = "источникСОТИАССО3минToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = @"СОТИАССО(3 мин)"; //"Источник: БД СОТИАССО - 3 мин";
                    ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                    this.Items[indx].Enabled = false;
                    // 
                    // источникСОТИАССО1минToolStripMenuItem
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.SOTIASSO_1_MIN;
                    this.Items[indx].Name = "источникСОТИАССО1минToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = @"СОТИАССО(1 мин)"; //"Источник: БД СОТИАССО - 1 мин";
                    ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                    this.Items[indx].Enabled = false;
                }
            }

            private object m_lockValue;

            public string SourceDataText {
                get
                {
                    for (HZedGraphControl.INDEX_CONTEXTMENU_ITEM indx = INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; indx < HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT; indx ++)
                        if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)indx]).Checked == true)
                            return ((ToolStripMenuItem)ContextMenuStrip.Items[(int)indx]).Text;
                        else
                            ;

                    return string.Empty;
                }
            }

            public HZedGraphControl(object lockVal)
            {
                this.ContextMenuStrip = new HContextMenuStripZedGraph();
                
                InitializeComponent();

                m_lockValue = lockVal;
            }

            private void InitializeComponent()
            {
                // 
                // zedGraph
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

                InitializeEventHandler ();

                this.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.OnPointValueEvent);
                this.DoubleClickEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.OnDoubleClickEvent);
            }

            private void InitializeEventHandler() {
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.SHOW_VALUES].Click += new System.EventHandler(показыватьЗначенияToolStripMenuItem_Click);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.COPY].Click += new System.EventHandler(копироватьToolStripMenuItem_Click);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.SAVE].Click += new System.EventHandler(сохранитьToolStripMenuItem_Click);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.SETTINGS_PRINT].Click += new System.EventHandler(параметрыПечатиToolStripMenuItem_Click);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.PRINT].Click += new System.EventHandler(распечататьToolStripMenuItem_Click);
            }

            public void InitializeEventHandler(EventHandler fToExcel, EventHandler fSourceData)
            {
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.TO_EXCEL] .Click += new System.EventHandler(fToExcel);
                for (int i = (int)INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; i < this.ContextMenuStrip.Items.Count; i ++)
                    ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[i].Click += new System.EventHandler(fSourceData);
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
            public HZedGraphControlHours(object obj) : base(obj) { InitializeComponent(); }

            private void InitializeComponent()
            {
                this.ContextMenuStrip.Items [(int)INDEX_CONTEXTMENU_ITEM.AISKUE].Text = @"АИСКУЭ";
            }
        }

        protected class HZedGraphControlMins : HZedGraphControl
        {
            public HZedGraphControlMins(object obj) : base(obj) { InitializeComponent(); }

            private void InitializeComponent()
            {
                this.ContextMenuStrip.Items[(int)INDEX_CONTEXTMENU_ITEM.AISKUE].Text = @"АИСКУЭ";

                this.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XScaleFormatEvent);
            }

            public string XScaleFormatEvent(GraphPane pane, Axis axis, double val, int index)
            {
                int diskretnost = -1;
                if (pane.CurveList.Count > 0)
                    diskretnost = 60 / pane.CurveList[0].Points.Count;
                else
                    diskretnost = 60 / 20;

                return ((val) * diskretnost).ToString();
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
        public int m_ID { get { return m_tecView.m_ID; } }

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

            m_tecView = new TecView(TecView.TYPE_PANEL.VIEW, indx_tec, indx_comp);

            HMark markQueries = new HMark();
            markQueries.Marked((int)CONN_SETT_TYPE.ADMIN);
            markQueries.Marked((int)CONN_SETT_TYPE.PBR);
            markQueries.Marked((int)CONN_SETT_TYPE.DATA_AISKUE);
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
                m_pnlQuickData.addTGView(tg);
            }

            dgvCellStyleError = new DataGridViewCellStyle();
            dgvCellStyleError.BackColor = Color.Red;
            dgvCellStyleCommon = new DataGridViewCellStyle();

            if (tec.type() == TEC.TEC_TYPE.BIYSK)
                ; //this.parameters = FormMain.papar;
            else
                ;

            //??? Алгоритм д.б. следующим:
            // 1) FormMain.formGraphicsSettings.m_connSettType_SourceData = 
            // 2) в соответствии с п. 1 присвоить значения пунктам меню
            // 3) в соответствии с п. 2 присвоить значения m_tecView.m_arTypeSourceData[...
            //if (FormMain.formGraphicsSettings.m_connSettType_SourceData == CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE)
                //08.12.2014 - значения по умолчанию - как и пункты меню
                m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] = 
                m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.HOURS] = CONN_SETT_TYPE.DATA_AISKUE;
            //else
            //    m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] =
            //        m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.HOURS] = FormMain.formGraphicsSettings.m_connSettType_SourceData;

            isActive = false;

            update = false;

            delegateTickTime = new DelegateObjectFunc(TickTime);
        }

        private void FillDefaultMins()
        {
            int cnt = m_dgwMins.Rows.Count - 1
                , diskretnost = 60 / cnt;

            for (int i = 0; i < cnt; i++)
            {
                this.m_dgwMins.Rows[i].Cells[0].Value = ((i + 1) * diskretnost).ToString();
                this.m_dgwMins.Rows[i].Cells[1].Value = 0.ToString("F2");
                this.m_dgwMins.Rows[i].Cells[2].Value = 0.ToString("F2");
                this.m_dgwMins.Rows[i].Cells[3].Value = 0.ToString("F2");
                this.m_dgwMins.Rows[i].Cells[4].Value = 0.ToString("F2");
                this.m_dgwMins.Rows[i].Cells[5].Value = 0.ToString("F2");
            }
            this.m_dgwMins.Rows[cnt].Cells[0].Value = "Итог";
            this.m_dgwMins.Rows[cnt].Cells[1].Value = 0.ToString("F2");
            this.m_dgwMins.Rows[cnt].Cells[2].Value = "-";
            this.m_dgwMins.Rows[cnt].Cells[3].Value = "-";
            this.m_dgwMins.Rows[cnt].Cells[4].Value = 0.ToString("F2");
            this.m_dgwMins.Rows[cnt].Cells[5].Value = 0.ToString("F2");
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
            m_tecView.Start();

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

            //initTableMinRows ();
            initTableHourRows ();            

            ////??? Перенос в 'Activate'
            ////В зависимости от установленных признаков в контекстном меню
            //// , расположение пунктов меню постоянно: 1-ый, 2-ой снизу
            //// , если установлен один, то обязательно снят другой
            //setTypeSourceData(TG.ID_TIME.MINUTES, ((ToolStripMenuItem)m_ZedGraphMins.ContextMenuStrip.Items[m_ZedGraphMins.ContextMenuStrip.Items.Count - 2]).Checked == true ? CONN_SETT_TYPE.DATA_ASKUE : CONN_SETT_TYPE.DATA_SOTIASSO);
            //setTypeSourceData(TG.ID_TIME.HOURS, ((ToolStripMenuItem)m_ZedGraphHours.ContextMenuStrip.Items[m_ZedGraphHours.ContextMenuStrip.Items.Count - 2]).Checked == true ? CONN_SETT_TYPE.DATA_ASKUE : CONN_SETT_TYPE.DATA_SOTIASSO);

            m_evTimerCurrent = new ManualResetEvent(true);
            //timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), evTimerCurrent, 0, Timeout.Infinite);
            m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, 0, 1000);

            //timerCurrent = new System.Windows.Forms.Timer ();
            //timerCurrent.Tick += TimerCurrent_Tick;

            //??? TecView::Start
            update = false;
            //setNowDate(true); //??? ...не требуется

            //??? Отображение графиков по 'Activate (true)'
            //DrawGraphMins(0);
            //DrawGraphHours();
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

        protected void initTableMinRows()
        {
            if ((m_tecView.m_arTypeSourceData [(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE)
                || (m_tecView.m_arTypeSourceData [(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO)
                || (m_tecView.m_arTypeSourceData [(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN))
                m_dgwMins.InitRows (21, false);
            else
                if (m_tecView.m_arTypeSourceData [(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN)
                    m_dgwMins.InitRows (61, true);
                else
                    ;

            FillDefaultMins ();
        }

        private void updateGUI_TM_Gen()
        {
            if (IsHandleCreated/*InvokeRequired*/ == true)
                this.BeginInvoke(new DelegateFunc(UpdateGUI_TM_Gen));
            else
                Logging.Logg().Error(@"PanelTecViewBase::updateGUI_TM_Gen () - ... BeginInvoke (UpdateGUI_TM_Gen) - ... ID = " + m_tecView.m_ID);
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
            if (IsHandleCreated/*InvokeRequired*/ == true)
                this.BeginInvoke(new DelegateIntIntFunc(UpdateGUI_Fact), hour, min);
            else
                Logging.Logg().Error(@"PanelTecViewBase::updateGUI_Fact () - ... BeginInvoke (UpdateGUI_Fact) - ... ID = " + m_tecView.m_ID);
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
                    Logging.Logg().Exception(e, @"PanelTecViewBase::UpdateGUI_Fact () - ... ID = " + m_tecView.m_ID);
                }
            }
        }

        private void setFirstDisplayedScrollingRowIndex(DataGridView dgv, int lastIndx)
        {//Вызов ТОЛЬКО для таблицы с ЧАСовыми значениями...
            int iFirstDisplayedScrollingRowIndex = -1;

            if (lastIndx < dgv.DisplayedRowCount(true))
            {
                iFirstDisplayedScrollingRowIndex = 0; 
            }
            else {
                iFirstDisplayedScrollingRowIndex = lastIndx - dgv.DisplayedRowCount(true) + 1;

                if (!(m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_AISKUE))
                    //Если отображается еще один лишний час...
                    iFirstDisplayedScrollingRowIndex ++;
                else
                    ;
            }

            dgv.FirstDisplayedScrollingRowIndex = iFirstDisplayedScrollingRowIndex;
        }

        private void FillGridMins(int hour)
        {
            double sumFact = 0, sumUDGe = 0, sumDiviation = 0;
            int min = m_tecView.lastMin;

            if (! (min == 0))
                min--;
            else
                ;

            for (int i = 0; i < m_tecView.m_valuesMins.Length - 1; i++)
            {
                //Ограничить отображение (для режима АИСКУЭ+СОТИАССО)
                m_dgwMins.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.FACT].Value = m_tecView.m_valuesMins[i + 1].valuesFact.ToString("F2");
                if (i < min)
                {                    
                    sumFact += m_tecView.m_valuesMins[i + 1].valuesFact;
                }
                else
                    ;

                m_dgwMins.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.PBR].Value = m_tecView.m_valuesMins[i].valuesPBR.ToString("F2");
                m_dgwMins.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.PBRe].Value = m_tecView.m_valuesMins[i].valuesPBRe.ToString("F2");
                m_dgwMins.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.UDGe].Value = m_tecView.m_valuesMins[i].valuesUDGe.ToString("F2");
                sumUDGe += m_tecView.m_valuesMins[i].valuesUDGe;
                if ((i < min) && (! (m_tecView.m_valuesMins[i].valuesUDGe == 0)))
                {
                    m_dgwMins.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.DEVIATION].Value =
                        ((double)(m_tecView.m_valuesMins[i + 1].valuesFact - m_tecView.m_valuesMins[i].valuesUDGe)).ToString("F2");
                    //if (Math.Abs(m_tecView.m_valuesMins.valuesFact[i + 1] - m_tecView.m_valuesMins.valuesUDGe[i]) > m_tecView.m_valuesMins.valuesDiviation[i]
                    //    && m_tecView.m_valuesMins.valuesDiviation[i] != 0)
                    //    m_dgwMins.Rows[i].Cells[5].Style = dgvCellStyleError;
                    //else
                    m_dgwMins.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.DEVIATION].Style = dgvCellStyleCommon;

                    sumDiviation += m_tecView.m_valuesMins[i + 1].valuesFact - m_tecView.m_valuesMins[i].valuesUDGe;
                }
                else
                {
                    m_dgwMins.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.DEVIATION].Value = 0.ToString("F2");
                    m_dgwMins.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.DEVIATION].Style = dgvCellStyleCommon;
                }
            }

            int cnt = m_dgwMins.Rows.Count - 1;
            if (! (min > 0))
            {
                m_dgwMins.Rows[cnt].Cells[(int)DataGridViewTables.INDEX_COLUMNS.FACT].Value = 0.ToString("F2");
                m_dgwMins.Rows[cnt].Cells[(int)DataGridViewTables.INDEX_COLUMNS.UDGe].Value = 0.ToString("F2");
                m_dgwMins.Rows[cnt].Cells[(int)DataGridViewTables.INDEX_COLUMNS.DEVIATION].Value = 0.ToString("F2");
            }
            else
            {
                if (min > cnt)
                    min = cnt;
                else
                    ;

                m_dgwMins.Rows[cnt].Cells[(int)DataGridViewTables.INDEX_COLUMNS.FACT].Value = (sumFact / min).ToString("F2");
                m_dgwMins.Rows[cnt].Cells[(int)DataGridViewTables.INDEX_COLUMNS.UDGe].Value = m_tecView.m_valuesMins[0].valuesUDGe.ToString("F2");
                m_dgwMins.Rows[cnt].Cells[(int)DataGridViewTables.INDEX_COLUMNS.DEVIATION].Value = (sumDiviation / min).ToString("F2");
            }

            ////Назначить крайней видимой строкой - строку с крайним полученным значением
            //setFirstDisplayedScrollingRowIndex(m_dgwMins, m_tecView.lastMin);
            //Назначить крайней видимой строкой - крайнюю строку
            if (! (m_dgwMins.DisplayedRowCount(true) == 0))
                m_dgwMins.FirstDisplayedScrollingRowIndex = m_dgwMins.RowCount - m_dgwMins.DisplayedRowCount(true) + 1;
            else
                m_dgwMins.FirstDisplayedScrollingRowIndex = 0;

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
                   (m_tecView.m_valuesHours[i + 0].valuesLastMinutesTM > 1))
                    cntWarn++;
                else
                    cntWarn = 0;

                if (! (cntWarn == 0))
                    curCellStyle = dgvCellStyleError;
                else
                    curCellStyle = dgvCellStyleCommon;
                m_dgwHours.Rows[i + 0].Cells[(int)DataGridViewTables.INDEX_COLUMNS.LAST_MINUTES].Style = curCellStyle;

                if (m_tecView.m_valuesHours[i + 0].valuesLastMinutesTM > 1)
                {
                    if (cntWarn > 0)
                        strWarn = cntWarn + @":";
                    else
                        strWarn = string.Empty;

                    m_dgwHours.Rows[i + 0].Cells[(int)DataGridViewTables.INDEX_COLUMNS.LAST_MINUTES].Value = strWarn + m_tecView.m_valuesHours[i + 0].valuesLastMinutesTM.ToString("F2");
                }
                else
                    m_dgwHours.Rows[i + 0].Cells[(int)DataGridViewTables.INDEX_COLUMNS.LAST_MINUTES].Value = 0.ToString("F2");

                bool bDevVal = false;
                if (m_tecView.currHour == true)
                    if ((i < (receivedHour + 1)) && ((!(m_tecView.m_valuesHours[i].valuesUDGe == 0)) && (m_tecView.m_valuesHours[i].valuesFact > 0)))
                    {
                        if ((m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_AISKUE)
                            || (i < receivedHour))
                            bDevVal = true;
                        else
                            ;
                    }
                    else
                    {
                    }
                else
                    if (m_tecView.serverTime.Date.Equals(HAdmin.ToMoscowTimeZone(DateTime.Now.Date)) == true)
                        if ((i < (receivedHour + 1)) && (!(m_tecView.m_valuesHours[i].valuesUDGe == 0)) && (m_tecView.m_valuesHours[i].valuesFact > 0))
                        {
                            bDevVal = true;
                        }
                        else
                        {
                        }
                    else
                        if ((!(m_tecView.m_valuesHours[i].valuesUDGe == 0)) && (m_tecView.m_valuesHours[i].valuesFact > 0))
                        {
                            bDevVal = true;
                        }
                        else
                        {
                        }    

                m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.FACT].Value = m_tecView.m_valuesHours[i].valuesFact.ToString("F2");
                if (bDevVal == true)
                    sumFact += m_tecView.m_valuesHours[i].valuesFact;
                else
                    ;

                m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.PBR].Value = m_tecView.m_valuesHours[i].valuesPBR.ToString("F2");
                m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.PBRe].Value = m_tecView.m_valuesHours[i].valuesPBRe.ToString("F2");
                m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.UDGe].Value = m_tecView.m_valuesHours[i].valuesUDGe.ToString("F2");
                sumUDGe += m_tecView.m_valuesHours[i].valuesUDGe;

                if (bDevVal == true)
                {
                    m_dgwHours.Rows[i].Cells[(int)DataGridViewTables.INDEX_COLUMNS.DEVIATION].Value = ((double)(m_tecView.m_valuesHours[i].valuesFact - m_tecView.m_valuesHours[i].valuesUDGe)).ToString("F2");
                    if (Math.Abs(m_tecView.m_valuesHours[i].valuesFact - m_tecView.m_valuesHours[i].valuesUDGe) > m_tecView.m_valuesHours[i].valuesDiviation
                        && (!(m_tecView.m_valuesHours[i].valuesDiviation == 0)))
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
            }
            m_dgwHours.Rows[itemscount].Cells[(int)DataGridViewTables.INDEX_COLUMNS.FACT].Value = sumFact.ToString("F2");
            m_dgwHours.Rows[itemscount].Cells[(int)DataGridViewTables.INDEX_COLUMNS.UDGe].Value = sumUDGe.ToString("F2");
            m_dgwHours.Rows[itemscount].Cells[(int)DataGridViewTables.INDEX_COLUMNS.DEVIATION].Value = sumDiviation.ToString("F2");

            setFirstDisplayedScrollingRowIndex(m_dgwHours, m_tecView.lastHour);

            //Logging.Logg().Debug(@"PanelTecViewBase::FillGridHours () - ...");
        }

        protected void NewDateRefresh()
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
            if (IsHandleCreated/*InvokeRequired*/ == true)
                this.BeginInvoke (new DelegateBoolFunc (setNowDate), true);
            else
                Logging.Logg().Error(@"PanelTecViewBase::setNowDate () - ... BeginInvoke (SetNowDate) - ...");
        }

        protected void setNowDate(bool received)
        {
            m_tecView.currHour = true;

            if (received == true)
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
            ////Вариань №1
            //setNowDate(false);

            //Вариань №2
            m_tecView.currHour = true;
            NewDateRefresh();
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

                    ////??? Перенос в 'Activate'
                    ////??? Перенос в 'enabledDataSource_...'
                    ////В зависимости от установленных признаков в контекстном меню
                    //// , расположение пунктов меню постоянно: 1-ый, 2-ой снизу
                    //// , если установлен один, то обязательно снят другой
                    //setTypeSourceData(TG.ID_TIME.MINUTES, ((ToolStripMenuItem)m_ZedGraphMins.ContextMenuStrip.Items[m_ZedGraphMins.ContextMenuStrip.Items.Count - 2]).Checked == true ? CONN_SETT_TYPE.DATA_ASKUE : CONN_SETT_TYPE.DATA_SOTIASSO);
                    //setTypeSourceData(TG.ID_TIME.HOURS, ((ToolStripMenuItem)m_ZedGraphHours.ContextMenuStrip.Items[m_ZedGraphHours.ContextMenuStrip.Items.Count - 2]).Checked == true ? CONN_SETT_TYPE.DATA_ASKUE : CONN_SETT_TYPE.DATA_SOTIASSO);

                    HMark markSourceData = enabledSourceData_ToolStripMenuItems();

                    if (m_tecView.currHour == true)
                        NewDateRefresh();
                    else {
                        updateGraphicsRetro(markSourceData);
                    }

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

                if (IsHandleCreated/*InvokeRequired*/ == true)
                    Invoke(delegateTickTime, m_tecView.serverTime);
                else
                    return;

                //if (!(((currValuesPeriod++) * 1000) < Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME]) * 1000))
                if (!(currValuesPeriod++ < POOL_TIME))
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

            int itemscount = m_tecView.m_valuesMins.Length - 1
                , diskretnost = 60 / itemscount;

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
            int iName = -1;

            for (int i = 0; i < itemscount; i++)
            {
                iName = ((i + 1) * diskretnost);
                if (iName % 3 == 0)
                    names[i] = iName.ToString();
                else
                    names[i] = string.Empty;
                //valuesPDiviation[i] = m_valuesMins.valuesUDGe[i] + m_valuesMins.valuesDiviation[i];
                //valuesODiviation[i] = m_valuesMins.valuesUDGe[i] - m_valuesMins.valuesDiviation[i];

                if (m_tecView.currHour == true)
                {
                    if ((i < (m_tecView.lastMin - 1)) || (!(m_tecView.adminValuesReceived == true)))
                        valuesRecommend[i] = 0;
                    else
                        valuesRecommend[i] = m_tecView.recomendation;
                }
                else
                    ;

                //if (minimum > valuesPDiviation[i] && valuesPDiviation[i] != 0)
                //{
                //    minimum = valuesPDiviation[i];
                //    noValues = false;
                //}
                //else
                //    ;

                //if (minimum > valuesODiviation[i] && valuesODiviation[i] != 0)
                //{
                //    minimum = valuesODiviation[i];
                //    noValues = false;
                //}
                //else
                //    ;

                if (m_tecView.currHour == true)
                {
                    if (minimum > valuesRecommend[i] && valuesRecommend[i] != 0)
                    {
                        minimum = valuesRecommend[i];
                        noValues = false;
                    }
                }
                else
                    ;

                if (minimum > valuesUDGe[i] && valuesUDGe[i] != 0)
                {
                    minimum = valuesUDGe[i];
                    noValues = false;
                }
                else
                    ;

                if (minimum > valuesFact[i] && valuesFact[i] != 0)
                {
                    minimum = valuesFact[i];
                    noValues = false;
                }
                else
                    ;

                //if (maximum < valuesPDiviation[i])
                //    maximum = valuesPDiviation[i];
                //else
                //    ;

                //if (maximum < valuesODiviation[i])
                //    maximum = valuesODiviation[i];
                //else
                //    ;

                if (m_tecView.currHour == true)
                {
                    if (maximum < valuesRecommend[i])
                        maximum = valuesRecommend[i];
                    else
                        ;
                }
                else
                    ;

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
            else
                    ;

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
                    else
                        ;
                    maximum_scale = maximum + (maximum - minimum) * 0.2;
                }
                else
                {
                    minimum_scale = minimum - minimum * 0.2;
                    maximum_scale = maximum + maximum * 0.2;
                }
            }

            Color colorChart = Color.Empty
                , colorPCurve = Color.Empty;
            getColorZEDGraph(TG.ID_TIME.MINUTES, out colorChart, out colorPCurve);
            pane.Chart.Fill = new Fill(colorChart);

            LineItem curve2 = pane.AddCurve("УДГэ", null, valuesUDGe, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.UDG));
            //LineItem curve4 = pane.AddCurve("", null, valuesODiviation, graphSettings.divColor);
            //LineItem curve3 = pane.AddCurve("Возможное отклонение", null, valuesPDiviation, graphSettings.divColor);

            switch (FormMain.formGraphicsSettings.m_graphTypes)
            {
                case FormGraphicsSettings.GraphTypes.Bar:
                    if ((! (m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                        || (m_tecView.currHour == false))
                    {
                        //BarItem
                        pane.AddBar("Мощность", null, valuesFact, colorPCurve);
                        //BarItem
                        pane.AddBar("Рекомендуемая мощность", null, valuesRecommend, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.REC));
                    }
                    else
                    {
                        bool order = false; //Порядок "накладывания" значений...
                        double[] valuesSOTIASSO = null;
                        switch (m_tecView.lastMin)
                        {
                            case 0:
                                valuesSOTIASSO = new double[valuesFact.Length];
                                valuesSOTIASSO[m_tecView.lastMin] = valuesFact[m_tecView.lastMin];
                                valuesFact[m_tecView.lastMin] = 0F;
                                //Порядок "накладывания" значений
                                if (valuesRecommend[m_tecView.lastMin] > valuesSOTIASSO[m_tecView.lastMin])
                                    order = true;
                                else
                                    ;
                                break;
                            case 21:
                                //valuesFact - заполнен,
                                //valuesRecommend = 0
                                break;
                            default:
                                try
                                {
                                    valuesSOTIASSO = new double[valuesFact.Length];
                                    valuesSOTIASSO[m_tecView.lastMin - 1] = valuesFact[m_tecView.lastMin - 1];
                                    valuesFact[m_tecView.lastMin - 1] = 0F;
                                    //Порядок "накладывания" значений
                                    if (valuesRecommend[m_tecView.lastMin - 1] > valuesSOTIASSO[m_tecView.lastMin - 1])
                                        order = true;
                                    else
                                        ;
                                }
                                catch (Exception e) {
                                    Logging.Logg().Exception(e, @"PanelTecViewBase::DrawGraphMins (hour=" + hour + @") - ... m_tecView.lastMin(>0)=" + m_tecView.lastMin);
                                }
                                break;
                        }

                        //BarItem
                        pane.AddBar("Мощность(АСКУЭ)", null, valuesFact, colorPCurve);
                        if (order == true)
                        {
                            //BarItem
                            pane.AddBar("Мощность(СОТИАССО)", null, valuesSOTIASSO, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.SOTIASSO));
                            //BarItem
                            pane.AddBar("Рекомендуемая мощность", null, valuesRecommend, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.REC));
                        }
                        else
                        {
                            //BarItem
                            pane.AddBar("Рекомендуемая мощность", null, valuesRecommend, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.REC));
                            //BarItem                        
                            pane.AddBar("Мощность(СОТИАССО)", null, valuesSOTIASSO, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.SOTIASSO));                        
                        }
                    }
                    break;
                case FormGraphicsSettings.GraphTypes.Linear:
                    PointPairList listValuesSOTIASSO = null
                        , listValuesAISKUE = null
                        , listValuesRec = null;
                    if ((!(m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                        || (m_tecView.currHour == false))
                    {
                        switch (m_tecView.lastMin)
                        {
                            case 0:
                                //LineItem
                                listValuesRec = new PointPairList();
                                if ((m_tecView.adminValuesReceived == true) && (m_tecView.currHour == true))
                                    for (int i = 0; i < itemscount; i++)
                                        listValuesRec.Add((double)(i + 1), valuesRecommend[i]);
                                else
                                    ;
                                break;
                            default:
                                listValuesAISKUE = new PointPairList();
                                for (int i = 0; i < m_tecView.lastMin - 1; i++)
                                    listValuesAISKUE.Add((double)(i + 1), valuesFact[i]);

                                listValuesRec = new PointPairList();
                                if ((m_tecView.adminValuesReceived == true) && (m_tecView.currHour == true))
                                    for (int i = m_tecView.lastMin - 1; i < itemscount; i++)
                                        listValuesRec.Add((double)(i + 1), valuesRecommend[i]);
                                else
                                    ;
                                break;
                        }

                        //LineItem
                        pane.AddCurve("Мощность", listValuesAISKUE, colorPCurve);
                        //LineItem
                        pane.AddCurve("Рекомендуемая мощность", listValuesRec, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.REC));
                    }
                    else
                    {
                        switch (m_tecView.lastMin)
                        {
                            case 0:
                                if (valuesFact[m_tecView.lastMin] > 0)
                                {
                                    listValuesSOTIASSO = new PointPairList();
                                    listValuesSOTIASSO.Add(1F, valuesFact[m_tecView.lastMin]);
                                    if ((m_tecView.adminValuesReceived == true) && (m_tecView.currHour == true))
                                        for (int i = 1; i < itemscount; i++)
                                            listValuesRec.Add((double)(i + 1), valuesRecommend[i]);
                                    else
                                        ;
                                }
                                else
                                    ;
                                break;
                            default:
                                listValuesAISKUE = new PointPairList();
                                for (int i = 0; i < m_tecView.lastMin - 1; i++)
                                    listValuesAISKUE.Add((double)(i + 1), valuesFact[i]);
                                if (valuesFact[m_tecView.lastMin - 1] > 0)
                                {
                                    listValuesSOTIASSO = new PointPairList();
                                    listValuesSOTIASSO.Add((double)m_tecView.lastMin - 0, valuesFact[m_tecView.lastMin - 1]);

                                    if ((m_tecView.adminValuesReceived == true) && (m_tecView.currHour == true))
                                    {
                                        listValuesRec = new PointPairList();
                                        for (int i = m_tecView.lastMin - 0; i < itemscount; i++)
                                            listValuesRec.Add((double)(i + 1), valuesRecommend[i]);
                                    }
                                    else
                                        ;
                                }
                                else
                                {
                                    if ((m_tecView.adminValuesReceived == true) && (m_tecView.currHour == true))
                                    {
                                        listValuesRec = new PointPairList();
                                        for (int i = m_tecView.lastMin - 1; i < itemscount; i++)
                                            listValuesRec.Add((double)(i + 1), valuesRecommend[i]);
                                    }
                                    else
                                        ;
                                }
                                break;
                        }

                        pane.AddCurve("Мощность(АСКУЭ)", listValuesAISKUE, colorPCurve);
                        pane.AddCurve("Мощность(СОТИАССО)", listValuesSOTIASSO, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.SOTIASSO));
                        pane.AddCurve("Рекомендуемая мощность", listValuesRec, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.REC));
                    }
                    break;
                default:
                    break;
            }

            pane.BarSettings.Type = BarType.Overlay;

            pane.XAxis.Type = AxisType.Linear;

            pane.XAxis.Title.Text = "";
            pane.YAxis.Title.Text = "";

            //По просьбе НСС-машинистов ДОБАВИТЬ - источник данных 05.12.2014
            //pane.Title.Text = @" (" + m_ZedGraphMins.SourceDataText + @")";
            pane.Title.Text = m_ZedGraphMins.SourceDataText;
            pane.Title.Text += new string(' ', 29);

            if (HAdmin.SeasonDateTime.Date == m_tecView.m_curDate.Date) {
                int offset = m_tecView.GetSeasonHourOffset(hour + 1);
                pane.Title.Text += //"Средняя мощность на " + /*System.TimeZone.CurrentTimeZone.ToUniversalTime(*/dtprDate.Value/*)*/.ToShortDateString() + " " + 
                                    (hour + 1 - offset).ToString();
                if (HAdmin.SeasonDateTime.Hour == hour)
                    pane.Title.Text += "*";
                else
                    ;

                pane.Title.Text += @" час";
            }
            else
                pane.Title.Text += //"Средняя мощность на " + /*System.TimeZone.CurrentTimeZone.ToUniversalTime(*/dtprDate.Value/*)*/.ToShortDateString() + " " + 
                                    (hour + 1).ToString() + " час";

            //По просьбе пользователей УБРАТЬ - источник данных
            //pane.Title.Text += @" (" + m_ZedGraphMins.SourceDataText + @")";

            pane.XAxis.Scale.Min = 0.5;
            pane.XAxis.Scale.Max = pane.XAxis.Scale.Min + itemscount;
            pane.XAxis.Scale.MinorStep = 1;
            pane.XAxis.Scale.MajorStep = itemscount / 20;

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
            pane.XAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

            // Включаем отображение сетки напротив крупных рисок по оси Y
            pane.YAxis.MajorGrid.IsVisible = true;
            // Аналогично задаем вид пунктирной линии для крупных рисок по оси Y
            pane.YAxis.MajorGrid.DashOn = 10;
            pane.YAxis.MajorGrid.DashOff = 5;
            // толщина линий
            pane.YAxis.MajorGrid.PenWidth = 0.1F;
            pane.YAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

            // Включаем отображение сетки напротив мелких рисок по оси Y
            pane.YAxis.MinorGrid.IsVisible = true;
            // Длина штрихов равна одному пикселю, ... 
            pane.YAxis.MinorGrid.DashOn = 1;
            pane.YAxis.MinorGrid.DashOff = 2;
            // толщина линий
            pane.YAxis.MinorGrid.PenWidth = 0.1F;
            pane.YAxis.MinorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

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
                else
                    ;

                if (maximum < valuesODiviation[i])
                    maximum = valuesODiviation[i];
                else
                    ;

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
            else
                ;

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

            Color colorChart = Color.Empty
                , colorPCurve = Color.Empty;
            getColorZEDGraph(TG.ID_TIME.HOURS, out colorChart, out colorPCurve);

            pane.Chart.Fill = new Fill(colorChart);

            //LineItem
            pane.AddCurve("УДГэ", null, valuesUDGe, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.UDG));
            //LineItem
            pane.AddCurve("", null, valuesODiviation, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.DIVIATION));
            //LineItem
            pane.AddCurve("Возможное отклонение", null, valuesPDiviation, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.DIVIATION));

            if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Bar)
            {
                if (! (m_tecView.m_arTypeSourceData [(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                    //BarItem
                    pane.AddBar("Мощность", null, valuesFact, colorPCurve);
                else {
                    int lh = -1;
                    if (m_tecView.currHour == true)
                        lh = m_tecView.lastHour;
                    else
                        if (HAdmin.ToMoscowTimeZone(DateTime.Now).Date.Equals(m_tecView.serverTime.Date) == true)
                            lh = m_tecView.serverTime.Hour;
                        else
                            lh = 24;

                    double [] valuesASKUE = new double [lh]
                        , valuesSOTIASSO = new double[lh + 1];
                    for (int i = 0; i < lh + 1; i ++)
                    {
                        if (i < lh - 0)
                        {
                            valuesASKUE[i] = valuesFact[i];
                            valuesSOTIASSO [i] = 0;
                        }
                        else
                        {
                            if (i < valuesFact.Length)
                                valuesSOTIASSO[i] = valuesFact[i];
                            else
                                ;
                        }
                    }

                    pane.AddBar("Мощность(АИСКУЭ)", null, valuesASKUE, colorPCurve);
                    pane.AddBar("Мощность(СОТИАССО)", null, valuesSOTIASSO, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.SOTIASSO));
                }
            }
            else
            {
                if (FormMain.formGraphicsSettings.m_graphTypes == FormGraphicsSettings.GraphTypes.Linear)
                {
                    if (! (m_tecView.m_arTypeSourceData [(int)TG.ID_TIME.HOURS] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                    {
                        double[] valuesFactLinear = new double[m_tecView.lastHour];
                        for (int i = 0; i < m_tecView.lastHour; i++)
                            valuesFactLinear[i] = valuesFact[i];

                        //LineItem
                        pane.AddCurve("Мощность", null, valuesFactLinear, colorPCurve);
                    }
                    else {
                        PointPairList valuesASKUE = new PointPairList()
                            , valuesSOTIASSO = new PointPairList();

                        for (int i = 0; i < m_tecView.lastHour + 1; i++)
                        {
                            if (i < m_tecView.lastHour - 0)
                            {
                                valuesASKUE.Add((double)(i + 1), valuesFact[i]);
                            }
                            else
                            {
                                valuesSOTIASSO.Add((double)(i + 1), valuesFact[i]);
                            }
                        }

                        //LineItem
                        pane.AddCurve("Мощность(АИСКУЭ)", valuesASKUE, colorPCurve);
                        //LineItem
                        pane.AddCurve("Мощность(СОТИАССО)", valuesSOTIASSO, FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.SOTIASSO));
                    }
                }
            }

            //Для размещения в одной позиции ОДНого значения
            pane.BarSettings.Type = BarType.Overlay;

            //...из minutes
            pane.XAxis.Scale.Min = 0.5;
            pane.XAxis.Scale.Max = pane.XAxis.Scale.Min + itemscount;
            pane.XAxis.Scale.MinorStep = 1;
            pane.XAxis.Scale.MajorStep = 1; //itemscount / 20;

            pane.XAxis.Type = AxisType.Linear; //...из minutes
            //pane.XAxis.Type = AxisType.Text;
            pane.XAxis.Title.Text = "";
            pane.YAxis.Title.Text = "";
            //По просьбе НСС-машинистов ДОБАВИТЬ - источник данных  05.12.2014
            //pane.Title.Text = @"(" + m_ZedGraphHours.SourceDataText + @")";
            pane.Title.Text = m_ZedGraphHours.SourceDataText;
            pane.Title.Text += new string(' ', 29);
            pane.Title.Text +=
                //"Мощность " +
                ////По просьбе пользователей УБРАТЬ - источник данных
                ////@"(" + m_ZedGraphHours.SourceDataText  + @") " +
                //@"на " +
                m_pnlQuickData.dtprDate.Value.ToShortDateString();

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
            pane.XAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

            // Включаем отображение сетки напротив крупных рисок по оси Y
            pane.YAxis.MajorGrid.IsVisible = true;
            // Аналогично задаем вид пунктирной линии для крупных рисок по оси Y
            pane.YAxis.MajorGrid.DashOn = 10;
            pane.YAxis.MajorGrid.DashOff = 5;
            // толщина линий
            pane.YAxis.MajorGrid.PenWidth = 0.1F;
            pane.YAxis.MajorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

            // Включаем отображение сетки напротив мелких рисок по оси Y
            pane.YAxis.MinorGrid.IsVisible = true;
            // Длина штрихов равна одному пикселю, ... 
            pane.YAxis.MinorGrid.DashOn = 1;
            pane.YAxis.MinorGrid.DashOff = 2;
            // толщина линий
            pane.YAxis.MinorGrid.PenWidth = 0.1F;
            pane.YAxis.MinorGrid.Color = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.GRID);

            // Устанавливаем интересующий нас интервал по оси Y
            pane.YAxis.Scale.Min = minimum_scale;
            pane.YAxis.Scale.Max = maximum_scale;

            m_ZedGraphHours.AxisChange();

            m_ZedGraphHours.Invalidate();
        }

        //private bool enabledSourceData_ToolStripMenuItems () {
        //private bool[] enabledSourceData_ToolStripMenuItems()
        private HMark enabledSourceData_ToolStripMenuItems()
        {
            //bool [] arRes = new bool [] {false, false};
            HMark markRes = new HMark ();

            if (FormMain.formGraphicsSettings.m_connSettType_SourceData == CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE) {
                //Пункты меню доступны для выбора
                ((ToolStripMenuItem)m_ZedGraphMins.ContextMenuStrip.Items[(int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO]).Enabled =
                ((ToolStripMenuItem)m_ZedGraphHours.ContextMenuStrip.Items[(int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO]).Enabled =
                    HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_ASKUE_PLUS_SOTIASSO);

                ((ToolStripMenuItem)m_ZedGraphMins.ContextMenuStrip.Items[(int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.SOTIASSO_3_MIN]).Enabled =
                ((ToolStripMenuItem)m_ZedGraphHours.ContextMenuStrip.Items[(int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.SOTIASSO_3_MIN]).Enabled =
                    HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_SOTIASSO_3_MIN);

                ((ToolStripMenuItem)m_ZedGraphMins.ContextMenuStrip.Items[(int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE]).Enabled =                 
                ((ToolStripMenuItem)m_ZedGraphMins.ContextMenuStrip.Items[(int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.SOTIASSO_1_MIN]).Enabled =
                ((ToolStripMenuItem)m_ZedGraphHours.ContextMenuStrip.Items[(int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE]).Enabled =                
                ((ToolStripMenuItem)m_ZedGraphHours.ContextMenuStrip.Items[(int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.SOTIASSO_1_MIN]).Enabled =
                    true;

                //оставить "как есть", но изменить источник данных при найденном НЕсоответствии
            } else {
                //Пункты меню НЕдоступны для выбора
                //Принудительно установить источник данных
                if (! (m_tecView.m_arTypeSourceData [(int)TG.ID_TIME.MINUTES] == FormMain.formGraphicsSettings.m_connSettType_SourceData))
                {
                    m_tecView.m_arTypeSourceData [(int)TG.ID_TIME.MINUTES] = FormMain.formGraphicsSettings.m_connSettType_SourceData;

                    //arRes [(int)TG.ID_TIME.MINUTES] = true;
                    markRes.Marked ((int)TG.ID_TIME.MINUTES);
                } else {
                }

                if (!(m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.HOURS] == FormMain.formGraphicsSettings.m_connSettType_SourceData))
                {
                    m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.HOURS] = FormMain.formGraphicsSettings.m_connSettType_SourceData;

                    //arRes[(int)TG.ID_TIME.HOURS] = true;
                    markRes.Marked((int)TG.ID_TIME.HOURS);
                }
                else
                {
                }

                //if (arRes[(int)TG.ID_TIME.MINUTES] == true) {
                if (markRes.IsMarked ((int)TG.ID_TIME.MINUTES) == true)
                {
                    initTableMinRows ();

                    enabledSourceData_ToolStripMenuItems (m_ZedGraphMins.ContextMenuStrip, m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.MINUTES]);
                }
                else ;

                //if (arRes[(int)TG.ID_TIME.HOURS] == true)
                if (markRes.IsMarked ((int)TG.ID_TIME.HOURS) == true) {
                    enabledSourceData_ToolStripMenuItems(m_ZedGraphHours.ContextMenuStrip, m_tecView.m_arTypeSourceData[(int)TG.ID_TIME.HOURS]);
                }
                else ;
            }

            //return arRes[(int)TG.ID_TIME.MINUTES] || arRes[(int)TG.ID_TIME.HOURS];
            //return arRes;
            return markRes;
        }

        private void enabledSourceData_ToolStripMenuItems (ContextMenuStrip menu, CONN_SETT_TYPE type)
        {
            int indx = -1;

            //Временно активируем пункты контекстного меню
            // для возможности изменить источник данных
            // (пользователь его изменил принудительно)
            for (indx = (int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; indx < (int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT; indx++)
                menu.Items[indx].Enabled =
                    true;

            switch (type)
            {
                case CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO:
                    indx = (int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO;
                    break;
                case CONN_SETT_TYPE.DATA_AISKUE:
                    indx = (int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE;
                    break;
                case CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                    indx = (int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.SOTIASSO_3_MIN;
                    break;
                case CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                    indx = (int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.SOTIASSO_1_MIN;
                    break;
                default:
                    break;
            }

            //Изменить источник данных
            ((ToolStripMenuItem)menu.Items[indx]).PerformClick();

            //Восстанавливаем "недоступность" пунктов контекстного меню
            for (indx = (int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; indx < (int)HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT; indx ++)
                menu.Items[indx].Enabled =
                    false;
        }

        /// <summary>
        /// Обновление компонентов вкладки с проверкой изменения источника данных
        /// </summary>
        /// <param name="markUpdate">указывает на изменившиеся источники данных</param>
        private void updateGraphicsRetro (HMark markUpdate)
        {
            //if (markUpdate.IsMarked() == false)
            //    return;
            //else
            if ((markUpdate.IsMarked((int)TG.ID_TIME.MINUTES) == true) && (markUpdate.IsMarked((int)TG.ID_TIME.HOURS) == false))
                //Изменение источника данных МИНУТЫ
                m_tecView.GetRetroMins();
            else
                if ((markUpdate.IsMarked((int)TG.ID_TIME.MINUTES) == false) && (markUpdate.IsMarked((int)TG.ID_TIME.HOURS) == true))
                    //Изменение источника данных ЧАС
                    m_tecView.GetRetroHours();
                else
                    if ((markUpdate.IsMarked((int)TG.ID_TIME.MINUTES) == true) && (markUpdate.IsMarked((int)TG.ID_TIME.HOURS) == true))
                        //Изменение источника данных ЧАС, МИНУТЫ
                        m_tecView.GetRetroValues();
                    else
                        ;
        }

        public void UpdateGraphicsCurrent(int type)
        {
            lock (m_tecView.m_lockValue)
            {
                //??? Проверка 'type' TYPE_UPDATEGUI
                HMark markChanged = enabledSourceData_ToolStripMenuItems ();
                if (markChanged.IsMarked () == false) {
                    DrawGraphMins(m_tecView.lastHour);
                    DrawGraphHours();
                } else {
                    if (m_tecView.currHour == true)
                        NewDateRefresh();
                    else
                    {//m_tecView.currHour == false
                        updateGraphicsRetro(markChanged);
                    }
                }
            }
        }

        private void stctrViewPanel1_SplitterMoved(object sender, SplitterEventArgs e)
        {
        }

        private void stctrViewPanel2_SplitterMoved(object sender, SplitterEventArgs e)
        {
        }

        private void sourceData_Click(ContextMenuStrip cms, ToolStripMenuItem sender, TG.ID_TIME indx_time)
        {
            CONN_SETT_TYPE prevTypeSourceData = m_tecView.m_arTypeSourceData[(int)indx_time]
                , curTypeSourceData = prevTypeSourceData;

            if (sender.Checked == false)
            {
                HZedGraphControl.INDEX_CONTEXTMENU_ITEM indx
                    , indxChecked = HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT;
                for (indx = HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; indx < HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT; indx++)
                    if (sender.Equals(cms.Items[(int)indx]) == true) {
                        indxChecked = indx;
                        ((ToolStripMenuItem)cms.Items[(int)indxChecked]).Checked = true;
                        
                        break;
                    }
                    else
                        ;

                if (! (indxChecked == HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT))
                {
                    for (indx = HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; indx < HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT; indx++)
                        if (! (indx == indxChecked))
                            ((ToolStripMenuItem)cms.Items[(int)indx]).Checked = false;
                        else
                            ;

                    switch (indxChecked) {
                        case HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO:
                            curTypeSourceData = CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO;
                            break;
                        case HZedGraphControl.INDEX_CONTEXTMENU_ITEM.AISKUE:
                            curTypeSourceData = CONN_SETT_TYPE.DATA_AISKUE;
                            break;
                        case HZedGraphControl.INDEX_CONTEXTMENU_ITEM.SOTIASSO_3_MIN:
                            curTypeSourceData = CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN;
                            break;
                        case HZedGraphControl.INDEX_CONTEXTMENU_ITEM.SOTIASSO_1_MIN:
                            curTypeSourceData = CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN;
                            break;
                        default:
                            indx = HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT;
                            break;
                    }

                    m_tecView.m_arTypeSourceData[(int)indx_time] = curTypeSourceData;


                    if (indx_time == TG.ID_TIME.MINUTES)
                    {
                        bool bInitTableMinRows = true;

                        switch (prevTypeSourceData)
                        {
                            case CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO:
                                switch (m_tecView.m_arTypeSourceData[(int)indx_time])
                                {
                                    //case CONN_SETT_TYPE.DATA_ASKUE_PLUS_SOTIASSO:
                                    //    break;
                                    case CONN_SETT_TYPE.DATA_AISKUE:
                                        bInitTableMinRows = false;
                                        break;
                                    case CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                                        bInitTableMinRows = false;
                                        break;
                                    case CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                                        //bInitTableMinRows = true;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case CONN_SETT_TYPE.DATA_AISKUE:
                                switch (m_tecView.m_arTypeSourceData[(int)indx_time])
                                {
                                    case CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO:
                                        bInitTableMinRows = false;
                                        break;
                                    //case CONN_SETT_TYPE.DATA_ASKUE:
                                    //    break;
                                    case CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                                        bInitTableMinRows = false;
                                        break;
                                    case CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                                        //bInitTableMinRows = true;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                                switch (m_tecView.m_arTypeSourceData[(int)indx_time])
                                {
                                    case CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO:
                                        bInitTableMinRows = false;
                                        break;
                                    case CONN_SETT_TYPE.DATA_AISKUE:
                                        bInitTableMinRows = false;
                                        break;
                                    //case CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                                    //    break;
                                    case CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                                        //bInitTableMinRows = true;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                                switch (m_tecView.m_arTypeSourceData[(int)indx_time])
                                {
                                    case CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO:
                                        //bInitTableMinRows = true;
                                        break;
                                    case CONN_SETT_TYPE.DATA_AISKUE:
                                        //bInitTableMinRows = true;
                                        break;
                                    case CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN:
                                        //bInitTableMinRows = true;
                                        break;
                                    //case CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN:
                                    //    break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                break;
                        }                    

                        if (bInitTableMinRows == true)
                            initTableMinRows();
                        else
                            ;
                    }
                    else
                        ;

                    if (m_tecView.currHour == true)
                        NewDateRefresh();
                    else
                    {//m_tecView.currHour == false
                        if (indx_time == TG.ID_TIME.MINUTES)
                            m_tecView.GetRetroMins();
                        else
                            m_tecView.GetRetroHours();
                    }
                }
                else
                    ; //Не найден ни ОДИН выделенный пункт контестного меню
            }
            else
                ; //Изменений нет

            //if (enabledSourceData_ToolStripMenuItems () == true) {
            //    NewDateRefresh ();
            //}
            //else
            //    ;
        }

        protected void sourceDataMins_Click(object sender, EventArgs e)
        {
            sourceData_Click(m_ZedGraphMins.ContextMenuStrip, (ToolStripMenuItem)sender, TG.ID_TIME.MINUTES);
        }

        protected void sourceDataHours_Click(object sender, EventArgs e)
        {
            sourceData_Click(m_ZedGraphHours.ContextMenuStrip, (ToolStripMenuItem)sender, TG.ID_TIME.HOURS);
        }

        private void getColorZEDGraph(TG.ID_TIME id_time, out Color colChart, out Color colP)
        {
            //Значения по умолчанию
            colChart = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.BG_ASKUE);
            colP = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.ASKUE);

            if ((m_tecView.m_arTypeSourceData[(int)id_time] == CONN_SETT_TYPE.DATA_AISKUE)
                || (m_tecView.m_arTypeSourceData[(int)id_time] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                ; // ...по умолчанию 
            else
                if ((m_tecView.m_arTypeSourceData[(int)id_time] == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN)
                    || (m_tecView.m_arTypeSourceData[(int)id_time] == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN))
                {
                    colChart = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.BG_SOTIASSO);
                    colP = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.SOTIASSO);
                }
                else
                    ;
        }
    }
}
