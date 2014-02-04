using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace StatisticCommon
{
    public static class ProgramBase
    {
        //Журналирование старта приложения
        public static void Start()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Logging.Logg().LogLock();
            Logging.Logg().LogToFile("=============Старт приложения...=============", true, true, false);
            Logging.Logg().LogUnlock();
        }

        //Журналирование завершения приложения
        public static void Exit()
        {
            Logging.Logg().LogLock();
            Logging.Logg().LogToFile("=============Выход из приложения...=============", true, true, false);
            Logging.Logg().LogUnlock();
        }

        public static void Abort()
        {
        }

        public static string AppName
        {
            get
            {
                return Logging.AppName + ".exe";
            }
        }
    }
}