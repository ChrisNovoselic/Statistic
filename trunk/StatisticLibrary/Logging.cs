using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace StatisticCommon
{
    public class Logging
    {
        private string fileNameStart;
        private string fileName;
        private bool externalLog;
        private DelegateStringFunc delegateUpdateLogText;
        private DelegateFunc delegateClearLogText;
        private Semaphore sema;
        private StreamWriter sw;
        private FileInfo fi;
        private bool logging = true;
        private bool logRotate = true;
        private const int logRotateSizeDefault = 1024 * 1024 * 5;
        private const int logRotateSizeMax = 1024 * 1024 * 1024;
        private int logRotateSize;
        private int logIndex;
        private const int logRotateFilesDefault = 1;
        private const int logRotateFilesMax = 100;
        private int logRotateFiles;

        private static Logging m_this = null;
        //private static string m_appName = null;
        public static Logging Logg () {
            if (m_this == null)
            {
                int posAppName = System.Environment.CommandLine.LastIndexOf('\\') + 1;
                string appName = System.Environment.CommandLine.Substring(posAppName, System.Environment.CommandLine.IndexOf(' ', posAppName) - posAppName - 1);
                appName = appName.Substring(0, appName.IndexOf ('.'));
                m_this = new Logging(System.Environment.CurrentDirectory + @"\" + appName + "_" + Environment.MachineName + "_log.txt", false, null, null);
            }
            else
                ;

            return m_this;
        }

        private Logging(string name, bool extLog, DelegateStringFunc updateLogText, DelegateFunc clearLogText)
        {
            externalLog = extLog;
            logRotateSize = logRotateSizeDefault;
            logRotateFiles = logRotateFilesDefault;
            fileNameStart = fileName = name;
            sema = new Semaphore(1, 1);
            FileInfo f = new FileInfo(fileName);
            FileStream fs = f.Open(FileMode.Append, FileAccess.Write, FileShare.Write);
            sw = new StreamWriter(fs, Encoding.GetEncoding("windows-1251"));
            fi = new FileInfo(fileName);
            logIndex = 0;
            delegateUpdateLogText = updateLogText;
            delegateClearLogText = clearLogText;
        }

        public void LogLock()
        {
            sema.WaitOne();
        }

        public void LogUnlock()
        {
            sema.Release();
        }

        public void LogToFile(string message, bool separator, bool timeStamp, bool locking/* = false*/)
        {
            if (logging)
            {
                if (locking)
                {
                    LogLock();
                    LogCheckRotate();
                }

                if (separator)
                    sw.WriteLine("================================================");
                if (timeStamp)
                {
                    sw.WriteLine(DateTime.Now.ToString());
                    sw.WriteLine("------------------------------------------------");
                }
                sw.WriteLine(message);
                sw.Flush();

                if (externalLog)
                {
                    if (timeStamp)
                        delegateUpdateLogText(DateTime.Now.ToString() + ": " + message + "\r\n");
                    else
                        delegateUpdateLogText(message + "\r\n");
                }

                if (locking)
                    LogUnlock();
            }
        }

        public bool Log
        {
            get { return logging; }
            set { logging = value; }
        }

        private void LogRotateNowLocked()
        {
            sw.Close();
            if (externalLog)
                delegateClearLogText();
            else
                ;

            logIndex = (logIndex + 1) % logRotateFiles;

            string newFilename;
            if (logIndex == 0)
                newFilename = Path.GetDirectoryName(fileNameStart) + "\\" + Path.GetFileNameWithoutExtension(fileNameStart) + Path.GetExtension(fileNameStart);
            else
                newFilename = Path.GetDirectoryName(fileNameStart) + "\\" + Path.GetFileNameWithoutExtension(fileNameStart) + logIndex.ToString() + Path.GetExtension(fileNameStart);
            
            sw = new StreamWriter(newFilename, false, Encoding.GetEncoding("windows-1251"));
            fi = new FileInfo(newFilename);
        }
        
        private void LogRotateNow()
        {
            LogLock();
            LogRotateNowLocked();
            LogUnlock();
        }
        
        private void LogCheckRotate()
        {
            fi.Refresh();
            if (fi.Length > logRotateSize)
                LogRotateNowLocked();
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
    }
}
