using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace StatisticTrans
{
    /// <summary>
    /// Функции winapi
    /// </summary>
    static public class WinApi
    {
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);

        public static int RegisterWindowMessage(string format, params object[] args)
        {
            string message = String.Format(format, args);
            return RegisterWindowMessage(message);
        }

        public const int HWND_BROADCAST = 0xffff;
        public const int SW_SHOWNORMAL = 1;

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImportAttribute("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImportAttribute("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void ShowToFront(IntPtr wndw)
        {
            ShowWindow(wndw, SW_SHOWNORMAL);
            SetForegroundWindow(wndw);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    static public class SingleInstance
    {
        public static readonly int WM_SHOWINSTANCE = WinApi.RegisterWindowMessage("WM_SHOWFIRSTINSTANCE|{0}", ProgramInfo.AssemblyGuid);
        static Mutex mtx;
        static string cmdCommnd = string.Empty;

        static public bool Start(string[] cmdLine)
        {
            bool onlyInstance = false;
            string mutexName = String.Format("Local\\{0}", ProgramInfo.AssemblyGuid);

            // string mutexName = String.Format("Global\\{0}", ProgramInfo.AssemblyGuid);
            HandlerCmd(cmdLine);
            mtx = new Mutex(true, mutexName, out onlyInstance);
            return onlyInstance;
        }

        /// <summary>
        /// освобождение мьютекса
        /// </summary>
        static public void StopMtx()
        {
            mtx.ReleaseMutex();
        }

        /// <summary>
        /// 
        /// </summary>
        static private void ShwnInstance()
        {
            WinApi.PostMessage((IntPtr)WinApi.HWND_BROADCAST,
                           WM_SHOWINSTANCE,
                           IntPtr.Zero,
                           IntPtr.Zero);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdLine"></param>
        static private void HandlerCmd(string[] cmdLine)
        {
            if (cmdLine.Length > 0)
            {
                if ((!(cmdLine[0].IndexOf("start") < 0)) && ((cmdLine[0][0] == '/')))
                    cmdCommnd = cmdLine[0];
                else if ((!(cmdLine[0].IndexOf("stop") < 0)) && ((cmdLine[0][0] == '/')))
                    cmdCommnd = cmdLine[0];
            }
            else ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdCommnd"></param>
        static public void restartInstance()
        {
            if (cmdCommnd.Length > 0)
            {
                if (cmdCommnd == "/start")
                    ShwnInstance();
                else
                    ;
            }
            else
                ShwnInstance();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public bool runSingleInstance()
        {
            if (cmdCommnd != string.Empty)
            {
                if (cmdCommnd == "/start")
                    return true;
                else
                    return false;
            }
            else
                return true;
        }

        /// <summary>
        /// уничтожает процесс
        /// </summary>
        static private void killProcess()
        {
            string processName = Process.GetCurrentProcess().ProcessName;

            Process[] RunningProcesses = Process.GetProcessesByName(processName);
            if (RunningProcesses.Length > 0)
            {
                Process runningProcess = RunningProcesses[0];
                if (runningProcess != null)
                    runningProcess.Kill();
            }
        }
    }
}

