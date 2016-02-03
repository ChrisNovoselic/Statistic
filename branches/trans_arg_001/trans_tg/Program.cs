﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using HClassLibrary;
using StatisticCommon;

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
            ProgramBase.Start();

            FormMainTransTG formMain = null;
            if (FormMainStatistic.SingleInstance.Start())
            {
            }
            else
            {
                if (FormMainStatistic.SingleInstance.stopbflg)
                {
                    try { formMain = new FormMainTransTG(); }
                    catch (Exception e)
                    {
                        Logging.Logg().Exception(e, "!Ошибка! запуска приложения.", Logging.INDEX_MESSAGE.NOT_SET);
                    }

                    if (!(formMain == null))
                        Application.Run(formMain);
                    else
                        ;
                }

                FormMainStatistic.SingleInstance.StopMtx();
            }

            ProgramBase.Exit();
        }
    }
}
