using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using HClassLibrary;
using StatisticCommon;

namespace StatisticAnalyzer
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

            FIleConnSett fileConnSett = new FIleConnSett ("connsett.ini", FIleConnSett.MODE.FILE);
            FormConnectionSettings formConnSett = new FormConnectionSettings(-1, fileConnSett.ReadSettingsFile, fileConnSett.SaveSettingsFile);

            int idListener = DbSources.Sources().Register(formConnSett.getConnSett(), false, @"CONFIG_DB");
            Application.Run(new StatisticCommon.FormMainAnalyzer(idListener, new InitTEC_200(idListener, true, false).tec));
            DbSources.Sources().UnRegister(idListener);

            ProgramBase.Exit ();
        }
    }
}
