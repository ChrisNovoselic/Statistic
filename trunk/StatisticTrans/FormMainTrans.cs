using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace StatisticCommon
{
    public abstract partial class FormMainTrans : FormMainBase
    {
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const Int32 TIMER_SERVICE_MIN_INTERVAL = 59666;
        private const Int32 TIMER_START_INTERVAL = 666;
        
        protected enum MODE_MASHINE : ushort { INTERACTIVE, AUTO, SERVICE, UNKNOWN };
        protected enum CONN_SETT_TYPE : short {SOURCE, DEST, COUNT_CONN_SETT_TYPE};
        protected enum INDX_UICONTROL_DB { SERVER_IP, PORT, NAME_DATABASE, USER_ID, PASS, COUNT_INDX_UICONTROL_DB };

        protected System.Windows.Forms.Control[,] m_arUIControlDB;

        protected System.Windows.Forms.Timer timerService;

        protected HAdmin[] m_arAdmin;
        protected FormConnectionSettings m_formConnectionSettings;
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
            if (!(comboBoxTECComponent.SelectedIndex < 0))
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
                            if (m_arUIControlDB[i, j].Enabled)
                                ((NumericUpDown)m_arUIControlDB[i, j]).Text = connSett.port.ToString();
                            else
                                ;
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

        protected virtual void CreateFormConnectionSettings(string connSettFileName)
        {
            m_formConnectionSettings = new FormConnectionSettings(connSettFileName);
        }

        protected virtual void FillComboBoxTECComponent () {
            if (!(comboBoxTECComponent.Items.Count == m_listTECComponentIndex.Count))
            {
                comboBoxTECComponent.Items.Clear();
                
                for (int i = 0; i < m_listTECComponentIndex.Count; i++)
                    comboBoxTECComponent.Items.Add(((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).allTECComponents[m_listTECComponentIndex[i]].tec.name + " - " + ((AdminTS)m_arAdmin[(Int16)CONN_SETT_TYPE.DEST]).allTECComponents[m_listTECComponentIndex[i]].name);

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
                    m_arAdmin[i].StopThreadSourceData();
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

                if (m_formConnectionSettings.Count > 1)
                    m_formConnectionSettings.SelectedIndex = m_IndexDB;
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
            m_formConnectionSettings.ShowDialog(this);

            //Эмуляция нажатия кнопки "Ок"
            /*
            m_formConnectionSettings.btnOk_Click(null, null);
            */

            DialogResult dlgRes = m_formConnectionSettings.DialogResult;
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

        protected /*virtual*/ void ErrorReport (string msg) {
            statusStripMain.BeginInvoke(delegateEvent);

            m_arAdmin[(int)CONN_SETT_TYPE.SOURCE].AbortRDGExcelValues();

            this.BeginInvoke(new DelegateBoolFunc(enabledButtonSourceExport), false);

            if ((m_bTransAuto == true || m_modeMashine == MODE_MASHINE.SERVICE) && (m_bEnabledUIControl == false))
                this.BeginInvoke(new DelegateFunc(trans_auto_next));
            else
                ;
        }

        protected void ActionReport(string msg)
        {
            statusStripMain.BeginInvoke(delegateEvent);
        }

        public bool UpdateStatusString()
        {
            bool have_eror = true;

            if ((!(m_arAdmin == null)) && (!(m_arAdmin[m_IndexDB] == null)))
            {
                have_eror = m_arAdmin[m_IndexDB].errored_state;

                if (((have_eror == true) || (m_arAdmin[m_IndexDB].actioned_state == true)) && (m_arAdmin[m_IndexDB].threadIsWorking == true))
                {
                    if (m_arAdmin[m_IndexDB].actioned_state == true)
                    {
                        lblDescError.Text = m_arAdmin[m_IndexDB].last_action;
                        lblDateError.Text = m_arAdmin[m_IndexDB].last_time_action.ToString();
                    }
                    else
                        ;

                    if (have_eror == true)
                    {
                        lblDescError.Text = m_arAdmin[m_IndexDB].last_error;
                        lblDateError.Text = m_arAdmin[m_IndexDB].last_time_error.ToString();
                    }
                    else
                        ;
                }
                else
                {
                    lblDescError.Text = string.Empty;
                    lblDateError.Text = string.Empty;
                }
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
}
