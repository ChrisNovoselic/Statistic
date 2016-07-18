using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
//using System.ComponentModel;
using System.Diagnostics;
using System.Data;
//using System.Data.SqlClient;
using System.Drawing; //Color..
using System.Threading;
using System.Globalization;

using ZedGraph;
using GemBox.Spreadsheet;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    public abstract partial  class PanelTecViewBase : PanelStatisticWithTableHourRows
    {
        protected PanelCustomTecView.HLabelCustomTecView m_label;

        protected uint SPLITTER_PERCENT_VERTICAL;

        //protected static AdminTS.TYPE_FIELDS s_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;

        protected abstract class HZedGraphControl : ZedGraph.ZedGraphControl
        {
            public enum INDEX_CONTEXTMENU_ITEM
            {
                SHOW_VALUES,
                SEPARATOR_1
                    , COPY, SAVE, TO_EXCEL,
                SEPARATOR_2
                    , SETTINGS_PRINT, PRINT,
                SEPARATOR_3
                    , AISKUE_PLUS_SOTIASSO, AISKUE, SOTIASSO_3_MIN,
                SOTIASSO_1_MIN
                    , COUNT
            };
            /// <summary>
            /// ������� - ��������� ������� �������������������������
            /// </summary>
            public DelegateFunc delegateSetScale;
            /// <summary>
            /// ����� ��� ������������ ����
            /// </summary>
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
                    // ������������������ToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.SHOW_VALUES; ;
                    this.Items[indx].Name = "������������������ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "���������� ��������";
                    ((ToolStripMenuItem)this.Items[indx]).Checked = true;

                    // 
                    // ����������ToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.COPY;
                    this.Items[indx].Name = "����������ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "����������";

                    // 
                    // ���������ToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.SAVE;
                    this.Items[indx].Name = "���������ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "��������� ������";

                    // 
                    // ������ToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.TO_EXCEL;
                    this.Items[indx].Name = "������ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "��������� � MS Excel";

                    // 
                    // ���������������ToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.SETTINGS_PRINT;
                    this.Items[indx].Name = "���������������ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "��������� ������";
                    // 
                    // �����������ToolStripMenuItemMins
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.PRINT;
                    this.Items[indx].Name = "�����������ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = "�����������";

                    // 
                    // �����������������������ToolStripMenuItem
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO;
                    this.Items[indx].Name = "�����������������������ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = @"������+��������"; //"��������: �� ������+�������� - 3 ���";
                    ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                    this.Items[indx].Enabled = false; //HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.SOURCEDATA_ASKUE_PLUS_SOTIASSO) == true;
                    // 
                    // ��������������ToolStripMenuItem
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.AISKUE;
                    this.Items[indx].Name = "��������������ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    //����������� � ������������ "��������"
                    //this.��������������ToolStripMenuItem.Text = "��������: �� ������ - 3 ���";
                    ((ToolStripMenuItem)this.Items[indx]).Checked = true;
                    this.Items[indx].Enabled = false;
                    // 
                    // ����������������3���ToolStripMenuItem
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.SOTIASSO_3_MIN;
                    this.Items[indx].Name = "����������������3���ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = @"��������(3 ���)"; //"��������: �� �������� - 3 ���";
                    ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                    this.Items[indx].Enabled = false;
                    // 
                    // ����������������1���ToolStripMenuItem
                    // 
                    indx = (int)INDEX_CONTEXTMENU_ITEM.SOTIASSO_1_MIN;
                    this.Items[indx].Name = "����������������1���ToolStripMenuItem";
                    this.Items[indx].Size = new System.Drawing.Size(197, 22);
                    this.Items[indx].Text = @"��������(1 ���)"; //"��������: �� �������� - 1 ���";
                    ((ToolStripMenuItem)this.Items[indx]).Checked = false;
                    this.Items[indx].Enabled = false;
                }
            }

            private object m_lockValue;
            /// <summary>
            /// �����, ���������� ��� ������������ ������
            /// </summary>
            public string SourceDataText
            {
                get
                {
                    for (HZedGraphControl.INDEX_CONTEXTMENU_ITEM indx = INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; indx < HZedGraphControl.INDEX_CONTEXTMENU_ITEM.COUNT; indx++)
                        if (((ToolStripMenuItem)ContextMenuStrip.Items[(int)indx]).Checked == true)
                            return ((ToolStripMenuItem)ContextMenuStrip.Items[(int)indx]).Text;
                        else
                            ;

                    return string.Empty;
                }
            }
            /// <summary>
            /// ����������� - �������� (� �����������)
            /// </summary>
            /// <param name="lockVal">������ �������������</param>
            /// <param name="fSetScale">������� ��������� �������� ���������������</param>
            public HZedGraphControl(object lockVal, DelegateFunc fSetScale)
            {
                this.ContextMenuStrip = new HContextMenuStripZedGraph();

                InitializeComponent();

                m_lockValue = lockVal;

                delegateSetScale = fSetScale;
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

                InitializeEventHandler();

                this.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(this.OnPointValueEvent);
                this.DoubleClickEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.OnDoubleClickEvent);
            }
            /// <summary>
            /// ������������� ������������ ������� ��� ������ ������� ���� (�����������)
            /// </summary>
            private void InitializeEventHandler()
            {
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.SHOW_VALUES].Click += new System.EventHandler(������������������ToolStripMenuItem_Click);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.COPY].Click += new System.EventHandler(����������ToolStripMenuItem_Click);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.SAVE].Click += new System.EventHandler(���������ToolStripMenuItem_Click);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.SETTINGS_PRINT].Click += new System.EventHandler(���������������ToolStripMenuItem_Click);
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.PRINT].Click += new System.EventHandler(�����������ToolStripMenuItem_Click);
            }
            /// <summary>
            /// ������������� ������������ ������� ��� ������ ������� ���� (������� � MS_Excel, ��������� ���� ������������ ������)
            /// </summary>
            /// <param name="fToExcel">������� ��������� ������� - ������� � MS_Excel</param>
            /// <param name="fSourceData">������� ��������� ������� - ��������� ���� ������������ ������</param>
            public void InitializeEventHandler(EventHandler fToExcel, EventHandler fSourceData)
            {
                ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[(int)INDEX_CONTEXTMENU_ITEM.TO_EXCEL].Click += new System.EventHandler(fToExcel);
                for (int i = (int)INDEX_CONTEXTMENU_ITEM.AISKUE_PLUS_SOTIASSO; i < this.ContextMenuStrip.Items.Count; i++)
                    ((HContextMenuStripZedGraph)this.ContextMenuStrip).Items[i].Click += new System.EventHandler(fSourceData);
            }

            private void ������������������ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
                this.IsShowPointValues = ((ToolStripMenuItem)sender).Checked;
            }

            private void ����������ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                lock (m_lockValue)
                {
                    this.Copy(false);
                }
            }

            private void ���������������ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                PageSetupDialog pageSetupDialog = new PageSetupDialog();
                pageSetupDialog.Document = this.PrintDocument;
                pageSetupDialog.ShowDialog();
            }

            private void �����������ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                lock (m_lockValue)
                {
                    this.PrintDocument.Print();
                }
            }

            private void ���������ToolStripMenuItem_Click(object sender, EventArgs e)
            {
                lock (m_lockValue)
                {
                    this.SaveAs();
                }
            }

            protected virtual string OnPointValueEvent(object sender, GraphPane pane, CurveItem curve, int iPt)
            {
                return curve[iPt].Y.ToString("F2");
            }

            private bool OnDoubleClickEvent(ZedGraphControl sender, MouseEventArgs e)
            {
                //FormMain.formGraphicsSettings.SetScale();
                delegateSetScale();

                return true;
            }

            public abstract void Draw (TecView.valuesTEC []values, params object []pars);

            //protected void getColorZedGraph(HDateTime.INTERVAL id_time, out Color colChart, out Color colP)
            //{
            //    getColorZedGraph(m_tecView.m_arTypeSourceData[(int)id_time], out colChart, out colP);
            //}

            protected void getColorZedGraph(CONN_SETT_TYPE typeConnSett, out Color colChart, out Color colP)
            {
                //�������� �� ���������
                colChart = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.BG_ASKUE);
                colP = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.ASKUE);

                if ((typeConnSett == CONN_SETT_TYPE.DATA_AISKUE)
                    || (typeConnSett == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO))
                    ; // ...�� ��������� 
                else
                    if ((typeConnSett == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN)
                        || (typeConnSett == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN))
                    {
                        colChart = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.BG_SOTIASSO);
                        colP = FormMain.formGraphicsSettings.COLOR(FormGraphicsSettings.INDEX_COLOR.SOTIASSO);
                    }
                    else
                        ;
            }
        
            public virtual bool FindNearestObject (PointF p, Graphics g, out object obj, out int index)
            {
                return GraphPane.FindNearestObject(p, g, out obj, out index);
            }
        }        

        protected int[] m_arPercRows = null; // [0] - ��� �������, [1] - ��� ������/����������, ��������� - ������ ����������� ������
        
        protected HPanelQuickData _pnlQuickData;

        protected System.Windows.Forms.SplitContainer stctrView;
        protected System.Windows.Forms.SplitContainer stctrViewPanel1, stctrViewPanel2;
        protected HZedGraphControl m_ZedGraphMins;
        protected HZedGraphControl m_ZedGraphHours;

        protected HDataGridViewBase m_dgwHours;
        protected HDataGridViewBase m_dgwMins;

        //private ManualResetEvent m_evTimerCurrent;
        private
            //System.Threading.Timer
            System.Windows.Forms.Timer
                m_timerCurrent
                ;
        private DelegateObjectFunc delegateTickTime;

        public TecView m_tecView;

        int currValuesPeriod;

        public int indx_TEC { get { return m_tecView.m_indx_TEC; } }
        public int indx_TECComponent { get { return m_tecView.indxTECComponents; } }
        public int m_ID { get { return m_tecView.m_ID; } }

        private bool update;

        protected virtual void InitializeComponent()
        {
            //this.m_pnlQuickData = new PanelQuickData(); ��������� � ������������

            createDataGridViewHours();
            createDataGridViewMins();

            ((System.ComponentModel.ISupportInitialize)(this.m_dgwHours)).BeginInit();
            if (!(this.m_dgwMins == null))
                ((System.ComponentModel.ISupportInitialize)(this.m_dgwMins)).BeginInit();
            else
                ;

            this._pnlQuickData.RestructControl();
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.Location = arPlacement[(int)CONTROLS.THIS].pt;
            this.Name = "pnlTecView";
            //this.Size = arPlacement[(int)CONTROLS.THIS].sz;
            this.TabIndex = 0;

            this._pnlQuickData.Dock = DockStyle.Fill;
            this._pnlQuickData.btnSetNow.Click += new System.EventHandler(this.btnSetNow_Click);
            this._pnlQuickData.dtprDate.ValueChanged += new System.EventHandler(this.dtprDate_ValueChanged);

            ((System.ComponentModel.ISupportInitialize)(this.m_dgwHours)).EndInit();
            if (!(this.m_dgwMins == null))
                ((System.ComponentModel.ISupportInitialize)(this.m_dgwMins)).EndInit();
            else
                ;

            createZedGraphControlMins(m_tecView.m_lockValue);
            createZedGraphControlHours(m_tecView.m_lockValue);
            this.m_ZedGraphHours.MouseUpEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.zedGraphHours_MouseUpEvent);

            this.stctrViewPanel1 = new System.Windows.Forms.SplitContainer();
            this.stctrViewPanel2 = new System.Windows.Forms.SplitContainer();

            this.stctrView = new System.Windows.Forms.SplitContainer();
            //this.stctrView.IsSplitterFixed = true;

            this._pnlQuickData.SuspendLayout();

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

            this._pnlQuickData.ResumeLayout(false);
            this._pnlQuickData.PerformLayout();

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

            if (!(m_label == null))
            {
                m_label.Text = m_tecView.m_tec.name_shr;
                if (!(indx_TECComponent < 0))
                    m_label.Text += @" - " + m_tecView.m_tec.list_TECComponents[indx_TECComponent].name_shr;
                else
                    ;

                m_label.EventRestruct += new DelegateObjectFunc(OnEventRestruct);
                //m_label.PerformRestruct (
            }
            else
                ;
        }

        protected abstract void createTecView(int indx_tec, int indx_comp);

        protected abstract void createDataGridViewHours();
        protected abstract void createDataGridViewMins();
        
        protected abstract void createZedGraphControlHours(object objLock);
        protected abstract void createZedGraphControlMins(object objLock);

        protected abstract void createPanelQuickData();

        public PanelTecViewBase(/*TecView.TYPE_PANEL type, */TEC tec, int indx_tec, int indx_comp, HMark markQueries)
        {
            //InitializeComponent();

            SPLITTER_PERCENT_VERTICAL = 50;

            createTecView(indx_tec, indx_comp); //m_tecView = new TecView(type, indx_tec, indx_comp);

            //HMark markQueries = new HMark(new int []{(int)CONN_SETT_TYPE.ADMIN, (int)CONN_SETT_TYPE.PBR, (int)CONN_SETT_TYPE.DATA_AISKUE, (int)CONN_SETT_TYPE.DATA_SOTIASSO});

            m_tecView.InitTEC(new List<StatisticCommon.TEC>() { tec }, markQueries);
            //m_tecView.SetDelegateReport(fErrRep, fWarRep, fActRep, fRepClr);

            m_tecView.setDatetimeView = new DelegateFunc(setNowDate);

            m_tecView.updateGUI_Fact = new IntDelegateIntIntFunc(updateGUI_Fact);
            m_tecView.updateGUI_TM_Gen = new DelegateFunc(updateGUI_TM_Gen);

            createPanelQuickData(); //������������ ����� 'InitializeComponent'
            if (m_tecView.listTG == null) //m_tecView.m_tec.m_bSensorsStrings == false
                m_tecView.m_tec.InitSensorsTEC();
            else
                ;

            AddTGView();

            if (tec.Type == TEC.TEC_TYPE.BIYSK)
                ; //this.parameters = FormMain.papar;
            else
                ;

            //??? �������� �.�. ���������:
            // 1) FormMain.formGraphicsSettings.m_connSettType_SourceData = 
            // 2) � ������������ � �. 1 ��������� �������� ������� ����
            // 3) � ������������ � �. 2 ��������� �������� m_tecView.m_arTypeSourceData[...
            //if (FormMain.formGraphicsSettings.m_connSettType_SourceData == CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE)
                //08.12.2014 - �������� �� ��������� - ��� � ������ ����
                m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] = 
                m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] = CONN_SETT_TYPE.DATA_AISKUE;
            //else
            //    m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES] =
            //        m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] = FormMain.formGraphicsSettings.m_connSettType_SourceData;

                if ((!(_pnlQuickData.ContextMenuStrip == null))
                    && (_pnlQuickData.ContextMenuStrip.Items.Count > 1))
                    m_tecView.m_bLastValue_TM_Gen = ((ToolStripMenuItem)_pnlQuickData.ContextMenuStrip.Items[1]).Checked;
                else
                    ;

            update = false;

            delegateTickTime = new DelegateObjectFunc(tickTime);
        }

        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            m_tecView.SetDelegateReport(ferr, fwar, fact, fclr);
        }

        public override void Start()
        {
            base.Start ();
            
            m_tecView.Start();
            // �������� �� ���������
            if (!(m_dgwMins == null))
                m_dgwMins.Fill();
            else
                ;
            m_dgwHours.Fill(m_tecView.m_curDate
                , m_tecView.m_valuesHours.Length
                , m_tecView.m_curDate.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0);

            DaylightTime daylight = TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year);
            TimeSpan timezone_offset = TimeSpan.FromHours (m_tecView.m_tec.m_timezone_offset_msc);
            timezone_offset = timezone_offset.Add(m_tecView.m_tsOffsetToMoscow);
            if (TimeZone.IsDaylightSavingTime(DateTime.Now, daylight))
                timezone_offset = timezone_offset.Add(TimeSpan.FromHours(1));
            else
                ;

            //����� �.�. ��� ???
            _pnlQuickData.dtprDate.Value = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).Add(timezone_offset);

            //initTableMinRows ();
            initTableHourRows ();            

            ////??? ������� � 'Activate'
            ////� ����������� �� ������������� ��������� � ����������� ����
            //// , ������������ ������� ���� ���������: 1-��, 2-�� �����
            //// , ���� ���������� ����, �� ����������� ���� ������
            //setTypeSourceData(HDateTime.INTERVAL.MINUTES, ((ToolStripMenuItem)m_ZedGraphMins.ContextMenuStrip.Items[m_ZedGraphMins.ContextMenuStrip.Items.Count - 2]).Checked == true ? CONN_SETT_TYPE.DATA_ASKUE : CONN_SETT_TYPE.DATA_SOTIASSO);
            //setTypeSourceData(HDateTime.INTERVAL.HOURS, ((ToolStripMenuItem)m_ZedGraphHours.ContextMenuStrip.Items[m_ZedGraphHours.ContextMenuStrip.Items.Count - 2]).Checked == true ? CONN_SETT_TYPE.DATA_ASKUE : CONN_SETT_TYPE.DATA_SOTIASSO);

            m_timerCurrent =
                //new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, 0, 1000)
                new System.Windows.Forms.Timer ()
                ;
            m_timerCurrent.Interval = 1000;
            m_timerCurrent.Tick += new EventHandler(TimerCurrent_Tick);
            m_timerCurrent.Start ();

            //??? TecView::Start
            update = false;
            //setNowDate(true); //??? ...�� ���������

            //??? ����������� �������� �� 'Activate (true)'
            //DrawGraphMins(0);
            //DrawGraphHours();
        }

        public override void Stop()
        {
            m_tecView.Stop ();

            //if (! (m_evTimerCurrent == null)) m_evTimerCurrent.Reset(); else ;
            if (!(m_timerCurrent == null)) { m_timerCurrent.Dispose(); m_timerCurrent = null; } else ;

            m_tecView.ReportClear(true);

            base.Stop();
        }

        protected override void initTableHourRows()
        {
            m_tecView.m_curDate = 
            m_tecView.serverTime = 
                _pnlQuickData.dtprDate.Value.Date;

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
            if ((m_tecView.m_arTypeSourceData [(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE)
                || (m_tecView.m_arTypeSourceData [(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_AISKUE_PLUS_SOTIASSO)
                || (m_tecView.m_arTypeSourceData [(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_3_MIN))
                m_dgwMins.InitRows (21, false);
            else
                if (m_tecView.m_arTypeSourceData [(int)HDateTime.INTERVAL.MINUTES] == CONN_SETT_TYPE.DATA_SOTIASSO_1_MIN)
                    m_dgwMins.InitRows (61, true);
                else
                    ;

            m_dgwMins.Fill ();
        }

        public virtual void AddTGView()
        {
            foreach (TG tg in m_tecView.listTG)
                _pnlQuickData.AddTGView(tg);
        }

        private int getHeightItem (bool bUseLabel, int iRow) { return bUseLabel == true ? m_arPercRows[iRow] : m_arPercRows[iRow] + m_arPercRows[iRow + 1]; }

        protected void OnEventRestruct(object pars)
        {
            int[] propView = pars as int[];

            this.Controls.Clear();
            this.RowStyles.Clear();
            stctrView.Panel1.Controls.Clear();
            stctrView.Panel2.Controls.Clear();
            this.stctrViewPanel1.Panel1.Controls.Clear();
            this.stctrViewPanel2.Panel1.Controls.Clear();

            int iRow = 0
                , iPercTotal = 100
                , iPercItem = -1;            
            bool bUseLabel = !(m_label == null);

            if (bUseLabel == true)
            {// ������ ��� ������� � ��������
                this.Controls.Add(m_label, 0, iRow);
                iPercItem = m_arPercRows[iRow];
                iPercTotal -= iPercItem;
                this.RowStyles.Add(new RowStyle(SizeType.Percent, m_arPercRows[iRow++]));
            }
            else
                ;
            //// ���������������� ������ � ������� ��� �������� � ���������� ��������
            //// , �� ~ �� ���� ������������ �� 'm_label'
            //iRow++;

            if (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.ORIENTATION] < 0)
            {
                //���������� ������ ���� �������
                bool bVisible = true;
                if (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_MINS] == 1)
                    this.Controls.Add(m_dgwMins, 0, iRow);
                else
                    if (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_HOURS] == 1)
                        this.Controls.Add(m_dgwHours, 0, iRow);
                    else
                        if (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_MINS] == 1)
                            this.Controls.Add(m_ZedGraphMins, 0, iRow);
                        else
                            if (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_HOURS] == 1)
                                this.Controls.Add(m_ZedGraphHours, 0, iRow);
                            else
                                bVisible = false;

                if (bVisible == true)
                {
                    iPercItem = getHeightItem (bUseLabel, iRow);
                    iPercTotal -= iPercItem;
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, iPercItem));
                    iRow++;
                }
                else
                    ;
            }
            else
            { //���������� ��� ��� ������ ��������
                if ((propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_MINS] == 1) && (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_HOURS] == 1) &&
                    (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_MINS] == 1) && (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_HOURS] == 1))
                { //���������� 4 �������� (�������(���) + �������(���) + ������(���) + ������(���))
                }
                else
                { //���������� ��� ��������
                    if (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.ORIENTATION] == 0)
                    {
                        stctrView.Orientation = Orientation.Vertical;

                        stctrView.SplitterDistance = stctrView.Width / (100 / (int)SPLITTER_PERCENT_VERTICAL);
                    }
                    else
                    {
                        stctrView.Orientation = Orientation.Horizontal;

                        stctrView.SplitterDistance = stctrView.Height / 2;
                    }

                    if ((propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_MINS] == 1) && (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_HOURS] == 1) &&
                        (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_MINS] == 0) && (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_HOURS] == 0))
                    { //���������� 2 �������� (�������(���) + �������(���))
                        stctrView.Panel1.Controls.Add(m_dgwMins);
                        stctrView.Panel2.Controls.Add(m_dgwHours);
                    }
                    else
                    {
                        if ((propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_MINS] == 0) && (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_HOURS] == 0) &&
                            (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_MINS] == 1) && (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_HOURS] == 1))
                        { //���������� 2 �������� (������(���) + ������(���))
                            stctrView.Panel1.Controls.Add(m_ZedGraphMins);
                            stctrView.Panel2.Controls.Add(m_ZedGraphHours);
                        }
                        else
                        {
                            if ((propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_MINS] == 1) && (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_HOURS] == 0) &&
                                (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_MINS] == 1) && (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_HOURS] == 0))
                            { //���������� 2 �������� (�������(���) + ������(���))
                                stctrView.Panel1.Controls.Add(m_dgwMins);
                                stctrView.Panel2.Controls.Add(m_ZedGraphMins);
                            }
                            else
                            {
                                if ((propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_MINS] == 0) && (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_HOURS] == 1) &&
                                    (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_MINS] == 0) && (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_HOURS] == 1))
                                { //���������� 2 �������� (�������(���) + ������(���))
                                    stctrView.Panel1.Controls.Add(m_dgwHours);
                                    stctrView.Panel2.Controls.Add(m_ZedGraphHours);
                                }
                                else
                                {
                                    if ((propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_MINS] == 0) && (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_HOURS] == 1) &&
                                        (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_MINS] == 1) && (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_HOURS] == 0))
                                    { //���������� 2 �������� (�������(���) + ������(���))
                                        stctrView.Panel1.Controls.Add(m_dgwHours);
                                        stctrView.Panel2.Controls.Add(m_ZedGraphMins);
                                    }
                                    else
                                    {
                                        if ((propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_MINS] == 1) && (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_HOURS] == 0) &&
                                            (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_MINS] == 0) && (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.GRAPH_HOURS] == 1))
                                        { //���������� 2 �������� (�������(���) + ������(���))
                                            stctrView.Panel1.Controls.Add(m_dgwMins);
                                            stctrView.Panel2.Controls.Add(m_ZedGraphHours);
                                        }
                                        else
                                        {
                                        }
                                    }
                                }
                            }
                        }
                    }

                    this.Controls.Add(this.stctrView, 0, iRow);
                    iPercItem = getHeightItem (bUseLabel, iRow);
                    iPercTotal -= iPercItem;
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, iPercItem));
                    iRow++;
                }

                switch (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.TABLE_AND_GRAPH])
                {
                    case -1: //������� � ������ � ������������ ����������� �� ����� ���� ��������� � ����� 'SplitContainer'
                        break;
                    case 0:
                        break;
                    case 1:
                        break;
                    default:
                        break;
                }
            }

            if (propView[(int)PanelCustomTecView.HLabelCustomTecView.INDEX_PROPERTIES_VIEW.QUICK_PANEL] == 1)
            {
                this.Controls.Add(_pnlQuickData, 0, iRow);
                _pnlQuickData.ShowFactValues();
                _pnlQuickData.ShowTMValues();
            }
            else
            {
            }

            this.RowStyles.Add(new RowStyle(SizeType.Percent, iPercTotal));
        }

        private void updateGUI_TM_Gen()
        {
            if (IsHandleCreated/*InvokeRequired*/ == true)
                this.BeginInvoke(new DelegateFunc(UpdateGUI_TM_Gen));
            else
                Logging.Logg().Error(@"PanelTecViewBase::updateGUI_TM_Gen () - ... BeginInvoke (UpdateGUI_TM_Gen) - ... ID = " + m_tecView.m_ID, Logging.INDEX_MESSAGE.D_001);
        }

        private void UpdateGUI_TM_Gen()
        {
            lock (m_tecView.m_lockValue)
            {
                _pnlQuickData.ShowTMValues();
            }
        }

        private int updateGUI_Fact(int hour, int min)
        {
            int iRes = (int)HClassLibrary.HHandler.INDEX_WAITHANDLE_REASON.SUCCESS;
            
            if (IsHandleCreated/*InvokeRequired*/ == true)
                this.BeginInvoke(new DelegateIntIntFunc(UpdateGUI_Fact), hour, min);
            else
                Logging.Logg().Error(@"PanelTecViewBase::updateGUI_Fact () - ... BeginInvoke (UpdateGUI_Fact) - ... ID = " + m_tecView.m_ID, Logging.INDEX_MESSAGE.D_001);

            return iRes;
        }

        protected virtual void UpdateGUI_Fact(int hour, int min)
        {
            lock (m_tecView.m_lockValue)
            {
                try
                {
                    FillGridHours();

                    FillGridMins(hour);

                    _pnlQuickData.ShowFactValues();

                    DrawGraphMins(hour);
                    DrawGraphHours();
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelTecViewBase::UpdateGUI_Fact () - ... ID = " + m_tecView.m_ID, Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
        }

        private void FillGridMins(int hour)
        {
            if (!(m_dgwMins == null))
                m_dgwMins.Fill(m_tecView.m_valuesMins
                    , hour, m_tecView.lastMin);
            else
                ;

            //Logging.Logg().Debug(@"PanelTecViewBase::FillGridMins () - ...");
        }

        private void FillGridHours()
        {
            // �������� �� ���������
            m_dgwHours.Fill(m_tecView.m_curDate
                , m_tecView.m_valuesHours.Length
                , m_tecView.m_curDate.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0);
            // �������� ��������
            m_dgwHours.Fill(m_tecView.m_valuesHours
                , m_tecView.lastHour
                , m_tecView.lastReceivedHour
                , m_tecView.m_valuesHours.Length
                , m_tecView.m_tec.m_id
                , m_tecView.currHour
                , m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS] == CONN_SETT_TYPE.DATA_AISKUE
                , m_tecView.serverTime);

            //Logging.Logg().Debug(@"PanelTecViewBase::FillGridHours () - ...");
        }

        protected void NewDateRefresh()
        {
            //Debug.WriteLine(@"PanelTecViewBase::NewDateRefresh () - m_tecView.currHour=" + m_tecView.currHour.ToString ());

            //delegateStartWait ();
            if (!(delegateStartWait == null)) delegateStartWait(); else ;
            
            //14.04.2015 ???
            if (m_tecView.currHour == true)
            {
                //// ��������� �-�
                //changeState();
                // ��������� �������� �� �-��
                m_tecView.m_curDate = _pnlQuickData.dtprDate.Value;
                m_tecView.ChangeState();
            }
            else
                m_tecView.GetRetroValues();

            //delegateStopWait ();
            if (!(delegateStopWait == null)) delegateStopWait(); else ;
        }

        private void dtprDate_ValueChanged(object sender, EventArgs e)
        {
            //Debug.WriteLine(@"PanelTecViewBase::dtprDate_ValueChanged () - DATE_pnlQuickData=" + _pnlQuickData.dtprDate.Value.ToString() + @", update=" + update);

            if (update == true)
            {
                //���������� ����/����� ????
                if (!(_pnlQuickData.dtprDate.Value.Date.CompareTo (m_tecView.m_curDate.Date) == 0))
                    m_tecView.currHour = false;
                else
                    ;

                //� ���� ������ ����/����� ������������ ???
                initTableHourRows ();

                NewDateRefresh();

                //setRetroTickTime(m_tecView.lastHour, (m_tecView.lastMin - 1) * m_tecView.GetIntervalOfTypeSourceData (HDateTime.INTERVAL.MINUTES));
                setRetroTickTime(m_tecView.lastHour, 60);
            }
            else
                update = true;
        }

        private void setNowDate()
        {
            //true, �.�. ������ ����� ��� result=true
            if (IsHandleCreated/*InvokeRequired*/ == true)
                this.BeginInvoke (new DelegateBoolFunc (setNowDate), true);
            else
                Logging.Logg().Error(@"PanelTecViewBase::setNowDate () - ... BeginInvoke (SetNowDate) - ...", Logging.INDEX_MESSAGE.D_001);
        }

        protected void setNowDate(bool received)
        {
            m_tecView.currHour = true;

            if (received == true)
            {
                update = false;
                _pnlQuickData.dtprDate.Value = m_tecView.m_curDate/*.Add(m_tecView.m_tsOffsetToMoscow)*/;
            }
            else
            {
                NewDateRefresh();
            }
        }

        private void btnSetNow_Click(object sender, EventArgs e)
        {
            ////������� �1
            //setNowDate(false);

            //������� �2
            m_tecView.currHour = true;
            NewDateRefresh();
        }

        //private void changeState()
        //{
        //    m_tecView.m_curDate = _pnlQuickData.dtprDate.Value;
            
        //    m_tecView.ChangeState ();
        //}

        protected bool zedGraphHours_MouseUpEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return true;

            object obj;
            PointF p = new PointF(e.X, e.Y);
            bool found;
            int index;

            found = m_ZedGraphHours.FindNearestObject(p, CreateGraphics(), out obj, out index);

            if ((found == true)
                && ((!(obj == null)) && (obj is CurveItem)))
            {
                if (((obj as CurveItem).IsBar == false) && ((obj as CurveItem).IsLine == false))
                    return true;

                if (!(m_tecView == null))
                {
                    if (!(delegateStartWait == null)) delegateStartWait(); else ;

                    bool bRetroHour = m_tecView.zedGraphHours_MouseUpEvent(index);

                    if (bRetroHour == true)
                        setRetroTickTime(m_tecView.lastHour, 60);
                    else
                    {
                        ////������� �1
                        //setNowDate(false);

                        //������� �2
                        m_tecView.currHour = true;
                        NewDateRefresh();
                    }

                    if (!(delegateStopWait == null)) delegateStopWait(); else ;
                }
            }
            else
                ;

            return true;
        }

        protected bool timerCurrentStarted
        {
            get { return ! (m_timerCurrent == null); }
        }

        public override bool Activate(bool active)
        {
            int err = 0;
            bool bRes = false;

            if ((timerCurrentStarted == true)
                && (!(Actived == active)))
            {
                bRes = base.Activate (active);

                if (Actived == true)
                {
                    currValuesPeriod = 0;

                    ////??? ������� � 'Activate'
                    ////??? ������� � 'enabledDataSource_...'
                    ////� ����������� �� ������������� ��������� � ����������� ����
                    //// , ������������ ������� ���� ���������: 1-��, 2-�� �����
                    //// , ���� ���������� ����, �� ����������� ���� ������
                    //setTypeSourceData(HDateTime.INTERVAL.MINUTES, ((ToolStripMenuItem)m_ZedGraphMins.ContextMenuStrip.Items[m_ZedGraphMins.ContextMenuStrip.Items.Count - 2]).Checked == true ? CONN_SETT_TYPE.DATA_ASKUE : CONN_SETT_TYPE.DATA_SOTIASSO);
                    //setTypeSourceData(HDateTime.INTERVAL.HOURS, ((ToolStripMenuItem)m_ZedGraphHours.ContextMenuStrip.Items[m_ZedGraphHours.ContextMenuStrip.Items.Count - 2]).Checked == true ? CONN_SETT_TYPE.DATA_ASKUE : CONN_SETT_TYPE.DATA_SOTIASSO);

                    HMark markSourceData = enabledSourceData_ToolStripMenuItems();

                    if (m_tecView.currHour == true)
                        NewDateRefresh();
                    else
                    {
                        updateGraphicsRetro(markSourceData);
                    }

                    _pnlQuickData.OnSizeChanged(null, EventArgs.Empty);

                    //m_timerCurrent.Change(0, 1000);
                    m_timerCurrent.Interval = 1000;
                    m_timerCurrent.Start ();
                }
                else
                {
                    m_tecView.ClearStates();

                    if (!(m_timerCurrent == null))
                        //m_timerCurrent.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        m_timerCurrent.Stop ();
                    else
                        ;
                }
            }
            else
            {
                err = -1; //???������

                Logging.Logg().Warning(@"PanelTecViewBase::Activate (" + active + @") - ... ID=" + m_ID + @", Started=" + Started + @", isActive=" + Actived, Logging.INDEX_MESSAGE.NOT_SET);
            }

            return bRes;
        }

        protected void setRetroTickTime(int hour, int min)
        {
            DateTime dt = _pnlQuickData.dtprDate.Value.Date;
            dt = dt.AddHours(hour);
            dt = dt.AddMinutes(min);

            if (IsHandleCreated == true)
                if (InvokeRequired == true)
                    Invoke(delegateTickTime, dt);
                else
                    tickTime(dt/*.Add(m_tecView.m_tsOffsetToMoscow)*/);
            else
                return;
        }

        /// <summary>
        /// ������� ���������� ���� '����� �������'
        /// </summary>
        /// <param name="dt">����/����� ��� �����������</param>
        private void tickTime(object dt)
        {
            _pnlQuickData.lblServerTime.Text = ((DateTime)dt).ToString("HH:mm:ss");
        }

        /// <summary>
        /// ����� ��������� ������ ������� 'timerCurrent'
        /// </summary>
        /// <param name="stateInfo">����� �������������</param>
        //private void TimerCurrent_Tick(Object stateInfo)
        private void TimerCurrent_Tick(object obj, EventArgs ev)
        {
            if ((m_tecView.currHour == true) && (Actived == true))
            {
                m_tecView.serverTime = m_tecView.serverTime.AddSeconds(1);

                if (IsHandleCreated == true)
                    if (InvokeRequired == true)
                        Invoke(delegateTickTime, m_tecView.serverTime);
                    else
                        tickTime(m_tecView.serverTime/*.Add(m_tecView.m_tsOffsetToMoscow)*/);
                else
                    return;

                //if (!(((currValuesPeriod++) * 1000) < Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME]) * 1000))
                if (!(currValuesPeriod++ < POOL_TIME * (m_tecView.m_idAISKUEParNumber == TecView.ID_AISKUE_PARNUMBER.FACT_03 ? 1 : 6)))
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
            //    Logging.Logg().Exception(e, "��������� � ���������� 'timerCurrent'", Logging.INDEX_MESSAGE.NOT_SET);
            //}
        }

        private void DrawGraphMins(int hour)
        {
            if (!(m_ZedGraphMins == null))
            {
                if (!(hour < m_tecView.m_valuesHours.Length))
                    hour = m_tecView.m_valuesHours.Length - 1;
                else
                    ;

                m_ZedGraphMins.Draw(m_tecView.m_valuesMins
                    , new object[] {
                        m_tecView.currHour
                        , m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.MINUTES]
                        , m_tecView.lastMin
                        , m_tecView.m_curDate.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0
                        , (IntDelegateIntFunc)m_tecView.GetSeasonHourOffset
                        , hour
                        , m_tecView.adminValuesReceived
                        , m_tecView.recomendation
                    }
                );
            }
            else
                ;
        }

        private void DrawGraphHours()
        {
            m_ZedGraphHours.Draw(m_tecView.m_valuesHours
                , new object [] {
                    m_tecView.currHour
                    , m_tecView.m_arTypeSourceData[(int)HDateTime.INTERVAL.HOURS]
                    , m_tecView.lastHour
                    , m_tecView.m_curDate.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0
                    , (IntDelegateIntFunc)m_tecView.GetSeasonHourOffset                    
                    , m_tecView.serverTime.Add (m_tecView.m_tsOffsetToMoscow)                    
                    , _pnlQuickData.dtprDate.Value.ToShortDateString()
                }
            );            
        }

        protected abstract HMark enabledSourceData_ToolStripMenuItems();        
        /// <summary>
        /// ���������� ����������� ������� � ��������� ��������� ��������� ������
        /// </summary>
        /// <param name="markUpdate">��������� �� ������������ ��������� ������</param>
        private void updateGraphicsRetro (HMark markUpdate)
        {
            //if (markUpdate.IsMarked() == false)
            //    return;
            //else
            if ((markUpdate.IsMarked((int)HDateTime.INTERVAL.MINUTES) == true) && (markUpdate.IsMarked((int)HDateTime.INTERVAL.HOURS) == false))
                //��������� ��������� ������ ������
                m_tecView.GetRetroMins();
            else
                if ((markUpdate.IsMarked((int)HDateTime.INTERVAL.MINUTES) == false) && (markUpdate.IsMarked((int)HDateTime.INTERVAL.HOURS) == true))
                    //��������� ��������� ������ ���
                    m_tecView.GetRetroHours();
                else
                    if ((markUpdate.IsMarked((int)HDateTime.INTERVAL.MINUTES) == true) && (markUpdate.IsMarked((int)HDateTime.INTERVAL.HOURS) == true))
                        //��������� ��������� ������ ���, ������
                        m_tecView.GetRetroValues();
                    else
                        ;
        }

        public void UpdateGraphicsCurrent(int type)
        {
            lock (m_tecView.m_lockValue)
            {
                //??? �������� 'type' TYPE_UPDATEGUI
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

        private void sourceData_Click(ContextMenuStrip cms, ToolStripMenuItem sender, HDateTime.INTERVAL indx_time)
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

                    if (indx_time == HDateTime.INTERVAL.MINUTES)
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
                        if (indx_time == HDateTime.INTERVAL.MINUTES)
                            m_tecView.GetRetroMins();
                        else
                            m_tecView.GetRetroHours();
                    }
                }
                else
                    ; //�� ������ �� ���� ���������� ����� ����������� ����
            }
            else
                ; //��������� ���

            //if (enabledSourceData_ToolStripMenuItems () == true) {
            //    NewDateRefresh ();
            //}
            //else
            //    ;
        }

        protected void sourceDataMins_Click(object sender, EventArgs e)
        {
            sourceData_Click(m_ZedGraphMins.ContextMenuStrip, (ToolStripMenuItem)sender, HDateTime.INTERVAL.MINUTES);
        }

        protected void sourceDataHours_Click(object sender, EventArgs e)
        {
            sourceData_Click(m_ZedGraphHours.ContextMenuStrip, (ToolStripMenuItem)sender, HDateTime.INTERVAL.HOURS);
        }
    }
}
