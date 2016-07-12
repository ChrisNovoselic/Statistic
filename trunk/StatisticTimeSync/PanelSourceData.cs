﻿using System;
using System.Collections.Generic;
using System.ComponentModel; //BacgroundWorker
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms; //TableLayoutPanel
using System.Data; //DataTable
using System.Data.Common; //DbConnection
using HClassLibrary;
using StatisticCommon;

namespace StatisticTimeSync
{
    /// <summary>
    /// Главная панель класса
    /// </summary>
    public partial class PanelSourceData : PanelStatistic
    {
        /// <summary>
        /// Работа с компонентами панели
        /// </summary>
        public partial class PanelGetDate : HPanelCommon
        {
            /// <summary>
            /// Перечисление - идентификаторы сообщений между панелями
            /// </summary>
            public enum ID_ASKED_DATAHOST
            {
                CONN_SETT
                ,
            }
            /// <summary>
            /// Перечисление - индексы для обращения к элементам массива (виды объектов дата/время)
            /// </summary>
            private enum INDEX_DATETME
            {
                UNKNOWN = -1
                , METKA, ETALON,
                SERVER
                    , COUNT
            }
            /// <summary>
            /// Событие - запрос на получение данных
            /// </summary>
            public event DelegateObjectFunc EvtAskedData;
            /// <summary>
            /// Тип делегата для обработки события - получение эталонного даты/время
            /// </summary>
            public DelegateDateFunc DelegateEtalonGetDate;
            /// <summary>
            /// Объект синхронизации доступа к панели одного источника
            /// </summary>
            private object m_lockGetDate;
            /// <summary>
            /// Объект для опроса текущего времени источника
            /// </summary>
            private HGetDate m_getDate;
            /// <summary>
            /// Массив объектов дата/время для хранения всех видов таких объектов
            /// </summary>
            private DateTime[] m_arDateTime;
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public PanelGetDate()
                : base(-1, -1)
            {
                initialize();
            }
            /// <summary>
            /// Конструктор - дополнительный (с параметром)
            /// </summary>
            /// <param name="container">Объект - родительский контейнер</param>
            public PanelGetDate(IContainer container)
                : base(container, -1, -1)
            {
                container.Add(this);

                initialize();
            }
            /// <summary>
            /// Инициализация объектов класса
            ///  (могла быть выполнена в конструкторе, но выделена в метод, т.к. имеется > 1 конструктора)
            /// </summary>
            private void initialize()
            {
                InitializeComponent();

                m_lockGetDate = new object();

                m_arDateTime = new DateTime[(int)INDEX_DATETME.COUNT] { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };
            }

            /// <summary>
            /// Обработчик события - изменение состояния признака вкл/выкл составной элемент управления
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            private void checkBoxTurnOn_CheckedChanged(object obj, EventArgs ev)
            {
                this.m_comboBoxSourceData.Enabled = !m_checkBoxTurnOn.Checked;

                if (m_checkBoxTurnOn.Checked == true)
                {
                    Activate(true);
                    //Start
                    //Спросить параметры соединения
                    IAsyncResult iar = BeginInvoke(new DelegateFunc(queryConnSett));
                }
                else
                    Activate(false);
            }

            /// <summary>
            /// Запрос на связь с базой
            /// </summary>
            private void queryConnSett()
            {
                try
                {
                    EvtAskedData(new EventArgsDataHost(-1, (int)ID_ASKED_DATAHOST.CONN_SETT, new object[] { this }));
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelSourceData.PanelGetDate::queryConnSett", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// Обработчик события - изменение выбранного элемента в списке
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие (список с источниками данных)</param>
            /// <param name="ev">Аргумент события</param>
            private void comboBoxSourceData_SelectedIndexChanged(object obj, EventArgs ev)
            {
                if (m_comboBoxSourceData.SelectedIndex > 0)
                    m_checkBoxTurnOn.Enabled = true;
                else
                    m_checkBoxTurnOn.Enabled = false;
            }

            /// <summary>
            /// добавления значений в комбобокс
            /// </summary>
            /// <param name="desc">Строка - описание источника данных</param>
            public void AddSourceData(string desc)
            {
                m_comboBoxSourceData.Items.Add(desc);
            }

            /// <summary>
            /// Возвратить короткое нименование выбранного источника данных
            /// </summary>
            /// <returns>Стпрка - короткое наименование источника данных</returns>
            public string GetNameShrSelectedSourceData()
            {
                return (string)Invoke(new Func<string>(() => m_comboBoxSourceData.SelectedItem.ToString()));
            }

            /// <summary>
            /// Обработчик события - получение данных (по запросу) от сервера
            /// </summary>
            /// <param name="ev">Аргумент события - полученные данные</param>
            public void OnEvtDataRecievedHost(EventArgsDataHost ev)
            {
                switch (ev.id_detail)
                {
                    case (int)ID_ASKED_DATAHOST.CONN_SETT: //получены параметры соединения
                        //Установить соедиение - создать объект
                        m_getDate = new HGetDate((ConnectionSettings)ev.par[0], recievedGetDate, errorGetDate);
                        //Установить соедиение - запустить поток обработки(отправления) запросов
                        m_getDate.StartDbInterfaces();
                        //Установить соедиение - начать обработку запросов
                        m_getDate.Start();
                        break;
                    default:
                        break;
                }
            }

            /// <summary>
            /// Обработать полученную дату/время
            /// </summary>
            /// <param name="date">Дата/время для обработки</param>
            private void recievedGetDate(DateTime date)
            {
                //Console.WriteLine (date.Kind.ToString ());
                m_arDateTime[(int)INDEX_DATETME.SERVER] = date/*.ToUniversalTime ()*/;
                //Обновить время сервера БД
                this.BeginInvoke(new DelegateFunc(updateGetDate));
                //Если панель с ЭТАЛОНным сервером БД
                if ((m_arDateTime[(int)INDEX_DATETME.SERVER].Equals(DateTime.MinValue) == false)
                    && (isEtalon == true))
                    DelegateEtalonGetDate(date);
                else
                    ;
            }

            /// <summary>
            /// Обновляет разницу времени сервера с БД
            /// </summary>
            /// <param name="date"></param>
            private void recievedEtalonDate(DateTime date)
            {
                m_arDateTime[(int)INDEX_DATETME.ETALON] = date;
                //Обновить разницу сервера БД с эталонным сервером БД
                this.BeginInvoke(new DelegateFunc(updateDiffDate));
            }

            private void errorGetDate()
            {
                //throw new NotImplementedException ();
            }

            /// <summary>
            /// run recievedGetDate
            /// </summary>
            private void updateGetDate()
            {
                string textTime = string.Empty;

                if (m_arDateTime[(int)INDEX_DATETME.SERVER].Equals(DateTime.MinValue) == false)
                {
                    textTime = m_arDateTime[(int)INDEX_DATETME.SERVER].ToString(@"HH:mm:ss.fff");
                }
                else
                {
                    //Признак останова (деактивации)
                    textTime = @"--:--:--.---";

                    m_arDateTime[(int)INDEX_DATETME.METKA] =
                    m_arDateTime[(int)INDEX_DATETME.ETALON] = DateTime.MinValue;
                    updateDiffDate();
                }
                m_labelTime.Text = textTime;
                m_labelTime.Refresh();
            }

            /// <summary>
            /// run recievedEtalonDate
            /// </summary>
            public void updateDiffDate()
            {
                string textDiff = string.Empty;
                double msecDiff = -1F;

                if (m_arDateTime[(int)INDEX_DATETME.ETALON].Equals(DateTime.MinValue) == false)
                    if (m_arDateTime[(int)INDEX_DATETME.SERVER].Equals(DateTime.MinValue) == false)
                    {
                        msecDiff = (m_arDateTime[(int)INDEX_DATETME.ETALON] - m_arDateTime[(int)INDEX_DATETME.SERVER]).TotalMilliseconds;
                        if (Math.Abs(msecDiff) < (1 * 60 * 60 * 1000))
                            ;
                        else
                            m_arDateTime[(int)INDEX_DATETME.SERVER] = m_arDateTime[(int)INDEX_DATETME.SERVER].AddHours(-3);

                        textDiff = ((m_arDateTime[(int)INDEX_DATETME.ETALON] - m_arDateTime[(int)INDEX_DATETME.SERVER]).TotalMilliseconds / 1000).ToString();

                    }
                    else
                        //Признак останова (деактивации)
                        textDiff = @"--.---";
                else
                    //Признак отсутствия эталонного времени
                    textDiff = @"--.---";

                m_labelDiff.Text = textDiff;
                m_labelDiff.Refresh();
            }

            /// <summary>
            /// Принимает сигнал, инициирующий посылку запроса даты/времени серверу БД
            /// </summary>
            /// <param name="obj">метка дата/время - начало запроса</param>
            public void OnEvtGetDate(object obj)
            {
                if (!(obj == null))
                {
                    m_arDateTime[(int)INDEX_DATETME.METKA] = (DateTime)obj;
                    m_arDateTime[(int)INDEX_DATETME.ETALON] =
                    m_arDateTime[(int)INDEX_DATETME.ETALON] = DateTime.MinValue;
                }
                else
                    ;

                lock (m_lockGetDate)
                {
                    if (!(m_getDate == null))
                        m_getDate.GetDate();
                    else
                        ;
                }
            }

            /// <summary>
            /// Получает сигнал с эталонной датой/временем
            /// </summary>
            /// <param name="date">эталонная дата/время</param>
            public void OnEvtEtalonDate(DateTime date)
            {
                recievedEtalonDate(date);
            }

            /// <summary>
            /// Остановить панель
            /// </summary>
            public override void Stop()
            {
                stop();

                base.Stop();
            }
            /// <summary>
            /// Остановить панель
            /// </summary>
            private void stop()
            {
                //Stop
                //Разорвать соедиенние
                lock (m_lockGetDate)
                {
                    if (!(m_getDate == null))
                    {
                        m_getDate.Stop();
                        m_getDate = null;
                    }
                    else
                        ;
                }
            }

            private bool isEtalon { get { return !(DelegateEtalonGetDate == null); } }

            /// <summary>
            /// Активировать панель
            /// </summary>
            /// <param name="activated">Признак активации</param>
            /// <returns>Признак изменения состояния панели после выполнения</returns>
            public override bool Activate(bool activated)
            {
                bool bRes = base.Activate(activated);

                if (bRes == true)
                    if (activated == true)
                        if (m_checkBoxTurnOn.Checked == true)
                        {
                            //Start
                            //Спросить параметры соединения
                            IAsyncResult iar = BeginInvoke(new DelegateFunc(queryConnSett));
                        }
                        else
                            ;
                    else
                    {
                        //Stop
                        stop();

                        //Признак деактивации
                        recievedGetDate(DateTime.MinValue);
                        recievedEtalonDate(DateTime.MinValue);
                        //!!! только, если панель эталонная! - сообщить всем клиентам, что эталонное время недоступно
                        if (isEtalon == true)
                            DelegateEtalonGetDate(DateTime.MinValue);
                        else
                            ;
                    }
                else
                    ;

                return bRes;
            }

            /// <summary>
            /// Включить групповой элемент управления для опроса/сравнения установленной даты/времени источника данных
            /// </summary>
            /// <param name="indx">Индекс элемента управления</param>
            public void TurnOn(int indx)
            {
                if (m_checkBoxTurnOn.Checked == false)
                {
                    if (indx > 0)
                    {
                        m_comboBoxSourceData.SelectedIndex = indx;
                        m_checkBoxTurnOn.Checked = true;
                    }
                    else
                        ;
                }
                else
                {
                    //Ничего не делаем...
                }
            }

            /// <summary>
            /// Включть/выключить панель
            /// </summary>
            /// <param name="indx">Индекс источника данных (при включении)</param>
            public void TurnOff(int indx = -1)
            {
                if (m_checkBoxTurnOn.Checked == true)
                {
                    m_checkBoxTurnOn.Checked = false;

                    switch (indx)
                    {
                        case -1:
                            break;
                        default:
                            m_comboBoxSourceData.SelectedIndex = indx;
                            break;
                    }
                }
                else
                {
                    //Ничего не делаем...
                }
            }

            /// <summary>
            ///  Выбрать источник данных из списка
            /// </summary>
            /// <param name="indx">Индекс источника данных</param>
            public void Select(int indx)
            {
                if (m_checkBoxTurnOn.Checked == false)
                {
                    if (indx > 0)
                    {
                        m_comboBoxSourceData.SelectedIndex = indx;

                        //m_comboBoxSourceData.SelectedIndex = indx;
                        //m_checkBoxTurnOn.Checked = true;
                    }
                    else
                        ;
                }
                else
                {
                    //Ничего не делаем...
                }
            }
        }

        /// <summary>
        /// Инициализация компонентов панели
        /// </summary>
        partial class PanelGetDate
        {
            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                initializeLayoutStyleEvenly(cols, rows);
            }

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
                this.Dock = System.Windows.Forms.DockStyle.Fill;


                this.m_checkBoxTurnOn = new System.Windows.Forms.CheckBox();
                this.m_comboBoxSourceData = new System.Windows.Forms.ComboBox();
                this.m_labelTime = new System.Windows.Forms.Label();
                this.m_labelDiff = new System.Windows.Forms.Label();


                this.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;

                initializeLayoutStyle(2, 2);

                this.Controls.Add(m_checkBoxTurnOn, 0, 1);
                this.Controls.Add(m_comboBoxSourceData, 0, 0);
                this.Controls.Add(m_labelDiff, 1, 1);
                this.Controls.Add(m_labelTime, 1, 0);

                this.SuspendLayout();
                // 
                // m_checkBoxTurnOn
                // 
                this.m_checkBoxTurnOn.AutoSize = true;
                //this.m_checkBoxTurnOn.Location = new System.Drawing.Point(0, 0);
                this.m_checkBoxTurnOn.Name = "m_checkBoxTurnOn";
                //this.m_checkBoxTurnOn.Size = new System.Drawing.Size(104, 24);
                this.m_checkBoxTurnOn.TabIndex = 0;
                this.m_checkBoxTurnOn.Text = "Включено";
                this.m_checkBoxTurnOn.UseVisualStyleBackColor = true;
                this.m_checkBoxTurnOn.CheckedChanged += new System.EventHandler(checkBoxTurnOn_CheckedChanged);
                this.m_checkBoxTurnOn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
                this.m_checkBoxTurnOn.Enabled = false;
                // 
                // m_comboBoxSourceData
                // 
                this.m_comboBoxSourceData.FormattingEnabled = true;
                //this.m_comboBoxSourceData.Location = new System.Drawing.Point(0, 0);
                this.m_comboBoxSourceData.Name = "m_comboBoxSourceData";
                this.m_comboBoxSourceData.Size = new System.Drawing.Size(121, 21);
                this.m_comboBoxSourceData.TabIndex = 0;
                this.m_comboBoxSourceData.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                this.m_comboBoxSourceData.Enabled = !this.m_checkBoxTurnOn.Checked;
                this.m_comboBoxSourceData.Dock = System.Windows.Forms.DockStyle.Fill;
                this.m_comboBoxSourceData.SelectedIndexChanged += new System.EventHandler(comboBoxSourceData_SelectedIndexChanged);
                this.m_comboBoxSourceData.Items.Add(@"[Нет]");
                this.m_comboBoxSourceData.SelectedIndex = 0;
                // 
                // m_labelTime
                // 
                this.m_labelTime.AutoSize = true;
                //this.m_labelTime.Location = new System.Drawing.Point(0, 0);
                //this.m_labelTime.Size = new System.Drawing.Size(100, 23);
                this.m_labelTime.Dock = System.Windows.Forms.DockStyle.Fill;
                this.m_labelTime.Name = "m_labelTime";
                this.m_labelTime.TabIndex = 0;
                this.m_labelTime.Text = "HH:mm:ss.ccc";
                this.m_labelTime.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                this.m_labelTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                // 
                // m_labelDiff
                // 
                this.m_labelDiff.AutoSize = true;
                //this.m_labelDiff.Location = new System.Drawing.Point(0, 0);
                //this.m_labelDiff.Size = new System.Drawing.Size(100, 23);
                this.m_labelDiff.Dock = System.Windows.Forms.DockStyle.Fill;
                this.m_labelDiff.Name = "m_labelDiff";
                this.m_labelDiff.TabIndex = 0;
                this.m_labelDiff.Text = "HH:mm:ss.ccc";
                this.m_labelDiff.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                this.m_labelDiff.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

                this.ResumeLayout(false);
            }

            #endregion

            private System.Windows.Forms.CheckBox m_checkBoxTurnOn;
            private System.Windows.Forms.ComboBox m_comboBoxSourceData;
            public System.Windows.Forms.Label m_labelTime;
            public System.Windows.Forms.Label m_labelDiff;
        }

        delegate void DelAddPan(object[] ar_pan);

        DelAddPan m_delAddPan;

        HandlerSourceData HSD = null;

        public PanelSourceData()
        {
            
            m_delAddPan = new DelAddPan(addPanel);
            initialize();
            HSD = new HandlerSourceData(this);
            HSD.Start();
            HSD.Activate(true);
            HSD.Push(null, new object[] {new object[] { new object[] {(int)HandlerSourceData.StatesMachine.AddControl} } });


        }

        public PanelSourceData(IContainer container)
        {
            container.Add(this);

            HSD = new HandlerSourceData(this);

            HSD.Activate(true);
            HSD.Start();
            HSD.AddState((int)HandlerSourceData.StatesMachine.AddControl);

            initialize();
        }

        private void initialize()
        {
            InitializeComponent();
        }

        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            base.Start();

        }        

        public override void Stop()
        {
            base.Stop();
        }

        public override bool Activate(bool activated)
        {
            bool bRes = base.Activate(activated);            

            return bRes;
        }

        private void addPanel(object[] obj)
        {
            PanelGetDate[] ar_pan = (PanelGetDate[])obj;
            //this.SuspendLayout();

            this.Controls.Add(ar_pan[0], 0, 0);

            int indx = -1
                , col = -1
                , row = -1;
            for (int i = 1; i < ar_pan.Length; i++)
            {
                indx = i;
                //if (! (indx < this.RowCount))
                indx += (int)(indx / this.RowCount);
                //else ;

                col = (int)(indx / this.RowCount);
                row = indx % (this.RowCount - 0);
                if (row == 0) row = 1; else ;
                this.Controls.Add(ar_pan[i], col, row);
            }
            this.ResumeLayout();

            HSD.StartPan(ar_pan);
        }

        public partial class HandlerSourceData : HHandlerQueue
        {
            /// <summary>
            /// Перечисление - индексы известных для обработки состояний
            /// </summary>
            public enum StatesMachine { Unknown = -1, AddControl=60 }

            private event DelegateDateFunc EvtEtalonDate;
            private event DelegateObjectFunc EvtGetDate;
            PanelSourceData.PanelGetDate[] m_ar_panels;

            private System.Windows.Forms.Timer m_timerGetDate;
            private object m_lockTimerGetDate;

            object m_panel = null;
            DataTable m_tableSourceData = null;

            public HandlerSourceData(object panel)
                : base()
            {
                Initialize();
                m_panel = panel;
                m_ar_panels = new PanelSourceData.PanelGetDate[INDEX_SOURCE_GETDATE.Length];
            }

            protected override int StateCheckResponse(int state, out bool error, out object outobj)
            {
                int iRes = 0;

                error = false;
                outobj = null;

                return iRes;
            }

            protected override HHandler.INDEX_WAITHANDLE_REASON StateErrors(int state, int req, int res)
            {
                return INDEX_WAITHANDLE_REASON.SUCCESS;
            }

            protected override int StateRequest(int state)
            {
                int iRes = 0;

                switch (state)
                {
                    case (int)StatesMachine.AddControl:
                        addPanels();
                        break;
                }
                return iRes;
            }

            protected override int StateResponse(int state, object obj)
            {
                int iRes = 0;

                obj = null;

                return iRes;
            }

            protected override void StateWarnings(int state, int req, int res)
            {
                throw new NotImplementedException();
            }

            public override bool Activate(bool active)
            {
                bool bRes = base.Activate(active);

                if (this.Actived == false)
                    for (int i = 0; i < m_ar_panels.Length; i++)
                        m_ar_panels[i].Activate(this.Actived);

                return bRes;
            }

            private void fThreadGetDate(object obj, EventArgs ev)
            {
                EvtGetDate(DateTime.UtcNow);

                //lock (m_lockTimerGetDate)
                //{
                //    if (!(m_timerGetDate == null))
                //        m_timerGetDate.Change(1000, System.Threading.Timeout.Infinite);
                //    else ;
                //}
            }

            //public override void Start()
            //{
            //    base.Start();
            //}

            public override void Stop()
            {
                stop();

                base.Stop();
            }

            private void stop()
            {
                for (int i = 0; i < INDEX_SOURCE_GETDATE.Length; i++)
                    m_ar_panels[i].Stop();

                if (!(m_timerGetDate == null))
                {
                    //m_timerGetDate.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    m_timerGetDate.Stop();
                    m_timerGetDate.Dispose();
                    m_timerGetDate = null;
                }

                else ;
            }

            protected override void Initialize()
            {
                m_lockTimerGetDate = new object();

                base.Initialize();
            }

            //public override void ClearStates()
            //{
            //    base.ClearStates();
            //}

            private void addPanels()
            {
                try
                {
                    m_ar_panels = new PanelSourceData.PanelGetDate[INDEX_SOURCE_GETDATE.Length];

                    for (int i = 0; i < INDEX_SOURCE_GETDATE.Length; i++)
                    {
                        m_ar_panels[i] = new PanelSourceData.PanelGetDate();
                    }

                    m_ar_panels[0].DelegateEtalonGetDate = new HClassLibrary.DelegateDateFunc(recievedEtalonDate);
                    //Для панелей с любыми серверами БД
                    for (int i = 0; i < m_ar_panels.Length; i++)
                    {
                        EvtGetDate += new HClassLibrary.DelegateObjectFunc(m_ar_panels[i].OnEvtGetDate);
                        EvtEtalonDate += new HClassLibrary.DelegateDateFunc(m_ar_panels[i].OnEvtEtalonDate);
                    }

                    ((PanelSourceData)m_panel).Invoke(((PanelSourceData)m_panel).m_delAddPan,new object[]{m_ar_panels});
                }
                catch (Exception e)
                {
                }
            }

            public void StartPan(PanelSourceData.PanelGetDate[] ar_panels)
            {
                int err = -1; //Признак выполнения метода/функции
                //Зарегистрировать соединение/получить идентификатор соединения
                int iListenerId = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");

                DbConnection dbConn = null;
                dbConn = DbSources.Sources().GetConnection(iListenerId, out err);

                if ((err == 0) && (!(dbConn == null)))
                {
                    m_tableSourceData = DbTSQLInterface.Select(ref dbConn, @"SELECT * FROM source", null, null, out err);

                    if (err == 0)
                    {
                        if (m_tableSourceData.Rows.Count > 0)
                        {
                            int i = -1
                                , j = -1;
                            for (i = 0; i < INDEX_SOURCE_GETDATE.Length; i++)
                            {
                                ar_panels[i].EvtAskedData += new DelegateObjectFunc(onEvtQueryAskedData);
                                for (j = 0; j < m_tableSourceData.Rows.Count; j++)
                                {
                                    ar_panels[i].AddSourceData(m_tableSourceData.Rows[j][@"NAME_SHR"].ToString());
                                }
                            }
                        }
                        else
                            ;
                    }
                    else
                        ;
                }
                else
                    throw new Exception(@"Нет соединения с БД");

                DbSources.Sources().UnRegister(iListenerId);

                for (int i = 0; i < INDEX_SOURCE_GETDATE.Length; i++)
                {
                    ar_panels[i].Start();
                    //m_arPanels[i].TurnOn(INDEX_SOURCE_GETDATE [i]);
                    ar_panels[i].Select(INDEX_SOURCE_GETDATE[i]);
                }

                //Выбрать действие
                lock (m_lockTimerGetDate)
                {
                    if (this.Actived == true)
                    {//Запустить поток
                        if (m_timerGetDate == null)
                        {
                            m_timerGetDate = new
                                //System.Threading.Timer(fThreadGetDate)
                                System.Windows.Forms.Timer()
                                ;
                            m_timerGetDate.Tick += new EventHandler(fThreadGetDate);
                            m_timerGetDate.Interval = 1000;
                        }
                        else
                            ;

                        //m_timerGetDate.Change(0, System.Threading.Timeout.Infinite);
                        m_timerGetDate.Start();
                    }
                    else
                    {
                        //Остановить поток
                        stop();
                    }
                }
                
                for (int i = 0; i < m_ar_panels.Length; i++)
                    m_ar_panels[i].Activate(this.Actived);
            }

            private void recievedEtalonDate(DateTime date)
            {
                EvtEtalonDate(date);
            }

            /// <summary>
            /// Массив значений - индексы источников данных (по таблице БД_КОНФИГУРАЦИИ.[SOURCE])
            /// </summary>
            private static int[] INDEX_SOURCE_GETDATE = 
            {
                26 //Эталон - ne2844
                //Вариант №1
                , 1, 4, 7, 10, 13, /*16*/-1
                , 2, 5, 8, 11, 14, 17
                , 3, 6, 9, 12, 15, -1
            };
            /// <summary>
            /// Обработчик запросов на получение информации
            /// </summary>
            /// <param name="ev"></param>
            private void onEvtQueryAskedData(object ev)
            {
                int iListenerId = -1
                    , id = -1
                    , err = -1;
                string nameShrSourceData = string.Empty;
                DataRow rowConnSett;
                // отправить ответ в ~ от идентификатора запрашиваемой информации
                try
                {
                    switch (((EventArgsDataHost)ev).id_detail)
                    {
                        case (int)PanelSourceData.PanelGetDate.ID_ASKED_DATAHOST.CONN_SETT: // запрошены параметры соединения
                            iListenerId = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
                            nameShrSourceData = ((PanelSourceData.PanelGetDate)((EventArgsDataHost)ev).par[0]).GetNameShrSelectedSourceData();
                            id = Int32.Parse(m_tableSourceData.Select(@"NAME_SHR = '" + nameShrSourceData + @"'")[0][@"ID"].ToString());
                            rowConnSett = ConnectionSettingsSource.GetConnectionSettings(/*TYPE_DATABASE_CFG.CFG_200,*/ iListenerId, id, 501, out err).Rows[0];
                            // создать объект с параметрами соединения
                            ConnectionSettings connSett = new ConnectionSettings(rowConnSett, -1);
                            // отправить запрашиваемые данные
                            ((PanelSourceData.PanelGetDate)((EventArgsDataHost)ev).par[0]).OnEvtDataRecievedHost(new EventArgsDataHost(-1, ((EventArgsDataHost)ev).id_detail, new object[] { connSett }));
                            DbSources.Sources().UnRegister(iListenerId);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelSourceData::onEvtQueryAskedData () - ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
        }
    }
}


