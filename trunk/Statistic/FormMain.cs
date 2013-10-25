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

namespace Statistic
{
    public delegate void DelegateFunc();
    public delegate void DelegateIntFunc(int param);
    public delegate void DelegateIntIntFunc(int param1, int param2);
    public delegate void DelegateStringFunc(string param);
    public delegate void DelegateBoolFunc(bool param);

    public partial class FormMain : Form
    {
        public List<TEC> tec;
        private FormWait formWait;
        private FormConnectionSettings formConnSett;
        private PanelAdmin m_panelAdmin;
        private List<TecView> tecViews;
        private List<TecView> selectedTecViews;
        private object lockValue;
        private int waitCounter;
        private FormPassword formPassword;
        private FormSetPassword formSetPassword;
        private FormChangeMode formChangeMode;
        private DelegateFunc delegateStartWait;
        private DelegateFunc delegateStopWait;
        private DelegateFunc delegateStopWaitForm;
        private DelegateFunc delegateEvent;
        private DelegateFunc delegateUpdateActiveGui;
        private DelegateFunc delegateHideGraphicsSettings;
        private DelegateFunc delegateParamsApply;
        private TecView tecView;
        private int oldSelectedIndex;
        private bool prevStateIsAdmin;
        private bool prevStateIsPPBR;
        private Thread tt;
        public static object lockFile = new object();
        public static string logPath;
        public GraphicsSettings graphicsSettingsForm;
        public FormParameters parametersForm;
        //public FormParametersTG parametersTGForm;
        public static Logging log;

        private bool show_error_alert = false;

        private bool firstStart;

        private object lockEvent;

        public static int MAX_RETRY = 2;
        public static int MAX_WAIT_COUNT = 25;
        public static int WAIT_TIME_MS = 100;

        public FormMain()
        {
            InitializeComponent();

            lockEvent = new object();

            logPath = System.Environment.CurrentDirectory;
            log = new Logging(System.Environment.CurrentDirectory + @"\" + Environment.MachineName + "_log.txt", false, null, null);

            delegateStartWait = new DelegateFunc(StartWait);
            delegateStopWait = new DelegateFunc(StopWait);

            formWait = new FormWait();
            delegateStopWaitForm = new DelegateFunc(formWait.StopWaitForm);
            delegateEvent = new DelegateFunc(EventRaised);
            delegateUpdateActiveGui = new DelegateFunc(UpdateActiveGui);
            delegateHideGraphicsSettings = new DelegateFunc(HideGraphicsSettings);

            formConnSett = new FormConnectionSettings();
            if (formConnSett.Protected == false)
            {
            }
            else
            {
                formChangeMode = new FormChangeMode(formConnSett.connectionSettings[formConnSett.connectionSettings.Count - 1]);
                Initialize();
            }
        }

        private bool Initialize()
        {
            firstStart = true;

            this.tec = formChangeMode.tec;
            oldSelectedIndex = 0;

            m_panelAdmin = new PanelAdmin(tec, stsStrip);
            m_panelAdmin.connSettConfigDB = formConnSett.getConnSett();

            m_panelAdmin.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);

            //formChangeMode = new FormChangeMode();
            formPassword = new FormPassword(m_panelAdmin);
            formSetPassword = new FormSetPassword(m_panelAdmin);
            graphicsSettingsForm = new GraphicsSettings(this, delegateUpdateActiveGui, delegateHideGraphicsSettings);
            parametersForm = new FormParameters();

            tecViews = new List<TecView>();
            selectedTecViews = new List<TecView>();
            lockValue = new object();
            waitCounter = 0;

            prevStateIsAdmin = false;
            prevStateIsPPBR = false;

            timer.Start();

            return true;
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
                if ((! (formChangeMode == null)) && formChangeMode.admin_was_checked)
                {
                    if (!m_panelAdmin.MayToClose())
                        e.Cancel = true;
                    else
                        ;
                }
                else
                    ;

                timer.Stop();

                if ((e.Cancel == false) && (! (tec == null)))
                {
                    foreach (TEC t in tec)
                        t.StopDbInterfaceForce();
                    m_panelAdmin.StopDbInterface();
                }
                else
                    ;
            }
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

            formPassword.SetIdPass(2);
            if (!formConnSett.Protected || formPassword.ShowDialog() == DialogResult.Yes)
            {
                DialogResult result;
                result = formConnSett.ShowDialog();
                if (result == DialogResult.Yes)
                {
                    if (! (tecViews == null)) tecViews.Clear (); else ;

                    if (timer.Enabled) timer.Stop(); else ;
                    if (! (m_panelAdmin == null)) m_panelAdmin.StopDbInterface(); else ;

                    formChangeMode.InitTEC (formConnSett.connectionSettings[formConnSett.connectionSettings.Count - 1]);
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
            else
                ;
        }

        public void StartWait()
        {
            lock (lockValue)
            {
                if (waitCounter == 0)
                {
//                    this.Opacity = 0.75;
                    if (tt != null && tt.IsAlive)
                        tt.Join();
                    tt = new Thread(new ParameterizedThreadStart(ThreadProc));
                    formWait.Location = new Point(this.Location.X + (this.Width - formWait.Width) / 2, this.Location.Y + (this.Height - formWait.Height) / 2);
                    tt.IsBackground = true;
                    tt.Start(formWait);
                }
                else
                    ;

                waitCounter++;
            }
        }

        public static void ThreadProc(object data)
        {
            FormWait fw = (FormWait)data;
            fw.StartWaitForm();
        }

        public void StopWait()
        {
            lock (lockValue)
            {
                waitCounter--;
                if (waitCounter < 0)
                    waitCounter = 0;

                if (waitCounter == 0)
                {
//                    this.Opacity = 1.0;
                    while(!formWait.IsHandleCreated);
                    formWait.Invoke(delegateStopWaitForm);
                }
                else
                    ;
            }
        }

        private void сменитьРежимToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (formConnSett.Protected == true)
            {
                int i;
                int index;
                int prevModeComponent = formChangeMode.getModeTECComponent ();
                // выбираем список отображаемых вкладок
                if (formChangeMode.ShowDialog() == DialogResult.OK)
                {
                    if ((! (prevModeComponent == formChangeMode.getModeTECComponent()))) {
                        this.tec = formChangeMode.tec;

                        prevStateIsAdmin = false;

                        tecViews.Clear ();
                        selectedTecViews.Clear ();
                    }
                    else {
                        
                    }

                    if (tecViews.Count == 0) {
                        m_panelAdmin.StopDbInterface ();
                        m_panelAdmin.Stop();

                        m_panelAdmin.InitTEC (formChangeMode.tec);
                        m_panelAdmin.mode(formChangeMode.getModeTECComponent ());
                        m_panelAdmin.StartDbInterface ();

                        // создаём все tecview
                        foreach (TEC t in formChangeMode.tec)
                        {
                            int index_gtp;
                            tecView = new TecView(t, -1, m_panelAdmin, stsStrip, graphicsSettingsForm, parametersForm);
                            tecView.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);
                            tecViews.Add(tecView);
                            if (t.list_TECComponents.Count > 0)
                            {
                                index_gtp = 0;
                                foreach (TECComponent g in t.list_TECComponents)
                                {
                                    tecView = new TecView(t, index_gtp, m_panelAdmin, stsStrip, graphicsSettingsForm, parametersForm);
                                    tecView.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);
                                    tecViews.Add(tecView);
                                    index_gtp++;
                                }
                            }
                        }
                    }
                    else
                        ;

                    StartWait();
                    tclTecViews.TabPages.Clear();
                    selectedTecViews.Clear();

                    Int16 parametrsTGBiysk = 0;
                    // отображаем вкладки ТЭЦ
                    for (i = 0; i < formChangeMode.tec_index.Count; i++)
                    {
                        index = formChangeMode.was_checked.IndexOf(i);

                        if (!(index < 0))
                        {
                            if ((tecViews[i].tec.type() == TEC.TEC_TYPE.BIYSK)/* && (параметрыТГБийскToolStripMenuItem.Visible == false)*/)
                                parametrsTGBiysk++;
                            else
                                ;

                            if (formChangeMode.TECComponent_index[formChangeMode.was_checked[index]] == -1)
                            {
                                tclTecViews.TabPages.Add(tec[formChangeMode.tec_index[i]].name);
                            }
                            else
                                tclTecViews.TabPages.Add(tec[formChangeMode.tec_index[i]].name + " - " + tec[formChangeMode.tec_index[i]].list_TECComponents[formChangeMode.TECComponent_index[formChangeMode.was_checked[index]]].name);

                            tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(tecViews[i]);
                            selectedTecViews.Add(tecViews[i]);

                            tecViews[i].Activate(false);
                            tecViews[i].Start();
                        }
                        else
                        {
                            tecViews[i].Stop();
                        }
                    }

                    параметрыТГБийскToolStripMenuItem.Visible = (parametrsTGBiysk > 0 ? true : false);

                    StopWait();
                    if (formChangeMode.admin_was_checked)
                    {
                        switch (formChangeMode.getModeTECComponent ()) {
                            case (int)FormChangeMode.MODE_TECCOMPONENT.TEC:
                                break;
                            case (int)FormChangeMode.MODE_TECCOMPONENT.GTP:
                                formPassword.SetIdPass(1);
                                break;
                            case (int)FormChangeMode.MODE_TECCOMPONENT.PC:
                                formPassword.SetIdPass(3);
                                break;
                        }

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
                            tclTecViews.TabPages.Add(formChangeMode.getNameAdminValues((short) formChangeMode.getModeTECComponent ()));

                            tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(m_panelAdmin);

                            m_panelAdmin.Start();
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
            if (formConnSett.Protected == true) {
                formPassword.SetIdPass(1);
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
            if (formConnSett.Protected == true) {
                formPassword.SetIdPass(2);
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
            lblError.Text = lblDateError.Text = "";
            for (int i = 0; i < selectedTecViews.Count; i++)
            {
                if (selectedTecViews[i].actioned_state && !selectedTecViews[i].tec.connSetts [(int) CONN_SETT_TYPE.DATA].ignore)
                {
                    if (selectedTecViews[i].isActive)
                    {
                        lblDateError.Text = selectedTecViews[i].last_time_action.ToString();
                        lblError.Text = selectedTecViews[i].last_action;
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
                        lblError.Text = selectedTecViews[i].last_error;
                    }
                }
                else
                    ;
            }

            if (m_panelAdmin.actioned_state && m_panelAdmin.isActive)
            {
                lblDateError.Text = m_panelAdmin.last_time_action.ToString();
                lblError.Text = m_panelAdmin.last_action;
            }
            else
                ;

            if (m_panelAdmin.errored_state)
            {
                have_eror = true;
                lblDateError.Text = m_panelAdmin.last_time_error.ToString();
                lblError.Text = m_panelAdmin.last_error;
            }
            else
                ;

            return have_eror;
        }

        public void EventRaised()
        {
            lock (lockEvent)
            {
                UpdateStatusString();
                lblError.Invalidate();
                lblDateError.Invalidate();
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (firstStart)
            {
                m_panelAdmin.StartDbInterface();

                // отображаем вкладки ТЭЦ
                int index;
                for (int i = 0; i < formChangeMode.tec_index.Count; i++)
                {
                    TEC t = tec[formChangeMode.tec_index[i]];

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
                        tclTecViews.TabPages.Add(formChangeMode.getNameAdminValues (1));

                        tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(m_panelAdmin);

                        m_panelAdmin.Start();
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
                lblError.Invalidate();
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
            if (formConnSett.Protected == true)
            {
                if (панельГрафическихToolStripMenuItem.Checked)
                    graphicsSettingsForm.Show();
                else
                    graphicsSettingsForm.Hide();
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
                ShowWindow(graphicsSettingsForm.Handle, SW_SHOWNOACTIVATE);
                SetWindowPos(graphicsSettingsForm.Handle.ToInt32(), HWND_TOP,
                graphicsSettingsForm.Left, graphicsSettingsForm.Top, graphicsSettingsForm.Width, graphicsSettingsForm.Height,
                SWP_NOACTIVATE);
            }
            else
                ;
        }

        private void параметрыПриложенияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (formConnSett.Protected == true)
                parametersForm.ShowDialog();
            else
                ;
        }

        private void параметрыТГБийскToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (formConnSett.Protected == true) {
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
            formPassword.SetIdPass(2);
            if (formPassword.ShowDialog() == DialogResult.Yes)
            {
                FormTECComponent tecComponent = new FormTECComponent(formConnSett.connectionSettings[formConnSett.connectionSettings.Count - 1]);
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
