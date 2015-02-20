using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Data.Common;

using System.Net;
using System.Net.Sockets;

using System.IO;

using HClassLibrary;

namespace StatisticCommon
{
    public abstract partial class FormMainAnalyzer : Form //FormMainBase//: FormMainBaseWithStatusStrip
    {
        Thread m_threadChecked;
        protected bool m_bThreadCheckedAllowed;
        protected LogParse m_LogParse;

        protected DataTable m_tableUsers
                    , m_tableRoles;

        protected DbConnection m_connConfigDB;

        CheckBox [] m_arCheckBoxMode;

        int m_prevDatetimeRowIndex;

        protected const string c_list_sorted = @"DESCRIPTION";
        protected const string c_NameFieldToConnect = "COMPUTER_NAME";

        List <TEC> m_listTEC;

        protected Dictionary <int, int []> m_dicTabVisibleIdItems;

        public FormMainAnalyzer(int idListener, List <TEC> tec)
        {
            InitializeComponent();
            /*
            //При наследовании от 'FormMainBaseWithStatusStrip'
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

            m_arCheckBoxMode = new CheckBox[] { checkBoxTEC, checkBoxGTP, checkBoxPC, checkBoxTG };

            m_listTEC = tec;

            m_dicTabVisibleIdItems = new Dictionary<int,int[]> ();
            FillDataGridViewTabVisible ();

            int i = -1;

            dgvFilterActives.Rows.Add (2);
            dgvFilterActives.Rows[0].Cells[0].Value = true; dgvFilterActives.Rows[0].Cells[1].Value = "Активные";
            dgvFilterActives.Rows[1].Cells[0].Value = true; dgvFilterActives.Rows[1].Cells[1].Value = "Не активные";
            dgvFilterActives.Enabled = false;

            int err = -1;

            m_connConfigDB = DbSources.Sources().GetConnection(idListener, out err);
            //DbConnection connDB = DbTSQLInterface.GetConnection(m_connSettConfigDB, out err);

            if ((! (m_connConfigDB == null)) && (err == 0))
            {
                HStatisticUsers.GetRoles(ref m_connConfigDB, string.Empty, string.Empty, out m_tableRoles, out err);
                FillDataGridViews(ref dgvFilterRoles, m_tableRoles, @"DESCRIPTION", err, true);

                HStatisticUsers.GetUsers(ref m_connConfigDB, string.Empty, c_list_sorted, out m_tableUsers, out err);
                FillDataGridViews(ref dgvClient, m_tableUsers, @"DESCRIPTION", err);

                //DbTSQLInterface.CloseConnection (connDB, out err);

                dgvTypeMessage.Rows.Add((int)LogParse.TYPE_LOGMESSAGE.COUNT_TYPE_LOGMESSAGE);
                for (i = 0; i < (int)LogParse.TYPE_LOGMESSAGE.COUNT_TYPE_LOGMESSAGE; i++)
                {
                    dgvTypeMessage.Rows [(int)i].Cells [0].Value = true;
                    dgvTypeMessage.Rows[(int)i].Cells[1].Value = LogParse.DESC_LOGMESSAGE[(int)i];
                    dgvTypeMessage.Rows[(int)i].Cells[2].Value = 0;
                }

                m_LogParse = new LogParse ();
                m_LogParse.Exit = LogParseExit;

                Thread_ProcCheckedStart ();
            }
            else 
                ;
        }

        private void Thread_ProcChecked (object data)
        {
            while (m_bThreadCheckedAllowed == true)
            {
                ProcChecked ();
            }
        }

        protected abstract void ProcChecked();

        /// <summary>
        /// Отключение от клиента (активного пользователя)
        /// </summary>
        protected abstract void Disconnect();

        private void FormMainAnalyzer_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect ();

            if (!(m_LogParse == null))
                m_LogParse.Stop();
            else
                ;

            Thread_ProcCheckedStop ();
        }

        protected virtual void Thread_ProcCheckedStart()
        {            
            m_bThreadCheckedAllowed = true;

            m_threadChecked = new Thread(Thread_ProcChecked);
            m_threadChecked.IsBackground = true;
            m_threadChecked.Name = "Поток опроса состояния пользователей";
            m_threadChecked.Start();
        }

        protected virtual void Thread_ProcCheckedStop ()
        {
            m_bThreadCheckedAllowed = false;
            
            if (! (m_threadChecked == null))
            {
                bool joined = m_threadChecked.Join(6666);
                if (joined == false)
                    m_threadChecked.Abort();
                else
                    ;
            } else
                ;            
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close ();
        }        

        protected void SetModeVisibleTabs (int mode)
        {
            //Состояние эл-ов упр-я 'CheckBox' (ТЭЦ, ГТП, ТГ, ЩУ)
            for (int i = 0; i < (int)FormChangeMode.MODE_TECCOMPONENT.UNKNOWN; i++)
                if (FormChangeMode.IsModeTECComponent(mode, (FormChangeMode.MODE_TECCOMPONENT)i) == true)
                    m_arCheckBoxMode[i].CheckState = CheckState.Checked;
                else
                    m_arCheckBoxMode[i].CheckState = CheckState.Unchecked;
        }

        protected abstract void dgvClient_SelectionChanged(object sender, EventArgs e);        

        protected void ErrorConnect(string ValueToCreate)
        {
            int indx = Convert.ToInt32 (ValueToCreate.Split(';')[1]);

            Console.WriteLine("FormAnalyzer::ErrorConnect () - {0}, индекс: {1}", ValueToCreate.Split(';')[0], ValueToCreate.Split(';')[1]);
            if (indx < dgvClient.Rows.Count) //m_tableUsers, m_listTcpClient
                dgvClient.Rows [indx].Cells[0].Value = false;
            else
                ; //Отработка ошибки соединения для пользователя УЖЕ отсутствующего в списке
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

        protected void TabLoggingClearDatetimeStart() { dgvDatetimeStart.Rows.Clear(); }

        protected void TabLoggingClearText() { textBoxLog.Clear(); }

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

        protected void TabVisibliesClearChecked ()
        {
            foreach (KeyValuePair<int, int[]> pair in m_dicTabVisibleIdItems)
                dgvTabVisible.Rows[m_dicTabVisibleIdItems[pair.Key][0]].Cells[m_dicTabVisibleIdItems[pair.Key][1]].Value = false;
        }

        protected void StartLogParse (string full_path)
        {
            FileInfo fi = new FileInfo(full_path);
            FileStream fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("windows-1251"));

            dgvDatetimeStart.SelectionChanged -= dgvDatetimeStart_SelectionChanged;

            m_LogParse.Start(sr.ReadToEnd());

            sr.Close();
        }        

        private void dgvFilterRoles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int i = -1, err = -1;
            string where = string.Empty;

            if (e.ColumnIndex == 0)
            {
                Thread_ProcCheckedStop ();

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

                HStatisticUsers.GetUsers(ref m_connConfigDB, where, c_list_sorted, out m_tableUsers, out err);
                FillDataGridViews(ref dgvClient, m_tableUsers, @"DESCRIPTION", err);

                Thread_ProcCheckedStart();
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

        protected void dgvDatetimeStart_SelectionChanged(object sender, EventArgs e)
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
                if (Convert.ToInt32(rows[i]["TYPE"]) == (int)LogParse.TYPE_LOGMESSAGE.DETAIL)
                    TabLoggingAppendText("    " + rows[i]["MESSAGE"].ToString() + Environment.NewLine);
                else
                    TabLoggingAppendText ("[" + rows[i]["DATE_TIME"] + "]: " + rows[i]["MESSAGE"].ToString() + Environment.NewLine);

                arTypeLogMsgCounter[Convert.ToInt32 (rows[i]["TYPE"])] ++;
            }

            TabLoggingAppendText (string.Empty);

            for (i = 0; i < (int)LogParse.TYPE_LOGMESSAGE.COUNT_TYPE_LOGMESSAGE; i++)
            {
                dgvTypeMessage.Rows [i].Cells [2].Value = arTypeLogMsgCounter [i];
            }
        }

        private void tabControlAnalyzer_SelectedIndexChanged(object sender, EventArgs e)
        {
            dgvClient_SelectionChanged (null, null);
        }

        private void tabControlAnalyzer_Selected(object sender, TabControlEventArgs e)
        {

        }

        private void FillDataGridViewTabVisible ()
        {
            int tec_indx = 0,
                comp_indx = 0,
                across_indx = -1
                , col = -1;

            if (!(m_listTEC == null))
                foreach (TEC t in m_listTEC)
                {
                    tec_indx ++;
                    comp_indx = 0;

                    col = (tec_indx - 1) * 2 + 1;

                    //Добавть 2 столбца (CheckBox, TextBox)
                    dgvTabVisible.Columns.AddRange (new DataGridViewColumn [] {new DataGridViewCheckBoxColumn (false), new DataGridViewTextBoxColumn ()});

                    this.dgvTabVisible.Columns [col - 1].Frozen = false;
                    this.dgvTabVisible.Columns[col - 1].HeaderText = "Use";
                    this.dgvTabVisible.Columns[col - 1].Name = "dataGridViewTabVisibleCheckBoxColumnUse";
                    this.dgvTabVisible.Columns[col - 1].ReadOnly = true;
                    this.dgvTabVisible.Columns[col - 1].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                    this.dgvTabVisible.Columns[col - 1].Width = 25;

                    this.dgvTabVisible.Columns[col].Frozen = false;
                    this.dgvTabVisible.Columns[col].HeaderText = "Desc";
                    this.dgvTabVisible.Columns[col].Name = "dataGridViewTabVisibleTextBoxColumnDesc";
                    this.dgvTabVisible.Columns[col].ReadOnly = true;
                    this.dgvTabVisible.Columns[col].Resizable = System.Windows.Forms.DataGridViewTriState.False;
                    this.dgvTabVisible.Columns[col].Width = 145;

                    across_indx++;

                    if (dgvTabVisible.Rows.Count < (comp_indx + 1))
                        dgvTabVisible.Rows.Add ();
                    else
                        ;

                    dgvTabVisible.Rows [comp_indx].Cells [col].Value = t.name_shr;
                    m_dicTabVisibleIdItems.Add(t.m_id, new int[] { comp_indx, col - 1 });

                    if (t.list_TECComponents.Count > 0)
                    {
                        foreach (TECComponent g in t.list_TECComponents)
                        {
                            comp_indx++;
                            
                            across_indx++;

                            if (dgvTabVisible.Rows.Count < (comp_indx + 1))
                                dgvTabVisible.Rows.Add();
                            else
                                ;

                            dgvTabVisible.Rows[comp_indx].Cells[col].Value = t.name_shr + " - " + g.name_shr;
                            m_dicTabVisibleIdItems.Add(g.m_id, new int[] { comp_indx, col - 1 });
                        }
                    }
                    else
                        ;
                }
            else
                ; //m_listTEC == null
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

    public class FormMainAnalyzer_TCPIP : FormMainAnalyzer
    {
        TcpClientAsync m_tcpClient;
        List<TcpClientAsync> m_listTCPClientUsers;

        public FormMainAnalyzer_TCPIP(int idListener, List<TEC> tec) : base (idListener, tec)
        {
        }

        /// <summary>
        /// Обмен данными (чтение) с приложением "Статистика" пользователя
        /// </summary>
        /// <param name="res">соединение с приложением пользователя</param>
        /// <param name="rec">ответ от приложения пользователя</param>
        void Read(TcpClient res, string rec)
        {
            bool bResOk = rec.Split('=')[1].Split(';')[0].Equals("OK", StringComparison.InvariantCultureIgnoreCase);

            if (bResOk == true)
            {
                //Message from Analyzer CMD;ARG1, ARG2,...,ARGN=RESULT
                switch (rec.Split('=')[0].Split(';')[0])
                {
                    case "INIT":
                        int indxTcpClient = getIndexTcpClient(res);

                        if (indxTcpClient < m_listTCPClientUsers.Count)
                        {
                            dgvClient.Rows[indxTcpClient].Cells[0].Value = true;
                            m_listTCPClientUsers[indxTcpClient].Write(@"DISCONNECT");

                            m_listTCPClientUsers[indxTcpClient].Disconnect();
                        }
                        else
                            ;
                        break;
                    case "LOG_LOCK":
                        //rec.Split('=')[1].Split(';')[1] - полный путь лог-файла
                        StartLogParse(rec.Split('=')[1].Split(';')[1]);

                        m_tcpClient.Write("LOG_UNLOCK=?");
                        break;
                    case "LOG_UNLOCK":
                        Disconnect();
                        break;
                    case "TAB_VISIBLE":
                        string[] recParameters = rec.Split('=')[1].Split(';'); //список отображаемых вкладок пользователя
                        int i = -1,
                            mode = -1
                            //, key = -1
                            ;
                        int[] IdItems;
                        string[] indexes = null;
                        bool bChecked = false;

                        if (recParameters.Length > 1)
                        {
                            mode = Convert.ToInt32(recParameters[1]);

                            BeginInvoke(new DelegateIntFunc(SetModeVisibleTabs), mode);

                            if (recParameters.Length > 2)
                            {
                                indexes = recParameters[2].Split(',');
                                //arIndexes = recParameters[2].Split(',').ToArray <int> ();

                                IdItems = new int[indexes.Length];
                                for (i = 0; i < indexes.Length; i++)
                                    IdItems[i] = Convert.ToInt32(indexes[i]);

                                foreach (KeyValuePair<int, int[]> pair in m_dicTabVisibleIdItems)
                                {
                                    if (IdItems.Contains(pair.Key) == true)
                                        bChecked = true;
                                    else
                                        bChecked = false;

                                    dgvTabVisible.Rows[m_dicTabVisibleIdItems[pair.Key][0]].Cells[m_dicTabVisibleIdItems[pair.Key][1]].Value = bChecked;
                                }
                            }
                            else
                                //Не отображается ни одна вкладка
                                TabVisibliesClearChecked();
                        }
                        else
                        {// !Ошибка! Не переданы индексы отображаемых вкладок
                        }

                        Disconnect();
                        break;
                    case "DISCONNECT":
                        break;
                    case "":
                        break;
                    default:
                        break;
                }
            }
            else
                ;
        }

        private int getIndexTcpClient(TcpClient obj)
        {
            int i = -1;
            for (i = 0; i < m_listTCPClientUsers.Count; i++)
            {
                if (m_listTCPClientUsers[i].Equals(obj) == true)
                    break;
                else
                    ;
            }

            return i;
        }

        protected void ConnectToChecked(TcpClient res, string data)
        {
            int indxTcpClient = getIndexTcpClient(res);

            if (indxTcpClient < m_listTCPClientUsers.Count)
                m_listTCPClientUsers[indxTcpClient].Write(@"INIT=?");
            else
                ;
        }

        protected void ConnectToLogRead(TcpClient res, string data)
        {
            m_tcpClient.Write("LOG_LOCK=?");
        }

        protected void ConnectToTab(TcpClient res, string data)
        {
            m_tcpClient.Write("TAB_VISIBLE=?");
        }

        protected override void Thread_ProcCheckedStart()
        {
            m_listTCPClientUsers = new List<TcpClientAsync>();
            for (int i = 0; i < m_tableUsers.Rows.Count; i++)
            {
                //Проверка активности
                m_listTCPClientUsers.Add(new TcpClientAsync());
                m_listTCPClientUsers[i].delegateConnect = ConnectToChecked;
                m_listTCPClientUsers[i].delegateErrorConnect = ErrorConnect;
                m_listTCPClientUsers[i].delegateRead = Read;
                //m_listTCPClientUsers[i].Connect (m_tableUsers.Rows[i][NameFieldToConnect].ToString(), 6666);
            }

            base.Thread_ProcCheckedStart ();
        }

        protected override void ProcChecked()
        {
            int i = -1;
            for (i = 0; (i < m_tableUsers.Rows.Count) && (m_bThreadCheckedAllowed == true); i++)
            {
                //Проверка активности
                m_listTCPClientUsers[i].Connect(m_tableUsers.Rows[i][c_NameFieldToConnect].ToString() + ";" + i, 6666);
            }
        }

        protected override void Thread_ProcCheckedStop()
        {
            base.Thread_ProcCheckedStop ();

            if (!(m_listTCPClientUsers == null))
                m_listTCPClientUsers.Clear();
            else
                ;
        }

        protected override void Disconnect()
        {
            if (!(m_tcpClient == null))
            {
                m_tcpClient.Write(@"DISCONNECT");
                m_tcpClient.Disconnect();

                m_tcpClient = null;
            }
            else
                ;
        }

        protected override void dgvClient_SelectionChanged(object sender, EventArgs e)
        {
            if (!(m_tcpClient == null))
            {
                Disconnect();
            }
            else
                ;

            if ((dgvClient.SelectedRows.Count > 0) && (!(dgvClient.SelectedRows[0].Index < 0)))
            {
                bool bUpdate = true;
                if (tabControlAnalyzer.SelectedIndex == 0)
                    if ((dgvDatetimeStart.Rows.Count > 0) && (dgvDatetimeStart.SelectedRows[0].Index < (dgvDatetimeStart.Rows.Count - 1)))
                        if (e == null)
                            bUpdate = false;
                        else
                            ;
                    else
                        ;
                else
                    ;

                if (bUpdate == true)
                {
                    m_tcpClient = new TcpClientAsync();
                    m_tcpClient.delegateRead = Read;

                    switch (tabControlAnalyzer.SelectedIndex)
                    {
                        case 0:
                            //Останов потока разбора лог-файла пред. пользователя
                            m_LogParse.Stop();

                            dgvDatetimeStart.SelectionChanged -= dgvDatetimeStart_SelectionChanged;

                            //Очистить элементы управления с данными от пред. лог-файла
                            if (IsHandleCreated/*InvokeRequired*/ == true)
                            {
                                BeginInvoke(new DelegateFunc(TabLoggingClearDatetimeStart));
                                BeginInvoke(new DelegateFunc(TabLoggingClearText));
                            }
                            else
                                Logging.Logg().Error(@"FormMainAnalyzer::dgvClient_SelectionChanged () - ... BeginInvoke (TabLoggingClearDatetimeStart, TabLoggingClearText) - ...", Logging.INDEX_MESSAGE.D_001);

                            //Если активна 0-я вкладка (лог-файл)
                            m_tcpClient.delegateConnect = ConnectToLogRead;
                            break;
                        case 1:
                            //Очистить элементы управления с данными от пред. пользователя
                            if (IsHandleCreated/*InvokeRequired*/ == true)
                            {
                                BeginInvoke(new DelegateIntFunc(SetModeVisibleTabs), 0);
                                BeginInvoke(new DelegateFunc(TabVisibliesClearChecked));
                            }
                            else
                                Logging.Logg().Error(@"FormMainAnalyzer::dgvClient_SelectionChanged () - ... BeginInvoke (SetModeVisibleTabs, TabVisibliesClearChecked) - ...", Logging.INDEX_MESSAGE.D_001);

                            //Если активна 1-я вкладка (вкладки)
                            m_tcpClient.delegateConnect = ConnectToTab;
                            break;
                        default:
                            break;
                    }

                    m_tcpClient.delegateErrorConnect = ErrorConnect;

                    //m_tcpClient.Connect("localhost", 6666);
                    m_tcpClient.Connect(m_tableUsers.Rows[dgvClient.SelectedRows[0].Index][c_NameFieldToConnect].ToString() + ";" + dgvClient.SelectedRows[0].Index, 6666);
                }
                else
                    ; //Обновлять нет необходимости
            }
            else
                ;
        }
    }

    public class FormMainAnalyzer_DB : FormMainAnalyzer
    {
        private DbConnection m_connLoggingDB;

        public FormMainAnalyzer_DB(int idListener, List<TEC> tec)
            : base(idListener, tec)
        {
            int err = -1
                , iListenerId = -1
                , idMainDB = -1;

            idMainDB = Int32.Parse (DbTSQLInterface.Select(ref m_connConfigDB, @"SELECT [VALUE] FROM [setup] WHERE [KEY]='" + @"Main DataSource" + @"'", null, null, out err).Rows[0][@"VALUE"].ToString ());
            DataTable tblConnSettMainDB = ConnectionSettingsSource.GetConnectionSettings(TYPE_DATABASE_CFG.CFG_200, ref m_connConfigDB, idMainDB, -1, out err);
            ConnectionSettings connSettMainDB = new ConnectionSettings(tblConnSettMainDB.Rows[0], false);
            iListenerId = DbSources.Sources().Register(connSettMainDB, false, @"");
            m_connLoggingDB = DbSources.Sources().GetConnection(iListenerId, out err);
        }

        protected override void Thread_ProcCheckedStart()
        {            
            base.Thread_ProcCheckedStart ();
        }

        protected override void Thread_ProcCheckedStop ()
        {
            base.Thread_ProcCheckedStop ();
        }

        protected override void ProcChecked()
        {
            int err = -1
                , i = -1
                , msecSleep = -1;
            if (! (m_connLoggingDB == null))
            {
                DataTable tblMaxDatetimeWR = DbTSQLInterface.Select(ref m_connLoggingDB, @"SELECT [ID_USER], MAX([DATETIME_WR]) as MAX_DATETIME_WR FROM logging GROUP BY [ID_USER] ORDER BY [ID_USER]", null, null, out err);
                DataRow [] rowsMaxDatetimeWR;
                if (err == 0)
                    for (i = 0; (i < m_tableUsers.Rows.Count) && (m_bThreadCheckedAllowed == true); i++)
                    {
                        //Проверка активности
                        rowsMaxDatetimeWR = tblMaxDatetimeWR.Select(@"[ID_USER]=" + m_tableUsers.Rows[i][@"ID"]);

                        bool bActive = false;
                        if (rowsMaxDatetimeWR.Length == 0) { //В течении 2-х недель нет ни одного запуска на выполнение ППО
                        }
                        else {
                            if (rowsMaxDatetimeWR.Length > 1) { //Ошибка
                            }
                            else {
                                //Обрабатываем...
                                bActive = (DateTime.Now - DateTime.Parse(rowsMaxDatetimeWR[0][@"MAX_DATETIME_WR"].ToString())).TotalSeconds < 66;
                            }
                        }

                        dgvClient.Rows[i].Cells[0].Value = bActive;
                    }
                else
                    ; //Ошибка при выборке данных...

                msecSleep = 66666;
            }
            else
                msecSleep = 666; //Нет соединения с БД...

            Thread.Sleep (msecSleep);
        }

        protected override void dgvClient_SelectionChanged(object sender, EventArgs e)
        {
        }

        protected override void Disconnect()
        {
        }
    }
}
