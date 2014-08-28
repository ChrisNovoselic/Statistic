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

using StatisticCommon;

namespace Statistic
{
    public class Hd2PercentControl
    {
        private double m_valuesBaseCalculate;

        public double valuesBaseCalculate { get { return m_valuesBaseCalculate; } }
        //public double difference { get { return m_valuesBaseCalculate; } }

        public Hd2PercentControl() { }

        public string Calculate(TecView.values values, int hour, bool bPmin, out int err)
        {
            string strRes = string.Empty;

            double dblRel = 0.0
                    , delta = -1.0
                    , dbl2AbsPercentControl = -1.0
                    ;
            int iReverse = 0;
            bool bAbs = false;

            if (values.valuesPBR[hour] == values.valuesPmax[hour])
            {
                m_valuesBaseCalculate = values.valuesPBR[hour];
                iReverse = 1;
            }
            else
            {
                //���������� "��"
                //if (values.valuesUDGe[hour] == values.valuesPBR[hour])
                if (!(values.valuesREC[hour] == 0))
                    values.valuesForeignCommand[hour] = false;
                else
                    ;

                if (values.valuesForeignCommand[hour] == true)
                {
                    m_valuesBaseCalculate = values.valuesUDGe[hour];
                    iReverse = 1;
                    bAbs = true;
                }
                else
                {
                    if (bPmin == true)
                        if (values.valuesPBR[hour] == values.valuesPmin[hour])
                        {
                            m_valuesBaseCalculate = values.valuesPBR[hour];
                            iReverse = -1;
                        }
                        else
                        {
                        }
                    else
                        ;
                }
            }

            if (m_valuesBaseCalculate > 1) {
                strRes += @"���=" + m_valuesBaseCalculate.ToString(@"F2");

                if (values.valuesLastMinutesTM[hour] > 1) {
                    if (!(iReverse == 0))
                    {
                        delta = iReverse * (m_valuesBaseCalculate - values.valuesLastMinutesTM[hour]);
                        if (bAbs == true)
                            delta = Math.Abs(delta);
                        else
                            ;
                    }
                    else
                        ;

                    dbl2AbsPercentControl = m_valuesBaseCalculate / 100 * 2;

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

                    strRes += @"; ����:" + (dbl2AbsPercentControl + dblRel).ToString(@"F1") + @"(" + (((dbl2AbsPercentControl + dblRel) / m_valuesBaseCalculate) * 100).ToString(@"F1") + @"%)";
                }
                else {
                    err = 0;

                    strRes += @";����:" + 0.ToString(@"F1") + @"(" + 0.ToString(@"F1") + @"%)";
                }
            }
            else {
                err = 0;

                strRes += StringToolTipEmpty;
            }           

            return strRes;
        }

        public static string StringToolTipEmpty = @"���:---;����:--(--%)";
    }

    public abstract class PanelTecViewBase : PanelStatisticView
    {
        protected static AdminTS.TYPE_FIELDS s_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;

        protected PanelQuickData m_pnlQuickData;

        protected System.Windows.Forms.SplitContainer stctrView;

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
        private ManualResetEvent evTimerCurrent;
        private System.Threading.Timer timerCurrent;
        //private System.Windows.Forms.Timer timerCurrent;
        private DelegateFunc delegateTickTime;

        //private AdminTS m_admin;
        //protected FormGraphicsSettings graphSettings;
        //protected FormParameters parameters;

        public volatile bool isActive;

        public TecView m_tecView;

        int currValuesPeriod;

        public int indx_TEC { get { return m_tecView.m_indx_TEC; } }
        public int indx_TECComponent { get { return m_tecView.indxTECComponents; } }

        //'public' ��� ������� �� ������� m_panelQuickData ������ 'PanelQuickData'
        //public TG[] sensorId2TG;        

        //public volatile TEC tec;

        //'public' ��� ������� �� ������� m_panelQuickData ������ 'PanelQuickData'
        //public List<TECComponentBase> m_list_TECComponents;

        private bool update;

        protected virtual void InitializeComponent()
        {
            //this.m_pnlQuickData = new PanelQuickData(); ��������� � ������������

            this.m_dgwHours = new DataGridViewHours();
            this.m_dgwMins = new DataGridViewMins();

            this.m_pnlQuickData.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)(this.m_dgwHours)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwMins)).BeginInit();

            this.SuspendLayout();

            this.RowCount = 2;
            this.RowStyles.Add(new RowStyle(SizeType.Percent, 86));
            this.RowStyles.Add(new RowStyle(SizeType.Percent, 14));

            this.Controls.Add(this.m_pnlQuickData, 0, 1);
            this.m_pnlQuickData.Initialize();
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.Location = arPlacement[(int)CONTROLS.THIS].pt;
            this.Name = "pnlTecView";
            //this.Size = arPlacement[(int)CONTROLS.THIS].sz;
            this.TabIndex = 0;

            this.m_pnlQuickData.Dock = DockStyle.Fill;
            this.m_pnlQuickData.btnSetNow.Click += new System.EventHandler(this.btnSetNow_Click);
            this.m_pnlQuickData.dtprDate.ValueChanged += new System.EventHandler(this.dtprDate_ValueChanged);

            ((System.ComponentModel.ISupportInitialize)(this.m_dgwHours)).EndInit();
            this.m_pnlQuickData.ResumeLayout(false);
            this.m_pnlQuickData.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwMins)).EndInit();

            //this.m_dgwMins.Rows.Add(21);
            //this.m_dgwHours.Rows.Add(25);

            this.ResumeLayout(false);

            this.stctrView = new System.Windows.Forms.SplitContainer();
        }

        public PanelTecViewBase(TEC tec, int indx_tec, int indx_comp, DelegateFunc fErrRep, DelegateFunc fActRep)
        {
            //InitializeComponent();

            m_tecView = new TecView(null, TecView.TYPE_PANEL.VIEW, indx_tec, indx_comp);
            m_tecView.InitTEC(new List<StatisticCommon.TEC>() { tec });
            m_tecView.SetDelegateReport(fErrRep, fActRep);

            m_tecView.setDatetimeView = new DelegateFunc(setNowDate);

            m_tecView.updateGUI_Fact = new DelegateIntIntFunc(updateGUI_Fact);
            m_tecView.updateGUI_TM_Gen = new DelegateFunc(updateGUI_TM_Gen);

            this.m_pnlQuickData = new PanelQuickData(); //������������ ����� 'InitializeComponent'
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

            delegateTickTime = new DelegateFunc(TickTime);
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
            this.m_dgwMins.Rows[20].Cells[0].Value = "����";
            this.m_dgwMins.Rows[20].Cells[1].Value = 0.ToString("F2");
            this.m_dgwMins.Rows[20].Cells[2].Value = "-";
            this.m_dgwMins.Rows[20].Cells[3].Value = "-";
            this.m_dgwMins.Rows[20].Cells[4].Value = 0.ToString("F2");
            this.m_dgwMins.Rows[20].Cells[5].Value = 0.ToString("F2");
        }

        private void FillDefaultHours()
        {
            int count;

            this.m_dgwHours.Rows.Clear();

            if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.SummerToWinter)
                count = 25;
            else
                if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.WinterToSummer)
                    count = 23;
                else
                    count = 24;

            this.m_dgwHours.Rows.Add(count + 1);

            for (int i = 0; i < count; i++)
            {
                if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.SummerToWinter)
                {
                    if (i <= m_tecView.m_valuesHours.hourAddon)
                        this.m_dgwHours.Rows[i].Cells[0].Value = (i + 1).ToString();
                    else
                        if (i == m_tecView.m_valuesHours.hourAddon + 1)
                            this.m_dgwHours.Rows[i].Cells[0].Value = i.ToString() + "*";
                        else
                            this.m_dgwHours.Rows[i].Cells[0].Value = i.ToString();
                }
                else
                    if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.WinterToSummer)
                    {
                        if (i < m_tecView.m_valuesHours.hourAddon)
                            this.m_dgwHours.Rows[i].Cells[0].Value = (i + 1).ToString();
                        else
                            this.m_dgwHours.Rows[i].Cells[0].Value = (i + 2).ToString();
                    }
                    else
                        this.m_dgwHours.Rows[i].Cells[0].Value = (i + 1).ToString();

                this.m_dgwHours.Rows[i].Cells[1].Value = 0.ToString("F2");
                this.m_dgwHours.Rows[i].Cells[2].Value = 0.ToString("F2");
                this.m_dgwHours.Rows[i].Cells[3].Value = 0.ToString("F2");
                this.m_dgwHours.Rows[i].Cells[4].Value = 0.ToString("F2");
                this.m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
            }

            this.m_dgwHours.Rows[count].Cells[0].Value = "�����";
            this.m_dgwHours.Rows[count].Cells[1].Value = 0.ToString("F2");
            this.m_dgwHours.Rows[count].Cells[2].Value = "-";
            this.m_dgwHours.Rows[count].Cells[3].Value = "-";
            this.m_dgwHours.Rows[count].Cells[4].Value = 0.ToString("F2");
            this.m_dgwHours.Rows[count].Cells[5].Value = 0.ToString("F2");
        }

        public override void Start()
        {            
            m_tecView.Start ();

            FillDefaultMins();
            FillDefaultHours();

            DaylightTime daylight = TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year);
            int timezone_offset = m_tecView.m_tec.m_timezone_offset_msc;
            if (TimeZone.IsDaylightSavingTime(DateTime.Now, daylight))
                timezone_offset++;
            else
                ;

            m_tecView.m_curDate = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(timezone_offset);
            m_tecView.serverTime = m_tecView.m_curDate;

            evTimerCurrent = new ManualResetEvent(true);
            timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), evTimerCurrent, 0, Timeout.Infinite);

            //timerCurrent = new System.Windows.Forms.Timer ();
            //timerCurrent.Tick += TimerCurrent_Tick;

            update = false;
            SetNowDate(true);
        }

        public override void Stop()
        {
            m_tecView.Stop ();

            if (! (evTimerCurrent == null)) evTimerCurrent.Reset(); else ;
            if (!(timerCurrent == null)) timerCurrent.Dispose(); else ;

            FormMainBaseWithStatusStrip.m_report.ClearStates ();
        }

        private void updateGUI_TM_Gen()
        {
            this.BeginInvoke(new DelegateFunc(UpdateGUI_TM_Gen));
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
            this.BeginInvoke(new DelegateIntIntFunc(UpdateGUI_Fact), hour, min);
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
                }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, @"PanelTecViewBase::UpdateGUI_Fact () - ...");
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

            for (int i = 0; i < m_tecView.m_valuesMins.valuesFact.Length - 1; i++)
            {
                m_dgwMins.Rows[i].Cells[1].Value = m_tecView.m_valuesMins.valuesFact[i + 1].ToString("F2");
                sumFact += m_tecView.m_valuesMins.valuesFact[i + 1];

                m_dgwMins.Rows[i].Cells[2].Value = m_tecView.m_valuesMins.valuesPBR[i].ToString("F2");
                m_dgwMins.Rows[i].Cells[3].Value = m_tecView.m_valuesMins.valuesPBRe[i].ToString("F2");
                m_dgwMins.Rows[i].Cells[4].Value = m_tecView.m_valuesMins.valuesUDGe[i].ToString("F2");
                sumUDGe += m_tecView.m_valuesMins.valuesUDGe[i];
                if (i < min && m_tecView.m_valuesMins.valuesUDGe[i] != 0)
                {
                    m_dgwMins.Rows[i].Cells[5].Value = ((double)(m_tecView.m_valuesMins.valuesFact[i + 1] - m_tecView.m_valuesMins.valuesUDGe[i])).ToString("F2");
                    //if (Math.Abs(m_tecView.m_valuesMins.valuesFact[i + 1] - m_tecView.m_valuesMins.valuesUDGe[i]) > m_tecView.m_valuesMins.valuesDiviation[i]
                    //    && m_tecView.m_valuesMins.valuesDiviation[i] != 0)
                    //    m_dgwMins.Rows[i].Cells[5].Style = dgvCellStyleError;
                    //else
                    m_dgwMins.Rows[i].Cells[5].Style = dgvCellStyleCommon;

                    sumDiviation += m_tecView.m_valuesMins.valuesFact[i + 1] - m_tecView.m_valuesMins.valuesUDGe[i];
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
                m_dgwMins.Rows[20].Cells[4].Value = m_tecView.m_valuesMins.valuesUDGe[0].ToString("F2");
                m_dgwMins.Rows[20].Cells[5].Value = (sumDiviation / min).ToString("F2");
            }

            setFirstDisplayedScrollingRowIndex(m_dgwMins, m_tecView.lastMin);

            Logging.Logg().LogDebugToFile(@"PanelTecViewBase::FillGridMins () - ...");
        }

        private void FillGridHours()
        {
            FillDefaultHours();

            double sumFact = 0, sumUDGe = 0, sumDiviation = 0;
            Hd2PercentControl d2PercentControl = new Hd2PercentControl();
            int hour = m_tecView.lastHour;
            int receivedHour = m_tecView.lastReceivedHour;
            int itemscount;
            int warn = -1,
                cntWarn = -1; ;
            string strWarn = string.Empty;

            if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.SummerToWinter)
            {
                itemscount = 25;
            }
            else
                if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.WinterToSummer)
                {
                    itemscount = 23;
                }
                else
                {
                    itemscount = 24;
                }

            cntWarn = 0;
            for (int i = 0; i < itemscount; i++)
            {
                bool bPmin = false;
                if (m_tecView.m_tec.m_id == 5) bPmin = true; else ;
                d2PercentControl.Calculate(m_tecView.m_valuesHours, i, bPmin, out warn);

                if ((!(warn == 0)) &&
                   (m_tecView.m_valuesHours.valuesLastMinutesTM[i] > 1))
                {
                    m_dgwHours.Rows[i].Cells[6].Style = dgvCellStyleError;
                    cntWarn++;
                }
                else
                {
                    m_dgwHours.Rows[i].Cells[6].Style = dgvCellStyleCommon;
                    cntWarn = 0;
                }

                if (m_tecView.m_valuesHours.valuesLastMinutesTM[i] > 1)
                {
                    if (cntWarn > 0)
                        strWarn = cntWarn + @":";
                    else
                        strWarn = string.Empty;

                    m_dgwHours.Rows[i].Cells[6].Value = strWarn + m_tecView.m_valuesHours.valuesLastMinutesTM[i].ToString("F2");
                }
                else
                    m_dgwHours.Rows[i].Cells[6].Value = 0.ToString("F2");

                if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.SummerToWinter)
                {
                    if (i <= m_tecView.m_valuesHours.hourAddon)
                    {
                        m_dgwHours.Rows[i].Cells[1].Value = m_tecView.m_valuesHours.valuesFact[i].ToString("F2");
                        sumFact += m_tecView.m_valuesHours.valuesFact[i];

                        m_dgwHours.Rows[i].Cells[2].Value = m_tecView.m_valuesHours.valuesPBR[i].ToString("F2");
                        m_dgwHours.Rows[i].Cells[3].Value = m_tecView.m_valuesHours.valuesPBRe[i].ToString("F2");
                        m_dgwHours.Rows[i].Cells[4].Value = m_tecView.m_valuesHours.valuesUDGe[i].ToString("F2");
                        sumUDGe += m_tecView.m_valuesHours.valuesUDGe[i];
                        if (i < receivedHour && m_tecView.m_valuesHours.valuesUDGe[i] != 0)
                        {
                            m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_tecView.m_valuesHours.valuesFact[i] - m_tecView.m_valuesHours.valuesUDGe[i])).ToString("F2");
                            if (Math.Abs(m_tecView.m_valuesHours.valuesFact[i] - m_tecView.m_valuesHours.valuesUDGe[i]) > m_tecView.m_valuesHours.valuesDiviation[i]
                                && m_tecView.m_valuesHours.valuesDiviation[i] != 0)
                                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                            else
                                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            sumDiviation += Math.Abs(m_tecView.m_valuesHours.valuesFact[i] - m_tecView.m_valuesHours.valuesUDGe[i]);
                        }
                        else
                        {
                            m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                            m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                        }
                    }
                    else
                        if (i == m_tecView.m_valuesHours.hourAddon + 1)
                        {
                            m_dgwHours.Rows[i].Cells[1].Value = m_tecView.m_valuesHours.valuesFactAddon.ToString("F2");
                            sumFact += m_tecView.m_valuesHours.valuesFactAddon;

                            m_dgwHours.Rows[i].Cells[2].Value = m_tecView.m_valuesHours.valuesPBRAddon.ToString("F2");
                            m_dgwHours.Rows[i].Cells[3].Value = m_tecView.m_valuesHours.valuesPBReAddon.ToString("F2");
                            m_dgwHours.Rows[i].Cells[4].Value = m_tecView.m_valuesHours.valuesUDGeAddon.ToString("F2");
                            sumUDGe += m_tecView.m_valuesHours.valuesUDGeAddon;
                            if (i <= receivedHour && m_tecView.m_valuesHours.valuesUDGeAddon != 0)
                            {
                                m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_tecView.m_valuesHours.valuesFactAddon - m_tecView.m_valuesHours.valuesUDGeAddon)).ToString("F2");
                                if (Math.Abs(m_tecView.m_valuesHours.valuesFactAddon - m_tecView.m_valuesHours.valuesUDGeAddon) > m_tecView.m_valuesHours.valuesDiviationAddon
                                    && m_tecView.m_valuesHours.valuesDiviationAddon != 0)
                                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                                else
                                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                                sumDiviation += Math.Abs(m_tecView.m_valuesHours.valuesFactAddon - m_tecView.m_valuesHours.valuesUDGeAddon);
                            }
                            else
                            {
                                m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            }
                        }
                        else
                        {
                            m_dgwHours.Rows[i].Cells[1].Value = m_tecView.m_valuesHours.valuesFact[i - 1].ToString("F2");
                            sumFact += m_tecView.m_valuesHours.valuesFact[i - 1];

                            m_dgwHours.Rows[i].Cells[2].Value = m_tecView.m_valuesHours.valuesPBR[i - 1].ToString("F2");
                            m_dgwHours.Rows[i].Cells[3].Value = m_tecView.m_valuesHours.valuesPBRe[i - 1].ToString("F2");
                            m_dgwHours.Rows[i].Cells[4].Value = m_tecView.m_valuesHours.valuesUDGe[i - 1].ToString("F2");
                            sumUDGe += m_tecView.m_valuesHours.valuesUDGe[i - 1];
                            if (i <= receivedHour && m_tecView.m_valuesHours.valuesUDGe[i - 1] != 0)
                            {
                                m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_tecView.m_valuesHours.valuesFact[i - 1] - m_tecView.m_valuesHours.valuesUDGe[i - 1])).ToString("F2");
                                if (Math.Abs(m_tecView.m_valuesHours.valuesFact[i - 1] - m_tecView.m_valuesHours.valuesUDGe[i - 1]) > m_tecView.m_valuesHours.valuesDiviation[i - 1]
                                    && m_tecView.m_valuesHours.valuesDiviation[i - 1] != 0)
                                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                                else
                                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                                sumDiviation += Math.Abs(m_tecView.m_valuesHours.valuesFact[i - 1] - m_tecView.m_valuesHours.valuesUDGe[i - 1]);
                            }
                            else
                            {
                                m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            }
                        }

                }
                else
                    if (m_tecView.m_valuesHours.season == TecView.seasonJumpE.WinterToSummer)
                    {
                        if (i < m_tecView.m_valuesHours.hourAddon)
                        {
                            m_dgwHours.Rows[i].Cells[1].Value = m_tecView.m_valuesHours.valuesFact[i].ToString("F2");
                            sumFact += m_tecView.m_valuesHours.valuesFact[i];

                            m_dgwHours.Rows[i].Cells[2].Value = m_tecView.m_valuesHours.valuesPBR[i].ToString("F2");
                            m_dgwHours.Rows[i].Cells[3].Value = m_tecView.m_valuesHours.valuesPBRe[i].ToString("F2");
                            m_dgwHours.Rows[i].Cells[4].Value = m_tecView.m_valuesHours.valuesUDGe[i].ToString("F2");
                            sumUDGe += m_tecView.m_valuesHours.valuesUDGe[i];
                            if (i < receivedHour && m_tecView.m_valuesHours.valuesUDGe[i] != 0)
                            {
                                m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_tecView.m_valuesHours.valuesFact[i] - m_tecView.m_valuesHours.valuesUDGe[i])).ToString("F2");
                                if (Math.Abs(m_tecView.m_valuesHours.valuesFact[i] - m_tecView.m_valuesHours.valuesUDGe[i]) > m_tecView.m_valuesHours.valuesDiviation[i]
                                    && m_tecView.m_valuesHours.valuesDiviation[i] != 0)
                                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                                else
                                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                                sumDiviation += Math.Abs(m_tecView.m_valuesHours.valuesFact[i] - m_tecView.m_valuesHours.valuesUDGe[i]);
                            }
                            else
                            {
                                m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            }
                        }
                        else
                        {
                            m_dgwHours.Rows[i].Cells[1].Value = m_tecView.m_valuesHours.valuesFact[i + 1].ToString("F2");
                            sumFact += m_tecView.m_valuesHours.valuesFact[i + 1];

                            m_dgwHours.Rows[i].Cells[2].Value = m_tecView.m_valuesHours.valuesPBR[i + 1].ToString("F2");
                            m_dgwHours.Rows[i].Cells[3].Value = m_tecView.m_valuesHours.valuesPBRe[i + 1].ToString("F2");
                            m_dgwHours.Rows[i].Cells[4].Value = m_tecView.m_valuesHours.valuesUDGe[i + 1].ToString("F2");
                            sumUDGe += m_tecView.m_valuesHours.valuesUDGe[i + 1];
                            if (i < receivedHour - 1 && m_tecView.m_valuesHours.valuesUDGe[i + 1] != 0)
                            {
                                m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_tecView.m_valuesHours.valuesFact[i + 1] - m_tecView.m_valuesHours.valuesUDGe[i + 1])).ToString("F2");
                                if (Math.Abs(m_tecView.m_valuesHours.valuesFact[i + 1] - m_tecView.m_valuesHours.valuesUDGe[i + 1]) > m_tecView.m_valuesHours.valuesDiviation[i + 1]
                                    && m_tecView.m_valuesHours.valuesDiviation[i + 1] != 0)
                                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                                else
                                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                                sumDiviation += Math.Abs(m_tecView.m_valuesHours.valuesFact[i + 1] - m_tecView.m_valuesHours.valuesUDGe[i + 1]);
                            }
                            else
                            {
                                m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            }
                        }
                    }
                    else
                    {
                        m_dgwHours.Rows[i].Cells[1].Value = m_tecView.m_valuesHours.valuesFact[i].ToString("F2");
                        sumFact += m_tecView.m_valuesHours.valuesFact[i];

                        m_dgwHours.Rows[i].Cells[2].Value = m_tecView.m_valuesHours.valuesPBR[i].ToString("F2");
                        m_dgwHours.Rows[i].Cells[3].Value = m_tecView.m_valuesHours.valuesPBRe[i].ToString("F2");
                        m_dgwHours.Rows[i].Cells[4].Value = m_tecView.m_valuesHours.valuesUDGe[i].ToString("F2");
                        sumUDGe += m_tecView.m_valuesHours.valuesUDGe[i];
                        if (i < receivedHour && m_tecView.m_valuesHours.valuesUDGe[i] != 0)
                        {
                            m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_tecView.m_valuesHours.valuesFact[i] - m_tecView.m_valuesHours.valuesUDGe[i])).ToString("F2");
                            if (Math.Abs(m_tecView.m_valuesHours.valuesFact[i] - m_tecView.m_valuesHours.valuesUDGe[i]) > m_tecView.m_valuesHours.valuesDiviation[i]
                                && m_tecView.m_valuesHours.valuesDiviation[i] != 0)
                                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                            else
                                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            sumDiviation += Math.Abs(m_tecView.m_valuesHours.valuesFact[i] - m_tecView.m_valuesHours.valuesUDGe[i]);
                        }
                        else
                        {
                            m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                            m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                        }
                    }
            }
            m_dgwHours.Rows[itemscount].Cells[1].Value = sumFact.ToString("F2");
            m_dgwHours.Rows[itemscount].Cells[4].Value = sumUDGe.ToString("F2");
            m_dgwHours.Rows[itemscount].Cells[5].Value = sumDiviation.ToString("F2");

            setFirstDisplayedScrollingRowIndex(m_dgwHours, m_tecView.lastHour);

            Logging.Logg().LogDebugToFile(@"PanelTecViewBase::FillGridHours () - ...");
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
                if (!(m_pnlQuickData.dtprDate.Value.Date == m_tecView.m_curDate.Date))
                    m_tecView.currHour = false;
                else
                    ;

                NewDateRefresh();
            }
            else
                update = true;
        }

        private void setNowDate()
        {
            //true, �.�. ������ ����� ��� result=true
            this.BeginInvoke (new DelegateBoolFunc (SetNowDate), true);
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

        public override void Activate(bool active)
        {
            isActive = active;

            if (isActive == true)
            {
                currValuesPeriod = 0;
                
                ChangeState ();
            }
            else
            {
                m_tecView.ClearStates ();
            }
        }

        private void ShowValues(string caption)
        {
            /*MessageBox.Show(this, "state = " + state + "\naction = " + action + "\ndate = " + dtprDate.Value.ToString() +
                            "\nnow_date = " + DateTime.Now.ToString() + "\ngmt0 = " + System.TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).ToString() +
                            "\nselectedTime = " + selectedTime.ToString() + "lastHour = " + lastHour + "\nlastMin = " + lastMin + "\ncurrHour = " + currHour + 
                            "\nadminValuesReceived = " + adminValuesReceived, caption, MessageBoxButtons.OK);*/

            MessageBox.Show(this, "", caption, MessageBoxButtons.OK);
        }

        private void TickTime()
        {
            m_tecView.serverTime = m_tecView.serverTime.AddSeconds(1);
            m_pnlQuickData.lblServerTime.Text = m_tecView.serverTime.ToString("HH:mm:ss");
        }

        private void TimerCurrent_Tick(Object stateInfo)
        {
            Invoke(delegateTickTime);
            if ((m_tecView.currHour == true) && (isActive == true))
                if (!(((currValuesPeriod++) * 1000) < Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME])))
                {
                    currValuesPeriod = 0;
                    NewDateRefresh();
                }
                else
                    ;
            else
                ;

            ((ManualResetEvent)stateInfo).WaitOne();
            try
            {
                timerCurrent.Change(1000, Timeout.Infinite);
            }
            catch (Exception e)
            {
                Logging.Logg().LogExceptionToFile(e, "��������� � ���������� 'timerCurrent'");
            }
        }
    }
}
