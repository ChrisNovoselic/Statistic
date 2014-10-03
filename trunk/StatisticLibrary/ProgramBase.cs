using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace StatisticCommon
{
    public delegate void DelegateFunc();
    public delegate void DelegateIntFunc(int param);
    public delegate void DelegateIntIntFunc(int param1, int param2);
    public delegate void DelegateStringFunc(string param);
    public delegate void DelegateBoolFunc(bool param);
    public delegate void DelegateObjectFunc(object obj);
    public delegate void DelegateRefObjectFunc(ref object obj);

    public static class ProgramBase
    {
        public static string MessageWellcome = "***************Старт приложения...***************";
        public static string MessageExit = "***************Выход из приложения...***************";

        //Журналирование старта приложения
        public static void Start()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Logging.s_mode = Logging.LOG_MODE.UNKNOWN; //Если назначить неизвестный тип логирования - 1-е сообщения б. утеряны
            //Logging.s_mode = Logging.LOG_MODE.DB;
            Logging.s_mode = Logging.LOG_MODE.FILE;
            Logging.Logg().Post(Logging.ID_MESSAGE.START, MessageWellcome, true, true, true);
        }

        //Журналирование завершения приложения
        public static void Exit()
        {
            Logging.Logg().Post(Logging.ID_MESSAGE.STOP, MessageExit, true, true, true);
        }

        //???
        public static void Abort() { }

        public static string AppName
        {
            get
            {
                return Logging.AppName + ".exe";
            }
        }
    }
}