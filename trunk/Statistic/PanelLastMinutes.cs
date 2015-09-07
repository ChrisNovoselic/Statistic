﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Data;

using HClassLibrary;
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

            //Создание панели с дата/время
            m_panelDateTime = new PanelDateTime();
        }

        #endregion
    }

    public partial class PanelLastMinutes : PanelStatisticView
    {
        protected static AdminTS.TYPE_FIELDS s_typeFields = AdminTS.TYPE_FIELDS.DYNAMIC;
        
        //private List <Label> m_listLabelDateTime;
        private PanelDateTime m_panelDateTime;

        private ManualResetEvent m_evTimerCurrent;
        private System.Threading.Timer m_timerCurrent;

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

        /// <summary>
        /// Событие инициации процедуры изменения состояния
        /// </summary>
        private event DelegateObjectFunc EventChangeDateTime;

        public PanelLastMinutes(List<StatisticCommon.TEC> listTec, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr)
        {
            InitializeComponent();

            this.ColumnCount = listTec.Count + 1;
            this.RowCount = 1;

            float fPercentColDatetime = 8F;
            this.Controls.Add(m_panelDateTime, 0, 0);
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, fPercentColDatetime));

            int iCountSubColumns = 0;
            
            for (int i = 0; i < listTec.Count; i++)
            {
                this.Controls.Add(new PanelTecLastMinutes(listTec[i], fErrRep, fWarRep, fActRep, fRepClr), i + 1, 0);
                EventChangeDateTime += new DelegateObjectFunc(((PanelTecLastMinutes)this.Controls [i + 1]).OnEventChangeDateTime);
                iCountSubColumns += ((PanelTecLastMinutes)this.Controls [i + 1]).CountTECComponent; //Слева столбец дата/время
            }

            initializeLayoutStyle(iCountSubColumns);

            m_msecPeriodUpdate = 60 * 60 * 1000;
        }

        public PanelLastMinutes(IContainer container, List<StatisticCommon.TEC> listTec, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr)
            : this(listTec, fErrRep, fWarRep, fActRep, fRepClr)
        {
            container.Add(this);
        }

        public override void Start()
        {
            base.Start();
            
            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecLastMinutes)
                {
                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecLastMinutes)ctrl).m_id_tec))
                        ((PanelTecLastMinutes)ctrl).Start();
                    //else ;
                    i++;
                }
                else
                    ;
            }

            //Милисекунды до первого запуска функции таймера
            Int32 msecUpdate = -1;
            //Милисекунды от начала часа
            msecUpdate = DateTime.Now.Minute * 60 * 1000 + DateTime.Now.Second * 1000;
            msecUpdate = 60 * 60 * 1000 - msecUpdate;
            msecUpdate += 666666;

            m_evTimerCurrent = new ManualResetEvent(true);
            m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, (Int64)msecUpdate, Timeout.Infinite);
            //Для отладки
            //m_timerCurrent = new System.Threading.Timer(new TimerCallback(TimerCurrent_Tick), m_evTimerCurrent, 0, m_msecPeriodUpdate - 1);

            //setDatetimePicker(m_panelDateTime.m_dtprDate.Value - HAdmin.GetOffsetOfCurrentTimeZone());
        }

        public override void Stop()
        {
            int i = 0;

            if (!(m_evTimerCurrent == null)) m_evTimerCurrent.Reset(); else ;
            if (!(m_timerCurrent == null)) m_timerCurrent.Dispose(); else ;

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecLastMinutes)
                {
                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecLastMinutes)ctrl).m_id_tec))
                        ((PanelTecLastMinutes)ctrl).Stop();
                    //else ;
                    i++;
                }
                else
                    ;
            }

            base.Stop ();
        }

        protected override void initTableHourRows()
        {
            //Перестраиваем "шкалу" времени
            //m_panelDateTime.initTableHourRows(); ??? Панель сама инициирует изменение даты, т.к. 'календарь' принадлежит ей
        }

        //???
        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            if (!(this.ColumnStyles.Count == 1))
                throw new Exception(@"PanelLastMinutes::initializeLayoutStyle () - ...");
            else
                ;

            float fPercentColDatetime = this.ColumnStyles[0].Width;
            
            //Размеры столбцов после создания столбцов, т.к.
            //кол-во "подстолбцов" в столбцах до их создания неизвестно
            for (int i = 0; i < this.ColumnCount - 1; i++)
            {
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, ((float)100 - fPercentColDatetime) / cols * ((PanelTecLastMinutes)this.Controls[i + 1]).CountTECComponent));
            }
        }

        public override bool Activate(bool active)
        {
            bool bRes = base.Activate (active);
            
            if (bRes == false)
                return bRes;
            else
                ;

            int i = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is PanelTecLastMinutes)
                {
                    //if ((HAdmin.DEBUG_ID_TEC == -1) || (HAdmin.DEBUG_ID_TEC == ((PanelTecLastMinutes)ctrl).m_id_tec))
                        ((PanelTecLastMinutes)ctrl).Activate(active);
                    //else ;
                    i++;
                }
                else
                    ;
            }

            if (Actived == true)
                EventChangeDateTime (m_panelDateTime.m_dtprDate.Value);
            else
                ;

            return bRes;
        }

        private void setDatetimePicker(DateTime dtSet)
        {
            m_panelDateTime.m_dtprDate.Value = dtSet;

            initTableHourRows ();
        }

        private void TimerCurrent_Tick (object obj) {
            if (Actived == true)
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new DelegateDateFunc(setDatetimePicker), HAdmin.ToMoscowTimeZone(DateTime.Now));
                else
                    Logging.Logg().Error(@"PanelLastMinutes::TimerCurrent_Tick () - ... BeginInvoke (setDatetimePicker) - ...", Logging.INDEX_MESSAGE.D_001);
            else
                ;

            if (! (m_timerCurrent == null))
                m_timerCurrent.Change (m_msecPeriodUpdate, Timeout.Infinite);
            else
                ;
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

                this.m_dtprDate = new DateTimePicker();
                this.m_dtprDate.Dock = DockStyle.Fill;
                //this.m_dtprDate.ValueChanged += new EventHandler(((PanelLastMinutes)Parent).OnDateTimeValueChanged);
                m_dtprDate.Value = HAdmin.ToMoscowTimeZone (DateTime.Now);
                this.m_dtprDate.ValueChanged += new EventHandler(OnDateTimeValueChanged);

                this.m_btnUpdate = new Button ();
                this.m_btnUpdate.Dock = DockStyle.Fill;
                this.m_btnUpdate.Text = @"Обнов.";
                this.m_btnUpdate.Click += new EventHandler(OnDateTimeValueChanged);
            }

            #endregion
        }

        private partial class PanelDateTime : PanelStatisticWithTableHourRows
        {
            public DateTimePicker m_dtprDate;
            private Dictionary<int, Label> m_dictLabelTime;

            private Button m_btnUpdate;

            public PanelDateTime() : base ()
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

                //int cntHours = 

                this.Dock = DockStyle.Fill;
                this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle
                this.RowCount = 24 + COUNT_FIXED_ROWS;

                //Добавить дату
                //Label lblDate = HLabel.createLabel(dtNow.ToString (@"dd.MM.yyyy"), PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_TEC]);
                this.Controls.Add(m_dtprDate, 0, 0);
                //this.SetRowSpan(m_dtprDate, COUNT_FIXED_ROWS);

                m_dictLabelTime = new Dictionary<int,Label> ();

                //Добавить кнопку принудительного обновления
                this.Controls.Add(this.m_btnUpdate, 0, 1);
                //this.SetRowSpan(this.m_btnUpdate, COUNT_FIXED_ROWS);

                initTableHourRows();

                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 6F));

                //m_dtprDate.Value = DateTime.Now; Иначе Парент == null ???
            }

            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                throw new NotImplementedException();
            }

            protected override void initTableHourRows()
            {
                DateTime dt = m_dtprDate.Value.Date;
                bool bSeason = false
                    , bChangedCountRows = false;
                int h = -1
                    , cntHours = -1;

                if (dt.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0)
                {
                    bSeason = true;
                    cntHours = 25;
                }
                else
                    cntHours = 24;

                if (m_dictLabelTime.Count == 0)
                {
                    for (h = 0; h < (cntHours + COUNT_FIXED_ROWS - 0); h++)
                    {
                        this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                    }

                    for (h = 0; h < cntHours; h++)
                    {
                        m_dictLabelTime[h] = HLabel.createLabel(@"--:--", PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.DATETIME]);
                        this.Controls.Add(m_dictLabelTime[h], 0, (h) + COUNT_FIXED_ROWS);                        
                    }                    

                    bChangedCountRows = true;
                }
                else
                {
                    if (bSeason == true)
                    {
                        if (m_dictLabelTime.Count < cntHours)
                        {
                            m_dictLabelTime.Add(24, HLabel.createLabel(dt.ToString(@"--:--"), PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.DATETIME]));
                            this.Controls.Add(m_dictLabelTime[24], 0, 24 + COUNT_FIXED_ROWS);

                            this.RowStyles.Add(PanelLastMinutes.fRowStyle());

                            bChangedCountRows = true;
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                        if (m_dictLabelTime.Count > cntHours)
                        {
                            this.Controls.Remove(m_dictLabelTime[24]);
                            m_dictLabelTime.Remove(cntHours);

                            this.RowStyles.RemoveAt(this.RowStyles.Count - 1);

                            bChangedCountRows = true;
                        }
                        else
                        {
                        }
                    }
                }

                if (bChangedCountRows == true)
                {
                    DateTime dtSel = m_dtprDate.Value.Date;
                    int offset = 0;
                    dtSel = dtSel.AddMinutes(59);
                    for (h = 0; h < m_dictLabelTime.Count; h++)
                    {
                        m_dictLabelTime[h].Text = dtSel.ToString(@"HH:mm");
                        if (bSeason == true)
                            if (!(h == HAdmin.SeasonDateTime.Hour))
                            {
                                dtSel = dtSel.AddHours(1);

                                offset = HAdmin.GetSeasonHourOffset(dtSel.Date, h);
                                if ((offset > 0) && ((h - 1) == HAdmin.SeasonDateTime.Hour))
                                {
                                    m_dictLabelTime[h].Text += @"*";
                                }
                                else
                                    ;
                            }
                            else
                                ;
                        else
                            dtSel = dtSel.AddHours(1);
                    }
                }
                else
                {
                }
            }

            private void OnDateTimeValueChanged(object obj, EventArgs ev)
            {
                DateTime dt = m_dtprDate.Value;
                ((PanelLastMinutes)Parent).EventChangeDateTime(dt);

                initTableHourRows();
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

        public partial class PanelTecLastMinutes : HPanelCommon
        {
            public int m_id_tec { get { return m_tecView.m_tec.m_id; } }

            List<TECComponentBase> m_list_TECComponents;
            public int CountTECComponent { get { return m_list_TECComponents.Count; } }

            //Для отображения значений
            private List <Dictionary<int, Label>> m_listDictLabelVal;
            private List <Dictionary<int, ToolTip>> m_listDictToolTip;

            //private Dictionary<int, TecView.valuesTECComponent> m_dictValuesHours;
            TecView m_tecView;

            public PanelTecLastMinutes(StatisticCommon.TEC tec, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc fRepClr)
                : base (-1, -1)
            {
                InitializeComponent();

                m_tecView = new TecView(TecView.TYPE_PANEL.LAST_MINUTES, -1, -1);

                //Признаки для регистрации соединения с необходимыми источниками данных
                HMark markQueries = new HMark();
                markQueries.Marked((int)CONN_SETT_TYPE.ADMIN);
                markQueries.Marked((int)CONN_SETT_TYPE.PBR);
                markQueries.Marked((int)CONN_SETT_TYPE.DATA_SOTIASSO);

                m_tecView.InitTEC (new List <TEC> () { tec }, markQueries);
                m_tecView.SetDelegateReport(fErrRep, fWarRep, fActRep, fRepClr);

                m_tecView.updateGUI_LastMinutes = new DelegateFunc(showLastMinutesTM);

                Initialize();
            }

            public PanelTecLastMinutes(IContainer container, StatisticCommon.TEC tec, DelegateStringFunc fErrRep, DelegateStringFunc fWarRep, DelegateStringFunc fActRep, DelegateBoolFunc frepClr)
                : this(tec, fErrRep, fWarRep, fActRep, frepClr)
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
                m_list_TECComponents = new List<TECComponentBase> ();
               
                this.Dock = DockStyle.Fill;
                this.BorderStyle = BorderStyle.None; //BorderStyle.FixedSingle
                this.RowCount = ((HAdmin.ToMoscowTimeZone(DateTime.Now)).Date.Equals(HAdmin.SeasonDateTime.Date) ? 25 : 24) + COUNT_FIXED_ROWS;

                for (i = 0; i < this.RowCount; i++)
                {
                    this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                }

                //Добавить наименование станции
                Label lblNameTEC = HLabel.createLabel(m_tecView.m_tec.name_shr, PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_TEC]);
                this.Controls.Add(lblNameTEC, 0, 0);

                foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
                    if ((g.m_id > 100) && (g.m_id < 500))
                    {
                        //Добавить наименование ГТП
                        this.Controls.Add(HLabel.createLabel(g.name_shr.Split(' ')[1], PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.NAME_COMPONENT]), CountTECComponent, COUNT_FIXED_ROWS - 1);

                        //Добавить компонент ТЭЦ (ГТП)
                        m_list_TECComponents.Add(g);
                    }
                    else
                        ;

                //initTableHourRows();

                for (i = 0; i < CountTECComponent; i++)
                {
                    this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / CountTECComponent));
                }

                if (CountTECComponent > 0)
                    this.SetColumnSpan(lblNameTEC, CountTECComponent);
                else
                    ;
            }

            public override void Start()
            {
                base.Start ();

                if (m_tecView.IsStarted == true)
                    return;
                else
                    ;

                m_tecView.Start ();
            }

            public override void Stop()
            {
                if (m_tecView.IsStarted == false)
                    return;
                else
                    ;

                m_tecView.Stop();

                m_tecView.ReportClear(true);

                base.Stop ();
            }

            private void ChangeState()
            {
                //m_tecView.m_curDate = ... получено при обработке события
                //m_tecView.m_curDate = m_tecView.m_curDate.Add(-HAdmin.GetUTCOffsetOfCurrentTimeZone ());

                m_tecView.ChangeState ();
            }

            public override bool Activate(bool active)
            {
                bool bRes = base.Activate (active);

                m_tecView.Activate(active);

                if (m_tecView.Actived == true)
                {
                    //ChangeState();
                }
                else
                {
                    m_tecView.ClearStates ();
                }

                return bRes;
            }
            /// <summary>
            /// Обработчие события - изменение значения свойства
            /// </summary>
            /// <param name="obj"></param>
            public void OnEventChangeDateTime (object obj) {
                m_tecView.m_curDate = (DateTime)obj;
                //m_tecView.m_curDate = new DateTime(((DateTime)obj).Year
                //                                    , ((DateTime)obj).Month
                //                                    , ((DateTime)obj).Day
                //                                    , ((DateTime)obj).Hour
                //                                    , ((DateTime)obj).Minute
                //                                    , ((DateTime)obj).Millisecond
                //                                    , DateTimeKind.Unspecified);

                initTableHourRows();

                ChangeState ();
            }

            private void addRow(int indx)
            {
                //Память под ячейки со значениями
                m_listDictLabelVal.Add(new Dictionary<int, Label>());
                m_listDictToolTip.Add(new Dictionary<int, ToolTip>());

                int col = 0;
                foreach (TECComponent g in m_tecView.m_tec.list_TECComponents)
                {
                    if ((g.m_id > 100) && (g.m_id < 500))
                    {
                        m_listDictLabelVal[indx].Add(g.m_id, HLabel.createLabel(0.ToString(@"F2"), PanelLastMinutes.s_arLabelStyles[(int)INDEX_LABEL.VALUE_COMPONENT]));

                        m_listDictToolTip[indx].Add(g.m_id, new ToolTip());
                        m_listDictToolTip[indx][g.m_id].IsBalloon = true;
                        m_listDictToolTip[indx][g.m_id].ShowAlways = true;
                        m_listDictToolTip[indx][g.m_id].SetToolTip(m_listDictLabelVal[indx][g.m_id], Hd2PercentControl.StringToolTipEmpty);

                        this.Controls.Add(m_listDictLabelVal[indx][g.m_id], col++, indx + COUNT_FIXED_ROWS);

                        this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                    }
                    else
                        ;
                }
            }

            private void initTableHourRows()
            {
                bool bSeason = false
                    , bChangedCountRows = false;
                int hour = -1
                    , cntHours = -1;

                if (! (m_tecView.m_curDate.Year == 1))
                {
                    if (m_tecView.m_curDate.Date.CompareTo(HAdmin.SeasonDateTime.Date) == 0)
                    {
                        bSeason = true;
                        cntHours = 25;
                    }
                    else
                        cntHours = 24;

                    if ((m_listDictLabelVal == null) || (m_listDictToolTip == null))
                    {
                        m_listDictLabelVal = new List<Dictionary<int, Label>>();
                        m_listDictToolTip = new List<Dictionary<int, ToolTip>>();
                    }
                    else
                        ;

                    if (m_listDictLabelVal.Count == 0)
                    {
                        for (hour = 0; hour < cntHours; hour++)
                        {
                            addRow(hour);
                        }

                        //for (hour = 0; hour < COUNT_FIXED_ROWS; hour++)
                        //{
                        //    this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                        //}
                    }
                    else
                    {
                        if (!(m_listDictLabelVal.Count == cntHours))
                        {
                            if (m_listDictLabelVal.Count > cntHours)
                            {
                                foreach (Label lbl in m_listDictLabelVal[cntHours].Values)
                                {
                                    this.Controls.Remove(lbl);
                                }

                                m_listDictLabelVal.RemoveAt(cntHours);
                                m_listDictToolTip.RemoveAt(cntHours);

                                this.RowStyles.RemoveAt(this.RowStyles.Count - 1);
                            }
                            else
                            {
                                if (m_listDictLabelVal.Count < cntHours)
                                {
                                    addRow(24);

                                    this.RowStyles.Add(PanelLastMinutes.fRowStyle());
                                }
                                else
                                {
                                }
                            }
                        }
                        else
                        {
                        }
                    }
                }
                else
                    ; //Дата/время не известны
            }

            private void showLastMinutesTM()
            {
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new DelegateFunc(ShowLastMinutesTM));
                else
                    Logging.Logg().Error(@"PanelTecLastMinutes::showLastMinutesTM () - ... BeginInvoke (ShowLastMinutesTM) - ...", Logging.INDEX_MESSAGE.D_001);
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
                    for (int hour = 1; hour < m_listDictLabelVal.Count + 1; hour++)
                    {
                        clrBackColor = s_clrBakColorLabelVal;
                        strToolTip = string.Empty;

                        bool bPmin = false;
                        if (m_tecView.m_tec.m_id == 5) bPmin = true; else ;
                        strToolTip = d2PercentControl.Calculate(m_tecView.m_dictValuesTECComponent[hour - 0][g.m_id], bPmin, out warn);

                        m_listDictToolTip[hour - 1][g.m_id].SetToolTip(m_listDictLabelVal[hour - 1][g.m_id], strToolTip);

                        if (m_tecView.m_dictValuesTECComponent[hour - 0][g.m_id].valuesLastMinutesTM > 1)
                        {
                            if ((! (warn == 0)) &&
                                (m_tecView.m_dictValuesTECComponent[hour - 0][g.m_id].valuesLastMinutesTM > 1))
                            {
                                cntWarn++;
                                if (cntWarn > 3)
                                    clrBackColor = Color.Red;
                                else
                                    clrBackColor = Color.Yellow;                                
                            }
                            else
                                cntWarn = 0;

                            if (cntWarn > 0) {
                                //strWarn = cntWarn + @":";
                            }
                            else
                                strWarn = string.Empty;

                            m_listDictLabelVal[hour - 1][g.m_id].Text = strWarn + m_tecView.m_dictValuesTECComponent[hour - 0][g.m_id].valuesLastMinutesTM.ToString(@"F2");
                        }
                        else
                            m_listDictLabelVal[hour - 1][g.m_id].Text = 0.ToString(@"F0");

                        m_listDictLabelVal[hour - 1][g.m_id].BackColor = clrBackColor;
                    }
                }
            }
        }
    }
}
