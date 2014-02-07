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
        private enum TYPE_LOGMESSAGE { START, STOP, DBOPEN, DBCLOSE, DBEXCEPTION, ERROR, DEBUG, UNKNOWN, COUNT_TYPE_LOGMESSAGE };
        private string [] DESC_LOGMESSAGE = { "Запуск", "Выход",
                                            "БД открыть", "БД закрыть", "БД исключение",
                                            "Ошибка", "Отладка", "Неопределенный тип" };
        private string[] SIGNATURE_LOGMESSAGE = { ProgramBase.MessageWellcome, ProgramBase.MessageExit,
                                                    DbTSQLInterface.MessageDbOpen, DbTSQLInterface.MessageDbClose, DbTSQLInterface.MessageDbException,
                                                    "!Ошибка!", "!Отладка!", "Неопределенный тип" };
        
        TcpClientAsync m_tcpClient;

        DataTable m_tableUsers
                    , m_tableRoles
                    , m_tableLog;

        ConnectionSettings m_connSettConfigDB;

        Thread m_threadParseLog;
        bool m_bThreadParseLogAllowed;

        int m_prevDatetimeRowIndex;

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

            Users.GetUsers(connDB, string.Empty, @"DESCRIPTION", out m_tableUsers, out err);
            FillDataGridViews(ref dgvClient, m_tableUsers, @"DESCRIPTION", err);

            DbTSQLInterface.CloseConnection (connDB, out err);

            dgvTypeMessage.Rows.Add ((int)TYPE_LOGMESSAGE.COUNT_TYPE_LOGMESSAGE);
            for (TYPE_LOGMESSAGE i = 0; i < TYPE_LOGMESSAGE.COUNT_TYPE_LOGMESSAGE; i ++)
            {
                dgvTypeMessage.Rows [(int)i].Cells [0].Value = true;
                dgvTypeMessage.Rows [(int)i].Cells [1].Value = DESC_LOGMESSAGE [(int)i];
                dgvTypeMessage.Rows[(int)i].Cells[2].Value = 0;
            }

            m_tableLog = new DataTable("ContentLog");
            DataColumn[] cols = new DataColumn[] { new DataColumn("DATE_TIME", typeof(DateTime)),
                                                    new DataColumn("TYPE", typeof(Int32)),
                                                    new DataColumn ("MESSAGE", typeof (string)) };
            m_tableLog.Columns.AddRange(cols);
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

            Thread_CloseParseLog ();
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
                Thread_CloseParseLog ();

                m_tcpClient = new TcpClientAsync("ne1150.ne.ru", 6666);
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
                        ctrl.Rows[i].Cells[1].Value = src.Rows[i]["DESCRIPTION"].ToString ();
                    }
                }
                else
                    ;
            }
            else
                ;
        }

        private void Thread_ProcParseLog(object data)
        {
            string line,
                   msg;
            int typeMsg = -1;
            DateTime dtMsg;
            bool bValid = false;

            line = msg = string.Empty;

            StringReader content = new StringReader (data as string);

            m_tableLog.Clear ();

            Console.WriteLine("Начало обработки лог-файла. Размер: {0}", (data as string).Length);

            do
            {
                try
                {
                    content.ReadLine();

                    line = content.ReadLine();
                    bValid = DateTime.TryParse(line, out dtMsg);
                    if (bValid == true)
                    {
                        content.ReadLine();

                        line = content.ReadLine();
                    }
                    else
                    {
                        //dtMsg = new DateTime(1666, 06, 06, 06, 06, 06);
                        dtMsg = (DateTime)m_tableLog.Rows[m_tableLog.Rows.Count - 1]["DATE_TIME"];
                    }

                    //string.Console.Write("[" + dtMsg.ToString() + "]: ");

                    if (line == null)
                    {
                        //BeginInvoke(delegateAppendText, string.Empty);
                        break;
                    }
                    else
                        ;

                    msg = line;
                    //Console.WriteLine(msg);

                    typeMsg = -1;

                    //Определение типа сообщения
                    TYPE_LOGMESSAGE i;
                    for (i = 0; i < TYPE_LOGMESSAGE.COUNT_TYPE_LOGMESSAGE - 1; i ++) //'- 1' чтобы не учитывать UNKNOWN
                    {
                        if (msg.Contains (SIGNATURE_LOGMESSAGE [(int)i]) == true)
                            break;
                        else
                            ;
                    }

                    //Был ли определен тип сообщения
                    if (i < TYPE_LOGMESSAGE.COUNT_TYPE_LOGMESSAGE)
                        typeMsg = (int)i;
                    else
                        //Не опредеоен
                        typeMsg = (int)TYPE_LOGMESSAGE.UNKNOWN;

                    //Добавление строки в таблицу с собщениями
                    m_tableLog.Rows.Add(new object[] { dtMsg, typeMsg, msg });

                    if (typeMsg == (int)TYPE_LOGMESSAGE.START)
                    {
                        //Добавление строки в 'dgvDatetimeStart'
                        //BeginInvoke(delegateAppendDatetimeStart, "false" + ";" + dtMsg);
                    }
                    else
                        ;

                    //Добавление строки в 'textBoxLog'
                    //BeginInvoke(delegateAppendText, "[" + dtMsg + "]: " + msg + Environment.NewLine);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    break;
                }
            }
            while (m_bThreadParseLogAllowed == true);

            Console.WriteLine("Окончание обработки лог-файла. Обработано строк: {0}", m_tableLog.Rows.Count);
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
                        
                        Thread_StartParseLog(sr.ReadToEnd());
                        new Thread (Thread_ProcJoinParseLog).Start ();

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

                Users.GetUsers(m_connSettConfigDB, where, @"DESCRIPTION", out m_tableUsers, out err);
                FillDataGridViews(ref dgvClient, m_tableUsers, @"DESCRIPTION", err);
            }
            else
                ;
        }

        private void Thread_CloseParseLog ()
        {
            bool joined = false;
            if ((!(m_threadParseLog == null)) && (m_threadParseLog.IsAlive == true))
            {
                m_bThreadParseLogAllowed = false;
                joined = m_threadParseLog.Join (6666);
                if (joined == false)
                    m_threadParseLog.Abort ();
                else
                    ;
            }
            else
                ;

            m_bThreadParseLogAllowed = true;
        }

        private void Thread_StartParseLog (string par)
        {
            m_threadParseLog = new Thread(new ParameterizedThreadStart(Thread_ProcParseLog));
            m_threadParseLog.IsBackground = true;
            m_threadParseLog.Name = "Разбор лог-файла";

            m_threadParseLog.Start (par);       
        }

        private void Thread_ProcJoinParseLog (object data)
        {
            bool joined = m_threadParseLog.Join (-1);
            int i =-1;
            DataRow [] rows;
            string where = string.Empty;

            DelegateStringFunc delegateAppendText = TabLoggingAppendText,
                                delegateAppendDatetimeStart = TabLoggingAppendDatetimeStart;            
            BeginInvoke (new DelegateFunc (TabLoggingClearDatetimeStart));
            BeginInvoke (new DelegateFunc (TabLoggingClearText));

            bool rowChecked = false;
            where = "TYPE=" + (int)TYPE_LOGMESSAGE.START;
            rows = m_tableLog.Select (where, "DATE_TIME");
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
            DataRow [] rows;
            string where = string.Empty;
            int i = -1;
            int rowIndex = dgvDatetimeStart.SelectedRows [0].Index;
            int [] arTypeLogMsgCounter = new int [(int)TYPE_LOGMESSAGE.COUNT_TYPE_LOGMESSAGE];

            dgvDatetimeStart.Rows [m_prevDatetimeRowIndex].Cells [0].Value = false;
            dgvDatetimeStart.Rows[rowIndex].Cells[0].Value = true;
            m_prevDatetimeRowIndex = rowIndex;

            TabLoggingClearText ();

            where = "DATE_TIME>='" + DateTime.Parse (dgvDatetimeStart.Rows [rowIndex].Cells[1].Value.ToString ()).ToString ("yyyy-MM-dd HH:mm:ss") + "'";
            if ((rowIndex + 1) < dgvDatetimeStart.Rows.Count)
                where += " AND DATE_TIME<'" + DateTime.Parse(dgvDatetimeStart.Rows[rowIndex + 1].Cells[1].Value.ToString()).ToString("yyyy-MM-dd HH:mm:ss") + "'";
            else
                ;
            rows = m_tableLog.Select(where, "DATE_TIME");
            for (i = 0; i < rows.Length; i++)
            {
                TabLoggingAppendText ("[" + rows[i]["DATE_TIME"] + "]: " + rows[i]["MESSAGE"].ToString() + Environment.NewLine);

                arTypeLogMsgCounter[Convert.ToInt32 (rows[i]["TYPE"])] ++;
            }

            TabLoggingAppendText (string.Empty);

            for (i = 0; i < (int)TYPE_LOGMESSAGE.COUNT_TYPE_LOGMESSAGE; i ++)
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
