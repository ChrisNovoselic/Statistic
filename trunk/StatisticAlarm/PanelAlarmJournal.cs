using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Data;

using System.Windows.Forms; //..., CheckBox

using HClassLibrary;
using StatisticCommon;

namespace StatisticAlarm
{
    /// <summary>
    /// Класс панели для отображения с списка событий
    /// </summary>
    public partial class PanelAlarmJournal : PanelStatistic, IDataHost
    {
        /// <summary>
        /// Перечисление для режимов работы вкладки
        /// </summary>
        public enum MODE { SERVICE, ADMIN, VIEW };
        /// <summary>
        /// Режим работы вкладки
        /// </summary>
        private MODE mode;
        /// <summary>
        /// Событие подтверждения сигнализации
        /// </summary>
        private event DelegateIntIntFunc EventConfirm;
        /// <summary>
        /// Список объектов ТЭЦ
        /// </summary>
        private List<TEC> m_list_tec;
        /// <summary>
        /// Список идентификаторов компонентов ТЭЦ (ГТП), отображающихся в списке 'Компоненты ТЭЦ'
        /// </summary>
        private List <int> m_listIdTECComponents;
        /// <summary>
        /// Объект проверки условий выполнения сигнализаций типов "Мощность ГТП", "ТГ вкл./откл."
        /// </summary>
        private AdminAlarm m_adminAlarm;
        /// <summary>
        /// Объект чтения/записи списка событий в БД
        /// </summary>
        private ViewAlarm m_viewAlarm;
        /// <summary>
        /// Таймер для обновления содержимого таблицы со списком событий
        /// </summary>
        private System.Windows.Forms.Timer m_timerView;
        /// <summary>
        /// Ширина панели для размещения активных элементов управления
        /// </summary>
        private int _widthPanelManagement = 166;
        /// <summary>
        /// Конструктор - основной (с параметрами)
        /// </summary>
        /// <param name="mode">Режим работы панели</param>
        public PanelAlarmJournal(MODE mode)
        {
            //Инициализация собственных значений
            initialize(mode);
        }
        /// <summary>
        /// Конструктор - дополнительный (с параметрами)
        /// </summary>
        /// <param name="container">См. документацию на 'Control'</param>
        /// <param name="mode">Режим работы панели</param>
        public PanelAlarmJournal(IContainer container, MODE mode)
        {
            container.Add(this);
            //Инициализация собственных значений
            initialize(mode);
        }
        /// <summary>
        /// Инициализация собственных параметров
        /// </summary>
        /// <param name="mode">Режим работы панели</param>
        private void initialize(MODE mode)
        {
            //Инициализация визуальных компонентов
            InitializeComponent();
            //Запомнить режим работы панели
            this.mode = mode;            
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {            
            base.Start ();

            Control ctrl = null;
            int err = -1 //Признак выполнения метода/функции
                //Зарегистрировать соединение/получить идентификатор соединения
                , iListenerId = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB")
                , indx = -1;
            //Инициализация списка с ТЭЦ
            m_list_tec = new InitTEC_200(iListenerId, true, false).tec;
            ////Инициализация 
            initAdminAlarm ();
            initViewAlarm(new ConnectionSettings(InitTECBase.getConnSettingsOfIdSource(TYPE_DATABASE_CFG.CFG_200, iListenerId, FormMainBase.s_iMainSourceData, -1, out err).Rows[0], 0));
            startViewAlarm();
            //Инициализировать таймер для обновления значений в таблице
            m_timerView = new Timer();            
            m_timerView.Tick += new EventHandler(fTimerView_Tick);
            //Отменить регистрацию соединения
            DbSources.Sources().UnRegister(iListenerId);
            //Заполнить списки             
            m_listIdTECComponents = new List<int>();
            ctrl = Find(INDEEX_CONTROL.CLB_TECCOMPONENT);
            (ctrl as CheckedListBox).Items.Add(@"Все компоненты", true);
            foreach (TEC tec in m_list_tec)
                foreach (TECComponent comp in tec.list_TECComponents)
                    if (comp.IsGTP == true)
                    {
                        indx = (ctrl as CheckedListBox).Items.Add(tec.name_shr + @" - " + comp.name_shr, true);
                        m_listIdTECComponents.Add(comp.m_id);
                    }
                    else
                        ;
            (ctrl as CheckedListBox).ItemCheck += new ItemCheckEventHandler(fTECComponent_OnItemCheck);
            (ctrl as CheckedListBox).SelectedIndexChanged += new EventHandler(fTECComponent_OnSelectedIndexChanged);
            (ctrl as CheckedListBox).SelectedIndex = 0;
            (ctrl as CheckedListBox).Enabled = false;
            //Запустить на выполнениие (при необходимости) таймер для обновления значений в таблице
            ctrl = Find(INDEEX_CONTROL.CBX_WORK) as CheckBox;
            if (mode == MODE.SERVICE)
                (ctrl as CheckBox).Checked = true;
            else
                if (mode == MODE.ADMIN)
                    (ctrl as CheckBox).Checked = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.AUTO_ALARM_KOMDISP);
                else
                    if (mode == MODE.VIEW)
                        (ctrl as CheckBox).Checked =
                        (ctrl as CheckBox).Enabled =
                             false;
                    else
                        ;
            if ((ctrl as CheckBox).Checked == true)
            {
                if (mode == MODE.SERVICE) startAdminAlarm (); else ;

                m_timerView.Interval = 1;
                m_timerView.Start();
            }
            else
                ;
            //Назначить обработчик событий при изменении признака "Включено/отключено"
            (ctrl as CheckBox).CheckedChanged += new EventHandler(cbxWork_OnCheckedChanged);
            //Назначить обработчик события изменение даты
            (Find (INDEEX_CONTROL.MCLDR_CURRENT) as MonthCalendar).DateChanged += new DateRangeEventHandler(onEventDateChanged);
            ////Назначить обработчик события при получении из БД списка событий (передать список для отображения)
            //m_viewAlarm.EvtGetData += new DelegateObjectFunc((Find (INDEEX_CONTROL.DGV_EVENTS) as DataGridViewAlarmJournal).OnEvtGetData);
        }
        /// <summary>
        /// Запустить на выполнение объект регистрации выполнения условий сигнализаций
        /// </summary>
        private void startAdminAlarm()
        {
            //Инициализировать (при необходимости) объект
            if (m_adminAlarm == null) initAdminAlarm(); else ;
            //Проверить состояние, позволяющее запуск на выполнение 
            if (m_adminAlarm.IsStarted == false)
            {
                m_adminAlarm.Start(); // запустить на выполнение
                m_adminAlarm.Activate(true); // активировать
            }
            else ;
        }
        /// <summary>
        /// Запустить на выполнение объект чтения/записи/обновления списка событий в БД
        /// </summary>
        private void startViewAlarm()
        {
            if (! (m_viewAlarm == null))
                if (m_viewAlarm.IsStarted == false)
                {
                    m_viewAlarm.Start();
                    m_viewAlarm.Activate(true);
                }
                else ;
            else
                throw new Exception (@"PanelAlarmJournal::startViewAlarm () - ...");
        }
        /// <summary>
        /// Метод обратного вызова для таймера обновления значений в таблице
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void fTimerView_Tick(object obj, EventArgs ev)
        {
            //m_viewAlarm.Push(this, new object [] { new object [] { new object[] { ViewAlarm.StatesMachine.List, DatetimeCurrent, HourBegin, HourEnd }}});
            //EvtDataAskedHost(new object[] { ViewAlarm.StatesMachine.List, DatetimeCurrent, HourBegin, HourEnd });
            DataAskedHost(new object[] { ViewAlarm.StatesMachine.List, DatetimeCurrent, HourBegin, HourEnd });
            //Назначить (при необходимости) интервал между вызовами
            if (! (m_timerView.Interval == PanelStatistic.POOL_TIME * 1000))
                m_timerView.Interval = PanelStatistic.POOL_TIME * 1000;
            else
                ;
        }
        /// <summary>
        /// Остановить панель, и все связанные с ней объекты
        /// </summary>
        public override void Stop() 
        {
            //Остановить объект "обзор событий"
            if (! (m_viewAlarm == null))
                if (m_viewAlarm.IsStarted == true)
                {
                    m_viewAlarm.Activate(false);
                    m_viewAlarm.Stop();
                }
                else ;
            else
                ;
            ////Остановить объект "регистрация событий"
            //if (! (m_adminAlarm == null))
            //    if (m_adminAlarm.IsStarted == true)
            //    {
            //        m_adminAlarm.Activate (false);
            //        m_adminAlarm.Stop();
            //    }
            //    else ;
            //else
            //    ;
            //Остановить таймер
            if (!(m_timerView == null))
            {
                m_timerView.Stop();
                m_timerView.Dispose();
                m_timerView = null;
            }

            base.Stop ();
        }
        /// <summary>
        /// Активировать/деактивировать панель
        /// </summary>
        /// <param name="activate">Признак активации/деактивации</param>
        /// <returns>Признак результата выполнения операции (Изменено/не_изменено стостояние)</returns>
        public override bool Activate(bool activate)
        {
            //Получтить признак изменения стостояния базовой панели
            bool bRes = base.Activate (activate);
            //Проверить признак изменения стостояния базовой панели
            if (bRes == true)
            {//Только при изменении состояния базовой панели
                if ((activate == true)
                    && ((Find (INDEEX_CONTROL.CBX_WORK) as CheckBox).Checked == false))
                    // при активации обновить список событий
                    (Find (INDEEX_CONTROL.BTN_REFRESH) as Button).PerformClick ();
                else
                    ;
                //???
                if ((Find (INDEEX_CONTROL.CBX_WORK) as CheckBox).Checked == true)
                {
                    switch (mode)
                    {
                        case MODE.SERVICE:                            
                            m_viewAlarm.Activate(activate);
                            break;
                        case MODE.ADMIN:
                            m_viewAlarm.Activate(activate);
                            break;
                        case MODE.VIEW:
                            m_viewAlarm.Activate(activate);
                            break;
                        default:
                            break;
                    }
                }
                else
                    ;
            }
            else
                ;

            return bRes;
        }
        /// <summary>
        /// Инициалировать параметры контэйнеров-столбцов, контэйнеров-строк
        /// </summary>
        /// <param name="cols">Количество контэйнеров-столбцов</param>
        /// <param name="rows">Количество контэйнеров-строк</param>
        protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
        {
            this.ColumnCount = cols;
            this.RowCount = rows;

            this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, _widthPanelManagement));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            this.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }
        /// <summary>
        /// Инициализация объекта регистрации выполнения условий сигнализаций
        /// </summary>
        private void initAdminAlarm()
        {
            m_adminAlarm = new AdminAlarm();
            m_adminAlarm.InitTEC(m_list_tec);

            m_adminAlarm.EventAdd += new AdminAlarm.DelegateOnEventReg(OnAdminAlarm_EventAdd);
            m_adminAlarm.EventRetry += new AdminAlarm.DelegateOnEventReg(OnAdminAlarm_EventRetry);

            this.EventConfirm += new DelegateIntIntFunc(m_adminAlarm.OnEventConfirm);
        }
        /// <summary>
        /// Инициализация объекта чтения/записи/обновления списка событий
        /// </summary>
        /// <param name="connSett">Объект с параметрами соединения с БД_конфигурации</param>
        private void initViewAlarm(ConnectionSettings connSett)
        {
            if (m_viewAlarm == null) m_viewAlarm = new ViewAlarm(connSett); else ;
        }
        //???
        private void OnAdminAlarm_EventAdd(TecView.EventRegEventArgs ev)
        {
            Console.WriteLine(@"PanelAlarmJournal::OnAdminAlarm_EventAdd () - ID=" + ev.Id + @", message=" + ev.m_message);
            
            if (IsHandleCreated/*InvokeRequired*/ == true)
            {//...для this.BeginInvoke
                //m_viewAlarm.Push(this, new object [] { new object [] { new object [] { ViewAlarm.StatesMachine.Insert, ev }}});
                DataAskedHost(new object[] { ViewAlarm.StatesMachine.Insert, ev });
            }
            else
                Logging.Logg().Error(@"PanelAlarm::OnAdminAlarm_EventAdd () - ... BeginInvoke (...) - ...", Logging.INDEX_MESSAGE.D_001);
        }
        //???
        private void OnAdminAlarm_EventRetry(TecView.EventRegEventArgs ev)
        {
        }
        /// <summary>
        /// Обработчик события снятия признака "использовать компонент ТЭЦ" в списке "Компоненты ТЭЦ"
        /// </summary>
        /// <param name="obj">Объект, иницировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void fTECComponent_OnItemCheck (object obj, ItemCheckEventArgs ev)
        {
            //Получить объект - "Список компонентов ТЭЦ"
            CheckedListBox ctrl = obj as CheckedListBox;
            //Проверить выбранный элемент "Все компоненты"
            if (ev.Index == 0)
            {
                //Отменить обработку события, т.к. ниже изменяется состояние ВСЕХ компонентов
                ctrl.ItemCheck -= new ItemCheckEventHandler(fTECComponent_OnItemCheck);
                //Изменить признак для всех компонентов в списке в соответствии с признаком у "Все компоненты"
                for (int i = 1; i < ctrl.Items.Count; i ++)
                    ctrl.SetItemCheckState (i, ev.NewValue);
                //Возобновить обработку события
                ctrl.ItemCheck += new ItemCheckEventHandler(fTECComponent_OnItemCheck);
            }
            else
                ;
        }
        /// <summary>
        /// Обработчик события изменение признака "Включено/отключено"
        /// </summary>
        /// <param name="obj">Объект, иницировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void cbxWork_OnCheckedChanged (object obj, EventArgs ev)
        {
            CheckBox ctrl = obj as CheckBox;

            //??? - Активировать объект "регистрация событий"
            if (mode == MODE.SERVICE) m_adminAlarm.Activate(ctrl.Checked); else ;
            //??? - Активировать объект чтения/записи/обновления списка событий
            m_viewAlarm.Activate(ctrl.Checked);
            //Запустить/остановить таймер обновления значений в таблице
            if (ctrl.Checked == true)
            {
                m_timerView.Interval = 1;
                m_timerView.Start();
            }
            else
                m_timerView.Stop();
        }
        /// <summary>
        /// Найти объект-'компонент ТЭЦ' по идентификатору в локальном списке компонентов ТЭЦ
        /// </summary>
        /// <param name="id">Идентификатор для поиска</param>
        /// <returns>Объект-'компонент ТЭЦ'</returns>
        private TECComponent findTECComponentOfID (int id)
        {
            foreach  (TEC tec in m_list_tec)
                foreach (TECComponent comp in tec.list_TECComponents)
                    if ((comp.IsGTP == true) && (comp.m_id == id))
                        return comp;
                    else
                        ;

            return null;
        }
        /// <summary>
        /// Обработчик события - выбор элемента в списке компонентов ТЭЦ
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ev"></param>
        private void fTECComponent_OnSelectedIndexChanged(object obj, EventArgs ev)
        {
            //Получить объект со значением коэфициента для выбранного компонента ТЭЦ
            NumericUpDown ctrl = Find(INDEEX_CONTROL.NUD_KOEF) as NumericUpDown;
            //Проверить индекс выбранного элемента
            if ((Find(INDEEX_CONTROL.CLB_TECCOMPONENT) as CheckedListBox).SelectedIndex > 0)
            {//Только для настоящего компонента ТЭЦ, а не виртуального "Все компоненты"
                //Включить (при необходимости) объект со значением коэфициента
                if (ctrl.Enabled == false) ctrl.Enabled = true; else ;
                //Отобразить значение коэффициента
                setNudnKoeffAlarmCurPowerValue ();
            }
            else
            {//Для виртуальног компонента "Все компоненты"
                //Выключить (при необходимости) объект со значением коэфициента
                if (ctrl.Enabled == true) ctrl.Enabled = false; else ;
                //Отобразить значение коэффициента
                setNudnKoeffAlarmCurPowerValue(-1);
            }
        }
        /// <summary>
        /// Отобразить значение коэффициента для выбранного элемента в списке "Компоненты ТЭЦ"
        /// </summary>
        private void setNudnKoeffAlarmCurPowerValue()
        {
            TECComponent comp = findTECComponentOfID (m_listIdTECComponents[(Find(INDEEX_CONTROL.CLB_TECCOMPONENT) as CheckedListBox).SelectedIndex - 1]);
            setNudnKoeffAlarmCurPowerValue (comp.m_dcKoeffAlarmPcur);
        }
        /// <summary>
        /// Отобразить значение коэффициента для выбранного элемента в списке "Компоненты ТЭЦ"
        /// </summary>
        /// <param name="value">Значение для отображения</param>
        private void setNudnKoeffAlarmCurPowerValue(decimal value)
        {
            //Получить объект со значением коэффициента для выбранного элемента
            NumericUpDown ctrl = Find(INDEEX_CONTROL.NUD_KOEF) as NumericUpDown;
            //Отменить обработку события - изменение значения, т.к. изменяем значение программно
            ctrl.ValueChanged -= new EventHandler(NudnKoeffAlarmCurPower_ValueChanged);
            if (value > 0)
            {//Для реального компонента ТЭЦ
                ctrl.Minimum = 2M;
                ctrl.Maximum = 90M;
            }
            else
            {//Для виртуального компонента ТЭЦ "Все компоненты"
                ctrl.Minimum =
                ctrl.Maximum =
                    -1;
            }
            ctrl.Value = value;
            //Возобновить обработку события
            ctrl.ValueChanged += new EventHandler(NudnKoeffAlarmCurPower_ValueChanged);
        }
        /// <summary>
        /// Обработчик события - изменение значения коэффициента для компонента ТЭЦ
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void NudnKoeffAlarmCurPower_ValueChanged(object obj, EventArgs ev)
        {
            TECComponent comp = findTECComponentOfID(m_listIdTECComponents[(Find(INDEEX_CONTROL.CLB_TECCOMPONENT) as CheckedListBox).SelectedIndex - 1]);
            //Запомнить установленное значение "времени выполнения"
            comp.m_dcKoeffAlarmPcur = (obj as NumericUpDown).Value;

            int err = -1
                //Зарегистрировать соединение с БД_конфигарации
                , idListenerConfigDB = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
            //Получить объект соединения с БД_конфигурации
            System.Data.Common.DbConnection dbConn = DbSources.Sources().GetConnection(idListenerConfigDB, out err);
            ////Сохранить установленное значение в БД_конфигурации
            //DbTSQLInterface.ExecNonQuery(ref dbConn, @"UPDATE [dbo].[GTP_LIST] SET [KoeffAlarmPcur] = " + comp.m_dcKoeffAlarmPcur + @" WHERE [ID] = " + comp.m_id, null, null, out err);
            //Отменить регистрацию соединения
            DbSources.Sources().UnRegister(idListenerConfigDB);
        }
        /// <summary>
        /// Найти дочерний элемент управления по идентификатору
        /// </summary>
        /// <param name="indx">Идентификатор элемента управления</param>
        /// <returns>Дочерний элемент управления</returns>
        private Control Find (INDEEX_CONTROL indx)
        {
            return Controls.Find (indx.ToString (), true) [0];
        }
        /// <summary>
        /// Текущая (выбранная дата) в элементе управления 'MonthCalendar'
        /// </summary>
        private DateTime DatetimeCurrent { get { return (Find(INDEEX_CONTROL.MCLDR_CURRENT) as MonthCalendar).SelectionStart.Date; } }
        /// <summary>
        /// Текущий (установленный) индекс часа начала периода в указанные сутки
        /// </summary>
        private int HourBegin { get { return (int)(Find(INDEEX_CONTROL.NUD_HOUR_BEGIN) as NumericUpDown).Value; } }
        /// <summary>
        /// Текущий (установленный) индекс часа окончания периода в указанные сутки
        /// </summary>
        private int HourEnd { get { return (int)(Find(INDEEX_CONTROL.NUD_HOUR_END) as NumericUpDown).Value; } }
        /// <summary>
        /// Обработчик события - нажатие на кнопку "Обновить"
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        private void btnRefresh_OnClick (object obj, EventArgs ev)
        {
            //Инициировать запрос К БД_значений со списком событий
            // за указанную дату
            // в период указанных часов
            //m_viewAlarm.Push(this, new object[] { new object[] { new object[] { ViewAlarm.StatesMachine.List, DatetimeCurrent, HourBegin, HourEnd } } });
            //DataAskedHost(new object[] { ViewAlarm.StatesMachine.List, DatetimeCurrent, HourBegin, HourEnd });
            DataAskedHost(new object[] { ViewAlarm.StatesMachine.List, DatetimeCurrent, HourBegin, HourEnd });
        }

        private void onEventDateChanged(object obj, DateRangeEventArgs ev)
        {
            //End.Date - эквивалентно, при 'MaxSelectionCount = 1'
            //m_viewAlarm.Push(this, new object[] { new object[] { new object[] { ViewAlarm.StatesMachine.List, DatetimeCurrent, HourBegin, HourEnd } } });
            DataAskedHost(new object[] { ViewAlarm.StatesMachine.List, DatetimeCurrent, HourBegin, HourEnd });
        }
        /// <summary>
        /// Событие запроса данных для плюг'ина из главной формы
        /// </summary>
        public event DelegateObjectFunc EvtDataAskedHost;
        /// <summary>
        /// Отиравить запрос на получение данных
        /// </summary>
        /// <param name="par">Аргумент с детализацией запрашиваемых данных</param>
        public void DataAskedHost(object par)
        {
            m_viewAlarm.Push(this, new object[] { new object[] { par } });
        }
        /// <summary>
        /// Обработчик события ответа от главной формы
        /// </summary>
        /// <param name="obj">объект класса 'EventArgsDataHost' с идентификатором/данными из главной формы</param>
        public void OnEvtDataRecievedHost(object res)
        {
            ViewAlarm.StatesMachine state = (ViewAlarm.StatesMachine)(res as object[])[0];

            switch (state)
            {
                case ViewAlarm.StatesMachine.List:
                    (Find(INDEEX_CONTROL.DGV_EVENTS) as DataGridViewAlarmJournal).OnEvtGetData((res as object[])[1]);
                    break;
                case ViewAlarm.StatesMachine.Insert:
                    //Получить идентификатор записи о событии сигнализации
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Класс с таблицей отображения списка событий сигнализаций
        /// </summary>
        private class DataGridViewAlarmJournal : DataGridView
        {
            /// <summary>
            /// Перечисление для индексов столбцов в таблице
            /// </summary>
            public enum iINDEX_COLUMN
            {
                TECCOMPONENT_NAMESHR, TYPE_ALARM, VALUE, DATETIME_REGISTRED, DATETIME_FIXED, DATETIME_CONFIRM,
                BTN_CONFIRM
                    , COUNT_INDEX_COLUMN
            }
            /// <summary>
            /// Делегат обновления значений в таблице
            /// </summary>
            private DelegateObjectFunc delegateOnGetData;
            /// <summary>
            /// Список идентификатор записей, отображаемых в таблице (поле [ID] в целевой таблице БД)
            /// </summary>
            private List <long> m_listIdRows;
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public DataGridViewAlarmJournal()
                : base()
            {
                InitializeComponent();
                //Создать объект списка с идентификаторами событий
                m_listIdRows = new List<long> ();
                //Создать делегат передачи в текущий поток кода по обновлению значений в таблице
                delegateOnGetData = new DelegateObjectFunc (onEvtGetData);
            }
            /// <summary>
            /// Установить параметры визуализации
            /// </summary>
            private void InitializeComponent()
            {
                //Объект 'столбец' - для добавления в таблицу
                DataGridViewColumn column = null;
                ////Объект 'режим отображения заголовка столбца'
                //DataGridViewAutoSizeColumnMode autoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                //Массив строк для заголовков столбцов
                string [] arHeaderText = { @"Компонент", @"Тип сигнализации", @"Значение", @"Время регистрации", @"Время фиксации", @"Время подтверждения", @"Подтверждение" };
                //Добавить столбцы в таблицу
                for (int i = 0; i < (int)iINDEX_COLUMN.COUNT_INDEX_COLUMN; i++)
                {
                    switch ((iINDEX_COLUMN)i)
                    {
                        case iINDEX_COLUMN.TECCOMPONENT_NAMESHR:
                        case iINDEX_COLUMN.TYPE_ALARM:
                        case iINDEX_COLUMN.VALUE:
                        case iINDEX_COLUMN.DATETIME_REGISTRED:
                        case iINDEX_COLUMN.DATETIME_FIXED:
                        case iINDEX_COLUMN.DATETIME_CONFIRM:
                            //Текстовое поле
                            column = new DataGridViewTextBoxColumn();
                            break;
                        case iINDEX_COLUMN.BTN_CONFIRM:
                            //Кнопка с возможностью изменения свойства 'Enabled'
                            column = new DataGridViewDisableButtonColumn();
                            break;
                        default:
                            break;
                    }
                    //Установить текст заголовка столбца
                    column.HeaderText = arHeaderText[i];
                    //Установить режим отображения значений в столбце
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    //Добавить столбец
                    this.Columns.Add(column);
                }
                //Установить режим отображения загловков в столбцах
                this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                this.AllowUserToAddRows = false; //Запретить пользователю добавлять строки
                this.AllowUserToDeleteRows = false; //Запретить пользователю удалять строки
                this.AllowUserToResizeRows = false; //Запретить пользователю изменять высоту строки
                this.RowHeadersVisible = false; //Отменить отображение заголовков для строк
                this.ReadOnly = true; //Установить режим - 'только чтение'
            }
            /// <summary>
            /// Обработчик события "Получение данных для отображения"
            /// </summary>
            /// <param name="obj">Объект - таблица с данными для отображения</param>
            public void OnEvtGetData (object obj)
            {
                //Перенести выполнение в текущий поток (для доступа к элементу управления)
                Invoke(delegateOnGetData, obj);
            }
            /// <summary>
            /// Отобразить полученные данные
            /// </summary>
            /// <param name="obj">Объект - таблица с данными для отображения</param>
            private void onEvtGetData (object obj)
            {
                DataTable tableRes = obj as DataTable;
                int indxRow = -1;
                //Очистить содержимое таблицы
                Rows.Clear();
                m_listIdRows.Clear ();
                //Добавить строки
                foreach (DataRow r in tableRes.Rows)
                {
                    indxRow = Rows.Add(new object[] {
                        r[@"ID_COMPONENT"]
                        , r[@"TYPE"]
                        , r[@"VALUE"]
                        , r[@"DATETIME_REGISTRED"]
                        , r[@"DATETIME_FIXED"]
                        , r[@"DATETIME_CONFIRM"]
                    });
                    m_listIdRows.Add ((long)r[@"ID"]);
                    //Установить доступность кнопки "Подтвердить"
                    (Rows[indxRow].Cells[this.Columns.Count - 1] as DataGridViewDisableButtonCell).Enabled = false;
                }
            }
        }
    }

    partial class PanelAlarmJournal
    {
        /// <summary>
        /// Перечисление идентификаторов дочерних элементов управления
        /// </summary>
        private enum INDEEX_CONTROL { UNKNOWN = -1
            ,MCLDR_CURRENT, NUD_HOUR_BEGIN, NUD_HOUR_END, BTN_REFRESH, CLB_TECCOMPONENT, NUD_KOEF
            , CBX_WORK
            , DGV_EVENTS };

        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            //this.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            _widthPanelManagement += 2 * Margin.Horizontal;

            Control ctrl = null // объект управления
                , ctrlRel = null; // объект управления, относительно которого размещается текущий объект
            INDEEX_CONTROL indxCurrent = INDEEX_CONTROL.UNKNOWN;
            int posX = -1 // позиция по горизонтали для элемента управления
                , posY = -1 // позиция по вертикали для элемента управления
                , cols = -1 // кол-во столбцов на панели с активными элементами управления
                , widthRel = -1; //ширина относительная столбца на панели с активными элементами управления
            //Установить кол-во контэйнеров-столбцов, контэйнеров-строк
            initializeLayoutStyle (2, 1);
            //Приостановить формирование макета элементов управления
            this.SuspendLayout();

            Panel panelManagement = new Panel();
            panelManagement.Dock = DockStyle.Fill;
            this.Controls.Add(panelManagement, 0, 0);

            posX = Margin.Horizontal;
            posY = Margin.Vertical;
            cols = 4;
            widthRel = (_widthPanelManagement - 2 * Margin.Horizontal) / 4;
            
            indxCurrent = INDEEX_CONTROL.MCLDR_CURRENT;
            ctrl = new System.Windows.Forms.MonthCalendar();
            ctrl.Name = indxCurrent.ToString();
            ctrl.Location = new System.Drawing.Point(posX, posY);
            ctrl.Anchor = (AnchorStyles)((AnchorStyles.Left | AnchorStyles.Top) | AnchorStyles.Right);
            (ctrl as MonthCalendar).MaxSelectionCount = 1;
            //(ctrl as MonthCalendar).ShowToday = false;
            //(ctrl as MonthCalendar).ShowTodayCircle = false;
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEEX_CONTROL.UNKNOWN;
            ctrl = new Label();
            ctrl.Location = new System.Drawing.Point (posX, posY += 172);
            ctrl.Size = new System.Drawing.Size(widthRel / 2, ctrl.Height);
            ctrl.Text = @"c";
            (ctrl as Label).TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEEX_CONTROL.NUD_HOUR_BEGIN;
            ctrl = new NumericUpDown();
            ctrl.Name = indxCurrent.ToString();
            ctrl.Location = new System.Drawing.Point(posX += widthRel / 2, posY);
            ctrl.Size = new System.Drawing.Size(widthRel + widthRel / 2, ctrl.Height);
            (ctrl as NumericUpDown).Minimum = 0;
            (ctrl as NumericUpDown).Maximum = 23;
            (ctrl as NumericUpDown).Enabled = false;
            (ctrl as NumericUpDown).ReadOnly = true;
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEEX_CONTROL.UNKNOWN;
            ctrl = new Label();
            ctrl.Location = new System.Drawing.Point(posX += widthRel + widthRel / 2, posY);
            ctrl.Size = new System.Drawing.Size(widthRel / 2, ctrl.Height);
            ctrl.Text = @"до";
            (ctrl as Label).TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEEX_CONTROL.NUD_HOUR_END;
            ctrl = new NumericUpDown();
            ctrl.Name = indxCurrent.ToString();
            ctrl.Location = new System.Drawing.Point(posX += widthRel / 2, posY);
            ctrl.Size = new System.Drawing.Size(widthRel + widthRel / 2, ctrl.Height);
            (ctrl as NumericUpDown).Minimum = 1;
            (ctrl as NumericUpDown).Maximum = 24;
            (ctrl as NumericUpDown).Value = 24;
            (ctrl as NumericUpDown).Enabled = false;
            (ctrl as NumericUpDown).ReadOnly = true;
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEEX_CONTROL.BTN_REFRESH;
            ctrlRel = Find(INDEEX_CONTROL.NUD_HOUR_END);
            ctrl = new Button();
            ctrl.Name = indxCurrent.ToString();
            ctrl.Location = new System.Drawing.Point(0, posY = ctrlRel.Location.Y + ctrlRel.Height + Margin.Vertical);
            ctrl.Size = new System.Drawing.Size(_widthPanelManagement - Margin.Horizontal, ctrl.Height);
            ctrl.Text = @"Обновить";
            (ctrl as Button).Click += new EventHandler(btnRefresh_OnClick);
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEEX_CONTROL.CLB_TECCOMPONENT;
            ctrlRel = Find(INDEEX_CONTROL.BTN_REFRESH);
            ctrl = new CheckedListBox ();
            ctrl.Name = indxCurrent.ToString();
            ctrl.Location = new System.Drawing.Point(0, posY = ctrlRel.Location.Y + ctrlRel.Height + Margin.Vertical);
            ctrl.Size = new System.Drawing.Size(_widthPanelManagement - Margin.Horizontal, _widthPanelManagement / 1);
            //(ctrl as CheckedListBox).CheckOnClick = true;
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEEX_CONTROL.UNKNOWN;
            ctrlRel = Find(INDEEX_CONTROL.CLB_TECCOMPONENT);
            ctrl = new Label();
            ctrl.Location = new System.Drawing.Point(posX = Margin.Horizontal, posY = ctrlRel.Location.Y + ctrlRel.Height);
            ctrl.Size = new System.Drawing.Size(5 * widthRel / 2, ctrl.Height);
            ctrl.Text = @"Коэффициент";
            (ctrl as Label).TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEEX_CONTROL.NUD_KOEF;
            ctrl = new NumericUpDown();
            ctrl.Name = indxCurrent.ToString();
            ctrl.Location = new System.Drawing.Point(posX += 5 * widthRel / 2, posY);
            ctrl.Size = new System.Drawing.Size(widthRel + widthRel / 2, ctrl.Height);
            (ctrl as NumericUpDown).ReadOnly = true;
            (ctrl as NumericUpDown).TextAlign = HorizontalAlignment.Right;
            (ctrl as NumericUpDown).Increment = 2M;
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEEX_CONTROL.CBX_WORK;
            ctrlRel = Find(INDEEX_CONTROL.NUD_KOEF);
            ctrl = new CheckBox();
            ctrl.Name = indxCurrent.ToString();
            ctrl.Location = new System.Drawing.Point(posX = Margin.Horizontal, posY = ctrlRel.Location.Y + ctrlRel.Height + Margin.Vertical);            
            ctrl.Text = @"Включено";
            panelManagement.Controls.Add(ctrl);

            ctrl = new DataGridViewAlarmJournal();
            ctrl.Name = INDEEX_CONTROL.DGV_EVENTS.ToString();
            ctrl.Dock = DockStyle.Fill;
            this.Controls.Add(ctrl, 1, 0);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
