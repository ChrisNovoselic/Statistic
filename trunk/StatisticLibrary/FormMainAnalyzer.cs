﻿using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Data.Common; //DbConnection

using System.Net;
using System.Net.Sockets;

using System.IO;

using HClassLibrary;

namespace StatisticCommon
{
    public abstract partial class FormMainAnalyzer : Form //FormMainBase//: FormMainBaseWithStatusStrip
    {
        protected System.Threading.Timer m_timerChecked;
        protected bool m_bThreadTimerCheckedAllowed;
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

        protected enum INDEX_DELIMETER {PART, ROW};
        protected static string[] m_chDelimeters = { @"DELIMETER_PART", "DELIMETER_ROW" };

        partial class PanelAnalyzer : PanelStatistic
        {
            public PanelAnalyzer()
                : base()
            {
                InitializeComponent();
            }            
        }

        public FormMainAnalyzer(int idListener, List <TEC> tec)
        {
            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
                ProgramBase.ss_MainCultureInfo;

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
            //dgvFilterActives.ReadOnly = false;
            dgvFilterActives.Columns[0].ReadOnly = false;
            dgvFilterActives.CellClick += new DataGridViewCellEventHandler(dgvFilterActives_CellClick);
            dgvFilterActives.Enabled = true;

            int err = -1;

            m_connConfigDB = DbSources.Sources().GetConnection(idListener, out err);
            //DbConnection connDB = DbTSQLInterface.GetConnection(m_connSettConfigDB, out err);

            if ((! (m_connConfigDB == null)) && (err == 0))
            {
                HStatisticUsers.GetRoles(ref m_connConfigDB, string.Empty, string.Empty, out m_tableRoles, out err);
                FillDataGridViews(ref dgvFilterRoles, m_tableRoles, @"DESCRIPTION", err, true);

                HStatisticUsers.GetUsers(ref m_connConfigDB, string.Empty, c_list_sorted, out m_tableUsers, out err);
                FillDataGridViews(ref dgvClient, m_tableUsers, @"DESCRIPTION", err);

                if (this is FormMainAnalyzer_DB)
                    m_LogParse = new LogParse_DB ();
                else
                    if (this is FormMainAnalyzer_TCPIP)
                        m_LogParse = new LogParse_File();
                    else
                        ;

                fillTypeMessage();

                if (! (m_LogParse == null))
                {
                    m_LogParse.Exit = LogParseExit;

                    Thread_ProcCheckedStart ();
                }
                else
                    ;
            }
            else 
                ;
        }

        protected abstract void procChecked(out bool[] arbActives, out int err);
        protected abstract void ProcChecked(object obj);

        /// <summary>
        /// Отключение от клиента (активного пользователя)
        /// </summary>
        protected abstract void Disconnect();

        protected abstract void fillTypeMessage();

        protected void fillTypeMessage(string []strTypeMessages)
        {
            dgvTypeMessage.Rows.Add(strTypeMessages.Length);
            for (int i = 0; i < strTypeMessages.Length; i++)
            {
                dgvTypeMessage.Rows[(int)i].Cells[0].Value = true;
                dgvTypeMessage.Rows[(int)i].Cells[1].Value = strTypeMessages[i];
                dgvTypeMessage.Rows[(int)i].Cells[2].Value = 0;
            }
        }

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
            m_bThreadTimerCheckedAllowed = true;

            m_timerChecked = new System.Threading.Timer(new TimerCallback (ProcChecked), null, 0, System.Threading.Timeout.Infinite);
        }

        protected virtual void Thread_ProcCheckedStop ()
        {
            m_bThreadTimerCheckedAllowed = false;
            
            if (! (m_timerChecked == null))
            {                    
                m_timerChecked.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                m_timerChecked.Dispose();
                m_timerChecked = null;
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

            Logging.Logg ().Error (string.Format ("FormAnalyzer::ErrorConnect () - {0}, индекс: {1}", ValueToCreate.Split(';')[0], ValueToCreate.Split(';')[1]), Logging.INDEX_MESSAGE.NOT_SET);

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

        protected void TabLoggingClearText() { m_LogParse.Clear(); /*textBoxLog.Clear();*/ dgvLogMessage.Rows.Clear(); }

        void TabLoggingPositionText()
        {
            /*
            textBoxLog.Select(0, 0);
            textBoxLog.ScrollToCaret();
            */

            dgvLogMessage.FirstDisplayedScrollingRowIndex = 0;
        }

        void TabLoggingAppendText (string rows)
        {
            if (rows.Equals(string.Empty) == true)
            {
                TabLoggingPositionText();
                //textBoxLog.Focus();
                //textBoxLog.AutoScrollOffset = new Point (0, 0);
            }
            else
            {
                int[] arTypeLogMsgCounter = new int[dgvTypeMessage.Rows.Count];
                List<int> listIdTypeMessages = LogParse_DB.s_IdTypeMessages.ToList();

                string[] messages = rows.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.ROW] }, StringSplitOptions.None);
                string[] parts;
                int i = -1;

                foreach (string text in messages)
                {
                    parts = text.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None);
                    dgvLogMessage.Rows.Add(parts);

                    i = listIdTypeMessages[Int32.Parse(parts[1])];
                    if ((i < arTypeLogMsgCounter.Length) && (!(i < 0)))
                    {
                        arTypeLogMsgCounter[i]++;
                        dgvTypeMessage.Rows[i].Cells[2].Value = arTypeLogMsgCounter[i];
                    }
                    else
                        Logging.Logg().Error(@"FormMainAnalyzer::TabLoggingAppendText () - неизвестный тип сообщения = " + parts[1], Logging.INDEX_MESSAGE.NOT_SET);
                }

                //for (i = 0; i < dgvTypeMessage.Rows.Count; i++)
                //    dgvTypeMessage.Rows[i].Cells[2].Value = arTypeLogMsgCounter[i];
            }
        }

        void TabLoggingAppendDatetimeStart(string rows)
        {
            string[] strDatetimeStartRows = rows.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.ROW] }, StringSplitOptions.None);

            bool bRowChecked = false;

            foreach (string row in strDatetimeStartRows)
            {
                bRowChecked = bool.Parse(row.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None)[0]);

                if (bRowChecked == true)
                {
                    dgvDatetimeStart.SelectionChanged += dgvDatetimeStart_SelectionChanged;
                    m_prevDatetimeRowIndex = dgvDatetimeStart.Rows.Count;
                }
                else
                    ;

                dgvDatetimeStart.Rows.Add(new object[] { bRowChecked, row.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None)[1] });
            }

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

        protected abstract void StartLogParse (string par);

        private void dgvFilterActives_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int i = -1, err = -1;

            if (e.ColumnIndex == 0)
            {
                m_timerChecked.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                dgvFilterActives.Rows[e.RowIndex].Cells[0].Value = !bool.Parse(dgvFilterActives.Rows[e.RowIndex].Cells[0].Value.ToString());

                HStatisticUsers.GetUsers(ref m_connConfigDB, string.Empty, c_list_sorted, out m_tableUsers, out err);

                if (dgvFilterActives.Rows[0].Cells[0].Value == dgvFilterActives.Rows[1].Cells[0].Value)
                    if (bool.Parse(dgvFilterActives.Rows[0].Cells[0].Value.ToString()) == true)
                        //Отображать всех...
                        ;
                    else
                        //Пустой список...
                        m_tableUsers.Clear ();
                else
                {
                    bool[] arbActives;
                    procChecked(out arbActives, out err);

                    if ((!(arbActives == null)) && (err == 0))
                    {
                        List<int> listIndexToRemoveUsers = new List<int>();

                        for (i = 0; (i < m_tableUsers.Rows.Count) && (i < arbActives.Length); i++)
                        {
                            if (((arbActives[i] == true) && (bool.Parse(dgvFilterActives.Rows[0].Cells[0].Value.ToString()) == false))
                                || ((arbActives[i] == false) && (bool.Parse(dgvFilterActives.Rows[1].Cells[0].Value.ToString()) == false)))
                            {
                                listIndexToRemoveUsers.Add(i);
                            }
                            else
                                ;
                        }

                        if (listIndexToRemoveUsers.Count > 0)
                        {
                            listIndexToRemoveUsers.Sort(delegate(int i1, int i2) { return i1 > i2 ? -1 : 1; });

                            //Удалить обработанные сообщения
                            foreach (int indx in listIndexToRemoveUsers)
                                m_tableUsers.Rows.RemoveAt(indx);

                            m_tableUsers.AcceptChanges();
                        }
                        else
                            ;
                    }
                    else
                        ;
                }

                FillDataGridViews(ref dgvClient, m_tableUsers, @"DESCRIPTION", err);

                m_timerChecked.Change(0, System.Threading.Timeout.Infinite);
            }
            else
                ;
        }

        private void dgvFilterRoles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int i = -1, err = -1;
            string where = string.Empty;

            if (e.ColumnIndex == 0)
            {
                //Thread_ProcCheckedStop();
                m_timerChecked.Change (System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

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

                m_timerChecked.Change(0, System.Threading.Timeout.Infinite);
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
            string strDatetimeStart = string.Empty;
            i = m_LogParse.Select(0, string.Empty, string.Empty, ref rows);
            for (i = 0; i < rows.Length; i ++)
            {
                if (i == (rows.Length - 1))
                    rowChecked = true;
                else
                    ;

                strDatetimeStart += rowChecked.ToString()
                        + m_chDelimeters[(int)INDEX_DELIMETER.PART]
                        + rows[i]["DATE_TIME"].ToString()
                        + m_chDelimeters[(int)INDEX_DELIMETER.ROW];
            }

            if (strDatetimeStart.Length > 0)
            {
                strDatetimeStart = strDatetimeStart.Substring(0, strDatetimeStart.Length - m_chDelimeters[(int)INDEX_DELIMETER.ROW].Length);
                BeginInvoke(delegateAppendDatetimeStart, strDatetimeStart);
            }
            else
                ;
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            dgvClient_SelectionChanged (null, null);
        }

        protected abstract int SelectLogMessage (int type, DateTime beg, DateTime end, ref DataRow []rows);

        private void filldgvLogMessages(object data)
        {
            //DateTime [] arDT = (DateTime [])data;
            DataRow[] rows = new DataRow[] { };
            int i = SelectLogMessage(-1, ((DateTime[])data)[0], /*((DateTime[])data)[1]*/((DateTime[])data)[0].AddDays (1), ref rows);

            string strRowsTodgvLogMessages = string.Empty;

            if (rows.Length > 0)
            {
                for (i = 0; i < rows.Length; i++)
                {
                    if (((i % 1000) == 0) && (strRowsTodgvLogMessages.Length > m_chDelimeters[(int)INDEX_DELIMETER.ROW].Length))
                    {
                        strRowsTodgvLogMessages = strRowsTodgvLogMessages.Substring(0, strRowsTodgvLogMessages.Length - m_chDelimeters[(int)INDEX_DELIMETER.ROW].Length);
                        this.BeginInvoke(new DelegateStringFunc(TabLoggingAppendText), strRowsTodgvLogMessages);
                        strRowsTodgvLogMessages = string.Empty;
                    }
                    else
                        strRowsTodgvLogMessages += getTabLoggingTextRow(rows[i]) + m_chDelimeters[(int)INDEX_DELIMETER.ROW];
                }

                if (strRowsTodgvLogMessages.Length > m_chDelimeters[(int)INDEX_DELIMETER.ROW].Length)
                {
                    //Остаток...                    
                    strRowsTodgvLogMessages = strRowsTodgvLogMessages.Substring(0, strRowsTodgvLogMessages.Length - m_chDelimeters[(int)INDEX_DELIMETER.ROW].Length);
                    this.BeginInvoke(new DelegateStringFunc(TabLoggingAppendText), strRowsTodgvLogMessages);
                }
                else
                    ;
            }
            else
                ;

            //Крайняя строка...
            this.BeginInvoke(new DelegateStringFunc(TabLoggingAppendText), string.Empty);
        }
        
        protected void dgvDatetimeStart_SelectionChanged(object sender, EventArgs e)
        {
            int i = -1;
            int rowIndex = dgvDatetimeStart.SelectedRows [0].Index;

            dgvDatetimeStart.Rows [m_prevDatetimeRowIndex].Cells [0].Value = false;
            dgvDatetimeStart.Rows[rowIndex].Cells[0].Value = true;
            m_prevDatetimeRowIndex = rowIndex;

            TabLoggingClearText ();

            DateTime dtBegin = DateTime.Parse(dgvDatetimeStart.Rows[rowIndex].Cells[1].Value.ToString())
                , dtEnd = DateTime.MaxValue;
            if ((rowIndex + 1) < dgvDatetimeStart.Rows.Count)
                dtEnd = DateTime.Parse(dgvDatetimeStart.Rows[rowIndex + 1].Cells[1].Value.ToString ());
            else
                ;

            Thread threadFilldgvLogMessages = new Thread(new ParameterizedThreadStart(filldgvLogMessages));
            threadFilldgvLogMessages.IsBackground = true;
            threadFilldgvLogMessages.Start(new DateTime [] { dtBegin, dtEnd });
        }

        protected abstract string getTabLoggingTextRow(DataRow r);        

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

        protected override void fillTypeMessage()
        {
            fillTypeMessage(LogParse_File.DESC_LOGMESSAGE);
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

        protected override void procChecked(out bool[] arbActives, out int err)
        {
            throw new NotImplementedException();
        }

        protected override void ProcChecked(object obj)
        {
            int i = -1;
            for (i = 0; (i < m_tableUsers.Rows.Count) && (m_bThreadTimerCheckedAllowed == true); i++)
            {
                //Проверка активности
                m_listTCPClientUsers[i].Connect(m_tableUsers.Rows[i][c_NameFieldToConnect].ToString() + ";" + i, 6666);
            }

            m_timerChecked.Change (66666, System.Threading.Timeout.Infinite);
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

        protected override void StartLogParse(string full_path)
        {
            FileInfo fi = new FileInfo(full_path);
            FileStream fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("windows-1251"));

            dgvDatetimeStart.SelectionChanged -= dgvDatetimeStart_SelectionChanged;

            m_LogParse.Start(sr.ReadToEnd());

            sr.Close();
        }

        protected override int SelectLogMessage(int type, DateTime beg, DateTime end, ref DataRow[] rows)
        {
            return m_LogParse.Select(-1, beg.ToShortDateString (), end.ToShortDateString (), ref rows);
        }

        protected override string getTabLoggingTextRow(DataRow r)
        {
            string strRes = string.Empty;

            if (Convert.ToInt32(r["TYPE"]) == (int)LogParse_File.TYPE_LOGMESSAGE.DETAIL)
                strRes = "    " + r["MESSAGE"].ToString() + Environment.NewLine;
            else
                strRes = "[" + r["DATE_TIME"] + "]: " + r["MESSAGE"].ToString() + Environment.NewLine;

            return strRes;
        }
    }

    public class FormMainAnalyzer_DB : FormMainAnalyzer
    {
        //class HLogMsgSource {
            //public
                DelegateIntFunc delegateConnect;
            //public
                DelegateFunc delegateErrorConnect;
        //}
        //HLogMsgSource m_logMsgSource;

        private int m_idListenerLoggingDB;

        public FormMainAnalyzer_DB(int idListener, List<TEC> tec)
            : base(idListener, tec)
        {
        }

        protected override void Thread_ProcCheckedStart()
        {
            int err = -1
                , idMainDB = -1;

            idMainDB = Int32.Parse(DbTSQLInterface.Select(ref m_connConfigDB, @"SELECT [VALUE] FROM [setup] WHERE [KEY]='" + @"Main DataSource" + @"'", null, null, out err).Rows[0][@"VALUE"].ToString());
            DataTable tblConnSettMainDB = ConnectionSettingsSource.GetConnectionSettings(TYPE_DATABASE_CFG.CFG_200, ref m_connConfigDB, idMainDB, -1, out err);
            ConnectionSettings connSettMainDB = new ConnectionSettings(tblConnSettMainDB.Rows[0], 1);
            m_idListenerLoggingDB = DbSources.Sources().Register(connSettMainDB, false, @"MAIN_DB", false);

            base.Thread_ProcCheckedStart ();
        }

        protected override void Thread_ProcCheckedStop ()
        {
            DbSources.Sources().UnRegister(m_idListenerLoggingDB);

            base.Thread_ProcCheckedStop ();
        }

        protected override void procChecked(out bool []arbActives, out int err)
        {
            err = 0;
            arbActives = null;

            int i = -1;
            DbConnection connLoggingDB = DbSources.Sources().GetConnection(m_idListenerLoggingDB, out err);
            if (!(connLoggingDB == null) && (err == 0))
            {
                DataTable tblMaxDatetimeWR = DbTSQLInterface.Select(ref connLoggingDB, @"SELECT [ID_USER], MAX([DATETIME_WR]) as MAX_DATETIME_WR FROM logging GROUP BY [ID_USER] ORDER BY [ID_USER]", null, null, out err);
                DataRow[] rowsMaxDatetimeWR;
                if (err == 0)
                {
                    arbActives = new bool[m_tableUsers.Rows.Count];

                    for (i = 0; (i < m_tableUsers.Rows.Count) && (m_bThreadTimerCheckedAllowed == true); i++)
                    {
                        //Проверка активности
                        rowsMaxDatetimeWR = tblMaxDatetimeWR.Select(@"[ID_USER]=" + m_tableUsers.Rows[i][@"ID"]);

                        if (rowsMaxDatetimeWR.Length == 0)
                        { //В течении 2-х недель нет ни одного запуска на выполнение ППО
                        }
                        else
                        {
                            if (rowsMaxDatetimeWR.Length > 1)
                            { //Ошибка
                            }
                            else
                            {
                                //Обрабатываем...
                                arbActives[i] = (DateTime.Now - DateTime.Parse(rowsMaxDatetimeWR[0][@"MAX_DATETIME_WR"].ToString())).TotalSeconds < 66;
                            }
                        }
                    }
                }
                else
                    err = -2; //Ошибка при выборке данных...
            }
            else
                err = -1; //Нет соединения с БД...
        }

        protected override void ProcChecked(object obj)
        {
            int err = -1
                , i = -1
                , msecSleep = System.Threading.Timeout.Infinite;
            bool[] arbActives;

            procChecked(out arbActives, out err);
            if (!(arbActives == null) && (err == 0))
            {
                for (i = 0; (i < m_tableUsers.Rows.Count) && (m_bThreadTimerCheckedAllowed == true) && (i < arbActives.Length); i++)
                    dgvClient.Rows[i].Cells[0].Value = arbActives[i];

                msecSleep = 66666;
            }
            else
                msecSleep = 666; //Нет соединения с БД...

            m_timerChecked.Change(msecSleep, System.Threading.Timeout.Infinite);
        }

        protected override void fillTypeMessage()
        {
            fillTypeMessage(LogParse_DB.DESC_LOGMESSAGE);
        }

        protected override void dgvClient_SelectionChanged(object sender, EventArgs e)
        {
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
                    switch (tabControlAnalyzer.SelectedIndex)
                    {
                        case 0:
                            //Останов потока разбора лог-файла пред. пользователя
                            m_LogParse.Stop();

                            dgvDatetimeStart.SelectionChanged -= dgvDatetimeStart_SelectionChanged;

                            //Очистить элементы управления с данными от пред. лог-файла
                            if (IsHandleCreated == true)
                                if (InvokeRequired == true)
                                {
                                    BeginInvoke(new DelegateFunc(TabLoggingClearDatetimeStart));
                                    BeginInvoke(new DelegateFunc(TabLoggingClearText));
                                }
                                else {
                                    TabLoggingClearDatetimeStart ();
                                    TabLoggingClearText ();
                                }
                            else
                                Logging.Logg().Error(@"FormMainAnalyzer_DB::dgvClient_SelectionChanged () - ... BeginInvoke (TabLoggingClearDatetimeStart, TabLoggingClearText) - ...", Logging.INDEX_MESSAGE.D_001);

                            //Если активна 0-я вкладка (лог-файл)
                            delegateConnect = ConnectToLogRead;
                            break;
                        case 1:
                            //Очистить элементы управления с данными от пред. пользователя
                            if (IsHandleCreated == true)
                            {
                                if (InvokeRequired == true)
                                {
                                    BeginInvoke(new DelegateIntFunc(SetModeVisibleTabs), 0);
                                    BeginInvoke(new DelegateFunc(TabVisibliesClearChecked));
                                }
                                else
                                {
                                    SetModeVisibleTabs(0);
                                    TabVisibliesClearChecked();
                                }
                            }
                            else
                                Logging.Logg().Error(@"FormMainAnalyzer_DB::dgvClient_SelectionChanged () - ... BeginInvoke (SetModeVisibleTabs, TabVisibliesClearChecked) - ...", Logging.INDEX_MESSAGE.D_001);

                            //Если активна 1-я вкладка (вкладки)
                            delegateConnect = ConnectToTab;
                            break;
                        default:
                            break;
                    }

                    delegateErrorConnect = ErrorConnect;

                    ////m_tcpClient.Connect("localhost", 6666);
                    //m_tcpClient.Connect(m_tableUsers.Rows[dgvClient.SelectedRows[0].Index][c_NameFieldToConnect].ToString() + ";" + dgvClient.SelectedRows[0].Index, 6666);
                    getLogMsg(m_tableUsers.Rows[dgvClient.SelectedRows[0].Index], dgvClient.SelectedRows[0].Index);
                }
                else
                    ; //Обновлять нет необходимости
            }
            else
                ;
        }

        protected override void Disconnect()
        {
        }

        private void ConnectToLogRead (int id) {
            StartLogParse (id.ToString ());
        }

        private void ConnectToTab(int id)
        {
        }

        private void ErrorConnect()
        {
        }

        private void getLogMsg (DataRow rowUser, int indxRowUser)
        {            
            if (! (m_idListenerLoggingDB < 0))
                delegateConnect(Int32.Parse (rowUser[@"ID"].ToString ()));
            else
                delegateErrorConnect ();
        }

        protected override void StartLogParse (string id)
        {
            dgvDatetimeStart.SelectionChanged -= dgvDatetimeStart_SelectionChanged;

            m_LogParse.Start (m_chDelimeters[(int)INDEX_DELIMETER.PART].Length
                    + m_chDelimeters[(int)INDEX_DELIMETER.PART]
                    + m_idListenerLoggingDB.ToString ()
                    + m_chDelimeters[(int)INDEX_DELIMETER.PART] + id);
        }

        protected override int SelectLogMessage(int type, DateTime beg, DateTime end, ref DataRow[] rows)
        {
            return (m_LogParse as LogParse_DB).Select(m_idListenerLoggingDB, -1, beg, end, ref rows);
        }

        protected override string getTabLoggingTextRow(DataRow r)
        {
            string strRes = string.Empty;

            strRes = string.Join(m_chDelimeters[(int)INDEX_DELIMETER.PART].ToString()
                                    , new string[] {
                                        r["DATE_TIME"].ToString ()
                                        , r["TYPE"].ToString ()
                                        , r["MESSAGE"].ToString()
                                    }
            );

            return strRes;
        }        
    }

    public class LogParse_DB : LogParse
    {
        public enum TYPE_LOGMESSAGE { START, STOP, ACTION, DEBUG, EXCEPTION, EXCEPTION_DB, ERROR, WARNING, UNKNOWN, COUNT_TYPE_LOGMESSAGE };
        public static string[] DESC_LOGMESSAGE = { "Запуск", "Выход",
                                            "Действие", "Отладка", "Исключение",
                                            "Исключение БД",
                                            "Ошибка",
                                            "Предупреждение",
                                            "Неопределенный тип" };

        private int m_idUser;

        public LogParse_DB () : base ()
        {
            m_idUser = -1;

            s_IdTypeMessages = new int[] {
                1, 2, 3, 4, 5, 6, 7, 8, 9
                , 10
            };
        }

        protected override void Thread_Proc(object data)
        {
            int err = -1
                , lDelimeter = 0;

            while (Char.IsDigit((data as string).ToCharArray()[lDelimeter]) == true) lDelimeter++;
            string strDelimeter = (data as string).Substring(lDelimeter, Int32.Parse((data as string).Substring (0, lDelimeter)));
            string text = (data as string).Substring(lDelimeter + strDelimeter.Length, (data as string).Length - (lDelimeter + strDelimeter.Length));
            DbConnection connLoggingDB = DbSources.Sources().GetConnection(Int32.Parse(text.Split(new string [] { strDelimeter }, StringSplitOptions.None)[0]), out err);
            m_idUser = Int32.Parse(text.Split(new string[] { strDelimeter }, StringSplitOptions.None)[1]);

            string query = @"SELECT DATEPART (DD, [DATETIME_WR]) as DD, DATEPART (MM, [DATETIME_WR]) as MM, DATEPART (YYYY, [DATETIME_WR]) as [YYYY], COUNT(*) as CNT"
                    + @" FROM [techsite-2.X.X].[dbo].[logging]"
                    + @" WHERE [ID_USER]=" + m_idUser
                    + @" GROUP BY DATEPART (DD, [DATETIME_WR]), DATEPART (MM, [DATETIME_WR]), DATEPART (YYYY, [DATETIME_WR])"
                    + @" ORDER BY [DD]";

            DataTable tblDatetimeStart = DbTSQLInterface.Select(ref connLoggingDB, query, null, null, out err);
            DateTime dtStart;

            int cnt = 0;
            for (int i = 0; i < tblDatetimeStart.Rows.Count; i ++)
            {
                dtStart = new DateTime (Int32.Parse(tblDatetimeStart.Rows[i][@"YYYY"].ToString ())
                                                , Int32.Parse(tblDatetimeStart.Rows[i][@"MM"].ToString ())
                                                , Int32.Parse(tblDatetimeStart.Rows[i][@"DD"].ToString ()));
                m_tableLog.Rows.Add(new object[] { dtStart, s_IdTypeMessages [(int)TYPE_LOGMESSAGE.START], ProgramBase.MessageWellcome });

                cnt += Int32.Parse(tblDatetimeStart.Rows[i][@"CNT"].ToString());
            }

            base.Thread_Proc(cnt);
        }

        public int Select(int iListenerId, int type, DateTime beg, DateTime end, ref DataRow[] res)
        {
            int iRes = -1;
            string where = string.Empty;

            m_tableLog.Clear();

            DbConnection connLoggingDB = DbSources.Sources ().GetConnection (iListenerId, out iRes);

            if (iRes == 0)
            {
                if (beg.Equals(DateTime.MaxValue) == false)
                {
                    //Вариан №1 диапазон даты/времени
                    where = "DATETIME_WR>='" + beg.ToString("yyyyMMdd HH:mm:ss") + "'";
                    if (end.Equals(DateTime.MaxValue) == false)
                        where += " AND DATETIME_WR<'" + end.ToString("yyyyMMdd HH:mm:ss") + "'";
                    else ;
                    ////Вариан №2 указанные сутки
                    //where = "DATETIME_WR='" + beg.ToString("yyyyMMdd") + "'";
                }
                else
                    ;

                if (!(type < 0))
                {
                    if (where.Equals(string.Empty) == false)
                        where += " AND ";
                    else
                        ;

                    where += "TYPE=" + type;
                }
                else
                    ;

                string query = @"SELECT DATETIME_WR as DATE_TIME, ID_LOGMSG as TYPE, MESSAGE FROM logging WHERE ID_USER=" + m_idUser;
                if (where.Equals(string.Empty) == false)
                    query += " AND " + where;
                else
                    ;
                m_tableLog = DbTSQLInterface.Select(ref connLoggingDB, query, null, null, out iRes);
            }
            else
                ;

            if (iRes == 0) {
                iRes = m_tableLog.Rows.Count;
                res  = m_tableLog.Select (string.Empty, @"DATE_TIME");
            }
            else
                ;

            return iRes;
        }        
    }
}