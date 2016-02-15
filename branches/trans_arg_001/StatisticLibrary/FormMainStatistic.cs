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
    /// <summary>
    /// 
    /// </summary>
    public abstract partial class FormMainStatistic : FormMainBaseWithStatusStrip
    {
        protected static FileINI m_sFileINI;//setup.ini
        protected static FIleConnSett m_sFileCS;//connsett.ini

        /// <summary>
        /// вызов класса работы с командной строкой
        /// </summary>
        /// <param name="args">параметры командной строки</param>
        /// <returns>класс</returns>
        protected virtual HCmd_Arg createHCmdArg(string[] args)
        {
            return new HCmd_Arg(args);
        }

        /// <summary>
        /// Контруктор класса
        /// </summary>
        public FormMainStatistic()
        {
            createHCmdArg(Environment.GetCommandLineArgs());
        }

        /// <summary>
        /// Класс обработки камандной строки
        /// </summary>
        public class HCmd_Arg
        {
            /// <summary>
            /// значения командной строки
            /// </summary>
            static public string cmd;
            /// <summary>
            /// параметр командной строки
            /// </summary>
            static public string param;

            /// <summary>
            /// Основной конструктор класса
            /// </summary>
            /// <param name="args">параметры командной строки</param>
            public HCmd_Arg(string[] args)
            {
                handlerArgs(args);
                if (!SingleInstance.onlyInstance)
                    execCmdLine(cmd);
                else
                    if (cmd == "stop")
                        execCmdLine(cmd);
                    else ;
            }

            /// <summary>
            /// обработка CommandLine
            /// </summary>
            /// <param name="cmdLine">командная строка</param>
            static private void handlerArgs(string[] cmdLine)
            {
                string[] m_cmd = new string[cmdLine.Length];

                if (m_cmd.Length > 1)
                {
                    m_cmd = cmdLine[1].Split('/', '=');

                    if (m_cmd.Length > 2)
                    {
                        cmd = m_cmd[1];
                        param = m_cmd[2];
                    }
                    else
                        cmd = m_cmd[1];
                }
            }

            /// <summary>
            /// Обработка команды старт/стоп
            /// </summary>
            /// <param name="CmdStr">команда приложению</param>
            static public void execCmdLine(string CmdStr)
            {
                switch (CmdStr)
                {
                    case "start":
                        if (!(SingleInstance.onlyInstance))
                        {
                            SingleInstance.SwitchToCurrentInstance();
                            SingleInstance.CloseForm();
                        }
                        break;
                    case "stop":
                        if (!(SingleInstance.onlyInstance))
                        {
                            SingleInstance.StopApp();
                            SingleInstance.CloseForm();
                        }
                        else
                            SingleInstance.CloseForm();
                        break;
                    default:
                        if (!(SingleInstance.onlyInstance))
                        {
                            SingleInstance.SwitchToCurrentInstance();
                            SingleInstance.CloseForm();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void timer_Start()
        {
            int i = -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
        public void CreatefileConnSett(string connSettFileName)
        {
            m_sFileCS = new FIleConnSett(connSettFileName, FIleConnSett.MODE.FILE);
            s_listFormConnectionSettings = new List<FormConnectionSettings>();
            s_listFormConnectionSettings.Add(new FormConnectionSettings(-1, m_sFileCS.ReadSettingsFile, m_sFileCS.SaveSettingsFile));
        }

        /// <summary>
        /// класс исключений
        /// </summary>
        public class SingleException : Exception
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="msg"></param>
            public SingleException(string msg)
                : base(msg)
            {
            }
        }

        /// <summary>
        /// класс по работе с запущенным приложением
        /// </summary>
        static public class SingleInstance
        {
            static private string mtxName = ProgramInfo.AssemblyGuid.ToString();
            static Mutex m_mtx;
            //static private IntPtr m_hWnd;

            /// <summary>
            /// 
            /// </summary>
            static public bool onlyInstance
            {
                get
                {
                    bool onlyInstance;
                    m_mtx = new Mutex(true, mtxName, out onlyInstance);
                    return onlyInstance;
                }
            }

            /// <summary>
            /// Отправка сообщения приложению
            /// для его активации
            /// </summary>
            /// <param name="hWnd">дескриптор окна</param>
            static private void sendMsg(IntPtr hWnd)
            {
                WinApi.SendMessage(hWnd, WinApi.SW_RESTORE, IntPtr.Zero, IntPtr.Zero);
            }

            /// <summary>
            /// освобождение мьютекса
            /// </summary>
            static public void ReleaseMtx()
            {
                m_mtx.ReleaseMutex();
            }

            /// <summary>
            /// Остановка работы формы
            /// </summary>
            static public void StopApp()
            {
                WinApi.SendMessage(mainhWnd, WinApi.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }

            /// <summary>
            /// Закрытие окна
            /// </summary>
            static public void CloseForm()
            {
                Environment.Exit(0);

                //IntPtr m_hWnd = IntPtr.Zero;
                //Process process = Process.GetCurrentProcess();
                //Process[] processes = Process.GetProcessesByName(process.ProcessName);
                //foreach (Process _process in processes)
                //{
                //    if (_process.Id == process.Id &&
                //        _process.MainModule.FileName == process.MainModule.FileName &&
                //        _process.Handle != IntPtr.Zero)
                //    {
                //        m_hWnd = _process.MainWindowHandle;
                  
                //      //if (m_hWnd == IntPtr.Zero)
                //      //    m_hWnd = enumID(_process.Id);
                //      //else ;
                //        break;
                //    }
                //}
            }

            /// <summary>
            /// поиск дескриптора по процессу
            /// </summary>
            /// <returns>дескриптор окна</returns>
            static public IntPtr mainhWnd
            {
                get
                {
                    IntPtr m_hWnd  = IntPtr.Zero;
                    Process process = Process.GetCurrentProcess();
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
                            m_hWnd = _process.MainWindowHandle;

                            if (m_hWnd == IntPtr.Zero)
                               m_hWnd = enumID(_process.Id);
                            else ;
                            break;
                        }
                    }
                    return m_hWnd;
                }
            }

            /// <summary>
            /// выборка всех запущенных приложений
            /// </summary>
            /// <param name="id">ид процесса приложения</param>
            static private IntPtr enumID(int id)
            {
                IntPtr hwnd = IntPtr.Zero;
                WinApi.EnumWindows((hWnd, lParam) =>
                {
                    if (WinApi.IsWindowVisible(hWnd) && (WinApi.GetWindowTextLength(hWnd) != 0))
                    {
                        if (WinApi.IsIconic(hWnd) != 0 &&
                            WinApi.GetPlacement(hWnd).showCmd == WinApi.ShowWindowCommands.Minimized)
                           hwnd = findCurProc(id, hWnd);
                        else ;
                    }
                    return true;
                }, IntPtr.Zero);

                return hwnd;
            }

            ///// <summary>
            ///// Получение заголовка окна
            ///// </summary>
            ///// <param name="hWnd">дескриптор приложения</param>
            ///// <returns>заголовок окна</returns>
            //private static string getWindowText(IntPtr hWnd)
            //{
            //    int len = WinApi.GetWindowTextLength(hWnd) + 1;
            //    StringBuilder sb = new StringBuilder(len);
            //    len = WinApi.GetWindowText(hWnd, sb, len);
            //    return sb.ToString(0, len);
            //}

            /// <summary>
            /// Активация окна
            /// </summary>
            static public void SwitchToCurrentInstance()
            {
                IntPtr hWnd = mainhWnd;
                sendMsg(hWnd);

                if (hWnd != IntPtr.Zero)
                {
                    // Restore window if minimised. Do not restore if already in
                    // normal or maximised window state, since we don't want to
                    // change the current state of the window.
                    if (WinApi.IsIconic(hWnd) != 0)
                        WinApi.ShowWindow(hWnd, WinApi.SW_RESTORE);
                    else
                        ;
                    // Set foreground window.
                    WinApi.SetForegroundWindow(hWnd);
                }
            }

            /// <summary>
            /// поиск нужного процесса
            /// </summary>
            /// <param name="id">идентификатор приложения</param>
            /// <param name="hwd">дескриптор окна</param>
            static private IntPtr findCurProc(int id, IntPtr hwd)
            {
                int _ProcessId;
                WinApi.GetWindowThreadProcessId(hwd, out _ProcessId);

                if (id == _ProcessId)
                   return hwd;
                else 
                    return IntPtr.Zero;
            }
        }
    }
}
