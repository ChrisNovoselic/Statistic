﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Security;
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

            FormMainTransMC formMain = null;

            if (SingleInstance.Start())
            {
                //SingleInstance.restartInstance();
            }   
            else
            {
                if (SingleInstance.stopbflg)
                {
                    ProgramBase.Start();

                    try
                    { formMain = new FormMainTransMC(); }
                    catch (Exception e)
                    { Logging.Logg().Exception(e, "Ошибка запуска приложения.", Logging.INDEX_MESSAGE.NOT_SET); }

                    MessageBox.Show("PERVZAPUSK");
                    try
                    { Application.Run(formMain); }
                    catch (Exception e)
                    { Logging.Logg().Exception(e, "Ошибка выполнения приложения.", Logging.INDEX_MESSAGE.NOT_SET); }

                    SingleInstance.StopMtx();

                    ProgramBase.Exit();
                }
                else ;
            }
        }
    }
}

