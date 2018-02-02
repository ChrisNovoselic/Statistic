﻿using ASUTP.Core;
using ASUTP.Database;
using ASUTP.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace StatisticAnalyzer
{
    partial class PanelAnalyzer
    {
        /// <summary>
        /// Класс для работы со строкой лог сообщений
        /// </summary>
        protected abstract class LogParse
        {
            public const int INDEX_START_MESSAGE = 0;

            public struct PARAM_THREAD_PROC
            {
                public LogParse.MODE Mode;

                public string Delimeter;

                public DataTable Table;
            }

            public enum MODE { UNKNOWN = -1, MESSAGE, LIST_DATE, COUNTER }

            protected List<DateTime> m_listDate;

            public List<DateTime> ListDate
            {
                get
                {
                    return m_listDate;
                }
            }

            /// <summary>
            /// Массив с идентификаторами типов сообщений
            /// </summary>
            public static int [] s_ID_LOGMESSAGES;

            /// <summary>
            /// Список типов сообщений
            /// </summary>
            public static string [] s_DESC_LOGMESSAGE;

            private Thread m_thread;
            protected DataTable m_tableLog;
            Semaphore m_semAllowed;

            public Action<PARAM_THREAD_PROC> Exit;

            public LogParse ()
            {
                m_semAllowed = new Semaphore (1, 1);

                m_tableLog = new DataTable ("LogParse");
                DataColumn [] cols = new DataColumn [] {
                        new DataColumn("DATE_TIME", typeof(DateTime)),
                        new DataColumn("TYPE", typeof(Int32)),
                        new DataColumn ("COUNT_MESSAGE", typeof (string))
                    };
                m_tableLog.Columns.AddRange (cols);

                m_listDate = new List<DateTime> ();

                m_thread = null;
            }

            /// <summary>
            /// Инициализация потока разбора лог-файла
            /// </summary>
            public void Start (object param)
            {
                m_semAllowed.WaitOne ();

                m_thread = new Thread (new ParameterizedThreadStart (thread_Proc));
                m_thread.IsBackground = true;
                m_thread.Name = "Разбор лог-файла";

                m_thread.CurrentCulture =
                m_thread.CurrentUICulture =
                    ProgramBase.ss_MainCultureInfo;

                switch (((PARAM_THREAD_PROC)param).Mode) {
                    case MODE.LIST_DATE:
                        m_listDate.Clear();
                        break;
                    case MODE.MESSAGE:
                        m_tableLog.Clear ();
                        break;
                    case MODE.COUNTER:
                        break;
                    default:
                        break;
                }

                m_thread.Start (param);
            }

            /// <summary>
            /// Остановка потока разбора лог-файла
            /// </summary>
            public void Stop ()
            {
                //if (m_bAllowed == false)
                //    return;
                //else
                //    ;

                bool joined = false;
                if ((!(m_thread == null))) {
                    if (m_thread.IsAlive == true) {
                        //m_bAllowed = false;
                        joined = m_thread.Join (6666);
                        if (joined == false) {
                            m_thread.Abort ();

                            try {
                                m_semAllowed.Release();
                            } catch (Exception e) {
                                Console.WriteLine("LogParse::Stop () - m_semAllowed.Release() - поток не был запущен или штатно завершился");
                            }
                        } else
                            ;
                    } else
                        ;
                } else
                    ;
            }

            /// <summary>
            /// Метод для разбора лог-сообщений в потоке
            /// </summary>
            protected virtual void thread_Proc (object data)
            {
                string message = string.Empty;

                switch (((PARAM_THREAD_PROC)data).Mode) {
                    case MODE.LIST_DATE:
                        message = string.Format("Окончание разбора сообщений. Обработано дат: {0}", m_listDate.Count);
                        break;
                    case MODE.MESSAGE:
                        message = string.Format ("Окончание разбора сообщений. Принято строк: {0}", m_tableLog.Rows.Count);
                        break;
                    default:
                        throw new InvalidOperationException ($"PanelAnalyzer.LogParse::thread_Proc () - неизвестный режим <{ ((PARAM_THREAD_PROC)data).Mode.ToString () } >...");
                        break;
                }

                Console.WriteLine (message);

                try {
                    m_semAllowed.Release ();
                } catch (Exception e) {
                    Console.WriteLine ("LogParse::thread_Proc () - m_semAllowed.Release() - поток не был запущен или штатно завершился");
                }

                Exit ((PARAM_THREAD_PROC)data);
            }

            /// <summary>
            /// Добавление типов сообщений
            /// </summary>
            /// <param name="nameFieldTypeMessage">Наименование колонки типов сообщений</param>
            /// <param name="strIndxType">Стартовы индекс</param>
            /// <returns>Строка для условия выборки сообщений по их типам</returns>
            protected string addingWhereTypeMessage (string nameFieldTypeMessage, string strIndxType)
            {
                string strRes = string.Empty;

                //if (!(indxType < 0))
                if (strIndxType.Equals (string.Empty) == false) {
                    List<int> listIndxType;
                    //listIndxType = strIndxType.Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.ROW] }, StringSplitOptions.None)[1].Split(new string[] { m_chDelimeters[(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None).ToList().ConvertAll<int>(new Converter<string, int>(delegate(string strIn) { return Int32.Parse(strIn); }));
                    string [] pars = strIndxType.Split (new string [] { s_chDelimeters [(int)INDEX_DELIMETER.ROW] }, StringSplitOptions.None);

                    if (pars.Length == 2) {
                        bool bUse = false;
                        //if ((pars[0].Equals(true.ToString()) == true) || (pars[0].Equals(false.ToString()) == true))
                        if (bool.TryParse (pars [0], out bUse) == true) {
                            if (pars [1].Length > 0) {
                                listIndxType = pars [1].Split (new string [] { s_chDelimeters [(int)INDEX_DELIMETER.PART] }, StringSplitOptions.None).ToList ().ConvertAll<int> (new Converter<string, int> (delegate (string strIn) {
                                    return Int32.Parse (strIn);
                                }));

                                strRes += nameFieldTypeMessage + @" ";

                                if (bUse == false)
                                    strRes += @"NOT ";
                                else
                                    ;

                                strRes += @"IN (";

                                foreach (int indx in listIndxType)
                                    strRes += s_ID_LOGMESSAGES [indx] + @",";

                                strRes = strRes.Substring (0, strRes.Length - 1);

                                strRes += @")";
                            } else
                                ;
                        } else
                            ;
                    } else
                        ;
                } else
                    ;

                return strRes;
            }

            /// <summary>
            /// Сортировка выборки по дате
            /// </summary>
            public DataRow [] Sort (string nameField)
            {
                return m_tableLog.Select (string.Empty, nameField);
            }

            //public int Select(int indxType, DateTime beg, DateTime end, ref DataRow[] res)
            /// <summary>
            /// Выборка лог-сообщений
            /// </summary>
            /// <param name="strIndxType">Индекс типа сообщения</param>
            /// <param name="beg">Начало периода</param>
            /// <param name="end">Оончание периода</param>
            /// <returns>Массив строк с сообщениями за период</returns>
            public DataRow [] ByDate (string strIndxType, DateTime beg, DateTime end)
            {
                string where = string.Empty;

                //m_tableLog.Clear();

                if (beg.Equals (DateTime.MaxValue) == false) {
                    where = $"DATE_TIME>='{beg.ToString ("yyyyMMdd HH:mm:ss")}'";
                    if (end.Equals (DateTime.MaxValue) == false)
                        where += $" AND DATE_TIME<'{end.ToString ("yyyyMMdd HH:mm:ss")}'";
                    else
                        ;
                } else
                    ;

                if (where.Equals (string.Empty) == false)
                    where += @" AND ";
                else
                    ;
                where += addingWhereTypeMessage (@"TYPE", strIndxType);

                return
                    //(from DataRow row in m_tableLog.Rows where ((DateTime)row ["DATE_TIME"]) >= beg && ((DateTime)row ["DATE_TIME"]) < end select row).ToArray()
                    m_tableLog.Select (where);
                ;
            }
        }
    }

    partial class PanelAnalyzer_DB
    {
        /// <summary>
        /// Класс для получения лог сообщений из БД
        /// </summary>
        private class LogParse_DB : LogParse {
            /// <summary>
            /// Список идентификаторов типов сообщений
            /// </summary>
            public enum INDEX_LOGMSG {
                START = LogParse.INDEX_START_MESSAGE, STOP
                , ACTION, DEBUG, EXCEPTION
                , EXCEPTION_DB
                , ERROR
                , WARNING
                , UNKNOWN
                    , COUNT_TYPE_LOGMESSAGE
            };

            public List<int> Counter;

            public LogParse_DB ()
                : base ()
            {
                s_DESC_LOGMESSAGE = new string [] { "Запуск", "Выход",
                                                    "Действие", "Отладка", "Исключение",
                                                    "Исключение БД",
                                                    "Ошибка",
                                                    "Предупреждение",
                                                    "Неопределенный тип" };

                //ID типов сообщений
                s_ID_LOGMESSAGES = new int [] {
                    1, 2, 3, 4, 5, 6, 7, 8, 9
                        , 10
                };


            }

            /// <summary>
            /// Потоковый метод опроса БД для выборки лог-сообщений
            /// </summary>
            /// <param name="data">Объект с данными для разборки методом</param>
            protected override void thread_Proc (object data)
            {
                DataTable tableReceived;
                DateTime dateStart;
                MODE mode = MODE.UNKNOWN;

                tableReceived = ((PARAM_THREAD_PROC)data).Table;
                mode = ((PARAM_THREAD_PROC)data).Mode;

                switch (mode) {
                    case MODE.LIST_DATE:
                    //перебор значений и добовление их в строку
                        for (int i = 0; i < tableReceived.Rows.Count; i++) {
                            dateStart = (DateTime)tableReceived.Rows [i] ["DATE_TIME"];
                            //m_tableLog.Rows.Add (new object [] { dateStart, s_ID_LOGMESSAGES [(int)INDEX_LOGMSG.START], ProgramBase.MessageWellcome });
                            m_listDate.Add (dateStart);
                        }
                        break;
                    case MODE.MESSAGE:
                        m_tableLog = tableReceived.Copy();
                        break;
                    default:
                        throw new InvalidOperationException ($"PanelAnalyzer_DB.LogParse_DB::thread_Proc () - неизвестный режим <{mode.ToString()}>...");
                        break;
                }
                
                //запрос разбора строки с лог-сообщениями
                base.thread_Proc (data);
            }
        }
    }

    partial class PanelAnalyzer_TCPIP
    {
        class LogParse_File : LogParse
        {
            public enum TYPE_LOGMESSAGE
            {
                START, STOP,
                DBOPEN, DBCLOSE, DBEXCEPTION,
                ERROR, DEBUG,
                DETAIL,
                SEPARATOR, SEPARATOR_DATETIME,
                UNKNOWN, COUNT_TYPE_LOGMESSAGE
            };

            public static string[] DESC_LOGMESSAGE = { "Запуск", "Выход",
                "БД открыть", "БД закрыть", "БД исключение",
                "Ошибка", "Отладка",
                "Детализация",
                "Раздел./сообщ.", "Раздел./дата/время",
                "Неопределенный тип" };

            protected string[] SIGNATURE_LOGMESSAGE = { ProgramBase.MessageWellcome, ProgramBase.MessageExit,
                DbTSQLInterface.MessageDbOpen, DbTSQLInterface.MessageDbClose, DbTSQLInterface.MessageDbException,
                "!Ошибка!", "!Отладка!",
                string.Empty,
                ASUTP.Logging.MessageSeparator, ASUTP.Logging.DatetimeStampSeparator,
                string.Empty
                , "Неопределенный тип" };

            public LogParse_File()
            {
                s_ID_LOGMESSAGES = new int[] {
                        0, 1
                        , 2, 3, 4
                        , 5, 6
                        , 7
                        , 8, 9
                        , 10
                        , 11
                    };
            }

            protected override void thread_Proc(object text)
            {
                string line;
                TYPE_LOGMESSAGE typeMsg;
                DateTime dtMsg;
                bool bValid = false;

                line = string.Empty;

                typeMsg = TYPE_LOGMESSAGE.UNKNOWN;

                StringReader content = new StringReader(text as string);

                Console.WriteLine("Начало обработки лог-файла. Размер: {0}", (text as string).Length);

                //Чтение 1-ой строки лог-файла
                line = content.ReadLine();

                while (!(line == null))
                {
                    //Проверка на разделитель сообщение-сообщение
                    if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.SEPARATOR]) == true)
                    {
                        //Следующая строка - дата/время
                        line = content.ReadLine();

                        //Попытка разбора дата/время
                        bValid = DateTime.TryParse(line, out dtMsg);
                        if (bValid == true)
                        {
                            //Следующая строка - разделитель дата/время-сообщение
                            content.ReadLine();
                            //Следующая строка - сообщение
                            line = content.ReadLine();
                        }
                        else
                        {
                            //Установка дата/время предыдущего сообщения
                            if (m_tableLog.Rows.Count > 0)
                                dtMsg = (DateTime)m_tableLog.Rows[m_tableLog.Rows.Count - 1]["DATE_TIME"];
                            else
                                ; //dtMsg = new DateTime(1666, 06, 06, 06, 06, 06);
                        }
                    }
                    else
                    {
                        //Попытка разбора дата/время
                        bValid = DateTime.TryParse(line, out dtMsg);
                        if (bValid == true)
                        {
                            //Следующая строка - разделитель дата/время-сообщение
                            content.ReadLine();
                            //Следующая строка - сообщение
                            line = content.ReadLine();
                        }
                        else
                        {
                            //Установка дата/время предыдущего сообщения
                            //dtMsg = new DateTime(1666, 06, 06, 06, 06, 06);
                            dtMsg = (DateTime)m_tableLog.Rows[m_tableLog.Rows.Count - 1]["DATE_TIME"];

                            if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.SEPARATOR_DATETIME]) == true)
                            {
                                //Следующая строка - сообщение
                                line = content.ReadLine();
                            }
                            else
                            {
                                //Строка содержит сообщение/детализация сообщения
                            }
                        }
                    }

                    if (line == null)
                    {
                        break;
                    }
                    else
                        ;

                    //Проверка на разделитель сообщение-сообщение
                    if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.SEPARATOR]) == true)
                    {
                        continue;
                    }
                    else
                    {
                        if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.START]) == true)
                        {
                            typeMsg = TYPE_LOGMESSAGE.START;
                        }
                        else
                            if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.STOP]) == true)
                            {
                                typeMsg = TYPE_LOGMESSAGE.STOP;
                            }
                            else
                                if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.DBOPEN]) == true)
                                {
                                    typeMsg = TYPE_LOGMESSAGE.DBOPEN;
                                }
                                else
                                    if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.DBCLOSE]) == true)
                                    {
                                        typeMsg = TYPE_LOGMESSAGE.DBCLOSE;
                                    }
                                    else
                                        if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.DBEXCEPTION]) == true)
                                        {
                                            typeMsg = TYPE_LOGMESSAGE.DBEXCEPTION;
                                        }
                                        else
                                            if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.ERROR]) == true)
                                            {
                                                typeMsg = TYPE_LOGMESSAGE.ERROR;
                                            }
                                            else
                                                if (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.DEBUG]) == true)
                                                {
                                                    typeMsg = TYPE_LOGMESSAGE.DEBUG;
                                                }
                                                else
                                                    typeMsg = TYPE_LOGMESSAGE.DETAIL;

                        //Добавление строки в таблицу с собщениями
                        m_tableLog.Rows.Add(new object[] { dtMsg, (int)typeMsg, line });

                        //Чтение строки сообщения
                        line = content.ReadLine();

                        while ((!(line == null)) && (line.Contains(SIGNATURE_LOGMESSAGE[(int)TYPE_LOGMESSAGE.SEPARATOR]) == false))
                        {
                            //Добавление строки в таблицу с собщениями
                            m_tableLog.Rows.Add(new object[] { dtMsg, (int)TYPE_LOGMESSAGE.DETAIL, line });

                            //Чтение строки сообщения
                            line = content.ReadLine();
                        }
                    }
                }

                base.thread_Proc(m_tableLog.Rows.Count);
            }
        }
    }
}
