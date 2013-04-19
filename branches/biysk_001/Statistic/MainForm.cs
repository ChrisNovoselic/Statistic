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

    public partial class MainForm : Form
    {
        public List<TEC> tec;
        private WaitForm waitForm;
        private ConnectionSettingsView connSettForm;
        private Admin adminPanel;
        private List<TecView> tecViews;
        private List<TecView> selectedTecViews;
        private object lockValue;
        private int waitCounter;
        private Password passwordForm;
        private SetPassword setPasswordForm;
        private PasswordSettings passwordSettingsForm;
        private SetPasswordSettings setPasswordSettingsForm;
        private ChangeMode changeMode;
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
        public Parameters parametersForm;
        public static Logging log;

        private bool show_error_alert = false;

        private bool firstStart;

        private object lockEvent;

        public static int MAX_RETRY = 3;
        public static int MAX_WAIT_COUNT = 25;
        public static int WAIT_TIME_MS = 100;

        public MainForm(List<TEC> tec)
        {
            this.tec = tec;
            InitializeComponent();
            oldSelectedIndex = 0;

            lockEvent = new object();

            logPath = System.Environment.CurrentDirectory;
            log = new Logging(System.Environment.CurrentDirectory + @"\" + Environment.MachineName + "_log.txt", false, null, null);

            firstStart = true;

            delegateStartWait = new DelegateFunc(StartWait);
            delegateStopWait = new DelegateFunc(StopWait);

            waitForm = new WaitForm();
            delegateStopWaitForm = new DelegateFunc(waitForm.StopWaitForm);
            delegateEvent = new DelegateFunc(EventRaised);
            delegateUpdateActiveGui = new DelegateFunc(UpdateActiveGui);
            delegateHideGraphicsSettings = new DelegateFunc(HideGraphicsSettings);

            adminPanel = new Admin(tec, stsStrip);
            changeMode = new ChangeMode(tec);
            passwordForm = new Password(adminPanel);
            setPasswordForm = new SetPassword(adminPanel);
            passwordSettingsForm = new PasswordSettings(adminPanel);
            setPasswordSettingsForm = new SetPasswordSettings(adminPanel);
            graphicsSettingsForm = new GraphicsSettings(this, delegateUpdateActiveGui, delegateHideGraphicsSettings);
            parametersForm = new Parameters(delegateParamsApply);

            adminPanel.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);
            connSettForm = new ConnectionSettingsView(tec, adminPanel);

            tecViews = new List<TecView>();
            selectedTecViews = new List<TecView>();
            lockValue = new object();
            waitCounter = 0;

            prevStateIsAdmin = false;
            prevStateIsPPBR = false;

            // создаём все tecview
            foreach (TEC t in tec)
            {
                int index_gtp;
                tecView = new TecView(t, -1, adminPanel, stsStrip, graphicsSettingsForm, parametersForm);
                tecView.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);
                tecViews.Add(tecView);
                if (t.GTP.Count > 1)
                {
                    index_gtp = 0;
                    foreach (GTP g in t.GTP)
                    {
                        tecView = new TecView(t, index_gtp, adminPanel, stsStrip, graphicsSettingsForm, parametersForm);
                        tecView.SetDelegate(delegateStartWait, delegateStopWait, delegateEvent);
                        tecViews.Add(tecView);
                        index_gtp++;
                    }
                }
            }

            //Form1 f = new Form1();
            //f.Show();

            timer.Start();
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
                if (changeMode.admin_was_checked)
                {
                    if (!adminPanel.MayToClose())
                        e.Cancel = true;
                }

                timer.Stop();

                if (!e.Cancel)
                {
                    foreach (TEC t in tec)
                        t.StopDbInterfaceForce();
                    adminPanel.StopDbInterface();
                }
            }
        }

        private void настройкиСоединенияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //???
            //string strPassword = "password";
            //MD5CryptoServiceProvider md5;
            //md5 = new MD5CryptoServiceProvider();
            //StringBuilder strPasswordHashed = new StringBuilder ();
            //byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(strPassword));
            
            if (!connSettForm.Protected || passwordSettingsForm.ShowDialog() == DialogResult.Yes)
            {
                DialogResult result;
                result = connSettForm.ShowDialog();

                if (result == DialogResult.Yes)
                {
                    foreach (TecView t in tecViews)
                    {
                        t.Reinit();
                    }

                    adminPanel.Reinit();
                }
            }
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
                    waitForm.Location = new Point(this.Location.X + (this.Width - waitForm.Width) / 2, this.Location.Y + (this.Height - waitForm.Height) / 2);
                    tt.IsBackground = true;
                    tt.Start(waitForm);
                }
                waitCounter++;
            }
        }

        public static void ThreadProc(object data)
        {
            WaitForm wf = (WaitForm)data;
            wf.StartWaitForm();
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
                    while(!waitForm.IsHandleCreated);
                    waitForm.Invoke(delegateStopWaitForm);
                }
            }
        }

        private void сменитьРежимToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i;
            TEC t;
            int index;
            // выбираем список отображаемых вкладок
            if (changeMode.ShowDialog() == DialogResult.OK)
            {
                StartWait();
                tclTecViews.TabPages.Clear();
                selectedTecViews.Clear();

                // отображаем вкладки ТЭЦ
                for (i = 0; i < changeMode.tec_index.Count; i++)
                {
                    if ((index = changeMode.was_checked.IndexOf(i)) >= 0)
                    {
                        t = tec[changeMode.tec_index[i]];

                        if (changeMode.gtp_index[changeMode.was_checked[index]] == -1) {
                            tclTecViews.TabPages.Add(t.name);
                        }
                        else
                            tclTecViews.TabPages.Add(t.name + " - " + t.GTP[changeMode.gtp_index[changeMode.was_checked[index]]].name);

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

                StopWait();
                if (changeMode.admin_was_checked)
                {
                    if (prevStateIsAdmin || passwordForm.ShowDialog() == DialogResult.Yes)
                    {
                        StartWait();
                        tclTecViews.TabPages.Add("Редактирование ПБР");

                        tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(adminPanel);

                        adminPanel.Start();
                        StopWait();
                    }
                    else
                        changeMode.admin_was_checked = false;
                }

                prevStateIsAdmin = changeMode.admin_was_checked;

                if (selectedTecViews.Count > 0)
                {
                    oldSelectedIndex = 0;
                    selectedTecViews[oldSelectedIndex].Activate(true);
                    adminPanel.Activate(false);
                }
                else
                    if (changeMode.admin_was_checked)
                        adminPanel.Activate(true);
            }
        }

        private void изменитьПарольКоммерческогоДиспетчераToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (passwordForm.ShowDialog() == DialogResult.Yes)
            {
                setPasswordForm.ShowDialog();
            }
        }

        private void изменитьПарольАдминистратораToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (passwordSettingsForm.ShowDialog() == DialogResult.Yes)
            {
                setPasswordSettingsForm.ShowDialog();
            }
        }

        private void tclTecViews_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tclTecViews.SelectedIndex >= 0 && tclTecViews.SelectedIndex < selectedTecViews.Count && oldSelectedIndex >= 0 && oldSelectedIndex < selectedTecViews.Count)
            {
                selectedTecViews[oldSelectedIndex].Activate(false);
                selectedTecViews[tclTecViews.SelectedIndex].Activate(true);
                oldSelectedIndex = tclTecViews.SelectedIndex;
                adminPanel.Activate(false);
            }
            else
            {
                if (oldSelectedIndex >= 0 && oldSelectedIndex < selectedTecViews.Count)
                    selectedTecViews[oldSelectedIndex].Activate(false);

                if (tclTecViews.SelectedIndex == selectedTecViews.Count)
                    adminPanel.Activate(true);
            }
        }

        public bool UpdateStatusString()
        {
            bool have_eror = false;
            lblError.Text = lblDateError.Text = "";
            for (int i = 0; i < selectedTecViews.Count; i++)
            {
                if (selectedTecViews[i].actioned_state && !selectedTecViews[i].tec.connSett.ignore)
                {
                    if (selectedTecViews[i].isActive)
                    {
                        lblDateError.Text = selectedTecViews[i].last_time_action.ToString();
                        lblError.Text = selectedTecViews[i].last_action;
                    }
                }

                if (selectedTecViews[i].errored_state && !selectedTecViews[i].tec.connSett.ignore)
                {
                    have_eror = true;
                    if (selectedTecViews[i].isActive)
                    {
                        lblDateError.Text = selectedTecViews[i].last_time_error.ToString();
                        lblError.Text = selectedTecViews[i].last_error;
                    }
                }
            }

            if (adminPanel.actioned_state && adminPanel.isActive)
            {
                lblDateError.Text = adminPanel.last_time_action.ToString();
                lblError.Text = adminPanel.last_action;
            }

            if (adminPanel.errored_state)
            {
                have_eror = true;
                lblDateError.Text = adminPanel.last_time_error.ToString();
                lblError.Text = adminPanel.last_error;
            }

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
                adminPanel.StartDbInterface();

                // отображаем вкладки ТЭЦ
                int index;
                for (int i = 0; i < changeMode.tec_index.Count; i++)
                {
                    TEC t = tec[changeMode.tec_index[i]];

                    if ((index = changeMode.was_checked.IndexOf(i)) >= 0)
                    {
                        if (changeMode.gtp_index[changeMode.was_checked[index]] == -1)
                            tclTecViews.TabPages.Add(t.name);
                        else
                            tclTecViews.TabPages.Add(t.name + " - " + t.GTP[changeMode.gtp_index[changeMode.was_checked[index]]].name);
                        tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(tecViews[i]);
                        selectedTecViews.Add(tecViews[i]);

                        t.StartDbInterface();
                        tecViews[i].Activate(false);
                        tecViews[i].Start();
                    }
                }

                if (selectedTecViews.Count > 0)
                {
                    oldSelectedIndex = 0;
                    selectedTecViews[oldSelectedIndex].Activate(true);
                }

                if (changeMode.admin_was_checked)
                {
                    //if (passwordForm.ShowDialog() == DialogResult.Yes)
                    {
                        tclTecViews.TabPages.Add("Редактирование ПБР");

                        tclTecViews.TabPages[tclTecViews.TabPages.Count - 1].Controls.Add(adminPanel);

                        adminPanel.Start();
                    }
                }

                firstStart = false;
            }

            lock (lockEvent)
            {
                bool have_eror = UpdateStatusString();

                if (have_eror)
                    lblMainState.Text = "ОШИБКА";

                if (!have_eror || !show_error_alert)
                    lblMainState.Text = "";

                show_error_alert = !show_error_alert;
                lblError.Invalidate();
                lblDateError.Invalidate();
            }
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About a = new About();
            a.ShowDialog();
        }

        private void панельГрафическихToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (панельГрафическихToolStripMenuItem.Checked)
                graphicsSettingsForm.Show();
            else
                graphicsSettingsForm.Hide();
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
        }

        private void параметрыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            parametersForm.ShowDialog();
        }
    }
}
