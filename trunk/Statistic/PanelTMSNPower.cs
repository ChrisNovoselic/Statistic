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

            m_msecPeriodUpdate = Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME]);

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
                m_tecView.InitTEC (new List <TEC> () { tec });
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
                this.BeginInvoke(new DelegateFunc(ShowTMGenPower));
            }

            private void showTMSNPower()
            {
                this.BeginInvoke(new DelegateFunc(ShowTMSNPower));
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
                                dblTECComponentPower_TM += setTextToLabelVal(null, tg.power_TM);
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
                //try { m_tecView.m_dtLastChangedAt_TM_Gen = HAdmin.ToCurrentTimeZone(m_tecView.m_dtLastChangedAt_TM_Gen); }
                //catch (Exception e)
                //{
                //    Logging.Logg().LogExceptionToFile(e, @"PanelTecCurPower::ShowTMPower () - HAdmin.ToCurrentTimeZone () - ...");
                //}
                m_arLabel[(int)INDEX_LABEL.DATETIME_TM_Gen].Text = m_tecView.m_dtLastChangedAt_TM_Gen.ToString(@"HH:mm:ss");
            }

            private void ShowTMSNPower()
            {
                setTextToLabelVal(m_arLabel[(int)INDEX_LABEL.VALUE_TM_SN], m_tecView.m_dblTotalPower_TM_SN);
                //try { m_dtLastChangedAt = HAdmin.ToCurrentTimeZone (m_dtLastChangedAt); }
                //catch (Exception e) { Logging.Logg ().LogExceptionToFile (e, @"PanelTMSNPower::ShowTMSNPower () - ..."); }
                m_arLabel[(int)INDEX_LABEL.DATETIME_TM_SN].Text = m_tecView.m_dtLastChangedAt_TM_SN.ToString(@"HH:mm:ss");
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

            private void PanelTecCurPower_TextChangedValue(object sender, EventArgs ev)
            {
                double val = -1.0;
                int ext = 2;
                Color clr;
                if (double.TryParse(((Label)sender).Text, out val) == true)
                {
                    if (val > 1)
                        clr = Color.LimeGreen;
                    else
                    {
                        clr = Color.Green;
                        ext = 0;
                    }

                    ((Label)sender).Text = val.ToString(@"F" + ext.ToString());
                }
                else
                    clr = Color.Green;

                ((Label)sender).ForeColor = clr;
            }

            //private bool GetCurrentTMResponse(DataTable table)
            //{
            //    bool bRes = true;
            //    int i = -1,
            //        id = -1;
            //    double value = -1;
            //    DateTime dtLastChangedAt =
            //        m_dtLastChangedAt_TM = DateTime.Now;
            //    TG tgTmp;

            //    foreach (TECComponent g in m_tec.list_TECComponents)
            //    {
            //        foreach (TG t in g.m_listTG)
            //        {
            //            t.power_TM = 0;
            //        }
            //    }

            //    for (i = 0; i < table.Rows.Count; i++)
            //    {
            //        if (int.TryParse(table.Rows[i]["ID"].ToString(), out id) == false)
            //            return false;
            //        else
            //            ;

            //        tgTmp = m_tec.FindTGById(id, TG.INDEX_VALUE.TM, (TG.ID_TIME)(-1));

            //        if (tgTmp == null)
            //            return false;
            //        else
            //            ;

            //        if (!(table.Rows[i]["value"] is DBNull))
            //            if (double.TryParse(table.Rows[i]["value"].ToString(), out value) == false)
            //                return false;
            //            else
            //                ;
            //        else
            //            value = 0.0;

            //        if ((!(value < 1)) && (DateTime.TryParse(table.Rows[i]["last_changed_at"].ToString(), out dtLastChangedAt) == false))
            //            return false;
            //        else
            //            ;

            //        if (m_dtLastChangedAt_TM > dtLastChangedAt)
            //            m_dtLastChangedAt_TM = dtLastChangedAt;
            //        else
            //            ;

            //        switch (m_tec.type())
            //        {
            //            case StatisticCommon.TEC.TEC_TYPE.COMMON:
            //                break;
            //            case StatisticCommon.TEC.TEC_TYPE.BIYSK:
            //                //value *= 20;
            //                break;
            //            default:
            //                break;
            //        }

            //        tgTmp.power_TM = value;
            //    }

            //    return bRes;
            //}

            //private void GetCurrentTMSNRequest()
            //{
            //    m_tec.Request(CONN_SETT_TYPE.DATA_SOTIASSO, m_tec.currentTMSNRequest());
            //}

            //private bool GetCurrentTMSNResponse(DataTable table)
            //{
            //    bool bRes = true;
            //    int id = -1;

            //    m_dtLastChangedAt_TM_SN = DateTime.Now;

            //    if (table.Rows.Count == 1)
            //    {
            //        if (int.TryParse(table.Rows[0]["ID_TEC"].ToString(), out id) == false)
            //            return false;
            //        else
            //            ;

            //        if (!(table.Rows[0]["SUM_P_SN"] is DBNull))
            //            if (double.TryParse(table.Rows[0]["SUM_P_SN"].ToString(), out m_dblTotalPower_TM_SN) == false)
            //                return false;
            //            else
            //                ;
            //        else
            //            m_dblTotalPower_TM_SN = 0.0;

            //        if ((!(m_dblTotalPower_TM_SN < 1)) && (DateTime.TryParse(table.Rows[0]["LAST_UPDATE"].ToString(), out m_dtLastChangedAt_TM_SN) == false))
            //            return false;
            //        else
            //            ;
            //    }
            //    else
            //    {
            //        bRes = false;
            //    }

            //    return bRes;
            //}

            //private void ErrorReportSensors(ref DataTable src)
            //{
            //    string error = "Ошибка определения идентификаторов датчиков в строке ";
            //    for (int j = 0; j < src.Rows.Count; j++)
            //        error += src.Rows[j][0].ToString() + " = " + src.Rows[j][1].ToString() + ", ";

            //    error = error.Substring(0, error.LastIndexOf(","));
            //    ErrorReport(error);
            //}

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
