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
        private partial class PanelGetDate : TableLayoutPanel
        {
            public enum ID_ASKED_DATAHOST { CONN_SETT }

            System.Threading.Timer m_timerGetDate;
            public event DelegateObjectFunc EvtAskedData;

            private HGetDate m_getDate;

            public PanelGetDate()
            {
                InitializeComponent();

                //m_getDate = new HGetDate ();
            }

            public PanelGetDate(IContainer container)
            {
                container.Add(this);

                InitializeComponent();
            }

            private void checkBoxTurnOn_CheckedChanged(object obj, EventArgs ev)
            {
                this.m_comboBoxSourceData.Enabled = !m_checkBoxTurnOn.Checked;

                if (m_checkBoxTurnOn.Checked == true)
                {
                    //Start
                    //Спросить параметры соединения
                    IAsyncResult iar = BeginInvoke(new DelegateFunc(queryConnSett));
                    //Установить соедиение, запустить поток
                }
                else
                {
                    //Stop
                    //Разорвать соедиенние
                    m_getDate.Stop();
                    m_getDate = null;

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

            private void queryConnSett()
            {
                EvtAskedData(new EventArgsDataHost(0, this));
            }

            private void comboBoxSourceData_SelectedIndexChanged(object obj, EventArgs ev)
            {
                if (m_comboBoxSourceData.SelectedIndex > 0)
                    m_checkBoxTurnOn.Enabled = true;
                else
                    m_checkBoxTurnOn.Enabled = false;
            }

            private void fThreadGetDate(object obj)
            {
                m_getDate.GetDate();

                m_timerGetDate.Change(1000, System.Threading.Timeout.Infinite);
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
                        m_getDate = new HGetDate((ConnectionSettings)ev.par, recievedGetDate);
                        m_getDate.StartDbInterfaces();
                        m_getDate.Start();

                        //Запустить поток
                        m_timerGetDate = new System.Threading.Timer(fThreadGetDate);
                        m_timerGetDate.Change(0, System.Threading.Timeout.Infinite);
                        break;
                    default:
                        break;
                }
            }

            protected void recievedGetDate(DateTime date)
            {
                this.BeginInvoke(new DelegateDateFunc(updateGetDate), date);
            }

            private void updateGetDate(DateTime date)
            {
                m_labelTime.Text = date.ToString(@"HH:mm:ss.fff");
                m_labelTime.Refresh();
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

        private ConnectionSettings m_connSett;
        private DataTable m_tableSourceData;

        public PanelSourceData()
        {
            InitializeComponent();
        }

        public PanelSourceData(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
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

            DbSources.Sources().UnRegister();
        }

        private void onEvtQueryAskedData(object ev)
        {
            switch (((EventArgsDataHost)ev).id)
            {
                case (int)PanelGetDate.ID_ASKED_DATAHOST.CONN_SETT:
                    int iListenerId = DbSources.Sources().Register(m_connSett, false, m_connSett.name)
                        , id = Int32.Parse(m_tableSourceData.Select(@"NAME_SHR = '" + ((PanelGetDate)((EventArgsDataHost)ev).par).GetSelectedSourceData() + @"'")[0][@"ID"].ToString())
                        , err = -1;
                    DataRow rowConnSett = ConnectionSettingsSource.GetConnectionSettings(TYPE_DATABASE_CFG.CFG_200, iListenerId, id, 501, out err).Rows[0];
                    ConnectionSettings connSett = new ConnectionSettings(rowConnSett, false);
                    ((PanelGetDate)((EventArgsDataHost)ev).par).OnEvtDataRecievedHost(new EventArgsDataHost(((EventArgsDataHost)ev).id, connSett));
                    DbSources.Sources().UnRegister(iListenerId);
                    break;
                default:
                    break;
            }
        }
    }
}
