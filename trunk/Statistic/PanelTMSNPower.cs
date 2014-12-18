using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Data;

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    partial class PanelTMSNPower
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        #endregion
    }

    public partial class PanelTMSNPower : PanelStatisticView
    {
        enum INDEX_LABEL : int
        {
            NAME,
            DATETIME_TM_Gen,
            DATETIME_TM_SN,
            VALUE_TM_Gen,
            VALUE_TM_SN,
            COUNT_INDEX_LABEL
        };
        const int COUNT_FIXED_ROWS = (int)INDEX_LABEL.VALUE_TM_SN - 1;

        static Color s_clrBackColorLabel = Color.FromArgb(212, 208, 200), s_clrBackColorLabelVal_TM = Color.FromArgb(219, 223, 227), s_clrBackColorLabelVal_TM_SN = Color.FromArgb(219, 223, 247);
        static HLabelStyles[] s_arLabelStyles = { new HLabelStyles(Color.Black, s_clrBackColorLabel, 22F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM, 18F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM_SN, 18F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM, 18F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBackColorLabelVal_TM_SN, 18F, ContentAlignment.MiddleCenter)};

        enum StatesMachine : int { Init_TM, Current_TM_Gen, Current_TM_SN };

        public int m_msecPeriodUpdate;

        //HReports m_report;

        public bool m_bIsActive;

        public PanelTMSNPower(List<StatisticCommon.TEC> listTec, DelegateFunc fErrRep, DelegateFunc fActRep)
        {
            InitializeComponent();

            m_msecPeriodUpdate = Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME]) * 1000;

            this.Dock = DockStyle.Fill;

            //this.Location = new System.Drawing.Point(40, 58);
            //this.Name = "pnlView";
            //this.Size = new System.Drawing.Size(705, 747);

            this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle;

            PanelTecTMSNPower ptcp;

            int i = -1;

            this.ColumnCount = listTec.Count / 2;
            if (this.ColumnCount == 0) this.ColumnCount++; else ;
            this.RowCount = listTec.Count / this.ColumnCount;

            for (i = 0; i < listTec.Count; i++)
            {
                ptcp = new PanelTecTMSNPower(listTec[i], fErrRep, fActRep);
                this.Controls.Add(ptcp, i % this.ColumnCount, i / this.ColumnCount);
            }

            for (i = 0; i < this.ColumnCount; i++)
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / this.ColumnCount));

            for (i = 0; i < this.RowCount; i++)
                this.RowStyles.Add(new RowStyle(SizeType.Percent, 100 / this.RowCount));
        }

        public PanelTMSNPower(IContainer container, List<StatisticCommon.TEC> listTec, DelegateFunc fErrRep, DelegateFunc fActRep)
            : this(listTec, fErrRep, fActRep)
        {
            container.Add(this);
        }

        public override void Start()
        {
            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecTMSNPower)
                {
                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecTMSNPower)ctrl).m_tecView.m_tec.m_id))
                        ((PanelTecTMSNPower)ctrl).Start();
                    //else ;
                    i++;
                }
                else
                    ;
            }
        }

        public override void Stop()
        {
            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecTMSNPower)
                {
                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecTMSNPower)ctrl).m_tecView.m_tec.m_id))
                        ((PanelTecTMSNPower)ctrl).Stop();
                    //else ;
                    i++;
                }
                else
                    ;
            }
        }

        protected override void initTableHourRows()
        {
            //Ничего не делаем, т.к. нет таблиц с часовыми значениями
        }

        public override void Activate(bool active)
        {
            if (m_bIsActive == active)
                return;
            else
                ;

            m_bIsActive = active;

            //TypeConverter conv;
            //dynamic dynObj = null;
            Type typeChildren; //PanelTecCurPower
            typeChildren = typeof(PanelTecTMSNPower);

            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl.GetType().Equals(typeChildren) == true)
                {
                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == (((PanelTecTMSNPower)ctrl).m_tecView.m_tec.m_id)))
                        ((PanelTecTMSNPower)ctrl).Activate(active);
                    //else ;
                    i++;
                }
                else
                    ;
            }
        }

        partial class PanelTecTMSNPower
        {
            /// <summary>
            /// Требуется переменная конструктора.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary> 
            /// Освободить все используемые ресурсы.
            /// </summary>
            /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

            #region Код, автоматически созданный конструктором компонентов

            /// <summary>
            /// Обязательный метод для поддержки конструктора - не изменяйте
            /// содержимое данного метода при помощи редактора кода.
            /// </summary>
            private void InitializeComponent()
            {
                components = new System.ComponentModel.Container();
            }

            #endregion
        }

        private partial class PanelTecTMSNPower : TableLayoutPanel
        {
            Label[] m_arLabel;
            Dictionary<int, Label> m_dictLabelVal;

            bool isActive;

            public TecView m_tecView;

            private ManualResetEvent m_evTimerCurrent;
            private System.Threading.Timer m_timerCurrent;

            //private DelegateFunc delegateUpdateGUI;

            public PanelTecTMSNPower(StatisticCommon.TEC tec, DelegateFunc fErrRep, DelegateFunc fActRep)
            {
                InitializeComponent();

                m_tecView = new TecView (null, TecView.TYPE_PANEL.CUR_POWER, -1, -1);

                HMark markQueries = new HMark();
                markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO);

                m_tecView.InitTEC (new List <TEC> () { tec }, markQueries);
                m_tecView.SetDelegateReport(fErrRep, fActRep);

                m_tecView.updateGUI_TM_Gen = new DelegateFunc(showTMGenPower);
                m_tecView.updateGUI_TM_SN = new DelegateFunc(showTMSNPower);

                Initialize();
            }

            public PanelTecTMSNPower(IContainer container, StatisticCommon.TEC tec, DelegateFunc fErrRep, DelegateFunc fActRep)
                : this(tec, fErrRep, fActRep)
            {
                container.Add(this);
            }

            private void Initialize()
            {
                int i = -1;

                m_dictLabelVal = new Dictionary<int, Label>();
                m_arLabel = new Label[(int)INDEX_LABEL.VALUE_TM_SN + 1];

                this.Dock = DockStyle.Fill;
                //Свойства колонок
                this.ColumnCount = 4;
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

                //Видимая граница для отладки
                this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle;

                string cntnt = string.Empty;

                i = (int)INDEX_LABEL.NAME;
                cntnt = m_tecView.m_tec.name_shr;
                m_arLabel[i] = HLabel.createLabel(cntnt, PanelTMSNPower.s_arLabelStyles[i]);
                ////Предусмотрим обработчик при изменении значения
                //if (i == (int)INDEX_LABEL.VALUE_TOTAL)
                //    m_arLabel[i].TextChanged += new EventHandler(PanelTecCurPower_TextChangedValue);
                //else
                //    ;
                this.Controls.Add(m_arLabel[i], 0, i);
                this.SetColumnSpan(m_arLabel[i], this.ColumnCount);

                //Наименование ТЭЦ, Дата/время, Значение для всех ГТП/ТГ
                for (i = (int)INDEX_LABEL.DATETIME_TM_Gen; i < (int)INDEX_LABEL.VALUE_TM_SN + 1; i++)
                {
                    switch (i)
                    {
                        case (int)INDEX_LABEL.DATETIME_TM_Gen:
                        case (int)INDEX_LABEL.DATETIME_TM_SN:
                            cntnt = @"--:--:--";
                            break;
                        case (int)INDEX_LABEL.VALUE_TM_Gen:
                        case (int)INDEX_LABEL.VALUE_TM_SN:
                            cntnt = @"---";
                            break;
                        default:
                            break;
                    }
                    m_arLabel[i] = HLabel.createLabel(cntnt, PanelTMSNPower.s_arLabelStyles[i]);
                    ////Предусмотрим обработчик при изменении значения
                    //if (i == (int)INDEX_LABEL.VALUE_TOTAL)
                    //    m_arLabel[i].TextChanged += new EventHandler(PanelTecCurPower_TextChangedValue);
                    //else
                    //    ;
                    this.Controls.Add(m_arLabel[i], (i % 2) * 2, i / 2);
                    this.SetColumnSpan(m_arLabel[i], 2);
                }

                this.RowCount = COUNT_FIXED_ROWS;

                //Свойства зафиксированных строк
                for (i = 0; i < COUNT_FIXED_ROWS; i++)
                    this.RowStyles.Add(new RowStyle(SizeType.Percent, 10));

                isActive = false;
            }

            public void Start()
            {
                m_tecView.Start();

                m_evTimerCurrent = new ManualResetEvent(true);
                m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, ((PanelTMSNPower)Parent).m_msecPeriodUpdate - 1, ((PanelTMSNPower)Parent).m_msecPeriodUpdate - 1);

                isActive = false;
            }

            public void Stop()
            {
                m_tecView.Stop ();

                if (!(m_evTimerCurrent == null)) m_evTimerCurrent.Reset(); else ;
                if (!(m_timerCurrent == null)) m_timerCurrent.Dispose(); else ;
            }

            private void ChangeState()
            {
                m_tecView.ChangeState ();
            }

            public void Activate(bool active)
            {
                if (isActive == active)
                    return;
                else
                    ;

                isActive = active;

                if (isActive == true)
                {
                    ChangeState();
                }
                else
                {
                    m_tecView.ClearStates ();
                }
            }

            private void showTMGenPower()
            {
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new DelegateFunc(ShowTMGenPower));
                else
                    Logging.Logg().Error(@"PanelTecTMSNPower::showTMGenPower () - ... BeginInvoke (ShowTMGenPower) - ...");
            }

            private void showTMSNPower()
            {
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new DelegateFunc(ShowTMSNPower));
                else
                    Logging.Logg().Error(@"PanelTecTMSNPower::showTMSNPower () - ... BeginInvoke (ShowTMSNPower) - ...");
            }

            private void ShowTMGenPower()
            {
                double dblTotalPower_TM = 0.0
                        , dblTECComponentPower_TM = 0.0;
                foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
                {
                    if ((g.m_id > 100) && (g.m_id < 500))
                    {
                        dblTECComponentPower_TM = 0.0;

                        foreach (TG tg in g.m_listTG)
                        {
                            if (tg.id_tm > 0)
                            {
                                dblTECComponentPower_TM += setTextToLabelVal(null, m_tecView.m_dictValuesTG [tg.m_id].m_powerCurrent_TM);
                            }
                            else
                                m_dictLabelVal[tg.m_id].Text = @"---";
                        }

                        dblTotalPower_TM += setTextToLabelVal(null, dblTECComponentPower_TM);
                    }
                    else
                        ;
                }

                //???
                //setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM], m_tecView.m_dblTotalPower_TM_SN);
                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM_Gen], dblTotalPower_TM);

                //m_tecView.m_dtLastChangedAt_TM_Gen = HAdmin.ToMoscowTimeZone(m_tecView.m_dtLastChangedAt_TM_Gen);

                if ((m_tecView.serverTime - m_tecView.m_dtLastChangedAt_TM_Gen).TotalMinutes < 3)
                {
                    m_arLabel[(int)INDEX_LABEL.DATETIME_TM_Gen].Text = m_tecView.m_dtLastChangedAt_TM_Gen.ToString(@"HH:mm:ss");
                    m_arLabel[(int)INDEX_LABEL.DATETIME_TM_Gen].ForeColor = Color.Black;
                }
                else
                {
                    m_arLabel[(int)INDEX_LABEL.DATETIME_TM_Gen].Text = m_tecView.m_dtLastChangedAt_TM_Gen.ToString(@"dd.MM.yyyy HH:mm:ss");
                    m_arLabel[(int)INDEX_LABEL.DATETIME_TM_Gen].ForeColor = Color.Red;
                }
            }

            private void ShowTMSNPower()
            {
                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM_SN], m_tecView.m_dblTotalPower_TM_SN);

                if ((m_tecView.serverTime - m_tecView.m_dtLastChangedAt_TM_SN).TotalMinutes < 3)
                {
                    m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN].Text = m_tecView.m_dtLastChangedAt_TM_SN.ToString(@"HH:mm:ss");
                    m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN].ForeColor = Color.Black;
                }
                else
                {
                    m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN].Text = m_tecView.m_dtLastChangedAt_TM_SN.ToString(@"dd.MM.yyyy HH:mm:ss");
                    m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN].ForeColor = Color.Red;
                }
            }

            private double setTextToLabelVal(Label lblVal, double val)
            {
                if (val > 1)
                {
                    if (! (lblVal == null)) lblVal.Text = val.ToString(@"F2"); else ;
                    return val;
                }
                else
                    if (!(lblVal == null)) lblVal.Text = 0.ToString(@"F0"); else ;

                return 0;
            }

            private void TimerCurrent_Tick(Object stateInfo)
            {
                if (isActive == true)
                {
                    ChangeState();
                }
                else
                    ;
            }
        }
    }
}
