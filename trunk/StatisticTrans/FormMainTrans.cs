using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using StatisticCommon;

namespace StatisticTrans
{
    public abstract partial class FormMainTrans : FormMainBaseWithStatusStrip
    {
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const Int32 TIMER_SERVICE_MIN_INTERVAL = 66666;
        private const Int32 TIMER_START_INTERVAL = 666;

        protected enum MODE_MASHINE : ushort { INTERACTIVE, AUTO, SERVICE, UNKNOWN };
        protected enum CONN_SETT_TYPE : short {SOURCE, DEST, COUNT_CONN_SETT_TYPE};
        protected enum INDX_UICONTROL_DB { SERVER_IP, PORT, NAME_DATABASE, USER_ID, PASS, COUNT_INDX_UICONTROL_DB };

        protected System.Windows.Forms.Control[,] m_arUIControlDB;

        protected System.Windows.Forms.Timer timerService;

        protected HAdmin[] m_arAdmin;
        private FIleConnSett m_fileConnSett;
        protected FormConnectionSettings m_formConnectionSettingsConfigDB;
        protected GroupBox[] m_arGroupBox;

        protected DataGridViewAdmin m_dgwAdminTable;

        protected List<int> m_listTECComponentIndex;

        protected DateTime m_arg_date;
        protected Int32 m_arg_interval;

        protected FormChangeMode.MODE_TECCOMPONENT m_modeTECComponent;

        protected MODE_MASHINE m_modeMashine = MODE_MASHINE.INTERACTIVE;

        protected CheckBox m_checkboxModeMashine;

        protected bool m_bTransAuto {
            get
            {
                //return WindowState == FormWindowState.Minimized ? true : false;
                //return ((WindowState == FormWindowState.Minimized) && (ShowInTaskbar == false) && (notifyIconMain.Visible == true));

                //return !timerMain.Enabled;

                return m_modeMashine == MODE_MASHINE.AUTO ? true : false;
            }
        }
        protected bool m_bEnabledUIControl = true;

        protected Int16 m_IndexDB
        {
            get {
                CONN_SETT_TYPE i;
                for (i = CONN_SETT_TYPE.SOURCE; i < CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
                {
                    if (m_arGroupBox [(Int16)i].BackColor == SystemColors.Info)
                        break;
                    else
                        ;
                }

                return (Int16)i;
            }

            set {
            }
        }

        public FormMainTrans()
        {
            InitializeComponent();

            m_report = new HReports();

            // m_statusStripMain
            this.m_statusStripMain.Location = new System.Drawing.Point(0, 546);
            this.m_statusStripMain.Size = new System.Drawing.Size(841, 22);
            // m_lblMainState
            this.m_lblMainState.Size = new System.Drawing.Size(166, 17);
            // m_lblDateError
            this.m_lblDateError.Size = new System.Drawing.Size(166, 17);
            // m_lblDescError
            this.m_lblDescError.Size = new System.Drawing.Size(463, 17);

            //this.notifyIconMain.ContextMenuStrip = this.contextMenuStripNotifyIcon;
            notifyIconMain.Click += new EventHandler(notifyIconMain_Click);

            //this.Deactivate += new EventHandler(FormMainTrans_Deactivate);

            this.m_checkboxModeMashine = new CheckBox();
            this.m_checkboxModeMashine.Name = "m_checkboxModeMashine";
            this.m_checkboxModeMashine.Text = "Фоновый режим";
            this.m_checkboxModeMashine.Location = new System.Drawing.Point(13, 516);
            this.m_checkboxModeMashine.Size = new System.Drawing.Size(123, 23);
            //this.m_checkboxModeMashine.CheckAlign = ContentAlignment.;
            this.m_checkboxModeMashine.TextAlign = ContentAlignment.MiddleLeft;
            this.m_checkboxModeMashine.CheckedChanged +=new EventHandler(m_checkboxModeMashine_CheckedChanged);
            this.Controls.Add(this.m_checkboxModeMashine);
            //Пока переходить из режима в режимпользователь НЕ может (нестабильная работа trans_tg.exe) ???
            this.m_checkboxModeMashine.Enabled = false;;

            //Значения аргументов по умолчанию
            m_arg_date = DateTime.Now;
            m_arg_interval = TIMER_SERVICE_MIN_INTERVAL; //Милисекунды

            string msg_throw = string.Empty;
            string [] args = Environment.GetCommandLineArgs();
            int argc = args.Length;
            if (argc > 1)
            {
                if ((!(argc == 2)))
                    ;
                else
                {
                    if ((!(args[1].IndexOf("date") < 0)) && ((args[1][0] == '/') && (!(args[1].IndexOf("=") < 0))))
                    {
                        m_modeMashine = MODE_MASHINE.AUTO;

                        string date = args[1].Substring(args[1].IndexOf("=") + 1, args[1].Length - (args[1].IndexOf("=") + 1));
                        if (date == "default")
                            m_arg_date = DateTime.Now.AddDays(1);
                        else
                            if (date == "now")
                                ; //Уже присвоено значение
                            else
                                m_arg_date = DateTime.Parse(date);
                    }
                    else
                        if ((!(args[1].IndexOf("service") < 0)) && ((args[1][0] == '/') && (!(args[1].IndexOf("=") < 0))))
                        {
                            m_modeMashine = MODE_MASHINE.SERVICE;

                            string interval = args[1].Substring(args[1].IndexOf("=") + 1, args[1].Length - (args[1].IndexOf("=") + 1));
                            if (interval == "default")
                                ;
                            else
                                m_arg_interval = Int32.Parse(interval);

                            if (m_arg_interval < TIMER_SERVICE_MIN_INTERVAL)
                            {
                                msg_throw = "Интервал задан меньше необходимого значения";
                                m_modeMashine = MODE_MASHINE.UNKNOWN;
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                            msg_throw = "Ошибка распознавания аргументов командной строки";
                            m_modeMashine = MODE_MASHINE.UNKNOWN;
                        }
                }

                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                notifyIconMain.Visible = true;
                //развернутьToolStripMenuItem.Enabled = false;

                dateTimePickerMain.Value = m_arg_date.Date;
            }
            else
            {                
            }

            m_arGroupBox = new GroupBox[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE] { groupBoxSource, groupBoxDest };

            delegateEvent = new DelegateFunc(EventRaised);

            if (m_modeMashine == MODE_MASHINE.UNKNOWN)
                throw new Exception(msg_throw);
            else
                if (m_modeMashine == MODE_MASHINE.AUTO)
                    enabledUIControl(false);
                else
                    enabledUIControl(true);
        }

        protected abstract void Start();

        private void enabledUIControl (bool enabled) {
            for (int i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                //m_arGroupBox[i].Enabled = enabled;
                if (!(m_arGroupBox[i].Enabled == enabled)) m_arGroupBox[i].Enabled = enabled; else ;
            }

            if (!(dateTimePickerMain.Enabled == enabled)) dateTimePickerMain.Enabled = enabled; else ;
            if (!(comboBoxTECComponent.Enabled == enabled)) comboBoxTECComponent.Enabled = enabled; else ;
            //Пока переходить из режима в режимпользователь НЕ может (нестабильная работа trans_tg.exe) ???
            //if (!(m_checkboxModeMashine.Enabled == enabled)) m_checkboxModeMashine.Enabled = enabled; else ;

            if (enabled)
            {
                comboBoxTECComponent.SelectedIndexChanged += new EventHandler(comboBoxTECComponent_SelectedIndexChanged);
                dateTimePickerMain.ValueChanged += new EventHandler(dateTimePickerMain_Changed);
            }
            else
            {
                comboBoxTECComponent.SelectedIndexChanged -= comboBoxTECComponent_SelectedIndexChanged;
                dateTimePickerMain.ValueChanged -= dateTimePickerMain_Changed;
            }

            m_bEnabledUIControl = enabled;
        }

        protected void setUIControlConnectionSettings(int i)
        {
            if (!(comboBoxTECComponent.SelectedIndex < 0) &&
                (m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex] < ((AdminTS)m_arAdmin[i]).allTECComponents.Count))
            {
                ConnectionSettings connSett = ((AdminTS)m_arAdmin[i]).allTECComponents[m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex]].tec.connSetts[(int)StatisticCommon.CONN_SETT_TYPE.PBR];
                for (int j = 0; j < (Int16)INDX_UICONTROL_DB.COUNT_INDX_UICONTROL_DB; j++)
                {
                    switch (j)
                    {
                        case (Int16)FormMainTrans.INDX_UICONTROL_DB.SERVER_IP:
                            ((TextBox)m_arUIControlDB[i, j]).Text = connSett.server;
                            break;
                        case (Int16)INDX_UICONTROL_DB.PORT:
                            //if (m_arUIControlDB[i, j].Enabled == true)
                                ((NumericUpDown)m_arUIControlDB[i, j]).Text = connSett.port.ToString();
                            //else
                            //    ;
                            break;
                        case (Int16)INDX_UICONTROL_DB.NAME_DATABASE:
                            ((TextBox)m_arUIControlDB[i, j]).Text = connSett.dbName;
                            break;
                        case (Int16)INDX_UICONTROL_DB.USER_ID:
                            ((TextBox)m_arUIControlDB[i, j]).Text = connSett.userName;
                            break;
                        case (Int16)INDX_UICONTROL_DB.PASS:
                            ((MaskedTextBox)m_arUIControlDB[i, j]).Text = connSett.password;
                            break;
                        default:
                            break;
                    }
                }
            }
            else
                ;
        }

        protected virtual void setUIControlSourceState()
        {
        }

        protected void enabledButtonSourceExport(bool enabled)
        {
            buttonSourceExport.Enabled = enabled;
        }

        protected virtual void CreateFormConnectionSettingsConfigDB(string connSettFileName)
        {
            m_fileConnSett = new FIleConnSett(connSettFileName);
            m_formConnectionSettingsConfigDB = new FormConnectionSettings(-1, m_fileConnSett.ReadSettingsFile, m_fileConnSett.SaveSettingsFile);
        }

        protected virtual void FillComboBoxTECComponent () {
            if (!(comboBoxTECComponent.Items.Count == m_listTECComponentIndex.Count))
            {
                comboBoxTECComponent.Items.Clear();
                
                for (int i = 0; i < m_listTECComponentIndex.Count; i++)
                    comboBoxTECComponent.Items.Add(((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).allTECComponents[m_listTECComponentIndex[i]].tec.name_shr + " - " + ((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).allTECComponents[m_listTECComponentIndex[i]].name_shr);

                if (comboBoxTECComponent.Items.Count > 0)
                    comboBoxTECComponent.SelectedIndex = 0;
                else
                    ;
            }
            else
                ;
        }

        protected virtual void setDataGridViewAdmin(DateTime date)
        {
            //if (WindowState == FormWindowState.Minimized)
            //if (m_bTransAuto == true)
            //if (m_modeMashine == MODE_MASHINE.AUTO || m_modeMashine == MODE_MASHINE.SERVICE)
            if ((m_bTransAuto == true || m_modeMashine == MODE_MASHINE.SERVICE) && (m_bEnabledUIControl == false))
            {
                m_arAdmin[(int)CONN_SETT_TYPE.DEST].getCurRDGValues(m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]);
                //((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.DEST]).m_bSavePPBRValues = true;

                this.BeginInvoke(new DelegateBoolFunc(SaveRDGValues), false);

                //this.BeginInvoke(new DelegateFunc(trans_auto_next));
            }
            else
            {
                updateDataGridViewAdmin (date);
            }
        }

        protected abstract void updateDataGridViewAdmin (DateTime date);

        protected virtual void saveDataGridViewAdminComplete()
        {
            if ((m_bTransAuto == true || m_modeMashine == MODE_MASHINE.SERVICE) && (m_bEnabledUIControl == false))
            {
                this.BeginInvoke(new DelegateFunc(trans_auto_next));
                //trans_auto_next ();
            }
            else
                ;
        }

        protected void setDatetimePickerMain(DateTime date)
        {
            dateTimePickerMain.Value = date;
        }

        protected void setDatetimePicker(DateTime date)
        {
            this.BeginInvoke(new DelegateDateFunction(setDatetimePickerMain), date);
        }

        private void Stop()
        {
            ClearTables();

            comboBoxTECComponent.Items.Clear();

            for (int i = 0; (i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE) && (!(m_arAdmin == null)); i++)
            {
                if (!(m_arAdmin[i] == null)) 
                {
                    m_arAdmin[i].Stop();
                    m_arAdmin[i] = null;
                }
                else
                    ;
            }

            timerMain.Stop();
        }
        
        protected virtual void buttonClose_Click(object sender, EventArgs e)
        {
            Stop();

            Close ();
        }

        private void groupBoxFocus (GroupBox groupBox) {
            GroupBox groupBoxOther = null;
            bool bBackColorChange = false;
            if (! (groupBox.BackColor == SystemColors.Info)) {
                groupBox.BackColor = SystemColors.Info;

                UpdateStatusString ();

                bBackColorChange = true;
            }
            else
                ;

            switch (groupBox.Name) {
                case "groupBoxSource":
                    groupBoxOther = groupBoxDest;
                    break;
                case "groupBoxDest":
                    groupBoxOther = groupBoxSource;
                    break;
                default:
                    break;
            }

            if (bBackColorChange) {
                groupBoxOther.BackColor = SystemColors.Control;

                if (m_formConnectionSettingsConfigDB.Count > 1)
                    m_formConnectionSettingsConfigDB.SelectedIndex = m_IndexDB;
                else
                    ;

                comboBoxTECComponent_SelectedIndexChanged(null, EventArgs.Empty);
            }
            else
                ;
        }

        private void groupBox_MouseClick(object sender, MouseEventArgs e)
        {
            groupBoxFocus(((GroupBox)sender));
        }

        private void groupBox_Enter(object sender, EventArgs e)
        {
            groupBoxFocus(((GroupBox)sender));
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonClose_Click (null, null);
        }

        private void конфигурацияБДToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //m_formConnectionSettings.StartPosition = FormStartPosition.CenterParent;
            m_formConnectionSettingsConfigDB.ShowDialog(this);

            //Эмуляция нажатия кнопки "Ок"
            /*
            m_formConnectionSettings.btnOk_Click(null, null);
            */

            DialogResult dlgRes = m_formConnectionSettingsConfigDB.DialogResult;
            if (dlgRes == System.Windows.Forms.DialogResult.Yes)
            {
                Stop();

                Start();
            }
            else
                ;
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout formAbout = new FormAbout ();
            formAbout.ShowDialog(this);
        }

        protected override void ErrorReport (string msg) {
            m_statusStripMain.BeginInvoke(delegateEvent);

            m_arAdmin[(int)CONN_SETT_TYPE.SOURCE].AbortRDGExcelValues();

            this.BeginInvoke(new DelegateBoolFunc(enabledButtonSourceExport), false);

            if ((m_bTransAuto == true || m_modeMashine == MODE_MASHINE.SERVICE) && (m_bEnabledUIControl == false))
                this.BeginInvoke(new DelegateFunc(trans_auto_next));
            else
                ;
        }

        protected override void ActionReport(string msg)
        {
            m_statusStripMain.BeginInvoke(delegateEvent);

            this.BeginInvoke(new DelegateBoolFunc(enabledButtonSourceExport), true);
        }

        protected override bool UpdateStatusString()
        {
            bool have_eror = true;

            if ((!(m_arAdmin == null)) && (!(m_arAdmin[m_IndexDB] == null)))
            {
                have_eror = m_report.errored_state;

                if (((have_eror == true) || (m_report.actioned_state == true)) && (!(m_arAdmin[m_IndexDB].threadIsWorking < 0)))
                {
                    if (m_report.actioned_state == true)
                    {
                        m_lblDescError.Text = m_report.last_action;
                        m_lblDateError.Text = m_report.last_time_action.ToString();
                    }
                    else
                        ;

                    if (have_eror == true)
                    {
                        m_lblDescError.Text = m_report.last_error;
                        m_lblDateError.Text = m_report.last_time_error.ToString();
                    }
                    else
                        ;
                }
                else
                {
                    m_lblDescError.Text = string.Empty;
                    m_lblDateError.Text = string.Empty;
                }
            }
            else
                ;

            return have_eror;
        }

        private void trans_auto_start()
        {
            ////Таймер больше не нужен (сообщения в "строке статуса")
            //timerMain.Stop();
            //timerMain.Interval = TIMER_START_INTERVAL;
            ////timerMain.Enabled = false;

            if (!(comboBoxTECComponent.SelectedIndex < 0))
            {
                comboBoxTECComponent.SelectedIndex = -1;

                trans_auto_next();
            }
            else
                if (m_bTransAuto) buttonClose.PerformClick(); else enabledUIControl(true);
        }

        protected void trans_auto_next () {
            if (comboBoxTECComponent.SelectedIndex + 1 < comboBoxTECComponent.Items.Count)
            {
                comboBoxTECComponent.SelectedIndex ++;
                //Обработчик отключен - вызов "программно"
                comboBoxTECComponent_SelectedIndexChanged(null, EventArgs.Empty);
            }
            else
                if (m_bTransAuto)
                    buttonClose.PerformClick();
                else
                {

                    if (IsTomorrow () == false) {
                        dateTimePickerMain.Value = DateTime.Now;

                        //enabledUIControl(true);
                    }
                    else
                    {
                        dateTimePickerMain.Value = dateTimePickerMain.Value.AddDays(1);
                        comboBoxTECComponent.SelectedIndex = 0;

                        comboBoxTECComponent_SelectedIndexChanged(null, EventArgs.Empty);
                    }
                }
        }

        protected virtual bool IsTomorrow() {
            TimeSpan timeSpan= new TimeSpan (4, 5, 6);
            DateTime dateApp = dateTimePickerMain.Value.AddDays (1);

            return (((DateTime)dateApp.Date) - DateTime.Now) > timeSpan ? false : true;
        }

        private void timerMain_Tick(object sender, EventArgs e)
        {
            if (timerMain.Interval == TIMER_START_INTERVAL)
            {
                //Первый запуск
                timerMain.Interval = 1000;

                m_listTECComponentIndex = ((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).GetListIndexTECComponent(m_modeTECComponent);

                if (m_modeMashine == MODE_MASHINE.AUTO)
                {
                    FillComboBoxTECComponent();

                    trans_auto_start();

                    //return;
                }
                else
                    if (m_modeMashine == MODE_MASHINE.SERVICE)
                        m_checkboxModeMashine.Checked = true;
                    else
                        FillComboBoxTECComponent();
            }
            else
                ;

            lock (lockEvent)
            {
                bool have_eror = UpdateStatusString();

                if (have_eror)
                    m_lblMainState.Text = "ОШИБКА";
                else
                    ;

                if (!have_eror || !show_error_alert)
                    m_lblMainState.Text = "";
                else
                    ;

                show_error_alert = !show_error_alert;
                m_lblDescError.Invalidate();
                m_lblDateError.Invalidate();
            }
        }

        private void timerService_Tick(object sender, EventArgs e)
        {
            enabledUIControl(false);

            if (timerService.Interval == TIMER_START_INTERVAL)
            {
                //Первый запуск
                if (m_arg_interval == timerService.Interval) m_arg_interval++; else ; //случайное совпадение
                timerService.Interval = m_arg_interval;

                FillComboBoxTECComponent();
            }
            else
                ;

            dateTimePickerMain.Value = DateTime.Now;

            trans_auto_start();
        }

        //private void FormMain_Activated(object sender, EventArgs e)
        //{
        //    m_arAdmin[m_IndexDB].GetCurrentTime ();
        //}

        protected virtual void buttonClear_Click(object sender, EventArgs e)
        {
            //m_IndexDB = только DEST
            ((AdminTS)m_arAdmin [m_IndexDB]).ClearRDGValues(dateTimePickerMain.Value.Date);
        }

        protected /*virtual*/ void buttonDestSave_Click(object sender, EventArgs e)
        {
            
        }

        protected /*virtual*/ void buttonSourceSave_Click(object sender, EventArgs e)
        {

        }

        protected virtual void component_Changed(object sender, EventArgs e)
        {
            //Не передавать значения в форму с параметрами соединения с БД конфигурации
            //Раньше эти настройки изменялись на самой форме...
            /*
            uint indxDB = (uint)m_IndexDB;
            ConnectionSettings connSett = new ConnectionSettings();

            connSett.server = m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.SERVER_IP].Text;
            connSett.port = (Int32)((NumericUpDown)m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.PORT]).Value;
            connSett.dbName = m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.NAME_DATABASE].Text;
            connSett.userName = m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.USER_ID].Text;
            connSett.password = m_arUIControlDB[indxDB, (Int16)INDX_UICONTROL_DB.PASS].Text;
            connSett.ignore = false;

            m_formConnectionSettings.ConnectionSettingsEdit = connSett;
            */
        }

        protected bool IsCanSelectedIndexChanged()
        {
            bool bRes = false;
            
            if ((!(m_arAdmin == null)) && (!(m_arAdmin[m_IndexDB] == null)) && (!(m_listTECComponentIndex == null)) &&
                (m_listTECComponentIndex.Count > 0) && (!(comboBoxTECComponent.SelectedIndex < 0)))
                bRes = true;
            else
                ;

            return bRes;
        }
        
        protected abstract void comboBoxTECComponent_SelectedIndexChanged (object cbx, EventArgs ev);

        private void dateTimePickerMain_Changed(object sender, EventArgs e)
        {
            comboBoxTECComponent_SelectedIndexChanged(null, EventArgs.Empty);
        }

        public void ClearTables()
        {
            m_dgwAdminTable.ClearTables ();
        }

        protected abstract void getDataGridViewAdmin(int indxDB);

        private void buttonSourceExport_Click(object sender, EventArgs e)
        {
            if (!(comboBoxTECComponent.SelectedIndex < 0)) {
                //Взять значения "с окна" в таблицу
                getDataGridViewAdmin((int)(Int16)CONN_SETT_TYPE.DEST);

                //ClearTables();

                SaveRDGValues (true);
            }
            else
                ;
        }

        private void m_checkboxModeMashine_CheckedChanged(object sender, EventArgs e)
        {
            if (!(m_modeMashine == MODE_MASHINE.AUTO))
                if (m_checkboxModeMashine.Checked == true)
                {
                    //if (m_modeMashine == MODE_MASHINE.INTERACTIVE) m_modeMashine = MODE_MASHINE.SERVICE; else ;
                    //То же самое
                    if (!(m_modeMashine == MODE_MASHINE.SERVICE)) m_modeMashine = MODE_MASHINE.SERVICE; else ;

                    m_dgwAdminTable.Enabled = false;

                    InitializeTimerService ();
                    
                    SendMessage(this.Handle, 0x112, 0xF020, 0);
                    timerService.Start();
                }
                else
                {
                    if (!(m_modeMashine == MODE_MASHINE.INTERACTIVE)) m_modeMashine = MODE_MASHINE.INTERACTIVE; else ;
                    
                    timerService.Stop();
                    //timerService.Interval = TIMER_START_INTERVAL;

                    timerService = null;
                }
            else
                ;
        }

        private void InitializeTimerService () {
            if (timerService == null) {
                timerService = new Timer(this.components);
                timerService.Interval = TIMER_START_INTERVAL; //Пеавый запуск
                timerService.Tick += new System.EventHandler(this.timerService_Tick);
            }
            else
                ;
        }

        protected virtual void SaveRDGValues (bool bCallback) {
            ((AdminTS)m_arAdmin[(int)(Int16)CONN_SETT_TYPE.DEST]).SaveRDGValues(m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value, bCallback);
        }

        private void notifyIconMain_Click(object sender, EventArgs e)
        {
            развернутьToolStripMenuItem.PerformClick();
        }

        private void развернутьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (notifyIconMain.Visible == true)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                notifyIconMain.Visible = false;
            }
            else
                ;
        }

        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonClose.PerformClick();
        }

        private void FormMainTrans_Deactivate(object sender, EventArgs e)
        {
        }

        // Перехват нажатия на кнопку свернуть
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x112)
            {
                if (m.WParam.ToInt32() == 0xF020)
                {
                    this.WindowState = FormWindowState.Minimized;
                    this.ShowInTaskbar = false;
                    notifyIconMain.Visible = true;

                    return;
                }
            }
            else
                ;

            base.WndProc(ref m);
        }
    }

    public partial class FormMainTransDB : FormMainTrans
    {
        private System.Windows.Forms.Label labelSourcePort;
        private System.Windows.Forms.NumericUpDown nudnSourcePort;
        private System.Windows.Forms.Label labelSourcePass;
        private System.Windows.Forms.Label labelSourceUserId;
        private System.Windows.Forms.Label labelSourceNameDatabase;
        private System.Windows.Forms.Label labelSourceServerIP;
        private System.Windows.Forms.MaskedTextBox mtbxSourcePass;
        private System.Windows.Forms.TextBox tbxSourceUserId;
        private System.Windows.Forms.TextBox tbxSourceNameDatabase;
        private System.Windows.Forms.TextBox tbxSourceServerIP;

        public FormMainTransDB()
        {
            InitializeComponentTransGTP();

            this.Text = "Конвертер данных плана и административных данных (ГТП)";

            this.m_dgwAdminTable = new StatisticCommon.DataGridViewAdminKomDisp();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).BeginInit();
            this.SuspendLayout();
            // 
            // m_dgwAdminTable
            // 
            this.m_dgwAdminTable.Location = new System.Drawing.Point(319, 5);
            this.m_dgwAdminTable.Name = "m_dgwAdminTable";
            this.m_dgwAdminTable.RowHeadersVisible = false;
            this.m_dgwAdminTable.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.m_dgwAdminTable.Size = new System.Drawing.Size(498, 471);
            this.m_dgwAdminTable.TabIndex = 27;
            this.panelMain.Controls.Add(this.m_dgwAdminTable);
            ((System.ComponentModel.ISupportInitialize)(this.m_dgwAdminTable)).EndInit();
            this.ResumeLayout(false);

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMainTrans));
            this.notifyIconMain.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic4"))); //$this.Icon
            this.notifyIconMain.Text = "Статистика: конвертер (ГТП)";
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("statistic4"))); //$this.Icon

            m_modeTECComponent = FormChangeMode.MODE_TECCOMPONENT.GTP;

            m_arUIControlDB = new System.Windows.Forms.Control[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE, (Int16)INDX_UICONTROL_DB.COUNT_INDX_UICONTROL_DB]
            { { tbxSourceServerIP, nudnSourcePort, tbxSourceNameDatabase, tbxSourceUserId, mtbxSourcePass },
            { tbxDestServerIP, nudnDestPort, tbxDestNameDatabase, tbxDestUserId, mtbxDestPass} };

            //Созжание массива для объектов получения данных
            m_arAdmin = new AdminTS[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];

            Start();
        }

        private void InitializeComponentTransGTP()
        {
            this.labelSourcePort = new System.Windows.Forms.Label();
            this.nudnSourcePort = new System.Windows.Forms.NumericUpDown();
            this.labelSourcePass = new System.Windows.Forms.Label();
            this.labelSourceUserId = new System.Windows.Forms.Label();
            this.labelSourceNameDatabase = new System.Windows.Forms.Label();
            this.labelSourceServerIP = new System.Windows.Forms.Label();
            this.mtbxSourcePass = new System.Windows.Forms.MaskedTextBox();
            this.tbxSourceUserId = new System.Windows.Forms.TextBox();
            this.tbxSourceNameDatabase = new System.Windows.Forms.TextBox();
            this.tbxSourceServerIP = new System.Windows.Forms.TextBox();

            ((System.ComponentModel.ISupportInitialize)(this.nudnSourcePort)).BeginInit();

            base.groupBoxSource.Controls.Add(this.labelSourcePort);
            this.groupBoxSource.Controls.Add(this.nudnSourcePort);
            this.groupBoxSource.Controls.Add(this.labelSourcePass);
            this.groupBoxSource.Controls.Add(this.labelSourceUserId);
            this.groupBoxSource.Controls.Add(this.labelSourceNameDatabase);
            this.groupBoxSource.Controls.Add(this.labelSourceServerIP);
            this.groupBoxSource.Controls.Add(this.mtbxSourcePass);
            this.groupBoxSource.Controls.Add(this.tbxSourceUserId);
            this.groupBoxSource.Controls.Add(this.tbxSourceNameDatabase);
            this.groupBoxSource.Controls.Add(this.tbxSourceServerIP);
            // 
            // labelSourcePort
            // 
            this.labelSourcePort.AutoSize = true;
            this.labelSourcePort.Location = new System.Drawing.Point(12, 55);
            this.labelSourcePort.Name = "labelSourcePort";
            this.labelSourcePort.Size = new System.Drawing.Size(32, 13);
            this.labelSourcePort.TabIndex = 21;
            this.labelSourcePort.Text = "Порт";
            // 
            // nudnSourcePort
            // 
            this.nudnSourcePort.Enabled = false;
            this.nudnSourcePort.Location = new System.Drawing.Point(129, 53);
            this.nudnSourcePort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudnSourcePort.Name = "nudnSourcePort";
            this.nudnSourcePort.Size = new System.Drawing.Size(69, 20);
            this.nudnSourcePort.TabIndex = 16;
            this.nudnSourcePort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudnSourcePort.Value = new decimal(new int[] {
            3306,
            0,
            0,
            0});
            // 
            // labelSourcePass
            // 
            this.labelSourcePass.AutoSize = true;
            this.labelSourcePass.Location = new System.Drawing.Point(11, 134);
            this.labelSourcePass.Name = "labelSourcePass";
            this.labelSourcePass.Size = new System.Drawing.Size(45, 13);
            this.labelSourcePass.TabIndex = 24;
            this.labelSourcePass.Text = "Пароль";
            // 
            // labelSourceUserId
            // 
            this.labelSourceUserId.AutoSize = true;
            this.labelSourceUserId.Location = new System.Drawing.Point(11, 108);
            this.labelSourceUserId.Name = "labelSourceUserId";
            this.labelSourceUserId.Size = new System.Drawing.Size(103, 13);
            this.labelSourceUserId.TabIndex = 23;
            this.labelSourceUserId.Text = "Имя пользователя";
            // 
            // labelSourceNameDatabase
            // 
            this.labelSourceNameDatabase.AutoSize = true;
            this.labelSourceNameDatabase.Location = new System.Drawing.Point(11, 82);
            this.labelSourceNameDatabase.Name = "labelSourceNameDatabase";
            this.labelSourceNameDatabase.Size = new System.Drawing.Size(98, 13);
            this.labelSourceNameDatabase.TabIndex = 22;
            this.labelSourceNameDatabase.Text = "Имя базы данных";
            // 
            // labelSourceServerIP
            // 
            this.labelSourceServerIP.AutoSize = true;
            this.labelSourceServerIP.Location = new System.Drawing.Point(11, 28);
            this.labelSourceServerIP.Name = "labelSourceServerIP";
            this.labelSourceServerIP.Size = new System.Drawing.Size(95, 13);
            this.labelSourceServerIP.TabIndex = 20;
            this.labelSourceServerIP.Text = "IP адрес сервера";
            // 
            // mtbxSourcePass
            //
            this.mtbxSourcePass.Enabled = false;
            this.mtbxSourcePass.Location = new System.Drawing.Point(129, 131);
            this.mtbxSourcePass.Name = "mtbxSourcePass";
            this.mtbxSourcePass.PasswordChar = '#';
            this.mtbxSourcePass.Size = new System.Drawing.Size(160, 20);
            this.mtbxSourcePass.TabIndex = 19;
            this.mtbxSourcePass.TextChanged += new System.EventHandler(component_Changed);
            // 
            // tbxSourceUserId
            // 
            this.tbxSourceUserId.Enabled = false;
            this.tbxSourceUserId.Location = new System.Drawing.Point(129, 105);
            this.tbxSourceUserId.Name = "tbxSourceUserId";
            this.tbxSourceUserId.Size = new System.Drawing.Size(160, 20);
            this.tbxSourceUserId.TabIndex = 18;
            this.tbxSourceUserId.TextChanged += new System.EventHandler(base.component_Changed);
            // 
            // tbxSourceNameDatabase
            // 
            this.tbxSourceNameDatabase.Enabled = false;
            this.tbxSourceNameDatabase.Location = new System.Drawing.Point(129, 79);
            this.tbxSourceNameDatabase.Name = "tbxSourceNameDatabase";
            this.tbxSourceNameDatabase.Size = new System.Drawing.Size(160, 20);
            this.tbxSourceNameDatabase.TabIndex = 17;
            this.tbxSourceNameDatabase.TextChanged += new System.EventHandler(this.component_Changed);
            // 
            // tbxSourceServerIP
            // 
            this.tbxSourceServerIP.Enabled = false;
            this.tbxSourceServerIP.Location = new System.Drawing.Point(129, 25);
            this.tbxSourceServerIP.Name = "tbxSourceServerIP";
            this.tbxSourceServerIP.Size = new System.Drawing.Size(160, 20);
            this.tbxSourceServerIP.TabIndex = 15;
            this.tbxSourceServerIP.TextChanged += new System.EventHandler(this.component_Changed);

            this.groupBoxSource.ResumeLayout(false);
            this.groupBoxSource.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudnSourcePort)).EndInit();
        }

        protected override void Start()
        {
            int i = -1;

            int[] arConfigDB = new int[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];
            string[] arKeyTypeConfigDB = new string[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE] { @"ТипБДКфгИсточник", @"ТипБДКфгНазначение" };
            FileINI fileINI = new FileINI(@"setup.ini");
            string sec = "Main (" + ProgramBase.AppName + ")";

            InitTECBase.TYPE_DATABASE_CFG[] arTypeConfigDB = new InitTECBase.TYPE_DATABASE_CFG[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE] { InitTECBase.TYPE_DATABASE_CFG.UNKNOWN, InitTECBase.TYPE_DATABASE_CFG.UNKNOWN };
            for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                arConfigDB[i] = fileINI.ReadInt(sec, arKeyTypeConfigDB[i], -1);
                for (InitTECBase.TYPE_DATABASE_CFG t = InitTECBase.TYPE_DATABASE_CFG.CFG_190; t < InitTECBase.TYPE_DATABASE_CFG.UNKNOWN; t++)
                {
                    if (t.ToString().Contains(arConfigDB[i].ToString()) == true)
                    {
                        arTypeConfigDB[i] = t;
                        break;
                    }
                    else
                        ;
                }
            }

            List<int> listID_TECNotUse = new List<int>();
            string[] arStrTypeField = new string[(Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE];
            arStrTypeField[(int)CONN_SETT_TYPE.SOURCE] = fileINI.ReadString(sec, @"РДГФорматТаблицаИсточник", string.Empty);
            arStrTypeField[(int)CONN_SETT_TYPE.DEST] = fileINI.ReadString(sec, @"РДГФорматТаблицаНазначение", string.Empty);

            string[] arStrID_TECNotUse = fileINI.ReadString(sec, @"ID_TECNotUse", string.Empty).Split(',');
            foreach (string str in arStrID_TECNotUse)
            {
                if (str.Equals (string.Empty) == false)
                    listID_TECNotUse.Add(Int32.Parse(str));
                else
                    ;
            }

            bool bIgnoreDateTime = false;
            if (Boolean.TryParse(fileINI.ReadString(sec, @"ИгнорДатаВремя-techsite", string.Empty), out bIgnoreDateTime) == false)
                bIgnoreDateTime = false;
            else
                ;

            int idListener = -1;
            //Инициализация объектов получения данных
            for (i = 0; i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE; i++)
            {
                m_arAdmin[i] = new AdminTS_KomDisp(m_report, new bool[] { false, true });
                idListener = DbSources.Sources().Register(m_formConnectionSettingsConfigDB.getConnSett(i), false, @"CONFIG_DB");
                try
                {
                    //((AdminTS_KomDisp)m_arAdmin[i]).InitTEC(m_formConnectionSettingsConfigDB.getConnSett((Int16)CONN_SETT_TYPE.DEST), m_modeTECComponent, true, false);
                    ((AdminTS_KomDisp)m_arAdmin[i]).InitTEC(idListener, m_modeTECComponent, arTypeConfigDB[i], true);
                    foreach (int id in listID_TECNotUse)
                    {
                        ((AdminTS_KomDisp)m_arAdmin[i]).RemoveTEC(id);
                    }
                }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, "FormMainTransGTP::FormMainTransGTP ()");
                    //ErrorReport("Ошибка соединения. Перехож в ожидание.");
                    //setUIControlConnectionSettings(i);
                    break;
                }

                //((AdminTS)m_arAdmin[i]).connSettConfigDB = m_formConnectionSettings.getConnSett(i);

                for (AdminTS.TYPE_FIELDS tf = AdminTS.TYPE_FIELDS.STATIC; i < (int)AdminTS.TYPE_FIELDS.COUNT_TYPE_FIELDS; tf++)
                    if (arStrTypeField[i].Equals(tf.ToString()) == true)
                    {
                        ((AdminTS)m_arAdmin[i]).m_typeFields = tf;
                        break;
                    }
                    else
                        ;

                m_arAdmin[i].m_ignore_date = bIgnoreDateTime;
                //m_arAdmin[i].m_ignore_connsett_data = true; //-> в конструктор

                setUIControlConnectionSettings(i);

                m_arAdmin[i].SetDelegateWait(delegateStartWait, delegateStopWait, delegateEvent);
                m_arAdmin[i].SetDelegateReport(ErrorReport, ActionReport);

                m_arAdmin[i].SetDelegateData(setDataGridViewAdmin);
                m_arAdmin[i].SetDelegateSaveComplete(saveDataGridViewAdminComplete);

                m_arAdmin[i].SetDelegateDatetime(setDatetimePicker);

                //m_arAdmin [i].mode (FormChangeMode.MODE_TECCOMPONENT.GTP);

                m_arAdmin[i].Start();

                DbSources.Sources().UnRegister(idListener);
            }

            if (!(i < (Int16)CONN_SETT_TYPE.COUNT_CONN_SETT_TYPE))
            {
                timerMain.Interval = 666; //Признак первой итерации
                timerMain.Start();
            }
            else
                ;
        }

        protected override void getDataGridViewAdmin(int indxDB)
        {
            //int indxDB = m_IndexDB;

            double value;
            bool valid;

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < (int)DataGridViewAdminKomDisp.DESC_INDEX.TO_ALL; j++)
                {
                    switch (j)
                    {
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN: // План
                            valid = double.TryParse((string)m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].Value, out value);
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr = value;
                            //((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr = ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curRDGValues[i].pbr;
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pmin = ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curRDGValues[i].pmin;
                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pmax = ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curRDGValues[i].pmax;

                            ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr_number = ((AdminTS)m_arAdmin[(int)CONN_SETT_TYPE.SOURCE]).m_curRDGValues[i].pbr_number;
                            break;
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION: // Рекомендация
                            {
                                //cellValidated(e.RowIndex, (int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION);

                                valid = double.TryParse((string)m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value, out value);
                                ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].recomendation = value;

                                break;
                            }
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE:
                            {
                                ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].deviationPercent = bool.Parse(this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE].Value.ToString());
                                break;
                            }
                        case (int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION: // Максимальное отклонение
                            {
                                valid = double.TryParse((string)this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION].Value, out value);
                                ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].deviation = value;

                                break;
                            }
                        default:
                            break;
                    }
                }
            }

            m_arAdmin[indxDB].CopyCurToPrevRDGValues();
        }

        protected override void updateDataGridViewAdmin(DateTime date)
        {
            int indxDB = m_IndexDB;

            for (int i = 0; i < 24; i++)
            {
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DATE_HOUR].Value = date.AddHours(i + 1).ToString("yyyy-MM-dd HH");
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.PLAN].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].pbr.ToString("F2");
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.RECOMENDATION].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].recomendation.ToString("F2");
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION_TYPE].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].deviationPercent.ToString();
                this.m_dgwAdminTable.Rows[i].Cells[(int)DataGridViewAdminKomDisp.DESC_INDEX.DEVIATION].Value = ((AdminTS)m_arAdmin[indxDB]).m_curRDGValues[i].deviation.ToString("F2");
            }

            //m_arAdmin[indxDB].CopyCurToPrevRDGValues ();

            //this.m_dgwAdminTable.Invalidate();
        }

        protected override void comboBoxTECComponent_SelectedIndexChanged(object cbx, EventArgs ev)
        {
            if (IsCanSelectedIndexChanged() == true)
            {
                ClearTables();

                short indexDB = m_IndexDB;

                switch (m_modeTECComponent)
                {
                    case FormChangeMode.MODE_TECCOMPONENT.GTP:
                        ((AdminTS)m_arAdmin[indexDB]).GetRDGValues((int)((AdminTS)m_arAdmin[indexDB]).m_typeFields, m_listTECComponentIndex[comboBoxTECComponent.SelectedIndex], dateTimePickerMain.Value.Date);
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.TG:
                        break;
                    case FormChangeMode.MODE_TECCOMPONENT.TEC:
                        break;
                    default:
                        break;
                }

                setUIControlConnectionSettings((int)CONN_SETT_TYPE.SOURCE);
                setUIControlConnectionSettings((int)CONN_SETT_TYPE.DEST);
            }
            else
                ;
        }

        protected override void CreateFormConnectionSettingsConfigDB(string connSettFileName)
        {
            base.CreateFormConnectionSettingsConfigDB(connSettFileName);

            if ((!(m_formConnectionSettingsConfigDB.Ready == 0)) || (m_formConnectionSettingsConfigDB.Count < 2))
            {
                while (m_formConnectionSettingsConfigDB.Count < m_arAdmin.Length)
                    m_formConnectionSettingsConfigDB.addConnSett(new ConnectionSettings());
            }
            else
                ;
        }
    }
}
