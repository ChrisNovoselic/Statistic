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
        private ConnectionSettingsSource m_connSettSource;
        private List <FormConnectionSettings> m_listFormConnectionSettings;
        private PanelAdmin [] m_arPanelAdmin;
        public AdminTS [] m_arAdmin;
        //public Users m_user;
        public Passwords m_passwords;
        private List<TecView> tecViews;
        private List<TecView> selectedTecViews;
        private FormPassword formPassword;
        private FormSetPassword formSetPassword;
        private FormChangeMode formChangeMode;
        private TecView tecView;
        private int m_prevSelectedIndex;
        private int prevStateIsAdmin;
        public FormGraphicsSettings formGraphicsSettings;
        public FormParameters formParameters;
        //public FormParametersTG parametersTGForm;

        TcpServerAsync m_TCPServer;

        private void Abort (bool bThrow = false)
        {
            string strThrowMsg = "Ошибка инициализации пользовательских компонентов формы";
            MessageBox.Show(this, strThrowMsg + ".\nОбратитесь к оператору тех./поддержки по тел. 4444 или по тел. 289-03-37.", "Ошибка инициализации!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            if (bThrow == true) throw new Exception(strThrowMsg); else ;
        }

        public FormMain()
        {
            InitializeComponent();

            // m_statusStripMain
            this.m_statusStripMain.Location = new System.Drawing.Point(0, 762);
            this.m_statusStripMain.Size = new System.Drawing.Size(982, 22);
            // m_lblMainState
            this.m_lblMainState.Size = new System.Drawing.Size(150, 17);
            // m_lblDateError
            this.m_lblDateError.Size = new System.Drawing.Size(150, 17);
            // m_lblDescError
            this.m_lblDescError.Size = new System.Drawing.Size(667, 17);

            delegateUpdateActiveGui = new DelegateFunc(UpdateActiveGui);
            delegateHideGraphicsSettings = new DelegateFunc(HideGraphicsSettings);

            m_fileConnSett = new FIleConnSett ("connsett.ini");
            m_listFormConnectionSettings = new List<FormConnectionSettings> ();
            m_listFormConnectionSettings.Add (new FormConnectionSettings(m_fileConnSett.ReadSettingsFile, m_fileConnSett.SaveSettingsFile));
            m_listFormConnectionSettings.Add(null);
            if (m_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {                
                if (Initialize() == false)
                {
                    Abort (true);
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
            m_TCPServer.Start ();
        }

        private bool Initialize()
        {
            bool bRes = true;
            int i = -1;

            timer.Interval = 666; //Признак первого старта

            m_passwords = new Passwords();
            m_passwords.connSettConfigDB = m_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett();

            Users user = null;
            try { user = new Users(m_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett()); }
            catch (Exception e)
            {
                Logging.Logg().LogExceptionToFile(e, "FormMain::Initialize ()");
                bRes = false;
            }

            bool bUseData = true; //Для объекта 'AdminTS'

            if (bRes == true)
            {
                if (! (user.Role == 2)) //Администратор
                {
                    параметрыToolStripMenuItem.Enabled =
                    администрированиеToolStripMenuItem.Enabled =
                    false;
                }
                else;

                m_arAdmin = new AdminTS[(int)FormChangeMode.MANAGER.COUNT_MANAGER];
                m_arPanelAdmin = new PanelAdmin[(int)FormChangeMode.MANAGER.COUNT_MANAGER];

                for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++) {
                    switch (i) {
                        case (int)FormChangeMode.MANAGER.DISP:
                            m_arAdmin[i] = new AdminTS();
                            break;
                        case (int)FormChangeMode.MANAGER.NSS:
                            m_arAdmin[i] = new AdminTS_NSS();
                            bUseData = false;
                            break;
                        default:
                            break;
                    }

                    //Logging.Logg().LogDebugToFile("FormMain::Initialize () - Создание объекта m_arAdmin[i]; i = " + i);

                    //m_admin.SetDelegateTECComponent(FillComboBoxTECComponent);
                    try { m_arAdmin[i].InitTEC(m_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), FormChangeMode.MODE_TECCOMPONENT.UNKNOWN, false, bUseData); }
                    catch (Exception e)
                    {
                        Logging.Logg().LogExceptionToFile(e, "FormMain::Initialize () - m_arAdmin[i].InitTEC (); i = " + i);
                        bRes = false;
                        break;
                    }
                    if (!(m_arAdmin[i].m_list_tec.Count > 0)) {
                        bRes = false;
                        break;
                    }
                    else
                        ;

                    switch (i)
                    {
                        case (int)FormChangeMode.MANAGER.DISP:
                            m_arPanelAdmin[i] = new PanelAdminKomDisp(m_arAdmin[i]);
                            break;
                        case (int)FormChangeMode.MANAGER.NSS:
                            m_arPanelAdmin[i] = new PanelAdminNSS(m_arAdmin[i]);
                            break;
                        default:
                            break;
                    }

                    m_arAdmin[i].SetDelegateData(m_arPanelAdmin[i].setDataGridViewAdmin);
                    m_arAdmin[i].SetDelegateDatetime(m_arPanelAdmin[i].CalendarSetDate);

                    m_arAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                    m_arAdmin[i].SetDelegateReport(ErrorReport, ActionReport);

                    m_arAdmin[i].m_typeFields = AdminTS.TYPE_FIELDS.STATIC;
                }

                formChangeMode = new FormChangeMode(m_arAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec, m_ContextMenuStripListTecViews, сменитьРежимToolStripMenuItem_Click);

                //formChangeMode = new FormChangeMode();
                formPassword = new FormPassword(m_passwords);
                formSetPassword = new FormSetPassword(m_passwords);
                formGraphicsSettings = new FormGraphicsSettings(this, delegateUpdateActiveGui, delegateHideGraphicsSettings);
                formParameters = new FormParameters("setup.ini");

                if (bRes == true)
                    timer.Start();
                else
                    ;
            }
            else
            {
                if (! (m_arAdmin == null))
                    for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++)
                        if (!(m_arAdmin[i] == null))
                        {
                            switch (i)
                            {
                                case (int)FormChangeMode.MANAGER.DISP:
                                    break;
                                case (int)FormChangeMode.MANAGER.NSS:
                                    bUseData = false;
                                    break;
                                default:
                                    break;
                            }
                            m_arAdmin[i].InitTEC(null, FormChangeMode.MODE_TECCOMPONENT.UNKNOWN, false, bUseData);
                        }
                        else
                            ;
                else
                    ;

                if (!(formChangeMode == null))
                    formChangeMode = new FormChangeMode(new List <TEC> (), m_ContextMenuStripListTecViews, сменитьРежимToolStripMenuItem_Click);
                else
                    ;
            }

            tecViews = new List<TecView>();
            selectedTecViews = new List<TecView>();

            prevStateIsAdmin = -1;

            m_prevSelectedIndex = 1;

            return bRes;
        }

        void delegateOnCloseTab(object sender, CloseTabEventArgs e)
        {
            //formChangeMode.SetItemChecked(m_ContextMenuStripListTecViews.Items.IndexOfKey(e.TabHeaderText), false);

            //ToolStripItem []items = m_ContextMenuStripListTecViews.Items.Find (e.TabHeaderText, true);
            //formChangeMode.SetItemChecked(m_ContextMenuStripListTecViews.Items.IndexOf(items [0]), false);            

            formChangeMode.SetItemChecked(e.TabIndex - 1, false);            
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
                if (!(m_arAdmin == null))
                    for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++)
                        if (!(m_arAdmin [i] == null))
                            if (m_arPanelAdmin[i].MayToClose() == false)
                                if (!(e == null)) {
                                    e.Cancel = true;
                                    break;
                                }
                                else
                                    ;
                            else
                                ;
                        else
                            ;
                else
                    ;
            }
            else
                ;

            timer.Stop();

            if (!(m_arAdmin == null))
                for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++) {
                    if (!(m_arAdmin [i] == null))
                        if ((e.Cancel == false) && ((!(m_arAdmin [i] == null)) && (!(m_arAdmin [i].m_list_tec == null))))
                        {                
                            foreach (TEC t in m_arAdmin[i].m_list_tec)
                                t.StopDbInterfaceForce();

                            m_arAdmin [i].StopThreadSourceData();
                        }
                        else
                            ;
                    else
                        ;
                }
            else
                ;

            if (! (m_passwords == null))
                ; //m_passwords.StopDbInterface();
            else
                ;

            m_TCPServer.Stop ();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.Cancel == false)
                if (MessageBox.Show(this, "Вы уверены, что хотите закрыть приложение?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
                else
                {
                    Stop (e);
                }
            else {
                e.Cancel = false;

                Stop(e);

                //Application.Exit (
            }
        }

        private int connectionSettings (CONN_SETT_TYPE type) {
            int iRes = -1;
            DialogResult result;
            result = m_listFormConnectionSettings [(int)type].ShowDialog(this);
            if (result == DialogResult.Yes)
            {
                if (! (tecViews == null)) tecViews.Clear (); else ;

                if (timer.Enabled) timer.Stop(); else ;
                int i = -1;
                if (!(m_arAdmin == null))
                    for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++) {
                        if (!(m_arAdmin [i] == null)) m_arAdmin [i].StopThreadSourceData(); else ;
                    }
                else
                    ;

                if (Initialize() == true)
                    iRes = 0;
                else
                    //iRes = 1;
                    Abort ();

                //foreach (TecView t in tecViews)
                //{
                //    t.Reinit();
                //}

                //m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].Reinit();
            }
            else
                ;

            return iRes;
        }

        private void closeTecViewsTabPages ()
        {
            if (tclTecViews.TabPages.Count > 0)
                if (! (MessageBox.Show(this, "Вы уверены, что хотите закрыть текущие вкладки?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes))
                {
                    return ; //e.Cancel = true;
                }
                else
                {
                    StartWait();
                    tclTecViews.TabPagesClear();
                    selectedTecViews.Clear();

                    if (! (tecViews == null))
                        for (int i = 0; i < formChangeMode.m_list_tec_index.Count; i++)
                        {
                            if ((i < tecViews.Count) && !(tecViews[i] == null)) tecViews[i].Stop(); else ;
                        }
                    else
                        ;

                    formChangeMode.btnClearAll_Click(formChangeMode, new EventArgs());

                    formChangeMode.admin_was_checked = false;
                    prevStateIsAdmin = -1;

                    StopWait();

                    this.Focus ();
                }
            else
                ;
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
            FormMainAnalyzer formAnalyzer = new FormMainAnalyzer(m_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett(), formChangeMode.m_list_tec);
            formAnalyzer.ShowDialog (this);
        }

        private void настройкиСоединенияToolStripMenuItem_Click(object sender, EventArgs e, CONN_SETT_TYPE type)
        {
            closeTecViewsTabPages ();

            //???
            //string strPassword = "password";
            //MD5CryptoServiceProvider md5;
            //md5 = new MD5CryptoServiceProvider();
            //StringBuilder strPasswordHashed = new StringBuilder ();
            //byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(strPassword));

            bool bShowFormConnectionSettings = false;
            if (formPassword == null)
            {
                bShowFormConnectionSettings = true;
            }
            else {
                if ((!(m_listFormConnectionSettings == null)) &&
                    (m_listFormConnectionSettings[(int)type] == null) && (!(m_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB] == null)))
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
                            if (m_connSettSource == null)
                                m_connSettSource = new ConnectionSettingsSource (m_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett ());
                            else
                                ;

                            delegateRead = m_connSettSource.Read;
                            delegateSave = m_connSettSource.Save;
                            break;
                        default:
                            break;
                    }

                    if ((!(delegateRead == null)) && (!(delegateSave == null)))
                        m_listFormConnectionSettings[(int)type] = new FormConnectionSettings (delegateRead, delegateSave);
                    else
                        Abort (false);
                }
                else
                    ;

                if ((!(m_listFormConnectionSettings[(int)type] == null)) && (!(m_listFormConnectionSettings[(int)type].Ready == 0)))
                {
                    bShowFormConnectionSettings = true;
                }
                else {
                    formPassword.SetIdPass(0, Passwords.ID_ROLES.ADMIN);
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
        }

        private void сменитьРежимToolStripMenuItem_Click()
        {
            StartWait();

            if (tecViews.Count == 0)
            {
                /*
                m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].StopDbInterface ();
                m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].Stop();

                //m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].InitTEC (formChangeMode.tec);
                //m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].mode(formChangeMode.getModeTECComponent ());
                m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].StartDbInterface ();
                */
                // создаём все tecview
                int tec_indx = 0,
                    comp_indx;
                foreach (TEC t in formChangeMode.m_list_tec)
                {
                    tecView = new TecView(t, tec_indx, -1, m_arAdmin[(int)FormChangeMode.MANAGER.DISP], m_statusStripMain, formGraphicsSettings, formParameters);
                    tecView.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);
                    tecViews.Add(tecView);
                    if (t.list_TECComponents.Count > 0)
                    {
                        comp_indx = 0;
                        foreach (TECComponent g in t.list_TECComponents)
                        {
                            tecView = new TecView(t, tec_indx, comp_indx, m_arAdmin[(int)FormChangeMode.MANAGER.DISP], m_statusStripMain, formGraphicsSettings, formParameters);
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
            tclTecViews.TabPagesClear();
            selectedTecViews.Clear();

            Int16 parametrsTGBiysk = 0;
            int tecView_index = -1
                , i = -1;
            //List<int> list_tecView_index_visible = new List<int>();
            List<int> list_tecView_index_checked = new List<int>();
            // отображаем вкладки ТЭЦ
            for (i = 0; i < formChangeMode.m_list_tec_index.Count; i++) //или TECComponent_index.Count
            {
                if (!(formChangeMode.was_checked.IndexOf(i) < 0))
                {
                    int tec_index = formChangeMode.m_list_tec_index[i],
                        TECComponent_index = formChangeMode.m_list_TECComponent_index[i];

                    for (tecView_index = 0; tecView_index < tecViews.Count; tecView_index++)
                    {
                        if ((tecViews[tecView_index].num_TEC == tec_index) && (tecViews[tecView_index].num_TECComponent == TECComponent_index))
                            break;
                        else
                            ;
                    }

                    if ((tecView_index < tecViews.Count))
                    {
                        list_tecView_index_checked.Add(tecView_index);

                        if ((tecViews[tecView_index].tec.type() == TEC.TEC_TYPE.BIYSK)/* && (параметрыТГБийскToolStripMenuItem.Visible == false)*/)
                            parametrsTGBiysk++;
                        else
                            ;

                        if (TECComponent_index == -1)
                        {
                            //tclTecViews.TabPages.Add(m_arAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec[tec_index].name);
                            tclTecViews.TabPages.Add(formChangeMode.m_list_tec[tec_index].name_shr + new string(' ', 3));
                        }
                        else
                            tclTecViews.TabPages.Add(formChangeMode.m_list_tec[tec_index].name_shr + " - " + formChangeMode.m_list_tec[tec_index].list_TECComponents[TECComponent_index].name_shr + new string(' ', 3));

                        tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(tecViews[tecView_index]);
                        selectedTecViews.Add(tecViews[tecView_index]);

                        tecViews[tecView_index].Activate(false);
                        tecViews[tecView_index].Start();
                    }
                    else
                        ;
                }
                else
                {
                }
            }

            for (tecView_index = 0; tecView_index < tecViews.Count; tecView_index++)
            {
                if (list_tecView_index_checked.IndexOf(tecView_index) < 0)
                    tecViews[tecView_index].Stop();
                else
                    ;
            }

            параметрыТГБийскToolStripMenuItem.Visible = (parametrsTGBiysk > 0 ? true : false);

            bool bAdminPanelUse = false;
            StopWait();
            if (formChangeMode.admin_was_checked)
            {
                if (formChangeMode.IsModeTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP) == true)
                {
                    formPassword.SetIdPass(0, Passwords.ID_ROLES.COM_DISP);
                }
                else
                    formPassword.SetIdPass(0, Passwords.ID_ROLES.NSS);

                //if (prevStateIsAdmin == false)
                //if (prevStateIsAdmin < 0)
                if (!(prevStateIsAdmin == (int)modePanelAdmin))
                    switch (formPassword.ShowDialog(this))
                    {
                        case DialogResult.Yes:
                            bAdminPanelUse = true;
                            break;
                        case DialogResult.Retry:
                            formSetPassword.SetIdPass(0, formPassword.GetIdRolePassword());
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
                    AddTabPageAdmin();
                    StopWait();

                    m_arPanelAdmin[(int)modePanelAdmin].Activate(true);
                }
                else
                    formChangeMode.admin_was_checked = false;
            }
            else
                ;

            if (selectedTecViews.Count > 0)
            {
                m_prevSelectedIndex = 1;
                //selectedTecViews[m_prevSelectedIndex - 1].Activate(true);
            }
            else
                if (formChangeMode.admin_was_checked == true)
                    ;
                else
                    ;

            //Проверить предыдущий выбор типа панели 'администратора'
            if (!(prevStateIsAdmin < 0))
                //Одна из панелей 'администратора' в предыдущем наборе вкладок отображалась (активация/деактивация)
                m_arPanelAdmin[prevStateIsAdmin].Activate(false);
            else
                ;

            //Запомнить текущий выбор типа панели 'администратора'
            if (bAdminPanelUse == true)
                prevStateIsAdmin = (int)modePanelAdmin; //formChangeMode.admin_was_checked;
            else
                prevStateIsAdmin = -1;
        }

        private void сменитьРежимToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_listFormConnectionSettings[(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                int i = -1; 
                if (!(formChangeMode == null))
                {
                    // выбираем список отображаемых вкладок
                    formChangeMode.ShowDialog();
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

            if ((tclTecViews.SelectedIndex > 0) &&
                (tclTecViews.SelectedIndex - 1 < selectedTecViews.Count) &&
                (m_prevSelectedIndex > 0) &&
                (m_prevSelectedIndex - 1 < selectedTecViews.Count))
            {
                selectedTecViews[m_prevSelectedIndex - 1].Activate(false);
                selectedTecViews[tclTecViews.SelectedIndex - 1].Activate(true);
                m_prevSelectedIndex = tclTecViews.SelectedIndex;
                m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].Activate(false);
                m_arPanelAdmin[(int)FormChangeMode.MANAGER.NSS].Activate(false);
            }
            else
            {
                if ((m_prevSelectedIndex > 0) &&
                    (m_prevSelectedIndex - 1 < selectedTecViews.Count))
                    selectedTecViews[m_prevSelectedIndex - 1].Activate(false);
                else
                    ;

                if (tclTecViews.SelectedIndex - 1 == selectedTecViews.Count)
                    m_arPanelAdmin[(int)modeAdmin].Activate(true);
                else
                    ;
            }

            //tclTecViews.Invalidate ();
            //this.Invalidate ();
        }

        protected override bool UpdateStatusString()
        {
            bool have_eror = false;
            m_lblDescError.Text = m_lblDateError.Text = "";
            for (int i = 0; i < selectedTecViews.Count; i++)
            {
                if (selectedTecViews[i].actioned_state && !selectedTecViews[i].tec.connSetts [(int) CONN_SETT_TYPE.DATA].ignore)
                {
                    if (selectedTecViews[i].isActive)
                    {
                        m_lblDateError.Text = selectedTecViews[i].last_time_action.ToString();
                        m_lblDescError.Text = selectedTecViews[i].last_action;
                    }
                }
                else
                    ;

                if (selectedTecViews[i].errored_state && !selectedTecViews[i].tec.connSetts[(int)CONN_SETT_TYPE.DATA].ignore)
                {
                    have_eror = true;
                    if (selectedTecViews[i].isActive)
                    {
                        m_lblDateError.Text = selectedTecViews[i].last_time_error.ToString();
                        m_lblDescError.Text = selectedTecViews[i].last_error;
                    }
                }
                else
                    ;
            }

            if (m_arAdmin[(int)modePanelAdmin].actioned_state && m_arPanelAdmin[(int)modePanelAdmin].isActive)
            {
                m_lblDateError.Text = m_arAdmin[(int)modePanelAdmin].last_time_action.ToString();
                m_lblDescError.Text = m_arAdmin[(int)modePanelAdmin].last_action;
            }
            else
                ;

            if (m_arAdmin[(int)modePanelAdmin].errored_state)
            {
                have_eror = true;
                m_lblDateError.Text = m_arAdmin[(int)modePanelAdmin].last_time_error.ToString();
                m_lblDescError.Text = m_arAdmin[(int)modePanelAdmin].last_error;
            }
            else
                ;

            return have_eror;
        }

        private void AddTabPageAdmin () {
            StatisticCommon.FormChangeMode.MODE_TECCOMPONENT mode = FormChangeMode.MODE_TECCOMPONENT.TEC; //FormChangeMode.MODE_TECCOMPONENT.TG;
            StatisticCommon.FormChangeMode.MANAGER modeAdmin = FormChangeMode.MANAGER.NSS;

            if (formChangeMode.IsModeTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP) == true)
            {
                tclTecViews.TabPages.Add(formChangeMode.getNameAdminValues(FormChangeMode.MODE_TECCOMPONENT.GTP) + new string (' ', 3));
                mode = FormChangeMode.MODE_TECCOMPONENT.GTP;
                modeAdmin = FormChangeMode.MANAGER.DISP;
            }
            else
                tclTecViews.TabPages.Add(formChangeMode.getNameAdminValues(FormChangeMode.MODE_TECCOMPONENT.TEC) + new string(' ', 3)); //PC или TG не важно

            tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(m_arPanelAdmin[(int)modeAdmin]);

            m_arPanelAdmin[(int)modeAdmin].InitializeComboBoxTecComponent(mode);

            m_arAdmin[(int)modeAdmin].Resume();
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
                for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++) {
                    m_arAdmin [i].StartThreadSourceData();
                }

                // отображаем вкладки ТЭЦ
                int index;
                for (i = 0; i < formChangeMode.m_list_tec_index.Count; i++)
                {
                    TEC t = m_arAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec[formChangeMode.m_list_tec_index[i]];

                    if ((index = formChangeMode.was_checked.IndexOf(i)) >= 0)
                    {
                        if (formChangeMode.m_list_TECComponent_index[formChangeMode.was_checked[index]] == -1)
                            tclTecViews.TabPages.Add(t.name_shr + new string(' ', 3));
                        else
                            tclTecViews.TabPages.Add(t.name_shr + " - " + t.list_TECComponents[formChangeMode.m_list_TECComponent_index[formChangeMode.was_checked[index]]].name_shr + new string(' ', 3));

                        tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(tecViews[i]);
                        selectedTecViews.Add(tecViews[i]);

                        t.StartDbInterface();
                        tecViews[i].Activate(false);
                        tecViews[i].Start();
                    }
                    else
                        ;
                }

                if ((! (selectedTecViews == null)) && (selectedTecViews.Count > 0))
                {
                    m_prevSelectedIndex = 1;
                    //selectedTecViews[m_prevSelectedIndex].Activate(true);
                    //tclTecViews.SelectedIndex = 1;
                }
                else
                    ;

                if (formChangeMode.admin_was_checked)
                {
                    //Никогда не выполняется...
                    //if (formPassword.ShowDialog() == DialogResult.Yes)
                    {
                        AddTabPageAdmin ();
                    }
                }
                else
                    ;

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
            if (m_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
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

        private void UpdateActiveGui()
        {
            if ((tclTecViews.SelectedIndex > 0) &&
                (tclTecViews.SelectedIndex < selectedTecViews.Count))
                selectedTecViews[tclTecViews.SelectedIndex].UpdateGraphicsCurrent();
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
            if (m_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
                formParameters.ShowDialog(this);
            else
                ;
        }

        private void параметрыТГБийскToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                foreach (TecView tv in tecViews) {
                    if (tv.tec.type () == TEC.TEC_TYPE.BIYSK) {
                        tv.tec.parametersTGForm.ShowDialog(this);
                        break;
                    }
                    else
                        ;
                }
            }
            else
                ;
        }

        private void изментьСоставТЭЦГТПЩУToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formPassword.SetIdPass(0, Passwords.ID_ROLES.ADMIN);
            formPassword.ShowDialog(this);
            DialogResult dlgRes = formPassword.DialogResult;
            if (dlgRes == DialogResult.Yes)
            {
                FormTECComponent tecComponent = new FormTECComponent(m_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett());
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
        }

        private void изментьСоставПользовательToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formPassword.SetIdPass(0, Passwords.ID_ROLES.ADMIN);
            formPassword.ShowDialog(this);
            DialogResult dlgRes = formPassword.DialogResult;
            if (dlgRes == DialogResult.Yes)
            {
                FormUser formUser = new FormUser(m_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].getConnSett());
                
                if (formUser.ShowDialog(this) == DialogResult.Yes)
                {
                    MessageBox.Show(this, "В БД конфигурации внесены изменения.\n\rНеобходим перезапуск приложения.\n\r", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Stop);
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

        private void изменитьПарольToolStripMenuItem_Click(object sender, EventArgs e, int id, Passwords.ID_ROLES id_role)
        {
            if (m_listFormConnectionSettings [(int)CONN_SETT_TYPE.CONFIG_DB].Ready == 0)
            {
                formPassword.SetIdPass(id, id_role);
                DialogResult dlgRes = formPassword.ShowDialog(this);
                if (dlgRes == DialogResult.Yes)
                {
                    formSetPassword.SetIdPass(0, formPassword.GetIdRolePassword());
                    formSetPassword.ShowDialog(this);
                }
                else
                    if (dlgRes == DialogResult.Abort)
                        connectionSettings(CONN_SETT_TYPE.CONFIG_DB);
                    else
                        ;
            }
            else
                ;
        }

        FormChangeMode.MANAGER modePanelAdmin {
            get {
                if (formChangeMode.admin_was_checked) {
                    if (formChangeMode.IsModeTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP) == true)
                        return FormChangeMode.MANAGER.DISP;
                    else
                        return FormChangeMode.MANAGER.NSS;
                }
                else
                    return FormChangeMode.MANAGER.DISP;
            }
        }
    }
}
