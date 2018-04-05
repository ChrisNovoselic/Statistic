﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

using System.Threading; //Mutex
using System.Reflection; //Assembly

//using HClassLibrary;
using StatisticCommon;
using System.Linq;
using ASUTP;
using ASUTP.Helper;

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

            ////Если в строке Assembly заменить номер Построения на "*", номер Ревизия на "очистить"
            //Version version = Assembly.GetExecutingAssembly().GetName().Version;
            //DateTime buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);

            string strHeaderError = string.Empty;
            bool bRun = false;  
            s_mtxApp = new Mutex(true, ProgramBase.AppName, out bRun);  
            if (bRun == true)  
            {
                ProgramBase.s_iAppID = ProgramBase.ID_APP.STATISTIC; //??? 
                //ProgramBase.s_iAppID = Int32.Parse ((string)Properties.Settings.Default [@"AppID"]);

                try { ProgramBase.Start (Logging.LOG_MODE.DB, true); }
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
                        Logging.Logg().Exception(e, strHeaderError, Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    if (!(formMain == null))
                        try { Application.Run(formMain); }
                        catch (Exception e)
                        {
                            strHeaderError = "Ошибка выполнения приложения";
                            MessageBox.Show((IWin32Window)null, e.Message + Environment.NewLine + ProgramBase.MessageAppAbort, strHeaderError);
                            Logging.Logg().Exception(e, strHeaderError, Logging.INDEX_MESSAGE.NOT_SET);
                        }
                    else
                        ;

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