using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using System.Net;
using System.Net.Sockets;

using System.IO;

namespace StatisticCommon
{
    public partial class FormMainAnalyzer : Form //FormMainBase//: FormMainBaseWithStatusStrip
    {
        TcpClientAsync m_tcpClient;
        LogParse m_LogParse;

        DataTable m_tableUsers
                    , m_tableRoles;

        ConnectionSettings m_connSettConfigDB;

        int m_prevDatetimeRowIndex;

        private const string list_sorted = @"DESCRIPTION";

        public FormMainAnalyzer(ConnectionSettings connSett)
        {
            InitializeComponent();
            /*
            //При наследовании от ''
            // m_statusStripMain
            this.m_statusStripMain.Location = new System.Drawing.Point(0, 546);
            this.m_statusStripMain.Size = new System.Drawing.Size(841, 22);
            // m_lblMainState
            this.m_lblMainState.Size = new System.Drawing.Size(166, 17);
            // m_lblDateError
            this.m_lblDateError.Size = new System.Drawing.Size(166, 17);
            // m_lblDescError
            this.m_lblDescError.Size = new System.Drawing.Size(463, 17);
            */

            m_connSettConfigDB = connSett;

            dgvFilterActives.Rows.Add (2);
            dgvFilterActives.Rows[0].Cells[0].Value = true; dgvFilterActives.Rows[0].Cells[1].Value = "Активные";
            dgvFilterActives.Rows[1].Cells[0].Value = true; dgvFilterActives.Rows[1].Cells[1].Value = "Не активные";
            dgvFilterActives.Enabled = false;

            int err = -1;

            MySql.Data.MySqlClient.MySqlConnection connDB = DbTSQLInterface.GetConnection(DbTSQLInterface.DB_TSQL_INTERFACE_TYPE.MySQL, m_connSettConfigDB, out err);

            Users.GetRoles(connDB, string.Empty, string.Empty, out m_tableRoles, out err);
            FillDataGridViews(ref dgvFilterRoles, m_tableRoles, @"DESCRIPTION", err, true);

            Users.GetUsers(connDB, string.Empty, list_sorted, out m_tableUsers, out err);
            FillDataGridViews(ref dgvClient, m_tableUsers, @"DESCRIPTION", err);

            DbTSQLInterface.CloseConnection (connDB, out err);

            dgvTypeMessage.Rows.Add((int)LogParse.TYPE_LOGMESSAGE.COUNT_TYPE_LOGMESSAGE);
            for (LogParse.TYPE_LOGMESSAGE i = 0; i < LogParse.TYPE_LOGMESSAGE.COUNT_TYPE_LOGMESSAGE; i++)
            {
                dgvTypeMessage.Rows [(int)i].Cells [0].Value = true;
                dgvTypeMessage.Rows[(int)i].Cells[1].Value = LogParse.DESC_LOGMESSAGE[(int)i];
                dgvTypeMessage.Rows[(int)i].Cells[2].Value = 0;
            }

            m_LogParse = new LogParse ();
            m_LogParse.Exit = LogParseExit;
        }

        private void FormMainAnalyzer_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_tcpClient.Write(@"DISCONNECT");

            try
            {
                m_tcpClient.Disconnect();
            }
            catch (Exception excpt)
            {
                Logging.Logg().LogExceptionToFile(excpt, "FormMainAnalyzer...FormClosing () - m_tcpClient.Disconnect()");
            }

            m_LogParse.Stop();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close ();
        }

        private void dgvClient_SelectionChanged(object sender, EventArgs e)
        {
            if (!(m_tcpClient == null))
            {
                m_tcpClient.Write (@"DISCONNECT");
                m_tcpClient.Disconnect ();
            }
            else
                ;
            m_tcpClient = null;

            if ((dgvClient.SelectedRows.Count > 0) && (!(dgvClient.SelectedRows[0].Index < 0)))
            {
                m_LogParse.Stop();

                m_tcpClient = new TcpClientAsync("localhost", 6666);
                m_tcpClient.delegateRead = Read;
                m_tcpClient.Connect();

                m_tcpClient.Write(@"INIT=?");
            }
            else
                ;
        }

        private void FillDataGridViews(ref DataGridView ctrl, DataTable src, string nameField, int run, bool checkDefault = false)
        {
            if (run == 0)
            {
                bool bCheckedItem = checkDefault;
                ctrl.Rows.Clear ();

                if (src.Rows.Count > 0)
                {
                    ctrl.Rows.Add (src.Rows.Count);

                    for (int i = 0; i < src.Rows.Count; i ++)
                    {
                        //Проверка активности
                        //m_tcpSender.Init(m_tableUsers.Rows[i]["COMPUTER_NAME"].ToString ());
                        //bCheckedItem = m_tcpSender.Connected;
                        //m_tcpSender.Close ();

                        ctrl.Rows[i].Cells[0].Value = bCheckedItem;
                        ctrl.Rows[i].Cells[1].Value = src.Rows[i][nameField].ToString();
                    }
                }
                else
                    ;
            }
            else
                ;
        }

        void TabLoggingClearDatetimeStart() { dgvDatetimeStart.Rows.Clear(); }

        void TabLoggingClearText() { textBoxLog.Clear (); }

        void TabLoggingPositionText()
        {
            textBoxLog.Select(0, 0);
            textBoxLog.ScrollToCaret();
        }

        void TabLoggingAppendText (string text)
        {
            if (text.Equals (string.Empty) == true)
            {
                TabLoggingPositionText ();
                //textBoxLog.Focus();
                //textBoxLog.AutoScrollOffset = new Point (0, 0);
            }
            else
                textBoxLog.AppendText(text);
        }

        void TabLoggingAppendDatetimeStart(string text)
        {
            bool bRowChecked = bool.Parse(text.Split(';')[0]);

            if (bRowChecked == true)
            {
                dgvDatetimeStart.SelectionChanged += dgvDatetimeStart_SelectionChanged;
                m_prevDatetimeRowIndex = dgvDatetimeStart.Rows.Count;
            }
            else
                ;

            dgvDatetimeStart.Rows.Add(new object[] { bRowChecked, text.Split(';')[1] });

            if (bRowChecked == true)
            {
                dgvDatetimeStart.Rows [dgvDatetimeStart.Rows.Count - 1].Selected = bRowChecked;                
                dgvDatetimeStart.FirstDisplayedScrollingRowIndex = dgvDatetimeStart.Rows.Count - 1;
            }
            else
                ;
        }

        void Read (string rec)
        {
             //Message from Analyzer CMD;ARG1, ARG2,...,ARGN=RESULT
            switch (rec.Split ('=') [0].Split (';')[0])
            {
                case "INIT":
                    if (rec.Split('=')[1].Split(';')[0].Equals ("OK", StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        m_tcpClient.Write("LOG_LOCK=?");
                    }
                    else
                        ;
                    break;
                case "LOG_LOCK":
                    if (rec.Split('=')[1].Split(';')[0].Equals ("OK", StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        //rec.Split('=')[1].Split(';')[1] - полный путь лог-файла
                        FileInfo fi = new FileInfo(rec.Split('=')[1].Split(';')[1]);
                        FileStream fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                        StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("windows-1251"));

                        dgvDatetimeStart.SelectionChanged -= dgvDatetimeStart_SelectionChanged;

                        m_LogParse.Start(sr.ReadToEnd());

                        sr.Close ();

                        m_tcpClient.Write("LOG_UNLOCK=?");                        
                    }
                    else
                        ;
                    break;
                case "LOG_UNLOCK":
                    if (rec.Split('=')[1].Split(';')[0].Equals ("OK", StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                    }
                    else
                        ;
                    break;
                case "":
                    break;
                default:
                    break;
            }
        }

        private void dgvFilterRoles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int i = -1, err = -1;
            string where = string.Empty;
            
            if (e.ColumnIndex == 0)
            {
                dgvFilterRoles.Rows [e.RowIndex].Cells [0].Value = ! bool.Parse (dgvFilterRoles.Rows [e.RowIndex].Cells [0].Value.ToString ());

                for (i = 0; i < m_tableRoles.Rows.Count; i ++)
                {
                    if (bool.Parse(dgvFilterRoles.Rows [i].Cells [0].Value.ToString ()) == false)
                    {
                        if (where.Equals (string.Empty) == true)
                        {
                            where = "ID_ROLE NOT IN (";
                        }
                        else
                            where += ",";

                        where += m_tableRoles.Rows [i]["ID"];
                    }
                    else
                        ;
                }

                if (where.Equals(string.Empty) == false)
                    where += ")";
                else
                    ;

                Users.GetUsers(m_connSettConfigDB, where, list_sorted, out m_tableUsers, out err);
                FillDataGridViews(ref dgvClient, m_tableUsers, @"DESCRIPTION", err);
            }
            else
                ;
        }

        public void LogParseExit()
        {
            int i =-1;
            DataRow [] rows = new DataRow [] {};

            DelegateStringFunc delegateAppendText = TabLoggingAppendText,
                                delegateAppendDatetimeStart = TabLoggingAppendDatetimeStart;            
            BeginInvoke (new DelegateFunc (TabLoggingClearDatetimeStart));
            BeginInvoke (new DelegateFunc (TabLoggingClearText));

            //Получение списка дата/время запуска приложения
            bool rowChecked = false;
            i = m_LogParse.Select((int)LogParse.TYPE_LOGMESSAGE.START, string.Empty, string.Empty, ref rows);
            for (i = 0; i < rows.Length; i ++)
            {
                if (i == (rows.Length - 1))
                    rowChecked = true;
                else
                    ;

                BeginInvoke (delegateAppendDatetimeStart, rowChecked.ToString() + ";" + rows[i]["DATE_TIME"].ToString());
            }

            //where = "DATE_TIME>='" + rows[rows.Length - 1]["DATE_TIME"] + "'";
            //rows = m_tableLog.Select(where, "DATE_TIME");
            //for (i = 0; i < rows.Length; i++)
            //{
            //    BeginInvoke (delegateAppendText, "[" + rows[i]["DATE_TIME"] + "]: " + rows[i]["MESSAGE"].ToString() + Environment.NewLine);
            //}

            //BeginInvoke(delegateAppendText, string.Empty);
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            dgvClient_SelectionChanged (null, null);
        }

        private void dgvDatetimeStart_SelectionChanged(object sender, EventArgs e)
        {
            DataRow [] rows = new DataRow [] {};
            string where = string.Empty;
            int i = -1;
            int rowIndex = dgvDatetimeStart.SelectedRows [0].Index;
            int[] arTypeLogMsgCounter = new int[(int)LogParse.TYPE_LOGMESSAGE.COUNT_TYPE_LOGMESSAGE];

            dgvDatetimeStart.Rows [m_prevDatetimeRowIndex].Cells [0].Value = false;
            dgvDatetimeStart.Rows[rowIndex].Cells[0].Value = true;
            m_prevDatetimeRowIndex = rowIndex;

            TabLoggingClearText ();

            string strDatetimeEnd = string.Empty;
            if ((rowIndex + 1) < dgvDatetimeStart.Rows.Count)
                strDatetimeEnd = dgvDatetimeStart.Rows[rowIndex + 1].Cells[1].Value.ToString();
            else
                ;
            i = m_LogParse.Select(-1, dgvDatetimeStart.Rows[rowIndex].Cells[1].Value.ToString(), strDatetimeEnd, ref rows);
            for (i = 0; i < rows.Length; i++)
            {
                TabLoggingAppendText ("[" + rows[i]["DATE_TIME"] + "]: " + rows[i]["MESSAGE"].ToString() + Environment.NewLine);

                arTypeLogMsgCounter[Convert.ToInt32 (rows[i]["TYPE"])] ++;
            }

            TabLoggingAppendText (string.Empty);

            for (i = 0; i < (int)LogParse.TYPE_LOGMESSAGE.COUNT_TYPE_LOGMESSAGE; i++)
            {
                dgvTypeMessage.Rows [i].Cells [2].Value = arTypeLogMsgCounter [i];
            }
        }

        /*
        //При наследовании от ''
        protected override bool UpdateStatusString()
        {
            bool have_eror = true;

            return have_eror;
        }
        */
    }
}
