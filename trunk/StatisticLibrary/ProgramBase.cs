using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace StatisticCommon
{
    public static class ProgramBase
    {
        public static void Start()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Logging.Logg().LogLock();
            Logging.Logg().LogToFile("=============Старт приложения...=============", true, true, false);
            Logging.Logg().LogUnlock();
        }

        public static void Exit()
        {
            Logging.Logg().LogLock();
            Logging.Logg().LogToFile("=============Выход из приложения...=============", true, true, false);
            Logging.Logg().LogUnlock();
        }

        public static void Abort()
        {
        }

        public static string AppName
        {
            get
            {
                string strRes = System.Environment.CommandLine.Substring(System.Environment.CommandLine.LastIndexOf("\\") + 1).Replace("\"", "").Trim();
                if (!(strRes.IndexOf('.') == strRes.LastIndexOf('.')))
                {
                    char delim = '.';
                    List<int> listPosDelim = new List<int>(); ;
                    int pos = strRes.IndexOf(delim, 0),
                        indxPos = -1;

                    while (!(pos < 0))
                    {
                        listPosDelim.Add(pos);
                        pos = strRes.IndexOf(delim, pos + 1);
                    }

                    if (listPosDelim.Count > 1)
                    {
                        strRes = strRes.Substring(0, listPosDelim[listPosDelim.Count - 2]) + strRes.Substring(listPosDelim[listPosDelim.Count - 1], strRes.Length - listPosDelim[listPosDelim.Count - 1]);
                    }
                    else
                        ;
                }
                else
                    ;

                return strRes;
            }
        }
    }
}