using ASUTP;
using ASUTP.Core;
using ASUTP.Database;
using ASUTP.Helper;
using StatisticCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;

namespace StatisticAnalyzer
{
    public partial class PanelAnalyzer_DB : PanelAnalyzer
    {
        public PanelAnalyzer_DB (List<StatisticCommon.TEC> tec, Color foreColor, Color backColor)
            : base(tec, foreColor, backColor)
        {
        }

        #region Наследуемые методы

        /// <summary>
        /// Метод для получения лога
        /// </summary>
        /// <returns>Возвращает лог сообщений</returns>
        protected override LogCounter newLogCounter() { return new LogCounter_DB(); }

        /// <summary>
        /// Запуск таймера обновления данных
        /// </summary>
        public override void Start()
        {
            int err = -1, 
                idMainDB = -1;

            DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings ();
            DbTSQLConfigDatabase.DbConfig().Register();

            if (DbTSQLConfigDatabase.DbConfig ().ListenerId > 0) {
                idMainDB = Int32.Parse (DbTSQLConfigDatabase.DbConfig().Select (@"SELECT [VALUE] FROM [setup] WHERE [KEY]='" + @"Main DataSource" + @"'", null, null, out err).Rows [0] [@"VALUE"].ToString ());
                DataTable tblConnSettMainDB = ConnectionSettingsSource.GetConnectionSettings (DbTSQLConfigDatabase.DbConfig ().ListenerId, idMainDB, -1, out err);

                if ((tblConnSettMainDB.Columns.Count > 0)
                    && (tblConnSettMainDB.Rows.Count > 0)) {
                // "-1" - значит будет назначени идентификатор из БД
                    LoggingReadHandler.SetConnectionSettings(new ConnectionSettings (tblConnSettMainDB.Rows [0], -1));
                    LoggingReadHandler.SetDelegateReport(delegateErrorReport, delegateWarningReport, delegateActionReport, delegateReportClear);
                    LoggingReadHandler.SetDelegateWait(delegateStartWait, delegateStopWait, delegate () { /* ничего не делаем */ });
                    LoggingReadHandler.StartDbInterfaces();
                    LoggingReadHandler.Start ();

                    //!!! только после 'старта' handler-а
                    base.Start ();
                } else
                    throw new Exception ($"PanelAnalyzer_DB::Start () - нет параметров соединения с БД значений ID={idMainDB}...");
            } else
                throw new Exception (@"PanelAnalyzer_DB::Start () - нет соединения с БД конфигурации...");

            DbTSQLConfigDatabase.DbConfig ().UnRegister ();
        }

        protected override void handlerCommandCounterToTypeByFilter (PanelAnalyzer.REQUEST req, DataTable tableRes)
        {
            m_dictDataGridViewLogCounter [(DATAGRIDVIEW_LOGCOUNTER)req.Args [0]].Fill (tableRes);

            delegateReportClear?.Invoke (true);
        }

        protected override void handlerCommandListMessageToUserByDate (PanelAnalyzer.REQUEST req, DataTable tableLogging)
        {
            if (req.State == PanelAnalyzer.REQUEST.STATE.Ok) {
                filldgvLogMessages(tableLogging.Select(string.Empty, @"DATE_TIME") as DataRow[]);
            } else
                ;
        }

        protected override void handlerCommandListDateByUser (PanelAnalyzer.REQUEST req, DataTable tableRes)
        {
            m_LogCounter.Start(new PARAM_THREAD_PROC {
                Delimeter = s_chDelimeters[(int)INDEX_DELIMETER.PART]
                , TableLogging = tableRes
            });
        }

        /// <summary>
        /// Метод для получения из БД списка активных вкладок для выбранного пользователя
        /// </summary>
        /// <param name="user">Пользователь для выборки</param>
        protected override IEnumerable<int> getTabActived(int user)
        {
            List<int> listRes = new List<int> ();

            int err = -1;
            int iRes = -1;
            string[] idTabs ;
            string query = string.Empty
                , where = string.Empty
                , tabs = string.Empty
                , role0 = string.Empty, role1 = string.Empty;
            DataTable tableProfileTabs;

            delegateActionReport ("Получение активных вкладок пользователя");

            #region Фомирование и выполнение запроса для получения списка открытых вкладок у пользователя

            if (Equals(user, null) == false) {
                query = @"SELECT [ID_EXT],[IS_ROLE],[ID_UNIT],[VALUE] FROM [dbo].[profiles] WHERE ";
                //Условие для поиска вкладок
                where = $"((ID_EXT={user} AND [IS_ROLE]=0) OR ([ID_EXT]=(SELECT [ID_ROLE] FROM dbo.[users] WHERE [ID]={user}) AND [IS_ROLE]=1)) AND ID_UNIT IN({(int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE}, {(int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS})";
                query += where;

                tableProfileTabs = DbTSQLConfigDatabase.DbConfig().Select(query, out iRes);

                if (tableProfileTabs.Rows.Count > 0) {
                    //Список строк с используемыми вкладками
                    DataRow[] table_role0 = tableProfileTabs.Select($"IS_ROLE=0 AND [ID_UNIT] IN ({(int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE}, {(int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS})");

                    //Список строк с вкладками по умолчанию
                    DataRow[] table_role1 = tableProfileTabs.Select($"IS_ROLE=1 AND [ID_UNIT] IN ({(int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE}, {(int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS})");

                    //проверка выборки для используемых вкладок
                    if ((!(table_role0.Length > 2))
                        && (!(table_role0.Length < 1))) {
                        for (int i = 0; i < table_role0.Length; i++)
                        {
                            if ((int)table_role0[i]["ID_UNIT"] == (int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE)
                                if (!(IsNumberContains(table_role0[i]["VALUE"].ToString()) < 0))
                                    tabs += table_role0[i]["VALUE"].ToString();
                                else
                                    ;
                            else
                                ;

                            if ((int)table_role0[i]["ID_UNIT"] == (int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS) {
                                if (tabs.Equals(string.Empty) == false)
                                    tabs += ";";
                                else
                                    ;

                                if (!(IsNumberContains(table_role0[i]["VALUE"].ToString()) < 0))
                                    tabs += table_role0[i]["VALUE"].ToString();
                                else
                                    ;
                            } else
                                ;
                        }
                    }
                    else
                        //проверка выборки для вкладок по умолчанию
                        if ((!(table_role1.Length > 2))
                            && (!(table_role1.Length < 1))) {
                            for (int i = 0; i < table_role1.Length; i++)
                            {
                                if ((int)table_role1[i]["ID_UNIT"] == (int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE)
                                    if (!(IsNumberContains(table_role1[i]["VALUE"].ToString()) < 0))
                                        tabs += table_role1[i]["VALUE"].ToString();
                                    else
                                        ;
                                else
                                    ;

                                if ((int)table_role1[i]["ID_UNIT"] == (int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS)
                                    if (!(IsNumberContains(table_role1[i]["VALUE"].ToString()) < 0))
                                        tabs += table_role1[i]["VALUE"].ToString();
                                    else
                                        ;
                                else
                                    ;
                            }
                        }
                        else
                            Logging.Logg().Error(@"PanelAnalyzer::PanelAnalyzer_DB () - нет данных по вкладкам для выбранного пользователя с ID = " + user
                                , Logging.INDEX_MESSAGE.NOT_SET);

                    //создание массива с ID вкладок
                    idTabs = tabs.Split(';');

                    for (int i = 0; i < idTabs.Length; i++)
                        if (IsPunctuationContains(idTabs[i]) == true) {
                        //разбор в случае составного идентификатора
                            string[] count;
                            count = idTabs[i].Split('=');
                            //добавление идентификатора в список
                            listRes.Add(Convert.ToInt32(count[0]));
                        } else
                            //фильтрация пустых идентификаторов
                            if (string.IsNullOrEmpty(idTabs[i].Trim()) == false)
                                if (!(IsNumberContains(idTabs[i]) < 0))
                                //добавление идентификатора в список
                                    listRes.Add(Convert.ToInt32(idTabs[i]));
                                else
                                    ;
                            else
                                ;
                } else
                // нет записей в БД с информацией о вкладках, автоматически загружаемых при запуске для пользователя
                    ;
            } else
            // идентификатор пользователя не указан
                ;

            #endregion

            delegateReportClear (true);

            return listRes;
        }

        private HLoggingReadHandlerDb LoggingReadHandler
        {
            get
            {
                return m_loggingReadHandler as HLoggingReadHandlerDb;
            }
        }

        /// <summary>
        /// Разорвть соединение с БД
        /// </summary>
        protected override void disconnect() { }

        private struct PARAM_THREAD_PROC
        {
            public string Delimeter;

            public DataTable TableLogging;
        }

        /// <summary>
        /// Получение первой строки лог-сообщений
        /// </summary>
        /// <param name="r">Строка из таблицы</param>
        /// <returns>Вызвращает строку string</returns>
        protected override string getTabLoggingTextRow(DataRow r)
        {
            string strRes = string.Empty;

            DateTime? dtVal = null;

            if (r["DATE_TIME"] is DateTime)
            {
                dtVal = r["DATE_TIME"] as DateTime?;


                strRes = string.Join(s_chDelimeters[(int)INDEX_DELIMETER.PART].ToString()
                                    , new string[] {
                                            ((DateTime)dtVal).ToString (@"HH:mm:ss.fff")
                                            , r["TYPE"].ToString ()
                                            , r["MESSAGE"].ToString()
                                        });
            }
            else
                ;

            return strRes;
        }

        #endregion

        /// <summary>
        /// Обновление счетчика типов сообщений
        /// </summary>
        /// <param name="dgv">DataGridView в которую поместить результаты</param>
        /// <param name="start_date">Начало периода</param>
        /// <param name="end_date">Окончание периода</param>
        /// <param name="users">Список пользователей</param>
        protected override void updateCounter(DATAGRIDVIEW_LOGCOUNTER tag, DateTime start_date, DateTime end_date, string users)
        {
            LoggingReadHandler.Command (StatesMachine.CounterToTypeByFilter, new object [] { tag, start_date, end_date, users }, false);
        }

        /// <summary>
        /// Наличие пунктуации в строке
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns>true-если есть</returns>
        static bool IsPunctuationContains(string input)
        {
            bool bRes = false;

            foreach (char c in input)
                if (Char.IsPunctuation(c))
                    bRes = true;

            return bRes;
        }

        /// <summary>
        /// Наличие цифр в строке
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns>-1 - нет, 0 - все цифры, 1 - есть, но не все</returns>
        static int IsNumberContains(string input)
        {
            int iRes = -1;

            int iInput = -1;

            if (int.TryParse (input, out iInput) == true)
                iRes = 0;
            else
                foreach (char c in input)
                    if (Char.IsNumber (c) == true) {
                        iRes = 1;

                        break;
                    } else
                        ;

            return iRes;
        }

        #region Обработчики событий

        public override void LogCounter_ThreadExit ()
        {
            base.LogCounter_ThreadExit ();
        }

        protected override bool [] procChecked ()
        {
            LoggingReadHandler.Command (StatesMachine.ProcCheckedFilter);
            // фиктивный результат для совместимости с синхронным вариантом
            return new bool [] { };
        }

        protected override ILoggingReadHandler newLoggingRead ()
        {
            return new HLoggingReadHandlerDb();
        }

        /// <summary>
        /// Обновление статистики сообщений из лога за выбранный период
        /// </summary>
        protected override void dgvUserView_SelectionChanged (object sender, EventArgs e)
        {
            bool bUpdate = false;

            base.dgvUserView_SelectionChanged (sender, e);

            bUpdate = !(IdCurrentUserView < 0);

            if (bUpdate == true) {
                //Очистить элементы управления с данными от пред. лог-файла
                if (IsHandleCreated == true)
                    if (InvokeRequired == true) {
                        BeginInvoke (new Action (clearListDateView));
                        BeginInvoke (new Action<bool> (clearMessageView), true);
                    } else {
                        clearListDateView ();
                        clearMessageView (true);
                    } else
                    Logging.Logg ().Error (@"FormMainAnalyzer::dgvUserStatistic_SelectionChanged () - ... BeginInvoke (TabLoggingClearDatetimeStart, TabLoggingClearText) - ...", Logging.INDEX_MESSAGE.D_001);

                startLogParse (IdCurrentUserView.ToString());

                DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings ();
                DbTSQLConfigDatabase.DbConfig ().Register ();
                //Заполнение ListBox'а активными вкладками
                fillListBoxTabVisible (
                    getTabActived (IdCurrentUserView).ToList ()
                );

                DbTSQLConfigDatabase.DbConfig ().UnRegister ();
            } else
                ;
        }

        #endregion

        /// <summary>
        /// Класс для получения лог сообщений из БД
        /// </summary>
        private class LogCounter_DB : LogCounter
        {
            /// <summary>
            /// Список идентификаторов типов сообщений
            /// </summary>
            public enum INDEX_LOGMSG { START = LogCounter.INDEX_START_MESSAGE, STOP
                , ACTION, DEBUG, EXCEPTION
                , EXCEPTION_DB
                , ERROR
                , WARNING
                , UNKNOWN
                    , COUNT_TYPE_LOGMESSAGE
            };

            public LogCounter_DB()
                : base()
            {
                s_DESC_LOGMESSAGE = new string[] { "Запуск", "Выход",
                                                    "Действие", "Отладка", "Исключение",
                                                    "Исключение БД",
                                                    "Ошибка",
                                                    "Предупреждение",
                                                    "Неопределенный тип" };

                //ID типов сообщений
                s_ID_LOGMESSAGES = new int[] {
                    1, 2, 3, 4, 5, 6, 7, 8, 9
                        , 10
                };
                
                
            }

            /// <summary>
            /// Потоковый метод опроса БД для выборки лог-сообщений
            /// </summary>
            /// <param name="data">Объект с данными для разборки методом</param>
            protected override void thread_Proc(object data)
            {
                DataTable tableListDate;
                DateTime dateStart;

                tableListDate = ((PARAM_THREAD_PROC)data).TableLogging;

                //перебор значений и добовление их в строку
                for (int i = 0; i < tableListDate.Rows.Count; i++)
                {
                    dateStart = (DateTime)tableListDate.Rows[i]["DATE_TIME"];
                    m_tableLog.Rows.Add(new object[] { dateStart, s_ID_LOGMESSAGES[(int)INDEX_LOGMSG.START], ProgramBase.MessageWellcome });
                }
                //запрос разбора строки с лог-сообщениями
                base.thread_Proc(tableListDate.Rows.Count);
            }
        }
    }
}
