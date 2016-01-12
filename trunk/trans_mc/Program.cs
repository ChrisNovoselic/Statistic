using System;
using System.Collections.Generic;
using System.Windows.Forms;

using HClassLibrary;
using StatisticCommon;
using StatisticTrans;

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
            //Logging.s_mode = Logging.LOG_MODE.UNKNOWN; //Если назначить неизвестный тип логирования - 1-е сообщения б. утеряны
            //Logging.s_mode = Logging.LOG_MODE.DB;
            Logging.s_mode = Logging.LOG_MODE.FILE_EXE;

            if (RunOneInstance.ChekRunProgramm("trans_mc"))
            {
                return;
            }

            ProgramBase.Start();

            FormMainTransMC formMain = null;

            if (formMain == null)
            {
                try { formMain = new FormMainTransMC(); }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, "Ошибка запуска приложения.", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            if (!(formMain == null))
                Application.Run(formMain);

            ProgramBase.Exit();
        }
    }
}
