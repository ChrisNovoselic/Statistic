using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using StatisticTrans;
using HClassLibrary;
using StatisticCommon;

namespace StatisticTrans
{
    /// <summary>
    /// 
    /// </summary>
    public class RunOneInstance
    {

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int ShowWindow(int hwnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetWindow(int hwnd, int nCmdShow);

        static Mutex _syncObject;
        static readonly string AppPath = Path.GetFileNameWithoutExtension(Application.ExecutablePath);

        /// <summary>
        /// Событие - получение данных 
        /// </summary>
        public static event DelegateBoolFunc EvtRecieved;

        /// <summary>
        /// Находит запущенную копию приложения и разворачивает окно
        /// </summary>
        /// <param name="UniqueValue">уникальное значение для каждой программы (можно имя)</param>
        /// <returns>true - если приложение было запущено</returns>
        public static bool ChekRunProgramm(string UniqueValue)
        {
            bool applicationRun = false;
            _syncObject = new Mutex(true, UniqueValue, out applicationRun);

            if (!applicationRun)
            {
                //восстановить/развернуть окно                              
                try
                {
                    Process[] procs = Process.GetProcessesByName(AppPath);
                    MessageBox.Show(AppPath.ToString());

                    foreach (Process proc in procs)
                        if (!(proc.Id == Process.GetCurrentProcess().Id))
                        {
                            MessageBox.Show("WOW");
                            ShowWindow((int)proc.MainWindowHandle, 1);//нормально развернутое
                            //ShowWindow((int)proc.MainWindowHandle, 3);//максимально развернутое
                            SetForegroundWindow(proc.MainWindowHandle);
                            GetWindow((int)proc.MainWindowHandle,1);
                            EvtRecieved(!applicationRun);
                            break;
                        }
                }
                catch 
                { return false; }
            }
         
            return !applicationRun;
        }
    }
}

    