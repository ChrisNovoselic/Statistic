using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace StatisticCommon
{
    //public class Logging
    //{

    //    //private Logging () {
    //    //}
    //}

    public class Logging //LoggingFS //: Logging
    {
        public enum LOG_MODE { ERROR = -1, UNKNOWN, FILE, DB };
        public enum ID_MESSAGE { START = 1, STOP, ACTION, DEBUG, EXCEPTION, EXCEPTION_DB, ERROR };

        private string m_fileNameStart;
        private string m_fileName;
        private bool externalLog;
        private DelegateStringFunc delegateUpdateLogText;
        private DelegateFunc delegateClearLogText;
        private Semaphore sema;
        private LogStreamWriter m_sw;
        private FileInfo m_fi;
        public static LOG_MODE s_mode = LOG_MODE.UNKNOWN;
        private bool logRotate = true;
        private const int MAX_ARCHIVE = 6;
        private static int logRotateSizeDefault = (int) Math.Floor((double)(1024 * 1024 * 5)); //1024 * 1024 * 5;
        private static int logRotateSizeMax = (int) Math.Floor((double)(1024 * 1024 * 10)); //1024 * 1024 * 1024;
        private int logRotateSize;
        //private int logIndex;
        private const int logRotateFilesDefault = 1;
        private const int logRotateFilesMax = 100;
        private int logRotateFiles;

        public static string DatetimeStampSeparator = new string ('-', 49); //"------------------------------------------------";
        public static string MessageSeparator = new string('=', 49); //"================================================";

        protected static Logging m_this = null;

        public static int s_iIdListener = -1;
        private static List<MESSAGE> m_listQueueMessage;

        /// <summary>
        /// Имя приложения без расширения
        /// </summary>
        public static string AppName
        {
            get
            {
                string appName = string.Empty;
                string [] args = System.Environment.GetCommandLineArgs ();
                int posAppName = -1
                    , posDelim = -1;

                posAppName = args[0].LastIndexOf('\\') + 1;

                //Отсечь параметры (после пробела)
                posDelim = args[0].IndexOf(' ', posAppName);
                if (!(posDelim < 0))
                    appName = args[0].Substring(posAppName, posDelim - posAppName - 1);
                else
                    appName = args[0].Substring(posAppName);
                //Отсечь расширение
                posDelim = appName.IndexOf('.');
                if (!(posDelim < 0))
                    appName = appName.Substring(0, posDelim);
                else
                    ;

                return appName;
            }
        }

        public static void ReLogg(LOG_MODE mode)
        {
            m_this = null;
            s_mode = mode;
        }

        public static Logging Logg()
        {
            if (m_this == null)
            {
                switch (s_mode) {
                    case LOG_MODE.FILE:
                        m_this = new Logging(System.Environment.CurrentDirectory + @"\" + AppName + "_" + Environment.MachineName + "_log.txt", false, null, null);
                        break;
                    case LOG_MODE.DB:
                    case LOG_MODE.UNKNOWN:
                    default:
                        m_this = new Logging ();
                        break;
                }
            }
            else
                ;

            return m_this;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name">имя лог-файла</param>
        /// <param name="extLog">признак - внешнее логгирование</param>
        /// <param name="updateLogText">функция записи во внешний лог-файл</param>
        /// <param name="clearLogText">функция очистки внешнего лог-файла</param>
        private Logging(string name, bool extLog, DelegateStringFunc updateLogText, DelegateFunc clearLogText)
        {
            externalLog = extLog;
            logRotateSize = logRotateSizeDefault;
            logRotateFiles = logRotateFilesDefault;
            m_fileNameStart = m_fileName = name;
            sema = new Semaphore(1, 1);

            try {
                m_sw = new LogStreamWriter(m_fileName, true, Encoding.GetEncoding("windows-1251"));
                m_fi = new FileInfo(m_fileName);
            }
            catch (Exception e) {
                //Нельзя сообщить программе...
                //throw new Exception(@"private Logging::Logging () - ...", e);
                ProgramBase.Abort ();
            }

            //logIndex = 0;
            delegateUpdateLogText = updateLogText;
            delegateClearLogText = clearLogText;
        }

        private Logging () {
        }

        /// <summary>
        /// Приостановка логгирования
        /// </summary>
        /// <returns>строка с именем лог-файла</returns>
        public string Suspend()
        {
            LogLock();

            Debug("Пауза ведения журнала...", false);

            m_sw.Close();

            return m_fi.FullName;
        }

        /// <summary>
        /// Восстановление гоггирования
        /// </summary>
        public void Resume()
        {
            m_sw = new LogStreamWriter(m_fi.FullName, true, Encoding.GetEncoding("windows-1251"));

            Debug("Возобновление ведения журнала...", false);

            LogUnlock();
        }

        /// <summary>
        /// Блокирование лог-файла для изменения содержания
        /// </summary>
        public void LogLock()
        {
            sema.WaitOne();
        }

        /// <summary>
        /// Разблокирование лог-файла после изменения содержания
        /// </summary>
        public void LogUnlock()
        {
            sema.Release();
        }

        private class MESSAGE {
            public int m_id;
            public string m_strDatetimeReg;
            public string m_text;            

            public MESSAGE (int id, DateTime dtReg, string text) {
                m_id = id;
                m_strDatetimeReg = dtReg.ToString (@"yyyyMMdd HH:mm:ss.fff");
                m_text = text;
            }
        }

        private string getInsertQuery (MESSAGE msg) {
            return @"INSERT INTO [techsite-2.X.X].[dbo].[logging]([ID_LOGMSG],[ID_APP],[ID_USER],[DATETIME_WR],[MESSAGE])VALUES" +
                                @"(" + msg.m_id + @"," + ProgramBase.s_iAppID + @"," + Users.Id + @",'" + msg.m_strDatetimeReg + @"','" + msg.m_text + @"')";
        }

        private string getInsertQuery(int id, string text)
        {
            return @"INSERT INTO [techsite-2.X.X].[dbo].[logging]([ID_LOGMSG],[ID_APP],[ID_USER],[DATETIME_WR],[MESSAGE])VALUES" +
                                @"(" + id + @"," + ProgramBase.s_iAppID + @"," + Users.Id + @",GETDATE (),'" + text + @"')";
        }
        
        /// <summary>
        /// Запись сообщения в лог-файл
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <param name="separator">признак наличия разделителя</param>
        /// <param name="timeStamp">признак наличия метки времени</param>
        /// <param name="locking">признак блокирования при записи сообщения</param>
        public void Post(ID_MESSAGE id, string message, bool separator, bool timeStamp, bool locking/* = false*/)
        {
            if (s_mode > LOG_MODE.UNKNOWN)
            {
                switch (s_mode)
                {
                    case LOG_MODE.DB:
                        if (! (s_iIdListener < 0)) {
                            if (m_listQueueMessage.Count > 0) {
                                string query = string.Empty
                                    , queryQueue = string.Empty;
                                while (m_listQueueMessage.Count > 0) {
                                    queryQueue += getInsertQuery (m_listQueueMessage[0]) + @";";
                                    m_listQueueMessage.RemoveAt(0);
                                }

                                DbSources.Sources().Request(s_iIdListener, queryQueue + query);
                            }
                            else                            
                                DbSources.Sources().Request(s_iIdListener, getInsertQuery ((int)id, message));
                        } else {
                            if (m_listQueueMessage == null) m_listQueueMessage = new List<MESSAGE>(); else ;
                            m_listQueueMessage.Add(new MESSAGE ((int)id, DateTime.Now, message));
                        }
                        break;
                    case LOG_MODE.FILE:
                        string msg = string.Empty;
                        if ((!(m_listQueueMessage == null)) && (m_listQueueMessage.Count > 0))
                        {
                             while (m_listQueueMessage.Count > 0) {
                                 msg += MessageSeparator + Environment.NewLine;

                                 msg += m_listQueueMessage[0].m_strDatetimeReg + Environment.NewLine;
                                 msg += DatetimeStampSeparator + Environment.NewLine;

                                 msg += m_listQueueMessage[0].m_text + Environment.NewLine;

                                 m_listQueueMessage.RemoveAt(0);
                             }
                        }
                        else
                        {
                        }

                        if (locking == true)
                        {
                            LogLock();
                            LogCheckRotate();
                        }
                        else
                            ;

                        if (separator == true)
                            msg += MessageSeparator + Environment.NewLine;
                        else
                            ;

                        if (timeStamp == true)
                        {
                            msg += DateTime.Now.ToString(@"dd/MM/yyyy HH:mm:ss.fff") + Environment.NewLine;
                            msg += DatetimeStampSeparator + Environment.NewLine;
                        }
                        else
                            ;

                        msg += message + Environment.NewLine;

                        if (File.Exists(m_fileName) == true)
                        {
                            try
                            {
                                if ((m_sw == null) || (m_fi == null))
                                {
                                    //Вариант №1
                                    //FileInfo f = new FileInfo(m_fileName);
                                    //FileStream fs = f.Open(FileMode.Append, FileAccess.Write, FileShare.Write);
                                    //m_sw = new LogStreamWriter(fs, Encoding.GetEncoding("windows-1251"));
                                    //Вариант №2                        
                                    m_sw = new LogStreamWriter(m_fileName, true, Encoding.GetEncoding("windows-1251"));

                                    m_fi = new FileInfo(m_fileName);
                                }
                                else
                                    ;

                                m_sw.Write(msg);
                                m_sw.Flush();
                            }
                            catch (Exception e)
                            {
                                /*m_sw.Close ();*/
                                m_sw = null;
                                m_fi = null;
                            }
                        }
                        else
                            ;

                        if (externalLog == true)
                        {
                            if (timeStamp == true)
                                delegateUpdateLogText(DateTime.Now.ToString() + ": " + message + Environment.NewLine);
                            else
                                delegateUpdateLogText(message + Environment.NewLine);
                        }
                        else
                            ;

                        if (locking == true)
                            LogUnlock();
                        else
                            ;
                        break;
                    default:
                        break;
                }
            }
            else
                ;
        }

        /*
        public bool Log
        {
            get { return logging; }
            set { logging = value; }
        }
        */
        
        /// <summary>
        /// Наименование лог-файла
        /// </summary>
        /// <returns>строка с наименованием лог-файла</returns>
        private string LogFileName(int indxArchive)
        {
            string strRes = string.Empty;
            if (indxArchive == 0)
                strRes = Path.GetDirectoryName(m_fileName) + "\\" + Path.GetFileNameWithoutExtension(m_fileName) + Path.GetExtension(m_fileName);
            else
                strRes = Path.GetDirectoryName(m_fileName) + "\\" + Path.GetFileNameWithoutExtension(m_fileName) + indxArchive.ToString() + Path.GetExtension(m_fileName);

            return strRes;
        }

        private void LogRotateNowLocked()
        {
            if (externalLog == true)
                delegateClearLogText();
            else
                ;

            try
            {
                m_sw.Close();

                //logIndex = (logIndex + 1) % logRotateFiles;
                //m_fileName = LogFileName();

                LogToArchive ();

                m_sw = new LogStreamWriter(m_fileName, false, Encoding.GetEncoding("windows-1251"));
                m_fi = new FileInfo(m_fileName);
            }
            catch (Exception e)
            {
                /*m_sw.Close ();*/
                m_sw = null;
                m_fi = null;
            }
        }

        private void LogToArchive (int indxArchive = 0) {
            string logFileName = LogFileName (indxArchive),
                logToFileName = LogFileName(++ indxArchive);

            if (File.Exists(logToFileName) == true)
            {
                if (! (indxArchive > (MAX_ARCHIVE - 1)))
                    LogToArchive(indxArchive);
                else
                    File.Delete(logToFileName);
            }
            else {
            }

            if (File.Exists(logToFileName) == false)
                File.Create (logToFileName).Close ();
            else
                ;

            File.Copy(logFileName, logToFileName, true);
        }

        private void LogRotateNow()
        {
            LogLock();
            LogRotateNowLocked();
            LogUnlock();
        }

        private void LogCheckRotate()
        {
            if (!(m_fi == null))
            {
                if (File.Exists (m_fileName) == true)
                    try {
                        m_fi.Refresh();

                        if (m_fi.Length > logRotateSize)
                            LogRotateNowLocked();
                        else
                            ;
                    }
                    catch (Exception e)
                    {
                        //m_fi = null;
                    }
                else
                    ;
            }
            else
                ;
        }

        public bool LogRotate
        {
            get { return logRotate; }
            set { logRotate = value; }
        }

        public int LogRotateMaxSize
        {
            get { return logRotateSize; }
            set 
            {
                if (value <= 0 || value > logRotateSizeMax)
                    logRotateSize = logRotateSizeMax;
                else
                    logRotateSize = value;
            }
        }

        public int LogRotateFiles
        {
            get { return logRotateFiles; }
            set
            {
                if (value <= 0 || value > logRotateFilesMax)
                    logRotateFiles = logRotateFilesMax;
                else
                    logRotateFiles = value;
            }
        }

        public void Action(string message, bool bLock = true)
        {
            Post(ID_MESSAGE.ACTION, "!Действие!: " + message, true, true, bLock);
        }

        public void Error(string message, bool bLock = true)
        {
            Post(ID_MESSAGE.ERROR, "!Ошибка!: " + message, true, true, bLock);
        }

        public void Debug(string message, bool bLock = true)
        {
            Post(ID_MESSAGE.DEBUG, "!Отладка!: " + message, true, true, bLock);
        }

        public void Exception(Exception e, string message, bool bLock = true)
        {
            string msg = string.Empty;
            msg += "!Исключение! обработка: " + message + Environment.NewLine;
            msg += "Исключение: " + e.Message + Environment.NewLine;
            msg += e.ToString();

            Post(ID_MESSAGE.EXCEPTION, msg, true, true, bLock);
        }

        internal class LogStreamWriter : StreamWriter
        {
            /*
            public LogStreamWriter(FileStream fs, System.Text.Encoding e)
                : base(fs, e)
            {
            }
            */

            public LogStreamWriter(string path, bool append, System.Text.Encoding e)
                : base(path, append, e)
            {
            }

            ~LogStreamWriter()
            {
                this.Dispose();
            }

            protected override void Dispose(bool disposing)
            {
                try { base.Dispose(disposing); }
                catch (Exception e) { }
            }
        }
    }
}
