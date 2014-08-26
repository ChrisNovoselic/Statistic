using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Data;

using StatisticCommon;

namespace Statistic
{
    partial class PanelLastMinutes
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

    public partial class PanelLastMinutes : PanelStatisticView
    {
        protected static AdminTS.TYPE_FIELDS s_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;
        
        List <Label> m_listLabelDateTime;

        enum INDEX_LABEL : int { NAME_TEC, NAME_COMPONENT, VALUE_COMPONENT, DATETIME, COUNT_INDEX_LABEL };
        static Color s_clrBakColorLabel = Color.FromArgb(212, 208, 200), s_clrBakColorLabelVal = Color.FromArgb(219, 223, 227);
        static HLabelStyles[] s_arLabelStyles = {new HLabelStyles(Color.Black, s_clrBakColorLabel, 14F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBakColorLabel, 12F, ContentAlignment.MiddleCenter),
                                                new HLabelStyles(Color.Black, s_clrBakColorLabelVal, 10F, ContentAlignment.MiddleRight),
                                                new HLabelStyles(Color.Black, s_clrBakColorLabel, 12F, ContentAlignment.MiddleCenter)};
        static int COUNT_FIXED_ROWS = (int)INDEX_LABEL.NAME_COMPONENT + 1;
        //static int COUNT_HOURS = 24;

        static RowStyle fRowStyle () { return new RowStyle(SizeType.Percent, (float)Math.Round((double)100 / (24 + COUNT_FIXED_ROWS), 6)); }

        //AdminTS m_admin;

        enum StatesMachine : int
        {
            Init_TM,
            LastMinutes_TM,
            PBRValues,
            AdminValues
        };

        //public DelegateFunc delegateEventUpdate;

        public int m_msecPeriodUpdate;

        public bool m_bIsActive;

        public PanelLastMinutes(List<StatisticCommon.TEC> listTec, DelegateFunc fErrRep, DelegateFunc fActRep)
        {
            int i = -1;
            
            InitializeComponent();

            this.Dock = DockStyle.Fill;

            this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle

            this.ColumnCount = listTec.Count + 1;
            this.RowCount = 1;

            //Создание панели с дата/время
            PanelDateTime panelDateTime = new PanelDateTime();

            float fPercentColDatetime = 8F;
            this.Controls.Add(panelDateTime, 0, 0);
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, fPercentColDatetime));

            int iCountSubColumns = 0;
            
            for (i = 0; i < listTec.Count; i++)
            {
                this.Controls.Add(new PanelTecLastMinutes(listTec[i], fErrRep, fActRep), i + 1, 0);
                iCountSubColumns += ((PanelTecLastMinutes)this.Controls [i + 1]).CountTECComponent; //Слева столбец дата/время
            }

            //Размеры столбцов после создания столбцов, т.к.
            //кол-во "подстолбцов" в столбцах до их создания неизвестно
            for (i = 0; i < listTec.Count; i++)
            {
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, ((float)100 - fPercentColDatetime) / iCountSubColumns * ((PanelTecLastMinutes)this.Controls[i + 1]).CountTECComponent));
            }
        }

        public PanelLastMinutes(IContainer container, List<StatisticCommon.TEC> listTec, DelegateFunc fErrRep, DelegateFunc fActRep)
            : this(listTec, fErrRep, fActRep)
        {
            container.Add(this);
        }

        public override void Start()
        {
            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecLastMinutes)
                {
                    if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecLastMinutes)ctrl).m_id_tec)) ((PanelTecLastMinutes)ctrl).Start(); else ;
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
                if (ctrl is PanelTecLastMinutes)
                {
                    if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecLastMinutes)ctrl).m_id_tec)) ((PanelTecLastMinutes)ctrl).Stop(); else ;
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

            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecLastMinutes)
                {
                    if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecLastMinutes)ctrl).m_id_tec)) ((PanelTecLastMinutes)ctrl).Activate(active); else ;
                    i++;
                }
                else
                    ;
            }
        }

        partial class PanelDateTime
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

        private partial class PanelDateTime : TableLayoutPanel
        {
            private Dictionary<int, Label> m_dictLabelTime;

            public PanelDateTime()
            {
                InitializeComponent();

                Initialize();
            }

            public PanelDateTime(IContainer container)
                : this()
            {
                container.Add(this);
            }

            private void Initialize()
            {
                int i = -1;
           
                this.Dock = DockStyle.Fill;
                this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle
                this.RowCount = 24 + COUNT_FIXED_ROWS;

                DateTime dtNow = DateTime.Now.Date;

                //Добавить дату
                Label lblDate = HLabel.createLabel(dtNow.ToString (@"dd.MM.yyyy"), PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_TEC]);
                this.Controls.Add(lblDate, 0, 0);
                this.SetRowSpan(lblDate, COUNT_FIXED_ROWS);

                m_dictLabelTime = new Dictionary<int,Label> ();

                dtNow = dtNow.AddMinutes(59);
                for (i = 1; i < 25; i++)
                {
                    m_dictLabelTime[i - 1] = HLabel.createLabel(dtNow.ToString (@"HH:mm"), PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.DATETIME]);
                    this.Controls.Add(m_dictLabelTime[i - 1], 0, (i - 1) + COUNT_FIXED_ROWS);

                    dtNow = dtNow.AddHours(1);
                }

                for (i = 0; i < (24 + COUNT_FIXED_ROWS - 1); i++)
                {
                    this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                }

                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 6F));
            }
        }

        partial class PanelTecLastMinutes
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

        public partial class PanelTecLastMinutes : TableLayoutPanel
        {
            public int m_id_tec { get { return m_tecView.m_tec.m_id; } }

            List<TECComponentBase> m_list_TECComponents;
            public int CountTECComponent { get { return m_list_TECComponents.Count; } }

            private List<TG> m_listSensorId2TG;

            private Dictionary<int, Label[]> m_dictLabelVal;
            private Dictionary<int, ToolTip[]> m_dictToolTip;

            private bool m_bIsActive,
                        m_bIsStarted,
                        m_bUpdate;

            //private Dictionary<int, TecView.valuesTECComponent> m_dictValuesHours;
            TecView m_tecView;

            private ManualResetEvent m_evTimerCurrent;
            private System.Threading.Timer m_timerCurrent;

            public PanelTecLastMinutes(StatisticCommon.TEC tec, DelegateFunc fErrRep, DelegateFunc fActRep)
            {
                InitializeComponent();

                m_tecView = new TecView (null, TecView.TYPE_PANEL.LAST_MINUTES);
                m_tecView.InitTEC (new List <TEC> () { tec });
                m_tecView.SetDelegateReport(fErrRep, fActRep);

                Initialize();
            }

            public PanelTecLastMinutes(IContainer container, StatisticCommon.TEC tec, DelegateFunc fErrRep, DelegateFunc fActRep)
                : this(tec, fErrRep, fActRep)
            {
                container.Add(this);
            }

            private void Initialize()
            {
                int i = -1;
                m_list_TECComponents = new List<TECComponentBase> ();
                m_dictLabelVal = new Dictionary<int, Label[]>();
                m_dictToolTip = new Dictionary<int, ToolTip[]>();

                this.Dock = DockStyle.Fill;
                this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle
                this.RowCount = 24 + COUNT_FIXED_ROWS;

                //Добавить наименование станции
                Label lblNameTEC = HLabel.createLabel(m_tecView.m_tec.name_shr, PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_TEC]);
                this.Controls.Add(lblNameTEC, 0, 0);

                foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
                {
                    if ((g.m_id > 100) && (g.m_id < 500))
                    {
                        //Добавить наименование ГТП
                        this.Controls.Add(HLabel.createLabel(g.name_shr.Split (' ')[1], PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_COMPONENT]), CountTECComponent, COUNT_FIXED_ROWS - 1);

                        //Память под ячейки со значениями
                        m_dictLabelVal.Add(g.m_id, new Label[24]);
                        m_dictToolTip.Add(g.m_id, new ToolTip[24]);

                        for (i = 0; i < 24; i ++)
                        {
                            m_dictLabelVal[g.m_id][i] = HLabel.createLabel (0.ToString (@"F2"), PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.VALUE_COMPONENT]);
                            m_dictToolTip[g.m_id][i] = new ToolTip();
                            m_dictToolTip[g.m_id][i].IsBalloon = true;
                            m_dictToolTip[g.m_id][i].ShowAlways = true;
                            m_dictToolTip[g.m_id][i].SetToolTip(m_dictLabelVal[g.m_id][i], @"ПБР: ---, d: --%");

                            this.Controls.Add(m_dictLabelVal[g.m_id][i], CountTECComponent, i + COUNT_FIXED_ROWS);
                        }

                        //Добавить компонент ТЭЦ (ГТП)
                        m_list_TECComponents.Add(g);
                    }
                    else
                        ;
                }

                for (i = 0; i < (24 + COUNT_FIXED_ROWS - 1); i++)
                {
                    this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                }

                for (i = 0; i < CountTECComponent; i++)
                {
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / CountTECComponent));
                }

                if (CountTECComponent > 0)
                    this.SetColumnSpan(lblNameTEC, CountTECComponent);
                else
                    ;

                m_dictValuesHours = new Dictionary<int, TecView.valuesTECComponent> ();
                foreach (TECComponent c in m_list_TECComponents)
                {
                    m_dictValuesHours.Add(c.m_id, new TecView.valuesTECComponent(24 + 1));
                }
            }

            public void Start()
            {
                if (m_bIsStarted == true)
                    return;
                else
                    ;

                m_bIsStarted = true;

                m_tecView.Start ();

                ClearValues();

                //Милисекунды до первого запуска функции таймера
                double msecUpdate = (new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour + 1, 11, 0) - DateTime.Now).TotalMilliseconds;

                m_evTimerCurrent = new ManualResetEvent(true);
                m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, (Int64) msecUpdate, ((PanelLastMinutes)Parent).m_msecPeriodUpdate - 1);

                m_bUpdate = false;
            }

            public void Stop()
            {
                if (m_bIsStarted == false)
                    return;
                else
                    ;

                m_evTimerCurrent.Reset();
                m_timerCurrent.Dispose();

                m_tecView.Stop ();

                FormMainBaseWithStatusStrip.m_report.ClearStates();
            }

            private void ChangeState()
            {
                m_tecView.ChangeState ();
            }

            public void Activate(bool active)
            {
                if (m_bIsActive == active)
                    return;
                else
                    ;

                m_bIsActive = active;

                if (m_bIsActive == true)
                {
                    ChangeState();
                }
                else
                {
                    m_tecView.ClearStates ();
                }
            }

            private void ShowLastMinutesTM()
            {
                Color clrBackColor;
                int warn = -1
                    , cntWarn = -1
                    ;
                Hd2PercentControl d2PercentControl = new Hd2PercentControl ();
                string strToolTip = string.Empty,
                        strWarn = string.Empty;
                
                foreach (TECComponent g in m_list_TECComponents)
                {
                    cntWarn = 0;
                    for (int hour = 1; hour < 25; hour++)
                    {
                        clrBackColor = s_clrBakColorLabelVal;
                        strToolTip = string.Empty;

                        bool bPmin = false;
                        if (m_tecView.m_tec.m_id == 5) bPmin = true; else ;
                        strToolTip = d2PercentControl.Calculate(m_dictValuesHours[g.m_id], hour, bPmin, out warn);

                        m_dictToolTip[g.m_id][hour - 1].SetToolTip(m_dictLabelVal[g.m_id][hour - 1], strToolTip);

                        if (m_dictValuesHours[g.m_id].valuesLastMinutesTM[hour] > 1)
                        {
                            if ((! (warn == 0)) &&
                                (m_dictValuesHours[g.m_id].valuesLastMinutesTM[hour] > 1))
                            {
                                clrBackColor = Color.Red;
                                cntWarn ++;
                            }
                            else
                                cntWarn = 0;

                            if (cntWarn > 0) {
                                //strWarn = cntWarn + @":";
                            }
                            else
                                strWarn = string.Empty;

                            m_dictLabelVal[g.m_id][hour - 1].Text = strWarn + m_dictValuesHours[g.m_id].valuesLastMinutesTM[hour].ToString(@"F2");
                        }
                        else
                            m_dictLabelVal[g.m_id][hour - 1].Text = 0.ToString (@"F0");

                        m_dictLabelVal[g.m_id][hour - 1].BackColor = clrBackColor;
                    }
                }
            }

            private void PanelTecCurPower_TextChangedValue(object sender, EventArgs ev)
            {
                double val = -1.0;
                Color clr;
                if (double.TryParse(((Label)sender).Text, out val) == true)
                {
                    if (val > 1)
                        clr = Color.LimeGreen;
                    else
                        clr = Color.Green;
                }
                else
                    clr = Color.Green;

                ((Label)sender).ForeColor = clr;
            }

            private void TimerCurrent_Tick(Object stateInfo)
            {
                if (m_bIsActive == true)
                {
                    ChangeState();
                }
                else
                    ;
            }

            //private void GetAdminValuesRequest(AdminTS.TYPE_FIELDS mode)
            //{
            //    lock (lockValue)
            //    {
            //        DbSources.Sources ().Request(m_tec.m_arIdListeners[(int)CONN_SETT_TYPE.ADMIN], m_tec.GetAdminValueQuery(-1, DateTime.Now.Date, mode));
            //    }
            //}

            //private bool GetAdminValuesResponse(DataTable table_in)
            //{
            //    DateTime date = DateTime.Now.Date
            //             , dtPBR;
            //    int hour;

            //    int offsetPrev = -1
            //        //, tableRowsCount = table_in.Rows.Count; //tableRowsCount = m_tablePBRResponse.Rows.Count
            //        , i = -1, j = -1,
            //        offsetUDG, offsetPlan, offsetLayout;

            //    offsetUDG = 1;
            //    offsetPlan = /*offsetUDG + 3 * tec.list_TECComponents.Count +*/ 1; //ID_COMPONENT
            //    offsetLayout = -1;

            //    //m_tablePBRResponse = restruct_table_pbrValues(m_tablePBRResponse);
            //    m_tablePBRResponse = TecView.restruct_table_pbrValues(m_tablePBRResponse, m_tec.list_TECComponents, -1);
            //    offsetLayout = (!(m_tablePBRResponse.Columns.IndexOf("PBR_NUMBER") < 0)) ? (offsetPlan + m_list_TECComponents.Count * 3) : m_tablePBRResponse.Columns.Count;

            //    //table_in = restruct_table_adminValues(table_in);
            //    table_in = TecView.restruct_table_adminValues(table_in, m_tec.list_TECComponents, -1);

            //    //if (!(table_in.Columns.IndexOf("ID_COMPONENT") < 0))
            //    //    try { table_in.Columns.Remove("ID_COMPONENT"); }
            //    //    catch (Exception excpt)
            //    //    {
            //    //        /*
            //    //        Logging.Logg().LogExceptionToFile(excpt, "catch - PanelLastMinutes.GetAdminValuesResponse () - ...");
            //    //        */
            //    //    }
            //    //else
            //    //    ;

            //    // поиск в таблице записи по предыдущим суткам (мало ли, вдруг нету)
            //    for (i = 0; i < m_tablePBRResponse.Rows.Count && offsetPrev < 0; i++)
            //    {
            //        if (!(m_tablePBRResponse.Rows[i]["DATE_PBR"] is System.DBNull))
            //        {
            //            try
            //            {
            //                hour = ((DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"]).Hour;
            //                if (hour == 0 && ((DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"]).Day == date.Day)
            //                {
            //                    offsetPrev = i;
            //                    //foreach (TECComponent g in tec.list_TECComponents)
            //                    for (j = 0; j < m_list_TECComponents.Count; j++)
            //                    {
            //                        if ((offsetPlan + j * 3) < m_tablePBRResponse.Columns.Count) {
            //                            m_dictValuesHours [m_list_TECComponents [j].m_id].valuesPBR[0] = (double)m_tablePBRResponse.Rows[i][offsetPlan + j * 3];
            //                            m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPmin[0] = (double)m_tablePBRResponse.Rows[i][offsetPlan + j * 3 + 1];
            //                            m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPmax[0] = (double)m_tablePBRResponse.Rows[i][offsetPlan + j * 3 + 2];
            //                        }
            //                        else {
            //                             m_dictValuesHours [m_list_TECComponents [j].m_id].valuesPBR[0] = 0.0;
            //                             m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPmin[0] = 0.0;
            //                             m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPmax[0] = 0.0;
            //                        }
            //                        //j++;
            //                    }
            //                }
            //                else
            //                    ;
            //            }
            //            catch (Exception excpt) { Logging.Logg().LogExceptionToFile(excpt, "catch - PanelLastMinutes.GetAdminValuesResponse () - ..."); }
            //        }
            //        else
            //        {
            //            try
            //            {
            //                hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
            //                if (hour == 0 && ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day == date.Day)
            //                {
            //                    offsetPrev = i;
            //                }
            //                else
            //                    ;
            //            }
            //            catch
            //            {
            //            }
            //        }
            //    }

            //    // разбор остальных значений
            //    for (i = 0; i < m_tablePBRResponse.Rows.Count; i++)
            //    {
            //        if (i == offsetPrev)
            //            continue;

            //        if (!(m_tablePBRResponse.Rows[i]["DATE_PBR"] is System.DBNull))
            //        {
            //            try
            //            {
            //                dtPBR = (DateTime)m_tablePBRResponse.Rows[i]["DATE_PBR"];

            //                hour = dtPBR.Hour;
            //                if ((hour == 0) && (!(dtPBR.Day == date.Day)))
            //                    hour = 24;
            //                else
            //                    if (hour == 0)
            //                        continue;
            //                    else
            //                        ;

            //                //foreach (TECComponent g in tec.list_TECComponents)
            //                for (j = 0; j < m_list_TECComponents.Count; j++)
            //                {
            //                    try
            //                    {
            //                        if (((offsetPlan + (j * 3)) < m_tablePBRResponse.Columns.Count) && (!(m_tablePBRResponse.Rows[i][offsetPlan + (j * 3)] is System.DBNull))) {
            //                             m_dictValuesHours [m_list_TECComponents [j].m_id].valuesPBR[hour] = (double)m_tablePBRResponse.Rows[i][offsetPlan + (j * 3)];
            //                             m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPmin[hour] = (double)m_tablePBRResponse.Rows[i][offsetPlan + (j * 3) + 1];
            //                             m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPmax[hour] = (double)m_tablePBRResponse.Rows[i][offsetPlan + (j * 3) + 2];
            //                        }
            //                        else
            //                            ;

            //                        DataRow[] row_in = table_in.Select("DATE_ADMIN = '" + dtPBR.ToString("yyyy-MM-dd HH:mm:ss") + "'");
            //                        if (row_in.Length > 0)
            //                        {
            //                            if (row_in.Length > 1)
            //                                ; //Ошибка...
            //                            else
            //                                ;

            //                            if (!(row_in[0][offsetUDG + j * 3] is System.DBNull))
            //                                //if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull)))
            //                                m_dictValuesHours[m_list_TECComponents[j].m_id].valuesREC[hour] = (double)row_in[0][offsetUDG + j * 3];
            //                            else
            //                                m_dictValuesHours[m_list_TECComponents[j].m_id].valuesREC[hour] = 0;

            //                            if (!(row_in[0][offsetUDG + 1 + j * 3] is System.DBNull))
            //                                m_dictValuesHours[m_list_TECComponents[j].m_id].valuesISPER[hour] = (int)row_in[0][offsetUDG + 1 + j * 3];
            //                            else
            //                                ;

            //                            if (!(row_in[0][offsetUDG + 2 + j * 3] is System.DBNull))
            //                                m_dictValuesHours [m_list_TECComponents[j].m_id].valuesDIV[hour] = (double)row_in[0][offsetUDG + 2 + j * 3];
            //                            else
            //                                ;
            //                        }
            //                        else
            //                        {
            //                            m_dictValuesHours[m_list_TECComponents[j].m_id].valuesREC[hour] = 0;
            //                        }
            //                    }
            //                    catch
            //                    {
            //                    }
            //                    //j++;
            //                }
            //            }
            //            catch
            //            {
            //            }
            //        }
            //        else
            //        {
            //            try
            //            {
            //                hour = ((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Hour;
            //                if ((hour == 0) && (!(((DateTime)table_in.Rows[i]["DATE_ADMIN"]).Day == date.Day)))
            //                    hour = 24;
            //                else
            //                    ;

            //                //foreach (TECComponent g in tec.list_TECComponents)
            //                for (j = 0; j < m_list_TECComponents.Count; j++)
            //                {
            //                    try
            //                    {
            //                         m_dictValuesHours [m_list_TECComponents [j].m_id].valuesPBR[hour] = 0;

            //                        if (i < table_in.Rows.Count)
            //                        {
            //                            if (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull))
            //                                //if ((offsetLayout < m_tablePBRResponse.Columns.Count) && (!(table_in.Rows[i][offsetUDG + j * 3] is System.DBNull)))
            //                                m_dictValuesHours[m_list_TECComponents[j].m_id].valuesREC[hour] = (double)table_in.Rows[i][offsetUDG + j * 3];
            //                            else
            //                                m_dictValuesHours[m_list_TECComponents[j].m_id].valuesREC[hour] = 0;

            //                            if (!(table_in.Rows[i][offsetUDG + 1 + j * 3] is System.DBNull))
            //                                m_dictValuesHours[m_list_TECComponents[j].m_id].valuesISPER[hour] = (int)table_in.Rows[i][offsetUDG + 1 + j * 3];
            //                            else
            //                                ;

            //                            if (!(table_in.Rows[i][offsetUDG + 2 + j * 3] is System.DBNull))
            //                                m_dictValuesHours[m_list_TECComponents[j].m_id].valuesDIV[hour] = (double)table_in.Rows[i][offsetUDG + 2 + j * 3];
            //                            else
            //                                ;
            //                        }
            //                        else
            //                        {
            //                            m_dictValuesHours[m_list_TECComponents[j].m_id].valuesREC[hour] = 0;
            //                        }
            //                    }
            //                    catch
            //                    {
            //                    }
            //                    //j++;
            //                }
            //            }
            //            catch
            //            {
            //            }
            //        }
            //    }

            //    for (i = 1; i < 25; i++)
            //    {
            //        for (j = 0; j < m_list_TECComponents.Count; j++)
            //        {
            //            if (i == 1)
            //            {
            //                m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPBRe [i] = (m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPBR[i] + m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPBR[0]) / 2;
            //            }
            //            else
            //            {
            //                m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPBRe[i] = (m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPBR[i] + m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPBR[i - 1]) / 2;
            //            }

            //            m_dictValuesHours[m_list_TECComponents[j].m_id].valuesUDGe[i] = m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPBRe[i] + m_dictValuesHours[m_list_TECComponents[j].m_id].valuesREC[i];

            //            if (m_dictValuesHours[m_list_TECComponents[j].m_id].valuesISPER[i] == 1)
            //                m_dictValuesHours[m_list_TECComponents[j].m_id].valuesDiviation[i] = (m_dictValuesHours[m_list_TECComponents[j].m_id].valuesPBRe[i] +
            //                                                                                        m_dictValuesHours[m_list_TECComponents[j].m_id].valuesREC[i]) * m_dictValuesHours[m_list_TECComponents[j].m_id].valuesDIV[i] / 100;
            //            else
            //                m_dictValuesHours[m_list_TECComponents[j].m_id].valuesDiviation[i] = m_dictValuesHours[m_list_TECComponents[j].m_id].valuesDIV[i];
            //        }
            //    }
                
            //    return true;
            //}

            /*
            Статический метод в 'PanelTecViewBase'
            private DataTable restruct_table_pbrValues(DataTable table_in)
            {
                DataTable table_in_restruct = new DataTable();
                List<DataColumn> cols_data = new List<DataColumn>();
                DataRow[] dataRows;
                int i = -1, j = -1, k = -1;
                string nameFieldDate = "DATE_PBR"; // m_tec.m_strNamesField[(int)StatisticCommon.TEC.INDEX_NAME_FIELD.PBR_DATETIME]

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
                    //List<TG> list_TG = null;
                    List<TECComponent> list_TECComponents = null;
                    int count_comp = -1;

                        list_TECComponents = new List<TECComponent>();
                        for (i = 0; i < m_tec.list_TECComponents.Count; i++)
                        {
                            if ((m_tec.list_TECComponents[i].m_id > 100) && (m_tec.list_TECComponents[i].m_id < 500))
                                list_TECComponents.Add(m_tec.list_TECComponents[i]);
                            else
                                ;
                        }

                    //Преобразование таблицы
                    for (i = 0; i < table_in.Columns.Count; i++)
                    {
                        if ((!(table_in.Columns[i].ColumnName.Equals("ID_COMPONENT") == true))
                            && (!(table_in.Columns[i].ColumnName.Equals(nameFieldDate) == true))
                            //&& (!(table_in.Columns[i].ColumnName.Equals(m_tec.m_strNamesField[(int)StatisticCommon.TEC.INDEX_NAME_FIELD.PBR_NUMBER]) == true)))
                            && (!(table_in.Columns[i].ColumnName.Equals (@"PBR_NUMBER") == true)))
                        //if (!(table_in.Columns[i].ColumnName == "ID_COMPONENT"))
                        {
                            cols_data.Add(table_in.Columns[i]);
                        }
                        else
                            if ((table_in.Columns[i].ColumnName.IndexOf(nameFieldDate) > -1)
                                //|| (table_in.Columns[i].ColumnName.Equals(m_tec.m_strNamesField[(int)StatisticCommon.TEC.INDEX_NAME_FIELD.PBR_NUMBER]) == true))
                                || (table_in.Columns[i].ColumnName.Equals (@"PBR_NUMBER") == true))
                            {
                                table_in_restruct.Columns.Add(table_in.Columns[i].ColumnName, table_in.Columns[i].DataType);
                            }
                            else
                                ;
                    }

                        count_comp = list_TECComponents.Count;

                    for (i = 0; i < count_comp; i++)
                    {
                        for (j = 0; j < cols_data.Count; j++)
                        {
                            table_in_restruct.Columns.Add(cols_data[j].ColumnName, cols_data[j].DataType);
                                table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TECComponents[i].m_id;
                        }
                    }

                    //if (m_tec.m_strNamesField[(int)StatisticCommon.TEC.INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                        //table_in_restruct.Columns[m_tec.m_strNamesField[(int)StatisticCommon.TEC.INDEX_NAME_FIELD.PBR_NUMBER]].SetOrdinal(table_in_restruct.Columns.Count - 1);
                        table_in_restruct.Columns[@"PBR_NUMBER"].SetOrdinal(table_in_restruct.Columns.Count - 1);
                    //else
                    //    ;

                    List<DataRow[]> listDataRows = new List<DataRow[]>();

                    for (i = 0; i < count_comp; i++)
                    {
                            dataRows = table_in.Select("ID_COMPONENT=" + list_TECComponents[i].m_id);

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
                                //if (m_tec.m_strNamesField[(int)StatisticCommon.TEC.INDEX_NAME_FIELD.PBR_NUMBER].Length > 0)
                                    //table_in_restruct.Rows[indx_row][m_tec.m_strNamesField[(int)StatisticCommon.TEC.INDEX_NAME_FIELD.PBR_NUMBER]] = listDataRows[i][j][m_tec.m_strNamesField[(int)StatisticCommon.TEC.INDEX_NAME_FIELD.PBR_NUMBER]];
                                    table_in_restruct.Rows[indx_row][@"PBR_NUMBER"] = listDataRows[i][j][@"PBR_NUMBER"];
                                //else
                                //    ;
                            }
                            else
                                indx_row = k;

                            for (k = 0; k < cols_data.Count; k++)
                            {
                                    table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_TECComponents[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                            }
                        }
                    }
                }
                else
                    table_in_restruct = table_in;

                return table_in_restruct;
            }

            Статический метод в 'PanelTecViewBase'
            private DataTable restruct_table_adminValues(DataTable table_in)
            {
                DataTable table_in_restruct = new DataTable();
                List<DataColumn> cols_data = new List<DataColumn>();
                DataRow[] dataRows;
                int i = -1, j = -1, k = -1;
                string nameFieldDate = "DATE_ADMIN"; // m_tec.m_strNamesField[(int)StatisticCommon.TEC.INDEX_NAME_FIELD.ADMIN_DATETIME]

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

                        list_TECComponents = new List<TECComponent>();
                        for (i = 0; i < m_tec.list_TECComponents.Count; i++)
                        {
                            if ((m_tec.list_TECComponents[i].m_id > 100) && (m_tec.list_TECComponents[i].m_id < 500))
                                list_TECComponents.Add(m_tec.list_TECComponents[i]);
                            else
                                ;
                        }

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

                        count_comp = list_TECComponents.Count;

                    for (i = 0; i < count_comp; i++)
                    {
                        for (j = 0; j < cols_data.Count; j++)
                        {
                            table_in_restruct.Columns.Add(cols_data[j].ColumnName, cols_data[j].DataType);

                                table_in_restruct.Columns[table_in_restruct.Columns.Count - 1].ColumnName += "_" + list_TECComponents[i].m_id;
                        }
                    }

                    List<DataRow[]> listDataRows = new List<DataRow[]>();

                    for (i = 0; i < count_comp; i++)
                    {
                            dataRows = table_in.Select("ID_COMPONENT=" + list_TECComponents[i].m_id);
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
                                    table_in_restruct.Rows[indx_row][cols_data[k].ColumnName + "_" + list_TECComponents[i].m_id] = listDataRows[i][j][cols_data[k].ColumnName];
                            }
                        }
                    }
                }
                else
                    table_in_restruct = table_in;

                return table_in_restruct;
            }
            */
            private void ClearValues()
            {
                foreach (TECComponent g in m_list_TECComponents)
                {
                    for (int i = 0; i < m_dictValuesHours[g.m_id].valuesLastMinutesTM.Length; i++)
                        m_dictValuesHours[g.m_id].valuesLastMinutesTM[i] = 0.0;
                }
            }
        }
    }
}
