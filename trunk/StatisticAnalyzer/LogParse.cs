using ASUTP.Core;
using ASUTP.Helper;
using System;
using System.Collections.Generic;
using System.Data;
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

            public DelegateFunc Exit;

            public LogParse ()
            {
                m_semAllowed = new Semaphore (1, 1);

                m_tableLog = new DataTable ("ContentLog");
                DataColumn [] cols = new DataColumn [] {
                        new DataColumn("DATE_TIME", typeof(DateTime)),
                        new DataColumn("TYPE", typeof(Int32)),
                        new DataColumn ("MESSAGE", typeof (string))
                    };
                m_tableLog.Columns.AddRange (cols);

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

                m_tableLog.Clear ();

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
                        if (joined == false)
                            m_thread.Abort ();
                        else
                            ;
                    } else
                        ;

                    try {
                        m_semAllowed.Release ();
                    } catch (Exception e) {
                        Console.WriteLine ("LogParse::Stop () - m_semAllowed.Release() - поток не был запущен или штатно завршился");
                    }
                } else
                    ;
            }

            /// <summary>
            /// Метод для разбора лог-сообщений в потоке
            /// </summary>
            protected virtual void thread_Proc (object data)
            {
                Console.WriteLine ("Окончание обработки лог-файла. Обработано строк: {0}", (int)data);

                Exit ();
            }

            /// <summary>
            /// Очистка
            /// </summary>
            public void Clear ()
            {//TODO:
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
}
