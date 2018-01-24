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
    public class PanelAnalyzer_DB : PanelAnalyzer
    {
        //private static string _nameShrMainDB = "MAIN_DB";

        /// <summary>
        /// Экземпляр класса 
        ///  для подключения/отправления/получения запросов к БД
        /// </summary>
        private HLoggingReadHandlerDb m_loggingReadHandlerDb;

        public PanelAnalyzer_DB (List<StatisticCommon.TEC> tec, Color foreColor, Color backColor)
            : base(tec, foreColor, backColor)
        {
            m_listTEC = tec;

            checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.GTP].CheckedChanged += new EventHandler(checkBox_click);
            checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.PC].CheckedChanged += new EventHandler(checkBox_click);
            checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TEC].CheckedChanged += new EventHandler(checkBox_click);
            checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.TG].CheckedChanged += new EventHandler(checkBox_click);
            checkBoxs[(int)FormChangeMode.MODE_TECCOMPONENT.ANY].CheckedChanged += new EventHandler(checkBox_click);

            m_loggingReadHandlerDb = new HLoggingReadHandlerDb ();

            _handlers = new Dictionary<HLoggingReadHandlerDb.StatesMachine, Action<HLoggingReadHandlerDb.REQUEST, DataTable>> () {
                { HLoggingReadHandlerDb.StatesMachine.ProcChecked, handlerCommandProcChecked }
                , { HLoggingReadHandlerDb.StatesMachine.ToUserByDate, handlerCommandUserByType }
                , { HLoggingReadHandlerDb.StatesMachine.ToUserGroupDate, handlerCommandToUserGroupDate }
                , { HLoggingReadHandlerDb.StatesMachine.UserByType, handlerCommandUserByType }
            };
            // зарегистрировать асинхронное соединение с БД_конфигурации
            m_loggingReadHandlerDb.EventCommandCompleted += loggingReadHandlerDb_onCommandCompleted;
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

            base.Start ();

            DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings ();
            DbTSQLConfigDatabase.DbConfig().Register();

            if (DbTSQLConfigDatabase.DbConfig ().ListenerId > 0) {
                idMainDB = Int32.Parse (DbTSQLConfigDatabase.DbConfig().Select (@"SELECT [VALUE] FROM [setup] WHERE [KEY]='" + @"Main DataSource" + @"'", null, null, out err).Rows [0] [@"VALUE"].ToString ());
                DataTable tblConnSettMainDB = ConnectionSettingsSource.GetConnectionSettings (DbTSQLConfigDatabase.DbConfig ().ListenerId, idMainDB, -1, out err);

                if ((tblConnSettMainDB.Columns.Count > 0)
                    && (tblConnSettMainDB.Rows.Count > 0)) {
                // "-1" - значит будет назначени идентификатор из БД
                    m_loggingReadHandlerDb.SetConnectionSettings(new ConnectionSettings (tblConnSettMainDB.Rows [0], -1));
                    m_loggingReadHandlerDb.SetDelegateReport(delegateErrorReport, delegateWarningReport, delegateActionReport, delegateReportClear);
                    m_loggingReadHandlerDb.SetDelegateWait(delegateStartWait, delegateStopWait, delegate () { });
                    m_loggingReadHandlerDb.StartDbInterfaces();
                    m_loggingReadHandlerDb.Start ();
                } else
                    throw new Exception ($"PanelAnalyzer_DB::Start () - нет параметров соединения с БД значений ID={idMainDB}...");

                base.Start ();
            } else
                throw new Exception (@"PanelAnalyzer_DB::Start () - нет соединения с БД конфигурации...");

            DbTSQLConfigDatabase.DbConfig ().UnRegister ();

            //dgvFilterTypeMessage.UpdateCounter(m_idListenerLoggingDB, DateTime.MinValue, DateTime.MaxValue, "");

            //dgvMessage.UpdateCounter(m_idListenerLoggingDB, StartCalendar.Value.Date, StopCalendar.Value.Date, get_users(m_tableUsers_stat, dgvUser, true));

        }

        /// <summary>
        /// Остановка таймера обновления данных
        /// </summary>
        public override void Stop ()
        {
            m_loggingReadHandlerDb.Activate (false); m_loggingReadHandlerDb.Stop ();

            base.Stop ();
        }

        private Dictionary<HLoggingReadHandlerDb.StatesMachine, Action<HLoggingReadHandlerDb.REQUEST , DataTable >> _handlers;

        private void loggingReadHandlerDb_onCommandCompleted (HLoggingReadHandlerDb.REQUEST req, DataTable tableRes)
        {
            switch (req.Key) {
                case HLoggingReadHandlerDb.StatesMachine.ProcChecked:
                case HLoggingReadHandlerDb.StatesMachine.ToUserByDate:
                case HLoggingReadHandlerDb.StatesMachine.ToUserGroupDate:
                case HLoggingReadHandlerDb.StatesMachine.UserByType:
                    _handlers [req.Key](req, tableRes);
                    break;
                default:
                    throw new InvalidOperationException ("PanelAnalyzer_DB::loggingReadHandlerDb_onCommandCompleted () - неизвестный тип запроса...");
                    break;
            }
        }

        private void handlerCommandUserByType(HLoggingReadHandlerDb.REQUEST req, DataTable tableRes)
        {
            m_dictDataGridViewLogCounter[(DATAGRIDVIEW_LOGCOUNTER)req.Args[0]].UpdateCounter(tableRes, (DateTime)req.Args[1], (DateTime)req.Args [2], (string)req.Args[3]);

            delegateReportClear?.Invoke(true);
        }

        private void handlerCommandToUserByDate(HLoggingReadHandlerDb.REQUEST req, DataTable tableLogging)
        {
            if (req.State == HLoggingReadHandlerDb.REQUEST.STATE.Ok) {
                filldgvLogMessages(tableLogging.Select(string.Empty, @"DATE_TIME"));
            } else
                ;

            (m_LogParse as LogParse_DB).SetDataTable(tableLogging);
        }

        private void handlerCommandToUserGroupDate(HLoggingReadHandlerDb.REQUEST req, DataTable tableRes)
        {
            m_LogParse.Start(new PARAM_THREAD_PROC {
                Delimeter = m_chDelimeters[(int)INDEX_DELIMETER.PART]
                , TableLogging = tableRes
            });
        }

        private void handlerCommandProcChecked (HLoggingReadHandlerDb.REQUEST req, DataTable tableRes)
        {
            int i = -1
                , msecSleep = System.Threading.Timeout.Infinite;
            bool[] arbActives;
            DataRow[] rowsUserMaxDatetimeWR;

            if (req.State == HLoggingReadHandlerDb.REQUEST.STATE.Ok) {
                arbActives = new bool [m_tableUsers.Rows.Count];

                for (i = 0;
                    (i < m_tableUsers.Rows.Count)
                        && (m_bThreadTimerCheckedAllowed == true);
                    i++) {
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
                            arbActives [i] = (HDateTime.ToMoscowTimeZone (DateTime.Now) - DateTime.Parse (rowsUserMaxDatetimeWR [0] [@"MAX_DATETIME_WR"].ToString ())).TotalSeconds < 66;
                        }
                    }
                }

                if (!(arbActives == null)) {
                    for (i = 0; (i < m_tableUsers.Rows.Count) && (m_bThreadTimerCheckedAllowed == true) && (i < arbActives.Length); i++)
                        dgvClient.Rows[i].Cells[0].Value = arbActives[i];

                    msecSleep = MSEC_TIMERCHECKED_STANDARD;
                } else
                    msecSleep = MSEC_TIMERCHECKED_FORCE; //Нет соединения с БД...

                //Вариант №0
                m_timerChecked.Change(msecSleep, System.Threading.Timeout.Infinite);
                ////Вариант №1
                //if (! (m_timerChecked.Interval == msecSleep)) m_timerChecked.Interval = msecSleep; else ;
                //Debug.WriteLine("procChecked () - date/time:" + DateTime.Now.ToString());
            } else
            //Ошибка при выборке данных...
                ;

            delegateReportClear (true);
        }

        /// <summary>
        /// Запись значений активности в CheckBox на DataGridView с пользователями
        /// </summary>
        /// <param name="obj">???</param>
        protected override void procChecked(object obj)
        {
            delegateActionReport("Получаем список активных пользователей");

            m_loggingReadHandlerDb.Command(HLoggingReadHandlerDb.StatesMachine.ProcChecked/*, handlerCommandProcChecked*/);
        }

        /// <summary>
        /// Метод для получения из БД списка активных вкладок для выбранного пользователя
        /// </summary>
        /// <param name="user">Пользователь для выборки</param>
        protected override void fill_active_tabs(int user)
        {
            int err = -1;
            int iRes = -1;
            string[] us ;
            string where = string.Empty;
            List<int> ID_tabs = new List<int>();
            DataTable tableProfileTabs;

            delegateActionReport ("Получение активных вкладок пользователя");

            #region Фомирование и выполнение запроса для получения списка открытых вкладок у пользователя

            string query = @"SELECT [ID_EXT],[IS_ROLE],[ID_UNIT],[VALUE] FROM [techsite_cfg-2.X.X].[dbo].[profiles] where ";

            if (user.Equals(null) == false)
            {
                //Условие для поиска вкладок
                where = "((ID_EXT = " + (int)user + " and [IS_ROLE] =0) or (id_ext=(select [ID_ROLE] from dbo.[users] where id=" + user + ") and is_role=1)) AND ID_UNIT IN(" + (int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE + ", " + (int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS + ")";
                query += where;

                tableProfileTabs = DbTSQLConfigDatabase.DbConfig().Select(query, out iRes);

                if (tableProfileTabs.Rows.Count > 0)
                {
                    //Список строк с используемыми вкладками
                    DataRow[] table_role0 = tableProfileTabs.Select("IS_ROLE=0 and ID_UNIT in (" + (int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE + ", " + (int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS + ")");
                    string role0 = string.Empty;

                    //Список строк с вкладками по умолчанию
                    DataRow[] table_role1 = tableProfileTabs.Select("IS_ROLE=1 and ID_UNIT in (" + (int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE + ", " + (int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS + ")");
                    string role1 = string.Empty;

                    string tabs = string.Empty;

                    if (table_role0.Length <= 2 & table_role0.Length >= 1)//проверка выборки для используемых вкладок
                    {
                        for (int i = 0; i < table_role0.Length; i++)
                        {
                            if ((int)table_role0[i]["ID_UNIT"] == (int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE)
                            {
                                if (IsNumberContains(table_role0[i]["VALUE"].ToString()) == true)
                                {
                                    tabs += table_role0[i]["VALUE"].ToString();
                                }
                            }
                            if ((int)table_role0[i]["ID_UNIT"] == (int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS)
                            {
                                if (tabs.Equals(string.Empty) == false)
                                {
                                    tabs += ";";
                                }
                                if (IsNumberContains(table_role0[i]["VALUE"].ToString()) == true)
                                {
                                    tabs += table_role0[i]["VALUE"].ToString();
                                }
                            }
                        }
                    }
                    else
                        if (table_role1.Length <= 2 & table_role1.Length >= 1)//проверка выборки для вкладок по умолчанию
                        {
                            for (int i = 0; i < table_role1.Length; i++)
                            {
                                if ((int)table_role1[i]["ID_UNIT"] == (int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE)
                                {
                                    if (IsNumberContains(table_role1[i]["VALUE"].ToString()) == true)
                                    {
                                        tabs += table_role1[i]["VALUE"].ToString();
                                    }
                                }
                                if ((int)table_role1[i]["ID_UNIT"] == (int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS)
                                {
                                    if (IsNumberContains(table_role1[i]["VALUE"].ToString()) == true)
                                    {
                                        tabs += table_role1[i]["VALUE"].ToString();
                                    }
                                }
                            }
                        }
                        else
                        {
                            Logging.Logg().Error(@"PanelAnalyzer::PanelAnalyzer_DB () - нет данных по вкладкам для выбранного пользователя с ID = " + user, Logging.INDEX_MESSAGE.NOT_SET);
                        }

                    us = tabs.Split(';');//создание массива с ID вкладок

                    for (int i = 0; i < us.Length; i++)
                    {
                        if (IsPunctuationContains(us[i]) == true)//разбор в случае составного идентификатора
                        {
                            string[] count;
                            count = us[i].Split('=');
                            ID_tabs.Add(Convert.ToInt32(count[0]));//добавление идентификатора в список
                        }
                        else
                        {
                            if (us[i] != "" & us[i] != " ")//фильтрация пустых идентификаторов
                            {
                                if (IsNumberContains(us[i])==true)
                                ID_tabs.Add(Convert.ToInt32(us[i]));//добавление идентификатора в список
                            }
                        }
                    }

                    fillListBoxTabVisible(ID_tabs);//Заполнение ListBox'а активными вкладками
                }
            }
            else
                ;

            #endregion

            delegateReportClear (true);
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

                if (checkBoxs[(int)tec_component].Checked == true)//проверка состояния фильтра для вкладки
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
                                listTabVisible.Items.Add(t.name_shr);
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
                                        listTabVisible.Items.Add($"{t.name_shr} - {g.name_shr}");
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
            dgvDatetimeStart.SelectionChanged -= dgvDatetimeStart_SelectionChanged;

            m_loggingReadHandlerDb.Command(HLoggingReadHandlerDb.StatesMachine.ToUserGroupDate, new object [] { int.Parse(id) }/*,  delegate (object obj, int err) {
                m_LogParse.Start(new PARAM_THREAD_PROC {
                    Delimeter = m_chDelimeters[(int)INDEX_DELIMETER.PART]
                    , TableLogging = obj as DataTable });
            }*/);
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

            m_loggingReadHandlerDb.Command(HLoggingReadHandlerDb.StatesMachine.ToUserByDate, new object [] { id_user, type, beg, end }/*, delegate (object obj, int err) {
                tableLogging = obj as DataTable;

                if (iRes == 0) {
                    iRes = tableLogging.Rows.Count;
                    funcResult(tableLogging.Select(string.Empty, @"DATE_TIME"));
                } else
                    ;

                (m_LogParse as LogParse_DB).SetDataTable(tableLogging);
            }*/);
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


                strRes = string.Join(m_chDelimeters[(int)INDEX_DELIMETER.PART].ToString()
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
            m_loggingReadHandlerDb.Command (HLoggingReadHandlerDb.StatesMachine.UserByType, new object [] { tag, start_date, end_date, users });
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
        /// Обработчик события выбора пользователя 
        /// </summary>
        protected override void dgvClient_SelectionChanged(object sender, EventArgs e)
        {
            if ((dgvClient.SelectedRows.Count > 0) && (!(dgvClient.SelectedRows[0].Index < 0)))
            {
                bool bUpdate = true;

                if ((dgvDatetimeStart.Rows.Count > 0) && (dgvDatetimeStart.SelectedRows[0].Index < (dgvDatetimeStart.Rows.Count - 1)))
                    if (e == null)
                        bUpdate = false;
                    else
                        ;
                else
                    ;

                if (bUpdate == true)
                {
                    //Останов потока разбора лог-файла пред. пользователя
                    m_LogParse.Stop();

                    dgvDatetimeStart.SelectionChanged -= dgvDatetimeStart_SelectionChanged;

                    //Очистить элементы управления с данными от пред. лог-файла
                    if (IsHandleCreated == true)
                        if (InvokeRequired == true)
                        {
                            BeginInvoke(new DelegateFunc(tabLoggingClearDatetimeStart));
                            BeginInvoke(new DelegateBoolFunc(tabLoggingClearText), true);
                        }
                        else
                        {
                            tabLoggingClearDatetimeStart();
                            tabLoggingClearText(true);
                        }
                    else
                        Logging.Logg().Error(@"FormMainAnalyzer_DB::dgvClient_SelectionChanged () - ... BeginInvoke (TabLoggingClearDatetimeStart, TabLoggingClearText) - ...", Logging.INDEX_MESSAGE.D_001);

                    startLogParse (m_tableUsers.Rows [dgvClient.SelectedRows [0].Index] ["ID"].ToString ().Trim ());

                    updateCounter(DATAGRIDVIEW_LOGCOUNTER.FILTER_TYPE_MESSAGE, DateTime.Today, DateTime.Today.AddDays(1), get_users(m_tableUsers, dgvClient, false));

                    listTabVisible.Items.Clear();//очистка списка активных вкладок

                    DbTSQLConfigDatabase.DbConfig ().SetConnectionSettings ();
                    DbTSQLConfigDatabase.DbConfig ().Register ();

                    fill_active_tabs ((int)m_tableUsers.Rows[dgvClient.SelectedRows[0].Index][0]);

                    DbTSQLConfigDatabase.DbConfig ().UnRegister ();
                }
                else
                    ;
            }
            else
                ;
        }

        /// <summary>
        /// Обработчик события изменения состояния CheckBox'ов
        /// </summary>
        protected void checkBox_click(object sender, EventArgs e)
        {
            listTabVisible.Items.Clear();

            fill_active_tabs((int)m_tableUsers.Rows[dgvClient.SelectedRows[0].Index][0]);
        }

        public override void LogParseExit ()
        {
            base.LogParseExit ();
        }

        protected override void procChecked(out bool[] arbActives, out int err)
        {
            throw new NotImplementedException();
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
            public enum TYPE_LOGMESSAGE { START = LogParse.INDEX_START_MESSAGE, STOP, ACTION, DEBUG, EXCEPTION, EXCEPTION_DB, ERROR, WARNING, UNKNOWN, COUNT_TYPE_LOGMESSAGE };

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
                s_IdTypeMessages = new int[] {
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
                    m_tableLog.Rows.Add(new object[] { dtStart, s_IdTypeMessages[(int)TYPE_LOGMESSAGE.START], ProgramBase.MessageWellcome });

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

        private class HLoggingReadHandlerDb : HHandlerDb
        {
            private ConnectionSettings m_connSett;

            private DateTime m_serverTime;

            private List<REQUEST> _requests;

            public enum StatesMachine {
                ServerTime
                , ProcChecked
                , ToUserByDate
                , ToUserGroupDate
                , UserByType
            }

            public class REQUEST
            {
                public enum STATE { Unknown, Ready, Ok, Error }

                public StatesMachine Key;

                public object[] Args;

                //public Action<object, int> Function;

                #region Запрос

                public string Query
                {
                    get
                    {
                        string strRes = string.Empty
                            , where = string.Empty;

                        switch (Key) {
                            case StatesMachine.ProcChecked:
                                strRes = @"SELECT [ID_USER], MAX ([DATETIME_WR]) as MAX_DATETIME_WR FROM logging GROUP BY [ID_USER] ORDER BY [ID_USER]";
                                break;
                            case StatesMachine.ToUserByDate:
                                strRes = @"SELECT DATETIME_WR as DATE_TIME, ID_LOGMSG as TYPE, MESSAGE FROM logging WHERE ID_USER=" + (int)Args [0];

                                if (((DateTime)Args[2]).Equals (DateTime.MaxValue) == false) {
                                    //Вариан №1 диапазон даты/времени
                                    where = $"DATETIME_WR>='{((DateTime)Args [2]).ToString ("yyyyMMdd HH:mm:ss")}'";
                                    if (((DateTime)Args [3]).Equals (DateTime.MaxValue) == false)
                                        where += $" AND DATETIME_WR<'{((DateTime)Args [3]).ToString ("yyyyMMdd HH:mm:ss")}'";
                                    else
                                        ;
                                    ////Вариан №2 указанные сутки
                                    //where = "DATETIME_WR='" + beg.ToString("yyyyMMdd") + "'";
                                } else
                                    ;

                                if (where.Equals (string.Empty) == false)
                                    strRes += @" AND " + where;
                                else
                                    ;
                                break;
                            case StatesMachine.ToUserGroupDate:
                                strRes = @"SELECT DATEPART (DD, [DATETIME_WR]) as DD, DATEPART (MM, [DATETIME_WR]) as MM, DATEPART (YYYY, [DATETIME_WR]) as [YYYY], COUNT(*) as CNT"
                                    + @" FROM [dbo].[logging]"
                                    + @" WHERE [ID_USER]=" + (int)Args[0]
                                    + @" GROUP BY DATEPART (DD, [DATETIME_WR]), DATEPART (MM, [DATETIME_WR]), DATEPART (YYYY, [DATETIME_WR])"
                                    + @" ORDER BY [DD]";
                                break;
                            case StatesMachine.UserByType:
                                bool byDate = !((DateTime)Args [1]).Equals (DateTime.MaxValue)
                                    , byUser = !string.IsNullOrEmpty(((string)Args [3]).Trim());

                                if (byDate == true) {
                                //диапазон даты/времени
                                    where = "WHERE DATETIME_WR BETWEEN '" + ((DateTime)Args [1]).ToString ("yyyyMMdd HH:mm:ss") + "'";
                                    if (((DateTime)Args [2]).Equals (DateTime.MaxValue) == false) {
                                        where += " AND '" + ((DateTime)Args [2]).ToString ("yyyyMMdd HH:mm:ss") + "'";
                                    } else
                                        ;
                                } else
                                    ;

                                if (byUser == true) {
                                //добавление идентификаторов пользователей к условиям выборки
                                    if (string.IsNullOrEmpty(where) == true)
                                        where += "WHERE";
                                    else
                                        where += " AND";
                                    where += " ID_USER in (" + ((string)Args [3]) + ")";
                                    strRes += where;
                                } else
                                    ;

                                strRes = $"SELECT [ID_LOGMSG], COUNT (*) as [COUNT] FROM [dbo].[logging] {where} GROUP BY [ID_LOGMSG] ORDER BY [ID_LOGMSG]";
                                break;
                            default:
                                break;
                        }

                        return strRes;
                    }
                }

                #endregion

                public STATE State;

                public REQUEST (StatesMachine key, object arg)
                {
                    Key = key;

                    if (Equals (arg, null) == false)
                        if (arg is Array) {
                            Args = new object [(arg as object []).Length];

                            for (int i = 0; i < Args.Length; i++)
                                Args [i] = (arg as object []) [i];
                        } else
                            Args = new object [] { arg };
                    else
                        Args = new object [] { };

                    State = STATE.Ready;
                }

                public bool IsEmpty
                {
                    get
                    {
                        return Args == null;
                    }
                }
            }

            public event Action<REQUEST, DataTable> EventCommandCompleted;

            public HLoggingReadHandlerDb ()
            {
                m_connSett = new ConnectionSettings();
                m_serverTime = DateTime.MinValue;
                _requests = new List<REQUEST>();
            }

            public void SetConnectionSettings (ConnectionSettings connSett)
            {
                m_connSett = connSett;
            }

            private int ListenerIdConfigDb
            {
                get
                {
                    return m_dictIdListeners[0][(int)CONN_SETT_TYPE.CONFIG_DB];
                }
            }

            private int ListenerIdMainDb
            {
                get
                {
                    return m_dictIdListeners [0] [(int)CONN_SETT_TYPE.LIST_SOURCE];
                }
            }

            public override void ClearValues ()
            {
                throw new NotImplementedException ();
            }

            public override void StartDbInterfaces ()
            {
                if (m_connSett.IsEmpty == false) {
                    if (m_dictIdListeners.ContainsKey(0) == false)
                        m_dictIdListeners.Add(0, new int[] { -1, -1 });
                    else
                        ;

                    //register(0, (int)CONN_SETT_TYPE.CONFIG_DB, FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), m_connSett.name);
                    register(0, (int)CONN_SETT_TYPE.LIST_SOURCE, m_connSett, m_connSett.name);
                } else
                    throw new InvalidOperationException("PanelAnalyzer_DB.HLoggingReadHandlerDb::StartDbInterfaces () - ");
            }

            /// <summary>
            /// Добавить состояния в набор для обработки
            /// данных 
            /// </summary>
            public void Command (StatesMachine state/*, Action <object, int> handlerCommand*/)
            {
                Command(state, null/*, handlerCommand*/);
            }

            public void Command(StatesMachine state, object args/*, Action<object, int> handlerCommand*/)
            {
                lock (m_lockState) {
                    //ClearStates();

                    AddState((int)StatesMachine.ServerTime);
                    AddState((int)state);

                    Logging.Logg().Debug($"PanelAnalyzer.HLoggingReadHandlerDb::Command () - добавлено {state}...", Logging.INDEX_MESSAGE.NOT_SET);
                    _requests.Add(new REQUEST (state, args));

                    Run(@"PanelAnalyzer.HLoggingReadHandlerDb::Command () - run...");
                }
            }

            /// <summary>
            /// Получить результат обработки события
            /// </summary>
            /// <param name="state">Событие для получения результата</param>
            /// <param name="error">Признак ошибки при получении результата</param>
            /// <param name="outobj">Результат запроса</param>
            /// <returns>Признак получения результата</returns>
            protected override int StateCheckResponse (int state, out bool error, out object outobj)
            {
                int iRes = 0;

                error = false;
                outobj = new DataTable();

                StatesMachine statesMachine = (StatesMachine)state;

                switch (statesMachine) {
                    case StatesMachine.ServerTime:
                    case StatesMachine.ProcChecked:
                    case StatesMachine.ToUserByDate:
                    case StatesMachine.ToUserGroupDate:
                    case StatesMachine.UserByType:
                        iRes = response (m_IdListenerCurrent, out error, out outobj);
                        break;
                    default:
                        error = true;
                        outobj = null;
                        break;
                }

                return iRes;
            }

            /// <summary>
            /// Функция обратного вызова при возникновения ситуации "ошибка"
            ///  при обработке списка состояний
            /// </summary>
            /// <param name="state">Состояние при котором возникла ситуация</param>
            /// <param name="req">Признак результата выполнения запроса</param>
            /// <param name="res">Признак возвращения результата при запросе</param>
            /// <returns>Индекс массива объектов синхронизации</returns>
            protected override INDEX_WAITHANDLE_REASON StateErrors (int state, int req, int res)
            {
                INDEX_WAITHANDLE_REASON iRes = INDEX_WAITHANDLE_REASON.SUCCESS;

                func_Completed("StateErrors", (StatesMachine)state, new DataTable(), res);

                errorReport (@"Получение значений из БД - состояние: " + ((StatesMachine)state).ToString ());

                return iRes;
            }

            protected override int StateRequest (int state)
            {
                int iRes = 0;

                REQUEST req;
                StatesMachine stateMachine = (StatesMachine)state;

                switch (stateMachine) {
                    case StatesMachine.ServerTime:
                        GetCurrentTimeRequest (DbInterface.DB_TSQL_INTERFACE_TYPE.MSSQL, ListenerIdMainDb);
                        actionReport (@"Получение времени с сервера БД - состояние: " + ((StatesMachine)state).ToString ());
                        break;
                    case StatesMachine.ProcChecked:
                    case StatesMachine.ToUserByDate:
                    case StatesMachine.ToUserGroupDate:
                    case StatesMachine.UserByType:
                        req = getFirstRequst (stateMachine);
                        if (req.IsEmpty == false) {
                            Request (ListenerIdMainDb, req.Query);
                        } else
                            ;
                        actionReport (@"Получение значений из БД - состояние: " + ((StatesMachine)state).ToString ());
                        break;
                    default:
                        break;
                }

                return iRes;
            }

            /// <summary>
            /// Обработка УСПЕШНО полученного результата
            /// </summary>
            /// <param name="state">Состояние для результата</param>
            /// <param name="table">Значение результата</param>
            /// <returns>Признак обработки результата</returns>
            protected override int StateResponse (int state, object table)
            {
                int iRes = 0;

                StatesMachine stateMachine = (StatesMachine)state;

                switch (stateMachine) {
                    case (int)StatesMachine.ServerTime:
                        m_serverTime = ((DateTime)(table as DataTable).Rows [0] [0]);
                        break;
                    case StatesMachine.ProcChecked:
                    case StatesMachine.ToUserByDate:
                    case StatesMachine.ToUserGroupDate:
                    case StatesMachine.UserByType:
                        func_Completed("StateResponse", (StatesMachine)state, table, iRes);
                        break;
                    default:
                        break;
                }

                //Проверить признак крайнего в наборе состояний для обработки
                if (isLastState (state) == true) {
                    //Удалить все сообщения в строке статуса
                    ReportClear(true);
                } else
                    ;

                return iRes;
            }

            protected override void StateWarnings (int state, int req, int res)
            {
                throw new NotImplementedException ();
            }

            private REQUEST getFirstRequst(StatesMachine state)
            {
                if (_requests.Select(h => h.Key).Contains<StatesMachine>(state) == true)
                    return _requests.First(h => { return h.Key == state; });
                else
                    return new REQUEST(state, null);
            }

            private void func_Completed(string nameFunc, StatesMachine state, object obj, int err)
            {
                REQUEST req =
                    getFirstRequst(state)
                    //_handlers.Pop()
                    ;

                req.State = err == 0 ? REQUEST.STATE.Ok : REQUEST.STATE.Error;

                //handler.Function.Invoke(obj, err);
                EventCommandCompleted(req, obj as DataTable);

                Logging.Logg().Debug($"PanelAnalyzer.HLoggingReadHandlerDb::Command () - удалено {state}...", Logging.INDEX_MESSAGE.NOT_SET);
                _requests.Remove(req);
            }
        }
    }
}
