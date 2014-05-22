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

//???
//using System.Security.Cryptography;

using StatisticCommon;

namespace Statistic
{
    public partial class FormMain : FormMainBase
    {
        //public List<TEC> tec;
        private FormConnectionSettings m_formConnectionSettings;
        //private PanelAdmin [] m_arPanelAdmin;
        PanelAdmin m_panelAdmin;
        //public AdminTS [] m_arAdmin;
        AdminTS m_admin;
        public Passwords m_passwords;
        private List<TecView> tecViews;
        private List<TecView> selectedTecViews;
        private FormPassword formPassword;
        private FormSetPassword formSetPassword;
        private FormChangeMode formChangeMode;
        private TecView tecView;
        private int m_prevSelectedIndex;
        private bool prevStateIsAdmin;
        private bool prevStateIsPPBR;
        //public static object lockFile = new object();
        //public static string logPath;
        //public static Logging log;
        public FormGraphicsSettings formGraphicsSettings;
        public FormParameters formParameters;
        //public FormParametersTG parametersTGForm;

        public FormMain()
        {
            InitializeComponent();

            delegateEvent = new DelegateFunc(EventRaised);
            delegateUpdateActiveGui = new DelegateFunc(UpdateActiveGui);
            delegateHideGraphicsSettings = new DelegateFunc(HideGraphicsSettings);

            //bool bShowFormConnectionSettings = true;
            m_formConnectionSettings = new FormConnectionSettings("connsett.ini");
            if (m_formConnectionSettings.Protected == true)
            {
                if (Initialize() == false)
                {
                    string strThrowMsg = "Ошибка инициализации пользовательских компонентов формы";
                    MessageBox.Show(this, strThrowMsg + ".\nОбратитесь к оператору тех./поддержки по тел. 4444 или по тел. 289-03-37.", "Ошибка инициализации!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    throw new Exception(strThrowMsg);
                }
                else
                    ;
            }
            else
            {//Файла с параметрами соединения нет совсем
                connectionSettings();
            }
        }

        ~FormMain ()
        {
            Logging.Logg().LogToFile("FormMain::~FormMain () - ...", true, true, true);
        }

        private bool Initialize()
        {
            bool bRes = true;

            timer.Interval = 666; //Признак первого старта

            m_passwords = new Passwords();
            m_passwords.SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
            m_passwords.SetDelegateReport(ErrorReport, ActionReport);
            m_passwords.connSettConfigDB = m_formConnectionSettings.getConnSett();

            //m_arAdmin = new AdminTS[(int)FormChangeMode.MANAGER.COUNT_MANAGER];
            //m_arPanelAdmin = new PanelAdmin[(int)FormChangeMode.MANAGER.COUNT_MANAGER];

            int i = -1;
            //for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++) {
            //    switch (i) {
            //        case (int)FormChangeMode.MANAGER.DISP:
                        m_admin = new AdminTS();
            //            break;
            //        case (int)FormChangeMode.MANAGER.NSS:
            //            m_arAdmin[i] = new AdminTS_NSS();
            //            break;
            //        default:
            //            break;
            //    }

            //    //m_admin.SetDelegateTECComponent(FillComboBoxTECComponent);
                try { m_admin.InitTEC(m_formConnectionSettings.getConnSett(), FormChangeMode.MODE_TECCOMPONENT.UNKNOWN, false, true); }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, "FormMain::Initialize ()");
                    bRes = false;
            //        break;
                }
                if (!(m_admin.m_list_tec.Count > 0)) {
                    bRes = false;
            //        break;
                }
                else
                    ;

            //    switch (i)
            //    {
            //        case (int)FormChangeMode.MANAGER.DISP:
                        m_panelAdmin = new PanelAdmin(m_admin);
            //            break;
            //        case (int)FormChangeMode.MANAGER.NSS:
            //            m_arPanelAdmin[i] = new PanelAdminNSS(m_arAdmin[i]);
            //            break;
            //        default:
            //            break;
            //    }

                m_admin.SetDelegateData(m_panelAdmin.setDataGridViewAdmin);
                m_admin.SetDelegateDatetime(m_panelAdmin.CalendarSetDate);

                m_admin.SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                m_admin.SetDelegateReport(ErrorReport, ActionReport);

                m_admin.delegateMessageBox = MessageBoxDebug;
            //}

            formChangeMode = new FormChangeMode(m_admin.m_list_tec);
            m_prevSelectedIndex = 0;

            //formChangeMode = new FormChangeMode();
            formPassword = new FormPassword(m_passwords);
            formSetPassword = new FormSetPassword(m_passwords);
            formGraphicsSettings = new FormGraphicsSettings(this, delegateUpdateActiveGui, delegateHideGraphicsSettings);
            formParameters = new FormParameters("setup.ini");

            tecViews = new List<TecView>();
            selectedTecViews = new List<TecView>();

            prevStateIsAdmin = false;
            prevStateIsPPBR = false;

            if (bRes == true)
                timer.Start();
            else
                ;

            return bRes;
        }

        private void ErrorReport(string msg)
        {
            stsStrip.BeginInvoke(delegateEvent);
        }

        private void ActionReport(string msg)
        {
            stsStrip.BeginInvoke(delegateEvent);
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
                //if (!(m_admin == null))
                    //for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++)
                        if (!(m_admin == null))
                            if (m_panelAdmin.MayToClose() == false)
                                if (!(e == null)) {
                                    e.Cancel = true;
                    //                break;
                                }
                                else
                                    ;
                            else
                                ;
                        else
                            ;
                //else
                //    ;
            }
            else
                ;

            timer.Stop();

            //if (!(m_arAdmin == null))
            //    for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++) {
            //        if (!(m_arAdmin [i] == null))
                        if ((e.Cancel == false) && ((!(m_admin == null)) && (!(m_admin.m_list_tec == null))))
                        {                
                            foreach (TEC t in m_admin.m_list_tec)
                            {
                                t.StopDbInterfacesForce();
                            }

                            m_admin.StopThreadSourceData();
                        }
                        else
                            ;
            //        else
            //            ;
            //    }
            //else
            //    ;

            if (! (m_passwords == null))
                ; //m_passwords.StopDbInterface();
            else
                ;
        }

        private void MessageBoxDebug (string msg)
        {
            MessageBox.Show (this, msg, "!Отладка!");
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

        private void connectionSettings () {
            DialogResult result;
            result = m_formConnectionSettings.ShowDialog(this);
            if (result == DialogResult.Yes)
            {
                if (! (tecViews == null)) tecViews.Clear (); else ;

                if (timer.Enabled) timer.Stop(); else ;
                int i = -1;
                //if (!(m_arAdmin == null))
                //    for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++) {
                        if (!(m_admin == null)) m_admin.StopThreadSourceData(); else ;
                //    }
                //else
                //    ;

                Initialize();

                //foreach (TecView t in tecViews)
                //{
                //    t.Reinit();
                //}

                //m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].Reinit();
            }
            else
                ;
        }

        private void настройкиСоединенияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tclTecViews.TabPages.Count > 0)
                if (MessageBox.Show(this, "Вы уверены, что хотите закрыть текущие вкладки?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return ; //e.Cancel = true;
                }
                else
                {
                    StartWait();
                    tclTecViews.TabPages.Clear();
                    selectedTecViews.Clear();

                    for (int i = 0; i < formChangeMode.tec_index.Count; i++)
                    {
                        tecViews [i].Stop ();
                    }

                    formChangeMode.btnClearAll_Click(formChangeMode, new EventArgs ());

                    formChangeMode.admin_was_checked =
                    prevStateIsAdmin = false;

                    StopWait();

                    this.Focus ();
                }
            else
                ;

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
                if (m_formConnectionSettings.Protected == false)
                {
                    bShowFormConnectionSettings = true;
                }
                else {
                    formPassword.SetIdPass(FormPassword.ID_ROLES.ADMIN);
                    DialogResult dlgRes = formPassword.ShowDialog(this);
                    if ((dlgRes == DialogResult.Yes) || (dlgRes == DialogResult.Abort))
                        bShowFormConnectionSettings = true;
                    else
                        ;
                }
            }

            if (bShowFormConnectionSettings == true)
                connectionSettings ();
            else
                ;
        }

        private void сменитьРежимToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_formConnectionSettings.Protected == true)
            {
                int i;
                //int index;
                int prevModeComponent = formChangeMode.getModeTECComponent ();
                // выбираем список отображаемых вкладок
                if (formChangeMode.ShowDialog() == DialogResult.OK)
                {
                    StartWait();

                    if ((! (prevModeComponent == formChangeMode.getModeTECComponent()))) {
                        //this.tec = formChangeMode.tec;

                        prevStateIsAdmin = false;

                        //tecViews.Clear ();
                        //selectedTecViews.Clear ();
                    }
                    else {
                    }

                    if (tecViews.Count == 0) {
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
                        foreach (TEC t in formChangeMode.tec)
                        {
                            tecView = new TecView(t, tec_indx, -1, m_admin, stsStrip, formGraphicsSettings, formParameters);
                            tecView.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);
                            tecViews.Add(tecView);
                            if (t.list_TECComponents.Count > 0)
                            {
                                comp_indx = 0;
                                foreach (TECComponent g in t.list_TECComponents)
                                {
                                    tecView = new TecView(t, tec_indx, comp_indx, m_admin, stsStrip, formGraphicsSettings, formParameters);
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
                    tclTecViews.TabPages.Clear();
                    selectedTecViews.Clear();

                    Int16 parametrsTGBiysk = 0;
                    int tecView_index = -1;
                    //List<int> list_tecView_index_visible = new List<int>();
                    List <int> list_tecView_index_checked = new List <int> ();
                    // отображаем вкладки ТЭЦ
                    for (i = 0; i < formChangeMode.tec_index.Count; i++) //или TECComponent_index.Count
                    {
                        if (!(formChangeMode.was_checked.IndexOf(i) < 0))
                        {
                            int tec_index = formChangeMode.tec_index [i],
                                TECComponent_index = formChangeMode.TECComponent_index[i];

                            for (tecView_index = 0; tecView_index < tecViews.Count; tecView_index ++) {
                                if ((tecViews [tecView_index].num_TEC == tec_index) && (tecViews [tecView_index].num_TECComponent == TECComponent_index))
                                    break;
                                else
                                    ;
                            }

                            if ((tecView_index < tecViews.Count)) {
                                list_tecView_index_checked.Add(tecView_index);

                                if ((tecViews[tecView_index].tec.type() == TEC.TEC_TYPE.BIYSK)/* && (параметрыТГБийскToolStripMenuItem.Visible == false)*/)
                                    parametrsTGBiysk++;
                                else
                                    ;

                                if (TECComponent_index == -1)
                                {
                                    tclTecViews.TabPages.Add(m_admin.m_list_tec[tec_index].name);
                                }
                                else
                                    tclTecViews.TabPages.Add(m_admin.m_list_tec[tec_index].name + " - " + m_admin.m_list_tec[tec_index].list_TECComponents[TECComponent_index].name_shr);

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

                    for (tecView_index = 0; tecView_index < tecViews.Count; tecView_index ++) {
                        if (list_tecView_index_checked.IndexOf(tecView_index) < 0)
                            tecViews[tecView_index].Stop();
                        else
                            ;
                    }

                    параметрыТГБийскToolStripMenuItem.Visible = (parametrsTGBiysk > 0 ? true : false);

                    StopWait();
                    if (formChangeMode.admin_was_checked)
                    {
                        if (formChangeMode.IsModeTECComponent (FormChangeMode.MODE_TECCOMPONENT.GTP) == true) {
                            formPassword.SetIdPass(FormPassword.ID_ROLES.COM_DISP);
                        }
                        else
                            formPassword.SetIdPass(FormPassword.ID_ROLES.NSS);

                        bool bAdminPanelUse = false;
                        if (prevStateIsAdmin == false)
                            switch (formPassword.ShowDialog(this)) {
                                case DialogResult.Yes:
                                    bAdminPanelUse = true;
                                    break;
                                case DialogResult.Retry:
                                    formSetPassword.SetIdPass (formPassword.GetIdPass ());
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

                        if (bAdminPanelUse)
                        {
                            StartWait();
                            AddTabPageAdmin ();
                            StopWait();
                        }
                        else
                            formChangeMode.admin_was_checked = false;
                    }
                    else
                        ;

                    prevStateIsAdmin = formChangeMode.admin_was_checked;

                    if (selectedTecViews.Count > 0)
                    {
                        m_prevSelectedIndex = 0;
                        selectedTecViews[m_prevSelectedIndex].Activate(true);
                        m_panelAdmin.Activate(false);
                    }
                    else
                        if (formChangeMode.admin_was_checked)
                            m_panelAdmin.Activate(true);
                        else
                            ;
                }
                else
                    ; //Отмена выбора закладок
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

            if (tclTecViews.SelectedIndex >= 0 && tclTecViews.SelectedIndex < selectedTecViews.Count && m_prevSelectedIndex >= 0 && m_prevSelectedIndex < selectedTecViews.Count)
            {
                selectedTecViews[m_prevSelectedIndex].Activate(false);
                selectedTecViews[tclTecViews.SelectedIndex].Activate(true);
                m_prevSelectedIndex = tclTecViews.SelectedIndex;
                m_panelAdmin.Activate(false);
            }
            else
            {
                if (m_prevSelectedIndex >= 0 && m_prevSelectedIndex < selectedTecViews.Count)
                    selectedTecViews[m_prevSelectedIndex].Activate(false);
                else
                    ;

                if (tclTecViews.SelectedIndex == selectedTecViews.Count)
                    m_panelAdmin.Activate(true);
                else
                    ;
            }

            //tclTecViews.Invalidate ();
            //tclTecViews.Refresh ();
            //this.Invalidate ();
            //this.Refresh ();

            //m_panelAdmin.Invalidate();
            //m_panelAdmin.Refresh();
        }

        public bool UpdateStatusString()
        {
            bool have_eror = false;
            lblDescError.Text = lblDateError.Text = "";
            for (int i = 0; i < selectedTecViews.Count; i++)
            {
                if ((selectedTecViews[i].actioned_state == true) && ((selectedTecViews[i].tec.connSetts [(int) CONN_SETT_TYPE.DATA_FACT].ignore == false) &&
                                                                    (selectedTecViews[i].tec.connSetts [(int) CONN_SETT_TYPE.DATA_TM].ignore == false)))
                {
                    if (selectedTecViews[i].isActive == true)
                    {
                        lblDateError.Text = selectedTecViews[i].last_time_action.ToString();
                        lblDescError.Text = selectedTecViews[i].last_action;
                    }
                }
                else
                    ;

                if ((selectedTecViews[i].errored_state == true) && ((selectedTecViews[i].tec.connSetts [(int) CONN_SETT_TYPE.DATA_FACT].ignore == false) &&
                                                                    (selectedTecViews[i].tec.connSetts [(int) CONN_SETT_TYPE.DATA_TM].ignore == false)))
                {
                    have_eror = true;
                    if (selectedTecViews[i].isActive == true)
                    {
                        lblDateError.Text = selectedTecViews[i].last_time_error.ToString();
                        lblDescError.Text = selectedTecViews[i].last_error;
                    }
                }
                else
                    ;
            }

            if ((m_admin.actioned_state = true) && (m_panelAdmin.isActive == true))
            {
                lblDateError.Text = m_admin.last_time_action.ToString();
                lblDescError.Text = m_admin.last_action;
            }
            else
                ;

            if (m_admin.errored_state == true)
            {
                have_eror = true;
                lblDateError.Text = m_admin.last_time_error.ToString();
                lblDescError.Text = m_admin.last_error;
            }
            else
                ;

            if (m_passwords.actioned_state == true)
            {
                lblDateError.Text = m_passwords.last_time_action.ToString();
                lblDescError.Text = m_passwords.last_action;
            }
            else
                ;

            if (m_passwords.errored_state == true)
            {
                have_eror = true;
                lblDateError.Text = m_passwords.last_time_error.ToString();
                lblDescError.Text = m_passwords.last_error;
            }
            else
                ;

            return have_eror;
        }

        protected override void EventRaised()
        {
            lock (lockEvent)
            {
                UpdateStatusString();
                lblDescError.Invalidate();
                lblDateError.Invalidate();
            }
        }

        private void AddTabPageAdmin () {
            StatisticCommon.FormChangeMode.MODE_TECCOMPONENT mode = FormChangeMode.MODE_TECCOMPONENT.TEC; //FormChangeMode.MODE_TECCOMPONENT.TG;
            //StatisticCommon.FormChangeMode.MANAGER modeAdmin = FormChangeMode.MANAGER.NSS;

            if (formChangeMode.IsModeTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP) == true)
            {
                tclTecViews.TabPages.Add(formChangeMode.getNameAdminValues(FormChangeMode.MODE_TECCOMPONENT.GTP));
                mode = FormChangeMode.MODE_TECCOMPONENT.GTP;
                //modeAdmin = FormChangeMode.MANAGER.DISP;
            }
            else
                tclTecViews.TabPages.Add(formChangeMode.getNameAdminValues(FormChangeMode.MODE_TECCOMPONENT.TEC)); //PC или TG не важно

            tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(m_panelAdmin);

            m_panelAdmin.InitializeComboBoxTecComponent(mode);

            m_admin.Resume();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (timer.Interval == 666)
            {
                int i = -1;
                //for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++) {
                    m_admin.StartThreadSourceData();
                //}

                // отображаем вкладки ТЭЦ
                int index;
                for (i = 0; i < formChangeMode.tec_index.Count; i++)
                {
                    TEC t = m_admin.m_list_tec[formChangeMode.tec_index[i]];

                    if ((index = formChangeMode.was_checked.IndexOf(i)) >= 0)
                    {
                        if (formChangeMode.TECComponent_index[formChangeMode.was_checked[index]] == -1)
                            tclTecViews.TabPages.Add(t.name);
                        else
                            tclTecViews.TabPages.Add(t.name + " - " + t.list_TECComponents[formChangeMode.TECComponent_index[formChangeMode.was_checked[index]]].name_shr);

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
                    m_prevSelectedIndex = 0;
                    selectedTecViews[m_prevSelectedIndex].Activate(true);
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
                    lblMainState.Text = "ОШИБКА";
                else
                    ;

                if ((have_eror == false) || (show_error_alert == false))
                    lblMainState.Text = "";
                else
                    ;

                show_error_alert = !show_error_alert;
                lblDescError.Invalidate();
                lblDateError.Invalidate();
            }
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout a = new FormAbout();
            a.ShowDialog(this);
        }

        private void панельГрафическихToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (m_formConnectionSettings.Protected == true)
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
            if (tclTecViews.SelectedIndex >= 0 && tclTecViews.SelectedIndex < selectedTecViews.Count)
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
            if (m_formConnectionSettings.Protected == true)
                formParameters.ShowDialog(this);
            else
                ;
        }

        private void параметрыТГБийскToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_formConnectionSettings.Protected == true) {
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

        private void изменитьПарольКоммерческогоДиспетчераToolStripMenuItem_Click(object sender, EventArgs e)
        {
            изменитьПарольToolStripMenuItem_Click(sender, e, FormPassword.ID_ROLES.COM_DISP);
        }

        private void изменитьПарольАдминистратораToolStripMenuItem_Click(object sender, EventArgs e)
        {
            изменитьПарольToolStripMenuItem_Click(sender, e, FormPassword.ID_ROLES.ADMIN);
        }

        private void изменитьПарольToolStripMenuItem_Click(object sender, EventArgs e, FormPassword.ID_ROLES id)
        {
            if (m_formConnectionSettings.Protected == true)
            {
                formPassword.SetIdPass(id);
                DialogResult dlgRes = formPassword.ShowDialog(this);
                if (dlgRes == DialogResult.Yes)
                {
                    formSetPassword.SetIdPass(formPassword.GetIdPass());
                    formSetPassword.ShowDialog(this);
                }
                else
                    if (dlgRes == DialogResult.Abort)
                        connectionSettings ();
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
