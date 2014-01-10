using System;
using System.Collections.Generic;
using System.Windows.Forms;

using StatisticCommon;

namespace Statistic
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Logging.Logg().LogLock();
            Logging.Logg().LogToFile("=============Старт приложения...=============", true, true, false);
            Logging.Logg().LogUnlock();

            //InitTEC init = new InitTEC();

            FormMain formMain = null;
            try { formMain = new FormMain(); }
            catch (Exception e)
            {
                Logging.Logg().LogExceptionToFile(e, "=============Ошибка запуска приложения...=============");
            }

            //mainForm.Show();
            //ToolStripItem [] items;
            //if (mainForm.tec == null)
            //{
            //    foreach (ToolStripItem item in mainForm.MainMenuStrip.Items)
            //    {
            //        if (item.Text.Contains ("Настройки")) {
            //            foreach (ToolStripMenuItem menuItem in ((ToolStripMenuItem)item).DropDownItems)
            //            {
            //                if (menuItem.Text.Contains("соединения"))
            //                    menuItem.PerformClick();
            //                else
            //                    ;
            //            }
            //        }
            //        else
            //            ;
            //    }
            //    //items = mainForm.MainMenuStrip.Items.Find("Настройки", false); // соединения
            //    //items[0].PerformClick();
            //}
            //else
            //    ;

            if (!(formMain == null))
                Application.Run(formMain);
            else
                ;

            Logging.Logg ().LogLock ();
            Logging.Logg().LogToFile("=============Выход из приложения...=============", true, true, false);
            Logging.Logg().LogUnlock();
        }
    }
}