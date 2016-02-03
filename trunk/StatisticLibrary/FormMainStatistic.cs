using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HClassLibrary;

namespace StatisticCommon
{
    public abstract partial class FormMainStatistic : FormMainBaseWithStatusStrip
    {
        protected static FileINI m_sFileINI;
        protected static FIleConnSett m_sFileCS;

        public FormMainStatistic()
        {

        }

        protected override void timer_Start()
        {
            int i = -1;
        }

        protected override int UpdateStatusString()
        {
            int have_msg = 0;

            have_msg = (m_report.errored_state == true) ? -1 : (m_report.warninged_state == true) ? 1 : 0;

            if (((!(have_msg == 0)) || (m_report.actioned_state == true)))
            {
                if (m_report.actioned_state == true)
                {
                    m_lblDescMessage.Text = m_report.last_action;
                    m_lblDateMessage.Text = m_report.last_time_action.ToString();
                }
                else
                    ;

                if (have_msg == 1)
                {
                    m_lblDescMessage.Text = m_report.last_warning;
                    m_lblDateMessage.Text = m_report.last_time_warning.ToString();
                }
                else
                    ;

                if (have_msg == -1)
                {
                    m_lblDescMessage.Text = m_report.last_error;
                    m_lblDateMessage.Text = m_report.last_time_error.ToString();
                }
                else
                    ;
            }
            else
            {
                m_lblDescMessage.Text = string.Empty;
                m_lblDateMessage.Text = string.Empty;
            }

            return have_msg;
        }

        protected override void HideGraphicsSettings()
        {
        }

        protected override void UpdateActiveGui(int type)
        {
        }

        /// <summary>
        /// создание ConnSett
        /// </summary>
        /// <param name="connSettFileName"></param>
        public void createfileConnSett(string connSettFileName)
        {
            m_sFileCS = new FIleConnSett(connSettFileName, FIleConnSett.MODE.FILE);
            s_listFormConnectionSettings = new List<FormConnectionSettings>();
            s_listFormConnectionSettings.Add(new FormConnectionSettings(-1, m_sFileCS.ReadSettingsFile, m_sFileCS.SaveSettingsFile));
        }

        /// <summary>
        /// класс обработки старт/стоп
        /// </summary>
        static public class SingleInstance
        {
            static public bool stopbflg = true;
            public static string mutexName = ProgramInfo.AssemblyGuid.ToString();
            static Mutex mtx;
            static public IntPtr m_hndl;
            static string CmdStr = string.Empty;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            static public bool Start()
            {
                string[] args = Environment.GetCommandLineArgs();

                handlerCmd(args);
                return execCmdLine();
            }

            /// <summary>
            /// проверка экземпляра на запуск
            /// </summary>
            /// <returns></returns>
            static private bool onlyInstance()
            {
                bool onlyInstance = false;
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
            /// обработка CommandLine
            /// </summary>
            /// <param name="cmdLine">командная строка</param>
            static private void handlerCmd(string[] cmdLine)
            {
                if (cmdLine.Count() > 1)
                {
                    cmdLine = cmdLine[1].Split('/');

                    if ((!(cmdLine[1].IndexOf("start") < 0)))
                        CmdStr = cmdLine[1];
                    else if ((!(cmdLine[1].IndexOf("stop") < 0)))
                        CmdStr = cmdLine[1];
                }
                else ;
            }

            /// <summary>
            /// Обработка команды старт/стоп
            /// </summary>
            /// <returns></returns>
            static public bool execCmdLine()
            {
                switch (CmdStr)
                {
                    case "start":
                        if (!onlyInstance())
                        {
                            WinApi.SendMessage(mainhwd(), WinApi.SW_RESTORE, IntPtr.Zero, IntPtr.Zero);
                            SwitchToCurrentInstance(mainhwd());
                            return true;
                        }
                        else return false;

                    case "stop":
                        if (!onlyInstance())
                        {
                            stopApp();
                            return true;
                        }
                        else
                        {
                            stopbflg = false;
                            return false;
                        }

                    default:
                        if (!onlyInstance())
                            return true;
                        else return false;
                }
            }

            /// <summary>
            /// Остановка работы формы
            /// </summary>
            static private void stopApp()
            {
                WinApi.SendMessage(mainhwd(), WinApi.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }

            /// <summary>
            /// поиск дескриптора по процессу
            /// </summary>
            /// <returns>дескриптор окна</returns>
            static public IntPtr mainhwd()
            {
                Process process = Process.GetCurrentProcess();
                Process[] proc = Process.GetProcesses();
                Process[] processes = Process.GetProcessesByName(process.ProcessName);
                foreach (Process _process in processes)
                {
                    // Get the first instance that is not this instance, has the
                    // same process name and was started from the same file name
                    // and location. Also check that the process has a valid
                    // window handle in this session to filter out other user's
                    // processes.
                    if (_process.Id != process.Id &&
                        _process.MainModule.FileName == process.MainModule.FileName &&
                        _process.Handle != IntPtr.Zero)
                    {
                        m_hndl = _process.MainWindowHandle;

                        if (m_hndl == IntPtr.Zero)
                            Enum(_process.Id);
               
                        WinApi.GetWindowTextLength(m_hndl);
                 
                        break;
                    }
                }
                return m_hndl;
            }

            /// <summary>
            /// выборка всех запущенных приложений
            /// </summary>
            /// <param name="id">ид процесса приложения</param>
            private static void Enum(int id)
            {
                WinApi.EnumWindows((hWnd, lParam) =>
                {
                    if (WinApi.IsWindowVisible(hWnd) && (WinApi.GetWindowTextLength(hWnd) != 0))
                    {
                        if (WinApi.IsIconic(hWnd) != 0 && 
                            WinApi.GetPlacement(hWnd).showCmd.ToString() == "Minimized")
                        {
                           FindCurProc(id,hWnd);
                        }
                        else ;
                    }
                    return true;
                }, IntPtr.Zero);
            }

            /// <summary>
            /// Получение заголовка окна
            /// </summary>
            /// <param name="hWnd">дескриптор приложения</param>
            /// <returns></returns>
            private static string GetWindowText(IntPtr hWnd)
            {
                int len = WinApi.GetWindowTextLength(hWnd) + 1;
                StringBuilder sb = new StringBuilder(len);
                len = WinApi.GetWindowText(hWnd, sb, len);
                return sb.ToString(0, len);
            }

            /// <summary>
            /// Активация окна
            /// </summary>
            private static void SwitchToCurrentInstance(IntPtr hWnd)
            {
                if (hWnd != IntPtr.Zero)
                {
                    // Restore window if minimised. Do not restore if already in
                    // normal or maximised window state, since we don't want to
                    // change the current state of the window.
                    if (WinApi.IsIconic(hWnd) != 0)
                        WinApi.ShowWindow(hWnd, WinApi.SW_RESTORE);
                    else ;

                    // Set foreground window.
                    WinApi.SetForegroundWindow(hWnd);
                }
            }

            /// <summary>
            /// поиск нужного процесса
            /// </summary>
            /// <param name="id">идентификатор приложения</param>
            /// <param name="hwd">дескриптор окна</param>
            private static void FindCurProc(int id, IntPtr hwd)
            {
                int _ProcessId;

                WinApi.GetWindowThreadProcessId(hwd, out _ProcessId);

                if (id == _ProcessId)
                    m_hndl = hwd;
            }
        }
    }
  
}
