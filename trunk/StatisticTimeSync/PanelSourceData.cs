using System;
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
        public PanelGetDate[] m_arPanels;

        /// <summary>
        /// Массив значений
        /// </summary>
        private static int[] INDEX_SOURCE_GETDATE = {
            26 //Эталон - ne2844
            //Вариант №1
            , 1, 4, 7, 10, 13, /*16*/-1
            , 2, 5, 8, 11, 14, 17
            , 3, 6, 9, 12, 15, -1
            ////Вариант №2
            //, -1, -1, -1, -1, -1, /*16*/-1
            //, -1, -1, -1, -1, -1, 17
            //, -1, -1, -1, -1, -1, -1
        };

        /// <summary>
        /// Работа с компонентами панели
        /// </summary>
        public partial class PanelGetDate : HPanelCommon
        {
            public enum ID_ASKED_DATAHOST
            {
                CONN_SETT
                ,
            }
            private enum INDEX_DATETME
            {
                METKA, ETALON,
                SERVER
                    , INDEX_DATETME_COUNT
            }

            public event DelegateObjectFunc EvtAskedData;
            public DelegateDateFunc DelegateEtalonGetDate;

            private object m_lockGetDate;
            private HGetDate m_getDate;
            private DateTime[] m_arDateTime;

            public PanelGetDate()
                : base(-1, -1)
            {
                initialize();
            }

            public PanelGetDate(IContainer container)
                : base(container, -1, -1)
            {
                container.Add(this);

                initialize();
            }

            private void initialize()
            {
                InitializeComponent();

                m_lockGetDate = new object();

                m_arDateTime = new DateTime[(int)INDEX_DATETME.INDEX_DATETME_COUNT] { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="ev"></param>
            private void checkBoxTurnOn_CheckedChanged(object obj, EventArgs ev)
            {
                this.m_comboBoxSourceData.Enabled = !m_checkBoxTurnOn.Checked;

                if (m_checkBoxTurnOn.Checked == true)
                {
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
                    EvtAskedData(new EventArgsDataHost((int)ID_ASKED_DATAHOST.CONN_SETT, new object[] { this }));
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"PanelSourceData.PanelGetDate::queryConnSett", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="ev"></param>
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
            /// <param name="desc"></param>
            public void AddSourceData(string desc)
            {
                m_comboBoxSourceData.Items.Add(desc);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public string GetNameShrSelectedSourceData()
            {
                return (string)Invoke(new Func<string>(() => m_comboBoxSourceData.SelectedItem.ToString()));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="ev"></param>
            public void OnEvtDataRecievedHost(EventArgsDataHost ev)
            {
                switch (ev.id)
                {
                    case (int)ID_ASKED_DATAHOST.CONN_SETT:
                        //Установить соедиение
                        m_getDate = new HGetDate((ConnectionSettings)ev.par[0], recievedGetDate, errorGetDate);
                        //Запустить поток
                        m_getDate.StartDbInterfaces();
                        m_getDate.Start();
                        break;
                    default:
                        break;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="date"></param>
            private void recievedGetDate(DateTime date)
            {
                //Console.WriteLine (date.Kind.ToString ());
                m_arDateTime[(int)INDEX_DATETME.SERVER] = date/*.ToUniversalTime ()*/;
                //Обновить время сервера БД
                this.BeginInvoke(new DelegateFunc(updateGetDate));
                //Если панель с ЭТАЛОНным сервером БД
                if ((m_arDateTime[(int)INDEX_DATETME.SERVER].Equals(DateTime.MinValue) == false)
                    && (!(DelegateEtalonGetDate == null)))
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

                if ((m_arDateTime[(int)INDEX_DATETME.ETALON].Equals(DateTime.MinValue) == false)
                    && m_arDateTime[(int)INDEX_DATETME.SERVER].Equals(DateTime.MinValue) == false)
                {
                    double msecDiff = (m_arDateTime[(int)INDEX_DATETME.ETALON] - m_arDateTime[(int)INDEX_DATETME.SERVER]).TotalMilliseconds;
                    if (Math.Abs(msecDiff) < (1 * 60 * 60 * 1000))
                        ;
                    else
                        m_arDateTime[(int)INDEX_DATETME.SERVER] = m_arDateTime[(int)INDEX_DATETME.SERVER].AddHours(-3);

                    textDiff = ((m_arDateTime[(int)INDEX_DATETME.ETALON] - m_arDateTime[(int)INDEX_DATETME.SERVER]).TotalMilliseconds / 1000).ToString();

                }
                else
                    //Признак останова (деактивации)
                    textDiff = @"--.---";
                //Activate(false);

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
            /// 
            /// </summary>
            public override void Stop()
            {
                stop();

                base.Stop();
            }

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

            public void Activate(bool activated)
            {
                if (activated == true)
                {
                    if (m_checkBoxTurnOn.Checked == true)
                    {
                        //Start
                        //Спросить параметры соединения
                        IAsyncResult iar = BeginInvoke(new DelegateFunc(queryConnSett));
                    }
                    else
                        ;
                }
                else
                {
                    //Stop
                    stop();

                    //Признак деактивации
                    recievedGetDate(DateTime.MinValue);
                    recievedEtalonDate(DateTime.MinValue);

                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="indx"></param>
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
            /// 
            /// </summary>
            /// <param name="indx"></param>
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
            ///  проверка на чек
            /// </summary>
            /// <param name="indx"></param>
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

        private object m_lockTimerGetDate;
        private System.Windows.Forms.Timer m_timerGetDate;
        private event DelegateObjectFunc EvtGetDate;
        private event DelegateDateFunc EvtEtalonDate;
        int iListenerId;
        private DataTable m_tableSourceData;

        public PanelSourceData()
        {
            initialize();
        }

        public PanelSourceData(IContainer container)
        {
            container.Add(this);

            initialize();
        }

        private void initialize()
        {
            InitializeComponent();

            m_lockTimerGetDate = new object();
        }

        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            base.Start();

            start();
        }

        private void start()
        {
            int err = -1; //Признак выполнения метода/функции
            //Зарегистрировать соединение/получить идентификатор соединения
            iListenerId = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");

            m_tableSourceData = null;

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
                            m_arPanels[i].EvtAskedData += new DelegateObjectFunc(onEvtQueryAskedData);
                            for (j = 0; j < m_tableSourceData.Rows.Count; j++)
                            {
                                m_arPanels[i].AddSourceData(m_tableSourceData.Rows[j][@"NAME_SHR"].ToString());
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
                //m_arPanels[i].TurnOn(INDEX_SOURCE_GETDATE [i]);
                m_arPanels[i].Select(INDEX_SOURCE_GETDATE[i]);
        }

        private void onEvtQueryAskedData(object ev)
        {
            try
            {
                switch (((EventArgsDataHost)ev).id)
                {
                    case (int)PanelGetDate.ID_ASKED_DATAHOST.CONN_SETT:
                        string nameShrSourceData = ((PanelGetDate)((EventArgsDataHost)ev).par[0]).GetNameShrSelectedSourceData();
                        int id = Int32.Parse(m_tableSourceData.Select(@"NAME_SHR = '" + nameShrSourceData + @"'")[0][@"ID"].ToString())
                            , err = -1;
                        DataRow rowConnSett = ConnectionSettingsSource.GetConnectionSettings(/*TYPE_DATABASE_CFG.CFG_200,*/ iListenerId, id, 501, out err).Rows[0];
                        ConnectionSettings connSett = new ConnectionSettings(rowConnSett, -1);
                        ((PanelGetDate)((EventArgsDataHost)ev).par[0]).OnEvtDataRecievedHost(new EventArgsDataHost(((EventArgsDataHost)ev).id, new object[] { connSett }));
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

        private void recievedEtalonDate(DateTime date)
        {
            EvtEtalonDate(date);
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

        public override void Stop()
        {
            stop();

            for (int i = 0; i < INDEX_SOURCE_GETDATE.Length; i++)
                m_arPanels[i].Stop();

            base.Stop();
        }

        private void stop()
        {
            if (!(m_timerGetDate == null))
            {
                //m_timerGetDate.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                m_timerGetDate.Stop();
                m_timerGetDate.Dispose();
                m_timerGetDate = null;
            }

            else ;
        }

        public override bool Activate(bool activated)
        {
            bool bRes = base.Activate(activated);

            //Выбрать действие
            lock (m_lockTimerGetDate)
            {
                if (activated == true)
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

            for (int i = 0; i < m_arPanels.Length; i++)
                m_arPanels[i].Activate(activated);

            return bRes;
        }
    }
}


