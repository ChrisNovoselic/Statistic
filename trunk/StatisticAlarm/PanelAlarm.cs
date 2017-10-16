﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Data;

using System.Windows.Forms; //..., CheckBox

using HClassLibrary;
using StatisticCommon;

namespace StatisticAlarm
{
    /// <summary>
    /// Перечисление для режимов работы вкладки
    /// </summary>
    public enum MODE { SERVICE, ADMIN, VIEW };
    /// <summary>
    /// Класс панели для отображения с списка событий
    /// </summary>
    public partial class PanelAlarm : PanelStatistic, IDataHost, IDisposable
    {
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
        ///  , чтения/записи списка событий в БД
        /// </summary>
        private AdminAlarm m_adminAlarm;
        /// <summary>
        /// Событие изменения даты, начала и окончания для запроса списка событий сигнализаций
        /// </summary>
        private AdminAlarm.DatetimeCurrentEventHandler delegateDatetimeChanged;
        /// <summary>
        /// Делегат для обработки события установка/снятие признака "Включено"
        ///  для всех режимов работы (SERVICE, ADMIN, VIEW)
        /// </summary>
        private DelegateBoolFunc delegateWorkCheckedChanged;
        /// <summary>
        /// Событие для оповещения пользователя о событии сигнализаций (новое/повтор)
        /// </summary>
        public event AlarmNotifyEventHandler EventGUIReg;
        /// <summary>
        /// Ширина панели для размещения активных элементов управления
        /// </summary>
        private int _widthPanelManagement = 166;

        private event DelegateObjectFunc eventFilter;

        //public PanelAlarm(MODE mode)
        //{
        //    //Зарегистрировать соединение/получить идентификатор соединения
        //    int iListenerId = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");

        //    DbSources.Sources().UnRegister(iListenerId);
        //}

        /// <summary>
        /// Конструктор - основной (с параметрами)
        /// </summary>
        /// <param name="iListenerConfigDB">Идентификатор установленного соединения с БД</param>
        /// <param name="markQueries">Массив признаков кстановления связи с тем или иным источником данных</param>
        /// <param name="mode">Режим работы панели</param>
        /// <param name="backColor">Цвет фона панели и всех дочерних элементов управления</param>
        public PanelAlarm (int iListenerConfigDB, HMark markQueries, MODE mode, Color backColor)
            : base(MODE_UPDATE_VALUES.AUTO, backColor)
        {
            //Инициализация собственных значений
            initialize(iListenerConfigDB, markQueries, mode);

            findControl(INDEX_CONTROL.DGV_EVENTS.ToString()).BackColor =
            findControl (INDEX_CONTROL.DGV_DETAIL.ToString ()).BackColor =
                backColor;
        }

        /// <summary>
        /// Конструктор - дополнительный (с параметрами)
        /// </summary>
        /// <param name="container">См. документацию на 'Control'</param>
        /// <param name="iListenerConfigDB">Идентификатор установленного соединения с БД</param>
        /// <param name="markQueries">Массив признаков кстановления связи с тем или иным источником данных</param>
        /// <param name="mode">Режим работы панели</param>
        public PanelAlarm (IContainer container, int iListenerConfigDB, HMark markQueries, MODE mode, Color backColor)
            : base (MODE_UPDATE_VALUES.AUTO, backColor)
        {
            container.Add(this);
            //Инициализация собственных значений
            initialize(iListenerConfigDB, markQueries, mode);
        }

        /// <summary>
        /// Инициализация собственных параметров
        /// </summary>
        /// <param name="iListenerConfigDB">Идентификатор установленного соединения с БД</param>
        /// <param name="markQueries">Массив признаков кстановления связи с тем или иным источником данных</param>
        /// <param name="mode">Режим работы панели</param>
        private int initialize(int iListenerConfigDB, HMark markQueries, MODE mode)
        {
            int err = -1 //Признак выполнения метода/функции
                         ////Зарегистрировать соединение/получить идентификатор соединения
                         //, iListenerId = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB")
                ;
            bool bWorkChecked = false;
            ConnectionSettings connSett;

            //Инициализация визуальных компонентов
            InitializeComponent();

            try {
                bWorkChecked = (HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.AUTO_ALARM_KOMDISP)
                    || (mode == MODE.SERVICE)) && (! (mode == MODE.VIEW));
                //Инициализация списка с ТЭЦ
                m_list_tec = new InitTEC_200(iListenerConfigDB, true, new int[] { 0, (int)TECComponent.ID.LK }, false).tec;
                //Инициализация
                connSett = new ConnectionSettings(InitTECBase.getConnSettingsOfIdSource(iListenerConfigDB
                        , FormMainBase.s_iMainSourceData
                        , -1
                        , out err).Rows[0]
                    , -1);

                initAdminAlarm(connSett
                    , mode
                    , markQueries
                    , bWorkChecked);
            } catch (Exception e) {
                Logging.Logg().Exception(e, string.Format("PanelAlarm::initialize () - отсутствуют строки в результате запроса необходимых значений параметров соединения с БД...")
                    , Logging.INDEX_MESSAGE.NOT_SET);
            }
            
            if (!(err < 0)) {
                //Назначить делегаты при изменении:
                // даты, часов начала, окончания для запроса списка событий
                delegateDatetimeChanged = new AdminAlarm.DatetimeCurrentEventHandler(m_adminAlarm.OnEventDatetimeChanged);
                // признака вкл./выкл.
                delegateWorkCheckedChanged = new DelegateBoolFunc(m_adminAlarm.OnWorkCheckedChanged);

                Control ctrl = null;
                int indx = -1;
                //Заполнить списки
                m_listIdTECComponents = new List<int>();
                ctrl = find(INDEX_CONTROL.CLB_TECCOMPONENT);
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
                //Назначить обработчик для события - изменение состояния переключателя элемента в списке компонентов ТЭЦ
                (ctrl as CheckedListBox).ItemCheck += new ItemCheckEventHandler(fTECComponent_OnItemCheck);
                //Назначить обработчик для события - выбор элемента в списке компонентов ТЭЦ
                (ctrl as CheckedListBox).SelectedIndexChanged += new EventHandler(fTECComponent_OnSelectedIndexChanged);
                //Выбрать 1-ый элемент ("Все компоненты")
                (ctrl as CheckedListBox).SelectedIndex = 0;
                (ctrl as CheckedListBox).Enabled = true; //mode == MODE.ADMIN
                //Запустить на выполнениие (при необходимости) таймер для обновления значений в таблице
                ctrl = find(INDEX_CONTROL.CBX_WORK) as CheckBox;
                (ctrl as CheckBox).Checked = bWorkChecked;
                (ctrl as CheckBox).Enabled = ! (mode == MODE.VIEW);
                //Назначить обработчик события изменение даты
                (find(INDEX_CONTROL.MCLDR_CURRENT) as MonthCalendar).DateChanged += new DateRangeEventHandler(onEventDateChanged);
                //Назначить обработчик событий при изменении признака "Включено/отключено"
                (ctrl as CheckBox).CheckedChanged += new EventHandler(cbxWork_OnCheckedChanged);            
                //Установить доступность элементов управления по изменению коэффициентов ГТП (вычисление пороговых значений при проверке выполнения условий сигнализации "Тек./мощность ГТП")
                (find(INDEX_CONTROL.NUD_HOUR_BEGIN) as NumericUpDown).Enabled =
                (find(INDEX_CONTROL.NUD_HOUR_END) as NumericUpDown).Enabled =
                     false; //mode == MODE.ADMIN;
                ctrl = find (INDEX_CONTROL.DGV_EVENTS);
                //Установить фильтр отображения - идентификаторы компонентов
                (ctrl as DataGridViewAlarmJournal).SetFilter (INDEX_FILTER.ID, getListIdCheckedIndices ());
                //Назначить обработчик события при получении из БД списка событий (передать список для отображения)
                m_adminAlarm.EvtGetDataMain += new DelegateObjectFunc((ctrl as DataGridViewAlarmJournal).OnEvtGetData);            
                eventFilter += new DelegateObjectFunc((ctrl as DataGridViewAlarmJournal).OnEventFilter);            
                (ctrl as DataGridViewAlarmJournal).EventConfirmed += new DelegateObjectFunc(OnEventConfirm);
                (ctrl as DataGridViewAlarmJournal).EventFixed += new DelegateObjectFunc(OnEventFixed); // только в режиме 'SERVICE'
                (ctrl as DataGridViewAlarmJournal).EventSelected += new DelegateObjectFunc (dgvJournal_OnSelectionChanged);

                ////Назначить обработчика для события - очистить таблицу по требованию (при отсутствии строк в основной таблице)
                //(ctrl as DataGridViewAlarmJournal).EventClearDetail += new DelegateFunc((find (INDEX_CONTROL.DGV_DETAIL) as DataGridViewAlarmDetail).OnEventClear);

                ctrl = find(INDEX_CONTROL.DGV_DETAIL);
                //Установить фильтр отображения - идентификаторы компонентов
                (ctrl as DataGridViewAlarmDetail).SetFilter(INDEX_FILTER.ID, getListIdCheckedIndices());
                m_adminAlarm.EvtGetDataDetail += new DelegateObjectFunc((ctrl as DataGridViewAlarmDetail).OnEvtGetData);
                eventFilter += new DelegateObjectFunc((ctrl as DataGridViewAlarmDetail).OnEventFilter);

                //Старт в любом режиме с учетом '(ctrl as CheckBox).Checked'
                // SERVICE - БД_значений-ДА, таймер-ДА/НЕТ, оповещение (список)-ДА/НЕТ, оповещение (сигнализация)-НЕТ
                // ADMIN - БД_значений-ДА, таймер-ДА/НЕТ, оповещение (список)-ДА/НЕТ, оповещение (сигнализация)-ДА/НЕТ
                // VIEW - БД_значений-ДА, таймер-ДА/НЕТ, оповещение (список)-ДА/НЕТ, оповещение (сигнализация)-НЕТ
                startAdminAlarm();
            } else
                ;

            if (err < 0)
                throw new Exception("PanelAlarm::initialize () - объект не был создан...");
            else
                ;

            return err;
        }
        
        /// <summary>
        /// Запустить на выполнение 
        /// </summary>
        public override void Start()
        {            
            base.Start ();

            ////Отладка
            //EventGUIReg (@"раз-раз");
            //EventGUIReg(@"два-два");
        }

        public override void SetDelegateReport(DelegateStringFunc ferr, DelegateStringFunc fwar, DelegateStringFunc fact, DelegateBoolFunc fclr)
        {
            m_adminAlarm.SetDelegateReport(ferr, fwar, fact, fclr);
        }

        /// <summary>
        /// Запустить на выполнение объект регистрации выполнения условий сигнализаций
        ///  , чтения/записи/обновления списка событий в БД
        /// </summary>
        private void startAdminAlarm()
        {
            //Инициализировать (при необходимости) объект
            if (!(m_adminAlarm == null))
                //Проверить состояние, позволяющее запуск на выполнение 
                if (m_adminAlarm.IsStarted == false)
                    m_adminAlarm.Start(); // запустить на выполнение
                else ;
            else ;
        }     

        /// <summary>
        /// Остановить панель, и все связанные с ней объекты
        /// </summary>
        public override void Stop() 
        {
            //??? останавливать связанные объекты нельзя
            // т.к. даже при закрытой вкладке должно присходить оповещение
            // можно лишь прекратиить обновление таблицы со списком событий
            // , но это, вероятно, уже выполнено при вызове Activate(false)
            m_adminAlarm.Activate(false);
            base.Stop ();
        }

        private void onDisposed(object obj, EventArgs ev)
        {
            //Остановить объект "обзор, регистрация событий"
            if (!(m_adminAlarm == null))
                if (m_adminAlarm.IsStarted == true)
                {
                    m_adminAlarm.Activate(false);
                    m_adminAlarm.Stop();
                }
                else ;
            else
                ;
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
                //Только при изменении состояния базовой панели
                m_adminAlarm.Activate(activate);
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
        private void initAdminAlarm(ConnectionSettings connSett, MODE mode, HMark markQueries, bool bWorkCheked)
        {
            m_adminAlarm = new AdminAlarm(connSett
                , mode
                , new AdminAlarm.DatetimeCurrentEventArgs(DateCurrent, HourBegin, HourEnd)
                , bWorkCheked);
            m_adminAlarm.InitTEC(m_list_tec, markQueries);

            m_adminAlarm.EventAdd += new AlarmNotifyEventHandler(OnViewAlarm_EventAdd);
            m_adminAlarm.EventRetry += new AlarmNotifyEventHandler(OnViewAlarm_EventRetry);

            this.EvtDataAskedHost += new DelegateObjectFunc(m_adminAlarm.OnEvtDataAskedHost);
        }

        /// <summary>
        /// Обработчик события - регистрация события сигнализации из БД!!!
        /// </summary>
        /// <param name="ev">Аргумент события</param>
        private void OnViewAlarm_EventAdd(AlarmNotifyEventArgs ev)
        {
            Console.WriteLine(@"PanelAlarm::OnViewAlarm_EventAdd (id_comp=" + ev.m_id_comp + @", message=" + ev.m_message_shr + @") - ...");

            //(IsHandleCreated/*InvokeRequired*/ == true)
            //{//...для this.BeginInvoke
                EventGUIReg(ev);
            //}
            //else
            //    Logging.Logg().Error(@"PanelAlarm::OnViewAlarm_EventAdd () - ... BeginInvoke (...) - ...", Logging.INDEX_MESSAGE.D_001);
        }

        /// <summary>
        /// Обработчик события - повтор регистрации события сигнализации из БД!!!
        /// </summary>
        /// <param name="ev">Аргумент события</param>
        private void OnViewAlarm_EventRetry(AlarmNotifyEventArgs ev)
        {
            Console.WriteLine(@"PanelAlarm::OnViewAlarm_EventRetry (id_comp=" + ev.m_id_comp + @", message=" + ev.m_message_shr + @") - ...");

            if (IsHandleCreated/*InvokeRequired*/ == true)
            {//...для this.BeginInvoke
                EventGUIReg(ev);
            }
            else
                Logging.Logg().Error(@"PanelAlarm::OnViewAlarm_EventRetry () - ... BeginInvoke (...) - ...", Logging.INDEX_MESSAGE.D_001);
        }

        /// <summary>
        /// Обработчик события - снятие с отображения 'MessageBox'
        ///  информации о событии сигнализаций
        /// </summary>
        /// <param name="obj">Аргумент события - описание события оповещения</param>
        public void OnEventFixed(object obj)
        {
            DataAskedHost (new object [] {
                new object [] {
                    AdminAlarm.StatesMachine.Fixed
                    , obj as AlarmNotifyEventArgs
                }
            });
        }

        /// <summary>
        /// Обработчик события - подтверждение события сигнализаций
        /// </summary>
        /// <param name="obj">Идентификатор события сигнализаций в БД</param>
        public void OnEventConfirm(object obj)
        {
            DataAskedHost(new object[] {
                new object [] {
                    AdminAlarm.StatesMachine.Confirm
                    , obj
                }
            });
        }

        /// <summary>
        /// Обработчик события изменения выбора строки в таблице
        ///  со списком событий сигнализаций
        /// </summary>
        /// <param name="obj">Идентификатор события сигнализаций в БД</param>
        public void dgvJournal_OnSelectionChanged(object obj)
        {
            DataAskedHost(new object[] {
                new object [] {
                    AdminAlarm.StatesMachine.Detail
                    , (long)obj
                }
            });
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

                if (ev.NewValue == CheckState.Checked)
                    eventFilter(new object [] { INDEX_FILTER.ID, getListIdCheckedIndices() });
                else
                    if (ev.NewValue == CheckState.Unchecked)
                    {
                        eventFilter(new object[] { INDEX_FILTER.ID, new List<int>() });
                    }
                    else
                        ;
            }
            else
                eventFilter(new object[] { INDEX_FILTER.ID, getListIdCheckedIndices(ev.Index, ev.NewValue) });
        }

        private List <int> getListIdCheckedIndices ()
        {
            List <int> listRes = new List<int> ();
            CheckedListBox ctrl = find(INDEX_CONTROL.CLB_TECCOMPONENT) as CheckedListBox;
            //Вариант №1
            for (int i = 1; i < ctrl.Items.Count; i++)
                if (ctrl.GetItemChecked(i) == true)
                    listRes.Add(m_listIdTECComponents[i - 1]);
                else
                    ;
            ////Вариант №2
            //foreach (int indx in ctrl.CheckedIndices)
            //    if (indx > 0)
            //        listRes.Add(m_listIdTECComponents[indx - 1]);
            //    else
            //        ;

            return listRes;
        }

        private List <int> getListIdCheckedIndices (int indx, CheckState newState)
        {
            List<int> listRes = new List<int>();

            CheckedListBox ctrl = find(INDEX_CONTROL.CLB_TECCOMPONENT) as CheckedListBox;
            //Вариант №1
            for (int i = 1; i < ctrl.Items.Count; i++)
                if (! (i == indx))
                    if (ctrl.GetItemChecked(i) == true)
                        listRes.Add(m_listIdTECComponents[i - 1]);
                    else
                        ;
                else
                    if (newState == CheckState.Checked)
                        listRes.Add(m_listIdTECComponents[i - 1]);
                    else
                        ;

            return listRes;
        }
        
        /// <summary>
        /// Найти объект-'компонент ТЭЦ' по идентификатору в локальном списке компонентов ТЭЦ
        /// </summary>
        /// <param name="id">Идентификатор для поиска</param>
        /// <returns>Объект-'компонент ТЭЦ'</returns>
        private TECComponent findGTPOfID (int id)
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
        /// Обработчик события изменение признака "Включено/отключено"
        /// </summary>
        /// <param name="obj">Объект, иницировавший событие</param>
        /// <param name="ev">Аргумент события</param>
        public void cbxWork_OnCheckedChanged(object obj, EventArgs ev)
        {
            CheckBox ctrl =
                obj
                //find(INDEX_CONTROL.CBX_WORK)
                    as CheckBox;

            //??? - Активировать объект регистрации/чтения/записи/обновления списка событий
            delegateWorkCheckedChanged(ctrl.Checked);
        }

        /// <summary>
        /// Обработчик события - выбор элемента в списке компонентов ТЭЦ
        /// </summary>
        /// <param name="obj">Объект, инициировавший событие (CheckListBox)</param>
        /// <param name="ev">Аргумент события</param>
        private void fTECComponent_OnSelectedIndexChanged(object obj, EventArgs ev)
        {
            //Получить объект со значением коэфициента для выбранного компонента ТЭЦ
            NumericUpDown ctrl = find(INDEX_CONTROL.NUD_KOEF) as NumericUpDown;
            //Проверить индекс выбранного элемента
            if ((find(INDEX_CONTROL.CLB_TECCOMPONENT) as CheckedListBox).SelectedIndex > 0)
            {//Только для настоящего компонента ТЭЦ, а не виртуального "Все компоненты"
                //Включить (при необходимости) объект со значением коэфициента
                if (ctrl.Enabled == false) ctrl.Enabled = m_adminAlarm.Mode == MODE.ADMIN; else ;
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
            TECComponent comp = findGTPOfID(m_listIdTECComponents[(find(INDEX_CONTROL.CLB_TECCOMPONENT) as CheckedListBox).SelectedIndex - 1]);
            setNudnKoeffAlarmCurPowerValue (comp.m_dcKoeffAlarmPcur);
        }

        /// <summary>
        /// Отобразить значение коэффициента для выбранного элемента в списке "Компоненты ТЭЦ"
        /// </summary>
        /// <param name="value">Значение для отображения</param>
        private void setNudnKoeffAlarmCurPowerValue(decimal value)
        {
            //Получить объект со значением коэффициента для выбранного элемента
            NumericUpDown ctrl = find(INDEX_CONTROL.NUD_KOEF) as NumericUpDown;
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
            TECComponent comp = findGTPOfID(m_listIdTECComponents[(find(INDEX_CONTROL.CLB_TECCOMPONENT) as CheckedListBox).SelectedIndex - 1]);
            // для логгирования
            decimal dcPrevKoeffAlarmPcur = comp.m_dcKoeffAlarmPcur;
            //Запомнить установленное значение "времени выполнения"
            comp.m_dcKoeffAlarmPcur = (obj as NumericUpDown).Value;

            int err = -1
                //Зарегистрировать соединение с БД_конфигарации
                , idListenerConfigDB = DbSources.Sources().Register(FormMain.s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
            //Получить объект соединения с БД_конфигурации
            System.Data.Common.DbConnection dbConn = DbSources.Sources().GetConnection(idListenerConfigDB, out err);
            //Сохранить установленное значение в БД_конфигурации
            DbTSQLInterface.ExecNonQuery(ref dbConn, @"UPDATE [dbo].[GTP_LIST] SET [KoeffAlarmPcur] = " + comp.m_dcKoeffAlarmPcur + @" WHERE [ID] = " + comp.m_id, null, null, out err);
            //Отменить регистрацию соединения
            DbSources.Sources().UnRegister(idListenerConfigDB);

            Logging.Logg().Action(@"PanelAlarm::NudnKoeffAlarmCurPower_ValueChanged () - пред.=" + dcPrevKoeffAlarmPcur + @", текущ.=" + comp.m_dcKoeffAlarmPcur + @" ..."
                , Logging.INDEX_MESSAGE.NOT_SET);
        }

        /// <summary>
        /// Найти дочерний элемент управления по идентификатору
        /// </summary>
        /// <param name="indx">Идентификатор элемента управления</param>
        /// <returns>Дочерний элемент управления</returns>
        private Control find (INDEX_CONTROL indx)
        {
            return findControl(indx.ToString());
        }

        /// <summary>
        /// Текущая (выбранная дата) в элементе управления 'MonthCalendar'
        /// </summary>
        private DateTime DateCurrent { get { return (find(INDEX_CONTROL.MCLDR_CURRENT) as MonthCalendar).SelectionStart.Date; } }
        
        /// <summary>
        /// Текущий (установленный) индекс часа начала периода в указанные сутки
        /// </summary>
        private int HourBegin { get { return (int)(find(INDEX_CONTROL.NUD_HOUR_BEGIN) as NumericUpDown).Value; } }
        
        /// <summary>
        /// Текущий (установленный) индекс часа окончания периода в указанные сутки
        /// </summary>
        private int HourEnd { get { return (int)(find(INDEX_CONTROL.NUD_HOUR_END) as NumericUpDown).Value; } }
        
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
            delegateDatetimeChanged(new AdminAlarm.DatetimeCurrentEventArgs(DateCurrent, HourBegin, HourEnd));
        }

        //private bool isActivateViewAlarm
        //{
        //    get
        //    {
        //        return ((find(INDEX_CONTROL.CBX_WORK) as CheckBox).Checked == true)
        //            && (IsDatetimeToday == true);
        //    }
        //}

        private void onEventDateChanged(object obj, DateRangeEventArgs ev)
        {
            delegateDatetimeChanged(new AdminAlarm.DatetimeCurrentEventArgs(DateCurrent, HourBegin, HourEnd));
            //(find (INDEX_CONTROL.DGV_EVENTS) as DataGridView).Rows.Clear ();
            (find(INDEX_CONTROL.DGV_DETAIL) as DataGridView).Rows.Clear();
        }

        /// <summary>
        /// Применить изменения графического оформления
        /// </summary>
        /// <param name="type">Тип изменений графического оформления</param>
        public override void UpdateGraphicsCurrent (int type)
        {
            //??? ничего не делать
        }

        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }

            set
            {
                base.BackColor = value;

                Control clbTECComponent;

                clbTECComponent = find (INDEX_CONTROL.CLB_TECCOMPONENT);

                if (Equals (clbTECComponent, null) == false)
                    clbTECComponent.BackColor = BackColor;
                else
                    ;
            }
        }

        /// <summary>
        /// Тип фильтра(ограничений)
        /// </summary>
        public enum INDEX_FILTER : uint {
            /// <summary>
            /// Ограничения не устновлены
            /// </summary>
            NOT
            /// <summary>
            /// Ограничения по метке даты/времени регистрации события
            /// </summary>
            , DATETIME_REGISTRED
            /// <summary>
            /// Ограничения по идентификатору компоненту ТЭЦ
            /// </summary>
            , ID
                , ALL }
        
        /// <summary>
        /// Класс для описания фильтра(ограничений) выбранных компонентов ТЭЦ
        /// </summary>
        private class Filter
        {
            private HMark m_mark;

            public DateTime? m_selDate;
            public List<int> m_listIdCheckedIndices;

            public Filter()
            {
                m_mark = new HMark(new int[] { (int)INDEX_FILTER.ID });

                m_selDate = null;
                m_listIdCheckedIndices = new List<int>();
            }

            public bool isVisibled(ViewAlarmBase vaj)
            {
                bool bRes = false;
                int id_comp = -1;

                if (m_mark.IsMarked((int)INDEX_FILTER.ID) == true)
                {
                    if (TECComponent.Mode(vaj.m_id_component) == FormChangeMode.MODE_TECCOMPONENT.GTP)
                        id_comp = vaj.m_id_component;
                    else
                        if (TECComponent.Mode(vaj.m_id_component) == FormChangeMode.MODE_TECCOMPONENT.TG)
                            id_comp = vaj.m_id_owner;
                        else
                            ;

                    if (!(m_listIdCheckedIndices.IndexOf(id_comp) < 0))
                        bRes = true;
                    else
                        ;
                }
                else
                    bRes = true;

                return bRes;
            }
        }        

        private abstract class DataGridViewAlarmBase : DataGridView
        {
            /// <summary>
            /// Делегат обновления значений в таблице
            /// </summary>
            protected DelegateObjectFunc delegateOnGetData;
            /// <summary>
            /// Объект фильтра отображаемых записей
            /// </summary>
            protected Filter m_filter;

            protected static string s_DateTimeFormat = @"HH:mm.ss.fff";
            
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public DataGridViewAlarmBase()
                : base()
            {
                InitializeComponent();
                //Создать делегат передачи в текущий поток кода по обновлению значений в таблице
                delegateOnGetData = new DelegateObjectFunc (onEvtGetData);

                m_filter = new Filter ();
            }
            
            /// <summary>
            /// Установить параметры визуализации
            /// </summary>
            protected virtual void InitializeComponent()
            {
                this.Dock = DockStyle.Fill;

                //Установить режим отображения загловков в столбцах
                this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                this.AllowUserToAddRows = false; //Запретить пользователю добавлять строки
                this.AllowUserToDeleteRows = false; //Запретить пользователю удалять строки
                this.AllowUserToResizeRows = false; //Запретить пользователю изменять высоту строки
                this.MultiSelect = false;
                this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                this.RowHeadersVisible = false; //Отменить отображение заголовков для строк
                this.ReadOnly = true; //Установить режим - 'только чтение'

                //BackColor = HDataGridViewTables.s_dgvCellStyles[(int)HDataGridViewTables.INDEX_CELL_STYLE.COMMON].BackColor;
            }
            
            /// <summary>
            /// Обработчик события "Получение данных для отображения"
            /// </summary>
            /// <param name="obj">Объект - таблица с данными для отображения</param>
            public void OnEvtGetData (object obj)
            {
                //Перенести выполнение в текущий поток (для доступа к элементу управления)
                try
                {                
                    Invoke(delegateOnGetData, obj);
                }
                catch (Exception e)
                {
                    Logging.Logg().Exception(e, @"DataGridViewAlarmBase::OnEvtGetData () - Invoke (obj.Type=" + obj.GetType().Name + @") ...", Logging.INDEX_MESSAGE.NOT_SET);
                }
            }
            
            /// <summary>
            /// Отобразить полученные данные
            /// </summary>
            /// <param name="obj">Объект - таблица с данными для отображения</param>
            protected virtual void onEvtGetData (object obj)
            {
                //Очистить содержимое таблицы
                Rows.Clear();            
            }
            
            /// <summary>
            /// Обработчик события - применить фильтр для строк в таблице
            /// </summary>
            /// <param name="obj">Параметры фильтра ([0] - тип, [1] - значения)</param>
            public virtual void OnEventFilter (object obj)
            {
                SetFilter ((INDEX_FILTER)(obj as object [])[0], (obj as object[])[1]);
                //Проверить необходимость применения фильтра
                if (Rows.Count == 0)
                    // нет строк - ничего не делать
                    return ;
                else
                    ;
            }
            
            /// <summary>
            /// Установить фильтр для отображаемых значений
            /// </summary>
            /// <param name="indxFilter">Тип фильтра</param>
            /// <param name="objFilter">Значения для фильтра указанного типа</param>
            public void SetFilter(INDEX_FILTER indxFilter, object objFilter)
            {
                switch (indxFilter)
                {
                    case INDEX_FILTER.ID:
                        m_filter.m_listIdCheckedIndices = objFilter as List<int>;
                        break;
                    default:
                        break;
                }
            }

            protected int indexOfIdRec(int indxColumn, long id)
            {
                int iRes = -1;

                foreach (DataGridViewRow r in this.Rows)
                {
                    if ((long)r.Cells[indxColumn].Value == id)
                    {
                        iRes = this.Rows.IndexOf(r);
                        break;
                    }
                    else
                        ;
                }

                return iRes;
            }

            public override Color BackColor
            {
                get
                {
                    return base.BackColor;
                }

                set
                {
                    base.BackColor = value == SystemColors.Control ? SystemColors.Window : value; ;

                    for (int j = 0; j < ColumnCount; j++)
                        for (int i = 0; i < RowCount; i++) {
                            if (Columns [j].GetType ().Name.Contains ("button") == false)
                                Rows [i].Cells [j].Style.BackColor = BackColor;
                            else
                                ;
                        }
                }
            }
        }
        
        /// <summary>
        /// Класс для описания события сигнализаций (для отображения в таблице)
        /// </summary>
        public class ViewAlarmBase
        {
            public long m_id; //Идентификатор записи в ьаблице БД
            public int m_id_owner;
            public int m_id_component;
            public float m_value;
            public string m_str_name_shr_component;
        }
        
        /// <summary>
        /// Класс для описания события сигнализаций (для отображения в таблице основной информации о событии)
        /// </summary>
        public class ViewAlarmJournal : ViewAlarmBase
        {
            public INDEX_TYPE_ALARM m_type;
            public string m_str_name_shr_type;                        
            public int m_id_user_registred;
            public DateTime? m_dt_registred;
            public int m_id_user_fixed;
            public DateTime? m_dt_fixed;
            public int m_id_user_confirmed;
            public DateTime? m_dt_confirmed;
            public int m_situation;
        }
        
        /// <summary>
        /// Класс с таблицей отображения списка событий сигнализаций
        /// </summary>
        private class DataGridViewAlarmJournal : DataGridViewAlarmBase
        {
            //public event DelegateFunc EventClearDetail;

            Dictionary <long, ViewAlarmJournal>  m_dictView;
            /// <summary>
            /// Событие для инициирования процессса подтверждения события сигнализаций
            /// </summary>
            public event DelegateObjectFunc EventConfirmed
                , EventSelected;
            /// <summary>
            /// Событие фиксации зарегистрированного события (только в режиме 'SERVICE')
            /// </summary>
            public event DelegateObjectFunc EventFixed;
            /// <summary>
            /// Режим работы "родительской" панели
            /// </summary>
            private MODE _mode { get { return (Parent.Parent as PanelAlarm).m_adminAlarm.Mode; } }
            /// <summary>
            /// Перечисление для индексов столбцов в таблице
            /// </summary>
            private enum iINDEX_COLUMN
            {
                ID_REC/*, ID_COMPONENT, SITUATION*/
                , TECCOMPONENT_NAMESHR, TYPE_ALARM, VALUE, DATETIME_REGISTRED, DATETIME_FIXED, DATETIME_CONFIRM,
                BTN_CONFIRM
                    , COUNT_INDEX_COLUMN
            }
            
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public DataGridViewAlarmJournal()
                : base()
            {
                m_dictView = new Dictionary<long,ViewAlarmJournal> ();
            }
            
            /// <summary>
            /// Установить параметры визуализации
            /// </summary>
            protected override void InitializeComponent()
            {
                //Объект 'столбец' - для добавления в таблицу
                DataGridViewColumn column = null;
                ////Объект 'режим отображения заголовка столбца'
                //DataGridViewAutoSizeColumnMode autoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                //Массив строк для заголовков столбцов
                string [] arHeaderText = { @"ИД_записи_неотображается"
                    , @"Компонент", @"Тип сигнализации", @"Значение", @"Время регистрации", @"Время фиксации", @"Время подтверждения", @"Подтверждение" };
                //Добавить столбцы в таблицу
                for (int i = 0; i < (int)iINDEX_COLUMN.COUNT_INDEX_COLUMN; i++)
                {
                    switch ((iINDEX_COLUMN)i)
                    {
                        case iINDEX_COLUMN.ID_REC:
                        //case iINDEX_COLUMN.ID_COMPONENT:
                        //case iINDEX_COLUMN.SITUATION:
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
                    //Значение идентификатора записи - не отображать
                    switch ((iINDEX_COLUMN)i)
                    {
                        case iINDEX_COLUMN.ID_REC:
                        //case iINDEX_COLUMN.ID_COMPONENT:
                        //case iINDEX_COLUMN.SITUATION:
                            column.Visible = false;
                            break;
                        default:
                            break;
                    }
                    //Установить текст заголовка столбца
                    column.HeaderText = arHeaderText[i];
                    //Установить режим отображения значений в столбце
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    //Добавить столбец
                    int indxCol = this.Columns.Add(column);
                }

                base.InitializeComponent();

                this.CellClick += new DataGridViewCellEventHandler(onCellClick);
                this.SelectionChanged += new EventHandler(onSelectionChanged);
            }
            
            /// <summary>
            /// Возвратить признак доступности кнопки в столбце "Подтверждение"
            /// </summary>
            /// <param name="iRow">Индекс строки</param>
            /// <returns></returns>
            private bool isRecEnabled (int iRow)
            {
                //Кнопка доступна, если: 1) событие сигнализации "зафиксировано"
                ////Вариант №1
                //return ((!this.Rows[iRow].Cells[(int)iINDEX_COLUMN.DATETIME_FIXED].Value.Equals(string.Empty)) || (_mode == MODE.SERVICE))
                //    // 2) событие сигнализации ранее не "подтверждено"
                //    && this.Rows[iRow].Cells[(int)iINDEX_COLUMN.DATETIME_CONFIRM].Value.Equals(string.Empty);
                //Вариант №2
                return ((!(m_dictView[(long)this.Rows[iRow].Cells[(int)iINDEX_COLUMN.ID_REC].Value].m_dt_fixed == null)) || (_mode == MODE.SERVICE))
                    // 2) событие сигнализации ранее не "подтверждено"
                    && (m_dictView[(long)this.Rows[iRow].Cells[(int)iINDEX_COLUMN.ID_REC].Value].m_dt_confirmed == null);
            }
            
            /// <summary>
            /// Отобразить полученные данные
            /// </summary>
            /// <param name="obj">Объект - таблица с данными для отображения</param>
            protected override void onEvtGetData (object obj)
            {
                int indxRow = -1;
                long lPrevIdRecSelected = -1L;
                if (SelectedRows.Count > 0)
                    lPrevIdRecSelected = (long)Rows[SelectedRows[0].Index].Cells[(int)iINDEX_COLUMN.ID_REC].Value;
                else
                    ;
                this.SelectionChanged -= onSelectionChanged;
                //Очистить содержимое таблицы
                base.onEvtGetData(obj);
                m_dictView.Clear ();
                List<ViewAlarmJournal> listView = (obj as List<ViewAlarmJournal>);
                foreach (ViewAlarmJournal r in listView)
                {
                    m_dictView.Add(r.m_id, r);
                    indxRow = Rows.Add(new object[] {
                        r.m_id
                        , r.m_str_name_shr_component
                        , r.m_str_name_shr_type
                        , r.m_value
                        , HDateTime.ToMoscowTimeZone (r.m_dt_registred.GetValueOrDefault()).ToString (s_DateTimeFormat)
                        , (!(r.m_dt_fixed == null)) ? HDateTime.ToMoscowTimeZone (r.m_dt_fixed.GetValueOrDefault()).ToString (s_DateTimeFormat) : string.Empty
                        , (!(r.m_dt_confirmed == null)) ? HDateTime.ToMoscowTimeZone (r.m_dt_confirmed.GetValueOrDefault()).ToString (s_DateTimeFormat) : string.Empty
                    });                    
                    //Установить доступность кнопки "Подтвердить"
                    (Rows[indxRow].Cells[this.Columns.Count - 1] as DataGridViewDisableButtonCell).Enabled = isRecEnabled((listView as List<ViewAlarmJournal>).IndexOf(r));

                    Rows[indxRow].Visible = m_filter.isVisibled (r);
                    Rows [indxRow].DefaultCellStyle.BackColor = BackColor;
                }

                this.SelectionChanged += new EventHandler(onSelectionChanged);

                indxRow = indexOfIdRec((int)iINDEX_COLUMN.ID_REC, lPrevIdRecSelected);
                if (indxRow < 0)
                    if (Rows.Count > 0)
                        indxRow = 0;
                    else
                        ;
                else
                    ;

                if (! (indxRow < 0))
                {
                    if (indxRow == SelectedRows[0].Index)
                        onSelectionChanged (this, EventArgs.Empty);
                    else
                    {
                        CurrentCell = Rows[indxRow].Cells[1];
                        CurrentCell.Selected = true;
                    }
                }
                else
                    ;
            }
            
            /// <summary>
            /// Обработчик события - применить фильтр для строк в таблице
            /// </summary>
            /// <param name="obj">Параметры фильтра ([0] - тип, [1] - значения)</param>
            public override void OnEventFilter (object obj)
            {
                base.OnEventFilter (obj);

                bool bChanged = false //Признак: кол-во строк с признаком "отображать" изменилось
                    , bVisibled = false; //Признак "отображать" для очередной строки
                int indxPrevRowSelected = -1 //Индекс предыдущей выделенной строки
                    , indxCurRowSelected = -1
                    , iCountRowsVisibled = 0 //Кол-во строк с признаком "отображать"
                    , i = -1;
                long lPrevIdRecSelected = -1L; //Идентификатор записи в БД для события, отображаемого в строке
                if (SelectedRows.Count > 0)
                {
                    indxPrevRowSelected = SelectedRows[0].Index;
                    lPrevIdRecSelected = (long)Rows[indxPrevRowSelected].Cells[(int)iINDEX_COLUMN.ID_REC].Value;
                }
                else
                    ;                
                
                foreach (DataGridViewRow r in Rows)
                {
                    //Определить признак "отображать" для очередной строки
                    bVisibled = m_filter.isVisibled (m_dictView[(long)r.Cells[(int)iINDEX_COLUMN.ID_REC].Value]);
                    //Установить признак: кол-во строк с признаком "отображать" изменилось
                    if ((bChanged == false)
                        && (! (r.Visible == bVisibled))
                    )
                        bChanged = true;
                    else
                        ;
                    //Установить признак отображать/не_отображать строке
                    r.Visible = bVisibled;
                    // подсчет кол-ва строк с признаком "отображать"
                    if (bVisibled == true)
                        iCountRowsVisibled ++;
                    else
                        ;
                }

                if (bChanged == true)
                    if (iCountRowsVisibled > 0)
                    {
                        if (!(indxPrevRowSelected < 0))
                            if (indxPrevRowSelected < iCountRowsVisibled)                        
                                indxCurRowSelected = indxPrevRowSelected;
                            else
                                indxCurRowSelected = iCountRowsVisibled - 1;
                        else
                            indxCurRowSelected = 0;

                        if (!(indxCurRowSelected < 0))
                        {
                            if (Rows[indxCurRowSelected].Visible == false)
                            {// поиск строки, имеющей признак "отображать"
                                indxCurRowSelected = findIndexRowToSelected (indxCurRowSelected);
                            }
                            else
                                ;

                            if (! (indxPrevRowSelected == indxCurRowSelected))
                            {
                                CurrentCell = Rows[indxCurRowSelected].Cells[1];
                                CurrentCell.Selected = true;
                            }
                            else
                                onSelectionChanged (this, EventArgs.Empty);
                        }
                        else
                            ;
                    }
                    else
                        ////??? очистить 'DataGridViewDetail'
                        //EventClearDetail ()
                        ;
                else
                    ; // нет изменений
            }
            
            /// <summary>
            /// Поиск индекса строки для указания ее в качестве выбранной
            ///  , единственный критерий - выбранная строка не может имет признак "не отображать"
            /// </summary>
            /// <param name="indxRowNotVisible">Индекс строки, получившей признак "не отображать"</param>
            /// <returns>Индекс строки (-1, если не найдена)</returns>
            private int findIndexRowToSelected (int indxRowNotVisible)
            {
                int iRes = -1
                    , i = -1;

                // вниз
                for (i = indxRowNotVisible + 1; i < Rows.Count; i++)
                    if (Rows[i].Visible == true)
                    {
                        iRes = i;
                        break;
                    }
                    else
                        ;

                if (iRes < 0)
                {// вверх
                    for (i = indxRowNotVisible - 1; !(i < 0); i--)
                        if (Rows[i].Visible == true)
                        {
                            iRes = i;
                            break;
                        }
                        else
                            ;
                }
                else
                    ;

                return iRes;
            }
            
            /// <summary>
            /// Метод для обновления значения (дата/время фиксации/подтверждения) одной записи
            /// </summary>
            /// <param name="pars">Аргумент- массив с идентификатором события, датой/временем фиксации/подтверждения</param>
            public void UpdateRec (object []pars)
            {
                int indxRow = indexOfIdRec((int)iINDEX_COLUMN.ID_REC, (long)pars[1])
                    , indxCol = -1;
                
                if (!(indxRow < 0))
                {
                    switch ((AdminAlarm.StatesMachine)pars[0])
                    {
                        case AdminAlarm.StatesMachine.Fixed:
                            indxCol = (int)iINDEX_COLUMN.DATETIME_FIXED;
                            break;
                        case AdminAlarm.StatesMachine.Confirm:
                            indxCol = (int)iINDEX_COLUMN.DATETIME_CONFIRM;
                            break;
                        default:
                            break;
                    }

                    this.Rows[indxRow].Cells[indxCol].Value = HDateTime.ToMoscowTimeZone((DateTime)pars[2]).ToString(s_DateTimeFormat);

                    //Установить доступность кнопки "Подтвердить"
                    (Rows[indxRow].Cells[this.Columns.Count - 1] as DataGridViewDisableButtonCell).Enabled = isRecEnabled (indxRow);                        
                }
                else
                    Logging.Logg().Error(@"DataGridViewAlarmJournal::OnUpdate () - не нйдена строка для события с ID=" + (long)pars[1], Logging.INDEX_MESSAGE.NOT_SET);
            }
            
            /// <summary>
            /// Обработчик события нажатия кнопки в столбце "Подтверждение"
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие</param>
            /// <param name="ev">Аргумент события</param>
            private void onCellClick (object obj, DataGridViewCellEventArgs ev)
            {
                long id = -1;
                
                if ((ev.ColumnIndex == (int)iINDEX_COLUMN.BTN_CONFIRM)
                    && ((this.Rows[ev.RowIndex].Cells[ev.ColumnIndex] as DataGridViewDisableButtonCell).Enabled == true))
                {
                    id = (long)Rows[ev.RowIndex].Cells[(int)iINDEX_COLUMN.ID_REC].Value;
                    if (this.Rows[ev.RowIndex].Cells[(int)iINDEX_COLUMN.DATETIME_FIXED].Value.Equals(string.Empty) == true)
                    {
                        // очевидно, что событие не зафиксировано (только в режиме 'SERVICE')
                        EventFixed(new AlarmNotifyEventArgs(m_dictView[id].m_id_component
                            , new AlarmNotifyEventArgs.EventReason () { value = m_dictView[id].m_value, UDGe = float.NaN, koeff = decimal.MinusOne }
                            , m_dictView[id].m_dt_registred.GetValueOrDefault()
                            , m_dictView[id].m_situation));
                    }
                    else
                        // очевидно, что событие не подтверждено
                        EventConfirmed(new object [] { id, m_dictView[id].m_id_component, m_dictView[id].m_situation });
                }
                else
                    ; // остальные столбцы не требуют обработки
            }
            
            /// <summary>
            /// Обработчик события изменения выбора строки в таблице
            /// </summary>
            /// <param name="obj">Объект, инициировавший событие (??? this)</param>
            /// <param name="ev">Аргумент события</param>
            private void onSelectionChanged(object obj, EventArgs ev)
            {
                if (this.SelectedRows.Count > 0)
                    EventSelected(Rows[this.SelectedRows[0].Index].Cells[(int)iINDEX_COLUMN.ID_REC].Value as object);
                else
                    ;
            }

            //protected override void onSorted(object obj, EventArgs e)
            //{
            //    iINDEX_COLUMN indx = (iINDEX_COLUMN)SortedColumn.Index;

            //    switch (indx)
            //    {
            //        case iINDEX_COLUMN.TECCOMPONENT_NAMESHR:
            //            m_listView.Sort(delegate(ViewAlarmBase item1, ViewAlarmBase item2) { return item1.m_id_component.CompareTo(item2.m_id_component); });
            //            break;
            //        case iINDEX_COLUMN.VALUE:
            //            m_listView.Sort(delegate(ViewAlarmBase item1, ViewAlarmBase item2) { return item1.m_value.CompareTo(item2.m_value); });
            //            break;
            //        case iINDEX_COLUMN.DATETIME_REGISTRED:
            //            m_listView.Sort(
            //                delegate(ViewAlarmBase item1, ViewAlarmBase item2)
            //                {
            //                    return (item1 as ViewAlarmJournal).m_dt_registred.GetValueOrDefault().CompareTo((item2 as ViewAlarmJournal).m_dt_registred);
            //                }
            //            );
            //            break;
            //        default:
            //            break;
            //    }
            //}
        }

        /// <summary>
        /// Структура для передачи значений для отображения
        ///  в таблицу детализации события сигнализации
        /// </summary>
        public class ViewAlarmDetail : ViewAlarmBase
        {
            public long m_id_event;           
            public DateTime? m_last_changed_at;
        }
        
        /// <summary>
        /// Класс для описания таблицы представления списка событий сигнализаций
        /// </summary>
        private class DataGridViewAlarmDetail : DataGridViewAlarmBase
        {
            Dictionary <long, ViewAlarmDetail> m_dictView;
            /// <summary>
            /// Перечисление для индексов столбцов в таблице
            /// </summary>
            private enum iINDEX_COLUMN
            {
                ID_REC, TECCOMPONENT_NAMESHR, VALUE, LAST_CHANGED_AT
                    , COUNT_INDEX_COLUMN
            }
            /// <summary>
            /// Конструктор - основной (без параметров)
            /// </summary>
            public DataGridViewAlarmDetail()
                : base()
            {
                m_dictView = new Dictionary<long,ViewAlarmDetail> ();
            }
            /// <summary>
            /// Установить параметры визуализации
            /// </summary>
            protected override void InitializeComponent()
            {
                //Объект 'столбец' - для добавления в таблицу
                DataGridViewColumn column = null;
                //Массив строк для заголовков столбцов
                string[] arHeaderText = { @"ИД_записи_неотображается"
                    , @"Компонент", @"Значение", @"Время значения" };
                //Добавить столбцы в таблицу
                for (int i = 0; i < (int)iINDEX_COLUMN.COUNT_INDEX_COLUMN; i++)
                {
                    switch ((iINDEX_COLUMN)i)
                    {
                        case iINDEX_COLUMN.ID_REC:
                        case iINDEX_COLUMN.TECCOMPONENT_NAMESHR:
                        case iINDEX_COLUMN.VALUE:
                        case iINDEX_COLUMN.LAST_CHANGED_AT:
                            //Текстовое поле
                            column = new DataGridViewTextBoxColumn();
                            break;
                        default:
                            break;
                    }
                    //Значение идентификатора записи - не отображать
                    switch ((iINDEX_COLUMN)i)
                    {
                        case iINDEX_COLUMN.ID_REC:
                            column.Visible = false;
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

                base.InitializeComponent();
            }

            protected override void onEvtGetData(object obj)
            {
                int indxRow = -1;
                long lPrevIdRecSelected = -1L;
                if (SelectedRows.Count > 0)
                    lPrevIdRecSelected = (long)Rows[SelectedRows[0].Index].Cells[(int)iINDEX_COLUMN.ID_REC].Value;
                else
                    ;
                //Очистить содержимое таблицы
                base.onEvtGetData(obj);
                m_dictView.Clear ();

                List<ViewAlarmDetail> listView = obj as List<ViewAlarmDetail>;
                foreach (ViewAlarmDetail r in listView)
                {
                    m_dictView.Add (r.m_id, r);
                    indxRow = Rows.Add(new object[] {
                        r.m_id
                        , r.m_str_name_shr_component
                        , r.m_value
                        , r.m_last_changed_at.GetValueOrDefault().ToString (s_DateTimeFormat)
                    });

                    Rows[indxRow].Visible = m_filter.isVisibled (r);
                    Rows [indxRow].DefaultCellStyle.BackColor = BackColor;
                }

                indxRow = indexOfIdRec ((int)iINDEX_COLUMN.ID_REC, lPrevIdRecSelected);
                if (! (indxRow < 0))
                {
                    CurrentCell = Rows[indxRow].Cells[1];
                    CurrentCell.Selected = true;
                }    
                else
                    ;
            }
            /// <summary>
            /// Обработчик события - применить фильтр для строк в таблице
            /// </summary>
            /// <param name="obj">Параметры фильтра ([0] - тип, [1] - значения)</param>
            public override void OnEventFilter (object obj)
            {
                base.OnEventFilter (obj);

                foreach (DataGridViewRow r in Rows)
                    r.Visible = m_filter.isVisibled (m_dictView[(long)r.Cells[(int)iINDEX_COLUMN.ID_REC].Value]);
            }

            public void OnEventClear ()
            {
                Rows.Clear ();
            }
        }
    }

    partial class PanelAlarm
    {
        /// <summary>
        /// Перечисление идентификаторов дочерних элементов управления
        /// </summary>
        private enum INDEX_CONTROL { UNKNOWN = -1
            ,MCLDR_CURRENT, NUD_HOUR_BEGIN, NUD_HOUR_END, BTN_REFRESH, CLB_TECCOMPONENT, NUD_KOEF
            , CBX_WORK
            , DGV_EVENTS, DGV_DETAIL };

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

        #region Код реализации интерфейса 'IDataHost'
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
            //??? почему так много вложенных массивов...
            //m_viewAlarm.Push(this, new object[] { new object[] { par } });
            //EvtDataAskedHost.BeginInvoke(new EventArgsDataHost(-1, new object[] { par }), new AsyncCallback(this.dataRecievedHost), new Random());
            EvtDataAskedHost(new EventArgsDataHost(this, par as object[]));
        }
        /// <summary>
        /// Обработчик события ответа от главной формы
        /// </summary>
        /// <param name="res">Объект класса 'EventArgsDataHost' с идентификатором/данными из главной формы</param>
        public void OnEvtDataRecievedHost(object res)
        {
            EventArgsDataHost ev = res as EventArgsDataHost;
            AdminAlarm.StatesMachine state = (AdminAlarm.StatesMachine)(ev.par as object[])[0];

            switch (state)
            {
                case AdminAlarm.StatesMachine.Detail:
                    //??? Прямой вызов метода-обработчика
                    (find(INDEX_CONTROL.DGV_EVENTS) as DataGridViewAlarmDetail).OnEvtGetData((ev.par as object[])[1]);
                    break;
                case AdminAlarm.StatesMachine.Fixed:
                case AdminAlarm.StatesMachine.Confirm:
                    (find(INDEX_CONTROL.DGV_EVENTS) as DataGridViewAlarmJournal).UpdateRec(ev.par);
                    break;
                default:
                    break;
            }
        }
        #endregion Код реализации интерфейса 'IDataHost'

        #region Код, автоматически созданный конструктором компонентов

        private class PanelView : HPanelCommon
        {
            public PanelView () : base (1, 8)
            {
                initializeLayoutStyleEvenly ();
            }
            
            protected override void initializeLayoutStyle(int cols = -1, int rows = -1)
            {
                throw new NotImplementedException();
            }
        }
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
            INDEX_CONTROL indxCurrent = INDEX_CONTROL.UNKNOWN;
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
            
            indxCurrent = INDEX_CONTROL.MCLDR_CURRENT;
            ctrl = new System.Windows.Forms.MonthCalendar();
            ctrl.Name = indxCurrent.ToString();
            ctrl.Location = new System.Drawing.Point(posX, posY);
            ctrl.Anchor = (AnchorStyles)((AnchorStyles.Left | AnchorStyles.Top) | AnchorStyles.Right);
            (ctrl as MonthCalendar).MaxSelectionCount = 1;
            //(ctrl as MonthCalendar).ShowToday = false;
            //(ctrl as MonthCalendar).ShowTodayCircle = false;
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEX_CONTROL.UNKNOWN;
            ctrl = new Label();
            ctrl.Location = new System.Drawing.Point (posX, posY += 172);
            ctrl.Size = new System.Drawing.Size(widthRel / 2, ctrl.Height);
            ctrl.Text = @"c";
            (ctrl as Label).TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEX_CONTROL.NUD_HOUR_BEGIN;
            ctrl = new NumericUpDown();
            ctrl.Name = indxCurrent.ToString();
            ctrl.Location = new System.Drawing.Point(posX += widthRel / 2, posY);
            ctrl.Size = new System.Drawing.Size(widthRel + widthRel / 2, ctrl.Height);
            (ctrl as NumericUpDown).Minimum = 0;
            (ctrl as NumericUpDown).Maximum = 23;
            (ctrl as NumericUpDown).Enabled = false;
            (ctrl as NumericUpDown).ReadOnly = true;
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEX_CONTROL.UNKNOWN;
            ctrl = new Label();
            ctrl.Location = new System.Drawing.Point(posX += widthRel + widthRel / 2, posY);
            ctrl.Size = new System.Drawing.Size(widthRel / 2, ctrl.Height);
            ctrl.Text = @"до";
            (ctrl as Label).TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEX_CONTROL.NUD_HOUR_END;
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

            indxCurrent = INDEX_CONTROL.BTN_REFRESH;
            ctrlRel = find(INDEX_CONTROL.NUD_HOUR_END);
            ctrl = new Button();
            ctrl.Name = indxCurrent.ToString();
            ctrl.Location = new System.Drawing.Point(0, posY = ctrlRel.Location.Y + ctrlRel.Height + Margin.Vertical);
            ctrl.Size = new System.Drawing.Size(_widthPanelManagement - Margin.Horizontal, ctrl.Height);
            ctrl.Text = @"Обновить";
            (ctrl as Button).Click += new EventHandler(btnRefresh_OnClick);
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEX_CONTROL.CLB_TECCOMPONENT;
            ctrlRel = find(INDEX_CONTROL.BTN_REFRESH);
            ctrl = new CheckedListBox ();
            ctrl.Name = indxCurrent.ToString();
            ctrl.Location = new System.Drawing.Point(0, posY = ctrlRel.Location.Y + ctrlRel.Height + Margin.Vertical);
            ctrl.Size = new System.Drawing.Size(_widthPanelManagement - Margin.Horizontal, _widthPanelManagement / 1);
            //(ctrl as CheckedListBox).CheckOnClick = true;
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEX_CONTROL.UNKNOWN;
            ctrlRel = find(INDEX_CONTROL.CLB_TECCOMPONENT);
            ctrl = new Label();
            ctrl.Location = new System.Drawing.Point(posX = Margin.Horizontal, posY = ctrlRel.Location.Y + ctrlRel.Height);
            ctrl.Size = new System.Drawing.Size(5 * widthRel / 2, ctrl.Height);
            ctrl.Text = @"Коэффициент";
            (ctrl as Label).TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEX_CONTROL.NUD_KOEF;
            ctrl = new NumericUpDown();
            ctrl.Name = indxCurrent.ToString();
            ctrl.Location = new System.Drawing.Point(posX += 5 * widthRel / 2, posY);
            ctrl.Size = new System.Drawing.Size(widthRel + widthRel / 2, ctrl.Height);
            (ctrl as NumericUpDown).ReadOnly = true;
            (ctrl as NumericUpDown).TextAlign = HorizontalAlignment.Right;
            (ctrl as NumericUpDown).Increment = 2M;
            panelManagement.Controls.Add(ctrl);

            indxCurrent = INDEX_CONTROL.CBX_WORK;
            ctrlRel = find(INDEX_CONTROL.NUD_KOEF);
            ctrl = new CheckBox();
            ctrl.Name = indxCurrent.ToString();
            ctrl.Location = new System.Drawing.Point(posX = Margin.Horizontal, posY = ctrlRel.Location.Y + ctrlRel.Height + Margin.Vertical);            
            ctrl.Text = @"Включено";
            panelManagement.Controls.Add(ctrl);
            ////Вариант №1
            //ctrl = new DataGridViewAlarmJournal();
            //ctrl.Name = INDEX_CONTROL.DGV_EVENTS.ToString();            
            //this.Controls.Add(ctrl, 1, 0);
            //Вариант №2
            PanelView panelView = new PanelView();
            ctrl = new DataGridViewAlarmJournal();
            ctrl.Name = INDEX_CONTROL.DGV_EVENTS.ToString();
            panelView.Controls.Add(ctrl, 0, 0); panelView.SetColumnSpan(ctrl, 1); panelView.SetRowSpan(ctrl, 7);
            ctrl = new DataGridViewAlarmDetail();
            ctrl.Name = INDEX_CONTROL.DGV_DETAIL.ToString();
            panelView.Controls.Add(ctrl, 0, 7); panelView.SetColumnSpan(ctrl, 1); panelView.SetRowSpan(ctrl, 1);
            this.Controls.Add(panelView, 1, 0);

            this.ResumeLayout(false);
            this.PerformLayout();

            this.Disposed += new EventHandler(onDisposed);
        }

        #endregion
    }
}
