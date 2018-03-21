using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using StatisticCommon;
using StatisticTransModes;
using System.Collections.ObjectModel;

using Modes;
using ModesApiExternal;

using ASUTP;
using ModesApiExternal;

namespace trans_mc
{
    public class AdminMC : AdminModes
    {
        public interface IEventArgs
        {
            DbMCInterface.ID_MC_EVENT m_id { get; }

            DateTime m_Date
            {
                get;
            }

            Type m_type { get; }
        }

        public class EventArgs<T> : System.EventArgs, IEventArgs
        {
            public DbMCInterface.ID_MC_EVENT m_id { get; }

            public DateTime m_Date { get; }

            public Type m_type { get { return typeof(T); } }

            public ReadOnlyCollection<T> m_listParameters;

            public EventArgs (DbMCInterface.ID_MC_EVENT id, DateTime date, ReadOnlyCollection<T> listParameters)
                : base ()
            {
                m_id = id;

                m_listParameters = new ReadOnlyCollection<T> (listParameters);
            }
        }

        //public event EventHandler EventMaketChanged
        //    , EventPlanDataChanged;

        private ObservableCollection<EventArgs> _listMCEventArgs;

        private IEnumerable<Guid> _maketIdentifiers;

        private string m_strMCServiceHost;

        private System.Threading.ManualResetEvent _eventConnected;

        protected enum StatesMachine
        {
            Unknown = -1

            , PPBRValues
            , PPBRDates
            
            , MaketEquipment
        }

        public AdminMC(string strMCServiceHost)
            : base()
        {
            _dictNotify = new Dictionary<DbMCInterface.ID_MC_EVENT, EventHandler> ();

            m_strMCServiceHost = strMCServiceHost;

            _eventConnected = new System.Threading.ManualResetEvent (false);

            _listMCEventArgs = new ObservableCollection<EventArgs> ();
            _listMCEventArgs.CollectionChanged += listMCEventArgs_CollectionChanged;
        }

        /// <summary>
        /// Возвратить признак наличия обработчика события указанного в аргументе типа
        /// </summary>
        /// <param name="idMCEvent">Тип события Модес-Центр</param>
        /// <returns>Признак наличия обработчика</returns>
        private bool isHandlerMCEvent(DbMCInterface.ID_MC_EVENT idMCEvent)
        {
            return (_dictNotify.ContainsKey(idMCEvent) == true)
                ? (Equals (_dictNotify[idMCEvent], null) == false)
                    : false;
        }

        /// <summary>
        /// Словарь событий 
        /// </summary>
        private static Dictionary<DbMCInterface.ID_MC_EVENT, EventHandler> _dictNotify;

        /// <summary>
        /// Признак, что родительская форма выполняется в режиме "обработка событий Модес-Центр"
        /// </summary>
        public bool IsServiceModesCentre
        {
            get
            {
                return _dictNotify.Keys.ToList().Exists(key => isHandlerMCEvent (key) == true);
            }
        }

        public void AddEventHandler (DbMCInterface.ID_MC_EVENT id_event, EventHandler handler)
        {
            _dictNotify.Add (id_event, handler);
        }

        private void listMCEventArgs_CollectionChanged (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IEventArgs arg = null;
            DbMCInterface.ID_MC_EVENT id = trans_mc.DbMCInterface.ID_MC_EVENT.Unknown;

            Func<IEventArgs, DbMCInterface.ID_MC_EVENT> doWork = delegate (IEventArgs ev) {
                DbMCInterface.ID_MC_EVENT iRes = DbMCInterface.ID_MC_EVENT.Unknown;

                if (typeof (Guid).IsAssignableFrom(ev.m_type) == true)
                    iRes = DbMCInterface.ID_MC_EVENT.RELOAD_PLAN_VALUES;
                else if (typeof (FormChangeMode.KeyDevice).IsAssignableFrom(ev.m_type) == true)
                    iRes = DbMCInterface.ID_MC_EVENT.NEW_PLAN_VALUES;
                else
                    ;

                if (!(iRes == DbMCInterface.ID_MC_EVENT.Unknown))
                    _dictNotify [iRes]?.Invoke (this, (ev as EventArgs));
                else
                    ;

                return iRes;
            };

            switch (e.Action) {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex == 0) {
                        arg = (IEventArgs)e.NewItems [0];
                    } else
                    // новые элементы будут ожидать обработки
                        ;
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if ((sender as ObservableCollection<EventArgs>).Count > 0) {
                        arg = (IEventArgs)(sender as ObservableCollection<EventArgs>) [0];
                    } else
                        ;
                    break;
                default:
                    break;
            }

            if (Equals (arg, null) == false) {
                id = doWork (arg);

                Logging.Logg ().Debug ($"AdminMC::listMCEventArgs_CollectionChanged (Act.={e.Action}, ID_EVENT={id}) - Count={_listMCEventArgs.Count}, NewIndex={e.NewStartingIndex}, OldIndex={e.OldStartingIndex}..."
                    , Logging.INDEX_MESSAGE.NOT_SET);
            } else
                Logging.Logg ().Debug ($"AdminMC::listMCEventArgs_CollectionChanged (Act.={e.Action}) - из коллекции удален крайний элемент ..."
                    , Logging.INDEX_MESSAGE.NOT_SET);
        }

        public void FetchEvent ()
        {
            try {
                if (_listMCEventArgs.Count > 0)
                    _listMCEventArgs?.RemoveAt (0);
                else
                    Logging.Logg ().Warning ($@"trans_mc.AdminMC::FetchEvent () - удаление невозможно, исходный размер = 0...{Environment.NewLine}Стэк={Environment.StackTrace}", Logging.INDEX_MESSAGE.NOT_SET);
            } catch (Exception e) {
                Logging.Logg ().Exception (e, $@"trans_mc.AdminMC::FetchEvent () - ...", Logging.INDEX_MESSAGE.NOT_SET);
            }
        }

        public override FormChangeMode.KeyDevice PrepareActionRDGValues ()
        {
            List<FormChangeMode.KeyDevice> listKey;

            if (IsServiceModesCentre == true)
                listKey = new List<FormChangeMode.KeyDevice> ((_listMCEventArgs [0] as EventArgs<FormChangeMode.KeyDevice>).m_listParameters);
            else
                listKey = GetListKeyTECComponent (FormChangeMode.MODE_TECCOMPONENT.GTP, true);

            if (_listTECComponentKey == null)
                _listTECComponentKey = new List<FormChangeMode.KeyDevice> ();
            else
                ;

            try {
                // проверить на наличие дубликатов
                if (listKey.Count - listKey.Distinct ().Count () == 0) {
                    _listTECComponentKey.Clear ();
                    listKey.ForEach ((key) => {
                        if (_listTECComponentKey.Contains (key) == false)
                            _listTECComponentKey.Add (key);
                        else
                            Logging.Logg ().Error (string.Format ("trans_mc.AdminMC::PrepareExportRDGValues () - добавление повторяющегося индекса {0}...", key.ToString ()), Logging.INDEX_MESSAGE.NOT_SET);
                    });

                    //TODO:
                    // аварийно прекратить выполнение предыдущей операции сохранения ПБР-значений
                } else
                    Logging.Logg ().Error (string.Format ("trans_mc.AdminMC::PrepareExportRDGValues () - в переданном списке <{0}> есть дубликаты...", string.Join (",", listKey.Select (key => key.ToString ()).ToArray ()))
                        , Logging.INDEX_MESSAGE.NOT_SET);

                Logging.Logg ().Action ($"trans_mc.AdminMC::PrepareExportRDGValues () - подготовлен список для опроса: <{string.Join (", ", _listTECComponentKey.ConvertAll<string> (key => key.Id.ToString ()).ToArray ())}>..."
                    , Logging.INDEX_MESSAGE.NOT_SET);
            } catch (Exception e) {
                Logging.Logg ().Exception (e, string.Format ("trans_mc.AdminMC::PrepareExportRDGValues () - ..."), Logging.INDEX_MESSAGE.NOT_SET);
            }

            return base.PrepareActionRDGValues ();
        }

        protected override void getPPBRValuesRequest(TEC t, IDevice comp, DateTime date/*, AdminTS.TYPE_FIELDS mode*/)
        {
            string query = DbMCInterface.Operation.PPBR.ToString();
            int i = -1;

            //Logging.Logg().Debug("AdminMC::GetPPBRValuesRequest (TEC, TECComponent, DateTime, AdminTS.TYPE_FIELDS) - вХод...: query=" + query, Logging.INDEX_MESSAGE.NOT_SET);

            query += ";";
            for (i = 0; i < comp.ListMCentreId.Count; i++)
            {
                query += comp.ListMCentreId[i];

                if ((i + 1) < comp.ListMCentreId.Count) query += ","; else ;
            }

            query += ";";
            query += date.ToOADate().ToString();

            DbMCSources.Sources().Request(m_IdListenerCurrent, query);

            Logging.Logg ().Debug ($"AdminMC::getPPBRValuesRequest (TEC={t.name_shr}, IDevice={comp.name_shr}, Stamp=[{date.ToString()}]) - вЫход...: query=" + query, Logging.INDEX_MESSAGE.D_002);
        }

        protected override void getPPBRDatesRequest (DateTime date)
        {
        }

        private void getMaketEquipmentRequest (IEnumerable<Guid> listIdentifiers, DateTime date)
        {
            string query = DbMCInterface.Operation.MaketEquipment.ToString();
            int i = -1;
            // добавить идентификаторы макетов
            query += ";";
            listIdentifiers.ToList ().ForEach (id => { query += string.Format("{0},", id.ToString()); });
            query = query.Substring (0, query.Length - 1);
            // добавить дату
            query += ";";
            query += date.ToOADate ().ToString ();

            DbMCSources.Sources ().Request (m_IdListenerCurrent, query);

            Logging.Logg ().Debug ($"AdminMC::getMaketEquipmentRequest (Identifiers.Count={listIdentifiers.Count()}, Stamp=[{date.ToString()}]) - вЫход...: query=" + query, Logging.INDEX_MESSAGE.D_002);
        }

        protected override int getPPBRValuesResponse(DataTable table, DateTime date)
        {
            int iRes = 0;
            int i = -1, j = -1,
                hour = -1,
                offsetPBR = 2
                , offset = 0;
            string msgDebug = string.Format(@"Модес-Центр, получено строк={0}, [{1}] на {2}: ", table.Rows.Count, @"PBR_NUMBER", date);

            for (i = 0; i < table.Rows.Count; i++)
            {
                try
                {
                    hour = ((DateTime)table.Rows[i]["DATE_PBR"]).Hour;
                    if ((hour == 0)
                        && (!(((DateTime)table.Rows[i]["DATE_PBR"]).Day == date.Day))) {
                    // это крайний час текущих суток
                        hour = 24;

                        msgDebug = msgDebug.Replace(@"PBR_NUMBER", table.Rows[i][@"PBR_NUMBER"].ToString());
                    } else
                        if (hour == 0)
                        // это предыдущие сутки
                            continue;
                        else
                            ;

                    hour += offset;

                    m_curRDGValues[hour - 1].pbr_number = table.Rows[i][@"PBR_NUMBER"].ToString();
                    // проблема решается ранее, в 'DbMCInterface::GetData'
                    //if (m_curRDGValues[hour - 1].pbr_number.IndexOf(HAdmin.PBR_PREFIX) < 0)
                    //    m_curRDGValues[hour - 1].pbr_number = HAdmin.PBR_PREFIX + m_curRDGValues[hour - 1].pbr_number;
                    //else
                    //    ;

                    //Logging.Logg().Debug(string.Format(@"AdminMC::getPPBRValuesResponse () - hour={0}, PBRNumber={1}...", hour, m_curRDGValues[hour - 1].pbr_number), Logging.INDEX_MESSAGE.NOT_SET);

                    //for (j = 0; j < 3 /*4 для SN???*/; j ++)
                    //{
                    j = 0;
                    if (!(table.Rows[i][offsetPBR + j] is DBNull))
                        m_curRDGValues[hour - 1].pbr = (double)table.Rows[i][offsetPBR + j];
                    else
                        m_curRDGValues[hour - 1].pbr = 0;
                    //}

                    j = 1;
                    if (!(table.Rows[i][offsetPBR + j] is DBNull))
                        m_curRDGValues[hour - 1].pmin = (double)table.Rows[i][offsetPBR + j];
                    else
                        m_curRDGValues[hour - 1].pmin = 0;

                    j = 2;
                    if (!(table.Rows[i][offsetPBR + j] is DBNull))
                        m_curRDGValues[hour - 1].pmax = (double)table.Rows[i][offsetPBR + j];
                    else
                        m_curRDGValues[hour - 1].pmax = 0;

                    m_curRDGValues[hour - 1].recomendation = 0;
                    m_curRDGValues[hour - 1].deviationPercent = false;
                    m_curRDGValues[hour - 1].deviation = 0;

                    //Копирование при переходе лето-зима (-1)
                    if ((m_curDate.Date.Equals(HAdmin.SeasonDateTime.Date) == true) && (hour == (HAdmin.SeasonDateTime.Hour - 0))) {
                        m_curRDGValues[hour].From(m_curRDGValues[hour - 1]);

                        offset++;
                    } else {
                    }

                    msgDebug += string.Format(@"[Час={0}, знач={1}],", hour, m_curRDGValues[hour - 1].pbr);
                } catch(Exception e) {
                    Logging.Logg().Exception(e, string.Format(@"AdminMC::getPPBRValuesResponse () - строка={0}", i), Logging.INDEX_MESSAGE.NOT_SET);
                }
            }

            msgDebug = msgDebug.Substring(0, msgDebug.Length - 1); // удалить лишнюю запятую
            Logging.Logg().Debug(msgDebug, Logging.INDEX_MESSAGE.D_002);

            return iRes;
        }

        protected override int getPPBRDatesResponse (DataTable table, DateTime date)
        {
            int iRes = 0;

            return iRes;
        }

        private int getMaketEquipmentResponse (DataTable table, DateTime date)
        {
            int iRes = 0;

            ReadOnlyCollection<FormChangeMode.KeyDevice> listKeyDevice = null;

            listKeyDevice = new ReadOnlyCollection<FormChangeMode.KeyDevice> ((from row in table.Rows.Cast<DataRow> ()
                select new FormChangeMode.KeyDevice { Id = int.Parse(row ["IdInner"].ToString())
                    , Mode = FormChangeMode.MODE_TECCOMPONENT.GTP })
                .ToList());

            FetchEvent ();

            if (listKeyDevice.Count > 0)
                _listMCEventArgs.Add (new EventArgs<FormChangeMode.KeyDevice> (DbMCInterface.ID_MC_EVENT.NEW_PLAN_VALUES, date, listKeyDevice));
            else
                Logging.Logg().Warning($@"AdminMC::getMaketEquipmentResponse () - получен пустой список с оборудованием...", Logging.INDEX_MESSAGE.NOT_SET);

            return iRes;
        }

        protected override bool InitDbInterfaces()
        {
            bool bRes = true;
            int i = -1;

            DbMCSources.Sources().SetMCApiHandler(dbMCSources_OnEventHandler);
            m_IdListenerCurrent = ASUTP.Database.DbSources.Sources().Register(m_strMCServiceHost, true, @"Modes-Centre");

            return bRes;
        }

        private void dbMCSources_OnEventHandler(object obj)
        {
            DbMCInterface.ID_MC_EVENT id_event;
            EventArgs argEventChanged = null; // оборудование для которого произошло событие
            TEC tec; // для оборудования которой произошло событие

            Func<DateTime, TimeSpan> difference = delegate (DateTime target) {
                // разница целевой даты объекта события и текущей даты
                return target - ASUTP.Core.HDateTime.ToMoscowTimeZone ();
            };

            Func<DateTime, bool> isRequired = delegate (DateTime target) {
                // разница целевой даты объекта события и текущей даты; для проверки необходимости обработки события (только текущие сутки)
                TimeSpan diff = difference(target);

                return diff.TotalDays > -1
                    && diff.TotalDays < 0;
            };

            if (obj is Array) {
                id_event = (DbMCInterface.ID_MC_EVENT)(obj as object []) [0];

                if (id_event == DbMCInterface.ID_MC_EVENT.GENOBJECT_MODIFIED) {
                    Modes.NetAccess.EventRefreshData53500 ev = (obj as object []) [1] as Modes.NetAccess.EventRefreshData53500;

                    #region Подготовка текста сообщения в журнал о событии
                    string msg = string.Empty
                        , listEquipment = string.Empty;
                    msg = string.Format (@"AdminMC::dbMCSources_OnEventHandler((ID_MC_EVENT={1}) - обработчик события - изменения[кол-во={2}]{0}для оборудования {3}..."
                        , Environment.NewLine, id_event, ev.Equipments.Count, @"СПИСОК");
                    foreach (KeyValuePair<DateTime, List<int>> pair in ev.Equipments)
                        listEquipment += string.Format (@"[Дата={0}, список=({1})],", pair.Key.ToString (), string.Join (", ", pair.Value));
                    listEquipment = listEquipment.Remove (listEquipment.Length - 1);
                    msg = msg.Replace (@"СПИСОК", listEquipment);
                    #endregion

                    Logging.Logg ().Action (msg, Logging.INDEX_MESSAGE.NOT_SET);
                } else if (id_event == DbMCInterface.ID_MC_EVENT.RELOAD_PLAN_VALUES) {
                    Modes.NetAccess.EventRefreshJournalMaket53500 ev = (obj as object []) [1] as Modes.NetAccess.EventRefreshJournalMaket53500;

                    DateTime dateTarget;
                    ReadOnlyCollection<Guid> makets;
                    string abbr = string.Empty
                        , taskModes = string.Empty;

                    dateTarget = (DateTime)ev.dtTarget.GetValueOrDefault ().SystemToLocalHqEx ();
                    makets = ev.makets as ReadOnlyCollection<Guid>;
                    abbr = ev.Task.GetAbbr();
                    taskModes = ev.Task.ModesTaskToString();

                    Logging.Logg ().Action (string.Format (@"AdminMC::dbMCSources_OnEventHandler(ID_MC_EVENT={0}) - обработчик события - переопубликация[на дату={1}, кол-во макетов={2}], Аббр={3}, описание={4}..."
                            , id_event.ToString (), dateTarget.ToString (), makets.Count, abbr, taskModes)
                        , Logging.INDEX_MESSAGE.NOT_SET);

                    if (isRequired (dateTarget) == true)
                        argEventChanged = new EventArgs<Guid> (id_event, dateTarget, makets);
                    else
                        Logging.Logg ().Debug ($"AdminMC::dbMCSources_OnEventHandler(ID_MC_EVENT={id_event.ToString ()}) - дата нового не актуальна; Day=[{dateTarget}], разн.(сутки)=[{difference (dateTarget).TotalDays}]..."
                           , Logging.INDEX_MESSAGE.NOT_SET);
                } else if (id_event == DbMCInterface.ID_MC_EVENT.NEW_PLAN_VALUES) {
                    Modes.NetAccess.EventPlanDataChanged ev = (obj as object []) [1] as Modes.NetAccess.EventPlanDataChanged;

                    DateTime day
                        , version;
                    string pbr_number = string.Empty
                        , id_mc_tec = string.Empty;
                    int id_gate = -1;

                    day = ev.Day.SystemToLocalHqEx ();
                    pbr_number = ev.Type.PlanTypeToString ();
                    version = ev.Version.SystemToLocalHqEx ();
                    id_mc_tec = ev.ClientId;
                    id_gate = ev.IdGate;

                    Logging.Logg ().Action (string.Format (@"AdminMC::dbMCSources_OnEventHandler(ID_MC_EVENT={0}) - обработчик события - новый план[на дату={1}, номер={2}, от={3}, для подразделения={4}, IdGate={5}]..."
                            , id_event.ToString(), day.ToString (), pbr_number, version.ToString (), id_mc_tec, id_gate)
                        , Logging.INDEX_MESSAGE.NOT_SET);

                    // проверить дату за которую получен новый план: только сегодняшние и следующие сутки сутки
                    if (isRequired(day) == true) {
                        tec = m_list_tec.Find (t => {
                            return t.name_MC.Trim ().Equals (id_mc_tec.ToString());
                        });

                        argEventChanged = new EventArgs<FormChangeMode.KeyDevice> (id_event, day, new ReadOnlyCollection<FormChangeMode.KeyDevice> (
                            allTECComponents.FindAll (comp => {
                                return (comp.IsGTP == true)
                                    && (comp.tec.m_id == tec.m_id);
                            }).ConvertAll (comp => new FormChangeMode.KeyDevice () { Id = comp.m_id, Mode = FormChangeMode.MODE_TECCOMPONENT.GTP })
                        ));
                    } else
                        Logging.Logg().Debug($"AdminMC::dbMCSources_OnEventHandler(ID_MC_EVENT={id_event.ToString ()}) - дата нового не актуальна; Day=[{day}], разн.(сутки)=[{difference(day).TotalDays}]..."
                            , Logging.INDEX_MESSAGE.NOT_SET);
                } else
                    ;
                // проверить сфорирован ли аргумент для события
                // , имеется ли обработчик; иначе из коллекции не смогут быть удалены элементы(удаление только из-вне)
                // , а значит коллекция увеличивается без ограничений, а элементы никаким образом не обрабатываются
                if ((Equals (argEventChanged, null) == false)
                    && (isHandlerMCEvent(id_event) == true))
                    _listMCEventArgs.Add (argEventChanged);
                else
                    ;
            } else {
                id_event = DbMCInterface.ID_MC_EVENT.Unknown;

                //TODO: проверить результат попытки установки соединения
                Logging.Logg ().Action ("", Logging.INDEX_MESSAGE.NOT_SET);

                _eventConnected.Set ();
            }
        }

        private bool isConnected
        {
            get
            {
                return _eventConnected.WaitOne(0);
            }
        }

        private bool waitConnected (int msec = System.Threading.Timeout.Infinite)
        {
            return _eventConnected.WaitOne (!(msec == System.Threading.Timeout.Infinite) ? msec : ASUTP.Core.Constants.MAX_WATING);
        }

        protected override int StateRequest(int /*StatesMachine*/ state)
        {
            int result = 0;

            string msg = string.Empty;
            StatesMachine stateMachine = StatesMachine.Unknown;
            TECComponent comp;

            stateMachine = (StatesMachine)state;

            switch (stateMachine)
            {
                case StatesMachine.PPBRValues:
                    comp = CurrentDevice as TECComponent;

                    ActionReport ("Получение данных плана.");
                    getPPBRValuesRequest(comp.tec, comp, m_curDate.Date/*, AdminTS.TYPE_FIELDS.COUNT_TYPE_FIELDS*/);
                    break;
                case StatesMachine.PPBRDates:
                    if ((serverTime.Date > m_curDate.Date)
                        && (m_ignore_date == false))
                    {
                        result = -1;
                        break;
                    }
                    else
                        ;
                    ActionReport("Получение списка сохранённых часовых значений.");
                    //GetPPBRDatesRequest(m_curDate);
                    break;

                case StatesMachine.MaketEquipment:
                    ActionReport ("Получение заголовков макетов.");
                    getMaketEquipmentRequest (_maketIdentifiers, m_curDate.Date);
                    break;
                default:
                    break;
            }

            //Logging.Logg().Debug(@"AdminMC::StateRequest () - state=" + state.ToString() + @" - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);
        
            return result;
        }

        protected override int StateCheckResponse(int /*StatesMachine*/ state, out bool error, out object table)
        {
            int iRes = -1;

            error = true;
            table = null;

            StatesMachine stateMachine = (StatesMachine)state;

            switch (stateMachine)
            {
                case StatesMachine.PPBRValues:
                case StatesMachine.PPBRDates:
                case StatesMachine.MaketEquipment:
                    //bRes = GetResponse(m_indxDbInterfaceCurrent, m_listListenerIdCurrent[m_indxDbInterfaceCurrent], out error, out table/*, false*/);
                    iRes = response(m_IdListenerCurrent, out error, out table/*, false*/);
                    break;
                default:
                    break;
            }

            return iRes;
        }

        protected override int StateResponse(int /*StatesMachine*/ state, object table)
        {
            int result = -1;

            StatesMachine stateMachine = (StatesMachine)state;

            switch (stateMachine)
            {
                case StatesMachine.PPBRValues:
                    delegateStopWait();

                    result = getPPBRValuesResponse(table as DataTable, m_curDate);
                    if (result == 0)
                    {
                        readyData(m_curDate, true);
                    }
                    else
                        ;
                    break;
                case StatesMachine.PPBRDates:
                    clearPPBRDates();
                    result = getPPBRDatesResponse(table as DataTable, m_curDate);
                    if (result == 0)
                    {
                    }
                    else
                        ;
                    break;
                case StatesMachine.MaketEquipment:
                    delegateStopWait ();

                    result = getMaketEquipmentResponse (table as DataTable, m_curDate);
                    if (result == 0) {
                    } else
                        ;
                    break;
                default:
                    break;
            }

            if (result == 0)
                ReportClear(false);
            else
                ;

            //Logging.Logg().Debug(@"AdminMC::StateResponse () - state=" + state.ToString() + @", result=" + result.ToString() + @" - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);
            //tPBR.InsertData();
            return result;
        }

        protected override INDEX_WAITHANDLE_REASON StateErrors(int /*StatesMachine*/ state, int request, int result)
        {
            INDEX_WAITHANDLE_REASON reasonRes = INDEX_WAITHANDLE_REASON.SUCCESS;

            bool bClear = false;

            StatesMachine stateMachine = (StatesMachine)state;
            string mesError = string.Empty;

            delegateStopWait ();

            switch (stateMachine)
            {
                case StatesMachine.PPBRValues:
                    if (request == 0)
                        mesError = "Ошибка разбора данных плана. Переход в ожидание.";
                    else
                    {
                        mesError = "Ошибка получения данных плана. Переход в ожидание.";

                        bClear = true;
                    }
                    break;
                case StatesMachine.PPBRDates:
                    try
                    {
                        if (request == 0)
                        {
                            mesError = "Ошибка разбора сохранённых часовых значений (PPBR). Переход в ожидание.";
                            //saveResult = Errors.ParseError;
                        }
                        else
                        {
                            mesError = "Ошибка получения сохранённых часовых значений (PPBR). Переход в ожидание.";
                            //saveResult = Errors.NoAccess;
                        }

                        //semaDBAccess.Release(1);
                    }
                    catch
                    {
                    }
                    break;
                case StatesMachine.MaketEquipment:
                    if (request == 0)
                        mesError = "Ошибка разбора содержания макетов. Переход в ожидание.";
                    else
                        mesError = "Ошибка получения содержания макетов. Переход в ожидание.";
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(mesError) == false)
                ErrorReport (mesError);

            if (bClear)
                ClearValues();

            errorData?.Invoke((int)stateMachine);

            return reasonRes;
        }

        protected override void StateWarnings(int state, int request, int result)
        {
        }

        public override void ClearValues ()
        {
            base.ClearValues ();
        }

        public override void TECComponentComplete (int state, bool bResult)
        {
            base.TECComponentComplete (state, bResult);

            if ((_listTECComponentKey.Count == 0)
                && (IsServiceModesCentre == true))
                FetchEvent ();
            else
                ;
        }

        public void GetMaketEquipment (FormChangeMode.KeyDevice key, EventArgs<Guid>identifiers, DateTime date)
        {
            delegateStartWait ();

            if (isConnected == false)
                waitConnected ();
            else
                ;

            lock (m_lockState) {
                ClearStates ();

                CurrentKey = key;
                _maketIdentifiers = new List<Guid>(identifiers.m_listParameters);

                ClearValues ();

                using_date = false;

                m_prevDate = date.Date;
                m_curDate = m_prevDate;

                AddState ((int)StatesMachine.MaketEquipment);

                Run (@"AdminMC::GetMaketHeader ()");
            }
        }

        public override void GetRDGValues(FormChangeMode.KeyDevice key, DateTime date)
        {
            delegateStartWait();

            if (isConnected == false)
                waitConnected ();
            else
                ;

            lock (m_lockState)
            {
                ClearStates();

                CurrentKey = key;

                ClearValues();

                using_date = false;
                //comboBoxTecComponent.SelectedIndex = indxTECComponents;

                m_prevDate = date.Date;
                m_curDate = m_prevDate;

                //if (m_listIGO.Count == 0)
                //{
                //    AddState((int)StatesMachine.InitIGO);
                //}
                //else
                //    ;

                AddState((int)StatesMachine.PPBRValues);

                Run(@"AdminMC::GetRDGValues ()");
            }
        }
    }
}
