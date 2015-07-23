using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using HClassLibrary;

namespace StatisticDiagnostic1
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
           /* Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormDiagnostic());
            PanelStatisticDiagnostic1 form = new PanelStatisticDiagnostic1();*/

                 /*Thread thread = new Thread(form.ThreadFunction);
                 //Запускаем поток
                 thread.Priority = ThreadPriority.Highest;
                 thread.Start();*/
             
          
            int iRes = 0;

            //Logging.s_mode = Logging.LOG_MODE.UNKNOWN; //Если назначить неизвестный тип логирования - 1-е сообщения б. утеряны
            //Logging.s_mode = Logging.LOG_MODE.DB;
            Logging.s_mode = Logging.LOG_MODE.FILE_EXE;

            try { ProgramBase.Start(); }
            catch (Exception e)
            {
                //MessageBox.Show(null, @"Возможно, повторный запуск приложения" + @".\nили обратитесь к оператору тех./поддержки по тел. 4444 или по тел. 289-03-37.", "Ошибка инициализации!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                MessageBox.Show(null, e.Message + "\nили обратитесь к оператору тех./поддержки по тел. 4444 или по тел. 289-03-37.", "Ошибка инициализации!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                iRes = -1;
            }

            FormDiagnostic formMainD = null;

            if (iRes == 0)
            {
                string strHeader = string.Empty;
                try { formMainD = new FormDiagnostic(); }
                catch (Exception e)
                {
                    strHeader = "Ошибка запуска приложения";
                    MessageBox.Show((IWin32Window)null, e.Message + Environment.NewLine + ProgramBase.MessageAppAbort, strHeader);
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, strHeader);
                }

                if (!(formMainD == null))
                    try { Application.Run(formMainD); }
                    catch (Exception e)
                    {
                        strHeader = "Ошибка выполнения приложения";
                        MessageBox.Show((IWin32Window)null, e.Message + Environment.NewLine + ProgramBase.MessageAppAbort, strHeader);
                        Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, strHeader);
                    }
                else
                    ;

                ProgramBase.Exit();
            }
            else
                ;
        }



    }
}
