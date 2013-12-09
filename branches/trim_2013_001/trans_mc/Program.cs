using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace trans_mc
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try { Application.Run(new FormMainTransMC()); }
            catch (Exception e) { 
                StatisticCommon.Logging.Logg().LogLock();
                //StatisticCommon.Logging.Logg().LogToFile("", true, true, false);
                StatisticCommon.Logging.Logg().LogToFile("Исключение " + e.Message, true, true, false);
                StatisticCommon.Logging.Logg().LogToFile(e.ToString(), false, false, false);
                StatisticCommon.Logging.Logg().LogUnlock();
            }
        }
    }
}
