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

            bool bShowFormConnectionSettings = true;
            m_formConnectionSettings = new FormConnectionSettings("connsett.ini");
            if (m_formConnectionSettings.Protected == true)
            {
                bShowFormConnectionSettings = ! Initialize();
                if (bShowFormConnectionSettings == true)
                {
                    //throw new Exception("Ошибка инициализации пользовательских компонентов формы.");
                    //m_formConnectionSettings.ShowDialog(this);
                }
                else
                    ;
            }
            else
            {
                //m_formConnectionSettings.ShowDialog(this);
            }

            if (bShowFormConnectionSettings == true)
            {
                connectionSettings ();
            }
            else
                ;
        }

        private bool Initialize()
        {
            bool bRes = true;
            
            timer.Interval = 666; //Признак первого старта

            m_passwords = new Passwords();
            m_passwords.SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
            m_passwords.SetDelegateReport(ErrorReport, ActionReport);
            m_passwords.connSettConfigDB = m_formConnectionSettings.getConnSett();

            Users user = new Users(m_formConnectionSettings.getConnSett());
            if (! (user.Role == 2)) //Администратор
                администрированиеToolStripMenuItem.Enabled = false;
            else;

            m_arAdmin = new AdminTS[(int)FormChangeMode.MANAGER.COUNT_MANAGER];
            m_arPanelAdmin = new PanelAdmin[(int)FormChangeMode.MANAGER.COUNT_MANAGER];            

            int i = -1;
            for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++) {
                switch (i) {
                    case (int)FormChangeMode.MANAGER.DISP:
                        m_arAdmin[i] = new AdminTS();
                        break;
                    case (int)FormChangeMode.MANAGER.NSS:
                        m_arAdmin[i] = new AdminTS_NSS();
                        break;
                    default:
                        break;
                }

                //m_admin.SetDelegateTECComponent(FillComboBoxTECComponent);
                try { m_arAdmin[i].InitTEC(m_formConnectionSettings.getConnSett(), FormChangeMode.MODE_TECCOMPONENT.UNKNOWN, false); }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, "FormMain::Initialize ()");
                    bRes = false;
                    break;
                }
                if (!(m_arAdmin[i].m_list_tec.Count > 0)) {
                    bRes = false;
                    break;
                }
                else
                    ;
                m_arAdmin[i].connSettConfigDB = m_formConnectionSettings.getConnSett();

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
            }

            formChangeMode = new FormChangeMode(m_arAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec);
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
            if ((!(formChangeMode == null)) && formChangeMode.admin_was_checked)
            //if ((!(formChangeMode == null)) && (formChangeMode.admin_was_checked[(int)FormChangeMode.MANAGER.DISP] || formChangeMode.admin_was_checked[(int)FormChangeMode.MANAGER.NSS]))
            {
                if (!m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].MayToClose())
                    if (!(e == null)) e.Cancel = true;
                else
                    ;
            }
            else
                ;

            timer.Stop();

            int i = -1;
            
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
                m_passwords.StopDbInterface();
            else
                ;
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
                if (!(m_arAdmin == null))
                    for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++) {
                        if (!(m_arAdmin [i] == null)) m_arAdmin [i].StopThreadSourceData(); else ;
                    }
                else
                    ;

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
                            tecView = new TecView(t, tec_indx, -1, m_arAdmin[(int)FormChangeMode.MANAGER.DISP], stsStrip, formGraphicsSettings, formParameters);
                            tecView.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);
                            tecViews.Add(tecView);
                            if (t.list_TECComponents.Count > 0)
                            {
                                comp_indx = 0;
                                foreach (TECComponent g in t.list_TECComponents)
                                {
                                    tecView = new TecView(t, tec_indx, comp_indx, m_arAdmin[(int)FormChangeMode.MANAGER.DISP], stsStrip, formGraphicsSettings, formParameters);
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

                    StartWait();
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
                                    tclTecViews.TabPages.Add(m_arAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec[tec_index].name);
                                }
                                else
                                    tclTecViews.TabPages.Add(m_arAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec[tec_index].name + " - " + m_arAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec[tec_index].list_TECComponents[TECComponent_index].name);

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
                        m_arPanelAdmin[(int)modePanelAdmin].Activate(false);
                    }
                    else
                        if (formChangeMode.admin_was_checked)
                            m_arPanelAdmin[(int)modePanelAdmin].Activate(true);
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
            if (tclTecViews.SelectedIndex >= 0 && tclTecViews.SelectedIndex < selectedTecViews.Count && m_prevSelectedIndex >= 0 && m_prevSelectedIndex < selectedTecViews.Count)
            {
                selectedTecViews[m_prevSelectedIndex].Activate(false);
                selectedTecViews[tclTecViews.SelectedIndex].Activate(true);
                m_prevSelectedIndex = tclTecViews.SelectedIndex;
                m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].Activate(false);
            }
            else
            {
                if (m_prevSelectedIndex >= 0 && m_prevSelectedIndex < selectedTecViews.Count)
                    selectedTecViews[m_prevSelectedIndex].Activate(false);
                else
                    ;

                if (tclTecViews.SelectedIndex == selectedTecViews.Count)
                    m_arPanelAdmin[(int)FormChangeMode.MANAGER.DISP].Activate(true);
                else
                    ;
            }

            //tclTecViews.Invalidate ();
            //this.Invalidate ();
        }

        public bool UpdateStatusString()
        {
            bool have_eror = false;
            lblDescError.Text = lblDateError.Text = "";
            for (int i = 0; i < selectedTecViews.Count; i++)
            {
                if (selectedTecViews[i].actioned_state && !selectedTecViews[i].tec.connSetts [(int) CONN_SETT_TYPE.DATA].ignore)
                {
                    if (selectedTecViews[i].isActive)
                    {
                        lblDateError.Text = selectedTecViews[i].last_time_action.ToString();
                        lblDescError.Text = selectedTecViews[i].last_action;
                    }
                }
                else
                    ;

                if (selectedTecViews[i].errored_state && !selectedTecViews[i].tec.connSetts[(int)CONN_SETT_TYPE.DATA].ignore)
                {
                    have_eror = true;
                    if (selectedTecViews[i].isActive)
                    {
                        lblDateError.Text = selectedTecViews[i].last_time_error.ToString();
                        lblDescError.Text = selectedTecViews[i].last_error;
                    }
                }
                else
                    ;
            }

            if (m_arAdmin[(int)modePanelAdmin].actioned_state && m_arPanelAdmin[(int)modePanelAdmin].isActive)
            {
                lblDateError.Text = m_arAdmin[(int)modePanelAdmin].last_time_action.ToString();
                lblDescError.Text = m_arAdmin[(int)modePanelAdmin].last_action;
            }
            else
                ;

            if (m_arAdmin[(int)modePanelAdmin].errored_state)
            {
                have_eror = true;
                lblDateError.Text = m_arAdmin[(int)modePanelAdmin].last_time_error.ToString();
                lblDescError.Text = m_arAdmin[(int)modePanelAdmin].last_error;
            }
            else
                ;

            if (m_passwords.actioned_state)
            {
                lblDateError.Text = m_passwords.last_time_action.ToString();
                lblDescError.Text = m_passwords.last_action;
            }
            else
                ;

            if (m_passwords.errored_state)
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
            StatisticCommon.FormChangeMode.MANAGER modeAdmin = FormChangeMode.MANAGER.NSS;

            if (formChangeMode.IsModeTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP) == true)
            {
                tclTecViews.TabPages.Add(formChangeMode.getNameAdminValues(FormChangeMode.MODE_TECCOMPONENT.GTP));
                mode = FormChangeMode.MODE_TECCOMPONENT.GTP;
                modeAdmin = FormChangeMode.MANAGER.DISP;
            }
            else
                tclTecViews.TabPages.Add(formChangeMode.getNameAdminValues(FormChangeMode.MODE_TECCOMPONENT.TEC)); //PC или TG не важно

            tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(m_arPanelAdmin[(int)modeAdmin]);

            m_arPanelAdmin[(int)modeAdmin].InitializeComboBoxTecComponent(mode);

            m_arAdmin[(int)modeAdmin].Resume();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (timer.Interval == 666)
            {
                m_passwords.StartDbInterface();
                
                int i = -1;
                for (i = 0; i < (int)FormChangeMode.MANAGER.COUNT_MANAGER; i ++) {
                    m_arAdmin [i].StartThreadSourceData();
                }

                // отображаем вкладки ТЭЦ
                int index;
                for (i = 0; i < formChangeMode.tec_index.Count; i++)
                {
                    TEC t = m_arAdmin[(int)FormChangeMode.MANAGER.DISP].m_list_tec[formChangeMode.tec_index[i]];

                    if ((index = formChangeMode.was_checked.IndexOf(i)) >= 0)
                    {
                        if (formChangeMode.TECComponent_index[formChangeMode.was_checked[index]] == -1)
                            tclTecViews.TabPages.Add(t.name);
                        else
                            tclTecViews.TabPages.Add(t.name + " - " + t.list_TECComponents[formChangeMode.TECComponent_index[formChangeMode.was_checked[index]]].name);

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

                if (have_eror)
                    lblMainState.Text = "ОШИБКА";
                else
                    ;

                if (!have_eror || !show_error_alert)
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

        private void изментьСоставТЭЦГТПЩУToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formPassword.SetIdPass(FormPassword.ID_ROLES.ADMIN);
            formPassword.ShowDialog(this);
            DialogResult dlgRes = formPassword.DialogResult;
            if (dlgRes == DialogResult.Yes)
            {
                FormTECComponent tecComponent = new FormTECComponent(m_formConnectionSettings.getConnSett());
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
                    connectionSettings();
                }
                else
                    ;
        }

        private void изментьСоставПользовательToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formPassword.SetIdPass(FormPassword.ID_ROLES.ADMIN);
            formPassword.ShowDialog(this);
            DialogResult dlgRes = formPassword.DialogResult;
            if (dlgRes == DialogResult.Yes)
            {
                FormUser formUser = new FormUser(m_formConnectionSettings.getConnSett());
                
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
                    connectionSettings();
                }
                else
                    ;
        }

        private void изменитьПарольНСС_Click(object sender, EventArgs e)
        {
            изменитьПарольToolStripMenuItem_Click(sender, e, FormPassword.ID_ROLES.NSS);
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
