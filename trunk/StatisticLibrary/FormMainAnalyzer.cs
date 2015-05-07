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
        abstract partial class PanelAnalyzer : PanelStatistic
        {
            public event EventHandler EvtClose;

            protected System.Threading.Timer m_timerChecked;
            protected bool m_bThreadTimerCheckedAllowed;
            protected LogParse m_LogParse;

            protected DataTable m_tableUsers
                        , m_tableRoles;

            //protected DbConnection m_connConfigDB;
            protected int m_iListenerIdConfigDB;

            CheckBox[] m_arCheckBoxMode;

            int m_prevDatetimeRowIndex;

            protected const string c_list_sorted = @"DESCRIPTION";
            protected const string c_NameFieldToConnect = "COMPUTER_NAME";

            List<TEC> m_listTEC;

            protected Dictionary<int, int[]> m_dicTabVisibleIdItems;

            protected enum INDEX_DELIMETER { PART, ROW };
            protected static string[] m_chDelimeters = { @"DELIMETER_PART", "DELIMETER_ROW" };

            public PanelAnalyzer(int idListener, List<TEC> tec)
                : base()
            {
                InitializeComponent();

                m_arCheckBoxMode = new CheckBox[] { checkBoxTEC, checkBoxGTP, checkBoxPC, checkBoxTG };

                m_iListenerIdConfigDB = idListener;
                m_listTEC = tec;

                m_dicTabVisibleIdItems = new Dictionary<int, int[]>();
                FillDataGridViewTabVisible();

                int i = -1;

                dgvFilterActives.Rows.Add(2);
                dgvFilterActives.Rows[0].Cells[0].Value = true; dgvFilterActives.Rows[0].Cells[1].Value = "Активные";
                dgvFilterActives.Rows[1].Cells[0].Value = true; dgvFilterActives.Rows[1].Cells[1].Value = "Не активные";
                //dgvFilterActives.ReadOnly = false;
                dgvFilterActives.Columns[0].ReadOnly = false;
                dgvFilterActives.CellClick += new DataGridViewCellEventHandler(dgvFilterActives_CellClick);
                dgvFilterActives.Enabled = true;

                dgvFilterTypeMessage.CellClick += new DataGridViewCellEventHandler(dgvFilterTypeMessage_CellClick);

                int err = -1;

                DbConnection connConfigDB = DbSources.Sources().GetConnection(m_iListenerIdConfigDB, out err);
                //DbConnection connDB = DbTSQLInterface.GetConnection(m_connSettConfigDB, out err);

                if ((!(connConfigDB == null)) && (err == 0))
                {
                    HStatisticUsers.GetRoles(ref connConfigDB, string.Empty, string.Empty, out m_tableRoles, out err);
                    FillDataGridViews(ref dgvFilterRoles, m_tableRoles, @"DESCRIPTION", err, true);

                    HStatisticUsers.GetUsers(ref connConfigDB, string.Empty, c_list_sorted, out m_tableUsers, out err);
                    FillDataGridViews(ref dgvClient, m_tableUsers, @"DESCRIPTION", err);

                    m_LogParse = newLogParse ();

                    fillTypeMessage();

                    if (!(m_LogParse == null))
                    {
                        m_LogParse.Exit = LogParseExit;

                        Start();
                    }
                    else
                    {
                        string strErr = @"Не создан объект разбора сообщений (класс 'LogParse')...";
                        Logging.Logg ().Error (strErr, Logging.INDEX_MESSAGE.NOT_SET);
                        throw new Exception (strErr);
                    }
                }
                else
                    ;
            }

            protected abstract LogParse newLogParse ();

            protected abstract void procChecked(out bool[] arbActives, out int err);
            protected abstract void ProcChecked(object obj);

            /// <summary>
            /// Отключение от клиента (активного пользователя)
            /// </summary>
            protected abstract void Disconnect();

            protected abstract void fillTypeMessage();

            protected void fillTypeMessage(string[] strTypeMessages)
            {
                dgvFilterTypeMessage.Rows.Add(strTypeMessages.Length);
                for (int i = 0; i < strTypeMessages.Length; i++)
                {
                    dgvFilterTypeMessage.Rows[(int)i].Cells[0].Value = true;
                    dgvFilterTypeMessage.Rows[(int)i].Cells[1].Value = strTypeMessages[i];
                    dgvFilterTypeMessage.Rows[(int)i].Cells[2].Value = 0;
                }
            }

            public override void Start()
            {
                m_bThreadTimerCheckedAllowed = true;

                m_timerChecked = new System.Threading.Timer(new TimerCallback(ProcChecked), null, 0, System.Threading.Timeout.Infinite);
            }

            public override void Stop()
            {
                m_bThreadTimerCheckedAllowed = false;

                if (!(m_timerChecked == null))
                {
                    m_timerChecked.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    m_timerChecked.Dispose();
                    m_timerChecked = null;
                }
                else
                    ;
            }

            public override void Activate(bool active)
            {
                throw new NotImplementedException();
            }

            private void buttonClose_Click(object sender, EventArgs e)
            {
                Disconnect();

                if (!(m_LogParse == null))
                    m_LogParse.Stop();
                else
                    ;

                Stop();

                EvtClose(this, e);
            }

            protected void SetModeVisibleTabs(int mode)
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
                int indx = Convert.ToInt32(ValueToCreate.Split(';')[1]);

                Logging.Logg().Error(string.Format("FormAnalyzer::ErrorConnect () - {0}, индекс: {1}", ValueToCreate.Split(';')[0], ValueToCreate.Split(';')[1]), Logging.INDEX_MESSAGE.NOT_SET);

                Console.WriteLine("FormAnalyzer::ErrorConnect () - {0}, индекс: {1}", ValueToCreate.Split(';')[0], ValueToCreate.Split(';')[1]);
                if (indx < dgvClient.Rows.Count) //m_tableUsers, m_listTcpClient
                    dgvClient.Rows[indx].Cells[0].Value = false;
                else
                    ; //Отработка ошибки соединения для пользователя УЖЕ отсутствующего в списке
            }

            private void FillDataGridViews(ref DataGridView ctrl, DataTable src, string nameField, int run, bool checkDefault = false)
            {
                if (run == 0)
                {
                    bool bCheckedItem = checkDefault;
                    ctrl.Rows.Clear();

                    if (src.Rows.Count > 0)
                    {
                        ctrl.Rows.Add(src.Rows.Count);

                        for (int i = 0; i < src.Rows.Count; i++)
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

            protected void TabLoggingClearText(bool bClearCounter)
            {
                m_LogParse.Clear();
                /*textBoxLog.Clear();*/ dgvLogMessage.Rows.Clear();
                if (bClearCounter == true)
                    for (int i = 0; i < dgvFilterTypeMessage.Rows.Count; i++)
                        dgvFilterTypeMessage.Rows[(int)i].Cells[2].Value = 0;
                else
                    ;
            }

            void TabLoggingPositionText()
            {
                /*
                textBoxLog.Select(0, 0);
                textBoxLog.ScrollToCaret();
                */

                if (dgvLogMessage.Rows.Count > 0)
                    dgvLogMessage.FirstDisplayedScrollingRowIndex = 0;
                else
                    ;
            }

            class HTabLoggingAppendTextPars
            {
                public bool bClearCounter;
                public string rows;

                public HTabLoggingAppendTextPars (bool bcc, string rs)
                {
                    bClearCounter = bcc;
                    rows = rs;
                }
            }
            
            void TabLoggingAppendText(object data)
            {
                HTabLoggingAppendTextPars tlatPars = data as HTabLoggingAppendTextPars;
                if (tlatPars.rows.Equals(string.Empty) == true)
                {
                    TabLoggingPositionText();
                    //textBoxLog.Focus();
                    //textBoxLog.AutoScrollOffset = new Point (0, 0);
                }
                else
                {
                    int[] arTypeLogMsgCounter = new int[dgvFilterTypeMessage.Rows.Count];
                    List<int> listIdTypeMessages = LogParse.s_IdTypeMessages.ToList();
                    
                    bool [] arCheckedTypeMessages = new bool [dgvFilterTypeMessage.Rows.Count];
                    foreach (DataGridViewRow r in dgvFilterTypeMessage.Rows)
                        arCheckedTypeMessages [dgvFilterTypeMessage.Rows.IndexOf (r)] = bool.Parse (r.Cells[0].Value.ToString ());

                    string[] messages = tlatPars.rows.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.ROW] }, StringSplitOptions.None);
                    string[] parts;
                    int indxTypeMessage = -1;

                    foreach (string text in messages)
                    {
                        parts = text.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None);
                        indxTypeMessage = listIdTypeMessages.IndexOf(Int32.Parse(parts[1]));

                        if (arCheckedTypeMessages[indxTypeMessage] == true)
                            dgvLogMessage.Rows.Add(parts);
                        else
                            ;

                        if (tlatPars.bClearCounter == true)
                        {                            
                            if ((indxTypeMessage < arTypeLogMsgCounter.Length) && (!(indxTypeMessage < 0)))
                            {
                                arTypeLogMsgCounter[indxTypeMessage]++;
                                //dgvFilterTypeMessage.Rows[i].Cells[2].Value = arTypeLogMsgCounter[i];
                            }
                            else
                                Logging.Logg().Error(@"FormMainAnalyzer::TabLoggingAppendText () - неизвестный тип сообщения = " + parts[1], Logging.INDEX_MESSAGE.NOT_SET);
                        }
                        else
                            ;
                    }

                    if (tlatPars.bClearCounter == true)
                        for (indxTypeMessage = 0; indxTypeMessage < dgvFilterTypeMessage.Rows.Count; indxTypeMessage++)
                            dgvFilterTypeMessage.Rows[indxTypeMessage].Cells[2].Value = Int32.Parse(dgvFilterTypeMessage.Rows[indxTypeMessage].Cells[2].Value.ToString()) + arTypeLogMsgCounter[indxTypeMessage];
                    else
                        ;
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
                    dgvDatetimeStart.Rows[dgvDatetimeStart.Rows.Count - 1].Selected = bRowChecked;
                    dgvDatetimeStart.FirstDisplayedScrollingRowIndex = dgvDatetimeStart.Rows.Count - 1;
                }
                else
                    ;
            }

            protected void TabVisibliesClearChecked()
            {
                foreach (KeyValuePair<int, int[]> pair in m_dicTabVisibleIdItems)
                    dgvTabVisible.Rows[m_dicTabVisibleIdItems[pair.Key][0]].Cells[m_dicTabVisibleIdItems[pair.Key][1]].Value = false;
            }

            protected abstract void StartLogParse(string par);

            private void dgvFilterActives_CellClick(object sender, DataGridViewCellEventArgs e)
            {
                int i = -1, err = -1;

                if (e.ColumnIndex == 0)
                {
                    m_timerChecked.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                    dgvFilterActives.Rows[e.RowIndex].Cells[0].Value = !bool.Parse(dgvFilterActives.Rows[e.RowIndex].Cells[0].Value.ToString());

                    DbConnection connConfigDB = DbSources.Sources ().GetConnection (m_iListenerIdConfigDB, out err);
                    if ((!(connConfigDB == null)) && (err == 0))
                    {
                        HStatisticUsers.GetUsers(ref connConfigDB, string.Empty, c_list_sorted, out m_tableUsers, out err);

                        if (dgvFilterActives.Rows[0].Cells[0].Value == dgvFilterActives.Rows[1].Cells[0].Value)
                            if (bool.Parse(dgvFilterActives.Rows[0].Cells[0].Value.ToString()) == true)
                                //Отображать всех...
                                ;
                            else
                                //Пустой список...
                                m_tableUsers.Clear();
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
                        throw new Exception(@"PanalAnalyzer::dgvFilterActives_CellClick () - нет соединения с БД конфигурации");
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
                    DbConnection connConfigDB = DbSources.Sources ().GetConnection (m_iListenerIdConfigDB, out err);
                    if ((!(connConfigDB == null)) && (err == 0))
                    {
                        //Thread_ProcCheckedStop();
                        m_timerChecked.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                        dgvFilterRoles.Rows[e.RowIndex].Cells[0].Value = !bool.Parse(dgvFilterRoles.Rows[e.RowIndex].Cells[0].Value.ToString());

                        for (i = 0; i < m_tableRoles.Rows.Count; i++)
                        {
                            if (bool.Parse(dgvFilterRoles.Rows[i].Cells[0].Value.ToString()) == false)
                            {
                                if (where.Equals(string.Empty) == true)
                                {
                                    where = "ID_ROLE NOT IN (";
                                }
                                else
                                    where += ",";

                                where += m_tableRoles.Rows[i]["ID"];
                            }
                            else
                                ;
                        }

                        if (where.Equals(string.Empty) == false)
                            where += ")";
                        else
                            ;

                        HStatisticUsers.GetUsers(ref connConfigDB, where, c_list_sorted, out m_tableUsers, out err);
                        FillDataGridViews(ref dgvClient, m_tableUsers, @"DESCRIPTION", err);

                        m_timerChecked.Change(0, System.Threading.Timeout.Infinite);
                    }
                    else
                        throw new Exception(@"PanelAnzlyzer::dgvFilterRoles_CellClick () - нет соединения с БД конфигурации...");
                }
                else
                    ;
            }

            private void dgvFilterTypeMessage_CellClick(object sender, DataGridViewCellEventArgs e)
            {
                if (e.ColumnIndex == 0)
                {
                    //DateTime dtBeg = DateTime.Parse(dgvDatetimeStart.Rows[dgvDatetimeStart.SelectedRows[0].Index].Cells[1].Value.ToString())
                    //    , dtEnd = DateTime.MaxValue;

                    //if (dgvDatetimeStart.SelectedRows[0].Index < (dgvDatetimeStart.Rows.Count - 1))
                    //{
                    //    dtEnd = dtBeg.AddDays(1);
                    //}
                    //else
                    //    ;

                    dgvFilterTypeMessage.Rows[e.RowIndex].Cells[0].Value = !bool.Parse(dgvFilterTypeMessage.Rows[e.RowIndex].Cells[0].Value.ToString());

                    //startFilldgvLogMessages (false);
                    startUpdatedgvLogMessages(false);
                }
                else
                    ;
            }

            public void LogParseExit()
            {
                int i = -1;
                DataRow[] rows = new DataRow[] { };

                //DelegateObjectFunc delegateAppendText = TabLoggingAppendText;
                //DelegateStringFunc delegateAppendDatetimeStart = TabLoggingAppendDatetimeStart;
                BeginInvoke(new DelegateFunc(TabLoggingClearDatetimeStart));
                BeginInvoke(new DelegateBoolFunc(TabLoggingClearText), true);

                //Получение списка дата/время запуска приложения
                bool rowChecked = false;
                string strDatetimeStart = string.Empty;
                //Нюанс для 'LogParse_File' и 'LogParse_DB' - "0" индекс в массиве типов сообщений (зрезервирован для "СТАРТ")
                //...для 'LogParse_File': m_tblLog содержит ВСЕ записи в файле
                //...для 'LogParse_DB': m_tblLog содержит ТОЛЛЬКО записи из БД с датами, за которые найдено хотя бы одно сообщение
                //      тип сообщения "СТАРТ" устанавливается "программно" (метод 'LogParse_DB::Thread_Proc')
                i = m_LogParse.Select(@"true" + m_chDelimeters[(int)INDEX_DELIMETER.ROW] + ((int)LogParse.INDEX_START_MESSAGE).ToString (), DateTime.MaxValue, DateTime.MaxValue, ref rows); //0 - индекс в массиве идентификаторов зарезервирован для сообщений типа "СТАРТ"
                for (i = 0; i < rows.Length; i++)
                {
                    if (i == (rows.Length - 1))
                        rowChecked = true;
                    else
                        ;

                    strDatetimeStart += rowChecked.ToString()
                            + m_chDelimeters[(int)INDEX_DELIMETER.PART]
                            + rows[i]["DATE_TIME"].ToString ()
                            + m_chDelimeters[(int)INDEX_DELIMETER.ROW];
                }

                if (strDatetimeStart.Length > 0)
                {
                    strDatetimeStart = strDatetimeStart.Substring(0, strDatetimeStart.Length - m_chDelimeters[(int)INDEX_DELIMETER.ROW].Length);
                    BeginInvoke(new DelegateStringFunc(TabLoggingAppendDatetimeStart), strDatetimeStart);
                }
                else
                    ;
            }

            private void buttonUpdate_Click(object sender, EventArgs e)
            {
                dgvClient_SelectionChanged(null, null);
            }

            //protected abstract int SelectLogMessage(int type, DateTime beg, DateTime end, ref DataRow[] rows);
            protected abstract int SelectLogMessage(string type, DateTime beg, DateTime end, ref DataRow[] rows);

            private void updatedgvLogMessages(object data)
            {
                DataRow[] rows = new DataRow[] { };
                int cntRow = m_LogParse.Select(ref rows);
                filldgvLogMessages(false, rows);
            }

            private void filldgvLogMessages(object data)
            {
                //DateTime [] arDT = (DateTime [])data;
                object[] pars = (object[])data;
                DataRow[] rows = new DataRow[] { };
                int cntRow = SelectLogMessage(((string)pars[0]), ((DateTime)pars[1]), ((DateTime)pars[1]).AddDays(1), ref rows);

                filldgvLogMessages ((bool)pars[3], rows);
            }

            private void filldgvLogMessages (bool bClearCounter, DataRow [] rows)
            {
                DelegateObjectFunc delegateAppendText = new DelegateObjectFunc(TabLoggingAppendText);
                string strRowsTodgvLogMessages = string.Empty;

                if (rows.Length > 0)
                {
                    for (int i = 0; i < rows.Length; i++)
                    {
                        if (((i % 1000) == 0) && (strRowsTodgvLogMessages.Length > m_chDelimeters[(int)INDEX_DELIMETER.ROW].Length))
                        {
                            strRowsTodgvLogMessages = strRowsTodgvLogMessages.Substring(0, strRowsTodgvLogMessages.Length - m_chDelimeters[(int)INDEX_DELIMETER.ROW].Length);
                            this.BeginInvoke(delegateAppendText, new HTabLoggingAppendTextPars(bClearCounter, strRowsTodgvLogMessages));
                            //this.BeginInvoke(new DelegateObjectFunc(TabLoggingAppendText), strRowsTodgvLogMessages);
                            strRowsTodgvLogMessages = string.Empty;
                        }
                        else
                            strRowsTodgvLogMessages += getTabLoggingTextRow(rows[i]) + m_chDelimeters[(int)INDEX_DELIMETER.ROW];
                    }

                    if (strRowsTodgvLogMessages.Length > m_chDelimeters[(int)INDEX_DELIMETER.ROW].Length)
                    {
                        //Остаток...                    
                        strRowsTodgvLogMessages = strRowsTodgvLogMessages.Substring(0, strRowsTodgvLogMessages.Length - m_chDelimeters[(int)INDEX_DELIMETER.ROW].Length);
                        this.BeginInvoke(delegateAppendText, new HTabLoggingAppendTextPars(bClearCounter, strRowsTodgvLogMessages));
                        //this.BeginInvoke(new DelegateObjectFunc(TabLoggingAppendText), strRowsTodgvLogMessages);
                        strRowsTodgvLogMessages = string.Empty;
                    }
                    else
                        ;
                }
                else
                    ;

                //Крайняя строка...
                this.BeginInvoke(delegateAppendText, new HTabLoggingAppendTextPars(bClearCounter, strRowsTodgvLogMessages));
                //this.BeginInvoke(new DelegateObjectFunc(TabLoggingAppendText), string.Empty);
            }

            private string getIndexTypeMessages ()
            {
                string []strRes = new string [] { string.Empty, string.Empty };
                int indxRes = -1
                    , cntTrue = 0;

                foreach (DataGridViewRow row in dgvFilterTypeMessage.Rows)
                {
                    if (((DataGridViewCheckBoxCell)row.Cells[0]).Value.Equals (true) == true)
                    {
                        strRes [0] += dgvFilterTypeMessage.Rows.IndexOf(row) + m_chDelimeters[(int)INDEX_DELIMETER.PART];
                        cntTrue++;
                    }
                    else
                        if (((DataGridViewCheckBoxCell)row.Cells[0]).Value.Equals (false) == true)
                        {
                            strRes [1] += dgvFilterTypeMessage.Rows.IndexOf(row) + m_chDelimeters[(int)INDEX_DELIMETER.PART];
                        }
                        else
                            ;
                }

                if (strRes[0].Length > 0) {
                    indxRes = 0;

                    if (cntTrue == dgvFilterTypeMessage.Rows.Count)
                        //Все типы "отмечены"
                        strRes[0] = string.Empty;
                    else {
                        strRes[0] = true.ToString() + m_chDelimeters[(int)INDEX_DELIMETER.ROW] + strRes [0];
                        strRes[0] = strRes[0].Substring(0, strRes[0].Length - m_chDelimeters[(int)INDEX_DELIMETER.PART].Length);
                    }
                }
                else {
                    indxRes = 1;
                    
                    strRes[1] = false.ToString() + m_chDelimeters[(int)INDEX_DELIMETER.ROW] + strRes[1];
                    strRes[1] = strRes[1].Substring(0, strRes[1].Length - m_chDelimeters[(int)INDEX_DELIMETER.PART].Length);
                }

                return strRes[indxRes];
            }

            private void startFilldgvLogMessages(bool bClearTypeMessageCounter)
            {
                TabLoggingClearText(bClearTypeMessageCounter);

                DateTime dtBegin = DateTime.Parse(dgvDatetimeStart.Rows[m_prevDatetimeRowIndex].Cells[1].Value.ToString())
                    , dtEnd = DateTime.MaxValue;
                if ((m_prevDatetimeRowIndex + 1) < dgvDatetimeStart.Rows.Count)
                    dtEnd = DateTime.Parse(dgvDatetimeStart.Rows[m_prevDatetimeRowIndex + 1].Cells[1].Value.ToString());
                else
                    ;

                Thread threadFilldgvLogMessages = new Thread(new ParameterizedThreadStart(filldgvLogMessages));
                threadFilldgvLogMessages.IsBackground = true;
                threadFilldgvLogMessages.Start(new object[] { getIndexTypeMessages(), dtBegin, dtEnd, bClearTypeMessageCounter });
            }

            private void startUpdatedgvLogMessages(bool bClearTypeMessageCounter)
            {
                TabLoggingClearText(bClearTypeMessageCounter);

                Thread threadFilldgvLogMessages = new Thread(new ParameterizedThreadStart(updatedgvLogMessages));
                threadFilldgvLogMessages.IsBackground = true;
                threadFilldgvLogMessages.Start();
            }

            protected void dgvDatetimeStart_SelectionChanged(object sender, EventArgs e)
            {
                int i = -1;
                int rowIndex = dgvDatetimeStart.SelectedRows[0].Index;

                dgvDatetimeStart.Rows[m_prevDatetimeRowIndex].Cells[0].Value = false;
                dgvDatetimeStart.Rows[rowIndex].Cells[0].Value = true;
                m_prevDatetimeRowIndex = rowIndex;

                startFilldgvLogMessages(true);
            }

            protected abstract string getTabLoggingTextRow(DataRow r);

            private void tabControlAnalyzer_SelectedIndexChanged(object sender, EventArgs e)
            {
                dgvClient_SelectionChanged(null, null);
            }

            private void tabControlAnalyzer_Selected(object sender, TabControlEventArgs e)
            {

            }

            private void FillDataGridViewTabVisible()
            {
                int tec_indx = 0,
                    comp_indx = 0,
                    across_indx = -1
                    , col = -1;

                if (!(m_listTEC == null))
                    foreach (TEC t in m_listTEC)
                    {
                        tec_indx++;
                        comp_indx = 0;

                        col = (tec_indx - 1) * 2 + 1;

                        //Добавть 2 столбца (CheckBox, TextBox)
                        dgvTabVisible.Columns.AddRange(new DataGridViewColumn[] { new DataGridViewCheckBoxColumn(false), new DataGridViewTextBoxColumn() });

                        this.dgvTabVisible.Columns[col - 1].Frozen = false;
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
                            dgvTabVisible.Rows.Add();
                        else
                            ;

                        dgvTabVisible.Rows[comp_indx].Cells[col].Value = t.name_shr;
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

            protected abstract class LogParse
            {
                public const int INDEX_START_MESSAGE = 0;
                public static int[] s_IdTypeMessages;

                private Thread m_thread;
                protected DataTable m_tableLog;
                Semaphore m_semAllowed;
                //bool m_bAllowed;

                public DelegateFunc Exit;

                public LogParse()
                {
                    m_semAllowed = new Semaphore(1, 1);

                    m_tableLog = new DataTable("ContentLog");
                    DataColumn[] cols = new DataColumn[] { new DataColumn("DATE_TIME", typeof(DateTime)),
                                                    new DataColumn("TYPE", typeof(Int32)),
                                                    new DataColumn ("MESSAGE", typeof (string)) };
                    m_tableLog.Columns.AddRange(cols);

                    m_thread = null;
                }

                public void Start(object text)
                {
                    m_semAllowed.WaitOne();

                    m_thread = new Thread(new ParameterizedThreadStart(Thread_Proc));
                    m_thread.IsBackground = true;
                    m_thread.Name = "Разбор лог-файла";

                    m_thread.CurrentCulture =
                    m_thread.CurrentUICulture =
                        ProgramBase.ss_MainCultureInfo;

                    m_tableLog.Clear();

                    m_thread.Start(text);
                }

                public void Stop()
                {
                    //if (m_bAllowed == false)
                    //    return;
                    //else
                    //    ;

                    bool joined = false;
                    if ((!(m_thread == null)))
                    {
                        if (m_thread.IsAlive == true)
                        {
                            //m_bAllowed = false;
                            joined = m_thread.Join(6666);
                            if (joined == false)
                                m_thread.Abort();
                            else
                                ;
                        }
                        else
                            ;

                        try { m_semAllowed.Release(); }
                        catch (Exception e)
                        {
                            Console.WriteLine("LogParse::Stop () - m_semAllowed.Release() - поток не был запущен или штатно завршился");
                        }
                    }
                    else
                        ;

                    //m_bAllowed = true;
                }

                protected virtual void Thread_Proc(object data)
                {
                    Console.WriteLine("Окончание обработки лог-файла. Обработано строк: {0}", (int)data);

                    Exit();
                }

                public void Clear()
                {
                    //m_tableLog.Clear();
                }

                protected string addingWhereTypeMessage(string nameFieldTypeMessage, string strIndxType)
                {
                    string strRes = string.Empty;

                    //if (!(indxType < 0))
                    if (strIndxType.Equals (string.Empty) == false)
                    {
                        List<int> listIndxType;
                        //listIndxType = strIndxType.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.ROW] }, StringSplitOptions.None)[1].Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None).ToList().ConvertAll<int>(new Converter<string, int>(delegate(string strIn) { return Int32.Parse(strIn); }));
                        string []pars = strIndxType.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.ROW] }, StringSplitOptions.None);

                        if (pars.Length == 2)
                        {
                            bool bUse = false;
                            //if ((pars[0].Equals(true.ToString()) == true) || (pars[0].Equals(false.ToString()) == true))
                            if (bool.TryParse(pars[0], out bUse) == true)
                            {
                                if (pars[1].Length > 0)
                                {
                                    listIndxType = pars[1].Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None).ToList().ConvertAll<int>(new Converter<string, int>(delegate(string strIn) { return Int32.Parse(strIn); }));                                    

                                    strRes += nameFieldTypeMessage + @" ";

                                    if (bUse == false)
                                        strRes += @"NOT ";
                                    else
                                        ;

                                    strRes += @"IN (";

                                    foreach (int indx in listIndxType)
                                        strRes += s_IdTypeMessages[indx] + @",";

                                    strRes = strRes.Substring (0, strRes.Length - 1);

                                    strRes += @")";
                                }
                                else
                                    ;
                            }
                            else
                                ;
                        }
                        else
                            ;
                    }
                    else
                        ;

                    return strRes;
                }

                public int Select(ref DataRow[] res)
                {
                    res = m_tableLog.Select(string.Empty, "DATE_TIME");

                    return res.Length > 0 ? res.Length : -1;
                }

                //public int Select(int indxType, DateTime beg, DateTime end, ref DataRow[] res)
                public int Select(string strIndxType, DateTime beg, DateTime end, ref DataRow[] res)
                {
                    int iRes = -1;
                    string where = string.Empty;

                    //m_tableLog.Clear();

                    if (beg.Equals(DateTime.MaxValue) == false)
                    {
                        where = "DATE_TIME>='" + beg.ToString("yyyyMMdd HH:mm:ss") + "'";
                        if (end.Equals(DateTime.MaxValue) == false)
                            where += " AND DATE_TIME<'" + end.ToString("yyyyMMdd HH:mm:ss") + "'";
                        else
                            ;
                    }
                    else
                        ;
                    
                    if (where.Equals(string.Empty) == false)
                        where += @" AND ";
                    else
                        ;
                    where += addingWhereTypeMessage (@"TYPE", strIndxType);

                    res = m_tableLog.Select(where, "DATE_TIME");

                    return res.Length > 0 ? res.Length : -1;
                }
            }
        }

        class PanelAnalyzer_TCPIP : PanelAnalyzer
        {
            TcpClientAsync m_tcpClient;
            List<TcpClientAsync> m_listTCPClientUsers;

            public PanelAnalyzer_TCPIP(int idListener, List<TEC> tec)
                : base(idListener, tec)
            {
            }

            protected override LogParse newLogParse() { return new LogParse_File(); }

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

            public override void Start()
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

                base.Start();
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

                m_timerChecked.Change(66666, System.Threading.Timeout.Infinite);
            }

            public override void Stop()
            {
                base.Stop();

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
                                    BeginInvoke(new DelegateBoolFunc(TabLoggingClearText), true);
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

            //protected override int SelectLogMessage(int type, DateTime beg, DateTime end, ref DataRow[] rows)
            protected override int SelectLogMessage(string type, DateTime beg, DateTime end, ref DataRow[] rows)
            {
                return m_LogParse.Select(string.Empty, beg, end, ref rows);
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

            class LogParse_File : LogParse
            {
                public enum TYPE_LOGMESSAGE
                {
                    START, STOP,
                    DBOPEN, DBCLOSE, DBEXCEPTION,
                    ERROR, DEBUG,
                    DETAIL,
                    SEPARATOR, SEPARATOR_DATETIME,
                    UNKNOWN, COUNT_TYPE_LOGMESSAGE
                };
                public static string[] DESC_LOGMESSAGE = { "Запуск", "Выход",
                                            "БД открыть", "БД закрыть", "БД исключение",
                                            "Ошибка", "Отладка",
                                            "Детализация",
                                            "Раздел./сообщ.", "Раздел./дата/время",
                                            "Неопределенный тип" };

                protected string[] SIGNATURE_LOGMESSAGE = { ProgramBase.MessageWellcome, ProgramBase.MessageExit,
                                                    DbTSQLInterface.MessageDbOpen, DbTSQLInterface.MessageDbClose, DbTSQLInterface.MessageDbException,
                                                    "!Ошибка!", "!Отладка!",
                                                    string.Empty,
                                                    Logging.MessageSeparator, Logging.DatetimeStampSeparator,
                                                    string.Empty, "Неопределенный тип" };

                public LogParse_File()
                {
                    s_IdTypeMessages = new int[] {
                        0, 1
                        , 2, 3, 4
                        , 5, 6
                        , 7
                        , 8, 9
                        , 10
                        , 11
                    };
                }

                protected override void Thread_Proc(object text)
                {
                    string line;
                    TYPE_LOGMESSAGE typeMsg;
                    DateTime dtMsg;
                    bool bValid = false;

                    line = string.Empty;

                    typeMsg = TYPE_LOGMESSAGE.UNKNOWN;

                    StringReader content = new StringReader(text as string);

                    Console.WriteLine("Начало обработки лог-файла. Размер: {0}", (text as string).Length);

                    //Чтение 1-ой строки лог-файла
                    line = content.ReadLine();

                    while (!(line == null))
                    {
                        //Проверка на разделитель сообщение-сообщение
                        if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.SEPARATOR]) == true)
                        {
                            //Следующая строка - дата/время
                            line = content.ReadLine();

                            //Попытка разбора дата/время
                            bValid = DateTime.TryParse(line, out dtMsg);
                            if (bValid == true)
                            {
                                //Следующая строка - разделитель дата/время-сообщение
                                content.ReadLine();
                                //Следующая строка - сообщение
                                line = content.ReadLine();
                            }
                            else
                            {
                                //Установка дата/время предыдущего сообщения
                                if (m_tableLog.Rows.Count > 0)
                                    dtMsg = (DateTime)m_tableLog.Rows[m_tableLog.Rows.Count - 1]["DATE_TIME"];
                                else
                                    ; //dtMsg = new DateTime(1666, 06, 06, 06, 06, 06);
                            }
                        }
                        else
                        {
                            //Попытка разбора дата/время
                            bValid = DateTime.TryParse(line, out dtMsg);
                            if (bValid == true)
                            {
                                //Следующая строка - разделитель дата/время-сообщение
                                content.ReadLine();
                                //Следующая строка - сообщение
                                line = content.ReadLine();
                            }
                            else
                            {
                                //Установка дата/время предыдущего сообщения
                                //dtMsg = new DateTime(1666, 06, 06, 06, 06, 06);
                                dtMsg = (DateTime)m_tableLog.Rows[m_tableLog.Rows.Count - 1]["DATE_TIME"];

                                if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.SEPARATOR_DATETIME]) == true)
                                {
                                    //Следующая строка - сообщение
                                    line = content.ReadLine();
                                }
                                else
                                {
                                    //Строка содержит сообщение/детализация сообщения
                                }
                            }
                        }

                        if (line == null)
                        {
                            break;
                        }
                        else
                            ;

                        //Проверка на разделитель сообщение-сообщение
                        if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.SEPARATOR]) == true)
                        {
                            continue;
                        }
                        else
                        {
                            if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.START]) == true)
                            {
                                typeMsg = TYPE_LOGMESSAGE.START;
                            }
                            else
                                if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.STOP]) == true)
                                {
                                    typeMsg = TYPE_LOGMESSAGE.STOP;
                                }
                                else
                                    if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.DBOPEN]) == true)
                                    {
                                        typeMsg = TYPE_LOGMESSAGE.DBOPEN;
                                    }
                                    else
                                        if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.DBCLOSE]) == true)
                                        {
                                            typeMsg = TYPE_LOGMESSAGE.DBCLOSE;
                                        }
                                        else
                                            if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.DBEXCEPTION]) == true)
                                            {
                                                typeMsg = TYPE_LOGMESSAGE.DBEXCEPTION;
                                            }
                                            else
                                                if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.ERROR]) == true)
                                                {
                                                    typeMsg = TYPE_LOGMESSAGE.ERROR;
                                                }
                                                else
                                                    if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.DEBUG]) == true)
                                                    {
                                                        typeMsg = TYPE_LOGMESSAGE.DEBUG;
                                                    }
                                                    else
                                                        typeMsg = TYPE_LOGMESSAGE.DETAIL;

                            //Добавление строки в таблицу с собщениями
                            m_tableLog.Rows.Add(new object[] { dtMsg, (int)typeMsg, line });

                            //Чтение строки сообщения
                            line = content.ReadLine();

                            while ((!(line == null)) && (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.SEPARATOR]) == false))
                            {
                                //Добавление строки в таблицу с собщениями
                                m_tableLog.Rows.Add(new object[] { dtMsg, (int)TYPE_LOGMESSAGE.DETAIL, line });

                                //Чтение строки сообщения
                                line = content.ReadLine();
                            }
                        }
                    }

                    base.Thread_Proc(m_tableLog.Rows.Count);
                }
            }
        }

        class PanelAnalyzer_DB : PanelAnalyzer
        {
            //class HLogMsgSource {
                //public
                    DelegateIntFunc delegateConnect;
                //public
                    DelegateFunc delegateErrorConnect;
            //}
            //HLogMsgSource m_logMsgSource;

            private int m_idListenerLoggingDB;

            public PanelAnalyzer_DB(int idListener, List<TEC> tec)
                : base(idListener, tec)
            {
            }

            protected override LogParse newLogParse() { return new LogParse_DB (); }

            public override void Start()
            {
                int err = -1
                    , idMainDB = -1;

                DbConnection connConfigDB = DbSources.Sources ().GetConnection (m_iListenerIdConfigDB, out err);
                if ((!(connConfigDB == null)) && (err == 0))
                {
                    idMainDB = Int32.Parse(DbTSQLInterface.Select(ref connConfigDB, @"SELECT [VALUE] FROM [setup] WHERE [KEY]='" + @"Main DataSource" + @"'", null, null, out err).Rows[0][@"VALUE"].ToString());
                    DataTable tblConnSettMainDB = ConnectionSettingsSource.GetConnectionSettings(TYPE_DATABASE_CFG.CFG_200, ref connConfigDB, idMainDB, -1, out err);
                    ConnectionSettings connSettMainDB = new ConnectionSettings(tblConnSettMainDB.Rows[0], 1);
                    m_idListenerLoggingDB = DbSources.Sources().Register(connSettMainDB, false, @"MAIN_DB", false);

                    base.Start ();
                }
                else
                    throw new Exception (@"PanelAnalyzer_DB::Start () - нет соединения с БД конфигурации...");
            }

            public override void Stop ()
            {
                DbSources.Sources().UnRegister(m_idListenerLoggingDB);

                base.Stop ();
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
                                    arbActives[i] = (HHandlerDb.ToMoscowTimeZone (DateTime.Now) - DateTime.Parse(rowsMaxDatetimeWR[0][@"MAX_DATETIME_WR"].ToString())).TotalSeconds < 66;
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
                                        BeginInvoke(new DelegateBoolFunc(TabLoggingClearText), true);
                                    }
                                    else {
                                        TabLoggingClearDatetimeStart ();
                                        TabLoggingClearText (true);
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

            //protected override int SelectLogMessage(int type, DateTime beg, DateTime end, ref DataRow[] rows)
            protected override int SelectLogMessage(string type, DateTime beg, DateTime end, ref DataRow[] rows)
            {
                return (m_LogParse as LogParse_DB).Select(m_idListenerLoggingDB, type, beg, end, ref rows);
            }

            protected override string getTabLoggingTextRow(DataRow r)
            {
                string strRes = string.Empty;

                strRes = string.Join(m_chDelimeters[(int)INDEX_DELIMETER.PART].ToString()
                                        , new string[] {
                                            DateTime.Parse (r["DATE_TIME"].ToString ()).ToString (@"HH:mm:ss.fff")
                                            , r["TYPE"].ToString ()
                                            , r["MESSAGE"].ToString()
                                        }
                );

                return strRes;
            }

            class LogParse_DB : LogParse
            {
                public enum TYPE_LOGMESSAGE { START = LogParse.INDEX_START_MESSAGE, STOP, ACTION, DEBUG, EXCEPTION, EXCEPTION_DB, ERROR, WARNING, UNKNOWN, COUNT_TYPE_LOGMESSAGE };
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

                //public int Select(int iListenerId, int type, DateTime beg, DateTime end, ref DataRow[] res)
                public int Select(int iListenerId, string strIndxType, DateTime beg, DateTime end, ref DataRow[] res)
                {
                    int iRes = -1;
                    string where = string.Empty;

                    m_tableLog.Clear();

                    DbConnection connLoggingDB = DbSources.Sources ().GetConnection (iListenerId, out iRes);

                    if (iRes == 0)
                    {
                        string query = @"SELECT DATETIME_WR as DATE_TIME, ID_LOGMSG as TYPE, MESSAGE FROM logging WHERE ID_USER=" + m_idUser;

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

                        if (where.Equals(string.Empty) == false)
                            query += @" AND " + where;
                        else
                            ;

                        //where = addingWhereTypeMessage(@"ID_LOGMSG", strIndxType);
                        //if (where.Equals(string.Empty) == false)
                        //    query += " AND " + where;
                        //else
                        //    ;

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

        public FormMainAnalyzer(int idListener, List <TEC> tec)
        {
            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
                ProgramBase.ss_MainCultureInfo;

            if (this is FormMainAnalyzer_TCPIP)
                m_panel = new PanelAnalyzer_TCPIP(idListener, tec);
            else
                if (this is FormMainAnalyzer_DB)
                    m_panel = new PanelAnalyzer_DB(idListener, tec);
                else
                    ;

            if (! (m_panel == null))
            {
                m_panel.EvtClose += new EventHandler(this.FormMainAnalyzer_OnEvtPanelClose);

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
            }
            else
                ; //???Исключение
        }

        private void FormMainAnalyzer_OnEvtPanelClose(object sender, EventArgs e)
        {
            Close ();
        }

        private void FormMainAnalyzer_FormClosed(object sender, FormClosingEventArgs e)
        {            
            m_panel.Stop ();
        }

        /*
        //При наследовании от 'FormMainBaseWithStatusStrip'
        protected override bool UpdateStatusString()
        {
            bool have_eror = true;

            return have_eror;
        }
        */
    }

    public class FormMainAnalyzer_TCPIP : FormMainAnalyzer
    {
        public FormMainAnalyzer_TCPIP(int idListener, List<TEC> tec)
            : base(idListener, tec)
        {
        }
    }

    public class FormMainAnalyzer_DB : FormMainAnalyzer
    {
        public FormMainAnalyzer_DB(int idListener, List<TEC> tec)
            : base(idListener, tec)
        {
        }
    }
}