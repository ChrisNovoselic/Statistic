﻿using ASUTP;
using ASUTP.Core;
using ASUTP.Database;
using ASUTP.Helper;
using ASUTP.Network;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace StatisticAnalyzer
{
    public class PanelAnalyzer_TCPIP : PanelAnalyzer
    {
        TcpClientAsync m_tcpClient;
        List<TcpClientAsync> m_listTCPClientUsers;

        public PanelAnalyzer_TCPIP(List<StatisticCommon.TEC> tec, Color foreColor, Color backColor)
            : base(tec, foreColor,  backColor)
        {
        }

        protected override void updateCounter(DATAGRIDVIEW_LOGCOUNTER tag, DateTime start_date, DateTime end_date, string users)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<int> getTabActived(int user)
        {
            throw new NotImplementedException();
        }

        protected override LogCounter newLogCounter() { return new LogParse_File(); }

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

                        if (indxTcpClient < m_listTCPClientUsers.Count) {
                            //TODO: в таблицу разместить строку с индексом??? пользователя и результатом (успешным: true) соединения
                            m_loggingReadHandler.PerformCommandCompleted (new REQUEST (StatesMachine.TODO, null), new DataTable ());
                            m_listTCPClientUsers [indxTcpClient].Write(@"DISCONNECT");

                            m_listTCPClientUsers[indxTcpClient].Disconnect();
                        } else
                            ;
                        break;
                    case "LOG_LOCK":
                        //rec.Split('=')[1].Split(';')[1] - полный путь лог-файла
                        startLogParse(rec.Split('=')[1].Split(';')[1]);

                        m_tcpClient.Write("LOG_UNLOCK=?");
                        break;
                    case "LOG_UNLOCK":
                        disconnect();
                        break;
                    case "TAB_VISIBLE":
                        string[] recParameters = rec.Split('=')[1].Split(';'); //список отображаемых вкладок пользователя
                        int i = -1,
                            mode = -1
                            //, key = -1
                            ;
                        int[] IdItems;
                        string[] indexes = null;

                        if (recParameters.Length > 1)
                        {
                            mode = Convert.ToInt32(recParameters[1]);

                            //BeginInvoke(new DelegateIntFunc(SetModeVisibleTabs)/*, mode*/);

                            if (recParameters.Length > 2)
                            {
                                indexes = recParameters[2].Split(',');
                                //arIndexes = recParameters[2].Split(',').ToArray <int> ();

                                IdItems = new int[indexes.Length];
                                for (i = 0; i < indexes.Length; i++)
                                    IdItems[i] = Convert.ToInt32(indexes[i]);
                            }
                            else
                                //Не отображается ни одна вкладка
                                ;
                        }
                        else
                        {// !Ошибка! Не переданы индексы отображаемых вкладок
                        }

                        disconnect();
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

        private void onListUserReceived(DataTable tableUsers)
        {
            int i = -1;

            base.Start();

            m_listTCPClientUsers = new List<TcpClientAsync>();

            tableUsers.Rows.Cast<DataRow>().ToList().ForEach(r => {
                //Проверка активности
                m_listTCPClientUsers.Add(new TcpClientAsync());
                i = m_listTCPClientUsers.Count - 1;

                m_listTCPClientUsers[i].delegateConnect = ConnectToChecked;
                m_listTCPClientUsers[i].delegateErrorConnect = errorConnect;
                m_listTCPClientUsers[i].delegateRead = Read;
                //m_listTCPClientUsers[i].Connect (m_tableUsers.Rows[i][NameFieldToConnect].ToString(), 6666);
            });
        }

        protected override bool [] procChecked ()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Запись значений активности в CheckBox на DataGridView с пользователями
        /// </summary>
        /// <param name="obj">???</param>
        protected void onCommand_ProcCheckState(object obj)
        {
            int i = -1
                , indx = -1;
            string name_pc = string.Empty;

            for (i = 0;
                (i < m_listTCPClientUsers.Count)
                    && (m_bThreadTimerCheckedAllowed == true);
                i++) {
                //name_pc = 
                //indx = 
                //Проверка активности
                m_listTCPClientUsers[i].Connect($"{name_pc};{indx}", 6666);
            }
        }

        public override void Stop()
        {
            base.Stop();

            if (!(m_listTCPClientUsers == null))
                m_listTCPClientUsers.Clear();
            else
                ;
        }

        protected override void disconnect()
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

        protected void onCommand_ListDateByUser(string full_path)
        {
            FileInfo fi = new FileInfo(full_path);
            FileStream fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("windows-1251"));
            
            m_loggingReadHandler.PerformCommandCompleted(new REQUEST(StatesMachine.ListDateByUser, null), fileToTable(sr.ReadToEnd()));

            sr.Close();
        }

        private DataTable fileToTable (string reader)
        {
            DataTable tableRes;
            DataRow newRow;
            string [] values;

            string [] columns = { "Index", "Content" };

            tableRes = new DataTable ();
            values = reader.Split (new [] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = 0; i < values.Count (); i++) {
                if (tableRes.Columns.Count == 0)
                    columns.ToList ().ForEach (column => tableRes.Columns.Add (column, typeof (string)));
                else
                    ;

                newRow = tableRes.NewRow ();
                newRow.SetField (0, i);
                newRow.SetField (1, values[i]);

                tableRes.Rows.Add (newRow);
            }

            tableRes.AcceptChanges ();

            return tableRes;
        }

        protected override string getTabLoggingTextRow(DataRow r)
        {
            string strRes = string.Empty;

            if (Convert.ToInt32(r["TYPE"]) == (int)LogParse_File.TYPE_LOGMESSAGE.DETAIL)
                strRes = string.Empty.PadLeft(4) + r["MESSAGE"].ToString() + Environment.NewLine;
            else
                strRes = "[" + r["DATE_TIME"] + "]: " + r["MESSAGE"].ToString() + Environment.NewLine;

            return strRes;
        }

        protected override ILoggingReadHandler newLoggingRead ()
        {
            throw new NotImplementedException ();
        }

        protected override void handlerCommandCounterToTypeMessageByDate(REQUEST req, DataTable tableRes)
        {
            throw new NotImplementedException ();
        }

        protected override void handlerCommandListMessageToUserByDate (REQUEST req, DataTable tableLogging)
        {
            throw new NotImplementedException ();
        }

        protected override void handlerCommandListDateByUser (REQUEST req, DataTable tableRes)
        {
            throw new NotImplementedException ();
        }

        protected override void handlerCommandProcChecked (REQUEST req, DataTable tableRes)
        {
            throw new NotImplementedException ();
        }

        /// <summary>
        /// Обработчик события выбора пользователя для формирования списка сообщений
        /// </summary>
        protected override void dgvUserView_SelectionChanged (object sender, EventArgs e)
        {
            bool bUpdate = false;
            string name_pc = string.Empty;

            if (!(m_tcpClient == null)) {
                disconnect ();
            } else
                ;

            base.dgvUserView_SelectionChanged (sender, e);

            bUpdate = !(IdCurrentUserView < 0);

            if (bUpdate == true) {
                m_tcpClient = new TcpClientAsync ();
                m_tcpClient.delegateRead = Read;

                //Очистить элементы управления с данными от пред. лог-файла
                if (IsHandleCreated/*InvokeRequired*/ == true) {
                    BeginInvoke (new DelegateFunc (clearListDateView));
                    BeginInvoke (new DelegateBoolFunc (clearMessageView), true);
                } else
                    Logging.Logg ().Error (@"PanelAnalyzer::dgvUserView_SelectionChanged () - ... BeginInvoke (TabLoggingClearDatetimeStart, TabLoggingClearText) - ...", Logging.INDEX_MESSAGE.D_001);

                //Если активна 0-я вкладка (лог-файл)
                m_tcpClient.delegateConnect = ConnectToLogRead;

                m_tcpClient.delegateErrorConnect = errorConnect;

                //name_pc = "localhost"; //??? c_NameFieldToConnect
                //m_tcpClient.Connect(name_pc, 6666);
                m_tcpClient.Connect ($"{name_pc};{IndexCurrentUserView}", 6666);
            } else
                ;
        }

        class LogParse_File : LogCounter
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
                s_ID_LOGMESSAGES = new int[] {
                        0, 1
                        , 2, 3, 4
                        , 5, 6
                        , 7
                        , 8, 9
                        , 10
                        , 11
                    };
            }

            protected override void thread_Proc(object text)
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

                base.thread_Proc(m_tableLog.Rows.Count);
            }
        }
    }
}
