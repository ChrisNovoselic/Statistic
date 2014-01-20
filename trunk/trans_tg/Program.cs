using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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
            try { formMain = new FormMainTransTG(); }
            catch (Exception e)
            {
                Logging.Logg().LogExceptionToFile(e, "Ошибка запуска приложения.");
            }

            if (!(formMain == null))
                Application.Run(formMain);
            else
                ;

            ProgramBase.Exit();
        }
    }
}
