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

            Logging.Logg().LogToFile(MessageWellcome, true, true, true);
        }

        //Журналирование завершения приложения
        public static void Exit()
        {
            Logging.Logg().LogToFile(MessageExit, true, true, true);
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