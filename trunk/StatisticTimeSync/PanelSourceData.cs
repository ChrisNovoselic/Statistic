using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Threading;

using System.Windows.Forms; //TableLayoutPanel

using System.Data; //DataTable
using System.Data.Common; //DbConnection

using HClassLibrary;

namespace StatisticTimeSync
{
    public partial class PanelSourceData : TableLayoutPanel
    {
        private static int [] INDEX_SOURCE_GETDATE = {
            //26
            //, 1, 4, 7, 10, 13, /*16*/-1
            //, 2, 5, 8, 11, 14, 17
            //, 3, 6, 9, 12, 15, -1
            26
            , -1, -1, -1, -1, -1, /*16*/-1
            , -1, -1, -1, -1, -1, 17
            , -1, -1, -1, -1, -1, -1
        };
        
        private partial class PanelGetDate : TableLayoutPanel
        {
            public enum ID_ASKED_DATAHOST { CONN_SETT
                                            ,  }
            private enum INDEX_DATETME { METKA, ETALON, SERVER
                                        , INDEX_DATETME_COUNT }

            public event DelegateObjectFunc EvtAskedData;
            public DelegateDateFunc DelegateEtalonGetDate;

            private object m_lockGetDate;
            private HGetDate m_getDate;
            private DateTime [] m_arDateTime;

            public PanelGetDate()
            {
                initialize();                
            }

            public PanelGetDate(IContainer container)
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
                {
                    Activate (false);
                }
            }

            private void queryConnSett()
            {
                EvtAskedData(new EventArgsDataHost((int)ID_ASKED_DATAHOST.CONN_SETT, new object [] { this } ));
            }

            private void comboBoxSourceData_SelectedIndexChanged(object obj, EventArgs ev)
            {
                if (m_comboBoxSourceData.SelectedIndex > 0)
                    m_checkBoxTurnOn.Enabled = true;
                else
                    m_checkBoxTurnOn.Enabled = false;
            }

            public void AddSourceData(string desc)
            {
                m_comboBoxSourceData.Items.Add(desc);
            }

            public string GetSelectedSourceData()
            {
                return m_comboBoxSourceData.SelectedItem.ToString();
            }

            public void OnEvtDataRecievedHost(EventArgsDataHost ev)
            {
                switch (ev.id)
                {
                    case (int)ID_ASKED_DATAHOST.CONN_SETT:
                        //Установить соедиение
                        m_getDate = new HGetDate((ConnectionSettings)ev.par [0], recievedGetDate, errorGetDate);
                        //Запустить поток
                        m_getDate.StartDbInterfaces();
                        m_getDate.Start();
                        break;
                    default:
                        break;
                }
            }

            private void recievedGetDate(DateTime date)
            {
                //Console.WriteLine (date.Kind.ToString ());
                m_arDateTime[(int)INDEX_DATETME.SERVER] = date/*.ToUniversalTime ()*/;
                //Обновить время сервера БД
                this.BeginInvoke(new DelegateFunc(updateGetDate));
                //Если панель с ЭТАЛОНным сервером БД
                if ((m_arDateTime[(int)INDEX_DATETME.SERVER].Equals (DateTime.MinValue) == false)
                    && (!(DelegateEtalonGetDate == null)))
                {
                    DelegateEtalonGetDate(date);
                }
                else
                    ;
            }

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

            private void updateDiffDate()
            {
                string textDiff = string.Empty;

                if ((m_arDateTime[(int)INDEX_DATETME.ETALON].Equals (DateTime.MinValue) == false)
                    && m_arDateTime[(int)INDEX_DATETME.SERVER].Equals(DateTime.MinValue) == false) {
                    double msecDiff = (m_arDateTime[(int)INDEX_DATETME.ETALON] - m_arDateTime[(int)INDEX_DATETME.SERVER]).TotalMilliseconds;
                    if (Math.Abs(msecDiff) < (1 * 60 * 60 * 1000))
                        ;
                    else
                        m_arDateTime[(int)INDEX_DATETME.SERVER] = m_arDateTime[(int)INDEX_DATETME.SERVER].AddHours (-3);

                    textDiff = ((m_arDateTime[(int)INDEX_DATETME.ETALON] - m_arDateTime[(int)INDEX_DATETME.SERVER]).TotalMilliseconds / 1000).ToString ();
                   
                } else
                    //Признак останова (деактивации)
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

                lock (m_lockGetDate) {
                    if (! (m_getDate == null))
                        m_getDate.GetDate ();
                    else ;
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

            public void Activate (bool activated) {
                if (activated == true) {
                    if (m_checkBoxTurnOn.Checked == true) {
                        //Start
                        //Спросить параметры соединения
                        IAsyncResult iar = BeginInvoke(new DelegateFunc(queryConnSett));
                    } else {
                    }
                } else {
                    //Stop
                    //Разорвать соедиенние
                    lock (m_lockGetDate) {
                        if (! (m_getDate == null)) {
                            m_getDate.Stop();
                            m_getDate = null;
                        } else {
                        }
                    }

                    //Признак деактивации
                    recievedGetDate (DateTime.MinValue);
                    recievedEtalonDate(DateTime.MinValue);
                }
            }

            public void TurnOn (int indx) {
                if (m_checkBoxTurnOn.Checked == false) {
                    if (indx > 0) {
                        m_comboBoxSourceData.SelectedIndex = indx;
                        m_checkBoxTurnOn.Checked = true;
                    } else {
                    }
                } else {
                    //Ничего не делаем...
                }
            }

            public void TurnOff(int indx = -1)
            {
                if (m_checkBoxTurnOn.Checked == true)
                {
                    m_checkBoxTurnOn.Checked = false;

                    switch (indx) {
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
        }

        partial class PanelGetDate
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
                this.Dock = System.Windows.Forms.DockStyle.Fill;

                this.m_checkBoxTurnOn = new System.Windows.Forms.CheckBox();
                this.m_comboBoxSourceData = new System.Windows.Forms.ComboBox();
                this.m_labelTime = new System.Windows.Forms.Label();
                this.m_labelDiff = new System.Windows.Forms.Label();

                this.ColumnCount = 2; this.RowCount = 2;
                this.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;

                for (int i = 0; i < this.ColumnCount; i++)
                    this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
                for (int i = 0; i < this.RowCount; i++)
                    this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));

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
            private System.Windows.Forms.Label m_labelTime;
            private System.Windows.Forms.Label m_labelDiff;
        }

        private object m_lockTimerGetDate;
        private System.Threading.Timer m_timerGetDate;
        private event DelegateObjectFunc EvtGetDate;
        private event DelegateDateFunc EvtEtalonDate;

        private ConnectionSettings m_connSett;
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

        public void OnLoad()
        {
            m_connSett = new ConnectionSettings()
            {
                id = -1
                ,
                name = @"DB_CONFIG"
                ,
                server = @"10.100.104.18"
                ,
                port = 1433
                ,
                dbName = @"techsite_cfg-2.X.X"
                ,
                userName = @"client"
                ,
                password = @"client"
                ,
                ignore = false
            };

            int iListenerId = DbSources.Sources().Register(m_connSett, false, m_connSett.name)
                , err = -1;

            DbConnection dbConn = null;
            m_tableSourceData = null;

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
                        for (i = 0; i < m_arPanels.Length; i++)
                        {
                            m_arPanels[i].EvtAskedData += new DelegateObjectFunc(onEvtQueryAskedData);
                            for (j = 0; j < m_tableSourceData.Rows.Count; j++)
                            {
                                m_arPanels[i].AddSourceData(m_tableSourceData.Rows[j][@"NAME_SHR"].ToString());
                            }
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
            else
                throw new Exception(@"Нет соединения с БД");

            DbSources.Sources().UnRegister(iListenerId);

            for (int i = 0; i < m_arPanels.Length; i ++)
                m_arPanels[i].TurnOn(INDEX_SOURCE_GETDATE [i]);
        }

        private void onEvtQueryAskedData(object ev)
        {
            switch (((EventArgsDataHost)ev).id)
            {
                case (int)PanelGetDate.ID_ASKED_DATAHOST.CONN_SETT:
                    int iListenerId = DbSources.Sources().Register(m_connSett, false, m_connSett.name)
                        , id = Int32.Parse(m_tableSourceData.Select(@"NAME_SHR = '" + ((PanelGetDate)((EventArgsDataHost)ev).par [0]).GetSelectedSourceData() + @"'")[0][@"ID"].ToString())
                        , err = -1;
                    DataRow rowConnSett = ConnectionSettingsSource.GetConnectionSettings(TYPE_DATABASE_CFG.CFG_200, iListenerId, id, 501, out err).Rows[0];
                    ConnectionSettings connSett = new ConnectionSettings(rowConnSett, false);
                    ((PanelGetDate)((EventArgsDataHost)ev).par [0]).OnEvtDataRecievedHost(new EventArgsDataHost(((EventArgsDataHost)ev).id, new object [] { connSett } ));
                    DbSources.Sources().UnRegister(iListenerId);
                    break;
                default:
                    break;
            }
        }

        private void recievedEtalonDate(DateTime date)
        {
            EvtEtalonDate(date);
        }

        private void fThreadGetDate(object obj)
        {
            EvtGetDate (DateTime.UtcNow);

            lock (m_lockTimerGetDate)
            {
                if (! (m_timerGetDate == null))
                    m_timerGetDate.Change(1000, System.Threading.Timeout.Infinite);
                else ;
            }
        }

        public void Activate (bool activated) {
            //Выбрать действие
            lock (m_lockTimerGetDate) {
                if (activated == true)
                {//Запустить поток
                    if (m_timerGetDate == null)
                        m_timerGetDate = new System.Threading.Timer(fThreadGetDate);
                    else
                        ;

                    m_timerGetDate.Change(0, System.Threading.Timeout.Infinite);
                } else {
                    //Остановить поток
                        if (!(m_timerGetDate == null))
                        {
                            m_timerGetDate.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                            m_timerGetDate.Dispose();
                            m_timerGetDate = null;
                        }
                        else
                            ;
                }
            }

            for (int i = 0; i < m_arPanels.Length; i ++) {
                m_arPanels[i].Activate(activated);
            }
        }
    }
}
