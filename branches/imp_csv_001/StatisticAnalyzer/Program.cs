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
            FormConnectionSettings formConnSett = new FormConnectionSettings(fileConnSett.ReadSettingsFile, fileConnSett.SaveSettingsFile);

            Application.Run(new StatisticCommon.FormMainAnalyzer(formConnSett.getConnSett(), new InitTEC(formConnSett.getConnSett(), true, false).tec));

            ProgramBase.Exit ();
        }
    }
}
