using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace trans_tg
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
            try { Application.Run(new FormMainTransTG()); }
            catch (Exception e)
            {
                StatisticCommon.Logging.Logg().LogLock();
                //StatisticCommon.Logging.Logg().LogToFile("", true, true, false);
                StatisticCommon.Logging.Logg().LogToFile("Исключение " + e.Message, true, true, false);
                StatisticCommon.Logging.Logg().LogToFile(e.ToString(), false, false, false);
                StatisticCommon.Logging.Logg().LogUnlock();
            }
        }
    }
}
