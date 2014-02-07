using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace StatisticCommon
{
    public static class ProgramBase
    {
        public static string MessageWellcome = "***************Старт приложения...***************";
        public static string MessageExit = "***************Выход из приложения...***************";
        
        //Журналирование старта приложения
        public static void Start()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Logging.Logg().LogLock();
            Logging.Logg().LogToFile(MessageWellcome, true, true, false);
            Logging.Logg().LogUnlock();
        }

        //Журналирование завершения приложения
        public static void Exit()
        {
            Logging.Logg().LogLock();
            Logging.Logg().LogToFile(MessageExit, true, true, false);
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