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
using StatisticCommon;

namespace StatisticDiagnostic
{
    public partial class PanelStatisticDiagnostic : PanelStatistic
    {
         private static int [] INDEX_SOURCE_GETDATE = {
            26//28-ITC288 //Эталон - ne2844
            //Вариант №1
            , 1, 4, 7, 10, 13, /*16*/-1
            , 2, 5, 8, 11, 14, 17
            , 3, 6, 9, 12, 15, -1
            ////Вариант №2
            //, -1, -1, -1, -1, -1, /*16*/-1
            //, -1, -1, -1, -1, -1, 17
            //, -1, -1, -1, -1, -1, -1
            };
        private partial class PanelDataDiagnostic : TableLayoutPanel 
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

            public PanelDataDiagnostic()
            {
                InitializeComponent();
            }

            public PanelDataDiagnostic(IContainer container)
            {
                container.Add(this);

                InitializeComponent();
            }
            private void initialize()
            {
                InitializeComponent();

                m_lockGetDate = new object();

                m_arDateTime = new DateTime[(int)INDEX_DATETME.INDEX_DATETME_COUNT] { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };
            }
            private void updateDiffDate()
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

                m_labelDiff.Text = textDiff;
                m_labelDiff.Refresh();
            }
            private void recievedEtalonDate(DateTime date)
            {
                m_arDateTime[(int)INDEX_DATETME.ETALON] = date;
                //Обновить разницу сервера БД с эталонным сервером БД
                this.BeginInvoke(new DelegateFunc(updateDiffDate));
            }
            public void OnEvtEtalonDate(DateTime date)
            {
                recievedEtalonDate(date);
            }
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
                    else ;
                }
            }
        }
        partial class PanelDataDiagnostic
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
            private void InitializeComponent() {

            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.labeltask = new System.Windows.Forms.Label();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.labelEValuesModes = new System.Windows.Forms.Label();
            this.labelTimeCheckModes = new System.Windows.Forms.Label();
            this.labelModes = new System.Windows.Forms.Label();
            this.labelConnModes = new System.Windows.Forms.Label();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.labelTec = new System.Windows.Forms.Label();
            this.labelSignofConn = new System.Windows.Forms.Label();
            this.labelExtremeValues = new System.Windows.Forms.Label();
            this.labelsourcedata = new System.Windows.Forms.Label();
            this.labelTimeCheck = new System.Windows.Forms.Label();
            this.labelExtremeValue = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.996146F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 92.00385F));
            this.tableLayoutPanel1.Controls.Add(this.labeltask, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.checkedListBox1, 0, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 146F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 156F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 236F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1038, 429);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Items.AddRange(new object[] {
            "ТЭЦ2 ",
            "ТЭЦ3 ",
            "ТЭЦ4",
            "ТЭЦ5 ",
            "БТЭЦ",
            "БиТЭЦ "});
            this.checkedListBox1.Location = new System.Drawing.Point(3, 326);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(76, 94);
            this.checkedListBox1.TabIndex = 0;
            // 
            // labeltask
            // 
            this.labeltask.AutoSize = true;
            this.labeltask.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labeltask.Location = new System.Drawing.Point(3, 302);
            this.labeltask.Name = "labeltask";
            this.labeltask.Size = new System.Drawing.Size(76, 21);
            this.labeltask.TabIndex = 6;
            this.labeltask.Text = "Список задач";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 4;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel4, 2);
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.64463F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67.35537F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 224F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 565F));
            this.tableLayoutPanel4.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.labelEValuesModes, 3, 0);
            this.tableLayoutPanel4.Controls.Add(this.labelTimeCheckModes, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.labelModes, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.labelConnModes, 1, 0);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 149);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.51515F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 88.48485F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(1032, 150);
            this.tableLayoutPanel4.TabIndex = 12;
            // 
            // labelEValuesModes
            // 
            this.labelEValuesModes.AutoSize = true;
            this.labelEValuesModes.Location = new System.Drawing.Point(469, 0);
            this.labelEValuesModes.Name = "labelEValuesModes";
            this.labelEValuesModes.Size = new System.Drawing.Size(100, 13);
            this.labelEValuesModes.TabIndex = 13;
            this.labelEValuesModes.Text = "labelEValuesModes";
            // 
            // labelTimeCheckModes
            // 
            this.labelTimeCheckModes.AutoSize = true;
            this.labelTimeCheckModes.Location = new System.Drawing.Point(245, 0);
            this.labelTimeCheckModes.Name = "labelTimeCheckModes";
            this.labelTimeCheckModes.Size = new System.Drawing.Size(115, 13);
            this.labelTimeCheckModes.TabIndex = 12;
            this.labelTimeCheckModes.Text = "labelTimeCheckModes";
            // 
            // labelModes
            // 
            this.labelModes.AutoSize = true;
            this.labelModes.Location = new System.Drawing.Point(3, 0);
            this.labelModes.Name = "labelModes";
            this.labelModes.Size = new System.Drawing.Size(61, 13);
            this.labelModes.TabIndex = 10;
            this.labelModes.Text = "labelModes";
            // 
            // labelConnModes
            // 
            this.labelConnModes.AutoSize = true;
            this.labelConnModes.Location = new System.Drawing.Point(82, 0);
            this.labelConnModes.Name = "labelConnModes";
            this.labelConnModes.Size = new System.Drawing.Size(86, 13);
            this.labelConnModes.TabIndex = 11;
            this.labelConnModes.Text = "labelConnModes";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel3.ColumnCount = 5;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel3, 2);
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.11382F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67.88618F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 294F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 211F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 280F));
            this.tableLayoutPanel3.Controls.Add(this.labelTec, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.labelSignofConn, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.labelExtremeValues, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.labelsourcedata, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.labelTimeCheck, 4, 0);
            this.tableLayoutPanel3.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 121F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1032, 140);
            this.tableLayoutPanel3.TabIndex = 10;
            // 
            // labelTec
            // 
            this.labelTec.AutoSize = true;
            this.labelTec.Location = new System.Drawing.Point(3, 0);
            this.labelTec.Name = "labelTec";
            this.labelTec.Size = new System.Drawing.Size(48, 13);
            this.labelTec.TabIndex = 11;
            this.labelTec.Text = "labelTec";
            // 
            // labelSignofConn
            // 
            this.labelSignofConn.AutoSize = true;
            this.labelSignofConn.Location = new System.Drawing.Point(249, 0);
            this.labelSignofConn.Name = "labelSignofConn";
            this.labelSignofConn.Size = new System.Drawing.Size(84, 13);
            this.labelSignofConn.TabIndex = 10;
            this.labelSignofConn.Text = "labelSignofConn";
            // 
            // labelExtremeValues
            // 
            this.labelExtremeValues.AutoSize = true;
            this.labelExtremeValues.Location = new System.Drawing.Point(543, 0);
            this.labelExtremeValues.Name = "labelExtremeValues";
            this.labelExtremeValues.Size = new System.Drawing.Size(99, 13);
            this.labelExtremeValues.TabIndex = 13;
            this.labelExtremeValues.Text = "labelExtremeValues";
            // 
            // labelsourcedata
            // 
            this.labelsourcedata.AutoSize = true;
            this.labelsourcedata.Location = new System.Drawing.Point(82, 0);
            this.labelsourcedata.Name = "labelsourcedata";
            this.labelsourcedata.Size = new System.Drawing.Size(101, 13);
            this.labelsourcedata.TabIndex = 14;
            this.labelsourcedata.Text = "Источники данных";
            // 
            // labelTimeCheck
            // 
            this.labelTimeCheck.AutoSize = true;
            this.labelTimeCheck.Location = new System.Drawing.Point(754, 0);
            this.labelTimeCheck.Name = "labelTimeCheck";
            this.labelTimeCheck.Size = new System.Drawing.Size(83, 13);
            this.labelTimeCheck.TabIndex = 11;
            this.labelTimeCheck.Text = "labelTimeCheck";
            // 
            // labelExtremeValue
            // 
            this.labelExtremeValue.AutoSize = true;
            this.labelExtremeValue.Location = new System.Drawing.Point(104, 92);
            this.labelExtremeValue.Name = "labelExtremeValue";
            this.labelExtremeValue.Size = new System.Drawing.Size(94, 13);
            this.labelExtremeValue.TabIndex = 12;
            this.labelExtremeValue.Text = "labelExtremeValue";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.tableLayoutPanel3.SetColumnSpan(this.label1, 5);
            this.label1.Location = new System.Drawing.Point(3, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "label1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.tableLayoutPanel4.SetColumnSpan(this.label2, 5);
            this.label2.Location = new System.Drawing.Point(3, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "label2";
          
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label labelExtremeValue;
        private System.Windows.Forms.Label labelTimeCheck;
        private System.Windows.Forms.Label labelSignofConn;
        private System.Windows.Forms.Label labelExtremeValues;
        private System.Windows.Forms.Label labelTec;
        private System.Windows.Forms.Label labelsourcedata;
        private System.Windows.Forms.Label labeltask;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label labelEValuesModes;
        private System.Windows.Forms.Label labelTimeCheckModes;
        private System.Windows.Forms.Label labelModes;
        private System.Windows.Forms.Label labelConnModes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    
            }

        private object m_lockTimerGetDate;
        private System.Threading.Timer m_timerGetDate;
        private event DelegateObjectFunc EvtGetDate;
        private event DelegateDateFunc EvtEtalonDate;

        private ConnectionSettings m_connSett;
        private DataTable m_tableSourceData;


       
        private void recievedEtalonDate(DateTime date)
        {
            EvtEtalonDate(date);
        }
        private void fThreadGetDate(object obj)
        {
            EvtGetDate(DateTime.UtcNow);

            lock (m_lockTimerGetDate)
            {
                if (!(m_timerGetDate == null))
                    m_timerGetDate.Change(1000, System.Threading.Timeout.Infinite);
                else ;
            }
        }
        private void stop()
        {
            if (!(m_timerGetDate == null))
            {
                m_timerGetDate.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                m_timerGetDate.Dispose();
                m_timerGetDate = null;
            }
            else
                ;
        }

        public override void Stop()
        {
            stop();

            for (int i = 0; i < m_arPanels.Length; i++)
            {
                m_arPanels[i].Stop();
            }
        }

        public override void Activate(bool activated)
        {
            //Выбрать действие
            lock (m_lockTimerGetDate)
            {
                if (activated == true)
                {//Запустить поток
                    if (m_timerGetDate == null)
                        m_timerGetDate = new System.Threading.Timer(fThreadGetDate);
                    else
                        ;

                    m_timerGetDate.Change(0, System.Threading.Timeout.Infinite);
                }
                else
                {
                    //Остановить поток
                    stop();
                }
            }

            for (int i = 0; i < m_arPanels.Length; i++)
            {
                m_arPanels[i].Activate(activated);
            }
        }

    }
}

