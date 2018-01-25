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
            m_listTEC = tec;

            arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].CheckedChanged += new EventHandler(checkBox_click);
            arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.PC].CheckedChanged += new EventHandler(checkBox_click);
            arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].CheckedChanged += new EventHandler(checkBox_click);
            arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.TG].CheckedChanged += new EventHandler(checkBox_click);
            arrayCheckBoxModeTECComponent[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].CheckedChanged += new EventHandler(checkBox_click);
        }

        #region Наследуемые методы

        /// <summary>
        /// Метод для получения лога
        /// </summary>
        /// <returns>Возвращает лог сообщений</returns>
        protected override LogParse newLogParse() { return new LogParse_DB(); }

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
                    LoggingReadHandler.SetDelegateWait(delegateStartWait, delegateStopWait, delegate () { });
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

            (m_LogParse as LogParse_DB).SetDataTable(tableLogging);
        }

        protected override void handlerCommandListDateByUser (PanelAnalyzer.REQUEST req, DataTable tableRes)
        {
            m_LogParse.Start(new PARAM_THREAD_PROC {
                Delimeter = s_chDelimeters[(int)INDEX_DELIMETER.PART]
                , TableLogging = tableRes
            });
        }

        protected override void handlerCommandProcChecked (PanelAnalyzer.REQUEST req, DataTable tableRes)
        {
            int i = -1
                , msecSleep = System.Threading.Timeout.Infinite;
            int[] ariTags;
            bool[] arbActives;
            DataRow[] rowsUserMaxDatetimeWR;

            if (req.State == PanelAnalyzer.REQUEST.STATE.Ok) {
                ariTags = new int[m_tableUsers.Rows.Count];
                arbActives = new bool [m_tableUsers.Rows.Count];

                for (i = 0;
                    (i < m_tableUsers.Rows.Count)
                        && (m_bThreadTimerCheckedAllowed == true);
                    i++) {
                    ariTags[i] = -1;
                    //Проверка активности
                    rowsUserMaxDatetimeWR = tableRes.Select (@"[ID_USER]=" + m_tableUsers.Rows [i] [@"ID"]);

                    if (rowsUserMaxDatetimeWR.Length == 0)
                    //В течении 2-х недель нет ни одного запуска на выполнение ППО
                        ;
                    else {
                        if (rowsUserMaxDatetimeWR.Length > 1) {
                            //Ошибка
                        } else {
                            //Обрабатываем...
                            ariTags[i] = int.Parse(m_tableUsers.Rows[i][@"ID"].ToString().Trim());
                            arbActives [i] = (HDateTime.ToMoscowTimeZone (DateTime.Now) - DateTime.Parse (rowsUserMaxDatetimeWR [0] [@"MAX_DATETIME_WR"].ToString ())).TotalSeconds < 66;
                        }
                    }
                }

                if (Equals(arbActives, null) == false) {
                    if (req.Key == StatesMachine.ProcCheckedState)
                        dgvUserView.Update(ariTags, arbActives);
                    else if (req.Key == StatesMachine.ProcCheckedFilter) {
                        filterActived (arbActives);
                        dgvUserView.Fill (m_tableUsers, c_list_sorted, 0);
                    } else
                        ;

                    msecSleep = MSEC_TIMERCHECKED_STANDARD;
                } else
                //Нет соединения с БД...
                    msecSleep = MSEC_TIMERCHECKED_FORCE;
            } else
            //Ошибка при выборке данных...
                msecSleep = MSEC_TIMERCHECKED_FORCE;

            m_timerProcChecked.Change(msecSleep, System.Threading.Timeout.Infinite);

            delegateReportClear (true);
        }

        /// <summary>
        /// Запись значений активности в CheckBox на DataGridView с пользователями
        /// </summary>
        /// <param name="obj">???</param>
        protected override void procChecked(object obj)
        {
            delegateActionReport("Получаем список активных пользователей");

            LoggingReadHandler.Command(StatesMachine.ProcCheckedState);
        }

        /// <summary>
        /// Метод для получения из БД списка активных вкладок для выбранного пользователя
        /// </summary>
        /// <param name="user">Пользователь для выборки</param>
        protected override IEnumerable<int> fill_active_tabs(int user)
        {
            List<int> listRes = new List<int> ();

            int err = -1;
            int iRes = -1;
            string[] us ;
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
                                if (IsNumberContains(table_role0[i]["VALUE"].ToString()) == true)
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

                                if (IsNumberContains(table_role0[i]["VALUE"].ToString()) == true)
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
                                    if (IsNumberContains(table_role1[i]["VALUE"].ToString()) == true)
                                        tabs += table_role1[i]["VALUE"].ToString();
                                    else
                                        ;
                                else
                                    ;

                                if ((int)table_role1[i]["ID_UNIT"] == (int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS)
                                    if (IsNumberContains(table_role1[i]["VALUE"].ToString()) == true)
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
                    us = tabs.Split(';');

                    for (int i = 0; i < us.Length; i++)
                    {
                        if (IsPunctuationContains(us[i]) == true) {
                        //разбор в случае составного идентификатора
                            string[] count;
                            count = us[i].Split('=');
                            //добавление идентификатора в список
                            listRes.Add(Convert.ToInt32(count[0]));
                        } else
                            //фильтрация пустых идентификаторов
                            if (string.IsNullOrEmpty(us[i].Trim()) == false)
                                if (IsNumberContains(us[i]) == true)
                                //добавление идентификатора в список
                                    listRes.Add(Convert.ToInt32(us[i]));
                                else
                                    ;
                            else
                                ;
                        
                    }
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

        /// <summary>
        /// Заполнение ListBox активными вкладками
        /// </summary>
        /// <param name="ID_tabs">Список активных вкладок</param>
        protected override void fillListBoxTabVisible(List<int> ID_tabs)
        {
            int tec_indx = 0,
               comp_indx = 0,
               across_indx = -1;
            bool start = false;

            for (int i = 0; i < ID_tabs.Count; i++)
            {
                FormChangeMode.MODE_TECCOMPONENT tec_component = TECComponentBase.Mode(ID_tabs[i]);//получение типа вкладки

                if (arrayCheckBoxModeTECComponent[(int)tec_component].Checked == true)//проверка состояния фильтра для вкладки
                    start = true;
                else
                    start = false;

                if (start == true)
                {
                    if (!(m_listTEC == null))
                        foreach (StatisticCommon.TEC t in m_listTEC)
                        {
                            tec_indx++;
                            comp_indx = 0;

                            if (t.m_id.ToString() == ID_tabs[i].ToString() & tec_component == FormChangeMode.MODE_TECCOMPONENT.TEC)
                            //добавление ТЭЦ в ListBox
                                listBoxTabVisible.Items.Add(t.name_shr);
                            else
                                ;

                            across_indx++;

                            if (t.list_TECComponents.Count > 0)
                            {
                                foreach (StatisticCommon.TECComponent g in t.list_TECComponents)
                                {
                                    comp_indx++;

                                    across_indx++;

                                    if (g.m_id.ToString() == ID_tabs[i].ToString())
                                    //Добавление вкладки в ListBox
                                        listBoxTabVisible.Items.Add($"{t.name_shr} - {g.name_shr}");
                                    else
                                        ;
                                }
                            }
                            else
                                ;
                        }
                    else
                        ;
                }
            }
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

        struct PARAM_THREAD_PROC
        {
            public string Delimeter;

            public DataTable TableLogging;
        }

        /// <summary>
        /// Старт разбора лог-сообщений
        /// </summary>
        /// <param name="id">ID пользователя</param>
        protected override void startLogParse(string id)
        {
            dgvListDatetView.SelectionChanged -= dgvDatetimeStart_SelectionChanged;

            LoggingReadHandler.Command(StatesMachine.ListDateByUser, new object [] { int.Parse(id) }, true);
        }

        /// <summary>
        /// Выборка лог-сообщений по параметрам
        /// </summary>
        /// <param name="id_user">Идентификатор пользователя</param>
        /// <param name="type">Тип сообщений</param>
        /// <param name="beg">Начало периода</param>
        /// <param name="end">Окончание периода</param>
        /// <returns>Массив сообщений</returns>
        protected override void selectLogMessage(int id_user, string type, DateTime beg, DateTime end, Action<DataRow[]>funcResult)
        {
            int iRes = -1;

            //where = addingWhereTypeMessage(@"ID_LOGMSG", strIndxType);
            //if (where.Equals(string.Empty) == false)
            //    query += " AND " + where;
            //else
            //    ;

            LoggingReadHandler.Command(StatesMachine.ListMessageToUserByDate, new object [] { id_user, type, beg, end }, false);
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
        /// Подключение к вкладке
        /// </summary>
        /// <param name="id">ID пользователя</param>
        private void connectToTab(int id)
        {
        }

        /// <summary>
        /// Ошибка подключения
        /// </summary>
        private void errorReadLogging()
        {
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
        /// <returns>true-если есть</returns>
        static bool IsNumberContains(string input)
        {
            bool bRes = false;

            foreach (char c in input)
                if (Char.IsNumber(c))
                    bRes = true;

            return bRes;
        }

        #region Обработчики событий

        /// <summary>
        /// Обработчик события изменения состояния CheckBox'ов
        /// </summary>
        protected void checkBox_click(object sender, EventArgs e)
        {
            listBoxTabVisible.Items.Clear();

            fillListBoxTabVisible (
                fill_active_tabs ((int)m_tableUsers.Rows [dgvUserView.SelectedRows [0].Index] [0]).ToList ()
            );
        }

        public override void LogParseExit ()
        {
            base.LogParseExit ();
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

        protected override void dgvUserStatistic_SelectionChanged (object sender, EventArgs e)
        {
            // throw new NotImplementedException ();
        }

        #endregion

        /// <summary>
        /// Класс для получения лог сообщений из БД
        /// </summary>
        private class LogParse_DB : LogParse
        {
            /// <summary>
            /// Список идентификаторов типов сообщений
            /// </summary>
            public enum INDEX_LOGMSG { START = LogParse.INDEX_START_MESSAGE, STOP
                , ACTION, DEBUG, EXCEPTION
                , EXCEPTION_DB
                , ERROR
                , WARNING
                , UNKNOWN
                    , COUNT_TYPE_LOGMESSAGE
            };

            public LogParse_DB()
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
                int err = -1;
                string text;
                DataTable tblDatetimeStart;
                DateTime dtStart;

                tblDatetimeStart = ((PARAM_THREAD_PROC)data).TableLogging;

                int cnt = 0;
                //перебор значений и добовление их в строку
                for (int i = 0; i < tblDatetimeStart.Rows.Count; i++)
                {
                    dtStart = new DateTime(Int32.Parse(tblDatetimeStart.Rows[i][@"YYYY"].ToString())
                        , Int32.Parse(tblDatetimeStart.Rows[i][@"MM"].ToString())
                        , Int32.Parse(tblDatetimeStart.Rows[i][@"DD"].ToString()));
                    m_tableLog.Rows.Add(new object[] { dtStart, s_ID_LOGMESSAGES[(int)INDEX_LOGMSG.START], ProgramBase.MessageWellcome });

                    cnt += Int32.Parse(tblDatetimeStart.Rows[i][@"CNT"].ToString());
                }
                //запрос разбора строки с лог-сообщениями
                base.thread_Proc(cnt);
            }

            /// <summary>
            /// Выборка лог-сообщений
            /// </summary>
            /// <param name="iListenerId"></param>
            /// <param name="strIndxType">Индекс типа сообщения</param>
            /// <param name="beg">Начало периода</param>
            /// <param name="end">Оончание периода</param>
            /// <param name="res">Массив строк лог-сообщений</param>
            public void SetDataTable(DataTable tableLogging)
            {
                m_tableLog.Clear();//очистка таблицы с лог-сообщениями
                m_tableLog = tableLogging.Copy ();
            }
        }
    }
}
