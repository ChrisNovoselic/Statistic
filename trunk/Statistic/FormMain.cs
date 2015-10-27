using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
//using System..SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Data.Common; //fTimerAppReset () - ...
using System.Media;
using System.IO;

using System.Net;

//???
//using System.Security.Cryptography;

using HClassLibrary;
using StatisticCommon;
using StatisticDiagnostic;
using StatisticTimeSync;
using StatisticAlarm;

namespace Statistic
{
    public partial class FormMain : FormMainBaseWithStatusStrip
    {
        //10001 = ADMIN_KOM_DISP, 10002 = ADMIN_NSS (FormChangeMode)
        private enum ID_ADDING_TAB
        {
            CUR_POWER = 10101, TM_SN_POWER, MONITOR_LAST_MINUTES, SOBSTV_NYZHDY, CUSTOM_2X2_1, CUSTOM_2X3_1,
            DATETIMESYNC_SOURCE_DATA
                , CUSTOM_2X2_2, CUSTOM_2X3_2, CUSTOM_2X2_3, CUSTOM_2X3_3, CUSTOM_2X2_4,
            CUSTOM_2X3_4
                , SOTIASSO, DIAGNOSTIC
        };
        private enum INDEX_CUSTOM_TAB { TAB_2X2, TAB_2X3 };
        private class ADDING_TAB
        {
            public ToolStripMenuItem menuItem;
            public PanelStatistic panel;

            public ADDING_TAB(string name, string text)
            {
                menuItem = new System.Windows.Forms.ToolStripMenuItem();
                menuItem.CheckOnClick = true;
                menuItem.Size = new System.Drawing.Size(243, 22);
                menuItem.Name = name;
                menuItem.Text = text;
                panel = null;
            }
        };
        /// <summary>
        /// Признак процесса авто/загрузки вкладок
        /// для предотвращения сохранения их в режиме "реальное время"
        /// </summary>
        private static bool m_bAutoActionTabs = false;

        public enum ID_ERROR_INIT { UNKNOWN = -1, }
        private enum INDEX_ERROR_INIT { UNKNOWN = 0, }
        private static string[] MSG_ERROR_INIT = { @"Неизвестная причина" };

        private Dictionary<int, Form> m_dictFormFloat;
        private PanelAdmin[] m_arPanelAdmin;
        private List<PanelTecViewBase> m_listStandardTabs;
        private Dictionary<int, ADDING_TAB> m_dictAddingTabs;
        private static ID_ADDING_TAB[,] m_arIdCustomTabs = new ID_ADDING_TAB[,] { { ID_ADDING_TAB.CUSTOM_2X2_1, ID_ADDING_TAB.CUSTOM_2X2_2, ID_ADDING_TAB.CUSTOM_2X2_3, ID_ADDING_TAB.CUSTOM_2X2_4 }
                                                                                , { ID_ADDING_TAB.CUSTOM_2X3_1, ID_ADDING_TAB.CUSTOM_2X3_2, ID_ADDING_TAB.CUSTOM_2X3_3, ID_ADDING_TAB.CUSTOM_2X3_4 }
                                                                            };
        public Passwords m_passwords;
        private FormPassword formPassword;
        private FormSetPassword formSetPassword;
        private FormChangeMode formChangeMode;
        private HMark m_markPrevStatePanelAdmin;
        private int m_prevSelectedIndex;
        public static FormGraphicsSettings formGraphicsSettings;
        public static FormParameters formParameters;
        private FormParametersTG m_formParametersTG;

        //TcpServerAsync m_TCPServer;
        private
            //System.Threading.Timer
            System.Windows.Forms.Timer
                m_timerAppReset;

        public FormMain()
        {
            InitializeComponent();

            ProgramBase.s_iMessageShowUnhandledException = 1;

            //??? как рез-т проверка на запуск нового экземпляра... см. 'Program.cs'
            //m_TCPServer = new TcpServerAsync(IPAddress.Any, 6666);
            //m_TCPServer.delegateRead = ReadAnalyzer;

            //??? как рез-т проверка на запуск нового экземпляра... см. 'Program.cs'
            //if (!(m_TCPServer.Start() == 0)) Abort(@"Запуск дублирующего экземпляра приложения", true, false); else ;
            //m_TCPServer.Stop();

            AdminTS.m_sOwner_PBR = 1; //Признак владельца ПБР - пользователь

            //DelegateGetINIParametersOfID = new StringDelegateIntFunc(GetINIParametersOfID);

            tclTecViews.EventHTabCtrlExClose += delegateOnCloseTab;
            tclTecViews.EventHTabCtrlExFloat += delegateOnFloatTab;
        }

        private string GetINIParametersOfID(int id)
        {
            return formParameters.m_arParametrSetup[id];
        }

        private int Initialize(out string msgError)
        {
            //StartWait ();
            delegateStartWait();

            msgError = string.Empty;
            //MessageBox.Show((IWin32Window)null, @"FormMain::Initialize () - вХод...", @"Отладка!");

            int iRes = 0;
            int i = -1;

            m_prevSelectedIndex = 1; //??? = -1
            m_markPrevStatePanelAdmin = new HMark();

            m_listStandardTabs = new List<PanelTecViewBase>();

            int idListenerConfigDB = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
            //MessageBox.Show((IWin32Window)null, @"FormMain::Initialize () - DbSources.Sources().Register (...)", @"Отладка!");

            try
            {
                //formParameters = new FormParameters_FIleINI("setup.ini");
                formParameters = new FormParameters_DB(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett());

                Logging.LinkId(Logging.INDEX_MESSAGE.D_002, (int)FormParameters.PARAMETR_SETUP.MAINFORMBASE_SETPBRQUERY_LOGPBRNUMBER);
                Logging.LinkId(Logging.INDEX_MESSAGE.D_003, (int)FormParameters.PARAMETR_SETUP.TECVIEW_LOGRECOMENDATIONVAL);
                Logging.LinkId(Logging.INDEX_MESSAGE.D_004, (int)FormParameters.PARAMETR_SETUP.PANELQUICKDATA_LOGDEVIATIONEVAL);
                Logging.LinkId(Logging.INDEX_MESSAGE.D_005, (int)FormParameters.PARAMETR_SETUP.MAINFORMBASE_SETPBRQUERY_LOGQUERY);
                Logging.LinkId(Logging.INDEX_MESSAGE.W_001, (int)FormParameters.PARAMETR_SETUP.TECVIEW_GETCURRENTTMGEN_LOGWARNING);
                Logging.LinkId(Logging.INDEX_MESSAGE.D_001, (int)FormParameters.PARAMETR_SETUP.MAINFORMBASE_CONTROLHANDLE_LOGERRORCREATE);

                Logging.DelegateGetINIParametersOfID = new StringDelegateIntFunc(GetINIParametersOfID);

                updateParametersSetup();                

                //Предустановленные в файле/БД конфигурации
                HUsers.s_REGISTRATION_INI[(int)HUsers.INDEX_REGISTRATION.DOMAIN_NAME] = formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.USERS_DOMAIN_NAME]; //string.Empty; //@"Отладчик";
                HUsers.s_REGISTRATION_INI[(int)HUsers.INDEX_REGISTRATION.ID] = 0; //Неизвестный пользователь
                HUsers.s_REGISTRATION_INI[(int)HUsers.INDEX_REGISTRATION.ID_TEC] = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.USERS_ID_TEC]); //5
                HUsers.s_REGISTRATION_INI[(int)HUsers.INDEX_REGISTRATION.ROLE] = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.USERS_ID_ROLE]); //2;
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"FormMain::Initialize () ... загрузка предустановленных параметров ...");

                msgError = e.Message;
                iRes = -5;
            }

            if (iRes == 0)
            {
                try
                {
                    //Т.к. все используемые члены-данные СТАТИЧЕСКИЕ
                    using (new HStatisticUsers(idListenerConfigDB)) { ; }
                }
                catch (Exception e)
                {
                    if (e is HException)
                    {
                        iRes = ((HException)e).m_code; //-2, -3, -4
                    }
                    else
                    {
                        iRes = -1;
                    }

                    msgError = e.Message;
                }

                if (iRes == 0)
                {
                    s_iMainSourceData = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.MAIN_DATASOURCE]);

                    PanelAlarm.ALARM_USE = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.ALARM_KOMDISP); //True;
                    //if (!(HStatisticUsers.allTEC == 0))
                    //    PanelAdminKomDisp.ALARM_USE = false;
                    //else ;

                    //ИМгструмент администратора
                    параметрыToolStripMenuItem.Enabled =
                    администрированиеToolStripMenuItem.Enabled =
                    рассинхронизацияДатаВремяСерверБДToolStripMenuItem.Enabled =
                    ДиагностикаToolStripMenuItem.Enabled =
                        HStatisticUsers.RoleIsAdmin;

                    HMark markSett = new HMark(Int32.Parse(HStatisticUsers.GetAllowed((int)HStatisticUsers.ID_ALLOWED.AUTO_LOADSAVE_USERPROFILE)));
                    файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Enabled = markSett.IsMarked(0);
                    файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked = markSett.IsMarked(1);

                    //панельГрафическихToolStripMenuItem.Enabled =
                    //выборОбъекты22ToolStripMenuItem.Enabled = 
                    //выборОбъекты23ToolStripMenuItem.Enabled = 
                    //значенияТекущаяМощностьГТПгТЭЦснToolStripMenuItem.Enabled = 
                    //значенияТекущаяМощностьТЭЦгТЭЦснToolStripMenuItem.Enabled = 
                    //мониторингПоследняяМинутаЧасToolStripMenuItem.Enabled =
                    //собственныеНуждыToolStripMenuItem.Enabled =
                    m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].menuItem.Enabled = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUITEM_SETTING_PARAMETERS_SYNC_DATETIME_DB);
                    m_dictAddingTabs[(int)ID_ADDING_TAB.DIAGNOSTIC].menuItem.Enabled = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUITEM_SETTING_PARAMETERS_DIAGNOSTIC);
                    m_dictAddingTabs[(int)ID_ADDING_TAB.SOTIASSO].menuItem.Enabled = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUITEM_VIEW_VALUES_SOTIASSO);

                    //ProgramBase.s_iAppID = Int32.Parse ((string)Properties.Settings.Default [@"AppID"]);
                    ProgramBase.s_iAppID = Int32.Parse((string)Properties.Resources.AppID);

                    //Если ранее тип логирования не был назанчен...
                    if (Logging.s_mode == Logging.LOG_MODE.UNKNOWN)
                    {
                        //назначить тип логирования - БД
                        Logging.s_mode = Logging.LOG_MODE.DB;
                    }
                    else { }

                    if (Logging.s_mode == Logging.LOG_MODE.DB)
                    {
                        //Инициализация БД-логирования
                        int err = -1;
                        HClassLibrary.Logging.ConnSett = new ConnectionSettings(InitTECBase.getConnSettingsOfIdSource(TYPE_DATABASE_CFG.CFG_200, idListenerConfigDB, s_iMainSourceData, -1, out err).Rows[0], 0);
                    }
                    else { }

                    //m_arAdmin = new AdminTS[(int)FormChangeMode.MANAGER.COUNT_MANAGER];
                    m_arPanelAdmin = new PanelAdmin[(int)FormChangeMode.MANAGER.COUNT_MANAGER];

                    HMark markQueries = new HMark();
                    markQueries.Marked((int)CONN_SETT_TYPE.ADMIN);
                    markQueries.Marked((int)CONN_SETT_TYPE.PBR);

                    for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i++)
                    {
                        switch (i)
                        {
                            case (int)FormChangeMode.MANAGER.DISP:
                                m_arPanelAdmin[i] = new PanelAdminKomDisp(idListenerConfigDB, markQueries);
                                ////((PanelAdminKomDisp)m_arPanelAdmin[i]).EventGUIReg += OnPanelAdminKomDispEventGUIReg;
                                //((PanelAdminKomDisp)m_arPanelAdmin[i]).EventGUIReg = new DelegateStringFunc(OnPanelAdminKomDispEventGUIReg);
                                break;
                            case (int)FormChangeMode.MANAGER.NSS:
                                m_arPanelAdmin[i] = new PanelAdminNSS(idListenerConfigDB, markQueries);
                                break;
                            default:
                                break;
                        }

                        m_arPanelAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                        //m_arPanelAdmin[i].SetDelegateWait(new DelegateFunc(StartWait), new DelegateFunc(StopWait), delegateEvent);
                        m_arPanelAdmin[i].SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);

                        foreach (TEC t in m_arPanelAdmin[i].m_list_tec)
                        {
                            t.EventUpdate += new EventHandler(InitTEC_200.OnTECUpdate);
                            t.PerformUpdate(idListenerConfigDB);
                        }
                    }

                    m_bAutoActionTabs = файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked;

                    List<int> listIDs = new List<int>();
                    //if (((HStatisticUsers.RoleIsAdmin == true) || (HStatisticUsers.RoleIsDisp == true)) && (PanelAdminKomDisp.ALARM_USE == true))
                    //if ((HStatisticUsers.IsAllowed ((int)HStatisticUsers.ID_ALLOWED.AUTO_TAB_PBR_KOMDISP) == true) && (PanelAdminKomDisp.ALARM_USE == true))
                    if (HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.AUTO_TAB_PBR_KOMDISP) == true)
                    {
                        m_markPrevStatePanelAdmin.Set((int)FormChangeMode.MANAGER.DISP, true);
                        //listIDs.Add (FormChangeMode.ID_SPECIAL_TAB[(int)FormChangeMode.MANAGER.DISP]);
                        listIDs.Add(FormChangeMode.ID_SPECIAL_TAB[(int)FormChangeMode.MANAGER.DISP]);
                    }
                    else
                        ;

                    //Добавить закладки автоматически...
                    //listIDs.Add(5); listIDs.Add(111);
                    if (m_bAutoActionTabs == true)
                    {
                        string[] ids = HStatisticUsers.GetAllowed((int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE).Split(';');
                        if ((ids.Length > 0) && (ids[0].Equals(string.Empty) == false))
                            foreach (string id in ids)
                                if (listIDs.IndexOf(Int32.Parse(id)) < 0)
                                    listIDs.Add(Int32.Parse(id));
                                else
                                    ;
                        else
                            ;
                    }
                    else
                        ;

                    if (!(formChangeMode == null))
                    {
                        formChangeMode.Dispose();
                        formChangeMode = null;
                    }
                    else
                        ;

                    formChangeMode = new FormChangeMode(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec, listIDs, this.ContextMenuStrip);
                    formChangeMode.ev_сменитьРежим += сменитьРежимToolStripMenuItem_Click;
                    if (сменитьРежимToolStripMenuItem.Enabled == false) сменитьРежимToolStripMenuItem.Enabled = true; else ;

                    m_passwords = new Passwords();
                    formPassword = new FormPassword(m_passwords);
                    formSetPassword = new FormSetPassword(m_passwords);
                    formGraphicsSettings = new FormGraphicsSettings(this, delegateUpdateActiveGui, delegateHideGraphicsSettings);

                    параметрыПриложенияToolStripMenuItem.Enabled = HStatisticUsers.RoleIsAdmin == true;

                    this.Text += @" - " + formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.MAIN_PRIORITY];

                    TecView.SEC_VALIDATE_TMVALUE = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.VALIDATE_TM_VALUE]);

                    if (iRes == 0)
                    {
                        Start(); //Старт 1-сек-го таймера для строки стостояния

                        //createAddingTabs ();

                        stopTimerAppReset();
                        //int msecTimerAppReset = Int32.Parse (formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.APP_VERSION_QUERY_INTERVAL]);
                        m_timerAppReset =
                            //new System.Threading.Timer(new TimerCallback(fTimerAppReset), null, msecTimerAppReset, msecTimerAppReset)
                            new System.Windows.Forms.Timer();
                            ;
                        m_timerAppReset.Interval = ProgramBase.TIMER_START_INTERVAL;
                        m_timerAppReset.Tick += new EventHandler(fTimerAppReset);
                        m_timerAppReset.Start();

                        if (m_bAutoActionTabs == true)
                            fileProfileLoadAddingTab();
                        else
                            ;

                        //сменитьРежимToolStripMenuItem_Click();
                        //formChangeMode.LoadProfile(@"116");
                        string strIDsToLog = string.Empty;
                        listIDs.ForEach(id => strIDsToLog += id.ToString() + ';');
                        Logging.Logg().Action(@"АвтоЗагрузка профайла (" + HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE.ToString() + @"): ids=" + strIDsToLog, Logging.INDEX_MESSAGE.NOT_SET);
                        //С пустой строкой имитация нажатия "Ок"...
                        formChangeMode.LoadProfile(string.Empty);

                        m_bAutoActionTabs = false;
                    }
                    else
                        ;
                }
                else
                    ;
            }
            else
            {
            }

            DbSources.Sources().UnRegister(idListenerConfigDB);

            delegateStopWait();

            return iRes;
        }

        private void stopTimerAppReset()
        {
            if (!(m_timerAppReset == null))
            {
                //m_timerAppReset.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                m_timerAppReset.Stop();
                m_timerAppReset.Dispose();
                m_timerAppReset = null;
            }
            else
            {
        }
        }

        private void update()
        {
            stopTimerAppReset();
            activateTabPage(tclTecViews.SelectedIndex, false);
            MessageBox.Show(this, @"Доступно обновление для приложения..." + Environment.NewLine +
                                    @"Для применения обновления" + Environment.NewLine +
                                    //@"будет произведен останов и повторный запуск на выполнение...",
                                    @"требуется произвести останов и повторный запуск на выполнение...",
                                    @"Обновление!",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Stop);

            //ProgramBase.AppRestart();
            ProgramBase.AppExit();
        }

        private void updateParametersSetup()
        {
            //Параметры записи сообщений лог-а...
            Logging.UpdateMarkDebugLog();

            //Параметры обновления "основной панели"...
            PanelStatistic.POOL_TIME = Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.POLL_TIME]);
            PanelStatistic.ERROR_DELAY = Int32.Parse(FormMain.formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ERROR_DELAY]);

            //Параметры перехода на сезонное времяисчисление...
            HAdmin.SeasonDateTime = DateTime.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.SEASON_DATETIME]);
            HAdmin.SeasonAction = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.SEASON_ACTION]);

            //Параметры обработки запросов к БД...
            DbInterface.MAX_RETRY = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.MAX_ATTEMPT]);
            DbInterface.MAX_WAIT_COUNT = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.WAITING_COUNT]);
            DbInterface.WAIT_TIME_MS = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.WAITING_TIME]);

            //Параметры валидности даты/времени получения данных СОТИАССО...
            TecView.SEC_VALIDATE_TMVALUE = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.VALIDATE_TM_VALUE]);

            //Параметрвы для ALARM...
            StatisticAlarm.AdminAlarm.MSEC_ALARM_TIMERUPDATE = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_TIMER_UPDATE]) * 1000;
            StatisticAlarm.AdminAlarm.MSEC_ALARM_EVENTRETRY = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_EVENT_RETRY]) * 1000;
            StatisticAlarm.AdminAlarm.MSEC_ALARM_TIMERBEEP = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_TIMER_BEEP]) * 1000;
            StatisticAlarm.AdminAlarm.FNAME_ALARM_SYSTEMMEDIA_TIMERBEEP = formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_SYSTEMMEDIA_TIMERBEEP];
        }

        //private void fTimerAppReset(object obj)
        private void fTimerAppReset(object obj, EventArgs ev)
        {
            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
                ProgramBase.ss_MainCultureInfo;

            if (m_timerAppReset.Interval == ProgramBase.TIMER_START_INTERVAL)
            {
                //При 1-ом запуске ожидать один интервал
                m_timerAppReset.Interval = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.APP_VERSION_QUERY_INTERVAL]);
                return;
            }
            else
                ;

            int err = -1
                , idListenerConfigDB = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");

            // прочитать актуальные значения из [setup]
            (formParameters as FormParameters_DB).Update(idListenerConfigDB, out err);
            // прочитать и обновить актуальные индивидуальные групповые (пользовательские) параметры
            using (new HStatisticUsers(idListenerConfigDB)) { ; }
            //InitTEC_200.PerformTECListUpdate(idListenerConfigDB);

            DbSources.Sources().UnRegister(idListenerConfigDB);

            if (err == 0)
            {
                //Динамическое обновление - применение актуальных параметров
                updateParametersSetup();                

                if (HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.APP_AUTO_RESET) == true)
                    if (formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.APP_VERSION].Equals(string.Empty) == false)
                        if (formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.APP_VERSION].Equals(Application.ProductVersion/*StatisticCommon.Properties.Resources.TradeMarkVersion*/) == false)
                        {
                            if (IsHandleCreated/**/ == true)
                                if (InvokeRequired == true)
                                {
                                    /*IAsyncResult iar = */
                                    this.BeginInvoke(new DelegateFunc(update));
                                    //this.EndInvoke (iar);
                                }
                                else
                                    update();
                            else
                                ;

                            //ProgramBase.AppRestart();

                        }
                        else
                            ;
                    else
                        //При ошибке - восстанавливаем значение...
                        ; //formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.APP_VERSION] = strPrevAppVersion;
                else
                    ;
            }
            else
                ; //DbSources.Sources().UnRegister(idListenerConfigDB);
        }

        private int m_iAlarmEventCounter;
        private int AlarmEventCounter { get { return m_iAlarmEventCounter; } set { m_iAlarmEventCounter = value; } }
        SoundPlayer m_sndAlarmEvent;
        private
            //System.Threading.Timer
            System.Windows.Forms.Timer
                m_timerAlarmEvent;

        //private void timerAlarmEvent (object obj)
        private void timerAlarmEvent(object obj, EventArgs ev)
        {
            //System.Media.SystemSounds.Question.Play();
            if (m_sndAlarmEvent == null)
                Console.Beep();
            else
                m_sndAlarmEvent.Play();
        }

        private void messageBoxShow(object text)
        {
            ////Вариант №1
            //MessageBox.Show((string)text, @"Сигнализация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //Вариант №2
            MessageBox.Show((string)text, @"Сигнализация", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);

            this.BeginInvoke(new DelegateFunc(messageBoxHide));
        }

        private void messageBoxHide()
        {
            //bool bContinue = false;

            lock (this)
            {
                m_iAlarmEventCounter--;

                if (m_iAlarmEventCounter == 0)
                {
                    //m_timerAlarmEvent.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    m_timerAlarmEvent.Stop();
                    m_timerAlarmEvent.Dispose();
                    m_timerAlarmEvent = null;

                    if (!(m_sndAlarmEvent == null))
                    {
                        m_sndAlarmEvent.Stop();
                        m_sndAlarmEvent.Dispose();
                        m_sndAlarmEvent = null;
                    }
                    else
                        ;

                    //bContinue = true;

                    //Активация текущей вкладки
                    activateTabPage(tclTecViews.SelectedIndex, true);

                    ////Продолжение работы ...
                    //((PanelAdminKomDisp)m_arPanelAdmin[(int)(int)FormChangeMode.MANAGER.DISP]).EventGUIConfirm();
                }
                else
                    ;
            }

            //if (bContinue == true)
            //{
            //    //Активация текущей вкладки
            //    activateTabPage(tclTecViews.SelectedIndex, true);

            //    //Продолжение работы ...
            //    ((PanelAdminKomDisp)m_arPanelAdmin[(int)(int)FormChangeMode.MANAGER.DISP]).EventGUIConfirm();
            //}
            //else
            //    ;
        }

        private void panelAdminKomDispEventGUIReg(string text)
        {
            lock (this)
            {
                if (m_timerAlarmEvent == null)
                {
                    //Деактивация текущей вкладки
                    activateTabPage(tclTecViews.SelectedIndex, false);

                    string strPathSnd = Environment.GetEnvironmentVariable("windir") + @"\Media\" + StatisticAlarm.AdminAlarm.FNAME_ALARM_SYSTEMMEDIA_TIMERBEEP;
                    if (File.Exists(strPathSnd) == true)
                        m_sndAlarmEvent = new SoundPlayer(strPathSnd);
                    else
                        ;

                    m_timerAlarmEvent =
                        //new System.Threading.Timer(new TimerCallback(timerAlarmEvent), null, 0, AdminAlarm.MSEC_ALARM_TIMERBEEP) //Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_TIMER_BEEP]) * 1000
                        new System.Windows.Forms.Timer();
                    m_timerAlarmEvent.Tick += new EventHandler(timerAlarmEvent);
                    m_timerAlarmEvent.Interval = StatisticAlarm.AdminAlarm.MSEC_ALARM_TIMERBEEP;
                    m_timerAlarmEvent.Start();

                    m_iAlarmEventCounter = 1;
                }
                else
                    m_iAlarmEventCounter++;
            }

            //Поверх остальных окон
            bool bPrevTopMost = this.TopMost;
            this.TopMost = true;
            //Диалоговое окно
            new Thread(new ParameterizedThreadStart(messageBoxShow)).Start(text);
            //Востановить значение по умолчанию
            this.TopMost = bPrevTopMost;
        }

        private void OnPanelAdminKomDispEventGUIReg(string text)
        {
            try
            {
                //panelAdminKomDispEventGUIReg(text);
                if (IsHandleCreated/*InvokeRequired*/ == true)
                    this.BeginInvoke(new DelegateStringFunc(panelAdminKomDispEventGUIReg), text);
                else
                    Logging.Logg().Error(@"FormMain::OnPanelAdminKomDispEventGUIReg () - ... BeginInvoke (panelAdminKomDispEventGUIReg) - ...", Logging.INDEX_MESSAGE.D_001);
            }
            catch (Exception e)
            {
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"FormMain::OnPanelAdminKomDispEventGUIReg (string) - text=" + text);
            }
        }

        void delegateOnCloseTab(object sender, HTabCtrlExEventArgs e)
        {
            //formChangeMode.SetItemChecked(m_ContextMenuStripListTecViews.Items.IndexOfKey(e.TabHeaderText), false);

            //ToolStripItem []items = m_ContextMenuStripListTecViews.Items.Find (e.TabHeaderText, true);
            //formChangeMode.SetItemChecked(m_ContextMenuStripListTecViews.Items.IndexOf(items [0]), false);

            if (tclTecViews.TabPages[e.TabIndex].Controls[0] is PanelTecViewBase)
            {
                formChangeMode.SetItemChecked(e.TabHeaderText, false);
            }
            else
                if (tclTecViews.TabPages[e.TabIndex].Controls[0] is PanelAdminKomDisp)
                    formChangeMode.SetItemChecked(-1, false);
                else
                    if (tclTecViews.TabPages[e.TabIndex].Controls[0] is PanelStatisticDiagnostic)
                    {
                        m_dictAddingTabs[(int)ID_ADDING_TAB.DIAGNOSTIC].menuItem.Checked = false;

                        ДиагностикаToolStripMenuItem.Checked = false;

                        видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.DIAGNOSTIC].panel, "Диагностика"
                         , new bool[] { ((ToolStripMenuItem)m_dictAddingTabs[(int)ID_ADDING_TAB.DIAGNOSTIC].menuItem).Checked, true });
                    }
                    else
                    if (tclTecViews.TabPages[e.TabIndex].Controls[0] is PanelAdminNSS)
                        formChangeMode.SetItemChecked(-2, false);
                    else
                        if (tclTecViews.TabPages[e.TabIndex].Controls[0] is PanelCurPower)
                            m_dictAddingTabs[(int)ID_ADDING_TAB.CUR_POWER].menuItem.Checked = false;
                        else
                            if (tclTecViews.TabPages[e.TabIndex].Controls[0] is PanelTMSNPower)
                                m_dictAddingTabs[(int)ID_ADDING_TAB.TM_SN_POWER].menuItem.Checked = false;
                            else
                                if (tclTecViews.TabPages[e.TabIndex].Controls[0] is PanelLastMinutes)
                                    m_dictAddingTabs[(int)ID_ADDING_TAB.MONITOR_LAST_MINUTES].menuItem.Checked = false;
                                else
                                    if (tclTecViews.TabPages[e.TabIndex].Controls[0] is PanelSobstvNyzhdy)
                                        m_dictAddingTabs[(int)ID_ADDING_TAB.SOBSTV_NYZHDY].menuItem.Checked = false;
                                    else
                                        if (tclTecViews.TabPages[e.TabIndex].Controls[0] is PanelCustomTecView)
                                        {
                                            int key = -1;
                                            ////Вариант №1
                                            //INDEX_CUSTOM_TAB indxTab = getIndexCustomTab (e.TabHeaderText);
                                            //int indxItem = getIndexItemCustomTab (e.TabHeaderText);                                            
                                            //key = (int)m_arIdCustomTabs[(int)indxTab, indxItem];
                                            //Вариант №2
                                            key = e.Id;

                                            m_dictAddingTabs[key].menuItem.Checked = false;
                                        }
                                        else
                                            if (tclTecViews.TabPages[e.TabIndex].Controls[0] is PanelSourceData)
                                            {
                                                //activateTabPage(e.TabIndex, false);
                                                //tclTecViews.TabPages.RemoveByKey(HTabCtrlEx.GetNameTab (e.TabHeaderText));

                                                m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].menuItem.Checked = false;
                                                    рассинхронизацияДатаВремяСерверБДToolStripMenuItem.Checked = false;

                                                                  видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].panel, "Дата/время серверов БД"
                                                                   , new bool[] { ((ToolStripMenuItem)m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].menuItem).Checked, true });

                                            }
                                            else
                                                if (tclTecViews.TabPages[e.TabIndex].Controls[0] is PanelSOTIASSO)
                                                    m_dictAddingTabs[(int)ID_ADDING_TAB.SOTIASSO].menuItem.Checked = false;
                                                else
                                                    ;
        }

        void delegateOnFloatTab(object sender, HTabCtrlExEventArgs e)
        {
            //Проверить создан ли ранее словарь...
            if (m_dictFormFloat == null)
                //Создать массив (размерность из массива с идентификаторами вкладок)...
                m_dictFormFloat = new Dictionary<int, Form>(); //new Form [m_arIdCustomTabs.GetLength(0), m_arIdCustomTabs.GetLength(1)];
            else
                ;

            this.BeginInvoke(new DelegateObjectFunc(showFormFloat), e);
        }

        INDEX_CUSTOM_TAB getIndexCustomTab(string text)
        {
            INDEX_CUSTOM_TAB indxRes = INDEX_CUSTOM_TAB.TAB_2X2; //e.TabHeaderText.Contains(@"2X2") == true
            if (text.Contains(@"2X3") == true)
                indxRes = INDEX_CUSTOM_TAB.TAB_2X3;
            else
                ;

            return indxRes;
        }

        int getIndexItemCustomTab(string text)
        {
            return Int32.Parse(text.Substring(text.Length - 1, 1)) - 1;
        }

        //private int getKeyOfPanel (Panel panel)
        //{
        //    int iRes = -1;

        //    if (panel is PanelCustomTecView)
        //    {
        //        //Определить индексы в массиве
        //        string text = tclTecViews.NameOfItemControl(panel);
        //        INDEX_CUSTOM_TAB indxTab = getIndexCustomTab(text);
        //        int indxItem = getIndexItemCustomTab(text);
        //        iRes = (int)m_arIdCustomTabs[(int)indxTab, indxItem];
        //    }
        //    else
        //    {
        //        if (panel is PanelTecViewBase)
        //        {
        //            iRes = (panel as PanelTecViewBase).m_ID;
        //        }
        //        else
        //        {
        //            throw new Exception(@"FormMain::getKeyOfPanel () - невозможно определить идентификатор панали - неизвестный тип панели...");
        //        }
        //    }

        //    return iRes;
        //}

        private int getKeyFormFloat(Form f)
        {
            int iRes = -1;

            if (m_dictFormFloat.ContainsValue(f) == true)
                foreach (KeyValuePair<int, Form> pair in m_dictFormFloat)
                    if (pair.Value.Equals(f) == true)
                    {
                        iRes = pair.Key;

                        break;
                    }
                    else
                        ;
            else
                ;

            if (iRes < 0)
                throw new Exception(@"FormMain::getKeyFormFloat () - не найден ключ в словаре для формы...");
            else
                ;

            return iRes;
        }

        private void showFormFloat(object obj)
        {
            HTabCtrlExEventArgs ev = obj as HTabCtrlExEventArgs;

            ////Тест...
            //Form formFloat = new FormAbout();
            //formFloat.Text = text;
            //formFloat.FormClosing += new FormClosingEventHandler(FormMain_OnFormFloat_Closing);
            //formFloat.Load += new EventHandler(FormMain_OnFormFloat_Load);
            //formFloat.Show();

            ////Деактивировать, остановить "панель"
            //m_dictAddingTabs[(int)m_arIdCustomTabs[(int)indxTab, indxItem]].panel.Activate(false);
            //m_dictAddingTabs[(int)m_arIdCustomTabs[(int)indxTab, indxItem]].panel.Stop();
            //Получить панель "открепляемой" вкладки
            Panel panel = tclTecViews.TabPages[ev.TabIndex].Controls[0] as Panel;
            //Удалить вкладку с "главного" окна
            tclTecViews.RemoveTabPage(ev.TabIndex);
            //Создать вспомогательное окно...
            FormMainFloat formFloat = null;
            //formFloat = new FormMainFloat(m_dictAddingTabs[(int)m_arIdCustomTabs[(int)indxTab, indxItem]].panel);
            formFloat = new FormMainFloat(panel);
            formFloat.Text = ev.TabHeaderText;
            //Назначить обработчики событий...
            formFloat.delegateFormClosing = FormMain_OnFormFloat_Closing;
            formFloat.delegateFormLoad = FormMain_OnFormFloat_Load;
            //Сохранить значение в массиве "вспомогательных" форм
            //m_dictFormFloat.Add((int)m_arIdCustomTabs[(int)indxTab, indxItem], formFloat);
            int key = -1;
            ////Вариант №1
            //key = getKeyOfPanel(panel);
            //Вариант №2
            if (panel is PanelCustomTecView)
            {
                //Определить индексы в массиве
                string text = string.Empty;
                ////Вариант №1
                //text = tclTecViews.NameOfItemControl(panel);
                //Вариант №2
                text = ev.TabHeaderText;
                INDEX_CUSTOM_TAB indxTab = getIndexCustomTab(text);
                int indxItem = getIndexItemCustomTab(text);
                key = (int)m_arIdCustomTabs[(int)indxTab, indxItem];
            }
            else
            {
                if (panel is PanelTecViewBase)
                {
                    key = (panel as PanelTecViewBase).m_ID;
                }
                else
                {
                    throw new Exception(@"FormMain::showFormFloat () - невозможно определить идентификатор панели - неизвестный тип панели...");
                }
            }
            m_dictFormFloat.Add(key, formFloat);
            //Отобразить окно, установить на нем фокус...
            formFloat.Show(null);
            formFloat.Focus();
        }

        private void FormMain_OnFormFloat_Load(object pars)
        {
            //Параметры
            FormMainFloat formFloat = ((object[])pars)[0] as FormMainFloat;
            EventArgs ev = ((object[])pars)[1] as EventArgs;
            DelegateStringFunc[] arFuncRep = ((object[])pars)[2] as DelegateStringFunc[];
            DelegateBoolFunc fRepClr = ((object[])pars)[3] as DelegateBoolFunc;

            ////Определить индексы в массиве
            //INDEX_CUSTOM_TAB indxTab = getIndexCustomTab(formFloat.Text);
            //int indxItem = getIndexItemCustomTab(formFloat.Text);
            //Назначить новые делегаты для заполнения строки статуса...
            Panel panel = formFloat.GetPanel();
            if (panel is PanelCustomTecView)
                ((PanelCustomTecView)panel).SetDelegateReport(arFuncRep[0], arFuncRep[1], arFuncRep[2], fRepClr);
            else
                if (panel is PanelTecView)
                    ((PanelTecView)panel).m_tecView.SetDelegateReport(arFuncRep[0], arFuncRep[1], arFuncRep[2], fRepClr);
                else
                    throw new Exception(@"FormMain::FormMain_OnFormFloat_Load () - невозможно назначить делегаты обновления строки статуса...");
            //"Стартовать", активировать "панель"...
            //m_dictAddingTabs[(int)m_arIdCustomTabs[(int)indxTab, indxItem]].panel.Start();
            //m_dictAddingTabs[(int)m_arIdCustomTabs[(int)indxTab, indxItem]].panel.Activate(true);            
            ((PanelStatistic)panel).Activate(true);
        }

        private void FormMain_OnFormFloat_Closing(object pars)
        {
            //Параметры
            FormMainFloat formFloat = ((object[])pars)[0] as FormMainFloat;
            Panel panel = formFloat.GetPanel();
            FormClosingEventArgs ev = ((object[])pars)[1] as FormClosingEventArgs;
            ////Определить индексы в массиве
            //INDEX_CUSTOM_TAB indxTab = getIndexCustomTab(formFloat.Text);
            //int indxItem = getIndexItemCustomTab(formFloat.Text);
            int keyTab = getKeyFormFloat(formFloat); //(int)m_arIdCustomTabs[(int)indxTab, indxItem];

            //Проверить авто/закрытие
            if (m_bAutoActionTabs == false)
            {
                //Восстановить старые делегаты для заполнения строки статуса...
                if (panel is PanelCustomTecView)
                    ((PanelCustomTecView)panel).SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);
                else
                    if (panel is PanelTecView)
                        ((PanelTecView)panel).m_tecView.SetDelegateReport(ErrorReport, WarningReport, ActionReport, ReportClear);
                    else
                        throw new Exception(@"FormMain::FormMain_OnFormFloat_Closing () - невозможно определить тип панели...");
                //Добавить вкладку в "основное" окно
                tclTecViews.AddTabPage(formFloat.Text, getKeyFormFloat(formFloat), HTabCtrlEx.TYPE_TAB.FLOAT);
                tclTecViews.TabPages[tclTecViews.TabCount - 1].Controls.Add(formFloat.GetPanel());

                //???Отладка
                Console.WriteLine(@"FormMain::FormMain_OnFormFloat_Closing () - TabCount=" + tclTecViews.TabCount + @", SelectedIndex=" + tclTecViews.SelectedIndex);
                //Проверить кол-во вкладок
                if (tclTecViews.TabCount > 1)
                    activateTabPage(tclTecViews.TabCount - 1, false);
                else
                    ;

                this.Focus();
            }
            else
                ;

            m_dictFormFloat[keyTab] = null;
            m_dictFormFloat.Remove(keyTab);
        }

        private void файлПрофильАвтоЗагрузитьСохранить_CheckedChanged(object sender, EventArgs e)
        {
            файлПрофильЗагрузитьToolStripMenuItem.Enabled =
            файлПрофильСохранитьToolStripMenuItem.Enabled =
                !файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked;

            if ((!(m_timer == null))
                //&& (! (m_timer.Interval == ProgramBase.TIMER_START_INTERVAL))
                )
            {
                HMark markSett = new HMark();
                markSett.Set(0, файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Enabled);
                markSett.Set(1, файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked);

                int idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, CONN_SETT_TYPE.CONFIG_DB.ToString());
                HStatisticUsers.SetAllowed(idListener, (int)HStatisticUsers.ID_ALLOWED.AUTO_LOADSAVE_USERPROFILE, markSett.Value.ToString());
                DbSources.Sources().UnRegister(idListener);
            }
            else
                ; //Загрузка формы...
        }

        private void файлПрофильЗагрузитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_bAutoActionTabs = true;

            fileProfileLoadStandatdTab();

            fileProfileLoadAddingTab();

            m_bAutoActionTabs = false;
        }

        private void fileProfileLoadStandatdTab()
        {
            string ids = HStatisticUsers.GetAllowed((int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE);
            Logging.Logg().Action(@"Загрузка профайла (" + HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE.ToString() + @"): ids=" + ids, Logging.INDEX_MESSAGE.NOT_SET);
            formChangeMode.LoadProfile(ids);
        }

        private void fileProfileLoadAddingTab()
        {
            string ids = HStatisticUsers.GetAllowed((int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS);
            Logging.Logg().Action(@"Загрузка профайла (" + HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS.ToString() + @"): ids=" + ids, Logging.INDEX_MESSAGE.NOT_SET);

            if (ids.Equals(string.Empty) == false)
            {
                string[] arProfie = ids.Split(';');
                foreach (string profile in arProfie)
                {
                    int id = -1;
                    if (profile.IndexOf('=') < 0)
                        id = Int32.Parse(profile);
                    else
                        id = Int32.Parse(profile.Substring(0, profile.IndexOf('=')));

                    if (m_dictAddingTabs.ContainsKey(id) == true)
                    {
                        m_dictAddingTabs[id].menuItem.PerformClick();

                        switch (id)
                        {
                            case (int)ID_ADDING_TAB.CUSTOM_2X2_1:
                            case (int)ID_ADDING_TAB.CUSTOM_2X2_2:
                            case (int)ID_ADDING_TAB.CUSTOM_2X2_3:
                            case (int)ID_ADDING_TAB.CUSTOM_2X2_4:
                            case (int)ID_ADDING_TAB.CUSTOM_2X3_1:
                            case (int)ID_ADDING_TAB.CUSTOM_2X3_2:
                            case (int)ID_ADDING_TAB.CUSTOM_2X3_3:
                            case (int)ID_ADDING_TAB.CUSTOM_2X3_4:
                                ((PanelCustomTecView)m_dictAddingTabs[id].panel).LoadProfile(profile.Substring(profile.IndexOf('=') + 1));
                                break;
                            default: //CUR_POWER, TM_SN_POWER...
                                break;
                        }
                    }
                    else
                    {
                        Logging.Logg().Error(@"FormMain::fileProfileLoadAddingTab () - m_dictAddingTabs не содержит ключ=" + id, Logging.INDEX_MESSAGE.NOT_SET);
                    }
                }
            }
            else
                ;
        }

        private void fileProfileSaveStandardTab()
        {
            int iListenerId = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, CONN_SETT_TYPE.CONFIG_DB.ToString());
            fileProfileSaveStandardTab(iListenerId);
            DbSources.Sources().UnRegister(iListenerId);
        }

        private void fileProfileSaveStandardTab(int idListener)
        {
            //Сохранить список основных вкладок...
            HStatisticUsers.SetAllowed(idListener, (int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE, formChangeMode.SaveProfile());
        }

        private void fileProfileSaveAddingTab()
        {
            int iListenerId = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, CONN_SETT_TYPE.CONFIG_DB.ToString());
            fileProfileSaveAddingTab(iListenerId);
            DbSources.Sources().UnRegister(iListenerId);
        }

        private void fileProfileSaveAddingTab(int idListener)
        {
            string ids = string.Empty;
            ids = string.Empty;

            foreach (int key in m_dictAddingTabs.Keys)
                if (m_dictAddingTabs[key].menuItem.Checked == true)
                {
                    string recTab = string.Empty;
                    switch (key)
                    {
                        case (int)ID_ADDING_TAB.CUSTOM_2X2_1:
                        case (int)ID_ADDING_TAB.CUSTOM_2X2_2:
                        case (int)ID_ADDING_TAB.CUSTOM_2X2_3:
                        case (int)ID_ADDING_TAB.CUSTOM_2X2_4:
                        case (int)ID_ADDING_TAB.CUSTOM_2X3_1:
                        case (int)ID_ADDING_TAB.CUSTOM_2X3_2:
                        case (int)ID_ADDING_TAB.CUSTOM_2X3_3:
                        case (int)ID_ADDING_TAB.CUSTOM_2X3_4:
                            recTab = key.ToString() + @"=" + ((PanelCustomTecView)m_dictAddingTabs[key].panel).SaveProfile();
                            break;
                        default: //CUR_POWER, TM_SN_POWER...
                            recTab = key.ToString();
                            break;
                    }
                    ids += recTab + @";";
                }
                else
                    ;

            if (ids.Length > 0)
                ids = ids.Substring(0, ids.Length - 1);
            else
                ;
            HStatisticUsers.SetAllowed(idListener, (int)HStatisticUsers.ID_ALLOWED.PROFILE_VIEW_ADDINGTABS, ids);
        }

        private void файлПрофильСохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, CONN_SETT_TYPE.CONFIG_DB.ToString());

            //Сохранить список основных вкладок...
            fileProfileSaveStandardTab(idListener);

            //Сохранить список "дополнительных" вкладок...
            fileProfileSaveAddingTab(idListener);

            DbSources.Sources().UnRegister(idListener);
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void stop(object o)
        {
            Stop(o as FormClosingEventArgs);
        }

        private void Stop(FormClosingEventArgs ev)
        {
            int i = -1;

            m_bAutoActionTabs = true;

            if ((!(formChangeMode == null))
                && ((m_markPrevStatePanelAdmin.IsMarked((int)FormChangeMode.MANAGER.DISP) == true)
                    || (m_markPrevStatePanelAdmin.IsMarked((int)FormChangeMode.MANAGER.NSS) == true)))
            //if ((!(formChangeMode == null)) && (formChangeMode.admin_was_checked[(int)FormChangeMode.MANAGER.DISP] || formChangeMode.admin_was_checked[(int)FormChangeMode.MANAGER.NSS]))
            {
                if (!(m_arPanelAdmin == null))
                {
                    for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i++)
                        if (!(m_arPanelAdmin[i] == null))
                            if (m_arPanelAdmin[i].MayToClose() == false)
                                if (!(ev == null))
                                {
                                    ev.Cancel = true;
                                    break;
                                }
                                else
                                    ;
                            else
                            {
                                //if (i == (int)FormChangeMode.MANAGER.DISP) stopAdminAlarm(); else ;

                                m_arPanelAdmin[i].Stop();
                                m_arPanelAdmin[i] = null;
                            }
                        else
                            ;

                    m_arPanelAdmin = null;
                }
                else
                    ;
            }
            else
                //stopAdminAlarm()
                ;

            Stop();

            //??? Закрывыаются все вкладки
            // , но 15 строк выше "админ"-ские закрываются СНОВА
            StopTabPages();

            //if (! (m_TCPServer == null)) {
            //    try {
            //        m_TCPServer.Stop ();
            //        m_TCPServer = null;
            //    } catch (Exception e) {
            //        Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"FormMain::Stop (FormClosingEventArgs...) - m_TCPServer.Stop () - ...");
            //    }
            //} else
            //    ;

            stopTimerAppReset();
        }

        //private void stopAdminAlarm()
        //{
        //    if ((!(m_arPanelAdmin == null)) && (!(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP] == null)) && (m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP] is PanelAdminKomDisp)
        //    //if (i == (int)FormChangeMode.MANAGER.DISP)
        //    && (PanelAdminKomDisp.ALARM_USE == true)
        //    && (!(((PanelAdminKomDisp)m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP]).m_adminAlarm == null)))
        //    {
        //        ((PanelAdminKomDisp)m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP]).m_adminAlarm.Activate(false);
        //        ((PanelAdminKomDisp)m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP]).m_adminAlarm.Stop();
        //    }
        //    else
        //        ;
        //}

        private void StopTabPages()
        {
            if (!(m_listStandardTabs == null))
            {
                clearTabPages(false);
            }
            else
                ;

            foreach (ADDING_TAB tab in m_dictAddingTabs.Values)
            {
                tab.menuItem.Checked = false;
                if (!(tab.panel == null)) tab.panel.Stop(); else ;
            }
        }

        private void clearTabPages(bool bAttachSelIndxChanged)
        {
            //Logging.Logg().Debug(@"FormMain::clearTabPages () - вХод...", Logging.INDEX_MESSAGE.NOT_SET);

            activateTabPage(tclTecViews.SelectedIndex, false);

            int i = -1;
            List<int> listToRemove = new List<int>();

            listToRemove.Clear();
            if (!(m_dictFormFloat == null))
                foreach (KeyValuePair<int, Form> pair in m_dictFormFloat)
                    if (pair.Key < 10000)
                        listToRemove.Add(pair.Key);
                    else
                        ;
            else
                ;

            for (i = listToRemove.Count - 1; !(i < 0); i--)
                m_dictFormFloat[listToRemove[i]].Close();

            listToRemove.Clear();
            foreach (TabPage tab in tclTecViews.TabPages)
            {
                if ((tab.Controls[0] is PanelTecViewBase) || (tab.Controls[0] is PanelAdmin))
                {
                    listToRemove.Add(tclTecViews.TabPages.IndexOf(tab));
                    ((PanelStatistic)tab.Controls[0]).Stop();
                }
                else
                    ;
            }

            tclTecViews.SelectedIndexChanged -= tclTecViews_SelectedIndexChanged;

            for (i = listToRemove.Count - 1; !(i < 0); i--)
                tclTecViews.RemoveTabPage(listToRemove[i]);

            if (bAttachSelIndxChanged == true)
                tclTecViews.SelectedIndexChanged += tclTecViews_SelectedIndexChanged;
            else
                ;

            //selectedTecViews.Clear();

            //Logging.Logg().Debug(@"FormMain::clearTabPages () - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        /// <summary>
        /// Активация текущей вкладки...
        /// </summary>
        private void activateTabPage()
        {
            activateTabPage(tclTecViews.SelectedIndex, true);
        }

        private void activateTabPage(int indx, bool active)
        {
            string strMsgDebug = string.Empty;

            if (!(indx < 0))
            {
                //strMsgDebug = @"FormMain::activateTabPage () - indx=" + indx + @", active=" + active.ToString() + @", type=" + tclTecViews.TabPages[indx].Controls[0].GetType().ToString();
                strMsgDebug = @"FormMain::activateTabPage () - indx=" + indx + @", active=" + active.ToString() + @", name=" + tclTecViews.TabPages[indx].Text;

                //if (tclTecViews.TabPages[indx].Controls[0] is PanelTecViewBase)
                //    ((PanelTecViewBase)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                //else
                //    ////В работе постоянно
                //    if (tclTecViews.TabPages[indx].Controls[0] is PanelCurPower)
                //        ((PanelCurPower)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                //    else
                //        if (tclTecViews.TabPages[indx].Controls[0] is PanelTMSNPower)
                //            ((PanelTMSNPower)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                //        else
                //            if (tclTecViews.TabPages[indx].Controls[0] is PanelLastMinutes)
                //                ((PanelLastMinutes)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                //            else
                //                if (tclTecViews.TabPages[indx].Controls[0] is PanelSobstvNyzhdy)
                //                    ((PanelSobstvNyzhdy)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                //                else
                //                    if (tclTecViews.TabPages[indx].Controls[0] is PanelCustomTecView)
                //                        ((PanelCustomTecView)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                //                    else
                //                        if (tclTecViews.TabPages[indx].Controls[0] is PanelAdmin)
                //                            ((PanelAdmin)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                //                        else
                //                            if (tclTecViews.TabPages[indx].Controls[0] is PanelSourceData)
                //                            {
                //                                ((PanelSourceData)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                //                            }
                //                            else
                //                                ;


                ((HPanelCommon)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
            }
            else
                strMsgDebug = @"FormMain::activateTabPage () - indx=" + indx + @", active=" + active.ToString();

            Logging.Logg().Debug(strMsgDebug + @" - (вЫход)...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        private void ActivateTabPage()
        {
            //selectedTecViews[tclTecViews.SelectedIndex].Activate(true);
            if (!(tclTecViews.SelectedIndex < 0))
                activateTabPage(tclTecViews.SelectedIndex, true);
            else
                ;

            //Деактивация
            if ((!(m_prevSelectedIndex < 0)) && (!(m_prevSelectedIndex == tclTecViews.SelectedIndex)) && (m_prevSelectedIndex < tclTecViews.TabCount))
            {
                activateTabPage(m_prevSelectedIndex, false);
            }
            else
                ;

            m_prevSelectedIndex = tclTecViews.SelectedIndex;
        }

        private void OnEventFileConnSettSave(FIleConnSett.eventFileConnSettSave ev)
        {
            Properties.Settings.Default[@"connsett"] = new string(ev.hash, 0, ev.length);
            Properties.Settings.Default.Save();
        }

        private void FormMain_FormLoad(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
                ProgramBase.ss_MainCultureInfo; //ru-Ru

            //Logging.Logg().Debug(@"FormMain_FormLoad () - ...", Logging.INDEX_MESSAGE.NOT_SET);

            s_fileConnSett = new FIleConnSett(@"connsett.ini", FIleConnSett.MODE.FILE);
            //m_fileConnSett = new FIleConnSett(new string [] {@"connsett", Properties.Settings.Default.Properties[@"connsett"].ToString ()});
            //m_fileConnSett = new FIleConnSett(Properties.Settings.Default.Properties [@"connsett"].DefaultValue.ToString (), FIleConnSett.MODE.SETTINGS);
            //m_fileConnSett = new FIleConnSett((string)Properties.Settings. [@"connsett"], FIleConnSett.MODE.SETTINGS);
            //m_fileConnSett = new FIleConnSett((string)Properties.Settings.Default[@"connsett"], FIleConnSett.MODE.SETTINGS);
            //MessageBox.Show((IWin32Window)null, @"FormMain::FormMain () - new FIleConnSett (...)", @"Отладка!");

            //Только для 'FIleConnSett.MODE.SETTINGS'
            //m_fileConnSett.EventFileConnSettSave += new FIleConnSett.DelegateOnEventFileConnSettSave(OnEventFileConnSettSave);

            string msg = string.Empty;
            bool bAbort = false;

            s_listFormConnectionSettings = new List<FormConnectionSettings>();
            s_listFormConnectionSettings.Add(new FormConnectionSettings(-1, s_fileConnSett.ReadSettingsFile, s_fileConnSett.SaveSettingsFile));
            s_listFormConnectionSettings.Add(null);
            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                switch (Initialize(out msg))
                {
                    case -1:
                        msg = @"Неизвестная причина";
                        break;
                    case -3: //Не найден пользователь
                        //Остальные п.п. меню блокируются в 'сменитьРежимToolStripMenuItem_EnabledChanged'
                        // этот п. блокируется только при конкретной ошибке "-3"
                        this.настройкиСоединенияБДКонфToolStripMenuItem.Enabled = false;
                        break;
                    case -2:
                    case -5:
                    case -4: //@"Необходимо изменить параметры соединения с БД"
                        //Сообщение получено из 'Initialize'
                        break;
                    default:
                        //Успех...
                        break;
                }
            }
            else
            {//Файла с параметрами соединения нет совсем или считанные параметры соединения не валидны
                msg = @"Необходимо изменить параметры соединения с БД конфигурации";
            }

            if (msg.Equals(string.Empty) == false)
                Abort(msg, bAbort);
            else
                ;

            this.Activate();
        }

        public override void Close(bool bForce) 
        { if (bForce == false) base.Close(bForce); 
        else FormMain_FormClosing(this, new FormClosingEventArgs(CloseReason.ApplicationExitCall, true)); }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Logging.Logg().Debug(@"FormMain_FormClosing () - ...", Logging.INDEX_MESSAGE.NOT_SET);

            if (
                //(! (m_TCPServer == null)) ||
                (!(m_arPanelAdmin == null))
                || (!(m_timer == null))
                )
                if (e.Cancel == false)
                    if (e.CloseReason == CloseReason.UserClosing)
                        if (MessageBox.Show(this, "Вы уверены, что хотите закрыть приложение?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        {
                            //Нет, не закрываем
                            activateTabPage(tclTecViews.SelectedIndex, true);

                            //Продолжаем и устанавливаем признак: завершить обработку события 'e'
                            e.Cancel = true;
                        }
                        else
                        {
                            //Да, закрываем; признаку оставляем прежнее значение 'False': продолжить обработку события 'e'
                            if (InvokeRequired == true)
                                this.BeginInvoke(new DelegateObjectFunc(stop), e);
                            else
                                Stop(e);
                        }
                    else
                        //Да, закрываем; признаку оставляем прежнее значение 'False': продолжить обработку события 'e'
                        if (InvokeRequired == true)
                            this.BeginInvoke(new DelegateObjectFunc(stop), e);
                        else
                            Stop(e);
                else
                {
                    //Закрываем и устанавливаем признак: продолжить обработку события 'e'
                    e.Cancel = false;

                    //Да, закрываем; признаку оставляем прежнее значение 'False': продолжить обработку события 'e'
                    if (InvokeRequired == true)
                        this.BeginInvoke(new DelegateObjectFunc(stop), e);
                    else
                        Stop(e);
                }
            else
                ;
        }

        private int connectionSettings(CONN_SETT_TYPE type)
        {
            int iRes = -1;
            DialogResult result;
            result = s_listFormConnectionSettings[(int)type].ShowDialog(this);
            if (result == DialogResult.Yes)
            {
                //??? Закрывыаются все вкладки
                // , но 7 строк ниже "админ"-ские закрываются СНОВА
                StopTabPages();

                base.Stop();

                int i = -1;
                if (!(m_arPanelAdmin == null))
                    for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i++)
                    {
                        if (!(m_arPanelAdmin[i] == null)) m_arPanelAdmin[i].Stop(); else ;
                    }
                else
                    ;

                string msg = string.Empty;
                iRes = Initialize(out msg);
                if (!(iRes == 0))
                    //@"Ошибка инициализации пользовательских компонентов формы"
                    Abort(msg, false);
                else
                    ;

                //foreach (PanelTecViewBase t in m_listStandardTabs)
                //{
                //    t.Reinit();
                //}

                //m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].Reinit();
            }
            else
                ;

            return iRes;
        }

        private bool closeTecViewsTabPages()
        {
            bool bRes = true;

            if (tclTecViews.TabCount > 0)
                if (MessageBox.Show(this, "Вы уверены, что хотите закрыть текущие вкладки?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //StartWait();
                    delegateStartWait();

                    //??? Закрывыаются все вкладки
                    // , но 8 строк ниже "админ"-ские закрываются СНОВА
                    StopTabPages();

                    if (!(m_listStandardTabs == null))
                        for (int i = 0; i < m_listStandardTabs.Count; i++)
                            m_listStandardTabs[i].Stop();
                    else
                        ;

                    m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].Stop();
                    m_arPanelAdmin[(int)FormChangeMode.MANAGER.NSS].Stop();
                    m_markPrevStatePanelAdmin.UnMarked();

                    formChangeMode.btnClearAll_Click(formChangeMode, new EventArgs());

                    delegateStopWait();

                    this.Focus();
                }
                else
                {
                    bRes = false; //e.Cancel = true;
                }
            else
                ;

            return bRes;
        }

        private void настройкиСоединенияБДКонфToolStripMenuItem_Click(object sender, EventArgs e)
        {
            настройкиСоединенияToolStripMenuItem_Click(sender, e, CONN_SETT_TYPE.CONFIG_DB);
        }

        private void настройкиСоединенияБДИсточникToolStripMenuItem_Click(object sender, EventArgs e)
        {
            настройкиСоединенияToolStripMenuItem_Click(sender, e, CONN_SETT_TYPE.LIST_SOURCE);
        }

        private void просмотрЖурналПрограммыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            activateTabPage(tclTecViews.SelectedIndex, false);

            int idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
            FormMainAnalyzer formAnalyzer = new FormMainAnalyzer_DB(idListener, formChangeMode.m_list_tec);
            ////Вариант №1
            //formAnalyzer.FormClosed += new FormClosedEventHandler(formAnalyzerCloused);
            //new Thread(() => formAnalyzer.ShowDialog()).Start();

            //Вариант №2
            formAnalyzer.ShowDialog(this);
            DbSources.Sources().UnRegister(idListener);
            activateTabPage(); //по умолчанию текущая вкладка и 'true'
        }

        ////Вариант №1
        //private void formAnalyzerCloused (object obj, FormClosedEventArgs ev)
        //{
        //    if (IsHandleCreated == true)
        //        if (InvokeRequired == true)
        //            this.BeginInvoke(new DelegateFunc(activateTabPage));
        //        else
        //            activateTabPage();
        //    else
        //        ;
        //}

        private void настройкиСоединенияToolStripMenuItem_Click(object sender, EventArgs e, CONN_SETT_TYPE type)
        {
            if (closeTecViewsTabPages() == true)
            {
                //???
                //string strPassword = "password";
                //MD5CryptoServiceProvider md5;
                //md5 = new MD5CryptoServiceProvider();
                //StringBuilder strPasswordHashed = new StringBuilder ();
                //byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(strPassword));

                bool bShowFormConnectionSettings = false;
                int idListener = -1;
                ConnectionSettingsSource connSettSource;
                if (formPassword == null)
                {
                    bShowFormConnectionSettings = true;
                }
                else
                {
                    if ((!(s_listFormConnectionSettings == null)) &&
                        (s_listFormConnectionSettings[(int)type] == null) && (!(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB] == null)))
                    {
                        DelegateReadConnSettFunc delegateRead = null;
                        DelegateSaveConnSettFunc delegateSave = null;

                        switch (type)
                        {
                            case CONN_SETT_TYPE.CONFIG_DB:
                                delegateRead = s_fileConnSett.ReadSettingsFile;
                                delegateSave = s_fileConnSett.SaveSettingsFile;
                                break;
                            case CONN_SETT_TYPE.LIST_SOURCE:
                                idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
                                //if (m_connSettSource == null)
                                    connSettSource = new ConnectionSettingsSource(idListener);
                                //else
                                //    ;

                                delegateRead = connSettSource.Read;
                                delegateSave = connSettSource.Save;
                                break;
                            default:
                                break;
                        }

                        if ((!(delegateRead == null)) && (!(delegateSave == null)))
                            s_listFormConnectionSettings[(int)type] = new FormConnectionSettings(idListener, delegateRead, delegateSave);
                        else
                            Abort(@"параметры соединения с БД конфигурации", false);
                    }
                    else
                        ;

                    if ((!(s_listFormConnectionSettings[(int)type] == null)) && (!(s_listFormConnectionSettings[(int)type].Ready == 0)))
                    {
                        bShowFormConnectionSettings = true;
                    }
                    else
                    {
                        if (idListener < 0) idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB"); else ;
                        formPassword.SetIdPass(idListener, 0, Passwords.ID_ROLES.ADMIN);
                        DialogResult dlgRes = formPassword.ShowDialog(this);
                        if ((dlgRes == DialogResult.Yes) || (dlgRes == DialogResult.Abort))
                            bShowFormConnectionSettings = true;
                        else
                            ;
                    }
                }

                DbSources.Sources().UnRegister(idListener);

                if (bShowFormConnectionSettings == true)
                    connectionSettings(type);
                else
                    ;
            }
            else
                ;
        }

        private void addPanelTecView(TEC tec, int ti, int ci)
        {
            if (tec.m_bSensorsStrings == false)
                tec.InitSensorsTEC();
            else
                ;

            PanelTecView panelTecView = new PanelTecView(tec, ti, ci, null, ErrorReport, WarningReport, ActionReport, ReportClear);
            panelTecView.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);
            //panelTecView.SetDelegate(new DelegateFunc(StartWait), new DelegateFunc (StopWait), delegateEvent);
            m_listStandardTabs.Add(panelTecView);
        }

        private void сменитьРежимToolStripMenuItem_Click()
        {
            delegateStartWait();

            //StartWait();
            clearTabPages(true);

            Int16 parametrsTGBiysk = 0;
            int tecView_index = -1
                , i = -1;
            //List<int> list_tecView_index_visible = new List<int>();
            //List<int> list_tecView_index_checked = new List<int>();
            // отображаем вкладки ТЭЦ - аналог PanelCustomTecView::MenuItem_OnClick
            for (i = 0; i < formChangeMode.m_listItems.Count; i++) //или TECComponent_index.Count
            {
                if ((formChangeMode.m_listItems[i].bVisibled == true) && (formChangeMode.m_listItems[i].bChecked == true))
                {
                    if (formChangeMode.m_listItems[i].id > 10000)
                        continue;
                    else
                        ;

                    for (tecView_index = 0; tecView_index < m_listStandardTabs.Count; tecView_index++)
                    {
                        if ((m_listStandardTabs[tecView_index].m_ID == formChangeMode.m_listItems[i].id))
                            break;
                        else
                            ;
                    }

                    if (!(tecView_index < m_listStandardTabs.Count))
                    {//Не найден элемент - создаем, добавляем
                        foreach (StatisticCommon.TEC t in formChangeMode.m_list_tec)
                        {
                            if (t.m_id == formChangeMode.m_listItems[i].id)
                            {
                                addPanelTecView(t, formChangeMode.m_list_tec.IndexOf(t), -1);

                                tecView_index = m_listStandardTabs.Count - 1;

                                break;
                            }
                            else
                                ;

                            if (t.list_TECComponents.Count > 0)
                            {
                                foreach (TECComponent g in t.list_TECComponents)
                                {
                                    if (g.m_id == formChangeMode.m_listItems[i].id)
                                    {
                                        addPanelTecView(t, formChangeMode.m_list_tec.IndexOf(t), t.list_TECComponents.IndexOf(g));

                                        tecView_index = m_listStandardTabs.Count - 1;

                                        break;
                                    }
                                    else
                                        ;
                                }
                            }
                            else
                                ;

                            if (tecView_index == (m_listStandardTabs.Count - 1))
                                break;
                            else
                                ;
                        }
                    }
                    else
                        ;

                    if (tecView_index < m_listStandardTabs.Count)
                    {
                        //list_tecView_index_checked.Add(tecView_index);

                        if ((m_listStandardTabs[tecView_index].m_tecView.m_tec.Type == StatisticCommon.TEC.TEC_TYPE.BIYSK)/* && (параметрыТГБийскToolStripMenuItem.Visible == false)*/)
                            parametrsTGBiysk++;
                        else
                            ;

                        tclTecViews.AddTabPage(formChangeMode.m_listItems[i].name_shr, m_listStandardTabs[tecView_index].m_ID, HTabCtrlEx.TYPE_TAB.FLOAT);

                        tclTecViews.TabPages[tclTecViews.TabCount - 1].Controls.Add(m_listStandardTabs[tecView_index]);

                        m_listStandardTabs[tecView_index].Start();
                    }
                    else
                        ;
                }
                else
                {
                }
            }

            //if (!(m_timer.Interval == ProgramBase.TIMER_START_INTERVAL))
            if (m_bAutoActionTabs == false)
                //Сохранить список основных вкладок...
                if (файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked == true)
                    fileProfileSaveStandardTab();
                else
                    ;
            else
                ;

            bool bTGBiysk = parametrsTGBiysk > 0;
            if ((HStatisticUsers.allTEC == 0) || (HStatisticUsers.allTEC == 6))
            {
                параметрыТГБийскToolStripMenuItem.Visible = bTGBiysk;

                //m_formParametersTG = new FormParametersTG_FileINI(@"setup.ini");
                m_formParametersTG = new FormParametersTG_DB(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec);
            }
            else
                ;

            delegateStopWait();

            addTabPagesAdmin();

            ActivateTabPage();
        }

        private void сменитьРежимToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                int i = -1;
                if ((!(formChangeMode == null)) && (formChangeMode.ShowDialog(this) == System.Windows.Forms.DialogResult.OK))
                {
                    // выбираем список отображаемых вкладок
                    сменитьРежимToolStripMenuItem_Click();
                }
                else
                    ;
            }
            else
                ; //Нет соединения с конфигурационной БД
        }

        private void сменитьРежимToolStripMenuItem_EnabledChanged(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            видToolStripMenuItem.Enabled =
            настройкиСоединенияБДИсточникToolStripMenuItem.Enabled =
            просмотрЖурналПрограммыToolStripMenuItem.Enabled =
            изменитьПарольДиспетчераToolStripMenuItem.Enabled =
            изменитьПарольАдминистратораToolStripMenuItem.Enabled =
            изменитьПарольНССToolStripMenuItem.Enabled =
            изментьСоставТЭЦГТПЩУToolStripMenuItem.Enabled =
            изментьСоставПользовательToolStripMenuItem.Enabled =
            параметрыToolStripMenuItem.Enabled =
                item.Enabled;
        }

        private void tclTecViews_SelectedIndexChanged(object sender, EventArgs e)
        {
            //StatisticCommon.FormChangeMode.MANAGER modeAdmin = FormChangeMode.MANAGER.NSS;

            //if (formChangeMode.IsModeTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP) == true)
            //    modeAdmin = FormChangeMode.MANAGER.DISP;
            //else
            //    ;

            ActivateTabPage();
        }

        protected override int UpdateStatusString()
        {
            int have_msg = 0;
            m_lblDescMessage.Text = m_lblDateMessage.Text = string.Empty;
            PanelTecViewBase selTecView = null;

            //for (int i = 0; i < selectedTecViews.Count; i++)
            //if ((selectedTecViews.Count > 0) /*&& (! (m_prevSelectedIndex < 0))*/)
            if ((!(m_prevSelectedIndex < 0)) && (m_prevSelectedIndex < tclTecViews.TabCount))
            {
                if ((tclTecViews.TabPages[m_prevSelectedIndex].Controls.Count > 0) && (tclTecViews.TabPages[m_prevSelectedIndex].Controls[0] is PanelTecViewBase))
                {
                    selTecView = (PanelTecViewBase)tclTecViews.TabPages[m_prevSelectedIndex].Controls[0];

                    if (!(selTecView == null) && ((!(selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_AISKUE] == null))
                        && (!(selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_SOTIASSO] == null))))
                    {
                        if ((m_report.actioned_state == true)
                            && ((selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_AISKUE].ignore == false)
                            && (selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_SOTIASSO].ignore == false)))
                        {
                            if (selTecView.Actived == true)
                            {
                                m_lblDateMessage.Text = m_report.last_time_action.ToString();
                                m_lblDescMessage.Text = m_report.last_action;
                            }
                            else
                                ;
                        }
                        else
                            ;

                        if ((m_report.warninged_state == true)
                            && ((selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_AISKUE].ignore == false)
                            && (selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_SOTIASSO].ignore == false)))
                        {
                            have_msg = 1;
                            if (selTecView.Actived == true)
                            {
                                m_lblDateMessage.Text = m_report.last_time_warning.ToString();
                                m_lblDescMessage.Text = m_report.last_warning;
                            }
                            else
                                ;
                        }
                        else
                            ;

                        if ((m_report.errored_state == true)
                            && ((selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_AISKUE].ignore == false)
                            && (selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_SOTIASSO].ignore == false)))
                        {
                            have_msg = -1;
                            if (selTecView.Actived == true)
                            {
                                m_lblDateMessage.Text = m_report.last_time_error.ToString();
                                m_lblDescMessage.Text = m_report.last_error;
                            }
                            else
                                ;
                        }
                        else
                            ;
                    }
                    else
                        ; //Вкладка не найдена
                }
                else
                {
                    if (m_report.actioned_state == true)
                    {
                        m_lblDateMessage.Text = m_report.last_time_action.ToString();
                        m_lblDescMessage.Text = m_report.last_action;
                    }
                    else
                        ;

                    if (m_report.warninged_state == true)
                    {
                        have_msg = 1;
                        m_lblDateMessage.Text = m_report.last_time_warning.ToString();
                        m_lblDescMessage.Text = m_report.last_warning;
                    }
                    else
                        ;

                    if (m_report.errored_state == true)
                    {
                        have_msg = -1;
                        m_lblDateMessage.Text = m_report.last_time_error.ToString();
                        m_lblDescMessage.Text = m_report.last_error;
                    }
                    else
                        ;
                }
            }
            else
                ;

            return have_msg;
        }

        private void addTabPagesAdmin()
        {
            if ((formChangeMode.m_markTabAdminChecked.IsMarked((int)FormChangeMode.MANAGER.DISP) == true)
                || (formChangeMode.m_markTabAdminChecked.IsMarked((int)FormChangeMode.MANAGER.NSS) == true))
            {
                int idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");

                if (formChangeMode.m_markTabAdminChecked.IsMarked((int)FormChangeMode.MANAGER.DISP) == true)
                {
                    addTabPageAdmin(idListener, FormChangeMode.MANAGER.DISP);
                }
                else
                    ;

                if (formChangeMode.m_markTabAdminChecked.IsMarked((int)FormChangeMode.MANAGER.NSS) == true)
                {
                    addTabPageAdmin(idListener, FormChangeMode.MANAGER.NSS);
                }
                else
                    ;

                DbSources.Sources().UnRegister(idListener);
            }
            else
            {
                m_markPrevStatePanelAdmin.UnMarked();
            }
        }

        private void addTabPageAdmin(int idListener, FormChangeMode.MANAGER modeAdmin)
        {
            StatisticCommon.FormChangeMode.MODE_TECCOMPONENT mode = FormChangeMode.MODE_TECCOMPONENT.UNKNOWN;

            if (HStatisticUsers.RoleIsDisp == true)
            {
                Passwords.ID_ROLES idRolesPassword = Passwords.ID_ROLES.ADMIN;
                DialogResult dlgRes = System.Windows.Forms.DialogResult.Yes;

                if (m_markPrevStatePanelAdmin.IsMarked((int)modeAdmin) == false)
                {
                    if (modeAdmin == FormChangeMode.MANAGER.DISP)
                        idRolesPassword = Passwords.ID_ROLES.COM_DISP;
                    else
                        if (modeAdmin == FormChangeMode.MANAGER.NSS)
                            idRolesPassword = Passwords.ID_ROLES.NSS;
                        else
                            ;

                    if (idRolesPassword == Passwords.ID_ROLES.ADMIN)
                        //??? Ошибка
                        return;
                    else
                        ;

                    formPassword.SetIdPass(idListener, 0, idRolesPassword);
                    dlgRes = formPassword.ShowDialog(this);
                }
                else
                {
                }

                if (dlgRes == DialogResult.Yes)
                {
                    //StartWait();
                    delegateStartWait();

                    m_arPanelAdmin[(int)modeAdmin].Start();

                    switch (modeAdmin)
                    {
                        case FormChangeMode.MANAGER.DISP:
                            mode = FormChangeMode.MODE_TECCOMPONENT.GTP;
                            break;
                        case FormChangeMode.MANAGER.NSS:
                            mode = FormChangeMode.MODE_TECCOMPONENT.TEC; //PC или TG не важно
                            break;
                        default:
                            break;
                    }

                    tclTecViews.AddTabPage(formChangeMode.getNameAdminValues(mode), -1, HTabCtrlEx.TYPE_TAB.FIXED);

                    tclTecViews.TabPages[tclTecViews.TabCount - 1].Controls.Add(m_arPanelAdmin[(int)modeAdmin]);

                    m_arPanelAdmin[(int)modeAdmin].InitializeComboBoxTecComponent(mode);

                    delegateStopWait();

                    m_markPrevStatePanelAdmin.Set((int)modeAdmin, true);
                }
                else
                    ;
            }
            else
                ; //Не требуется отображать вкладку 'panelAdmin'
        }

        private void ReadAnalyzer(TcpClient res, string cmd)
        {
            //Message from Analyzer CMD;ARG1, ARG2,...,ARGN=RESULT
            switch (cmd.Split('=')[0].Split(';')[0])
            {
                case "INIT":
                    //m_TCPServer.Write(res, cmd.Substring(0, cmd.IndexOf("=") + 1) + "OK");
                    break;
                case "LOG_LOCK":
                    //m_TCPServer.Write(res, cmd.Substring(0, cmd.IndexOf("=") + 1) + "OK;" + Logging.Logg().Suspend());
                    break;
                case "LOG_UNLOCK":
                    Logging.Logg().Resume();

                    //m_TCPServer.Write(res, cmd.Substring(0, cmd.IndexOf("=") + 1) + "OK");
                    break;
                case "TAB_VISIBLE":
                    int i = -1,
                        mode = formChangeMode.getModeTECComponent();
                    string strIdItems = string.Empty,
                            mes = "OK" + ";";
                    mes += mode;

                    strIdItems = formChangeMode.getIdsOfCheckedIndicies();
                    if (strIdItems.Equals(string.Empty) == false)
                        mes += "; " + strIdItems;
                    else
                        ;

                    //m_TCPServer.Write(res, cmd.Substring(0, cmd.IndexOf("=") + 1) + mes);
                    break;
                case "DISONNECT":
                    break;
                case "":
                    break;
                default:
                    break;
            }
        }

        private void FormMain_PanelCustomTecView_EvtContentChanged()
        {
            if (m_bAutoActionTabs == false)
                this.BeginInvoke(new DelegateFunc(fileProfileSaveAddingTab));
            else
                ;
        }

        protected override void  timer_Start()
        {
 	        int i = -1;
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormAbout formAbout = new FormAbout(this.Icon.ToBitmap() as Image))
            {
                formAbout.ShowDialog(this);
            }
        }

        private void панельГрафическихToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                if (панельГрафическихToolStripMenuItem.Checked == true)
                    formGraphicsSettings.Show();
                else
                    formGraphicsSettings.Hide();
            }
            else
                ;
        }

        protected override void HideGraphicsSettings()
        {
            панельГрафическихToolStripMenuItem.Checked = false;
        }

        private void значенияТекущаяМощностьГТПгТЭЦснToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[(int)ID_ADDING_TAB.CUR_POWER].panel == null)
            {
                m_dictAddingTabs[(int)ID_ADDING_TAB.CUR_POWER].panel = new PanelCurPower(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec, ErrorReport, WarningReport, ActionReport, ReportClear);
                ((PanelStatistic)m_dictAddingTabs[(int)ID_ADDING_TAB.CUR_POWER].panel).SetDelegate(null, null, delegateEvent);
            }
            else
                ;

            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.CUR_POWER].panel, @"P тек ГТПг, ТЭЦсн"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void значенияТекущаяМощностьТЭЦгТЭЦснToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[(int)ID_ADDING_TAB.TM_SN_POWER].panel == null)
            {
                m_dictAddingTabs[(int)ID_ADDING_TAB.TM_SN_POWER].panel = new PanelTMSNPower(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec, ErrorReport, WarningReport, ActionReport, ReportClear);
                ((PanelStatistic)m_dictAddingTabs[(int)ID_ADDING_TAB.TM_SN_POWER].panel).SetDelegate(null, null, delegateEvent);
            }
            else
                ;

            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.TM_SN_POWER].panel, @"P тек ТЭЦг, ТЭЦсн"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void мониторингПоследняяМинутаЧасToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[(int)ID_ADDING_TAB.MONITOR_LAST_MINUTES].panel == null)
            {
                m_dictAddingTabs[(int)ID_ADDING_TAB.MONITOR_LAST_MINUTES].panel = new PanelLastMinutes(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec, ErrorReport, WarningReport, ActionReport, ReportClear);
                ((PanelStatistic)m_dictAddingTabs[(int)ID_ADDING_TAB.MONITOR_LAST_MINUTES].panel).SetDelegate(null, null, delegateEvent);
            }
            else
                ;

            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.MONITOR_LAST_MINUTES].panel, @"Монитор P-d4%"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void ДиагностикаToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[(int)ID_ADDING_TAB.DIAGNOSTIC].panel == null)
            {
                m_dictAddingTabs[(int)ID_ADDING_TAB.DIAGNOSTIC].panel = new PanelStatisticDiagnostic();
            }
            else
                ;
            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.DIAGNOSTIC].panel, "Диагностика"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void собственныеНуждыToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[(int)ID_ADDING_TAB.SOBSTV_NYZHDY].panel == null)
            {
                m_dictAddingTabs[(int)ID_ADDING_TAB.SOBSTV_NYZHDY].panel = new PanelSobstvNyzhdy(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec, ErrorReport, WarningReport, ActionReport, ReportClear);
                ((PanelSobstvNyzhdy)m_dictAddingTabs[(int)ID_ADDING_TAB.SOBSTV_NYZHDY].panel).SetDelegate(null, null, delegateEvent);
            }
            else
                ;

            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.SOBSTV_NYZHDY].panel
                , ((ToolStripMenuItem)sender).Text
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void выборОбъекты22ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            выборОбъектыToolStripMenuItem_CheckedChanged(sender, e, INDEX_CUSTOM_TAB.TAB_2X2);
        }

        private void выборОбъекты23ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            выборОбъектыToolStripMenuItem_CheckedChanged(sender, e, INDEX_CUSTOM_TAB.TAB_2X3);
        }

        private Size getPanelCustomTecViewSize(INDEX_CUSTOM_TAB type)
        {
            Size szRes = Size.Empty;

            switch (type)
            {
                case INDEX_CUSTOM_TAB.TAB_2X2:
                    szRes = new Size(2, 2);
                    break;
                case INDEX_CUSTOM_TAB.TAB_2X3:
                    szRes = new Size(3, 2);
                    break;
                default:
                    throw new Exception(@"FormMain::getPanelCustomTecViewSize () - невозможно определить размерность вкладки...");
            }

            return szRes;
        }

        private void выборОбъектыToolStripMenuItem_CheckedChanged(object sender, EventArgs e, INDEX_CUSTOM_TAB indx)
        {
            ToolStripMenuItem obj = sender as ToolStripMenuItem;
            int indxItem = ((ToolStripMenuItem)obj.OwnerItem).DropDownItems.IndexOf(obj);
            bool bStoped = true;
            int keyTab = (int)m_arIdCustomTabs[(int)indx, indxItem];

            if ((obj.Checked == false)
                && (!(m_dictFormFloat == null))
                && (m_dictFormFloat.ContainsKey(keyTab) == true)
                && (!(m_dictFormFloat[keyTab] == null)))
            {
                m_dictFormFloat[keyTab].Close();
                bStoped = false;
            }
            else
                ;

            if ((m_dictAddingTabs[keyTab].panel == null)
                //&& (bStoped == true)
                //&& (obj.Checked == true)
                )
            {
                m_dictAddingTabs[keyTab].panel = new PanelCustomTecView(formChangeMode, getPanelCustomTecViewSize(indx), ErrorReport, WarningReport, ActionReport, ReportClear);
                ((PanelCustomTecView)m_dictAddingTabs[keyTab].panel).EventContentChanged += new DelegateFunc(FormMain_PanelCustomTecView_EvtContentChanged);
            }
            else
                ;

            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[keyTab].panel
                , obj.OwnerItem.Text + @" - " + obj.Text
                , new bool[] { obj.Checked, bStoped });
        }

        private void рассинхронизацияДатаВремяСерверБДToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].panel == null)
                m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].panel = new PanelSourceData();
            else
                ;

            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].panel, "Дата/время серверов БД"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void значенияСОТИАССОToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[(int)ID_ADDING_TAB.SOTIASSO].panel == null)
                m_dictAddingTabs[(int)ID_ADDING_TAB.SOTIASSO].panel = new PanelSOTIASSO(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec, ErrorReport, WarningReport, ActionReport, ReportClear);
            else
                ;

            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.SOTIASSO].panel, "Значения СОТИАССО"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void видSubToolStripMenuItem_CheckedChanged(PanelStatistic obj, string nameTab, bool[] arCheckedStoped)
        {
            if (arCheckedStoped[0] == true)
            {
                HTabCtrlEx.TYPE_TAB typeTab = HTabCtrlEx.TYPE_TAB.FIXED;
                int key = -1;

                if (nameTab.IndexOf(@"Окно") > -1)
                {
                    INDEX_CUSTOM_TAB indxTab = getIndexCustomTab(nameTab);
                    int indxItem = getIndexItemCustomTab(nameTab);
                    key = (int)m_arIdCustomTabs[(int)indxTab, indxItem];
                    typeTab = HTabCtrlEx.TYPE_TAB.FLOAT;
                }
                else
                    ;

                tclTecViews.AddTabPage(nameTab, key, typeTab);
                tclTecViews.TabPages[tclTecViews.TabCount - 1].Controls.Add(obj);

                obj.Start();
                if (m_bAutoActionTabs == false)
                    ActivateTabPage();
                else
                    ;
            }
            else
            {//arCheckedStoped[0] == false
                bool bRes = tclTecViews.RemoveTabPage(nameTab);
                if (arCheckedStoped[1] == true)
                {
                    obj.Activate(false);
                    obj.Stop();
                }
                else
                    ;
            }

            if (m_bAutoActionTabs == false)
                //Сохранить список дополнительных вкладок...
                if (файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked == true)
                    fileProfileSaveAddingTab();
                else
                    ;
            else
                ;
        }

        protected override void UpdateActiveGui(int type)
        {
            if ((!(tclTecViews.SelectedIndex < 0)) && (tclTecViews.SelectedIndex < tclTecViews.TabCount))
                if (tclTecViews.TabPages[tclTecViews.SelectedIndex].Controls[0] is PanelTecView)
                    //selectedTecViews[tclTecViews.SelectedIndex].UpdateGraphicsCurrent();
                    ((PanelTecView)tclTecViews.TabPages[tclTecViews.SelectedIndex].Controls[0]).UpdateGraphicsCurrent(type);
                else
                    if (tclTecViews.TabPages[tclTecViews.SelectedIndex].Controls[0] is PanelSobstvNyzhdy)
                        ((PanelSobstvNyzhdy)tclTecViews.TabPages[tclTecViews.SelectedIndex].Controls[0]).UpdateGraphicsCurrent(type);
                    else
                        if (tclTecViews.TabPages[tclTecViews.SelectedIndex].Controls[0] is PanelCustomTecView)
                            ((PanelCustomTecView)tclTecViews.TabPages[tclTecViews.SelectedIndex].Controls[0]).UpdateGraphicsCurrent(type);
                        else
                            if (tclTecViews.TabPages[tclTecViews.SelectedIndex].Controls[0] is PanelSOTIASSO)
                                ((PanelSOTIASSO)tclTecViews.TabPages[tclTecViews.SelectedIndex].Controls[0]).UpdateGraphicsCurrent(type);
                            else
                                ;
            else
                ;
        }

        private const int SW_SHOWNOACTIVATE = 4;
        private const int HWND_TOP = 0;
        private const uint SWP_NOACTIVATE = 0x0010;
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static extern bool SetWindowPos(
             int hWnd,           // window handle
             int hWndInsertAfter,    // placement-order handle
             int X,          // horizontal position
             int Y,          // vertical position
             int cx,         // width
             int cy,         // height
             uint uFlags);       // window positioning flags

        private void FormMain_Activated(object sender, EventArgs e)
        {
            Logging.Logg().Debug(@"FormMain::FormMain_Activated () - ...", Logging.INDEX_MESSAGE.NOT_SET);

            if (панельГрафическихToolStripMenuItem.Checked == true)
            {
                ShowWindow(formGraphicsSettings.Handle, SW_SHOWNOACTIVATE);
                SetWindowPos(formGraphicsSettings.Handle.ToInt32(), HWND_TOP,
                            formGraphicsSettings.Left, formGraphicsSettings.Top, formGraphicsSettings.Width, formGraphicsSettings.Height,
                            SWP_NOACTIVATE);
            }
            else
                ;
        }

        private void параметрыПриложенияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
                formParameters.ShowDialog(this);
            else
                ;
        }

        private void параметрыТГБийскToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                foreach (PanelTecViewBase tv in m_listStandardTabs)
                    if (tv.m_tecView.m_tec.Type == StatisticCommon.TEC.TEC_TYPE.BIYSK)
                    {
                        if (!(m_formParametersTG == null))
                            //tv.tec.parametersTGForm.ShowDialog(this);
                            m_formParametersTG.ShowDialog(this);
                        else
                            Logging.Logg().Error(@"FormMain::параметрыТГБийскToolStripMenuItem_Click () - m_formParametersTG == null", Logging.INDEX_MESSAGE.NOT_SET);

                        break;
                    }
                    else
                        ;
            }
            else
                ;
        }

        private void изментьСоставТЭЦГТПЩУToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
            formPassword.SetIdPass(idListener, 0, Passwords.ID_ROLES.ADMIN);
            formPassword.ShowDialog(this);
            DialogResult dlgRes = formPassword.DialogResult;
            if (dlgRes == DialogResult.Yes)
            {
                FormTECComponent tecComponent = new FormTECComponent(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett());
                if (tecComponent.ShowDialog(this) == DialogResult.Yes)
                {
                    MessageBox.Show(this, "В БД конфигурации внесены изменения.\n\rНеобходим останов/запуск приложения.\n\r", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    выходToolStripMenuItem.PerformClick();
                    //Stop (new FormClosingEventArgs (CloseReason.UserClosing, true));
                    //MainForm_FormClosing (this, new FormClosingEventArgs (CloseReason.UserClosing, true));
                }
                else
                    ;
            }
            else
                if (dlgRes == DialogResult.Abort)
                {
                    //Errors.NoAccess
                    connectionSettings(CONN_SETT_TYPE.CONFIG_DB);
                }
                else
                    ;

            DbSources.Sources().UnRegister(idListener);
        }

        private void изментьСоставПользовательToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
            formPassword.SetIdPass(idListener, 0, Passwords.ID_ROLES.ADMIN);
            formPassword.ShowDialog(this);
            DialogResult dlgRes = formPassword.DialogResult;
            if (dlgRes == DialogResult.Yes)
            {
                FormUser formUser = new FormUser(idListener);

                if (formUser.ShowDialog(this) == DialogResult.Yes)
                {
                    MessageBox.Show(this, "В БД конфигурации внесены изменения.\n\rНеобходим перезапуск приложения.\n\r", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    выходToolStripMenuItem.PerformClick();
                    //Stop (new FormClosingEventArgs (CloseReason.UserClosing, true));
                    //MainForm_FormClosing (this, new FormClosingEventArgs (CloseReason.UserClosing, true));
                }
                else
                    ;

                DbSources.Sources().UnRegister(idListener);
            }
            else
                if (dlgRes == DialogResult.Abort)
                {
                    //Errors.NoAccess
                    connectionSettings(CONN_SETT_TYPE.CONFIG_DB);
                }
                else
                    ;

            DbSources.Sources().UnRegister(idListener);
        }

        private void изменитьПарольНССToolStripMenuItem_Click(object sender, EventArgs e)
        {
            изменитьПарольToolStripMenuItem_Click(sender, e, 0, Passwords.ID_ROLES.NSS);
        }

        private void диагностикаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[(int)ID_ADDING_TAB.DIAGNOSTIC].panel == null)
            {
                m_dictAddingTabs[(int)ID_ADDING_TAB.DIAGNOSTIC].panel = new PanelStatisticDiagnostic();
            }
            else
                ;
            if (ДиагностикаToolStripMenuItem.Checked == true)
            {
                ((ToolStripMenuItem)sender).Checked = true;
            }
            else ((ToolStripMenuItem)sender).Checked = false;

            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.DIAGNOSTIC].panel, "Диагностика"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        private void рассинхронизацияДатаВремяСерверБДToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].panel == null)
                m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].panel = new PanelSourceData();
            else
                ;
            if (рассинхронизацияДатаВремяСерверБДToolStripMenuItem.Checked == true)
            {
                ((ToolStripMenuItem)sender).Checked = true;
            }
            else ((ToolStripMenuItem)sender).Checked = false;

            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].panel, "Дата/время серверов БД"
                , new bool[] { ((ToolStripMenuItem)sender).Checked, true });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameTab"></param>
        /// <param name="obj"></param>
        /// <param name="check"></param>
        private void closeTabMenu(string nameTab, PanelStatistic obj, bool check)
        {
            bool bRes = tclTecViews.RemoveTabPage(nameTab);
            if (check == true)
            {
                obj.Activate(false);
                obj.Stop();
            }
            else
                ;
        }

        private void изменитьПарольКоммерческогоДиспетчераToolStripMenuItem_Click(object sender, EventArgs e)
        {
            изменитьПарольToolStripMenuItem_Click(sender, e, 0, Passwords.ID_ROLES.COM_DISP);
        }

        private void изменитьПарольАдминистратораToolStripMenuItem_Click(object sender, EventArgs e)
        {
            изменитьПарольToolStripMenuItem_Click(sender, e, 0, Passwords.ID_ROLES.ADMIN);
        }

        private void изменитьПарольToolStripMenuItem_Click(object sender, EventArgs e, int id_ext, Passwords.ID_ROLES id_role)
        {
            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                int idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
                formPassword.SetIdPass(idListener, id_ext, id_role);
                DialogResult dlgRes = formPassword.ShowDialog(this);
                if (dlgRes == DialogResult.Yes)
                {
                    formSetPassword.SetIdPass(idListener, 0, formPassword.GetIdRolePassword());
                    formSetPassword.ShowDialog(this);
                }
                else
                    if (dlgRes == DialogResult.Abort)
                        connectionSettings(CONN_SETT_TYPE.CONFIG_DB);
                    else
                        ;

                DbSources.Sources().UnRegister(idListener);
            }
            else
                ;
        }

        private void menuStrip_MenuActivate(object sender, EventArgs e)
        {
            activateTabPage(tclTecViews.SelectedIndex, false);
        }

        private ToolStripMenuItem getSelectedMenuItem(ToolStripMenuItem owner)
        {
            ToolStripMenuItem itemRes = null;

            foreach (ToolStripItem item in owner.DropDownItems)
            {
                if (item is ToolStripMenuItem)
                    if ((item as ToolStripMenuItem).DropDownItems.Count > 0 && item.Enabled == true)
                    {
                        itemRes = getSelectedMenuItem(item as ToolStripMenuItem);
                        if (!(itemRes == null))
                            break;
                        else
                            ;
                    }
                    else
                    {
                        if (item.Selected == true)
                        {
                            itemRes = item as ToolStripMenuItem;
                            break;
                        }
                        else
                            ;
                    }
                else
                    ;
            }

            return itemRes;
        }

        private void menuStrip_MenuDeactivate(object sender, EventArgs e)
        {
           bool bExit = false;
           ToolStripMenuItem selItem = null;

           foreach (ToolStripMenuItem item in ((MenuStrip)sender).Items)
           {
                if (item.DropDownItems.Count > 0 && item.Enabled == true)
                {
                    selItem = getSelectedMenuItem(item);
                    break;
                }
                else
                    ;
            }

           if ((!(selItem == null)) && (selItem.Text.Equals(@"Выход") == true))
                bExit = true;
            else
                ;

            if (bExit == false)
                activateTabPage(tclTecViews.SelectedIndex, true);
            else
                ;
        }

        private void contextMenuStrip_Closed(object sender, System.Windows.Forms.ToolStripDropDownClosedEventArgs e)
        {
            if (!(e.CloseReason == ToolStripDropDownCloseReason.ItemClicked))
                activateTabPage(tclTecViews.SelectedIndex, true);
            else
                ;
        }

        //FormChangeMode.MANAGER modePanelAdmin {
        //    get {
        //        FormChangeMode.MANAGER modeRes = FormChangeMode.MANAGER.UNKNOWN;

        //        if (formChangeMode.admin_was_checked == true) {
        //            switch (HStatisticUsers.Role) {
        //                case HStatisticUsers.ID_ROLES.ADMIN:
        //                    if (formChangeMode.IsModeTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP) == true)
        //                        modeRes = FormChangeMode.MANAGER.DISP;
        //                    else
        //                        modeRes = FormChangeMode.MANAGER.NSS;
        //                    break;
        //                case HStatisticUsers.ID_ROLES.KOM_DISP:
        //                    modeRes = FormChangeMode.MANAGER.DISP;
        //                    break;
        //                case HStatisticUsers.ID_ROLES.NSS:
        //                    modeRes = FormChangeMode.MANAGER.NSS;
        //                    break;
        //                default:
        //                    break;
        //            }

        //            return modeRes;
        //        }
        //        else
        //            return FormChangeMode.MANAGER.DISP;
        //    }
        //}

        //protected override void WndProc(ref Message m)
        //{
        //    base.WndProc(ref m);

        //    //Logging.Logg().Debug(@"FormMain::WndProc () - msg.ID=" + m.Msg + @", msg.Res=" + m.Result, Logging.INDEX_MESSAGE.NOT_SET);
        //    Console.WriteLine(@"FormMain::WndProc () - msg.ID=" + m.Msg + @", msg.Res=" + m.Result);
        //}
    }
}