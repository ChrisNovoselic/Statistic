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

using System.Net;

//???
//using System.Security.Cryptography;

using HClassLibrary;
using StatisticCommon;

using StatisticTimeSync;

namespace Statistic
{
    public partial class FormMain : FormMainBaseWithStatusStrip
    {
        //10001 = ADMIN_KOM_DISP, 10002 = ADMIN_NSS (FormChangeMode)
        private enum ID_ADDING_TAB { CUR_POWER = 10101, TM_SN_POWER, MONITOR_LAST_MINUTES, SOBSTV_NYZHDY, CUSTOM_2X2, CUSTOM_2X3, DATETIMESYNC_SOURCE_DATA, };
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
        private static bool m_bAutoLoadTabs = false;

        public enum ID_ERROR_INIT { UNKNOWN = -1, }
        private enum INDEX_ERROR_INIT { UNKNOWN = 0, }
        private static string [] MSG_ERROR_INIT = { @"Неизвестная причина" };

        private PanelAdmin [] m_arPanelAdmin;
        Dictionary<int, ADDING_TAB> m_dictAddingTabs;
        //public AdminTS [] m_arAdmin;
        //public Users m_user;
        public Passwords m_passwords;
        private List<PanelTecViewBase> tecViews;
        //private List<PanelTecViewBase> selectedTecViews;
        private FormPassword formPassword;
        private FormSetPassword formSetPassword;
        private FormChangeMode formChangeMode;
        private HMark m_markPrevStatePanelAdmin;
        private int m_prevSelectedIndex;
        public static FormGraphicsSettings formGraphicsSettings;
        public static FormParameters formParameters;
        //public FormParametersTG parametersTGForm;
        HStatisticUsers m_user;
        FormParametersTG m_formParametersTG;

        TcpServerAsync m_TCPServer;
        System.Threading.Timer m_timerAppReset;

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

            tclTecViews.OnClose += delegateOnCloseTab;
        }

        private string GetINIParametersOfID(int id)
        {
            return formParameters.m_arParametrSetup [id];
        }

        private int Initialize(out string msgError)
        {
            StartWait ();

            msgError = string.Empty;
            //MessageBox.Show((IWin32Window)null, @"FormMain::Initialize () - вХод...", @"Отладка!");

            int iRes = 0;
            int i = -1;

            m_prevSelectedIndex = 1; //??? = -1
            m_markPrevStatePanelAdmin = new HMark ();

            tecViews = new List<PanelTecViewBase>();

            int idListenerConfigDB = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
            //MessageBox.Show((IWin32Window)null, @"FormMain::Initialize () - DbSources.Sources().Register (...)", @"Отладка!");

            try {
                //formParameters = new FormParameters_FIleINI("setup.ini");
                formParameters = new FormParameters_DB(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett());

                Logging.LinkId (Logging.INDEX_MESSAGE.D_002, (int)FormParameters.PARAMETR_SETUP.MAINFORMBASE_SETPBRQUERY_LOGPBRNUMBER);
                Logging.LinkId (Logging.INDEX_MESSAGE.D_003, (int)FormParameters.PARAMETR_SETUP.TECVIEW_LOGRECOMENDATIONVAL);
                Logging.LinkId (Logging.INDEX_MESSAGE.D_004, (int)FormParameters.PARAMETR_SETUP.PANELQUICKDATA_LOGDEVIATIONEVAL);
                Logging.LinkId (Logging.INDEX_MESSAGE.D_005, (int)FormParameters.PARAMETR_SETUP.MAINFORMBASE_SETPBRQUERY_LOGQUERY);
                Logging.LinkId (Logging.INDEX_MESSAGE.W_001, (int)FormParameters.PARAMETR_SETUP.TECVIEW_GETCURRENTTMGEN_LOGWARNING);
                Logging.LinkId (Logging.INDEX_MESSAGE.D_001, (int)FormParameters.PARAMETR_SETUP.MAINFORMBASE_CONTROLHANDLE_LOGERRORCREATE);

                Logging.DelegateGetINIParametersOfID = new StringDelegateIntFunc(GetINIParametersOfID);
 
                updateParametersSetup();

                //Предустановленные в файле/БД конфигурации
                HUsers.s_REGISTRATION_INI [(int)HUsers.INDEX_REGISTRATION.DOMAIN_NAME] = formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.USERS_DOMAIN_NAME]; //string.Empty; //@"Отладчик";
                HUsers.s_REGISTRATION_INI[(int)HUsers.INDEX_REGISTRATION.ID] = 0; //Неизвестный пользователь
                HUsers.s_REGISTRATION_INI[(int)HUsers.INDEX_REGISTRATION.ID_TEC] = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.USERS_ID_TEC]); //5
                HUsers.s_REGISTRATION_INI[(int)HUsers.INDEX_REGISTRATION.ROLE] = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.USERS_ID_ROLE]); //2;
            }
            catch (Exception e) {
                Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"FormMain::Initialize () ... загрузка предустановленных параметров ...");

                msgError = e.Message;
                iRes = -5;
            }

            if (iRes == 0)
            {
                m_user = null;
                try
                {
                    m_user = new HStatisticUsers(idListenerConfigDB);
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

                    PanelAdminKomDisp.ALARM_USE = HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.ALARM_KOMDISP); //True;
                    if (!(HStatisticUsers.allTEC == 0))
                        PanelAdminKomDisp.ALARM_USE = false;
                    else ;
                    
                    //ИМгструмент администратора
                    параметрыToolStripMenuItem.Enabled =
                    администрированиеToolStripMenuItem.Enabled =
                        HStatisticUsers.RoleIsAdmin;

                    HMark markSett = new HMark (Int32.Parse(HStatisticUsers.GetAllowed((int)HStatisticUsers.ID_ALLOWED.AUTO_LOADSAVE_USERPROFILE)));                    
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
                                //((PanelAdminKomDisp)m_arPanelAdmin[i]).EventGUIReg += OnPanelAdminKomDispEventGUIReg;
                                ((PanelAdminKomDisp)m_arPanelAdmin[i]).EventGUIReg = new DelegateStringFunc(OnPanelAdminKomDispEventGUIReg);
                                break;
                            case (int)FormChangeMode.MANAGER.NSS:
                                m_arPanelAdmin[i] = new PanelAdminNSS(idListenerConfigDB, markQueries);
                                break;
                            default:
                                break;
                        }

                        m_arPanelAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                        m_arPanelAdmin[i].SetDelegateReport(ErrorReport, WarningReport, ActionReport);
                    }

                    string listIDs = string.Empty;
                    //if (((HStatisticUsers.RoleIsAdmin == true) || (HStatisticUsers.RoleIsDisp == true)) && (PanelAdminKomDisp.ALARM_USE == true))
                    //if ((HStatisticUsers.IsAllowed ((int)HStatisticUsers.ID_ALLOWED.AUTO_TAB_PBR_KOMDISP) == true) && (PanelAdminKomDisp.ALARM_USE == true))
                    if (HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.AUTO_TAB_PBR_KOMDISP) == true)
                    {
                        m_markPrevStatePanelAdmin.Set ((int)FormChangeMode.MANAGER.DISP, true);
                        //listIDs.Add (FormChangeMode.ID_SPECIAL_TAB[(int)FormChangeMode.MANAGER.DISP]);
                        listIDs += FormChangeMode.ID_SPECIAL_TAB[(int)FormChangeMode.MANAGER.DISP].ToString ();
                    }
                    else
                        ;

                    //Добавить закладки автоматически...
                    //listIDs.Add(5); listIDs.Add(111);
                    if (файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked == true)
                    {
                        string ids = HStatisticUsers.GetAllowed((int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE);
                        if (ids.Equals (string.Empty) == false)
                        {
                            if (listIDs.Equals(string.Empty) == false)
                                listIDs += @";" + ids;
                            else
                                ;

                            listIDs += ids;
                        }
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

                    TecView.SEC_VALIDATE_TMVALUE = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.VALIDATE_TM_VALUE]);

                    if (iRes == 0) {
                        Start(); //Старт 1-сек-го таймера для строки стостояния

                        createAddingTabs ();

                        stopTimerAppReset ();                        
                        int msecTimerAppReset = Int32.Parse (formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.APP_VERSION_QUERY_INTERVAL]);
                        m_timerAppReset = new System.Threading.Timer(new TimerCallback(fTimerAppReset), null, msecTimerAppReset, msecTimerAppReset);

                        m_bAutoLoadTabs = файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked;
                        
                        //сменитьРежимToolStripMenuItem_Click();
                        //formChangeMode.LoadProfile(@"116");
                        Logging.Logg().Action(@"АвтоЗагрузка профайла (" + HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE.ToString() + @"): ids=" + listIDs, Logging.INDEX_MESSAGE.NOT_SET);
                        formChangeMode.LoadProfile(string.Empty);

                        if (m_bAutoLoadTabs == true)
                            fileProfileLoadAddingTab();
                        else
                            ;

                        m_bAutoLoadTabs = false;
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

            StopWait();

            return iRes;
        }

        private void stopTimerAppReset () {
            if (! (m_timerAppReset == null)) {
                m_timerAppReset.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                m_timerAppReset.Dispose ();
                m_timerAppReset = null;
            } else {
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
            ProgramBase.AppExit ();
        }

        private void updateParametersSetup () {
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
            AdminAlarm.MSEC_ALARM_TIMERUPDATE = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_TIMER_UPDATE]) * 1000;
            AdminAlarm.MSEC_ALARM_EVENTRETRY = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.ALARM_EVENT_RETRY]) * 1000;
        }

        private void fTimerAppReset(object obj)
        {
            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture = 
                ProgramBase.ss_MainCultureInfo;

            if (HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.APP_AUTO_RESET) == true)
            {
                int err = -1;

                formParameters.Update (out err);

                if (err == 0)
                {
                    //Динамическое обновление параметров...
                    updateParametersSetup ();

                    if (formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.APP_VERSION].Equals(string.Empty) == false)
                        if (formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.APP_VERSION].Equals(Application.ProductVersion/*StatisticCommon.Properties.Resources.TradeMarkVersion*/) == false)
                        {
                            if (IsHandleCreated/**/ == true)
                            {
                                if (InvokeRequired == true)
                                {
                                    /*IAsyncResult iar = */
                                    this.BeginInvoke(new DelegateFunc(update));
                                    //this.EndInvoke (iar);
                                }
                                else
                                {
                                    update();
                                }
                            }
                            else
                                ;

                            //ProgramBase.AppRestart();

                        }
                        else
                        {
                        }
                    else
                        //При ошибке - восстанавливаем значение...
                        ; //formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.APP_VERSION] = strPrevAppVersion;
                }
                else
                    ; //DbSources.Sources().UnRegister(idListenerConfigDB);
            }
            else
            {
            }
        }

        private void panelAdminKomDispEventGUIReg(string text)
        {
            //Деактивация текущей вкладки
            activateTabPage(tclTecViews.SelectedIndex, false);

            //Диалоговое окно
            MessageBox.Show(this, text, @"Сигнализация", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //Активация текущей вкладки
            activateTabPage(tclTecViews.SelectedIndex, true);

            //Продолжение работы ...
            ((PanelAdminKomDisp)m_arPanelAdmin[(int)(int)FormChangeMode.MANAGER.DISP]).EventGUIConfirm();
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

        void delegateOnCloseTab(object sender, CloseTabEventArgs e)
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
                                            if (e.TabHeaderText.Contains(@"2X2") == true)
                                                m_dictAddingTabs[(int)ID_ADDING_TAB.CUSTOM_2X2].menuItem.Checked = false;
                                            else if (e.TabHeaderText.Contains(@"2X3") == true)
                                                m_dictAddingTabs[(int)ID_ADDING_TAB.CUSTOM_2X3].menuItem.Checked = false;
                                            else
                                                ;
                                        }
                                        else
                                            if (tclTecViews.TabPages[e.TabIndex].Controls[0] is PanelSourceData)
                                            {
                                                //activateTabPage(e.TabIndex, false);
                                                //tclTecViews.TabPages.RemoveByKey(HTabCtrlEx.GetNameTab (e.TabHeaderText));

                                                m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].menuItem.Checked = false;
                                            }
                                            else
                                                ;
        }

        private void файлПрофильАвтоЗагрузитьСохранить_CheckedChanged(object sender, EventArgs e)
        {
            файлПрофильЗагрузитьToolStripMenuItem.Enabled = 
            файлПрофильСохранитьToolStripMenuItem.Enabled =
                ! файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked;

            if ((!(m_timer == null)) && (! (m_timer.Interval == ProgramBase.TIMER_START_INTERVAL)))
            {
                HMark markSett = new HMark ();
                markSett.Set (0, файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Enabled);
                markSett.Set(1, файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked);

                int idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett (), false, CONN_SETT_TYPE.CONFIG_DB.ToString());
                HStatisticUsers.SetAllowed(idListener, (int)HStatisticUsers.ID_ALLOWED.AUTO_LOADSAVE_USERPROFILE, markSett.Value.ToString ());
                DbSources.Sources().UnRegister(idListener);
            }
            else
                ; //Загрузка формы...
        }

        private void файлПрофильЗагрузитьToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            m_bAutoLoadTabs = true;
            
            fileProfileLoadStandatdTab ();

            fileProfileLoadAddingTab ();

            m_bAutoLoadTabs = false;
        }

        private void fileProfileLoadStandatdTab () {
            string ids = HStatisticUsers.GetAllowed((int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE);
            Logging.Logg().Action(@"Загрузка профайла (" + HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE.ToString() + @"): ids=" + ids, Logging.INDEX_MESSAGE.NOT_SET);
            formChangeMode.LoadProfile(ids);
        }

        private void fileProfileLoadAddingTab () {
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

                    m_dictAddingTabs[id].menuItem.PerformClick ();

                    switch (id)
                    {
                        case (int)ID_ADDING_TAB.CUSTOM_2X2:
                        case (int)ID_ADDING_TAB.CUSTOM_2X3:
                            ((PanelCustomTecView)m_dictAddingTabs[id].panel).LoadProfile(profile.Substring(profile.IndexOf('=') + 1));
                            break;
                        default: //CUR_POWER, TM_SN_POWER...
                            break;
                    }
                }
            }
            else
                ;
        }

        private void fileProfileSaveStandardTab () {
            int iListenerId = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, CONN_SETT_TYPE.CONFIG_DB.ToString());
            fileProfileSaveStandardTab (iListenerId);
            DbSources.Sources().UnRegister (iListenerId);
        }

        private void fileProfileSaveStandardTab(int idListener)
        {
            //Сохранить список основных вкладок...
            HStatisticUsers.SetAllowed(idListener, (int)HStatisticUsers.ID_ALLOWED.PROFILE_SETTINGS_CHANGEMODE, formChangeMode.SaveProfile());
        }

        private void fileProfileSaveAddingTab()
        {
            int iListenerId = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, CONN_SETT_TYPE.CONFIG_DB.ToString());
            fileProfileSaveAddingTab (iListenerId);
            DbSources.Sources().UnRegister (iListenerId);
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
                        case (int)ID_ADDING_TAB.CUSTOM_2X2:
                        case (int)ID_ADDING_TAB.CUSTOM_2X3:
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
            int idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett (), false, CONN_SETT_TYPE.CONFIG_DB.ToString());

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

            if ((!(formChangeMode == null))
                && ((m_markPrevStatePanelAdmin.IsMarked ((int)FormChangeMode.MANAGER.DISP) == true)
                    || (m_markPrevStatePanelAdmin.IsMarked ((int)FormChangeMode.MANAGER.NSS) == true)))
            //if ((!(formChangeMode == null)) && (formChangeMode.admin_was_checked[(int)FormChangeMode.MANAGER.DISP] || formChangeMode.admin_was_checked[(int)FormChangeMode.MANAGER.NSS]))
            {
                if (!(m_arPanelAdmin == null)) {
                    for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++)
                        if (!(m_arPanelAdmin[i] == null))
                            if (m_arPanelAdmin[i].MayToClose() == false)
                                if (!(ev == null)) {
                                    ev.Cancel = true;
                                    break;
                                }
                                else
                                    ;
                            else {
                                if (i == (int)FormChangeMode.MANAGER.DISP) stopAdminAlarm(); else ;

                                m_arPanelAdmin[i].Stop ();
                                m_arPanelAdmin[i] = null;
                            }
                        else
                            ;

                    m_arPanelAdmin = null;
                } else
                    ;
            }
            else
                stopAdminAlarm ();

            Stop ();

            StopTabPages ();

            if (! (m_TCPServer == null)) {
                try {
                    m_TCPServer.Stop ();
                    m_TCPServer = null;
                } catch (Exception e) {
                    Logging.Logg().Exception(e, Logging.INDEX_MESSAGE.NOT_SET, @"FormMain::Stop (FormClosingEventArgs...) - m_TCPServer.Stop () - ...");
                }
            } else
                ;

            stopTimerAppReset ();
        }

        private void stopAdminAlarm () {
            if ((!(m_arPanelAdmin == null)) && (!(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP] == null)) && (m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP] is PanelAdminKomDisp)
            //if (i == (int)FormChangeMode.MANAGER.DISP)
            && (PanelAdminKomDisp.ALARM_USE == true)
            && (! (((PanelAdminKomDisp)m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP]).m_adminAlarm == null)))
            {
                ((PanelAdminKomDisp)m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP]).m_adminAlarm.Activate(false);
                ((PanelAdminKomDisp)m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP]).m_adminAlarm.Stop();
            }
            else
                ;
        }

        private void StopTabPages()
        {
            if (!(tecViews == null))
            {
                //for (i = 0; i < formChangeMode.tec_index.Count; i++)
                foreach (PanelTecViewBase tv in tecViews)
                {
                    tv.Stop();
                }

                tecViews.Clear();

                tclTecViews.SelectedIndexChanged -= tclTecViews_SelectedIndexChanged;

                tclTecViews.TabPages.Clear();

                tclTecViews.SelectedIndexChanged -= tclTecViews_SelectedIndexChanged;

                //selectedTecViews.Clear();
            }
            else
                ;

            foreach (ADDING_TAB tab in m_dictAddingTabs.Values)
            {
                tab.menuItem.Checked = false;
                if (!(tab.panel == null)) tab.panel.Stop(); else ;
            }
        }

        private void ClearTabPages()
        {
            Logging.Logg().Debug(@"FormMain::ClearTabPages () - вХод...", Logging.INDEX_MESSAGE.NOT_SET);

            activateTabPage(tclTecViews.SelectedIndex, false);

            List<int> indxRemove = new List<int>();

            foreach (TabPage tab in tclTecViews.TabPages)
            {
                if ((tab.Controls[0] is PanelTecViewBase) || (tab.Controls[0] is PanelAdmin)) {
                    indxRemove.Add(tclTecViews.TabPages.IndexOf(tab));
                    ((PanelStatistic)tab.Controls[0]).Stop ();
                }
                else
                    ;
            }

            tclTecViews.SelectedIndexChanged -= tclTecViews_SelectedIndexChanged;

            for (int i = indxRemove.Count - 1; !(i < 0); i--)
            {
                tclTecViews.TabPages.RemoveAt(indxRemove[i]);
            }

            tclTecViews.SelectedIndexChanged += tclTecViews_SelectedIndexChanged;

            //selectedTecViews.Clear();

            Logging.Logg().Debug(@"FormMain::ClearTabPages () - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);
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

            if (!(indx < 0)) {
                //strMsgDebug = @"FormMain::activateTabPage () - indx=" + indx + @", active=" + active.ToString() + @", type=" + tclTecViews.TabPages[indx].Controls[0].GetType().ToString();
                strMsgDebug = @"FormMain::activateTabPage () - indx=" + indx + @", active=" + active.ToString() + @", name=" + tclTecViews.TabPages[indx].Text;

                if (tclTecViews.TabPages[indx].Controls[0] is PanelTecViewBase)
                    ((PanelTecViewBase)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                else
                    ////В работе постоянно
                    if (tclTecViews.TabPages[indx].Controls[0] is PanelCurPower)
                        ((PanelCurPower)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                    else
                        if (tclTecViews.TabPages[indx].Controls[0] is PanelTMSNPower)
                            ((PanelTMSNPower)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                        else
                            if (tclTecViews.TabPages[indx].Controls[0] is PanelLastMinutes)
                                ((PanelLastMinutes)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                            else
                                if (tclTecViews.TabPages[indx].Controls[0] is PanelSobstvNyzhdy)
                                    ((PanelSobstvNyzhdy)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                                else
                                    if (tclTecViews.TabPages[indx].Controls[0] is PanelCustomTecView)
                                        ((PanelCustomTecView)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                                    else
                                        if (tclTecViews.TabPages[indx].Controls[0] is PanelAdmin)
                                            ((PanelAdmin)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                                        else
                                            if (tclTecViews.TabPages[indx].Controls[0] is PanelSourceData)
                                            {
                                                ((PanelSourceData)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                                            }
                                            else
                                                ;
            }
            else
                strMsgDebug = @"FormMain::activateTabPage () - indx=" + indx + @", active=" + active.ToString();

            Logging.Logg().Debug(strMsgDebug + @" - вЫход...", Logging.INDEX_MESSAGE.NOT_SET);
        }

        private void ActivateTabPage()
        {
            //selectedTecViews[tclTecViews.SelectedIndex].Activate(true);
            if (!(tclTecViews.SelectedIndex < 0))
                activateTabPage(tclTecViews.SelectedIndex, true);
            else
                ;

            //Деактивация
            if ((!(m_prevSelectedIndex < 0)) && (!(m_prevSelectedIndex == tclTecViews.SelectedIndex)) && (m_prevSelectedIndex < tclTecViews.TabPages.Count))
            {
                activateTabPage(m_prevSelectedIndex, false);
            }
            else
                ;

            m_prevSelectedIndex = tclTecViews.SelectedIndex;
        }

        private void OnEventFileConnSettSave (FIleConnSett.eventFileConnSettSave ev) {
            Properties.Settings.Default[@"connsett"] = new string(ev.hash, 0, ev.length);
            Properties.Settings.Default.Save ();
        }

        private void MainForm_FormLoad(object sender, EventArgs e) {
            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
                ProgramBase.ss_MainCultureInfo; //ru-Ru

            s_fileConnSett = new FIleConnSett(@"connsett.ini", FIleConnSett.MODE.FILE);
            //m_fileConnSett = new FIleConnSett(new string [] {@"connsett", Properties.Settings.Default.Properties[@"connsett"].ToString ()});
            //m_fileConnSett = new FIleConnSett(Properties.Settings.Default.Properties [@"connsett"].DefaultValue.ToString (), FIleConnSett.MODE.SETTINGS);
            //m_fileConnSett = new FIleConnSett((string)Properties.Settings. [@"connsett"], FIleConnSett.MODE.SETTINGS);
            //m_fileConnSett = new FIleConnSett((string)Properties.Settings.Default[@"connsett"], FIleConnSett.MODE.SETTINGS);
            //MessageBox.Show((IWin32Window)null, @"FormMain::FormMain () - new FIleConnSett (...)", @"Отладка!");

            //Только для 'FIleConnSett.MODE.SETTINGS'
            //m_fileConnSett.EventFileConnSettSave += new FIleConnSett.DelegateOnEventFileConnSettSave(OnEventFileConnSettSave);

            s_listFormConnectionSettings = new List<FormConnectionSettings>();
            s_listFormConnectionSettings.Add(new FormConnectionSettings(-1, s_fileConnSett.ReadSettingsFile, s_fileConnSett.SaveSettingsFile));
            s_listFormConnectionSettings.Add(null);
            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                string msg = string.Empty;
                switch (Initialize(out msg))
                {
                    case -1:
                        Abort(@"Неизвестная причина", false);
                        break;
                    case -2:
                    case -3:
                    case -5:
                    case -4: //@"Необходимо изменить параметры соединения с БД"
                        Abort(msg, false);
                        break;
                    default:
                        //Успех...
                        break;
                }
            }
            else
            {//Файла с параметрами соединения нет совсем или считанные параметры соединения не валидны
                Abort(@"Необходимо изменить параметры соединения с БД конфигурации", false);
            }

            this.Activate();            
        }

        public override void Close(bool bForce) { if (bForce == false) base.Close(bForce); else MainForm_FormClosing (this, new FormClosingEventArgs (CloseReason.ApplicationExitCall, true)); }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((! (m_TCPServer == null)) || (! (m_arPanelAdmin == null)) || (! (m_timer == null)))
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
                                this.BeginInvoke (new DelegateObjectFunc (stop), e);
                            else
                                Stop (e);
                        }
                    else
                        //Да, закрываем; признаку оставляем прежнее значение 'False': продолжить обработку события 'e'
                        if (InvokeRequired == true)
                            this.BeginInvoke(new DelegateObjectFunc(stop), e);
                        else
                            Stop(e);
                else {
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

        private int connectionSettings (CONN_SETT_TYPE type) {
            int iRes = -1;
            DialogResult result;
            result = s_listFormConnectionSettings [(int)type].ShowDialog(this);
            if (result == DialogResult.Yes)
            {
                StopTabPages ();

                base.Stop();

                int i = -1;
                if (!(m_arPanelAdmin == null))
                    for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++) {
                        if (!(m_arPanelAdmin[i] == null)) m_arPanelAdmin[i].Stop(); else ;
                    }
                else
                    ;

                string msg = string.Empty;
                iRes = Initialize(out msg);
                if (! (iRes == 0))
                    //@"Ошибка инициализации пользовательских компонентов формы"
                    Abort(msg, false);
                else
                    ;

                //foreach (PanelTecViewBase t in tecViews)
                //{
                //    t.Reinit();
                //}

                //m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].Reinit();
            }
            else
                ;

            return iRes;
        }

        private bool closeTecViewsTabPages ()
        {
            bool bRes = true;

            if (tclTecViews.TabPages.Count > 0)
                if (MessageBox.Show(this, "Вы уверены, что хотите закрыть текущие вкладки?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    StartWait();

                    StopTabPages();

                    if (!(tecViews == null))
                        for (int i = 0; i < tecViews.Count; i++)
                            tecViews[i].Stop();
                    else
                        ;

                    m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].Stop();
                    m_arPanelAdmin[(int)FormChangeMode.MANAGER.NSS].Stop();
                    m_markPrevStatePanelAdmin.UnMarked();

                    formChangeMode.btnClearAll_Click(formChangeMode, new EventArgs());

                    StopWait();

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
            настройкиСоединенияToolStripMenuItem_Click (sender, e, CONN_SETT_TYPE.LIST_SOURCE);
        }

        private void текущееСостояниеПользовательToolStripMenuItem_Click(object sender, EventArgs e)
        {
            activateTabPage(tclTecViews.SelectedIndex, false);

            int idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
            FormMainAnalyzer formAnalyzer = new FormMainAnalyzer_DB(idListener, formChangeMode.m_list_tec);
            ////Вариант №1
            //formAnalyzer.FormClosed += new FormClosedEventHandler(formAnalyzerCloused);
            //new Thread(() => formAnalyzer.ShowDialog()).Start();

            //Вариант №2
            formAnalyzer.ShowDialog(this);
            DbSources.Sources().UnRegister (idListener);
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
            if (closeTecViewsTabPages () == true) {
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
                else {
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
                            Abort (@"параметры соединения с БД конфигурации", false);
                    }
                    else
                        ;

                    if ((!(s_listFormConnectionSettings[(int)type] == null)) && (!(s_listFormConnectionSettings[(int)type].Ready == 0)))
                    {
                        bShowFormConnectionSettings = true;
                    }
                    else {
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
                tec.InitSensorsTEC ();
            else
                ;

            PanelTecView panelTecView = new PanelTecView(tec, ti, ci, null, ErrorReport, WarningReport, ActionReport);
            panelTecView.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);
            tecViews.Add(panelTecView);
        }

        private void сменитьРежимToolStripMenuItem_Click()
        {
            StartWait();

            //StartWait();
            ClearTabPages ();

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

                    for (tecView_index = 0; tecView_index < tecViews.Count; tecView_index++)
                    {
                        if ((tecViews[tecView_index].m_ID == formChangeMode.m_listItems[i].id))
                            break;
                        else
                            ;
                    }

                    if (! (tecView_index < tecViews.Count))
                    {//Не найден элемент - создаем, добавляем
                        foreach (StatisticCommon.TEC t in formChangeMode.m_list_tec)
                        {
                            if (t.m_id == formChangeMode.m_listItems[i].id)
                            {
                                addPanelTecView(t, formChangeMode.m_list_tec.IndexOf (t), -1);

                                tecView_index = tecViews.Count - 1;

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
                                        addPanelTecView(t, formChangeMode.m_list_tec.IndexOf(t), t.list_TECComponents.IndexOf (g));

                                        tecView_index = tecViews.Count - 1;

                                        break;
                                    }
                                    else
                                        ;
                                }
                            }
                            else
                                ;

                            if (tecView_index == (tecViews.Count - 1))
                                break;
                            else
                                ;
                        }
                    }
                    else
                        ;

                    if (tecView_index < tecViews.Count)
                    {
                        //list_tecView_index_checked.Add(tecView_index);

                        if ((tecViews[tecView_index].m_tecView.m_tec.type() == StatisticCommon.TEC.TEC_TYPE.BIYSK)/* && (параметрыТГБийскToolStripMenuItem.Visible == false)*/)
                            parametrsTGBiysk++;
                        else
                            ;

                        tclTecViews.AddTabPage(formChangeMode.m_listItems[i].name_shr);

                        tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(tecViews[tecView_index]);

                        tecViews[tecView_index].Start();
                    }
                    else
                        ;
                }
                else
                {
                }
            }

            //if (!(m_timer.Interval == ProgramBase.TIMER_START_INTERVAL))
            if (m_bAutoLoadTabs == false)
                //Сохранить список основных вкладок...
                if (файлПрофильАвтоЗагрузитьСохранитьToolStripMenuItem.Checked == true)
                    fileProfileSaveStandardTab ();
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

            StopWait();

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
                    сменитьРежимToolStripMenuItem_Click ();
                }
                else
                    ;
            }
            else
                ; //Нет соединения с конфигурационной БД
        }

        private void сменитьРежимToolStripMenuItem_EnabledChanged (object sender, EventArgs e) {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            видToolStripMenuItem.Enabled =
            настройкиСоединенияБДИсточникToolStripMenuItem.Enabled =
            текущееСостояниеПользовательToolStripMenuItem.Enabled =
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

            ActivateTabPage ();
        }

        protected override int UpdateStatusString()
        {
            int have_msg = 0;
            m_lblDescMessage.Text = m_lblDateMessage.Text = string.Empty;
            PanelTecViewBase selTecView = null;

            //for (int i = 0; i < selectedTecViews.Count; i++)
            //if ((selectedTecViews.Count > 0) /*&& (! (m_prevSelectedIndex < 0))*/)
            if ((!(m_prevSelectedIndex < 0)) && (m_prevSelectedIndex < tclTecViews.TabPages.Count))
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
                            if (selTecView.isActive == true)
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
                            if (selTecView.isActive == true)
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
                            if (selTecView.isActive == true)
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
                    //for (int i = (int)FormChangeMode.MANAGER.DISP; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i++)
                    //    if (m_arPanelAdmin [i].isActive == true)
                    //    {
                    //        if (m_report.actioned_state == true)
                    //        {
                    //            m_lblDateMessage.Text = m_report.last_time_action.ToString();
                    //            m_lblDescError.Text = m_report.last_action;
                    //        }
                    //        else
                    //            ;

                    //        if (m_report.errored_state == true)
                    //        {
                    //            have_eror = true;
                    //            m_lblDateMessage.Text = m_report.last_time_error.ToString();
                    //            m_lblDescError.Text = m_report.last_error;
                    //        }
                    //        else
                    //            ;
                    //    }
                    //    else
                    //        ;

                    //if (m_panelCurPower.m_bIsActive == true)
                    //{
                    //    if (m_report.actioned_state == true)
                    //    {
                    //        m_lblDateMessage.Text = m_report.last_time_action.ToString();
                    //        m_lblDescError.Text = m_report.last_action;
                    //    }
                    //    else
                    //        ;

                    //    if (m_report.errored_state == true)
                    //    {
                    //        have_eror = true;
                    //        m_lblDateMessage.Text = m_report.last_time_error.ToString();
                    //        m_lblDescError.Text = m_report.last_error;
                    //    }
                    //    else
                    //        ;
                    //}
                    //else
                    //    ;

                    //if (m_panelSNPower.m_bIsActive == true)
                    //{
                    //    if (m_report.actioned_state == true)
                    //    {
                    //        m_lblDateMessage.Text = m_report.last_time_action.ToString();
                    //        m_lblDescError.Text = m_report.last_action;
                    //    }
                    //    else
                    //        ;

                    //    if (m_report.errored_state == true)
                    //    {
                    //        have_eror = true;
                    //        m_lblDateMessage.Text = m_report.last_time_error.ToString();
                    //        m_lblDescError.Text = m_report.last_error;
                    //    }
                    //    else
                    //        ;
                    //}
                    //else
                    //    ;

                    //if (m_panelLastMinutes.m_bIsActive == true)
                    //{
                    //    if (m_report.actioned_state == true)
                    //    {
                    //        m_lblDateMessage.Text = m_report.last_time_action.ToString();
                    //        m_lblDescError.Text = m_report.last_action;
                    //    }
                    //    else
                    //        ;

                    //    if (m_report.errored_state == true)
                    //    {
                    //        have_eror = true;
                    //        m_lblDateMessage.Text = m_report.last_time_error.ToString();
                    //        m_lblDescError.Text = m_report.last_error;
                    //    }
                    //    else
                    //        ;
                    //}
                    //else
                    //    ;

                    //if (m_panelCustomTecView.m_bIsActive == true)
                    //{
                    //    if (m_report.actioned_state == true)
                    //    {
                    //        m_lblDateMessage.Text = m_report.last_time_action.ToString();
                    //        m_lblDescError.Text = m_report.last_action;
                    //    }
                    //    else
                    //        ;

                    //    if (m_report.errored_state == true)
                    //    {
                    //        have_eror = true;
                    //        m_lblDateMessage.Text = m_report.last_time_error.ToString();
                    //        m_lblDescError.Text = m_report.last_error;
                    //    }
                    //    else
                    //        ;
                    //}
                    //else
                    //    ;

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
            if ((formChangeMode.m_markTabAdminChecked.IsMarked ((int)FormChangeMode.MANAGER.DISP) == true)
                || (formChangeMode.m_markTabAdminChecked.IsMarked ((int)FormChangeMode.MANAGER.NSS) == true))
            {
                int idListener = DbSources.Sources().Register (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");

                if (formChangeMode.m_markTabAdminChecked.IsMarked ((int)FormChangeMode.MANAGER.DISP) == true)
                {
                    addTabPageAdmin(idListener, FormChangeMode.MANAGER.DISP);
                }
                else
                    ;

                if (formChangeMode.m_markTabAdminChecked.IsMarked ((int)FormChangeMode.MANAGER.NSS) == true)
                {
                    addTabPageAdmin(idListener, FormChangeMode.MANAGER.NSS);
                }
                else
                    ;

                DbSources.Sources ().UnRegister (idListener);
            }
            else {
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
                    StartWait();

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

                    tclTecViews.AddTabPage(formChangeMode.getNameAdminValues(mode));

                    tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(m_arPanelAdmin[(int)modeAdmin]);

                    m_arPanelAdmin[(int)modeAdmin].InitializeComboBoxTecComponent(mode);

                    StopWait();

                    m_markPrevStatePanelAdmin.Set((int)modeAdmin, true);
                }
                else
                    ;
            }
            else
                ; //Не требуется отображать вкладку 'panelAdmin'
        }

        private void ReadAnalyzer (TcpClient res, string cmd)
        {
            //Message from Analyzer CMD;ARG1, ARG2,...,ARGN=RESULT
            switch (cmd.Split ('=') [0].Split (';')[0])
            {
                case "INIT":
                    m_TCPServer.Write(res, cmd.Substring(0, cmd.IndexOf("=") + 1) + "OK");
                    break;
                case "LOG_LOCK":
                    m_TCPServer.Write(res, cmd.Substring(0, cmd.IndexOf("=") + 1) + "OK;" + Logging.Logg().Suspend());
                    break;
                case "LOG_UNLOCK":
                    Logging.Logg ().Resume ();

                    m_TCPServer.Write(res, cmd.Substring(0, cmd.IndexOf("=") + 1) + "OK");
                    break;
                case "TAB_VISIBLE":
                    int i = -1,
                        mode = formChangeMode.getModeTECComponent ();
                    string strIdItems = string.Empty,
                            mes = "OK" + ";";
                    mes += mode;

                    strIdItems = formChangeMode.getIdsOfCheckedIndicies ();
                    if (strIdItems.Equals (string.Empty) == false)
                        mes += "; " + strIdItems;
                    else
                        ;

                    m_TCPServer.Write(res, cmd.Substring(0, cmd.IndexOf("=") + 1) + mes);
                    break;
                case "DISONNECT":
                    break;
                case "":
                    break;
                default:
                    break;
            }
        }

        private void createAddingTabs () {
            m_dictAddingTabs[(int)ID_ADDING_TAB.CUR_POWER].panel = new PanelCurPower(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec, ErrorReport, WarningReport, ActionReport);
            ((PanelStatisticView)m_dictAddingTabs[(int)ID_ADDING_TAB.CUR_POWER].panel).SetDelegate(null, null, delegateEvent);
            //m_panelCurPower.Start();
            ////В работе постоянно
            //m_panelCurPower.Activate (true);

            m_dictAddingTabs[(int)ID_ADDING_TAB.TM_SN_POWER].panel = new PanelTMSNPower(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec, ErrorReport, WarningReport, ActionReport);
            ((PanelStatisticView)m_dictAddingTabs[(int)ID_ADDING_TAB.TM_SN_POWER].panel).SetDelegate(null, null, delegateEvent);

            m_dictAddingTabs[(int)ID_ADDING_TAB.MONITOR_LAST_MINUTES].panel = new PanelLastMinutes(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec, ErrorReport, WarningReport, ActionReport);
            ((PanelStatisticView)m_dictAddingTabs[(int)ID_ADDING_TAB.MONITOR_LAST_MINUTES].panel).SetDelegate(null, null, delegateEvent);
            //m_panelLastMinutes.Start();

            m_dictAddingTabs[(int)ID_ADDING_TAB.SOBSTV_NYZHDY].panel = new PanelSobstvNyzhdy(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec, ErrorReport, WarningReport, ActionReport);
            ((PanelSobstvNyzhdy)m_dictAddingTabs[(int)ID_ADDING_TAB.SOBSTV_NYZHDY].panel).SetDelegate(null, null, delegateEvent);

            m_dictAddingTabs[(int)ID_ADDING_TAB.CUSTOM_2X2].panel = new PanelCustomTecView(formChangeMode, new Size(2, 2), ErrorReport, WarningReport, ActionReport);
            m_dictAddingTabs[(int)ID_ADDING_TAB.CUSTOM_2X3].panel = new PanelCustomTecView(formChangeMode, new Size(3, 2), ErrorReport, WarningReport, ActionReport);
            //m_panelCustomTecView.SetDelegate(null, null, delegateEvent);
            //m_panelCustomTecView.Start();
            ((PanelCustomTecView)m_dictAddingTabs[(int)ID_ADDING_TAB.CUSTOM_2X2].panel).EventContentChanged += new DelegateFunc(FormMain_PanelCustomTecView_EvtContentChanged);
            ((PanelCustomTecView)m_dictAddingTabs[(int)ID_ADDING_TAB.CUSTOM_2X3].panel).EventContentChanged += new DelegateFunc(FormMain_PanelCustomTecView_EvtContentChanged);

            if (m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].menuItem.Enabled == true)
            //if (HStatisticUsers.IsAllowed((int)HStatisticUsers.ID_ALLOWED.MENUITEM_SETTING_PARAMETERS_SYNC_DATETIME_DB) == true)
            {
                m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].panel = new PanelSourceData();
                //m_panelSourceData.Start();
            }
            else
                ;
        }

        private void FormMain_PanelCustomTecView_EvtContentChanged () {
            if (m_bAutoLoadTabs == false)
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
            new FormAbout().ShowDialog(this);
        }

        private void панельГрафическихToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
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
            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.CUR_POWER].panel, @"P тек ГТПг, ТЭЦсн", ((ToolStripMenuItem)sender).Checked);
        }

        private void значенияТекущаяМощностьТЭЦгТЭЦснToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.TM_SN_POWER].panel, @"P тек ТЭЦг, ТЭЦсн", ((ToolStripMenuItem)sender).Checked);
        }

        private void мониторингПоследняяМинутаЧасToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.MONITOR_LAST_MINUTES].panel, @"Монитор P-d4%", ((ToolStripMenuItem)sender).Checked);
        }

        private void собственныеНуждыToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.SOBSTV_NYZHDY].panel, @"Собственные нужды", ((ToolStripMenuItem)sender).Checked);
        }

        private void выборОбъекты22ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.CUSTOM_2X2].panel, @"Объекты по выбору 2X2", ((ToolStripMenuItem)sender).Checked);
        }

        private void выборОбъекты23ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.CUSTOM_2X3].panel, @"Объекты по выбору 2X3", ((ToolStripMenuItem)sender).Checked);
        }

        private void рассинхронизацияДатаВремяСерверБДToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            видSubToolStripMenuItem_CheckedChanged(m_dictAddingTabs[(int)ID_ADDING_TAB.DATETIMESYNC_SOURCE_DATA].panel, "Дата/время серверов БД", ((ToolStripMenuItem)sender).Checked);
        }

        private void видSubToolStripMenuItem_CheckedChanged(PanelStatistic obj, string nameTab, bool bChecked)
        {
            if (bChecked == true)
            {
                tclTecViews.AddTabPage(nameTab);
                tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(obj);

                obj.Start();
                ActivateTabPage();
            }
            else
            {
                tclTecViews.TabPages.RemoveByKey(HTabCtrlEx.GetNameTab(nameTab));
                obj.Activate(false);
                obj.Stop();
            }

            if (m_bAutoLoadTabs == false)
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
            if ((!(tclTecViews.SelectedIndex < 0)) && (tclTecViews.SelectedIndex < tclTecViews.TabPages.Count))
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

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (панельГрафическихToolStripMenuItem.Checked)
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
            if (s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
                formParameters.ShowDialog(this);
            else
                ;
        }

        private void параметрыТГБийскToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                foreach (PanelTecViewBase tv in tecViews)
                    if (tv.m_tecView.m_tec.type() == StatisticCommon.TEC.TEC_TYPE.BIYSK)
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
                FormTECComponent tecComponent = new FormTECComponent(s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett());
                if (tecComponent.ShowDialog (this) == DialogResult.Yes) {
                    MessageBox.Show (this, "В БД конфигурации внесены изменения.\n\rНеобходим останов/запуск приложения.\n\r", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    выходToolStripMenuItem.PerformClick ();
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

            DbSources.Sources ().UnRegister (idListener);
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

                DbSources.Sources ().UnRegister (idListener);
            }
            else
                if (dlgRes == DialogResult.Abort)
                {
                    //Errors.NoAccess
                    connectionSettings(CONN_SETT_TYPE.CONFIG_DB);
                }
                else
                    ;

            DbSources.Sources ().UnRegister (idListener);
        }

        private void изменитьПарольНССToolStripMenuItem_Click(object sender, EventArgs e)
        {
            изменитьПарольToolStripMenuItem_Click(sender, e, 0, Passwords.ID_ROLES.NSS);
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
            if (s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
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

                DbSources.Sources ().UnRegister (idListener);
            }
            else
                ;
        }

        private void menuStrip_MenuActivate(object sender, EventArgs e)
        {
            activateTabPage (tclTecViews.SelectedIndex, false);
        }

        private ToolStripMenuItem getSelectedMenuItem (ToolStripMenuItem owner) {
            ToolStripMenuItem itemRes = null;

            foreach (ToolStripItem item in owner.DropDownItems)
            {
                if (item is ToolStripMenuItem)
                    if ((item as ToolStripMenuItem).DropDownItems.Count > 0 && item.Enabled == true)
                    {
                        itemRes = getSelectedMenuItem(item as ToolStripMenuItem);
                        if (! (itemRes == null))
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
    }
}