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
            int iRes = 0;
            try { ProgramBase.Start (); }
            catch (Exception e) {
                //MessageBox.Show(null, @"Возможно, повторный запуск приложения" + @".\nили обратитесь к оператору тех./поддержки по тел. 4444 или по тел. 289-03-37.", "Ошибка инициализации!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                MessageBox.Show(null, e.Message + "\nили обратитесь к оператору тех./поддержки по тел. 4444 или по тел. 289-03-37.", "Ошибка инициализации!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                iRes = -1;
            }

            FormMain formMain = null;
            
            if (iRes == 0)
            {
                try { formMain = new FormMain(); }
                catch (Exception e)
                {
                    Logging.Logg ().Exception (e, "Ошибка запуска приложения.");
                }

                if (!(formMain == null))
                    try { Application.Run(formMain); }
                    catch (Exception e)
                    {
                        Logging.Logg ().Exception (e, "Ошибка выполнения приложения.");
                    }
                else
                    ;

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

                ProgramBase.Exit();

                DbSources.Sources().UnRegister();
            }
            else
                ;
        }
    }
}