using System;
using System.Collections.Generic;
using System.Windows.Forms;

using System.Threading; //Mutex

using HClassLibrary;
using StatisticCommon;

namespace Statistic
{
    static class Program
    {
        static Mutex s_mtxApp;
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            int iRes = 0;

            //Logging.s_mode = Logging.LOG_MODE.UNKNOWN; //Если назначить неизвестный тип логирования - 1-е сообщения б. утеряны
            Logging.s_mode = Logging.LOG_MODE.DB;
            //Logging.s_mode = Logging.LOG_MODE.FILE;

            string strHeaderError = string.Empty;
            bool bRun = false;  
            s_mtxApp = new Mutex(true, Logging.AppName, out bRun);  
            if (bRun == true)  
            {
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
                        strHeaderError = "Ошибка запуска приложения";
                        MessageBox.Show((IWin32Window)null, e.Message + Environment.NewLine + ProgramBase.MessageAppAbort, strHeaderError);
                        Logging.Logg().Exception(e, strHeaderError);
                    }

                    if (!(formMain == null))
                        try { Application.Run(formMain); }
                        catch (Exception e)
                        {
                            strHeaderError = "Ошибка выполнения приложения";
                            MessageBox.Show((IWin32Window)null, e.Message + Environment.NewLine + ProgramBase.MessageAppAbort, strHeaderError);
                            Logging.Logg().Exception(e, strHeaderError);
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
                }
                else
                    ;
            }
            else
            {
                strHeaderError = @"Ошибка";
                MessageBox.Show((IWin32Window)null, @"Запуск дублирующего экземпляра приложения" + Environment.NewLine + ProgramBase.MessageAppAbort, strHeaderError);
            }
        }
    }
}