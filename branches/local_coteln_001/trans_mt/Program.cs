﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using HClassLibrary;
using StatisticCommon;
using StatisticTrans;

namespace trans_mt
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logging.s_mode = Logging.LOG_MODE.FILE_EXE;

            ProgramBase.Start();

            if (RunOneInstance.ChekRunProgramm("trans_mt")) return;

            FormMainTransMT formMain = null;
            try { formMain = new FormMainTransMT(); }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, "!Ошибка! запуска приложения.", Logging.INDEX_MESSAGE.NOT_SET);
            }

            if (!(formMain == null))
                Application.Run(formMain);
            else
                ;

            ProgramBase.Exit();
        }
    }
}
