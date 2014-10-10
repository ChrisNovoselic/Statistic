﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

using System.Threading;

namespace StatisticCommon
{
    public delegate void DelegateFunc();
    public delegate void DelegateIntFunc(int param);
    public delegate void DelegateIntIntFunc(int param1, int param2);
    public delegate void DelegateStringFunc(string param);
    public delegate void DelegateBoolFunc(bool param);
    public delegate void DelegateObjectFunc(object obj);
    public delegate void DelegateRefObjectFunc(ref object obj);

    public class HException : Exception
    {
        public int m_code;

        public HException(int code, string msg)
            : base(msg)
        {
            m_code = code;
        }
    }

    public static class ProgramBase
    {
        public enum ID_APP { STATISTIC = 1, TRANS_GTP, TRANS_GTP_TO_NE22, TRANS_GTP_FROM_NE22, TRANS_BYISK_GTP_TO_NE22, TRANS_MODES_CENTRE, TRANS_MODES_CENTRE_GUI, TRANS_MODES_CENTRE_CMD, TRANS_MODES_TERMINALE, TRANS_TG }
        
        public static string MessageWellcome = "***************Старт приложения...***************"
            , MessageExit = "***************Выход из приложения...***************"
            , MessageAppAbort = @"Приложение будет закрыто...";

        //Журналирование старта приложения
        public static void Start()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            s_iMessageShowUnhandledException = 0;

            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            //Logging.s_mode = Logging.LOG_MODE.UNKNOWN; //Если назначить неизвестный тип логирования - 1-е сообщения б. утеряны
            Logging.s_mode = Logging.LOG_MODE.DB;
            //Logging.s_mode = Logging.LOG_MODE.FILE;
            Logging.Logg().Post(Logging.ID_MESSAGE.START, MessageWellcome, true, true, true);
        }

        //Журналирование завершения приложения
        public static void Exit()
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is FormMainBase)
                    (f as FormMainBase).Close(true);
                else
                    f.Close();
            }

            Logging.Logg().Post(Logging.ID_MESSAGE.STOP, MessageExit, true, true, true);
            Logging.Logg().Stop ();

            DbSources.Sources().UnRegister();

            System.ComponentModel.CancelEventArgs cancelEvtArgs = new System.ComponentModel.CancelEventArgs (true);
            Application.Exit(cancelEvtArgs);
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            string strHeader = "ProgramBase::Application_ThreadException () - ...";
            if (s_iMessageShowUnhandledException > 0) MessageBox.Show((IWin32Window)null, e.Exception.Message + Environment.NewLine + MessageAppAbort, strHeader); else ;

            // here you can log the exception ...
            Logging.Logg().Exception(e.Exception, strHeader);

            Exit ();            
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string strHeader = "ProgramBase::CurrentDomain_UnhandledException () - ...";
            if (s_iMessageShowUnhandledException > 0)
                MessageBox.Show((IWin32Window)null, (e.ExceptionObject as Exception).Message + Environment.NewLine + MessageAppAbort, strHeader);
            else ;

            // here you can log the exception ...
            Logging.Logg().Exception(e.ExceptionObject as Exception, strHeader);

            Exit ();
        }

        //???
        public static void Abort() { }

        public static int s_iAppID = -1;
        public static int s_iMessageShowUnhandledException = -1;

        public static string AppName
        {
            get
            {
                return Logging.AppName + ".exe";
            }
        }
    }
}