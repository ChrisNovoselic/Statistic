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
                st = new SingleInstanceRun(args[0]);
            else
                st = new SingleInstanceRun();

            if (true)
            {
                using (Mutex mutex = new Mutex(true, st.nameMutex, out isFirstInstance))
                {
                    if (!isFirstInstance && !mutex.WaitOne(0, false))
                    {
                        MessageBox.Show("DOUBLERUN!");
                        string processName = Process.GetCurrentProcess().ProcessName;
                        BringOldInstanceToFront(processName);
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
                    }
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
                    //runningProcess.Handle
                    IntPtr mainWindowHandle = runningProcess.Handle;
                    NativeMethods.ShowWindowAsync(mainWindowHandle, 1);
                    NativeMethods.ShowWindowAsync(mainWindowHandle, 9);
                }
            }
        }
    }
}

