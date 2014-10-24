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
        public enum TYPE_LOGMESSAGE { START, STOP,
                                    DBOPEN, DBCLOSE, DBEXCEPTION,
                                    ERROR, DEBUG,
                                    DETAIL,
                                    SEPARATOR, SEPARATOR_DATETIME,
                                    UNKNOWN, COUNT_TYPE_LOGMESSAGE };
        public static string[] DESC_LOGMESSAGE = { "������", "�����",
                                            "�� �������", "�� �������", "�� ����������",
                                            "������", "�������",
                                            "�����������",
                                            "������./�����.", "������./����/�����",
                                            "�������������� ���" };

        private string[] SIGNATURE_LOGMESSAGE = { ProgramBase.MessageWellcome, ProgramBase.MessageExit,
                                                    DbTSQLInterface.MessageDbOpen, DbTSQLInterface.MessageDbClose, DbTSQLInterface.MessageDbException,
                                                    "!������!", "!�������!",
                                                    string.Empty,
                                                    Logging.MessageSeparator, Logging.DatetimeStampSeparator,
                                                    string.Empty, "�������������� ���" };

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
            m_thread.Name = "������ ���-�����";

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

                try { m_semAllowed.Release(); }
                catch (Exception e)
                {
                    Console.WriteLine("LogParse::Stop () - m_semAllowed.Release() - ����� �� ��� �������");
                }
            }
            else
                ;

            m_bAllowed = true;
        }

        private void Thread_Proc (object data)
        {
            string line;
            TYPE_LOGMESSAGE typeMsg;
            DateTime dtMsg;
            bool bValid = false;

            line = string.Empty;

            typeMsg = TYPE_LOGMESSAGE.UNKNOWN;

            StringReader content = new StringReader(data as string);

            m_tableLog.Clear();

            Console.WriteLine("������ ��������� ���-�����. ������: {0}", (data as string).Length);

            //������ 1-�� ������ ���-�����
            line = content.ReadLine();

            while (!(line == null))
            {
                //�������� �� ����������� ���������-���������
                if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.SEPARATOR]) == true)
                {
                    //��������� ������ - ����/�����
                    line = content.ReadLine();

                    //������� ������� ����/�����
                    bValid = DateTime.TryParse(line, out dtMsg);
                    if (bValid == true)
                    {
                        //��������� ������ - ����������� ����/�����-���������
                        content.ReadLine();
                        //��������� ������ - ���������
                        line = content.ReadLine();
                    }
                    else
                    {
                        //��������� ����/����� ����������� ���������
                        if (m_tableLog.Rows.Count > 0)
                            dtMsg = (DateTime)m_tableLog.Rows[m_tableLog.Rows.Count - 1]["DATE_TIME"];
                        else
                            ; //dtMsg = new DateTime(1666, 06, 06, 06, 06, 06);
                    }
                }
                else
                {
                    //������� ������� ����/�����
                    bValid = DateTime.TryParse(line, out dtMsg);
                    if (bValid == true)
                    {
                        //��������� ������ - ����������� ����/�����-���������
                        content.ReadLine();
                        //��������� ������ - ���������
                        line = content.ReadLine();
                    }
                    else
                    {
                        //��������� ����/����� ����������� ���������
                        //dtMsg = new DateTime(1666, 06, 06, 06, 06, 06);
                        dtMsg = (DateTime)m_tableLog.Rows[m_tableLog.Rows.Count - 1]["DATE_TIME"];

                        if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.SEPARATOR_DATETIME]) == true)
                        {
                            //��������� ������ - ���������
                            line = content.ReadLine();
                        }
                        else
                        {
                            //������ �������� ���������/����������� ���������
                        }
                    }
                }

                if (line == null)
                {
                    break;
                }
                else
                    ;

                //�������� �� ����������� ���������-���������
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

                    //���������� ������ � ������� � ����������
                    m_tableLog.Rows.Add(new object[] { dtMsg, (int)typeMsg, line });

                    //������ ������ ���������
                    line = content.ReadLine();

                    while ((!(line == null)) && (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.SEPARATOR]) == false))
                    {
                        //���������� ������ � ������� � ����������
                        m_tableLog.Rows.Add(new object[] { dtMsg, (int)TYPE_LOGMESSAGE.DETAIL, line });
                        
                        //������ ������ ���������
                        line = content.ReadLine();
                    }
                }
            }

            Console.WriteLine("��������� ��������� ���-�����. ���������� �����: {0}", m_tableLog.Rows.Count);

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
