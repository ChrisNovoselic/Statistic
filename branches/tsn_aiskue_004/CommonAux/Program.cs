﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using HClassLibrary;
using StatisticCommon;

namespace CommonAux
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

            FIleConnSett fileConnSett = new FIleConnSett("connsett.ini", FIleConnSett.MODE.FILE);
            FormConnectionSettings formConnSett = new FormConnectionSettings(-1, fileConnSett.ReadSettingsFile, fileConnSett.SaveSettingsFile);

            int idListener = DbSources.Sources().Register(formConnSett.getConnSett(), false, @"CONFIG_DB");

            //PanelCommonAux formMain = null;

            //try { formMain = new PanelCommonAux(idListener); }
            //catch (Exception e)
            //{
            //    Logging.Logg().Exception(e, "!Ошибка! запуска приложения.", Logging.INDEX_MESSAGE.NOT_SET);
            //}

            Application.Run(new FormMain(idListener, new InitTEC_200(idListener, true, new int[] { 0, (int)TECComponent.ID.GTP }, false).tec));
            ProgramBase.Exit();
        }
    }
}
