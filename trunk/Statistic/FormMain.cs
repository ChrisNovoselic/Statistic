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
        private PanelAdmin m_panelAdmin;
        public Admin m_admin;
        private List<TecView> tecViews;
        private List<TecView> selectedTecViews;
        private FormPassword formPassword;
        private FormSetPassword formSetPassword;
        private FormChangeMode formChangeMode;
        private TecView tecView;
        private int oldSelectedIndex;
        private bool prevStateIsAdmin;
        private bool prevStateIsPPBR;
        //public static object lockFile = new object();
        //public static string logPath;
        //public static Logging log;
        public FormGraphicsSettings formGraphicsSettings;
        public FormParameters formParameters;
        //public FormParametersTG parametersTGForm;

        private bool firstStart;

        public FormMain()
        {
            InitializeComponent();

            delegateEvent = new DelegateFunc(EventRaised);
            delegateUpdateActiveGui = new DelegateFunc(UpdateActiveGui);
            delegateHideGraphicsSettings = new DelegateFunc(HideGraphicsSettings);

            m_formConnectionSettings = new FormConnectionSettings();
            if (m_formConnectionSettings.Protected == true)
            {
                Initialize();
            }
            else
            {
            }
        }

        private bool Initialize()
        {
            firstStart = true;

            m_admin = new Admin();
            //m_admin.SetDelegateTECComponent(FillComboBoxTECComponent);
            m_admin.InitTEC(m_formConnectionSettings.getConnSett(), FormChangeMode.MODE_TECCOMPONENT.UNKNOWN);
            m_admin.connSettConfigDB = m_formConnectionSettings.getConnSett();

            m_panelAdmin = new PanelAdmin(m_admin);

            m_admin.SetDelegateData(m_panelAdmin.setDataGridViewAdmin);
            m_admin.SetDelegateDatetime(m_panelAdmin.CalendarSetDate);

            m_admin.SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
            m_admin.SetDelegateReport(ErrorReport, ActionReport);

            formChangeMode = new FormChangeMode(m_admin.m_list_tec);
            oldSelectedIndex = 0;

            //formChangeMode = new FormChangeMode();
            formPassword = new FormPassword(m_admin);
            formSetPassword = new FormSetPassword(m_admin);
            formGraphicsSettings = new FormGraphicsSettings(this, delegateUpdateActiveGui, delegateHideGraphicsSettings);
            formParameters = new FormParameters();

            tecViews = new List<TecView>();
            selectedTecViews = new List<TecView>();

            prevStateIsAdmin = false;
            prevStateIsPPBR = false;

            timer.Start();

            return true;
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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show(this, "Вы уверены, что хотите закрыть приложение?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
            }
            else
            {
                if ((!(formChangeMode == null)) && formChangeMode.admin_was_checked)
                //if ((!(formChangeMode == null)) && (formChangeMode.admin_was_checked[(int)FormChangeMode.MANAGER.DISP] || formChangeMode.admin_was_checked[(int)FormChangeMode.MANAGER.NSS]))
                {
                    if (!m_panelAdmin.MayToClose())
                        e.Cancel = true;
                    else
                        ;
                }
                else
                    ;

                timer.Stop();

                if ((e.Cancel == false) && (! (m_admin.m_list_tec == null)))
                {
                    foreach (TEC t in m_admin.m_list_tec)
                        t.StopDbInterfaceForce();
                    m_admin.StopDbInterface();
                }
                else
                    ;
            }
        }

        private void connectionSettings () {
            DialogResult result;
            result = m_formConnectionSettings.ShowDialog();
            if (result == DialogResult.Yes)
            {
                if (! (tecViews == null)) tecViews.Clear (); else ;

                if (timer.Enabled) timer.Stop(); else ;
                if (! (m_admin == null)) m_admin.StopDbInterface(); else ;

                Initialize();

                //foreach (TecView t in tecViews)
                //{
                //    t.Reinit();
                //}

                //m_panelAdmin.Reinit();
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

            if (formPassword == null)
            {
                connectionSettings ();
            }
            else {
                formPassword.SetIdPass(FormPassword.ID_ROLES.ADMIN);
                if ((m_formConnectionSettings.Protected == false) || formPassword.ShowDialog() == DialogResult.Yes)
                {
                    connectionSettings ();
                }
                else
                    ;
            }
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
                        m_panelAdmin.StopDbInterface ();
                        m_panelAdmin.Stop();

                        //m_panelAdmin.InitTEC (formChangeMode.tec);
                        //m_panelAdmin.mode(formChangeMode.getModeTECComponent ());
                        m_panelAdmin.StartDbInterface ();
                        */
                        // создаём все tecview
                        int index_tec = 0;
                        foreach (TEC t in formChangeMode.tec)
                        {
                            int index_gtp;
                            tecView = new TecView(t, index_tec, -1, m_admin, stsStrip, formGraphicsSettings, formParameters);
                            tecView.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);
                            tecViews.Add(tecView);
                            if (t.list_TECComponents.Count > 0)
                            {
                                index_gtp = 0;
                                foreach (TECComponent g in t.list_TECComponents)
                                {
                                    tecView = new TecView(t, index_tec, index_gtp, m_admin, stsStrip, formGraphicsSettings, formParameters);
                                    tecView.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);
                                    tecViews.Add(tecView);
                                    index_gtp++;
                                }
                            }

                            index_tec ++;
                        }
                    }
                    else
                        ;

                    StartWait();
                    tclTecViews.TabPages.Clear();
                    selectedTecViews.Clear();

                    Int16 parametrsTGBiysk = 0;
                    int tecView_index = -1;
                    List <int> list_tecView_index_start = new List <int> ();
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
                                list_tecView_index_start.Add(tecView_index);

                                if ((tecViews[tecView_index].tec.type() == TEC.TEC_TYPE.BIYSK)/* && (параметрыТГБийскToolStripMenuItem.Visible == false)*/)
                                    parametrsTGBiysk++;
                                else
                                    ;

                                if (TECComponent_index == -1)
                                {
                                    tclTecViews.TabPages.Add(m_admin.m_list_tec[tec_index].name);
                                }
                                else
                                    tclTecViews.TabPages.Add(m_admin.m_list_tec[tec_index].name + " - " + m_admin.m_list_tec[tec_index].list_TECComponents[TECComponent_index].name);

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
                        if (list_tecView_index_start.IndexOf(tecView_index) < 0)
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
                            switch (formPassword.ShowDialog()) {
                                case DialogResult.Yes:
                                    bAdminPanelUse = true;
                                    break;
                                case DialogResult.Retry:
                                    formSetPassword.SetIdPass (formPassword.GetIdPass ());
                                    if (formSetPassword.ShowDialog() == DialogResult.Yes)
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
                            if (formChangeMode.IsModeTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP) == true)
                            {
                                tclTecViews.TabPages.Add(formChangeMode.getNameAdminValues(FormChangeMode.MODE_TECCOMPONENT.GTP));
                            }
                            else
                                tclTecViews.TabPages.Add(formChangeMode.getNameAdminValues(FormChangeMode.MODE_TECCOMPONENT.TEC)); //PC или TG не важно

                            tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(m_panelAdmin);

                            m_admin.Start();
                            StopWait();
                        }
                        else
                            formChangeMode.admin_was_checked = false;
                    }

                    prevStateIsAdmin = formChangeMode.admin_was_checked;

                    if (selectedTecViews.Count > 0)
                    {
                        oldSelectedIndex = 0;
                        selectedTecViews[oldSelectedIndex].Activate(true);
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

        private void изменитьПарольКоммерческогоДиспетчераToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_formConnectionSettings.Protected == true) {
                formPassword.SetIdPass(FormPassword.ID_ROLES.COM_DISP);
                if (formPassword.ShowDialog() == DialogResult.Yes) {
                    formSetPassword.SetIdPass(formPassword.GetIdPass ());
                    formSetPassword.ShowDialog();
                }
                else
                    ;
            }
            else
                ;
        }

        private void изменитьПарольАдминистратораToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_formConnectionSettings.Protected == true) {
                formPassword.SetIdPass(FormPassword.ID_ROLES.ADMIN);
                if (formPassword.ShowDialog() == DialogResult.Yes)
                {
                    formSetPassword.SetIdPass(formPassword.GetIdPass());
                    formSetPassword.ShowDialog();
                }
                else
                    ;
            }
            else
                ;
        }

        private void tclTecViews_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tclTecViews.SelectedIndex >= 0 && tclTecViews.SelectedIndex < selectedTecViews.Count && oldSelectedIndex >= 0 && oldSelectedIndex < selectedTecViews.Count)
            {
                selectedTecViews[oldSelectedIndex].Activate(false);
                selectedTecViews[tclTecViews.SelectedIndex].Activate(true);
                oldSelectedIndex = tclTecViews.SelectedIndex;
                m_panelAdmin.Activate(false);
            }
            else
            {
                if (oldSelectedIndex >= 0 && oldSelectedIndex < selectedTecViews.Count)
                    selectedTecViews[oldSelectedIndex].Activate(false);

                if (tclTecViews.SelectedIndex == selectedTecViews.Count)
                    m_panelAdmin.Activate(true);
                else
                    ;
            }
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

            if (m_admin.actioned_state && m_panelAdmin.isActive)
            {
                lblDateError.Text = m_admin.last_time_action.ToString();
                lblDescError.Text = m_admin.last_action;
            }
            else
                ;

            if (m_admin.errored_state)
            {
                have_eror = true;
                lblDateError.Text = m_admin.last_time_error.ToString();
                lblDescError.Text = m_admin.last_error;
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

        private void timer_Tick(object sender, EventArgs e)
        {
            if (firstStart)
            {
                m_admin.StartDbInterface();

                // отображаем вкладки ТЭЦ
                int index;
                for (int i = 0; i < formChangeMode.tec_index.Count; i++)
                {
                    TEC t = m_admin.m_list_tec[formChangeMode.tec_index[i]];

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
                    oldSelectedIndex = 0;
                    selectedTecViews[oldSelectedIndex].Activate(true);
                }
                else
                    ;

                if (formChangeMode.admin_was_checked)
                {
                    //if (formPassword.ShowDialog() == DialogResult.Yes)
                    {
                        if (formChangeMode.IsModeTECComponent(FormChangeMode.MODE_TECCOMPONENT.GTP) == true)
                        {
                            tclTecViews.TabPages.Add(formChangeMode.getNameAdminValues(FormChangeMode.MODE_TECCOMPONENT.GTP));
                        }
                        else
                            tclTecViews.TabPages.Add(formChangeMode.getNameAdminValues(FormChangeMode.MODE_TECCOMPONENT.TEC)); //PC или TG не важно
                        

                        tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(m_panelAdmin);

                        m_admin.Start();
                    }
                }
                else
                    ;

                firstStart = false;
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
            a.ShowDialog();
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
                formParameters.ShowDialog();
            else
                ;
        }

        private void параметрыТГБийскToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_formConnectionSettings.Protected == true) {
                foreach (TecView tv in tecViews) {
                    if (tv.tec.type () == TEC.TEC_TYPE.BIYSK) {
                        tv.tec.parametersTGForm.ShowDialog();
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
            if (formPassword.ShowDialog() == DialogResult.Yes)
            {
                FormTECComponent tecComponent = new FormTECComponent(m_formConnectionSettings.getConnSett());
                if (tecComponent.ShowDialog () == DialogResult.OK) {
                }
                else
                    ;
            }
            else
                ;
        }
    }
}
