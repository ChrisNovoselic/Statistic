using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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

            FIleConnSett fileConnSett = new FIleConnSett ("connsett.ini");
            FormConnectionSettings formConnSett = new FormConnectionSettings(-1, fileConnSett.ReadSettingsFile, fileConnSett.SaveSettingsFile);

            int idListener = DbSources.Sources ().Register (formConnSett.getConnSett(), false);
            Application.Run(new StatisticCommon.FormMainAnalyzer(idListener, new InitTEC(idListener, true, false).tec));
            DbSources.Sources().UnRegister(idListener);

            ProgramBase.Exit ();
        }
    }
}
