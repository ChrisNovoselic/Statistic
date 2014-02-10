using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Threading;

namespace StatisticCommon
{
    public class LogParse
    {
        public enum TYPE_LOGMESSAGE { START, STOP, DBOPEN, DBCLOSE, DBEXCEPTION, ERROR, DEBUG, UNKNOWN, COUNT_TYPE_LOGMESSAGE };
        public static string[] DESC_LOGMESSAGE = { "Запуск", "Выход",
                                            "БД открыть", "БД закрыть", "БД исключение",
                                            "Ошибка", "Отладка", "Неопределенный тип" };

        private string[] SIGNATURE_LOGMESSAGE = { ProgramBase.MessageWellcome, ProgramBase.MessageExit,
                                                    DbTSQLInterface.MessageDbOpen, DbTSQLInterface.MessageDbClose, DbTSQLInterface.MessageDbException,
                                                    "!Ошибка!", "!Отладка!", "Неопределенный тип" };

        private Thread m_thread;
        private DataTable m_tableLog;
        Semaphore m_semAllowed;
        bool m_bAllowed;

        public DelegateFunc Exit;

        public LogParse ()
        {
            m_semAllowed = new Semaphore (1, 1);

            m_tableLog = new DataTable("ContentLog");
            DataColumn[] cols = new DataColumn[] { new DataColumn("DATE_TIME", typeof(DateTime)),
                                                    new DataColumn("TYPE", typeof(Int32)),
                                                    new DataColumn ("MESSAGE", typeof (string)) };
            m_tableLog.Columns.AddRange(cols);

            m_thread = null;
        }

        public void Start(string text)
        {
            m_semAllowed.WaitOne ();

            m_thread = new Thread(new ParameterizedThreadStart(Thread_Proc));
            m_thread.IsBackground = true;
            m_thread.Name = "Разбор лог-файла";

            m_thread.Start (text);
        }

        public void Stop ()
        {
            bool joined = false;
            if ((!(m_thread == null)))
            {
                if (m_thread.IsAlive == true)
                {
                    m_bAllowed = false;
                    joined = m_thread.Join (6666);
                    if (joined == false)
                        m_thread.Abort ();
                    else
                        ;
                }
                else
                    ;

                m_semAllowed.Release();
            }
            else
                ;

            m_bAllowed = true;
        }

        private void Thread_Proc (object data)
        {
            string line,
                   msg;
            int typeMsg = -1;
            DateTime dtMsg;
            bool bValid = false;

            line = msg = string.Empty;

            StringReader content = new StringReader(data as string);

            m_tableLog.Clear();

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
                    for (i = 0; i < TYPE_LOGMESSAGE.COUNT_TYPE_LOGMESSAGE - 1; i++) //'- 1' чтобы не учитывать UNKNOWN
                    {
                        if (msg.Contains(SIGNATURE_LOGMESSAGE[(int)i]) == true)
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
            while (m_bAllowed == true);

            Console.WriteLine("Окончание обработки лог-файла. Обработано строк: {0}", m_tableLog.Rows.Count);

            Exit ();
        }

        public int Select (int type, string beg, string end, ref DataRow []res)
        {
            int iRes = -1;
            string where = string.Empty;

            if (!(beg.Equals (string.Empty) == true))
            {
                where = "DATE_TIME>='" + DateTime.Parse (beg).ToString ("yyyy-MM-dd HH:mm:ss") + "'";
                if (!(end.Equals (string.Empty) == true))
                    where += " AND DATE_TIME<'" + DateTime.Parse (end).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                else
                    ;
            }
            else
                ;

            if (!(type < 0))
            {
                if (!(where.Equals (string.Empty) == true))
                    where += " AND ";
                else
                    ;

                where += "TYPE=" + type;
            }
            else
                ;

            res = m_tableLog.Select(where, "DATE_TIME");

            return iRes;
        }
    }
}
