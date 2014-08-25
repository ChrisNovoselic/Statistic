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

        public string Calculate(PanelTecViewBase.values values, int hour, bool bPmin, out int err)
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
                //Вычисление "ВК"
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

            if ((values.valuesLastMinutesTM[hour] > 1) && (valuesBaseCalculate > 1))
                dblRel = delta - dbl2AbsPercentControl;
            else
                ;

            if ((dblRel > 0) && (!(iReverse == 0)))
                err = 1;
            else
                err = 0;

            //strToolTip += @"УДГэ=" + dblUDGe.ToString (@"F2");
            strRes += @"ПБР=" + m_valuesBaseCalculate.ToString(@"F2");
            strRes += @";Откл:" + (dbl2AbsPercentControl + dblRel).ToString(@"F1");

            return strRes;
        }
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

        protected DelegateBoolFunc delegateSetNowDate;

        private DelegateIntIntFunc delegateUpdateGUI_Fact;
        private DelegateFunc delegateUpdateGUI_TM;

        protected object m_lockValue;

        private Thread taskThread;
        protected Semaphore m_sem;
        private volatile bool threadIsWorking;
        protected volatile bool m_newState;
        protected volatile List<StatesMachine> m_states;
        private int currValuesPeriod = 0;
        private ManualResetEvent evTimerCurrent;
        private System.Threading.Timer timerCurrent;
        //private System.Windows.Forms.Timer timerCurrent;
        private DelegateFunc delegateTickTime;

        //private AdminTS m_admin;
        protected FormGraphicsSettings graphSettings;
        protected FormParameters parameters;

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public volatile bool currHour;
        public volatile int lastHour;
        private volatile int lastReceivedHour;
        public volatile int lastMin;
        public volatile bool lastMinError;
        public volatile bool lastHourError;
        public volatile bool lastHourHalfError;
        public volatile string lastLayout;

        protected enum StatesMachine
        {
            Init,
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

        public abstract class values
        {
            public volatile double[] valuesLastMinutesTM;

            public volatile double[] valuesPBR;
            public volatile bool[] valuesForeignCommand;
            public volatile double[] valuesPmin;
            public volatile double[] valuesPmax;
            public volatile double[] valuesPBRe;
            public volatile double[] valuesUDGe;

            public volatile double[] valuesDiviation; //Значение в ед.изм.

            public volatile double[] valuesREC;

            public values(int sz)
            {
                valuesLastMinutesTM = new double[sz];

                valuesPBR = new double[sz];
                valuesForeignCommand = new bool[sz];
                valuesPmin = new double[sz];
                valuesPmax = new double[sz];
                valuesPBRe = new double[sz];
                valuesUDGe = new double[sz];
                valuesDiviation = new double[sz];

                valuesREC = new double[sz];
            }
        }

        public class valuesTECComponent : values
        {
            //public volatile double[] valuesREC;
            public volatile double[] valuesISPER; //Признак ед.изм. 'valuesDIV'
            public volatile double[] valuesDIV; //Значение из БД

            public valuesTECComponent(int sz)
                : base(sz)
            {
                //valuesREC = new double[sz];
                valuesISPER = new double[sz];
                valuesDIV = new double[sz];
            }
        }

        public class valuesTEC : values
        {
            public volatile double[] valuesFact;

            public double valuesFactAddon;
            public double valuesPBRAddon;
            public double valuesPBReAddon;
            public double valuesUDGeAddon;
            public double valuesDiviationAddon;
            public int hourAddon;
            public seasonJumpE season;
            public bool addonValues;

            public valuesTEC(int sz)
                : base(sz)
            {
                valuesFact = new double[sz];

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

        private bool update;

        public volatile bool isActive;

        private bool started;

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public volatile bool adminValuesReceived;

        //'public' для доступа из объекта m_panelQuickData класса 'PanelQuickData'
        public volatile bool recalcAver;

        protected virtual void InitializeComponent()
        {
            this.m_pnlQuickData = new PanelQuickData();

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

        public PanelTecViewBase(TEC tec, int num_tec, int num_comp, StatusStrip sts, FormGraphicsSettings gs, FormParameters par, HReports rep)
        {
            this.tec = tec;
            this.num_TEC = num_tec;
            this.num_TECComponent = num_comp;
            m_report = rep;

            //InitializeComponent();

            started = false;

            dgvCellStyleError = new DataGridViewCellStyle();
            dgvCellStyleError.BackColor = Color.Red;
            dgvCellStyleCommon = new DataGridViewCellStyle();

            //this.m_admin = admin;
            this.graphSettings = gs;
            this.parameters = par;

            if (tec.type() == TEC.TEC_TYPE.BIYSK)
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

            m_lockValue = new object();

            m_valuesMins = new valuesTEC(21);
            m_valuesHours = new valuesTEC(24);

            stsStrip = sts;

            delegateSetNowDate = new DelegateBoolFunc(SetNowDate);

            delegateUpdateGUI_Fact = new DelegateIntIntFunc(UpdateGUI_Fact);
            delegateUpdateGUI_TM = new DelegateFunc(UpdateGUI_TM);

            delegateTickTime = new DelegateFunc(TickTime);

            m_states = new List<StatesMachine>();
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
            int count;

            this.m_dgwHours.Rows.Clear();

            if (m_valuesHours.season == seasonJumpE.SummerToWinter)
                count = 25;
            else
                if (m_valuesHours.season == seasonJumpE.WinterToSummer)
                    count = 23;
                else
                    count = 24;

            this.m_dgwHours.Rows.Add(count + 1);

            for (int i = 0; i < count; i++)
            {
                if (m_valuesHours.season == seasonJumpE.SummerToWinter)
                {
                    if (i <= m_valuesHours.hourAddon)
                        this.m_dgwHours.Rows[i].Cells[0].Value = (i + 1).ToString();
                    else
                        if (i == m_valuesHours.hourAddon + 1)
                            this.m_dgwHours.Rows[i].Cells[0].Value = i.ToString() + "*";
                        else
                            this.m_dgwHours.Rows[i].Cells[0].Value = i.ToString();
                }
                else
                    if (m_valuesHours.season == seasonJumpE.WinterToSummer)
                    {
                        if (i < m_valuesHours.hourAddon)
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

            this.m_dgwHours.Rows[count].Cells[0].Value = "Сумма";
            this.m_dgwHours.Rows[count].Cells[1].Value = 0.ToString("F2");
            this.m_dgwHours.Rows[count].Cells[2].Value = "-";
            this.m_dgwHours.Rows[count].Cells[3].Value = "-";
            this.m_dgwHours.Rows[count].Cells[4].Value = 0.ToString("F2");
            this.m_dgwHours.Rows[count].Cells[5].Value = 0.ToString("F2");
        }

        public override void Start()
        {
            if (started == true)
                return;
            else
                ;

            adminValuesReceived = false;
            currHour = true;
            currValuesPeriod = Int32.Parse(parameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME]) / 1000 - 1;
            started = true;

            tec.StartDbInterfaces(CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE);
            threadIsWorking = true;

            taskThread = new Thread(new ParameterizedThreadStart(TecView_ThreadFunction));
            taskThread.Name = "Интерфейс к данным";
            taskThread.IsBackground = true;

            m_sem = new Semaphore(1, 1);

            m_sem.WaitOne();
            taskThread.Start();

            ClearValues();

            FillDefaultMins();
            FillDefaultHours();

            DaylightTime daylight = TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year);
            int timezone_offset = tec.m_timezone_offset_msc;
            if (TimeZone.IsDaylightSavingTime(DateTime.Now, daylight))
                timezone_offset++;
            else
                ;

            selectedTime = TimeZone.CurrentTimeZone.ToUniversalTime(DateTime.Now).AddHours(timezone_offset);

            serverTime = selectedTime;

            evTimerCurrent = new ManualResetEvent(true);
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

            evTimerCurrent.Reset();
            timerCurrent.Dispose();

            started = false;
            bool joined;
            threadIsWorking = false;
            lock (m_lockValue)
            {
                m_newState = true;
                m_states.Clear();
            }

            if (taskThread.IsAlive)
            {
                try
                {
                    m_sem.Release(1);
                }
                catch (Exception excpt) { Logging.Logg().LogExceptionToFile(excpt, "catch - PanelTecViewBase.Stop () - m_sem.Release(1)"); }

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
            lock (m_lockValue)
            {
                m_pnlQuickData.ShowTMValues();
            }
        }

        protected virtual void UpdateGUI_Fact(int hour, int min)
        {
            lock (m_lockValue)
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

        private void GetCurrentTimeRequest()
        {
            string query = string.Empty;
            DbInterface.DB_TSQL_INTERFACE_TYPE typeDB = DbTSQLInterface.getTypeDB(tec.connSetts[(int)CONN_SETT_TYPE.DATA_ASKUE].port);

            switch (typeDB)
            {
                case DbInterface.DB_TSQL_INTERFACE_TYPE.MySQL:
                    query = @"SELECT now()";
                    break;
                case DbInterface.DB_TSQL_INTERFACE_TYPE.MSSQL:
                    query = @"SELECT GETDATE()";
                    break;
                default:
                    break;
            }

            if (query.Equals(string.Empty) == false)
                tec.Request(CONN_SETT_TYPE.DATA_ASKUE, query);
            else
                ;
        }

        //private void GetSensorsFactRequest()
        //{
        //    tec.Request(CONN_SETT_TYPE.CONFIG_DB, tec.sensorsFactRequest());
        //}

        //private void GetSensorsTMRequest()
        //{
        //    tec.Request(CONN_SETT_TYPE.CONFIG_DB, tec.sensorsTMRequest());
        //}

        private void GetHoursRequest(DateTime date)
        {
            tec.Request(CONN_SETT_TYPE.DATA_ASKUE, tec.hoursRequest(date, sensorsStrings_Fact[(int)TG.ID_TIME.HOURS]));
        }

        private void GetMinsRequest(int hour)
        {
            tec.Request(CONN_SETT_TYPE.DATA_ASKUE, tec.minsRequest(selectedTime, hour, sensorsStrings_Fact[(int)TG.ID_TIME.MINUTES]));
        }

        private void GetCurrentTMRequest()
        {
            tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, tec.currentTMRequest(sensorsString_TM));
        }

        private void GetLastMinutesTMRequest(DateTime dtReq)
        {
            tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, tec.lastMinutesTMRequest(dtReq.Date, sensorsString_TM));
        }

        private void GetPBRValuesRequest()
        {
            //m_admin.Request(tec.m_arIdListeners[(int)CONN_SETT_TYPE.PBR], tec.GetPBRValueQuery(num_TECComponent, m_pnlQuickData.dtprDate.Value.Date, m_admin.m_typeFields));
            tec.Request(CONN_SETT_TYPE.PBR, tec.GetPBRValueQuery(num_TECComponent, m_pnlQuickData.dtprDate.Value.Date, s_typeFields));
        }

        private void GetAdminValuesRequest(AdminTS.TYPE_FIELDS mode)
        {
            //m_admin.Request(tec.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], tec.GetAdminValueQuery(num_TECComponent, m_pnlQuickData.dtprDate.Value.Date, mode));
            tec.Request(CONN_SETT_TYPE.ADMIN, tec.GetAdminValueQuery(num_TECComponent, m_pnlQuickData.dtprDate.Value.Date, mode));
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
            int min = lastMin;

            if (min != 0)
                min--;
            else
                ;

            for (int i = 0; i < m_valuesMins.valuesFact.Length - 1; i++)
            {
                m_dgwMins.Rows[i].Cells[1].Value = m_valuesMins.valuesFact[i + 1].ToString("F2");
                sumFact += m_valuesMins.valuesFact[i + 1];

                m_dgwMins.Rows[i].Cells[2].Value = m_valuesMins.valuesPBR[i].ToString("F2");
                m_dgwMins.Rows[i].Cells[3].Value = m_valuesMins.valuesPBRe[i].ToString("F2");
                m_dgwMins.Rows[i].Cells[4].Value = m_valuesMins.valuesUDGe[i].ToString("F2");
                sumUDGe += m_valuesMins.valuesUDGe[i];
                if (i < min && m_valuesMins.valuesUDGe[i] != 0)
                {
                    m_dgwMins.Rows[i].Cells[5].Value = ((double)(m_valuesMins.valuesFact[i + 1] - m_valuesMins.valuesUDGe[i])).ToString("F2");
                    //if (Math.Abs(m_valuesMins.valuesFact[i + 1] - m_valuesMins.valuesUDGe[i]) > m_valuesMins.valuesDiviation[i]
                    //    && m_valuesMins.valuesDiviation[i] != 0)
                    //    m_dgwMins.Rows[i].Cells[5].Style = dgvCellStyleError;
                    //else
                    m_dgwMins.Rows[i].Cells[5].Style = dgvCellStyleCommon;

                    sumDiviation += m_valuesMins.valuesFact[i + 1] - m_valuesMins.valuesUDGe[i];
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
                m_dgwMins.Rows[20].Cells[4].Value = m_valuesMins.valuesUDGe[0].ToString("F2");
                m_dgwMins.Rows[20].Cells[5].Value = (sumDiviation / min).ToString("F2");
            }

            setFirstDisplayedScrollingRowIndex(m_dgwMins, lastMin);

            Logging.Logg().LogDebugToFile(@"PanelTecViewBase::FillGridMins () - ...");
        }

        private void FillGridHours()
        {
            FillDefaultHours();

            double sumFact = 0, sumUDGe = 0, sumDiviation = 0;
            Hd2PercentControl d2PercentControl = new Hd2PercentControl();
            int hour = lastHour;
            int receivedHour = lastReceivedHour;
            int itemscount;
            int warn = -1,
                cntWarn = -1; ;
            string strWarn = string.Empty;

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

            cntWarn = 0;
            for (int i = 0; i < itemscount; i++)
            {
                bool bPmin = false;
                if (tec.m_id == 5) bPmin = true; else ;
                d2PercentControl.Calculate(m_valuesHours, i, bPmin, out warn);

                if ((!(warn == 0)) &&
                   (m_valuesHours.valuesLastMinutesTM[i] > 1))
                {
                    m_dgwHours.Rows[i].Cells[6].Style = dgvCellStyleError;
                    cntWarn++;
                }
                else
                {
                    m_dgwHours.Rows[i].Cells[6].Style = dgvCellStyleCommon;
                    cntWarn = 0;
                }

                if (m_valuesHours.valuesLastMinutesTM[i] > 1)
                {
                    if (cntWarn > 0)
                        strWarn = cntWarn + @":";
                    else
                        strWarn = string.Empty;

                    m_dgwHours.Rows[i].Cells[6].Value = strWarn + m_valuesHours.valuesLastMinutesTM[i].ToString("F2");
                }
                else
                    m_dgwHours.Rows[i].Cells[6].Value = 0.ToString("F2");

                if (m_valuesHours.season == seasonJumpE.SummerToWinter)
                {
                    if (i <= m_valuesHours.hourAddon)
                    {
                        m_dgwHours.Rows[i].Cells[1].Value = m_valuesHours.valuesFact[i].ToString("F2");
                        sumFact += m_valuesHours.valuesFact[i];

                        m_dgwHours.Rows[i].Cells[2].Value = m_valuesHours.valuesPBR[i].ToString("F2");
                        m_dgwHours.Rows[i].Cells[3].Value = m_valuesHours.valuesPBRe[i].ToString("F2");
                        m_dgwHours.Rows[i].Cells[4].Value = m_valuesHours.valuesUDGe[i].ToString("F2");
                        sumUDGe += m_valuesHours.valuesUDGe[i];
                        if (i < receivedHour && m_valuesHours.valuesUDGe[i] != 0)
                        {
                            m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i])).ToString("F2");
                            if (Math.Abs(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i]) > m_valuesHours.valuesDiviation[i]
                                && m_valuesHours.valuesDiviation[i] != 0)
                                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                            else
                                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            sumDiviation += Math.Abs(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i]);
                        }
                        else
                        {
                            m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                            m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                        }
                    }
                    else
                        if (i == m_valuesHours.hourAddon + 1)
                        {
                            m_dgwHours.Rows[i].Cells[1].Value = m_valuesHours.valuesFactAddon.ToString("F2");
                            sumFact += m_valuesHours.valuesFactAddon;

                            m_dgwHours.Rows[i].Cells[2].Value = m_valuesHours.valuesPBRAddon.ToString("F2");
                            m_dgwHours.Rows[i].Cells[3].Value = m_valuesHours.valuesPBReAddon.ToString("F2");
                            m_dgwHours.Rows[i].Cells[4].Value = m_valuesHours.valuesUDGeAddon.ToString("F2");
                            sumUDGe += m_valuesHours.valuesUDGeAddon;
                            if (i <= receivedHour && m_valuesHours.valuesUDGeAddon != 0)
                            {
                                m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_valuesHours.valuesFactAddon - m_valuesHours.valuesUDGeAddon)).ToString("F2");
                                if (Math.Abs(m_valuesHours.valuesFactAddon - m_valuesHours.valuesUDGeAddon) > m_valuesHours.valuesDiviationAddon
                                    && m_valuesHours.valuesDiviationAddon != 0)
                                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                                else
                                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                                sumDiviation += Math.Abs(m_valuesHours.valuesFactAddon - m_valuesHours.valuesUDGeAddon);
                            }
                            else
                            {
                                m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            }
                        }
                        else
                        {
                            m_dgwHours.Rows[i].Cells[1].Value = m_valuesHours.valuesFact[i - 1].ToString("F2");
                            sumFact += m_valuesHours.valuesFact[i - 1];

                            m_dgwHours.Rows[i].Cells[2].Value = m_valuesHours.valuesPBR[i - 1].ToString("F2");
                            m_dgwHours.Rows[i].Cells[3].Value = m_valuesHours.valuesPBRe[i - 1].ToString("F2");
                            m_dgwHours.Rows[i].Cells[4].Value = m_valuesHours.valuesUDGe[i - 1].ToString("F2");
                            sumUDGe += m_valuesHours.valuesUDGe[i - 1];
                            if (i <= receivedHour && m_valuesHours.valuesUDGe[i - 1] != 0)
                            {
                                m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_valuesHours.valuesFact[i - 1] - m_valuesHours.valuesUDGe[i - 1])).ToString("F2");
                                if (Math.Abs(m_valuesHours.valuesFact[i - 1] - m_valuesHours.valuesUDGe[i - 1]) > m_valuesHours.valuesDiviation[i - 1]
                                    && m_valuesHours.valuesDiviation[i - 1] != 0)
                                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                                else
                                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                                sumDiviation += Math.Abs(m_valuesHours.valuesFact[i - 1] - m_valuesHours.valuesUDGe[i - 1]);
                            }
                            else
                            {
                                m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            }
                        }

                }
                else
                    if (m_valuesHours.season == seasonJumpE.WinterToSummer)
                    {
                        if (i < m_valuesHours.hourAddon)
                        {
                            m_dgwHours.Rows[i].Cells[1].Value = m_valuesHours.valuesFact[i].ToString("F2");
                            sumFact += m_valuesHours.valuesFact[i];

                            m_dgwHours.Rows[i].Cells[2].Value = m_valuesHours.valuesPBR[i].ToString("F2");
                            m_dgwHours.Rows[i].Cells[3].Value = m_valuesHours.valuesPBRe[i].ToString("F2");
                            m_dgwHours.Rows[i].Cells[4].Value = m_valuesHours.valuesUDGe[i].ToString("F2");
                            sumUDGe += m_valuesHours.valuesUDGe[i];
                            if (i < receivedHour && m_valuesHours.valuesUDGe[i] != 0)
                            {
                                m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i])).ToString("F2");
                                if (Math.Abs(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i]) > m_valuesHours.valuesDiviation[i]
                                    && m_valuesHours.valuesDiviation[i] != 0)
                                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                                else
                                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                                sumDiviation += Math.Abs(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i]);
                            }
                            else
                            {
                                m_dgwHours.Rows[i].Cells[5].Value = 0.ToString("F2");
                                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            }
                        }
                        else
                        {
                            m_dgwHours.Rows[i].Cells[1].Value = m_valuesHours.valuesFact[i + 1].ToString("F2");
                            sumFact += m_valuesHours.valuesFact[i + 1];

                            m_dgwHours.Rows[i].Cells[2].Value = m_valuesHours.valuesPBR[i + 1].ToString("F2");
                            m_dgwHours.Rows[i].Cells[3].Value = m_valuesHours.valuesPBRe[i + 1].ToString("F2");
                            m_dgwHours.Rows[i].Cells[4].Value = m_valuesHours.valuesUDGe[i + 1].ToString("F2");
                            sumUDGe += m_valuesHours.valuesUDGe[i + 1];
                            if (i < receivedHour - 1 && m_valuesHours.valuesUDGe[i + 1] != 0)
                            {
                                m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_valuesHours.valuesFact[i + 1] - m_valuesHours.valuesUDGe[i + 1])).ToString("F2");
                                if (Math.Abs(m_valuesHours.valuesFact[i + 1] - m_valuesHours.valuesUDGe[i + 1]) > m_valuesHours.valuesDiviation[i + 1]
                                    && m_valuesHours.valuesDiviation[i + 1] != 0)
                                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                                else
                                    m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                                sumDiviation += Math.Abs(m_valuesHours.valuesFact[i + 1] - m_valuesHours.valuesUDGe[i + 1]);
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
                        m_dgwHours.Rows[i].Cells[1].Value = m_valuesHours.valuesFact[i].ToString("F2");
                        sumFact += m_valuesHours.valuesFact[i];

                        m_dgwHours.Rows[i].Cells[2].Value = m_valuesHours.valuesPBR[i].ToString("F2");
                        m_dgwHours.Rows[i].Cells[3].Value = m_valuesHours.valuesPBRe[i].ToString("F2");
                        m_dgwHours.Rows[i].Cells[4].Value = m_valuesHours.valuesUDGe[i].ToString("F2");
                        sumUDGe += m_valuesHours.valuesUDGe[i];
                        if (i < receivedHour && m_valuesHours.valuesUDGe[i] != 0)
                        {
                            m_dgwHours.Rows[i].Cells[5].Value = ((double)(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i])).ToString("F2");
                            if (Math.Abs(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i]) > m_valuesHours.valuesDiviation[i]
                                && m_valuesHours.valuesDiviation[i] != 0)
                                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleError;
                            else
                                m_dgwHours.Rows[i].Cells[5].Style = dgvCellStyleCommon;
                            sumDiviation += Math.Abs(m_valuesHours.valuesFact[i] - m_valuesHours.valuesUDGe[i]);
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

            setFirstDisplayedScrollingRowIndex(m_dgwHours, lastHour);

            Logging.Logg().LogDebugToFile(@"PanelTecViewBase::FillGridHours () - ...");
        }

        protected void ClearValues()
        {
            ClearValuesMins();
            ClearValuesHours();
        }

        protected void ClearValuesMins()
        {
            for (int i = 0; i < 21; i++)
                m_valuesMins.valuesFact[i] =
                m_valuesMins.valuesDiviation[i] =
                m_valuesMins.valuesPBR[i] =
                m_valuesMins.valuesPBRe[i] =
                m_valuesMins.valuesUDGe[i] = 0;
        }

        protected void ClearValuesHours()
        {
            for (int i = 0; i < 24; i++)
            {
                m_valuesHours.valuesFact[i] =
                m_valuesHours.valuesLastMinutesTM[i] =
                m_valuesHours.valuesDiviation[i] =
                m_valuesHours.valuesPBR[i] =
                m_valuesHours.valuesPmin[i] =
                m_valuesHours.valuesPmax[i] =
                m_valuesHours.valuesPBRe[i] =
                m_valuesHours.valuesUDGe[i] = 0;

                m_valuesHours.valuesForeignCommand[i] = true;
            }

            m_valuesHours.valuesFactAddon =
            m_valuesHours.valuesDiviationAddon =
            m_valuesHours.valuesPBRAddon =
            m_valuesHours.valuesPBReAddon =
            m_valuesHours.valuesUDGeAddon = 0;
            m_valuesHours.season = seasonJumpE.None;
            m_valuesHours.hourAddon = 0;
            m_valuesHours.addonValues = false;
        }

        private void ClearPBRValues()
        {
        }

        private void ClearAdminValues()
        {
            for (int i = 0; i < 24; i++)
            {
                m_valuesHours.valuesDiviation[i] =
                m_valuesHours.valuesPBR[i] =
                m_valuesHours.valuesPmin[i] =
                m_valuesHours.valuesPmax[i] =
                m_valuesHours.valuesPBRe[i] =
                m_valuesHours.valuesUDGe[i] = 0;

                m_valuesHours.valuesForeignCommand[i] = true;
            }

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
                    Logging.Logg().LogExceptionToFile(excpt, "PanelTecViewBase::GetCurrentTimeReponse () - (DateTime)table.Rows[0][0]");

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
                try
                {
                    //if (!DateTime.TryParse(table.Rows[0][6].ToString(), out dt))
                    if (DateTime.TryParse(table.Rows[0][@"DATA_DATE"].ToString(), out dt) == false)
                        return false;

                    //if (!int.TryParse(table.Rows[0][8].ToString(), out season))
                    if (int.TryParse(table.Rows[0][@"SEASON"].ToString(), out season) == false)
                        return false;
                }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, @"PanelTecViewBase::GetHoursResponse () - ...");

                    dt = DateTime.Now.Date;
                }

                GetSeason(dt, season, out season);
                prev_season = season;
                hour = dt.Hour;
                dtNeeded = dt;

                if (dt.Minute == 0)
                    half++;
                else
                    ;
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

                    try
                    {
                        if (DateTime.TryParse(table.Rows[i][@"DATA_DATE"].ToString(), out dt) == false)
                            return false;

                        if (int.TryParse(table.Rows[i][@"SEASON"].ToString(), out season) == false)
                            return false;
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().LogExceptionToFile(e, @"PanelTecViewBase::GetHoursResponse () - ...");

                        dt = DateTime.Now.Date;
                    }

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
                    if (table.Columns.Contains(@"ID") == true)
                        if (int.TryParse(table.Rows[i][@"ID"].ToString(), out id) == false)
                            return false;
                        else
                            ;
                    else
                        return false;

                    tgTmp = FindTGById(id, TG.INDEX_VALUE.FACT, TG.ID_TIME.HOURS);
                    if (tgTmp == null)
                        return false;
                    else
                        ;

                    //if (!double.TryParse(table.Rows[i][5].ToString(), out value))
                    if (table.Columns.Contains(@"VALUE0") == true)
                        if (double.TryParse(table.Rows[i][@"VALUE0"].ToString(), out value) == false)
                            return false;
                        else
                            ;
                    else
                        return false;

                    switch (tec.type())
                    {
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

                if (!(table.Rows[i]["value"] is DBNull))
                    if (double.TryParse(table.Rows[i]["value"].ToString(), out value) == false)
                        return false;
                    else
                        ;
                else
                    value = 0.0;

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

        private bool GetLastMinutesTMResponse(DataTable table_in, DateTime dtReq)
        {
            bool bRes = true;
            int i = -1,
                hour = -1,
                offsetUTC = (int)HAdmin.GetUTCOffsetOfCurrentTimeZone().TotalHours;
            double value = -1;
            DateTime dtVal = DateTime.Now;
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
                            if (!(tgRows[i]["value"] is DBNull))
                                if (double.TryParse(tgRows[i]["value"].ToString(), out value) == false)
                                    return false;
                                else
                                    ;
                            else
                                value = 0.0;

                            if ((!(value < 1)) && (DateTime.TryParse(tgRows[i]["last_changed_at"].ToString(), out dtVal) == false))
                                return false;
                            else
                                ;

                            hour = dtVal.Hour + offsetUTC + 1; //Т.к. мин.59 из прошедшего часа
                            if (!(hour < 24)) hour -= 24; else ;

                            tg.power_LastMinutesTM[hour] = value;

                            //Запрос с учетом значения перехода через сутки
                            if (hour > 0 && value > 1)
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
                        if (tgRows[i] == null)
                            continue;
                        else
                            ;

                        try
                        {
                            if (double.TryParse(tgRows[i]["value"].ToString(), out value) == false)
                                return false;
                            else
                                ;

                            if (DateTime.TryParse(tgRows[i]["last_changed_at"].ToString(), out dtVal) == false)
                                return false;
                            else
                                ;
                        }
                        catch (Exception e)
                        {
                            Logging.Logg().LogExceptionToFile(e, @"PanelTecViewBase::GetLastMinutesTMResponse () - ...");

                            dtVal = DateTime.Now.Date;
                        }

                        hour = dtVal.Hour + offsetUTC + 1;
                        //if (!(hour < 24))
                        if (hour > 24)
                            hour -= 24;
                        else ;

                        //if (dtReq.Date.Equals (dtVal.Date) == true) {
                        tg.power_LastMinutesTM[hour] = value;

                        if (hour > 0 && value > 1)
                            m_valuesHours.valuesLastMinutesTM[hour - 1] += value;
                        else
                            ;
                        //} else ;
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
                if (table.Columns.Contains(@"DATA_DATE") == true)
                    if (DateTime.TryParse(table.Rows[0][@"DATA_DATE"].ToString(), out dt) == false)
                        return false;
                    else
                        ;
                else
                    return false;

                if (table.Columns.Contains(@"SEASON") == true)
                    if (int.TryParse(table.Rows[0][@"SEASON"].ToString(), out season) == false)
                        return false;
                    else
                        ;
                else
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
                if (!int.TryParse(table.Rows[i][@"SEASON"].ToString(), out season))
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

                    try
                    {
                        if (!DateTime.TryParse(table.Rows[i][@"DATA_DATE"].ToString(), out dt))
                            return false;
                        if (!int.TryParse(table.Rows[i][@"SEASON"].ToString(), out season))
                            return false;
                    }
                    catch (Exception e)
                    {
                        Logging.Logg().LogExceptionToFile(e, @"PanelTecViewBase::GetLastMinutesTMResponse () - ...");

                        dt = DateTime.Now.Date;
                    }

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

                    if (!int.TryParse(table.Rows[i][@"ID"].ToString(), out id))
                        return false;

                    tgTmp = FindTGById(id, TG.INDEX_VALUE.FACT, (int)TG.ID_TIME.MINUTES);

                    if (tgTmp == null)
                        return false;

                    if (!double.TryParse(table.Rows[i][@"VALUE0"].ToString(), out value))
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
                //lastMin = ((selectedTime.Minute - 1) / 3) + 1;
            }

            return true;
        }

        private int LayotByName(string l)
        {
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

            int num1 = LayotByName(l1),
                num2 = LayotByName(l2);

            if (num2 > num1)
                bRes = true;
            else
                ;

            return bRes;
        }

        private bool GetPBRValuesResponse(DataTable table)
        {
            bool bRes = true;

            if (!(table == null))
                m_tablePBRResponse = table.Copy();
            else
                ;

            return bRes;
        }

        public static DataTable restruct_table_pbrValues(DataTable table_in, List<TECComponent> listTECComp, int num_comp)
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

                if (num_comp < 0)
                {
                    list_TECComponents = new List<TECComponent>();
                    for (i = 0; i < listTECComp.Count; i++)
                    {
                        if ((listTECComp[i].m_id > 100) && (listTECComp[i].m_id < 500))
                            list_TECComponents.Add(listTECComp[i]);
                        else
                            ;
                    }
                }
                else
                    list_TG = listTECComp[num_comp].TG;

                //Преобразование таблицы
                for (i = 0; i < table_in.Columns.Count; i++)
                {
                    if ((!(table_in.Columns[i].ColumnName.Equals("ID_COMPONENT") == true))
                        && (!(table_in.Columns[i].ColumnName.Equals(nameFieldDate) == true))
                        //&& (!(table_in.Columns[i].ColumnName.Equals (tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]) == true))
                        && (!(table_in.Columns[i].ColumnName.Equals(@"PBR_NUMBER") == true))
                    )
                    //if (!(table_in.Columns[i].ColumnName == "ID_COMPONENT"))
                    {
                        cols_data.Add(table_in.Columns[i]);
                    }
                    else
                        if ((table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)
                            //|| (table_in.Columns[i].ColumnName.Equals (tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]) == true)
                            || (table_in.Columns[i].ColumnName.Equals(@"PBR_NUMBER") == true)
                        )
                        {
                            table_in_restruct.Columns.Add(table_in.Columns[i].ColumnName, table_in.Columns[i].DataType);
                        }
                        else
                            ;
                }

                if (num_comp < 0)
                    count_comp = list_TECComponents.Count;
                else
                    count_comp = list_TG.Count;

                for (i = 0; i < count_comp; i++)
                {
                    for (j = 0; j < cols_data.Count; j++)
                    {
                        table_in_restruct.Columns.Add(cols_data[j].ColumnName, cols_data[j].DataType);
                        if (num_comp < 0)
                            table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TECComponents[i].m_id;
                        else
                            table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TG[i].m_id;
                    }
                }

                //if (tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                //table_in_restruct.Columns[tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]].SetOrdinal(table_in_restruct.Columns.Count - 1);
                table_in_restruct.Columns[@"PBR_NUMBER"].SetOrdinal(table_in_restruct.Columns.Count - 1);
                //else
                //    ;

                List<DataRow[]> listDataRows = new List<DataRow[]>();

                for (i = 0; i < count_comp; i++)
                {
                    if (num_comp < 0)
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
                            //if (tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                            //table_in_restruct.Rows[indx_row][tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]] = listDataRows[i][j][tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.PBR_NUMBER]];
                            table_in_restruct.Rows[indx_row][@"PBR_NUMBER"] = listDataRows[i][j][@"PBR_NUMBER"];
                            //else
                            //    ;
                        }
                        else
                            indx_row = k;

                        for (k = 0; k < cols_data.Count; k++)
                        {
                            if (num_comp < 0)
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

        public static DataTable restruct_table_adminValues(DataTable table_in, List<TECComponent> listTECComp, int num_comp)
        {
            DataTable table_in_restruct = new DataTable();
            List<DataColumn> cols_data = new List<DataColumn>();
            DataRow[] dataRows;
            int i = -1, j = -1, k = -1;
            string nameFieldDate = "DATE_ADMIN"; // tec.m_strNamesField[(int)TEC.INDEX_NAME_FIELD.ADMIN_DATETIME]

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

                if (num_comp < 0)
                {
                    list_TECComponents = new List<TECComponent>();
                    for (i = 0; i < listTECComp.Count; i++)
                    {
                        if ((listTECComp[i].m_id > 100) && (listTECComp[i].m_id < 500))
                            list_TECComponents.Add(listTECComp[i]);
                        else
                            ;
                    }
                }
                else
                    list_TG = listTECComp[num_comp].TG;

                //Преобразование таблицы
                for (i = 0; i < table_in.Columns.Count; i++)
                {
                    if ((!(table_in.Columns[i].ColumnName == "ID_COMPONENT")) && (!(table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)))
                    //if (!(table_in.Columns[i].ColumnName == "ID_COMPONENT"))
                    {
                        cols_data.Add(table_in.Columns[i]);
                    }
                    else
                        if (table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)
                        {
                            table_in_restruct.Columns.Add(table_in.Columns[i].ColumnName, table_in.Columns[i].DataType);
                        }
                        else
                            ;
                }

                if (num_comp < 0)
                    count_comp = list_TECComponents.Count;
                else
                    count_comp = list_TG.Count;

                for (i = 0; i < count_comp; i++)
                {
                    for (j = 0; j < cols_data.Count; j++)
                    {
                        table_in_restruct.Columns.Add(cols_data[j].ColumnName, cols_data[j].DataType);
                        if (num_comp < 0)
                            table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TECComponents[i].m_id;
                        else
                            table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TG[i].m_id;
                    }
                }

                List<DataRow[]> listDataRows = new List<DataRow[]>();

                for (i = 0; i < count_comp; i++)
                {
                    if (num_comp < 0)
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
                        }
                        else
                            indx_row = k;

                        for (k = 0; k < cols_data.Count; k++)
                        {
                            if (num_comp < 0)
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
            DateTime date = m_pnlQuickData.dtprDate.Value.Date
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
                double[,] valuesPmin = new double[m_list_TECComponents.Count, 25];
                double[,] valuesPmax = new double[m_list_TECComponents.Count, 25];
                double[,] valuesREC = new double[m_list_TECComponents.Count, 25];
                int[,] valuesISPER = new int[m_list_TECComponents.Count, 25];
                double[,] valuesDIV = new double[m_list_TECComponents.Count, 25];

                offsetUDG = 1;
                offsetPlan = /*offsetUDG + 3 * tec.list_TECComponents.Count +*/ 1; //ID_COMPONENT
                offsetLayout = -1;

                m_tablePBRResponse = restruct_table_pbrValues(m_tablePBRResponse, tec.list_TECComponents, num_TECComponent);
                offsetLayout = (!(m_tablePBRResponse.Columns.IndexOf("PBR_NUMBER") < 0)) ? (offsetPlan + m_list_TECComponents.Count * 3) : m_tablePBRResponse.Columns.Count;

                table_in = restruct_table_adminValues(table_in, tec.list_TECComponents, num_TECComponent);

                //if (!(table_in.Columns.IndexOf("ID_COMPONENT") < 0))
                //    try { table_in.Columns.Remove("ID_COMPONENT"); }
                //    catch (Exception excpt)
                //    {
                //        /*
                //        Logging.Logg().LogExceptionToFile(excpt, "catch - PanelTecViewBase.GetAdminValuesResponse () - ...");
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
                                for (j = 0; j < m_list_TECComponents.Count; j++)
                                {
                                    if ((offsetPlan + j * 3) < m_tablePBRResponse.Columns.Count)
                                    {
                                        valuesPBR[j, 24] = (double)m_tablePBRResponse.Rows[i][offsetPlan + j * 3];
                                        valuesPmin[j, 24] = (double)m_tablePBRResponse.Rows[i][offsetPlan + j * 3 + 1];
                                        valuesPmax[j, 24] = (double)m_tablePBRResponse.Rows[i][offsetPlan + j * 3 + 2];
                                    }
                                    else
                                    {
                                        valuesPBR[j, 24] = 0.0;
                                        valuesPmin[j, 24] = 0.0;
                                        valuesPmax[j, 24] = 0.0;
                                    }
                                    //j++;
                                }
                            }
                            else
                                ;
                        }
                        catch (Exception excpt) { Logging.Logg().LogExceptionToFile(excpt, "catch - PanelTecViewBase.GetAdminValuesResponse () - ..."); }
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
                                    {
                                        valuesPBR[j, hour - 1] = (double)m_tablePBRResponse.Rows[i][offsetPlan + (j * 3)];
                                        valuesPmin[j, hour - 1] = (double)m_tablePBRResponse.Rows[i][offsetPlan + (j * 3) + 1];
                                        valuesPmax[j, hour - 1] = (double)m_tablePBRResponse.Rows[i][offsetPlan + (j * 3) + 2];
                                    }
                                    else
                                    {
                                        valuesPBR[j, hour - 1] = 0.0;
                                    }

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
                                catch (Exception e)
                                {
                                    Logging.Logg().LogExceptionToFile(e, @"PanelTecViewBase::GetAdminValuesResponse () - ...");
                                }
                                //j++;
                            }

                            string tmp = "";
                            //if ((m_tablePBRResponse.Columns.Contains ("PBR_NUMBER")) && !(m_tablePBRResponse.Rows[i][offsetLayout] is System.DBNull))
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
                            Logging.Logg().LogExceptionToFile(e, @"PanelTecViewBase::GetAdminValuesResponse () - ...");
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
                        m_valuesHours.valuesPmin[i] += valuesPmin[j, i];
                        m_valuesHours.valuesPmax[i] += valuesPmax[j, i];
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

                        m_valuesHours.valuesREC[i] += valuesREC[j, i];

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
                double[] valuesPmin = new double[25];
                double[] valuesPmax = new double[25];
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
                                valuesPmin[24] = (double)m_tablePBRResponse.Rows[i][offsetPlan + 1];
                                valuesPmax[24] = (double)m_tablePBRResponse.Rows[i][offsetPlan + 2];
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
                            {
                                valuesPBR[hour - 1] = (double)m_tablePBRResponse.Rows[i][offsetPlan];
                                valuesPmin[hour - 1] = (double)m_tablePBRResponse.Rows[i][offsetPlan + 1];
                                valuesPmax[hour - 1] = (double)m_tablePBRResponse.Rows[i][offsetPlan + 2];
                            }
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

                                if (!(row_in[0][offsetUDG] is System.DBNull))
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
                            Logging.Logg().LogExceptionToFile(e, "PanelTecViewBase::GetAdminValueResponse ()...");
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
                    m_valuesHours.valuesPmin[i] = valuesPmin[i];
                    m_valuesHours.valuesPmax[i] = valuesPmax[i];
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
            //delegateStartWait ();
            if (!(delegateStartWait == null)) delegateStartWait(); else ;
            lock (m_lockValue)
            {
                ChangeState();

                try
                {
                    m_sem.Release(1);
                }
                catch
                {
                }

            }
            //delegateStopWait ();
            if (!(delegateStopWait == null)) delegateStopWait(); else ;
        }

        private void dtprDate_ValueChanged(object sender, EventArgs e)
        {
            if (update)
            {
                if (!(m_pnlQuickData.dtprDate.Value.Date == selectedTime.Date))
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
                m_pnlQuickData.dtprDate.Value = selectedTime;
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
            m_newState = true;
            m_states.Clear();

            if ((sensorsString_TM.Equals(string.Empty) == false) ||
                ((sensorsStrings_Fact[(int)TG.ID_TIME.MINUTES].Equals(string.Empty) == false) && (sensorsStrings_Fact[(int)TG.ID_TIME.HOURS].Equals(string.Empty) == false)))
            {
                if (currHour == true)
                {
                    m_states.Add(StatesMachine.CurrentTime);
                }
                else
                {
                    selectedTime = m_pnlQuickData.dtprDate.Value.Date;
                }
            }
            else
            {
                m_states.Add(StatesMachine.Init);
                m_states.Add(StatesMachine.CurrentTime);
            }

            m_states.Add(StatesMachine.CurrentHours_Fact);
            m_states.Add(StatesMachine.CurrentMins_Fact);
            m_states.Add(StatesMachine.Current_TM);
            m_states.Add(StatesMachine.LastMinutes_TM);
            m_states.Add(StatesMachine.PBRValues);
            m_states.Add(StatesMachine.AdminValues);
        }

        public override void Activate(bool active)
        {
            isActive = active;

            if (isActive == true)
            {
                currValuesPeriod = 0;
                lock (m_lockValue)
                {
                    ChangeState();

                    try
                    {
                        m_sem.Release(1);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                lock (m_lockValue)
                {
                    m_newState = true;
                    m_states.Clear();
                    m_report.errored_state =
                    m_report.actioned_state = false;
                }
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

        object[] generateValues(DateTime dt, int indx_tg, int indx_halfhours, int indx_id_time, int season)
        {
            //Согласно структуры запросов 'GetHoursRequest' и 'GetMinsRequest'
            int indx_season = 2; //8
            object[] resValues = new object[9];

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
                        table.Rows.Add(generateValues(date, j, i, (int)TG.ID_TIME.HOURS, 0));
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
            //Logging.Logg().LogDebugToFile(@"PanelTecViewBase::StateRequest () - name=" + Parent.Text + @", state=" + state.ToString() + @" - вХод...");

            switch (state)
            {
                case StatesMachine.Init:
                    ActionReport("Получение идентификаторов датчиков.");
                    //switch (tec.type())
                    //{
                    //    case TEC.TEC_TYPE.COMMON:

                    //        break;
                    //    case TEC.TEC_TYPE.BIYSK:
                    //        GetSensors();
                    //        break;
                    //    default:
                    //        break;
                    //}
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
                    GetLastMinutesTMRequest(selectedTime);
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

            Logging.Logg().LogDebugToFile(@"PanelTecViewBase::StateRequest () - name=" + Parent.Text + @", state=" + state.ToString() + @" - вЫход...");
        }

        private bool StateCheckResponse(StatesMachine state, out bool error, out DataTable table)
        {
            error = false;
            table = null;

            switch (state)
            {
                case StatesMachine.Init:
                    return true;
                case StatesMachine.CurrentTime:
                case StatesMachine.CurrentHours_Fact:
                case StatesMachine.CurrentMins_Fact:
                case StatesMachine.RetroHours:
                case StatesMachine.RetroMins:
                    return tec.Response(CONN_SETT_TYPE.DATA_ASKUE, out error, out table);
                case StatesMachine.Current_TM:
                case StatesMachine.LastMinutes_TM:
                    return tec.Response(CONN_SETT_TYPE.DATA_SOTIASSO, out error, out table);
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
            //Logging.Logg().LogDebugToFile(@"PanelTecViewBase::StateResponse () - name=" + Parent.Text + @", state=" + state.ToString() + @" - вХод...");

            bool result = false;
            switch (state)
            {
                case StatesMachine.Init:
                    result = GetSensors();
                    break;
                case StatesMachine.CurrentTime:
                    result = GetCurrentTimeReponse(table);
                    if (result == true)
                    {
                        //this.BeginInvoke(delegateShowValues, "StatesMachine.CurrentTime");
                        selectedTime = selectedTime.AddSeconds(-1 * Int32.Parse(parameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ERROR_DELAY]));
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
                    result = GetLastMinutesTMResponse(table, selectedTime);
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

            Logging.Logg().LogDebugToFile(@"PanelTecViewBase::StateResponse () - name=" + Parent.Text + @", state=" + state.ToString() + @", result=" + result.ToString() + @" - вЫход...");

            return result;
        }

        private void StateErrors(StatesMachine state, bool response)
        {
            string reason = string.Empty,
                    waiting = string.Empty,
                    msg = string.Empty;

            switch (state)
            {
                case StatesMachine.Init:
                    reason = @"идентификаторов датчиков (факт., телемеханика)";
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
                msg = "Ошибка " + reason + ". " + waiting + ".";
            else
                msg = "Ошибка " + reason + ".";

            ErrorReport(msg);

            Logging.Logg().LogDebugToFile(@"PanelTecViewBase::StateErrors () - name=" + Parent.Text + @", state=" + state.ToString() + @", error=" + msg + @" - вЫход...");
        }

        private void TecView_ThreadFunction(object data)
        {
            int index;
            StatesMachine currentState;

            while (threadIsWorking)
            {
                m_sem.WaitOne();

                index = 0;

                lock (m_lockValue)
                {
                    if (m_states.Count == 0)
                        continue;
                    else
                        ;

                    currentState = m_states[index];
                    m_newState = false;
                }

                while (true)
                {
                    bool error = true;
                    bool dataPresent = false;
                    DataTable table = null;
                    for (int i = 0; i < DbInterface.MAX_RETRY && !dataPresent && !m_newState; i++)
                    {
                        if (error)
                            StateRequest(currentState);
                        else
                            ;

                        error = false;
                        for (int j = 0; j < DbInterface.MAX_WAIT_COUNT && !dataPresent && !error && !m_newState; j++)
                        {
                            System.Threading.Thread.Sleep(DbInterface.WAIT_TIME_MS);
                            dataPresent = StateCheckResponse(currentState, out error, out table);
                        }
                    }

                    bool responseIsOk = true;
                    if (dataPresent && !error && !m_newState)
                        responseIsOk = StateResponse(currentState, table);
                    else
                        ;

                    if ((!responseIsOk || !dataPresent || error) && !m_newState)
                    {
                        StateErrors(currentState, !responseIsOk);
                        lock (m_lockValue)
                        {
                            if (!m_newState)
                            {
                                m_states.Clear();
                                m_newState = true;
                            }
                            else
                                ;
                        }
                    }

                    index++;

                    lock (m_lockValue)
                    {
                        if (index == m_states.Count)
                            break;
                        else
                            ;

                        if (m_newState == true)
                            break;
                        else
                            ;

                        currentState = m_states[index];
                    }
                }
            }
            try
            {
                m_sem.Release(1);
            }
            catch
            {
            }
        }

        private void TickTime()
        {
            serverTime = serverTime.AddSeconds(1);
            m_pnlQuickData.lblServerTime.Text = serverTime.ToString("HH:mm:ss");
        }

        private void TimerCurrent_Tick(Object stateInfo)
        {
            Invoke(delegateTickTime);
            if (currHour && isActive)
                if (!(((currValuesPeriod++) * 1000) < Int32.Parse(parameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME])))
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
                Logging.Logg().LogExceptionToFile(e, "Обращение к переменной 'timerCurrent'");
            }
        }
    }
}
