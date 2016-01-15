using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Security;
using System.Runtime.InteropServices;
using System.IO;
using HClassLibrary;
using StatisticCommon;
using StatisticTrans;

namespace trans_mc
{
    static class Program
    {

        // Имя мьютекса
        /* static readonly string MutexName = "{E42FC05F-0575-4EAB-8075-F2F542BDA909}";

         // Промежуток времени, в течение которого подождать возможного закрытия уже работающей копии приложения.
         static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(200);*/

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //Logging.s_mode = Logging.LOG_MODE.UNKNOWN; //Если назначить неизвестный тип логирования - 1-е сообщения б. утеряны
            //Logging.s_mode = Logging.LOG_MODE.DB;
            Logging.s_mode = Logging.LOG_MODE.FILE_EXE;

            FormMainTransMC formMain = null;
            ProgramBase.Start();
            SingleInstanceRun st = new SingleInstanceRun();

            if (st.SingleInstance() == true)
            {
                
            }

            if (formMain == null)
            {
                try { formMain = new FormMainTransMC(); }
                catch (Exception e) { Logging.Logg().Exception(e, "Ошибка запуска приложения.", Logging.INDEX_MESSAGE.NOT_SET); }
            }

            Application.Run(formMain);

            ProgramBase.Exit();
        }
    }
}

