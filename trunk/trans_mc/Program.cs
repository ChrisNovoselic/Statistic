using System;
using System.Collections.Generic;
using System.Windows.Forms;

using StatisticCommon;

namespace trans_mc
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

            FormMainTransMC formMain = null;
            try { formMain = new FormMainTransMC(); }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, "Ошибка запуска приложения.");
            }

            if (!(formMain == null))
                Application.Run(formMain);
            else
                ;

            ProgramBase.Exit();

            DbSources.Sources().UnRegister();
        }
    }
}
