using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using HClassLibrary;
using StatisticCommon;

namespace StatisticAnalyzer
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logging.s_mode = Logging.LOG_MODE.FILE_EXE;

            ProgramBase.Start();

            Application.Run(new FormMain_DB());

            ProgramBase.Exit ();
        }
    }
}
