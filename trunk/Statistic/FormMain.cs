using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net.Sockets;

using System.Net;

//???
//using System.Security.Cryptography;

using StatisticCommon;

namespace Statistic
{
    public partial class FormMain : FormMainBaseWithStatusStrip
    {
        //public List<TEC> tec;
        private FIleConnSett m_fileConnSett;
        //private ConnectionSettingsSource m_connSettSource;
        public static List <FormConnectionSettings> s_listFormConnectionSettings;
        private PanelAdmin [] m_arPanelAdmin;
        PanelCurPower m_panelCurPower;
        PanelTMSNPower m_panelSNPower;
        PanelLastMinutes m_panelLastMinutes;
        PanelCustomTecView m_panelCustomTecView;
        //public AdminTS [] m_arAdmin;
        //public Users m_user;
        public Passwords m_passwords;
        private List<PanelTecViewBase> tecViews;
        //private List<PanelTecViewBase> selectedTecViews;
        private FormPassword formPassword;
        private FormSetPassword formSetPassword;
        private FormChangeMode formChangeMode;
        private PanelTecViewBase tecView;
        private int m_prevSelectedIndex;
        private FormChangeMode.MANAGER prevStateIsAdmin;
        public static FormGraphicsSettings formGraphicsSettings;
        public static FormParameters formParameters;
        //public FormParametersTG parametersTGForm;
        Users m_user;
        FormParametersTG m_formParametersTG;

        TcpServerAsync m_TCPServer;

        public void Abort (string msg, bool bThrow = false)
        {
            MessageBox.Show(this, msg + @"." + Environment.NewLine + @"Обратитесь к оператору тех./поддержки по тел. 4444 или по тел. 289-03-37.", "Ошибка в работе программы!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            if (bThrow == true) throw new Exception(msg + @"." + Environment.NewLine + @"Обратитесь к оператору тех./поддержки по тел. 4444 или по тел. 289-03-37."); else ;
        }

        public FormMain()
        {
            InitializeComponent();

            m_report = new HReports ();

            // m_statusStripMain
            FormMainBaseWithStatusStrip.m_statusStripMain.Location = new System.Drawing.Point(0, 762);
            FormMainBaseWithStatusStrip.m_statusStripMain.Size = new System.Drawing.Size(982, 22);
            // m_lblMainState
            this.m_lblMainState.Size = new System.Drawing.Size(150, 17);
            // m_lblDateError
            this.m_lblDateError.Size = new System.Drawing.Size(150, 17);
            // m_lblDescError
            this.m_lblDescError.Size = new System.Drawing.Size(667, 17);

            delegateUpdateActiveGui = new DelegateFunc(UpdateActiveGui);
            delegateHideGraphicsSettings = new DelegateFunc(HideGraphicsSettings);

            m_fileConnSett = new FIleConnSett ("connsett.ini");
            s_listFormConnectionSettings = new List<FormConnectionSettings> ();
            s_listFormConnectionSettings.Add (new FormConnectionSettings(-1, m_fileConnSett.ReadSettingsFile, m_fileConnSett.SaveSettingsFile));
            s_listFormConnectionSettings.Add(null);
            if (s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {                
                if (Initialize() == false)
                {
                    Abort (@"Параметры соединения с БД конфигурации", true);
                }
                else
                    ;
            }
            else
            {//Файла с параметрами соединения нет совсем
                connectionSettings(CONN_SETT_TYPE.CONFIG_DB);
            }

            tclTecViews.OnClose += delegateOnCloseTab;

            m_TCPServer = new TcpServerAsync(IPAddress.Any, 6666);
            m_TCPServer.delegateRead = ReadAnalyzer;

            if (!(m_TCPServer.Start() == 0)) Abort(@"Запуск дублирующего экземпляра приложения", true); else ;
        }

        private bool Initialize()
        {
            bool bRes = true;
            int i = -1;

            timer.Interval = 666; //Признак первого старта

            int idListenerConfigDB = DbSources.Sources ().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");

            m_user = null;
            try {
                m_user = new Users(idListenerConfigDB);
            }
            catch (Exception e)
            {
                //Logging.Logg().LogExceptionToFile(e, "FormMain::Initialize ()");
                //bRes = false;

                Abort(e.Message, true);
            }

            bool bUseData = true; //Для объекта 'AdminTS'

            if (bRes == true)
            {
                formParameters = new FormParameters_FIleINI("setup.ini");
                s_iMainSourceData = Int32.Parse(formParameters.m_arParametrSetup[(int)FormParameters.PARAMETR_SETUP.MAIN_DATASOURCE]);

                if (! (Users.Role == 2)) //Администратор
                {
                    параметрыToolStripMenuItem.Enabled =
                    администрированиеToolStripMenuItem.Enabled =
                    false;
                }
                else;

                //m_arAdmin = new AdminTS[(int)FormChangeMode.MANAGER.COUNT_MANAGER];
                m_arPanelAdmin = new PanelAdmin[(int)FormChangeMode.MANAGER.COUNT_MANAGER];

                for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++) {
                    switch (i)
                    {
                        case (int)FormChangeMode.MANAGER.DISP:
                            m_arPanelAdmin[i] = new PanelAdminKomDisp(idListenerConfigDB);
                            //((PanelAdminKomDisp)m_arPanelAdmin[i]).EventGUIReg += OnPanelAdminKomDispEventGUIReg;
                            ((PanelAdminKomDisp)m_arPanelAdmin[i]).EventGUIReg = new DelegateStringFunc (OnPanelAdminKomDispEventGUIReg);
                            break;
                        case (int)FormChangeMode.MANAGER.NSS:
                            m_arPanelAdmin[i] = new PanelAdminNSS(idListenerConfigDB);
                            break;
                        default:
                            break;
                    }

                    m_arPanelAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                    m_arPanelAdmin[i].SetDelegateReport(ErrorReport, ActionReport);
                }

                formChangeMode = new FormChangeMode(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec, m_ContextMenuStripListTecViews);

                m_passwords = new Passwords ();
                formPassword = new FormPassword(m_passwords);
                formSetPassword = new FormSetPassword(m_passwords);
                formGraphicsSettings = new FormGraphicsSettings(this, delegateUpdateActiveGui, delegateHideGraphicsSettings);

                if (bRes == true)
                    timer.Start();
                else
                    ;
            }
            else
            {
                if (!(formChangeMode == null))
                    formChangeMode = new FormChangeMode(new List<StatisticCommon.TEC>(), m_ContextMenuStripListTecViews);
                else
                    ;
            }

            if (!(formChangeMode == null))
                formChangeMode.ev_сменитьРежим += сменитьРежимToolStripMenuItem_Click;
            else
               ;

            tecViews = new List<PanelTecViewBase>();

            prevStateIsAdmin = FormChangeMode.MANAGER.UNKNOWN;

            m_prevSelectedIndex = 1;

            DbSources.Sources().UnRegister(idListenerConfigDB);

            return bRes;
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
                this.BeginInvoke(new DelegateStringFunc(panelAdminKomDispEventGUIReg), text);
                //panelAdminKomDispEventGUIReg(text);
            }
            catch (Exception e)
            {
                Logging.Logg().LogExceptionToFile(e, @"FormMain::OnPanelAdminKomDispEventGUIReg (string) - text=" + text);
            }
        }

        void delegateOnCloseTab(object sender, CloseTabEventArgs e)
        {
            //formChangeMode.SetItemChecked(m_ContextMenuStripListTecViews.Items.IndexOfKey(e.TabHeaderText), false);

            //ToolStripItem []items = m_ContextMenuStripListTecViews.Items.Find (e.TabHeaderText, true);
            //formChangeMode.SetItemChecked(m_ContextMenuStripListTecViews.Items.IndexOf(items [0]), false);

            if (tclTecViews.TabPages [e.TabIndex].Controls [0] is PanelTecViewBase) {
                formChangeMode.SetItemChecked(e.TabHeaderText, false);
            }
            else
                if ((tclTecViews.TabPages[e.TabIndex].Controls[0] is PanelAdminKomDisp) || (tclTecViews.TabPages[e.TabIndex].Controls[0] is PanelAdminNSS))
                {
                    formChangeMode.SetItemChecked(-1, false);
                }
                else
                    if (tclTecViews.TabPages [e.TabIndex].Controls [0] is PanelCurPower) {
                        значенияТекущаяМощностьГТПгТЭЦснToolStripMenuItem.Checked = false;
                    }
                    else
                        if (tclTecViews.TabPages [e.TabIndex].Controls [0] is PanelTMSNPower) {
                            значенияТекущаяМощностьТЭЦгТЭЦснToolStripMenuItem.Checked = false;
                        }
                        else
                            if (tclTecViews.TabPages [e.TabIndex].Controls [0] is PanelLastMinutes) {
                                мониторингПоследняяМинутаЧасToolStripMenuItem.Checked = false;
                            }
                            else
                                if (tclTecViews.TabPages [e.TabIndex].Controls [0] is PanelCustomTecView) {
                                    выборОбъектыToolStripMenuItem.Checked = false;
                                }
                                else
                                    ;
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Stop(FormClosingEventArgs e = null)
        {
            int i = -1;

            if ((!(formChangeMode == null)) && formChangeMode.admin_was_checked)
            //if ((!(formChangeMode == null)) && (formChangeMode.admin_was_checked[(int)FormChangeMode.MANAGER.DISP] || formChangeMode.admin_was_checked[(int)FormChangeMode.MANAGER.NSS]))
            {
                if (!(m_arPanelAdmin == null))
                    for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++)
                        if (!(m_arPanelAdmin[i] == null))
                            if (m_arPanelAdmin[i].MayToClose() == false)
                                if (!(e == null)) {
                                    e.Cancel = true;
                                    break;
                                }
                                else
                                    ;
                            else {
                                m_arPanelAdmin[i].Stop ();

                                if (i == (int)FormChangeMode.MANAGER.DISP) stopAdminAlarm (); else ;
                                
                            }
                        else
                            ;
                else
                    ;
            }
            else
                stopAdminAlarm ();

            timer.Stop();

            StopTabPages ();

            m_TCPServer.Stop ();
        }

        private void stopAdminAlarm () {
            if ((!(m_arPanelAdmin == null)) && (!(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP] == null)) && (m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP] is PanelAdminKomDisp)
            //if (i == (int)FormChangeMode.MANAGER.DISP)
            && (PanelAdminKomDisp.ALARM_USE == true))
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

            
            значенияТекущаяМощностьГТПгТЭЦснToolStripMenuItem.Checked = false;
            ////В работе постоянно
            //m_panelCurPower.Activate(false);
            //m_panelCurPower.Stop();

            мониторингПоследняяМинутаЧасToolStripMenuItem.Checked = false;
            выборОбъектыToolStripMenuItem.Checked = false;

            if (!(m_panelCurPower == null)) m_panelCurPower.Stop(); else ;
            if (!(m_panelSNPower == null)) m_panelSNPower.Stop(); else ;
            if (!(m_panelLastMinutes == null)) m_panelLastMinutes.Stop(); else ;
            if (!(m_panelCustomTecView == null)) m_panelCustomTecView.Stop(); else ;
        }

        private void ClearTabPages()
        {
            Logging.Logg().LogDebugToFile(@"FormMain::ClearTabPages () - вХод...");
            
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

            activateTabPage(tclTecViews.SelectedIndex, false);

            tclTecViews.SelectedIndexChanged -= tclTecViews_SelectedIndexChanged;

            for (int i = indxRemove.Count - 1; !(i < 0); i--)
            {
                tclTecViews.TabPages.RemoveAt(indxRemove[i]);
            }

            tclTecViews.SelectedIndexChanged += tclTecViews_SelectedIndexChanged;

            //selectedTecViews.Clear();

            Logging.Logg().LogDebugToFile(@"FormMain::ClearTabPages () - вЫход...");
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
                                if (tclTecViews.TabPages[indx].Controls[0] is PanelCustomTecView)
                                    ((PanelCustomTecView)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                                else
                                    if (tclTecViews.TabPages[indx].Controls[0] is PanelAdmin)
                                        ((PanelAdmin)tclTecViews.TabPages[indx].Controls[0]).Activate(active);
                                    else
                                        ;
            }
            else
                strMsgDebug = @"FormMain::activateTabPage () - indx=" + indx + @", active=" + active.ToString();

            Logging.Logg().LogDebugToFile(strMsgDebug + @" - вЫход...");
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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.Cancel == false)
                if (MessageBox.Show(this, "Вы уверены, что хотите закрыть приложение?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    //Нет, не закрываем
                    activateTabPage(tclTecViews.SelectedIndex, true);

                    //Продолжаем и устанавливаем признак: завершить обработку события 'e'
                    e.Cancel = true;
                }
                else
                {
                    //Да, закрываем; признаку оставляем прежнее значение: продолжить обработку события 'e'
                    Stop (e);
                }
            else {
                //Закрываем и устанавливаем признак: продолжить обработку события 'e'
                e.Cancel = false;

                Stop(e);
            }
        }

        private int connectionSettings (CONN_SETT_TYPE type) {
            int iRes = -1;
            DialogResult result;
            result = s_listFormConnectionSettings [(int)type].ShowDialog(this);
            if (result == DialogResult.Yes)
            {
                StopTabPages ();

                if (timer.Enabled) timer.Stop(); else ;
                int i = -1;
                if (!(m_arPanelAdmin == null))
                    for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++) {
                        if (!(m_arPanelAdmin[i] == null)) m_arPanelAdmin[i].Stop(); else ;
                    }
                else
                    ;

                if (Initialize() == true)
                    iRes = 0;
                else
                    //iRes = 1;
                    Abort(@"Ошибка инициализации пользовательских компонентов формы");

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
                        for (int i = 0; i < formChangeMode.m_list_tec_index.Count; i++)
                        {
                            if ((i < tecViews.Count) && !(tecViews[i] == null)) tecViews[i].Stop(); else ;
                        }
                    else
                        ;

                    formChangeMode.btnClearAll_Click(formChangeMode, new EventArgs());

                    formChangeMode.admin_was_checked = false;
                    prevStateIsAdmin = FormChangeMode.MANAGER.UNKNOWN;

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
            int idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
            FormMainAnalyzer formAnalyzer = new FormMainAnalyzer(idListener, formChangeMode.m_list_tec);
            formAnalyzer.ShowDialog (this);
            DbSources.Sources ().UnRegister (idListener);
        }

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
                                delegateRead = m_fileConnSett.ReadSettingsFile;
                                delegateSave = m_fileConnSett.SaveSettingsFile;
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
                        formPassword.SetIdPass(idListener, 0, Passwords.ID_ROLES.ADMIN);
                        DialogResult dlgRes = formPassword.ShowDialog(this);
                        if ((dlgRes == DialogResult.Yes) || (dlgRes == DialogResult.Abort))
                            bShowFormConnectionSettings = true;
                        else
                            ;
                    }
                }

                if (bShowFormConnectionSettings == true)
                    connectionSettings(type);
                else
                    ;

                DbSources.Sources().UnRegister (idListener);
            }
            else
                ;
        }

        private void сменитьРежимToolStripMenuItem_Click()
        {
            StartWait();

            if (tecViews.Count == 0)
            {
                // создаём все tecview
                int tec_indx = 0,
                    comp_indx;
                foreach (StatisticCommon.TEC t in formChangeMode.m_list_tec)
                {
                    if (t.m_bSensorsStrings == false)
                        continue;
                    else
                        ;

                    tecView = new PanelTecViewGraph(t, tec_indx, -1, ErrorReport, ActionReport);
                    tecView.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);
                    tecViews.Add(tecView);
                    if (t.list_TECComponents.Count > 0)
                    {
                        comp_indx = 0;
                        foreach (TECComponent g in t.list_TECComponents)
                        {
                            tecView = new PanelTecViewGraph(t, tec_indx, comp_indx, ErrorReport, ActionReport);
                            tecView.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);
                            tecViews.Add(tecView);
                            comp_indx++;
                        }
                    }
                    else
                        ;

                    tec_indx++;
                }
            }
            else
                ;

            //StartWait();
            ClearTabPages ();

            Int16 parametrsTGBiysk = 0;
            int tecView_index = -1
                , i = -1;
            //List<int> list_tecView_index_visible = new List<int>();
            List<int> list_tecView_index_checked = new List<int>();
            // отображаем вкладки ТЭЦ - аналог PanelCustomTecView::MenuItem_OnClick
            for (i = 0; i < formChangeMode.m_list_tec_index.Count; i++) //или TECComponent_index.Count
            {
                if (!(formChangeMode.was_checked.IndexOf(i) < 0))
                {
                    int tec_index = formChangeMode.m_list_tec_index[i],
                        TECComponent_index = formChangeMode.m_list_TECComponent_index[i];

                    for (tecView_index = 0; tecView_index < tecViews.Count; tecView_index++)
                    {
                        if ((tecViews[tecView_index].indx_TEC == tec_index) && (tecViews[tecView_index].indx_TECComponent == TECComponent_index))
                            break;
                        else
                            ;
                    }

                    if ((tecView_index < tecViews.Count))
                    {
                        list_tecView_index_checked.Add(tecView_index);

                        if ((tecViews[tecView_index].m_tecView.m_tec.type() == StatisticCommon.TEC.TEC_TYPE.BIYSK)/* && (параметрыТГБийскToolStripMenuItem.Visible == false)*/)
                            parametrsTGBiysk++;
                        else
                            ;

                        if (TECComponent_index == -1)
                        {
                            //tclTecViews.TabPages.Add(m_arAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec[tec_index].name);
                            tclTecViews.AddTabPage(formChangeMode.m_list_tec[tec_index].name_shr);
                        }
                        else
                            tclTecViews.AddTabPage (formChangeMode.m_list_tec[tec_index].name_shr + " - " + formChangeMode.m_list_tec[tec_index].list_TECComponents[TECComponent_index].name_shr);

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

            //Реализовано в 'ClearTabPages ()'
            //for (tecView_index = 0; tecView_index < tecViews.Count; tecView_index++)
            //{
            //    if (list_tecView_index_checked.IndexOf(tecView_index) < 0)
            //        tecViews[tecView_index].Stop();
            //    else
            //        ;
            //}

            bool bTGBiysk = parametrsTGBiysk > 0;
            if ((Users.allTEC == 0) || (Users.allTEC == 6)) {
                параметрыТГБийскToolStripMenuItem.Visible = bTGBiysk;
                параметрыToolStripMenuItem.Enabled = bTGBiysk;
                параметрыПриложенияToolStripMenuItem.Enabled = !bTGBiysk;

                m_formParametersTG = new FormParametersTG_FileINI(@"setup.ini");
            }
            else
                ;

            bool bAdminPanelUse = false;
            StopWait();
            if (formChangeMode.admin_was_checked == true)
            {
                int idListener = DbSources.Sources().Register(s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), false, @"CONFIG_DB");
                Passwords.ID_ROLES idRolesPassword = Passwords.ID_ROLES.COM_DISP;

                switch (Users.Role) {
                    case (int)Users.ID_ROLES.ADMIN:
                        if (formChangeMode.IsModeTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP) == true)
                            ;
                        else
                            idRolesPassword = Passwords.ID_ROLES.NSS;
                        break;
                    case (int)Users.ID_ROLES.KOM_DISP:
                        break;
                    case (int)Users.ID_ROLES.NSS:
                        idRolesPassword = Passwords.ID_ROLES.NSS;
                        break;
                    default:
                        break;
                }

                formPassword.SetIdPass(idListener, 0, idRolesPassword);

                //if (prevStateIsAdmin == false)
                //if (prevStateIsAdmin < 0)
                if (!(prevStateIsAdmin == modePanelAdmin))
                    switch (formPassword.ShowDialog(this))
                    {
                        case DialogResult.Yes:
                            bAdminPanelUse = true;
                            break;
                        case DialogResult.Retry:
                            formSetPassword.SetIdPass(idListener, 0, formPassword.GetIdRolePassword());
                            if (formSetPassword.ShowDialog(this) == DialogResult.Yes)
                                bAdminPanelUse = true;
                            else
                                ;
                            break;
                        default:
                            break;
                    }
                else
                    bAdminPanelUse = true;

                if (bAdminPanelUse == true)
                {
                    StartWait();

                    m_arPanelAdmin[(int)modePanelAdmin].Start();
                    AddTabPageAdmin();

                    StopWait();

                    //Реализовано в 'ClearTabPages ()'
                    //for (i = (int)FormChangeMode.MANAGER.DISP; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i++)
                    //    if (! (i == (int)modePanelAdmin)) m_arPanelAdmin[i].Stop(); else ;
                }
                else
                    formChangeMode.admin_was_checked = false;

                DbSources.Sources ().UnRegister (idListener);
            }
            else {
                //Реализовано в 'ClearTabPages ()'
                //for (i = (int)FormChangeMode.MANAGER.DISP; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++)
                //    m_arPanelAdmin[i].Stop();
            }

            //prevStateIsAdmin = formChangeMode.getModeTECComponent ();
            if (formChangeMode.admin_was_checked == true)
                prevStateIsAdmin = modePanelAdmin;
            else
                prevStateIsAdmin = FormChangeMode.MANAGER.UNKNOWN;

            ActivateTabPage();
        }

        private void сменитьРежимToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (s_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                int i = -1;
                if ((!(formChangeMode == null)) && (formChangeMode.ShowDialog() == System.Windows.Forms.DialogResult.OK))
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

        private void tclTecViews_SelectedIndexChanged(object sender, EventArgs e)
        {
            StatisticCommon.FormChangeMode.MANAGER modeAdmin = FormChangeMode.MANAGER.NSS;

            if (formChangeMode.IsModeTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP) == true)
                modeAdmin = FormChangeMode.MANAGER.DISP;
            else
                ;

            ActivateTabPage ();
        }

        protected override bool UpdateStatusString()
        {
            bool have_eror = false;
            m_lblDescError.Text = m_lblDateError.Text = string.Empty;
            PanelTecViewBase selTecView = null;

            //for (int i = 0; i < selectedTecViews.Count; i++)
            //if ((selectedTecViews.Count > 0) /*&& (! (m_prevSelectedIndex < 0))*/)
            if ((!(m_prevSelectedIndex < 0)) && (m_prevSelectedIndex < tclTecViews.TabPages.Count))
            {
                if (tclTecViews.TabPages[m_prevSelectedIndex].Controls[0] is PanelTecViewBase)
                {
                    selTecView = (PanelTecViewBase)tclTecViews.TabPages[m_prevSelectedIndex].Controls[0];

                    if (!(selTecView == null) && ((!(selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_ASKUE] == null)) && (!(selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_SOTIASSO] == null))))
                    {
                        if ((m_report.actioned_state == true) && ((selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_ASKUE].ignore == false) &&
                                                                            (selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_SOTIASSO].ignore == false)))
                        {
                            if (selTecView.isActive == true)
                            {
                                m_lblDateError.Text = m_report.last_time_action.ToString();
                                m_lblDescError.Text = m_report.last_action;
                            }
                            else
                                ;
                        }
                        else
                            ;

                        if ((m_report.errored_state == true) && ((selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_ASKUE].ignore == false) &&
                                                                            (selTecView.m_tecView.m_tec.connSetts[(int)CONN_SETT_TYPE.DATA_SOTIASSO].ignore == false)))
                        {
                            have_eror = true;
                            if (selTecView.isActive == true)
                            {
                                m_lblDateError.Text = m_report.last_time_error.ToString();
                                m_lblDescError.Text = m_report.last_error;
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
                    for (int i = (int)FormChangeMode.MANAGER.DISP; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i++)
                        if (m_arPanelAdmin [i].isActive == true)
                        {
                            if (m_report.actioned_state == true)
                            {
                                m_lblDateError.Text = m_report.last_time_action.ToString();
                                m_lblDescError.Text = m_report.last_action;
                            }
                            else
                                ;

                            if (m_report.errored_state == true)
                            {
                                have_eror = true;
                                m_lblDateError.Text = m_report.last_time_error.ToString();
                                m_lblDescError.Text = m_report.last_error;
                            }
                            else
                                ;
                        }
                        else
                            ;

                    if (m_panelCurPower.m_bIsActive == true)
                    {
                        if (m_report.actioned_state == true)
                        {
                            m_lblDateError.Text = m_report.last_time_action.ToString();
                            m_lblDescError.Text = m_report.last_action;
                        }
                        else
                            ;

                        if (m_report.errored_state == true)
                        {
                            have_eror = true;
                            m_lblDateError.Text = m_report.last_time_error.ToString();
                            m_lblDescError.Text = m_report.last_error;
                        }
                        else
                            ;
                    }
                    else
                        ;

                    if (m_panelSNPower.m_bIsActive == true)
                    {
                        if (m_report.actioned_state == true)
                        {
                            m_lblDateError.Text = m_report.last_time_action.ToString();
                            m_lblDescError.Text = m_report.last_action;
                        }
                        else
                            ;

                        if (m_report.errored_state == true)
                        {
                            have_eror = true;
                            m_lblDateError.Text = m_report.last_time_error.ToString();
                            m_lblDescError.Text = m_report.last_error;
                        }
                        else
                            ;
                    }
                    else
                        ;

                    if (m_panelLastMinutes.m_bIsActive == true)
                    {
                        if (m_report.actioned_state == true)
                        {
                            m_lblDateError.Text = m_report.last_time_action.ToString();
                            m_lblDescError.Text = m_report.last_action;
                        }
                        else
                            ;

                        if (m_report.errored_state == true)
                        {
                            have_eror = true;
                            m_lblDateError.Text = m_report.last_time_error.ToString();
                            m_lblDescError.Text = m_report.last_error;
                        }
                        else
                            ;
                    }
                    else
                        ;

                    if (m_panelCustomTecView.m_bIsActive == true)
                    {
                        if (m_report.actioned_state == true)
                        {
                            m_lblDateError.Text = m_report.last_time_action.ToString();
                            m_lblDescError.Text = m_report.last_action;
                        }
                        else
                            ;

                        if (m_report.errored_state == true)
                        {
                            have_eror = true;
                            m_lblDateError.Text = m_report.last_time_error.ToString();
                            m_lblDescError.Text = m_report.last_error;
                        }
                        else
                            ;
                    }
                    else
                        ;
                }
            }
            else
                ;

            return have_eror;
        }

        private void AddTabPageAdmin () {
            StatisticCommon.FormChangeMode.MODE_TECCOMPONENT mode = FormChangeMode.MODE_TECCOMPONENT.TEC; //FormChangeMode.MODE_TECCOMPONENT.TG;
            StatisticCommon.FormChangeMode.MANAGER modeAdmin = FormChangeMode.MANAGER.NSS;

            if (Users.RoleIsDisp == true) {
                switch (Users.Role) {
                    case (int)Users.ID_ROLES.ADMIN:
                        if (formChangeMode.IsModeTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP) == true)
                        {
                            mode = FormChangeMode.MODE_TECCOMPONENT.GTP;
                            modeAdmin = FormChangeMode.MANAGER.DISP;
                        }
                        else
                            mode = FormChangeMode.MODE_TECCOMPONENT.TEC; //PC или TG не важно
                        break;
                    case (int)Users.ID_ROLES.KOM_DISP:
                        mode = FormChangeMode.MODE_TECCOMPONENT.GTP;
                        modeAdmin = FormChangeMode.MANAGER.DISP;
                        break;
                    case (int)Users.ID_ROLES.NSS:
                        break;
                    default:
                        break;
                }

                tclTecViews.AddTabPage(formChangeMode.getNameAdminValues(mode));

                tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(m_arPanelAdmin[(int)modeAdmin]);

                m_arPanelAdmin[(int)modeAdmin].InitializeComboBoxTecComponent(mode);

                //m_arPanelAdmin[(int)modeAdmin].Activate(true);
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

                    strIdItems = formChangeMode.getIdItemsCheckedIndicies ();
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

        private void timer_Tick(object sender, EventArgs e)
        {
            if (timer.Interval == 666)
            {
                int i = -1;

                // отображаем вкладки ТЭЦ
                int index;
                for (i = 0; i < formChangeMode.m_list_tec_index.Count; i++)
                {
                    StatisticCommon.TEC t = m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec[formChangeMode.m_list_tec_index[i]];

                    if ((index = formChangeMode.was_checked.IndexOf(i)) >= 0)
                    {
                        if (formChangeMode.m_list_TECComponent_index[formChangeMode.was_checked[index]] == -1)
                            tclTecViews.AddTabPage(t.name_shr);
                        else
                            tclTecViews.AddTabPage(t.name_shr + " - " + t.list_TECComponents[formChangeMode.m_list_TECComponent_index[formChangeMode.was_checked[index]]].name_shr);

                        tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(tecViews[i]);

                        tecViews[i].Start();
                    }
                    else
                        ;
                }

                if (tclTecViews.TabPages.Count > 0)
                {
                    ActivateTabPage ();
                }
                else
                    ;

                if (formChangeMode.admin_was_checked == true)
                {
                    //Никогда не выполняется...
                    //if (formPassword.ShowDialog() == DialogResult.Yes)
                    {
                        AddTabPageAdmin ();
                    }
                }
                else
                    ;

                if ((PanelAdminKomDisp.ALARM_USE == true) && (! (((PanelAdminKomDisp)m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP]).m_adminAlarm == null))
                    && (PanelAdminKomDisp.ALARM_USE == true))
                    ((PanelAdminKomDisp)m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP]).m_adminAlarm.Start();
                else
                    ;
                
                m_panelCurPower = new PanelCurPower(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec, ErrorReport, ActionReport);
                m_panelCurPower.SetDelegate(null, null, delegateEvent);
                //m_panelCurPower.Start();
                ////В работе постоянно
                //m_panelCurPower.Activate (true);

                m_panelSNPower = new PanelTMSNPower(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec, ErrorReport, ActionReport);
                m_panelSNPower.SetDelegate(null, null, delegateEvent);

                m_panelLastMinutes = new PanelLastMinutes(m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec, ErrorReport, ActionReport);
                m_panelLastMinutes.SetDelegate(null, null, delegateEvent);
                //m_panelLastMinutes.Start();

                m_panelCustomTecView = new PanelCustomTecView(formChangeMode, ErrorReport, ActionReport);
                //m_panelLastMinutes.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);
                m_panelLastMinutes.SetDelegate(null, null, delegateEvent);
                //m_panelLastMinutes.Start();

                timer.Interval = 1000;
            }

            lock (lockEvent)
            {
                bool have_eror = UpdateStatusString();

                if (have_eror == true)
                    m_lblMainState.Text = "ОШИБКА";
                else
                    ;

                if ((have_eror == false) || (show_error_alert == false))
                    m_lblMainState.Text = "";
                else
                    ;

                show_error_alert = !show_error_alert;
                m_lblDescError.Invalidate();
                m_lblDateError.Invalidate();
            }
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout a = new FormAbout();
            a.ShowDialog(this);
        }

        private void панельГрафическихToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (s_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                if (панельГрафическихToolStripMenuItem.Checked)
                    formGraphicsSettings.Show();
                else
                    formGraphicsSettings.Hide();
            }
            else
                ;

        }

        public void HideGraphicsSettings()
        {
            панельГрафическихToolStripMenuItem.Checked = false;
        }

        private void значенияТекущаяМощностьГТПгТЭЦснToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            string nameTab = "P тек ГТПг, ТЭЦсн";
            //if (((ToolStripMenuItem) sender).Checked == true) {
            if (значенияТекущаяМощностьГТПгТЭЦснToolStripMenuItem.Checked == true)
            {
                tclTecViews.AddTabPage(nameTab);
                tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(m_panelCurPower);

                m_panelCurPower.Start();
                ActivateTabPage();
            }
            else
            {
                tclTecViews.TabPages.RemoveByKey(HTabCtrlEx.GetNameTab(nameTab));
                m_panelCurPower.Activate(false);
                m_panelCurPower.Stop();
            }
        }

        private void значенияТекущаяМощностьТЭЦгТЭЦснToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            string nameTab = "P тек ТЭЦг, ТЭЦсн";
            //if (((ToolStripMenuItem) sender).Checked == true) {
            if (значенияТекущаяМощностьТЭЦгТЭЦснToolStripMenuItem.Checked == true)
            {
                tclTecViews.AddTabPage(nameTab);
                tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(m_panelSNPower);

                m_panelSNPower.Start();
                ActivateTabPage();
            }
            else
            {
                tclTecViews.TabPages.RemoveByKey(HTabCtrlEx.GetNameTab(nameTab));
                m_panelSNPower.Activate(false);
                m_panelSNPower.Stop();
            }
        }

        private void мониторингПоследняяМинутаЧасToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            string nameTab = "Монитор P-2%";
            if (((ToolStripMenuItem)sender).Checked == true)
            {
                tclTecViews.AddTabPage(nameTab);
                tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(m_panelLastMinutes);

                m_panelLastMinutes.Start();
                ActivateTabPage();
            }
            else
            {
                tclTecViews.TabPages.RemoveByKey(HTabCtrlEx.GetNameTab (nameTab));
                m_panelLastMinutes.Activate(false);
                m_panelLastMinutes.Stop();
            }
        }

        private void выборОбъектыToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            string nameTab = @"Объекты по выбору";
            if (((ToolStripMenuItem)sender).Checked == true)
            {
                tclTecViews.AddTabPage(nameTab);
                tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(m_panelCustomTecView);

                m_panelCustomTecView.Start();
                ActivateTabPage();
            }
            else
            {
                tclTecViews.TabPages.RemoveByKey(HTabCtrlEx.GetNameTab(nameTab));
                m_panelLastMinutes.Activate(false);
                m_panelLastMinutes.Stop();
            }
        }

        private void UpdateActiveGui()
        {
            //if (tclTecViews.SelectedIndex >= 0 && tclTecViews.SelectedIndex < selectedTecViews.Count)
            if (tclTecViews.TabPages[tclTecViews.SelectedIndex].Controls[0] is PanelTecViewBase)
                //selectedTecViews[tclTecViews.SelectedIndex].UpdateGraphicsCurrent();
                ((PanelTecViewGraph)tclTecViews.TabPages[tclTecViews.SelectedIndex].Controls[0]).UpdateGraphicsCurrent();
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
                    if ((tv.m_tecView.m_tec.type() == StatisticCommon.TEC.TEC_TYPE.BIYSK) && (!(m_formParametersTG == null)))
                    {
                        //tv.tec.parametersTGForm.ShowDialog(this);
                        m_formParametersTG.ShowDialog(this);
                        break;
                    }
                    else
                        Logging.Logg().LogErrorToFile(@"FormMain::параметрыТГБийскToolStripMenuItem_Click () - m_formParametersTG == null");
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
                    MessageBox.Show (this, "В БД конфигурации внесены изменения.\n\rНеобходим перезапуск приложения.\n\r", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Stop);
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

        private void изменитьПарольНСС_Click(object sender, EventArgs e)
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
            foreach (ToolStripMenuItem item in owner.DropDownItems)
            {
                if (item.DropDownItems.Count > 0 && item.Enabled == true)
                {
                    return getSelectedMenuItem (item);
                }
                else
                {
                    if (item.Selected == true) {
                        return item;
                    }
                    else
                        ;
                }
            }

            return null;
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
                }
                else
                    ;
            }

           if ((!(selItem == null)) && (selItem.Text.Equals(@"Выход") == true))
                activateTabPage(tclTecViews.SelectedIndex, true);
            else
                ;
        }

        FormChangeMode.MANAGER modePanelAdmin {
            get {
                FormChangeMode.MANAGER modeRes = FormChangeMode.MANAGER.UNKNOWN;

                if (formChangeMode.admin_was_checked == true) {
                    switch (Users.Role) {
                        case (int)Users.ID_ROLES.ADMIN:
                            if (formChangeMode.IsModeTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP) == true)
                                modeRes = FormChangeMode.MANAGER.DISP;
                            else
                                modeRes = FormChangeMode.MANAGER.NSS;
                            break;
                        case (int)Users.ID_ROLES.KOM_DISP:
                            modeRes = FormChangeMode.MANAGER.DISP;
                            break;
                        case (int)Users.ID_ROLES.NSS:
                            modeRes = FormChangeMode.MANAGER.NSS;
                            break;
                        default:
                            break;
                    }

                    return modeRes;
                }
                else
                    return FormChangeMode.MANAGER.DISP;
            }
        }
    }
}