using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Security;
using System.Runtime.InteropServices;
using System.IO;
using HClassLibrary;
using StatisticCommon;
using StatisticTrans;

namespace trans_mc
{

    static class Program
    {

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int ShowWindow(int hwnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetWindow(int hwnd, int nCmdShow);

        static readonly string AppPath = Path.GetFileNameWithoutExtension(Application.ExecutablePath);

        static bool isFirstInstance;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //Logging.s_mode = Logging.LOG_MODE.UNKNOWN; //Если назначить неизвестный тип логирования - 1-е сообщения б. утеряны
            //Logging.s_mode = Logging.LOG_MODE.DB;
            Logging.s_mode = Logging.LOG_MODE.FILE_EXE;

            FormMainTransMC formMain = null;
            SingleInstanceRun st;
            ProgramBase.Start();

            if (args.Length > 0)
                st = new SingleInstanceRun(args);
            else
                st = new SingleInstanceRun();

            using (Mutex mutex = new Mutex(true, st.nameMutex, out isFirstInstance))
            {
                //st.CheckArgCmdLine(isFirstInstance);

                if (!isFirstInstance && !mutex.WaitOne(0,false))
                {

                }
                else
                {
                    GC.Collect();

                    if (formMain == null)
                    {
                        try { formMain = new FormMainTransMC(); }
                        catch (Exception e)
                        { Logging.Logg().Exception(e, "Ошибка запуска приложения.", Logging.INDEX_MESSAGE.NOT_SET); }
                    }
                    MessageBox.Show("PERVZAPUSK");
                    Application.Run(formMain);

                    MessageBox.Show("NO RUN - NO SHOW");
                }
            }

            ProgramBase.Exit();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processName"></param>
        private static void BringOldInstanceToFront(string processName)
        {
            Process[] RunningProcesses = Process.GetProcessesByName(processName);
            if (RunningProcesses.Length > 0)
            {
                Process runningProcess = RunningProcesses[0];
                if (runningProcess != null)
                {
                    ShowWindow((int)runningProcess.MainWindowHandle, 1);//нормально развернутое
                    //ShowWindow((int)runningProcess.MainWindowHandle, 3);//максимально развернутое
                    SetForegroundWindow(runningProcess.Handle);
                    GetWindow((int)runningProcess.Handle, 1);

                }
            }
        }
    }
}

