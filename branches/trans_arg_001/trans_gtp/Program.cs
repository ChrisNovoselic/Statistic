﻿using System;
using System.Collections.Generic;
//using System.Linq;
using System.Windows.Forms;

using HClassLibrary;
using StatisticCommon;

namespace trans_gtp
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Logging.s_mode = Logging.LOG_MODE.UNKNOWN; //Если назначить неизвестный тип логирования - 1-е сообщения б. утеряны
            //Logging.s_mode = Logging.LOG_MODE.DB;
            Logging.s_mode = Logging.LOG_MODE.FILE_EXE;
            ProgramBase.Start();

            FormMainTransGTP formMain = null;
            try { formMain = new FormMainTransGTP(); }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, "!Ошибка! запуска приложения.", Logging.INDEX_MESSAGE.NOT_SET);
            }
            try
            {
                if (!(formMain == null))
                    Application.Run(formMain);
                else
                    ;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, "Ошибка выполнения приложения.", Logging.INDEX_MESSAGE.NOT_SET);
            }


            ProgramBase.Exit();
        }
    }
}
