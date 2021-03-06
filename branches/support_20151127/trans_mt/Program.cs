﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using HClassLibrary;
using StatisticCommon;

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
            ProgramBase.Start();

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
