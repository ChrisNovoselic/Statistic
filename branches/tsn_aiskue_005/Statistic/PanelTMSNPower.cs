﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Data;

//using HClassLibrary;
using StatisticCommon;
using ASUTP.Core;
using ASUTP.Control;
using ASUTP;

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

    public partial class PanelTMSNPower : PanelStatisticWithTableHourRows
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
        //HReports m_report;

        public bool m_bIsActive;

        public PanelTMSNPower(List<StatisticCommon.TEC> listTec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
            : base (MODE_UPDATE_VALUES.AUTO, FormMain.formGraphicsSettings.BackgroundColor)
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;

            //this.Location = new System.Drawing.Point(40, 58);
            //this.Name = "pnlView";
            //this.Size = new System.Drawing.Size(705, 747);

            this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle;

            PanelTecTMSNPower ptcp;

            int i = -1;

            initializeLayoutStyle (listTec.Count / 2, listTec.Count);

            // фильтр ТЭЦ-ЛК
            for (i = 0; i < listTec.Count; i++)
                if (!(listTec[i].m_id > (int)TECComponent.ID.LK))
                {
                    ptcp = new PanelTecTMSNPower(listTec[i]/*, fErrRep, fWarRep, fActRep, fRepClr*/);
                    this.Controls.Add(ptcp, i % this.ColumnCount, i / this.ColumnCount);
                }
                else
                    ;
        }

        public PanelTMSNPower(IContainer container, List<StatisticCommon.TEC> listTec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
            : this(listTec/*, fErrRep, fWarRep, fActRep, fRepClr*/)
        {
            container.Add(this);
        }

        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            this.ColumnCount = cols;
            if (this.ColumnCount == 0) this.ColumnCount++; else ;
            this.RowCount = rows / this.ColumnCount;
            
            initializeLayoutStyleEvenly();
        }

        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            foreach (Control ptcp in this.Controls)
                if (ptcp is PanelTecTMSNPower)
                    (ptcp as PanelTecTMSNPower).SetDelegateReport(ferr, fwar, fact, fclr);
                else
                    ;
        }

        public override void Start()
        {
            base.Start();

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

            base.Stop();
        }

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate (active);
            
            if (bRes == false)
                return bRes;
            else
                ;

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

            return bRes;
        }

        protected override void initTableHourRows ()
        {
            //Ничего не делаем, т.к. нет таблиц с часовыми значениями
        }

        public override void UpdateGraphicsCurrent (int type)
        {
            List<PanelTecTMSNPower> listPanelTec = getTypedControls (this, new Type [] { typeof(PanelTecTMSNPower) }).Cast<PanelTecTMSNPower> ().ToList();

            listPanelTec.ForEach (panel => panel.UpdateGraphicsCurrent());
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

        private partial class PanelTecTMSNPower : HPanelCommon
        {
            Label[] m_arLabel;
            Dictionary<int, Label> m_dictLabelVal;

            //bool isActive;

            public TecViewTMPower m_tecView;

            private ManualResetEvent m_evTimerCurrent;
            private
                System.Threading.Timer //Вариант №0
                //System.Windows.Forms.Timer //Вариант №1
                    m_timerCurrent;

            //private DelegateFunc delegateUpdateGUI;

            public PanelTecTMSNPower(StatisticCommon.TEC tec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
                : base (-1, -1)
            {
                InitializeComponent();

                m_tecView = new TecViewTMPower();

                HMark markQueries = new HMark(new int[] { (int)CONN_SETT_TYPE.ADMIN, (int)CONN_SETT_TYPE.DATA_SOTIASSO });
                //markQueries.Marked((int)CONN_SETT_TYPE.ADMIN); //Для получения даты/времени
                //markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO);

                m_tecView.InitTEC (new List <TEC> () { tec }, markQueries);
                //m_tecView.SetDelegateReport(fErrRep, fWarRep, fActRep, fRepClr);

                m_tecView.updateGUI_TM_Gen = new DelegateFunc(updateGUI_TM_Gen);
                m_tecView.updateGUI_TM_SN = new DelegateFunc(updateGUI_TM_SN);

                Initialize();
            }

            public PanelTecTMSNPower(IContainer container, StatisticCommon.TEC tec/*, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr*/)
                : this(tec/*, fErrRep, fWarRep, fActRep, fRepClr*/)
            {
                container.Add(this);
            }

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                throw new NotImplementedException();
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

                //isActive = false;
            }

            public void SetDelegateReport(DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr)
            {
                m_tecView.SetDelegateReport(fErrRep, fWarRep, fActRep, fRepClr);
            }

            public override void Start()
            {
                if (Started == false)
                {
                    base.Start();

                    if (m_tecView.IsStarted == false)
                        m_tecView.Start();
                    else
                        ;

                    m_evTimerCurrent = new ManualResetEvent(true);
                    //m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, PanelStatistic.POOL_TIME * 1000 - 1, PanelStatistic.POOL_TIME * 1000 - 1);
                    m_timerCurrent =
                        new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, PanelStatistic.POOL_TIME * 1000 - 1, System.Threading.Timeout.Infinite)
                        //new System.Windows.Forms.Timer ()
                        ;
                    ////Вариант №1
                    //m_timerCurrent.Tick += new EventHandler(TimerCurrent_Tick);
                    //m_timerCurrent.Interval = ProgramBase.TIMER_START_INTERVAL; // по этому признаку определим задержку выполнения итерации
                    //m_timerCurrent.Start ();
                }
                else
                    ;
            }

            public override void Stop()
            {
                if (m_tecView.IsStarted == true)
                    m_tecView.Stop();
                else
                    ;

                if (Started == true)
                {
                    if (!(m_evTimerCurrent == null)) m_evTimerCurrent.Reset(); else ;
                    if (!(m_timerCurrent == null))
                    {
                        ////Вариант №1
                        //m_timerCurrent.Stop ();
                        //Вариант №0
                        m_timerCurrent.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                        m_timerCurrent.Dispose();

                        m_timerCurrent = null;
                    }
                    else ;

                    base.Stop();
                }
                else
                    ;
            }

            private void changeState()
            {
                m_tecView.ChangeState ();
            }

            public override bool Activate(bool active)
            {
                bool bRes = base.Activate (active);

                if (bRes == false)
                    return false;
                else
                    ;

                if (Actived == true)
                {
                    //Вариант №0
                    m_timerCurrent.Change (0, System.Threading.Timeout.Infinite);
                    ////Вариант №1
                    //m_timerCurrent.Start ();
                }
                else
                {
                    m_tecView.ClearStates ();
                }

                return bRes;
            }

            private void updateGUI_TM_Gen()
            {
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new DelegateFunc(showTMGenPower));
                else
                    Logging.Logg().Error(@"PanelTecTMSNPower::showTMGenPower () - ... BeginInvoke (ShowTMGenPower) - ...", Logging.INDEX_MESSAGE.D_001);
            }

            private void updateGUI_TM_SN()
            {
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new DelegateFunc(showTMSNPower));
                else
                    Logging.Logg().Error(@"PanelTecTMSNPower::showTMSNPower () - ... BeginInvoke (ShowTMSNPower) - ...", Logging.INDEX_MESSAGE.D_001);
            }

            private void showTMGenPower()
            {
                double dblTotalPower_TM = 0.0
                        , dblTECComponentPower_TM = 0.0;                

                foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
                {
                    if (g.IsGTP == true)
                    {
                        dblTECComponentPower_TM = 0.0;

                        foreach (TG tg in g.m_listLowPointDev)
                        {
                            if (tg.m_strKKS_NAME_TM.Length > 0)
                            {
                                dblTECComponentPower_TM += setTextToLabelVal(null, m_tecView.m_dictValuesLowPointDev [tg.m_id].m_powerCurrent_TM);
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

                //m_tecView.m_dtLastChangedAt_TM_Gen = HDateTime.ToMoscowTimeZone(m_tecView.m_dtLastChangedAt_TM_Gen);

                setTextToLabelDateTime(m_tecView.m_dtLastChangedAt_TM_Gen, (int)INDEX_LABEL.DATETIME_TM_Gen);
            }

            private void showTMSNPower()
            {
                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM_SN], m_tecView.m_dblTotalPower_TM_SN);

                setTextToLabelDateTime(m_tecView.m_dtLastChangedAt_TM_SN, (int)INDEX_LABEL.DATETIME_TM_SN);
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
            /// <summary>
            /// Отобразить значение дату/время (копия в PanelCurPower.PanelTecCurPower)
            /// </summary>
            /// <param name="dt">Дата/время для отображения</param>
            /// <param name="indx">Индекс значения (генерация или СН)</param>
            private void setTextToLabelDateTime (DateTime dt, int indx)
            {
                Color clrDatetime = Color.Empty;
                string strFmtDatetime = @"HH:mm:ss";

                if (TecView.ValidateDatetimeTMValue(m_tecView.serverTime, dt) == true)
                    // формат даты/времени без изменения (без даты)
                    clrDatetime = Color.Black;
                else
                {
                    strFmtDatetime = @"dd.MM.yyyy " + strFmtDatetime; // добавить дату
                    clrDatetime = Color.Red;
                }

                m_arLabel[indx].Text = dt.ToString(strFmtDatetime);
                m_arLabel[indx].ForeColor = clrDatetime;
            }

            private void TimerCurrent_Tick(Object stateInfo)
            //private void TimerCurrent_Tick(Object stateInfo, EventArgs ev)
            {
                if (Actived == true)
                {
                    ////Вариант №1
                    ////Задержка выполнения итерации
                    //if (m_timerCurrent.Interval == ProgramBase.TIMER_START_INTERVAL)
                    //{
                    //    m_timerCurrent.Interval = PanelStatistic.POOL_TIME * 1000 - 1;

                    //    return;
                    //}
                    //else
                    //    ;

                    changeState();

                    //Вариант №0
                    m_timerCurrent.Change (PanelStatistic.POOL_TIME * 1000 - 1, System.Threading.Timeout.Infinite);
                }
                else
                    ;
            }

            public void UpdateGraphicsCurrent ()
            {
                showTMGenPower ();
                showTMSNPower ();
            }
        }
    }
}
